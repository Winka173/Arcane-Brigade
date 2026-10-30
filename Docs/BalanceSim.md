# Balance simulator

Run from the project root with the installed Unity editor:

```powershell
& '<Unity editor path>/Unity.exe' -batchmode -nographics -quit -projectPath 'C:/ABClones/wt2' -executeMethod ArcaneBrigade.Editor.BalanceSim.Run -logFile '.utmp/balance-unity.log'
```

The entry point runs its deterministic checks first, then writes `PreviewExports/Balance/balance.csv` and `balance_summary.md`. It uses the production `RunCatalog`, `RunSession.PrepareCombat`, `CombatSimulation` and normal director. No scene, presentation objects, shopping, or gameplay tuning changes are involved.

The matrix is twelve recipes × three fixed seeds × twenty-two waves (792 trials). All companions have one star and their catalog weapons. The hero follows a circle at 24% of the arena's shorter dimension, steering from its actual position. Input and simulation use a fixed 0.05 s step. Waves 1–20, 40 and 60 are attempted even after defeat; every wave starts with the full deployed team. Surviving sampled waves award actual runtime bond, hero and trait growth. Unsampled waves award nothing. This makes late checkpoints tests of the same no-purchase roster, rather than claims about completing every intermediate wave.

Section 4 supplies bond targets, not hero/companion lists. `BalanceSimRecipes.cs` supplies explicit, reproducible fixtures, prioritizing the first major when targets cannot coexist. Requested/achieved bonds and every missing target appear in CSV and summary. These approximations must not be treated as measurements of impossible target combinations. Some named minors also cannot be activated alongside the requested majors. Extra memberships from Tower Rook and Planet Hamster use existing runtime rules.

CSV is long form: one row per damaging source, with wave totals repeated. Group by build/wave/seed before summing wave metrics. Damage uses the effective enemy-damage ledger, excluding overkill and damage to friends. Primary attacks, including hero attacks, are weapon damage; separately emitted hero rules are hero damage. Bond rule IDs are measurement metadata, leaving execute, proc and recursion behavior unchanged. Skill and bond amplification of another attack remains credited to that attack. Combined Stone/Spiky Shell reflection remains explicitly unresolved. Skill reflection names its protected recipient.

Bond steps describe the highest active threshold. They do not measure the marginal effect of unlocking a tier. Outlier screening groups all emitted rules of the same bond and compares direct damage share with twice the design's 5/12/25/50% targets. This is a damage proxy for broader team power. Skills have no specified share target: their >10% flags are labelled heuristic. No damage screen can assess healing, control, armor, economy or causal interactions by itself.

Falls and summed down seconds include the hero. Terminal fallen intervals end at the last simulated time. Enemy peak is sampled at step boundaries. Step mean/worst timings measure `CombatSimulation.Step` alone, excluding telemetry/export work; machine time is intentionally separate from deterministic outcomes. Victory time follows runtime survival/boss victory semantics and may leave enemies alive.

A wave is censored after 240 simulated seconds, covering ordinary duration plus boss overtime. A 540 s watchdog covers the batch simulation and output is written in `finally`; budget exhaustion throws after retaining partial measurements. Timeouts and missing trials never count as losses. The watchdog bounds work; it does not prove that all 792 trials finish under ten minutes on the target machine. Startup/import and editor shutdown lie outside the simulation watchdog.

Verification without launching Unity:

```powershell
python Tools/Verify-BalanceSim.py
```

This compiles every original runtime/editor source against installed Unity references and runs the existing managed rules adapter, including the new deterministic 100-enemy telemetry checks. The adapter replaces native allocation/destruction/logging and Euler conversion in disposable harness copies. It does not load the production asset catalog or produce balance measurements. Full matrix outcomes and batch runtime require Unity execution.

Verified on 2026-10-28: runtime/editor compilation; v3/v5 bond checks; mortality; 120 skills and 27 protections; 36 heroes and 16 traits; enemies; integration; wide regression; and BalanceSim checks all passed. BalanceSim checks cover valid unique ten-companion fixtures, cause semantics, 100-enemy overkill exclusion, repeated-seed damage determinism, falls, partial terminal down intervals, idempotent telemetry finalization and source-total reconciliation. Full Unity batch was not run, as project rules prohibit launching Unity in this task.
