param([string]$CandidateDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(-not $CandidateDirectory){$CandidateDirectory=Join-Path (Split-Path $PSScriptRoot) 'Art/Candidates/MawTerrain-Harsh-v1/Packed-v2'}
$policy=Get-Content -Raw -LiteralPath (Join-Path (Split-Path $PSScriptRoot) 'Content/Diagnostics/PackedMaterialMap.cs')
$harness=@'
namespace apogean.Content.Diagnostics {
 public static class PackedMapTest {
  public static int Run(byte[] bytes) {
   var m=new PackedMaterialMap(bytes);int count=0,pitch=m.Body==32?36:18;
   for(int y=0;y<m.Rows;y++)for(int x=0;x<m.Columns;x++)for(int j=0;j<16;j++)for(int i=0;i<16;i++) {
    short ax,ay,bx,by;
    if(!m.TryMap(i,j,x*pitch,y*pitch,out ax,out ay)||ax<0||ay<0||ax+m.Body>m.Width||ay+m.Body>m.Height)throw new System.Exception("FRAME_BOUNDS");
    if(!m.TryMap(i+8,j+8,x*pitch,y*pitch,out bx,out by)||ax!=bx||ay!=by)throw new System.Exception("PHASE_PERIOD");
    count++;
   }
   short tx,ty;
   if(m.TryMap(-1,0,0,0,out tx,out ty)||m.TryMap(0,0,-1,0,out tx,out ty)||m.TryMap(0,0,1,0,out tx,out ty)||m.TryMap(0,0,m.Columns*pitch,0,out tx,out ty)||m.TryMap(0,0,0,m.Rows*pitch,out tx,out ty))throw new System.Exception("INVALID_FRAME_ACCEPTED");
   for(int variant=0;variant<3;variant++) {
    byte[] bad=(byte[])bytes.Clone();if(variant==0)bad[0]^=1;else if(variant==1)bad[4]=0;else for(int k=32;k<36;k++)bad[k]=255;
    bool rejected=false;try{new PackedMaterialMap(bad);}catch(System.IO.InvalidDataException){rejected=true;}
    if(!rejected)throw new System.Exception("BAD_METADATA_ACCEPTED");
   }
   return count+8;
  }
 }
}
'@
Add-Type -TypeDefinition ($policy+$harness)
$checks=0
$maps=@(Get-ChildItem -LiteralPath $CandidateDirectory -Filter map.bin -Recurse)
if($maps.Count -ne 24){throw 'MISSING_MAPS'}
foreach($map in $maps){$checks += [apogean.Content.Diagnostics.PackedMapTest]::Run([IO.File]::ReadAllBytes($map.FullName))}
Write-Output "PASS actual runtime lookup: $checks exhaustive frame/phase/bounds/invalid-metadata checks."
