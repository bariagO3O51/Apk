from __future__ import annotations

import argparse

from .agents import EchoAgent
from .models import AgentSpec, Task
from .orchestrator import InMemoryOrchestrator
from .policy import MaxDescriptionLengthPolicy


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="pyforge", description="PyForge Multi-Agent Orchestrator CLI")
    subparsers = parser.add_subparsers(dest="command", required=True)

    subparsers.add_parser("demo", help="Run enhanced local orchestrator demo")
    subparsers.add_parser("health", help="Print basic runtime health information")
    subparsers.add_parser("metrics", help="Print metrics after a sample workload")
    subparsers.add_parser("parallel-demo", help="Run workload using parallel scheduler")
    return parser


def _build_demo_orchestrator() -> InMemoryOrchestrator:
    orchestrator = InMemoryOrchestrator()
    orchestrator.add_policy(MaxDescriptionLengthPolicy(max_chars=4000))

    orchestrator.register_agent(EchoAgent(AgentSpec(name="codex", capabilities={"planning", "python"})))
    orchestrator.register_agent(EchoAgent(AgentSpec(name="blackbox", capabilities={"review", "python"})))
    return orchestrator


def run_demo() -> int:
    orchestrator = _build_demo_orchestrator()

    task1 = orchestrator.create_task(
        Task(
            title="Create initial project skeleton",
            description="Generate base package structure for PyForge.",
            required_capability="python",
            priority=10,
            tags={"bootstrap"},
            idempotency_key="bootstrap-task",
        )
    )
    task2 = orchestrator.create_task(
        Task(
            title="Create initial project skeleton",
            description="Generate base package structure for PyForge.",
            required_capability="python",
            priority=20,
            tags={"cache-demo"},
        )
    )

    first = orchestrator.run_task(task1.id)
    second = orchestrator.run_task(task2.id)
    print(first.summary)
    print(second.summary)
    return 0 if first.success and second.success else 1


def run_health() -> int:
    orchestrator = _build_demo_orchestrator()
    print(f"status=ok agents={len(orchestrator.agents)} tasks={len(orchestrator.tasks)} policies={len(orchestrator.policies)}")
    return 0


def run_metrics() -> int:
    orchestrator = _build_demo_orchestrator()
    tasks = [
        Task(title=f"Task {idx}", description="Batch task", required_capability="python", priority=idx)
        for idx in range(1, 4)
    ]
    orchestrator.create_tasks(tasks)
    orchestrator.run_pending()
    snapshot = orchestrator.metrics_snapshot()
    print(
        "created={created} completed={completed} failed={failed} cache_hits={cache} blocked={blocked} "
        "avg_ms={avg:.2f} p95_ms={p95:.2f} p99_ms={p99:.2f} success_rate={sr:.2%}".format(
            created=snapshot.created_tasks,
            completed=snapshot.completed_tasks,
            failed=snapshot.failed_tasks,
            cache=snapshot.cached_hits,
            blocked=snapshot.blocked_tasks,
            avg=snapshot.avg_duration_ms,
            p95=snapshot.p95_duration_ms,
            p99=snapshot.p99_duration_ms,
            sr=snapshot.success_rate,
        )
    )
    return 0


def run_parallel_demo() -> int:
    orchestrator = _build_demo_orchestrator()
    tasks = [
        Task(title=f"Parallel Task {idx}", description="parallel run", required_capability="python", priority=idx)
        for idx in range(1, 11)
    ]
    orchestrator.create_tasks(tasks)
    results = orchestrator.run_pending_parallel(max_workers=4)
    ok = sum(1 for r in results if r.success)
    print(f"parallel_completed={ok}/{len(results)}")
    return 0 if ok == len(results) else 1


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    if args.command == "demo":
        return run_demo()
    if args.command == "health":
        return run_health()
    if args.command == "metrics":
        return run_metrics()
    if args.command == "parallel-demo":
        return run_parallel_demo()
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
