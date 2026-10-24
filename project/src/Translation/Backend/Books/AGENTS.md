# Books instructions

Follow [backend instructions](../AGENTS.md) and read [MEMORY.md](MEMORY.md).

- Keep import orchestration separate from format/size/segmentation policy in ISourceDocumentReader.
- Preserve caller stream ownership, source text, stable identity and explicit paragraph order.
- Test invalid input, reader failure, ordering and access-failure propagation before handing off changes.
