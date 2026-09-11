param(
	[string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
	[string]$TreeCandidateDirectory = '',
	[string]$WastesCutoutCandidateDirectory = '',
	[string]$WastesCityCandidateDirectory = '',
	[string]$WastesScaleCandidateDirectory = '',
	[string]$WastesDepotAssemblyDirectory = '',
	[string]$WastesStationBridgeDirectory = '',
	[string]$WastesMidDepthDirectory = '',
	[string]$ArrivalPodCandidateDirectory = '',
	[string]$MawBoneCandidateDirectory = '',
	[string]$MawFangCandidateDirectory = '',
	[string]$MawToothArtCandidateDirectory = '',
	[string]$MawToothClusterCandidateDirectory = '',
	[string]$AssetSnapshotPath = '',
	[switch]$PackedMawPreview,
	[switch]$KeepWorkspace,
	[switch]$CompileOnly
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
	if ($WastesStationBridgeDirectory) {
		if (-not $WastesScaleCandidateDirectory) { throw 'Station bridge requires the existing scale gallery.' }
		$candidateRoot = (Resolve-Path -LiteralPath $WastesStationBridgeDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
			throw 'Station bridge must come from this project Art/Candidates directory.'
		}
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-WastesStationBridgeStudy.ps1') -StudyDirectory $candidateRoot
		if ($LASTEXITCODE -ne 0) { throw 'Station bridge audit failed.' }
		$asset = Join-Path $candidateRoot 'Station-Upper.png'
		# Only the named disposable ground gallery receives this upper-only art.
		# Never substitute it for WastesModules/Station's 1408px-deep texture.
		Copy-Item -LiteralPath $asset -Destination (Join-Path $mirrorRoot 'Content/Backgrounds/Candidates/WastesScaleGallery/Station-Upper.png')
		Write-Host "QA approved Station bridge override: SHA256=$((Get-FileHash -LiteralPath $asset).Hash); ground-only; mirror only."
	}
	if ($WastesMidDepthDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $WastesMidDepthDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Ruin bank must be in Art/Candidates.' }
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-WastesMidDepthAssembly.ps1') -CandidateDirectory $candidateRoot
		if ($LASTEXITCODE -ne 0) { throw 'Ruin bank audit failed.' }
		$destination = Join-Path $mirrorRoot 'Content/Backgrounds/Candidates/WastesRuinBank'
		New-Item -ItemType Directory -Path $destination -Force | Out-Null
		foreach ($name in @('Station','MotorDepot','BrokenShell','Checkpoint')) {
			Copy-Item -LiteralPath (Join-Path $candidateRoot "$name.png") -Destination $destination
		}
		# Preserve approved upper studies in the separate ground-scale fixture too.
		if ($WastesScaleCandidateDirectory) {
			foreach ($name in @('BrokenShell','Checkpoint')) {
				Copy-Item -LiteralPath (Join-Path $sourceRoot "Art/Candidates/WastesRuinUpperStyle-v1/Study/$name-Upper.png") -Destination (Join-Path $mirrorRoot 'Content/Backgrounds/Candidates/WastesScaleGallery')
			}
		}
		Write-Host 'QA full-depth bank included; named disposable Wastes path only, no production art promotion.'
	}
	if ($ArrivalPodCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $ArrivalPodCandidateDirectory).Path
		$v1Root = (Resolve-Path -LiteralPath (Join-Path $sourceRoot 'Art/Candidates/ArrivalPod-v1/Native-v1')).Path
		$v2Root = (Resolve-Path -LiteralPath (Join-Path $sourceRoot 'Art/Candidates/ArrivalPod-v1/Native-v2')).Path
		$v3Root = (Resolve-Path -LiteralPath (Join-Path $sourceRoot 'Art/Candidates/ArrivalPod-v1/Native-v3')).Path
		$pixelClusterSize = 1
		$flatFooting = $false
		$entries = @(
			@('ArrivalPod_Tile.png', 'ArrivalPodTile.png', '880E4B45CEF78E984246F7C84F878EC0711C948E8719B9B519970B946A7C4996'),
			@('ArrivalPod.png', 'ArrivalPodItem.png', '63D4F04E0E76BE08B4997BEE2B4F919749415B1E3261D9DF8EF776070C310A8B')
		)
		if ($candidateRoot -eq $v2Root) {
			# User requested native scale review; this is NOT final visual approval.
			$pixelClusterSize = 2
			$entries = @(
				@('ArrivalPod_Tile.png', 'ArrivalPodTile.png', '6A60495AC6F473FFF6237B9BFDAC1AB88622CD2AC3794C148C4100605F1A65DC'),
				@('ArrivalPod.png', 'ArrivalPodItem.png', '822A50B8EE634A671281F2CFC5AA3EE760031E0DA3757246C8216911D83B6407')
			)
		} elseif ($candidateRoot -eq $v3Root) {
			$pixelClusterSize = 2
			$flatFooting = $true
			$entries = @(
				@('ArrivalPod_Tile.png', 'ArrivalPodTile.png', '9EA7FE54A2B75C175AF995D7848B8EB9FEBE53BA3F8457525DBDE3C7EADAD678'),
				@('ArrivalPod.png', 'ArrivalPodItem.png', '8F0C3E52F1367B8B2AFD71FD490954AA6504C1FDFFB2A173070C7E15E51A912D')
			)
		} elseif ($candidateRoot -ne $v1Root) { throw 'Pod build requires a specifically pinned Native-v1, Native-v2 or Native-v3 review candidate.' }
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-ArrivalPodNative.ps1') -Directory $candidateRoot -PixelClusterSize $pixelClusterSize -RequireFlatFooting:$flatFooting
		if ($LASTEXITCODE -ne 0) { throw 'Pod atlas audit failed.' }
		foreach ($entry in $entries) {
			$asset = Join-Path $candidateRoot $entry[0]
			if ((Get-FileHash -LiteralPath $asset).Hash -ne $entry[2]) { throw 'Pod art differs from the pinned review candidate.' }
			Copy-Item -LiteralPath $asset -Destination (Join-Path $mirrorRoot "Content/Tiles/Diagnostics/$($entry[1])")
		}
		Write-Host "Pinned pod review candidate included: $candidateRoot; $pixelClusterSize-pixel grid; isolated build enables bounded new-world arrival QA, never existing-world regeneration."
	}
	if ($MawBoneCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $MawBoneCandidateDirectory).Path
		$approvedRoot = (Resolve-Path -LiteralPath (Join-Path $sourceRoot 'Art/Candidates/MawBone-v1/MaskedNative-v2')).Path
		if ($candidateRoot -ne $approvedRoot) { throw 'Only the reviewed MaskedNative-v2 bone is authorized for this fixture.' }
		$asset = Join-Path $candidateRoot 'OssuaryBone-candidate.png'
		if ((Get-FileHash -LiteralPath $asset).Hash -ne '5B4721D3A914EE5AEFD9D56AF95C0F883052BFB633005BC842E5CD101A5CF19E') {
			throw 'Bone differs from the approved candidate.'
		}
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-OssuaryBoneAtlas.ps1') -Atlas $asset
		if ($LASTEXITCODE -ne 0) { throw 'Bone atlas audit failed.' }
		Copy-Item -LiteralPath $asset -Destination (Join-Path $mirrorRoot 'Content/Tiles/OssuaryBone.png')
		Write-Host 'Approved bone overrides the existing tile texture in this QA package only; no world-generation changes.'
	}
	if ($MawFangCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $MawFangCandidateDirectory).Path
		$allowedRoot = (Join-Path $sourceRoot 'Art/Candidates/MawTooth-v1') + [IO.Path]::DirectorySeparatorChar
		if (-not $candidateRoot.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Fang must be in the isolated tooth study.' }
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-MawFang.ps1') -CandidateDirectory $candidateRoot
		if ($LASTEXITCODE -ne 0) { throw 'Fang contract/atlas audit failed.' }
		$asset = Join-Path $candidateRoot 'MawFangTile.png'
		if ((Get-FileHash -LiteralPath $asset).Hash -ne '52DFA889B6CFADED43D9997E870D2EBCA0BE991188E29AF29616CE226DBC3D68') { throw 'Fang candidate changed; review required.' }
		$destination = Join-Path $mirrorRoot 'Content/Tiles/Diagnostics'
		New-Item -ItemType Directory -Path $destination -Force | Out-Null
		Copy-Item -LiteralPath $asset -Destination (Join-Path $destination 'MawFangTile.png')
		Write-Host 'Candidate fang included only in this QA build; no world-generation changes.'
	}
	if ($MawToothArtCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $MawToothArtCandidateDirectory).Path
		$allowed = (Resolve-Path -LiteralPath (Join-Path $sourceRoot 'Art/Candidates/MawToothFamily-v1/Native-v1')).Path
		if ($candidateRoot -ne $allowed) { throw 'Only the exact tooth family art study is authorized.' }
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-MawToothArtFixture.ps1')
		if ($LASTEXITCODE -ne 0) { throw 'Tooth art contract failed.' }
		$destination = Join-Path $mirrorRoot 'Content/Tiles/Diagnostics/MawToothArt'
		New-Item -ItemType Directory -Path $destination -Force | Out-Null
		$recipe = Get-Content -Raw -LiteralPath (Join-Path $candidateRoot 'recipe.json') | ConvertFrom-Json
		foreach ($record in $recipe.records) {
			if ($record.name -notin @('short','long','wide')) { throw 'Unknown tooth art variant.' }
			$folder = Join-Path $candidateRoot $record.name
			if ((Get-FileHash -LiteralPath (Join-Path $folder 'tooth.png')).Hash -ne $record.spriteSHA256) { throw 'Reviewed tooth pixels changed.' }
			Copy-Item -LiteralPath (Join-Path $folder 'upright-art-atlas.png') -Destination (Join-Path $destination "$($record.name).png")
		}
		Write-Host 'Exact tooth family included as non-solid, non-damaging native art specimens only.'
	}
	if ($MawToothClusterCandidateDirectory) {
		$candidateRoot = (Resolve-Path -LiteralPath $MawToothClusterCandidateDirectory).Path
		$legacy = (Resolve-Path -LiteralPath (Join-Path $sourceRoot 'Art/Candidates/MawToothCluster-v1/Native-v3')).Path
		$curves = (Resolve-Path -LiteralPath (Join-Path $sourceRoot 'Art/Candidates/MawToothCluster-v1/Native-v4')).Path
		if ($candidateRoot -ne $legacy -and $candidateRoot -ne $curves) { throw 'Only a contracted cluster candidate is authorized.' }
		$validator = if ($candidateRoot -eq $curves) { 'Test-MawClusterPlayerCurves.ps1' } else { 'Test-MawClusterRootedCandidate.ps1' }
		& pwsh -NoProfile -File (Join-Path $sourceRoot "Tools/$validator") -CandidateDirectory $candidateRoot
		if ($LASTEXITCODE -ne 0) { throw 'Cluster art/mask contract failed.' }
		$destination = Join-Path $mirrorRoot 'Content/Tiles/Diagnostics/MawToothCluster'
		New-Item -ItemType Directory -Path $destination -Force | Out-Null
		foreach ($name in @('cluster.png','cluster-atlas.png','contact-mask.bin')) {
			Copy-Item -LiteralPath (Join-Path $candidateRoot $name) -Destination (Join-Path $destination $name)
		}
		Write-Host 'Placeable thorn-style cluster included in this candidate package only.'
	}
	if ($AssetSnapshotPath) {
		# Check the assembled bytes BEFORE Build can package/install them. A valid
		# source tree alone does not prove the QA candidate overrides were retained.
		& pwsh -NoProfile -File (Join-Path $sourceRoot 'Tools/Test-QAAssetSnapshot.ps1') -BuildRoot $mirrorRoot -SnapshotPath $AssetSnapshotPath
		if ($LASTEXITCODE -ne 0) { throw 'Pinned QA asset continuity failed; package not built/installed.' }
	}
	Push-Location $mirrorRoot
	try {
		$buildOptions = @()
		if ($PackedMawPreview) { $buildOptions += '-p:ApogeanMawPackedQA=true' }
		if ($CompileOnly) {
			# Local tML targets package/install AfterTargets=Build. Compile only,
			# retaining their framework/reference defaults but never calling Build.
			& dotnet build '.\apogean.csproj' -v:minimal -t:Compile @buildOptions
		} else {
			& dotnet build '.\apogean.csproj' -v:minimal @buildOptions
		}
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
