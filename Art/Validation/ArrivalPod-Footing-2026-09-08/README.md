# Arrival pod footing — Native-v3, September 8

User reports that the accepted pod floats and its curved underside does not sit flush on flat terrain. This is a bounded footing correction, not acceptance of the final pod or a new-world spawn integration.

## What changed

- Native-v2 contacted the bottom row across only 18 pixels; the old test required 12. A contact-count check did not prove a flat supporting base.
- Native-v3 retains all 7,040 pixels above row 88 exactly. Only the bottom eight rows are rebuilt as crushed, flat metal. It has 64 pixels of continuous bottom contact, 19 colors and strict 2x2 clusters on the same 80x96 canvas; its 90x108 tile atlas reconstructs exactly.
- `DrawYOffset` changes from 0 to 2. Native WastesSoil top frames contain two-pixel recesses; the inset seats the artwork into that edge. Tile anchor, placement footprint, collision and spawn are not moved. The fixture assertion now checks the actual registered offset of 2.
- A new flat-footing test rejects the previous curved underside and a deliberately broken 2x2 contact patch. Deterministic export and content/overwrite refusal are tested through the actual command line, not only helper functions.

Source, edit provenance, exact hashes and replay instructions: [Native-v3 candidate](../../Candidates/ArrivalPod-v1/Native-v3/README.md).

## Build and preservation

The isolated build completed with zero warnings and zero errors. New package SHA256: `7F8DF90A2C012B26D4FFDAFE0BCFB2AB7C59AD3B13DCA51443BA9B8D5C9DB18B`.

Only the two pod PNGs differ from the preceding isolated mirror; 408 surrounding Content PNGs match exactly. The repository's Content PNGs remain untouched. Runtime changes are the two-pixel drawing inset and the matching diagnostic assertion. No background, tree, biome or worldgen redesign.

Before installation the disposable world was saved and the game exited normally. Previous package, QA world and gg character were backed up to `C:/Users/max_h/AppData/Local/Temp/ArrivalPodFootingBackup-e72620a22680427892ed713fb1a16c65`. Build mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/f9152dd8706c4574b6afeddb2526ec45/apogean`.

## Native proof

Only gg / Apogee Native Visual V3 was opened. Existing platform at X4476/Y540, 64x24, retained: three pods, blue-painted middle pod, fourth slot empty. No fixture rebuild and no ordinary world opened. This platform is not the planned divot.

- Fresh reload at 13:08:08 passed the unchanged pod checkpoint `273B29B0F2AF8EAA6628C85793FDA1587E5087B1FA5F6D6B6028F523EBF8BB8B`.
- All 58 programmatic native mechanical checks were rerun at 13:08:42 and passed, including registration, placement, non-solid passage, painting, mining/single drops, support loss, re-placement and unchanged spawn.
- Daylight, player overlap and night views were inspected using actual game screenshots. The main base rests against the soil. Upper pod/hatch design is unchanged; no dirt patch was added to conceal a hovering sprite.
- Returned to daylight at 13:15:16 and left the player beside the pod for user inspection.

The reload precedes the mechanical matrix. The current log validator passes without `-RequireReload`; do not label this as a second post-matrix reload.

| Capture | Meaning |
| --- | --- |
| [day.png](day.png) | Native daylight view after loading v3 |
| [day-detail.png](day-detail.png) | Exact 460x190 crop at (1100,615) of day.png; no enlargement or repaint |
| [inside.png](inside.png) | Player drawn in front of the non-solid pod |
| [night.png](night.png) | Native nighttime view; player equipment adds light |
| [ready.png](ready.png) | Final daylight view left ready for inspection |

These are captured game pixels, not an offline mockup. Weather and equipment illumination vary; this is not a matched-lighting before/after study.

## Checks and limits

Passing this turn: Test-ArrivalPodNative with RequireFlatFooting, Test-ArrivalPodFooting, Test-ArrivalPodNativePipeline, isolated build, all 58 native checks, Test-ArrivalPodLive on the selected log, and Status content gate. The historical native pipeline retains both previous variant replays, eleven original defect cases, three coarse-grid controls and nineteen unchanged pins.

[native-pod.log](native-pod.log) is an exact selected excerpt retaining the pod records and the unrelated grove failure, not a sanitized claim that the whole session is error-free. Grove reload still fails: expected `87B951FEB5C9FA9056E12F69F6862C946A7624328210500971B319E56B9929E4`, actual `43FE618C0BD207DA7FCC35AD53379B734E67F990B3DB7041A0C6E3D5DAE3CE3E`. No rebuild or rebaseline was performed. Pod commands preserve the current within-session grove snapshot; that does not establish cross-reload grove correctness. The cause of that unrelated mismatch was not diagnosed here.

User review of the revised footing, manual inventory/pickup/re-placement visual checks, actuator/sloped-anchor cases and multiplayer remain pending. The broad Structure gate was not rerun; its previously recorded two legacy expectation failures remain open. No automatic production-asset promotion, sanctuary/divot generation, regular-world retrofit or relay implementation.
