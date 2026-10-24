using Translation.Backend.Contracts;

namespace Translation.Backend.Books;

public interface IBookService
{
    // Validates agreed format/size, segments input and durably creates ordered paragraphs.
    // The caller retains ownership of Content and disposes it after this call completes.
    Task<OperationResult<BookSummary>> ImportAsync(
        ReviewerContext reviewer, ImportBook request, CancellationToken cancellationToken = default);

    // Must enforce book access; paragraphs have stable IDs and explicit reading order.
    Task<OperationResult<BookReview>> GetAsync(
        ReviewerContext reviewer, Guid bookId, CancellationToken cancellationToken = default);
}

public sealed record ImportBook(string Title, string FileName, string MediaType, Stream Content);
public sealed record BookSummary(Guid Id, string Title);
public sealed record BookReview(BookSummary Book, IReadOnlyList<ParagraphView> Paragraphs);
public sealed record ParagraphView(
    Guid Id, int Position, string SourceText, string? MachineDraft,
    string? ReviewedText, string Version);
