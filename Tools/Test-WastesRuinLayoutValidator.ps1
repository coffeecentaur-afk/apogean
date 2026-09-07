param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$validator=Join-Path $PSScriptRoot 'Test-WastesRuinLayout.ps1'
& pwsh -NoProfile -File $validator
if($LASTEXITCODE -ne 0){throw 'Layout baseline failed'}
foreach($control in @(@('MissingGroup','VISIBLE_BANK_MISMATCH'),@('PhaseJump','PHASE_JUMP'),@('WrongAsset','VISIBLE_BANK_MISMATCH'),@('OldDensity','LANDMARK_DENSITY_NOT_REDUCED'))) {
    $output=& pwsh -NoProfile -File $validator -Mutation $control[0] 2>&1
    if($LASTEXITCODE -eq 0 -or ($output -join "`n") -notmatch $control[1]) {
        throw "Layout control not meaningfully rejected: $($control[0]); $output"
    }
    Write-Host "PASS: rejected $($control[0]) via layout CLI"
}
Write-Host 'Static layout/validator proof only; no native composition approval.'
