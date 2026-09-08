param([Parameter(Mandatory)][string]$OutputDirectory)
# Bounded component assembly of the built-in image edit, not a redraw of the pod.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$original=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v2/ArrivalPod.png'
$edit=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v3/footing-edit.png'
if((Get-FileHash $original).Hash -ne '822A50B8EE634A671281F2CFC5AA3EE760031E0DA3757246C8216911D83B6407'){throw 'ORIGINAL_HASH'}
if((Get-FileHash $edit).Hash -ne '83936064AFE1D83FEE4859820301A3A8381DD4B69F525610ED54D0B58D352660'){throw 'EDIT_HASH'}
$out=(Resolve-Path -LiteralPath $OutputDirectory).Path
$content=Join-Path $repo 'Content'
if($out -eq $content -or $out.StartsWith($content+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'PRODUCTION_OUTPUT_FORBIDDEN'}
foreach($name in @('ArrivalPod.png','ArrivalPod_Tile.png')){if(Test-Path (Join-Path $out $name)){throw 'OUTPUT_EXISTS'}}
$base=[Drawing.Bitmap]::new($original)
$donor=[Drawing.Bitmap]::new($edit)
$art=[Drawing.Bitmap]::new(80,96,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$sheet=[Drawing.Bitmap]::new(90,108,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    if($donor.Width -ne 1145 -or $donor.Height -ne 1374){throw 'EDIT_DIMENSIONS'}
    $palette=@(0x070707,0x171819,0x232424,0x31302D,0x403C36,0x514B41,0x625B4C,0x756B57,0x887D68,0x9D927D,0xB0A590,0xC1B69F,0xD1C6AF,0xE2D7C1,0x3F2911,0x654316,0x906021,0xC0872B,0x4D4640,0xA38F70)
    # All 7040 upper pixels are copied exactly. Remove only rejected rows88..95.
    for($y=0;$y -lt 88;$y++){for($x=0;$x -lt 80;$x++){$art.SetPixel($x,$y,$base.GetPixel($x,$y))}}
    # Reviewed interior-metal rectangle, wholly inside the generated flat base.
    # The model returned an opaque matte and altered the upper body; neither is
    # used. This all-opaque strip needs no guessed white key or silhouette mask.
    # 56,1236,964,68 -> 32x4 logical pixels -> 64x8 native at (2,88).
    for($y=0;$y -lt 4;$y++){for($x=0;$x -lt 32;$x++){
        $sx=56+[int][Math]::Floor(($x+0.5)*964/32)
        $sy=1236+[int][Math]::Floor(($y+0.5)*68/4)
        $p=$donor.GetPixel($sx,$sy)
        if($p.A -ne 255 -or ($p.R -gt 220 -and $p.G -gt 220 -and $p.B -gt 220)){throw 'DONOR_OUTSIDE_METAL'}
        $best=0; $score=[int]::MaxValue
        foreach($c in $palette){
            $dr=[int]$p.R-(($c -shr 16) -band 255);$dg=[int]$p.G-(($c -shr 8) -band 255);$db=[int]$p.B-($c -band 255)
            $distance=3*$dr*$dr+4*$dg*$dg+2*$db*$db
            if($distance -lt $score){$score=$distance;$best=$c}
        }
        $pixel=[Drawing.Color]::FromArgb(255,(($best -shr 16) -band 255),(($best -shr 8) -band 255),($best -band 255))
        for($dy=0;$dy -lt 2;$dy++){for($dx=0;$dx -lt 2;$dx++){$art.SetPixel((2+$x*2+$dx),(88+$y*2+$dy),$pixel)}}
    }}
    for($y=0;$y -lt 96;$y++){for($x=0;$x -lt 80;$x++){
        $sheet.SetPixel(([int][Math]::Floor($x/16)*18+$x%16),([int][Math]::Floor($y/16)*18+$y%16),$art.GetPixel($x,$y))
    }}
    foreach($entry in @(@($art,'ArrivalPod.png'),@($sheet,'ArrivalPod_Tile.png'))){
        $stream=[IO.File]::Open((Join-Path $out $entry[1]),[IO.FileMode]::CreateNew)
        try{$entry[0].Save($stream,[Drawing.Imaging.ImageFormat]::Png)}finally{$stream.Dispose()}
    }
}finally{$base.Dispose();$donor.Dispose();$art.Dispose();$sheet.Dispose()}
Write-Output 'Footing candidate: upper88rows unchanged; generated metal-only strip fitted to 64x8 at (2,88); 2px grid. Static assembly, not live acceptance.'
