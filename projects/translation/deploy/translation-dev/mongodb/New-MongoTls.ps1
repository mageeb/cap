function New-MongoTls {
    param([string]$Directory, [string]$DockerExecutable, [string]$Image = 'mongo:8.0')
    New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    $tlsScript = 'set -eu; cd /certs; openssl req -x509 -newkey rsa:3072 -sha256 -nodes -days 365 -subj /CN=Translation-DEV-MongoDB-CA -keyout ca.key -out ca.crt >/dev/null 2>&1; openssl req -newkey rsa:3072 -nodes -subj /CN=mongodb.translation-dev.svc.cluster.local -keyout server.key -out server.csr >/dev/null 2>&1; printf "%s\n" "subjectAltName=DNS:localhost,IP:127.0.0.1,DNS:mongodb,DNS:mongodb.translation-dev,DNS:mongodb.translation-dev.svc,DNS:mongodb.translation-dev.svc.cluster.local,DNS:mongodb-0.mongodb.translation-dev.svc.cluster.local" "extendedKeyUsage=serverAuth,clientAuth" > server.ext; openssl x509 -req -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out server.crt -days 365 -sha256 -extfile server.ext >/dev/null 2>&1; cat server.crt server.key > server.pem'
    & $DockerExecutable --context desktop-linux run --rm --entrypoint bash --mount "type=bind,source=$Directory,target=/certs" $Image -ec $tlsScript
    if ($LASTEXITCODE -ne 0) { throw 'MongoDB TLS certificate creation failed' }
}

function Remove-MongoTemporaryDirectory {
    param([string]$Directory)
    $resolvedDirectory = [IO.Path]::GetFullPath($Directory)
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (-not $resolvedDirectory.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Refusing cleanup outside the temporary directory' }
    Remove-Item -LiteralPath $resolvedDirectory -Recurse -Force
}
