#!/usr/bin/env bash
set -euo pipefail

# Fully automated PyForge bootstrap + install script
# - Creates folder structure
# - Generates core PyForge files
# - Creates virtualenv
# - Installs dependencies and package
# - Runs smoke checks/tests

TARGET_DIR=""
FORCE=0
SKIP_TESTS=0

usage() {
  cat <<'USAGE'
Usage: ./scripts/install_pyforge.sh [options]

Options:
  --target <dir>     Target project directory (default: ./pyforge_bootstrap)
  --force            Overwrite existing files
  --skip-tests       Skip smoke checks and tests
  -h, --help         Show this help
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --target)
      TARGET_DIR="${2:-}"
      shift 2
      ;;
    --force)
      FORCE=1
      shift
      ;;
    --skip-tests)
      SKIP_TESTS=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ -z "$TARGET_DIR" ]]; then
  TARGET_DIR="$(pwd)/pyforge_bootstrap"
fi

mkdir -p "$TARGET_DIR"

write_file() {
  local file="$1"
  local content="$2"
  local dir
  dir="$(dirname "$file")"
  mkdir -p "$dir"

  if [[ -f "$file" && "$FORCE" -ne 1 ]]; then
    echo "[skip] $file already exists (use --force to overwrite)"
    return 0
  fi

  printf "%s" "$content" > "$file"
  echo "[write] $file"
}

PYPROJECT_CONTENT='[build-system]
requires = ["setuptools>=68", "wheel"]
build-backend = "setuptools.build_meta"

[project]
name = "pyforge"
version = "0.1.0"
description = "Python Multi-Agent Orchestrator"
readme = "README.md"
requires-python = ">=3.10"
authors = [{name = "PyForge Team"}]

[project.scripts]
pyforge = "pyforge.cli:main"

[tool.setuptools]
package-dir = {"" = "src"}

[tool.setuptools.packages.find]
where = ["src"]
'

GITIGNORE_CONTENT='__pycache__/
*.pyc
.venv/
.pytest_cache/
'

README_CONTENT='# PyForge

PyForge ist ein Python Multi-Agent Orchestrator.

## Installation (automatisch)

```bash
bash scripts/install_pyforge.sh --target ./pyforge_bootstrap
```

## Danach ausführen

```bash
cd pyforge_bootstrap
source .venv/bin/activate
pyforge health
pyforge demo
pyforge metrics
```
'

INIT_CONTENT='"""PyForge package."""

from .orchestrator import InMemoryOrchestrator

__all__ = ["InMemoryOrchestrator"]
'

ERRORS_CONTENT='class PyForgeError(Exception):
    """Base exception for PyForge."""


class AgentRegistrationError(PyForgeError):
    pass


class AgentNotFoundError(PyForgeError):
    pass


class TaskNotFoundError(PyForgeError):
    pass


class NoCompatibleAgentError(PyForgeError):
    pass


class InvalidTaskStateError(PyForgeError):
    pass
'

MODELS_CONTENT='from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime, timezone
from enum import Enum
from typing import Any, Optional
from uuid import uuid4


def utc_now() -> datetime:
    return datetime.now(timezone.utc)


class TaskState(str, Enum):
    QUEUED = "queued"
    RUNNING = "running"
    AWAITING_REVIEW = "awaiting_review"
    DONE = "done"
    FAILED = "failed"
    BLOCKED = "blocked"
    SKIPPED = "skipped"


@dataclass(slots=True)
class AgentSpec:
    name: str
    capabilities: set[str] = field(default_factory=set)
    metadata: dict[str, Any] = field(default_factory=dict)
    enabled: bool = True
    max_tasks_per_minute: int = 120
    failure_threshold: int = 3
    cooldown_seconds: int = 30


@dataclass(slots=True)
class Task:
    title: str
    description: str
    required_capability: Optional[str] = None
    id: str = field(default_factory=lambda: str(uuid4()))
    state: TaskState = TaskState.QUEUED
    assigned_agent: Optional[str] = None
    attempts: int = 0
    max_attempts: int = 3
    error: Optional[str] = None
    priority: int = 100
    dependencies: list[str] = field(default_factory=list)
    tags: set[str] = field(default_factory=set)
    timeout_seconds: float = 30.0
    idempotency_key: Optional[str] = None
    created_at: datetime = field(default_factory=utc_now)
    updated_at: datetime = field(default_factory=utc_now)


@dataclass(slots=True)
class TaskResult:
    task_id: str
    success: bool
    summary: str
    output: dict[str, Any] = field(default_factory=dict)
    duration_ms: float = 0.0
    from_cache: bool = False


@dataclass(slots=True)
class AuditEvent:
    at: datetime
    event: str
    details: dict[str, Any] = field(default_factory=dict)
'

