# Translation backend proposal

Status: **Boundaries approved by the user on October 23, 2026**. Book, review and draft
application services are implemented against injected ports. Real input-reader, storage
and translation-provider adapters await user choices. No HTTP host exists yet.

One service and one assembly, organized by capability:

```text
Translation/
  Backend/
    Books/          IBookService: import and read a book with ordered paragraphs
    Reviews/        IReviewService: save a paragraph correction
    Drafts/         IDraftTranslationService: request translation and inspect its status
    Integrations/   ITranslationProvider: external paragraph translation
    Persistence/    IReviewStore: durable book, correction and draft-operation storage
    Contracts/      caller context and explicit success/failure outcomes
  Frontend/         reserved for the future web frontend
```

Future HTTP handlers call the three application services. Those services enforce access
and coordinate the storage/provider ports. Infrastructure implements the ports. Keep
HTTP types, database models and provider SDK types outside these public contracts.
`ISourceDocumentReader` isolates format/size/segmentation policy; its real adapter awaits
the input-format decision. `BookService` validates reader output and submits one ordered
book creation operation without disposing the caller's stream.

## Implemented behavior

`ReviewService` validates the caller, IDs and expected version, then delegates atomic
access enforcement and compare-and-save to the store. Empty reviewed text is permitted.
`DraftTranslationService` checks book access before provider work, honors atomic request
ownership, translates paragraphs in reading order and publishes only a complete draft.
Provider errors are sanitized. Cancellation attempts to record failure and is rethrown.
Cleanup is best effort; a process crash or unavailable storage can leave an operation
pending. Restart recovery and production timeouts are not implemented.

Import streams remain caller-owned. Language selection is not exposed as a product
feature; provider parameters support the Arabic-to-English use case. Get returns the
bounded document; pagination can be added if the approved input limit requires it.
The caller context must come from validated authentication. Authorization semantics
and HTTP mappings still require design; possession of a context is not proof of access.

The draft operation is an orchestration boundary, not a choice of background
infrastructure. Restart recovery, retry rules, partial results and atomicity feasibility
must be resolved before implementation. The proposed store publishes a complete draft
atomically; this is a design for review, not a settled partial-result product policy.
Completion interfaces intentionally await the paragraph/document completion decision.
Accepted formats/size, identity provider, storage engine and translation vendor remain open.

## Verification

The proposal targets locally available .NET 8 to permit compilation. Select a supported
deployment target and SDK policy before adding a runnable host.

```powershell
dotnet build project/src/Translation/Backend/Translation.Backend.csproj
```

Run the dependency-free executable regression suite:

```powershell
dotnet run --project project/src/Translation/Backend/Tests/Translation.Backend.Tests.csproj
```

The suite exits nonzero on failure. It verifies orchestration with deterministic fakes,
not concrete database durability or identity enforcement. Those require adapter tests.
There are no external NuGet packages in either project.

See the [specification](../../../specs/002-backend-skeleton/spec.md),
[task evidence](../../../specs/002-backend-skeleton/tasks.md), and
[confirmed decision DEC-032](../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries).

Current implementation and remaining work: [003 tasks](../../../specs/003-backend-implementation/tasks.md).
