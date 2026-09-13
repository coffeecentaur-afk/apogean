using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using S = apogean.Content.Diagnostics.MawShallowTraversalLab;

namespace apogean.Content.Diagnostics
{
    // Native API microchecks only. No item-use, movement, worldgen, or saved
    // specimen edits. The initially empty patch is restored after every case.
    internal static class MawRopeMaterialStudy
    {
        internal static void Run(log4net.ILog log)
        {
            if(Main.gameMenu||WorldGen.gen||Main.netMode!=NetmodeID.SinglePlayer||Main.ActiveWorldFileData?.Name!=MawShallowQaScope.World||
                Main.LocalPlayer.name!="gg"||Main.LocalPlayer.dead||Main.LocalPlayer.channel||Main.LocalPlayer.itemAnimation>0||!MawPackedPreview.Enabled||!MawAnatomyMaterials.Available||
                ModContent.GetInstance<QAPerformanceLab>().Recording)
                throw new InvalidOperationException("Rope microchecks require idle packed anatomy gg/V3/SP.");
            // Item.NewItem may recycle a live slot if the global pool is full.
            // Keep generous empty capacity rather than risking unrelated drops.
            if(Main.item.Take(Main.maxItems).Count(item=>!item.active)<32)
                throw new InvalidOperationException("Rope microchecks require 32 free item slots; nothing changed.");
            var history=S.Historical().Append(ModContent.GetInstance<S>().PreservedBounds).ToArray();
            string historyBefore=S.Fingerprint(history);
            Rectangle patch=MawConversionLoadStudy.FindEmptyPatch(history),guard=patch;guard.Inflate(12,12);
            Rectangle pixelGuard=new(guard.X*16,guard.Y*16,guard.Width*16,guard.Height*16);
            var original=new S.CellState[guard.Width*guard.Height];
            int Index(int x,int y)=>(x-guard.X)*guard.Height+y-guard.Y;
            for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)original[Index(x,y)]=S.CellState.Read(x,y);
            string before=S.Fingerprint(new[]{guard});
            Point top=new(patch.X+10,patch.Y+6);
            var cases=new List<object>();var ownDrops=new HashSet<int>();
            int checks=0,negativeControls=0;bool restored=false,historyUnchanged=false;string error=null;
            var clock=Stopwatch.StartNew();
            void Check(bool yes,string what){if(!yes)throw new InvalidOperationException("Rope microcheck: "+what);checks++;}
            void Budget(){if(clock.Elapsed.TotalSeconds>2)throw new TimeoutException("Two-second microcheck work limit; no further case started.");}
            void Restore(){
                foreach(int i in ownDrops)if(Main.item[i].active&&pixelGuard.Contains(Main.item[i].Center.ToPoint()))Main.item[i].TurnToAir();
                ownDrops.Clear();
                for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)original[Index(x,y)].Restore(x,y);
            }
            object Drops(Action action,out int ropeCount){
                bool[] active=new bool[Main.maxItems];for(int i=0;i<active.Length;i++)active[i]=Main.item[i].active;
                try{action();}finally{
                    for(int i=0;i<active.Length;i++)if(!active[i]&&Main.item[i].active&&pixelGuard.Contains(Main.item[i].Center.ToPoint()))ownDrops.Add(i);
                }
                ropeCount=ownDrops.Where(i=>Main.item[i].type==ItemID.Rope).Sum(i=>Main.item[i].stack);
                return ownDrops.Select(i=>new{type=Main.item[i].type,stack=Main.item[i].stack}).ToArray();
            }
            void Place(int x,int y,int type){
                Check(S.Empty(Main.tile[x,y]),"target empty before native placement");
                WorldGen.PlaceTile(x,y,type,mute:true,forced:false,plr:Main.LocalPlayer.whoAmI);
                Check(Main.tile[x,y].HasTile&&Main.tile[x,y].TileType==type,"native tile exists after placement");
            }
            int Count(){int count=0;for(int k=0;k<16;k++)if(Main.tile[top.X,top.Y+k].HasTile&&Main.tile[top.X,top.Y+k].TileType==TileID.Rope)count++;return count;}
            try{
                // Empty recognition is an actual negative control, not an invented support rule.
                Check(!WorldGen.IsRope(top.X,top.Y),"empty target is not rope");negativeControls++;
                int rib=MawAnatomyMaterials.Tile("rib").Type,cap=MawAnatomyMaterials.Tile("cap").Type;
                var hosts=new[]{(Name:"vanilla-gray-brick",Type:(int)TileID.GrayBrick),(Name:"candidate-rib",Type:rib),(Name:"candidate-fiber-cap",Type:cap)};
                foreach(var host in hosts)foreach(string anchor in new[]{"above","left","right","below"}){
                    Budget();Restore();Point at=anchor switch{
                        "above"=>new(top.X,top.Y-1),"left"=>new(top.X-1,top.Y),"right"=>new(top.X+1,top.Y),_=>new(top.X,top.Y+16)};
                    Place(at.X,at.Y,host.Type);
                    for(int k=0;k<16;k++)Place(top.X,top.Y+k,TileID.Rope);
                    WorldGen.SquareTileFrame(at.X,at.Y);
                    for(int k=0;k<16;k++){WorldGen.TileFrame(top.X,top.Y+k);Check(WorldGen.IsRope(top.X,top.Y+k),"native rope recognition");}
                    Check(Count()==16,"sixteen rope cells survive framing");
                    var frames=Enumerable.Range(0,16).Select(k=>new{x=(int)Main.tile[top.X,top.Y+k].TileFrameX,y=(int)Main.tile[top.X,top.Y+k].TileFrameY}).ToArray();
                    object drops=Drops(()=>Main.LocalPlayer.PickTile(top.X,top.Y+8,35),out int ropeDrops);
                    Check(!Main.tile[top.X,top.Y+8].HasTile&&Count()==15,"one native rope pick removes only selected segment");
                    Check(ropeDrops==1&&ownDrops.Sum(i=>Main.item[i].stack)==1,"exactly one real rope drop");
                    Check(Main.tile[at].HasTile&&Main.tile[at].TileType==host.Type,"rope mining preserves host");
                    WorldGen.KillTile(at.X,at.Y,noItem:true);WorldGen.SquareTileFrame(at.X,at.Y);
                    Check(!Main.tile[at].HasTile&&Count()==15,"support removal retains remaining ordinary rope");
                    cases.Add(new{kind="rope-api",host=host.Name,anchor,placed=16,afterMining=Count(),ropeDrops,drops,frames,
                        note="Direct PlaceTile bypasses player adjacency. Support persistence is not player-placement or ascent evidence."});
                }
                // Deliberately unsupported API placement demonstrates the seam's limitation.
                Budget();Restore();Place(top.X,top.Y,TileID.Rope);WorldGen.SquareTileFrame(top.X,top.Y);
                Check(WorldGen.IsRope(top.X,top.Y),"isolated rope created by tile API remains rope");
                cases.Add(new{kind="isolated-tile-api",recognized=true,playerPlacementTested=false});
                foreach(var material in new[]{(Name:"candidate-rib",Type:rib,Power:59),(Name:"candidate-fiber-cap",Type:cap,Power:35)}){
                    Budget();Restore();Place(top.X,top.Y,material.Type);
                    int rejectedCalls=0;
                    if(material.Type==rib){for(;rejectedCalls<30;rejectedCalls++)Main.LocalPlayer.PickTile(top.X,top.Y,58);Check(Main.tile[top].HasTile,"rib rejects pick58");negativeControls++;}
                    int hits=0;
                    object drops=Drops(()=>{while(Main.tile[top].HasTile&&hits<40){Main.LocalPlayer.PickTile(top.X,top.Y,material.Power);hits++;}},out _);
                    Check(!Main.tile[top].HasTile,"candidate removed through native pick-power path");
                    int registeredDrop=TileLoader.GetItemDropFromTypeAndStyle(material.Type);
                    cases.Add(new{kind="candidate-mining",host=material.Name,power=material.Power,rejected58Calls=rejectedCalls,hits,registeredDrop,drops,
                        note="Candidate tile may have no placeable item binding. Record absence; do not promote it as a shipping pickup/replacement feature."});
                }
            }catch(Exception ex){error=ex.GetType().Name+": "+ex.Message;}
            finally{
                Restore();restored=true;
                for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)restored&=original[Index(x,y)]==S.CellState.Read(x,y);
                historyUnchanged=historyBefore==S.Fingerprint(history);
            }
            string after=S.Fingerprint(new[]{guard});
            bool pass=error==null&&restored&&historyUnchanged&&before==after&&cases.Count==15&&negativeControls==2;
            var report=new{schemaVersion=1,utc=DateTime.UtcNow,pass,error,checks,negativeControls,restored,historyUnchanged,before,after,
                historyKnown=history.Count(r=>r.Width>0&&r.Height>0),historyMissing=history.Count(r=>r.Width<=0||r.Height<=0),
                patch=new{patch.X,patch.Y,patch.Width,patch.Height},elapsedMs=clock.Elapsed.TotalMilliseconds,cases,
                scope="Synchronous disposable native tile/mining APIs. No item-use placement, reach, swing timing, pulley ascent, manual feel, multiplayer or production promotion. Missing historical locations unverified."};
            string directory=Path.Combine(Main.SavePath,"Captures");Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"maw-rope-material-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N")[..6]+".json");
            using(var file=new FileStream(path+".partial",FileMode.CreateNew,FileAccess.Write,FileShare.None))JsonSerializer.Serialize(file,report,new JsonSerializerOptions{WriteIndented=true});
            File.Move(path+".partial",path);
            log.Info($"MAW ROPE MATERIAL COMPLETE: pass={pass}; cases={cases.Count}; checks={checks}; restored={restored}; historyKnown={report.historyKnown}; missing={report.historyMissing}; evidence={path}. No player-placement or ascent claim.");
            if(!pass)throw new InvalidOperationException(error??"Rope microcheck restoration/completion failed.");
        }
    }
}
