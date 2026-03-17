from __future__ import annotations

from collections import defaultdict, deque
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass, field
from datetime import datetime, timedelta
from itertools import cycle
from threading import RLock
from time import perf_counter
from typing import Any, Callable

from .agents import AgentRunner
from .errors import (
    AgentNotFoundError,
    AgentRegistrationError,
    InvalidTaskStateError,
    NoCompatibleAgentError,
    TaskNotFoundError,
)
from .metrics import MetricsCollector, MetricsSnapshot
from .models import AuditEvent, Task, TaskResult, TaskState, utc_now
from .policy import PolicyRule, RequiredTitlePolicy


_VALID_TRANSITIONS: dict[TaskState, set[TaskState]] = {
    TaskState.QUEUED: {TaskState.RUNNING, TaskState.DONE, TaskState.BLOCKED, TaskState.FAILED, TaskState.SKIPPED},
    TaskState.RUNNING: {TaskState.AWAITING_REVIEW, TaskState.DONE, TaskState.FAILED},
    TaskState.AWAITING_REVIEW: {TaskState.RUNNING, TaskState.DONE, TaskState.FAILED, TaskState.SKIPPED},
    TaskState.DONE: set(),
    TaskState.FAILED: set(),
    TaskState.BLOCKED: {TaskState.QUEUED, TaskState.SKIPPED},
    TaskState.SKIPPED: set(),
}


