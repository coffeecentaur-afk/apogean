Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
$policy=Get-Content -LiteralPath (Join-Path $repo 'Content/Diagnostics/MawMaterialFramePolicy.cs') -Raw
$test=@'
public static class MawMaterialPolicyTests {
 public static int Run() {
  int count=0;
  for(int y=0;y<16;y++)for(int x=0;x<16;x++)
   for(int row=0;row<15;row++)for(int col=0;col<16;col++) {
    short a=(short)(col*18),b=(short)(row*18),u,v;
    if(!apogean.Content.Diagnostics.MawMaterialFramePolicy.TryMap(x,y,a,b,out u,out v)||
       u!=(x%8)*288+a||v!=(y%8)*270+b||u+16>2304||v+16>2160)
      throw new System.Exception("POLICY_FRAME");
    short revisitX,revisitY;
    if(!apogean.Content.Diagnostics.MawMaterialFramePolicy.TryMap(x+8,y+8,a,b,out revisitX,out revisitY)||
      u!=revisitX||v!=revisitY)throw new System.Exception("POLICY_PERIOD");
    count++;
   }
  short rx,ry;
  if(apogean.Content.Diagnostics.MawMaterialFramePolicy.TryMap(-1,0,0,0,out rx,out ry)||
     apogean.Content.Diagnostics.MawMaterialFramePolicy.TryMap(0,-1,0,0,out rx,out ry)||
     apogean.Content.Diagnostics.MawMaterialFramePolicy.TryMap(0,0,-1,0,out rx,out ry)||
     apogean.Content.Diagnostics.MawMaterialFramePolicy.TryMap(0,0,288,0,out rx,out ry)||
     apogean.Content.Diagnostics.MawMaterialFramePolicy.TryMap(0,0,0,270,out rx,out ry))
      throw new System.Exception("POLICY_BOUNDS");
  return count+5;
 }
}
'@
Add-Type -TypeDefinition ($policy+$test)
$count=[MawMaterialPolicyTests]::Run()
Write-Output "PASS $count exact production-policy cases; stateless offsets only, no saved-frame changes."
