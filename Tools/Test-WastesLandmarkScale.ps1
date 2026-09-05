param(
    [string]$Capture = 'Art/Validation/WastesLandscapeV1/Live/Horizon-ground-2560x1440.png',
    [int]$X = 1855, [int]$Y = 515, [int]$Width = 128, [int]$Height = 34
)
# Narrow image regression: daylight orange truck upper-body span, not a UI scale guess.
# Coordinates select the visibly unobscured truck in a recorded live fixture.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
public static class TruckSpan {
 public static int Measure(string path, int x, int y, int w, int h) {
  using(var image=new Bitmap(path)) {
   if(x<0 || y<0 || w<=0 || h<=0 || x>image.Width-w || y>image.Height-h)
    throw new ArgumentOutOfRangeException("ROI", "Fixture region must fit inside the capture.");
   int left=int.MaxValue,right=-1;
   for(int a=x;a<x+w;a++)for(int b=y;b<y+h;b++) {
    var p=image.GetPixel(a,b);
    if(p.A>0 && p.R>70 && p.R>p.G*1.20 && p.R>p.B*1.25) {left=Math.Min(left,a);right=Math.Max(right,a);}
   }
   if(right<left)throw new Exception("No truck pigment in selected fixture region");
   return right-left+1;
  }
 }
}
'@
$root=Split-Path -Parent $PSScriptRoot
$source=[TruckSpan]::Measure((Join-Path $root 'Content/Backgrounds/Candidates/WastesV1/Mid.png'),460,193,125,35)
$actual=[TruckSpan]::Measure((Join-Path $root $Capture),$X,$Y,$Width,$Height)
$scale=$actual/[double]$source
Write-Host "Truck body: source=$source px; live=$actual px; measured scale=$([math]::Round($scale,3))"
if ([math]::Abs($actual-$source) -gt 3) { throw 'FAIL: the live landmark is enlarged despite Draw(scale:1).' }
Write-Host 'PASS: daylight truck pigment span is source-sized within 3 pixels. This is not a full renderer-quality test.'
