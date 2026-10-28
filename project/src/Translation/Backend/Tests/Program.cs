using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Drafts;
using Translation.Backend.Integrations;
using Translation.Backend.Persistence;
using Translation.Backend.Reviews;

var tests = new (string Name, Func<Task> Run)[]
{
    ("Review validation prevents writes", ReviewValidation),
    ("Review allows clearing text and propagates conflicts", ReviewConflict),
    ("Draft access denial prevents provider and operation calls", DraftAccess),
    ("Repeated request does not invoke provider", DraftDuplicate),
    ("Successful draft preserves paragraph alignment and language", DraftSuccess),
    ("Provider failure does not persist partial drafts", DraftFailure),
    ("Blank provider output fails operation", DraftBlank),
    ("Cancellation propagates and marks operation failed", DraftCancellation),
    ("Draft validation prevents store access", DraftValidation),
    ("Status preserves book and reviewer scope", DraftStatus),
    ("Provider exceptions are sanitized", DraftException),
    ("Cancellation during finish propagates", DraftFinishCancellation),
    ("Import orders paragraphs and retains caller stream", BookImport),
    ("Import reader failure prevents writes", BookReaderFailure),
    ("Import rejects invalid paragraph sets", BookInvalidParagraphs),
    ("Import validates before reading", BookValidation),
    ("Book read validates and preserves access failure", BookRead)
};
var failed = 0;
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception error) { failed++; Console.Error.WriteLine($"FAIL {test.Name}: {error}"); }
}
Console.WriteLine($"{tests.Length - failed}/{tests.Length} tests passed.");
return failed == 0 ? 0 : 1;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static async Task ReviewValidation()
{
    var store = new FakeStore();
    var service = new ReviewService(store);
    var valid = new SaveReview("edited", "v1");
    var results = new[] {
        await service.SaveAsync(new(" "), store.BookId, store.ParagraphId, valid),
        await service.SaveAsync(new("owner"), Guid.Empty, store.ParagraphId, valid),
        await service.SaveAsync(new("owner"), store.BookId, Guid.Empty, valid),
        await service.SaveAsync(new("owner"), store.BookId, store.ParagraphId, new(null!, "v1")),
        await service.SaveAsync(new("owner"), store.BookId, store.ParagraphId, new("edited", " "))
    };
    Check(results[0] is OperationResult<ParagraphView>.Failure { Code: ErrorCode.AccessDenied }, "Missing identity not denied.");
    Check(results.Skip(1).All(x => x is OperationResult<ParagraphView>.Failure { Code: ErrorCode.InvalidInput }), "Invalid review accepted.");
    Check(store.Saves == 0, "Invalid review reached persistence.");
}

static async Task ReviewConflict()
{
    var store = new FakeStore();
    var result = await new ReviewService(store).SaveAsync(new("owner"), store.BookId, store.ParagraphId, new("", "stale"));
    Check(result is OperationResult<ParagraphView>.Failure { Code: ErrorCode.Conflict }, "Conflict lost.");
    Check(store.Saves == 1 && store.LastReview == new SaveReview("", "stale"), "Empty correction or expected version changed.");
}

static async Task DraftAccess()
{
    var store = new FakeStore { ReadError = ErrorCode.AccessDenied };
    var provider = new FakeProvider();
    var result = await new DraftTranslationService(store, provider).RequestAsync(new("intruder"), store.BookId, "request");
    Check(result is OperationResult<DraftOperation>.Failure { Code: ErrorCode.AccessDenied }, "Access failure lost.");
    Check(store.Begins == 0 && provider.Inputs.Count == 0, "Unauthorized request caused side effects.");
}

static async Task DraftDuplicate()
{
    var store = new FakeStore { Created = false };
    var provider = new FakeProvider();
    var result = await new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request");
    Check(result is OperationResult<DraftOperation>.Success { Value: var value } && value == store.Operation, "Existing operation not returned.");
    Check(provider.Inputs.Count == 0 && store.Outcomes.Count == 0, "Duplicate request translated again.");
}

