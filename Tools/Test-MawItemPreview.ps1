Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawItemPreview.cs')
$start=$source.IndexOf('private bool Draw()')
$end=$source.IndexOf('public override void PostUpdateEverything()', $start)
if($start -lt 0 -or $end -le $start){throw 'Preview draw seam changed; inspect the implementation.'}
$draw=$source.Substring($start,$end-$start)
# Execute the actual draw body against API doubles: dispatch/cleanup only.
$stub=@'
using System;
using System.Collections.Generic;
public struct Vector2 {
 public float X,Y;public Vector2(float x,float y){X=x;Y=y;}
 public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.X+b.X,a.Y+b.Y);
}
public struct Rectangle {public Rectangle(int x,int y,int w,int h){}}
public struct Color {public Color(int a,int b,int c,int d){}public static Color White,LightGray;}
public class Item {public int type=3,stack=1;}
public class Player {public bool mouseInterface;public int selectedItem=4;}
public class Batch {public void Draw(object a,Rectangle b,Color c){}}
public class Texture {public int Width=52,Height=52;}
public class Asset {public Texture Value=new();}
public static class TextureAssets {public static Asset MagicPixel=new(),InventoryBack=new();}
public static class Utils {public static void DrawBorderString(Batch b,string s,Vector2 p,Color c,float scale=1){}}
public static class Main {
 public static float inventoryScale=.73f,UIScale=1.25f;public static ulong GameUpdateCount=123;
 public static Item mouseItem=new(),HoverItem=new();public static Player LocalPlayer=new();
 public static bool playerInventory;public static Batch spriteBatch=new();
}
public static class Mod {public static class Logger {public static void Info(string s){}public static void Error(string s,Exception e){}}}
public static class ItemSlot {
 public static class Context {public const int InWorld=31;}
 public static int FailAt=-1;public static bool MissingHooks,MutateUi;
 public static Preview Owner;public static readonly List<(Item[] Items,int Context,int Slot,Vector2 Position,float Scale)> Calls=new();
 public static void Draw(Batch batch,Item[] items,int context,int slot,Vector2 position,Color color){
  Calls.Add((items,context,slot,position,Main.inventoryScale));
  if(slot==FailAt)throw new Exception("synthetic native failure");
  if(slot<2&&!MissingHooks)Owner.hooks.Add(new object());
  if(MutateUi)Main.LocalPlayer.selectedItem=9;
 }
}
public class Preview {
 private Item[] items={new(),new(),new(),new()};private bool Context=true,collecting;
 private int draws;public readonly List<object> hooks=new();private object firstFrame;
 private readonly string[] labels={"rib","cap","stone","dirt"};private string reason;
 private void Cancel(string r){items=null;reason=r;}
'@
$tests=@'
 public static int Test(){
  int count=0;void Check(bool b,string why){if(!b)throw new Exception("ITEM_PREVIEW: "+why);count++;}
  void Reset(Preview p){ItemSlot.Owner=p;ItemSlot.Calls.Clear();ItemSlot.FailAt=-1;ItemSlot.MissingHooks=false;ItemSlot.MutateUi=false;Main.inventoryScale=.73f;Main.LocalPlayer.selectedItem=4;}
  var p=new Preview();Reset(p);var array=p.items;
  Check(p.Draw(),"later UI layers continue");Check(p.draws==1&&p.reason==null&&p.firstFrame!=null,"one successful frame");
  Check(Main.inventoryScale==.73f,"normal scale restoration");Check(!p.collecting,"collection flag restoration");
  Check(ItemSlot.Calls.Count==4,"exactly four slots");
  for(int i=0;i<4;i++){
   var call=ItemSlot.Calls[i];Check(ReferenceEquals(call.Items,array),"owned array overload");
   Check(call.Context==31&&call.Slot==i,"non-navigation context and ordered index");
   Check(call.Position.X==100+i*192&&call.Position.Y==230,"slot top-left, not icon center");
   Check(call.Scale==1,"equal logical scale");
  }
  for(int i=0;i<4;i++){
   p=new Preview();Reset(p);ItemSlot.FailAt=i;
   Check(p.Draw(),"exception contained");Check(p.reason=="draw-failure"&&p.draws==0,"failed frame not success");
   Check(Main.inventoryScale==.73f&&!p.collecting,"exception scale/flag restoration");
  }
  p=new Preview();Reset(p);ItemSlot.MissingHooks=true;p.Draw();Check(p.reason=="draw-failure","missing custom hooks rejected");
  p=new Preview();Reset(p);ItemSlot.MutateUi=true;p.Draw();Check(p.reason=="draw-failure","sampled UI mutation rejected, not repaired");
  p=new Preview();Reset(p);p.Context=false;p.Draw();Check(ItemSlot.Calls.Count==0,"wrong-context draw refused");
  return count;
 }
}
'@
function Code([string]$ns,[string]$body){'namespace '+$ns+' {'+$stub+$body+$tests+'}'}
Add-Type -TypeDefinition (Code 'PreviewBaseline' $draw)
$count=[PreviewBaseline.Preview]::Test()
$bad=$draw.Replace('Main.inventoryScale = originalScale;', 'Main.inventoryScale = 1f;')
if($bad -eq $draw){throw 'No actual scale restoration to mutate.'}
Add-Type -TypeDefinition (Code 'PreviewBadScale' $bad)
$rejected=$false
try{[PreviewBadScale.Preview]::Test()|Out-Null}catch{if($_.Exception.ToString() -notmatch 'ITEM_PREVIEW:'){throw};$rejected=$true}
if(-not $rejected){throw 'Missing scale restoration was not rejected.'}
Write-Output "PASS $count actual-draw-body dispatch/exception checks and wrong-restoration negative. API doubles only, not native icon evidence."
