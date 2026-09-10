Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$source=Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v2'
$validator=Join-Path $PSScriptRoot 'Test-MawToothCluster.ps1'
$temporary=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanClusterControls-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
foreach($case in @('dimensions','padding','white','soft','severed','mask','hash','rotation')) {
    $folder=Join-Path $temporary $case
    New-Item -ItemType Directory -Path $folder | Out-Null
    Copy-Item -LiteralPath (Join-Path $source 'cluster.png'),(Join-Path $source 'cluster-atlas.png'),(Join-Path $source 'contact-mask.bin'),(Join-Path $source 'recipe.json') -Destination $folder
    if($case-eq'mask') {
        $path=Join-Path $folder 'contact-mask.bin';$data=[IO.File]::ReadAllBytes($path);$data[0]=1;[IO.File]::WriteAllBytes($path,$data)
    } elseif($case-eq'hash') {
        $path=Join-Path $folder 'recipe.json';$r=Get-Content -Raw $path|ConvertFrom-Json;$r.spriteSHA256='INVALID';$r|ConvertTo-Json -Depth 8|Set-Content $path
    } else {
        $path=Join-Path $folder 'cluster-atlas.png'
        $loaded=[Drawing.Bitmap]::new($path);$edited=[Drawing.Bitmap]::new($loaded);$loaded.Dispose()
        try {
            switch($case){
                dimensions {$edited.Dispose();$edited=[Drawing.Bitmap]::new(70,72)}
                padding {$edited.SetPixel(16,0,[Drawing.Color]::Red)}
                white {$edited.SetPixel(0,0,[Drawing.Color]::White)}
                soft {$edited.SetPixel(0,0,[Drawing.Color]::FromArgb(128,90,80,70))}
                severed {for($x=0;$x-lt72;$x++){$edited.SetPixel($x,50,[Drawing.Color]::FromArgb(0,0,0,0))}}
                rotation {for($y=0;$y-lt72;$y++){for($x=0;$x-lt72;$x++){$edited.SetPixel(72+$x,$y,$edited.GetPixel($x,$y))}}}
            }
            $edited.Save($path,[Drawing.Imaging.ImageFormat]::Png)
        } finally {$edited.Dispose()}
    }
    & pwsh -NoProfile -File $validator -CandidateDirectory $folder *> $null
    if($LASTEXITCODE-eq0){throw "FALSE PASS: $case"}
    Write-Host "PASS: actual validator rejected $case"
}
# Retain bounded test artifacts; no recursive cleanup.
Write-Host "Eight negative controls retained at $temporary"
