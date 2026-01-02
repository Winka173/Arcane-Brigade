# Arcane Brigade — MVP Implementation Plan

**Goal:** Build a playable vertical slice that proves the full core loop (move → fight wave → earn gold → shop blessings/relics/allies → next wave → boss) in one arena, with enough variety to feel like the design doc.

**Tech base:** Unity 2D URP (existing project), New Input System (already in package manifest), 2D Sprite primitives + simple Sprite Editor-drawn shapes. Keyboard + mouse.

---

## MVP Content Scope

### Champion (1)
- **Aldric (Generalist)** — 45 HP / 30 ATK / 15 Speed
  - Default weapon: **Runeblade** (Blade — 120 DMG, 3/sec, cleave cone)
  - Passive: +3% crit chance (works with both melee and ranged)
  - Can switch to ranged **Repeating Crossbow** if a ranged blessing is equipped (toggle later — flagged P1)
  - **Control:** Player moves with WASD. Champion auto-attacks closest in-range enemy (same AI loop as allies). No manual attack input. Facing rotates toward current target.

### Allies (4 — one from each ally role)
1. **Sword Squire** (Vanguard / Blade) — melee cleave
2. **Ballista Crew** (Vanguard / Catapult) — ranged AOE explosion
3. **Shield Bearer** (Bulwark / Shield) — melee defense, 50% block, area damage reduction
4. **Light Cleric** (Mystic / Healing Staff) — ranged heal projectiles

Each ally: Tier 1 only for MVP. Merge / Tier 2/3 deferred.

### Enemies (4)
1. **Bone Shield Guardian** — melee tank (50 DMG, low threat, 100 HP)
2. **Frost Ogre** — melee knockback hammer (100 DMG, 150 HP)
3. **Volley Archer** — ranged rapid-fire (30 DMG × 5/sec, 95 HP)
4. **Imp Swarm** — ranged spread, 5 small imps (60 DMG each, 30 HP each)

### Boss (1 — mini-boss for MVP)
- **Frost Ogre Elite** variant (3× HP, 1.5× damage, telegraphed slam attack). Full bosses (Dread Lich, Inferno Dragon, etc.) deferred.

### Arena (1)
- **The Throne Room** — flat rectangular play area with simple wall colliders. No environmental hazards in MVP.

### Shop / Economy
- Gold earned: 5g per kill, +20g per wave clear
- Shop opens between waves. 3 random offers per slot:
  - 2 ally slots
  - 2 blessing slots
  - 2 relic slots
- Refresh: 5g per refresh (escalating later)

### Blessings (6 starter)
- **Sharpened Edge** (Battle) — +20% melee damage
- **Steady Aim** (Battle) — +20% ranged damage
- **Iron Ward** (Ward) — +30 max HP
- **Swift Step** (Ward) — +20% move speed
- **Empowered Allies** (Mystic) — allies +15% damage
- **Frostbite Curse** (Curse) — enemies move 15% slower

### Relics (6 starter)
- **Iron Amulet** — +5 ATK
- **Leather Bracers** — +10 max HP
- **Swift Boots** — +3 move speed
- **Apprentice Tome** — +1 mana per blessing equipped (placeholder — mana hooked up later)
- **Crit Charm** (Uncommon) — +5% crit chance
- **Bloodthirsty Fang** (Rare) — 10% lifesteal

### Wave Structure (MVP run)
- Wave 1: 6 enemies (mostly Bone Shields and Volley Archers)
- *Shop*
- Wave 2: 10 enemies, introduce Imp Swarm + Frost Ogre
- *Shop*
- Wave 3: 14 enemies, mixed
- *Shop*
- Wave 4 (Boss): Frost Ogre Elite + 4 supporting enemies

---

## Technical Architecture

