$ErrorActionPreference = 'Stop'
Push-Location (Resolve-Path "$PSScriptRoot/../../../../..")
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Coverage tool restore failed.' }
    $project = 'project/src/Translation/Backend/Tests/Translation.Backend.Tests.csproj'
    dotnet build $project
    if ($LASTEXITCODE -ne 0) { throw 'Test build failed.' }
    $assembly = 'project/src/Translation/Backend/Tests/bin/Debug/net8.0/Translation.Backend.Tests.dll'
    # Only unit execution contributes to this full-production-assembly coverage gate.
    dotnet tool run coverlet -- $assembly --target dotnet --targetargs "$assembly unit" `
        --include '[Translation.Backend]*' --format cobertura --format json `
        --output outputs/coverage/unit --threshold 80 --threshold-type line --threshold-type branch
    if ($LASTEXITCODE -ne 0) { throw 'Unit tests or 80% line/branch coverage gate failed.' }
    dotnet $assembly component
    if ($LASTEXITCODE -ne 0) { throw 'Component tests failed.' }
}
finally {
    Pop-Location
}
