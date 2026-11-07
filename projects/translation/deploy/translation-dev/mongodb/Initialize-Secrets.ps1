param(
    [string]$Kubeconfig = 'C:\Users\HanyG\AppData\Local\Temp\translation-dev.kubeconfig',
    [string]$Image = 'mongo:8.0'
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'New-MongoTls.ps1')
$kubectl = 'C:\Users\HanyG\AppData\Local\Temp\translation-dev-aks-tools\kubectl.exe'
$env:PATH = 'C:\Users\HanyG\AppData\Local\Temp\translation-dev-aks-tools\kubelogin-package\bin\windows_amd64;' + $env:PATH
$dockerDirectory = 'C:\Users\HanyG\AppData\Local\Programs\DockerDesktop\resources\bin'
$env:PATH = "$dockerDirectory;$env:PATH"
function Test-Secret([string]$Name) {
    $existing = & $kubectl --kubeconfig $Kubeconfig --context translation-dev-aks -n translation-dev get secret $Name --ignore-not-found -o name
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect MongoDB credentials' }
    return -not [string]::IsNullOrWhiteSpace($existing)
}
function Write-Secret([string]$Name, [hashtable]$Values) {
    $secret = @{ apiVersion='v1';kind='Secret';metadata=@{name=$Name;namespace='translation-dev'};type='Opaque';stringData=$Values } | ConvertTo-Json -Depth 6
    $secret | & $kubectl --kubeconfig $Kubeconfig --context translation-dev-aks apply -f -
    if ($LASTEXITCODE -ne 0) { throw 'MongoDB credential provisioning failed' }
}
if (-not (Test-Secret 'translation-mongodb-auth')) {
    Write-Secret 'translation-mongodb-auth' @{'root-username'='mongo_admin';'root-password'=([guid]::NewGuid().ToString('N')+[guid]::NewGuid().ToString('N'))}
}
if (-not (Test-Secret 'translation-mongodb-app')) {
    $applicationPassword = [guid]::NewGuid().ToString('N')+[guid]::NewGuid().ToString('N')
    Write-Secret 'translation-mongodb-app' @{'app-username'='translation_app';'app-password'=$applicationPassword;'connection-string'="mongodb://translation_app:${applicationPassword}@mongodb-0.mongodb.translation-dev.svc.cluster.local:27017/translation_review?authSource=translation_review&replicaSet=rs0&tls=true"}
}
if (-not (Test-Secret 'translation-mongodb-keyfile')) {
    $keyBytes = New-Object byte[] 756
    $generator = [Security.Cryptography.RandomNumberGenerator]::Create()
    $generator.GetBytes($keyBytes)
    $generator.Dispose()
    Write-Secret 'translation-mongodb-keyfile' @{keyfile=[Convert]::ToBase64String($keyBytes)}
}
if (-not (Test-Secret 'translation-mongodb-tls')) {
    $tlsDirectory = Join-Path ([IO.Path]::GetTempPath()) ('translation-mongodb-tls-'+[guid]::NewGuid().ToString('N'))
    try {
        New-MongoTls -Directory $tlsDirectory -DockerExecutable (Join-Path $dockerDirectory 'docker.exe') -Image $Image
        Write-Secret 'translation-mongodb-tls' @{'ca.crt'=[IO.File]::ReadAllText((Join-Path $tlsDirectory 'ca.crt'));'server.pem'=[IO.File]::ReadAllText((Join-Path $tlsDirectory 'server.pem'))}
    } finally { if (Test-Path -LiteralPath $tlsDirectory) { Remove-MongoTemporaryDirectory -Directory $tlsDirectory } }
}
Write-Output 'MongoDB secrets initialized; existing secrets preserved. No credentials written to source.'
