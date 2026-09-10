param(
 [Parameter(Mandatory)][string]$SourcePath,
 [Parameter(Mandatory)][string]$ReferenceAtlas,
 [Parameter(Mandatory)][string]$OutputDirectory,
 [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$SourceSHA256,
 [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$ReferenceSHA256
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
$allowed=[IO.Path]::GetFullPath((Join-Path $repo 'Art/Candidates'))+[IO.Path]::DirectorySeparatorChar
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(-not $output.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)){throw 'CANDIDATE_OUTPUT_ONLY'}
if(Test-Path -LiteralPath $output){throw 'OUTPUT_EXISTS'}
$source=(Resolve-Path -LiteralPath $SourcePath).Path
$reference=(Resolve-Path -LiteralPath $ReferenceAtlas).Path
if((Get-FileHash -LiteralPath $source).Hash -ine $SourceSHA256){throw 'SOURCE_HASH'}
if((Get-FileHash -LiteralPath $reference).Hash -ine $ReferenceSHA256){throw 'REFERENCE_HASH'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'WorldMaterialCompiler.cs.txt'))
New-Item -ItemType Directory -Path $output | Out-Null
[WorldMaterialCompiler]::Build($source,$reference,$output)
$count=[WorldMaterialCompiler]::Verify($output,$reference)
[ordered]@{
 schemaVersion=1;material='stone';sourcePath=$source;sourceSHA256=$SourceSHA256
 referencePath=$reference;referenceSHA256=$ReferenceSHA256
 atlasSHA256=(Get-FileHash -LiteralPath (Join-Path $output 'atlas.png')).Hash
 nativeWidth=288;nativeHeight=270;phaseCountX=8;phaseCountY=8
 width=2304;height=2160;rawBytes=19906560;checkedPixels=$count
 fitting='Uniform128x128 center sampling;12 colors;only four-pixel outer strips blend opposite edges for periodic continuity. No whole-field mirroring. Lossy fitting, not source-pixel identity.'
 topology='Pinned native Stone alpha replicated in64 banks. No reference RGB.'
 nativeRendered=$false;installed=$false
} | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $output 'recipe.json') -Encoding utf8
Write-Output "PASS compiled and verified $count pixels; no live assets changed."
