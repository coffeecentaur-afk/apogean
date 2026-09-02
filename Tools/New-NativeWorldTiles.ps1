param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex, [int]$alpha = 255) {
    $source = [System.Drawing.ColorTranslator]::FromHtml($hex)
    [System.Drawing.Color]::FromArgb($alpha, $source.R, $source.G, $source.B)
}

$palettes = @{
    Kessler = @((C '#17191b'),(C '#24282b'),(C '#34393c'),(C '#4a4f50'),(C '#6b6d68'),(C '#8d8b7f'),(C '#532d28'),(C '#9a4937'),(C '#cf7a35'),(C '#e5ad54'))
    Helix = @((C '#202422'),(C '#343a35'),(C '#555d54'),(C '#7a8175'),(C '#a5aa9d'),(C '#d2d1bf'),(C '#394c40'),(C '#587360'),(C '#a37529'),(C '#d9ad48'))
    Sentrix = @((C '#101820'),(C '#172734'),(C '#203b4d'),(C '#2c566d'),(C '#397a96'),(C '#65abc2'),(C '#176f91'),(C '#27a6d1'),(C '#6fd5ea'),(C '#b8f2f4'))
    WastesSoil = @((C '#211a17'),(C '#332721'),(C '#4a362a'),(C '#624833'),(C '#7d5c3d'),(C '#9a7650'),(C '#b69466'),(C '#d1b17b'),(C '#5a4937'),(C '#d7c69c'))
    WastesStone = @((C '#201d1b'),(C '#34302c'),(C '#49423b'),(C '#5f554b'),(C '#776a5d'),(C '#8d7e6e'),(C '#aa9983'),(C '#c0ad91'),(C '#574435'),(C '#d4c7aa'))
    WastesGrass = @((C '#201814'),(C '#34251c'),(C '#4b3424'),(C '#67482e'),(C '#86623a'),(C '#a67d44'),(C '#c59b55'),(C '#dfbd72'),(C '#6e5029'),(C '#ead29a'))
    WastesSand = @((C '#30271c'),(C '#493923'),(C '#65502e'),(C '#80663a'),(C '#9e7e47'),(C '#b99860'),(C '#d0b277'),(C '#e3cb96'),(C '#745a32'),(C '#f0dfb2'))
    WastesIce = @((C '#1e2526'),(C '#303a3b'),(C '#465153'),(C '#5f6c6e'),(C '#78878a'),(C '#93a2a3'),(C '#aeb9b6'),(C '#d2d8ce'),(C '#5e5543'),(C '#eee8d0'))
    WastesSnow = @((C '#36332e'),(C '#514c43'),(C '#6b655a'),(C '#817b6f'),(C '#9e9789'),(C '#b5ae9f'),(C '#d0c8b8'),(C '#e2dbc9'),(C '#736047'),(C '#f2ead6'))
    WastesMud = @((C '#191714'),(C '#28231d'),(C '#393026'),(C '#4a3d2d'),(C '#5e4c34'),(C '#765e3c'),(C '#8e7349'),(C '#aa8c5b'),(C '#4a402d'),(C '#c1a36f'))
    MawDirt = @((C '#171515'),(C '#24201e'),(C '#342b25'),(C '#49382b'),(C '#604a31'),(C '#7b603a'),(C '#9b7a42'),(C '#c39a45'),(C '#e3b64b'),(C '#e9d2a1'))
    MawStone = @((C '#171617'),(C '#252224'),(C '#353033'),(C '#494044'),(C '#5c5050'),(C '#746158'),(C '#92764f'),(C '#bd9140'),(C '#e2b146'),(C '#e6d1a6'))
    MawGrass = @((C '#161515'),(C '#27201c'),(C '#3a2c21'),(C '#503a27'),(C '#694d2b'),(C '#88642f'),(C '#aa7b31'),(C '#d09b38'),(C '#edbc45'),(C '#f2d58b'))
    MawSand = @((C '#211c18'),(C '#33291f'),(C '#493825'),(C '#624a2a'),(C '#7d5e30'),(C '#9e7637'),(C '#bd8d3d'),(C '#dca744'),(C '#f0c453'),(C '#f0dda3'))
    MawIce = @((C '#17191a'),(C '#24292a'),(C '#343c3b'),(C '#48514c'),(C '#5e675c'),(C '#777d68'),(C '#9b8b58'),(C '#bd9c45'),(C '#ddb340'),(C '#e8d9a7'))
    MawSnow = @((C '#242322'),(C '#37332f'),(C '#4a433a'),(C '#605543'),(C '#786a4c'),(C '#94805a'),(C '#b29455'),(C '#d2a94b'),(C '#edc358'),(C '#efe0b2'))
    MawMud = @((C '#141313'),(C '#211e1b'),(C '#302821'),(C '#413329'),(C '#56432e'),(C '#6f542f'),(C '#916a31'),(C '#b48335'),(C '#d9a640'),(C '#e6d29e'))
    MawBone = @((C '#211e19'),(C '#393329'),(C '#51493a'),(C '#6d624c'),(C '#8a7d60'),(C '#a89a76'),(C '#c4b68c'),(C '#ddd0a5'),(C '#b18439'),(C '#f0dea9'))
}

