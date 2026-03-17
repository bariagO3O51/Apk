# PyForge – Enterprise-Planung (Python Multi-Agent Orchestrator)

## 1) Produktvision

**PyForge** ist eine Python-Orchestrator-Plattform, die mehrere AI-Agenten (z. B. Codex CLI, Blackbox AI CLI/API und weitere Tools) in einem gemeinsamen, kontrollierten Entwicklungsprozess koordiniert. Ziel ist ein reproduzierbarer, sicherer und auditierbarer Workflow für Softwareentwicklung in Teams und Unternehmen.

**Kernnutzen:**
- Aufgaben parallel an spezialisierte Agenten verteilen.
- Gemeinsamen Projektkontext verwalten (Code, Anforderungen, Entscheidungen, Logs).
- Änderungen kontrolliert integrieren (Branches, Reviews, Policy-Gates).
- Qualität, Sicherheit und Compliance im Prozess verankern.

---

## 2) Zielgruppen & Haupt-Use-Cases

### Zielgruppen
- Enterprise-Engineering-Teams mit komplexen Repositories.
- Plattform-/DevEx-Teams, die AI-Workflows standardisieren wollen.
- Tech Leads/Architekten, die Governance und Qualität sicherstellen müssen.

### Use-Cases
1. **Feature Delivery:** Story in Subtasks splitten, parallel durch Agenten umsetzen, konsolidieren.
2. **Refactoring-Wellen:** Große Umstellungen (API, Naming, Architektur) automatisiert koordinieren.
3. **Test-/Quality-Automation:** Agenten generieren Tests, verbessern Coverage, fixen Flaky Tests.
4. **Security & Compliance:** SAST/Lint/Policy-Checks als verpflichtende Gates.
5. **Legacy-Modernisierung:** Schrittweise Migration mit nachvollziehbaren Entscheidungen.

---

## 3) Produktanforderungen (MVP → Enterprise)

## MVP (Phase 1)
- Projekt- und Agenten-Registry (z. B. `codex`, `blackbox`, `local_llm`).
- Task-Orchestrierung mit klaren Zuständen (queued/running/review/done/failed).
- Gemeinsamer Kontextspeicher (Datei-Metadaten, Entscheidungen, Task-Historie).
- Git-Integration: pro Task eigener Branch, strukturierte Commit Messages.
- Baseline-Checks: Lint + Tests + Security-Scan als Gate vor Merge.
- CLI + REST API für Automatisierung.

## Phase 2
- Graph-basierte Workflow-Engine (DAG, Abhängigkeiten, Retry-Strategien).
- Role-based Policies (wer darf was, welche Agenten dürfen schreiben/mergen).
- Prompt-/Template-Management mit Versionierung.
- Konfliktmanagement bei konkurrierenden Codeänderungen.
- Human-in-the-loop Review UI.

## Phase 3 (Enterprise)
- Multi-Repo-/Monorepo-Orchestrierung.
- SSO/OIDC, RBAC/ABAC, Audit-Trails, Data Residency Controls.
- Kosten-/Token-Budgeting pro Team/Projekt/Agent.
- Observability Suite (Tracing, Metrics, Agent-Effektivität).
- Plugin/Adapter-Ökosystem für weitere Agenten und Provider.

---

## 4) Architekturvorschlag

## High-Level Komponenten
1. **Orchestrator Core**
   - Zustandsmaschine für Tasks/Workflows.
   - Scheduling, Priorisierung, Retry, Timeout.
2. **Agent Adapter Layer**
   - Einheitliches Interface für CLI/API-gebundene Agenten.
   - Capability-Matrix (Codegen, Analyse, Testfix, Doku, Security).
3. **Context & Knowledge Store**
   - Projektzustand, Artefakte, Summaries, Entscheidungen.
   - Optional Vektorsuche für Long-Context-Retrieval.
4. **Git/SCM Service**
   - Branching, Commits, PR-Erstellung, Diff-Analyse.
   - Merge-Strategien und Konfliktauflösung.
5. **Policy & Guardrails Engine**
   - Regeln (z. B. keine Secrets, keine unreviewten Merges in Main).
   - Quality Gates (Tests, Lint, Security).
6. **Execution Sandbox**
   - Isolierte Ausführung pro Agent-Task (Container/VM).
   - Resource-Limits (CPU/RAM/Time/Network).
7. **Interface Layer**
   - CLI für Entwickler.
   - REST/gRPC API für Integrationen.
   - Optional Web UI für Monitoring/Reviews.

### Beispieltechnologie (Python)
- **Backend:** FastAPI + Pydantic + Uvicorn
- **Queue/Orchestrierung:** Celery/RQ oder Temporal Python SDK
- **State DB:** PostgreSQL
- **Cache/Queue:** Redis
- **Events:** NATS/Kafka (später)
- **Sandbox:** Docker + seccomp/AppArmor (oder K8s Jobs)
- **Observability:** OpenTelemetry + Prometheus + Grafana

---

## 5) Domänenmodell (vereinfachter Entwurf)

