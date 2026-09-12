param([string]$CandidateDirectory=(Join-Path (Split-Path $PSScriptRoot) 'Art/Candidates/MawHangingFiber-v1/Native-v1'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;using System.Drawing;using System.Collections.Generic;using System.IO;
public static class HangingFiberChecks {
 static int Check(Bitmap art,Bitmap sheet){
  if(art.Width!=16||art.Height!=96||sheet.Width!=18||sheet.Height!=108)throw new Exception("DIMENSIONS");
  int count=0;var colors=new HashSet<int>();var cells=new HashSet<(int,int)>();
  for(int y=0;y<96;y++){int row=0;for(int x=0;x<16;x++){
   Color c=art.GetPixel(x,y);if(c.A!=0&&c.A!=255)throw new Exception("SOFT_ALPHA");
   if(c.A==0){if(c.ToArgb()!=0)throw new Exception("TRANSPARENT_RGB");}else{
    if(c.R>180&&c.B>150&&c.G<100)throw new Exception("MAGENTA_LEAK");row++;count++;colors.Add(c.ToArgb());cells.Add((x,y));}
  }if(row==0)throw new Exception("STRAND_GAP");}
  if(colors.Count>8)throw new Exception("PALETTE");
  for(int y=0;y<108;y++)for(int x=0;x<18;x++){
   int expected=x<16&&y%18<16?art.GetPixel(x,y/18*16+y%18).ToArgb():0;
   if(sheet.GetPixel(x,y).ToArgb()!=expected)throw new Exception("FRAME_OR_GUARD");
  }
  var seen=new HashSet<(int,int)>();var q=new Queue<(int,int)>();foreach(var c in cells){q.Enqueue(c);break;}
  while(q.Count>0){var a=q.Dequeue();if(!cells.Contains(a)||!seen.Add(a))continue;
   for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++)if(dx!=0||dy!=0)q.Enqueue((a.Item1+dx,a.Item2+dy));}
  if(seen.Count!=count)throw new Exception("DETACHED_FIBER");return count;
 }
 public static string Run(string folder){using var art=new Bitmap(Path.Combine(folder,"fiber.png"));using var sheet=new Bitmap(Path.Combine(folder,"atlas.png"));int count=Check(art,sheet),negatives=0;
  using(var broken=new Bitmap(art)){for(int x=0;x<16;x++)broken.SetPixel(x,40,Color.FromArgb(0,0,0,0));try{Check(broken,sheet);}catch(Exception e)when(e.Message=="STRAND_GAP"){negatives++;}}
  using(var broken=new Bitmap(sheet)){broken.SetPixel(16,0,Color.White);try{Check(art,broken);}catch(Exception e)when(e.Message=="FRAME_OR_GUARD"){negatives++;}}
  if(negatives!=2)throw new Exception("NEGATIVE_SURVIVED");return $"PASS 16x96 strand; 18x108 ordered sheet; {count} connected opaque pixels; hard alpha, no matte color; two deliberate broken-input controls. Native draw still required.";
 }
}
'@
[HangingFiberChecks]::Run((Resolve-Path -LiteralPath $CandidateDirectory).Path)
