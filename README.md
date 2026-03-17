# PyForge

PyForge ist ein **Python Multi-Agent Orchestrator** für Softwareentwicklung.

## Neu integrierte Performance- und Enterprise-Features

Die folgenden Erweiterungen wurden direkt implementiert, um Leistung, Robustheit und Skalierbarkeit zu erhöhen:

1. **Prioritätsbasiertes Scheduling** (`Task.priority`, `get_ready_tasks`).
2. **Dependency-Handling** (`Task.dependencies`, BLOCKED-Status bei offenen Abhängigkeiten).
3. **Batch-Task-Erstellung** (`create_tasks`).
4. **Deterministische + load-aware Agent-Zuweisung** (Round-Robin + least-loaded).
5. **Result-Cache** für identische Task-Signaturen.
6. **Policy-Engine** mit erweiterbaren Rules (`policy.py`).
7. **Timeout-geschützte Ausführung** pro Task.
8. **Rate-Limiting pro Agent** (`max_tasks_per_minute`).
9. **Circuit Breaker pro Agent** (`failure_threshold`, `cooldown_seconds`).
10. **Metrics Collector + Snapshot** (created/completed/failed/cache_hits/blocked/avg_ms).
11. **Event Hooks** für Audit-Event-Streaming.
12. **Agent enable/disable** zur operativen Steuerung.
13. **Idempotency-Key Ingestion** zur Deduplizierung.
14. **Parallel Pending Scheduler** für höheren Durchsatz.
15. **Queue Backpressure** über `max_queue_size`.
16. **Advanced SLO-Metrics** (`p95`, `p99`, `success_rate`).
17. **Adaptive Agent Routing** auf Basis historischer Latenz.

## Aktueller Stand

Dieses Setup liefert außerdem:
- Fehlerklassen für robuste Domänenfehler
- Task-Lifecycle mit validierten State-Transitions
- Audit-Logging für Nachvollziehbarkeit
- CLI (`demo`, `health`, `metrics`)
- Unit-Tests für Kern- und Fehlerpfade

## Schnellstart

```bash
python -m venv .venv
source .venv/bin/activate
pip install -e .
pyforge demo
pyforge health
pyforge metrics
pyforge parallel-demo
```

## Tests

```bash
python -m unittest discover -s tests -v
```

## Analysen

- `docs/pyforge_plan.md`
- `docs/pyforge_enterprise_assessment.md`
- `docs/pyforge_performance_expansion.md` (Round 2 Upgrade)
- `docs/pyforge_bug_audit.md` (Fehler- und Bug-Analyse)

## Vollautomatisierte Installation per Shell-Skript

```bash
bash scripts/install_pyforge.sh --target ./pyforge_bootstrap
```

Optional: `--force` zum Überschreiben bestehender Dateien, `--skip-tests` zum Überspringen der Smoke-Tests.
