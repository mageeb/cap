# Business API tasks

- [x] A-1 (AC-1, AC-2): Map five service routes with fail-closed prerequisite/identity gate.
- [x] A-2 (AC-3, AC-4): Bind transport DTOs and map safe success/failure HTTP responses.
- [x] A-3 (AC-5): Run meaningful loopback HTTP checks with doubles and cancellation.
- [x] A-4: Complete adversarial review, local links and whitespace verification.

Choices: [DEC-039](../../../DECISIONS.md#dec-039-business-http-contracts), pending user review.
Production adapters, identity integration and readiness remain outside this task.

Evidence (2026-10-30): `dotnet run --project
project/src/Translation/Host.ApiTests/Translation.Host.ApiTests.csproj --no-restore`
passed 13/13 grouped HTTP contract checks against actual loopback Kestrel with test identity
and service doubles. Verified five routes, dependency gate before parsing, forged headers,
anonymous/ambiguous subjects, import byte/metadata fidelity and 201 Location, upload stream
closure after the service call, intentional empty review text/version, draft request/status,
all service outcome mappings, malformed JSON/forms/content types, dependency exception
sanitization, invalid GUID routing, and request-disconnect cancellation propagation.

Independent review identified that parsing exceptions caught around service calls could
misclassify service failures as 400. Parsing exceptions are now wrapped only during body
reading, with regressions proving service JsonException/InvalidDataException return 500.
An initial stream-lifetime check used CanRead, which does not reflect disposal for the
framework upload wrapper; the corrected regression verifies ReadByte throws after closure.
Production service wiring, real authentication and adapter integration remain unimplemented.
Independent review accepted the API contracts and independently reproduced all 13 checks.
Integrated container HTTP smoke confirms mapped business route returns503 and cannot be
unblocked by a forged Authorization header. Parent verified safe route-template logging
with a known trace ID and no synthetic credential/query leakage.
