using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using Terraria.Graphics.Capture;
using apogean.Content.Tiles;
using P = apogean.Content.Diagnostics.MawSketchPlan;
using S = apogean.Content.Diagnostics.MawShallowTraversalLab;

namespace apogean.Content.Diagnostics
{
    // Explicit drawing-led construction. No automatic build, worldgen hook or held camera.
    public sealed class MawSketchLab : ModSystem
    {
        private const string SaveKey = "mawSketchSeptember13V1";
        private Rectangle bounds;
        private string original, saved;
        private bool failed, visiting, lamp, oldDay, oldRain, oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private int captureDelay=-1;
        private static bool Context => S.IsQa && MawPackedPreview.Enabled && MawAnatomyMaterials.Available;
        internal bool Visiting => Context && visiting;
        private static MawToothClusterTile Teeth => ModContent.GetInstance<MawToothClusterTile>();
        private static int TileType(string key) => key switch {
            "rib" or "cap" => MawAnatomyMaterials.Tile(key).Type,
            "amber" => ModContent.TileType<MawAmberLitTile>(), _ => MawPackedPreview.TileType(key)
        };
        private static int WallType(string key) => key == null ? WallID.None : key == "amber" ? ModContent.WallType<MawAmberLitWall>() : MawPackedPreview.Wall(key).Type;
        private static Rectangle[] History() => S.Historical().Append(ModContent.GetInstance<S>().PreservedBounds).ToArray();
        private string Fingerprint() => S.Fingerprint(new[]{bounds});
        private void Require() {
            if(failed||original==null||bounds.Width!=P.Width||bounds.Height!=P.Height||!WorldGen.InWorld(bounds.Left,bounds.Top,30)||!WorldGen.InWorld(bounds.Right,bounds.Bottom,30))
                throw new InvalidOperationException("Missing/failed sketch scene; no automatic rebuild.");
        }
        private void Build() {
            if(Main.LocalPlayer.name!="gg")throw new InvalidOperationException("Build requires gg; Plain can only visit this scene.");
            if(original!=null||!bounds.IsEmpty)throw new InvalidOperationException("Sketch already reserved; never rebuild or rebaseline.");
            var p=P.Create();var history=History();int attempts=0;
            foreach(int x in new[]{Main.spawnTileX-2700,Main.spawnTileX-2350,Main.spawnTileX-2000,Main.spawnTileX+2100}) {
                foreach(int y in new[]{80,250,410}) {
                    attempts++;Rectangle site=new(x,y,P.Width,P.Height),guard=site;guard.Inflate(12,12);
                    if(!WorldGen.InWorld(guard.Left,guard.Top,40)||!WorldGen.InWorld(guard.Right,guard.Bottom,40)||!S.ActorsAbsent(guard))continue;
                    bool occupied=false;
                    foreach(Rectangle old in history){if(old.IsEmpty)continue;Rectangle expanded=old;expanded.Inflate(24,24);occupied|=expanded.Intersects(guard);}
                    if(occupied||GenVars.structures!=null&&!GenVars.structures.CanPlace(guard))continue;
                    foreach(Chest chest in Main.chest)if(chest!=null&&guard.Contains(chest.x,chest.y))occupied=true;
                    for(int i=guard.Left;i<guard.Right;i++)for(int j=guard.Top;j<guard.Bottom;j++)occupied|=!S.Empty(Main.tile[i,j]);
                    if(occupied)continue;
                    bounds=site;break;
                }
                if(!bounds.IsEmpty)break;
            }
            if(bounds.IsEmpty)throw new InvalidOperationException($"No empty safe sketch site in {attempts} attempts; nothing cleared.");
            Rectangle envelope=bounds;envelope.Inflate(12,12);GenVars.structures?.AddProtectedStructure(envelope);
            var snapshot=new S.CellState[envelope.Width,envelope.Height];
            for(int x=0;x<envelope.Width;x++)for(int y=0;y<envelope.Height;y++)snapshot[x,y]=S.CellState.Read(envelope.X+x,envelope.Y+y);
            var createdChests=new List<int>();
            try {
                for(int x=0;x<P.Width;x++)for(int y=0;y<P.Height;y++) {
                    P.Cell c=p.Cells[x,y];Tile tile=Main.tile[bounds.X+x,bounds.Y+y];
                    if(c.Key!=null){tile.HasTile=true;tile.TileType=(ushort)TileType(c.Key);tile.Slope=(SlopeType)c.Slope;}
                    if(c.Wall!=null)tile.WallType=(ushort)WallType(c.Wall);
                }
                WorldGen.RangeFrame(bounds.Left-1,bounds.Top-1,bounds.Right+1,bounds.Bottom+1);
                foreach(P.Tooth tooth in p.Teeth) {
                    var origin=MawToothClusterTile.PlacementOrigin(tooth.Face);
                    int x=bounds.X+tooth.Root.X,y=bounds.Y+tooth.Root.Y;
                    if(!WorldGen.PlaceObject(x+origin.X,y+origin.Y,Teeth.Type,mute:true,style:tooth.Variant/4))
                        throw new InvalidOperationException($"Sketch tooth placement failed at {tooth.Root}, variant{tooth.Variant}.");
                }
                // PlaceChest expects BOTTOM-left; P.Chests records top-left.
                foreach(P.Point chest in p.Chests) {
                    int id=WorldGen.PlaceChest(bounds.X+chest.X,bounds.Y+chest.Y+1,TileID.Containers,false,0);
                    if(id<0)throw new InvalidOperationException("Sketch chest placement failed: "+chest);
                    createdChests.Add(id);Main.chest[id].name="Maw side-cache layout study (empty)";
                }
                int fiber=ModContent.Find<ModTile>("apogean/MawHangingFiber").Type;
                foreach(P.Strand strand in p.Fibers)for(int d=1;d<=strand.Length;d++) {
                    int x=bounds.X+strand.Root.X,y=bounds.Y+strand.Root.Y+d;
                    Tile tile=Main.tile[x,y];if(tile.HasTile)throw new InvalidOperationException("Fiber footprint occupied.");
                    tile.HasTile=true;tile.TileType=(ushort)fiber;tile.TileFrameX=0;tile.TileFrameY=(short)((d-1)*18);
                    WorldGen.SquareTileFrame(x,y);
                }
                VerifyOriginal(p);original=saved=Fingerprint();
                Mod.Logger.Info($"MAW SKETCH BUILD: bounds={bounds}; attempts={attempts}; ribs={p.Ribs.Length}; teeth={p.Teeth.Count}; fiberStrands={p.Fibers.Count}; emptyChests={createdChests.Count}; original={original}. Existing assets, new QA scene only.");
            } catch {
                foreach(int id in createdChests) {
                    Chest chest=Main.chest[id];
                    if(chest!=null&&bounds.Contains(chest.x,chest.y)&&chest.item.All(i=>i==null||i.IsAir))Main.chest[id]=null;
                }
                for(int x=0;x<envelope.Width;x++)for(int y=0;y<envelope.Height;y++)snapshot[x,y].Restore(envelope.X+x,envelope.Y+y);
                failed=true;Mod.Logger.Error("MAW SKETCH ROLLBACK: only newly owned empty envelope and new empty chest records restored. Failed reservation retained.");throw;
            }
        }
        private void VerifyOriginal(P p) {
            var objects=new Dictionary<P.Point,(int type,int fx,int fy)>();
            foreach(var tooth in p.Teeth)for(int dx=0;dx<4;dx++)for(int dy=0;dy<4;dy++)
                objects.Add(new(tooth.Root.X+dx,tooth.Root.Y+dy),(Teeth.Type,tooth.Variant*72+dx*18,dy*18));
            foreach(var chest in p.Chests) {
                int id=Chest.FindChest(bounds.X+chest.X,bounds.Y+chest.Y);
                if(id<0||Main.chest[id].item.Any(i=>i!=null&&!i.IsAir))throw new InvalidOperationException("Missing/nonempty original sketch chest.");
                for(int dx=0;dx<2;dx++)for(int dy=0;dy<2;dy++)objects.Add(new(chest.X+dx,chest.Y+dy),(TileID.Containers,dx*18,dy*18));
            }
            int fiber=ModContent.Find<ModTile>("apogean/MawHangingFiber").Type;
            foreach(var strand in p.Fibers)for(int d=1;d<=strand.Length;d++)objects.Add(new(strand.Root.X,strand.Root.Y+d),(fiber,0,(d-1)*18));
            for(int x=0;x<P.Width;x++)for(int y=0;y<P.Height;y++) {
                P.Cell c=p.Cells[x,y];Tile t=Main.tile[bounds.X+x,bounds.Y+y];bool obj=objects.TryGetValue(new(x,y),out var value);
                bool bad=t.HasTile!=(c.Key!=null||obj)||t.WallType!=WallType(c.Wall)||t.LiquidAmount!=0||t.HasActuator||t.IsActuated||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||t.TileColor!=0||t.WallColor!=0||t.IsTileInvisible||t.IsWallInvisible||t.IsTileFullbright||t.IsWallFullbright;
                if(t.HasTile)bad|=t.TileType!=(obj?value.type:TileType(c.Key))||t.IsHalfBlock||(byte)t.Slope!=c.Slope||obj&&(t.TileFrameX!=value.fx||t.TileFrameY!=value.fy);
                if(bad)throw new InvalidOperationException($"Sketch original differs at {x},{y}: actual={t.TileType}/{t.HasTile}/{t.Slope}/{t.WallType}; expected={c}/{(obj?value.ToString():"terrain")}. No repair.");
            }
            Mod.Logger.Info($"MAW SKETCH ORIGINAL PASS: {P.Width*P.Height} native tile/wall/slope/object cells; 2 actual empty chest records. Not visual or movement acceptance.");
        }
        internal void Run(string request) {
            if(!Context||ModContent.GetInstance<QAPerformanceLab>().Recording)throw new InvalidOperationException("Sketch requires idle packed V3 single-player, gg or Plain.");
            Rectangle[] history=History();string before=S.Fingerprint(history);
            Mod.Logger.Info("MAW SKETCH REQUEST: "+request);
            try {
                switch(request) {
                    case "build":Build();break;
                    case "pristine":Require();VerifyOriginal(P.Create());break;
                    case "reload":Require();if(saved!=Fingerprint())throw new InvalidOperationException("Sketch saved state differs; not restored.");Mod.Logger.Info("MAW SKETCH RELOAD PASS: saved interaction state unchanged.");break;
                    case "entrance":case "ribs":case "cave":case "lower":Visit(request);break;
                    case "light-on":case "light-off":Require();if(!visiting)throw new InvalidOperationException("Visit sketch first.");lamp=request=="light-on";Main.NewText(lamp?"Sketch inspection light ON — not natural Maw lighting. Free movement.":"Sketch inspection light OFF — natural lighting. Free movement.",Color.Wheat);break;
                    case "capture":Require();if(!visiting)throw new InvalidOperationException("Visit sketch before capture.");captureDelay=15;break;
                    case "release":Release();break;
                    default:throw new InvalidOperationException("Unknown sketch request.");
                }
            } finally {
                if(before!=S.Fingerprint(history))throw new InvalidOperationException("Earlier fixture changed during sketch command; not repaired.");
                Mod.Logger.Info("MAW SKETCH PRESERVATION: "+S.DescribeHistory(history));
            }
            Mod.Logger.Info("MAW SKETCH COMPLETE: "+request);
        }
        private void Visit(string view) {
            Require();var p=P.Create();
            P.Point at=view switch {"ribs"=>new(40,59),"cave"=>new(123,60),"lower"=>new(180,111),_=>p.Entrance};
            Vector2 target=new((bounds.X+at.X)*16,(bounds.Y+at.Y)*16);
            if(Collision.SolidCollision(target,Main.LocalPlayer.width,Main.LocalPlayer.height)||Teeth.Touching(new((int)target.X,(int)target.Y,Main.LocalPlayer.width,Main.LocalPlayer.height)))
                throw new InvalidOperationException("Sketch visit position obstructed; not clearing it.");
            if(!visiting){oldPosition=Main.LocalPlayer.position;oldDay=Main.dayTime;oldTime=Main.time;oldRain=Main.raining;oldEclipse=Main.eclipse;visiting=true;}
            Main.LocalPlayer.Teleport(target,1);Main.LocalPlayer.velocity=Vector2.Zero;
            Main.dayTime=true;Main.time=27000;Main.raining=false;Main.eclipse=false;
            Main.NewText("Drawing-led Maw study — FREE CONTROLS. Teeth hurt; chests are empty layout markers.",Color.Wheat);
        }
        internal void Release() {
            lamp=false;captureDelay=-1;if(!visiting)return;
            if(Context){Main.LocalPlayer.Teleport(oldPosition,1);Main.LocalPlayer.velocity=Vector2.Zero;}
            Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;visiting=false;
        }
        public override void PostUpdateEverything() {
            if(!Visiting)return;
            // One optional local inspection lamp, no tile mutation/fullbright/saved lighting.
            if(lamp)Lighting.AddLight(Main.LocalPlayer.Center,1.5f,1.4f,1.2f);
            if(captureDelay<0||captureDelay--!=0)return;
            string name="Apogean Maw Drawing "+(lamp?"inspection-":"natural-")+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            CaptureManager.Instance.Capture(new CaptureSettings {Area=bounds,
                Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
                CaptureBackground=true,CaptureEntities=true,UseScaling=true,OutputName=name});
            Mod.Logger.Info("MAW SKETCH CAPTURE DISPATCHED: "+name+"; inspect output before accepting.");
        }
        public override void SaveWorldData(TagCompound tag) {
            if(Main.ActiveWorldFileData?.Name!=MawShallowQaScope.World||bounds.IsEmpty)return;
            if(WorldGen.InWorld(bounds.Left,bounds.Top,30)&&WorldGen.InWorld(bounds.Right,bounds.Bottom,30))saved=Fingerprint();else failed=true;
            tag[SaveKey]=new TagCompound{["x"]=bounds.X,["y"]=bounds.Y,["version"]=P.Version,["original"]=original??"",["saved"]=saved??"",["failed"]=failed};
            Mod.Logger.Info($"MAW SKETCH SAVE: original={original}; saved={saved}; failed={failed}.");
        }
        public override void LoadWorldData(TagCompound tag) {
            if(Main.ActiveWorldFileData?.Name!=MawShallowQaScope.World||!tag.ContainsKey(SaveKey))return;
            var t=tag.GetCompound(SaveKey);bounds=new(t.GetInt("x"),t.GetInt("y"),P.Width,P.Height);original=t.GetString("original");saved=t.GetString("saved");failed=t.GetBool("failed")||t.GetInt("version")!=P.Version;
        }
        public override void ClearWorld(){bounds=Rectangle.Empty;original=saved=null;failed=visiting=lamp=false;captureDelay=-1;}
    }
    public sealed class MawSketchCommand : ModCommand
    {
        public override string Command=>"mawsketch";
        public override CommandType Type=>CommandType.Chat;
        public override string Usage=>"/mawsketch entrance|ribs|cave|lower|light-on|light-off|release";
        public override string Description=>"Visit the disposable drawing-led Maw study with free controls.";
        public override void Action(CommandCaller caller,string input,string[] args) {
            if(args.Length!=1||args[0] is not ("entrance" or "ribs" or "cave" or "lower" or "light-on" or "light-off" or "release"))throw new UsageException(Usage);
            ModContent.GetInstance<S>().Release();ModContent.GetInstance<MawRibContourStudy>().Release();
            ModContent.GetInstance<MawSketchLab>().Run(args[0]);
        }
    }
    public sealed class MawSketchAmbientGate : GlobalNPC
    {
        public override void EditSpawnRate(Player player,ref int spawnRate,ref int maxSpawns) {
            if(player.whoAmI==Main.myPlayer&&ModContent.GetInstance<MawSketchLab>().Visiting)maxSpawns=0;
        }
    }
}
