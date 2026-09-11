param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$OutputPath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $OutputPath){throw 'Evidence output already exists; preserve it.'}
$source=(Resolve-Path -LiteralPath $LogPath).Path
$inputStream=[IO.File]::Open($source,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
$snapshot=[IO.MemoryStream]::new()
try{$inputStream.CopyTo($snapshot);$logBytes=$snapshot.ToArray()}finally{$inputStream.Dispose();$snapshot.Dispose()}
$lines=@([Text.Encoding]::UTF8.GetString($logBytes) -split '\r?\n' | Where-Object {$_ -match '\[apogean\]: MAW (NATURAL|LAB REQUEST|LAB COMPLETE|PROPERTY|PRODUCTION PROPERTIES|PROPERTIES RESTORE|SAND|SEAM|JOIN|FIBER)'})
if(-not $lines.Count){throw 'No matching native Maw evidence in this log.'}
$record=[ordered]@{
    schemaVersion=1
    capturedUtc=[DateTime]::UtcNow.ToString('o')
    sourceLogSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($logBytes))
    logTimeZone='America/Chicago'
    scope='Verbatim scoped native records, including failures. Not a blanket acceptance verdict.'
    records=$lines
}
$bytes=[Text.Encoding]::UTF8.GetBytes(($record|ConvertTo-Json -Depth 8))
$stream=[IO.File]::Open([IO.Path]::GetFullPath($OutputPath),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
Write-Output "Exported $($lines.Count) scoped native records. Existing evidence is never overwritten."
