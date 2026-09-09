param(
    [Parameter(Mandatory)][string]$BeforeDirectory,
    [Parameter(Mandatory)][string]$AfterDirectory,
    [Parameter(Mandatory)][string]$OutputPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates/MawBone-v1'))
$output=[IO.Path]::GetFullPath($OutputPath)
if(-not $output.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($output) -ine '.png'){throw 'OUTPUT_OUTSIDE_BONE_CANDIDATES'}
if((Test-Path -LiteralPath $output) -or (Test-Path -LiteralPath ($output+'.json'))){throw 'OUTPUT_EXISTS'}
Add-Type -AssemblyName System.Drawing
$paths=@((Resolve-Path -LiteralPath (Join-Path $BeforeDirectory 'OssuaryBone-candidate.png')).Path,(Resolve-Path -LiteralPath (Join-Path $AfterDirectory 'OssuaryBone-candidate.png')).Path)
$atlases=@([Drawing.Bitmap]::new($paths[0]),[Drawing.Bitmap]::new($paths[1]))
$board=[Drawing.Bitmap]::new(880,490)
$graphics=[Drawing.Graphics]::FromImage($board)
$title=[Drawing.Font]::new('Segoe UI',16,[Drawing.FontStyle]::Bold)
$label=[Drawing.Font]::new('Segoe UI',10)
try {
    if($atlases[0].Width -ne 288 -or $atlases[0].Height -ne 270 -or $atlases[0].Size -ne $atlases[1].Size){throw 'ATLAS_DIMENSIONS'}
    for($y=0;$y -lt 270;$y++){for($x=0;$x -lt 288;$x++){
        if($atlases[0].GetPixel($x,$y).A -ne $atlases[1].GetPixel($x,$y).A){throw 'ALPHA_CHANGED'}
    }}
    $full=[Collections.Generic.List[int]]::new()
    for($n=0;$n -lt 240;$n++){
        $opaque=$true
        for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){
            if($atlases[0].GetPixel(($n%16)*18+$x,[int][Math]::Floor($n/16)*18+$y).A -ne 255){$opaque=$false}
        }}
        if($opaque){$full.Add($n)}
    }
    if($full.Count -lt 2){throw 'NO_INTERIOR_VARIATION'}
    $graphics.Clear([Drawing.Color]::FromArgb(27,29,32))
    $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
    $graphics.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::None
    $graphics.DrawString('Bone material - same frames, same scale',$title,[Drawing.Brushes]::White,20,10)
    $graphics.DrawString('OFFLINE actual atlas pixels. Not game lighting, slopes, merges or an art approval.',$label,[Drawing.Brushes]::LightGray,20,44)
    $names=@('Previous - fine mottling','Revised - quieter material')
    for($side=0;$side -lt 2;$side++){
        $left=20+$side*440
        $graphics.DrawString($names[$side],$title,[Drawing.Brushes]::White,$left,82)
        foreach($scale in @(2,1)){
            $top=if($scale -eq 2){118}else{350}
            for($row=0;$row -lt 6;$row++){for($col=0;$col -lt 12;$col++){
                $index=$full[($col*17+$row*7)%$full.Count]
                $dst=[Drawing.Rectangle]::new($left+$col*16*$scale,$top+$row*16*$scale,16*$scale,16*$scale)
                $src=[Drawing.Rectangle]::new(($index%16)*18,[int][Math]::Floor($index/16)*18,16,16)
                $graphics.DrawImage($atlases[$side],$dst,$src,[Drawing.GraphicsUnit]::Pixel)
            }}
        }
        $graphics.DrawString('Above: 2x nearest-neighbor. Below: 1x texture pixels.',$label,[Drawing.Brushes]::LightGray,$left,321)
    }
    $graphics.DrawString('Both retain identical native masks. Teeth are separate; this is safe structural material.',$label,[Drawing.Brushes]::LightGray,20,463)
    $board.Save($output,[Drawing.Imaging.ImageFormat]::Png)
    [ordered]@{
        beforeSHA256=(Get-FileHash -LiteralPath $paths[0]).Hash
        afterSHA256=(Get-FileHash -LiteralPath $paths[1]).Hash
        sameAlpha=$true; selection='12x6, full[(column*17+row*7)%count], identical on both sides'
        outputSHA256=(Get-FileHash -LiteralPath $output).Hash
        nativeRendered=$false; artApproved=$false
    } | ConvertTo-Json | Set-Content -LiteralPath ($output+'.json') -Encoding utf8
} finally {
    $graphics.Dispose();$board.Dispose();$title.Dispose();$label.Dispose()
    foreach($bitmap in $atlases){$bitmap.Dispose()}
}
Write-Output $output
