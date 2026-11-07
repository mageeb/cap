param(
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [string]$SourceDirectory = 'C:\Repos\Translation\AI-Assisted-Translation\Books\Romans - Abouna Antonios Fekry\Translation\Chapter01'
)
$ErrorActionPreference = 'Stop'
function Get-StableId([string]$Name) {
    $hasher = [Security.Cryptography.SHA256]::Create()
    try { $hashBytes = $hasher.ComputeHash([Text.Encoding]::UTF8.GetBytes('translation-review-sample:'+$Name)) }
    finally { $hasher.Dispose() }
    $idBytes = New-Object byte[] 16
    [Array]::Copy($hashBytes, $idBytes, 16)
    return [guid]::new($idBytes).ToString('D')
}
$source = [IO.File]::ReadAllLines((Join-Path $SourceDirectory 'Romans.01.txt'))
$target = [IO.File]::ReadAllLines((Join-Path $SourceDirectory 'Romans.01.txt.Target.txt'))
$draft = [IO.File]::ReadAllLines((Join-Path $SourceDirectory 'Romans.01.txt.UneditedTarget.txt'))
$models = [IO.File]::ReadAllLines((Join-Path $SourceDirectory 'Romans.01.txt.Models.txt'))
if ($source.Length -ne $target.Length -or $source.Length -ne $draft.Length -or $source.Length -ne $models.Length) { throw 'Source and translation files are not line-aligned' }
$reviewerId = 'sample:romans-antonios-fekry'
$bookId = Get-StableId 'romans-antonios-fekry'
$chapterId = Get-StableId 'romans-antonios-fekry:chapter01'
$paragraphs = @()
for ($line = 0; $line -lt $source.Length; $line++) {
    $isBlank = [string]::IsNullOrWhiteSpace($source[$line])
    if ($isBlank -ne [string]::IsNullOrWhiteSpace($target[$line]) -or $isBlank -ne [string]::IsNullOrWhiteSpace($draft[$line]) -or $isBlank -ne [string]::IsNullOrWhiteSpace($models[$line])) { throw "Alignment mismatch at source line $($line+1)" }
    if ($isBlank) { continue }
    $paragraphs += @{ _id=(Get-StableId "romans-antonios-fekry:chapter01:line:$line");reviewerId=$reviewerId;bookId=$bookId;chapterId=$chapterId;position=$paragraphs.Count;chapterPosition=$paragraphs.Count;sourceLineNumber=($line+1);sourceText=$source[$line];machineDraft=$draft[$line];reviewedText=$target[$line];translationModel=$models[$line];importStatus='ImportedCurrentTarget';version='import-v1' }
}
$files = @(Get-ChildItem -LiteralPath $SourceDirectory -File | Sort-Object Name | ForEach-Object {
    $fileBytes = [IO.File]::ReadAllBytes($_.FullName)
    @{ _id=(Get-StableId ('romans-antonios-fekry:chapter01:file:'+$_.Name));reviewerId=$reviewerId;bookId=$bookId;chapterId=$chapterId;fileName=$_.Name;sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant();byteCount=$fileBytes.Length;contentBase64=[Convert]::ToBase64String($fileBytes) }
})
$payload = @{
    book=@{_id=$bookId;reviewerId=$reviewerId;title='Romans';author='Abouna Antonios Fekry';sourceLanguage='ar';targetLanguage='en';originKey='romans-antonios-fekry'}
    chapter=@{_id=$chapterId;reviewerId=$reviewerId;bookId=$bookId;position=0;chapterNumber=1;title=$target[0];sourceTitle=$source[0];originKey='romans-antonios-fekry/chapter01';sourceLineCount=$source.Length}
    paragraphs=$paragraphs;files=$files
}
$payload | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding utf8
Write-Output "Sample payload prepared: 1 book, 1 chapter, $($paragraphs.Count) paragraphs, $($files.Count) original files."
