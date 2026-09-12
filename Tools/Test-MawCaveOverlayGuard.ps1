Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawCaveBackdropProbe.cs')
$method=[regex]::Match($source,'(?s)private void CheckCompetition\(\).*?(?=\s*internal bool TryCamera)')
if(-not $method.Success){throw 'Competition method boundary changed; review extraction.'}
$template=@'
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
public class CaveOverlayGuardHarness {
 enum OverlayMode {Inactive,FadeIn,Active,FadeOut}
 class Overlay {public OverlayMode Mode; public float Opacity; public int Priority; public int Layer=0; public bool Visible; public bool IsVisible()=>Visible;}
 class EffectManager<T> {protected internal Dictionary<string,T> _effects=new Dictionary<string,T>();}
 class OverlayManager:EffectManager<Overlay> {
  private LinkedList<Overlay>[] _activeOverlays=Enumerable.Range(0,5).Select(_=>new LinkedList<Overlay>()).ToArray();
  public void Add(string name,Overlay overlay,bool scheduled){_effects[name]=overlay;if(scheduled)_activeOverlays[overlay.Priority].AddLast(overlay);}
 }
 static class Overlays {public static OverlayManager Scene;}
 static class JsonSerializer {public static string Serialize(object value)=>"";}
 class Logger {public void Info(string value){}}
 class FakeMod {public Logger Logger=new Logger();}
 FakeMod Mod=new FakeMod(); Overlay overlay=new Overlay();
 METHOD
 public static int Test() {
  int checks=0;
  foreach(bool scheduled in new[]{false,true})foreach(bool visible in new[]{false,true})
   foreach(OverlayMode mode in Enum.GetValues<OverlayMode>())foreach(int priority in Enumerable.Range(0,5)) {
    var h=new CaveOverlayGuardHarness();Overlays.Scene=new OverlayManager();
    Overlays.Scene.Add("own",h.overlay,true);
    Overlays.Scene.Add("other",new Overlay{Mode=mode,Visible=visible,Priority=priority,Opacity=visible?1:0},scheduled);
    bool refused=false;
    try{h.CheckCompetition();}catch(InvalidOperationException e){if(!e.Message.StartsWith("Another overlay"))throw;refused=true;}
    if(refused!=scheduled)throw new Exception("GUARD_MEMBERSHIP_MISMATCH scheduled="+scheduled+" visible="+visible+" mode="+mode);
    checks++;
   }
  return checks;
 }
}
'@
$code=$template.Replace('METHOD',$method.Value)
Add-Type -TypeDefinition $code -WarningAction SilentlyContinue
$count=[CaveOverlayGuardHarness]::Test()
$bad=$code.Replace('CaveOverlayGuardHarness','CaveOverlayBadGuardHarness').Replace('foreach(var list in scheduled)foreach(var other in list)',
 'foreach(var pair in registered)if(pair.Value!=overlay && pair.Value.IsVisible())throw new InvalidOperationException("Another overlay is visible"); foreach(var list in scheduled)foreach(var other in list)')
Add-Type -TypeDefinition $bad -WarningAction SilentlyContinue
$rejected=$false
try{[CaveOverlayBadGuardHarness]::Test()|Out-Null}catch{if($_.Exception.ToString() -match 'GUARD_MEMBERSHIP_MISMATCH'){$rejected=$true}else{throw}}
if(-not $rejected){throw 'Oracle accepted the original shader-visibility false positive.'}
Write-Output "PASS: $count real guard method/stub-manager cases; only scheduled membership rejects; own overlay is permitted. Original visible-but-unscheduled policy fails. Installed manager field layout still requires native verification."
