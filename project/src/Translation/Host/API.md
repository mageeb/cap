# Business HTTP contracts

These routes are implemented, but the production host registers no concrete business
services or authentication provider. All business routes therefore return sanitized 503
before reading the request body. Health readiness remains 503. There is no bypass switch.
With all three service ports registered, the caller still needs an authenticated principal
with one unambiguous `sub` or name-identifier claim. Raw headers/body IDs are never identity.
A future trusted authentication adapter must validate issuer/audience/signature and select
CSRF protection if cookie-based identity is introduced. Backend services enforce resource access.

| Method | Route | Request | Success |
|---|---|---|---|
| POST | `/api/books` | Multipart form: one `title` field and one `file` upload | 201 BookSummary, Location to book |
| GET | `/api/books/{bookId}` | None | 200 BookReview |
| PUT | `/api/books/{bookId}/paragraphs/{paragraphId}/review` | JSON `reviewedText`, `expectedVersion` | 200 ParagraphView |
| POST | `/api/books/{bookId}/drafts` | JSON `requestId` | 200 DraftOperation |
| GET | `/api/books/{bookId}/drafts/{operationId}` | None | 200 DraftOperation |

IDs use GUID syntax. Draft requests await the existing synchronous orchestration contract;
200 does not imply a queue or a new background worker. Import preserves source bytes,
filename and media type, and closes its upload stream after the service completes. The
HTTP layer does not select accepted source formats or segmentation; a real reader must
implement the later approved policy. ASP.NET Core transport/form limits still apply.
Intentional empty reviewed text is valid; expectedVersion and requestId must be nonblank.

Service failures map InvalidInput/NotFound/AccessDenied/Conflict/DependencyUnavailable to
400/404/403/409/503. Problems expose stable codes instead of dependency messages; unexpected
exceptions return 500. Missing identity returns 401 after service prerequisites exist.
Malformed JSON/forms return 400; unsupported JSON or multipart content type returns 415.
Cancellation follows request disconnects and propagates to services. No payloads, caller
IDs, filenames or dependency exception text are deliberately logged by this API layer.

Run real HTTP contract checks from the worktree root:

```powershell
dotnet run --project project/src/Translation/Host.ApiTests/Translation.Host.ApiTests.csproj
```

The test-only host injects identity directly and registers doubles. These checks do not
verify a real identity provider, adapter durability or external translation. Production
host telemetry is verified separately by Host.Tests.

See [006 specification](../../../specs/006-business-api/spec.md),
[task evidence](../../../specs/006-business-api/tasks.md), and
[DEC-039](../../../../DECISIONS.md#dec-039-business-http-contracts).
