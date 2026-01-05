# ARCANE BRIGADE - COMPLETE GAME DESIGN DOCUMENT

> **Gameplay update — 2026-10-13:** This document is retained as the original content/vision reference. The owner's confirmed 2D, movement-only, auto-combat rules take precedence. See [Production_Plan_2D.md](Production_Plan_2D.md) for the revised production direction, superseded mechanics, and reference-verification requirements. The original "Production-Ready" label below does not certify the revised design or an implemented game.

**Version:** 4.0 - Expanded Design Edition
**Status:** Production-Ready with Base Game Analysis
**Genre:** High-Fantasy Magic Roguelike Arcade
**Reference:** Bizarre Brigade (Zenless Zone Zero mini-game)

> **Reference audit correction — 2026-10-13:** The owner selected the expanded **1.5+** version. The historical base-game analysis below is not a verified parity spec: duration/wave counts, universal merge behavior and some economic rules need correction or version-specific evidence. Use `Bizarre_Brigade_15_Reference_Audit.md` and `Reference_Parity_Backlog.md`. Latest camera direction is a bounded arena larger than the viewport, not a fully visible fixed-screen arena. No permanent candy-stat grind, upkeep or manual combat skill is reintroduced.
**Tagline:** *Forge your Brigade. Master the arcane. Hold the line against the Scourge.*

---

## TABLE OF CONTENTS

### Core Systems
1. Base Game Analysis (Bizarre Brigade)
2. Core Weapon System Architecture
3. Melee Weapon Categories & Mechanics
4. Ranged Weapon Categories & Mechanics

### Roster
5. Champion Weapons (18 Heroes)
6. Ally (Vassal) Weapons - Vanguard / Bulwark / Mystics
7. Enemy (Scourge) Weapons

### Combat Systems
8. Visual Identification & Animation
9. Weapon Scaling & Progression
10. Combat Flow & Mechanics
11. Implementation Details

### NEW: Expanded Content
12. Arcane Relics System (30 Named Relics)
13. Blessings & Spells System (28 Blessings)
14. Status Effects & Elemental Reactions
15. Champion Ultimate Abilities (18 Ultimates)
16. The Specialists - Fourth Ally Faction (4 New Allies)
17. Elite Enemies & Multi-Boss Roster
18. Arenas & Environmental Hazards
19. Game Modes & Long-Term Progression
20. Synergy & Faction Bond System
21. Wave Events & Run Encounters
22. Design Philosophy & Summary

---

# SECTION 1: BASE GAME ANALYSIS (BIZARRE BRIGADE)

## Bizarre Brigade Overview

**Game Type:** Roguelike arcade mini-game at Godfinger Arcade in Zenless Zone Zero
**Core Loop:** Survive waves → Kill enemies → Earn coins → Buy companions/upgrades → Rest → Next wave
**Duration:** 20 minutes per run (8-15 waves depending on difficulty)
**Difficulty Tiers:** 5 main difficulties, Shared Dream (2-player co-op)

---

## Bizarre Brigade Companion System

### What Are Companions?

In Bizarre Brigade, companions are NPC allies that fight alongside your Dream Seeker (player character). They:
- **Do not take damage** (cannot die)
- **Attack automatically** (no player control)
- **Have randomized weapons** (different each run)
- **Can be upgraded** (3 copies merge into stronger version)
- **Cost income** (5 coins per companion, occupies slot)
- **Deal damage** (melee attacks, ranged attacks)

### How Companions Fight

**Key Mechanics:**
1. **Weapon Types:** Each companion comes with random melee or ranged weapon
2. **Visual Clarity:** Melee companions visibly walk up to enemies with swords/hammers; Ranged companions stand back and shoot
3. **Bond System:** Companions are grouped by "Bond" (faction/type); matching bonds grant stat bonuses
4. **No Damage:** Companions never die, just keep fighting
5. **Stacking:** 3 identical copies of a companion merge into upgraded version
6. **Team Composition:** Players strategically pick melee, ranged, and support companions

### Companion Attack Behavior

**Melee Companions (with swords, hammers, etc.):**
- Walk toward enemies
- Physically attack when in range (1-2 unit distance)
- Deal consistent per-swing damage
- Create visual impact (visible strikes)
- Cannot attack from distance
- Must be positioned close to enemies

**Ranged Companions (with guns, staffs, etc.):**
- Stay at distance from enemies
- Fire projectiles toward enemies
- Deal damage per projectile
- Projectiles visible traveling through air
- Can attack from 5+ units away
- Don't need to move as much to attack

### Damage Types in Bizarre Brigade

Companions deal damage through 3 main damage types:
1. **Melee Damage** - Physical weapon strikes
2. **Ranged Damage** - Projectile-based attacks
3. **Collision Damage** - When running into enemies (Clementine special)

---

## Bizarre Brigade Dream Seekers (Player Characters)

### Player Character Roles

**Damage Dealers:**
- High offensive capabilities
- Actively kill enemies
- Build around boosting damage

**Utility/Economy:**
- Collect resources efficiently
- Support companions
- Manage shop economy

**Tank/Balanced:**
- Survive longer
- Balanced stats
- Good for learning

### Player Attack Mechanics

Players in Bizarre Brigade:
- **Actively participate** in combat (unlike companions who auto-fight)
- **Control movement** directly
- **Can damage enemies** through attacks or collision
- **Must avoid enemy attacks** (take damage and die if health reaches 0)
- **Earn income** from kills and wave completion

---

## Bizarre Brigade Items/Relics System

### What Are Relics?

Relics are purchasable upgrades that provide stat bonuses or passive effects:
- **Stat Bonuses:** +HP, +Speed, +Attack
- **Passive Effects:** Abilities, bonuses on conditions
- **Rarity Tiers:** Common → Uncommon → Rare → Legendary
- **Purchased from Shop:** Appear after waves, cost coins
- **Purchased via Refresh:** Can reroll shop items for increasing cost

### How Relics Work

- Each relic is a one-time purchase per run
- Takes effect immediately
- Stacks with other relics
- Scales with game progression
- Some relics synergize with companions (create meta-strategies)

---

## Bizarre Brigade Bond System

### What Are Bonds?

Companions are grouped by "Bond" - a trait/faction that connects multiple companions:
- Each companion belongs to ONE bond
- Collecting multiple companions from same bond grants bonus
- Example: "Warriors" bond → 2x Warriors grant +5% ATK, 3x grant +10% ATK

### Mechanical Impact

- **2x Same Bond:** Small stat bonus (+3-5%)
- **3x Same Bond:** Medium bonus (+8-12%) + special effect
- **Incentivizes Diversity:** Players choose between focusing on one bond vs spreading across multiple

---

## Key Insights for Arcane Brigade

**What Works in Bizarre Brigade:**
1. ✅ Companion system gives sense of "team building"
2. ✅ Melee vs Ranged creates clear visual distinction
3. ✅ Randomized weapons add RNG element
4. ✅ Set bonuses reward strategic choices
5. ✅ Simple but deep mechanics (easy to learn, hard to master)
6. ✅ Visual clarity (you instantly know what each companion does)

**What We're Improving in Arcane Brigade:**
1. 🎯 Allies have FIXED weapon types (no RNG on weapons)
2. 🎯 Champion has multiple weapon choices (not just one)
3. 🎯 Enemies clearly show weapon types visually
4. 🎯 More detailed damage types (Fire, Holy, Physical, Arcane)
5. 🎯 Endless mode scaling for weapons
6. 🎯 Blessing system affects weapon effectiveness

---

# SECTION 2: CORE WEAPON SYSTEM ARCHITECTURE

## Fundamental Design

Every combat-capable entity in Arcane Brigade has ONE primary weapon type:

**THREE ENTITY CATEGORIES:**
1. **Player (Champion)** - Flexible, can switch based on equipment
2. **Allies (Vassals)** - Fixed weapon type per ally
3. **Enemies (Scourge)** - Fixed weapon type per enemy type

**TWO WEAPON CATEGORIES:**
1. **MELEE** - Close range (1-2 units), physical weapons, swing animations
2. **RANGED** - Distance attacks (5-20 units), projectiles, aiming animations

---

## Design Philosophy

### Why Separate Melee/Ranged?

1. **Visual Clarity:** Player instantly knows how enemy/ally fights
2. **Strategic Depth:** Different positioning, tactics for each type
3. **Game Feel:** Melee feels powerful up-close, Ranged feels safe at distance
4. **Balance:** Can tune melee/ranged separately
5. **Role Definition:** Allies have clear roles (attacker, defender, support)

### Why Fixed Ally Weapons?

In Bizarre Brigade, companions have random weapons - why fix ours?

**Our Reasons:**
1. **Clarity:** Sword Squire ALWAYS has a sword, never a crossbow
2. **Identity:** Each ally feels unique and recognizable
3. **Synergy:** Can design blessings that specifically buff sword-users
4. **Balance:** No RNG on weapon stats (simpler economy)
5. **Visual Consistency:** Player learns what each ally looks like

### Why Champion Can Switch?

Unlike allies, the Champion needs flexibility:

1. **Build Variety:** Different blessings create different playstyles
2. **Adaptation:** Can counter enemy composition with weapon choice
3. **Depth:** Adding melee/ranged toggle creates strategic layer
4. **Replayability:** Same character can play differently each run

---

# SECTION 3: MELEE WEAPON CATEGORIES & MECHANICS

## Melee Weapon Framework

All melee weapons share these characteristics:
- **Range:** 1-2 unit distance (close combat)
- **Animation:** Swing, slash, or stab motion
- **Damage:** Direct per-hit damage (no falloff)
- **Visual:** Weapon visible in hand, physical strikes
- **Color Coding:** Red/Orange glow indicates melee
- **Attack Type:** Discrete attacks with cooldown between

---

## Category 1: BLADES (Sword-like Weapons)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 120 |
| Attack Speed | 3 attacks/second |
| DPS (Raw) | 360 |
| Range | 1-2 units |
| Animation | Slash in arc pattern |
| Recovery | Medium (0.33 seconds between attacks) |

### Mechanics

**Primary Effect - Cleave:**
- Hits all enemies in 120-degree cone in front of character
- Damages 3-5 enemies per slash
- Encourages aggressive positioning

**Secondary Effects (Per Rarity):**
- Common: Base slash only
- Uncommon: +Bleed (10 damage/sec for 8 seconds)
- Rare: +Bleed + Shockwave (every 3rd hit creates shockwave)
- Legendary: +Bleed + Shockwave + Lifesteal (10% of damage dealt heals player)

### Examples

**Champion Weapons:**
- Runeblade (standard enchanted sword)
- Spectral Sword (ghostly blade)
- Wraith Katana (elegant curved blade)

**Ally Weapons:**
- Sword Squire → Runeblade (120 DMG, 3/sec, cleave)

**Enemy Weapons:**
- Silencing Wraith → Hex Wave (120 DMG, 2/sec, blocks abilities)

---

## Category 2: BLUNT (Hammer-like Weapons)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 200 |
| Attack Speed | 1 attack/second |
| DPS (Raw) | 200 |
| Range | 1-2 units |
| Animation | Overhead slam/impact |
| Recovery | Long (1 second between attacks) |

### Mechanics

**Primary Effect - Knockback:**
- Pushes enemies away 2 meters on impact
- Prevents enemies from reaching player
- Creates space for repositioning
- Interrupts enemy attacks in progress

**Secondary Effect - Stun:**
- Enemies hit are slowed for 2 seconds (30% movement speed reduction)
- Allows follow-up attacks

**Tertiary Effect - Armor Break:**
- Reduces enemy defense by 30% per hit
- Stacks up to 90% reduction
- Makes armor-heavy enemies vulnerable

### Examples

**Champion Weapons:**
- Mountainsplitter (gravity-enchanted hammer)
- Stormmaul (lightning-imbued maul)
- Earthcrusher (siege-class impact weapon)

**Enemy Weapons:**
- Frost Ogre → Frost Maul (200 DMG, stun + knockback)

---

## Category 3: PIERCING (Spear/Dagger-like)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 100 |
| Attack Speed | 3.5 attacks/second |
| DPS (Raw) | 350 |
| Range | 1-2 units (reaches slightly further) |
| Animation | Thrust/stab motion |
| Recovery | Very fast (0.28 seconds between attacks) |

### Mechanics

**Primary Effect - Piercing:**
- Ignores 25% of enemy armor
- Effective against heavily-armored enemies
- Makes armor-stacking builds less effective

**Secondary Effect - Multi-Hit:**
- Can hit same enemy twice in quick succession
- Good for single-target DPS
- Visual: Two quick thrusts in sequence

**Tertiary Effect - Execute:**
- Deals 50% more damage to low-health enemies (< 30% HP)
- Encourages finishing blows
- Creates satisfying execution mechanic

### Examples

**Champion Weapons:**
- Holy Lance (long blessed spear)
- Shadowfang Dagger (quick strikes)
- Spirit Spear (ethereal weapon)

**Ally Weapons:**
- Shadow Wisp → Shadowfang Dagger (100 DMG, 3.5/sec, execute)

---

## Category 4: CLAWS/GAUNTLETS (Unarmed Style)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 90 |
| Attack Speed | 4 attacks/second |
| DPS (Raw) | 360 |
| Range | 0.5 units (shortest range) |
| Animation | Punch/claw swipe motion |
| Recovery | Very fast (0.25 seconds) |

### Mechanics

**Primary Effect - Combo:**
- Every 3rd hit deals 2x damage
- Creates rhythm/pattern to combat
- Encourages continuous attacking

**Secondary Effect - Lifesteal:**
- Heal 10% of damage dealt
- Sustained healing during combat
- Works with high-attack-speed builds

**Tertiary Effect - Reflect:**
- When hit, gain temporary damage reduction (-20% damage taken) for 3 seconds
- Can stack up to 3 times (60% total reduction)
- Encourages aggressive play despite taking hits

### Examples

**Champion Weapons:**
- Beast Claws (rune-infused talons)
- Champion's Gauntlets (boxing style)
- Shadowtalons (curved slashing)

**Ally Weapons:**
- Wolf Companion → Beast Claws (90 DMG, 4/sec, combo)

---

## Category 5: SPECIAL MELEE - SHIELDS (Defensive)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 5 (essentially no damage) |
| Block Chance | 50% when facing attacker |
| Reflection | Bounces 20% of incoming damage |
| Defense Bonus | +50% damage reduction while equipped |
| Animation | Defensive stance with shield |

### Mechanics

**Primary Effect - Block:**
- 50% chance to completely negate incoming damage
- Only works if enemy is in front of shield user
- Doesn't work to attacks from behind

**Secondary Effect - Reflection:**
- 20% of blocked damage reflects back to attacker
- Creates damage consequence for enemy attacking
- Works with multiple reflective items/blessings

**Tertiary Effect - Area Control:**
- Shield physically takes up space
- Allies can position behind shield for protection
- Creates tactical positioning game

### Examples

**Ally Weapons:**
- Shield Bearer → Aegis of Light (melee defense, 50% block)
- Halberdier → Wrathful Halberd (melee defense + counter)

---

# SECTION 4: RANGED WEAPON CATEGORIES & MECHANICS

## Ranged Weapon Framework

All ranged weapons share these characteristics:
- **Range:** 5-20 units distance (can attack from afar)
- **Animation:** Aiming, then firing/casting motion
- **Projectiles:** Visible traveling from source to target
- **Damage Falloff:** Full at 0-10 units, 80% at 10-20 units
- **Visual:** Blue/Cyan glow indicates ranged
- **Attack Type:** Discrete attacks with cooldown between (except beams)

---

## Category 1: CROSSBOWS (Rifle-like - Rapid-Fire)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 60 |
| Fire Rate | 5 shots/second |
| DPS (Raw) | 300 |
| Range | 5-20 units |
| Projectile | Bolt-sized, travels straight |
| Accuracy | Slight spread (easier to miss) |
| Animation | Quick aiming then rapid firing |

### Mechanics

**Primary Effect - Rapid Fire:**
- 5 bolts per second creates continuous barrage
- Hits multiple enemies if lined up
- High sustained DPS for damage role

**Secondary Effect - Piercing Bolt:**
- Bolts can hit multiple enemies in line
- Penetrates armor enemies
- Doesn't stop on first impact

**Tertiary Effect - No Knockback:**
- Bolts don't push enemies around
- Enemies can push toward player (tension)
- Unlike melee weapons which control space

### Examples

**Champion Weapons:**
- Repeating Crossbow (standard rapid-fire bow)
- Hand Crossbow (quick-firing sidearm)
- Storm Crossbow (lightning-enchanted barrage)

**Ally Weapons:**
- None (no vanguard allies use crossbows, that's the Champion's territory)

**Enemy Weapons:**
- Volley Archer → Volley Crossbow (150 DMG, 5/sec, assault)

---

## Category 2: CATAPULTS (Heavy/Slow - Explosive)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 150 |
| Fire Rate | 1 shot/second |
| DPS (Raw) | 150 (but with AOE) |
| Range | 5-20 units |
| Projectile | Large boulder/blast, travels in arc |
| AOE | 5 meter explosion radius |
| Animation | Charging then firing with recoil |

### Mechanics

**Primary Effect - Explosion:**
- On impact, creates explosion in 5m radius
- Deals damage to all enemies in radius
- Creates tactical consideration for placement

**Secondary Effect - Knockback:**
- Strong knockback on hit
- Pushes enemies away 2-3 meters
- Clears space around impact point

**Tertiary Effect - Armor Penetration:**
- Ignores 50% of enemy armor
- Effective against heavily armored enemies
- Explosion penetrates shields

### Examples

**Champion Weapons:**
- Siege Catapult (military-style)
- Fire Catapult (flame-imbued boulders)
- Boulder Trebuchet (massive stone hurler)

**Ally Weapons:**
- Ballista Crew → Siege Catapult (150 DMG, 1/sec, explosion)

**Enemy Weapons:**
- The Dread Lich → Adaptive Tome (switches forms, learns damage types)

---

## Category 3: STAFFS (Support-like - Utility)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 40 (low, support-focused) |
| Fire Rate | 3 casts/second |
| DPS (Raw) | 120 (not meant for damage) |
| Range | 5-8 units |
| Projectile | Glowing orbs, move slower |
| Effect | Healing, buffs, support |
| Animation | Channeling then releasing orb |

### Mechanics

**Primary Effect - Support Projectiles:**
- Heals allies on impact (not damage)
- Buff projectiles strengthen allies
- Example: Staff of Restoration shoots green orbs that heal on hit

**Secondary Effect - Residual Effects:**
- Leave buff area when they land
- Area persists for 5 seconds
- Allies standing in area gain bonus

**Tertiary Effect - No Direct Damage:**
- Staff attacks don't damage enemies
- Purely support role (damage comes from other sources)
- Encourages team play

### Examples

**Ally Weapons:**
- Light Cleric → Staff of Restoration (support, heal projectiles)
- Battle Bard → Staff of Empowerment (support, buff projectiles)
- Wind Sprite → Staff of Swiftness (support, speed projectiles)

---

## Category 4: BEAMS (Laser-like - Continuous Magic)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 50 |
| Fire Rate | Continuous (10/sec effectively) |
| DPS (Raw) | 500 |
| Range | 5-20 units |
| Projectile Type | Beam, not projectile |
| Beam Width | 1 unit (narrow focus) |
| Animation | Beam turns on and holds |

### Mechanics

**Primary Effect - Continuous Beam:**
- Hold to fire, creates persistent beam of light/magic
- Beam stays on target while moving
- Can sweep across enemies

**Secondary Effect - Penetration:**
- Beam goes through multiple enemies in line
- Hits all enemies in beam path
- No damage reduction per enemy

**Tertiary Effect - Persistent Damage:**
- Damage applies every frame while active
- Creates burning sensation (constant damage)
- Different feel from discrete hits

### Examples

**Ally Weapons:**
- Owl Familiar → Scrying Light (utility, reveals enemies)
- Mirror Spirit → Mirror Light (defense, bounces projectiles)

**Enemy Weapons:**
- Mirror Sorcerer → Reflection Beam (defense, bounces damage back)
- Hunter Wraith → Seeker's Beam (offense, aims ahead)

---

## Category 5: SPECIAL RANGED - MIRRORS (Defensive)

### Characteristics

| Aspect | Value |
|--------|-------|
| Base Damage | 0 (pure defense) |
| Reflection | 30% of damage bounces back |
| Range | Emits in all directions |
| Effect | Bounces projectiles, creates shields |
| Animation | Pulsing protective field |

### Mechanics

**Primary Effect - Projectile Bounce:**
- Incoming projectiles are reflected back
- At 30% strength (not full damage back)
- Can bounce multiple times (up to 5 bounces)

**Secondary Effect - AOE Protection:**
- Protects allies in area (2 unit radius)
- Creates safe zone from ranged attacks
- Doesn't stop melee attacks (only projectiles)

**Tertiary Effect - No Offensive Damage:**
- Pure defense weapon (deals no damage)
- Support role (enable teammates)
- Encourages positioning strategy

### Examples

