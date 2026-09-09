param(
    [string]$LogPath='E:/SteamLibrary/steamapps/common/tModLoader/tModLoader-Logs/client.log',
    [Parameter(Mandatory)][string]$OutputDirectory
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$output=(Resolve-Path -LiteralPath $OutputDirectory).Path
$allowed=(Join-Path $root 'Art/Validation')+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)){throw 'EVIDENCE_OUTSIDE_VALIDATION'}
$lines=Get-Content -LiteralPath $LogPath
foreach($required in @('MAW FANG MATRIX PASS: 199','native-slope-resolves-solid-0','native-slope-resolves-solid-3','native-Hurt-health-loss-30','native-Hurt-immunity-prevents-double-hit','mechanical-envelope-restored')) {
    if(-not ($lines -match [regex]::Escape($required))){throw "INCOMPLETE_NATIVE_RUN: $required"}
}
if($lines -match 'Fang assertion failed'){throw 'NATIVE_RUN_HAS_FAILED_ASSERTION'}
$selected=$lines | Where-Object {$_ -match 'MAW FANG CHECK PASS:|MAW FANG MATRIX PASS:|MAW FANG GROVE GUARD:'}
$destination=Join-Path $output 'native-checks.log'
$stream=[IO.File]::Open($destination,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
try {
    $text="Filtered snapshot of the actual client.log; replay/evidence only, not a new run.`n"+($selected -join "`n")+"`n"
    $bytes=[Text.UTF8Encoding]::new($false).GetBytes($text);$stream.Write($bytes,0,$bytes.Length)
} finally {$stream.Dispose()}
Get-FileHash -LiteralPath $destination
