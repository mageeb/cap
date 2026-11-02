using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

Environment.SetEnvironmentVariable("OTEL_BLRP_SCHEDULE_DELAY", "100");
var originalOutput = Console.Out;
var capturedOutput = new StringWriter();
Console.SetOut(capturedOutput);
try
{
    // Configuration must fail without reflecting a credential-bearing value.
    const string secret = "PRIVATE_MARKER_5f390a";
    try
    {
        await using var invalid = HostApplication.Build(["--OTEL_EXPORTER_OTLP_ENDPOINT", $"http://user:{secret}@localhost:4318"]);
        throw new Exception("Credential-bearing endpoint was accepted.");
    }
    catch (InvalidOperationException error)
    {
        Check(!error.ToString().Contains(secret), "Invalid config disclosed credentials.");
    }

    // Export is optional. No backend configuration must preserve HTTP behavior.
    await using (var standalone = HostApplication.Build(["--urls", "http://127.0.0.1:0"]))
    {
        await standalone.StartAsync();
        using var client = new HttpClient();
        var root = Address(standalone);
        Check((int)(await client.GetAsync(root + "/health/live")).StatusCode == 200, "Standalone liveness failed.");
        Check((int)(await client.GetAsync(root + "/health/ready")).StatusCode == 503, "Standalone readiness changed.");
        await standalone.StopAsync();
    }

    await using var collector = new Collector();
    await using (var app = HostApplication.Build(["--urls", "http://127.0.0.1:0", "--OTEL_EXPORTER_OTLP_ENDPOINT", collector.Endpoint]))
    {
        app.MapGet("/test/{id}", () => Results.Ok());
        app.MapGet("/failure/{id}", (HttpContext _) => throw new InvalidOperationException(secret));
        app.MapGet("/outbound", async () =>
        {
            using var downstream = new HttpClient();
            await downstream.GetAsync(collector.Endpoint + "/" + secret + "?token=" + secret);
            return Results.Ok();
        });
        app.MapGet("/outbound-failure", async () =>
        {
            using var downstream = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            try { using var response = await downstream.GetAsync(collector.Endpoint + "/" + secret + "/failed-dependency?token=" + secret); response.EnsureSuccessStatusCode(); }
            catch (HttpRequestException) { return Results.StatusCode(503); }
            return Results.Ok();
        });
        await app.StartAsync();
        using var client = new HttpClient();
        var root = Address(app);
        await client.GetAsync(root + "/health/live?token=" + secret);
        await client.GetAsync(root + "/health/ready");
        Flush(app);
        var firstSignals = collector.Snapshot();
        Check(!firstSignals.Any(s => s.Text.Contains("/health")), "Health routes entered telemetry.");
        collector.Clear();

        const string traceId = "1234567890abcdef1234567890abcdef";
        using var request = new HttpRequestMessage(HttpMethod.Get, root + "/test/" + secret + "?token=" + secret);
        request.Headers.Add("traceparent", "00-" + traceId + "-1234567890abcdef-01");
        request.Headers.Add("tracestate", "private=" + secret);
        request.Headers.Add("baggage", "private=" + secret);
        request.Headers.Authorization = new("Bearer", secret);
        using (SuppressInstrumentationScope.Begin())
            Check((int)(await client.SendAsync(request)).StatusCode == 200, "Test route failed.");
        Check((int)(await client.GetAsync(root + "/unknown/" + secret + "?q=" + secret)).StatusCode == 404, "Unknown route changed.");
        using var methodRequest = new HttpRequestMessage(new HttpMethod(secret), root + "/" + secret);
        await client.SendAsync(methodRequest);
        Check((int)(await client.GetAsync(root + "/failure/" + secret)).StatusCode == 500, "Failure was not sanitized.");
        using var outboundRequest = new HttpRequestMessage(HttpMethod.Get, root + "/outbound");
        outboundRequest.Headers.Add("traceparent", "00-" + traceId + "-1234567890abcdef-01");
        using (SuppressInstrumentationScope.Begin())
            Check((int)(await client.SendAsync(outboundRequest)).StatusCode == 200, "Outbound request failed.");
        Check((int)(await client.GetAsync(root + "/outbound-failure")).StatusCode == 503, "Dependency failure changed.");
        Flush(app);
        await app.StopAsync();
        var signals = collector.Snapshot();
        foreach (var signal in new[] { "traces", "logs", "metrics" })
            Check(signals.Any(s => s.Path == "/v1/" + signal && s.Bytes.Length > 0), "Missing OTLP " + signal);
        Check(signals.All(s => !s.Text.Contains(secret)), "Sensitive request data entered OTLP.");
        Check(signals.Any(s => s.Path.Contains(secret) && s.Parent?.Contains(traceId) == true), "Outbound trace context was not propagated.");
        var logPayload = string.Join("", signals.Where(s => s.Path == "/v1/logs").Select(s => s.Text));
        Check(logPayload.Contains(traceId), "Log trace correlation missing.");
        Check(logPayload.Contains("/test/{id}"), "Route template missing from logs.");
        Check(logPayload.Contains("unmatched") && logPayload.Contains("_OTHER"), "Unbounded path/method labels.");
        var tracePayload = string.Join("", signals.Where(s => s.Path == "/v1/traces").Select(s => s.Text));
        Check(tracePayload.Contains("/test/{id}"), "Server trace route missing.");
        Check(tracePayload.Contains("System.Net.Http"), "HttpClient trace scope missing.");
        Check(signals.Where(s => s.Path == "/v1/traces").Any(s => Contains(s.Bytes, Convert.FromHexString(traceId))), "Inbound trace ID not exported.");
        var metricPayload = string.Join("", signals.Where(s => s.Path == "/v1/metrics").Select(s => s.Text));
        Check(metricPayload.Contains("translation.http.requests") && metricPayload.Contains("translation.http.request.duration"), "Request metrics missing.");
        Check(metricPayload.Contains("process.runtime.dotnet") || metricPayload.Contains("dotnet."), "Runtime metrics missing.");
    }
    // An unavailable collector must not turn telemetry into an HTTP dependency.
    await using (var unavailable = HostApplication.Build(["--urls", "http://127.0.0.1:0", "--OTEL_EXPORTER_OTLP_ENDPOINT", "http://127.0.0.1:1"]))
    {
        await unavailable.StartAsync();
        using var client = new HttpClient();
        Check((int)(await client.GetAsync(Address(unavailable) + "/missing")).StatusCode == 404, "Exporter failure broke HTTP.");
        await unavailable.StopAsync();
    }
    Check(!capturedOutput.ToString().Contains(secret), "Sensitive data entered console logs.");
    Check(capturedOutput.ToString().Contains("\"TraceId\""), "Structured console trace field missing.");
}
finally { Console.SetOut(originalOutput); }
Console.WriteLine("PASS configuration/privacy validation");
Console.WriteLine("PASS no-exporter HTTP behavior and readiness");
Console.WriteLine("PASS OTLP logs/traces/request and runtime metrics received");
Console.WriteLine("PASS route/method bounds, trace correlation and outbound privacy");
Console.WriteLine("PASS health filtering and sanitized failure");
Console.WriteLine("PASS unavailable exporter preserves HTTP");

