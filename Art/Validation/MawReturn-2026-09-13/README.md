# Shallow return attempts and history-coverage correction

These are bounded native QA observations, not complete Maw traversal or final
gameplay approval. The player/world were Maw QA Plain / Apogee Native Visual V3.
No ordinary world was opened, and no terrain or art was redesigned in this slice.

## S: two identical return attempts

Installed package SHA256:
`01ECF26568225E8F94FB444A399D7C7B953F3BD8CD462615A1F26EAC4F8E62FB`.
The explicit setup moves the20x42 starter player to local(944,1062) on the lower
connector floor. During the sequence only movement/jump inputs are supplied;
native physics, health, inventory, geometry and damage are untouched.

At00:15:38 and00:20:19UTC both runs stopped at the360-update budget. Both record
65 right-input ticks,105 jump-held ticks,123 rising/106 falling ticks,98.0718px
maximum rise, zero tooth contacts, and100→100HP. Neither reached the upper exit.
Both raw JSON files independently pass `Test-MawShallowReturnTrace.ps1 -Path`.
That validator explicitly distinguishes valid evidence from route success.

The unchanged geometry hash is
`24163E65FC677E84DA67BBE7DC7E1535CEBB4096AE3326516418EC0A6213F5A5`.
It remains separate from creation identity
`F3CC4D2B7672EFA22C657E047CD71F1D18418F3442B8BFD3699568E966885FF9`.
Normal save at00:23:04UTC retains the interaction hash and contour6F7998FF.
The real pinned TagIO comparison against the separately verified B copy finds
all16 existing system records unchanged after the native Plain-character save.
This is metadata equality, not all-world tile equality.

No conclusion that the route is impossible follows from six scripted jump
cycles. Rope, mining, building, other inputs/equipment and manual feel remain
untested here. No safety ledge, forced-damage path or named gear gate was added.

## T: honest history coverage and entry regression

Package SHA256:
`994E4BE3BD8137F74DC78B14433E1BE052B16D648DA051D61AAA0B0E2B9D58C9`.
Isolated build succeeded with zero warnings/errors and the same466 pinned base
assets,7 anatomy assets and1 separate cave-study texture. No artwork changed.

The old reporting expression claimed `all17` from the array length, even when
all17 rectangles were empty. `history-report-red.txt` retains that reproduction.
The actual corrected expression/helper passes56 count/zero-area checks.
Native T at00:29:43UTC reports8/17 nonempty historical locations and9 empty slots
`[1,2,3,4,5,6,7,8,9]`. In current Historical() order these are ForestSpray,
ArrivalPod, Bone, Fang, ToothArt, ToothCluster, ClusterOrientation, Material and
MaterialFamily. An empty slot alone cannot distinguish never-created from lost
metadata; the headless audit independently identifies eight already-absent tags.
Do not reconstruct any of them automatically. Raw earlier `all16/all17` reports
remain unchanged but must not be cited as proof of16/17 existing preserved areas.

T's11648-cell pristine and reload checks pass. Its entry regression again lands
in119 updates,46 falling ticks, zero tooth contacts, with18→18 actual HP.
Before the test, native UI showed Plain killed by a Demon Eye at the nighttime
spawn; this was outside the controlled passage and is NOT evidence of passage
damage. The18HP trace is deliberately labeled, not presented as a100HP repeat.
No gear or health was changed to hide this. The trace passes the existing
independent movement validator. Native screenshots were viewed in the tool;
no new art-review capture is claimed or stored here.

T's explicit release and normal save at00:32:48–49UTC retain both known specimen
hashes. The main menu was observed after saving. `T-native-complete.json` keeps
all19 scoped native records; the exporter found no relevant caught draw stack.
This does not certify absent historical scenes or the untouched ordinary worlds.

## Reproduction and scope

Run `Tools/Invoke-ApogeanContentGate.ps1 -Profile MawShallow`, and `-Profile
Persistence`, plus both raw S files through the return validator and T through
`Test-MawShallowMotionTrace.ps1`. Synthetic checks are not native physics proof.
No automatic scene rebuild, immutable-baseline reset, art promotion, modpack/
multiplayer certification, performance guarantee or generator integration.
