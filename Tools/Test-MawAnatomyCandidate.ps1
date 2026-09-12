param([string]$CandidateDirectory=(Join-Path (Split-Path $PSScriptRoot) 'Art/Candidates/MawRibSurface-v1/Native-v1'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$recipe=Get-Content -Raw -LiteralPath (Join-Path $CandidateDirectory 'recipe.json') | ConvertFrom-Json
if($recipe.schemaVersion -ne 1 -or $recipe.mode -ne 'separate-diagnostic-only' -or @($recipe.entries).Count -ne 2){throw 'RECIPE'}
if((@($recipe.entries.name | Sort-Object) -join ',') -ne 'cap,rib'){throw 'DUPLICATE_OR_MISSING_MATERIAL'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'PackedMaterialCompiler.cs.txt'))
foreach($entry in $recipe.entries){
 if($entry.name -notin @('rib','cap')){throw 'UNKNOWN_MATERIAL'}
 foreach($pin in $entry.pins){if((Get-FileHash -LiteralPath $pin.path).Hash -ne $pin.sha256){throw 'PIN_CHANGED'}}
 # Test the requested output, not the original folder embedded in a copied recipe.
 $checked=[PackedMaterialCompiler]::Verify($entry.material,$entry.reference,(Join-Path $CandidateDirectory $entry.name),$entry.grass,$false,$entry.overlay)
 if($checked -ne $entry.checks){throw 'CHECK_COUNT'}
 Write-Output "PASS $($entry.name): $checked independently replayed native mask pixels. Not visual acceptance."
}
