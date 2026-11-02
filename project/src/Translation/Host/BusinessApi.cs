using System.Security.Claims;
using System.Text.Json;
using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Drafts;
using Translation.Backend.Reviews;

namespace Translation.Host;

public static class BusinessApi
{
    public static void MapBusinessApi(this WebApplication app)
    {
        app.MapPost("/api/books", context => Execute(context, Import));
        app.MapGet("/api/books/{bookId:guid}", context => Execute(context, async (http, reviewer) =>
            Map(await http.RequestServices.GetRequiredService<IBookService>().GetAsync(
                reviewer, Id(http, "bookId"), http.RequestAborted))));
        app.MapPut("/api/books/{bookId:guid}/paragraphs/{paragraphId:guid}/review", context => Execute(context, Save));
        app.MapPost("/api/books/{bookId:guid}/drafts", context => Execute(context, RequestDraft));
        app.MapGet("/api/books/{bookId:guid}/drafts/{operationId:guid}", context => Execute(context, async (http, reviewer) =>
            Map(await http.RequestServices.GetRequiredService<IDraftTranslationService>().GetStatusAsync(
                reviewer, Id(http, "bookId"), Id(http, "operationId"), http.RequestAborted))));
    }

    private static async Task Execute(HttpContext http, Func<HttpContext, ReviewerContext, Task<IResult>> handler)
    {
        IResult result;
        var registrations = http.RequestServices.GetRequiredService<IServiceProviderIsService>();
        if (!registrations.IsService(typeof(IBookService)) || !registrations.IsService(typeof(IReviewService)) ||
            !registrations.IsService(typeof(IDraftTranslationService)))
            result = Problem(503, "business_dependencies_not_configured");
        else if (Reviewer(http.User) is not { } reviewer)
            result = Problem(401, "validated_identity_required");
        else
        {
            try { result = await handler(http, reviewer); }
            catch (OperationCanceledException) when (http.RequestAborted.IsCancellationRequested) { throw; }
            catch (RequestParseException error) { result = Problem(error.Status, "invalid_request"); }
            catch (Exception) { result = Problem(500, "operation_failed"); }
        }
        await result.ExecuteAsync(http);
    }

    private static ReviewerContext? Reviewer(ClaimsPrincipal principal)
    {
        // Only trusted authentication middleware may create an authenticated identity.
        var subjects = principal.Identities.Where(identity => identity.IsAuthenticated)
            .SelectMany(identity => identity.Claims)
            .Where(claim => claim.Type is "sub" or ClaimTypes.NameIdentifier)
            .Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray();
        return subjects.Length == 1 && !string.IsNullOrWhiteSpace(subjects[0])
            ? new ReviewerContext(subjects[0]) : null;
    }

    private static Guid Id(HttpContext http, string key) => Guid.Parse((string)http.Request.RouteValues[key]!);

    private static async Task<IResult> Import(HttpContext http, ReviewerContext reviewer)
    {
        if (http.Request.ContentType?.Split(';', 2)[0].Trim().Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase) != true) return Problem(415, "multipart_form_required");
        var form = await Parse(() => http.Request.ReadFormAsync(http.RequestAborted));
        if (form["title"].Count != 1 || string.IsNullOrWhiteSpace(form["title"]) || form.Files.Count != 1 ||
            form.Files[0].Name != "file" || string.IsNullOrWhiteSpace(form.Files[0].FileName))
            return Problem(400, "invalid_request");
        var file = form.Files[0];
        await using var content = file.OpenReadStream();
        var outcome = await http.RequestServices.GetRequiredService<IBookService>().ImportAsync(reviewer,
            new ImportBook(form["title"].ToString(), file.FileName, file.ContentType, content), http.RequestAborted);
        return outcome is OperationResult<BookSummary>.Success success
            ? Results.Created($"/api/books/{success.Value.Id:D}", success.Value) : Map(outcome);
    }

    private static async Task<IResult> Save(HttpContext http, ReviewerContext reviewer)
    {
        if (!http.Request.HasJsonContentType()) return Problem(415, "json_required");
        var request = await Parse(async () => await http.Request.ReadFromJsonAsync<SaveReview>(http.RequestAborted));
        if (request is null || request.ReviewedText is null || string.IsNullOrWhiteSpace(request.ExpectedVersion))
            return Problem(400, "invalid_request");
        return Map(await http.RequestServices.GetRequiredService<IReviewService>().SaveAsync(reviewer,
            Id(http, "bookId"), Id(http, "paragraphId"), request, http.RequestAborted));
    }

    private static async Task<IResult> RequestDraft(HttpContext http, ReviewerContext reviewer)
    {
        if (!http.Request.HasJsonContentType()) return Problem(415, "json_required");
        var request = await Parse(async () => await http.Request.ReadFromJsonAsync<DraftRequest>(http.RequestAborted));
        if (request is null || string.IsNullOrWhiteSpace(request.RequestId)) return Problem(400, "invalid_request");
        return Map(await http.RequestServices.GetRequiredService<IDraftTranslationService>().RequestAsync(
            reviewer, Id(http, "bookId"), request.RequestId, http.RequestAborted));
    }

    private static async Task<T> Parse<T>(Func<Task<T>> read)
    {
        try { return await read(); }
        catch (JsonException) { throw new RequestParseException(400); }
        catch (InvalidDataException) { throw new RequestParseException(400); }
        catch (BadHttpRequestException error) { throw new RequestParseException(error.StatusCode); }
    }

    private sealed class RequestParseException(int status) : Exception
    {
        public int Status => status;
    }
    private static IResult Map<T>(OperationResult<T> outcome) => outcome switch
    {
        OperationResult<T>.Success success => Results.Ok(success.Value),
        OperationResult<T>.Failure failure => failure.Code switch
        {
            ErrorCode.InvalidInput => Problem(400, "invalid_request"),
            ErrorCode.NotFound => Problem(404, "resource_not_found"),
            ErrorCode.AccessDenied => Problem(403, "access_denied"),
            ErrorCode.Conflict => Problem(409, "version_or_request_conflict"),
            ErrorCode.DependencyUnavailable => Problem(503, "dependency_unavailable"),
            _ => Problem(500, "operation_failed")
        },
        _ => Problem(500, "operation_failed")
    };

    private static IResult Problem(int status, string code) => Results.Problem(
        statusCode: status, title: code, extensions: new Dictionary<string, object?> { ["code"] = code });

    public sealed record DraftRequest(string RequestId);
}
