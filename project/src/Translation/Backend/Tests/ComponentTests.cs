using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Drafts;
using Translation.Backend.Integrations;
using Translation.Backend.Persistence;
using Translation.Backend.Reviews;

// Exercises the real application services together. These in-memory boundary doubles
// do not verify a production adapter's authorization, durability or concurrency.
static class ComponentTests
{
    public static IReadOnlyList<(string Name, Func<Task> Run)> Cases { get; } =
    [
        ("Component: import, review and translation preserve paragraph identity", FullWorkflow),
        ("Component: repeating a request returns its terminal operation", DuplicateRequest),
        ("Component: another reviewer cannot read, edit, translate or inspect status", AccessDenied),
        ("Component: failed translation publishes no partial draft", ProviderFailure),
        ("Component: stale review cannot overwrite the saved correction", StaleReview),
        ("Component: cancelled translation preserves edits and records failed status", Cancellation)
    ];

    private static async Task FullWorkflow()
    {
        var app = await Fixture.Create();
        var before = await app.Read();
        Require(before.Book.Title == "Book" && before.Paragraphs.Select(p => p.Position).SequenceEqual([1, 4]), "Import title/order changed.");
        var edited = Value(await app.Reviews.SaveAsync(app.Owner, app.BookId, before.Paragraphs[0].Id, new("human correction", "v1")));
        var operation = Value(await app.Drafts.RequestAsync(app.Owner, app.BookId, "first"));
        Require(operation.State == DraftState.Succeeded, "Translation did not succeed.");
        var after = await app.Read();
        Require(app.Provider.Inputs.Select(p => p.SourceText).SequenceEqual(before.Paragraphs.Select(p => p.SourceText)), "Provider input was not in reading order.");
        Require(app.Provider.Inputs.All(p => p.SourceLanguage == "ar" && p.TargetLanguage == "en"), "Language mapping changed.");
        Require(after.Paragraphs.Select(p => p.Id).SequenceEqual(before.Paragraphs.Select(p => p.Id)), "Paragraph identity/alignment changed.");
        Require(after.Paragraphs.Select(p => p.SourceText).SequenceEqual(before.Paragraphs.Select(p => p.SourceText)), "Source text changed.");
        Require(after.Paragraphs.All(p => p.MachineDraft == "English:" + p.SourceText), "Machine drafts were not aligned with sources.");
        Require(after.Paragraphs[0].ReviewedText == edited.ReviewedText && after.Paragraphs[0].Version == edited.Version, "Translation overwrote the correction/version.");
        Require(after.Paragraphs[1].ReviewedText is null, "Translation invented a review.");
        Require(Value(await app.Drafts.GetStatusAsync(app.Owner, app.BookId, operation.Id)) == operation, "Terminal status was not readable.");
    }

    private static async Task DuplicateRequest()
    {
        var app = await Fixture.Create();
        var first = Value(await app.Drafts.RequestAsync(app.Owner, app.BookId, "same"));
        var calls = app.Provider.Inputs.Count;
        var second = Value(await app.Drafts.RequestAsync(app.Owner, app.BookId, "same"));
        Require(first == second && second.State == DraftState.Succeeded, "Duplicate lost the original terminal operation.");
        Require(app.Provider.Inputs.Count == calls && app.Store.CompleteWrites == 1, "Duplicate performed translation or publication again.");
    }

    private static async Task AccessDenied()
    {
        var app = await Fixture.Create();
        var book = await app.Read();
        var operation = Value(await app.Drafts.RequestAsync(app.Owner, app.BookId, "owner"));
        var intruder = new ReviewerContext("other");
        var beforeCalls = app.Provider.Inputs.Count;
        Denied(await app.Books.GetAsync(intruder, app.BookId));
        Denied(await app.Reviews.SaveAsync(intruder, app.BookId, book.Paragraphs[0].Id, new("stolen edit", "v1")));
        Denied(await app.Drafts.RequestAsync(intruder, app.BookId, "intruder"));
        Denied(await app.Drafts.GetStatusAsync(intruder, app.BookId, operation.Id));
        Require(app.Provider.Inputs.Count == beforeCalls && app.Store.OperationCount == 1, "Denied translation created work.");
        Require((await app.Read()).Paragraphs[0].ReviewedText is null, "Denied review mutated the book.");
    }

