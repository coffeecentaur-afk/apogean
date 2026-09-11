using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // A geometry oracle, not an atlas-to-itself comparison: fully enclosed
    // solid cells must not show sky. Uses only the existing empty 5x5 QA pad.
    internal static class MawMaterialJoinChecks
    {
        private static readonly (string A, string B)[] Pairs = {
            ("bone", "fibers"), ("bone", "stone"), ("fibers", "stone"),
            ("bone", "membrane"), ("fibers", "membrane"), ("membrane", "stone"),
            ("amber", "stone")
        };

        internal static void Run(Point p, Rectangle savedScene, log4net.ILog log)
        {
            if (!MawPackedPreview.Enabled || Main.netMode != NetmodeID.SinglePlayer ||
                Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || Main.LocalPlayer.name != "gg")
                throw new InvalidOperationException("Mixed join probe requires packed gg/V3/SP.");
            Rectangle area = new(p.X - 2, p.Y - 2, 5, 5);
            for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
                Tile t = Main.tile[x, y];
                if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount != 0 || t.HasActuator ||
                    t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire || t.TileColor != PaintID.None || t.WallColor != PaintID.None)
                    throw new InvalidOperationException("Mixed join probe pad occupied; nothing cleared.");
            }
            var cache = new Dictionary<Texture2D, Color[]>();
            int Holes(int x, int y) {
                Tile t = Main.tile[x, y];
                if (!t.HasTile) return 0; // Air has no base draw to audit.
                Main.instance.LoadTiles(t.TileType);
                Texture2D texture = TextureAssets.Tile[t.TileType].Value;
                if (!cache.TryGetValue(texture, out Color[] data)) {
                    data = new Color[texture.Width * texture.Height]; texture.GetData(data); cache.Add(texture, data);
                }
                short fx = t.TileFrameX, fy = t.TileFrameY; int w = 16, h = 16, offset = 0;
                TileLoader.SetDrawPositions(x, y, ref w, ref offset, ref h, ref fx, ref fy);
                int holes = 0;
                for (int py = 0; py < 16; py++) for (int px = 0; px < 16; px++)
                    if (data[(fy + py) * texture.Width + fx + px].A == 0) holes++;
                return holes;
            }
            bool Solid(int x, int y) {
                Tile t = Main.tile[x, y];
                return t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType] &&
                    t.Slope == SlopeType.Solid && !t.IsHalfBlock && !t.IsActuated;
            }
            bool Family(int type) {
                foreach (var pair in Pairs) if (type == MawPackedPreview.TileType(pair.A) || type == MawPackedPreview.TileType(pair.B)) return true;
                return false;
            }
            // Inspect actual saved frames before any temporary rules/framing.
            int observed = 0, bad = 0, pixels = 0;
            for (int x = savedScene.Left + 1; x < savedScene.Right - 1; x++)
                for (int y = savedScene.Top + 1; y < savedScene.Bottom - 1; y++) {
                    Tile t = Main.tile[x, y];
                    if (!Family(t.TileType)) continue;
                    bool enclosed = true;
                    for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++)
                        enclosed &= Solid(x + dx, y + dy) && Family(Main.tile[x + dx, y + dy].TileType);
                    if (!enclosed) continue;
                    observed++; int holes = Holes(x, y); pixels += holes;
                    if (holes == 0) continue;
                    if (bad++ < 24) log.Info($"MAW JOIN SAVED HOLE: {x},{y}; {TileLoader.GetTile(t.TileType)?.Name}; frame={t.TileFrameX},{t.TileFrameY}; missing={holes}/256; neighbors L/R/U/D={Main.tile[x-1,y].TileType}/{Main.tile[x+1,y].TileType}/{Main.tile[x,y-1].TileType}/{Main.tile[x,y+1].TileType}");
                }
            log.Info($"MAW JOIN SAVED RESULT: {bad}/{observed} fully enclosed cells contain {pixels} transparent base pixels; actual saved frames; no scene mutation.");

            void Clear() { for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) Main.tile[x, y].ClearEverything(); }
            int Sample(int a, int b, int shape) {
                Clear();
                for (int dx = -2; dx <= 2; dx++) for (int dy = -2; dy <= 2; dy++) {
                    bool first = shape switch {
                        0 => dx <= 0, 1 => dy <= 0,
                        2 => dx <= 0 && dy <= 0, 3 => dx >= 0 && dy <= 0,
                        4 => dx >= 0 && dy >= 0, _ => dx <= 0 && dy >= 0
                    };
                    Tile t = Main.tile[p.X + dx, p.Y + dy]; t.HasTile = true; t.TileType = (ushort)(first ? a : b);
                }
                WorldGen.RangeFrame(area.Left, area.Top, area.Right, area.Bottom);
                int holes = 0;
                for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++) holes += Holes(p.X + dx, p.Y + dy);
                return holes;
            }
            int checks = 0;
            try {
                int soil = MawPackedPreview.TileType("soil"), stone = MawPackedPreview.TileType("stone");
                // Copy the actual screenshot neighborhood, not an invented corner.
                Point source = new(savedScene.X + 52, savedScene.Y + 20);
                for (int dy = -2; dy <= 2; dy++) {
                    string row = "";
                    for (int dx = -2; dx <= 2; dx++) {
                        Tile t = Main.tile[source.X + dx, source.Y + dy];
                        row += $" [{dx},{dy}:{TileLoader.GetTile(t.TileType)?.Name};{t.HasTile};frame={t.TileFrameX},{t.TileFrameY};holes={Holes(source.X+dx,source.Y+dy)}]";
                    }
                    log.Info("MAW JOIN EXACT SOURCE:" + row);
                }
                bool soilStone = Main.tileMerge[soil][stone], stoneSoil = Main.tileMerge[stone][soil];
                try {
                    foreach (string trial in new[] { "original", "soil-pair", "restored" }) {
                        Main.tileMerge[soil][stone] = trial == "soil-pair" || soilStone;
                        Main.tileMerge[stone][soil] = trial == "soil-pair" || stoneSoil;
                        for (int shape = 0; shape < 6; shape++) for (int order = 0; order < 2; order++) {
                            int native = Sample(order == 0 ? TileID.Dirt : TileID.Stone, order == 0 ? TileID.Stone : TileID.Dirt, shape);
                            int maw = Sample(order == 0 ? soil : stone, order == 0 ? stone : soil, shape);
                            log.Info($"MAW JOIN SOIL CORNER: trial={trial}; shape={shape}; order={order}; native={native}; maw={maw}; extra={maw-native}; native framing, no RNG override.");
                        }
                        for (int control = 0; control < 2; control++) {
                            Clear();
                            for (int dx = -2; dx <= 2; dx++) for (int dy = -2; dy <= 2; dy++) {
                                Tile original = Main.tile[source.X + dx, source.Y + dy];
                                Tile target = Main.tile[p.X + dx, p.Y + dy];
                                if (!original.HasTile) continue;
                                int type = original.TileType;
                                if (control == 0) type = type == soil ? TileID.Dirt : type == stone ? TileID.Stone : type;
                                target.HasTile = true; target.TileType = (ushort)type;
                                target.Slope = original.Slope; target.IsHalfBlock = original.IsHalfBlock;
                            }
                            WorldGen.RangeFrame(area.Left, area.Top, area.Right, area.Bottom);
                            int holes = 0;
                            for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++) holes += Holes(p.X+dx,p.Y+dy);
                            log.Info($"MAW JOIN EXACT COPY: trial={trial}; native={control==0}; holes={holes}; center={Main.tile[p].TileFrameX},{Main.tile[p].TileFrameY}");
                        }
                    }
                } finally { Main.tileMerge[soil][stone] = soilStone; Main.tileMerge[stone][soil] = stoneSoil; }
                foreach (var pair in Pairs) {
                    int a = MawPackedPreview.TileType(pair.A), b = MawPackedPreview.TileType(pair.B);
                    bool ab = Main.tileMerge[a][b], ba = Main.tileMerge[b][a];
                    bool ca = TileID.Sets.ChecksForMerge[a], cb = TileID.Sets.ChecksForMerge[b];
                    try {
                        foreach (string trial in new[] { "original", "checks-only", "pair-only", "both", "restored" }) {
                            Main.tileMerge[a][b] = trial is "pair-only" or "both" ? true : ab;
                            Main.tileMerge[b][a] = trial is "pair-only" or "both" ? true : ba;
                            TileID.Sets.ChecksForMerge[a] = trial is "checks-only" or "both" ? true : ca;
                            TileID.Sets.ChecksForMerge[b] = trial is "checks-only" or "both" ? true : cb;
                            int holes = 0, failures = 0, nativeHoles = 0;
                            for (int shape = 0; shape < 6; shape++) for (int order = 0; order < 2; order++) {
                                int control = Sample(TileID.Stone, TileID.Stone, shape);
                                int actual = Sample(order == 0 ? a : b, order == 0 ? b : a, shape);
                                nativeHoles += control; holes += actual; if (actual > control) failures++;
                            }
                            checks += 12;
                            log.Info($"MAW JOIN TRIAL: {pair.A}/{pair.B}; trial={trial}; excessCases={failures}/12; holes={holes}; nativeHoles={nativeHoles}; originalRules={ab}/{ba}/{ca}/{cb}");
                            if (trial is "original" or "restored" && failures != 0)
                                throw new InvalidOperationException($"Mixed join regression: {pair.A}/{pair.B} has {failures} broken cases.");
                        }
                        // Independent red control: remove the repaired relation,
                        // not the rendering data or saved fixture checkpoint.
                        if (pair.A == "bone" && pair.B is "fibers" or "membrane") {
                            Main.tileMerge[a][b] = Main.tileMerge[b][a] = false;
                            int rejected = 0;
                            for (int shape = 0; shape < 6; shape++) for (int order = 0; order < 2; order++)
                                if (Sample(order == 0 ? a : b, order == 0 ? b : a, shape) > 0) rejected++;
                            if (rejected != 12) throw new InvalidOperationException("Missing bone relation was not detected in all twelve controls.");
                            log.Info($"MAW JOIN NEGATIVE PASS: {pair.A}/{pair.B}; {rejected}/12 missing-rule defects detected.");
                        }
                    } finally {
                        Main.tileMerge[a][b] = ab; Main.tileMerge[b][a] = ba;
                        TileID.Sets.ChecksForMerge[a] = ca; TileID.Sets.ChecksForMerge[b] = cb;
                    }
                }
                if (bad != 0) throw new InvalidOperationException("Saved tissue-only interior still contains holes; view scene first to obtain native frames.");
                log.Info($"MAW JOIN REGRESSION PASS: {checks} comparisons; two missing-rule negative controls; saved tissue-only interiors closed. Not all materials/overlays or visual acceptance.");
            } finally { Clear(); cache.Clear(); }
        }
    }
}
