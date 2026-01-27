# K21 Windows gate — 2026-10-15

- Windows build: succeeded, 0 errors, 0 warnings; 699003099 bytes reported by Unity.
- K21 focused: 199 assertions passed. K20 anatomy: 417. K19 encounter: 325. K10 stats: 172. K16 motion: 220. K17 content: 2474. K18 experience: 91. K8 merchant: 17. K4 merge checks also passed.
- Native boss fixture: 129 checks passed, no failed checks, 56 full-resolution 3840x2160 captures. Includes all ten bosses, real organ windup/release/breaks, signature actions, stable body height, collection categories and preview cleanup.
- Native contact/shop fixture: 8 checks passed, 4 full-resolution captures. Covers reciprocal contact damage, champion attribution, stock proximity, reroll focus/price and actual two-gold purchase.
- Selected accepted images are in `Accepted/`. Full diagnostic captures stay in ignored `Library/K21-Stable-Boss` and `Library/K21-Stable-Shop`; notes are copied alongside this report.
- Build executable: `Builds/FullRun/ArcaneBrigade.exe`; share the entire `Builds/FullRun` folder. Main-branch Toybox audio is included, not regenerated.
- Runtime assembly SHA256 (`ArcaneBrigade.Runtime.dll`): `3409C0AB1511C195DA7C96F6BEE32195D0C5F869D3784626E39E190C72E8F199`.
- Resources SHA256 (`resources.assets`): `EE379A96E8ED50039F5464FC58B187D01EC6899DE29F05B9FE9D02524D07F864`.

Historical K8 UI and K32 full-run suites were also attempted but stop on their existing Vietnamese unlock/fixed roster assumptions. They are not counted as passing. See `Docs/K21_Boss_Contact_Collection.md` for limitations, source-art resolution, build behavior and remaining ordinary-enemy evolution work. Human gameplay/balance approval and mobile device gates are not replaced by these automated checks.
