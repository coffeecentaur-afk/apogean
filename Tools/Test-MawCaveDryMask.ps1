Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawCaveBackdropProbe.cs')
$build=[regex]::Match($source,'(?s)Array.Clear\(wetPrefix\);(?<body>.*?)\s*dryCells=excludedCells=runs=0;')
$query=[regex]::Match($source,'(?s)private bool Dry\(int x,int y\)\s*\{(?<body>.*?)\n        \}')
if(-not $build.Success -or -not $query.Success){throw 'Runtime dry-mask method boundaries changed; review the extraction.'}
# Compile the actual runtime prefix/query arithmetic with a minimal tile grid.
# This tests the mask, not Terraria draw ordering or a pixel-exact liquid footprint.
$template=@'
using System;
public class CaveMaskArithmetic {
 const int Width=96,Height=64,Margin=2,Pitch=101;
 struct Position {public int X,Y;}
 struct Cell {public byte LiquidAmount;}
 static class Main {public static Cell[,] tile=new Cell[116,84];}
 Position region=new Position{X=8,Y=8};
 int[] wetPrefix=new int[101*69];
 bool[,] wet=new bool[100,68];
 void Build(){Array.Clear(wetPrefix); BUILD }
 bool Dry(int x,int y){ QUERY }
 public static int Test() {
  var probe=new CaveMaskArithmetic();var rng=new Random(81224);int assertions=0;
  for(int c=0;c<70;c++) {
   for(int x=0;x<100;x++)for(int y=0;y<68;y++) {
    bool v=c==1 || c>=6 && rng.Next(45)==0;
    if(c==2)v=x==0&&y==0;
    if(c==3)v=x==99&&y==67;
    if(c==4)v=x==50&&y==0;
    if(c==5)v=x==99&&y==34;
    probe.wet[x,y]=v;Main.tile[x+6,y+6].LiquidAmount=(byte)(v?1:0);
   }
   probe.Build();
   for(int x=0;x<96;x++)for(int y=0;y<64;y++) {
    bool expected=true;
    for(int xx=x;xx<=x+4;xx++)for(int yy=y;yy<=y+4;yy++)expected &= !probe.wet[xx,yy];
    if(probe.Dry(x,y)!=expected)throw new Exception("DRY_MASK_MISMATCH "+c+"/"+x+"/"+y);
    assertions++;
   }
  }
  return assertions;
 }
}
'@
$code=$template.Replace('BUILD',$build.Groups['body'].Value).Replace('QUERY',$query.Groups['body'].Value)
Add-Type -TypeDefinition $code
$count=[CaveMaskArithmetic]::Test()
# Deliberately omit the furthest column. Independent brute-force oracle must catch it.
$bad=$code.Replace('CaveMaskArithmetic','CaveMaskBadArithmetic').Replace('x1=x+Margin*2+1','x1=x+Margin*2')
if($bad -eq $code){throw 'Negative control did not change source.'}
Add-Type -TypeDefinition $bad
$rejected=$false
try{[CaveMaskBadArithmetic]::Test()|Out-Null}catch{if($_.Exception.ToString() -match 'DRY_MASK_MISMATCH'){$rejected=$true}else{throw}}
if(-not $rejected){throw 'Oracle accepted missing liquid-edge column.'}
Write-Output "PASS: $count comparisons of actual dry-mask arithmetic against independent 5x5 occupancy; all-dry, all-wet, exterior edges, seeded sparse layouts; missing-column mutation rejected. Not a native renderer/liquid-edge visual pass."
