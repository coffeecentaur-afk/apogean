param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$MaskPath,
    [Parameter(Mandatory)][string]$OutputPath,
    [ValidatePattern('^[0-9a-fA-F]{64}$')][string]$SourceSHA256,
    [ValidatePattern('^[0-9a-fA-F]{64}$')][string]$MaskSHA256
)
# Explicit hard-mask export for pixel-art candidates. No segmentation or recoloring.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$source=(Resolve-Path -LiteralPath $SourcePath).Path
$mask=(Resolve-Path -LiteralPath $MaskPath).Path
$output=[IO.Path]::GetFullPath($OutputPath)
$reportPath=$output+'.report.json'
if([IO.Path]::GetExtension($output) -ine '.png'){throw 'OUTPUT_MUST_BE_PNG'}
if(Test-Path -LiteralPath $output){throw 'OUTPUT_EXISTS'}
if(Test-Path -LiteralPath $reportPath){throw 'REPORT_EXISTS'}
if(-not (Test-Path -LiteralPath ([IO.Path]::GetDirectoryName($output)) -PathType Container)){throw 'OUTPUT_DIRECTORY_MISSING'}
$sourceHash=(Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$maskHash=(Get-FileHash -LiteralPath $mask -Algorithm SHA256).Hash
if($SourceSHA256 -and $sourceHash -ine $SourceSHA256){throw 'SOURCE_HASH_MISMATCH'}
if($MaskSHA256 -and $maskHash -ine $MaskSHA256){throw 'MASK_HASH_MISMATCH'}
Add-Type -AssemblyName System.Drawing
$references=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$references+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
public static class HardMaskExport {
    public static long[] Run(string source, string mask, string output) {
        using(var art=new Bitmap(source)) using(var selection=new Bitmap(mask)) {
            if(art.RawFormat.Guid!=ImageFormat.Png.Guid || selection.RawFormat.Guid!=ImageFormat.Png.Guid)
                throw new InvalidDataException("INPUT_MUST_BE_PNG");
            if(art.Width!=selection.Width || art.Height!=selection.Height)
                throw new InvalidDataException("MASK_DIMENSIONS_MISMATCH");
            if((long)art.Width*art.Height>32000000) throw new InvalidDataException("CANVAS_TOO_LARGE");
            long kept=0,removed=0;
            using(var result=new Bitmap(art.Width,art.Height,PixelFormat.Format32bppArgb)) {
                for(int y=0;y<art.Height;y++) for(int x=0;x<art.Width;x++) {
                    Color m=selection.GetPixel(x,y), c=art.GetPixel(x,y);
                    if(m.A!=255 || m.R!=m.G || m.G!=m.B || (m.R!=0 && m.R!=255))
                        throw new InvalidDataException("MASK_NOT_BINARY_OPAQUE at "+x+","+y);
                    if(m.R==255) {
                        if(c.A!=255) throw new InvalidDataException("KEPT_SOURCE_NOT_OPAQUE at "+x+","+y);
                        result.SetPixel(x,y,c); kept++;
                    } else {
                        result.SetPixel(x,y,Color.FromArgb(0,0,0,0)); removed++;
                    }
                }
                if(kept==0) throw new InvalidDataException("EMPTY_ART_MASK");
                if(removed==0) throw new InvalidDataException("NO_TRANSPARENT_REGION");
                // Encode and independently read back before any output write.
                using(var bytes=new MemoryStream()) {
                    result.Save(bytes,ImageFormat.Png); bytes.Position=0;
                    using(var decoded=new Bitmap(bytes)) {
                        for(int y=0;y<art.Height;y++) for(int x=0;x<art.Width;x++) {
                            int expected=selection.GetPixel(x,y).R==255?art.GetPixel(x,y).ToArgb():0;
                            if(decoded.GetPixel(x,y).ToArgb()!=expected)
                                throw new InvalidDataException("PNG_ROUNDTRIP_CHANGED_PIXELS");
                        }
                    }
                    using(var destination=new FileStream(output,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                        bytes.WriteTo(destination);
                }
            }
            return new long[]{art.Width,art.Height,kept,removed};
        }
    }
}
'@
$metrics=[HardMaskExport]::Run($source,$mask,$output)
$report=[ordered]@{
    schemaVersion=1; width=$metrics[0]; height=$metrics[1]
    keptPixels=$metrics[2]; transparentPixels=$metrics[3]; partialAlphaPixels=0
    sourceSHA256=$sourceHash; maskSHA256=$maskHash
    outputSHA256=(Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
    keptPixelsUnchanged=$true; transparentRGBZero=$true; pngRoundtripVerified=$true
    pass=$true
    scope='Exact hard-mask export only. Mask semantics, edge color, artwork, runtime dimensions, seams and live rendering require separate review.'
} | ConvertTo-Json -Depth 3
$reportStream=[IO.File]::Open($reportPath,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
try {
    $bytes=[Text.UTF8Encoding]::new($false).GetBytes($report)
    $reportStream.Write($bytes,0,$bytes.Length)
} finally { $reportStream.Dispose() }
Write-Output $report
