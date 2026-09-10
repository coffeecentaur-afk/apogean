# Bounded archival only: no game input, source edits, or world/save access.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot
$captures=Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures'
$destination=Join-Path $repo 'Art/Validation/MawMaterials-2026-09-09'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$files=[ordered]@{
 'Apogean Maw Material 20260910-015443.png'='stone-v1-rejected.png'
 'Apogean Maw Material 20260910-020632.png'='stone-v2-day.png'
 'Apogean Maw Material 20260910-020738.png'='stone-v2-night.png'
 'Apogean Maw Family row1 20260910-023226.png'='family-row1.png'
 'Apogean Maw Family row2 20260910-023246.png'='family-row2.png'
 'Apogean Maw Family row3 20260910-023312.png'='family-row3.png'
 'Apogean Maw Family row3 20260910-023331.png'='family-row3-night.png'
 'Apogean Maw Family row1 20260910-023509.png'='family-row1-reloaded.png'
}
$pins=@()
foreach($pair in $files.GetEnumerator()) {
 $source=Join-Path $captures $pair.Key;$target=Join-Path $destination $pair.Value
 if(Test-Path -LiteralPath $target) {
  if((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $target).Hash){throw "ARCHIVE_CONFLICT $target"}
 } else {Copy-Item -LiteralPath $source -Destination $target}
 $pins+=[ordered]@{file=$pair.Value;originalName=$pair.Key;sha256=(Get-FileHash -LiteralPath $target).Hash}
}
$art=Join-Path $repo 'Art/Candidates/MawTerrain-Harsh-v1'
$sources=@('terrain-study.png','Native-v1/stone-source.png','Native-v1/stone-prompt.txt')
foreach($name in @('soil','sand','mud','clay','snow','ice','bone','fibers','membrane','amber')) {
 $sources+="Sources/$name.png";$sources+="Sources/$name-prompt.txt"
}
$sourcePins=@($sources | ForEach-Object {[ordered]@{file="Art/Candidates/MawTerrain-Harsh-v1/$_";sha256=(Get-FileHash -LiteralPath (Join-Path $art $_)).Hash}})
[ordered]@{schemaVersion=1;captureOrigin='Terraria CaptureManager; original files unchanged';captures=$pins;sourceArt=$sourcePins} |
 ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $destination 'provenance.json') -Encoding utf8
Write-Output "Archived $($pins.Count) native captures and $($sourcePins.Count) source/prompt pins."
