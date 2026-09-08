param(
	[Parameter(Mandatory)][ValidateSet('build','test','day','inside','night','reload','release')][string]$Case,
	[string]$TModLoaderRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader')
)
$ErrorActionPreference = 'Stop'
$directory = Join-Path $TModLoaderRoot 'Captures'
New-Item -ItemType Directory -Path $directory -Force | Out-Null
$path = Join-Path $directory 'ApogeanLiveValidation.request'
if (Test-Path -LiteralPath $path) { throw 'A live request is pending; refusing to replace it.' }
[IO.File]::WriteAllText($path, "arrival-pod-$Case", [Text.Encoding]::ASCII)
Write-Host "Requested arrival-pod-$Case (gg / disposable QA world / single-player only)."
