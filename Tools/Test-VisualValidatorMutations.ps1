param([ValidateSet('Tree', 'Terrain', 'All')][string]$Profile = 'All')

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$shell = (Get-Process -Id $PID).Path
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('Apogean-ValidatorMutations-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $fixture | Out-Null
$failures = [Collections.Generic.List[string]]::new()
$checks = 0

function Expect-Command([string]$label, [string[]]$arguments, [bool]$accept, [string]$reason = '') {
    $output = & $shell -NoProfile -ExecutionPolicy Bypass @arguments 2>&1
    $code = $LASTEXITCODE
    $script:checks++
    $text = $output -join "`n"
    if (($code -eq 0) -ne $accept -or ($reason -and $text -notmatch $reason)) {
        $script:failures.Add("$label (exit $code; expected accept=$accept, reason=$reason)")
        Write-Host "FAIL: $label`n$text" -ForegroundColor Red
    }
    else { Write-Host "PASS: $label (exit $code)" -ForegroundColor Green }
}

function Save-SocketFixture([string]$path, [string]$kind) {
    $image = [Drawing.Bitmap]::new(246, 82)
    try {
        for ($frame = 0; $frame -lt 3; $frame++) {
            for ($y = 60; $y -le 79; $y++) {
                if ($kind -eq 'gap' -and $y -eq 73) { continue }
                if ($kind -eq 'missing' -and $frame -eq 2) { continue }
                $shift = if ($kind -eq 'curved') { [int][Math]::Floor((79 - $y) / 5.0) } else { 0 }
                if ($kind -eq 'offcenter') { $shift = 4 }
                $radius = if ($kind -eq 'neck' -and $y -eq 73) { 0 } else { 4 }
                for ($x = -$radius; $x -le $radius; $x++) {
                    $image.SetPixel($frame * 82 + 40 + $shift + $x, $y, [Drawing.Color]::SaddleBrown)
                }
            }
        }
        $image.Save($path, [Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $image.Dispose() }
}

if ($Profile -in @('Tree', 'All')) {
    foreach ($spec in @(@('trunk', 176, 264), @('branches', 84, 126))) {
        $image = [Drawing.Bitmap]::new($spec[1], $spec[2])
        try {
            $image.SetPixel(10, 10, [Drawing.Color]::SaddleBrown)
            $image.Save((Join-Path $fixture ($spec[0] + '.png')), [Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $image.Dispose() }
    }
    foreach ($case in @(@('straight', $true, ''), @('curved', $true, ''), @('gap', $false, 'socket|overlap'), @('neck', $false, 'socket|overlap'), @('missing', $false, 'socket|anchor'), @('offcenter', $false, 'centered'))) {
        $tops = Join-Path $fixture ($case[0] + '.png')
        Save-SocketFixture $tops $case[0]
        Expect-Command "tree $($case[0])" @('-File', (Join-Path $root 'AgentSkills/tmodloader-tree-authoring/scripts/Test-TreeSet.ps1'), '-Trunk', (Join-Path $fixture 'trunk.png'), '-Branches', (Join-Path $fixture 'branches.png'), '-Tops', $tops) $case[1] $case[2]
    }
    $treeMirror = Join-Path $fixture 'tree-repo'
    foreach ($relative in @('Tools/Invoke-ApogeanContentGate.ps1', 'Tools/Test-TreeProductionReadiness.ps1', 'AgentSkills/tmodloader-tree-authoring/scripts/Test-TreeSet.ps1', 'Content/Tiles/DeadForestTree.png', 'Content/Tiles/DeadForestTree_Branches.png', 'Content/Tiles/DeadForestTree_Tops.png', 'Content/Tiles/DeadForestTree.cs', 'Content/Tiles/DeadForestTreeRootGlobalTile.cs')) {
        $destination = Join-Path $treeMirror $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $root $relative) -Destination $destination
    }
    $treeGate = @('-File', (Join-Path $treeMirror 'Tools/Invoke-ApogeanContentGate.ps1'), '-Profile', 'Tree')
    Expect-Command 'tree pristine real gate' $treeGate $true
    Copy-Item -LiteralPath (Join-Path $fixture 'gap.png') -Destination (Join-Path $treeMirror 'Content/Tiles/DeadForestTree_Tops.png') -Force
    Expect-Command 'tree broken socket real gate propagation' $treeGate $false 'socket'
}

if ($Profile -in @('Terrain', 'All')) {
    # Exercise the REAL gate in an isolated minimal mirror. Copy bytes, never hard-link
    # mutable PNGs: deliberate damage must not reach production textures or references.
    $mirror = Join-Path $fixture 'repo'
    New-Item -ItemType Directory -Path $mirror | Out-Null
    Copy-Item -LiteralPath (Join-Path $root 'Tools') -Destination (Join-Path $mirror 'Tools') -Recurse
    $paths = [Collections.Generic.HashSet[string]]::new()
    foreach ($test in @('Test-ReportedVisualRegressions.ps1', 'Test-SurfaceRegression.ps1')) {
        $source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot $test)
        foreach ($match in [regex]::Matches($source, "'(Content/[^']+)'")) {
            if (Test-Path -LiteralPath (Join-Path $root $match.Groups[1].Value) -PathType Leaf) {
                [void]$paths.Add($match.Groups[1].Value)
            }
        }
    }
    foreach ($file in Get-ChildItem -Path (Join-Path $root 'Content/Tiles/Wastes*.png'), (Join-Path $root 'Content/Walls/Wastes*Unsafe.png') -File) {
        [void]$paths.Add($file.FullName.Substring($root.Length + 1))
    }
    foreach ($relative in $paths) {
        $destination = Join-Path $mirror $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $root $relative) -Destination $destination
    }
    $gateArguments = @('-File', (Join-Path $mirror 'Tools/Invoke-ApogeanContentGate.ps1'), '-Profile', 'Terrain')
    Expect-Command 'terrain pristine gate' $gateArguments $true
    foreach ($case in @(
        @('soil hole', 'Content/Tiles/WastesSoil.png', 'hole', 'alpha topology'),
        @('wall hole', 'Content/Walls/WastesDirtWallUnsafe.png', 'hole', 'alpha topology'),
        @('grass rogue white', 'Content/Tiles/WastesGrass.png', 'white', 'white.*mask|mask.*white'),
        @('soil soft alpha', 'Content/Tiles/WastesSoil.png', 'soft', 'soft-alpha'),
        @('soil wrong size', 'Content/Tiles/WastesSoil.png', 'size', 'Width|Reference is'),
        @('missing soil', 'Content/Tiles/WastesSoil.png', 'missing', 'Missing|does not exist')
    )) {
        $target = Join-Path $mirror $case[1]
        if ($case[2] -eq 'missing') {
            Move-Item -LiteralPath $target -Destination ($target + '.held')
        }
        else {
            $source = [Drawing.Bitmap]::new((Join-Path $root $case[1]))
            $image = if ($case[2] -eq 'size') { [Drawing.Bitmap]::new(18, 18) } else { [Drawing.Bitmap]::new($source) }
            try {
                :pixels for ($y = 0; $y -lt $image.Height; $y++) {
                    for ($x = 0; $x -lt $image.Width; $x++) {
                        $pixel = $image.GetPixel($x, $y)
                        if ($pixel.A -ne 255 -or $pixel.ToArgb() -eq [Drawing.Color]::White.ToArgb()) { continue }
                        $replacement = switch ($case[2]) {
                            'hole' { [Drawing.Color]::Transparent }
                            'white' { [Drawing.Color]::White }
                            'soft' { [Drawing.Color]::FromArgb(128, $pixel.R, $pixel.G, $pixel.B) }
                        }
                        $image.SetPixel($x, $y, $replacement)
                        break pixels
                    }
                }
                if ($case[2] -eq 'size') { $image.SetPixel(0, 0, [Drawing.Color]::SaddleBrown) }
                $image.Save($target, [Drawing.Imaging.ImageFormat]::Png)
            }
            finally { $source.Dispose(); $image.Dispose() }
        }
        Expect-Command "terrain $($case[0])" $gateArguments $false $case[3]
        Copy-Item -LiteralPath (Join-Path $root $case[1]) -Destination $target -Force
    }
}

Write-Host "Mutation fixtures retained at $fixture"
if ($failures.Count -gt 0) {
    Write-Host "VISUAL VALIDATOR MUTATIONS: FAIL ($($failures.Count)/$checks)" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" }
    exit 1
}
Write-Host "VISUAL VALIDATOR MUTATIONS: PASS ($checks checks); live/art approval is separate." -ForegroundColor Green
