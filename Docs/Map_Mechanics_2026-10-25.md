# Map mechanics (approved 25/10/2026)

Six maps, each with five slots:

1. **Stat boost:** base stats only (damage, attack speed, armour, gold, move speed). Every boost shows text over the hero's head: the stat and its amount (e.g. "+15% DAMAGE"), no seconds.
2. **Atmosphere:** debuffs enemies.
3. **Enemy mechanic:** buffs enemies, but we profit if a condition is met.
4. **Unique 1:** buffs enemies, but we profit if a condition is met.
5. **Unique 2:** buffs us.

Rules that apply to every map:
- Every area is a solid fill with no outline.
- Armour goes to the hero only. Friends are immortal and never get heals, armour or dodge.
- "Hero hit" means the hero's weapon damage.
- Overhead text uses ImpactWords.
- All effects are drawn in code (ink, shaders where needed). Only objects come from art.

Map order: Waiting Room waves 1-4, Post Office 5-8, Glasshouse 9-11, Lamp Hall 12-14, Theatre 15-17, Archive Vault 18-20 (endless repeats the cycle).

**Bosses (changed 26/10/2026: too many bosses):** one boss every fourth night.
- 4 Coin Widow, 8 Chair Porter, 12 Door That Stayed Shut, 16 The Marquee Maw, 20 Last Customer.
- A boss night is 10 s longer than an ordinary one (was 30 s).
- The six mid-map bosses are gone from the road (their bodies remain in endless).
- The Keeper's carry happens on nights 6, 11 and 17 (never a boss night).

**Removed:** sliding chairs, sliding shelves, the old gust push, moths, the ledger ring.

## 1. The Waiting Room (1-4)
- **Boost – Queue tickets.**
  - One ticket every 12 s near the hero; it lies on the floor for 8 s.
  - Each ticket lasts 10 s, by colour:
    - red: +20% damage;
    - yellow: +20% attack speed;
    - grey: +3 armour;
    - white: +1 gold a kill;
    - blue: +15% move speed.
  - A ticket matching the number on the board gives double.
  - Picking the same colour again adds 10 s, capped at 20 s.
- **Atmosphere – Ceiling dust.** 3-4 patches that move every 15 s. Enemies inside are slowed 15% and attack 10% slower. Enemies leave footprints for 3 s.
- **Enemy – Impatient waiters.**
  - An enemy that has barely moved for 4 s becomes angry for 6 s: +25% move speed, +20% damage.
  - **We profit:** while angry it takes +25% damage, and killing it then drops double gold.
- **Unique 1 – Number board.**
  - A number is called every 15 s. Its door shakes, a solid red glow shows under it, and a 3-2-1 countdown runs.
  - The door has a health bar of about 3 ordinary enemies.
  - **Broken in time:** no pack, 3-8 gold, plus 1 ticket.
  - **Not in time:** 6-10 enemies pour out, with no gold.
- **Unique 2 – Wall clock.**
  - The hand goes round every 30 s. At 12 the clock glows for 6 s.
  - Standing within 2 units triggers it: every enemy is stunned for 5 s (bosses for 1.5 s).

## 2. The Post Office (5-8)
- **Boost – Chute letters.**
  - 5 letters every 13 s; the third is gold.
  - Each letter gives one of: +15% damage, +15% attack speed, +2 armour, +1 gold a kill, or +15% move speed.
  - A letter lasts 8 s, a gold one 11 s.
  - Three from one drop is a FULL DELIVERY: all three active together, +4 s.
- **Atmosphere – Paper wind.** A 4 s gust every 11 s, warned 1 s before. Enemies walking against it are slowed 20%; enemy shots drift and fly 25% slower.
- **Enemy – Stamped enemies.**
  - Every 10 s one enemy gets a red stamp for 8 s: +20% move speed, +20% damage.
  - **We profit:** killed before the stamp burns out, it bursts in a 1.5-unit radius for 30% of its max health on nearby enemies and drops a letter.
- **Unique 1 – Giant parcel.**
  - On about a third of nights one lands, warned 1.3 s before. Left for 11 s, an elite climbs out.
  - **We profit:** it has a health bar of about 4 ordinary enemies and can be broken by shots or blades from any range. It bursts in a 2.8-unit radius for 4x the hero hit and spills 10-25 gold.
- **Unique 2 – Paper planes.**
  - Every 18 s, 8-12 planes cross the map along a solid lane 1.5 units wide, warned 1.5 s before.
  - Each plane hits an enemy for 50% of the hero hit, at most 3 planes per enemy per pass. Planes never hurt the hero.

## 3. The Glasshouse (9-11, new map)
- **Boost – Blooms.**
  - 6 pots; each blooms after 15 s (10 s in sun).
  - Walking through a bloom picks it for 10 s:
    - red: +15% damage;
    - yellow: +20% attack speed;
    - brown: +3 armour;
    - orange: +1 gold a kill;
    - blue: +15% move speed.
  - A picked pot regrows in 15 s.
- **Atmosphere – Sun and rain, alternating every 20 s.**
  - **Sun:** enemies are dazzled and attack 15% slower. Wax Admiral takes +30% damage and is slowed 20%.
  - **Rain:** enemies are slowed 15% and knocked back 50% further.
