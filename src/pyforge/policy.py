from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol

from .models import Task


class PolicyRule(Protocol):
    name: str

    def validate(self, task: Task) -> tuple[bool, str]:
        ...


@dataclass
class MaxDescriptionLengthPolicy:
    max_chars: int = 5000
    name: str = "max_description_length"

    def validate(self, task: Task) -> tuple[bool, str]:
        if len(task.description) <= self.max_chars:
            return True, "ok"
        return False, f"Task description exceeds {self.max_chars} chars"


@dataclass
class RequiredTitlePolicy:
    name: str = "required_title"

    def validate(self, task: Task) -> tuple[bool, str]:
        if task.title.strip():
            return True, "ok"
        return False, "Task title must not be empty"
