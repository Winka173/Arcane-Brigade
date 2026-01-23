# K18 — movement-only experience expansion

## Selected scope and implementation

All eight requested proposals are implemented. Combat still requires movement only. There are no active skill buttons, upkeep, permanent stat progression or new blocking event menus.

| Proposal | Implementation | Verification |
| --- | --- | --- |
| 2 — controlled merge assistance | Paid rerolls track eligible two-copy species missed by that market. Selection preference rises from 85% by 3 percentage points per miss; after four misses an eligible companion slot prioritizes the most-overdue pair. | Assistance persists in saves; failed rerolls do not advance it; wrong-market refreshes do not advance it; a completed merge clears it. |
| 5 — behavior-changing weapons | Moonseed Orb, Afterimage Sabre and Satellite Disc are recruitable variants with delayed bloom, fixed-position delayed slash and target-orbit/return behavior. | Attributed-damage fixtures, native projectile/field observations, orbit-to-return observation, collection/sandbox explanation checks. |
| 6 — short optional events | Paper Parade, Gilded Trap and Wandering Eclipse occur on odd non-boss waves starting at wave 5. | Actual simulation and native captures cover all three warnings, active states, expiry and cleanup. |
| 8 — purchase consequences first | Default shop copy shows short scoped stat changes and owned counts. Ally previews account for cascading merges and net support changes. INFO opens the longer explanation and compact before/after column; BACK restores the short view. | Pure-preview save/RNG invariants, actual cascading merge comparison, native focused offer and expanded details captures. |
| 9 — weighted impacts | Normal, critical, heavy and finishing hits have distinct bounded accents. Heavy/critical impacts can briefly hold the attacking visual as well as the victim. | Existing impact regressions and bounded-accent checks pass. Gameplay time is never globally stopped. |
| 10 — state transitions | Gait weight eases into/out of motion, acceleration changes squash, landing settles into walking, attack-facing rotation blends rather than snapping. | Numerical transition checks and actual native run/stop gait checks; existing authored frames remain in use. |
| 7 — readable combat | Crowded scenes reduce allied cosmetic opacity/count, while enemy shots remain solid, pink-accented and above friendly shots. Hostile field warnings retain priority. | Explicit native dense-projectile fixture checks both factions, draw hierarchy and opacity. |
| 3 — wave rhythm | From wave 3, each wave has opening, build-up, climax, recovery and finish phases instead of a continuously accelerating spawn interval. | Theoretical spawn-volume checks at 35/45/55 seconds and actual early/late population regressions pass. |

## Guardrails

- Assistance is species-based, not a promise of a particular random weapon. It does not create a companion slot after the quota: the existing 15% slot gate after two purchases and three-body shared maximum remain. Unlocks, market eligibility, hero restrictions and available weapon combinations still apply. A displayed eligible copy resets the drought; deliberately passing an offer is not a missed roll.
- The short ally stat preview reports the net support change from that species after merging, not a fictitious additional tier-1 buff. Bond changes are called out separately, including BOND DOWN warnings. Random weapon results are not advertised before purchase.
- Moonseed Orb is dream-recruit-only. Blooming fragments cannot recursively bloom. The afterimage remains at the original cast point. Satellite Disc keeps its victim set across orbit/return, so it cannot farm repeat hits on one enemy.
- Events have a two-second damage-free warning and last ten seconds. Parade rewards are capped at twelve single-value coin drops. The trap offers one chest. Eclipse exposure can yield up to eight two-value coin drops. Event contact is limited to one hit per second, at most 5% of maximum HP per hit, with smaller absolute caps. Leaving the footprint avoids subsequent contact. Events freeze with simulation pause and clear in the shop.
- Event actors reuse project-owned animated courier/pedestal art plus new geometric flourishes. Weapon variants reuse original weapon silhouettes with different tint, projectile motifs and motion. This checkpoint does not claim newly hand-drawn sprite sheets or native-4K source artwork.
- Reduced Motion suppresses ornamental movement without hiding event danger. Reduced Effects reduces friendly cosmetics, never hostile projectile visibility. Camera accents retain their cooldown; neither hit-stop nor animation smoothing changes player input, AI simulation or collision timings.
- Rhythm is normalized to keep expected spawning within 94–106% of the prior cadence on the tested wave durations. An initial unnormalized draft reduced coins too much and is not the shipped rule. Enemy HP, boss HP, wave pricing and recruit quota from the previous balance checkpoint are otherwise preserved.

