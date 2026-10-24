# Reviews instructions

Follow [backend instructions](../AGENTS.md) and read [MEMORY.md](MEMORY.md).

- Validate reviewer/resource/version inputs; preserve atomic store conflict and access outcomes.
- Allow intentional empty reviewed text under current policy; never silently replace source or machine draft.
- Test validation, empty edits, stale versions and access failures when changing save behavior.
