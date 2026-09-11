param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanAssetContinuityControls-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$repo=Split-Path $PSScriptRoot
$real=Join-Path $repo 'Content/Diagnostics/Materials/soil/Tile.bin'
$sha=(Get-FileHash -LiteralPath $real).Hash
$size=(Get-Item -LiteralPath $real).Length
foreach($case in @('positive','changed','missing','unexpected','duplicate','traversal','invalidHash')){
    $root=Join-Path $scratch $case
    New-Item -ItemType Directory -Path (Join-Path $root 'Content') -Force | Out-Null
    $target=Join-Path $root 'Content/probe.bin'
    if($case -ne 'missing'){Copy-Item -LiteralPath $real -Destination $target}
    if($case -eq 'changed'){
        $stream=[IO.File]::OpenWrite($target);try{$stream.WriteByte(0)}finally{$stream.Dispose()}
    }
    if($case -eq 'unexpected'){Copy-Item -LiteralPath $real -Destination (Join-Path $root 'Content/extra.bin')}
    $entry=[ordered]@{path='Content/probe.bin';sha256=$sha;bytes=$size}
    if($case -eq 'traversal'){$entry.path='../probe.bin'}
    if($case -eq 'invalidHash'){$entry.sha256='not-a-hash'}
    $entries=@($entry);if($case -eq 'duplicate'){$entries+= $entry}
    $manifest=Join-Path $root 'snapshot.json'
    [ordered]@{schemaVersion=1;entries=$entries} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifest -Encoding utf8
    $result=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-QAAssetSnapshot.ps1') -BuildRoot $root -SnapshotPath $manifest 2>&1 | Out-String
    $exit=$LASTEXITCODE
    $expected=switch($case){positive{'PASS QA'}changed{'CHANGED_ASSET'}missing{'MISSING_ASSET'}unexpected{'UNEXPECTED_ASSET'}duplicate{'DUPLICATE_SNAPSHOT_PATH'}traversal{'SNAPSHOT_PATH_ESCAPE'}invalidHash{'INVALID_SNAPSHOT_HASH'}}
    if(($case -eq 'positive' -and $exit -ne 0) -or ($case -ne 'positive' -and $exit -eq 0) -or $result -notmatch $expected){throw "CONTROL_FAILED $case $result"}
    Write-Output "PASS actual CLI continuity control: $case"
}
Write-Output "Seven controls retained at $scratch; no real asset modified."
