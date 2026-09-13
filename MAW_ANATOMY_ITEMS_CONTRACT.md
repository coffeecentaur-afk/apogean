# QA anatomy blocks: pickup and replacement

Two existing candidate materials only: `Anatomy_rib` and `Anatomy_cap`.
Keep their artwork, registered tile names, native frames, mining power/resistance,
collision, slopes and all historical specimens unchanged. No shipping recipes,
economy, biome spread, new drops for hanging fibers or world generation.

Add one clearly labeled QA placeable item per candidate. Native mining returns
exactly one matching item; the item's createTile points back to that exact
candidate. Rib retains pick59; cap retains ordinary pick access. Do not grant
these items to any player as part of the automated trial.

Draw each icon from one16x16 mapped cell of the SAME loaded candidate atlas,
at native block scale. No duplicate atlas/texture or generated art. Inspect both
inventory/world render paths separately before approving appearance. Icon
appearance remains provisional; a mapping check is not visual approval.

Extend the existing owned32x32 rope/material scratch test rather than adding
another world or harness. Observe registered and actual item drops, construct
an unattached Item object with those defaults, replace a tile through its
createTile binding, mine it again, and require another matching drop. These
are native tile/mining APIs, NOT actual mouse pickup or item-use evidence.
Remove only new scratch drop entities and restore exact original56x56 native
storage in finally. Never alter inventory or saved galleries. Retain original
U no-binding reports under schema1; new binding assertions require schema2.

Engine contract: [official ModItem reference](https://docs.tmodloader.net/docs/stable/class_mod_item.html)
and [ModType instancing](https://docs.tmodloader.net/docs/stable/class_mod_type.html).
Parameterized dynamically registered items opt into `CloneNewInstances`; the
immutable material key is shared explicitly. Installed compiler verifies the
protected override. Runtime `new Item().SetDefaults(type)` remains a separate
instancing test, not proven by compiling the override.
