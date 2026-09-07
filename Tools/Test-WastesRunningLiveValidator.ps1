param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanRunTraceControls-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
# Fabricated records test the validator itself, NEVER the engine or artwork.
$records=@(foreach($case in 'run-flat','run-diagonal') {
    foreach($i in 0..20) {
        $x=10000+$i*180
        $y=9000 + $(if($case -eq 'run-diagonal') {64*[math]::Sin($i*[math]::PI/5)} else {0})
        $px=[math]::Floor(2500-$x*.2)
        $py=[math]::Floor(1200-$y*.06)
        [string]::Format([cultureinfo]::InvariantCulture,
            'WASTES RUN MOTION SAMPLE: case={0}; tick={1}; cell=8; cameraX={2:F3}; cameraY={3:F3}; pixelX={4:F3}; pixelY={5:F3}; horizontal=0.200; vertical=0.060; simulatedCamera=True',
            $case,($i*60),$x,$y,$px,$py)
    }
    $travel=if($case -eq 'run-diagonal') {'128.000'} else {'0.000'}
    "WASTES RUN MOTION RESULT: case=$case; viewport=2560x1369; checks=1800; failures=0; travelX=3600.000; travelY=$travel; maxError=1.000; horizontal=0.200; vertical=0.060; simulatedCamera=True; artApproval=False"
})
$baseline=$records -join "`n"
$scenarios=@(
    @{Name='valid';Text=$baseline;Expected='PASS: run-diagonal'},
    @{Name='old-depth';Text=$baseline.Replace('horizontal=0.200','horizontal=0.300');Expected='BAD_RUN'},
    @{Name='false-zero-failures';Text=$baseline.Replace('pixelY=660.000','pixelY=800.000');Expected='REACTIVE_RUN_SAMPLE'},
    @{Name='no-diagonal-rise';Text=$baseline.Replace('travelY=128.000','travelY=0.000');Expected='WRONG_RUN_PATH'},
    @{Name='missing-result';Text=($records.Where({$_ -notlike '*RESULT: case=run-diagonal;*'}) -join "`n");Expected='MISSING_RUN'}
)
foreach($s in $scenarios) {
    if($s.Name -ne 'valid' -and $s.Text -eq $baseline) {throw "Empty mutation: $($s.Name)"}
    $path=Join-Path $scratch ($s.Name+'.log')
    [IO.File]::WriteAllText($path,$s.Text)
    $output=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-WastesRunningLive.ps1') -LogPath $path 2>&1
    $code=$LASTEXITCODE
    if(($s.Name -eq 'valid' -and $code -ne 0) -or ($s.Name -ne 'valid' -and $code -eq 0) -or ($output -join "`n") -notmatch $s.Expected) {
        throw "Trace validator failed control $($s.Name): $output"
    }
    Write-Host "PASS: running-trace validator control $($s.Name)"
}
Write-Host "Synthetic validator-only evidence at $scratch. Not native test results."
