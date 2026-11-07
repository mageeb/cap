# Translation review frontend

Local, dependency-free JavaScript review workspace for seeded Arabic/English documents.
Requires Node.js 22 or later. No installation or build step.

## Run

From this folder:

```powershell
node server.mjs
```

Open http://127.0.0.1:4173/?preview=1 for the sample. Preview edits stay on the page;
saving is disabled and a refresh discards edits. It is not database persistence.

To connect a backend, start a new server process with its origin configured:

```powershell
$env:BACKEND_URL = 'http://127.0.0.1:5000'
node server.mjs
```

Use the actual backend origin, then enter a seeded document UUID or open
`http://127.0.0.1:4173/?book=YOUR-DOCUMENT-UUID`.
Set `PORT` if 4173 is occupied. The Node server binds only to loopback and proxies
the two supported API routes; browser CORS configuration is unnecessary.
The backend URL is server configuration, never supplied by the page.
This exercise includes no authentication or credential forwarding.

## API contract

- GET `/api/books/{bookId}` returns
  `{book:{id,title},paragraphs:[{id,position,sourceText,machineDraft,reviewedText,version}]}`.
- PUT `/api/books/{bookId}/paragraphs/{paragraphId}/review` accepts
  `{reviewedText,expectedVersion}` and returns the updated paragraph.
- HTTP 409 preserves local edits and retrieves the latest paragraph for comparison.
  Choose Use saved version or Keep my edit; keeping an edit still requires explicit Save.
- Empty saved corrections take precedence over machine drafts.
- Ctrl/Cmd+S saves the focused paragraph, or all changes if focus is elsewhere.
  Alt+Up/Down moves between paragraph editors. Unsaved navigation warns before discard.

## Current integration limit

The root checkout has application services but no HTTP host. The separate deployment
worktree defines these routes but currently registers no business adapters (503) and
requires authenticated identity once adapters exist (401). Real database demonstration
still requires a seeded persistence adapter and an explicitly configured demo identity
in the backend. This frontend does not bypass backend identity checks.

## Checks

```powershell
node --check app.js
node --check server.mjs
node --test tests/review.test.mjs
```

Tests cover empty corrections, in-flight typing, failure retention, and real HTTP
read/save/reload/conflict through a test-only in-memory backend. They do not establish
database durability. Browser verification covers the sample's layout and editing;
see [feature evidence](../../../specs/004-frontend-review/tasks.md).

## DEV deployment connection

Start the frontend against the recorded AKS DEV tunnel:

```powershell
node start-dev.mjs
```

This reads dev.config.json: frontend port 4174, backend http://127.0.0.1:18081.
BACKEND_URL and PORT can override those settings. The page displays the configured
API target; that label indicates configuration, not backend readiness.
Keep the existing AKS port-forward running. The deployment's Start-PortForward.ps1
script is under C:/Repos/scratch-space/deploy/translation-dev.

Open http://127.0.0.1:4174/?book=a784c44a-bb58-20a5-63fb-f8cc47a9d5dc
for the deterministic Romans sample ID from the deployment import script.

Verified 2026-11-06: tunnel liveness 200; readiness 503; the seeded-book read through
the frontend proxy returns 503 business_dependencies_not_configured. This confirms
routing to DEV, but reading and saving remain blocked by the backend dependency gate.
