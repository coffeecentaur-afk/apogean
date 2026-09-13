using System;
using P = apogean.Content.Diagnostics.MawFiberGrowthPolicy;

namespace apogean.Content.Diagnostics
{
    internal static class MawGrowthLoadPlan
    {
        internal const int Side = 32, Updates = 240, InitialGrass = 4, MatureGrass = 160;
        internal static bool Budget(int value) => value is 0 or 1 or 8 or 32;
        internal static bool Context(string world, string player, bool single, bool menu) =>
            single && !menu && world == "Apogee Native Visual V3" && player == "gg";
        internal static P.Cell[,] Seed()
        {
            var cells = new P.Cell[Side, Side];
            foreach(int left in new[]{2,18}) foreach(int top in new[]{2,18}) {
                for(int x=left;x<left+11;x++) for(int y=top;y<top+11;y++)
                    cells[x,y]=new(P.Host.Soil);
                cells[left+5,top]=new(P.Host.Grass);
            }
            cells[15,15]=new(P.Host.Soil); // Separate unseeded island.
            for(int y=3;y<13;y++) cells[15,y]=new(P.Host.Other); // Structural host control.
            return cells;
        }
        internal static bool IsMatureGrass(int x,int y)
        {
            foreach(int left in new[]{2,18}) foreach(int top in new[]{2,18})
                if(x>=left&&x<left+11&&y>=top&&y<top+11 &&
                    (x==left||x==left+10||y==top||y==top+10)) return true;
            return false;
        }
    }
}
