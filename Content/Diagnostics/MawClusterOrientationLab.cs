using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using apogean.Content.Items.Placeable;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
    // Separate saved room: never rebuild the existing approved floor gallery.
    public sealed class MawClusterOrientationLab : ModSystem
    {
        private Rectangle bounds;
        private bool viewing, oldDay, oldRain, oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private int captureDelay = -1, checks;
        private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer && Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
        private MawToothClusterTile Cluster => ModContent.GetInstance<MawToothClusterTile>();
        private int ItemType => ModContent.ItemType<MawToothCluster>();
        private int GroundType => ModContent.TileType<MawDirt>();
        private Point At(int x, int y) => new(bounds.X + x, bounds.Y + y);
        private static readonly Point[] DisplayLocations = { new(10,28), new(6,12), new(23,4), new(38,20), new(14,28), new(18,28) };

        internal void Run(string request)
        {
            if (!IsQa || !MawToothClusterTile.Included(Mod)) throw new InvalidOperationException("Orientation QA requires gg/V3/SP and candidate package.");
            var grove = ModContent.GetInstance<VegetationVisualLab>(); string before = grove.CheckpointSnapshot();
            try {
                switch (request) {
                    case "build": Build(); Test(); View(false); break;
                    case "test": Test(); break;
                    case "reload": Test(); View(false); break;
                    case "day": View(false); break;
                    case "night": View(true); break;
                    case "capture": Require(); captureDelay = 45; break;
                    case "release": Release(); break;
                    default: throw new InvalidOperationException("Unknown orientation request.");
                }
            } finally {
                if (before != grove.CheckpointSnapshot()) throw new InvalidOperationException("Orientation fixture changed preserved grove.");
                Mod.Logger.Info("MAW ORIENTATION GROVE GUARD: unchanged; historical failure not rebaselined.");
            }
        }
        private void Require()
        {
            if (bounds.Width != 104 || bounds.Height != 42 || bounds.Left < 40 || bounds.Top < 40 || bounds.Right >= Main.maxTilesX - 40 || bounds.Bottom >= Main.maxTilesY - 40)
                throw new InvalidOperationException("Missing/invalid saved orientation fixture.");
        }
        private void Build()
        {
            if (!bounds.IsEmpty) throw new InvalidOperationException("Saved orientation fixture exists; use reload, never rebuild.");
            Rectangle protectedArea = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds; protectedArea.Inflate(20,20);
            foreach (int dx in new[] { 1660, -1690, 1810, -1840 }) {
                foreach (int dy in new[] { -100, -160, -225 }) {
                    Rectangle trial = new(Main.spawnTileX + dx, Main.spawnTileY + dy,104,42), envelope = trial; envelope.Inflate(4,4);
                    if (envelope.Left < 40 || envelope.Top < 40 || envelope.Right >= Main.maxTilesX - 40 || envelope.Bottom >= Main.maxTilesY - 40 || envelope.Intersects(protectedArea)) continue;
                    bool empty = true;
                    for (int x=envelope.Left; x<envelope.Right; x++) for (int y=envelope.Top; y<envelope.Bottom; y++) {
                        Tile t = Main.tile[x,y];
                        if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount > 0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire) empty = false;
                    }
                    if (empty) { bounds=trial; break; }
                }
                if (!bounds.IsEmpty) break;
            }
            Require(); // Failure leaves all terrain intact.
            for (int x=4; x<99; x++) for (int y=32; y<37; y++) Ground(At(x,y));
            for (int x=4; x<44; x++) for (int y=2; y<4; y++) Ground(At(x,y));
            for (int y=4; y<32; y++) { Ground(At(5,y)); Ground(At(42,y)); }
            WorldGen.RangeFrame(bounds.Left,bounds.Top,bounds.Right,bounds.Bottom);
            for (int k=0;k<DisplayLocations.Length;k++) Place(At(DisplayLocations[k].X,DisplayLocations[k].Y), k<4?k:0);
            Mod.Logger.Info($"MAW ORIENTATION BUILD: {bounds}; empty envelope; six displays, all four attachments.");
        }
        private void Ground(Point p) => WorldGen.PlaceTile(p.X,p.Y,GroundType,mute:true);
        private bool PlaceNative(Point p,int facing)
        {
            var origin = MawToothClusterTile.PlacementOrigin(facing);
            // No forced alternate: exercise the same anchor chooser as item placement.
            return WorldGen.PlaceObject(p.X+origin.X,p.Y+origin.Y,Cluster.Type,mute:true);
        }
        private void Place(Point p,int facing)
        {
            if (!PlaceNative(p,facing)) throw new InvalidOperationException($"Native placement rejected facing {facing}.");
            Frames(p,facing);
        }
        private void Frames(Point p,int facing)
        {
            for(int x=0;x<4;x++) for(int y=0;y<4;y++) {
                Tile t = Main.tile[p.X+x,p.Y+y];
                if (!t.HasTile || t.TileType!=Cluster.Type || t.TileFrameX!=facing*72+x*18 || t.TileFrameY!=y*18) throw new InvalidOperationException($"Wrong facing/frame {facing} at {p}.");
            }
        }
        private int Count(Point p)
        {
            int n=0; for(int x=0;x<4;x++) for(int y=0;y<4;y++) if(Main.tile[p.X+x,p.Y+y].HasTile && Main.tile[p.X+x,p.Y+y].TileType==Cluster.Type)n++; return n;
        }
        private List<Item> Drops()
        {
            var result=new List<Item>(); Rectangle area=new(bounds.X*16,bounds.Y*16,bounds.Width*16,bounds.Height*16);
            foreach(Item item in Main.ActiveItems) if(item.type==ItemType && area.Intersects(item.Hitbox))result.Add(item); return result;
        }
        private void Check(bool pass,string name)
        {
            if(!pass)throw new InvalidOperationException("Orientation check failed: "+name);
            checks++; Mod.Logger.Info("MAW ORIENTATION CHECK PASS: "+name);
        }
        private void Removed(Point p,string name)
        {
            WorldGen.RangeFrame(p.X-1,p.Y-1,p.X+5,p.Y+5); var drops=Drops();
            Check(Count(p)==0 && drops.Count==1 && drops[0].stack==1,name);
            drops[0].TurnToAir(); // Only our one newly asserted QA test drop.
        }
        private void Test()
        {
            Require(); checks=0;
            for(int k=0;k<DisplayLocations.Length;k++) Frames(At(DisplayLocations[k].X,DisplayLocations[k].Y),k<4?k:0);
            Point p=At(72,16);
            Check(Count(p)==0 && Drops().Count==0,"empty-trial-and-no-existing-drops");
            Main.instance.LoadTiles(Cluster.Type); var texture=TextureAssets.Tile[Cluster.Type].Value;
            Check(texture.Width==288 && texture.Height==72,"native-288x72-atlas");
            Color[] pixels=new Color[288*72];texture.GetData(pixels);
            for(int facing=0;facing<4;facing++) {
                for(int cell=0;cell<4;cell++)Ground(MawToothClusterTile.Support(p,facing,cell));
                Place(p,facing);Check(true,$"automatic-anchor-choice-{facing}");
                var data=TileObjectData.GetTileData(Main.tile[p.X,p.Y]); Point inset=MawToothClusterTile.Inset(facing);
                Check(data.DrawXOffset==inset.X && data.DrawYOffset==inset.Y,$"embedded-root-offset-{facing}");
                int nativeWidth=0,nativeY=0,nativeHeight=0;short nativeFrameX=Main.tile[p.X,p.Y].TileFrameX,nativeFrameY=0;
                TileLoader.SetDrawPositions(p.X,p.Y,ref nativeWidth,ref nativeY,ref nativeHeight,ref nativeFrameX,ref nativeFrameY);
                Check(nativeY==inset.Y && nativeWidth==16 && nativeHeight==16,$"actual-placed-draw-offset-{facing}");
                int opaque=0;Point hit=default;
                for(int y=0;y<64;y++)for(int x=0;x<64;x++) {
                    bool expected=pixels[(y/16*18+y%16)*288+facing*72+x/16*18+x%16].A==255;
                    Rectangle probe=new(p.X*16+inset.X+x,p.Y*16+inset.Y+y,1,1);
                    if(Cluster.TouchesAt(probe,p)!=expected || Cluster.Touching(probe)!=expected)throw new InvalidOperationException($"Hurt/render mismatch {facing} {x},{y}");
                    if(expected){opaque++;if(x>7 && x<56 && y>7 && y<56)hit=new Point(x,y);}
                }
                Check(opaque==1478,$"4096-native-pixel-contacts-{facing}");
                Check(!Collision.SolidCollision(p.ToVector2()*16,64,64),$"nonsolid-{facing}");
                Player player=new(){whoAmI=Main.myPlayer,width=1,height=1,statLife=200,statLifeMax=200,statLifeMax2=200};
                player.position=p.ToVector2()*16+inset.ToVector2()+hit.ToVector2();
                Tile part=Main.tile[p.X+hit.X/16,p.Y+hit.Y/16];part.IsActuated=true;
                Check(!Cluster.Touching(player.Hitbox),$"actuation-safe-{facing}");part.IsActuated=false;
                int before=player.statLife;Cluster.ApplyContact(player);int after=player.statLife;
                Check(after<before && player.immune,$"native-hurt-{facing}");Cluster.ApplyContact(player);
                Check(player.statLife==after,$"immunity-{facing}");
                WorldGen.KillTile(p.X,p.Y);Removed(p,$"first-recovery-{facing}");
                for(int x=0;x<4;x++)for(int y=0;y<4;y++) {
                    Place(p,facing);int hits=0;while(Count(p)>0 && hits<10){Main.LocalPlayer.PickTile(p.X+x,p.Y+y,35);hits++;}
                    Check(hits<=3,$"starter-pick-{facing}-{x}-{y}");Removed(p,$"one-item-{facing}-{x}-{y}");
                }
                for(int cell=0;cell<4;cell++) {
                    Point support=MawToothClusterTile.Support(p,facing,cell);WorldGen.KillTile(support.X,support.Y,noItem:true);
                    Check(!PlaceNative(p,facing),$"incomplete-support-rejected-{facing}-{cell}");Ground(support);Place(p,facing);
                    WorldGen.KillTile(support.X,support.Y,noItem:true);Removed(p,$"support-loss-{facing}-{cell}");Ground(support);
                }
                for(int cell=0;cell<4;cell++){Point support=MawToothClusterTile.Support(p,facing,cell);WorldGen.KillTile(support.X,support.Y,noItem:true);}
                Check(!PlaceNative(p,facing),$"unsupported-air-rejected-{facing}");
            }
            for(int k=0;k<DisplayLocations.Length;k++) Frames(At(DisplayLocations[k].X,DisplayLocations[k].Y),k<4?k:0);
            Mod.Logger.Info($"MAW ORIENTATION MATRIX PASS: {checks} checks; 16384 actual texture/contact comparisons; six displays intact. Native APIs, not manual/multiplayer certification.");
        }
        private void View(bool night)
        {
            Require();if(!viewing){oldPosition=Main.LocalPlayer.position;oldDay=Main.dayTime;oldTime=Main.time;oldRain=Main.raining;oldEclipse=Main.eclipse;viewing=true;}
            Main.dayTime=!night;Main.time=night?16000:27000;Main.raining=false;Main.eclipse=false;
            Main.LocalPlayer.Teleport(new Vector2((bounds.X+28)*16,(bounds.Y+32)*16-Main.LocalPlayer.height),1);Main.LocalPlayer.velocity=Vector2.Zero;
            Main.NewText("Maw clusters: floor, ceiling and both walls. Same item; mine any segment. This is a QA room, not a generated safe ledge.",Color.Wheat);
        }
        public override void PostUpdateEverything()
        {
            if(!IsQa || captureDelay<0 || captureDelay--!=0)return;captureDelay=-1;
            CaptureManager.Instance.Capture(new CaptureSettings{Area=new Rectangle(bounds.X+3,bounds.Y+1,42,37),Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),CaptureBackground=true,CaptureEntities=true,UseScaling=true,OutputName="Apogean Maw Orientations "+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")});
        }
        internal void Release()
        {
            captureDelay=-1;if(!viewing)return;Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;
            if(IsQa)Main.LocalPlayer.Teleport(oldPosition,1);viewing=false;
        }
        public override void SaveWorldData(TagCompound tag){if(IsQa && !bounds.IsEmpty)tag["mawClusterOrientationV1"]=new TagCompound{["x"]=bounds.X,["y"]=bounds.Y};}
        public override void LoadWorldData(TagCompound tag){if(Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3" && tag.ContainsKey("mawClusterOrientationV1")){var t=tag.GetCompound("mawClusterOrientationV1");bounds=new Rectangle(t.GetInt("x"),t.GetInt("y"),104,42);}}
        public override void ClearWorld(){bounds=Rectangle.Empty;viewing=false;captureDelay=-1;}
    }
}
