using System;
using System.Collections.Generic;

namespace apogean.Content.Diagnostics
{
    // Geometry-only QA candidate: native whole tiles and four hammer slopes.
    // No custom collision, runtime tessellation, per-frame generation or asset edits.
    internal static class MawRibContour
    {
        internal readonly record struct Cell(int X,int Y,byte Slope,bool Root);
        private static void Validate(int length,int direction)
        {
            if(length<8||length>16)throw new ArgumentOutOfRangeException(nameof(length));
            if(direction is not (1 or -1))throw new ArgumentOutOfRangeException(nameof(direction));
        }
        private static Cell Orient(int d,int y,byte slope,bool root,int length,int direction) =>
            direction==1 ? new(d,y,slope,root) : new(length-1-d,y,slope switch {
                1=>2,2=>1,3=>4,4=>3,_=>(byte)0
            },root);

        internal static Cell[] Create(int length,int direction)
        {
            Validate(length,direction);
            var top=new int[length+1];var bottom=new int[length+1];
            for(int d=0;d<=length;d++) {
                top[d]=d*d/(2*length);
                int thickness=d==length?0:4-3*d/(length-1);
                // The legacy independently rounded thickness stepped the underside
                // inward, then back out. A continuous outer envelope removes that notch.
                bottom[d]=Math.Max(d==0?0:bottom[d-1],top[d]+thickness);
            }
            var result=new List<Cell>();
            for(int d=0;d<length;d++)for(int y=top[d];y<bottom[d+1];y++) {
                // 1 = down-left (solid bottom/left); 4 = up-right (solid top/right).
                // In screen coordinates these are the upper/lower edges falling to the right.
                byte slope=y==top[d]&&top[d+1]>top[d] ? (byte)1 :
                    y==bottom[d]&&bottom[d+1]>bottom[d] ? (byte)4 : (byte)0;
                result.Add(Orient(d,y,slope,d<2,length,direction));
            }
            return result.ToArray();
        }

        // Explicit old-shape control in the new gallery only. Never rewrites shallowV1.
        internal static Cell[] Legacy(int length,int direction)
        {
            Validate(length,direction);var result=new List<Cell>();
            for(int d=0;d<length;d++) {
                int top=d*d/(2*length),thickness=4-3*d/(length-1);
                for(int h=0;h<thickness;h++) {
                    byte slope=d>=2&&h==0&&(d==length-1||(d+1)*(d+1)/(2*length)>top)?(byte)1:(byte)0;
                    result.Add(Orient(d,top+h,slope,d<2,length,direction));
                }
            }
            return result.ToArray();
        }
    }
}
