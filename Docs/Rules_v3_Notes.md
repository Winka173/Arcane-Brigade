# Rules v3 integration and presentation handoff

The rules source is [Complete design v3, sections 1–9](Complete_Design_Bonds_Skills_Heroes_Enemies_2026-10-28.md). Membership is compiled from `DesignInput/roster120.json`. No scenes, sprites, shaders, particles, UI or GameObjects are created by these packages.

`CombatSimulation` exposes `Bonds`, `Mortality`, `SkillsV3`, `HeroRulesV3`, `TraitsV3` and `EnemyRulesV3`. Simulation adapters apply gameplay requests once through the combat damage/death/loot path or the bounded friend roster. Presentation must observe these events without applying damage, shields, healing, currency or spawning a second gameplay actor.

Subscribe to simulation-level events **before** `Start()`. Package runtime objects are replaced by `Start()`; rebind their events after each start. Initial bond support/spawn buffers are consumed by the simulation, and cleared regularly. Do not drain them a second time. `TryGetRulesV3Entity(index, out request)` enumerates active abstract entities across `RulesV3Tuning.ConstructCapacity` slots for consumers attached after start. Pause/rest does not advance combat skill bars or revival clocks.

## Bonds

There are 20 majors with nine roster members each and gates 2/4/6/8, and 32 minors with five or seven members and their authored gates. `BondV3Catalog.Configure` supplies canonical definitions and tags. `BondReworkBuilder.Run` writes existing companion memberships and bond definitions without generating art. The runtime catalog adapter also supplies the 24 missing companion definitions in memory. `BondBuildRecipes.All` exposes the twelve recipes from section 4.

Canonical major stat rewards stop at step 8, retain their full authored values, and double Hearth's package at eight. Fractional health rewards compile as health correction. The hero contributes its two minor memberships; Planet Hamster and Tower Rook contribute their extra trait memberships. Existing excess species remain stored, with at most ten distinct companions deployed.

| Event | Payload | What presentation draws |
|---|---|---|
| `BondRuntime.RuleTriggered` | `BondRuleEvent`: rule key, position, subject ID, value | Stacks, marks, freezes, bursts, chains, stillness, weather, counters, pulses, beats and transformations. Interpret value using the rule key and `BondTuning`; it may be a stack count, multiplier or duration. |
| `BondRuntime.SpawnRequested` | `BondSpawnRequest`: kind, position, source ID, lifetime, damage, heal fraction, coins, free rerolls | Phoenix, Ice Soldier, Shade, Lamp Wisp, Chef, Leviathan, Wind Beast, Sprout, Treant, Colossus, Turret, Spirit, Orchestra, Snack or Clover. These are abstract simulation entities, not additional presentation-owned combatants. |
| `BondRuntime.DamageRequested` | `BondDamageRequest`: covered target and attacker, damage, rule | Optional hit confirmation for an already-resolved area/chain. The accompanying rule event describes the overall action. |
| `BondRuntime.SupportRequested` | `BondSupportRequest`: kind, subject, position, amount, duration, radius, rule | Shields, healing, sharing, immunity, revival, armor, walls, copies, economy, choice/law/crown, dash, piercing volley, pull and time stop. Zero-duration wave shields last the wave. Heal/shield/revive amounts are max-health fractions; shared damage is absolute. |
| `CombatSimulation.RulesV3SupportApplied` | Same `BondSupportRequest` | Subscription point available before start, including initial wave support. |
| `BondRuntime.StrengthStolen` | Enemy, benefiting companion, fraction | Strength leaving a marked enemy and reaching a friend. |
| `BondRuntime.BonusShotRequested` | Attacker, damage | Gear's bonus projectile; the simulation creates its projectile state. |
| `BondRuntime.EmbersCreated` | Position, lifetime | Embers; use `BondTuning.BurstRadius` for their radius. Crossings are already tested by rules. |

Choices remain rules APIs: `ChooseEdict`, `ChooseLaw`, `ChooseConstellation`, `ChooseTreeFruit`, `CashOut`. `SetRulesV3WindBeastMode` selects guard/shoot/sweep, and `UpgradeRulesV3Turret` upgrades an abstract turret. Choice panels and dragging are presentation work.

## Mortal friends

