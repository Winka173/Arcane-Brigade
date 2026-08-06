# Rules v3 verification — 28 October 2026

Run `python Tools/Verify-RulesV3.py` from the repository root. An optional argument selects another Unity Editor `Data` directory. The default uses the installed Unity 6000.4.8f1 references and .NET 9 SDK.

The script compiles **all runtime C#**, then compiles the seven task checks, `RulesV3AllChecks` and `BondReworkBuilder` against the runtime and Unity managed/editor assemblies. It executes `RulesV3AllChecks.Run()` under .NET, adapting only ScriptableObject allocation/identity, destruction and logging. Gameplay code, Unity vectors/math, fixtures and assertions are unchanged. Output and temporary assemblies stay under ignored `.utmp/rules-v3-lead/`.

| Check | Result | Coverage |
|---|---|---|
| Runtime Roslyn compilation | PASS | Complete runtime, including existing presentation callers |
| Task editor Roslyn compilation | PASS | Six checks and bond builder against installed Unity references |
| `BondEngineChecks.Run()` | PASS | Canonical 120-member roster, 20/32 bond counts and gates, twelve recipes, 100-enemy status/area/cadence/reset fixtures |
| `BondV5RulesChecks.Run()` | PASS | 100 enemies; Cogs, Springs, Glyphs, Inferno, pools/Leviathan, Beats/Orchestra, banked Chapters, generated-shot kills, Sniper carry and Clover persistence |
| `MortalityChecks.Run()` | PASS | 100 enemies, finite friend health, fall/rise/loss, exact 15 s/40%, class/star defenses, item armor sharing, formula floors, protection, faster revival and reset |
| `CompanionRulesV3Checks.Run()` | PASS | 120 skills, 27 protections, time-only fill, per-member haste, held/released Clockwork casts, crowd geometry, deterministic rolls, attack replays, healing and reset |
| `HeroRulesV3Checks.Run()` | PASS | 36 hero definitions, 144 unique relics, 16 traits, 100-enemy actions, coins versus passive income, choices, hero haste without faster effect clocks, and persistent growth |
| `EnemyRulesV3Checks.Run()` | PASS | All 53 kinds, 12 affixes, six bosses, warned geometry/timing, 100-enemy auras, egg cancellation, chains, choir, contagion, body/skill tiers, 80% quotas, 2/8 caps, elites, scaled fallback and armor breaking |
| `RulesV3IntegrationChecks.Run()` | PASS | 100 enemies in actual simulation, companion damage, fallen-body retention, revival, all-down defeat, restart, wave shields, initial hero requests, serialization callbacks, canonical stat caps, actual armor/shield damage, break/revive/reset, shared spawn limits, and next-shop Verse/Four-Leaf consumption |
| `git diff --check` | PASS | Whitespace/conflict-marker hygiene |

Unity was **not launched**. Unity asset import, native ScriptableObject lifetime/serialization, Editor execution, performance profiling and play/visual validation remain unverified. These fixtures validate representative rules and integration; they are not an exhaustive test of every skill/relic combination. Draft interpretations and presentation contracts are in [Rules v3 notes](Rules_v3_Notes.md).

No art, scenes, UI, shaders or effects assets changed. Work remains uncommitted as requested.

Latest design-v5 managed output:

```text
runtime: compile PASS
editor: compile PASS
managed-checks: compile PASS
BondEngineChecks: PASS
BondV5RulesChecks: PASS
MortalityChecks: PASS
CompanionRulesV3Checks: PASS
HeroRulesV3Checks: PASS
EnemyRulesV3Checks: PASS
RulesV3IntegrationChecks: PASS
RulesV3AllChecks: PASS
```

New classes have Unity metadata. The managed harness compiles presentation callers but does not execute them. Unity remains unlaunched; native asset import, physics/play timing, profiling and visual output remain outside this verification. Changes remain uncommitted as requested.
