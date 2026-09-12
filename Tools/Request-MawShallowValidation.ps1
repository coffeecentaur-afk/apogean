param([Parameter(Mandatory)][ValidateSet('build','test','pristine','audit','reload','top','bottom','pocket','rib1','rib2','rib3','play','motion-entry','light','light-awake','light-dormant','light-natural','capture','release')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-shallow-$Case"
Write-Output 'Queued only; require fresh MAW SHALLOW COMPLETE and preservation records. Held views are not traversal proof.'
