param(
    [string]$LogPath = 'E:/SteamLibrary/steamapps/common/tModLoader/tModLoader-Logs/client.log',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../Art/Validation/MawToothCluster-2026-09-09'),
    [string]$Name = 'native-checks.log'
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if([IO.Path]::GetFileName($Name)-ne$Name){throw 'Leaf filename required.'}
$lines=@(Get-Content -LiteralPath $LogPath | Where-Object {
    $_ -match 'MAW CLUSTER|MAW ORIENTATION|Preserved grove differs after reload|AUTOMATIC WASTES TERRAIN LAB BUILD FAILED|LIVE VALIDATION REQUEST CONSUMED: qa-save-and-quit|Saving world data|Validating world save|Saving modded world data|Loading World: Apogee Native Visual V3'
})
if(-not($lines -match 'MAW (CLUSTER|ORIENTATION) MATRIX PASS:')){throw 'No actual native cluster matrix result.'}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$lines | Set-Content -LiteralPath (Join-Path $OutputDirectory $Name) -Encoding utf8
Write-Host "Exported $($lines.Count) scoped evidence lines, including known failure."
