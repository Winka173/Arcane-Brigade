# Balance simulation

Complete; 792 build/wave/seed samples; 306.5743368 s wall time.
Fixed seeds, fixed timestep, default difficulty, one-star companions, no purchases, scripted circular kiting, normal wave director. CSV repeats wave totals per source; group by build/wave/seed before aggregating wave metrics.

Damage is actual enemy health removed, excluding overkill. Shares describe directly attributed damage, not the full team power of buffs, healing, shields, control, economy or their interactions. Existing runtime effect labels can combine multiple mechanics; unclassified damage remains visible. Step timing measures CombatSimulation.Step only and varies by machine; it is not deterministic output. Time to clear is the runtime's victory time (survival waves can end with live enemies). Timeouts are censored, not losses.
Bond step is the highest active step, not a marginal decomposition of each unlocked tier. Stat and haste benefits remain credited to the damaging source. Stone/Spiky Shell reflection is combined under a shared source with unknown step; reflected skill damage names the protected recipient. Companion skill copies retain the runtime caster attribution. Falls and down seconds include the hero and companions. Peak enemies is sampled at step boundaries.
Wave trials continue after defeats with full-health teams. Successful sampled waves award runtime survival growth; waves 21-39 and 41-59 are skipped without synthetic growth. The 240 s wave cap censors boss overtime. The 540 s batch watchdog writes partial output and fails if exhausted; a full run under ten minutes still requires measurement in Unity batch mode.

## Builds ranked

Ranked by wins, then mean victory time; compare equal completed sample counts. These are diagnostic wave trials, including trials after a defeat, rather than run survival probabilities.

| Build | Hero | Wins / samples | Losses | Mean victory seconds |
|---|---|---:|---:|---:|
| Feast Fire | SupperMortar | 66 / 66 | 0 | 62.314168468 |
| Cross build: Shadow Garden | DrowsySeer | 66 / 66 | 0 | 70.523001758 |
| Frozen Shore | DrowsySeer | 66 / 66 | 0 | 78.762140968 |
| Cross build: Wet Thunder | BellDiver | 66 / 66 | 0 | 79.166711114 |
| Poison Grove | VerdantMatron | 65 / 66 | 1 | 78.160749406 |
| Lamp Court | VelvetRegent | 58 / 66 | 5 | 67.6855255 |
| Black Hole Clock | BonsaiClocksmith | 57 / 66 | 1 | 57.749583529 |
| Ghost Choir | SupperMortar | 57 / 66 | 0 | 61.623498415 |
| Burning Court | StormFencer | 57 / 66 | 9 | 67.119452694 |
| Iron Fortress | LanternKnight | 53 / 66 | 4 | 56.837307696 |
| Night Library | MoonProwler | 42 / 66 | 15 | 53.775758289 |
| Static Gale | CometRunner | 41 / 66 | 19 | 53.262974158 |

## Recipe audit

Section 4 supplies bond targets without exact heroes or companion rosters. Trials use reproducible selections; achieved counts below must be checked before treating a result as the intended recipe.

- **Feast Fire**: requested hearth 8; ember 6; sweet-tooth 2; ball 2; achieved ball 3; demolitionist 2; ember 6; hearth 8; mend 2; sweet-tooth 7.
- **Cross build: Shadow Garden**: requested dusk 6; orchard 6; clover 2; cloud 2; achieved astral 5; cloud 2; clover 2; dusk 6; hat 5; orchard 4; root 3; spiky-shell 3. orchard 4/6
- **Frozen Shore**: requested frost 8; tidal 6; gauntlet 2; cloud 2; achieved cloud 3; frost 8; gauntlet 5; hat 2; sea-shanty 3; tidal 6.
- **Cross build: Wet Thunder**: requested tidal 6; static-club 6; rain-script 2; demolitionist 2; achieved bastion 2; frost 6; gauntlet 3; rain-script 5; sea-shanty 2; static-club 4; tidal 6. static-club 4/6; demolitionist 0/2
- **Poison Grove**: requested orchard 8; venom 6; root 2; spiky-shell 2; achieved bastion 2; bulwark 2; mend 2; orchard 8; root 6; shoe 3; spiky-shell 2; venom 6.
- **Lamp Court**: requested lantern-guild 6; regal 6; hat 2; mask 2; achieved archive 6; exhibit-hall 2; lantern-guild 6; mask 4; oddity 4; regal 4; rune 4. regal 4/6; hat 1/2
- **Black Hole Clock**: requested gravity 8; clock 6; singularity 2; gear 2; achieved clock 3; echo 3; gear 4; gravity 8; melody 6; singularity 4; stone 3. clock 3/6
- **Ghost Choir**: requested spirit 8; melody 4; echo 2; mend 2; achieved bulwark 7; echo 3; gravity 3; melody 3; mend 5; shield 3; spirit 8. melody 3/4
- **Burning Court**: requested ember 8; oddity 4; desperado 2; bladenado 2; achieved ball 3; desperado 3; ember 8; hearth 5; oddity 2; sweet-tooth 3. oddity 2/4; bladenado 0/2
- **Iron Fortress**: requested stone 8; bulwark 6; bastion 2; monolith 2; achieved bastion 2; bulwark 3; clock 6; gear 4; horn 4; monolith 2; shield 2; spirit 2; stone 8. bulwark 3/6
- **Night Library**: requested archive 6; dusk 6; rune 2; bludgeoner 2; achieved archive 6; cloud 2; dusk 4; hat 3; lantern-guild 4; rune 6. dusk 4/6; bludgeoner 1/2
- **Static Gale**: requested static-club 6; windborne 8; sniper 2; rapidfire 2; achieved paper-wings 5; rain-script 2; rapidfire 3; sniper 3; static-club 7; windborne 8.

