using System;
using System.Collections.Generic;
using System.Linq;

namespace apogean.Content.Diagnostics
{
    // Drawing-led, fixed QA plan. No assets, world access or update-time generation.
    internal sealed class MawSketchPlan
    {
        internal const int Width = 200, Height = 136, Version = 1;
        internal readonly record struct Point(int X, int Y);
        internal readonly record struct Cell(string Key = null, string Wall = null, byte Slope = 0, bool Side = false);
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
        internal Cell[,] Cells { get; } = new Cell[Width, Height];
        internal List<Tooth> Teeth { get; } = new();
        internal List<Strand> Fibers { get; } = new();
        internal Point[] Chests { get; } = { new(120, 63), new(178, 114) };
        internal Point Entrance { get; private set; }
        internal Point Exit => new(40, 124);
        internal Rib[] Ribs { get; } = {
            new(new(59,28),new(77,29),new[]{new Point(49,25),new Point(50,30),new Point(53,35)}),
            new(new(95,43),new(86,30),new[]{new Point(106,38),new Point(110,43),new Point(105,49)}),
            new(new(36,64),new(57,61),new[]{new Point(23,54),new Point(20,63),new Point(27,70)}),
            new(new(84,76),new(78,58),new[]{new Point(91,86),new Point(88,92),new Point(96,87)}),
            new(new(26,96),new(51,88),new[]{new Point(13,89),new Point(10,96),new Point(16,103)}),
            new(new(67,111),new(62,96),new[]{new Point(76,121),new Point(84,119),new Point(80,113)})
        };
        private static readonly Point[] Surface = {
            new(4,48),new(20,46),new(35,41),new(48,33),new(57,24),new(66,22),
            new(78,24),new(94,22),new(108,21),new(122,25),new(149,29),new(170,28),new(196,26)
        };
        private static readonly Point[] Mouth = {
            new(64,4),new(100,4),new(100,27),new(94,34),new(95,46),new(93,55),
            new(87,62),new(91,74),new(83,86),new(76,97),new(66,108),new(60,120),
            new(61,132),new(5,132),new(12,124),new(17,113),new(22,102),new(27,94),
            new(28,84),new(32,73),new(34,65),new(37,57),new(44,47),new(51,43),new(56,35),new(58,29)
        };
        private static readonly Point[] Cave = {
            new(114,58),new(124,54),new(140,55),new(151,57),new(158,62),new(159,69),
            new(166,76),new(168,82),new(161,87),new(147,84),new(141,76),new(131,70),new(117,67),new(112,62)
        };
        private MawSketchPlan() { Build(); Validate(); }
        internal static MawSketchPlan Create() => new();
        private static int Ground(int x) {
            for(int i=1;i<Surface.Length;i++) if(x<=Surface[i].X) {
                Point a=Surface[i-1],b=Surface[i];return a.Y+(x-a.X)*(b.Y-a.Y)/(b.X-a.X);
            }
            return Surface[^1].Y;
        }
        private static bool Inside(double x,double y,Point[] poly) {
            bool inside=false;
            for(int i=0,j=poly.Length-1;i<poly.Length;j=i++) {
                Point a=poly[i],b=poly[j];
                if((a.Y>y)!=(b.Y>y)&&x<(double)(b.X-a.X)*(y-a.Y)/(b.Y-a.Y)+a.X)inside=!inside;
            }
            return inside;
        }
        private static double Distance(double x,double y,Point a,Point b,out double t) {
            double dx=b.X-a.X,dy=b.Y-a.Y;
            t=Math.Clamp(((x-a.X)*dx+(y-a.Y)*dy)/(dx*dx+dy*dy),0,1);
            return Math.Sqrt(Math.Pow(x-a.X-t*dx,2)+Math.Pow(y-a.Y-t*dy,2));
        }
        private void CarvePath(Point[] path,double radius) {
            for(int i=1;i<path.Length;i++) for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++)
                if(Distance(x+.5,y+.5,path[i-1],path[i],out _)<=radius)
                    Cells[x,y]=new(null,"stone",0,true);
        }
        private void Build() {
            for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++) {
                int ground=Ground(x);
                if(y>=ground) {
                    string key=y<ground+2?"cap":y<ground+4?"grass":y<ground+10?"soil":"stone";
                    Cells[x,y]=new(key,y<ground+5?"soil":"stone");
                }
                // Four-row geological facets accept the retained native four-cell
                // tooth anchors without invisible support blocks inside the throat.
                if(Inside(x+.5,y/4*4+2,Mouth))Cells[x,y]=new(null,y<ground+3?null:"stone");
                if(Inside(x+.5,y+.5,Cave))Cells[x,y]=new(null,"stone",0,true);
            }
            CarvePath(new[]{new Point(88,64),new Point(97,72),new Point(103,81),new Point(114,83),new Point(128,80),new Point(141,77)},3.4);
            CarvePath(new[]{new Point(160,79),new Point(170,87),new Point(173,98),new Point(168,106),new Point(155,115),new Point(150,124)},3.4);
            // Tiny discovery pocket off the winding secondary passage.
            CarvePath(new[]{new Point(170,100),new Point(180,105),new Point(184,113)},3.2);
            for(int x=173;x<190;x++)for(int y=108;y<119;y++)
                if(Math.Pow((x-181.0)/9,2)+Math.Pow((y-113.0)/6,2)<1)Cells[x,y]=new(null,"stone",0,true);

