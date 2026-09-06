param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$fixtureRoot=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanModularValidator-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
try {
    $control=@('ground','below-ground','diagonal-left','diagonal-right') | ForEach-Object {
        "WASTES MODULAR RESULT: case=$_; matrixChecks=120; frameChecks=80; failures=0; maxPixelError=0.01; artApproval=False"
        "WASTES V1 GROUND LOCK: case=$_; checks=40; maxError=0.5; pass=True; artApproval=False"
        if($_ -like 'diagonal-*'){"WASTES V1 SWEEP: case=$_; coveragePass=True; artApproval=False"}
    }
    $control=$control -join "`n"
    $fixtures=@(
        @{Name='control';Text=$control;Error=$null},
        @{Name='missing';Text=($control -replace 'case=ground;','case=missing;');Error='MISSING_RESULT'},
        @{Name='zero';Text=($control -replace 'matrixChecks=120','matrixChecks=0');Error='MODULAR_FAILURE'},
        @{Name='bad-count';Text=($control -replace 'failures=0','failures=1');Error='MODULAR_FAILURE'},
        @{Name='shift';Text=($control -replace 'maxPixelError=0.01','maxPixelError=4.0');Error='MODULAR_FAILURE'},
        @{Name='anchor';Text=($control -replace 'pass=True','pass=False');Error='GROUND_LOCK_FAILURE'},
        @{Name='range';Text=($control -replace 'coveragePass=True','coveragePass=False');Error='INSUFFICIENT_SWEEP'}
    )
    foreach($fixture in $fixtures) {
        $fixturePath=Join-Path $fixtureRoot ($fixture.Name+'.log')
        [IO.File]::WriteAllText($fixturePath,$fixture.Text)
        $result=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-WastesModularLive.ps1') -LogPath $fixturePath 2>&1
        if($null -eq $fixture.Error){if($LASTEXITCODE -ne 0){throw "Positive CLI rejected: $result"}}
        elseif($LASTEXITCODE -eq 0 -or ($result -join ' ') -notmatch $fixture.Error){throw "Negative CLI failed: $($fixture.Name): $result"}
        Write-Output "PASS actual CLI validator: $($fixture.Name)"
    }
} finally {
    $resolvedFixture=[IO.Path]::GetFullPath($fixtureRoot)
    $resolvedTemporary=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')+'\'
    if(-not $resolvedFixture.StartsWith($resolvedTemporary,[StringComparison]::OrdinalIgnoreCase) -or (Split-Path -Leaf $resolvedFixture) -notlike 'ApogeanModularValidator-*'){throw 'Unsafe temporary fixture cleanup path'}
    Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
}