**Ally Weapons:**
- Mirror Spirit → Mirror Light (defense, bounces projectiles back at enemies)
- Stone Golem → Stone Barrier (pure tank, no offensive capability)

---

# SECTION 5: CHAMPION WEAPONS (PLAYER)

## How Champion Weapons Work

**Champion Flexibility:**
- Champions can use both melee and ranged
- Weapon choice determined by equipped blessings/runes
- Switching weapons changes visual appearance
- Switch happens instantly with brief transition effect

**Primary Weapon Determination:**
- Character's primary passive determines default weapon
- Equipped blessings can override weapon type
- Runes can enhance specific weapon types
- Build determines final weapon setup

---

## All 18 Champions - Weapon Assignment

### STARTER TIER CHAMPIONS

**Aldric (Generalist)**
- **Default Weapon:** Melee - Runeblade (Sword)
- **Alternative:** Ranged - Repeating Crossbow (if blessings chosen)
- **Passive Bonus:** +3% crit works with both
- **Best Weapon Build:** Melee-focused (suits aggressive style)
- **Primary Passive Interaction:** Crits apply to melee strikes (visual sparks on hit)
- **Base Stats:** 45 HP, 30 ATK, 15 Speed

**Zephyr (Speed Runner)**
- **Default Weapon:** Melee - Beast Claws (Gauntlets)
- **Alternative:** Ranged - Repeating Crossbow (with speed blessings)
- **Passive Bonus:** +1 Ward per 5 speed (scales with ranged distance)
- **Best Weapon Build:** Ranged (leverage speed for positioning)
- **Primary Passive Interaction:** Speed increases projectile range
- **Base Stats:** 40 HP, 25 ATK, 20 Speed (high speed)

**Cassia (Collector)**
- **Default Weapon:** Ranged - Support Staff (utility)
- **Alternative:** Melee - Spectral Sword (if economy-focused)
- **Passive Bonus:** All allies appear in shop (weapon variants available)
- **Best Weapon Build:** Ranged support (enables ally economy)
- **Primary Passive Interaction:** Can purchase weapon variants in shop
- **Base Stats:** 50 HP, 28 ATK, 12 Speed

### TIER 2 CHAMPIONS (Stat Unlocks)

**Arcanus (Spellweaver)**
- **Default Weapon:** Melee - Stormmaul (Blunt)
- **Primary Passive:** +2 Mana per blessing
- **Weapon Interaction:** Each blessing equip gives bonus ATK
- **Best Build:** Melee with many blessings (hammer hits harder with more arcane power)
- **Base Stats:** 48 HP, 32 ATK, 13 Speed

**Shade (Infiltrator)**
- **Default Weapon:** Melee - Shadowfang Dagger (Piercing/Execute)
- **Primary Passive:** +40% damage vs elites
- **Weapon Interaction:** Dagger's execute bonus combines with elite damage
- **Best Build:** Melee vs tough enemies (execute weak ones)
- **Base Stats:** 42 HP, 35 ATK, 16 Speed

**Bron (Berserker)**
- **Default Weapon:** Melee - Beast Claws (Gauntlets/Lifesteal)
- **Primary Passive:** 3% self-damage but +15% all damage
- **Weapon Interaction:** Lifesteal from claws offsets self-damage
- **Best Build:** Melee with lifesteal healing
- **Base Stats:** 38 HP, 40 ATK, 14 Speed (high ATK, low HP)

**Morgath (Cursed Hexer)**
- **Default Weapon:** Ranged - Hex Beam (Beam)
- **Primary Passive:** Allies apply 8% max HP corruption DOT
- **Weapon Interaction:** Beam applies corruption while allies spread it
- **Best Build:** Ranged with ally supports (spread corruption)
- **Base Stats:** 44 HP, 28 ATK, 15 Speed

**Mira (Mirror Knight)**
- **Default Weapon:** Melee - Spectral Sword (Blade/Cleave)
- **Primary Passive:** Copy enemy abilities at 50% power
- **Weapon Interaction:** Melee cleave hits multiple enemies, copies all
- **Best Build:** Melee to get close, copy strong attacks
- **Base Stats:** 46 HP, 32 ATK, 13 Speed

### TIER 3 CHAMPIONS (Difficulty Unlocks)

**Falka (Ranger)**
- **Default Weapon:** Ranged - Repeating Crossbow (Piercing bolts)
- **Primary Passive:** Crits apply Surge (120% crit damage every 4s)
- **Weapon Interaction:** Crossbow crits cascade, stacking crit bonuses
- **Best Build:** Ranged crit build (bolt crits stacked)
- **Base Stats:** 40 HP, 33 ATK, 17 Speed

**Drakar (Siegebreaker)**
- **Default Weapon:** Ranged - Siege Catapult (Explosive)
- **Primary Passive:** +6 ATK ranged allies, -25% melee allies
- **Weapon Interaction:** Catapults get direct ATK boost from passive
- **Best Build:** Pure ranged (synergy with siege allies)
- **Base Stats:** 43 HP, 36 ATK, 12 Speed

**Garrick (Guardian)**
- **Default Weapon:** Melee - Aegis of Light (Defense)
- **Primary Passive:** +8 Barrier, -30% barrier cooldown
- **Weapon Interaction:** Shield reflects damage, barrier persists
- **Best Build:** Melee tank with barrier stacking
- **Base Stats:** 52 HP, 24 ATK, 10 Speed (tanky)

**Aurelius (Crusader)**
- **Default Weapon:** Melee - Runeblade (Cascade)
- **Primary Passive:** +3 all stats per wave start
- **Weapon Interaction:** Stats grow, blade damage scales with stats
- **Best Build:** Melee scaling build (gets stronger every wave)
- **Base Stats:** 45 HP, 28 ATK, 14 Speed (grows per wave)

### TIER 4 CHAMPIONS (Advanced)

**Lyra (Witch)**
- **Default Weapon:** Ranged - Curse Staff (Support)
- **Primary Passive:** -8% enemy accuracy per ally
- **Weapon Interaction:** Staff debuffs enemies, reduces their accuracy
- **Best Build:** Ranged support with many allies
- **Base Stats:** 42 HP, 26 ATK, 16 Speed

**Seraphina (Seer)**
- **Default Weapon:** Ranged - Seeker's Crossbow (Aiming ahead)
- **Primary Passive:** Reveals 3 extra shop items
- **Weapon Interaction:** Crossbow bolts predict enemy movement
- **Best Build:** Ranged with economic advantage
- **Base Stats:** 41 HP, 30 ATK, 15 Speed

**Ulric (Paladin)**
- **Default Weapon:** Melee - Mountainsplitter (Knockback tank)
- **Primary Passive:** +25 HP per ally, shields overflow to allies
- **Weapon Interaction:** Hammer knockback protects allies behind
- **Best Build:** Melee tank with allies
- **Base Stats:** 55 HP, 28 ATK, 11 Speed (highest HP)

**Ronan (Outrider)**
- **Default Weapon:** Ranged - Storm Crossbow (Rapid-fire bolts)
- **Primary Passive:** +12% attack speed per upgrade purchased
- **Weapon Interaction:** Crossbow fire rate increases with upgrades
- **Best Build:** Ranged buying many upgrades (stacking speed)
- **Base Stats:** 39 HP, 29 ATK, 19 Speed (fastest)

### TIER 5 CHAMPIONS (Ultimate)

**Wyrd (Wildmage)**
- **Default Weapon:** Melee - Random Weapon (switches per wave)
- **Primary Passive:** ±30% random stat per wave start
- **Weapon Interaction:** Weapon type changes randomly (high variance)
- **Best Build:** Adapt based on RNG (high-skill character)
- **Base Stats:** 44 HP, 31 ATK, 15 Speed (unpredictable)

**Auron (High King)**
- **Default Weapon:** Melee - Runeblade (Enhanced)
- **Primary Passive:** -40% blessing cost, +50% refresh cost
- **Weapon Interaction:** Blade damage boosted by free blessings
- **Best Build:** Melee with many blessings (economy advantage)
- **Base Stats:** 47 HP, 34 ATK, 13 Speed

---

## Weapon Switch Mechanic

**How Switching Works:**

1. **Equip Blessing** (e.g., "Ranged Damage Boost")
   - Champion's default melee weapon fades out
   - New ranged weapon fades in (0.5 second transition)
   - Weapon sprite changes, animations change
   - Next attack uses new weapon type

2. **Combat During Switch**
   - Cannot attack during 0.5s transition
   - Creates tactical decision (when to switch)
   - Visible to enemies (they see weapon change)

3. **Visual Feedback**
   - Old weapon fades (white trail)
   - New weapon appears (energetic burst)
   - Character model updates with new weapon
   - Particles show transition effect

---

# SECTION 6: ALLY (VASSAL) WEAPONS

## Ally Weapon Framework

**Key Principle:** Each ally type has ONE FIXED weapon that NEVER changes.

**Why Fixed?**
1. **Visual Clarity:** Player instantly knows what Sword Squire does (always has sword)
2. **Identity:** Ally identity tied to weapon (Ballista Crew = catapult)
3. **No RNG:** Weapon stats don't vary per run
4. **Synergy Design:** Can create blessings that buff "sword allies" or "catapult allies"
5. **Player Expectation:** Learn once, use reliably forever

---

## VANGUARD (Offensive Allies)

### Sword Squire - Melee Sword

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Blade |
| **Base Damage** | 120 |
| **Attack Speed** | 3/second |
| **DPS** | 360 |
| **Range** | 1-2 units |
| **Special Effect** | Cleave (hits 3-5 enemies in cone) |
| **Tier 2 Effect** | Bleed (10 DMG/sec, 8 seconds) |
| **Tier 3 Effect** | Shockwave (3rd hit creates shockwave) |
| **Visual** | Glowing rune-etched sword with trail |
| **Color** | Red/Orange glow |
| **Animation** | Diagonal slash, horizontal sweep |

**Combat Behavior:**
- Walks toward enemies
- Attacks when within 2 units
- Visible sword strikes
- Cleave hits multiple enemies
- Self-sustaining in combat

**Synergy Allies:**
- Works well with other Vanguard (stacking offense)
- Synergizes with 5x Vanguard set bonus (+40% attack speed)

---

### Ballista Crew - Ranged Catapult

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Catapult |
| **Base Damage** | 150 |
| **Fire Rate** | 1/second |
| **DPS** | 150 + AOE damage |
| **Range** | 5-20 units |
| **Special Effect** | Explosion AOE (5m radius) |
| **Knockback** | 2-3 meters |
| **Armor Pen** | 50% |
| **Tier 2 Effect** | Larger AOE (7m radius) |
| **Tier 3 Effect** | +50% explosion damage |
| **Visual** | Large siege catapult with blast effect |
| **Color** | Blue/Cyan glow on projectile |
| **Animation** | Rotate, charge, fire with recoil |

**Combat Behavior:**
- Stands at distance from enemies
- Shoots when enemies in range (5-20 units)
- Visible explosion on impact
- Pushes enemies away
- Effective against clustered enemies

**Synergy Allies:**
- Excellent with area control builds
- Works with any ally (no specific bond synergy)

---

### Wolf Companion - Melee Claws/Beast

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Claws |
| **Base Damage** | 90 |
| **Attack Speed** | 4/second |
| **DPS** | 360 |
| **Range** | 0.5 units (close) |
| **Special Effect** | Combo (3rd hit 2x damage) |
| **Lifesteal** | 10% of damage |
| **Tier 2 Effect** | Reflect shield (-20% damage taken) |
| **Tier 3 Effect** | 3x reflect stacks (60% total reduction) |
| **Visual** | Snarling wolf with glowing claws and fangs |
| **Color** | Red/Orange glow |
| **Animation** | Pounce, claw swipes, bite |

**Combat Behavior:**
- Runs toward enemies fast
- Attacks rapidly at close range
- Heals from damage dealt (lifesteal)
- Combo hits create burst damage
- Best vs grouped enemies

**Synergy Allies:**
- Works with offensive Vanguard
- Lifesteal sustains in long battles

---

### Shadow Wisp - Melee Dagger/Spirit

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Piercing |
| **Base Damage** | 100 |
| **Attack Speed** | 3.5/second |
| **DPS** | 350 |
| **Range** | 1-2 units |
| **Special Effect** | Execute (50% bonus damage to low HP) |
| **Armor Pen** | 25% |
| **Tier 2 Effect** | Multi-hit (double thrust per attack) |
| **Tier 3 Effect** | Execute threshold raised to 50% HP |
| **Visual** | Ethereal floating spirit with shadowy dagger |
| **Color** | Red/Purple glow |
| **Animation** | Quick thrust, phase-step between strikes |

**Combat Behavior:**
- Targets low-health enemies first
- Quick stabbing strikes
- Phases between targets
- Executes weak enemies for bonus
- Best vs tough single targets after softening

**Synergy Allies:**
- Pairs with Vanguard that soften enemies
- Excellent finisher in team comps

---

## BULWARK (Defensive Allies)

### Shield Bearer - Melee Shield

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Special Defense (Shield) |
| **Base Damage** | 5 (minimal) |
| **Block Chance** | 50% |
| **Reflection** | 20% damage reflected |
| **Range** | 1-2 units |
| **Defense Bonus** | +50% damage reduction in radius |
| **Tier 2 Effect** | Block chance increases to 65% |
| **Tier 3 Effect** | Allies behind shield take -75% damage |
| **Visual** | Knight in plate armor with massive tower shield |
| **Color** | Red/Gold glow (melee defense) |
| **Animation** | Steady defensive stance, shield raised |

