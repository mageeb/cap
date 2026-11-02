using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Drafts;
using Translation.Backend.Persistence;
using Translation.Backend.Reviews;

static class UnitBoundaryTests
{
    public static IReadOnlyList<(string Name, Func<Task> Run)> Cases =>
    [
        ("Pre-cancelled calls produce no dependency work", PreCancelled),
        ("Import rejects every missing input before reading", ImportInputs),
        ("Import rejects null paragraphs without persistence", NullParagraph),
        ("Import preserves creation failure code and message", CreationFailure),
        ("Review preserves caller scope, version and cancellation token", ReviewScope),
        ("Draft ownership failure stops translation", BeginFailure),
        ("Draft translates in position order and maps exact text", DraftOrdering),
        ("Draft preserves final persistence conflict", FinishFailure),
        ("First provider failure stops work and publishes only sanitized failure", StopOnFailure),
        ("Cleanup failure cannot replace cancellation", CleanupFailure),
        ("Status validates all identifiers before persistence", StatusValidation)
    ];

    static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static async Task Cancelled(Func<Task> action, CancellationToken expected)
    {
        try { await action(); }
        catch (OperationCanceledException error)
        {
            Check(error.CancellationToken == expected, "Cancellation token changed.");
            return;
        }
        throw new InvalidOperationException("Cancellation swallowed.");
    }

    static async Task PreCancelled()
    {
        var store = new FakeStore();
        var reader = new FakeReader();
        var provider = new FakeProvider();
        using var source = new CancellationTokenSource();
        source.Cancel();
        var token = source.Token;
        var caller = new ReviewerContext("owner");
        using var stream = new MemoryStream();
        var books = new BookService(store, reader);
        var drafts = new DraftTranslationService(store, provider);
        await Cancelled(() => books.ImportAsync(caller, new("Book", "source", "test/source", stream), token), token);
        await Cancelled(() => books.GetAsync(caller, store.BookId, token), token);
        await Cancelled(() => new ReviewService(store).SaveAsync(caller, store.BookId, store.ParagraphId, new("edit", "v1"), token), token);
        await Cancelled(() => drafts.RequestAsync(caller, store.BookId, "request", token), token);
        await Cancelled(() => drafts.GetStatusAsync(caller, store.BookId, store.Operation.Id, token), token);
        Check(reader.Input is null && store.Reads == 0 && store.Saves == 0 && store.Begins == 0 &&
            store.StatusScope is null && provider.Inputs.Count == 0, "Cancelled call reached a dependency.");
    }

    static async Task ImportInputs()
    {
        var store = new FakeStore();
        var reader = new FakeReader();
        var service = new BookService(store, reader);
        using var stream = new MemoryStream();
        using var closed = new MemoryStream();
        closed.Dispose();
        var valid = new ImportBook("Book", "source", "test/source", stream);
        ImportBook[] invalid = [null!, valid with { FileName = " " }, valid with { MediaType = "" },
            valid with { Content = null! }, valid with { Content = closed }];
        foreach (var request in invalid)
            Check(await service.ImportAsync(new("owner"), request) is OperationResult<BookSummary>.Failure { Code: ErrorCode.InvalidInput }, "Invalid import accepted.");
        Check(await service.ImportAsync(null!, valid) is OperationResult<BookSummary>.Failure { Code: ErrorCode.AccessDenied }, "Null caller accepted.");
        Check(reader.Input is null && store.NewBooks.Count == 0 && stream.CanRead, "Validation caused effects.");
    }

    static async Task NullParagraph()
    {
        var store = new FakeStore();
        using var stream = new MemoryStream();
        var result = await new BookService(store, new FakeReader { Paragraphs = [null!] })
            .ImportAsync(new("owner"), new("Book", "source", "test/source", stream));
        Check(result is OperationResult<BookSummary>.Failure { Code: ErrorCode.InvalidInput } && store.NewBooks.Count == 0, "Null paragraph persisted.");
    }

    static async Task CreationFailure()
    {
        var store = new FakeStore { CreateError = ErrorCode.DependencyUnavailable };
        using var stream = new MemoryStream();
        var result = await new BookService(store, new FakeReader())
            .ImportAsync(new("owner"), new("Book", "source", "test/source", stream));
        Check(result is OperationResult<BookSummary>.Failure { Code: ErrorCode.DependencyUnavailable, Message: "create rejected" }, "Creation failure changed.");
        Check(store.NewBooks.Count == 1 && stream.CanRead, "Import repeated write or took stream ownership.");
    }

