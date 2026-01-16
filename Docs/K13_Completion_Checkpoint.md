# K13 gameplay, presentation and UI checkpoint

Audio/music and release/platform work are excluded by the owner's request. This is a Windows gameplay candidate, not a production-art or human-playtest certification.

## Delivered

- Recruitment prioritizes merge-ready species on 85% of eligible first-slot rolls, retaining ordinary random recruits, per-wave quota, rarity/Luck and roster soft-cap rules. Owned species remain available for upgrades.
- Two actual tier-one companions approach focused merge stock. Gold threads and star badges indicate the projected tier; cascades to tier three are handled. Hover does not change saved roster, wallet, receipts or shop/weapon randomness. Transaction rollback still creates no merge reveal.
- Eight original weapons: giant dream orb staff, extinguisher, three-bolt plasma gun, four-feather fan, dual sabres, outbound-homing/returning clock crescent, long parasol thrust and stamp hammer. Melee/ranged count is four/four. Orb/crescent are special-only; elite recruitment prefers an available exclusive weapon after respecting roster melee/ranged balance. Each projectile has its own authored shape where appropriate.
- Starting hero weapons are fixed: Key Cleaver, Parcel Launcher and Tuning Bell. Authored base speeds are 3.30/3.65/3.10; run modifiers still apply. Seer's starting identity adds a second bell projectile, 150 base HP and stronger periodic healing rather than a manual skill.
- Existing authored unit frames receive independent head/legs, species/material idle gestures, windup/release, recoil and material-dependent death motion. Bosses have distinct motif flourishes; hammer impacts use dust rather than a generic sword slash. Chase dust, shadows and tier scaling remain active.
- Shop labels distinguish allies/relics, show merge stars, center on stock and fit large prices on one line. Compact scoped stat names remain icon-linked/clickable; glossary punctuation stays attached to the stat instead of orphaning on another line. Detailed inventory still exposes the full calculation.
- Champion cards use concise one-line unlock requirements; all three selected-character panels are included in the desktop audit.
- Same-scene staged merchant/carpet/stock arrivals and departures remain intact. Focus/denial timing now freezes with the presentation clock. New painted projectiles do not acquire oversized generic halo/core circles.
- The map is redrawn as layered stone/ceramic, transparent patina and fine grain. Thirty decorative raised props are removed, preserving interactive toys and boundaries. Broader phase palettes still animate in-place.

## Balance decisions

Dense wave-four/five batches and large budgets are retained. Spawn cadence is relaxed from the excessively compressed previous curve to `max(0.22, 0.46 - (wave - 4)*0.012)` seconds before existing wave/count modifiers. Boss health remains substantially boosted. Introductory incoming damage uses a difficulty pressure multiplier from 0.62 to 1.00; there is no shared post-hit invulnerability or companion bullet blocking without an item.

Enemy health and speed continue growing. The per-source damage ceiling now also grows after wave ten; the champion's existing per-hit fractional-HP safety cap remains. Prices still grow 7% per wave for gold stock, 3.5% for dream stock, before the established rarity/discount calculations.

The new deterministic movement/shop policy collects actual nearby loot, avoids enemies/projectiles, uses available dream stock and prioritizes useful upgrades over dangerous enemy-stat contracts. It does not grant free currency, immunity or bonus combat stats. Normal difficulty uses three published seeds for each hero; the higher four difficulties use their original seeds. First-pass and final CSVs are kept separately. A complete normal route must exist for each hero, while defeats are retained rather than relabelled as passes.

## Acceptance evidence

Focused checks: `PreviewExports/K13/Focused_Checks.txt`; build report: `PreviewExports/K13/Windows_Build.txt`. Related stat, melee/tier and rollback/reveal regressions also run. The legacy melee fixture now uses separate three-recruit windows, consistent with the new per-wave quota; the old stat assertion checks the current English personal-trait wording rather than removed Vietnamese text.

`PreviewExports/K13/Player` contains native merge/purchase/denial/weapon captures, desktop menu pages and inventory/shop at 1280x720, 1600x1200 and 2560x1080, in addition to 3840x2160. The weapon showroom uses an explicit stationary durable enemy and delayed champion attack to expose companion delivery; it is not a natural campaign capture.

`PreviewExports/K13/HordePlayer` separately checks natural dense combat, boss entrances and both no-loading market transitions on the final executable. These are saved fixtures, not a full natural rendered campaign.

The final balance matrix, runtime exit status, check counts and executable fingerprints must be read from the accompanying reports. Full source and used art are included; share the entire `Builds/FullRun` folder, not its EXE alone. Android/iOS/Steam, controller hardware, soundtrack and subjective human quality approval are not claimed.

Art source/provenance and animation technique: [K13 Art and Motion](K13_Art_and_Motion.md).

## Final results

- Windows build succeeds with zero errors/warnings. Numeric gates: 121 core, 1,223 polish, 496 market, 22 transitions, 216 K13, 1,258 melee/tier and 165 stat checks (3,501 assertions), plus the rollback/merge reveal suite.
- Both native executable fixtures exit zero: 24 gameplay/UI checks and 39 horde/transition checks, with 44 screenshots across the tested desktop surfaces. The final horde fixture reaches 31 live enemies; 1,320 local RTX 5070 Ti frame samples report 16.67ms median and 16.68ms p95. This is not a maximum-roster or minimum-hardware certification.
- Final 21-run published balance matrix: 10 victories, 11 defeats. Normal routes win 3/3 Lantern, 2/3 Postmaster and 2/3 Seer; victories clear wave 20 and exercise continuation to wave 23. Hard-mode failures remain recorded. The new policy and tuning are not compared as if identical to the legacy bot's purchase strategy.
- Final assembly, packed scene/art data and launcher hashes are recorded in `PreviewExports/K13/Artifact_SHA256.txt`. The earlier K12 executable manifests/evidence are historical.
- All requested implementation categories in this checkpoint are closed. Soundtrack/SFX work and platform release remain deliberately excluded; subjective human aesthetic approval is not inferred from automated checks.
