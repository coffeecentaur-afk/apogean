param([Parameter(Mandatory)][string]$OriginalDirectory)
# Verify publication crops against their untouched local native captures.
# This is not an asset generator or a screenshot-color comparison to source art.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll'|Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
public static class NativeCropAudit {
 public static void Check(string original,string crop,int x,int y,int w,int h) {
  using(var a=new Bitmap(original))using(var b=new Bitmap(crop)) {
   if(a.Width!=2560||a.Height!=1401||b.Width!=w||b.Height!=h)throw new Exception("DIMENSIONS");
   for(int j=0;j<h;j++)for(int i=0;i<w;i++)
    if(a.GetPixel(i+x,j+y).ToArgb()!=b.GetPixel(i,j).ToArgb())throw new Exception("NOT_EXACT_CROP");
  }
 }
}
'@
$entries=@(
 @('ground','ground',160,390,2020,540),
 @('below-ground','below-ground',160,110,2020,1120),
 @('space-fade','space-fade',160,110,2020,1120),
 @('space-edge-active','space-edge',160,110,2020,1120),
 @('sky','sky',160,45,2020,1185),
 @('phase-left-checkpoint','checkpoint-sequence',160,390,2020,540),
 @('phase-left-later','station-sequence',160,390,2020,540),
 @('night','night',160,390,2020,540),
 @('eclipse','eclipse',160,390,2020,540),
 @('rain','rain',160,390,2020,540)
)
foreach($e in $entries) {
 $original=Join-Path $OriginalDirectory ($e[0]+'.jpg')
 $crop=Join-Path $PSScriptRoot ($e[1]+'.png')
 [NativeCropAudit]::Check($original,$crop,$e[2],$e[3],$e[4],$e[5])
 Write-Output "PASS native decoded pixel crop: $($e[1]); sourceSHA256=$((Get-FileHash -LiteralPath $original).Hash)"
}
