param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$OutputPath,
    [Parameter(Mandatory)][int[]]$Rectangle,
    [ValidateRange(1,8)][int]$Scale=4,
    [string]$Backdrop='233342'
)
# Inspection only: nearest-neighbor crops, never runtime texture enlargement.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$source=(Resolve-Path -LiteralPath $SourcePath).Path
$output=[IO.Path]::GetFullPath($OutputPath)
if($source -eq $output){throw 'PREVIEW_CANNOT_OVERWRITE_SOURCE'}
if($Rectangle.Count -ne 4){throw 'RECTANGLE_REQUIRES_X_Y_WIDTH_HEIGHT'}
$b=[Drawing.Bitmap]::new($source)
try {
    $r=[Drawing.Rectangle]::new($Rectangle[0],$Rectangle[1],$Rectangle[2],$Rectangle[3])
    if($r.Width -le 0 -or $r.Height -le 0 -or $r.X -lt 0 -or $r.Y -lt 0 -or $r.Right -gt $b.Width -or $r.Bottom -gt $b.Height){throw 'CROP_OUTSIDE_SOURCE'}
    $preview=[Drawing.Bitmap]::new($r.Width*$Scale,$r.Height*$Scale)
    try {
        $g=[Drawing.Graphics]::FromImage($preview)
        try {
            $g.Clear([Drawing.ColorTranslator]::FromHtml("#$Backdrop"))
            $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
            $g.DrawImage($b,[Drawing.Rectangle]::new(0,0,$preview.Width,$preview.Height),$r,[Drawing.GraphicsUnit]::Pixel)
        } finally {$g.Dispose()}
        $preview.Save($output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$preview.Dispose()}
} finally {$b.Dispose()}
Write-Output "Inspection crop at ${Scale}x (not a game screenshot): $output"