## Verification and handoff

- `ArcaneBrigade.Editor.K18ExperienceBuilder.Build` authors/rebuilds the three variants and ink metrics, then verifies/builds Windows. `Verify` checks existing content without resetting weapon definitions. `Campaign` writes separate K18 bot/economy observations.
- K18 focused checks: 91 passes. Ink/weapon checks: 268 passes. Shop/carpet checks: 2,247 passes. Existing K17/K16/K15/K14/K8/K12/market/K11/K13/K10 regressions pass. The historical K14 six-weapon check remains scoped to its original six mechanics; K18 checks cover the appended mechanics.
- `PreviewExports/K18/Weapon_DPS.csv` compares each variant with its source against eight stationary high-health targets for five seconds. This is a narrow authored-weapon diagnostic, not equal-power certification, ordinary-run DPS or proof of a general win rate.
- Native Windows evidence uses actual 3840×2160 frames. The weapon, dense-projectile and boss scenes are explicitly isolated fixtures, not ordinary encounters or performance benchmarks. Event samples accelerate simulation between screenshots; human animation/audio quality still requires playing the build.
- Final native results: shop/transport 793 passes normally and 748 with Reduced Motion; weapon/event/readability fixtures 25 passes in each mode. All four runs have zero failed assertions or captured runtime errors. Captures and individual notes live in `PreviewExports/K18/ShopFinal`, `ShopReduced`, `ExperienceFinal` and `ExperienceReduced`.
- Ten-hero campaign at seed 171077: four victories, six defeats, no timeouts. Glass Dice reached wave 10, Tide Singer and Mirror Keeper wave 16, Lantern Knight wave 19, Postmaster and Comet Runner wave 20 before defeat. This is one movement-bot policy/seed, not a human win-rate or a claim that every hero is equally balanced. `Campaign.csv` and `Economy.csv` preserve the observations.
- Windows build: succeeded, zero errors/warnings, 652560967 bytes. Run `Builds/FullRun/ArcaneBrigade.exe`; distribute the entire `Builds/FullRun` folder.
- Runtime assembly SHA-256: `F5CA84A821E0FCDAB94A9EBC269FE554937607EC1F339372A852355C35E4C4FF`.
- Resource data SHA-256: `A651AE51E002FC29A13FD4029B165C60FA3EBD9E4FC74A04F9116A94ADBC20A6`.

No audio clips, music manifests or the audio agent's working documents/tools are included in this gameplay change. New weapon attacks reuse the existing motion/family/element audio routing, and event loot/damage uses existing loot/hit events. Android/iOS device testing and release gates remain open as previously agreed.

## Playtest checklist

- In the shop, confirm the short purchase result is understandable before opening INFO; compare a three-star cascade and watch for a lost bond milestone.
- With two matching one-star allies, verify paid rerolls favor the missing species over time without bypassing quota or dream/normal market separation.
- Try each new weapon in the collection sandbox and then in a real run; moving enemies should make the fixed afterimage feel different from an immediate sword swing.
- On eligible waves, ignore one event, approach another during its warning, and enter/leave during its active phase. Reward should feel optional, and warning geometry must communicate the risk without another panel.
- At a large army size, verify enemy shots and the player's foot marker remain readable. Review normal/reduced settings separately.
- Check start/stop, pursuing melee allies, landing and critical finishing blows for responsive weight without input delay. Bot checks and still images do not replace this feel review.