## Waves lost on every seed

- Iron Fortress: 10.
- Night Library: 10, 17, 19.
- Static Gale: 10, 14.

## Damage-share outliers

Bond screens: direct damage share greater than twice 5% / 12% / 25% / 50% for active steps 2 / 4 / 6 / 8. These design targets describe team power, so this is a screening signal, not a causal balance verdict. Skills have no expected-share target in the design: skill share above 10% is listed only as a heuristic (twice the 5% step-2 proxy), not a design-target violation.

278 flagged source samples; showing the largest 30. Full source shares are in balance.csv.

| Build | Wave / seed | Source | Step | Share | Screen |
|---|---|---|---:|---:|---|
| Cross build: Shadow Garden | 19 / 7919 | EclipseEgg | — | 29.65 % | skill heuristic |
| Black Hole Clock | 1 / 104729 | NewtonApple | — | 28.63 % | skill heuristic |
| Black Hole Clock | 1 / 104729 | TopBeetle | — | 28.22 % | skill heuristic |
| Iron Fortress | 1 / 104729 | TopBeetle | — | 27.88 % | skill heuristic |
| Iron Fortress | 6 / 7919 | TopBeetle | — | 27.76 % | skill heuristic |
| Iron Fortress | 14 / 7919 | TopBeetle | — | 27.20 % | skill heuristic |
| Iron Fortress | 2 / 281026 | TopBeetle | — | 27.11 % | skill heuristic |
| Cross build: Shadow Garden | 16 / 104729 | EclipseEgg | — | 26.42 % | skill heuristic |
| Iron Fortress | 2 / 7919 | TopBeetle | — | 26.29 % | skill heuristic |
| Iron Fortress | 6 / 104729 | TopBeetle | — | 26.05 % | skill heuristic |
| Iron Fortress | 5 / 104729 | TopBeetle | — | 24.75 % | skill heuristic |
| Iron Fortress | 3 / 104729 | TopBeetle | — | 24.64 % | skill heuristic |
| Cross build: Wet Thunder | 1 / 104729 | RaincoatFrog | — | 24.40 % | skill heuristic |
| Iron Fortress | 1 / 7919 | TopBeetle | — | 24.11 % | skill heuristic |
| Iron Fortress | 2 / 104729 | TopBeetle | — | 23.68 % | skill heuristic |
| Iron Fortress | 4 / 7919 | TopBeetle | — | 23.19 % | skill heuristic |
| Iron Fortress | 5 / 281026 | TopBeetle | — | 23.16 % | skill heuristic |
| Cross build: Shadow Garden | 18 / 281026 | EclipseEgg | — | 22.68 % | skill heuristic |
| Black Hole Clock | 2 / 104729 | TopBeetle | — | 22.15 % | skill heuristic |
| Iron Fortress | 3 / 7919 | TopBeetle | — | 21.98 % | skill heuristic |
| Black Hole Clock | 2 / 104729 | NewtonApple | — | 21.88 % | skill heuristic |
| Iron Fortress | 3 / 281026 | TopBeetle | — | 21.82 % | skill heuristic |
| Iron Fortress | 11 / 281026 | TopBeetle | — | 21.81 % | skill heuristic |
| Night Library | 1 / 104729 | PillowOracle | — | 21.78 % | skill heuristic |
| Black Hole Clock | 3 / 281026 | TopBeetle | — | 21.73 % | skill heuristic |
| Black Hole Clock | 1 / 7919 | TopBeetle | — | 21.70 % | skill heuristic |
| Iron Fortress | 40 / 7919 | TopBeetle | — | 21.64 % | skill heuristic |
| Iron Fortress | 20 / 7919 | TopBeetle | — | 21.64 % | skill heuristic |
| Cross build: Shadow Garden | 14 / 7919 | EclipseEgg | — | 21.58 % | skill heuristic |
| Night Library | 19 / 281026 | PillowOracle | — | 21.47 % | skill heuristic |
