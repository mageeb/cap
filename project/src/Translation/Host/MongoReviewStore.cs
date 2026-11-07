using MongoDB.Bson;
using MongoDB.Driver;
using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Drafts;
using Translation.Backend.Persistence;
using Translation.Backend.Reviews;

namespace Translation.Host;

// Adapter for the provisioned DEV schema. Unsupported capabilities fail without writes.
public sealed class MongoReviewStore : IReviewStore
{
    private readonly IMongoDatabase database;
    private readonly IMongoCollection<BsonDocument> books;
    private readonly IMongoCollection<BsonDocument> paragraphs;

    public MongoReviewStore(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        this.database = database.WithReadConcern(ReadConcern.Majority)
            .WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
        books = this.database.GetCollection<BsonDocument>("books");
        paragraphs = this.database.GetCollection<BsonDocument>("paragraphs");
    }

    public async Task<bool> CheckReadyAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            var schema = await database.GetCollection<BsonDocument>("schemaVersions")
                .Find(new BsonDocument("_id", "translation-review-v1")).FirstOrDefaultAsync(cancellationToken);
            if (schema is null) return false;
            // Check the same collection privileges used by loading and saving documents.
            await books.Find(FilterDefinition<BsonDocument>.Empty).Project(new BsonDocument("_id", 1))
                .Limit(1).FirstOrDefaultAsync(cancellationToken);
            await paragraphs.Find(FilterDefinition<BsonDocument>.Empty).Project(new BsonDocument("_id", 1))
                .Limit(1).FirstOrDefaultAsync(cancellationToken);
            return true;
        }
        catch (MongoException) { return false; }
        catch (TimeoutException) { return false; }
    }

    public async Task<OperationResult<BookReview>> GetAsync(
        ReviewerContext reviewer, Guid bookId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidReviewer(reviewer)) return Denied<BookReview>();
        if (bookId == Guid.Empty) return Invalid<BookReview>();
        try
        {
            var book = await books.Find(BookScope(reviewer, bookId)).FirstOrDefaultAsync(cancellationToken);
            if (book is null) return Missing<BookReview>();
            var rows = await paragraphs.Find(ParagraphScope(reviewer, bookId))
                .Sort(new BsonDocument("position", 1)).ToListAsync(cancellationToken);
            return new OperationResult<BookReview>.Success(new BookReview(
                new BookSummary(Guid.Parse(book["_id"].AsString), book["title"].AsString),
                rows.Select(ToParagraph).ToArray()));
        }
        catch (MongoException) { return Unavailable<BookReview>(); }
        catch (TimeoutException) { return Unavailable<BookReview>(); }
    }

    public async Task<OperationResult<ParagraphView>> SaveReviewAsync(
        ReviewerContext reviewer, Guid bookId, Guid paragraphId, SaveReview review,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidReviewer(reviewer)) return Denied<ParagraphView>();
        if (bookId == Guid.Empty || paragraphId == Guid.Empty || review is null ||
            review.ReviewedText is null || string.IsNullOrWhiteSpace(review.ExpectedVersion))
            return Invalid<ParagraphView>();
        try
        {
            var book = await books.Find(BookScope(reviewer, bookId)).Project(new BsonDocument("_id", 1))
                .FirstOrDefaultAsync(cancellationToken);
            if (book is null) return Missing<ParagraphView>();
            var scope = ParagraphScope(reviewer, bookId);
            scope.Add("_id", paragraphId.ToString("D"));
            var expected = new BsonDocument(scope).Add("version", review.ExpectedVersion);
            var update = new BsonDocument("$set", new BsonDocument
            {
                { "reviewedText", review.ReviewedText },
                { "version", Guid.NewGuid().ToString("D") },
                { "updatedAt", new BsonDateTime(DateTime.UtcNow) }
            });
            var saved = await paragraphs.FindOneAndUpdateAsync(expected, update,
                new FindOneAndUpdateOptions<BsonDocument> { ReturnDocument = ReturnDocument.After }, cancellationToken);
            if (saved is not null) return new OperationResult<ParagraphView>.Success(ToParagraph(saved));
            var existing = await paragraphs.Find(scope).Project(new BsonDocument("_id", 1))
                .FirstOrDefaultAsync(cancellationToken);
            return existing is null ? Missing<ParagraphView>() :
                new OperationResult<ParagraphView>.Failure(ErrorCode.Conflict, "The paragraph version has changed.");
        }
        catch (MongoException) { return Unavailable<ParagraphView>(); }
        catch (TimeoutException) { return Unavailable<ParagraphView>(); }
    }

    public Task<OperationResult<BookReview>> CreateAsync(ReviewerContext reviewer, NewBook book,
        CancellationToken cancellationToken = default) => Unsupported<BookReview>(cancellationToken);

    public Task<OperationResult<BeginDraftResult>> BeginDraftAsync(ReviewerContext reviewer, Guid bookId,
        string requestId, CancellationToken cancellationToken = default) => Unsupported<BeginDraftResult>(cancellationToken);

    public Task<OperationResult<DraftOperation>> FinishDraftAsync(ReviewerContext reviewer, Guid bookId,
        Guid operationId, DraftOutcome outcome, CancellationToken cancellationToken = default) => Unsupported<DraftOperation>(cancellationToken);

    public Task<OperationResult<DraftOperation>> GetDraftAsync(ReviewerContext reviewer, Guid bookId,
        Guid operationId, CancellationToken cancellationToken = default) => Unsupported<DraftOperation>(cancellationToken);

    private static BsonDocument BookScope(ReviewerContext reviewer, Guid bookId) =>
        new() { { "_id", bookId.ToString("D") }, { "reviewerId", reviewer.SubjectId } };

    private static BsonDocument ParagraphScope(ReviewerContext reviewer, Guid bookId) =>
        new() { { "bookId", bookId.ToString("D") }, { "reviewerId", reviewer.SubjectId } };

    private static ParagraphView ToParagraph(BsonDocument row) => new(
        Guid.Parse(row["_id"].AsString), row["position"].AsInt32, row["sourceText"].AsString,
        NullableText(row, "machineDraft"), NullableText(row, "reviewedText"), row["version"].AsString);

    private static string? NullableText(BsonDocument row, string name) =>
        row.TryGetValue(name, out var value) && !value.IsBsonNull ? value.AsString : null;

    private static bool ValidReviewer(ReviewerContext reviewer) =>
        reviewer is not null && !string.IsNullOrWhiteSpace(reviewer.SubjectId);

    private static OperationResult<T> Denied<T>() =>
        new OperationResult<T>.Failure(ErrorCode.AccessDenied, "A validated reviewer is required.");
    private static OperationResult<T> Invalid<T>() =>
        new OperationResult<T>.Failure(ErrorCode.InvalidInput, "Valid resource IDs and review content are required.");
    private static OperationResult<T> Missing<T>() =>
        new OperationResult<T>.Failure(ErrorCode.NotFound, "The requested resource was not found.");
    private static OperationResult<T> Unavailable<T>() =>
        new OperationResult<T>.Failure(ErrorCode.DependencyUnavailable, "Document storage is unavailable.");
    private static Task<OperationResult<T>> Unsupported<T>(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<OperationResult<T>>(new OperationResult<T>.Failure(
            ErrorCode.DependencyUnavailable, "This capability is not configured."));
    }
}
