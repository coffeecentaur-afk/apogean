param(
    [Parameter(Mandatory = $true)][string]$ReferenceAtlas,
    [Parameter(Mandatory = $true)][string]$EvidenceDirectory
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$shell = (Get-Process -Id $PID).Path
$referencePath = (Resolve-Path -LiteralPath $ReferenceAtlas).Path
# Installed-engine Stone export used ONLY as a conventional connected-block
# topology control. This is not the final bone artwork or a universal tile mask.
$referenceHash = '48907D0C61D9B68997C33FD25B0BAADB0A2D8276B6E759761C14E6B153C917EC'
if ((Get-FileHash -LiteralPath $referencePath).Hash -ne $referenceHash) {
    throw 'Native topology reference fingerprint changed. Review the new export before updating the contract.'
}
$evidencePath = [IO.Path]::GetFullPath($EvidenceDirectory)
$candidateRoot = [IO.Path]::GetFullPath((Join-Path $root 'Art/Candidates/MawBone-v1'))
if (-not $evidencePath.StartsWith($candidateRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Evidence must stay in a subdirectory of Art/Candidates/MawBone-v1; never Content or the game installation.'
}
if (-not (Test-Path -LiteralPath $evidencePath)) { New-Item -ItemType Directory -Path $evidencePath | Out-Null }
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanBoneControls-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$candidate = Join-Path $evidencePath 'OssuaryBone-topology-probe.png'
$results = [Collections.Generic.List[object]]::new()
$productionBefore = (Get-FileHash -LiteralPath (Join-Path $root 'Content/Tiles/OssuaryBone.png')).Hash

# Mechanical topology/palette control through the existing compiler. No new
# creative texture is authored here; retain this provenance in the review.
& (Join-Path $PSScriptRoot 'New-TModLoaderPaletteAtlas.ps1') -Source $referencePath -Destination $candidate -Palette @(
    '#29251e', '#4a4231', '#71644a', '#978460', '#bba578', '#d7c295', '#e6d6b2', '#ede0c2'
)

function Check-Atlas([string]$Name, [string]$Path, [bool]$Pass, [string]$FailurePattern = '', [bool]$UseReference = $true) {
    $argsForCheck = @('-NoProfile', '-File', (Join-Path $PSScriptRoot 'Test-OssuaryBoneAtlas.ps1'), '-Atlas', $Path)
    if ($UseReference) { $argsForCheck += @('-ReferenceAtlas', $referencePath) }
    $lines = & $shell @argsForCheck 2>&1
    $code = $LASTEXITCODE
    $message = $lines -join [Environment]::NewLine
    if (($code -eq 0) -ne $Pass) { throw "$Name returned unexpected exit $code. $message" }
    if (-not $Pass -and $message -notmatch $FailurePattern) { throw "$Name failed for the wrong reason: $message" }
    $results.Add([pscustomobject]@{ case = $Name; expectedPass = $Pass; exitCode = $code; output = $message })
    Write-Host "CONTROL PASS: $Name"
}
function Save-Mutation([string]$Name, [scriptblock]$Mutation) {
    $bitmap = [Drawing.Bitmap]::new($candidate)
    $path = Join-Path $scratch ($Name + '.png')
    try { & $Mutation $bitmap; $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png) }
    finally { $bitmap.Dispose() }
    return $path
}

Check-Atlas 'compiled native topology' $candidate $true
Check-Atlas 'native Stone positive control' $referencePath $true
$squares = Save-Mutation 'square-frames' {
    param($b)
    for ($y = 0; $y -lt $b.Height; $y++) {
        for ($x = 0; $x -lt $b.Width; $x++) {
            $color = if ($x % 18 -lt 16 -and $y % 18 -lt 16) {
                [Drawing.Color]::FromArgb(255,113,100,74)
            } else { [Drawing.Color]::Transparent }
            $b.SetPixel($x,$y,$color)
        }
    }
}
Check-Atlas 'recreated square-frame defect' $squares $false 'no exterior/isolated' $false

# Locate samples from the actual mask instead of assuming an interior frame.
$control = [Drawing.Bitmap]::new($candidate)
try {
    $solid = $null; $empty = $null
    for ($y = 0; $y -lt $control.Height; $y++) {
        for ($x = 0; $x -lt $control.Width; $x++) {
            if ($control.GetPixel($x, $y).A -eq 255 -and $null -eq $solid) { $solid = @($x, $y) }
            if ($control.GetPixel($x, $y).A -eq 0 -and $null -eq $empty) { $empty = @($x, $y) }
        }
    }
    if ($null -eq $solid -or $null -eq $empty) { throw 'Positive control must contain both foreground and negative space.' }
}
finally { $control.Dispose() }

$p = Save-Mutation 'missing-edge-pixel' { param($b) $b.SetPixel($solid[0], $solid[1], [Drawing.Color]::Transparent) }
Check-Atlas 'one lost native pixel' $p $false 'alpha topology at 1 pixels'
$p = Save-Mutation 'filled-negative-space' { param($b) $b.SetPixel($empty[0], $empty[1], [Drawing.Color]::FromArgb(255,113,100,74)) }
Check-Atlas 'one added native pixel' $p $false 'alpha topology at 1 pixels'
$p = Save-Mutation 'white-fringe' { param($b) $b.SetPixel($solid[0], $solid[1], [Drawing.Color]::White) }
Check-Atlas 'opaque white fringe' $p $false 'opaque-white'
$p = Save-Mutation 'magenta-key' { param($b) $b.SetPixel($solid[0], $solid[1], [Drawing.Color]::Magenta) }
Check-Atlas 'magenta exporter key' $p $false 'visible exporter key'
$p = Save-Mutation 'legacy-key' { param($b) $b.SetPixel($solid[0], $solid[1], [Drawing.Color]::FromArgb(255,247,119,249)) }
Check-Atlas 'legacy pink exporter key' $p $false 'visible exporter key'
$p = Save-Mutation 'soft-alpha' { param($b) $b.SetPixel($solid[0], $solid[1], [Drawing.Color]::FromArgb(128,113,100,74)) }
Check-Atlas 'soft contour' $p $false 'soft-alpha'
$p = Save-Mutation 'palette-overrun' {
    param($b)
    $index = 0
    for ($y = 0; $y -lt $b.Height -and $index -lt 20; $y++) {
        for ($x = 0; $x -lt $b.Width -and $index -lt 20; $x++) {
            if ($b.GetPixel($x,$y).A -eq 255) {
                $b.SetPixel($x,$y,[Drawing.Color]::FromArgb(255,1,2,30 + $index))
                $index++
            }
        }
    }
}
Check-Atlas 'palette budget' $p $false 'opaque colors; maximum'
$blank = [Drawing.Bitmap]::new(288,270)
$p = Join-Path $scratch 'blank.png'
try { $blank.Save($p,[Drawing.Imaging.ImageFormat]::Png) } finally { $blank.Dispose() }
Check-Atlas 'empty artwork' $p $false 'no opaque artwork'
$wrongSize = [Drawing.Bitmap]::new(286,270)
$p = Join-Path $scratch 'wrong-size.png'
try { $wrongSize.Save($p,[Drawing.Imaging.ImageFormat]::Png) } finally { $wrongSize.Dispose() }
Check-Atlas 'wrong stride envelope' $p $false 'Width is 286, expected 288'

if ((Get-FileHash -LiteralPath (Join-Path $root 'Content/Tiles/OssuaryBone.png')).Hash -ne $productionBefore) {
    throw 'The isolated pipeline unexpectedly modified production bone.'
}
$report = [ordered]@{
    schema = 1; date = (Get-Date -Format 'yyyy-MM-dd')
    scope = 'Offline topology/compiler/validator controls only. Stone-colored-as-bone diagnostic, not original art or native rendering.'
    nativeGameplayTested = $false; artApproved = $false
    referenceSha256 = $referenceHash; candidateSha256 = (Get-FileHash -LiteralPath $candidate).Hash
    productionBoneSha256 = $productionBefore; productionUnchanged = $true
    controls = $results.ToArray()
}
$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidencePath 'checks.json') -Encoding utf8
Write-Host "BONE PIPELINE: PASS ($($results.Count) CLI controls). Native rendering and original art remain unproved."
