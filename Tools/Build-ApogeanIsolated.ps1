param(
	[string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
	[string]$TreeCandidateDirectory = '',
	[string]$WastesCutoutCandidateDirectory = '',
	[string]$WastesCityCandidateDirectory = '',
	[string]$WastesScaleCandidateDirectory = '',
	[string]$WastesDepotAssemblyDirectory = '',
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
	if ($TreeCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $TreeCandidateDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
			throw 'Tree test assets must come from this project Art/Candidates directory.'
		}
		# Package the reviewed candidate in the temporary mirror for a disposable
		# live fixture. Repository Content textures stay untouched until live proof.
		foreach ($asset in @('DeadForestTree.png', 'DeadForestTree_Branches.png', 'DeadForestTree_Tops.png')) {
			Copy-Item -LiteralPath (Join-Path $candidateRoot $asset) -Destination (Join-Path $mirrorRoot "Content/Tiles/$asset")
		}
		Write-Host "QA tree asset override: $candidateRoot (build mirror only)."
	}
	if ($WastesCutoutCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $WastesCutoutCandidateDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
			throw 'Wastes test assets must come from this project Art/Candidates directory.'
		}
		# Only these two approved repairs enter the temporary package. Keep the
		# repository's current QA art and all other biome assets unchanged.
		foreach ($entry in @(@('Station.png','Station-report.json'), @('Foreground-Deep.png','Foreground-report.json'))) {
			$asset = Join-Path $candidateRoot $entry[0]
			$report = Get-Content -Raw -LiteralPath (Join-Path $candidateRoot $entry[1]) | ConvertFrom-Json
			if (-not $report.pass -or (Get-FileHash -LiteralPath $asset).Hash -ne $report.candidateSHA256) {
				throw "Wastes candidate differs from its passing export report: $asset"
			}
			Copy-Item -LiteralPath $asset -Destination (Join-Path $mirrorRoot "Content/Backgrounds/Candidates/WastesModules/$($entry[0])")
			Write-Host "QA Wastes override: $($entry[0]); SHA256=$($report.candidateSHA256) (build mirror only)."
		}
	}
	if ($WastesCityCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $WastesCityCandidateDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
			throw 'City test assets must come from this project Art/Candidates directory.'
		}
		$asset = Join-Path $candidateRoot 'Far.png'
		$report = Get-Content -Raw -LiteralPath (Join-Path $candidateRoot 'Runtime-report.json') | ConvertFrom-Json
		if (-not $report.pass -or (Get-FileHash -LiteralPath $asset).Hash -ne $report.candidateSHA256) {
			throw 'City candidate differs from its passing runtime-fit report.'
		}
		# Re-run the independent pixel/depth audit rather than trusting a boolean
		# report. Its memory/coverage scope is not live visual acceptance.
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-WastesCityRuntime.ps1') -CandidateDirectory $candidateRoot
		if ($LASTEXITCODE -ne 0) { throw 'City runtime audit failed.' }
		$destination = Join-Path $mirrorRoot 'Content/Backgrounds/Candidates/WastesCity'
		New-Item -ItemType Directory -Path $destination -Force | Out-Null
		Copy-Item -LiteralPath $asset -Destination (Join-Path $destination 'Far.png')
		Write-Host "QA city override: $($report.candidateSHA256) (build mirror only)."
	}
	if ($WastesScaleCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $WastesScaleCandidateDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
			throw 'Scale study assets must come from this project Art/Candidates directory.'
		}
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-WastesMidScaleStudy.ps1') -CandidateDirectory $candidateRoot
		if ($LASTEXITCODE -ne 0) { throw 'Mid scale study audit failed.' }
		$destination = Join-Path $mirrorRoot 'Content/Backgrounds/Candidates/WastesScaleGallery'
		New-Item -ItemType Directory -Path $destination -Force | Out-Null
		foreach ($name in @('Station','BrokenShell','MotorDepot','Checkpoint')) {
			Copy-Item -LiteralPath (Join-Path $candidateRoot "$name-Upper.png") -Destination $destination
		}
		Write-Host 'QA ground-scale gallery included; no ordinary Mid module replacement.'
	}
	if ($WastesDepotAssemblyDirectory) {
		if (-not $WastesScaleCandidateDirectory) { throw 'Depot assembly requires the existing scale gallery.' }
		$candidateRoot = (Resolve-Path -LiteralPath $WastesDepotAssemblyDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
			throw 'Depot assembly must come from this project Art/Candidates directory.'
		}
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-WastesDepotAssembly.ps1') -CandidateDirectory $candidateRoot
		if ($LASTEXITCODE -ne 0) { throw 'Depot assembly audit failed.' }
		$asset = Join-Path $candidateRoot 'MotorDepot-Upper.png'
		Copy-Item -LiteralPath $asset -Destination (Join-Path $mirrorRoot 'Content/Backgrounds/Candidates/WastesScaleGallery/MotorDepot-Upper.png')
		Write-Host "QA depot assembly override: SHA256=$((Get-FileHash -LiteralPath $asset).Hash); ground-only; mirror only."
	}
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
