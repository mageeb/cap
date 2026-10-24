using Translation.Backend.Books;
using Translation.Backend.Contracts;

namespace Translation.Backend.Reviews;

public interface IReviewService
{
    // Checks operation and book access. A stale version returns Conflict without overwriting.
    // Success means durable persistence; a subsequent authorized read sees this edit.
    Task<OperationResult<ParagraphView>> SaveAsync(
        ReviewerContext reviewer, Guid bookId, Guid paragraphId,
        SaveReview request, CancellationToken cancellationToken = default);
}

public sealed record SaveReview(string ReviewedText, string ExpectedVersion);
