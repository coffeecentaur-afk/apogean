param([ValidateRange(0,4)][int]$Mutation=0)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$references=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Color].Assembly.Location,'System.Runtime','System.IO')
$references+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
public static class ForegroundDepthCheck
{
    static bool Same(Color a,Color b)=>a.A==b.A&&(a.A==0||a.ToArgb()==b.ToArgb());
    public static void Run(string root,int mutation)
    {
        using(var full=new Bitmap(Path.Combine(root,"Derived/Foreground-Deep.png")))
        using(var upper=new Bitmap(Path.Combine(root,"Derived/Foreground-Bank.png")))
        using(var top=new Bitmap(Path.Combine(root,"Derived/Foreground-Top.png")))
        using(var lower=new Bitmap(Path.Combine(root,"Derived/Foreground-Continuation.png")))
        using(var source=new Bitmap(Path.Combine(root,"Foreground-Lower-Source.png")))
        {
            if(full.Width!=1448||full.Height!=1915||top.Height!=830||lower.Height!=1085||source.Width!=1449||source.Height!=1085)
                throw new InvalidDataException("DIMENSIONS");
            if(mutation==1)full.SetPixel(700,1400,Color.FromArgb(128,10,10,10));
            if(mutation==2)full.SetPixel(700,500,Color.White);
            if(mutation==3)lower.SetPixel(700,300,Color.White);
            if(mutation==4)full.SetPixel(700,1400,Color.White);
            for(int y=0;y<full.Height;y++)for(int x=0;x<full.Width;x++)
            {
                Color c=full.GetPixel(x,y);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("SOFT_ALPHA");
                if(y<830&&!Same(c,upper.GetPixel(x,y)))throw new InvalidDataException("UPPER_CHANGED");
                if(y>=1086&&!Same(c,source.GetPixel(x,y-830)))throw new InvalidDataException("LOWER_CHANGED");
                if(!Same(c,y<830?top.GetPixel(x,y):lower.GetPixel(x,y-830)))throw new InvalidDataException("SOCKET_SHIFT");
            }
        }
    }
}
'@
$repoRoot=Split-Path -Parent $PSScriptRoot
$candidate=Join-Path $repoRoot 'Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1'
[ForegroundDepthCheck]::Run($candidate,$Mutation)
foreach($asset in @('Highway','Quiet','Station','Foreground-Deep')) {
    $sourceHash=(Get-FileHash (Join-Path $candidate "Derived/$asset.png")).Hash
    $runtimeHash=(Get-FileHash (Join-Path $repoRoot "Content/Backgrounds/Candidates/WastesModules/$asset.png")).Hash
    if($sourceHash -ne $runtimeHash){throw "STALE_RUNTIME_ASSET: $asset"}
}
Write-Output 'PASS: 2,772,920 foreground pixels, hard alpha, original upper 830 rows, native lower source pixels, exact socket assembly, four QA asset hashes.'
