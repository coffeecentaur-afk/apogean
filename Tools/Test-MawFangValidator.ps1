param([string]$CandidateDirectory = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates/MawTooth-v1/Native-v1'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$valid=Join-Path (Resolve-Path -LiteralPath $CandidateDirectory).Path 'MawFangTile.png'
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawFang.ps1') -CandidateDirectory $CandidateDirectory
if($LASTEXITCODE -ne 0){throw 'VALID_FANG_REJECTED'}
$temporary=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanFangNegative-'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $temporary
$cases=@('wrong-size','opaque-padding','soft-alpha','missing-tip','dirty-transparent-rgb')
foreach ($case in $cases) {
    $directory=Join-Path $temporary $case
    $null=New-Item -ItemType Directory -Path $directory
    $bitmap=if($case -eq 'wrong-size'){[Drawing.Bitmap]::new(54,217)}else{[Drawing.Bitmap]::new($valid)}
    try {
        switch($case) {
            'opaque-padding' {$bitmap.SetPixel(17,17,[Drawing.Color]::White)}
            'soft-alpha' {$bitmap.SetPixel(0,0,[Drawing.Color]::FromArgb(120,49,39,33))}
            'missing-tip' {$bitmap.SetPixel(0,0,[Drawing.Color]::FromArgb(0,0,0,0))}
            'dirty-transparent-rgb' {$bitmap.SetPixel(17,17,[Drawing.Color]::FromArgb(0,255,255,255))}
        }
        $bitmap.Save((Join-Path $directory 'MawFangTile.png'),[Drawing.Imaging.ImageFormat]::Png)
    } finally {$bitmap.Dispose()}
    $result=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawFang.ps1') -CandidateDirectory $directory 2>&1
    if($LASTEXITCODE -eq 0){throw "DEFECT_WAS_ACCEPTED: $case"}
    if(($result -join "`n") -notmatch 'FANG_ATLAS_DIMENSIONS|FANG_ALPHA_OCCUPANCY'){throw "WRONG_FAILURE: $case $result"}
    Write-Host "PASS: actual validator rejected $case"
}
Write-Host "Five deliberately defective atlases rejected. Test artifacts retained: $temporary"
