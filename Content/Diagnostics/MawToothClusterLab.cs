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
	public sealed class MawToothClusterLab : ModSystem
	{
		private Rectangle bounds;
		private bool viewing, oldDay, oldRain, oldEclipse;
		private double oldTime;
		private Vector2 oldPosition;
		private int captureDelay = -1, checks;
		private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer && Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
		private MawToothClusterTile Cluster => ModContent.GetInstance<MawToothClusterTile>();
		private int ItemType => ModContent.ItemType<MawToothCluster>();
		private int Floor => bounds.Y + 18;
		private Point At(int x) => new(bounds.X + x, Floor - 4);
		private static readonly int[] Displays = { 6, 18, 22, 26 };
		internal void Run(string request)
		{
			if (!IsQa || !MawToothClusterTile.Included(Mod)) throw new InvalidOperationException("Cluster QA requires gg/V3/SP and the candidate package.");
			var grove = ModContent.GetInstance<VegetationVisualLab>(); string before = grove.CheckpointSnapshot();
			try {
				switch (request) {
					case "build": Build(); break;
					case "test": Test(); break;
					case "reload": Test(); View(false); break;
					case "day": View(false); break;
					case "night": View(true); break;
					case "capture": Require(); captureDelay = 45; break;
					case "release": Release(); break;
					default: throw new InvalidOperationException("Unknown cluster request.");
				}
			} finally {
				if (before != grove.CheckpointSnapshot()) throw new InvalidOperationException("Cluster changed the preserved grove.");
				Mod.Logger.Info("MAW CLUSTER GROVE GUARD: unchanged; historical reload failure not rebaselined.");
			}
		}
		private static bool Empty(Rectangle area)
		{
			for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
				Tile t = Main.tile[x,y];
				if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount > 0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire) return false;
			}
			return true;
		}
		private void Build()
		{
			if (!bounds.IsEmpty) throw new InvalidOperationException("Saved cluster fixture exists; use reload, no rebuild.");
			Rectangle protectedArea = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds; protectedArea.Inflate(20,20);
			foreach (int dx in new[] { 1360, -1420, 1500, -1550 }) {
				foreach (int dy in new[] { -100, -155, -215 }) {
					Rectangle trial = new(Main.spawnTileX + dx, Main.spawnTileY + dy, 60, 24), envelope = trial;
					envelope.Inflate(4,4);
					if (envelope.Left < 40 || envelope.Top < 40 || envelope.Right >= Main.maxTilesX - 40 || envelope.Bottom >= Main.maxTilesY - 40 || envelope.Intersects(protectedArea) || !Empty(envelope)) continue;
					bounds = trial; break;
				}
				if (!bounds.IsEmpty) break;
			}
			if (bounds.IsEmpty) throw new InvalidOperationException("No empty cluster site; no terrain cleared.");
			for (int x = 2; x < 58; x++) for (int y = 18; y < 23; y++) WorldGen.PlaceTile(bounds.X + x, bounds.Y + y, ModContent.TileType<MawDirt>(), mute:true);
			WorldGen.RangeFrame(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			foreach (int x in Displays) Place(At(x));
			Mod.Logger.Info($"MAW CLUSTER BUILD: {bounds}; empty envelope; one group and three adjacent groups.");
			View(false); Test();
		}
		private void Require()
		{
			if (bounds.Width != 60 || bounds.Height != 24 || bounds.Left < 40 || bounds.Top < 40 || bounds.Right >= Main.maxTilesX-40 || bounds.Bottom >= Main.maxTilesY-40) throw new InvalidOperationException("Missing/invalid saved cluster bounds.");
		}
		private void Check(bool pass, string name)
		{
			if (!pass) throw new InvalidOperationException("Cluster check failed: " + name);
			checks++; Mod.Logger.Info("MAW CLUSTER CHECK PASS: " + name);
		}
		private int Count(Point p)
		{
			int n=0;
			for (int x=0;x<4;x++) for(int y=0;y<4;y++) if (Main.tile[p.X+x,p.Y+y].HasTile && Main.tile[p.X+x,p.Y+y].TileType == Cluster.Type) n++;
			return n;
		}
		private void AssertFrames(Point p)
		{
			for (int x=0;x<4;x++) for(int y=0;y<4;y++) {
				Tile t = Main.tile[p.X+x,p.Y+y];
				if (!t.HasTile || t.TileType != Cluster.Type || t.TileFrameX != x*18 || t.TileFrameY != y*18) throw new InvalidOperationException("Cluster missing or wrong native frame.");
			}
		}
		private void Place(Point p)
		{
			if (!WorldGen.PlaceObject(p.X+1,p.Y+3,Cluster.Type,mute:true)) throw new InvalidOperationException("Cluster native placement failed.");
			AssertFrames(p);
		}
		private List<Item> Drops()
		{
			var result = new List<Item>(); Rectangle area = new(bounds.X*16,bounds.Y*16,bounds.Width*16,bounds.Height*16);
			foreach (Item item in Main.ActiveItems) if (item.type == ItemType && area.Intersects(item.Hitbox)) result.Add(item);
			return result;
		}
		private void Removed(Point p,string name)
		{
			WorldGen.RangeFrame(p.X-1,p.Y-1,p.X+5,p.Y+5);
			var drops = Drops(); Check(Count(p)==0 && drops.Count==1 && drops[0].stack==1,name);
			drops[0].TurnToAir(); // Only the one newly counted test drop, never arbitrary items.
		}
		private void Test()
		{
			Require(); checks=0; foreach(int x in Displays) AssertFrames(At(x));
			Point p=At(43); Check(Count(p)==0 && Drops().Count==0,"empty-trial-slot-no-preexisting-drops");
			TileObjectData d=TileObjectData.GetTileData(Cluster.Type,0);
			Check(d.Width==4 && d.Height==4 && d.CoordinateFullWidth==72 && d.CoordinateFullHeight==72 && d.Origin==new Terraria.DataStructures.Point16(1,3) && d.DrawYOffset==4,"registered-object-contract");
			Check(!Main.tileSolid[Cluster.Type] && !Main.tileSolidTop[Cluster.Type] && !Main.tileBlockLight[Cluster.Type] && Cluster.MinPick==0 && Cluster.MineResist==0.8f,"thorn-not-solid-starter-pick");
			Item item=new(ItemType); Check(item.createTile==Cluster.Type && item.consumable && item.maxStack==9999,"native-placeable-stackable-item");
			Place(p); Check(!WorldGen.PlaceObject(p.X+1,p.Y+3,Cluster.Type,mute:true),"occupied-placement-rejected");
			Main.instance.LoadTiles(Cluster.Type); var texture=TextureAssets.Tile[Cluster.Type].Value;
			Check(texture.Width==416 && texture.Height==144,"loaded-atlas-size");
			Color[] pixels=new Color[416*144];texture.GetData(pixels); int opaque=0; Point hit=default;
			for(int y=0;y<64;y++) for(int x=0;x<64;x++) {
				Color color=pixels[(y/16*18+y%16)*416+x/16*18+x%16]; bool expected=color.A==255;
				Rectangle point=new(p.X*16+x,p.Y*16+y+4,1,1);
				if(Cluster.TouchesAt(point,p)!=expected || Cluster.ContactMask[y*64+x]!=(expected?1:0)) throw new InvalidOperationException($"Pixel contact mismatch {x},{y}");
				if(expected){opaque++;if(y<56)hit=new Point(x,y);} // Hurt probe stays above the buried root.
			}
			Check(opaque==1478,"all-4096-render-pixels-vs-contact-points");
			Check(!Cluster.Touching(new Rectangle(p.X*16-1,p.Y*16,1,64)) && !Cluster.Touching(new Rectangle((p.X+4)*16,p.Y*16,1,64)),"exterior-air-safe");
			Check(!Collision.SolidCollision(p.ToVector2()*16,64,64),"no-invisible-solid-wall");
			Player probe=new(){whoAmI=Main.myPlayer,width=1,height=1,statLife=200,statLifeMax=200,statLifeMax2=200};
			probe.position=p.ToVector2()*16+new Vector2(hit.X,hit.Y+4);
			Tile part=Main.tile[p.X+hit.X/16,p.Y+hit.Y/16];part.IsActuated=true;
			Check(!Cluster.Touching(probe.Hitbox),"actuated-part-no-contact");part.IsActuated=false;
			int before=probe.statLife;double dealt=Cluster.ApplyContact(probe);
			Check(dealt>0 && probe.statLife<before && !probe.dead,$"native-hurt-health-loss-{before-probe.statLife}");
			int after=probe.statLife;Cluster.ApplyContact(probe);Check(probe.immune && probe.statLife==after,"native-immunity-no-overlap-multihit");
			Check(TileLoader.CanExplode(p.X,p.Y),"explosion-permission");
			WorldGen.KillTile(p.X,p.Y);Removed(p,"initial-one-item-recovery");
			for(int x=0;x<4;x++) for(int y=0;y<4;y++) {
				Place(p);int hits=0; while(Count(p)>0 && hits<10){Main.LocalPlayer.PickTile(p.X+x,p.Y+y,35);hits++;}
				Check(hits<=3,$"starter-pick-at-most-three-hits-{x}-{y}");Removed(p,$"whole-object-one-drop-{x}-{y}");
			}
			for(int x=0;x<4;x++) {
				WorldGen.KillTile(p.X+x,Floor,noItem:true);
				Check(!WorldGen.PlaceObject(p.X+1,p.Y+3,Cluster.Type,mute:true),"missing-support-rejected-"+x);
				WorldGen.PlaceTile(p.X+x,Floor,ModContent.TileType<MawDirt>(),mute:true);Place(p);
				WorldGen.KillTile(p.X+x,Floor,noItem:true);Removed(p,"support-loss-single-recovery-"+x);
				WorldGen.PlaceTile(p.X+x,Floor,ModContent.TileType<MawDirt>(),mute:true);
			}
			foreach(int x in Displays) AssertFrames(At(x));
			Mod.Logger.Info($"MAW CLUSTER MATRIX PASS: {checks} checks plus4096 native pixel/contact comparisons; four displays intact. Programmatic APIs, not manual/multiplayer certification.");
		}
		private void View(bool night)
		{
			Require();if(!viewing){oldPosition=Main.LocalPlayer.position;oldDay=Main.dayTime;oldTime=Main.time;oldRain=Main.raining;oldEclipse=Main.eclipse;viewing=true;}
			Main.dayTime=!night;Main.time=night?16000:27000;Main.raining=false;Main.eclipse=false;
			Main.LocalPlayer.Teleport(new Vector2((bounds.X+13)*16,Floor*16-Main.LocalPlayer.height),1);Main.LocalPlayer.velocity=Vector2.Zero;
			Main.NewText("Tooth cluster: one item left, three adjacent items right. Hazardous! Mine any piece to pick up and place again.",Color.Wheat);
		}
		public override void PostUpdateEverything()
		{
			if(!IsQa || captureDelay<0 || captureDelay--!=0)return;captureDelay=-1;
			CaptureManager.Instance.Capture(new CaptureSettings{Area=bounds,Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),CaptureBackground=true,CaptureEntities=true,UseScaling=true,OutputName="Apogean Maw Cluster "+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")});
		}
		internal void Release()
		{
			captureDelay=-1;if(!viewing)return;Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;
			if(IsQa)Main.LocalPlayer.Teleport(oldPosition,1);viewing=false;
		}
		public override void SaveWorldData(TagCompound tag){if(IsQa && !bounds.IsEmpty)tag["mawClusterV1"]=new TagCompound{["x"]=bounds.X,["y"]=bounds.Y};}
		public override void LoadWorldData(TagCompound tag){if(Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3" && tag.ContainsKey("mawClusterV1")){var t=tag.GetCompound("mawClusterV1");bounds=new Rectangle(t.GetInt("x"),t.GetInt("y"),60,24);}}
		public override void ClearWorld(){bounds=Rectangle.Empty;viewing=false;captureDelay=-1;}
	}
}
