Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanCurveControls-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch|Out-Null
$source=Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v4'
$validator=Join-Path $PSScriptRoot 'Test-MawClusterPlayerCurves.ps1'
foreach($case in 'padding','contact','wrong-curve','missing-inset','metadata'){
    $dir=Join-Path $scratch $case;New-Item -ItemType Directory -Path $dir|Out-Null
    foreach($file in 'cluster.png','cluster-atlas.png','contact-mask.bin','recipe.json'){Copy-Item -LiteralPath (Join-Path $source $file) -Destination $dir}
    if($case-eq'contact'){
        $path=Join-Path $dir 'contact-mask.bin';$bytes=[IO.File]::ReadAllBytes($path);$bytes[4*4096]=1-$bytes[4*4096];[IO.File]::WriteAllBytes($path,$bytes)
    }elseif($case-eq'metadata'){
        $path=Join-Path $dir 'recipe.json';$record=Get-Content -Raw $path|ConvertFrom-Json;$record.styleWrapLimit=4;$record|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $path
    }else{
        $path=Join-Path $dir 'cluster-atlas.png';$read=[Drawing.Bitmap]::new($path);$edited=[Drawing.Bitmap]::new($read);$read.Dispose()
        try{
            if($case-eq'padding'){$edited.SetPixel(16,0,[Drawing.Color]::White)}
            if($case-eq'wrong-curve'){for($y=0;$y-lt72;$y++){for($x=0;$x-lt72;$x++){$edited.SetPixel(288+$x,$y,$edited.GetPixel($x,$y))}}}
            if($case-eq'missing-inset'){$edited.SetPixel(728,72,[Drawing.Color]::White)}
            $edited.Save($path,[Drawing.Imaging.ImageFormat]::Png)
        }finally{$edited.Dispose()}
    }
    $result=& pwsh -NoProfile -File $validator -CandidateDirectory $dir 2>&1
    if($LASTEXITCODE-eq0 -or ($result -join ' ')-notmatch 'CURVES_'){throw "FALSE_PASS_OR_UNRELATED_FAILURE: $case"}
    Write-Host "PASS: actual atlas validator rejects $case"
}
$runtime=Get-Content -Raw (Join-Path $PSScriptRoot '../Common/Geometry/MawToothPlacement.cs')
foreach($case in 'tip-follows-facing','wall-always-up'){
    $code=if($case-eq'tip-follows-facing'){$runtime.Replace('0 => playerFacing == 1 ? 0 : 1','0 => playerFacing == 1 ? 1 : 0')}else{$runtime.Replace('_ => playerCenterY < toothCenterY ? 1 : 0','_ => 0')}
    if($code-eq$runtime){throw 'MUTATION_NOT_APPLIED'}
    $path=Join-Path $scratch "$case.cs";Set-Content -LiteralPath $path -Value $code
    $result=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawToothPlacement.ps1') -SourcePath $path 2>&1
    if($LASTEXITCODE-eq0 -or ($result -join ' ')-notmatch 'CURVE_POLICY:'){throw "FALSE_PASS_OR_UNRELATED_FAILURE: $case"}
    Write-Host "PASS: runtime policy test rejects $case"
}
$generator=Join-Path $PSScriptRoot 'New-MawClusterPlayerCurves.ps1'
$replay=Join-Path $scratch 'replay'
& pwsh -NoProfile -File $generator -OutputDirectory $replay
if($LASTEXITCODE-ne0){throw 'REPLAY_FAILED'}
foreach($file in 'cluster.png','cluster-atlas.png','contact-mask.bin','faces.png'){
    if((Get-FileHash (Join-Path $replay $file)).Hash-ne(Get-FileHash (Join-Path $source $file)).Hash){throw 'NONDETERMINISTIC_REPLAY'}
}
foreach($guard in @(@($replay,'OUTPUT_EXISTS'),@((Join-Path $PSScriptRoot '../Content'),'PRODUCTION_OUTPUT_FORBIDDEN'))){
    $result=& pwsh -NoProfile -File $generator -OutputDirectory $guard[0] 2>&1
    if($LASTEXITCODE-eq0 -or ($result -join ' ')-notmatch $guard[1]){throw "OUTPUT_GUARD_FAILED: $($guard[1])"}
}
Write-Host "PASS: deterministic replay and both output refusals. Seven real-CLI negative controls retained at $scratch"
