param(
	[string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
	[switch]$KeepWorkspace
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$sourceRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$projectFile = Join-Path $sourceRoot 'apogean.csproj'
$targetsFile = Join-Path (Split-Path -Parent $sourceRoot) 'tModLoader.targets'
if (-not (Test-Path -LiteralPath $projectFile)) {
	throw "Missing Apogean project file: $projectFile"
}
if (-not (Test-Path -LiteralPath $targetsFile)) {
	throw "Missing tModLoader.targets beside the mod source folder: $targetsFile"
}

# OneDrive applies an inherited deny-delete ACL to this machine's ModSources
# directory. tModLoader rebuilds compile_temp by deleting it, so builds inside
# that tree fail even when every source file is valid. Compile an exact mirror
# under the local temp directory and let tModLoader package the result normally.
$temporaryBase = Join-Path ([IO.Path]::GetTempPath()) 'ApogeanTmlBuild'
$workspace = Join-Path $temporaryBase ([Guid]::NewGuid().ToString('N'))
$mirrorRoot = Join-Path $workspace (Split-Path -Leaf $sourceRoot)
New-Item -ItemType Directory -Path $mirrorRoot -Force | Out-Null

try {
	$excludedDirectories = @(
		'/XD',
		(Join-Path $sourceRoot '.git'),
		(Join-Path $sourceRoot '.vs'),
		(Join-Path $sourceRoot 'bin'),
		(Join-Path $sourceRoot 'obj'),
		(Join-Path $sourceRoot 'compile_temp')
	)
	& robocopy.exe $sourceRoot $mirrorRoot /E /NFL /NDL /NJH /NJS /NP @excludedDirectories | Out-Null
	if ($LASTEXITCODE -gt 7) {
		throw "robocopy failed while creating the isolated build mirror (exit code $LASTEXITCODE)."
	}

	Copy-Item -LiteralPath $targetsFile -Destination (Join-Path $workspace 'tModLoader.targets')
	Push-Location $mirrorRoot
	try {
		& dotnet build '.\apogean.csproj' -v:minimal
		if ($LASTEXITCODE -ne 0) {
			throw "The isolated Apogean build failed (exit code $LASTEXITCODE)."
		}
	}
	finally {
		Pop-Location
	}

	Write-Host "PASS: isolated Apogean build completed from $mirrorRoot"
}
finally {
	if (-not $KeepWorkspace -and (Test-Path -LiteralPath $workspace)) {
		$resolvedBase = [IO.Path]::GetFullPath($temporaryBase).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
		$resolvedWorkspace = [IO.Path]::GetFullPath($workspace)
		if (-not $resolvedWorkspace.StartsWith($resolvedBase, [StringComparison]::OrdinalIgnoreCase)) {
			throw "Refusing to remove unexpected build workspace: $resolvedWorkspace"
		}
		Remove-Item -LiteralPath $resolvedWorkspace -Recurse -Force
	}
}
