param(
	[Parameter(Mandatory = $true)]
	[ValidateSet('conversion', 'vegetation', 'wastes-terrain', 'wastes-properties', 'material', 'grass', 'desert-background', 'jungle-background', 'snow-background', 'corruption-background', 'kessler-campus')]
	[string]$Fixture,
	[string]$TModLoaderRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader')
)

$captureDirectory = Join-Path $TModLoaderRoot 'Captures'
$requestPath = Join-Path $captureDirectory 'ApogeanLiveValidation.request'
New-Item -ItemType Directory -Path $captureDirectory -Force | Out-Null
Set-Content -LiteralPath $requestPath -Value $Fixture -Encoding ascii -NoNewline
Write-Host "Requested Apogean live-validation fixture '$Fixture' at $requestPath"
