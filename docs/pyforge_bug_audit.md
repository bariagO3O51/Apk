# PyForge Bug & Risk Audit (Ground-up)

## Scope

Analysierte Artefakte:
- `src/pyforge/orchestrator.py`
- `src/pyforge/models.py`
- `src/pyforge/metrics.py`
- `src/pyforge/cli.py`
- `tests/test_orchestrator.py`

Zusätzlich wurden Runtime-Checks ausgeführt (`unittest`, CLI-Demos).

---

## Executive Summary

PyForge ist funktional, aber enthält mehrere Stellen, die unter Last, Parallelität und Fehlerfällen
zu realen Bugs oder Inkonsistenzen führen können. Die wichtigsten Risiken liegen in:

1. Nebenläufigkeit ohne vollständige Synchronisierung,
2. inkonsistenter Fehlermetrik,
3. BLOCKED-Task-Lifecycle-Falle,
4. Timeout-/Thread-Handling,
5. ungebremstem Speicherwachstum.

---

## Präzise Bug-/Fehlerliste inkl. Fix-Empfehlung

## Kritisch

### 1) Race-Conditions bei paralleler Ausführung (shared mutable state)
**Befund:** `run_pending_parallel()` ruft `run_task()` parallel auf, aber zentrale mutable Strukturen werden ohne durchgängige Locks verändert (`tasks`, `history`, `metrics`, `_assignment_count`, `_rate_window`, `_agent_duration_ms`, `_result_cache`).

**Codehinweise:**
- Parallelstart: `run_pending_parallel`【F:src/pyforge/orchestrator.py†L326-L339】
- Mutationen in `run_task`/`assign_task`/`_set_task_state`【F:src/pyforge/orchestrator.py†L183-L216】【F:src/pyforge/orchestrator.py†L269-L310】【F:src/pyforge/orchestrator.py†L110-L116】

**Risiko:** Datenrennen, inkonsistente Status, doppelte Zuweisungen, ungenaue Metrics.

**Fix:**
- Einheitliches Locking-Konzept (mind. coarse-grained Lock in `run_task`-kritischen Abschnitten oder feingranulare Locks pro Datenstruktur).
- Optional: Aufgaben nur im Scheduler-Thread mutieren, Worker liefern reine Resultate.

---

### 2) BLOCKED-Tasks können „hängen bleiben“
**Befund:** Wenn Dependencies fehlen, wird Task auf `BLOCKED` gesetzt. `get_ready_tasks()` berücksichtigt aber nur `QUEUED` und `AWAITING_REVIEW`. Ein BLOCKED-Task wird daher später nicht automatisch wieder aufgenommen.

**Codehinweise:**
- BLOCKED setzen【F:src/pyforge/orchestrator.py†L230-L233】
- Ready-Filter ohne BLOCKED【F:src/pyforge/orchestrator.py†L131-L137】

**Risiko:** Deadlock-artiges Verhalten im Orchestrator-Lifecycle.

**Fix:**
- Auto-Unblock-Mechanismus (`BLOCKED -> QUEUED` sobald Dependencies erfüllt).
- Alternativ `get_ready_tasks()` um BLOCKED+dependency-check erweitern.

---

### 3) Timeout-Fall beendet Agent-Thread nicht sauber
**Befund:** Timeout wird via `future.result(timeout=...)` ausgelöst, aber laufende Arbeit im Thread kann weiterlaufen (keine harte Cancellation bei Python-Threads).

**Codehinweise:**
- Timeout-Mechanik【F:src/pyforge/orchestrator.py†L277-L280】

**Risiko:** Zombie-Workloads, Ressourcenverbrauch, Doppelarbeit.

**Fix:**
- Prozessbasierte Isolation (multiprocessing/subprocess/container) statt Thread.
- Harte Kill-Strategie pro Task-Execution.

---

## Hoch

### 4) Fehlermetrik inkonsistent bei Crash-/Timeout-Failures
**Befund:** `metrics.failed_tasks` wird im Crash/Exception-Pfad nicht hochgezählt; nur bei fachlichem Fehlschlag mit `attempts >= max_attempts`.

**Codehinweise:**
- Exception-Pfad ohne `failed_tasks += 1`【F:src/pyforge/orchestrator.py†L280-L286】
- Inkrement nur in Teilpfad【F:src/pyforge/orchestrator.py†L302-L305】
- Success-Rate basiert auf completed/failed【F:src/pyforge/metrics.py†L39-L53】

