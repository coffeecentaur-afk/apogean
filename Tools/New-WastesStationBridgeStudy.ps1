param([Parameter(Mandatory)][string]$OutputDirectory)
# One pinned, upper-only offline style study. Not a runtime importer.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$pins=@(
    @('Art/Candidates/WastesStationBridge-v1/Selected/Station-Transparent.png','E1E6FF649F83A4997CAEF37A4065A576E115A15B4E959235362C7EAFAF597C30'),
    @('Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3/Station-Upper.png','83C8EDE6580E6CEB2CEECD2C0F24078EFCADE3EB10680C8619BC3B1D9207C59B'),
    @('Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1/MotorDepot-Upper.png','AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890'))
foreach($p in $pins){if((Get-FileHash -LiteralPath (Join-Path $root $p[0])).Hash -ne $p[1]){throw 'SOURCE_CHANGED'}}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
public static class StationBridgeStudy {
    public static void Make(string source,string old,string garage,string output) {
        using(var art=new Bitmap(source))using(var a=new Bitmap(old))using(var b=new Bitmap(garage))
        using(var fit=new Bitmap(512,460,PixelFormat.Format32bppArgb)) {
            if(art.Width!=1323||art.Height!=1189||a.Width!=512||a.Height!=460||b.Width!=512||b.Height!=460)
                throw new InvalidDataException("SOURCE_DIMENSIONS");
            // Explicit nearest-center 3/8 study fit. Soil around source y800 -> y340.
            // Generated output did not honor the requested canvas/grid; never call this lossless.
            for(int y=0;y<460;y++)for(int x=0;x<512;x++) {
                int sx=(int)Math.Floor((x+0.5)*8/3),sy=(int)Math.Floor((y-40+0.5)*8/3);
                Color c=sx<art.Width&&sy>=0&&sy<art.Height?art.GetPixel(sx,sy):Color.FromArgb(0,0,0,0);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("SOFT_ALPHA");
                fit.SetPixel(x,y,c.A==0?Color.FromArgb(0,0,0,0):c);
            }
            Directory.CreateDirectory(output);
            fit.Save(Path.Combine(output,"Station-Upper.png"),ImageFormat.Png);
            foreach(bool dark in new[]{false,true})using(var board=new Bitmap(1536,553,PixelFormat.Format32bppArgb))
            using(var g=Graphics.FromImage(board))using(var font=new Font("Consolas",11))
            using(var ink=new SolidBrush(dark?Color.Gainsboro:Color.FromArgb(25,25,30))) {
                g.Clear(dark?Color.FromArgb(40,37,42):Color.FromArgb(151,185,201));
                g.TextRenderingHint=TextRenderingHint.SingleBitPerPixelGridFit;
                g.DrawString("OFFLINE 1:1 ASSET PIXELS - comparable ground scale, not a game screenshot",font,ink,12,8);
                g.DrawString("Original station - unchanged",font,ink,12,32);
                g.DrawString("New station - style-bridge candidate",font,ink,524,32);
                g.DrawString("Garage - unchanged",font,ink,1036,32);
                g.DrawImageUnscaled(a,0,60);g.DrawImageUnscaled(fit,512,60);g.DrawImageUnscaled(b,1024,60);
                g.DrawString("Soil datum about y340. New source fit 3/8 nearest-center, +40y. Upper art only; deep foundations and native test still required.",font,ink,12,534);
                board.Save(Path.Combine(output,dark?"Comparison-Dark.png":"Comparison-Light.png"),ImageFormat.Png);
            }
        }
    }
}
'@
[StationBridgeStudy]::Make((Join-Path $root $pins[0][0]),(Join-Path $root $pins[1][0]),(Join-Path $root $pins[2][0]),$output)
[ordered]@{
    recipe='StationBridgeStudy-v1';width=512;height=460;sourceWidth=1323;sourceHeight=1189
    sourceSHA256=$pins[0][1];referenceStationSHA256=$pins[1][1];referenceGarageSHA256=$pins[2][1]
    scaleNumerator=3;scaleDenominator=8;offsetY=40;approximateSoilY=340
    candidateSHA256=(Get-FileHash -LiteralPath (Join-Path $output 'Station-Upper.png')).Hash
    scope='Offline upper-only art review; nearest-center resample, not lossless export or runtime-ready foundation. Original references untouched. No game installation or visual acceptance.'
}|ConvertTo-Json -Depth 3|Set-Content -LiteralPath (Join-Path $output 'study.json')
Write-Output "Offline study: $output"
