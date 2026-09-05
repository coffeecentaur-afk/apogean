param(
    [Parameter(Mandatory = $true)][string]$Trunk,
    [Parameter(Mandatory = $true)][string]$Branches,
    [Parameter(Mandatory = $true)][string]$Tops,
    [string]$TrunkReference = '',
    [int]$ExpectedTrunkWidth = 176,
    [int]$ExpectedTrunkHeight = 264,
    [int]$ExpectedBranchWidth = 84,
    [int]$ExpectedBranchHeight = 126,
    [int]$ExpectedTopWidth = 246,
    [int]$ExpectedTopHeight = 82,
    [double]$MaximumBranchOpaqueRatio = 0.35,
    [double]$MaximumTopOpaqueRatio = 0.32,
    [int]$MaximumOpaqueColors = 24
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$failures = [Collections.Generic.List[string]]::new()

function Test-Texture([string]$label, [string]$path, [int]$width, [int]$height, [double]$maximumOpaqueRatio) {
    $resolved = (Resolve-Path -LiteralPath $path).Path
    $bitmap = [Drawing.Bitmap]::new($resolved)
    try {
        if ($bitmap.Width -ne $width -or $bitmap.Height -ne $height) {
            $failures.Add("$label is $($bitmap.Width)x$($bitmap.Height); expected ${width}x${height}")
        }
        $opaque = 0
        $soft = 0
        $colors = [Collections.Generic.HashSet[int]]::new()
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $soft++ }
                if ($pixel.A -eq 255) { $opaque++; [void]$colors.Add($pixel.ToArgb()) }
            }
        }
        if ($soft -gt 0) { $failures.Add("$label contains $soft soft-alpha pixels") }
        if ($colors.Count -gt $MaximumOpaqueColors) { $failures.Add("$label uses $($colors.Count) colors; maximum is $MaximumOpaqueColors") }
        if ($opaque -eq 0) { $failures.Add("$label is empty") }
        if ($maximumOpaqueRatio -gt 0) {
            $ratio = $opaque / [double]($bitmap.Width * $bitmap.Height)
            if ($ratio -gt $maximumOpaqueRatio) {
                $failures.Add("$label opaque ratio $([Math]::Round($ratio, 3)) exceeds $maximumOpaqueRatio; likely a canopy or oversized mass")
            }
        }
    }
    finally { $bitmap.Dispose() }
}

function Test-AlphaTopology([string]$candidatePath, [string]$referencePath) {
    $candidate = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $candidatePath).Path)
    $reference = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $referencePath).Path)
    try {
        if ($candidate.Height -ne $reference.Height -or $candidate.Width -gt $reference.Width) {
            $failures.Add("trunk reference is $($reference.Width)x$($reference.Height), candidate is $($candidate.Width)x$($candidate.Height) and cannot be compared as its leading native segment")
            return
        }
        $mismatches = 0
        for ($y = 0; $y -lt $candidate.Height; $y++) {
            for ($x = 0; $x -lt $candidate.Width; $x++) {
                if (($candidate.GetPixel($x, $y).A -gt 0) -ne ($reference.GetPixel($x, $y).A -gt 0)) { $mismatches++ }
            }
        }
        if ($mismatches -gt 0) { $failures.Add("trunk changes $mismatches pixels of the authoritative vanilla alpha topology") }
    }
    finally { $candidate.Dispose(); $reference.Dispose() }
}

function Test-TopSocket([string]$path) {
    $bitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $path).Path)
    try {
        # Ordinary ModTree tops are three 80x80 frames separated by two-pixel
        # gutters. The renderer rotates each top around the bottom-center wind
        # anchor, so every frame needs a centered, trunk-width socket that
        # reaches the final visible row and overlaps upward for several rows.
        if ($bitmap.Width -ne 246 -or $bitmap.Height -lt 80) { return }

        for ($frame = 0; $frame -lt 3; $frame++) {
            $centerX = $frame * 82 + 40
            $bottomOpaque = [Collections.Generic.List[int]]::new()
            for ($x = $centerX - 8; $x -le $centerX + 8; $x++) {
                if ($bitmap.GetPixel($x, 79).A -eq 255) { $bottomOpaque.Add($x) }
            }

            if ($bottomOpaque.Count -lt 7) {
                $failures.Add("top frame $frame does not reach the wind anchor with a trunk-width socket")
                continue
            }

            $socketMidpoint = ($bottomOpaque[0] + $bottomOpaque[$bottomOpaque.Count - 1]) / 2.0
            if ([Math]::Abs($socketMidpoint - $centerX) -gt 1.5) {
                $failures.Add("top frame $frame socket is not centered on the trunk/wind anchor")
            }

            # Row counts alone accept severed tips: eleven wide rows plus a fully
            # transparent row used to pass. Follow only the wood connected to the
            # bottom-center anchor, using edge-connected pixels inside the socket.
            $connected = [Collections.Generic.HashSet[int]]::new()
            $pending = [Collections.Generic.Queue[int]]::new()
            if ($bitmap.GetPixel($centerX, 79).A -eq 255) {
                [void]$connected.Add(195) # 11 * 17 + 8
                $pending.Enqueue(195)
            }
            while ($pending.Count -gt 0) {
                $position = $pending.Dequeue()
                $localX = $position % 17
                $localY = [int][Math]::Floor($position / 17.0)
                foreach ($step in @(@(-1, 0), @(1, 0), @(0, -1), @(0, 1))) {
                    $nx = $localX + $step[0]
                    $ny = $localY + $step[1]
                    if ($nx -lt 0 -or $nx -ge 17 -or $ny -lt 0 -or $ny -ge 12) { continue }
                    $next = $ny * 17 + $nx
                    if (-not $connected.Contains($next) -and $bitmap.GetPixel($centerX - 8 + $nx, 68 + $ny).A -eq 255) {
                        [void]$connected.Add($next)
                        $pending.Enqueue($next)
                    }
                }
            }
            for ($y = 79; $y -ge 68; $y--) {
                $rowOpaque = 0
                for ($x = $centerX - 8; $x -le $centerX + 8; $x++) {
                    if ($connected.Contains(($y - 68) * 17 + ($x - $centerX + 8))) { $rowOpaque++ }
                }
                $minimum = if ($y -eq 79) { 7 } else { 5 }
                if ($rowOpaque -lt $minimum) {
                    $failures.Add("top frame $frame socket loses connected trunk-width overlap at row $y ($rowOpaque pixels, minimum $minimum)")
                    break
                }
            }
        }
    }
    finally { $bitmap.Dispose() }
}

Test-Texture 'Trunk' $Trunk $ExpectedTrunkWidth $ExpectedTrunkHeight 0
Test-Texture 'Branches' $Branches $ExpectedBranchWidth $ExpectedBranchHeight $MaximumBranchOpaqueRatio
Test-Texture 'Tops' $Tops $ExpectedTopWidth $ExpectedTopHeight $MaximumTopOpaqueRatio
Test-TopSocket $Tops
if (-not [string]::IsNullOrWhiteSpace($TrunkReference)) { Test-AlphaTopology $Trunk $TrunkReference }

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: tree sheets satisfy dimensions, hard alpha, palette, sparse leafless mass, and trunk-matched top-socket contracts. Live grove validation is still required.' -ForegroundColor Green
