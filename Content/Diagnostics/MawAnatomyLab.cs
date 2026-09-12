using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace apogean.Content.Diagnostics
{
    public sealed class MawAnatomyLab : ModSystem
    {
        private Rectangle bounds;
        internal Rectangle PreservedBounds => bounds;
        private string digest;
        private bool viewing,oldDay,oldRain,oldEclipse;
        private bool viewingVines,viewingAmber;
        private double oldTime;
        private Vector2 oldPosition;
        private int captureDelay=-1;
        private const string SaveKey="mawAnatomyStudyV1";
        private static bool IsQa=>Main.netMode==NetmodeID.SinglePlayer && Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3" && Main.LocalPlayer.name=="gg";
        private static int Type(string key)=>key is "rib" or "cap"?MawAnatomyMaterials.Tile(key).Type:MawPackedPreview.TileType(key);
        private static string PlanDigest()
        {
            using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
            foreach(var cell in MawAnatomyPlan.Create()){writer.Write(cell.Key??"air");writer.Write(cell.Slope);writer.Write(cell.Wall??"air");}
            writer.Flush();return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }
        private static bool Empty(Tile t)=>!t.HasTile&&t.WallType==WallID.None&&t.LiquidAmount==0&&!t.HasActuator&&!t.IsActuated&&
            !t.RedWire&&!t.BlueWire&&!t.GreenWire&&!t.YellowWire&&t.TileColor==PaintID.None&&t.WallColor==PaintID.None&&
            !t.IsTileInvisible&&!t.IsWallInvisible&&!t.IsTileFullbright&&!t.IsWallFullbright;
        private void Build()
        {
            if(!bounds.IsEmpty||digest!=null)throw new InvalidOperationException("Anatomy study already exists; no rebuild.");
            foreach(int dx in new[]{3200,-3200,3000,-3000}) {
                foreach(int dy in new[]{-440,-520,-320}) {
                    Rectangle site=new(Main.spawnTileX+dx,Main.spawnTileY+dy,MawAnatomyPlan.Width,MawAnatomyPlan.Height),envelope=site;envelope.Inflate(8,8);
                    if(envelope.Left<40||envelope.Top<40||envelope.Right>Main.maxTilesX-40||envelope.Bottom>Main.maxTilesY-40)continue;
                    bool empty=true;for(int x=envelope.Left;x<envelope.Right;x++)for(int y=envelope.Top;y<envelope.Bottom;y++)empty &= Empty(Main.tile[x,y]);
                    if(empty){bounds=site;break;}
                }
                if(!bounds.IsEmpty)break;
            }
            if(bounds.IsEmpty)throw new InvalidOperationException("No empty anatomy envelope; no terrain cleared.");
            var plan=MawAnatomyPlan.Create();digest=PlanDigest();
            for(int x=0;x<bounds.Width;x++)for(int y=0;y<bounds.Height;y++) {
                var c=plan[x,y];Tile t=Main.tile[bounds.X+x,bounds.Y+y];
                if(c.Key!=null){t.HasTile=true;t.TileType=(ushort)Type(c.Key);t.Slope=(SlopeType)c.Slope;}
                if(c.Wall!=null)t.WallType=(ushort)MawPackedPreview.Wall(c.Wall).Type;
            }
            WorldGen.RangeFrame(bounds.Left-1,bounds.Top-1,bounds.Right+1,bounds.Bottom+1);
            Mod.Logger.Info($"MAW ANATOMY BUILD: {bounds}; original={digest}; new empty envelope only; no worldgen.");
            Validate();View();
        }
        private void Validate()
        {
            if(bounds.Width!=MawAnatomyPlan.Width||bounds.Height!=MawAnatomyPlan.Height||digest!=PlanDigest()||
                bounds.Left<40||bounds.Top<40||bounds.Right>Main.maxTilesX-40||bounds.Bottom>Main.maxTilesY-40)
                throw new InvalidOperationException("Missing/changed anatomy contract; no rebuild/rebaseline.");
            var plan=MawAnatomyPlan.Create();int slopes=0,walls=0;
            for(int x=0;x<bounds.Width;x++)for(int y=0;y<bounds.Height;y++) {
                var c=plan[x,y];Tile t=Main.tile[bounds.X+x,bounds.Y+y];
                if(t.HasTile!=(c.Key!=null)||(c.Key!=null&&(t.TileType!=Type(c.Key)||(byte)t.Slope!=c.Slope||t.IsHalfBlock))||
                    t.WallType!=(c.Wall==null?WallID.None:MawPackedPreview.Wall(c.Wall).Type)||t.LiquidAmount!=0||t.HasActuator||t.IsActuated||
                    t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||
                    t.IsTileInvisible||t.IsTileFullbright||t.IsWallInvisible||t.IsWallFullbright)
                    throw new InvalidOperationException($"Anatomy scene differs at{x},{y}; preserved, not repaired.");
                if(c.Slope!=0)slopes++;if(c.Wall!=null)walls++;
            }
            Mod.Logger.Info($"MAW ANATOMY STATE PASS: all7600 cells exact; {slopes} native slopes; {walls} unsafe wall cells; digest={digest}. Not traversal/art approval.");
        }
        internal void Run(string request)
        {
            if(!IsQa||!MawAnatomyMaterials.Available)throw new InvalidOperationException("Anatomy needs packed study build, gg/V3/SP.");
            string grove=ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot();
            Mod.Logger.Info("MAW ANATOMY REQUEST: "+request);
            try {
                switch(request) {
                    case "build":Build();break;
                    case "test":Validate();MawAnatomyChecks.Run(bounds,Mod.Logger);Validate();break;
                    case "reload":Validate();break;
                    case "view":Validate();View();break;
                    case "capture":Validate();if(viewingVines)ModContent.GetInstance<MawHangingFiberStudy>().Validate();if(viewingAmber)ModContent.GetInstance<MawAmberLightStudy>().Validate();captureDelay=60;break;
                    case "release":Release();break;
                    case "vines-build":Validate();ModContent.GetInstance<MawHangingFiberStudy>().Build(bounds);break;
                    case "vines-grow":ModContent.GetInstance<MawHangingFiberStudy>().Grow();break;
                    case "vines-test":ModContent.GetInstance<MawHangingFiberStudy>().Test();break;
                    case "vines-probe":ModContent.GetInstance<MawHangingFiberStudy>().Test(false);break;
                    case "vines-solar":ModContent.GetInstance<MawHangingFiberStudy>().TestSolarCut();break;
                    case "vines-audit":ModContent.GetInstance<MawHangingFiberStudy>().Audit();break;
                    case "vines-reload":ModContent.GetInstance<MawHangingFiberStudy>().Validate();break;
                    case "vines-view":ModContent.GetInstance<MawHangingFiberStudy>().Validate();View();viewingVines=true;break;
                    case "amber-build":Validate();ModContent.GetInstance<MawAmberLightStudy>().Build(ModContent.GetInstance<MawHangingFiberStudy>().CheckedBounds());break;
                    case "amber-test":ModContent.GetInstance<MawAmberLightStudy>().Test();break;
                    case "amber-reload":ModContent.GetInstance<MawAmberLightStudy>().Validate();break;
                    case "amber-sample":ModContent.GetInstance<MawAmberLightStudy>().Sample();break;
                    case "amber-awake":case "amber-dormant":
                        ModContent.GetInstance<MawAmberLightStudy>().Validate();View();viewingAmber=true;
                        ModContent.GetInstance<MawAmberLightStudy>().SetPreview(request=="amber-dormant");
                        Main.dayTime=false;Main.time=18000;break;
                    default:throw new InvalidOperationException("Unknown anatomy request.");
                }
            } finally {
                if(grove!=ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot())throw new InvalidOperationException("Preserved grove changed.");
            }
            Mod.Logger.Info("MAW ANATOMY COMPLETE: "+request);
        }
        private Vector2 ViewPosition()=>viewingAmber?
            new Vector2(ModContent.GetInstance<MawAmberLightStudy>().Bounds.Center.X*16,(ModContent.GetInstance<MawAmberLightStudy>().Bounds.Top-6)*16):viewingVines?
            new Vector2((ModContent.GetInstance<MawHangingFiberStudy>().Bounds.X+48)*16,(ModContent.GetInstance<MawHangingFiberStudy>().Bounds.Y+12)*16):
            new Vector2((bounds.X+36)*16,(bounds.Y+38)*16-Main.LocalPlayer.height);
        internal bool TryLightingCamera(out Vector2 position)
        {
            position=default;
            if(!IsQa||!viewing||!viewingAmber)return false;
            // Keep equipment outside the sealed rooms while warming/rendering
            // the entire chamber. Player position is not a lighting-camera target.
            Rectangle area=ModContent.GetInstance<MawAmberLightStudy>().Bounds;
            position=area.Center.ToVector2()*16-new Vector2(Main.screenWidth,Main.screenHeight)*.5f;
            return true;
        }
        private void View()
        {
            viewingVines=false;viewingAmber=false;
            if(!viewing){oldPosition=Main.LocalPlayer.position;oldDay=Main.dayTime;oldTime=Main.time;oldRain=Main.raining;oldEclipse=Main.eclipse;viewing=true;}
            Main.LocalPlayer.Teleport(ViewPosition(),1);Main.LocalPlayer.velocity=Vector2.Zero;Main.dayTime=true;Main.time=27000;Main.raining=false;Main.eclipse=false;
            Main.NewText("Anatomy candidate: tapered cortical ribs, thick fiber cap. Right: old/new comparisons and separate unsafe walls. No worldgen or traversal claim.",Color.Wheat);
        }
        internal void Release()
        {
            captureDelay=-1;ModContent.GetInstance<MawAmberLightStudy>().SetPreview(null);if(!viewing)return;
            Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;
            if(IsQa)Main.LocalPlayer.Teleport(oldPosition,1);viewing=false;
        }
        public override void PostUpdateEverything()
        {
            if(!IsQa)return;
            if(viewing){Main.LocalPlayer.position=ViewPosition();Main.LocalPlayer.velocity=Vector2.Zero;}
            if(captureDelay<0||captureDelay--!=0)return;captureDelay=-1;
            CaptureManager.Instance.Capture(new CaptureSettings{Area=viewingAmber?ModContent.GetInstance<MawAmberLightStudy>().Bounds:viewingVines?ModContent.GetInstance<MawHangingFiberStudy>().Bounds:bounds,Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),CaptureBackground=true,
                CaptureEntities=true,UseScaling=true,OutputName="Apogean Maw Anatomy "+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")});
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3"&&digest!=null)tag[SaveKey]=new TagCompound{["x"]=bounds.X,["y"]=bounds.Y,["original"]=digest};
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||!tag.ContainsKey(SaveKey))return;
            var saved=tag.GetCompound(SaveKey);bounds=new(saved.GetInt("x"),saved.GetInt("y"),MawAnatomyPlan.Width,MawAnatomyPlan.Height);digest=saved.GetString("original");
        }
        public override void ClearWorld(){bounds=Rectangle.Empty;digest=null;viewing=false;viewingVines=false;viewingAmber=false;captureDelay=-1;}
    }
}
