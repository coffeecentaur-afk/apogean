using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Explicit short QA input sequence. Terraria owns acceleration, gravity,
    // slopes, collision and damage. Never assign position/velocity during it.
    public sealed class MawShallowMotionProbe : ModPlayer
    {
        private const int Limit = 360;
        private Sample[] samples;
        private (int type, int prefix)[] equipmentState;
        private Rectangle scene;
        private int count, controlled, airborne, settled, startLife;
        private long started;
        private bool active, braked, baselineStart, connector;
        private Vector2 start;
        private string equipment;
        private readonly record struct Sample(int Tick, float X, float Y, float Vx, float Vy,
            bool Right, bool Left, bool Grounded, bool Teeth, int Life);
        internal bool Active => active;
        private bool Context => Player.whoAmI == Main.myPlayer && MawShallowTraversalLab.IsQa && MawPackedPreview.Enabled;
        internal static bool PlainBaseline(Player p)
        {
            if (p.name != MawShallowQaScope.Plain || p.difficulty != 0 || p.statLifeMax != 100 ||
                p.statLifeMax2 != 100 || p.statManaMax != 20 || p.wingsLogic != 0) return false;
            foreach(Item i in p.armor) if(!i.IsAir) return false;
            foreach(Item i in p.miscEquips) if(!i.IsAir) return false;
            foreach(Item i in p.inventory) if(!i.IsAir && i.type is not (ItemID.CopperShortsword or ItemID.CopperPickaxe or ItemID.CopperAxe)) return false;
            for(int i=0;i<p.buffType.Length;i++) if(p.buffType[i]!=0 && p.buffTime[i]>0) return false;
            return true;
        }
        private string Equipment()
        {
            string result = "";
            foreach (Item i in Player.armor) result += i.type + ":" + i.prefix + ",";
            return result;
        }
        private bool SameEquipment()
        {
            if (equipmentState == null || Player.armor.Length != equipmentState.Length) return false;
            for (int i=0;i<equipmentState.Length;i++)
                if (Player.armor[i].type != equipmentState[i].type || Player.armor[i].prefix != equipmentState[i].prefix) return false;
            return true;
        }
        internal void Start(Rectangle bounds, bool connectorOut = false)
        {
            if (!Context || active || Player.dead || Player.mount.Active || Player.width != 20 || Player.height != 42 || Player.gravDir != 1 || Player.pulley || Player.grapCount != 0)
                throw new InvalidOperationException("Entry motion requires living unmounted/unhooked QA player with ordinary20x42 gravity; no loadout is changed.");
            baselineStart=PlainBaseline(Player);
            if(Player.name==MawShallowQaScope.Plain && !baselineStart) throw new InvalidOperationException("Plain motion requires fresh Classic100HP starter-only inventory, no equipment/buffs. Nothing is removed or granted.");
            if (ModContent.GetInstance<QAPerformanceLab>().Recording) throw new InvalidOperationException("Do not overlap this input probe with passive timings.");
            scene = bounds; connector = connectorOut; start = Player.position; startLife = Player.statLife; equipment = Equipment();
            samples=new Sample[Limit]; equipmentState=new (int,int)[Player.armor.Length];
            for(int i=0;i<equipmentState.Length;i++) equipmentState[i]=(Player.armor[i].type,Player.armor[i].prefix);
            count = controlled = airborne = settled = 0; braked = false; started = Stopwatch.GetTimestamp(); active = true;
            Mod.Logger.Info($"MAW SHALLOW MOTION START: route={Route}; max360 updates/10s; actual player inputs only; player={Player.name}; plainBaseline={baselineStart}; start={start}; equipment={equipment}; no gear, immunity, speed, gravity or damage overrides. Not whole-descent or difficulty approval.");
        }
        public override void SetControls()
        {
            if (!active) return;
            if (!Context || Player.dead || Main.gamePaused || !Main.instance.IsActive ||
                Stopwatch.GetElapsedTime(started).TotalSeconds > 10 || Player.mount.Active || Player.gravDir != 1 || Player.pulley || Player.grapCount != 0) {
                Cancel("context-pause-timeout-or-movement-mode"); return;
            }
            // Two native traces at x32 coasted beyond the rib before landing.
            // Release at x30 so native momentum still has room to dissipate;
            // there is no velocity override, snap-to-surface or terrain change.
            if (connector) {
                // Steer through the narrow middle shaft before approaching the lower exit.
                // The stopping estimate decides INPUT only; actual friction/collision remain native.
                bool lower = Player.position.Y >= (scene.Y + 63) * 16;
                float target = (scene.X + (lower ? 59 : 70)) * 16;
                float delta = target - Player.Center.X;
                float stopping = Math.Max(3, Player.velocity.X * Player.velocity.X / .2f + 2);
                bool coast = delta * Player.velocity.X > 0 && Math.Abs(delta) <= stopping;
                Player.controlRight = !coast && delta > 3;
                Player.controlLeft = !coast && delta < -3;
                braked = lower && Math.Abs(delta) <= 6;
            } else {
                if (Player.Center.X >= (scene.X + 30) * 16) braked = true;
                Player.controlRight = !braked; Player.controlLeft = false;
            }
            Player.controlUp = Player.controlDown = Player.controlJump = false;
            Player.controlHook = Player.controlMount = Player.controlUseItem = Player.controlUseTile = Player.controlThrow = false;
            controlled++;
        }
        public override void PostUpdate()
        {
            if (!active) return;
            if (!Context || count >= Limit) { Cancel("context-or-tick-budget"); return; }
            if (!SameEquipment()) { Cancel("equipment-changed"); return; }
            if (baselineStart && !PlainBaseline(Player)) { Cancel("plain-baseline-changed"); return; }
            Vector2 local = Player.position - scene.Location.ToVector2() * 16;
            bool grounded = Math.Abs(Player.velocity.Y) < .001f && Collision.SolidCollision(Player.position + new Vector2(0,2),20,42);
            bool tooth = ModContent.GetInstance<apogean.Content.Tiles.MawToothClusterTile>().Touching(Player.Hitbox);
            samples[count] = new(count,local.X,local.Y,Player.velocity.X,Player.velocity.Y,Player.controlRight,Player.controlLeft,grounded,tooth,Player.statLife);
            count++;
            if (Player.velocity.Y > .25f) airborne++;
            if (braked && grounded && Player.position.Y - start.Y > 200) settled++; else settled = 0;
            bool outside = connector ? local.X < 54*16 || local.X > 90*16 || local.Y < 36*16 || local.Y > 72*16 :
                local.X < 4*16 || local.X > 108*16 || local.Y < 0 || local.Y > 48*16;
            if (outside || Player.dead) { Cancel("left-route-test-envelope-or-died"); return; }
            if (settled >= 12) Finish("landed", true);
            else if (count >= Limit) Finish("tick-budget", false);
        }
        internal void Cancel(string reason) { if (active) Finish(reason,false); }
        private string Route => connector ? "connector-out" : "entry";
        private void Finish(string reason, bool landed)
        {
            if (!active) return; active = false;
            Player.controlRight = Player.controlLeft = false;
            bool pass = landed && controlled > 0 && airborne > 10 && count >= 12 &&
                SameEquipment() && Player.position.Y - start.Y > 200;
            try {
                string folder = Path.Combine(Main.SavePath,"Captures");
                string path = Path.Combine(folder,"Apogean Maw Shallow Motion " + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".json");
                var evidence = new { schemaVersion=3, route=Route, utc=DateTime.UtcNow, reason, pass, controlled, airborne, settled,
                    baselineStart, baselineEnd=PlainBaseline(Player),
                    scope="One named actual walk/fall/landing route. Plain baseline is asserted separately; NOT return traversal, full descent, manual play, vanilla-only modpack or difficulty acceptance.",
                    world=Main.ActiveWorldFileData?.Name, player=Player.name, equipment, startLife, endLife=Player.statLife,
                    scene=new {scene.X,scene.Y,scene.Width,scene.Height}, samples=samples.AsSpan(0,count).ToArray() };
                string temp=path+".partial";
                using (var f=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)) JsonSerializer.Serialize(f,evidence,new JsonSerializerOptions {WriteIndented=true});
                File.Move(temp,path);
                Mod.Logger.Info($"MAW SHALLOW MOTION {(pass ? "PASS" : "STOP")}: route={Route}; {reason}; {count} actual updates/{controlled} controlled; airborne={airborne}; settled={settled}; life={startLife}->{Player.statLife}; evidence={Path.GetFileName(path)}.");
            } catch(Exception e) { Mod.Logger.Error("MAW SHALLOW MOTION EXPORT FAILED: result not certified.",e); }
            finally { samples=null; equipmentState=null; }
        }
        public override void Initialize() { active=false; count=0; samples=null; equipmentState=null; }
    }
}
