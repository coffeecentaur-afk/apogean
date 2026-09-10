# Harsh Maw materials — implementation checkpoint

The user approved terrain-study.png (SHA256 0993BAB7546E10EBE139418E906BB2DA31B1695070FA3F0CE04C963554E7413B) and requested faithful in-game implementation. The earlier soft direction remains rejected.

## Scope and provenance

- Original opaque source: stone in Native-v1/stone-source.png; ten other material sources in Sources/. Exact prompts are retained beside them. Built-in image generation used the approved board as style reference. Original generated files were retained.
- Grass combines the new soil field with the fiber field through native grass semantic masks. It is not a separately generated finished plant silhouette.
- Packed-v2 contains twelve diagnostic tile sheets and twelve darker wall sheets. They are actual game-loadable candidates, not screenshot overlays.
- No production terrain replacement, ordinary-world edits, generation, acid, mobs, bosses, recipes, ore gates or tooth changes.
- Sand and amber study blocks are explicitly art-only solids. They do NOT prove falling sand, Sandgun behavior or localized emitted light. Fibers and membrane are material prototypes, not new progression content.

## Stone proof

Initial whole-quadrant mirroring looked repetitive in-game and was rejected internally. v2 preserves the full source field and fits only four-pixel outer seam strips.

- Saved fixture: X5316 Y450 W80 H38.
- Fingerprint BE2B4A8B64EC761519339AEB23C7C296235ED40C6F6DD3C11600FC3686974C08.
- v2 package E440428C5B50A415C49438A813938A530627A65FFFA022BCE9DA6BFC60AC086B.
- Native save/reload: 13 checks and 340 actual draw-frame probes. Saved frames unchanged. Power58 mining blocked; power59 removed a temporary tile in five native PickTile calls. Collision, actuation and explosion permission passed. No actual bomb, manual mining, drops or multiplayer claim.
- Day/night inspected, ordinary material non-emissive. Native captures 20260910-020632 and 20260910-020738; rejected-pattern capture 20260910-015443.

## Packed family contract

PackedMaterialCompiler fits separate sources to periodic 128x128 fields with at most sixteen colors. Stone retains its tested twelve-color field. Quantization and seam fitting are disclosed lossy steps, not exact source-pixel extraction.

Native topology comes from installed-game exports in Captures/ApogeanTileLabReferences. Reference RGB is never imported. Identical rendered alpha/semantic masks are deduplicated with two transparent guard pixels. Saved frames remain native; a bounded draw-only lookup chooses a world-coordinate phase.

Grass uses the full 288x1980 reference. Green membership selects fibers; soil and white exporter masks select soil. Live slope-to-soil review is mandatory. Ordinary tiles use 288x270 references. Walls use 468x180 references, 32px overlapping bodies, 36px native stride and a -8px world origin. Wall colors are reduced to 62% to sit behind terrain.

- Packed-v1 is a partial failed export: the initial 400-mask ceiling rejected grass's 405 masks. It was not installed.
- Packed-v2: 24 sheets; 148,945,920 reconstructed native-frame pixel checks pass.
- Actual PackedMaterialMap.cs: 1,326,272 exhaustive frame/phase/bounds/invalid-metadata checks pass.
- Texture cost: 245.79 MiB uncompressed, excluding paint copies. Largest sheet grass1152x7290. This is a QA budget, not shipping-memory/performance certification. Avoid duplicate diagnostic/production textures when promoting.
- First family load caught a derived-type lookup error. Lookup corrected to tModLoader's ModTile/ModWall registries before any world was entered.

## Native gallery and safety

MawMaterialFamilyLab is restricted to gg / Apogee Native Visual V3 / single-player. It searches bounded truly empty air envelopes, refuses existing saved-gallery replacement, fingerprints terrain/walls/wires/liquids and preserves the grove guard. Three rows follow board order. Each material has broad fill, wall strip, four slopes, half-block, paint and actuator samples.

Native gallery completed September9 local / September10 UTC:

- Package2558EE6F3A63FD1BBD2352CA3624C123A584466B523F07A2B028EB46638E2F8B loaded successfully.
- Built once at X5796 Y360 W152 H94. Fingerprint D5059C3066ED8D42F26FE5113AACA2101E0549FA552E91ABAB9A36A60C5A59AF.
- 3,444 native tile draws and1,680 wall frames pass before and after actual save/quit/reopen. Same fixture, no rebuilding or rebaselining.
- All three rows inspected and captured, plus row3 at night and row1 after reopening. Images are unchanged native CaptureManager outputs, not mockups.
- No white grid/exporter halo was observed in these samples. Broad soil/stone fills connect; grass's flat top merges into its soil. This does NOT certify every slope-to-soil permutation: the small slope specimens are homogeneous material blocks, not all mixed-substrate cases.
- Repetition remains visible over broad surfaces, especially bone/fiber/snow. Contextual natural shapes, material interfaces and user appearance review remain before production promotion. The grass edge is not the board's complete hanging-root silhouette.
- Eight compiler controls pass: positive, deleted alpha, white pixel, dirty guard, wrong phase, wrong size, invalid index, damaged outer half of an overlapping wall. These invoke the compiler verifier, not a CLI provenance test.
- All416 Content PNGs from the previous accepted isolated package are byte-identical;25 new diagnostic PNGs were added. No production replacement.
- [Native screenshots and test record](../../Validation/MawMaterials-2026-09-09/README.md).

Known grove mismatch remains RED and untouched:
expected87B951FEB5C9FA9056E12F69F6862C946A7624328210500971B319E56B9929E4;
previous actual5948966A08598CA91FD8CE0478CE1338D2BA4930F2A6D065773F9D9D6EF7DF07;
family-package first entry actualEF09F1F1DF265B5E972E2FB7ECE2F1A0536B2F8C1895B5F427F1A87E76C2CA9B;
after reopen actual9D21A431C3E3FD8742B303DD28E3C994340E90D68F72969AA14F04BC20F2909F.
Within-request grove guards pass, but these changing cross-reload digests are NOT resolved or rebaselined. No claim of a globally clean QA log.

## Next gates

1. Review the native family against the approved board; then one bounded natural-context sample, not whole-world replacement.
2. Test mixed grass/soil corner slopes and material boundaries; address repetition/edge silhouette without regenerating accepted unrelated art.
3. Promote through existing classes only after fixture acceptance; independently test real physics, drops and two-step purification, especially sand/projectiles and grass merges.
4. Keep teeth as separate easily mined hazards. Hanging plant silhouettes and localized amber organs need their own native proof; flat material fields are not finished decorations.

Engine references: [ModWall registration](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/ModWall.cs), [wall draw hook](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/GameContent/Drawing/WallDrawing.cs.patch), installed tModLoader XML for SetDrawPositions and ModBlockType.PreDraw.
