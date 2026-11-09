param(
    [string]$SourceDirectory = 'C:\Repos\Translation\AI-Assisted-Translation\Books\Romans - Abouna Antonios Fekry\Translation\Chapter01',
    [string]$Kubeconfig = 'C:\Users\HanyG\AppData\Local\Temp\translation-dev.kubeconfig'
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'New-MongoTls.ps1')
$toolsDirectory = 'C:\Users\HanyG\AppData\Local\Temp\translation-dev-aks-tools'
$kubectl = Join-Path $toolsDirectory 'kubectl.exe'
$env:PATH = "$toolsDirectory\kubelogin-package\bin\windows_amd64;$env:PATH"
$OutputEncoding = [Text.UTF8Encoding]::new($false)
$payloadDirectory = Join-Path ([IO.Path]::GetTempPath()) ('translation-mongodb-import-'+[guid]::NewGuid().ToString('N'))
$payloadStaged = $false
New-Item -ItemType Directory -Path $payloadDirectory | Out-Null
try {
    & $kubectl --kubeconfig $Kubeconfig --context translation-dev-aks -n translation-dev wait --for=condition=Ready pod/mongodb-0 --timeout=45s
    if ($LASTEXITCODE -ne 0) { throw 'DEV MongoDB is not ready' }
    $payloadPath = Join-Path $payloadDirectory 'romans-chapter01.json'
    & (Join-Path $PSScriptRoot 'New-SamplePayload.ps1') -SourceDirectory $SourceDirectory -OutputPath $payloadPath
    Get-Content -LiteralPath $payloadPath -Raw -Encoding utf8 | & $kubectl --kubeconfig $Kubeconfig --context translation-dev-aks -n translation-dev exec -i mongodb-0 -c mongodb -- sh -c 'cat > /tmp/romans-chapter01.json'
    if ($LASTEXITCODE -ne 0) { throw 'Cloud sample payload transfer failed' }
    $payloadStaged = $true
    & $kubectl --kubeconfig $Kubeconfig --context translation-dev-aks -n translation-dev exec mongodb-0 -c mongodb -- mongosh --quiet --nodb --file /schema/import-sample.js
    if ($LASTEXITCODE -ne 0) { throw 'Cloud sample import/verification failed' }
} finally {
    if ($payloadStaged) {
        & $kubectl --kubeconfig $Kubeconfig --context translation-dev-aks -n translation-dev exec mongodb-0 -c mongodb -- sh -c 'rm -f /tmp/romans-chapter01.json'
        if ($LASTEXITCODE -ne 0) { Write-Warning 'Could not remove the temporary payload from the pod; inspect /tmp/romans-chapter01.json.' }
    }
    Remove-MongoTemporaryDirectory -Directory $payloadDirectory
}
