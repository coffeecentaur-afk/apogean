using System;
using System.Collections.Generic;
using System.Linq;

namespace apogean.Common.WorldGeneration
{
    // Engine-free anatomy plan. Versioned separately from the immutable approved QA sketch.
    internal sealed class MawSeedPlan
    {
        internal const int Width = 280, Height = 256, SurfaceY = 56, Version = 1;
        internal readonly record struct Point(int X, int Y);
        internal readonly record struct Cell(string Key = null, string Wall = null, byte Slope = 0, bool Side = false, bool Write = false);
        internal readonly record struct Tooth(Point Root, int Variant)
        {
            internal int Face => Variant % 4;
            internal Point Support(int i) => Face switch {
                1 => new(Root.X - 1, Root.Y + i), 2 => new(Root.X + i, Root.Y - 1),
                3 => new(Root.X + 4, Root.Y + i), _ => new(Root.X + i, Root.Y + 4)
            };
        }
        internal readonly record struct Strand(Point Root, int Length);
        internal readonly record struct Rib(Point Root, Point Tip, Point[] Roots);
        internal readonly record struct Pocket(Point Center, int RadiusX, int RadiusY, string Role);
        internal Cell[,] Cells { get; } = new Cell[Width, Height];
        internal List<Tooth> Teeth { get; } = new();
        internal List<Strand> Fibers { get; } = new();
        internal List<Rib> Ribs { get; } = new();
        internal List<Pocket> Pockets { get; } = new();
        internal List<Point> Chests { get; } = new();
        internal List<Point> NodeSites { get; } = new();
        internal Point Entrance { get; private set; }
        internal Point Exit { get; private set; }
        internal int LayoutVersion { get; }
        internal int[] WallStartY { get; } = new int[Width];
        // The aperture follows sampled shoulders, not the raised lip. Buried roots have a local host mask.
        internal bool AllowsWall(int x, int y) => In(x,y) && (y >= WallStartY[x] || buriedBacking[x,y]);
        internal Point LowerRejoinEntrance { get; private set; }
        internal Point LowerRejoinExit { get; private set; }
        internal Tooth LowerRejoinHazard { get; private set; }
        internal bool IsCentralShaft(int x,int y) => In(x,y) && Math.Abs(x-Center[y/4*4+2])<=halfWidth[y/4*4+2];
        internal int[] Ground { get; } = new int[Width];
        internal int[] Center { get; } = new int[Height];
        private readonly bool[,] active = new bool[Width, Height], air = new bool[Width, Height], side = new bool[Width, Height], route = new bool[Width, Height];
        private readonly bool[,] buriedBacking = new bool[Width,Height];
        private readonly int[] halfWidth = new int[Height];
        private uint random;
        private int Next(int min, int max) { random ^= random << 13; random ^= random >> 17; random ^= random << 5; return min + (int)(random % (uint)(max - min)); }
        private static bool In(int x, int y) => x >= 4 && y >= 4 && x < Width - 4 && y < Height - 4;
        private static double Distance(double x, double y, Point a, Point b, out double t)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y, length = dx * dx + dy * dy;
            t = length == 0 ? 0 : Math.Clamp(((x - a.X) * dx + (y - a.Y) * dy) / length, 0, 1);
            return Math.Sqrt(Math.Pow(x - a.X - t * dx, 2) + Math.Pow(y - a.Y - t * dy, 2));
        }
        private MawSeedPlan(int version, int seed, int[] spine, int leftSurface, int rightSurface)
        {
            LayoutVersion = version;
            random = unchecked((uint)seed) ^ 0x9e3779b9u; if (random == 0) random = 1;
            Build(spine, leftSurface, rightSurface); Validate();
        }
        internal static MawSeedPlan Create(int seed, int[] spine = null, int leftSurface = SurfaceY, int rightSurface = SurfaceY)
            => new(1, seed, spine, leftSurface, rightSurface);
        internal static MawSeedPlan CreateVersion(int version, int seed, int[] spine = null, int leftSurface = SurfaceY, int rightSurface = SurfaceY)
            => version is 1 or 2 or 3 ? new(version, seed, spine, leftSurface, rightSurface)
                : throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported Maw seed layout version.");

