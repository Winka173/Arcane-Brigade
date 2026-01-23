# K17 Windows review checkpoint

## Delivered scope

Eight original body-attack enemies, five original bosses (ten total), ten selectable heroes, five hostile shot trajectories, clearer companion/item rarity, stronger old bosses, escalating prices and an actual three-purchase ally quota shared by both shops. Shop stock no longer intersects the merchant. Three moving pedestal styles, downward carpet unrolling, a paper-manta transfer every four cleared waves, three painted ground materials, selected collection motion, plain shorter descriptions and a player beacon/rim are connected to runtime.

Companion attack rarity bonuses are 18/36/54%, in addition to the existing elite bonus. Three-star merging does not refund the wave purchase quota. No upkeep, extra skill buttons or permanent between-run stat upgrades are introduced. Defeated Coin Leeches return stolen loose coins.

Wave-20 ordinary enemy HP is approximately x19.4 before difficulty/build modifiers; boss growth adds its boss multiplier. Speed approaches, but never reaches, x2.8. Damage pressure grows while the existing per-hit safety cap remains. Scaling speed/damage literally x20 would violate the established avoidability/no-one-shot requirements.

## Verification

- `Library/K17-Ship.log`: Unity compilation, build and regression execution; Windows build succeeds with zero errors and zero warnings.
- 7,907 editor assertions across current K17 and earlier focused/regression suites, including real existing-species purchases/fusion, cross-shop quota, all ten hero starts, stationary combat versus shop movement, solo item-only shops, real enemy/boss ability execution and ring/lane hit geometry. Fifteen selected atlases pass clean-gutter import checks separately.
- `PreviewExports/K17/ArtPlayer`: twelve native 3840x2160 pose captures, 171 passing checks. All five new bosses and eight new enemies use actual authored sprite rectangles, not concept mockups.
- `PreviewExports/K17/PresentationFinal` and `ReducedPresentation`: real shop arrival, rarity, stock placement, purchase, downward departure and same-scene map transfer, including passenger-following camera. Additional normal-mode boss-effect fixture is explicitly not normal gameplay or a performance benchmark.
- `PreviewExports/K17/Campaign.csv` and `Economy.csv`: ten deterministic, movement-only routes. Five victories, five defeats, no timeouts for seed 171077. Lone Jelly uses a melee-aware movement policy; the old brigade-kiting policy never approached its boss. No free currency or invulnerability is granted to campaign bots.
- `git diff --check` passes. PNG sources are original generated illustrations; see K17_Art_Provenance.md.

## Limits and remaining acceptance

These checks do not certify that every hero is equally easy or that economy feels ideal. Root Oracle and Comet Runner lose at wave four in this bot seed; that evidence is retained, not relabeled as a balanced campaign. Human playtesting remains necessary for challenge tuning. The build contains normal hard encounters, not the boosted-health five-boss presentation fixture used to inspect rendering.

Music scene gains are reduced; no claim of ear-verified mix quality is made. UI was not wholesale redesigned over the other agent's work. Windows runtime is verified; Android/iOS real-device gates and publishing remain open as previously agreed. Source sprite sheets are not native 4K; native captures are 4K.

## Play build

Run `Builds/FullRun/ArcaneBrigade.exe`. Distribute the entire `Builds/FullRun` directory, including data, UnityPlayer.dll and notices, not only the executable.

Quick review: compare enemy bodies/shot paths after wave two; survive the first boss; buy three allies and reroll to confirm item-only stock; inspect purple/gold and rare-companion auras; clear wave four to watch the carrier/material change; try Root Oracle (stationary only in combat) and Lone Jelly (no allies); view moving collection previews. Test Reduced Motion as well.
