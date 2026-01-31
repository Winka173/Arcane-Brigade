# Mode Balance 01 — 16 October 2026

## Scope and changes

Six selectable Free Journey difficulties; Night Shift, First Light and Small Brigade at their required fixed seeds/difficulty; endless continuation. No unsupported challenge/difficulty combination is fabricated.

Difficulty HP no longer multiplies two large factors: hardest late factor changes from 5.225 to 2.975. Difficulty pressure ramps from wave 1 through 8; hardest late damage factor rises from 1.095 to 1.209. Final difficulty density factor remains 1.6; only its early introduction changes. Peaceful base enemy factors, existing late-wave swarms/boss bulk, duration and endless exponential scaling are unchanged. LoneJelly's own weapon gets a bounded 1.2–1.5 damage catch-up. Special-currency stage inflation is removed; rarity/tier/veteran prices stay, ordinary gold economy stays.

Rationale, formulas and reference limitations: `Docs/Mode_Balance_Pass.md`. This is an original measured candidate, not a claim that Bizarre Brigade's exact numbers were obtained/copied. Fandom returned HTTP 402; qualitative community guide and prior local reference data informed direction, not invented source coefficients.

## Natural campaign matrix

Six representative heroes, two free-journey seeds, all six difficulties, plus three challenges at their single required seed: 90 cases per pass. No injected gold, HP, companions or victory. Actual shop prices, quotas and merge rules. K13 bot steering/purchasing is not a human player. Normal cutoff 60s / boss cutoff 180s; unresolved is neither a win nor loss.

| Journey difficulty | Baseline campaign wins / 12 | Candidate wins / 12 | Baseline mean terminal wave | Candidate mean terminal wave |
| --- | ---: | ---: | ---: | ---: |
| Peaceful | 6 | 6 | 16.08 | 16.33 |
| Wandering | 3 | 5 | 10.58 | 13.83 |
| Restless | 2 | 3 | 7.75 | 11.42 |
| Nightmare | 0 | 1 | 4.58 | 8.08 |
| Sleepless | 0 | 0 | 3.25 | 6.58 |
| Beyond the Lamp | 0 | 2 | 3.17 | 6.33 |

Per-stat difficulty ordering is strictly increasing, but this tiny bot sample does not demonstrate monotonic human win rates. Two wins on the highest difficulty do not prove it is easier than Sleepless; RNG paths, purchases and hero distribution matter.

Night Shift clears remain 2/6. First Light's actual wave-10 objective goes 4/6 → 5/6; judging it only by campaign victory would be wrong. Small Brigade's <=6-body final objective stays 1/6: two candidate campaign wins include one over capacity through build-generated gifts. The buyer does not delete gifts or force a win.

Baseline: 805 wave samples. Candidate: 1,020 wave samples, 23 campaign victories / 57 defeats / 10 unresolved across all modes. A second 90-run candidate replay reproduces the entire campaign CSV byte-for-byte (SHA-256 `4B6B3DF15ADBC232383F24695E836EF8BFD55964FF07CF9258BFF2E791CE3465`). Total full diagnostic campaigns: 270, not 270 distinct seeds or heroes. The earlier 90-case eight-wave baseline is separate historical evidence and not counted here. Raw CSVs and `Comparison.json` retain all cases.

## Endless

All 17 successful Free Journey builds continue through the real ending/EnterEndless boundary, with earned inventories/currency and no injected stats. Across 66 encounters: all 17 clear waves 21/22; 15 clear wave 23; at wave 24 nine clear, four die and two remain unresolved. Two died at wave 23. There are no naturally successful Sleepless entrants in this sample; all-tier compiled probes do not replace that missing natural coverage.

Separately, 18 explicitly injected modest fixed-loadout cases cover six difficulties at waves 21/24/30; all die. This is not a natural late build and is not used to justify an endless nerf. Raw files: `Endless_Controlled.csv`, `Endless_Method.txt`, `NaturalCampaign/Natural_Endless.csv`.

## Editor and Windows build

`Library/ModeBalance-FinalGate.log`: combat repair 133, Act One 512, MienCho 272, mode balance 4,229, FinalQuality 922, roster15 1,021, recruitment 799, hero identity 2,330, expanded performance 6,070, outside-stage 48, October 500, secrets 459, nightmare 224, nightmare visuals 34, farewell 1,214: PASS. Mode checks compile all 31 heroes x six difficulties at seven checkpoints, including wave-20 Customer duel and endless 21/30.

Actual Windows x64 build succeeds: zero build errors/warnings, reported size 1,628,378,648 bytes. Includes concurrent shared working-tree combat/art/story changes and all four scenes, not only balance code. Runtime DLL SHA-256 `B08DCB365B7FC2CE0AE20AE49AC05FBC522AA175507D96B2AAB074B77B19CEF6`. Player identity: `Build_Identity.json` and `Builds/FullRun/BuildInfo.json`. No commit/push is claimed.

## Native regression failure — not waived

Actual standard-profile shop capture boots and renders merchandise but fails `Stock and exit have disjoint interaction regions`. The concurrently changed bottom-centre exit is at (0,-4.5) plus layout lift, reach 1.85; the dynamically lowered merchandise row can overlap it. These are shop-layout changes outside this balance pass, not caused by difficulty coefficients. The assertion was not weakened or removed and the UI geometry was not overwritten. Reduced shop run is not counted as passed because the launcher stops on failure. Evidence: `NativeShop/Final/Native_Checks.txt`, `Player.log`, screenshots and pre-launch player identity.

This build is a **balance candidate, not a fully accepted release** until that shop regression is resolved and rerun. Separate native combat/story smoke completed with 58 passing checks and 21 actual screenshots, including both endings and automatic arrival/read-later resumption (`NativeCombat/Native_Checks.txt`). This single-profile smoke does not substitute for the failed shop gate or a reduced-settings pair.

## Remaining acceptance

Windows candidate package: `Builds/ArcaneBrigade-Windows-ModeBalance-01-Candidate.zip`, 654,305,302 bytes, 167 files verified against the source player after ZIP extraction hashing. SHA-256 `C6293131008A0970341138BBCD22E5FE692065CC1C5363807C8AA2462B019F64`. Package verification: `Package_Verification.json`. Native capture identity also matches the packaged player; packaging does not waive the shop regression.

Human playtesting, larger seed/hero/build coverage, low-spec frame timing and real Android/iOS testing remain open. No fresh performance improvement or locked-60 claim is made. Broader natural coverage of Sleepless and specialised purchasing for solo/small-team builds remains particularly important. Runtime stat monotonicity alone does not certify difficulty feel.
