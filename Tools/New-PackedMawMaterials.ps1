param([Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot
$output=[IO.Path]::GetFullPath($OutputDirectory)
$allowed=[IO.Path]::GetFullPath((Join-Path $repo 'Art/Candidates'))+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)){throw 'CANDIDATES_ONLY'}
if(Test-Path -LiteralPath $output){throw 'OUTPUT_EXISTS'}
$sourceRoot=Join-Path $repo 'Art/Candidates/MawTerrain-Harsh-v1/Sources'
$native=Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences'
$names=@('soil','stone','grass','sand','mud','clay','snow','ice','bone','fibers','membrane','amber')
foreach($name in $names | Where-Object {$_ -notin @('grass','stone')}){if(-not(Test-Path -LiteralPath (Join-Path $sourceRoot "$name.png"))){throw "MISSING_SOURCE $name"}}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'PackedMaterialCompiler.cs.txt'))
New-Item -ItemType Directory -Path $output | Out-Null
$fields=Join-Path $output 'Fields'
New-Item -ItemType Directory -Path $fields | Out-Null
foreach($name in $names | Where-Object {$_ -notin @('grass','stone')}){
 [PackedMaterialCompiler]::Prepare((Join-Path $sourceRoot "$name.png"),(Join-Path $fields "$name.png"))
}
Copy-Item -LiteralPath (Join-Path $repo 'Art/Candidates/MawTerrain-Harsh-v1/Native-v2/Stone/material-native.png') -Destination (Join-Path $fields 'stone.png')
$entries=@()
foreach($name in $names){
 $material=Join-Path $fields "$(if($name -eq 'grass'){'soil'}else{$name}).png"
 $overlay=Join-Path $fields 'fibers.png'
 $tileRef=switch($name){soil{'Dirt'} grass{'Grass'} sand{'Sand'} mud{'Mud'} snow{'Snow'} ice{'Ice'} default{'Stone'}}
 $wallRef=switch($name){soil{'DirtUnsafe'} grass{'DirtUnsafe'} sand{'Sandstone'} mud{'MudUnsafe'} snow{'SnowUnsafe'} ice{'IceUnsafe'} default{'Stone'}}
 foreach($kind in @('Tile','Wall')){
  $wall=$kind -eq 'Wall';$grass=$name -eq 'grass' -and -not $wall
  $reference=Join-Path $native "Vanilla-$(if($wall){$wallRef}else{$tileRef})-$(if($wall){'Wall'}else{'Tile'}).png"
  $relative="$name/$kind";$folder=Join-Path $output $relative
  New-Item -ItemType Directory -Path $folder -Force | Out-Null
  $size=[PackedMaterialCompiler]::Build($material,$reference,$folder,$grass,$wall,$overlay)
  $checks=[PackedMaterialCompiler]::Verify($material,$reference,$folder,$grass,$wall,$overlay)
  $pins=@($material,$reference,(Join-Path $folder 'atlas.png'),(Join-Path $folder 'map.bin'))
  if($grass){$pins+= $overlay}
  $pins=@($pins | ForEach-Object {[ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath $_).Hash}})
  $entries+= [ordered]@{name=$name;folder=$relative;materialPath=$material;referencePath=$reference;overlayPath=$overlay;grass=$grass;wall=$wall;width=$size[0];height=$size[1];masks=$size[2];nativeFrames=$size[3];rawBytes=$size[0]*$size[1]*4;checkedPixels=$checks;pins=$pins}
  Write-Output "$name $kind : $($size[0])x$($size[1]); $($size[2]) masks; $checks verified pixels"
 }
}
[ordered]@{schemaVersion=1;mode='diagnostic-only';fitting='128px center sampling, 16-color deterministic quantization; four-pixel periodic edge fitting; no whole-image mirroring';maskContract='Rendered native frame bodies, compact role-mask dictionary, two transparent guard pixels. Grass has soil/foliage semantic masks. Walls have 32px overlap and a world-aligned -8px origin.';entries=$entries} | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $output 'recipe.json') -Encoding utf8