AGENTS_CONTENT='from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol

from .models import AgentSpec, Task, TaskResult


class AgentRunner(Protocol):
    spec: AgentSpec

    def run(self, task: Task) -> TaskResult:
        ...


@dataclass
class EchoAgent:
    spec: AgentSpec

    def run(self, task: Task) -> TaskResult:
        return TaskResult(
            task_id=task.id,
            success=True,
            summary=f"[{self.spec.name}] completed task '{task.title}'",
            output={"echo": task.description},
        )
'

METRICS_CONTENT='from dataclasses import dataclass, field


def _percentile(values: list[float], p: float) -> float:
    if not values:
        return 0.0
    ordered = sorted(values)
    idx = min(len(ordered) - 1, max(0, int(round((p / 100) * (len(ordered) - 1)))))
    return ordered[idx]


@dataclass
class MetricsSnapshot:
    created_tasks: int
    completed_tasks: int
    failed_tasks: int
    cached_hits: int
    blocked_tasks: int
    avg_duration_ms: float
    p95_duration_ms: float
    p99_duration_ms: float
    success_rate: float


@dataclass
class MetricsCollector:
    created_tasks: int = 0
    completed_tasks: int = 0
    failed_tasks: int = 0
    cached_hits: int = 0
    blocked_tasks: int = 0
    durations: list[float] = field(default_factory=list)

    def record_duration(self, ms: float) -> None:
        self.durations.append(ms)

    def snapshot(self) -> MetricsSnapshot:
        avg = sum(self.durations) / len(self.durations) if self.durations else 0.0
        total_finished = self.completed_tasks + self.failed_tasks
        success_rate = (self.completed_tasks / total_finished) if total_finished else 0.0
        return MetricsSnapshot(
            created_tasks=self.created_tasks,
            completed_tasks=self.completed_tasks,
            failed_tasks=self.failed_tasks,
            cached_hits=self.cached_hits,
            blocked_tasks=self.blocked_tasks,
            avg_duration_ms=avg,
            p95_duration_ms=_percentile(self.durations, 95),
            p99_duration_ms=_percentile(self.durations, 99),
            success_rate=success_rate,
        )
'

POLICY_CONTENT='from dataclasses import dataclass
from typing import Protocol

from .models import Task


class PolicyRule(Protocol):
    name: str

    def validate(self, task: Task) -> tuple[bool, str]:
        ...


@dataclass
class RequiredTitlePolicy:
    name: str = "required_title"

    def validate(self, task: Task) -> tuple[bool, str]:
        return (True, "ok") if task.title.strip() else (False, "Task title must not be empty")
'

ORCH_CONTENT='from dataclasses import dataclass, field

from .agents import AgentRunner
from .errors import AgentRegistrationError, NoCompatibleAgentError
from .metrics import MetricsCollector
from .models import Task, TaskResult, TaskState
from .policy import PolicyRule, RequiredTitlePolicy


@dataclass
class InMemoryOrchestrator:
    agents: dict[str, AgentRunner] = field(default_factory=dict)
    tasks: dict[str, Task] = field(default_factory=dict)
    history: list[TaskResult] = field(default_factory=list)
    policies: list[PolicyRule] = field(default_factory=lambda: [RequiredTitlePolicy()])
    metrics: MetricsCollector = field(default_factory=MetricsCollector)

    def register_agent(self, agent: AgentRunner) -> None:
        if not agent.spec.name:
            raise AgentRegistrationError("Agent name must not be empty")
        if agent.spec.name in self.agents:
            raise AgentRegistrationError(f"Agent {agent.spec.name} already exists")
        self.agents[agent.spec.name] = agent

    def create_task(self, task: Task) -> Task:
        self.tasks[task.id] = task
        self.metrics.created_tasks += 1
        return task

    def assign_task(self, task_id: str) -> Task:
        task = self.tasks[task_id]
        for name, agent in self.agents.items():
            if task.required_capability is None or task.required_capability in agent.spec.capabilities:
                task.assigned_agent = name
                return task
        raise NoCompatibleAgentError(f"No compatible agent found for task {task.id}")

    def run_task(self, task_id: str) -> TaskResult:
        task = self.tasks[task_id]
        if not task.assigned_agent:
            self.assign_task(task_id)
        task.state = TaskState.RUNNING
        result = self.agents[task.assigned_agent].run(task)
        task.state = TaskState.DONE if result.success else TaskState.FAILED
        self.history.append(result)
        self.metrics.completed_tasks += 1 if result.success else 0
        self.metrics.failed_tasks += 0 if result.success else 1
        return result
'

CLI_CONTENT='import argparse

from .agents import EchoAgent
from .models import AgentSpec, Task
from .orchestrator import InMemoryOrchestrator


