param([string]$CaptureDirectory = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures'))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$result = Get-Content -LiteralPath (Join-Path $CaptureDirectory 'Apogean-ForestSpray.json') -Raw | ConvertFrom-Json
if (([datetime]::UtcNow - [datetime]$result.utc).TotalMinutes -gt 15) { throw 'FAIL: stale spray evidence; rerun forest-restoration-spray.' }
if (-not $result.pass) { throw "FAIL: spray/fade check: $($result | ConvertTo-Json -Compress)" }
Write-Host "PASS: native PureSpray restored the scene with $($result.partialFrames) intermediate fade frames and no opacity mismatches. This is not manual weapon-input or multiplayer proof."
