param(
    [string]$SourcePath = (Join-Path $PSScriptRoot '../Content/Diagnostics/MawShallowTraversalPlan.cs'),
    [ValidateSet('none','blocked-connector','one-high','one-wide','missing-rib','missing-root','unsupported-cluster','bad-variant','extra-opening','bad-boundary','bad-size')]
    [string]$Defect = 'none'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# In-memory compilation of this one pure source and its independent oracle only.
# No tML references, project build, files written, native requests or image work.
$source = Get-Content -Raw -LiteralPath $SourcePath
$harness = @'
namespace apogean.Content.Diagnostics {
    using System;
    using System.Collections.Generic;
    using P = MawShallowTraversalPlan;

    public static class ShallowTraversalChecks {
        private static int checks;
        private static void Check(bool value, string name) {
            if (!value) throw new Exception("SHALLOW_TEST: " + name);
            checks++;
        }

        // Independent oracle: pixel AABBs at the actual contracted 20x42 size,
        // including axis-aligned sweeps between positions. No production Fits/BFS calls.
        private static bool[,] Reach(P.Cell[,] cells, P.Cluster[] teeth, int sx, int sy, bool body) {
            int width = cells.GetLength(0), height = cells.GetLength(1);
            int bodyW = body ? 20 : 1, bodyH = body ? 42 : 1;
            int insetX = body ? 6 : 8, insetY = body ? 3 : 8;
            bool Clear(int left, int top, int w, int h) {
                if (left < 0 || top < 0 || left + w > width * 16 || top + h > height * 16) return false;
                for (int x = left / 16; x <= (left + w - 1) / 16; x++)
                    for (int y = top / 16; y <= (top + h - 1) / 16; y++)
                        if (cells[x,y].Key != null || cells[x,y].Region == P.Zone.Outside) return false;
                // All pixels of each full 64x64 footprint are conservatively forbidden.
                // Native draw/contact insets are into the already-solid support, not extra free air.
                foreach (var tooth in teeth)
                    if (left < tooth.Root.X * 16 + 64 && left + w > tooth.Root.X * 16 &&
                        top < tooth.Root.Y * 16 + 64 && top + h > tooth.Root.Y * 16) return false;
                return true;
            }
            var seen = new bool[width,height]; var queue = new Queue<(int x,int y)>();
            if (Clear(sx*16+insetX,sy*16+insetY,bodyW,bodyH)) { seen[sx,sy]=true; queue.Enqueue((sx,sy)); }
            int[] dx={-1,1,0,0}, dy={0,0,-1,1};
            while(queue.Count>0) {
                var a=queue.Dequeue();
                for(int d=0;d<4;d++) {
                    int x=a.x+dx[d], y=a.y+dy[d];
                    if(x<0||y<0||x>=width||y>=height||seen[x,y])continue;
                    int left=Math.Min(a.x,x)*16+insetX, top=Math.Min(a.y,y)*16+insetY;
                    if(!Clear(left,top,bodyW+Math.Abs(dx[d])*16,bodyH+Math.Abs(dy[d])*16))continue;
                    seen[x,y]=true;queue.Enqueue((x,y));
                }
            }
            return seen;
        }

        private static bool RibConnected(P.Cell[,] cells, int rootX, int rootY, int dir, int count) {
            for(int d=0;d<2;d++) for(int h=0;h<4;h++)
                if(cells[rootX+dir*d,rootY+h].Key!="bone")return false;
            var seen=new HashSet<(int x,int y)>();var queue=new Queue<(int x,int y)>();queue.Enqueue((rootX,rootY));
            while(queue.Count>0) {
                var a=queue.Dequeue();
                if(a.x<0||a.y<0||a.x>=cells.GetLength(0)||a.y>=cells.GetLength(1)||seen.Contains(a)||
                    cells[a.x,a.y].Key is not ("bone" or "rib"))continue;
                seen.Add(a);queue.Enqueue((a.x-1,a.y));queue.Enqueue((a.x+1,a.y));queue.Enqueue((a.x,a.y-1));queue.Enqueue((a.x,a.y+1));
            }
            return seen.Count==count;
        }

        private static void Mutate(string name, P.Cell[,] cells, P.Cluster[] teeth) {
            void Solid(int x,int y) => cells[x,y]=cells[x,y] with { Key="stone", Slope=0 };
            switch(name) {
                case "blocked-connector": for(int y=66;y<=68;y++)Solid(62,y);break;
                case "one-high": Solid(62,66);Solid(62,68);break;
                case "one-wide": Solid(69,59);break;
                case "missing-rib": for(int y=31;y<40;y++)if(cells[32,y].Key=="rib")cells[32,y]=cells[32,y] with { Key=null,Slope=0 };break;
                case "missing-root": cells[26,31]=cells[26,31] with { Key="stone" };break;
                case "unsupported-cluster": cells[88,46]=cells[88,46] with { Key=null };break;
                case "bad-variant": teeth[0]=teeth[0] with { Variant=8 };break;
                case "extra-opening": cells[80,80]=new(null,0,"stone",P.Zone.Connector);break;
                case "bad-boundary": cells[0,0]=new("stone");break;
                default: throw new Exception("Unknown mutation " + name);
            }
        }
        private static string Reason(string defect) => defect switch {
            "blocked-connector" or "one-high" or "one-wide" => "BODY_PATH",
            "missing-rib" or "missing-root" => "RIB_OR_ROOT",
            "unsupported-cluster" => "CLUSTER_ANCHOR", "bad-variant" => "CLUSTER_VARIANT",
            "extra-opening" => "CELL_LAYOUT", "bad-boundary" => "BOUNDARY", "bad-size" => "DIMENSIONS",
            _ => throw new Exception("Unknown expected reason")
        };
        public static void RejectDefect(string defect) {
            var plan=P.Create();var cells=plan.ExportCells();var teeth=plan.ExportClusters();
            if(defect=="bad-size")cells=new P.Cell[1,1];else Mutate(defect,cells,teeth);
            // Deliberately uncaught: -Defect is a real failing CLI path, not a printed PASS.
            plan.ValidateExport(cells,teeth);
            throw new Exception("SHALLOW_TEST: defect accepted " + defect);
        }
        public static string Run() {
            checks=0;var plan=P.Create();var cells=plan.ExportCells();var teeth=plan.ExportClusters();
            Check(cells.GetLength(0)==112&&cells.GetLength(1)==104,"fixed dimensions");
            Check(P.BodyWidth==20&&P.BodyHeight==42,"public Player default body seam");
            var other=P.Create().ExportCells();int caps=0,bone=0,rib=0,amber=0,amberWall=0;
            for(int x=0;x<112;x++)for(int y=0;y<104;y++) {
                Check(cells[x,y]==other[x,y],"deterministic export");
                if(cells[x,y].Key=="cap")caps++;
                if(cells[x,y].Key=="bone")bone++;
                if(cells[x,y].Key=="rib")rib++;
                if(cells[x,y].Key=="amber")amber++;
                if(cells[x,y].Wall=="amber")amberWall++;
            }
            Check(caps==76&&bone==24&&rib==66&&amber==18&&amberWall==36,"bounded material counts");
            for(int x=4;x<108;x++)if(x<28||x>=56)
                Check(cells[x,12].Key=="cap"&&cells[x,13].Key=="grass"&&cells[x,14].Key=="soil","full rooted cap");
            Check(RibConnected(cells,26,31,1,32),"left continuous tapered rib");
            Check(RibConnected(cells,57,55,-1,35),"right continuous tapered rib");
            Check(RibConnected(cells,26,81,1,23),"lower continuous tapered rib");
            // Independent column occupancy: monotonically tapered, single terminal cell.
            foreach(var r in plan.ExportRibs()) {
                int previous=5;
                for(int d=0;d<r.Length;d++) {
                    int count=0;for(int y=r.Root.Y;y<r.Root.Y+10;y++)
                        if(cells[r.Root.X+r.Direction*d,y].Key is "bone" or "rib")count++;
                    Check(count>0&&count<=previous,"non-increasing shaft thickness");previous=count;
                }
                Check(previous==1,"one-cell tapered tip");
            }
            int[,] expected={{88,42,0,1,3},{28,20,1,0,1},{88,28,2,1,0},{52,40,3,3,1},
                {92,42,4,1,3},{28,24,5,0,1},{92,28,6,1,0},{52,44,7,3,1}};
            for(int n=0;n<8;n++) {
                var c=teeth[n];int face=n%4;
                Check(c.Root==new P.Point(expected[n,0],expected[n,1])&&c.Variant==expected[n,2],"saved root/variant");
                Check(c.Surface==face&&c.Style==n/4&&c.OriginOffset==new P.Point(expected[n,3],expected[n,4]),"native bank mapping");
                for(int q=0;q<4;q++) {
                    int sx=face==1?c.Root.X-1:face==3?c.Root.X+4:c.Root.X+q;
                    int sy=face==0?c.Root.Y+4:face==2?c.Root.Y-1:c.Root.Y+q;
                    Check(c.Support(q)==new P.Point(sx,sy),"native four-cell anchor mapping");
                    Check(cells[sx,sy].Key!=null&&cells[sx,sy].Slope==0&&cells[sx,sy].Region==P.Zone.Geology,"full geological socket");
                }
            }
            Check(cells[69,59].Key==null&&cells[70,59].Key==null&&cells[68,59].Key!=null&&cells[71,59].Key!=null,"two-wide bare rise");
            Check(cells[62,66].Key==null&&cells[62,67].Key==null&&cells[62,68].Key==null&&cells[62,65].Key!=null&&cells[62,69].Key!=null,"three-high bare throat");
            foreach(var turn in new[]{(68,65),(68,50),(78,50),(78,39)})
                for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++)Check(cells[turn.Item1+dx,turn.Item2+dy].Key==null,"widened turn");
            var forward=Reach(cells,teeth,40,14,true);var reverse=Reach(cells,teeth,99,36,true);
            Check(forward[40,91]&&forward[99,36]&&reverse[40,14],"independent swept standing-body routes with hazard envelopes");
            int negatives=0;
            foreach(string defect in new[]{"blocked-connector","one-high","one-wide","missing-rib","missing-root","unsupported-cluster","bad-variant","extra-opening","bad-boundary","bad-size"}) {
                var broken=plan.ExportCells();var brokenTeeth=plan.ExportClusters();
                if(defect=="bad-size")broken=new P.Cell[1,1];else Mutate(defect,broken,brokenTeeth);
                if(defect=="blocked-connector"||defect=="one-high"||defect=="one-wide") {
                    Check(!Reach(broken,brokenTeeth,40,14,true)[99,36],"independent body rejection "+defect);
                    Check(Reach(broken,brokenTeeth,40,14,false)[99,36]==(defect!="blocked-connector"),"point-only false-positive control "+defect);
                }
                if(defect=="missing-rib"||defect=="missing-root")Check(!RibConnected(broken,26,31,1,32),"independent rib/root rejection");
                bool rejected=false;
                try{plan.ValidateExport(broken,brokenTeeth);}
                catch(InvalidOperationException e){if(e.Message!="SHALLOW_PLAN: "+Reason(defect))throw;rejected=true;}
                Check(rejected,"real exported-plan rejection "+defect);negatives++;
            }
            cells[40,14]=new("stone");teeth[0]=new(new(0,0),99);
            plan.ValidateExport(plan.ExportCells(),plan.ExportClusters());Check(plan.ExportCells()[40,14].Key==null,"exports cannot mutate plan");
            return $"PASS shallow traversal pure plan: {checks} assertions; {negatives} rejected exported-plan defects; 20x42 swept-body and point-only negative controls. No native movement, damage, art or worldgen claim.";
        }
    }
}
'@
Add-Type -TypeDefinition ($source + "`n" + $harness)
if ($Defect -ne 'none') {
    [apogean.Content.Diagnostics.ShallowTraversalChecks]::RejectDefect($Defect)
} else {
    [apogean.Content.Diagnostics.ShallowTraversalChecks]::Run()
}
