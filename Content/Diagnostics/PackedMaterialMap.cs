using System;
using System.IO;

namespace apogean.Content.Diagnostics
{
    // Draw-only lookup: vanilla still owns tile state, framing and geometry.
    internal sealed class PackedMaterialMap
    {
        internal readonly int Width, Height, Columns, Rows, Body, Pitch, Masks;
        private readonly int[] frames;

        internal PackedMaterialMap(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes, false);
            using var reader = new BinaryReader(stream);
            if (reader.ReadInt32() != 0x4D415750) throw new InvalidDataException("PACKED_MAGIC");
            Width = reader.ReadInt32(); Height = reader.ReadInt32();
            Columns = reader.ReadInt32(); Rows = reader.ReadInt32();
            Body = reader.ReadInt32(); Pitch = reader.ReadInt32(); Masks = reader.ReadInt32();
            if ((Body != 16 && Body != 32) || Pitch != Body + 2 ||
                Columns < 1 || Columns > 64 || Rows < 1 || Rows > 128 ||
                Masks < 1 || Masks > 1760 || (Width / Pitch != 64 && Width / Pitch != 128 && Width / Pitch != 256) ||
                Width % Pitch != 0 || Width > 8192 || Height != (Masks * 64 + Width / Pitch - 1) / (Width / Pitch) * Pitch || Height > 8192 ||
                stream.Length != 32L + Columns * Rows * 4L)
                throw new InvalidDataException("PACKED_DIMENSIONS");
            frames = new int[Columns * Rows];
            for (int k = 0; k < frames.Length; k++) {
                frames[k] = reader.ReadInt32();
                if (frames[k] < 0 || frames[k] >= Masks) throw new InvalidDataException("PACKED_INDEX");
            }
        }

        internal bool TryMap(int i, int j, int nativeX, int nativeY, out short x, out short y)
        {
            x = y = 0;
            int nativePitch = Body == 32 ? 36 : 18;
            if (i < 0 || j < 0 || nativeX < 0 || nativeY < 0 ||
                nativeX % nativePitch != 0 || nativeY % nativePitch != 0 ||
                nativeX / nativePitch >= Columns || nativeY / nativePitch >= Rows) return false;
            int mask = frames[nativeY / nativePitch * Columns + nativeX / nativePitch];
            int phase = i % 8 + j % 8 * 8;
            int slot = mask * 64 + phase, across = Width / Pitch;
            x = (short)(slot % across * Pitch); y = (short)(slot / across * Pitch);
            return true;
        }
    }
}
