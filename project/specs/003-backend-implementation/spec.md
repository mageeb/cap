# 003: Backend service implementation

Status: In progress; real adapter choices awaiting user input.

Implement the approved application boundaries without HTTP/vendor coupling.

- AC-1: Save validates caller, resource identifiers and version, preserves store conflict/access outcomes and propagates cancellation.
- AC-2: Draft requests check book access before provider work; duplicate request keys invoke no additional translation work.
- AC-3: Successful translation covers every ordered paragraph and publishes one complete result. Blank output, provider failure or cancellation never publish partial drafts or overwrite reviewer corrections.
- AC-4: Failures expose sanitized dependency messages; cancellation remains cancellation and attempts to mark the operation failed.
- AC-5: Import validates the agreed format/size, preserves caller stream ownership and produces ordered source paragraphs. Input/storage/provider choices await the pending user response.
- AC-6: Meaningful automated checks verify happy paths and failure/access/conflict boundaries. Real persistence and provider tests require selected adapters.

The [approved skeleton](../002-backend-skeleton/spec.md) remains the interface-review record.
