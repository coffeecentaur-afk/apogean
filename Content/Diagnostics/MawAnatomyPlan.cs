namespace apogean.Content.Diagnostics
{
    // Fixed disposable comparison; deliberately not a world generator.
    internal static class MawAnatomyPlan
    {
        internal const int Width=100,Height=76;
        internal readonly record struct Cell(string Key,byte Slope=0,string Wall=null);
        internal static Cell[,] Create()
        {
            var p=new Cell[Width,Height];
            for(int x=4;x<65;x++)for(int y=12;y<68;y++) {
                if(x>=24&&x<45)continue;
                string key=y==12?"cap":y==13?"grass":y<17?"soil":x==23||x==45?"grass":x>=21&&x<24||x>=45&&x<48?"soil":"stone";
                p[x,y]=new(key);
            }
            // The old porous bone remains in the root. The long continuous shaft
            // uses cortical material and native hammered slopes at exposed steps.
            void Rib(int rootX,int rootY,int dir,int[] top,int[] width) {
                for(int d=0;d<top.Length;d++)for(int h=0;h<width[d];h++) {
                    byte slope=0;
                    // Native slope1 descends right; slope2 descends left. Only
                    // the outer upper stair cell is cut, never interior fill.
                    if(d>2&&h==0&&d+1<top.Length&&top[d+1]>top[d])slope=(byte)(dir>0?1:2);
                    if(d==top.Length-1)slope=(byte)(dir>0?1:2);
                    p[rootX+dir*d,rootY+top[d]+h]=new(d<2?"bone":"rib",slope);
                }
            }
            Rib(20,27,1,new[]{0,0,0,0,0,1,1,2,2,3,4,5,6},new[]{4,4,4,4,4,4,4,4,3,3,3,2,1});
            Rib(49,44,-1,new[]{0,0,0,0,1,1,2,3,4},new[]{4,4,4,4,4,4,3,2,1});
            Rib(20,60,1,new[]{0,0,0,1,1,2},new[]{3,3,3,3,2,1});
            // Full cap / rooted transition / plain soil beside the old single
            // grass coat, with identical depth and no backing wall to hide gaps.
            for(int x=72;x<94;x++)for(int y=12;y<23;y++) {
                if(x==82||x==83)continue;
                p[x,y]=new(y==12?(x<82?"cap":"grass"):y==13&&x<82?"grass":"soil");
            }
            // New/old bone mass and actual unsafe wall swatches. Walls are
            // separate from all rib roots and cannot hide their render defects.
            for(int x=72;x<81;x++)for(int y=28;y<33;y++)p[x,y]=new("rib");
            for(int x=84;x<93;x++)for(int y=28;y<33;y++)p[x,y]=new("bone");
            string[] walls={"soil","fibers","bone","membrane","amber","stone"};
            for(int n=0;n<walls.Length;n++)for(int dx=0;dx<9;dx++)for(int dy=0;dy<7;dy++)
                p[72+(n%2)*12+dx,38+(n/2)*11+dy]=new(null,0,walls[n]);
            return p;
        }
    }
}
