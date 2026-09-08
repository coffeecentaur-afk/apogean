param(
    [string]$OutputDirectory,
    [ValidateSet('Native-v1','Native-v2')][string]$Variant = 'Native-v1'
)
# One approved source and one inspected mask proposal. Candidate-only, never Content.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$source = Join-Path $repo 'Art/Candidates/ArrivalPod-v1/design-a3-blend.png'
$proposal = Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v1/mask-proposal.png'
$sourceHash = '8A8860FA62D702B3464BC3450443ED0688130E3F4B8192DA8F0F4E6E992511CB'
$proposalHash = '882A0072FEA17E59BB15C00D0AC1AC4A8CE90F4322C0F3A3289D1613B3A6281E'
$capture = ''
$coarse = $Variant -eq 'Native-v2'
if ($coarse) {
    $source = Join-Path $repo 'Art/Candidates/ArrivalPod-v1/A5-ImpactReview/design-a5-impact.png'
    $proposal = Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v2/mask-proposal.png'
    $sourceHash = '3EEC46AFFC7BB149B6A38528AFB93ABB79E27D24E61D97995A5A18768AEB1C67'
    $proposalHash = '962B2BDAB1B4D381E1B8B8F4C4EB26777BE4D57D95751A5B460013F630527BB6'
    $capture = Join-Path $repo 'Art/Validation/ArrivalPod-2026-09-07/inside.png'
    if ((Get-FileHash $capture).Hash -ne '43EF92098AE961F7C25CC06535752FDD4DF3B9DC144469C6B90057ED08EE69F4') { throw 'CAPTURE_HASH_MISMATCH' }
}
if (!$OutputDirectory) { $OutputDirectory = Join-Path $repo "Art/Candidates/ArrivalPod-v1/$Variant" }
if ((Get-FileHash $source).Hash -ne $sourceHash) { throw 'SOURCE_HASH_MISMATCH' }
if ((Get-FileHash $proposal).Hash -ne $proposalHash) { throw 'PROPOSAL_HASH_MISMATCH' }
$out = [IO.Path]::GetFullPath($OutputDirectory)
if ($out.StartsWith((Join-Path $repo 'Content')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or $out -eq (Join-Path $repo 'Content')) { throw 'PRODUCTION_OUTPUT_FORBIDDEN' }
if (!(Test-Path -LiteralPath $out -PathType Container)) { throw 'OUTPUT_DIRECTORY_MISSING' }
$names = @('mask.png','cutout.png','cutout.png.report.json','ArrivalPod.png','ArrivalPod_Tile.png','preview.png','native-report.json')
if ($coarse) { $names += 'context-scale.png' }
foreach ($name in $names) { if (Test-Path -LiteralPath (Join-Path $out $name)) { throw "OUTPUT_EXISTS: $name" } }
Add-Type -AssemblyName System.Drawing
$references = @([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$references += @(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
public static class ArrivalPodNative {
    static void Save(Bitmap bitmap, string path) {
        using(var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            bitmap.Save(stream, ImageFormat.Png);
    }
    public static int PrepareMask(string source, string path) {
        using(var input = new Bitmap(source)) using(var result = new Bitmap(1254,1254)) {
            if(input.Width != 1254 || input.Height != 1254) throw new InvalidDataException("MASK_SIZE");
            int bridges = 0;
            for(int y=0;y<1254;y++) for(int x=0;x<1254;x++) {
                Color p=input.GetPixel(x,y);
                bool selected = p.R+p.G+p.B >= 384;
                // The model misclassified the black hinge hardware as exterior space.
                // Two reviewed source-coordinate strips reconnect the attached hatch.
                bool hinge = (x>=752 && x<=794 && y>=575 && y<=621) ||
                             (x>=769 && x<=807 && y>=783 && y<=835);
                if(hinge && !selected) bridges++;
                result.SetPixel(x,y, selected || hinge ? Color.White : Color.Black);
            }
            Save(result,path); return bridges;
        }
    }
    public static int PrepareImpactMask(string source,string path) {
        using(var input=new Bitmap(source)) using(var result=new Bitmap(1536,1024)) {
            if(input.Width!=1536 || input.Height!=1024) throw new InvalidDataException("MASK_SIZE");
            int w=input.Width,h=input.Height,changed=0;
            bool[] keep=new bool[w*h],outside=new bool[w*h];
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
                Color p=input.GetPixel(x,y);
                keep[y*w+x]=p.R+p.G+p.B>=384;
                // A5-specific source-coordinate hardware. The proposal drew
                // interior ink lines instead of a solid selection. Close these
                // three reviewed joins, then fill only enclosed mask holes.
                bool join=(x>=470 && x<=1005 && y>=716 && y<=739) ||
                          (x>=895 && x<=940 && y>=390 && y<=434) ||
                          (x>=907 && x<=948 && y>=554 && y<=600);
                if(join && !keep[y*w+x]) { keep[y*w+x]=true;changed++; }
            }
            var queue=new Queue<int>();
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
                int i=y*w+x;
                if((x==0 || x==w-1 || y==0 || y==h-1) && !keep[i]) {outside[i]=true;queue.Enqueue(i);}
            }
            int[] dx={-1,1,0,0},dy={0,0,-1,1};
            while(queue.Count>0) {
                int i=queue.Dequeue(),x=i%w,y=i/w;
                for(int n=0;n<4;n++) {
                    int nx=x+dx[n],ny=y+dy[n];
                    if(nx<0 || nx>=w || ny<0 || ny>=h) continue;
                    int next=ny*w+nx;
                    if(!keep[next] && !outside[next]) {outside[next]=true;queue.Enqueue(next);}
                }
            }
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
                int i=y*w+x;
                if(!keep[i] && !outside[i]) changed++;
                result.SetPixel(x,y,!outside[i]?Color.White:Color.Black);
            }
            Save(result,path);return changed;
        }
    }
    // A fixed 20-color material palette, without dithering or a smooth rescale.
    static readonly int[] Palette = {
        0x070707,0x171819,0x232424,0x31302D,0x403C36,
        0x514B41,0x625B4C,0x756B57,0x887D68,0x9D927D,
        0xB0A590,0xC1B69F,0xD1C6AF,0xE2D7C1,0x3F2911,
        0x654316,0x906021,0xC0872B,0x4D4640,0xA38F70
    };
    static Color Quantize(Color color) {
        int best=0, score=int.MaxValue;
        for(int i=0;i<Palette.Length;i++) {
            int value=Palette[i], r=color.R-((value>>16)&255), g=color.G-((value>>8)&255), b=color.B-(value&255);
            int distance=3*r*r+4*g*g+2*b*b;
            if(distance<score) { score=distance;best=value; }
        }
        return Color.FromArgb(255,(best>>16)&255,(best>>8)&255,best&255);
    }
    public static int[] Build(string source, string directory, int pixelSize, string capture) {
        using(var input=new Bitmap(source)) {
            int left=input.Width,top=input.Height,right=-1,bottom=-1;
            for(int y=0;y<input.Height;y++) for(int x=0;x<input.Width;x++) {
                Color p=input.GetPixel(x,y);
                if(p.A!=0 && p.A!=255) throw new InvalidDataException("INPUT_SOFT_ALPHA");
                if(p.A==0) continue;
                left=Math.Min(left,x);right=Math.Max(right,x);top=Math.Min(top,y);bottom=Math.Max(bottom,y);
            }
            if(right<left) throw new InvalidDataException("INPUT_EMPTY");
            int sw=right-left+1,sh=bottom-top+1;
            double scale=Math.Min((80.0/pixelSize)/sw,(96.0/pixelSize)/sh);
            int width=Math.Max(1,(int)Math.Floor(sw*scale)), height=Math.Max(1,(int)Math.Floor(sh*scale));
            int dx=(80/pixelSize-width)/2,dy=96/pixelSize-height;
            using(var art=new Bitmap(80,96,PixelFormat.Format32bppArgb))
            using(var sheet=new Bitmap(90,108,PixelFormat.Format32bppArgb)) {
                for(int y=0;y<height;y++) for(int x=0;x<width;x++) {
                    int sx=left+Math.Min(sw-1,(int)Math.Floor((x+.5)*sw/width));
                    int sy=top+Math.Min(sh-1,(int)Math.Floor((y+.5)*sh/height));
                    Color p=input.GetPixel(sx,sy);
                    if(p.A==255) {
                        Color c=Quantize(p);
                        for(int py=0;py<pixelSize;py++) for(int px=0;px<pixelSize;px++)
                            art.SetPixel((dx+x)*pixelSize+px,(dy+y)*pixelSize+py,c);
                    }
                }
                for(int y=0;y<96;y++) for(int x=0;x<80;x++)
                    sheet.SetPixel((x/16)*18+x%16,(y/16)*18+y%16,art.GetPixel(x,y));
                Save(art,Path.Combine(directory,"ArrivalPod.png"));
                Save(sheet,Path.Combine(directory,"ArrivalPod_Tile.png"));
                Preview(art,Path.Combine(directory,"preview.png"),pixelSize);
                if(pixelSize==2) ContextPreview(art,capture,Path.Combine(directory,"context-scale.png"));
            }
            return new int[]{left,top,sw,sh,width*pixelSize,height*pixelSize,dx*pixelSize,dy*pixelSize};
        }
    }
    static void ContextPreview(Bitmap art,string capture,string path) {
        using(var original=new Bitmap(capture))
        using(var canvas=new Bitmap(650,335)) {
            if(original.Width!=2560 || original.Height!=1401) throw new InvalidDataException("CAPTURE_SIZE");
            using(var g=Graphics.FromImage(canvas))
            using(var font=new Font("Consolas",11))
            using(var small=new Font("Consolas",9)) {
                g.Clear(Color.FromArgb(24,26,30));
                g.DrawString("SCALE COMPARISON - NEW POD IS NOT INSTALLED",font,Brushes.White,14,10);
                g.DrawString("REAL CAPTURE / old pod",small,Brushes.LightGray,16,42);
                g.DrawString("OFFLINE / new pod",small,Brushes.LightGray,368,42);
                g.DrawLine(Pens.Gray,365,221,622,221);
                g.DrawString("Left: unchanged crop of Sep 7's player/old pod capture.",small,Brushes.LightGray,16,282);
                g.DrawString("Right: approximate 4/3 screen scale, inferred from 14-tile pod spacing.",small,Brushes.LightGray,16,300);
                g.DrawString("Native-size pixels are on preview.png; live appearance remains untested.",small,Brushes.LightGray,16,315);
            }
            // No repaint, mask or re-scaling of the actual capture crop.
            for(int y=0;y<200;y++) for(int x=0;x<320;x++)
                canvas.SetPixel(16+x,64+y,original.GetPixel(1180+x,590+y));
            // An explicitly separate preview, never composited into game evidence.
            for(int y=0;y<128;y++) for(int x=0;x<107;x++) {
                Color p=art.GetPixel(Math.Min(79,(x*3)/4),(y*3)/4);
                if(p.A==255) canvas.SetPixel(411+x,93+y,p);
            }
            Save(canvas,path);
        }
    }
    static void CopyScaled(Bitmap source, Bitmap target, int x, int y, int scale) {
        for(int sy=0;sy<source.Height;sy++) for(int sx=0;sx<source.Width;sx++) {
            Color p=source.GetPixel(sx,sy);
            if(p.A==0) continue;
            for(int yy=0;yy<scale;yy++) for(int xx=0;xx<scale;xx++)
                target.SetPixel(x+sx*scale+xx,y+sy*scale+yy,p);
        }
    }
    static void Preview(Bitmap art,string path,int pixelSize) {
        using(var canvas=new Bitmap(1060,500)) {
            using(var g=Graphics.FromImage(canvas))
            using(var font=new Font("Consolas",11))
            using(var small=new Font("Consolas",9))
            using(var paper=new SolidBrush(Color.FromArgb(149,185,208)))
            using(var dark=new SolidBrush(Color.FromArgb(32,34,39)))
            using(var line=new Pen(Color.FromArgb(176,152,111))) {
                g.Clear(Color.FromArgb(18,20,24));
                g.FillRectangle(dark,8,38,230,182);
                g.FillRectangle(dark,258,38,384,408);
                g.FillRectangle(paper,660,38,384,408);
                g.DrawString(pixelSize==1?"A3 / actual 80 x 96":"A5 / actual 80 x 96",font,Brushes.White,12,10);
                g.DrawString("4x exact pixels / dark",font,Brushes.White,260,10);
                g.DrawString("4x exact pixels / light",font,Brushes.White,662,10);
                g.DrawRectangle(Pens.Gray,144,126,20,42);
                g.DrawLine(line,16,168,220,168);
                g.DrawString("20 x 42 collision guide",small,Brushes.LightGray,16,192);
                g.DrawLine(line,258,430,642,430);
                g.DrawLine(line,660,430,1044,430);
                g.DrawString("OFFLINE ASSEMBLY - not an in-game screenshot. No asset installed.",font,Brushes.White,16,462);
            }
            CopyScaled(art,canvas,28,72,1);
            CopyScaled(art,canvas,288,46,4);
            CopyScaled(art,canvas,692,46,4);
            Save(canvas,path);
        }
    }
}
'@
if ($coarse) { $bridgeCount=[ArrivalPodNative]::PrepareImpactMask($proposal,(Join-Path $out 'mask.png')) }
else { $bridgeCount=[ArrivalPodNative]::PrepareMask($proposal,(Join-Path $out 'mask.png')) }
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath $source -MaskPath (Join-Path $out 'mask.png') -OutputPath (Join-Path $out 'cutout.png') -SourceSHA256 $sourceHash
if ($LASTEXITCODE -ne 0) { throw 'HARD_MASK_EXPORT_FAILED' }
$pixelSize=if($coarse){2}else{1}
$geometry=[ArrivalPodNative]::Build((Join-Path $out 'cutout.png'),$out,$pixelSize,$capture)
$report=[ordered]@{
    schemaVersion=1; sourceSHA256=$sourceHash; maskProposalSHA256=$proposalHash
    maskSHA256=(Get-FileHash (Join-Path $out 'mask.png')).Hash
    maskThreshold='R+G+B >= 384'; hingeBridgePixels=$bridgeCount
    maskRepair=if($coarse){'A5-specific base/hinge joins [470,716..1005,739], [895,390..940,434], [907,554..948,600]; fill enclosed holes with 4-connected exterior flood. hingeBridgePixels includes enclosed fill. Preserve cockpit RGB; exclude independent ground.'}else{'Two A3-specific hinge strips; no hole fill'}
    sourceBounds=@($geometry[0],$geometry[1],$geometry[2],$geometry[3])
    fittedSize=@($geometry[4],$geometry[5]); destinationOffset=@($geometry[6],$geometry[7])
    nativeSize=@(80,96); sheetSize=@(90,108); coordinateHeights=@(16,16,16,16,16,16); padding=2
    sampling='Centre nearest-neighbour; aspect preserved within integer rounding; bottom aligned'
    palette='20 fixed source-directed material colors; RGB weighted-nearest; no dithering'
    pixelClusterSize=$pixelSize; logicalCanvas=@((80/$pixelSize),(96/$pixelSize))
    originalPixelsPreserved=$false
    scope='Exact-source mask cutout followed by explicit LOSSY size/palette fitting. Static candidate only; native art review and live furniture behavior pending.'
    nativeSHA256=(Get-FileHash (Join-Path $out 'ArrivalPod.png')).Hash
    sheetSHA256=(Get-FileHash (Join-Path $out 'ArrivalPod_Tile.png')).Hash
} | ConvertTo-Json -Depth 4
$reportStream=[IO.File]::Open((Join-Path $out 'native-report.json'),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
try {
    $reportBytes=[Text.UTF8Encoding]::new($false).GetBytes($report)
    $reportStream.Write($reportBytes,0,$reportBytes.Length)
} finally { $reportStream.Dispose() }
Write-Output $report
