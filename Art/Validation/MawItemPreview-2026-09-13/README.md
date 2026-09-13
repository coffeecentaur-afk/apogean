# AA — passive native inventory icon comparison

03:17–03:18UTC September13, gg / Apogee Native Visual V3, single player.
Installed QA package SHA256:
`2450BDD240BB338F463578C0562652A25E8D2FB9F4512A3B82898851643E6343`.
Isolated build263a8177b3e042e1a895634166f4d157:0warnings/0errors;
466+7+1 accepted/candidate art pins unchanged. No new textures or worldgen.

`aa-native-icons.jpg` is an unedited Computer Use capture from the real game,
not an offline montage or CaptureManager photo. Retained in its original JPEG
encoding; an initial PNG-only save refused before writing and was corrected
to preserve the provided format. UI scale1.15; comparison slot scale1.

The cortical rib and full-fiber icons look centered and comparable in size
to vanilla stone/dirt. Both use the16x16 mapped candidate tile frame, visibly
distinct from the stone fallback. This is a technical native-size/readability
inspection, not the user's final art approval. Nighttime lightning and Solar
retaliation affect the scene behind the opaque panel, not its white UI tint.

Two native runs:1953 and651 drawn frames. First-frame callbacks report centers
(126,256)/(318,256), scale1, source(0,180,16,16)/(0,270,16,16), fallback16x16,
origin(8,8). Runtime restored the prior0.75 inventory scale in finally and
checked a small set of UI interaction fields on every successful draw.
Those checks do not prove arbitrary mod/global purity. Context31 is an
inventory-hook path despite its `InWorld` name; **no world-item draw claimed**.

First run stops on explicit stop; second releases before native Save&Quit,
both represented by the shared dispatcher reason `new-command`. No preview
remains on the menu. The90-second timeout/context-loss branches were not
separately exercised live. No inventory items were granted by the preview;
ambient enemies and ordinary Solar retaliation/pickups still occurred at
spawn, so this is not a whole-player-before/after equality test.

`aa-save-tags.json`: all16 existing canonical QA records unchanged after
normal save03:18:19–20UTC. Contour6F7998FF and shallow24163E65 unchanged.
`aa-native-log.json`:8 scoped native records, no relevant caught draw stack.
The independent old grove reload mismatch87B951FE vsC6A92EB1 remains RED;
its pre-existing failure was not repaired/rebaselined or counted as a clean log.

36 actual-draw-body decisions tested with API doubles, including exceptions
at each slot and a wrong-restoration negative.75 shared-exit checks/7 omitted
releases;16 malformed-report controls and both native first frames replayed
independently. Neither synthetic checks nor the screenshot prove native mouse
pickup, actual item-use, ground-item lighting/physics, multiplayer or production.
