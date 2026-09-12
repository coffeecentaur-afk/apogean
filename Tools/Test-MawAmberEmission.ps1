Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/PackedEmissionMap.cs')
$harness=@'
namespace apogean.Content.Diagnostics {
 public static class EmissionChecks {
  public static string Run(){int checks=0;void Require(bool v){if(!v)throw new System.Exception("EMISSION_ASSERT");checks++;}
   using var bytes=new System.IO.MemoryStream();using var writer=new System.IO.BinaryWriter(bytes);
   writer.Write(0x4D454D41);writer.Write(36);writer.Write(18);writer.Write(18);writer.Write(2);writer.Write((ushort)0);writer.Write((ushort)256);writer.Flush();
   byte[] valid=bytes.ToArray();var map=new PackedEmissionMap(valid);
   Require(map.Count(0,0)==0);Require(map.Count(18,0)==256);
   foreach(var p in new[]{(-1,0),(0,-1),(36,0),(0,18),(1,0)})Require(map.Count(p.Item1,p.Item2)==0);
   for(int n=0;n<=256;n++){float awake=PackedEmissionMap.Strength(n,false),sleep=PackedEmissionMap.Strength(n,true);
    Require(awake>=0&&awake<=1);Require(System.Math.Abs(sleep-awake*.32f)<.00001);if(n>0)Require(awake>=PackedEmissionMap.Strength(n-1,false));}
   Require(PackedEmissionMap.Strength(0,false)==0);Require(PackedEmissionMap.Strength(0,true)==0);
   foreach(int mutation in new[]{0,4,12,16,22}){byte[] bad=(byte[])valid.Clone();bad[mutation]=255;bool rejected=false;
    try{new PackedEmissionMap(bad);}catch(System.IO.InvalidDataException){rejected=true;}Require(rejected);}
   bool truncated=false;try{new PackedEmissionMap(new byte[3]);}catch(System.IO.InvalidDataException){truncated=true;}Require(truncated);
   return $"PASS {checks} emission lookup/monotonic activity assertions and six corrupt-input controls. No native lighting claim.";
  }
 }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$harness)
[apogean.Content.Diagnostics.EmissionChecks]::Run()
