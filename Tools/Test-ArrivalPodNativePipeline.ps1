Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$baseline=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v1'
$coarse=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v2'
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
$coarseFiles=@('ArrivalPod.png','ArrivalPod_Tile.png','mask-proposal.png','mask.png','cutout.png','preview.png','context-scale.png','native-report.json','cutout.png.report.json')
foreach($f in $coarseFiles){$pinned+=Join-Path $coarse $f}
$pinned+=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/A5-ImpactReview/design-a5-impact.png'
$before=@{}; foreach($p in $pinned){$before[$p]=(Get-FileHash $p).Hash}
function Invoke-Check([string]$Directory,[string]$Expected='',[int]$Cluster=1) {
    $lines=(& pwsh -NoProfile -File $validator -Directory $Directory -PixelClusterSize $Cluster 2>&1 | Out-String)
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
Invoke-Check $coarse '' 2
# The old (valid but overly detailed) native export must fail the new grid rule.
Invoke-Check $baseline 'COARSE_PIXEL_GRID' 2
foreach($defect in @('singleColor','singleHole')) {
    $dir=Join-Path $temp $defect; [void](New-Item -ItemType Directory -Path $dir)
    $art=[Drawing.Bitmap]::new((Join-Path $coarse 'ArrivalPod.png'))
    $sheet=[Drawing.Bitmap]::new((Join-Path $coarse 'ArrivalPod_Tile.png'))
    try {
        if($art.GetPixel(40,60).A -ne 255){throw 'COARSE_CONTROL_ANCHOR_INVALID'}
        $color=[Drawing.Color]::FromArgb(0,0,0,0)
        if($defect -eq 'singleColor') {
            $old=$art.GetPixel(40,60).ToArgb()
            for($y=0;$y -lt 96;$y++){for($x=0;$x -lt 80;$x++){
                $p=$art.GetPixel($x,$y)
                if($p.A -eq 255 -and $p.ToArgb() -ne $old){$color=$p;break}
            };if($color.A -eq 255){break}}
        }
        Set-PixelPair $art $sheet 40 60 $color
        $art.Save((Join-Path $dir 'ArrivalPod.png'),[Drawing.Imaging.ImageFormat]::Png)
        $sheet.Save((Join-Path $dir 'ArrivalPod_Tile.png'),[Drawing.Imaging.ImageFormat]::Png)
    } finally {$art.Dispose();$sheet.Dispose()}
    Invoke-Check $dir 'COARSE_PIXEL_GRID' 2
}
$coarseOverwrite=(& pwsh -NoProfile -File $generator -Variant Native-v2 -OutputDirectory $coarse 2>&1 | Out-String)
if($LASTEXITCODE -eq 0 -or !$coarseOverwrite.Contains('OUTPUT_EXISTS')){throw 'COARSE_OVERWRITE_NOT_REJECTED'}
$coarseProduction=(& pwsh -NoProfile -File $generator -Variant Native-v2 -OutputDirectory (Join-Path $repo 'Content/ForbiddenPodProbe') 2>&1 | Out-String)
if($LASTEXITCODE -eq 0 -or !$coarseProduction.Contains('PRODUCTION_OUTPUT_FORBIDDEN')){throw 'COARSE_PRODUCTION_PATH_NOT_REJECTED'}
if(Test-Path (Join-Path $repo 'Content/ForbiddenPodProbe')){throw 'PRODUCTION_DIRECTORY_CREATED'}
$coarseRepeat=Join-Path $temp 'coarseRepeat'; [void](New-Item -ItemType Directory -Path $coarseRepeat)
$coarseLog=(& pwsh -NoProfile -File $generator -Variant Native-v2 -OutputDirectory $coarseRepeat 2>&1 | Out-String)
if($LASTEXITCODE -ne 0){throw "COARSE_REPEAT_FAILED: $coarseLog"}
Invoke-Check $coarseRepeat '' 2
foreach($f in @('mask.png','cutout.png','ArrivalPod.png','ArrivalPod_Tile.png','preview.png','context-scale.png','native-report.json','cutout.png.report.json')) {
    if((Get-FileHash (Join-Path $coarseRepeat $f)).Hash -ne (Get-FileHash (Join-Path $coarse $f)).Hash){throw "COARSE_NONDETERMINISTIC: $f"}
}
foreach($p in $pinned){if((Get-FileHash $p).Hash -ne $before[$p]){throw "BASELINE_CHANGED: $p"}}
Write-Output "PASS: both variants + repeated exports; 11 defective/missing inputs and 3 coarse-grid controls rejected; both variants refuse overwrite/Content. Original 5 image hashes and coarse 6 image/2 report hashes reproduced; $($pinned.Count) inputs/outputs preserved. Temporary controls: $temp"