$terrainFrameMask = [System.Drawing.Bitmap]::new((Join-Path $Root 'Tools/Templates/TerrariaTerrainFrameMask.png'))
$wallFrameMask = [System.Drawing.Bitmap]::new((Join-Path $Root 'Tools/Templates/TerrariaWallFrameMask.png'))

function New-Bitmap([int]$width, [int]$height) {
    [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
}

function Save-Bitmap([System.Drawing.Bitmap]$bitmap, [string]$relativePath) {
    $path = Join-Path $Root $relativePath
    $directory = Split-Path -Parent $path
    if (-not (Test-Path -LiteralPath $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

function Hash-Pixel([int]$x, [int]$y, [int]$salt) {
    $value = ([int64]$x * 73856093) -bxor ([int64]$y * 19349663) -bxor ([int64]$salt * 83492791)
    [int]($value -band 0x7fffffff)
}

function Put([System.Drawing.Bitmap]$bitmap, [int]$x, [int]$y, [System.Drawing.Color]$color) {
    if ($x -ge 0 -and $y -ge 0 -and $x -lt $bitmap.Width -and $y -lt $bitmap.Height) {
        $bitmap.SetPixel($x, $y, $color)
    }
}

function Apply-AtlasMask([System.Drawing.Bitmap]$bitmap,[System.Drawing.Bitmap]$mask,[bool]$usesMagentaKey=$false) {
    if($bitmap.Width -ne $mask.Width -or $bitmap.Height -ne $mask.Height){
        throw "Atlas mask $($mask.Width)x$($mask.Height) does not match output $($bitmap.Width)x$($bitmap.Height)."
    }
    for($y=0;$y -lt $bitmap.Height;$y++){
        for($x=0;$x -lt $bitmap.Width;$x++){
            $source=$mask.GetPixel($x,$y)
            $transparent=$source.A -eq 0 -or ($usesMagentaKey -and $source.R -gt 220 -and $source.B -gt 220 -and $source.G -lt 160)
            if($transparent){$bitmap.SetPixel($x,$y,[System.Drawing.Color]::Transparent)}
        }
    }
}

function Draw-CorporateFrame([System.Drawing.Bitmap]$bitmap, [int]$originX, [int]$originY, [System.Drawing.Color[]]$p, [string]$part, [int]$frame) {
    for ($y = 0; $y -lt 16; $y++) {
        for ($x = 0; $x -lt 16; $x++) {
            $cluster = Hash-Pixel ([int]($x / 5)) ([int]($y / 5)) ($frame * 17)
			$index = if ($cluster % 23 -eq 0) { 1 } elseif ($cluster % 13 -eq 0) { 4 } elseif ($cluster % 7 -eq 0) { 3 } else { 2 }
            Put $bitmap ($originX + $x) ($originY + $y) $p[$index]
        }
    }

    switch ($part) {
        'Block' {
            if ($frame % 4 -eq 0) {
                for ($y=2;$y -lt 15;$y++) { Put $bitmap ($originX+10) ($originY+$y) $p[0] }
                for ($y=3;$y -lt 14;$y++) { Put $bitmap ($originX+11) ($originY+$y) $p[4] }
            }
            if ($frame % 5 -eq 1) {
                for ($x=2;$x -lt 13;$x++) { Put $bitmap ($originX+$x) ($originY+11) $p[1] }
                for ($x=3;$x -lt 12;$x++) { Put $bitmap ($originX+$x) ($originY+12) $p[4] }
            }
			# Sparse serial lights/rivets; broad material planes remain readable at 1x.
            if ($frame % 12 -eq 0) { Put $bitmap ($originX+3) ($originY+3) $p[8]; Put $bitmap ($originX+4) ($originY+3) $p[9] }
            if ($frame % 7 -eq 2) { Put $bitmap ($originX+12) ($originY+13) $p[0]; Put $bitmap ($originX+13) ($originY+12) $p[1] }
        }
        'Trim' {
            for ($x=0;$x -lt 16;$x++) {
                Put $bitmap ($originX+$x) ($originY+3) $p[0]
                Put $bitmap ($originX+$x) ($originY+4) $p[5]
                Put $bitmap ($originX+$x) ($originY+11) $p[1]
            }
            for ($x=2+($frame%3);$x -lt 15;$x+=7) { Put $bitmap ($originX+$x) ($originY+7) $p[8]; Put $bitmap ($originX+$x+1) ($originY+7) $p[9] }
        }
        'Floor' {
            for ($y=2;$y -lt 16;$y+=5) { for($x=0;$x -lt 16;$x++){ Put $bitmap ($originX+$x) ($originY+$y) $p[0] } }
            for ($x=-8;$x -lt 20;$x+=6) {
                for ($step=0;$step -lt 7;$step++) {
                    Put $bitmap ($originX+$x+$step) ($originY+5+$step) $p[4]
                    Put $bitmap ($originX+$x+$step) ($originY+6+$step) $p[1]
                }
            }
        }
        'Glass' {
            for($y=2;$y -lt 14;$y++){for($x=2;$x -lt 14;$x++){Put $bitmap ($originX+$x) ($originY+$y) ([System.Drawing.Color]::FromArgb(215,$p[1].R,$p[1].G,$p[1].B))}}
            for($x=0;$x -lt 16;$x++){Put $bitmap ($originX+$x) $originY $p[0];Put $bitmap ($originX+$x) ($originY+15) $p[0]}
            for($y=0;$y -lt 16;$y++){Put $bitmap $originX ($originY+$y) $p[0];Put $bitmap ($originX+15) ($originY+$y) $p[0]}
            for($step=0;$step -lt 9;$step++){Put $bitmap ($originX+3+$step) ($originY+11-$step) $p[5]}
            Put $bitmap ($originX+12) ($originY+12) $p[8]
        }
        'Beam' {
            for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){Put $bitmap ($originX+$x) ($originY+$y) $p[1]}}
            for($step=0;$step -lt 16;$step++){
                Put $bitmap ($originX+$step) ($originY+$step) $p[4]
                Put $bitmap ($originX+15-$step) ($originY+$step) $p[3]
            }
            for($x=0;$x -lt 16;$x++){Put $bitmap ($originX+$x) $originY $p[0];Put $bitmap ($originX+$x) ($originY+15) $p[0]}
            for($y=0;$y -lt 16;$y++){Put $bitmap $originX ($originY+$y) $p[0];Put $bitmap ($originX+15) ($originY+$y) $p[0]}
            Put $bitmap ($originX+7) ($originY+7) $p[8]; Put $bitmap ($originX+8) ($originY+7) $p[9]
        }
    }
}

function New-CorporateSheet([string]$family, [string]$part, [string]$name) {
    $bitmap = New-Bitmap 288 270
    $p = $palettes[$family]
    for($fy=0;$fy -lt 15;$fy++){
        for($fx=0;$fx -lt 16;$fx++){
            $frame=$fy*16+$fx
            Draw-CorporateFrame $bitmap ($fx*18) ($fy*18) $p $part $frame
        }
    }
    Apply-AtlasMask $bitmap $terrainFrameMask $true
    Save-Bitmap $bitmap "Content/Tiles/$name.png"
}

function Draw-NaturalFrame([System.Drawing.Bitmap]$bitmap,[int]$originX,[int]$originY,[System.Drawing.Color[]]$p,[string]$motif,[int]$frame,[bool]$maw) {
    for($y=0;$y -lt 16;$y++){
        for($x=0;$x -lt 16;$x++){
            $cluster=Hash-Pixel ([int]($x/4)) ([int]($y/4)) ($frame*31)
			$index=if($cluster%23 -eq 0){1}elseif($cluster%17 -eq 0){6}elseif($cluster%11 -eq 0){4}elseif($cluster%7 -eq 0){2}else{3}
            Put $bitmap ($originX+$x) ($originY+$y) $p[$index]
        }
    }
    switch -Wildcard ($motif) {
        '*Stone*' {
            for($x=1+($frame%4);$x -lt 15;$x+=6){Put $bitmap ($originX+$x) ($originY+3+(($x+$frame)%9)) $p[0];Put $bitmap ($originX+$x+1) ($originY+3+(($x+$frame)%9)) $p[5]}
        }
        '*Grass*' {
            for($x=($frame%3);$x -lt 16;$x+=5){for($y=0;$y -lt 6;$y++){Put $bitmap ($originX+$x+[int]($y/3)) ($originY+$y) $p[6]}}
            for($x=2;$x -lt 16;$x+=7){Put $bitmap ($originX+$x) ($originY+8) $p[8];Put $bitmap ($originX+$x) ($originY+9) $p[7]}
        }
        '*Sand*' {
            for($y=3+($frame%3);$y -lt 15;$y+=5){for($x=1;$x -lt 15;$x++){if(($x+$frame)%5 -ne 0){Put $bitmap ($originX+$x) ($originY+$y) $p[5]}}}
        }
        '*Ice*' {
            for($step=0;$step -lt 9;$step++){Put $bitmap ($originX+2+$step) ($originY+12-$step) $p[6]}
            for($step=0;$step -lt 5;$step++){Put $bitmap ($originX+10+$step) ($originY+4+$step) $p[1]}
        }
        '*Snow*' {
            for($x=0;$x -lt 16;$x++){Put $bitmap ($originX+$x) ($originY+(($x+$frame)%3)) $p[7]}
            for($x=2;$x -lt 14;$x+=5){Put $bitmap ($originX+$x) ($originY+7) $p[9]}
        }
        '*Mud*' {
            for($x=2+($frame%2);$x -lt 15;$x+=6){Put $bitmap ($originX+$x) ($originY+5+(($x+$frame)%6)) $p[0];Put $bitmap ($originX+$x+1) ($originY+5+(($x+$frame)%6)) $p[1]}
        }
        '*Bone*' {
            for($y=2;$y -lt 15;$y+=5){
                for($x=1+($frame%3);$x -lt 15;$x+=7){
                    Put $bitmap ($originX+$x) ($originY+$y) $p[0]
                    Put $bitmap ($originX+$x+1) ($originY+$y-1) $p[7]
                    Put $bitmap ($originX+$x+2) ($originY+$y) $p[5]
                }
            }
            if($frame%5 -eq 0){
                for($step=0;$step -lt 7;$step++){ Put $bitmap ($originX+4+$step) ($originY+12-$step) $p[7] }
            }
        }
        default {
            for($x=3+($frame%4);$x -lt 15;$x+=7){Put $bitmap ($originX+$x) ($originY+4+(($x+$frame)%8)) $p[7]}
        }
    }
    if($maw){
        for($y=0;$y -lt 16;$y++){
            $x=(($frame*3)+$y+[int]($y/3))%16
            Put $bitmap ($originX+$x) ($originY+$y) $p[0]
            if($y%4 -eq 0 -and $x -lt 15){Put $bitmap ($originX+$x+1) ($originY+$y) $p[8]}
        }
        if($frame%6 -eq 0){
            for($step=0;$step -lt 8;$step++){Put $bitmap ($originX+2+$step) ($originY+13-[int]($step/2)) $p[9]}
        }
    }
}

function New-NaturalSheet([string]$name,[string]$palette,[string]$motif,[bool]$maw=$false) {
    $bitmap=New-Bitmap 288 270
    $p=$palettes[$palette]
    for($fy=0;$fy -lt 15;$fy++){
        for($fx=0;$fx -lt 16;$fx++){
            Draw-NaturalFrame $bitmap ($fx*18) ($fy*18) $p $motif ($fy*16+$fx) $maw
        }
    }
    Apply-AtlasMask $bitmap $terrainFrameMask $true
    Save-Bitmap $bitmap "Content/Tiles/$name.png"
}

function New-WallSheet([string]$name,[string]$palette,[string]$motif,[bool]$maw=$false) {
    $bitmap=New-Bitmap 468 180
    for($fy=0;$fy -lt 10;$fy++){
        for($fx=0;$fx -lt 26;$fx++){
            Draw-NaturalFrame $bitmap ($fx*18) ($fy*18) $palettes[$palette] $motif ($fy*26+$fx) $maw
        }
    }
    Apply-AtlasMask $bitmap $wallFrameMask $false
    Save-Bitmap $bitmap "Content/Walls/$name.png"
}

foreach($family in @('Kessler','Helix','Sentrix')){
    foreach($part in @('Block','Trim','Floor','Glass','Beam')){New-CorporateSheet $family $part "$family$part"}
}
New-CorporateSheet 'Kessler' 'Block' 'KesslerPlating'
New-CorporateSheet 'Helix' 'Block' 'HelixContainmentPanel'
New-CorporateSheet 'Sentrix' 'Block' 'SentrixPanel'
New-CorporateSheet 'Kessler' 'Block' 'KesslerRuinBlock'
New-CorporateSheet 'Helix' 'Block' 'HelixRuinBlock'
New-CorporateSheet 'Sentrix' 'Block' 'SentrixRuinBlock'
New-CorporateSheet 'Kessler' 'Block' 'PrewarConcrete'
New-CorporateSheet 'Helix' 'Block' 'MawResearchBlock'

foreach($spec in @(
    @('WastesSoil','WastesSoil','Soil',$false),@('WastesStone','WastesStone','Stone',$false),
    @('WastesGrass','WastesGrass','Grass',$false),@('WastesSand','WastesSand','Sand',$false),
    @('WastesIce','WastesIce','Ice',$false),@('WastesSnow','WastesSnow','Snow',$false),
    @('WastesMud','WastesMud','Mud',$false),@('DeadGrass','WastesGrass','Grass',$false),
    @('MawDirt','MawDirt','Dirt',$true),@('Mawstone','MawStone','Stone',$true),
    @('MawGrass','MawGrass','Grass',$true),@('MawSand','MawSand','Sand',$true),
    @('MawIce','MawIce','Ice',$true),@('MawSnow','MawSnow','Snow',$true),
    @('MawMud','MawMud','Mud',$true),@('MawClay','MawDirt','Clay',$true),
    @('EngraftTurf','MawGrass','Grass',$true),@('OssuaryBone','MawBone','Bone',$false)
)){
    New-NaturalSheet $spec[0] $spec[1] $spec[2] $spec[3]
}

foreach($spec in @(
    @('KesslerBulkheadWall','Kessler','Block',$false),@('KesslerWindowWall','Kessler','Glass',$false),
    @('HelixLaboratoryWall','Helix','Block',$false),@('HelixObservationWall','Helix','Glass',$false),
    @('SentrixDataWall','Sentrix','Block',$false),@('SentrixWindowWall','Sentrix','Glass',$false),
    @('MawDirtWallUnsafe','MawDirt','Dirt',$true),@('MawStoneWallUnsafe','MawStone','Stone',$true),
    @('MawGrassWallUnsafe','MawGrass','Grass',$true),@('MawSandWallUnsafe','MawSand','Sand',$true),
    @('MawIceWallUnsafe','MawIce','Ice',$true),@('MawSnowWallUnsafe','MawSnow','Snow',$true),
    @('MawMudWallUnsafe','MawMud','Mud',$true),@('MawWallUnsafe','MawStone','Stone',$true),
	@('WastesDirtWallUnsafe','WastesSoil','Soil',$false),@('WastesStoneWallUnsafe','WastesStone','Stone',$false),
	@('WastesGrassWallUnsafe','WastesGrass','Grass',$false),@('WastesSandWallUnsafe','WastesSand','Sand',$false),
	@('WastesIceWallUnsafe','WastesIce','Ice',$false),@('WastesSnowWallUnsafe','WastesSnow','Snow',$false),
	@('WastesMudWallUnsafe','WastesMud','Mud',$false),
    @('DeadGrassWallUnsafe','WastesSoil','Soil',$false),@('DeadFlowerWallUnsafe','WastesGrass','Grass',$false)
)){
    New-WallSheet $spec[0] $spec[1] $spec[2] $spec[3]
}

$terrainFrameMask.Dispose()
$wallFrameMask.Dispose()

Write-Host 'Generated native Terraria-format corporate, Wastes, and Maw tile families.'
