param([Parameter(Mandatory)][string]$LogPath,[string]$Viewport='2560x1369')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$original = Get-Content -Raw -LiteralPath $LogPath
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanHeightTrace-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$variants = @(
    @('unchanged',$original,$true),
    @('height fade',$original.Replace('factor=1.00000;','factor=0.50000;'),$false),
    @('failed position',$original.Replace('exited=True; failed=False','exited=True; failed=True'),$false),
    @('no exit',$original.Replace('exited=True;','exited=False;'),$false),
    @('missing result',($original -replace '(?m)^.*HEIGHT POSITION RESULT: case=space-ascent;.*(?:\r?\n|$)',''),$false)
)
foreach ($v in $variants) {
    $path=Join-Path $scratch ($v[0].Replace(' ','-')+'.log')
    [IO.File]::WriteAllText($path,$v[1])
    $output = & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-WastesHeightLive.ps1') -LogPath $path -Viewport $Viewport -Cases space-ascent 2>&1
    $ok = $LASTEXITCODE -eq 0
    if ($ok -ne $v[2]) { throw "Wrong trace validator result: $($v[0]); $output" }
    if (-not $v[2] -and ($output -join "`n") -notmatch 'HEIGHT_FADE|BAD_POSITION|INCOMPLETE_FLIGHT|MISSING_RESULT') { throw 'Negative trace failed for an unrelated reason' }
    Write-Host "PASS native-trace control: $($v[0])"
}
Write-Host "Trace controls retained at $scratch. No live log was edited."
