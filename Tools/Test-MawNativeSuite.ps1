Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
# CLI controls for the orchestration only. The request sender is replaced in a
# temporary copy; these tests NEVER send requests to Terraria.
$sandbox=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanSuiteControls-'+[Guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $sandbox
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Invoke-MawNativeSuite.ps1'),(Join-Path $PSScriptRoot 'Export-MawNativeEvidence.ps1') -Destination $sandbox
$stub=@'
param([string]$Case,[switch]$Playable)
$log=Join-Path $PSScriptRoot 'client.log'
$mode=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'mode.txt')
if($mode -eq 'stale-only'){return}
if($mode -eq 'sender-failure'){throw 'Synthetic occupied request queue'}
$head="[apogean]: MAW LAB REQUEST: MawPlayableLab/$Case; productionTypes=True.`n"
if($mode -eq 'native-reject'){
 [IO.File]::AppendAllText($log,$head+"LIVE VALIDATION REQUEST FAILED: test`n");return
}
$line=switch($Case){
 'test' {'MAW NATURAL MATRIX PASS:'}
 'properties' {'MAW PRODUCTION PROPERTIES: 449/449 passed;'}
 'sand' {if($mode -eq 'sand-without-physics'){'MAW SAND RESTORE PASS: exact whole digest=True;'}else{"MAW SAND PHYSICS PASS:`n[apogean]: MAW SAND RESTORE PASS: exact whole digest=True;"}}
 'seams' {'MAW SEAM REGRESSION PASS:'}
 'negative' {'MAW NATURAL MUTATIONS PASS: 8 native cell controls;'}
}
[IO.File]::AppendAllText($log,$head+"[apogean]: $line`n")
if($mode -ne 'missing-completion'){
 [IO.File]::AppendAllText($log,"[apogean]: MAW LAB COMPLETE: MawPlayableLab/$Case`n")
}
'@
[IO.File]::WriteAllText((Join-Path $sandbox 'Request-MawNaturalValidation.ps1'),$stub)
$passed=0
foreach($case in @(
 @{Name='valid';Expected=0},
 @{Name='stale-only';Expected=1},
 @{Name='native-reject';Expected=1},
 @{Name='sand-without-physics';Expected=1},
 @{Name='missing-completion';Expected=1},
 @{Name='sender-failure';Expected=1},
 @{Name='existing-evidence';Expected=1}
)){
 $log=Join-Path $sandbox 'client.log'
 $evidence=Join-Path $sandbox ($case.Name+'.json')
 # The stale log deliberately contains a prior success. It must never count.
 [IO.File]::WriteAllText($log,"[apogean]: MAW LAB REQUEST: MawPlayableLab/test;`n[apogean]: MAW NATURAL MATRIX PASS:`n")
 [IO.File]::WriteAllText((Join-Path $sandbox 'mode.txt'),$case.Name)
 if($case.Name -eq 'existing-evidence'){[IO.File]::WriteAllText($evidence,'preserve-me')}
 $output=& pwsh -NoProfile -File (Join-Path $sandbox 'Invoke-MawNativeSuite.ps1') -EvidencePath $evidence -LogPath $log -TimeoutSeconds 3 2>&1
 $code=$LASTEXITCODE
 if(($code -eq 0) -ne ($case.Expected -eq 0)){throw "Unexpected CLI verdict for $($case.Name): $code`n$output"}
 if(-not(Test-Path -LiteralPath $evidence)){throw "Missing red/green evidence for $($case.Name)"}
 if($case.Name -eq 'existing-evidence' -and (Get-Content -Raw -LiteralPath $evidence) -ne 'preserve-me'){throw 'Overwrote evidence'}
 $passed++;Write-Output "PASS suite CLI control: $($case.Name)"
}
Write-Output "PASS $passed isolated suite controls. No native requests sent; retained scratch: $sandbox"
