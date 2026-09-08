param([string]$EvidenceLog = (Join-Path $PSScriptRoot '../Art/Validation/ArrivalPod-2026-09-07/native.log'))
$ErrorActionPreference = 'Stop'
$validator = Join-Path $PSScriptRoot 'Test-ArrivalPodLive.ps1'
$source = [IO.File]::ReadAllText((Resolve-Path -LiteralPath $EvidenceLog))
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanPodLiveControls-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
function Invoke-Control([string]$Name,[string]$Text,[string]$Expected='') {
    $path = Join-Path $scratch "$Name.log"
    [IO.File]::WriteAllText($path,$Text)
    $output = (& pwsh -NoProfile -File $validator -LogPath $path -RequireReload 2>&1 | Out-String)
    if($Expected){if($LASTEXITCODE -eq 0 -or !$output.Contains($Expected)){throw "Negative control failed: $Name $output"}}
    elseif($LASTEXITCODE -ne 0){throw "Positive evidence failed: $output"}
    Write-Output "PASS: $Name"
}
Invoke-Control baseline $source
Invoke-Control missing-start ($source -replace '(?m)^.*ARRIVAL POD MATRIX START:.*\r?\n','') 'NO_MATRIX_START'
Invoke-Control missing-cell ($source -replace '(?m)^.*ARRIVAL POD CHECK PASS: copper-pick-power-cell-2-3-one-drop-no-remnants.*\r?\n','') 'MISSING_OR_DUPLICATE_CHECK'
Invoke-Control duplicate-cell ($source + "`nARRIVAL POD CHECK PASS: copper-pick-power-cell-2-3-one-drop-no-remnants") 'MISSING_OR_DUPLICATE_CHECK'
Invoke-Control incomplete ($source -replace '(?m)^.*ARRIVAL POD MATRIX PASS:.*\r?\n','') 'MATRIX_NOT_COMPLETED'
Invoke-Control stale-success ($source + "`nARRIVAL POD MATRIX START: newer-unfinished-run") 'MISSING_OR_DUPLICATE_CHECK'
Invoke-Control later-failure ($source + "`nLIVE VALIDATION REQUEST FAILED: arrival-pod-reload") 'LATER_FAILURE'
Invoke-Control missing-guard ($source -replace '(?m)^.*ARRIVAL POD GROVE GUARD:.*\r?\n','') 'NO_GROVE_GUARD'
Invoke-Control no-reload ($source -replace '(?m)^.*ARRIVAL POD RELOAD PASS:.*\r?\n','') 'RELOAD_NOT_PROVEN'
Invoke-Control wrong-reload ($source -replace '(ARRIVAL POD RELOAD PASS: unchanged=)[A-F0-9]{64}', ('${1}' + ('0' * 64))) 'RELOAD_NOT_PROVEN'
Write-Output "PASS: archived native evidence plus 9 defective real-CLI controls. Synthetic mutations are validator tests only. Retained: $scratch"