    private static async Task ProviderFailure()
    {
        var app = await Fixture.Create();
        var before = await app.Read();
        Value(await app.Reviews.SaveAsync(app.Owner, app.BookId, before.Paragraphs[0].Id, new("keep this", "v1")));
        app.Provider.FailAt = 2;
        var operation = Value(await app.Drafts.RequestAsync(app.Owner, app.BookId, "failure"));
        Require(operation.State == DraftState.Failed && operation.FailureMessage == "Translation is temporarily unavailable.", "Dependency failure was not sanitized.");
        var after = await app.Read();
        Require(after.Paragraphs.All(p => p.MachineDraft is null) && app.Store.CompleteWrites == 0, "Partial drafts were published.");
        Require(after.Paragraphs[0].ReviewedText == "keep this", "Failed translation lost the correction.");
        Require(Value(await app.Drafts.GetStatusAsync(app.Owner, app.BookId, operation.Id)) == operation, "Failure status was not persisted.");
    }

    private static async Task StaleReview()
    {
        var app = await Fixture.Create();
        var paragraph = (await app.Read()).Paragraphs[0];
        var saved = Value(await app.Reviews.SaveAsync(app.Owner, app.BookId, paragraph.Id, new("", paragraph.Version)));
        Require(saved.ReviewedText == "" && saved.Version != paragraph.Version, "Clearing the review failed to advance its version.");
        Require(await app.Reviews.SaveAsync(app.Owner, app.BookId, paragraph.Id, new("stale", paragraph.Version))
            is OperationResult<ParagraphView>.Failure { Code: ErrorCode.Conflict }, "Stale edit was accepted.");
        Require((await app.Read()).Paragraphs[0] == saved, "Stale edit overwrote the saved correction.");
    }

    private static async Task Cancellation()
    {
        var app = await Fixture.Create();
        var paragraph = (await app.Read()).Paragraphs[0];
        var saved = Value(await app.Reviews.SaveAsync(app.Owner, app.BookId, paragraph.Id, new("retain on cancellation", paragraph.Version)));
        using var cancellation = new CancellationTokenSource();
        app.Provider.CancelAt = 2;
        app.Provider.Cancellation = cancellation;
        try
        {
            await app.Drafts.RequestAsync(app.Owner, app.BookId, "cancel", cancellation.Token);
            throw new InvalidOperationException("Cancellation was swallowed.");
        }
        catch (OperationCanceledException) { }
        var after = await app.Read();
        Require(after.Paragraphs.All(p => p.MachineDraft is null), "Cancelled translation published a partial draft.");
        Require(after.Paragraphs[0] == saved, "Cancellation lost the existing correction/version.");
        var operation = app.Store.SingleOperation;
        Require(operation.State == DraftState.Failed && operation.FailureMessage == "Translation was cancelled.", "Cancellation left a pending operation.");
        Require(Value(await app.Drafts.GetStatusAsync(app.Owner, app.BookId, operation.Id)) == operation, "Cancelled status was not readable.");
    }

    private static T Value<T>(OperationResult<T> result) => result is OperationResult<T>.Success success
        ? success.Value : throw new InvalidOperationException($"Expected success; got {result}.");
    private static void Denied<T>(OperationResult<T> result) => Require(result is OperationResult<T>.Failure { Code: ErrorCode.AccessDenied }, "Access denial was not preserved.");
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class Fixture
    {
        public ReviewerContext Owner { get; } = new("owner");
        public MemoryStore Store { get; } = new();
        public Provider Provider { get; } = new();
        public BookService Books { get; }
        public ReviewService Reviews { get; }
        public DraftTranslationService Drafts { get; }
        public Guid BookId { get; private set; }
        private Fixture()
        {
            Books = new(Store, new Reader());
            Reviews = new(Store);
            Drafts = new(Store, Provider);
        }
        public static async Task<Fixture> Create()
        {
            var app = new Fixture();
            using var stream = new MemoryStream([1]);
            app.BookId = Value(await app.Books.ImportAsync(app.Owner, new(" Book ", "source", "test/source", stream))).Id;
            Require(stream.CanRead, "Import disposed the caller stream.");
            return app;
        }
        public async Task<BookReview> Read() => Value(await Books.GetAsync(Owner, BookId));
    }

    private sealed class Reader : ISourceDocumentReader
    {
        public Task<OperationResult<IReadOnlyList<SourceParagraph>>> ReadAsync(ImportBook input, CancellationToken cancellationToken = default) =>
            Task.FromResult<OperationResult<IReadOnlyList<SourceParagraph>>>(new OperationResult<IReadOnlyList<SourceParagraph>>.Success([new(4, "ثان"), new(1, "أول")]));
    }

