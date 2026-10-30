# Bond engine integration

`CombatSimulation.Bonds` owns wave state. `RunSession.PrepareCombat()` snapshots catalog definitions and `BondCounts`; rules use member counts, never stat values or bond levels. Authored `Steps.Count` gates later steps. Current catalog majors put step 2 in `Threshold`, and minors have empty `Steps`; those use their review-pack 2/4(/6) gates until explicit steps are authored. IDs follow the catalog (`static-club` = Storm, `lantern-guild` = Lantern; singular `mask`, `horn`, `gear`, `rune`).

Public events on `BondRuntime`:

| Event | Payload and consumer |
|---|---|
| `RuleTriggered` | `BondRuleEvent`: rule key, position, subject ID, value. Effects can react to `burn-stack`, `chill-stack`, `poison-stack`, `poison-spread`, `rot`, `charge`, `root-stillness`, `lantern-stillness` (zero on movement), `burn-burst`, `freeze`, `frozen-hit`, `shatter`, `inferno`, `blizzard`, `storm-chain`, `discharge`, `thunderclap`, `storm-cloud`, `dusk-mark`, `eclipse`, `mask-double`, `combo`, `grit`, `horn`, `rune`, `gear`, `sea-shanty`, `cast`. |
| `SpawnRequested` | `BondSpawnRequest`; arena creates an ally or pickup. |
| `DamageRequested` | `BondDamageRequest`; combat already handles this through its damage/death/loot path. Effects must not apply damage again. `Rule` also identifies `burn`, `poison`, `execute` and `eclipse`. |
| `StrengthStolen` | Enemy, strongest weapon-damage companion, stolen fraction. Gameplay already weakens the enemy and buffs that companion. |
| `BonusShotRequested` | Attacker and weapon damage; combat already creates exactly one projectile. |
| `EmbersCreated` | Position and lifetime; combat already tests crossings. Radius is in `BondTuning`. |

Spawn kinds are `Phoenix`, `IceSoldier`, `Shade`, `LampWisp`, `Snack`, `Clover`. Requests include position, source ID, lifetime (0 = wave), damage, heal fraction, coin count and free rerolls. Snack healing/coins and Clover rerolls apply **on collection**, not when requested. No GameObjects are created by the engine.

Attach consumers after `Start`, drain the buffered `SpawnRequests`, call `ClearSpawnRequests()`, then handle future requests through either the buffer or event (consume each once). Rebind when combat starts again. Pending requests and all statuses/counters reset each wave; Winter stacks and Rank are recomputed. `State(enemy)` exposes stacks, mark, freeze/stun deadlines and DoT sources. Public bonuses are consumed by combat for stillness, regeneration, armor, crit/luck, speed and ordinary weapon reach. `OnSoak`/`OnRecord` are notification hooks for the Tide/Archive systems; `OnHeal` and `OnVolley` update their counters. Applause counts distinct companions casting within five seconds.

Draft choices, all tunable in `BondTuning`: burn/poison last 4 s and deal 10% weapon damage per stack per second; burst leaves 1 Burn stack and spreads without recursive bursts; common burst/spread radius is 2. Ice Soldiers cap at 3 per wave and shatter deals 100% weapon damage. Poison hits cap at 5, but spread can reach 20, allowing Rot at 8. Shades last 12 s; one lamp wisp lasts the wave. Cloud damage is 100%; thunderclap lasts 1 s. Rune ring radii are 0.5–2; Horns shove 1 unit. Combo/Grit bonuses affect subsequent hits; Sea Shanty refreshes without stacking. Eclipse rounds its target count up and starts at 8 s. Marks steal 12% of the greater of enemy weapon/contact damage for the wave. Inferno triggers once per wave at 100 simultaneously burning enemies. Existing catalog stat packages remain in the session stat path.

Run Unity checks with `-executeMethod ArcaneBrigade.Editor.BondEngineChecks.Run`. See [verification evidence](Bond_Engine_Verification.md).
