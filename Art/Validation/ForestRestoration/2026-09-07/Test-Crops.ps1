param([Parameter(Mandatory)][string]$OriginalDirectory)
# Publication audit only: no texture editing, rescaling or reconstruction.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll'|Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
public static class RoutingCropAudit {
 public static void Check(string original,string crop) {
  using(var a=new Bitmap(original))using(var b=new Bitmap(crop)) {
   if(a.Width!=2560||a.Height!=1401||b.Width!=2020||b.Height!=700)throw new Exception("DIMENSIONS");
   for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++)
    if(a.GetPixel(x+160,y+400).ToArgb()!=b.GetPixel(x,y).ToArgb())throw new Exception("NOT_EXACT_CROP");
  }
 }
}
'@
$entries=@(@('01-wastes','native-wastes'),@('08-spray-green','native-spray-green'),@('09-jungle','native-jungle'),@('10-wastes-return','native-wastes-return'))
foreach($e in $entries){
 $source=Join-Path $OriginalDirectory ($e[0]+'.jpg')
 [RoutingCropAudit]::Check($source,(Join-Path $PSScriptRoot ($e[1]+'.png')))
 Write-Output "PASS exact native crop: $($e[1]); sourceSHA256=$((Get-FileHash -LiteralPath $source).Hash)"
}
