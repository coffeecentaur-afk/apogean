param([Parameter(Mandatory)][string]$BuildRoot,[Parameter(Mandatory)][string]$SnapshotPath,[switch]$AllowAdditionalAssets)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $BuildRoot).Path
$contentPrefix=[IO.Path]::GetFullPath((Join-Path $root 'Content'))+[IO.Path]::DirectorySeparatorChar
$manifest=Get-Content -Raw -LiteralPath $SnapshotPath | ConvertFrom-Json
if($manifest.schemaVersion -ne 1 -or @($manifest.entries).Count -eq 0){throw 'INVALID_SNAPSHOT'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$failures=[Collections.Generic.List[string]]::new()
foreach($entry in $manifest.entries){
    $path=[string]$entry.path
    $absolute=[IO.Path]::GetFullPath((Join-Path $root $path))
    if([IO.Path]::IsPathRooted($path) -or $path -match '(^|[/\\])\.\.([/\\]|$)' -or -not $absolute.StartsWith($contentPrefix,[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($path) -notin @('.png','.bin')){throw 'SNAPSHOT_PATH_ESCAPE'}
    if(-not $paths.Add($absolute)){throw 'DUPLICATE_SNAPSHOT_PATH'}
    if([string]$entry.sha256 -notmatch '^[0-9A-Fa-f]{64}$' -or $entry.bytes -lt 0){throw 'INVALID_SNAPSHOT_HASH'}
    if(-not(Test-Path -LiteralPath $absolute -PathType Leaf)){$failures.Add("MISSING_ASSET $path");continue}
    if((Get-Item -LiteralPath $absolute).Length -ne $entry.bytes -or (Get-FileHash -LiteralPath $absolute).Hash -ne $entry.sha256){$failures.Add("CHANGED_ASSET $path")}
}
if(-not $AllowAdditionalAssets){
    Get-ChildItem -LiteralPath (Join-Path $root 'Content') -Recurse -File | Where-Object {$_.Extension -in @('.png','.bin')} | ForEach-Object {
        if(-not $paths.Contains($_.FullName)){$failures.Add('UNEXPECTED_ASSET '+[IO.Path]::GetRelativePath($root,$_.FullName))}
    }
}
if($failures.Count){throw ($failures -join "`n")}
Write-Output "PASS QA asset continuity: $($paths.Count) pinned PNG/map assets. This is NOT visual acceptance."
