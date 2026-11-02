param(
    [Parameter(Mandatory = $true)][string]$PublishedDirectory,
    [int]$Port = 18080
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$assembly = Join-Path (Resolve-Path $PublishedDirectory) 'Translation.Host.dll'
if (!(Test-Path $assembly)) { throw "Publish the host before running this check." }
# Refuse a busy port so a different server cannot produce a false positive.
$listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
$listener.Start()
$listener.Stop()
$process = Start-Process dotnet -ArgumentList @('"' + $assembly + '"', '--urls', "http://127.0.0.1:$Port") -WindowStyle Hidden -PassThru
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(2)
try {
    $base = "http://127.0.0.1:$Port"
    $started = $false
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if ($process.HasExited) { throw "Host exited before startup." }
        try {
            $response = $client.GetAsync("$base/health/live").GetAwaiter().GetResult()
            $started = $response.IsSuccessStatusCode
            $response.Dispose()
            if ($started) { break }
        } catch { }
        Start-Sleep -Milliseconds 250
    }
    if (!$started) { throw 'Host did not become live.' }
    foreach ($case in @(
        @{ Path = '/health/live'; Code = 200; Status = 'alive' },
        @{ Path = '/health/ready'; Code = 503; Status = 'not_ready' },
        @{ Path = '/books'; Code = 404; Status = $null },
        @{ Path = '/api/books/00000000-0000-0000-0000-000000000001'; Code = 503; Status = $null }
    )) {
        $response = $client.GetAsync($base + $case.Path).GetAwaiter().GetResult()
        try {
            if ([int]$response.StatusCode -ne $case.Code) { throw "Wrong status for $($case.Path): $($response.StatusCode)" }
            if ($case.Status) {
                $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
                if ($body.status -ne $case.Status) { throw "Wrong body for $($case.Path)" }
                if ($case.Code -eq 503 -and $body.reason -ne 'business_dependencies_not_configured') { throw 'Wrong readiness reason.' }
            }
            Write-Output "PASS $($case.Path): $($case.Code)"
        } finally { $response.Dispose() }
    }
} finally {
    $client.Dispose()
    if (!$process.HasExited) { Stop-Process -Id $process.Id -Force }
    $process.Dispose()
}
