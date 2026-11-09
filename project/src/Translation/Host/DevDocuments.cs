using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Driver;
using Translation.Backend.Books;
using Translation.Backend.Contracts;
using Translation.Backend.Persistence;
using Translation.Backend.Reviews;

namespace Translation.Host;

// Explicit opt-in for the private DEV review workspace, not production authentication.
public sealed record DevReviewer(string Subject);

public static class DevDocuments
{
    public static void AddDevDocuments(this WebApplicationBuilder builder)
    {
        if (!builder.Configuration.GetValue<bool>("Translation:DevReview:Enabled")) return;
        if (builder.Configuration["Translation:DeploymentTier"] != "DEV" ||
            builder.Configuration["Translation:DevReview:ReviewerId"] != "sample:romans-antonios-fekry")
            throw new InvalidOperationException("DEV review requires the DEV tier and approved sample reviewer.");
        var uri = builder.Configuration["Translation:Mongo:ConnectionString"];
        var caPath = builder.Configuration["Translation:Mongo:CaFile"];
        if (string.IsNullOrWhiteSpace(uri) || string.IsNullOrWhiteSpace(caPath))
            throw new InvalidOperationException("DEV review requires MongoDB connection and CA configuration.");
        MongoClientSettings settings;
        X509Certificate2 ca;
        try
        {
            settings = MongoClientSettings.FromConnectionString(uri);
            if (!settings.UseTls || settings.AllowInsecureTls)
                throw new InvalidOperationException();
            ca = X509Certificate2.CreateFromPem(File.ReadAllText(caPath));
        }
        catch (Exception error) when (error is ArgumentException or FormatException or
            IOException or CryptographicException or InvalidOperationException or MongoConfigurationException)
        {
            // URI parsing exceptions can contain credentials. Never expose the original exception.
            throw new InvalidOperationException("Invalid DEV MongoDB TLS configuration.");
        }
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        settings.ConnectTimeout = TimeSpan.FromSeconds(5);
        settings.SocketTimeout = TimeSpan.FromSeconds(10);
        settings.SslSettings = new SslSettings
        {
            ServerCertificateValidationCallback = (_, certificate, _, errors) =>
            {
                if (certificate is null || (errors & (SslPolicyErrors.RemoteCertificateNameMismatch |
                    SslPolicyErrors.RemoteCertificateNotAvailable)) != 0) return false;
                using var chain = new X509Chain();
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(ca);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Private CA has no CRL service.
                chain.ChainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.1"));
                using var server = new X509Certificate2(certificate);
                return chain.Build(server);
            }
        };
        var database = new MongoClient(settings).GetDatabase("translation_review");
        builder.Services.AddSingleton(new DevReviewer("sample:romans-antonios-fekry"));
        builder.Services.AddSingleton(new MongoReviewStore(database));
        builder.Services.AddSingleton<IReviewStore>(sp => sp.GetRequiredService<MongoReviewStore>());
        builder.Services.AddSingleton<ISourceDocumentReader, UnconfiguredReader>();
        builder.Services.AddSingleton<IBookService, BookService>();
        builder.Services.AddSingleton<IReviewService, ReviewService>();
        // No translation provider is configured: draft routes remain unavailable.
    }

    public static void UseDevReviewer(this WebApplication app)
    {
        if (app.Services.GetService<DevReviewer>() is not { } reviewer) return;
        app.Use(async (context, next) =>
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", reviewer.Subject)], "explicit-private-dev-review"));
            await next(context);
        });
    }

    public static async Task<IResult> Readiness(HttpContext context)
    {
        if (context.RequestServices.GetService<DevReviewer>() is null ||
            context.RequestServices.GetService<MongoReviewStore>() is not { } store)
            return Results.Json(new { status = "not_ready", reason = "business_dependencies_not_configured" }, statusCode: 503);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        bool ready;
        try { ready = await store.CheckReadyAsync(timeout.Token); }
        catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested) { ready = false; }
        return ready ? Results.Ok(new { status = "ready", capabilities = new[] { "document-read", "review-save" } }) :
            Results.Json(new { status = "not_ready", reason = "document_storage_unavailable" }, statusCode: 503);
    }

    private sealed class UnconfiguredReader : ISourceDocumentReader
    {
        public Task<OperationResult<IReadOnlyList<SourceParagraph>>> ReadAsync(ImportBook input,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<OperationResult<IReadOnlyList<SourceParagraph>>>(
                new OperationResult<IReadOnlyList<SourceParagraph>>.Failure(
                    ErrorCode.DependencyUnavailable, "Import is not configured."));
        }
    }
}
