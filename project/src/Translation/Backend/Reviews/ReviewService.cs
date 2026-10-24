using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Persistence;

namespace Translation.Backend.Reviews;

public sealed class ReviewService(IReviewStore store) : IReviewService
{
    public Task<OperationResult<ParagraphView>> SaveAsync(
        ReviewerContext reviewer, Guid bookId, Guid paragraphId,
        SaveReview request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (reviewer is null || string.IsNullOrWhiteSpace(reviewer.SubjectId))
            return Task.FromResult<OperationResult<ParagraphView>>(
                new OperationResult<ParagraphView>.Failure(ErrorCode.AccessDenied, "A validated reviewer is required."));
        if (bookId == Guid.Empty || paragraphId == Guid.Empty || request is null ||
            request.ReviewedText is null || string.IsNullOrWhiteSpace(request.ExpectedVersion))
            return Task.FromResult<OperationResult<ParagraphView>>(
                new OperationResult<ParagraphView>.Failure(ErrorCode.InvalidInput, "Book, paragraph, text and expected version are required."));

        // Empty text is an intentional edit. The store enforces ownership and compare-and-save atomically.
        return store.SaveReviewAsync(reviewer, bookId, paragraphId, request, cancellationToken);
    }
}
