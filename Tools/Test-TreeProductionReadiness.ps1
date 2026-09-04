Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$skillScript = Join-Path ([Environment]::GetFolderPath('UserProfile')) '.codex/skills/tmodloader-tree-authoring/scripts/Test-TreeSet.ps1'
if (-not (Test-Path -LiteralPath $skillScript)) { throw "Missing installed tree-authoring validator: $skillScript" }
$referenceRoot = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences'
$trunkReference = Join-Path $referenceRoot 'Vanilla-ForestTree-Trunk.png'
if (-not (Test-Path -LiteralPath $trunkReference)) { throw "Missing authoritative vanilla tree reference: $trunkReference" }

& $skillScript `
    -Trunk (Join-Path $root 'Content/Tiles/DeadForestTree.png') `
    -Branches (Join-Path $root 'Content/Tiles/DeadForestTree_Branches.png') `
    -Tops (Join-Path $root 'Content/Tiles/DeadForestTree_Tops.png') `
    -TrunkReference $trunkReference

$treeSource = Get-Content -Raw -LiteralPath (Join-Path $root 'Content/Tiles/DeadForestTree.cs')
$rootSource = Get-Content -Raw -LiteralPath (Join-Path $root 'Content/Tiles/DeadForestTreeRootGlobalTile.cs')
foreach ($contract in @('GetTexture', 'GetBranchTextures', 'GetTopTextures')) {
    if ($treeSource -notmatch [regex]::Escape($contract)) { Write-Host "FAIL: ModTree missing $contract" -ForegroundColor Red; exit 1 }
}
if ($rootSource -notmatch 'intentionally draws nothing') {
    Write-Host 'FAIL: legacy global root overlay is active or its ownership is ambiguous.' -ForegroundColor Red
    exit 1
}
Write-Host 'PASS: the tree candidate uses segmented ModTree assets and no global whole-tree/root overlay. Its base remains rejected until the live grove is replaced and reviewed.' -ForegroundColor Green
