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
foreach($fileName in @('ApogeanMawEntrance.request','ApogeanArrivalSite.request')){
 & (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request 'view' -CaptureDirectory $directory -RequestFileName $fileName
 $namedTarget=Join-Path $directory $fileName
 if([IO.File]::ReadAllText($namedTarget) -cne 'view'){throw 'Named request incomplete.'}
 $refused=$false
 try{& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request 'shallow' -CaptureDirectory $directory -RequestFileName $fileName}catch{$refused=$true}
 if(-not $refused -or [IO.File]::ReadAllText($namedTarget) -cne 'view'){throw 'Named pending request overwritten.'}
}
foreach($bad in @('../outside.request','Other.request','C:/outside.request')){
 $refused=$false
 try{& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request 'view' -CaptureDirectory $directory -RequestFileName $bad}catch{$refused=$true}
 if(-not $refused){throw 'Publisher accepted a path outside its three known queues.'}
}
if([IO.File]::ReadAllText($target) -cne 'qa-perf-snapshot'){throw 'Named queues changed the live queue.'}
if(@(Get-ChildItem -LiteralPath $directory -Filter '*.staging').Count -ne 0){throw 'Unowned staging leftover.'}
Write-Output 'PASS old writer reproduces read failure; all three queues publish closed payloads, preserve pending requests and refuse unknown paths. Temporary evidence retained; game queue untouched.'
