Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$baseline=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v1'
$validator=Join-Path $PSScriptRoot 'Test-ArrivalPodNative.ps1'
$generator=Join-Path $PSScriptRoot 'New-ArrivalPodNative.ps1'
$temp=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanPodChecks-'+[Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $temp)
$pinned=@(
    (Join-Path $baseline 'ArrivalPod.png'), (Join-Path $baseline 'ArrivalPod_Tile.png'),
    (Join-Path $baseline 'mask-proposal.png'), (Join-Path $baseline 'mask.png'),
    (Join-Path $baseline 'cutout.png'), (Join-Path $baseline 'preview.png'),
    (Join-Path $baseline 'native-report.json'), (Join-Path $baseline 'cutout.png.report.json'),
    (Join-Path $repo 'Art/Candidates/ArrivalPod-v1/design-a3-blend.png')
)
$before=@{}; foreach($p in $pinned){$before[$p]=(Get-FileHash $p).Hash}
function Invoke-Check([string]$Directory,[string]$Expected='') {
    $lines=(& pwsh -NoProfile -File $validator -Directory $Directory 2>&1 | Out-String)
    $result=$LASTEXITCODE
    if($Expected){
        if($result -eq 0 -or !$lines.Contains($Expected)){throw "NEGATIVE_CONTROL_FAILED: $Expected`n$lines"}
        Write-Output "PASS CLI rejects $Expected"
    } elseif($result -ne 0){throw "VALID_FIXTURE_REJECTED: $lines"}
}
function Set-PixelPair($Art,$Sheet,[int]$X,[int]$Y,$Color) {
    $Art.SetPixel($X,$Y,$Color)
    $sx=[int][Math]::Floor($X/16)*18+$X%16
    $sy=[int][Math]::Floor($Y/16)*18+$Y%16
    $Sheet.SetPixel($sx,$sy,$Color)
}
function Reframe($Art,$Sheet) {
    for($y=0;$y -lt 96;$y++){for($x=0;$x -lt 80;$x++){
        Set-PixelPair $Art $Sheet $x $y ($Art.GetPixel($x,$y))
    }}
}
Invoke-Check $baseline
Invoke-Check (Join-Path $temp 'missing') 'NATIVE_ART_MISSING'
$cases=[ordered]@{
    wrongSize='NATIVE_DIMENSIONS'; soft='SOFT_ALPHA'; hiddenRgb='HIDDEN_MATTE_RGB'
    keyColor='EXPORTER_KEY_COLOR'; empty='SILHOUETTE_OCCUPANCY'; palette='PALETTE_BUDGET'
    float='FLOATING_FOOT'; detached='DETACHED_PIXELS'; frame='FRAME_ROUNDTRIP'; padding='NONEMPTY_PADDING'
}
foreach($name in $cases.Keys){
    $dir=Join-Path $temp $name; [void](New-Item -ItemType Directory -Path $dir)
    $art=[Drawing.Bitmap]::new((Join-Path $baseline 'ArrivalPod.png'))
    $sheet=[Drawing.Bitmap]::new((Join-Path $baseline 'ArrivalPod_Tile.png'))
    try {
        switch($name){
            'wrongSize' { $art.Dispose(); $art=[Drawing.Bitmap]::new(81,96) }
            'soft' { Set-PixelPair $art $sheet 30 80 ([Drawing.Color]::FromArgb(128,30,30,30)) }
            'hiddenRgb' { Set-PixelPair $art $sheet 79 0 ([Drawing.Color]::FromArgb(0,200,30,30)) }
            'keyColor' { Set-PixelPair $art $sheet 30 80 ([Drawing.Color]::Magenta) }
            'empty' {
                $art.Dispose();$sheet.Dispose()
                $art=[Drawing.Bitmap]::new(80,96);$sheet=[Drawing.Bitmap]::new(90,108)
            }
            'palette' {
                for($i=0;$i -lt 25;$i++){
                    Set-PixelPair $art $sheet (20+$i%10) (70+[int][Math]::Floor($i/10)) ([Drawing.Color]::FromArgb(255,30,100+$i,220))
                }
            }
            'float' {
                for($y=0;$y -lt 95;$y++){for($x=0;$x -lt 80;$x++){$art.SetPixel($x,$y,$art.GetPixel($x,$y+1))}}
                for($x=0;$x -lt 80;$x++){$art.SetPixel($x,95,[Drawing.Color]::FromArgb(0,0,0,0))}
                Reframe $art $sheet
            }
            'detached' { Set-PixelPair $art $sheet 79 0 ($art.GetPixel(40,80)) }
            'frame' { $sheet.SetPixel(0,0,[Drawing.Color]::Red) }
            'padding' { $sheet.SetPixel(16,0,[Drawing.Color]::Red) }
        }
        $art.Save((Join-Path $dir 'ArrivalPod.png'),[Drawing.Imaging.ImageFormat]::Png)
        $sheet.Save((Join-Path $dir 'ArrivalPod_Tile.png'),[Drawing.Imaging.ImageFormat]::Png)
    } finally { $art.Dispose();$sheet.Dispose() }
    Invoke-Check $dir $cases[$name]
}
$overwrite=(& pwsh -NoProfile -File $generator -OutputDirectory $baseline 2>&1 | Out-String)
if($LASTEXITCODE -eq 0 -or !$overwrite.Contains('OUTPUT_EXISTS')){throw 'OVERWRITE_NOT_REJECTED'}
Write-Output 'PASS CLI refuses existing candidate outputs'
$production=(& pwsh -NoProfile -File $generator -OutputDirectory (Join-Path $repo 'Content/ForbiddenPodProbe') 2>&1 | Out-String)
if($LASTEXITCODE -eq 0 -or !$production.Contains('PRODUCTION_OUTPUT_FORBIDDEN')){throw 'PRODUCTION_PATH_NOT_REJECTED'}
if(Test-Path (Join-Path $repo 'Content/ForbiddenPodProbe')){throw 'PRODUCTION_DIRECTORY_CREATED'}
Write-Output 'PASS CLI refuses production Content output without creating the directory'
$repeat=Join-Path $temp 'repeat'; [void](New-Item -ItemType Directory -Path $repeat)
$repeatLog=(& pwsh -NoProfile -File $generator -OutputDirectory $repeat 2>&1 | Out-String)
if($LASTEXITCODE -ne 0){throw "REPEAT_GENERATOR_FAILED: $repeatLog"}
Invoke-Check $repeat
foreach($f in @('mask.png','cutout.png','ArrivalPod.png','ArrivalPod_Tile.png','preview.png')){
    if((Get-FileHash (Join-Path $repeat $f)).Hash -ne (Get-FileHash (Join-Path $baseline $f)).Hash){throw "NONDETERMINISTIC: $f"}
}
foreach($p in $pinned){if((Get-FileHash $p).Hash -ne $before[$p]){throw "BASELINE_CHANGED: $p"}}
Write-Output "PASS: valid baseline + repeated export, 11 defective/missing inputs rejected, overwrite refused, 5 image hashes reproduced, 9 inputs/outputs preserved. Temporary controls: $temp"
