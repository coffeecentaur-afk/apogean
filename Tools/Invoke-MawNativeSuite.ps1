param(
    [Parameter(Mandatory)][string]$EvidencePath,
    [ValidateRange(3,30)][int]$TimeoutSeconds=15,
    [string]$LogPath='E:/SteamLibrary/steamapps/common/tModLoader/tModLoader-Logs/client.log'
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
# This never starts the game, enters worlds, builds fixtures or resets a hash.
# The native request handler independently requires gg/V3/single-player.
if(Test-Path -LiteralPath $EvidencePath){throw 'Evidence already exists; use a new path.'}
$null=Get-Item -LiteralPath $LogPath
$requests=@(
    @{Case='test';Pass='MAW NATURAL MATRIX PASS:'},
    @{Case='properties';Pass='MAW PRODUCTION PROPERTIES: 449/449 passed;'},
    @{Case='sand';Pass='MAW SAND RESTORE PASS:.*exact whole digest=True;';Also='MAW SAND PHYSICS PASS:'},
    @{Case='seams';Pass='MAW SEAM REGRESSION PASS:'},
    @{Case='negative';Pass='MAW NATURAL MUTATIONS PASS: 8 native cell controls;'},
    @{Case='test';Pass='MAW NATURAL MATRIX PASS:'}
)
try {
    foreach($request in $requests) {
        # Only records appended AFTER this request count. Existing green lines
        # cannot make an unconsumed, rejected or timed-out request pass.
        $offset=(Get-Item -LiteralPath $LogPath).Length
        & (Join-Path $PSScriptRoot 'Request-MawNaturalValidation.ps1') -Case $request.Case -Playable
        $watch=[Diagnostics.Stopwatch]::StartNew()
        $observed=''
        $passed=$false
        while($watch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
            $stream=[IO.File]::Open($LogPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
            try {
                if($stream.Length -lt $offset){throw 'Log rotated during suite; no verdict inferred.'}
                $null=$stream.Seek($offset,[IO.SeekOrigin]::Begin)
                $reader=[IO.StreamReader]::new($stream)
                try{$observed=$reader.ReadToEnd()}finally{$reader.Dispose()}
            } finally {$stream.Dispose()}
            if($observed -match 'LIVE VALIDATION REQUEST FAILED|MAW SAND (PHYSICS FAIL|RESTORE FAIL)'){
                throw "Native $($request.Case) failed. Inspect preserved evidence."
            }
            $began=$observed -match ('MAW LAB REQUEST: MawPlayableLab/'+[regex]::Escape($request.Case)+';')
            $completed=$observed -match ('MAW LAB COMPLETE: MawPlayableLab/'+[regex]::Escape($request.Case)+'(?:\r?\n|$)')
            $additional=(-not $request.ContainsKey('Also')) -or ($observed -match $request.Also)
            if($began -and $completed -and $observed -match $request.Pass -and $additional) {
                $passed=$true;break
            }
            Start-Sleep -Milliseconds 250
        }
        if(-not $passed){throw "Native $($request.Case) timed out. A queued file is NOT a pass; it was not deleted or replaced."}
        Write-Output "PASS native $($request.Case) ($([math]::Round($watch.Elapsed.TotalSeconds,2))s)."
    }
    Write-Output 'PASS six bounded native requests. Visual/manual/MP/worldgen acceptance remains separate.'
} finally {
    # Export red as well as green runs; refuse replacing earlier evidence.
    & (Join-Path $PSScriptRoot 'Export-MawNativeEvidence.ps1') -LogPath $LogPath -OutputPath $EvidencePath
}
