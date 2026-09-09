# Maw entrance and creature reference intake - September 8

Status: design intake, not implemented or visually accepted as game assets.
Tracks Wayfinder #26 and the later entity-design queue. Preserve the accepted
Wastes, arrival pod/divot, existing route reservations and wall-binding repair.

## Source and interpretation boundary

The user supplied a two-page `Daily Notes.pdf`: page 1 has the current game
entrance above their colored cross-section; page 2 contains four creature
reference images. Both complete pages were visually inspected. The PDF has no
extractable text describing creature behavior. Do not invent supplied names,
attack descriptions, progression tiers or ownership of the reference artwork.

Local source SHA256:
`075B9316B3372540B614ECB87C2B9DAF298F01045E4B35DEE8CD695F40F8E947`.
The original PDF and its reference images remain local; this record describes
the design without redistributing other artists' images as Apogean assets.

## Explicit user decisions

- The drawing represents the desired entrance into the Gullet of the Maw.
- Yellow areas with orange squiggles are **acid water**, not solid blocks. The
  user still wants this but accepts that it can be deferred if too difficult.
- Follow-up asks whether a custom liquid can retain all water properties and
  simply cause damage. Do not present damaging real water as impossible: the
  open complexity is independent acid identity during transport/mixing and
  localized rendering. Latest follow-up selects portability for player-built
  enemy traps outside the Maw; the fixed-region suggestion does not satisfy
  that target. The feature may still be deferred if reliable transport is too
  costly. Mixing with other liquids is proposed to neutralize acidity.
- The Maw's exposed teeth hurt on contact, like Dungeon spikes, but should be
  easy to mine. This supersedes the previous blanket rule that static bone is
  always harmless. Ordinary supporting bone/ribs remain separate from teeth.
- The four creature images are directions the user would like to implement;
  they are not completed sprite sheets or agreed AI specifications.

## Reading the entrance sketch

The working interpretation is an open central throat between two irregular
raised banks, with inward-facing teeth lining the descent. A broad digestive
basin lies on the left and a smaller basin on the right, held back from the
throat by raised lips. More teeth project from exposed upper ridges. This is a
different composition from the current thin circular dome around a central
wall-filled column. Do not simply decorate that dome with a few spikes.

The drawing is a shape/composition reference, not a tile-accurate blueprint:
keep a readable, navigable gap between tooth tips, avoid a mandatory acid swim,
and preserve route options using normal ropes, platforms, hooks and mining.
Keep the current full-route and Stomach reservations; no new landmark placement
or whole-world rebuild is authorized by this intake.

Small orange dots may denote amber organs and green strokes may denote fumes;
these meanings are **inferred, unconfirmed**. Do not silently turn them into
extra hazards or separate required mechanics. Pool asymmetry is visible in the
sketch; exact dimensions, hazard density and how much canopy is retained need
the next entrance preview, not guessed numeric worldgen changes.

## Brittle teeth versus structural bone

Confirmed: exposed teeth are static contact hazards and easily mineable.
Proposed tuning, not a user-approved number: ordinary starter pickaxe access,
low mining resistance and discrete damage contacts with ordinary hurt immunity
rather than damage every tick. Surrounding Mawstone retains its existing
Platinum-tier/explosive access. Do not weaken all OssuaryBone to implement teeth.

Author the hazardous tooth as a visibly distinct tip/attached growth, not an
invisible damage rectangle over harmless ribs. The fixture must cover each
orientation, art/collision agreement, gaps between teeth, standing on adjacent
safe bone, removal and save/reload, multiplayer damage, and starter-tool mining.
Drop/replacement rules and exact damage remain open; no new material economy
or automatic regrowth is implied.

