param(
    [Parameter(Mandatory = $true)][string]$Image,
    [Parameter(Mandatory = $true)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
if ($Image -notmatch '^[a-zA-Z0-9][a-zA-Z0-9._:/-]*@sha256:[a-f0-9]{64}$') {
    throw 'Image must be a registry image reference with a real sha256 digest.'
}
$template = Get-Content (Join-Path $PSScriptRoot 'kubernetes/backend.yaml') -Raw
$placeholder = 'image: translation-backend:local'
if ([regex]::Matches($template, [regex]::Escape($placeholder)).Count -ne 1) {
    throw 'Expected exactly one template image placeholder.'
}
$rendered = $template.Replace($placeholder, "image: $Image")
[System.IO.File]::WriteAllText($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath), $rendered, [System.Text.UTF8Encoding]::new($false))
