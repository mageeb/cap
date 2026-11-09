param([ValidateRange(1024, 65535)][int]$Port = 18081)
$ErrorActionPreference = 'Stop'
$toolsDirectory = Join-Path $env:TEMP 'translation-dev-aks-tools'
$kubectl = Join-Path $toolsDirectory 'kubectl.exe'
$loginDirectory = Join-Path $toolsDirectory 'kubelogin-package\bin\windows_amd64'
$kubeconfig = Join-Path $env:TEMP 'translation-dev.kubeconfig'
foreach ($requiredPath in @($kubectl, (Join-Path $loginDirectory 'kubelogin.exe'), $kubeconfig)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Missing deployment client or credentials: $requiredPath. See deployment-status.md."
    }
}
$env:PATH = "$loginDirectory;$env:PATH"
Write-Host "Forwarding AKS backend to http://127.0.0.1:$Port. Keep this terminal open; Ctrl+C stops forwarding."
# Forward directly to the deployment for diagnostics while readiness is false.
& $kubectl --kubeconfig $kubeconfig --context translation-dev-aks -n translation-dev port-forward deployment/translation-backend "${Port}:8080" --address 127.0.0.1
if ($LASTEXITCODE -ne 0) { throw 'Port-forward failed.' }
