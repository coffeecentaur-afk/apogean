param([Parameter(Mandatory)][string]$CandidateDirectory,
    [ValidateSet('None','BuildingChanged','SeamGap','PaddingLeak')][string]$Fault='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$art=Join-Path $root 'Art/Candidates/WastesMidRuins-v2'
$building=Join-Path $art 'PixelStyle-v1/Selected/MotorDepot-Upper.png'
$terrain=Join-Path $art 'PixelStyle-v2/GridReview/MotorDepot-Upper.png'
$pins=@(
    @($building,'CA51A9C972F61DCD5E598AB523FD00A50416D5B672A7064CD786A3777D66045B'),
    @($terrain,'8995B665B0895670E8949EFF4CE2F5A8E1B9220771D8C6A1352B68033AF612B4'),
    @((Join-Path $art 'ScaleStudy-v3/Station-Upper.png'),'83C8EDE6580E6CEB2CEECD2C0F24078EFCADE3EB10680C8619BC3B1D9207C59B'),
    @((Join-Path $root 'Art/Candidates/WastesFarCity-v1/QA-Package-v1/Station.png'),'C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C'),
    @((Join-Path $root 'Art/Candidates/WastesFarCity-v1/QA-Package-v1/Foreground-Deep.png'),'9D039C003929EC128F43C3DFEFB3F98C22DC15488EB0AA9B0F9151C00D726FE4'),
    @((Join-Path $root 'Art/Candidates/WastesFarCity-v1/Runtime-v1/Far.png'),'D61106D719B292675607B8D9275BDCA73BE7CE01D9405606CF04EB8D1C966FE6'))
foreach($p in $pins){if((Get-FileHash -LiteralPath $p[0]).Hash -ne $p[1]){throw 'SOURCE_CHANGED'}}
$candidate=Join-Path $CandidateDirectory 'MotorDepot-Upper.png'
$report=Get-Content -Raw -LiteralPath (Join-Path $CandidateDirectory 'assembly.json') | ConvertFrom-Json
if($report.recipe -ne 'DepotAssembly-v1' -or $report.candidateSHA256 -ne (Get-FileHash -LiteralPath $candidate).Hash){throw 'REPORT_MISMATCH'}
Add-Type -AssemblyName System.Drawing
$a=[Drawing.Bitmap]::new($building)
$b=[Drawing.Bitmap]::new($terrain)
$c=[Drawing.Bitmap]::new($candidate)
$m=[Drawing.Bitmap]::new((Join-Path $CandidateDirectory 'Source-Selection.png'))
try{
    if($c.Width -ne 512 -or $c.Height -ne 460 -or $m.Width -ne 512 -or $m.Height -ne 460){throw 'DIMENSIONS'}
    switch($Fault){
        'BuildingChanged' {$c.SetPixel(120,230,[Drawing.Color]::Magenta)}
        'SeamGap' {$c.SetPixel(200,346,[Drawing.Color]::FromArgb(0,0,0,0))}
        'PaddingLeak' {$c.SetPixel(200,440,[Drawing.Color]::White)}
    }
    # Independently enumerated selection, not loaded from the generator's report.
    for($x=0;$x -lt 512;$x++){
        $join=if($x -lt 112){342}elseif($x -lt 152){344}elseif($x -lt 192){342}elseif($x -lt 240){346}elseif($x -lt 296){344}elseif($x -lt 344){342}elseif($x -lt 400){346}else{344}
        for($y=0;$y -lt 460;$y++){
            $actual=$c.GetPixel($x,$y)
            if($y -ge 432){
                if($actual.ToArgb() -ne 0){throw "PADDING_LEAK: $x,$y"}
                $expectedMask=[Drawing.Color]::Black
            }else{
                $upper=$y -lt $join
                $expected=if($upper){$a.GetPixel($x,$y)}else{$b.GetPixel($x,$y)}
                $expectedArgb=if($expected.A -eq 0){0}else{$expected.ToArgb()}
                if($actual.ToArgb() -ne $expectedArgb){
                    if($upper){throw "BUILDING_PROVENANCE: $x,$y"}else{throw "TERRAIN_PROVENANCE: $x,$y"}
                }
                if($actual.A -ne 0 -and $actual.A -ne 255){throw 'SOFT_ALPHA'}
                $expectedMask=if($upper){[Drawing.Color]::Red}else{[Drawing.Color]::Blue}
                if($x -ge 100 -and $x -le 414 -and [Math]::Abs($y-$join) -le 2 -and $actual.A -ne 255){throw "FOOTING_GAP: $x,$y"}
            }
            if($m.GetPixel($x,$y).ToArgb() -ne $expectedMask.ToArgb()){throw 'SOURCE_SELECTOR'}
        }
    }
}finally{$a.Dispose();$b.Dispose();$c.Dispose();$m.Dispose()}
Write-Output 'PASS: all235520 pixels traced; v1 upper unchanged, v2 ground unchanged, hard alpha, covered footing, empty padding. Art/flight approval not implied.'