    static async Task ReviewScope()
    {
        var store = new FakeStore();
        var caller = new ReviewerContext("owner");
        using var source = new CancellationTokenSource();
        var service = new ReviewService(store);
        Check(await service.SaveAsync(null!, store.BookId, store.ParagraphId, new("edit", "v1")) is OperationResult<ParagraphView>.Failure { Code: ErrorCode.AccessDenied }, "Null caller accepted.");
        Check(await service.SaveAsync(caller, store.BookId, store.ParagraphId, null!) is OperationResult<ParagraphView>.Failure { Code: ErrorCode.InvalidInput }, "Null review accepted.");
        var review = new SaveReview(" exact edit \n", "v1");
        await service.SaveAsync(caller, store.BookId, store.ParagraphId, review, source.Token);
        Check(store.Saves == 1 && store.SaveScope == (caller, store.BookId, store.ParagraphId) &&
            store.LastReview == review && store.LastToken == source.Token, "Review scope, data or token changed.");
    }

    static async Task BeginFailure()
    {
        var store = new FakeStore { BeginError = ErrorCode.Conflict };
        var provider = new FakeProvider();
        var result = await new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request");
        Check(result is OperationResult<DraftOperation>.Failure { Code: ErrorCode.Conflict, Message: "begin rejected" }, "Ownership failure changed.");
        Check(provider.Inputs.Count == 0 && store.Outcomes.Count == 0, "Non-owner translated or finalized.");
    }

    static async Task DraftOrdering()
    {
        var store = new FakeStore { ReverseParagraphs = true };
        var provider = new FakeProvider { Output = " exact draft \n" };
        await new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request");
        var expected = store.Book.Paragraphs.OrderBy(p => p.Position).ToArray();
        Check(provider.Inputs.Select(p => p.SourceText).SequenceEqual(expected.Select(p => p.SourceText)), "Provider received wrong source order.");
        Check(store.Outcomes.Single() is DraftOutcome.Completed completed &&
            completed.Paragraphs.Select(p => p.ParagraphId).SequenceEqual(expected.Select(p => p.Id)) &&
            completed.Paragraphs.All(p => p.Text == provider.Output), "Draft order, text or alignment changed.");
    }

    static async Task FinishFailure()
    {
        var store = new FakeStore { FinishError = ErrorCode.Conflict };
        var result = await new DraftTranslationService(store, new FakeProvider()).RequestAsync(new("owner"), store.BookId, "request");
        Check(result is OperationResult<DraftOperation>.Failure { Code: ErrorCode.Conflict, Message: "finish rejected" } && store.Outcomes.Count == 1, "Final persistence failure changed or retried.");
    }

    static async Task CleanupFailure()
    {
        var store = new FakeStore { ThrowCleanup = true };
        using var source = new CancellationTokenSource();
        var provider = new FakeProvider { Cancel = source };
        await Cancelled(() => new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request", source.Token), source.Token);
        Check(store.Outcomes.Single() is DraftOutcome.Failed && !store.FinishToken.CanBeCanceled, "Cleanup was not attempted independently.");
    }

    static async Task StopOnFailure()
    {
        foreach (var provider in new[] { new FakeProvider { FailAt = 1 }, new FakeProvider { Output = null! } })
        {
            var store = new FakeStore();
            var result = await new DraftTranslationService(store, provider).RequestAsync(new("owner"), store.BookId, "request");
            Check(provider.Inputs.Count == 1, "Translation continued after the first failed/invalid response.");
            Check(store.Outcomes.Single() is DraftOutcome.Failed { Message: "Translation is temporarily unavailable." } &&
                result is OperationResult<DraftOperation>.Success { Value.State: DraftState.Failed }, "Failure was not sanitized and finalized exactly once.");
        }
    }

    static async Task StatusValidation()
    {
        var store = new FakeStore();
        var service = new DraftTranslationService(store, new FakeProvider());
        Check(await service.GetStatusAsync(null!, store.BookId, store.Operation.Id) is OperationResult<DraftOperation>.Failure { Code: ErrorCode.AccessDenied }, "Null caller accepted.");
        Check(await service.GetStatusAsync(new(" "), store.BookId, store.Operation.Id) is OperationResult<DraftOperation>.Failure { Code: ErrorCode.AccessDenied }, "Blank caller accepted.");
        foreach (var (book, operation) in new[] { (Guid.Empty, store.Operation.Id), (store.BookId, Guid.Empty) })
            Check(await service.GetStatusAsync(new("owner"), book, operation) is OperationResult<DraftOperation>.Failure { Code: ErrorCode.InvalidInput }, "Invalid status ID accepted.");
        Check(store.StatusScope is null, "Invalid status reached store.");
        using var source = new CancellationTokenSource();
        await service.GetStatusAsync(new("owner"), store.BookId, store.Operation.Id, source.Token);
        Check(store.LastToken == source.Token, "Status token changed.");
    }
}
