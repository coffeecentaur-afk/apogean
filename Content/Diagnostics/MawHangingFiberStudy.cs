using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace apogean.Content.Diagnostics
{
    public sealed class MawHangingFiberStudy : ModSystem
    {
        internal Rectangle Bounds;
        private int steps;
        private Point? probe;
        internal static int Fiber=>ModContent.Find<ModTile>("apogean/MawHangingFiber").Type;
        private Point Root(int n)=>new(Bounds.X+8+n*12,Bounds.Y+4);
        internal static bool AllowedRoot(Point p)
        {
            if(Main.netMode!=NetmodeID.SinglePlayer||Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||Main.LocalPlayer.name!="gg")return false;
            var lab=ModContent.GetInstance<MawHangingFiberStudy>();
            if(lab.probe==p)return true;
            if(lab.Bounds.IsEmpty)return false;
            for(int n=0;n<7;n++)if(lab.Root(n)==p)return true;return false;
        }
        private static bool Empty(Tile t)=>!t.HasTile&&t.WallType==WallID.None&&t.LiquidAmount==0&&!t.HasActuator&&!t.IsActuated&&!t.RedWire&&!t.BlueWire&&!t.GreenWire&&!t.YellowWire&&
            t.TileColor==PaintID.None&&t.WallColor==PaintID.None&&!t.IsTileInvisible&&!t.IsWallInvisible&&!t.IsTileFullbright&&!t.IsWallFullbright;
        internal void Build(Rectangle parent)
        {
            if(!Bounds.IsEmpty)throw new InvalidOperationException("Hanging-fiber study exists; no rebuild.");
            Rectangle candidate=new(parent.Left,parent.Bottom+20,100,20),envelope=candidate;envelope.Inflate(3,3);
            if(!WorldGen.InWorld(envelope.Left,envelope.Top,30)||!WorldGen.InWorld(envelope.Right,envelope.Bottom,30))throw new InvalidOperationException("Vine bounds.");
            for(int x=envelope.Left;x<envelope.Right;x++)for(int y=envelope.Top;y<envelope.Bottom;y++)
                if(!Empty(Main.tile[x,y]))throw new InvalidOperationException("Vine exhibit envelope occupied; no writes.");
            Bounds=candidate;steps=0;
            for(int n=0;n<7;n++) {
                Point root=Root(n);
                for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=0;dy++) {
                    Tile t=Main.tile[root.X+dx,root.Y+dy];t.HasTile=true;
                    t.TileType=(ushort)MawPackedPreview.TileType(dy==0?"grass":"soil");
                }
            }
            WorldGen.RangeFrame(Bounds.Left,Bounds.Top,Bounds.Right,Bounds.Bottom);
            Validate();Mod.Logger.Info($"MAW HANGING BUILD: {Bounds}; seven roots; separate empty envelope; no natural growth timer.");
        }
        internal void Grow()
        {
            Validate();if(steps>=MawHangingFiber.MaxLength)return;
            for(int n=0;n<7;n++)if(!MawHangingFiber.Grow(Root(n),Fiber))throw new InvalidOperationException("Registered root did not grow.");
            steps++;Validate();
        }
        internal void Validate()
        {
            if(Bounds.Width!=100||Bounds.Height!=20||steps<0||steps>MawHangingFiber.MaxLength||
                !WorldGen.InWorld(Bounds.Left,Bounds.Top,30)||!WorldGen.InWorld(Bounds.Right,Bounds.Bottom,30))
                throw new InvalidOperationException("Missing/invalid hanging fixture.");
            int count=0;
            for(int x=0;x<100;x++)for(int y=0;y<20;y++) {
                int expected=-1,depth=0;
                for(int n=0;n<7;n++) {
                    int rx=8+n*12;
                    if(Math.Abs(x-rx)<=1&&y>=3&&y<=4)expected=MawPackedPreview.TileType(y==4?"grass":"soil");
                    if(x==rx&&y>4&&y<=4+steps){expected=Fiber;depth=y-4;}
                }
                Tile t=Main.tile[Bounds.X+x,Bounds.Y+y];
                if(expected<0){if(!Empty(t))throw new InvalidOperationException($"Unexpected hanging cell {x},{y}");continue;}
                if(!t.HasUnactuatedTile||t.TileType!=expected||t.Slope!=SlopeType.Solid||t.IsHalfBlock||t.WallType!=WallID.None||t.LiquidAmount!=0||
                    t.HasActuator||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||t.IsTileInvisible||t.IsWallInvisible||t.IsTileFullbright||t.IsWallFullbright)
                    throw new InvalidOperationException($"Hanging exhibit changed at {x},{y}; has={t.HasTile},type={t.TileType},actuated={t.IsActuated},slope={t.Slope},half={t.IsHalfBlock},liquid={t.LiquidAmount}/{t.LiquidType},paint={t.TileColor},frame={t.TileFrameX},{t.TileFrameY}; no repair.");
                if(depth>0){if(t.TileFrameX!=0||t.TileFrameY!=(depth-1)*18)throw new InvalidOperationException("Hanging frame/depth mismatch.");count++;}
            }
            Mod.Logger.Info($"MAW HANGING STATE PASS: all2000 cells; steps={steps}; fibers={count}; exact ordered frames; no auto-rebaseline.");
        }
        internal Rectangle CheckedBounds()
        {
            if(Bounds.Width!=100||Bounds.Height!=20||!WorldGen.InWorld(Bounds.Left,Bounds.Top,30)||!WorldGen.InWorld(Bounds.Right,Bounds.Bottom,30))throw new InvalidOperationException("Missing hanging bounds.");
            return Bounds;
        }
        private string Fingerprint()
        {
            CheckedBounds();var text=new System.Text.StringBuilder();
            for(int x=Bounds.Left;x<Bounds.Right;x++)for(int y=Bounds.Top;y<Bounds.Bottom;y++) {
                Tile t=Main.tile[x,y];text.Append($"{t.HasTile},{t.TileType},{t.WallType},{t.Slope},{t.IsHalfBlock},{t.TileFrameX},{t.TileFrameY},{t.WallFrameX},{t.WallFrameY},{t.LiquidAmount},{t.LiquidType},{t.HasActuator},{t.IsActuated},{t.RedWire},{t.BlueWire},{t.GreenWire},{t.YellowWire},{t.TileColor},{t.WallColor},{t.IsTileInvisible},{t.IsWallInvisible},{t.IsTileFullbright},{t.IsWallFullbright};");
            }
            return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text.ToString())));
        }
        internal void Audit()
        {
            CheckedBounds();
            for(int n=0;n<7;n++){Point root=Root(n);var s=new System.Text.StringBuilder();for(int d=0;d<=7;d++) {
                Tile t=Main.tile[root.X,root.Y+d];s.Append($"d{d}[has={t.HasTile},type={t.TileType},act={t.IsActuated},slope={t.Slope},liquid={t.LiquidAmount}/{t.LiquidType},frame={t.TileFrameX},{t.TileFrameY}] ");}
                Mod.Logger.Info($"MAW HANGING AUDIT: root{n} {root}: {s}");}
            Mod.Logger.Info("MAW HANGING AUDIT: current-only fingerprint="+Fingerprint()+"; not a replacement baseline.");
        }
        internal void Test(bool strictScene=true)
        {
            if(strictScene)Validate();else CheckedBounds();string before=Fingerprint();
            Point root=new(Bounds.X+92,Bounds.Y+4);Rectangle scratch=new(root.X-2,root.Y-2,5,12);
            for(int x=scratch.Left;x<scratch.Right;x++)for(int y=scratch.Top;y<scratch.Bottom;y++)if(!Empty(Main.tile[x,y]))throw new InvalidOperationException("Vine scratch occupied.");
            probe=root;int checks=0;
            void Require(bool condition,string name){if(!condition)throw new InvalidOperationException("Hanging check: "+name);checks++;}
            void Clear(){for(int x=scratch.Left;x<scratch.Right;x++)for(int y=scratch.Top;y<scratch.Bottom;y++)Main.tile[x,y].ClearEverything();}
            void Anchor(int type){Clear();Tile t=Main.tile[root];t.HasTile=true;t.TileType=(ushort)type;}
            void Mature(){for(int n=0;n<MawHangingFiber.MaxLength;n++)Require(MawHangingFiber.Grow(root,Fiber),"growth step");Require(!MawHangingFiber.Grow(root,Fiber),"length cap");}
            int Count(){int count=0;for(int y=1;y<=7;y++)if(Main.tile[root.X,root.Y+y].HasTile)count++;return count;}
            try {
                Require(!Main.tileSolid[Fiber]&&!Main.tileSolidTop[Fiber]&&!Main.tileRope[Fiber]&&Main.tileCut[Fiber]&&!TileID.Sets.VineThreads[Fiber],"cuttable rigid non-rope");
                foreach(string substrate in new[]{"soil","grass"}) {
                    int type=MawPackedPreview.TileType(substrate);
                    for(int shape=0;shape<6;shape++) {
                        Anchor(type);Tile shapedAnchor=Main.tile[root];shapedAnchor.Slope=(SlopeType)(shape==5?0:shape);shapedAnchor.IsHalfBlock=shape==5;
                        Require(MawHangingFiber.Grow(root,Fiber)==(shape!=3&&shape!=4),"flat-underside matrix");
                    }
                    foreach(int cut in new[]{1,3,6}) {
                        Anchor(type);Mature();WorldGen.KillTile(root.X,root.Y+cut,noItem:true);
                        Require(Count()==cut-1,"middle cut keeps prefix only");
                    }
                    Anchor(type);Mature();WorldGen.KillTile(root.X,root.Y,noItem:true);Require(Count()==0,"root removal cascades");
                    Anchor(type);Mature();Main.tile[root].TileType=TileID.Dirt;WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==0,"purified support clears");
                    Anchor(type);Mature();Tile changed=Main.tile[root];changed.IsActuated=true;WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==0,"actuated support clears");
                    Anchor(type);Mature();changed=Main.tile[root];changed.Slope=SlopeType.SlopeUpLeft;WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==0,"bottom-sloped support clears");
                    Anchor(type);Mature();Main.tile[root].TileType=(ushort)MawPackedPreview.TileType(substrate=="soil"?"grass":"soil");
                    WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==6,"allowed direct conversion retains fiber");
                }
                foreach(int liquid in new[]{LiquidID.Water,LiquidID.Lava,LiquidID.Honey,LiquidID.Shimmer})foreach(byte amount in new byte[]{1,255}) {
                    Anchor(MawPackedPreview.TileType("grass"));Tile t=Main.tile[root.X,root.Y+1];t.LiquidType=liquid;t.LiquidAmount=amount;
                    Require(!MawHangingFiber.Grow(root,Fiber),"wet destination denied");
                }
                // Exercise native liquid contact, not just the declared flags.
                foreach(int liquid in new[]{LiquidID.Water,LiquidID.Lava,LiquidID.Honey,LiquidID.Shimmer}) {
                    Anchor(MawPackedPreview.TileType("grass"));Mature();
                    Tile contact=Main.tile[root.X,root.Y+3];contact.LiquidType=liquid;contact.LiquidAmount=255;
                    Liquid.AddWater(root.X,root.Y+3);
                    Require(Count()==(liquid==LiquidID.Lava?2:6),"native liquid contact/detached suffix");
                }
                Anchor(MawPackedPreview.TileType("grass"));Tile staleLiquid=Main.tile[root.X,root.Y+1];staleLiquid.LiquidType=LiquidID.Lava;
                Require(MawHangingFiber.Grow(root,Fiber),"zero-amount stale liquid is dry");
                Anchor(MawPackedPreview.TileType("grass"));Tile a=Main.tile[root];a.TileColor=PaintID.RedPaint;a.IsTileInvisible=true;a.IsTileFullbright=true;
                Mature();for(int d=1;d<=6;d++){Tile t=Main.tile[root.X,root.Y+d];Require(t.TileColor==a.TileColor&&t.IsTileInvisible&&t.IsTileFullbright,"parent coating copied");}
                Anchor(MawPackedPreview.TileType("grass"));Mature();Main.tile[root.X,root.Y+3].ClearTile();Require(!MawHangingFiber.Grow(root,Fiber),"gap does not skip orphan");
                Anchor(MawPackedPreview.TileType("grass"));Mature();Tile overflow=Main.tile[root.X,root.Y+7];overflow.HasTile=true;overflow.TileType=(ushort)Fiber;
                WorldGen.SquareTileFrame(root.X,root.Y+7);Require(!overflow.HasTile,"overlength injected segment rejected");
                Require(!MawHangingFiber.Grow(new Point(root.X+1,root.Y),Fiber),"unregistered root denied");
            } finally {probe=null;Clear();if(before!=Fingerprint())throw new InvalidOperationException("Hanging probe changed preserved cells.");}
            if(strictScene)Validate();
            Mod.Logger.Info($"MAW HANGING LIFECYCLE PASS: {checks} native checks; support/cut/coating/length, dry-growth gates and native Liquid.AddWater contact; scratch restored. strictScene={strictScene}; a scratch-only pass does not clear a red saved exhibit. Fluid-flow time evolution and multiplayer remain untested.");
        }
        // Value copies, not Tile.Clone(): Tile is a handle into shared tile data.
        // Preserve even stale air frames/liquid flags in the seven owned cells.
        private readonly record struct SolarCell(TileTypeData Type,WallTypeData Wall,
            TileWallWireStateData State,LiquidData Liquid,TileWallBrightnessInvisibilityData Coating)
        {
            internal static SolarCell Read(Point p)
            {
                Tile t=Main.tile[p];
                return new(t.Get<TileTypeData>(),t.Get<WallTypeData>(),t.Get<TileWallWireStateData>(),
                    t.Get<LiquidData>(),t.Get<TileWallBrightnessInvisibilityData>());
            }
            internal void Restore(Point p)
            {
                Tile t=Main.tile[p];
                t.Get<TileTypeData>()=Type;t.Get<WallTypeData>()=Wall;t.Get<TileWallWireStateData>()=State;
                t.Get<LiquidData>()=Liquid;t.Get<TileWallBrightnessInvisibilityData>()=Coating;
            }
        }
        internal void TestSolarCut()
        {
            if(Main.dedServ||Main.gameMenu||WorldGen.gen||Main.netMode!=NetmodeID.SinglePlayer||
                Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||Main.LocalPlayer.name!="gg"||
                !Main.LocalPlayer.active||Main.LocalPlayer.dead||Main.LocalPlayer.channel||
                !MawPackedPreview.Enabled||!MawAnatomyMaterials.Available||probe.HasValue)
                throw new InvalidOperationException("Solar fiber probe requires idle gg/V3/SP, packed anatomy and no other vine probe.");
            Rectangle savedBounds=CheckedBounds();int savedSteps=steps;string before=Fingerprint();
            Point root=new(Bounds.X+92,Bounds.Y+4);
            Rectangle scratch=new(root.X-2,root.Y-2,5,12);
            // Installed tML 2026.07.3.0 / 666f699: SolarCounter (608) defaults
            // to 160x160; Kill IL5bfc-5c0b calls Damage, without resizing it.
            // CutTilesAt IL0000-0041 scans floor(left/top)..floor(right/bottom)
            // INCLUSIVELY, including the extra boundary tile at the far edge.
            Rectangle lower=new(root.X*16+8-80,(root.Y+4)*16,160,160);
            Rectangle outside=new(lower.X,(root.Y+8)*16,160,160);
            Rectangle CutScan(Rectangle hitbox)=>new(hitbox.Left/16,hitbox.Top/16,
                hitbox.Right/16-hitbox.Left/16+1,hitbox.Bottom/16-hitbox.Top/16+1);
            Rectangle envelope=Rectangle.Union(scratch,Rectangle.Union(CutScan(lower),CutScan(outside)));
            envelope.Inflate(3,3); // Includes scratch framing and neighbors of every potential cut.
            if(!WorldGen.InWorld(envelope.Left,envelope.Top,30)||!WorldGen.InWorld(envelope.Right,envelope.Bottom,30))
                throw new InvalidOperationException("Solar cutting/framing envelope outside safe world bounds.");
            Rectangle pixels=new(envelope.X*16,envelope.Y*16,envelope.Width*16,envelope.Height*16);
            bool Owned(int x,int y)=>x==root.X&&y>=root.Y&&y<=root.Y+6;
            var original=new SolarCell[envelope.Width*envelope.Height];
            int Index(int x,int y)=>(y-envelope.Top)*envelope.Width+x-envelope.Left;
            for(int x=envelope.Left;x<envelope.Right;x++)for(int y=envelope.Top;y<envelope.Bottom;y++) {
                if(!Empty(Main.tile[x,y]))throw new InvalidOperationException($"Solar cutting/framing envelope occupied at {x},{y}; no writes.");
                original[Index(x,y)]=SolarCell.Read(new Point(x,y));
            }
            void GuardActors(Projectile owned=null)
            {
                foreach(Player player in Main.player)if(player.active&&pixels.Intersects(player.Hitbox))
                    throw new InvalidOperationException("Player in Solar cutting/framing envelope; no burst.");
                foreach(NPC npc in Main.npc)if(npc.active&&pixels.Intersects(npc.Hitbox))
                    throw new InvalidOperationException("NPC in Solar cutting/framing envelope; no burst.");
                foreach(Projectile p in Main.projectile)if(p.active&&!ReferenceEquals(p,owned)&&pixels.Intersects(p.Hitbox))
                    throw new InvalidOperationException("Unowned projectile in Solar cutting/framing envelope; no burst.");
            }
            GuardActors();int checks=0,bursts=0;bool ownsScratch=false;
            void Require(bool condition,string name)
            {
                if(!condition)throw new InvalidOperationException("Solar fiber check: "+name);
                checks++;
            }
            void CheckUnowned()
            {
                for(int x=envelope.Left;x<envelope.Right;x++)for(int y=envelope.Top;y<envelope.Bottom;y++)
                    if(!Owned(x,y)&&SolarCell.Read(new Point(x,y))!=original[Index(x,y)])
                        throw new InvalidOperationException($"Solar probe changed unowned envelope cell {x},{y}; not cleared or repaired.");
            }
            int fiber=Fiber,anchor=MawPackedPreview.TileType("grass");
            Require(MawHangingFiber.MaxLength==6&&Main.tileCut[fiber]&&!Main.tileSolid[fiber]&&!Main.tileSolidTop[fiber],"six cuttable non-solid segments");
            var mature=new SolarCell[7];
            void CheckStrand(int length,string stage)
            {
                Require(MawHangingFiber.Anchor(Main.tile[root]),stage+" anchor retained");
                for(int d=1;d<=6;d++) {
                    Point p=new(root.X,root.Y+d);Tile t=Main.tile[p];
                    Require(d<=length ? t.HasUnactuatedTile&&t.TileType==fiber&&t.TileFrameX==0&&
                        t.TileFrameY==(d-1)*18&&SolarCell.Read(p)==mature[d] : Empty(t),stage+" d"+d);
                }
                CheckUnowned();
            }
            void FrameRepeatedly(int length,string stage)
            {
                // Ordinary neighbor framing, never section-framing the saved exhibit.
                for(int pass=0;pass<3;pass++) {
                    for(int d=0;d<=7;d++)WorldGen.SquareTileFrame(root.X,root.Y+d,resetFrame:false);
                    CheckStrand(length,stage+" pass"+pass);
                }
            }
            void Burst(Rectangle expected,string stage)
            {
                GuardActors();CheckUnowned();
                // NewProjectile scans from slot zero, and replaces an existing
                // projectile if the pool is full. Never enter that fallback.
                int slot=-1;
                for(int n=0;n<Main.maxProjectiles;n++)if(!Main.projectile[n].active){slot=n;break;}
                Require(slot>=0,"free projectile slot before "+stage);
                int owner=Main.myPlayer;
                // Fresh native identity arrays contain zero, not necessarily -1.
                // Preserve the old binding, while refusing to overwrite a live identity.
                int binding=Main.projectileIdentity[owner,slot];
                Require(binding<0||binding>=Main.maxProjectiles||!Main.projectile[binding].active||
                    Main.projectile[binding].owner!=owner||Main.projectile[binding].identity!=slot,
                    "no live identity binding before "+stage);
                Projectile owned=Main.projectile[slot]; // Retain ownership even if OnSpawn throws.
                bool mining=Terraria.GameContent.Achievements.AchievementsHelper.CurrentlyMining;
                var cutting=DelegateMethods.tilecut_0;var ignore=DelegateMethods.tileCutIgnore;
                try {
                    int actual=Projectile.NewProjectile(Main.LocalPlayer.GetSource_Misc("Apogean Solar fiber scratch"),
                        new Vector2(expected.Center.X,expected.Center.Y),Vector2.Zero,ProjectileID.SolarCounter,1,0f,owner);
                    Require(actual==slot&&ReferenceEquals(Main.projectile[slot],owned)&&owned.active&&
                        owned.identity==slot&&owned.owner==owner&&owned.type==ProjectileID.SolarCounter,"owned native SolarCounter "+stage);
                    Require(owned.aiStyle==ProjAIStyleID.SolarEffect&&owned.friendly&&!owned.hostile&&!owned.npcProj&&!owned.minion&&
                        !owned.tileCollide&&owned.damage==1&&owned.velocity==Vector2.Zero&&owned.Hitbox==expected,
                        "unaltered 160x160 Solar defaults "+stage);
                    // Solar has no special Damage_GetHitbox inflation in this
                    // binary. Reject a mod's enlarged damage box before Kill.
                    Rectangle damage=owned.Hitbox;ProjectileLoader.ModifyDamageHitbox(owned,ref damage);
                    Require(damage==expected&&owned.Hitbox==expected,"native damage bounds "+stage);
                    GuardActors(owned);CheckUnowned();
                    owned.Kill(); // Native Kill -> Damage -> CutTiles -> CutTilesAt. No direct tile kill.
                    Require(!owned.active&&owned.timeLeft==0,"native projectile terminated "+stage);
                    bursts++;
                } finally {
                    // Never invoke Kill again from cleanup: it would repeat damage.
                    // The preselected inactive object is exclusively owned by this synchronous call.
                    owned.active=false;owned.timeLeft=0;
                    if(Main.projectileIdentity[owner,slot]==slot||Main.projectileIdentity[owner,slot]==-1)
                        Main.projectileIdentity[owner,slot]=binding;
                    Terraria.GameContent.Achievements.AchievementsHelper.CurrentlyMining=mining;
                    DelegateMethods.tilecut_0=cutting;DelegateMethods.tileCutIgnore=ignore;
                }
                Require(!Main.projectile[slot].active&&Main.projectileIdentity[owner,slot]==binding,"no projectile/identity leftover "+stage);
            }
            try {
                // Seed exactly one new scratch root/strand. Never call Build,
                // Grow or Validate on the preserved, deliberately red exhibit.
                probe=root;ownsScratch=true;
                for(int d=0;d<=6;d++) {
                    Tile t=Main.tile[root.X,root.Y+d];t.HasTile=true;t.TileType=(ushort)(d==0?anchor:fiber);
                    t.Slope=SlopeType.Solid;t.IsHalfBlock=false;t.TileFrameX=0;t.TileFrameY=(short)(d==0?0:(d-1)*18);
                }
                for(int d=0;d<=7;d++)WorldGen.SquareTileFrame(root.X,root.Y+d,resetFrame:false);
                for(int d=0;d<=6;d++) {
                    Point p=new(root.X,root.Y+d);mature[d]=SolarCell.Read(p);
                    Rectangle cell=new(p.X*16,p.Y*16,16,16);
                    Require(lower.Intersects(cell)==(d>=4)&&CutScan(lower).Contains(p)==(d>=4),"only d4+ in damage/cutting bounds, d"+d);
                    Require(!outside.Intersects(cell)&&!CutScan(outside).Contains(p),"outside-envelope negative geometry, d"+d);
                }
                CheckStrand(6,"initial six");FrameRepeatedly(6,"framing-only control");
                Burst(outside,"outside-envelope negative");CheckStrand(6,"outside-envelope negative");
                FrameRepeatedly(6,"framing after negative");
                Burst(lower,"lower d4+ positive");CheckStrand(3,"native Solar cut");
                FrameRepeatedly(3,"framing after cut");Require(bursts==2,"both native bursts executed");
            } finally {
                probe=null;
                if(ownsScratch)for(int d=0;d<=6;d++)original[Index(root.X,root.Y+d)].Restore(new Point(root.X,root.Y+d));
                // Read-only outside the seven owned cells, including on failure.
                string after=Fingerprint();
                if(Bounds!=savedBounds||steps!=savedSteps||before!=after)
                    throw new InvalidOperationException("Solar probe changed the preserved exhibit fingerprint/state; no rebaseline or repair.");
                CheckUnowned();
                for(int d=0;d<=6;d++)if(SolarCell.Read(new Point(root.X,root.Y+d))!=original[Index(root.X,root.Y+d)])
                    throw new InvalidOperationException("Solar probe did not restore an owned scratch cell.");
            }
            Mod.Logger.Info($"MAW HANGING SOLAR PASS: {checks} checks; native SolarCounter NewProjectile/Kill x{bursts}; outside-envelope negative kept6; lower d4+ damage/cutting kept d1-3 and removed d4-6; three repeated framing passes at each stage; scratch/identity restored; preserved fingerprint={before}. Mechanism reproduction only: original Solar event, capture trigger and multiplayer NOT proven; original exhibit remains RED.");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3"&&!Bounds.IsEmpty)tag["mawHangingFiberV1"]=new TagCompound{["x"]=Bounds.X,["y"]=Bounds.Y,["steps"]=steps};
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||!tag.ContainsKey("mawHangingFiberV1"))return;
            var saved=tag.GetCompound("mawHangingFiberV1");Bounds=new(saved.GetInt("x"),saved.GetInt("y"),100,20);steps=saved.GetInt("steps");
        }
        public override void ClearWorld(){Bounds=Rectangle.Empty;steps=0;probe=null;}
    }
}
