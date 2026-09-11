Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawFiberGrowthPolicy.cs')
$harness=@'
namespace apogean.Content.Diagnostics {
public static class FiberPolicyChecks {
 public static string Run() {
  int checks=0;
  void Require(bool condition,string name) { if(!condition)throw new System.Exception(name); checks++; }
  var cells=new MawFiberGrowthPolicy.Cell[16,10];
  for(int x=2;x<=6;x++)for(int y=2;y<=6;y++)cells[x,y]=new(MawFiberGrowthPolicy.Host.Soil);
  cells[4,2]=new(MawFiberGrowthPolicy.Host.Grass);
  cells[8,2]=new(MawFiberGrowthPolicy.Host.Soil); // island separated by air
  cells[0,2]=new(MawFiberGrowthPolicy.Host.Soil); // owned-edge rejection
  cells[1,2]=new(MawFiberGrowthPolicy.Host.Grass);
  var before=cells[3,2];
  var first=MawFiberGrowthPolicy.Plan(cells,32,true);
  Require(cells[3,2]==before,"planning mutated substrate");
  Require(!first.Contains(new(4,3)),"buried center was exposed");
  Require(!first.Contains(new(0,2)),"ownership boundary escaped");
  Require(MawFiberGrowthPolicy.Plan(cells,1,true).Count==1,"budget not enforced");
  Require(MawFiberGrowthPolicy.Plan(cells,32,false).Count==0,"disabled growth");
  Require(MawFiberGrowthPolicy.Plan(cells,0,true).Count==0,"zero budget");
  for(int step=0;step<30;step++)foreach(var site in MawFiberGrowthPolicy.Plan(cells,3,true))cells[site.X,site.Y]=new(MawFiberGrowthPolicy.Host.Grass);
  int coated=0;
  for(int x=2;x<=6;x++)for(int y=2;y<=6;y++) {
   bool edge=x==2||x==6||y==2||y==6;
   Require((cells[x,y].Material==MawFiberGrowthPolicy.Host.Grass)==edge,"four-sided perimeter or buried core");
   if(edge)coated++;
  }
  Require(cells[8,2].Material==MawFiberGrowthPolicy.Host.Soil,"jumped air trench");
  var tiny=new MawFiberGrowthPolicy.Cell[5,5];
  tiny[2,2]=new(MawFiberGrowthPolicy.Host.Soil,true); tiny[1,2]=new(MawFiberGrowthPolicy.Host.Grass);
  Require(MawFiberGrowthPolicy.Plan(tiny,32,true).Count==0,"protected target");
  tiny[2,2]=new(MawFiberGrowthPolicy.Host.Soil);tiny[1,2]=new(MawFiberGrowthPolicy.Host.Grass,true);
  Require(MawFiberGrowthPolicy.Plan(tiny,32,true).Count==0,"blocked source");
  tiny[1,2]=new(MawFiberGrowthPolicy.Host.Air);tiny[1,1]=new(MawFiberGrowthPolicy.Host.Grass);
  Require(MawFiberGrowthPolicy.Plan(tiny,32,true).Count==0,"diagonal seed jumped air");
  tiny[1,2]=new(MawFiberGrowthPolicy.Host.Grass);
  tiny[2,2]=new(MawFiberGrowthPolicy.Host.Other);
  Require(MawFiberGrowthPolicy.Plan(tiny,32,true).Count==0,"stone/bone identity lost");
  tiny[2,2]=new(MawFiberGrowthPolicy.Host.Soil,false,true);
  tiny[2,1]=tiny[2,3]=tiny[3,2]=new(MawFiberGrowthPolicy.Host.Other);
  Require(MawFiberGrowthPolicy.Plan(tiny,32,true).Count==1,"exposed slope lost");
  bool rejected=false;try{MawFiberGrowthPolicy.Plan(tiny,33,true);}catch(System.ArgumentOutOfRangeException){rejected=true;}
  Require(rejected,"unbounded update accepted");
  return $"PASS {checks} pure-policy checks; {coated} perimeter cells, four faces, buried soil preserved. Not native framing, vines, ecology rate, multiplayer or worldgen approval.";
 }
}}
'@
Add-Type -TypeDefinition ($source+"`n"+$harness)
[apogean.Content.Diagnostics.FiberPolicyChecks]::Run()
