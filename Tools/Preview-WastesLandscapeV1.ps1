param()
# Offline composition proof only; not a game capture or routing test.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
public static class WastesLandscapePreview
{
    public static void Write(string root, string destination)
    {
        Directory.CreateDirectory(destination);
        string[] names = {"Far", "Mid", "Close"};
        using (var far = new Bitmap(Path.Combine(root, "Far.png")))
        using (var mid = new Bitmap(Path.Combine(root, "Mid.png")))
        using (var close = new Bitmap(Path.Combine(root, "Close.png")))
        {
            var layers = new[]{far,mid,close};
            foreach (string scene in new[]{"Ground", "Aerial", "Night", "RepeatJoin"})
            using (var canvas = new Bitmap(1920,1080,PixelFormat.Format32bppArgb))
            using (var graphics = Graphics.FromImage(canvas))
            using (var font = new Font("Consolas",14))
            {
                bool night = scene == "Night";
                graphics.Clear(night ? Color.FromArgb(15,23,42) : Color.FromArgb(151,195,219));
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                float delta = scene == "Aerial" ? 1200 : -20;
                float cameraX = scene == "RepeatJoin" ? 14500 : 5400;
                for (int i=0;i<3;i++)
                {
                    float h = i==0?.055f:i==1?.14f:.30f;
                    float v = i==0?.10f:i==1?.18f:.30f;
                    float authoredTop=1080*(.57f+i*.025f)-740+delta*v;
                    int top=(int)Math.Floor(i==2?Math.Max(1080-layers[i].Height,authoredTop):authoredTop);
                    float phase = cameraX*h % layers[i].Width;
                    using (var attributes = new ImageAttributes())
                    {
                        var matrix = new ColorMatrix();
                        if (night) {matrix.Matrix00=65/255f;matrix.Matrix11=75/255f;matrix.Matrix22=98/255f;}
                        attributes.SetColorMatrix(matrix);
                        for(float x=-phase;x<1920;x+=layers[i].Width)
                            graphics.DrawImage(layers[i],new Rectangle((int)Math.Floor(x),top,layers[i].Width,layers[i].Height),0,0,layers[i].Width,layers[i].Height,GraphicsUnit.Pixel,attributes);
                    }
                }
                using(var band=new SolidBrush(Color.FromArgb(230,16,21,26))) graphics.FillRectangle(band,0,0,1920,38);
                graphics.DrawString("OFFLINE COMPOSITION ONLY | Wastes V1 | " + scene + " | 1920x1080, scale 1 | not a tModLoader screenshot",font,Brushes.White,12,8);
                canvas.Save(Path.Combine(destination,scene+".png"),ImageFormat.Png);
            }
        }
    }
}
'@
$root = Split-Path -Parent $PSScriptRoot
[WastesLandscapePreview]::Write((Join-Path $root 'Content/Backgrounds/Candidates/WastesV1'),(Join-Path $root 'Art/Validation/WastesLandscapeV1/Offline'))
