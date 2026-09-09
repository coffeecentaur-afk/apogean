# Maw fang v1 — prototype, not approved production art

Original AI study: `source.png`, generated using `prompt.txt` on September9.
SHA256 `3EE52DF9AA906B98B9F5B067B858C6D1B71E1B9292E954B795EDC6097150A954`.
It is1024x1536 and has a rendered dark matte. It did NOT satisfy the requested
native dimensions or transparency. Never install it directly or call it an atlas.

`Tools/New-MawFangCandidate.ps1` explicitly resamples and recontours this source
into the authored32x48 physical wedge, fits an8-color palette, and makes a binary
frame mask. This is a lossy art adaptation, not an unchanged cutout. The established
`Export-MaskedBackground.ps1` then exports the fitted master through that mask
without further color changes; padding has alpha0/RGB0. No checkerboard removal
heuristic and no new image-processing dependency.

`Native-v1/MawFangTile.png`:54x216,4 orientations, five occupied16px cells per
orientation at18px pitch. SHA256
`52DFA889B6CFADED43D9997E870D2EBCA0BE991188E29AF29616CE226DBC3D68`.
`preview.png` is a labelled OFFLINE assembly with a42px player-height ruler.
Its generated reference shape is straighter and broader than a curved animal
fang; ask for visual approval after the native fixture, not merely because the
mechanics pass. Rotated shading is prototype shading, not four final lighting studies.

The shared pure geometry is `Common/Geometry/MawFangShape.cs`. Native collision
uses actual solid/sloped Terraria cells; empty parts of the3x3 envelope are air.
Native contact damage sets are rectangular enough to require a separate
shape-aware Hurt predicate. Standard Player.Hurt handles damage and immunity.
No native collision detour, TileEntity, extra library, enemy-damage claim, or
worldgen integration. Mining any segment removes the full object. See contract.md.

## Validation and observed failures

- The contract test initially failed on missing implementation (after repairing
  a PowerShell parser typo; the parser error was not a meaningful red test).
- First assembled atlas failed at18,126: the180/270-degree slope mapping was
  swapped. Correct mapping for clockwise rotations of slope1 is1,3,4,2.
  The actual independently rotated pixel atlas caught this error before installation.
- Passing static test:20 frame roundtrips, invalid-frame rejection,1280 independent
  pixel cases, exact atlas occupancy, hard alpha, empty padding,8 colors.
- The real validator rejects five corrupt atlases: dimensions, opaque padding,
  soft alpha, missing tip, and white RGB hidden in transparent pixels.
- Native evidence lives in `Art/Validation/MawFang-2026-09-09/README.md`.

Installed tML1.4.4.9+2026.07.3.0 reference: Spikes48,234x90,solid,not
frame-important,TouchDamageImmediate60. Export occurred through Main.Assets with
the user's existing resource packs enabled; it is an installed-runtime reference,
not a claim of pristine vanilla pixels. The reference PNG/decompiled game code
are kept outside the repository and are not redistributed.

Primary API references: [ModTile](https://docs.tmodloader.net/docs/stable/class_mod_tile.html)
and [TileLoader source](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/ModLoader/TileLoader.cs).
The exact native slope/Hurt behavior was checked against the installed assembly.
