using Translation.Backend.Contracts;
using Translation.Backend.Integrations;
using Translation.Backend.Persistence;

namespace Translation.Backend.Drafts;

public sealed class DraftTranslationService(
    IReviewStore store, ITranslationProvider provider) : IDraftTranslationService
{
    private const string ProviderFailure = "Translation is temporarily unavailable.";

    public async Task<OperationResult<DraftOperation>> RequestAsync(
        ReviewerContext reviewer, Guid bookId, string RequestId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidReviewer(reviewer))
            return AccessDenied();
        if (bookId == Guid.Empty || string.IsNullOrWhiteSpace(RequestId))
            return InvalidInput();

        // Authorize before creating an operation or sending source text to the provider.
        var bookResult = await store.GetAsync(reviewer, bookId, cancellationToken);
        if (bookResult is OperationResult<Books.BookReview>.Failure bookFailure)
            return new OperationResult<DraftOperation>.Failure(bookFailure.Code, bookFailure.Message);
        var book = ((OperationResult<Books.BookReview>.Success)bookResult).Value;

        var beginResult = await store.BeginDraftAsync(reviewer, bookId, RequestId, cancellationToken);
        if (beginResult is OperationResult<BeginDraftResult>.Failure beginFailure)
            return new OperationResult<DraftOperation>.Failure(beginFailure.Code, beginFailure.Message);
        var begin = ((OperationResult<BeginDraftResult>.Success)beginResult).Value;
        if (!begin.Created)
            return new OperationResult<DraftOperation>.Success(begin.Operation);

        DraftOutcome outcome;
        try
        {
            var drafts = new List<ParagraphDraft>(book.Paragraphs.Count);
            var failed = false;
            foreach (var paragraph in book.Paragraphs.OrderBy(paragraph => paragraph.Position))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await provider.TranslateAsync(
                    new TranslationInput(paragraph.SourceText, "ar", "en"), cancellationToken);
                if (result is not OperationResult<string>.Success translated ||
                    string.IsNullOrWhiteSpace(translated.Value))
                {
                    // Never persist provider-controlled error details or publish a partial draft.
                    failed = true;
                    break;
                }
                drafts.Add(new ParagraphDraft(paragraph.Id, translated.Value));
            }
            outcome = failed
                ? new DraftOutcome.Failed(ProviderFailure)
                : new DraftOutcome.Completed(drafts);
        }
        catch (OperationCanceledException)
        {
            await MarkCancelledAsync(reviewer, bookId, begin.Operation.Id);
            throw;
        }
        catch (Exception)
        {
            outcome = new DraftOutcome.Failed(ProviderFailure);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await store.FinishDraftAsync(
                reviewer, bookId, begin.Operation.Id, outcome, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await MarkCancelledAsync(reviewer, bookId, begin.Operation.Id);
            throw;
        }
    }

    public Task<OperationResult<DraftOperation>> GetStatusAsync(
        ReviewerContext reviewer, Guid bookId, Guid operationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidReviewer(reviewer))
            return Task.FromResult(AccessDenied());
        if (bookId == Guid.Empty || operationId == Guid.Empty)
            return Task.FromResult(InvalidInput());

        // Store lookup enforces reviewer, book and operation scope together.
        return store.GetDraftAsync(reviewer, bookId, operationId, cancellationToken);
    }

    private async Task MarkCancelledAsync(ReviewerContext reviewer, Guid bookId, Guid operationId)
    {
        try
        {
            await store.FinishDraftAsync(reviewer, bookId, operationId,
                new DraftOutcome.Failed("Translation was cancelled."), CancellationToken.None);
        }
        catch (Exception)
        {
            // Cleanup is best effort and must preserve the original cancellation.
        }
    }

    private static bool ValidReviewer(ReviewerContext reviewer) =>
        reviewer is not null && !string.IsNullOrWhiteSpace(reviewer.SubjectId);

    private static OperationResult<DraftOperation> InvalidInput() =>
        new OperationResult<DraftOperation>.Failure(
            ErrorCode.InvalidInput, "A reviewer, book and request or operation ID are required.");

    private static OperationResult<DraftOperation> AccessDenied() =>
        new OperationResult<DraftOperation>.Failure(
            ErrorCode.AccessDenied, "A validated reviewer is required.");
}
