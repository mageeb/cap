# Contracts instructions

Follow [backend instructions](../AGENTS.md) and read [MEMORY.md](MEMORY.md).

- Keep shared types independent of HTTP frameworks, persistence engines and provider SDKs.
- Preserve typed success/failure outcomes; cancellation remains cancellation rather than a business failure.
- Construct reviewer context from validated server identity; possession of context is not proof of book access.
