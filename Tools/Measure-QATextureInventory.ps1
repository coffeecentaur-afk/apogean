param([Parameter(Mandatory)][string]$BuildRoot,[Parameter(Mandatory)][string]$OutputPath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $OutputPath){throw 'INVENTORY_OUTPUT_EXISTS'}
$root=(Resolve-Path -LiteralPath $BuildRoot).Path
$content=Join-Path $root 'Content'
if(-not(Test-Path -LiteralPath $content -PathType Container)){throw 'CONTENT_ROOT_MISSING'}
$entries=@(foreach($file in (Get-ChildItem -LiteralPath $content -Recurse -File | Where-Object Extension -eq '.png' | Sort-Object FullName)) {
 $stream=[IO.File]::OpenRead($file.FullName)
 try{$header=[byte[]]::new(24);$stream.ReadExactly($header)}catch{throw "PNG_HEADER_TRUNCATED $($file.Name)"}finally{$stream.Dispose()}
 $signature=[byte[]](137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82)
 for($n=0;$n -lt 16;$n++){if($header[$n] -ne $signature[$n]){throw "PNG_HEADER_INVALID $($file.Name)"}}
 function BigEndian32([int]$start){[long]$value=0;for($n=$start;$n -lt $start+4;$n++){$value=$value*256+$header[$n]};return $value}
 $width=BigEndian32 16;$height=BigEndian32 20
 if($width -lt 1 -or $height -lt 1 -or $width -gt 65536 -or $height -gt 65536){throw 'PNG_DIMENSIONS_INVALID'}
 [pscustomobject]@{path=[IO.Path]::GetRelativePath($root,$file.FullName).Replace('\','/');width=$width;height=$height;
  decodedRgba8Bytes=[long]($width*$height*4);fileBytes=$file.Length;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}
})
if(-not $entries.Count){throw 'EMPTY_PNG_INVENTORY'}
$groups=@($entries | Group-Object sha256)
[long]$all=($entries | Measure-Object decodedRgba8Bytes -Sum).Sum
[long]$unique=($groups | ForEach-Object {$_.Group[0].decodedRgba8Bytes} | Measure-Object -Sum).Sum
$report=[ordered]@{
 schemaVersion=1;capturedUtc=[DateTime]::UtcNow.ToString('o');scope='PNG header inventory, assuming one RGBA8 base level per path. NOT measured runtime RAM/VRAM, simultaneous residency, mipmaps, a full PNG decoder, or an acceptance gate.'
 pngCount=$entries.Count;uniqueFileHashCount=$groups.Count;perPathRgba8Bytes=$all;uniqueFileHashRgba8Bytes=$unique
 potentialIdenticalFileRgba8Bytes=$all-$unique
 largest=@($entries | Sort-Object decodedRgba8Bytes -Descending | Select-Object -First 20)
 duplicateFiles=@($groups | Where-Object Count -gt 1 | ForEach-Object {[ordered]@{sha256=$_.Name;paths=@($_.Group.path);perCopyRgba8Bytes=$_.Group[0].decodedRgba8Bytes}})
 entries=$entries
}
$bytes=[Text.Encoding]::UTF8.GetBytes(($report|ConvertTo-Json -Depth 7))
$out=[IO.File]::Open([IO.Path]::GetFullPath($OutputPath),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$out.Write($bytes,0,$bytes.Length)}finally{$out.Dispose()}
Write-Output ('PNG inventory: {0} paths; RGBA8 inventory {1:N1} MiB; identical-file opportunity {2:N1} MiB. Not measured residency.' -f $entries.Count,($all/1MB),(($all-$unique)/1MB))
