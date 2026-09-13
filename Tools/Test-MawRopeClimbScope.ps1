Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$src=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawRopeClimbScope.cs')
$src=$src.Replace('internal static','public static').Replace('internal const','public const')
Add-Type -TypeDefinition $src
$type=[apogean.Content.Diagnostics.MawRopeClimbScope]
$checks=0
function Need($ok,[string]$why){if(-not $ok){throw $why};$script:checks++}
foreach($world in @('Apogee Native Visual V3','aga','', 'Apogee Campus Validation')){
 foreach($player in @('Maw QA Rope','Maw QA Plain','gg','GOLB','')){
  foreach($sp in @($true,$false)){foreach($menu in @($true,$false)){
   Need ($type::Context($world,$player,$sp,$menu) -eq ($world -ceq 'Apogee Native Visual V3' -and $player -ceq 'Maw QA Rope' -and $sp -and -not $menu)) 'Rope context leak'
  }}
 }
}
foreach($r in @('qa-save-and-quit','maw-rope-climb-vanilla','maw-rope-climb-rib','maw-rope-climb-none','maw-rope-climb-stop')){Need ($type::Request($r)) 'Missing supported request'}
foreach($r in @('maw-shallow-build','maw-rope-properties','maw-rope-climb','maw-rope-climb-','maw-rope-climb-fake','qa-perf-start','')){Need (-not $type::Request($r)) 'Unsafe request accepted'}
foreach($tick in -1..211){Need ($type::Up($tick) -eq ($tick -ge 0 -and $tick -lt 180)) 'Up sequence';Need ($type::Complete($tick) -eq ($tick -eq 210)) 'Completion boundary'}
foreach($v in @('vanilla','rib','none')){Need ($type::Variant($v)) 'Variant rejected'}
foreach($v in @('stop','cap','maw','')){Need (-not $type::Variant($v)) 'Unknown variant accepted'}
Write-Output "PASS $checks actual rope-scope and bounded-input policy checks. Not native movement or cleanup evidence."
