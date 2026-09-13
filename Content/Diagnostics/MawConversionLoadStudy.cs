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
using Terraria.WorldBuilding;
using apogean.Content.Tiles;
using apogean.Content.Walls;
using apogean.Content.World;

namespace apogean.Content.Diagnostics
{
    // Synchronous throughput probe. No production scheduler/spread/generation edits.
    internal static class MawConversionLoadStudy
    {
        private readonly record struct Family(string Name,int Pure,int Maw,int Waste,int PureWall,int MawWall,int WasteWall);
        private static Family[] Families() => new[] {
            new Family("soil",TileID.Dirt,ModContent.TileType<MawDirt>(),ModContent.TileType<WastesSoil>(),WallID.DirtUnsafe,ModContent.WallType<MawDirtWallUnsafe>(),ModContent.WallType<WastesDirtWallUnsafe>()),
            new Family("stone",TileID.Stone,ModContent.TileType<Mawstone>(),ModContent.TileType<WastesStone>(),WallID.Stone,ModContent.WallType<MawStoneWallUnsafe>(),ModContent.WallType<WastesStoneWallUnsafe>()),
            new Family("ice",TileID.IceBlock,ModContent.TileType<MawIce>(),ModContent.TileType<WastesIce>(),WallID.IceUnsafe,ModContent.WallType<MawIceWallUnsafe>(),ModContent.WallType<WastesIceWallUnsafe>()),
            new Family("mud",TileID.Mud,ModContent.TileType<MawMud>(),ModContent.TileType<WastesMud>(),WallID.MudUnsafe,ModContent.WallType<MawMudWallUnsafe>(),ModContent.WallType<WastesMudWallUnsafe>())
        };
        internal static Rectangle FindEmptyPatch(Rectangle[] history)
        {
            foreach(int dx in new[]{-2800,-2520,-1960,-1680}) foreach(int dy in new[]{-660,-540,-420,-300}) {
                Rectangle patch=new(Main.spawnTileX+dx,Main.spawnTileY+dy,32,32),guard=patch;guard.Inflate(12,12);
                if(guard.Left<40||guard.Top<40||guard.Right>Main.maxTilesX-40||guard.Bottom>Main.maxTilesY-40)continue;
                if(history.Any(old=> { if(old.IsEmpty)return false;old.Inflate(24,24);return old.Intersects(guard); }))continue;
                if(!MawShallowTraversalLab.ActorsAbsent(guard)||GenVars.structures!=null&&!GenVars.structures.CanPlace(guard))continue;
                bool empty=true;
                for(int x=guard.Left;x<guard.Right&&empty;x++)for(int y=guard.Top;y<guard.Bottom;y++)
                    if(!MawShallowTraversalLab.Empty(Main.tile[x,y])){empty=false;break;}
                if(empty)return patch;
            }
            throw new InvalidOperationException("No empty conversion-load envelope; nothing cleared.");
        }
        internal static void Run(int side, log4net.ILog log)
        {
            if(side is not (16 or 32))throw new ArgumentOutOfRangeException(nameof(side));
            if(Main.gameMenu||WorldGen.gen||Main.netMode!=NetmodeID.SinglePlayer||Main.ActiveWorldFileData?.Name!=MawShallowQaScope.World||
                Main.LocalPlayer.name!="gg"||Main.LocalPlayer.dead||Main.LocalPlayer.channel||!MawPackedPreview.Enabled||ModContent.GetInstance<QAPerformanceLab>().Recording)
                throw new InvalidOperationException("Conversion-load probe requires idle packed gg/V3/SP.");
            var history=MawShallowTraversalLab.Historical().Append(ModContent.GetInstance<MawShallowTraversalLab>().PreservedBounds).ToArray();
            string oldHistory=MawShallowTraversalLab.Fingerprint(history);
            Rectangle patch=FindEmptyPatch(history);patch.Width=patch.Height=side;Rectangle guard=patch;guard.Inflate(12,12);
            var original=new MawShallowTraversalLab.CellState[guard.Width*guard.Height];
            int Index(int x,int y)=>(x-guard.X)*guard.Height+y-guard.Y;
            for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)original[Index(x,y)]=MawShallowTraversalLab.CellState.Read(x,y);
            string before=MawShallowTraversalLab.Fingerprint(new[]{guard});
            var families=Families();var batches=QABatchPlan.Create(side*side,32);var phases=new List<object>();
            int checks=0,completedCells=0,negativeControls=0;bool restored=false,historyUnchanged=false;string error=null;
            var total=Stopwatch.StartNew();
            void Check(bool yes,string name){if(!yes)throw new InvalidOperationException("Conversion-load: "+name);checks++;}
            Point PointAt(int n)=>new(patch.X+n/side,patch.Y+n%side);
            Family Material(int n)=>families[(n/side+n%side)%families.Length];
            void ValidateCell(int n,int phase) {
                Point p=PointAt(n);Tile t=Main.tile[p];Family f=Material(n);int shape=n%6;
                int tile=phase==0||phase==3?f.Pure:phase==1?f.Maw:f.Waste;
                int wall=phase==0||phase==3?f.PureWall:phase==1?f.MawWall:f.WasteWall;
                Check(t.HasTile&&t.TileType==tile&&t.WallType==wall,$"mapping {f.Name}/{phase}/{n}: tile{t.TileType} wall{t.WallType}");
                Check(t.Slope==(SlopeType)(shape==5?0:shape)&&t.IsHalfBlock==(shape==5)&&t.TileColor==PaintID.BluePaint&&t.WallColor==PaintID.RedPaint&&
                    t.RedWire&&t.BlueWire&&!t.GreenWire&&!t.YellowWire&&t.HasActuator&&t.IsActuated==(n%2==0)&&t.IsTileFullbright&&t.IsWallFullbright&&
                    t.IsTileInvisible==(n%3==0)&&t.IsWallInvisible==(n%3==0)&&t.LiquidAmount==0,$"state {phase}/{n}");
            }
            try {
                for(int n=0;n<side*side;n++) {
                    Point p=PointAt(n);Tile t=Main.tile[p];Family f=Material(n);int shape=n%6;
                    t.HasTile=true;t.TileType=(ushort)f.Pure;t.WallType=(ushort)f.PureWall;t.Slope=(SlopeType)(shape==5?0:shape);t.IsHalfBlock=shape==5;
                    t.TileColor=PaintID.BluePaint;t.WallColor=PaintID.RedPaint;t.RedWire=t.BlueWire=t.HasActuator=true;t.IsActuated=n%2==0;
                    t.IsTileFullbright=t.IsWallFullbright=true;t.IsTileInvisible=t.IsWallInvisible=n%3==0;
                }
                for(int n=0;n<side*side;n++)ValidateCell(n,0);
                // A deliberately skipped conversion must be noticed, not blessed by timing.
                try{ValidateCell(0,1);}catch(InvalidOperationException){negativeControls++;}
                Check(negativeControls==1,"unconverted-cell control not detected");
                for(int cycle=0;cycle<3;cycle++)for(int phase=1;phase<=3;phase++) {
                    var timings=new double[batches.Count];long allocation=GC.GetAllocatedBytesForCurrentThread();int index=0;bool validated=false;
                    try { foreach(var batch in batches) {
                        if(total.Elapsed.TotalSeconds>2)throw new TimeoutException("Two-second soft work deadline; no further batch issued.");
                        long start=Stopwatch.GetTimestamp();
                        for(int n=batch.Start;n<batch.Start+batch.Count;n++) {
                            Point p=PointAt(n);
                            if(phase==1)MawConversionSystem.ConvertAt(p.X,p.Y,true,true);
                            else {TileLoader.Convert(p.X,p.Y,BiomeConversionID.Purity);WallLoader.Convert(p.X,p.Y,BiomeConversionID.Purity);}
                        }
                        timings[index++]=(Stopwatch.GetTimestamp()-start)*1000.0/Stopwatch.Frequency;completedCells+=batch.Count;
                        for(int n=batch.Start;n<batch.Start+batch.Count;n++)ValidateCell(n,phase);
                    } validated=true; }
                    finally { phases.Add(new {cycle,phase,cells=index*32,budgetCells=32,validated,conversionBatchMs=timings.Take(index).ToArray(),statistics=QASampleStatistics.Describe(timings,index),
                        threadAllocatedBytes=GC.GetAllocatedBytesForCurrentThread()-allocation}); }
                }
                Point control=new(patch.Right+3,patch.Top);Tile built=Main.tile[control];
                foreach(var pair in new[]{(TileID.GrayBrick,WallID.Wood),(ModContent.TileType<KesslerBlock>(),ModContent.WallType<KesslerBulkheadWall>())}) {
                    built.HasTile=true;built.TileType=(ushort)pair.Item1;built.WallType=(ushort)pair.Item2;built.RedWire=true;
                    Check(!MawConversionSystem.ConvertAt(control.X,control.Y,true,true)&&built.TileType==pair.Item1&&built.WallType==pair.Item2&&built.RedWire,"constructed control preserved");
                }
            } catch(Exception ex) {error=ex.GetType().Name+": "+ex.Message;}
            finally {
                for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)original[Index(x,y)].Restore(x,y);
                restored=true;
                for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)restored&=original[Index(x,y)]==MawShallowTraversalLab.CellState.Read(x,y);
                historyUnchanged=oldHistory==MawShallowTraversalLab.Fingerprint(history);
            }
            double totalMs=total.Elapsed.TotalMilliseconds;string after=MawShallowTraversalLab.Fingerprint(new[]{guard});
            bool pass=error==null&&restored&&historyUnchanged&&before==after&&completedCells==side*side*9;
            var report=new {schemaVersion=1,utc=DateTime.UtcNow,pass,error,side,cycles=3,completedCells,checks,negativeControls,restored,historyUnchanged,before,after,totalMs,
                origin=new{x=patch.X,y=patch.Y},phases,
                limitations="Synchronous disposable native conversion throughput. Batch timings exclude setup/checks/restore; phase allocations INCLUDE validation. Not frame/update scheduling, spread speed, GPU, multiplayer, visual or low-end acceptance."};
            string directory=Path.Combine(Main.SavePath,"Captures","ApogeanPerformance");Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"conversion-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N")[..6]+".json");
            using(var file=new FileStream(path+".partial",FileMode.CreateNew,FileAccess.Write,FileShare.None))JsonSerializer.Serialize(file,report,new JsonSerializerOptions{WriteIndented=true});
            File.Move(path+".partial",path);log.Info("QA PERFORMANCE CONVERSION EXPORT: "+path);
            log.Info($"QA PERFORMANCE CONVERSION COMPLETE: pass={pass}; side={side}; cells={completedCells}; restored={restored}; historyUnchanged={historyUnchanged}; totalMs={totalMs:F3}. Synchronous probe, not update scheduling.");
            if(!pass)throw new InvalidOperationException(error??"Conversion-load restoration or completion failed.");
        }
    }
}
