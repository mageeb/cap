# 006: Business HTTP contracts

Implement the approved book/review/draft service boundaries over HTTP. Real adapters,
identity provider and production access remain blocked until selected and configured.

- AC-1: Five routes map import/read/review/request-draft/status to existing service ports,
  passing cancellation and a reviewer derived only from authenticated identity.
- AC-2: Before reading a body or invoking services, return sanitized 503 when any business
  service is unregistered. With services present, missing/ambiguous authenticated subject
  returns 401. Request headers/body cannot establish identity.
- AC-3: Success returns JSON with import 201/Location and other operations 200. Service
  InvalidInput/NotFound/AccessDenied/Conflict/DependencyUnavailable map to 400/404/403/409/503.
- AC-4: Malformed JSON/forms return sanitized 400; unsupported JSON content type returns
  415. Unexpected dependency failures return sanitized 500; caller cancellation propagates.
- AC-5: Real HTTP contract tests use service doubles and explicitly injected test identity;
  verify gate, binding, cancellation, errors, sanitization and Location behavior.

Import uses multipart/form-data with title and one file; this defines HTTP transport only,
not accepted document formats, source-size policy or a concrete reader. No cookie-based
identity is enabled; a future identity adapter must decide CSRF protection before rollout.
