param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
# Compatibility entrypoint. September8 replaces hard-cap policy with smooth
# altitude response. Replay historical evidence with its original Git revision.
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-WastesSmoothFlight.ps1') -ProjectRoot $ProjectRoot
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
