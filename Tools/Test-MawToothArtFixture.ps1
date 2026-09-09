Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
foreach($entry in @(@('short',16,32),@('long',16,48),@('wide',32,64))) {
    & pwsh -NoProfile -File (Join-Path $root 'Tools/Test-MawSlimTooth.ps1') -CandidateDirectory (Join-Path $root "Art/Candidates/MawToothFamily-v1/Native-v1/$($entry[0])") -Width $entry[1] -Height $entry[2]
    if($LASTEXITCODE -ne 0){throw 'ART_CONTRACT_FAILED'}
}
$tile=Join-Path $root 'Content/Tiles/MawToothArtTile.cs'
if(-not(Test-Path -LiteralPath $tile)){throw 'MISSING_NATIVE_ART_TILE'}
$code=Get-Content -Raw -LiteralPath $tile
foreach($rule in @('Main.tileSolid\[Type\] = false','Main.tileSolidTop\[Type\] = false','DrawYOffset = 4','CoordinatePadding = 2','CoordinateWidth = 16','AnchorType.SolidTile','CandidateIncluded','addTile\(Type\)')) {
    if($code -notmatch $rule){throw "ART_TILE_RULE_MISSING $rule"}
}
if($code -match 'override (bool PreDraw|void PostDraw|void ModifyLight)'){throw 'UNEXPECTED_CUSTOM_RENDERER'}
$dispatcher=Get-Content -Raw -LiteralPath (Join-Path $root 'Content/Diagnostics/TileLabPlayer.cs')
$branch=$dispatcher.IndexOf('request.StartsWith("maw-tooth-art-"')
if($branch -lt 0 -or $branch -gt $dispatcher.IndexOf('else vegetation.ClearFixture()')){throw 'UNSAFE_DISPATCH_ORDER'}
Write-Host 'PASS: art exports and static native-render seam guards. Build/live inspection still required.'
