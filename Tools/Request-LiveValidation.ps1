param(
	[Parameter(Mandatory = $true)]
	[ValidateSet('conversion', 'vegetation', 'wastes-terrain', 'wastes-properties', 'material', 'grass', 'forest-background', 'forest-background-night', 'forest-background-eclipse', 'desert-background', 'jungle-background', 'snow-background', 'corruption-background', 'crimson-background', 'hallow-background', 'ocean-background', 'mushroom-background', 'underworld-background', 'kessler-construction', 'kessler-campus', 'kessler-world')]
	[string]$Fixture,
	[string]$TModLoaderRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader')
)

$captureDirectory = Join-Path $TModLoaderRoot 'Captures'
$requestPath = Join-Path $captureDirectory 'ApogeanLiveValidation.request'
New-Item -ItemType Directory -Path $captureDirectory -Force | Out-Null
Set-Content -LiteralPath $requestPath -Value $Fixture -Encoding ascii -NoNewline
Write-Host "Requested Apogean live-validation fixture '$Fixture' at $requestPath"
