param([ValidateSet('None','IgnoreGrove','IgnoreIsolation')][string]$Mutation='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
Add-Type -Path (Join-Path $root 'Common/Backgrounds/BackgroundFixturePlacement.cs')
$checks=0
foreach($width in @(4200,6400,8400)) {
 foreach($left in @(20,400,[int]($width/2),($width-190))) {
  $right=$left+170
  foreach($preferred in @(0,220,$left,($left+85),($right+190),($width-1))) {
   $x=[apogean.Common.Backgrounds.BackgroundFixturePlacement]::Center($preferred,$width,$left,$right)
   if($Mutation -eq 'IgnoreGrove'){$x=[Math]::Clamp($preferred,211,$width-211)}
   if($Mutation -eq 'IgnoreIsolation'){$x=$right+96}
   if($x-191 -lt 20 -or $x+191 -gt $width-20){throw 'WORLD_MARGIN'}
   if($x+199 -gt $left -and $x-199 -lt $right){throw 'GROVE_OVERLAP'}
   if($x -ne [apogean.Common.Backgrounds.BackgroundFixturePlacement]::Center($x,$width,$left,$right)){throw 'UNSTABLE_SITE'}
   $checks++
  }
 }
}
if([apogean.Common.Backgrounds.BackgroundFixturePlacement]::Center(1000,4200,0,0) -ne 1000){throw 'NO_GROVE_MOVED'}
$rejected=$false
try{[void][apogean.Common.Backgrounds.BackgroundFixturePlacement]::Center(1000,4200,20,4180)}catch{$rejected=$true}
if(-not $rejected){throw 'IMPOSSIBLE_SITE_ACCEPTED'}
Write-Output "PASS: $checks bounded placements, empty protection and impossible-site rejection. Not native terrain proof."