Hero and companions have finite health. Fallen bodies stay in the unit lookup, cannot act, and rise after 15 seconds at 40% health. Starting a wave restores the deployed team to full health. The hero uses the same revival rule; defeat occurs when all deployed friends are down together. Spirit and protection skills can shorten or prevent a fall. An unanswered boss final countdown explicitly defeats the roster.

| Event | Payload | What presentation draws |
|---|---|---|
| `MortalityRuntime.Fell` | `FriendMortalityEvent`: friend/source IDs, position, time, health, max health, rise deadline, hero flag | Fallen pose and revival countdown. |
| `MortalityRuntime.Rose` | Same `FriendMortalityEvent` | Standing up and restored health. |
| `MortalityRuntime.WaveLost` | `FriendWaveLostEvent`: time, fallen count, last friend ID | Wave defeat transition. |

Enemy contact and projectiles can hit companions. Crowd contact protection is tracked per friend, so one friend's contact does not consume another's budget. Shields, reduction, sharing, reflection and revival apply to the actual combat bodies.

## Companion skills

`CompanionSkillTable.All` contains all 120 authored descriptions, mechanic kinds, body origins and 27 protection flags. `CompanionSkillTuning.Profiles` contains their values. Grouped handlers use discs, rings, lines, cones, species and a fixed cluster grid. Ordinary attacks, hits and kills never charge bars. Explicit free copies/refunds from the design remain separate from time-based fill.

| Event | Payload | What presentation draws |
|---|---|---|
| `CompanionSkillRuntime.CastRequested` | `SkillCastRequest`: source ID, skill ID, shape, position/end, radius, duration, warning, strength | The whole cast and its warning geometry. |
| `CompanionSkillRuntime.DamageRequested` | `SkillDamageRequest`: attacker/covered target, skill ID, damage | Already-applied damage confirmation. This is per covered enemy, not a target-selection instruction. |
| `CompanionSkillRuntime.EnemyEffectRequested` | `SkillEnemyEffectRequest`: source/target, skill ID, effect flags, origin/direction, duration, amounts, stack cap | Burn, chill, poison, wet, slow, freeze, stun, sleep, silence, weaken, blind, root, pull/push/fear, marks, lit, taunt, charge and healing/attack debuffs. |
| `CompanionSkillRuntime.FriendRequested` | `SkillFriendRequest`: source/target, skill ID, kind, value, duration | Protection, timed healing, sharing, revive, cleanse, reflection, buffs, swaps, safety pulls and bar/economy changes. Swaps have already moved both bodies. |
| `CompanionSkillRuntime.ConstructRequested` | `SkillConstructRequest`: source/target IDs, skill ID, kind, position/end, radius, duration, value, count, body/bullet blocking flags | Walls, transforms, summons, echoes, coins, cherries, Snacks, taunt skulls, healing pumpkins and rebirth feathers. Skill summon/transform damage is already scheduled by the skill runtime. |

Confirmed skill kills produce the relevant pickups. Feathers revive on collection; Phoenix's trait raises that revival to 60% health and grants its damage buff. Mirror Newt's six reflected hits form a shared team budget. Walls intercept swept hostile projectiles; body walls also stop movement. Grand Sprout's root, timed healing, attack history replays and source-following orbit/transform geometry are rules state.

## Personal traits and heroes

`HeroRulesV3Catalog` contains 16 traits, 36 heroes, three build directions per hero and four hero-only relics each. The adapter supplies the 144 relic entries to the rules catalog. The new hero runtime replaces legacy hero activation paths, including the retained first ten mechanics. Relic inventory, purchases, species, waves, coals, seed ages and choices are run state. Hash sets/dictionaries have serialization surrogates.

| Event | Payload | What presentation draws |
|---|---|---|
| `HeroRulesV3Runtime.Requested` | `HeroRuleRequest`: action, shape, rule, source/friend IDs, position/end, radius, duration, value, count, species, options | Hero area/line attacks, wards, marks, healing/shields, movement/control, bullet handling, summons, walls, rain, fleet, juggling, copies, revival, bar refill, economy and choices. |
| `PersonalTraitV3Runtime.Requested` | Same `HeroRuleRequest` | Persistent coals, wind lanes, seed pods/fruit, stacked skill speed, copied pages, spotlight counters, carousel rings, sweeping beams, tolls, feather buffs and bullet gravity. |
| `CombatSimulation.RulesV3HeroApplied` | Same `HeroRuleRequest` | Subscription point available before start, including initial wards/choices and restored persistent traits. |

