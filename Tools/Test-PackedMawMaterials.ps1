param([string]$CandidateDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(-not $CandidateDirectory){$CandidateDirectory=Join-Path (Split-Path $PSScriptRoot) 'Art/Candidates/MawTerrain-Harsh-v1/Packed-v2'}
$recipe=Get-Content -Raw -LiteralPath (Join-Path $CandidateDirectory 'recipe.json') | ConvertFrom-Json
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'PackedMaterialCompiler.cs.txt'))
if($recipe.schemaVersion -ne 1 -or $recipe.entries.Count -ne 24){throw 'INCOMPLETE_FAMILY'}
$total=0L
foreach($entry in $recipe.entries){
 foreach($pin in $entry.pins){if((Get-FileHash -LiteralPath $pin.path).Hash -ne $pin.sha256){throw "HASH_PIN $($pin.path)"}}
 $folder=Join-Path $CandidateDirectory $entry.folder
 $total += [PackedMaterialCompiler]::Verify($entry.materialPath,$entry.referencePath,$folder,$entry.grass,$entry.wall,$entry.overlayPath)
}
Write-Output "PASS 24 packed tile/wall atlases; $total reconstructed native-frame pixels checked. Not native render acceptance."
