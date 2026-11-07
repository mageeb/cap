using System.Net;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Reviews;
using Translation.Host;

const string owner = "sample:romans-antonios-fekry";
var checks = 0;
async Task Check(string name, Func<Task> action)
{
    await action();
    checks++;
    Console.WriteLine("PASS " + name);
}
static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
static async Task Response(HttpResponseMessage response, HttpStatusCode status, string? code = null)
{
    using (response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert(response.StatusCode == status, $"Unexpected HTTP status: {response.StatusCode}");
        Assert(!body.Contains("secret-sentinel", StringComparison.Ordinal), "Sensitive dependency content leaked.");
        if (code is not null) Assert(body.Contains(code, StringComparison.Ordinal), "Unexpected dependency code.");
    }
}
static Dictionary<string, string?> Settings(string? uri = null, string? ca = null) => new()
{
    ["Translation:DevReview:Enabled"] = "true",
    ["Translation:DeploymentTier"] = "DEV",
    ["Translation:DevReview:ReviewerId"] = "sample:romans-antonios-fekry",
    ["Translation:Mongo:ConnectionString"] = uri,
    ["Translation:Mongo:CaFile"] = ca
};

await Check("Default liveness succeeds while documents fail closed", async () =>
{
    await using var harness = await Harness.Start();
    await Response(await harness.Client.GetAsync("/health/live"), HttpStatusCode.OK);
    await Response(await harness.Client.GetAsync("/health/ready"), HttpStatusCode.ServiceUnavailable,
        "business_dependencies_not_configured");
    await Response(await harness.Client.GetAsync($"/api/books/{Guid.NewGuid()}"), HttpStatusCode.ServiceUnavailable,
        "business_dependencies_not_configured");
});

await Check("Explicit DEV opt-in rejects missing or wrong tier and owner", () =>
{
    foreach (var (key, value) in new[] {
        ("Translation:DeploymentTier", "PROD"), ("Translation:DeploymentTier", ""),
        ("Translation:DevReview:ReviewerId", "another-reviewer"), ("Translation:DevReview:ReviewerId", "") })
    {
        var config = Settings(); config[key] = value;
        Reject(config);
    }
    Reject(Settings());
    return Task.CompletedTask;
});

