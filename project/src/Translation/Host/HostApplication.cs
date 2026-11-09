using Translation.Host;

public static class HostApplication
{
    public static WebApplication Build(string[] args, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceTelemetry();
        configure?.Invoke(builder);
        builder.AddDevDocuments();
        var app = builder.Build();
        app.UseRouting();
        app.UseSafeRequestTelemetry();
        app.UseDevReviewer();
        app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
        app.MapGet("/health/ready", (Func<HttpContext, Task<IResult>>)DevDocuments.Readiness);
        app.MapBusinessApi();
        return app;
    }
}
