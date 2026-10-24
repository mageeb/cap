using Translation.Backend.Contracts;
using Translation.Backend.Persistence;

namespace Translation.Backend.Books;

// Implement after format, size and segmentation rules are agreed. Never dispose Content.
public interface ISourceDocumentReader
{
    Task<OperationResult<IReadOnlyList<SourceParagraph>>> ReadAsync(
        ImportBook input, CancellationToken cancellationToken = default);
}
