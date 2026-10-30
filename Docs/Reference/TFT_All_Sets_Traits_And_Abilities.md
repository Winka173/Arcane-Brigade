# Teamfight Tactics: every set's traits and every champion's ability (reference)

Generated 27/10/2026 from CommunityDragon's raw game data (raw.communitydragon.org, the TFT `en_us.json` of several patches). Numbers are the values in that patch's data, not necessarily the set's launch values. Placeholder names in square brackets are computed values the data does not spell out; `{...}` hashed variables are shown as bare numbers (in the order the data lists them). Star levels in ability numbers read 1-star/2-star/3-star. This file is for design reference only.

## Contents

- [Set 1: Beta (original)](#set-1)
- [Set 2: Rise of the Elements (data name: War of the Elements)](#set-2)
- [Set 3: Galaxies](#set-3)
- [Set 4: Fates](#set-4)
- [Set 5: Reckoning](#set-5)
- [Set 6: Gizmos & Gadgets](#set-6)
- [Set 7: Dragonlands](#set-7)
- [Set 8: Monsters Attack!](#set-8)
- [Set 9: Runeterra Reforged](#set-9)
- [Set 10: Remix Rumble](#set-10)
- [Set 11: Inkborn Fables](#set-11)
- [Set 12: Magic n' Mayhem](#set-12)
- [Set 13: Into the Arcane](#set-13)
- [Set 14: Cyber City](#set-14)
- [Set 15: K.O. Coliseum](#set-15)
- [Set 16: Lore & Legends](#set-16)
- [Set 17: Space Gods](#set-17)
- [Set 18: Enchanted Wilds](#set-18)


<a id="set-1"></a>

# Set 1: Beta (original)

Source: patch 9.20 data. 57 champions, 24 traits.

## Traits

### Assassin

Innate: At the start of combat, Assassins leap to the farthest enemy. Assassins gain bonus Critical Strike Damage and Chance.

  - (3-5) 75, 5
  - (6-8) 150, 15
  - (9) 225, 30

### Blademaster

Blademaster Basic Attacks have a [ProcChance]% chance to trigger additional attacks against their target. These additional attacks deal damage like Basic Attacks and trigger on-hit effects.

  - (3-5) ExtraAttacks=1, 45
  - (6-8) ExtraAttacks=2, 45
  - (9) ExtraAttacks=4, 45

### Brawler

Brawlers gain bonus Maximum Health.

  - (2-3) BonusHealth=250
  - (4-5) BonusHealth=500
  - (6) BonusHealth=900

### Demon

Basic Attacks from Demons have a [ManaDrainProcChance]% chance to reduce their target's Mana by [ManaDrainBurn] and restore Mana to the Demon.

  - (2-3) 20, 15, 40
  - (4-5) 20, 30, 40
  - (6) 20, 45, 40

### Dragon


  - (2) MagicReduction=75

### Elementalist

Innate: Elementalists gain double mana from Basic Attacks.

  - (3) HPMultiplier=null, SizeCoefficient=null, null

### Exile


  - (1) ShieldPercent=100

### Glacial

Basic Attacks from Glacials have a chance to stun their target for [StunDuration] seconds.

  - (2-3) StunDuration=1.5, 20
  - (4-5) StunDuration=1.5, 33
  - (6) StunDuration=1.5, 50

### Guardian


  - (2) Armor=45

### Gunslinger

Gunslinger Basic Attacks have a [ProcPercent]% chance to fire additional bullets at other enemies within range. These bullets deal damage like Basic Attacks and apply on-hit effects.

  - (2-3) 50, 1, 1
  - (4-5) 50, 3, 2
  - (6) 50, 5, 3

### Hextech

At the start of combat, an ally Hextech launches and detonates a pulse bomb, temporarily disabling nearby enemy items for [DisableDuration] seconds.

  - (2-3) 7, 1.25, 1
  - (4) 7, 1.25, 2

### Imperial

At the start of combat, select Imperials are empowered to deal double damage.

  - (2-3) DamageMultiplier=2
  - (4) DamageMultiplier=2

### Knight

All allies block a flat amount of damage from all sources.

  - (2-3) BlockAmount=15
  - (4-5) BlockAmount=35
  - (6) BlockAmount=60

### Ninja

Ninjas gain bonus Attack Damage and Spell Power. This trait is only active when you have exactly 1 or 4 unique Ninjas.

  - (1) 50, 50
  - (4) 80, 80

### Noble

At the start of combat, select allies gain +[Armor] Armor, +[MagicResist] Magic Resistance, and heal [Heal] Health on-hit.

  - (3-5) Armor=50, Heal=30, MagicResist=50
  - (6) Armor=50, Heal=30, MagicResist=50

### Phantom


  - (2) 100

### Pirate


  - (3) (no numbers)

### Ranger

Every [ProcRate] seconds, Rangers have a chance to double their Attack Speed for [ProcRate] seconds.

  - (2-3) ASMultiplier=2, 3, 25
  - (4) ASMultiplier=2, 3, 70

### Robot

([MinUnits]) Robots start combat at full Mana

  - (1) (no numbers)

### Shapeshifter

Shapeshifters gain bonus Maximum Health when they transform.

  - (3-5) HPPercent=60
  - (6) HPPercent=100

### Sorcerer

Innate: Sorcerers gain double Mana from Basic Attacks. All allies have increased Spell Power.

  - (3-5) AP=40
  - (6-8) AP=100
  - (9) AP=175

### Void

Basic Attacks and Spells from select Void champions deal true damage.

  - (2-3) 100, 1
  - (4) 100, 1

### Wild

Select allies' Basic Attacks are empowered to grant +[AttackSpeedPercent]% Bonus Attack Speed (stacks up to 5 times).

  - (2-3) 10
  - (4) 10

### Yordle

Yordles gain a chance to dodge enemy Basic Attacks.

  - (3-5) MissPercent=30
  - (6-8) MissPercent=60
  - (9) MissPercent=90

## Champions (57)

### Camille (1-cost, )

- Traits: Hextech, Blademaster | mana 75/125
- **The Hextech Ultimatum**: Camille singles out and leaps on an enemy, dealing [ModifiedDamage] magic damage and rooting them for 4/5/6 seconds. Allies within range will prioritize attacking Camille's target.

### Darius (1-cost, )

- Traits: Imperial, Knight | mana 0/100
- **Decimate**: Darius swings his axe in a circle, dealing [ModifiedDamage] magic damage and healing himself for [ModifiedHeal] health for each enemy hit.

### Elise (1-cost, )

- Traits: Demon, Shapeshifter | mana 0/100
- **Spider Form**: Elise transforms into a giant spider and summons 1/2/4 spiderlings to fight alongside her. While in spider form, Elise gains [LifestealPercent] bonus lifesteal. Spiderlings benefit from active Demon trait bonuses.

### Fiora (1-cost, )

- Traits: Noble, Blademaster | mana 0/75
- **Riposte**: Fiora enters a defensive stance for 1.5 seconds, becoming immune to damage and enemy spell effects. As she exits this stance, she ripostes, dealing [ModifiedDamage] magic damage to a nearby enemy and stunning them for 1.5 seconds.

### Garen (1-cost, )

- Traits: Noble, Knight | mana 0/100
- **Judgement**: Garen spins his sword for 4 seconds, becoming immune to magic damage and dealing [ModifiedDamage] magic damage to nearby enemies over the duration.

### Graves (1-cost, )

- Traits: Pirate, Gunslinger
- **Buckshot**: Passive: Graves' basic attacks deal 5% bonus physical damage and hit all enemies in a cone in front of him. Graves applies on-hit effects to all enemies hit.

### Kassadin (1-cost, )

- Traits: Void, Sorcerer
- **Nether Blade**: Passive: Kassadin's basic attacks steal 25/50/75 mana from his target and grant him a [ModifiedShield] damage shield for 4 seconds.

### Kha'Zix (1-cost, )

- Traits: Void, Assassin | mana 0/65
- **Taste their Fear**: Kha'Zix slashes the nearest enemy, dealing [ModifiedDamage] magic damage. If the enemy has no adjacent teammates, this damage is increased to [ModifiedIsolationDamage].

### Mordekaiser (1-cost, )

- Traits: Phantom, Knight | mana 50/150
- **Obliterate**: Mordekaiser slams his mace on the two spaces in front of him, dealing [ModifiedDamage] magic damage to enemies within.

### Nidalee (1-cost, )

- Traits: Wild, Shapeshifter | mana 0/85
- **Primal Surge**: Nidalee heals herself and an ally, restoring [ModifiedHeal] health over 6 seconds, then transforms into cat form. While in cat form, Nidalee's range is reduced to 1 hex and she gains 20/70/120 Bonus Attack Damage.

### Tristana (1-cost, )

- Traits: Yordle, Gunslinger | mana 0/50
- **Explosive Charge**: Tristana places a bomb on her current target which will detonate after she Basic Attacks them 3 times or 4 seconds pass without Tristana attacking them. When the bomb detonates, it deals [ModifiedDamage] magic damage to all nearby enemies, plus [PercentDamageIncrease] for each Basic Attack, up to a maximum of [ModifiedMaxDamage].

### Vayne (1-cost, )

- Traits: Noble, Ranger
- **Silver Bolts**: Passive: Whenever Vayne Basic Attacks the same target 3 times, she deals [ModifiedDamage] of her target's Maximum Health as true damage.

### Warwick (1-cost, )

- Traits: Wild, Brawler | mana 50/150
- **Infinite Duress**: Warwick pounces on the weakest enemy, stunning them for 1.5 seconds and striking them 3 times, dealing [ModifiedDamage] magic damage. Each strike triggers on-hit effects and heals Warwick for 100% of the damage dealt.

### Ahri (2-cost, )

- Traits: Wild, Sorcerer | mana 0/75
- **Orb of Deception**: Ahri fires an orb in a line, dealing [ModifiedDamage] magic damage to all enemies it passes through. It then returns to her, dealing [ModifiedDamage] true damage to all enemies it passes through.

### Blitzcrank (2-cost, )

- Traits: Robot, Brawler | mana 0/125
- **Rocket Grab**: Blitzcrank pulls the furthest enemy to him, dealing [ModifiedDamage] magic damage and stunning them for 2.5 seconds. His next attack after pulling knocks up for 1 second. Allies within range will prioritize attacking Blitzcrank's target.

### Braum (2-cost, )

- Traits: Glacial, Guardian | mana 0/75
- **Unbreakable**: Braum puts up his shield for 4 seconds, absorbing and stopping all incoming missiles and reducing his damage taken from that direction by 70/80/90%.

### Jayce (2-cost, )

- Traits: Hextech, Shapeshifter | mana 50/100
- **Transform Mercury Cannon**: Jayce knocks a nearby enemy away, dealing [ModifiedDamage] magic damage and stunning them for 2.5/4.25/6 seconds. He then transforms into cannon form and gains max attack speed for his next 3/5/7 Basic Attacks. While in cannon form, Jayce's Attack Range is increased by 3 hexes.

### Lissandra (2-cost, )

- Traits: Glacial, Elementalist | mana 0/85
- **Frozen Tomb**: Lissandra encases an enemy in ice, stunning them for 1.5 seconds and dealing [ModifiedDamage] magic damage to all enemies in the surrounding area. If she is below [SelfUltPercentage] health, Lissandra instead encases herself, becoming untargetable for 2 seconds and dealing [ModifiedDamage] magic damage to all enemies in the surrounding area.

### Lucian (2-cost, )

- Traits: Noble, Gunslinger | mana 0/35
- **Relentless Pursuit**: Lucian dashes away from his current target, then Basic Attacks them and fires a second shot which deals [ModifiedDamage] magic damage.

### Lulu (2-cost, )

- Traits: Yordle, Sorcerer | mana 50/150
- **Wild Growth**: Lulu makes 1/2/3 allies giant, granting them [ModifiedBonusHealth] bonus health and knocking up adjacent enemies near them for 1.25 seconds.

### Pyke (2-cost, )

- Traits: Pirate, Assassin | mana 50/125
- **Phantom Undertow**: Pyke leaves an afterimage at his location, then dashes behind the farthest enemy. After 1 second, his afterimage returns to Pyke, dealing [ModifiedDamage] magic damage to all enemies it passes through and stunning them for 1.5/2/2.5 seconds.

### Rek'Sai (2-cost, )

- Traits: Void, Brawler | mana 50/150
- **Burrow**: Rek'Sai burrows, becoming untargetable for 1 second and healing for [ModifiedHeal] health. She emerges at her target, dealing [ModifiedDamage] magic damage and knocking them up for 1.75 seconds.

### Shen (2-cost, )

- Traits: Ninja, Blademaster | mana 100/150
- **Spirit's Refuge**: Shen creates a zone around himself for 3/4/5 seconds, granting all nearby allies 100% chance to dodge incoming basic attacks.

### Twisted Fate (2-cost, )

- Traits: Pirate, Sorcerer | mana 0/50
- **Pick a Card**: Twisted Fate randomly picks a card, then throws it at his current target, dealing [ModifiedDamage] magic damage and creating a special effect based on the color of the card. Blue: Restores 30/50/70 mana to all allies adjacent to Twisted Fate. Red: Deals [ModifiedDamage] to all enemies adjacent to the target. Gold: Stuns the target for 2/3/4 seconds.

### Varus (2-cost, )

- Traits: Demon, Ranger | mana 0/75
- **Piercing Arrow**: Varus channels for 1.5 seconds, then fires a piercing arrow, dealing [ModifiedDamage] magic damage to all enemies in its path.

### Zed (2-cost, )

- Traits: Ninja, Assassin | mana 0/75
- **Razor Shuriken**: Zed throws a shuriken in a line, dealing [ModifiedDamage] magic damage to all enemies in its path.

### Aatrox (3-cost, )

- Traits: Demon, Blademaster | mana 0/75
- **The Darkin Blade**: Aatrox cleaves the area in front of him, dealing [ModifiedDamage] magic damage to all enemies within.

### Ashe (3-cost, )

- Traits: Glacial, Ranger | mana 0/125
- **Enchanted Crystal Arrow**: Ashe fires an arrow at the farthest enemy. It stops on the first target hit, dealing [ModifiedDamage] magic damage and stunning them for 1/1.5/2 seconds per Hex traveled.

### Evelynn (3-cost, )

- Traits: Demon, Assassin | mana 0/75
- **Last Caress**: Evelynn deals [ModifiedDamage] magic damage to up to 3 nearby enemies, then teleports away. This damage is increased to [ModifiedCritDamage] against targets below [ExecuteThreshholdPercent] health.

### Gangplank (3-cost, )

- Traits: Pirate, Gunslinger, Blademaster | mana 0/100
- **Powder Kegs**: Passive: Gangplank periodically places barrels near enemies. Active: Gangplank shoots his barrels, causing them all to explode, dealing [ModifiedDamage] magic damage to nearby enemies and applying on-hit effects.

### Katarina (3-cost, )

- Traits: Imperial, Assassin | mana 0/85
- **Death Lotus**: Katarina channels for 2.5 seconds, throwing knives at 4/6/8 nearby enemies, dealing [ModifiedDamage] magic damage over the duration and reducing healing by 80% to her targets for 3 seconds.

### Kennen (3-cost, )

- Traits: Ninja, Yordle, Elementalist | mana 50/150
- **Slicing Maelstrom**: Kennen summons a storm around himself for 3 seconds, calling down lightning bolts on all enemies in the area every 0.5 second and dealing [ModifiedDamage] magic damage over the duration. Each enemy struck by 3 lightning bolts is stunned for 1.5 seconds.

### Morgana (3-cost, )

- Traits: Demon, Sorcerer | mana 50/150
- **Soul Shackles**: Morgana fires chains to all nearby enemies, dealing [ModifiedDamage] magic damage. After 3 seconds, all chained enemies still within range of Morgana are stunned for 2/4/6 seconds and take [ModifiedDamage] magic damage again.

### Poppy (3-cost, )

- Traits: Yordle, Knight | mana 0/75
- **Keeper's Verdict**: Poppy creates a shockwave in a straight line that can hit up to 1/2/3 enemies, dealing [ModifiedDamage] magic damage, knocking up and stunning them for 2/3/4 seconds.

### Rengar (3-cost, )

- Traits: Wild, Assassin | mana 0/75
- **Savagery**: Rengar leaps to and stabs the weakest enemy, dealing [LeapDamage] physical damage and applying on-hit effects to his target. After this leap, Rengar gains [ASBuffPercent] Attack Speed and [CritBuffPercent] Critical Strike Chance for the next 6 seconds.

### Shyvana (3-cost, )

- Traits: Dragon, Shapeshifter | mana 0/85
- **Dragon's Descent**: Shyvana dashes away from her current target and transforms into dragon form. While in dragon form, Shyvana's Basic Attack becomes ranged, she gains 100/150/200 Attack Damage, and her Basic Attacks burn her enemies for 3 seconds, dealing [ModifiedDamage] magic damage.

### Veigar (3-cost, )

- Traits: Yordle, Sorcerer | mana 0/75
- **Primordial Burst**: Veigar blasts an enemy with magical energy, dealing [ModifiedDamage] magic damage. If Veigar is a higher star level than his target, this damage is increased to [ModifiedLowerStarDamage].

### Vi (3-cost, )

- Traits: Hextech, Brawler | mana 75/150
- **Assault and Battery**: Vi charges at the farthest enemy, knocking aside and dealing [ModifiedDamage] magic damage to all enemies along the way. When she reaches her target, she deals [ModifiedDamage] magic damage and knocks them up for 2/2.5/3 seconds.

### Volibear (3-cost, )

- Traits: Glacial, Brawler | mana 0/75
- **Thunder Claws**: Volibear empowers his attacks for the next 20 seconds, causing them to chain to up to 3 nearby enemies. These chains deal 80% physical damage and apply on-hit effects.

### Akali (4-cost, )

- Traits: Ninja, Assassin | mana 0/25
- **Five Point Strike**: Akali throws kunai at her target, dealing 150 magic damage. There is a 25% chance that this spell will critically strike for 225 magic damage instead.

### Aurelion Sol (4-cost, )

- Traits: Dragon, Sorcerer | mana 0/125
- **Voice of Light**: Aurelion Sol breathes fire in a line, dealing [ModifiedDamage] magic damage to all enemies in the area.

### Brand (4-cost, )

- Traits: Demon, Elementalist | mana 50/150
- **Pyroclasm**: Brand launches a bouncing fireball at an enemy. This fireball will bounce to nearby enemies up to 4/6/20 times, dealing [ModifiedDamage] magic damage each time it hits.

### Cho'Gath (4-cost, )

- Traits: Void, Brawler | mana 50/150
- **Rupture**: Cho'Gath ruptures a large area, dealing [ModifiedDamage] magic damage and knocking up all enemies within for 1.5/1.75/2 seconds.

### Draven (4-cost, )

- Traits: Imperial, Blademaster | mana 0/50
- **Spinning Axes**: Draven starts spinning an axe, granting him 150% bonus physical damage on-hit on his next attack. It will return to his location after striking the target and if Draven catches it, it will refresh the buff. Draven can spin up to two axes at a time.

### Gnar (4-cost, )

- Traits: Wild, Yordle, Shapeshifter | mana 0/100
- **GNAR!**: Gnar leaps towards an enemy and transforms into Mega Gnar, throwing nearby enemies, dealing [ModifiedDamage] magic damage and stunning them for 2 seconds. While in Mega Gnar form, Gnar gains 250/450/650 Health and 50/100/150 Attack Damage.

### Jinx (4-cost, )

- Traits: Hextech, Gunslinger
- **Get Excited!**: Passive: Jinx gets excited as she helps take down enemy units. After her first takedown, Jinx gains [PercentAttackSpeed] bonus Attack Speed. After her second takedown, Jinx swaps to her rocket launcher, causing her Basic Attacks to deal [ModifiedDamage] bonus magic damage to all enemies in a small area around her target.

### Kindred (4-cost, )

- Traits: Phantom, Ranger | mana 50/150
- **Lamb's Respite**: Kindred creates a zone around themselves for 3/4/5 seconds that prevents allies within from dropping below 300/600/900 health or dying.

### Leona (4-cost, )

- Traits: Noble, Guardian | mana 50/100
- **Solar Flare**: Leona calls down a solar ray in an area, dealing [ModifiedDamage] magic damage to all enemies within and stunning the enemy in the center for 5/7/9 seconds.

### Sejuani (4-cost, )

- Traits: Glacial, Knight | mana 50/150
- **Glacial Prison**: After a 2 second delay, Sejuani creates a glacial prison on an enemy, dealing [ModifiedDamage] magic damage to all nearby enemeis and stunning them for 2/3.5/5 seconds.

### Anivia (5-cost, )

- Traits: Glacial, Elementalist | mana 50/125
- **Glacial Storm**: Anivia creates a large hailstorm that lasts for 6 seconds, dealing [ModifiedDamage] magic damage over the duration and slowing the attack speed of all enemies inside of it by 50/70/90%.

### Kai'Sa (5-cost, )

- Traits: Void, Ranger, Assassin | mana 50/125
- **Killer Instinct**: Kai'Sa dashes to the furthest enemy unit, gaining +50/75/100% Attack Speed and a shield that blocks 400/700/1000 damage for 3 seconds.

### Karthus (5-cost, )

- Traits: Phantom, Sorcerer | mana 40/125
- **Requiem**: Karthus channels for 2.25 seconds, then deals [ModifiedDamage] magic damage to 5/7/9 random enemies.

### Kayle (5-cost, )

- Traits: Noble, Knight | mana 50/125
- **Divine Judgement**: Kayle shields her 1/2/3 weakest allies, making them immune to all damage for 2/2.5/3 seconds.

### Miss Fortune (5-cost, )

- Traits: Pirate, Gunslinger | mana 0/150
- **Bullet Time**: Miss Fortune channels and fires waves of bullets in a cone for 3 seconds, dealing [ModifiedDamage] magic damage to all enemies within over the duration.

### Pantheon (5-cost, )

- Traits: Dragon, Guardian | mana 150/200
- **Grand Starfall**: Pantheon leaps in the air, crashing down towards the farthest enemy, stunning them for 2 seconds. Enemies in Pantheon's path take [ModifiedDamage] of their Maximum Health as magic damage. They then burn for an additional 20% of their Maximum Health as true damage over 10 seconds, reducing healing by 80%.

### Swain (5-cost, )

- Traits: Imperial, Demon, Shapeshifter | mana 0/100
- **Demonflare**: Swain transforms into demon form for 6 seconds. While in demon form, Swain drains health from nearby enemies, dealing [ModifiedDamage] magic damage and healing himself for [ModifiedHeal] over the duration for each enemy in range. At the end of the transformation, Swain sends out a Soul Flare, dealing [ModifiedSoulFlareDamage] magic damage to all nearby enemies.

### Yasuo (5-cost, )

- Traits: Exile, Blademaster | mana 0/25
- **Steel Tempest**: Yasuo stabs his sword into the two spaces in front of him, dealing [ModifiedDamage] magic damage and applying on-hit effects to enemies within. Every third cast, Yasuo instead throws a tornado in a line that travels 6 hexes and additionally knocks enemies up for 1.5 seconds.

## Monsters, summons, pets and other trait-less units (0)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.


<a id="set-2"></a>

# Set 2: Rise of the Elements (data name: War of the Elements)

Source: patch 10.5 data. 69 champions, 27 traits.

## Traits

### Alchemist

Innate: Alchemists ignore collision and never stop moving.

  - (1) null=null

### Assassin

Innate: At the start of combat, Assassins leap to the farthest enemy. Assassins gain bonus Critical Strike Damage and Chance.

  - (3-5) 70, 10
  - (6) 210, 20

### Avatar

Innate: Having an Avatar on your bench or board transforms all other Avatars in your shop to the Element of that Avatar. (1) An Avatar's Origin Element is counted twice for Trait bonuses

  - (1) null=null

### Berserker

Innate: At the start of combat, Berserkers leap to the nearest enemy. When Berserkers attack, they have a [ProcChance]% chance to hit all units in a cone in front of them and apply on-hit effects.

  - (3-5) null, 2, 100, 45
  - (6) 25, 2, 100, 100

### Blademaster

Blademasters' Basic Attacks have a [ProcChance]% chance to trigger additional attacks against their target. These additional attacks deal damage like Basic Attacks and trigger on-hit effects.

  - (2-3) ExtraAttacks=1, 40
  - (4-5) ExtraAttacks=2, 40
  - (6) ExtraAttacks=3, 35

### Cloud

All allies gain a chance to dodge enemy Basic Attacks.

  - (2) 20
  - (3) 25
  - (4) 35

### Crystal

Crystal Champions have a maximum amount of damage they can take from a single hit.

  - (2-3) 100, 110
  - (4) 100, 60

### Desert

Reduces each enemy's Armor.

  - (2-3) 40
  - (4) 100

### Druid


  - (2) HealAmount=45, 1

### Electric

Electric Champions shock nearby enemies whenever they deal or receive a critical strike.

  - (2) Damage=80
  - (3) Damage=250
  - (4) Damage=550

### Glacial

Basic Attacks from Glacials have a [ProcPercent]% chance to stun their target for [StunDuration] seconds and do bonus Magic Damage.

  - (2-3) BonusDamage=75, StunDuration=1.5, 25
  - (4-5) BonusDamage=175, StunDuration=1.5, 25
  - (6) BonusDamage=475, StunDuration=1.5, 25

### Inferno

Inferno critical hits and spell damage burns the ground beneath the target, dealing a percentage of that spell's pre-mitigation damage over [ZoneDuration] seconds.

  - (3-5) 10, 70, 4, 1
  - (6-8) 10, 150, 4, 3
  - (9) 10, 250, 4, 5

### Light

When a Light Champion dies, all other Light Champions gain Attack Speed and recover [HealAmount]% of the dying champion's maximum health.

  - (3-5) HealAmount=20, 20
  - (6-8) HealAmount=20, 30
  - (9) HealAmount=20, 50

### Lunar

([MinUnits]) Every [Frequency] seconds, all allied champions in combat are infused with Moonlight, increasing Crit Chance by [CritChance]%, Crit Damage by [CritDamage]%, and Spell Power by [AP]% (stacks to 4 times).

  - (2) AP=15, CritChance=15, CritDamage=15, 7

### Mage

Mages have a chance, when casting, to Doublecast instead.

  - (3-5) null, 50
  - (6) 20, 100

### Mountain


  - (2) ShieldValue=1500

### Mystic

All allies gain Magic Resistance.

  - (2-3) MagicResist=40
  - (4) MagicResist=120

### Ocean

All allies gain mana every [TickRate] seconds.

  - (2-3) 4, 10
  - (4-5) 4, 30
  - (6) 4, 60

### Poison


  - (3) Duration=60, 33

### Predator


  - (3) 25

### Ranger

Every [ProcRate] seconds, Rangers have a chance to gain Attack Speed for [ProcRate] seconds.

  - (2-3) ASMultiplier=100, 3, 2, 35
  - (4-5) ASMultiplier=100, 3, 2, 80
  - (6) ASMultiplier=150, 3, 2.5, 100

### Shadow

Shadow Champions deal increased damage for [Duration] seconds at combat start and every time they get a takedown.

  - (3-5) Duration=6, null, 65
  - (6) Duration=6, 1, 150

### Soulbound

The first Soulbound champion to die in a round will instead enter the Spirit Realm, becoming untargetable and continuing to fight as long as another Soulbound ally is alive.

  - (2) (no numbers)

### Steel

Steel Champions gain damage immunity for a few seconds when they are reduced below [HealthPercentThreshold]% health.

  - (2) 50, 2
  - (3) 50, 3
  - (4) 50, 4

### Summoner

Summoned units gain bonus health and last longer.

  - (3-5) 30
  - (6) 120

### Warden

Wardens gain Armor.

  - (2-3) 150
  - (4-5) 300
  - (6) 999

### Woodland

At the start of combat, Woodland Champions make copies of themselves

  - (3-5) null
  - (6) 1

## Champions (69)

### Diana (1-cost, )

- Traits: Inferno, Assassin | mana 0/100
- **Flame Cascade**: Diana creates 3/4/6 flame orbs which rotate around her and explode for [ModifiedDamage] magic damage when they collide with an enemy. She also shields herself for 4 seconds, absorbing the next [ModifiedShield] damage she would take.

### Ivern (1-cost, )

- Traits: Woodland, Druid | mana 50/100
- **Triggerseed**: Ivern shields the lowest-health ally for 5 seconds, absorbing the next [ModifiedShield] damage.

### Kog'Maw (1-cost, )

- Traits: Poison, Predator | mana 0/40
- **Living Artillery**: Kog'Maw launches an acidic blob at a random enemy, dealing [ModifiedDamage] magic damage.

### Leona (1-cost, )

- Traits: Lunar, Warden | mana 50/100
- **Lunar Barrier**: Leona reduces all damage by 40/80/120 for 4 seconds.

### Maokai (1-cost, )

- Traits: Woodland, Druid
- **Sap Magic**: Passive: After being damaged by an enemy spell, Maokai's next attack heals him for [ModifiedHeal].

### Nasus (1-cost, )

- Traits: Light, Warden | mana 0/100
- **Fury of the Dawn**: Nasus surrounds himself in light for 7 seconds, gaining [ModifiedBonusHealth] maximum health and dealing [ModifiedDamage] magic damage to adjacent enemies every second.

### Ornn (1-cost, )

- Traits: Electric, Warden | mana 0/75
- **Lightning Breath**: Ornn breathes lightning in a cone, dealing [ModifiedDamage] magic damage to enemies therein, and increasing their chance to be critically struck by 25% for the next 10 seconds.

### Renekton (1-cost, )

- Traits: Desert, Berserker | mana 0/100
- **Cull the Meek**: Renekton swings his blade, dealing [ModifiedDamage] magic damage to nearby enemies and healing himself for [ModifiedHeal] per enemy hit.

### Taliyah (1-cost, )

- Traits: Mountain, Mage | mana 30/80
- **Seismic Shove**: Taliyah causes the ground to erupt under a random enemy, dealing [ModifiedDamage] magic damage to them, and stunning them for 2 seconds. This also knocks ranged enemies closer, or pushes melee enemies away.

### Vayne (1-cost, )

- Traits: Light, Ranger
- **Silver Bolts**: Passive: Whenever Vayne Basic Attacks the same target 3 times, she deals [ModifiedDamage] of her target's maximum health as true damage.

### Vladimir (1-cost, )

- Traits: Ocean, Mage | mana 0/75
- **Drain**: Vladimir deals [ModifiedDamage] magic damage to the target and heals for the same amount.

### Warwick (1-cost, )

- Traits: Glacial, Predator | mana 50/125
- **Infinite Duress**: Warwick pounces on the weakest enemy, stunning them for 1.5 seconds and striking them 3 times, dealing [ModifiedDamage] magic damage. Each strike triggers on-hit effects and heals Warwick for 100% of the damage dealt.

### Zyra (1-cost, )

- Traits: Inferno, Summoner | mana 0/65
- **Rampant Growth**: Zyra summons 2/3/5 Flame Spitters at random locations on the edge of the arena. They attack the nearest enemy for 4 seconds, dealing [ModifiedDamage] magic damage per attack a total of 4 times.

### Braum (2-cost, )

- Traits: Glacial, Warden | mana 0/65
- **Unbreakable**: Braum puts up his shield for 4 seconds, absorbing and stopping all incoming missiles and reducing his damage taken from that direction by 80/85/90%.

### Jax (2-cost, )

- Traits: Light, Berserker | mana 50/125
- **Counter Strike**: Jax dodges all incoming attacks for 2 seconds, then strikes all nearby enemies, dealing [ModifiedDamage] magic damage and stunning them for 1.5 seconds.

### LeBlanc (2-cost, )

- Traits: Woodland, Assassin, Mage | mana 50/80
- **Ethereal Chain**: LeBlanc launches an ethereal chain at a random enemy, dealing [ModifiedDamage] magic damage and, after a short delay, stunning them for 1.5 seconds.

### Malzahar (2-cost, )

- Traits: Shadow, Summoner | mana 75/125
- **Shadow Swarm**: Malzahar summons 2/3/4 Shadow Spawn, who hit for [ModifiedDamage] magic damage each attack. Shadow Spawn benefit from active Shadow trait bonuses.

### Neeko (2-cost, )

- Traits: Woodland, Druid | mana 0/60
- **Blooming Burst**: Neeko throws a seed at a random target that explodes for [ModifiedDamage] magic damage three times. Each explosion affects a bigger area than the last.

### Rek'Sai (2-cost, )

- Traits: Steel, Predator | mana 0/75
- **Furious Bite**: Rek'Sai bites an enemy, dealing [ModifiedDamage] true damage.

### Senna (2-cost, )

- Traits: Shadow, Soulbound | mana 50/75
- **Piercing Darkness**: Senna fires a beam through her furthest ally, dealing [ModifiedDamage] magic damage to enemies, and buffing allies' on-hits for 5 seconds to deal [ModifiedAllyDamage] magic damage from Senna.

### Skarner (2-cost, )

- Traits: Crystal, Predator | mana 0/65
- **Crystalline Exoskeleton**: Skarner gains a shield against [ModifiedShield] damage for 8 seconds. While the shield persists, Skarner gains 50/75/100% attack speed.

### Syndra (2-cost, )

- Traits: Ocean, Mage | mana 0/65
- **Hydro Sphere**: Syndra conjures a sphere of water which explodes near a random enemy, dealing [ModifiedDamage] magic damage to all enemies hit.

### Thresh (2-cost, )

- Traits: Ocean, Warden | mana 50/125
- **Deep Sea Lantern**: Thresh throws his lantern to the lowest-health ally, shielding them and nearby allies against [ModifiedShield] damage for 5 seconds.

### Varus (2-cost, )

- Traits: Inferno, Ranger | mana 50/100
- **Piercing Arrow**: Varus channels for 1.5 seconds, then fires a piercing arrow, dealing [ModifiedDamage] magic damage to all enemies in its path.

### Volibear (2-cost, )

- Traits: Glacial, Electric, Berserker | mana 0/75
- **Frenzied Bite**: Volibear bites his target, dealing [ModifiedDamage] magic damage. If the target is below 35% health, Frenzied Bite kills the target and fully restores Volibear's mana.

### Yasuo (2-cost, )

- Traits: Cloud, Blademaster | mana 0/90
- **Last Breath**: Yasuo blinks to the enemy with the most items and knocks them up, holding them airborne for 1 seconds and hitting them 4/5/6 times, dealing Basic Attack damage and applying on-hit effects.

### Aatrox (3-cost, )

- Traits: Light, Blademaster | mana 0/70
- **The Arclight Blade**: Aatrox cleaves the area in front of him, dealing [ModifiedDamage] magic damage to all enemies therein.

### Azir (3-cost, )

- Traits: Desert, Summoner | mana 50/100
- **Arise!**: Azir summons a Sand Soldier near a random enemy. Sand Soldiers attack nearby enemies whenever Azir attacks, dealing [ModifiedDamage] magic damage in a line, and last 6 seconds.

### Dr. Mundo (3-cost, )

- Traits: Poison, Berserker | mana 75/125
- **Adrenaline Rush**: Dr. Mundo creates a toxic cloud around him for 8 seconds. The cloud deals [ModifiedDamage] + 1.25% of Dr. Mundo's maximum health as magic damage to nearby enemies each second, and heals him for 100/125/150% of that value.

### Ezreal (3-cost, )

- Traits: Glacial, Ranger | mana 0/30
- **Ice Shot**: Ezreal fires a shard of ice at the lowest-health enemy, dealing [ModifiedDamage] magic damage and applying on-hit effects.

### Karma (3-cost, )

- Traits: Lunar, Mystic | mana 75/100
- **Inspire**: At start of combat, Karma tethers to her closest ally. Karma shields the tethered ally (or a random one if the tether is dead) for 5 seconds, absorbing the next [ModifiedShield] damage. While the shield holds, the ally receives [ModifiedAS]% bonus Attack Speed.

### Kindred (3-cost, )

- Traits: Shadow, Inferno, Ranger | mana 0/35
- **Dance of Dread**: Wolf mauls Kindred's target, dealing [ModifiedDamage] magic damage and reducing their healing by 80% for 5 seconds. Meanwhile, Lamb leaps away from Kindred's target.

### Nautilus (3-cost, )

- Traits: Ocean, Warden | mana 50/125
- **Depth Charge**: Nautilus sends out a depth charge that seeks out his farthest enemy, knocking them up and stunning them for 3/4/6 seconds. It also deals [ModifiedDamage] magic damage to all enemies it passes through.

### Nocturne (3-cost, )

- Traits: Steel, Assassin
- **Steel Blades**: Passive - Every three hits Nocturne's attack deals damage and applys on-hit effects to all adjacent units. It also heals [ModifiedHeal] of the damage dealt.

### Qiyana (3-cost, )

- Traits: Ocean, Assassin | mana 50/100
- **Elemental Blade**: Qiyana blasts a short line in front of her, stunning enemies for 2.5/3.5/4.5 seconds and dealing [ModifiedDamage] magic damage.

### Qiyana (3-cost, )

- Traits: Ocean, Assassin | mana 50/100
- **Elemental Blade**: Qiyana blasts a short line in front of her, stunning enemies for 2.5/3.5/4.5 seconds and dealing [ModifiedDamage] magic damage.

### Qiyana (3-cost, )

- Traits: Inferno, Assassin | mana 50/100
- **Elemental Blade**: Qiyana blasts a short line in front of her, stunning enemies for 2.5/3.5/4.5 seconds and dealing [ModifiedDamage] magic damage.

### Qiyana (3-cost, )

- Traits: Mountain, Assassin | mana 50/100
- **Elemental Blade**: Qiyana blasts a short line in front of her, stunning enemies for 2.5/3.5/4.5 seconds and dealing [ModifiedDamage] magic damage.

### Qiyana (3-cost, )

- Traits: Cloud, Assassin | mana 50/100
- **Elemental Blade**: Qiyana blasts a short line in front of her, stunning enemies for 2.5/3.5/4.5 seconds and dealing [ModifiedDamage] magic damage.

### Sion (3-cost, )

- Traits: Shadow, Berserker | mana 75/125
- **Decimating Smash**: Sion winds up for a moment, then smashes his axe down in a direction, knocking up and dealing [ModifiedDamage] magic damage to all enemies hit.

### Sivir (3-cost, )

- Traits: Desert, Blademaster | mana 0/65
- **Ricochet**: For the next 5 seconds, Sivir's attacks bounce up to 5/7/9 times, dealing [DamageCalc] physical damage to enemies hit and applying on-hit effects.

### Soraka (3-cost, )

- Traits: Light, Mystic | mana 0/60
- **Equinox**: Soraka calms the area around a random enemy for 3/5/7 seconds, dealing [ModifiedDamage] magic damage to all enemies standing in it and preventing them from gaining mana.

### Veigar (3-cost, )

- Traits: Shadow, Mage | mana 0/60
- **Primordial Burst**: Veigar blasts an enemy, dealing [ModifiedDamage] magic damage. If Veigar is a higher star level than his target they are instantly killed instead.

### Annie (4-cost, )

- Traits: Inferno, Summoner | mana 0/75
- **Tibbers!**: Annie summons Tibbers near her target, dealing [ModifiedDamage] magic damage to all adjacent enemies. Tibbers fights for the rest of the round or until killed, dealing [ModifiedTibbers] magic damage with each Basic Attack. Annie can't gain mana while Tibbers is active.

### Ashe (4-cost, )

- Traits: Crystal, Ranger | mana 0/35
- **Ranger's Focus**: Ashe gains 50/75/100% Attack Speed for 5 seconds, and her Basic Attacks fire a flurry of arrows for [ModifiedADDamage] physical damage.

### Brand (4-cost, )

- Traits: Inferno, Mage | mana 0/80
- **Pyroclasm**: Brand launches a bouncing fireball at an enemy. This fireball will bounce to nearby enemies up to 5/7/20 times, dealing [ModifiedDamage] magic damage each time it hits.

### Janna (4-cost, )

- Traits: Cloud, Mystic | mana 50/125
- **Monsoon**: Janna summons a Monsoon around her, healing allies for [ModifiedHeal]% of their max health over 3 seconds. Nearby enemies are knocked back and stunned for 1.5 second.

### Kha'Zix (4-cost, )

- Traits: Desert, Assassin | mana 0/40
- **Arid Assault**: Kha'Zix stealths and attacks the lowest health enemy, critically striking for [ModifiedDamage] physical damage and gaining 5/10/15 mana.

### Lucian (4-cost, )

- Traits: Light, Soulbound | mana 0/50
- **The Culling**: Lucian fires 12 shots in a direction over 4 seconds, each hitting for 50% of his AD, applying on-hits, and dealing [ModifiedDamage] magic damage. Lucian fires extra shots based on his Attack Speed. (Bonus Shots: [ShotsFromAS]) Lucian will dash during The Culling to keep hitting enemies.

### Malphite (4-cost, )

- Traits: Mountain, Warden | mana 50/150
- **Unstoppable Force**: Malphite charges to a random enemy, dealing [ModifiedDamage] magic damage to nearby enemies, knocking them into the air, and stunning them for 2/2.5/5 seconds.

### Olaf (4-cost, )

- Traits: Glacial, Berserker | mana 0/90
- **Ragnarok**: Olaf gains 125/150/450% Attack Speed, up to 50% Lifesteal based on missing health, and immunity to crowd control for the rest of the round.

### Twitch (4-cost, )

- Traits: Poison, Ranger | mana 0/70
- **Spray and Pray**: Twitch gains infinite range and has [BonusADDamage] Attack Damage for 8 seconds. During this time his attacks travel their full range and hit every enemy they pass through. This also applies on-hit effects.

### Yorick (4-cost, )

- Traits: Light, Summoner | mana 0/85
- **Shepherd of Souls**: Yorick blesses his 2/3/12 lowest-health allies (except Minions of Light). When they die they are resurrected as a Minion of Light with 500/700/2000 health and [ModifiedAD] attack damage. Minions of Light benefit from active Light trait bonuses.

### Amumu (5-cost, )

- Traits: Inferno, Warden | mana 75/150
- **Curse of the Sad Mummy**: Amumu explodes in an infernal tantrum, dealing [RModifiedDamage] magic damage to all enemies within 3 Hexes and stunning them for 1.5/2/10 seconds.

### Master Yi (5-cost, )

- Traits: Shadow, Mystic, Blademaster | mana 100/150
- **Meditate**: Master Yi meditates, becoming untargetable for 1 seconds and healing [ModifiedHeal] of his max health over the duration. After Master Yi finishes meditating, he gains 100% Attack Speed and deals [ModifiedDamage] bonus magic damage on hit for 6 seconds.

### Nami (5-cost, )

- Traits: Ocean, Mystic | mana 50/125
- **Tidal Wave**: Nami summons a massive tidal wave which deals [ModifiedDamage] magic damage to all enemies hit, knocking up and stunning them for 1.5/2/2.5 seconds. Allies hit are empowered, and their Basic Attacks deal an additional [ModifiedBoost] magic damage for the next 8 seconds.

### Singed (5-cost, )

- Traits: Poison, Alchemist
- **Poison Trail**: Passive: Singed leaves a toxic cloud wherever he moves, poisoning enemies who stand in it for [ModifiedDamage] magic damage over 6 seconds.

### Taric (5-cost, )

- Traits: Crystal, Warden | mana 50/125
- **Cosmic Radiance**: After a 1.5 second delay, Taric pulses cosmic energy onto allied champions within 2/3/4 hexes, making them invulnerable for 2.5/2.5/8 seconds.

### Zed (5-cost, )

- Traits: Electric, Summoner, Assassin | mana 75/150
- **Living Lightning**: Zed creates a clone of himself behind his current target. This clone inherits its creator's items, stats, and current health, and can cast Living Lightning. Its mana cost is 50/25/-125 higher.

### Lux (7-cost, )

- Traits: Crystal, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Electric, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Glacial, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Inferno, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Light, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Steel, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Ocean, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Shadow, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Cloud, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

### Lux (7-cost, )

- Traits: Woodland, Avatar | mana 0/80
- **Final Spark**: Lux fires an elemental blast dealing [ModifiedDamage] magic damage to all enemies in a line. If Final Spark kills a unit, Lux is refunded 50 mana.

## Monsters, summons, pets and other trait-less units (0)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.


<a id="set-3"></a>

# Set 3: Galaxies

Source: current client data (Galaxies revival). 82 champions, 26 traits.

## Traits

### Astro

([MinUnits]) Astro abilities cost [ManaCostReduction] less Mana.

  - (3) ManaCostReduction=30

### Battlecast

After dealing or taking 10 instances of damage, Battlecast champions deal magic damage to the nearest enemy. If below 50% Health, heal that much instead.

  - (2-3) HealOrDamageAmount=90
  - (4-5) HealOrDamageAmount=200
  - (6-7) HealOrDamageAmount=400
  - (8) HealOrDamageAmount=1000

### Blademaster

Blademasters' attacks have a chance to trigger additional attacks against the current target.

  - (3-5) ExtraAttacks=1, ProcChance=30
  - (6-8) ExtraAttacks=2, ProcChance=30
  - (9) ExtraAttacks=2, ProcChance=80

### Blaster

Every 4th attack, Blasters fire additional attacks at random enemies. These additional attacks can critically hit.

  - (2-3) AdditionalAttacks=1
  - (4) AdditionalAttacks=2

### Brawler

Brawlers gain Health.

  - (2-3) BonusHealth=350
  - (4) BonusHealth=600

### Celestial

Your team gains Omnivamp.

  - (2-3) HealPercent=12
  - (4-5) HealPercent=25
  - (6) HealPercent=50

### Chrono

Your team gains [PercentAttackSpeed]% Attack Speed at the start of combat and every few seconds.

  - (2-3) Frequency=7, PercentAttackSpeed=10
  - (4-5) Frequency=4, PercentAttackSpeed=10
  - (6-7) Frequency=1.5, PercentAttackSpeed=10
  - (8) Frequency=0.5, PercentAttackSpeed=10

### Cybernetic

Cybernetics holding at least 1 item gain Attack Damage and Health.

  - (3-5) ADAmp=0.3, HealthAmp=350
  - (6) ADAmp=0.6, HealthAmp=700

### Dark Star

Whenever an allied champion dies, Dark Stars gain Attack Damage and Ability Power.

  - (2-3) ADandSP=6
  - (4-5) ADandSP=15
  - (6-7) ADandSP=25
  - (8) ADandSP=45

### Demolitionist


  - (2) StunDuration=1.5

### Infiltrator

Innate: At the start of combat, Infiltrators move to the enemy's backline. Infiltrators gain Attack Speed for [Duration] seconds at the start of combat, refreshed on enemy takedowns.

  - (2-3) AttackSpeedPercent=25, Duration=6, Reset=1
  - (4-5) AttackSpeedPercent=45, Duration=6, Reset=1
  - (6) AttackSpeedPercent=75, Duration=6, Reset=1

### Mana-Reaver

Mana Reave: Increase max Mana by [CostIncreasePercent]% until the next cast.

  - (2) CostIncreasePercent=30

### Mech-Pilot

([MinUnits]) Start of Combat: 3 random Mech-Pilots merge into a Super-Mech. The Super-Mech has the Traits of its pilots, 3 random items from among them, and gains 65% of the combined pilots' Health and Attack Damage. When the Super-Mech dies, the Pilots are ejected with [EjectedPilotHealthPercent]% of their max Health.

  - (3) CleaveRatio=0.8, EjectedPilotHealthPercent=35, MechCombinedADRatio=0.65, 1500, 0.65, None, 90

### Mercenary

([MinUnits]) Innate: Upgrades for Mercenaries' abilities have a chance to appear in the shop.

  - (1) (no numbers)

### Mystic

Your team gains Magic Resist.

  - (2-3) MagicResist=30
  - (4) MagicResist=80

### Paragon

([MinUnits]) Your team's attacks are converted to magic damage. Your Star Guardians' attacks deal true damage instead.

  - (1) (no numbers)

### Protector

Whenever a Protector casts their ability, they gain a Shield for [Duration] seconds.

  - (2-3) Duration=4, ShieldPercent=30
  - (4-5) Duration=4, ShieldPercent=40
  - (6) Duration=4, ShieldPercent=50

### Rebel

At start of combat for [ShieldDuration] seconds, Rebels gain a Shield and bonus damage for each adjacent Rebel.

  - (3-5) DamageAmp=6, ShieldAmount=150, ShieldDuration=8
  - (6-8) DamageAmp=10, ShieldAmount=225, ShieldDuration=8
  - (9) DamageAmp=20, ShieldAmount=400, ShieldDuration=8

### Sniper

Snipers deal bonus damage for each hex between themselves and their target.

  - (2-3) PercentDamageIncrease=9
  - (4) PercentDamageIncrease=16

### Sorcerer

Your team gains Ability Power.

  - (2-3) AP=25
  - (4-5) AP=45
  - (6) AP=75

### Space Pirate

Whenever a Space Pirate lands a killing blow, there is a chance to drop loot.

  - (2-3) CoinDropChance=50, ItemDropChance=None
  - (4) CoinDropChance=50, ItemDropChance=33

### Star Guardian

Whenever a Star Guardian casts, an amount of Mana is divided between all other Star Guardians.

  - (3-5) ManaRestore=15
  - (6-8) ManaRestore=25
  - (9) ManaRestore=80

### Starship

([MinUnits]) Starships gain [ManaPerSecond] Mana per second, maneuver around the board, and are immune to movement impairing effects, but don't attack.

  - (1) ManaPerSecond=40

### Valkyrie


  - (2) PercentHealth=40

### Vanguard

Vanguards gain Armor.

  - (2-3) Armor=70
  - (4-5) Armor=150
  - (6) Armor=350

### Void

([MinUnits]) Attacks and spells from Void champions deal true damage.

  - (3) (no numbers)

## Champions (57)

### Caitlyn (1-cost, APCaster)

- Traits: Chrono, Sniper | mana 20/110
- **Ace In The Hole**: After a delay, fire a bullet towards the furthest enemy that deals [ModifiedDamage] () magic damage to the first enemy it hits.

### Fiora (1-cost, ADFighter)

- Traits: Cybernetic, Blademaster | mana 0/85
- **Riposte**: For 1.5 seconds, become immune to damage and enemy ability effects. After, deal [ModifiedDamage] () magic damage to a nearby enemy and Stun them for 1.5/1.5/2 seconds.

### Graves (1-cost, APFighter)

- Traits: Space Pirate, Blaster | mana 30/80
- **Smoke Grenade**: Launch a smoke grenade toward the enemy with the most Attack Speed. It explodes on impact, dealing [ModifiedDamage] () magic damage to adjacent enemies and Disarms them for 1.5 seconds. Disarm: Cannot move or attack

### Illaoi (1-cost, APTank)

- Traits: Battlecast, Brawler | mana 40/80
- **Tentacle Smash**: Slam a tentacle in a line, dealing [ModifiedDamage] () magic damage and stealing 45/45/55% Armor and Magic Resist from each target hit for 6 seconds.

### Jarvan IV (1-cost, APTank)

- Traits: Dark Star, Protector | mana 60/100
- **Ageless Standard**: Allies within 3 hexes gain [ModifiedAS] () Attack Speed for 6 seconds.

### Leona (1-cost, APTank)

- Traits: Cybernetic, Vanguard | mana 40/80
- **Cyber Barrier**: Reduce all incoming damage by [ModifiedDamageReduction] () for 4 seconds.

### Malphite (1-cost, APTank)

- Traits: Rebel, Brawler
- **Energy Shield**: Passive: Start of combat: gain [TooltipShield] () Shield.

### Nocturne (1-cost, APReaper)

- Traits: Battlecast, Infiltrator | mana 40/80
- **Unspeakable Horror**: Stun the target for 1/1.25/1.5 seconds and deal [ModifiedDamage] () magic damage over the duration.

### Poppy (1-cost, APTank)

- Traits: Star Guardian, Vanguard | mana 30/75
- **Buckler Toss**: Throw the buckler at the furthest enemy that deals [ModifiedDamage] () magic damage. When it bounces back, gain [ModifiedShield] () Shield.

### Twisted Fate (1-cost, APCaster)

- Traits: Chrono, Sorcerer | mana 0/75
- **Wild Cards**: Throw three cards in a cone that deal [ModifiedDamage] () magic damage to each enemy they pass through.

### Xayah (1-cost, ADCarry)

- Traits: Celestial, Blademaster | mana 0/60
- **Deadly Plumage**: Gain [ModifiedAS] () Attack Speed for 8 seconds.

### Ziggs (1-cost, APCaster)

- Traits: Rebel, Demolitionist | mana 0/40
- **Bomb!**: Deal [ModifiedDamage] () magic damage to the current target.

### Zoe (1-cost, APCaster)

- Traits: Star Guardian, Sorcerer | mana 40/80
- **Sleepy Trouble Bubble**: Kick a bubble at the enemy with the highest Health, dealing [ModifiedDamage] () magic damage and Stunning for 2/2/2.5 seconds. Highest total Health breaks targeting ties.

### Ahri (2-cost, APCaster)

- Traits: Star Guardian, Sorcerer | mana 0/60
- **Orb of Deception**: Fire an orb in a line, dealing [ModifiedDamage] () magic damage to all enemies it passes through. It returns dealing [ModifiedDamage] () true damage to all enemies it passes through.

### Annie (2-cost, APTank)

- Traits: Mech-Pilot, Sorcerer | mana 50/120
- **Galaxy Shield-Blast**: Deal [ModifiedDamage] () magic damage to enemies in a cone and gain a [Modifiedshield] () Shield for 4 seconds.

### Blitzcrank (2-cost, APTank)

- Traits: Chrono, Brawler | mana 175/175
- **Rocket Grab**: Pull the furthest enemy into melee range, dealing [ModifiedDamage] () magic damage and Stunning them for 2 seconds. Allies will prioritize attacking them. Blitzcrank's next attack Stuns for 1 second.

### Darius (2-cost, APFighter)

- Traits: Space Pirate, Mana-Reaver | mana 0/50
- **Dreadnova Guillotine**: Deal [ModifiedDamage] () magic damage. If this kills the target, immediately cast this again. Targets below 50% Health take double damage.

### Kog'Maw (2-cost, APCarry)

- Traits: Battlecast, Blaster | mana 0/30
- **Barrage**: Gain infinite Attack Range, 60% Attack Speed, and deal [ModifiedPercentMaxHPDamage]% () of the target's maximum Health as bonus magic damage per attack for 3 seconds.

### Lucian (2-cost, ADCarry)

- Traits: Cybernetic, Blaster | mana 0/30
- **Relentless Pursuit**: Dash away from the current target, then attack them twice, with the second attack dealing [ModifiedDamage] () magic damage.

### Mordekaiser (2-cost, APTank)

- Traits: Dark Star, Vanguard | mana 0/90
- **Indestructible**: Gain [Modifiedshield] () Shield for 8 seconds. While the Shield is up, deal [ModifiedDamage] () magic damage per second to all adjacent enemies.

### Nautilus (2-cost, APTank)

- Traits: Astro, Vanguard | mana 50/120
- **Impact Crater**: Deal [ModifiedDamage] () magic damage and Stun the current target for 2/2/2.5 seconds. Enemies within 1 hex receive 50% of this effect.

### Rakan (2-cost, APTank)

- Traits: Celestial, Protector | mana 50/100
- **Grand Entrance**: Dash to the furthest enemy within range and Stun all enemies within 1 hex for 1.5/1.5/2 seconds, dealing [ModifiedDamage] () magic damage.

### Shen (2-cost, APTank)

- Traits: Chrono, Blademaster | mana 100/150
- **Future's Refuge**: All nearby allies automatically dodge incoming attacks for 2.5/3/5 seconds. While active, gain [ModifiedMR] () Magic Resist.

### Xin Zhao (2-cost, ADFighter)

- Traits: Celestial, Protector | mana 0/60
- **Three Talon Strike**: Attack the current target three times, dealing [TooltipDamage] () total physical damage. The third strike Stuns them for 1 seconds and deals [ModifiedDamage] () bonus magic damage.

### Yasuo (2-cost, ADFighter)

- Traits: Rebel, Blademaster | mana 0/80
- **Last Breath**: Teleport to the furthest enemy in range. Stun them for 1 seconds and attack them [ModifiedAttacks] () times.

### Zed (2-cost, ADReaper)

- Traits: Rebel, Infiltrator
- **Contempt For The Weak**: Passive: Every third attack, deal [ModifiedDamage] () bonus magic damage and steal 25/25/30% of the current target's Attack Damage.

### Ashe (3-cost, APCaster)

- Traits: Celestial, Sniper | mana 40/100
- **Enchanted Crystal Arrow**: Fire an arrow at the farthest enemy that explodes on the first target hit, dealing [ModifiedDamage] () magic damage to all adjacent enemies and Stun them for 1/1/1.5 seconds.

### Bard (3-cost, APCaster)

- Traits: Astro, Mystic | mana 0/90
- **Traveler's Call**: Passive: Attacks grant [ModifiedManaPerAttack] () additional Mana. Mana is not gained in rounds without enemy champions. Active: Spawn a meep on the bench. Meeps sell for 1 XP.

### Cassiopeia (3-cost, APCaster)

- Traits: Battlecast, Mystic | mana 0/30
- **Noxious Blast**: Launch poison at the nearest unpoisoned enemy, dealing [ModifiedDamage] () magic damage over 14 seconds, and reducing the effectiveness of new Shields by 50%.

### Ezreal (3-cost, APCaster)

- Traits: Chrono, Blaster | mana 30/80
- **E.M.P.**: Fire a pulse at a random enemy, dealing [ModifiedDamage] () magic damage to all nearby enemies on impact and 20% Mana Reaving them. Mana Reave: Increase max Mana until the next cast

### Jayce (3-cost, APFighter)

- Traits: Space Pirate, Vanguard | mana 0/80
- **To the Skies!**: Deal [ModifiedDamage] () magic damage to adjacent enemies.

### Karma (3-cost, APCaster)

- Traits: Dark Star, Mystic | mana 75/100
- **Inspire**: Passive: At start of combat, bond to the closest ally. Active: Grant the bonded ally (or a random one if that ally is dead) [Modifiedshield] () Shield for 4 seconds. While the Shield holds, they have [ModifiedAS] () bonus Attack Speed.

### Master Yi (3-cost, ADCarry)

- Traits: Rebel, Blademaster | mana 0/55
- **Chosen One**: For 6 seconds, gain increased Move Speed and attacks deal [ModifiedDamage] () bonus true damage. While active, heal [ModifiedHeal] () Health every second.

### Neeko (3-cost, APTank)

- Traits: Star Guardian, Protector | mana 35/110
- **Pop Blossom**: Deal [ModifiedDamage] () magic damage to all nearby enemies and Stun them for 1.5/1.75/2 seconds.

### Rumble (3-cost, APFighter)

- Traits: Mech-Pilot, Demolitionist | mana 0/50
- **Flamespitter**: Deal [ModifiedDamage] () magic damage in a cone over 3 seconds, and Wound for 5 seconds. Wound: Reduce healing received by 50%

### Shaco (3-cost, ADCarryCrit)

- Traits: Dark Star, Infiltrator | mana 30/70
- **Deceive**: Teleport behind the current target and attack the target for [ModifiedDamage] () physical damage. This is always a critical hit.

### Syndra (3-cost, APCaster)

- Traits: Star Guardian, Sorcerer | mana 20/60
- **Unleashed Power**: Pull in all orbs on the battlefield and create 3 new ones, then fire them all at the enemy with the highest percent Health, dealing [ModifiedDamage] () magic damage per orb. Lowest total Health breaks targeting ties.

### Vayne (3-cost, ADCarry)

- Traits: Cybernetic, Sniper | mana 0/50
- **Final Hour**: Focus for [ModifiedDuration] () seconds, tumbling away from the current target immediately and every third attack thereafter. After tumbling, become untargetable, shed negative effects, and the next attack deals [TooltipDamage] () physical damage.

### Vi (3-cost, APTank)

- Traits: Cybernetic, Brawler | mana 70/140
- **Assault and Battery**: Charge the farthest enemy, knocking aside and dealing [ModifiedSecondaryDamage] () magic damage to all enemies along the way. When the target is reached, deal [ModifiedPrimaryDamage] () magic damage and Stun them for 2/2.5/3 seconds.

### Fizz (4-cost, APReaper)

- Traits: Mech-Pilot, Infiltrator | mana 50/120
- **Chum the Waters**: Throw a lure at the current target. After a delay a shark emerges dealing [ModifiedDamage] () magic damage to all enemies within 2 hexes, Stunning them for 1.5 seconds and knocking them back.

### Gnar (4-cost, ADFighter)

- Traits: Astro, Brawler | mana 20/120
- **GNAR!**: Transform and gain 750/1250/2000 Health and 90/95/200% Attack Damage. Leap over the current target and knock back and Stun adjacent enemies for 1 seconds, dealing [ModifiedDamage] () magic damage.

### Irelia (4-cost, ADFighter)

- Traits: Cybernetic, Mana-Reaver, Blademaster | mana 0/30
- **Bladesurge**: Dash to the current target and deal [TooltipDamage] () physical damage. If this kills the target, cast again immediately at the enemy with the highest Mana.

### Jhin (4-cost, ADCarry)

- Traits: Dark Star, Sniper | mana 0/4
- **Whisper**: Passive: Attack Speed is always 0.85/0.85/1. Convert 1% bonus Attack Speed into 0.8% Attack Damage. Every fourth shot deals [TooltipDamage] () physical damage.

### Jinx (4-cost, ADCarryCrit)

- Traits: Rebel, Blaster
- **Get Excited!**: Passive: After the first takedown, gain [PercentAttackSpeed] bonus Attack Speed. After a second takedown, swap to the rocket launcher, causing attacks to deal [ModifiedDamage] () bonus magic damage to all enemies in a small area.

### Riven (4-cost, APFighter)

- Traits: Chrono, Blademaster | mana 0/30
- **Energy Slash**: Dash and gain [Modifiedshield] () Shield. Then, deal [ModifiedDamage] () magic damage to adjacent enemies. Every third cast, launch a wave of energy that deals [ModifiedDamageFinal] () magic damage.

### Soraka (4-cost, APCaster)

- Traits: Star Guardian, Mystic | mana 50/120
- **Wish**: Heal your team for [ModifiedHeal] ().

### Teemo (4-cost, APCaster)

- Traits: Astro, Sniper | mana 0/60
- **Satellite Traps**: Toss 3 traps around the nearest enemy. When an enemy nears a trap (or after 3 seconds) it explodes, dealing [ModifiedDamage] () damage to nearby enemies and slowing their movement speed for 3 seconds.

### Viktor (4-cost, APCaster)

- Traits: Battlecast, Sorcerer | mana 0/60
- **Death Ray**: Burn a path between the two enemies most distant from one another, first dealing [ModifiedPercentHealthDamage]% () maximum Health in magic damage, then 1 second later dealing [ModifiedFlatDamage] () magic damage in an explosion along the same path.

### Wukong (4-cost, APTank)

- Traits: Chrono, Vanguard | mana 50/150
- **Cyclone**: Deal [ModifiedDamage] () magic damage to nearby enemies over 3 seconds. The first time each enemy is hit, Stun them for 2/2/8 seconds.

### Aurelion Sol (5-cost, APCaster)

- Traits: Rebel, Starship | mana 120/120
- **Mobilize the Fleet**: Launch 4/4/10 fighters that fly out to random enemies, dealing [ModifiedDamage] () magic damage and draining 10/15/50 Mana, and then return. Subsequent casts launch all returned fighters, plus 2/2/10 additional.

### Ekko (5-cost, APFighter)

- Traits: Cybernetic, Infiltrator | mana 80/150
- **Chronobreak**: Freeze all units in time before attacking 5/7/20 times dealing [ModifiedDamage] () bonus magic damage. Each strike targets the highest Attack Speed enemy and Chills them for 1.5/2/7 seconds. Chill: Reduce Attack Speed by 30%

### Gangplank (5-cost, APFighter)

- Traits: Space Pirate, Mercenary, Demolitionist | mana 70/160
- **Orbital Strike**: Call down 1/1/9 orbital strike(s) on the current target. After 2 seconds, deal [ModifiedDamage] () magic damage to all enemies within 3 hexes.

### Janna (5-cost, APCaster)

- Traits: Star Guardian, Paragon | mana 20/100
- **Howling Gale**: Summon 5 tornados which fly forward in a large cone. Allies hit gain [ModifiedPercentAttackSpeed]% () Attack Speed for 5 seconds. Enemies hit are Stunned for 1.5/1.5/10 seconds.

### Lulu (5-cost, APCaster)

- Traits: Celestial, Mystic | mana 50/120
- **Mass Polymorph**: Polymorph the 2/3/100 nearest enemies for 2/2.5/15 seconds, Stunning them and causing them to hop aimlessly. Polymorphed enemies take [ModifiedBonusDamage] () additional damage from all sources.

### Thresh (5-cost, APTank)

- Traits: Chrono, Mana-Reaver | mana 50/90
- **Temporal Passage**: Toss a lantern towards 1/1/9 random unit(s) on your bench, pulling onto the battlefield with [ModifiedMana] () bonus Mana. Traits are unaffected.

### Urgot (5-cost, APCaster)

- Traits: Battlecast, Protector | mana 50/120
- **Fear Beyond Death**: Fire a drill toward the farthest enemy and reel in the first enemy hit over 3/2/1 seconds. Once reeled in, repeatedly deal 2000/2000/10000 true damage very quickly until they die. After his first successful cast, the Mana cost of this ability is reduced by [ModifiedManaCostReduction] ().

### Xerath (5-cost, APCarry)

- Traits: Dark Star, Sorcerer | mana 20/80
- **Abyssal Bombardment**: Summon meteors to strike random enemies in place of normal attacks for 6/8/45 seconds. Meteors deal [ModifiedDamage] () magic damage upon impact and if they kill their target, all adjacent enemies take [ModifiedAoEDamage] () magic damage and are Stunned for 1.5 seconds.

## Monsters, summons, pets and other trait-less units (25)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Cytes (1-cost, )

- Traits: (none)
- **(no ability)**: 

### Elder Dragon (1-cost, ADTank)

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Infiltrator (1-cost, )

- Traits: (none)
- **(no ability)**: 

### Kayn (1-cost, )

- Traits: (none) | mana 75/100
- **Blade's Reach**: Kayn swings his scythe, dealing 200 to enemies in a line.

### Kayn (1-cost, )

- Traits: (none) | mana 175/200
- **Umbral Trespass**: Kayn temporarily infests the furthest enemy, dealing 1500 to the target when he bursts out.

### Krug (1-cost, ADTank)

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Raptors (1-cost, )

- Traits: (none) | mana 0/100
- **Enrage**: When one Arcraptor dies, the others gain bonus armor and attack speed.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, ADTank)

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### game_character_displayname_SRUKrugMini (1-cost, )

- Traits: (none)
- **Revitalize**: When one Terraform dies, it heals all others to full health.

### Meep (3-cost, )

- Traits: (none) | mana 0/100
- **Sell For Experience**: Meeps can be sold in the shop for experience points. 1 meep = 1 XP

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Super-Mech (10-cost, APTank)

- Traits: (none) | mana 0/200
- **Mecha Justice**: Passive: Attacks cleave the area in front of it dealing [FinalAttackDamage] () physical damage. Active: Call down a devastating blast at the target's location, dealing [ModifiedMainDamage] () magic damage to target and [ModifiedSplashDamage] () magic damage to all other nearby enemies. This spell's damage increases with the Star Level of the Super-Mech's Pilots.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!


<a id="set-4"></a>

# Set 4: Fates

Source: patch 15.5 data (Fates revival). 141 champions, 26 traits.

## Traits

### Adept

Adepts calm the flow of battle, reducing the Attack Speed of all enemies by 50% for a few seconds at the start of combat.

  - (2) 2
  - (3) 3.5
  - (4) 6

### Assassin

Innate: Leap to the enemy backline when combat starts. Assassins' Abilities can critically strike and they gain bonus Critical Strike Chance and bonus Critical Strike Damage.

  - (2-3) CritAmpPercent=25, CritChanceAmpPercent=10
  - (4-5) CritAmpPercent=55, CritChanceAmpPercent=30
  - (6) CritAmpPercent=90, CritChanceAmpPercent=50

### Brawler

Brawlers gain bonus Health and Attack Damage.

  - (2-3) BonusAD=10, BonusHealth=400
  - (4-5) BonusAD=20, BonusHealth=700
  - (6-7) BonusAD=40, BonusHealth=1000
  - (8) BonusAD=80, BonusHealth=1400

### Cultist

Once your team loses [CultistHealthPercentTaken]% of their Health, Galio is summoned, slamming into the largest cluster of enemies and Stunning them. Galio's strength increases based on total star level of all active Cultists.

  - (3-5) Health=800, 1, 80, 0.16, None, None, 50
  - (6-8) Health=1300, 2, 165, 0.16, None, None, 50
  - (9-10) Health=2000, 3, 330, 0.16, 20, 60, 50
  - (11) Health=2000, 3, 330, 0.16, 20, 60, 50

### Dazzler

Dazzlers' spells reduce the Attack Damage of enemies hit for [Duration] seconds.

  - (2-3) Duration=8, 40
  - (4) Duration=8, 80

### Divine

Upon attacking [NumAttacksToProc] times or dropping below [HealthThreshold]% Health, Divine champions remove all crowd control and ascend, gaining Durability and dealing bonus true damage for [Duration] seconds.

  - (2-3) DamageReductionPercent=25, Duration=6, HealthThreshold=50, 6, 25
  - (4-5) DamageReductionPercent=40, Duration=6, HealthThreshold=50, 6, 40
  - (6-7) DamageReductionPercent=55, Duration=6, HealthThreshold=50, 6, 55
  - (8-2500) DamageReductionPercent=70, Duration=6, HealthThreshold=50, 6, 70

### Duelist

Innate: Duelists move faster. Duelists' attacks grant Attack Speed, up to [MaxStacks] stacks.

  - (2-3) AttackSpeed=10, MaxStacks=10
  - (4-5) AttackSpeed=15, MaxStacks=10
  - (6-7) AttackSpeed=25, MaxStacks=10
  - (8) AttackSpeed=35, MaxStacks=10

### Dusk

Dusk champions increase all allies' Spell Power.

  - (2-3) None, 20
  - (4-5) 50, 20
  - (6) 70, 40

### Elderwood

Every 2 seconds all Elderwood champions grow, gaining bonus stats. This effect stacks up to 5 times.

  - (3-5) BonusADSP=8, BonusResists=15, MaxStacks=5, TickRate=2, 0.02
  - (6-8) BonusADSP=12, BonusResists=20, MaxStacks=5, TickRate=2, 0.04
  - (9) BonusADSP=20, BonusResists=30, MaxStacks=5, TickRate=2, 0.06

### Emperor

Azir deploys with two Sand Guards who can be placed anywhere on the battlefield. They do not move or attack, and die when Azir does.

  - (1) (no numbers)

### Enlightened

Enlightened champions generate more Mana.

  - (2-3) 50
  - (4-5) 100
  - (6-7) 150
  - (8) 250

### Exile

If an Exile has no adjacent allies at the start of combat, they gain:

  - (1) LifeSteal=None, ShieldPercent=50
  - (2) LifeSteal=60, ShieldPercent=50

### Fortune


  - (3-5) Heal=None, 1
  - (6-9) Heal=None, 3, None
  - (10) Heal=2, 3, None

### Hunter

Every few seconds, all Hunters will attack the lowest percent Health enemy, dealing bonus damage.

  - (2) DamageAmp=125, ShotFrequency=3.5
  - (3) DamageAmp=150, ShotFrequency=3
  - (4) DamageAmp=175, ShotFrequency=2.5
  - (5) DamageAmp=200, ShotFrequency=2

### Keeper

At the start of combat, Keepers grant themselves and all adjacent allies a Shield for a duration. This Shield is 50% stronger on Keepers.

  - (2-3) Duration=12, ShieldAmount=175, 50
  - (4-5) Duration=12, ShieldAmount=225, 50
  - (6-7) Duration=12, ShieldAmount=275, 50
  - (8) Duration=12, ShieldAmount=500, 50

### Mage

Mages cast their Abilities twice and have modified total Ability Power.

  - (3-4) ProcChance=100, 80
  - (5-6) ProcChance=100, 105
  - (7-9) ProcChance=100, 130
  - (10) MaxMana=20, ProcChance=100, 190

### Moonlight

([MinUnits]) At the start of combat, a number of Moonlight Champions star up once until combat ends. (Prioritizes the lowest star-level champions. If tied, champions with the most items are chosen.)

  - (3-4) 1
  - (5) 2

### Mystic

Your team gains Magic Resist.

  - (2) MagicResist=40
  - (3) MagicResist=80
  - (4) MagicResist=150
  - (5) MagicResist=250

### Ninja

Ninjas gain bonus Attack Damage and Ability Power. This trait is only active when you have exactly 1 or 4 unique Ninjas.

  - (1) ADValue=50, APValue=50
  - (4) ADValue=140, APValue=140

### Shade

Innate: When combat starts, Shades teleport to the enemy backline. Shades dip into the shadows after every [NumberOfAttacks] attacks, stealthing and causing their next basic attack to deal bonus magic damage.

  - (2) Damage=125, 3
  - (3) Damage=400, 3
  - (4-28) Damage=750, 3

### Sharpshooter

Sharpshooters' attacks and Abilities ricochet to nearby enemies, dealing reduced damage.

  - (2-3) MaxBounces=1, PercentDamageReduction=55
  - (4-5) MaxBounces=2, PercentDamageReduction=50
  - (6) MaxBounces=3, PercentDamageReduction=45

### Spirit

The first time each Spirit casts, all allies gain Attack Speed.

  - (2-3) AttackSpeedPercent=15
  - (4-5) AttackSpeedPercent=25
  - (6-9999) AttackSpeedPercent=40

### The Boss

When Sett first drops below 40% Health, he leaves combat to start doing sit-ups. Each sit-up restores [HealPercent]% of his maximum Health and gives him [AttackSpeedPercent]% Attack and Move Speed. If he reaches full Health he returns to combat Pumped Up: all of his attacks and spells deal true damage. If all of his allies die, he will immediately return to combat.

  - (1) AttackSpeedPercent=40, HealPercent=15

### Tormented

Tormented can be transformed after participating in [RoundsToTransform] combats, enhancing their abilities.

  - (1) 3

### Vanguard

Vanguards gain bonus Armor and Magic Resist.

  - (2-3) Armor=125, MR=20
  - (4-5) Armor=275, MR=40
  - (6-7) Armor=450, MR=60
  - (8) Armor=800, MR=100

### Warlord

Warlords have bonus Health and Ability Power. Each victorious player combat increases the bonus by [VictoryPercent]%, stacking up to [MaxCombats] times.

  - (3-5) InitialHealth=300, MaxCombats=5, VictoryPercent=10, 30
  - (6-8) InitialHealth=450, MaxCombats=5, VictoryPercent=10, 45
  - (9-10) InitialHealth=700, MaxCombats=5, VictoryPercent=10, 70
  - (11) InitialHealth=888, MaxCombats=5, VictoryPercent=88, 88

## Champions (116)

### Brand (1-cost, APCaster)

- Traits: Dragonsoul, Mage | mana 0/60
- **Dragonfire Pillar**: After a brief delay, cause the ground under the enemy with the highest current Health to erupt in flames, dealing [ModifiedDamage] () magic damage to enemies within a 1-hex radius. Enemies caught in the epicenter are Stunned for 1.5/2/3 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Diana (1-cost, )

- Traits: Moonlight, Assassin | mana 0/70
- **Pale Cascade**: Gain a [ModifiedShield] () Shield over 4 seconds and summon 4/5/7 orbs to orbit. These orbs explode for [ModifiedDamage] () magic damage when they hit an enemy, refreshing the Shield when the final orb explodes. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Diana (1-cost, APFighter)

- Traits: Spirit, Assassin | mana 0/70
- **Pale Cascade**: Gain a [ModifiedShield] () Shield over 4 seconds and summon 4/5/6 orbs to orbit. These orbs explode for [ModifiedDamage] () magic damage when they hit an enemy, refreshing the Shield when the final orb explodes. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Elise (1-cost, APTank)

- Traits: Cultist, Keeper | mana 0/75
- **Spider Form**: Transform into a spider, gaining [ModifiedPercentHealth] () maximum Health. Spider Form attacks heal for [ModifiedLifesteal] () and 35/35/50% Mana Reave the target. Mana Reave: increase maximum Mana until the next cast [TFTUnitProperty.:TFT4_Headliner_TRA]

### Elise (1-cost, APTank)

- Traits: Cultist, Keeper | mana 0/75
- **Spider Form**: Transform into a spider, gaining [ModifiedPercentHealth] () maximum Health. Spider Form attacks heal for [ModifiedLifesteal] () and 35/35/50% Mana Reave the target. Mana Reave: increase maximum Mana until the next cast [TFTUnitProperty.:TFT4_Headliner_TRA]

### Fiora (1-cost, APFighter)

- Traits: Enlightened, Duelist | mana 0/95
- **Riposte**: Enter a defensive stance for 1.5 seconds, becoming immune to damage and enemy spell effects. Afterwards, strike, dealing [ModifiedDamage] () magic damage to a nearby enemy and Stunning them for 1/1.5/2 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Fiora (1-cost, APFighter)

- Traits: Enlightened, Duelist | mana 0/95
- **Riposte**: Enter a defensive stance for 1.5 seconds, becoming immune to damage and enemy spell effects. Afterwards, strike, dealing [ModifiedDamage] () magic damage to a nearby enemy and Stunning them for 1/1.5/2 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Garen (1-cost, APTank)

- Traits: Warlord, Vanguard | mana 0/100
- **Judgement**: Spin for 4 seconds, dealing [ModifiedDamage] () magic damage to nearby enemies and reducing incoming magic damage by 80% over the duration. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Garen (1-cost, APTank)

- Traits: Warlord, Vanguard | mana 0/100
- **Judgement**: Spin for 4 seconds, dealing [ModifiedDamage] () magic damage to nearby enemies and reducing incoming magic damage by 80% over the duration. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Lissandra (1-cost, )

- Traits: Moonlight, Dazzler | mana 0/50
- **1000 Daggers**: Hurl a dagger toward the enemy with the highest Attack Damage, dealing [ModifiedDamage] to the first target hit. After hitting its initial target or at its final destination, the dagger explodes, dealing [ModifiedSecondaryDamage] to nearby targets. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Maokai (1-cost, APTank)

- Traits: Elderwood, Brawler | mana 0/75
- **Bramble Smash**: Smash the ground, sending a shockwave towards the current target that deals [ModifiedDamage] () magic damage to enemies hit and Chills them for 3 seconds. Chill: Reduce Attack Speed by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Maokai (1-cost, APTank)

- Traits: Elderwood, Brawler | mana 0/75
- **Bramble Smash**: Smash the ground, sending a shockwave towards the current target that deals [ModifiedDamage] () magic damage to enemies hit and Chills them for 3 seconds. Chill: Reduce Attack Speed by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Nami (1-cost, )

- Traits: Enlightened, Mage | mana 60/100
- **Aqua Prison**: Trap the closest enemy in a bubble, dealing [ModifiedDamage] and Stunning them for 2.5/3/4 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Nasus (1-cost, APTank)

- Traits: Divine, Syphoner | mana 0/70
- **Wither**: Wither the enemy with the highest percent Health, dealing [ModifiedDamage] () magic damage over 5 seconds, Chilling them, and slowing their movement speed by 50% for the duration. Chill: Reduce Attack Speed by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Nidalee (1-cost, APCarry)

- Traits: Warlord, Sharpshooter | mana 0/60
- **Javelin Toss**: Throw a javelin at the farthest enemy, dealing [ModifiedDamage] () magic damage plus 80% more for each hex traveled. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Nidalee (1-cost, APCarry)

- Traits: Warlord, Sharpshooter | mana 0/60
- **Javelin Toss**: Throw a javelin at the farthest enemy, dealing [ModifiedDamage] () magic damage plus 80% more for each hex traveled. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Tahm Kench (1-cost, APTank)

- Traits: Fortune, Brawler
- **Thick Skin**: Take [ModifiedDR] () reduced damage from all sources. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Tahm Kench (1-cost, APTank)

- Traits: Fortune, Brawler
- **Thick Skin**: Take [ModifiedDR] () reduced damage from all sources. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Tristana (1-cost, APCarry)

- Traits: Dragonsoul, Sharpshooter | mana 0/40
- **Rapid Fire**: Gain 60/65/70% Attack Speed and attacks deal [ModifiedBonusDamage] () bonus magic damage for 3 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Twisted Fate (1-cost, APCaster)

- Traits: Cultist, Mage | mana 0/70
- **Wild Cards**: Throw 3 cards in a cone toward the current target, dealing [ModifiedDamage] () magic damage to each enemy they pass through. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Twisted Fate (1-cost, APCaster)

- Traits: Cultist, Mage | mana 0/70
- **Wild Cards**: Throw 3 cards in a cone toward the current target, dealing [ModifiedDamage] () magic damage to each enemy they pass through. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Vayne (1-cost, )

- Traits: Dusk, Sharpshooter
- **Silver Bolts**: Passive: Every third attack deals [ModifiedDamage] bonus true damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Wukong (1-cost, ADFighter)

- Traits: Divine, Vanguard | mana 0/50
- **Crushing Blow**: Slam the target with a staff, dealing [TooltipDamage] () physical damage and Stunning them for 1.5/2/3 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Wukong (1-cost, ADFighter)

- Traits: Divine, Vanguard | mana 0/50
- **Crushing Blow**: Slam the target with a staff, dealing [TooltipDamage] () physical damage and Stunning them for 1.5/2/3 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Yasuo (1-cost, ADFighter)

- Traits: Exile, Duelist | mana 0/45
- **Striking Steel**: Strike forward 2 hexes, dealing [TooltipDamage] () physical damage to enemies hit. If 2 enemies wouldn't be hit by this, attempt to dash to a better position. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Yasuo (1-cost, ADFighter)

- Traits: Exile, Duelist | mana 0/45
- **Striking Steel**: Strike forward 2 hexes, dealing [TooltipDamage] () physical damage to enemies hit. If 2 enemies wouldn't be hit by this, attempt to dash to a better position. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Annie (2-cost, APTank)

- Traits: Fortune, Mage | mana 0/65
- **Burst Shield**: Blast a cone of fire, dealing [ModifiedDamage] () magic damage to enemies in front, then gain a [ModifiedShield] () Shield for 8 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Annie (2-cost, APTank)

- Traits: Fortune, Mage | mana 0/65
- **Burst Shield**: Blast a cone of fire, dealing [ModifiedDamage] () magic damage to enemies in front, then gain a [ModifiedShield] () Shield for 8 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Aphelios (2-cost, )

- Traits: Moonlight, Hunter | mana 50/80
- **Sentry Turret**: Summon a turret for [ModifiedDuration] seconds. The turret deals damage as Aphelios, applies Aphelios' on-hit effects, and dies when Aphelios dies. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Braum (2-cost, APTank)

- Traits: Dragonsoul, Vanguard | mana 30/60
- **Unbreakable**: Gain [ModifiedShieldDR] () Durability in the direction of the largest cluster of enemies for 4 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Hecarim (2-cost, )

- Traits: Elderwood, Vanguard | mana 0/110
- **Spirit Of Life**: Create an aura around for 3 seconds. Each enemy within takes [ModifiedDamage], and heal for [ModifiedHealing] over the duration. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Janna (2-cost, APCaster)

- Traits: Enlightened, Mystic | mana 40/70
- **Eye of the Storm**: Shield the 2/3/4 lowest-Health allies for [ModifiedShieldAmount] () for 4 seconds. Shielded allies also gain 15/20/25% bonus Attack Damage for that duration. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Janna (2-cost, APCaster)

- Traits: Enlightened, Mystic | mana 40/70
- **Eye of the Storm**: Shield the 2/3/4 lowest-Health allies for [ModifiedShieldAmount] () for 4 seconds. Shielded allies also gain 15/20/25% bonus Attack Damage for that duration. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Jarvan IV (2-cost, APFighter)

- Traits: Warlord, Keeper | mana 50/100
- **Dragon Strike**: Throw a standard near the farthest enemy and pull towards it with a lance, dealing [DamageCalc] () magic damage to enemies passed through, Stunning them for 1 second. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Jarvan IV (2-cost, APFighter)

- Traits: Warlord, Keeper | mana 50/100
- **Dragon Strike**: Throw a standard near the farthest enemy and pull towards it with a lance, dealing [DamageCalc] () magic damage to enemies passed through, Stunning them for 1 second. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Jax (2-cost, APTank)

- Traits: Divine, Duelist | mana 60/110
- **Counter Strike**: Dodge all incoming attacks for 2 seconds, then strike all nearby enemies, dealing [ModifiedDamage] () magic damage and Stunning them for 1.5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Jax (2-cost, APTank)

- Traits: Divine, Duelist | mana 60/110
- **Counter Strike**: Dodge all incoming attacks for 2 seconds, then strike all nearby enemies, dealing [ModifiedDamage] () magic damage and Stunning them for 1.5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Lulu (2-cost, APCasterHighMana)

- Traits: Elderwood, Mage | mana 75/140
- **Wild Growth**: Transform a low-Health ally into a giant that Stuns nearby enemies for 1 second. Giant allies gain [ModifiedBonusHealth] () bonus Health for the rest of combat. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Lulu (2-cost, APCasterHighMana)

- Traits: Elderwood, Mage | mana 75/140
- **Wild Growth**: Transform a low-Health ally into a giant that Stuns nearby enemies for 1 second. Giant allies gain [ModifiedBonusHealth] () bonus Health for the rest of combat. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Nautilus (2-cost, APTank)

- Traits: Fabled, Vanguard | mana 85/150
- **Impact Crater**: Cause the ground beneath the target to erupt, knocking them up, Stunning them for 3/4/5 seconds, and dealing [ModifiedDamage] () magic damage. Fabled Bonus: Also gain a shield that gives 50% Durability for 5 seconds. Durability: Reduce damage received [TFTUnitProperty.:TFT4_Headliner_TRA]

### Pyke (2-cost, APReaper)

- Traits: Cultist, Assassin, Slayer | mana 60/120
- **Phantom Undertow**: Leave a phantom at the current location, then dash behind the farthest enemy. After 1 second, the phantom returns, dealing [ModifiedDamage] () magic damage to all enemies it passes through and Stunning them for 1.5/2/2.75 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Pyke (2-cost, APReaper)

- Traits: Cultist, Assassin, Slayer | mana 60/120
- **Phantom Undertow**: Leave a phantom at the current location, then dash behind the farthest enemy. After 1 second, the phantom returns, dealing [ModifiedDamage] () magic damage to all enemies it passes through and Stunning them for 1.5/2/2.75 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Rakan (2-cost, APTank)

- Traits: Elderwood, Keeper | mana 60/120
- **Disarming Diversion**: Dash to the furthest enemy within attack range, Disarming all units passed through for 2/2.5/3 seconds, and Taunting the target. Create a [ModifiedShield] () Shield for 4 seconds. Disarm: Target cannot attack [TFTUnitProperty.:TFT4_Headliner_TRA]

### Sylas (2-cost, )

- Traits: Moonlight, Brawler | mana 30/60
- **Chain Lash**: Lash chains in a 3-hex line, dealing [ModifiedDamage] to enemies hit and increasing the mana cost of their next spell by 33%. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Teemo (2-cost, APCaster)

- Traits: Spirit, Sharpshooter | mana 0/60
- **Sporecloud Dart**: Fire a dart that explodes into a cloud of spores, poisoning nearby enemies in 1-hex area. Poisoned enemies take [ModifiedDamage] () over 4/5/6 seconds and are Blinded for 1 seconds. Blind: Attack might miss [TFTUnitProperty.:TFT4_Headliner_TRA]

### Teemo (2-cost, APCaster)

- Traits: Spirit, Sharpshooter | mana 0/60
- **Sporecloud Dart**: Fire a dart that explodes into a cloud of spores, poisoning nearby enemies in 1-hex area. Poisoned enemies take [ModifiedDamage] () over 4/5/6 seconds and are Blinded for 1 seconds. Blind: Attack might miss [TFTUnitProperty.:TFT4_Headliner_TRA]

### Thresh (2-cost, )

- Traits: Dusk, Vanguard | mana 50/100
- **Spectral Lantern**: Throw a lantern to the lowest-Health ally, shielding them and nearby allies against [ModifiedShield] damage for 4 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Vi (2-cost, APFighter)

- Traits: Warlord, Brawler | mana 0/50
- **Denting Blow**: Deal [ModifiedDamage] () magic damage to all enemies in a cone toward the current target and Sunder them by 20/30/50% for 8 seconds. Sunder: Reduce Armor [TFTUnitProperty.:TFT4_Headliner_TRA]

### Vi (2-cost, APFighter)

- Traits: Warlord, Brawler | mana 0/50
- **Denting Blow**: Deal [ModifiedDamage] () magic damage to all enemies in a cone toward the current target and Sunder them by 20/30/50% for 8 seconds. Sunder: Reduce Armor [TFTUnitProperty.:TFT4_Headliner_TRA]

### Vladimir (2-cost, APTank)

- Traits: Cultist, Syphoner | mana 0/60
- **Transfusion**: Deal [ModifiedDamage] () damage to the target. All allies within 2-hex radius are healed for 50% of the damage dealt. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Zed (2-cost, )

- Traits: Ninja, Shade
- **Contempt For The Weak**: Passive: Every third attack deals [ModifiedDamage] () bonus magic damage and steals 25/30/35% of the target's current Attack Damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Zed (2-cost, ADFighter)

- Traits: Ninja, Slayer
- **Contempt For The Weak**: Innate: At the start of combat, jumps to the enemy backline. Passive: Every third attack deals [ModifiedDamage] () bonus magic damage and steals 25/30/35% of the target's current Attack Damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Akali (3-cost, APReaper)

- Traits: Ninja, Assassin | mana 0/40
- **Five Point Strike**: Throw kunai at the current target, dealing [ModifiedDamage] () magic damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Akali (3-cost, APReaper)

- Traits: Ninja, Assassin | mana 0/40
- **Five Point Strike**: Throw kunai at the current target, dealing [ModifiedDamage] () magic damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Darius (3-cost, APFighter)

- Traits: Fortune, Slayer | mana 0/50
- **Fortune's Guillotine**: Dunk the current target, dealing [ModifiedDamage] () magic damage. While dunking, become Unstoppable. If the target dies, immediately cast again, dealing 25/20/15% reduced damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Evelynn (3-cost, )

- Traits: Cultist, Shade | mana 0/60
- **Last Caress**: Deal [ModifiedDamage] to up to 3 nearby enemies, then teleport away. This damage increases to [ModifiedCritDamage] against targets below [ExecuteThreshholdPercent] Health. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Irelia (3-cost, APTank)

- Traits: Enlightened, Divine, Adept | mana 50/100
- **Bladestorm**: Launch a storm of blades in a line at the current target, dealing [ModifiedDamage] () magic damage and Disarming enemies for 2.5/3/5 seconds. Disarm: Target cannot attack [TFTUnitProperty.:TFT4_Headliner_TRA]

### Irelia (3-cost, APTank)

- Traits: Enlightened, Divine, Adept | mana 50/100
- **Bladestorm**: Launch a storm of blades in a line at the current target, dealing [ModifiedDamage] () magic damage and Disarming enemies for 2.5/3/5 seconds. Disarm: Target cannot attack [TFTUnitProperty.:TFT4_Headliner_TRA]

### Jinx (3-cost, )

- Traits: Fortune, Sharpshooter | mana 70/120
- **Fishbones**: Fire a rocket at the target, which explodes on impact, dealing [ModifiedDamage] to all nearby enemies and Stunning them for 1.5/2/2.5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kalista (3-cost, APCarryNoCast)

- Traits: Cultist, Duelist
- **Rend**: Passive: Spears remain in the target, each dealing [ModifiedDamage] () of the target's max Health when removed. Spears are removed when doing so would kill the target. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kalista (3-cost, APCarryNoCast)

- Traits: Cultist, Duelist
- **Rend**: Passive: Spears remain in the target, each dealing [ModifiedDamage] () of the target's max Health when removed. Spears are removed when doing so would kill the target. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Katarina (3-cost, APReaper)

- Traits: Warlord, Fortune, Assassin | mana 0/90
- **Death Lotus**: Channel for 2.5 seconds, throwing knives at 4/5/6 enemies in a 2-hex radius, dealing [ModifiedDamage] () magic damage over the duration and applying Wound for 5 seconds. Wound: Reduce healing received by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Katarina (3-cost, APReaper)

- Traits: Warlord, Fortune, Assassin | mana 0/90
- **Death Lotus**: Channel for 2.5 seconds, throwing knives at 4/5/6 enemies in a 2-hex radius, dealing [ModifiedDamage] () magic damage over the duration and applying Wound for 5 seconds. Wound: Reduce healing received by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kennen (3-cost, APCasterHighMana)

- Traits: Ninja, Keeper | mana 50/125
- **Slicing Maelstrom**: Summon a storm for 3 seconds, calling down lightning bolts on all enemies every 0.5 second, dealing [ModifiedDamage] () magic damage over the duration. Each enemy struck by 3 lightning bolts is Stunned for 1.5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kennen (3-cost, APCasterHighMana)

- Traits: Ninja, Keeper | mana 50/125
- **Slicing Maelstrom**: Summon a storm for 3 seconds, calling down lightning bolts on all enemies every 0.5 second, dealing [ModifiedDamage] () magic damage over the duration. Each enemy struck by 3 lightning bolts is Stunned for 1.5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kindred (3-cost, )

- Traits: Spirit, Hunter | mana 0/35
- **Dance of Dread**: Deal [ModifiedDamage] () magic damage and applying Wound for 5 seconds, while leaping away. Wound: Reduce healing received by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kindred (3-cost, APCaster)

- Traits: Spirit, Executioner | mana 0/40
- **Dance of Dread**: Deal [ModifiedDamage] () magic damage and applying Wound for 5 seconds, while leaping away. Wound: Reduce healing received by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Lux (3-cost, )

- Traits: Divine, Dazzler | mana 40/100
- **Light Binding**: Fire a sphere of light toward the farthest enemy. Enemies in the sphere's path take [ModifiedDamage] and are Stunned for 1.5/2/3 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Neeko (3-cost, APCaster)

- Traits: Fabled, Mystic | mana 0/70
- **Blooming Burst**: Throw a seed at a random target that explodes for [ModifiedDamage] () magic damage 3 times. Each explosion affects a bigger area than the last. Fabled Bonus: The third explosion is empowered, dealing 200% bonus damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Nunu & Willump (3-cost, APTank)

- Traits: Elderwood, Brawler | mana 0/85
- **Consume**: Bite the target, dealing [ModifiedDamage] () magic damage. If the target has less Health, deal an additional 50% and convert it to true damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Nunu & Willump (3-cost, APTank)

- Traits: Elderwood, Brawler | mana 0/85
- **Consume**: Bite the target, dealing [ModifiedDamage] () magic damage. If the target has less Health, deal an additional 50% and convert it to true damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Shyvana (3-cost, APFighter)

- Traits: Dragonsoul, Brawler | mana 0/75
- **Dragon's Descent**: Dash away from the current target and transform into a dragon, gaining 40/45/50% bonus Health and 30/60/120% Attack Damage . Attacks gain 3 range that deal [ModifiedDamage] () bonus magic damage over 2 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Sivir (3-cost, ADCarry)

- Traits: Cultist, Sharpshooter | mana 0/70
- **On the Hunt**: Rally allies within 2 hexes, granting them [ModifiedAS] () Attack Speed for 5 seconds. Gain 100/150/200% bonus Attack Damage for the duration. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Veigar (3-cost, APCaster)

- Traits: Elderwood, Mage | mana 0/55
- **Dark Blossom**: Blast the enemy with the lowest Health in range, dealing [ModifiedDamage] () magic damage. If the target dies, permanently gain 1/1/2 Ability Power. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Veigar (3-cost, APCaster)

- Traits: Elderwood, Mage | mana 0/55
- **Dark Blossom**: Blast the enemy with the lowest Health in range, dealing [ModifiedDamage] () magic damage. If the target dies, permanently gain 1/1/2 Ability Power. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Xin Zhao (3-cost, )

- Traits: Warlord, Duelist | mana 30/60
- **Crescent Guard**: Sweep around, dealing [ModifiedDamage] of Attack Damage (total: [TooltipDamage]) to nearby enemies and gaining 50/60/75 Armor and Magic Resist for the rest of combat. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Yuumi (3-cost, APCaster)

- Traits: Spirit, Mystic | mana 0/40
- **Zoomies**: Dash to the lowest-Health ally, healing for [ModifiedHealing] () of missing Health and granting [ModifiedAS] () Attack Speed for 5 seconds. Repeat this on the farthest ally. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Yuumi (3-cost, APCaster)

- Traits: Spirit, Mystic | mana 0/40
- **Zoomies**: Dash to the lowest-Health ally, healing for [ModifiedHealing] () of missing Health and granting [ModifiedAS] () Attack Speed for 5 seconds. Repeat this on the farthest ally. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Aatrox (4-cost, APTank)

- Traits: Cultist, Vanguard | mana 75/150
- **Infernal Chains**: Pull in 2/2/5 of the farthest enemies, then slam the ground in front, dealing [ModifiedDamage] () magic damage in a 1-hex radius. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Aatrox (4-cost, APTank)

- Traits: Cultist, Vanguard | mana 75/150
- **Infernal Chains**: Pull in 2/2/5 of the farthest enemies, then slam the ground in front, dealing [ModifiedDamage] () magic damage in a 1-hex radius. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Ahri (4-cost, )

- Traits: Spirit, Mage | mana 0/75
- **Spirit Bomb**: Channel energy into a spirit orb for 1.5 seconds, then lob it toward a random enemy, dealing [ModifiedDamage] () to all enemies in a large area. If interrupted, release the orb early, reducing the impact area size. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Ashe (4-cost, )

- Traits: Elderwood, Hunter | mana 0/30
- **Hunter's Focus**: Gain [ModifiedAS] Attack Speed for 5 seconds, and attacks fire a flurry of arrows dealing [ModifiedADDamage]. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Aurelion Sol (4-cost, APCasterHighMana)

- Traits: Dragonsoul, Mage | mana 0/80
- **Voice of Lightning**: Breathe lightning in a line toward the farthest enemy, dealing [ModifiedDamage] () magic damage and Overcharging all enemies hit for 10 seconds. If an enemy is already Overcharged, consume the effect and deal 50% bonus magic damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Cassiopeia (4-cost, )

- Traits: Dusk, Mystic | mana 80/150
- **Petrifying Gaze**: Cassiopeia projects a cone of magic energy at her target, dealing [ModifiedDamage] magic damage to all enemies hit, stunning them for 2.5/3/5 seconds, and causing them to take [ModifiedDamageAmp] increased damage while stunned.

### Cho'Gath (4-cost, APTank)

- Traits: Fabled, Brawler | mana 100/180
- **Rupture**: Rupture a 2.5-hex radius area around a random enemy, dealing [ModifiedDamage] () magic damage and Stunning all enemies within for 2/2.5/8 seconds. Fabled Bonus: Knock up ALL enemies on the board. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Jhin (4-cost, )

- Traits: Cultist, Sharpshooter | mana 0/4
- **Whisper**: Passive: Always attack 0.85/0.9/1.2 times per second. Convert each 1% bonus Attack Speed into 0.8 Attack Damage. Passive: Every fourth shot deals [ModifiedPercent] of Attack Damage. (Total: [TooltipDamage]) [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kayle (4-cost, APCarry)

- Traits: Divine, Executioner | mana 0/60
- **Divine Ascension**: Ascend, causing attacks to launch waves that deal [ModifiedDamage] () bonus magic damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Morgana (4-cost, )

- Traits: Enlightened, Dazzler | mana 60/120
- **Hallowed Ground**: Hallow the ground beneath a random target, dealing [ModifiedDamage] to enemies over 5 seconds while they remain inside. Heal for 20/25/30% of the damage dealt. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Morgana (4-cost, APCasterHighMana)

- Traits: Enlightened, Syphoner | mana 60/120
- **Hallowed Ground**: Hallow the ground beneath a random target, dealing [ModifiedDamage] () to enemies within 2.5-hex area over 5 seconds and reducing their Attack Damage by 40/40/50%. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Olaf (4-cost, ADFighter)

- Traits: Dragonsoul, Slayer | mana 0/70
- **Ragnarok**: Gain 150/175/350% Attack Speed, become Unstoppable, and cleave attacks in a cone dealing [ModifiedCleaveDamage] () physical damage for the rest of combat. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Riven (4-cost, )

- Traits: Dusk, Keeper | mana 0/20
- **Sweeping Strikes**: Dash and shield for [ModifiedShield], then slash forward, dealing [ModifiedDamage] to nearby enemies. Every third cast, leap into the air and launch a wave of energy, dealing [ModifiedDamageFinal]. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Sejuani (4-cost, APTank)

- Traits: Fortune, Vanguard | mana 80/160
- **Firecracker**: Throw a giant firecracker at the closest enemy that explodes in a large circle after a 2-second delay. Enemies in the explosion take [ModifiedDamage] () magic damage and are Stunned for 2.5/2.5/8 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Sejuani (4-cost, APTank)

- Traits: Fortune, Vanguard | mana 80/160
- **Firecracker**: Throw a giant firecracker at the closest enemy that explodes in a large circle after a 2-second delay. Enemies in the explosion take [ModifiedDamage] () magic damage and are Stunned for 2.5/2.5/8 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Shen (4-cost, APTank)

- Traits: Ninja, Adept, Mystic | mana 50/100
- **Shadow Dash**: Dash past the target, gaining a [ModifiedShield] () Shield and Taunting all adjacent enemies for 4/4/8 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Shen (4-cost, APTank)

- Traits: Ninja, Adept, Mystic | mana 50/100
- **Shadow Dash**: Dash past the target, gaining a [ModifiedShield] () Shield and Taunting all adjacent enemies for 4/4/8 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Talon (4-cost, ADReaper)

- Traits: Enlightened, Assassin | mana 0/40
- **Truestrike**: Deal [ModifiedDamage] () physical damage to the current target. If this kills them, become Unstoppable and Untargetable while leaping to the lowest Armor enemy. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Talon (4-cost, ADReaper)

- Traits: Enlightened, Assassin | mana 0/40
- **Truestrike**: Deal [ModifiedDamage] () physical damage to the current target. If this kills them, become Unstoppable and Untargetable while leaping to the lowest Armor enemy. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Tryndamere (4-cost, ADFighter)

- Traits: Warlord, Slayer, Duelist | mana 60/100
- **Spinning Slash**: Spin in a line towards the most enemies possible within 4 hexes, dealing [TooltipDamage] () physical damage to enemies in the path and empowering the next 3 attacks to deal [BonusDamage] () bonus physical damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Warwick (4-cost, )

- Traits: Divine, Hunter, Brawler | mana 0/70
- **Primal Hunger**: Gain [ModifiedAS] Attack Speed, 40/40/200% Lifesteal, and increased movement speed for the rest of combat. While in this state, if a takedown is scored, howl to grant all allies who share a Trait an additional 60/75/200% Attack Speed for 3 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Xayah (4-cost, ADCasterMana)

- Traits: Elderwood, Executioner, Keeper | mana 70/120
- **Featherstorm**: Leap into the air and become Untargetable and Invulnerable, throwing a fan of 9 feather daggers in a cone towards the current target that deal [ModifiedPhysicalDamagePerFeather] () physical damage to enemies they pass through. After a brief delay, recall feathers, striking enemies hit for [ModifiedMagicDamagePerFeather] () magic damage per feather. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Azir (5-cost, APCarryHighMana)

- Traits: Warlord, Keeper, Emperor | mana 75/150
- **Emperor's Divide**: Call forth a wall of soldiers that charge through the current target, dealing [ModifiedDamage] () magic damage and slowing all enemies hit for 3 seconds. Enemies nearby are knocked back, while enemies farther away are Stunned for 1.5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Azir (5-cost, APCarryHighMana)

- Traits: Warlord, Keeper, Emperor | mana 75/150
- **Emperor's Divide**: Call forth a wall of soldiers that charge through the current target, dealing [ModifiedDamage] () magic damage and slowing all enemies hit for 3 seconds. Enemies nearby are knocked back, while enemies farther away are Stunned for 1.5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Ezreal (5-cost, )

- Traits: Elderwood, Dazzler | mana 90/150
- **Flux Barrage**: Fire a barrage at the enemy with the highest Attack Speed, healing allies it passes through for [ModifiedHeal], granting them 50% Attack Speed, and dealing [ModifiedDamage] to enemies it passes through, slowing their Attack Speed by 50% for 5 seconds. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Kayn (5-cost, )

- Traits: Tormented, Shade | mana 0/50
- **Reaping Slash**: Dash to the target and slash all adjacent enemies, dealing [ModifiedDamage] and increasing the mana cost of their next spell by 33%. If the slash hits exactly one target, immediately cast again. As Rhaast: Reaping Slash heals for 50% of the damage dealt. As Shadow Assassin: Reaping Slash deals an extra 50% damage for the first 10 seconds of combat. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Lee Sin (5-cost, APTank)

- Traits: Divine, Duelist | mana 0/40
- **God Fist**: Punch the target, dealing [ModifiedDamage] () magic damage, knocking them back to the edge of the battlefield, and Stunning them for 2/3/10 seconds. Enemies struck by the target take half damage and are also Stunned for 1/1/10 second. If the target cannot be pushed back further, knock them out of the battlefield, removing them from combat. If the target survives, dash to them. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Lee Sin (5-cost, APTank)

- Traits: Divine, Duelist | mana 0/40
- **God Fist**: Punch the target, dealing [ModifiedDamage] () magic damage, knocking them back to the edge of the battlefield, and Stunning them for 2/3/10 seconds. Enemies struck by the target take half damage and are also Stunned for 1/1/10 second. If the target cannot be pushed back further, knock them out of the battlefield, removing them from combat. If the target survives, dash to them. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Lillia (5-cost, )

- Traits: Dusk, Mage | mana 80/120
- **Lilting Lullaby**: Cast a magical lullaby, causing 2/3/10 waking enemies with the highest current Health to fall asleep for 3/4/8 seconds. Enemies awaken if they take 500 damage while sleeping, causing an additional [ModifiedDamage]. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Ornn (5-cost, APTank)

- Traits: Elderwood, Vanguard, Blacksmith | mana 80/160
- **Stampede**: Summon an elemental from behind the farthest enemy to travel toward Ornn, Chilling all enemies hit for 3 seconds and dealing [ModifiedDamage] () magic damage. If the elemental runs into Ornn, headbutt it, redirecting it toward another distant enemy, Stunning for 2/2.5/15 seconds and dealing [ModifiedDamage] () magic damage to all enemies hit. Chill: Reduce Attack Speed by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Samira (5-cost, ADCarry)

- Traits: Daredevil, Sharpshooter, Slayer | mana 0/4
- **Inferno Trigger**: Dash away from the current target and become Unstoppable, gaining 100% dodge chance and unleashing [ModifiedShotsPerSecond] () shots per second at 3/3/9 enemies within 2.5 hexes for 2 seconds. Each shot deals [DamagePerShot] physical damage. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Sett (5-cost, APReaper)

- Traits: The Boss, Brawler | mana 100/175
- **Showstopper**: Grab the target and slam them forward, dealing [ModifiedPercentMaxHealthDamagePrimary] () magic damage of their max Health and [ModifiedPercentMaxHealthDamageSecondary] () magic damage of the slammed enemy's max Health to all other enemies in 3-hex area. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Sett (5-cost, APReaper)

- Traits: The Boss, Brawler | mana 100/175
- **Showstopper**: Grab the target and slam them forward, dealing [ModifiedPercentMaxHealthDamagePrimary] () magic damage of their max Health and [ModifiedPercentMaxHealthDamageSecondary] () magic damage of the slammed enemy's max Health to all other enemies in 3-hex area. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Swain (5-cost, APFighter)

- Traits: Dragonsoul, Syphoner | mana 40/100
- **Draconic Ascension**: Transform into a dragon form for 6 seconds, gaining 60/65/100% max Health and dealing [ModifiedDamage] () magic damage each second to enemies within 2.5/2.5/5 hexes. While in dragon form, breathe fire in a cone in front every 2 seconds, dealing [ModifiedConeDamage] () magic damage, burning enemies for 25% of their max Health as true damage over 10 seconds, and Wounding them for the duration. Wound: Reduce healing received by 50% [TFTUnitProperty.:TFT4_Headliner_TRA]

### Yone (5-cost, APReaper)

- Traits: Exile, Adept | mana 0/60
- **Seal Fate**: Strike along a path towards the current target, dealing [ModifiedDamage] () split between all enemies hit. Enemies hit are marked for death and are Sundered and Shredded for 40/40/50% for 8 seconds. This Ability becomes “Unforgotten” until no enemies are marked for death. Unforgotten (10 mana): Dash to the lowest-Health enemy marked for death, dealing [ModifiedUnforgottenDamage] () magic damage, increased based on missing health. Sunder: Reduce Armor Shred: Reduce Magic Resist [TFTUnitProperty.:TFT4_Headliner_TRA]

### Yone (5-cost, APReaper)

- Traits: Exile, Adept | mana 0/60
- **Seal Fate**: Strike along a path towards the current target, dealing [ModifiedDamage] () split between all enemies hit. Enemies hit are marked for death and are Sundered and Shredded for 40/40/50% for 8 seconds. This Ability becomes “Unforgotten” until no enemies are marked for death. Unforgotten (10 mana): Dash to the lowest-Health enemy marked for death, dealing [ModifiedUnforgottenDamage] () magic damage, increased based on missing health. Sunder: Reduce Armor Shred: Reduce Magic Resist [TFTUnitProperty.:TFT4_Headliner_TRA]

### Zilean (5-cost, APCasterHighMana)

- Traits: Cultist, Mystic | mana 100/150
- **Rewind Fate**: Place a protective Time Rune on 2/3/10 allies with the lowest Health (excluding self). If they die, they resurrect after 3/2/0.5 seconds with [HealthAmountCalc] () Health and shed all negative effects. After resurrecting, they gain [ModifiedAS] () Attack Speed for the rest of combat. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Zilean (5-cost, APCasterHighMana)

- Traits: Cultist, Mystic | mana 100/150
- **Rewind Fate**: Place a protective Time Rune on 2/3/10 allies with the lowest Health (excluding self). If they die, they resurrect after 3/2/0.5 seconds with [HealthAmountCalc] () Health and shed all negative effects. After resurrecting, they gain [ModifiedAS] () Attack Speed for the rest of combat. [TFTUnitProperty.:TFT4_Headliner_TRA]

## Monsters, summons, pets and other trait-less units (25)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, )

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Galio (1-cost, )

- Traits: (none) | mana 75/150
- **Provocation Aura**: Gain 50% Durability for 3 seconds, Taunting all enemies in a 2-hex radius. At the end of the taunt, send out a pulse of infernal fire, dealing [ModifiedDamage] () magic damage to enemies around.

### Galio (1-cost, )

- Traits: (none) | mana 75/150
- **Provocation Aura**: Gain 50% Durability for 3 seconds, Taunting all enemies in a 2-hex radius. At the end of the taunt, send out a pulse of infernal fire, dealing [ModifiedDamage] () magic damage to enemies around.

### Golem (1-cost, )

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, )

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, )

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Training Dummy (1-cost, )

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Nexus Minion (2-cost, )

- Traits: (none) | mana 200/250
- **(no ability)**: 

### Nexus Minion (2-cost, )

- Traits: (none) | mana 200/250
- **(no ability)**: 

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Sand Guard (8-cost, )

- Traits: (none)
- **Stand Firm**: Azir's Sand Guards accompany their Emperor on the battlefield. They don't attack, but they get in the enemy's way.

### Sand Guard (8-cost, )

- Traits: (none)
- **Stand Firm**: Azir's Sand Guards accompany their Emperor on the battlefield. They don't attack, but they get in the enemy's way.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Money Sprout (11-cost, )

- Traits: (none) | mana 0/100
- **Prosperity**: Money Sprout equips a different item depending if its on the Front or Back rows. Front row: Unstable Treasure Chest Back row: Gold Leaves (May drop gold when taking damage. Capped at 2 per round. )

### Money Sprout (11-cost, )

- Traits: (none) | mana 0/100
- **Prosperity**: Money Sprout equips a different item depending if its on the Front or Back rows. Front row: Unstable Treasure Chest Back row: Gold Leaves (May drop gold when taking damage. Capped at 2 per round. )


<a id="set-5"></a>

# Set 5: Reckoning

Source: patch 14.24 data (Reckoning revival). 80 champions, 27 traits.

## Traits

### Abomination

When [AllyDeaths] allies have died, the Monstrosity will rise from its grave. The Monstrosity receives bonus Health and Attack Damage based on allied Abomination champions' star levels. The 3 Abomination champions nearest the grave will also grant the Monstrosity a random copy of one of their items when combat starts.

  - (3) AllyDeaths=2, BonusAD=7, BonusHealth=90, FlatAD=70, FlatHealth=900, starLevel=2, 30, 3, 13, 30
  - (4) AllyDeaths=2, BonusAD=9, BonusHealth=150, FlatAD=90, FlatHealth=1500, starLevel=3, 45, 4, 20, 45
  - (5) AllyDeaths=2, BonusAD=11, BonusHealth=200, FlatAD=110, FlatHealth=2000, starLevel=4, 60, 4, 20, 60

### Assassin

Innate: When combat starts, Assassins leap to the enemy backline. Assassins' Abilities can critically strike and they gain bonus Critical Strike Chance and bonus Critical Strike Damage.

  - (2-3) CritAmpPercent=15, CritChanceAmpPercent=20
  - (4-5) CritAmpPercent=25, CritChanceAmpPercent=40
  - (6) CritAmpPercent=40, CritChanceAmpPercent=75

### Brawler

Brawlers gain max Health.

  - (2-3) BonusHealth=400
  - (4-5) BonusHealth=1000
  - (6-2500) BonusHealth=1600

### Cannoneer

Every 5th Cannoneer attack is replaced with a cannon shot that deals a percent of that attack's damage in an explosion around the target as physical damage.

  - (2-3) AttackPercent=225
  - (4-5) AttackPercent=475
  - (6) AttackPercent=1200

### Caretaker

([MinUnits]) Caretakers deploy with a Baby Dragon that can be placed anywhere on the battlefield. The Baby Dragon gains [AttackSpeed]% of its Caretaker's Attack Speed, and restores [ManaRefund] Mana to the Caretaker upon death.

  - (1) AttackSpeed=100, ManaRefund=80

### Cavalier

Innate: Cavaliers charge quickly towards their target whenever they move. Cavaliers gain Durability. At the start of combat and after each charge, this effect is doubled for [Duration] seconds.

  - (2) DamageReductionPercent=20, Duration=4
  - (3) DamageReductionPercent=25, Duration=4
  - (4) DamageReductionPercent=30, Duration=4

### Cruel

Teemo hungers to be alone against exactly 1 enemy left standing. Teemo is purchased with Player Health instead of gold. He can be sold for gold, but not Health. You're welcome.

  - (1) 4.5

### Dawnbringer

Once per combat at [FirstThreshold]% Health, Dawnbringers rapidly heal. When this occurs, all allied Dawnbringers gain [BonusDamage]% bonus damage.

  - (2-3) BonusDamage=10, FirstThreshold=50, PercentOfMaximumHealth=30, 10
  - (4-5) BonusDamage=10, FirstThreshold=50, PercentOfMaximumHealth=55, 15
  - (6-7) BonusDamage=10, FirstThreshold=50, PercentOfMaximumHealth=80, 20
  - (8) BonusDamage=10, FirstThreshold=50, PercentOfMaximumHealth=100, 25

### Draconic


  - (3-4) BonusHealth=None, GoldAmount=2, 1, 50
  - (5) BonusHealth=300, GoldAmount=2, 2, 50

### Forgotten

Forgotten champions have bonus Attack Damage and Ability Power. Each combat win increases the bonus by [VictoryPercent]%, stacking up to [MaxCombats] times.

  - (2-3) InitialADandAP=20, MaxCombats=5, VictoryPercent=10, 1
  - (4-5) InitialADandAP=40, MaxCombats=5, VictoryPercent=10, 2
  - (6-7) InitialADandAP=60, MaxCombats=5, VictoryPercent=10, 3
  - (8) InitialADandAP=120, MaxCombats=5, VictoryPercent=10, 4

### God-King

([MinUnits]) Victorious champions deal [PercentDamageAmp]% bonus damage to enemies who are wearing a Shadow item.

  - (1) PercentDamageAmp=20

### Hellion

Hellions gain Attack Speed. Whenever a Hellion dies, a Doppelhellion (a one star level lower copy) jumps out of the Hellion portal and joins the fight!

  - (2-3) AttackSpeedPercent=15
  - (4-5) AttackSpeedPercent=40
  - (6-7) AttackSpeedPercent=80
  - (8) AttackSpeedPercent=140

### Inanimate

([MinUnits]) Combat start: Gwen summons Harrowing Mist in adjacent hexes surrounding her for [Duration] seconds, granting all allies [DamageReductionPercent]% Durability while they remain within it.

  - (1) DamageReductionPercent=0.33, Duration=8

### Invoker

Your team gains Mana from their attacks.

  - (2-3) ManaPerAttack=2
  - (4) ManaPerAttack=5

### Ironclad

Your team gains Armor.

  - (2) Armor=30
  - (3) Armor=60
  - (4) Armor=120

### Knight

Your team blocks a flat amount of damage from all sources.

  - (2-3) BlockAmount=15
  - (4-5) BlockAmount=30
  - (6) BlockAmount=60

### Legionnaire

Legionnaires gain Attack Speed and Omnivamp.

  - (2-3) ASBoostPercent=25, Omnivamp=0.15
  - (4-5) ASBoostPercent=65, Omnivamp=0.25
  - (6-7) ASBoostPercent=110, Omnivamp=0.3
  - (8) ASBoostPercent=160, Omnivamp=0.4

### Mystic

Your team gains Magic Resist.

  - (2) MagicResist=30
  - (3) MagicResist=60
  - (4) MagicResist=100
  - (5-2500) MagicResist=150

### Nightbringer

Nightbringers gain a Shield for [ShieldDuration] seconds equal to a percent of their maximum Health the first time they drop below [FirstThreshold]%. When this occurs, that Nightbringer gains bonus damage.

  - (2-3) BonusDamage=20, FirstThreshold=50, MaxPercentHealthShield=30, ShieldDuration=8
  - (4-5) BonusDamage=30, FirstThreshold=50, MaxPercentHealthShield=60, ShieldDuration=8
  - (6-7) BonusDamage=40, FirstThreshold=50, MaxPercentHealthShield=90, ShieldDuration=8
  - (8) BonusDamage=70, FirstThreshold=50, MaxPercentHealthShield=150, ShieldDuration=8

### Ranger

Rangers gain an Attack Speed buff that switches on and off every [BuffDuration] seconds. This buff applies to the Ranger's total Attack Speed.

  - (2-3) AttackSpeed=100, BuffDuration=4, DelayBetweenBuffs=4, InitialDelay=4
  - (4-5) AttackSpeed=200, BuffDuration=4, DelayBetweenBuffs=4, InitialDelay=4
  - (6-2500) AttackSpeed=500, BuffDuration=4, DelayBetweenBuffs=4, InitialDelay=4

### Redeemed

Redeemed champions gain Armor, Magic Resist, and Ability Power. When they die, their bonus is split among remaining Redeemed allies.

  - (3-5) AbilityPowerAmount=30, ArmorAmount=20, MagicResist=20
  - (6-7) AbilityPowerAmount=55, ArmorAmount=30, MagicResist=30
  - (8-9) AbilityPowerAmount=65, ArmorAmount=40, MagicResist=40
  - (10) AbilityPowerAmount=150, ArmorAmount=100, MagicResist=100

### Renewer

Renewers heal for a percent of their max Health each second. If Health is full, restore Mana instead.

  - (2-3) PercentHealth=4, PercentMana=4
  - (4-5) PercentHealth=7, PercentMana=7
  - (6) PercentHealth=10, PercentMana=10

### Revenant

Revenants revive after their first death each combat. Once revived, they take and deal [DamageAmp]% increased damage.

  - (2) DamageAmp=25, HealthPercent=10
  - (3) DamageAmp=25, HealthPercent=35
  - (4) DamageAmp=25, HealthPercent=60
  - (5) DamageAmp=25, HealthPercent=90

### Sentinel

Combat start: The Sentinel with the most items (ties broken by highest Attack Speed) gains a Shield that grants stacking Attack Speed. When it is destroyed or when its duration expires, it bounces to the ally with lowest percent Health and renews. The Attack Speed bonus persists between Shield bounces.

  - (3-5) AttackSpeed=0.2, Duration=4, Shield=175
  - (6-7) AttackSpeed=1, Duration=3, Shield=1200
  - (8-9) AttackSpeed=1.8, Duration=2, Shield=1600
  - (10) AttackSpeed=8, Duration=1, Shield=2400

### Skirmisher

Combat start: Skirmishers gain a Shield and bonus Attack Damage each second.

  - (3-5) AttackDamage=0.04, PercentHealthShield=20
  - (6-7) AttackDamage=0.08, PercentHealthShield=50
  - (8-9) AttackDamage=0.12, PercentHealthShield=70
  - (10) AttackDamage=0.3, PercentHealthShield=150

### Spellweaver

Spellweavers gain Ability Power, which increases whenever any champion casts, until the end of combat.

  - (2-3) AP=20, BonusAP=3, StackCap=10
  - (4-5) AP=40, BonusAP=6, StackCap=15
  - (6) AP=80, BonusAP=20, StackCap=20

### Victorious

When Garen kills an enemy, his next attack is empowered to deal [PercentDamage]% of the target's missing Health as bonus magic damage.

  - (1) PercentDamage=0.6

## Champions (46)

### Aatrox (1-cost, ADFighter)

- Traits: Redeemed, Legionnaire | mana 30/80
- **Deathbringer Strike**: Deal [TotalDamage] () physical damage and heal for [ModifiedHeal] ().

### Gragas (1-cost, APTank)

- Traits: Dawnbringer, Brawler | mana 40/70
- **Drunken Rage**: Drink from a cask, gaining 40/45/55 for 4 seconds. The next attack deals [ModifiedDamage] () bonus magic damage .

### Kalista (1-cost, ADCarry)

- Traits: Abomination, Legionnaire | mana 0/120
- **Pierce**: Hurl a spear towards the farthest enemy, dealing [TotalDamage] () physical damage to the first enemy hit. If the enemy dies, the spear continues and deals any excess damage to the next enemy hit.

### Kha'Zix (1-cost, APReaper)

- Traits: Dawnbringer, Assassin | mana 0/60
- **Taste their Fear**: Slash the nearest enemy and deal [ModifiedDamage] () magic damage. If the enemy has no adjacent allies, deal [ModifiedIsolationDamage] () damage instead.

### Kled (1-cost, ADFighter)

- Traits: Hellion, Cavalier
- **Violent Tendencies**: Passive: Every 4th attack deals [TotalDamage] () physical damage. Enter combat on Skaarl and gain a Shield for [ModifiedShieldPercent] () of max Health. When it breaks, dismount and become Untargetable briefly, gaining 70/80/110% Attack Speed.

### Leona (1-cost, ADTank)

- Traits: Redeemed, Knight | mana 40/80
- **Solar Barrier**: Reduces all incoming damage by [ModifiedDamageReduction] () for 4 seconds.

### Poppy (1-cost, APTank)

- Traits: Hellion, Knight | mana 50/70
- **Buckler Toss**: Throw a buckler at the farthest enemy that deals [ModifiedDamage] () magic damage. The buckler returns, granting [ModifiedShield] () Shield.

### Udyr (1-cost, ADFighter)

- Traits: Draconic, Skirmisher | mana 30/40
- **Feral Instinct**: Swap between Turtle and Tiger stance with each cast, gaining the following benefits: Turtle: Gain [ModifiedShieldAmount] () Shield for 4 seconds. Tiger: The next attack strikes 3 times quickly for [ModifiedPercentAD] () physical damage per strike.

### Vayne (1-cost, APCarry)

- Traits: Forgotten, Ranger
- **Silver Bolts**: Passive: Every third attack on the same target deals [ModifiedDamage] () bonus true damage.

### Vladimir (1-cost, APTank)

- Traits: Nightbringer, Renewer | mana 0/80
- **Transfusion**: Deal [ModifiedDamage] () magic damage to the current target and Heal [ModifiedHeal] () Health.

### Ziggs (1-cost, APCaster)

- Traits: Hellion, Spellweaver | mana 0/40
- **Arcane Bomb**: Throw an arcane bomb at the current target, dealing [ModifiedDamage] () magic damage.

### Brand (2-cost, APCaster)

- Traits: Abomination, Spellweaver | mana 0/30
- **Sear**: Launch a ball of fire at the nearest non-seared enemy, searing them for [ModifiedDamage] () magic damage over 10 seconds. Seared enemies are 20/30/50% Shredded. Shred: Reduce Magic Resist

### Hecarim (2-cost, APTank)

- Traits: Forgotten, Cavalier | mana 75/125
- **Spirit of Dread**: Over the next 3 seconds, restore [ModifiedHealing] () Health, and deal [ModifiedDamage] () magic damage to adjacent enemies.

### Kennen (2-cost, APFighter)

- Traits: Hellion, Skirmisher | mana 60/125
- **Flame Rush**: Burst into flames and dash behind the current target, then the farthest enemy. Enemies passed through are dealt [ModifiedDamage] () magic damage and Stunned for 1.5/1.75/2 seconds.

### Nautilus (2-cost, APTank)

- Traits: Ironclad, Knight | mana 50/120
- **Anchor Slam**: Stun the target for 3/4/5 seconds and deal [ModifiedDamage] () magic damage. Enemies adjacent to the target receive 50% of this effect.

### Sejuani (2-cost, APTank)

- Traits: Nightbringer, Cavalier, Brawler | mana 20/80
- **Fury of the North**: Stun the current target for 2/3/4 seconds and deal [ModifiedDamage] () magic damage. Then gain Frost Armor, granting 60/100/200 Armor and Magic Resist for 4 seconds.

### Sett (2-cost, ADFighter)

- Traits: Draconic, Brawler | mana 0/50
- **Haymaker**: After a brief wind up, punch forward in a cone, reducing enemies' armor by [ModifiedArmorShred] () for 10 seconds and dealing [TotalDamage] physical damage.

### Soraka (2-cost, APCasterHighMana)

- Traits: Dawnbringer, Renewer | mana 40/80
- **Equinox**: Calm the area around the enemy with the highest current percent Mana, dealing [ModifiedDamage] () magic damage and Mana Reaving enemies within a 2-hex radius. Mana Reave: Increase max Mana by 35% until the next cast.

### Syndra (2-cost, APCasterHighMana)

- Traits: Redeemed, Invoker | mana 50/90
- **Force of Will**: Grab the nearest enemy and fling them towards the farthest enemy. Upon landing, they are Stunned for 1.5/1.5/2 seconds and all adjacent enemies take [ModifiedDamage] () magic damage.

### Thresh (2-cost, APTank)

- Traits: Forgotten, Knight | mana 80/120
- **Death Sentence**: Hook the farthest enemy with a chain scythe, dealing [ModifiedDamage] () magic damage, Stunning and pulling them closer for 2/2/3 seconds.

### Varus (2-cost, ADCarry)

- Traits: Redeemed, Ranger | mana 0/60
- **Holy Arrows**: Fire a hail of arrows in a two-hex radius around the current target. Enemies take [TooltipDamage] ()] physical damage. Varus and allies within the radius deal [ModifiedDamage] () bonus magic damage on attack for the next 6 seconds.

### Ashe (3-cost, APCarryHighMana)

- Traits: Draconic, Ranger | mana 40/90
- **Enchanted Arrow**: Fire an arrow at the farthest enemy, dealing [ModifiedDamage] () magic damage to the first enemy hit and Stunning them for 1.5/1.5/2 seconds. If the arrow travels at least 5 hexes, Stun duration doubles. Enemies within 1 hex receive 50% of these effects.

### Lee Sin (3-cost, APTank)

- Traits: Nightbringer, Skirmisher | mana 30/80
- **Cripple**: Slam the ground, dealing [ModifiedDamage]() magic damage to nearby enemies and 50% Chilling them for 4 seconds. Chill: Reduce Attack Speed

### Lulu (3-cost, APCasterHighMana)

- Traits: Hellion, Mystic | mana 60/120
- **Whimsy**: Enchant the 3/4/5 closest allies or enemies. Enchanted allies gain [ModifiedAttackSpeedPercent] () bonus Attack Speed for 5 seconds. Enchanted enemies are Stunned and transformed into a docile feline for 1.5 seconds, taking 20% increased damage.

### Lux (3-cost, APCasterHighMana)

- Traits: Redeemed, Mystic | mana 40/80
- **Prismatic Illumination**: Throw a wand to the farthest ally and then recall it. The wand gives [ModifiedShieldAmount] () Shield for each ally it passes for 3 seconds. The next attack deals an additional [ModifiedHitDamage] () magic damage.

### Nidalee (3-cost, ADFighter)

- Traits: Dawnbringer, Skirmisher | mana 0/60
- **Aspect of the Cougar**: Transform into a cougar, gaining 30/50/75 Armor and 30/50/75% Attack Speed, then leap behind the lowest Health enemy. Every fourth attack deals [ModifiedDamage] () bonus magic damage"

### Nocturne (3-cost, ADReaper)

- Traits: Revenant, Assassin
- **Umbra Blades**: Passive: Every 3rd attack, slash all adjacent enemies for [ModifiedTotalDamage] () and Heal for 100% of the damage dealt. If only 1 target is hit, Attack Speed increases by 60% for 3 seconds.

### Nunu & Willump (3-cost, APTank)

- Traits: Abomination, Brawler | mana 30/90
- **Consume**: Bite the current target, dealing [ModifiedDamage] () magic damage. If the target has less Health than Willump before the bite, deal additional 50% damage and becomes true damage.

### Riven (3-cost, ADTank)

- Traits: Dawnbringer, Legionnaire | mana 0/40
- **Blade of the Dawn**: Stun adjacent enemies for 1.5 seconds and deal [ModifiedMagicDamage] () magic damage. For the next 8 seconds, gain 90/100/115% bonus Attack Damage.

### Yasuo (3-cost, APFighter)

- Traits: Nightbringer, Legionnaire | mana 0/40
- **Burning Blade**: Strike the target, dealing [ModifiedDamage] () magic damage. Yasuo then empowers his blade, gaining [ModifiedTrueDamageBonus] () stacking bonus true damage on-hit for the rest of combat.

### Zyra (3-cost, APCasterHighMana)

- Traits: Draconic, Spellweaver | mana 40/100
- **Grasping Roots**: Send a surge of vines towards the farthest enemy, dealing [ModifiedDamage] () magic damage and Stunning all enemies hit for 1.5/1.5/2 seconds.

### Aphelios (4-cost, ADCarry)

- Traits: Nightbringer, Ranger | mana 0/140
- **Dark Vigil**: Launch 5/5/10 attacks, one at the target and the rest at nearby enemies, dealing [TotalDamage] () physical damage to each.

### Diana (4-cost, APReaper)

- Traits: Nightbringer, Assassin | mana 50/140
- **Moonfall**: Draw in all nearby enemies within a 3-hex radius, dealing [ModifiedDamage] () magic damage and Stunning them for 2/2/8 seconds.

### Draven (4-cost, ADCarryCrit)

- Traits: Forgotten, Legionnaire | mana 0/40
- **Spinning Axes**: Passive: Ignore 50% of the current target's Armor. Active: Spin an axe, empowering the next attack to deal [TotalDamage] () bonus physical damage. It will return to Draven's original location after striking the target. If Draven catches it, he will empower the axe again. Draven can spin up to 2 axes at a time.

### Ivern (4-cost, APCasterHighMana)

- Traits: Revenant, Invoker, Renewer | mana 100/180
- **Daisy!**: Summon the sentinel, Daisy, who receives 100% () of Ivern's Ability Power and immediately casts Shockwave upon arrival. If Daisy is already in battle, increase her Ability Power by [ModifiedAbilityPowerBonus] () and command her to cast Shockwave again.

### Jax (4-cost, ADFighter)

- Traits: Ironclad, Skirmisher | mana 0/20
- **Empowered Strike**: Slam the current target for [TotalDamage] () physical damage and gain [ModifiedAS] () stacking Attack Speed for the remainder of combat. Leap to the nearest enemy if no target is in Attack Range.

### Karma (4-cost, APCaster)

- Traits: Dawnbringer, Invoker | mana 0/50
- **Soulflare**: Fire a burst of energy at a random enemy that deals [ModifiedDamage] magic damage to adjacent enemies and reduces Max Mana by 15/15/40 (down to a min of 10). Every 3rd cast is empowered and bursts towards 3 different targets instead of 1.

### Rell (4-cost, ADTank)

- Traits: Redeemed, Ironclad, Cavalier | mana 80/150
- **Attract and Repel**: Leap into the air and create a tether to the farthest ally. Upon landing, the tether breaks and Stuns all nearby enemies in its path for 1.5/2/8 seconds. Nearby allies gain a [ModifiedShieldAmount] () Shield for 4 seconds.

### Vel'Koz (4-cost, APCasterHighMana)

- Traits: Redeemed, Spellweaver | mana 0/90
- **Lifeform Disintegration Ray**: Over 3 seconds, deal [ModifiedDamage] () magic damage targeting the enemy nearest the center of the battlefield. The beam widens over the duration.

### Garen (5-cost, APTank)

- Traits: Victorious, Dawnbringer, Knight | mana 80/160
- **God-Lion's Justice**: Call down a sword that strikes 3 hexes around the current target, dealing [ModifiedPercentMaxHealth] () of each target's max Health in magic damage and Shredding them for 8 seconds. Gain a Shield equal to [ModifiedShieldPercent]() of his maximum Health for 5 seconds. Shred: Reduce Magic Resist by 50%

### Heimerdinger (5-cost, APCasterHighMana)

- Traits: Draconic, Renewer, Caretaker | mana 0/140
- **Upgrade!!!**: Empower the Baby Dragon to launch 3 fireballs across the battlefield that deal [ModifiedTurretMagicDamage] () magic damage, burning enemies for 18% of their max Health as true damage, and 50% Wound them over 6 seconds. If Heimerdinger does not have a Baby Dragon, this Ability will create one instead. Wound: Reduce healing received

### Kayle (5-cost, ADCarry)

- Traits: Redeemed, Legionnaire | mana 0/5
- **Divine Ascent**: Passive: Ascend every 5/5/1 seconds. Each bonus stacks with the previous, until full ascension. 1st: Attacks deal [TotalAttackDamage]() bonus true damage. 2nd: Attacks explode, dealing the same damage to adjacent enemies. 3rd: Every 12th attack grants immunity to damage for 1 second. 4th: Attacks cause swords to rain down around the target, dealing [TotalAoEDamage]() magic damage.

### Teemo (5-cost, APCaster)

- Traits: Cruel, Hellion, Invoker | mana 30/60
- **Teemo's Cruelty**: Scatter 5/6/42 Infernal Souls around a random enemy. When an enemy nears an Infernal Soul (or after 3 seconds), it explodes, dealing [ModifiedDamage] () magic damage to nearby enemies and 50% Chills them for 3 seconds. Chill: Reduce Attack Speed

### Viego (5-cost, APReaper)

- Traits: Forgotten, Skirmisher, Assassin | mana 0/50
- **Sovereign's Domination**: Corrupt the soul of a target for 5 seconds, Stunning them and dealing [ModifiedDamage] () magic damage that increases by 50% each second. Allies prefer not to target the victim during soul corruption. Viego will be interrupted if he is stunned. If the victim dies, they resurrect at full Health to fight for him, but lose 10/5/0% of their max Health per second. Resurrected enemies receive the Trait bonuses of Viego's team instead of their own.

### Volibear (5-cost, APTank)

- Traits: Revenant, Brawler | mana 110/200
- **Doombringer**: Leap towards the current target and slam the ground. Enemies within a 3-hex radius are Stunned for 2/3/20 seconds and take [ModifiedDamage] () magic damage. This damage is doubled against shielded targets.

### Wolf (8-cost, )

- Traits: Eternal | mana 0/20
- **Wolf's Frenzy**: Wolf returns to Lamb, healing them both for [ModifiedHeal] health and then launches himself at the lowest health enemy, dealing [ModifiedDamage] magic damage.

## Monsters, summons, pets and other trait-less units (34)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Daisy (1-cost, )

- Traits: (none)
- **Shockwave**: Smash the ground around the current target, dealing [ModifiedDamage] magic damage, Knocking up all nearby enemies for 0.5/0.5/6 seconds.

### Elder Dragon (1-cost, )

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, )

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, )

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Lissandra (1-cost, )

- Traits: (none) | mana 0/50
- **1000 Daggers**: Lissandra hurls a dagger toward the enemy with the highest Attack Damage, dealing [ModifiedDamage] magic damage to the first target it hits. After hitting its initial target or at its final destination, the dagger explodes dealing [ModifiedSecondaryDamage] magic damage to nearby targets. All enemies hit have their Attack Damage reduced by 40% for 8 seconds.

### Murk Wolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, )

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Target Dummy (1-cost, )

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### The Monstrosity (1-cost, )

- Traits: (none) | mana 0/100
- **Enrage**: Become Unstoppable for 4 seconds, gaining [ModifiedAttackSpeed] () Attack Speed, causing Basic Attacks heal for 60% of the damage dealt.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Warwick (1-cost, )

- Traits: (none) | mana 60/100
- **Infinite Duress**: Warwick leaps to the enemy with the lowest percent Health, stunning them, dealing [ModifiedDamage] magic damage, and healing himself for 80% of the damage over 2 seconds. If this Ability kills the target, Warwick will instantly cast it again.

### LeBlanc (2-cost, )

- Traits: (none) | mana 20/60
- **Ethereal Chain**: LeBlanc launches a chain in a line at her target's position, shackling them. After a short delay, they take [ModifiedDamage] magic damage. Multicaster: The chain continues on to additional targets, dealing reduced damage.

### Trundle (2-cost, )

- Traits: (none) | mana 30/80
- **Subjugate**: Trundle drains his target's strength, reducing their Health, Armor, Magic Resist, and Attack Damage by 35% and granting himself [ModifiedStatStealMultiplier] of the drained stats for 8 seconds.

### Viktor (2-cost, )

- Traits: (none) | mana 30/70
- **Siphon Power**: Viktor blasts his target, dealing [ModifiedDamage] magic damage. He also grants a [ModifiedShield] Health shield to the 3 allies nearest his target for 5 seconds.

### Draconic Egg (3-cost, )

- Traits: (none) | mana 0/100
- **Hatching**: After a few rounds this egg will hatch into Draconic champions or other loot! In Hyper Roll, eggs hatch a lot faster!

### Katarina (3-cost, )

- Traits: (none) | mana 0/20
- **Sinister Blade**: Katarina throws a dagger at the farthest enemy, dealing [ModifiedDamage] magic damage. When the dagger lands near the target Katarina teleports to it and immediately launches 3 additional daggers at the nearest enemies that each deal [ModifiedSecondaryDamage] magic damage. Enemies hit have their incoming healing reduced by 50% for 6 seconds.

### Morgana (3-cost, )

- Traits: (none) | mana 80/120
- **Soul Shackles**: Morgana fires chains to nearby enemies, dealing [ModifiedDamage] magic damage. After 3 seconds, all chained enemies are dealt an additional [ModifiedEndDamage] magic damage and stunned for 2.5/3/4 seconds.

### Pantheon (3-cost, )

- Traits: (none) | mana 30/60
- **Aegis Assault**: Pantheon braces his shield, reducing all incoming damage by [ModifiedDamageReductionPercent] for 4 seconds, and dealing 400/450/650% of his Attack Damage (total: [TotalDamage]) in the area in front of him over the duration. Enemies hit have their incoming healing reduced by 50% for 8 seconds.

### Mordekaiser (4-cost, )

- Traits: (none) | mana 0/80
- **Infernal Rise**: Mordekaiser shields himself for [ModifiedShieldPercent]% of his maximum Health for 5 seconds, empowering his mace to deal [ModifiedDamageOnHit] bonus magic damage and gain 1 hex of range for the same duration.

### Ryze (4-cost, )

- Traits: (none) | mana 20/50
- **Rune Prison**: Ryze imprisons the nearest enemy, dealing [ModifiedDamage] magic damage and stunning them for 1.5/2/4 seconds. His next cast spreads from his target, applying the same damage and half the stun to all enemies in a large area around that target.

### Taric (4-cost, )

- Traits: (none) | mana 120/200
- **Nature's Touch**: All allies in a large area around Taric are healed for [ModifiedHealth] Health and gain 50/75/500 Armor for 5 seconds.

### Darius (5-cost, )

- Traits: (none) | mana 40/100
- **God-Wolf's Carnage**: Darius transforms into a God-Wolf, becoming unstoppable as he lunges towards a nearby enemy. 3/4/10 God-Wolves join him in his hunt, each targeting their own prey. When God-Wolves bite enemies, they deal 200/225/2000% of Darius' Attack Damage (total: [ModifiedDamage]) as physical damage, and reduce the enemy's Armor by 50% for 8 seconds. When Darius bites his prey, his next 2 attacks restore [ModifiedHealing] of his maximum Health.

### Kindred (5-cost, )

- Traits: (none) | mana 100/200
- **Lamb's Respite**: Lamb creates a zone around herself for 2.5/3/10 seconds that prevents allies within from falling below [ModifiedMinHealth] Health or dying. While Lamb's Respite is active, Wolf is invulnerable.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Baby Dragon (8-cost, )

- Traits: (none)
- **Turret Plating**: Baby Dragons are immune to crowd control effects.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!


<a id="set-6"></a>

# Set 6: Gizmos & Gadgets

Source: patch 12.10 data. 67 champions, 28 traits.

## Traits

### Arcanist

Arcanists increase the Ability Power of your team.

  - (2-3) 20, null
  - (4-5) 20, 30
  - (6-7) 50, 50
  - (8) 150, null

### Assassin

Innate: When combat starts, Assassins leap to the enemy backline. Assassins' Abilities can critically strike and they gain bonus Critical Strike Chance and bonus Critical Strike Damage.

  - (2-3) 20, 10
  - (4-5) 40, 30
  - (6) 75, 60

### Bodyguard

Bodyguards have increased Armor. Shortly after combat begins, Bodyguards gain a shield and taunt adjacent enemies, forcing them to attack the Bodyguard.

  - (2-3) BonusArmor=80, ShieldAmount=200
  - (4-5) BonusArmor=160, ShieldAmount=400
  - (6-7) BonusArmor=250, ShieldAmount=800
  - (8-9999) BonusArmor=450, ShieldAmount=1800

### Bruiser

Your team gains bonus maximum Health. Bruisers gain double the bonus.

  - (2-3) BonusHealth=125
  - (4-5) BonusHealth=225
  - (6-7) BonusHealth=450
  - (8) BonusHealth=800

### Challenger

Challengers get bonus Attack Speed. Upon scoring a takedown, Challengers dash to a new target and double this bonus for [BurstDuration] seconds.

  - (2-3) BonusAS=25, 2.5
  - (4-5) BonusAS=50, 2.5
  - (6-7) BonusAS=85, 2.5
  - (8-9999) BonusAS=160, 2.5

### Chemtech

After dropping below [HealthThreshold]% Health, Chemtech champions become chem-powered, gaining Attack Speed, [DamageReduction]% damage reduction, and regenerating a percentage of their maximum Health each second for [Duration] seconds.

  - (3-4) AttackSpeed=15, DamageReduction=15, Duration=8, HealthRegen=4, HealthThreshold=75
  - (5-6) AttackSpeed=40, DamageReduction=15, Duration=8, HealthRegen=8, HealthThreshold=75
  - (7-8) AttackSpeed=90, DamageReduction=15, Duration=8, HealthRegen=15, HealthThreshold=75
  - (9) AttackSpeed=200, DamageReduction=15, Duration=8, HealthRegen=25, HealthThreshold=75

### Clockwork

Your team has increased Attack Speed, with an additional increase per augment in the Hexcore.

  - (2-3) 0.05, 0.1
  - (4-5) 0.1, 0.35
  - (6) 0.15, 0.7

### Colossus

Innate: Colossi are bigger and more powerful. They gain [BonusHealthTooltip] bonus Health and immunity to crowd control effects. However, each Colossus requires 2 team slots.

  - (2) DamageReduction=0.25, 2, 800
  - (3) DamageReduction=0.25, 3, 1888, 800

### Debonair

Debonair champions gain bonus Health and Ability Power, and you have a higher chance to see Debonair VIPs in your Shop. If there is a Debonair VIP in play, they activate their unique bonus. Sell the old VIP for a chance to see a new one in your Shop.

  - (3-4) Health=200, 20
  - (5-6) Health=450, 45
  - (7) Health=800, 80

### Enchanter

Your team has bonus Magic Resist. Enchanters gain bonus healing and shielding.

  - (2) 25, 0.25
  - (3) 50, 0.5
  - (4) 80, 0.8
  - (5-9999) 125, 1.25

### Enforcer


  - (3-4) HPPercent=0.4, 4, 1
  - (5) AttackSpeed=1, HPPercent=0.4, 4, 2

### Glutton

An ally from the bench can be fed to Tahm Kench once per planning phase, permanently granting him either Ability Power, Health, Armor, or Magic Resist. To feed, hold an ally from the bench over Tahm Kench until his mouth opens, then release.

  - (1) (no numbers)

### Hextech

At the start of combat and every [Frequency] seconds afterwards, the Hexcore sends out a pulse that charges up allied Hextech champions with a shield for [ShieldDuration] seconds (does not stack). While the shield is active, attacks deal bonus magic damage on hit. Additionally, the shield and bonus magic damage are [BonusPercentPerAugment]% stronger for each augment in the Hexcore.

  - (2-3) MagicDamage=15, ShieldAmount=100, ShieldDuration=4, 6, 20
  - (4-5) MagicDamage=30, ShieldAmount=175, ShieldDuration=4, 6, 20
  - (6-7) MagicDamage=55, ShieldAmount=300, ShieldDuration=4, 6, 20
  - (8) MagicDamage=90, ShieldAmount=425, ShieldDuration=4, 6, 20

### Innovator

Innovators build a mechanical companion to join the battle. The companion receives bonus Health and Attack Damage based on allied Innovators' star levels.

  - (3-4) 1, 0.25
  - (5-6) 2, 0.25
  - (7) 3, 0.25

### Mastermind

At the start of combat, the Mastermind grants the 2 allies directly in front of him [ManaGrant] Mana (this effect does not stack).

  - (1) 30

### Mercenary

Gain a treasure chest that opens when you win combat against a player. At the start of each planning phase vs a player, dice rolls add loot to the chest. The longer you've gone without opening the chest, the luckier the dice.

  - (3-4) (no numbers)
  - (5-6) (no numbers)
  - (7) (no numbers)

### Mutant

Mutants gain a different Trait bonus from game to game. Cybernetic Enhancement grants bonus stats to Mutants with at least 1 item. Voracious Appetite strengthens Mutants on each ally's death. Voidborne executes targets below a Health threshold and deals true damage. Hyper-Adrenal Glands give a chance for Mutants to deal additional attacks. Synaptic Web reduces the Mana cost of Mutants' Abilities. Bio-Leeching grants Omnivamp (healing for a percentage of damage dealt) to your team. Metamorphosis grants stacking bonuses to Mutants at regular intervals.

  - (3-4) 8, 18, 30, 0.02, 20, 1, 50, 2, 450, 18, null, 20, 2, null, 30, 40, 175, null
  - (5-6) 12, 25, 65, 0.03, 20, 2, 50, 2, 800, 25, null, 40, 4, 40, 60, 75, 225, null
  - (7) 8, 35, 100, 0.04, 20, 3, 50, 2, 1250, 35, 40, 40, 7, 80, 90, 100, 175, 50

### Rival

This trait is only active when you have exactly 1 unique Rival unit, as Rivals refuse to work together. Vi's mana cost is reduced by [ViManaReduction]. Jinx gains [JinxEmpoweredAS]% Attack Speed for [JinxASDuration] seconds after scoring a takedown.

  - (1) 3, 4, 0.4, 20

### Scholar

Your team gains Mana every [TickRate] seconds.

  - (2-3) 2, 5
  - (4-5) 2, 15
  - (6) 2, 30

### Scrap

At the start of combat, components held by Scrap champions turn into full items for the rest of combat. Also, your team gains a shield for each component equipped by your team, including those that are part of a full item.

  - (2-3) 1, 20
  - (4-5) 3, 30
  - (6) 12, 50

### Sniper

Innate: Snipers gain [HexRangeIncrease] hex Attack Range. Snipers deal bonus damage for each hex between themselves and their target.

  - (2-3) 1, 8
  - (4-5) 1, 16
  - (6) 1, 30

### Socialite

Socialites reveal a spotlight on the battlefield. The unit standing in the spotlight at the start of combat gains unique bonuses.

  - (1) DamagePercent=0.18, null, 1, null, null
  - (2) DamagePercent=0.18, null, 2, null, 4
  - (3-4) DamagePercent=0.18, null, 3, 0.33, 4
  - (5) DamagePercent=0.36, null, 4, 0.66, 8

### Striker

Strikers gain bonus Attack Damage.

  - (2-3) 30
  - (4-5) 70
  - (6) 120

### Syndicate

Certain allies are cloaked in shadows, gaining [Armor] Armor, [MagicResist] Magic Resist and [PercentOmnivamp]% Omnivamp (healing for a percentage of all damage dealt.)

  - (3-4) Armor=50, MagicResist=50, 1, 20
  - (5-6) Armor=50, MagicResist=50, 2, 20
  - (7) Armor=50, MagicResist=50, 3, 0.5, 20

### Transformer

Jayce adopts melee form when placed in the front 2 rows, and ranged form in the back 2 rows.

  - (1) (no numbers)

### Twinshot

Twinshots gain bonus Attack Damage and have a chance to fire twice whenever they attack or cast an Ability.

  - (2) BonusAD=10, 40
  - (3) BonusAD=25, 55
  - (4) BonusAD=50, 75
  - (5) BonusAD=80, 100

### Yordle


  - (3-5) null
  - (6) 40

### Yordle-Lord

(1) Benefits from the Yordle trait. Veigar is summoned from a Yordle Portal when every Yordle is 3-star.

  - (1) (no numbers)

## Champions (60)

### Brand (1-cost, )

- Traits: Debonair, Arcanist | mana 0/20
- **Sear**: Brand launches a fireball at the nearest enemy, lighting them ablaze for 4 seconds and dealing [ModifiedDamage] magic damage. If the enemy is already on fire, they take [ModifiedBonusDamage] bonus magic damage which stuns them for 1/1.5/2 seconds. (total: [TotalDamage]) VIP Bonus: Brand fires a 2nd fireball at a different nearby target (prioritizes ablaze enemies) dealing 45% reduced damage.

### Caitlyn (1-cost, )

- Traits: Enforcer, Sniper | mana 0/100
- **Ace in the Hole**: Caitlyn takes aim at the farthest enemy, firing a deadly bullet towards them that deals [ModifiedDamage] magic damage to the first enemy it hits.

### Camille (1-cost, )

- Traits: Clockwork, Challenger | mana 0/60
- **Defensive Sweep**: Camille gains a shield blocking [ModifiedShield] damage over 4 seconds, then sweeps her leg, dealing [ModifiedDamage] magic damage to enemies in a cone. While this shield holds, Camille's attacks restore [ModifiedHealing] Health.

### Darius (1-cost, )

- Traits: Syndicate, Bodyguard | mana 70/120
- **Decimate**: Darius swings his axe in a circle, dealing [ModifiedDamage] magic damage, then heals himself [ModifiedHeal] Health for each enemy hit.

### Ezreal (1-cost, )

- Traits: Scrap, Innovator | mana 0/40
- **Mystic Shot**: Ezreal fires a missile at his target, dealing [TotalDamage] physical damage (150% of his Attack Damage + 25/50/100). If the missile hits, he grants himself [ModifiedAS] bonus stacking Attack Speed, up to [MaxAS] at 5 stacks.

### Illaoi (1-cost, )

- Traits: Mercenary, Bruiser | mana 40/80
- **Harsh Lesson**: Illaoi slams her target, linking her soul to it for 5 seconds and dealing [ModifiedDamage] magic damage. While linked, Illaoi is healed for 30/35/45% of the damage taken by her target.

### Jarvan IV (1-cost, )

- Traits: Hextech, Striker | mana 50/100
- **Ageless Standard**: Jarvan calls down his standard to a nearby location, granting all nearby allies [ModifiedAS] Attack Speed for 5 seconds.

### Kassadin (1-cost, )

- Traits: Mutant, Scholar | mana 60/100
- **Null Sphere**: Kassadin fires an orb of void energy at his target, dealing [ModifiedDamage] magic damage. Enemies hit by the orb suffer a 50% increased Mana cost for their next Ability cast. Kassadin also grants himself a [ModifiedShieldAmount] shield and takes 25% less damage for 5 seconds.

### Nocturne (1-cost, )

- Traits: Hextech, Assassin | mana 40/80
- **Unspeakable Horror**: Nocturne terrifies his target, stunning them with fear for 2/2.5/3 seconds and dealing [ModifiedDamage] magic damage over the duration.

### Poppy (1-cost, )

- Traits: Yordle, Bodyguard | mana 50/100
- **Buckler Toss**: Poppy throws her buckler at the farthest enemy, dealing 180/220/280% of her Armor as magic damage. (total: [TotalDamage]) The buckler then bounces back, granting Poppy a shield that blocks [ModifiedShield] damage.

### Singed (1-cost, )

- Traits: Chemtech, Innovator | mana 100/150
- **Fling**: Singed flings a nearby enemy towards the largest cluster of enemies, stunning his target for 1.5/2/3 seconds when they land. All adjacent enemies take [ModifiedDamage] magic damage and are briefly stunned.

### Twitch (1-cost, )

- Traits: Chemtech, Assassin | mana 0/45
- **Piercing Bolt**: Twitch fires a powerful bolt towards his target that pierces through enemies, deals [TotalDamage] physical damage (125% of his Attack Damage + [ModifiedDamage]), and reduces healing by 50% for 5 seconds.

### Ziggs (1-cost, )

- Traits: Scrap, Yordle, Arcanist | mana 0/60
- **Mini Inferno Bomb**: Ziggs hurls a bomb at his target. After a moderate delay, the bomb lands dealing [ModifiedDamage] magic damage to the enemy in the epicenter, and half to adjacent enemies.

### Ashe (2-cost, )

- Traits: Syndicate, Sniper | mana 30/60
- **Volley**: Ashe fires a volley of 8 arrows centered on her target, dealing [TotalDamage] physical damage per arrow (110% of her Attack Damage ) and slowing their Attack Speed by 20% for [ModifiedDuration] seconds.

### Blitzcrank (2-cost, )

- Traits: Scrap, Bodyguard | mana 175/175
- **Rocket Grab**: Blitzcrank pulls the farthest enemy, dealing [ModifiedDamage] magic damage and stunning them for 1.5 seconds. His next attack knocks them up for 1 second. Allies within range will prefer attacking Blitzcrank's target.

### Corki (2-cost, )

- Traits: Yordle, Twinshot | mana 25/50
- **Bombardment**: Corki fires a missile at his target that explodes on impact, dealing [ModifiedDamage] magic damage to enemies within 1 hex.

### Lulu (2-cost, )

- Traits: Yordle, Enchanter | mana 60/125
- **Wild Growth**: Lulu enlarges 1/2/3 low Health allies, granting them [ModifiedBonusHealth] bonus Health and knocking up enemies near them. If the ally is already enlarged, they are healed instead.

### Quinn (2-cost, )

- Traits: Mercenary, Challenger | mana 70/140
- **Disarming Assault**: Quinn sends Valor out at the target with the highest Attack Speed, dealing [ModifiedDamage] magic damage to the target and nearby enemies, disarming them for 1.5/2/2.5 seconds.

### Rek'Sai (2-cost, )

- Traits: Mutant, Striker, Bruiser | mana 50/70
- **Furious Bite**: Rek'Sai bites her target, dealing [TotalDamage] physical damage (125% of her Attack Damage + 100/150/200) and healing herself for [ModifiedHeal] health. If she has already bit her target, she heals for [ModifiedHealBonus] health instead.

### Sejuani (2-cost, )

- Traits: Hextech, Enforcer, Bruiser | mana 20/80
- **Fury of the North**: Sejuani signals Bristle to charge, dealing [ModifiedDamage] magic damage and stunning her target for 1.5/1.5/2 seconds. She then gains Frost Armor, granting her 50/75/125 Armor and Magic Resist for 4 seconds.

### Swain (2-cost, )

- Traits: Hextech, Arcanist | mana 40/85
- **Death's Hand**: Swain unleashes piercing bolts of eldritch power in a cone towards his target, dealing [ModifiedDamage] magic damage to enemies within and healing Swain for [ModifiedHealing] for each enemy hit.

### Syndra (2-cost, )

- Traits: Debonair, Scholar | mana 50/100
- **Force of Will**: Syndra flings the nearest enemy towards the farthest enemy, dealing [ModifiedDamage] magic damage to all enemies within 1 hex upon impact and stunning the thrown target for 2/2.5/3 seconds. VIP Bonus: Force of Will's impact area is 1 hex larger and knocks up affected enemies for 1.5 seconds.

### Talon (2-cost, )

- Traits: Debonair, Assassin
- **Blade's End**: Passive: Talon's 1st attack on an enemy causes them to bleed for [ModifiedDamage] magic damage over 7 seconds. Every 3rd attack on a target applies an additional bleed. VIP Bonus: Talon's bleed now deals true damage and lasts 100% longer.

### Warwick (2-cost, )

- Traits: Chemtech, Challenger
- **Eternal Hunger**: Passive: Warwick's Attacks deal an additional [ModifiedPercentHealth] of his target's current Health as bonus magic damage, and heal him for [ModifiedHealAmount] Health.

### Zilean (2-cost, )

- Traits: Clockwork, Innovator | mana 30/70
- **Time Bomb**: Zilean places a bomb on the closest enemy, stunning them for 1.5 seconds. When the stun ends or the target dies, the bomb explodes, dealing [ModifiedDamage] magic damage to adjacent enemies and reducing their Attack Speed by 30/40/50% for 4 seconds.

### Zyra (2-cost, )

- Traits: Syndicate, Scholar | mana 60/120
- **Grasping Spines**: Zyra summons vines in the row with the most enemies, dealing [ModifiedDamage] magic damage and stunning them for 1.5/2/2.5 seconds.

### Cho'Gath (3-cost, )

- Traits: Mutant, Bruiser, Colossus | mana 100/165
- **Feast**: Cho'Gath devours the lowest Health enemy within range, dealing [ModifiedDamage] magic damage. If this kills the target, Cho'Gath gains a stack of Feast (increased to two in Hyper Roll), up to 30/60/999. Each stack of Feast permanently grants 2% bonus Health and size.

### Ekko (3-cost, )

- Traits: Scrap, Innovator, Assassin | mana 60/120
- **Parallel Convergence**: Ekko invokes an afterimage that bats a device towards the largest group of units. Upon landing, it deals [ModifiedDamage] magic damage to enemies within and applies 20/20/30% reduced Attack Speed for 4 seconds. Allies inside gain [ModifiedASBuff] bonus Attack Speed for 4 seconds.

### Gangplank (3-cost, )

- Traits: Mercenary, Twinshot | mana 0/50
- **Parrrley**: Gangplank attacks his target with his gun, dealing [TotalDamage] physical damage (170% of his Attack Damage + [ModifiedDamage]). If this attack kills a champion, Gangplank plunders 1 gold.

### Gnar (3-cost, )

- Traits: Yordle, Socialite, Striker | mana 0/80
- **GNAR!**: Gnar transforms into Mega Gnar for the rest of combat. Mega Gnar throws a boulder at the farthest enemy within 3 hexes, dealing [TotalDamage] physical damage (175% of his Attack Damage + 150/200/300) to all enemies it passes through. While in Mega Gnar form, Gnar is melee, gains [ModifiedTransformHealth] Health, and his mana costs are reduced by 20.

### Leona (3-cost, )

- Traits: Debonair, Bodyguard | mana 75/125
- **Solar Eclipse**: Leona calls down a beacon of light upon herself, granting herself a [ModifiedShielding] Health shield for 5/5/8 seconds. Leona and allies within 2 hexes gain 30/50/80 Armor and Magic Resistance for the same duration. VIP Bonus: Every second, Leona heals herself for 1.25% max health for each unit targeting her.

### Lucian (3-cost, )

- Traits: Hextech, Twinshot | mana 0/40
- **Relentless Pursuit**: Lucian dashes away from his current target and fires 2/2/3 shots at nearby enemies that each deal [ModifiedDamage] magic damage.

### Malzahar (3-cost, )

- Traits: Mutant, Arcanist | mana 50/80
- **Malefic Visions**: Malzahar infects the mind of the closest unafflicted target, dealing [ModifiedDamage] magic damage over 8 seconds and reducing the Magic Resist of enemies by 40%. If an afflicted target dies, Malefic Visions spreads to the nearest 1/1/2 unafflicted targets with the remaining duration.

### Miss Fortune (3-cost, )

- Traits: Mercenary, Sniper | mana 40/80
- **Make it Rain**: Miss Fortune rains 4 waves of bullets down around her target, dealing [ModifiedMagicDamage] total magic damage to enemies in the area and reducing their incoming healing by 50% for 6 seconds.

### Morgana (3-cost, )

- Traits: Syndicate, Enchanter | mana 70/130
- **Soul Shackles**: Morgana grants herself a [ModifiedShieldAmount] shield for 3 seconds and shackles herself to all enemies in a 2 hex radius. She then deals [ModifiedDamagePerSecond] magic damage per second to them, as long as the shield holds. If the shield expires without being broken, all targets are stunned for 1.5/2/2.5 seconds. If the shield is broken, Morgana instead refunds herself 30 mana.

### Senna (3-cost, )

- Traits: Socialite, Enchanter | mana 30/70
- **Piercing Darkness**: Senna fires a beam in the direction of her target, dealing [TotalDamage] physical damage (150% of her Attack Damage + 100/140/200). For each enemy hit, she heals the lowest health ally for [ModifiedPercentHealing] of the damage dealt.

### Tryndamere (3-cost, )

- Traits: Chemtech, Challenger | mana 40/100
- **Spinning Slash**: Tryndamere spins in a line towards the most enemies, dealing [TooltipDamage] physical damage (100% of his Attack Damage + [ModifiedDamage]) to enemies in his path and empowering his next 3 attacks to deal 25% more damage.

### Vex (3-cost, )

- Traits: Yordle, Arcanist | mana 50/90
- **Personal Space**: Vex shields herself against [ModifiedShield] damage over 4 seconds. When the shield expires, it deals [ModifiedDamage] magic damage to all enemies within 2 hexes, and an additional [ModifiedBonusDamage] magic damage if it wasn't destroyed. If it was destroyed, Personal Space becomes 15% stronger this combat. This effect can stack.

### Zac (3-cost, )

- Traits: Chemtech, Bruiser | mana 50/90
- **Yoink!**: Zac stretches his arms up to 3 hexes to pull the 2 most distant enemies towards him, dealing [ModifiedDamage] magic damage. During this time, Zac gains 60% damage reduction.

### Ahri (4-cost, )

- Traits: Syndicate, Arcanist | mana 30/50
- **Orbs of Deception**: Ahri fires an orb in a 5 hex line, dealing [ModifiedDamage] magic damage to all enemies it passes through on the way out and the way back. Enemies already hit by an orb take 60% damage for each subsequent orb. Ahri fires 1/1/2 additional orb(s) for each time she has cast this combat.

### Alistar (4-cost, )

- Traits: Hextech, Colossus | mana 80/160
- **Pulverize**: Alistar charges at his target, knocking them back for a short distance. Then he slams the ground, dealing [ModifiedDamage] magic damage and stunning all nearby enemies for 1.5/2/8 seconds.

### Braum (4-cost, )

- Traits: Syndicate, Bodyguard | mana 120/200
- **Vault Breaker**: Braum slams his vault door into the ground, creating a fissure towards his target. Enemies within 2 hexes of Braum, and those struck by the fissure, are stunned for 2/2.75/8 seconds and take [ModifiedDamage] magic damage.

### Draven (4-cost, )

- Traits: Debonair, Challenger | mana 0/40
- **Spinning Axes**: Passive: Draven ignores 25% of his target's Armor. Draven starts spinning an axe, empowering his next attack to deal [TotalDamage] bonus physical damage (170/180/400% of his Attack Damage + [ModifiedDamage]). It will return to his original location after striking the target. If Draven catches it, he will empower the axe again. Draven can spin up to 2 axes at a time. VIP Bonus: Draven gains infinite attack range and ignores an additional 25% his target's Armor.

### Irelia (4-cost, )

- Traits: Scrap, Striker | mana 0/40
- **Bladesurge**: Irelia dashes to her target, striking them for [TooltipDamage] physical damage (190% of her Attack Damage + [ModifiedDamage]). If this kills the target, she Bladesurges again at the lowest Health enemy.

### Jhin (4-cost, )

- Traits: Clockwork, Sniper | mana 0/60
- **Curtain Call**: Jhin transforms his weapon into a powerful sniper rifle for his next 4 shots. Each shot deals [TotalDamage] physical damage (175/200/300% of his Attack Damage ), reduced by 33% for each target it pierces through. The 4th shot is guaranteed to critically strike, and deals [ModifiedBonusDamage] more damage based on his target's missing Health. Passive: Jhin always attacks 0.9/0.9/1.4 times per second. He converts each 1% of bonus Attack Speed into 0.8 Attack Damage.

### Kha'Zix (4-cost, )

- Traits: Mutant, Assassin | mana 0/40
- **Void Assault**: Kha'Zix leaps towards the lowest Health enemy, striking them for [TotalDamage] physical damage (180/185/260% of his Attack Damage + [ModifiedDamage]) and increasing their maximum Mana by 50% until they cast.

### Orianna (4-cost, )

- Traits: Clockwork, Enchanter | mana 40/100
- **Command: Shockwave**: Orianna sends her ball towards the largest group of champions, then commands it to release a shockwave. Allies within 2 hexes gain [ModifiedShieldAmount] shield for 4 seconds, while enemies within the area are briefly knocked up and dealt [ModifiedDamage] magic damage. Enemies adjacent to the ball are drawn in and stunned for 1/1/4 seconds.

### Renata Glasc (4-cost, )

- Traits: Chemtech, Scholar | mana 0/60
- **Toxic Wave**: Renata unleashes a toxic poison wave towards the largest group of nearby enemies within 6 hexes, poisoning all enemies caught in its path for 15 seconds. Poisoned enemies suffer 20% reduced Attack Speed and take [ModifiedDamagePerSecond] magic damage per second. The poison's damage can stack, but is removed if Renata dies.

### Seraphine (4-cost, )

- Traits: Socialite, Innovator | mana 70/130
- **Encore**: Seraphine projects her song towards the largest group of enemy units, dealing [ModifiedDamage] magic damage to enemies. Allies it passes through are healed for [ModifiedHeal] Health and gain 30/45/100% bonus Attack Speed for 4 seconds.

### Sivir (4-cost, )

- Traits: Hextech, Striker | mana 0/60
- **Ricochet**: For the next 5 seconds, Sivir grants herself [ModifiedAttackSpeed] bonus Attack Speed and her attacks bounce up to 4/5/9 times, dealing [DamageCalc] physical damage (30% of her Attack Damage ) to enemies hit.

### Vi (4-cost, )

- Traits: Enforcer, Rival, Bruiser | mana 0/40
- **Piltover Pulverizer**: Vi shields herself for [ModifiedShield] damage and deals [ModifiedDamage] magic damage to her target and enemies behind it. On her 2nd cast, she also dashes through her target. On her 3rd cast, she instead throws her target into the air and slams them back to the ground, dealing [ModifiedDamageFinal] in a circle around her target.

### Galio (5-cost, )

- Traits: Socialite, Bodyguard, Colossus | mana 200/300
- **Colossal Entrance**: Galio becomes invulnerable and leaps into the sky before crashing down on the largest group of enemies. Enemies within a large radius take [TotalDamage] magic damage ([ModifiedDamage] + 10% of Galio's maximum Health ) and are knocked up for 1.5/1.5/10 second(s). Passive: Galio's Critical Strikes slam the ground, dealing [ModifiedBonusDamage] bonus magic damage to enemies around his target.

### Jayce (5-cost, )

- Traits: Enforcer, Innovator, Transformer | mana 0/60
- **Mercury Cannon/Mercury Hammer**: Melee form: Jayce slams his hammer down onto nearby enemies, dealing damage and reducing their Armor and Magic Resist. Ranged form: Jayce deploys an acceleration gate, granting bonus Attack Speed to allies in the same row, then fires an orb of electricity at the largest enemy group.

### Jinx (5-cost, )

- Traits: Scrap, Rival, Twinshot | mana 0/99
- **Super Mega Death Rocket**: Jinx rides her rocket into the sky, then comes crashing down near the center-most enemy, dealing [ModifiedDamage] magic damage to enemies around the epicenter, and 50% to all other enemies in a large area. The epicenter burns every unit except Jinx for 5 seconds, dealing 2/3/4% of the target's maximum Health as true damage, and reducing healing by 50% for the duration. She then swaps to her rocket launcher and targets random units for the rest of combat. Her attacks now explode for [TooltipDamage] physical damage (2.1/2.2/8.88% of her Attack Damage ) in a small area around her target.

### Kai'Sa (5-cost, )

- Traits: Mutant, Challenger | mana 80/160
- **Icathian Monsoon**: Kai'sa dashes away from all enemies, then fires a volley of 12/18/100 missiles spread evenly among all enemies that deal [ModifiedDamage] magic damage each. For each time Kai'Sa has attacked this combat, she'll fire an additional missile.

### Silco (5-cost, )

- Traits: Mastermind, Scholar | mana 0/40
- **Unstable Concoction**: Silco injects an unstable concoction into 1/1/5 of his lowest Health allies, granting them 50% maximum Health, 50/125/666% bonus Attack Speed, and immunity to crowd control for 6/7/10 seconds. When the concoction wears off, unstable units die by explosion, dealing [ModifiedDamage] magic damage to nearby enemies.

### Tahm Kench (5-cost, )

- Traits: Mercenary, Bruiser, Glutton | mana 30/60
- **Devour**: Tahm Kench devours his target, storing them in his belly and dealing [ModifiedDamage] magic damage over 3 seconds. During this time, they are invulnerable to other sources of damage and Tahm Kench takes 30% less damage. If they die while inside, Tahm Kench either spits out a random component they were holding, or the cost of the unit in gold. Otherwise, he spits them towards the farthest enemy, briefly stunning targets on impact. If a target is immune to crowd control, Tahm Kench will instead deal [ReducedModifiedDamage] magic damage.

### Viktor (5-cost, )

- Traits: Chemtech, Arcanist | mana 0/145
- **Chaos Rays**: Passive: Viktor's attacks melt his target's defenses, reducing their Armor by 70% for 6 seconds. Viktor summons multiple singularities that fire death rays, which cut across the battlefield in a line. To enemies caught in its path, death rays deal [ModifiedDamage] magic damage, destroy 40/50/200% of any remaining shields.

### Zeri (5-cost, )

- Traits: Debonair, Sniper | mana 0/60
- **Lightning Crash**: Zeri charges herself up with electricity for 6 seconds. While electrified, she aims at the farthest enemy, her attacks pierce, and she dashes after every shot. Passive: Zeri's Basic Attacks fire 5/5/30 bullets, each dealing [TooltipDamage] physical damage (18% of her Attack Damage ) and [ModifiedBonusOnHit] bonus magic damage. VIP Bonus: Zeri's electrified duration lasts until the end of combat.

### Veigar (7-cost, )

- Traits: Yordle-Lord | mana 0/75
- **(no ability)**: 

## Monsters, summons, pets and other trait-less units (7)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Treasure Dragon Egg (5-cost, )

- Traits: (none) | mana 0/100
- **Hatching**: An Egg from the Treasure Dragon. The longer it takes to hatch, the better the loot! But how did it get here? 'There are always stories from the days of old; whispers about gifts from the blessed ancient. Before technology, and before Hextech, there were dragons.'

### Mechanical Bear (7-cost, )

- Traits: (none) | mana 0/100
- **Power Surge**: The Bear surges with power, gaining 75% bonus Attack Damage and granting 15 Attack Damage and Ability Power to your team for [TotalBuffDuration] seconds.

### Mechanical Dragon (7-cost, )

- Traits: (none) | mana 200/300
- **Electrifying Roar**: Passive: The Dragon is immune to crowd control effects. Every 3rd attack is charged with lightning, dealing [ModifiedLightningDamage] bonus magic damage to 3 enemies. The Dragon lands and lets out a bellowing roar, causing all enemies within 5 hexes to cower in fear for 2 seconds. The roar also energizes your team, making all attacks critically strike and granting your team 25% Critical Strike Damage for 8 seconds.

### The Golden Egg (7-cost, )

- Traits: (none) | mana 0/100
- **Hatching**: An Egg from the Treasure Dragon. The longer it takes to hatch, the better the loot! But how did it get here? 'There are always stories from the days of old; whispers about gifts from the blessed ancient. Before technology, and before Hextech, there were dragons.'

### Annie (10-cost, )

- Traits: (none) | mana 0/65
- **EMP Wave**: Annie blasts a cone of fire dealing [ModifiedDamage] magic damage to enemies in front of her and reducing their Attack Damage by 40/40/80% for 6 seconds.

### Mechanical Scarab (10-cost, )

- Traits: (none) | mana 40/80
- **Reflective Barrier**: The Scarab creates a barrier on itself and the ally targeted by the most enemies, then taunts all enemies who have the Scarab within its attack range, forcing them to attack the Scarab. The barriers last for 4 seconds, reduce incoming damage by 20% and enemies attacking the barrier take [ModifiedDamageAmount] magic damage. Enemies may only take damage from a barrier once per second.

### Mercenary Chest (10-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!


<a id="set-7"></a>

# Set 7: Dragonlands

Source: current client data. 90 champions, 29 traits.

## Traits

### Assassin

Innate: Leap to the enemy backline when combat starts. Assassins gain Critical Strike Chance and Critical Strike Damage. Their Ability can critically strike.

  - (2-3) CritAmpPercent=5, CritChanceAmpPercent=20
  - (4-5) CritAmpPercent=15, CritChanceAmpPercent=50
  - (6) CritAmpPercent=65, CritChanceAmpPercent=75

### Astral

After each player combat, gain an Astral Orb. The total star level of your Astral champions increases the orb quality. Astral champions gain Ability Power.

  - (3-4) AbilityPower=10, AttackDamage=0.05, 1, 4
  - (5-7) AbilityPower=50, AttackDamage=0.1, 1, 4
  - (8) AbilityPower=100, AttackDamage=0.25, 1, 4

### Bard

Allies that survive player combat have a [DropChancePercent]% chance to create a Doot. Bard always creates a Doot when dancing. Each Doot you collect increases your Shop odds for higher-tier champions by 1%.

  - (1) DropChancePercent=2

### Bruiser

Your team gains Health. Bruisers gain double.

  - (2-3) BonusHealth=180
  - (4-5) BonusHealth=375
  - (6-7) BonusHealth=700
  - (8) BonusHealth=1500

### Cannoneer

Every 5th attack fires a cannon shot that explodes for physical damage around the target. Damage increases with Base Attack Damage.

  - (2-3) AttackPercent=1.75
  - (4-5) AttackPercent=3.5
  - (6) AttackPercent=5.75

### Cavalier

Innate: Charge quickly towards their target whenever they move. Cavaliers gain Armor and Magic Resist. At the start of combat and after each charge, gain [ChargeIncreasePct]% of this bonus for [Duration] seconds.

  - (2) Armor=35, ChargeIncreasePct=200, Duration=4, MagicResist=35
  - (3) Armor=50, ChargeIncreasePct=200, Duration=4, MagicResist=50
  - (4) Armor=65, ChargeIncreasePct=200, Duration=4, MagicResist=65
  - (5) Armor=85, ChargeIncreasePct=200, Duration=4, MagicResist=85
  - (6) Armor=110, ChargeIncreasePct=200, Duration=4, MagicResist=110

### Darkflight

Summon a sacrificial altar. Combat start: the unit placed on the altar is sacrificed, granting a copy of one of its items and Health to each Darkflight champion. Emblems and non-craftable items are corrupted when they're copied, instead becoming Darkflight Essence.

  - (2-3) BonusHealth=750
  - (4-5) BonusHealth=1050
  - (6-7) BonusHealth=1650
  - (8) BonusHealth=2400

### Dragon

Innate: Require 2 team slots. Dragons provide +3 to the marked trait and gain additional bonuses.

  - (1) AttackSpeed=None, BonusHealth=500, Defenses=None, Healing=None, PercentBonusDamage=None, TeamSizeIncrease=None, None
  - (2) AttackSpeed=None, BonusHealth=500, Defenses=20, Healing=None, PercentBonusDamage=None, TeamSizeIncrease=None, None
  - (3) AttackSpeed=None, BonusHealth=500, Defenses=20, Healing=None, PercentBonusDamage=0.1, TeamSizeIncrease=None, None
  - (4) AttackSpeed=None, BonusHealth=500, Defenses=20, Healing=None, PercentBonusDamage=0.1, TeamSizeIncrease=1, None
  - (5) AttackSpeed=0.1, BonusHealth=500, Defenses=20, Healing=40, PercentBonusDamage=0.1, TeamSizeIncrease=1, None
  - (6) AttackSpeed=0.1, BonusHealth=500, Defenses=20, Healing=40, PercentBonusDamage=0.1, TeamSizeIncrease=1, 30

### Dragonmancer

Use the Dragonmancer Blessing item to choose a Hero. The Hero gains massively increased Health and Ability Power, which increases by [StatIncreasePercent]% per star level of your Dragonmancers.

  - (2-3) BonusAP=30, BonusHealth=300, StatIncreasePercent=20
  - (4-5) BonusAP=45, BonusHealth=600, StatIncreasePercent=20
  - (6-8) BonusAP=60, BonusHealth=850, StatIncreasePercent=20
  - (9-999) BonusAP=145, BonusHealth=1600, StatIncreasePercent=20

### Evoker

Evokers gain Mana whenever an ally or enemy casts an Ability.

  - (2) ManaAmount=3
  - (3) ManaAmount=6
  - (4-999) ManaAmount=8

### Guardian

Once per combat at [HealthThreshold]% Health, Guardians Shield themselves and their closest ally for a percent of their maximum Health. Shields stack!

  - (2-3) Duration=60, HealthThreshold=50, ShieldPercentAmount=35
  - (4-5) Duration=60, HealthThreshold=50, ShieldPercentAmount=50
  - (6-7) Duration=60, HealthThreshold=50, ShieldPercentAmount=100
  - (8-999) Duration=60, HealthThreshold=50, ShieldPercentAmount=150

### Guild

Grant a unique bonus to your team; Guild members gain double the amount. Increases for each Guild member in play! Sejuani: +[TFTUnitProperty.trait:TFT7_Trait_Guild_SejuaniBonus] HealthSejuani: Gain Health Twitch: +[TFTUnitProperty.trait:TFT7_Trait_Guild_TwitchBonus]% Attack SpeedTwitch: Gain Attack Speed Zippy: +[TFTUnitProperty.trait:TFT7_Trait_Guild_DragonBonus] Armor and Magic ResistZippy: Gain Armor and Magic Resist Jayce: +[TFTUnitProperty.trait:TFT7_Trait_Guild_JayceBonus]% Attack Damage and Ability PowerJayce: Gain Attack Damage and Ability Power Bard: +[TFTUnitProperty.trait:TFT7_Trait_Guild_BardBonus] Mana per attackBard: Mana per attack Emblem: +[TFTUnitProperty.trait:TFT7_Trait_Guild_EmblemBonus]% OmnivampEmblem: Omnivamp

  - (1) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=100, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2
  - (2) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=110, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2
  - (3) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=120, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2
  - (4) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=130, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2
  - (5) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=140, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2
  - (6) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=150, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2
  - (7) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=160, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2
  - (8) BardBaseBonus=2, DragonBaseBonus=8, EmblemBaseBonus=0.03, JayceBaseBonus=5, Multiplier=175, SejuaniBaseBonus=130, TwitchBaseBonus=0.11, 2

### Jade

Summon movable Jade Statues. Combat Start: Allies who start combat adjacent to a statue restore maximum Health every [Interval] seconds and gain Attack Speed. When a statue is destroyed, it deals [PercentHealth]% of its Health as magic damage to nearby enemies.

  - (3-4) AttackSpeed=0.1, HealthRegen=2, Interval=2, PercentHealth=33, 1, 0.25
  - (5-6) AttackSpeed=0.22, HealthRegen=3, Interval=2, PercentHealth=33, 2, 0.25
  - (7-10) AttackSpeed=0.55, HealthRegen=5, Interval=2, PercentHealth=33, 3, 0.25
  - (11) AttackSpeed=0.88, HealthRegen=8, Interval=2, PercentHealth=33, 4, 0.25

### Lagoon

Summon a Seastone which grants loot as Lagoon champions cast Abilities. Lagoon champions gain Ability Power and Attack Speed. Total casts: [TFTUnitProperty.trait:TFT7_LagoonCount] Gold granted: [TFTUnitProperty.trait:TFT7_LagoonRewardCountGold] Rewards granted: [TFTUnitProperty.trait:TFT7_LagoonRewardCount][TFTUnitProperty.trait:TFT_Modes_TRA1]

  - (3-5) AbilityPower=10, AttackSpeed=0.1
  - (6-8) AbilityPower=35, AttackSpeed=0.35
  - (9-11) AbilityPower=60, AttackSpeed=0.6
  - (12-999) AbilityPower=200, AttackSpeed=2

### Mage

Mages cast their Abilities twice and have modified total Ability Power.

  - (3-4) APMultiplier=0.9
  - (5-6) APMultiplier=1.2
  - (7-8) APMultiplier=1.5
  - (9) APMultiplier=1.9

### Mirage

Mirage champions gain a different Trait bonus from game to game.

  - (2-3) DawnbringerPercentHealing=40, DawnbringerPercentHealthThreshold=50, DuelistMaxStacks=10, DuelistStackingAttackSpeed=6, Durability=None, ElectricChance=20, ElectricDamagePercent=9, ExecutionerCritDamage=20, ExecutionerHealthThreshold=50, SpellswordAPPerAttack=5, WarlordBonusAP=18, WarlordBonusHealth=150, WarlordPercentIncreasePerStack=10, 2, 200
  - (4-5) DawnbringerPercentHealing=65, DawnbringerPercentHealthThreshold=50, DuelistMaxStacks=10, DuelistStackingAttackSpeed=12, Durability=None, ElectricChance=33, ElectricDamagePercent=9, ExecutionerCritDamage=33, ExecutionerHealthThreshold=65, SpellswordAPPerAttack=8, WarlordBonusAP=33, WarlordBonusHealth=300, WarlordPercentIncreasePerStack=10, 4, 200
  - (6-7) DawnbringerPercentHealing=65, DawnbringerPercentHealthThreshold=50, DuelistMaxStacks=10, DuelistStackingAttackSpeed=20, Durability=None, ElectricChance=50, ElectricDamagePercent=9, ExecutionerCritDamage=45, ExecutionerHealthThreshold=80, SpellswordAPPerAttack=12, WarlordBonusAP=50, WarlordBonusHealth=500, WarlordPercentIncreasePerStack=10, 6, 200
  - (8) DawnbringerPercentHealing=110, DawnbringerPercentHealthThreshold=50, DuelistMaxStacks=10, DuelistStackingAttackSpeed=30, Durability=12, ElectricChance=100, ElectricDamagePercent=9, ExecutionerCritDamage=60, ExecutionerHealthThreshold=100, SpellswordAPPerAttack=20, WarlordBonusAP=75, WarlordBonusHealth=750, WarlordPercentIncreasePerStack=10, 8, 200

### Monolith

([MinUnits]) Terra empowers (3/3/All) hexes on the battlefield. Combat start: units standing in the hex at the start of combat gain [DamageReductionPercent]% Durability.

  - (3) DamageReductionPercent=18

### Mystic

Your team gains Magic Resist.

  - (2) MagicResist=30
  - (3) MagicResist=60
  - (4) MagicResist=130
  - (5) MagicResist=280

### Prodigy

([MinUnits]) Nomsy gains a random trait each game. She summons a former Trainer to aid her in battle, who also gains this trait. Trainers' Ability Power is always equal to Nomsy's Ability Power. When Nomsy dies, Trainers gain [AttackSpeed]% Attack Speed.

  - (3) AttackSpeed=1

### Ragewing

Innate: Convert Mana to Rage; attacks generate 15 Rage. After casting an Ability, enrage for 4 seconds: 25% Attack Speed but can't gain Rage. Gain Attack Speed and Omnivamp when enraged: Ragewings don't gain base mana per attack nor mana from taking damage.

  - (2-3) AttackSpeedPercent=50, EnrageDuration=4, Omnivamp=10, 25
  - (4-5) AttackSpeedPercent=100, EnrageDuration=4, Omnivamp=25, 25
  - (6-7) AttackSpeedPercent=150, EnrageDuration=4, Omnivamp=40, 25
  - (8-999) AttackSpeedPercent=325, EnrageDuration=4, Omnivamp=80, 25

### Scalescorn

Scalescorns deal a percent of their damage as bonus magic damage. They also take [PercentDamageReduction]% reduced damage from enemies with more than [HealthThreshold] Health.

  - (2-3) HealthThreshold=1900, PercentBonusDamage=15, PercentDamageReduction=15
  - (4-5) HealthThreshold=1900, PercentBonusDamage=35, PercentDamageReduction=15
  - (6) HealthThreshold=1900, PercentBonusDamage=80, PercentDamageReduction=15

### Shapeshifter

Gain Health after transforming from an Ability.

  - (2-3) HPPercent=55
  - (4-999) HPPercent=140

### Shimmerscale

Gain unique Shimmerscale items.

  - (3-4) NumItems=1, 1.1
  - (5-6) NumItems=2, 1.1
  - (7-8) NumItems=4, 1.1
  - (9) NumItems=5, 1.1
  - (10) NumItems=5, 1.1

### Spell Thief

([MinUnits]) Zoe nabs a new Ability after each cast and at the start of every round.

  - (1) (no numbers)

### Starcaller

([MinUnits]) The first Starcaller to cast their Ability during player combat heals you for ([Tier1Heal]/[Tier2Heal]/[Tier3Heal]) Tactician damage, depending on their star level. Excess healing disintegrates an enemy champion.

  - (1) Tier1Heal=1, Tier1HealDU=1, Tier1HealHR=1, Tier2Heal=3, Tier2HealDU=2, Tier2HealHR=2, Tier3Heal=75, Tier3HealDU=30, Tier3HealHR=20, 1

### Swiftshot

Innate: Gain +[HexRangeIncrease] Attack Range. Swiftshots gain Attack Speed for each hex between themselves and their target.

  - (2) BonusAS=10, HexRangeIncrease=2
  - (3) BonusAS=20, HexRangeIncrease=2
  - (4) BonusAS=30, HexRangeIncrease=2
  - (5-999) BonusAS=45, HexRangeIncrease=2

### Tempest

After [DelayTime] seconds, lightning strikes the battlefield, dealing true damage to enemies, Stunning them for [StunDuration] second, and granting Tempest champions Damage Amp for the rest of combat.

  - (2-3) BuffDuration=60, DamageAmp=0.1, DelayTime=8, StunDuration=1, TrueDamageHPRatio=0.1, 1
  - (4-5) BuffDuration=60, DamageAmp=0.3, DelayTime=8, StunDuration=1, TrueDamageHPRatio=0.2, 2
  - (6-7) BuffDuration=60, DamageAmp=0.6, DelayTime=8, StunDuration=1, TrueDamageHPRatio=0.3, 3
  - (8-9) BuffDuration=60, DamageAmp=0.9, DelayTime=8, StunDuration=1, TrueDamageHPRatio=0.4, 3
  - (10) BuffDuration=60, DamageAmp=0.65, DelayTime=8, StunDuration=1, TrueDamageHPRatio=0.35, 4

### Warrior

Warrior attacks have a [ProcChance]% chance to increase the damage of their next attack.

  - (2-3) PercentDamage=0.75, ProcChance=50
  - (4-5) PercentDamage=1.5, ProcChance=50
  - (6) PercentDamage=2.75, ProcChance=50

### Whispers

Damage shrinks enemies, applying 20% Shred, Sunder for [Duration] seconds. When Whispers damage a shrunken enemy, Whispers gain stacking Attack Damage and Ability Power: {{TFT_Keyword_Shred}} {{TFT_Keyword_Sunder}}

  - (2-3) ADandAP=1, Duration=6
  - (4-5) ADandAP=3, Duration=6
  - (6) ADandAP=7, Duration=6

## Champions (62)

### Ezreal (1-cost, APCaster)

- Traits: Tempest, Swiftshot | mana 0/40
- **Mystic Shot**: Fire an energy bolt towards the target, dealing [ModifiedDamage] () magic damage to the first enemy hit. Gain [ModifiedAttackSpeed] () Attack Speed, stacking up to 5 times.

### Karma (1-cost, APCaster)

- Traits: Jade, Dragonmancer | mana 0/40
- **Inner Flame**: Fire a burst of energy towards the current target, dealing [ModifiedDamage] () magic damage in a 1-hex radius centered on the first enemy hit.

### Leona (1-cost, APTank)

- Traits: Mirage, Guardian | mana 10/50
- **Solar Barrier**: Create a surrounding barrier that reduces all incoming damage by [ModifiedDamageReduction] () for 4 seconds.

### Malphite (1-cost, APTank)

- Traits: Lagoon, Bruiser | mana 50/50
- **Coral Shield**: Gain [ModifiedShield] () Shield for 5 seconds.

### Nasus (1-cost, APTank)

- Traits: Shimmerscale, Guardian | mana 0/50
- **Fury of the Dawn**: Gain 225/350/650 max Health for 6 seconds, dealing [ModifiedDamage] () magic damage to adjacent enemies every second.

### Nidalee (1-cost, ADFighter)

- Traits: Astral, Shapeshifter | mana 0/30
- **Primal Surge**: Transform into Cougar Form for the rest of combat, reducing Attack Range to 1 and gaining bonus movement speed and [ModifiedAS] () Attack Speed. While in Cougar Form, every 3rd attack swipes the target for [TotalDamage] () physical damage.

### Sejuani (1-cost, APTank)

- Traits: Guild, Cavalier | mana 30/80
- **Warrior's Wrath**: Deal [SpellDamage] () magic damage to enemies in a cone. Then repeat the damage on the target and the enemy directly behind, Stunning them for 1.5/1.75/2 seconds.

### Senna (1-cost, ADCaster)

- Traits: Ragewing, Cannoneer | mana 15/75
- **Last Embrace**: Launch mist toward the farthest enemy, dealing [TotalDamage] () physical damage in a 1-hex radius centered on the first enemy hit, plus [ModifiedBonusDamage] () bonus magic damage to the target.

### Sett (1-cost, ADFighter)

- Traits: Ragewing, Dragonmancer | mana 30/60
- **Knuckle Down**: For 4 seconds, gain [ModifiedResistAmount] () Armor and Magic Resist. Every other punch deals [TotalBonusDamage] () bonus physical damage.

### Skarner (1-cost, APTank)

- Traits: Astral, Bruiser | mana 10/50
- **Crystalline Exoskeleton**: Gain [ModifiedShield] () Shield for 8 seconds and 160/180/200% Attack Speed while it holds.

### Taliyah (1-cost, APCaster)

- Traits: Lagoon, Mage | mana 0/50
- **Flowing Volley**: Throw 3 stones towards the current target, each dealing [ModifiedMagicDamage] () magic damage to the first enemy hit.

### Vladimir (1-cost, APTank)

- Traits: Astral, Mage | mana 0/70
- **Transfusion**: Heal [ModifiedHeal] () and deal [ModifiedDamage] () magic damage to the target.

### Wukong (1-cost, ADFighter)

- Traits: Jade, Warrior | mana 0/30
- **Crushing Blow**: Deal [TotalDamage] () physical damage to the target and Stun them for 1/1.25/1.5 second(s).

### Aphelios (2-cost, ADCaster)

- Traits: Darkflight, Cannoneer | mana 0/90
- **Binding Eclipse<br>**: Fires [TotalBoltNumber] () bolts in a cone, each dealing [TotalDamage] () physical damage. Enemies only take damage from one bolt. Then, Stun all enemies hit for 1 second(s).

### Braum (2-cost, APTank)

- Traits: Scalescorn, Guardian | mana 40/70
- **Unbreakable**: Raise shield against the largest group of enemies for 4 seconds, gaining [ModifiedDurability] () Durability against damage from that direction.

### Gnar (2-cost, ADFighter)

- Traits: Jade, Shapeshifter | mana 0/40
- **Mega Gnar**: Jump to the current target and Stun nearby enemies for 1 second. Gain 95/100/100% Attack Damage and 40/60/80 Armor, but reduce Attack Range to 1 hex.

### Jax (2-cost, APTank)

- Traits: Jade, Shimmerscale, Bruiser | mana 0/70
- **Counter Strike**: Dodge all incoming attacks for 2 seconds. Then, deal [ModifiedDamage] () magic damage to all nearby enemies and Stun them for 1/1.25/1.5 second(s).

### Kai'Sa (2-cost, APCaster)

- Traits: Lagoon, Dragonmancer | mana 0/45
- **Tidal Burst**: Fire 4 missiles split between the current target and up to 2 other targets within attack range, each dealing [ModifiedDamage] () magic damage. Increase by 2 missiles each cast, up to 12.

### Lillia (2-cost, APFighter)

- Traits: Scalescorn, Cavalier, Mage | mana 0/30
- **Watch Out!**: Deal [ModifiedMagicDamage] () magic damage spread amongst the target and all enemies within a 1-hex radius, plus [ModifiedCenterBonusMagicDamage] () bonus magic damage to the target.

### Lux (2-cost, APCaster)

- Traits: Astral, Mage | mana 0/50
- **Cosmic Flare**: Fire a star at the farthest enemy, dealing [ModifiedDamage] () magic damage to the first enemy hit and [ModifiedSecondaryDamage] () magic damage to subsequent enemies.

### Qiyana (2-cost, APFighter)

- Traits: Tempest, Assassin | mana 15/45
- **Elemental Blade**: Dash to the best position to strike enemies with a blade, dealing [ModifiedDamage] () magic damage in a short line through the target and Disarming them for 1.5/1.75/2 seconds. {{TFT_Keyword_Disarm}}

### Rell (2-cost, APTank)

- Traits: Darkflight, Cavalier | mana 0/70
- **Iron Bond**: Tether to the lowest percent Health ally, dealing [ModifiedDamage] () magic damage to all enemies between. Gain 35/40/50% of current Armor and Magic Resist, and grant the same amount to the bound ally for 4 seconds.

### Twitch (2-cost, ADCaster)

- Traits: Guild, Swiftshot | mana 40/80
- **Blast Potion**: Deal [TotalDamage] () physical damage to enemies in a 1-hex radius centered on the target and 20% Sunder them for 5 seconds. {{TFT_Keyword_Sunder}}

### Yone (2-cost, ADFighter)

- Traits: Mirage, Warrior
- **Way of the Hunter**: Passive: Attacks alternate between dealing [ModifiedBonusDamage] () bonus magic damage and dealing [BonusPhysicalDamage] () bonus physical damage.

### Zac (2-cost, APTank)

- Traits: Lagoon, Guardian | mana 20/75
- **Unstable Current**: Restore [ModifiedHeal] () Health and explode outwards towards enemies within 2 hexes, dealing [ModifiedPercentHealthDamage] of their max Health as magic damage.

### Zyra (2-cost, APCaster)

- Traits: Whispers, Evoker | mana 0/70
- **Grasping Spines**: Summon vines in the row with the most enemies, dealing [ModifiedDamage] () magic damage and Stunning them for 1 second(s).

### Diana (3-cost, APFighter)

- Traits: Scalescorn, Assassin | mana 15/45
- **Pale Cascade**: Gain a [ModifiedShield] () Shield over 6 seconds and summon 5/6/7 orbs to orbit. These orbs explode for [ModifiedDamage] () magic damage when they hit an enemy, refreshing the Shield when the final orb explodes. [TFTUnitProperty.:TFT4_Headliner_TRA]

### Heimerdinger (3-cost, APCaster)

- Traits: Mage | mana 50/100
- **Egg Toss**: Heimerdinger lobs an egg at the highest percent Health enemy, dealing [ModifiedDamage] magic damage and stunning them for 0.5 seconds.

### Lee Sin (3-cost, APFighter)

- Traits: Tempest, Dragonmancer | mana 15/45
- **Dragon's Rage**: Kick the target, Stunning them for 1.25 seconds and knocking them back. The target and all enemies hit take [ModifiedMagicDamage] () magic damage. 20% Shred damaged enemies for 5 seconds. {{TFT_Keyword_Shred}}

### Lulu (3-cost, APCaster)

- Traits: Evoker | mana 0/60
- **Whimsy**: Enchant the 3 closest allies or enemies. Enchanted allies gain [ModifiedAttackSpeedPercent] () bonus Attack Speed for 5 seconds. Enchanted enemies are Stunned and transformed into a docile feline for 1 seconds, taking 20% increased damage.

### Nomsy (3-cost, APCaster)

- Traits: Prodigy, Dragon | mana 0/60
- **Fireballs!**: Lob a fireball towards a random enemy that explodes on impact, dealing [ModifiedDamage] () magic damage to enemies within 1 hex, and bouncing additional fireballs that deal [PercentDamage]% of the damage.

### Nunu & Willump (3-cost, APTank)

- Traits: Mirage, Cavalier | mana 30/90
- **Consume**: Bite the target, dealing [ModifiedDamage] () magic damage. If the target has less Health than Nunu & Willump prior to the bite, deal an additional 50% damage.

### Olaf (3-cost, ADFighter)

- Traits: Scalescorn, Bruiser, Warrior | mana 20/40
- **Recklessness**: Passive: Permanently gain 4% Attack Damage after each death. (Current Bonus: [TFTUnitProperty.unit:TFT7_OlafPassiveBonusAD]% Attack Damage.) Active: Deal [TotalDamage] () physical damage and gain [ModifiedAttackSpeedRatio] () Attack Speed for 5 seconds. If below 50% Health, the Attack Speed is doubled.

### Rakan (3-cost, APTank)

- Traits: Ragewing, Guardian, Mystic | mana 15/75
- **Disarming Diversion**: Dash to the furthest enemy within 2 hexes, Stunning all units passed through for 1.5 seconds, and Taunting the target. Then, gain a [ModifiedShield] () Shield for 4 seconds. Attacks grant an additional [ModifiedBonusShield] () Shield.

### Rengar (3-cost, ADFighter)

- Traits: Darkflight, Assassin | mana 30/60
- **Unseen Predator**: Leap to the lowest Armor enemy, deal [TotalDamage] () physical damage, then gain 200% Attack Speed for 2 attacks.

### Seraphine (3-cost, APCaster)

- Traits: Lagoon, Evoker, Mystic | mana 40/80
- **Serenade of the Seas**: Grant [ModifiedShield] () Shield and [ModifiedDamage] () bonus magic damage on-attack to allies within 2 hexes for 4 seconds.

### Sylas (3-cost, APTank)

- Traits: Whispers, Mage, Bruiser | mana 20/60
- **Petricite Burst**: Deal [ModifiedDamage] () magic damage to nearby enemies and gain [ModifiedShield] () Shield for 6 seconds. If Shield is already active, hit a larger area and 30% Mana Reave enemies struck. {{TFT_Keyword_ManaReave}}

### Tristana (3-cost, ADCaster)

- Traits: Cannoneer | mana 50/100
- **Explosive Charge**: Tristana fires up to 1 explosive charges, prioritizing the closest targets without a charge. Charges detonate after 1 seconds, dealing [TotalDamage] physical damage ([PercentAD]% Attack Damage ) to nearby enemies and an additional [ModifiedDamage] magic damage to the target.

### Varus (3-cost, HCaster)

- Traits: Astral, Swiftshot | mana 0/110
- **Chain of Constellations**: Shoot a tendril towards the current target, dealing [ModifiedPhysicalDamage] () physical damage to the first enemy hit and Stunning them for 1 seconds. It then spreads to 3 nearby enemies, dealing [ModifiedMagicDamage] () magic damage and Stunning them for the same duration.

### Volibear (3-cost, APFighter)

- Traits: Shimmerscale, Dragonmancer | mana 0/40
- **Relentless Storm**: Gain [ModifiedBonusHealth] max Health. For the rest of combat, every 3rd attack deals [ModifiedDamagePerTarget] () bonus magic damage to the target, and [ModifiedDamagePerTarget] () magic damage to up to 4 additional enemies.

### Zeri (3-cost, ADCaster)

- Traits: Lagoon, Cannoneer | mana 0/60
- **Watershock Laser**: Fire a water pulse at the closest enemy within 3 hexes, or at the centermost enemy, Stunning them for 0.75 seconds. When the pulse explodes it deals [TargetDamage] () physical damage and creates a laser behind the target that deals [ModifiedDamage] () magic damage to all enemies hit.

### Zippy (3-cost, ADFighter)

- Traits: Guild, Dragon | mana 0/50
- **Somersault Assault**: Gain [ModifiedShield] () Shield for 2 seconds and deal [ModifiedDamage] () physical damage to the highest percent health enemy, Stunning and knocking them back slightly.

### Daeja (4-cost, APCarry)

- Traits: Mirage, Dragon | mana 0/100
- **Windblast**: Passive: Launch three barrages that each deal [ModifiedPassiveDamage] () bonus magic damage and reduce the target's Magic Resist by 5. Active: Deal [ModifiedBlastDamage] () magic damage toward the most enemies in a line.

### Graves (4-cost, ADFighter)

- Traits: Tempest, Cannoneer | mana 0/30
- **Quickdraw**: Passive: Attacks fire 4 bullets in a cone that deal [TotalDamage] () physical damage each. Active: Dash towards the target, fire two attacks, and gain [ModifiedResists] () Armor and Magic Resist for 4 seconds.

### Hecarim (4-cost, APTank)

- Traits: Ragewing, Cavalier | mana 30/90
- **Onslaught of Shadows**: Summon spectral riders that charge through the target, dealing [ModifiedDamage] () magic damage and Stunning enemies hit for 1.5/2/8 seconds.

### Idas (4-cost, APTank)

- Traits: Shimmerscale, Dragon, Guardian | mana 40/100
- **Golden Scales**: For 2 seconds, reduce incoming damage by 30/50/400. Then heal [ModifiedHeal] () Health and grant other nearby allies [ModifiedShieldValue] () Shield for 5 seconds. The Shield grants 30% Attack Speed while it holds.

### Jayce (4-cost, APFighter)

- Traits: Guild, Shapeshifter | mana 0/50
- **Mercurial Justice**: Transform into melee form, dealing [ModifiedMagicDamage] () magic damage to the current target and nearby enemies. Replace this Ability with Mercurial Judgement and gain 40 Armor and Magic Resist for the rest of combat. Mercurial Judgement: Deal [ModifiedSmiteDamage] () magic damage and [ModifiedSecondMagicDamage] () bonus magic damage to nearby enemies. Then, heal for [ModifiedHeal] () health and an additional 20% per enemy struck.

### Nilah (4-cost, ADFighter)

- Traits: Lagoon, Assassin | mana 0/25
- **Slipstream**: Dash through the target, dealing [TotalDamage] () physical damage. If this damages a Shield, gain a Shield equal to 80/100/300% of the damage dealt to it for 4 seconds. Instantly recast if the target is killed or remains Shielded.

### Pantheon (4-cost, ADTank)

- Traits: Whispers, Warrior | mana 30/90
- **Aegis Assault**: Passive: Gain 20/25/40% Durability. Active: Brace a shield for 2 seconds, doubling passive Durability and dealing [TotalDamage] () physical damage in a cone towards enemies over the duration. Wound enemies hit for 3 seconds. {{TFT_Keyword_Wound}}

### Shi Oh Yu (4-cost, ADFighter)

- Traits: Jade, Mystic, Dragon | mana 30/50
- **Jade Form**: Enter Jade stance, gaining [ModifiedDamageReduction] Durability, immunity to crowd control, and empowering the next 3 attacks with special effects. Attack 1: Deal [TotalDamage] () physical damage and Stun the target for 1 second(s). Attack 2: Deal [TotalDamage] true damage. Attack 3: End the stance, dealing [TotalDamage] () physical damage to all enemies in a line and Stunning them for 1 second(s).

### Sohm (4-cost, APCaster)

- Traits: Lagoon, Mage, Dragon | mana 0/40
- **Tideblossom**: Mark an unmarked enemy, dealing [ModifiedDamage] () magic damage to enemies along the way. Marks transfer to a nearby enemy on Mark owner death. After 3 casts, the next cast deals [ModifiedExplodeDamage] () magic damage in a 2-hex radius around all marked enemies.

### Sy'fen (4-cost, ADFighter)

- Traits: Whispers, Bruiser, Dragon | mana 30/75
- **Rampage**: Charge toward the farthest enemy within 2.5 hexes, dealing [TotalDamage] () physical damage and Stunning enemies hit. Then bite an enemy, dealing [TotalBiteDamage] () physical damage and ignoring [ModifiedArmorPenPercent]% () of the target's Armor.

### Tyrant Swain (4-cost, APTank)

- Traits: Darkflight, Dragon | mana 60/125
- **Dragon Master's Decree**: Launch 8 dragonlings toward nearby enemies. Each deals [ModifiedDamage] () magic damage to their target, then returns to restore for 12% missing Health.

### Xayah (4-cost, ADCarry)

- Traits: Ragewing, Swiftshot | mana 0/60
- **Feathers FLY!**: For 4 seconds, attacks leave a feather behind the target and 2/2/4 nearby enemies. At the end of this duration, recall all feathers, dealing [ModifiedPullbackDamagePerFeather] () physical damage per feather to enemies struck.

### Ao Shin (5-cost, APCaster)

- Traits: Tempest, Dragon | mana 0/160
- **Lightning Rain**: Fire a barrage of 20/20/100 lightning strikes at random enemies. Each strike deals [ModifiedDamage] () magic damage and removes 10 current Mana from the current target.

### Aurelion Sol (5-cost, APCaster)

- Traits: Astral, Evoker, Dragon | mana 0/60
- **Black Hole**: Summon a black hole under a random enemy. After 2 seconds, it deals [ModifiedSpellDamage] () magic damage to the target and enemies within 2 hexes, and increases their damage taken by 20% for 10 seconds. Black holes generated after 18 seconds of combat are larger and deal 20% more damage.

### Bard (5-cost, APCaster)

- Traits: Guild, Mystic, Bard | mana 160/240
- **Tempered Fate**: Stun the largest group of enemies for 1.25/2/15 seconds. While Stunned, enemies lose [ModifiedDamageAmpRatio] () Durability. If at least one enemy is struck, Bard dances.

### Shyvana (5-cost, APFighter)

- Traits: Ragewing, Shapeshifter, Dragon | mana 15/60
- **Dragon's Descent**: Transform into a dragon for the rest of combat and replace this ability with Flame Breath. Become briefly untargetable and dive-bomb into the largest group of enemies, dealing [ModifiedInitialDamage] () magic damage in a 3-hex circle and Stunning for 1.25/1.25/10 seconds. After landing, cast Flame Breath immediately. Flame Breath: Breathe fire in a cone, dealing up to [ModifiedFlameBreathDamage] () magic damage to enemies hit. While breathing fire, is always enraged.

### Soraka (5-cost, APCaster)

- Traits: Jade, Starcaller | mana 100/200
- **Starfall**: Passive: While any ally is below 50% Health, gain an additional 15 mana per attack. Active: Call down a shower of 14 stars over the next 2 seconds. Allies are healed for [ModifiedHealing] () each time a star hits them.

### Terra (5-cost, ADTank)

- Traits: Monolith, Dragon | mana 30/70
- **Earthquake**: Stomp three times, creating a surrounding earthquake that deals [TotalDamage] () physical damage. When it ends, gain [ModifiedResistBuff] () Armor and Magic Resist for 8 seconds. While casting, become Unstoppable. {{TFT_Keyword_Unstoppable}}

### Yasuo (5-cost, ADFighter)

- Traits: Mirage, Dragonmancer, Warrior | mana 0/30
- **Sweeping Blade**: Gain [ModifiedShieldAmount] () Shield for 2/2/20 seconds and dash through the target, slashing nearby enemies for [TotalDamage] () physical damage. Every third cast the slash deals 250% damage, hits a larger area, and Stuns enemies for 1.5 seconds. If only one enemy remains, repeatedly slash them until they die.

### Zoe (5-cost, APCaster)

- Traits: Shimmerscale, Spell Thief, Mage | mana 40/90
- **Spell Thief**: Nab an Ability from another dimension and cast it.

## Monsters, summons, pets and other trait-less units (28)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, ADTank)

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Jade Krug (1-cost, ADTank)

- Traits: (none) | mana 0/60
- **Jadesplosion**: Deals its Jade explosion damage around it.

### Krug (1-cost, ADTank)

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, ADTank)

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### GnarBig (11-cost, )

- Traits: (none) | mana 0/100
- **Mega Gnar**: Gnar has transformed!

### Golden Dragon Bank (11-cost, )

- Traits: (none) | mana 0/100
- **Golden Dragon Bank**: Store gold on player combat losses and convert them all to rewards when you win. (Gold from next loss: [TFT7_Shimmerscale_PointsPerLoss]g) Gold generated from Shimmerscale items gets stored here. Total gold stored: [TFT7_Shimmerscale_GoldValue]g (From items: [TFT7_Shimmerscale_PointsFromKills]g)

### Jade Statue (11-cost, )

- Traits: (none)
- **Shatter**: When its Health is depleted, the Jade Statue will explode.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### PetChoncc (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Seastone (11-cost, )

- Traits: (none) | mana 0/80
- **Sea Blessing**: The Seastone periodically grants loot as Lagoon units cast more Abilities. Total casts: [TFTUnitProperty.:TFT7_LagoonCount] Gold granted: [TFTUnitProperty.:TFT7_LagoonRewardCountGold] Rewards granted: [TFTUnitProperty.:TFT7_LagoonRewardCount]

### ShyvanaDragon (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Tempest Strike (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Zoe's Daisy (11-cost, )

- Traits: (none)
- **Smash**: Daisy smashes the ground around her target, dealing [TooltipDaisyDamage] magic damage.

### ZoeOrbs (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 


<a id="set-8"></a>

# Set 8: Monsters Attack!

Source: patch 13.10 data. 75 champions, 28 traits.

## Traits

### A.D.M.I.N.

A.D.M.I.N. programs a custom configuration per player each game.

  - (2-3) 4Tooltip=25, 6Tooltip=100, 1
  - (4-5) 4Tooltip=25, 6Tooltip=100, 1.25
  - (6) 4Tooltip=25, 6Tooltip=100, 2

### Ace

This Trait is active only when you have exactly 1 or 4 unique Aces.

  - (1) ExecuteThreshhold=15
  - (4) ExecuteThreshhold=33

### Aegis

Your team gains bonus Magic Resist, and Aegis units gain more.

  - (2) AegisMagicResist=40, MagicResist=20
  - (3) AegisMagicResist=80, MagicResist=40
  - (4) AegisMagicResist=120, MagicResist=60
  - (5) AegisMagicResist=180, MagicResist=90

### Anima Squad

Anima Squad members pose after champion kills, increasing their fame. Each point of fame grants [HealthPerStack] permanent Health to each Anima Squad member. They also gain: Total fame: [TFTUnitProperty.trait:TFT8_AnimaFameStacks] Bonus Health: [TFTUnitProperty.trait:TFT8_AnimaFameHP]

  - (3-4) HealthPerStack=5, InitialAD=15, InitialAP=15
  - (5-6) HealthPerStack=5, InitialAD=35, InitialAP=35
  - (7) HealthPerStack=5, InitialAD=60, InitialAP=60

### Brawler

Brawlers gain additional maximum Health.

  - (2-3) BonusPercentHealth=0.2
  - (4-5) BonusPercentHealth=0.45
  - (6-7) BonusPercentHealth=0.7
  - (8) BonusPercentHealth=1

### Corrupted

([MinUnits]) Combat start: Lie dormant while absorbing the souls of allies that die. Gain [BonusAP] Ability Power for each soul. Once per combat at [HealthThreshold]% Health (or when your team has died), come alive and fight.

  - (1) BonusAP=40, HealthThreshold=70

### Defender

Your team gains bonus Armor, and Defenders gain more.

  - (2-3) BonusArmor=25, DefenderBonusArmor=50, HexRadius=1
  - (4-5) BonusArmor=70, DefenderBonusArmor=140, HexRadius=1
  - (6) BonusArmor=180, DefenderBonusArmor=360, HexRadius=1

### Duelist

Innate: Increased movement speed. Duelists' attacks grant bonus Attack Speed, up to [MaxStacks] stacks.

  - (2-3) ASPerStack=5, DRPercent=None, MaxStacks=12, 200
  - (4-5) ASPerStack=8, DRPercent=None, MaxStacks=12, 200
  - (6-7) ASPerStack=10, DRPercent=10, MaxStacks=12, 200
  - (8) ASPerStack=16, DRPercent=20, MaxStacks=12, 200

### Forecaster

Depending on the weather, The Forecaster grants a different bonus to adjacent allies. The weather changes every game.

  - (1) RainyManaTier1=20, RainyManaTier2=40, RainyManaTier3=100, SunnyDurationTier1=10, SunnyDurationTier2=10, SunnyDurationTier3=45, SunnyHealthShieldTier1=400, SunnyHealthShieldTier2=600, SunnyHealthShieldTier3=2000, WindyBonusTier1=8, WindyBonusTier2=15, WindyBonusTier3=99, WindyRefresh=5

### Gadgeteen

Each round, Gadgeteens create random modified weapons with powerful effects that fall apart after one round. Gadgeteens also gain Damage and Damage Reduction for each item equipped to them.

  - (3-4) Itembuff=4, ItemsToMake=1
  - (5) Itembuff=8, ItemsToMake=2

### Hacker

Hackers gain Omnivamp and summon a H4ckerr!m. Any unit placed in the rider hex will be sent to the enemy backline and is untargetable for the first [SecondsUntargetable] seconds of combat. Omnivamp: heal for some of damage dealt

  - (3) Horsevamp=None, Omnivamp=0.1, SecondsUntargetable=2
  - (4) Horsevamp=0.15, Omnivamp=0.2, SecondsUntargetable=2
  - (5) Horsevamp=0.3, Omnivamp=0.4, SecondsUntargetable=2

### Heart

When Heart units cast their Ability, your team gains stacking Ability Power for the rest of combat.

  - (2-3) APPerCast=4
  - (4-5) APPerCast=7
  - (6) APPerCast=11

### InfiniTeam

The InfiniTeam opens a portal to an alternate timeline. At the start of combat, any InfiniTeam unit placed on a portal summons an alternate version of themself with different items. They get 1 for each Augment you own. The copies have reduced Health and damage.

  - (3-4) DamageReduction=50, NumberOfPortals=1, PercentHealth=50, 1
  - (5-6) DamageReduction=55, NumberOfPortals=2, PercentHealth=55, 1
  - (7) DamageReduction=85, NumberOfPortals=2, PercentHealth=85, 1

### LaserCorps

When a LaserCorp agent attacks or is hit by an attack, their drone deals magic damage to the agent's target ([ICD] second cooldown). Drones are reassigned to another champion with a drone when the agent dies. Drone lasers deal:

  - (3) ICD=0.4, MagicDamage=27, 1, None, 1, None
  - (4) ICD=0.4, MagicDamage=35, 1, 1, 1, None
  - (5) ICD=0.4, MagicDamage=55, 1, 1, 1, None
  - (6) ICD=0.4, MagicDamage=80, 1, 1, 1, None

### Mascot

Your team heals a percentage of their maximum Health every [Seconds] seconds, and Mascots heal double the amount. When Mascots die, they retreat to the sidelines to cheer on your team. Your team's healing increases by [HealthRegenPerMascot]% for each cheering Mascot.

  - (2-3) BaseHealthRegen=1.5, HealthRegenPerMascot=1, Seconds=2
  - (4-5) BaseHealthRegen=3, HealthRegenPerMascot=1, Seconds=2
  - (6) BaseHealthRegen=6, HealthRegenPerMascot=1, Seconds=2

### Mecha:PRIME

Use the Mecha selector item to choose a PRIME. Combat start: The PRIME combines with the 2 nearest Mecha, absorbing [HealthPercent]% of their Base Health.

  - (3-4) ADPercent=60, APAmount=60, HealthPercent=90
  - (5) ADPercent=55, APAmount=55, HealthPercent=90

### Ox Force

Ox Force units gain bonus defenses. Once per combat, when they would drop below one health, they instead shield themselves for [ShieldPercent]% of their maximum health for [Duration] seconds.

  - (2-3) Defenses=5, Duration=1.5, ShieldPercent=50
  - (4-5) Defenses=35, Duration=1.5, ShieldPercent=50
  - (6) Defenses=75, Duration=1.5, ShieldPercent=50

### Parallel

([MinUnits]) Ezreal learns from his older, more handsome self and joins him in future adventures. Ezreal's Ability becomes a blast that hits all enemies in a line. Ultimate Ezreal's Ability summons two additional temporal duplicates.

  - (2) (no numbers)

### Prankster


  - (2) HealthRegen=150, HealthThreshold=50, StunDuration=None
  - (3) HealthRegen=150, HealthThreshold=50, StunDuration=1.5
  - (4) HealthRegen=350, HealthThreshold=50, StunDuration=2

### Quickdraw

After every 2 damaging ability projectiles, Quickdraw units fire a bonus ability projectile. Bonus shots deal:

  - (2) DamageModifier=0.7, 2
  - (3) DamageModifier=1.4, 2
  - (4) DamageModifier=2.2, 2

### Renegade

Renegade units deal bonus damage, and the last one standing deals more.

  - (3-4) AdditionalBonusDamage=20, BasePercentBonusDamage=25
  - (5-6) AdditionalBonusDamage=30, BasePercentBonusDamage=40
  - (7) AdditionalBonusDamage=40, BasePercentBonusDamage=70

### Riftwalker


  - (3) ZacBloblets=None, ZacBlobletsHealthPercent=50, 175, 12, 1, 1.5

### Spellslinger

Every [RefreshSeconds] seconds, Spellslinger's next attack instead fires a magic orb at a random target, which explodes for [MagicDamage]% Ability Power as magic damage. Spellslingers gain:

  - (2-3) AP=25, MagicDamage=50, RefreshSeconds=5
  - (4-5) AP=45, MagicDamage=50, RefreshSeconds=5
  - (6-7) AP=70, MagicDamage=50, RefreshSeconds=5
  - (8) AP=110, MagicDamage=100, RefreshSeconds=5

### Star Guardian

Star Guardians gain more Mana in combat.

  - (2-3) AdditionalManaPercentage=35
  - (4-5) AdditionalManaPercentage=70
  - (6-7) AdditionalManaPercentage=120
  - (8) AdditionalManaPercentage=175

### Supers

Bonus damage: [TFTUnitProperty.trait:TFT8_Supers_TotalBonusDamage]%

  - (3) BaseBonusDamage=18, BonusDamagePer3Star=3

### Sureshot

Combat start: Sureshots gain bonus Attack Damage now, and every [Frequency] seconds.

  - (2) BonusAD=0.07, Frequency=4
  - (3) BonusAD=0.12, Frequency=4
  - (4) BonusAD=0.18, Frequency=4
  - (5) BonusAD=0.25, Frequency=4

### Threat

Threats do not have a Trait bonus, but instead have powerful Abilities and increased base stats.

  - (1) (no numbers)

### Underground

The Underground must sneak through the sewers after each player combat and crack [RoundCount] locks to open a vault. When the vault opens, you may choose to take the loot now, or attempt another heist for even better rewards. Current Heist Level: [TFTUnitProperty.trait:TFT8_Underground_CurrentStage]

  - (3) LossProgress=3, LossProgressHR=6, RoundCount=10, RoundCountDU=20, WinProgress=2, WinProgressHR=4, None
  - (4) LossProgress=4, LossProgressHR=8, RoundCount=10, RoundCountDU=20, WinProgress=2, WinProgressHR=4, None
  - (5) LossProgress=5, LossProgressHR=10, RoundCount=10, RoundCountDU=20, WinProgress=3, WinProgressHR=6, None
  - (6) LossProgress=7, LossProgressHR=14, RoundCount=10, RoundCountDU=20, WinProgress=4, WinProgressHR=8, None

## Champions (59)

### Ashe (1-cost, )

- Traits: LaserCorps, Sureshot | mana 0/50
- **Ranger's Focus**: Ashe gains [TotalDamage] Attack Damage (60% Attack Damage + [ModifiedADGain]) for the next 5 seconds.

### Blitzcrank (1-cost, )

- Traits: A.D.M.I.N., Brawler | mana 40/80
- **Static Defenses**: Blitzcrank creates an empowered field around himself, reducing all damage taken by [ModifiedDR] for 4 seconds.

### Gangplank (1-cost, )

- Traits: Supers, Duelist | mana 0/50
- **Trial by Fire**: Gangplank fires a flaming bullet at his target. It deals [ModifiedDamage] magic damage and 33% Wounds the enemy for 8 seconds. Wound: reduces healing received

### Kayle (1-cost, )

- Traits: Underground, Duelist | mana 0/30
- **Starfire Spellblade**: Kayle's next 3 attacks deal [ModifiedPercentDamage] physical damage (155% Attack Damage ) to her target, and [ModifiedBonusDamage] magic damage to enemies within 1 hex of the target.

### Lucian (1-cost, )

- Traits: InfiniTeam, Quickdraw, Renegade | mana 0/40
- **Chrono-Barrage**: PASSIVE: Lucian's attacks deal [BonusDamagePerAttack] bonus magic damage. ACTIVE: Lucian fires 4 shots in his target's direction. Each shot deals [DamagePerShot] magic damage to the first target hit.

### Lulu (1-cost, )

- Traits: Gadgeteen, Heart | mana 0/50
- **Glitterlance**: Lulu fires a magic bolt at her target, dealing [ModifiedDamage] magic damage to the first enemy hit and [SecondaryModifiedDamage] to other enemies behind.

### Lux (1-cost, )

- Traits: Star Guardian, Spellslinger | mana 0/70
- **Lucent Singularity**: Lux sends out a singularity to her current target. When it detonates, it reduces the target's Attack Damage by 40% for 4 seconds, and deals [ModifiedDamage] to nearby enemies.

### Nasus (1-cost, )

- Traits: Anima Squad, Mascot | mana 0/60
- **Bonk!**: Nasus slams his staff on his target, dealing [TotalDamage] physical damage (225% Attack Damage + [ModifiedBonusDamage]) and 40% Chilling them for 3 seconds. Chill: reduce Attack Speed

### Pantheon (1-cost, )

- Traits: InfiniTeam, Heart | mana 30/60
- **Time Out!**: Pantheon leaps onto his target, dealing [TotalDamage] magic damage (290/435/650% Magic Resist) and stunning them for 1.5 seconds. He then grants himself a [ModifiedShieldingAmount] Health shield for 4 seconds.

### Poppy (1-cost, )

- Traits: Gadgeteen, Defender | mana 40/80
- **Buckler Toss**: Poppy throws her buckler at the farthest enemy, dealing 160/240/320% of her Armor as magic damage. (total: [TotalDamage]) The buckler then bounces back, granting Poppy a shield that blocks [ModifiedShield] damage.

### Renekton (1-cost, )

- Traits: LaserCorps, Brawler | mana 0/50
- **Dominus**: Renekton empowers himself for 8 seconds, gaining [ModifiedBonusHealth] maximum Health, and causing his attacks to deal [TotalDamage] (6% Maximum Health ) as bonus magic damage.

### Sylas (1-cost, )

- Traits: Anima Squad, Renegade | mana 30/80
- **Battle Blast**: Sylas whirls his chains around him, dealing [ModifiedPercentHealthDamage] (10/12/15% Maximum Health ) magic damage to nearby enemies and healing himself for [ModifiedHealAmount] Health.

### Wukong (1-cost, )

- Traits: Mecha:PRIME, Defender | mana 50/100
- **Stone Skin**: Wukong spins his staff around him, dealing [ModifiedDamage] magic damage to nearby enemies. He gains 60/70/80 Armor and Magic Resist for each enemy hit for 3 seconds.

### Annie (2-cost, )

- Traits: Gadgeteen, Ox Force, Spellslinger | mana 30/90
- **Burst Shield**: Annie blasts a cone of fire, dealing [ModifiedDamage] magic damage to enemies in front of her, then creates a [ModifiedShield] Health shield on herself for 4 seconds.

### Camille (2-cost, )

- Traits: A.D.M.I.N., Renegade | mana 0/70
- **Tactical Sweep**: Camille sweeps with her leg, dealing [TotalDamage] physical damage (240% Attack Damage + [ModifiedBonusDamage]) and Disarming enemies hit for 1.75/1.8/2 seconds. Disarm: target cannot move or attack

### Draven (2-cost, )

- Traits: Mecha:PRIME, Ace | mana 70/130
- **Whirling Death**: Draven hurls 2 axes at his target's direction, which then come back to him. Enemies caught in their path take [TotalDamage] physical damage (130% Attack Damage + [ModifiedDamage]), reduced by 25% for each enemy they have already hit.

### Ezreal (2-cost, )

- Traits: Underground, Parallel, Quickdraw | mana 0/55
- **Sabotage **: Ezreal fires an orb of physical sabotage at his target, 30/40/50% Chilling them for 4 seconds. Then he fires another shot, which deals [TotalDamage] magic damage to the target and nearby enemies. Chill: reduce Attack Speed

### Fiora (2-cost, )

- Traits: Ox Force, Duelist | mana 30/80
- **En Garde!**: Fiora lunges at her enemy, dealing [ModifiedDamage] physical damage (250/250/260% Attack Damage ) to all enemies hit. For the next 8 seconds, she takes 20% less damage and her attacks restore [ModifiedHeal] Health.

### Jinx (2-cost, )

- Traits: Anima Squad, Prankster | mana 0/65
- **Fishbones**: Jinx fires a rocket at her target that explodes on impact, damaging all nearby enemies for [ModifiedDamage] magic damage and Stunning them for 1.5 seconds. Stun: cannot move, attack, or cast Abilities

### Lee Sin (2-cost, )

- Traits: Supers, Heart, Brawler | mana 0/70
- **Safeguard**: Lee Sin dashes towards a nearby ally, shielding them and himself for [ModifiedShieldAmount] Health for 4 seconds. He then dashes back and deals [TotalDamage] physical damage (275/275/280% Attack Damage ) to his target. If there are no nearby allies, he gains a [DoubledShieldAmount] Health shield instead.

### Malphite (2-cost, )

- Traits: Supers, Mascot | mana 40/80
- **Ground Slam**: Malphite slams the ground, dealing [ModifiedDamage] magic damage to nearby enemies and granting himself 100/125/150 Armor for 4 seconds.

### Pyke (2-cost, )

- Traits: Riftwalker, Hacker | mana 60/120
- **Phantom Undertow**: Pyke stabs his current target for [ModifiedStabDamage] true damage and dashes behind the furthest enemy. After 1 second, all enemies in the path of Pyke's dash are stunned for 1.25/1.5/1.75 seconds and take [ModifiedDamage] magic damage.

### Rell (2-cost, )

- Traits: Star Guardian, Defender | mana 80/150
- **Iron Bond**: Rell forms a tether between herself and her lowest percent Health ally, dealing [ModifiedDamage] magic damage to all enemies between herself and her bound ally. She also grants herself and her bound ally 50/60/70% of Rell's current Armor and Magic Resist for 4 seconds.

### Sivir (2-cost, )

- Traits: InfiniTeam, Sureshot | mana 0/50
- **Piece of Time**: Sivir throws her blade at her target, dealing [TotalDamage] physical damage (170/170/180% Attack Damage + [ModifiedBonusDamage]). Her blade then splits, hitting the 3 nearest enemies for [MiniBladeDamage] physical damage (120% Attack Damage ).

### Vi (2-cost, )

- Traits: Underground, Brawler, Aegis | mana 0/80
- **Blast Shield**: Vi grants herself a [ModifiedShieldAmount] Health shield for 4 seconds. Then she deals [TotalDamage] physical damage to nearby enemies (250% Attack Damage ) and 50% Sunders them for 4 seconds. Sunder: reduce Armor

### Yasuo (2-cost, )

- Traits: LaserCorps, Duelist | mana 0/70
- **Steel Tempest**: Yasuo fires a whirlwind at his target dealing [ModifiedDamage] magic damage and knocking up units hit for 1.5 seconds. If the current position isn't ideal, he will dash up to 1 hex in order to find more units in a line.

### Alistar (3-cost, )

- Traits: Ox Force, Mascot, Aegis | mana 75/175
- **Pulverize**: Alistar slams the ground underneath his target, dealing [ModifiedDamage] magic damage and knocking them up for 1.5 seconds. He then roars, healing himself and the lowest health ally champion for [TotalHealing] (20% Maximum Health )

### Gnar (3-cost, )

- Traits: Gadgeteen, Prankster | mana 20/80
- **Robo-Rage**: Gnar slashes in a 2 hex radius around himself, dealing [ModifiedAoEDamage] physical damage (100% Attack Damage + [ModifiedAPDamage]) to enemies hit. His current target takes 250% damage (Total: [ModifiedTargetDamage]), and the Ability critically strikes them.

### Jax (3-cost, )

- Traits: Mecha:PRIME, Brawler
- **Adaptive Strike**: PASSIVE: Jax leaps to his target and deals [ModifiedBaseDamage] bonus magic damage every 3rd attack. The bonus damage increases by [ModifiedStackDamage] every 3rd attack, and stacks up to 7 times. Current Damage on hit: [TFTUnitProperty.:TFT8_Jax_TotalDamage]

### Kai'Sa (3-cost, )

- Traits: Star Guardian, Quickdraw | mana 20/105
- **Starcharged**: PASSIVE: Kai'Sa's attacks apply Plasma. Her next attack against a target with 2 stacks of Plasma deals [ModifiedDamage] magic damage and removes the Plasma. ACTIVE: Kai'Sa gains 60% Attack Speed for 10 seconds. This bonus stacks!

### LeBlanc (3-cost, )

- Traits: A.D.M.I.N., Spellslinger, Hacker | mana 0/60
- **Sigil of Malice**: LeBlanc fires 6/6/8 sigils at her target, each dealing [ModifiedDamage] magic damage. If the target dies, she completes casting the remaining sigils at the nearest enemy but adds 1 more sigil.

### Morgana (3-cost, )

- Traits: Threat | mana 50/100
- **Dark Binding**: PASSIVE: When Morgana deals damage to an enemy, she 40% Sunders and Shreds them for 4 seconds. ACTIVE: Morgana sends out dark bindings towards the 2 closest enemies, dealing [TotalDamage] magic damage and stunning them for 1.5 seconds. Shred: reduce Magic Resist Sunder: reduce Armor

### Nilah (3-cost, )

- Traits: Star Guardian, Duelist | mana 50/125
- **Apotheosis**: Nilah whirls her whip-blade, dealing [WhirlDamage] magic damage to nearby enemies while healing herself and allies for [ModifiedHealAmount] Health over 1 second. She finishes with a burst that deals [BurstDamage] magic damage.

### Rammus (3-cost, )

- Traits: Threat | mana 60/120
- **Sand Slam**: PASSIVE: +60/90/150 Armor ACTIVE: Rammus powers himself up and leaps to the largest group of nearby enemies, dealing [TotalDamage] magic damage (150/175/205% Armor + [ModifiedBonusDamage]), knocking them up for 1.5 seconds, and reducing their Attack Damage by 40% for 4 seconds.

### Riven (3-cost, )

- Traits: Anima Squad, Brawler, Defender | mana 30/90
- **Bunny Blade**: Riven gains a [ModifiedShield] Health shield for 4 seconds and empowers her next 3 attacks with [ModifiedDamage] bonus magic damage. If Riven has more Health than her target, she deals 100% more bonus magic damage.

### Shen (3-cost, )

- Traits: InfiniTeam, Defender, Hacker | mana 50/100
- **Stand United**: Shen tethers himself to the ally who has done the most damage this round and reduces all damage he takes by [ModifiedDR] for 4 seconds. While the tether is active, his ally is invunerable and any damage they would take is redirected to Shen instead.

### Sona (3-cost, )

- Traits: Underground, Spellslinger, Heart | mana 0/40
- **Crescendo Sequence**: Sona fires 2 beams that cycle between 3 phases: 1: Deal [ModifiedQDamage] magic damage to the nearest enemies 2: Heal the lowest Health allies for [ModifiedWHeal] Health 3: Deal [ModifiedEDamage] magic damage to the nearest enemies and Stun them for 1.5 seconds Stun: cannot move, attack, or cast Abilities

### Vayne (3-cost, )

- Traits: Anima Squad, Duelist | mana 0/40
- **Silver Bolt**: Vayne fires a heavy bolt at her target, dealing [TotalDamage] true damage (150/150/160% Attack Damage + [ModifiedDamage]).

### Vex (3-cost, )

- Traits: Riftwalker, Mascot | mana 0/40
- **Shadow Surge**: Vex launches a blast of gloom at her current target, dealing [ModifiedDamage] magic damage. Every third cast, her gloom becomes unstoppable and passes through enemies, dealing [ModifiedBurstDamage] magic damage to the first enemy hit and [ModifiedSecondaryDamage] magic damage to subsequent enemies hit.

### Aatrox (4-cost, )

- Traits: Threat | mana 0/75
- **Soul Crush**: Aatrox sucks the soul of nearby enemies dealing [ModifiedSoulDamage] magic damage and gaining [MaxHPIncrease] maximum Health (30/30/66% of his max HP ). He then unleashes his demonic form for the rest of combat, replacing his spell with The Darkin Blade. The Darkin Blade: Aatrox leaps up and slams his greatsword down on his target, dealing [ModifiedDamage] magic damage to all enemies hit and healing himself for [ModifiedHealing] Health.

### Aurelion Sol (4-cost, )

- Traits: Threat | mana 30/90
- **Meteor Shower**: Aurelion Sol calls down 4/4/6 meteors on random enemies. Meteors deal [ModifiedDamage] magic damage, then 10% Burn and 33% Wound enemies for 10 seconds. Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received

### Bel'Veth (4-cost, )

- Traits: Threat | mana 0/40
- **Endless Banquet**: Bel'Veth dashes around her target and unleashes a flurry of [NumAttacks] attacks (250% of her Attacks per second), each dealing [TotalDamage] physical damage (75/75/200% Attack Damage + [ModifiedBonusDamage]). Each time this is cast, Bel'Veth gains 30% bonus Attack Speed for the rest of combat.

### Ekko (4-cost, )

- Traits: Star Guardian, Prankster, Aegis | mana 70/140
- **Star Diver**: Ekko dives in, granting himself a [ModifiedShieldAmount] shield for 5 seconds and taunts nearby enemies. When the shield breaks, he emits a time field, dealing [ModifiedDamage] to nearby enemies and slowing their attack speed by 50% for 2 seconds.

### Garen (4-cost, )

- Traits: Mecha:PRIME, Defender | mana 100/200
- **Mecha Justice**: Garen brings his sword down on the largest group of nearby enemies, dealing [TotalDamage] physical damage (140/140/325% of Attack Damage + [ModifiedAPDamage]) to enemies within 2 hexes and stunning them for 2/2.25/6 seconds.

### Jhin (4-cost, )

- Traits: Riftwalker, Renegade | mana 0/4
- **Otherworldy Flourish**: PASSIVE: Jhin's Attack Speed is always 0.8/0.8/1. He converts 80% of bonus Attack Speed into Attack Damage and replaces every 4th attack with an Ability cast. ACTIVE: Fire a bullet towards the lowest health enemy, dealing [TotalDamage] physical damage (2.44/2.44/5.44% Attack Damage ) to all enemies in a line, increased up to [ModifiedBonusDamage] based on their missing Health. The bullet's damage is reduced by 44% each time it hits an enemy.

### Miss Fortune (4-cost, )

- Traits: Anima Squad, Ace, Quickdraw | mana 0/120
- **Bullet Time**: Miss Fortune fires 8 waves of bullets towards the largest group of enemies within channels for 2 seconds. Enemies take [ModifiedDamage] magic damage from the first bullet that hits them, and [ModifiedReducedDamage] magic damage from any other bullets that hit.

### Neeko (4-cost, )

- Traits: Star Guardian, Spellslinger | mana 0/80
- **Hop Blossoms**: Neeko's frog hops 3 times from enemy to enemy, dealing [ModifiedDamage] magic damage in a small area each time. The final hop is empowered and deals [ModifiedBonusDamage] magic damage in a large area instead.

### Samira (4-cost, )

- Traits: Underground, Sureshot, Ace | mana 0/65
- **Flair**: Samira rapidly fires bullets that deal a total of [TotalDamage] physical damage (460/460/1200% Attack Damage + [ModifiedBonusDamage]). Bullets ricochet to nearby enemies for 20% less damage.

### Twisted Fate (4-cost, )

- Traits: InfiniTeam, Duelist, Spellslinger | mana 0/30
- **It's Time to Duel!**: Twisted Fate draws 6/6/12 cards and begins drawing another card after every auto attack. After 2.5 seconds, Twisted Fate throws all of his drawn cards, each dealing [ModifiedBlueCardDamage] magic damage. The final card thrown is a red card that deals [ModifiedAOEDamage] magic damage to nearby enemies.

### Viego (4-cost, )

- Traits: Ox Force, Renegade, Heart | mana 0/45
- **Heartbreaker**: Viego becomes unstoppable, leaping to the largest cluster of nearby enemies and slamming his sword down on the enemy with the lowest Health, dealing [ModifiedDamage] magic damage to the primary target and [ModifiedAOEDamage] magic damage to enemies nearby. If Viego kills an enemy, he casts again but deals 18/18/1% less damage.

### Warwick (4-cost, )

- Traits: A.D.M.I.N., LaserCorps, Brawler | mana 40/90
- **hack&SLASH**: PASSIVE: Warwick gains [ModifiedOmnivamp] Omnivamp. Upon his first takedown, he enters KILL MODE 2.0, granting himself 60% Attack Speed and causing him to leap to new targets for the rest of combat. ACTIVE: Warwick attacks his target 6 times, dealing a total of [TotalDamage] physical damage. (460/480/1200% Attack Damage). If his target dies he gains 10 mana for each remaining slash.

### Fiddlesticks (5-cost, )

- Traits: Corrupted, Threat | mana 66/66
- **Dark Harvest**: Fiddlesticks teleports into the largest cluster of enemies and fears them for 1/1/5 seconds. For the next 5/5/30 seconds, Fiddlesticks drains the life from nearby enemies dealing [ModifiedDamage] magic damage per second and healing for 20% of the damage dealt.

### Janna (5-cost, )

- Traits: Forecaster, Spellslinger | mana 40/120
- **Twister!**: Janna sends a twister towards the largest group of enemies. The twister expands when it reaches its destination, knocking all nearby enemies up in the air, Stunning them for 1.25/1.5/8 seconds, and dealing [TotalMagicDamage] magic damage over for 3 seconds. Stun: cannot move, attack, or cast Abilities

### Leona (5-cost, )

- Traits: Mecha:PRIME, Renegade, Aegis | mana 0/75
- **Deus Ex Machina**: Leona locks onto her target and calls down an orbital laser strike. The laser deals [ModifiedMaxHealthPerSec] true damage per second to the target until the target dies, and [ModifiedDamage] magic damage to nearby enemies.

### Mordekaiser (5-cost, )

- Traits: LaserCorps, Ace | mana 60/150
- **Skylines**: Mordekaiser transports the arena into his realm for 4/4/6 seconds. He 50/50/70% Shreds ALL enemies, then slams a building onto the arena that deals [ModifiedDamage] magic damage to enemies hit. Shred: reduce Magic Resist

### Nunu & Willump (5-cost, )

- Traits: Gadgeteen, Mascot | mana 0/90
- **Biggest Roboball Ever!**: PASSIVE: Nunu and Willump roll a ball around the arena towards the furthest enemy. The ball deals [ModifiedDamage] magic damage and briefly stuns all enemies it passes through. When they reach their target, they will find a new destination. Nunu and Willump gain 30/30/90 Mana per second. ACTIVE: Nunu and Willump grow the ball, increasing their movement speed, the size of the ball, and the amount of damage by 40%.

### Syndra (5-cost, )

- Traits: Star Guardian, Heart | mana 70/125
- **Force of Will**: Syndra grants a [ModifiedShieldAmount] Health shield to a random champion on your bench, then flings them into the arena inside an orb at her closest enemy. The orb deals [ModifiedDamage] magic damage and briefly knocks up all enemies hit. Traits of champions flung onto the board do not become active, except for Corrupted.

### Ultimate Ezreal (5-cost, )

- Traits: InfiniTeam, Parallel, Sureshot | mana 50/150
- **Alternate Ezrealities**: Ezreal shatters the timestream, calling in 5/5/25 temporal duplicates of himself that each fire a blast of energy towards the largest group of enemies. Enemies hit by a blast take [TotalDamage] physical damage (133/133/533% of Attack Damage + [ModifiedAPDamage]).

### Urgot (5-cost, )

- Traits: Threat | mana 80/160
- **Undertow**: PASSIVE: Urgot's claw fires 5/5/20 attacks per second, each dealing [DamagePerShot] ([ADModified]+[ASModified]) physical damage. ACTIVE: Urgot summons a gigantic wave behind his enemies that deals 5/8/75% of their maximum Health as magic damage. The wave knocks enemies forward 1 hex, Stuns them for 1/1.5/5 seconds, and has a [ModifiedTreasureChance] chance to dredge up treasure (gold and loot). Stun: cannot move, attack, or cast Abilities

## Monsters, summons, pets and other trait-less units (16)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Hackerim (1-cost, )

- Traits: (none)
- **Infiltration**: Combat start: H4kerr!m picks up the unit in the rider hex and then charges directly forward into the enemy backline.

### Target Dummy (1-cost, )

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Glitched Egg (3-cost, )

- Traits: (none) | mana 0/100
- **Hatching**: Each turn this dimensionally unstable egg gets closer to hatching, which is definitely not alarming at all.

### Archive Of Augments (8-cost, )

- Traits: (none) | mana 0/100
- **Archive Of Augments**: Drag this Archive to the Shop to open an Armory full of Augments! You will have 30 seconds to choose. Depending on when the Archive is sold, the shop will have a higher tier of Augments available. Stage 1-2: Tier 1 (Silver) Stage 3: Tier 2 (Gold) Stage 4+: Tier 3 (Prismatic)

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Ornn Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Ornn Item Anvil**: Drag this to the Shop to open an Armory full of Artifacts crafted by Ornn! You will have 30 seconds to choose.

### The Golden Egg (8-cost, )

- Traits: (none) | mana 0/100
- **Hatching**: This egg will soon hatch into tons of loot! Victorious player combats accelerate the hatch timer by an additional turn.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Giant Crabgot (11-cost, )

- Traits: (none) | mana 0/300
- **Undertow**: PASSIVE: Urgot focuses fire on the nearest enemy, raining blasts into them that deal physical damage. ACTIVE: Urgot summons 2 gigantic waves from above and below that converge towards the center of the board and each deal [ModifiedDamage] magic damage. The force of the wave knocks back enemies 1 hex and stuns them for 2 seconds.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Nemesis Morgana (11-cost, )

- Traits: (none) | mana 0/120
- **Corrupted Ooze**: PASSIVE: Morgana's damaging attacks and abilities 50% Shred and Sunder enemies. ACTIVE: Morgana spawns growing toxic pools under random targets for 6 seconds. The pools deal [TotalDamage] magic damage every second and heal Morgana for 50% of the damage dealt. Sunder: reduce Armor Shred: reduce Magic Resist

### Target Dummy (11-cost, )

- Traits: (none) | mana 0/150
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Volcanic Sol (11-cost, )

- Traits: (none) | mana 0/300
- **Meteor Shower**: PASSIVE: Aurelion Sol's attacks 30% Burn his target and a random ally. ACTIVE: Aurelion Sol calls down 10 meteors on random enemies. Meteors deal [ModifiedDamage] magic damage, then 30% Burn and 50% Wound enemies for 10 seconds. Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received

### Zac (11-cost, )

- Traits: (none) | mana 0/100
- **Unstable Matter**: Zac bounces into the air and slams down, dealing [ModifiedDamage] + 20% of his current Health as magic damage to nearby enemies. Then he heals 20% of his missing Health.


<a id="set-9"></a>

# Set 9: Runeterra Reforged

Source: patch 14.10 data. 90 champions, 29 traits.

## Traits

### Bastion

Bastion champions gain Armor and Magic Resist. This is increased by [BonusPercent]% for the first [Duration] seconds of combat.

  - (2-3) BonusArmor=20, BonusMR=20, BonusPercent=1, Duration=10, StatMultiplier=2
  - (4-5) BonusArmor=45, BonusMR=45, BonusPercent=1, Duration=10, StatMultiplier=2
  - (6-7) BonusArmor=75, BonusMR=75, BonusPercent=1, Duration=10, StatMultiplier=2
  - (8) BonusArmor=140, BonusMR=140, BonusPercent=1, Duration=10, StatMultiplier=2

### Bruiser

Your units gain [TeamFlatHealth] Health. Bruisers gain additional maximum Health.

  - (2) BonusPercentHealth=0.1, TeamFlatHealth=100
  - (3) BonusPercentHealth=0.12, TeamFlatHealth=100
  - (4-5) BonusPercentHealth=0.45, TeamFlatHealth=100
  - (6) BonusPercentHealth=0.8, TeamFlatHealth=100

### Challenger

Challengers gain bonus Attack Speed. When their target dies, Challengers dash to a new target and increase their Attack Speed bonus by [IncreasePct]% for [BurstDuration] seconds.

  - (2-3) BonusAS=30, BurstDuration=2.5, IncreasePct=0.5
  - (4-5) BonusAS=65, BurstDuration=2.5, IncreasePct=0.5
  - (6-7) BonusAS=105, BurstDuration=2.5, IncreasePct=0.5
  - (8) BonusAS=150, BurstDuration=2.5, IncreasePct=0.5

### Darkin

When Aatrox or the holder of the Darkin Blade dies, the Darkin Blade is equipped to the nearest allied champion, granting them [BonusHealth] Health and [BonusOmnivamp]% Omnivamp. After being equipped to a champion for [RespawnTimer] seconds, Aatrox will revive upon their death instead.

  - (1) BonusHealth=350, BonusOmnivamp=0.15, RespawnTimer=4

### Deadeye

Innate: +[BonusRange] Range Every [ShotFrequency] seconds, Deadeyes attack the enemy with the highest percent Health and deal bonus damage.

  - (2-3) BonusRange=1, DamageAmp=0.2, ShotFrequency=3
  - (4-5) BonusRange=1, DamageAmp=0.65, ShotFrequency=3
  - (6) BonusRange=1, DamageAmp=1.5, ShotFrequency=3

### Demacia

Your strongest Demacians become Elite and equip a Radiant item each combat. Elites grant Armor and Magic Resist to themselves and adjacent allies, but this does not stack. Radiant items are stronger versions of full items.

  - (3-4) BonusDefenses=5, NumElites=1
  - (5-6) BonusDefenses=25, NumElites=2
  - (7-8) BonusDefenses=45, NumElites=3
  - (9) BonusDefenses=150, NumElites=6

### Empress

When Bel'Veth kills an enemy, they leave behind Void Coral that she will consume. The first Coral increases her maximum Health by [PercentBonusMaxHealth]%, while further Coral heals her [PercentHealthHeal]% maximum Health. When a Void Coral is consumed, she deals [PercentHealthMagicDamage]% maximum Health as magic damage to enemies within [HexRange] hexes.

  - (1) HexRange=2, PercentBonusMaxHealth=0.5, PercentHealthHeal=0.2, PercentHealthMagicDamage=0.1

### Freljord

After [DelayTime] seconds, an ice storm strikes the battlefield. Enemies take a percentage of their max Health as true damage and gain debuffs. for [Duration] seconds Sunder: Reduce Armor Shred: Reduce Magic Resist

  - (2) DelayTime=8, Duration=10, PercentHealthDamage=10, ShredSunder=30, 1
  - (3) DelayTime=8, Duration=10, PercentHealthDamage=18, StunDuration=1.5, 2

### Gunner

When Gunner champions attack, they gain bonus Attack Damage, up to [MaxStacks] stacks.

  - (2-3) MaxStacks=8, PercentAD=7
  - (4-5) MaxStacks=8, PercentAD=13
  - (6) MaxStacks=8, PercentAD=25

### Invoker

Every [TickRate] seconds, your units gain Mana.

  - (2-3) SelfBonusMana=None, TeamAP=None, TeamMana=5, TickRate=3
  - (4-5) SelfBonusMana=15, TeamAP=None, TeamMana=5, TickRate=3
  - (6-7) SelfBonusMana=15, TeamAP=None, TeamMana=20, TickRate=3
  - (8) SelfBonusMana=None, TeamAP=30, TeamMana=40, TickRate=3

### Ionia

Each Ionian has a unique bonus within their Ability. Every [SpiritDuration] seconds, a number of Ionians are enlightened to their spirit form, gaining [Mana] mana and doubling stat bonuses for [SpiritDuration] seconds.

  - (3-5) EmblemBonus=25, Mana=20, NumberOfSpirits=1, SpiritDuration=4, TooltipBonus=1, 8, 25, 25, 2, 180, 1, 5, 15, 15, 25, 3
  - (6-8) EmblemBonus=50, Mana=20, NumberOfSpirits=2, SpiritDuration=4, TooltipBonus=2, 16, 50, 50, 2, 360, 1, 10, 30, 30, 50, 6
  - (9) EmblemBonus=106.25, Mana=20, NumberOfSpirits=4, SpiritDuration=4, TooltipBonus=4.25, 38.25, 106.25, 106.25, 2, 765, 1, 21.25, 63.75, 63.75, 106.25, 12.75

### Juggernaut

Juggernaut champions take less damage as their Health decreases.

  - (2-3) Duration=7, MaxDR=0.25, MinDR=0.15
  - (4-5) Duration=7, MaxDR=0.35, MinDR=0.2
  - (6) Duration=7, MaxDR=0.5, MinDR=0.35

### Multicaster

Multicasters cast their Ability 1 additional time. Bonus casts have reduced effectiveness.

  - (2) BonusCasts=1, DamageReduction=0.55, ManaPerAuto=None
  - (3) BonusCasts=1, DamageReduction=0.35, ManaPerAuto=None
  - (4) BonusCasts=1, DamageReduction=0.35, ManaPerAuto=5

### Noxus

Noxus champions gain Health, Ability Power and Attack Damage. This is increased by [BonusRatio]% for each different opponent that either you have conquered in combat or is dead. Conquered bonus: [TFTUnitProperty.trait:TFT9_Trait_ConquerorBonus]% / [BonusRatio]%

  - (3-4) AD=16, AP=16, BonusRatio=0.05, Health=160
  - (5-6) AD=30, AP=30, BonusRatio=0.05, Health=300
  - (7-8) AD=42, AP=42, BonusRatio=0.05, Health=420
  - (9) AD=75, AP=75, BonusRatio=0.05, Health=750

### Piltover

Gain the T-Hex. Every time you lose a player combat, the T-Hex gains Charges. Winning converts the Charges to Power for the T-Hex and grants you loot based on the number of Charges converted.

  - (3-5) None, 1
  - (6) 1, 1

### Redeemer

Whenever an ally gains a Shield, Senna grants them [AttackSpeed]% stacking Attack Speed for the rest of combat.

  - (1) AttackSpeed=0.06

### Rogue


  - (2-3) BleedDuration=None, BleedPct=None, DamageReduction=100, HealthThreshold=0.35, HexRange=4, StealthDuration=1.5
  - (4) BleedDuration=2.5, BleedPct=0.55, DamageReduction=100, HealthThreshold=0.5, HexRange=4, StealthDuration=1.5

### Shadow Isles

After dealing or receiving damage [DamageInstances] times, Shadow Isles champions gain a Shield for [Duration] seconds and become Spectral for the rest of combat. Spectral champions gain Mana every second.

  - (2-3) DamageInstances=10, Duration=15, ManaRegen=6, MaxHealthShield=45, TickRate=1
  - (4-5) DamageInstances=10, Duration=15, ManaRegen=12, MaxHealthShield=80, TickRate=1
  - (6) DamageInstances=10, Duration=15, ManaRegen=18, MaxHealthShield=115, TickRate=1

### Shurima

Every [PulsePeriod] seconds, Shurimans heal [HealthRegen]% maximum Health. After [SecondsToAscend] seconds, select Shurimans Ascend and gain [BonusHealthPercent]% maximum Health and [BonusASPercent]% Attack Speed.

  - (2-3) AscensionStrengthBoost=None, BonusASPercent=0.3, BonusHealthPercent=0.2, HealthRegen=0.05, PulsePeriod=4, SecondsToAscend=8, 2
  - (4-5) AscensionStrengthBoost=0.3, BonusASPercent=0.3, BonusHealthPercent=0.2, HealthRegen=0.05, PulsePeriod=4, SecondsToAscend=8, 2
  - (6-8) AscensionStrengthBoost=0.6, BonusASPercent=0.3, BonusHealthPercent=0.2, HealthRegen=0.05, PulsePeriod=4, SecondsToAscend=8, 2
  - (9) AscensionStrengthBoost=1.11, BonusASPercent=0.3, BonusHealthPercent=0.2, HealthRegen=0.05, PulsePeriod=4, SecondsToAscend=8, 2

### Slayer

Slayers gain [Omnivamp]% Omnivamp. Slayers deal bonus damage, doubled against units below [HealthThreshold]% Health.

  - (2-3) BonusDamageBase=6, HealthThreshold=0.66, Omnivamp=12
  - (4-5) BonusDamageBase=20, HealthThreshold=0.66, Omnivamp=12
  - (6) BonusDamageBase=40, HealthThreshold=0.66, Omnivamp=12

### Sorcerer

Sorcerers gain bonus Ability Power. When an enemy dies after being damaged by a Sorcerer, they deal a percentage of that enemy's maximum Health to another enemy.

  - (2-3) BonusAP=25, HealthPct=0.07, TargetNum=1, 3
  - (4-5) BonusAP=50, HealthPct=0.1, TargetNum=1, 3
  - (6-7) BonusAP=90, HealthPct=0.12, TargetNum=2, 3
  - (8) BonusAP=135, HealthPct=0.15, TargetNum=2, 3

### Strategist

Combat start: Allies in the front 2 rows gain a Shield for [ShieldDuration] seconds. Allies in the back 2 rows gain Ability Power.

  - (2) BonusAP=15, ShieldDuration=15, ShieldHP=250
  - (3) BonusAP=25, ShieldDuration=15, ShieldHP=400
  - (4) BonusAP=40, ShieldDuration=15, ShieldHP=600
  - (5) BonusAP=60, ShieldDuration=15, ShieldHP=900

### Targon

All of your units' healing and shielding is improved.

  - (2) HealShieldIncrease=0.18
  - (3) HealShieldIncrease=0.35
  - (4) HealShieldIncrease=0.6

### Technogenius

Gain a placeable Apex Turret with 3 upgrade slots. Upgrades to the Apex Turret will show up in your shop for [GoldCost] gold. The Apex Turret shares Heimerdinger's Attack Speed and Ability Power. You may only have 1 Apex Turret.

  - (1) GoldCost=6, 1, 100

### Vanquisher

Damage from Vanquisher Abilities can critically strike. Vanquishers gain bonus Critical Strike Chance and Critical Strike Damage.

  - (2-3) BaseCritChance=15, CritDmg=15
  - (4-5) BaseCritChance=35, CritDmg=35
  - (6) BaseCritChance=55, CritDmg=55

### Void

Get a placeable void egg. At the start of combat, it hatches into an unspeakable horror and knocks up adjacent enemies. Each Void star level increases the horror's Health and Ability Power by [PercentHealthAPPerStarLevel]%.

  - (3-5) PercentHealthAPPerStarLevel=0.25, 0.4
  - (6-7) PercentHealthAPPerStarLevel=0.25, 0.4
  - (8) PercentHealthAPPerStarLevel=0.25, 0.4

### Wanderer

Ryze's ability changes depending on the Region Portal players voted for at the start of the game.

  - (1) (no numbers)

### Yordle

Your units gain [BonusAttackSpeed]% Attack Speed per star level. If you have three 3-star champions, your Yordles can become 4-star, which gives their Ability a wacky upgrade! Ties for most items are broken by the Yordle fielded most recently.

  - (3-4) BonusAttackSpeed=0.1, DodgeChance=100, DodgeDuration=2, ManaReduction=15, 1, 0.5, 1
  - (5) BonusAttackSpeed=0.1, DodgeChance=100, DodgeDuration=2, ManaReduction=15, 2, 0.5, 2

### Zaun

Zaun champions create random chem-mods that only they can use. Champions can be modded once, and mods can be removed by benching or selling the champion.

  - (2-3) 1, None
  - (4-5) 2, 1
  - (6) 5, 1

## Champions (69)

### Cassiopeia (1-cost, APCaster)

- Traits: Noxus, Shurima, Invoker | mana 0/30
- **Twin Fang**: Deal [ModifiedBaseDamage] () magic damage to the current target and Wound them for 5 seconds. If they are already Wounded, deal [BonusDamage] () bonus magic damage. Wound: Reduce healing received by 33%

### Cho'Gath (1-cost, APTank)

- Traits: Void, Bruiser | mana 40/100
- **Feast**: Deal [TotalDamage] () magic damage to the lowest Health enemy in range. If this kills them, devour them and permanently gain 35 maximum Health (doubled in Hyper Roll). (Current Bonus: +[TFTUnitProperty.:TFT9_ChoGathBonusHealth] Health)

### Irelia (1-cost, APTank)

- Traits: Ionia, Challenger | mana 40/80
- **Defiant Dance**: Enter a defensive stance and gain [ModifiedShield] () Shield that rapidly decays over 3 seconds. When it expires, deal [ModifiedBaseStrikeDamage] () magic damage + 30% of the damage absorbed to enemies around and in front of Irelia. Ionia Bonus: +25 Armor and Magic Resist

### Jhin (1-cost, ADCarry)

- Traits: Ionia, Vanquisher | mana 44/114
- **Curtain Call**: Take aim at the current target and deal [TotalDamage] () physical damage to enemies in a line; each hit reduces this damage by 56%. Ionia Bonus: +25% Attack Damage

### Kayle (1-cost, APCarry)

- Traits: Demacia, Slayer
- **Divine Ascent**: Passive: Gains new Passive effects as your Tactician levels up. Level 1: Attacks deal [ModifiedMagicDamage] () bonus magic damage. Level 6: Every 3rd attack launches a wave that deals [AscensionModifiedMagicDamage] () magic damage and 20% Shreds enemies for 3 seconds. Level 9: Every attack launches a wave and they travel farther. Shred: Reduce Magic Resist

### Malzahar (1-cost, APCaster)

- Traits: Void, Sorcerer | mana 0/50
- **Call of the Void**: Open two portals near the current target. Destroy 50% of Shields and deal [ModifiedDamage] () magic damage to all enemies caught between the portals.

### Maokai (1-cost, APTank)

- Traits: Shadow Isles, Bastion | mana 50/125
- **Sap Magic**: Passive: Whenever an enemy uses their Ability, gain 5 Mana. Active: On next attack, heal for [ModifiedHeal] ().

### Orianna (1-cost, APCaster)

- Traits: Piltover, Sorcerer | mana 20/70
- **Command: Protect**: Grant the lowest Health ally [ModifiedShielding] () Shield for 4 seconds and empowers Orianna's next attack to deal [ModifiedDamage] () bonus magic damage.

### Poppy (1-cost, APTank)

- Traits: Demacia, Bastion | mana 30/90
- **Steadfast Hammer**: Gain [ModifiedShieldAmount] () Shield for 4 seconds. Deal [ModifiedMagicDamage] () magic damage to the current target.

### Renekton (1-cost, APTank)

- Traits: Shurima, Bruiser | mana 50/110
- **Cull the Meek**: Deal [ModifiedDamage] () magic damage to adjacent enemies. Heal [ModifiedInitialHeal] () for the first enemy hit and another [ModifiedKickerHeal] () for each additional one.

### Samira (1-cost, ADCarry)

- Traits: Noxus, Challenger | mana 0/30
- **Flair**: Shoot at the current target and deal [TotalDamage] () to the first enemy hit. Reduce their Armor by [ModifiedArmorShred] () for the rest of combat.

### Tristana (1-cost, ADCarry)

- Traits: Yordle, Gunner | mana 0/40
- **Rapid Fire**: Gain [ModifiedAttackSpeed] () Attack Speed for 4 seconds. For the duration, attacks explode on impact and deal [BonusDamage] () physical damage to enemies within 1 hex. 4-Star Upgrade: Every 10th attack deals [4StarShotDamage] () bonus physical damage and ricochets to the largest clump of enemies, dealing [ExplosionDamage] () physical damage to enemies within 1 hex.

### Viego (1-cost, APReaper)

- Traits: Shadow Isles, Rogue | mana 0/50
- **Blade of the Ruined King**: Deal [ModifiedStabDamage] () magic damage to the current target. For the rest of combat, Viego's attacks deal [ModifiedStackingDamage] () bonus stacking magic damage. (Current Bonus: +[TFTUnitProperty.:TFT9_ViegoBonusDamage] magic damage)

### Ashe (2-cost, ADCarry)

- Traits: Freljord, Vanquisher | mana 20/70
- **Volley**: Fire 8 arrows in a cone, each dealing [ArrowDamage] () physical damage to the first enemy hit and Chilling them for [ModifiedDuration] () seconds. Chill: Reduce Attack Speed by 30%

### Galio (2-cost, APTank)

- Traits: Demacia, Invoker | mana 60/110
- **Shield of Durand**: Reduce damage taken by 25/25/35% and heal for [ModifiedHeal] () over 2.5 seconds. After, deal [ModifiedDamage] () magic damage to adjacent enemies.

### Jinx (2-cost, ADCarry)

- Traits: Zaun, Gunner | mana 10/70
- **Fishbones!**: Fire 5 rockets at random enemies within 2 hexes of the current target. Each rocket deals [TotalDamage] () physical damage.

### Kassadin (2-cost, APTank)

- Traits: Void, Bastion | mana 50/100
- **Force Pulse**: Gain [ModifiedShield] () Shield for 4 seconds. Deal [ModifiedDamage] () magic damage to enemies in a cone and Disarm them for 1.5/1.75/2 seconds. Disarm: Cannot move or attack, but CAN cast spells

### Kled (2-cost, ADFighter)

- Traits: Noxus, Yordle, Slayer | mana 0/175
- **Skaaaaaaarl!**: Passive: Start combat with [Shield] () Shield. When it expires, dismount and gain [ModifiedAttackSpeedPercent] () stacking Attack Speed for the rest of combat. Active: Remount and gain [Shield] () Shield. 4-Star Upgrade: Execute enemies under 25% Health.

### Sett (2-cost, APTank)

- Traits: Ionia, Juggernaut | mana 60/110
- **Facebreaker**: Grab an enemy from either side and slam them together, knocking them up. Deals [ModifiedDamage] () magic damage and Stuns them for 1.25/1.5/2 seconds. If only one enemy is grabbed, the damage and Stun duration are increased by 50%. Ionia Bonus: +180 max Health

### Soraka (2-cost, APCaster)

- Traits: Targon, Invoker | mana 30/90
- **Astral Infusion**: Heal the lowest health ally for [ModifiedHeal] (). If the ally is below 50% Health, heal them for an additional [TooltipBonusHeal] (). Over the next 5 seconds, 5 stars hit the enemy closest to them. Each deals [ModifiedMagicDamage] () magic damage.

### Swain (2-cost, APTank)

- Traits: Noxus, Strategist, Sorcerer | mana 60/135
- **Demonflare**: Transform and gain [ModifiedBonusHealth] () max Health. While transformed, deal [ModifiedDamage] () magic damage to enemies within 2 hexes every second. If already transformed, gain an additional [ModifiedSecondaryBonusHealth] () max Health and deal [ModifiedSecondaryDamage] () magic damage to enemies within.

### Taliyah (2-cost, APCaster)

- Traits: Shurima, Multicaster | mana 20/60
- **Seismic Shove**: Passive: Whenever an enemy is knocked up or back by anything, throw a boulder towards them. It deals [ModifiedBoulderDamage] () magic damage to the first enemy hit. Active: Deal [ModifiedSpellDamage] () magic damage to the current target and knock them up, Stunning them for 2 seconds.

### Teemo (2-cost, APCaster)

- Traits: Yordle, Strategist, Multicaster | mana 0/50
- **Noxious Trap**: Hurl an explosive heat-seeking mushroom at the current target. When it detonates, enemies within 1 hex are Wounded and dealt [ModifiedDamage] () magic damage over 3 seconds. 4-Star Upgrade: Increase the explosion radius by 1 hex. Wound: Reduce healing received by 33%

### Vi (2-cost, ADTank)

- Traits: Piltover, Bruiser | mana 20/70
- **Blast Shield**: Gain [ModifiedShieldAmount] () Shield for 4 seconds. Deal [TotalDamage] () physical damage to enemies within 1 hex of the current target and 20% Sunder them for 4 seconds. Sunder: Reduce Armor

### Warwick (2-cost, ADTank)

- Traits: Zaun, Juggernaut, Challenger | mana 40/90
- **Primal Howl**: Passive: Attacks heal for [ModifiedHealAmount] (). Active: Gain 100% Attack Speed for 2.5 seconds. Then, Stun adjacent enemies for 1.25/1.5/2 seconds.

### Zed (2-cost, ADReaper)

- Traits: Ionia, Rogue, Slayer | mana 0/70
- **Living Shadow**: Create a shadow at the furthest enemy within [SpellRangeTooltip] () hexes. Zed and his shadow slash adjacent enemies, dealing [TotalDamage] () physical damage. Ionia Bonus: +15% Critical Strike Chance and Critical Strike Damage

### Akshan (3-cost, ADCarry)

- Traits: Shurima, Deadeye | mana 30/110
- **Comeuppance**: Lock on to the farthest enemy and unleash a hail of 6 shots toward them. Each deals [TotalDamage] () physical damage to the first enemy hit.

### Darius (3-cost, ADFighter)

- Traits: Noxus, Juggernaut, Vanquisher | mana 20/80
- **Noxian Guillotine**: Deal [TotalDamage] () physical damage to the current target. If they die, immediately cast this again with 90/95/100% damage.

### Ekko (3-cost, APReaper)

- Traits: Zaun, Piltover, Rogue | mana 0/45
- **Phase Dive**: Heal 20% of damage taken in the last 4 seconds and deal [ModifiedDamage] () magic damage to the current target.

### Garen (3-cost, ADFighter)

- Traits: Demacia, Juggernaut | mana 0/80
- **Judgement**: Spin [SpinsTooltip] () times every second for the next [ModifiedDuration] () seconds. Each spin deals [TotalDamage] () physical damage to adjacent enemies.

### Jayce (3-cost, ADCaster)

- Traits: Piltover, Gunner | mana 30/80
- **Acceleration Blast**: Grant [ModifiedEnhancedAttackSpeed] () Attack Speed to Jayce and [ModifiedPercentAttackSpeed] () to the allies on his left and right for 3 seconds. Fire a blast at the current target that explodes on the first enemy hit. It deals [TotalDamage] () physical damage to enemies near the blast.

### Kalista (3-cost, APCarry)

- Traits: Shadow Isles, Challenger | mana 0/80
- **Pierce and Rend**: Passive: Attacks impale a spear in the target, which deals [ModifiedDamage] () true damage when removed. Kalista rips the spears out if it would kill the target. Active: Impale 6 spears in the current target.

### Karma (3-cost, APCaster)

- Traits: Ionia, Invoker | mana 0/45
- **Inner Flame**: Fire a burst of energy that explodes upon impact, dealing [ModifiedDamage] () magic damage to adjacent enemies. Every third cast launches 3 bursts. Ionia Bonus: +25 Ability Power

### Katarina (3-cost, APReaper)

- Traits: Noxus, Rogue | mana 20/80
- **Voracity**: Throw three daggers that land next to enemies within [SpellRange] () hexes. Then, teleport to each and deal [ModifiedDamage] () magic damage to adjacent enemies and Wound them for 6 seconds. Wound: Reduce healing received by 33%

### Lissandra (3-cost, APCaster)

- Traits: Freljord, Invoker | mana 30/90
- **Frozen Tomb**: Stun the current target for 2 seconds and deal [ModifiedDamage] () magic damage to enemies within 2 hexes of them. They take 10% more damage for the next 4 seconds.

### Rek'Sai (3-cost, ADFighter)

- Traits: Void, Bruiser, Slayer | mana 50/75
- **Furious Bite**: Deal [TotalDamage] () physical damage to the current target and mark them. If they are below 66% max Health, deal true damage instead. If the target is marked, deal [MarkDamage] () bonus true damage. If this kills the target, rip through them 1 hex and heal for [HealAmount] ().

### Sona (3-cost, APCaster)

- Traits: Demacia, Multicaster | mana 35/80
- **Crescendo**: Send a wave of sound at the largest clump of enemies and deal [ModifiedDamage] () magic damage to enemies hit; each hit reduces this damage by 33%. Allies hit by the wave gain 20/25/35% Attack Speed for the rest of combat.

### Taric (3-cost, APTank)

- Traits: Targon, Bastion, Sorcerer | mana 15/85
- **Radiance**: Gain [ModifiedShieldAmount] () Shield for 4 seconds. It redirects 50% of the damage received by adjacent allies.

### Vel'Koz (3-cost, APCaster)

- Traits: Void, Multicaster, Sorcerer | mana 0/60
- **Plasma Fission**: Fire a plasma bolt towards the current target that deals [ModifiedMagicDamage] () magic damage to the first enemy hit. It then splits in two, dealing 50% less damage whenever it passes through an enemy.

### Aphelios (4-cost, ADCarry)

- Traits: Targon, Gunner | mana 50/100
- **Moonlight Vigil**: Fire a moon blast at the largest group of enemies that deals [TotalBlastDamage] () physical damage to enemies within 2 hexes. For 7 seconds, equip 3 Chakram, plus 1 more for each enemy hit by the blast. Attacks deal [TotalChakramDamage] () bonus physical damage for each Chakram equipped. Aphelios heals for [ModifiedChakramHealing] () of damage dealt by Chakrams.

### Azir (4-cost, APCarry)

- Traits: Shurima, Strategist | mana 40/50
- **Arise!**: Passive: Every third attack, a Sand Soldier deals [ModifiedMagicDamage] () magic damage to their target. Active: Summon a Sand Soldier to attack your target. If Azir already has 3 Soldiers, they all immediately deal [MaxSoldierStrike] () magic damage instead.

### Gwen (4-cost, APFighter)

- Traits: Shadow Isles, Slayer | mana 0/35
- **Snip Snip!**: Dash around the current target and snip 3 times in a cone, each dealing [ModifiedMagicDamage] () magic damage to enemies hit. Every 3 casts, gain 120/120/300 Armor and Magic Resist for 3/3/5 seconds.

### Jarvan IV (4-cost, APTank)

- Traits: Demacia, Strategist | mana 100/180
- **Cataclysm**: Leap on a nearby enemy. Deal [TotalDamage] () magic damage to enemies within 2 hexes and Stun them for 1.5/2/8 seconds.

### Kai'Sa (4-cost, APCarry)

- Traits: Void, Challenger | mana 60/120
- **Icathian Rain**: Dash away from all enemies, then fire 15/15/25 missiles split across the 4 nearest enemies. Each missile deals [TotalDamage] () magic damage.

### Lux (4-cost, APCaster)

- Traits: Demacia, Sorcerer | mana 0/40
- **Torrent of Light**: Channel a barrage of light at the current target that deals [ModifiedDamage] () magic damage over 3 seconds, reducing their Magic Resist by 15/15/40 every second. If they die, shift to a new target.

### Nasus (4-cost, ADTank)

- Traits: Shurima, Juggernaut | mana 70/150
- **Soul Eater**: Steal 4/4/15% maximum Health, 8% Attack Damage, 4/4/20 Armor, and 4/4/20 Magic Resist from the nearest 4/5/7 enemies for [ModifiedDuration] () seconds. While empowered, every third attack deals [EmpoweredDamage] () physical damage.

### Sejuani (4-cost, APTank)

- Traits: Freljord, Bruiser | mana 60/120
- **Fury of the North**: Passive: Whenever an ally attacks a Chilled enemy, they deal [HealthDamage] () bonus true damage. Active: Gain [ModifiedShield] () Shield for 4 seconds. Deal [ModifiedDamage] () magic damage to nearby enemies and Chill them for 4 seconds. Chill: Reduce Attack Speed by 30%

### Shen (4-cost, APTank)

- Traits: Ionia, Bastion, Invoker | mana 90/170
- **Ki Barrier**: Gain [ModifiedSelfShieldAmount] () Shield and grant [ModifiedAllyShieldAmount] () Shield to the 2 lowest-Health allies for 4 seconds. After shielding his allies, Shen's Shield refreshes with a burst, dealing [ModifiedDamage] () magic damage to adjacent enemies. Ionia Bonus: +8% Damage Reduction

### Urgot (4-cost, ADFighter)

- Traits: Zaun, Deadeye | mana 40/100
- **Disdain**: Passive: Whenever an enemy enters Attack Range, fire a blast from the leg facing that direction that deals [TotalDamage] () physical damage. Each of Urgot's 6 legs has a 4/4/1 second cooldown. Active: Gain [ModifiedShieldAmount] () Shield for 5 seconds and dash behind the current target. Reset all leg cooldowns.

### Yasuo (4-cost, ADFighter)

- Traits: Ionia, Challenger | mana 50/110
- **Last Breath**: Send a whirlwind at the furthest enemy within [SpellRangeTooltip] hexes, knocking up and Stunning all enemies hit. Dash to and slash the original target, dealing [TotalDamage] () physical damage to them. Slam them into the ground and deal [ModifiedAOEDamage] () physical damage to enemies within 1 hex. Ionia Bonus: +15% Omnivamp Omnivamp: Heal for percentage of damage dealt

### Zeri (4-cost, ADCarry)

- Traits: Zaun, Gunner | mana 0/50
- **Surge**: Passive: Execute enemies below [ModifiedExecuteThreshold] () Health. Active: For the next 8/8/15 seconds, attacks chain lightning to 3/3/5 additional enemies, dealing [TotalChainDamage] () physical damage.

### Aatrox (5-cost, ADFighter)

- Traits: Darkin, Slayer, Juggernaut | mana 0/50
- **World Ender**: Transform for 12/12/30 seconds, gaining [ModifiedOmnivamp] () Omnivamp and converting 80% of bonus Attack Speed to Attack Damage. While transformed, attacks deal [GreatswordDamage] () physical damage in an area. Omnivamp: Heal for percentage of damage dealt

### Ahri (5-cost, APCaster)

- Traits: Ionia, Sorcerer | mana 0/50
- **Essence Thief**: Steal essence from enemies around the current target, dealing [ModifiedEssenceDamage] () magic damage and 20% Mana Reaving them. Every [ActualBoomTooltip] casts, unleash a wave that deals [ModifiedMagicDamage] () magic damage to all enemies hit. This deals 33% more damage to enemies whose essence has been stolen. Ionia Bonus: +3 Mana per second Mana Reave: Increase max Mana until the next cast

### Bel'Veth (5-cost, ADFighter)

- Traits: Void, Empress | mana 20/70
- **Royal Maelstrom**: Lash out at the lowest Health enemy in range [ModifiedNumStrikes] () times. Each lash deals [TotalDamage] () physical damage and 2% of the target's max Health as true damage.

### Heimerdinger (5-cost, APCaster)

- Traits: Piltover, Technogenius | mana 80/160
- **CH-3X Lightning Grenade**: Hurl a grenade towards the largest group of enemies, dealing [ModifiedDamage] () magic damage to enemies in a large area and Stunning them for 1.5/2/15 seconds.

### K'Sante (5-cost, APTank)

- Traits: Shurima, Bastion | mana 0/70
- **All Out Strike**: Heal for [HealAmount] (), then knock up the current target before smashing them to the edge of the battlefield. Deal [ModifiedDamage] () magic damage and Stun them for 2/2.5/10 seconds. Deal [CollisionDamage] () magic damage to enemies the target collides with and Stun them for 1/1.25/5 seconds. If the target can't be pushed back any further, knock them off the battlefield. Otherwise, chase after them.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 0/999
- **Realm Warp: Bandle City**: Create a portal to Bandle City under the most expensive unit on your bench. They are summoned to the battlefield with [ModifiedBonusMana] () bonus Mana and are...slightly smaller? If there are no units left on your bench, Ryze summons a random high tier unit instead.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 50/100
- **Realm Warp: Demacia**: Create a portal to Demacia that protects allies for 2/2/5 seconds. Allies within the zone gain [ModifiedShield] () Shield. If an ally leaves the safety of the portal, they are healed for [ModifiedHeal] (). When the portal expires, enemies within are dealt [ModifiedDamage] () magic damage.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 30/65
- **Realm Warp: Bandle City**: Create a portal to Bandle City under the most expensive unit on your bench. They are summoned to the battlefield with [ModifiedBonusMana] () bonus Mana and are...slightly smaller? If there are no units left on your bench, Ryze summons a random high tier unit instead.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 15/60
- **Realm Warp: Freljord**: Create a portal to the Freljord under a random group of enemies for 2 seconds. The freezing winds deal [ModifiedDamage] () magic damage to enemies within two hexes over the duration and apply Permafrost, a permanent Chill. If enemies are already affected by Permafrost, they are instead Stunned for the duration. Chill: Reduce Attack Speed by 30%

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 30/100
- **Realm Warp: Ionia**: Create a portal to Ionia in the center of the battlefield, summoning forth a Spirit Tree that affects the 5/7/20 closest units. Allies gain 30/40/300% Attack Speed for 6 seconds and are healed for [ModifiedHeal] () over the duration. Enemies dance, dealing [ModifiedDamage] () magic damage and Stunning them for 2/2.5/20 seconds.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 40/100
- **Realm Warp: Noxus**: Create a portal to Noxus that spews out a torrent of weapons and armor, granting 3/4/10 completed items to random allies with open slots, and throwing 5/5/15 axes towards random enemies that deal [ModifiedDamage] () magic damage each. If an ally would recieve an item while all slots are full, they instead gain a stacking 10% damage increase for the rest of combat.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 15/75
- **Realm Warp: Piltover**: Create a portal to Piltover that launches an enforcer grenade towards the largest clump of enemies, dealing [ModifiedDamage] () magic damage to enemies within two hexes and creating a containment area for 5/5/10 seconds. Enemies are unable to leave the area, and 30/30/500% of the damage dealt to them is also dealt to all other enemies within.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 30/70
- **Realm Warp: Shadow Isles**: Mark the 1/1/10 ally's soul(s) with the lowest current health to be reclaimed by the Shadow Isles. After they die, they return to the battlefield with [ModifiedHealthOnReturn] () of their maximum Health. If all living allies are already marked, Ryze instead resurrects a deceased ally. Ryze cannot resurrect himself.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 60/120
- **Realm Warp: Shurima**: Create a portal to Shurima, summoning a roaming sand tornado that deals [ModifiedDamage] () magic damage to enemies hit and knocks them up, Stunning them for 1/1.25/3 second(s). The tornado has a chance to unearth buried treasures and gold as it travels.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 30/90
- **Realm Warp: Targon**: Create a portal to the skies above Mount Targon that beckons the current target. After a few seconds, they descend from the heavens as a meteor towards the largest group of enemies, dealing [ModifiedPercentMaxHealth] () of the initial target's maximum Health as magic damage. Enemies in the epicenter are briefly knocked up. If the target is the only enemy remaining, they never descend.

### Ryze (5-cost, APCaster)

- Traits: Wanderer, Invoker | mana 30/90
- **Realm Warp: Zaun**: Create 5/8/25 portals to Zaun, causing hired guns to fire at nearby enemies. Deals [ModifiedDamage] () magic damage and Wounds, 30% Shreds, and 30% Sunders for 6 seconds. The more gold you have, the more powerful the portals become. 25 Gold: +1 portal. Portals fire a rocket that deals 50% damage in a one-hex radius. 50 Gold: Double the portals. Summon 1 additional portals for every 20 Gold more. Wound: Reduce healing received by 50% Shred: Reduce Magic Resist Sunder: Reduce Armor

### Senna (5-cost, ADCaster)

- Traits: Shadow Isles, Gunner, Redeemer | mana 75/150
- **Dawning Shadow**: Fire a massive beam at the furthest enemy, dealing [TotalDamage] () physical damage to all enemies hit. Any allies hit gain [TotalShieldAmount] () Shield for 4/4/15 seconds.

### Sion (5-cost, ADTank)

- Traits: Noxus, Bruiser | mana 70/160
- **Glory in Death**: Passive: Upon death, reanimate with 100% Health, decaying at 20/13/0% maximum Health per second. Sion can no longer use his Ability, but is immune to crowd control and gains [ModifiedAttackSpeedPct] () Attack Speed. Active: Charge towards the most enemies within [DashRangeTooltip] () hexes. Deal [ModifiedDamage] () physical damage to any enemies along the way and knock them up, Stunning them for 1.25/1.5/15 seconds.

## Monsters, summons, pets and other trait-less units (21)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Sentinel (1-cost, )

- Traits: (none) | mana 30/100
- **Sentinel Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Target Dummy (1-cost, )

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Baron Nashor (8-cost, )

- Traits: (none) | mana 0/2
- **Corrosive Acid**: Passive: Cast every 2 attacks and cycle between 3 different Abilities: this, Voracious Bite (damage an enemy and heal), and Void Tentacle (damage and Stun a group). Active: Lob three pools of acid towards the largest group of enemies, dealing [ModifiedDamage] () magic damage and 40% Shredding and Sundering for 4 seconds. Shred: Reduce Magic Resist Sunder: Reduce Armor

### Baron Nashor (8-cost, )

- Traits: (none) | mana 0/2
- **Corrosive Acid**: Passive: Cast every [CastCadence] attacks and cycle between 3 different Abilities: this, Voracious Bite (damage an enemy and heal), and Void Tentacle (damage and Stun a group). Active: Lob three pools of acid towards the largest group of enemies, dealing [ModifiedDamage] () magic damage and [TooltipShred]% Shredding and Sundering for [DebuffDuration] seconds. Shred: Reduce Magic Resist Sunder: Reduce Armor

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Sun Disc (9-cost, )

- Traits: (none)
- **Ascension**: When reconstructed, the Sun Disc ascends select Shuriman.

### Apex Turret (11-cost, )

- Traits: (none) | mana 0/100
- **H-28G Laser Beam**: Fire a laser beam at the current target. Deal [ModifiedDamage] () magic damage to enemies it passes through.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Noxian Banner (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Rift Herald (11-cost, )

- Traits: (none) | mana 80/120
- **Void Charge**: Charge toward the furthest enemy and crash into the first enemy hit. Deal [ModifiedMagicDamage] () magic damage to enemies adjacent to the impact and Stun them for 2 seconds.

### Rift Herald (11-cost, )

- Traits: (none) | mana 60/120
- **Void Charge**: Charge toward the furthest enemy and crash into the first enemy hit. Deal [ModifiedMagicDamage] () magic damage to enemies adjacent to the impact and Stun them for [StunDuration] seconds.

### SLIME_Crab (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### T-Hex (11-cost, )

- Traits: (none) | mana 20/80
- **Powering Up**: T-Hex is still powering up and has no Ability. Once she reaches [PowerRequirement] Power, gain Hextech Breath. Hextech Breath: Unleash a huge laser dealing magic damage.

### Void Remora (11-cost, )

- Traits: (none) | mana 50/100
- **Void Shield**: Gain [ModifiedShield] () Shield for 2 seconds. When it expires, deal [ModifiedDamage] () magic damage to adjacent enemies.

### Void Remora (11-cost, )

- Traits: (none) | mana 50/100
- **Void Shield**: Gain [ModifiedShield] () Shield for [Duration] seconds. When it expires, deal [ModifiedDamage] () magic damage to adjacent enemies.

### Void Totem (11-cost, )

- Traits: (none)
- **Totem**: This totem is destroyed the first time it is attacked, but otherwise takes no damage.


<a id="set-10"></a>

# Set 10: Remix Rumble

Source: patch 15.10 data. 84 champions, 29 traits.

## Traits

### 8-bit

Keep score of your team's damage dealt. For each high score you beat, 8-bit champions gain Attack Damage. Bonus: [TFTUnitProperty.trait:TFT10_Trait_8bit_CurrentBonus]% ([TFTUnitProperty.trait:TFT10_Trait_8bit_StackCount]/[NumHighScores])

  - (2-3) AttackDamage=5, NumHighScores=10, 10000, 2500, 35000, 20000, 5000, 170000, 62500, 42500, 500, 125000, 10000, 1000, 345678, 85000, 85000, 225000, 55000, 27500, 199999, 112500, 5000, 17500
  - (4-5) AttackDamage=9, NumHighScores=10, 10000, 2500, 35000, 20000, 5000, 170000, 62500, 42500, 500, 125000, 10000, 1000, 345678, 85000, 85000, 225000, 55000, 27500, 199999, 112500, 5000, 17500
  - (6) AttackDamage=14, NumHighScores=10, 10000, 2500, 35000, 20000, 5000, 170000, 62500, 42500, 500, 125000, 10000, 1000, 345678, 85000, 85000, 225000, 55000, 27500, 199999, 112500, 5000, 17500

### Big Shot

Big Shots gain Attack Damage, which increases for [Duration] seconds when they use their Ability.

  - (2-3) Duration=3, PercentAD=0.1, PercentADEmpowered=0.35
  - (4-5) Duration=3, PercentAD=0.2, PercentADEmpowered=0.7
  - (6) Duration=3, PercentAD=0.4, PercentADEmpowered=1.5

### Breakout

(unique) Akali is a member of K/DA or True Damage depending on which trait has more fielded champions. She gains a different Ability depending on which form she takes.

  - (1) (no numbers)

### Bruiser

Your team gains [TeamFlatHealth] Health. Bruisers gain bonus max Health.

  - (2-3) BonusPercentHealth=0.2, TeamFlatHealth=100
  - (4-5) BonusPercentHealth=0.45, TeamFlatHealth=100
  - (6) BonusPercentHealth=0.9, TeamFlatHealth=100

### Country

When your team loses [HealthRatio]% of their Health, call a Dreadsteed that empowers your team. Each Country star level increases the Dreadsteed's Health and Attack Damage.

  - (3-4) BonusResists=None, HealthRatio=0.3, Level=1, 0.2
  - (5-6) BonusResists=30, HealthRatio=0.3, Level=2, 0.2
  - (7) BonusResists=60, HealthRatio=0.3, Level=3, 0.2

### Crowd Diver

After Crowd Divers die, they leap onto the furthest enemy, dealing [JumpDamage] magic damage. Enemies within [StunRange] hex are Stunned for [StunDuration] seconds. They also gain Damage Amp, increased by [BonusPerSecond]% each second.

  - (2-3) BaseBonus=0.12, BonusPerSecond=0.01, JumpDamage=300, StunDuration=1.5, StunRange=1
  - (4-5) BaseBonus=0.3, BonusPerSecond=0.01, JumpDamage=300, StunDuration=1.5, StunRange=1
  - (6) BaseBonus=0.45, BonusPerSecond=0.01, JumpDamage=300, StunDuration=1.5, StunRange=1

### Dazzler

Dazzlers' Abilities reduce their target's damage by [DamageDecrease]% and deal bonus magic damage over [Duration] seconds.

  - (2-3) DOTPercent=0.2, DamageDecrease=0.1, Duration=2
  - (4-5) DOTPercent=0.6, DamageDecrease=0.1, Duration=2
  - (6) DOTPercent=1.1, DamageDecrease=0.1, Duration=2

### Disco

Summon a movable Disco Ball. Combat start: Allies next to it gain Attack Speed and healing immediately and every [TickRate] seconds.

  - (3) AttackSpeed=0.05, Healing=0.02, HexRadius=1, TickRate=3, 1
  - (4) AttackSpeed=0.08, Healing=0.03, HexRadius=1, TickRate=3, 1
  - (5) AttackSpeed=0.12, Healing=0.04, HexRadius=1, TickRate=3, 2
  - (6) AttackSpeed=0.2, Healing=0.05, HexRadius=1, TickRate=3, 2

### EDM

Use the EDM selector item to choose an EDM champion and see the frequency for each. At the selected champion's frequency, your EDM champions cast the selected Ability with modified effectiveness.

  - (2) EDMstats=0.9, 3.5, 8, 2.5, 2, 8, 1, 8, 6
  - (3) EDMstats=1.1, 3.5, 8, 2.5, 2, 8, 1, 8, 6
  - (4) EDMstats=1.25, EDMtimereduction=1, 3.5, 8, 2.5, 2, 8, 1, 8, 6
  - (5) EDMstats=1.45, EDMtimereduction=1, 3.5, 8, 2.5, 2, 8, 1, 8, 6

### Edgelord

Edgelords gain Attack Speed, which doubles when their target drops below [HealthRatio]% Health. Edgelords with 1-hex range also dash through them on their next attack.

  - (3-4) ADRatio=2, AttackSpeed=0.3, HealthRatio=0.5
  - (5-6) ADRatio=2, AttackSpeed=0.55, HealthRatio=0.5
  - (7) ADRatio=2, AttackSpeed=0.9, HealthRatio=0.5

### Emo

Emo champions' Abilities cost less Mana to cast, and they gain Mana whenever an allied champion dies.

  - (2-3) InitialMana=20, StackingMana=20
  - (4-5) InitialMana=30, StackingMana=30
  - (6) BonusAP=15, InitialMana=40, StackingMana=35

### Executioner

Executioner Abilities can critically strike and they gain Critical Strike Damage. Their Critical Strike Chance is increased based on their target's missing Health.

  - (2-3) CRIT_DAMAGE=0.05, CRIT_PERCENT=0.25
  - (4-5) CRIT_DAMAGE=0.15, CRIT_PERCENT=0.8
  - (6) CRIT_DAMAGE=0.3, CRIT_PERCENT=1.6

### Guardian

Once per combat at [PercentHealthThreshold]% Health, Guardians shield themselves and their closest ally for a percent of their max Health.

  - (2-3) PercentHealthThreshold=50, ShieldPercentAmount=25
  - (4-5) PercentHealthThreshold=50, ShieldPercentAmount=45
  - (6) PercentHealthThreshold=50, ShieldPercentAmount=70

### Heartsteel

Earn Hearts by killing enemies. Gain even more by losing player combat. Every [RoundCount] player combats, convert Hearts into powerful rewards!

  - (3-4) HeartKeepPercentage=None, PercentHeartRate=1, RoundCount=4, RoundCount_HR=3, 25, 3, 1, 80, 30, 40, 25, 32, 10, 18, 1, 1, 10, None, 2, 20, 40, 1, 15, 4, 2, 0.5, 50, 1, 1, 1, 1, 1
  - (5-6) HeartKeepPercentage=None, PercentHeartRate=2, RoundCount=4, RoundCount_HR=3, 25, 3, 1, 80, 30, 40, 25, 32, 10, 18, 1, 1, 10, None, 2, 20, 40, 1, 15, 4, 2, 0.5, 50, 1, 1, 1, 1, 1
  - (7-9) HeartKeepPercentage=None, PercentHeartRate=5.5, RoundCount=4, RoundCount_HR=3, 25, 3, 1, 80, 30, 40, 25, 32, 10, 18, 1, 1, 10, None, 2, 20, 40, 1, 15, 4, 2, 0.5, 50, 1, 1, 1, 1, 1
  - (10) HeartKeepPercentage=0.8, PercentHeartRate=20, RoundCount=4, RoundCount_HR=3, 25, 3, 1, 80, 30, 40, 25, 32, 10, 18, 1, 1, 10, 1, 2, 20, 40, 1, 15, 4, 2, 0.5, 50, 1, 1, 1, 1, 1

### Hyperpop

When Hyperpop champions use an Ability, they grant Mana and [BuffDuration] seconds of Attack Speed to their [NumOfAllies] closest allies.

  - (1) AS=0.1, BuffDuration=4, Mana=5, NumOfAllies=2
  - (2) AS=0.2, BuffDuration=4, Mana=10, NumOfAllies=2
  - (3) AS=0.35, BuffDuration=4, Mana=15, NumOfAllies=2
  - (4) AS=0.6, BuffDuration=4, Mana=20, NumOfAllies=2

### ILLBEATS

(unique) Gain [1Star]/[2Star]/[3Star] placeable Spirit Tentacles, based on Illaoi's star level. Tentacles gain Illaoi's bonus Armor and Magic Resist.

  - (1) 1Star=2, 2Star=2, 3Star=8

### Jazz

For each active trait (except uniques), your team gains bonus Health and Damage Amp. Bonus Health: [TFTUnitProperty.trait:TFT10_Trait_Jazz_TraitTracker_CurrentBonus]% Damage Amp: [TFTUnitProperty.trait:TFT10_Trait_Jazz_TraitTracker_CurrentBonus_Damage]%

  - (2) BonusDamage=0.01, BonusHealth=0.015
  - (3) BonusDamage=0.015, BonusHealth=0.025
  - (4) BonusDamage=0.02, BonusHealth=0.04

### K/DA

Your team gains max Health, Ability Power, and Attack Damage if they are in a lighted hex. K/DA champions gain double!

  - (3-4) BonusAP=9, BonusMaxHealth=0.09, ManaPerSecond=None, 2
  - (5-6) BonusAP=15, BonusMaxHealth=0.15, ManaPerSecond=None, 2
  - (7-9) BonusAP=27, BonusMaxHealth=0.27, ManaPerSecond=None, 2
  - (10) BonusAP=45, BonusMaxHealth=0.45, ManaPerSecond=10, 2

### Maestro

(unique) The Maestro always attacks at a fixed pace, converting [BonusAttackSpeed]% bonus Attack Speed into [BonusAttackDamage]% Attack Damage.

  - (1) BonusAttackDamage=0.8, BonusAttackSpeed=1, ManaPerAttack=10, 0.9, 2

### Mixmaster

(unique) Choose a mode that changes the Mixmaster's attacks and Ability!

  - (1) (no numbers)

### Mosher

Moshers gain Attack Speed and Omnivamp, which increases up to [AmountIncrease]% based on their missing Health.

  - (2-3) AmountIncrease=1, FighterAS=20, FighterVamp=10
  - (4-5) AmountIncrease=1, FighterAS=40, FighterVamp=15
  - (6-7) AmountIncrease=1, FighterAS=80, FighterVamp=20
  - (8) AmountIncrease=1, FighterAS=135, FighterVamp=30

### Pentakill

Pentakill champions gain 15% Durability and Damage Amp. For each champion kill, a Pentakill champion rocks out, increasing its Damage Amp by [Kill_Bonus]%. On the [Kill_Count]th kill, all Pentakill champions rock out and your team gains [Attack_Speed]% Attack Speed.

  - (3-4) Attack_Speed=0.5, BONUS_DAMAGE=0.15, Kill_Bonus=0.25, Kill_Count=5, 0.15
  - (5-6) Attack_Speed=0.5, BONUS_DAMAGE=0.33, Kill_Bonus=0.25, Kill_Count=5, 0.15
  - (7-9) Attack_Speed=0.5, BONUS_DAMAGE=0.5, Kill_Bonus=0.25, Kill_Count=5, 0.15
  - (10) Attack_Speed=0.5, BONUS_DAMAGE=0.99, Kill_Bonus=0.25, Kill_Count=5, 0.5

### Punk

Punks gain bonus Health and Attack Damage, which increases by 1% every time you spend gold on a Shop reroll. After Punks fight in combat, gain a 1 gold Reroll for that round. Your first reroll each round grants [FIRST_ROLL_BONUS]% bonus instead! Current Bonus: [TFTUnitProperty.trait:TFT10_Punk_CurrentBonus]% [TFTUnitProperty.trait:TFT10_Punk_CurrentHealthBonus] and [TFTUnitProperty.trait:TFT10_Punk_CurrentADBonus]%

  - (2-3) BONUS_AD_AP=0.2, BONUS_HEALTH=200, FIRST_ROLL_BONUS=3, 1
  - (4-5) BONUS_AD_AP=0.35, BONUS_HEALTH=350, FIRST_ROLL_BONUS=3, 1
  - (6) BONUS_AD_AP=0.45, BONUS_HEALTH=450, FIRST_ROLL_BONUS=3, 1

### Rapidfire

Your team gains [TeamwideAttackSpeed]% Attack Speed. Rapidfire champions gain more on every attack, up to [MaxStacks] stacks.

  - (2-3) MaxStacks=10, PercentAS=0.04, TeamwideAttackSpeed=0.1
  - (4-5) MaxStacks=10, PercentAS=0.09, TeamwideAttackSpeed=0.1
  - (6) MaxStacks=10, PercentAS=0.16, TeamwideAttackSpeed=0.1

### Sentinel

Your team gains Armor and Magic Resist. Sentinels gain double.

  - (2-3) BonusArmor=16, 32
  - (4-5) BonusArmor=36, 72
  - (6-7) BonusArmor=60, 120
  - (8) BonusArmor=100, 200

### Spellweaver

Your team gains [TeamwideAP] Ability Power. Spellweavers gain more, plus extra Ability Power whenever a Spellweaver casts an Ability.

  - (3-4) AP=20, BonusAP=1, TeamwideAP=15
  - (5-6) AP=35, BonusAP=2, TeamwideAP=15
  - (7-8) AP=80, BonusAP=4, TeamwideAP=15
  - (9) AP=140, BonusAP=6, TeamwideAP=15

### Superfan

Superfans improve your Headliner!

  - (3) HealthBonus=300, OmnivampBonus=0.15
  - (4) HealthBonus=300, OmnivampBonus=0.15
  - (5) HealthBonus=300, OmnivampBonus=0.15

### True Damage

True Damage champions deal bonus true damage. If they are holding an item, they gain a unique Bling Bonus for their Ability.

  - (2-3) BLING_BONUS=1, BONUS_DAMAGE=0.15, None
  - (4-5) BLING_BONUS=1, BONUS_DAMAGE=0.33, None
  - (6-8) BLING_BONUS=1, BONUS_DAMAGE=0.5, None
  - (9) BLING_BONUS=1, BONUS_DAMAGE=0.99, 1

### Wildcard

(unique) If you win player combat, Kayn becomes the Shadow Assassin. If not, he becomes Rhaast. You receive a reward based on his form every time he kills [NumOfKills] enemy champions. Shadow Assassin: [AssassinGoldPerKill]g Rhaast: [RhaastPlayerHealthPerKill] player health

  - (1) AssassinGoldPerKill=5, NumOfKills=2, RhaastPlayerHealthPerKill=2

## Champions (62)

### Annie (1-cost, APCaster)

- Traits: Emo, Spellweaver | mana 0/50
- **Disintegrate**: Passive: After 4 casts, gain 40% Attack Speed and casts deal [ModifiedSecondaryDamage] () magic damage to another nearby enemy. Active: Deal [ModifiedMagicDamage] () magic damage to the current target. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Corki (1-cost, ADCarry)

- Traits: 8-bit, Big Shot | mana 15/60
- **Blown to 8 Bits**: Deal [TotalDamage] () physical damage to the current target and adjacent enemies, then Wound them for [ModifiedWoundDuration] () seconds. Wound: Reduce healing received by 33% [TFTUnitProperty.:TFT10_Headliner_TRA]

### Evelynn (1-cost, APReaper)

- Traits: K/DA, Crowd Diver | mana 30/75
- **Whiplash**: Deal [ModifiedMagicDamage] () magic damage to the current target. Gain 150% Attack Speed that decays over 4 seconds. While active, attacks restore 25/30/35 Health. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Jinx (1-cost, ADCarry)

- Traits: Punk, Rapidfire | mana 0/50
- **Escalation**: Passive Minigun: Attacks grant [TOTAL_ATTACK_SPEED] () Attack Speed. Passive Rocket Launcher: Attacks deal [ROCKET_DAMAGE] () bonus physical damage. Active: Swap between Minigun and Rocket Launcher. [TFTUnitProperty.:TFT10_Headliner_TRA]

### K'Sante (1-cost, ADTank)

- Traits: Heartsteel, Sentinel | mana 0/40
- **Block the Haters**: Enter a defensive stance and gain [ModifiedDR] () Durability for 2.5 seconds. Afterwards, deal [TotalDamage] () physical damage to the current target. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Kennen (1-cost, APTank)

- Traits: True Damage, Superfan, Guardian | mana 30/90
- **Shock and Awe**: Discharge 2 Jolts over 2 seconds. Each Jolt deals [ModifiedDamage] () magic damage to a random enemy within range and Stuns them for 0.75 seconds. Bling Bonus: Discharge at 2nd target for [TFTUnitProperty.:TFT10_KennenPercentage]%[TFTUnitProperty.:TFT10_KennenPercentage]% damage and Stun duration. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Lillia (1-cost, APTank)

- Traits: K/DA, Superfan, Sentinel | mana 60/110
- **Confetti Bloom**: Deal [ModifiedMagicDamage] () magic damage to adjacent enemies. Heal Lillia for [ModifiedHealing] () and her nearest ally for [ModifiedAllyHealing] (). [TFTUnitProperty.:TFT10_Headliner_TRA]

### Nami (1-cost, APCasterHighMana)

- Traits: Disco, Dazzler | mana 15/60
- **Disco Prison**: Deal [TOTAL_DAMAGE] () magic damage to the current target and Stun them for 1.25 seconds. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Olaf (1-cost, ADFighter)

- Traits: Pentakill, Bruiser
- **Berserker Rage**: Passive: Heal [ModifiedHealValue] () Health on attack. For every 1% missing Health, gain [ModifiedASValue]% () Attack Speed. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Tahm Kench (1-cost, APTank)

- Traits: Country, Bruiser
- **Rawhide**: Passive: Reduce each instance of incoming damage by [ModifiedDR] (). [TFTUnitProperty.:TFT10_Headliner_TRA]

### Taric (1-cost, APTank)

- Traits: Disco, Guardian | mana 50/140
- **Mirrorball's Blessing**: Gain a [SHIELD_TOTAL] () Shield for 4 seconds. Taric's next 2 attacks deal [BONUS_DAMAGE] () bonus magic damage. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Vi (1-cost, ADFighter)

- Traits: Punk, Mosher | mana 40/70
- **The Harder They Fall**: Deal [PUNCH_DAMAGE] () physical damage to the current target, or [EMPOWERED_PUNCH_DAMAGE] () if they have more current Health than Vi. Stun them for 1.5/1.75/2 seconds and reduce their Armor by [ModifiedArmorReduction] () for the rest of combat. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Yasuo (1-cost, ADFighter)

- Traits: True Damage, Edgelord | mana 0/50
- **Synthesizer Strike**: Passive: Yasuo permanently gains 1.5% Attack Damage when he kills an enemy champion (doubled in Hyper Roll). (Current: [TFTUnitProperty.:TFT10_YasuoADGain]% ) Active: Deal [TOTAL_DAMAGE] () physical damage to the current target. Bling Bonus: Synthesizer Strike executes enemies under [TFTUnitProperty.:TFT10_YasuoExecute]%[TFTUnitProperty.:TFT10_YasuoExecute]% Health [TFTUnitProperty.:TFT10_Headliner_TRA]

### Aphelios (2-cost, ADCaster)

- Traits: Heartsteel, Rapidfire | mana 40/110
- **Moonlight Lullaby**: Deal [ModifiedPhysicalDamage] () physical damage and Stun the current target for 1.5/1.5/2 seconds. Deal [ModifiedAOEDamage] () physical damage to enemies within 1 hex. If the original target dies, Stun enemies within 1 hex for 1 second. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Bard (2-cost, APCarry)

- Traits: Jazz, Dazzler | mana 15/60
- **Improv**: Play a 4-note tune at random using 3 possible notes: Doot - Deal [ModifiedSingleTarDmg] () magic damage to the current target. Chime - Heal the lowest Health ally for [ModifiedHeal] () Health. Tip - Drop 1 gold and play another note. (3 Gold in Hyper Roll) [TFTUnitProperty.:TFT10_Headliner_TRA]

### Garen (2-cost, ADTank)

- Traits: 8-bit, Sentinel | mana 40/80
- **Power-Up!**: Gain [ModifiedMaxHealthHealing] () max Health. Garen's next attack deals [BaseDamage] () physical damage. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Gnar (2-cost, ADFighter)

- Traits: Pentakill, Superfan, Mosher | mana 0/70
- **Rabid Fandom**: Leap over the current target and transform into Mega Gnar for the rest of combat, gaining [ModifiedHealthGain] () Health and 70% Attack Damage. Subsequent casts deal [TotalDamage] () physical damage to the current target. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Gragas (2-cost, APTank)

- Traits: Disco, Spellweaver, Bruiser | mana 40/80
- **Boogie Hour**: Heal 500/625/750 Health over 2 seconds. Then, deal [TOTAL_DAMAGE] () magic damage to adjacent enemies and Chill them for 3 seconds. Chill: Reduce Attack Speed by 30% [TFTUnitProperty.:TFT10_Headliner_TRA]

### Jax (2-cost, APFighter)

- Traits: EDM, Mosher | mana 50/120
- **Counter Melody**: Leap at the highest Health enemy within [ModifiedHexRadius] () hexes and deal [ModifiedSlamDamage] () magic damage to them. Then, deal [ModifiedSpinDamage] () magic damage to all adjacent enemies. Gain 10% Attack Damage and Ability Power for the rest of combat. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Kai'Sa (2-cost, ADCarry)

- Traits: K/DA, Big Shot | mana 0/30
- **Got the Boom**: Dash up to 2 hexes and fire a missile at the current target. It deals [TooltipDamage] () physical damage to the first enemy hit. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Katarina (2-cost, APReaper)

- Traits: Country, Crowd Diver | mana 0/60
- **Bouncing Blade**: Throw a blade at the current target that bounces 3 times, dealing [ModifiedMagicDamage] () magic damage and applying Wound for 6 seconds. Wound: Reduce healing received by 33% [TFTUnitProperty.:TFT10_Headliner_TRA]

### Kayle (2-cost, APCarry)

- Traits: Pentakill, Edgelord | mana 0/120
- **Fires of Ascension**: Passive: Attacks deal [ModifiedAttackDamage] () bonus magic damage in waves at the current target and behind them. Active: Deal [ModifiedFinaleDamage] () magic damage to enemies around the target. Shred enemies hit for 4 seconds. Shred: Reduce Magic Resist by 20% [TFTUnitProperty.:TFT10_Headliner_TRA]

### Pantheon (2-cost, ADTank)

- Traits: Punk, Guardian | mana 0/55
- **Too Tough To Kill**: Gain [DR_TOTAL] () Durability for 3 seconds. Afterwards, deal [TOTAL_DAMAGE] () physical damage to the 3 enemies who dealt the most damage to Pantheon during the duration. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Senna (2-cost, APCasterHighMana)

- Traits: True Damage, Rapidfire | mana 30/90
- **Concussive Noise**: Fire a blast of sound at the current target's location. It pulses 3 times, each dealing [TOTAL_DAMAGE] () magic damage to enemies within 1 hex. Bling Bonus: -[TFTUnitProperty.:TFT10_SennaMana][TFTUnitProperty.:TFT10_SennaMana] max Mana [TFTUnitProperty.:TFT10_Headliner_TRA]

### Seraphine (2-cost, APCaster)

- Traits: K/DA, Spellweaver | mana 0/60
- **High Note**: Send a high note to the largest clump of units that hits all units within 1 hex. Enemies take [ModifiedDamage] () magic damage. Allies heal for [ModifiedHeal] () Health. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Twitch (2-cost, ADCaster)

- Traits: Punk, Executioner | mana 20/70
- **Bottled Anarchy**: Throw a bottle at the current target, which deals [ModifiedInitialDamage] () physical damage to enemies within 1 hex. It explodes into 4 shards; each dealing [ModifiedSecondaryDamage] () magic damage to a random enemy within 2 hexes. Each enemy critically struck by the bottle creates 1 more shard. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Amumu (3-cost, APTank)

- Traits: Emo, Guardian | mana 0/45
- **Tantrum**: Passive: When attacked, gain 4 Armor (stacks up to 25 times). Active: Deal [TotalDamage] () magic damage to adjacent enemies. Every 3rd cast hits enemies within 2 hexes and Stuns them for 1.5 seconds. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Ekko (3-cost, APTank)

- Traits: True Damage, Spellweaver, Sentinel | mana 50/125
- **Record Scratch**: Deal [TOTAL_DAMAGE] () magic damage to enemies within 2 hexes and Stun them for 1 seconds. Gain a [SHIELD_TOTAL] () Health shield for 4 seconds. Bling Bonus: While the shield is up, heal [TFTUnitProperty.:TFT10_EkkoHealPerSecond][TFTUnitProperty.:TFT10_EkkoHealPerSecond] every second. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Lulu (3-cost, APCaster)

- Traits: Hyperpop, Spellweaver | mana 0/40
- **Tastes like Glitter**: Fire a bolt toward the current target. It deals [ModifiedMagicDamage] () magic damage to the first enemy it passes through and [ModifiedSecondaryDamage] () magic damage to the second. Every 3rd cast, Stun the 3 nearest enemies for 1.25 seconds and deal [ModifiedBigCastDamage] () magic damage instead. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Lux (3-cost, APCasterHighMana)

- Traits: EDM, Dazzler | mana 15/75
- **Laser Light Show**: Fire a beam at the farthest enemy, dealing [ModifiedMagicDamage] () magic damage to all enemies it hits. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Miss Fortune (3-cost, ADCaster)

- Traits: Jazz, Big Shot | mana 0/60
- **Double Time**: Deal [PrimaryTotalDamage] () physical damage to the current target and [SecondaryTotalDamage] () physical damage to the closest enemy behind them. If either die, gain [ScaledASBuff]% () Attack Speed for 4 seconds. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Mordekaiser (3-cost, APTank)

- Traits: Pentakill, Sentinel | mana 70/120
- **Face-Melter**: Gain a [ModifiedShieldAmount] () Shield and deal [ModifiedZoneDamage] () magic damage to adjacent enemies over 3 seconds. Afterwards, deal [ModifiedFinaleDamage] () magic damage to nearby enemies. When Face-Melter kills an enemy, gain 4% Ability Power and Attack Damage, and 4 Armor and Magic Resist. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Neeko (3-cost, APTank)

- Traits: K/DA, Superfan, Guardian | mana 0/90
- **Cosplay**: Cosplay the highest Health ally and gain a [ModifiedShield] () Shield + 8% of the ally's Health for 4 seconds. When it breaks, deal 90/110/140% of its initial value as magic damage to adjacent enemies. [TFTUnitProperty.unit:TFT10_Headliner_TRA]

### Riven (3-cost, ADFighter)

- Traits: 8-bit, Edgelord | mana 50/100
- **Voxel Defense**: For the next 6 seconds, gain a [ModifiedShield] () Shield and attacks deal [ModifiedSplashDamage] () bonus physical damage to her target and enemies within 1 hex of the target. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Samira (3-cost, ADCarryCrit)

- Traits: Country, Executioner | mana 0/100
- **Thrills & Kills**: Passive: Attacks that critically strike grant a stack of Style, up to 6 stacks. Each stack grants [ModifiedAS] () Attack Speed. Active: Deal [BaseADDamage] () physical damage to the current target, then another [ADDamagePerStack] () physical damage per Style stack. Afterwards, reset Style. [TFTUnitProperty.unit:TFT10_Headliner_TRA]

### Sett (3-cost, APTank)

- Traits: Heartsteel, Bruiser, Mosher | mana 30/90
- **THE PUNCHLINE**: Gain a 375/425/475 Shield for 2 seconds, increased by 1 for every 4 missing Health. Deal [TotalDamage] () magic damage to the current target and [TotalSecondaryDamage] () magic damage to enemies in a cone around them. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Urgot (3-cost, ADFighter)

- Traits: Country, Mosher | mana 0/20
- **Fire from his Fingertips**: For 8 seconds, convert bonus Attack Speed to Attack Damage. Attacks deal [TotalDamage] () physical damage in a cone and grant Urgot 20% of the damage dealt as a 5 second Shield. If Urgot only hits one target, deal 50% bonus damage. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Vex (3-cost, APCaster)

- Traits: Emo, Executioner | mana 25/75
- **Your Funeral**: Send a shadow to the current target that Stuns enemies within 1 hex for 0.75 seconds. Then, it deals [ModifiedDamage] () magic damage to enemies within 1 hex. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Yone (3-cost, ADFighter)

- Traits: Heartsteel, Edgelord, Crowd Diver | mana 0/70
- **Pop Off**: Gain stacking Move Speed and [StackingOmnivamp]% () Omnivamp for the rest of combat. Slash twice, dealing [ModifiedMagicDamage] () physical damage to enemies in a cone each time. Omnivamp: Heal for percentage of damage dealt [TFTUnitProperty.:TFT10_Headliner_TRA]

### Ahri (4-cost, APCaster)

- Traits: K/DA, Spellweaver | mana 0/30
- **POP/STAR**: Blow a kiss at the current target. It deals [modifieddamage] () magic damage and briefly Stuns them. If the target has been kissed before, deal [bonusKissDamage] () magic damage instead. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Akali (4-cost, ADReaper)

- Traits: True Damage, Executioner, Breakout | mana 0/60
- **Three Point Strike**: Deal [TOTAL_DAMAGE] () physical damage three times split between the three closest enemies within three hexes. For each enemy that survives all three waves, refund [TOTAL_MANA_REFUND] () Mana. Bling Bonus: Heal for [TFTUnitProperty.:TFT10_AkaliOmnivamp]%[TFTUnitProperty.:TFT10_AkaliOmnivamp]% of the damage dealt with Three Point Strike. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Akali (4-cost, ADReaper)

- Traits: K/DA, Executioner, Breakout | mana 10/50
- **The Baddest**: Throw a shuriken at the farthest unmarked target, marking them. Dash to every marked enemy, striking them for [Total_Strike_Damage] () physical damage and dealing [Total_Dash_Damage] () physical damage to enemies dashed through. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Blitzcrank (4-cost, APTank)

- Traits: Disco, Sentinel | mana 50/110
- **Boogie Barrier**: Passive: Deal [ZapDamage] () magic damage to a nearby enemy every 2 seconds. Active: Gain a [ShieldAmount] () Shield. For 5 seconds, Blitzcrank's Passive deals damage every second and deals an additional 1% of the target's max Health as magic damage. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Caitlyn (4-cost, ADCaster)

- Traits: 8-bit, Rapidfire | mana 0/75
- **Champ Hunt**: Fire shots at the 4 furthest enemies. Shots deal [TOTAL_DAMAGE] () physical damage to the first enemy they hit. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Ezreal (4-cost, ADCarry)

- Traits: Heartsteel, Big Shot | mana 0/45
- **Crash the Party**: Blink away from the current target and deal [TotalDamage] () physical damage to them. Every 3rd cast, deal [ModifiedBigDamage] () physical damage to all enemies in a line. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Karthus (4-cost, APCasterHighMana)

- Traits: Pentakill, Executioner | mana 15/105
- **Mortal Reminder**: Passive: Gain Ability Power based on Critical Strike Chance. (Current: [TotalBonusAP]% ()) Active: Deal [ModifiedDamage] () magic damage to the 4/4/6 lowest Health enemies, always critically striking if possible. Gain 40% Ability Power for the rest of combat. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Poppy (4-cost, ADTank)

- Traits: Emo, Mosher | mana 30/100
- **Hammer Time**: Passive: Gain Attack Damage based on bonus Health. (Current: [TFTUnitProperty.unit:TFT10_Poppy_HealthBonus]% ()) Active: Gain 100 Armor and Magic Resistance for 6 seconds. Hammer the nearest enemy 3 times; each deals [TotalDamage] () physical damage and heals [TotalHealing] () Health. If this kills an enemy, slam an additional time. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Thresh (4-cost, APTank)

- Traits: Country, Guardian | mana 70/160
- **Devil's Round Up**: Stun the largest group of nearby enemies for 1.75/2/8 seconds and deal [ModifiedDamage] () magic damage to each. Thresh heals for 250/300/600 + 50% of the total damage dealt. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Twisted Fate (4-cost, APCarry)

- Traits: Disco, Dazzler | mana 30/100
- **Hustle & Shuffle**: Throw [ModifiedCardCount] () cards divided between the current target and 3 nearest enemies. They reduce Magic Resist by 1 and deal [ModifiedDamage] () magic damage. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Viego (4-cost, ADFighter)

- Traits: Pentakill, Edgelord | mana 70/120
- **Riff of the Ruined King**: Slam on a nearby hex. Deal [ModifiedSlamDamage] () physical damage to enemies within two hexes and mark them for [ModifiedMarkDuration] () seconds. Marked enemies take 10% increased damage from all sources. Viego's attacks on marked enemies deal [ModifiedBonusDamage] () physical damage instead. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Zac (4-cost, APTank)

- Traits: EDM, Bruiser | mana 60/120
- **Let's Bounce!**: Bounce 3 times on nearby enemies. Each bounce deals [ModifiedMagicDamage] () magic damage, Stuns for 1 second, and heals Zac for [ModifiedHealing] () Health. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Zed (4-cost, ADReaper)

- Traits: EDM, Crowd Diver | mana 30/50
- **Shadow Dance**: Mark the current target and spawn an untargetable Shadow with [FinalAttackDamage] () Attack Damage for 4 seconds. After a brief delay or when the marked enemy falls below [ModifiedHealthThreshold]% () health, deal an additional [TotalMarkDamage] () physical damage. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Illaoi (5-cost, APTank)

- Traits: ILLBEATS, Bruiser | mana 70/150
- **Drums of the Deep**: Passive: Every 3 seconds, Illaoi's tentacles deal [ModifiedTentacleDamage] () magic damage to nearby enemies. Active: Gain 90/135/400 Armor and Magic Resist for 3.5 seconds. Revive or fully heal the lowest Health tentacle. Leap at the nearest enemy and deal [TooltipDamage] () magic damage in a large circle 3/3/7 times as tentacles slam in unison. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Jhin (5-cost, ADCaster)

- Traits: Maestro, Big Shot | mana 30/50
- **Concerto of Demise in D Minor**: Passive: Attacks generate 10 additional Mana. If your bench has 4 Grand Finale Rifles, begin conducting instead of attacking. Each Rifle fires at the same rate as the Maestro and deals [TOTAL_DAMAGE] () physical damage. Every 4th volley deals 200% damage. Active: Put 1/1/4 Grand Finale Rifle into one of your empty bench slots. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Kayn (5-cost, APReaper)

- Traits: Heartsteel, Edgelord, Wildcard | mana 0/40
- **Fear the Reaper**: Dash, then deal [ModifiedDamage] () magic damage to adjacent enemies and Chill them for 3 seconds. If Kayn only hits one target, he immediately casts again. Chill: Reduce Attack Speed by 20% [TFTUnitProperty.:TFT10_Headliner_TRA]

### Lucian (5-cost, ADCarry)

- Traits: Jazz, Rapidfire | mana 50/125
- **Arpeggio**: Fire [TotalNumShots] () shots toward the furthest enemy. Each shot explodes on the first enemy hit, dealing [TOTAL_DAMAGE] () physical damage to all nearby enemies and reducing their Armor by 3/5/20 for the rest of combat. If Arpeggio ends early, gain Mana based on the number of unused shots. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Qiyana (5-cost, ADReaper)

- Traits: True Damage, Crowd Diver | mana 0/40
- **Sample & Remix**: Copy 1/1/3 of the current target's items and throw them to an ally. Deal [TOTAL_DAMAGE] () physical damage and knock them back. If the target had no items, deal [TD_TOTAL_DAMAGE] () additional true damage and knock them back to the edge of the board instead. Bling Bonus: After Sample & Remix kills a champion and copies an item, create [TFTUnitProperty.:TFT10_QiyanaComponentCount] permanent component(s). [Radiant items, Artifacts, and Emblems cannot be copied] [TFTUnitProperty.:TFT10_Headliner_TRA]

### Sona (5-cost, APCarryHighMana)

- Traits: Mixmaster, Spellweaver | mana 0/160
- **The Drop (Kinetic)**: Passive: Attacks instead send a beat to an ally, healing them for 6/9/100% of their maximum Health. Active: Send a beat to all allies and grant them a [ModifiedShieldAmount] () Shield for 2.5/2.5/30 seconds. [TFTUnitProperty.:TFT10_Headliner_TRA]

### Yorick (5-cost, ADFighter)

- Traits: Pentakill, Mosher, Guardian | mana 60/110
- **GET IN THIS PIT! (ft. Ghouls)**: Summon 9/10/25 headbanging ghouls that pile towards the center of the board. Each deals [TOTAL_DAMAGE] () physical damage over three attacks. Ghouls' damage 20% Sunders for 3 seconds. Every other cast also summons a BIG ghoul with [BigZombieModifiedHealth] () Health that deals [BigZombieModifiedDamage] () physical damage each attack. Sunder: Reduce Armor by 20% [TFTUnitProperty.:TFT10_Headliner_TRA]

### Ziggs (5-cost, APCarry)

- Traits: Hyperpop, Dazzler | mana 30/75
- **Chaos Theory**: Throw a bomb at the current target that deals [ModifiedMagicDamage] () magic damage. It splits into [TFTUnitProperty.:TFT10_ZiggsSmallBombs] smaller bombs that Shred their target for 4 seconds and deal [ModifiedBombDamage] () magic damage. Each cast increases the number of smaller bombs by 1. Shred: Reduce Magic Resist by 30% [TFTUnitProperty.:TFT10_Headliner_TRA]

### Disco Ball (11-cost, )

- Traits: Disco
- **Disco Ball**: Disco Ball likes disco music.

### Yorick (11-cost, )

- Traits: Pentakill
- **GET IN THIS PIT! (ft. Ghouls)**: Summon [ZOMBIE_COUNT] headbanging ghouls that pile towards the center of the board. Each deals [TOTAL_DAMAGE] () physical damage over three attacks. Ghouls' damage [ArmorShredPercent]% Sunders for [ArmorShredDuration] seconds. Every other cast also summons a BIG ghoul with [BigZombieModifiedHealth] () Health that deals [BigZombieModifiedDamage] () physical damage each attack. Sunder: Reduce Armor by 20% [TFTUnitProperty.:TFT10_Headliner_TRA]

## Monsters, summons, pets and other trait-less units (22)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, )

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, )

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, )

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, )

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Training Dummy (1-cost, )

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Tentacle (9-cost, )

- Traits: (none)
- **Rhythm of an Elder God**: Cannot move. Slam every 3 seconds at targets in range for [TFTUnitProperty.trait:TFT10_Trait_ILLBEATS_TentacleDamage] () magic damage. If Illaoi is casting her Ability, slam in unison with her drums instead.

### Hacked Egg (11-cost, )

- Traits: (none)
- **Hacked Egg**: This egg will soon hatch into loot! LOSSES accelerate the hatch timer by an additional turn.

### LuluPolymorphCritter (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### SightWard (11-cost, )

- Traits: (none) | mana 0/100
- **Heartsteel Tour Case**: Raised the Stakes: Earn [TFTUnitProperty.:TFT10_PBJ_OnFireBonusHearts]x more hearts, but if you win a fight, lose [TFTUnitProperty.:TFT10_PBJ_OnFireWinPenalty]% of your hearts and cash out immediately. The final cashout also gains [TFTUnitProperty.:TFT10_PBJ_OnFireCompletionHearts] hearts! Hearts per kill: [TFTUnitProperty.:TFT10_PBJ_HeartsPerKill] Hearts per loss: [TFTUnitProperty.:TFT10_PBJ_HeartsPerLoss] Player combats until rewards: [TFTUnitProperty.:TFT10_PBJ_RoundsUntilCashout] Estimated Value: [TFTUnitProperty.:TFT10_PBJ_CurrentCashoutLevel] Later stages grant more Hearts!

### The Dreadsteed (11-cost, )

- Traits: (none) | mana 50/60
- **Onslaught of Shadows**: Hecarim summons spectral riders that charge through his target, dealing [ModifiedDamage] magic damage and stunning enemies hit for [StunDuration] seconds.


<a id="set-11"></a>

# Set 11: Inkborn Fables

Source: patch 14.24 data. 87 champions, 28 traits.

## Traits

### Altruist

Altruists heal the lowest Health ally for [PercentDamageToHeal]% of damage they deal. Your team gains Armor and Magic Resist.

  - (2) PercentDamageToHeal=0.15, Resists=10
  - (3) PercentDamageToHeal=0.15, Resists=20
  - (4) PercentDamageToHeal=0.15, Resists=40

### Arcanist

Arcanists gain Ability Power and grant Ability Power to allies.

  - (2-3) ArcanistAbilityPower=20, TeamAbilityPower=20
  - (4-5) ArcanistAbilityPower=50, TeamAbilityPower=20
  - (6-7) ArcanistAbilityPower=90, TeamAbilityPower=50
  - (8) ArcanistAbilityPower=135, TeamAbilityPower=135

### Artist

The Artist paints the champion you place in a special bench slot. Get a 1-star copy of the champion placed there when the Artist's work is complete. Rounds to Complete = Unit Cost

  - (1) ExtraTurns=None

### Behemoth

Behemoths gain increased Armor and Magic Resist. Whenever a Behemoth dies, the nearest Behemoth gains [BonusPercent]% more for [Duration] seconds.

  - (2-3) BonusPercent=0.5, Defenses=25, Duration=8, 1
  - (4-5) BonusPercent=0.5, Defenses=55, Duration=8, 1
  - (6-7) BonusPercent=0.5, Defenses=85, Duration=8, 1
  - (8) BonusPercent=0.5, Defenses=115, Duration=8, 1

### Bruiser

Your team gains [TeamBonusHealth] maximum Health. Bruisers gain additional maximum Health.

  - (2-3) BonusPercentHealth=0.2, RefreshSeconds=3, SlamPercentHealth=None, TeamBonusHealth=100
  - (4-5) BonusPercentHealth=0.4, RefreshSeconds=3, SlamPercentHealth=None, TeamBonusHealth=100
  - (6-7) BonusPercentHealth=0.65, RefreshSeconds=3, SlamPercentHealth=None, TeamBonusHealth=100
  - (8) BonusPercentHealth=0.8, RefreshSeconds=3, SlamPercentHealth=0.06, TeamBonusHealth=100

### Dragonlord

After [FirstDelay] seconds of combat, the Dragon strikes the board, dealing true damage to enemies and granting all allies Attack Speed for the rest of combat.

  - (2) ASDuration=60, AttackSpeedBuff=0.12, FirstDelay=8, TrueDamageHPRatio=0.05, 1
  - (3) ASDuration=60, AttackSpeedBuff=0.18, FirstDelay=8, StunDuration=1.25, TrueDamageHPRatio=0.1, 2
  - (4) ASDuration=60, AttackSpeedBuff=0.18, FirstDelay=8, StunDuration=1.5, TrueDamageHPRatio=0.1, 3
  - (5) ASDuration=60, AttackSpeedBuff=0.45, FirstDelay=8, StunDuration=1.5, TrueDamageHPRatio=0.15, 3

### Dryad

Dryads gain Ability Power and [StartingHealth] Health. Each enemy death grants permanent additional Health. (Total health: [TFTUnitProperty.trait:TFT11_Trait_DryadHealth] ) (Enemies killed: [TFTUnitProperty.trait:TFT11_Trait_DryadCompost])

  - (2-3) AP=15, Duration=4, HealthPerCompost=3, HealthPerCompostHR=4, StartingHealth=150, StartingHealthHR=150
  - (4-5) AP=30, Duration=4, HealthPerCompost=7, HealthPerCompostHR=12, StartingHealth=150, StartingHealthHR=150
  - (6) AP=55, Duration=4, HealthPerCompost=10, HealthPerCompostHR=20, StartingHealth=150, StartingHealthHR=150

### Duelist

Duelists gain Attack Speed on each attack, stacking up to [MaxStacks] times.

  - (2-3) AttackSpeedPercent=0.05, DamageReduction=None, MaxStacks=12
  - (4-5) AttackSpeedPercent=0.09, DamageReduction=None, MaxStacks=12
  - (6-7) AttackSpeedPercent=0.13, DamageReduction=0.12, MaxStacks=12
  - (8) AttackSpeedPercent=0.18, DamageReduction=0.18, MaxStacks=12

### Exalted

Your team gains [BaseDamageAmp]% bonus damage, plus more based on your level. After combat, store XP in a Soul Core. Sell the Core to claim the XP. (Current bonus: [TFTUnitProperty.trait:TFT11_Trait_Exalted_CurrentBonus]%). Each game a different set of units is Exalted.

  - (3-4) BaseAmountXP=1, BaseDamageAmp=7, DamageAmpPerLevel=1, HRGold=2, 1
  - (5) BaseAmountXP=2, BaseDamageAmp=7, DamageAmpPerLevel=3, HRGold=4, 3

### Fated

Hover one Fated unit over another to form a pair and unlock a Fated Bonus. Your pair gains [PairHPBonus]% .

  - (3-4) BondIncreasePercent=1, PairHPBonus=0.2, 0.2, 1, None
  - (5-6) BondIncreasePercent=2, PairHPBonus=0.2, 0.2, 1, 1
  - (7-9) BondIncreasePercent=3, PairHPBonus=0.2, 0.2, 1, 1
  - (10) BondIncreasePercent=3, PairHPBonus=0.2, 0.2, 1, 1

### Fortune

When you lose a player combat, gain Luck. The more fights in a row you lose, the more Luck you get. Lose Luck when you win. Each Festival, roll a new die.

  - (3-4) Heal=None, HealDU=None, HealHR=None, WinPenalty=0.1, 5, 4, 4, 3, 3, 1, 1, None, 6
  - (5-6) Heal=2, HealDU=1, HealHR=1, WinPenalty=0.1, 5, 4, 4, 3, 3, 1, 1, None, 6
  - (7) Heal=2, HealDU=1, HealHR=1, WinPenalty=None, 5, 4, 4, 3, 3, 1, 1, 1, 12

### Ghostly

Upon dealing or taking damage [NumInstances] times, Ghostly units send [NumSpectres] spectres to haunt nearby enemies and heal [HealingPercent]% max Health every [TickRate] seconds. Haunted enemies take bonus damage for each spectre on them, and pass spectres on death.

  - (2-3) DamagePercent=0.05, HealingPercent=0.02, NumInstances=5, NumSpectres=2, TickRate=2
  - (4-5) DamagePercent=0.1, HealingPercent=0.02, NumInstances=5, NumSpectres=2, TickRate=2
  - (6-7) DamagePercent=0.16, HealingPercent=0.02, NumInstances=5, NumSpectres=2, TickRate=2
  - (8) DamagePercent=0.36, HealingPercent=0.02, NumInstances=5, NumSpectres=2, TickRate=2

### Great

Every [CastCount] casts, Wukong grows his weapon, modifying his Abilities.

  - (1) CastCount=3

### Heavenly

Heavenly units grant unique stats to your team, increased for each Heavenly unit in play. Heavenly units gain [SelfishHeavenlyBonus]% more. Kha'Zix - [TFTUnitProperty.trait:TFT11_Trait_Heavenly_Crit]% Malphite - [TFTUnitProperty.trait:TFT11_Trait_Heavenly_Defenses] Neeko - [TFTUnitProperty.trait:TFT11_Trait_Heavenly_Health] Qiyana - [TFTUnitProperty.trait:TFT11_Trait_Heavenly_AD]% Soraka - [TFTUnitProperty.trait:TFT11_Trait_Heavenly_AP]% Wukong - [TFTUnitProperty.trait:TFT11_Trait_Heavenly_AS]% Emblem - Omnivamp [TFTUnitProperty.trait:TFT11_Trait_Heavenly_Omnivamp]%

  - (2) HeavenlyBonus=1, SelfishHeavenlyBonus=0.7, 1, 10, 1, 5, 10, 10, 10, 60, 8
  - (3) HeavenlyBonus=1.1, SelfishHeavenlyBonus=0.7, 1, 10, 1, 5, 10, 10, 10, 60, 8
  - (4) HeavenlyBonus=1.25, SelfishHeavenlyBonus=0.7, 1, 10, 1, 5, 10, 10, 10, 60, 8
  - (5) HeavenlyBonus=1.45, SelfishHeavenlyBonus=0.7, 1, 10, 1, 5, 10, 10, 10, 60, 8
  - (6) HeavenlyBonus=1.7, SelfishHeavenlyBonus=0.7, 1, 10, 1, 5, 10, 10, 10, 60, 8
  - (7) HeavenlyBonus=2, SelfishHeavenlyBonus=0.7, 1, 10, 1, 5, 10, 10, 10, 60, 8

### Inkshadow

Gain unique Inkshadow items. Inkshadow champions gain [PercentBuff]% bonus damage and damage reduction.

  - (3-4) IncreasedPercentBuff=None, PercentBuff=5, 1
  - (5-6) IncreasedPercentBuff=11, PercentBuff=5, 2
  - (7) IncreasedPercentBuff=17, PercentBuff=5, 4

### Invoker

Every [TickRate] seconds, your units gain Mana.

  - (2-3) InvokerMana=5, TeamAP=None, TeamMana=5, TickRate=3
  - (4-5) InvokerMana=20, TeamAP=None, TeamMana=5, TickRate=3
  - (6) InvokerMana=35, TeamAP=None, TeamMana=20, TickRate=3

### Lovers

Change which Lover takes the field depending on whether they are placed in the front or back 2 rows. When the fielded Lover casts, the other provides a bonus effect. Front: Altruist Rakan Back: Trickshot Xayah

  - (1) 1500, 200, 1000, 200, 300, 3, 150

### Mythic

Mythic champions gain Health, Ability Power, and Attack Damage. After 4 player combats, they become Epic, increasing the bonus by [Multiplier]%.

  - (3-4) AP=11, HealthPct=0.08, Multiplier=0.5
  - (5-6) AP=22, HealthPct=0.18, Multiplier=0.5
  - (7-9) AP=35, HealthPct=0.25, Multiplier=0.5
  - (10) AP=35, HealthPct=0.25, Multiplier=0.5, PrismaticMultiplier=2.5

### Porcelain

After casting, Porcelain champions boil, gaining Attack Speed and taking less damage for [Duration] seconds.

  - (2-3) AS=0.3, DR=0.2, Duration=4
  - (4-5) AS=0.55, DR=0.33, Duration=4
  - (6) AS=1, DR=0.5, Duration=4

### Reaper


  - (2-3) BleedDamage=None, CritChance=0.25, Duration=2
  - (4) BleedDamage=0.5, CritChance=0.25, Duration=2

### Sage

Combat start: Allies in the front 2 rows gain Omnivamp. Allies in the back 2 rows gain Ability Power.

  - (2) APAmount=15, Omnivamp=0.12
  - (3) APAmount=30, Omnivamp=0.2
  - (4) APAmount=45, Omnivamp=0.3
  - (5) APAmount=75, Omnivamp=0.45

### Sniper

Innate: Snipers gain [HexRangeIncrease] Attack Range. Snipers deal more damage to targets farther away.

  - (2-3) BonusHexRangeIncrease=None, HexRangeIncrease=1, PercentDamageIncrease=8
  - (4-5) BonusHexRangeIncrease=None, HexRangeIncrease=1, PercentDamageIncrease=17.9
  - (6) BonusHexRangeIncrease=2, HexRangeIncrease=1, PercentDamageIncrease=34.7

### Spirit Walker

The first time the Spirit Walker drops below [RevivePercent]% Health, he unleashes the rage within, healing to full Health, gaining increased movement speed, and changing his Ability from Ram Slam to Tiger Strikes.

  - (1) DamageReduction=100, OmnivampPercent=None, RevivePercent=0.5

### Storyweaver

Storyweavers summon a Hero named Kayle and evolve her. Storyweavers gain max Health. Each Storyweaver star level increases Kayle's Health and Ability Power. Kayle gets [BonusASPerStage]% Attack Speed for each game Stage.

  - (3-4) BonusASPerStage=0.15, Health=60, 3, 100, 1, 10, 20, 25, 3, 0.1
  - (5-6) BonusASPerStage=0.15, Health=100, 3, 100, 1, 10, 20, 25, 3, 0.1
  - (7-9) BonusASPerStage=0.15, Health=150, 3, 100, 1, 10, 20, 25, 3, 0.1
  - (10) BonusASPerStage=0.15, Health=250, 3, 100, 1, 10, 20, 25, 3, 0.1

### Trickshot

Trickshots' abilities ricochet. Each ricochet deals a percentage of the previous bounce's damage.

  - (2-3) MaxBounces=1, PercentDamage=0.45
  - (4) MaxBounces=2, PercentDamage=0.6

### Trickshot/Altruist

Place in the back two rows to get Trickshot, place in the front two rows to get Altruist!

  - (1) (no numbers)

### Umbral

The moon illuminates hexes, Shielding units placed in them at the start of combat. Umbral units in illuminated hexes execute low Health enemies.

  - (2-3) DesireExecuteRewardPercent=None, DesireExecuteThreshhold=10, ShieldAmount=200
  - (4-5) DesireExecuteRewardPercent=None, DesireExecuteThreshhold=18, ShieldAmount=450
  - (6-8) DesireExecuteRewardPercent=None, DesireExecuteThreshhold=25, ShieldAmount=900
  - (9) DesireExecuteRewardPercent=1, DesireExecuteRewardPercentHR=1, DesireExecuteThreshhold=60, ShieldAmount=1000, 0.25

### Warden

Wardens take less damage. For the first [Duration] seconds of combat, they take an additional [StartOfCombatDRPercent]% less damage.

  - (2-3) DRPercent=10, Duration=10, StartOfCombatDRPercent=18
  - (4-5) DRPercent=22, Duration=10, StartOfCombatDRPercent=18
  - (6-7) DRPercent=35, Duration=10, StartOfCombatDRPercent=18
  - (8) DRPercent=45, Duration=10, StartOfCombatDRPercent=18

## Champions (61)

### Ahri (1-cost, APCaster)

- Traits: Fated, Arcanist | mana 0/60
- **Love Bomb**: Deal [ModifiedDamage] () magic damage to the current target and [ModifiedSecondaryDamage] () magic damage to adjacent enemies. Fated Bonus: Gain 27 .

### Caitlyn (1-cost, ADCaster)

- Traits: Ghostly, Sniper | mana 0/105
- **Ace in the Hole**: Fire a bullet towards the farthest enemy, dealing [TotalDamage] () physical damage to the first enemy hit.

### Cho'Gath (1-cost, APTank)

- Traits: Mythic, Behemoth | mana 40/90
- **Eruption**: Gain [ModifiedShield] () Shield for 4 seconds and breathe a cone of fire, dealing [ModifiedDamage] () magic damage, Burning, and Wounding enemies hit for 9 seconds. Burn: Deal 1% of the target's max Health as true damage every second Wound: Reduce healing received by 33%

### Darius (1-cost, APFighter)

- Traits: Umbral, Duelist | mana 0/30
- **Decimate**: Deal [ModifiedDamage] () magic damage to adjacent enemies. For the next 4 seconds, attacks deal [ModifiedDamageOnHit] () bonus magic damage.

### Garen (1-cost, ADTank)

- Traits: Storyweaver, Warden | mana 30/80
- **Courage**: Gain [ModifiedShield] () Shield for 5 seconds. Garen's next attack deals [AdditionalDamage] () physical damage.

### Jax (1-cost, APTank)

- Traits: Inkshadow, Warden | mana 0/40
- **Counter Strike**: Gain 90 Armor and Magic Resist for the next 2 seconds. Afterwards, deal [ModifiedAOEDamage] () magic damage to adjacent enemies. The current target takes [ModifiedDamage] () magic damage and is Stunned for 1.5 seconds.

### Kha'Zix (1-cost, ADReaper)

- Traits: Heavenly, Reaper | mana 0/40
- **Leap!**: Leap towards the lowest Health enemy within 3 hexes and deal [TotalDamage] () physical damage.

### Kobuko (1-cost, APTank)

- Traits: Fortune, Bruiser | mana 30/70
- **Money Dance**: Passive: Gain 10/12/20 permanent Health for each gold gained from interest. (Total: [TFTUnitProperty.:TFT11_Unit_FortuneYordle_Health]) Active: Heal [ModifiedHeal] () Health over 2 seconds. Kobuko's next attack deals [ModifiedDamageAmount] () bonus magic damage. In Hyper Roll, instead gain 60/80/200 permanent Health per Round.

### Kog'Maw (1-cost, APCaster)

- Traits: Mythic, Invoker, Sniper | mana 0/30
- **Raining Artillery**: Deal [ModifiedDamage] () magic damage to the lowest Health enemy within range. Every 3 casts, gain +1 Attack Range.

### Malphite (1-cost, ADTank)

- Traits: Heavenly, Behemoth | mana 40/80
- **Topaz Skin**: For 8 seconds, gain [ModifiedArmor] () Armor and attacks deal [TotalDamage] () magic damage to enemies in a cone.

### Rek'Sai (1-cost, APTank)

- Traits: Dryad, Bruiser | mana 40/80
- **Uproot**: Gain 75 Armor and Magic Resist for 5 seconds. Stun nearby enemies for 1 second and deal [ModifiedDamage] () magic damage.

### Sivir (1-cost, ADCarry)

- Traits: Storyweaver, Trickshot | mana 0/50
- **On the Hunt**: Gain 95/95/110% Attack Damage and grant [ModifiedAttackSpeed] () Attack Speed to self and adjacent allies for 6/8/10 seconds.

### Yasuo (1-cost, APTank)

- Traits: Fated, Duelist | mana 0/70
- **Steel Tempest**: Passive: Attacks while Shielded deal [ModifiedShieldDamage] () bonus magic damage. Active: Gain [ModifiedShield] () Shield for 5 seconds and deal [ModifiedDamage] () magic damage to the current target. Fated Bonus: Gain 250 Shield at the start of combat.

### Aatrox (2-cost, APFighter)

- Traits: Ghostly, Inkshadow, Bruiser | mana 0/60
- **Phantom Blade**: 20% Shred the current target for 4 seconds and deal [ModifiedDamage] () magic damage to them. Heal [ModifiedHealing] () Health. Shred: Reduce Magic Resist

### Gnar (2-cost, ADFighter)

- Traits: Dryad, Warden | mana 10/60
- **Boulder Toss**: Passive: Gain [ModifiedADGain]% () Attack Damage upon attacking or getting hit, up to 45 times. (Current bonus: [TFTUnitProperty.:TFT11_Gnar_CurrentADBonus]%) Active: Fling a boulder at the current target, dealing [TotalDamage] () physical damage to targets in a line.

### Janna (2-cost, APCarryHighMana)

- Traits: Dragonlord, Invoker | mana 25/75
- **Heavenly Winds**: Grant [ModifiedShield] () Shield to the 2 lowest Health allies for 4 seconds. Then, deal [ModifiedDamage] () magic damage to the 2 nearest enemies.

### Kindred (2-cost, APCaster)

- Traits: Fated, Dryad, Reaper | mana 0/30
- **Flourish of Arrows**: Dash away from the current target and deal [ModifiedDamage] () magic damage to them and [ModifiedSecondaryDamage] () magic damage to the nearest enemy. Fated Bonus: Gain 18% Attack Speed.

### Lux (2-cost, APCasterHighMana)

- Traits: Porcelain, Arcanist | mana 15/75
- **Bunny Binding**: Throw a spirit bunny towards the farthest enemy, dealing [ModifiedDamage] () magic damage to enemies along the way, reduced by 30% each time it hits an enemy. The first 2 enemies hit are Stunned for 1.5 seconds.

### Neeko (2-cost, APTank)

- Traits: Mythic, Heavenly, Arcanist | mana 40/80
- **Hop Blossom**: Hop into the air, reducing damage taken by 30/30/35% and Healing [ModifiedHeal] () Health over 2 seconds. After, slam down and deal [TotalDamage] () magic damage to adjacent enemies.

### Qiyana (2-cost, ADReaper)

- Traits: Heavenly, Duelist | mana 0/20
- **Supreme Display of Talent**: For 4 seconds, attacks deal [ModifiedDamage] () physical damage to the target and [SecondaryDamage] () physical damage to enemies behind them. Qiyana will dash to find the best angle.

### Riven (2-cost, ADFighter)

- Traits: Storyweaver, Altruist, Bruiser | mana 0/20
- **Broken Wings**: Deal [ModifiedDamage] () physical damage to the current target. Every 3rd cast, instead deal [BoostedModifiedDamage] () physical damage to the current target and enemies adjacent to them, then heal for [ModifiedHeal] () Health.

### Senna (2-cost, ADCarry)

- Traits: Inkshadow, Sniper | mana 20/75
- **Absolution**: Fire a laser 2 Hexes beyond the current target, dealing [ModifiedDamage] () physical damage to enemies hit. Grant 20/25/35% Attack Damage to Senna and allies to the left and right for 4 seconds.

### Shen (2-cost, APTank)

- Traits: Ghostly, Behemoth | mana 25/75
- **Spirit's Refuge**: For 4 seconds, reduce damage taken by [ModifiedShenSelfishReduction] () for Shen and [ModifiedFlatDamageReduction] () for adjacent allies. Shen's next 3 attacks deal [ModifiedTrueDamage] () bonus true damage.

### Teemo (2-cost, APCaster)

- Traits: Fortune, Trickshot | mana 0/30
- **Dim-Sum Delivery**: Throw a spoiled dumpling at the nearest non-poisoned enemy, poisoning them for [ModifiedDamage] () magic damage over 8 seconds.

### Yorick (2-cost, APTank)

- Traits: Umbral, Behemoth | mana 40/80
- **Dark Despoil**: Sap power from the 2 nearest enemies, gaining 300/360/420 max Health and dealing [ModifiedDamage] () magic damage to them over 3 seconds.

### Zyra (2-cost, APCasterHighMana)

- Traits: Storyweaver, Sage | mana 30/75
- **Paper Thorns**: Summon 2 Vine Lashers that attack the nearest enemy 3 times. Their attacks deal [ModifiedDamage] () magic damage and Wound their targets for 6 seconds. Wound: Reduce healing received by 33%

### Alune (3-cost, APCaster)

- Traits: Umbral, Invoker | mana 0/60
- **Lunar Barrage**: Rain meteors through the row with the most total enemy Health. Meteors deal [ModifiedDamage] () magic damage split between all enemies hit. Deal [ModifiedPrimaryDamage] () magic damage to the current target. All enemies hit are 20% Shredded for 6 seconds. Grant 20/25/30% Attack Speed split amongst allies in Alune's row for 5 seconds. Shred: Reduce Magic Resist

### Amumu (3-cost, APTank)

- Traits: Porcelain, Warden | mana 40/80
- **Herbal Tea-rs**: Instantly heal [ModifiedSelfHealingAmount] () Health and heal nearby allies for [ModifiedAllyHealingAmount] () Health. Cry for the next 3 seconds, healing Amumu and his allies for the same amount, and dealing [ModifiedDamage] () magic damage to nearby enemies over the total duration.

### Aphelios (3-cost, ADCarry)

- Traits: Fated, Sniper | mana 40/100
- **Duskwave**: 20% Sunder the nearest 3 enemies for 7 seconds. Then fire 3 shots at them, dealing [ModifiedDamage] () physical damage each. Fated Bonus: Gain 25% . Sunder: Reduce Armor

### Bard (3-cost, ADCarry)

- Traits: Mythic, Trickshot | mana 0/50
- **Meep Meep!**: For the next [ScaledDuration] seconds, attacks instead throw 2 Meeps, each dealing [ModifiedPhysicalDamage] () physical damage and [ModifiedMagicDamage] () magic damage.

### Diana (3-cost, APTank)

- Traits: Dragonlord, Sage | mana 50/100
- **Purification Rites**: Heal [ModifiedHeal] () Health and create a 1-hex sacred zone around Diana for 4 seconds, taking 30/30/35% reduced damage from enemies outside the zone. While the zone is up, Diana's attacks deal [TotalDamage] () bonus magic damage.

### Illaoi (3-cost, APTank)

- Traits: Ghostly, Arcanist, Warden | mana 30/90
- **Prophet of Spirits**: Gain [ModifiedShield] () Shield and summon a tentacle near the current target for 4 seconds. The tentacle slams every second, dealing [ModifiedDamage] () magic damage in a line and healing Illaoi for [ModifiedHealAmount] () Health.

### Soraka (3-cost, APCaster)

- Traits: Heavenly, Altruist | mana 0/60
- **Divine Judgement**: Passive: Each time your team loses 20% of their total Health, gain 8 Ability Power for the rest of combat. Active: Mana Reave the largest group of enemies within range. Deal [ModifiedDamage] () magic damage to the center enemy and [ModifiedAoEDamage] () magic damage to adjacent enemies. Mana Reave: Increase max Mana by 20% until the next cast

### Tahm Kench (3-cost, APTank)

- Traits: Mythic, Bruiser | mana 40/100
- **Tongue Lash**: Gain [ModifiedShield] () Shield for 3 seconds and deal [ModifiedDamage] () magic damage to the current target. If Tahm Kench has more Health than them, deal [BonusModifiedDamage] () magic damage instead. Otherwise, gain [BonusModifiedShield] () more Shield.

### Thresh (3-cost, APTank)

- Traits: Fated, Behemoth | mana 30/90
- **Guided Passage**: Grant [ModifiedShield] () Shield to Thresh and the lowest Health ally for 4 seconds and deal [ModifiedDamage] () magic damage to enemies adjacent to both. Thresh shares 15/15/20% of his Armor and Magic Resist with the shielded ally for 6 seconds. Fated Bonus: Gain 18 Armor and Magic Resist.

### Tristana (3-cost, ADReaper)

- Traits: Fortune, Duelist | mana 40/100
- **Firecracker Frenzy**: Gain 40/40/50% Attack Damage for 6 seconds. Jump on the current target and deal [ModifiedDamage] () physical damage to adjacent enemies, before bouncing to a safe location.

### Volibear (3-cost, APTank)

- Traits: Inkshadow, Duelist | mana 10/60
- **Ink Smash**: Empower the next three attacks. The first two heal for [ModifiedHeal] () Health and deal [ModifiedSmallDamage] () bonus magic damage. The final attack deals [ModifiedLargeDamage] () magic damage and Stuns the target for 1 second.

### Yone (3-cost, ADFighter)

- Traits: Umbral, Reaper | mana 10/70
- **Fate Sealed**: Dash in spirit form to the farthest enemy within [TFTUnitProperty.:TFT11_Unit_YoneDashRange] Hexes for 3 seconds, dealing [TotalDamage] () physical damage and Wounding enemies along the way for 5 seconds. While in spirit form, gain [ShieldAmount] () Shield and 75% Attack Speed. Afterwards, Yone returns to his starting position. Each cast this combat increases his dash range by 1 Hex. Wound: Reduce healing received by 33%

### Zoe (3-cost, APCaster)

- Traits: Fortune, Storyweaver, Arcanist | mana 0/60
- **Calculated!**: Passive: Kills increase the number of ricochets by 1 for the rest of combat. Active: Fire a missile dealing [TotalDamage] () magic damage to the current target. The missile ricochets to the farthest unit within 2 hexes, dealing [ModifiedSecondarySpellDamage] () magic damage.

### Annie (4-cost, APTank)

- Traits: Fortune, Invoker | mana 70/120
- **Get 'em, Tibbers!**: Passive: Heal [PassiveHeal] () Health when damaging a Burning enemy. Active: On first cast, hop on Tibbers, gaining [BonusHealth] () max Health and Stunning enemies within 2 hexes for 1.5 seconds. Deal [TotalDamage] () magic damage, Burn, and Wound enemies within 2 hexes for 5 seconds. Burn: Deal 1% of the target's max Health as true damage every second Wound: Reduce healing received by 33%

### Ashe (4-cost, ADCarry)

- Traits: Porcelain, Sniper | mana 30/90
- **Rain of Shards**: Passive: Start with 15 arrows. Gain 1/1/3 every 3 attacks. (Current arrows: [TFTUnitProperty.:TFT11_Unit_AsheArrows]) Active: Fire a volley split between the current target and the 3 closest enemies within range, dealing [ModifiedDamage] () physical damage per stored arrow for a total of [TFTUnitProperty.:TFT11_Unit_AsheTotalSpellDamage] () physical damage.

### Galio (4-cost, APTank)

- Traits: Storyweaver, Bruiser | mana 70/120
- **Savior's Protection**: Taunt enemies within 3 hexes and gain [ModifiedArmor] () Armor and [ModifiedMagicResist] () Magic Resist for 4/4/2 seconds. Galio is immobile and unstoppable during this time. After, deal [ModifiedDamage] () magic damage to all enemies in range and grant [ModifiedShieldAmount] () Shield to allies in range for 4 seconds. Taunt: enemies that are able and in range must attack the taunter

### Kai'Sa (4-cost, ADCaster)

- Traits: Inkshadow, Trickshot | mana 20/75
- **Inkstorm**: Fire 20 waves at the current target over 2 seconds, each dealing [ModifiedDamage] () physical damage to the first enemy hit. If an enemy is hit by 10 waves, they take [AmplifiedDamage] () damage per wave instead.

### Kayn (4-cost, ADReaper)

- Traits: Ghostly, Reaper | mana 0/60
- **Shadow Assassin**: On first cast, transform, dealing [TotalTransformDamage] () physical damage to nearby enemies. While transformed, gain 30% Critical Strike Chance and critical attacks damage adjacent enemies. If the spin only hits one enemy, the damage is increased by 100%. Subsequent casts deal [TotalSwipeDamage] () physical damage to units in a line through the target instead.

### Lee Sin (4-cost, ADFighter)

- Traits: Dragonlord, Duelist | mana 40/100
- **Dragon's Dance**: Kick the target's spirit out of their body, dealing [ModifiedDamage] () physical damage, 30% Mana Reaving them, and Stunning them. Enemies hit by the spirit take [ModifiedAOEDamage] () physical damage. Gain [ModifiedShieldAmount] () Shield for 3 seconds. Mana Reave: Increase max Mana until the next cast

### Lillia (4-cost, APCasterHighMana)

- Traits: Mythic, Invoker | mana 0/50
- **Eep!**: Hurl an orb towards the current target, dealing [ModifiedDamage] () magic damage to the first enemy hit and adjacent enemies. It continues rolling as a smaller orb, dealing [SecondaryDamage] () magic damage to enemies it hits. Every third cast launches a mega orb at her target that deals [ModifiedMegaDamage] () magic damage and splits into 3/3/5 smaller orbs that roll towards the farthest enemies.

### Morgana (4-cost, APCasterHighMana)

- Traits: Ghostly, Sage | mana 0/60
- **The Tempest's Torment**: Create a blizzard around a random enemy that deals [ModifiedDamage] () magic damage over 3 seconds and Chills for 1 second. Chill: Reduce Attack Speed by 10%

### Nautilus (4-cost, APTank)

- Traits: Mythic, Warden | mana 60/170
- **Depth Charge**: Send 3 shockwaves towards the most enemies in a line, dealing [ModifiedDamage] () magic damage and Stunning all enemies hit for 1.75/2/8 seconds.

### Ornn (4-cost, APTank)

- Traits: Dryad, Behemoth | mana 80/130
- **Forge of the Forest**: Gain [ModifiedTotalShieldValue] () shield for 4 seconds. Channel and forge unstoppably over 2 seconds, dealing [ModifiedMagicDamage] () magic damage to adjacent enemies over the duration. After, toss an ephemeral Completed item to the nearest ally.

### Sylas (4-cost, APFighter)

- Traits: Umbral, Bruiser | mana 0/40
- **Unchain The Demon**: Dash around the target and slam, dealing [ModifiedDamage] () magic damage and healing for [ModifiedHeal] () Health. Then, swipe twice, dealing [ModifiedAOEDamage] () magic damage to nearby enemies.

### Syndra (4-cost, APCaster)

- Traits: Fated, Arcanist | mana 0/30
- **Phantom Butterflies**: Passive: Start combat with 6/6/10 butterflies and gain one every 1 casts. Active: Throw all butterflies at the current target, dealing [ModifiedDamagePerButt] () magic damage for each butterfly. Fated Bonus: Deal 10% more damage.

### Azir (5-cost, APCasterHighMana)

- Traits: Dryad, Invoker | mana 70/130
- **Blight**: Fire a beam through the current target, dealing [ModifiedLaserDamage] () magic damage to enemies hit. Spawn a Guardian adjacent to the first enemy hit with 1100/1500/8000 Health. The Guardian does not move or attack.

### Hwei (5-cost, APCasterHighMana)

- Traits: Mythic, Artist | mana 30/105
- **A Brush with Fate**: Create an expanding paint pool around the most units that heals allies within for [ModifiedHealing] () Health every second for 3 seconds. After 3 seconds, it explodes, healing allies within for [ModifiedEndHeal] () Health and dealing [ModifiedDamage] () magic damage to enemies.

### Irelia (5-cost, ADCarry)

- Traits: Storyweaver, Duelist | mana 0/8
- **Blade Dance**: Passive: Attacks send blades that dart between enemies, dealing [Damage_BladeHit] () physical damage and 30% Sunder for 5 seconds. Cast every 8 attacks. Active: Command the blades to swiftly strike the three lowest Health enemies, dealing [Damage_SpellHit] () physical damage and applying on-hit effects. Irelia converts 25% of bonus attack speed into faster blade movement, and 70% into Attack Damage. Sunder: Reduce Armor

### Lissandra (5-cost, APCasterHighMana)

- Traits: Porcelain, Arcanist | mana 45/105
- **Let 'em Brew**: Steep the current target in a teapot, Stunning and dealing [ModifiedDamage] () magic damage over 3/3/15 seconds. If they die, there's a 45% chance that Lissandra turns them into a loot orb. (Rewards are doubled in Hyper Roll) If the target is the last enemy remaining, kill them instantly.

### Rakan (5-cost, APReaper)

- Traits: Dragonlord, Lovers, Altruist | mana 30/90
- **Grand Entrance**: Dash into the largest group of enemies and deal [ModifiedDamage] () magic damage to enemies in a 2-hex range. Gain [ModifiedShield] () Shield for each enemy hit for 2 seconds and reduce their damage by 10% for 4 seconds.

### Sett (5-cost, ADFighter)

- Traits: Fated, Umbral, Warden | mana 80/160
- **Light and Shadow**: Passive: At the start of combat, squat adjacent units in the same row. Permanently gain 2% Attack Damage for each 1000 Health squatted. Gains are doubled in Hyper Roll (Net gains: [TFTUnitProperty.:TFT11_Unit_SettADGained]%) Active: Slam the current target forward, Stunning them and dealing [TotalPrimaryDamage] () physical damage. Deal [TotalAoEDamage] () plus 8/12/1000% of the target's maximum health as physical damage to enemies within 2.5 hexes. Fated Bonus: Gain 15% Omnivamp.

### Udyr (5-cost, APFighter)

- Traits: Inkshadow, Behemoth, Spirit Walker | mana 30/90
- **Ram Slam / Tiger Strikes**: Ram Slam: Gain [ModifiedShield] () Shield for 3/3/10 seconds and charge through the current target, briefly knocking up any enemies he hits. Grab the nearest enemy and slam them into the ground, dealing [ModifiedDamage] () total magic damage to them and adjacent enemies. Tiger Strikes: Gain 60/70/500% Attack Speed and empower Udyr's attacks for the next 6/6/30 seconds to strike 4/4/6 additional times dealing [ModifiedShurikenDamage] () bonus magic damage.

### Wukong (5-cost, ADFighter)

- Traits: Great, Heavenly, Sage | mana 0/30
- **Legendary Prowess**: Cycle through the following abilities; Throw staff out, dealing [ModifiedStunDamage] () physical damage to the current target and Stunning them for 1.5/1.5/8 seconds. Spin, dealing [ModifiedSpinDamage] () physical damage to enemies within 1-hex range. Dash to hit the most enemies in a 3-hex line, then slam down staff dealing [ModifiedBonkDamage] () physical damage to enemies hit.

### Xayah (5-cost, ADCarry)

- Traits: Dragonlord, Lovers, Trickshot | mana 30/100
- **Feathers Fly!**: Throw 12/12/30 feathers at the 4 closest enemies, each dealing [ModifiedFeatherDamage] () physical damage. Then, recall the feathers, dealing [TotalFeatherRecallDamage] () physical damage to enemies they pass through.

### Xayah & Rakan (5-cost, )

- Traits: Lovers, Dragonlord, Trickshot/Altruist | mana 0/100
- **Dynamic Duo**: Xayah fires feathers at the nearest enemies, dealing physical damage. Rakan dashes towards the largest clump of units, deals magic damage to and debuffs them, and gains a shield for each enemy hit.

## Monsters, summons, pets and other trait-less units (26)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, )

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, )

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, )

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, )

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Target Dummy (1-cost, )

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Tibbers (5-cost, )

- Traits: (none) | mana 50/120
- **Childhood Rage**: Deal [ModifiedDamage] magic damage to enemies in a semi-circle and Stun them for 1.75/2.5/8 seconds.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### AzirSoldier (11-cost, )

- Traits: (none)
- **Placeholder Name**: Placeholder Tooltip

### Dragonlord Flames (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Golden Tree (11-cost, )

- Traits: (none) | mana 0/100
- **Golden Tree**: Luck per loss: [TFTUnitProperty.:TFT11_Trait_FortuneLossLevels] Luck penalty per win: [TFTUnitProperty.:TFT11_Trait_FortuneWinPenalty]% Player combats until rewards: [TFTUnitProperty.:TFT11_Trait_FortuneRoundsRemaining] Estimated Value: [TFTUnitProperty.:TFT11_Trait_FortuneGoldValue]g

### Kayle (11-cost, )

- Traits: (none) | mana 0/100
- **Celestial Flames**: Passive: Attacks deal [ModifiedOnHitDamage] () bonus magic damage. Active: Fire a wave at the current target that deals [ModifiedDamage] () magic damage to all enemies hit.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Rakan (11-cost, )

- Traits: (none) | mana 0/100
- **Gleaming Quill**: Rakan throws feathers at the 3 lowest Health allies, healing them for 150/250/1000 Health. This is affected by Trickshot.

### SightWard (11-cost, )

- Traits: (none) | mana 0/100
- **Exalted Soul Core**: Sell Core to gain [TFTUnitProperty.:TFT11_Exalted_NumEXP] XP

### Xayah (11-cost, )

- Traits: (none) | mana 0/100
- **Deadly Plumage**: Xayah deals 200/300/800 physical damage to all enemies hit by Rakan's ability.

### ZyraThornPlant (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 


<a id="set-12"></a>

# Set 12: Magic n' Mayhem

Source: patch 14.24 data. 88 champions, 28 traits.

## Traits

### Arcana

Use the Arcana signifier item to choose a High Arcana champion. Each Arcana grants power based on a different condition. Gain power for: Ahri: Fielding 3-star champions. Hecarim: Equipping items. Tahm Kench: Activating traits. Xerath: Purchasing Charms. Emblem: Equipping Spatula items.

  - (2) AhriAbilityPowerBase=10, AhriAbilityPowerPer3Star=5, EmblemBonusDamageBase=0.08, EmblemBonusDamagePerSpat=0.02, HecarimDRAmount=0.08, HecarimPercAD=0.1, TahmKenchHPPerTrait=20, XerathNumCharms=3, XerathNumCharmsHR=2, XerathPVEDamage=0.18, XerathScalingDamage=0.02
  - (3) AhriAbilityPowerBase=15, AhriAbilityPowerPer3Star=8, EmblemBonusDamageBase=0.12, EmblemBonusDamagePerSpat=0.03, HecarimDRAmount=0.08, HecarimPercAD=0.25, TahmKenchHPPerTrait=40, XerathNumCharms=3, XerathNumCharmsHR=2, XerathPVEDamage=0.3, XerathScalingDamage=0.03
  - (4) AhriAbilityPowerBase=20, AhriAbilityPowerPer3Star=12, EmblemBonusDamageBase=0.18, EmblemBonusDamagePerSpat=0.04, HecarimDRAmount=0.08, HecarimPercAD=0.45, TahmKenchHPPerTrait=60, XerathNumCharms=3, XerathNumCharmsHR=2, XerathPVEDamage=0.42, XerathScalingDamage=0.045
  - (5) AhriAbilityPowerBase=25, AhriAbilityPowerPer3Star=18, EmblemBonusDamageBase=0.25, EmblemBonusDamagePerSpat=0.06, HecarimDRAmount=0.08, HecarimPercAD=0.7, TahmKenchHPPerTrait=105, XerathNumCharms=3, XerathNumCharmsHR=2, XerathPVEDamage=0.6, XerathScalingDamage=0.07

### Ascendant

(1) Ascendant Charms can appear in your shop. The odds increase by [PercentageChancePerPlayerCombat]% after player combat, up to [MaxPercentageChance]%. When you buy one, the odds reset. Current Odds: [TFTUnitProperty.trait:TFT12_AscendantOdds]%

  - (1) MaxPercentageChance=50, PercentageChancePerPlayerCombat=12

### Bastion

Your team gains [TeamwideResists] Armor and Magic Resist. Bastions gain more, and the value doubles in the first [Duration] seconds of combat.

  - (2-3) BonusArmor=15, BonusMR=15, BonusPercent=1, Duration=10, StatMultiplier=2, TeamwideResists=10
  - (4-5) BonusArmor=45, BonusMR=45, BonusPercent=1, Duration=10, StatMultiplier=2, TeamwideResists=10
  - (6-7) BonusArmor=100, BonusMR=100, BonusPercent=1, Duration=10, StatMultiplier=2, TeamwideResists=10
  - (8) BonusArmor=200, BonusMR=200, BonusPercent=1, Duration=10, NumUnits=2, StatMultiplier=2, TeamwideResists=10, 4

### Bat Queen

When Morgana's bats kill an enemy, they have a [PercentChanceToAbduct]% chance to abduct a 1-star copy of it. In Choncc's Treasure, the chance to abduct increases at stage 3-1 and 4-1.

  - (1) PercentChanceToAbduct=40, PercentChanceToAbductCTR2=1, PercentChanceToAbductCTR3=20, 100

### Best Friends

Norra summons Yuumi to help your team! She can be attached to allies by holding her over them. Yuumi shares her Ability Power with Norra and dies when Norra does.

  - (1) (no numbers)

### Blaster

Blasters gain Damage Amp, which increases for [Duration] seconds after casting their Ability.

  - (2-3) BaseDamageAmp=0.15, DamageAmpAfterCasting=0.3, Duration=3
  - (4-5) BaseDamageAmp=0.35, DamageAmpAfterCasting=0.65, Duration=3
  - (6) BaseDamageAmp=0.6, DamageAmpAfterCasting=1, Duration=3

### Chrono

Chronos gain [BaseAP] Ability Power. Combat start: begin a [Timer] second countdown, which speeds up by [SecondsPerCast] second after each Chrono Ability cast. At the end of the countdown, gain a special effect:

  - (2-3) BaseAP=15, BonusAP=20, BonusAS=None, FreezeDuration=None, MaxHealthHeal=0.15, SecondsPerCast=1, TimeHeal=None, Timer=10
  - (4-5) BaseAP=15, BonusAP=55, BonusAS=None, FreezeDuration=3, MaxHealthHeal=0.15, SecondsPerCast=1, TimeHeal=None, Timer=10
  - (6) BaseAP=15, BonusAP=70, BonusAS=35, FreezeDuration=4, MaxHealthHeal=0.15, SecondsPerCast=1, TimeHeal=1, Timer=10

### Dragon

Burn: Deals a percent of the target's max Health as true damage every second

  - (2) BurnPercent=1, BurnWoundDuration=5, WoundPercent=33
  - (3) BurnPercent=2, BurnWoundDuration=5, WoundPercent=33

### Druid

Wukong does not have a trait bonus, but instead has a powerful Ability and increased stats.

  - (1) (no numbers)

### Eldritch

When your team loses [HealthRatio]% of their Health, an Old God awakens. It gains [PercentHealthPerStarLevel]% Health and [APPerStarLevel]% Ability Power for each Eldritch star level.

  - (3-4) APPerStarLevel=0.1, HealthRatio=0.2, PercentHealthPerStarLevel=0.25, 2
  - (5-6) APPerStarLevel=0.1, HealthRatio=0.2, PercentHealthPerStarLevel=0.25, 2
  - (7-9) APPerStarLevel=0.1, HealthRatio=0.2, PercentHealthPerStarLevel=0.25, 2
  - (10) APPerStarLevel=0.1, HealthRatio=0.2, PercentHealthPerStarLevel=0.25, 2

### Faerie

Faeries gain Health and create special items that become stronger with trait tiers. Only Faeries can hold Faerie items.

  - (3-4) BonusHealth=150, DamageAmp=0.3, 1, None
  - (5-6) BonusHealth=400, DamageAmp=0.45, 1, 1
  - (7-8) BonusHealth=700, DamageAmp=0.6, 2, 1
  - (9) BonusHealth=999, DamageAmp=0.66, 2, 1

### Frost

Frost champions gain Ability Power and Attack Damage. The first few enemies to die become allied ice soldiers which lure enemy attacks if they are within range and retargeting. Soldiers have [HealthPerStage] Health per stage.

  - (3-4) BonusStats=16, HealthPerStage=200, NumStatues=1, None, 110
  - (5-6) BonusStats=40, HealthPerStage=200, NumStatues=2, None, 110
  - (7-8) BonusStats=55, HealthPerStage=200, NumStatues=3, 1, 110
  - (9) BonusStats=90, HealthPerStage=200, NumStatues=4, 1, 110

### Honeymancy

Honeymancers gain [MiniCount] Bees that deal magic damage to their target every [Period] seconds. Each Bee deals damage based on the damage a Honeymancer deals and takes. When a Honeymancer dies, they leave [NumTransfers] Bees that follow nearby Honeymancers for the rest of combat.

  - (3-4) BonusDamage=0.07, DamageReceived=0.03, MiniCount=5, NumTransfers=2, Period=3
  - (5-6) BonusDamage=0.13, DamageReceived=0.05, MiniCount=5, NumTransfers=2, Period=3
  - (7) BonusDamage=0.2, DamageReceived=0.1, MiniCount=5, NumTransfers=2, Period=1.5

### Hunter

Hunters gain Attack Damage, increased the first time they get a takedown each combat.

  - (2-3) BaseAD=15, BaseAS=None, BonusADAfterTakedown=35
  - (4-5) BaseAD=40, BaseAS=None, BonusADAfterTakedown=70
  - (6) BaseAD=65, BaseAS=0.2, BonusADAfterTakedown=100

### Incantor

Your team gains [TeamwideAP] Ability Power. When Incantors attack or cast, all Incantors gain stacks of up to [MaxStacks]. Every other attack grants [StacksPerAttack] stack, and each Ability cast grants [StacksPerCast] stacks.

  - (2-3) BonusAPPerStack=1, MaxStacks=40, StacksPerAttack=1, StacksPerCast=5, TeamwideAP=10
  - (4) BonusAPPerStack=2, MaxStacks=40, StacksPerAttack=1, StacksPerCast=5, TeamwideAP=30

### Mage

Mages cast their Abilities twice and have modified total Ability Power.

  - (3-4) APMultiplier=0.85
  - (5-6) APMultiplier=0.95
  - (7-9) APMultiplier=1.1
  - (10) APMultiplier=2, MaxMana=20

### Multistriker

Multistrikers' attacks have a chance to trigger [NumBonusAttacks] extra attacks.

  - (3-4) NumBonusAttacks=2, OnHitHealPerc=None, ProcChance=25, 5
  - (5-6) NumBonusAttacks=2, OnHitHealPerc=None, ProcChance=65, 2
  - (7-8) NumBonusAttacks=2, OnHitHealPerc=0.03, ProcChance=80, 2
  - (9) NumBonusAttacks=2, OnHitHealPerc=0.1, ProcChance=100, 1

### Portal

Combat start: Portal champions Shield for [ShieldDuration] seconds. Objects from other dimensions fly out of a portal every few seconds to help allies and disrupt enemies. The portal becomes [IncreasePerStarLevel]% stronger for each Portal champion's star level.

  - (3-5) IncreasePerStarLevel=0.08, ShieldAmount=200, ShieldDuration=15, -1, -1, 20, None, 6, -1, 4, 3, 1, 0.07, None, 75
  - (6-7) IncreasePerStarLevel=0.08, ShieldAmount=450, ShieldDuration=15, -1, -1, 30, None, 6, 1, 3, 4, 1, 0.09, None, 200
  - (8-9) IncreasePerStarLevel=0.08, ShieldAmount=700, ShieldDuration=15, 1, -1, 40, 2, 6, 2, 3, 4, 1, 0.11, None, 250
  - (10) IncreasePerStarLevel=0.08, ShieldAmount=1500, ShieldDuration=15, 2, 1, 55, 5, 6, 3, 2.25, 10, 1, 0.15, None, 800

### Preserver

Your team heals for a percent of their max Health every [Duration] seconds. If they're full Health, restore Mana instead. Preservers gain double the amount.

  - (2) Duration=3, HealthPct=0.02, ManaPct=0.03, 2
  - (3) Duration=3, HealthPct=0.04, ManaPct=0.05, 2
  - (4) Duration=3, HealthPct=0.06, ManaPct=0.07, 2
  - (5) Duration=3, HealthPct=0.08, ManaPct=0.1, 2

### Pyro

Your team gains [BaseASPct]% Attack Speed. Pyro champions gain more and execute enemies under [ExecutePct]% Health. For each kill, Pyro champions create an infernal cinder that you collect the next round. For every [NumCinders] cinders you collect, your team gains [ASPctPerNumCinders]% more . Cinders collected: [TFTUnitProperty.trait:TFT12_PyroCindersCollected] Team : [TFTUnitProperty.trait:TFT12_PyroAttackSpeed]%

  - (2) ASPctPerNumCinders=2, BaseASPct=3, ExecutePct=10, NumCinders=5, NumCindersHR=3, PyroUnitAS=5
  - (3) ASPctPerNumCinders=2, BaseASPct=3, ExecutePct=10, NumCinders=5, NumCindersHR=3, PyroUnitAS=25
  - (4) ASPctPerNumCinders=2, BaseASPct=3, ExecutePct=10, NumCinders=5, NumCindersHR=3, PyroUnitAS=50
  - (5) ASPctPerNumCinders=2, BaseASPct=3, ExecutePct=15, NumCinders=5, NumCindersHR=3, PyroUnitAS=65

### Ravenous

Briar gains [TOOLTIPDamageAmpPerHealth]% Damage Amp for each player health you are missing. Each round, gain a Light Snack to give her [BriarHealthGain] in exchange for [TacticianHealthToFeed] of your player health. In Choncc's Treasure, Light Snack's effect will be enhanced at stage 3-1 and 4-1.

  - (1) BriarHealthGain=180, BriarHealthGainCTR2=60, BriarHealthGainCTR3=120, BriarHealthGainDU=250, BriarHealthGainHR=250, GoldToFeedPvE=3, TOOLTIPDamageAmpPerHealth=0.008, TOOLTIPDamageAmpPerHealthDU=0.008, TOOLTIPDamageAmpPerHealthHR=0.04, TOOLTIPDamageAmpPvE=0.4, TacticianHealthToFeed=3, TacticianHealthToFeedCTR2=1, TacticianHealthToFeedCTR3=2, TacticianHealthToFeedDU=2, TacticianHealthToFeedHR=1

### Relic

After combat, Milio has a 40/60/100% chance to convert a temporary relic component into a real component.

  - (1-5) (no numbers)

### Scholar

Scholars gain bonus Mana on attack and Ability Power.

  - (2-3) AP=10, ManaPerAttack=3
  - (4-5) AP=20, ManaPerAttack=5
  - (6) AP=20, AllyHealing=0.1, ManaPerAttack=10

### Shapeshifter

Shapeshifters gain max Health. After their first Ability cast, they triple this effect.

  - (2-3) BonusHealthRatio=0.1
  - (4-5) BonusHealthRatio=0.18
  - (6-7) BonusHealthRatio=0.27
  - (8-9) BonusHealthRatio=0.33, HealDuration=2, MaxHeal=0.03
  - (10) BonusHealthRatio=0.66, HealDuration=2, MaxHeal=0.05

### Sugarcraft

Sugarcrafters build a layer cake from sugar. Gain sugar for each component your champions are holding after player combat. They gain Attack Damage and Ability Power, increased by [BonusRatio]% for each cake layer. When the cake has reached [MaxLayers] layers, gain treats instead! Total: [TFTUnitProperty.trait:TFT12_Pastry_BonusStats] Layers: [TFTUnitProperty.trait:TFT12_Pastry_NumLayers] / [MaxLayers] Sugar per round: [TFTUnitProperty.trait:TFT12_Pastry_SugarToAdd] = [TFTUnitProperty.trait:TFT12_Pastry_TotalComponents] components x [TFTUnitProperty.trait:TFT12_Pastry_TraitSugarMult] from trait

  - (2-3) BaseAPAD=22, BonusRatio=0.1, MaxLayers=7, SugarMultiplier=3, SugarMultiplierHR=6, SugarMultiplierPVE=1, 400
  - (4-5) BaseAPAD=33, BonusRatio=0.1, MaxLayers=7, SugarMultiplier=4, SugarMultiplierHR=8, SugarMultiplierPVE=3, 400
  - (6-7) BaseAPAD=36, BonusHealth=100, BonusRatio=0.1, MaxLayers=7, SugarMultiplier=6, SugarMultiplierHR=12, SugarMultiplierPVE=5, 400
  - (8) BaseAPAD=50, BonusHealth=500, BonusRatio=0.1, MaxLayers=7, SugarMultiplier=10, SugarMultiplierHR=20, SugarMultiplierPVE=7, 400

### Vanguard

Vanguards gain [DamageReductionPct]% Durability while Shielded. Combat start and at [HealthThreshold]% Health: Gain a Shield for a percent of max Health for [ShieldDuration] seconds.

  - (2-3) DamageReductionPct=0.1, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.18
  - (4-5) DamageReductionPct=0.1, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.35
  - (6) DamageReductionPct=0.15, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.5

### Warrior

Warriors gain Omnivamp and Damage Amp. When Warriors drop below [HealthThreshold]% health, they gain double .

  - (2-3) BonusDamage=0.1, HealthThreshold=0.7, Omnivamp=0.1, 2
  - (4-5) BonusDamage=0.2, HealthThreshold=0.7, Omnivamp=0.2, 2
  - (6) BonusDamage=0.25, DamageReduction=0.15, HealthThreshold=0.7, Omnivamp=0.25, 2

### Witchcraft

Witch Abilities apply a curse to enemies for [CurseDuration] seconds. When a cursed enemy dies, the curse spreads to the nearest enemy. Cursed enemies become:

  - (2-3) ChaseIncreasePercent=None, CurseDuration=4, CurseHealPct=None, FrogPolymorphCooldown=None, FrogPolymorphDuration=None, MaxHealthReduce=120, TrueDamagePct=None
  - (4-5) ChaseIncreasePercent=None, CurseDuration=4, CurseHealPct=6, FrogPolymorphCooldown=None, FrogPolymorphDuration=None, MaxHealthReduce=120, TrueDamagePct=None
  - (6-7) ChaseIncreasePercent=None, CurseDuration=4, CurseHealPct=6, FrogPolymorphCooldown=None, FrogPolymorphDuration=None, MaxHealthReduce=120, TrueDamagePct=25
  - (8) ChaseIncreasePercent=50, CurseDuration=4, CurseHealPct=6, FrogPolymorphCooldown=8, FrogPolymorphDuration=2, MaxHealthReduce=120, TrueDamagePct=25

## Champions (60)

### Ashe (1-cost, ADCarry)

- Traits: Eldritch, Multistriker | mana 40/75
- **Dark Tidings**: For the rest of combat, fire an additional arrow at a nearby enemy that deals [TotalDamage] () physical damage. This effect stacks.

### Blitzcrank (1-cost, APTank)

- Traits: Honeymancy, Vanguard | mana 40/90
- **Honey Barrier**: Gain [ModifiedShield] () Shield for 4 seconds.

### Elise (1-cost, APTank)

- Traits: Eldritch, Shapeshifter | mana 30/90
- **Spider Form**: Transform into a spider, replace this Ability with Petrifying Venom, then cast it. Petrifying Venom: Stun the target for 1.5 seconds and deal [ModifiedDamage] () magic damage to them. Heal for [ModifiedHeal] ().

### Jax (1-cost, APTank)

- Traits: Chrono, Multistriker | mana 20/60
- **Grandmaster-At-Arms**: Deal [ModifiedMagicDamage] () magic damage to adjacent enemies and gain [ModifiedBonusResists] () Armor and Magic Resist for 6 seconds. During this time, attacks deal [ModifiedBonusDamage] ( ) bonus magic damage.

### Jayce (1-cost, ADFighter)

- Traits: Portal, Shapeshifter | mana 0/60
- **Hammer Form**: Transform into hammer form and gain [ModifiedResistGain] () Armor and Magic Resist. Replace this Ability with Hammer Slam and immediately cast it. Hammer Slam: Leap to the target, dealing [Damage] () physical damage.

### Lillia (1-cost, APTank)

- Traits: Faerie, Bastion | mana 40/100
- **Dream Bloom**: Heal for [ModifiedHeal] (), then deal [ModifiedDamage] () magic damage to adjacent enemies.

### Nomsy (1-cost, ADCaster)

- Traits: Dragon, Hunter | mana 10/50
- **Fiery Sneeze**: Sneeze fire at the target, dealing [SpellDamage] () physical damage. 50% of overkill damage is dealt to the closest 2 targets. Dragon Upgrade: Deal [UpgradeSpellDamage] () physical damage instead. Overkill hits 4 targets.

### Poppy (1-cost, ADTank)

- Traits: Witchcraft, Bastion | mana 30/80
- **Clean-Up Duty**: Gain [ModifiedShield] () Shield for 4 seconds. Then, wallop the target for [ModifiedDamage] () physical damage.

### Seraphine (1-cost, APCaster)

- Traits: Faerie, Mage | mana 10/60
- **Sharp Note**: Fires a soundwave towards the longest line of enemies that deals [ModifiedMagicDamage] () magic damage, reduced by 30% for each enemy hit.

### Soraka (1-cost, APCasterHighMana)

- Traits: Sugarcraft, Mage | mana 10/50
- **Starfall**: Call down a star on the target that deals [ModifiedDamage] () magic damage to adjacent enemies then heals a nearby ally for [ModifiedHeal] ().

### Twitch (1-cost, ADCasterMana)

- Traits: Frost, Hunter | mana 0/35
- **Ice Shard**: Fire a piercing shard of ice that Sunders for 5 seconds and deals [TotalDamage] () physical damage, reduced by 20% for each enemy hit. Sunder: Reduce Armor

### Warwick (1-cost, ADFighter)

- Traits: Frost, Vanguard
- **Ravenous Hunter**: Passive: Permanently gain [TFTUnitProperty.:TFT12_WarwickASAmountToGain]% Attack Speed and movement speed for each champion killed. Attacks heal for [ModifiedHeal] () Health and deal [BonusOnAttackDamage] () bonus physical damage. Gain double the healing and physical damage for targets below 25% Health. (Current Bonus: +[TFTUnitProperty.:TFT12_WarwickBonusAS]% Attack Speed and movement speed)

### Ziggs (1-cost, APCaster)

- Traits: Honeymancy, Incantor | mana 0/40
- **Bouncing Bee**: Throw a bouncing bomb at the farthest enemy. The bomb explodes on the first enemy hit dealing [MagicDamage] () magic damage to the target and [ModifiedSecondaryDamage] () to all nearby enemies.

### Zoe (1-cost, APCaster)

- Traits: Portal, Witchcraft, Scholar | mana 0/30
- **Trouble Bottle**: Deal [ModifiedDamage] () magic damage to the target and reduce their Magic Resist by 10/15/20 for the rest of combat.

### Ahri (2-cost, APCaster)

- Traits: Arcana, Scholar | mana 0/35
- **Orb of Perception**: Fire an orb at the current target dealing [ModifiedOrbDamage] () magic damage. Launch [ModifiedFoxfires] firefoxes at enemies near the target, each dealing [ModifiedTrueDamage] () true damage. Gain an additional foxfire this combat.

### Akali (2-cost, ADReaper)

- Traits: Pyro, Warrior, Multistriker | mana 0/60
- **Fire Point Strike**: Dash to the farthest enemy within 3 hexes, dealing [ModifiedDashDamage] () physical damage to them. The next 3 attacks launch kunai at the target, dealing [TotalAutoDamage] () damage.

### Cassiopeia (2-cost, APCarry)

- Traits: Witchcraft, Incantor | mana 0/30
- **Witch Fang**: Empower the next 3 attacks to deal [ModifiedDamage] () magic damage.

### Galio (2-cost, APTank)

- Traits: Portal, Vanguard, Mage | mana 30/90
- **Convergence**: Reduce incoming damage by [ModifiedDamageReduction] () for 4 seconds. Fire two magic bolts that converge on the furthest enemy within 3 hexes, Stunning them for 1.25/1.25/1.5 seconds. Each bolt deals [ModifiedDamage] () magic damage to enemies it passes through. Mage casts will target a different enemy.

### Kassadin (2-cost, APFighter)

- Traits: Portal, Multistriker | mana 30/60
- **Nether Blade**: Deal [ModifiedStabDamage] () magic damage to the target and gain [ShieldAmount] () Shield for 3 seconds. For the rest of combat, attacks deal [ModifiedStackingDamage] () bonus stacking magic damage.

### Kog'Maw (2-cost, ADCarry)

- Traits: Honeymancy, Hunter | mana 20/70
- **Hive Ooze**: Launches a honey wad through the current target, dealing [ModifiedPhysicalDamage] () physical damage to enemies hit. Adjacent allies and allies hit by the trail are set abuzz, gaining [ModifiedAttackSpeed] () Attack Speed for 4 seconds.

### Nilah (2-cost, ADReaper)

- Traits: Eldritch, Warrior | mana 0/60
- **Formless Blade**: Passive: Shields grant 60% Attack Speed for 4 seconds. Active: Dash up to 2 hexes away, then deal [ModifiedStrikeDamage] () physical damage to enemies in a line through the target. Gain [ModifiedShield] () Shield for 4 seconds.

### Nunu (2-cost, APTank)

- Traits: Honeymancy, Bastion | mana 25/75
- **Sticky Bite**: Bite the target, healing for [ModifiedHeal] (), dealing [ModifiedDamage] () magic damage, and Chilling them for 3 seconds. Chill: Reduce Attack Speed by 20%

### Rumble (2-cost, APTank)

- Traits: Sugarcraft, Vanguard, Blaster | mana 30/70
- **Hot-Tea**: For 3 seconds, gain [ModifiedDamageReduction] () Durability and create a cone of flame that deals [ModifiedDamage] () magic damage over the duration and Wounds enemies for 5 seconds. Wound: Reduce healing received by 33%

### Shyvana (2-cost, APFighter)

- Traits: Dragon, Shapeshifter | mana 0/70
- **Dragon's Descent**: Transform into dragon form and gain an aura that deals [TotalDamagePerSecond] () magic damage per second to adjacent enemies. Then fly through the largest group of enemies within 2 hexes and Stun them for 1/1/1.25 seconds. Dragon Upgrade: Double the size of the aura. It deals 15% more damage.

### Syndra (2-cost, APCasterHighMana)

- Traits: Eldritch, Incantor | mana 0/60
- **Transcendence**: Conjure a rift on the target that deals [ModifiedPrimaryTargetDamage] () magic damage and 20% Shreds for 6 seconds. This ability upgrades the more it is cast. (Total Casts: [TFTUnitProperty.:TFT12_Syndra_TimesCast] casts) [TFTUnitProperty.:TFT12_Syndra_CastUpgrade1] Casts: Enemies adjacent to the target are Shredded and take [ModifiedAOEDamage] () magic damage. [TFTUnitProperty.:TFT12_Syndra_CastUpgrade2]+ Casts: For every [TFTUnitProperty.:TFT12_Syndra_CastUpgrade2] casts, create an additional rift on a nearby enemy that deals 35% damage. Shred: Reduce Magic Resist

### Tristana (2-cost, ADCaster)

- Traits: Faerie, Blaster | mana 0/45
- **Explosive Charge**: Fire a cannonball at the target, dealing [TotalDamage] () physical damage to the first enemy hit. That enemy is knocked back 2 hexes and Stunned if they are within 2 hexes of Tristana.

### Zilean (2-cost, APCasterHighMana)

- Traits: Frost, Chrono, Preserver | mana 0/60
- **Time Bomb**: Place a bomb on the target, dealing [ModifiedInitialDamage] () magic damage and Stunning them for 1 seconds. When the stun ends or the target dies, the bomb explodes, dealing [ModifiedDamage] () magic damage to the target and [ModifiedSecondaryDamage] () magic damage to adjacent enemies.

### Bard (3-cost, APCaster)

- Traits: Sugarcraft, Preserver, Scholar | mana 30/80
- **Traveling Whimsy**: Launch a magic missile at the target that bounces 4 times between enemies dealing [ModifiedDamage] () magic damage to enemies hit. They also take 10% more damage for 3 seconds.

### Ezreal (3-cost, ADCaster)

- Traits: Portal, Blaster | mana 15/75
- **Portalshot Barrage**: Fire a blast in a wide line through the current target, dealing [TotalDamage] () physical damage to enemies hit, reduced by 25% for each enemy is passes through. The first enemy hit takes an additional [ModifiedFirstTargetDamage] () magic damage. If an enemy is adjacent, blink to safety before firing.

### Hecarim (3-cost, ADFighter)

- Traits: Arcana, Bastion, Multistriker | mana 0/40
- **Rampage**: Passive: On takedown of the current target, empower the next attack to deal [ModifiedChargeDamage] () physical damage. If the next target is out of range, charge to them. Active: For the next 6 seconds, attacks also cleave adjacent enemies for [TotalCleaveDamage] () physical damage.

### Hwei (3-cost, APCaster)

- Traits: Frost, Blaster | mana 0/40
- **Subject: Storm**: Paint a lightning bolt on the target. A moment later the bolt strikes, dealing [ModifiedBoltDamage] () magic damage to the target and [ModifiedAreaDamage] () magic damage to adjacent enemies. Every 3rd cast, paint 2 extra bolts on the lowest percent Health enemies.

### Jinx (3-cost, ADCarry)

- Traits: Sugarcraft, Hunter | mana 0/50
- **Get Excited!**: Gain 125% decaying Attack Speed for 5 seconds. For the duration, attacks fire rockets that deal [RocketDamage] () physical damage and ignore 50% Armor.

### Katarina (3-cost, APReaper)

- Traits: Faerie, Warrior | mana 10/70
- **Death Lotus**: Blink to the largest group of enemies within 2 hexes and spin, dealing [TotalDamage] () magic damage over 3 seconds to enemies within 2 hexes and Wounding them for 5 seconds. The 2 nearest enemies take [ModifiedAdditionalDamage] () magic damage instead. If Katarina kills an enemy, she blinks to reposition again. Wound: Reduce healing received by 33%

### Mordekaiser (3-cost, APTank)

- Traits: Eldritch, Vanguard | mana 0/40
- **Obliterate**: Gain [ModifiedShieldAmount] () Shield that decays over 3 seconds and slam the target, dealing [ModifiedSlamDamage] () magic damage in a 2-hex line. Gain 10% Damage Amp for the rest of combat.

### Neeko (3-cost, APTank)

- Traits: Witchcraft, Shapeshifter | mana 40/110
- **Surprise!!!**: Transform into a Crab for 2 seconds. Over the duration, regain [ModifiedSelfHeal] () health and release scuttleblasts at 3 nearby allies and 3 nearby enemies. Allies hit are healed for [ModifiedHeal] (); enemies hit take [ModifiedDamage] () magic damage and are Stunned for 1.25 seconds.

### Shen (3-cost, APTank)

- Traits: Pyro, Bastion | mana 50/120
- **Twilight Eruption**: Gain [ModifiedDamageReduction] () Durability for 4 seconds. Create 3 volcanic ruptures under the closest enemies which Stun for 1.5 seconds and deal [ModifiedFlameDamage] () magic damage. Subsequent ruptures on the same enemy are 50% effective.

### Swain (3-cost, APTank)

- Traits: Frost, Shapeshifter | mana 60/120
- **Blizzard**: Transform, gaining [ModifiedInitialHealth] () max Health. While transformed, deal [ModifiedMagicDamage] () magic damage to enemies within 2 hexes every second. If already transformed, gain [ModifiedSecondaryHealth] () additional max Health and increase the damage by [ModifiedSecondaryDamage] ().

### Veigar (3-cost, APCaster)

- Traits: Honeymancy, Mage | mana 0/40
- **King Bee's Judgement**: Passive: Gain [TFTUnitProperty.:TFT12_VeigarAbilityPowerGainPerZap] Ability Power for every Charm you buy. (Charms Bought: [TFTUnitProperty.:TFT12_VeigarZapsPurchased]) Active: Deal [ModifiedDamage] () magic damage to the target.

### Vex (3-cost, APTank)

- Traits: Chrono, Mage | mana 50/100
- **Personal Space**: Gain [ModifiedShieldAmount] () Shield for 4 seconds, increased by 5% for each enemy targeting Vex. When it expires, deal [ModifiedBaseDamage] () magic damage to enemies within 2 hexes.

### Wukong (3-cost, APTank)

- Traits: Druid | mana 20/70
- **Cyclone**: Combat Start: Gain 45 Armor and Magic Resist. Lose 1 of each every second. Active: Spin, gaining [ModifiedShield] () Shield for 4 seconds and dealing a total of [SpellDamage] () physical damage to each adjacent enemy.

### Fiora (4-cost, ADFighter)

- Traits: Witchcraft, Warrior | mana 70/130
- **Dance of Blades**: Leap backward and briefly gain 70% Durability. Then, sweep through the most enemies possible within 2 hexes, dealing [TotalBaseDamage] () physical damage to each one, plus [TotalSplitDamage] () split between all enemies hit.

### Gwen (4-cost, APFighter)

- Traits: Sugarcraft, Warrior | mana 0/30
- **Snip Snip!**: Dash to a safe position and snip [ModifiedNumSnips] times in a cone. The first and last snip each deal [SnipDamage] () magic damage, while snips inbetween deal [SmallSnipDamage] (). Increase the number of snips this combat by 1.

### Kalista (4-cost, ADCarry)

- Traits: Faerie, Multistriker | mana 0/70
- **Rend**: Passive: Attacks embed a spear in her target. Every 3/3/1 attacks, also embed spears into 2/2/3 nearby enemies. Active: Deal [SpearDamage] () physical damage to each enemy for each of their embedded spears. If there is a safe position nearby, leap to it.

### Karma (4-cost, APCaster)

- Traits: Chrono, Incantor | mana 0/30
- **Borrowed Time**: Attach a temporal rift to the target and the closest unaffected enemy that deals [ModifiedDamage] () magic damage over 6/6/4 seconds. Reset the duration of all other rifts on the enemy.

### Nami (4-cost, APCaster)

- Traits: Eldritch, Mage | mana 0/50
- **Leviathan**: Fire a bubble that deals [ModifiedBubbleDamage] () magic damage at the largest group of enemies within range. Every 3rd cast, summon The Leviathan instead, which Stuns and deals [ModifiedLeviathanDamage] () magic damage to enemies in a line.

### Nasus (4-cost, ADTank)

- Traits: Pyro, Shapeshifter | mana 0/80
- **Hellhound**: Steal 350/500/5000 Health split between the nearest 3/4/10 enemies. Each cast after heals [ModifiedAttackHealAmount] () Health and deals [EmpoweredDamage] () physical damage to the target instead.

### Olaf (4-cost, ADFighter)

- Traits: Frost, Hunter | mana 0/50
- **Ragnarok**: Passive: Every 4 attacks and when changing targets, leap at the target and deal [SlamDamage] () physical damage to the target and adjacent enemies. Active: For the next 5 seconds, gain [ModifiedAttackSpeed] Attack Speed, [ModifiedOmnivamp]% () Omnivamp, and crowd control immunity.

### Rakan (4-cost, APTank)

- Traits: Faerie, Preserver | mana 40/120
- **Gleaming Quill**: Gain [ModifiedShieldAmount] () Shield for 3 seconds and shoot 8 feathers in a wide cone, dealing [ModifiedMagicDamage] () magic damage to enemies they hit. Then land, granting allies within 2 hexes a [ModifiedAllyShieldAmount] () Shield for 4 seconds.

### Ryze (4-cost, APCasterHighMana)

- Traits: Portal, Scholar | mana 15/90
- **Runic Portal**: Open portals that fire [ModifiedNumMissiles] () magic bolts split among the 3/3/6 closest enemies. Each bolt deals [ModifiedDamage] () magic damage.

### Tahm Kench (4-cost, APTank)

- Traits: Arcana, Vanguard | mana 80/180
- **Divination Dive**: Dive underground and surface next to the closest enemy. Send out a shockwave towards the farthest enemy within 4 hexes. Enemies within 2 hexes or hit by the shockwave take [TotalDamage] () magic damage and are Stunned for 1.75/2/8 seconds.

### Taric (4-cost, APTank)

- Traits: Portal, Bastion | mana 50/100
- **All Eyes on Me**: Gain [ModifiedDamageReduction] () Durability for 4 seconds and attract nearby enemy projectiles. At the end, release a nova that deals [ModifiedMagicDamage] () magic damage in a 2/2/3 hex range.

### Varus (4-cost, ADCaster)

- Traits: Pyro, Blaster | mana 20/100
- **Hail of Hellfire**: Fire a supernova at a cluster of enemies within [ModifiedCastRange] () hexes that deals [TotalDamage] () physical damage to the target and adjacent enemies. It explodes into a cluster of fireballs, dealing [ClusterDamage] () physical damage to all other enemies within three hexes.

### Briar (5-cost, ADFighter)

- Traits: Ravenous, Eldritch, Shapeshifter | mana 0/80
- **Certain Death**: Leap to the largest group of enemies, dealing [LeapDamage] () physical damage to enemies in a 2-hex radius and Stun them for 1.25 seconds. Then, enter a frenzy, gaining [ModifiedFrenzyAttackSpeed] () Attack Speed, 15/15/100% Omnivamp, and causing attacks to deal [FrenzyCleaveDamage] () bonus physical damage to the target and adjacent enemies. If already frenzied, empower the next attack to chomp, dealing [ModifiedBiteDamage] () physical damage. (Bonus Health: +[TFTUnitProperty.:TFT12_BriarHealthFed] Health)

### Camille (5-cost, ADFighter)

- Traits: Chrono, Multistriker | mana 30/100
- **Temporal Anomaly**: Passive: On target death, hookshot to the lowest percent health enemy within 3 hexes and deal [IsolationZoneDamage] () physical damage. After a hookshot or Ability cast, the next attack deals [TrueDamage] () true damage. Active: Knock back and Stun enemies adjacent to the target for 1/1/7 seconds, dealing [IsolationZoneDamage] () physical damage. Create a containment zone around the target for 4/4/7 seconds. Gain 30% Durability and 225% Attack Damage while in the zone.

### Diana (5-cost, APFighter)

- Traits: Frost, Bastion | mana 40/120
- **Snowfall**: Dash within 2 hexes to hit the most enemies possible and heal for [ModifiedBonusSnowHeal] (). Excess healing becomes a 3 second Shield. Then, deal [ModifiedInnerRingDamage] () magic damage to adjacent enemies and [ModifiedOuterRingDamage] () magic damage to enemies 2 hexes away. Every 2/2/1 casts create a snowfall that Chills all enemies and heals all allies for [ModifiedSnowHeal] () over 3 seconds. Excess healing also becomes a 3 second Shield. Chill: Reduce Attack Speed by 20%

### Milio (5-cost, APCaster)

- Traits: Faerie, Scholar | mana 0/90
- **Knick-Knack Attack**: Throw 2/2/8 completed items to allies with open slots. If an ally has no room for items, they instead gain a stacking 12% Damage Amp for the rest of combat. Then, throw 3/3/8 knick-knacks at enemies that each deal [MagicDamage] () magic damage. Occasionally, also finds something special in the backpack.

### Morgana (5-cost, APFighter)

- Traits: Witchcraft, Bat Queen, Preserver | mana 40/110
- **My Lovely Pets**: Gain [ModifiedShieldAmount] () shield for 4 seconds and summon a swarm of 8/8/30 bats. They seek out the 4 closest enemies within 3 hexes and deal [ModifiedMagicDamage] () magic damage each. Bats deal true damage to enemies below 25% Health.

### Norra & Yuumi (5-cost, APCaster)

- Traits: Portal, Best Friends, Mage | mana 30/80
- **Portalpalooza**: Launch a ball at the current target that explodes for [MagicDamage] () magic damage, and colors the area for 5/5/15 seconds, dealing [DOTDamage] () magic damage over the duration. If the ball lands in an area that is already colored, it will bounce to a new location within 2-hexes before exploding.

### Smolder (5-cost, ADCarry)

- Traits: Dragon, Blaster | mana 0/40
- **Learning to Fly**: Passive: Fly around and attack the nearest enemy. Active: Gain 50/50/500% Attack Speed for the next 4/4/30 attacks. These attacks launch fireballs that deal [ModifiedDamage] () physical damage. Dragon Upgrade: Next 6/6/99 attacks instead. Fireballs deal [ModifiedDragonDamage] () physical damage.

### Xerath (5-cost, APCasterHighMana)

- Traits: Arcana, Ascendant | mana 0/120
- **Eye of the Magus**: Launch 10/10/99 arcane blasts randomly split between the closest 5 enemies that each deal [ModifiedDamage] () magic damage.

## Monsters, summons, pets and other trait-less units (28)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, )

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, )

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, )

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, )

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, )

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, )

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Target Dummy (1-cost, )

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### The Tower (1-cost, )

- Traits: (none)
- **Lightning Strike**: Every 3 seconds, deal 10% max Health true damage to the 2 nearest enemies within 3 hexes.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Dark Monolith (11-cost, )

- Traits: (none) | mana 30/60
- **Earthquake**: Passive: On arrival, Stun all adjacent enemies for 2 seconds. Active: Deal [ModifiedSlamDamage] () to the target and all adjacent enemies. For the next 8 seconds, attacks chain lightning 2 times to nearby enemies, dealing [ModifiedLightningDamage] () magic damage.

### Frog (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Ice Soldier (11-cost, )

- Traits: (none)
- **Soldier's Sacrifice**: On death deal 0/0/500-0/0/700 magic damage (based on current Stage) to enemies within 0/0/1 hexes.

### Many-Eyed Beast (11-cost, )

- Traits: (none) | mana 50/90
- **Sky Splitter**: Passive: On arrival, Stun all enemies within 2 hexes. Every 3 attacks will chain lightning 4 times to nearby enemies. Lightning deals [ModifiedPassiveDamage] () magic damage. Active: Deal [ModifiedActiveDamage] () magic damage to enemies near the target. Then, gain 60% Attack Speed for 4 seconds.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Shyvana (11-cost, )

- Traits: (none) | mana 0/100
- **Dragon's Descent**: Transform into dragon form and gain an aura that deals [TotalDamagePerSecond] () magic damage per second to adjacent enemies. Then fly through the largest group of enemies within [HexRange] hexes and Stun them for [StunDuration] seconds. Dragon Upgrade: Double the size of the aura. It deals [DragonPercentDamageIncrease]% more damage.

### Sru_Crab (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Stormbringer (11-cost, )

- Traits: (none) | mana 70/70
- **Sky Splitter**: Passive: On arrival, Stun all enemies within [StunHexRange] hexes. Every [NumAttacksToZap] attacks will chain lightning [NumChains] times to nearby enemies. Lightning deals [ModifiedPassiveDamage] () magic damage. Active: Deal [ModifiedActiveDamage] () magic damage to enemies near the target. Then, gain [ASRatio]% Attack Speed for [ASDuration] seconds.

### Swain (11-cost, )

- Traits: (none) | mana 0/100
- **Blizzard**: Transform, gaining [ModifiedInitialHealth] () max Health. While transformed, deal [ModifiedMagicDamage] () magic damage to enemies within 2 hexes every second. If already transformed, gain [ModifiedSecondaryHealth] () additional max Health and increase the damage by [ModifiedSecondaryDamage] ().

### Tainted Golem (11-cost, )

- Traits: (none) | mana 40/80
- **Elder Slam**: Deal [ModifiedDamage] () magic damage to the target and adjacent enemies, Stunning them for 1.25 seconds.

### Yuumi (11-cost, )

- Traits: (none)
- **Friends Forever!**: Launch a ball at the current target that explodes for [MagicDamage] () magic damage, and colors the area for [DOTDuration] seconds, dealing [DOTDamage] () magic damage over the duration. If the ball lands in an area that is already colored, it will bounce to a new location within 2-hexes before exploding.


<a id="set-13"></a>

# Set 13: Into the Arcane

Source: current client data. 88 champions, 38 traits.

## Traits

### Academy

The Academy sponsors 3 items each game. Copies of sponsored items grant bonus max Health and Damage Amp. Academy units holding sponsored items gain double the amount, plus an additional [BaseValues]% Health and Damage Amp.

  - (3) BaseValues=0.05, NumOfItems=1, PercentIncreaseSponsored=0.02
  - (4) BaseValues=0.05, NumOfItems=2, PercentIncreaseSponsored=0.03
  - (5) BaseValues=0.05, NumOfItems=3, PercentIncreaseSponsored=0.05
  - (6) BaseValues=0.05, NumOfItems=3, PercentIncreaseSponsored=0.09

### Ambusher

Damage from Ambushers' Abilities can critically strike. They also gain bonus Critical Strike Chance and Critical Strike Damage.

  - (2) CritChance=0.25, CritDamage=0.1
  - (3) CritChance=0.35, CritDamage=0.2
  - (4) CritChance=0.45, CritDamage=0.25
  - (5) CritChance=0.55, CritDamage=0.25, DamageReductionPct=0.15

### Artillerist

Every [NumOfAttacks] attacks, Artillerists launch a rocket that deals [PercentDamage]% Attack Damage around the target. They also gain Attack Damage.

  - (2-3) AD=0.1, NumOfAttacks=5, PercentDamage=1.25
  - (4-5) AD=0.45, NumOfAttacks=5, PercentDamage=1.25
  - (6) AD=0.7, NumOfAttacks=3, PercentDamage=2.5

### Automata

Automata gain a crystal when they deal damage. At [TriggerNumCrystals] crystals, they blast their current target, dealing magic damage + [ConversionDamagePct]% of damage dealt since the previous blast and reset. They also gain Armor and Magic Resist.

  - (2-3) ConversionDamagePct=0.25, MagicDamage=150, Resists=25, TriggerNumCrystals=20, 1
  - (4-5) ConversionDamagePct=0.25, MagicDamage=450, Resists=60, TriggerNumCrystals=20, 1
  - (6) ConversionDamagePct=0.25, MagicDamage=1337, Resists=111, TriggerNumCrystals=20, 1

### Banished Mage

The first time you would be eliminated, if Mel has cast [MinimumCasts] times during player combat this game, she saves you and you remain alive. Afterwards, Mel permanently gains [ProcDamageAmp]% Damage Amp. [TFTUnitProperty.:TFT13_MissMageTrait_TrackerTRA]

  - (1) MinimumCasts=12, MinimumCastsDU=12, MinimumCastsHR=6, MissMageHealDU=15, PlayerHealth=1, ProcDamageAmp=0.1, ProcDamageAmpDU=0.1, ProcDamageAmpHR=0.1

### Betrayal

Maddie always targets Ambessa's target with her Ability and attacks. Every round where either gets at least [NumKills] kills, gain a Maddie.

  - (2) NumKills=2

### Black Rose

Each Black Rose champion's star level increases Sion's power

  - (3) NumCastsToUnlock=5, PercentHealthPerStarLevel=0.15, PercentHealthUnlock=0.65, 0.15, 40, 0.35, 4
  - (4) NumCastsToUnlock=5, PercentHealthPerStarLevel=0.15, PercentHealthUnlock=0.65, 0.15, 45, 0.35, 4
  - (5-6) NumCastsToUnlock=5, PercentHealthPerStarLevel=0.15, PercentHealthUnlock=0.65, 0.15, 50, 0.35, 4
  - (7) NumCastsToUnlock=5, PercentHealthPerStarLevel=0.15, PercentHealthUnlock=0.65, 0.15, 100, 0.35, 4

### Blood Hunter

Warwick devours enemies that drop below [EnemyHealthPercentThreshold]% Health, healing him for [DevourHealing] and granting him [ManaPerKill] Mana.

  - (1) DevourHealing=450, EnemyHealthPercentThreshold=0.12, ManaPerKill=50, 5

### Bruiser

Your team gains [TeamFlatHealth] max Health. Bruisers gain more.

  - (2-3) BonusPercentHealth=0.2, TeamFlatHealth=100
  - (4-5) BonusPercentHealth=0.45, TeamFlatHealth=100
  - (6) BonusPercentHealth=0.8, TeamFlatHealth=100

### Chem-Baron

Gain Shimmer after each player combat. If your loss streak is at least [MinStreakLength], gain more. At each stack of [ShimmerBreakpoint] Shimmer, the Black Market offers you contraband that only Chem-Barons can use. Chem-Barons gain max Health for each Black Market you pass on. Markets Skipped: [TFTUnitProperty.trait:TFT13_Crime_PassedMarkets] Current Health: [TFTUnitProperty.trait:TFT13_Crime_CurrentHealth]

  - (3) HealthPerSkip=20, MinStreakLength=3, ShimmerBreakpoint=100, ShimmerPerCombat=15, ShimmerPerCombatStreaking=35, ShimmerPerCombatStreaking_DU=25, ShimmerPerCombatStreaking_HR=50, ShimmerPerCombat_DU=10, ShimmerPerCombat_HR=25, ShimmerPerDeath_PVE=1, ShimmerPerRound_PVE=15, 1
  - (4) HealthPerSkip=60, MinStreakLength=3, ShimmerBreakpoint=100, ShimmerPerCombat=15, ShimmerPerCombatStreaking=50, ShimmerPerCombatStreaking_DU=35, ShimmerPerCombatStreaking_HR=60, ShimmerPerCombat_DU=10, ShimmerPerCombat_HR=30, ShimmerPerDeath_PVE=1, ShimmerPerRound_PVE=20, 1
  - (5) HealthPerSkip=100, MinStreakLength=3, ShimmerBreakpoint=100, ShimmerPerCombat=20, ShimmerPerCombatStreaking=70, ShimmerPerCombatStreaking_DU=50, ShimmerPerCombatStreaking_HR=70, ShimmerPerCombat_DU=15, ShimmerPerCombat_HR=35, ShimmerPerDeath_PVE=2, ShimmerPerRound_PVE=30, 1
  - (6) HealthPerSkip=150, MinStreakLength=3, ShimmerBreakpoint=100, ShimmerPerCombat=25, ShimmerPerCombatStreaking=90, ShimmerPerCombatStreaking_DU=70, ShimmerPerCombatStreaking_HR=90, ShimmerPerCombat_DU=20, ShimmerPerCombat_HR=45, ShimmerPerDeath_PVE=3, ShimmerPerRound_PVE=40, 1

### Conqueror

Conquerors' takedowns grant stacks of Conquest. After gaining enough Conquest, open War Chests full of loot! Conquerors gain Attack Damage and Ability Power, increased by [PercentIncreasePerChest]% for each War Chest opened. War Chests Opened: [TFTUnitProperty.trait:TFT13_Warband_ChestsCompleted] Current Stats: [TFTUnitProperty.trait:TFT13_Warband_CurrentADAP]%

  - (2-3) ADAPBase=0.22, PercentIncreasePerChest=0.08, StacksPerKill=1
  - (4-5) ADAPBase=0.33, PercentIncreasePerChest=0.08, StacksPerKill=3
  - (6-8) ADAPBase=0.4, PercentIncreasePerChest=0.08, StacksPerKill=6
  - (9) ADAPBase=1.2, PercentIncreasePerChest=0.08, StacksPerKill=20

### Dominator

Combat start: Dominators gain a Shield for [ShieldDuration] seconds. When Dominators cast, they gain stacking Ability Power based on the Mana spent.

  - (2-3) APPercent=0.25, ShieldAmount=350, ShieldDuration=15
  - (4-5) APPercent=0.45, ShieldAmount=550, ShieldDuration=15
  - (6) APPercent=0.65, ShieldAmount=800, ShieldDuration=15

### Emissary

This trait is active only when you have exactly 1 or 4 unique Emissaries. Ambessa: Allies gain [AmbessaArmor] Armor and Magic Resist for each opponent defeated. ([TFTUnitProperty.trait:TFT13_Ambassador_PlayersDefeated]/7) Garen: On Combat Start, Garen and allies to his left and right gain [GarenHealthPercent]% of his max Health. Nami: Allies' attacks grant [NamiMana] bonus Mana. Tristana: Allies gain [StarAS]% Attack Speed per star level.

  - (1) AmbessaArmor=2, AmbessaArmor_PVE=4, BonusAS=None, BonusHealth=250, CCImmuneDuration=8, GarenHealthPercent=0.12, NamiMana=3, SelfishDamageAmp=0.2, StarAS=0.06, 2, None, 4
  - (4) AmbessaArmor=2, AmbessaArmor_PVE=4, BonusAS=None, BonusHealth=250, CCImmuneDuration=8, GarenHealthPercent=0.12, NamiMana=3, SelfishDamageAmp=0.2, StarAS=0.06, 2, None, 4

### Enforcer

Combat Start: Enforcers gain Shield and Damage Amp. The highest Health enemy units become WANTED! When a Wanted enemy dies, Enforcers gain [AttackSpeedPerWantedKill]% Attack Speed.

  - (2-3) AttackSpeedPerWantedKill=0.3, BaseDA=0.12, NumOfWantedEnemies=1, PercentMaxHealthShield=0.12, None, None
  - (4-5) AttackSpeedPerWantedKill=0.3, BaseDA=0.2, NumOfWantedEnemies=2, PercentMaxHealthShield=0.22, None, None
  - (6-7) AttackSpeedPerWantedKill=0.3, BaseDA=0.33, NumOfWantedEnemies=4, PercentMaxHealthShield=0.33, None, None
  - (8-9) AttackSpeedPerWantedKill=0.3, BaseDA=0.44, NumOfWantedEnemies=5, PercentMaxHealthShield=0.44, None, None
  - (10) AttackSpeedPerWantedKill=0.3, BaseDA=1.5, NumOfWantedEnemies=99, PercentMaxHealthShield=1, None, 1

### Experiment

Gain Laboratory hexes on your board. Combat start: Experiments standing on Laboratory hexes gain the Experiment bonuses of all Experiments on Laboratory hexes, plus max Health.

  - (3-4) ExperimentBonusIncrease=None, MaxHealth=100, NumLaboratory=2, 1.15
  - (5-6) ExperimentBonusIncrease=None, MaxHealth=450, NumLaboratory=3, 1.3
  - (7) ExperimentBonusIncrease=1, MaxHealth=450, NumLaboratory=3, 1.5

### Family

Family members support each other, reducing their max Mana and gaining extra bonuses. [TFTUnitProperty.trait:TFT13_JunkerKing_SelfDestructTRA]

  - (3) AttackSpeed=0.2, DamageReduction=0.2, ManaReduction=0.25
  - (4) AttackSpeed=0.2, DamageReduction=0.2, ManaReduction=0.3
  - (5) AttackSpeed=0.2, DamageReduction=0.2, ManaReduction=0.4, 0.75, 2.5, 1.25, 1.5, 17, 2, 2, 0.5

### Firelight

Every [Duration] seconds, Firelights dash. While dashing, they attack with infinite range and heal a percentage of the damage taken since their last dash.

  - (2) Duration=6, PercentDamageTakenHeal=0.25, None, 1
  - (3) Duration=6, PercentDamageTakenHeal=0.4, None, 1
  - (4) Duration=6, PercentDamageTakenHeal=0.45, 3.5, 1

### Form Swapper

Innate: Form Swappers change their stats and ability based on if they're placed in the front 2 rows or back 2 rows. Frontline Form Swappers gain Durability. Backline Form Swappers gain Damage Amp.

  - (2) DA=0.2, DR=0.15
  - (3) DA=0.3, DR=0.22
  - (4) DA=0.44, DR=0.33

### Geniuses

When Heimerdinger casts, Ekko releases [NumEkkoAfterImages] afterimages, each dealing [ReducedEkkoDamage]% damage. When Ekko casts, Heimerdinger fires [NumHeimerMissiles] missiles, each dealing [ReducedHeimerDamage]% damage.

  - (2) NumEkkoAfterImages=3, NumHeimerMissiles=3, ReducedEkkoDamage=0.5, ReducedHeimerDamage=1.5

### High Roller

When casting, Sevika rolls a random Jinx modification to her Ability and gains [Durability]% Durability for [Duration] seconds. Mods: -Rocket: Launch [Bonus1NumRockets] rockets that deal [Bonus1RocketDamage] physical damage each -Shield: Gain [Bonus2ShieldAmount] Shield for [Bonus2ShieldDuration] seconds -Coin: Gain [Bonus4Gold] gold -Triple: Enhance her rolled ability! In Choncc's Treasure, the Coin modification grants less gold in earlier stages.

  - (1) Bonus1NumRockets=8, Bonus1RocketDamage=100, Bonus2ShieldAmount=500, Bonus2ShieldDuration=4, Bonus4Gold=3, Durability=0.8, Duration=1.5, 0.35, 0.35, 0.1, 0.35, 0.2, 0.005, None, 1.5, 0.245, 0.4

### Junker King

Every [NumRounds1Star] rounds, open an armory to purchase permanent upgrades to your strongest Rumble's mech. Rounds Until Upgrade: [TFTUnitProperty.trait:TFT13_JunkerKing_RoundsUntilOpening]

  - (1) NumRounds1Star=3, NumRounds1Star_HR=2, NumRounds2Star=3, NumRounds2Star_HR=2, NumRounds3Star=1, NumRounds3Star_HR=1

### Machine Herald

Viktor has a fixed Attack Speed of [AS] attacks per second and converts ALL bonus Attack Damage, Attack Speed, and Mana into Ability Power. Instead of Mana, Viktor gains 1 Chaos Energy every attack and casts when he has [MaxMana].

  - (1) AS=0.55, MaxMana=8, startingmana=3, 1

### Martial Law

When Ambessa casts, Caitlyn fires an empowered attack at the target, dealing [CaitAD]% damage. Ambessa gains [ADShare]% of Caitlyn's Attack Damage.

  - (2) ADShare=0.18, CaitAD=2

### Menaces

Powder gains Dominator but no longer benefits from Family. When her monkey explodes, it creates [NumMonstrosities] of Silco's monstrosities.

  - (2) NumMonstrosities=3

### Pit Fighter

Pit Fighters gain [OmnivampPercent]% Omnivamp and deal bonus true damage. Once per combat at [HealthThreshold]% Health, they heal a percentage of their max Health over [Duration] seconds.

  - (2-3) Duration=2, HealPercent=0.1, HealthThreshold=0.5, OmnivampPercent=0.15, TrueDamagePercent=0.06
  - (4-5) Duration=2, HealPercent=0.35, HealthThreshold=0.5, OmnivampPercent=0.15, TrueDamagePercent=0.12
  - (6-7) Duration=2, HealPercent=0.55, HealthThreshold=0.5, OmnivampPercent=0.15, TrueDamagePercent=0.22
  - (8) Duration=2, HealPercent=0.8, HealthThreshold=0.5, OmnivampPercent=0.15, TrueDamagePercent=0.35

### Quickstriker

Quickstrikers move faster and gain Attack Speed, based on their target's missing Health.

  - (2) MaxBonusAS=0.6, MinBonusAS=0.2, TeamwideAS=None, 200
  - (3) MaxBonusAS=0.8, MinBonusAS=0.3, TeamwideAS=None, 200
  - (4) MaxBonusAS=1, MinBonusAS=0.4, ShieldAmount=200, ShieldDuration=3, TeamwideAS=None, 200

### Rammus

Rammus always makes room for himself in any team. +1 Team Size, but only if Rammus is on your team.

  - (1) (no numbers)

### Rebel

Rebels gain [MaxHealthIncrease]% max Health. After your team loses [PercentHealthTrigger]% of their Health, a smoke signal appears, granting Rebels [BurstAS]% Attack Speed for [BurstDuration] seconds and extra power for the rest of combat.

  - (3-4) ADAP=0.2, BurstAS=0.6, BurstDuration=5, ForceDuration=None, MaxHealthIncrease=0.15, PercentHealthTrigger=0.25, PercentMaxHealthBonus=None, StunDuration=None, None
  - (5-6) ADAP=0.4, BurstAS=0.6, BurstDuration=5, ForceDuration=None, MaxHealthIncrease=0.15, PercentHealthTrigger=0.25, PercentMaxHealthBonus=0.12, StunDuration=None, None
  - (7-9) ADAP=0.45, BurstAS=0.6, BurstDuration=5, ForceDuration=None, MaxHealthIncrease=0.15, PercentHealthTrigger=0.25, PercentMaxHealthBonus=0.15, StunDuration=2, None
  - (10) ADAP=0.5, BurstAS=0.6, BurstDuration=5, ForceDuration=12, MaxHealthIncrease=0.15, PercentHealthTrigger=0.25, PercentMaxHealthBonus=0.3, StunDuration=2, 1

### Reunion

When Vi casts, Ekko releases 3 afterimages towards her target each dealing [EkkoAfterImageReducedDamage]% damage. When Ekko casts, Vi slams an earthquake towards his target dealing [ViEarthquakeReducedDamage]% damage.

  - (2) EkkoAfterImageReducedDamage=0.4, ViEarthquakeReducedDamage=1

### Scrap

Combat start: Components held by Scrap units temporarily turn into full items. Scrap units gain Shield for [ShieldDuration] seconds for each component held by your team, including those that make up a full item. Shield Amount: [TFTUnitProperty.:TFT13_Trait_Scrap_ShieldTotal]

  - (2-3) HPShieldAmount=25, HPShieldAmountCT=25, NumComponents=1, ShieldDuration=24, None, None
  - (4-5) HPShieldAmount=35, HPShieldAmountCT=35, NumComponents=3, ShieldDuration=24, None, None
  - (6-8) HPShieldAmount=45, HPShieldAmountCT=45, NumComponents=20, ShieldDuration=24, 1, None
  - (9) HPShieldAmount=60, HPShieldAmountCT=60, NumComponents=20, ShieldDuration=24, 1, 1

### Sentinel

Your team gains Armor and Magic Resist. Sentinels gain triple.

  - (2-3) BonusArmor=12, 2
  - (4-5) BonusArmor=25, 2
  - (6) BonusArmor=42, 2

### Sisters

When Jinx scores a takedown, Vi gains [ADBuff]% bonus Attack Damage for [ViDuration] seconds. When Vi scores a takedown, Jinx gains [ASBuff]% bonus Attack Speed for [JinxDuration] seconds.

  - (2) ADBuff=0.8, ASBuff=0.8, JinxDuration=10, ViDuration=10

### Sniper

Snipers gain Damage Amp and deal more damage to targets farther away.

  - (2-3) BaseDamageAmp=0.05, BonusHexRangeIncrease=None, PercentDamageIncrease=5
  - (4-5) BaseDamageAmp=0.35, BonusHexRangeIncrease=None, PercentDamageIncrease=10
  - (6) BaseDamageAmp=0.75, BonusHexRangeIncrease=5, PercentDamageIncrease=20

### Sorcerer

Your team gains [TeamAP] Ability Power. Sorcerers gain more.

  - (2-3) BaseAP=20, TeamAP=10
  - (4-5) BaseAP=55, TeamAP=10
  - (6) BaseAP=90, DamageDecrease=0.2, DamageDecreaseDuration=3, TeamAP=10

### Unlikely Duo

Jinx and Sevika gain [AD]% Attack Damage and [Health] Health. Whenever one casts, they grant the other [Mana] mana. Sevika's arm is luckier.

  - (2) AD=0.1, Health=100, Mana=10, 0.1

### Visionary

Whenever Visionaries gain Mana, they gain more.

  - (2-3) EnlightenedBonusMana=0.25
  - (4-5) EnlightenedBonusMana=0.5
  - (6) AllyHealing=0.16, EnlightenedBonusMana=0.75

### Watcher

Watchers gain Durability, increased while above [HealthBreakpoint]% Health.

  - (2-3) BaseDR=0.15, HealthBreakpoint=0.5, IncreasedDR=0.3
  - (4-5) BaseDR=0.25, HealthBreakpoint=0.5, IncreasedDR=0.45
  - (6) BaseDR=0.4, HealthBreakpoint=0.5, IncreasedDR=0.5

### What Could Have Been

Vander gains [TFTTrait.TFT13_Teamup_BloodBrothers.1:MaxHealthGainPerCast] permanent max Health for each time Silco casts. Silco gains [TFTTrait.TFT13_Teamup_BloodBrothers.1:APGainPerDeath] permanent Ability Power for each time Vander dies. Bonuses: [TFTUnitProperty.trait:TFT13_Sidekick_BloodBrothers_HealthGained] | [TFTUnitProperty.trait:TFT13_Sidekick_BloodBrothers_APGained]

  - (2) APGainPerDeath=6, MaxHealthGainPerCast=30

## Champions (63)

### Amumu (1-cost, APTank)

- Traits: Automata, Watcher
- **Obsolete Technology**: Passive: Reduce all incoming damage by 15/20/30. Every second, emit sparks that deal [ModifiedDamage] () magic damage to adjacent enemies.

### Darius (1-cost, ADFighter)

- Traits: Conqueror, Watcher | mana 30/70
- **Decimate**: Spin, dealing [ModifiedDamage] () physical damage to adjacent enemies and healing [ModifiedHeal] (). Apply a [ModifiedBleedDamage] () physical damage bleed to target over 4 seconds.

### Draven (1-cost, ADCarry)

- Traits: Conqueror, Pit Fighter | mana 30/60
- **Spinning Axes**: Passive: If Draven has an empowered axe in hand, his next attack will throw it, dealing a total of [TotalDamage] () physical damage. Empowered axes return to Draven after hitting an enemy. Active: Spin an empowered axe.

### Irelia (1-cost, APTank)

- Traits: Rebel, Sentinel | mana 30/70
- **Defiant Dance**: Enter a defensive stance and gain [ModifiedShield] () Shield that rapidly decays over 3 seconds. When it expires, deal [ModifiedBaseStrikeDamage] () magic damage + 75% of the damage absorbed to enemies around and in front of Irelia.

### Lux (1-cost, APCaster)

- Traits: Academy, Sorcerer | mana 0/50
- **Prismatic Barrier**: Grant [ModifiedShield] () Shield to the lowest current Health ally. Lux's next attack deals [ModifiedDamage] () bonus magic damage.

### Maddie (1-cost, ADCaster)

- Traits: Enforcer, Sniper | mana 20/120
- **Fan the Hammer**: Fire 6 shots towards the farthest enemy that deal [TotalDamage] () physical damage to the first enemy they hit.

### Morgana (1-cost, APCaster)

- Traits: Black Rose, Visionary | mana 0/40
- **Tormented Soul**: Curse the nearest non-cursed enemy, dealing [ModifiedDamage] () magic damage over 10 seconds and reducing the effectiveness of shields used on them by 50%.

### Powder (1-cost, APCasterHighMana)

- Traits: Family, Scrap, Ambusher | mana 0/90
- **Misfit Toy**: Send a monkey towards the largest group of enemies, causing a 2-hex radius explosion on impact. Enemies hit take [ModifiedDamage] () magic damage, reduced by 25% for each hex they are away from the epicenter. Wound and 1% Burn applied for 5 seconds to all enemies hit. Burn: Deal a percent of the target's max Health as true damage every second Wound: Reduce healing received by 33%

### Singed (1-cost, APTank)

- Traits: Chem-Baron, Sentinel | mana 0/50
- **Dangerous Mutations**: Gain [ModifiedDurability] () Durability and grant the ally who has dealt the most damage this round 100/115/130% Attack Speed, decaying over 4 seconds.

### Steb (1-cost, APTank)

- Traits: Enforcer, Bruiser | mana 25/75
- **Field Medicine**: Passive: On heal, heal the 2 closest allies for 25% of the amount. Active: Heal for [ModifiedHeal] () and strike target for [ModifiedDamage] () magic damage.

### Trundle (1-cost, ADTank)

- Traits: Scrap, Bruiser | mana 30/80
- **Desperate Chomp**: Heal [ModifiedHeal] () and chomp target for [ModifiedDamage] () physical damage. Both effects are increased by up to 75% based on Trundle's missing Health.

### Vex (1-cost, APCaster)

- Traits: Rebel, Visionary | mana 0/60
- **Looming Darkness**: Deal [ModifiedDamage] () magic damage to target and create a one-hex radius zone of darkness around them. After a brief delay, deal [ModifiedSecondaryDamage] () magic damage to enemies still in the zone.

### Violet (1-cost, ADFighter)

- Traits: Family, Pit Fighter | mana 20/65
- **1-2-3 Combo**: Jab target 2 times for [ModifiedDamage] () physical damage each. Then uppercut them, dealing [TotalDamage] () physical damage and briefly knocking them up.

### Zyra (1-cost, APCasterHighMana)

- Traits: Experiment, Sorcerer | mana 10/60
- **Grasping Roots**: Send vines towards the current target, Stunning them for 1 second and dealing [ModifiedTargetDamage] () magic damage. Then smaller vines seek out the 2 nearest enemies and deal [ModifiedAOEDamage] () magic damage to them. Experiment Bonus: Ability damage bleeds enemies for [TFTUnitProperty.:TFT13_ZyraCurrentExperimentBonus]% bonus true damage over 2 seconds.

### Akali (2-cost, APReaper)

- Traits: Rebel, Quickstriker | mana 0/55
- **Shuriken Flip**: Throw a shuriken at target, dealing [ModifiedDamage] () magic damage and marking them to take 15% more damage for 4 seconds. Then dash away from target. After a brief delay, dash towards them and deal [ModifiedSecondaryDamage] () magic damage.

### Camille (2-cost, ADFighter)

- Traits: Enforcer, Ambusher | mana 0/25
- **Adaptive Strike**: Kick the target, dealing [TotalDamage] () Adaptive Damage. Heal for 33% of the damage dealt. Adaptive Damage: Uses the damage type the target resists less

### Leona (2-cost, APTank)

- Traits: Academy, Sentinel | mana 20/65
- **Eclipse**: Fortify for 3 seconds, gaining [ModifiedDurability] () Durability. Afterwards, deal [ModifiedDamage] () magic damage to adjacent enemies.

### Nocturne (2-cost, ADReaper)

- Traits: Automata, Quickstriker | mana 0/40
- **Overdrive Blades**: For 6 seconds, attacks also cause adjacent enemies to bleed for [ModifiedDamage] () physical damage over 1 second.

### Rell (2-cost, APTank)

- Traits: Conqueror, Sentinel, Visionary | mana 40/90
- **Shattering Strike**: Gain [ModifiedShield] () Shield for 4 seconds. Lance enemies in a line for [ModifiedDamage] () magic damage and steal 10/12/15 Armor and Magic Resist from enemies hit.

### Renata Glasc (2-cost, APCasterHighMana)

- Traits: Chem-Baron, Visionary | mana 20/85
- **Loyalty Program**: Fire a pair of missiles at target. Allies they pass through gain [ModifiedShield] () Shield for 3 seconds. When they collide, they deal [ModifiedTargetDamage] () magic damage to target and [ModifiedExplosionDamage] () magic damage to adjacent enemies.

### Sett (2-cost, APTank)

- Traits: Rebel, Bruiser, Pit Fighter | mana 50/100
- **Facebreaker**: Pull in an enemy on either side and slam them together, dealing [ModifiedDamage] () magic damage and Stunning them for 1.5/1.5/2 seconds. If only one enemy is grabbed, the damage and Stun duration are increased by 50%.

### Tristana (2-cost, ADCaster)

- Traits: Emissary, Artillerist | mana 20/60
- **Draw a Bead**: Fire a cannonball at target, dealing [TotalDamage] () physical damage. If they die, the cannonball ricochets to the nearest enemy, dealing the overkill damage. When it does, permanently gain [TFTUnitProperty.:TFT13_TristanaASPerStack]% Attack Damage. (Current Bonus: [TFTUnitProperty.:TFT13_TristanaASGain]% )

### Urgot (2-cost, ADFighter)

- Traits: Experiment, Pit Fighter, Artillerist | mana 45/90
- **Corrosive Charge**: Fire an explosive charge, dealing [ModifiedPrimaryDamage] () physical damage to target and [ModifiedSecondaryDamage] () physical damage to adjacent enemies. 20% Sunder all enemies hit for 6 seconds. Sunder: Reduce Armor Experiment Bonus: Dash to targets. On cast, gain [TFTUnitProperty.:TFT13_UrgotCurrentExperimentBonusShield]% max Health Shield and [TFTUnitProperty.:TFT13_UrgotCurrentExperimentBonusAS]% Attack Speed for 5 seconds.

### Vander (2-cost, ADTank)

- Traits: Family, Watcher | mana 0/50
- **Hound of the Underground**: Stop attacking and brace for 2.5 seconds, gaining [ModifiedDefenses] () Armor and Magic Resist. Vander's next attack is replaced with a strike that deals [ModifiedDamage] () physical damage, increased by [ModifiedBonusDamage] () physical damage for each 1 or 2 cost champion on your team.

### Vladimir (2-cost, APTank)

- Traits: Black Rose, Watcher, Sorcerer | mana 0/65
- **Transfusion**: Heal [ModifiedHeal] () and deal [ModifiedDamage] () magic damage to the target.

### Zeri (2-cost, ADCarry)

- Traits: Firelight, Sniper | mana 0/3
- **Living Battery**: Passive: Every 3rd attack is replaced with a spark that deals [TotalDamage] () physical damage to target and [SecondaryDamage] () physical damage to 2 nearby enemies.

### Ziggs (2-cost, APCaster)

- Traits: Scrap, Dominator | mana 15/60
- **Bomb Full of Bombs**: Toss a bomb at target, dealing [ModifiedInitialDamage] () magic damage. 3 mini-bombs fly out, dealing [ModifiedSecondaryDamage] () magic damage to random enemies.

### Blitzcrank (3-cost, APTank)

- Traits: Automata, Dominator | mana 20/70
- **Static Field**: Passive: After surviving damage, deal 3% of the damage absorbed as magic damage to target. Active: Gain [ModifiedShield] () Shield for 4 seconds. Shock the nearest 3 enemies for [ModifiedDamage] () magic damage and reduce their damage by 10% for 4 seconds.

### Cassiopeia (3-cost, APCaster)

- Traits: Black Rose, Dominator | mana 0/40
- **Thorned Miasma**: Blast target for [ModifiedDamage] () magic damage. Every third cast, splash miasma to 2 enemies within 3 hexes, dealing [ModifiedMiasmaDamage] () magic damage to each.

### Ezreal (3-cost, ADCaster)

- Traits: Academy, Rebel, Artillerist | mana 0/60
- **Essence Flux**: Fire a shot towards current target that deals [TotalDamage] () physical damage to all enemies within 1 hex. Then, deal [ModifiedDamage] () physical damage to the unit in the center of the blast.

### Gangplank (3-cost, ADFighter)

- Traits: Scrap, Form Swapper, Pit Fighter | mana 0/10
- **Harvest from Flames**: Restore [ModifiedHeal] () Health. Then slash, dealing [ModifiedDamage] () physical damage to enemies in a line. If only one enemy is hit, the damage is doubled.

### Kog'Maw (3-cost, APCarry)

- Traits: Automata, Sniper | mana 0/40
- **Upgrading Barrage Module**: Passive: Attacks deal [ModifiedDamage] () bonus magic damage. Active: Gain 25% stacking Attack Speed for the rest of combat. After every 3 casts, gain +1 Range.

### Loris (3-cost, APTank)

- Traits: Enforcer, Sentinel | mana 40/80
- **Piltover Bulwark**: Gain [ModifiedShield] () Shield for 4 seconds. It redirects 50% of damage taken by adjacent allies. When it expires, deal [ModifiedDamage] () magic damage in a cone.

### Nami (3-cost, APCaster)

- Traits: Emissary, Sorcerer | mana 0/60
- **Ocean's Ebb**: Launch a wave at target that bounces 3 times to enemies within 3 hexes and deals [ModifiedDamage] () magic damage.

### Nunu & Willump (3-cost, APTank)

- Traits: Experiment, Bruiser, Visionary | mana 50/100
- **ZOMBIE POWER!!						**: For 3 seconds, gain [ModifiedDurability] () Durability and create a 2-hex cloud of noxious fumes that deals [ModifiedDamage] () magic damage to enemies within. Afterwards, detonate the cloud and deal [ModifiedSecondDamage] () magic damage to all enemies within. Experiment Bonus: After dealing damage, deal [TFTUnitProperty.:TFT13_NunuCurrentExperimentBonus]% max bonus magic damage (1s cooldown).

### Renni (3-cost, ADTank)

- Traits: Chem-Baron, Bruiser | mana 40/90
- **Sludgerunner's Smash**: Heal [ModifiedHealing] () over 1.5 seconds. For the duration, raise target into the air, Stunning them, and dealing [ModifiedDamage] () physical damage. Afterwards, slam them down, dealing [FinalDamage] () physical damage to all adjacent enemies.

### Scar (3-cost, APTank)

- Traits: Firelight, Watcher | mana 80/170
- **Sumpsnipe Surprise**: Lob bombs at the nearest 3 enemies, Stunning them for 1.5/1.5/1.75 seconds and dealing [ModifiedDamage] () magic damage to each. Heal [ModifiedHeal] ().

### Smeech (3-cost, ADReaper)

- Traits: Chem-Baron, Ambusher | mana 20/70
- **Scrap Hacker**: Leap towards the enemy with the fewest items within 4 hexes. Slash 3 times, dealing a total of [TotalDamage] () physical damage. If they die, leap again, dealing 30% less damage.

### Swain (3-cost, APTank)

- Traits: Conqueror, Form Swapper, Sorcerer | mana 0/10
- **Demonic Ascension**: Heal [ModifiedHeal] () and ascend for 6 seconds. While ascended, heal [ModifiedHealPerSecond] () and deal [ModifiedDamage] () magic damage to adjacent enemies every second. On takedown, the ascension's duration is extended by 2 seconds.

### Twisted Fate (3-cost, APCasterHighMana)

- Traits: Enforcer, Quickstriker | mana 25/75
- **Wild Cards**: Throw 3 cards at different targets. Blue Card: Restore [ModifiedHeal] () Health to the lowest Health ally. Red Card: Deal [ModifiedRedDamage] () magic damage to the largest circle of enemies. Yellow Card: Deal [ModifiedYellowDamage] () magic damage to target and Stun them for 1 second.

### Ambessa (4-cost, ADFighter)

- Traits: Emissary, Conqueror, Quickstriker | mana 30/70
- **Unrelenting Huntress**: Ambessa switches between two stances on cast: Chains: Gain +1 Range. Attacks deal [ModifiedChainDamage] () physical damage. On cast, dash to target and strike in a half-circle, dealing [ModifiedStrikeDamage] () physical damage to enemies hit. Fists: Gain [ModifiedOmnivamp] () Omnivamp and attack twice as fast. On cast, briefly Stun target before slamming them into the ground, dealing [ModifiedSlamDamage] () physical damage, then dash away.

### Corki (4-cost, ADCaster)

- Traits: Scrap, Artillerist | mana 0/60
- **Broadside Barrage**: Lock onto target and strafe to a nearby position, unleashing 21/21/35 missiles split between the target and all enemies within two hexes. Each missile deals [ModifiedDamage] () physical damage and reduces Armor by 1. Every 7th missile deals [ModifiedSpecialDamage] () physical damage and reduces Armor by [SpecialMissileArmorReduction].

### Dr. Mundo (4-cost, APTank)

- Traits: Experiment, Dominator | mana 30/100
- **Maximum Dosage**: Become energized and heal [ModifiedHeal] () over 2 seconds. While energized, deal [ModifiedDamage] () magic damage to a nearby enemy each second. Afterwards, deal [ModifiedSecondaryDamage] () magic damage to all enemies within 2 hexes. Experiment Bonus: Gain [TFTUnitProperty.:TFT13_DrMundoBaseHealthExperimentBonus] max Health. On each takedown, gain [TFTUnitProperty.:TFT13_DrMundoCurrentExperimentBonus] more max Health.

### Ekko (4-cost, APReaper)

- Traits: Firelight, Scrap, Ambusher | mana 0/50
- **Splitting Seconds**: Summon an assault of afterimages that deals [ModifiedDamage] () magic damage to the target and [ModifiedSecondaryDamage] () magic damage to 2/2/4 other nearby enemies. Afterimages reduce their target's Magic Resist by 5 for the rest of combat.

### Elise (4-cost, APTank)

- Traits: Black Rose, Form Swapper, Bruiser | mana 0/10
- **Cocoon**: Jump to a nearby hex and web all enemies within 2 hexes, Stunning them for 1.75/2/8 seconds and dealing [ModifiedDamage] () magic damage. Heal [ModifiedHeal] ().

### Garen (4-cost, ADTank)

- Traits: Emissary, Watcher | mana 60/125
- **Demacian Justice**: Passive: After dealing damage, heal [ModifiedHeal] (). Active: Gain [ModifiedShield] () Shield for 4 seconds. Slam a massive sword on target, dealing [ModifiedDamage] () physical damage to them and [ModifiedSecondaryDamage] () physical damage to enemies within 2 hexes.

### Heimerdinger (4-cost, APCaster)

- Traits: Academy, Visionary | mana 0/40
- **PROGRESSSSS!**: Fire 5/5/7 missiles at random enemies that deal [ModifiedDamage] () magic damage. Each cast fires 1 more missile than the last.

### Illaoi (4-cost, APTank)

- Traits: Rebel, Sentinel | mana 65/125
- **Test of Spirit**: Gain 50/50/90% Durability for 3 seconds. Over the duration, drain [ModifiedHealthSteal] () Health from the nearest 4/4/10 enemies. Then slam down, dealing [ModifiedDamage] () magic damage to all enemies within 2 hexes.

### Silco (4-cost, APCasterHighMana)

- Traits: Chem-Baron, Dominator | mana 30/80
- **Canned Monstrosity**: Throw a canister at target, dealing [ModifiedDamage] () magic damage to them and releasing 4/4/8 monstrosities. Monstrosities attack 5 times and deal [ModifiedMinionDamage] () magic damage per attack.

### Twitch (4-cost, ADCarry)

- Traits: Experiment, Sniper | mana 0/40
- **Spray and Pray**: For the next 8 attacks, gain 75% Attack Speed, infinite range, and replace attacks with a piercing bolt that targets random enemies. Bolts deal [TotalDamage] () physical damage, reduced by 40/40/10% for each enemy they pass through. Experiment Bonus: After every 5 attacks, deal physical damage to the nearest enemy equal to [TFTUnitProperty.:TFT13_TwitchCurrentExperimentBonus]% of their max Health.

### Vi (4-cost, ADFighter)

- Traits: Enforcer, Pit Fighter | mana 40/100
- **Wrecking Crew**: Gain [ModifiedShield] () Shield for 3 seconds, then Stun target for 1.5 seconds. Slam them down, dealing [ModifiedDamage] () physical damage to them and causing a shockwave in their row. Enemies hit take [ModifiedSecondaryDamage] () physical damage and are briefly knocked up.

### Zoe (4-cost, APCasterHighMana)

- Traits: Rebel, Sorcerer | mana 20/80
- **Paddle Star!**: Launch a star at target that deals [ModifiedDamage] () magic damage. It bounces to the farthest enemy within 4 hexes, then bounces back to the target. This effect repeats 2/2/4 times, hitting a different enemy each time.

### Caitlyn (5-cost, ADCaster)

- Traits: Enforcer, Sniper | mana 0/50
- **Air Raid**: Enter a sniper's stance and call in an airship that circles the battlefield for 5 seconds, dropping 4/4/20 bombs at a random cluster of enemies over the duration. Bombs deal [TotalDamage] ( ) physical damage in a one-hex circle. Whenever an enemy is caught in the epicenter of an Air Raid blast, reduce their Armor and Magic Resist by 20 and fire a shot towards them, dealing [HeadshotDamage] () physical damage.

### Jayce (5-cost, ADFighter)

- Traits: Academy, Form Swapper | mana 0/10
- **Special Delivery**: Passive: Summon a placeable Hextech Forge. On cast, the 3 allies closest to it gain [ModifiedShield] () Shield for 4 seconds. If it's dead, revive it with 100% Health. Active: Summon 2 Hexgates and knock target into one of them, dealing [ModifiedDamage] () physical damage and sending them flying back to their original position. While flying, they deal [ModifiedSecondaryDamage] () physical damage to all other enemies in the path.

### Jinx (5-cost, ADCaster)

- Traits: Rebel, Ambusher | mana 0/50
- **Ruin Everything**: Jinx alternates between Zap, Flame Chompers, and Death Rocket for her ability. Zap: Deal [ZapModifiedDamage] () physical damage to enemies in a line and Stun them for 1.25/1.5/10 seconds. Flame Chompers: Deal [FlameChompersModifiedDamage] () physical damage to 3 1-hex circles of enemies. Death Rocket: Fire a rocket at the center of the board that [DeathRocketModifiedDamage] () physical damage to ALL enemies, reduced by 10% for each hex they are away from the epicenter.

### LeBlanc (5-cost, APCasterHighMana)

- Traits: Black Rose, Sorcerer | mana 45/90
- **The Chains of Fate**: Chain together the nearest 4/4/20 enemies for 5 seconds, dealing [ModifiedDamage] () magic damage split between them. When one takes damage, 12/20/100% of the amount is split as bonus true damage to the others. LeBlanc's next 3 attacks deal [ModifiedAutoDamage] () bonus magic damage, increased by 50% for each enemy killed by the initial damage.

### Malzahar (5-cost, APCasterHighMana)

- Traits: Automata, Visionary | mana 30/90
- **Call of the Machine**: Summon a gate in a 5-hex line across target. Enemies hit take [ModifiedDamage] () magic damage and are 20% Shredded for 4 seconds. Malzahar spreads 5 stacks of infection between enemies hit. Infection deals [ModifiedInfectionDamage] () magic damage per second for the rest of combat. This effect can stack infinitely. When an infected target dies, they spread their stacks to nearby enemies. Shred: Reduce Magic Resist

### Mordekaiser (5-cost, APFighter)

- Traits: Conqueror, Dominator | mana 25/100
- **Grasp of the Iron Revenant**: Briefly gain 40% Durability and summon a massive claw, dealing [ModifiedDamage] () magic damage to the most enemies in a line. The 4 closest enemies take 25% more damage and are pulled towards Mordekaiser. For the next 10 seconds, gain 30% Omnivamp, +1 Attack Range, and replace every attack with a slam that deals [ModifiedTargetDamage] () magic damage to target and [ModifiedSecondaryDamage] () magic damage to all other enemies within 2 hexes.

### Rumble (5-cost, APFighter)

- Traits: Junker King, Scrap, Sentinel | mana 40/120
- **The Equalizer**: Call down a rain of 5 missiles on target's row that each deal [ModifiedDamage] () magic damage, Wound, and 1% Burn units hit for 5 seconds. For each missile that doesn't hit an enemy, restore 20 Mana. If there's only 1 enemy left, fire all of the missiles at them. Burn: Deal a percent of the target's max Health as true damage every second Wound: Reduce healing received by 33% [TFTUnitProperty.:TFT13_JunkerKing_UpgradeTRAKeyWrapper]

### Sevika (5-cost, ADFighter)

- Traits: High Roller, Chem-Baron, Pit Fighter | mana 0/50
- **Beat the Odds**: Randomly cast 1 of 3 spells, with a chance of a Jackpot! Flamethrower: {{Spell_TFT13_LieutenantSpell1_Tooltip}} Extendo-Punch: {{Spell_TFT13_LieutenantSpell2_Tooltip}} Chomp: {{Spell_TFT13_LieutenantSpell3_Tooltip}}

### Mel (6-cost, APReaper)

- Traits: Banished Mage | mana 0/40
- **Conduit of Magic**: Dash to a nearby hex, then gain 300/600/10000 Shield and grant the same Shield to 2 nearby allies. 50% of the damage blocked by the Shields is stored as unstable energy. After dashing, deal [ModifiedDamage] () magic damage to 3 nearby enemies. Every 3rd cast, unleash the unstable energy + [ModifiedSecondaryDamage] () as magic damage split between the 5 nearest enemies.

### Viktor (6-cost, APCasterViktorS13)

- Traits: Machine Herald | mana 0/100
- **Chaos Storm**: Passive: Attacks are replaced with a Death Ray that deals [ModifiedDeathRayDamage] () magic damage and 25/90/1000 true damage in a 2-hex line. Active: Summon a chaos storm that engulfs the battlefield, knocking up ALL enemies into the air for 2/3/30 seconds. At the end of the duration, slam them to the ground, dealing [ModifiedSpellDamage] () plus 3/10/100% of their max Health as magic damage.

### Warwick (6-cost, ADFighter)

- Traits: Experiment, Blood Hunter | mana 60/100
- **Blood Hunt**: Passive: While Bloodfrenzied, move faster, gain [PercentOmnivamp]% () Omnivamp, 40% Attack Speed, and deal [ModifiedDamage] () physical damage each second to target. After killing 5 enemies, become Unstoppable and Bloodfrenzied for the rest of combat. Active: Become Bloodfrenzied for 4 seconds. Experiment Bonus: On kill, Stun enemies adjacent to the dead target for [TFTUnitProperty.:TFT13_WarwickCurrentExperimentBonus] second.

## Monsters, summons, pets and other trait-less units (25)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, ADTank)

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, ADTank)

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, ADTank)

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### ApheliosTurret (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Blue_Monkey (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### EliseSpider (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### EliseSpiderling (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Hextech Forge (11-cost, )

- Traits: (none)
- **Placeholder Name**: Placeholder Tooltip

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### SRU_Horde_Mini (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Sion (11-cost, )

- Traits: (none) | mana 120/120
- **Decimating Smash**: Freed: Charge through the nearest enemy, dealing [ModifiedChargeDamage] () physical damage to all enemies hit. Active: Gain [ModifiedShield] () Shield for 3 seconds. Stun enemies in a line on target for 1 seconds and deal [ModifiedDamage] () physical damage to them.

### SwainDemonForm (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip


<a id="set-14"></a>

# Set 14: Cyber City

Source: current client data. 94 champions, 26 traits.

## Traits

### A.M.P.

A.M.P. champions upgrade their abilities in unique ways with Amp (%i:set14AmpIcon%). They also gain Health.

  - (2) HPAmt=100, SuperchargeAmount=1
  - (3) HPAmt=200, SuperchargeAmount=2
  - (4) HPAmt=300, SuperchargeAmount=4
  - (5) HPAmt=500, SuperchargeAmount=5

### Anima Squad

At each tier, pick a weapon that a random Anima Squad champion fires periodically during combat. Anima Squad champions gain Armor, Magic Resist, and Damage Amp. Weapons scale with Anima Squad star level and stage.

  - (3-4) DamageAmp=0.05, Resists=10, 4, 0.06
  - (5-6) DamageAmp=0.1, Resists=25, 4, 0.06
  - (7-9) DamageAmp=0.15, Resists=40, 4, 0.06
  - (10) DamageAmp=0.25, Resists=75, 4, 0.06

### Bastion

Your team gains [TeamwideArmor] Armor and Magic Resist. Bastions gain more. For the first [Duration] seconds of combat, Bastions increase their bonus by [StatMultiplier]%.

  - (2-3) BonusArmor=18, BonusMR=18, Duration=10, EnhancedTeamwideArmor=25, StatMultiplier=1, TeamwideArmor=10, 20
  - (4-5) BonusArmor=36, BonusMR=36, Duration=10, EnhancedTeamwideArmor=25, StatMultiplier=1, TeamwideArmor=10, 20
  - (6) BonusArmor=70, BonusMR=70, Duration=10, EnhancedTeamwideArmor=25, StatMultiplier=1, TeamwideArmor=10, 20

### BoomBot

BoomBots fire a missile every [DamageDealtThreshold] damage dealt at a nearby enemy that deals magic damage. [DamageTakenMod]% of damage taken contributes to damage dealt.

  - (2-3) DamageDealtThreshold=400, DamageTakenMod=0.08, MissileDamage=150
  - (4-5) DamageDealtThreshold=400, DamageTakenMod=0.08, MissileDamage=325
  - (6) DamageDealtThreshold=400, DamageTakenMod=0.08, MissileDamage=260

### Bruiser

Your team gains [TeamwideHealth] Health. Bruisers gain more.

  - (2-3) HealthBonus=0.2, TeamwideHealth=100, 20
  - (4-5) HealthBonus=0.45, TeamwideHealth=100, 20
  - (6) HealthBonus=0.7, TeamwideHealth=100, 20

### Cyberboss

Your strongest Cyberboss upgrades to its final form and gains Health, Ability Power, and its ability hits more enemies.

  - (2) BigBossAP=40, BigBossPercentHealth=0.4, BossAP=20, BossPercentHealth=0.25, DamageMod=0.3, 0.8, 0.35
  - (3) BigBossAP=40, BigBossPercentHealth=0.4, BossAP=30, BossPercentHealth=0.33, DamageMod=0.3, 0.8, 0.35
  - (4) BigBossAP=40, BigBossPercentHealth=0.4, BossAP=30, BossPercentHealth=0.33, DamageMod=0.3, 0.8, 0.35

### Cypher

Gain Intel by losing combat, increased for loss streaks. Gain a small amount for killing enemies. You may trade your Intel for loot one time on round [CashoutStage1]-[CashoutRound1], [CashoutStage2]-[CashoutRound2], [CashoutStage3]-[CashoutRound3], [CashoutStage4]-[CashoutRound4], or [CashoutStage5]-[CashoutRound5]. After trading Intel, Cypher champions gain Attack Damage and Ability Power.

  - (3) AbilityPower=30, AttackDamage=0.3, CashoutRound1=None, CashoutRound2=None, CashoutRound3=None, CashoutRound4=None, CashoutRound5=None, CashoutStage1=None, CashoutStage2=None, CashoutStage3=None, CashoutStage4=None, CashoutStage5=None, ValueMultiplier=1, 0.03, 0.5, 5, None, None, 2, None
  - (4) AbilityPower=50, AttackDamage=0.5, CashoutRound1=None, CashoutRound2=None, CashoutRound3=None, CashoutRound4=None, CashoutRound5=None, CashoutStage1=None, CashoutStage2=None, CashoutStage3=None, CashoutStage4=None, CashoutStage5=None, ValueMultiplier=1.5, 0.03, 0.5, 5, None, None, 2, None
  - (5) AbilityPower=70, AttackDamage=0.7, CashoutRound1=None, CashoutRound2=None, CashoutRound3=None, CashoutRound4=None, CashoutRound5=None, CashoutStage1=None, CashoutStage2=None, CashoutStage3=None, CashoutStage4=None, CashoutStage5=None, ValueMultiplier=2, 0.03, 0.5, 5, None, None, 2, None
  - (6) AbilityPower=90, AttackDamage=0.9, CashoutRound1=None, CashoutRound2=None, CashoutRound3=None, CashoutRound4=None, CashoutRound5=None, CashoutStage1=None, CashoutStage2=None, CashoutStage3=None, CashoutStage4=None, CashoutStage5=None, ValueMultiplier=2.5, 0.03, 0.5, 5, None, None, 2, None

### Divinicorp

Divinicorp champions grant unique stats to your team, increased for each Divinicorp in play. Divinicorp champions gain double. Morgana - [TFTUnitProperty.trait:TFT14_Trait_Divinicorp_AP]% Rhaast - [TFTUnitProperty.trait:TFT14_Trait_Divinicorp_Defenses] Senna - [TFTUnitProperty.trait:TFT14_Trait_Divinicorp_AD]% Gragas - [TFTUnitProperty.trait:TFT14_Trait_Divinicorp_Health] Vex - [TFTUnitProperty.trait:TFT14_Trait_Divinicorp_Crit]% Renekton - [TFTUnitProperty.trait:TFT14_Trait_Divinicorp_AS]% Emblem - [TFTUnitProperty.trait:TFT14_Trait_Divinicorp_Omnivamp]%

  - (1) DivinicorpBonus=1, 9, 3, 7, 5, 1, 8, 50, 6
  - (2) DivinicorpBonus=1.1, 9, 3, 7, 5, 1, 8, 50, 6
  - (3) DivinicorpBonus=1.25, 9, 3, 7, 5, 1, 8, 50, 6
  - (4) DivinicorpBonus=1.4, 9, 3, 7, 5, 1, 8, 50, 6
  - (5) DivinicorpBonus=1.65, 9, 3, 7, 5, 1, 8, 50, 6
  - (6) DivinicorpBonus=1.9, 9, 3, 7, 5, 1, 8, 50, 6
  - (7) DivinicorpBonus=2.1, 9, 3, 7, 5, 1, 8, 50, 6

### Dynamo

Every [Timer] seconds, your team gains Mana. Dynamos gain [ThirstyBonus]% more.

  - (2) Mana=4, ThirstyBonus=1, Timer=3, 20
  - (3) Mana=6, ThirstyBonus=1, Timer=3, 20
  - (4) Mana=10, ThirstyBonus=1, Timer=3, 20

### Executioner

Executioner Abilities can critically strike. They also gain bonus Critical Strike Chance and Critical Strike Damage. If the target's Health is below [HealthThreshold]%, the bonus Critical Strike Damage is doubled.

  - (2) CRIT_DAMAGE=0.05, CRIT_PERCENT=0.25, DamageReduction=None, HealthThreshold=0.2, 20
  - (3) CRIT_DAMAGE=0.1, CRIT_PERCENT=0.35, DamageReduction=None, HealthThreshold=0.2, 20
  - (4) CRIT_DAMAGE=0.15, CRIT_PERCENT=0.5, DamageReduction=None, HealthThreshold=0.2, 20
  - (5) CRIT_DAMAGE=0.15, CRIT_PERCENT=0.55, DamageReduction=0.12, HealthThreshold=0.2, 20

### Exotech

Gain unique items that can only be equipped by Exotech champions. They gain Health and Attack Speed for each item equipped.

  - (3-4) ASBonus=2, HealthBonus=50, 20
  - (5-6) ASBonus=6, HealthBonus=160, 20
  - (7-9) ASBonus=13, HealthBonus=270, 20
  - (10) ASBonus=40, HealthBonus=500, 20

### God of the Net

After [Rounds] player combats, open an Armory of Trait Mods that permanently reprogram a champion to benefit from a trait (but not contribute). Every time you get a Trait Mod, the next one requires [RoundIncrement] additional round. Rounds remaining: [TFTUnitProperty.trait:TFT14_Netgod_RoundsRemaining] Champions can only have one Trait Mod. Equipped Mods: [TFTUnitProperty.trait:TFT14_Netgod_Trait1] - [TFTUnitProperty.trait:TFT14_Netgod_Champ1] [TFTUnitProperty.trait:TFT14_Netgod_Trait2] - [TFTUnitProperty.trait:TFT14_Netgod_Champ2] [TFTUnitProperty.trait:TFT14_Netgod_Trait3] - [TFTUnitProperty.trait:TFT14_Netgod_Champ3] [TFTUnitProperty.trait:TFT14_Netgod_Trait4] - [TFTUnitProperty.trait:TFT14_Netgod_Champ4] [TFTUnitProperty.trait:TFT14_Netgod_Trait5] - [TFTUnitProperty.trait:TFT14_Netgod_Champ5] [TFTUnitProperty.trait:TFT14_Netgod_Trait6] - [TFTUnitProperty.trait:TFT14_Netgod_Champ6] [TFTUnitProperty.trait:TFT14_Netgod_Trait7] - [TFTUnitProperty.trait:TFT14_Netgod_Champ7] [TFTUnitProperty.trait:TFT14_Netgod_Trait8] - [TFTUnitProperty.trait:TFT14_Netgod_Champ8] [TFTUnitProperty.trait:TFT14_Netgod_Trait9] - [TFTUnitProperty.trait:TFT14_Netgod_Champ9]

  - (1) RoundIncrement=1, rounds=2, 9

### Golden Ox

Golden Ox gain Damage Amp and have a chance to drop gold on kill. If you spend [TFTUnitProperty.trait:TFT14_Immortal_GoldNeededForBreakpoint] gold on rerolls or XP in a single turn, permanently increase their Damage Amp and the gold required for the next bonus. Rerolls count double towards gold spent. (Golden Ox must have fought last round) Current Bonus: [TFTUnitProperty.trait:TFT14_Immortal_GoldBonusStacks]%

  - (2-3) ComponentDropChance=None, DamageAmp=0.12, InitialGoldBreakpoint=None, KillDrop=0.2, 1, 1, 0.08, None, 7, None
  - (4-5) ComponentDropChance=None, DamageAmp=0.24, InitialGoldBreakpoint=None, KillDrop=0.6, 1, 1, 0.08, None, 4, None
  - (6) ComponentDropChance=0.06, DamageAmp=0.25, InitialGoldBreakpoint=None, KillDrop=1, 1, 1, 0.08, None, 3, None

### Marksman

Marksmen gain Attack Damage. After [EnhancedTime] seconds of combat, they increase their bonus by [EnhancedBonus]%.

  - (2-3) BonusAD=0.18, EnhancedBonus=1, EnhancedTime=8, RepeatBonus=None, RepeatTime=None, TeamBonus=None, 20
  - (4) BonusAD=0.35, EnhancedBonus=1, EnhancedTime=8, RepeatBonus=0.25, RepeatTime=6, TeamBonus=None, 20

### Nitro

Every round, unique Nitro champions grant Chrome to R-080T, based on their star level. Each Chrome grants [HealthPerScrap] Health and [APPerScrap] Ability Power. At [FourStarScrapThreshold] Chrome, it upgrades to T-43X! Chrome per star: [ScrapPerOneStar]/[ScrapPerTwoStar]/[ScrapPerThreeStar]/[ScrapPerFourStar]

  - (3) APPerScrap=1, FourStarScrapThreshold=200, HealthPerScrap=14, ScrapPerFourStar=None, ScrapPerOneStar=None, ScrapPerThreeStar=None, ScrapPerTwoStar=None, 300, None, 800, 130, 60, 500, 100
  - (4) APPerScrap=1, FourStarScrapThreshold=200, HealthPerScrap=14, ScrapPerFourStar=None, ScrapPerOneStar=None, ScrapPerThreeStar=None, ScrapPerTwoStar=None, 300, None, 800, 130, 60, 500, 100

### Overlord

The Overlord takes a bite out of the unit in the hex behind him, dealing [HPPercentDamage]% of their max Health as true damage. He gains [PercentHPGain]% of their Health and [PercentADGain]% of their Attack Damage.

  - (1) HPPercentDamage=0.4, NumHexes=1, PercentADGain=0.25, PercentHPGain=0.4, 1, 0.33, 1

### Rapidfire

Your team gains [TeamBonus]% Attack Speed. Rapidfire champions gain more on each attack, stacking up to [MaxStacks] times.

  - (2-3) AttackSpeed=0.04, MaxStacks=10, TeamBonus=0.1, 20
  - (4-5) AttackSpeed=0.11, MaxStacks=10, TeamBonus=0.1, 20
  - (6) AttackSpeed=0.22, MaxStacks=10, TeamBonus=0.1, 20

### Slayer

Slayers gain Attack Damage and Omnivamp. Overhealing heals the lowest percent Health Slayer for [OverhealPercent]% of the excess amount.

  - (2-3) AttackDamage=0.15, Healing=0.15, OverhealPercent=0.5, 20
  - (4-5) AttackDamage=0.4, Healing=0.15, OverhealPercent=0.5
  - (6) AttackDamage=0.7, Healing=0.2, OverhealPercent=0.5

### Sniper

Snipers deal more damage to targets farther away.

  - (1) BaseDamageAmp=None, BonusHexRangeIncrease=None, PercentDamageIncrease=1

### Soul Killer

Gain a hologram copy of the highest cost enemy Viego helped kill last round. It has [Level1Health]/[Level2Health]/[Level3Health] Health, deals [Level1Damage]%/[Level2Damage]%/[Level3Damage]% damage, and has [ItemCount] recommended item.

  - (1) ItemCount=1, Level1Damage=0.3, Level1Health=900, Level2Damage=0.4, Level2Health=1350, Level3Damage=2, Level3Health=9001

### Strategist

Combat Start: Allies in the back 2 rows gain Damage Amp. Allies in the front 2 rows gain Durability. Strategists get triple.

  - (2) DamageAmp=0.06, Durability=0.04, 2
  - (3) DamageAmp=0.09, Durability=0.06, 2
  - (4) DamageAmp=0.12, Durability=0.08, 2
  - (5) DamageAmp=0.15, Durability=0.1, 2

### Street Demon

Allies in painted hexes gain Health, Ability Power, and Attack Damage. Some hexes are Signature hexes and grant [EmpoweredHexPercentBonus]% more. Street Demons double all bonuses.

  - (3-4) ADAP=6, EmpoweredHexPercentBonus=0.5, HP=0.06, 2, 20
  - (5-6) ADAP=10, EmpoweredHexPercentBonus=0.5, HP=0.1, 2, 20
  - (7-9) ADAP=16, EmpoweredHexPercentBonus=0.5, HP=0.16, 2, 20
  - (10) ADAP=45, EmpoweredHexPercentBonus=0.5, HP=0.45, 2, 20

### Syndicate

Get a Kingpin hat that uniquely upgrades a Syndicate champion's ability. Syndicate champions gain Health and Damage Amp.

  - (3-4) DamageAmp=0.05, MobHealth=100, NumBosses=1, 1
  - (5-6) DamageAmp=0.18, MobHealth=450, NumBosses=2, 2
  - (7) DamageAmp=0.2, MobHealth=550, NumBosses=2, 2

### Techie

Techies gain Ability Power. Enemies hit by their abilities deal [DamageAmpDebuff]% less damage for [DebuffDuration] seconds.

  - (2-3) APBonus=20, DamageAmpDebuff=0.1, DebuffDuration=3, EnhancedDebuff=0.18
  - (4-5) APBonus=50, DamageAmpDebuff=0.1, DebuffDuration=3, EnhancedDebuff=0.18
  - (6-7) APBonus=95, DamageAmpDebuff=0.1, DebuffDuration=3, EnhancedDebuff=0.18
  - (8) APBonus=140, DamageAmpDebuff=0.1, DebuffDuration=3, EnhancedDebuff=0.18

### Vanguard

Vanguards gain [DamageReductionPct]% Durability while Shielded. Combat start and [HealthThreshold]% Health: Gain a max Health Shield for [ShieldDuration] seconds.

  - (2-3) DamageReductionPct=0.12, EnhancedDurability=0.18, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.16
  - (4-5) DamageReductionPct=0.12, EnhancedDurability=0.18, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.32
  - (6) DamageReductionPct=0.12, EnhancedDurability=0.18, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.45

### Virus

The Virus infects your shop with a [ChanceToSee]% chance to spawn a bloblet. When purchased, it merges and increases the strongest Zac's max Health by [BonusPercentHPPerStack]% and Ability Power by [BonusAPPerStack] (doubled in Hyper Roll). (Total Bloblets: [TFTUnitProperty.trait:TFT14_Virus_Stacks])

  - (1) BonusAPPerStack=1, BonusPercentHPPerStack=0.015, ChanceToSee=20

## Champions (60)

### Alistar (1-cost, APTank)

- Traits: Golden Ox, Bruiser | mana 40/80
- **Gilded Endurance**: Passive: Reduce all incoming damage by [ModifiedDamageReduction] (). Active: Deal [ModifiedDamage] () magic damage to the current target and Stun them for 2 seconds.

### Dr. Mundo (1-cost, ADTank)

- Traits: Street Demon, Bruiser, Slayer | mana 25/75
- **Flex On 'Em**: Passive: Gain 30% more max Health from all sources. (Bonus Health: [TFTUnitProperty.unit:TFT14_DrMundo_TotalBonusHealth]) Active: Flex, healing [TotalHealing] () Health, then deal [TotalDamage] () physical damage to the current target.

### Jax (1-cost, APTank)

- Traits: Exotech, Bastion | mana 20/80
- **Ion Barrier**: Gain [ModifiedShield] () Shield for 4 seconds and deal [ModifiedDamage] () magic damage to adjacent enemies.

### Kindred (1-cost, ADCaster)

- Traits: Nitro, Rapidfire, Marksman | mana 0/50
- **Rev Up**: Fire wolf at the current target, dealing [TotalDamage] () physical damage. Overkill damage bounces to the nearest enemy.

### Kog'Maw (1-cost, ADCarry)

- Traits: BoomBot, Rapidfire | mana 0/40
- **Fire at Will**: For the next 5 seconds, gain 50% Attack Speed and attacks deal [TotalDamage] () bonus physical damage.

### Morgana (1-cost, APCaster)

- Traits: Divinicorp, Dynamo | mana 0/40
- **Short Circuit**: Hack the target and the nearest non-hacked enemy, dealing [ModifiedDamage] () magic damage over 16 seconds.

### Nidalee (1-cost, APFighter)

- Traits: Nitro, A.M.P. | mana 20/60
- **Charged Claws**: Deal [ModifiedDamage] () magic damage to the current target and [ModifiedAmpedDamage] () magic damage to the [ModifiedNumTargets] (%i:set14AmpIcon%) nearest enemies. Heal 100/125/210 Health + 20/25/40 per enemy hit.

### Poppy (1-cost, APTank)

- Traits: Cyberboss, Bastion | mana 20/65
- **Buckler Toss**: Throw a buckler at the farthest enemy within 4 hexes, dealing [TotalDamage] () magic damage to them and [BossDamage] () magic damage to enemies within 1 hex. Gain [ModifiedShield] () Shield.

### Seraphine (1-cost, APCasterHighMana)

- Traits: Anima Squad, Techie | mana 0/60
- **Ping()**: Fire a soundwave through the current target that deals [ModifiedDamage] () magic damage, reduced by 40% for each enemy hit.

### Shaco (1-cost, ADReaper)

- Traits: Syndicate, Slayer | mana 0/50
- **Felony Fantastique**: Passive: Gain 1.5% Attack Damage for each kill this game. (Kills: [TFTUnitProperty.unit:TFT14_ShacoADStacks], [TFTUnitProperty.unit:TFT14_ShacoTotalStackAD]% ) Active: Deal [TotalDamage] () physical damage to the current target. Kingpin bonus: Teleport behind the lowest Health enemy within 3 hexes and deal [ModifiedT1Damage] () bonus physical damage[ModifiedT3Damage] () bonus physical damage. If this kills, immediately cast again with 75% damage.

### Sylas (1-cost, APTank)

- Traits: Anima Squad, Vanguard | mana 25/75
- **Chains of the Pack**: Gain [ModifiedHealth] () max Health and deal [ModifiedDamage] () magic damage to the current target. If they die, gain an additional 75/100/125 Health.

### Vi (1-cost, ADTank)

- Traits: Cypher, Vanguard | mana 30/65
- **Block Chain**: Gain [TotalShield] () Shield that rapidly decays over 4 seconds. Vi's next attack deals [ModifiedDamage] () physical damage.

### Zyra (1-cost, APCasterHighMana)

- Traits: Street Demon, Techie | mana 0/60
- **Stroke of Brilliance**: Send a large vine towards the current target, Stunning them for 1 second and dealing [ModifiedTargetDamage] () magic damage. Send a smaller vine at the nearest target that deals [ModifiedSecondaryDamage] () magic damage. Grant all allies 1% Attack Speed.

### Darius (2-cost, ADTank)

- Traits: Syndicate, Bruiser | mana 30/80
- **Kneecapper**: Heal [TotalHeal] () Health, then deal [Damage] () physical damage and 20% Sunder enemies within 1 hex for 6 seconds. Kingpin bonus: Increase Ability range to 24 hexes. Damage reduced by 5% per hex away. Heal [TotalBossHeal] () Health for each enemy hit. {{TFT_Keyword_Sunder}}

### Ekko (2-cost, APTank)

- Traits: Street Demon, Strategist | mana 30/70
- **Paintover**: Heal [ModifiedHealActive] () Health and deal [ModifiedDamage] () magic damage to the current target.

### Graves (2-cost, ADReaper)

- Traits: Golden Ox, Executioner | mana 0/2
- **Buckshot of Midas**: Passive: Attacks fire 5 projectiles in a cone that deal [ModifiedProjectileDamage] () physical damage each. Graves casts every 2 attacks. Active: Dash next to the target and quickly fire 2 empowered shells that deal [ModifiedEmpoweredProjectileDamage] () physical damage per projectile.

### Illaoi (2-cost, APTank)

- Traits: Anima Squad, Bastion | mana 0/60
- **Bionic Battle Bear**: Heal [ModifiedHeal] () Health. Deal [ModifiedDamage] () magic damage the current target and [ModifiedSplashDamage] () to enemies within 1-hex. Reduce their Magic Resist by 15 for the rest of combat.

### Jhin (2-cost, ADCaster)

- Traits: Exotech, Marksman, Dynamo | mana 14/64
- **Logic Bomb**: Fire a bouncing grenade that hits four enemies, dealing [BounceDamage] () physical damage and 20% Sundering them for [ModifiedSunderDuration] () seconds. The fourth bounce returns to the initial target and deals [FinalBounceDamage] () physical damage. {{TFT_Keyword_Sunder}}

### LeBlanc (2-cost, APCasterHighMana)

- Traits: Cypher, Strategist | mana 0/50
- **Data Spike**: Send [ModifiedSigilCount] sigils, alternating between the current target and the next nearest enemy, each dealing [ModifiedDamage] () magic damage. Every cast, send 1 more sigil.

### Naafiri (2-cost, ADFighter)

- Traits: Exotech, A.M.P. | mana 0/50
- **Alpha Protocol**: Passive: Gain [ModifiedOmnivamp] () Omnivamp. Active: Deal [ModifiedDamage] () physical damage to the current target. Then, summon [ModifiedNumPackmates] packmates that deal [PackmateDamage] () physical damage and [ModifiedNumGigaDogs] (%i:set14AmpIcon%) Alphas that deal [ModifiedGigaDogDamage] () physical damage to the current target.

### Rhaast (2-cost, ADTank)

- Traits: Divinicorp, Vanguard | mana 40/80
- **Reaping Slash**: Knock the target into the air for 1.75 seconds. Over the duration, heal [ModifiedHeal] () Health and deal [TotalDamage] () physical damage to the target.

### Shyvana (2-cost, APTank)

- Traits: Nitro, Bastion, Techie | mana 50/100
- **Burnout**: First cast: Every second for the rest of combat, heal [ModifiedHealPerTick] () Health and deal [ModifiedDamage] () magic damage to adjacent enemies. Gain [ModifiedHealth] () max Health and 10% Damage Amp.

### Skarner (2-cost, APTank)

- Traits: BoomBot, Vanguard | mana 25/70
- **Missile Defense**: Gain [TotalShield] () Shield for 3 seconds and dredge up a missile. When the Shield ends, hurl the missile at the group of most enemies within [ModifiedCastRange] () hexes, dealing [TotalDamage] () magic damage to the centermost enemy, and 50% of the damage to enemies adjacent to them.

### Twisted Fate (2-cost, APCarry)

- Traits: Syndicate, Rapidfire | mana 10/70
- **Ace High**: Passive: Gain 1.5 Ability Power each Attack. Active: Throw a card at the current target and next nearest target, dealing [Damage] () magic damage. Kingpin bonus: Throw a random special card at the current target that deals [ModifiedBonusDamage] () magic damage.Throw two special cards at two targets that deal [ModifiedBonusDamage] () magic damage: one Red and one Blue. Red card hits enemies in a 1-hex area. Blue card deals 50% of its damage as true damage. Rarely, throw a Yellow card that grants gold.

### Vayne (2-cost, ADCarry)

- Traits: Anima Squad, Slayer | mana 25/75
- **Bat Bolts**: Rapidly fire 3 attacks. The first two deal [ModifiedTrueDamage] () bonus true damage, and the final attack deals [ModifiedFinalHitDamage] () bonus true damage.

### Veigar (2-cost, APCaster)

- Traits: Cyberboss, Techie | mana 0/40
- **Cyber Supremacy**: Deal [ModifiedDamage] () magic damage to the current target and [BossDamage] () magic damage to adjacent enemies. If Veigar is the same star level as the target, deal 25% true damage. If Veigar is a higher star level, deal 40% true damage.

### Braum (3-cost, APTank)

- Traits: Syndicate, Vanguard | mana 30/90
- **Protection Racket**: Gain [ModifiedShield] () Shield for 4 seconds. When it breaks, deal [ModifiedDamage] () magic damage to the 3 closest enemies and Chill them for 3 seconds. Kingpin bonus: Gain 3080 Armor and Magic Resist. Every 42 seconds, Braum's next attack deals [ModifiedBossDamage] () bonus magic damage. {{TFT_Keyword_Chill}}

### Draven (3-cost, ADCaster)

- Traits: Cypher, Rapidfire | mana 30/120
- **Cache Hack**: Hurl two massive axes at the most enemies in a line, dealing [BigOutModifiedDamage] () physical damage before returning. Damage is reduced by 20% for each enemy hit.

### Elise (3-cost, APCasterHighMana)

- Traits: Nitro, Dynamo | mana 0/50
- **Fire on all Cylinders**: Fire 4 lasers at the current target and 4 more split between the nearest 2 enemies, each dealing [ModifiedDamage] () magic damage and reducing Magic Resist by 5 for the rest of combat.

### Fiddlesticks (3-cost, APFighter)

- Traits: BoomBot, Techie | mana 0/45
- **3RR0R**: For 3 seconds, tether to the closest 3 enemies, dealing [ModifiedDamagePerSecond] () magic damage per second split between them. Tethers apply 20% Shred and heal Fiddlesticks for 15% of the damage dealt. {{TFT_Keyword_Shred}}

### Galio (3-cost, APTank)

- Traits: Cypher, Bastion | mana 20/80
- **Stonewall**: Gain [ModifiedDurability] () Durability for 3 seconds. Afterwards, heal [ModifiedHeal] () Health and send a whirlwind at the enemy within 3 hexes that dealt the most damage to Galio during that time. It deals [ModifiedDamage] () magic damage and Stuns for 1.5 seconds.

### Gragas (3-cost, APTank)

- Traits: Divinicorp, Bruiser | mana 20/70
- **Corporate Mixer**: Heal [ModifiedHeal] () Health, then throw a cask at the largest group of enemies within 3 hexes, dealing [ModifiedDamage] () magic damage to enemies within 1 hex.

### Jarvan IV (3-cost, ADTank)

- Traits: Golden Ox, Vanguard, Slayer | mana 25/75
- **Half-Moon Sweep**: Strike the most enemies in a semi-circle, gaining [ModifiedShield] () Shield for 4 seconds plus [ModifiedBonusShield] () per enemy hit. Deal [ModifiedSweepDamage] () physical damage to enemies hit and reduce their Armor by 15/18/20 for the rest of combat.

### Jinx (3-cost, ADCaster)

- Traits: Street Demon, Marksman | mana 0/50
- **Rampant Vandalism**: Fire [ModifiedRockets] rockets at random enemies within 2 hexes of the current target. Each rocket deals [TotalDamage] () physical damage. Each cast fires an additional rocket.

### Mordekaiser (3-cost, APTank)

- Traits: Exotech, Bruiser, Techie | mana 30/80
- **Antivirus**: Gain [ModifiedShield] () Shield for 4 seconds. Slam the current target, dealing [ModifiedDamage] () magic damage in a 2-hex line and Stun all enemies hit for 1.25 seconds.

### Rengar (3-cost, ADReaper)

- Traits: Street Demon, Executioner | mana 0/60
- **Freestyle**: Heal [ModifiedHealAmount] () Health and leap to the lowest Health enemy in [ModifiedHexRange] hexes, dealing [ModifiedDamageAmount] () physical damage. Then, strike 2 times, each dealing [ModifiedSlashDamage] () physical damage. Each cast increases leap range by 1 hex.

### Senna (3-cost, ADCaster)

- Traits: Divinicorp, Slayer | mana 0/100
- **Ion Beam**: Passive: Attacks deal [TotalPassiveDamage] () physical damage to enemies adjacent to the target. Active: Fire a beam two hexes beyond the current target, dealing [TotalSpellDamage] () physical damage to enemies hit.

### Varus (3-cost, APCasterHighMana)

- Traits: Exotech, Executioner, Sniper | mana 20/70
- **Malware Chain**: Fire a chain that deals [MainTargetModifiedDamage] () magic damage and Stuns for 1 seconds. The chain links to the 3 closest enemies, dealing [SecondaryTargetModifiedDamage] () magic damage to each.

### Yuumi (3-cost, APCaster)

- Traits: Anima Squad, A.M.P., Strategist | mana 0/30
- **Yuum.AI**: Passive: Gain [TotalManaReduction] (%i:set14AmpIcon%) Mana every second. Active: Launch a magic missile at the current target that bounces to the nearest enemy. Deal [ModifiedDamage] () magic damage to the current target and [ModifiedSecondaryDamage] () to the other, marking both of them. If they're already marked, deal 190% damage.

### Annie (4-cost, APCasterHighMana)

- Traits: Golden Ox, A.M.P. | mana 0/40
- **Goldilocks**: Throw a fireball at the current target that deals [ModifiedMainDamage] () magic damage and [TotalNumFireballs] (%i:set14AmpIcon%) mini-fireballs split between the target and 2 nearby enemies that deal [ModifiedBonusDamage] () magic damage each. Every 4 casts, instead, summon Tibbers (%i:set14AmpIcon%) next to the current target, dealing [ModifiedTibbersDamage] () magic damage to enemies within 1 hex.

### Aphelios (4-cost, ADCarry)

- Traits: Golden Ox, Marksman | mana 0/50
- **Moonlight Vigil**: Fire a moon blast at the largest group of enemies that deals [TotalBlastDamage] () physical damage to enemies within 2 hexes. Equip 4 Chakram, plus 1 more for each enemy hit by the blast, that last for 8 attacks and deal [TotalChakramDamage] () bonus physical damage each.

### Brand (4-cost, APCasterHighMana)

- Traits: Street Demon, Techie | mana 10/70
- **Paint Bomb**: Throw a paint bomb at the largest group of enemies within [ModifiedCastRange] () hexes, dealing [ModifiedDamage] () magic damage to enemies within 1 hex and [ModifiedSecondaryDamage] () magic damage to the nearest 4 enemies.

### Cho'Gath (4-cost, APTank)

- Traits: BoomBot, Bruiser | mana 60/100
- **Sonic Scream**: Fire a disorienting sound blast in a cone, dealing [TotalDamage] () magic damage. Gain [TFTUnitProperty.trait:TFT14_ChogathTotalHealth] () max Health, get bigger, and increase the size of future sound blasts.

### Leona (4-cost, APTank)

- Traits: Anima Squad, Vanguard | mana 60/120
- **Solar Flare**: Gain [ModifiedDurability] () Durability for 4 seconds. Deal [ModifiedDamage] () magic damage to the largest clump of enemies within 2 hexes and Stun the center enemy for 2 seconds. Enemies damaged by this ability take [MagicResistDamage] () bonus magic damage from allies' attacks for 4/4/10 seconds.

### Miss Fortune (4-cost, ADCaster)

- Traits: Syndicate, Dynamo | mana 40/135
- **Street Sweeper**: Fire 9 waves of bullets towards the largest clump of enemies over 2 seconds. Enemies struck take [ModifiedDamage] () physical damage from the first bullet of each wave, and 25% less from other bullets in the same wave. Kingpin bonus: Fire 3 more waves. Bullets pierce enemies for 40% damage.

### Neeko (4-cost, APTank)

- Traits: Street Demon, Strategist | mana 30/90
- **Pop Up Mural**: Gain [ModifiedTotalShield] () Shield for 4 seconds and create an expanding zone that pops three times, each dealing [ModifiedDamage] () magic damage to enemies hit. The third pop Chills enemies for 3 seconds. {{TFT_Keyword_Chill}}

### Sejuani (4-cost, APTank)

- Traits: Exotech, Bastion | mana 50/140
- **Blue Screen**: Passive: Gain 30% more Armor and Magic Resist from all sources. Active: Throw an EMP at the current target that explodes after 2 seconds. It deals [ModifiedDamage] () magic damage to enemies within 3 hexes and Stun them for 1.75/2/8 seconds.

### Vex (4-cost, APCaster)

- Traits: Divinicorp, Executioner | mana 0/30
- **Retribution**: Passive: Gain 15% Omnivamp. 150% of overhealing from Retribution's damage is converted to bonus true damage to its primary target. Active: Send a shadow to the target, dealing [ModifiedInitialDamage] () magic damage to all enemies it passes through. The shadow then explodes, dealing [ModifiedDamage] () magic damage to the target and [ModifiedAOEDamage] () to enemies within 1 hex.

### Xayah (4-cost, ADCaster)

- Traits: Anima Squad, Marksman | mana 25/75
- **Buffer Overflow**: Impale 6 feathers into the current target that each deal [ModifiedCastDamage] () physical damage. When the target dies, the feathers seek out nearby enemies within 2-hexes, each dealing [ModifiedExplodeDamage] () physical damage.

### Zed (4-cost, ADReaper)

- Traits: Cypher, Slayer | mana 0/50
- **Cutthroat Business**: Throw a shuriken at the furthest unmarked enemy within 4 hexes that deals [ModifiedShurikenDamage] () magic damage and marks them. Then become untargetable, teleport to them, and summon shadows on every other marked enemy. Deal [ModifiedSpinDamage] () physical damage to adjacent enemies, while shadows deal [ModifiedShadowSpinDamage] () physical damage to adjacent enemies.

### Zeri (4-cost, ADCarry)

- Traits: Exotech, Rapidfire | mana 0/40
- **Hyperthread Rush**: Dash to a nearby location so quickly an untargetable echo lingers. The echo Attacks () as if it were Zeri, but deals 40/40/200% damage. Echoes last [ModifiedDuration] () seconds, and disappear when Zeri dies.

### Ziggs (4-cost, APCasterHighMana)

- Traits: Cyberboss, Strategist | mana 20/70
- **Bomberyordle**: Throw a bomb that hits the most enemies in a + shapewhole row and column within Attack Range, dealing [ModifiedDamage] () magic damage to enemies within 1 hex of the center and [ModifiedBossDamage] () magic damage to enemies further away. If only one unit is hit, deal 100% more.

### Aurora (5-cost, APCasterHighMana)

- Traits: Anima Squad, Dynamo | mana 20/80
- **Pierce the Veil**: On first cast, swap places with the left-most champion on your bench and continue fighting. That champion gains [ModifiedBonusAttackSpeed] () Attack Speed. If the swapped champion dies, return to the board. Teleport to hit the most enemies in a line, dealing [ModifiedDamage] () to each one, plus [ModifiedSplitDamage] () split between all enemies hit. Aurora cannot swap places with another Aurora.

### Garen (5-cost, ADFighter)

- Traits: God of the Net | mana 20/80
- **Banwave**: Gain [ModifiedShield] () Shield for 1.5/1.5/10 seconds. Deal [ModifiedDamage] () physical damage to the current target and send a wave that deals [ModifiedWaveDamage] () physical damage to enemies hit. If the wave hits 2 or fewer enemies, gain 30 Mana.

### Kobuko (5-cost, APFighter)

- Traits: Cyberboss, Bruiser
- **Game Over!**: Passive: Attacks deal [ModifiedOnHitDamage] () bonus magic damage, and 50% in a one hex radius. At 50/50/90% Health, trigger 'Phase 2', then enrage for the rest of the fight, gaining 140% Attack Speed, 30/30/100% Omnivamp, and increasing the bonus magic damage by 35%. Phase 2: Gain [ModifiedShield] () Shield for 4/4/60 seconds. Pick up a nearby enemy, knocking up all enemies within 2 hexes for 1.5 seconds. Then, slam down, dealing [ModifiedPrimaryTargetDamage] () magic damage to the target and [ModifiedSecondaryTargetDamage] () magic damage to all other enemies hit.

### Renekton (5-cost, ADFighter)

- Traits: Overlord, Divinicorp, Bastion | mana 70/140
- **Dominus**: Deal [TotalSpellDamage] () physical damage to enemies within 2 hexes and Burn, and Wound them for 5 seconds. Gain 400/650/10000 max Health. First Cast: Enter frenzy mode where attacks strike twice, dealing [DoubleStrikeTotalDamage] () total physical damage and can dash to new targets. Second Cast: Frenzy mode now strikes thrice, dealing [TripleStrikeTotalDamage] () total physical damage. Cannot cast again. {{TFT_Keyword_Burn}} {{TFT_Keyword_Wound}}

### Samira (5-cost, ADCaster)

- Traits: Street Demon, A.M.P. | mana 20/100
- **Demon Trigger**: Dash towards the target and fire a volley of [ModifiedNumBullets] (%i:set14AmpIcon%) bullets over 2 seconds split between enemies within three hexes. Each bullet reduces armor by 3 and deals [ModifiedBulletDamage] () physical damage, reduced by 20% per hex distance from Samira (Total Damage: [TotalVolleyDamage] ()). When the volley ends, dash back to safety while dealing [ModifiedSlideDamage] () physical damage to the current target. For the duration of the Ability, Samira is Unstoppable and has [ModifiedOmnivamp] () Omnivamp.

### Urgot (5-cost, ADCarry)

- Traits: BoomBot, Executioner | mana 0/40
- **Fear Beyond Death**: For 5/5/60 seconds, gain 125/125/500% Attack Speed and lock-on to the lowest percent Health enemy within [ModifiedCastRange] () hexes. Attacks also fire 3 rockets, each dealing [TotalRocketDamage] () total physical damage. Each cast, the first time a locked-on enemy falls beneath 15/15/95% Health, execute them, reel them in, and grind them into parts.

### Viego (5-cost, APReaper)

- Traits: Soul Killer, Golden Ox, Techie | mana 25/65
- **Holoblade**: Passive: When units die, absorb their soul and heal [ModifiedSoulHeal] () Health. Active: Jump to the largest clump of enemies within 2 hexes, dealing [ModifiedDamage] () magic damage to adjacent enemies and [ModifiedSplashDamage] () magic damage to other enemies within 2 hexes. 20% Shred all enemies hit for 4 seconds. Shred: Reduce Magic Resist

### Zac (5-cost, APTank)

- Traits: Virus | mana 30/100
- **Replicating Malware**: Passive: At 10% Health, split into two copies with 50/50/100% Health. Active: Bounce 3 times on nearby enemies. Each bounce deals [ModifiedDamage] () magic damage, Stuns for 1 second, and heals Zac [ModifiedHealing] () Health.

## Monsters, summons, pets and other trait-less units (34)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, ADTank)

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, ADTank)

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Mechacannon (1-cost, )

- Traits: (none)
- **Cannon Blast**: Cleans up the scum of the streets.

### Mechadrone (1-cost, )

- Traits: (none) | mana 0/60
- **Surveillance**: Always watching you.

### Mechazir (1-cost, )

- Traits: (none) | mana 10/30
- **Divide and Cull**: Call forth a wall of charging Mechasoldiers that deal [ModifiedDamage] magic damage and Stun enemies hit for 1 second.

### Murk Wolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, ADTank)

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Mechaminion (2-cost, )

- Traits: (none)
- **Dispersal Protocol**: Breaks up particularly stubborn factions.

### R-080T (2-cost, )

- Traits: (none) | mana 50/100
- **Rage With the Machines**: Fire grenades at the nearest [NumMissiles] enemies, dealing [ModifiedMissileDamage] () magic damage and Stunning them for [StunDuration] seconds. Heal [ModifiedHeal] () Health. [TFTUnitProperty.:TFT14_HotRod_LaserTooltip_Blank]

### Mechasoldier (3-cost, )

- Traits: (none)
- **Shock Troops**: Specialized soldiers for solving specialized problems.

### T-43X (4-cost, )

- Traits: (none) | mana 50/120
- **RawrXD**: 

### Mechaurelion (5-cost, )

- Traits: (none) | mana 10/30
- **Tears in the Rain**: Call down 6 meteors on random enemies that deal [ModifiedDamage] magic damage, Burn, and Wound for 10 seconds. {{TFT_Keyword_Burn}} {{TFT_Keyword_Wound}}

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### None (10-cost, )

- Traits: (none) | mana 0/100
- **Permanently increase Zac's maximum Health and Ability Power.**: Permanently increase Zac's maximum Health and Ability Power.

### None (10-cost, )

- Traits: (none) | mana 0/100
- **Permanently increase Zac's maximum Health and Ability Power.**: Permanently increase Zac's maximum Health and Ability Power.

### Bloblet (10-cost, )

- Traits: (none) | mana 0/100
- **Permanently increase Zac's maximum Health and Ability Power.**: Permanently increase Zac's maximum Health and Ability Power.

### Anima Squad Weapons (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Hacked Egg (11-cost, )

- Traits: (none)
- **Hacked Egg**: This egg will soon hatch into loot! LOSSES accelerate the hatch timer by an additional turn.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### SightWard (11-cost, )

- Traits: (none) | mana 0/100
- **Anima Squad XP Orb**: Sell this orb to gain 1XP.

### T-URR37 (11-cost, )

- Traits: (none) | mana 0/60
- **Rage With the Machines**: Fire a laser through the current target, dealing [ModifiedTotalLaserDamage] () total magic damage to enemies hit over [LaserDuration] seconds reduced by [DamageFalloffPercent]% per enemy hit.

### Tibbers (11-cost, )

- Traits: (none) | mana 0/80
- **Meltdown**: Passive: Has [ModifiedTibbersAS] (%i:set14AmpIcon%) Attack Speed. Burns and Wounds adjacent enemies for 5 seconds. {{TFT_Keyword_Burn}} {{TFT_Keyword_Wound}}


<a id="set-15"></a>

# Set 15: K.O. Coliseum

Source: current client data. 86 champions, 173 traits.

## Traits

### 100 Push Ups

Gain [TFTTrait.TFT15_MechanicTrait_100PushUps.1:DR]% Damage Reduction. For every [NumRerolls] rerolls you spend gold on, gain [TFTTrait.TFT15_MechanicTrait_100PushUps.1:DAPerReroll]% permanent Damage Amp. (Current: [TFTUnitProperty.trait:TFT15_MechanicTrait_100PushUps_TotalPercentDA]% )

  - (1) DAPerReroll=0.01, DR=0.15, NumRerolls=3, None, None

### 2X Contender

Vi, Ahri, Darius, Yasuo, Jinx, Braum, and Ekko gain [Amp]% Damage Amp and [DR]% Durability for each unique copy fielded. This unit gains [Percent]% more.

  - (1) Amp=0.05, DR=0.03, percent=0.75

### 2XKO

After two takedowns, stun enemies within 2 hexes for [StunDuration] seconds and gain [DamageAmp]% Damage Amp for the rest of combat.

  - (1) DamageAmp=0.35, StunDuration=2

### 404 Power Up

Gain [Stats]% Attack Damage, Ability Power, Damage Amp, and [Stats] Armor and Magic Resistance. How did you find this?

  - (1) Stats=8

### Adaptive Skin

When taking damage, gain [TFTTrait.TFT15_MechanicTrait_AdaptiveSkin.1:ResistsPerDamageInstancePhysical] Armor or [TFTTrait.TFT15_MechanicTrait_AdaptiveSkin.1:ResistsPerDamageInstanceMagic] Magic Resistance based on the damage type, up to [TFTTrait.TFT15_MechanicTrait_AdaptiveSkin.1:TotalResists] total resists.

  - (1) ResistsPerDamageInstanceMagic=10, ResistsPerDamageInstancePhysical=3, TotalResists=80

### All Out

At combat start go ALL OUT, gaining [Durability]% Damage Reduction and [AD]% Attack Damage. Health no longer decays while ALL OUT.

  - (1) AD=0.6, Durability=0.2

### Annihilation

Gain [BaseDA]% Damage Amp, increased to [IncreasedDA]% Damage Amp after the first takedown each combat.

  - (1) BaseDA=12, IncreasedDA=28

### Artistic KO

Every 4th attack deals [IncreasedDamage]% more damage. Overkill damage bounces to the nearest enemy.

  - (1) IncreasedDamage=0.35, NumEnemies=1

### Assassinate


  - (1) Duration=8, ReducedDamage=0.2
  - (2) Duration=8, ReducedDamage=0.15

### Atomic

Store [PercentMaxHealthInitial]% max Health at combat start and [PercentMaxHealthPerSecond]% more every second. On death, deal the stored Health as magic damage in a large radius.

  - (1) PercentMaxHealthInitial=10, PercentMaxHealthPerSecond=2, 15, 1.75, 4

### Attack Expert

Gain [BaseAD]% Attack Damage. All other sources of Attack Damage give [TFTTrait.TFT15_MechanicTrait_AttackExpert.1:PctBonusAD]% more.

  - (1) BaseAD=0.05, PctBonusAD=35

### Attempt #37

Ekko's spell prioritizes the closest enemy, but he heals [HealPercent]% Max Health on cast and deals [BonusDamagePerAttack]% more magic damage each time his spell hits the target.

  - (1) BonusDamagePerAttack=0.06, HealPercent=0.2

### Bastion

Your team gains [TeamwideResists] Armor and Magic Resist. Bastions gain more, and the value doubles in the first [Duration] seconds of combat.

  - (2-3) BonusArmor=18, BonusMR=18, Duration=10, EnhancedTeamwideArmor=25, StatMultiplier=2, TeamwideResists=10
  - (4-5) BonusArmor=40, BonusMR=40, Duration=10, EnhancedTeamwideArmor=25, StatMultiplier=2, TeamwideResists=10
  - (6) BonusArmor=75, BonusMR=75, Duration=10, EnhancedTeamwideArmor=25, StatMultiplier=2, TeamwideResists=10

### Battle Academia

Battle Academia champions upgrade their abilities and gain Potential %i:set14AmpIcon%. Potential improves their abilities.

  - (3-4) DA=0.1, DR=0.05, NumPotential=3, PrismaticItemRequirement=140, UltimateNumPotential=10
  - (5-6) DA=0.25, DR=0.1, NumPotential=5, PrismaticItemRequirement=140, UltimateNumPotential=10
  - (7-9) DA=0.5, DR=0.25, NumPotential=7, PrismaticItemRequirement=140, UltimateNumPotential=10
  - (10) DA=0.75, DR=0.4, NumPotential=12, PrismaticItemRequirement=140, UltimateNumPotential=10

### Best Defense

When granting or receiving shields, add [PercentDamage]% of the shield's value as bonus physical damage on the next attack.

  - (1) PercentDamage=1

### Bestest Boy

Every [Cadence] seconds, Lulu feeds a snack to her Monster: [PercentHeal]% Healing for Rammus, [AD]% Attack Damage for Smolder, or [AP]% Ability Power for Kog'Maw.

  - (1) AD=0.14, AP=14, Cadence=5, PercentHeal=0.13

### Bladenado

Attacks deal [PercentDamage]% of Basic Attack Damage as physical damage to enemies within attack range of the target.

  - (1) PercentDamage=0.25

### Blink Attack

When switching targets, blink to the next target. The next attack deals [TFTTrait.TFT15_MechanicTrait_TimeSkip.1:PctAdditionalDamage]% more as magic damage.

  - (1) PctAdditionalDamage=200

### Blood Brothers

Damage dealt by Yone and Yasuo bleeds enemies for [BleedPct]% of damage dealt over [BleedDuration] seconds.

  - (1) BleedDuration=2, BleedPct=20

### Bludgeoner

Attacks and Abilities ignore [PercentIgnore]% of enemy's Armor.

  - (1) PercentIgnore=45

### Body Change

At combat start, take the form of the nearest ally, granting them and this champion a [TFTTrait.TFT15_MechanicTrait_BodyChange.1:PctHealth]% Health shield.

  - (1) PctHealth=0.3

### Bonus Bloom

Gain an additional plant. Plants deal [PctOfDamage]% less damage and have [PctOfHealth]% less Health.

  - (1) PctOfDamage=0.2, PctOfHealth=0.3

### Bullet Hell

Fire [TFTTrait.TFT15_MechanicTrait_BulletHell.1:IncreasedProjectiles]% more projectiles with abilities.

  - (1) IncreasedProjectiles=30, 30

### Caretaker

On cast, the lowest percent Health ally gains [TFTTrait.TFT15_MechanicTrait_Caretaker.1:ShieldAmount] () shield for [TFTTrait.TFT15_MechanicTrait_Caretaker.1:Duration] seconds.

  - (1) Duration=6, ShieldAmount=135

### Classy

This champion's last-listed trait is [TFTTrait.TFT15_MechanicTrait_Classy.1:IncreasedEffectiveness]% more effective on them.

  - (1) IncreasedEffectiveness=45

### Colossal

Take up two unit slots. Gain Health, Durability, and Damage Amp.

  - (1) BonusDA=0.1, BonusDR=0.2, BonusHealth=900
  - (2) BonusDA=0.2, BonusDR=0.25, BonusHealth=1250

### Corner Carry

Gain [AS]% Attack Speed. At 75% and 25% health, dash to the safest position and gain infinite Range.

  - (1) AS=0.3

### Corrosive

[Shred]% Shred and Sunder enemies within 2 hexes. Take [DamageReduction]% less damage from Shredded or Sundered enemies.

  - (1) DamageReduction=10, Shred=30

### Corrupted

Start dormant. Gain [TFTTrait.TFT15_MechanicTrait_Corrupted.1:DAPerSoul]% Damage Amp and [TFTTrait.TFT15_MechanicTrait_Corrupted.1:HealthPerSoul] Health whenever any unit dies. At [TFTTrait.TFT15_MechanicTrait_Corrupted.1:HealthThreshold]% Health, awaken and blink toward enemies, stunning them for [TFTTrait.TFT15_MechanicTrait_Corrupted.1:StunDuration] seconds.

  - (1) DAPerSoul=0.045, HealthPerSoul=80, HealthThreshold=0.6, StunDuration=1.5

### Crimson Veil

Gain [Omnivamp]% Omnivamp. Overhealing from Omnivamp converts to shield. (Max: [MaxPercentHealthShield]% Health)

  - (1) MaxPercentHealthShield=40, Omnivamp=20

### Critical Threat

Abilities can Critically Strike. Every [TFTTrait.TFT15_MechanicTrait_CriticalThreat.1:Interval] seconds, gain [TFTTrait.TFT15_MechanicTrait_CriticalThreat.1:CriticalStrikeChance]% Critical Strike Chance.

  - (1) CriticalStrikeChance=5, Interval=3

### Crystal Gambit

Kills and losses during player combats earn Gem Power. Every [NumCombats] player combats choose to convert Gem Power into rewards or Double Down. While Double Down is active losses grant [PercentIncreasedGP]% more Gem Power, but wins lose [PercentLost]% Gem Power and cash out immediately. Gem Power gains increase at stages 3 and 4. .

  - (3-4) 7PeiceMult=1.3, 7PieceAmp=0.15, 7PieceHealth=300, GPPerDeathPVE=2, GPPerRoundPVE=10, NumCombats=3, NumCombatsPM=2, NumCombatsPVE=3, PercentIncreaseGPPVE=1, PercentIncreasedGP=1, PercentLossPVE=0.5, PercentLost=0.5, 10, 1, 1, 2, 15, 5, 1.4, 1.75
  - (5-6) 7PeiceMult=1.3, 7PieceAmp=0.15, 7PieceHealth=300, GPPerDeathPVE=3, GPPerRoundPVE=15, NumCombats=3, NumCombatsPM=2, NumCombatsPVE=3, PercentIncreaseGPPVE=1, PercentIncreasedGP=1, PercentLossPVE=0.5, PercentLost=0.5, 10, 1, 1, 2, 15, 7, 1.4, 1.75
  - (7-9) 7PeiceMult=1.3, 7PieceAmp=0.15, 7PieceHealth=300, GPPerDeathPVE=3, GPPerRoundPVE=15, NumCombats=3, NumCombatsPM=2, NumCombatsPVE=3, PercentIncreaseGPPVE=1, PercentIncreasedGP=1, PercentLossPVE=0.5, PercentLost=0.5, 10, 1, 1, 2, 15, 7, 1.4, 1.75
  - (10) 7PeiceMult=1.3, 7PieceAmp=0.15, 7PieceHealth=300, DamageReduction=0.99, GPPerDeathPVE=3, GPPerRoundPVE=15, NumCombats=3, NumCombatsPM=2, NumCombatsPVE=3, PercentIncreaseGPPVE=1, PercentIncreasedGP=1, PercentLossPVE=0.5, PercentLost=0.5, TrueDamagePerSecond=500, 10, 1, 1, 2, 15, 8, 1.4, 1.75

### Cyclone Rush

Gain Attack Speed. At combat start, disable the opposite enemy for [Duration] seconds.

  - (1) ASBoost=0.15, Duration=3
  - (2) ASBoost=0.25, Duration=3

### Dark Amulet

Heal for [HealPct]% of magic damage dealt. On cast, take [HealthLossPct]% max Health as true damage, but gain [StackingAP]% stacking Ability Power.

  - (1) HealPct=15, HealthLossPct=8, StackingAP=12

### Demolitionist

Ability damage Stuns for [TFTTrait.TFT15_MechanicTrait_Demolitionist.1:StunDuration] second.

  - (1) StunDuration=1

### Denting Blows

Vi's spell now shields for [ShieldDuration] seconds instead of healing, but is increased by [ShieldBoost]%. When converted, strike quickly strike [FlurryCount] times, each hit reducing the targets resists by [ResistShred].

  - (1) FlurryCount=4, ResistShred=5, ShieldBoost=0.3, ShieldDuration=4

### Desperado

After every [NumAttacks]th attack, fire five projectiles at the closest enemies that deal [PercentBADDamage]% of Basic Attack Damage as physical damage.

  - (1) NumAttacks=8, PercentBADDamage=100

### Doom Barrage

Arrows cost [ManaReduction] less Mana during Varus's Ability.

  - (1) ManaReduction=2

### Doomsayer

After [TFTTrait.TFT15_MechanicTrait_Doomsayer.1:Countdown] seconds, if still alive, deal [TFTTrait.TFT15_MechanicTrait_Doomsayer.1:PercentOfDamageTaken]% of damage taken this combat as magic damage to all enemies.

  - (1) Countdown=15, PercentOfDamageTaken=0.4

### Double Down

Gain [DamageAmp]% Damage Amp. When this unit casts, your other Powered Up unit gains [ManaPercent]% of the Mana spent.

  - (1) DamageAmp=0.18, ManaPercent=0.2

### Doublestrike

Attacks have a [TFTTrait.TFT15_MechanicTrait_Doublestrike.1:ProcChance]% chance to trigger an additional attack.

  - (1) ProcChance=30

### Dread Note


  - (1) FirstDeath=18, SecondDeath=30
  - (2) FirstDeath=15, SecondDeath=25

### Drift Duo

Senna and Lucian both gain [ManaRegen] mana regen. If both are 3-star, gain +1 Mighty Mech.

  - (1) ManaRegen=3

### Duelist

Duelists gain Attack Speed on each attack, stacking up to [MaxStacks] times.

  - (2-3) AttackSpeedPercent=0.04, DamageReduction=None, MaxStacks=12
  - (4-5) AttackSpeedPercent=0.07, DamageReduction=None, MaxStacks=12
  - (6) AttackSpeedPercent=0.1, DamageReduction=12, MaxStacks=12

### Edgelord

Edgelords gain Omnivamp and Attack Damage. While attacking enemies under [HealthThreshold]% Health, they gain [BonusAS]% Attack Speed.

  - (2-3) AD=0.15, BonusAS=0.4, HealthThreshold=0.5, Omnivamp=0.1
  - (4-5) AD=0.3, BonusAS=0.4, HealthThreshold=0.5, Omnivamp=0.12
  - (6) AD=0.6, BonusAS=0.4, HealthThreshold=0.5, Omnivamp=0.15

### Efficient

Abilities cost [Reduction] less Mana to cast.

  - (1) Reduction=15

### Essence Share

On cast, the [NumAllies] nearest allies gain [TFTTrait.TFT15_MechanicTrait_EssenceShare.1:ManaPct]% of Mana spent.

  - (1) ManaPct=0.35, NumAllies=2

### Executioner

Executioners gain Critical Strike Chance and Critical Strike Damage. Their Ability can critically strike.

  - (2) CritAmpPercent=10, CritChanceAmpPercent=25
  - (3) CritAmpPercent=12, CritChanceAmpPercent=35
  - (4) CritAmpPercent=18, CritChanceAmpPercent=50
  - (5) CritAmpPercent=28, CritChanceAmpPercent=55

### Fairy Tail

On cast spawn 2 fairies that each deal [TFTTrait.TFT15_MechanicTrait_FairyTail.1:APDamage1]-[TFTTrait.TFT15_MechanicTrait_FairyTail.1:APDamage4] (based on Stage) magic damage to the target over the next [TFTTrait.TFT15_MechanicTrait_FairyTail.1:Duration] seconds.

  - (1) APDamage1=75, APDamage2=120, APDamage3=170, APDamage4=260, Duration=8

### Fan Service

Xayah and Rakan gain bonus stats based on each other's star level.

  - (1) AD=8, AS=8, Health=150, Resists=5
  - (2) AD=15, AS=15, Health=275, Resists=10

### Fighter Spirit


  - (1) ADAP=5, Health=75

### Final Ascent

Ascend an additional time at levels [Level1] and [Level2]. Level [Level1]: Gain [APPerAttack]% Ability Power every [NumAttacks] attacks. Level [Level2]: Launch 2 additional waves that deal [ReducedWaveDamage]% damage.

  - (1) APPerAttack=6, Level1=7, Level2=10, NumAttacks=3, ReducedWaveDamage=0.6

### Final Boss

Gain [Omnivamp]% Omnivamp. Whenever an ally dies, gain [DamageAmp]% stacking Damage Amp.

  - (1) DamageAmp=0.045, Omnivamp=0.1

### Final Form

After round [AppropriateStage]-[AppropriateRound] if this champion is 3-star, become 4-star at combat start.

  - (1) AppropriateRound=2, AppropriateRoundEventPM=6, AppropriateRoundPVE=7, AppropriateStage=5, AppropriateStageEventPM=3, AppropriateStagePVE=2

### Finalist

Gain [TFTTrait.TFT15_MechanicTrait_Finalist.1:InitialDA]% Damage Amp and [TFTTrait.TFT15_MechanicTrait_Finalist.1:InitialDR]% Damage Reduction, increased by [TFTTrait.TFT15_MechanicTrait_Finalist.1:IncreasedDA]% and [TFTTrait.TFT15_MechanicTrait_Finalist.1:IncreasedDR]% for each eliminated player.

  - (1) IncreasedDA=0.03, IncreasedDR=0.03, InitialDA=0.15, InitialDR=0.08

### Frost Touch

Attacks have a [PercentageChanceToTrigger]% chance to Stun the target for [StunDuration] second.

  - (1) DamagePerStage=55, PercentageChanceToTrigger=15, StunDuration=1

### Fury Break

Gain [AS]% Attack Speed. When your other Powered Up unit dies, gain a burst of [ASBurst]% Attack Speed that decays over [Duration] seconds. If you die first, give them the burst instead.

  - (1) AS=0.3, ASBurst=3, Duration=4

### Fusion Dance

At combat start, fuse with the nearest ally, gaining [TFTTrait.TFT15_MechanicTrait_FusionDance.1:StatShare]% of their Health, Attack Damage, and Ability Power.

  - (1) StatShare=0.45

### Gather Force

On cast, gain stacking Attack Damage equal to [ADPctFromMana]% of Mana spent.

  - (1) ADPctFromMana=40

### Gem Core

The Mighty Mech converts [Absorption]% of damage taken into Attack Damage and Ability Power, and gains [Resists] Armor and Magic Resist. Crystal Rose champions are treated as Mighty Mechs for the Mech's abilities.

  - (1) Absorption=0.08, Resists=100

### Golden Edge

Attacking [NumAttacksPerProc] times grants [TFTUnitProperty.trait:TFT15_MechanicTrait_GoldenEdge_Gold] gold. This gold value increases by 1 each time it activates. (Attacks: [TFTUnitProperty.trait:TFT15_MechanicTrait_GoldenEdge_NumAttacks] / [NumAttacksPerProc])

  - (1) NumAttacksPerProc=72

### Golden Gloves

Whenever you gain interest, permanently gain [AD]% Attack Damage per gold collected. (Current Bonus: [TFTUnitProperty.trait:TFT15_SetMechanic_GoldenGlovesStacks]% )

  - (1) AD=0.02

### Hat Trick

Gain [BaseADAP]% Attack Damage and Ability Power. Each takedown, don a hat that grants [ADAPPerHat]% of each. On death, lose [PercentHatLoss]% of your hats. (Current Hats: [TFTUnitProperty.trait:TFT15_SetMechanic_MaxCap_Hats])

  - (1) ADAPPerHat=1, BaseADAP=16, PercentHatLoss=0.5

### Heart of Gold

If this champion survives a player combat, gain [NumGold] gold.

  - (1) NumGold=2

### Heavyweight

Your team gains [TeamBonusHealth] Health. Heavyweights gain additional bonus Health, and Attack Damage equal to a percentage of their Health.

  - (2-3) BonusPercentHealth=0.2, HealthPercentToAD=0.2, TeamBonusHealth=100
  - (4-5) BonusPercentHealth=0.4, HealthPercentToAD=0.4, TeamBonusHealth=100
  - (6) BonusPercentHealth=0.65, HealthPercentToAD=0.6, TeamBonusHealth=100

### Hemorrhage

Abilities deal [PercentBleed]% bonus damage as true damage over [BleedDuration] seconds.

  - (1) BleedDuration=4, PercentBleed=60

### Hero's Arc

Gain [TFTTrait.TFT15_MechanicTrait_HerosArc.1:DA]% Damage Amp per player level. At level 10 gain an additional [TFTTrait.TFT15_MechanicTrait_HerosArc.1:DamageIncrease]% Damage Amp.

  - (1) DA=0.045, DamageIncrease=0.35

### Hungry Hero

Gain [TFTTrait.TFT15_MechanicTrait_BloodFury.1:BaseHealth] Health and [TFTTrait.TFT15_MechanicTrait_BloodFury.1:BaseAD]% Attack Damage. Each round, you may pay [TFTTrait.TFT15_MechanicTrait_BloodFury.1:PlayerHealthPerSacrifice] player health for [TFTTrait.TFT15_MechanicTrait_BloodFury.1:HealthPerSacrifice] and [TFTTrait.TFT15_MechanicTrait_BloodFury.1:ADPerSacrifice] . (Total: [TFTUnitProperty.:TFT15_MechanicTrait_BloodFury_Health] [TFTUnitProperty.:TFT15_MechanicTrait_BloodFury_AD] )

  - (1) ADPerSacrifice=0.04, BaseAD=0.1, BaseAP=None, BaseHealth=100, HealthPerSacrifice=150, PlayerHealthPerSacrifice=3, 2

### Hyperactive

Takedowns grant [AttackSpeed]% Attack Speed that rapidly decays over [DecayDuration] seconds.

  - (1) AttackSpeed=100, DecayDuration=3

### Ice Bender

Combat Start: Ryze is frozen for the first [Duration] seconds of combat. His Ability [Chill]% Chills enemies for [ChillDuration] second, and deals [PctSpellDamage]% bonus damage.

  - (1) Chill=20, ChillDuration=1, Duration=3, PctSpellDamage=0.2

### Infernal Speed

Gain [TFTTrait.TFT15_MechanicTrait_InfernalSpeed.1:BaseAS]% Attack Speed. On takedown, generate a Cinder. Grant your team [TFTTrait.TFT15_MechanicTrait_InfernalSpeed.1:CinderAS]% bonus for every [TFTTrait.TFT15_MechanicTrait_InfernalSpeed.1:NumCinders] Cinders. (Cinders: [TFTUnitProperty.trait:TFT15_MechanicTrait_InfernalSpeed_CindersCollected]; Team : [TFTUnitProperty.trait:TFT15_MechanicTrait_InfernalSpeed_PermanentAS]%)

  - (1) BaseAS=10, CinderAS=2, NumCinders=8

### Inner Fire

Every [ICD] seconds, Burn and Wound the closest non-Burning enemy for [Duration] seconds. Take [DamageReduction]% less damage from Burning enemies.

  - (1) DamageReduction=0.15, Duration=10, ICD=2

### Juggernaut

Juggernauts gain Durability, increased above [HealthBreakpoint]% health. When a Juggernaut dies, other Juggernauts heal for [HealRatio]% of their max Health.

  - (2-3) BaseDR=0.15, HealRatio=0.1, HealthBreakpoint=0.5, IncreasedDR=0.25
  - (4-5) BaseDR=0.2, HealRatio=0.1, HealthBreakpoint=0.5, IncreasedDR=0.3
  - (6) BaseDR=0.25, HealRatio=0.1, HealthBreakpoint=0.5, IncreasedDR=0.35

### Kahunahuna

Every [TFTTrait.TFT15_MechanicTrait_Kahunahuna.1:NumAttacks] attacks, deal [TFTTrait.TFT15_MechanicTrait_Kahunahuna.1:ADDamage] () true damage.

  - (1) ADDamage=175, NumAttacks=6

### Keen Eye

Attacks and Abilities ignore [PercentIgnore]% of enemy's Magic Resistance.

  - (1) PercentIgnore=45

### Killer Instinct

Abilities target the lowest Health enemy in range instead. Gain [TFTTrait.TFT15_MechanicTrait_KillerInstinct.1:ManaRegen] Mana Regen.

  - (1) ManaRegen=3

### Living Wall

Gain [Resists] Armor and Magic Resistance. At combat start allies in the back row gain [AS]% Attack Speed for every [PerXResists] combined and this champion has.

  - (1) AS=1.5, PerXResists=30, Resists=20

### Luchador

Luchadors gain bonus Attack Damage. At [HealthRatio]% health, Luchadors cleanse negative effects, heal, and leap back into the fight, Stunning enemies in a 1-hex radius for [StunDuration] seconds.

  - (2-3) ADBonus=0.15, HealRatio=0.25, HealthRatio=0.5, ShieldDuration=5, StunDuration=1
  - (4) ADBonus=0.4, HealRatio=0.5, HealthRatio=0.5, ShieldDuration=5, StunDuration=1

### Mage

Cast Abilities twice, but deal [TFTTrait.TFT15_MechanicTrait_Mage.1:DAMultiplier]% less total damage.

  - (1) DAMultiplier=0.2

### Magic Expert

Gain [TFTTrait.TFT15_MechanicTrait_MagicExpert.1:BaseAP]% Ability Power. All other sources of Ability Power give [TFTTrait.TFT15_MechanicTrait_MagicExpert.1:PctBonusAP]% more.

  - (1) BaseAP=10, PctBonusAP=35

### Mana Rush

Start combat with full Mana.

  - (1) (no numbers)

### Mastermind

At combat start, gain [SelfishMana] Mana and give [AllyMana] Mana to all allies in a line in front of this champion.

  - (1) AllyMana=25, SelfishMana=50

### Max Arcana

Gain [Base]% Ability Power, and permanently gain [Amount]% every 2 takedowns. (Current: [TFTUnitProperty.trait:TFT15_SetMechanic_PlusPlus_Property]% )

  - (1) Amount=1, Base=15

### Max Attack

Gain [Base]% Attack Damage, and permanently gain [Amount]% every 2 takedowns. (Current: [TFTUnitProperty.trait:TFT15_SetMechanic_PlusPlus_Property]% )

  - (1) Amount=0.01, Base=0.12

### Max Speed

Gain [Base]% Attack Speed, and permanently gain [Amount]% every 3 takedowns. (Current: [TFTUnitProperty.trait:TFT15_SetMechanic_PlusPlus_Property]% )

  - (1) Amount=0.01, Base=0.15

### Max Vitality

At combat start gain [PerCombatAmount] permanent Health. Every [Interval] seconds this champion survives, gain [Amount] additional permanent Health. (Current: [TFTUnitProperty.trait:TFT15_SetMechanic_PlusPlus_Property] )

  - (1) Amount=20, Interval=10, PerCombatAmount=40, PerCombatAmountEventPM=80

### Mech Pilot


  - (1) HealthShare=0.35, MechHealth=0.6, OtherStatShare=0.45
  - (2) HealthShare=0.35, MechHealth=0.6, OtherStatShare=0.45

### Mechablade

Gain Attack Speed equal to [PercentHealthAsAS]% maximum Health. Attacks grant [BonusMana] bonus Mana.

  - (1) BonusMana=5, PercentHealthAsAS=0.05

### Mechador

When you field Mighty Mech and Luchador, The Mighty Mech gains [Health] Health and becomes a Luchador granting +1 to the trait.

  - (1) Health=100

### Mentor

This trait is active only when you have exactly 1 or 4 unique Mentors. Kobuko: [Durability]% Damage Reduction Udyr: [ADAP]% Attack Damage and Ability Power Yasuo: [AS]% Attack Speed Ryze: Attacks grant [Mana] bonus Mana

  - (1) ADAP=0.08, AS=0.1, Durability=0.06, Mana=2
  - (2-3) ADAP=0.08, AS=0.1, Durability=0.06, Mana=2
  - (4) ADAP=0.08, AS=0.1, Durability=0.06, Mana=2

### Midas Touch

Attacks and Abilities execute enemies below [PercentHealthThreshold]% Health. Kills have a [PercentChanceGold]% chance to drop a gold. (Gold: [TFTUnitProperty.trait:TFT15_MechanicTrait_MidasTouch_GoldEarned])

  - (1) PercentChanceGold=35, PercentHealthThreshold=10

### Mighty Mech

Gain The Mighty Mech. Mighty Mechs heal it for [GalioHealRatio]% of the damage they deal. Unlock the Mightiest Mech Each Mighty Mech champion's star level increases The Mighty Mech's power.

  - (3-4) AutoAttackDamage=65, GalioHealRatio=0.12, NumItems=1, 1, 0.16, 599, 5, 50, 0.05, None, None, 2, 0.15
  - (5-6) AutoAttackDamage=100, GalioHealRatio=0.12, Health=1300, NumItems=2, 2, 0.16, 699, 5, 75, 0.05, None, None, 2, 0.15
  - (7) AutoAttackDamage=110, GalioHealRatio=0.12, Health=2000, NumItems=3, 3, 0.16, 949, 5, 85, 0.05, 20, 60, 2, 0.15
  - (7) AutoAttackDamage=120, GalioHealRatio=0.12, Health=2000, NumItems=3, 4, 0.16, 949, 5, 90, 0.05, 20, 60, 2, 0.15

### Mind Battery

After the first cast each combat, gain [ManaRegen] Mana Regen and [AP]% Ability Power.

  - (1) AP=30, ManaRegen=5

### Momentum

([MinUnits]) Gain [BonusAS]% Attack Speed and moves significantly faster.

  - (1) BonusAS=0.6, BonusMS=0.5

### Monster Trainer

Choose which monster Lulu summons to replace her in combat!

  - (1) FirstEvolveLevel=15, HealthGainPerLevel=0.01, SecondEvolveLevel=30, 50, 0.03, 3

### Needlework

Fire [BonusNeedles] bonus needles with every 1st and 2nd cast that deal [ReducedDamage]% damage.

  - (1) BonusNeedles=2, ReducedDamage=0.45

### Not Done Yet

Once this champion has died [NumDeathsStage2] times, gain [TFTTrait.TFT15_MechanicTrait_NotDoneYet.1:PctHealth]% Health, [TFTTrait.TFT15_MechanicTrait_NotDoneYet.1:Omnivamp]% Omnivamp, and [TFTTrait.TFT15_MechanicTrait_NotDoneYet.1:Stats]% Damage Amp. (Deaths: [TFTUnitProperty.trait:TFT15_MechanicTrait_NotDoneYet_DeathsAchieved])

  - (1) NumDeathsStage2=6, NumDeathsStage3=6, NumDeathsStage4=6, Omnivamp=10, PctHealth=18, Stats=25

### Noxious Trap

Damage dealt Burns, Wounds, 20% Shreds, and 20% Sunders enemies for [Duration] seconds.

  - (1) Duration=4

### On The Edge

Start combat at [HealthPercent]% Health, but gain [DA]% Damage Amp.

  - (1) DA=0.35, HealthPercent=0.65

### Ordinary

If this champion has no active traits, gain [TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:HP1]-[TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:HP4] Health and [TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:AS1]-[TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:AS4]% Attack Speed (based on current Stage).

  - (1) AS1=40, AS2=55, AS3=80, AS4=100, HP1=300, HP2=400, HP3=750, HP4=1000, Multiplier=0.2

### Over 9000

At the start of each round, gain a random permanent stat bonus. Current: +[TFTUnitProperty.trait:TFT15_Over9000_AD]% , +[TFTUnitProperty.trait:TFT15_Over9000_AP]% , +[TFTUnitProperty.trait:TFT15_Over9000_AS]% , +[TFTUnitProperty.trait:TFT15_Over9000_Armor] , +[TFTUnitProperty.trait:TFT15_Over9000_MR] , +[TFTUnitProperty.trait:TFT15_Over9000_Health] , +[TFTUnitProperty.trait:TFT15_Over9000_Mana]

  - (1) AD=0.04, AP=4, AS=0.04, Armor=8, Health=35, MR=8, Mana=1

### Pack Tactics

Gain [NumPackmates] packmates that deal [DamagePercent]% () physical damage. When Naafiri uses her Ability, packmates dash to the lowest Health enemy within [NumHexes] hexes.

  - (1) DamagePercent=0.22, NumHexes=2, NumPackmates=2

### Power Font

At combat start and every [Cadence] seconds, gain [Mana] Mana Regen.

  - (1) Cadence=3, Mana=1

### Precision

Set Attack Speed to [TFTTrait.TFT15_MechanicTrait_Precision.1:ForcedAttackSpeed]. Convert [TFTTrait.TFT15_MechanicTrait_Precision.1:ASScale]% Attack Speed to [TFTTrait.TFT15_MechanicTrait_Precision.1:ADScale]% Attack Damage. Attacks deal [TFTTrait.TFT15_MechanicTrait_Precision.1:AttackAmount]% more damage and grant [TFTTrait.TFT15_MechanicTrait_Precision.1:Mana] more Mana.

  - (1) ADScale=0.008, ASScale=0.01, AttackAmount=1.3, ForcedAttackSpeed=0.7, Mana=5

### Prodigy

Your team gains Mana Regen. Prodigies gain more.

  - (2) ProdigyBonusMana=3, TeamBonusMana=1
  - (3) ProdigyBonusMana=4, TeamBonusMana=1
  - (4) ProdigyBonusMana=6, TeamBonusMana=1
  - (5) AllyHealing=0.12, ProdigyBonusMana=7, TeamBonusMana=1

### Protector

Units gain [DamageReductionPct]% Durability while shielded. Once per combat at [HealthThreshold]% Health, Protectors shield themselves and their closest ally for a percent of their maximum Health. Shields stack.

  - (2-3) DamageReductionPct=0.05, HealthThreshold=0.5, ShieldPercentAmount=0.18
  - (4-5) DamageReductionPct=0.05, HealthThreshold=0.5, ShieldPercentAmount=0.36
  - (6) DamageReductionPct=0.05, HealthThreshold=0.5, ShieldPercentAmount=0.55

### Pure Heart

Every [HealTickRate] seconds heal this champion and allies within 2-hexes for [MissingHealthHeal]% of their missing Health.

  - (1) HealTickRate=4, MissingHealthHeal=0.18

### Pursuit

Lucian's Ability dashes, fires +[NumAdditionalShots] additional bullets, and grants [APGain]% Ability Power for the rest of combat.

  - (1) APGain=8, NumAdditionalShots=1

### Quick Tag

On death, spawn a unit of the same role with the same number of items. This unit is 1 cost lower, and has [PercentHealth]% reduced Health.

  - (1) PercentHealth=0.15

### Ramping Rage

Attacks grant [ASPerAttack]% stacking Attack Speed.

  - (1) ASperAttack=0.035

### Rare Treat

Your Monster has +[Levels] Monster Trainer levels.

  - (1) Levels=15

### Regenerative

Heal [TFTTrait.TFT15_MechanicTrait_MetabolicExplosion.1:PctRegen]% max Health every [TFTTrait.TFT15_MechanicTrait_MetabolicExplosion.1:Interval] seconds. Gain [IncreasedHealing]% increased healing from all sources.

  - (1) IncreasedHealing=10, Interval=3, PctRegen=6.5

### Resistant

Reduce damage taken by [TFTTrait.TFT15_MechanicTrait_Resistant.1:FlatDR].

  - (1) FlatDR=25, Resists=None

### Rising Chaos

On cast, fire an additional orb that deals [PercentDamage]% damage to a random enemy. Subsequent casts add an additional orb.

  - (1) PercentDamage=0.07

### Robo Ranger

Gain [TFTTrait.TFT15_MechanicTrait_RobustRanger.1:StatsPct]% of The Mighty Mech's Attack Damage and Ability Power, and [TFTTrait.TFT15_MechanicTrait_RobustRanger.1:HealthPct]% of its Health.

  - (1) HealthPct=10, StatsPct=50

### Rocket Grab

At combat start, gain a [ShieldPercent]% Health shield for [ShieldDuration] seconds. When your target dies, pull the next target into melee range and regain the shield.

  - (1) ShieldDuration=4, ShieldPercent=0.2

### Rogue Captain

Twisted Fate upgrades the Crew Ship to deal [Bounty_TheCrew_RocketUpgrade_TrueDamagePercent]% of it's damage as true damage and draws Bounty Cards each round that grant random rewards. Bounty Cards get better for each player combat. (Rounds Fielded: [TFTUnitProperty.trait:TFT15_Trait_RogueCaptain_Bounty_NumRoundsLeft]) Current Bounty Card: [TFTUnitProperty.trait:TFT15_Trait_RogueCaptain_CurrentBounty] In Choncc's Treasure, Twisted Fate will not draw a Bounty Card until Stage 3-5.

  - (1) Bounty_CasterEmpower_DA=0.02, Bounty_FreeRerolls_NumRerolls=2, Bounty_GainAttackSpeed_AS=0.15, Bounty_GainGoldLarge_MinGold=12, Bounty_GainGold_MinGold=2, Bounty_GainXP_Amount=12, Bounty_Omnivamp_Omnivamp=0.15, Bounty_PermanentAP_AP=18, Bounty_TankEmpower_MaxHealth=40, Bounty_TeamShield_Duration=8, Bounty_TeamShield_Shield=160, Bounty_TheCrew_GainHealth_PermanentADAP=5, Bounty_TheCrew_GainHealth_PermanentHealth=60, Bounty_TheCrew_GoldPerStar_GoldAmount=1, Bounty_TheCrew_RocketUpgrade_TrueDamagePercent=0.15, Bounty_ThinkFast_Cadence=3, Bounty_ThinkFast_Gold=5, 1, 1, 12, 6, 0.1, 2

### Rosemother

Gain [1Star]/[2Star]/[3Star] placeable plants, based on Zyra's star level. Plants in the front two rows grow into durable Grasping Roots, while plants in the back two rows grow into Deadly Spines. Rosemother plants benefit from Zyra's Ability Power and Attack Speed. When Zyra casts, her plant restores [BonusHealthAndAS]% Health and gains [BonusHealthAndAS]% Attack Speed for the rest of combat. If the plant is dead, she revives it instead.

  - (1) 1Star=1, 2Star=1, 3Star=8, BonusHealthAndAS=0.35

### Round Two

On death, summon a Training Dummy equipped with the same items. It has [TFTTrait.TFT15_MechanicTrait_RoundTwo.1:PercentHealth]% of this unit's health.

  - (1) PercentHealth=60

### S.O.L.E. Fighter

If this champion is the only Soul Fighter fielded, it gains the benefits of Soul Fighter (4).

  - (1) (no numbers)

### Selfish

Gain [TFTTrait.TFT15_MechanicTrait_Selfish.1:DR]% Durability. Heal for [TFTTrait.TFT15_MechanicTrait_Selfish.1:HealPct]% of damage dealt by all other allies.

  - (1) DR=10, HealPct=5

### Serious Slam

The first attack on each enemy deals [TFTTrait.TFT15_MechanicTrait_SeriousSlam.1:AttackScale]% () physical damage and Stuns for [TFTTrait.TFT15_MechanicTrait_SeriousSlam.1:KnockupDuration] seconds.

  - (1) AttackScale=2.1, HealthScale=None, KnockupDuration=1

### Shadow Clone

Create a perfect copy of this champion with the same items equipped that deals [TFTTrait.TFT15_MechanicTrait_ShadowJutsu.1:DamagePercent]% damage and has [TFTTrait.TFT15_MechanicTrait_ShadowJutsu.1:HealthMod]% health.

  - (1) DamagePercent=0.22, HealthMod=0.5

### Shine Brighter

Every other cast, Lux's spell explodes in a 2-hex radius, and deals [Percent]% more damage.

  - (1) percent=0.3

### Singularity

Become a [NumHexes]-hex singularity, dealing [PercentHealth]% () as magic damage every second. When enemies die within the radius, gain [PermanentHealth] permanent Health. (Total: [TFTUnitProperty.:TFT15_MechanicTrait_Singularity_Health] )

  - (1) NumHexes=2, PercentHealth=0.01, PermanentHealth=15

### Sky Piercer

Damage 30% Shreds and Sunders enemies for [Duration] seconds. Deal [DamageAmp]% increased damage against Shredded and Sundered enemies.

  - (1) DamageAmp=0.05, Duration=6

### Snax Time

Braum's casts also throw [PoroNum] Poros at random enemies. Each deals [DamagePercent]% of Braum's Basic Attack Damage as physical damage. This is safe for Poros. Braum is a professional.

  - (1) DamagePercent=2, PoroNum=5

### Sniper

Snipers gain Damage Amp, increased against targets farther away.

  - (2) BonusHexRangeIncrease=None, PerHexIncrease=3, PercentDamageIncrease=13
  - (3) BonusHexRangeIncrease=None, PerHexIncrease=5, PercentDamageIncrease=18
  - (4) BonusHexRangeIncrease=None, PerHexIncrease=7, PercentDamageIncrease=25
  - (5) BonusHexRangeIncrease=None, PerHexIncrease=10, PercentDamageIncrease=25

### Socialite

Combat start: Mark adjacent hexes. Allies in marked hexes gain [DamageAmp]% Damage Amp and [Health] Health. This champion gains [PercentIncrease]% of the bonus.

  - (1) DamageAmp=0.06, Health=60, ManaPerSecond=None, NumRounds=99, PercentIncrease=3

### Solar Breath

Every [ICD] seconds, Burn and Wound the closest non-Burning enemy for [Duration] seconds. Deal [DamageAmp]% more damage to Burning enemies.

  - (1) DamageAmp=0.1, Duration=10, HexRange=12, ICD=2

### Sorcerer

Sorcerers gain bonus Ability Power. When an enemy dies after being damaged by a Sorcerer, they deal a percentage of that enemy's maximum Health to another enemy.

  - (2-3) AbilityPower=20, HealthPct=0.08, TargetNum=1, 3
  - (4-5) AbilityPower=55, HealthPct=0.1, TargetNum=1, 3
  - (6) AbilityPower=80, HealthPct=0.12, TargetNum=2, 3

### Soul Chipper

Dealing magic damage reduces Magic Resistance by [MagicResistReduce] for the rest of combat.

  - (1) MagicResistReduce=5

### Soul Fighter

Soul Fighters gain bonus Health, and gain Attack Damage and Ability Power every second up to [MaxStacks] stacks. At max stacks deal bonus true damage. MAXIMUM SOUL POWER.

  - (2-3) BonusTrueDamage=0.1, FlatHealth=120, MaxStacks=8, PlayerDamageToPrismatic=10, StatsPerSecond=0.01
  - (4-5) BonusTrueDamage=0.16, FlatHealth=240, MaxStacks=8, PlayerDamageToPrismatic=10, StatsPerSecond=0.025
  - (6-7) BonusTrueDamage=0.22, FlatHealth=425, MaxStacks=8, PlayerDamageToPrismatic=10, StatsPerSecond=0.04
  - (8-10) BonusTrueDamage=0.28, FlatHealth=600, MaxStacks=8, PlayerDamageToPrismatic=10, StatsPerSecond=0.04
  - (11) BonusTrueDamage=0.3, FlatHealth=700, MaxStacks=8, PlayerDamageToPrismatic=10, StatsPerSecond=0.04

### Space Ace

Damage this champion deals contributes [Percent]% more towards launching rockets from The Crew's ship.

  - (1) percent=0.75

### Spiky Shell

Gain [Armor] Armor. Enemies attacking this champion take [Damage] + [DamagePerStage] per stage physical damage. ([ICD] second cooldown per attacker)

  - (1) Armor=40, Damage=25, DamagePerStage=8, ICD=1

### Spirit Sword

Gain [CritChance]% Critical Strike Chance. Attacks that critically strike deal [TFTTrait.TFT15_MechanicTrait_SpiritSword.1:BonusMagicDamage]% bonus magic damage.

  - (1) BonusMagicDamage=30, CritChance=0.2

### Stalking Dart

Every 10 Mana Ahri gains fires a foxfire dart towards her target, dealing [PercentDamage]% of her spell's damage as magic damage.

  - (1) PercentDamage=0.075

### Stance Master

When you field Lee Sin, choose between Duelist Stance, Executioner Stance, and Juggernaut Stance! Each stance has a unique ability, and grants Lee Sin the associated trait.

  - (1) 8

### Stand Alone

If this champion starts combat alone in its row, gain [TFTTrait.TFT15_MechanicTrait_StandStrong.1:HealthRatio]% Health.

  - (1) HealthRatio=0.3

### Stand United

Shen's ability also shields the lowest health ally for [ShieldPercent]% of his shield and gains [MagicResist] stacking Magic Resist on cast.

  - (1) MagicResist=25, ShieldPercent=0.5

### Star Guardian

Star Guardians have a unique Teamwork bonus that is granted to all Star Guardians. Every Star Guardian fielded increases the bonus! Prismatic: Field 11 Star Guardians and AWAKEN THE STARS! Prismatic: Spend [ManaToPrismatic] mana ([TFTUnitProperty.trait:TFT15_StarGuardian_ManaSpent]) THE STARS AWAKEN. Rell: Gain [TFTUnitProperty.trait:TFT15_StarGuardian_RellValue] shieldRell: Gain shields Syndra: Gain [TFTUnitProperty.trait:TFT15_StarGuardian_SyndraValue]% Ability Power every [SyndraTimer] secondsSyndra: Gain Ability Power Xayah: Every 3rd attack deals [TFTUnitProperty.trait:TFT15_StarGuardian_XayahValue] (increases per stage) magic damageXayah: Magic damage on attack Ahri: After casting, gain [TFTUnitProperty.trait:TFT15_StarGuardian_AhriValue] Mana over [AhriDuration] secondsAhri: On cast, gain Mana Neeko: +[TFTUnitProperty.trait:TFT15_StarGuardian_NeekoValue]% healing and shieldingNeeko: Increase heals & shields Poppy: At 40% Health, heal [TFTUnitProperty.trait:TFT15_StarGuardian_PoppyValue]% Poppy: Heal at low Health Jinx: +[TFTUnitProperty.trait:TFT15_StarGuardian_JinxValue]% Attack Speed. +[TFTUnitProperty.trait:TFT15_StarGuardian_JinxBonusASValue]% after takedowns, decaying over 3sJinx: Gain Attack Speed Seraphine: Gain [TFTUnitProperty.trait:TFT15_StarGuardian_SeraphineValue] and [TFTUnitProperty.trait:TFT15_StarGuardian_SeraphineValue]% Seraphine: Gain every stat Emblem: Increase bonuses by [EmblemBonus]%!Emblem: Increase other bonuses

  - (2) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (3) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.05, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (4) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.1, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (5) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.12, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (6) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.14, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (7) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.16, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (8) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.18, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (9-10) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.2, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18
  - (11) AhriDuration=2, EmblemBonus=0.1, ManaToPrismatic=16000, Multiplier=1.2, SyndraTimer=3, 0.12, 5, 60, 0.05, 2, 0.08, 50, None, 240, None, 750, 6, 0.2, 500, 0.18

### Star Sailor


  - (1) HealingPercent=None, StatIncrease=0.33
  - (2) HealingPercent=0.15, StatIncrease=0.25

### Star Student


  - (1) BaseHealth=200, PctAdditionalPotential=40
  - (2) BaseHealth=200, BaseXP=1, PctAdditionalPotential=40, XPPerSurvival=1

### Starry Knight

Gain [Health]% Health and [AP]% Ability Power for every 1-cost fielded, doubled if they're 3-star.

  - (1) AP=10, Health=0.07, Mult=2

### Storm Bender


  - (1) PercentDamageEcho=0.22, 8, 1, 2
  - (2) PercentDamageEcho=0.25, 10, 1, 2

### Strategist

Combat Start: Allies in the front 2 rows gain a shield for [ShieldDuration] seconds. Allies in the back 2 rows gain Damage Amp. Strategists gain triple.

  - (2) DamageAmp=4, Shield=150, ShieldDuration=15
  - (3) DamageAmp=6, Shield=200, ShieldDuration=15
  - (4) DamageAmp=10, Shield=300, ShieldDuration=15
  - (5) DamageAmp=14, Shield=400, ShieldDuration=15

### Stretchy Arms

Gain [BaseAS]% Attack Speed. On takedown, gain +1 Attack Range.

  - (1) BaseAS=0.1

### Strong Spark

Every [Cadence] seconds, zap the closest enemy for [Damage]% of their max Health as magic damage. Whenever an enemy is Stunned by any source, zap them.

  - (1) Cadence=3, Damage=0.11

### Super Genius

Every [Cadence] seconds gain Ability Power equal to your Mana Regen.

  - (1) Cadence=2

### Super Mega

Jinx gains [AD]% Attack Damage. Every 3rd cast is twice as big, and splits [SplitDamageIncrease]% more damage among enemies hit.

  - (1) AD=0.3, SplitDamageIncrease=0.5

### Superstar

Your team gains [DABase]% Damage Amp, increased by [TFTTrait.TFT15_MechanicTrait_Superstar.1:DAPer3Star]% for each 3-Star on your team. This champion gains [Multiplier]% of the bonus.

  - (1) DABase=4, DAPer3Star=3, Multiplier=2

### Supremacy

While this champion is the Supreme Cell, they execute enemies under [ExecutePct]% Health instead.

  - (1) ExecutePct=0.18

### Supreme Cells

The Cell who dealt the most damage last combat is Supreme. When the Supreme Cell dies, the Cell with the highest current damage becomes Supreme. Cells gain Damage Amp. The Supreme Cell gains more and executes enemies under [OverlordExecute]% Health.

  - (2) DamageAmp=8, OverlordDamageAmp=12, OverlordExecute=0.1, 1
  - (3) DamageAmp=18, OverlordDamageAmp=30, OverlordExecute=0.1, 1
  - (4) DamageAmp=28, OverlordDamageAmp=50, OverlordExecute=0.1, 2

### Supreme Surge

Whenever an enemy is executed by a Supreme Cell, heal [Percent]% missing health and gain [mana] mana.

  - (1) Mana=15, percent=0.15

### Surge 66

Gain [AS]% Attack Speed and [AP]% Ability Power. After [FirstThreshold] and [SecondThreshold] attacks, this amount doubles. (Attacks: [TFTUnitProperty.trait:TFT15_MechanicTrait_Surge66_NumAttacks])

  - (1) AP=10, AS=20, FirstThreshold=15, SecondThreshold=66

### Tank-zilla

Gain [TFTTrait.TFT15_MechanicTrait_KaijuSize.1:HealthRatio]% max Health and grow larger. After [TFTTrait.TFT15_MechanicTrait_KaijuSize.1:Countdown] seconds, Stun all enemies for 1.5 seconds.

  - (1) Countdown=15, HealthRatio=0.2

### Team Captain


  - (1) StatIncrease=0.15
  - (2) StatIncrease=0.2

### Tentacle Smash

After [InitialDelay] seconds, Stun enemies within 3 hexes for [StunDuration] second. Repeat every [Cadence] seconds.

  - (1) Cadence=8, InitialDelay=4, StunDuration=1

### The Champ

The Champ's victories against players grant Poro-fans equal to his star level. On loss, Poro-fans prevent [PlayerDamagePrevented] Tactician damage each, then lose all of your Poro-fans. Poro Fanbase: [TFTUnitProperty.trait:TFT15_Trait_ElTigre_NumPoros]

  - (1) PctPorosLost=100, PlayerDamagePrevented=1, PlayerHealthPerVictoryDU=1, PoroAmountForHealPVE=20, PorosPerVictory=1, PorosPerVictoryDU=1, TacticianHealPVE=1, None

### The Crew

Crew champions gain [StatBonus]% Health and Attack Speed for each Crew member fielded. Every 3-star Crew champion grants an additional bonus. (1x %i:3StarEnabled%%i:3StarDisabled%): +[XPPerReroll] XP per paid reroll, and Crew unit odds never drop with player level. (2x %i:3StarEnabled%%i:3StarDisabled%): +1 free reroll / round. (3x %i:3StarEnabled%%i:3StarDisabled%): Deal [BaseDamageThreshold] damage to fire a rocket dealing [BaseRocketDamage]. (4x %i:3StarEnabled%%i:3StarDisabled%): Fire rockets [RocketRatePerLevel]% more often per player level. (5x %i:3StarEnabled%%i:3StarDisabled%): Fire the Planet Cracker.

  - (2) BaseDamageThreshold=280, BaseRocketDamage=75, RocketRatePerLevel=0.025, StatBonus=0.1, XPPerReroll=1
  - (2) BaseDamageThreshold=280, BaseRocketDamage=75, RocketRatePerLevel=0.025, StatBonus=0.1, XPPerReroll=1
  - (2) BaseDamageThreshold=280, BaseRocketDamage=75, RocketRatePerLevel=0.025, StatBonus=0.1, XPPerReroll=1
  - (2) BaseDamageThreshold=280, BaseRocketDamage=75, RocketRatePerLevel=0.025, StatBonus=0.1, XPPerReroll=1
  - (2) BaseDamageThreshold=280, BaseRocketDamage=75, RocketRatePerLevel=0.025, StatBonus=0.1, XPPerReroll=1
  - (2) BaseDamageThreshold=280, BaseRocketDamage=75, RocketRatePerLevel=0.025, StatBonus=0.1, XPPerReroll=1
  - (2) BaseDamageThreshold=280, BaseRocketDamage=75, RocketRatePerLevel=0.025, StatBonus=0.1, XPPerReroll=1

### Thrillseeker


  - (1) AS=0.4, Duration=3, HealPercent=15
  - (2) AS=0.4, Duration=3, ExecuteThreshold=0.1, HealPercent=15

### Tiny Terror

Gain [TFTTrait.TFT15_MechanicTrait_TinyTerror.1:AS]% Attack Speed. At [TFTTrait.TFT15_MechanicTrait_TinyTerror.1:Threshold]% Health, double it for the rest of combat. Then, shrink in size for [TFTTrait.TFT15_MechanicTrait_TinyTerror.1:Duration] seconds, dodging all attacks.

  - (1) AS=0.15, Duration=3, Threshold=0.7

### Trickster


  - (1) DA=0.04, DummyHealth=250, HealthThreshold=0.5
  - (2) DA=0.06, DummyHealth=1250, HealthThreshold=0.5

### Ultra Stance

Gain [BonusHealth] Health and grant an additional +[BonusTraitAmount] to the selected Stance's trait.

  - (1) BonusHealth=100, BonusTraitAmount=1

### Unflinching

Gain [TFTTrait.TFT15_MechanicTrait_Unflinching.1:OmnivampPct]% Omnivamp. For the first [TFTTrait.TFT15_MechanicTrait_Unflinching.1:Duration] seconds of combat, gain immunity to crowd control.

  - (1) Duration=15, OmnivampPct=25

### Unstoppable

Gain [Health] Health and immunity to crowd control. Charge to new targets, Stunning enemies passed through for [StunDuration] seconds.

  - (1) Health=100, StunDuration=0.75

### Untouchable

Become untargetable while [TFTTrait.TFT15_MechanicTrait_Untouchable.1:NumAllies]+ allies are alive. If this champion ends the fight at full Health, permanently gain [TFTTrait.TFT15_MechanicTrait_Untouchable.1:DamageAmpScaling]% Damage Amp. (Current: [TFTUnitProperty.:TFT15_Untouchable_DamageAmp]% )

  - (1) DamageAmpScaling=0.02, NumAllies=4

### Veteran

This unit is a Mentor. They grant [Amp]% Damage Amp, and at Mentor 4 they gain [Stats]% Attack Damage and Ability Power instead of upgrading their spell.

  - (1) Amp=0.04, Stats=0.3

### Warming Up

At combat start or when this activates gain [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Resists] Armor and Magic Resistance. Every second, convert [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Conversion] resists into [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Conversion]% Attack Damage. (Up to [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Resists])

  - (1) Conversion=3, Resists=48

### Weights

Gain [NumWeights] Weights. At 1+, move and attack [PercentReduction]% slower. Player victories drop [WeightPerCombat] Weights, 2 on a loss. At 0, move faster and gain [AS]% Attack Speed and [Durability]% Damage Reduction. (Weights Left: [TFTUnitProperty.trait:TFT15_MechanicTrait_Weights_NumWeights])

  - (1) AS=0.5, Durability=0.12, NumWeights=10, PercentReduction=0.5, WeightPerCombat=1

### Wind Wall

Yasuo gains [CritChance]% Crit Chance, and takes [ReducedDamage]% reduced damage from non-adjacent enemies.

  - (1) CritChance=0.6, ReducedDamage=0.15

### Wonder Twins


  - (1) Stats=40
  - (2) NumItems=None, Stats=60

### Wraith

Every [RealmTimer] seconds, the Shadow Realm strikes the [NumEnemy] closest enemies, dealing total magic damage equal to a portion of damage dealt by Wraiths since the last trigger. Your lowest health Wraith heals for [HealPercent]% of damage dealt by Wraiths and the Shadow Realm.

  - (2-3) DamagePercent=0.2, HealPercent=0.18, NumEnemy=3, RealmTimer=4
  - (4-5) DamagePercent=0.45, HealPercent=0.18, NumEnemy=3, RealmTimer=4
  - (6) DamagePercent=0.6, HealPercent=0.18, NumEnemy=3, RealmTimer=4

## Champions (65)

### Aatrox (1-cost, ADTank)

- Traits: Mighty Mech, Juggernaut, Heavyweight | mana 20/80
- **Blade Vigor**: Heal [TotalHealing] () and deal [TotalDamage] () physical damage to the current target. Healing and damage are increased by up to 100% based on Aatrox's missing health.

### Ezreal (1-cost, HCaster)

- Traits: Battle Academia, Prodigy | mana 0/50
- **Rising Spell Force**: Fire a blast at the current target, dealing [ModifiedAPDamage] () magic damage and [ModifiedADDamage] () physical damage. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Ezreal_FullPotential_TRA]

### Garen (1-cost, ADTank)

- Traits: Battle Academia, Bastion | mana 20/80
- **Mighty Blade of Justice**: Heal [TotalHealing] () and deal [AdditionalDamage] () physical damage to the target. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Garen_FullPotential_TRA]

### Gnar (1-cost, ADCarry)

- Traits: Luchador, Sniper | mana 0/40
- **Gadagadagada!**: Passive: Gain [ModifiedASPerStack] () Attack Speed for each attack against the same target, up to [MaxAS] (). Active: For 6 seconds, gain 90% Attack Damage and grant two nearby allies 20/25/35% Attack Damage, prioritizing Attack champions.

### Kalista (1-cost, ADCaster)

- Traits: Soul Fighter, Executioner | mana 10/90
- **Spirit Spear**: Hurl a spear at the farthest enemy, dealing [TotalDamage] () physical damage to the first enemy hit. If the enemy dies the spear continues, dealing overkill damage to the next enemy hit.

### Kayle (1-cost, APSpecialist)

- Traits: Wraith, Duelist
- **Unleash the Demon**: Passive: Attacks deal [ModifiedDamage] () bonus magic damage. Gain additional power based on your Tactician level. Level 6: Every 3rd attack launches a wave that deals [AscensionModifiedMagicDamage] () magic damage and 20% Shreds enemies for 3 seconds. Level 9: Every attack launches a wave. Waves travel farther. Shred: Reduce Magic Resist

### Kennen (1-cost, APTank)

- Traits: Supreme Cells, Protector, Sorcerer | mana 30/85
- **Eye of the Hurricane**: Heal [TotalHealing] (), then deal [ModifiedDamage] () magic damage to the 2 nearest enemies and Stun them for 1 seconds.

### Lucian (1-cost, APCaster)

- Traits: Mighty Mech, Sorcerer | mana 0/40
- **Twin Cannon Fire**: Fire 4 shots towards the target. Each deals [ModifiedDamage] () magic damage to the first enemy hit. For each miss, restore 8/8/15 mana.

### Malphite (1-cost, APTank)

- Traits: The Crew, Protector | mana 0/100
- **Regolith Resistance**: Passive: Gain 12 Armor for each item equipped. Active: Deal [ModifiedDamage] () physical damage to enemies in a cone.

### Naafiri (1-cost, ADFighter)

- Traits: Soul Fighter, Juggernaut | mana 0/30
- **Spinning Soul-Saw**: Gain [FinalShield] () shield for 2 seconds, then deal [ModifiedDamage] () physical damage to the target.

### Rell (1-cost, APTank)

- Traits: Star Guardian, Bastion | mana 0/40
- **Unbreakable Celestial Lance**: Deal [ModifiedDamage] () magic damage in a short line through the target, then reduce damage taken by [ModifiedFlatDamageReduction] () for 4 seconds.

### Sivir (1-cost, ADCaster)

- Traits: The Crew, Sniper | mana 0/50
- **Star Cutter**: Launch a returning blade 2-hexes through the current target. It deals [TotalDamage] () physical damage to enemies hit, reduced by 40% per hit.

### Syndra (1-cost, APCaster)

- Traits: Crystal Gambit, Star Guardian, Prodigy | mana 0/30
- **Bright Star Blast**: Deal [ModifiedDamage] () magic damage to the target and reduces their Magic Resist by 10.

### Zac (1-cost, APTank)

- Traits: Wraith, Heavyweight | mana 30/75
- **Reality Ripple**: Heal [ModifiedHeal] () and deal [FinalDamage] () magic damage to enemies within a 1-hex radius.

### Dr. Mundo (2-cost, ADTank)

- Traits: Luchador, Juggernaut | mana 30/90
- **Steel Chair Slam**: Passive: Every 7 seconds the next attack deals [TotalDamage] () bonus physical damage. Active: Gain [TotalHeal] () maximum health over 1.5 seconds and immediately trigger the passive.

### Gangplank (2-cost, ADFighter)

- Traits: Mighty Mech, Duelist | mana 0/50
- **Golden Giga Gun**: Deal [TotalDamage] () physical damage to the lowest health enemy within [ModifiedCastRange] hexes. If the target dies, [ModifiedGoldChance]% chance to gain 1 gold. Gold Earned: [TFTUnitProperty.:TFT15_GangplankSpell_GoldCount]

### Janna (2-cost, APCaster)

- Traits: Crystal Gambit, Protector, Strategist | mana 10/60
- **Crystal Chrysalis**: Grant [ModifiedAllyShield] () shield to the 2 lowest Health allies for 4 seconds and fire 4 butterflies dealing [DamageTotal] () magic damage split between the 2 closest enemies.

### Jhin (2-cost, ADSpecialist)

- Traits: Wraith, Sniper | mana 0/4
- **The Curtain Falls**: Passive: Attack Speed is always 0.75/0.75/0.85. Convert 1% bonus Attack Speed into 0.8% Attack Damage. Every fourth attack is enhanced to deal [TotalDamage] () physical damage.

### Kai'Sa (2-cost, ADCarry)

- Traits: Supreme Cells, Duelist | mana 0/50
- **Missile Massacre**: Passive: Kills grant 1% permanent Attack Damage (+[TFTUnitProperty.:TFT15_KaiSaSpell_PermanentAD]% ). Active: Fire 8 missiles divided between the target and nearby enemies. Each missile deals [TotalDamage] () physical damage.

### Katarina (2-cost, APFighter)

- Traits: Battle Academia, Executioner | mana 0/30
- **Teleports Behind You**: Blink to the enemy with the lowest health within 2 hexes and deal [ModifiedDamage] () magic damage to enemies in a 1-hex radius. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Katarina_FullPotential_TRA]

### Kobuko (2-cost, ADTank)

- Traits: Mentor, Heavyweight | mana 30/85
- **Golden Paw Mantra**: Gain [TotalShield] () Shield for 3 seconds. The next attack deals [ModifiedDamage] () bonus physical damage and Stuns for 1.5 seconds.

### Lux (2-cost, APCaster)

- Traits: Soul Fighter, Sorcerer | mana 10/50
- **Soulfire Explosion**: Fire an energy ball towards the largest group of enemies containing the target. On arrival, 20% Shred enemies in a 1-hex radius for 5 seconds and deal [ModifiedDamage] () magic damage split between them plus [ModifiedBaseDamage] () magic damage to each. Shred: Reduce Magic Resist

### Rakan (2-cost, APTank)

- Traits: Battle Academia, Protector | mana 25/75
- **All Eyes On Me**: Heal [ModifiedHeal] () and deal [ModifiedDamage] () magic damage to the 3 closest enemies. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Rakan_FullPotential_TRA]

### Shen (2-cost, APTank)

- Traits: The Crew, Bastion, Edgelord | mana 25/75
- **Power Suit: Full Charge**: Shield [ModifiedShield] () for 4 seconds. The next 3 attacks deal [ModifiedDamage] () bonus true damage.

### Vi (2-cost, ADTank)

- Traits: Crystal Gambit, Juggernaut | mana 20/75
- **Shining Knuckle Explosion**: Passive: Gain 2 for each component equipped to your team. Active: Heal [ModifiedHeal] (), deal [ModifiedDamage] () physical damage in a 1-hex radius centered on the target, and 20% Sunder enemies hit for 4 seconds. Sunder: Reduce Armor

### Xayah (2-cost, ADCarry)

- Traits: Star Guardian, Edgelord | mana 20/70
- **Sharp Feathers Shine**: Gain [FinalAttackSpeed] () attack speed for 3 attacks. These attacks deal [ModifiedDamage] () bonus physical damage to the target and 2 nearby enemies

### Xin Zhao (2-cost, ADTank)

- Traits: Soul Fighter, Bastion | mana 10/65
- **Guarding Crescent**: Heal [ModifiedHeal] (), deal [ModifiedDamage] () physical damage to enemies in a 2-hex radius, and mark the current target. For 3 seconds, take 40% reduced damage from unmarked enemies.

### Ahri (3-cost, APCaster)

- Traits: Star Guardian, Sorcerer | mana 10/50
- **Pure Heart Breaker**: Deal [ModifiedDamage] () magic damage to the current target, increased up to 50% based on the target's missing health. If the target dies, deal 80% of overkill damage split between the 2 closest enemies, plus [ModifiedSecondaryDamage] () magic damage per target.

### Caitlyn (3-cost, ADCaster)

- Traits: Battle Academia, Sniper | mana 0/60
- **Magic Bouncing Bullet**: Fire a bullet at the farthest enemy, dealing [TotalDamage] () physical damage to the first target hit. The bullet bounces to a nearby enemy, dealing [ModifiedSecondaryDamage] () physical damage. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Caitlyn_FullPotential_TRA]

### Darius (3-cost, ADFighter)

- Traits: Supreme Cells, Heavyweight | mana 0/30
- **Grand Guillotine**: Heal [ModifiedHeal] (), then deal [ModifiedDamage] () physical damage to the current target, increased by 60% against Tanks. If the target dies, immediately recast dealing 75% damage.

### Jayce (3-cost, ADFighter)

- Traits: Battle Academia, Heavyweight | mana 0/40
- **Thundering Hammerstrike**: Deal [TotalDamage] () physical damage to enemies in a 1-hex radius centered on the target. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Jayce_FullPotential_TRA]

### Kog'Maw (3-cost, APCarry)

- Traits: Monster Trainer | mana 0/50
- **Static Surge**: Passive: Attacks deal [ModifiedDamage] () bonus magic damage. Increase Max Attack Speed to 6. Active: The next attack deals [ModifiedExtraProjectileDamage] () bonus magic damage to the target and the 2 closest enemies, then 30% Shred and Sunder them for 4 seconds. On-cast, gain 15% Attack Speed for the rest of combat. Shred: Reduce Magic Resist Sunder: Reduce Armor

### Lulu (3-cost, APCaster)

- Traits: Monster Trainer | mana 0/100
- **Choose A Monster**: Field Lulu to choose a monster to train! Each monster has unique abilities.

### Malzahar (3-cost, APCaster)

- Traits: Wraith, Prodigy | mana 0/30
- **Insidious Whispers**: Infect the closest 1 uninfected targets, dealing [ModifiedDamage] () magic damage over 15 seconds. If an infected target dies, the infection spreads to the nearest uninfected target for any remaining time. If all enemies are infected, this effect can stack.

### Neeko (3-cost, APTank)

- Traits: Star Guardian, Protector | mana 20/70
- **Falling Flower Eruption**: Gain 40% Durability and heal [ModifiedHeal] () over 2 seconds, then deal [ModifiedDamage] () magic damage to enemies in a 2-hex radius.

### Rammus (3-cost, APTank)

- Traits: Monster Trainer | mana 30/90
- **Durian Defense**: Gain [TotalShieldValue] () Shield for 4 seconds. Leap to the largest group within 2 hexes, dealing [TotalDamage] () magic damage and Taunting enemies hit. Gain [ModifiedLeapHealth] () max Health per enemy Taunted. When the Shield breaks, shoot spines at the nearest 3 enemies, repeating the damage and Stunning them for 1/1.25/1.5 second.

### Senna (3-cost, ADCaster)

- Traits: Mighty Mech, Executioner | mana 0/60
- **Super Beam Cannon**: Over 1 second, deal [TotalDamage] () physical damage in a line through the current target. Enemies after the first take [SecondaryDamage] () instead. When no targets are left, regain mana based on the time remaining.

### Smolder (3-cost, ADCaster)

- Traits: Monster Trainer | mana 0/50
- **Burn Blast**: Passive: Damage Burns and Wounds for 5 seconds. Ignore 30% of the target's Armor. Active: Deal [ModifiedDamage] () physical damage to the target, and [ModifiedEmberDamage] () magic damage to the 3 nearest targets. Every 2 casts, fire a second blast at the next closest enemy, dealing 75% damage. {{TFT_Keyword_Burn}} {{TFT_Keyword_Wound}}

### Swain (3-cost, APTank)

- Traits: Crystal Gambit, Bastion, Sorcerer | mana 20/80
- **Glittering Plumage Regalia**: Gain [ModifiedInitialHealth] () max Health, then deal [ModifiedDamage] () magic damage every second to enemies in a 2-hex radius. Subsequent casts heal [ModifiedHealing] () and increase the damage by [ModifiedSecondaryDamage] ().

### Udyr (3-cost, ADTank)

- Traits: Mentor, Juggernaut, Duelist | mana 40/80
- **Blazing Phoenix Stance**: Make 3 rapid attacks. Each heals Udyr for [ModifiedHealing] () and deals [ModifiedAOEDamage] () additional damage to enemies in a 1-hex radius.

### Viego (3-cost, APFighter)

- Traits: Soul Fighter, Duelist, Edgelord | mana 0/40
- **Combo of the Ruined King**: Passive: Attacks deal [ModifiedPassiveDamage] () magic damage. Active: The next three attacks unleash a devastating combo. Pommel Bash: Deal [ModifiedComboDamage] () magic damage. Rising Slash: Deal [ModifiedComboDamage] () magic damage and briefly Stun the target. Falling Slash: Deal [ModifiedFinalComboDamage] () magic damage.

### Yasuo (3-cost, ADFighter)

- Traits: Mentor, Edgelord | mana 0/40
- **Unsheath the Storm**: Deal [TotalDamage] () physical damage to the 3 closest enemies.

### Ziggs (3-cost, APCarry)

- Traits: The Crew, Strategist | mana 0/90
- **Orbital Ordinance**: Passive: Attacks are bouncing bombs that deal [TotalAttackDamage] () magic damage to the first enemy hit. Active: Deal [ModifiedExplosionDamage] () magic damage in a 1-hex radius centered on the target.

### Akali (4-cost, APFighter)

- Traits: Supreme Cells, Executioner | mana 0/40
- **Piercing Phantom Strike**: Mark the farthest unmarked target within 4 hexes, then dash through all marked enemies in order. Each dash deals [Total_Strike_Damage] () magic damage to its target and [Total_Dash_Damage] () magic damage to other enemies hit. If only one enemy remains, dash 3 times instead

### Ashe (4-cost, ADCarry)

- Traits: Crystal Gambit, Duelist | mana 0/70
- **Gates of Avarosa**: The next 8 attacks fire bonus arrows dealing [TotalDamage] () total physical damage split between the target and up to two random enemies within 2 hexes.

### Jarvan IV (4-cost, APTank)

- Traits: Mighty Mech, Strategist | mana 45/150
- **Mega Drill Crash**: Gain [ModifiedShield] () Shield for 4 seconds, then slam into a group of enemies, dealing [ModifiedDamage] () magic damage to enemies within a 2 hex radius and Stunning them for 1.5/1.75/8 seconds.

### Jinx (4-cost, ADCaster)

- Traits: Star Guardian, Sniper | mana 10/80
- **Star Rocket Blast Off!**: Passive: Attacks grant [ModifiedASPerStack]% () stacking Attack Speed, up to [ModifiedMaxAS]% (). Critical strikes instead grant [ASPerCrit]% (). Active: Deal [ModifiedDamage] () physical damage to the target and [ModifiedRocketAOEDamage] () physical damage split across a 2-hex radius centered on the target.

### K'Sante (4-cost, ADTank)

- Traits: Wraith, Protector | mana 30/90
- **Breakout Force**: Passive (ALL OUT): On first death instead heal to full, gain 125/125/300% Attack Speed, convert 50% of Armor and Magic Resist into Attack Damage, and set max mana to 30. Then dash through the target and briefly Stun them. While ALL OUT, lose 6/6/0% Health each second and heal [ModifiedHeal] () on kill. Active: Gain [ModifiedDurability] () Durability for 3 seconds, then deal [ModifiedTankDamage] () physical damage to the target. Active (ALL OUT): . Deal [ModifiedFighterDamage] () physical damage to the target.

### Karma (4-cost, APCaster)

- Traits: Mighty Mech, Sorcerer | mana 0/65
- **Destruction Disc**: Fire an energy disc that deals [ModifiedDamage] () total magic damage in a 1-hex radius centered on the target.

### Leona (4-cost, APTank)

- Traits: Battle Academia, Bastion | mana 30/90
- **Aegis of Unbreakable Dawn**: Shield for [ModifiedShield] () for 4 seconds and deal [ModifiedDamage] () magic damage to enemies in a 2-hex radius. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Leona_FullPotential_TRA]

### Poppy (4-cost, ADTank)

- Traits: Star Guardian, Heavyweight | mana 45/105
- **Spinning Galaxy Hammer**: Gain [ModifiedShield] () Shield for 4 seconds. When it breaks, deal [ModifiedDamage] () physical damage to the target and [SecondaryDamage] () physical damage to other enemies in a 1-hex radius centered on the target. The target is Stunned for 0.5/0.5/8-[MaxStunDuration] seconds based on how long the shield held, and other enemies are briefly Stunned.

### Ryze (4-cost, APCaster)

- Traits: Mentor, Executioner, Strategist | mana 10/60
- **Tsunami Beam**: Deal [ModifiedDamage] () magic damage to the target over 3 seconds. Enemies in a 1-hex radius around the target take [ModifiedAdditionalDamage] () over the same duration.

### Samira (4-cost, ADCaster)

- Traits: Soul Fighter, Edgelord | mana 0/15
- **Soul Shot / Inner Arsenal Unleashed**: Passive: After casting, gain 1 Style and 0.05% Omnivamp, up to 4 stacks. At max stacks, the next cast is Inner Arsenal Unleashed. Soul Shot: Deal [ModifiedQBulletDamage] () physical damage to the target and dash a short distance. Inner Arsenal Unleashed: Double the passive bonus and leap to a nearby group of enemies, dealing [ModifiedDamage] () to all enemies in a 2-hex radius. Reset your Style to 0.

### Sett (4-cost, ADTank)

- Traits: Soul Fighter, Juggernaut | mana 40/100
- **Maximum Grit Overdrive**: Over a short duration, heal [ModifiedHeal] () and deal [ModifiedFlurryDamage] () physical damage to the target. Then, deal [ModifiedConeDamage] () physical damage in a cone. Enemies in the center line take [ModifiedTrueDamage] more physical damage, increased based on healing received this combat.

### Volibear (4-cost, ADFighter)

- Traits: Luchador, Edgelord | mana 0/40
- **Ursine Instinct**: Passive: Every 4th attack deals [SlamDamage] () physical damage in a 1-hex radius centered on the target. When changing targets, immediately slam for 70% damage. Active: For 5 seconds gain [ModifiedAttackSpeed] Attack Speed, [ModifiedOmnivamp]% () Omnivamp, and crowd control immunity.

### Yuumi (4-cost, APCaster)

- Traits: Battle Academia, Prodigy | mana 0/40
- **Read Or Die**: Launch [ModifiedNumPages] pages dealing [ModifiedDamage] () magic damage each, split between the target and enemies within 2 hexes. Each subsequent cast launches 4 more pages. Potential ([TFTUnitProperty.:TFT15_BattleAcademia_CurrentPotential]%i:set14AmpIcon%): [TFTUnitProperty.:TFT15_BattleAcademia_Yuumi_FullPotential_TRA]

### Braum (5-cost, ADFighter)

- Traits: The Champ, Luchador, Bastion | mana 0/60
- **El Tigre Tornado**: Stun the target and spin them around for 1.5 seconds, dealing [ModifiedDamage] () physical damage to the target and [ModifiedAOEDamage] () to other enemies in a 2-hex radius. Then throw the target forward, dealing [ModifiedThrowDamage] () in a 1-hex area around the target. If the target drops below [ExecuteThreshold] () health, throw them off the board instead.

### Gwen (5-cost, APFighter)

- Traits: Soul Fighter, Sorcerer | mana 0/40
- **Soul Stitch / Rip The Threads**: Passive: Attacks deal [ModifiedConeDamage] () magic damage, and [ModifiedConeSplitDamage] () magic damage split between enemies in a cone. Every 3rd cast is Rip the Threads. Soul Stitch: Fire 3 needles through the board, dealing [ModifiedNeedleDamage] () magic damage to enemies hit and leaving threads behind. Rip The Threads: Deal [ModifiedDividedDamage] () split between all enemies and [ModifiedBurstDamage] () bonus magic damage to each. Enemies near at least one thread take 10% more damage.

### Lee Sin (5-cost, ADFighter)

- Traits: Stance Master | mana 0/100
- **A Demonstration of Skill**: Field Lee Sin to choose a stance. Each stance has a unique ability, and grants Lee Sin a bonus trait.

### Lee Sin (5-cost, ADFighter)

- Traits: Executioner, Duelist, Juggernaut | mana 0/100
- **A Demonstration of Skill**: Field Lee Sin to choose a stance. Each stance has a unique ability, and grants Lee Sin a bonus trait.

### Seraphine (5-cost, APCaster)

- Traits: Star Guardian, Prodigy | mana 0/140
- **Ultimate Friendship Bomb**: Passive: Ultimate Friendship Bomb's damage scales on Power of Friendship, which is based on all of your team's bonus stats (). Stats on Seraphine contribute more. Active: Fire a friendship bomb towards the largest group of enemies within [ModifiedCastRange] () hexes, dealing [TFTUnitProperty.trait:TFT15_Seraphine_PowerOfFriendship] (Power of Friendship) magic damage in a 2-hex radius.

### Twisted Fate (5-cost, HCarry)

- Traits: Rogue Captain, The Crew | mana 40/120
- **52 Card Calamity**: Passive: Attacks bounce between 3 enemies, dealing [ModifiedPassiveDamage] () bonus magic damage and marking enemies hit. Attacks deal 30% reduced damage per hit. Active: Deal [ModifiedDamage] () physical damage to the 4/4/20 closest enemies, and detonate their marks for [ModifiedMagicDamage] () magic damage each.

### Varus (5-cost, ADCaster)

- Traits: Wraith, Sniper | mana 10/85
- **Level Zero: 100 Shot Demon**: Passive: Gain mana when units die. Enemies grant 5, allies 3. Active: Fire [ModifiedArrowsPerSecond] () piercing arrows per second at random enemies until Varus is out of mana. Each arrow costs 12/12/6 mana and deals [TotalArrowDamage] () physical damage, reduced by 35% per hit.

### Yone (5-cost, ADFighter)

- Traits: Mighty Mech, Edgelord | mana 80/180
- **Dual Blade Technique: Inner Reflection**: Passive: Attacks grant 5/5/25% stacking Attack Speed and Movement Speed, and alternate between dealing [ModifiedAttackTrueDamage] () bonus true damage and [ModifiedAttackMagicDamage] () bonus magic damage. Active: Send an echo towards the two furthest enemies, dealing [ModifiedStrikeDamage] () physical damage and briefly Stunning enemies hit.

### Zyra (5-cost, APCaster)

- Traits: Crystal Gambit, Rosemother | mana 40/100
- **Crystal Thorn Expansion**: Summon 4/4/20 untargetable Thornspitters that deal [ModifiedDamage] () per attack and attack 4 times. Non-plant allies gain [ModifiedAS] () Attack Speed decaying over 4 seconds.

## Monsters, summons, pets and other trait-less units (21)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Deadly Spine (1-cost, ADCarry)

- Traits: (none)
- **Crystal Barbs**: Attacks deal [ModifiedDamage] () bonus magic damage.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Grasping Root (1-cost, APTank)

- Traits: (none)
- **Crystal Barbs**: Attacks deal [ModifiedDamage] () bonus magic damage.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Test Cost (1-cost, )

- Traits: (none) | mana 0/100
- **Test Cost**: -Jinx

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Rammus (3-cost, )

- Traits: (none) | mana 0/100
- **Durian Defense**: Gain [TotalShieldValue] () Shield for [ShieldDuration] seconds. Leap to the largest group within 2 hexes, dealing [TotalDamage] () magic damage and Taunting enemies hit. Gain [ModifiedLeapHealth] () max Health per enemy Taunted. When the Shield breaks, shoot spines at the nearest 3 enemies, repeating the damage and Stunning them for [StunDuration] second.

### Ekko (5-cost, APFighter)

- Traits: (none) | mana 50/110
- **The World Stands Still**: Ekko was pulled from another universe by the Isekai augment. Freeze all units while attacking the nearest 4 enemies 7 total times. Each attack deals [ModifiedDamage] () bonus magic damage,

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Crystalline Radiance (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### PetChoncc (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### The Crew Ship (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### The Mighty Mech (11-cost, APTank)

- Traits: (none) | mana 30/70
- **Piston Punch**: Gain [ModifiedCoreEnergyShield] () Shield for 3/4/4 seconds and deal [ModifiedSpell1Damage] () magic damage to the target.

### The Power Of The Stars (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### The Shadow Realm (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 


<a id="set-16"></a>

# Set 16: Lore & Legends

Source: current client data. 130 champions, 53 traits.

## Traits

### Arcanist

Your team gains Ability Power, Arcanists gain more.

  - (2-3) AllyAP=15, BonusAP=25
  - (4-5) AllyAP=20, BonusAP=50
  - (6) AllyAP=35, BonusAP=70

### Ascendant

After each combat, an Ascendant Charm appears in your shop. These are powerful effects that can be bought for gold.[TFTUnitProperty.trait:TFT16_XerathUnique_PurchasedCharmDisplay] In Choncc's Treasure, the Faithful Followers Charm cannot appear until you are level 7.

  - (1) (no numbers)

### Assimilator

Kai'Sa has a different Ability depending if her Attack Damage or Ability Power is higher.

  - (1) (no numbers)

### Bilgewater

Each player combat, gain Silver Serpents (), plus [SerpentsPerTakedown] additional for each Bilgewater takedown. Silver Serpents can be spent in the Black Market to grant Bilgewater champions bonus stats and special loot. Higher tiers unlock rarer loot. Current Bonus Stats: [TFTUnitProperty.:TFT16_Bilgewater_BonusAD]% , [TFTUnitProperty.:TFT16_Bilgewater_BonusAS]% , [TFTUnitProperty.:TFT16_Bilgewater_BonusHealth]% Current Serpents: [TFTUnitProperty.:TFT16_Bilgewater_SilverSerpents]

  - (3-4) MaxItems=8, SerpentsPerRound=15, SerpentsPerTakedown=1, StatMult=1, 300, 1.5, 2
  - (5-6) MaxItems=8, SerpentsPerRound=35, SerpentsPerTakedown=1, StatMult=1, 300, 1.5, 2
  - (7-9) MaxItems=8, SerpentsPerRound=65, SerpentsPerTakedown=1, StatMult=1, 300, 1.5, 2
  - (10) MaxItems=8, SerpentsPerRound=150, SerpentsPerTakedown=1, StatMult=2, 300, 1.5, 2

### Blacksmith

After each player combat your strongest Ornn participates in, he makes progress forging an Artifact item. Each item he is equipped with grants him [FlatHealthPerItem1Star]/[FlatHealthPerItem2Star]/[FlatHealthPerItem3Star] max Health. Artifacts grant [ArtifactMult]% more. Forging Progress: [TFTUnitProperty.:TFT16_OrnnProgress]/[RoundsToCraft]

  - (1) ArtifactMult=3, FlatHealthPerItem1Star=350, FlatHealthPerItem2Star=600, FlatHealthPerItem3Star=2000, RoundsToCraft=4, None

### Bruiser

Your team gains [TeamFlatHealth] maximum Health. Bruisers gain more.

  - (2-3) BonusPercentHealth=0.25, TeamFlatHealth=150
  - (4-5) BonusPercentHealth=0.4, TeamFlatHealth=150
  - (6) BonusPercentHealth=0.6, TeamFlatHealth=150

### Caretaker

After winning player combat, gain a random unit from Runeterra. After losing combat, gain free rerolls, with the amount increasing by stage. Rerolls on Loss: [TFTUnitProperty.:TFT16_Caretaker_NumRerolls]

  - (1) (no numbers)

### Chainbreaker

Sylas rotates between 3 different Abilities, depending on which one is most useful at the moment. He cannot cast the same one twice in a row.

  - (1) (no numbers)

### Chosen Wolves

When Kindred casts, Ambessa always gains their protection against death and gains [AmbessaBonusAD]% Attack Damage while protected. When Ambessa casts, Wolf strikes all enemies hit, dealing [WolfDamage]% damage.

  - (2) AmbessaBonusAD=0.4, WolfDamage=0.3

### Chronokeeper

Every [CastsToGainXP] casts, the Chronokeeper stores [XPPerCast] XP. If he has enough stored for you to level up, he transfers you the required XP. If you are level 10, he instead gains [DAGainAtMaxLevel]% Damage Amp. Stored XP: [TFTUnitProperty.trait:TFT16_ChronoKeeperStoredXP] In Choncc's Treasure, the amount of casts required to gain XP is increased to [CastsToGainXPCT] until Stage 3-5.

  - (1) CastsToGainXP=2, CastsToGainXPCT=3, CastsToGainXPPAIRS=2, DAGainAtMaxLevel=15, XPPerCast=2, None

### Dark Child

Annie summons Tibbers on your bench. He can be fielded, takes up a team slot, and can be equipped with items separately. His star level is the same as Annie's.

  - (1) (no numbers)

### Darkin


  - (1) Damage=400, EnemyNum=2, HealThreshold=525, TacticianPercent=0.25, Vamp=0.15
  - (2) Damage=400, EnemyNum=2, HealThreshold=525, TacticianPercent=0.25, Vamp=0.15
  - (3) Damage=400, EnemyNum=2, HealThreshold=525, TacticianPercent=0.25, Vamp=0.15

### Defender

Your team gains [TeamwideArmorMR] Armor and Magic Resist. Defenders gain more.

  - (2-3) BonusArmorMR=30, TeamwideArmorMR=12
  - (4-5) BonusArmorMR=55, TeamwideArmorMR=12
  - (6) BonusArmorMR=80, TeamwideArmorMR=12

### Demacia

Each time your team loses [MaxHealthLost]% max Health, Demacians Rally, reducing their Mana cost by [ManaReductionPct]%. Demacians gain Armor and Magic Resist.

  - (3-4) ArmorMR=12, EnemyTrueDamage=0.05, ManaReductionPct=0.1, MaxHealthLost=0.25, 0.25, 1500, 3, 0.5
  - (5-6) ArmorMR=35, EnemyTrueDamage=0.05, ManaReductionPct=0.1, MaxHealthLost=0.25, 0.25, 1500, 3, 0.5
  - (7-10) ArmorMR=35, EnemyTrueDamage=0.05, ManaReductionPct=0.1, MaxHealthLost=0.25, 0.25, 1500, 3, 0.5
  - (11) ArmorMR=150, EnemyTrueDamage=0.05, ManaReductionPct=0.1, MaxHealthLost=0.25, 0.25, 1500, 3, 0.5

### Disruptor

Disruptors' abilities Dazzle enemies hit. Disruptors deal bonus damage to Dazzled enemies. Dazzle: Reduce damage dealt by [DamageReduction]%

  - (2-3) DamageAmp=0.18, DamageReduction=0.1
  - (4) DamageAmp=0.4, DamageReduction=0.1

### Dragonborn

On cast, Shyvana replaces her spell with Flame Breath. While transformed, all allies take [AbilityDamageReduction]% less damage from enemy Abilities.

  - (1) AbilityDamageReduction=0.08

### Dragonguards

When Jarvan casts, Shyvana gains [JarvanShieldShare]% of the Shield. When Shyvana casts, Jarvan jumps in and deals [JarvanResistDamage1]/[JarvanResistDamage2]/[JarvanResistDamage3]/[JarvanResistDamage4]% of his Armor and Magic Resist as magic damage in a 2-hex radius.

  - (2) JarvanResistDamage1=1, JarvanResistDamage2=1, JarvanResistDamage3=1.25, JarvanResistDamage4=1.5, JarvanShieldShare=0.75

### Emperor

Azir deploys two Guards who can be placed anywhere on the battlefield. They do not move or attack, and die when Azir does.

  - (1) (no numbers)

### Eternal

Every 3rd attack from Kindred on the same enemy deals [PercentDamageMult]% damage, as Wolf mauls their target.

  - (1) PercentDamageMult=3.25

### Freljord

Summon a Frozen Tower. Allies in front of it gain Health and allies behind it gain Damage Amp. Freljordians gain [FreljordBonus]% more.

  - (3-4) DA=0.1, FreljordBonus=1.5, HealthPercent=0.08, StunDuration=None, None, 1
  - (5-6) DA=0.16, FreljordBonus=1.5, HealthPercent=0.12, StunDuration=None, 1, 2
  - (7) DA=0.22, FreljordBonus=1.5, HealthPercent=0.16, StunDuration=1.5, 1, 2

### Glutton

Once per Planning phase, you can feed Tahm Kench a champion, permanently granting him either Health, Attack Speed, or Ability Power based on their role and star level. To feed, hold an ally from the bench over Tahm Kench until his mouth opens, then release. Current Stats: [TFTUnitProperty.:TFT16_GluttonStat_HP] , [TFTUnitProperty.:TFT16_GluttonStat_AS]% , [TFTUnitProperty.:TFT16_GluttonStat_AP]%

  - (1) 20, 1.25

### Gunslinger

Gunslingers gain Attack Damage. Every [NumAttacks]th attack from Gunslingers deals bonus physical damage.

  - (2-3) AD=0.22, BonusDamage=100, NumAttacks=4
  - (4) AD=0.44, BonusDamage=200, NumAttacks=4

### Harvester

Each time an enemy champion dies, gain [ManaPerEnemyDeath] Mana. Casting abilities does not consume Mana. All enemies lose [EnemyArmorMRReduction] Armor and Magic Resist.

  - (1) EnemyArmorMRReduction=10, ManaPerEnemyDeath=10

### Heroic

Galio cannot be fielded in combat. Instead, his traits count from the bench and when Demacians Rally, he joins the fight. On landing, he creates a [HexRadius]-hex shockwave that deals [PercentMaxHP]% of his max Health as magic damage. Enemies hit are knocked up for [KnockupDuration] seconds, reduced by distance from the center.

  - (1) HexRadius=3, KnockupDuration=1, PercentMaxHP=0.1

### HexMech

Gain a Pilot Hex. Combat Start: the unit within jumps into T-Hex, granting her [PercentHealth]% of their Health and bonus stats based on their role and star level. Tank: +[Vamp1]/[Vamp2]/[Vamp3]% Omnivamp Fighter: +[AD1]/[AD2]/[AD3]% Attack Damage Marksman: +[Amp1]/[Amp2]/[Amp3]% Damage Amp Caster: +[Mana1]/[Mana2]/[Mana3] Mana Regen Assassin: +[Crit1]/[Crit2]/[Crit3]% Crit Chance On death, the pilot ejects while T-Hex knocks back and Stuns nearby enemies for [StunDuration] seconds.

  - (1) AD1=20, AD2=30, AD3=40, Amp1=12, Amp2=20, Amp3=30, Crit1=40, Crit2=70, Crit3=100, Mana1=2, Mana2=4, Mana3=5, PercentHealth=0.8, StunDuration=1.5, Vamp1=20, Vamp2=30, Vamp3=40, 0.5

### Huntress

While your strongest Neeko is alive, your strongest Nidalee cannot be targeted by basic attacks and gains [Durability]% Durability.

  - (1) Durability=0.5

### Immortal

While Xin Zhao is not on your board, Zaahen gains the Ionia, Demacia, and Warden traits. On kill, Zaahen gains a stack of Determination for the rest of combat. If he has [FocusNeededToRevive] or more Determination when dying, he spends it to revive and restore [PercentMaxHealthHeal]% max Health.

  - (1) FocusNeededToRevive=3, PercentMaxHealthHeal=0.6, ReviveDuration=0.5, None

### Invoker

Your team gains [TeamBonusMana] Mana Regen. Invokers gain more Mana from all sources.

  - (2-3) PercentManaIncrease=0.25, TeamBonusMana=1
  - (4) PercentManaIncrease=0.4, TeamBonusMana=1

### Ionia

Ionians gain Shield, Attack Damage, and Ability Power. Each game, they walk a different path. [TFTUnitProperty.trait:TFT16_Ionia_PathTooltip]

  - (3-4) BladesAP=15, BladesFlatDamage=None, BladesPercentChance=30, DeterminationFlatDamage=20, DeterminationResists=15, EnlightenmentADAP=10, EnlightenmentADAPPerLevel=2, EnlightenmentXP=1, GenerosityADAP=10, GenerosityIncreasePerGold=0.02, SpiritADAP=3, SpiritHealth=0.25, Transcendence3StarBuff=1.3, TranscendenceHealth=0.1, TranscendenceMagicDamage=0.2, 5
  - (5-6) BladesAP=25, BladesFlatDamage=None, BladesPercentChance=38, DeterminationFlatDamage=40, DeterminationResists=25, EnlightenmentADAP=15, EnlightenmentADAPPerLevel=3, EnlightenmentXP=2, GenerosityADAP=25, GenerosityIncreasePerGold=0.02, SpiritADAP=4, SpiritHealth=0.3, Transcendence3StarBuff=1.3, TranscendenceHealth=0.15, TranscendenceMagicDamage=0.25, 4
  - (7) BladesAP=50, BladesFlatDamage=None, BladesPercentChance=45, DeterminationFlatDamage=60, DeterminationResists=40, EnlightenmentADAP=20, EnlightenmentADAPPerLevel=4, EnlightenmentXP=4, GenerosityADAP=40, GenerosityIncreasePerGold=0.02, SpiritADAP=5, SpiritHealth=0.35, Transcendence3StarBuff=1.3, TranscendenceHealth=0.2, TranscendenceMagicDamage=0.3, 3
  - (10) BladesAP=100, BladesFlatDamage=25, BladesPercentChance=70, DeterminationFlatDamage=100, DeterminationResists=75, EnlightenmentADAP=100, EnlightenmentADAPPerLevel=None, EnlightenmentXP=4, GenerosityADAP=50, GenerosityIncreasePerGold=0.02, SpiritADAP=7, SpiritHealth=0.75, Transcendence3StarBuff=1.3, TranscendenceHealth=0.3, TranscendenceMagicDamage=0.7, 0.03, 1, 0.75, 0.3, 3

### Ixtal

Completing a Quest before combat ends grants you a new one at the start of next round.

  - (3-4) BonusHealth_Pairs=None, Heal=None
  - (5-6) BonusHealth_Pairs=250, Heal=2
  - (7) BonusClues=50, BonusHealth_Pairs=250, Heal=2

### Juggernaut

Juggernauts gain Durability, increased while above [HealthBreakpoint]% health. When a Juggernaut dies, other Juggernauts heal for [HealRatio]% of their max Health.

  - (2-3) BaseDR=0.18, HealRatio=0.05, HealthBreakpoint=0.5, IncreasedDR=0.25
  - (4-5) BaseDR=0.2, HealRatio=0.05, HealthBreakpoint=0.5, IncreasedDR=0.3
  - (6) BaseDR=0.26, HealRatio=0.05, HealthBreakpoint=0.5, IncreasedDR=0.36

### Longshot

Longshots gain Damage Amp and deal more damage to targets farther away.

  - (2) BaseDamageAmp=0.18, PercentDamageIncrease=0.02
  - (3) BaseDamageAmp=0.24, PercentDamageIncrease=0.03
  - (4) BaseDamageAmp=0.28, PercentDamageIncrease=0.04
  - (5) BaseDamageAmp=0.32, PercentDamageIncrease=0.05

### Noxus

After the enemy team has lost [HPLostTrigger]% of their Health, summon Atakhan, Bringer of Ruin. Each Noxian champion's star level increases his power.

  - (3-4) HPLostTrigger=0.15, RuinTimer=10
  - (5-6) HPLostTrigger=0.15, RuinTimer=10
  - (7-9) HPLostTrigger=0.15, RuinTimer=10
  - (10) HPLostTrigger=0.15, RuinTimer=10

### Piltover

Build an invention on the right side of the board. After [InventionFireTime] seconds, the invention activates, triggering each of its Modules.

  - (2-3) InventionFireTime=8
  - (4-5) InventionFireTime=8
  - (6) InventionFireTime=8

### Poison Pals

Gain a 2-star Teemo. Singed's damage refreshes Teemo's poisons and Teemo's damage grants Singed ramping Ability Power. Gain a Teemo after each player combat.

  - (2) SingedAPGain=2, TeemoDamageReq=100

### Quickstriker

Your team gains [TeamwideAS]% Attack Speed. Quickstrikers gain bonus Attack Speed based on their target's missing Health.

  - (2) MaxBonusAS=0.3, MinBonusAS=0.1, TeamwideAS=0.15
  - (3) MaxBonusAS=0.45, MinBonusAS=0.2, TeamwideAS=0.15
  - (4) MaxBonusAS=0.6, MinBonusAS=0.3, TeamwideAS=0.15
  - (5) MaxBonusAS=0.8, MinBonusAS=0.4, TeamwideAS=0.15

### Riftscourge

Baron Nashor takes up 2 team slots and grants +2 to the Void trait. After [VoidTime] seconds, the board enters a Void Rift. The Rift grants Void units [DamageAmp]% Damage Amp and constantly strikes enemies with bolts of plasma, dealing [LightningPercentHealth]% max Health as magic damage.

  - (1) DamageAmp=0.3, LightningPercentHealth=0.05, VoidTime=8, 0.5

### Rune Mage

The Rune Mage benefits from all active Region traits, but contributes to none. He harnesses the Runic power from active Region traits to modify his Ability, enhancing it in unique ways.

  - (1) (no numbers)

### Sentinels of Light

When Vayne casts on an enemy afflicted with Black Mist, she fires [NumBonusBolts] extra bolts at [BoltEffectiveness]% effectiveness. When she gets a takedown, Lucian and Senna gain [LucianSennaAS]% Attack Speed.

  - (2) BoltEffectiveness=0.25, LucianSennaAS=0.5, NumBonusBolts=2

### Shadow Isles

Whenever ANY champion dies in a player combat, gain Souls (). Shadow Isles champions' Abilities are uniquely empowered based on the number of Souls you have. Shadow Isles champions gain Attack Damage and Ability Power. Souls Collected: [TFTUnitProperty.trait:TFT16_ShadowIsles_SoulsPlayerFacing]

  - (2) ADAP=0.18, SoulMult=1, 135, 65, None, 25, 1, 160, 215, 255, 185
  - (3) ADAP=0.2, SoulMult=1.3, 135, 65, None, 25, 1, 160, 215, 255, 185
  - (4) ADAP=0.25, SoulMult=1.6, 135, 65, None, 25, 1, 160, 215, 255, 185
  - (5) ADAP=0.33, SoulMult=1.9, 135, 65, None, 25, 1, 160, 215, 255, 185

### Shurima

Each second, Shurimans gain [ASPerSecond]% Attack Speed and restore [HealthRestorePerSecond] Health. At combat start, they gain additional effects. Prismatic: Unlock Nasus, Renekton, and Xerath, then field 4 Shurima

  - (2) ASPerSecond=0.02, ArmorMR=None, BonusHealth=0.2, GodhoodCadence=None, HealthRestorePerSecond=20, SkinScale=0.08, None
  - (3) ASPerSecond=0.02, ArmorMR=50, BonusHealth=0.2, GodhoodCadence=None, HealthRestorePerSecond=20, SkinScale=0.08, None
  - (4) ASPerSecond=0.02, ArmorMR=50, BonusHealth=0.2, GodhoodCadence=3, HealthRestorePerSecond=20, SkinScale=0.08, 1
  - (4) ASPerSecond=0.02, ArmorMR=50, BonusHealth=0.2, GodhoodCadence=None, HealthRestorePerSecond=20, SkinScale=0.08, None

### Slayer

Slayers gain Omnivamp and Attack Damage. They increase the bonus Attack Damage by up to [AmountIncrease]% based on their missing Health.

  - (2-3) AmountIncrease=0.5, BonusAD=0.2, BonusOmnivamp=0.1
  - (4-5) AmountIncrease=0.5, BonusAD=0.3, BonusOmnivamp=0.16
  - (6) AmountIncrease=0.5, BonusAD=0.4, BonusOmnivamp=0.2

### Soulbound

Lucian and Senna fight together, swapping when either casts. Each champion has a unique ability. Your strongest Soulbound pair grants bonuses to your team while they survive. While Senna is out, all allies gain [SennaDR]% Damage Reduction. While Lucian is out, all allies gain [LucianDA]% Damage Amp.

  - (1) LucianDA=0.07, SennaDR=0.07

### Star Forger

Aurelion Sol gains [BonusStardustPerOneStar]/[BonusStardustPerTwoStar]/[BonusStardustPerThreeStar]% increased Stardust for each other unique Targonian fielded in combat. Current Bonus: [TFTUnitProperty.:TFT16_AurelionSol_StarMod]%

  - (1) BonusStardustPerOneStar=0.25, BonusStardustPerThreeStar=1, BonusStardustPerTwoStar=0.3

### Targon

Targonians are forged by the stars. They are traitless, but stronger than the average champion.

  - (1) (no numbers)

### The Boss

When Sett first drops below [MaxHealthThreshold]% max Health, he leaves combat to start doing sit-ups. Each sit-up restores [HealPercent]% max Health and gives him [AttackSpeedPercent]% Attack and Move Speed. If he reaches full Health, he returns to combat Pumped Up, converting all damage from his attacks and Ability to true damage. If all of his allies die, he immediately returns to combat.

  - (1) AttackSpeedPercent=60, HealPercent=15, MaxHealthThreshold=0.45

### Timewinders


  - (1) DamageShare=0.1, TOOLTIPONLYXP=2
  - (2) DamageShare=0.1, TOOLTIPONLYXP=2

### Vanquisher

Vanquishers' abilities can critically strike. They also gain Critical Strike Chance and Critical Strike Damage.

  - (2) BaseCritChance=15, CritDmg=15
  - (3) BaseCritChance=20, CritDmg=20
  - (4) BaseCritChance=25, CritDmg=25
  - (5) BaseCritChance=30, CritDmg=30

### Void

Gain Mutations that only Void champions can use. Void champions gain Attack Speed. Champions can only have one Mutation at a time. Benching a champion removes their Mutation.

  - (2-3) BonusAS=0.08, NumMutations=1, SuperchargedMult=None
  - (4-5) BonusAS=0.18, NumMutations=2, None
  - (6-8) BonusAS=0.33, NumMutations=3, SuperchargedMult=None
  - (9) BonusAS=0.4, NumMutations=3, SuperchargedMult=1

### Warden

When Wardens first drop below [FirstShieldPercentHealth]% and [SecondShieldPercentHealth]% Health, they gain a Shield based on their max Health.

  - (2) FirstShieldPercentHealth=0.75, PercentHealthShield=0.16, SecondShieldPercentHealth=0.25
  - (3) FirstShieldPercentHealth=0.75, PercentHealthShield=0.2, SecondShieldPercentHealth=0.25
  - (4) FirstShieldPercentHealth=0.75, PercentHealthShield=0.26, SecondShieldPercentHealth=0.25
  - (5) FirstShieldPercentHealth=0.75, PercentHealthShield=0.33, SecondShieldPercentHealth=0.25

### World Ender

Aatrox gains Attack Damage equal to [OmnivampToADRatio]% of his Omnivamp. On first death, he becomes briefly untargetable and heals back to full Health over [HealDuration] seconds. Afterwards, he reduces his maximum Mana by [ManaReduction] and gains [Durability]% Durability.

  - (1) Durability=0.2, HealDuration=2, ManaReduction=10, OmnivampToADRatio=2

### Yordle


  - (2-3) AS=0.05, BonusHealth=40, BonusMultiplier=1.2, BonusStatMultiplier=1, FreeRerolls=None, None, None, 700, 1.5, 800, 200, None
  - (4-5) AS=0.05, BonusHealth=40, BonusMultiplier=1.2, BonusStatMultiplier=1, FreeRerolls=None, 1, None, 700, 1.5, 800, 200, None
  - (6-7) AS=0.05, BonusHealth=40, BonusMultiplier=1.2, BonusStatMultiplier=1, FreeRerolls=1, 1, None, 700, 1.5, 800, 200, None
  - (8) AS=0.05, BonusHealth=40, BonusMultiplier=1.2, BonusStatMultiplier=1, FreeRerolls=1, 1, 1, 700, 1.5, 800, 200, None
  - (10) AS=0.05, BonusHealth=40, BonusMultiplier=1.2, BonusStatMultiplier=1, FreeRerolls=1, 1, 1, 700, 1.5, 800, 200, 1

### Zaun

After [InitialDelay] seconds, Zaunites become Shimmer-Fused, granting them [ShimmerFuseDurability]% Durability and [ShimmerFuseAttackSpeed]% decaying Attack Speed for [ShimmerFuseDuration] seconds. After a short cooldown, they become Shimmer-Fused again.

  - (3-4) HealDuration=3, HealThreshold=0.6, InitialDelay=4, MaxHealthHeal=None, RefreshSeconds=4, ReviveDuration=3, ShimmerFuseAttackSpeed=0.9, ShimmerFuseDurability=0.15, ShimmerFuseDuration=4, ShimmerFustMult=None, 0.35
  - (5-6) HealDuration=3, HealThreshold=0.6, InitialDelay=4, MaxHealthHeal=0.3, RefreshSeconds=4, ReviveDuration=3, ShimmerFuseAttackSpeed=0.9, ShimmerFuseDurability=0.15, ShimmerFuseDuration=4, ShimmerFustMult=None, 0.35
  - (7) HealDuration=3, HealThreshold=0.6, InitialDelay=4, MaxHealthHeal=0.3, RefreshSeconds=3, ReviveDuration=3, ShimmerFuseAttackSpeed=0.9, ShimmerFuseDurability=0.15, ShimmerFuseDuration=4, ShimmerFustMult=0.5, 0.35

## Champions (101)

### Anivia (1-cost, APCaster)

- Traits: Freljord, Invoker | mana 0/40
- **Frostbite**: Fire an ice shard at target that deals [ModifiedDamage] () magic damage. If they are Chilled, the damage Critically Strikes. {{TFT_Keyword_Chill}}

### Blitzcrank (1-cost, APTank)

- Traits: Zaun, Juggernaut | mana 60/105
- **Static Field**: Gain [ModifiedShield] () Shield for 4 seconds and deal [ModifiedDamage] () magic damage to all enemies within 1 hex.

### Briar (1-cost, ADReaper)

- Traits: Noxus, Slayer, Juggernaut | mana 0/40
- **Blood Frenzy**: Leap to the furthest enemy within 2 hexes. For 4 seconds gain increased Move Speed, 25% Attack Damage, and [ModifiedAttackSpeed] () decaying Attack Speed.

### Caitlyn (1-cost, ADCaster)

- Traits: Piltover, Longshot | mana 0/80
- **Ace in the Hole**: Fire a bullet towards the farthest enemy, dealing [TotalDamage] () physical damage to the first enemy it hits. If they die, the bullet ricochets, dealing excess damage to the next enemy hit.

### Illaoi (1-cost, ADTank)

- Traits: Bilgewater, Bruiser | mana 40/100
- **Tentacle Smash**: Restore [ModifiedHeal] () Health and slam a tentacle, dealing [ModifiedDamage] () physical damage to enemies in a line.

### Jarvan IV (1-cost, APTank)

- Traits: Demacia, Defender | mana 30/100
- **Demacian Standard**: Gain [ModifiedShield] () Shield for 4 seconds. Grant all allies [ModifiedAttackSpeed] () Attack Speed for 4 seconds.

### Jhin (1-cost, ADCarry)

- Traits: Ionia, Gunslinger, Longshot | mana 0/70
- **Curtain Call**: For the next 4 attacks, set Attack Speed to 1, gain infinite range and replace attacks with a cannon shot. Cannon shots deal [TotalDamage] () physical damage. The 4th shot deals 144% more damage.

### Kog'Maw (1-cost, APCaster)

- Traits: Void, Arcanist, Longshot | mana 0/30
- **Caustic Spittle**: Deal [ModifiedDamage] () magic damage to target and reduce their Armor and Magic Resist by 8/10/15. Adjacent enemies are dealt 50% of the damage and effect.

### Lulu (1-cost, APCaster)

- Traits: Yordle, Arcanist | mana 10/55
- **Whimsy**: Deal [ModifiedDamage] () magic damage to target and make them dance for 1 seconds. The projectile then bounces to the nearest target, dealing [ModifiedSecondaryDamage] () magic damage.

### Qiyana (1-cost, ADFighter)

- Traits: Ixtal, Slayer | mana 0/30
- **Clear The Brush**: Dash to a nearby hex and swing forward, dealing [TotalDamage] () physical damage to enemies in a 2-hex line.

### Rumble (1-cost, APTank)

- Traits: Yordle, Defender | mana 40/110
- **Junkyard Titan**: Gain [ModifiedShield] () Shield for 4 seconds. Fire a burst of flames, dealing [TotalDamage] () magic damage in a cone.

### Shen (1-cost, APTank)

- Traits: Ionia, Bruiser | mana 30/90
- **Stand United**: Grant [ModifiedShield] () Shield to this unit and a nearby damaged ally for 4 seconds.

### Sona (1-cost, APCaster)

- Traits: Demacia, Invoker | mana 0/30
- **Power Chord**: Deal [ModifiedDamage] () magic damage to 2 nearby enemies and restore [ModifiedHeal] () Health to the lowest Health ally.

### Viego (1-cost, HFighter)

- Traits: Shadow Isles, Quickstriker | mana 0/30
- **Blade of the Ruined King**: Passive: Gain [ModifiedAttackSpeed]% () Attack Speed. Active: Deal [ModifiedDamage] () physical damage to target. For the rest of combat, Viego's attacks deal [ModifiedStackingDamage] () bonus stacking magic damage.

### Aphelios (2-cost, ADCarry)

- Traits: Targon | mana 0/40
- **Incendiary Onslaught**: Start with Infernum equipped. On cast, switch to Severum for [ModifiedNumAttacks] () attacks. Infernum: Attacks hit 3 enemies and apply 30% Sunder for 6 seconds. Severum: Gain 200% Attack Speed and 10% Omnivamp. Attacks are replaced with swipes that deal [ModifiedDamage] () physical damage. {{TFT_Keyword_Sunder}}

### Ashe (2-cost, ADCarry)

- Traits: Freljord, Quickstriker | mana 20/80
- **True Ice Arrow**: Fire an arrow at target that deals [TotalDamage] () physical damage to them and [ModifiedDamage] () damage to adjacent enemies. Targets below 30% max Health take true damage instead. 30% Chill all enemies hit for 3 seconds. Bloodsworn Bond: When you field Ashe and Tryndamere, your strongest copies permanently increase their power on kill.Bloodsworn Bond: Attacks fire a bonus arrow for every 10% Attack Damage granted to Tryndamere by Bloodsworn Bond. These arrows deal [SmallArrowDamageFinal] () physical damage. (Current Bonus Arrows: [TFTUnitProperty.:TFT16_Ashe_CurrentArrows]). {{TFT_Keyword_Chill}}

### Bard  (2-cost, APCaster)

- Traits: Caretaker | mana 0/55
- **Traveler's Call**: Launch 6 Meeps at nearby enemies that deal [ModifiedDamage] () magic damage. Add an additional Meep for each 3-star unit on your board. (Total Meeps: [TFT16_Bard_Num3Stars])

### Cho'Gath (2-cost, APTank)

- Traits: Void, Juggernaut | mana 40/100
- **Rupture**: Gain 10/15/25 permanent max Health (Total: [TFTUnitProperty.:TFT16_ChoGath_BonusHealth]). Restore [ModifiedHeal] () Health and rupture a 2 hex circle beneath target. After a brief delay, enemies within are knocked up for 1.5 seconds and take [TotalDamage] () magic damage.

### Ekko (2-cost, APFighter)

- Traits: Zaun, Disruptor | mana 0/40
- **Parallel Convergence**: Create a 1 hex zone around the target that lasts 3 seconds. The next 3 attacks deal [BonusMagicDamage] () bonus magic damage. When the zone ends, it deals [MagicDamage] () magic damage + 10% of the target's damage taken during the duration to all enemies in the zone.

### Graves  (2-cost, ADFighter)

- Traits: Bilgewater, Gunslinger | mana 0/50
- **Collateral Damage**: Passive: Attacks fire in a cone on target, dealing 45% damage to other enemies hit. Active: Fire an explosive shell at target, dealing [TotalDamage] () physical damage split among enemies in a 1 hex circle. Partners in Crime: When you field Graves and Twisted Fate, both gain bonuses.

### Neeko (2-cost, APTank)

- Traits: Ixtal, Arcanist, Defender | mana 15/65
- **Pop Blossom**: Leap at target and gain [ModifiedShield] () Shield for 4 seconds. Slam down after a brief delay, dealing [ModifiedDamage] () magic damage to enemies within two hexes and 20% Chilling them for 2 seconds. {{TFT_Keyword_Chill}}

### Orianna  (2-cost, APCaster)

- Traits: Piltover, Invoker | mana 20/50
- **Command: Protect**: Send the Ball to target, dealing [ModifiedDamage] () magic damage and splitting [ModifiedSecondaryDamage] () magic damage among enemies within a 1 hex circle. Then grant [ModifiedShield] () Shield to the two lowest Health allies for 3 seconds.

### Poppy  (2-cost, ADTank)

- Traits: Demacia, Yordle, Juggernaut | mana 40/80
- **Hammer Shock**: Gain [ModifiedShield] () Shield for 4 seconds and deal [ModifiedDamage] () physical damage to target. After a delay, briefly knock them up and deal the damage again. Bucklers fly out from the target, granting 2 nearby allies 30% of the Shield gained.

### Rek'Sai (2-cost, ADReaper)

- Traits: Void, Vanquisher | mana 0/30
- **Burrow**: Dash to the furthest enemy within 3 hexes and deal [TotalDamage] () physical damage to them. The next 3 attacks hit adjacent enemies and deal [ModifiedSecondaryDamage] () physical damage.

### Sion (2-cost, APTank)

- Traits: Noxus, Bruiser | mana 0/70
- **Soul Furnace**: Passive: Takedowns grant 18/20/50 permanent max Health. (Total: [TFTUnitProperty.:TFT16_Sion_BonusHealth]) Active: Gain [ModifiedShield] () Shield for 4 seconds. When it breaks, deal [ModifiedDamage] () magic damage to adjacent enemies.

### Teemo (2-cost, APCaster)

- Traits: Yordle, Longshot | mana 0/30
- **Toxic Dart**: Shoot the closest unpoisoned enemy, dealing [ModifiedDamage] () magic damage and poisoning them for 8 seconds. Poisoned enemies take [ModifiedDamagePerSecond] () magic damage each second.

### Tristana (2-cost, ADCaster)

- Traits: Yordle, Gunslinger | mana 0/50
- **Buster Shot						**: Fire a massive cannonball at target, dealing [TotalDamage] () physical damage. If they are within 2 hexes, knock them back 2 hexes and briefly Stun them.

### Tryndamere  (2-cost, ADFighter)

- Traits: Freljord, Slayer | mana 0/50
- **Undying Rage**: Become enraged for 5 seconds. While enraged, gain [ModifiedDurability] () Durability and attacks are replaced with a strike that deals [ModifiedDamage] () physical damage. Ignore 30% of Armor against Chilled enemies. Bloodsworn Bond: When you field Ashe and Tryndamere, your strongest copies permanently increase their power on kill.Bloodsworn Bond: Gain increased Move Speed. Gain 10% Attack Damage, permanently increased by 1% whenever Ashe or Tryndamere score a kill. (Current Bonus: [TFTUnitProperty.:TFT16_Tryndamere_PassiveAS]%) {{TFT_Keyword_Chill}}

### Twisted Fate (2-cost, APSpecialist)

- Traits: Bilgewater, Quickstriker | mana 0/3
- **Stacked Deck**: Passive: Attacks deal [ModifiedBonusAPDamage] () bonus magic damage. After every 3rd attack, throw 3 cards in a cone that deal [ModifiedDamage] () magic damage, reduced by 50% for each enemy they pass through. Partners in Crime: When you field Graves and Twisted Fate, both gain bonuses

### Vi (2-cost, ADFighter)

- Traits: Piltover, Zaun, Defender | mana 0/30
- **Relentless Force**: Gain [ModifiedShield] () Shield for 2 seconds. Slam on target, dealing [ModifiedDamage] () physical damage to enemies in a 1 hex range. Every third cast deals [ModifiedSecondaryDamage] () damage in a 2 hex range instead and knocks up enemies hit for 1 second.

### Xin Zhao (2-cost, ADTank)

- Traits: Demacia, Ionia, Warden | mana 15/70
- **Three Talon Strike**: Strike target 3 times. Each strike deals [ModifiedDamage] () physical damage and restores [ModifiedHeal] () Health. The final one Stuns for 1.5 seconds.

### Yasuo (2-cost, ADFighter)

- Traits: Ionia, Slayer | mana 0/30
- **Sweeping Blade**: Dash, then deal [TotalDamage] () physical damage to adjacent enemies. If only one enemy is hit, deal double damage. Blade Brothers: When you field Yasuo and Yone, Yasuo assists Yone's casts.Blade Brothers: When Yone casts, your strongest Yasuo blinks to every target Yone hits and deals 100% physical damage to each.

### Yorick  (2-cost, APTank)

- Traits: Shadow Isles, Warden | mana 15/75
- **Mourning Mist**: Restore [ModifiedHeal] () Health. Throw a globule of black mist at the enemy who has dealt the most damage this combat, preferring unique enemies. It 20% Chills them for 4 seconds and raises the Mana cost of their next cast by 20. It then deals [ModifiedDamage] () magic damage in 1 hex radius. {{TFT_Keyword_Chill}}

### Ahri (3-cost, APCaster)

- Traits: Ionia, Arcanist | mana 0/30
- **Fox-Fire**: Fire 3 flames at target, each dealing [ModifiedDamage] () magic damage. Every 3rd cast, fire 9 flames split between the target and up to 2 nearby enemies. If there are nearby enemies, dash away.

### Darius (3-cost, ADTank)

- Traits: Noxus, Defender | mana 20/65
- **Decimate**: Restore [TotalHealing] () Health and spin, dealing [ModifiedDamage] () physical damage to adjacent enemies and applying a stack of Hemorrhage that deals [ModifiedSecondaryDamage] () physical damage each second for 5 seconds. If an enemy with Hemorrhage drops below 8% Health, slam on them, instantly killing them. (Each stack increases the execute threshold by 5%)

### Dr. Mundo (3-cost, ADTank)

- Traits: Zaun, Bruiser | mana 0/60
- **Maximum Dosage**: Bulk up for 5 seconds. While bulked, restore [TotalHealing] () Health every second and attacks deal [TotalDamage] () physical damage.

### Draven (3-cost, ADCarry)

- Traits: Noxus, Quickstriker | mana 60/100
- **Spinning Axes**: Passive: If Draven has an empowered axe in hand, his next attack will throw it. On takedown, gain an Adoration stack, doubled if Draven or Darius scored the kill. Every 11 Adoration stacks grants 1/3/5 Gold. (Adoration Left: [TFTUnitProperty.:TFT16_DravenTakedownsRemaining], Gold Generated: [TFTUnitProperty.:TFT16_DravenGoldGenerated]g) Active: Spin an empowered axe that replaces his basic attack and deals [ModifiedDamage] () physical damage and returns to Draven after hitting an enemy.

### Gangplank (3-cost, ADFighter)

- Traits: Bilgewater, Slayer, Vanquisher | mana 0/50
- **Powder Kegs**: Toss barrels at the 3 nearest enemies and detonate them. They deal [TotalDamage] () damage, reduced by 25% for each barrel in the chain. Barrels reduce their target's Armor by 10, increased to 20 if the damage critically strikes.

### Gwen  (3-cost, APFighter)

- Traits: Shadow Isles, Disruptor | mana 0/30
- **Snip Snip!**: Passive: Gain 15% Omnivamp. Active: Dash around the target and snip [ModifiedCastSnipTimes] () times, dealing [ModifiedDamage] () magic damage to them and [ModifiedSecondaryDamage] () damage to all other enemies in a cone.

### Jinx (3-cost, ADSpecialist)

- Traits: Zaun, Gunslinger | mana 0/18
- **Switcheroo!**: Passive: After attacking 18/18/16 times, switch to Fishbones. While equipped, attacks are replaced with 3 rockets that target random enemies and deal [TotalDamage] () physical damage.

### Kennen  (3-cost, APTank)

- Traits: Ionia, Yordle, Defender | mana 70/125
- **Slicing Maelstrom**: Gain [ModifiedShield] () Shield and summon storm of lightning. 6 waves of bolts hit enemies within, dealing [ModifiedDamage] () magic damage over 3 seconds. The first 3 targets to be struck by 3 bolts are Stunned for 1.5 seconds. Multiple Stuns on the same enemy have 50% effectiveness.

### Kobuko & Yuumi (3-cost, APTank)

- Traits: Yordle, Bruiser, Invoker | mana 30/90
- **You & Me!**: Passive: Yuumi joins your strongest Kobuko. Every 3 seconds, she alternates between healing the lowest Health ally for [ModifiedHeal] () Health or granting 25/33/50% Attack Speed to the ally who has dealt the most damage for 4 seconds. Active: Dance, restoring [ModifiedSelfHeal] () Health over 3 seconds.

### LeBlanc  (3-cost, APCaster)

- Traits: Noxus, Invoker | mana 0/60
- **Distortions**: Summon 3 dashing mirror images that strike the target and up to [ModifiedNumTargets] other nearby enemies. The target takes [ModifiedDamage] () magic damage and is Stunned for 1 second, while other units hit take [ModifiedSecondaryDamage] () magic damage.

### Leona (3-cost, APTank)

- Traits: Targon | mana 70/120
- **Solar Flare**: Passive: Reduce instances of incoming damage by [ModifiedDamageReduction] (). Your strongest Leona grants your strongest Diana 3/4/5 Ability Power every second she survives. Active: Call down sun beams on the 3/3/4 nearest enemies, Stunning them for 1 second and dealing [ModifiedDamage] () magic damage. Wound and 1% Burn them for 4 seconds. {{TFT_Keyword_Wound}} {{TFT_Keyword_Burn}}

### Loris (3-cost, APTank)

- Traits: Piltover, Warden | mana 70/140
- **Piltover Tussle**: Gain [ModifiedShield] () Shield for 4 seconds. Charge and knock the target backwards, Stunning them and units hit for 1.25 seconds and dealing [ModifiedDamage] () magic damage to them. After charging, force nearby enemies to target this unit.

### Malzahar (3-cost, APCaster)

- Traits: Void, Disruptor | mana 0/35
- **Void Swarm**: Summon 2 Voidlings that each attack a nearby enemy 8 times. Each attack deals [ModifiedDamage] () magic damage.

### Milio (3-cost, APCaster)

- Traits: Ixtal, Invoker | mana 0/60
- **Ultra Mega Fire Kick!!!**: Launch a fuemigo at target that bounces 3 times, dealing [ModifiedDamage] () magic damage to enemies hit. The last bounce deals [ModifiedAOEDamage] () magic damage to enemies in a 1 hex radius.

### Nautilus (3-cost, APTank)

- Traits: Bilgewater, Juggernaut, Warden | mana 60/120
- **Titan's Wrath**: Passive: Every 4 seconds and on cast, the next attack deals damage in a 1 hex circle and deals [ModifiedDamage] () bonus magic damage. Active: Gain [ModifiedShield] () Shield for 4 seconds.

### Sejuani (3-cost, APTank)

- Traits: Freljord, Defender | mana 40/80
- **Winter's Wrath**: Gain [ModifiedShield] () Shield for 4 seconds. Strike in a cone and a line. Enemies hit take [ModifiedDamage] () magic damage and are 30% Chilled for 4 seconds. If they are already Chilled, Stun them for 1 second. {{TFT_Keyword_Chill}}

### Vayne (3-cost, ADCarry)

- Traits: Demacia, Longshot | mana 0/40
- **Tumble**: Tumble to a nearby hex, then fire a bolt at target that deals [TotalDamage] () true damage.

### Zoe (3-cost, APCaster)

- Traits: Targon | mana 30/60
- **Double Trouble Bubble**: Passive: Damaging an enemy applies 30% Shred for 6 seconds. Active: Kick 2 bubbles at nearby enemies, dealing [ModifiedDamage] () magic damage and 30% Chilling them for 2 seconds. {{TFT_Keyword_Shred}} {{TFT_Keyword_Chill}}

### Ambessa (4-cost, ADFighter)

- Traits: Noxus, Vanquisher | mana 0/20
- **Lacerate**: Swipe and dash around target, dealing [TotalDamage] () physical damage to enemies in the path. Then slam chains on target, dealing [ModifiedDamage] () physical damage in a line. If any enemies are killed, immediately recast this ability, dealing 80% damage. The slam deals 40/40/100% more damage to Tanks.

### Bel'Veth (4-cost, ADFighter)

- Traits: Void, Slayer | mana 0/100
- **Endless Banquet**: Passive: On takedown, gain 75/75/200% Attack Speed that decays over 3 seconds. Active: Deal [ModifiedDamage] () physical damage in a 2 hex radius and transform for the rest of combat. While transformed, gain [ModifiedAttackSpeed] () Attack Speed, 33% max Health, and increased Move Speed. Attacks deal 15/25/100 bonus true damage, increased by 3/5/30 for each time the same enemy is hit.

### Braum (4-cost, APTank)

- Traits: Freljord, Warden | mana 60/105
- **Unbreakable Frost**: Raise shield for 4 seconds, gaining [ModifiedDurability] () Durability and redirecting projectiles to towards Braum instead. When hit, deal [ModifiedDamage] () magic damage to nearby enemies and 30% Chill them for 2 seconds. {{TFT_Keyword_Chill}}

### Diana  (4-cost, APReaper)

- Traits: Targon | mana 0/30
- **Lunar Rush**: Passive: Ability damage from your strongest Leona and strongest Diana marks enemies as Moonlit, reducing their damage by 10%. Active: Gain [ModifiedShield] () Shield. Unleash a bolt of lunar energy at the farthest non-Moonlit enemy within 4 hexes, dealing [ModifiedDamage] () magic damage. Then, dash to all Moonlit enemies, dealing [ModifiedSecondaryDamage] () magic damage to them.

### Fizz (4-cost, APReaper)

- Traits: Bilgewater, Yordle | mana 0/10
- **Playful Trickster**: Become briefly untargetable, then jump to the farthest enemy within 4 hexes, avoiding Tanks and Fighters, and deal [ModifiedDamage] () magic damage to adjacent enemies. For the next 4 attacks, gain 100% Attack Speed and deal [ModifiedAttackDamage] () bonus magic damage on hit.

### Garen (4-cost, ADTank)

- Traits: Demacia, Defender | mana 50/130
- **Judgment**: Spin for 3 seconds, gaining 50/50/80% Durability and healing for [ModifiedHeal] () over the duration. Each second, deal [ModifiedDamage] () physical damage to adjacent enemies and reduce their Armor and Magic Resistance by 5.

### Kai'Sa  (4-cost, ADCasterFormSwapper)

- Traits: Assimilator, Void, Longshot | mana 0/10
- **Icathian Rain**: Dash away from all enemies, then fire 15/15/25 missiles split across the 4 nearest enemies. Each missile deals [TotalDamage] () physical damage.

### Kalista  (4-cost, ADCaster)

- Traits: Shadow Isles, Vanquisher | mana 20/65
- **Endless Rend**: Summon [TotalNumberOfSpears] () spears split among the 4 nearest enemies. After a brief delay, they each impale their target, dealing [TotalDamage] () physical damage and reducing Armor by 1.

### Lissandra (4-cost, APCaster)

- Traits: Freljord, Invoker | mana 20/65
- **Frozen Tomb**: Encase target in ice, Stunning them for 1 second and dealing [ModifiedDamage] () magic damage. All enemies within 2 hexes of them split [ModifiedSecondaryDamage] () magic damage. If the target dies, shards fly out and split excess damage between the 2 nearest enemies. Chilled enemies take 12% more damage from this ability. {{TFT_Keyword_Chill}}

### Lux (4-cost, APCaster)

- Traits: Demacia, Arcanist | mana 0/50
- **Final Spark**: Fire a ball of light at the most enemies, dealing [ModifiedDamage_Q] () magic damage to the first 2 enemies hit and Rooting them for 1 second. Then fire a beam, dealing [ModifiedDamage] () magic damage to enemies hit by the ball and [ModifiedSecondaryDamage] () magic damage to all other enemies hit.

### Miss Fortune (4-cost, ADCaster)

- Traits: Bilgewater, Gunslinger | mana 0/30
- **Heartbreaker**: Passive: Attacks ricochet to the highest current Health enemy, dealing 50/50/100% damage. Active: Fire waves of bullets that land on the 2/2/6 nearest enemies. The first wave deals [TotalDamage] () physical damage and the rest deal [ModifiedSecondaryDamage] () physical damage. Every 3rd cast, increase the number of waves by 1.

### Nasus  (4-cost, APTank)

- Traits: Shurima | mana 10/70
- **Fury of the Sands**: Steal [ModifiedHealthSteal] () Health split between the 4/4/8 nearest enemies. For 8 seconds, gain 30 Armor and Magic Resist and deal [ModifiedDamagePerSecond] () magic damage to adjacent enemies each second.

### Nidalee  (4-cost, APFighter)

- Traits: Ixtal, Huntress | mana 0/40
- **Relentless Pounce**: Jump on an adjacent enemy with the lowest health, then deal [ModifiedDamage] () magic damage to them and [ModifiedSecondaryDamage] () magic damage to adjacent enemies. If any die, cast again, dealing 70% damage. Tanks take 60% more damage from this ability and Fighters take 30% more.

### Renekton  (4-cost, ADFighter)

- Traits: Shurima | mana 0/30
- **Slice and Dice**: Dash to the lowest Health enemy within 2 hexes, dealing [ModifiedDashDamage] () physical damage to all enemies in the path. Then slash 4 times, each dealing [ModifiedSlashDamage] () physical damage. Subsquent casts increase the number of slashes by 1.

### Rift Herald  (4-cost, APTank)

- Traits: Void, Bruiser | mana 60/125
- **Warp Armor**: Gain 300/425/2000 max Health and grow larger. Slam in a 2 Hex circle on the target, dealing [ModifiedDamage] () magic damage to enemies hit. For 4 seconds, gain [ModifiedDurability] () Durability + 1% for each enemy targeting this unit.

### Seraphine (4-cost, APCaster)

- Traits: Piltover, Disruptor | mana 0/20
- **Grand Encore**: Gain 3 notes. For each one active, deal [ModifiedDamagePerCrystal] () magic damage to a nearby enemy. While holding 12 notes on cast, consume them to project a wave of energy. Allies hit restore [ModifiedBigCastHeal] () Health. Enemies hit take [ModifiedBigCastDamage] () magic damage, reduced by 30% for each enemy it's passed through.

### Singed (4-cost, APTank)

- Traits: Zaun, Juggernaut | mana 0/80
- **Poison Trail**: Passive: Run around the battlefield, leaving a poison trail that lingers for 4 seconds. Poison trails deal [ModifiedDamagePerSecond] () magic damage per second to enemies within. Gain [ModifiedManaPerSec] () Mana each second. Active: For 4 seconds, gain increased Move Speed, restore [ModifiedHealPerSecond] () Health each second, and gain 35/60/100 Armor and Magic Resistance.

### Skarner  (4-cost, ADTank)

- Traits: Ixtal | mana 60/120
- **Impale**: Passive: All allies gain 15/30/100 Armor. Active: Gain [ModifiedShield] () Shield for 4 seconds. Impale up to three nearby enemies in a line, dealing [ModifiedDamage] () physical damage and Stunning them for 1.5/1.75/8 seconds.

### Swain (4-cost, APTank)

- Traits: Noxus, Arcanist, Juggernaut | mana 40/155
- **Nevermove**: Passive: Each second, restore [TotalHealing] () Health and deal [ModifiedDamage] () magic damage to enemies within 1 Hex. Active: Open a demonic eye on the largest group of nearby enemies, dealing [ModifiedHandDamage] () magic damage and Stunning them for 1.5/1.75/8 seconds.

### Taric (4-cost, APTank)

- Traits: Targon | mana 0/80
- **Starlight's Touch**: Passive: All allies gain 15/30/100 Magic Resist. Once per combat after dropping below 35% max Health, restore 300/700/2500 health over 2 seconds. Active: Gain [ModifiedShield] () Shield and grant the 2 lowest Health allies [ModifiedAllyShield] () Shield for 4 seconds. The next 2/2/10 attacks deal [ModifiedDamage] () bonus magic damage.

### Veigar (4-cost, APCaster)

- Traits: Yordle, Arcanist | mana 0/60
- **Dark Storm**: Passive: Gain 50% more Ability Power from all sources. Active: Call down a storm of 12/12/24 meteors across the battlefield that deal [ModifiedDamage] () magic damage each. They can innately critically strike. For each one that kills an enemy, permanently gain 1 Ability Power. (Permanent Ability Power: [TFTUnitProperty.:TFT16_Unit_VeigarBonusAP]% )

### Warwick (4-cost, ADFighter)

- Traits: Zaun, Quickstriker | mana 0/70
- **Eternal Hunger**: Gain 100/100/400% Attack Speed, 15% Omnivamp, increased Move Speed, and deal [ModifiedDamage] () bonus physical damage on attack for the rest of combat. While this effect is active, takedowns grant Warwick, Jinx, and Vi [ModifiedTakedownAttackSpeed] () Attack Speed for 3 seconds.

### Wukong (4-cost, ADTank)

- Traits: Ionia, Bruiser | mana 30/60
- **Diverting Trickster**: Passive: On death, summon a Stone copy with [ModifiedCloneHealth] () Health and Wukong's items. Active: For 4 seconds, gain [ModifiedDefenses] () Armor and Magic Resist. Then spin, dealing [ModifiedDamage] () physical damage to all enemies within 2 hexes.

### Yone (4-cost, HFighter)

- Traits: Ionia, Slayer | mana 30/100
- **Kin of the Stained Blade**: Passive: Attacks alternate between dealing [ModifiedBasicAttackADDamage] () bonus physical damage and [ModifiedBasicAttackAPDamage] () bonus magic damage. Active: Strike through the most enemies possible within 3 hexes, knocking them up for 1.5 seconds and dealing [ModifiedPerTargetDamage] () physical damage and [ModifiedPerTargetDamage] () magic damage. Yone then splits [ModifiedStrikeDamage] () physical damage and [ModifiedStrikeDamage] () magic damage among all enemies hit. Blade Brothers: When you field Yasuo and Yone, Yasuo assists Yone's casts.Blade Brothers: When Yone casts, your strongest Yasuo blinks to each target, dealing bonus physical damage.

### Yunara (4-cost, ADCarry)

- Traits: Ionia, Quickstriker | mana 0/50
- **Transcendent State**: For the next 4 seconds, gain [ModifiedAttackSpeed] Attack Speed, infinite range, and replace attacks with lasers. Lasers deal [ModifiedDamage] () physical damage in a line, reduced by 75/75/30% for each enemy they pass through. When lasers critically strike, they deal 33% bonus true damage.

### Aatrox  (5-cost, ADFighter)

- Traits: Darkin, World Ender, Slayer | mana 0/20
- **The Darkin Blade**: On cast, cycle through one of the following abilities: Slash in a line, dealing [FirstCastModifiedDamage] () physical damage to enemies hit and 20% Sundering them for 4 seconds. Swipe in a cone, dealing [SecondCastModifiedDamage] () physical damage to enemies hit and knocking them up for 1 second. Slam on target, dealing [ThirdCastModifiedDamage] () physical damage in a 1 hex circle. Targets below 15% max Health are executed.

### Annie (5-cost, APCaster)

- Traits: Dark Child, Arcanist | mana 0/160
- **Enraged Inferno**: Erupt the entire battlefield in flames, permanently Burning and Wounding all enemies and dealing [ModifiedDamage] () magic damage to each of them over 45 seconds. Annie's max Mana is reduced by 140 for the rest of combat. Subsquent casts launch a fireball at target that deals [ModifiedSecondaryDamage] () magic damage. {{TFT_Keyword_Burn}} {{TFT_Keyword_Wound}}

### Aurelion Sol (5-cost, APCaster)

- Traits: Star Forger, Targon | mana 0/85
- **The Skies Descend**: Passive: For every 250 ability damage dealt to enemies in player combat, gain Stardust. Stardust makes The Skies Descend more powerful. (Current Stardust: [TFTUnitProperty.:TFT16_AurelionSol_Stardust]) Active: Crash a star on target, dealing [ModifiedDamage] () magic damage in a 2 hex radius. Stardust Upgrades: 25: Create a shockwave that deals [ModifiedThreshold1Damage] () additional magic damage in a 3 Hex radius.25: Create a shockwave that deals [ModifiedThreshold1Damage] () additional magic damage in a 3 Hex radius. 60: The star crash deals 15% more damage.60: The star crash deals 15% more damage. 140: Shockwave increases to a 4 hex radius.140: Shockwave increases to a 4 hex radius. 200: Knock up targets in the star crash for 1.5 seconds.200: Knock up targets in the star crash for 1.5 seconds. 300: Shockwave increases to 10 radius.300: Shockwave increases to 10 radius. 550: The star crash deals 33% bonus true damage.550: The star crash deals 33% bonus true damage. 800: Also call down a meteor storm that deals [ModifiedThreshold7Damage] () magic damage split between living enemies.800: Also call down a meteor storm that deals [ModifiedThreshold7Damage] () magic damage split between living enemies. 1444: Summon a black hole at the epicenter, instantly killing enemies within.1444: Summon a black hole at the epicenter, instantly killing enemies within.

### Azir (5-cost, APCarry)

- Traits: Shurima, Emperor, Disruptor | mana 20/40
- **Arise!**: Passive: After witnessing 3 attacks from Azir, Sand Soldiers deal [ModifiedDamage] () magic damage to their target. Active: Summon a Sand Soldier near target and command them to strike. If 2 Soldiers are already out, they all immediately deal [ModifiedSecondaryDamage] () magic damage instead.

### Baron Nashor (5-cost, ADFighter)

- Traits: Void, Riftscourge | mana 0/50
- **Wrath of the Void**: Passive: Immune to Crowd Control effects. Attacks deal damage in a line on target. If no enemies are in range, dash to the next one and knock up adjacent enemies on arrival. While in the Void Rift, erupt a tentacle below a random enemy every 3/3/0.25 seconds that knocks them up for 1.5 seconds. Active: Slam and erect spikes in a 2 hex circle, dealing [TotalDamage] () physical damage to enemies hit. Then over 3 seconds, spit 10 globs of acid that deal [ModifiedAcidDamage] () physical damage.

### Brock (5-cost, ADFighter)

- Traits: Ixtal | mana 40/100
- **Seismic Slam**: Passive: Immune to crowd control effects. Attacks briefly knock up the target and deal 50% damage in a 1 hex radius around them. Active: Slam the ground, dealing [ModifiedSlamDamage] () physical damage to ALL enemies, reduced by 15% for each hex from the center (down to 40%.) If this is the first time they've been hit by Seismic Slam this combat, knock them up for up to 1.75 seconds. 12/12/99 rocks fall around the battlefield, each dealing [ModifiedRockDamage] () physical damage to enemies hit.

### Fiddlesticks (5-cost, APReaper)

- Traits: Harvester, Vanquisher | mana 30/70
- **Crowstorm**: Teleport into a large cluster of enemies within 2 hexes and Stun them for 1.25/1.25/99 seconds. Begin losing 22 mana per second. While casting, deal [ModifiedDamagePerSecond] () magic damage per second to all enemies within a 2 hex radius. The closest 2 enemies take 33% more damage.

### Galio (5-cost, APTank)

- Traits: Demacia, Heroic | mana 90/140
- **Inspirational Might**: Passive: Every 3rd attack deals [BonusPassiveDamage] () magic damage in a circle around the target. Active: Dash into the largest group of units and inspire all allies within 2 hexes, granting them [ModifiedShield] () Shield for 4 seconds. Then crash down, dealing [ModifiedActiveDamage] () bonus magic damage to all enemies with 2 hexes.

### Kindred (5-cost, ADCarry)

- Traits: Eternal, Quickstriker | mana 80/200
- **Lamb's Respite**: Dash, then create a 2-hex radius zone for 2.5/2.5/99 seconds that shields allies within for [ModifiedShieldAmount] () health for the zone's duration. While active, double current Attack Speed and on attack, fire a secondary arrow at a nearby enemy that deals [ModifiedDamage] () physical damage. When the zone ends, heal all allies for [ModifiedHealPercentage] () of damage dealt while it was up and dash away.

### Lucian & Senna (5-cost, ADCaster)

- Traits: Soulbound, Gunslinger | mana 0/70
- **The Culling**: Fire [TotalNumShots] () shots towards target. Each shot explodes on the first enemy hit, dealing [TotalDamage] () physical damage to all nearby enemies.

### Mel  (5-cost, APCaster)

- Traits: Noxus, Disruptor | mana 30/200
- **Radiant Eclipse**: Passive: While gaining Mana, throw out 2/2/9 radiant orbs that orbit around target. They deal [ModifiedDamage] () magic damage in a 1 hex radius and drain 1 Mana from enemies hit each second. After draining 215/215/10 mana, gain a random Radiant Item. ([TFTUnitProperty.:TFT16_Mel_ManaSpent]/215/215/10) Active: Detonate orbs on target, causing a 3 hex radius explosion that deals [ModifiedTargetDamage] () magic damage to them and [ModifiedSecondaryDamage] () damage to all other enemies hit.

### Ornn (5-cost, APTank)

- Traits: Blacksmith, Warden, Freljord | mana 50/150
- **Call of the Forge**: Summon an elemental from behind the farthest enemy. It 20% Chills enemies hit for 5 seconds and deals [ModifiedDamage] () magic damage to them. When it arrives, headbutt it and redirect it toward another distant enemy, dealing [ModifiedSecondaryDamage] () magic damage instead. {{TFT_Keyword_Chill}}

### Ryze  (5-cost, APCaster)

- Traits: Rune Mage | mana 0/45
- **Runic Overload**: Release a runic blast, dealing [ModifiedDamage] () magic damage. It splits in two, dealing 33% of the damage to the closest two enemies. Each secondary blast splits an additional time. Bilgewater: Blasts explode on impact, dealing 18% of the damage as physical damage to adjacent enemies. Demacia: Execute targets below [ModifiedDemaciaExecuteThreshold] () Health. Freljord: Chill targets for 2 seconds and deal [ModifiedFreljordTrueDamage] () bonus true damage. Ionia: Blasts split an additional time, dealing 50% damage. Ixtal: Gain a random Region's effect on combat start and every 10 seconds. Noxus: Blasts now pierce targets, dealing 60% reduced damage per enemy hit. Piltover: Every third cast is Overcharged, dealing 40% more damage. Shadow Isles: Increase blast damage by [ModifiedShadowIslesBonusDamage] (). Shurima: Knock up enemies for 0.5 seconds, with a chance to dredge up Gold or Treasures. Targon: Heal the lowest Health ally for 10% of the damage dealt. Void: Ignore 30% of enemies' Armor and Magic Resistance. Yordle: Shrink targets hit, lowering their damage by 8% percent for 4 seconds. Zaun: Poison targets, dealing 15/30/500 magic damage [ModifiedZaunTicks] () times over 4 seconds.

### Sett (5-cost, APFighter)

- Traits: Ionia, The Boss | mana 10/90
- **The Show Stopper**: Grab target and slam them forward, dealing [ModifiedDamage] () + [ModifiedPercentOfTargetMaxHealth] () of their max Health as magic damage to them. All other enemies within 3 hexes take 50% of the total damage.

### Shyvana (5-cost, ADFighter)

- Traits: Dragonborn, Juggernaut | mana 0/30
- **Dragon's Descent / Flame Breath**: Dragon's Descent: Transform into a dragon, gaining 600/1000/9999 max Health. Briefly gain 20/20/90% Durability and dive-bomb into the largest group of enemies, dealing [ModifiedDivebombDamage] () physical damage in a 3 hex circle. Flame Breath: Breath fire in a cone for 3 seconds, dealing [ModifiedFireDamagePerSecond] () physical damage each second to enemies hit.

### Sylas  (5-cost, APFighter)

- Traits: Chainbreaker, Arcanist, Defender | mana 0/30
- **Stolen Might**: Cast one of three spells: Cataclysm: Leap on a nearby enemy. Deal [ModifiedJarvanDamage] () magic damage to all enemies within 2 hexes and Stun them for 1.5/1.75/30 seconds. Demacian Justice: Deal [ModifiedGarenDamage] () magic damage to target. If they are below 15% max Health, execute them. Final Spark: Gain [ModifiedShield] () Shield for 1.25 seconds. Deal [ModifiedLuxDamage] () magic damage to all enemies in a line.

### T-Hex (5-cost, ADFighter)

- Traits: HexMech, Piltover, Gunslinger | mana 10/80
- **Hextech Arsenal**: Passive: Attacks fire a barrage of 4 bullets. Active: Gain [ModifiedShield] () Shield and begin consuming 30 mana per second. Each second while casting, fire a laser that deals [ModifiedLaserDamagePerSecond] () physical damage, reduced by 70% per enemy hit up to 90%. Additionally, launch [ModifiedMissilesPerSecond] () missiles split between 4 nearby enemies that each deal [ModifiedMissileDamage] () physical damage.

### Tahm Kench (5-cost, APTank)

- Traits: Bilgewater, Glutton, Bruiser | mana 10/90
- **Devour**: Devour target for 2.5 seconds, dealing [ModifiedDamage] () magic damage and making them immune to other sources of damage. During this time, gain 30% Durability. If they are immune to crowd control, deal [ReducedModifiedDamage] () magic damage instead. If they die while devoured, spit out a random component they were holding or the unit's cost in gold. Otherwise, spit them towards the farthest enemy, briefly Stunning targets on impact.

### Thresh  (5-cost, APFighter)

- Traits: Shadow Isles, Warden | mana 0/40
- **The Box**: Passive: Each second, deal [ModifiedHealthDrain] () magic damage to all enemies within 2 hexes. Heal for 75% of the damage dealt by this effect. Active: Create a prison of walls in a 2 hex radius that lasts 5/5/60 seconds. While active, the Passive's damage is doubled. When an enemy touches a wall, they take [ModifiedDamage] () magic damage. If there are less than 2 enemies inside The Box at any time, drag an enemy in.

### Volibear  (5-cost, ADFighter)

- Traits: Freljord, Bruiser | mana 0/20
- **The Relentless Storm**: Bite the target and mark them, dealing [ModifiedBiteDamage] () physical damage, increased by 60% if they are already marked. After casting 5/5/2 times, leap to a nearby hex, activating Stormbringer and dealing [ModifiedSlamDamage] () physical damage to enemies within a two hex radius. Stormbringer: Gain 400/550/9999 Health and 50/50/999% Attack Speed. Every 2/2/0.3 seconds, summon a lightning bolt on a nearby enemy, dealing [ModifiedBoltDamage] () magic damage. Takedowns summon a bolt on fallen enemies.

### Xerath  (5-cost, APCaster)

- Traits: Shurima, Ascendant | mana 0/135
- **Eye of the Ascendant**: Launch 10/10/99 arcane blasts randomly split between the closest 4/4/10 enemies that each deal [ModifiedDamage] () magic damage.

### Zaahen (5-cost, ADFighter)

- Traits: Darkin, Immortal | mana 0/40
- **Divine Challenge**: Dash around target and slash in a circle, dealing [TotalDamage] () physical damage to target and [ModifiedDamage] () physical damage to all other adjacent enemies hit. If the target doesn't die, immediately cast again. If this Ability is cast 22 times in a row, instead slam on target, instantly killing them and dealing [ModifiedBigAOEDamage] () physical damage to all enemies within 3 hexes.

### Ziggs  (5-cost, APCarry)

- Traits: Zaun, Yordle, Longshot | mana 0/140
- **Mega Inferno Bomb**: Passive: Attacks are replaced with a bomb that bounces 1 time, dealing [ModifiedBasicAttackDamage] () magic damage to units hit. Active: Throw an enormous bomb at the largest 4-hex group of enemies that explodes on impact, dealing between [ModifiedMinDamage] () and [ModifiedMaxDamage] () magic damage to enemies hit based on their distance from the epicenter.

### Zilean (5-cost, APCaster)

- Traits: Chronokeeper, Invoker | mana 0/35
- **Visions of the End**: Throw a Time Bomb on the nearest enemy without one. Time Bombs deal [ModifiedDamage] () magic damage each second. When the target dies, they explode, dealing [ModifiedSecondaryDamage] () magic damage in a 1 hex radius and grant Zilean 15 mana. If a Time Bomb is on an enemy for 18/16/1 seconds and Zilean is alive, the enemy instantly dies.

### Tibbers (11-cost, APTank)

- Traits: Arcanist | mana 40/100
- **Rage of Embers**: Passive: After attacking an enemy, restore [ModifiedHeal] () Health. If they are Burning, the healing is doubled. Active: Gain 200/200/500% Attack Speed and [ModifiedShield] () Shield that decays over 4 seconds. Gain immunity to Crowd Control effects for the same duration. {{TFT_Keyword_Burn}}

## Monsters, summons, pets and other trait-less units (29)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Baby Brambleback (1-cost, )

- Traits: (none)
- **Searing Blast**: Passive: Attacks 1% Burn and 33% Wound enemies for 4 seconds. Burn: Deals a percent of the target's max Health as true damage every second. Wound: Reduces healing received.

### Baby Sentinel (1-cost, )

- Traits: (none)
- **Insight Blast**: Passive: Attacks deal an additional [ModifiedDamage] () magic damage.

### Brambleback (1-cost, )

- Traits: (none)
- **Bramble Smash**: Passive: Every 3rd attack deals [ModifiedSlamAttack] () physical damage.

### Dragon (1-cost, )

- Traits: (none)
- **Fiery Breath**: Passive: Attacks splash in a cone behind the target.

### Elder Dragon (1-cost, ADTank)

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Gromp (1-cost, )

- Traits: (none)
- **Gromp!**: Passive: Heal to full Health whenever an allied Gromp dies.

### Piltover Invention (1-cost, )

- Traits: (none)
- **Piltover Customs Special**: Each item equipped to the invention will determine what it does when it fires.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Sentinel (1-cost, )

- Traits: (none)
- **Regenerative Stone**: Passive: Every 4 seconds heal 400 Health.

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Void Grub (1-cost, )

- Traits: (none)
- **Void Slash**: Void Grubs lack the intelligence to do anything but attack.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Atakhan (11-cost, APTank)

- Traits: (none) | mana 40/85
- **Punish the Unworthy**: Passive: Gain 30% Omnivamp. Every 2 attacks, deal [ModifiedCleaveDamage] () magic damage to enemies within 1 hex.

### Bilgewater (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Demacia (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Emperor's Guard (11-cost, APTank)

- Traits: (none)
- **Stand Guard**: Does not move or attack. This unit dies when the Emperor dies.

### Frozen Tower (11-cost, APTank)

- Traits: (none)
- **Ancient Sentinel**: Built before anyone can remember, these frozen statues line the bridges above the Howling Abyss, eternally vigilant.

### GangplankBarrel (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### MalzaharVoidling (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### Sand Soldier (11-cost, APTank)

- Traits: (none) | mana 0/100
- **Stand Guard**: Does not move or attack. This unit dies when the Emperor dies.

### Senna (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Shyvana (11-cost, )

- Traits: (none) | mana 0/100
- **Placeholder Name**: Placeholder Tooltip

### Tempest Strike (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 


<a id="set-17"></a>

# Set 17: Space Gods

Source: current client data. 88 champions, 44 traits.

## Traits

### Anima

After losing a player combat, gain [TechPerCombat] Tech, plus additional Tech equal to [TechPerLoss] times the length of your loss streak. Additionally, gain [TechPerKill] Tech per Anima takedown. Each time Animas get [TechBreakpoint] Tech, they prototype new Anima Weapons. You can take them, or save your Tech to get more powerful weapons next time. Gain [PairsPartnerWinMultiplier]% Tech from losing if your partner wins a combat in Double Up.

  - (3-4) PairsPartnerWinMultiplier=0.85, TechBreakpoint=100, TechPerCombat=20, TechPerCombat_DU=20, TechPerDeath_PVE=1, TechPerKill=2, TechPerLoss=5, TechPerLoss_DU=5, TechPerRound_PVE=20
  - (5) PairsPartnerWinMultiplier=0.85, TechBreakpoint=100, TechPerCombat=20, TechPerCombat_DU=20, TechPerDeath_PVE=1, TechPerKill=2, TechPerLoss=5, TechPerLoss_DU=5, TechPerRound_PVE=20

### Arbiter

Scribe a unique divine law, allowing you to choose an effect to apply to Arbiters when a chosen cause occurs.

  - (2) DamageInstances=10, HealthThreshold=0.4, ManaToSpend=50, NumAttacks=3, NumSeconds=4, ShieldDuration=4, 1
  - (3) DamageInstances=10, HealthThreshold=0.4, ManaToSpend=50, NumAttacks=3, NumSeconds=4, ShieldDuration=4, 1

### Bastion

Your team gains [TeamwideResists] Armor and Magic Resist. Bastions gain more, and the value doubles in the first [Duration] seconds of combat.

  - (2-3) BonusArmor=16, BonusMR=16, Duration=10, EnhancedTeamwideArmor=20, StatMultiplier=2, TeamwideResists=15
  - (4-5) BonusArmor=40, BonusMR=40, Duration=10, EnhancedTeamwideArmor=20, StatMultiplier=2, TeamwideResists=15
  - (6) BonusArmor=60, BonusMR=60, Duration=10, EnhancedTeamwideArmor=20, StatMultiplier=2, TeamwideResists=15

### Brawler

Your team gains [TeamwideBonus]% Health. Brawlers gain more.

  - (2-3) HealthBonus=0.25, TeamwideBonus=0.05
  - (4-5) HealthBonus=0.45, TeamwideBonus=0.05
  - (6) HealthBonus=0.65, TeamwideBonus=0.05

### Bulwark

Summon a placeable relic. At the start of combat, it grants adjacent allies a [PercentHealthShield]% max Health shield and [AttackSpeed]% Attack Speed.

  - (1) AttackSpeed=0.1, AttackSpeedDuration=60, PercentHealthShield=0.1, ShieldDuration=60

### Challenger

Your team gains [TeamwideAS]% Attack Speed. Challengers gain bonus Attack Speed. When their target dies, Challengers dash to a new target and increase their Attack Speed bonus by [BurstPercent]% for [BurstDuration] seconds.

  - (2) AttackSpeedPercent=0.15, BurstDuration=2.5, BurstPercent=0.5, DamageReduction=None, TeamwideAS=0.1
  - (3) AttackSpeedPercent=0.28, BurstDuration=2.5, BurstPercent=0.5, DamageReduction=None, TeamwideAS=0.1
  - (4) AttackSpeedPercent=0.42, BurstDuration=2.5, BurstPercent=0.5, DamageReduction=None, TeamwideAS=0.1
  - (5) AttackSpeedPercent=0.55, BurstDuration=2.5, BurstPercent=0.5, DamageReduction=None, TeamwideAS=0.1

### Choose Trait

When you field Miss Fortune, choose between Conduit Mode, Challenger Mode, and Replicator Mode. Miss Fortune has a unique ability based on her mode and gains the associated trait.


### Commander

([MinUnits]) Sona gives you a random Command Mod every [RoundsPerMod] rounds which allows you to alter the way an ally behaves during combat. Command Mods last [RoundsPerMod] player combats even if they are not equipped.

  - (1) AS=0.15, HealthRatio=0.15, RoundsPerMod=2

### Conduit

Innate: Conduits gain [InnateManaGain]% additional Mana from all sources. Your team gains Mana Regen, increased for Conduits.

  - (2) ChannelerManaRegen=3, InnateManaGain=0.2, TeamManaRegen=1
  - (3) ChannelerManaRegen=5, InnateManaGain=0.2, TeamManaRegen=1
  - (4) ChannelerManaRegen=7, InnateManaGain=0.2, TeamManaRegen=2
  - (5) ChannelerManaRegen=9, InnateManaGain=0.2, TeamManaRegen=2

### Dark Lady

Your team gains [Durability]% Durability, increased to [TransformedDurability]% when Morgana is in her Dark Form.

  - (1) Durability=0.04, TransformedDurability=0.1

### Dark Star


  - (2-3) ADAP=45, ExecuteHPPercent=0.08, PercentHealth=0.25, SupermassivePercentBonus=0.7
  - (4-5) ADAP=45, ExecuteHPPercent=0.08, PercentHealth=0.25, SupermassivePercentBonus=0.7
  - (6-8) ADAP=45, ExecuteHPPercent=0.08, PercentHealth=0.25, SupermassivePercentBonus=0.7
  - (9) ADAP=45, ExecuteHPPercent=0.08, PercentHealth=0.25, SupermassivePercentBonus=0.7

### Divine Duelist

Your Tactician heals for [PlayerOmnivamp]% of player damage dealt from winning. Fiora always wins a one on one duel.

  - (1) DUHeal=1, PVEHP=1, PVENumWins=4, PlayerOmnivamp=0.15

### Doomer

Combat Start: Mark all enemies with Doom. The first time enemies are damaged each combat, their Doom is consumed, stealing [ADAP1]% Attack Damage and Ability Power from them and granting it to your strongest Vex.

  - (1) ADAP1=12

### Eradicator

Enemies have [PctResists]% less Armor and Magic Resist.

  - (1) PctResists=0.1

### Factory New

After participating in combat, open an armory to purchase a permanent upgrade for your strongest Graves. Every [NumberOfUpgradesBeforeRoundCostIncrease] upgrades, future upgrades will take an additional round. Next Upgrade: [TFTUnitProperty.trait:TFT17_GravesTrait_RoundsUntilUpgrade] Rounds.

  - (1) NumberOfUpgradesBeforeRoundCostIncrease=3

### Fateweaver

Innate: Fateweavers have Precision. {{TFT_Keyword_Precision}} Lucky: Check twice and take the better outcome.

  - (2-3) CritDamage=None
  - (4) CritChance=0.2, CritDamage=20

### Galaxy Hunter

Zed is obtained from the Invader Zed augment. While at least one clone is alive, Zed gains [BonusAD]% bonus Attack Damage.

  - (1) BonusAD=0.4

### God-Blessed

[TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey1] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey2] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey3] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey4] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey5] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey6] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey7] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey8] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey9] [TFTUnitProperty.trait:TFT17_CarouselMarket_TraitTRAKey10]

  - (None) (no numbers)

### Gun Goddess

When you field Miss Fortune, choose between Conduit Mode, Challenger Mode, and Replicator Mode. Miss Fortune has a unique ability based on her mode and gains the associated trait.

  - (1) Allies=2, DamageAmp=0.01, FlatDamage=100, Mana=2, NumAttacks=3

### Marauder

Your team gains [TeamwideBonus]% Omnivamp. Marauders gain more Omnivamp, Attack Damage, and their Omnivamp overhealing is converted into Shield (up to [MaxPercentHealthShield]% max Health.)

  - (2-3) AD=0.18, MaxPercentHealthShield=0.25, Omnivamp=0.05, ShieldAD=None, TeamwideBonus=0.05
  - (4-5) AD=0.35, MaxPercentHealthShield=0.25, Omnivamp=0.07, ShieldAD=None, TeamwideBonus=0.05
  - (6) AD=0.55, MaxPercentHealthShield=0.25, Omnivamp=0.1, ShieldAD=0.2, TeamwideBonus=0.05

### Mecha

Innate: Mecha units can transform into their Ultimate form, upgrading their ability and gaining [TransformedPercentHealth]% Health. Transformed Mechas take up two team slots and count twice for the Mecha trait. Use the Mecha-Former item to toggle the forms of your Mecha units

  - (3) AD=0.25, AP=25, TeamSize=1, TransformedPercentHealth=0.4
  - (4-5) AD=0.4, AP=40, TeamSize=1, TransformedPercentHealth=0.4
  - (6) AD=0.4, AP=40, TeamSize=1, TransformedPercentHealth=0.4

### Meeple

Meeple attract Meeps that empower Meeple abilities in meepy ways. They also gain bonus Health. Cloning time = Champion cost

  - (3-4) BonusHealth=100, Meeps=2
  - (5-6) BonusHealth=350, Meeps=3
  - (7-9) BonusHealth=450, Meeps=4
  - (10) BonusHealth=550, Meeps=6

### N.O.V.A.

Aatrox: Ally Damage [ShredAndSunder]% Shred and Sunders enemiesAatrox: Shred and Sunder enemies Caitlyn: Grant allies [AS]% Attack SpeedCaitlyn: Grant Attack Speed Akali: Allies gain PrecisionAkali: Allies gain Precision Maokai: Allies heal [Heal]% max HealthMaokai: Heal allies Kindred: Shield the strongest Tank for [ShieldValue]Kindred: Shield an ally Emblem: Allies deal [BonusTrueDamage]% stacking bonus true damageEmblem: Deal bonus true damage

  - (2-4) AS=0.2, BonusTrueDamage=0.1, Heal=0.12, ShieldValue=800, ShredAndSunder=0.3, TeamAttackDelay=6
  - (5) AS=0.2, BonusTrueDamage=0.1, Heal=0.12, ShieldValue=800, ShredAndSunder=0.3, TeamAttackDelay=6

### Oracle

Every [Rounds] rounds, Tahm Kench grants a reward! Rounds Remaining: [TFTUnitProperty.trait:TFT17_TahmKench_RoundsRemaining] Last Reward: [TFTUnitProperty.trait:TFT17_TahmKench_LastReward]

  - (1) rounds=3

### Party Animal

Once per combat, after falling below [HealthThreshold]% percent Health, become untargetable and repair [PercentHealthHeal]% max Health per second. Upon reaching full Health, or when no other allies remain, return to combat. If fully healed, for the rest of combat Blitzcrank is in {{TFT17_SpaceGroove_TheGroove}} and Party Crasher's passive fires bolts four times as fast.

  - (1) HealthThreshold=0.45, PercentHealthHeal=0.15

### Primordian

[DamageTakenPercentModifier]% of damage taken contributes to damage dealt.

  - (2) DamageMultiplier=1, DamageTakenPercentModifier=0.08, PercentMoreSwarmlings=45, 6
  - (3) DamageMultiplier=1.45, DamageTakenPercentModifier=0.08, PercentMoreSwarmlings=45, 6

### Psionic

Gain Psionic items that can be equipped to any ally.

  - (2-3) None
  - (4) 50

### Redeemer

Teamwide Attack Speed: [TFTUnitProperty.trait:TFT17_RhaastUnique_OffensiveStatToGain]% Teamwide Resists: [TFTUnitProperty.trait:TFT17_RhaastUnique_DefensiveStatToGain]

  - (1) BonusDefensiveStat1=2, BonusDefensiveStat2=2, BonusDefensiveStat3=2, BonusOffensiveStat1=0.02, BonusOffensiveStat2=0.02, BonusOffensiveStat3=0.02

### Replicator

Replicator abilities occur a second time at reduced effectiveness.

  - (2-3) Effectiveness=0.22
  - (4) Effectiveness=0.5

### Rogue

Rogues gain Attack Damage and Ability Power. The first time they fall below [HealthThreshold]% health, they slip into shadows. Enemies targeting them are redirected to a nearby unit, preferring Tanks.

  - (2) AD=0.12, AP=12, Duration=3, HealthThreshold=0.5
  - (3) AD=0.25, AP=25, Duration=3, HealthThreshold=0.5
  - (4) AD=0.45, AP=45, Duration=3, HealthThreshold=0.5
  - (5) AD=0.6, AP=60, Duration=3, HealthThreshold=0.5

### Shepherd

Shepherds summon the Bond of the Stars to aid them in battle. Bia and Bayin's power are increased by the total star level of all Shepherds.

  - (3-4) (no numbers)
  - (5-6) (no numbers)
  - (7) (no numbers)

### Sniper

Snipers gain Damage Amp, increased against targets farther away.

  - (2) PerHexIncrease=2, PercentDamageIncrease=18
  - (3) PerHexIncrease=3, PercentDamageIncrease=25
  - (4) PerHexIncrease=4, PercentDamageIncrease=35

### Space Groove

{{TFT17_SpaceGroove_TheGroove}}: [TFTUnitProperty.:TFT17_SpaceGroove_AS]% , [TFTUnitProperty.:TFT17_SpaceGroove_HealthRegen]%

  - (1-2) ADAPPerSecond=None, EffectBonus=None, StartOfCombatDuration=None
  - (3-4) ADAPPerSecond=None, EffectBonus=None, StartOfCombatDuration=4
  - (5-6) ADAPPerSecond=5, EffectBonus=None, StartOfCombatDuration=4
  - (7-9) ADAPPerSecond=5, EffectBonus=10, StartOfCombatDuration=4
  - (10) ADAPPerSecond=10, EffectBonus=500, StartOfCombatDuration=60, 800, 1500

### Stargazer

Stargazers chart a different constellation every game. This game: The Boar. Gain gold after winning player combat. Allies in empowered hexes gain [Wolf_Health_Teamwide]% Health, Attack Damage, and Ability Power. Stargazers gain more. (Gold Earned: [TFTUnitProperty.trait:TFT17_Stargazer_Wolf_TotalGold]) More hexes reveal at each player level

  - (3) Wolf_ADAP=10, Wolf_Gold=1, Wolf_Health=0.02, Wolf_Health_Teamwide=0.08, 8
  - (4) Wolf_ADAP=15, Wolf_Gold=2, Wolf_Health=0.08, Wolf_Health_Teamwide=0.08, 8
  - (5) Wolf_ADAP=25, Wolf_Gold=4, Wolf_Health=0.15, Wolf_Health_Teamwide=0.08, 8
  - (6) Wolf_ADAP=30, Wolf_Gold=6, Wolf_Health=0.22, Wolf_Health_Teamwide=0.08, 8

### Stargazer

Stargazers chart a different constellation every game. This game: The Medallion. (Current Bonus: [TFTUnitProperty.trait:TFT17_Stargazer_Medallion_Increase]% ) More hexes reveal at each player level

  - (3) Medallion_DA=15, Medallion_IncreasePer3Star=5, 10, 20, 0.05

### Stargazer

Stargazers chart a different constellation every game. This game: The Huntress. Combat Start: Mark the highest Health enemies. Allies in empowered hexes gain [Huntress_AS_Teamwide]% Attack Speed. Stargazers in empowered hexes gain more and heal for [Huntress_Heal]% of their max Health when a marked enemy dies. More hexes reveal at each player level

  - (3-4) Huntress_AS=0.15, Huntress_AS_Teamwide=0.15, Huntress_Heal=0.15, NumMarks=3
  - (5-6) Huntress_AS=0.45, Huntress_AS_Teamwide=0.15, Huntress_Heal=0.15, NumMarks=5
  - (7) Huntress_AS=0.7, Huntress_AS_Teamwide=0.15, Huntress_Heal=0.15, NumMarks=9

### Stargazer

Stargazers chart a different constellation every game. This game: The Serpent. Allies in empowered hexes gain [Serpent_DR_Teamwide]% Durability. Stargazers in empowered hexes gain more and poison enemies, repeating a portion of damage dealt as magic damage over [Serpent_Duration] seconds. More hexes reveal at each player level

  - (3-4) Serpent_DR=0.05, Serpent_DR_Teamwide=0.05, Serpent_Duration=3, Serpent_Poison=0.25
  - (5-6) Serpent_DR=0.1, Serpent_DR_Teamwide=0.05, Serpent_Duration=3, Serpent_Poison=0.45
  - (7) Serpent_DR=0.15, Serpent_DR_Teamwide=0.05, Serpent_Duration=3, Serpent_Poison=0.6

### Stargazer

Stargazers chart a different constellation every game. This game: The Altar. More hexes reveal at each player level

  - (3) Shield_AS_Teamwide=8, Shield_CashoutAS=18, Shield_CashoutHP=20, Shield_Health_Teamwide=8, Shield_NumDeaths=60

### Stargazer

Stargazers chart a different constellation every game. This game: The Fountain. Allies in empowered hexes heal [Fountain_HealthRegen_Teamwide]% max Health every [Fountain_Interval] seconds. Stargazers in empowered hexes heal an additional [Fountain_HealthRegen]% and gain stacking Attack Damage and Ability Power every [Fountain_Interval] seconds. More hexes reveal at each player level

  - (3-4) Fountain_HealthRegen=0.03, Fountain_HealthRegen_Teamwide=0.01, Fountain_Interval=2, Fountain_StackingADAP=4, MaxStacks=None
  - (5) Fountain_HealthRegen=0.03, Fountain_HealthRegen_Teamwide=0.01, Fountain_Interval=2, Fountain_StackingADAP=9, MaxStacks=None

### Stargazer

Stargazers chart a different constellation every game. This game: The Mountain Every [Mountain_RoundsPerEmblem] player combats, gain a Stargazer Emblem. Stargazers in empowered hexes gain various bonuses. (Combats Remaining: [TFTUnitProperty.trait:TFT17_Stargazer_Mountain_CombatsRemaining]) More hexes reveal at each player level

  - (3) Mountain_ADAP=None, Mountain_AS=None, Mountain_DR=None, Mountain_Health=0.15, Mountain_Resists=None, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, 25000, None
  - (4) Mountain_ADAP=0.15, Mountain_AS=None, Mountain_DR=None, Mountain_Health=0.15, Mountain_Resists=None, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, 25000, None
  - (5) Mountain_ADAP=0.15, Mountain_AS=None, Mountain_DR=None, Mountain_Health=0.15, Mountain_Resists=10, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, 25000, None
  - (6) Mountain_ADAP=0.15, Mountain_AS=0.08, Mountain_DR=None, Mountain_Health=0.15, Mountain_Resists=10, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, 25000, None
  - (7) Mountain_ADAP=0.15, Mountain_AS=0.08, Mountain_DR=0.05, Mountain_Health=0.15, Mountain_Resists=10, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, 25000, None
  - (8) Mountain_ADAP=0.15, Mountain_AS=0.08, Mountain_DR=0.05, Mountain_Health=0.15, Mountain_Resists=10, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=0.1, 25000, None
  - (9) Mountain_ADAP=0.15, Mountain_AS=0.08, Mountain_DR=0.05, Mountain_Health=0.15, Mountain_Resists=10, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=0.1, 25000, None
  - (10) Mountain_ADAP=0.15, Mountain_AS=0.08, Mountain_DR=0.05, Mountain_Health=0.15, Mountain_Resists=10, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=0.1, 25000, None
  - (11) Mountain_ADAP=0.15, Mountain_AS=0.08, Mountain_DR=0.05, Mountain_Health=0.15, Mountain_Resists=8, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=0.1, 25000, None

### Stargazer

Stargazers chart a different constellation every game. Stargazers in empowered hexes gain various bonuses, starting at ([MinUnits]) units. More hexes reveal at each player level.

  - (3-4) Family_BaseSharePercent=0.1, Family_HexesToReveal_TOOLTIPONLY=3, Family_Stats=10, Fountain_HealthRegen=0.04, Fountain_HealthRegen_Teamwide=0.02, Fountain_StackingADAP=2, Huntress_AS=0.3, Huntress_AS_Teamwide=0.15, Huntress_Heal=0.05, MaxStacks=None, Medallion_DA=0.2, Medallion_IncreasePer3Star=20, Mountain_Health=0.2, Mountain_RoundsPerEmblem=5, NumMarks=3, Serpent_DR=0.15, Serpent_DR_Teamwide=0.08, Serpent_Duration=3, Serpent_Poison=0.3, Shield_AS_Teamwide=8, Shield_CashoutAS=18, Shield_CashoutHP=20, Shield_Duration=10, Shield_Health_Teamwide=8, Shield_Interval=1, Shield_NumDeaths=60, Wolf_ADAP=10, Wolf_Gold=1, Wolf_Health=0.02, Wolf_Health_Teamwide=0.08, 0.15, 10, 20, 1, 1000, 0.25, 0.08, 0.5, 10, 1.3, 0.1, 0.05, 0.02, 0.15
  - (5-6) Family_BaseSharePercent=0.1, Family_HexesToReveal_TOOLTIPONLY=2, Family_Stats=35, Fountain_HealthRegen=0.04, Fountain_HealthRegen_Teamwide=0.02, Fountain_StackingADAP=4, Huntress_AS=0.45, Huntress_AS_Teamwide=0.15, Huntress_Heal=0.08, MaxStacks=None, Medallion_DA=0.3, Medallion_IncreasePer3Star=20, Mountain_ADAP=0.12, Mountain_Health=0.3, Mountain_RoundsPerEmblem=5, NumMarks=5, Serpent_DR=0.2, Serpent_DR_Teamwide=0.08, Serpent_Duration=3, Serpent_Poison=0.45, Shield_AS_Teamwide=8, Shield_Duration=10, Shield_Health_Teamwide=8, Shield_Interval=1, Shield_NumDeaths=60, Wolf_ADAP=15, Wolf_Gold=2, Wolf_Health=0.08, Wolf_Health_Teamwide=0.08, 0.25, 10, 30, 2, 1000, 0.35, 0.08, 0.5, 10, 1.5, 0.1, 0.05, 0.03, 0.15
  - (7) Family_BaseSharePercent=0.1, Family_EmpoweredSharePercent=0.3, Family_Stats=35, Fountain_HealthRegen=0.04, Fountain_HealthRegen_Teamwide=0.02, Fountain_StackingADAP=4, Huntress_AS=0.6, Huntress_AS_Teamwide=0.15, Huntress_Heal=0.1, MaxStacks=None, Medallion_DA=0.4, Medallion_IncreasePer3Star=20, Mountain_AS=None, Mountain_DR=None, Mountain_Health=0.45, Mountain_Resists=12, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, NumMarks=7, Serpent_DR=0.25, Serpent_DR_Teamwide=0.08, Serpent_Duration=3, Serpent_Poison=0.6, Shield_AS_Teamwide=8, Shield_Duration=10, Shield_Health_Teamwide=8, Shield_Interval=1, Shield_NumDeaths=60, Wolf_ADAP=25, Wolf_Gold=4, Wolf_Health=0.15, Wolf_Health_Teamwide=0.08, 0.35, 10, 40, 3, 0.01, 1000, 0.45, 0.08, 0.5, 10, 1.75, 0.1, 0.05, 2, 0.04, 0.15
  - (8) Family_BaseSharePercent=0.1, Fountain_HealthRegen=0.04, Fountain_HealthRegen_Teamwide=0.02, Huntress_AS_Teamwide=0.15, MaxStacks=None, Medallion_IncreasePer3Star=20, Mountain_ADAP=None, Mountain_AS=None, Mountain_DR=None, Mountain_Resists=None, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, NumMarks=3, Serpent_DR_Teamwide=0.08, Serpent_Duration=3, Shield_AS_Teamwide=8, Shield_CashoutAS=18, Shield_CashoutHP=20, Shield_Duration=10, Shield_Health_Teamwide=8, Shield_Interval=1, Shield_NumDeaths=60, Wolf_ADAP=10, Wolf_Gold=1, Wolf_Health=0.02, Wolf_Health_Teamwide=0.08, 10, 1000, 0.08, 0.5, 10, 0.1, 0.05, 0.15
  - (9) Family_BaseSharePercent=0.1, Fountain_HealthRegen=0.04, Fountain_HealthRegen_Teamwide=0.02, Huntress_AS_Teamwide=0.15, MaxStacks=None, Medallion_IncreasePer3Star=20, Mountain_ADAP=0.12, Mountain_AS=0.12, Mountain_DR=0.06, Mountain_Health=0.12, Mountain_Resists=12, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=None, Serpent_DR_Teamwide=0.08, Serpent_Duration=3, Shield_AS_Teamwide=8, Shield_Duration=10, Shield_Health_Teamwide=8, Shield_Interval=1, Shield_NumDeaths=60, Wolf_Health_Teamwide=0.08, 10, 1000, 0.08, 0.5, 10, 0.1, 0.05, 0.15
  - (10) Family_BaseSharePercent=0.1, Fountain_HealthRegen=0.04, Fountain_HealthRegen_Teamwide=0.02, Huntress_AS_Teamwide=0.15, MaxStacks=None, Medallion_IncreasePer3Star=20, Mountain_ADAP=0.12, Mountain_AS=0.12, Mountain_DR=0.06, Mountain_Health=0.12, Mountain_Resists=12, Mountain_RoundsPerEmblem=5, Mountain_StatIncrease=0.1, Serpent_DR_Teamwide=0.08, Serpent_Duration=3, Shield_AS_Teamwide=8, Shield_Duration=10, Shield_Health_Teamwide=8, Shield_Interval=1, Shield_NumDeaths=60, Wolf_Health_Teamwide=0.08, 10, 1000, 0.08, 0.5, 10, 0.1, 0.05, 0.15

### Timebreaker

Rerolls on Loss: [TFTUnitProperty.:TFT17_Timebreaker_NumRerollsTooltip] XP on Win: [TFTUnitProperty.:TFT17_Timebreaker_NumXPTooltip]

  - (2) AttackSpeed=0.12, TimebreakerAdditionalAS=None, None
  - (3) AttackSpeed=0.12, TimebreakerAdditionalAS=None, 1
  - (4) AttackSpeed=0.12, TimebreakerAdditionalAS=0.5, 1

### Vanguard

Vanguards gain [DamageReductionPct]% Durability while Shielded. Combat start and [HealthThreshold]% Health: Gain a max Health Shield for [ShieldDuration] seconds.

  - (2-3) DamageReductionPct=0.05, EnhancedDurability=0.08, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.16
  - (4-5) DamageReductionPct=0.05, EnhancedDurability=0.08, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.3
  - (6) DamageReductionPct=0.05, EnhancedDurability=0.08, HealthThreshold=0.5, ShieldDuration=10, ShieldPercentAmount=0.4

### Voyager

Combat Start: Your Tanks gain a Shield for [ShieldDuration] seconds. Your other allies gain Damage Amp. Voyagers gain double.

  - (2) BonusDA=0.09, ShieldDuration=15, ShieldHP=175, 2
  - (3) BonusDA=0.15, ShieldDuration=15, ShieldHP=250, 2
  - (4) BonusDA=0.18, ShieldDuration=15, ShieldHP=350, 2
  - (5) BonusDA=0.22, ShieldDuration=15, ShieldHP=500, 2
  - (6) BonusDA=0.27, ShieldDuration=15, ShieldHP=700, 2

## Champions (64)

### Aatrox (1-cost, ADTank)

- Traits: N.O.V.A., Bastion | mana 30/90
- **Stellar Slash**: Heal [ModifiedHeal] (), then deal [ModifiedDamage] () physical damage to the current target. N.O.V.A. Strike: Cleave the battlefield, briefly knocking up all enemies and dealing [ModifiedNovaDamage] () physical damage.

### Briar (1-cost, ADFighter)

- Traits: Anima, Primordian, Rogue | mana 0/40
- **Fish Frenzy**: Passive: For every 1% missing Health, gain [ModifiedAS]% () Attack Speed Active: Deal [ModifiedDamage] () physical damage to the target, increased by 50% if they're a tank.

### Caitlyn (1-cost, ADSpecialist)

- Traits: N.O.V.A., Fateweaver
- **Aim For The Head**: Passive: Attacks have a 15% Lucky chance to fire an empowered Headshot, dealing [ModifiedHeadshotDamage] () physical damage. N.O.V.A. Strike: Mark all enemies, increasing damage taken by 10%. The first time marked targets drop below 50% Health, Headshot them for [ModifiedNovaHeadshotDamage] () physical damage. Lucky: Check twice and take the better outcome.

### Cho'Gath (1-cost, APTank)

- Traits: Dark Star, Brawler | mana 30/70
- **Accretion**: Deal [TotalDamage] () magic damage to the lowest Health enemy in range and permanently gain 12/18/33 maximum Health. If this kills them, permanently gain 30/40/70 maximum Health instead. (Current Bonus: +[TFTUnitProperty.:TFT17_ChoGathBonusHealth] Health)

### Ezreal (1-cost, ADCaster)

- Traits: Timebreaker, Sniper | mana 0/30
- **Temporal Shot**: Active: Fire a blast at the current target that deals [TotalDamage] () physical damage. Every 7 takedowns, gain a drone that deals [ModifiedDroneDamage] () physical damage to the current target on cast. (Total Takedowns: [TFTUnitProperty.:TFT17_Ezreal_Takedowns])

### Leona (1-cost, APTank)

- Traits: Arbiter, Vanguard | mana 50/110
- **Shield of Daybreak**: Gain [ModifiedShield] () Shield for 4 seconds. Bash the current target, dealing [ModifiedDamage] () magic damage and stunning them for 1.75/1.75/2 seconds.

### Lissandra (1-cost, APCaster)

- Traits: Dark Star, Shepherd, Replicator | mana 0/30
- **Dark Matter**: Hurl a shard towards the current target, dealing [ModifiedDamage] () magic damage to the first target it hits. After hitting its initial target or at its final destination, the dagger explodes dealing [ModifiedSecondaryDamage] () magic damage to nearby targets.

### Nasus (1-cost, APTank)

- Traits: Space Groove, Vanguard | mana 60/120
- **Groovin' Susan**: Transform for 6 seconds, temporarily gaining 300/400/600 max Health, entering {{TFT17_SpaceGroove_TheGroove}}, and dealing [ModifiedDamage] () magic damage to adjacent enemies each second.

### Poppy (1-cost, APTank)

- Traits: Meeple, Bastion | mana 30/100
- **Huddle Up!**: Gain [ModifiedShield] () Shield for 4 seconds. For the duration, allies within two hexes gain [ModifiedResists] () Armor and Magic Resistance. Meep Bonus: Meeps grant [ModifiedMeepShield] () Shield to the nearest [ModifiedNumMeeps] (%i:set14AmpIcon%) allies for 4 seconds.

### Rek'Sai (1-cost, APTank)

- Traits: Primordian, Brawler | mana 40/100
- **Upheaval**: Heal [TotalHealing] (), then briefly knock up adjacent enemies and deal [ModifiedDamage] () magic damage.

### Talon (1-cost, ADReaper)

- Traits: Stargazer, Rogue | mana 0/30
- **Diviner's Judgment**: Stab the target, causing them to bleed for [ModifiedBleedDamage] () physical damage over 18 seconds. After the attack, leap to the highest percent Health enemy within 3 hexes.

### Teemo (1-cost, APCarry)

- Traits: Space Groove, Shepherd | mana 0/50
- **Double Time**: Passive: Attacks deal [ModifiedHitDamage] () bonus magic damage and an additional [ModifiedMagicDamage] () stacking magic damage over 6 seconds. While an enemy has 5 or more stacks, Teemo is in {{TFT17_SpaceGroove_TheGroove}}. Active: Gain 150% Attack Speed for 3 attacks.

### Twisted Fate (1-cost, APCaster)

- Traits: Stargazer, Fateweaver | mana 0/50
- **Fate's Gambit**: Draw a card with a value between 1 and 9 by Lucky chance, then throw it at the target. Based on the card drawn, deal between [ModifiedDamageMin] () and [ModifiedDamageMax] () magic damage. Overkill damage bounces to the nearest enemy. If a 9 is thrown, generate 1 gold. If a 1 is thrown, take 15% max health true damage. Lucky: Check twice and take the better outcome.

### Veigar (1-cost, APCaster)

- Traits: Meeple, Replicator | mana 10/50
- **Meepteor Shower**: Call down a Meepteor on the target that deals [ModifiedDamage] () magic damage. Meep Bonus: An additional [ModifiedMiniMeeps] (%i:set14AmpIcon%) mini Meepteor are called down on nearby targets dealing [ModifiedMiniDamage] () magic damage each.

### Akali (2-cost, ADFighter)

- Traits: N.O.V.A., Marauder | mana 0/30
- **Star Strike**: Reposition next to the target to strike the most enemies. Then throw 5 piercing kunai, each dealing [ModifiedDamage] () physical damage to the first enemy hit, reduced to [ModifiedSecondaryDamage] () for each subsequent target. Kunai remove 1 Armor, 2 if they crit. N.O.V.A. Strike: Slice all enemies, applying Wound and a bleed that deals [ModifiedNovaDamage] () physical damage each second. Kunai increase the damage of the bleed by 12%.

### Bel'Veth (2-cost, ADFighter)

- Traits: Primordian, Challenger, Marauder | mana 0/50
- **Tidal Slashes**: Unleash a flurry of [TotalNumSlashes] () slashes at the current target over 2 seconds, dealing [TotalDamage] () physical damage each.

### Gnar (2-cost, ADSpecialist)

- Traits: Meeple, Brawler, Sniper | mana 0/5
- **Slingshot Maneuver**: Passive: Every 5th attack, launch a returning boomerang at the target that travels two hexes past the first enemy it hits. It deals [ModifiedDamage] () physical damage, reduced by 75% per hit. Meep Bonus: [ModifiedNumMeeps] (%i:set14AmpIcon%) Meeps attack alongside Gnar, dealing [ModifiedMeepDPS] () physical damage per second.

### Gragas (2-cost, APTank)

- Traits: Psionic, Brawler | mana 30/80
- **Chemical Rage**: Heal [ModifiedHeal] () over 2 seconds. Then, deal [DamageTotal] () magic damage to target and enemies adjacent to them and 30% Chill them for 2 seconds. {{TFT_Keyword_Chill}}

### Gwen (2-cost, APReaper)

- Traits: Space Groove, Rogue | mana 0/30
- **Dance n' Dice**: Passive: Attacks deal magic damage. Gwen is in {{TFT17_SpaceGroove_TheGroove}} while targeting an enemy below 50% Health. Active: Dash to a nearby hex to snip the lowest percent Health enemy, dealing [ModifiedDamage] () magic damage to the target and [ModifiedAreaDamage] () magic damage to enemies in a cone. If this kills, dash and snip again at 65% damage!

### Jax (2-cost, APTank)

- Traits: Stargazer, Bastion | mana 20/80
- **Counter Star-ike**: Enter a defensive stance for 3 seconds, reducing incoming damage by [ModifiedFlatDR] and gaining [ModifiedShield] () Shield. When the stance ends, strike all nearby enemies, dealing [ModifiedDamage] () magic damage and Stunning them for 1/1.25/1.5 second(s).

### Jinx (2-cost, ADCarry)

- Traits: Anima, Challenger | mana 20/80
- **Explosive Attitude**: Fire a barrage of [ModifiedNumRockets] () rockets in a cone, each dealing [TotalDamage] () physical damage to the first target hit.

### Meepsie (2-cost, APFighter)

- Traits: Meeple, Shepherd, Voyager | mana 0/55
- **The Big Bang**: Passive: Attacks deal [TotalBonusDamage] (%i:set14AmpIcon%) bonus magic damage. Active: Leap to largest group of enemies within 2 hexes and slam the ground, dealing [DamageTotal] () magic damage in a three hex radius, reduced by 35% per hex from the epicenter. The closest 3 enemies are knocked up for 1/1/1.5 seconds. Meep Bonus: The passive's bonus damage is increased by 50% per Meep.

### Milio (2-cost, APCaster)

- Traits: Timebreaker, Fateweaver | mana 0/40
- **Mega Time Kick**: Kick a ball at the current target that deals [ModifiedDamage] () magic damage. On impact, the ball has a 100% Lucky chance to bounce to a new target dealing [ModifiedBounceDamage] () magic damage. These bounces can trigger additional bounces, but the odds halve with each bounce. Milio has a 40% chance to cast an additional time at an additional target. Bonus casts can trigger more bonus casts. Lucky: Check twice and take the better outcome.

### Mordekaiser (2-cost, APTank)

- Traits: Dark Star, Conduit, Vanguard | mana 40/100
- **Indestructible**: Gain [ModifiedInitialShield] () Shield. Each second for the next 64 seconds, gain [ModifiedShieldPerProc] () more Shield and deal [ModifiedDamagePerProc] () magic damage to adjacent enemies. When this ability ends, consume the remaining Shield and heal for 40% of its value.

### Pantheon (2-cost, ADTank)

- Traits: Timebreaker, Brawler, Replicator | mana 20/80
- **Advanced Defences**: Gain [ModifiedShield] () Shield and 15% Durability for 4 seconds. Over the duration, deal [ModifiedDamage] () physical damage each second to enemies in a cone.

### Pyke (2-cost, ADReaper)

- Traits: Psionic, Voyager | mana 0/40
- **Marked for Death**: Reposition up to one hex to throw a harpoon at the furthest enemy. The harpoon pulls the first enemy hit one hex forward and deals [ModifiedDamage] () physical damage. Then, teleport behind them and cleave, dealing [ModifiedTargetDamage] () physical damage to them and [ModifiedAreaDamage] () physical damage to nearby enemies.

### Zoe (2-cost, APCaster)

- Traits: Arbiter, Conduit | mana 0/50
- **Paddle Star**: Fire a paddle star at the current target, dealing [ModifiedDamage] () magic damage to the first target hit and [ModifiedSecondaryDamage] () to others it passes through. When the missile reaches its destination, redirect it to a distant enemy, increasing its speed and repeating the damage. This redirect can occur 54 times.

### Aurora (3-cost, APCaster)

- Traits: Anima, Voyager | mana 20/80
- **Hopped-Up Hacks**: Open a 2 hex rift containing the target, Hacking enemies within for 4 seconds and dealing [ModifiedDamage] () magic damage to each, plus [ModifiedSplitDamage] () magic damage split between all enemies hit. Hacked enemies store 10% of all damage they take. When the Hack ends, they take true damage equal to the stored damage. The Hack ends early if it would kill the target.

### Diana (3-cost, APFighter)

- Traits: Arbiter, Challenger | mana 0/40
- **Pale Cascade**: Passive: Attacks deal [ModifiedBonusDamageToAttacks] () bonus magic damage. Active: Gain [ModifiedShield] () Shield and summon 3 encircling orbs for 3 seconds. Orbs deal [ModifiedDamage] () magic damage to enemies they pass through, and rotate faster based on Attack Speed.

### Fizz (3-cost, APReaper)

- Traits: Meeple, Rogue | mana 0/20
- **Meep Bait**: Dash through the current target, dealing [ModifiedDamage] () magic damage. Every third cast also summons a Mega Meep after a delay, briefly knocking the target up and dealing [ModifiedChompDamage] (%i:set14AmpIcon%) magic damage. Adjacent enemies take 50% damage. Meep Bonus: Add [ModifiedNumMeeps] (%i:set14AmpIcon%) Meep to the bait, increasing Mega Meep damage by [ModifiedMeepBonusDamage] ().

### Illaoi (3-cost, APTank)

- Traits: Anima, Vanguard, Shepherd | mana 40/100
- **Test of Spirit**: Gain [ModifiedShield] () Shield for 3 seconds. Over the duration, drain [ModifiedHealthDrain] () Health from the nearest 3 enemies. Then slam down, dealing [ModifiedDamage] () magic damage to all enemies within 2 hexes.

### Kai'Sa (3-cost, ADCaster)

- Traits: Dark Star, Rogue | mana 0/50
- **Bullet Cluster**: Passive: On takedown, gain 10 mana. Active: Fire 16 missiles in a 2-hex radius around the current target, dealing [TotalDamage] () physical damage each.

### Lulu (3-cost, APCaster)

- Traits: Stargazer, Replicator | mana 0/55
- **It's Raining Stars**: Passive: Gain a different secondary effect each game based on the Stargazer constellation. Active: Call down something from the sky, dealing [ModifiedDamage] () magic damage to 3 nearby enemies and do some special effect based on this game's Stargazer Constellation.

### Maokai (3-cost, APTank)

- Traits: N.O.V.A., Brawler | mana 30/100
- **Grasp of Convergence**: Passive: Gain 50% more max Health from all sources. Active: Converge an X-shape of vines on the target, dealing [DamageTotal] () magic damage to each enemy hit and Stunning them for 1.5/1.5/1.75 seconds. N.O.V.A. Strike: Send forth a wave of dragons that stun all enemies for 1.5/1.5/1.75 seconds. For the rest of combat, Maokai's attacks deal [ModifiedNovaDamage] () bonus physical damage.

### Miss Fortune (3-cost, )

- Traits: Gun Goddess, Choose Trait | mana 0/100
- **Gun Goddess Arsenal**: Field Miss Fortune to choose whether she activates Conduit Mode, Challenger Mode, or Replicator Mode. The chosen mode determines her ability and her trait.

### Miss Fortune (3-cost, ADCaster)

- Traits: Conduit, Challenger, Replicator | mana 0/100
- **Gun Goddess Arsenal**: Field Miss Fortune to choose whether she activates Conduit Mode, Challenger Mode, or Replicator Mode. The chosen mode determines her ability and her trait.

### Ornn (3-cost, APTank)

- Traits: Space Groove, Bastion | mana 40/100
- **Disco Inferno**: Passive: On combat start, forge a temporary completed item. If 3 items are already equipped, a random completed item becomes Radiant this combat instead. Active: Gain [ModifiedShield] () Shield for 3 seconds, then breath fire in a cone dealing [ModifiedDamage] () magic damage. After any Shield on Ornn expires, Ornn enters {{TFT17_SpaceGroove_TheGroove}} for 2.5 seconds.

### Rhaast (3-cost, ADTank)

- Traits: Redeemer | mana 30/90
- **Divine Scythe**: Gain 20% Durability for 1 seconds, healing [ModifiedHeal] () over the duration. Afterwards, slash forward in a line, dealing [ModifiedDamage] () physical damage to enemies hit and knocking them up for 1 second.

### Samira (3-cost, ADCaster)

- Traits: Space Groove, Sniper | mana 0/65
- **Jump and Jive**: Passive: Whenever an enemy is knocked up, shoot them, dealing [ModifiedPassiveDamage] () physical damage and entering {{TFT17_SpaceGroove_TheGroove}} for 3 seconds. Active: Unleash a volley of bullets at the target, dealing [ModifiedDamage] () physical damage and knocking up for 0.75 seconds.

### Urgot (3-cost, ADFighter)

- Traits: Mecha, Brawler, Marauder | mana 0/50
- **Unstoppable Dreadnought**: Passive - Proximity Blast: Whenever an enemy enters a [ShotgunRange] () hex radius, fire a blast in a cone towards the closest adjacent hex that deals [ModifiedShotgunDamage] () physical damage with 30% falloff per hex. Each adjacent hex has a 5 second cooldown, and starts combat on cooldown. Active: Gain [ModifiedShield] () Shield for 3 seconds and reposition up to one hex to maximize the number of targets within Proximity Blast's radius. Reset Proximity Blast's cooldowns.

### Viktor (3-cost, APCaster)

- Traits: Psionic, Conduit | mana 20/80
- **Psionic Storm**: Channel a one-hex psionic storm that follows enemies for 54 seconds. Each second, it grows larger and deals [ModifiedDamage] () magic damage to enemies within, reduced by 60% per hex from the epicenter.

### Aurelion Sol (4-cost, APCaster)

- Traits: Mecha, Conduit | mana 15/75
- **Deathbeam**: Channel a deathbeam in a line towards the current target for 43 seconds. It deals [ModifiedDamage] () magic damage per second, reduced by 80% per enemy it passes through. Deathbeam ignores 30% of the enemy's Magic Resist.

### Corki (4-cost, ADCaster)

- Traits: Meeple, Fateweaver | mana 0/60
- **Asteroid Blaster**: Strafe to a nearby position, unleashing 21 missiles split between the target and all enemies within two hexes. Missiles deal [ModifiedDamage] () physical damage with a 20% Lucky chance of firing a mega missile that deals [ModifiedProcDamage] () instead. Meep Bonus: Every [ModifiedMeepCooldown] (%i:set14AmpIcon%) seconds, launch an Explosive Meep at the target, dealing [ModifiedMeepDamage] () physical damage in a one hex radius on impact. Lucky: Check twice and take the better outcome.

### Karma (4-cost, APCaster)

- Traits: Dark Star, Voyager | mana 10/55
- **Singularity**: Gather the force of a black hole, dealing [ModifiedDamage] () magic damage split between the target and the 2/2/4 closest enemies to them. The target takes an additional [ModifiedSecondaryDamage] () magic damage.

### Kindred (4-cost, ADCarry)

- Traits: N.O.V.A., Challenger | mana 0/40
- **Cosmic Pursuit**: Passive: Attacks and Abilities mark the target. When an enemy reaches 3 marks, Wolf consumes the marks, dealing [TotalDamage] () physical damage. Active: Jump up to 1 hex away and fire arrows at the nearest 3/3/5 targets, each dealing [ModifiedDamage] () physical damage. N.O.V.A. Strike: Gain 15% Damage Amp. Now and every 4.5 seconds after, add a mark to all enemies.

### LeBlanc (4-cost, APCarry)

- Traits: Arbiter, Shepherd | mana 0/40
- **Fracture Reality**: Passive: Attacks deal [ModifiedBaseAttackDamage] () magic damage instead. Active: Summon 5 clones that attack alongside for 5 attacks, dealing 25/25/150% damage. For their final attack, clones fire a bolt that deals [ModifiedBoltDamage] () magic damage.

### Master Yi (4-cost, ADFighter)

- Traits: Psionic, Marauder | mana 20/60
- **Psi Strikes**: Passive: Every third attack is a doubleslash that deals [ModifiedPassiveDamage] () bonus physical damage. Active: After brief meditation, enter a Psi-State for 5 seconds, gaining 10% Omnivamp, 70% Attack Speed, and increased movement speed. Twice a second, fire a psychic projection at a random nearby enemy that deals [ModifiedDamage] () physical damage

### Morgana (4-cost, APTank)

- Traits: Dark Lady, Conduit | mana 45/95
- **Dark Form**: Transform for 3 seconds, gaining [ModifiedHealthGain] () Health and tethering to nearby champions. Over the duration, deal [ModifiedDamage] () magic damage to the closest 3 enemies and restore [ModifiedHeal] () health to the closest 2 injured allies.

### Nami (4-cost, APCaster)

- Traits: Space Groove, Replicator | mana 20/65
- **Bubble Pop**: Toss a disco bubble at the target that deals [ModifiedDamage] () split between enemies in a one hex radius. The explosion sends 3 globs towards nearby enemies that deal [ModifiedFirstBounceDamage] () magic damage. On cast, Nami enters {{TFT17_SpaceGroove_TheGroove}} for 3 seconds.

### Nunu & Willump (4-cost, APTank)

- Traits: Stargazer, Vanguard | mana 40/145
- **Calamity**: Gain [ModifiedShield] () Shield for 4 seconds. Summon an astrolabe to crash down on a nearby hex, dealing [ModifiedInitialDamage] () magic damage to enemies within 2 hexes. Then, push the astrolabe towards the end of the board, dealing [ModifiedFollowupDamage] () magic damage. All enemies hit by the astrolabe are knocked up for 1.5/1.75/8 seconds.

### Rammus (4-cost, APTank)

- Traits: Meeple, Bastion | mana 20/80
- **Gravitational Spin**: Gain [ModifiedShield] () shield for 4 seconds. Then, strike enemies in a three hex line, dealing [ModifiedDamage] () magic damage. Meep Bonus: Reduce the damage of incoming attacks by [ModifiedFlatDR] (%i:set14AmpIcon%). After being attacked 20 times, deal [ModifiedPassiveDamage] (%i:set14AmpIcon%) magic damage in a two hex radius.

### Riven (4-cost, HFighter)

- Traits: Timebreaker, Rogue | mana 0/20
- **Time Warp**: Passive: This Ability adapts to Attack Damage or Ability Power, whichever is higher. Attacks deal [ModifiedPassiveAPDamage] () magic[ModifiedPassiveADDamage] () physical damage. Active: Dash to a nearby hex, gaining 100/150/1200 Shield for 2 seconds and slashing adjacent enemies for [ModifiedAPDamage] () magic[ModifiedADDamage] () physical damage. Every third cast, leap into the air and launch a wave of energy that deals [ModifiedAPWaveDamage] () magic[ModifiedADWaveDamage] () physical damage to enemies in a line.

### Tahm Kench (4-cost, APTank)

- Traits: Oracle, Brawler | mana 50/110
- **Tounge Lash**: Passive: Once per combat after dropping below 35% Health, gain Shield for 4 seconds equal to 40% of healing received this combat. Active: Heal [ModifiedHeal] (), then tongue lash all enemies within two hexes dealing [ModifiedDamage] () magic damage.

### The Mighty Mech (4-cost, ADTank)

- Traits: Mecha, Voyager | mana 40/100
- **Gravity Matrix**: Enter a defensive stance for 4 seconds gaining 20/20/60% Durability. While in the defensive stance, attract nearby enemy projectiles and heal [ModifiedHeal] () over the duration. When this ends, release a shockwave that deals [ModifiedDamage] () as physical damage in a 2-hex range.

### Xayah (4-cost, ADCarry)

- Traits: Stargazer, Sniper | mana 0/50
- **Stellar Ricochet**: Passive: Attacks bounce to strike 3/3/5 times, dealing 60/60/30% reduced damage per target hit and leaving a Feather behind the final target. Active: Gain 75% Attack Speed for the next 6 attacks. At the end, recall all Feathers split between the closest 3 enemies, dealing [TotalDamage] () physical damage each. Xayah's current target takes [ModifiedPrimaryTargetBonusDamage] () additional physical damage per feather.

### Bard (5-cost, APCaster)

- Traits: Meeple, Conduit | mana 0/65
- **Ultra Friendly Object**: Summon a flying saucer over the current target that lasts 54 seconds. Each second it deals [ModifiedDamagePerSecond] () magic damage to the target, plus [ModifiedSplitDamagePerSecond] () magic damage split between all enemies within 1 hex. The saucer deals 30% increased damage to Tanks. If an enemy under the UFO dies, Bard has a 15/20/100% chance to abduct them and create a 1-star copy on your bench. Meep bonus: On combat start, grant the nearest [ModifiedNumAllies] (%i:set14AmpIcon%) Meeple allies an additional Meep. Friends abducted: [TFTUnitProperty.:TFT17_Bard_FriendsAbducted]

### Blitzcrank (5-cost, APFighter)

- Traits: Party Animal, Space Groove, Vanguard | mana 30/120
- **Party Crasher**: Passive: Every 2/2/0.5 seconds, call down a bolt on the highest Health nearby enemy that deals [ModifiedBoltDamage] () magic damage. Active: Summon a disco ball at the largest clump of enemies, then knock up the current target into it, dealing [ModifiedUppercutDamage] () magic damage. They crash down into the disco ball, dealing [ModifiedExplosionDamage] () magic damage in a three hex radius. Enter {{TFT17_SpaceGroove_TheGroove}} for 1 second per enemy hit.

### Fiora (5-cost, ADFighter)

- Traits: Divine Duelist, Anima, Marauder | mana 0/70
- **Perfect Bladework**: Passive: Every 2 attacks, reveal a Vital on the target. If a Vital exists, dash to attack it dealing [ModifiedVitalDamage] () bonus true damage and heal for 15% of the damage dealt. Active: Reveal 6 Vitals on the target and quickly attack them all. If the target dies, remaining Vitals transfer to the nearest enemy. Upon striking the final Vital, create a two hex aura that heals allies within for [ModifiedHealing] () over 5 seconds.

### Graves (5-cost, ADCarry)

- Traits: Factory New | mana 0/60
- **Collateral Damage**: Passive: Attacks fire 5 projectiles in a cone that deal [ModifiedPassiveDamage] () physical damage each. Active: Fire an explosive shell that deals [ModifiedDamage] () physical damage to the target, and [ModifiedSecondaryDamage] () physical damage to adjacent enemies.

### Jhin (5-cost, ADCarry)

- Traits: Dark Star, Eradicator, Sniper | mana 0/44
- **Space Opera**: Passive: Jhin has a fixed attack speed of 0.9/0.9/1.4 and converts every 1% of bonus Attack Speed into 0.75 bonus Attack Damage. Active: Summon 4 spectral hands that fire alongside Jhin for the next 4 attacks. Each hand deals [TotalDamage] () physical damage per shot. The final shots deal 244% more damage and pierce through the most enemies in a line, dealing 44% reduced damage per target hit.

### Shen (5-cost, APFighter)

- Traits: Bulwark, Timebreaker, Bastion | mana 20/70
- **Reality Tear**: Passive: On cast, attacks gain [ModifiedBonusDamage] () stacking bonus magic damage. Starting from the third cast, gain true damage instead. Active: Gain [ModifiedShield] () Shield for 3 seconds, then slice open a rift on the largest group of champions dealing the Passive's bonus damage to all enemies within. Enemies have their Attack Speed slowed by 50/50/99% while allies within gain 80/80/999% Attack Speed, both rapidly decaying over 3 seconds. Shen always gains his rift's full bonus for this duration.

### Sona (5-cost, APCaster)

- Traits: Commander, Psionic, Shepherd | mana 0/25
- **Psionic Crush**: Hurl a chunk of magnetic debris at the nearest target without one, dealing [ModifiedDebrisDamage] () magic damage and sticking it to them. If an enemy with debris dies, it passes to the nearest enemy without one. Every 5 casts, instead rip off all debris dealing [ModifiedDebrisRipDamage] () magic damage, then crush all the debris onto the target, dealing [ModifiedSlamDamage] () magic damage and briefly stunning them.

### Vex (5-cost, APCarry)

- Traits: Doomer, Stargazer | mana 0/60
- **Lend Me a Hand, Shadow!**: Passive: Every time Vex attacks, Shadow strikes a nearby enemy, dealing [ModifiedShadowHandDamage] () magic damage. Whenever an enemy is struck 5 times by Shadow, Shadow strikes them again. Active: Shadow launches 3 empowered strikes, dealing [ModifiedShadowHandMagicDamage] () magic damage instead.

### Zed (5-cost, ADFighter)

- Traits: Galaxy Hunter | mana 40/100
- **Quantum Clone**: Create a clone behind the target with 25/35/40% reduced max Health and 30 increased Mana cost. The clone inherits its creator's items, stats, and current Health, and can cast Quantum Clone.

## Monsters, summons, pets and other trait-less units (24)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Mini Black Hole (1-cost, APTank)

- Traits: (none)
- **Gravitational Collapse**: A miniature black hole created by a supernova. Gains max health equal to 30% of ally Dark Star Health.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Swarmling Damage (1-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Apex Primordian (5-cost, ADTank)

- Traits: (none) | mana 30/70
- **Harbinger of The End**: Passive: Swords fly out in a grid dealing [TotalGridDamage] () physical damage to enemies they hit. Active: Launch 20 swords into the air and gain 25% Durability. When they land, they deal [TotalSwordAoEDamage] () physical damage to enemies they hit in a 1-hex range.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Bia & Bayin (11-cost, APTank)

- Traits: (none) | mana 0/40
- **Screech**: Passive: Attacks deal [ModifiedAADamage] () bonus magic damage. Active: Gain [ModifiedShield] () Shield for 3 seconds and grant allies 10% Ability Power for the rest of combat.

### Black Hole (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Cosmic Bruiser (11-cost, ADTank)

- Traits: (none)
- **Nothing To See Here!**: Cosmic Bruisers lack the intelligence to do anything but mindlessly attack. Careful, they pack a punch!

### Cosmic Elder Dragon (11-cost, ADTank)

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target. Watch out!

### Cosmic Flutterbye (11-cost, ADFighter)

- Traits: (none)
- **Nothing To See Here! **: Cosmic Flutterbyes lack the intelligence to do anything but mindlessly attack. A bit annoying, really.

### Cosmic Gromp (11-cost, ADTank)

- Traits: (none)
- **Nothing To See Here!**: Cosmic Gromps lack the intelligence to do anything but mindlessly attack. They're just protecting their...part of the cosmos?

### Cosmic Scrapper (11-cost, ADFighter)

- Traits: (none)
- **Nothing To See Here!**: Cosmic Scrappers lack the intelligence to do anything but mindlessly attack. They're doing their best though!

### Cosmic Squid (11-cost, ADFighter)

- Traits: (none)
- **Nothing To See Here!**: Cosmic Squids lack the intelligence to do anything but mindlessly attack. They're very squishy, though.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!

### N.O.V.A. Team Attack (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### Tempest Strike (11-cost, )

- Traits: (none) | mana 0/100
- **(no ability)**: 

### TimebreakerCore (11-cost, )

- Traits: (none) | mana 0/100
- **Temporal Core**: Sell Core to gain [TFTUnitProperty.:TFT17_Timebreaker_NumEXP] XP


<a id="set-18"></a>

# Set 18: Enchanted Wilds

Source: current client data. 91 champions, 36 traits.

## Traits

### Adaptor

Innate: Adaptor abilities change depending on if their Attack Damage or Ability Power is higher. Adaptors gain Ability Power or Attack Damage, depending on which is higher.

  - (2) 0.25
  - (3) 0.35
  - (4) 0.5

### Apex Predator

Elder Dragon takes up 2 team slots and grants +2 to the Riftbeast trait.

  - (1) (no numbers)

### Attuned

Your strongest Alune cycles to a new phase of the moon after each cast. While the moon is at or below half full, your team gains [Durability]% Durabilty. While above, your team gains [DamageAmp]% Damage Amp.

  - (1) DamageAmp=0.07, Durability=0.05

### Avatar

Having an Avatar on your bench or board transforms all other Avatars in your shop to the Trait of that Avatar. An Avatar's chosen Trait is counted twice for Trait bonuses.

  - (1) (no numbers)

### Blackthorn

The ally on the Blackthorn hex is sacrificed before combat, granting your champions Health. Blackthorn champions gain additional stats based on the sacrifice's Role, Star Level, and Cost.

  - (2-3) Health=175
  - (4-5) Health=350, StatMultiplier=0.3
  - (6) Health=600, StatMultiplier=0.5

### Blossom

After combat, your Wisps are empowered. Blossom champions gain Attack Damage, Ability Power, and [BonusHealth]% max Health.

  - (3-4) ADAP=0.12, BonusHealth=0.1
  - (5-6) ADAP=0.3, BonusHealth=0.1
  - (7-8) ADAP=0.5, BonusHealth=0.1, 4
  - (9-10) ADAP=0.55, BonusHealth=0.1
  - (11) ADAP=1, BonusHealth=0.1

### Bounty Seeker

Choose a bounty. Your strongest Draven can progress on the bounty to earn its reward. Upon completing a bounty, choose another.

  - (1) (no numbers)

### Brawler

Your team gains [BrawlerTeamHealth] max Health. Brawlers gain more.

  - (2-3) 120, 0.25
  - (4-5) 120, 0.4
  - (6) 120, 0.65

### Caustic

Caustic damage [ShredPercent]% Shreds and Sunders enemies for [ShredDuration] seconds. Shred: Reduce Magic Resist Sunder: Reduce Armor

  - (1) ShredDuration=4, ShredPercent=30

### Coven

Gather Essence by killing champions, plus more by losing player combat. Beginning at [EssenceForRitualTooltip] Essence, choose to convert Essence to rewards or continue gathering Essence. Each reward requires increasing amounts of Essence.

  - (3) 2, 40, 22
  - (4) 2, 40, 28
  - (5-6) 3, 40, 35
  - (7) 10, 40, 60

### Defender

Your team gains [NonDefenderDefenseGain] Armor and Magic Resist. Defenders gain more.

  - (2-3) 12, 25
  - (4-5) 12, 60
  - (6) 12, 115

### Eclipse

After [DelaySeconds] seconds of combat, the Eclipse kills the lowest Health enemy, repeating every [EclipseBeamPeriod] seconds.

  - (None) DelaySeconds=8, 3.5

### Elderwood

Gain placeable Elderwood plants.

  - (3-4) 0.25, 0.1
  - (5-6) 0.25, 0.1, 200
  - (7-8) 0.25, 0.1
  - (9-10) 0.25, 0.1
  - (11) 0.25, 0.1

### Emerald Aspect

Drag an ally onto your strongest Taric to pair them, granting the ally powerful bonuses from Taric's Ability.

  - (1) (no numbers)

### Executioner


  - (2) CritChance=0.15
  - (3) BleedDuration=3, 0.25
  - (4) 0.35

### Fae

Your team's damage, healing, and shielding attracts Pixies. Each Pixie grants Fae champions Attack Damage and Ability Power, and after they fall below [HealThreshold]% Health, they heal for each Pixie.

  - (2-3) ADAP=4.5, Heal=3, HealThreshold=0.5
  - (4) ADAP=7, Heal=5, HealThreshold=0.5

### Flora Fatalis

Flora Fatalis champions harvest enemies on takedown, gaining:

  - (1) Mana=10
  - (2) PercentHeal=0.07

### Greenfather

Gain [SeedsPerCast] seed when your strongest Ivern casts, plus [CombatStartSeeds] every combat. Ivern uses [SeedsToGrowHex] seeds to grow a hex on your board that grants a bonus to its occupant. Seeds: 0 / [SeedsToGrowHex] Current Biome:

  - (1) 1, 3, 5

### Hunter

Hunters gain Attack Damage. If a Hunter hasn't swapped targets for [HunterDuration] seconds, they gain [DamageAmp]% Damage Amp.

  - (2) DamageAmp=0.1, 0.2, 3
  - (3) DamageAmp=0.1, 0.3, 3
  - (4) DamageAmp=0.1, 0.45, 3
  - (5) DamageAmp=0.1, 0.6, 3

### Inferno

Inferno damage Burns and Wounds enemies for [Duration] seconds. Inferno Burns stack with other Burns.

  - (2) Duration=3, WoundPercent=33, 1
  - (3-4) Duration=3, 1
  - (5-6) Duration=3, 3, 2
  - (7) Duration=3, 4, 4

### Invoker

Your team gains Mana Regen, increased for Invokers.

  - (2) TeamManaRegen=1, 3
  - (3) TeamManaRegen=1, 4
  - (4) TeamManaRegen=2, 6
  - (5) TeamManaRegen=2, 8

### Juggernaut

Your team gains Durability. Juggernauts gain more.

  - (2-3) 0.2, 0.04
  - (4-5) 0.3, 0.06
  - (6) 0.42, 0.08

### Lunar

Lunar champions and adjacent allies gain Attack Speed and Ability Power. Lunar champions gain [LunarMultiplier]% more.

  - (2) AbilityPower=0.07, AttackSpeed=0.07, 1
  - (3) AbilityPower=0.12, AttackSpeed=0.1, 1
  - (4) AbilityPower=0.16, AttackSpeed=0.14, 1
  - (5) AbilityPower=0.2, AttackSpeed=0.18, 1

### Monolith

Monoliths gain [Resists] Armor and Magic Resist for each enemy targeting them.

  - (1) Resists=10

### Old Growth

Whenever an enemy within [UniqueTraitHexRange] hexes dies, your strongest Old Growth champion gains [UniqueTraitHealthPerStack] permanent max Health. Current: 0

  - (1) 30, 3

### Primal

Choose one of four Primal Blessings.

  - (2-3) (no numbers)
  - (4) (no numbers)

### Rapidfire

Your team gains [TeamAS]% Attack Speed. Rapidfire champions gain more on every attack, up to [MaxStacks] stacks.

  - (2) ASperAttack=0.03, MaxStacks=10, 0.1
  - (3) ASperAttack=0.05, MaxStacks=10, 0.1
  - (4) ASperAttack=0.07, MaxStacks=10, 0.1
  - (5) ASperAttack=0.1, MaxStacks=10, 0.1

### Ravager

Ravagers gain [Omnivamp]% Omnivamp. Additionally, Ravagers deal bonus damage, doubled against units below [EnemyHealthThreshold]% Health.

  - (2-3) Omnivamp=0.1, 0.12, 0.5
  - (4-5) Omnivamp=0.1, 0.25, 0.5
  - (6) Omnivamp=0.1, 0.4, 0.5

### Riftbeast


  - (3-4) (no numbers)
  - (5-6) 3
  - (7-9) 0.06, 50, 5, 0.06, 1, 0.06, 5
  - (10) TeamSize=2

### Rival


  - (1) (no numbers)
  - (1) 2, 30, 5, 3, 0.05, 8, 0.002
  - (2) (no numbers)

### Solar


  - (3) BonusMagicDamage=0.08, ShieldRatio=0.05, 3, 5, 8, 0.4, 0.15, 12, 0.01, 4

### Spellweaver

Your team gains [TeamwideAP]% Ability Power. Spellweavers gain more, plus extra Ability Power whenever a Spellweaver casts an Ability.

  - (2-3) APPerCast=0.01, TeamwideAP=0.1, 0.1
  - (4-5) APPerCast=0.01, TeamwideAP=0.1, 0.3
  - (6) APPerCast=0.02, TeamwideAP=0.1, 0.55

### Sprykin

Gain the Big Furry Friend. Drop a Sprykin on the BFF to pick its Rider.

  - (3-4) AttackSpeedIncrease=0.15, HealthIncrease=0.15
  - (5-6) AttackSpeedIncrease=0.35, HealthIncrease=0.4, 0.5
  - (7) AttackSpeedIncrease=0.55, HealthIncrease=0.55, 1

### Summoner

Summoners empower their summons in different ways. Yorick: +[HealthMult]% Health Azir: +[AzirDamageMult]% Damage Mama Beak: +[DamageMult]% Damage Zyra: +[NumExtraAttacks] Plant Attacks

  - (2) DamageMult=0.45, 4, 0.3, 0.2
  - (3) DamageMult=0.45, 4, 0.3, 0.2

### Thornmaiden

Your team gains [BaseDurability]% Durability, increased to [IncreasedDurability]% if [PlantNumThreshold] or more Zyra plants are alive.

  - (1) BaseDurability=0.05, 6, 0.1

### Vanguard

At Combat Start and after dropping below [HealthThreshold]% Health, Vanguards gain a max Health Shield for [ShieldDuration] seconds.

  - (2-3) HealthThreshold=0.5, MaxHealthShield=0.18, ShieldDuration=10
  - (4-5) HealthThreshold=0.5, MaxHealthShield=0.3, ShieldDuration=10
  - (6) HealthThreshold=0.5, MaxHealthShield=0.4, ShieldDuration=10, 0.05

## Champions (74)

### Akali (1-cost, )

- Traits: Inferno, Adaptor, Ravager | mana 0/25
- **Kunai Strike**: Adaptor : Throw a volley of kunai at the target, dealing [PhysicalDamageCalc1] physical damage. If the target is Burning, deal an additional [PhysicalDamageCalc2] damage. \n \nAdaptor : If the Ability kills, cast it again.

### Camille (1-cost, )

- Traits: Coven, Ravager | mana 0/25
- **Defensive Sweep**: Slice the target for [PhysicalDamageCalc1] physical damage and gain [AbilityShield] Shield for [ShieldDuration] seconds.

### Cinderling (1-cost, )

- Traits: Riftbeast, Hunter | mana 0/50
- **Razor Leaves**: Summon five razor-sharp leaves that converge on the current target, dealing a total of [PhysicalDamageCalc1] physical damage and applying [WoundLevel]% Wound and [BurnAmount]% Burn for [WoundDuration] seconds. \n \nScarlet Buff: Cinderling gains [TraitADOnCast] Attack Damage each cast. \n \nWound: Reduces healing received. \nBurn: Deals a percent of the target's max Health as true damage every second.

### Karma (1-cost, )

- Traits: Blossom, Spellweaver | mana 0/40
- **Karmic Bond**: Tether the current target, dealing [MagicDamageCalc1] magic damage over [TetherDuration] seconds. Then release a burst of power around them, dealing [MagicDamageCalc2] magic damage to all enemies in a [AoEHexRange] Hex radius and [SlowPercent]% Slowing them for [SlowDuration] seconds. \n \nSlow: Reduce Attack Speed

### Kobuko (1-cost, APTank)

- Traits: Sprykin, Brawler | mana 30/90
- **Dance of Life**: Restore [HealthCalc1] Health over 2 seconds. The next attack is replaced with a bash that deals [MagicDamageCalc1] magic damage.

### Leona (1-cost, )

- Traits: Solar, Defender | mana 30/90
- **Shield Bash**: Passive: Start combat with [GenericCalc1] bonus Armor and Magic Resist that decays over [DecayDuration] seconds. \n \nActive: Bash the current target, dealing [MagicDamageCalc1] magic damage and Stunning them for [StunDuration] seconds.

### Ornn (1-cost, )

- Traits: Elderwood, Defender | mana 40/100
- **Bellows Breath**: Active: Gain [ShieldCalc1] Shield for [ShieldDuration] seconds and deal [MagicDamageCalc1] magic damage to enemies in a cone. \n \nQuest: Each player combat, Ornn stores damage blocked as Forge Power, increased by [DamageTaken3StarScalar] at 3-star. Gain an Artifact Anvil each time he gains enough Forge Power. (Forge Power: [Stack] / [GenericCalc1])

### Pebbles (1-cost, )

- Traits: Riftbeast, Invoker | mana 30/70
- **Azure Laser**: Begin consuming [PercentManaPerSecond] max Mana per second and channeling a laser on the target. Each second while casting, deal [MagicDamageCalc1] magic damage to them and reduce their Magic Resist by [MRReduction]. \n \nTeal Buff: Gain [TraitManaRegenTooltip] Mana Regen for every [TraitChannelSecondsTooltip] seconds channeled.

### Rakan (1-cost, )

- Traits: Fae, Juggernaut, Vanguard | mana 35/105
- **Entrancing Dance**: Gain [ShieldCalc1] Shield for [ShieldDuration] seconds. Then grant the ally who has dealt the most damage this combat [AttackSpeedCalc1] decaying Attack Speed for [AttackSpeedDuration] seconds.

### Rek'Sai (1-cost, )

- Traits: Blackthorn, Brawler | mana 40/100
- **Uproot**: Passive: Restore [HealthCalc2] Health each second, tripled for [SpellMultiplierDuration] seconds after casting. \n \nActive: Lunge out of the ground, Stunning adjacent enemies for [KnockupDuration] second and dealing [MagicDamageCalc1] magic damage to them.

### Varus (1-cost, )

- Traits: Inferno, Rapidfire | mana 20/110
- **Piercing Arrow**: Wind up, then fire an arrow at the most enemies in line with the target. It deals [PhysicalDamageCalc1] physical damage to enemies hit, reduced by [DamageReductionPerHit] for each enemy it passes through (minimum [MinDamagePercent]).

### Veigar (1-cost, )

- Traits: Blackthorn, Sprykin, Spellweaver | mana 0/30
- **Primordial Burst**: Launch a giant blast at the target that deals [MagicDamageCalc1] magic damage, increased to [MagicDamageCalc2] if they're below [HPThreshold] max Health. \n \nIf they die, permanently gain [APOnKill]% Ability Power. \n \nCurrent bonus: [Stack]%

### Xayah (1-cost, )

- Traits: Elderwood, Fae, Rapidfire | mana 0/50
- **Deadly Plumage**: Gain [AttackSpeed] Attack Speed for the next [NumAttacks] attacks. These attacks are replaced with feathers that deal [PhysicalDamageCalc1] physical damage and reduce Armor by [GenericCalc1] .

### Yorick (1-cost, )

- Traits: Blossom, Juggernaut, Summoner | mana 50/110
- **Last Rites**: Passive: On death, spawn a Spirit Walker with [HealthCalc2] max Health which immediately taunts, forcing enemies to attack it. \n \nActive: Restore [HealthCalc1] Health and strike the target, dealing [PhysicalDamageCalc1] physical damage.

### Alistar (2-cost, )

- Traits: Elderwood, Brawler | mana 30/90
- **Triumphant Roar**: Roar, restoring [HealthCalc1] Health, cleansing disables, and healing the two lowest percent Health allies for [HealthCalc2] . Then slam the current target, dealing [MagicDamageCalc1] magic damage and Stunning them for [StunDuration] seconds.

### Caitlyn (2-cost, )

- Traits: Coven, Hunter | mana 0/3
- **Headshot**: Passive: Every third attack is replaced with a Headshot that deals [PhysicalDamageCalc1] physical damage.

### Elise (2-cost, )

- Traits: Coven, Vanguard | mana 20/70
- **Spider Queen**: Transform into a spider and gain [SpiderHealthBuff] max Health. Attacks while in Spider Form deal [MagicDamageCalc1] bonus magic damage and heal for [HealthCalc1] . Subsequent casts grant [DecayingAS] decaying Attack Speed for [ASBuffDuration] seconds.

### Gromp (2-cost, )

- Traits: Riftbeast, Adaptor | mana 0/45
- **Belchy Bubble**: Adaptor : Belch a noxious bubble at the current target that explodes on the first enemy hit, dealing [MagicDamageCalc1] magic damage. Enemies within a 1 hex radius of the explosion take [MagicDamageCalc2] magic damage over [PoisonDurationAP] seconds. \n \nAdaptor : Heavily slows the target. \n \nPurple Buff: Every [TraitTimer] seconds, Gromp gains [TraitTimedAP] Ability Power.

### Kayle (2-cost, )

- Traits: Solar, Rapidfire
- **Solar Judgement**: Passive: Kayle ascends based on her star level, granting her stacking bonuses. \n \n1st Ascension: Attacks deal [MagicDamageCalc1] bonus magic damage. \n2nd Ascension: Attacks [ShredLevel]% Shred enemies hit for [ShredDuration] seconds. \n3rd Ascension: Attacks fire waves, dealing [MagicDamageCalc2] magic damage to all other units hit. \n4th Ascension: Gain infinite range. Every [NumAttacksForBigWave4Star]rd wave is much larger. \n \nShred: Reduce Magic Resist

### LeBlanc (2-cost, )

- Traits: Elderwood, Spellweaver | mana 0/40
- **Mirror Image**: Passive: After player combat, your strongest LeBlanc has a [PercentChanceToPrint] chance to create a copy of an ally on your board, increased by [PercentChanceToPrintKillBonus] per takedown. \n \nActive: Launch a mirror image at the current target, dealing [MagicDamageCalc1] magic damage and [MagicDamageCalc2] to adjacent enemies.

### Murkwolf (2-cost, )

- Traits: Riftbeast, Ravager | mana 0/40
- **Rending Claws**: Leap to the lowest Health enemy within [LeapHexRadius] Hexes and deal [PhysicalDamageCalc1] physical damage. The next [NumEmpoweredAttacks] attacks gain [EmpowerAspd] Attack Speed and deal [PhysicalDamageCalc2] bonus physical damage. \n \nGrey Buff: Gain Precision and [TraitBaseCrit] Critical Strike Chance, increased up to [TraitBonusCrit] based on missing Health.

### Scuttlecrab (2-cost, )

- Traits: Riftbeast, Juggernaut | mana 40/100
- **Can You Dig It?**: Passive: Attacks are replaced by a dance that deals [PhysicalDamageCalc1] physical damage to all adjacent enemies. \n \nActive: Burrow underground, gaining [BurrowDurability] Durability for [BurrowDuration] seconds and healing [HealthCalc1] over the duration. \n \nGreen Buff: When allies fall below [TraitHealthThreshold] Health, they restore [TraitPercentHeal] max Health.

### Sejuani (2-cost, )

- Traits: Solar, Juggernaut | mana 40/100
- **Sun's Wrath**: Gain [ShieldCalc1] Shield for [ShieldDuration] seconds. Then cleave in a cone, dealing [MagicDamageCalc1] magic damage and strike in a line, dealing [MagicDamageCalc2] magic damage to enemies hit.

### Shen (2-cost, )

- Traits: Inferno, Defender | mana 30/90
- **Ki Barrier**: Grant [ShieldCalc1] Shield to Shen and [ShieldCalc2] Shield to a nearby damaged ally for [MagicShieldDuration] seconds. Both of their next [NumAttacksBuff] attacks gain [AttackSpeedBuff] Attack Speed and deal [MagicDamageCalc1] bonus magic damage.

### Teemo (2-cost, )

- Traits: Sprykin, Invoker | mana 0/50
- **Fungus Among Us**: Throw 2 clusters of mushrooms that deal [MagicDamageCalc1] magic damage to the [NumEnemiesHit] nearest enemies. Then hurl a giant mushroom that deals [MagicDamageCalc2] magic damage to the target. \n \nEach cast has a [PercentChanceOfForage] chance to forage an extra mushroom that can be collected for bonus effects. \n \nRed: [RedRerolls] Reroll (Reds Foraged: [Stack]) \nGreen: [GreenHealth] Tactician Health (Greens Foraged: [GenericCalc1]) \nYellow: [YellowXP] XP (Yellows Foraged: [GenericCalc2])

### Warwick (2-cost, )

- Traits: Blackthorn, Ravager | mana 0/40
- **Jaws of The Beast**: Bite the current target, dealing [PhysicalDamageCalc1] physical damage and healing for [GenericCalc1] of the damage dealt. Gain [AttackSpeedBuff] Attack Speed for the rest of combat.

### Yunara (2-cost, )

- Traits: Blossom, Executioner | mana 0/35
- **Cultivation of Spirit**: Dash, then launch an orb at the current target that deals [PhysicalDamageCalc1] physical damage and splits, dealing [PhysicalDamageCalc2] physical damage to [NumSecondaryTargets] nearby enemies.

### Azir (3-cost, )

- Traits: Blackthorn, Executioner, Summoner | mana 0/35
- **Arise!**: Gain [AttackSpeed] Attack Speed and summon [SoldiersToSpawn] soldiers for the next [NumAttacks] attacks. These attacks are replaced by commands which direct each soldier to deal [MagicDamageCalc1] magic damage per attack.

### Cassiopeia (3-cost, )

- Traits: Coven, Spellweaver | mana 0/30
- **Noxious Blast**: Poison the target and the nearest non-poisoned enemy, dealing [MagicDamageCalc1] magic damage over [Duration] seconds. Poisons stack.

### Diana (3-cost, )

- Traits: Lunar, Ravager, Vanguard | mana 0/40
- **Pale Barrier**: Gain [ShieldCalc1] shield for [ShieldDuration] seconds and send out [NumOrbs] moonlight orbs spread among enemies within 2 hexes, each dealing [MagicDamageCalc1] magic damage.

### Fiddlesticks (3-cost, )

- Traits: Flora Fatalis, Defender, Spellweaver | mana 30/90
- **Harvest**: Reduce the Magic Resist of the [NumTargets] nearest enemies by [MRReduction]. Then drain life from them over [CastDuration] seconds, healing for [HealthCalc1] Health and dealing [MagicDamageCalc1] magic damage to each over the duration.

### Hecarim (3-cost, )

- Traits: Elderwood, Vanguard | mana 50/110
- **Spirit of Dread**: Gain [Resists] Armor and Magic Resist for 3 seconds and restore [HealthCalc1] Health over the duration. Launch spectral riders at the [NumEnemies] nearest enemies, dealing [MagicDamageCalc1] magic damage and Stunning them for [StunDuration] seconds.

### Kha'Zix (3-cost, )

- Traits: Rival | mana 0/25
- **Taste Their Fear**: Leap to the farthest enemy within [LeapRangeHexes] Hexes, dealing [MagicDamageCalc1] magic damage. If they have no adjacent allies, deal [MagicDamageCalc2] magic damage instead and gain [IsolateManaGrant] mana.{Augment.Variant.RivalsAugment.AbilityTooltip}

### Kog'Maw (3-cost, )

- Traits: Caustic, Adaptor, Invoker | mana 15/55
- **Raining Artillery**: Adaptor : Launch acid at the target and the other nearest enemy, dealing [PhysicalDamageCalc1] physical damage. Enemies below [ADBonusDamageThreshold] max Health take [PhysicalDamageCalc2] physical damage instead. \n \nAdaptor: Damages enemies over time.

### Krug (3-cost, )

- Traits: Riftbeast, Brawler | mana 80/135
- **Rock and Roll**: Passive: On death, split into two Kruglettes with [HealthCalc1] Health which immediately taunt, forcing enemies to attack them. \n \nActive: Gain [HealthCalc2] max Health, then roll into the target, dealing [PhysicalDamageCalc1] damage. \n \nSlate Buff: On death, Krug and Kruglettes Shield allies for [TraitShieldHealth] max Health.

### Mama Beak (3-cost, )

- Traits: Riftbeast, Summoner, Rapidfire | mana 20/60
- **Flock Family**: Summon 4 untargetable Tiny Beaks nearby for [GenericCalc1] seconds. Whenever Mama Beak attacks, Tiny Beaks attack the same enemy, dealing [PhysicalDamageCalc1] physical damage. \n \nOrange Buff: Dealing physical damage reduces enemy Armor by [ArmorReduc].

### Master Yi (3-cost, )

- Traits: Blossom, Adaptor | mana 0/3
- **Wuju Style**: Passive: Every third attack is a Double Strike. On takedown, gain a burst of movement speed. \n \nAdaptor : Double Strikes grant [AttackSpeedCalc1] stacking Attack Speed. \n \nAdaptor : Double Strikes deal bonus damage and heal.

### Rammus (3-cost, )

- Traits: Sprykin, Defender | mana 30/80
- **Defensive Ball Curl**: Taunt, forcing enemies to attack this champion. For [Duration] seconds, gain [ShieldCalc1] Shield and [ArmorMR] Armor and Magic Resist. \n \nWhen the Shield breaks, deal [PhysicalDamageCalc1] physical damage to enemies in [DamageHexRange] Hexes.

### Rengar (3-cost, )

- Traits: Rival | mana 10/50
- **Savagery**: Jump to the lowest percent Health enemy within [JumpHexRange] hexes and stab them, dealing [PhysicalDamageCalc1] physical damage. Then heal for [HealthCalc1] , increased to up to [HealthCalc2] based on their missing Health.{Augment.Variant.RivalsAugment.AbilityTooltip}

### Tristana (3-cost, )

- Traits: Fae, Sprykin, Hunter | mana 0/50
- **Explosive Charge**: Attach an explosive charge to the target that lasts [Duration] seconds. While active, gain infinite range and [AttackSpeedCalc1] Attack Speed. After the duration, the charge explodes and splits damage between all enemies within 2 hexes, dealing [PhysicalDamageCalc1] physical damage + [PhysicalDamageCalc2] for each attack while casting. \n \nIf an enemy with an explosive charge on them dies, it attaches to Tristana's new target.

### Vi (3-cost, )

- Traits: Primal, Juggernaut | mana 0/60
- **Furious Fists**: Passive: On attack, restore [HealthCalc1] Health. \n \nActive: Unleash a primal roar, restoring [HealthCalc3] Health. Then gain [SpellAS] Attack Speed, [SpellDurability] Durability, and is unstoppable for [SpellDuration] seconds.

### Ahri (4-cost, )

- Traits: Blossom, Spellweaver | mana 20/100
- **Spirit Bomb**: Launch a spirit bomb at the location within [AbilityCenterHexRange] hexes that has the most surrounding enemies. It deals [MagicDamageCalc1] magic damage to enemies in a [HexRadius] hex radius, reduced by [HexPercentDamageFalloffTooltip] per hex away from the epicenter.

### Amumu (4-cost, )

- Traits: Inferno, Juggernaut | mana 30/125
- **Tantrum**: Passive: Every second, restore [HealthCalc1] Health and deal [MagicDamageCalc1] magic damage to enemies within [PassiveHexRadius] hex. \n \nActive: Deal [MagicDamageCalc3] magic damage to enemies within [BigHexRadius] hexes and Stun them for [StunDuration] seconds, increased to [GenericCalc1] seconds if the target is Burning. \n \nBurn: Deals a percent of the target's max Health as true damage every second.

### Aphelios (4-cost, )

- Traits: Lunar, Rapidfire | mana 20/70
- **Moonlight's Onslaught**: Equip Severum and swipe the target [GenericCalc1] times over [Duration] seconds, each dealing [PhysicalDamageCalc1] physical damage. \n \nAfter the onslaught finishes, fire a blast that deals [PhysicalDamageCalc2] physical damage split among all enemies in a 2 Hex radius.

### Brambleback (4-cost, )

- Traits: Riftbeast, Ravager | mana 0/40
- **Crimson Fury**: Passive: When the target dies, leap at the next target, dealing [PhysicalDamageCalc1] physical damage. \n \nActive: Gain [FrenzyADPercent] Attack Damage for [Duration] seconds. During this time ignore [GenericCalc1] Armor. \n \nRed Buff: Attacks [BurnAmount]% Burn enemies and heal Brambleback for [TraitMaxHealthHeal] max Health.

### Ezreal (4-cost, )

- Traits: Elderwood, Executioner | mana 0/30
- **Forest's Flurry**: Blink away from the current target, deal [PhysicalDamageCalc1] physical damage to them, and gain [AttackSpeedCalc1] Attack Speed. \n \nEvery 4th cast consumes the Attack Speed granted from Forest's Flurry and fires a blast through the largest group of enemies that deals [PhysicalDamageCalc2] physical damage, reduced by [DamageReductionPerHit] for each enemy it passes through (minimum [MinDamagePercent]).

### Lillia (4-cost, )

- Traits: Fae, Defender | mana 40/140
- **Lilting Lullaby**: Restore [HealthCalc1] Health and send [NumEnemiesToFireAt] butterflies at nearby enemies. Enemies hit take [MagicDamageCalc1] magic damage and Sleep for [SleepDuration] seconds. If they take [DamageNeededToAwaken] damage, they wake up and take an additional [WakeupDamage] max Health magic damage.

### Malphite (4-cost, )

- Traits: Blackthorn, Monolith | mana 30/80
- **Petrified Bark**: Gain [ShieldCalc1] Shield for [ShieldDuration] seconds and become petrified. When the shield breaks, unleash a wave of dark energy, dealing [MagicDamageCalc1] magic damage to enemies within [AoEHexRange] hexes.

### Morgana (4-cost, )

- Traits: Coven, Invoker | mana 0/65
- **Withering Curse**: Passive: Gain [Omnivamp] Omnivamp. \n \nActive: Fire a dark blast at [NumEnemiesCursed] nearby enemies, dealing [MagicDamageCalc1] magic damage to them and cursing them for [SpellDuration] seconds. Then, spawn a [HexRangeAOE] Hex withering zone for the same duration that deals [MagicDamageCalc2] magic damage per second. Cursed enemies take [MagicDamageCalc3] more damage per curse.

### Nidalee (4-cost, )

- Traits: Primal, Adaptor | mana 0/45
- **Javelin Toss**: Adaptor : Gain [BonusAttackSpeed] Attack Speed for the next [NumEmpoweredAttacks] attacks. These attacks are replaced with javelins that deal [MagicDamageCalc1] magic damage. The 3rd attack instead targets the furthest enemy with the least items and deals [MagicDamageCalc2] magic damage. \n \nAdaptor : Transform into a Melee cougar Assassin that swipes enemies.

### Sentinel (4-cost, )

- Traits: Riftbeast, Vanguard, Invoker | mana 60/150
- **Azure Shockwave**: Gain [ShieldCalc1] Shield for [ShieldDuration] seconds. Slam the ground, sending a fissure in the direction of the most enemies. Enemies hit are briefly knocked up, take [MagicDamageCalc1] magic damage, and are Mana Reaved for [ManaReaveFlat]. \n \nBlue Buff: Each time Sentinel casts, allies gain [TraitAlliesManaRegen] Mana Regen. \n \nMana Reave: Increase the Mana cost of the next Ability cast.

### Sett (4-cost, )

- Traits: Blossom, Brawler | mana 60/135
- **Haymaker**: Passive: Upon falling below [ManaHPThreshold] max Health the first time each combat, gain 100 mana. \n \nActive: Wind up a big punch, rapidly healing for [HealthCalc1] before dealing [PhysicalDamageCalc1] physical damage in a large cone.

### Sivir (4-cost, )

- Traits: Primal, Hunter | mana 0/40
- **Boomerang Blade**: Throw a large crossblade that deals [PhysicalDamageCalc1] physical damage to the current target and bounces [NumBounces] times between nearby enemies, dealing [PhysicalDamageCalc2] physical damage each bounce. When this kills an enemy, bounce [BonusKillBounces] additional times.

### Soraka (4-cost, )

- Traits: Flora Fatalis, Executioner | mana 0/30
- **Starcall**: Call down a star on the current target, dealing [MagicDamageCalc1] magic damage. If a star has previously fallen on them, call down [NumAdditionalStars] additional stars that each deal [MagicDamageCalc2] magic damage.

### Zyra (4-cost, )

- Traits: Thornmaiden, Summoner | mana 0/45
- **Rampant Growth**: Spawn [NumPlantsToSpawn] plants around the battlefield that attack the nearest enemy [ThornSpitterNumAttacks] times. Each attack deals [MagicDamageCalc1] magic damage.

### Alune (5-cost, APCaster)

- Traits: Attuned, Lunar, Spellweaver | mana 0/35
- **Moonfall**: Rain [NumMoonshards] moonshards split among the [NumEnemies] nearest enemies, dealing [MagicDamageCalc1] magic damage each. \n \nIf the moon is full, instead crash it onto the board, dealing [MagicDamageCalc2] magic damage split among all enemies.

### Ashe (5-cost, )

- Traits: Blossom, Hunter | mana 0/80
- **Spirit Rift**: Fire an arrow through the most enemies in a line that deals [PhysicalDamageCalc1] physical damage, reduced by [DamageFalloffPerEnemy] per enemy hit (minimum [MinDamagePercent]). If no enemies are hit refund [ManaRefund] of mana spent. \n \nThe arrow leaves a trail for [RiftDuration] seconds that deals [PhysicalDamageCalc2] + [MaxHealthDamagePerSecond] max Health physical damage per second to enemies within and [ChillPercent]% Slows them. \n \nSlow: Reduce Attack Speed

### Draven (5-cost, )

- Traits: Bounty Seeker | mana 0/110
- **Whirling Death**: Passive: Attacks target random enemies in range and apply a bleed that deals [PhysicalDamageCalc1] physical damage over [BleedDuration] seconds. Every attack has a [GenericCalc1] chance to deal [SpinningAxeDamageRatio] bonus physical damage and apply [SpinningAxeBleedStacks] bleeds instead. \n \nActive: Throw two giant axes towards the enemy with the most bleeds. Deal [PhysicalDamageCalc3] physical damage to enemies hit and consume their bleed, instantly dealing the remaining damage. The axes then return, dealing [GenericCalc2] physical damage to enemies hit. If no enemies are hit refund [ManaRefund] of mana spent.

### Elder Dragon (5-cost, )

- Traits: Apex Predator, Riftbeast | mana 20/60
- **Heat Without Equal**: Passive: Attacks deal [AttackAoERatio] damage to enemies adjacent to the target. \n \nActive: Become invulnerable and fly into the air. On return, stun all enemies for [StunDuration] seconds, gain [Omnivamp] Omnivamp, and Ignite them for [IgniteDuration] seconds. Ignited enemies take [IgniteMaxHealthDamage] max Health physical damage per second. Replace this ability with Flame Breath and immediately cast it. \n \nFlame Breath: Spew fire at the most enemies in a line through the target, dealing [PhysicalDamageCalc2] physical damage, reduced by [DamageReductionPerHit] for each enemy it passes through (minimum [MinimumDamageThreshold].) Ignite those enemies for [IgniteDuration] seconds. \n \nElder Dragon Buff: Damaging an enemy below [TraitExecuteThreshold] Health executes them.

### Gnar (5-cost, )

- Traits: Elderwood, Sprykin, Brawler | mana 0/70
- **Rage Gene**: Passive: Gain [RagePerSecond] Rage per second and [RagePerAttack] Rage per attack. \n \nActive: Transform into Mega Gnar and leap into the largest group of enemies within [TransformSearchRadius] hexes. Then, deal [PhysicalDamageCalc3] physical damage to enemies within [TransformHexRadius] hexes, reduce their Armor and Magic Resist by [GenericCalc1] , and Stun them for [StunDuration] second. Mega Gnar gains [HealthCalc1] Health, and replaces his Ability with Grab n' Throw. \n \nGrab n' Throw: Throw the target at the farthest enemy, dealing [PhysicalDamageCalc1] physical damage to them and [PhysicalDamageCalc2] physical damage to enemies they pass through. If there's only 1 enemy left, throw them off the battlefield instead.

### Ivern (5-cost, )

- Traits: Greenfather | mana 50/80
- **Triggerseed**: Passive: Shields from this Ability can critically strike with Precision. \n \nActive: Grant [NumAlliesToShield] allies [ShieldCalc1] Shield and [DamageAmpAmount] Damage Amp for [ShieldDuration] seconds. Then deal [MagicDamageCalc1] magic damage to enemies adjacent to them. After casting [CastThreshold] times, also grant [AttackSpeed] stacking Attack Speed to targets.

### Kennen (5-cost, )

- Traits: Inferno, Executioner | mana 0/40
- **Firestorm**: Charge up, gaining [APPerBurningEnemy] Ability Power per Burning enemy. Then, gain [ShieldCalc1] Shield for [ShieldDuration] seconds and rush through a nearby group of enemies, dealing [MagicDamageCalc1] magic damage to each. \n \nAfter rushing, unleash a [FirestormHexRadius] Hex firestorm that deals a total of [MagicDamageCalc2] magic damage split among all enemies hit over [FirestormDuration] seconds.

### Lux (5-cost, )

- Traits: Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the largest group of enemies that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit.

### Lux (Blackthorn) (5-cost, )

- Traits: Blackthorn, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nBlackthorn Bonus: Stuns targets hit for [BlackthornStunDuration] second.

### Lux (Blossom) (5-cost, )

- Traits: Blossom, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nBlossom Bonus: The first target hit is dealt [BlossomBonusDamageRatio] bonus damage.

### Lux (Coven) (5-cost, )

- Traits: Coven, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nCoven Bonus: Reduces the Armor and Magic Resist of targets hit by [CovenResistReduction] for the rest of combat.

### Lux (Elderwood) (5-cost, )

- Traits: Elderwood, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nElderwood Bonus: Casts grant all allies [ElderwoodHealthIncrease] increased max Health for the rest of combat.

### Lux (Fae) (5-cost, )

- Traits: Fae, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nFae Bonus: Heals the lowest percent Health ally for [FaeHealRatio] of ability damage dealt.

### Lux (Inferno) (5-cost, )

- Traits: Inferno, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nInferno Bonus: Restores [InfernoManaRestore] mana per takedown.

### Lux (Lunar) (5-cost, )

- Traits: Lunar, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nLunar Bonus: Applies [LunarVulnerablePercent]% Vulnerable to enemies hit for [LunarVulnerableDuration] seconds. \n \nVulnerable: Increases damage taken

### Lux (Primal) (5-cost, )

- Traits: Primal, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nPrimal Bonus: Gain [PrimalAttackSpeed] Attack Speed for [PrimalASDuration] seconds after cast.

### Lux (Solar) (5-cost, )

- Traits: Solar, Avatar | mana 20/70
- **Final Spark**: Passive: On cast, all allies that share a trait with Lux gain [ManaCalc1] mana. \n \nActive: Fire a laser towards the most enemies in a line that deals [MagicDamageCalc1] magic damage, reduced by [DamageReductionPerUnit] for each enemy hit (minimum [MinimumFalloffDamageRatio]). \n \nSolar Bonus: Deal [SolarBonusDamageRatio] bonus damage. For each unique 3-star champion fielded, deal an additional [SolarBonusDamagePer3StarRadio] bonus damage.

### Maokai (5-cost, )

- Traits: Old Growth, Juggernaut | mana 30/100
- **Sow the Seeds**: Passive: After every [DamageMitigatedPerSapling] damage blocked, a sapling jumps towards a nearby enemy and deals [MagicDamageCalc1] magic damage. On death, [SaplingsOnDeath] saplings jump out. \n \nActive: Deal [MagicDamageCalc2] magic damage to target and restore [HealthCalc1] + [ActiveMissingHealthHeal] missing Health.

### Taric (5-cost, )

- Traits: Emerald Aspect, Vanguard | mana 0/70
- **Emerald Radiance**: Passive: The first time Taric or his paired ally drops below [PassivePercentHealthThreshold] Health, unleash emerald energy from both of them, granting [ShieldCalc1] Shield to allies within [PassiveHexRadius] Hexes for [PassiveShieldDuration] seconds. \n \nActive: Restore [HealthCalc1] Health. Taric and his paired ally deal [MagicDamageCalc1] bonus magic damage on their next [NumAttacks] attacks.

## Monsters, summons, pets and other trait-less units (17)

Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do.

### Elder Dragon (1-cost, ADTank)

- Traits: (none)
- **Primordial Breath**: Passive: Attacks splash in a cone behind the target.

### Golem (1-cost, APTank)

- Traits: (none) | mana 30/100
- **Golem Smash**: Slam the target dealing [ModifiedDamage] () magic damage.

### Krug (1-cost, ADTank)

- Traits: (none)
- **Stone Salvage**: Passive: Heal to full Health whenever an allied Krug dies.

### Murk Wolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Murkwolf (1-cost, ADTank)

- Traits: (none)
- **Pack Attack**: Passive: Leap to the enemy backline at the beginning of combat.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Razorbeak (1-cost, ADTank)

- Traits: (none)
- **Furious Flock**: Passive: Gain Attack Speed whenever an allied Razorbeak dies.

### Rift Herald (1-cost, ADTank)

- Traits: (none)
- **Void Charge**: Combat start: Charge forward, dealing 50% max Health magic damage and stunning enemies.

### Rift Scuttler (1-cost, )

- Traits: (none) | mana 0/100
- **Swimming Speed**: Passive: Occasionally dash around the board.

### Training Dummy (1-cost, ADTank)

- Traits: (none)
- **On Duty!**: The Training Dummy cannot move or attack. It is also dressed like a devilishly handsome Yordle.

### Voidspawn (1-cost, )

- Traits: (none)
- **Voidborn**: The Voidspawn gains bonus Health and Attack Damage based on the current Stage.

### Artifact Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Artifact Item Anvil**: Drag this to the Shop to open an Armory full of Artifact items! You will have 30 seconds to choose.

### Completed Item Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Completed Item Anvil**: Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose.

### Component Anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Component Anvil**: Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose.

### Support item anvil (8-cost, )

- Traits: (none) | mana 0/100
- **Support item anvil**: Drag this to the Shop to open an Armory full of Support items! You will have 30 seconds to choose.

### Tome of Traits (8-cost, )

- Traits: (none) | mana 0/100
- **Tome of Traits**: Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose.

### Mercenary Chest (11-cost, )

- Traits: (none) | mana 0/12
- **Mercenary Chest**: This chest stores all of the accumulated loot you've collected. Win a combat against a player to open it!


---
Total: 1969 champion entries and 699 traits across 18 sets.