**Risiko:** Falsche SLO-Daten, falsches Alerting/Steering.

**Fix:**
- Jeder terminale Failure-Pfad muss `failed_tasks` erhöhen.
- Einheitliche Outcome-Normalisierung vor Metrics-Update.

---

### 5) Event-Hook-Ausnahmen können Kernfluss brechen
**Befund:** `_record()` ruft Hooks direkt auf ohne Schutz.

**Codehinweise:**
- Hook-Aufruf ohne Guard【F:src/pyforge/orchestrator.py†L56-L60】

**Risiko:** Ein defekter Hook kann Scheduler/Task-Flow crashen.

**Fix:**
- Hook-Ausnahmen isolieren (pro Hook try/except), Fehler separat auditieren.

---

### 6) Queue-Limit nutzt Domänenfalsche Exception
**Befund:** Queue-Overflow wirft `InvalidTaskStateError` statt passender Kapazitäts-/Backpressure-Exception.

**Codehinweise:**
- Limit-Fehler【F:src/pyforge/orchestrator.py†L93-L95】

**Risiko:** API-Semantik unklar; Clients reagieren falsch.

**Fix:**
- Neue Exception `QueueCapacityExceededError`.

---

## Mittel

### 7) Idempotency-Key-Dedupe kann Payload-Mismatch maskieren
**Befund:** Bei gleichem `idempotency_key` wird bestehender Task zurückgegeben, ohne zu prüfen, ob neue Payload identisch ist.

**Codehinweise:**
- Dedupe-Rückgabe【F:src/pyforge/orchestrator.py†L96-L99】
- Key-Feld im Modell【F:src/pyforge/models.py†L49-L52】

**Risiko:** Stille Datenkonflikte bei fehlerhaftem Client-Retry.

**Fix:**
- Hash der relevanten Felder speichern und bei gleicher ID-Key-Nutzung validieren.
- Bei Konflikt explizite `IdempotencyConflictError`.

---

### 8) Unbegrenztes Speicherwachstum
**Befund:** `audit_log`, `history`, `_agent_duration_ms`, `_result_cache` wachsen unbegrenzt.

**Codehinweise:**
- Strukturen【F:src/pyforge/orchestrator.py†L40-L53】
- Agent-Durations Append【F:src/pyforge/orchestrator.py†L289-L290】

**Risiko:** Memory-Leak-artiges Verhalten bei langen Laufzeiten.

**Fix:**
- Retention-Limits/Ringbuffer.
- Optional persistentes Offloading (DB/TSDB).

---

### 9) Cache-Key zu grob für komplexe Workloads
**Befund:** Cache-Key basiert nur auf `(title, description, required_capability)`.

**Codehinweise:**
- Cache-Key【F:src/pyforge/orchestrator.py†L245-L246】

**Risiko:** Falsche Cache-Treffer (z. B. bei unterschiedlichen Policies, Dependencies, Tags, Agent-Kontext).

**Fix:**
- Signatur erweitern (z. B. Policy-Version, Dependency-Snapshot, Agent-/Model-Version, relevante Task-Optionen).

---

### 10) Testlücken für reale Parallel-/Failure-Szenarien
**Befund:** Tests prüfen Funktionalität, aber nicht deterministisch auf Datenrennen, Hook-Failures, BLOCKED-Recovery, Memory-Retention.

**Codehinweise:**
- vorhandene Testsuite【F:tests/test_orchestrator.py†L39-L164】

**Risiko:** Kritische Produktionsbugs bleiben unentdeckt.

**Fix:**
- Concurrency-Stresstests + flaky-resilient assertions.
- Tests für Hook-Ausnahmeisolierung, BLOCKED->READY-Recovery, failure-metrics.

---

## Fix-Priorisierung (empfohlen)

1. **Sofort (P0):** #1, #2, #3, #4
2. **Kurzfristig (P1):** #5, #6, #7
3. **Mittel (P2):** #8, #9, #10

---

## Fazit

Es gibt keine akuten Syntaxfehler, aber mehrere **architektonische Bug-Risiken**, die unter
Enterprise-Last wahrscheinlich auftreten. Für den nächsten Schritt sollten zuerst
Nebenläufigkeit, BLOCKED-Recovery, Timeout-Isolation und korrekte Failure-Metrics gehärtet werden.
