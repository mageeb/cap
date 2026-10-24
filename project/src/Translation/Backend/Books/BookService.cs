using Translation.Backend.Contracts;
using Translation.Backend.Persistence;

namespace Translation.Backend.Books;

public sealed class BookService(IReviewStore store, ISourceDocumentReader reader) : IBookService
{
    public async Task<OperationResult<BookSummary>> ImportAsync(
        ReviewerContext reviewer, ImportBook request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidReviewer(reviewer))
            return new OperationResult<BookSummary>.Failure(ErrorCode.AccessDenied, "A validated reviewer is required.");
        if (request is null || string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.MediaType) ||
            request.Content is null || !request.Content.CanRead)
            return new OperationResult<BookSummary>.Failure(ErrorCode.InvalidInput, "Title and readable source input are required.");

        var read = await reader.ReadAsync(request, cancellationToken);
        if (read is OperationResult<IReadOnlyList<SourceParagraph>>.Failure error)
            return new OperationResult<BookSummary>.Failure(error.Code, error.Message);
        var paragraphs = ((OperationResult<IReadOnlyList<SourceParagraph>>.Success)read).Value;
        if (paragraphs.Count == 0 || paragraphs.Any(p => p is null || p.Position < 0 || string.IsNullOrWhiteSpace(p.Text)) ||
            paragraphs.Select(p => p.Position).Distinct().Count() != paragraphs.Count)
            return new OperationResult<BookSummary>.Failure(ErrorCode.InvalidInput, "Source must contain nonempty paragraphs with distinct nonnegative positions.");

        cancellationToken.ThrowIfCancellationRequested();
        var created = await store.CreateAsync(reviewer,
            new NewBook(request.Title.Trim(), paragraphs.OrderBy(p => p.Position).ToArray()), cancellationToken);
        return created switch
        {
            OperationResult<BookReview>.Success success => new OperationResult<BookSummary>.Success(success.Value.Book),
            OperationResult<BookReview>.Failure failure => new OperationResult<BookSummary>.Failure(failure.Code, failure.Message),
            _ => throw new InvalidOperationException("Unrecognized creation result.")
        };
    }

    public Task<OperationResult<BookReview>> GetAsync(
        ReviewerContext reviewer, Guid bookId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidReviewer(reviewer))
            return Task.FromResult<OperationResult<BookReview>>(new OperationResult<BookReview>.Failure(
                ErrorCode.AccessDenied, "A validated reviewer is required."));
        if (bookId == Guid.Empty)
            return Task.FromResult<OperationResult<BookReview>>(new OperationResult<BookReview>.Failure(
                ErrorCode.InvalidInput, "A book ID is required."));
        return store.GetAsync(reviewer, bookId, cancellationToken);
    }

    private static bool ValidReviewer(ReviewerContext reviewer) =>
        reviewer is not null && !string.IsNullOrWhiteSpace(reviewer.SubjectId);
}
