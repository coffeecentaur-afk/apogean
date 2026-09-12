using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Explicit synchronous QA command, not a draw/update hook. Setup, warmup,
    // identity checks and logging are outside every allocation-counting window.
    internal static class MawRenderAllocationProbe
    {
        private const int Calls = 4096;
        private const int WarmupCalls = 4096;
        private readonly record struct DrawInput(int X, int Y, short FrameX, short FrameY);
        private readonly record struct DrawOutput(int Width, int OffsetY, int Height, short FrameX, short FrameY);
        private readonly record struct LookupOutput(int Type, bool SameInstance);
        private readonly record struct Measurement(long Bytes, bool OutputsEqual);

        internal static void Run(Mod mod)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            if (Main.dedServ || Main.gameMenu || WorldGen.gen || !MawPackedPreview.Enabled ||
                Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" ||
                !Main.LocalPlayer.active || Main.LocalPlayer.dead || Main.LocalPlayer.name != "gg")
                throw new InvalidOperationException("Render allocation probe requires packed, active gg/V3/single-player.");
            // The caller must reject this command during recording as well.
            if (ModContent.GetInstance<QAPerformanceLab>().Recording || CaptureManager.Instance.IsCapturing)
                throw new InvalidOperationException("Run allocation probes separately from passive timing and captures.");

            Rectangle bounds = ModContent.GetInstance<MawAmberLightStudy>().Bounds;
            bool hasAmberBounds = bounds.Width == 100 && bounds.Height == 52 &&
                WorldGen.InWorld(bounds.Left, bounds.Top, 30) && WorldGen.InWorld(bounds.Right, bounds.Bottom, 30);
            // Draw hooks only read their supplied frame/coordinate arguments.
            // Missing amber evidence must not discard those useful draw cases.
            Point drawOrigin = hasAmberBounds ? bounds.Location : new Point(40, 40);

            CheckIdentities(mod, "before");
            int measured = 0, skipped = 0;
            bool outputsEqual = true, hooksZero = true, controlsZero = true;
            mod.Logger.Info($"QA RENDER ALLOCATION START: callsPerCase={Calls}; warmupCallsPerCase={WarmupCalls}; current-thread allocated bytes, not retained memory. Draw inputs are synthetic valid native frames at bounded coordinates; light inputs are existing saved emitters. No world writes, requests, readbacks or forced GC.");

            foreach (string key in new[] { "soil", "stone", "grass", "bone" }) {
                ModTile tile = TileLoader.GetTile(MawPackedPreview.TileType(key));
                PackedMaterialMap map = MawTerrainStudies.Tile(key).Map;
                if (tile == null || tile.Mod != mod || map == null) {
                    LogCase(mod, new { kind = "draw", key, skipped = true,
                        reason = "Registered production tile/map unavailable." });
                    skipped++;
                    continue;
                }

                var inputs = new DrawInput[64];
                var before = new DrawOutput[inputs.Length];
                var direct = new DrawOutput[inputs.Length];
                bool frameEquality = true;
                for (int n = 0; n < inputs.Length; n++) {
                    // Cover every 8x8 atlas phase without placing a tile or
                    // pretending these are observed production terrain cells.
                    inputs[n] = new(drawOrigin.X + n % 8, drawOrigin.Y + n / 8,
                        (short)(n % map.Columns * 18), (short)(n / map.Columns % map.Rows * 18));
                    direct[n] = MapDraw(map, inputs[n]);
                    before[n] = Draw(tile, inputs[n]);
                    frameEquality &= before[n] == direct[n];
                }

                Measurement hook = Measure(n => Draw(tile, inputs[n]), before);
                Measurement control = Measure(n => MapDraw(map, inputs[n]), direct);
                for (int n = 0; n < inputs.Length; n++)
                    frameEquality &= Draw(tile, inputs[n]) == before[n] && MapDraw(map, inputs[n]) == direct[n];
                frameEquality &= hook.OutputsEqual && control.OutputsEqual;
                LogCase(mod, new { kind = "draw", key, type = tile.Type, calls = Calls,
                    hookBytes = hook.Bytes, directMapBytes = control.Bytes, frameEquality,
                    hookZeroAllocation = hook.Bytes == 0,
                    scope = "Actual registered ModTile.SetDrawPositions; synthetic valid frames, all 64 phases, geometry refs included in equality." });
                measured++;
                outputsEqual &= frameEquality;
                hooksZero &= hook.Bytes == 0;
                controlsZero &= control.Bytes == 0;
            }

            ModContent.TryFind<ModTile>("apogean/MawAmberLitTile", out ModTile amberTile);
            ModContent.TryFind<ModWall>("apogean/MawAmberLitWall", out ModWall amberWall);
            Point? tileCell = null, wallCell = null;
            // At most 5200 reads, only inside the already-saved amber study.
            for (int x = bounds.Left; hasAmberBounds && x < bounds.Right && (!tileCell.HasValue || !wallCell.HasValue); x++)
                for (int y = bounds.Top; y < bounds.Bottom && (!tileCell.HasValue || !wallCell.HasValue); y++) {
                    Tile cell = Main.tile[x, y];
                    bool dormant = MawAmberMaterials.Dormant(x, y);
                    if (!tileCell.HasValue && amberTile is MawAmberLitTile litTile && cell.HasUnactuatedTile &&
                        cell.TileType == litTile.Type && litTile.Emission(x, y, dormant).X > 0)
                        tileCell = new Point(x, y);
                    if (!wallCell.HasValue && amberWall is MawAmberLitWall litWall && cell.WallType == litWall.Type &&
                        litWall.Emission(x, y, dormant).X > 0)
                        wallCell = new Point(x, y);
                }

            if (tileCell.HasValue && amberTile is MawAmberLitTile tileEmitter &&
                ReferenceEquals(TileLoader.GetTile(tileEmitter.Type), tileEmitter)) {
                Point p = tileCell.Value;
                bool dormant = MawAmberMaterials.Dormant(p.X, p.Y);
                Measurement hook = LightCase(mod, "amber-tile", p, MawTerrainStudies.Tile("amber").Map, false,
                    () => TileLight(amberTile, p), () => tileEmitter.Emission(p.X, p.Y, dormant), out bool equal, out bool zeroControl);
                measured++; outputsEqual &= equal; hooksZero &= hook.Bytes == 0; controlsZero &= zeroControl;
            } else {
                LogCase(mod, new { kind = "light", name = "amber-tile", skipped = true,
                    reason = hasAmberBounds ? "No registered positive emitter in saved amber bounds." : "Missing/invalid saved amber bounds; no scan or repair." });
                skipped++;
            }
            if (wallCell.HasValue && amberWall is MawAmberLitWall wallEmitter &&
                ReferenceEquals(WallLoader.GetWall(wallEmitter.Type), wallEmitter)) {
                Point p = wallCell.Value;
                bool dormant = MawAmberMaterials.Dormant(p.X, p.Y);
                Measurement hook = LightCase(mod, "amber-wall", p, MawTerrainStudies.Wall("amber").Map, true,
                    () => WallLight(amberWall, p), () => wallEmitter.Emission(p.X, p.Y, dormant), out bool equal, out bool zeroControl);
                measured++; outputsEqual &= equal; hooksZero &= hook.Bytes == 0; controlsZero &= zeroControl;
            } else {
                LogCase(mod, new { kind = "light", name = "amber-wall", skipped = true,
                    reason = hasAmberBounds ? "No registered positive emitter in saved amber bounds." : "Missing/invalid saved amber bounds; no scan or repair." });
                skipped++;
            }

            // This deliberately allocates like the former RESOLVER only. It is
            // not an A/B run of old native hooks and is not subtracted from them.
            ResolverControls(mod, ref outputsEqual, ref controlsZero);
            CheckIdentities(mod, "after");
            LogCase(mod, new { kind = "summary", measuredHookCases = measured, skippedHookCases = skipped,
                completeCoverage = measured == 6 && skipped == 0, outputsEqual,
                currentHooksZeroAllocation = measured > 0 && hooksZero, currentControlsZeroAllocation = controlsZero,
                scope = "Nonzero bytes are not a zero-allocation pass. No FPS, GPU, residency, old-hook speedup or reload-cycle verdict; rerun identities after each content reload." });
        }

        private static void LogCase(Mod mod, object metrics) =>
            mod.Logger.Info("QA RENDER ALLOCATION CASE: " + JsonSerializer.Serialize(metrics));

        private static void CheckIdentities(Mod mod, string stage)
        {
            if (MawTerrainStudies.Materials.Length != 12)
                throw new InvalidOperationException("Render allocation probe expects all twelve material keys.");
            foreach (string key in MawTerrainStudies.Materials) {
                var tile = MawTerrainStudies.Tile(key);
                var wall = MawTerrainStudies.Wall(key);
                if (!ReferenceEquals(tile, ModContent.Find<ModTile>("apogean/Study_" + key)) ||
                    !ReferenceEquals(wall, ModContent.Find<ModWall>("apogean/StudyWall_" + key)) ||
                    !ReferenceEquals(tile, TileLoader.GetTile(tile.Type)) ||
                    !ReferenceEquals(wall, WallLoader.GetWall(wall.Type)) || tile.Mod != mod || wall.Mod != mod ||
                    tile.Map == null || wall.Map == null)
                    throw new InvalidOperationException("Study cache/registry identity mismatch: " + key);
            }
            mod.Logger.Info($"QA RENDER CACHE IDENTITY: stage={stage}; materials=12; registeredInstances=24; referenceEquality=True; mapsAlreadyLoaded=True; current content load only.");
        }

        private static DrawOutput Draw(ModTile tile, DrawInput input)
        {
            int width = 16, offsetY = 0, height = 16;
            short x = input.FrameX, y = input.FrameY;
            tile.SetDrawPositions(input.X, input.Y, ref width, ref offsetY, ref height, ref x, ref y);
            return new(width, offsetY, height, x, y);
        }

        private static DrawOutput MapDraw(PackedMaterialMap map, DrawInput input)
        {
            if (!map.TryMap(input.X, input.Y, input.FrameX, input.FrameY, out short x, out short y))
                throw new InvalidOperationException("Allocation probe input is not a valid native map frame.");
            return new(16, 0, 16, x, y);
        }

        private static Vector3 TileLight(ModTile tile, Point p)
        {
            float r = 0, g = 0, b = 0;
            tile.ModifyLight(p.X, p.Y, ref r, ref g, ref b);
            return new(r, g, b);
        }

        private static Vector3 WallLight(ModWall wall, Point p)
        {
            float r = 0, g = 0, b = 0;
            wall.ModifyLight(p.X, p.Y, ref r, ref g, ref b);
            return new(r, g, b);
        }

        private static Measurement LightCase(Mod mod, string name, Point p, PackedMaterialMap map, bool wall,
            Func<Vector3> hook, Func<Vector3> emission, out bool equal, out bool zeroControl)
        {
            Tile cell = Main.tile[p];
            int nativeX = wall ? cell.WallFrameX : cell.TileFrameX;
            int nativeY = wall ? cell.WallFrameY : cell.TileFrameY;
            if (!map.TryMap(p.X, p.Y, nativeX, nativeY, out short x, out short y))
                throw new InvalidOperationException("Saved amber emitter has an unmapped native frame.");
            var input = new DrawInput(p.X, p.Y, (short)nativeX, (short)nativeY);
            DrawOutput mapped = MapDraw(map, input);
            Vector3 before = hook(), expected = emission();
            Measurement actual = Measure(_ => hook(), new[] { before });
            Measurement directEmission = Measure(_ => emission(), new[] { expected });
            Measurement directMap = Measure(_ => MapDraw(map, input), new[] { mapped });
            Vector3 after = hook();
            equal = before == expected && after == before && emission() == expected &&
                actual.OutputsEqual && directEmission.OutputsEqual && directMap.OutputsEqual && MapDraw(map, input) == mapped &&
                (wall ? cell.WallFrameX == nativeX && cell.WallFrameY == nativeY : cell.TileFrameX == nativeX && cell.TileFrameY == nativeY);
            zeroControl = directEmission.Bytes == 0 && directMap.Bytes == 0;
            LogCase(mod, new { kind = "light", name, cell = new { x = p.X, y = p.Y },
                packedFrame = new { x, y }, calls = Calls, hookBytes = actual.Bytes,
                directEmissionBytes = directEmission.Bytes, directMapBytes = directMap.Bytes,
                rgbBefore = new { r = before.X, g = before.Y, b = before.Z },
                rgbAfter = new { r = after.X, g = after.Y, b = after.Z },
                outputAndFrameEquality = equal, hookZeroAllocation = actual.Bytes == 0,
                scope = "Actual registered ModifyLight. Emission is the existing helper/control, not an independent pixel oracle; current dormancy only." });
            return actual;
        }

        private static Measurement Measure<T>(Func<int, T> invoke, T[] expected) where T : struct, IEquatable<T>
        {
            // Delegate/array construction happens at the call site, before this
            // window. Warm the same path and value equality used by measurement.
            bool warmEqual = true;
            for (int n = 0; n < WarmupCalls; n++) {
                int index = n % expected.Length;
                warmEqual &= invoke(index).Equals(expected[index]);
            }
            bool equal = warmEqual;
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int n = 0; n < Calls; n++) {
                int index = n % expected.Length;
                equal &= invoke(index).Equals(expected[index]);
            }
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            return new(bytes, equal);
        }

        private static void ResolverControls(Mod mod, ref bool outputsEqual, ref bool controlsZero)
        {
            string[] keys = MawTerrainStudies.Materials;
            var tiles = new ModTile[keys.Length];
            var walls = new ModWall[keys.Length];
            var expectedTiles = new LookupOutput[keys.Length];
            var expectedWalls = new LookupOutput[keys.Length];
            for (int n = 0; n < keys.Length; n++) {
                tiles[n] = ModContent.Find<ModTile>("apogean/Study_" + keys[n]);
                walls[n] = ModContent.Find<ModWall>("apogean/StudyWall_" + keys[n]);
                expectedTiles[n] = new(tiles[n].Type, true);
                expectedWalls[n] = new(walls[n].Type, true);
            }
            Measurement cachedTile = Measure(n => {
                ModTile value = MawTerrainStudies.Tile(keys[n]);
                return new LookupOutput(value.Type, ReferenceEquals(value, tiles[n]));
            }, expectedTiles);
            Measurement cachedWall = Measure(n => {
                ModWall value = MawTerrainStudies.Wall(keys[n]);
                return new LookupOutput(value.Type, ReferenceEquals(value, walls[n]));
            }, expectedWalls);
            Measurement legacyTile = Measure(n => {
                ModTile value = LegacyTile(keys[n]);
                return new LookupOutput(value.Type, ReferenceEquals(value, tiles[n]));
            }, expectedTiles);
            Measurement legacyWall = Measure(n => {
                ModWall value = LegacyWall(keys[n]);
                return new LookupOutput(value.Type, ReferenceEquals(value, walls[n]));
            }, expectedWalls);
            bool equal = cachedTile.OutputsEqual && cachedWall.OutputsEqual && legacyTile.OutputsEqual && legacyWall.OutputsEqual;
            outputsEqual &= equal;
            controlsZero &= cachedTile.Bytes == 0 && cachedWall.Bytes == 0;
            LogCase(mod, new { kind = "resolver-control", callsPerCase = Calls, materialsCycled = 12,
                cachedTileBytes = cachedTile.Bytes, cachedWallBytes = cachedWall.Bytes,
                deliberateLegacyTileBytes = legacyTile.Bytes, deliberateLegacyWallBytes = legacyWall.Bytes,
                identityEquality = equal, legacyAllocationObserved = legacyTile.Bytes > 0 && legacyWall.Bytes > 0,
                scope = "Equivalent old name-construction/Find resolver only, NOT old native hooks; no subtraction or speedup claim." });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ModTile LegacyTile(string key) => ModContent.Find<ModTile>("apogean/Study_" + key);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ModWall LegacyWall(string key) => ModContent.Find<ModWall>("apogean/StudyWall_" + key);
    }
}
