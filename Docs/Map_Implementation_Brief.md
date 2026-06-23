# Brief for the map implementers (25/10/2026)

**Design:** `Docs/Map_Mechanics_2026-10-25.md` is the approved spec. Follow its numbers exactly.

## Files you may touch
Only your maps' files:
- `Assets/ArcaneBrigade/Runtime/Core/Maps/Map<Theme>.cs`: combat and rules. Partial `MapEnvironmentState` fields, partial `MapEnvironmentRules` constants, and `CombatSimulation` hooks:
  - `Init<Theme>`
  - `Step<Theme>`
  - `<Theme>StandingBoost`
  - `<Theme>EnemyFactor`
  - `<Theme>AllyCycle`
  - `<Theme>HeroPace`
  - `<Theme>Coins`
  - `<Theme>CaptureFixture`
- `Assets/ArcaneBrigade/Runtime/Presentation/Maps/Map<Theme>Visual.cs`: `Draw<Theme>`, `<Theme>PropLight` on `MapEnvironmentVisual`, and `Air<Theme>` on `MapAtmosphere`.
- New files of your own: extra partial files named after your map, and shaders under `Assets/ArcaneBrigade/Resources/Shaders/Map<Theme>*.shader`. Load shaders with `Resources.Load<Shader>("Shaders/...")` and fall back to drawing without them when `!isSupported`.

Do **not** edit:
- central files (`MapEnvironmentCombat.cs`, `MapBoosts.cs`, `MapEnvironmentVisual.cs`, `MapAtmosphere.cs`, `CombatSimulation.cs`, `MapIdentity.cs`);
- another map's files.

If you truly need a new central hook, describe it in your report instead.

## Framework to use (`Core/MapBoosts.cs`)
- **`GrantMapBoost(MapStat stat, float value, float seconds, string source, string prefix = null)`**
  - Timed base-stat boosts: Damage share, AttackSpeed share, Armor points, Gold a kill, MoveSpeed share.
  - Shows a line over the hero's head automatically (stat and amount, no seconds).
  - The same source again keeps the larger value and adds time, up to 20 s ahead.
- **`MapSay(string text, Color colour)`:** any other line over the hero's head, such as "FULL DELIVERY!" or "IN THE LIGHT".
- **`<Theme>StandingBoost(MapStat)`:** stats lent only while standing somewhere (a light, a spotlight, a flower bed).
- **`<Theme>EnemyFactor(UnitState, MapEnemyAxis)`:** per-enemy factors, where 1 means unchanged.
  - `Speed`: move pace.
  - `Cycle`: attack cooldown factor, above 1 is slower (20% slower attacks = 1.25).
  - `DamageDealt`: damage the enemy deals.
  - `DamageTaken`: damage the enemy takes.
  - `Range`: target-finding reach.
  - `Knockback`: how far our blows push it.
- **Rule on overhead text:** every boost needs a line over the hero. Enemy buffs and map moments should get a short readable cue too (`MapSay` for the hero's own gains; world-space ink or a small label at the enemy for enemy states).

## Look (very important)
The owner wants map effects to be **extremely beautiful**.
- Draw effects in code with the existing ink system:
  - `ArsenalInk`: `FloorZone`, `FloorLane`, `Cone`, `FloorStroke`, `Stroke`, `TokenAt`, `Light`, `Glow`;
  - `InkCanvas`: `Stroke`, `GlowRibbon`, `Sector`, `Ellipse`, `Band`.
- Use shaders when they truly help: fog, rain, sun shafts, smoke, ink pools, dust, glints.
- Areas are **solid fills of one colour with no outline or rim**.
- Our side never draws circles.
- One light direction for shadows.
- Use `GroundShadow` with `PinCastToFoot = true` for props standing on the floor.
- Animate everything; Reduced Motion and Reduced Effects should calm it.
- Danger to the hero is red; friendly things are amber, cream, brass or teal; violet means enemies.

## Art
Painted.
- **Props:** `Resources/Environment/<Folder>/<Name>.png`, loaded through `MapIdentity.Sprite("<Folder>/<Name>")` (`Put(...)` in `MapEnvironmentVisual`).
- **Expected names:**
  - WaitingRoom: `Clock_Wall`, `Ticket_Red`, `Ticket_Yellow`, `Ticket_Grey`, `Ticket_White`, `Ticket_Blue` (existing: `Board_*`, `Door_*`).
  - PostOffice: `Paper_Plane` (existing: `Chute`, `Letter_*`, `Parcel_*`, `Scrap_*`, `Stamp`).
  - Glasshouse: `Pot_Bud`, `Bloom_Red`, `Bloom_Yellow`, `Bloom_Brown`, `Bloom_Orange`, `Bloom_Blue`, `Bud_Big`, `Flower_Turret`.
  - LampHall: `Lantern_Carry`, `Oil_Vial` (existing: `Lamp_*`, `LightGlow`, `Hand_*`).
  - Theatre: `Firework_Machine`, `Curtain_Velvet`, `Seats_Row`.
  - ArchiveVault: `Page_A`, `Page_B`, `Inkpot_Up`, `Inkpot_Tipped`, `Stamp_Giant` (existing: `Clock_*`).
- These may not be imported yet when you start. Always handle a missing sprite: draw a tasteful code fallback, or skip the prop.
- Cracks on a door, planes' trails, glass shards, rain, smoke, sun shafts and spotlights are code, not art.

## Rules of the game to respect
- Friends are immortal: no heals, armour or dodge for friends. Armour boosts go to the hero only.
- Bosses: stuns are shortened as the spec says, and "% of max health" hits are capped as written (bosses 5%).
- Nothing on a map blocks or pushes the hero (no sliding furniture).
- Damage to enemies from map elements should go through the simulation's normal damage path (look at how existing map elements or relic bursts deal damage, e.g. the parcel burst) so that numbers and kills credit correctly.
- Overhead text: the hero's gains via `GrantMapBoost` / `MapSay`.

## Style and process
- Match the surrounding code.
- Comments state the requirement ("// requirement 25/10/2026: ..."), never "owner".
- Do NOT build, run Unity, commit or push. Another session compiles and verifies.
- **Report concisely:**
  - what you implemented per slot with the numbers;
  - files touched;
  - any central hook you needed;
  - guide tip lines for MapIdentity (one per element, short) for the integrator to paste.
