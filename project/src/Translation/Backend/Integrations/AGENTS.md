# Integrations instructions

Follow [backend instructions](../AGENTS.md) and read [MEMORY.md](MEMORY.md).

- Keep vendor SDKs, configuration and credentials inside adapters; never expose secrets through outcomes.
- Preserve one source-to-target paragraph mapping and cancellation; leave persistence and reviewer authorization to application/store boundaries.
- Verify configured provider failures and malformed/blank responses; fakes do not prove real integration.
