[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Backgrounds/CloseBackgroundDimensions.cs')
$resolver=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Backgrounds/ApogeanCloseBackgroundDimensions.cs')
# Dependencies are stubs; the resolver under test is the unmodified production file.
# This checks ownership/branch behavior, not a graphics device or native loading.
$stubs=@'
namespace Terraria {
 public static class Main { public static int[] backgroundWidth,backgroundHeight; }
}
namespace Terraria.GameContent {
 public sealed class Texture { public int Width,Height; }
 public sealed class Asset {
  public bool IsLoaded; public Texture Texture;
  public Texture Value => IsLoaded ? Texture : throw new System.Exception("unready asset accessed");
 }
 public static class TextureAssets { public static Asset[] Background; }
}
'@
$checks=@'
namespace apogean.Content.Backgrounds {
 public static class CloseDimensionsChecks {
  public static int Run() {
   int n=0;
   void Check(bool value,string why) { if(!value)throw new System.Exception(why); n++; }
   Check(CloseBackgroundDimensions.Usable(true,952,480,1.25f),"valid Maw fallback");
   Check(CloseBackgroundDimensions.Usable(true,2,2,1.25f),"transparent placeholder is not a zero divisor");
   Check(CloseBackgroundDimensions.Usable(true,1,1,.5f),"smallest safe integer stride");
   foreach(bool loaded in new[]{false,true}) foreach(int w in new[]{-1,0,1,2,952,int.MaxValue})
   foreach(int h in new[]{-1,0,1,480}) foreach(float s in new[]{float.NaN,float.NegativeInfinity,float.PositiveInfinity,-1f,0f,float.Epsilon,.01f,.499f,.5f,1.25f,float.MaxValue}) {
    bool result=CloseBackgroundDimensions.Usable(loaded,w,h,s);
    float native=w*(s*2f);
    bool expected=loaded&&w>0&&h>0&&float.IsFinite(s)&&s>0&&float.IsFinite(native)&&native>=1&&(double)native<=int.MaxValue;
    Check(result==expected,"loader arithmetic boundary");
    if(result) Check((int)native>0,"admitted zero/negative divisor");
   }
   // Original failure: asset available but stale metadata, so the engine alone divides by zero.
   bool red=false; try { int stale=0; _=2560/stale; } catch(System.DivideByZeroException) {red=true;}
   Check(red,"old zero-cache negative control");
   Terraria.Main.backgroundWidth=new[]{77,0,99}; Terraria.Main.backgroundHeight=new[]{33,0,55};
   Terraria.GameContent.TextureAssets.Background=new Terraria.GameContent.Asset[3];
   Check(ApogeanCloseBackgroundDimensions.Resolve(-1,1.25f)==-1,"negative slot");
   Check(ApogeanCloseBackgroundDimensions.Resolve(3,1.25f)==-1,"past asset array");
   Check(ApogeanCloseBackgroundDimensions.Resolve(1,1.25f)==-1,"null asset");
   var asset=new Terraria.GameContent.Asset { IsLoaded=false,Texture=new Terraria.GameContent.Texture {Width=952,Height=480} };
   Terraria.GameContent.TextureAssets.Background[1]=asset;
   Check(ApogeanCloseBackgroundDimensions.Resolve(1,1.25f)==-1,"pending asset must not dereference Value");
   Check(Terraria.Main.backgroundWidth[1]==0,"pending does not mutate cache");
   asset.IsLoaded=true;
   Check(ApogeanCloseBackgroundDimensions.Resolve(1,1.25f)==1,"loaded fallback admitted");
   Check(Terraria.Main.backgroundWidth[1]==952&&Terraria.Main.backgroundHeight[1]==480,"both stale dimensions repaired");
   Check(Terraria.Main.backgroundWidth[0]==77&&Terraria.Main.backgroundWidth[2]==99&&Terraria.Main.backgroundHeight[0]==33&&Terraria.Main.backgroundHeight[2]==55,"unselected slots preserved");
   Terraria.Main.backgroundWidth[1]=17; Terraria.Main.backgroundHeight[1]=19;
   Check(ApogeanCloseBackgroundDimensions.Resolve(1,0)==-1,"invalid scale skipped");
   Check(Terraria.Main.backgroundWidth[1]==17&&Terraria.Main.backgroundHeight[1]==19,"rejected request never updates cache");
   Terraria.Main.backgroundHeight=new int[1];
   Check(ApogeanCloseBackgroundDimensions.Resolve(1,1.25f)==-1,"partial metadata arrays guarded");
   return n;
  }
 }
}
'@
# Add-Type accepts one compilation string. Hoist the resolver's ordinary imports;
# method bodies remain verbatim (no behavioral replacement in the harness).
$combined="using Terraria;`nusing Terraria.GameContent;`n"+$source+"`n"+
    ([regex]::Replace($resolver,'(?m)^using [^;]+;\r?\n',''))+"`n"+$stubs+"`n"+$checks
Add-Type -TypeDefinition $combined
$n=[apogean.Content.Backgrounds.CloseDimensionsChecks]::Run()
Write-Output "PASS $n close-texture dimension/stride checks. Native cache repair, loading and visual checks remain separate."