static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
static string Address(WebApplication app) => app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
static void Flush(WebApplication app)
{
    Check(app.Services.GetRequiredService<TracerProvider>().ForceFlush(5000), "Trace flush failed.");
    Check(app.Services.GetRequiredService<MeterProvider>().ForceFlush(5000), "Metric flush failed.");
    Thread.Sleep(400); // Log exporter batches independently; test schedule is 100ms.
}
static bool Contains(byte[] data, byte[] needle)
{
    for (var index = 0; index <= data.Length - needle.Length; index++)
        if (data.AsSpan(index, needle.Length).SequenceEqual(needle)) return true;
    return false;
}

sealed class Collector : IAsyncDisposable
{
    private readonly HttpListener listener = new();
    private readonly ConcurrentQueue<Signal> received = new();
    private readonly Task loop;
    public string Endpoint { get; }
    public Collector()
    {
        var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        Endpoint = "http://127.0.0.1:" + port;
        listener.Prefixes.Add(Endpoint + "/");
        listener.Start();
        loop = Task.Run(async () =>
        {
            while (listener.IsListening)
            {
                HttpListenerContext context;
                try { context = await listener.GetContextAsync(); }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                using var body = new MemoryStream();
                await context.Request.InputStream.CopyToAsync(body);
                received.Enqueue(new Signal(context.Request.Url!.AbsolutePath, body.ToArray(), context.Request.Headers["traceparent"]));
                context.Response.StatusCode = context.Request.Url!.AbsolutePath.EndsWith("/failed-dependency") ? 503 : 200;
                context.Response.ContentType = "application/x-protobuf";
                context.Response.Close();
            }
        });
    }
    public Signal[] Snapshot() => received.ToArray();
    public void Clear() { while (received.TryDequeue(out _)) { } }
    public async ValueTask DisposeAsync() { listener.Close(); await loop.WaitAsync(TimeSpan.FromSeconds(5)); }
}
sealed record Signal(string Path, byte[] Bytes, string? Parent)
{
    public string Text => Encoding.UTF8.GetString(Bytes);
}
