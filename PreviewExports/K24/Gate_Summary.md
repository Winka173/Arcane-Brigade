# K23/K24 Windows gate — 2026-10-15

- Windows build succeeded: 0 errors, 0 warnings, 751107777 bytes reported by Unity. Local executable: `Builds/FullRun/ArcaneBrigade.exe`. Share the whole `Builds/FullRun` directory. Build binaries are ignored by Git; source, imported art and evidence are tracked.
- K24 focused checks: 48 passed. K23 merge animation/receipt fixtures and cooperative combat checks pass. The cooperation fixtures explicitly run after wave one, since wave-one hostile projectiles are intentionally suppressed; incidental weapon knockback is disabled in pull-specific fixtures.
- Regressions pass: K22 evolution 681, K21 encounter/contact 199, K20 anatomy 417, K19 encounters 325, K10 stats 172, K16 motion 220, K17 content 2476, K18 experience 91, K8 merchant 17; K4 merge also passes. K17 now includes the new Crimson/Rainbow companion rarities.
- Native Windows scenario: 23 passing checks, zero failures, 18 actual 3840x2160 captures. Real committed star-up purchase; hand challenge entry; all three boss phases; actual player-kill finale; three saved gifts with one claim; unchanged shop wave; subsequent swarm spawning; hand carry/toss/landing and resume.
- Reduced Motion native scenario: 23 passing checks, zero failures, 18 captures. Short transfer completes before the normal-flight checkpoint rather than failing because no carrier remains. No extended toss is required in reduced mode.
- Contact/shop native regression: 8 passing checks, zero failures, 4 captures. Reciprocal contact, nearby reroll placement, focus and two-gold payment remain correct.
- Selected accepted images are in `Accepted/`; complete image sets remain in ignored `Library/K24-Accepted-Native`, `Library/K24-Accepted-Reduced` and `Library/K24-Accepted-Contact`. Earlier oversized-layout and reduced-timing failures are not accepted evidence.
- Campaign/economy: ten champions at seed 171077; seven victories and three defeats, waves 14–20. Reports are fixed-policy bot observations, not human balance, frame-rate or fun approval. These routes take the ordinary exit rather than evaluating super-boss difficulty.
- Runtime DLL SHA256: `8A0E8ED27B0B243B0BCE26790E170AEB592F466DE6F3DC8F7702880CDABFEC70`.
- Resources SHA256: `35C1122332454226C0F5D94CE1693925D2589145599625EFB910BE4982AFF0A2`.

## Limits

Original K24 atlases are 1536x1024, not native-4K sources; frame/transform animation is not a skeletal rig. Existing hazard primitives are reused for six super-boss patterns. The hand transition is shipped; tongue/eye transfers and the previously approved enemy species/affix backlog are not implemented here. See `Docs/K24_Outside_Stage_And_Swarms.md` and `Docs/K24_Art_Prompts.md`.

Another agent is actively editing audio assets in the shared worktree. This local build imports the audio state present at build time; those audio files are deliberately excluded from this gameplay commit. Its resources hash therefore identifies this local artifact, not a promise that another checkout rebuilding only this gameplay commit reproduces the same audio. No audio listening approval, Android/iOS device gate, performance gate or publishing completion is claimed.
