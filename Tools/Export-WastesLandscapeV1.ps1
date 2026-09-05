param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class WastesLandscapeExport
{
    const int Width = 2048, Height = 1280, Overlap = 124, GroundStart = 628;
    static double Difference(Color a, Color b)
    {
        if (a.A != b.A) return 1000000;
        if (a.A == 0) return 0;
        return (a.R-b.R)*(a.R-b.R) + (a.G-b.G)*(a.G-b.G) + (a.B-b.B)*(a.B-b.B);
    }

    // Minimum-error connected cuts choose existing source pixels. Unlike an
    // ordered dither or alpha blend, they never introduce a checkerboard band.
    static int[] Cut(int across, int steps, Func<int,int,double> cost)
    {
        var previous = new double[across];
        var parents = new int[steps, across];
        for (int step = 0; step < steps; step++)
        {
            var next = new double[across];
            for (int x = 0; x < across; x++)
            {
                int best = x;
                for (int p = Math.Max(0,x-1); p <= Math.Min(across-1,x+1); p++)
                    if (previous[p] < previous[best]) best = p;
                parents[step,x] = best;
                next[x] = previous[best] + cost(x,step) + Math.Abs(x - across/2)*.01;
            }
            previous = next;
        }
        int end = 0;
        for (int x = 1; x < across; x++) if (previous[x] < previous[end]) end = x;
        var path = new int[steps];
        for (int step = steps-1; step >= 0; step--) { path[step] = end; end = parents[step,end]; }
        return path;
    }

    static Color Key(Color pixel)
    {
        // The user authorized local matte processing. Detect the deliberately
        // disjoint magenta key, including mixed boundary pixels, not grey rock.
        if (pixel.R - pixel.G > 18 && pixel.B - pixel.G > 18)
            return Color.Transparent;
        return Color.FromArgb(255, pixel.R, pixel.G, pixel.B);
    }

    static Color ReadLayer(Bitmap image, int x, int y, bool middle)
    {
        // Give the bridge a broken left end inside the repeat rather than
        // splicing a cut-off roadway directly into the station at the next copy.
        int brokenEnd = 154 + ((y / 6 * 7) % 17);
        if (middle && y < 414 && x < brokenEnd) return Color.Transparent;
        return Key(image.GetPixel(x, y));
    }

    static Color WrapLayer(Bitmap image, int x, int y, bool middle, int[] cut)
    {
        Color left = ReadLayer(image, x, y, middle);
        if (x >= Overlap) return left;
        Color right = ReadLayer(image, Width + x, y, middle);
        return x >= cut[y] ? left : right;
    }

    static Color WrapGround(Bitmap image, int x, int y, bool far, int[] cut)
    {
        Color color = image.GetPixel(x, y);
        if (x < Overlap && x < cut[y])
            color = image.GetPixel(Width + x, y);
        if (!far) return Color.FromArgb(255, color.R, color.G, color.B);
        int value = (color.R * 3 + color.G * 5 + color.B * 2) / 10;
        return Color.FromArgb(255, value, value, Math.Min(255, value + 5));
    }

    public static void Export(string sourceRoot, string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        using (var ground = new Bitmap(Path.Combine(sourceRoot, "Ground-Source.png")))
        {
            if (ground.Width != Width + Overlap || ground.Height < 724)
                throw new InvalidDataException("Ground dimensions differ from the measured source contract.");
            int[] groundCut = Cut(Overlap, ground.Height, (x,y) => Difference(ground.GetPixel(x,y), ground.GetPixel(Width+x,y)));
            foreach (string name in new[] {"Far", "Mid", "Close"})
            using (var source = new Bitmap(Path.Combine(sourceRoot, name + "-Matte.png")))
            using (var output = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                if (source.Width != Width + Overlap || source.Height != 724)
                    throw new InvalidDataException("Refuse to rescale changed source: " + name);
                int[] sourceCut = Cut(Overlap, source.Height, (x,y) => Difference(ReadLayer(source,x,y,name == "Mid"), ReadLayer(source,Width+x,y,name == "Mid")));
                int[] bottomCut = Cut(96, Width, (y,x) => Difference(
                    WrapLayer(source,x,GroundStart+y,name == "Mid",sourceCut),
                    WrapGround(ground,x,64+y,name == "Far",groundCut)));
                for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Color pixel;
                    if (y < GroundStart) pixel = WrapLayer(source, x, y, name == "Mid",sourceCut);
                    else
                    {
                        // 652 rows of separately authored lower strata, kept at
                        // native scale. No repeated last row or enlarged source.
                        pixel = WrapGround(ground, x, 64 + y - GroundStart, name == "Far",groundCut);
                        if (y < GroundStart + bottomCut[x])
                            pixel = WrapLayer(source, x, y, name == "Mid",sourceCut);
                    }
                    output.SetPixel(x, y, pixel);
                }
                // Edge equality is only a static guard; the complete overlap
                // and adjacent landmarks still require a panning render check.
                for (int y = 0; y < Height; y++) output.SetPixel(0, y, output.GetPixel(Width - 1, y));
                output.Save(Path.Combine(outputRoot, name + ".png"), ImageFormat.Png);
            }
        }
    }
}
'@
$root = Split-Path -Parent $PSScriptRoot
$sources = Join-Path $root 'Art/Source/Backgrounds/WastesV1'
$destination = Join-Path $root 'Content/Backgrounds/Candidates/WastesV1'
[WastesLandscapeExport]::Export($sources, $destination)
Write-Host 'Exported Wastes V1: 3 x 2048x1280, 30 MiB raw RGBA, no source enlargement. Live validation required.'