**Combat Behavior:**
- Positions between enemies and allies
- Blocks attacks passively
- Reflects portion of damage back
- Creates safe zone for ranged allies
- Holds ground (doesn't chase)

**Synergy Allies:**
- Protects Mystics and ranged Vanguard
- Synergizes with 5x Bulwark set bonus (defense aura)

---

### Stone Golem - Melee Wall/Tank

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Special Defense (Barrier) |
| **Base Damage** | 0 (pure tank, no offense) |
| **HP** | 300 (massive durability) |
| **Range** | 1-2 units |
| **Special Effect** | Physical wall blocks enemy path |
| **Block Effect** | Enemies can't pass through |
| **Tier 2 Effect** | HP increased to 450 |
| **Tier 3 Effect** | Slams ground every 10s (knockback all nearby) |
| **Visual** | Hulking stone construct with glowing runes |
| **Color** | Red/Earth glow (melee defense) |
| **Animation** | Stomping walk, body-block stance |

**Combat Behavior:**
- Stands as impassable wall
- Blocks enemy movement
- Doesn't attack (pure obstruction)
- Forces enemies to path around
- Creates choke points for damage

**Synergy Allies:**
- Funnels enemies into Ballista Crew AOE
- Protects backline Mystics

---

### Mirror Spirit - Ranged Reflector

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Special Defense (Mirror) |
| **Base Damage** | 0 (pure defense) |
| **Reflection Rate** | 30% projectile bounce |
| **Range** | 5-20 units (emits in all directions) |
| **Special Effect** | Bounces enemy projectiles back |
| **Bounce Count** | Up to 5 bounces per projectile |
| **Tier 2 Effect** | Reflection rate increases to 50% |
| **Tier 3 Effect** | Reflected projectiles deal 2x damage |
| **Visual** | Ethereal floating spirit emitting reflection beam in all directions |
| **Color** | Blue/Cyan glow radiating |
| **Animation** | Pulsing protective field |

**Combat Behavior:**
- Emits reflection aura
- Bounces enemy projectiles automatically
- Creates "reflected projectile" attacks
- Protects allies in radius (2 units)
- Works best against ranged enemies

---

### Halberdier - Melee Defense (Halberd)

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Special Defense |
| **Base Damage** | 50 (low) |
| **Block/Counter** | Blocks melee attacks, counters |
| **Range** | 1-2 units |
| **Counter Damage** | 50% of blocked damage reflected |
| **Stun** | Blocking stuns attacker 1 second |
| **Tier 2 Effect** | Reflect 60% (stronger counters) |
| **Tier 3 Effect** | Counter generates resource for player |
| **Visual** | Knight wielding ornate halberd in defensive stance |
| **Color** | Red/Orange glow (melee) |
| **Animation** | Defensive stance, counter strike |

**Combat Behavior:**
- Blocks melee attacks from enemies
- Counters blocked attacks (reflects damage)
- Stuns enemies that attack it
- Creates crowd control through defense
- Works best against melee enemies

---

## MYSTICS (Support Allies)

### Light Cleric - Ranged Support (Healing Staff)

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Healing Staff |
| **Base Damage** | 40 (support, low) |
| **Heal Amount** | 50 per projectile hit |
| **Range** | 5-8 units |
| **Fire Rate** | 3/second |
| **Projectile Type** | Green healing orb |
| **Heal Target** | Allies only (no friendly fire) |
| **Area Heal** | Projectile creates heal zone (5s) |
| **Tier 2 Effect** | +75% heal (stronger healing) |
| **Tier 3 Effect** | Heal spreads to nearby allies |
| **Visual** | Robed priestess channeling green orbs toward allies |
| **Color** | Green glow (healing) |
| **Animation** | Channeling staff, releasing healing orb |

**Combat Behavior:**
- Targets wounded allies
- Fires healing projectiles at distance
- Creates healing zones on impact
- Maintains ally health
- Never attacks enemies (pure support)

**Synergy Allies:**
- Works with tanky allies (keeps them alive)
- Synergizes with 5x Mystics set bonus (double buffs)

---

### Battle Bard - Ranged Support (Empowerment Staff)

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Empowerment Staff |
| **Base Damage** | 40 (support, low) |
| **Buff Amount** | +10% ATK per buff hit |
| **Range** | 5-8 units |
| **Fire Rate** | 3/second |
| **Projectile Type** | Orange buff orb |
| **Buff Duration** | 10 seconds |
| **Buff Target** | Allies only |
| **Residual** | Buff zone persists 3 seconds |
| **Tier 2 Effect** | +15% ATK (stronger buff) |
| **Tier 3 Effect** | Buff stacks (up to 3x = +45% ATK) |
| **Visual** | Bard with enchanted lute and staff, releasing orange orbs |
| **Color** | Orange glow (buff) |
| **Animation** | Strumming + channeling staff, releasing buff orb |

**Combat Behavior:**
- Targets all allies equally
- Fires buff projectiles
- Creates buff zones
- Stacks buffs for higher damage
- Enhances ally offense

---

### Wind Sprite - Ranged Support (Swiftness Staff)

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Swiftness Staff |
| **Base Damage** | 40 (support, low) |
| **Speed Bonus** | +15% movement speed |
| **Range** | 5-8 units |
| **Fire Rate** | 3/second |
| **Projectile Type** | Blue speed orb |
| **Duration** | 10 seconds |
| **Effects** | Movement speed + attack speed |
| **Tier 2 Effect** | +25% speed (stronger boost) |
| **Tier 3 Effect** | Speed stacks with other Mystics |
| **Visual** | Small winged fairy/sprite trailing wind currents |
| **Color** | Blue glow (speed) |
| **Animation** | Quick channeling, fast-moving orb |

**Combat Behavior:**
- Targets all allies
- Fires speed-boost projectiles
- Creates movement speed zones
- Increases ally positioning flexibility
- Stacks with other speed sources

---

### Owl Familiar - Ranged Utility (Scrying Beam)

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Scrying Light |
| **Base Damage** | 0 (utility, no damage) |
| **Scan Range** | 15 units (reveals map area) |
| **Reveal Effect** | Shows hidden enemies |
| **Range** | 5-20 units |
| **Fire Rate** | Continuous beam |
| **Projectile Type** | Yellow scrying beam |
| **Duration** | Persists while scanning |
| **Tier 2 Effect** | +50% scan range (22 units) |
| **Tier 3 Effect** | Marks enemies (takes 30% more damage) |
| **Visual** | Magical owl with glowing eyes emitting yellow scrying beam |
| **Color** | Yellow glow (detection) |
| **Animation** | Hovering scanner with rotating beam |

**Combat Behavior:**
- Scans area for enemies
- Reveals hidden enemies
- Marks high-threat enemies
- Provides information advantage
- Never deals direct damage

---

## Ally Tier Progression

All allies have 3 tiers:

**Tier 1 (Base):**
- Starting form
- Can be obtained early
- Base stats and effects

**Tier 2 (Upgraded):**
- 3 copies of same ally merge
- +50% damage
- Enhanced special effect
- Better scaling

**Tier 3 (Ultimate):**
- 5 copies of same ally merge
- +100% damage
- Ultimate special ability
- Strongest form

**Example:** Sword Squire
- Tier 1: 120 DMG, 3/sec, cleave 5
- Tier 2: 180 DMG, 3/sec, cleave 5 + bleed
- Tier 3: 240 DMG, 3/sec, cleave 5 + bleed + shockwave effect

---

# SECTION 7: ENEMY (SCOURGE) WEAPONS

## Enemy Weapon Assignment

**Key Principle:** Enemy type determines weapon type. No surprises.

**Design Goal:** Visual clarity - you see an enemy and immediately know if it's melee or ranged threat.

---

## MELEE ENEMIES

### Bone Shield Guardian - Melee Defense

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Shield (Defense) |
| **Base Damage** | 50 (low, tank-focused) |
| **Block Chance** | 50% |
| **Block Effect** | Blocks 50% of incoming damage |
| **HP** | 100 (high, tanky) |
| **Range** | 1-2 units |
| **Threat Level** | Low (defensive, not aggressive) |
| **Visual** | Skeleton warrior with bone shield and rusted mace |
| **Appearance** | Pale blue/grey defensive undead unit |
| **Weakness** | Holy damage (bypasses bone defenses) |
| **Strategy** | Use Holy damage to ignore block |

**Combat Behavior:**
- Blocks attacks passively
- Low offensive threat
- High durability
- Creates space denial (blocks path)
- Should be avoided or use specific damage type

---

### Frost Ogre - Melee Crowd Control

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Hammer |
| **Base Damage** | 100 |
| **Attack Speed** | 2/second |
| **Special Effect** | Knockback 2m |
| **Crowd Control** | Stun 2 seconds |
| **HP** | 150 (medium) |
| **Range** | 1-2 units |
| **Threat Level** | Medium (can disrupt) |
| **Visual** | Hulking ogre wielding a massive ice-encrusted maul |
| **Appearance** | Blue/icy mountain ogre |
| **Weakness** | Fire damage (melts ice) |
| **Strategy** | Use Fire to damage + prevent freeze |

**Combat Behavior:**
- Walks toward player
- Heavy hammer strikes
- Knocks player away (pushes back)
- Stuns player (immobilizes briefly)
- Forces repositioning (threat)

---

### Tunnel Goblin - Melee Armor Break

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Pickaxe/Drill |
| **Base Damage** | 120 |
| **Attack Speed** | 2/second |
| **Special Effect** | Armor Break (-30% DEF) |
| **Stacking** | Armor break stacks (max -90%) |
| **HP** | 125 (medium) |
| **Range** | 1-2 units |
| **Threat Level** | Medium-High (reduces defenses) |
| **Visual** | Squat goblin with massive rotating pickaxe/drill |
| **Appearance** | Orange/brown burrowing goblin |
| **Weakness** | Physical damage (breaks tools) |
| **Strategy** | Kill quickly before armor stacks break |

**Combat Behavior:**
- Walks forward steadily
- Drilling motion attack
- Each hit reduces player defenses
- Multiple hits stack (dangerous)
- Must be neutralized quickly

---

### Silencing Wraith - Melee Ability Block

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Hex Wave |
| **Base Damage** | 80 (low) |
| **Attack Speed** | 1/second |
| **Special Effect** | Blocks abilities (silences spells) |
| **Block Duration** | 4 seconds |
| **Block Cooldown** | 6 seconds (repeats) |
| **HP** | 110 (medium-low) |
| **Range** | 1-2 units |
| **Threat Level** | High (disables player) |
| **Visual** | Tattered hooded wraith emitting purple silencing waves |
| **Appearance** | Purple/black anti-magic specter |
| **Weakness** | Arcane damage (raw magic shatters hex) |
| **Strategy** | Use Arcane to break silence fast |

**Combat Behavior:**
- Stands in place
- Periodically generates ability-blocking wave
- Prevents player blessing/ability use
- Creates high-pressure situation (can't use tools)
- Must kill to regain agency

---

## RANGED ENEMIES

### Imp Swarm - Ranged Spread

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Projectile Cloud |
| **Base Damage** | 60 per projectile |
| **Fire Rate** | 5/second (barrage) |
| **Special Effect** | Spreads (projectiles spread) |
| **Number** | 5 imps per swarm |
| **HP** | 30 per imp (5 imps) |
| **Range** | 5-15 units |
| **Threat Level** | Medium (high DPS if not killed) |
| **Visual** | Cluster of small flying imps hurling fireballs |
| **Appearance** | Green/red toxic swarm of demonlets |
| **Weakness** | Fire damage (burns wings, AOE-friendly) |
| **Strategy** | Use AOE or Fire to kill cluster |

**Combat Behavior:**
- Stands at distance
- Fires projectile cloud (fireballs)
- Spreads out (harder to dodge)
- Must kill all imps or they re-cluster
- Area attacks most effective

---

### Mirror Sorcerer - Ranged Defense

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Reflection Beam |
| **Base Damage** | 0 (pure defense) |
| **Reflection** | Bounces 30% damage back |
| **HP** | 140 (high) |
| **Range** | 5-20 units (emits everywhere) |
| **Threat Level** | High (reflects damage back) |
| **Special Effect** | Reflect loop (can bounce 5x) |
| **Visual** | Robed sorcerer with floating mirror shards |
| **Appearance** | Blue/silver enchanter unit |
| **Weakness** | Holy damage (shatters mirror magic) |
| **Strategy** | Use Holy to break mirror quickly |

**Combat Behavior:**
- Stands far away
- Reflects incoming projectiles
- Creates reflected projectile attacks
- High durability + reflect makes it dangerous
- Player must switch to Holy approach

---

### Mimic Fiend - Ranged Mimic

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Ability Copying |
| **Base Damage** | 50 (varies by copied ability) |
| **Ability Copy** | Mimics player abilities at 50% power |
| **Mimic Delay** | 2 second delay (copies what you used) |
| **HP** | 130 (medium-high) |
| **Range** | 5-15 units |
| **Threat Level** | High (uses player's own abilities) |
| **Special Effect** | Steals item effects |
| **Visual** | Shapeshifting demonic fiend with morphing form |
| **Appearance** | Black/iridescent mimic creature |
| **Weakness** | Arcane damage (disrupts copying magic) |
| **Strategy** | Use unpredictable abilities to confuse mimic |

**Combat Behavior:**
- Watches player actions
- Copies abilities with 2 second delay
- Uses copied ability against player
- At 50% damage (weaker copy)
- Predictable strategy fails (use random plays)

---

### Volley Archer - Ranged Rapid-Fire

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Rapid-Fire Crossbow |
| **Base Damage** | 30 per shot |
| **Fire Rate** | 5/second (150 DPS) |
| **Special Effect** | Attack Speed Ramps |
| **Attack Speed Scaling** | 1→5 shots/second |
| **HP** | 95 (low) |
| **Range** | 5-20 units |
| **Threat Level** | High (DPS scales) |
| **Visual** | Goblin/dark archer with rapid-fire crossbow contraption |
| **Appearance** | Orange/red assault archer unit |
| **Weakness** | Fire damage (burns crossbow strings) |
| **Strategy** | Kill quickly before attack speed maxes |

**Combat Behavior:**
- Starts slow firing rate
- Increases fire rate each second
- By second 5, firing 150 DPS
- Must be killed quickly or becomes overwhelming
- Fire slows approach viable

---

### Hunter Wraith - Ranged Prediction

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Prediction Shot |
| **Base Damage** | 80 |
| **Fire Rate** | 2/second |
| **Special Effect** | Predicts Movement |
| **Prediction Accuracy** | Aims where player will be |
| **HP** | 105 (medium) |
| **Range** | 5-20 units |
| **Threat Level** | High (hard to dodge) |
| **Visual** | Spectral hunter with longbow targeting future position |
| **Appearance** | Purple/violet predator wraith |
| **Weakness** | Randomness (breaks prediction) |
| **Strategy** | Use unpredictable movement to evade |

**Combat Behavior:**
- Predicts where player is moving
- Aims projectile at predicted location
- Difficult to dodge (predicts your evasion)
- Requires skilled movement to avoid
- Random movement breaks prediction

---

## BOSS ENEMY

### The Dread Lich (Main Boss) - Hybrid

| Property | Value |
|----------|-------|
| **Weapon Type** | HYBRID - Switches between melee & ranged |
| **Base Damage** | 100+ (scales per tier) |
| **HP** | 500-1500+ (scales per difficulty/wave) |
| **Phases** | 4 phases (25% HP each) |
| **Pattern** | Melee for 20s → Ranged for 20s → Switch |
| **Threat Level** | Extreme (adapts to player strategy) |
| **Special Effect** | Learns from player damage types |
| **Learning** | Gains resistance to repeated damage |
| **Visual** | Crowned skeletal lich wreathed in shifting void magic |
| **Appearance** | Void/reality-distortion themed undead king |
| **Weakness** | Varied damage types (breaks learning) |
| **Strategy** | Mix damage types to prevent adaptation |

**Melee Phase:**
- **Weapon:** Wrathful Halberd (cursed polearm)
- **Damage:** 150 melee damage
- **Attacks:** 2/second
- **Range:** 1-2 units (closes in)
- **Effect:** Knockback + stun
- **Weakness:** Keep distance, use ranged

**Ranged Phase:**
- **Weapon:** Adaptive Tome (learns attacks)
- **Damage:** 120 ranged damage
- **Attacks:** 1/second
- **Range:** 5-20 units
- **Effect:** Explosion + homing (tracks player)
- **Weakness:** Use melee to close in

**Learning Mechanic:**
- First Fire damage: 100% effective
- Second Fire damage: 90% effective
- Third Fire damage: 80% effective (resistance grows)
- Fourth Fire damage: 70% effective
- Must use different damage types to maintain effectiveness

**Boss Scaling:**
- Difficulty 1: 500 HP, 100 DMG
- Difficulty 2: 600 HP, 120 DMG
- Difficulty 3: 700 HP, 140 DMG
- Difficulty 4: 850 HP, 160 DMG
- Difficulty 5: 1000 HP, 200 DMG
- Endless mode: +5% per wave after wave 50

---

# SECTION 8: VISUAL IDENTIFICATION & ANIMATION

## Visual Distinction System

### Melee Entities

**Visual Indicators:**
- **Weapon:** Clearly visible physical weapon in hand (sword, hammer, spear, claws)
- **Distance:** Standing 1-2 units from enemies
- **Color:** Red/Orange glow around weapon and entity
- **Animation:** Swinging, slashing, stabbing motions
- **Effect Particles:** Sparks, light trails on hit
- **Audio:** Metallic impact sounds, whoosh sounds
- **HUD:** Red health bar, melee icon

**Examples:**
- Champion with Runeblade → Glowing sword, red aura
- Sword Squire → Energy sword, cleaving attacks
- Bone Shield Guardian (enemy) → Shield + mace, defensive stance

---

### Ranged Entities

**Visual Indicators:**
- **Weapon:** Clearly visible aiming/firing weapon (crossbow, catapult, staff, beam)
- **Distance:** Standing 5+ units away from enemies
- **Color:** Blue/Cyan glow around weapon and entity
- **Animation:** Aiming, firing, channeling motions
- **Projectiles:** Visible traveling through air
- **Effect Particles:** Projectile trails, impact effects
- **Audio:** Shooting sounds, projectile whoosh
- **HUD:** Yellow health bar, ranged icon

**Examples:**
- Champion with Repeating Crossbow → Bow with scope, blue aura, firing
- Ballista Crew → Large siege weapon, explosion effects
- Volley Archer (enemy) → Rapid-fire crossbow, barrage animation

---

### Support Entities

**Visual Indicators:**
- **Weapon:** Visible staff or magic implement
- **Distance:** Standing 5+ units from allies (out of danger)
- **Color:** Green/Yellow glow around weapon and entity
- **Animation:** Channeling, casting motions
- **Projectiles:** Visible healing/buff orbs traveling
- **Effect Particles:** Sparkles, light trails toward allies
- **Audio:** Chime sounds, magical hums
- **HUD:** Green health bar, support icon

**Examples:**
- Light Cleric → Staff of Restoration with green healing orbs
- Battle Bard → Staff of Empowerment with orange buff orbs
- Wind Sprite → Staff of Swiftness with blue speed orbs

---

## Animation Timeline Reference

### Melee Attack Animation Timeline

```
SWORD ATTACK (Total ~333ms):
0ms:   Weapon at ready position
80ms:  Wind-up (sword raised)
150ms: Strike begins (swing forward)
200ms: Impact frame (HIT DETECTION)
250ms: Follow-through
333ms: Ready for next attack

HAMMER ATTACK (Total ~1000ms):
0ms:   Weapon at ready
200ms: Wind-up (overhead raise)
500ms: Slam begins (downward)
700ms: Impact (full force hit)
950ms: Recovery (slow return)
1000ms: Ready for next

SPEAR ATTACK (Total ~286ms):
0ms:   Weapon at neutral
50ms:  Quick thrust (very fast)
100ms: Impact
150ms: Quick return
286ms: Ready (faster than others)

CLAW ATTACK (Total ~250ms):
0ms:   At ready
80ms:  Swipe forward (quick)
120ms: Impact (claw contact)
180ms: Recovery (very fast)
250ms: Ready for next
```

### Ranged Attack Animation Timeline

```
CROSSBOW ATTACK (Total ~200ms):
0ms:   Crossbow at neutral
50ms:  Aim (slight rise)
100ms: Fire (string release)
120ms: Bolt spawns and travels
200ms: Ready for next shot (rapid)

Projectile flight: 50-200ms to target (depends on distance)

CATAPULT ATTACK (Total ~1000ms):
0ms:   Catapult at neutral
300ms: Charging (visible charge-up)
700ms: Fire (large blast)
800ms: Projectile travels in arc
900ms: Impact (explosion effect)
1000ms: Ready for next

STAFF ATTACK (Total ~400ms):
0ms:   Staff at neutral
100ms: Channeling (orb forms)
250ms: Release (projectile fires)
300ms: Projectile travels
400ms: Ready for next

BEAM ATTACK (Continuous):
0ms:   Beam activates (turns on)
100ms: Beam fully formed
[ongoing]: Beam persists while active
[X]ms: Beam deactivates (turns off)
```

---

## HUD/UI Visual Feedback

### Enemy Health Bar with Weapon Indicator

```
┌─────────────────────────────────────┐
│ Bone Shield Guardian [MELEE SHIELD] │ ← Enemy name + weapon type
│ ████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ ← Health bar
│ 100/150 HP                          │ ← Numbers
│ 🛡️ Block (50%) | ⚠️ Threat: LOW   │ ← Special effects + threat level
└─────────────────────────────────────┘

ICONS:
🛡️  = Melee/Defense weapon
🏹 = Ranged weapon
⚡ = Special ability active
⚠️  = Threat level indicator (LOW/MEDIUM/HIGH)
```

### Champion Weapon Display (Top Right)

```
┌──────────────────────────────┐
│ RUNEBLADE (MELEE)            │ ← Current weapon
├──────────────────────────────┤
│ Damage: 120                  │ ← Base damage
│ Speed: 3.0 attacks/sec       │ ← Attack speed
│ DPS: 360                     │ ← Calculated DPS
│ Range: 1-2 units            │ ← Attack range
│ Effect: Cleave (5 enemies)  │ ← Special mechanic
│                              │
│ [NEXT WEAPON] [STATS]        │ ← UI buttons
└──────────────────────────────┘
```

### Ally Card in Party Panel

```
┌──────────────────────────────┐
│ Sword Squire (Tier 2)        │ ← Name + tier
├──────────────────────────────┤
│ Weapon: Runeblade (Melee)    │ ← Weapon type
│ ⚔️  [Melee Icon]             │ ← Visual icon
│ Damage: 180 (Tier 2)         │ ← Current stats
│ Speed: 3/sec                 │ ← Attack speed
│ Effect: Cleave + Bleed       │ ← Abilities
├──────────────────────────────┤
│ ████████░░░░░░░░░░░░░░░░    │ ← Utility bar
│ 2/3 copies for upgrade       │ ← Progress to next tier
└──────────────────────────────┘
```

---

# SECTION 9: WEAPON SCALING & PROGRESSION

## Damage Formula

**Base Formula:**
```
Final Damage = Base Damage × (1 + ATK%) × Blessing Multiplier × Weakness Bonus
```

**Where:**
- Base Damage: Weapon's inherent damage (Sword = 120, Catapult = 150)
- ATK%: Player's attack stat / 100 (each 1 ATK point = 1% damage)
- Blessing Multiplier: All damage-boosting blessings combined (typically 1.1 - 2.5x)
- Weakness Bonus: Damage type advantage vs enemy type (e.g., Fire vs Ice = 1.25x)

**Example Calculation:**
```
Player: Shade with Shadowfang Dagger
- Base Dagger Damage: 100
- Player ATK Stat: 35 → +35% damage multiplier
- Blessing: "Critical Strike Rune" → +15% bonus
- Blessing: "Piercing Enhancement" → +20% bonus
- Enemy Type: Elite (Shade passive) → +40% elite bonus
- Damage Type: Physical vs Melee → 1.0x

Final = 100 × (1 + 0.35) × (1 + 0.15 + 0.20) × 1.40
Final = 100 × 1.35 × 1.35 × 1.40
Final = 255 damage per hit
```

---

## Ally Weapon Scaling

Allies inherit scaling from:

**1. Player ATK Stat (50% inheritance):**
- Ally receives 50% of player's ATK bonus
- If player has 35 ATK (+35%), ally gets +17.5%

**2. Ally Individual Stats:**
- Each ally has its own ATK value
- Base: 80-150 depending on type

**3. Ally Tier Bonus:**
- Tier 1 (Base): 100% damage
- Tier 2 (3 copies merged): +50% damage
- Tier 3 (5 copies merged): +100% damage
- Example: Sword Squire Tier 3 = 120 × 2.0 = 240 base damage

**4. Set Bonus Multiplier:**
- 3x Vanguard Bonus: +12% attack speed
- 5x Vanguard Bonus: +40% attack speed
- These stack multiplicatively with above

**5. Blessing Enhancements:**
- Blessings like "Vanguard Damage Boost" → +X% to vanguard
- Affects ally damage directly
- Example: +20% blessing applies to ally damage

**Combined Ally Damage Example:**
```
Sword Squire (Tier 3)
Base: 120 × 2.0 (tier multiplier) = 240
× Player inheritance (50% of 35% ATK) = 240 × 1.175 = 282
× Blessing bonus (Vanguard +20%) = 282 × 1.20 = 338
× Set bonus (3x Vanguard +12% attack) = 338 × 1.12 = 378 final damage

378 damage per strike at 3 attacks/sec = 1,134 DPS
```

---

## Enemy Weapon Scaling

Enemy weapons scale by:

**1. Difficulty Tier Multiplier:**
| Tier | Name | Multiplier |
|------|------|------------|
| 1 | Novice | 0.6x |
| 2 | Standard | 1.0x |
| 3 | Advanced | 1.3x |
| 4 | Hardcore | 1.6x |
| 5 | Nightmare | 2.0x |
| 6+ | Apocalypse | 2.5x-3.0x+ |

**Example:**
- Bone Shield Guardian Tier 1: 50 damage base × 0.6 = 30 damage
- Bone Shield Guardian Tier 5: 50 damage base × 2.0 = 100 damage

**2. Endless Mode Wave Scaling:**
- After wave 50: +5% damage per wave
- Example: Wave 100 = +250% scaling
- Dread Lich Boss wave 100: 1000 base × 3.5 = 3,500 damage

**3. Number of Enemies:**
- Some enemies appear in groups
- Total threat = per-enemy damage × number of enemies
- Example: 5 Imp Swarms = 60 × 5 = 300 DPS combined

---

## Weapon Rarity Tiers

### Tier 1 (Common) - Level 1-30

**Stats:**
- Base damage: 100-120 range
- 1 attack animation
- No special effects
- Available from start

**Examples:**
- Runeblade (sword)
- Repeating Crossbow (bow)

### Tier 2 (Uncommon) - Level 30-60

**Stats:**
- Base damage: 140-160 range
- Slight animation enhancement
- 1 special effect (bleed, knockback, etc.)
- Unlock condition: Character level 30+

**Examples:**
- Enhanced Runeblade (+bleed effect)
- Upgraded Repeating Crossbow (+piercing)

### Tier 3 (Rare) - Level 60-100

**Stats:**
- Base damage: 180-200 range
- Enhanced animations
- 2 special effects
- Unlock condition: Character level 60+

**Examples:**
- Legendary Runeblade (+bleed + shockwave)
- Precision Repeating Crossbow (+piercing + homing)

### Tier 4 (Legendary) - Prestige Only

**Stats:**
- Base damage: 250+ range
- Unique animations
- 3+ special effects
- Ultimate mechanics
- Unlock condition: Character level 100 + Prestige required

**Examples:**
- Omniscient Runeblade (bleed + shockwave + lifesteal + crits)
- God-Tier Repeating Crossbow (piercing + homing + ricochet + explosive)

---

# SECTION 10: COMBAT FLOW & MECHANICS

## Melee Combat Sequence

**Step-by-Step Flow:**

1. **Positioning Phase**
   - Player moves to 1-2 unit distance from enemy
   - Weapon appears in hand (Runeblade visible)
   - Visual indicator shows ready to attack
   - Time: Instant

2. **Attack Phase**
   - Player initiates attack (click/input)
   - Character plays attack animation (swing motion)
   - Melee weapon swings forward
   - Hit detection occurs during swing
   - Time: 0.3 seconds

3. **Damage Phase**
   - If hit connects: Damage is dealt
   - Calculation: Base × ATK% × Blessings × Weakness
   - Example: 120 × 1.35 × 1.20 = 194 damage
   - Particles burst on hit (sparks, light trails)
   - Sound effect plays (metal clang, impact)
   - Enemy health decreases visibly
   - Time: Instant (on frame of impact)

4. **Recovery Phase**
   - Character returns to neutral stance
   - Must wait for cooldown before next attack
   - Cooldown: Based on weapon speed
   - Blade (3/sec): 0.33 seconds cooldown
   - Hammer (1/sec): 1.0 second cooldown
   - Time: 0.3-1.0 seconds

5. **Loop**
   - Player can move while recovering
   - Next attack available when cooldown expires
   - Repeat cycle until enemy dies or player moves away

---

## Ranged Combat Sequence

**Step-by-Step Flow:**

1. **Positioning Phase**
   - Player stays 5-20 units from enemy
   - Weapon appears (Repeating Crossbow visible)
   - Player aimed toward target
   - Time: Instant

2. **Aim Phase**
   - Player initiates attack
   - Character raises weapon (aiming animation)
   - Targeting reticle appears on screen
   - Time: 0.1 seconds

3. **Fire Phase**
   - Weapon fires (string release/muzzle flash)
   - Projectile spawns at weapon location
   - Projectile travels toward target
   - Visual trail follows projectile
   - Time: 0.05-0.2 seconds (depends on range)

4. **Travel Phase**
   - Projectile moves from source to target
   - Visible in mid-air (clear trajectory)
   - Can be visually tracked
   - Time: 50-200ms (depends on distance)

5. **Impact Phase**
   - Projectile reaches target
   - Collision detection on impact
   - Damage is dealt
   - Calculation: Base × ATK% × Blessings × Weakness
   - Example: 60 × 1.35 × 1.20 × 1.25 = 121.5 damage
   - Impact effect (explosion, spark)
   - Sound effect (blast, impact)
   - Enemy health decreases
   - Time: Instant

6. **Recovery Phase**
   - Character returns to neutral
   - Cooldown before next shot
   - Crossbow (5/sec): 0.2 seconds cooldown
   - Catapult (1/sec): 1.0 second cooldown
   - Time: 0.2-1.0 seconds

7. **Loop**
   - Player can move during attack
   - Next shot available when cooldown expires
   - Repeat until enemy dies

---

## Ally Combat AI

### Melee Ally AI Loop

```
MELEE_ALLY_AI:
1. Calculate distance to nearest enemy
2. IF distance < 2 units:
   a. Play melee attack animation
   b. Deal damage to enemies in range
   c. Wait for attack cooldown
   d. Go back to step 1
3. ELSE (distance >= 2 units):
   a. Move toward nearest enemy
   b. Play walking animation
   c. Go back to step 1
```

**Example: Sword Squire**
```
Frame 1: Nearest enemy is 5 units away
→ Move toward enemy

Frame 60: Now 3 units away
→ Still too far, keep moving

Frame 120: Now 1.5 units away (in range!)
→ Play attack animation

Frame 150: Attack lands
→ Deal 240 damage to enemies in cone (cleave)
→ Wait 0.33 seconds (3/sec attack speed)

Frame 160: Cooldown expires
→ Attack again
→ This time hitting 4 enemies in cone
→ Deal 240 × 4 = 960 total damage
```

### Ranged Ally AI Loop

```
RANGED_ALLY_AI:
1. Calculate distance to nearest enemy
2. IF distance < 20 units AND has line of sight:
   a. Play ranged attack animation
   b. Fire projectile toward enemy
   c. Projectile travels and hits
   d. Deal damage on impact
   e. Wait for attack cooldown
   f. Go back to step 1
3. ELSE:
   a. Move closer to nearest enemy
   b. Play walking animation
   c. Go back to step 1
```

**Example: Ballista Crew**
```
Frame 1: Nearest enemy is 12 units away (in range!)
→ Play attack animation

Frame 30: Fire projectile
→ Projectile travels 12 units
→ Takes 150ms to reach target

Frame 180: Projectile impacts
→ Deal 150 damage
→ Create explosion in 5m radius
→ Hits 3 nearby enemies
→ Total damage: 450 (150 × 3)
→ Wait 1.0 second (1/sec attack speed)

Frame 270: Cooldown expires
→ Attack again
→ Fire new projectile
```

---

## Special Combat Situations

### Melee vs Ranged Enemy

**Scenario:** Player using melee weapon vs Ranged enemy (Volley Archer)

1. **Player Approach Phase**
   - Player moves toward enemy (5+ units away)
   - Enemy starts shooting (5/sec fire rate)
   - Player takes damage during approach
   - Tension: Getting hit while moving in

2. **Close Range Phase**
   - Player reaches 1-2 unit range
   - Enemy can still shoot (no range limit on ranged)
   - Player now in melee range
   - Both can attack each other
   - Player melee faster (3/sec vs enemy's 5/sec)

3. **Advantage**
   - Player melee damage > enemy ranged damage per hit
   - 120 × 3 = 360 DPS player
   - 30 × 5 = 150 DPS enemy
   - But enemy is still dangerous (still dealing damage)

4. **Strategy**
   - Get close quickly to minimize damage taken during approach
   - Use knockback melee to keep enemy in range
   - Use defensive blessings to reduce damage during approach

---

### Ranged vs Melee Enemy

**Scenario:** Player using ranged weapon vs Melee enemy (Bone Shield Guardian)

1. **Safe Distance Phase**
   - Player stands 10+ units away (safe range)
   - Enemy walks toward player
   - Player shoots projectiles (safe)
   - Enemy takes several seconds to reach

2. **Engagement Phase**
   - Enemy closes distance (1-2 units)
   - Player can still shoot (ranged works at all ranges)
   - Enemy attacks with melee (limited to close range)
   - Both deal damage

3. **Advantage**
   - Player can maintain distance (ranged weapon)
   - Keep enemy at range while dealing damage
   - Enemy can't chase effectively if moving away
   - Ranged damage output scales with distance advantage

4. **Strategy**
   - Maintain 5-15 unit distance (optimal range)
   - Keep moving backward while attacking
   - Use knock projectiles to push enemy away
   - Avoid being cornered (need space to retreat)

---

# SECTION 11: IMPLEMENTATION DETAILS

## Weapon Visual Rendering

### Melee Weapon Rendering

**In 3D Space:**
1. Character hand position (right hand for most weapons)
2. Weapon model spawns in hand
3. Weapon follows hand movement
4. During attack: Hand and weapon swing together
5. Weapon trails: Light effect follows weapon path
6. Impact effects: Particles burst on hit

**Example: Runeblade**
```
Character model → Right hand position
→ Runeblade model attaches to hand
→ Blade position = hand + offset (0.2, 0.1, 0)
→ On attack animation:
  → Hand swings forward (0.5 seconds)
  → Blade follows hand
  → Blade creates light trail (red/orange)
  → On frame 10 (impact frame):
    → Particle effect spawns at blade tip
    → Sparks burst outward
    → Sound effect plays
```

### Ranged Weapon Rendering

**In 3D Space:**
1. Character hand position or weapon mount
2. Weapon model (crossbow, catapult, staff)
3. Weapon rotation toward target
4. During attack: Weapon aims/fires
5. Projectile spawns at barrel/tip
6. Projectile travels with trail effect

**Example: Repeating Crossbow**
```
Character model → Right side position
→ Repeating Crossbow model attaches to side
→ Crossbow position = torso + offset
→ On attack animation:
  → Character aims (slight rotation)
  → Crossbow rotates to target
  → On frame 5 (fire frame):
    → String release effect at bow tip
    → Bolt spawns at crossbow tip
    → Bolt travels at constant speed
    → Bolt creates trail (blue/cyan)
    → Sound effect plays (string twang)
```

---

## Animation State Machine

### Simple Animation States

```
MELEE_WEAPON_STATES:
├─ IDLE: Standing ready (default)
│  └─ Transitions to: ATTACK
├─ ATTACK: Swing animation playing
│  ├─ Frame 0-5: Wind-up
│  ├─ Frame 6-10: Strike (HIT DETECTION HERE)
│  ├─ Frame 11-15: Recovery
│  └─ Transitions to: RECOVERY when attack finishes
├─ RECOVERY: Cooldown in progress
│  └─ Transitions to: IDLE when cooldown expires
└─ MOVING: Walking animation
   └─ Transitions to: IDLE when stopped

RANGED_WEAPON_STATES:
├─ IDLE: Standing ready (default)
│  └─ Transitions to: AIM
├─ AIM: Aiming animation
│  └─ Transitions to: FIRE
├─ FIRE: Firing animation
│  ├─ Frame 0-3: Aim
│  ├─ Frame 4: Fire (PROJECTILE SPAWNS)
│  └─ Transitions to: RECOVERY
├─ RECOVERY: Cooldown
│  └─ Transitions to: IDLE when cooldown expires
└─ MOVING: Walking animation
   └─ Transitions to: IDLE when stopped
```

---

## Ally Weapon Logic in Code

**Pseudocode for ally attack:**

```python
class Ally:
    def __init__(self, weapon_type):
        self.weapon_type = weapon_type  # "blade", "catapult", "staff"
        self.current_target = None
        self.cooldown_remaining = 0
        self.in_combat = False
    
    def update(self, delta_time):
        # Reduce cooldown
        if self.cooldown_remaining > 0:
            self.cooldown_remaining -= delta_time
        
        # Find nearest enemy
        enemies_in_range = find_enemies_within(20)  # 20 unit range
        if enemies_in_range:
            self.current_target = enemies_in_range[0]  # Closest enemy
            self.in_combat = True
            self.attack()
    
    def attack(self):
        if self.cooldown_remaining <= 0:  # Ready to attack
            if self.weapon_type == "blade":
                self.melee_attack()
            elif self.weapon_type == "catapult":
                self.ranged_attack()
            elif self.weapon_type == "staff":
                self.support_attack()
    
    def melee_attack(self):
        # Calculate distance to target
        distance = calculate_distance(self, self.current_target)
        
        if distance < 2:  # In melee range
            # Determine damage
            damage = self.base_damage * self.player.attack_multiplier
            
            # Check if cleave hits multiple enemies
            enemies_in_cone = find_enemies_in_cone(self, angle=120, radius=2)
            for enemy in enemies_in_cone:
                enemy.take_damage(damage, self.weapon_type)
            
            # Apply status effect if any
            if self.weapon_tier >= 2:
                apply_bleed(enemies_in_cone, 10, 8)  # 10 DMG/sec, 8 seconds
            
            # Set cooldown
            self.cooldown_remaining = 1.0 / 3.0  # 3 attacks/sec = 0.33 sec between
        else:
            # Walk toward target
            move_toward(self.current_target)
    
    def ranged_attack(self):
        distance = calculate_distance(self, self.current_target)
        
        if distance < 20:  # In ranged range
            # Fire projectile
            projectile = Projectile(
                start_pos=self.position,
                end_pos=self.current_target.position,
                damage=self.base_damage,
                weapon_type="catapult",
                effect="explosion"
            )
            projectiles.append(projectile)
            
            # Set cooldown
            self.cooldown_remaining = 1.0  # 1 attack/sec = 1.0 sec between
        else:
            # Move closer
            move_toward(self.current_target)
```

---

# SECTION 12: ARCANE RELICS SYSTEM

## What Are Relics?

Relics are powerful magical artifacts purchased from the shop between waves. Each relic provides permanent passive effects for the duration of the run. Players collect relics over many waves to build powerful synergies.

**Key Properties:**
- **Permanent for the Run:** Once bought, relic effect lasts entire run
- **Stack with Others:** Multiple relics combine effects
- **Rarity Tiers:** Common (gray) → Uncommon (green) → Rare (blue) → Legendary (gold)
- **Purchase Cost:** Scales with rarity (10/25/50/100 gold)
- **Shop Refresh:** Reroll shop for increasing cost (5/10/20 gold per refresh)
- **Synergize with Champions/Allies/Blessings:** Some relics enable specific build paths

---

## COMMON RELICS (10 Gold - 8 Relics)

Simple stat bonuses. Foundation pieces.

### 1. Iron Amulet
- **Effect:** +5 ATK
- **Lore:** A simple iron pendant blessed by the Brigade's chaplain.
- **Best With:** Any offensive Champion (Bron, Shade)

### 2. Leather Bracers
- **Effect:** +10 Max HP
- **Lore:** Worn leather forearm guards from a fallen knight.
- **Best With:** Tank builds (Garrick, Ulric)

### 3. Swift Boots
- **Effect:** +3 Movement Speed
- **Lore:** Light boots enchanted with a minor wind charm.
- **Best With:** Mobile Champions (Zephyr, Ronan)

### 4. Apprentice Tome
- **Effect:** +1 Mana per Blessing equipped
- **Lore:** A novice mage's first spellbook.
- **Best With:** Arcanus (Spellweaver)

### 5. Hunter's Sigil
- **Effect:** +5% Critical Hit Chance
- **Lore:** A hunter's mark carved into bone.
- **Best With:** Aldric, Falka

### 6. Knight's Pendant
- **Effect:** +5% damage to enemies above 75% HP
- **Lore:** Pendant of the First Strike Order.
- **Best With:** Burst damage builds

### 7. Wood Charm
- **Effect:** +1 Gold per wave completed
- **Lore:** A carved oak leaf, supposedly lucky.
- **Best With:** Cassia (Collector)

### 8. Healer's Wrap
- **Effect:** Heal 1 HP per second
- **Lore:** Bandages soaked in restoration potion.
- **Best With:** Long survival builds

---

## UNCOMMON RELICS (25 Gold - 8 Relics)

Conditional effects. Begin enabling build paths.

### 9. Lightning Rod
- **Effect:** Every 5th attack chains lightning to 3 nearby enemies (50 Arcane damage)
- **Lore:** A copper rod humming with stored storm magic.
- **Best With:** Fast attackers (Ronan, Zephyr)

### 10. Healing Crystal
- **Effect:** Heal 5 HP per second + 20 HP on wave clear
- **Lore:** A glowing geode pulled from a Mystic vein.
- **Best With:** Survival builds, low-HP Champions

### 11. Berserker Gauntlet
- **Effect:** +2% damage per 5% missing HP (max +40%)
- **Lore:** Spiked gauntlet from a Bloodfury barbarian.
- **Best With:** Bron (Berserker)

### 12. Lucky Coin
- **Effect:** 15% chance to double gold drops from enemies
- **Lore:** A coin from a god of fortune.
- **Best With:** Economy builds, Cassia

### 13. Battle Standard
- **Effect:** All allies gain +10% ATK when Champion is in combat
- **Lore:** Banner of an old Brigade chapter.
- **Best With:** Ally-heavy builds, Ulric (Paladin)

### 14. Shield of Reflection
- **Effect:** 20% chance to reflect 50% of damage taken back to attacker
- **Lore:** Mirror-polished shield of a fallen knight.
- **Best With:** Tank builds (Garrick)

### 15. Mage's Tome of Focus
- **Effect:** +20% Arcane damage, but -10% Physical damage
- **Lore:** A focused mage's grimoire.
- **Best With:** Morgath, Lyra

### 16. Vampire's Fang
- **Effect:** Lifesteal 8% of damage dealt (melee only)
- **Lore:** Tooth of an ancient vampire lord.
- **Best With:** Melee Champions (Bron, Shade)

---

## RARE RELICS (50 Gold - 8 Relics)

Powerful effects that transform playstyle.

### 17. Dragon's Heart
- **Effect:** +30 Max HP, +15% Fire damage, immune to Burn
- **Lore:** The still-warm heart of a slain wyrm.
- **Best With:** Fire-focused builds, tanks

### 18. Frost Crown
- **Effect:** Attacks have 20% chance to Freeze enemies for 1.5 seconds
- **Lore:** Crown of the Frostbound Queen.
- **Best With:** Control builds, slow-attacking weapons

### 19. Time Hourglass
- **Effect:** Reduces all ability cooldowns by 25% + Ultimates charge 50% faster
- **Lore:** A relic from before time itself.
- **Best With:** Ultimate-focused builds

### 20. Holy Symbol of the Sun
- **Effect:** +30% damage to undead and demons + heal 5 HP on undead/demon kill
- **Lore:** Forged in the first dawn.
- **Best With:** Vs Dread Lich, vs Voidlord boss

### 21. Storm Caller's Rod
- **Effect:** Every 10 seconds, lightning strikes nearest enemy for 250 Arcane damage
- **Lore:** Channeling rod of the Stormwarden.
- **Best With:** Passive damage builds

### 22. Mirror Cape
- **Effect:** Block the first hit per wave with no damage taken
- **Lore:** Cape woven from mirror-silk.
- **Best With:** Glass cannon Champions (Bron, Pulse-style)

### 23. Soul Reaper's Brand
- **Effect:** 8% chance per kill to instantly fully recharge Ultimate
- **Lore:** A brand burned into the soul of a reaper.
- **Best With:** Ultimate-heavy Champions (Aldric, Auron)

### 24. Wyrmscale Plate
- **Effect:** -20% damage taken, but -10% movement speed
- **Lore:** Armor scaled from a dragon's hide.
- **Best With:** Tank Champions (Garrick, Ulric)

---

## LEGENDARY RELICS (100 Gold - 6 Relics)

Game-changing artifacts. Run-defining choices.

### 25. Excalibur Shard
- **Effect:** Critical hits deal +200% damage. Crits also generate +5 Ultimate charge.
- **Lore:** A fragment of the True Blade, drawn only by the worthy.
- **Best With:** Crit builds (Aldric, Falka)
- **Visual:** Glowing golden shard, embedded in weapon hilt

### 26. Crown of Thorns
- **Effect:** Take +30% damage, but all allies deal +60% damage and you deal +30%
- **Lore:** Crown of the Martyr-King who bled for his people.
- **Best With:** Brigade-focused builds, Ulric, Aurelius
- **Visual:** Thorny crown that draws blood

### 27. Worldtree Seed
- **Effect:** Heal 8% Max HP every 5 seconds. Allies heal 4% Max HP every 5 seconds.
- **Lore:** A seed from the World Tree at the center of all realms.
- **Best With:** Long endurance runs
- **Visual:** Glowing green seed sprouting tiny leaves

### 28. Void Eye Pendant
- **Effect:** Reveal next 2 enemy waves. See all shop items before purchase. +1 free shop reroll per wave.
- **Lore:** A pendant containing a captured eye of the Voidlord.
- **Best With:** Seraphina, planning builds
- **Visual:** Purple pendant with a slit-pupil eye

### 29. Star Forge Hammer (Weapon Relic)
- **Effect:** All blunt weapons (hammers/maces) ignore 100% enemy armor and deal +50% damage to armored enemies
- **Lore:** Hammer forged in the heart of a dying star.
- **Best With:** Arcanus, Ulric (hammer wielders)
- **Visual:** Hammer head wreathed in starlight

### 30. Phoenix Crown
- **Effect:** Revive twice per run with 100% HP. After each revive, gain +30% damage for the rest of the run (stacks).
- **Lore:** Crown forged from phoenix feathers and eternal flame.
- **Best With:** Risky builds, any Champion
- **Visual:** Crown of flickering fire feathers

---

## Relic Synergies (Build Examples)

### Crit Storm Build (Aldric + Falka)
- Excalibur Shard (legendary)
- Hunter's Sigil (common +crit)
- Lightning Rod (uncommon chain)
- Storm Caller's Rod (rare passive lightning)
- **Result:** Massive crit storm with constant lightning chains

### Vampire Lord Build (Bron)
- Vampire's Fang (uncommon lifesteal)
- Berserker Gauntlet (uncommon low-HP damage)
- Wyrmscale Plate (rare damage reduction)
- Phoenix Crown (legendary revives)
- **Result:** Glass cannon that never dies, heals from kills

### Brigade Commander Build (Ulric)
- Battle Standard (uncommon ally boost)
- Crown of Thorns (legendary martyr buff)
- Worldtree Seed (legendary heal)
- Holy Symbol (rare anti-undead)
- **Result:** Allies dominate while Champion tanks

### Ultimate Machine Build (Auron)
- Soul Reaper's Brand (rare ult on kill)
- Time Hourglass (rare cooldown reduction)
- Excalibur Shard (legendary crit + ult charge)
- **Result:** Ultimate available almost every fight

---

## Relic Shop Mechanics

**Shop Refresh System:**
- After each wave, shop appears with 3 random relics + 2 random blessings
- Refresh available for cost: 5g (1st refresh), 10g (2nd), 20g (3rd+ resets per wave)
- Cassia (Champion) sees 5 relics instead of 3
- Seraphina (Champion) reveals 2 extra shop slots

**Relic Locking:**
- Players can "lock" a relic to keep it in the shop next wave (5 gold cost)
- Lets player save for an expensive legendary

**Forbidden Relics (Curse Mechanic):**
- Some relics have negative + positive effects
- E.g., Crown of Thorns: take +30% damage but allies deal +60%
- Encourages risky decision-making


---

# SECTION 13: BLESSINGS & SPELLS SYSTEM

## What Are Blessings?

Blessings are active magical enchantments cast on your Brigade. Unlike Relics (purely passive), Blessings define your strategic build. They drop after waves and can be equipped to your Champion. Each Champion has limited Blessing slots (3-6 depending on character).

**Key Properties:**
- **Equipped, Not Permanent:** Can be unequipped/replaced
- **Slot-Based:** Limited slots force strategic choice
- **Synergize with Champion Passives:** Some blessings enhance specific Champions
- **Categorized:** 4 main schools

**Blessing Slot Counts by Champion:**
- Aldric, Cassia, Mira: 4 slots (balanced)
- Arcanus: 6 slots (Spellweaver — most slots)
- Bron, Shade: 3 slots (offensive — fewer slots, raw power)
- Ulric, Garrick: 5 slots (tanky)
- Auron (High King): 5 slots + 40% cost reduction

---

## Blessing Schools (4 Categories)

| School | Color | Theme | Best For |
|--------|-------|-------|----------|
| **Battle Blessings** | Red | Direct damage, attack speed, crits | Offensive builds |
| **Ward Blessings** | Blue | HP, defense, blocks, barriers | Tank builds |
| **Mystic Blessings** | Purple | Utility, gold, ally upgrades | Economy/support |
| **Curse Blessings** | Green | Debuffs on enemies, DOTs, control | Control builds |

---

## BATTLE BLESSINGS (Red - Offensive)

### 1. Bladestorm
- **Cost:** 30 gold
- **Effect:** +20% melee weapon damage
- **Synergy:** Sword/Hammer Champions (Aldric, Arcanus)

### 2. Eagle Eye
- **Cost:** 30 gold
- **Effect:** +20% ranged weapon damage
- **Synergy:** Ranged Champions (Falka, Drakar, Ronan)

### 3. Crimson Edge
- **Cost:** 50 gold
- **Effect:** +15% damage to bleeding enemies
- **Synergy:** Cleaving weapons (Sword Squire)

### 4. Soul Burn
- **Cost:** 50 gold
- **Effect:** Critical hits ignite enemies for 100 Fire damage over 3s
- **Synergy:** Crit builds

### 5. Heavy Strike
- **Cost:** 40 gold
- **Effect:** Every 4th attack deals +150% damage
- **Synergy:** Slow weapons (Hammers, Catapults)

### 6. Berserker's Howl
- **Cost:** 75 gold
- **Effect:** +30% ATK below 50% HP
- **Synergy:** Bron (Berserker)

### 7. Holy Smite
- **Cost:** 60 gold
- **Effect:** Attacks deal +25% damage to enemies with armor
- **Synergy:** Vs Bone Shield Guardian, vs Dark Knight

---

## WARD BLESSINGS (Blue - Defensive)

### 8. Stone Skin
- **Cost:** 30 gold
- **Effect:** +15% damage reduction from all sources
- **Synergy:** All tanks

### 9. Sanctuary
- **Cost:** 40 gold
- **Effect:** Heal 5 HP for every enemy killed within 3 units
- **Synergy:** Melee Champions

### 10. Mirror Image
- **Cost:** 50 gold
- **Effect:** 25% chance to dodge incoming attacks
- **Synergy:** Light Champions (Zephyr, Shade)

### 11. Iron Bulwark
- **Cost:** 60 gold
- **Effect:** +50 Max HP, but -5% Movement Speed
- **Synergy:** Tank Champions (Garrick, Ulric)

### 12. Phoenix Resurrection
- **Cost:** 100 gold
- **Effect:** Once per wave, revive with 50% HP instead of dying
- **Synergy:** Glass cannon builds

### 13. Light's Embrace
- **Cost:** 50 gold
- **Effect:** Heal 50 HP when picking up gold
- **Synergy:** Cassia (Collector), economy builds

### 14. Guardian's Vow
- **Cost:** 75 gold
- **Effect:** -20% damage taken when allies are within 5 units
- **Synergy:** Ulric, Brigade-focused builds

---

## MYSTIC BLESSINGS (Purple - Utility)

### 15. Coin Charm
- **Cost:** 20 gold
- **Effect:** +25% gold from all sources
- **Synergy:** Cassia, economy

### 16. Arcane Insight
- **Cost:** 30 gold
- **Effect:** +1 free shop reroll per wave
- **Synergy:** RNG-dependent builds

### 17. Squire's Training
- **Cost:** 40 gold
- **Effect:** Allies require -1 copy to upgrade tiers (3→2 for Tier 2)
- **Synergy:** Ally-heavy builds

### 18. Time Slip
- **Cost:** 50 gold
- **Effect:** Ultimate cooldown -30%
- **Synergy:** Ultimate-focused Champions

### 19. Beast Bond
- **Cost:** 40 gold
- **Effect:** Wolf Companion, Owl Familiar, and Beast Tamer's pets gain +50% damage
- **Synergy:** Beast-themed Specialist builds

### 20. Mass Resurrection
- **Cost:** 100 gold
- **Effect:** All allies who fall return at start of next wave with +20% damage
- **Synergy:** Risky aggressive builds

### 21. Scholar's Wisdom
- **Cost:** 35 gold
- **Effect:** +10 XP per kill (faster Champion leveling for future runs)
- **Synergy:** Account progression

---

## CURSE BLESSINGS (Green - Debuffs)

### 22. Weakening Hex
- **Cost:** 30 gold
- **Effect:** Hit enemies deal -15% damage for 5s
- **Synergy:** Survival builds

### 23. Plague Touch
- **Cost:** 40 gold
- **Effect:** Attacks apply Poison: 20 DMG/sec for 4 seconds (stacks 3x)
- **Synergy:** Fast attackers (Wolf Companion combo)

### 24. Mass Confusion
- **Cost:** 50 gold
- **Effect:** Hit enemies have 20% chance to attack each other for 3s
- **Synergy:** Crowd control builds

### 25. Soul Drain
- **Cost:** 60 gold
- **Effect:** Killed enemies leave a soul wisp that heals you 5 HP if collected
- **Synergy:** Mobile Champions (Zephyr)

### 26. Marking Curse
- **Cost:** 45 gold
- **Effect:** First enemy hit per wave gets marked: takes +50% damage from all sources
- **Synergy:** Focus-target builds

### 27. Voidlord's Whisper
- **Cost:** 100 gold
- **Effect:** Enemies you damage take +30% damage from your allies for 5s
- **Synergy:** Brigade builds with strong allies

### 28. Banishment
- **Cost:** 80 gold
- **Effect:** Every 30 seconds, the lowest-HP enemy on screen is instantly banished (killed)
- **Synergy:** Survival builds vs swarms

---

## Blessing Stacking & Limits

**Stack Rules:**
- Same-named blessings cannot be equipped twice
- Different blessings of same school stack
- Blessings can be sold for 50% of purchase price

**Equip/Swap Mechanics:**
- Swap blessings during shop phase (not during waves)
- Each swap during a wave costs 5 gold (penalty)

**School Mastery Bonus:**
- Equip 3+ blessings of same school: gain +5% effectiveness of that school
- Equip 5+ blessings of same school: gain +15% effectiveness + unique passive
  - **Battle Mastery:** Critical hits stun for 0.5s
  - **Ward Mastery:** Damage reduction also reflects 10% damage
  - **Mystic Mastery:** Shop refresh free once per wave
  - **Curse Mastery:** Debuffs last twice as long


---

# SECTION 14: STATUS EFFECTS & ELEMENTAL REACTIONS

## Overview

Status effects are temporary debuffs/buffs applied to entities. They create strategic depth: players choose builds around applying specific effects, while enemies threaten different conditions.

**Two Categories:**
1. **Player-Applied (Offensive)** - Things you do to enemies
2. **Enemy-Applied (Threats)** - Things enemies do to you

---

## OFFENSIVE STATUS EFFECTS (Applied to Enemies)

### Fire School

#### 🔥 BURN
- **Source:** Fire damage attacks, Fire weapons, Soul Burn blessing
- **Effect:** Damage Over Time - 20 Fire DMG/sec for 4 seconds
- **Stacks:** Up to 3 times (60 DMG/sec)
- **Visual:** Flame particle effect surrounding enemy
- **Counter:** Some enemies are Fire-resistant (Dragon, Volcanic enemies)

#### 🔥 SCORCH
- **Source:** Critical fire attacks
- **Effect:** Enemy takes +25% damage from all sources for 5 seconds
- **Stacks:** No (refreshes)
- **Visual:** Glowing orange aura
- **Counter:** Cleanse spells

### Holy School

#### ✨ MARK OF JUDGMENT
- **Source:** Holy attacks, Marking Curse blessing
- **Effect:** Marked enemy takes +50% damage from next attack
- **Stacks:** No (consumed on damage)
- **Visual:** Glowing cross-mark above enemy head
- **Counter:** Dark Knight enemies are Holy-resistant

#### ✨ EXPOSE
- **Source:** Holy crits, Smite blessings
- **Effect:** Enemy armor reduced by 100% (revealed weakness)
- **Stacks:** No
- **Visual:** Cracked armor visual
- **Counter:** Light immunity (only some bosses)

### Physical School

#### 🩸 BLEED
- **Source:** Sword/dagger attacks, Bladestorm blessing
- **Effect:** 15 Physical DMG/sec for 5 seconds, moving increases damage by 50%
- **Stacks:** Up to 5 times
- **Visual:** Red blood drip effect
- **Counter:** Undead/Demon enemies (no blood)

#### 💥 STUN
- **Source:** Hammer attacks, Heavy Strike blessing
- **Effect:** Enemy cannot attack or move for 1.5 seconds
- **Stacks:** No (immune after stun for 3 seconds)
- **Visual:** Stars circling head
- **Counter:** Boss-tier enemies are stun-resistant (50% duration)

#### 💢 ARMOR BREAK
- **Source:** Drill weapons, pickaxe attacks (Tunnel Goblin), some Champion abilities
- **Effect:** -30% enemy armor per stack (max -90%)
- **Stacks:** Up to 3 times
- **Visual:** Cracking armor texture
- **Counter:** Re-armoring abilities (some Mirror Sorcerer skills)

### Arcane School

#### 🌀 HEX
- **Source:** Arcane attacks, Weakening Hex blessing
- **Effect:** Enemy deals -25% damage for 5 seconds
- **Stacks:** No (refreshes)
- **Visual:** Purple swirling aura
- **Counter:** Anti-magic enemies (Silencing Wraith resists)

#### ❄️ FREEZE
- **Source:** Frost Crown relic, Time Mage ally
- **Effect:** Enemy immobilized for 1.5 seconds, takes +50% damage from Physical attacks
- **Stacks:** No (immunity after 3 seconds)
- **Visual:** Encased in ice crystals
- **Counter:** Fire damage instantly breaks freeze

#### 🐌 SLOW
- **Source:** Wind Sprite reverse (enemy version), Time Mage ally
- **Effect:** -50% movement and attack speed for 4 seconds
- **Stacks:** No (refreshes)
- **Visual:** Blue translucent overlay
- **Counter:** None (always applies)

#### 😱 FEAR
- **Source:** Specialist abilities (Trickster), some Legendary relics
- **Effect:** Enemy flees from player for 2 seconds (cannot attack)
- **Stacks:** No (immunity 5 seconds)
- **Visual:** Question mark above head, running animation
- **Counter:** Boss enemies cannot be feared

---

## DEFENSIVE STATUS EFFECTS (Applied to Champion/Allies)

#### 🛡️ BLESS
- **Source:** Battle Bard ally, Sanctuary blessing
- **Effect:** +20% damage, +20% movement for 10 seconds
- **Stacks:** No (refreshes)
- **Visual:** Golden glow around character

#### 🌿 REGENERATION
- **Source:** Light Cleric ally, Worldtree Seed relic
- **Effect:** Heal 3% Max HP per second
- **Stacks:** Yes (sources add)
- **Visual:** Green leaf particles

#### ⚡ HASTE
- **Source:** Wind Sprite ally, Time Slip blessing
- **Effect:** +30% attack speed and movement speed
- **Duration:** 6 seconds
- **Visual:** Blue speed trails

#### 🔮 BARRIER
- **Source:** Mirror Spirit, Holy Bulwark ultimate
- **Effect:** Absorb next X damage (e.g., 100 HP shield)
- **Stacks:** Yes (multiple barriers)
- **Visual:** Translucent blue bubble

#### 👻 STEALTH
- **Source:** Shade's Ultimate, some Trickster abilities
- **Effect:** Invisible to enemies for X seconds, first attack auto-crits
- **Visual:** Transparent character outline

---

## ENEMY-APPLIED THREATS (What Hurts You)

#### 🔥 BURNING
Same as Burn above. Imp Swarm and Inferno Dragon apply.

#### ❄️ FROZEN
Same as Freeze. Frost Ogre applies (rare).

#### 🩸 BLEEDING
Same as Bleed. Wolf-type enemies apply.

#### 😨 SILENCED
- **Source:** Silencing Wraith melee attack
- **Effect:** Cannot use Ultimate or Blessings for 4 seconds
- **Visual:** Crossed-out spell icon
- **Counter:** Anti-magic resistance, kill source quickly

#### 🎯 MARKED (Enemy version)
- **Source:** Hunter Wraith prediction shots
- **Effect:** Next attack will track you (cannot dodge)
- **Visual:** Crosshair on character
- **Counter:** Break line of sight, hide behind obstacles

#### 🐌 ENSNARED
- **Source:** Spider Queen webs, Vine Hazard
- **Effect:** Cannot move for 2 seconds
- **Visual:** Webbing on legs
- **Counter:** Cleanse, fire damage burns webs

#### 💀 CURSED
- **Source:** Mimic Fiend, Voidlord boss
- **Effect:** -50% healing received for 8 seconds
- **Visual:** Black aura
- **Counter:** Holy damage removes

---

## ELEMENTAL REACTIONS (Damage Type Combos)

When two damage types hit an enemy in quick succession (within 2 seconds), they create special reactions:

### 🔥 + 🌀 = COMBUSTION
- **Fire + Arcane**
- **Effect:** Massive AOE explosion (300 damage in 4m radius)
- **Best With:** Morgath + Fire ally
- **Cooldown:** 3 seconds per enemy

### 🔥 + ✨ = PURGATION
- **Fire + Holy**
- **Effect:** Bonus +100% damage to undead/demons
- **Best With:** Vs Dread Lich, vs Bone Shield Guardian
- **Cooldown:** None (always applies)

### ✨ + 🌀 = SANCTIFICATION
- **Holy + Arcane**
- **Effect:** Heal all allies in 5m radius for 50 HP
- **Best With:** Brigade healing builds
- **Cooldown:** 5 seconds per cast

### ✨ + ⚔️ = CRUSADER STRIKE
- **Holy + Physical**
- **Effect:** Armor break + 50 bonus Holy damage
- **Best With:** Knight Champions (Aldric, Ulric, Aurelius)
- **Cooldown:** None (always applies)

### ⚔️ + 🌀 = DISENCHANT
- **Physical + Arcane**
- **Effect:** Removes one enemy buff + deals damage equal to buff potency
- **Best With:** Vs Mirror Sorcerer (removes reflect), vs Mimic Fiend
- **Cooldown:** 4 seconds per enemy

### ⚔️ + 🔥 = FORGE HEAT
- **Physical + Fire**
- **Effect:** Champion's next 3 attacks deal +50% damage
- **Best With:** Sword/Hammer builds
- **Cooldown:** 6 seconds

---

## Damage Type Effectiveness Matrix

| Damage Type | Strong Against | Weak Against | Neutral |
|-------------|----------------|--------------|---------|
| 🔥 Fire | Ice enemies, Imps, plant enemies | Dragons, Fire elementals | Most |
| ✨ Holy | Undead, Demons, Mirror enemies | Light beings, Holy bosses | Most |
| ⚔️ Physical | Tunnel Goblin (drills), Volley Archer (strings) | Stone Golem, Bone Guardian | Most |
| 🌀 Arcane | Silencing Wraith (anti-magic), Mimic Fiend | Voidlord, Anti-magic bosses | Most |

**Damage Multipliers:**
- Strong: 1.5x damage
- Weak: 0.6x damage
- Neutral: 1.0x damage

**Strategy Implication:**
Champions specializing in one damage type can become locked out vs certain enemies. Mixing types (via Blessings, Relics, or Ally compositions) is rewarded.


---

# SECTION 15: CHAMPION ULTIMATE ABILITIES

## Overview

Every Champion possesses one signature **Ultimate Ability** — a powerful active skill that defines their identity. Ultimates charge during combat and provide game-changing effects when unleashed.

**Ultimate Charging:**
- Filled by dealing damage, taking damage, killing enemies, and completing waves
- Wave clear: +30% Ultimate charge
- Kill: +5% per enemy, +20% per elite, +50% per boss
- Critical hits: +2% extra charge
- Visual: Glowing aura around Champion when ready

**Activation:**
- Press dedicated Ultimate button (R/Triangle/Y)
- Brief cinematic flash (0.5s)
- Effect applies
- Charge resets to 0%

---

## STARTER TIER ULTIMATES

### 🌟 Aldric — *Knight's Rally*
- **Effect:** All allies and Champion gain +25% ATK and +20% Movement Speed for 8 seconds
- **AOE:** Self + all allies on screen
- **Visual:** Brigade banner unfurls behind Aldric, golden light radiates outward
- **Tactical Use:** Pre-wave burst, save for elites
- **Charge Rate:** Standard

### 💨 Zephyr — *Wind Step*
- **Effect:** Teleport behind nearest enemy (up to 15 units away). Next attack auto-crits with +200% damage and applies Stun
- **AOE:** Single target teleport
- **Visual:** Wind vortex at start/end positions, leaves blue trail
- **Tactical Use:** Assassinate Volley Archers or back-line ranged enemies
- **Charge Rate:** Fast (Zephyr passive +20%)

### 💰 Cassia — *Treasure Burst*
- **Effect:** Drop 50 gold + 1 random Uncommon Relic on the field (Champion picks up)
- **AOE:** Self
- **Visual:** Treasure chest bursts from ground, coins spray
- **Tactical Use:** Mid-run economy boost
- **Charge Rate:** Slow (powerful effect)

---

## TIER 2 ULTIMATES

### 📜 Arcanus — *Arcane Cascade*
- **Effect:** Instantly triggers the basic effect of every equipped Blessing simultaneously (e.g., heals from Sanctuary, damage from Bladestorm targets, gold from Coin Charm)
- **AOE:** Varies by blessings
- **Visual:** Arcane runes spiral around Champion, each Blessing's icon flashes
- **Tactical Use:** Combo with full Blessing slots (6 slots)
- **Charge Rate:** Slow

### 🗡️ Shade — *Shadow Pact*
- **Effect:** Become invisible for 6 seconds. During invisibility, all attacks auto-crit and execute enemies below 50% HP (instant kill, non-bosses)
- **AOE:** Self
- **Visual:** Dissolves into black smoke, reforms in shadow
- **Tactical Use:** Clear elite waves, finish bosses below threshold
- **Charge Rate:** Standard

### 🔥 Bron — *Bloodfury*
- **Effect:** +100% ATK and +50% Attack Speed for 6 seconds. Champion takes +50% damage during.
- **AOE:** Self
- **Visual:** Bron's body glows red, fangs grow, eyes turn crimson
- **Tactical Use:** Quick boss damage but high risk
- **Charge Rate:** Fast (Bron passive)

### 🌀 Morgath — *Mass Curse*
- **Effect:** Applies Hex to all enemies on screen for 8 seconds (-25% damage, +50% damage taken)
- **AOE:** All enemies
- **Visual:** Dark cloud erupts from Morgath, every enemy briefly outlined in purple
- **Tactical Use:** Open hard waves, soften bosses
- **Charge Rate:** Standard

### 👯 Mira — *Duplicate Self*
- **Effect:** Spawn 3 clones of Champion that mimic attacks at 50% damage for 10 seconds. Clones cannot die.
- **AOE:** Self spawns clones
- **Visual:** Mirror images split from Mira, each slightly translucent
- **Tactical Use:** 4x effective DPS, screen control
- **Charge Rate:** Slow (powerful)

---

## TIER 3 ULTIMATES

### 🏹 Falka — *Arrow Storm*
- **Effect:** Rain 30 arrows over 3 seconds in a 5m radius. Each arrow deals 80 damage and can crit. Costs no ammunition.
- **AOE:** Target area
- **Visual:** Sky darkens, arrows rain down with whistling sound
- **Tactical Use:** Wave clear, grouped enemies
- **Charge Rate:** Standard

### 💥 Drakar — *Volcanic Bombardment*
- **Effect:** Calls down 3 massive siege strikes over 4 seconds. Each strike deals 400 damage in 6m radius and applies Burn.
- **AOE:** 3 separate target areas
- **Visual:** Sky cracks open, flaming boulders crash down
- **Tactical Use:** Boss + adds, dense groups
- **Charge Rate:** Slow

### 🛡️ Garrick — *Aegis Wall*
- **Effect:** Create a divine barrier that blocks ALL incoming damage for Champion and allies for 10 seconds
- **AOE:** 6m radius around Champion
- **Visual:** Golden translucent dome forms with cross emblem
- **Tactical Use:** Survive enemy ultimates, push through danger
- **Charge Rate:** Standard

### ☀️ Aurelius — *Divine Surge*
- **Effect:** PERMANENT +5 to all stats (HP, ATK, Speed) for the rest of the run
- **AOE:** Self (permanent buff)
- **Visual:** Holy light beam descends, Aurelius briefly transcendent
- **Tactical Use:** Use as often as possible for scaling
- **Charge Rate:** Very Slow (charges from wave clears mostly)

---

## TIER 4 ULTIMATES

### 🧙‍♀️ Lyra — *Coven's Eye*
- **Effect:** All enemies on screen have -50% accuracy for 10 seconds (most attacks miss)
- **AOE:** All enemies
- **Visual:** Giant purple eye appears in sky, gazing down
- **Tactical Use:** Survive ranged enemy waves
- **Charge Rate:** Standard

### 🔮 Seraphina — *Visions of Fate*
- **Effect:** Reveals the next 3 enemy waves (you see exactly what's coming). All shops refresh for free. +1 free reroll banked for each future shop.
- **AOE:** Information/utility
- **Visual:** Time stops briefly, future enemy silhouettes display
- **Tactical Use:** Plan critical relic purchases
- **Charge Rate:** Standard

### 🛡️ Ulric — *Holy Bulwark*
- **Effect:** Champion and all allies gain 200 HP Barrier (absorbs damage). Allies behind Ulric also gain +50% damage for 10 seconds.
- **AOE:** Self + all allies
- **Visual:** Massive golden shield erupts in front of Ulric
- **Tactical Use:** Brigade composition with many allies
- **Charge Rate:** Slow

### ⚡ Ronan — *Lightning Pace*
- **Effect:** +300% Attack Speed for 5 seconds. Movement also boosted to 200% normal.
- **AOE:** Self
- **Visual:** Lightning crackles, motion blur
- **Tactical Use:** Burst damage in tight window
- **Charge Rate:** Fast (Ronan passive)

---

## TIER 5 ULTIMATES

### 🎲 Wyrd — *Chaos Surge*
- **Effect:** Casts 5 random Ultimates from other Champions simultaneously
- **AOE:** Varies wildly (can be amazing or whiff)
- **Visual:** Reality warps, swirling colors, random effects layer
- **Tactical Use:** Hail-mary for desperate moments
- **Charge Rate:** Standard
- **Note:** Wyrd is the only Champion whose ult can include Aurelius's permanent buff (occasional run-defining luck)

### 👑 Auron — *Imperial Decree*
- **Effect:** For the next 30 seconds, all Blessings cost 0 gold. Shop refreshes are free. Any Blessing purchased during this window also lasts the entire run permanently.
- **AOE:** Economy/utility
- **Visual:** Crown burst overhead, golden royal aura
- **Tactical Use:** Mid-run economy explosion, build entire kit in 30s
- **Charge Rate:** Very Slow

---

## Ultimate Synergy with Relics

| Relic | Effect on Ultimates |
|-------|---------------------|
| Time Hourglass | +50% Ultimate charge rate |
| Soul Reaper's Brand | 8% chance per kill to instantly fill ult |
| Excalibur Shard | Crits charge ult faster |
| Phoenix Crown | Revive restores 50% Ultimate charge |

## Ultimate Synergy with Blessings

| Blessing | Effect on Ultimates |
|----------|---------------------|
| Time Slip | -30% Ultimate cooldown after use |
| Arcane Insight | First Ultimate of each wave is free (instant) |
| Battle Mastery (5 Battle blessings) | Crits give +5% Ult per crit |


---

# SECTION 16: THE SPECIALISTS — FOURTH ALLY FACTION

## Overview

The **Specialists** are unconventional allies who don't fit neatly into Vanguard, Bulwark, or Mystics. Each Specialist has a unique mechanic that introduces new playstyles to the Brigade.

**Faction Identity:**
- **Color:** Iridescent (rainbow shimmer)
- **Theme:** Hybrid, experimental, unique
- **Role:** Build-defining specialists with rare abilities

**Acquisition:**
- Specialists appear less frequently in shops (8% vs 25% for other factions)
- Cost 8 gold (vs 5 for standard allies — premium tier)
- Cannot be obtained from common merchant — requires Wave 5+ progression

---

## SPECIALIST 1: BEAST TAMER

**Role:** Summon-based offensive specialist

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Summoning Horn |
| **Base Damage** | 30 (Tamer herself) + summons |
| **Attack Speed** | 1 summon every 8 seconds |
| **Range** | 5-15 units (summons appear near enemies) |
| **Special Effect** | Summons rotating beast companion |
| **Beast Roster** | Bear (high HP/melee), Hawk (ranged/dive), Boar (charges/knockback) |
| **Beast Duration** | 12 seconds per beast |
| **Tier 2 Effect** | Summons appear in pairs |
| **Tier 3 Effect** | Permanent beast that doesn't despawn |
| **Visual** | Cloaked tamer with hunting horn, beasts materialize from forest mist |
| **Color** | Forest green / iridescent |
| **Animation** | Blows horn, beast leaps from glowing portal |

**Combat Behavior:**
- Stays at safe distance
- Summons one beast at a time (rotates randomly)
- Beasts attack independently
- Bear tanks enemies, Hawk dives on ranged enemies, Boar charges through groups

**Synergies:**
- Beast Bond Blessing (+50% summon damage)
- Worldtree Seed (heals beasts)
- Wolf Companion (Vanguard) — stacks beast synergy

---

## SPECIALIST 2: NECROMANCER

**Role:** Resurrection-based offensive specialist

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Bone Staff |
| **Base Damage** | 40 (necro bolt) |
| **Attack Speed** | 2/second |
| **Range** | 5-12 units |
| **Special Effect** | Raises skeletons from killed enemies |
| **Skeleton Stats** | 50 HP, 30 ATK, 2/sec attack speed |
| **Skeleton Cap** | Max 5 skeletons at once |
| **Tier 2 Effect** | Skeletons have +50% HP/ATK |
| **Tier 3 Effect** | On Necromancer death (not happens since allies don't die), skeletons explode for 200 damage |
| **Visual** | Hooded mage with bone staff, purple necrotic glow |
| **Color** | Purple/black necrotic |
| **Animation** | Channels staff downward at corpse, skeleton crawls up from ground |

**Combat Behavior:**
- Waits for enemies to die (other allies' kills count)
- Raises skeletons that fight melee for 15 seconds
- Skeletons explode for 100 damage at end of life (Tier 2+)
- Best in long fights with many kills

**Synergies:**
- High-DPS allies (Sword Squire, Wolf Companion) provide bodies
- Soul Drain Curse (wisps for healing while skeletons fight)
- Mass Resurrection (Mystic Blessing) — necromancer skeletons return next wave

**Note:** This is the only Specialist that can target enemies, kill them, and create more allies from corpses — chained snowball.

---

## SPECIALIST 3: TIME MAGE

**Role:** Crowd control / utility specialist

| Property | Value |
|----------|-------|
| **Weapon Type** | Ranged - Time Staff (Beam) |
| **Base Damage** | 25 (low, support-focused) |
| **Attack Speed** | Continuous beam |
| **Range** | 5-10 units |
| **Special Effect** | Slows all enemies in beam by 60% movement & attack speed |
| **Time Stop** | Every 20 seconds, freezes ALL enemies in 8m radius for 3 seconds |
| **Tier 2 Effect** | Slow becomes 75%, Time Stop radius +50% |
| **Tier 3 Effect** | Time Stop also reverses enemy projectiles |
| **Visual** | Robed mage with hourglass staff, blue temporal aura |
| **Color** | Blue/silver chronomantic |
| **Animation** | Channeling beam with rotating hourglass effect |

**Combat Behavior:**
- Stays back, beams nearest enemy continuously
- Slows all enemies in beam path
- Triggers Time Stop on cooldown (3s of frozen enemies)
- Combos with high-damage allies who exploit frozen targets

**Synergies:**
- Frost Crown relic (freeze on attack stacks with Time Stop)
- High-damage Champions (Drakar, Ronan) — exploit Time Stop window
- Crusader Strike combos (Time Stop + Holy + Physical = devastating)

---

## SPECIALIST 4: TRICKSTER

**Role:** Chaos specialist — random effects on every attack

| Property | Value |
|----------|-------|
| **Weapon Type** | Melee - Dual Daggers (with chaos magic) |
| **Base Damage** | 70 (medium) |
| **Attack Speed** | 3.5/second |
| **Range** | 1-2 units |
| **Special Effect** | Each attack rolls a random effect (see table below) |
| **Tier 2 Effect** | Two effects roll per attack (stacks!) |
| **Tier 3 Effect** | Three effects roll per attack + 25% chance to roll Legendary effect |
| **Visual** | Masked jester with two glowing daggers and grin |
| **Color** | Rainbow iridescent shifting |
| **Animation** | Spinning dual-strike with sparkles |

**Trickster's Chaos Table (Roll per attack):**

| Roll (1-100) | Effect |
|--------------|--------|
| 1-25 | Normal Damage (no special) |
| 26-40 | Bleed (15 DMG/sec, 4s) |
| 41-50 | Burn (20 DMG/sec, 3s) |
| 51-60 | Stun (1s) |
| 61-70 | Heal Champion for 25 HP |
| 71-80 | Knockback enemy 2m |
| 81-88 | Apply Mark (next attack +50% damage) |
| 89-94 | Apply Fear (enemy flees 2s) |
| 95-98 | Double damage |
| 99 | Triple damage + critical |
| 100 | LEGENDARY: Instant kill (non-bosses only) |

**Combat Behavior:**
- Walks to enemies like normal melee
- Each attack rolls table — unpredictable but exciting
- High variance damage output
- Best paired with stable damage dealers

**Synergies:**
- Lucky Coin relic (10% chance to reroll bad rolls)
- High attack speed Blessings (more rolls = more effects)
- Chaos Surge ultimate (Wyrd) thematic match

---

## Specialist Faction Bond Bonuses

Specialists don't share traditional bonds, but have unique stacking bonuses:

**2x Specialists Equipped:** "Chaos Resonance" - All Specialist effects have +10% trigger chance
**3x Specialists Equipped:** "Unstable Convergence" - +25% to all unique mechanic potency (skeletons stronger, Trickster rerolls better)
**4x All Specialists:** "The Court of Curiosities" - One Specialist randomly upgrades to Tier 3 each wave, plus you gain access to special Specialist-only shop

---

## How Specialists Compare to Standard Factions

| Faction | Strength | Weakness | Best Picks |
|---------|----------|----------|------------|
| **Vanguard** | High consistent DPS | Single-target focus | Sword Squire, Wolf Companion |
| **Bulwark** | Crowd control, tank | Low damage | Shield Bearer, Stone Golem |
| **Mystics** | Heal/buff backbone | Need attackers to enable | Light Cleric, Battle Bard |
| **Specialists** | Unique mechanics, high ceiling | Inconsistent, expensive | Necromancer, Time Mage |

**Recommended Team Compositions:**

### "The Iron Line" (Pure Defensive)
2x Shield Bearer + 2x Stone Golem + 1x Halberdier + 1x Light Cleric
- Walls of armor + healing
- Slow but unkillable

### "Hunters of the Wild" (Beast Theme)
1x Beast Tamer + 2x Wolf Companion + 1x Owl Familiar + 1x Battle Bard
- Beast Bond Blessing synergy
- Fast, fierce, mobile damage

### "Court of Death" (Necromancer Build)
1x Necromancer + 2x Sword Squire + 1x Light Cleric + 1x Halberdier
- Skeletons multiply your damage exponentially
- Sword Squires kill bodies → skeletons → more kills

### "Mages of the Tower" (Caster Build)
1x Time Mage + 1x Trickster + 2x Battle Bard + 1x Light Cleric
- Buff stacking + crowd control
- Champion does heavy damage in time-stop windows


---

# SECTION 17: ELITE ENEMIES & MULTI-BOSS ROSTER

## Overview

In addition to standard Scourge enemies (Section 7), Arcane Brigade features:
- **5 Elite Enemies** — Mid-tier threats appearing in higher waves
- **4 Total Bosses** — Major encounters at milestone waves
- **Mini-Boss Variants** — Elite versions of bosses for endless mode

This adds variety and difficulty pacing throughout the run.

---

## ELITE ENEMIES

Elite enemies appear from Wave 5 onwards, alongside regular Scourge. They have:
- 3x the HP of regular enemies
- Unique mechanics not found in basic Scourge
- Drop bonus gold (10g instead of 2g)
- 50% chance to drop a Common/Uncommon relic on death
- Visually distinct (glowing aura, larger size)

---

### ELITE 1: PLAGUE SPREADER

| Property | Value |
|----------|-------|
| **Tier** | Elite Melee |
| **Weapon Type** | Melee - Plague Censer |
| **Base Damage** | 60 |
| **Attack Speed** | 1.5/second |
| **HP** | 250 (3x normal) |
| **Special Effect** | On hit applies Poison (15 DMG/sec, 5s) |
| **Unique Mechanic** | Releases toxic cloud on death (8m radius, 100 damage over 4s) |
| **Threat Level** | High |
| **Visual** | Robed plague doctor with skull mask and dripping censer |
| **Appearance** | Sickly green miasma trails |
| **Weakness** | Fire (burns plague) and Holy |
| **Strategy** | Kill at range, don't stand in death cloud |

**Combat Behavior:**
- Walks toward player slowly
- Swings censer in arcs (slow but high damage)
- Toxic cloud explodes on death (don't kill while standing close)
- Threatens via positioning

---

### ELITE 2: STORM MAGE

| Property | Value |
|----------|-------|
| **Tier** | Elite Ranged |
| **Weapon Type** | Ranged - Lightning Staff |
| **Base Damage** | 90 per bolt |
| **Attack Speed** | 1/second |
| **HP** | 200 |
| **Special Effect** | Lightning chains to 2 nearby targets (50% damage to each chain) |
| **Unique Mechanic** | Casts "Storm Surge" every 15s — 6m AOE lightning around mage (200 damage) |
| **Threat Level** | High |
| **Visual** | Tall mage with crackling staff, hovering above ground |
| **Appearance** | Blue robes with arcs of electricity |
| **Weakness** | Physical (no resistance) and Holy |
| **Strategy** | Spread allies out (chain), close gap during Storm Surge |

**Combat Behavior:**
- Stands at far range (20+ units when possible)
- Targets player with chain lightning
- Periodically casts AOE around self
- Forces movement to break chains

---

### ELITE 3: CRYSTAL BEAST

| Property | Value |
|----------|-------|
| **Tier** | Elite Melee Tank |
| **Weapon Type** | Melee - Crystal Claws |
| **Base Damage** | 70 |
| **Attack Speed** | 2/second |
| **HP** | 400 (massive) |
| **Special Effect** | Regenerates 30 HP/second |
| **Unique Mechanic** | "Crystal Burst" — At 50% HP, spawns 3 small Crystal Shards (small enemies) |
| **Shards** | 50 HP each, 40 DMG, 3/sec attack speed |
| **Threat Level** | Very High |
| **Visual** | Quadruped beast covered in glowing crystals |
| **Appearance** | Cyan/translucent crystal skin |
| **Weakness** | Physical (shatters crystal) — Arcane is weak against this |
| **Strategy** | Burst it down before 50% HP to avoid shards, prioritize over allies |

**Combat Behavior:**
- Walks toward player
- Claw attacks at close range
- Regens rapidly if disengaged
- Becomes more dangerous when spawning Shards

---

### ELITE 4: DARK KNIGHT

| Property | Value |
|----------|-------|
| **Tier** | Elite Melee |
| **Weapon Type** | Melee - Black Sword (mirrors Champion's weapon) |
| **Base Damage** | Scales with player ATK (75% of player damage) |
| **Attack Speed** | 3/second |
| **HP** | 300 |
| **Special Effect** | Uses Champion's Ultimate at 50% HP (50% effectiveness) |
| **Unique Mechanic** | "Mirror Stance" — When blocking, reflects 100% of damage taken |
| **Threat Level** | Extreme (scales with player's own build) |
| **Visual** | Knight in black armor identical to Champion's silhouette |
| **Appearance** | Corrupted version of Champion model |
| **Weakness** | Holy (purification) — Resists Physical |
| **Strategy** | Be unpredictable, don't rely on one combo, save Ultimate to avoid Mirror Stance |

**Combat Behavior:**
- Mirrors player's attack patterns
- Uses player's own moves against them
- High threat in builds with strong Ultimates (DK uses yours back at you)
- A skill check on player's adaptability

**Special:** Dark Knight is unique — its damage and abilities scale with the PLAYER's current build, making it threatening regardless of how powerful the player has become.

---

### ELITE 5: SPIDER QUEEN

| Property | Value |
|----------|-------|
| **Tier** | Elite Ranged + Summoner |
| **Weapon Type** | Ranged - Web Shooter |
| **Base Damage** | 50 (web bolts) |
| **Attack Speed** | 1.5/second |
| **HP** | 220 |
| **Special Effect** | Web bolts apply Ensnared (cannot move 2s) |
| **Unique Mechanic** | Spawns 1 Small Spider every 5 seconds (max 5 active) |
| **Small Spiders** | 30 HP, 20 DMG, 1.5/sec attack speed, melee |
| **Threat Level** | High (zone control) |
| **Visual** | Massive spider with crown of legs, glowing red eyes |
| **Appearance** | Black chitinous body with web sacs |
| **Weakness** | Fire (burns webs and spiders) — Resists Physical |
| **Strategy** | AOE kill spiders fast, then focus Queen, avoid web ensnare |

**Combat Behavior:**
- Stands back, fires web bolts
- Continuously spawns small spiders
- Players get overwhelmed if Queen ignored too long
- AOE-heavy compositions thrive

---

## MULTI-BOSS ROSTER

Arcane Brigade now features 4 distinct bosses, each appearing at major wave milestones:

| Wave | Boss | Difficulty | Theme |
|------|------|------------|-------|
| **Wave 10** | The Dread Lich | Medium | Undead Magic |
| **Wave 25** | The Inferno Dragon | Hard | Fire & Flight |
| **Wave 50** | The Forgotten King | Very Hard | Ancient Knight |
| **Wave 100+** | The Voidlord | Extreme (Endless) | Cosmic Horror |

---

### BOSS 1: THE DREAD LICH (Wave 10)
*See Section 7 for full details — first major boss, hybrid melee/ranged.*

**Summary:**
- Skeletal lich king with shifting void magic
- Phases: Melee (Wrathful Halberd) ↔ Ranged (Adaptive Tome)
- Learning mechanic — gains resistance to repeated damage
- HP: 500-1500 by difficulty

---

### BOSS 2: THE INFERNO DRAGON (Wave 25)

| Property | Value |
|----------|-------|
| **Weapon Type** | HYBRID - Flying ranged + grounded melee |
| **Base Damage** | 200+ (extreme) |
| **HP** | 2000-4000 (scales) |
| **Phases** | 3 phases (Aerial → Grounded → Frenzy) |
| **Threat Level** | Extreme |
| **Special Effect** | Spawns Fire Pools on the ground |
| **Visual** | Massive crimson dragon with smoldering scales and obsidian claws |
| **Appearance** | Volcanic dragon themed |
| **Weakness** | Ice/Holy (anti-fire) — Resists Fire |
| **Strategy** | Stay mobile, avoid fire pools, target wings to ground it |

**Phase 1: Aerial Bombardment (100% → 66% HP)**
- Flies high (cannot be hit by melee)
- Drops fireballs from sky (3 per second in 4m AOE)
- Forces ranged attacks only
- Champions/allies with ranged DOMINATE this phase

**Phase 2: Grounded Fury (66% → 33% HP)**
- Lands on field, becomes melee-attackable
- Massive claw swipes (300 damage, 4m range)
- Bite attack pierces shields (150 damage, ignores block)
- Now everyone can hit it

**Phase 3: Inferno Frenzy (33% → 0% HP)**
- Wings reignite, breathes fire in 90-degree cone
- Spawns 3 Imp Swarms as adds
- Final desperate phase, intense damage

**Special Mechanic: Fire Pools**
- Boss burns ground where it lands (8s duration pools)
- Pools deal 50 DMG/sec to players standing in
- Multiple pools create kill zones over time
- Players must constantly reposition

**Boss Scaling:**
- Difficulty 1: 2000 HP, 200 DMG
- Difficulty 3: 3000 HP, 280 DMG
- Difficulty 5: 4000 HP, 400 DMG
- Endless 100+: +5% per wave

---

### BOSS 3: THE FORGOTTEN KING (Wave 50)

| Property | Value |
|----------|-------|
| **Weapon Type** | SHIFTING - Cycles through 4 weapon types |
| **Base Damage** | 250+ |
| **HP** | 5000-8000 |
| **Phases** | 4 phases (one per weapon type) |
| **Threat Level** | Extreme |
| **Special Effect** | Summons Phantom Knights as adds |
| **Visual** | Spectral knight king in tarnished royal armor, cloaked in fading glory |
| **Appearance** | Ghostly king with crown, holding rotating ethereal weapons |
| **Weakness** | Holy (purification) — Resists Physical |
| **Strategy** | Adapt to weapon phase, defeat Phantom Knights quickly |

**Weapon Phases (Cycles in order):**

**Phase 1: Sword of Heroes (Blade)**
- Quick cleave attacks (3/sec, 150 damage)
- Hits in 120° cone in front
- Phase lasts 25% HP

**Phase 2: Hammer of Ages (Blunt)**
- Slow but heavy slams (1/sec, 350 damage)
- Stuns for 2s on hit
- Causes ground shockwaves
- Phase lasts 25% HP

**Phase 3: Lance of Light (Piercing)**
- Long-range thrust attacks (2/sec, 200 damage)
- Piercing — ignores 50% armor
- Phase lasts 25% HP

**Phase 4: Bow of Eternity (Ranged)**
- Rapid arrow barrage (5/sec, 80 damage each)
- Tracking arrows (some homing)
- Final phase — most aggressive
- Phase lasts 25% HP

**Special Mechanic: Phantom Knights**
- Every 30 seconds, summons 2 Phantom Knights (50 DMG melee, 200 HP each)
- Phantom Knights mirror boss's current weapon
- Must be killed quickly to avoid being overwhelmed

**Boss Scaling:**
- Difficulty 1: 5000 HP, 250 DMG
- Difficulty 3: 6500 HP, 350 DMG
- Difficulty 5: 8000 HP, 500 DMG

---

### BOSS 4: THE VOIDLORD (Wave 100+ - Endless Mode)

| Property | Value |
|----------|-------|
| **Weapon Type** | REALITY-WARPING - Defies categorization |
| **Base Damage** | 500+ |
| **HP** | 15000-50000+ (endless scaling) |
| **Phases** | 5 phases (extremely long fight) |
| **Threat Level** | Apocalyptic |
| **Special Effect** | Reality distortions, banishment, summoning |
| **Visual** | Massive eldritch entity with 8 floating tentacles, eyes everywhere, void-rifts around it |
| **Appearance** | Cosmic horror with shifting form |
| **Weakness** | All damage types reduced 50% — only weapon synergies break through |
| **Strategy** | Survive long enough to whittle down, use damage type combos religiously |

**Phase 1: Awakening (100% → 80% HP)**
- Slow tentacle strikes (2/sec, 400 damage)
- Summons standard Scourge enemies as adds
- Boss is feeling out player capabilities

**Phase 2: Spatial Distortion (80% → 60% HP)**
- Random void rifts appear (deal 100 DMG/sec to players standing in)
- Tentacle strikes faster (3/sec)
- Map shrinks toward center over time

**Phase 3: Eyes of Madness (60% → 40% HP)**
- Spawns 6 floating eye minions (each casts Hex on allies)
- Boss casts "Maddening Gaze" — random ally is forced to attack other allies for 4 seconds
- Champion must kill eyes quickly

**Phase 4: Banishment (40% → 20% HP)**
- Every 20 seconds, banishes one Champion's Ally (gone for 15s)
- Reduces Brigade strength temporarily
- Voidlord summons 2 Mimic Fiends per minute

**Phase 5: Apocalyptic Frenzy (20% → 0% HP)**
- All abilities active simultaneously
- Tentacle barrage (5/sec)
- Spawns continuous adds
- Final desperate phase, takes 1-3 minutes to defeat
- True test of endurance

**Boss Scaling:**
- Wave 100: 15000 HP, 500 DMG
- Wave 150: 25000 HP, 750 DMG
- Wave 200+: 50000+ HP, 1500 DMG (theoretical hard cap)

**Special Reward:** Defeating Voidlord at any wave grants the "Voidbreaker" achievement and unlocks the Auron Champion if not already unlocked.

---

## MINI-BOSS VARIANTS (Endless Mode)

In endless mode (after Wave 50), Elite enemies sometimes appear as Mini-Boss versions:

- **Plague Lord** (Elite Plague Spreader) - 5x HP, larger toxic cloud
- **Storm King** (Elite Storm Mage) - Constant Storm Surge effect
- **Crystal Goliath** (Elite Crystal Beast) - 10x HP, 6 Crystal Shard spawns
- **Black Champion** (Elite Dark Knight) - Uses TWO of player's Ultimates
- **Spider Empress** (Elite Spider Queen) - Spawns Elite Spiders (Tier 2)

These add unpredictable threats to endless runs.


---

# SECTION 18: ARENAS & ENVIRONMENTAL HAZARDS

## Overview

Combat doesn't happen in a vacuum — each run takes place in one of 6 themed Arenas with unique environmental features and hazards. Arenas affect:
- Movement patterns
- Available cover/hazards
- Environmental damage sources
- Aesthetic atmosphere

Players select Arena before starting a run (or randomized in certain modes).

---

## ARENA 1: THE THRONE ROOM (Starter Arena)

**Theme:** Royal castle throne hall — the Brigade's home base
**Atmosphere:** Stained glass windows, banners, golden chandeliers
**Size:** 30×30 unit square (open layout)

**Hazards:** None (tutorial arena)
**Bonuses:** None — pure combat focus
**Visual Highlights:** Light beams from windows, banners swaying

**Best For:** Learning the game, no environmental complications

---

## ARENA 2: THE ENCHANTED FOREST

**Theme:** Mystical woodland — ancient trees, glowing fungi
**Atmosphere:** Misty, magical, dappled light
**Size:** 35×35 with tree obstacles

**Hazards:**
- 🌿 **Sentient Vines** - Random vines grab player legs, slowing for 2s (every 30s)
- 🍄 **Spore Clouds** - Mushroom patches release toxic spores on contact (20 DMG/sec)
- 🌳 **Tree Cover** - Trees block ranged attacks (good and bad)

**Bonuses:**
- 🌱 **Forest Spirit** - Healing pools spawn at random locations (heal 30 HP on touch)
- 🍃 **Wind Currents** - Ranged projectiles travel 25% faster in certain zones

**Best For:** Ranger-style Champions (Falka, Zephyr), beast-themed builds

---

## ARENA 3: THE VOLCANIC FOUNDRY

**Theme:** Dwarven forge in active volcano
**Atmosphere:** Glowing lava, smoke, anvils, hot metal
**Size:** 25×40 (long rectangular)

**Hazards:**
- 🔥 **Lava Pools** - Walking on lava deals 100 DMG/sec (clearly marked red)
- 🌋 **Magma Spurts** - Random eruptions every 20s in random spots (200 damage AOE)
- 💨 **Smoke Vents** - Obscure vision for 5 seconds (low visibility)

**Bonuses:**
- 🔨 **Forge Anvils** - Standing near anvil grants +20% weapon damage for 10s
- 🔥 **Heat Aura** - All Champion fire damage +15% in this arena
- ⚙️ **Lava Geyser Trap** - Players can lure enemies into geysers for environmental kills

**Best For:** Fire-themed builds (Drakar, Bron), tank builds avoiding lava

---

## ARENA 4: THE FROZEN CITADEL

**Theme:** Abandoned ice fortress in eternal winter
**Atmosphere:** Snow, frost, frozen statues, blizzards
**Size:** 30×30 with ice patches

**Hazards:**
- ❄️ **Ice Patches** - Slippery surfaces — character keeps sliding when stopping
- 🌨️ **Blizzard Periods** - Every 45s, blizzard reduces visibility and slows everyone 25% for 10s
- 🧊 **Falling Icicles** - From ceilings, deal 80 damage (telegraphed 1.5s)

**Bonuses:**
- ❄️ **Frost Aura** - All attacks have 5% chance to apply Freeze
- 🏔️ **Cold Healing** - Champion HP regen +50% in this arena
- ⛄ **Frozen Statues** - Can be broken for random drops (gold, small items)

**Best With:** Ice-themed builds, control builds (Time Mage, Frost Crown relic)

---

## ARENA 5: THE VOID RIFT

**Theme:** Tear in reality — fragments of multiple dimensions
**Atmosphere:** Floating debris, void-purple sky, shifting geometry
**Size:** 25×25 (compact) with floating platforms

**Hazards:**
- 🕳️ **Void Pockets** - Random dark spots disable Blessings for anyone inside (3s)
- 🪐 **Gravity Inversions** - Some patches reverse character movement (15s duration)
- 👁️ **Eye Tendrils** - Floating eyes mark random players, focusing enemy attention

**Bonuses:**
- ✨ **Arcane Saturation** - All Arcane damage +25% in this arena
- 🌌 **Reality Tears** - Player can briefly phase through enemies (every 30s)
- 🔮 **Cosmic Echoes** - Ultimates charge 30% faster

**Best With:** Arcane-focused builds (Morgath, Lyra), Auron's economy build

**Note:** This arena is recommended for advanced players — many hazards.

---

## ARENA 6: THE CATHEDRAL

**Theme:** Sacred holy cathedral — pillars of light, stained glass
**Atmosphere:** Reverent, awe-inspiring, golden hour lighting
**Size:** 40×30 (large open central hall)

**Hazards:**
- ☠️ **Cursed Tiles** - Black tiles deal 20 DMG/sec to undead-themed enemies but also to player if standing in
- 🛐 **Penitent Pulse** - Every 60s, healing pulse but Champion takes 20 damage too
- 📜 **Sealing Runes** - Random ground runes silence anyone standing in them (no abilities for 3s)

**Bonuses:**
- ✨ **Holy Aura** - All Holy damage +30%
- 🙏 **Sanctuary Pillars** - 4 pillars in arena grant +50% damage reduction within 2m
- ⚜️ **Divine Light** - Ultimate charge +20% per second when inside a Light Pillar (4 spots)
- 🕊️ **Resurrection Site** - One ally that died this wave returns at next wave start

**Best With:** Holy-themed builds (Aurelius, Ulric), Champions that synergize with allies

---

## Arena Selection in Different Modes

| Mode | Arena Selection |
|------|----------------|
| Campaign Story | Fixed per chapter |
| Endless Mode | Player chooses or random |
| Daily Challenge | Specific arena enforced |
| Boss Rush | Arena varies per boss (matches boss theme) |
| Twin Sigils (Co-op) | Both players vote, random tiebreaker |

## Arena Mastery System

Playing in each arena unlocks **Mastery Levels** (1-10):
- Each level grants permanent bonuses for that arena
- Mastery 5: +5% damage in arena
- Mastery 10: +10% damage + unique cosmetic emblem
- Encourages players to learn each arena's quirks

---

## Future Arena Expansion Ideas (Roadmap)

Suggested arenas for post-launch content:
- 🏜️ **The Endless Desert** - Sandstorm hazards, sand-burrowing enemies
- 🌊 **The Sunken Temple** - Underwater pillars, slow movement
- ⛰️ **The Mountain Peak** - Wind gusts pushing players, narrow ledges
- 🌃 **The Shadowmoor** - Constant darkness, lantern mechanics


---

# SECTION 19: GAME MODES & LONG-TERM PROGRESSION

## Overview

Arcane Brigade features multiple game modes serving different player goals — from focused story to endless challenge to co-op. Plus, long-term progression keeps players returning across many sessions.

---

## GAME MODES

### Mode 1: STORY CAMPAIGN

**Description:** The narrative-driven mode where players progress through 5 difficulty tiers, each representing a chapter in the Arcane Brigade's saga.

**Structure:**
- **Chapter 1: The Awakening (Novice)** - Tutorial chapter, Throne Room
- **Chapter 2: Into the Wilds (Standard)** - Enchanted Forest, meet first allies
- **Chapter 3: The Forge War (Advanced)** - Volcanic Foundry, vs Dread Lich
- **Chapter 4: Winter's Grip (Hardcore)** - Frozen Citadel, vs Inferno Dragon
- **Chapter 5: Voidfall (Nightmare)** - Void Rift, vs Forgotten King
- **Epilogue: The Final Stand (Apocalypse)** - All arenas, vs Voidlord

**Mechanics:**
- 15 waves per chapter
- Boss at wave 10 and wave 15
- Champion locked per chapter (cycles through different ones)
- Unlocks happen mid-chapter (new allies, blessings, relics)
- Save progress between chapters

**Rewards:**
- Story Mode XP (10x normal)
- Cosmetic unlocks
- Champion Mastery progression

---

### Mode 2: ENDLESS MODE

**Description:** Survive as long as possible. Waves never end, scaling forever.

**Structure:**
- Player chooses Champion + Arena freely
- Waves continue indefinitely
- Each wave is harder than the last
- Run continues until Champion dies (no continues)
- Boss spawns at wave 10, 25, 50, 100, then every 50 waves

**Scaling:**
- +5% enemy HP per wave after wave 10
- +5% enemy damage per wave after wave 10
- New enemy types unlock at certain wave thresholds
- Elite spawn rate increases over time

**Rewards:**
- Run XP based on waves cleared
- Leaderboards (global wave records)
- Permanent unlock progression (faster than story)

**Special Mechanic:**
- After Wave 50, "Voidcorruption" begins:
  - Random rules change per wave (e.g., "All enemies have 50% resistance to Fire", "Allies cost 2x gold")
  - Forces adaptation

---

### Mode 3: DAILY CHALLENGE

**Description:** A unique daily run with mandatory rules and conditions.

**Structure:**
- New challenge every 24 hours
- All players play same modifiers
- Locked Champion + Arena
- Specific Blessing/Relic restrictions (e.g., "Only Battle Blessings allowed")
- 15 waves with daily boss

**Example Daily Challenges:**
- *"Iron Vow"* - Bron only, no defensive Blessings, only melee weapons
- *"Mage's Trial"* - Arcanus only, only Arcane damage allowed
- *"Treasure Hunt"* - Cassia only, win condition: collect 2000 gold
- *"Pacifist Path"* - Garrick only, can't damage enemies directly (allies must do work)

**Rewards:**
- Daily XP bonus (2x)
- Unique cosmetic per perfect day clear
- Streak bonuses (consecutive daily wins multiply rewards)

---

### Mode 4: BOSS RUSH

**Description:** Skip normal waves — fight every boss back-to-back.

**Structure:**
- 4 bosses sequentially: Dread Lich → Inferno Dragon → Forgotten King → Voidlord
- 15-second rest between bosses (heal, buy Blessings)
- Pre-set starting gold (500g)
- Champion + 2 starting allies chosen by player

**Difficulty Variants:**
- **Normal Boss Rush** - Standard boss HP
- **Hardcore Boss Rush** - +50% boss HP and damage
- **Nightmare Boss Rush** - +100% boss HP, learning mechanic active for all bosses

**Rewards:**
- Boss-specific cosmetics
- "Boss Slayer" titles
- High-tier relic unlocks for future runs

---

### Mode 5: TRIAL OF ASCENSION

**Description:** Focus on mastering one Champion through escalating challenges.

**Structure:**
- Select one Champion
- 10 sequential Trials (waves), each adding a restriction
- Trial 1: Standard run
- Trial 5: Limited Blessings, fewer allies
- Trial 10: Final boss + 50% damage taken

**Champion-Specific Trials:**
- Each Champion has a unique 10-Trial path designed for their playstyle
- Completing all 10 trials grants "Ascended" status for that Champion
- Ascended Champions get +10% all stats and unique outfit

**Rewards:**
- Champion Mastery levels accelerate
- Ascended cosmetic (gold outline)
- Unlocks Champion's hidden lore

---

### Mode 6: TWIN SIGILS (Co-op)

**Description:** Two-player cooperative mode (inspired by ZZZ's Shared Dream).

**Structure:**
- 2 players, each controlling their own Champion
- Shared allies pool (16 allies total between both players)
- Shared gold economy (collaborative)
- Wave count and boss spawns adjusted for 2 players
- Each player has own Blessings and Relics

**Co-op Mechanics:**
- **Resurrection** - If one player dies, partner can revive at next wave start (50% HP)
- **Tag-Team Ultimate** - When both Champions ult within 3s, combo bonus +50%
- **Linked Blessings** - Some Blessings only activate in co-op (e.g., "Twin Strike" - both Champions hit same target = +100% damage)
- **Shared Curse** - If one player gets Curse Blessing, partner gets minor version

**Cross-platform:** Yes, friend codes
**Voice:** Push-to-talk built in
**Rewards:**
- Co-op XP shared
- Friendship medals
- Exclusive co-op cosmetics

---

## LONG-TERM PROGRESSION SYSTEMS

### System 1: CHAMPION MASTERY

Each Champion has independent Mastery levels (1-100).

**XP Sources:**
- Playing as Champion (10 XP per wave)
- Killing enemies as Champion
- Completing chapters/dailies with Champion
- Trial of Ascension completions

**Mastery Rewards Per Level (every 5 levels):**
- Level 5: +1 starting Blessing slot
- Level 10: -10% Blessing cost
- Level 25: New Ultimate visual effect (cosmetic)
- Level 50: Champion gets "Awakened" tier (slightly stronger)
- Level 100: "Legend" tier — unique passive bonus + golden outline

---

### System 2: ARCANE BRIGADE LEVEL (Account)

Account-wide level (1-500+).

**XP Sources:**
- Anything you do in-game
- Daily missions
- Boss kills
- Wave clear bonuses

**Account Level Rewards:**
- Level 1: Unlock Story Mode
- Level 10: Unlock Endless Mode
- Level 25: Unlock Daily Challenge
- Level 50: Unlock Boss Rush
- Level 75: Unlock Twin Sigils (Co-op)
- Level 100: Unlock Trial of Ascension
- Level 150+: Cosmetic unlocks every 10 levels

---

### System 3: PRESTIGE (Ascension)

After reaching Account Level 200, players can "Ascend" — reset their level for permanent bonuses.

**Ascension Levels (1-10):**
- **Ascension 1:** +5% all stats permanently
- **Ascension 3:** Unlock secret Champion (Sovereign)
- **Ascension 5:** Unlock secret Ally (Phoenix Guardian)
- **Ascension 7:** Voidlord becomes available in Story Mode
- **Ascension 10:** True Ending unlocked (story epilogue)

Each Ascension resets account level but is permanent.

---

### System 4: BRIGADE COLLECTION (Achievements)

Long-term collection goals:

**Champion Collection:**
- Unlock all 18 Champions
- Reach Mastery 100 with each
- Complete Trial of Ascension with each

**Ally Collection:**
- Unlock all 16 Allies
- Max-tier (Tier 3) each ally
- Discover all ally synergies

**Relic Collection:**
- Find each of 30 relics in shop (running diary)
- Win run with each Legendary relic

**Boss Slayer:**
- Defeat each boss on each difficulty
- Defeat Voidlord at increasing wave counts (100, 150, 200+)

**Total Achievement Points:** 1000+ (sets long-term goals)

---

### System 5: DAILY & WEEKLY QUESTS

**Daily Quests (3 per day):**
- Examples: Win 1 run, Kill 200 enemies, Use 5 Ultimates
- Reward: 100-300 XP, small gold for next run

**Weekly Quests (5 per week):**
- Examples: Win 5 runs, Defeat all bosses, Try every Champion
- Reward: Significant XP, guaranteed Rare/Legendary relic for next run

**Weekend Challenges:**
- Special weekend-only runs with extreme modifiers
- Higher rewards
- Limited-time cosmetics


---

# SECTION 20: SYNERGY & FACTION BOND SYSTEM

## Overview

The depth of Arcane Brigade comes from how characters, allies, and items synergize. The Faction Bond System rewards strategic team composition with stacking bonuses.

---

## FACTION BOND BONUSES (Detailed)

### VANGUARD BONDS (Offensive)

| Count | Bond Name | Effect |
|-------|-----------|--------|
| 2x | "Forward March" | +5% Vanguard ATK |
| 3x | "Brigade Front" | +10% Vanguard ATK + Bleed on hit (10 DMG/sec, 3s) |
| 5x | "Conquering Force" | +25% Vanguard ATK + +25% Crit Chance + "Berserker Aura" passive (all Vanguard +20% attack speed when one Vanguard kills enemy) |

### BULWARK BONDS (Defensive)

| Count | Bond Name | Effect |
|-------|-----------|--------|
| 2x | "Iron Wall" | +5% Bulwark HP, +10% block chance |
| 3x | "Shieldwall" | +15% Bulwark HP + Bulwarks reflect 20% damage |
| 5x | "Fortress Order" | +50% Bulwark HP + Bulwarks have 40% block + Champion takes -25% damage when behind any Bulwark |

### MYSTICS BONDS (Support)

| Count | Bond Name | Effect |
|-------|-----------|--------|
| 2x | "Whispered Wisdom" | Mystics' buffs +10% duration |
| 3x | "Sacred Chorus" | Mystics' effects +20% potency + all allies +5 HP regen/sec |
| 5x | "Divine Convergence" | Mystics buff all allies simultaneously + heal pulse every 5s |

### SPECIALISTS BONDS (Unique)

| Count | Bond Name | Effect |
|-------|-----------|--------|
| 2x | "Chaos Resonance" | All Specialist effects +10% trigger chance |
| 3x | "Unstable Convergence" | +25% Specialist mechanic potency |
| 4x (All) | "Court of Curiosities" | One Specialist randomly auto-upgrades to Tier 3 each wave + access to Specialist-only shop |

---

## MIXED FACTION BONUSES

Beyond single-faction bonds, certain MIXES grant special bonuses:

### Mixed Bond 1: "Balanced Order"
- **Requirement:** 2 Vanguard + 2 Bulwark + 2 Mystics
- **Bonus:** +5% all stats for Champion, +10% damage reduction
- **Description:** The classic Arcane Brigade composition

### Mixed Bond 2: "Brigade Charge"
- **Requirement:** 3 Vanguard + 2 Bulwark
- **Bonus:** Champion + frontline allies have +15% damage
- **Description:** Frontline-heavy aggressive build

### Mixed Bond 3: "Mystic Wall"
- **Requirement:** 3 Bulwark + 2 Mystics
- **Bonus:** Bulwarks healed by Mystics gain +25% HP and reflect damage
- **Description:** Unkillable wall build

### Mixed Bond 4: "Hunter's Pack"
- **Requirement:** Wolf Companion + Owl Familiar + Beast Tamer + 1 Vanguard
- **Bonus:** All beasts gain +50% damage and act as one swarming unit
- **Description:** Beast-themed special composition

### Mixed Bond 5: "The Court of Death"
- **Requirement:** Necromancer + Halberdier + Light Cleric (resurrection theme)
- **Bonus:** Skeletons from Necromancer are healed by Light Cleric, gaining +100% HP
- **Description:** Undead army build

### Mixed Bond 6: "Wildmage's Court"
- **Requirement:** Wyrd (Champion) + 3 Specialists
- **Bonus:** Chaos Surge ultimate has +50% chance to cast Legendary effects
- **Description:** Maximum chaos build

---

## DAMAGE TYPE COMBO CHAINS

Building on Section 14's Elemental Reactions, here are CHAIN COMBOS that activate when specific damage types hit in sequence:

### 3-Chain: Inferno Combo
**Sequence:** Fire → Fire → Arcane
**Effect:** Triggers Combustion + creates persistent flame pillar (200 DMG/sec for 5s)
**Best Champion:** Drakar with Storm Caller relic

### 3-Chain: Crusader's Wrath
**Sequence:** Physical → Holy → Physical
**Effect:** Massive bonus damage (+300%) on third hit
**Best Champion:** Ulric, Aurelius

### 3-Chain: Soul Drain Combo
**Sequence:** Arcane → Physical → Holy
**Effect:** Steal 25% enemy max HP as Champion healing
**Best Champion:** Auron or Champions with anti-undead builds

### 5-Chain: The Forge
**Sequence:** Physical → Fire → Physical → Fire → Holy
**Effect:** Permanent +5 ATK for run (one-time per run)
**Best Champion:** Bron, Aurelius (combos with Surge passive)

### Champion-Exclusive Combos

Some combos are tied to specific Champions:

**Aldric's "Banner Strike":** Fire + Holy with at least 3 allies on screen = AOE rally buff
**Auron's "Royal Decree":** Any 4 damage types within 8s = next Blessing is free
**Wyrd's "Wild Magic":** Random damage type on each hit (any chain triggers)

---

## ALLY SYNERGY MATRIX

Some allies combo together better than others:

### Strong Synergies (★★★)

| Ally A | Ally B | Synergy |
|--------|--------|---------|
| Sword Squire | Light Cleric | Cleric heals Squire's Bleed effect targets, boosting damage |
| Wolf Companion | Beast Tamer | Beast Bond shared bonuses |
| Stone Golem | Necromancer | Skeletons hide behind Golem, are protected |
| Mirror Spirit | Time Mage | Frozen enemies' reflected projectiles are devastating |
| Halberdier | Battle Bard | Bard's empowerment makes counterstrikes deadly |

### Medium Synergies (★★)

| Ally A | Ally B | Synergy |
|--------|--------|---------|
| Ballista Crew | Owl Familiar | Owl marks targets, Ballista hits marked enemies harder |
| Light Cleric | Shadow Wisp | Cleric keeps the team alive while Wisp executes |
| Wind Sprite | Trickster | Trickster's rolls happen faster with attack speed |
| Shield Bearer | Drakar (Champion) | Drakar's cannon needs the safety Shield Bearer provides |

### Anti-Synergies (✗)

Some pairings don't work well:
- Stone Golem + Wind Sprite: Speed buffs wasted on immobile Golem
- Necromancer + Holy-themed builds: Skeletons take bonus damage from your own area effects
- Trickster + Time Mage: Trickster's rolls "miss" during Time Stop (no enemy hits)

---

## VISUAL FEEDBACK FOR SYNERGIES

When a synergy activates, players get clear feedback:

**Bond Activation:**
- All bonded allies briefly flash with faction color
- Audio: Trumpet flare for offense, anvil clang for defense, bell chime for support
- Bond name appears as text banner across top of screen
- Persistent UI element shows active bonds with icons

**Combo Activation:**
- Damage type icons appear above target during chain
- Final chain hit shows large COMBO text + bonus damage number
- Special sound for each combo type
- Combo counter in HUD tracks streaks

**Ally Synergy Triggers:**
- Glowing line connects synergizing allies briefly
- Particles flow between them
- Status icon shows on both allies during effect


---

# SECTION 21: WAVE EVENTS & RUN ENCOUNTERS

## Overview

To break up the wave-grind monotony of typical roguelikes, Arcane Brigade introduces **Wave Events** — random encounters that interrupt the standard combat flow with unique mini-scenarios.

**When Events Occur:**
- 30% chance after any wave (except boss waves)
- Always after Wave 5, 15, 30, 45 (guaranteed)
- Higher chance in Endless mode (40%)
- Player chooses whether to engage or skip (skipping refunds wave reward)

**Event Categories:**
1. Beneficial (Shrines, Merchants, Treasure)
2. Challenging (Ambushes, Curses, Trials)
3. Mysterious (Random outcomes - high risk, high reward)

---

## BENEFICIAL EVENTS

### Event 1: The Traveling Merchant
**Description:** A cloaked merchant appears with rare wares.

**Mechanics:**
- Special shop with 5 items (instead of normal 3)
- 1 guaranteed Rare relic at 25% discount
- 1 Legendary relic available at full price (only chance in early waves)
- Stays for 1 wave only
- Merchant disappears after wave

**Choice:** Always beneficial, just costs gold

---

### Event 2: Ancient Shrine
**Description:** Discovered a divine shrine. Offer gold to receive blessings.

**Mechanics:**
- Pay 50 gold: Heal to full HP
- Pay 100 gold: Random Uncommon Blessing
- Pay 200 gold: Permanent +3 to chosen stat (HP/ATK/Speed)
- Pay 500 gold: Random Legendary Relic

**Choice:** Trade gold for power — economy-dependent

---

### Event 3: Treasure Vault
**Description:** Discovered a hidden vault. Pick a lock.

**Mechanics:**
- 3 chests displayed (Bronze, Silver, Gold)
- Bronze (free): 50 gold or Common Relic
- Silver (25g): Uncommon Relic or Blessing
- Gold (100g): Rare Relic or Legendary Blessing
- Player picks ONE chest only

**Choice:** Risk vs reward — better chest = harder to afford

---

### Event 4: Mystic Spring
**Description:** A magical spring grants temporary boons.

**Mechanics:**
- Free choice of one:
  - "Healing Spring": Heal 100% HP + remove all debuffs
  - "Strength Spring": +50% damage for next 3 waves
  - "Swift Spring": +50% movement speed for next 3 waves
  - "Wisdom Spring": Free shop reroll all of next 3 waves

**Choice:** Tactical buff for upcoming waves

---

### Event 5: Pilgrim's Camp
**Description:** A wandering pilgrim offers wisdom.

**Mechanics:**
- Choose a faith:
  - "Faith in Combat": Sacrifice 1 random Blessing → Receive 1 Rare random Blessing
  - "Faith in Companions": Sacrifice 1 ally → All other allies upgrade 1 tier
  - "Faith in Self": Champion permanent +5 ATK
  - "Refuse Faith": Receive 100 gold

**Choice:** Trade-off mechanics — sacrifice for gain

---

## CHALLENGING EVENTS

### Event 6: Ambush!
**Description:** Trapped! Surprise wave with no rest time.

**Mechanics:**
- Surprise wave starts immediately (no shop access first)
- Enemies are mixed Elites (3-4 elite enemies)
- No gold drops during ambush
- After surviving: Triple gold reward + guaranteed Rare drop

**Choice:** No choice — forced encounter, high difficulty

---

### Event 7: The Cursed Coin
**Description:** A glowing cursed coin appears on the ground.

**Mechanics:**
- Pick up coin: Receive 500 gold immediately
- Side effect: Take a random Curse for the rest of the run:
  - "Plague Curse": -10% Max HP
  - "Slowness Curse": -10% Movement Speed
  - "Fragility Curse": Take +5% damage
- Don't pick up: No effect, lose nothing

**Choice:** Pure risk/reward — big gold for permanent debuff

---

### Event 8: Champion's Trial
**Description:** A mystical challenger appears to test you 1-on-1.

**Mechanics:**
- Dark version of Champion appears (Dark Knight elite, 2x HP)
- Allies disabled for this duel
- Champion fights alone
- Reward for victory: Permanent +10% ATK and HP
- Penalty for loss: -100 HP for the rest of the run

**Choice:** Test of skill — pure player ability check

---

### Event 9: Time Anomaly
**Description:** Time twists strangely.

**Mechanics:**
- Next wave runs at 2x speed (everything fast — enemies, projectiles, player)
- Reward for surviving: All ally cooldowns/charges reset, +1 free Blessing pick

**Choice:** No choice — bracing for chaos wave

---

### Event 10: The Banished
**Description:** A captured enemy begs to be freed (or killed).

**Mechanics:**
- Mimic Fiend prisoner in cage
- Free it: Mimic Fiend joins your side as ally for 3 waves, then turns hostile
- Kill it: Receive 100 gold and a small Blessing

**Choice:** Risky temporary ally vs steady reward

---

## MYSTERIOUS EVENTS

### Event 11: The Wandering Wizard
**Description:** An old wizard offers to gamble with you.

**Mechanics:**
- Random outcome from a deck of effects (player can choose card):
  - 30%: Beneficial (gold, relic, blessing)
  - 20%: Neutral (just lore)
  - 30%: Challenging (curse, lose item)
  - 20%: Game-changing (random ally upgrade, random Legendary)

**Choice:** Pure RNG gambling

---

### Event 12: The Mirror of Self
**Description:** A magical mirror reflects another path.

**Mechanics:**
- Reroll your Champion to a different one (lose current Champion's relics/blessings, gain new Champion at same level)
- Keep allies you have

**Choice:** Reset/switch playstyle mid-run (last resort)

---

### Event 13: Soul Forge
**Description:** Ancient forge requires sacrifice.

**Mechanics:**
- Sacrifice gold:
  - 100g → Reroll all current Blessings
  - 250g → Upgrade one ally to next tier instantly
  - 500g → Combine 2 Common relics into 1 Rare relic
  - 1000g → Combine 3 Rare relics into 1 Legendary relic

**Choice:** Economy-based crafting

---

### Event 14: Ghost of a Champion
**Description:** Spirit of a deceased Champion appears to grant power.

**Mechanics:**
- Champion's spirit offers their signature Ultimate (one-time use)
- Player chooses one Ultimate from random selection of 3 other Champions
- Adds to Champion's kit for the run (Champion now has 2 Ultimates)

**Choice:** Strategic Ultimate addition

---

### Event 15: The Wager
**Description:** Cosmic gambler appears.

**Mechanics:**
- "Bet your soul" — Bet any amount of gold
- Coin flip:
  - Win: Double the gold + free Rare relic
  - Lose: Lose all gold for next wave (cannot purchase)

**Choice:** All-in gambling moment

---

## EVENT FREQUENCY & TUNING

**Distribution per Run (Average):**
- Beneficial Events: 50% of all events
- Challenging Events: 35% of all events
- Mysterious Events: 15% of all events

**Tuning Goals:**
- Events shouldn't be too frequent (kills pacing)
- Beneficial events shouldn't be free (incentivize gold management)
- Challenging events shouldn't be unavoidable (allow skipping)
- Mysterious events should feel rare and exciting

**Visual Feedback:**
- Beneficial: Golden aura, bell chime
- Challenging: Red warning sign, drum beat
- Mysterious: Purple swirl, ethereal sound

---

## EVENT DISCOVERY & ACHIEVEMENTS

Players slowly discover events through play:
- "Encountered 5 different events" → 1 Achievement
- "Discovered all 15 events" → Cosmetic unlock
- "Survived every challenging event" → Title: "The Steadfast"
- "Lucky 7 wins from Mysterious events" → Title: "Cosmic Lucky"

This adds discovery and surprise to long-term play.


---

# SECTION 22: DESIGN PHILOSOPHY & SUMMARY

## The Core Vision

**Arcane Brigade** is built on three pillars:

1. **Visual Clarity** - Player should INSTANTLY understand every entity on screen
2. **Strategic Depth** - Decisions should matter, builds should differ, mastery should pay off
3. **Compelling Loop** - Each run feels different, fresh, rewarding

---

## What Makes Arcane Brigade Different from Bizarre Brigade?

The original ZZZ Bizarre Brigade is excellent but limited (it's a mini-game). Arcane Brigade expands on its strengths while addressing limitations:

### Improvements

| Bizarre Brigade | Arcane Brigade |
|-----------------|----------------|
| Random companion weapons | FIXED ally weapons (predictable identity) |
| 1 player character with limited weapon | 18 Champions with full weapon switching |
| Simple enemies (palette swaps) | 9 unique enemies + 5 elites + 4 bosses |
| 1 boss type | Multi-boss system with phases |
| 3 damage types | 4 damage types with reactions and combos |
| Bond bonuses (basic) | Bond bonuses + Mixed Bonds + Mastery system |
| 20-minute run length | Variable (10-min Daily to endless 2hr+ runs) |
| No long-term progression | Champion Mastery + Ascension + Achievements |
| Single arena | 6 distinct arenas with hazards |
| No story | Full Story Campaign with chapters |
| No co-op (single player) | Twin Sigils 2-player co-op mode |
| Static items | 30 Named Relics + 28 Blessings + Wave Events |

### What We Kept Intact

The CORE LOOP that makes Bizarre Brigade addictive:
- ✅ Quick decision-making between waves
- ✅ Build-up and snowball feel
- ✅ Visual distinction (melee vs ranged)
- ✅ Allies that fight alongside you
- ✅ Shop economy with refreshes
- ✅ Set bonuses for matching factions
- ✅ Wave-based difficulty scaling
- ✅ Boss as climax

---

## Why These Mechanics Make the Game FUN

### 1. Discovery
Players unlock new content slowly (Champions, allies, relics, blessings, events, modes). Even after 100 hours, players find new things.

### 2. Customization
With 18 Champions, 16 Allies, 30 Relics, 28 Blessings, the build combinations are mathematically enormous. Every player has a "favorite build."

### 3. Mastery
Champion Mastery, Arena Mastery, Ascension — there's always something to grind. Skill-based players will master mechanics; collectors will love unlocks.

### 4. Risk/Reward
Many systems force decisions: Crown of Thorns relic (more damage but take more), Phoenix Crown (revive but only twice), Cursed Coin event (gold for curse), risky Boss Rush mode.

### 5. Social
Daily Challenge has shared seeds — friends compare results. Co-op for partnerships. Leaderboards for competitive players.

### 6. Surprise
Wave Events, random Specialist rolls, Trickster's chaos table, Wyrd's random Ultimate — RNG creates memorable moments.

---

## Tone & Aesthetic Direction

**Setting:**
- High fantasy world inspired by Arthurian legend and Tolkien
- Brigades of noble knights and mages defending the realm from the Voidlord's Scourge
- Magic is sacred and rare; relics are revered

**Visual Style:**
- Stylized 3D (think Genshin Impact / Honkai meets darker themes)
- Vibrant colors with clear team identification
- Red/Orange for melee, Blue/Cyan for ranged, Green/Yellow for support
- Particle effects communicate everything

**Music & Audio:**
- Orchestral fantasy themes with brass for battle
- Ethereal choirs for Mystic effects
- Each arena has signature music
- Bosses have unique themes (memorable Inferno Dragon roar, eerie Voidlord whispers)

**Narrative:**
- The Arcane Brigade is a sacred order defending Aetherium
- The Voidlord seeks to unmake reality
- Champions are unique heroes drawn to the Brigade's call
- Story Mode reveals Brigade history through chapter cutscenes

---

## How a Typical Run Feels

**Phase 1: Setup (Waves 1-3)**
- Pick Champion + Arena
- Light combat, learn the wave
- Buy starting Blessings/Relics
- Decide build direction

**Phase 2: Build-Up (Waves 4-10)**
- Allies unlock and merge
- Faction bonds start activating
- First major Wave Event appears (~Wave 5)
- First boss at Wave 10 — test of build

**Phase 3: Snowball (Waves 11-25)**
- Build comes online (combos firing)
- Multiple Elite enemies appear
- More events introduce variety
- Inferno Dragon at Wave 25

**Phase 4: Climax (Waves 26-50)**
- Build reaches power peak
- Multiple bosses sequence
- Resources running short — tough choices
- Forgotten King at Wave 50

**Phase 5: Endless (Waves 51+ in endless mode)**
- Voidcorruption rules begin
- Scaling becomes brutal
- Survival is the goal
- Eventually death — but memorable run

**Total Run Duration:** 20-45 min (Story), 5-15 min (Daily), 1-3 hours (endless)

---

## Critical Design Principles

These are the bedrock rules that should NEVER be violated:

### Rule 1: Visual Clarity Above All
If a player can't tell what's happening, the design failed. Every weapon, status effect, and ability must be visually distinguishable.

### Rule 2: Allies Are Reliable
Ally weapons are FIXED. Player knows what each ally does. No RNG on the building blocks.

### Rule 3: Champions Have Identity
Each of 18 Champions has a clear playstyle and Ultimate. No generic characters.

### Rule 4: Decisions Have Weight
Shop choices, Wave Events, build direction — every decision should feel meaningful.

### Rule 5: Death Should Teach
When a Champion dies, the player should understand WHY. Telegraphed dangers, clear damage indicators, post-run summary.

### Rule 6: Reward Mastery
Skilled play matters. Crit chains, combo executions, perfect dodges — players should feel skill-based progression alongside item progression.

### Rule 7: Respect Player Time
Daily Challenges = 10 minutes. Story chapters = 30 minutes. Endless = however long they want. Cater to different time budgets.

---

## Roadmap (Suggested Post-Launch Content)

### Update 1: "Beasts of the Wild"
- 3 New Champions
- 4 New Allies (Beast-themed)
- New Arena: Endless Desert
- New Boss: The Sand Wyrm

### Update 2: "Twin Sigils Expansion"
- Co-op specific content
- New co-op-only allies
- New co-op-only events
- Cross-faction tournaments

### Update 3: "The Voidfall Saga"
- Story expansion (Chapter 7)
- New endgame boss: The True Voidlord
- Ascension Levels 11-20
- New relic tier: Cosmic (above Legendary)

### Update 4: "Champions of the Old Order"
- 5 New Champions (legendary heroes returned)
- Story tie-ins
- Champion-specific lore quests

---

## Final Summary

**Arcane Brigade is:**
- A high-fantasy magic roguelike inspired by Bizarre Brigade
- 18 Champions, 16 Allies, 9 Enemies + 5 Elites + 4 Bosses
- 30 Relics + 28 Blessings + 12 Status Effects + 6 Elemental Reactions
- 6 Arenas, 6 Game Modes, 15 Wave Events
- Champion Mastery + Account Progression + Ascension
- Built for solo + co-op
- Designed for both casual 10-min sessions and obsessive 100+ hour mastery

**The Brigade calls. The Voidlord waits.**

*Forge your Brigade. Master the arcane. Hold the line against the Scourge.*

---

## END OF DOCUMENT

**Total Document:**
- 22 Sections
- 18 Champions Fully Detailed
- 16 Allies (12 original + 4 Specialists)
- 14 Enemies (9 regular + 5 elites)
- 4 Bosses
- 30 Named Relics
- 28 Named Blessings
- 18 Champion Ultimates
- 15 Wave Events
- 6 Arenas
- 6 Game Modes
- 12 Status Effects + 6 Elemental Reactions
- Complete scaling formulas
- Complete combat flow documentation
- Implementation pseudocode

**Status:** Production-Ready Design Specification

**Next Steps:**
1. Visual concept art for each Champion, Ally, Enemy
2. Prototype build with 3 Champions, 4 Allies, 1 Arena
3. Playtest core loop with 20 players (5-day cycle)
4. Balance pass based on data
5. Vertical slice: 1 Champion's full Trial of Ascension
6. Full production

---
