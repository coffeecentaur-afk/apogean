param(
 [Parameter(Mandatory)][string]$ManifestPath,
 [Parameter(Mandatory)][string]$OutputPath,
 [Parameter(Mandatory)][ValidateSet('running','exited')][string]$Phase,
 [Parameter(Mandatory)][int]$ServerProcessId
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $OutputPath){throw 'Evidence already exists; no overwrite.'}
$manifest=Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
$sandbox=[IO.Path]::GetFullPath($manifest.sandbox)
$expectedBase=[IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) 'ApogeanHeadless'))+[IO.Path]::DirectorySeparatorChar
if(-not $sandbox.StartsWith($expectedBase,[StringComparison]::OrdinalIgnoreCase)){throw 'Not an owned headless sandbox.'}
$process=Get-CimInstance Win32_Process -Filter "ProcessId=$ServerProcessId"
$connections=@(Get-NetTCPConnection | Where-Object OwningProcess -eq $ServerProcessId | ForEach-Object {
 [ordered]@{localAddress=$_.LocalAddress;localPort=$_.LocalPort;remoteAddress=$_.RemoteAddress;remotePort=$_.RemotePort;state=$_.State.ToString()}
})
if($process -and ($process.ExecutablePath -ne $manifest.executable -or -not $process.CommandLine.Contains($sandbox))){throw 'PID is not the isolated server; refuse reused/other process.'}
$runningScope=$process -and @($connections|Where-Object {$_.state -eq 'Listen' -and $_.localAddress -eq '127.0.0.1' -and $_.localPort -eq 17777}).Count -eq 1 -and
 @($connections|Where-Object {$_.state -eq 'Listen' -and ($_.localAddress -ne '127.0.0.1' -or $_.localPort -ne 17777)}).Count -eq 0
$lifecycle=if($Phase -eq 'running'){[bool]$runningScope}else{(-not $process) -and $connections.Count -eq 0}
$sources=@(foreach($entry in $manifest.inputs){
 $copy=[IO.Path]::GetFullPath($entry.copy)
 if(-not $copy.StartsWith($sandbox+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Copy escaped sandbox.'}
 $sourceHash=(Get-FileHash -LiteralPath $entry.source).Hash
 [ordered]@{name=[IO.Path]::GetFileName($entry.source);originalSha256=$entry.sha256;sourceSha256=$sourceHash;
  sourceUnchanged=($entry.sha256 -eq $sourceHash);copySha256=(Get-FileHash -LiteralPath $copy).Hash}
})
$logPath=Join-Path $manifest.workingDirectory 'tModLoader-Logs/server.log'
$stream=[IO.File]::Open($logPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
$buffer=[IO.MemoryStream]::new()
try{$stream.CopyTo($buffer);$bytes=$buffer.ToArray()}finally{$stream.Dispose();$buffer.Dispose()}
$raw=[Text.Encoding]::UTF8.GetString($bytes)
if(-not $raw.Contains('Saves Are Located At: '+$sandbox)){throw 'Server log does not belong to this isolated saves directory.'}
$errors=@([regex]::Matches($raw,'(?m)^\[[^\r\n]*(?:/ERROR\]|Silently Caught Exception:)[\s\S]*?(?=^\[|\z)')|ForEach-Object{$_.Value.TrimEnd()})
$records=@($raw -split '\r?\n' | Where-Object {
 $_ -match 'Starting tModLoader server|Launch Parameters:|Saves Are Located At:|Selected (apogean|CheatSheet)|Mods using the most RAM:|RAM physical:|RAM virtual:|Mod Load Completed|Loading World:|\[StatusText\]: (Loading world data|Settling liquids|Starting server|Server started|Saving world data|Validating world save|Backing up world file|Saving modded world data)|Listening on port|SSDP search|\[apogean\]: MAW (RIB CONTOUR|SHALLOW) SAVE:'
})
$unchanged=@($sources|Where-Object {-not $_.sourceUnchanged}).Count -eq 0
$result=[ordered]@{schemaVersion=1;utc=[DateTime]::UtcNow.ToString('o');phase=$Phase;processId=$ServerProcessId;
 lifecycleScope=$lifecycle;originalSourcesUnchanged=$unchanged;connections=$connections;sources=$sources;
 sourceLogSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes));records=$records;errors=$errors;
 limits='Read-only loopback/listener and original-file guards plus verbatim scoped server log. Copied world hashes may legitimately change on save. No proof of all mod tags, client synchronization, active simulation, performance under players, or visual correctness. UPnP is disabled by flags; logged SSDP discovery is retained rather than claiming no network traffic.'}
$outputStream=[IO.File]::Open([IO.Path]::GetFullPath($OutputPath),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$writer=[IO.StreamWriter]::new($outputStream);$writer.Write(($result|ConvertTo-Json -Depth 9));$writer.Flush()}finally{$outputStream.Dispose()}
if(-not $lifecycle -or -not $unchanged){throw 'Headless lifecycle/original-source guard failed; report retained.'}
Write-Output "Recorded $Phase headless evidence, $($records.Count) scoped records/$($errors.Count) error stacks; original inputs unchanged. No blanket multiplayer acceptance."
