param([string]$PreviewPath='',[switch]$PreviewOnly)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawSketchPlan.cs')
if($PreviewOnly){$source=$source.Replace('Build(); Validate();','Build();');Write-Output 'UNVALIDATED diagnostic layout, not a passing test.'}
$test=@'
namespace apogean.Content.Diagnostics {
 public static class SketchChecks {
  public static string Diagnose(){var p=MawSketchPlan.Create();string result=$"teeth={p.Teeth.Count}, fibers={p.Fibers.Count}; ";var reach=p.Reachable();foreach(var c in p.Chests)result+=$"chest {c}: {reach[c.X,c.Y-2]}; ";p.Teeth.Clear();reach=p.Reachable();foreach(var c in p.Chests)result+=$"without teeth {c}: {reach[c.X,c.Y-2]}; ";return result;}
  public static string Run(){
   var p=MawSketchPlan.Create();var q=MawSketchPlan.Create();int checks=0;
   for(int x=0;x<MawSketchPlan.Width;x++)for(int y=0;y<MawSketchPlan.Height;y++){
    if(p.Cells[x,y]!=q.Cells[x,y])throw new System.Exception("Nondeterministic plan");checks++;
    if((x<4||y<4||x>=MawSketchPlan.Width-4||y>=MawSketchPlan.Height-4)&&p.Cells[x,y]!=default)
     throw new System.Exception("Outside padded footprint");
   }
   // Run the real validator against three intentional layout defects.
   var saved=p.Cells[59,28];p.Cells[59,28]=default;Reject(p);p.Cells[59,28]=saved;
   saved=p.Cells[10,70];p.Cells[10,70]=new("bone");Reject(p);p.Cells[10,70]=saved;
   var tooth=p.Teeth.ToArray();p.Teeth.Clear();Reject(p);p.Teeth.AddRange(tooth);
   p.Validate();return $"PASS {checks} deterministic cells, 3 rejected defects; ribs={p.Ribs.Length}, teeth={p.Teeth.Count}, fibers={p.Fibers.Count}, chests={p.Chests.Length}. Spatial connectivity only, not playtest proof.";
  }
  static void Reject(MawSketchPlan p){bool rejected=false;try{p.Validate();}catch(System.InvalidOperationException){rejected=true;}if(!rejected)throw new System.Exception("Defect survived");}
  public static string Svg(){
   var p=MawSketchPlan.Create();var s=new System.Text.StringBuilder("<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 136' width='1600' height='1088'><rect width='200' height='136' fill='#797d82'/>");
   for(int y=0;y<MawSketchPlan.Height;y++)for(int x=0;x<MawSketchPlan.Width;x++){
    var c=p.Cells[x,y];string fill=c.Key switch{"soil"=>"#715244","grass"=>"#a67735","cap"=>"#d1a14e","stone"=>"#423b39","rib"=>"#e2d8b8","amber"=>"#eea517",_=>c.Wall==null?null:"#1c1a1b"};
    if(fill!=null)s.Append($"<rect x='{x}' y='{y}' width='1' height='1' fill='{fill}'/>");
   }
   foreach(var t in p.Teeth)s.Append($"<rect x='{t.Root.X}' y='{t.Root.Y}' width='4' height='4' fill='#e9d617' opacity='.65'/>");
   foreach(var f in p.Fibers)s.Append($"<path d='M{f.Root.X+.5},{f.Root.Y+1} v{f.Length}' stroke='#d49333' stroke-width='.5'/>");
   foreach(var c in p.Chests)s.Append($"<rect x='{c.X}' y='{c.Y}' width='2' height='2' fill='#d92d27'/>");
   s.Append($"<rect x='{p.Entrance.X}' y='{p.Entrance.Y}' width='1.25' height='2.625' fill='#00dddd'/></svg>");return s.ToString();
  }
 }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$test)
if(-not $PreviewOnly){[apogean.Content.Diagnostics.SketchChecks]::Run()}
else{[apogean.Content.Diagnostics.SketchChecks]::Diagnose()}
if($PreviewPath){[IO.File]::WriteAllText([IO.Path]::GetFullPath($PreviewPath),[apogean.Content.Diagnostics.SketchChecks]::Svg())}