### Folder layout
```
Assets/ArcaneBrigade/
  Scripts/
    Core/         (GameManager, WaveManager, ShopManager, EconomyManager, GameState)
    Combat/       (Health, DamageDealer, Weapon, Projectile, HitFlash)
    Entities/    (ChampionController, AllyController, EnemyController, base Unit)
    AI/          (UnitTargeting, MeleeAttackBehaviour, RangedAttackBehaviour, SupportBehaviour)
    Data/        (ScriptableObject definitions)
    UI/          (HUD, ShopUI, HealthBar, GameOverScreen)
    Input/       (InputReader wrapping the Input System)
    Util/        (object pools, sprite factory)
  Data/
    Champions/    (.asset)
    Allies/
    Enemies/
    Weapons/
    Blessings/
    Relics/
    Waves/
  Prefabs/
    Champion/
    Allies/
    Enemies/
    Projectiles/
    UI/
  Sprites/        (Sprite assets created via Sprite Editor)
  Scenes/
    Boot.unity     (loads Game)
    Game.unity     (the Throne Room arena)
```

### Core ScriptableObjects
- `WeaponDefinition` — type (Blade/Blunt/Piercing/Claws/Shield/Crossbow/Catapult/Staff/Beam/Mirror), damage, attack rate, range, projectile prefab, effects list
- `ChampionDefinition` — base stats, default weapon, passive id, sprite color
- `AllyDefinition` — faction, weapon, base stats, sprite color/shape
- `EnemyDefinition` — base stats, weapon, behaviour tag, sprite color/shape, weakness
- `BlessingDefinition` — school, stat modifiers, special effect id
- `RelicDefinition` — rarity, cost, stat modifiers
- `WaveDefinition` — list of spawn entries (enemy type × count × delay)

### Sprite plan (primitives + sprite editor drawing)
- Champion: blue circle (body) + small square (weapon) attached via child transform
- Allies: color-coded triangles/diamonds, with weapon sub-sprite
- Enemies: red shapes; tank = hexagon, ranged = triangle, swarm = small circles
- Projectiles: tiny tinted shapes
- Weapon trails / impact: Unity Trail Renderer + Particle System with primitives
- Where the design doc calls for distinct silhouettes (ogre, archer, etc.), I'll combine 2-3 primitives in a parent prefab so each enemy is visually recognizable at a glance

### Combat loop
- Discrete fixed-update tick for AI and cooldowns
- Damage events go through a single `DamageDealer.Apply(source, target, weaponDef, modifiers)` so blessings/relics/passives can hook in
- Projectiles are simple Rigidbody2D-driven prefabs with `Projectile` component, pooled

### Input
- `PlayerControls.inputactions` asset with:
  - Move (WASD / arrows)
  - Aim (mouse delta toward cursor)
  - Attack (LMB)
  - Ultimate (Space) — stubbed in MVP
  - Interact (E) — for shop
  - Refresh shop (R) when in shop

---

## Execution order (chapters)

1. **Project skeleton** — folders, asmdef, input asset, boot scene wiring
2. **Sprite factory + Champion movement** — Aldric moves, camera follows
3. **Health / damage pipeline** — HitFlash, death, gold drop
4. **Melee weapon (Runeblade)** — Aldric attacks, cleave cone, cooldown
5. **First enemy + melee AI** — Bone Shield Guardian walks at player, swings
6. **Wave manager + spawn system** — spawns wave 1, detects clear
7. **Ranged weapon + projectile pool** — Volley Archer shoots
8. **Remaining enemies** — Frost Ogre (knockback), Imp Swarm
9. **Ally system + 4 allies** — Sword Squire, Ballista Crew, Shield Bearer, Light Cleric
10. **Economy + Shop UI** — gold, buy/refresh, blessing/relic/ally cards
11. **Blessings + Relics applied to stats/effects** — 6 of each
12. **Boss wave + ultimate stub**
13. **HUD polish** — HP bar, gold counter, wave counter, kill counter
14. **Game over / restart loop**

Each chapter ends with a "playable to here" checkpoint so we always have a runnable build.

---

## Out of scope for MVP (deferred but designed-for)
- 17 other champions, weapon switching at runtime
- Tier 2/3 ally merging
- Remaining enemies, all 4 full bosses, elite variants
- 5 other arenas + environmental hazards
- Status effects beyond knockback/slow (no bleed/burn/freeze chains yet — hook in V2)
- Elemental reaction matrix
- Faction bond bonuses (data modeled but not applied)
- Story/Endless/Daily/Boss Rush/Trial/Co-op modes — MVP is just "play one run"
- Mastery / Account Level / Prestige / Achievements
- Save system, leaderboards
- Audio (sound effects deferred — silent MVP unless quick to add)
