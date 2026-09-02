# Native world and tileset validation — V3

Validated 2026-09-01/02 against tModLoader `v2026.7.3.0` and Terraria `1.4.4.9`.

## Package gates

- `Tools/Test-WorldVisualIntegrity.ps1`: PASS
- `Tools/Test-VisualContracts.ps1`: PASS
- `dotnet build --no-restore`: PASS, zero errors; the existing lowercase `apogean` class-name compiler warning remains.
- Dedicated-server content load: PASS with no Apogean warnings or errors after removing invalid placeholder banner registrations.

## Fresh large-world proof

- Name: `Apogee Native Visual V3`
- Input seed: `APOGEE-NATIVE-TILES-V3`
- Numeric seed: `997890696`
- Size: `8400 × 2400`
- Difficulty: Classic
- Evil: Crimson
- Vanilla + Apogean generation time: `30.607 s`
- Save, validation, reload, and clean server shutdown: PASS

Apogean generation log:

```text
World-plan validation passed: schema=2; hash=603D98E9; ruptures=3; landmarks=8
Maw-route validation passed: route=pass depth=2150/2097; reachable=41039;
max-fall=83/120; stomach-clearance=41/30-60; intestine=2385/2370;
plug=85/70; legacy-acid=0; maw-water=0; all-spine-waypoints-reached
```

## Visual acceptance fixture

Run `/apogean gallery` in a disposable single-player validation world. It clears a reported rectangle to the player's right and places labeled rows covering:

- all Wastes tile/wall families;
- all Maw tile/wall families and Ossuary bone;
- Kessler, Helix, and Sentrix block, trim, floor, glass, beam, and wall combinations;
- connected edges, corners, interior framing, half-blocks, and slopes.

The atlases use the official tModLoader ExampleMod framing topology as an opaque/transparent mask. Every visible pixel, palette, material motif, ruin, amber vein, and corporate detail remains Apogean-authored. No Terraria resource pack is required.
