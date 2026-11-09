# 007: Restore DEV document loading and saving

Root cause: the deployed host has neither registered application services nor storage
adapters. A global service gate requires even the unused translation service; readiness
is hard-coded false. MongoDB provisioning alone does not wire application dependencies.

The user explicitly approved a private, tunnel-accessed DEV sample reviewer on 2026-11-06.

- AC-1: Explicit DEV-only configuration wires the existing MongoDB schema for book reads
  and version-checked review saves, scoped to the approved sample owner.
- AC-2: Missing configuration retains default 503/identity protection. Invalid DEV tier,
  owner or TLS settings fail closed without leaking credentials. No request header sets identity.
- AC-3: Read/save routes require their own services, not an unconfigured translation provider.
  Import and draft generation remain unavailable; readiness reports supported capabilities.
- AC-4: MongoDB TLS verifies the private CA, server name and validity. Credentials come
  from existing Kubernetes Secrets; mount only public CA material into the app.
- AC-5: Actual DEV readiness/read/save/reload succeed; stale writes return409; source/draft
  and original files remain intact. Validate saved edits across process replacement where possible.

No production authentication or new cloud resource is introduced.
