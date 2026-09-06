param([ValidateRange(0,6)][int]$Mutation = 0)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$references=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Color].Assembly.Location,
    'System.Runtime','System.Collections','System.Linq')
$references+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
public static class ModularMidCheck
{
    static bool Same(Color a,Color b)=>a.A==b.A&&(a.A==0||a.ToArgb()==b.ToArgb());
    static void Check(Bitmap image,Bitmap top,Bitmap lower,Bitmap[] groups)
    {
        if(image.Width!=1536||image.Height!=1408||top.Width!=1536||top.Height!=768||lower.Width!=1536||lower.Height!=640)
            throw new InvalidDataException("DIMENSIONS");
        int[] xs={0,616,1048}, widths={576,391,488};
        for(int i=0;i<3;i++)if(groups[i].Width!=widths[i]||groups[i].Height!=1408)
            throw new InvalidDataException("GROUP_DIMENSIONS");
        long transparent=0;
        for(int y=0;y<image.Height;y++)for(int x=0;x<image.Width;x++)
        {
            Color c=image.GetPixel(x,y);
            if(c.A!=0&&c.A!=255)throw new InvalidDataException("SOFT_ALPHA");
            if(c.A==255&&c.R-c.G>24&&c.B-c.G>24)throw new InvalidDataException("MATTE");
            if(c.A==0)transparent++;
            if((x>=576&&x<616||x>=1007&&x<1048)&&c.A!=0)throw new InvalidDataException("VALLEY_CLOSED");
            Color socket=y<768?top.GetPixel(x,y):lower.GetPixel(x,y-768);
            if(!Same(c,socket))throw new InvalidDataException("SOCKET_SHIFT");
            for(int i=0;i<3;i++)if(x>=xs[i]&&x<xs[i]+widths[i]&&!Same(c,groups[i].GetPixel(x-xs[i],y)))
                throw new InvalidDataException("GROUP_SHIFT");
        }
        if(transparent!=813629)throw new InvalidDataException("ALPHA_MASK_CHANGED");
    }
    public static string RunOne(string path,int mutation)
    {
        using(var joined=new Bitmap(Path.Combine(path,"Joined.png")))
        using(var top=new Bitmap(Path.Combine(path,"Joined-Top.png")))
        using(var lower=new Bitmap(Path.Combine(path,"Joined-Continuation.png")))
        using(var highway=new Bitmap(Path.Combine(path,"Highway.png")))
        using(var quiet=new Bitmap(Path.Combine(path,"Quiet.png")))
        using(var station=new Bitmap(Path.Combine(path,"Station.png")))
        using(var wrongWidth=new Bitmap(575,1408))
        {
            var groups=new[]{highway,quiet,station};
            if(mutation==1)joined.SetPixel(40,700,Color.FromArgb(80,100,80,50));
            if(mutation==2)joined.SetPixel(40,700,Color.Magenta);
            if(mutation==3)joined.SetPixel(590,700,Color.Black);
            if(mutation==4)lower.SetPixel(40,0,Color.Magenta);
            if(mutation==5)groups[0]=wrongWidth;
            if(mutation==6)quiet.SetPixel(20,900,Color.Magenta);
            Check(joined,top,lower,groups);
            return "pass";
        }
    }
    public static string Run(string path)
    {
        RunOne(path,0);
        string[] expected={"SOFT_ALPHA","MATTE","VALLEY_CLOSED","SOCKET_SHIFT","GROUP_DIMENSIONS","GROUP_SHIFT"};
        for(int i=1;i<=expected.Length;i++)
        {
            string error=null;
            try{RunOne(path,i);}catch(InvalidDataException ex){error=ex.Message;}
            if(error!=expected[i-1])throw new InvalidDataException("Negative control "+i+" did not reject its intended defect: "+error);
        }
        return "PASS: 2,162,688 pixels; exact upper/lower reconstruction, three module crops, hard alpha, open valleys; six deliberately broken in-memory controls rejected. Not art, motion, routing or live-game approval.";
    }
}
'@
$repoRoot=Split-Path -Parent $PSScriptRoot
$derived=Join-Path $repoRoot 'Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/Derived'
if($Mutation -eq 0){Write-Output ([ModularMidCheck]::Run($derived))}
else{Write-Output ([ModularMidCheck]::RunOne($derived,$Mutation))}