static async Task DraftSuccess()
{
    var store = new FakeStore();
    var provider = new FakeProvider();
    var result = await new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request");
    Check(result is OperationResult<DraftOperation>.Success { Value.State: DraftState.Succeeded }, "Draft did not succeed.");
    Check(provider.Inputs.Count == 2 && provider.Inputs.All(x => x.SourceLanguage == "ar" && x.TargetLanguage == "en"), "Wrong language or call count.");
    Check(store.Outcomes.Single() is DraftOutcome.Completed completed && completed.Paragraphs.Count == 2 &&
        completed.Paragraphs.Select(x => x.ParagraphId).ToHashSet().SetEquals(store.Book.Paragraphs.Select(x => x.Id)), "Paragraph alignment lost.");
}

static async Task DraftFailure()
{
    var store = new FakeStore();
    var provider = new FakeProvider { FailAt = 2 };
    await new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request");
    Check(store.Outcomes.Count == 1 && store.Outcomes[0] is DraftOutcome.Failed, "Partial results persisted on provider failure.");
}

static async Task DraftBlank()
{
    var store = new FakeStore();
    await new DraftTranslationService(store, new FakeProvider { Output = " " }).RequestAsync(new("owner"), store.BookId, "request");
    Check(store.Outcomes.Single() is DraftOutcome.Failed, "Blank draft persisted.");
}

static async Task DraftCancellation()
{
    var store = new FakeStore();
    using var cancellation = new CancellationTokenSource();
    var provider = new FakeProvider { Cancel = cancellation };
    try
    {
        await new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request", cancellation.Token);
        throw new InvalidOperationException("Cancellation swallowed.");
    }
    catch (OperationCanceledException) { }
    Check(store.Outcomes.Single() is DraftOutcome.Failed, "Canceled operation left pending.");
    Check(!store.FinishToken.IsCancellationRequested, "Cleanup used canceled token.");
}

static async Task DraftValidation()
{
    var store = new FakeStore();
    var service = new DraftTranslationService(store, new FakeProvider());
    var results = new[] {
        await service.RequestAsync(new(""), store.BookId, "request"),
        await service.RequestAsync(new("owner"), Guid.Empty, "request"),
        await service.RequestAsync(new("owner"), store.BookId, " ")
    };
    Check(results[0] is OperationResult<DraftOperation>.Failure { Code: ErrorCode.AccessDenied }, "Missing identity not denied.");
    Check(results.Skip(1).All(x => x is OperationResult<DraftOperation>.Failure { Code: ErrorCode.InvalidInput }), "Invalid draft request accepted.");
    Check(store.Reads == 0 && store.Begins == 0, "Invalid request reached persistence.");
}

static async Task DraftStatus()
{
    var store = new FakeStore();
    var reviewer = new ReviewerContext("owner");
    var result = await new DraftTranslationService(store, new FakeProvider()).GetStatusAsync(reviewer, store.BookId, store.Operation.Id);
    Check(result is OperationResult<DraftOperation>.Success, "Status not returned.");
    Check(store.StatusScope == (reviewer, store.BookId, store.Operation.Id), "Status scope lost.");
}

static async Task DraftException()
{
    var store = new FakeStore();
    await new DraftTranslationService(store, new FakeProvider { Throw = true }).RequestAsync(new("owner"), store.BookId, "request");
    Check(store.Outcomes.Single() is DraftOutcome.Failed failed && !failed.Message.Contains("secret"), "Exception details persisted.");
}

static async Task DraftFinishCancellation()
{
    using var cancellation = new CancellationTokenSource();
    var store = new FakeStore { CancelFinish = cancellation };
    try
    {
        await new DraftTranslationService(store, new FakeProvider()).RequestAsync(new("owner"), store.BookId, "request", cancellation.Token);
        throw new InvalidOperationException("Finish cancellation swallowed.");
    }
    catch (OperationCanceledException) { }
    Check(store.Outcomes.Last() is DraftOutcome.Failed && !store.FinishToken.IsCancellationRequested, "Finish cancellation did not clean up.");
}

