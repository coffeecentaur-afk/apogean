param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$OutputPath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $OutputPath){throw 'Evidence output already exists; preserve it.'}
$source=(Resolve-Path -LiteralPath $LogPath).Path
$inputStream=[IO.File]::Open($source,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
$snapshot=[IO.MemoryStream]::new()
try{$inputStream.CopyTo($snapshot);$logBytes=$snapshot.ToArray()}finally{$inputStream.Dispose();$snapshot.Dispose()}
$decoded=[Text.Encoding]::UTF8.GetString($logBytes)
# Engine draw failures can occur after a successful QA request. Preserve the
# relevant caught stack as well; request COMPLETE is not a render-health pass.
# Restrict this to draw stacks, not arbitrary telemetry elsewhere in client.log.
$renderFailures=@([regex]::Matches($decoded,'(?m)^\[[^\r\n]*\[tML\]: Silently Caught Exception:[\s\S]*?(?=^\[|\z)') |
    ForEach-Object { $_.Value.TrimEnd() } |
    Where-Object { $_ -match 'Terraria\.ModLoader\.SurfaceBackgroundStylesLoader\.DrawCloseBackground|Terraria\.Main\.DrawLiquid|MawCaveBackdropProbe.*\.Draw|Terraria\.Graphics\.Effects\.OverlayManager\.Draw' })
$includeStack=$false
$lines=@(foreach($line in ($decoded -split '\r?\n')) {
    if($line -match '^\[') {
        $includeStack=$line -match '\[apogean\]: LIVE VALIDATION REQUEST FAILED: (maw-|qa-perf-)|\[apogean\]: (QA PERFORMANCE|MAW SHALLOW MOTION) EXPORT FAILED'
        if($includeStack -or $line -match '\[apogean\]: MAW (ROPE|CAVE|NATURAL|LAB REQUEST|LAB COMPLETE|PROPERTY|PRODUCTION PROPERTIES|PROPERTIES RESTORE|SAND|SEAM|JOIN|FIBER|RIB|ANATOMY|HANGING|AMBER|SHALLOW)|\[apogean\]: QA (PERFORMANCE|RENDER ALLOCATION|RENDER CACHE IDENTITY)|\[apogean\]: \[DEBUG-closewidth\]'){$line}
    } elseif($includeStack) {$line}
})
if(-not $lines.Count){throw 'No matching native Maw evidence in this log.'}
$record=[ordered]@{
    schemaVersion=1
    capturedUtc=[DateTime]::UtcNow.ToString('o')
    sourceLogSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($logBytes))
    logTimeZone='America/Chicago'
    scope='Verbatim scoped native records, including failures. Not a blanket acceptance verdict.'
    records=$lines
    caughtRenderFailures=$renderFailures
}
$bytes=[Text.Encoding]::UTF8.GetBytes(($record|ConvertTo-Json -Depth 8))
$stream=[IO.File]::Open([IO.Path]::GetFullPath($OutputPath),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
Write-Output "Exported $($lines.Count) scoped native records. Existing evidence is never overwritten."
