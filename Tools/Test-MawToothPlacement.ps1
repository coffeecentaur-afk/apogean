param([string]$SourcePath=(Join-Path $PSScriptRoot '../Common/Geometry/MawToothPlacement.cs'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -TypeDefinition (Get-Content -Raw -LiteralPath $SourcePath)
$checks=0
function Assert-Style($face,$dir,$playerY,$expected){
    $actual=[apogean.Common.Geometry.MawToothPlacement]::CurveStyle($face,$dir,$playerY,100)
    if($actual-ne$expected){throw "CURVE_POLICY: surface=$face facing=$dir y=$playerY expected=$expected actual=$actual"}
    $script:checks++
}
foreach($y in -200,99,100,101,300){
    Assert-Style 0 1 $y 0; Assert-Style 0 -1 $y 1
    Assert-Style 2 1 $y 1; Assert-Style 2 -1 $y 0
}
foreach($face in 1,3){foreach($dir in -1,1){
    Assert-Style $face $dir 99 1; Assert-Style $face $dir 101 0
    Assert-Style $face $dir 100 0
}}
foreach($bad in @(@(-1,1,100),@(4,1,100),@(0,0,100),@(1,1,[float]::NaN),@(1,1,[float]::PositiveInfinity))){
    $rejected=$false
    try{[apogean.Common.Geometry.MawToothPlacement]::CurveStyle($bad[0],$bad[1],$bad[2],100)|Out-Null}catch{$rejected=$true}
    if(-not$rejected){throw 'INVALID_INPUT_ACCEPTED'};$checks++
}
Write-Host "PASS: $checks checks against the runtime C# policy. Rounded backs follow facing; wall points away vertically; exact-height tie points up."
