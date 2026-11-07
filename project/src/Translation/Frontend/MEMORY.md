# Frontend rationale

## 2026-10-23: Reserved frontend boundary

- **Choice / why:** A sibling Frontend folder preserves the user-selected application structure while work focuses on the C# backend.
- **Status:** Folder separation confirmed by the user; framework and detailed UX remain undecided.
- **Evidence:** [Backend overview](../Backend/README.md); [DEC-032](../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries).
- **Limits:** No frontend implementation or browser verification exists yet.

## 2026-11-06: Seeded-document review workspace

- **Choice / why:** Native browser modules and a local Node API proxy keep the initial
  Arabic/English review exercise small and runnable without dependency installation.
- **Scope:** Known document read, aligned editing, explicit saves, conflict handling,
  unsaved navigation protection and keyboard navigation. No upload, translation or auth UI.
- **Status:** Implemented locally; technical choices pending user review in
  [DEC-042](../../../../DECISIONS.md#dec-042-basic-local-review-frontend).
- **Evidence:** [Feature tasks](../../../specs/004-frontend-review/tasks.md),
  [run instructions](README.md), six passing Node tests and browser fixture save/reload.
- **Limits:** Sample preview never persists. Real seeded database workflow still needs
  backend business adapters and demo identity configuration. Unfinished pre-existing
  repository merge prevented creating a feature branch or committing this work.

## 2026-11-06: Connect the recorded DEV tunnel

- **User direction:** Connect the existing DEV deployment via its tunnel.
- **Implementation:** dev.config.json and start-dev.mjs target the recorded AKS
  port-forward http://127.0.0.1:18081; frontend runs on4174. The UI shows the configured
  target without claiming readiness. Environment variables override the defaults.
- **Evidence:** Live liveness200; readiness503; seeded document request through frontend
  returns503 business_dependencies_not_configured. Six regression tests pass.
- **Limit:** Tunnel routing verified, but real read/save remains blocked by DEV dependencies.
