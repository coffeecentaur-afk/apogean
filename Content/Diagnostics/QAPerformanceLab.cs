using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Opt-in instrumentation, not a production performance feature. No forcing
    // GC, texture requests/readbacks, movement, world writes or graphics changes.
    public sealed class QAPerformanceLab : ModSystem
    {
        private const int Capacity = 16384;
        private const double WarmupSeconds = 2, SampleSeconds = 30;
        private double[] updates, draws;
        private int updateCount, drawCount, overflow, pausedCallbacks, inactiveCallbacks, captureCallbacks;
        private long started, updateStart, lastDraw;
        private bool running;
        internal bool Recording => running;
        private object before;
        private static bool IsQa => !Main.gameMenu && Main.netMode == NetmodeID.SinglePlayer &&
            Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg" && MawPackedPreview.Enabled;
        private static double Seconds(long from, long to) => (to - from) / (double)Stopwatch.Frequency;

        internal void Run(string command)
        {
            if (!IsQa) throw new InvalidOperationException("Performance recorder requires packed gg/V3/single-player.");
            if (command == "allocations") {
                if (running) throw new InvalidOperationException("Do not run an allocation probe during passive timing.");
                MawRenderAllocationProbe.Run(Mod);
            } else if (command == "snapshot") {
                if (running) throw new InvalidOperationException("Do not allocate a texture inventory during a timing sample.");
                Write("snapshot", new { schemaVersion = 1, utc = DateTime.UtcNow, scope = Scope(), memory = Memory(), textures = Textures() });
            } else if (command == "start") {
                if (running) throw new InvalidOperationException("Performance sample already running.");
                updates = new double[Capacity]; draws = new double[Capacity];
                updateCount = drawCount = overflow = pausedCallbacks = inactiveCallbacks = captureCallbacks = 0;
                updateStart = lastDraw = 0;
                before = new { memory = Memory(), scope = Scope() };
                started = Stopwatch.GetTimestamp(); running = true;
                Mod.Logger.Info("QA PERFORMANCE START: 2s warmup + 30s passive scene sample. Avoid captures, new QA commands and focus changes until COMPLETE.");
            } else if (command == "stop") {
                if (!running) throw new InvalidOperationException("No performance sample to stop.");
                Finish("manual-stop");
            } else throw new InvalidOperationException("Unknown performance command.");
        }

        public override void PreUpdateEntities()
        {
            if (running) updateStart = Stopwatch.GetTimestamp();
        }

        public override void PostUpdateEverything()
        {
            if (!running) return;
            long now = Stopwatch.GetTimestamp();
            if (!IsQa) { Finish("left-qa-context"); return; }
            double elapsed = Seconds(started, now);
            if (elapsed >= WarmupSeconds && updateStart > 0) {
                if (updateCount < Capacity) updates[updateCount++] = Seconds(updateStart, now) * 1000;
                else overflow++;
            }
            updateStart = 0;
            if (elapsed >= WarmupSeconds + SampleSeconds) Finish("duration-complete");
        }

        public override void PostDrawTiles()
        {
            if (!running) return;
            long now = Stopwatch.GetTimestamp();
            double elapsed = Seconds(started, now);
            if (Main.gamePaused) pausedCallbacks++;
            if (!Main.instance.IsActive) inactiveCallbacks++;
            if (CaptureManager.Instance.IsCapturing) captureCallbacks++;
            if (elapsed >= WarmupSeconds) {
                if (lastDraw != 0) {
                    if (drawCount < Capacity) draws[drawCount++] = Seconds(lastDraw, now) * 1000;
                    else overflow++;
                }
                lastDraw = now;
            }
            // Draw callbacks can still close a bounded sample when updates pause.
            if (elapsed >= WarmupSeconds + SampleSeconds) Finish("duration-complete");
        }

        private void Finish(string reason)
        {
            if (!running) return;
            running = false; // File IO/serialization is outside the measured interval.
            long ended = Stopwatch.GetTimestamp();
            bool usable = reason == "duration-complete" && updateCount >= 300 && drawCount >= 300 &&
                overflow == 0 && pausedCallbacks == 0 && inactiveCallbacks == 0 && captureCallbacks == 0;
            var result = new {
                schemaVersion = 1, utc = DateTime.UtcNow, reason, usable, elapsedSeconds = Seconds(started, ended),
                warmupSeconds = WarmupSeconds, targetSampleSeconds = SampleSeconds, overflow, pausedCallbacks, inactiveCallbacks, captureCallbacks,
                before, after = new { memory = Memory(), scope = Scope() },
                worldUpdateSlice = QASampleStatistics.Describe(updates, updateCount),
                worldDrawCallbackIntervals = QASampleStatistics.Describe(draws, drawCount),
                updateSamplesMs = updates.Take(updateCount).ToArray(), drawIntervalsMs = draws.Take(drawCount).ToArray(),
                limitations = "World-update hook slice and wall-clock intervals between PostDrawTiles, not GPU times, presentation times, whole-engine CPU cost or multiplayer/low-end acceptance. Process memory is not per-mod ownership. No active capture or competing probes permitted."
            };
            try { Write("timing", result); }
            catch (Exception exception) {
                Mod.Logger.Error("QA PERFORMANCE EXPORT FAILED: usable=False; no completed result claimed.", exception);
                return;
            }
            finally { updates = draws = null; before = null; }
            Mod.Logger.Info($"QA PERFORMANCE COMPLETE: reason={reason}; usable={usable}; updateSamples={updateCount}; drawIntervals={drawCount}. Measurements, not a pass verdict.");
        }

        private static object Scope() => new {
            world = Main.ActiveWorldFileData?.Name, widthTiles = Main.maxTilesX, heightTiles = Main.maxTilesY,
            screenWidth = Main.screenWidth, screenHeight = Main.screenHeight,
            zoom = new { x = Main.GameViewMatrix.Zoom.X, y = Main.GameViewMatrix.Zoom.Y },
            camera = new { x = Main.screenPosition.X, y = Main.screenPosition.Y },
            player = new { x = Main.LocalPlayer.position.X, y = Main.LocalPlayer.position.Y },
            Main.dayTime, Main.raining, Main.gamePaused,
            processId = Environment.ProcessId, is64Bit = Environment.Is64BitProcess,
            runtime = Environment.Version.ToString(),
            mods = ModLoader.Mods.Select(m => new { m.Name, version = m.Version.ToString() }).ToArray()
        };

        private static object Memory()
        {
            using Process process = Process.GetCurrentProcess();
            process.Refresh();
            var gc = GC.GetGCMemoryInfo();
            return new {
                processPrivateBytes = process.PrivateMemorySize64, processWorkingSetBytes = process.WorkingSet64,
                processPeakWorkingSetBytes = process.PeakWorkingSet64, managedLiveEstimateBytes = GC.GetTotalMemory(false),
                gcHeapBytes = gc.HeapSizeBytes, gcCommittedBytes = gc.TotalCommittedBytes, gcFragmentedBytes = gc.FragmentedBytes,
                gc0 = GC.CollectionCount(0), gc1 = GC.CollectionCount(1), gc2 = GC.CollectionCount(2)
            };
        }

        private object Textures()
        {
            // GetLoadedAssets also returns pending assets in this installed
            // ReLogic version. Never access Value until IsLoaded is true.
            var ids = new Dictionary<Texture2D, int>(ReferenceEqualityComparer.Instance);
            var rows = new List<object>();
            long uniqueColorBytes = 0, keyColorBytes = 0;
            int unsupportedFormats = 0;
            foreach (var value in Mod.Assets.GetLoadedAssets().OrderBy(a => a.Name)) {
                if (value is not Asset<Texture2D> asset) continue;
                if (!asset.IsLoaded) {
                    rows.Add(new { name = asset.Name, loaded = false, state = asset.State.ToString() }); continue;
                }
                Texture2D texture = asset.Value;
                if (texture == null || texture.IsDisposed) {
                    rows.Add(new { name = asset.Name, loaded = true, disposedOrNull = true }); continue;
                }
                long? payload = null;
                if (texture.Format == SurfaceFormat.Color) {
                    long size = 0;
                    for (int level = 0; level < texture.LevelCount; level++)
                        size += (long)Math.Max(1, texture.Width >> level) * Math.Max(1, texture.Height >> level) * 4;
                    payload = size; keyColorBytes += size;
                } else unsupportedFormats++;
                if (!ids.TryGetValue(texture, out int id)) {
                    id = ids.Count + 1; ids.Add(texture, id); uniqueColorBytes += payload ?? 0;
                }
                rows.Add(new { name = asset.Name, loaded = true, state = asset.State.ToString(), textureId = id,
                    width = texture.Width, height = texture.Height, levels = texture.LevelCount,
                    format = texture.Format.ToString(), colorPayloadBytes = payload, sourceType = asset.Source?.GetType().FullName });
            }
            var walls = new List<object>();
            foreach (int type in new[] { MawTerrainStudies.Wall("soil").Type, MawTerrainStudies.Wall("grass").Type,
                MawPackedPreview.Wall("soil").Type, MawPackedPreview.Wall("grass").Type }) {
                var asset = TextureAssets.Wall[type];
                Texture2D texture = asset != null && asset.IsLoaded ? asset.Value : null;
                walls.Add(new { type, path = asset?.Name, loaded = asset?.IsLoaded ?? false,
                    textureId = texture != null && ids.TryGetValue(texture, out int id) ? id : (int?)null });
            }
            return new { pending = Mod.Assets.PendingAssets, uniqueLoadedTextures = ids.Count, uniqueColorBytes,
                keyColorBytes, unsupportedFormats, rows, soilGrassWallConsumers = walls,
                limitation = "Logical loaded Color texels only. No Request/Wait/GetData/GC. Excludes native paint render targets, other mods and driver allocations; not VRAM residency." };
        }

        private void Write(string kind, object result)
        {
            string directory = Path.Combine(Main.SavePath, "Captures", "ApogeanPerformance");
            Directory.CreateDirectory(directory);
            string name = kind + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N")[..6] + ".json";
            string path = Path.Combine(directory, name);
            string staging = path + ".partial";
            using (var stream = new FileStream(staging, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                JsonSerializer.Serialize(stream, result, new JsonSerializerOptions { WriteIndented = true });
            File.Move(staging, path); // Only closed, complete reports receive the .json name.
            Mod.Logger.Info("QA PERFORMANCE EXPORT: " + path);
        }

        public override void OnWorldUnload() { if (running) Finish("world-unload"); }
        public override void Unload() { running = false; updates = draws = null; before = null; }
    }
}