`HeroRuleOptions` describes critical casts, returning missed parcels, split reflections, copied global heals/longer shields, wind-following fleets, extra burn, juggling effects and blind. Copies are guarded against recursive cast-trigger loops. `HeroRulesV3.Trim` exposes the once-per-wave pruning action. `ChooseRulesV3Decree` exposes session choices; `SellRulesV3Friend` exposes Pawn Ticket's receipt-based refund without adding UI.

## Enemies, affixes and bosses

`EnemyRulesV3Catalog` supplies 53 enemy kinds, 12 affixes and six phased bosses, including rules-only definitions when art is absent. `BossForNight` follows the exact authored nights. Spawned crumbs arrive in packs of 30–60, subject to the wave budget. Recognized enemies bypass legacy movement and variant handlers. Corpses needed by revival and simultaneous boss mechanics remain available during the wave.

| Event | Payload | What presentation draws |
|---|---|---|
| `EnemyRulesV3Runtime.DangerRequested` | `EnemyDangerRequest`: request/source IDs, shape, action, rule, position/end, radius, warning time, duration, value | Solid filled circle, line, cone, ring or arena warning. Gameplay resolves the immutable warned geometry after the warning. |
| `EnemyRulesV3Runtime.Requested` | `EnemyRuleRequest`: action, source/target IDs, count/group, rule/species, shape, position/end, radius, duration, value | Resolved attacks/statuses, silence/time zones, dark/charm/swallow, currency/pickup drains, group buffs, banners, leech/binder chains, spawn/revive/despawn, boss phases and countdowns. |

Shield/heal representation requests emitted by the enemy runtime do not require a second gameplay application. Linked leech/binder damage uses the ordinary damage/death path. Enemy auras cache per-hit modifiers; warnings and persistent actions use fixed pools. Friends are a bounded roster for target selection and contagion.

Egg and Warden-chain objectives have `StrikeRulesV3Objective(id)`, with ordinary skill geometry, melee arcs and projectile sweeps also testing them. The Warden chain ID is negative source ID; an egg uses its warning request ID. The enemy runtime also exposes `CancelEgg` and `CutChain` for arena input adapters. Objective hit testing requires no scene objects.

## Abstract world entities

`CombatSimulation.RulesV3EntityChanged` carries `RulesV3EntityRequest`: entity/source/target IDs, kind, Created/Collected/Expired change, rule, position/end, radius, remaining duration and value. It covers allied bond entities, Snacks, Clover, feathers, pages, pumpkins, skulls, banners, seeds and wind lanes. Render each identity once; the simulation owns collection, currency, healing, revival, area attacks and lifetime. Runtime arrays have fixed capacities in `RulesV3Tuning`.

## Draft choices and review limits

- The roster has some four/five-tag companions, although section 1 says two/three. Its explicit membership lists are retained. Legacy house indices in the input are not used as bond identities.
- The document leaves many damage amounts, radii, summon attack cadences, favored weather, law choices, jackpot/fruit amounts and minor interaction details unspecified. They are explicit tuning/default choices, not additional approved balance requirements. Weather preference uses a stable ID-derived default; Court exposes damage/armor/haste edicts and crit/kill/cast → heal/shield/burst laws.
- Hero mortality is interpreted as the same revival rule as other friends, because the task makes the hero mortal and says all friends down loses. Section 10 asks the question, but the task explicitly authorizes this loss rule.
- Physical dash, chain, orbit, fleet, juggling, summon and transformation skills use deterministic geometry and pooled pulses rather than new independent physics actors. This is gameplay behavior, not a promise that a later visual trajectory changes hit timing. Bespoke motion/ricochet tuning should be reviewed in Unity with the effects implementation.
- Several hero relic descriptions assume systems that have no precise v3 definition: chest tiers, turret relic slots, absorbed trait combinations and selecting borrowed skills. The current adapters expose their rule state and use draft tuning where an existing system permits a concrete effect. These interactions need design review; a catalog description alone should not be treated as evidence that every possible relic combination has been play-tested.
- No Unity import, Editor execution, play session or visual validation was performed. See [verification](Rules_v3_Verification.md) for the managed compilation and deterministic checks that did run.

Changes are intentionally uncommitted on `feat/bond-engine` for review.
