Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot '../Content/Diagnostics'
$source=(Get-Content -Raw (Join-Path $root 'MawFiberGrowthPolicy.cs'))+"`n"+(Get-Content -Raw (Join-Path $root 'MawGrowthLoadPlan.cs'))
# Concatenated files retain using directives before namespaces in separate trees.
$source=$source.Replace('using P = apogean.Content.Diagnostics.MawFiberGrowthPolicy;','').Replace('P.','MawFiberGrowthPolicy.')
$source=$source.Replace("`nusing System;",'')
$test=@'
namespace apogean.Content.Diagnostics {
 public static class GrowthChecks {
  public static int Run(){int checks=0;void Need(bool ok){if(!ok)throw new System.Exception("GROWTH_PLAN");checks++;}
   foreach(int budget in new[]{0,1,8,32}){
    var cells=MawGrowthLoadPlan.Seed();var seed=(MawFiberGrowthPolicy.Cell[,])cells.Clone();int converted=0;
    for(int tick=0;tick<240;tick++){
     var before=(MawFiberGrowthPolicy.Cell[,])cells.Clone();var sites=MawFiberGrowthPolicy.Plan(cells,budget,budget!=0);
     Need(sites.Count<=budget);foreach(var p in sites){Need(MawGrowthLoadPlan.IsMatureGrass(p.X,p.Y));Need(before[p.X,p.Y].Material==MawFiberGrowthPolicy.Host.Soil);}
     for(int x=0;x<32;x++)for(int y=0;y<32;y++)Need(before[x,y]==cells[x,y]);
     foreach(var p in sites)cells[p.X,p.Y]=new(MawFiberGrowthPolicy.Host.Grass);converted+=sites.Count;
    }
    Need(converted==(budget==0?0:156));int grass=0;
    for(int x=0;x<32;x++)for(int y=0;y<32;y++){
     bool expected=budget!=0&&MawGrowthLoadPlan.IsMatureGrass(x,y);
     Need(cells[x,y]==(expected?new(MawFiberGrowthPolicy.Host.Grass):seed[x,y]));
     if(cells[x,y].Material==MawFiberGrowthPolicy.Host.Grass)grass++;
    }Need(grass==(budget==0?4:160));
   }
   foreach(int b in new[]{-1,2,33,1000})Need(!MawGrowthLoadPlan.Budget(b));
   foreach(string w in new[]{"Apogee Native Visual V3","aga",null})foreach(string p in new[]{"gg","Maw QA Plain","Maw QA Rope",null})foreach(bool single in new[]{false,true})foreach(bool menu in new[]{false,true})
    Need(MawGrowthLoadPlan.Context(w,p,single,menu)==(w=="Apogee Native Visual V3"&&p=="gg"&&single&&!menu));
   return checks;
  }
 }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$test)
Write-Output "PASS $([apogean.Content.Diagnostics.GrowthChecks]::Run()) fixed-grid/budget/context comparisons (mostly unchanged-cell checks). No native growth, performance or visual proof."
