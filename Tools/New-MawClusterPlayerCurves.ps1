param([string]$OutputDirectory=(Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v4'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$out=[IO.Path]::GetFullPath($OutputDirectory)
$content=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Content'))
if($out-eq$content -or $out.StartsWith($content+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'PRODUCTION_OUTPUT_FORBIDDEN'}
if(Test-Path -LiteralPath $out){throw 'OUTPUT_EXISTS'}
$source=Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v3'
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawClusterRootedCandidate.ps1') -CandidateDirectory $source
if($LASTEXITCODE-ne0){throw 'LEGACY_CONTRACT_FAILED'}
$old=[Drawing.Bitmap]::new((Join-Path $source 'faces.png'))
$atlas=[Drawing.Bitmap]::new(832,144);$faces=[Drawing.Bitmap]::new(512,64)
$mask=[byte[]]::new(32768)
New-Item -ItemType Directory -Path $out | Out-Null
try{
    for($v=0;$v-lt8;$v++){for($y=0;$y-lt64;$y++){for($x=0;$x-lt64;$x++){
        $f=$v%4;$wall=$f-eq1 -or $f-eq3
        $sx=$x;$sy=$y
        if($v-ge4){if($wall){$sy=63-$y}else{$sx=63-$x}}
        $c=$old.GetPixel($f*64+$sx,$sy)
        $faces.SetPixel($v*64+$x,$y,$c)
        $cx=[int][Math]::Floor($x/16);$cy=[int][Math]::Floor($y/16)
        $atlas.SetPixel($v*72+$cx*18+$x%16,$cy*18+$y%16,$c)
        $mask[$v*4096+$y*64+$x]=[byte]($c.A-eq255)
        if($wall){$pad=if($f-eq3){8}else{0};$atlas.SetPixel($v*104+$cx*26+$x%16+$pad,72+$cy*18+$y%16,$c)}
    }}}
    $atlas.Save((Join-Path $out 'cluster-atlas.png'),[Drawing.Imaging.ImageFormat]::Png)
    $faces.Save((Join-Path $out 'faces.png'),[Drawing.Imaging.ImageFormat]::Png)
    [IO.File]::WriteAllBytes((Join-Path $out 'contact-mask.bin'),$mask)
    Copy-Item -LiteralPath (Join-Path $source 'cluster.png') -Destination (Join-Path $out 'cluster.png')
    [ordered]@{schema=4;atlasSize=@(832,144);variants=8;styleMultiplier=4;styleWrapLimit=8;savedCellStride=18;wallDrawRegionY=72;wallDrawCellStride=26;wallDrawWidth=24;legacyStyles='0..3 unchanged';newStyles='4..7: floor/ceiling horizontal reflection; walls vertical reflection';sourceSHA256=(Get-FileHash (Join-Path $source 'cluster.png')).Hash;atlasSHA256=(Get-FileHash (Join-Path $out 'cluster-atlas.png')).Hash;maskSHA256=(Get-FileHash (Join-Path $out 'contact-mask.bin')).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'recipe.json') -Encoding utf8
}finally{$old.Dispose();$atlas.Dispose();$faces.Dispose()}
Write-Host 'Compiled eight placement curves; legacy frames, rooted sockets and original colors unchanged. This is NOT simplified-art approval.'
