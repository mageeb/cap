# ADR 004: Gated business HTTP contracts

Status: Pending user review; [DEC-039](../../../DECISIONS.md#dec-039-business-http-contracts).

Expose the five existing book/review/draft service operations through Minimal API routes
in the separate host. Keep HTTP DTO binding and safe status mapping outside Backend.
Use multipart import to preserve arbitrary source bytes without selecting a reader format.

Gate requests before body parsing: all business service ports must be registered, then an
unambiguous subject must come from authenticated identity. Current production startup
registers neither real services nor identity middleware, so business requests return 503.
No deployment/configuration flag bypasses this gate. Readiness remains deliberately 503.

Alternative: wire temporary production adapters or accept a reviewer ID from the client.
Those would violate the user's explicit instruction to leave production blocked until
adapters and identity are configured.

Consequence: HTTP binding/status behavior is testable now using doubles and test identity,
but successful production traffic still requires actual adapters and validated identity.
Future cookie authentication requires a deliberate CSRF decision before production access.
See [API contracts](../../src/Translation/Host/API.md) and [006 evidence](../../specs/006-business-api/tasks.md).
