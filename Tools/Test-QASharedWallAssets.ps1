Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$runner=(Get-Process -Id $PID).Path
$check=Join-Path $PSScriptRoot 'Assert-QASharedWallAssets.ps1'
& $runner -NoProfile -File $check
if($LASTEXITCODE -ne 0){throw 'Real shared assets do not match.'}
$fixture=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanWallAlias-'+[Guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path (Join-Path $fixture 'grass'),(Join-Path $fixture 'soil')
foreach($key in @('soil','grass')) {foreach($ext in @('png','bin')) {
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot "../Content/Diagnostics/Materials/$key/Wall.$ext") -Destination (Join-Path $fixture "$key/Wall.$ext")
}}
foreach($ext in @('png','bin')) {
 $path=Join-Path $fixture "grass/Wall.$ext"
 $valid=[IO.File]::ReadAllBytes($path);$bad=[byte[]]$valid.Clone();$bad[$bad.Length-1]=$bad[$bad.Length-1] -bxor 1
 [IO.File]::WriteAllBytes($path,$bad)
 & $runner -NoProfile -File $check -MaterialDirectory $fixture 2>$null | Out-Null
 if($LASTEXITCODE -eq 0){throw "Corrupted $ext incorrectly accepted."}
 [IO.File]::WriteAllBytes($path,$valid)
}
Write-Output 'PASS actual shared-wall CLI rejects texture AND map divergence. Independent temporary fixtures retained.'
