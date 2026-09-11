param([Parameter(Mandatory)][string]$BuildRoot,[Parameter(Mandatory)][string]$OutputPath,[Parameter(Mandatory)][string]$EvidenceReference)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $BuildRoot).Path
if(-not (Test-Path -LiteralPath (Join-Path $root 'apogean.csproj'))){throw 'BUILD_ROOT_REQUIRED'}
if(Test-Path -LiteralPath $OutputPath){throw 'SNAPSHOT_EXISTS: use a new version; never replace a failed baseline.'}
$entries=@(Get-ChildItem -LiteralPath (Join-Path $root 'Content') -Recurse -File | Where-Object {$_.Extension -in @('.png','.bin')} | Sort-Object FullName | ForEach-Object {
    [ordered]@{path=[IO.Path]::GetRelativePath($root,$_.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;bytes=$_.Length}
})
if($entries.Count -eq 0){throw 'EMPTY_SNAPSHOT'}
$data=[ordered]@{schemaVersion=1;scope='Byte continuity of a named QA package, NOT blanket art or production approval';evidenceReference=$EvidenceReference;entries=$entries}
$parent=Split-Path -Parent ([IO.Path]::GetFullPath($OutputPath))
New-Item -ItemType Directory -Path $parent -Force | Out-Null
$json=$data | ConvertTo-Json -Depth 5
$stream=[IO.File]::Open([IO.Path]::GetFullPath($OutputPath),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
try {$bytes=[Text.Encoding]::UTF8.GetBytes($json);$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
Write-Output "Snapshot recorded: $($entries.Count) assets. Verify with Test-QAAssetSnapshot; approval is separate."