        private void MarkPath(Point[] points, double radius, bool isSide)
        {
            for (int i = 1; i < points.Length; i++) {
                Point a = points[i - 1], b = points[i];
                int left = Math.Max(4, Math.Min(a.X,b.X) - (int)radius - 12), right = Math.Min(Width-5, Math.Max(a.X,b.X) + (int)radius + 12);
                int top = Math.Max(4, Math.Min(a.Y,b.Y) - (int)radius - 12), bottom = Math.Min(Height-5, Math.Max(a.Y,b.Y) + (int)radius + 12);
                for (int x=left;x<=right;x++) for (int y=top;y<=bottom;y++) {
                    double d = Distance(x+.5,y+.5,a,b,out _);
                    if(d<=radius+10) active[x,y]=true;
                    if(d<=radius) { air[x,y]=true; side[x,y]|=isSide; }
                    if(d<=2.4) route[x,y]=true;
                }
            }
        }
        private void MarkPocket(Point c, int rx, int ry, string role)
        {
            Pockets.Add(new(c,rx,ry,role));
            // Angular but irregular cavern contour, not an isolated rectangular room.
            int[] radii = Enumerable.Range(0,12).Select(_=>Next(88,108)).ToArray();
            for(int x=Math.Max(4,c.X-rx-12);x<Math.Min(Width-4,c.X+rx+13);x++)
                for(int y=Math.Max(4,c.Y-ry-12);y<Math.Min(Height-4,c.Y+ry+13);y++) {
                    double nx=(x-c.X)/(double)rx, ny=(y-c.Y)/(double)ry, d=Math.Sqrt(nx*nx+ny*ny);
                    double angle=(Math.Atan2(ny,nx)+Math.PI)/(Math.PI*2)*12;
                    int band=(int)angle%12; double amount=angle-Math.Floor(angle);
                    double edge=(radii[band]*(1-amount)+radii[(band+1)%12]*amount)/100.0;
                    if(d<=edge+.5) active[x,y]=true;
                    if(d<=edge) {air[x,y]=true;side[x,y]=true;}
                }
        }
        private void Build(int[] spine, int leftSurface, int rightSurface)
        {
            if(spine!=null && spine.Length!=Height)throw new ArgumentException("Spine must cover every local row.");
            if(leftSurface is < 40 or > 76 || rightSurface is < 40 or > 76)throw new ArgumentException("Surface shoulders exceed the bounded slope profile.");
            int phase=Next(0,100), lean=Next(0,2)==0?-1:1;
            for(int y=0;y<Height;y++) {
                double fade=Math.Min(1,Math.Max(0,(Height-24-y)/24.0));
                int baseX=spine==null?Width/2:spine[y];
                Center[y]=baseX+(int)(Math.Sin((y+phase)/49.0)*12*fade);
                if(Center[y]<85||Center[y]>Width-85)throw new ArgumentException("Spine exceeds shallow placement envelope.");
                halfWidth[y]=y>Height-28 ? 9+(Height-5-y)/3 : 25+(int)(Math.Sin((y+phase)/31.0)*4);
                if(LayoutVersion>=2&&y<112)halfWidth[y]-=(int)(10*Math.Clamp((112-y)/56.0,0,1));
            }
            int mouthX=Center[32];
            for(int x=4;x<Width-4;x++) {
                int baseline=leftSurface+(rightSurface-leftSurface)*(x-4)/(Width-9);
                int rise=Math.Max(0,26-(int)(Math.Abs(Math.Abs(x-mouthX)-32)*.55));
                if(LayoutVersion==2)rise=Math.Max(0,26-(int)(Math.Abs(Math.Abs(x-mouthX)-22)*1.5));
                if(LayoutVersion==3) {
                    // Rounded, unequal banks reuse the existing phase/lean; no new RNG draws.
                    int direction=x<mouthX?-1:1,width=direction==lean?24:22;
                    double distance=(Math.Abs(x-mouthX)-width)/(double)width;
                    double dome=Math.Sqrt(Math.Max(0,1-distance*distance));
                    double grain=1.1*Math.Sin((x+phase)/4.5)+.55*Math.Sin((x-phase)/2.3);
                    rise=Math.Clamp((int)Math.Round(dome*(25.5+grain)),0,26);
                }
                Ground[x]=baseline-rise;
                WallStartY[x]=LayoutVersion==1?Ground[x]+3:baseline+3;
                if(LayoutVersion==3) {
                    int radius=halfWidth[34]+3,dx=x-mouthX;
                    int centerBaseline=leftSurface+(rightSurface-leftSurface)*(mouthX-4)/(Width-9);
                    if(Math.Abs(dx)<=radius)WallStartY[x]=centerBaseline+3+(int)Math.Sqrt(radius*radius-dx*dx);
                }
                for(int y=4;y<Height-4;y++) {
                    int facet=y/4*4+2;
                    bool throat=Math.Abs(x-Center[facet])<=halfWidth[facet];
                    if(Math.Abs(x-Center[facet])<=halfWidth[facet]+15)active[x,y]=true;
                    // A tapered surface wound blends into sampled shoulders; no full-box clear.
                    if(Math.Abs(x-mouthX)<=90 && y>=Ground[x]-10 && y<baseline+22)active[x,y]=true;
                    if(throat || y<Ground[x])air[x,y]=true;
                    if(Math.Abs(x-Center[facet])<=3)route[x,y]=true;
                }
            }
            // Independent cave groups have a substantial vertical interval, not a cache per rib.
            for(int group=0;group<2;group++) {
                int y=(group==0?99+Math.Max(0,Math.Max(leftSurface,rightSurface)-SurfaceY)/2:204)+Next(-6,7), direction=group==0?lean:-lean;
                int cx=Math.Clamp(Center[y]+direction*Next(71,86),36,Width-37);
                Point c=new(cx,y);int rx=group==0?Next(22,28):Next(17,23), ry=Next(12,17);
                MarkPocket(c,rx,ry,group==0?"cache":"exploration");
                Point a=new(Center[y-15]+direction*(halfWidth[y-15]-2),y-15);
                if(LayoutVersion>=2&&group==0)MarkLowerRejoin(c,direction,a);
                else MarkPath(new[]{a,new Point(a.X+direction*15,y+6),new Point(cx-direction*14,y+9),c},3.6,true);
                if(group==0) Chests.Add(new(cx,y+ry-5));
                else {
                    Point node=new(Math.Clamp(cx+direction*23,18,Width-19),y-35);
                    MarkPath(new[]{c,new Point(cx+direction*10,y-14),new Point(node.X,y-24),node},3.2,true);
                    MarkPocket(node,10,8,"node-reservation");NodeSites.Add(node);
                }
            }
            for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++) {
                if(!active[x,y])continue;
                string key=air[x,y]?null:y<Ground[x]+2?"cap":y<Ground[x]+4?"grass":y<Ground[x]+10?"soil":"stone";
                // Two clear wall rows avoid the native wall renderer protruding above topsoil.
                string wall=LayoutVersion==1 ? (y<Ground[x]+3?null:y<Ground[x]+8?"soil":"stone") : Backing(x,y);
                Cells[x,y]=new(key,wall,0,side[x,y],true);
            }
            var geology=(Cell[,])Cells.Clone();
            for(int x=5;x<Width-5;x++)for(int y=5;y<Height-5;y++) {
                if(geology[x,y].Key==null||geology[x,y].Key=="cap")continue;
                foreach(Point d in new[]{new Point(-1,0),new Point(1,0),new Point(0,-1),new Point(0,1)}) {
                    Cell n=geology[x+d.X,y+d.Y];
                    if(n.Write&&n.Key==null&&n.Wall!=null) {Cells[x,y]=new("grass",geology[x,y].Wall==null?null:"grass",0,n.Side,true);break;}
                }
            }
            // Cohesive tapering shafts and three connected roots. No porous joining block.
            bool[,] bone=new bool[Width,Height];
            void Stroke(Point a,Point b,double ra,double rb) {
                for(int x=Math.Max(4,Math.Min(a.X,b.X)-4);x<=Math.Min(Width-5,Math.Max(a.X,b.X)+4);x++)
                    for(int y=Math.Max(4,Math.Min(a.Y,b.Y)-4);y<=Math.Min(Height-5,Math.Max(a.Y,b.Y)+4);y++)
                        if(Distance(x,y,a,b,out double t)<=ra+(rb-ra)*t)bone[x,y]=true;
            }
            int index=0;
            int firstRib=LayoutVersion==1?Math.Max(43,Math.Max(leftSurface,rightSurface)-13):Math.Max(leftSurface,rightSurface)+8;
            for(int y=firstRib;y<Height-35;y+=Next(25,34)) {
                int dir=(index%3==2?lean:-lean)*(index%2==0?1:-1), cy=y+Next(-3,4), facet=cy/4*4+2;
                bool BranchIntersects(int direction) {
                    int bx=Center[facet]+direction*(halfWidth[facet]+2), tx=bx-direction*25;
                    int outer=LayoutVersion==1?bx:bx+direction*23;
                    for(int x=Math.Min(outer,tx)-3;x<=Math.Max(outer,tx)+3;x++)for(int yy=cy-18;yy<=cy+(LayoutVersion==1?10:12);yy++)
                        if(In(x,yy)&&side[x,yy])return true;
                    return false;
                }
                // Move a conflicting rib to the other bank, not into the narrow connector.
                // Both banks are considered once; this is a bounded decision, not a retry loop.
                if(BranchIntersects(dir))dir=-dir;
                if(BranchIntersects(dir)){index++;continue;}
                int boundary=Center[facet]+dir*(halfWidth[facet]+2);
                int projection=Next(15,24);
                if(LayoutVersion>=2)projection=Math.Min(projection,halfWidth[facet]-6);
                Point root=new(boundary,cy),tip=new(boundary-dir*projection,cy-Next(0,16));
                Point[] ends={new(boundary+dir*Next(9,15),cy-7),new(boundary+dir*Next(12,20),cy+1),new(boundary+dir*Next(9,16),cy+9)};
                index++;
                Ribs.Add(new(root,tip,ends));
                foreach(Point end in ends)Stroke(end,root,.65,2.4);
                Point bend=new((root.X+tip.X)/2,(root.Y+tip.Y)/2+3);
                Stroke(root,bend,2.7,2.1);Stroke(bend,tip,2.1,.65);
            }
            if(LayoutVersion>=2) BackDeepRoots(bone);
            for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++)if(bone[x,y]) {
                bool l=bone[x-1,y],r=bone[x+1,y],u=bone[x,y-1],d=bone[x,y+1];
                byte slope=!u&&d&&l&&!r?(byte)1:!u&&d&&r&&!l?(byte)2:!d&&u&&l&&!r?(byte)3:!d&&u&&r&&!l?(byte)4:(byte)0;
                string wall=LayoutVersion==1 ? (y<Ground[x]+3?null:Cells[x,y].Wall) : (AllowsWall(x,y)?Cells[x,y].Wall??Backing(x,y):null);
                Cells[x,y]=new("rib",wall,slope,Cells[x,y].Side,true);
            }
            foreach(Point c in Chests) {
                for(int x=c.X-2;x<=c.X+4;x++)for(int y=c.Y-3;y<=c.Y+1;y++)Cells[x,y]=new(null,"grass",0,true,true);
                for(int x=c.X-2;x<=c.X+4;x++)Cells[x,c.Y+2]=new("grass","grass",0,true,true);
            }
            foreach(Pocket p in Pockets) {
                Point amber=new(p.Center.X,p.Center.Y-p.RadiusY);
                for(int x=amber.X-1;x<=amber.X+1;x++)for(int y=amber.Y-1;y<=amber.Y+1;y++) {
                    Cell c=Cells[x,y];Cells[x,y]=new(c.Key==null?null:"amber","amber",0,true,true);
                }
            }
            bool[,] occupied=new bool[Width,Height];
            if(LayoutVersion>=2) {
                Tooth t=LowerRejoinHazard;
                Teeth.Add(t);
                for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++)occupied[t.Root.X+dx,t.Root.Y+dy]=true;
                for(int i=0;i<4;i++){Point at=t.Support(i);Cells[at.X,at.Y]=new("grass","stone",0,true,true);}
            }
            bool CacheNear(int x,int y)=>Chests.Any(c=>x>=c.X-3&&x<=c.X+4&&y>=c.Y-4&&y<=c.Y+3);
            for(int y=14;y<Height-18;y++)for(int x=6;x<Width-9;x++) {
                if(!Cells[x,y].Write||Cells[x,y].Key!=null||CacheNear(x,y))continue;
                for(int face=0;face<4;face++) {
                    Tooth t=new(new(x,y),face+Next(0,2)*4);bool valid=true;
                    for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++) {
                        Cell c=Cells[x+dx,y+dy];
                        valid&=c.Write&&c.Key==null&&!occupied[x+dx,y+dy]&&!route[x+dx,y+dy]&&!CacheNear(x+dx,y+dy);
                    }
                    for(int i=0;i<4;i++){Point a=t.Support(i);Cell c=Cells[a.X,a.Y];valid&=c.Write&&c.Key!=null&&c.Key!="rib"&&c.Slope==0;}
                    if(Cells[x,y].Side&&(x+y)%7!=0)valid=false;
                    if(!valid)continue;Teeth.Add(t);
                    for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++)occupied[x+dx,y+dy]=true;
                    break;
                }
            }
            for(int x=10;x<Width-10;x+=Next(4,8))for(int y=60;y<Height-15;y++) {
                if(Cells[x,y].Key!="grass"||!Cells[x,y+1].Side)continue;
                int length=Next(2,7);bool valid=true;
                for(int d=1;d<=length;d++)valid&=Cells[x,y+d].Write&&Cells[x,y+d].Key==null&&!occupied[x,y+d]&&!CacheNear(x,y+d);
                if(!valid)continue;Fibers.Add(new(new(x,y),length));break;
            }
            if(LayoutVersion==3)ClaimSurfaceAir();
            // Choose a clear inspection arrival on the existing approach, never clear teeth
            // or manufacture a safety ledge to make a fixed teleport coordinate fit.
            Exit=new(Center[Height-8]-1,Height-9);
            bool[,] outletComponent=Reachable(Exit);
            Entrance=default;
            for(int x=mouthX-85;x<=mouthX-40;x+=2) {
                for(int lift=4;lift<=(LayoutVersion==1?4:9);lift++) {
                    int y=Ground[x]-lift;bool fits=true;
                    for(int dx=0;dx<2;dx++)for(int dy=0;dy<3;dy++)
                        fits&=In(x+dx,y+dy)&&Cells[x+dx,y+dy].Write&&Cells[x+dx,y+dy].Key==null&&!occupied[x+dx,y+dy];
                    if(fits&&outletComponent[x,y]){Entrance=new(x,y);break;}
                }
                if(Entrance!=default)break;
            }
            if(Entrance==default)throw new InvalidOperationException("MAW_SEED: no clear natural approach.");
        }
        private void ClaimSurfaceAir()
        {
            // Apply after anatomy/object RNG, never overwrite an existing desired cell.
            // Native preflight must approve this entire mask plus impact and reject terrain
            // crossing its y=4 ceiling; clipping a hill here is not proof of a clear upper edge.
            int mouth=Center[32];
            for(int x=Math.Max(4,mouth-90);x<=Math.Min(Width-5,mouth+90);x++)
                for(int y=4;y<Ground[x];y++)if(!Cells[x,y].Write)Cells[x,y]=new(Write:true);
        }
        private string Backing(int x,int y) => !AllowsWall(x,y)?null:y<WallStartY[x]+5?"soil":"stone";
        private void MarkLowerRejoin(Point cave,int direction,Point upper)
        {
            int top=upper.Y,bottom=cave.Y+52;
            int Bank(int y) {
                // The complete 2x3 body stays outside the central shaft, including bends in supplied spines.
                int edge=direction>0?0:Width;
                for(int yy=y-4;yy<=y+4;yy++) {
                    int row=yy/4*4+2,value=Center[row]+direction*(halfWidth[row]+9);
                    edge=direction>0?Math.Max(edge,value):Math.Min(edge,value);
                }
                return edge;
            }
            Point[] bends={new(Bank(top),top),cave,new(cave.X+direction*9,cave.Y+17),
                new(cave.X-direction*13,cave.Y+33),new(Bank(bottom),bottom)};
            var path=new List<Point>();int segment=1;
            for(int y=top;y<=bottom;y++) {
                while(y>bends[segment].Y)segment++;
                Point a=bends[segment-1],b=bends[segment];
                int x=a.X+(b.X-a.X)*(y-a.Y)/(b.Y-a.Y);
                x=direction>0?Math.Max(x,Bank(y)):Math.Min(x,Bank(y));
                path.Add(new(Math.Clamp(x,18,Width-19),y));
            }
            MarkPath(path.ToArray(),4.6,true);
            Point last=path[^1];
            MarkPath(new[]{upper,path[0]},4.6,true);
            MarkPath(new[]{last,new Point(Center[bottom/4*4+2],bottom)},4.6,true);
            LowerRejoinEntrance=new(path[0].X-1,top-1);LowerRejoinExit=new(last.X-1,bottom-1);
            Point mid=path[cave.Y+26-top];
            Point root=new(mid.X+(direction>0?6:-9),mid.Y-2);
            LowerRejoinHazard=new(root,(direction>0?3:1)+Next(0,2)*4);
            // A four-tile native tooth sits beside, not across, the protected body corridor.
            for(int x=Math.Min(mid.X,root.X);x<=Math.Max(mid.X,root.X+3);x++)for(int y=root.Y;y<root.Y+4;y++)
                {active[x,y]=true;air[x,y]=true;side[x,y]=true;}
            for(int i=0;i<4;i++){Point at=LowerRejoinHazard.Support(i);active[at.X,at.Y]=true;air[at.X,at.Y]=false;}
        }
        private void BackDeepRoots(bool[,] bone)
        {
            for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++)
                if(bone[x,y]&&y>=Ground[x]+10)buriedBacking[x,y]=true;
            // Only bury the bankward branches, never the projecting shaft or its tip.
            foreach(Rib rib in Ribs)foreach(Point end in rib.Roots)
                for(int x=Math.Max(4,Math.Min(end.X,rib.Root.X)-3);x<=Math.Min(Width-5,Math.Max(end.X,rib.Root.X)+3);x++)
                    for(int y=Math.Max(4,Math.Min(end.Y,rib.Root.Y)-3);y<=Math.Min(Height-5,Math.Max(end.Y,rib.Root.Y)+3);y++) {
                        if(y<Ground[x]+10||Distance(x,y,end,rib.Root,out _) > 3.5)continue;
                        // Existing air/side routes are authoritative; the native writer still preflights every newly owned cell.
                        Cell c=Cells[x,y];
                        if(bone[x,y]||route[x,y]||side[x,y]||(c.Write&&c.Key==null))continue;
                        buriedBacking[x,y]=true;
                        Cells[x,y]=new(c.Key??"stone",c.Wall??Backing(x,y),c.Slope,c.Side,true);
                    }
        }
        internal bool[,] Reachable(Point? start = null)
            => ReachableCore(start??Entrance,false);
        internal bool[,] ReachableBranch() => ReachableCore(LowerRejoinEntrance,true);
        private bool[,] ReachableCore(Point origin,bool branchOnly)
        {
            var blocked=new bool[Width,Height];
            foreach(Tooth t in Teeth)for(int x=0;x<4;x++)for(int y=0;y<4;y++)blocked[t.Root.X+x,t.Root.Y+y]=true;
            bool Fits(int x,int y) {
                if(!In(x,y)||!In(x+1,y+2))return false;
                for(int dx=0;dx<2;dx++)for(int dy=0;dy<3;dy++)
                    if(!Cells[x+dx,y+dy].Write||Cells[x+dx,y+dy].Key!=null||blocked[x+dx,y+dy]||
                        (branchOnly&&(!Cells[x+dx,y+dy].Side||IsCentralShaft(x+dx,y+dy))))return false;
                return true;
            }
            var seen=new bool[Width,Height];var queue=new Queue<Point>();
            void Visit(int x,int y){if(!Fits(x,y)||seen[x,y])return;seen[x,y]=true;queue.Enqueue(new(x,y));}
            Visit(origin.X,origin.Y);
            while(queue.Count>0){Point p=queue.Dequeue();Visit(p.X-1,p.Y);Visit(p.X+1,p.Y);Visit(p.X,p.Y-1);Visit(p.X,p.Y+1);}
            return seen;
        }
        internal void Validate()
        {
            void Check(bool ok,string why){if(!ok)throw new InvalidOperationException("MAW_SEED: "+why);}
            Check(Ribs.Count>=3&&Ribs.Count<=9,"rib count");Check(Teeth.Count>=25&&Teeth.Count<=150,"tooth count "+Teeth.Count);
            Check(Chests.Count==1&&NodeSites.Count==1&&Pockets.Count==3,"sparse distinct pocket roles");
            Check(Pockets[0].Role=="cache"&&Pockets[1].Role=="exploration"&&Pockets[2].Role=="node-reservation","pocket semantics");
            Check(Pockets[1].Center.Y-Pockets[1].RadiusY-(Pockets[0].Center.Y+Pockets[0].RadiusY)>=50,"independent cave separation");
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++) {
                Cell c=Cells[x,y];
                Check(c.Key is null or "cap" or "grass" or "soil" or "stone" or "rib" or "amber","material");
                Check(c.Wall is null or "grass" or "soil" or "stone" or "amber","wall material");
                Check(c.Slope<=4,"slope");if(!c.Write)Check(c==default,"inactive write");
                if(!In(x,y))Check(c==default,"write padding");
                if(c.Key=="amber"||c.Wall=="amber")Check(c.Side,"side amber");
                if(LayoutVersion==3&&c.Wall!=null&&y<WallStartY[x])
                    Check(c.Key!=null&&y>=Ground[x]+10,"sky wall above U contour");
            }
            for(int y=0;y<Height;y++)Check(Center[y]>=85&&Center[y]<=Width-85,"center metadata");
            for(int x=4;x<Width-4;x++)Check(Ground[x]>=14&&Ground[x]<=76,"ground metadata");
            foreach(Pocket p in Pockets)Check(p.RadiusX>0&&p.RadiusY>0&&In(p.Center.X-p.RadiusX,p.Center.Y-p.RadiusY)&&In(p.Center.X+p.RadiusX,p.Center.Y+p.RadiusY),"pocket envelope");
            foreach(Rib rib in Ribs) {
                Check(rib.Roots!=null&&rib.Roots.Length==3&&rib.Roots.Distinct().Count()==3,"rib root metadata");
                var seen=new HashSet<Point>();var queue=new Queue<Point>();queue.Enqueue(rib.Root);
                while(queue.Count>0){Point p=queue.Dequeue();if(!In(p.X,p.Y)||Cells[p.X,p.Y].Key!="rib"||!seen.Add(p))continue;queue.Enqueue(new(p.X-1,p.Y));queue.Enqueue(new(p.X+1,p.Y));queue.Enqueue(new(p.X,p.Y-1));queue.Enqueue(new(p.X,p.Y+1));}
                Check(seen.Contains(rib.Tip)&&rib.Roots.All(seen.Contains),"continuous rib and branches");
                bool rooted=seen.Any(p=>new[]{new Point(p.X-1,p.Y),new Point(p.X+1,p.Y),new Point(p.X,p.Y-1),new Point(p.X,p.Y+1)}
                    .Any(n=>In(n.X,n.Y)&&Cells[n.X,n.Y].Write&&Cells[n.X,n.Y].Key is "stone" or "soil" or "grass" or "cap"));
                Check(rooted,"rib attachment to host terrain");
                if(LayoutVersion>=2)foreach(Point end in rib.Roots) {
                    if(end.Y<Ground[end.X]+10||!AllowsWall(end.X,end.Y))continue;
                    bool hosted=false;
                    for(int dx=-2;dx<=2;dx++)for(int dy=-2;dy<=2;dy++) {
                        Cell c=Cells[end.X+dx,end.Y+dy];
                        hosted|=c.Write&&c.Wall!=null&&c.Key is "stone" or "soil" or "grass" or "cap";
                    }
                    Check(hosted,"deep root owned host envelope");
                }
            }
            var occupied=new HashSet<Point>();
            foreach(Tooth t in Teeth) {
                Check(t.Variant>=0&&t.Variant<8,"tooth variant");
                for(int x=0;x<4;x++)for(int y=0;y<4;y++) {
                    Point p=new(t.Root.X+x,t.Root.Y+y);
                    Check(In(p.X,p.Y)&&Cells[p.X,p.Y].Write&&Cells[p.X,p.Y].Key==null&&occupied.Add(p),"tooth footprint");
                }
                for(int i=0;i<4;i++){Point p=t.Support(i);Check(In(p.X,p.Y)&&Cells[p.X,p.Y].Write&&Cells[p.X,p.Y].Key!=null&&Cells[p.X,p.Y].Key!="rib"&&Cells[p.X,p.Y].Slope==0,"tooth support");}
            }
            foreach(Point c in Chests) {
                Check(In(c.X-2,c.Y-3)&&In(c.X+4,c.Y+2),"chest bounds");
                for(int x=0;x<2;x++)for(int y=0;y<2;y++)Check(Cells[c.X+x,c.Y+y].Side&&Cells[c.X+x,c.Y+y].Key==null&&occupied.Add(new(c.X+x,c.Y+y)),"chest footprint");
                Check(Cells[c.X,c.Y+2].Key=="grass"&&Cells[c.X+1,c.Y+2].Key=="grass","chest feet");
            }
            Check(Fibers.Count>=5&&Fibers.Count<=70,"fiber count");
            foreach(Strand f in Fibers) {
                Check(f.Length is >=2 and <=6&&In(f.Root.X,f.Root.Y)&&In(f.Root.X,f.Root.Y+f.Length)&&Cells[f.Root.X,f.Root.Y].Key=="grass","fiber anchor");
                for(int d=1;d<=f.Length;d++)Check(Cells[f.Root.X,f.Root.Y+d].Write&&Cells[f.Root.X,f.Root.Y+d].Key==null&&occupied.Add(new(f.Root.X,f.Root.Y+d)),"fiber footprint");
            }
            foreach(Point c in NodeSites)Check(In(c.X,c.Y)&&Cells[c.X,c.Y].Write&&Cells[c.X,c.Y].Side&&Cells[c.X,c.Y].Key==null,"node reservation");
            for(int x=4;x<Width-4;x++)for(int y=4;y<Math.Min(Height-4,WallStartY[x]);y++)
                if(!AllowsWall(x,y))Check(Cells[x,y].Wall==null,"surface wall fringe");
            if(LayoutVersion>=2)for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++)
                if(Cells[x,y].Key=="rib"&&(y>=Ground[x]+10||AllowsWall(x,y)))Check(Cells[x,y].Wall!=null,"buried rib backing");
            bool[,] reachable=Reachable();Check(reachable[Exit.X,Exit.Y],"mouth to deep outlet");
            foreach(Point c in Chests)Check(reachable[c.X,c.Y-2],"cache access");
            foreach(Point c in NodeSites)Check(reachable[c.X,c.Y-1],"node pocket access");
            if(LayoutVersion>=2) {
                Check(Teeth.Contains(LowerRejoinHazard),"lower rejoin hazard");
                Check(reachable[LowerRejoinEntrance.X,LowerRejoinEntrance.Y]&&reachable[LowerRejoinExit.X,LowerRejoinExit.Y],"lower rejoin junctions");
                Check(ReachableBranch()[LowerRejoinExit.X,LowerRejoinExit.Y],"branch-only lower rejoin");
            }
            if(LayoutVersion==3)for(int x=Math.Max(4,Center[32]-90);x<=Math.Min(Width-5,Center[32]+90);x++)
                for(int y=4;y<Ground[x];y++)Check(Cells[x,y].Write&&Cells[x,y].Wall==null,"full-column surface clearance");
        }
    }
}