@dataclass
class InMemoryOrchestrator:
    agents: dict[str, AgentRunner] = field(default_factory=dict)
    tasks: dict[str, Task] = field(default_factory=dict)
    history: list[TaskResult] = field(default_factory=list)
    audit_log: list[AuditEvent] = field(default_factory=list)
    policies: list[PolicyRule] = field(default_factory=lambda: [RequiredTitlePolicy()])
    event_hooks: list[Callable[[AuditEvent], None]] = field(default_factory=list)
    metrics: MetricsCollector = field(default_factory=MetricsCollector)
    max_queue_size: int = 10_000

    _agent_cursor: Any = None
    _assignment_count: dict[str, int] = field(default_factory=lambda: defaultdict(int))
    _agent_duration_ms: dict[str, list[float]] = field(default_factory=lambda: defaultdict(list))
    _rate_window: dict[str, deque[datetime]] = field(default_factory=lambda: defaultdict(deque))
    _circuit_state: dict[str, tuple[int, datetime | None]] = field(default_factory=dict)
    _result_cache: dict[tuple[str, str, str | None], TaskResult] = field(default_factory=dict)
    _idempotency_index: dict[str, str] = field(default_factory=dict)
    _lock: RLock = field(default_factory=RLock)

    def _record(self, event: str, **details: Any) -> None:
        entry = AuditEvent(at=utc_now(), event=event, details=details)
        self.audit_log.append(entry)
        for hook in self.event_hooks:
            hook(entry)

    def add_event_hook(self, hook: Callable[[AuditEvent], None]) -> None:
        self.event_hooks.append(hook)

    def add_policy(self, policy: PolicyRule) -> None:
        self.policies.append(policy)

    def register_agent(self, agent: AgentRunner) -> None:
        if not agent.spec.name:
            raise AgentRegistrationError("Agent name must not be empty")
        if agent.spec.name in self.agents:
            raise AgentRegistrationError(f"Agent '{agent.spec.name}' already registered")

        self.agents[agent.spec.name] = agent
        self._agent_cursor = cycle(self.agents.keys())
        self._circuit_state[agent.spec.name] = (0, None)
        self._record("agent.registered", agent=agent.spec.name, capabilities=sorted(agent.spec.capabilities))

    def disable_agent(self, name: str) -> None:
        if name not in self.agents:
            raise AgentNotFoundError(name)
        self.agents[name].spec.enabled = False
        self._record("agent.disabled", agent=name)

    def enable_agent(self, name: str) -> None:
        if name not in self.agents:
            raise AgentNotFoundError(name)
        self.agents[name].spec.enabled = True
        self._record("agent.enabled", agent=name)

    def create_task(self, task: Task) -> Task:
        with self._lock:
            if len(self.tasks) >= self.max_queue_size:
                raise InvalidTaskStateError(f"Queue limit exceeded ({self.max_queue_size})")

            if task.idempotency_key and task.idempotency_key in self._idempotency_index:
                existing_id = self._idempotency_index[task.idempotency_key]
                return self.tasks[existing_id]

            self.tasks[task.id] = task
            if task.idempotency_key:
                self._idempotency_index[task.idempotency_key] = task.id
            self.metrics.created_tasks += 1
            self._record("task.created", task_id=task.id, capability=task.required_capability, priority=task.priority)
            return task

    def create_tasks(self, tasks: list[Task]) -> list[Task]:
        return [self.create_task(t) for t in tasks]

    def _set_task_state(self, task: Task, new_state: TaskState) -> None:
        if new_state not in _VALID_TRANSITIONS[task.state]:
            raise InvalidTaskStateError(f"Invalid transition: {task.state} -> {new_state}")
        previous = task.state
        task.state = new_state
        task.updated_at = utc_now()
        self._record("task.state_changed", task_id=task.id, old=previous.value, new=new_state.value)

    def _get_task(self, task_id: str) -> Task:
        try:
            return self.tasks[task_id]
        except KeyError as exc:
            raise TaskNotFoundError(f"Unknown task '{task_id}'") from exc

    def _dependencies_done(self, task: Task) -> bool:
        for dep_id in task.dependencies:
            dep = self._get_task(dep_id)
            if dep.state != TaskState.DONE:
                return False
        return True

    def get_ready_tasks(self) -> list[Task]:
        ready = [
            t
            for t in self.tasks.values()
            if t.state in {TaskState.QUEUED, TaskState.AWAITING_REVIEW} and self._dependencies_done(t)
        ]
        return sorted(ready, key=lambda t: (t.priority, t.created_at))

    def get_next_ready_task(self) -> Task | None:
        ready = self.get_ready_tasks()
        return ready[0] if ready else None

    def _is_rate_limited(self, agent_name: str) -> bool:
        agent = self.agents[agent_name]
        now = utc_now()
        window = self._rate_window[agent_name]
        while window and (now - window[0]) > timedelta(minutes=1):
            window.popleft()
        return len(window) >= agent.spec.max_tasks_per_minute

    def _touch_rate_limit(self, agent_name: str) -> None:
        self._rate_window[agent_name].append(utc_now())

    def _is_circuit_open(self, agent_name: str) -> bool:
        failures, open_until = self._circuit_state.get(agent_name, (0, None))
        if open_until is None:
            return False
        if utc_now() >= open_until:
            self._circuit_state[agent_name] = (0, None)
            self._record("agent.circuit_reset", agent=agent_name)
            return False
        return failures > 0

    def _mark_agent_failure(self, agent_name: str) -> None:
        failures, _ = self._circuit_state.get(agent_name, (0, None))
        failures += 1
        spec = self.agents[agent_name].spec
        open_until = None
        if failures >= spec.failure_threshold:
            open_until = utc_now() + timedelta(seconds=spec.cooldown_seconds)
            self._record("agent.circuit_open", agent=agent_name, cooldown_seconds=spec.cooldown_seconds)
        self._circuit_state[agent_name] = (failures, open_until)

    def _mark_agent_success(self, agent_name: str) -> None:
        self._circuit_state[agent_name] = (0, None)

    def _agent_perf_score(self, agent_name: str) -> float:
        durations = self._agent_duration_ms[agent_name]
        if not durations:
            return 0.0
        return sum(durations) / len(durations)

    def assign_task(self, task_id: str) -> Task:
        task = self._get_task(task_id)
        compatible_agents = [
            name
            for name, agent in self.agents.items()
            if agent.spec.enabled
            and not self._is_circuit_open(name)
            and not self._is_rate_limited(name)
            and (task.required_capability is None or task.required_capability in agent.spec.capabilities)
        ]

        if not compatible_agents:
            raise NoCompatibleAgentError(f"No compatible agent found for task {task.id}")

        min_load = min(self._assignment_count[a] for a in compatible_agents)
        least_loaded = [a for a in compatible_agents if self._assignment_count[a] == min_load]
        least_loaded = sorted(least_loaded, key=self._agent_perf_score)

        if self._agent_cursor is None:
            self._agent_cursor = cycle(self.agents.keys())
        selected = None
        for _ in range(len(self.agents)):
            candidate = next(self._agent_cursor)
            if candidate in least_loaded:
                selected = candidate
                break
        if selected is None:
            selected = least_loaded[0]

        task.assigned_agent = selected
        task.updated_at = utc_now()
        self._assignment_count[selected] += 1
        self._record("task.assigned", task_id=task.id, agent=selected)
        return task

    def _policy_validate(self, task: Task) -> tuple[bool, str]:
        for policy in self.policies:
            ok, msg = policy.validate(task)
            if not ok:
                return False, f"{policy.name}: {msg}"
        return True, "ok"

    def run_task(self, task_id: str, use_cache: bool = True) -> TaskResult:
        task = self._get_task(task_id)
        if task.state in {TaskState.DONE, TaskState.FAILED, TaskState.SKIPPED}:
            raise InvalidTaskStateError(f"Task {task.id} already terminal ({task.state.value})")

        if not self._dependencies_done(task):
            self._set_task_state(task, TaskState.BLOCKED)
            self.metrics.blocked_tasks += 1
            return TaskResult(task_id=task.id, success=False, summary="Task blocked by dependencies")

        ok, reason = self._policy_validate(task)
        if not ok:
            self._set_task_state(task, TaskState.SKIPPED)
            self.metrics.failed_tasks += 1
            task.error = reason
            result = TaskResult(task_id=task.id, success=False, summary=f"Policy rejected: {reason}")
            self.history.append(result)
            self._record("task.policy_rejected", task_id=task.id, reason=reason)
            return result

        cache_key = (task.title, task.description, task.required_capability)
        if use_cache and cache_key in self._result_cache:
            cached = self._result_cache[cache_key]
            self.metrics.cached_hits += 1
            self._set_task_state(task, TaskState.DONE)
            result = TaskResult(
                task_id=task.id,
                success=cached.success,
                summary=f"[cache] {cached.summary}",
                output=cached.output,
                duration_ms=0.0,
                from_cache=True,
            )
            self.history.append(result)
            self.metrics.completed_tasks += 1
            self._record("task.cache_hit", task_id=task.id)
            return result

        if not task.assigned_agent:
            task = self.assign_task(task_id)

        if task.assigned_agent not in self.agents:
            raise AgentNotFoundError(f"Assigned agent '{task.assigned_agent}' no longer exists")

        self._set_task_state(task, TaskState.RUNNING)
        task.attempts += 1
        self._touch_rate_limit(task.assigned_agent)

        agent = self.agents[task.assigned_agent]
        started = perf_counter()
        crashed = False
        try:
            with ThreadPoolExecutor(max_workers=1) as pool:
                future = pool.submit(agent.run, task)
                result = future.result(timeout=task.timeout_seconds)
        except Exception as exc:  # noqa: BLE001
            crashed = True
            task.error = str(exc)
            self._mark_agent_failure(task.assigned_agent)
            self._set_task_state(task, TaskState.FAILED)
            result = TaskResult(task_id=task.id, success=False, summary=f"Agent failed: {exc}", output={"error": str(exc)})

        duration_ms = (perf_counter() - started) * 1000
        result.duration_ms = duration_ms
        self.metrics.record_duration(duration_ms)
        self._agent_duration_ms[task.assigned_agent].append(duration_ms)

        if result.success:
            self._mark_agent_success(task.assigned_agent)
            self._set_task_state(task, TaskState.DONE)
            task.error = None
            if use_cache:
                self._result_cache[cache_key] = result
            self.metrics.completed_tasks += 1
        elif not crashed:
            self._mark_agent_failure(task.assigned_agent)
            task.error = result.summary
            if task.attempts >= task.max_attempts:
                self._set_task_state(task, TaskState.FAILED)
                self.metrics.failed_tasks += 1
            else:
                self._set_task_state(task, TaskState.AWAITING_REVIEW)

        self.history.append(result)
        self._record("task.completed", task_id=task.id, success=result.success, attempts=task.attempts, duration_ms=duration_ms)
        return result

    def run_pending(self, limit: int | None = None) -> list[TaskResult]:
        results: list[TaskResult] = []
        remaining = limit
        while True:
            task = self.get_next_ready_task()
            if task is None:
                break
            if remaining is not None and remaining <= 0:
                break
            results.append(self.run_task(task.id))
            if remaining is not None:
                remaining -= 1
        return results

    def run_pending_parallel(self, max_workers: int = 4, limit: int | None = None) -> list[TaskResult]:
        ready = self.get_ready_tasks()
        if limit is not None:
            ready = ready[:limit]

        results: list[TaskResult] = []
        if not ready:
            return results

        with ThreadPoolExecutor(max_workers=max_workers) as pool:
            futures = {pool.submit(self.run_task, task.id): task.id for task in ready}
            for fut in as_completed(futures):
                results.append(fut.result())
        return results

    def metrics_snapshot(self) -> MetricsSnapshot:
        return self.metrics.snapshot()

    def clear_cache(self) -> None:
        self._result_cache.clear()
        self._record("cache.cleared")
