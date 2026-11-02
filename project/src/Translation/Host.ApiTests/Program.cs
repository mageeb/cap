using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Drafts;
using Translation.Backend.Reviews;
using Translation.Host;

var bookId = Guid.NewGuid();
var paragraphId = Guid.NewGuid();
var operationId = Guid.NewGuid();
var checks = 0;
async Task Check(string name, Func<Task> check)
{
    await check();
    checks++;
    Console.WriteLine($"PASS {name}");
}
static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
static async Task Status(HttpResponseMessage response, HttpStatusCode expected)
{
    using (response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert(response.StatusCode == expected, $"Expected {expected}, got {response.StatusCode}: {body}");
        Assert(!body.Contains("secret-sentinel", StringComparison.Ordinal), "Dependency detail leaked");
    }
}
static ClaimsPrincipal Identity(params string[] subjects) => new(new ClaimsIdentity(
    subjects.Select(subject => new Claim("sub", subject)), "test-only"));
var mode = new TestIdentity();
await using var unavailable = await Harness.Start(null, mode);
await Check("Missing services block all business routes before body parsing", async () =>
{
    foreach (var request in new[] {
        new HttpRequestMessage(HttpMethod.Post, "/api/books") { Content = new StringContent("not multipart") },
        new HttpRequestMessage(HttpMethod.Get, $"/api/books/{bookId}"),
        new HttpRequestMessage(HttpMethod.Put, $"/api/books/{bookId}/paragraphs/{paragraphId}/review") { Content = new StringContent("{") },
        new HttpRequestMessage(HttpMethod.Post, $"/api/books/{bookId}/drafts") { Content = new StringContent("{") },
        new HttpRequestMessage(HttpMethod.Get, $"/api/books/{bookId}/drafts/{operationId}") })
    {
        request.Headers.Add("Authorization", "Bearer secret-sentinel");
        request.Headers.Add("X-User-Id", "forged");
        await Status(await unavailable.Client.SendAsync(request), HttpStatusCode.ServiceUnavailable);
        request.Dispose();
    }
});
var fake = new Services(bookId, paragraphId, operationId);
await using var server = await Harness.Start(fake, mode);
await Check("Forged headers do not establish identity", async () =>
{
    using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{bookId}");
    request.Headers.Add("X-User-Id", "forged");
    request.Headers.Add("Authorization", "Bearer forged");
    await Status(await server.Client.SendAsync(request), HttpStatusCode.Unauthorized);
    Assert(fake.Calls == 0, "Unauthenticated request invoked a service");
});
await Check("Ambiguous and anonymous subjects are rejected", async () =>
{
    mode.Principal = Identity("one", "two");
    await Status(await server.Client.GetAsync($"/api/books/{bookId}"), HttpStatusCode.Unauthorized);
    mode.Principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "anonymous") }));
    await Status(await server.Client.GetAsync($"/api/books/{bookId}"), HttpStatusCode.Unauthorized);
});
mode.Principal = Identity("validated-reviewer");
await Check("Read maps IDs, validated subject and cancellation token", async () =>
{
    using var response = await server.Client.GetAsync($"/api/books/{bookId}");
    Assert(response.StatusCode == HttpStatusCode.OK, "Read failed");
    var result = await response.Content.ReadFromJsonAsync<BookReview>();
    Assert(result?.Book.Id == bookId && fake.Subject == "validated-reviewer" && fake.LastToken.CanBeCanceled,
        "Read did not propagate context");
});
await Check("Multipart import preserves bytes, metadata and returns Location", async () =>
{
    using var multipart = new MultipartFormDataContent();
    multipart.Add(new StringContent("Document title"), "title");
    var content = new ByteArrayContent(new byte[] { 0, 1, 127, 255 });
    content.Headers.ContentType = new("application/x-test-document");
    multipart.Add(content, "file", "source.bin");
    using var response = await server.Client.PostAsync("/api/books", multipart);
    Assert(response.StatusCode == HttpStatusCode.Created && response.Headers.Location?.ToString() == $"/api/books/{bookId}", "Import status/location failed");
    Assert(fake.ImportedTitle == "Document title" && fake.ImportedName == "source.bin" && fake.ImportedMedia == "application/x-test-document" && fake.ImportedBytes!.SequenceEqual(new byte[] { 0, 1, 127, 255 }), "Import changed input");
    try { fake.ImportedStream!.ReadByte(); throw new Exception("Import stream remains readable after service call"); }
    catch (ObjectDisposedException) { }

});
await Check("Review preserves intentionally empty text and expected version", async () =>
{
    using var response = await server.Client.PutAsJsonAsync($"/api/books/{bookId}/paragraphs/{paragraphId}/review",
        new { reviewedText = "", expectedVersion = "v1", subjectId = "forged" });
    Assert(response.StatusCode == HttpStatusCode.OK && fake.Saved == new SaveReview("", "v1") && fake.Subject == "validated-reviewer", "Review binding failed");
});
await Check("Draft request and status preserve synchronous contract", async () =>
{
    using var requested = await server.Client.PostAsJsonAsync($"/api/books/{bookId}/drafts", new { requestId = "request-key" });
    Assert(requested.StatusCode == HttpStatusCode.OK && fake.RequestKey == "request-key", "Draft request binding failed");
    using var status = await server.Client.GetAsync($"/api/books/{bookId}/drafts/{operationId}");
    Assert(status.StatusCode == HttpStatusCode.OK && (await status.Content.ReadFromJsonAsync<DraftOperation>())?.Id == operationId, "Draft status failed");
});
await Check("Every service failure maps to sanitized status", async () =>
{
    foreach (var item in new[] { (ErrorCode.InvalidInput,400), (ErrorCode.NotFound,404), (ErrorCode.AccessDenied,403), (ErrorCode.Conflict,409), (ErrorCode.DependencyUnavailable,503), ((ErrorCode)999,500) })
    {
        fake.Error = item.Item1;
        await Status(await server.Client.GetAsync($"/api/books/{bookId}"), (HttpStatusCode)item.Item2);
    }
    fake.Error = null;
});
await Check("Malformed and incomplete JSON return 400 without service work", async () =>
{
    var before = fake.Calls;
    foreach (var body in new[] { "{", "null", "{}", "{\"reviewedText\":null,\"expectedVersion\":\"v1\"}" })
        await Status(await server.Client.PutAsync($"/api/books/{bookId}/paragraphs/{paragraphId}/review", new StringContent(body, Encoding.UTF8, "application/json")), HttpStatusCode.BadRequest);
    Assert(fake.Calls == before, "Invalid body invoked service");
});
await Check("Unsupported content types and malformed multipart are rejected", async () =>
{
    await Status(await server.Client.PostAsync($"/api/books/{bookId}/drafts", new StringContent("request-key")), HttpStatusCode.UnsupportedMediaType);
    await Status(await server.Client.PostAsync("/api/books", new StringContent("hello")), HttpStatusCode.UnsupportedMediaType);
    using var multipart = new MultipartFormDataContent();
    multipart.Add(new StringContent("title"), "title");
    await Status(await server.Client.PostAsync("/api/books", multipart), HttpStatusCode.BadRequest);
});
await Check("Unhandled dependency exception returns sanitized 500", async () =>
{
    foreach (var error in new Exception[] { new Exception("secret-sentinel"), new System.Text.Json.JsonException("secret-sentinel"), new InvalidDataException("secret-sentinel") })
    {
        fake.Exception = error;
        await Status(await server.Client.GetAsync($"/api/books/{bookId}"), HttpStatusCode.InternalServerError);
    }
    fake.Exception = null;
});
await Check("Invalid route identifier does not invoke business service", async () =>
{
    var before = fake.Calls;
    await Status(await server.Client.GetAsync("/api/books/not-a-guid"), HttpStatusCode.NotFound);
    Assert(before == fake.Calls, "Malformed route invoked service");
});
await Check("HTTP disconnect propagates cancellation to service", async () =>
{
    fake.Cancel = true;
    using var source = new CancellationTokenSource();
    var request = server.Client.GetAsync($"/api/books/{bookId}", source.Token);
    await fake.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
    source.Cancel();
    try { await request; throw new Exception("Client request was not cancelled"); }
    catch (OperationCanceledException) { }
    await fake.Cancelled.Task.WaitAsync(TimeSpan.FromSeconds(5));
});
Console.WriteLine($"{checks}/{checks} API contract checks passed");

