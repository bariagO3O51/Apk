import sys
import time
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "src"))

from pyforge.agents import EchoAgent
from pyforge.errors import AgentRegistrationError, InvalidTaskStateError, NoCompatibleAgentError
from pyforge.models import AgentSpec, Task, TaskResult, TaskState
from pyforge.orchestrator import InMemoryOrchestrator


class FailingAgent:
    def __init__(self, name: str = "failing") -> None:
        self.spec = AgentSpec(name=name, capabilities={"python"}, failure_threshold=2, cooldown_seconds=1)

    def run(self, task: Task) -> TaskResult:
        return TaskResult(task_id=task.id, success=False, summary="failed validation")


class CrashingAgent:
    def __init__(self, name: str = "crasher") -> None:
        self.spec = AgentSpec(name=name, capabilities={"python"}, failure_threshold=2, cooldown_seconds=1)

    def run(self, task: Task) -> TaskResult:
        raise RuntimeError("boom")


class SlowAgent:
    def __init__(self, name: str = "slow") -> None:
        self.spec = AgentSpec(name=name, capabilities={"python"})

    def run(self, task: Task) -> TaskResult:
        time.sleep(0.1)
        return TaskResult(task_id=task.id, success=True, summary="slow done")


class OrchestratorTests(unittest.TestCase):
    def test_assign_and_run_task(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))

        task = orchestrator.create_task(Task(title="Build feature", description="Implement endpoint", required_capability="python"))
        result = orchestrator.run_task(task.id)

        self.assertTrue(result.success)
        self.assertEqual(orchestrator.tasks[task.id].state, TaskState.DONE)

    def test_idempotency_key_deduplicates_create(self) -> None:
        orchestrator = InMemoryOrchestrator()
        t1 = orchestrator.create_task(Task(title="A", description="B", idempotency_key="key-1"))
        t2 = orchestrator.create_task(Task(title="C", description="D", idempotency_key="key-1"))
        self.assertEqual(t1.id, t2.id)

    def test_cache_hit_for_identical_task_signature(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))
        t1 = orchestrator.create_task(Task(title="X", description="Y", required_capability="python"))
        t2 = orchestrator.create_task(Task(title="X", description="Y", required_capability="python"))

        _ = orchestrator.run_task(t1.id)
        r2 = orchestrator.run_task(t2.id)

        self.assertTrue(r2.success)
        self.assertTrue(r2.from_cache)

    def test_dependency_blocking(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))
        dep = orchestrator.create_task(Task(title="dep", description="d", required_capability="python"))
        main = orchestrator.create_task(Task(title="main", description="m", required_capability="python", dependencies=[dep.id]))

        result = orchestrator.run_task(main.id)
        self.assertFalse(result.success)
        self.assertEqual(orchestrator.tasks[main.id].state, TaskState.BLOCKED)

    def test_priority_run_pending(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))
        high = orchestrator.create_task(Task(title="high", description="a", required_capability="python", priority=1))
        low = orchestrator.create_task(Task(title="low", description="b", required_capability="python", priority=100))

        results = orchestrator.run_pending()
        self.assertEqual(results[0].task_id, high.id)
        self.assertEqual(results[1].task_id, low.id)

    def test_parallel_pending_execution(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="a", capabilities={"python"})))
        orchestrator.register_agent(EchoAgent(AgentSpec(name="b", capabilities={"python"})))
        tasks = [Task(title=f"t{idx}", description="x", required_capability="python") for idx in range(8)]
        orchestrator.create_tasks(tasks)

        results = orchestrator.run_pending_parallel(max_workers=4)
        self.assertEqual(len(results), 8)
        self.assertTrue(all(r.success for r in results))

    def test_metrics_snapshot_contains_percentiles(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))
        task = orchestrator.create_task(Task(title="m", description="n", required_capability="python"))
        orchestrator.run_task(task.id)
        snap = orchestrator.metrics_snapshot()
        self.assertGreaterEqual(snap.p95_duration_ms, 0.0)
        self.assertGreaterEqual(snap.p99_duration_ms, 0.0)

    def test_raises_when_no_compatible_agent_exists(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="reviewer", capabilities={"review"})))
        task = orchestrator.create_task(Task(title="Build feature", description="Implement endpoint", required_capability="python"))

        with self.assertRaises(NoCompatibleAgentError):
            orchestrator.run_task(task.id)

    def test_duplicate_agent_registration_is_rejected(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))

        with self.assertRaises(AgentRegistrationError):
            orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"review"})))

    def test_failed_task_moves_to_review_then_failed_after_retries(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(FailingAgent())
        task = orchestrator.create_task(Task(title="Build feature", description="Implement endpoint", required_capability="python", max_attempts=2))

        first = orchestrator.run_task(task.id)
        self.assertFalse(first.success)
        self.assertEqual(orchestrator.tasks[task.id].state, TaskState.AWAITING_REVIEW)

        second = orchestrator.run_task(task.id)
        self.assertFalse(second.success)
        self.assertEqual(orchestrator.tasks[task.id].state, TaskState.FAILED)

    def test_crashing_agent_marks_task_failed(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(CrashingAgent())
        task = orchestrator.create_task(Task(title="Crash", description="Crash test", required_capability="python"))

        result = orchestrator.run_task(task.id)
        self.assertFalse(result.success)
        self.assertEqual(orchestrator.tasks[task.id].state, TaskState.FAILED)

    def test_timeout_marks_task_failed(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(SlowAgent())
        task = orchestrator.create_task(Task(title="Slow", description="x", required_capability="python", timeout_seconds=0.01))

        result = orchestrator.run_task(task.id)
        self.assertFalse(result.success)
        self.assertEqual(orchestrator.tasks[task.id].state, TaskState.FAILED)

    def test_running_terminal_task_raises(self) -> None:
        orchestrator = InMemoryOrchestrator()
        orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))
        task = orchestrator.create_task(Task(title="Build", description="done", required_capability="python", state=TaskState.DONE))

        with self.assertRaises(InvalidTaskStateError):
            orchestrator.run_task(task.id)


if __name__ == "__main__":
    unittest.main()
