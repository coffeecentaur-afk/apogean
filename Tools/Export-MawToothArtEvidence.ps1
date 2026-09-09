param(
    [string]$LogPath = 'E:/SteamLibrary/steamapps/common/tModLoader/tModLoader-Logs/client.log',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../Art/Validation/MawToothFamily-2026-09-09')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$lines = @(Get-Content -LiteralPath $LogPath | Where-Object {
    $_ -match 'MAW TOOTH ART|Preserved grove differs after reload|AUTOMATIC WASTES TERRAIN LAB BUILD FAILED|LIVE VALIDATION REQUEST CONSUMED: qa-save-and-quit|Saving world data|Validating world save|Saving modded world data|Loading World: Apogee Native Visual V3'
})
if (-not ($lines -match 'MAW TOOTH ART MATRIX PASS: 52')) { throw 'No native tooth art result in this log.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$lines | Set-Content -LiteralPath (Join-Path $OutputDirectory 'native-checks.log') -Encoding utf8
Write-Host "Exported $($lines.Count) scoped native log lines; includes the unresolved grove failure."
