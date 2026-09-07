param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$Viewport)
# Historical opacity-log replay only; current geometry uses Test-WastesHeightLive.ps1.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if($Viewport -notmatch '^\d+x(\d+)$'){throw 'Invalid viewport'}
$height=[int]$Matches[1]
$lines=Get-Content -LiteralPath $LogPath
$samples=@{}
foreach($case in 'ground','wings','mid-altitude','high-altitude','below-ground') {
    $sample=@($lines | Where-Object {$_ -match "GROUND SAMPLE: case=$case; viewport=$Viewport;"})
    if(-not $sample.Count){throw "FAIL: missing $case sample at $Viewport"}
    $values=@{}
    foreach($field in 'soilY','gameZoom','closeTop','altitude','midOpacity') {
        if($sample[-1] -notmatch "(?:; )${field}=(-?\d+(?:\.\d+)?);" ){throw "FAIL: invalid $case $field"}
        $values[$field]=[double]::Parse($Matches[1],[Globalization.CultureInfo]::InvariantCulture)
    }
    $samples[$case]=$values
    $reports=@($lines | Where-Object {$_ -match "GROUND LOCK: case=$case;"})
    if(-not $reports.Count -or $reports[-1] -notmatch 'checks=(\d+); maxError=([\d.]+); pass=True;' -or [int]$Matches[1] -lt 120 -or [double]$Matches[2] -gt 1.1){throw "FAIL: insufficient ground-lock proof for $case"}
}
foreach($case in 'wings','mid-altitude','high-altitude') {
    if($samples[$case].closeTop -le $height){throw "FAIL: Close has not left the viewport at $case"}
}
if($samples['ground'].altitude -gt .03 -or $samples['ground'].midOpacity -lt .99){throw 'FAIL: ground staging'}
if($samples['mid-altitude'].altitude -lt .32 -or $samples['mid-altitude'].altitude -gt .37 -or $samples['mid-altitude'].midOpacity -lt .99){throw 'FAIL: middle scenery at first third'}
if($samples['high-altitude'].altitude -lt .79 -or $samples['high-altitude'].midOpacity -ne 0){throw 'FAIL: middle did not yield to Far'}
if($samples['below-ground'].altitude -ne 0 -or $samples['below-ground'].midOpacity -ne 1){throw 'FAIL: unexpected shallow-descent staging'}
$descent=($samples['ground'].soilY-$samples['below-ground'].soilY)/$samples['ground'].gameZoom
if([math]::Abs($descent-400) -gt 1.1){throw 'FAIL: shallow descent moved the ground anchor incorrectly'}
Write-Host "PASS: five altitude holds at $Viewport preserve world locking and stage Mid/Far. Screenshots still own visibility, art and cave-handoff acceptance. Use one viewport/session per retained log."
