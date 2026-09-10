Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$source=Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v3'
$temporary=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanRootedControls-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
foreach($case in @('padding','soft','wrong-curve','wrong-inset','mask','hash')) {
    $folder=Join-Path $temporary $case
    New-Item -ItemType Directory -Path $folder | Out-Null
    Copy-Item -LiteralPath (Join-Path $source 'cluster.png'),(Join-Path $source 'cluster-atlas.png'),(Join-Path $source 'contact-mask.bin'),(Join-Path $source 'recipe.json') -Destination $folder
    if($case-eq'mask') {
        $path=Join-Path $folder 'contact-mask.bin';$data=[IO.File]::ReadAllBytes($path);$data[0]=1;[IO.File]::WriteAllBytes($path,$data)
    } elseif($case-eq'hash') {
        $path=Join-Path $folder 'recipe.json';$r=Get-Content -Raw $path|ConvertFrom-Json;$r.atlasSHA256='INVALID';$r|ConvertTo-Json -Depth 8|Set-Content $path
    } else {
        $path=Join-Path $folder 'cluster-atlas.png';$loaded=[Drawing.Bitmap]::new($path);$edited=[Drawing.Bitmap]::new($loaded);$loaded.Dispose()
        try {
            switch($case){
                padding {$edited.SetPixel(16,0,[Drawing.Color]::Red)}
                soft {$edited.SetPixel(0,0,[Drawing.Color]::FromArgb(128,90,80,70))}
                wrong-curve {for($y=0;$y-lt64;$y++){for($x=0;$x-lt64;$x++){
                    $rx=[int][Math]::Floor($x/16)*18+$x%16;$ry=[int][Math]::Floor($y/16)*18+$y%16
                    $flipped=63-$y;$fy=[int][Math]::Floor($flipped/16)*18+$flipped%16
                    $edited.SetPixel(216+$rx,$ry,$edited.GetPixel(72+$rx,$fy))
                }}}
                wrong-inset {$edited.SetPixel(312,72,[Drawing.Color]::White)}
            }
            $edited.Save($path,[Drawing.Imaging.ImageFormat]::Png)
        } finally {$edited.Dispose()}
    }
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawClusterRootedCandidate.ps1') -CandidateDirectory $folder *> $null
    if($LASTEXITCODE-eq0){throw "FALSE PASS: $case"}
    Write-Host "PASS: actual rooted validator rejects $case"
}
Write-Host "Six negative controls retained at $temporary"
