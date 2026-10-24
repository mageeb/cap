# Translation backend boundaries

Status: Accepted by the user on October 23, 2026. See [DEC-032](../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries).

Use one capability-organized backend assembly with application services and narrow
integration/storage ports. This keeps the review focused on responsibilities and avoids
multiple layer projects before there is implementation complexity to justify them.
Alternative: separate domain/application/infrastructure/API assemblies now.
Consequence: folder boundaries rely on review rather than assembly-level enforcement.

[Interface proposal](../../src/Translation/Backend/README.md).
