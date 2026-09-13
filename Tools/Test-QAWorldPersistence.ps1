param([string]$OutputPath=(Join-Path ([IO.Path]::GetTempPath()) ('ApogeanPersistence-'+[guid]::NewGuid().ToString('N')+'.json')))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $OutputPath){throw 'Preserve prior test evidence.'}
$root=Split-Path -Parent $PSScriptRoot
function Body([string]$Source,[string]$Method) {
 $m=[regex]::Match($Source,'public override void '+$Method+'\(TagCompound tag\)\s*\{')
 if(-not $m.Success){throw "Missing hook $Method"}
 $start=$m.Index+$m.Length;$depth=1;$end=$start
 # These selected hook bodies have no brace-containing strings or comments.
 # Reject those if introduced rather than silently changing the extraction.
 for(;$end -lt $Source.Length;$end++){
  if($Source[$end] -eq '{'){$depth++}elseif($Source[$end] -eq '}'){$depth--;if($depth -eq 0){break}}
 }
 if($depth -ne 0){throw 'Unbalanced hook'}
 return $Source.Substring($start,$end-$start)
}
$names=@('ArrivalPodLab','MawBoneLab','MawFangLab','MawToothArtLab','MawToothClusterLab','MawClusterOrientationLab','MawMaterialLab','MawMaterialFamilyLab','VegetationVisualLab','WastesGroundProfileSystem')
$classes=@(foreach($name in $names){
 $folder=if($name -eq 'WastesGroundProfileSystem'){'Backgrounds'}else{'Diagnostics'}
 $src=Get-Content -Raw -LiteralPath (Join-Path $root "Content/$folder/$name.cs")
 $predicates=@([regex]::Matches($src,'private static bool (?:IsQa|IsQaWorld|InScope|OwnsQaSave)\s*=>[^;]+;')|ForEach-Object {$_.Value}) -join "`n"
 $save=Body $src 'SaveWorldData';$load=Body $src 'LoadWorldData'
 "public sealed class $name : Fields { $predicates public override void SaveWorldData(TagCompound tag){$save} public override void LoadWorldData(TagCompound tag){$load} }"
})
$support=@'
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using apogean.Common.Backgrounds;
public class TagCompound : Dictionary<string,object> {
 public TagCompound GetCompound(string k)=>(TagCompound)this[k];
 public int GetInt(string k)=>TryGetValue(k,out var v)?(int)v:0;
 public string GetString(string k)=>TryGetValue(k,out var v)?(string)v:"";
 public bool GetBool(string k)=>TryGetValue(k,out var v)&&(bool)v;
 public bool TryGet<T>(string k,out T value){if(TryGetValue(k,out var v)&&v is T t){value=t;return true;}value=default;return false;}
}
public struct Rectangle {
 public int X,Y,Width,Height;public int Left=>X;public int Top=>Y;public bool IsEmpty=>Width==0&&Height==0;
 public Rectangle(int x,int y,int w,int h){X=x;Y=y;Width=w;Height=h;}
}
public class WorldFile {public string Name;}
public class Player {public string name;}
public static class NetmodeID {public const int SinglePlayer=0;}
public static class Main {public static int netMode,maxTilesX=8400,maxTilesY=2400;public static WorldFile ActiveWorldFileData;public static Player LocalPlayer;}
public class Fields {
 protected Rectangle bounds;protected string checkpoint,legacyCheckpoint;
 protected int worldSpawnX,worldSpawnY,bedSpawnX,bedSpawnY;
 protected bool curveProof;protected WastesGroundProfile profile;
 protected const string SaveKey="wastesGroundProfileQA2";
 protected static int ExpectedCount=264;
 public virtual void LoadWorldData(TagCompound tag){}public virtual void SaveWorldData(TagCompound tag){}
}
public static class PersistenceCases {
 static TagCompound Seed(string name) {
  var value=new TagCompound{{"x",100},{"y",200},{"checkpoint","never-recompute-this"}};
  string key=name switch{
   "ArrivalPodLab"=>"podFixtureV1","MawBoneLab"=>"mawBoneFixtureV1","MawFangLab"=>"mawFangFixtureV1",
   "MawToothArtLab"=>"mawToothArtV1","MawToothClusterLab"=>"mawClusterV1","MawClusterOrientationLab"=>"mawClusterOrientationV1",
   "MawMaterialLab"=>"mawMaterialFixtureV1","MawMaterialFamilyLab"=>"mawFamilyFixtureV1",_=>""};
  if(name=="VegetationVisualLab")return new TagCompound{{"groveLeft",100},{"groveTop",200},{"groveCheckpointV1","never-recompute-grove"}};
  if(name=="WastesGroundProfileSystem")return new TagCompound{{"wastesGroundProfileQA2",Enumerable.Repeat(175,264).ToArray()}};
  if(name=="ArrivalPodLab")value=new TagCompound{{"left",100},{"top",200},{"checkpoint","never-recompute-this"},{"fingerprintVersion",2},{"legacyCheckpoint","legacy"},{"spawnX",60},{"spawnY",70},{"bedX",80},{"bedY",90}};
  if(name is "MawFangLab" or "MawToothArtLab" or "MawToothClusterLab" or "MawClusterOrientationLab")value.Remove("checkpoint");
  if(name=="MawClusterOrientationLab")value["curveProofV4"]=true;
  return new TagCompound{{key,value}};
 }
 static string Canonical(object value) {
  if(value is TagCompound t)return "{"+string.Join(",",t.OrderBy(p=>p.Key,StringComparer.Ordinal).Select(p=>JsonSerializer.Serialize(p.Key)+":"+Canonical(p.Value)))+"}";
  return JsonSerializer.Serialize(value,value.GetType());
 }
 public static object[] Run(){
  var results=new List<object>();
  foreach(string name in new[]{NAMES})foreach(int mode in new[]{0,1,2})foreach(string player in new[]{"gg","Maw Plain",null})foreach(bool existing in new[]{true,false}){
   string error=null;try{
    Main.netMode=mode;Main.ActiveWorldFileData=new WorldFile{Name="Apogee Native Visual V3"};Main.LocalPlayer=player==null?null:new Player{name=player};
    var seed=existing?Seed(name):new TagCompound();var first=(Fields)Activator.CreateInstance(typeof(PersistenceCases).Assembly.GetType(name));
    first.LoadWorldData(seed);var saved=new TagCompound();first.SaveWorldData(saved);
    if(Canonical(seed)!=Canonical(saved))throw new Exception("METADATA_OMITTED_OR_CHANGED");
    var second=(Fields)Activator.CreateInstance(first.GetType());second.LoadWorldData(saved);var savedAgain=new TagCompound();second.SaveWorldData(savedAgain);
    if(Canonical(seed)!=Canonical(savedAgain))throw new Exception("SECOND_ROUND_TRIP_CHANGED");
    // Existing in-memory data cannot be serialized to an ordinary or unloaded world.
    Main.ActiveWorldFileData=new WorldFile{Name="ordinary-world"};var foreign=new TagCompound();first.SaveWorldData(foreign);if(foreign.Count!=0)throw new Exception("FOREIGN_WORLD_SAVE");
    var foreignLoad=(Fields)Activator.CreateInstance(first.GetType());foreignLoad.LoadWorldData(seed);Main.ActiveWorldFileData=new WorldFile{Name="Apogee Native Visual V3"};var foreignResult=new TagCompound();foreignLoad.SaveWorldData(foreignResult);if(foreignResult.Count!=0)throw new Exception("FOREIGN_WORLD_LOAD");
    Main.ActiveWorldFileData=null;var absentWorld=new TagCompound();first.SaveWorldData(absentWorld);if(absentWorld.Count!=0)throw new Exception("NULL_WORLD_SAVE");
   }catch(Exception e){error=e.GetBaseException().Message;}
   results.Add(new{system=name,netMode=mode,player=player??"<no-player>",existing,passed=error==null,error});
  }
  return results.ToArray();
 }
}
'@
$support=$support.Replace('NAMES',(($names|ForEach-Object {'"'+$_+'"'}) -join ','))
$profile=Get-Content -Raw -LiteralPath (Join-Path $root 'Common/Backgrounds/WastesGroundProfile.cs')
# Separate source units preserve original using declarations and the real immutable profile class.
Add-Type -TypeDefinition ($support+"`n"+($classes -join "`n")+"`n"+$profile.Replace('using System;',''))
$records=[PersistenceCases]::Run()
$report=[ordered]@{schemaVersion=1;utc=[DateTime]::UtcNow.ToString('o');cases=$records;limits='Actual selected load/save method bodies and scope predicates, minimal environment/TagCompound stand-ins, real ground-profile class. Native TagIO round trip is a separate required check. No QA action/collision/rendering/network certification.'}
$file=[IO.File]::Open([IO.Path]::GetFullPath($OutputPath),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$writer=[IO.StreamWriter]::new($file);$writer.Write(($report|ConvertTo-Json -Depth 8));$writer.Flush()}finally{$file.Dispose()}
$failed=@($records|Where-Object {-not $_.passed})
$failed|Group-Object system,error|Select-Object Name,Count|Format-Table
if($failed.Count){throw "QA persistence regression: $($failed.Count)/$($records.Count) cases failed; report retained."}
Write-Output "PASS $($records.Count) actual-hook cases, each with two round trips, foreign/null-world guards. Existing metadata only; no new fixture creation."
