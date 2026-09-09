param([string]$CandidateDirectory='',
      [ValidateSet(16,32)][int]$Width=16,
      [ValidateSet(32,48,64)][int]$Height=48)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$candidate=if($CandidateDirectory){[IO.Path]::GetFullPath($CandidateDirectory)}else{Join-Path $root 'Art/Candidates/MawTooth-v2/Native-v1'}
$validator=Join-Path $PSScriptRoot 'Test-MawSlimTooth.ps1'
$trial=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanSlimToothChecks-'+[Guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $trial
$before=(Get-FileHash (Join-Path $candidate 'tooth.png')).Hash
& pwsh -NoProfile -File $validator -CandidateDirectory $candidate -Width $Width -Height $Height
if ($LASTEXITCODE -ne 0) { throw 'POSITIVE_CONTROL_FAILED' }
Add-Type -AssemblyName System.Drawing
$cases=@{
    'wide'='SLIM_TOOTH_DIMENSIONS'; 'split'='SEVERED_TOOTH_ROW';
    'soft'='SOFT_ALPHA'; 'white'='BRIGHT_EDGE_OR_SPECKLE';
    'padding'='ATLAS_PADDING'; 'blunt'='BLUNT_TIP'; 'shift'='ATLAS_REASSEMBLY_CHANGED_ART'
}
$results=@()
foreach ($name in ($cases.Keys | Sort-Object)) {
    $directory=Join-Path $trial $name; $null=New-Item -ItemType Directory -Path $directory
    $sprite=[Drawing.Bitmap]::new((Join-Path $candidate 'tooth.png'))
    $atlas=[Drawing.Bitmap]::new((Join-Path $candidate 'upright-art-atlas.png'))
    try {
        $px=0; while ($sprite.GetPixel($px,0).A -eq 0) { $px++ }
        switch ($name) {
            'wide' { $sprite.Dispose(); $sprite=[Drawing.Bitmap]::new($Width*2,$Height) }
            'split' {
                $middle=[int]($Height/2);$frameY=[int][Math]::Floor($middle/16)*18+$middle%16
                foreach ($x in 0..($Width-1)) {
                    $frameX=[int][Math]::Floor($x/16)*18+$x%16
                    $sprite.SetPixel($x,$middle,[Drawing.Color]::FromArgb(0)); $atlas.SetPixel($frameX,$frameY,[Drawing.Color]::FromArgb(0))
                }
            }
            'soft' { $sprite.SetPixel($px,0,[Drawing.Color]::FromArgb(128,110,100,80)) }
            'white' { $sprite.SetPixel($px,0,[Drawing.Color]::White) }
            'padding' { $atlas.SetPixel($atlas.Width-1,$atlas.Height-1,[Drawing.Color]::Black) }
            'blunt' { foreach ($x in 0..($Width-1)) { $sprite.SetPixel($x,0,[Drawing.Color]::FromArgb(110,100,80)) } }
            'shift' { $atlas.SetPixel($px,0,[Drawing.Color]::FromArgb(0)) }
        }
        $sprite.Save((Join-Path $directory 'tooth.png'),[Drawing.Imaging.ImageFormat]::Png)
        $atlas.Save((Join-Path $directory 'upright-art-atlas.png'),[Drawing.Imaging.ImageFormat]::Png)
    } finally { $sprite.Dispose(); $atlas.Dispose() }
    $output=(& pwsh -NoProfile -File $validator -CandidateDirectory $directory -Width $Width -Height $Height 2>&1 | Out-String)
    if ($LASTEXITCODE -eq 0 -or $output -notmatch $cases[$name]) { throw "NEGATIVE_CONTROL_FAILED_$name $output" }
    Write-Host "PASS: actual CLI rejects $name ($($cases[$name]))."
    $results+=@{case=$name; expected=$cases[$name]; rejected=$true}
}
if ((Get-FileHash (Join-Path $candidate 'tooth.png')).Hash -ne $before) { throw 'CONTROL_CHANGED_CANDIDATE' }
@{ positiveControl=$true; nativePhysicsTested=$false; originalUnchanged=$true; negativeControls=$results } | ConvertTo-Json -Depth 5
Write-Host "Mutation copies retained outside project: $trial"
