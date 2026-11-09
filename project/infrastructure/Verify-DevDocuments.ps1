param([string]$BaseUrl = 'http://127.0.0.1:4174',
    [Guid]$BookId = 'a784c44a-bb58-20a5-63fb-f8cc47a9d5dc')
$ErrorActionPreference = 'Stop'
$url = "$($BaseUrl.TrimEnd('/'))/api/books/$BookId"
$before = Invoke-RestMethod -Uri $url -TimeoutSec 15
$paragraph = $before.paragraphs | Where-Object { $null -ne $_.reviewedText } | Select-Object -First 1
if (-not $paragraph) { throw 'No reviewed paragraph available for a content-preserving save check.' }
$saveUrl = "$url/paragraphs/$($paragraph.id)/review"
$payload = @{ reviewedText = $paragraph.reviewedText; expectedVersion = $paragraph.version } | ConvertTo-Json -Compress
$saved = Invoke-RestMethod -Method Put -Uri $saveUrl -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($payload)) -TimeoutSec 15
if ($saved.version -eq $paragraph.version) { throw 'Save did not advance version.' }
$after = Invoke-RestMethod -Uri $url -TimeoutSec 15
$actual = $after.paragraphs | Where-Object { $_.id -eq $paragraph.id }
if ($actual.version -ne $saved.version -or $actual.reviewedText -cne $paragraph.reviewedText -or
    $actual.sourceText -cne $paragraph.sourceText -or $actual.machineDraft -cne $paragraph.machineDraft) {
    throw 'Saved content or protected source/draft fields did not match.'
}
$conflict = $false
try { Invoke-RestMethod -Method Put -Uri $saveUrl -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($payload)) -TimeoutSec 15 | Out-Null }
catch {
    if ($_.Exception.Response -and [int]$_.Exception.Response.StatusCode -eq 409) { $conflict = $true }
    else { throw }
}
if (-not $conflict) { throw 'Stale save was not rejected.' }
$final = Invoke-RestMethod -Uri $url -TimeoutSec 15
$finalParagraph = $final.paragraphs | Where-Object { $_.id -eq $paragraph.id }
if ($finalParagraph.version -ne $saved.version -or $finalParagraph.reviewedText -cne $paragraph.reviewedText) {
    throw 'Stale request changed the saved review.'
}
# Do not emit source or translation text. The save preserves text and advances only version/time.
@{ status = 'PASS'; bookId = "$BookId"; paragraphId = $paragraph.id;
    paragraphs = $final.paragraphs.Count; version = $saved.version;
    verified = 'read/save/reload/stale409/source-and-draft-preserved' } | ConvertTo-Json -Compress
