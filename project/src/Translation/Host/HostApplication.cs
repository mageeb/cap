using Translation.Host;

public static class HostApplication
{
    public static WebApplication Build(string[] args, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceTelemetry();
        configure?.Invoke(builder);
        var app = builder.Build();
        app.UseRouting();
        app.UseSafeRequestTelemetry();
        app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
        // Fail closed until production identity and business dependencies are configured.
        app.MapGet("/health/ready", () => Results.Json(
            new { status = "not_ready", reason = "business_dependencies_not_configured" },
            statusCode: StatusCodes.Status503ServiceUnavailable));
        app.MapBusinessApi();
        return app;
    }
}
