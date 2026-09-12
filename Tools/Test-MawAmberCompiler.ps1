Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$temp=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanAmberCompiler-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
# Tiny synthetic fixtures, not content art. Exercise the actual compiler CLI:
# opaque palette membership, alpha rejection, frame gutters and wall overlap.
$tile=[Drawing.Bitmap]::new(36,18)
$wall=[Drawing.Bitmap]::new(68,34)
try {
    $amber=[Drawing.Color]::FromArgb(255,146,82,26)
    $tile.SetPixel(0,0,$amber)
    $tile.SetPixel(15,15,$amber)
    $tile.SetPixel(16,0,$amber) # Native frame gutter: must not emit.
    $tile.SetPixel(1,0,[Drawing.Color]::FromArgb(128,146,82,26))
    $tile.SetPixel(18,0,[Drawing.Color]::FromArgb(255,60,45,30))
    $wallAmber=[Drawing.Color]::FromArgb(255,90,50,16) # Integer 62% role.
    foreach($p in @(@(8,8),@(23,23),@(12,12))){$wall.SetPixel($p[0],$p[1],$wallAmber)}
    $wall.SetPixel(7,8,$wallAmber) # Overlap outside the center: excluded.
    $wall.SetPixel(24,8,$wallAmber)
    $wall.SetPixel(34+8,8,[Drawing.Color]::FromArgb(128,90,50,16))
    $tile.Save((Join-Path $temp 'tile.png'),[Drawing.Imaging.ImageFormat]::Png)
    $wall.Save((Join-Path $temp 'wall.png'),[Drawing.Imaging.ImageFormat]::Png)
} finally {$tile.Dispose();$wall.Dispose()}
$output=Join-Path $temp 'compiled'
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'New-MawAmberEmission.ps1') -TileAtlas (Join-Path $temp 'tile.png') -WallAtlas (Join-Path $temp 'wall.png') -OutputDirectory $output
if($LASTEXITCODE -ne 0){throw 'Actual emission compiler failed its positive fixture.'}
foreach($entry in @(@('tile',2,18),@('wall',3,34))) {
    $bytes=[IO.File]::ReadAllBytes((Join-Path $output ('amber-'+$entry[0]+'-light.bin')))
    if($bytes.Length-ne 24 -or [BitConverter]::ToInt32($bytes,0)-ne 0x4D454D41 -or
       [BitConverter]::ToInt32($bytes,12)-ne $entry[2] -or [BitConverter]::ToUInt16($bytes,20)-ne $entry[1] -or
       [BitConverter]::ToUInt16($bytes,22)-ne 0){throw "Wrong actual $($entry[0]) emitter counts/header."}
}
# Erase only the role-bearing test fixture; a completely quiet source must fail.
$quiet=[Drawing.Bitmap]::new(36,18)
try{$quiet.Save((Join-Path $temp 'quiet.png'),[Drawing.Imaging.ImageFormat]::Png)}finally{$quiet.Dispose()}
$negative=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'New-MawAmberEmission.ps1') -TileAtlas (Join-Path $temp 'quiet.png') -WallAtlas (Join-Path $temp 'wall.png') -OutputDirectory (Join-Path $temp 'negative') 2>&1
if($LASTEXITCODE-eq 0 -or ($negative -join "`n")-notmatch 'EMISSION_MISSING_CONTROL'){throw 'Quiet-source mutation was not specifically rejected.'}
Write-Output 'PASS actual amber compiler CLI: exact 2/0 terrain,3/0 wall counts; alpha, gutters, central overlap and quiet-source rejection. Synthetic fixtures only; not native lighting.'
