param([string]$ProjectRoot=(Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$production=Get-Content -LiteralPath (Join-Path $ProjectRoot 'Common/WorldGeneration/ArrivalSitePlanner.cs') -Raw
$harness=@'
namespace apogean.Common.WorldGeneration {
public static class ArrivalPlannerChecks {
 public static int Run() {
  int checks=0;
  Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception(name);checks++;};
  Func<int,int,ArrivalCell> flat=(x,y)=>y>=200?ArrivalCell.Soil:ArrivalCell.Empty;
  string reason;
  var site=ArrivalSitePlanner.At(500,200,false,8400,2400,flat,out reason);
  check(site!=null&&!site.Settled,"FLAT_DIVOT");
  int[] expected={200,200,201,201,202,202,202,202,202,201,201,200,200};
  for(int i=0;i<13;i++)check(site.Floor[i]==expected[i],"AUTHORED_SHAPE");
  for(int i=4;i<=8;i++)check(site.Floor[i]==site.PodTop+6,"FIVE_FLAT_ANCHORS");
  for(int i=1;i<13;i++)check(Math.Abs(site.Floor[i]-site.Floor[i-1])<=1,"WALK_JUMP_ROUTE");
  check(ArrivalSitePlanner.CoverFits(site,new ArrivalBounds(499,197,2,3)),"WHOLE_COVER_REJECTED");
  check(!ArrivalSitePlanner.CoverFits(site,new ArrivalBounds(site.Left-1,198,2,2)),"PARTIAL_COVER_CUT");
  check(ArrivalSitePlanner.CoverFits(site,new ArrivalBounds(site.Left-3,198,2,2)),"UNTOUCHED_COVER_REJECTED");
  // Every cell in the padded edit/framing envelope can independently veto the site.
  for(int x=site.Envelope.X;x<site.Envelope.Right;x++)for(int y=site.Envelope.Y;y<site.Envelope.Bottom;y++) {
   int bx=x,by=y;
   Func<int,int,ArrivalCell> blocked=(cx,cy)=>cx==bx&&cy==by?ArrivalCell.Blocked:flat(cx,cy);
   check(!ArrivalSitePlanner.Recheck(site,blocked,out reason),"IGNORED_OBSTACLE");
  }
  for(int x=site.Left;x<site.Left+13;x++)for(int y=200;y<=205;y++) {
   int bx=x,by=y;
   Func<int,int,ArrivalCell> cavity=(cx,cy)=>cx==bx&&cy==by?ArrivalCell.Empty:flat(cx,cy);
   if(y<=site.Floor[x-site.Left]+3)check(!ArrivalSitePlanner.Recheck(site,cavity,out reason),"CAVITY_OPENED");
  }
  Func<int,int,ArrivalCell> step=(x,y)=>y>=200+(x>500?1:0)?ArrivalCell.Soil:ArrivalCell.Empty;
  check(ArrivalSitePlanner.At(500,200,false,8400,2400,step,out reason)==null,"EXCESS_RELIEF_ACCEPTED");
  // A natural one-step bowl can safely deepen; sloped cells are classified as natural soil by the engine.
  Func<int,int,ArrivalCell> bowl=(x,y)=>y>=200+(Math.Abs(x-500)<=3?1:0)?ArrivalCell.Soil:ArrivalCell.Empty;
  check(ArrivalSitePlanner.At(500,200,false,8400,2400,bowl,out reason)!=null,"SAFE_BOWL_REJECTED");
  Func<int,int,ArrivalCell> grove=(x,y)=>y<200&&y>=180&&x%12==0?ArrivalCell.Blocked:flat(x,y);
  check(ArrivalSitePlanner.Find(500,200,0,8400,2400,grove,b=>true,null)!=null,"NO_TREE_PRESERVING_COMPACT_SITE");
  for(int seed=0;seed<64;seed++) {
   int attempts=0;
   var a=ArrivalSitePlanner.Find(500,200,seed,8400,2400,flat,b=>true,s=>attempts++);
   var b=ArrivalSitePlanner.Find(500,200,seed,8400,2400,flat,r=>true,null);
   check(a!=null&&a.CenterX==b.CenterX&&a.PodTop==b.PodTop,"NONDETERMINISTIC_SITE");
   check(!a.Envelope.Intersects(new ArrivalBounds(497,195,7,10)),"SPAWN_ENVELOPE_CHANGED");
   check(Math.Abs(a.CenterX-500)<=38&&attempts<=48,"UNBOUNDED_SEARCH");
  }
  int rejected=0;
  check(ArrivalSitePlanner.Find(500,200,0,8400,2400,flat,b=>false,s=>rejected++)==null&&rejected==48,"RESERVATION_BYPASS_OR_RETRY_LOOP");
  // Shallow soil over a cavity: zero-dig settlement survives, 2-deep cut does not.
  Func<int,int,ArrivalCell> thin=(x,y)=>y>=200&&y<=203?ArrivalCell.Soil:ArrivalCell.Empty;
  var fallback=ArrivalSitePlanner.Find(500,200,0,8400,2400,thin,b=>true,null);
  check(fallback!=null&&fallback.Settled,"NO_SAFE_UNDUG_FALLBACK");
  for(int i=0;i<fallback.Width;i++)check(fallback.Floor[i]==fallback.Surface[i],"FALLBACK_DIGS");
  check(ArrivalSitePlanner.Find(500,200,0,8400,2400,(x,y)=>ArrivalCell.Blocked,b=>true,null)==null,"NO_SLOT_FORCED");
  check(ArrivalSitePlanner.At(15,200,false,8400,2400,flat,out reason)==null,"WORLD_EDGE");
  return checks;
 }
}
}
'@
Add-Type -TypeDefinition ($production + $harness)
Write-Host "PASS: $([apogean.Common.WorldGeneration.ArrivalPlannerChecks]::Run()) real-planner shape, full-envelope obstacle, cavity, seed, spawn and fallback checks. Not native generation proof."
