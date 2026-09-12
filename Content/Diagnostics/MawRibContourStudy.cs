using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using S = apogean.Content.Diagnostics.MawShallowTraversalLab;

namespace apogean.Content.Diagnostics
{
    // Separate finite, empty-air specimen. Not a worldgen pass or a shallowV1 replacement.
    public sealed class MawRibContourStudy : ModSystem
    {
        private const string SaveKey="mawRibContourStudyV1";
        private const int Width=124,Height=82;
        private Rectangle bounds;
        private string creation,saved;
        private bool failed,held,visiting,lamp,oldDay,oldRain,oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private int row,captureDelay=-1;
        internal Rectangle PreservedBounds=>bounds;
        internal bool PlainVisit=>S.IsQa&&MawPackedPreview.Enabled&&visiting&&Main.LocalPlayer.name==MawShallowQaScope.Plain;
        private Rectangle Envelope { get { var r=bounds;r.Inflate(12,12);return r; } }
        private Rectangle Panel=>new(bounds.X,bounds.Y+row*22+4,Width,28);
        private Vector2 ViewPosition=>new((bounds.X+60)*16,(bounds.Y+row*22+12)*16);
        private static int RibType=>MawAnatomyMaterials.Tile("rib").Type;
        private static int BoneType=>MawPackedPreview.TileType("bone");
        private static int StoneType=>MawPackedPreview.TileType("stone");
        private readonly record struct Cell(int Type,byte Slope);
        private static int Length(int row)=>row switch {0=>8,1=>11,_=>12};
        private static int Start(int col)=>col switch {0=>9,1=>39,2=>73,_=>103};
        private static Cell?[,] Plan()
        {
            var p=new Cell?[Width,Height];
            for(int r=0;r<3;r++)for(int col=0;col<4;col++) {
                int length=Length(r),dir=col<2?1:-1,left=Start(col),top=14+r*22;
                var cells=col%2==0?MawRibContour.Legacy(length,dir):MawRibContour.Create(length,dir);
                foreach(var c in cells)p[left+c.X,top+c.Y]=new(c.Root?BoneType:RibType,c.Slope);
                // Narrow, solid backing proves actual insertion without concealing the rib contour.
                int root=dir==1?left-2:left+length;
                for(int x=root;x<root+2;x++)for(int y=top-2;y<top+7;y++)p[x,y]=new(StoneType,0);
            }
            return p;
        }
        private Rectangle[] History()=>S.Historical().Where(r=>!r.IsEmpty&&r!=bounds)
            .Append(ModContent.GetInstance<S>().PreservedBounds).Where(r=>!r.IsEmpty).ToArray();
        private void Build()
        {
            if(Main.LocalPlayer.name!="gg")throw new InvalidOperationException("Only gg constructs a new specimen; Plain never builds.");
            if(!bounds.IsEmpty||creation!=null)throw new InvalidOperationException("Contour specimen already reserved; no rebuild/rebaseline.");
            var p=Plan();var history=History();int attempts=0;
            foreach(int dx in new[]{-3080,-2800,-2520,-1960,-1680}) {
                foreach(int dy in new[]{-660,-540,-420,-300}) {
                    attempts++;var site=new Rectangle(Main.spawnTileX+dx,Main.spawnTileY+dy,Width,Height);var guard=site;guard.Inflate(12,12);
                    if(guard.Left<40||guard.Top<40||guard.Right>Main.maxTilesX-40||guard.Bottom>Main.maxTilesY-40)continue;
                    bool reject=false;
                    foreach(var old in history) {if(old.IsEmpty)continue;var reserved=old;reserved.Inflate(24,24);reject|=reserved.Intersects(guard);}
                    if(reject||!S.ActorsAbsent(guard)||GenVars.structures!=null&&!GenVars.structures.CanPlace(guard))continue;
                    for(int x=guard.Left;x<guard.Right;x++)for(int y=guard.Top;y<guard.Bottom;y++)reject|=!S.Empty(Main.tile[x,y]);
                    if(reject)continue;bounds=site;break;
                }
                if(!bounds.IsEmpty)break;
            }
            if(bounds.IsEmpty)throw new InvalidOperationException($"No empty contour site after {attempts} candidates; nothing cleared.");
            creation="reserved-v1";GenVars.structures?.AddProtectedStructure(Envelope);
            var envelope=Envelope;var original=new S.CellState[envelope.Width,envelope.Height];
            for(int x=0;x<envelope.Width;x++)for(int y=0;y<envelope.Height;y++)original[x,y]=S.CellState.Read(envelope.X+x,envelope.Y+y);
            try {
                for(int x=0;x<Width;x++)for(int y=0;y<Height;y++)if(p[x,y] is Cell c) {
                    Tile t=Main.tile[bounds.X+x,bounds.Y+y];t.HasTile=true;t.TileType=(ushort)c.Type;t.Slope=(SlopeType)c.Slope;
                }
                WorldGen.RangeFrame(bounds.Left-1,bounds.Top-1,bounds.Right+1,bounds.Bottom+1);
                Pristine();creation=S.Fingerprint(new[]{bounds});
                Mod.Logger.Info($"MAW RIB CONTOUR BUILD: bounds={bounds}; attempts={attempts}; creation={creation}; 8/11/12 tiles; old/new right then old/new left; no walls to conceal silhouette.");
            }catch {
                failed=true;
                for(int x=0;x<envelope.Width;x++)for(int y=0;y<envelope.Height;y++)original[x,y].Restore(envelope.X+x,envelope.Y+y);
                Mod.Logger.Error("MAW RIB CONTOUR ROLLBACK: restored only owned empty envelope; failed reservation retained.");throw;
            }
        }
        private void Require()
        {
            if(failed||creation==null||bounds.Width!=Width||bounds.Height!=Height||!WorldGen.InWorld(bounds.Left,bounds.Top,30)||!WorldGen.InWorld(bounds.Right,bounds.Bottom,30))
                throw new InvalidOperationException("Missing/failed contour specimen; no automatic build.");
        }
        private void Pristine()
        {
            Require();var p=Plan();int count=0;
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++) {
                Tile t=Main.tile[bounds.X+x,bounds.Y+y];var c=p[x,y];
                bool bad=t.HasTile!=c.HasValue||t.WallType!=WallID.None||t.LiquidAmount!=0||t.IsHalfBlock||t.HasActuator||t.IsActuated||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||
                    t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||t.IsTileInvisible||t.IsWallInvisible||t.IsTileFullbright||t.IsWallFullbright;
                if(c.HasValue){count++;bad|=t.TileType!=c.Value.Type||(byte)t.Slope!=c.Value.Slope;}
                if(bad)throw new InvalidOperationException($"Contour original differs at {x},{y}; not repaired.");
            }
            Mod.Logger.Info($"MAW RIB CONTOUR PRISTINE PASS: {Width*Height} exact cells, {count} solid cells; old control retained beside candidate.");
        }
        internal void Run(string request)
        {
            if(!S.IsQa||!MawPackedPreview.Enabled||!MawAnatomyMaterials.Available||ModContent.GetInstance<QAPerformanceLab>().Recording)
                throw new InvalidOperationException("Contour study requires idle packed V3/SP/gg or Plain.");
            if(!MawShallowQaScope.Request(Main.LocalPlayer.name,"maw-contour-"+request))throw new InvalidOperationException("Contour request denied.");
            var history=History();string before=S.Fingerprint(history);Mod.Logger.Info("MAW RIB CONTOUR REQUEST: "+request);
            try {
                switch(request) {
                    case "build":Build();break;
                    case "pristine":Pristine();break;
                    case "reload":Require();if(saved==null||saved!=S.Fingerprint(new[]{bounds}))throw new InvalidOperationException("Contour saved interaction state differs; not repaired.");Mod.Logger.Info("MAW RIB CONTOUR RELOAD PASS: exact saved state.");break;
                    case "short":case "medium":case "long":Require();row=request=="short"?0:request=="medium"?1:2;Visit();break;
                    case "inspect-on":case "inspect-off":
                        Require();if(!held||!PlainVisit||!MawShallowMotionProbe.PlainBaseline(Main.LocalPlayer))throw new InvalidOperationException("Contour lamp requires held Plain baseline.");
                        lamp=request=="inspect-on";Mod.Logger.Info($"MAW RIB CONTOUR INSPECTION: lamp={lamp}; diagnostic lights only, NOT natural illumination.");break;
                    case "capture":Require();if(!held)throw new InvalidOperationException("Choose a held row first.");captureDelay=90;break;
                    case "release":Release();break;
                    default:throw new InvalidOperationException("Unknown contour request.");
                }
            }finally {
                if(before!=S.Fingerprint(history))throw new InvalidOperationException("Historical scene changed during contour command; not repaired.");
                Mod.Logger.Info($"MAW RIB CONTOUR PRESERVATION: {history.Length} earlier bounds unchanged, including shallowV1.");
            }
            Mod.Logger.Info("MAW RIB CONTOUR COMPLETE: "+request);
        }
        private void Visit()
        {
            if(Collision.SolidCollision(ViewPosition,Main.LocalPlayer.width,Main.LocalPlayer.height))throw new InvalidOperationException("Contour view obstructed; not clearing.");
            if(!visiting){oldPosition=Main.LocalPlayer.position;oldDay=Main.dayTime;oldTime=Main.time;oldRain=Main.raining;oldEclipse=Main.eclipse;visiting=true;}
            held=true;Main.LocalPlayer.Teleport(ViewPosition,1);Main.LocalPlayer.velocity=Vector2.Zero;
        }
        internal bool TryCamera(out Vector2 camera)
        {
            camera=default;if(!S.IsQa||!held)return false;
            camera=Panel.Center.ToVector2()*16-new Vector2(Main.screenWidth,Main.screenHeight)*.5f;return true;
        }
        internal void Release()
        {
            held=lamp=false;captureDelay=-1;if(!visiting)return;
            if(S.IsQa){Main.LocalPlayer.Teleport(oldPosition,1);Main.LocalPlayer.velocity=Vector2.Zero;}
            Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;visiting=false;
        }
        public override void PostUpdateEverything()
        {
            if(!S.IsQa||!held)return;
            Main.LocalPlayer.position=ViewPosition;Main.LocalPlayer.velocity=Vector2.Zero;
            Main.dayTime=true;Main.time=27000;Main.raining=false;Main.eclipse=false;
            if(lamp&&PlainVisit&&MawShallowMotionProbe.PlainBaseline(Main.LocalPlayer))
                for(int col=0;col<4;col++)Lighting.AddLight(new Vector2((bounds.X+Start(col)+Length(row)/2)*16,(bounds.Y+16+row*22)*16),1.2f,1.1f,.9f);
            if(captureDelay<0||captureDelay--!=0)return;
            CaptureManager.Instance.Capture(new CaptureSettings {Area=Panel,Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
                CaptureBackground=true,CaptureEntities=true,UseScaling=true,OutputName="Apogean Maw Rib Contour "+row+(lamp?"-inspection":"")+" "+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")});
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name!=MawShallowQaScope.World||creation==null)return;
            if(WorldGen.InWorld(bounds.Left,bounds.Top,30)&&WorldGen.InWorld(bounds.Right,bounds.Bottom,30))saved=S.Fingerprint(new[]{bounds});else{failed=true;saved??="invalid-bounds";}
            tag[SaveKey]=new TagCompound{["x"]=bounds.X,["y"]=bounds.Y,["version"]=1,["creation"]=creation,["saved"]=saved,["failed"]=failed};
            Mod.Logger.Info($"MAW RIB CONTOUR SAVE: creation={creation}; actual={saved}; failed={failed}.");
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name!=MawShallowQaScope.World||!tag.ContainsKey(SaveKey))return;
            var t=tag.GetCompound(SaveKey);bounds=new(t.GetInt("x"),t.GetInt("y"),Width,Height);creation=t.GetString("creation");saved=t.GetString("saved");failed=t.GetBool("failed")||t.GetInt("version")!=1;
        }
        public override void ClearWorld(){bounds=Rectangle.Empty;creation=saved=null;failed=held=visiting=lamp=false;captureDelay=-1;}
    }
}
