$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanArrivalLive-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$valid=@'
ARRIVAL SITE: version=1; outcome=placed; reason=native-object-and-support-verified; spawn-unchanged=True
ARRIVAL SITE POSTGEN: outcome=placed; object30=True; route=True; dry=True; version=1
ARRIVAL SITE LOAD: outcome=placed; no-generation-on-entry=True
ARRIVAL SITE VIEW: world=control; outcome=placed; object30=True; native-tiles=True
ARRIVAL SITE LOAD: outcome=placed; no-generation-on-entry=True
ARRIVAL SITE VIEW: world=control; outcome=placed; object30=True; native-tiles=True
'@
$cases=@(
 @('valid',$valid,''),
 @('skip',($valid+"`nARRIVAL SITE: outcome=skipped"),'SITE_NOT_PLACED'),
 @('late-failure',($valid+"`nARRIVAL SITE VIEW: object30=False"),'NATIVE_FAILURE_PRESENT'),
 @('wet',$valid.Replace('dry=True','dry=False'),'NATIVE_FAILURE_PRESENT'),
 @('moved-spawn',$valid.Replace('spawn-unchanged=True','spawn-unchanged=False'),'NATIVE_FAILURE_PRESENT'),
 @('no-final',$valid.Replace('ARRIVAL SITE POSTGEN:','WRONG:'),'FINAL_NATIVE_CHECK_FAILED'),
 @('no-view',$valid.Replace('ARRIVAL SITE VIEW:','WRONG:'),'NO_NATIVE_VIEW'),
 @('no-reload',(($valid -split '\r?\n' | Select-Object -First 4) -join "`n"),'NO_RELOAD_VIEW_SEQUENCE')
)
foreach($case in $cases){
 $path=Join-Path $scratch ($case[0]+'.log')
 [IO.File]::WriteAllText($path,$case[1])
 $result=& pwsh -NoProfile -File "$PSScriptRoot/Test-ArrivalSiteLive.ps1" -LogPath $path -RequireView -RequireReload 2>&1 | Out-String
 if($case[2] -eq ''){if($LASTEXITCODE -ne 0){throw "Positive control rejected: $result"}}
 elseif($LASTEXITCODE -eq 0 -or $result -notmatch $case[2]){throw "Wrong rejection $($case[0]): $result"}
 Write-Host "PASS validator control: $($case[0])"
}
Write-Host 'PASS: 1 valid sequence and 7 deliberately defective native logs tested through the actual CLI. Not game evidence.'
