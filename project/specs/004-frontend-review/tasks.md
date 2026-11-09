# Tasks and evidence

- [x] F01: Responsive workspace markup and styles.
- [x] F02: API client and state; six Node tests pass.
- [x] F03: Document loading, editor and explicit saving implemented.
- [x] F04: Conflict comparison and explicit resolution implemented.
- [x] F05: Loopback static server and API proxy; real HTTP fixture passes.
- [x] F06: Labeled sample preview with saving disabled.
- [x] F07: Six regression tests pass with Node 24.22.1.
- [x] F08: Setup instructions, scope and backend limitations recorded.
- [x] F09: Browser edit/save/reload and keyboard navigation passed against an HTTP fixture; independent re-review ACCEPT after fixing loading edit-loss and split Unicode handling.

Checks: node --check app.js; node --check server.mjs; node --test tests/review.test.mjs
(all pass). Browser loaded sample and screenshot confirms aligned Arabic/English layout.
HTTP GET / returned 200 after local network permission was granted.
Tests use a test-only in-memory service; real database durability remains unverified.

Decision: [DEC-042](../../../DECISIONS.md#dec-042-basic-local-review-frontend).
Branch: feature/week8-session-materials, pre-existing merge in progress.
Attempt to create feature/frontend-review-mvp was rejected by Git because of the merge.
Preserved the merge and unrelated staged changes. No stage, commit, merge abort or publish.
DECISIONS.md already contains unresolved conflicts; only append the new entry.

Next: integrate a seeded backend with demo identity. Narrow-screen browser QA and real database acceptance remain unverified.
After the owner resolves the pre-existing merge, put this feature on its own branch and
commit only frontend/feature documentation changes.

## DEV tunnel follow-up (2026-11-06)

- [x] D01 (2–5 min, AC-1/6): Read deployment records and restore the existing
  localhost:18081 -> AKS translation-backend:8080 port-forward.
- [x] D02 (2–5 min, AC-1/6): Add dev.config.json/start-dev.mjs, dev script, and
  configured API target caption. No browser storage or fixture fallback.
- [x] D03 (2–5 min, AC-1/6): Verify frontend4174 -> tunnel18081 -> real DEV API.
  /connection.json returns configured=true and backendOrigin=http://127.0.0.1:18081.
  GET seeded Romans document a784c44a-bb58-20a5-63fb-f8cc47a9d5dc returns
  HTTP503 code=business_dependencies_not_configured, matching direct DEV response.
  Liveness is 200 and readiness is 503. Six Node regressions pass.
- [ ] Real DEV read/save acceptance: blocked by backend business dependency registration.

The user explicitly requested connecting the DEV deployment through the tunnel.
No readiness bypass, backend security change, seed write or cloud deployment occurred.
The tunnel and frontend are running for use during this session.

Browser DEV tab confirmed API target18081 and real backend error. Independent follow-up review ACCEPT. Existing sample edits preserved in their original tab.
