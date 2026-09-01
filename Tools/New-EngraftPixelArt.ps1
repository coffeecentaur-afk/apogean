param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Add-Type -AssemblyName System.Drawing

$palette = @{
    '.' = [System.Drawing.Color]::Transparent
    'K' = [System.Drawing.Color]::FromArgb(255, 24, 22, 24)   # charcoal outline
    'C' = [System.Drawing.Color]::FromArgb(255, 54, 49, 48)   # charcoal fill
    'R' = [System.Drawing.Color]::FromArgb(255, 112, 48, 30)  # dried rust
    'A' = [System.Drawing.Color]::FromArgb(255, 185, 112, 24) # sickly amber
    'O' = [System.Drawing.Color]::FromArgb(255, 132, 82, 24)  # dead ochre soil
    'L' = [System.Drawing.Color]::FromArgb(255, 246, 190, 71) # amber light
    'B' = [System.Drawing.Color]::FromArgb(255, 213, 193, 150)# bone
    'W' = [System.Drawing.Color]::FromArgb(255, 255, 228, 151)# hard highlight
    'G' = [System.Drawing.Color]::FromArgb(255, 115, 197, 91) # restrained clinical indicator
}

function Save-Pattern {
    param([string]$RelativePath, [string[]]$Rows)
    $width = ($Rows | Measure-Object -Maximum Length).Maximum
    $bitmap = [System.Drawing.Bitmap]::new($width, $Rows.Count, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt $Rows.Count; $y++) {
        for ($x = 0; $x -lt $Rows[$y].Length; $x++) {
            $symbol = [string]$Rows[$y][$x]
            if ($palette.ContainsKey($symbol)) { $bitmap.SetPixel($x, $y, $palette[$symbol]) }
        }
    }
    $path = Join-Path $Root $RelativePath
    $directory = Split-Path -Parent $path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

# Tiles: these must be exact native Terraria dimensions; their variation comes from world placement, not blur.
Save-Pattern 'Content/Tiles/EngraftTurf.png' @(
    'KOOOAAOOAOOAAOOK', 'KOOAOAOOOAAOOOOK', 'KOOKOOOKOOOKOOOK', 'KOOOOAOOOOAOOOOK',
    'KOKOOOOAOOOOOKOK', 'KOOOOOOOOOOOOOOK', 'KOOOKOOOKOOOKOOK', 'KOOOOOOOOOOOOOOK',
    'KOKOOOOOKOOOOOKK', 'KOOOOOOOOOOOOOOK', 'KOOOKOOOKOOOKOOK', 'KOOOOOOOOOOOOOOK',
    'KOKOOOOOKOOOOOKK', 'KOOOOOOOOOOOOOOK', 'KOOOKOOOKOOOKOOK', 'KKKKKKKKKKKKKKKK'
)

Save-Pattern 'Content/Tiles/EngraftTuft.png' @(
    '..................', '........A.........', '...R....A....R....', '....R...A...R.....',
    '.....R.OAO.R......', '..A...ROAOR...A...', '...A...OAO...A....', '....A..OAO..A.....',
    '.....A.OAO.A......', '......AOOOA.......', '.......OOO........', '.......OOO........',
    '.......OOO........', '.......OOO........', '......KOOOK.......', '.....KKOOOKK......',
    '..................', '..................'
)

Save-Pattern 'Content/Tiles/DeadGrass.png' @(
    'RRRRRRRRRRRRRRRR', 'ROOOROOOOROOOROR', 'ROCOOOCOOOOCOOOR', 'ROOOCOOOCOOOCOOR',
    'ROOOOOOOOOOOOOOR', 'ROOCOOOOCOOOOORR', 'ROOOOOOOOOOOOOOR', 'ROOOOCOOOOCOOOOR',
    'ROOOOOOOOOOOOOOR', 'ROCOOOOCOOOOCOOR', 'ROOOOOOOOOOOOOOR', 'ROOOCOOOOCOOOOOR',
    'ROOOOOOOOOOOOOOR', 'ROCOOOOCOOOCOOOR', 'ROOOOOOOOOOOOOOR', 'RRRRRRRRRRRRRRRR'
)

Save-Pattern 'Content/Tiles/DeadTuft.png' @(
    '..................', '....R.......R.....', '.....R.....R......', '..R..R.....R..R...',
    '...R..R...R..R....', '....R.R...R.R.....', '.....RR...RR......', '......R...R.......',
    '......R...R.......', '.......R.R........', '.......R.R........', '........R.........',
    '........R.........', '.......ROR........', '......ROOOR.......', '.....RROOORR......',
    '..................', '..................'
)

Save-Pattern 'Content/Tiles/MawNode.png' @(
    '................', '.......A........', '......ARA.......', '.....ARARA......',
    '....AKCCKRA.....', '...AKCACC KRA...'.Replace(' ',''), '...KCAAA CKRA...'.Replace(' ',''), '...KCAWAC KRA...'.Replace(' ',''),
    '...KCAAA CKRA...'.Replace(' ',''), '...AKCACC KRA...'.Replace(' ',''), '....AKCCKRA.....', '.....ARARA......',
    '......ARR.......', '.......R........', '................', '................'
)

# Four vertically stacked animation frames for the first Engraft enemies.
Save-Pattern 'Content/NPCs/Engraft/GraftHound.png' @(
    '................................................', '................................................', '.................K..............................', '...............KCKK............................',
    '...........KKKKAACCK...........................', '.........KKCCCAAACCK...........................', '.......KKCCRAAAAACCKK..........................', '......KCCCRAAWAAACCCK..........................',
    '.....KCCKCAAAAKCCCCC K.........................'.Replace(' ',''), '....KCCKCCKCCKCCCCCCKK........................', '...KCCK....KCCKCCCKCCK.......................', '...KK.......KK..KK.KKK.........................',
    '................................................', '................................................', '................................................', '................................................', '................................................', '................K...............................', '..............KCKK.............................', '..........KKKKAACCK............................',
    '........KKCCCAAACCK............................', '......KKCCRAAAAACCKK...........................', '.....KCCCRAAWAAACCCK...........................', '....KCCKCAAAAKCCCCC K..........................'.Replace(' ',''), '...KCCKCCKCCKCCCCCCKK.........................', '...KCCK....KCCKCCCKCCK........................', '....KK......KK..KK.KKK.........................',
    '................................................', '................................................', '................................................', '................................................', '..................K.............................', '................KCKK...........................', '............KKKKAACCK..........................', '..........KKCCCAAACCK..........................',
    '........KKCCRAAAAACCKK.........................', '.......KCCCRAAWAAACCCK.........................', '......KCCKCAAAAKCCCCC K........................'.Replace(' ',''), '.....KCCKCCKCCKCCCCCCKK.......................', '.....KCCK....KCCKCCCKCCK......................', '......KK......KK..KK.KKK.......................',
    '................................................', '................................................', '................................................', '................................................', '.................K..............................', '...............KCKK............................', '...........KKKKAACCK...........................', '.........KKCCCAAACCK...........................',
    '.......KKCCRAAAAACCKK..........................', '......KCCCRAAWAAACCCK...........................', '.....KCCKCAAAAKCCCCC K.........................'.Replace(' ',''), '....KCCKCCKCCKCCCCCCKK........................', '...KCCK....KCCKCCCKCCK.......................', '...KK.......KK..KK.KKK.........................', '................................................', '................................................'
)

Save-Pattern 'Content/NPCs/Engraft/Mawling.png' @(
    '................', '................', '......KK........', '....KKCCK.......', '...KCAAAK.......', '...KCAWAK.......', '...KCAAACK......', '....KCRCCK......', '.....KCCK.......', '......KK........', '................', '................', '................', '................', '................', '................',
    '................', '......KK........', '....KKCCK.......', '...KCAAAK.......', '...KCAWAK.......', '...KCAAACK......', '....KCRCCK......', '.....KCCK.......', '......KK........', '................', '................', '................', '................', '................', '................', '................',
    '................', '................', '.....KK.........', '...KKCCK........', '..KCAAAK........', '..KCAWAK........', '..KCAAACK.......', '...KCRCCK.......', '....KCCK........', '.....KK.........', '................', '................', '................', '................', '................', '................',
    '................', '................', '......KK........', '....KKCCK.......', '...KCAAAK.......', '...KCAWAK.......', '...KCAAACK......', '....KCRCCK......', '.....KCCK.......', '......KK........', '................', '................', '................', '................', '................', '................'
)

Save-Pattern 'Content/NPCs/Engraft/CarrionKite.png' @(
    '......................', '......................', '...K..............K...', '....K....KK....K......', '..KKCKKKCAAKKKCCKK....', '.KCCCAAAAWAAACCCCK....', '..KKCKKCAAAACKKCCK....', '....K....KK....K......', '...K..............K...', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................',
    '......................', '..K................K..', '...K.....KK.....K.....', '.KKCKKKKCAAKKKKCCKK...', 'KCCCAAAAWAAACCCCCCK...', '.KKCKKKCAAAACKKCCKK...', '...K.....KK.....K.....', '..K................K..', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................',
    '......................', '......................', '....K..........K......', '...KKCKK....KKCCKK....', '..KCCCAKKKKACCCCK.....', '.KCCAAA WAAW AAACCK...'.Replace(' ',''), '..KCCCAKKKKACCCCK.....', '...KKCKK....KKCCKK....', '....K..........K......', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................',
    '......................', '..K................K..', '...K.....KK.....K.....', '.KKCKKKKCAAKKKKCCKK...', 'KCCCAAAAWAAACCCCCCK...', '.KKCKKKCAAAACKKCCKK...', '...K.....KK.....K.....', '..K................K..', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................', '......................'
)

Save-Pattern 'Content/Items/Materials/MawFibre.png' @(
    '....................','.........A..........','........ARA.........','.......ARARA........','......AKCCKRA.......',
    '.....AKCAACKRA......','.....KCAWACKRA......','.....KCAAACKRA......','......KCRCCRA.......','.......KCCKRA.......',
    '........KCKRA.......','.........KR........','....................','....................','....................','....................',
    '....................','....................','....................','....................'
)

function Save-SimpleSprite {
    param([string]$RelativePath, [int]$Width, [int]$Height, [scriptblock]$Draw)
    $bitmap = [System.Drawing.Bitmap]::new($Width, $Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        & $Draw $graphics
        $path = Join-Path $Root $RelativePath
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

$darkPen7 = { [System.Drawing.Pen]::new($palette.K, 7) }
$ochrePen3 = { [System.Drawing.Pen]::new($palette.O, 3) }
$amberPen3 = { [System.Drawing.Pen]::new($palette.A, 3) }
$bonePen2 = { [System.Drawing.Pen]::new($palette.B, 2) }

Save-SimpleSprite 'Content/Items/Weapons/RendHook.png' 40 40 {
    param($g)
    $outer = & $darkPen7; $inner = & $ochrePen3; $hookOuter = [System.Drawing.Pen]::new($palette.K, 6); $hook = [System.Drawing.Pen]::new($palette.B, 3)
    try {
        $g.DrawLine($outer, 7, 34, 26, 15); $g.DrawLine($inner, 7, 34, 26, 15)
        $g.DrawArc($hookOuter, 20, 3, 16, 18, 188, 230); $g.DrawArc($hook, 20, 3, 16, 18, 188, 230)
        $g.FillRectangle([System.Drawing.SolidBrush]::new($palette.A), 4, 31, 6, 6)
    }
    finally { $outer.Dispose(); $inner.Dispose(); $hookOuter.Dispose(); $hook.Dispose() }
}

Save-SimpleSprite 'Content/Items/Weapons/AmberSiphon.png' 36 36 {
    param($g)
    $outer = [System.Drawing.Pen]::new($palette.K, 6); $inner = & $amberPen3; $string = [System.Drawing.Pen]::new($palette.L, 2)
    try {
        $g.DrawLine($outer, 7, 31, 17, 19); $g.DrawLine($inner, 7, 31, 17, 19)
        $g.DrawArc($outer, 13, 5, 18, 19, 125, 205); $g.DrawArc($inner, 13, 5, 18, 19, 125, 205)
        $g.DrawLine($string, 22, 9, 31, 4); $g.FillEllipse([System.Drawing.SolidBrush]::new($palette.A), 20, 8, 6, 6)
    }
    finally { $outer.Dispose(); $inner.Dispose(); $string.Dispose() }
}

Save-SimpleSprite 'Content/Items/Weapons/SinewBow.png' 36 36 {
    param($g)
    $outer = [System.Drawing.Pen]::new($palette.K, 6); $limb = & $ochrePen3; $string = & $bonePen2
    try {
        $g.DrawArc($outer, 4, 3, 22, 30, 275, 170); $g.DrawArc($limb, 4, 3, 22, 30, 275, 170)
        $g.DrawLine($string, 17, 4, 17, 32); $g.DrawLine($string, 17, 18, 30, 18)
        $g.FillRectangle([System.Drawing.SolidBrush]::new($palette.A), 27, 16, 5, 5)
    }
    finally { $outer.Dispose(); $limb.Dispose(); $string.Dispose() }
}

Save-SimpleSprite 'Content/Items/Weapons/MawEffigy.png' 38 38 {
    param($g)
    $outline = [System.Drawing.Pen]::new($palette.K, 4); $body = [System.Drawing.SolidBrush]::new($palette.O); $amber = [System.Drawing.SolidBrush]::new($palette.A)
    try {
        [System.Drawing.Point[]]$shape = @(
            [System.Drawing.Point]::new(19, 3), [System.Drawing.Point]::new(29, 13), [System.Drawing.Point]::new(25, 27),
            [System.Drawing.Point]::new(32, 35), [System.Drawing.Point]::new(19, 31), [System.Drawing.Point]::new(6, 35),
            [System.Drawing.Point]::new(13, 27), [System.Drawing.Point]::new(9, 13))
        $g.FillPolygon($body, $shape); $g.DrawPolygon($outline, $shape)
        $g.FillRectangle($amber, 15, 12, 8, 10); $g.DrawLine($outline, 13, 27, 6, 35); $g.DrawLine($outline, 25, 27, 32, 35)
    }
    finally { $outline.Dispose(); $body.Dispose(); $amber.Dispose() }
}

Save-SimpleSprite 'Content/Buffs/MawEffigyBuff.png' 32 32 {
    param($g)
    $outline = [System.Drawing.Pen]::new($palette.K, 3); $body = [System.Drawing.SolidBrush]::new($palette.O); $amber = [System.Drawing.SolidBrush]::new($palette.A)
    try {
        [System.Drawing.Point[]]$shape = @(
            [System.Drawing.Point]::new(16, 3), [System.Drawing.Point]::new(25, 12), [System.Drawing.Point]::new(22, 24),
            [System.Drawing.Point]::new(28, 29), [System.Drawing.Point]::new(16, 27), [System.Drawing.Point]::new(4, 29),
            [System.Drawing.Point]::new(10, 24), [System.Drawing.Point]::new(7, 12))
        $g.FillPolygon($body, $shape); $g.DrawPolygon($outline, $shape); $g.FillRectangle($amber, 13, 11, 6, 9)
    }
    finally { $outline.Dispose(); $body.Dispose(); $amber.Dispose() }
}

function Save-DeadTreeAtlas {
    $logical = [System.Drawing.Bitmap]::new(48, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($logical)
    try {
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
        $trunk = [System.Drawing.Pen]::new($palette.K, 7)
        $wood = [System.Drawing.Pen]::new($palette.R, 3)
        try {
            foreach ($pen in @($trunk, $wood)) {
                $graphics.DrawLine($pen, 24, 63, 24, 10)
                $graphics.DrawLine($pen, 24, 22, 8, 8)
                $graphics.DrawLine($pen, 24, 31, 41, 15)
                $graphics.DrawLine($pen, 13, 13, 5, 17)
                $graphics.DrawLine($pen, 36, 20, 44, 23)
            }
        }
        finally { $trunk.Dispose(); $wood.Dispose() }
    }
    finally { $graphics.Dispose() }

    $atlas = [System.Drawing.Bitmap]::new(54, 72, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt 64; $y++) {
            for ($x = 0; $x -lt 48; $x++) {
                $pixel = $logical.GetPixel($x, $y)
                if ($pixel.A -eq 0) { continue }
                $atlasX = $x + 2 * [Math]::Floor($x / 16)
                $atlasY = $y + 2 * [Math]::Floor($y / 16)
                $atlas.SetPixel($atlasX, $atlasY, $pixel)
            }
        }
        $atlas.Save((Join-Path $Root 'Content/Tiles/DeadTree.png'), [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $atlas.Dispose(); $logical.Dispose() }
}

Save-DeadTreeAtlas

Write-Host "Generated Engraft pixel assets in $Root"
