Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawAnatomyPlan.cs')
$harness=@'
namespace apogean.Content.Diagnostics {
public static class AnatomyPlanChecks {
 public static string Run() {
  int checks=0;void Require(bool value,string name){if(!value)throw new System.Exception(name);checks++;}
  var p=MawAnatomyPlan.Create();
  Require(p.GetLength(0)==100&&p.GetLength(1)==76,"bounds");
  Require(p[10,12].Key=="cap"&&p[10,13].Key=="grass"&&p[10,14].Key=="soil","cap transition");
  Require(p[23,21].Key=="grass"&&p[45,21].Key=="grass","liked side growth changed");
  Require(p[72,12].Key=="cap"&&p[84,12].Key=="grass","comparison controls");
  int slopes=0,walls=0;
  for(int x=0;x<100;x++)for(int y=0;y<76;y++) {
   var c=p[x,y];
   if(x<4||x>=94||y<12||y>=68)Require(c.Key==null&&c.Wall==null,"reserved boundary");
   if(c.Slope!=0){Require(c.Key=="rib"&&c.Slope<=2,"slope escaped ribs");slopes++;}
   if(c.Wall!=null){Require(x>=72&&c.Key==null,"wall conceals root");walls++;}
  }
  Require(walls==378,"six independent wall swatches");Require(slopes>=12,"tapers absent");
  bool Connected(int x,int y,int expected) {
   var seen=new System.Collections.Generic.HashSet<(int,int)>();var q=new System.Collections.Generic.Queue<(int,int)>();q.Enqueue((x,y));
   while(q.Count>0){var a=q.Dequeue();if(a.Item1<0||a.Item2<0||a.Item1>=100||a.Item2>=76||seen.Contains(a)||p[a.Item1,a.Item2].Key is not ("rib" or "bone"))continue;
    seen.Add(a);q.Enqueue((a.Item1+1,a.Item2));q.Enqueue((a.Item1-1,a.Item2));q.Enqueue((a.Item1,a.Item2+1));q.Enqueue((a.Item1,a.Item2-1));}
   return seen.Count==expected;
  }
  Require(Connected(20,27,44),"first continuous rib");Require(Connected(49,44,30),"second continuous rib");Require(Connected(20,60,15),"third continuous rib");
  // Disconnect a shaft: same oracle must reject the deliberately broken plan.
  for(int y=0;y<76;y++)if(p[27,y].Key=="rib")p[27,y]=new(null);
  Require(!Connected(20,27,44),"disconnected-rib negative not rejected");
  return $"PASS {checks} anatomy layout checks and broken-shaft negative; no native collision/visual claim.";
 }
}}
'@
Add-Type -TypeDefinition ($source+"`n"+$harness)
[apogean.Content.Diagnostics.AnatomyPlanChecks]::Run()
