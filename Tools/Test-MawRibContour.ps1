param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawRibContour.cs')
$test=@'
namespace apogean.Content.Diagnostics {
    using System;
    public static class RibContourChecks {
        static int checks;
        static void Check(bool b,string why) { if(!b)throw new Exception("RIB_CONTOUR: "+why);checks++; }
        // Independent pixel-center occupancy for the four vanilla triangles.
        // A horizontal reflection swaps 1/2 and 3/4; solid is unchanged.
        static bool Solid(byte slope,int x,int y) => slope switch {
            0=>true,1=>y>=x,2=>y>=15-x,3=>y<=15-x,4=>y<=x,_=>false
        };
        static bool[,] Pixels(MawRibContour.Cell[] cells,int length) {
            var p=new bool[length*16,16*16];
            foreach(var c in cells) {
                Check(c.X>=0&&c.X<length&&c.Y>=0&&c.Y<16&&c.Slope<=4,"bounded cell");
                for(int x=0;x<16;x++)for(int y=0;y<16;y++)
                    if(Solid(c.Slope,x,y)) { Check(!p[c.X*16+x,c.Y*16+y],"unique pixels");p[c.X*16+x,c.Y*16+y]=true; }
            }
            return p;
        }
        static bool Continuous(bool[,] p) {
            int oldTop=-1,oldBottom=-1;
            for(int x=0;x<p.GetLength(0);x++) {
                int top=-1,bottom=-1;
                for(int y=0;y<p.GetLength(1);y++)if(p[x,y]) { if(top<0)top=y;bottom=y; }
                if(top<0)return false;
                for(int y=top;y<=bottom;y++)if(!p[x,y])return false;
                if(x>0&&(Math.Abs(top-oldTop)>1||Math.Abs(bottom-oldBottom)>1||bottom<oldBottom))return false;
                oldTop=top;oldBottom=bottom;
            }
            return true;
        }
        public static string Run() {
            foreach(int length in new[]{8,11,12})
                Check(!Continuous(Pixels(MawRibContour.Legacy(length,1),length)),"old underside notch must remain RED "+length);
            for(int length=8;length<=16;length++) {
                var right=MawRibContour.Create(length,1);var left=MawRibContour.Create(length,-1);
                var rp=Pixels(right,length);var lp=Pixels(left,length);
                Check(Continuous(rp),"unbroken tapered contour "+length);
                for(int x=0;x<length*16;x++)for(int y=0;y<256;y++)
                    Check(rp[x,y]==lp[length*16-1-x,y],"exact mirrored native triangles");
                for(int x=0;x<32;x++)for(int y=0;y<64;y++)Check(rp[x,y],"four-tile rooted insertion");
                int tip=0;for(int y=0;y<256;y++)if(rp[length*16-1,y])tip++;
                Check(tip<=2,"pointed tip, no square cap");
                // Break one interior cell: oracle must catch the gap, not simply accept any connected bounding box.
                var broken=(bool[,])rp.Clone();for(int y=0;y<256;y++)broken[40,y]=false;
                Check(!Continuous(broken),"missing slice negative control");
            }
            foreach(int length in new[]{-1,0,7,17,int.MaxValue}) {
                bool rejected=false;try{MawRibContour.Create(length,1);}catch(ArgumentOutOfRangeException){rejected=true;}
                Check(rejected,"finite length contract");
            }
            foreach(int dir in new[]{-2,0,2}) {
                bool rejected=false;try{MawRibContour.Create(11,dir);}catch(ArgumentOutOfRangeException){rejected=true;}
                Check(rejected,"direction contract");
            }
            return "PASS "+checks+" rib contour/pixel/mirror checks; old notches rejected. Not a native render or collision verdict.";
        }
    }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$test)
[apogean.Content.Diagnostics.RibContourChecks]::Run()
