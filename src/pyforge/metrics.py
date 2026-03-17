from __future__ import annotations

from dataclasses import dataclass, field


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