def _build() -> InMemoryOrchestrator:
    orch = InMemoryOrchestrator()
    orch.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))
    orch.register_agent(EchoAgent(AgentSpec(name="blackbox", capabilities={"python", "review"})))
    return orch


def run_demo() -> int:
    orch = _build()
    t = orch.create_task(Task(title="Create initial project skeleton", description="Generate base structure", required_capability="python"))
    r = orch.run_task(t.id)
    print(r.summary)
    return 0


def run_health() -> int:
    orch = _build()
    print(f"status=ok agents={len(orch.agents)} tasks={len(orch.tasks)}")
    return 0


def run_metrics() -> int:
    orch = _build()
    for idx in range(1, 4):
        task = orch.create_task(Task(title=f"Task {idx}", description="batch", required_capability="python"))
        orch.run_task(task.id)
    m = orch.metrics.snapshot()
    print(f"created={m.created_tasks} completed={m.completed_tasks} failed={m.failed_tasks} avg_ms={m.avg_duration_ms:.2f}")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(prog="pyforge")
    parser.add_argument("command", choices=["health", "demo", "metrics"])
    args = parser.parse_args()

    if args.command == "health":
        return run_health()
    if args.command == "demo":
        return run_demo()
    return run_metrics()


if __name__ == "__main__":
    raise SystemExit(main())
'

TEST_CONTENT='import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "src"))

from pyforge.agents import EchoAgent
from pyforge.models import AgentSpec, Task, TaskState
from pyforge.orchestrator import InMemoryOrchestrator


class OrchestratorTests(unittest.TestCase):
    def test_assign_and_run(self) -> None:
        orch = InMemoryOrchestrator()
        orch.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"python"})))
        task = orch.create_task(Task(title="Build", description="Implement", required_capability="python"))
        result = orch.run_task(task.id)
        self.assertTrue(result.success)
        self.assertEqual(orch.tasks[task.id].state, TaskState.DONE)


if __name__ == "__main__":
    unittest.main()
'

write_file "$TARGET_DIR/pyproject.toml" "$PYPROJECT_CONTENT"
write_file "$TARGET_DIR/.gitignore" "$GITIGNORE_CONTENT"
write_file "$TARGET_DIR/README.md" "$README_CONTENT"

write_file "$TARGET_DIR/src/pyforge/__init__.py" "$INIT_CONTENT"
write_file "$TARGET_DIR/src/pyforge/errors.py" "$ERRORS_CONTENT"
write_file "$TARGET_DIR/src/pyforge/models.py" "$MODELS_CONTENT"
write_file "$TARGET_DIR/src/pyforge/agents.py" "$AGENTS_CONTENT"
write_file "$TARGET_DIR/src/pyforge/metrics.py" "$METRICS_CONTENT"
write_file "$TARGET_DIR/src/pyforge/policy.py" "$POLICY_CONTENT"
write_file "$TARGET_DIR/src/pyforge/orchestrator.py" "$ORCH_CONTENT"
write_file "$TARGET_DIR/src/pyforge/cli.py" "$CLI_CONTENT"

write_file "$TARGET_DIR/tests/test_orchestrator.py" "$TEST_CONTENT"
write_file "$TARGET_DIR/docs/pyforge_plan.md" "# PyForge Plan\n\nGenerated by install script.\n"
write_file "$TARGET_DIR/docs/pyforge_enterprise_assessment.md" "# PyForge Enterprise Assessment\n\nGenerated by install script.\n"
write_file "$TARGET_DIR/docs/pyforge_performance_expansion.md" "# PyForge Performance Expansion\n\nGenerated by install script.\n"
write_file "$TARGET_DIR/docs/pyforge_bug_audit.md" "# PyForge Bug Audit\n\nGenerated by install script.\n"

if ! command -v python3 >/dev/null 2>&1; then
  echo "python3 not found. Please install Python 3.10+." >&2
  exit 1
fi

pushd "$TARGET_DIR" >/dev/null

python3 -m venv .venv
# shellcheck disable=SC1091
source .venv/bin/activate
python -m pip install --upgrade pip setuptools wheel || echo "[warn] pip/setuptools/wheel upgrade skipped (offline/proxy)"
python -m pip install -e . --no-build-isolation

if [[ "$SKIP_TESTS" -ne 1 ]]; then
  PYTHONPATH=src python -m pyforge.cli health
  PYTHONPATH=src python -m pyforge.cli demo
  PYTHONPATH=src python -m pyforge.cli metrics
  python -m unittest discover -s tests -v
fi

popd >/dev/null

echo

echo "✅ PyForge wurde vollautomatisiert installiert unter: $TARGET_DIR"
echo "➡️  Aktivieren: source $TARGET_DIR/.venv/bin/activate"
echo "➡️  Starten:    pyforge health"
