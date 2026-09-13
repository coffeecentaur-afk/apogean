# Y — candidate block pickup and replacement

Native gg / Apogee Native Visual V3 / single player, September13 02:38UTC.
Pinned QA package SHA256:
`6AD8D2A63ECAC6E129B7BDBE82EE2834FE258E6BD5E31F8D461C2DB4B364614F`.
Build: zero warnings/errors;466+7+1 accepted/candidate asset pins unchanged.

Existing `Anatomy_rib` and `Anatomy_cap` now register clearly named QA-only
placeable items. No artwork, terrain framing, collision or mining-value change.
No recipes, inventory grant/refund, world-generation promotion or economy.

## Actual results

Both unmodified raw `y-binding-*.json` reports pass15 cases/687 checks:

- Cortical rib:30 native Pick58 calls leave it intact;5 Pick59 calls break it.
  First drop is item5546 x1; Item.SetDefaults binds tile700. API replacement
  succeeds,5 more Pick59 calls remove it, and item5546 x1 drops again.
- Full fiber cap:4 Pick35 calls break it. First drop is item5547 x1; native
  defaults bind tile701. Replacement and4-call repeat mining return5547 x1.
- Full names identify the materials; numeric IDs are specific to this package.
- Both scratch56x56 native-storage digests restore exactly. Nine existing
  historical locations are unchanged; nine absent locations remain unverified.
- Existing vanilla/rib/cap cardinal rope support/mining checks also pass.

Independent replay: `Tools/Test-MawRopeEvidence.ps1 -Path <report>
-RequireAnatomyItems`. Eighteen original plus ten new deliberately corrupt
report controls reject optimistic results. Older U schema1 evidence is retained
unchanged and correctly fails the new mandatory-binding requirement.

`y-save-tags.json`: normal save02:44:50UTC preserves all16 existing canonical
system records. This does not assert equality of the entire evolving world.
`y-native-log.json`: four scoped native records, no relevant caught draw stack.
The older automatic grove mismatch87B951... versus C6A92... still occurs before
these trials and was neither repaired nor rebaselined. The scoped export is
not a blanket clean-client verdict.

## Remaining limits

These are native tile/mining APIs and real defaults, not player item-use or
mouse-pickup proof. No item was granted or retained from scratch. Each icon
uses one16x16 mapped cell of the same atlas; mapping bounds do not approve
appearance. Native item-browser text search stayed blank after one refocused
retry, so inventory and dropped-item presentation remain UNINSPECTED.
No multiplayer, save migration, whole-biome or final art approval.
