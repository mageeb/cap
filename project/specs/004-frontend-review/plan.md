# Implementation plan

Use native browser modules and CSS with a small Node loopback static server/API proxy.
Keep frontend state, HTTP requests and rendering separate without a framework dependency.
Selected API contracts come from Host/API.md in the deployment worktree.

Tasks (each estimated 2–5 minutes including its focused verification):
1. F01: index.html and styles.css — aligned desktop/mobile workspace (AC-2, AC-5).
2. F02: review.js — edit state and read/save client; state regression checks (AC-1–4).
3. F03: app.js — load/render/edit/save and navigation; browser sample check (AC-1–5).
4. F04: app.js — conflict comparison and explicit resolution; failure review (AC-4).
5. F05: server.mjs — loopback server and fixed-origin proxy; HTTP checks (AC-1, AC-3, AC-6).
6. F06: sample.js — clearly labeled sample; verify save remains disabled (AC-6).
7. F07: tests/review.test.mjs — read/save/reload/conflict fixture regression (AC-2–4).
8. F08: README, MEMORY, feature records — run instructions and integration limits.
9. F09: independent correctness/refuter review, actual evidence and handoff.

No backend implementation or deployment belongs to this change.
