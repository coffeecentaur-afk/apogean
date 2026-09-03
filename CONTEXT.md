# Apogee Wastes

This glossary defines the setting-specific language shared by world generation, progression, art, dialogue, and documentation. Player-facing systems use these terms consistently.

## Implementation discipline

**Renderer-validated asset**:
A tile, wall, tree, furniture object, or background that has passed its static atlas contract, a clean build, and inspection in a disposable in-game capture at useful scale. A PNG that looks correct outside Terraria is not complete until its framing, seams, anchors, and gameplay rendering pass this gate.

**Composite tree renderer**:
A wide visual silhouette anchored to Terraria's ordinary `TileID.Trees` gameplay column. Transparent native-size atlases suppress incompatible vanilla-width art, while a tile-render hook draws the approved composite in both normal play and capture-camera output. Chopping, shaking, regrowth, networking, and drops remain native.

## World ecology

**The Wastes**:
The neutral, non-spreading dead-world biome that replaces the original forest across the starting surface. It is damaged land, not Broodmass territory and not world evil.
_Avoid_: Dead biome, Engraft, Maw terrain

**The Maw**:
The hostile, spreading biome occupied and reshaped by the Broodmass. Maw contamination is tracked separately from the Corruption, Crimson, Hallow, and neutral Wastes.
_Avoid_: Engraft, Canker

**Broodmass**:
The distributed bioengineered organism and its connected ecology within the Maw. Individual mutants may be infected by or grown from the Broodmass without being the whole organism.
_Avoid_: The Maw when referring to the organism itself

**Maw Rupture**:
The major continuous planetary scar connecting the surface, underground, caverns, and an infected region of the Underworld.
_Avoid_: Engraft scar, generic Maw biome

**Maw Outgrowth**:
A small secondary patch of Maw contamination outside the primary Rupture. An Outgrowth may contain enemies and a Maw Node, but it is not a second Gullet and never owns required Brood Nests.
_Avoid_: Secondary Maw, Brood Nest, minor Rupture

**Maw Node**:
A feeding and amplification organ extended by the Broodmass into a Maw region. It accelerates local spread and hostility but does not sustain the Maw's intrinsic life or passive growth.
_Avoid_: Boss summon, biome heart

**Ossamber**:
Amber-yellow mineralized Broodmass tissue condensed when a cauterized Maw Node retracts. Raw Ossamber supports Warden-era equipment; MATRIARCH-7A-1 Mutagen Cells stabilize selected recipes at the final pre-Hardmode power ceiling.
_Avoid_: Generic metal ore, faction-specific Hardmode ore

**Brood Nest**:
A reproductive structure whose destruction provokes the early Nest Warden. It does not control terrain spread.
_Avoid_: Brood Cyst, Maw Node

**Frayed growth**:
The approachable outer Maw terrain that early tools can cut or mine. It communicates infestation without serving as a progression gate.
_Avoid_: Mawstone, core membrane

**Mawstone**:
Hardened structural Maw terrain and bone that require approximately Platinum-tier pickaxe power or ordinary explosives.
_Avoid_: Frayed growth, progression membrane

**Core membrane**:
An explicit biological progression gate that cannot be removed with ordinary pickaxes or explosives until its associated encounter unlocks it.
_Avoid_: Mawstone, arbitrary unbreakable terrain

**Digestive basin**:
A reserved depression in an authored Maw chamber for an optional later prototype of localized amber digestive fluid. Environmental Alpha never fills it with fake solid acid tiles, and neither the basin nor any future liquid is required for progression.
_Avoid_: Acid block, universal Maw water, pressure system

**The Stomach**:
The broad pre-Hardmode Matriarch arena suspended immediately above the Underworld inside the Burning Root. It ends the Gullet and is not itself an open entrance into Hell.
_Avoid_: Burning Root when referring only to the arena, Matriarch box

**Intestinal descent**:
A narrow, enclosed Maw organ continuing from the sealed floor of the Stomach through the Underworld toward the world floor. Players must deliberately breach its Mawstone wall to enter ordinary Hell terrain.
_Avoid_: Open Hell chute, second Gullet

**Maw dormancy**:
The subdued state after MATRIARCH-7A-1 is defeated, when amber organs dim and biological activity falls to its minimum without erasing Maw terrain or passive growth.
_Avoid_: Purification, destruction of the Maw

**Deep Maw**:
The later post-corporate hive domain where the Broodmass is no longer constrained to recognizable terrestrial forms.
_Avoid_: Maw Rupture

**Restored wilds**:
Vanilla living grass, plants, and trees deliberately recovered from the Wastes by the player. It is a restoration state, not the default condition of a new Apogee world.
_Avoid_: Purity when referring to untouched world generation

## Corporate geography

**Corporate Campus**:
One permanent, faction-specific headquarters landmark generated with the world and reused for contact, contracts, testing, the company war, and later salvage. Arrival changes its occupants and access state rather than creating the structure.
_Avoid_: Temporary faction base, ambassador camp

**Public frontage**:
The exterior approach, reception area, or service post that players may reach before a Campus fully opens. It provides presence and limited salvage without granting access to protected corporate interiors.
_Avoid_: Entire Campus, raid entrance

**Corporate territory**:
The bounded ground or structure claimed by a Campus, beginning at its explicit threshold such as Kessler's perimeter gate. Territory state may affect trespass and conversion resistance without forbidding ordinary building outside its narrow envelope.
_Avoid_: Whole biome, permanent no-build zone

**Orbital omen**:
A temporary spacecraft or signal visible in the upper sky after a corporation's progression prerequisite and before its arrival event. It activates an existing Campus; it does not generate a new headquarters.
_Avoid_: Orbital headquarters, new world-generation pass