### Entitäten
- `Project(id, name, repo_url, default_branch, policy_set)`
- `Agent(id, type, capabilities, config, status)`
- `Task(id, project_id, title, description, priority, state, assignee_agent_id)`
- `Workflow(id, project_id, dag_spec, state)`
- `ContextItem(id, project_id, scope, content, source, embedding_ref)`
- `CodeChange(id, task_id, branch, commit_sha, diff_summary, checks_status)`
- `Review(id, code_change_id, reviewer, verdict, comments)`
- `AuditEvent(id, actor, action, target, timestamp, metadata)`

### Statusmodell Task
`queued -> running -> awaiting_review -> approved -> merged -> done`

Fehlerpfade:
- `running -> failed`
- `awaiting_review -> changes_requested -> running`

---

## 6) Workflow-Design (Beispiel)

1. Produktanforderung wird als Epic/Task angelegt.
2. Planner-Agent zerlegt in Subtasks (Implementierung, Tests, Doku).
3. Orchestrator weist Subtasks passenden Agenten zu.
4. Jeder Agent arbeitet in isoliertem Branch/Workspace.
5. Automatische Checks laufen nach jedem Commit.
6. Reviewer-Agent + Mensch prüfen Diffs und Risiken.
7. Bei Erfolg: Merge + Changelog + Kontext-Update.
8. Lernschleife: Outcome-Metriken fließen in zukünftiges Scheduling.

---

## 7) Sicherheits- und Compliance-Rahmen

- **Least Privilege:** Agenten erhalten nur minimale Repo-/API-Rechte.
- **Secret Management:** Keine Secrets in Prompts/Logs; Vault-Integration.
- **Prompt/Output Filtering:** PII/Secrets/IP-Filter vor Persistenz.
- **Immutable Audit Logs:** Jede Aktion nachvollziehbar und signierbar.
- **Policy-as-Code:** Merge-Regeln, Testpflicht, Lizenz- und Security-Gates.
- **Data Governance:** Retention, Löschkonzepte, Mandantentrennung.

---

## 8) Betriebsmodell (Enterprise)

- **Deployment-Optionen:** On-Prem, Private Cloud, Hybrid.
- **Skalierung:** Horizontale Worker-Skalierung je Agent-Typ.
- **SLOs:**
  - Task-Start-Latenz < 30s (P95)
  - Merge-Lead-Time -20% ggü. Baseline
  - Fehlerrate automatischer Änderungen < 5%
- **Disaster Recovery:** DB Backups, Queue Replay, Idempotente Jobs.

---

## 9) KPI-Framework

- Durchlaufzeit pro Task (Lead/Cycle Time)
- Erfolgsquote von Agent-PRs
- Review-Aufwand (Mensch vs. automatisch)
- Rework-Rate nach Merge
- Defect-Escape-Rate
- Kosten pro erfolgreich abgeschlossenem Task

---

## 10) Roadmap (90 Tage)

### Tag 0–30 (Foundation)
- Repositoriumstruktur, Core-Datenmodell, Agent-Interface.
- Codex- und Blackbox-Adapter (MVP-Funktionen).
- Task-Lifecycle + Git-Branch-Automation.

### Tag 31–60 (Execution)
- Policy-Gates + CI-Checks Integration.
- Kontextspeicher inkl. Entscheidungsprotokoll.
- Erste End-to-End-Workflows in Pilotprojekt.

### Tag 61–90 (Enterprise Readiness)
- Audit-Logging, RBAC, Kostenkontrolle.
- Observability-Dashboard.
- Betriebsdokumentation und Security Review.

---

## 11) Vorschlag Repository-Struktur

```text
pyforge/
  apps/
    api/                 # FastAPI Endpunkte
    cli/                 # Typer-basierte CLI
  core/
    orchestration/       # Workflow Engine, Scheduler
    agents/              # Agent Interfaces + Adapter
    context/             # Kontextspeicher und Retrieval
    policy/              # Guardrails, Regeln, Gates
    scm/                 # Git/PR Integrationen
  workers/
    task_worker/
    check_worker/
  infra/
    docker/
    k8s/
  docs/
    architecture/
    runbooks/
  tests/
    unit/
    integration/
    e2e/
```

---

## 12) Risiken & Gegenmaßnahmen

1. **Agenten-Inkonsistenz** (nicht deterministische Ergebnisse)
   - Gegenmaßnahme: deterministische Pipelines + Wiederholbarkeit + Review-Gates.
2. **Merge-Konflikte bei Parallelisierung**
   - Gegenmaßnahme: feingranulare Tasks + häufige Rebase-/Sync-Strategien.
3. **Kostenexplosion bei LLM-Nutzung**
   - Gegenmaßnahme: Budget-Limits, Routing nach Task-Komplexität.
4. **Sicherheits-/Compliance-Verstöße**
   - Gegenmaßnahme: Policy-as-Code + Audit + isolierte Laufzeitumgebung.

---

## 13) Nächste konkrete Schritte

1. **Scope fixieren:** 2–3 Pilot-Use-Cases und Erfolgskriterien definieren.
2. **MVP aufsetzen:** Core Orchestrator + 2 Agent-Adapter + Git-Workflow.
3. **Governance früh integrieren:** Policies, Audit, Security-Checkpoints von Anfang an.
4. **Pilot durchführen:** In einem realen Repo messen und iterativ verbessern.

