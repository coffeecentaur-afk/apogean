param([Parameter(Mandatory)][string]$LogPath,[string]$Viewport='2560x1369')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$original = Get-Content -Raw -LiteralPath $LogPath
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanHeightTrace-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$variants = @(
    @('unchanged',$original,$true),
    @('old cap profile',$original.Replace('profile=smooth-flight-v1','profile=hard-cap-v0'),$false),
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
    if (-not $v[2] -and ($output -join "`n") -notmatch 'HEIGHT_FADE|BAD_POSITION|INCOMPLETE_FLIGHT|MISSING_RESULT|WRONG_FLIGHT_PROFILE|EXIT_FLAG_MISMATCH') { throw 'Negative trace failed for an unrelated reason' }
    Write-Host "PASS native-trace control: $($v[0])"
}
Write-Host "Trace controls retained at $scratch. No live log was edited."

# Require actual intermediate holds and a return; a complete ascent trace is
# insufficient evidence for stopping and reversing in the middle of flight.
foreach($control in 'unchanged','no-mid-hold','no-return') {
    $modified=@($original -split '\r?\n' | Where-Object {
        if($_ -notmatch 'HEIGHT POSITION SAMPLE: case=flight-turnaround;.*tick=(\d+);'){return $true}
        $tick=[int]$Matches[1]
        return -not (($control -eq 'no-mid-hold' -and $tick -ge 600 -and $tick -le 900) -or ($control -eq 'no-return' -and $tick -gt 900))
    }) -join "`n"
    $path=Join-Path $scratch ('turnaround-'+$control+'.log')
    [IO.File]::WriteAllText($path,$modified)
    $output=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-WastesHeightLive.ps1') -LogPath $path -Viewport $Viewport -Cases flight-turnaround 2>&1
    if(($LASTEXITCODE -eq 0) -ne ($control -eq 'unchanged')) {throw "Wrong turnaround control result: $control; $output"}
    if($control -ne 'unchanged' -and ($output -join "`n") -notmatch 'INCOMPLETE_FLIGHT|INCOMPLETE_TURNAROUND') {throw 'Turnaround control failed for unrelated reason'}
    Write-Host "PASS turnaround control: $control"
}
