param([Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Join-Path $root 'Art/Candidates/MawTooth-v1/source.png'
if ((Get-FileHash -LiteralPath $source).Hash -ne '3EE52DF9AA906B98B9F5B067B858C6D1B71E1B9292E954B795EDC6097150A954') { throw 'SOURCE_STUDY_CHANGED' }
$output=[IO.Path]::GetFullPath($OutputDirectory)
$allowed=(Join-Path $root 'Art/Candidates/MawTooth-v1')+[IO.Path]::DirectorySeparatorChar
if (-not $output.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)) { throw 'CANDIDATE_PATH_OUTSIDE_TOOTH_STUDY' }
if (Test-Path -LiteralPath $output) { throw 'OUTPUT_EXISTS' }
$null=New-Item -ItemType Directory -Path $output
Add-Type -AssemblyName System.Drawing
$references=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$references+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $references -TypeDefinition ((Get-Content (Join-Path $PSScriptRoot 'MawFangCandidate.cs.txt') -Raw) + (Get-Content (Join-Path $root 'Common/Geometry/MawFangShape.cs') -Raw).Replace('using System;',''))
[MawFangCandidate]::Compile($source,$output)
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath (Join-Path $output 'color-master.png') -MaskPath (Join-Path $output 'frame-mask.png') -OutputPath (Join-Path $output 'MawFangTile.png')
if ($LASTEXITCODE -ne 0) { throw 'MASK_EXPORT_FAILED' }
[MawFangCandidate]::Preview((Join-Path $output 'MawFangTile.png'),(Join-Path $output 'preview.png'))
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawFang.ps1') -CandidateDirectory $output
if ($LASTEXITCODE -ne 0) { throw 'FANG_CANDIDATE_FAILED' }
