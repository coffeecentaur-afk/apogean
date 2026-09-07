param([Parameter(Mandatory)][string]$OutputDirectory)
# Deterministic component selection only. Never generates, retouches or installs art.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$art=Join-Path $root 'Art/Candidates/WastesMidRuins-v2'
$building=Join-Path $art 'PixelStyle-v1/Selected/MotorDepot-Upper.png'
$terrain=Join-Path $art 'PixelStyle-v2/GridReview/MotorDepot-Upper.png'
$station=Join-Path $art 'ScaleStudy-v3/Station-Upper.png'
$pins=@(
    @($building,'CA51A9C972F61DCD5E598AB523FD00A50416D5B672A7064CD786A3777D66045B'),
    @($terrain,'8995B665B0895670E8949EFF4CE2F5A8E1B9220771D8C6A1352B68033AF612B4'),
    @($station,'83C8EDE6580E6CEB2CEECD2C0F24078EFCADE3EB10680C8619BC3B1D9207C59B'))
foreach($p in $pins){if((Get-FileHash -LiteralPath $p[0]).Hash -ne $p[1]){throw 'SOURCE_CHANGED'}}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
public static class DepotAssembly {
    public static void Make(string building,string terrain,string station,string output){
        int[] starts={0,112,152,192,240,296,344,400};
        int[] joins={342,344,342,346,344,342,346,344};
        using(var a=new Bitmap(building))using(var b=new Bitmap(terrain))
        using(var result=new Bitmap(512,460,PixelFormat.Format32bppArgb))
        using(var selector=new Bitmap(512,460,PixelFormat.Format32bppArgb)){
            if(a.Width!=512||b.Width!=512||a.Height!=432||b.Height!=432)throw new InvalidDataException("SOURCE_DIMENSIONS");
            for(int x=0;x<512;x++){
                int band=0;while(band+1<starts.Length&&x>=starts[band+1])band++;
                for(int y=0;y<460;y++){
                    // 28 bottom rows are explicitly empty QA padding, NOT a deep foundation.
                    Color c=y>=432?Color.FromArgb(0,0,0,0):(y<joins[band]?a:b).GetPixel(x,y);
                    result.SetPixel(x,y,c.A==0?Color.FromArgb(0,0,0,0):c);
                    selector.SetPixel(x,y,y>=432?Color.Black:y<joins[band]?Color.Red:Color.Blue);
                }
            }
            Directory.CreateDirectory(output);
            result.Save(Path.Combine(output,"MotorDepot-Upper.png"),ImageFormat.Png);
            selector.Save(Path.Combine(output,"Source-Selection.png"),ImageFormat.Png);
            foreach(bool dark in new[]{false,true})using(var board=new Bitmap(1024,535,PixelFormat.Format32bppArgb))
            using(var g=Graphics.FromImage(board))using(var font=new Font("Consolas",11))
            using(var ink=new SolidBrush(dark?Color.Gainsboro:Color.FromArgb(25,25,30)))
            using(var reference=new Bitmap(station)){
                g.Clear(dark?Color.FromArgb(40,37,42):Color.FromArgb(151,185,201));
                g.TextRenderingHint=TextRenderingHint.SingleBitPerPixelGridFit;
                g.DrawString("OFFLINE 1:1 PIXELS - ground-scale assembly, not a game screenshot",font,ink,12,8);
                g.DrawString("Station - unchanged",font,ink,12,32);
                g.DrawString("Preferred building + rougher terrain",font,ink,524,32);
                g.DrawImageUnscaled(reference,0,60);g.DrawImageUnscaled(result,512,60);
                g.DrawString("No repainting. Soil y340. Last 28 rows empty. Full-height foundation NOT authored.",font,ink,12,514);
                board.Save(Path.Combine(output,dark?"Comparison-Dark.png":"Comparison-Light.png"),ImageFormat.Png);
            }
        }
    }
}
'@
[DepotAssembly]::Make($building,$terrain,$station,$output)
[ordered]@{
    recipe='DepotAssembly-v1'; width=512;height=460;soilRow=340;authoredRows=432
    buildingSHA256=$pins[0][1];terrainSHA256=$pins[1][1];stationSHA256=$pins[2][1]
    sourceSelection='Red=v1 building; blue=v2 grid terrain; black=empty QA padding. Direct same-coordinate source pixels; no blend or rescale.'
    joinStartX=@(0,112,152,192,240,296,344,400);joinY=@(342,344,342,346,344,342,346,344)
    candidateSHA256=(Get-FileHash -LiteralPath (Join-Path $output 'MotorDepot-Upper.png')).Hash
    scope='Ground-only disposable gallery. Not a deep module or art approval.'
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $output 'assembly.json')
Write-Output "Assembled without changing either source: $output"
