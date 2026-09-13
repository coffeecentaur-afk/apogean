using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using P = apogean.Content.Diagnostics.MawFiberGrowthPolicy;
using S = apogean.Content.Diagnostics.MawShallowTraversalLab;

namespace apogean.Content.Diagnostics
{
    // Explicit finite QA updates, not a production infection/growth scheduler.
    public sealed class MawGrowthLoadStudy : ModSystem
    {
        private Rectangle patch, guard;
        private Rectangle[] history;
        private S.CellState[] original;
        private P.Cell[,] expected, cells;
        private string before, historyBefore;
        private int budget, count, soil, grass, bone, worldId;
        private long started;
        private ulong previousUpdate;
        private bool active;
        private object failedStep;
        private readonly List<Sample> samples = new(MawGrowthLoadPlan.Updates);
        private readonly record struct Sample(int Tick, ulong GameUpdate, int Changed, int Grass,
            double WorkMs, long WorkAllocatedBytes, double ValidationMs);
        internal bool Active => active;
        private bool Context => MawGrowthLoadPlan.Context(Main.ActiveWorldFileData?.Name,
            Main.LocalPlayer.name, Main.netMode == NetmodeID.SinglePlayer, Main.gameMenu) && Main.worldID == worldId;
        private bool Interrupted => WorldGen.gen || Main.gamePaused || !Main.instance.IsActive ||
            Main.LocalPlayer.dead || Main.playerInventory || Main.drawingPlayerChat || Main.editSign ||
            Main.LocalPlayer.talkNPC != -1 || CaptureManager.Instance.IsCapturing;
        private int Index(int x, int y) => (x-guard.X)*guard.Height+y-guard.Y;
        private Tile At(int x, int y) => Main.tile[patch.X+x, patch.Y+y];
        private int Type(P.Host host) => host switch { P.Host.Soil => soil, P.Host.Grass => grass, P.Host.Other => bone, _ => 0 };
        private static int Slope(int x, int y) => (x,y) switch { (2,2)=>1, (28,2)=>2, (2,28)=>3, (28,28)=>4, _=>0 };
        private static bool Half(int x, int y) => x==2 && y==8;
        private void RestoreTiles()
        {
            if(original==null) return;
            for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) original[Index(x,y)].Restore(x,y);
        }
        internal void Start(int requestedBudget)
        {
            if(!MawGrowthLoadPlan.Budget(requestedBudget)) throw new ArgumentOutOfRangeException(nameof(requestedBudget));
            worldId=Main.worldID;
            if(active || !Context || Interrupted || Main.LocalPlayer.channel || Main.LocalPlayer.itemAnimation>0 ||
                !MawPackedPreview.Enabled || ModContent.GetInstance<QAPerformanceLab>().Recording ||
                Main.LocalPlayer.GetModPlayer<MawShallowMotionProbe>().Active || Main.LocalPlayer.GetModPlayer<MawRopeClimbProbe>().Active)
                throw new InvalidOperationException("Growth load requires idle packed gg/V3/SP without another probe.");
            budget=requestedBudget; count=0; failedStep=null; samples.Clear();
            soil=MawPackedPreview.TileType("soil"); grass=MawPackedPreview.TileType("grass"); bone=MawPackedPreview.TileType("bone");
            history=S.Historical().Append(ModContent.GetInstance<S>().PreservedBounds).ToArray(); historyBefore=S.Fingerprint(history);
            patch=MawConversionLoadStudy.FindEmptyPatch(history); guard=patch; guard.Inflate(12,12);
            original=new S.CellState[guard.Width*guard.Height];
            for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) original[Index(x,y)]=S.CellState.Read(x,y);
            before=S.Fingerprint(new[]{guard}); expected=MawGrowthLoadPlan.Seed(); cells=new P.Cell[32,32];
            try {
                for(int x=0;x<32;x++) for(int y=0;y<32;y++) {
                    P.Host host=expected[x,y].Material; if(host==P.Host.Air) continue;
                    Tile t=At(x,y); t.HasTile=true; t.TileType=(ushort)Type(host);
                    t.Slope=(SlopeType)Slope(x,y); t.IsHalfBlock=Half(x,y); t.BlueWire=true;
                    t.TileColor=PaintID.BluePaint; t.IsTileFullbright=(x+y)%3==0;
                }
                WorldGen.RangeFrame(patch.Left-1,patch.Top-1,patch.Right+1,patch.Bottom+1);
                Validate(); previousUpdate=Main.GameUpdateCount; started=Stopwatch.GetTimestamp(); active=true;
                Mod.Logger.Info($"QA PERFORMANCE GROWTH START: budget={budget}; patch={patch}; 240updates/8seconds, one1024-cell observation per update, native framing; no player or production edits.");
            } catch { RestoreTiles(); original=null; throw; }
        }
        private int Validate()
        {
            int total=0;
            for(int x=0;x<32;x++) for(int y=0;y<32;y++) {
                Tile t=At(x,y); P.Host host=expected[x,y].Material;
                if(t.HasTile!=(host!=P.Host.Air) || (t.HasTile && (t.TileType!=Type(host) || (byte)t.Slope!=Slope(x,y) || t.IsHalfBlock!=Half(x,y) ||
                    !t.BlueWire || t.TileColor!=PaintID.BluePaint || t.IsTileFullbright!=((x+y)%3==0))) ||
                    t.WallType!=WallID.None || t.LiquidAmount!=0 || t.RedWire || t.GreenWire || t.YellowWire || t.HasActuator || t.IsActuated ||
                    t.IsTileInvisible || t.WallColor!=PaintID.None || t.IsWallFullbright || t.IsWallInvisible)
                    throw new InvalidOperationException($"Growth native state mismatch at{x},{y}; no repair.");
                if(t.HasTile && t.TileType==grass) total++;
            }
            return total;
        }
        public override void PostUpdateWorld()
        {
            if(!active) return;
            if(!Context || Interrupted || !S.ActorsAbsent(guard) || Stopwatch.GetElapsedTime(started).TotalSeconds>8) { Cancel("context-actor-pause-timeout"); return; }
            ulong update=Main.GameUpdateCount;
            if(update==previousUpdate) return; // Never two budgets in one game update.
            if(count>0 && update!=previousUpdate+1) { Cancel("missed-game-update"); return; }
            previousUpdate=update;
            long stepStarted=Stopwatch.GetTimestamp();
            try {
                long allocation=GC.GetAllocatedBytesForCurrentThread(), begin=Stopwatch.GetTimestamp();
                for(int x=0;x<32;x++) for(int y=0;y<32;y++) {
                    Tile t=At(x,y);
                    P.Host host=!t.HasTile?P.Host.Air:t.TileType==soil?P.Host.Soil:t.TileType==grass?P.Host.Grass:P.Host.Other;
                    cells[x,y]=new(host,t.IsActuated,t.Slope!=SlopeType.Solid || t.IsHalfBlock);
                }
                var pending=P.Plan(cells,budget,budget!=0);
                foreach(var p in pending) At(p.X,p.Y).TileType=(ushort)grass;
                foreach(var p in pending) WorldGen.SquareTileFrame(patch.X+p.X,patch.Y+p.Y);
                double workMs=Stopwatch.GetElapsedTime(begin).TotalMilliseconds;
                long allocated=GC.GetAllocatedBytesForCurrentThread()-allocation;
                begin=Stopwatch.GetTimestamp();
                if(pending.Count>budget) throw new InvalidOperationException("Growth budget exceeded.");
                foreach(var p in pending) {
                    if(!MawGrowthLoadPlan.IsMatureGrass(p.X,p.Y) || expected[p.X,p.Y].Material!=P.Host.Soil)
                        throw new InvalidOperationException("Invalid/non-exposed growth target.");
                    expected[p.X,p.Y]=new(P.Host.Grass);
                }
                int total=Validate();
                samples.Add(new(count,update,pending.Count,total,workMs,allocated,Stopwatch.GetElapsedTime(begin).TotalMilliseconds)); count++;
                if(count==MawGrowthLoadPlan.Updates) Finish("complete");
            } catch(Exception ex) {
                failedStep=new{tick=count,gameUpdate=update,elapsedIncludingValidationMs=Stopwatch.GetElapsedTime(stepStarted).TotalMilliseconds};
                Finish("native-error",ex.GetType().Name+": "+ex.Message);
            }
        }
        internal void Cancel(string reason) { if(active) Finish(reason); }
        public override void UpdateUI(GameTime gameTime)
        {
            if(active && (!Context || Interrupted || Stopwatch.GetElapsedTime(started).TotalSeconds>8)) Cancel("ui-context-pause-timeout");
        }
        public override void OnWorldUnload() => Cancel("world-unload");
        private void Finish(string reason,string error=null)
        {
            if(!active) return; active=false;
            string after=null,historyAfter=null; bool restored=false;
            double measuredElapsed=Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            try {
                if(Main.worldID!=worldId || Main.ActiveWorldFileData?.Name!=MawShallowQaScope.World)
                    throw new InvalidOperationException("World changed; refusing scratch write into another world.");
                RestoreTiles(); after=S.Fingerprint(new[]{guard}); restored=before==after;
                for(int x=guard.Left;x<guard.Right;x++) for(int y=guard.Top;y<guard.Bottom;y++) restored&=original[Index(x,y)]==S.CellState.Read(x,y);
                historyAfter=S.Fingerprint(history);
            } catch(Exception ex) { error=(error??"")+" Cleanup: "+ex.Message; }
            try {
                Sample[] rows=samples.ToArray(); int converted=rows.Sum(s=>s.Changed),finalGrass=rows.Length==0?4:rows[^1].Grass;
                bool pass=error==null && reason=="complete" && count==240 && restored && historyBefore==historyAfter &&
                    converted==(budget==0?0:156) && finalGrass==(budget==0?4:160);
                object Describe(Sample[] set) => new { count=set.Length,work=QASampleStatistics.Describe(set.Select(s=>s.WorkMs).ToArray(),set.Length),
                    validation=QASampleStatistics.Describe(set.Select(s=>s.ValidationMs).ToArray(),set.Length), allocatedBytes=set.Sum(s=>s.WorkAllocatedBytes) };
                var report=new {schemaVersion=1,utc=DateTime.UtcNow,reason,error,failedStep,pass,budget,updates=count,converted,initialGrass=4,finalGrass,
                    worldId,player=Main.LocalPlayer.name,patch=new{patch.X,patch.Y,patch.Width,patch.Height},
                    before,after,restored,historyBefore,historyAfter,historyUnchanged=historyBefore==historyAfter,
                    historyKnown=history.Count(r=>r.Width>0&&r.Height>0),historyMissing=history.Count(r=>r.Width<=0||r.Height<=0),
                    measuredElapsedMs=measuredElapsed,samples=rows,active=Describe(rows.Where(s=>s.Changed>0).ToArray()),idle=Describe(rows.Where(s=>s.Changed==0).ToArray()),
                    scope="One bounded native growth plan per actual game update over1024 observed cells. Work includes read/plan/apply/frame, not setup/validation/cleanup/export. Allocations are per-thread work only. Not production spread speed, full-world scheduler, render/frame/GPU time, visual, low-end or multiplayer proof."};
                string directory=Path.Combine(Main.SavePath,"Captures","ApogeanPerformance"); Directory.CreateDirectory(directory);
                string path=Path.Combine(directory,"growth-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-b"+budget+".json");
                using(var file=new FileStream(path+".partial",FileMode.CreateNew,FileAccess.Write,FileShare.None)) JsonSerializer.Serialize(file,report,new JsonSerializerOptions{WriteIndented=true});
                File.Move(path+".partial",path);
                Mod.Logger.Info($"QA PERFORMANCE GROWTH COMPLETE: pass={pass}; budget={budget}; updates={count}; converted={converted}; restored={restored}; evidence={path}.");
            } catch(Exception ex) { Mod.Logger.Error("QA PERFORMANCE EXPORT FAILED: growth evidence incomplete.",ex); }
            finally { original=null; expected=cells=null; history=null; samples.Clear(); }
        }
    }
}
