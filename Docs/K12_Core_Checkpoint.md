# K12 core checkpoint — 2026-10-14

This is an intermediate combat/shop checkpoint, not completion of the requested animation, map, UI and audio overhaul. Another agent is handling music; this checkpoint changes no audio source files.

## Implemented

- Boss compiled HP multiplied by4 relative to the preceding build; enemies gain additional health scaling after wave3. Existing wave-based damage/speed growth, per-hit champion safety caps and hostile projectile speed caps remain.
- Wave4 enemy budget is3 times the earlier curve, wave5+ is4 times. Spawn intervals sharply fall from wave4; the simulation's old0.3-second floor is removed so the authored faster interval actually takes effect. Budget remains a concurrent-enemy bound, not a total per-wave kill quota.
- Ordinary weapon damage now applies a small positional knockback to surviving enemies using existing PushDistance/Knockback stats. Per-unit KnockbackResistance reduces it; compiled regular enemies have at least15%, large enemies60%, bosses85%. Spatial index moves with the target. The old umbrella-specific second push is removed to avoid double knockback.
- Original four-frame Furnace Toad and Wardrobe Crab strips are imported with true alpha, stable four-cell slicing, BC7 Windows/ASTC mobile settings. They unlock at waves4/5, have180/240 base HP,1.65/1.9 body heights and65%/75% resistance. They use body attack definitions, never held weapons. Generated source2172x724 is not native4K.
- Gold-shop descriptions now show concise rarity/stacking/stat deltas; companion summaries show traits, passive and merge progress. Detailed mechanics remain in the brigade ledger/glossary rather than being deleted.
- At most3 new species can be purchased per wave across shop stages; saved NewRecruitsThisWave tracks purchases. Existing species copies remain available for star upgrades. This interprets the requested2–3 allowance as3 new species, not a hard cap on all recruit purchases or total roster.
- New-species appearance chance declines at roster counts10/12/14/16/20:45%/30%/18%/10%/3%, multiplied by Luck with1–95% bounds. Existing copies are not filtered by this soft cap. Multiple species in the pool share one appearance roll so large pools cannot defeat the probability reduction. Existing saves/rosters are not forcibly trimmed to20.
- Eclipse Egg and Pillow Oracle are promoted to dream-shop-exclusive recruits with35% extra compiled damage, cost4 eggs. They retain existing art and traits; no claim of new special-companion artwork. Existing owned copies remain valid.
- Luck already biases higher-rarity item weights and egg/heart loot. Gold item prices now multiply by1+0.4×rarity; special prices by1+0.25×rarity, preserving discount/blood minimum-HP rules. Purple/gold are not guaranteed by Luck.

## Research boundaries

Indexed [reference relics](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics) support Luck, Treasure Detection, knockback and rarity/stat tradeoffs. Indexed [companions](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions) show companion-specific modifiers and rarity. Full wiki pages remain incomplete/inaccessible. Exact2–3 per-wave recruitment limits, soft-cap probabilities, rarity prices and universal special-shop power multipliers were not verified as reference formulas: these are explicit Arcane design choices based on user instructions.

Treasure Detection, pickup radius, chest quality, Luck, pierce, split, bounce, size/speed/travel distance, income and interest already exist; they are not re-added under duplicate stat names. Treasure Detection affects chest frequency, not a newly claimed separate chest-range stat. Additional missing-reference mechanics need verified audit before claiming full parity.

## Validation and remaining work

This is the historical core checkpoint. Subsequent market/animation work and its acceptance evidence are documented in [K12 Market Polish](K12_Market_Polish.md).

K12 focused121 assertions and K11 transition22 assertions pass; Windows build succeeds with zero errors/warnings. Checks cover real wave5 density, wave growth, first-wave shooter exclusion, quota enforcement/copy availability/save persistence, rerolls, exclusive pool, nonzero soft cap and imported art/compact summaries.

Native 4K capture, knockback gameplay fixtures, full-run balance and device performance have not yet been rerun for K12; K11 captures are historical. Stronger bosses/density require balance review rather than assuming earlier15 bot victories still apply.

Still required: species-specific movement/attack/death animations, combat-boss entrances, refreshed environmental art, visual tier identities beyond scaling, highlighted shop synergy, native performance/effect-readability checks and human quality review. Music/audio is being handled separately. Do not mark the entire quality request complete from this checkpoint.
