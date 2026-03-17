from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol

from .models import AgentSpec, Task, TaskResult


class AgentRunner(Protocol):
    spec: AgentSpec

    def run(self, task: Task) -> TaskResult:
        ...


@dataclass
class EchoAgent:
    """Minimaler Agent für lokale End-to-End-Tests."""

    spec: AgentSpec

    def run(self, task: Task) -> TaskResult:
        text = f"[{self.spec.name}] completed task '{task.title}'"
        return TaskResult(task_id=task.id, success=True, summary=text, output={"echo": task.description})
