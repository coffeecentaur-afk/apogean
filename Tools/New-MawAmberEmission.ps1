param([Parameter(Mandatory)][string]$TileAtlas,[Parameter(Mandatory)][string]$WallAtlas,[Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;using System.IO;using System.Drawing;using System.Collections.Generic;
public static class AmberEmissionCompiler {
 public static string Compile(string source,string target,bool wall){
  // Explicit palette roles from the approved amber field, not image recoloring.
  int[,] palette={{146,82,26},{171,101,30},{189,117,35},{204,132,43},{216,146,52},{228,163,66},{233,173,81},{242,188,98},{246,204,124}};
  var colors=new HashSet<int>();for(int n=0;n<palette.GetLength(0);n++)colors.Add(Color.FromArgb(
   palette[n,0]*(wall?62:100)/100,palette[n,1]*(wall?62:100)/100,palette[n,2]*(wall?62:100)/100).ToArgb());
  using var image=new Bitmap(source);int pitch=wall?34:18,offset=wall?8:0;
  if(image.Width%pitch!=0||image.Height%pitch!=0)throw new Exception("EMISSION_ATLAS_SIZE");
  using var file=new FileStream(target,FileMode.CreateNew);using var output=new BinaryWriter(file);
  output.Write(0x4D454D41);output.Write(image.Width);output.Write(image.Height);output.Write(pitch);output.Write(image.Width/pitch*(image.Height/pitch));
  int lit=0,quiet=0;for(int y=0;y<image.Height;y+=pitch)for(int x=0;x<image.Width;x+=pitch){
   int count=0;for(int dy=0;dy<16;dy++)for(int dx=0;dx<16;dx++)if(colors.Contains(image.GetPixel(x+offset+dx,y+offset+dy).ToArgb()))count++;
   output.Write((ushort)count);if(count>0)lit++;else quiet++;
  }
  if(lit==0||quiet==0)throw new Exception("EMISSION_MISSING_CONTROL");
  return $"Amber {(wall?"wall":"tile")}: {lit} emitting and {quiet} quiet packed cells. Native alpha + explicit palette; cell-level light, not per-pixel rays.";
 }
}
'@
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
[AmberEmissionCompiler]::Compile((Resolve-Path -LiteralPath $TileAtlas).Path,(Join-Path $OutputDirectory 'amber-tile-light.bin'),$false)
[AmberEmissionCompiler]::Compile((Resolve-Path -LiteralPath $WallAtlas).Path,(Join-Path $OutputDirectory 'amber-wall-light.bin'),$true)
