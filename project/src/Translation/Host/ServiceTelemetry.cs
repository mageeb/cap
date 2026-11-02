using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Routing;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Translation.Host;

public static class ServiceTelemetry
{
    public const string ServiceName = "translation-backend";
    public const string MeterName = "Translation.Host";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> Requests = Meter.CreateCounter<long>("translation.http.requests", "{request}");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("translation.http.request.duration", "s");
    private static readonly HashSet<string> Methods = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS", "TRACE", "CONNECT"];

    public static void AddServiceTelemetry(this WebApplicationBuilder builder)
    {
        var endpointValue = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        Uri? endpoint = null;
        if (!string.IsNullOrWhiteSpace(endpointValue) &&
            (!Uri.TryCreate(endpointValue, UriKind.Absolute, out endpoint) ||
             (endpoint.Scheme != "http" && endpoint.Scheme != "https") ||
             !string.IsNullOrEmpty(endpoint.UserInfo) || !string.IsNullOrEmpty(endpoint.Query) ||
             !string.IsNullOrEmpty(endpoint.Fragment)))
            throw new InvalidOperationException("OTEL_EXPORTER_OTLP_ENDPOINT must be an HTTP(S) base URL without credentials, query or fragment.");

        builder.Logging.ClearProviders();
        // Framework request/error messages can include attacker-controlled URLs or exception text.
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.None);
        builder.Logging.AddFilter("System.Net.Http", LogLevel.None);
        builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
        builder.Services.Configure<LoggerFactoryOptions>(options =>
            options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);
        var resource = ResourceBuilder.CreateDefault().AddService(ServiceName);
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resource);
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            if (endpoint is not null)
                options.AddOtlpExporter(exporter => ConfigureExporter(exporter, endpoint, "logs"));
        });
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(ServiceName))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context => !IsHealth(context.Request.Path);
                    options.RecordException = false;
                });
                tracing.AddHttpClientInstrumentation(options => options.RecordException = false);
                tracing.AddProcessor(new SafeTraceProcessor());
                if (endpoint is not null)
                    tracing.AddOtlpExporter(exporter => ConfigureExporter(exporter, endpoint, "traces"));
            })
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(MeterName).AddRuntimeInstrumentation();
                if (endpoint is not null)
                    metrics.AddOtlpExporter(exporter => ConfigureExporter(exporter, endpoint, "metrics"));
            });
    }

    private static void ConfigureExporter(OtlpExporterOptions exporter, Uri endpoint, string signal)
    {
        exporter.Endpoint = new Uri(endpoint.AbsoluteUri.TrimEnd('/') + "/v1/" + signal);
        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
        exporter.TimeoutMilliseconds = 2000;
    }

    public static void UseSafeRequestTelemetry(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (IsHealth(context.Request.Path)) { await next(context); return; }
            var start = Stopwatch.GetTimestamp();
            var failed = false;
            try { await next(context); }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                failed = true;
                context.Response.StatusCode = 499;
            }
            catch (Exception)
            {
                failed = true;
                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { error = "internal_error" });
                }
                else context.Abort();
            }
            finally
            {
                var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched";
                var method = SafeMethod(context.Request.Method);
                var status = failed ? (context.RequestAborted.IsCancellationRequested ? 499 : 500) : context.Response.StatusCode;
                var elapsed = Stopwatch.GetElapsedTime(start).TotalSeconds;
                var tags = new TagList { { "http.request.method", method }, { "http.route", route }, { "http.response.status_code", status } };
                Requests.Add(1, tags);
                Duration.Record(elapsed, tags);
                app.Logger.LogInformation("HTTP {Method} {Route} completed {StatusCode} in {DurationMs} ms; trace {TraceId} span {SpanId}",
                    method, route, status, elapsed * 1000, Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());
            }
        });
    }

    private static bool IsHealth(PathString path) => path.StartsWithSegments("/health");
    private static string SafeMethod(string method) => Methods.Contains(method) ? method : "_OTHER";

    private sealed class SafeTraceProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            var method = SafeMethod(activity.GetTagItem("http.request.method")?.ToString() ?? "_OTHER");
            var route = activity.GetTagItem("http.route")?.ToString();
            var status = activity.GetTagItem("http.response.status_code");
            foreach (var tag in activity.TagObjects.ToArray()) activity.SetTag(tag.Key, null);
            activity.SetTag("http.request.method", method);
            if (status is not null) activity.SetTag("http.response.status_code", status);
            if (activity.Kind == ActivityKind.Server) activity.SetTag("http.route", route ?? "unmatched");
            activity.DisplayName = activity.Kind == ActivityKind.Server ? $"{method} {route ?? "unmatched"}" : method;
            activity.TraceStateString = null;
            activity.SetStatus(activity.Status);
        }
    }
}