sealed class TestIdentity { public ClaimsPrincipal Principal { get; set; } = new(new ClaimsIdentity()); }
sealed class Harness(WebApplication app, HttpClient client) : IAsyncDisposable
{
    public HttpClient Client => client;
    public static async Task<Harness> Start(Services? fake, TestIdentity identity)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        if (fake is not null)
        {
            builder.Services.AddSingleton<IBookService>(fake);
            builder.Services.AddSingleton<IReviewService>(fake);
            builder.Services.AddSingleton<IDraftTranslationService>(fake);
        }
        var app = builder.Build();
        app.Use((http, next) => { http.User = identity.Principal; return next(http); });
        app.MapBusinessApi();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        return new(app, new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) });
    }
    public async ValueTask DisposeAsync() { client.Dispose(); await app.StopAsync(); await app.DisposeAsync(); }
}
sealed class Services(Guid bookId, Guid paragraphId, Guid operationId) : IBookService, IReviewService, IDraftTranslationService
{
    public int Calls; public string? Subject; public CancellationToken LastToken;
    public ErrorCode? Error; public Exception? Exception; public bool Cancel;
    public TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Cancelled = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public string? ImportedTitle, ImportedName, ImportedMedia, RequestKey;
    public byte[]? ImportedBytes; public Stream? ImportedStream; public SaveReview? Saved;
    private ParagraphView Paragraph => new(paragraphId, 0, "source", null, Saved?.ReviewedText, "v2");
    private OperationResult<T> Result<T>(ReviewerContext reviewer, CancellationToken token, T value)
    {
        Calls++; Subject = reviewer.SubjectId; LastToken = token;
        if (Exception is not null) throw Exception;
        return Error is { } error ? new OperationResult<T>.Failure(error, "secret-sentinel") : new OperationResult<T>.Success(value);
    }
    public async Task<OperationResult<BookReview>> GetAsync(ReviewerContext reviewer, Guid id, CancellationToken cancellationToken = default)
    {
        AssertId(id, bookId);
        if (Cancel)
        {
            Entered.TrySetResult();
            try { await Task.Delay(Timeout.Infinite, cancellationToken); }
            catch (OperationCanceledException) { Cancelled.TrySetResult(); throw; }
        }
        return Result(reviewer, cancellationToken, new BookReview(new BookSummary(bookId, "title"), new[] { Paragraph }));
    }
    public async Task<OperationResult<BookSummary>> ImportAsync(ReviewerContext reviewer, ImportBook request, CancellationToken cancellationToken = default)
    {
        ImportedTitle = request.Title; ImportedName = request.FileName; ImportedMedia = request.MediaType; ImportedStream = request.Content;
        using var bytes = new MemoryStream(); await request.Content.CopyToAsync(bytes, cancellationToken); ImportedBytes = bytes.ToArray();
        return Result(reviewer, cancellationToken, new BookSummary(bookId, request.Title));
    }
    public Task<OperationResult<ParagraphView>> SaveAsync(ReviewerContext reviewer, Guid book, Guid paragraph, SaveReview request, CancellationToken cancellationToken = default)
    {
        AssertId(book, bookId); AssertId(paragraph, paragraphId); Saved = request;
        return Task.FromResult(Result(reviewer, cancellationToken, Paragraph));
    }
    public Task<OperationResult<DraftOperation>> RequestAsync(ReviewerContext reviewer, Guid book, string requestId, CancellationToken cancellationToken = default)
    {
        AssertId(book, bookId); RequestKey = requestId;
        return Task.FromResult(Result(reviewer, cancellationToken, new DraftOperation(operationId, DraftState.Succeeded, null)));
    }
    public Task<OperationResult<DraftOperation>> GetStatusAsync(ReviewerContext reviewer, Guid book, Guid operation, CancellationToken cancellationToken = default)
    {
        AssertId(book, bookId); AssertId(operation, operationId);
        return Task.FromResult(Result(reviewer, cancellationToken, new DraftOperation(operationId, DraftState.Succeeded, null)));
    }
    private static void AssertId(Guid actual, Guid expected) { if (actual != expected) throw new Exception("Identifier propagation failed"); }
}
