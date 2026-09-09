param([Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Join-Path $root 'Art/Candidates/MawTooth-v2/source.png'
if ((Get-FileHash -LiteralPath $source).Hash -ne 'C29FC9C9CD74EA976CADFD866E5513B98C618D30F6F27810A9B8E4A321FE5B7A') { throw 'SOURCE_CHANGED' }
$output=[IO.Path]::GetFullPath($OutputDirectory)
$allowed=(Join-Path $root 'Art/Candidates/MawTooth-v2')+[IO.Path]::DirectorySeparatorChar
if (!$output.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase) -or (Test-Path -LiteralPath $output)) { throw 'NEW_CANDIDATE_PATH_REQUIRED' }
$null=New-Item -ItemType Directory -Path $output
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content (Join-Path $PSScriptRoot 'MawSlimToothStudy.cs.txt') -Raw)
$fitting=[MawSlimToothStudy]::Prepare($source,$output)
Write-Host $fitting
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath (Join-Path $output 'color-master.png') -MaskPath (Join-Path $output 'mask.png') -OutputPath (Join-Path $output 'tooth.png')
if ($LASTEXITCODE -ne 0) { throw 'MASK_EXPORT_FAILED' }
$bone=Join-Path $root 'Art/Candidates/MawBone-v1/MaskedNative-v2/OssuaryBone-candidate.png'
$dirt=Join-Path $root 'Content/Tiles/MawDirt.png'
[MawSlimToothStudy]::Assemble($output,$bone,$dirt)
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawSlimTooth.ps1') -CandidateDirectory $output
if ($LASTEXITCODE -ne 0) { throw 'SLIM_TOOTH_STATIC_FAILED' }
# Generated recipe is a reproducibility artifact, not runtime or visual approval.
@{ sourceSHA256=(Get-FileHash $source).Hash; fitting=$fitting; boneSHA256=(Get-FileHash $bone).Hash; dirtSHA256=(Get-FileHash $dirt).Hash; generatorSHA256=(Get-FileHash (Join-Path $PSScriptRoot 'MawSlimToothStudy.cs.txt')).Hash; spriteSHA256=(Get-FileHash (Join-Path $output 'tooth.png')).Hash; rootOcclusionPixels=4; artOnly=$true } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'recipe.json') -Encoding utf8
