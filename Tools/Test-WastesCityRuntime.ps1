param(
    [string]$CandidateDirectory = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates/WastesFarCity-v1/Runtime-v1'),
    [string]$ReportPath = ''
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Join-Path $root 'Art/Candidates/WastesFarCity-v1/Transparent-v1/City-Transparent.png'
if((Get-FileHash -LiteralPath $source).Hash -ne '71C14F299210D278E1EB61B31EEBC9E341F2EF986E0C4D4159D0B3C2A41B785B'){throw 'CITY_SOURCE_CHANGED'}
$candidate=Join-Path $CandidateDirectory 'Far.png'
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Collections.Generic;
public static class CityRuntimeAudit {
    static void Check(Bitmap source,Bitmap fitted) {
        if(fitted.Width!=1458||fitted.Height!=1792)throw new Exception("DIMENSIONS");
        var ground=new HashSet<int>();
        for(int y=688;y<992;y++)for(int x=0;x<1586;x++)ground.Add(source.GetPixel(x,y).ToArgb());
        for(int y=0;y<1792;y++)for(int x=0;x<1458;x++){
            Color pixel=fitted.GetPixel(x,y); int value=pixel.ToArgb();
            if(pixel.A!=0&&pixel.A!=255)throw new Exception("PARTIAL_ALPHA");
            if(pixel.A==0&&value!=0)throw new Exception("HIDDEN_RGB");
            if(y>=688&&pixel.A!=255)throw new Exception("LOWER_HOLE");
            if(y<928){
                int original=source.GetPixel(x,y).ToArgb();
                if(x>=128&&value!=original)throw new Exception("PROTECTED_NATIVE_PIXELS");
                if(x<128&&value!=original&&value!=source.GetPixel(x+1458,y).ToArgb())throw new Exception("JOIN_DONOR");
                if(x==0&&value!=source.GetPixel(1458,y).ToArgb())throw new Exception("LEFT_JOIN");
                if(x==1457&&value!=source.GetPixel(1457,y).ToArgb())throw new Exception("RIGHT_JOIN");
            }else if(!ground.Contains(value))throw new Exception("GROUND_SOURCE_PALETTE");
        }
    }
    public static int Run(string sourcePath,string candidatePath) {
        using(var source=new Bitmap(sourcePath))using(var candidate=new Bitmap(candidatePath)){
            Check(source,candidate);
            // These controls deliberately invalidate independent contract clauses.
            int rejected=0;
            using(var changed=(Bitmap)candidate.Clone()){
                changed.SetPixel(500,500,Color.Magenta);
                try { Check(source,changed); } catch(Exception e) { if(e.Message!="PROTECTED_NATIVE_PIXELS")throw; rejected++; }
            }
            using(var changed=(Bitmap)candidate.Clone()){
                changed.SetPixel(500,1200,Color.Transparent);
                try { Check(source,changed); } catch(Exception e) { if(e.Message!="LOWER_HOLE"&&e.Message!="HIDDEN_RGB")throw; rejected++; }
            }
            using(var changed=(Bitmap)candidate.Clone()){
                changed.SetPixel(500,1200,Color.Magenta);
                try { Check(source,changed); } catch(Exception e) { if(e.Message!="GROUND_SOURCE_PALETTE")throw; rejected++; }
            }
            using(var shortImage=new Bitmap(1458,992)){
                try { Check(source,shortImage); } catch(Exception e) { if(e.Message!="DIMENSIONS")throw; rejected++; }
            }
            if(rejected!=4)throw new Exception("NEGATIVE_CONTROL_ESCAPED");
            return rejected;
        }
    }
}
'@
$negativeControls=[CityRuntimeAudit]::Run($source,$candidate)
Add-Type -Path (Join-Path $root 'Common/Backgrounds/WastesCameraProjection.cs')
Add-Type -Path (Join-Path $root 'Common/Backgrounds/WastesParallaxContract.cs')
$checks=0
$minimumMargin=[double]::PositiveInfinity
foreach($viewportHeight in 1080,1369,1440) {
    $viewportWidth=if($viewportHeight -eq 1080){1920}else{2560}
    $zoom=[single][math]::Max($viewportWidth/1920.0,$viewportHeight/1080.0)
    foreach($lift in -1600,-400,0,96,1200,2400,4200,8000) {
        $camera=[single](9584-$viewportHeight*.55-$lift)
        $top=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,$camera,$viewportHeight,1792,0)
        $margin=[math]::Floor($top)+1792-$viewportHeight
        if($margin -lt 0){throw "CITY_DEPTH_EXPOSED height=$viewportHeight lift=$lift"}
        $minimumMargin=[math]::Min($minimumMargin,$margin)
        $checks++
    }
    foreach($phase in 0,.01,1,64,729,1457.99) {
        $positions=@(for($x=-$phase;$x -lt $viewportWidth;$x+=1458){[math]::Floor($x)})
        $previous=$null
        foreach($x in $positions) {
            $pixel=[apogean.Common.Backgrounds.WastesCameraProjection]::LogicalCoordinate([single]$x,$zoom)*$zoom
            if($null -ne $previous -and [math]::Abs($pixel-$previous-1458) -gt .01){throw 'CITY_REPEAT_GAP'}
            $previous=$pixel
        }
        if($positions[0] -gt 0 -or $positions[-1]+1458 -lt $viewportWidth){throw 'CITY_SCREEN_EDGE_GAP'}
        $checks++
    }
}
$repeats=[apogean.Common.Backgrounds.WastesParallaxContract]::Repeats(128640,0,1458)
if([math]::Abs($repeats-4.853) -gt .002){throw 'CITY_REPEAT_TELEMETRY'}
$report=[ordered]@{
    pass=$true;candidateSHA256=(Get-FileHash -LiteralPath $candidate).Hash
    sourceSHA256=(Get-FileHash -LiteralPath $source).Hash
    size='1458x1792';nativeScale=1;rawBytes=1458*1792*4
    protectedRectangle='128,0,1330,928';negativeControls=$negativeControls
    projectionChecks=$checks;minimumFarBottomMargin=$minimumMargin
    scope='Offline pixel provenance, alpha and projection contracts only. Reused ground texture is not new painted detail. Not GPU, art, routing or production approval.'
}
if($ReportPath) {
    if(Test-Path -LiteralPath $ReportPath){throw 'REPORT_EXISTS'}
    $report | ConvertTo-Json | Set-Content -LiteralPath $ReportPath -Encoding utf8
}
$report | ConvertTo-Json
