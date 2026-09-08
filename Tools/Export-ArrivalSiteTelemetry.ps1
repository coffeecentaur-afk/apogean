param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$OutputPath)
$ErrorActionPreference='Stop'
if([IO.Path]::GetFullPath($LogPath) -eq [IO.Path]::GetFullPath($OutputPath)){throw 'Keep raw log intact'}
$records=@(Get-Content -LiteralPath $LogPath | ForEach-Object {if($_ -match '(ARRIVAL SITE(?: SURVEY| POSTGEN| LOAD| VIEW| QA FAILED)?:[^\r\n]*)$'){$Matches[1]}})
if(-not $records.Count){throw 'No arrival records'}
[IO.File]::WriteAllLines([IO.Path]::GetFullPath($OutputPath),[string[]]$records)
Write-Host "Exported $($records.Count) allowlisted arrival records, not raw client/server telemetry."
