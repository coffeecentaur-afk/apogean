using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Explicit dry ordinary-cave seam experiment. No world generation, wall removal,
    // player teleport, shader, new render target, liquid redraw, or production route.
    public sealed class MawCaveBackdropProbe : ModSystem
    {
        private const string Key = "apogean:qa-dry-cave-seam-v1";
        private const string AssetPath = "apogean/Content/Diagnostics/CaveBackdrop/master";
        private const int Width=96, Height=64, Margin=2, Pitch=Width+Margin*2+1;
        private Rectangle region;
        private Vector2 pan;
        private string mode="off", original;
        private bool held, zoomed;
        private int callbacks, dryCells, excludedCells, runs, attempts;
        private readonly int[] wetPrefix=new int[(Width+Margin*2+1)*(Height+Margin*2+1)];
        private Asset<Texture2D> master;
        private ProbeOverlay overlay;
        private double searchMilliseconds;
        private object lastFrame;
        private static bool IsQa => !Main.gameMenu && Main.netMode==NetmodeID.SinglePlayer &&
            Main.ActiveWorldFileData?.Name==MawShallowQaScope.World && Main.LocalPlayer.name=="gg" && MawPackedPreview.Enabled;

        internal void Run(string request)
        {
            if(!IsQa || CaptureManager.Instance.IsCapturing || ModContent.GetInstance<QAPerformanceLab>().Recording)
                throw new InvalidOperationException("Dry cave probe requires idle packed gg/V3/SP normal frames.");
            if(request=="locate") {
                if(held)throw new InvalidOperationException("Release the previous probe before locating; no automatic replacement.");
                FindExistingAperture(); held=true; pan=Vector2.Zero; zoomed=false; mode="baseline"; callbacks=0;
            } else if(request=="release") {Release();return;}
            else {
                if(!held)throw new InvalidOperationException("Locate an existing aperture first; no scene is constructed.");
                switch(request) {
                    case "baseline": mode="baseline";overlay?.Deactivate();break;
                    case "pattern":case "art":
                        if(request=="art") {
                            if(!ModContent.HasAsset(AssetPath))throw new InvalidOperationException("Explicit cave study asset missing from this package.");
                            master??=ModContent.Request<Texture2D>(AssetPath,AssetRequestMode.ImmediateLoad);
                            if(master.Value.Width!=Width*16||master.Value.Height!=Height*16)throw new InvalidOperationException("Cave master dimensions differ; no fitting/scaling.");
                        }
                        EnsureOverlay(); CheckCompetition(); mode=request;
                        Overlays.Scene.Activate(Key);break;
                    case "pan":pan=new Vector2(16,-16);break;
                    case "origin":pan=Vector2.Zero;break;
                    case "zoom":zoomed=true;break;
                    case "normal-zoom":zoomed=false;break;
                    case "report":break;
                    default:throw new InvalidOperationException("Unknown cave probe command.");
                }
            }
            Report(request);
        }

        private void FindExistingAperture()
        {
            var timer=Stopwatch.StartNew();attempts=0;int best=-1;Rectangle chosen=Rectangle.Empty;
            var history=MawShallowTraversalLab.Historical().Where(r=>!r.IsEmpty).ToArray();
            int top=(int)Main.worldSurface+40, bottom=Math.Min((int)Main.rockLayer+160,Main.maxTilesY-300);
            foreach(int dx in new[]{0,-128,128,-256,256,-384,384,-512,512,-640,640,-768,768}) {
                for(int y=top;y+Height<bottom && y<top+480;y+=32) {
                    var r=new Rectangle(Main.spawnTileX+dx-Width/2,y,Width,Height); attempts++;
                    if(!WorldGen.InWorld(r.Left,r.Top,40)||!WorldGen.InWorld(r.Right,r.Bottom,40)||history.Any(h=>h.Intersects(r)))continue;
                    int openings=0,solid=0,walled=0,wet=0;
                    for(int x=r.Left;x<r.Right;x+=2)for(int yy=r.Top;yy<r.Bottom;yy+=2) {
                        Tile t=Main.tile[x,yy]; if(t.LiquidAmount>0){wet++;continue;}
                        if(t.HasTile && Main.tileSolid[t.TileType])solid++;
                        else if(t.WallType==WallID.None)openings++;else walled++;
                    }
                    // Prefer a real mixed natural aperture, not empty sky or a solid block.
                    if(openings<100||solid<70||walled<20)continue;
                    int score=Math.Min(openings,700)+Math.Min(walled,160)-wet;
                    if(score>best){best=score;chosen=r;}
                    if(timer.Elapsed.TotalSeconds>1)break; // bounded read-only search, no retry loop
                }
                if(timer.Elapsed.TotalSeconds>1)break;
            }
            searchMilliseconds=timer.Elapsed.TotalMilliseconds;
            if(chosen.IsEmpty)throw new InvalidOperationException($"No mixed cave aperture in {attempts} bounded candidates. No tiles changed.");
            region=chosen;original=MawShallowTraversalLab.Fingerprint(new[]{region});
        }

        private void EnsureOverlay()
        {
            if(overlay!=null)return;
            if(Overlays.Scene[Key]!=null)throw new InvalidOperationException("Probe key already bound. Restart the game; do not replace an unknown effect.");
            overlay=new ProbeOverlay(this);Overlays.Scene[Key]=overlay;
        }
        private void CheckCompetition()
        {
            // Read-only QA inspection: the public manager offers no enumeration.
            // Refuse on unknown layout; never change another mod's effect or priority.
            var field=typeof(EffectManager<Overlay>).GetField("_effects",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
            if(field?.GetValue(Overlays.Scene) is not Dictionary<string,Overlay> registered)
                throw new InvalidOperationException("Cannot establish overlay exclusivity on this engine build.");
            var scheduledField=typeof(OverlayManager).GetField("_activeOverlays",BindingFlags.Instance|BindingFlags.NonPublic);
            if(scheduledField?.GetValue(Overlays.Scene) is not LinkedList<Overlay>[] scheduled)
                throw new InvalidOperationException("Cannot establish scheduled overlay membership on this engine build.");
            // SimpleOverlay.IsVisible reads shader opacity, not manager membership.
            // An unscheduled registered shader cannot be evicted by our activation.
            foreach(var list in scheduled)foreach(var other in list)
                if(other!=overlay)
                    throw new InvalidOperationException("Another overlay is scheduled: "+registered.FirstOrDefault(p=>p.Value==other).Key+". No priority override permitted.");
            Mod.Logger.Info("MAW CAVE OVERLAY INVENTORY: "+JsonSerializer.Serialize(registered.Select(p=>new {
                key=p.Key,mode=p.Value.Mode.ToString(),visible=p.Value.IsVisible(),opacity=p.Value.Opacity,
                scheduled=scheduled.Any(list=>list.Contains(p.Value)),priority=p.Value.Priority.ToString(),layer=p.Value.Layer.ToString()})));
        }

        internal bool TryCamera(out Vector2 camera)
        {
            camera=default;if(!held||!IsQa)return false;
            camera=region.Center.ToVector2()*16-new Vector2(Main.screenWidth,Main.screenHeight)*.5f+pan;return true;
        }
        public override void ModifyTransformMatrix(ref Terraria.Graphics.SpriteViewMatrix transform)
        {
            if(held&&IsQa&&zoomed)transform.Zoom*=1.25f; // temporary matrix only, never writes saved zoom settings
        }
        public override void PostUpdateEverything()
        {
            if(!held)return;
            if(!IsQa){Release();return;}
            // Same bounded inspection lights for baseline/pattern/art. No tile emissions changed.
            for(int x=6;x<Width;x+=12)for(int y=4;y<Height;y+=8)
                Lighting.AddLight(new Vector2((region.X+x)*16,(region.Y+y)*16),1.1f,1.05f,1f);
        }

        private void Draw(SpriteBatch batch,float opacity)
        {
            if(!held||!IsQa||mode=="baseline"||CaptureManager.Instance.IsCapturing||Main.mapFullscreen)return;
            // Update an allocation-free wet integral image from current cells every draw.
            // Exclude every cell within TWO tiles of any liquid. This is a conservative
            // dry test, NOT a proven complete animated liquid-footprint mask.
            Array.Clear(wetPrefix);int width=Width+Margin*2,height=Height+Margin*2;
            for(int y=0;y<height;y++) {
                int row=0;
                for(int x=0;x<width;x++) {
                    row+=Main.tile[region.X-Margin+x,region.Y-Margin+y].LiquidAmount>0?1:0;
                    wetPrefix[(y+1)*Pitch+x+1]=wetPrefix[y*Pitch+x+1]+row;
                }
            }
            dryCells=excludedCells=runs=0;
            for(int y=0;y<Height;y++) {
                int start=-1;
                for(int x=0;x<=Width;x++) {
                    bool dry=x<Width&&Dry(x,y);
                    if(dry){dryCells++;if(start<0)start=x;}else if(x<Width)excludedCells++;
                    if(start>=0&&(!dry||x==Width)) {
                        var source=new Rectangle(start*16,y*16,(x-start)*16,16);
                        Vector2 position=new Vector2(region.X*16+source.X,region.Y*16+source.Y)-Main.screenPosition;
                        if(mode=="art")batch.Draw(master.Value,position,source,Color.White*opacity);
                        else batch.Draw(TextureAssets.MagicPixel.Value,new Rectangle((int)position.X,(int)position.Y,source.Width,16),
                            (y%8<4?new Color(40,115,145):new Color(155,70,100))*opacity);
                        runs++;start=-1;
                    }
                }
            }
            callbacks++;
            if(callbacks==1 || callbacks%300==0) {
                var device=Main.instance.GraphicsDevice;var m=Main.GameViewMatrix.TransformationMatrix;
                lastFrame=new { mode,callbacks,dryCells,excludedCells,runs,Main.drawToScreen,Main.offScreenRange,
                    camera=new {x=Main.screenPosition.X,y=Main.screenPosition.Y},Main.screenWidth,Main.screenHeight,
                    viewport=new {device.Viewport.X,device.Viewport.Y,device.Viewport.Width,device.Viewport.Height},
                    zoom=new{x=Main.GameViewMatrix.Zoom.X,y=Main.GameViewMatrix.Zoom.Y},
                    matrix=new[]{m.M11,m.M12,m.M21,m.M22,m.M41,m.M42},
                    defaultSampler=Main.DefaultSamplerState.Filter.ToString(),renderTargetCount=device.GetRenderTargets().Length };
            }
        }
        private bool Dry(int x,int y)
        {
            int x0=x,y0=y,x1=x+Margin*2+1,y1=y+Margin*2+1;
            return wetPrefix[y1*Pitch+x1]-wetPrefix[y0*Pitch+x1]-wetPrefix[y1*Pitch+x0]+wetPrefix[y0*Pitch+x0]==0;
        }
        private void Report(string request)
        {
            var result=new {schemaVersion=1,utc=DateTime.UtcNow,request,mode,held,zoomed,pan=new{x=pan.X,y=pan.Y},
                region=new{region.X,region.Y,region.Width,region.Height},attempts,searchMilliseconds,original,
                current=region.IsEmpty?null:MawShallowTraversalLab.Fingerprint(new[]{region}),lastFrame,
                overlayMode=overlay?.Mode.ToString(),opacity=overlay?.Opacity,
                limits="QA-only callback/occlusion probe. No world writes or player teleport; fixed inspection lights. Natural simulation may change cells. Scale1 pre-transform world pixels; 1.25 alternate matrix. Dry exclusion only. Photo CaptureManager omits Overlay; inspect completed normal game frames. No production art/routing/repeat/lighting/wet-scene approval."};
            string path=Path.Combine(Main.SavePath,"Captures","cave-probe-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N")[..6]+".json");
            File.WriteAllText(path,JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));
            Mod.Logger.Info($"MAW CAVE PROBE COMPLETE: {request}; mode={mode}; callbacks={callbacks}; report={path}");
        }
        internal void Release()
        {
            if(!held)return;
            mode="off";held=zoomed=false;pan=Vector2.Zero;overlay?.Deactivate();Report("release");
        }
        public override void OnWorldUnload(){Release();region=Rectangle.Empty;original=null;lastFrame=null;}
        public override void Unload(){held=false;master=null;overlay?.Detach();overlay=null;}

        private sealed class ProbeOverlay : Overlay
        {
            private MawCaveBackdropProbe owner;
            internal ProbeOverlay(MawCaveBackdropProbe owner):base(EffectPriority.VeryLow,RenderLayers.Background){this.owner=owner;}
            public override void Draw(SpriteBatch batch)=>owner?.Draw(batch,Opacity);
            public override void Update(GameTime time){if(owner==null||!owner.held||!IsQa)Deactivate();}
            public override bool IsVisible()=>owner!=null&&owner.held&&IsQa&&Opacity>0;
            public override void Activate(Vector2 position,params object[] args){Mode=OverlayMode.FadeIn;}
            public override void Deactivate(params object[] args){Mode=OverlayMode.FadeOut;}
            internal void Detach(){owner=null;Opacity=0;Deactivate();}
        }
    }
}
