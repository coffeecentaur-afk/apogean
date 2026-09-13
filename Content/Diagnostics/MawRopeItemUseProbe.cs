using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using P = apogean.Content.Diagnostics.MawRopeItemUsePlan;
using S = apogean.Content.Diagnostics.MawShallowTraversalLab;

namespace apogean.Content.Diagnostics
{
    // Public input/item-check hooks only. Not direct placement or a new rope system.
    public sealed class MawRopeItemUseProbe : ModPlayer
    {
        private static MawRopeItemUseProbe owner;
        private Rectangle patch, guard;
        private Rectangle[] history;
        private S.CellState[] original;
        private Cell[] logical;
        private ItemState[] inventory;
        private string before, historyBefore, gear, variant;
        private Vector2 returnPosition, returnVelocity, start;
        private int worldId, slot, selectedBefore, startStack, startLife, count, controlled, preCalls, postCalls;
        private int returnFall, returnFall2, animationMax, timeMax, reuseDelay;
        private int oldTargetX, oldTargetY, aimedX, aimedY, nativeX, nativeY, nativeItem;
        private bool active, targetOverride, releaseBefore, occupied, targetRestorationFailed;
        private ulong targetUpdate;
        private long started;
        private Sample[] samples;
        private readonly record struct ItemState(int Type, int Stack, int Prefix)
        {
            internal static ItemState Read(Item item) => new(item.type, item.stack, item.prefix);
        }
        private readonly record struct Cell(bool Present, ushort Type, ushort Wall, byte Slope, bool Half,
            byte Liquid, int LiquidType, bool Red, bool Blue, bool Green, bool Yellow, bool Actuator,
            bool Actuated, byte Paint, byte WallPaint, bool Bright, bool Invisible, bool WallBright, bool WallInvisible)
        {
            internal static Cell Read(Tile t) => new(t.HasTile, t.TileType, t.WallType, (byte)t.Slope, t.IsHalfBlock,
                t.LiquidAmount, t.LiquidType, t.RedWire, t.BlueWire, t.GreenWire, t.YellowWire, t.HasActuator,
                t.IsActuated, t.TileColor, t.WallColor, t.IsTileFullbright, t.IsTileInvisible, t.IsWallFullbright, t.IsWallInvisible);
        }
        private readonly record struct Sample(int Tick, ulong Update, bool Use, int PreCalls, int PostCalls,
            int AimX, int AimY, int NativeX, int NativeY, int NativeItem, bool TargetPresent, int TargetType, int Stack, int Animation, int ItemTime,
            float X, float Y, int Life, int RangeX, int RangeY, int TileBoost, int BlockRange);
        internal bool Active => active;
        private Point Target => new(patch.X + P.TargetX(variant), patch.Y + P.TargetY);
        private Point Parking => new(patch.X + 2, patch.Y + 2);
        private int CellIndex(int x, int y) => (x-guard.X)*guard.Height+y-guard.Y;
        private bool Context => Player.whoAmI == Main.myPlayer && Main.worldID == worldId &&
            P.Context(Main.ActiveWorldFileData?.Name, Player.name, Main.netMode == NetmodeID.SinglePlayer, Main.gameMenu);
        private bool Interrupted => WorldGen.gen || Main.gamePaused || !Main.instance.IsActive || Main.playerInventory ||
            Main.drawingPlayerChat || Main.editSign || Player.talkNPC != -1 || Player.sign != -1 || CaptureManager.Instance.IsCapturing;
        private bool ModeChanged => Player.dead || Player.mount.Active || Player.gravDir != 1 || Player.pulley ||
            Player.grapCount != 0 || Player.width != 20 || Player.height != 42 || PlayerInput.ShouldFastUseItem ||
            Player.altFunctionUse!=0 || Player.controlTorch || Player.selectItemOnNextUse || Player.noItems || Player.noBuilding;
        private string Gear() => string.Join(";", Player.armor.Concat(Player.miscEquips).Select(i => ItemState.Read(i).ToString()));
        private bool InventoryUnchangedExceptRope()
        {
            for (int i=0; i<inventory.Length; i++) {
                ItemState now=ItemState.Read(Player.inventory[i]), old=inventory[i];
                if (i==slot) {
                    if (now.Type!=old.Type || now.Prefix!=old.Prefix || now.Stack>old.Stack || now.Stack<old.Stack-1) return false;
                } else if(now!=old) return false;
            }
            return true;
        }
        private bool ForeignActors()
        {
            Rectangle area=new(guard.X*16,guard.Y*16,guard.Width*16,guard.Height*16); area.Inflate(32,32);
            foreach(Player p in Main.ActivePlayers) if(p.whoAmI!=Player.whoAmI && area.Intersects(p.Hitbox)) return true;
            foreach(NPC n in Main.ActiveNPCs) if(area.Intersects(n.Hitbox)) return true;
            foreach(Projectile p in Main.ActiveProjectiles) if(area.Intersects(p.Hitbox)) return true;
            foreach(Item i in Main.ActiveItems) if(area.Intersects(i.Hitbox)) return true;
            return false;
        }
        internal void Start(string choice)
        {
            if(!P.Variant(choice)) throw new ArgumentException("Unknown item-use variant.");
            worldId=Main.worldID;
            if(!Context || active || owner!=null || Interrupted || ModeChanged || Player.channel || Player.itemAnimation!=0 || Player.itemTime!=0 || Player.reuseDelay!=0 ||
                !MawPackedPreview.Enabled || !MawAnatomyMaterials.Available || ModContent.GetInstance<QAPerformanceLab>().Recording ||
                ModContent.GetInstance<MawGrowthLoadStudy>().Active || Player.GetModPlayer<MawShallowMotionProbe>().Active || Player.GetModPlayer<MawRopeClimbProbe>().Active)
                throw new InvalidOperationException("Item-use probe requires idle gg/V3/SP and no other probe.");
            slot=Array.FindIndex(Player.inventory,0,10,i=>i.type==ItemID.Rope && i.stack>=2 && i.createTile==TileID.Rope);
            if(slot<0) throw new InvalidOperationException("No existing ordinary Rope stack in hotbar; nothing granted.");
            variant=choice; gear=Gear(); inventory=Player.inventory.Select(ItemState.Read).ToArray();
            history=S.Historical().Append(ModContent.GetInstance<S>().PreservedBounds).ToArray(); historyBefore=S.Fingerprint(history);
            patch=MawConversionLoadStudy.FindEmptyPatch(history); guard=patch; guard.Inflate(12,12);
            original=new S.CellState[guard.Width*guard.Height]; logical=new Cell[original.Length];
            for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) original[CellIndex(x,y)]=S.CellState.Read(x,y);
            before=S.Fingerprint(new[]{guard});
            returnPosition=Player.position; returnVelocity=Player.velocity; returnFall=Player.fallStart; returnFall2=Player.fallStart2;
            selectedBefore=Player.selectedItem; animationMax=Player.itemAnimationMax; timeMax=Player.itemTimeMax; reuseDelay=Player.reuseDelay; releaseBefore=Player.releaseUseItem;
            bool moved=false;
            try {
                int support=choice=="rib"?MawAnatomyMaterials.Tile("rib").Type:choice=="cap"?MawAnatomyMaterials.Tile("cap").Type:TileID.GrayBrick;
                for(int x=0;x<32;x++) for(int y=0;y<32;y++) {
                    Tile t=Main.tile[patch.X+x,patch.Y+y];
                    if(P.Floor(x,y)||P.Support(choice,x,y)) { t.HasTile=true; t.TileType=(ushort)(P.Floor(x,y)?TileID.GrayBrick:support); }
                    if(P.Wall(choice,x,y)) t.WallType=WallID.GrayBrick;
                }
                WorldGen.RangeFrame(patch.Left,patch.Top,patch.Right,patch.Bottom);
                for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) logical[CellIndex(x,y)]=Cell.Read(Main.tile[x,y]);
                if(Main.tile[Target].HasTile) throw new InvalidOperationException("Target already occupied; no auto-extension allowed.");
                start=new((patch.X+8)*16+8-10,(patch.Y+26)*16-42);
                if(Collision.SolidCollision(start,20,42)) throw new InvalidOperationException("Blocked setup body.");
                samples=new Sample[P.Updates]; count=controlled=preCalls=postCalls=0; occupied=targetOverride=targetRestorationFailed=false;
                startStack=Player.inventory[slot].stack; startLife=Player.statLife;
                Player.position=start; Player.velocity=Vector2.Zero; Player.fallStart=Player.fallStart2=(int)(start.Y/16); Player.selectedItem=slot; moved=true;
                started=Stopwatch.GetTimestamp(); active=true; owner=this;
                Mod.Logger.Info($"MAW ROPE ITEMUSE START: {choice}; patch={patch}; existing slot={slot}, stack={startStack};60updates/6seconds; no rope preplacement/grants/refunds.");
            } catch { RestoreTiles(); if(moved) RestorePlayer(); original=null; throw; }
        }
        private void Neutral() => Player.controlUp=Player.controlDown=Player.controlLeft=Player.controlRight=Player.controlJump=
            Player.controlHook=Player.controlMount=Player.controlUseItem=Player.controlUseTile=Player.controlThrow=false;
        private bool CanContinue() => Context && !Interrupted && !ModeChanged && Player.selectedItem==slot &&
            Gear()==gear && InventoryUnchangedExceptRope() && !ForeignActors() && Stopwatch.GetElapsedTime(started).TotalSeconds<6;
        public override void SetControls()
        {
            if(!active) return;
            if(targetOverride) { Cancel("previous-itemcheck-did-not-return"); return; }
            if(!CanContinue()) { Cancel("context-inventory-actor-timeout"); return; }
            Neutral(); occupied=Main.tile[Target].HasTile;
            Player.controlUseItem=P.Use(controlled,occupied); controlled++;
        }
        public override bool PreItemCheck()
        {
            if(!active) return true;
            if(targetOverride) { Cancel("nested-itemcheck-target"); return true; }
            if(!CanContinue()) { Cancel("precheck-context"); return true; }
            // Keep normal timer/placement code running. Never point a continuing
            // animation at existing rope (which could extend outside its target).
            if(controlled==2 && (Player.itemAnimation!=0 || Player.itemTime!=0 || Player.reuseDelay!=0 || Main.tile[Target].HasTile))
                { Cancel("pulse-not-ready"); return true; }
            Point aim=controlled==2?Target:Parking;
            if(aim==Parking) for(int x=aim.X-1;x<=aim.X+1;x++) for(int y=aim.Y-1;y<=aim.Y+1;y++)
                if(!S.Empty(Main.tile[x,y])) { Cancel("parking-not-empty"); return true; }
            oldTargetX=Terraria.Player.tileTargetX; oldTargetY=Terraria.Player.tileTargetY; targetOverride=true; targetUpdate=Main.GameUpdateCount;
            Terraria.Player.tileTargetX=aim.X; Terraria.Player.tileTargetY=aim.Y;
            aimedX=aim.X-patch.X; aimedY=aim.Y-patch.Y; preCalls++;
            return true;
        }
        private void RestoreTarget()
        {
            if(!targetOverride) return;
            if(Context && targetUpdate==Main.GameUpdateCount) {
                Terraria.Player.tileTargetX=oldTargetX; Terraria.Player.tileTargetY=oldTargetY;
            } else targetRestorationFailed=true; // Never replay a stale target into another update/world.
            targetOverride=false;
        }
        public override void PostItemCheck()
        {
            if(!targetOverride) return;
            try {
                postCalls++; nativeX=Terraria.Player.tileTargetX-patch.X; nativeY=Terraria.Player.tileTargetY-patch.Y;
                nativeItem=Player.inventory[Player.selectedItem].type;
            } finally { RestoreTarget(); }
            if(nativeX!=aimedX || nativeY!=aimedY || nativeItem!=ItemID.Rope) Cancel("native-retarget-or-selection");
        }
        public override void PostUpdate()
        {
            if(!active || controlled<=count) return;
            if(targetOverride) { Cancel("itemcheck-did-not-return"); return; }
            if(!CanContinue()) { Cancel("postcheck-context"); return; }
            Tile target=Main.tile[Target]; Vector2 local=Player.position-patch.Location.ToVector2()*16;
            samples[count]=new(count,Main.GameUpdateCount,Player.controlUseItem,preCalls,postCalls,aimedX,aimedY,nativeX,nativeY,nativeItem,target.HasTile,target.TileType,
                Player.inventory[slot].stack,Player.itemAnimation,Player.itemTime,local.X,local.Y,Player.statLife,
                Terraria.Player.tileRangeX,Terraria.Player.tileRangeY,Player.inventory[slot].tileBoost,Player.blockRange); count++;
            if(Player.statLife<startLife || Math.Abs(Player.position.X-start.X)>4 || Math.Abs(Player.position.Y-start.Y)>4) { Cancel("damage-or-body-motion"); return; }
            if(count==P.Updates) Finish("complete");
        }
        internal void Cancel(string why) { if(active) Finish(why); }
        internal static void StopActive(string why) => owner?.Cancel(why);
        internal static void CheckActive()
        {
            // UI is outside ItemCheck; a still-owned override means its normal
            // post hook was skipped. Restore first, retain an incomplete run.
            if(owner!=null && (owner.targetOverride || !owner.CanContinue())) owner.Cancel("ui-context-or-skipped-posthook");
        }
        private void RestoreTiles()
        {
            if(original==null) return;
            for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) original[CellIndex(x,y)].Restore(x,y);
        }
        private void RestorePlayer()
        {
            // Teardown only; no sampled motion/timer/consumption is fabricated.
            RestoreTarget(); Neutral(); Player.position=returnPosition; Player.velocity=returnVelocity;
            Player.fallStart=returnFall; Player.fallStart2=returnFall2; Player.selectedItem=selectedBefore;
            Player.itemAnimation=Player.itemTime=0; Player.itemAnimationMax=animationMax; Player.itemTimeMax=timeMax;
            Player.reuseDelay=reuseDelay; Player.releaseUseItem=releaseBefore;
        }
        private void Finish(string reason)
        {
            if(!active) return; active=false; owner=null; RestoreTarget(); Neutral();
            int endStack=Player.inventory[slot].stack, endAnimation=Player.itemAnimation, endItemTime=Player.itemTime;
            bool inventoryOk=InventoryUnchangedExceptRope() && Gear()==gear, restored=false, targetRope=false;
            string error=null, after=null, historyAfter=null; int ropeCells=0, collateral=0;
            bool same=Main.worldID==worldId && Main.ActiveWorldFileData?.Name==MawShallowQaScope.World;
            try {
                if(!same) throw new InvalidOperationException("World changed; no scratch write into another world.");
                targetRope=Main.tile[Target].HasTile && Main.tile[Target].TileType==TileID.Rope;
                for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) {
                    Tile t=Main.tile[x,y]; Cell now=Cell.Read(t), wanted=logical[CellIndex(x,y)];
                    if(t.HasTile && t.TileType==TileID.Rope) ropeCells++;
                    if(x==Target.X && y==Target.Y && targetRope) wanted=wanted with {Present=true,Type=TileID.Rope};
                    if(now!=wanted) collateral++;
                }
            } catch(Exception ex) { error=ex.Message; }
            finally {
                if(same) try {
                    RestoreTiles(); after=S.Fingerprint(new[]{guard}); restored=before==after;
                    for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) restored&=original[CellIndex(x,y)]==S.CellState.Read(x,y);
                    historyAfter=S.Fingerprint(history);
                } catch(Exception ex) { error=(error??"")+" Restore: "+ex.Message; }
                if(same) RestorePlayer();
            }
            try {
                var rows=samples.AsSpan(0,count).ToArray();
                bool traceOk=count==60;
                for(int i=0;i<rows.Length;i++) {
                    Sample s=rows[i];
                    traceOk &= s.Tick==i && (i==0 || s.Update==rows[i-1].Update+1) && s.Use==(i==1) &&
                        s.PreCalls==i+1 && s.PostCalls==i+1 && s.AimX==(i==1?P.TargetX(variant):2) && s.AimY==(i==1?23:2) &&
                        s.NativeX==s.AimX && s.NativeY==s.AimY && s.NativeItem==ItemID.Rope;
                }
                bool expected=P.ExpectedPlacement(variant), pass=error==null && reason=="complete" && count==60 && controlled==60 &&
                    traceOk && rows[1].Animation>0 && preCalls==60 && postCalls==60 && endAnimation==0 && endItemTime==0 && !targetRestorationFailed &&
                    inventoryOk && Player.statLife>=startLife && collateral==0 &&
                    restored && historyBefore==historyAfter && targetRope==expected && ropeCells==(expected?1:0) && startStack-endStack==(expected?1:0);
                var report=new {schemaVersion=1,utc=DateTime.UtcNow,reason,error,pass,variant,expected,targetRope,ropeCells,collateral,
                    player=Player.name,worldId,slot,startStack,endStack,inventoryOk,startLife,endLife=Player.statLife,count,controlled,preCalls,postCalls,traceOk,
                    endAnimation,endItemTime,targetRestored=!targetOverride&&!targetRestorationFailed,selectedRestored=Player.selectedItem==selectedBefore,
                    patch=new{patch.X,patch.Y,patch.Width,patch.Height},before,after,restored,historyBefore,historyAfter,
                    historyKnown=history.Count(r=>r.Width>0&&r.Height>0),historyMissing=history.Count(r=>r.Width<=0||r.Height<=0),
                    elapsedMs=Stopwatch.GetElapsedTime(started).TotalMilliseconds,samples=rows,
                    scope="Scripted target around public native ItemCheck with existing QA rope consumed, no refunds. Native timers/placement/consumption during samples; setup relocation and teardown timer/slot restoration excluded. Not manual use, starter reach, full-route return, multiplayer or production placement proof."};
                string directory=Path.Combine(Main.SavePath,"Captures"); Directory.CreateDirectory(directory);
                string path=Path.Combine(directory,"maw-rope-itemuse-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+variant+".json");
                using(var f=new FileStream(path+".partial",FileMode.CreateNew,FileAccess.Write,FileShare.None)) JsonSerializer.Serialize(f,report,new JsonSerializerOptions{WriteIndented=true});
                File.Move(path+".partial",path);
                Mod.Logger.Info($"MAW ROPE ITEMUSE COMPLETE: {variant}; pass={pass}; count={count}; consumed={startStack-endStack}; rope={ropeCells}; restored={restored}; evidence={path}.");
            } catch(Exception ex) { Mod.Logger.Error("MAW ROPE ITEMUSE EXPORT FAILED: no certified result.",ex); }
            finally { original=null;logical=null;inventory=null;history=null;samples=null; }
        }
    }
    public sealed class MawRopeItemUseLifetime : ModSystem
    {
        public override void UpdateUI(GameTime gameTime) => MawRopeItemUseProbe.CheckActive();
        public override void OnWorldUnload() => MawRopeItemUseProbe.StopActive("world-unload");
    }
}
