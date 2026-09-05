param(
    [Parameter(Mandatory)]
    [ValidateSet('ground','jump','wings','sky','left','right','sunset','night','rain','eclipse','release')]
    [string]$Case,
    [string]$TModLoaderRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader')
)
# The in-engine receiver additionally requires the named disposable SP world.
$request = Join-Path $TModLoaderRoot 'Captures/ApogeanLiveValidation.request'
Set-Content -LiteralPath $request -Value "wastes-camera-$Case" -Encoding ascii -NoNewline
Write-Host "Requested live camera $Case; hold expires after 1800 ticks."
