using System;
using System.IO;

namespace apogean.Content.Diagnostics
{
    // Small CPU lookup compiled from the exact pinned atlas. No texture readback,
    // per-frame allocation, generated glow sprite, or change to approved pixels.
    internal sealed class PackedEmissionMap
    {
        private readonly int width,height,pitch;
        private readonly ushort[] counts;
        internal PackedEmissionMap(byte[] bytes)
        {
            using var stream=new MemoryStream(bytes,false);using var reader=new BinaryReader(stream);
            if(bytes.Length<20||reader.ReadInt32()!=0x4D454D41)throw new InvalidDataException("EMISSION_MAGIC");
            width=reader.ReadInt32();height=reader.ReadInt32();pitch=reader.ReadInt32();int length=reader.ReadInt32();
            if(width<1||height<1||width>8192||height>8192||(pitch!=18&&pitch!=34)||width%pitch!=0||height%pitch!=0||
                length!=width/pitch*(height/pitch)||bytes.Length!=20L+length*2L)throw new InvalidDataException("EMISSION_DIMENSIONS");
            counts=new ushort[length];for(int n=0;n<length;n++){counts[n]=reader.ReadUInt16();if(counts[n]>256)throw new InvalidDataException("EMISSION_COUNT");}
        }
        internal int Count(int x,int y)
        {
            if(x<0||y<0||x>=width||y>=height||x%pitch!=0||y%pitch!=0)return 0;
            return counts[y/pitch*(width/pitch)+x/pitch];
        }
        internal static float Strength(int pixels,bool dormant)
        {
            if(pixels<=0)return 0;
            return (0.25f+0.75f*MathF.Sqrt(Math.Clamp(pixels/80f,0f,1f)))*(dormant?0.32f:1f);
        }
    }
}
