using System;
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
    // Separate native movement experiment; does not weaken Plain's jump baseline.
    public sealed class MawRopeClimbProbe : ModPlayer
    {
        private static MawRopeClimbProbe owner;
        private Rectangle patch,guard;
        private Rectangle[] history;
        private S.CellState[] original;
        private string emptyBefore,geometryBefore,historyBefore,variant;
        private Vector2 returnPosition,returnVelocity,start;
        private int returnFall,returnFall2,worldId,startLife,count,controlled;
        private long started;
        private Sample[] samples;
        private bool active;
        private readonly record struct Sample(int Tick,float X,float Y,float Vx,float Vy,bool Up,bool Down,bool Jump,bool Pulley,int PulleyDir,int RopeCount,int Life);
        internal bool Active=>active;
        private bool Context=>Player.whoAmI==Main.myPlayer&&MawRopeClimbScope.Context(Main.ActiveWorldFileData?.Name,Player.name,Main.netMode==NetmodeID.SinglePlayer,Main.gameMenu)&&Main.worldID==worldId;
        private static bool Starter(Player p){
            if(p.name!=MawRopeClimbScope.PlayerName||p.difficulty!=0||p.statLifeMax!=100||p.statLifeMax2!=100||p.statManaMax!=20||p.wingsLogic!=0)return false;
            foreach(Item i in p.armor)if(!i.IsAir)return false;
            foreach(Item i in p.miscEquips)if(!i.IsAir)return false;
            foreach(Item i in p.inventory)if(!i.IsAir&&i.type is not (ItemID.CopperShortsword or ItemID.CopperPickaxe or ItemID.CopperAxe))return false;
            for(int i=0;i<p.buffType.Length;i++)if(p.buffType[i]!=0&&p.buffTime[i]>0)return false;
            return true;
        }
        private bool Interrupted=>Main.gamePaused||!Main.instance.IsActive||Main.playerInventory||Main.drawingPlayerChat||Main.editSign||Player.talkNPC!=-1||Player.sign!=-1;
        private bool ModeChanged=>Player.dead||Player.mount.Active||Player.gravDir!=1||Player.grapCount!=0||Player.width!=20||Player.height!=42||!Starter(Player);
        internal void Start(string choice){
            if(!MawRopeClimbScope.Variant(choice))throw new ArgumentException("Unknown rope comparison.");
            worldId=Main.worldID;
            if(!Context||owner!=null||active||Interrupted||ModeChanged||Player.pulley||Player.ropeCount!=0||Player.channel||Player.itemAnimation>0||
                !MawPackedPreview.Enabled||!MawAnatomyMaterials.Available||ModContent.GetInstance<QAPerformanceLab>().Recording||Player.GetModPlayer<MawShallowMotionProbe>().Active)
                throw new InvalidOperationException("Rope climb requires idle unmodified Classic Maw QA Rope/V3, no overlapping probe or movement mode.");
            variant=choice;history=S.Historical().Append(ModContent.GetInstance<S>().PreservedBounds).ToArray();historyBefore=S.Fingerprint(history);
            patch=MawConversionLoadStudy.FindEmptyPatch(history);guard=patch;guard.Inflate(12,12);
            original=new S.CellState[guard.Width*guard.Height];
            for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)original[CellIndex(x,y)]=S.CellState.Read(x,y);
            emptyBefore=S.Fingerprint(new[]{guard});
            returnPosition=Player.position;returnVelocity=Player.velocity;returnFall=Player.fallStart;returnFall2=Player.fallStart2;
            bool moved=false;
            try{
                void Place(int x,int y,int type){
                    x+=patch.X;y+=patch.Y;
                    if(!S.Empty(Main.tile[x,y]))throw new InvalidOperationException("Nonempty rope target; no overwrite.");
                    WorldGen.PlaceTile(x,y,type,mute:true,forced:false,plr:Player.whoAmI);
                    if(!Main.tile[x,y].HasTile||Main.tile[x,y].TileType!=type)throw new InvalidOperationException("Rope fixture placement failed.");
                }
                for(int x=12;x<=20;x++)Place(x,26,TileID.GrayBrick);
                Place(16,5,choice=="vanilla"?TileID.GrayBrick:MawAnatomyMaterials.Tile("rib").Type);
                if(choice!="none")for(int y=6;y<=25;y++)Place(16,y,TileID.Rope);
                for(int x=patch.Left;x<patch.Right;x++)for(int y=patch.Top;y<patch.Bottom;y++)WorldGen.TileFrame(x,y);
                geometryBefore=S.Fingerprint(new[]{guard});
                start=new((patch.X+16)*16+8-10,(patch.Y+26)*16-42);
                if(Collision.SolidCollision(start,20,42))throw new InvalidOperationException("Rope body clearance failed.");
                samples=new Sample[210];count=controlled=0;startLife=Player.statLife;started=Stopwatch.GetTimestamp();
                // Setup-only relocation. All subsequent measured motion belongs to Terraria.
                Player.position=start;Player.velocity=Vector2.Zero;Player.fallStart=Player.fallStart2=(int)(start.Y/16);moved=true;
                active=true;owner=this;
                Mod.Logger.Info($"MAW ROPE CLIMB START: {choice}; player={Player.name}; patch={patch}; max210 updates/8s; setup relocation only, no physics writes during measurement; no item grants.");
            }catch{
                RestoreTiles();original=null;
                if(moved)RestorePlayer();
                throw;
            }
        }
        private int CellIndex(int x,int y)=>(x-guard.X)*guard.Height+y-guard.Y;
        private void RestoreTiles(){if(original!=null)for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)original[CellIndex(x,y)].Restore(x,y);}
        private void RestorePlayer(){
            // Teardown only, after evidence sampling stops; never counted as movement.
            Player.position=returnPosition;Player.velocity=returnVelocity;Player.fallStart=returnFall;Player.fallStart2=returnFall2;Player.pulley=false;
        }
        private void Neutral(){Player.controlUp=Player.controlDown=Player.controlLeft=Player.controlRight=Player.controlJump=Player.controlHook=Player.controlMount=Player.controlUseItem=Player.controlUseTile=Player.controlThrow=false;}
        public override void SetControls(){
            if(!active)return;
            if(!Context||Interrupted||ModeChanged||Stopwatch.GetElapsedTime(started).TotalSeconds>8){Cancel("context-mode-pause-timeout");return;}
            Neutral();Player.controlUp=MawRopeClimbScope.Up(controlled);controlled++;
        }
        public override void PostUpdate(){
            if(!active)return;
            if(!Context||ModeChanged||count>=samples.Length){Cancel("context-mode-or-budget");return;}
            if(controlled<=count)return; // Start may run in another ModPlayer's PostUpdate.
            Vector2 local=Player.position-patch.Location.ToVector2()*16;
            samples[count]=new(count,local.X,local.Y,Player.velocity.X,Player.velocity.Y,Player.controlUp,Player.controlDown,Player.controlJump,Player.pulley,Player.pulleyDir,Player.ropeCount,Player.statLife);count++;
            if(local.X<11*16||local.X>21*16||local.Y<2*16||local.Y>27*16||Player.statLife<startLife){Cancel("envelope-or-damage");return;}
            if(MawRopeClimbScope.Complete(count))Finish("complete");
        }
        internal void Cancel(string why){if(active)Finish(why);}
        internal static void StopActive(string why)=>owner?.Cancel(why);
        internal static void CheckActive(){if(owner!=null&&(!owner.Context||owner.Interrupted||Stopwatch.GetElapsedTime(owner.started).TotalSeconds>8))owner.Cancel("ui-context-pause-timeout");}
        private void Finish(string reason){
            if(!active)return;active=false;owner=null;Neutral();
            bool sameWorld=Main.worldID==worldId&&Main.ActiveWorldFileData?.Name==MawShallowQaScope.World;
            string geometryAfter=null,emptyAfter=null,historyAfter=null,error=null;bool unchanged=false,restored=false,historyUnchanged=false;
            // Cleanup precedes serialization and survives a failed observation.
            try{if(sameWorld){geometryAfter=S.Fingerprint(new[]{guard});unchanged=geometryAfter==geometryBefore;}}
            catch(Exception ex){error="Observation: "+ex.Message;}
            finally{
                if(sameWorld){
                    try{
                        RestoreTiles();emptyAfter=S.Fingerprint(new[]{guard});restored=emptyBefore==emptyAfter;
                        for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)restored&=original[CellIndex(x,y)]==S.CellState.Read(x,y);
                        historyAfter=S.Fingerprint(history);historyUnchanged=historyBefore==historyAfter;
                    }catch(Exception ex){error=(error??"")+" Restore: "+ex.Message;}
                    finally{RestorePlayer();}
                }
            }
            try{
                var rows=samples.AsSpan(0,count).ToArray();
                int pulleyTicks=rows.Count(s=>s.Pulley),upwardPulleyTicks=rows.Count(s=>s.Pulley&&s.Vy<-.1f);
                float rise=count==0?0:start.Y-(rows.Min(s=>s.Y)+patch.Y*16);
                bool attached=pulleyTicks>0,ascended=pulleyTicks>=30&&upwardPulleyTicks>=30&&rise>=128;
                bool baseline=Starter(Player);int endLife=Player.statLife;
                bool complete=error==null&&reason=="complete"&&count==210&&controlled==210&&baseline&&endLife>=startLife&&unchanged&&restored&&historyUnchanged;
                bool comparisonPass=complete&&(variant=="none"?!attached&&!ascended&&Math.Abs(rise)<1:ascended);
                var report=new{schemaVersion=1,utc=DateTime.UtcNow,error,reason,variant,comparisonPass,complete,attached,ascended,pulleyTicks,upwardPulleyTicks,rise,
                    controlled,baseline,startLife,endLife,worldId,player=Player.name,patch=new{patch.X,patch.Y,patch.Width,patch.Height},geometryBefore,geometryAfter,emptyBefore,emptyAfter,historyBefore,historyAfter,unchanged,restored,historyUnchanged,
                    historyKnown=history.Count(r=>r.Width>0&&r.Height>0),historyMissing=history.Count(r=>r.Width<=0||r.Height<=0),elapsedMs=Stopwatch.GetElapsedTime(started).TotalMilliseconds,samples=rows,
                    scope="Native input-driven attachment/ascent on preplaced ordinary rope. Setup/teardown relocate only outside samples. No item-use placement, top exit, manual play, whole-route returnability, natural-light art, multiplayer or production rope generation proof."};
                string directory=Path.Combine(Main.SavePath,"Captures");Directory.CreateDirectory(directory);
                string path=Path.Combine(directory,"maw-rope-climb-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+variant+".json");
                using(var f=new FileStream(path+".partial",FileMode.CreateNew,FileAccess.Write,FileShare.None))JsonSerializer.Serialize(f,report,new JsonSerializerOptions{WriteIndented=true});
                File.Move(path+".partial",path);
                Mod.Logger.Info($"MAW ROPE CLIMB COMPLETE: {variant}; complete={complete}; comparisonPass={comparisonPass}; attached={attached}; rise={rise}; restored={restored}; evidence={path}.");
            }catch(Exception ex){Mod.Logger.Error("MAW ROPE CLIMB EXPORT OR RESTORE FAILED: no certified result.",ex);}
            finally{original=null;samples=null;history=null;}
        }
        public override void Initialize(){active=false;original=null;samples=null;}
    }
    public sealed class MawRopeClimbLifetime : ModSystem
    {
        public override void UpdateUI(GameTime gameTime)=>MawRopeClimbProbe.CheckActive();
        public override void OnWorldUnload()=>MawRopeClimbProbe.StopActive("world-unload");
    }
}
