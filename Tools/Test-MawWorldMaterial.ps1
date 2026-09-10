param([string]$CandidateDirectory = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates/MawTerrain-Harsh-v1/Native-v2/Stone'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $CandidateDirectory).Path
$report=Get-Content -Raw -LiteralPath (Join-Path $root 'recipe.json') | ConvertFrom-Json
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
$repo=Split-Path -Parent $PSScriptRoot
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'WorldMaterialCompiler.cs.txt'))
if($report.nativeRendered -or $report.installed){throw 'FALSE_EVIDENCE'}
if((Get-FileHash -LiteralPath (Join-Path $root 'atlas.png')).Hash -ne $report.atlasSHA256){throw 'ATLAS_HASH'}
if((Get-FileHash -LiteralPath $report.referencePath).Hash -ne $report.referenceSHA256){throw 'REFERENCE_HASH'}
if((Get-FileHash -LiteralPath $report.sourcePath).Hash -ne $report.sourceSHA256){throw 'SOURCE_HASH'}
$count=[WorldMaterialCompiler]::Verify($root,$report.referencePath)
Write-Output "PASS $count native-mask/source pixel checks,128px world pattern; native render still required."
