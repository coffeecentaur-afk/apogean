param([Parameter(Mandatory)][string]$CapturePath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$image=[Drawing.Bitmap]::new((Resolve-Path -LiteralPath $CapturePath).Path)
try {
    if($image.Width-ne672 -or $image.Height-ne592){throw 'Expected native 42x37 orientation-room capture.'}
    # Fixed room: ceiling underside y48, tooth socket x320..383. Discriminate
    # pale, near-neutral bone from ochre dirt and blue/green daytime sky.
    $contact=0
    for($y=48;$y-lt51;$y++){for($x=320;$x-lt384;$x++){
        $c=$image.GetPixel($x,$y)
        if($c.R-ge110 -and $c.G-ge105 -and $c.B-ge90 -and $c.R-ge$c.G -and ($c.R-$c.G)-lt40 -and ($c.G-$c.B)-lt40){$contact++}
    }}
    if($contact-lt10){throw "CEILING_GAP: only $contact visible bone pixels at the terrain socket; need10."}
    Write-Host "PASS: $contact native bone pixels connect the ceiling socket. Daylight fixture only, not a universal art detector."
} finally {$image.Dispose()}
