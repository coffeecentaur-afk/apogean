param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanMaskExport-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null
$runner = (Get-Process -Id $PID).Path
$exporter = Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1'
$sourcePath = Join-Path $testDirectory 'source.png'
$maskPath = Join-Path $testDirectory 'mask.png'
$source = [Drawing.Bitmap]::new(8,8)
$mask = [Drawing.Bitmap]::new(8,8)
$rows = @('........','....#...','..####..','..#.##..','######..','..####..','........','........')
try {
    for ($y=0; $y -lt 8; $y++) { for ($x=0; $x -lt 8; $x++) {
        $source.SetPixel($x,$y,[Drawing.Color]::FromArgb(255,242,242,242))
        $mask.SetPixel($x,$y,$(if($rows[$y][$x] -eq '#'){[Drawing.Color]::White}else{[Drawing.Color]::Black}))
    }}
    $source.SetPixel(3,2,[Drawing.Color]::White) # Intentional pale material must survive.
    $source.SetPixel(1,4,[Drawing.Color]::FromArgb(255,24,18,11)) # One-pixel connection.
    $source.Save($sourcePath,[Drawing.Imaging.ImageFormat]::Png)
    $mask.Save($maskPath,[Drawing.Imaging.ImageFormat]::Png)
    $outputPath = Join-Path $testDirectory 'actual.png'
    $log = & $runner -NoProfile -File $exporter -SourcePath $sourcePath -MaskPath $maskPath -OutputPath $outputPath 2>&1
    if($LASTEXITCODE -ne 0){throw "VALID_EXPORT_FAILED: $log"}
    $actual = [Drawing.Bitmap]::new($outputPath)
    try {
        if($actual.Width -ne 8 -or $actual.Height -ne 8){throw 'CANVAS_CHANGED'}
        for($y=0;$y -lt 8;$y++){for($x=0;$x -lt 8;$x++){
            $pixel=$actual.GetPixel($x,$y)
            if($rows[$y][$x] -eq '#'){
                if($pixel.ToArgb() -ne $source.GetPixel($x,$y).ToArgb()){throw "KEPT_PIXEL_CHANGED_$($x)_$y"}
            } elseif($pixel.ToArgb() -ne 0){throw "REMOVE_PIXEL_$($x)_$y"}
        }}
    } finally { $actual.Dispose() }
    Write-Output 'PASS real export CLI: transparent exterior/window, preserved gray/white art and one-pixel bridge.'
    $report=Get-Content -LiteralPath ($outputPath+'.report.json') -Raw | ConvertFrom-Json
    if(-not $report.pass -or $report.keptPixels -ne 18 -or $report.transparentPixels -ne 46 -or -not $report.keptPixelsUnchanged){throw 'BAD_REPORT'}
    $baseline=(Get-FileHash -LiteralPath $outputPath).Hash
    $sourceHash=(Get-FileHash -LiteralPath $sourcePath).Hash
    $maskHash=(Get-FileHash -LiteralPath $maskPath).Hash
    $second=Join-Path $testDirectory 'second.png'
    $log=& $runner -NoProfile -File $exporter -SourcePath $sourcePath -MaskPath $maskPath -OutputPath $second -SourceSHA256 $sourceHash -MaskSHA256 $maskHash 2>&1
    if($LASTEXITCODE -ne 0 -or (Get-FileHash -LiteralPath $second).Hash -ne $baseline){throw "NONDETERMINISTIC_EXPORT: $log"}
    Write-Output 'PASS pinned inputs reproduce the exact PNG hash and measured report.'
    $cases=@{
        dimensions='MASK_DIMENSIONS_MISMATCH'; gray='MASK_NOT_BINARY_OPAQUE'
        softMask='MASK_NOT_BINARY_OPAQUE'; colored='MASK_NOT_BINARY_OPAQUE'
        empty='EMPTY_ART_MASK'; full='NO_TRANSPARENT_REGION'
        softSource='KEPT_SOURCE_NOT_OPAQUE'; hash='SOURCE_HASH_MISMATCH'
        maskHash='MASK_HASH_MISMATCH'; overwrite='OUTPUT_EXISTS'; reportCollision='REPORT_EXISTS'
    }
    foreach($case in $cases.Keys){
        $badMask=if($case -eq 'dimensions'){[Drawing.Bitmap]::new(7,8)}else{[Drawing.Bitmap]::new($maskPath)}
        $badSource=[Drawing.Bitmap]::new($sourcePath)
        $candidateMask=Join-Path $testDirectory ($case+'-mask.png')
        $candidateSource=Join-Path $testDirectory ($case+'-source.png')
        try {
            switch($case){
                gray {$badMask.SetPixel(2,2,[Drawing.Color]::FromArgb(255,128,128,128))}
                softMask {$badMask.SetPixel(2,2,[Drawing.Color]::FromArgb(128,255,255,255))}
                colored {$badMask.SetPixel(2,2,[Drawing.Color]::Red)}
                empty {for($y=0;$y -lt 8;$y++){for($x=0;$x -lt 8;$x++){$badMask.SetPixel($x,$y,[Drawing.Color]::Black)}}}
                full {for($y=0;$y -lt 8;$y++){for($x=0;$x -lt 8;$x++){$badMask.SetPixel($x,$y,[Drawing.Color]::White)}}}
                softSource {$badSource.SetPixel(2,2,[Drawing.Color]::FromArgb(128,242,242,242))}
            }
            $badMask.Save($candidateMask,[Drawing.Imaging.ImageFormat]::Png)
            $badSource.Save($candidateSource,[Drawing.Imaging.ImageFormat]::Png)
        } finally {$badSource.Dispose();$badMask.Dispose()}
        $caseOutput=if($case -eq 'overwrite'){$outputPath}else{Join-Path $testDirectory ($case+'.png')}
        if($case -eq 'reportCollision'){[IO.File]::WriteAllText($caseOutput+'.report.json','preserve me')}
        $extra=@()
        if($case -eq 'hash'){$extra=@('-SourceSHA256',('0'*64))}
        if($case -eq 'maskHash'){$extra=@('-MaskSHA256',('0'*64))}
        $log=& $runner -NoProfile -File $exporter -SourcePath $candidateSource -MaskPath $candidateMask -OutputPath $caseOutput @extra 2>&1
        if($LASTEXITCODE -eq 0 -or ($log | Out-String) -notmatch $cases[$case]){throw "EXPECTED_REJECTION_$($case): $log"}
        if($case -ne 'overwrite' -and (Test-Path -LiteralPath $caseOutput)){throw "FAILED_EXPORT_WROTE_PNG_$case"}
        if($case -eq 'reportCollision' -and [IO.File]::ReadAllText($caseOutput+'.report.json') -ne 'preserve me'){throw 'REPORT_OVERWRITTEN'}
        Write-Output "PASS real export CLI rejects $case"
    }
    if((Get-FileHash -LiteralPath $sourcePath).Hash -ne $sourceHash -or (Get-FileHash -LiteralPath $maskPath).Hash -ne $maskHash -or (Get-FileHash -LiteralPath $outputPath).Hash -ne $baseline){throw 'EXISTING_INPUT_OR_OUTPUT_CHANGED'}
} finally {
    $source.Dispose(); $mask.Dispose()
    $resolved=(Resolve-Path -LiteralPath $testDirectory).Path
    $tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')+'\'
    if($resolved -ne [IO.Path]::GetFullPath($testDirectory) -or -not $resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -or -not [IO.Path]::GetFileName($resolved).StartsWith('ApogeanMaskExport-')){throw 'UNSAFE_TEST_CLEANUP'}
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