var temporary = Path.Combine(Path.GetTempPath(), "translation-devtests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporary);
try
{
    using var rsa = RSA.Create(2048);
    var certificateRequest = new CertificateRequest("CN=Translation DevTests CA", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
    using var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
    var ca = Path.Combine(temporary, "ca.crt");
    File.WriteAllText(ca, certificate.ExportCertificatePem());

    await Check("Invalid Mongo TLS configuration is rejected without credential leakage", () =>
    {
        foreach (var uri in new[] {
            "mongodb://user:secret-sentinel@localhost:27017/?tls=false",
            "mongodb://user:secret-sentinel@localhost:27017/?tls=true&tlsInsecure=true",
            "not-a-mongo-uri-secret-sentinel" }) Reject(Settings(uri, ca));
        Reject(Settings("mongodb://user:secret-sentinel@localhost:27017/?tls=true", Path.Combine(temporary, "absent.crt")));
        return Task.CompletedTask;
    });

    await Check("Valid explicit opt-in registers only document capabilities", async () =>
    {
        await using var app = Harness.Build(Settings("mongodb://localhost:1/?tls=true", ca));
        Assert(app.Services.GetService<DevReviewer>()?.Subject == owner, "Approved sample identity missing.");
        Assert(app.Services.GetService<IBookService>() is not null, "Book service missing.");
        Assert(app.Services.GetService<IReviewService>() is not null, "Review service missing.");
        Assert(app.Services.GetService<Translation.Backend.Drafts.IDraftTranslationService>() is null,
            "Unconfigured translation capability was registered.");
    });
}
finally { Directory.Delete(temporary, true); }

await Check("Read and save work without a translation dependency", async () =>
{
    var fake = new DocumentServices();
    await using var harness = await Harness.Start(configure: builder =>
    {
        builder.Services.AddSingleton(new DevReviewer(owner));
        builder.Services.AddSingleton<IBookService>(fake);
        builder.Services.AddSingleton<IReviewService>(fake);
    });
    using var loaded = await harness.Client.GetAsync($"/api/books/{fake.BookId}");
    Assert(loaded.StatusCode == HttpStatusCode.OK, "Read incorrectly requires translation.");
    await Response(await harness.Client.PutAsJsonAsync($"/api/books/{fake.BookId}/paragraphs/{fake.ParagraphId}/review",
        new { reviewedText = "", expectedVersion = "v1" }), HttpStatusCode.OK);
    Assert(fake.Subject == owner && fake.Saved == new SaveReview("", "v1"), "Scoped save was not dispatched.");
    await Response(await harness.Client.PostAsJsonAsync($"/api/books/{fake.BookId}/drafts", new { requestId = "key" }),
        HttpStatusCode.ServiceUnavailable, "business_dependencies_not_configured");
});

var mongoUri = Environment.GetEnvironmentVariable("TRANSLATION_DEV_MONGO_URI");
var mongoCa = Environment.GetEnvironmentVariable("TRANSLATION_DEV_MONGO_CA");
if (!string.IsNullOrWhiteSpace(mongoUri) || !string.IsNullOrWhiteSpace(mongoCa))
{
    Assert(!string.IsNullOrWhiteSpace(mongoUri) && !string.IsNullOrWhiteSpace(mongoCa), "Both Mongo integration settings are required.");
    await Check("Real Mongo read save conflict and ownership isolation", async () =>
    {
        using var root = X509Certificate2.CreateFromPem(File.ReadAllText(mongoCa!));
        var settings = MongoClientSettings.FromConnectionString(mongoUri!);
        Assert(settings.UseTls && !settings.AllowInsecureTls, "Integration requires verified TLS.");
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        settings.SslSettings = new SslSettings { ServerCertificateValidationCallback = (_, cert, _, errors) =>
        {
            if (cert is null || (errors & (SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable)) != 0) return false;
            using var chain = new X509Chain();
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            chain.ChainPolicy.CustomTrustStore.Add(root);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.1"));
            using var server = new X509Certificate2(cert);
            return chain.Build(server);
        } };
        var database = new MongoClient(settings).GetDatabase("translation_review");
        var books = database.GetCollection<BsonDocument>("books");
        var paragraphs = database.GetCollection<BsonDocument>("paragraphs");
        var bookId = Guid.NewGuid(); var paragraphId = Guid.NewGuid(); var other = Guid.NewGuid();
        var stamp = new BsonDateTime(DateTime.UtcNow);
        try
        {
            await books.InsertManyAsync(new[] {
                new BsonDocument { { "_id", bookId.ToString("D") }, { "title", "Disposable integration fixture" }, { "reviewerId", owner }, { "createdAt", stamp }, { "updatedAt", stamp } },
                new BsonDocument { { "_id", other.ToString("D") }, { "title", "Other owner fixture" }, { "reviewerId", "another-owner" }, { "createdAt", stamp }, { "updatedAt", stamp } }
            });
            await paragraphs.InsertOneAsync(new BsonDocument {
                { "_id", paragraphId.ToString("D") }, { "bookId", bookId.ToString("D") }, { "reviewerId", owner },
                { "position", 0 }, { "sourceText", "Synthetic source" }, { "machineDraft", "Synthetic draft" },
                { "reviewedText", BsonNull.Value }, { "version", "v1" }, { "chapterId", Guid.NewGuid().ToString("D") },
                { "chapterPosition", 0 }, { "createdAt", stamp }, { "updatedAt", stamp }
            });
            await using var harness = await Harness.Start(Settings(mongoUri, mongoCa));
            var original = await harness.Client.GetFromJsonAsync<BookReview>($"/api/books/{bookId}");
            Assert(original?.Paragraphs.Single().Version == "v1", "Database read failed.");
            using var saved = await harness.Client.PutAsJsonAsync($"/api/books/{bookId}/paragraphs/{paragraphId}/review",
                new { reviewedText = "", expectedVersion = "v1" });
            Assert(saved.StatusCode == HttpStatusCode.OK, "Database save failed.");
            var changed = await saved.Content.ReadFromJsonAsync<ParagraphView>();
            Assert(changed?.ReviewedText == "" && changed.Version != "v1", "Database save did not update content/version.");
            await Response(await harness.Client.PutAsJsonAsync($"/api/books/{bookId}/paragraphs/{paragraphId}/review",
                new { reviewedText = "stale", expectedVersion = "v1" }), HttpStatusCode.Conflict);
            var reload = await harness.Client.GetFromJsonAsync<BookReview>($"/api/books/{bookId}");
            Assert(reload?.Paragraphs.Single() is { ReviewedText: "", SourceText: "Synthetic source", MachineDraft: "Synthetic draft" },
                "Reload lost correction or altered source/draft.");
            await Response(await harness.Client.GetAsync($"/api/books/{other}"), HttpStatusCode.NotFound);
            await Response(await harness.Client.PutAsJsonAsync($"/api/books/{other}/paragraphs/{paragraphId}/review",
                new { reviewedText = "cross-book", expectedVersion = changed!.Version }), HttpStatusCode.NotFound);
            var competing = await Task.WhenAll(
                harness.Client.PutAsJsonAsync($"/api/books/{bookId}/paragraphs/{paragraphId}/review",
                    new { reviewedText = "winner-one", expectedVersion = changed!.Version }),
                harness.Client.PutAsJsonAsync($"/api/books/{bookId}/paragraphs/{paragraphId}/review",
                    new { reviewedText = "winner-two", expectedVersion = changed!.Version }));
            try
            {
                Assert(competing.Count(r => r.StatusCode == HttpStatusCode.OK) == 1 &&
                    competing.Count(r => r.StatusCode == HttpStatusCode.Conflict) == 1,
                    "Concurrent same-version writes did not have exactly one winner.");
            }
            finally { foreach (var response in competing) response.Dispose(); }
        }
        finally
        {
            await paragraphs.DeleteManyAsync(new BsonDocument("bookId", bookId.ToString("D")));
            await books.DeleteManyAsync(new BsonDocument("_id", new BsonDocument("$in", new BsonArray { bookId.ToString("D"), other.ToString("D") })));
        }
    });
}
else Console.WriteLine("SKIP real Mongo integration: explicit connection and CA settings absent.");
Console.WriteLine($"{checks}/{checks} DEV dependency checks passed.");

static void Reject(Dictionary<string, string?> settings)
{
    try { var app = Harness.Build(settings); app.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
    catch (InvalidOperationException error)
    {
        Assert(!error.ToString().Contains("secret-sentinel", StringComparison.Ordinal), "Invalid configuration leaked credentials.");
        return;
    }
    throw new Exception("Unsafe DEV configuration was accepted.");
}

sealed class Harness(WebApplication app, HttpClient client) : IAsyncDisposable
{
    public HttpClient Client => client;
    public static WebApplication Build(Dictionary<string, string?>? settings = null, Action<WebApplicationBuilder>? configure = null) =>
        HostApplication.Build([], builder =>
        {
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(settings ?? new Dictionary<string, string?>());
            builder.Logging.ClearProviders();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            configure?.Invoke(builder);
        });
    public static async Task<Harness> Start(Dictionary<string, string?>? settings = null, Action<WebApplicationBuilder>? configure = null)
    {
        var app = Build(settings, configure);
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        return new(app, new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(15) });
    }
    public async ValueTask DisposeAsync() { client.Dispose(); await app.StopAsync(); await app.DisposeAsync(); }
}

sealed class DocumentServices : IBookService, IReviewService
{
    public Guid BookId { get; } = Guid.NewGuid();
    public Guid ParagraphId { get; } = Guid.NewGuid();
    public string? Subject;
    public SaveReview? Saved;
    private ParagraphView Paragraph => new(ParagraphId, 0, "synthetic", "draft", Saved?.ReviewedText, "v2");
    public Task<OperationResult<BookReview>> GetAsync(ReviewerContext reviewer, Guid bookId, CancellationToken cancellationToken = default)
    {
        Subject = reviewer.SubjectId;
        return Task.FromResult<OperationResult<BookReview>>(new OperationResult<BookReview>.Success(
            new BookReview(new BookSummary(BookId, "synthetic"), [Paragraph])));
    }
    public Task<OperationResult<ParagraphView>> SaveAsync(ReviewerContext reviewer, Guid bookId, Guid paragraphId,
        SaveReview request, CancellationToken cancellationToken = default)
    {
        Subject = reviewer.SubjectId; Saved = request;
        return Task.FromResult<OperationResult<ParagraphView>>(new OperationResult<ParagraphView>.Success(Paragraph));
    }
    public Task<OperationResult<BookSummary>> ImportAsync(ReviewerContext reviewer, ImportBook request,
        CancellationToken cancellationToken = default) => throw new NotSupportedException();
}
