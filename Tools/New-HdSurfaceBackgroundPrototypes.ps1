param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$exporterSource = @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class ApogeanHdLayerExporter
{
    public static void Export(string sourcePath, string outputPath)
    {
        using var loaded = new Bitmap(sourcePath);
        var rectangle = new Rectangle(0, 0, loaded.Width, loaded.Height);
        using var source = loaded.Clone(rectangle, PixelFormat.Format32bppArgb);
        using var output = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

        BitmapData sourceData = source.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        byte[] bytes;
        try
        {
            if (sourceData.Stride <= 0)
                throw new InvalidOperationException("Unexpected non-positive bitmap stride: " + sourcePath);
            bytes = new byte[sourceData.Stride * source.Height];
            Marshal.Copy(sourceData.Scan0, bytes, 0, bytes.Length);
        }
        finally
        {
            source.UnlockBits(sourceData);
        }

        int stride = source.Width * 4;
        var backdropPalette = new bool[32768];
        int paletteRows = Math.Min(8, source.Height);
        for (int y = 0; y < paletteRows; y++)
        for (int x = 0; x < source.Width; x++)
        {
            int offset = y * stride + x * 4;
            backdropPalette[QuantizedColor(bytes[offset + 2], bytes[offset + 1], bytes[offset])] = true;
        }

        for (int y = 0; y < source.Height; y++)
        {
            int row = y * stride;
            for (int x = 0; x < source.Width; x++)
            {
                int offset = row + x * 4;
                byte sourceAlpha = bytes[offset + 3];
                int key = QuantizedColor(bytes[offset + 2], bytes[offset + 1], bytes[offset]);
                bytes[offset + 3] = sourceAlpha == 0 || backdropPalette[key] ? (byte)0 : (byte)255;
            }
        }

        RemoveBackdropFringe(bytes, source.Width, source.Height, stride);

        for (int y = 0; y < source.Height; y++)
        {
            int row = y * stride;
            int last = row + (source.Width - 1) * 4;
            Buffer.BlockCopy(bytes, row, bytes, last, 4);
        }

        BitmapData outputData = output.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            Marshal.Copy(bytes, 0, outputData.Scan0, bytes.Length);
        }
        finally
        {
            output.UnlockBits(outputData);
        }

        output.Save(outputPath, ImageFormat.Png);
    }

    private static void RemoveBackdropFringe(byte[] bytes, int width, int height, int stride)
    {
        var remove = new bool[width * height];

        // The authored extraction sheets used a pale checkerboard instead of a
        // real alpha channel. Remove only pale, near-neutral pixels touching an
        // already transparent pixel. Three conservative passes strip the white
        // matte without eroding brown branches, masonry, or coloured landmarks.
        for (int pass = 0; pass < 3; pass++)
        {
            Array.Clear(remove, 0, remove.Length);
            bool changed = false;
            for (int y = 1; y < height - 1; y++)
            for (int x = 1; x < width - 1; x++)
            {
                int offset = y * stride + x * 4;
                if (bytes[offset + 3] == 0)
                    continue;

                byte blue = bytes[offset];
                byte green = bytes[offset + 1];
                byte red = bytes[offset + 2];
                int minimum = Math.Min(red, Math.Min(green, blue));
                int maximum = Math.Max(red, Math.Max(green, blue));
                if (minimum < 112 || maximum - minimum > 34)
                    continue;

                bool touchesTransparency =
                    bytes[offset - 4 + 3] == 0 ||
                    bytes[offset + 4 + 3] == 0 ||
                    bytes[offset - stride + 3] == 0 ||
                    bytes[offset + stride + 3] == 0;
                if (!touchesTransparency)
                    continue;

                remove[y * width + x] = true;
                changed = true;
            }

            if (!changed)
                break;

            for (int y = 1; y < height - 1; y++)
            for (int x = 1; x < width - 1; x++)
            {
                if (remove[y * width + x])
                    bytes[y * stride + x * 4 + 3] = 0;
            }
        }
    }

    private static int QuantizedColor(byte red, byte green, byte blue) =>
        (red >> 3) << 10 | (green >> 3) << 5 | (blue >> 3);

}
'@

$drawingDirectory = Split-Path -Parent ([System.Drawing.Bitmap].Assembly.Location)
$drawingReferences = @(
	(Join-Path $drawingDirectory 'System.Drawing.Common.dll'),
	(Join-Path $drawingDirectory 'System.Private.Windows.GdiPlus.dll'),
	(Join-Path $drawingDirectory 'System.Private.Windows.Core.dll'),
	(Join-Path $drawingDirectory 'System.Drawing.Primitives.dll'),
	(Join-Path $drawingDirectory 'System.Drawing.dll')
)
Add-Type -TypeDefinition $exporterSource -ReferencedAssemblies $drawingReferences

$biomes = @('Forest', 'Desert', 'Jungle', 'Snow', 'Corruption', 'Crimson', 'Hallow', 'Ocean', 'Mushroom')
$sourceOverrides = @{
	'Mushroom:Mid' = 'V0-Mid-extraction-v2.png'
	'Mushroom:Close' = 'V0-Close-extraction-v2.png'
}

function Export-HdLayer([string]$biome, [string]$layer) {
	$key = "${biome}:${layer}"
	$sourceName = if ($sourceOverrides.ContainsKey($key)) {
		$sourceOverrides[$key]
	} else {
		"V0-${layer}-extraction-v1.png"
	}
	$sourcePath = Join-Path $Root "Art/Source/Backgrounds/$biome/$sourceName"
	$outputPath = Join-Path $Root "Content/Backgrounds/Diagnostics/HD/${biome}ConceptV0_${layer}.png"
	if (-not (Test-Path -LiteralPath $sourcePath)) {
		throw "Missing authored $biome $layer source: $sourcePath"
	}

	$directory = Split-Path -Parent $outputPath
	if (-not (Test-Path -LiteralPath $directory)) {
		New-Item -ItemType Directory -Path $directory -Force | Out-Null
	}
	[ApogeanHdLayerExporter]::Export($sourcePath, $outputPath)
	$image = [System.Drawing.Image]::FromFile($outputPath)
	try { Write-Host "$biome ${layer}: $($image.Width)x$($image.Height) native-detail export" }
	finally { $image.Dispose() }
}

foreach ($biome in $biomes) {
	foreach ($layer in @('Far', 'Mid', 'Close')) {
		Export-HdLayer $biome $layer
	}
}

$transparentPath = Join-Path $Root 'Content/Backgrounds/Diagnostics/HD/Transparent.png'
$transparent = [System.Drawing.Bitmap]::new(2, 2, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
	$transparent.Save($transparentPath, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally {
	$transparent.Dispose()
}

Write-Host 'Generated nine native-detail V0 surface-background benchmarks.'
