param(
	[Parameter(Mandatory = $true)]
    [ValidateSet('conversion', 'vegetation', 'vegetation-view-day', 'vegetation-view-night', 'vegetation-view-night-fullmoon', 'vegetation-view-wind-left', 'vegetation-view-wind-right', 'vegetation-view-paint', 'vegetation-view-release', 'vegetation-view-properties', 'vegetation-view-growth', 'vegetation-view-coatings', 'vegetation-view-checkpoint', 'qa-save-and-quit', 'wastes-terrain', 'wastes-properties', 'material', 'grass', 'entity-scale', 'forest-background', 'forest-background-aerial', 'forest-background-night', 'forest-background-eclipse', 'desert-background', 'jungle-background', 'jungle-routing', 'snow-background', 'corruption-background', 'crimson-background', 'hallow-background', 'ocean-background', 'mushroom-background', 'underworld-background', 'kessler-construction', 'helix-construction', 'kessler-campus', 'kessler-world', 'forest-restoration-wastes', 'forest-restoration-mixed', 'forest-restoration-green')]
	[string]$Fixture,
	[string]$TModLoaderRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader')
)

$captureDirectory = Join-Path $TModLoaderRoot 'Captures'
$requestPath = Join-Path $captureDirectory 'ApogeanLiveValidation.request'
New-Item -ItemType Directory -Path $captureDirectory -Force | Out-Null
Set-Content -LiteralPath $requestPath -Value $Fixture -Encoding ascii -NoNewline
Write-Host "Requested Apogean live-validation fixture '$Fixture' at $requestPath"
