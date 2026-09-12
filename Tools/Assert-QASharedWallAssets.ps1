param([string]$MaterialDirectory=(Join-Path $PSScriptRoot '../Content/Diagnostics/Materials'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
foreach($extension in @('png','bin')) {
 $soil=Join-Path $MaterialDirectory "soil/Wall.$extension"
 $grass=Join-Path $MaterialDirectory "grass/Wall.$extension"
 if((Get-FileHash -LiteralPath $soil -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $grass -Algorithm SHA256).Hash) {
  throw "Grass/soil wall $extension differs. Shared texture path is no longer a lossless contract."
 }
}
Write-Output 'PASS shared-wall contract: grass/soil PNG and frame-map bytes match exactly. Runtime identity still requires native snapshot.'
