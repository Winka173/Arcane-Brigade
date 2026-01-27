# K22 Windows gate — 2026-10-15

- Windows build succeeded: 0 errors, 0 warnings, 752777261 bytes reported by Unity. Executable: `Builds/FullRun/ArcaneBrigade.exe`; distribute the entire `Builds/FullRun` folder, not the EXE alone. Build output is local and ignored by Git; source, art/import metadata and evidence are versioned.
- K22 focused checks: 681 assertions passed. Regressions: K21 boss/contact 199, K20 anatomy 417, K19 encounters 325, K10 stats 172, K16 motion 220, K17 content 2474, K18 experience 91, K8 merchant 17. Total explicitly counted: 4596 assertions; K4 merge checks also pass.
- Native evolution fixture: 125 checks passed, zero failures, 60 captures at actual 3840x2160. Covers normal/mutated body art for all twenty species, no held enemy weapons, anticipation before release, actual body attack outcomes, real coin-fueled barrage, stable pose height and unchanged source catalog.
- Native boss regression: 129 checks passed, zero failures, 56 full-resolution captures of all ten bosses, real organ attacks/detachments, signature actions, stable core height and collection cleanup.
- Native contact/shop regression: 8 checks passed, zero failures, 4 full-resolution captures. Reciprocal collision damage, stock proximity, nearby reroll focus and two-gold purchase remain correct. Details: `Native_Contact_Shop_Checks.txt`.
- Eight selected full-resolution evolution captures are retained in `Accepted/`. Complete captures stay in ignored `Library/K22-Final-Native`, `Library/K22-Boss-Regression` and `Library/K22-Contact-Regression`. The earlier failed oversized-pose captures are not accepted evidence.
- Campaign diagnostics: ten champions, one deterministic seed each (171077); four victories, six defeats, final waves 11–20. See `Campaign.csv` and `Economy.csv`. This fixed-policy bot is not a representative human win-rate or a balancing approval.
- Runtime DLL SHA256: `335E42A0A6578DF2CC44B25B64064C147BF21B2A3E11E689C623B7B410A97BB0`.
- Resources SHA256: `B7F739344BD9C3A810EC33C0E6A901456DDDA9F7A9C7D49D658106811FF0CE79`.

Scope and source-art limitations: `Docs/K22_Enemy_Evolution.md`; exact prompt set: `Docs/K22_Enemy_Art_Prompts.md`. Sources are not native 4K, and eight distinct drawings per form are not a skeletal animation rig. Main-branch audio is retained without regeneration. Human play/art/audio approval, performance profiling, mobile device gates and publishing are not claimed complete. Historical K8 UI/K32 suites retain the pre-existing text/fixed-roster assumptions noted in the K21 report and are not included among passing suites.
