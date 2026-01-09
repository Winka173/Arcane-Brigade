# K11.3 — melee pursuit, star growth and English

## Gameplay changes

- Friendly melee weapons gain50% effective damage,35% attack range and55% authored visual width. The buff applies once to compiled clones, after run stat bonuses; it never modifies enemy weapons or stacks again on purchases/reloads. Ranged damage is unchanged.
- Melee companions gain30% movement speed and a leash2 world units wider than ranged companions. Movement has explicit Patrol/Pursuit modes. Pursuit searches beyond weapon reach within the permitted player neighborhood, approaches/orbits enemies at useful striking distance, and returns to patrol without valid targets.
- Patrol emits light painted foot dust. Pursuit emits denser/larger puffs, subtle speed streaks, quicker footsteps and a forward body lean. Effects are pooled/capped, freeze on pause and disappear in reduced-motion mode. Companions retain random idle rests, immortality and default nonblocking bullets.
- Tier2/3 companions gain18%/36% body size, attack range and projectile size over tier1, plus15%/30% weapon width. Projectile visual width and collision radius scale together; flight lifetime grows with range. Existing extra bullets/pierce, damage multipliers and melee cooldown/sweep tier benefits remain. The body-size display cap now grows with stars rather than flattening larger tiers.
- Wave victory restores the champion to current maximum HP before the shop/save boundary, including the final normal wave and endless waves. Defeat never revives the champion. Blood-shop purchases still spend HP, and a shop recompile/reload is not a free heal.

## Recruiting balance

Recruit, merge and altar weapon selection prioritize the underrepresented melee/ranged class in the current live roster; ties use the saved weapon RNG. This corrects the earlier50/50 coin-flip sampling, which could produce heavily skewed small parties. Unique body+weapon rules and thematic recruit pools remain intact.

This is an adaptive50/50 target, not forced conversion of existing weapons: merges remove three units and can temporarily shift counts; the next selections compensate. If the needed class has no unused variant for that species, selection falls back to available variants rather than blocking recruitment or violating uniqueness. Fixed starters/old saves are retained.

## English presentation

Runtime UI, menus, shop interactions/dialogue,48 stat labels, glossary explanations, museum, receipts, records, controls and automatic-mechanic descriptions are English. Authored unit/relic/bond names and descriptions are migrated without changing IDs, modifiers, prices or save format. Existing English signature descriptions are preserved. Purchased stats still reset on a new run; no profile stat growth.

Existing user/profile history strings are not destructively rewritten; old free-text nicknames can remain in their original language. This checkpoint makes the game English-only rather than adding a language selector. Font family names and legal notices remain unchanged.

The build ledger reads independently compiled snapshots, so displayed damage, speed, range and projectile counts match actual melee/star rules without mutating the active run. Weapon sandbox uses the same melee and tier scaling.

## Validation

Focused model checks cover every weapon, recompilation immutability, real distant melee pursuit/kills, leash return, pause/reduced-motion dust, full-wave/final/endless healing, blood purchases, adaptive recruitment with merges, English authored content, star growth and read-only ledger agreement. Separate theme/transition and K8 motion gates are rerun. Windows native captures and full-run movement-bot diagnostics are reported with final results, not treated as human art/audio/balance approval.

Final core gates:1,141 melee/tier/English assertions,22 transition checks,167 themed-mechanic checks and51 K8 motion checks. Earlier legacy gates are recorded separately rather than claimed rerun under these changed rules.15 scripted full runs reached victory across three champions and five difficulty tiers; this is smoke coverage, not a human difficulty assessment.

Source audit finds zero remaining accented-language literals in runtime code. Native review caught a clipped long English merchant title; the dialogue panel now fits title font size and expands height for the full line instead of clipping text.

Windows build succeeded with zero errors/warnings. Final native player:65 PASS, zero FAIL,21 framebuffer PNGs verified3840x2160. Includes natural wave-end full healing, live pursuit, English main menu/dialogue and a separately saved six-variant tier1/2/3 showcase with actual enlarged projectiles. The refreshed38-second movie is silent Unity framebuffer footage at1920x1080/timeScale1, not an FPS benchmark or audio approval.

Remaining broader K11 work: artifact diversity, concise highlighted synergy previews, bespoke enemy/environment artwork and human difficulty/pacing review. Android/iOS device gates remain open. Combat-boss arrival choreography is separate from shopkeeper entrances.