    private sealed class Provider : ITranslationProvider
    {
        public List<TranslationInput> Inputs { get; } = [];
        public int FailAt { get; set; }
        public int CancelAt { get; set; }
        public CancellationTokenSource? Cancellation { get; set; }
        public Task<OperationResult<string>> TranslateAsync(TranslationInput input, CancellationToken cancellationToken = default)
        {
            Inputs.Add(input);
            if (Inputs.Count == CancelAt) Cancellation?.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<OperationResult<string>>(Inputs.Count == FailAt
                ? new OperationResult<string>.Failure(ErrorCode.DependencyUnavailable, "secret provider credential")
                : new OperationResult<string>.Success("English:" + input.SourceText));
        }
    }

    private sealed class MemoryStore : IReviewStore
    {
        private string? owner;
        private BookReview? book;
        private readonly Dictionary<string, DraftOperation> operations = [];
        public int CompleteWrites { get; private set; }
        public int OperationCount => operations.Count;
        public DraftOperation SingleOperation => operations.Values.Single();
        private bool Allowed(ReviewerContext reviewer, Guid id) => reviewer.SubjectId == owner && book?.Book.Id == id;
        private static Task<OperationResult<T>> Success<T>(T value) => Task.FromResult<OperationResult<T>>(new OperationResult<T>.Success(value));
        private static Task<OperationResult<T>> Failure<T>(ErrorCode code) => Task.FromResult<OperationResult<T>>(new OperationResult<T>.Failure(code, code.ToString()));
        public Task<OperationResult<BookReview>> CreateAsync(ReviewerContext reviewer, NewBook input, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            owner = reviewer.SubjectId;
            book = new(new(Guid.NewGuid(), input.Title), input.Paragraphs.Select(p => new ParagraphView(Guid.NewGuid(), p.Position, p.Text, null, null, "v1")).ToArray());
            return Success(book);
        }
        public Task<OperationResult<BookReview>> GetAsync(ReviewerContext reviewer, Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Allowed(reviewer, id) ? Success(book!) : Failure<BookReview>(ErrorCode.AccessDenied);
        }
        public Task<OperationResult<ParagraphView>> SaveReviewAsync(ReviewerContext reviewer, Guid id, Guid paragraphId, SaveReview review, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Allowed(reviewer, id)) return Failure<ParagraphView>(ErrorCode.AccessDenied);
            var paragraph = book!.Paragraphs.Single(p => p.Id == paragraphId);
            if (paragraph.Version != review.ExpectedVersion) return Failure<ParagraphView>(ErrorCode.Conflict);
            var updated = paragraph with { ReviewedText = review.ReviewedText, Version = Guid.NewGuid().ToString() };
            book = book with { Paragraphs = book.Paragraphs.Select(p => p.Id == paragraphId ? updated : p).ToArray() };
            return Success(updated);
        }
        public Task<OperationResult<BeginDraftResult>> BeginDraftAsync(ReviewerContext reviewer, Guid id, string requestId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Allowed(reviewer, id)) return Failure<BeginDraftResult>(ErrorCode.AccessDenied);
            var created = !operations.TryGetValue(requestId, out var operation);
            if (created) operations.Add(requestId, operation = new(Guid.NewGuid(), DraftState.Pending, null));
            return Success(new BeginDraftResult(operation!, created));
        }
        public Task<OperationResult<DraftOperation>> FinishDraftAsync(ReviewerContext reviewer, Guid id, Guid operationId, DraftOutcome outcome, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Allowed(reviewer, id)) return Failure<DraftOperation>(ErrorCode.AccessDenied);
            var key = operations.Single(p => p.Value.Id == operationId).Key;
            var operation = operations[key];
            if (operation.State != DraftState.Pending) return Failure<DraftOperation>(ErrorCode.Conflict);
            if (outcome is DraftOutcome.Completed complete)
            {
                Require(complete.Paragraphs.Count == book!.Paragraphs.Count && complete.Paragraphs.Select(p => p.ParagraphId).ToHashSet().SetEquals(book.Paragraphs.Select(p => p.Id)), "Incomplete publication was attempted.");
                book = book with { Paragraphs = book.Paragraphs.Select(p => p with { MachineDraft = complete.Paragraphs.Single(d => d.ParagraphId == p.Id).Text }).ToArray() };
                CompleteWrites++;
                operation = operation with { State = DraftState.Succeeded };
            }
            else operation = operation with { State = DraftState.Failed, FailureMessage = ((DraftOutcome.Failed)outcome).Message };
            operations[key] = operation;
            return Success(operation);
        }
        public Task<OperationResult<DraftOperation>> GetDraftAsync(ReviewerContext reviewer, Guid id, Guid operationId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Allowed(reviewer, id)) return Failure<DraftOperation>(ErrorCode.AccessDenied);
            var operation = operations.Values.SingleOrDefault(p => p.Id == operationId);
            return operation is null ? Failure<DraftOperation>(ErrorCode.NotFound) : Success(operation);
        }
    }
}
