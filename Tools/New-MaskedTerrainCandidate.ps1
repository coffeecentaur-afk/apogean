param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$ReferenceAtlas,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$SourceSHA256,
    [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$ReferenceSHA256
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=(Resolve-Path -LiteralPath $SourcePath).Path
$reference=(Resolve-Path -LiteralPath $ReferenceAtlas).Path
$output=[IO.Path]::GetFullPath($OutputDirectory)
$candidateRoot=[IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates'))
$tempPrefix=Join-Path ([IO.Path]::GetTempPath()) 'ApogeanTerrainMask-'
if(-not ($output.StartsWith($candidateRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or
    $output.StartsWith($tempPrefix,[StringComparison]::OrdinalIgnoreCase))) { throw 'OUTPUT_OUTSIDE_CANDIDATES' }
if(Test-Path -LiteralPath $output){throw 'OUTPUT_EXISTS'}
if((Get-FileHash -LiteralPath $source).Hash -ine $SourceSHA256){throw 'SOURCE_HASH_MISMATCH'}
if((Get-FileHash -LiteralPath $reference).Hash -ine $ReferenceSHA256){throw 'REFERENCE_HASH_MISMATCH'}
Add-Type -AssemblyName System.Drawing
$references=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$references+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
# Text template stays out of SDK/tModLoader C# source discovery.
Add-Type -ReferencedAssemblies $references -TypeDefinition (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'TerrainMaskMaterialCompiler.cs.txt') -Raw)
New-Item -ItemType Directory -Path $output | Out-Null
$size=[TerrainMaskMaterialCompiler]::Prepare($source,$reference,$output)
$shell=(Get-Process -Id $PID).Path
$master=Join-Path $output 'color-master.png'
$mask=Join-Path $output 'frame-mask.png'
$atlas=Join-Path $output 'OssuaryBone-candidate.png'
# Reuse the SAME proven cutout exporter. Do not fork its transparency algorithm.
& $shell -NoProfile -File (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath $master -MaskPath $mask -OutputPath $atlas -SourceSHA256 (Get-FileHash -LiteralPath $master).Hash -MaskSHA256 (Get-FileHash -LiteralPath $mask).Hash
if($LASTEXITCODE -ne 0){throw 'MASK_EXPORT_FAILED'}
& $shell -NoProfile -File (Join-Path $PSScriptRoot 'Test-OssuaryBoneAtlas.ps1') -Atlas $atlas -ReferenceAtlas $reference
if($LASTEXITCODE -ne 0){throw 'ATLAS_VALIDATION_FAILED'}
$metrics=[TerrainMaskMaterialCompiler]::Verify($master,$mask,$atlas,$reference)
[TerrainMaskMaterialCompiler]::Preview($output)
$report=[ordered]@{
    schemaVersion=1; sourceSHA256=$SourceSHA256; referenceSHA256=$ReferenceSHA256
    sourceWidth=$size[0]; sourceHeight=$size[1]; fittedMaterialWidth=128; fittedMaterialHeight=128
    fitting='Lossy64x64 nearest-center sampling, nine-color quantization,2x2 native color clusters;64 source patches with shared two-pixel edge sockets.'
    palette=@('#292722','#3b352b','#51483a','#625a4c','#736b5c','#877e6b','#9d927b','#b6aa8e','#a27836')
    mask='Exact binary visibility of the pinned native connected-terrain reference. No imported reference RGB.'
    atlasWidth=288; atlasHeight=270; opaquePixels=$metrics[0]; transparentPixels=$metrics[1]
    pairwiseColorSocketChecks=$metrics[2]; exactMaskExport=$true
    nativeRendered=$false; artApproved=$false
    limitation='Color sockets do not prove absence of visual repetition, correct gameplay frames, lighting, merges, slopes or tooth behavior.'
    compilerSHA256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'TerrainMaskMaterialCompiler.cs.txt')).Hash
}
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $output 'recipe.json') -Encoding utf8
Write-Host 'MASKED TERRAIN: offline export verified; visual and native gates remain open.'
