from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime, timezone
from enum import Enum
from typing import Any, Dict, Optional
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
    metadata: Dict[str, Any] = field(default_factory=dict)
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
    output: Dict[str, Any] = field(default_factory=dict)
    duration_ms: float = 0.0
    from_cache: bool = False


@dataclass(slots=True)
class AuditEvent:
    at: datetime
    event: str
    details: Dict[str, Any] = field(default_factory=dict)
