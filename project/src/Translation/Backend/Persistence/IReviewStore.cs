using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Drafts;
using Translation.Backend.Reviews;

namespace Translation.Backend.Persistence;

// Application-specific persistence port. Implementations are internal infrastructure,
// not public endpoints. Every operation is scoped by a validated reviewer identity.
public interface IReviewStore
{
    // Atomically creates the book and its ordered source paragraphs, or creates neither.
    Task<OperationResult<BookReview>> CreateAsync(
        ReviewerContext reviewer, NewBook book, CancellationToken cancellationToken = default);

    Task<OperationResult<BookReview>> GetAsync(
        ReviewerContext reviewer, Guid bookId, CancellationToken cancellationToken = default);

    Task<OperationResult<ParagraphView>> SaveReviewAsync(
        ReviewerContext reviewer, Guid bookId, Guid paragraphId, SaveReview review,
        CancellationToken cancellationToken = default);

    // Atomically records one logical request per book/request key; a repeat returns the same operation.
    // Only the caller receiving Created=true may invoke the provider for this request.
    Task<OperationResult<BeginDraftResult>> BeginDraftAsync(
        ReviewerContext reviewer, Guid bookId, string requestId,
        CancellationToken cancellationToken = default);

    // Saves draft results plus terminal state atomically, preserving source/reviewed text.
    // Must reject stale operations; failure/retry policy remains subject to product review.
    Task<OperationResult<DraftOperation>> FinishDraftAsync(
        ReviewerContext reviewer, Guid bookId, Guid operationId, DraftOutcome outcome,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DraftOperation>> GetDraftAsync(
        ReviewerContext reviewer, Guid bookId, Guid operationId,
        CancellationToken cancellationToken = default);
}

public sealed record NewBook(string Title, IReadOnlyList<SourceParagraph> Paragraphs);
public sealed record SourceParagraph(int Position, string Text);
public sealed record ParagraphDraft(Guid ParagraphId, string Text);
public sealed record BeginDraftResult(DraftOperation Operation, bool Created);
public abstract record DraftOutcome
{
    private DraftOutcome() { }
    public sealed record Completed(IReadOnlyList<ParagraphDraft> Paragraphs) : DraftOutcome;
    public sealed record Failed(string Message) : DraftOutcome;
}