            // Fiber wraps the actual cave boundary. No full-width decorative overlay.
            var geology=(Cell[,])Cells.Clone();
            for(int x=5;x<Width-5;x++)for(int y=5;y<Height-5;y++) {
                if(geology[x,y].Key==null||geology[x,y].Key=="cap")continue;
                bool edge=false,side=false;
                foreach(Point d in new[]{new Point(-1,0),new Point(1,0),new Point(0,-1),new Point(0,1)}) {
                    var neighbor=geology[x+d.X,y+d.Y];
                    if(neighbor.Key==null&&neighbor.Wall!=null){edge=true;side|=neighbor.Side;}
                }
                if(edge)Cells[x,y]=new("grass","grass",0,side);
            }
            // Each root is the same material as its shaft, with three buried offshoots.
            var bone=new bool[Width,Height];
            void Stroke(Point a,Point b,double ra,double rb) {
                for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++) {
                    double distance=Distance(x,y,a,b,out double t);
                    if(distance<=ra+(rb-ra)*t)bone[x,y]=true;
                }
            }
            foreach(Rib r in Ribs) {
                foreach(Point root in r.Roots)Stroke(root,r.Root,.65,2.4);
                Point bend=new((r.Root.X+r.Tip.X)/2,(r.Root.Y+r.Tip.Y)/2+3);
                Stroke(r.Root,bend,2.7,2.1);Stroke(bend,r.Tip,2.1,.65);
            }
            for(int x=4;x<Width-4;x++)for(int y=4;y<Height-4;y++)if(bone[x,y]) {
                bool l=bone[x-1,y],r=bone[x+1,y],u=bone[x,y-1],d=bone[x,y+1];
                byte slope=!u&&d&&l&&!r?(byte)1:!u&&d&&r&&!l?(byte)2:
                    !d&&u&&l&&!r?(byte)3:!d&&u&&r&&!l?(byte)4:(byte)0;
                Cells[x,y]=new("rib",Cells[x,y].Wall,slope,Cells[x,y].Side);
            }

            // Actual chest cavities: two-cell feet plus three rows of clearance, inside side caves.
            foreach(Point chest in Chests) {
                for(int x=chest.X-2;x<chest.X+5;x++)for(int y=chest.Y-3;y<chest.Y+2;y++)Cells[x,y]=new(null,"grass",0,true);
                for(int x=chest.X-2;x<chest.X+5;x++)Cells[x,chest.Y+2]=new("grass","grass",0,true);
            }
            // Amber stays in side-path margins, NOT luminous ribs/main-shaft beacons.
            foreach(Point organ in new[]{new Point(99,70),new Point(127,54),new Point(171,95),new Point(184,120)})
                for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++) {
                    int x=organ.X+dx,y=organ.Y+dy;
                    Cells[x,y]=new(Cells[x,y].Key==null?null:"amber","amber",0,true);
                }
            // Choose actual four-cell flat supports from this terrain; never manufacture anchor pads.
            bool[,] occupied=new bool[Width,Height];
            bool ChestNear(int x,int y)=>Chests.Any(c=>x>=c.X-3&&x<=c.X+4&&y>=c.Y-4&&y<=c.Y+3);
            for(int y=12;y<Height-8;y++)for(int x=6;x<Width-8;x++) {
                if(Cells[x,y].Key!=null||ChestNear(x,y))continue;
                for(int face=0;face<4;face++) {
                    var t=new Tooth(new(x,y),face+(Teeth.Count%2)*4);bool valid=true;
                    for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++)
                        valid &= Cells[x+dx,y+dy].Key==null&&!occupied[x+dx,y+dy]&&!ChestNear(x+dx,y+dy);
                    for(int i=0;i<4;i++) {Point a=t.Support(i);valid&=Cells[a.X,a.Y].Key!=null&&Cells[a.X,a.Y].Key!="rib"&&Cells[a.X,a.Y].Slope==0;}
                    // Dense throat/crown, lighter side passages; preserve an open player route.
                    if(x>101&&((x+y)%9!=0||!Inside(x+2,y+2,Cave)))valid=false;
                    if(!valid)continue;
                    Teeth.Add(t);
                    for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++)occupied[x+dx,y+dy]=true;
                    break;
                }
            }
            for(int x=105;x<Width-7;x+=5)for(int y=45;y<Height-10;y++) {
                if(Cells[x,y].Key!="grass"||!Cells[x,y+1].Side)continue;
                int length=2+(x/5)%5;bool valid=true;
                for(int d=1;d<=length;d++)valid&=Cells[x,y+d].Key==null&&!occupied[x,y+d]&&!ChestNear(x,y+d);
                if(!valid)continue;
                Fibers.Add(new(new(x,y),length));break;
            }
            // Standing surface entry outside the hazardous crown; no new safety shelf or supplied rope.
            Entrance=new(30,Ground(30)-3);
        }
        internal bool[,] Reachable() {
            var blocked=new bool[Width,Height];
            foreach(Tooth t in Teeth)for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++)blocked[t.Root.X+dx,t.Root.Y+dy]=true;
            bool Fits(int x,int y) {
                if(x<4||y<4||x+2>=Width-4||y+3>=Height-4)return false;
                for(int dx=0;dx<2;dx++)for(int dy=0;dy<3;dy++)if(Cells[x+dx,y+dy].Key!=null||blocked[x+dx,y+dy])return false;
                return true;
            }
            var seen=new bool[Width,Height];var q=new Queue<Point>();
            void Visit(int x,int y){if(!Fits(x,y)||seen[x,y])return;seen[x,y]=true;q.Enqueue(new(x,y));}
            Visit(Entrance.X,Entrance.Y);
            while(q.Count>0){Point p=q.Dequeue();Visit(p.X-1,p.Y);Visit(p.X+1,p.Y);Visit(p.X,p.Y-1);Visit(p.X,p.Y+1);}
            return seen;
        }
        internal void Validate() {
            void Check(bool ok,string why){if(!ok)throw new InvalidOperationException("MAW_SKETCH: "+why);}
            Check(Teeth.Count>=20&&Teeth.Count<=100,"bounded dense tooth family: "+Teeth.Count);
            Check(Fibers.Count>=5&&Fibers.Count<=30,"hanging side fiber");
            foreach(Cell c in Cells){Check(c.Key is null or "soil" or "grass" or "cap" or "stone" or "rib" or "amber","material/no dedicated join");Check(c.Slope<=4,"native slopes");if(c.Key=="amber")Check(c.Side,"side-path amber");}
            foreach(Rib r in Ribs){
                Check(Cells[r.Root.X,r.Root.Y].Key=="rib","root union");
                var connected=new HashSet<Point>();var queue=new Queue<Point>();queue.Enqueue(r.Root);
                while(queue.Count>0){
                    Point at=queue.Dequeue();
                    if(at.X<0||at.Y<0||at.X>=Width||at.Y>=Height||Cells[at.X,at.Y].Key!="rib"||!connected.Add(at))continue;
                    queue.Enqueue(new(at.X-1,at.Y));queue.Enqueue(new(at.X+1,at.Y));queue.Enqueue(new(at.X,at.Y-1));queue.Enqueue(new(at.X,at.Y+1));
                }
                Check(connected.Contains(r.Tip),"continuous rib tip");
                foreach(Point root in r.Roots)Check(connected.Contains(root),"buried root connected to shaft");
            }
            foreach(Point c in Chests) {Check(Cells[c.X,c.Y].Side&&Cells[c.X,c.Y].Key==null,"side chest");Check(Cells[c.X,c.Y+2].Key!=null&&Cells[c.X+1,c.Y+2].Key!=null,"chest feet");}
            bool[,] reach=Reachable();Check(reach[Exit.X,Exit.Y],"main route clearance");
            foreach(Point c in Chests)Check(reach[c.X,c.Y-2],"side cave route clearance "+c);
        }
    }
}
