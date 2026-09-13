Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/TileLabPlayer.cs')
# Execute the real shared dispatch prefix AND save branch together. Looking only
# at the final branch misses the newer scene releases that already precede it.
$start=$source.IndexOf('if (request.StartsWith("qa-perf-", System.StringComparison.Ordinal))')
$end=$source.IndexOf('if (request.StartsWith("maw-shallow-", System.StringComparison.Ordinal) ||', $start)
if($start -lt 0 -or $end -lt $start){throw 'Shared command seam changed; inspect before adjusting this test.'}
$dispatch=$source.Substring($start,$end-$start)
$names=@('MawCaveBackdropProbe','MawShallowTraversalLab','MawRibContourStudy','MawAnatomyLab','MawFiberRibLab','MawNaturalLab','MawPlayableLab','MawMaterialFamilyLab','MawMaterialLab','MawClusterOrientationLab','MawToothClusterLab','MawToothArtLab','MawFangLab','MawBoneLab','ArrivalPodLab','WastesLandscapeCameraLab','ForestSprayVisualLab','VegetationVisualLab')
$classes=($names|ForEach-Object {"public sealed class $_ : View {}"}) -join "`n"
$expected=($names|ForEach-Object {'"'+$_+'"'}) -join ','
$support=@'
using System;
using System.Collections.Generic;
using System.Linq;
public static class Seen {
 public static readonly HashSet<string> Active=new();public static readonly List<string> Events=new();public static bool Recording;
 public static void Reset(){Active.Clear();Events.Clear();Recording=false;}
}
public class View {
 public void Release(){string n=GetType().Name;Seen.Active.Remove(n);Seen.Events.Add(n+"/release");}
 public void Stop()=>Release();public void Start(string s)=>Seen.Events.Add(GetType().Name+"/start");
}
public class QAPerformanceLab {public bool Recording=>Seen.Recording;public void Run(string s)=>Seen.Events.Add("performance/"+s);}
public static class ModContent {public static T GetInstance<T>() where T:new()=>new T();}
public static class Player {public static T GetModPlayer<T>() where T:new()=>new T();}
public static class Mod {public static class Logger {public static void Info(string s){}}}
public static class WorldGen {public static void SaveAndQuit(){
 if(Seen.Active.Count!=0)throw new Exception("EXIT_DISPATCH: saved with active views: "+string.Join(",",Seen.Active));
 Seen.Events.Add("save");
}}
'@
$test=@'
 public static int Test(){
  int n=0;void Check(bool ok,string why){if(!ok)throw new Exception("EXIT_DISPATCH: "+why);n++;}
  foreach(string name in Names){Seen.Reset();Seen.Active.Add(name);Run("qa-save-and-quit");
   Check(Seen.Active.Count==0,name+" cleanup");Check(Seen.Events.Last()=="save",name+" releases before save");
   Check(Seen.Events.Count(x=>x=="save")==1,"one save");}
  Seen.Reset();foreach(string name in Names)Seen.Active.Add(name);Run("qa-save-and-quit");Check(Seen.Active.Count==0,"all registered test views released");
  Seen.Reset();Seen.Recording=true;Seen.Active.Add("MawShallowTraversalLab");bool blocked=false;
  try{Run("qa-save-and-quit");}catch(InvalidOperationException){blocked=true;}
  Check(blocked&&Seen.Events.Count==0&&Seen.Active.Count==1,"ongoing sample requires explicit stop first");
  Seen.Reset();Seen.Recording=true;Seen.Active.Add("MawShallowTraversalLab");Run("qa-perf-stop");
  Check(Seen.Events.SequenceEqual(new[]{"performance/stop"})&&Seen.Active.Count==1,"passive command does not move measured scene");
  foreach(var pair in new[]{("maw-shallow-pristine","MawShallowTraversalLab"),("maw-cave-report","MawCaveBackdropProbe"),("maw-contour-pristine","MawRibContourStudy")}){
   Seen.Reset();Seen.Active.Add(pair.Item2);Run(pair.Item1);Check(Seen.Active.Contains(pair.Item2),"same-family command retains own view");Check(!Seen.Events.Contains("save"),"no incidental save");
  }
  return n;
 }
}
'@
function Code([string]$ns,[string]$body){
 'namespace '+$ns+' {'+$support+$classes+' public static class Dispatch { public static readonly string[] Names=new[]{'+$expected+'}; public static void Run(string request){'+$body+'}'+$test+'}'
}
Add-Type -TypeDefinition (Code 'Baseline' $dispatch)
$count=[Baseline.Dispatch]::Test()
$controls=0
foreach($name in @('MawCaveBackdropProbe','MawShallowTraversalLab','MawRibContourStudy')){
 $call="ModContent.GetInstance<$name>().Release();"
 $bad=$dispatch.Replace($call,'{ }')
 if($bad -eq $dispatch){throw "No actual release to mutate for $name"}
 $ns='Missing'+$controls
 Add-Type -TypeDefinition (Code $ns $bad)
 $caught=$false
 try{($ns+'.Dispatch' -as [type])::Test()|Out-Null}catch{if($_.Exception.ToString() -notmatch 'EXIT_DISPATCH:'){throw};$caught=$true}
 if(-not $caught){throw "Missing-release mutation survived for $name"}
 $controls++
}
Write-Output "PASS $count shared-dispatch ordering checks and $controls omitted-release controls. Not native view restoration, outer authorization or engine saving proof."
