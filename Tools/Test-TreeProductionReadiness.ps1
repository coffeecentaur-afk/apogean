param([string]$CandidateDirectory = (Join-Path $PSScriptRoot '../Content/Tiles'))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$skillScript = Join-Path $root 'AgentSkills/tmodloader-tree-authoring/scripts/Test-TreeSet.ps1'
if (-not (Test-Path -LiteralPath $skillScript)) { throw "Missing versioned tree-authoring validator: $skillScript" }
$referenceRoot = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences'
$trunkReference = Join-Path $referenceRoot 'Vanilla-ForestTree-Trunk.png'
if (-not (Test-Path -LiteralPath $trunkReference)) { throw "Missing authoritative vanilla tree reference: $trunkReference" }

$shell = (Get-Process -Id $PID).Path
# These are the user-reviewed v3 bytes, not newly approved art. A deliberate
# later revision must update the documented approval and tests together.
$approved = @{
    'DeadForestTree.png' = 'B925D5D1BD7FFE4B1315E1D441B393D22CC1CAABBFF605F6DCD53EEBFE396433'
    'DeadForestTree_Tops.png' = '75BED027B9541217BD04C44525B568B4C16194667A61CAA364929D33CA55F394'
    'DeadForestTree_Branches.png' = 'F7970EB6682CA9052FD0432D8B1C2C4B2D21DFED25B23C5E8FBB61F820360F19'
}
foreach ($name in $approved.Keys) {
    if ((Get-FileHash -LiteralPath (Join-Path $CandidateDirectory $name)).Hash -ne $approved[$name]) {
        Write-Host "APPROVED_TREE: $name differs from the reviewed v3 asset." -ForegroundColor Red
        exit 1
    }
}
& $shell -NoProfile -ExecutionPolicy Bypass -File $skillScript `
    -Trunk (Join-Path $CandidateDirectory 'DeadForestTree.png') `
    -Branches (Join-Path $CandidateDirectory 'DeadForestTree_Branches.png') `
    -Tops (Join-Path $CandidateDirectory 'DeadForestTree_Tops.png') `
    -TrunkReference $trunkReference
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $shell -NoProfile -File (Join-Path $PSScriptRoot 'Test-SnappedTreeRevision.ps1') `
    -CandidateDirectory $CandidateDirectory `
    -BaselineDirectory (Join-Path $root 'Art/Candidates/WastesSnappedA-v1') `
    -ThicknessReferenceDirectory (Join-Path $root 'Art/Candidates/WastesSnappedA-v2')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$treeSource = Get-Content -Raw -LiteralPath (Join-Path $root 'Content/Tiles/DeadForestTree.cs')
$rootSource = Get-Content -Raw -LiteralPath (Join-Path $root 'Content/Tiles/DeadForestTreeRootGlobalTile.cs')
foreach ($contract in @('GetTexture', 'GetBranchTextures', 'GetTopTextures')) {
    if ($treeSource -notmatch [regex]::Escape($contract)) { Write-Host "FAIL: ModTree missing $contract" -ForegroundColor Red; exit 1 }
}
if ($rootSource -notmatch 'intentionally draws nothing') {
    Write-Host 'FAIL: legacy global root overlay is active or its ownership is ambiguous.' -ForegroundColor Red
    exit 1
}
Write-Host 'PASS: the fixture-pass tree candidate uses segmented ModTree assets, a validated trunk-matched top socket, and no global whole-tree/root overlay. Fresh-world distribution and user review remain required.' -ForegroundColor Green
