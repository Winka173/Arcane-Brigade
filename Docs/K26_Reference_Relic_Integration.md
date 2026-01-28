# K26 Reference Relic Integration

Status: in progress, not a completed 76-relic release.

## Reference inventory

Read on 2026-10-15 using public MediaWiki wikitext endpoints after the normal page fetch was unavailable:

- [Relics](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics): 76 distinct entries.
- [Companions](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions): 60 distinct names, 91 tier-specific rows, 19 bond passives.
- [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers): 28 entries, including seven later character-themed variants. This does not mean 28 original base protagonists.

The wiki is incomplete: its weapon section is still a stub and page change-history labels do not establish exhaustive 1.5+ coverage. Do not present this inventory as every mechanic in every later version.

## Implemented foundation

`ReferenceRelicCatalog.Create()` defines 76 original-name adaptations with short English copy, categories, tradeoffs, exclusive shop pools and explicit mechanic identifiers. Prices and unit conversions are adapted for this game's armor, speed and luck formulas rather than copying incompatible raw numbers. Original flavor prose and external-game sprites are not imported.

`RunRelics` persists drink/food growth, wave growth, interest, temporary wave curses, locked stat floors, phantom weapon identities and immediate purchase rewards. Wave-start grants are journaled so a shop rebuild or save restore cannot award them again. Buffs belong to this run only.

`RelicCombat` connects coin/chest effects, damage/dodge reactions, stationary builds, temporary pierce/critical bonuses, income poison, periodic health-based pulses, companion attack restrictions and six phantom weapons. Existing basic, melee and ranged weapon systems remain in use.

## Art and shipping status

- 32 reference adaptations have new dedicated icons and are installed in the live catalog.
- Eight additional original defensive relics have dedicated icons and are installed.
- The remaining 44 reference adaptations are defined and smoke-tested but intentionally NOT offered without their art. No recycled ally/weapon icons are substituted.
- Nine larger carrier drawings are imported and connected to the map-transfer presentation and challenge-hand marker. These are higher-resolution individual parts, not native-4K textures.
- Generation of relic sheets 3–5 was interrupted by revoked tool network permission. Prompts are preserved in `K26_Art_Prompts.md`.
- `PreviewExports/K26/Relic_Coverage.tsv` maps every reference name to its adaptation, mechanic and installed status.

Re-running `ArcaneBrigade.Editor.K26ReviewBuilder.Build` imports any completed numbered sheets, replaces only its own reference/defensive entries and does not double-apply the existing-item balance migration.

## Intentional differences and remaining acceptance work

- Normal and special shops use disjoint relic pools, even when a reference relic originally appeared in both.
- Summoned allies from relics respect the per-wave recruitment quota and the solo-character restriction.
- Regeneration and armor values use this game's actual formulas; negative regeneration reduces healing rather than silently becoming damage.
- Void pulse uses current enemy health, avoiding automatic fixed-duration boss kills.
- Six phantom weapons retain a saved weapon roll; shared inheritance currently selects stronger player/ally damage and projectile properties rather than blindly doubling both.
- Some generated icon silhouettes are close to their cell edges. In-game visual acceptance and additional padding/redraw may still be required.
- All 76 have definition/runtime smoke coverage; this is NOT a claim of exhaustive behavioral or visual acceptance for every combination.
- The 19 reference bonds and 28 hero entries have been inventoried, but are not all imported as playable content. Existing original heroes and bonds are preserved pending a separate mapping and art pass.
- Boss death choreography still needs its own visual acceptance pass; this relic implementation does not close that review item.

## Validation

- `K26RelicChecks.Run`: catalog integrity, descriptions, pools, all mechanic identifiers, wave-grant idempotence, save restore, purchase triggers, temporary expiration, stat floors, alternate currencies, malformed journal rejection, 76 live simulation smoke cases and lotus removal.
- Logs and generated reports are under `Library/K26-*` and `PreviewExports/K26/`.
- Latest focused relic run: 156 checks passed, including a repeated-hit poison regression and saved six-weapon identity checks. K24 outside-stage regression passes after updating its expected swarm payout from one per six kills to one per three.
- K25 enemy-affix regression also passes against the combined working tree.
- An older K10 regression currently fails its 4–6-species-per-bond rule for `hearth` while a separate secret-event content change is being edited in this workspace. This pass does not alter that other agent's roster or weaken its assertion.
- No new Windows build is claimed until runtime/visual checks finish.
