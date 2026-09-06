param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
# Synthetic mutation tests; never present these traces as live evidence.
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('Apogean-CameraValidators-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$checks=0
foreach($mutation in 'control','missing-case','wrong-viewport','drift','false-summary','no-checks','non-finite') {
    $lines=foreach($case in 'ground','wings','diagonal-left','diagonal-right') {
        if($mutation -eq 'missing-case' -and $case -eq 'wings'){continue}
        $viewport=if($mutation -eq 'wrong-viewport'){'2560x1440'}else{'1920x1080'}
        $errorValue=if($mutation -eq 'drift'){'10.0'}elseif($mutation -eq 'non-finite'){'NaN'}else{'0.5'}
        $passValue=if($mutation -eq 'false-summary'){'False'}else{'True'}
        $count=if($mutation -eq 'no-checks'){0}else{150}
        "WASTES V1 GROUND SAMPLE: case=$case; viewport=$viewport;"
        "WASTES V1 GROUND LOCK: case=$case; checks=$count; maxError=$errorValue; pass=$passValue; artApproval=False"
    }
    $path=Join-Path $scratch "$mutation.log"
    $lines | Set-Content -LiteralPath $path
    $output=& pwsh -NoProfile -File (Join-Path $ProjectRoot 'Tools/Test-WastesGroundLockLive.ps1') -LogPath $path -Viewport 1920x1080 2>&1
    if(($LASTEXITCODE -eq 0) -ne ($mutation -eq 'control')){throw "FAIL: validator mutation $mutation : $output"}
    $checks++
}
Write-Host "PASS: $checks synthetic ground-lock validator mutations through the real CLI. Not live evidence."

$stageChecks=0
foreach($mutation in 'control','missing-case','wrong-viewport','close-follows','early-middle-fade','no-far-stage','descent-jump','non-finite','no-checks') {
    $lines=foreach($case in 'ground','wings','mid-altitude','high-altitude','below-ground') {
        if($mutation -eq 'missing-case' -and $case -eq 'below-ground'){continue}
        $viewport=if($mutation -eq 'wrong-viewport'){'2560x1440'}else{'1920x1080'}
        $altitude=switch($case){'ground'{.01};'wings'{.21};'mid-altitude'{.34};'high-altitude'{.8};'below-ground'{0}}
        $opacity=if($case -eq 'high-altitude'){0}else{1}
        $soil=switch($case){'ground'{546};'wings'{1746};'mid-altitude'{2530};'high-altitude'{5306};'below-ground'{146}}
        if($mutation -eq 'close-follows' -and $case -eq 'wings'){$soil=600}
        if($mutation -eq 'early-middle-fade' -and $case -eq 'mid-altitude'){$opacity=.5}
        if($mutation -eq 'no-far-stage' -and $case -eq 'high-altitude'){$opacity=1}
        if($mutation -eq 'descent-jump' -and $case -eq 'below-ground'){$soil=200}
        $top=$soil-488
        if($mutation -eq 'non-finite'){$altitude='NaN'}
        $count=if($mutation -eq 'no-checks'){0}else{150}
        "WASTES V1 GROUND SAMPLE: case=$case; viewport=$viewport; soilY=$soil; gameZoom=1; closeTop=$top; altitude=$altitude; midOpacity=$opacity;"
        "WASTES V1 GROUND LOCK: case=$case; checks=$count; maxError=0.5; pass=True; artApproval=False"
    }
    $path=Join-Path $scratch "altitude-$mutation.log"
    $lines | Set-Content -LiteralPath $path
    $output=& pwsh -NoProfile -File (Join-Path $ProjectRoot 'Tools/Test-WastesAltitudeLive.ps1') -LogPath $path -Viewport 1920x1080 2>&1
    if(($LASTEXITCODE -eq 0) -ne ($mutation -eq 'control')){throw "FAIL: altitude validator mutation $mutation : $output"}
    $stageChecks++
}
Write-Host "PASS: $stageChecks synthetic altitude validator mutations through the real CLI. Not live evidence."
