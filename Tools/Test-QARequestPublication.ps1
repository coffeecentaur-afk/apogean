Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$directory=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanRequestTest-'+[Guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $directory
$target=Join-Path $directory 'ApogeanLiveValidation.request'
# Reproduce the former native File.ReadAllText race with the actual sharing flags.
$writer=[IO.File]::Open($target,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
$caught=$false
try { try {$null=[IO.File]::ReadAllText($target)}catch [IO.IOException]{$caught=$true} } finally {$writer.Dispose()}
if(-not $caught){throw 'Old request writer did not reproduce the Windows sharing failure.'}
Remove-Item -LiteralPath $target # Exact temporary test file only.
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request 'qa-perf-snapshot' -CaptureDirectory $directory
if([IO.File]::ReadAllText($target) -cne 'qa-perf-snapshot'){throw 'Published incomplete request.'}
$refused=$false
try {& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request 'qa-perf-start' -CaptureDirectory $directory}catch {$refused=$true}
if(-not $refused -or [IO.File]::ReadAllText($target) -cne 'qa-perf-snapshot'){throw 'Pending request overwritten.'}
if(@(Get-ChildItem -LiteralPath $directory -Filter '*.staging').Count -ne 0){throw 'Unowned staging leftover.'}
Write-Output 'PASS old writer reproduces read failure; atomic publication readable/complete; pending request preserved. Temporary evidence retained; game queue untouched.'