static async Task BookImport()
{
    var store = new FakeStore();
    var reader = new FakeReader { Paragraphs = [new(4, "fourth"), new(1, "first")] };
    using var stream = new MemoryStream([1, 2]);
    var request = new ImportBook(" Book ", "source", "test/source", stream);
    var result = await new BookService(store, reader).ImportAsync(new("owner"), request);
    Check(result is OperationResult<BookSummary>.Success, "Import failed.");
    Check(store.NewBooks.Count == 1 && store.NewBooks[0].Title == "Book" &&
        store.NewBooks[0].Paragraphs.Select(p => p.Position).SequenceEqual([1, 4]), "Import changed order or was not one write.");
    Check(ReferenceEquals(reader.Input, request) && stream.CanRead, "Reader input changed or caller stream disposed.");
}

static async Task BookReaderFailure()
{
    var store = new FakeStore();
    using var stream = new MemoryStream();
    var result = await new BookService(store, new FakeReader { Error = ErrorCode.InvalidInput })
        .ImportAsync(new("owner"), new("Book", "source", "test/source", stream));
    Check(result is OperationResult<BookSummary>.Failure { Code: ErrorCode.InvalidInput } && store.NewBooks.Count == 0, "Reader failure created a book.");
    Check(stream.CanRead, "Failure disposed caller stream.");
}

static async Task BookInvalidParagraphs()
{
    IReadOnlyList<SourceParagraph>[] invalid = [[], [new(0, "one"), new(0, "two")], [new(-1, "text")], [new(0, " ")]];
    foreach (var paragraphs in invalid)
    {
        var store = new FakeStore();
        using var stream = new MemoryStream();
        var result = await new BookService(store, new FakeReader { Paragraphs = paragraphs })
            .ImportAsync(new("owner"), new("Book", "source", "test/source", stream));
        Check(result is OperationResult<BookSummary>.Failure { Code: ErrorCode.InvalidInput } && store.NewBooks.Count == 0, "Invalid paragraph set persisted.");
    }
}

static async Task BookValidation()
{
    var store = new FakeStore();
    var reader = new FakeReader();
    var service = new BookService(store, reader);
    using var stream = new MemoryStream();
    var request = new ImportBook("Book", "source", "test/source", stream);
    Check(await service.ImportAsync(new(" "), request) is OperationResult<BookSummary>.Failure { Code: ErrorCode.AccessDenied }, "Identity not validated.");
    Check(await service.ImportAsync(new("owner"), request with { Title = " " }) is OperationResult<BookSummary>.Failure { Code: ErrorCode.InvalidInput }, "Title not validated.");
    Check(reader.Input is null && store.NewBooks.Count == 0, "Invalid import caused effects.");
}

static async Task BookRead()
{
    var store = new FakeStore { ReadError = ErrorCode.AccessDenied };
    var service = new BookService(store, new FakeReader());
    Check(await service.GetAsync(new(" "), store.BookId) is OperationResult<BookReview>.Failure { Code: ErrorCode.AccessDenied }, "Missing identity not denied.");
    Check(await service.GetAsync(new("owner"), Guid.Empty) is OperationResult<BookReview>.Failure { Code: ErrorCode.InvalidInput }, "Empty ID not rejected.");
    Check(store.Reads == 0, "Invalid read reached store.");
    Check(await service.GetAsync(new("owner"), store.BookId) is OperationResult<BookReview>.Failure { Code: ErrorCode.AccessDenied } && store.Reads == 1, "Store access failure lost.");
}

sealed class FakeReader : ISourceDocumentReader
{
    public ImportBook? Input { get; private set; }
    public IReadOnlyList<SourceParagraph> Paragraphs { get; init; } = [new(0, "source")];
    public ErrorCode? Error { get; init; }
    public Task<OperationResult<IReadOnlyList<SourceParagraph>>> ReadAsync(ImportBook input, CancellationToken cancellationToken = default)
    {
        Input = input;
        return Task.FromResult<OperationResult<IReadOnlyList<SourceParagraph>>>(Error is { } code
            ? new OperationResult<IReadOnlyList<SourceParagraph>>.Failure(code, "invalid document")
            : new OperationResult<IReadOnlyList<SourceParagraph>>.Success(Paragraphs));
    }
}

