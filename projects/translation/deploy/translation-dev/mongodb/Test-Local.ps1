param([string]$Image = 'mongo:8.0', [string]$SampleDirectory)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'New-MongoTls.ps1')
$dockerDirectory = 'C:\Users\HanyG\AppData\Local\Programs\DockerDesktop\resources\bin'
$dockerExecutable = Join-Path $dockerDirectory 'docker.exe'
$env:PATH = "$dockerDirectory;$env:PATH"
$tlsDirectory = Join-Path ([IO.Path]::GetTempPath()) ('translation-mongodb-test-' + [guid]::NewGuid().ToString('N'))
$testContainer = $null
try {
    New-MongoTls -Directory $tlsDirectory -DockerExecutable $dockerExecutable -Image $Image
    $randomBytes = New-Object byte[] 756
    $random = [Security.Cryptography.RandomNumberGenerator]::Create()
    $random.GetBytes($randomBytes)
    $random.Dispose()
    $replicaKey = [Convert]::ToBase64String($randomBytes)
    $rootPassword = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
    $appPassword = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
    $serverScript = 'printf %s "$MONGODB_KEYFILE" > /keyfile/keyfile; chmod 400 /keyfile/keyfile; exec /usr/local/bin/docker-entrypoint.sh mongod --replSet rs0 --bind_ip_all --keyFile /keyfile/keyfile --wiredTigerCacheSizeGB 0.25 --tlsMode requireTLS --tlsCertificateKeyFile /tls/server.pem --tlsCAFile /tls/ca.crt --tlsAllowConnectionsWithoutCertificates'
    $testContainer = & $dockerExecutable --context desktop-linux run -d --rm --user 999:999 --read-only --cap-drop ALL --security-opt no-new-privileges --memory 1g --tmpfs /data/db:rw,uid=999,gid=999 --tmpfs /data/configdb:rw,uid=999,gid=999 --tmpfs /tmp:rw,uid=999,gid=999 --tmpfs /keyfile:rw,uid=999,gid=999,mode=0700 --mount "type=bind,source=$tlsDirectory,target=/tls,readonly" --mount "type=bind,source=$PSScriptRoot,target=/schema,readonly" -e HOME=/tmp -e MONGO_INITDB_ROOT_USERNAME=mongo_admin -e "MONGO_INITDB_ROOT_PASSWORD=$rootPassword" -e "MONGODB_APP_PASSWORD=$appPassword" -e MONGODB_HOST=localhost -e MONGODB_TLS_CA=/tls/ca.crt -e "MONGODB_KEYFILE=$replicaKey" --entrypoint bash $Image -ec $serverScript
    if ($LASTEXITCODE -ne 0) { throw 'MongoDB local container startup failed' }
    Start-Sleep -Seconds 3
    & $dockerExecutable --context desktop-linux exec $testContainer mongosh --quiet --nodb --file /schema/bootstrap.js --file /schema/verify.js
    if ($LASTEXITCODE -ne 0) { throw 'Local MongoDB component verification failed' }
    if ($SampleDirectory) {
        $samplePayload = Join-Path $tlsDirectory 'romans-chapter01.json'
        & (Join-Path $PSScriptRoot 'New-SamplePayload.ps1') -SourceDirectory $SampleDirectory -OutputPath $samplePayload
        for ($importPass = 0; $importPass -lt 2; $importPass++) {
            & $dockerExecutable --context desktop-linux exec -e MONGODB_IMPORT_FILE=/tls/romans-chapter01.json $testContainer mongosh --quiet --nodb --file /schema/import-sample.js
            if ($LASTEXITCODE -ne 0) { throw 'Sample import or idempotency verification failed' }
        }
    }
    Write-Output 'PASS: real MongoDB schema and transaction tests with TLS, auth, UID 999 and read-only root filesystem'
} finally {
    if ($testContainer) { & $dockerExecutable --context desktop-linux stop $testContainer | Out-Null }
    if (Test-Path -LiteralPath $tlsDirectory) { Remove-MongoTemporaryDirectory -Directory $tlsDirectory }
}
