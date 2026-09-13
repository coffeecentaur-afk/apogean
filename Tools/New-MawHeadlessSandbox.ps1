param([Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$ExpectedModSha256)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
# Prepare only. No process launch, downloads, installed config edits or original-save writes.
$repo=(Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$saves=(Resolve-Path -LiteralPath (Join-Path $repo '../..')).Path
$runtime='E:/SteamLibrary/steamapps/common/tModLoader'
$expectedMod=$ExpectedModSha256.ToUpperInvariant()
$expectedDll='D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C'
if((Get-FileHash -LiteralPath (Join-Path $runtime 'tModLoader.dll')).Hash -ne $expectedDll){throw 'Runtime pin changed; review server contract.'}
if((Get-FileHash -LiteralPath (Join-Path $saves 'Mods/apogean.tmod')).Hash -ne $expectedMod){throw 'Explicit reviewed package pin does not match; no substituted build.'}
$running=@(Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" | Where-Object {$_.CommandLine -match 'tModLoader.dll'})
if($running.Count){throw 'Close tModLoader normally before snapshotting QA saves.'}
$enabled=@(Get-Content -LiteralPath (Join-Path $saves 'Mods/enabled.json') -Raw | ConvertFrom-Json)
if($enabled.Count -ne 2 -or $enabled -cnotcontains 'CheatSheet' -or $enabled -cnotcontains 'apogean'){throw 'Enabled mod set changed; do not silently alter companions.'}
$base=[IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) 'ApogeanHeadless'))
$sandbox=Join-Path $base ([guid]::NewGuid().ToString('N'))
if(Test-Path -LiteralPath $sandbox){throw 'Sandbox already exists; preserve it.'}
$null=New-Item -ItemType Directory -Path (Join-Path $sandbox 'Worlds'),(Join-Path $sandbox 'Mods')
$inputs=@(
 @{Source=(Join-Path $saves 'Worlds/Apogee_Native_Visual_V3.wld');Relative='Worlds/Apogee_Native_Visual_V3.wld'},
 @{Source=(Join-Path $saves 'Worlds/Apogee_Native_Visual_V3.twld');Relative='Worlds/Apogee_Native_Visual_V3.twld'},
 @{Source=(Join-Path $saves 'Mods/apogean.tmod');Relative='Mods/apogean.tmod'},
 @{Source='E:/SteamLibrary/steamapps/workshop/content/1281930/2563784437/2026.7/CheatSheet.tmod';Relative='Mods/CheatSheet.tmod'},
 @{Source=(Join-Path $saves 'Mods/enabled.json');Relative='Mods/enabled.json'}
)
$records=@(foreach($inputFile in $inputs){
 $source=(Resolve-Path -LiteralPath $inputFile.Source).Path
 $before=(Get-FileHash -LiteralPath $source).Hash
 $dest=Join-Path $sandbox $inputFile.Relative
 Copy-Item -LiteralPath $source -Destination $dest
 if((Get-FileHash -LiteralPath $dest).Hash -ne $before -or (Get-FileHash -LiteralPath $source).Hash -ne $before){throw 'Snapshot mismatch or moving source.'}
 [ordered]@{source=$source;copy=$dest;sha256=$before;bytes=(Get-Item -LiteralPath $source).Length}
})
$config="maxplayers=1`nport=17777`nupnp=0`npriority=3`nlanguage=en-US`n"
[IO.File]::WriteAllText((Join-Path $sandbox 'serverconfig.txt'),$config)
$argsForServer=@('tModLoader.dll','-server','-nosteam','-noupnp','-ip','127.0.0.1','-port','17777',
 '-players','1','-forcepriority','3','-config',(Join-Path $sandbox 'serverconfig.txt'),
 '-tmlsavedirectory',$sandbox,'-modpath',(Join-Path $sandbox 'Mods'),'-world',(Join-Path $sandbox 'Worlds/Apogee_Native_Visual_V3.wld'))
$manifest=[ordered]@{schemaVersion=1;createdUtc=[DateTime]::UtcNow.ToString('o');sandbox=$sandbox;runtimeDllSha256=$expectedDll;
 workingDirectory=$runtime;executable=(Join-Path $runtime 'dotnet/dotnet.exe');arguments=$argsForServer;inputs=$records;
 limits='Copied existing QA world only, no autocreate, no players copied, loopback only, no UPnP/Steam server. Prepare does not launch. Defaults for isolated mod configs; no gameplay/modpack equivalence claim. Original saves never overwritten.'}
[IO.File]::WriteAllText((Join-Path $sandbox 'manifest.json'),($manifest|ConvertTo-Json -Depth 8))
Write-Output ($manifest|ConvertTo-Json -Depth 8)
