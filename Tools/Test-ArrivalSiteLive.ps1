param([Parameter(Mandatory)][string]$LogPath,[string]$ClientLogPath,[switch]$RequireView,[switch]$RequireReload)
$ErrorActionPreference='Stop'
$source=Get-Content -LiteralPath $LogPath -Raw
if($ClientLogPath){$source+="`n"+(Get-Content -LiteralPath $ClientLogPath -Raw)}
if($source -match 'ARRIVAL SITE QA FAILED|outcome=skipped|rolled-back:'){throw 'SITE_NOT_PLACED'}
if($source -match '(?:object30|route|dry|spawn-unchanged|no-generation-on-entry)=False'){throw 'NATIVE_FAILURE_PRESENT'}
if($source -notmatch 'ARRIVAL SITE: version=1; outcome=placed;.*spawn-unchanged=True'){throw 'NO_GENERATED_DIVOT'}
if($source -notmatch 'ARRIVAL SITE POSTGEN: outcome=placed; object30=True; route=True; dry=True; version=1'){throw 'FINAL_NATIVE_CHECK_FAILED'}
if(($RequireView -or $RequireReload) -and $source -notmatch 'ARRIVAL SITE VIEW:.*outcome=placed;.*object30=True;.*native-tiles=True'){throw 'NO_NATIVE_VIEW'}
if($RequireReload){
 $records=@($source -split '\r?\n' | Where-Object {$_ -match '^ARRIVAL SITE (?:LOAD|VIEW):'})
 if($records.Count -lt 4 -or ($records -join "`n") -notmatch '(?s)LOAD: outcome=placed; no-generation-on-entry=True.*VIEW:.*object30=True.*LOAD: outcome=placed; no-generation-on-entry=True.*VIEW:.*object30=True'){throw 'NO_RELOAD_VIEW_SEQUENCE'}
}
Write-Host 'PASS: new-world divot, native 30-cell pod/support, dry escape route and unchanged spawn. View/persistence/multiplayer remain independent.'
