using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.WorldGeneration;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
    // Read/visit controls for separately generated candidate worlds. Never builds terrain.
    public sealed class MawSeedReview : ModSystem
    {
        private bool visiting, lamp, oldDay, oldRain, oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private int poll, capture = -1;
        private static MawSeedWorld World => ModContent.GetInstance<MawSeedWorld>();
        private static bool Context => !Main.gameMenu && Main.netMode == NetmodeID.SinglePlayer &&
            MawSeedWorld.IsTestWorldName(Main.ActiveWorldFileData?.Name) && World.HasLayout &&
            Main.LocalPlayer.name is "gg" or "Maw QA Plain";
        internal bool Visiting => Context && visiting;

        internal void Run(string command)
        {
            if(!Context)throw new InvalidOperationException("Maw seed review requires a generated candidate world, gg/Plain, and single player.");
            Mod.Logger.Info("MAW SEED REVIEW REQUEST: "+command);
            switch(command) {
                case "entrance": case "ribs": case "cache": case "node": case "outlet": Visit(command); break;
                case "pristine": Mod.Logger.Info(World.VerifyOriginal()); break;
                case "reload": Mod.Logger.Info(World.VerifySaved()); break;
                case "light-on": case "light-off":
                    if(!visiting)throw new InvalidOperationException("Visit the candidate first.");
                    lamp=command=="light-on";
                    Main.NewText(lamp?"Local inspection lamp ON — not natural biome lighting.":"Inspection lamp OFF — natural lighting.",Color.Wheat); break;
                case "capture": if(!visiting)throw new InvalidOperationException("Visit before capture."); capture=20; break;
                case "release": Release(); break;
                case "save-and-quit": Release();WorldGen.SaveAndQuit();break;
                default: throw new InvalidOperationException("Unknown review command; no action.");
            }
            Mod.Logger.Info("MAW SEED REVIEW COMPLETE: "+command);
        }
        private void Visit(string view)
        {
            MawSeedPlan p=World.Blueprint;Rectangle bounds=World.Bounds;
            MawSeedPlan.Point at=view switch {
                "cache"=>new(p.Chests[0].X,p.Chests[0].Y-3),
                "node"=>new(p.NodeSites[0].X,p.NodeSites[0].Y-2),
                "outlet"=>p.Exit,
                "ribs"=>new(p.Center[65]-1,62),
                _=>p.Entrance
            };
            Vector2 target=new((bounds.X+at.X)*16,(bounds.Y+at.Y)*16);
            Rectangle body=new((int)target.X,(int)target.Y,Main.LocalPlayer.width,Main.LocalPlayer.height);
            if(Collision.SolidCollision(target,body.Width,body.Height)||ModContent.GetInstance<MawToothClusterTile>().Touching(body))
                throw new InvalidOperationException("Review destination is obstructed; no tile clearing or forced teleport.");
            if(!visiting) {oldPosition=Main.LocalPlayer.position;oldDay=Main.dayTime;oldTime=Main.time;oldRain=Main.raining;oldEclipse=Main.eclipse;visiting=true;}
            Main.LocalPlayer.Teleport(target,1);Main.LocalPlayer.velocity=Vector2.Zero;
            Main.dayTime=true;Main.time=27000;Main.raining=false;Main.eclipse=false;
            Main.NewText("Generated Maw candidate — FREE MOVEMENT. Cache is empty; node pocket is a reservation. Deep generation remains legacy.",Color.Wheat);
        }
        internal void Release()
        {
            lamp=false;capture=-1;
            if(!visiting)return;
            if(Context){Main.LocalPlayer.Teleport(oldPosition,1);Main.LocalPlayer.velocity=Vector2.Zero;}
            Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;visiting=false;
        }
        public override void PostUpdateEverything()
        {
            if(!Context)return;
            if(poll--<=0) {
                poll=30;string path=Path.Combine(Main.SavePath,"Captures","ApogeanLiveValidation.request");
                if(File.Exists(path)) {
                    try {
                        string request=File.ReadAllText(path).Trim();
                        // Consume once only in the exact review context. No eval or terrain commands.
                        File.Delete(path);
                        if(request=="qa-save-and-quit")Run("save-and-quit");
                        else if(request.StartsWith("maw-seed-",StringComparison.Ordinal)&&request.Length<=64)Run(request[9..]);
                        else throw new InvalidOperationException("Request is outside the seed-review allowlist.");
                    } catch(Exception error){Mod.Logger.Error("MAW SEED REVIEW FAILED",error);Main.NewText("Maw seed review stopped; see client.log. No automatic repair.",Color.OrangeRed);}
                }
            }
            if(!Visiting)return;
            if(lamp)Lighting.AddLight(Main.LocalPlayer.Center,1.5f,1.4f,1.2f);
            if(capture<0||capture--!=0)return;
            // Capture what was actually lit and inspected. A whole-biome capture can contain
            // unrendered black regions and must not be passed off as full-scene visual evidence.
            Rectangle visible = new((int)(Main.screenPosition.X/16), (int)(Main.screenPosition.Y/16),
                Math.Max(1,Main.screenWidth/16),Math.Max(1,Main.screenHeight/16));
            Rectangle area=Rectangle.Intersect(World.Bounds,visible);
            if(area.Width<=0||area.Height<=0){Mod.Logger.Warn("MAW SEED CAPTURE REFUSED: candidate is offscreen.");return;}
            string name="Apogean Maw Seed "+(lamp?"inspection-":"natural-")+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            CaptureManager.Instance.Capture(new CaptureSettings {Area=area,
                Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
                CaptureBackground=true,CaptureEntities=true,UseScaling=true,OutputName=name});
            Mod.Logger.Info("MAW SEED CAPTURE DISPATCHED: "+name+"; actual PNG/render inspection required.");
        }
        public override void OnWorldUnload(){visiting=lamp=false;capture=-1;poll=0;}
    }
    public sealed class MawSeedReviewCommand : ModCommand
    {
        public override string Command=>"mawseed";
        public override CommandType Type=>CommandType.Chat;
        public override string Usage=>"/mawseed entrance|ribs|cache|node|outlet|pristine|reload|light-on|light-off|capture|release";
        public override string Description=>"Inspect the separately generated Maw seed candidate without building or holding movement.";
        public override void Action(CommandCaller caller,string input,string[] args)
        {
            if(args.Length!=1||args[0] is not ("entrance" or "ribs" or "cache" or "node" or "outlet" or "pristine" or "reload" or "light-on" or "light-off" or "capture" or "release"))throw new UsageException(Usage);
            ModContent.GetInstance<MawSeedReview>().Run(args[0]);
        }
    }
    public sealed class MawSeedReviewAmbientGate : GlobalNPC
    {
        public override void EditSpawnRate(Player player,ref int spawnRate,ref int maxSpawns)
        {
            if(player.whoAmI==Main.myPlayer&&ModContent.GetInstance<MawSeedReview>().Visiting)maxSpawns=0;
        }
    }
}
