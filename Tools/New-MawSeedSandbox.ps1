param(
    [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$ExpectedModSha256,
    [int[]]$Seeds = @(101,202,303),
    [ValidateSet(1,2,3)][int]$LayoutVersion = 1
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Prepare isolated fresh-world trials, never existing-world migrations. Does not launch a server.
$repo = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$saves = (Resolve-Path -LiteralPath (Join-Path $repo '../..')).Path
$runtime = 'E:/SteamLibrary/steamapps/common/tModLoader'
$dllPin = 'D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C'
if ((Get-FileHash -LiteralPath (Join-Path $runtime 'tModLoader.dll')).Hash -ne $dllPin) { throw 'Runtime changed; inspect the installed server contract before proceeding.' }
$mod = Join-Path $saves 'Mods/apogean.tmod'
if ((Get-FileHash -LiteralPath $mod).Hash -ne $ExpectedModSha256.ToUpperInvariant()) { throw 'Candidate package does not match the explicit build pin.' }
if ($Seeds.Count -lt 1 -or $Seeds.Count -gt 4 -or @($Seeds | Select-Object -Unique).Count -ne $Seeds.Count -or @($Seeds | Where-Object { $_ -lt 0 }).Count) { throw 'Use one to four unique nonnegative test seeds.' }
$enabled = @(Get-Content -LiteralPath (Join-Path $saves 'Mods/enabled.json') -Raw | ConvertFrom-Json)
if ($enabled.Count -ne 2 -or $enabled -cnotcontains 'apogean' -or $enabled -cnotcontains 'CheatSheet') { throw 'Companion mod set changed; no automatic substitution.' }
$sandbox = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanMawSeeds-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $sandbox
$runs = @(foreach ($seed in $Seeds) {
    $run = Join-Path $sandbox "seed-$seed"
    $null = New-Item -ItemType Directory -Path (Join-Path $run 'Worlds'),(Join-Path $run 'Mods')
    foreach ($source in @($mod,'E:/SteamLibrary/steamapps/workshop/content/1281930/2563784437/2026.7/CheatSheet.tmod',(Join-Path $saves 'Mods/enabled.json'))) {
        $before = (Get-FileHash -LiteralPath $source).Hash
        $destination = Join-Path $run ('Mods/' + [IO.Path]::GetFileName($source))
        Copy-Item -LiteralPath $source -Destination $destination
        if ((Get-FileHash -LiteralPath $destination).Hash -ne $before -or (Get-FileHash -LiteralPath $source).Hash -ne $before) { throw 'Moving source or incomplete candidate copy.' }
    }
    $worldName = if ($LayoutVersion -eq 1) { "Apogean Maw Seed QA $seed" } else { "Apogean Maw Seed V$LayoutVersion QA $seed" }
    $world = Join-Path $run ('Worlds/' + $worldName.Replace(' ','_') + '.wld')
    if (Test-Path -LiteralPath $world) { throw 'Fresh-world target already exists.' }
    $port = 17800 + [Array]::IndexOf($Seeds,$seed)
    $config = Join-Path $run 'serverconfig.txt'
    [IO.File]::WriteAllText($config,"maxplayers=1`nport=$port`nupnp=0`npriority=3`nlanguage=en-US`ndifficulty=0`n")
    [ordered]@{
        seed=$seed; layoutVersion=$LayoutVersion; name=$worldName; directory=$run; world=$world
        executable=(Join-Path $runtime 'dotnet/dotnet.exe'); workingDirectory=$runtime
        arguments=@('tModLoader.dll','-server','-nosteam','-noupnp','-ip','127.0.0.1','-port',"$port",'-players','1','-forcepriority','3',
            '-config',$config,'-tmlsavedirectory',$run,'-modpath',(Join-Path $run 'Mods'),'-world',$world,'-autocreate','3','-seed',"$seed",'-worldname',$worldName)
    }
})
$manifest = [ordered]@{
    schemaVersion=1; createdUtc=[DateTime]::UtcNow.ToString('o'); sandbox=$sandbox
    runtimeDllSha256=$dllPin; modSha256=$ExpectedModSha256.ToUpperInvariant(); runs=$runs
    limits='Fresh Large test worlds only. Loopback/no UPnP/no Steam. No player or original world copied, edited or deleted. Isolated default mod configs; not a Calamity compatibility or multiplayer gameplay certification. Prepare does not launch.'
}
[IO.File]::WriteAllText((Join-Path $sandbox 'manifest.json'),($manifest | ConvertTo-Json -Depth 8))
$manifest | ConvertTo-Json -Depth 8