sealed class FakeProvider : ITranslationProvider
{
    public List<TranslationInput> Inputs { get; } = [];
    public int FailAt { get; init; }
    public string Output { get; init; } = "translation";
    public CancellationTokenSource? Cancel { get; init; }
    public bool Throw { get; init; }
    public Task<OperationResult<string>> TranslateAsync(TranslationInput input, CancellationToken cancellationToken = default)
    {
        Inputs.Add(input);
        if (Throw) throw new InvalidOperationException("secret credential");
        Cancel?.Cancel();
        cancellationToken.ThrowIfCancellationRequested();
        OperationResult<string> result = Inputs.Count == FailAt
            ? new OperationResult<string>.Failure(ErrorCode.DependencyUnavailable, "provider unavailable")
            : new OperationResult<string>.Success(Output);
        return Task.FromResult(result);
    }
}

sealed class FakeStore : IReviewStore
{
    public Guid BookId { get; } = Guid.NewGuid();
    public Guid ParagraphId { get; } = Guid.NewGuid();
    public BookReview Book => new(new(BookId, "Book"), [new(ParagraphId, 0, "أول", null, "review", "v1"), new(Guid.Parse("b887f367-74ed-4b43-af0f-4a4ea83a0317"), 1, "ثان", null, null, "v1")]);
    public DraftOperation Operation { get; } = new(Guid.NewGuid(), DraftState.Pending, null);
    public ErrorCode? ReadError { get; init; }
    public bool Created { get; init; } = true;
    public int Reads { get; private set; }
    public int Begins { get; private set; }
    public int Saves { get; private set; }
    public SaveReview? LastReview { get; private set; }
    public List<DraftOutcome> Outcomes { get; } = [];
    public CancellationToken FinishToken { get; private set; }
    public CancellationTokenSource? CancelFinish { get; init; }
    public (ReviewerContext, Guid, Guid)? StatusScope { get; private set; }
    public List<NewBook> NewBooks { get; } = [];
    public Task<OperationResult<BookReview>> CreateAsync(ReviewerContext reviewer, NewBook book, CancellationToken cancellationToken = default)
    {
        NewBooks.Add(book);
        return Task.FromResult<OperationResult<BookReview>>(new OperationResult<BookReview>.Success(Book));
    }
    public Task<OperationResult<BookReview>> GetAsync(ReviewerContext reviewer, Guid bookId, CancellationToken cancellationToken = default)
    {
        Reads++;
        return Task.FromResult<OperationResult<BookReview>>(ReadError is { } code ? new OperationResult<BookReview>.Failure(code, "denied") : new OperationResult<BookReview>.Success(Book));
    }
    public Task<OperationResult<ParagraphView>> SaveReviewAsync(ReviewerContext reviewer, Guid bookId, Guid paragraphId, SaveReview review, CancellationToken cancellationToken = default)
    {
        Saves++;
        LastReview = review;
        return Task.FromResult<OperationResult<ParagraphView>>(new OperationResult<ParagraphView>.Failure(ErrorCode.Conflict, "stale"));
    }
    public Task<OperationResult<BeginDraftResult>> BeginDraftAsync(ReviewerContext reviewer, Guid bookId, string requestId, CancellationToken cancellationToken = default)
    {
        Begins++;
        return Task.FromResult<OperationResult<BeginDraftResult>>(new OperationResult<BeginDraftResult>.Success(new(Operation, Created)));
    }
    public Task<OperationResult<DraftOperation>> FinishDraftAsync(ReviewerContext reviewer, Guid bookId, Guid operationId, DraftOutcome outcome, CancellationToken cancellationToken = default)
    {
        Outcomes.Add(outcome);
        FinishToken = cancellationToken;
        if (outcome is DraftOutcome.Completed && CancelFinish is { } cancel)
        {
            cancel.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
        }
        return Task.FromResult<OperationResult<DraftOperation>>(new OperationResult<DraftOperation>.Success(Operation with { State = outcome is DraftOutcome.Completed ? DraftState.Succeeded : DraftState.Failed }));
    }
    public Task<OperationResult<DraftOperation>> GetDraftAsync(ReviewerContext reviewer, Guid bookId, Guid operationId, CancellationToken cancellationToken = default)
    {
        StatusScope = (reviewer, bookId, operationId);
        return Task.FromResult<OperationResult<DraftOperation>>(new OperationResult<DraftOperation>.Success(Operation));
    }
}
