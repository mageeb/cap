# Persistence instructions

Follow [backend instructions](../AGENTS.md) and read [MEMORY.md](MEMORY.md).

- Enforce reviewer/book scope, atomic create, compare-and-save versions and atomic request ownership in the adapter.
- Preserve source/reviewed text when publishing drafts; reject stale operations and maintain explicit paragraph order.
- Add real adapter integration checks for concurrency, read-after-save and restart durability; do not substitute fake-store results.
