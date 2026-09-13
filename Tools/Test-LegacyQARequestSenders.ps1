Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanLegacySenders-'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $scratch
$cases=@(
 @{Script='Request-LiveValidation.ps1';Args=@{Fixture='conversion'};Root=$true;Expected='conversion'},
 @{Script='Request-WastesCameraCheck.ps1';Args=@{Case='ground'};Root=$true;Expected='wastes-camera-ground'},
 @{Script='Request-ArrivalPodValidation.ps1';Args=@{Case='day'};Root=$true;Expected='arrival-pod-day'},
 @{Script='Request-MawBoneValidation.ps1';Args=@{Case='test'};Root=$false;Expected='maw-bone-test'},
 @{Script='Request-MawClusterValidation.ps1';Args=@{Case='orientation-test'};Root=$false;Expected='maw-cluster-orientation-test'},
 @{Script='Request-MawClusterValidation.ps1';Args=@{Case='save-and-quit'};Root=$false;Expected='qa-save-and-quit'},
 @{Script='Request-MawFamilyValidation.ps1';Args=@{Case='row1'};Root=$false;Expected='maw-family-row1'},
 @{Script='Request-MawMaterialValidation.ps1';Args=@{Case='test'};Root=$false;Expected='maw-material-test'},
 @{Script='Request-MawToothArtValidation.ps1';Args=@{Case='day'};Root=$false;Expected='maw-tooth-art-day'},
 @{Script='Request-MawToothArtValidation.ps1';Args=@{Case='save-and-quit'};Root=$false;Expected='qa-save-and-quit'},
 @{Script='Request-MawEntranceValidation.ps1';Args=@{Case='survey'};Root=$false;Expected='survey';FileName='ApogeanMawEntrance.request'},
 @{Script='Request-ArrivalSiteValidation.ps1';Args=@{Case='view'};Root=$true;RootParameter='SaveRoot';Expected='view';FileName='ApogeanArrivalSite.request'})
$index=0
foreach($case in $cases){
 $root=Join-Path $scratch ([string]$index++)
 $captures=Join-Path $root 'Captures';$null=New-Item -ItemType Directory -Path $captures
 $fileName=if($case.ContainsKey('FileName')){$case.FileName}else{'ApogeanLiveValidation.request'}
 $pending=Join-Path $captures $fileName
 [IO.File]::WriteAllText($pending,'qa-perf-snapshot')
 $arguments=$case.Args.Clone()
 if($case.Root){
  $rootParameter=if($case.ContainsKey('RootParameter')){$case.RootParameter}else{'TModLoaderRoot'}
  $arguments[$rootParameter]=$root
 }else{$arguments.CaptureDirectory=$captures}
 $script=Join-Path $PSScriptRoot $case.Script
 $refused=$false
 try{& $script @arguments | Out-Null}catch{$refused=$true}
 if(-not $refused -or [IO.File]::ReadAllText($pending) -cne 'qa-perf-snapshot'){
  throw "LEGACY_QA_SENDER: $($case.Script) overwrote or failed to refuse the pending request. Scratch=$root"
 }
 # Delete only the exact owned marker, never the real game's queue.
 Remove-Item -LiteralPath $pending
 & $script @arguments | Out-Null
 if([IO.File]::ReadAllText($pending) -cne $case.Expected){throw "Wrong published payload for $($case.Script)"}
 if(@(Get-ChildItem -LiteralPath $captures -Filter '*.staging').Count){throw 'Sender left staging files.'}
 $source=Get-Content -LiteralPath $script -Raw
 if($source -notmatch 'Publish-QARequest.ps1' -or $source -match 'WriteAllText|FileMode\]::CreateNew|Set-Content'){
  throw "Sender bypasses the tested atomic publisher: $($case.Script)"
 }
 Write-Output "PASS $($case.Script) / $($case.Expected): pending preserved, exact closed payload, no staging leftover."
}
foreach($script in Get-ChildItem -LiteralPath $PSScriptRoot -Filter 'Request*.ps1'){
 $source=Get-Content -LiteralPath $script.FullName -Raw
 if($source -notmatch 'Publish-QARequest.ps1' -or $source -match 'WriteAllText|FileMode\]::CreateNew|Set-Content'){
  throw "Request wrapper bypasses the common publisher: $($script.Name)"
 }
}
Write-Output "PASS $($cases.Count) actual sender cases in $scratch; real game queue untouched."