- **Enemy – Pollen carriers.**
  - One every 12 s, with a gold aura. Enemies within 2.5 units get +20% attack speed and +10% move speed.
  - **We profit:** killing it drops a flower bed (solid, 2-unit radius, 8 s). Inside it the hero gets +15% damage and +15% attack speed.
- **Unique 1 – Cracking roof.**
  - Every 18 s a pane cracks over a solid red 2-unit area, warned 2.5 s before.
  - When it breaks, the hero inside loses 10% of max health.
  - **We profit:** enemies inside lose 30% of max health (bosses 5%).
- **Unique 2 – Shooting flower.**
  - 2 big buds. Standing by one for 2 s (with a progress bar) opens it into a turret for 15 s.
  - The turret shoots the nearest enemy within 7 units every 0.6 s for 60% of the hero hit. It returns after 20 s.

## 4. The Lamp Hall (12-14)
- **Boost – Lamp light and lamp oil.**
  - Standing in a lamp's light (2.8 x 1.8 oval): +15% damage, +2 armour, friends +10% attack speed.
  - Every 15 s a lit lamp drops an oil vial. It gives 10 s of one of: +20% attack speed, +1 gold a kill, or +15% move speed.
- **Atmosphere – Drowsy dark.** Black ash drifts outside the light. Enemies in the dark attack 20% slower.
- **Enemy – Night prowlers.**
  - Enemies in the dark get +20% move speed.
  - **We profit:** an enemy stepping into the light is dazzled, frozen 1 s and takes +20% damage for 3 s (at most once per 5 s each).
- **Unique 1 – Snuffing hand.**
  - Snuffs one lamp every 15 s. The lamp relights by itself after 9 s.
  - **We profit:** relit within 3 s (walk within 1.3 units), the lamp flares and stuns enemies in its light for 1.5 s.
- **Unique 2 – Carried lantern.**
  - One appears every 20 s. Standing by it for 1.5 s (with a progress bar) picks it up for 10 s.
  - While held:
    - the hero is a moving light (2-unit radius) with the full light boost;
    - the flame burns up to 6 enemies within 2 units for 40% of the hero hit per second;
    - burnt enemies attack 20% slower.

## 5. The Forgotten Theatre (15-17, new map)
- **Boost – Three-colour spotlight.**
  - A 1.8-unit spotlight drifts about 1 unit/s and changes colour every 10 s. Inside it:
    - white: +20% damage;
    - yellow: +20% attack speed;
    - pink: +3 armour.
  - **ENCORE:** 3 s straight in the light gives +1 gold a kill and +15% move speed for 8 s, even after leaving it (10 s cooldown).
- **Atmosphere – Stage smoke.** Low smoke; a drum every 15 s parts it. Enemies inside are slowed 10%, and ranged enemies lose 25% range.
- **Enemy – Ghost audience.**
  - While the hero is outside the spotlight, enemies in the seats throw something every 3 s for 50% of an ordinary enemy's hit.
  - **We profit:** while the hero is in the light they applaud and throw coins instead (1-3 gold every 3 s).
- **Unique 1 – Velvet curtain.**
  - Every 25 s it drops across the map for 5 s, warned 2 s before by a solid area. Our shots don't pass it.
  - **We profit:** enemies under it lose 25% of max health (bosses 5%).
- **Unique 2 – Firework machine.**
  - Each kill inside the spotlight loads 1. At 25 it is ready.
  - Standing by it for 1 s fires: every enemy takes 3x the hero hit and freezes for 1 s.

## 6. The Archive Vault (18-20)
- **Boost – Flying pages.**
  - A page crosses the map every 10 s. Each lasts 10 s, randomly one of: +15% damage, +15% attack speed, +2 armour, +1 gold a kill, or +15% move speed.
  - Three within 30 s is a CHAPTER: all three together for 20 s.
- **Atmosphere – Ink fog.** 2-3 lanes that move every 20 s. Enemies inside are slowed 10% and deal 10% less damage.
- **Enemy – Bookworms.**
  - They crawl to pages on the floor and eat them: +30% max health and +15% damage per page, up to 3.
  - **We profit:** killing one spits back every page eaten (to pick up), plus 5 gold per page.
- **Unique 1 – Spilled inkpot.**
  - Every 25 s a giant inkpot tips (warned 3 s) into a solid 3-unit ink pool that lasts 10 s.
  - Enemies in the pool get +30% move speed and +15% damage.
  - **We profit:** an enemy that dies in the pool bursts for 50% of its max health on enemies within 1.5 units (chains).
- **Unique 2 – Stamp from the sky.**
  - Every 15 s a giant stamp falls on the densest crowd: a solid 2.5-unit area, warned 1.5 s before.
  - Enemies hit take 3x the hero hit, and survivors are marked PROCESSED: +25% damage taken for 8 s.
  - It never hurts the hero.

## Art needed
- **Floors:** Glasshouse, Theatre.
- **Props:**
  - door with 3 crack stages;
  - wall clock;
  - paper plane;
  - pot (bud and bloom, 5 colours);
  - big bud and turret flower;
  - lantern;
  - oil vial;
  - firework machine;
  - curtain;
  - inkpot;
  - stamp;
  - tickets (5 colours);
  - pages.
