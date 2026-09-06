param([Parameter(Mandatory)][string]$LogPath,[string[]]$Cases=@('ground','below-ground','diagonal-left','diagonal-right'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$lines=Get-Content -LiteralPath $LogPath
foreach($case in $Cases) {
    $escaped=[regex]::Escape($case)
    $record=@($lines | Select-String -Pattern "WASTES MODULAR RESULT: case=$escaped;" | Select-Object -Last 1)
    if($record.Count -ne 1){throw "MISSING_RESULT: $case"}
    if($record[0].Line -notmatch 'matrixChecks=(\d+); frameChecks=(\d+); failures=(\d+); maxPixelError=([\d.]+)'){throw 'BAD_RESULT'}
    if([int]$Matches[1] -lt 1 -or [int]$Matches[2] -lt 2 -or [int]$Matches[3] -ne 0 -or [double]::Parse($Matches[4],[cultureinfo]::InvariantCulture) -gt 1.1){throw "MODULAR_FAILURE: $($record[0].Line)"}
    $anchor=@($lines | Select-String -Pattern "WASTES V1 GROUND LOCK: case=$escaped;" | Select-Object -Last 1)
    if($anchor.Count -ne 1 -or $anchor[0].Line -notmatch 'pass=True'){throw "GROUND_LOCK_FAILURE: $case"}
    if($case -like 'diagonal-*') {
        $sweep=@($lines | Select-String -Pattern "WASTES V1 SWEEP: case=$escaped;" | Select-Object -Last 1)
        if($sweep.Count -ne 1 -or $sweep[0].Line -notmatch 'coveragePass=True'){throw "INSUFFICIENT_SWEEP: $case"}
    }
    Write-Output "PASS live modular ${case}: submitted positions/counts/depth and ground anchor. Not visual/routing approval."
}
