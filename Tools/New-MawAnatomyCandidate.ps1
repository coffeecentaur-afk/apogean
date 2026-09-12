param(
 [Parameter(Mandatory)][string]$OutputDirectory,
 [string]$SourcePath=(Join-Path (Split-Path $PSScriptRoot) 'Art/Candidates/MawRibSurface-v1/source.png')
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot
$output=[IO.Path]::GetFullPath($OutputDirectory)
$prefix=[IO.Path]::GetFullPath((Join-Path $repo 'Art/Candidates'))+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)){throw 'CANDIDATES_ONLY'}
if(Test-Path -LiteralPath $output){throw 'OUTPUT_EXISTS'}
$source=(Resolve-Path -LiteralPath $SourcePath).Path
if(-not $source.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)){throw 'CANDIDATE_SOURCE_ONLY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'PackedMaterialCompiler.cs.txt'))
New-Item -ItemType Directory -Path $output | Out-Null
$rib=Join-Path $output 'rib-field.png'
[PackedMaterialCompiler]::Prepare($source,$rib)
$fiber=Join-Path $repo 'Art/Candidates/MawTerrain-Harsh-v1/Packed-v1/Fields/fibers.png'
$native=Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences'
$entries=@()
foreach($key in @('rib','cap')){
 $material=if($key -eq 'rib'){$rib}else{$fiber}
 $grass=$key -eq 'cap'
 $reference=Join-Path $native "Vanilla-$(if($grass){'Grass'}else{'Stone'})-Tile.png"
 $folder=Join-Path $output $key
 New-Item -ItemType Directory -Path $folder | Out-Null
 $size=[PackedMaterialCompiler]::Build($material,$reference,$folder,$grass,$false,$fiber)
 $checks=[PackedMaterialCompiler]::Verify($material,$reference,$folder,$grass,$false,$fiber)
 $pins=@($source,$material,$reference,$fiber,(Join-Path $folder 'atlas.png'),(Join-Path $folder 'map.bin')) | Select-Object -Unique | ForEach-Object {
  [ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath $_).Hash}
 }
 $entries+= [ordered]@{name=$key;folder=$folder;material=$material;reference=$reference;grass=$grass;overlay=$fiber;size=$size;checks=$checks;pins=@($pins)}
 Write-Output "$key : $size; $checks exact masked pixels."
}
[ordered]@{schemaVersion=1;mode='separate-diagnostic-only';entries=$entries} | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $output 'recipe.json') -Encoding utf8