The official ModTile contract exposes `MinPick` separately from `MineResist`,
so easy removal need not change surrounding terrain's mining gate. Contact
damage is a separate implementation/fixture requirement; merely marking a tile
dangerous is not proof of damage.
[Official ModTile reference](https://docs.tmodloader.net/docs/stable/class_mod_tile.html).

## Acid-water boundary

### Latest follow-up: portable trap fluid, optional until proved

Wayfinder #27 owns this optional feasibility branch; #26 remains the entrance.

The user wants to collect/move acid out of the Maw and use it to hurt enemies
in base traps. Leaving the biome must not neutralize unmixed acid. This replaces
the earlier proposed fixed-basin policy, not the prohibition on fake acid tiles.
Adding damage to players alone would not fulfill the trap use case.

User-proposed simplification: contact/mixing with ordinary water or other
liquids removes acidity and takes the other liquid's properties. Record this
as the desired simplification, not proof it is a built-in feature. Neutralization
does not remove the need to track acid during pure flow, bucket transfer, pumps,
save/load and multiplayer. A stationary coordinate flag is not portable acid.

Confirmed by the user's follow-up agreement: **remove acidity, then let native
liquid reactions happen**, rather than literally converting acid into the
other liquid. For example, neutralized water meeting lava can produce the
usual solid product instead of becoming more lava. This settles the desired
mixing outcome, not transfer coverage or a completed implementation.

Also uncontracted: how much liquid a mixing event neutralizes. Do not silently
make one drop erase an entire connected lake, nor promise quantity-limited
neutralization without a real amount/transfer model. A one-drop test must reveal
propagation before this rule is chosen. No concentration simulation, dilution
ratios, new chemical products or renewable acid generator is approved yet.

Enemy-trap balance proposals: one non-stacking environmental damage source,
normal hostile-enemy proof first, then explicit boss/Maw-native/friendly-NPC
rules. Exact damage, immunities, kill credit/loot, collection recipe and unlock
remain open. Player contact remains hazardous; PvP and friendly-NPC harm are
not granted by a request for enemy traps. Carrying acid does not imply Maw
biome spread or automatic terrain corrosion.

Recommended future feasibility fixture (not implemented): one acid source,
one empty receiving basin outside the Maw, one neighboring ordinary-water
control, and one normal hostile target. Prove controlled drain/flow, collection
and replacement, pumps, partial mixing, neutralization visual/harm agreement,
save/reload and a second client. No new liquid ID, global liquid overhaul or
dependency is installed from this design discussion. Preserve the entrance
terrain work; optional acid must not hold the whole Maw slice hostage.

Keep real liquid behavior as the desired result. The old `MawAcidPool` tile
prototype is not an acceptable shortcut. Reserve shaped basins in the entrance
design independently from whether their liquid prototype is ready. No acid
progression gate, new pressure system, global unsafe water or invented fifth
liquid ID. Portable acid must remain distinguishable from unrelated water;
do not silently poison every player-built pool in the Maw. The earlier
basin-bound alternative is insufficient for the new trap requirement.

See `RESEARCH_MAW_BASIN_REVISIT_2026-09-08.md` for the bounded recheck. Any
real-water prototype needs visible boundaries, actual liquid-height overlap,
flow/drain/pump/bucket cases, biome overlap, native capture, save/reload and
multiplayer proof before generation uses it. If parked, report acid as pending;
do not call ordinary water acid or restore fake acid blocks.

## Four creature directions - proposals, not settled designs

Names below are descriptive working labels, not approved content names.

| Reference | Visual idea to carry forward | Proposed distinct gameplay role |
| --- | --- | --- |
| Upper left | Low armored reptilian shape, long tail, exaggerated hooked forelimb | **Hook-limbed prowler:** crouches visibly, makes a committed short pounce, then has a recovery window. An ordinary predator candidate; does not automatically replace the optional Alpha boss. |
| Upper right | Heavy biped, tendril-framed mouth, strongly asymmetric gripping arm | **Tether brute:** a telegraphed, breakable short tether creates a positioning problem. Keep pulling bounded and escapable, especially near acid/teeth. Recommend a rarer deeper/later threat, not a crowd of entrance stun-lockers. |
| Lower left | Recognizable humanoid overtaken by large segmented amber growths | **Larval host:** visibly swollen sacs shed a small capped brood. Distinct infected-person read; kill/interrupt the carrier to control adds. Best candidate for the first modest mob prototype after the entrance fixture. |
| Lower right | Low many-legged creature with large open funnels on its back | **Vent-backed crawler:** plants its feet, opens its vents, then releases a spaced arcing discharge. Moves again after firing; no endless cloud carpet. Exact spit/larva/spore payload remains a design choice. |

The user has not assigned these references to pre-Hardmode, Hardmode or the
Deep Maw, nor confirmed ordinary-enemy versus elite/boss roles. The roles above
are agent recommendations only. Do not introduce four new spawn entries now.

Use the references for silhouette/function, not a direct reproduction of their
specific anatomy, colors and detail. Original Apogean designs use charcoal,
ochre/amber connective tissue and restrained pale bone. Do not import the
reference art or reduce a detailed concept to a noisy tiny sprite sheet.

Before each new enemy family: choose two same-tier vanilla size controls,
define canvas/hitbox/baseline/frame intent, show one original native-scale key
frame beside the character, then animate and test one behavior. Not even the
smallest combatant should become an unreadable speck; exact dimensions remain
uncontracted until the creature's tier and role are chosen.

## Next bounded work

1. Preserve the native wall repair; verify the structural bone atlas/merges.
2. Show an original entrance layout following this sketch, including safe
   route, mineable teeth, basin retaining lips and character scale.
3. After review, prove one tooth/hazard fixture and the changed mouth geometry
   in disposable QA. No automatic rewrite of existing player worlds.
4. Investigate real-water basins separately; integrate only after their own
   proof. Finish this slice's own Maw scenery/amber baseline.
5. Then contract and prototype one selected creature, not the whole roster.

The next scale proposal and its finite geometry checks are recorded in
`Art/Candidates/MawEntrance-v1/README.md`. Its dimensions and eight tooth
clusters are proposals, not approved world-generation parameters. The bone
baseline is now explicitly red for missing exterior edge silhouettes.

This update creates no runtime changes and advances no native evidence state.
