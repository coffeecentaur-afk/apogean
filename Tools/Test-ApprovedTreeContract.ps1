Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$shell=(Get-Process -Id $PID).Path
$validator=Join-Path $PSScriptRoot 'Test-TreeProductionReadiness.ps1'
& $shell -NoProfile -File $validator
if($LASTEXITCODE -ne 0){throw 'Production v3 tree baseline failed.'}
foreach($revision in @('v1','v2')){
 $candidate=Join-Path $PSScriptRoot "../Art/Candidates/WastesSnappedA-$revision"
 $output=& $shell -NoProfile -File $validator -CandidateDirectory $candidate
 if($LASTEXITCODE -eq 0 -or ($output -join "`n") -notmatch 'APPROVED_TREE:'){
  throw "Historical $revision was not refused at the reviewed-byte contract."
 }
 Write-Output "PASS: historical $revision refused as production v3; no pixels edited."
}
Write-Output 'PASS current production/v1/v2 through the actual validator CLI. Static identity/topology only, not fresh native behavior.'
