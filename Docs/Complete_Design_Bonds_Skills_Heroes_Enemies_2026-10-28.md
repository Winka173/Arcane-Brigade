# Complete design file: bonds, traits, companion skills, heroes, enemies (28/10/2026)

Version 3. Replaces the review pack of the same day. DRAFT: numbers are placeholders for balance.

## 0. Changes since the last file

- **Companions are mortal.** A friend has health. At 0 it falls and stands up **15 s** later with 40% health. If every friend is down at the same time, the wave is lost. At the start of each wave all fallen friends are back with full health.
- **Houses:** 10 houses, 20 major bonds (9 members each, steps 2/4/6/8), 32 minor bonds (5 or 7 members, steps 2/4/6), **120 companions**, **36 heroes**. Two new houses: **Bulwark + Spirit** and **Melody + Gravity**.
- **Crowd rule:** late waves hold 100+ enemies. Every skill hits an area, line, cone, ring, cluster or species, or works on the hero, friends, economy or time; weak single-target and tiny-summon skills were rewritten. Numbers scale with the enemies in range (capped), never with one enemy.
- **Protection:** 27 companions now carry a protection skill (shield, damage sharing, taunt with damage reduction, wall, heal, revive faster). The new Bulwark and Spirit bonds and the Shield, Mend, Bastion, Spiky Shell, Monolith and Cloud minors support them.
- **Enemies:** 53 enemy kinds with their own skills in five tiers, 12 elite affixes and 6 bosses with phases. Friends are mortal, so ranged, hunter and control enemies now matter.

## Contents
1. Rules and numbers
2. Major bonds (20)
3. Minor bonds (32)
4. Build recipes
5. Personal traits (16)
6. Companion skills (120)
7. Protection skills
8. Heroes (36)
9. Enemies (53), affixes, bosses
10. Questions

## 1. Rules and numbers

- **Team:** up to 10 companions + the hero; a run lasts about 20 waves at the start and goes on to 100.
- **Spare member rule:** a bond that needs 8 members has 9, one that needs 6 has 7, one that needs 4 has 5 (one spare, so a build can choose).
- **Targets:** `8+6+2+2+2`, `6+6+2+2+2`, `8+4+2+4+2`. A companion has 2 or 3 bonds; the hero counts as a member of 2 minor bonds.
- **Skill bars** fill with time only (Shooter 6 s, Striker 7 s, Support 8 s, Controller 9 s, Guard 10 s, Summoner 12 s); bonds and relics can speed them up.
- **Effects** are code ink; art is only a piece of the creature itself. No two characters share a species. Body colour = main bond.
- **Houses (10):** Hearth + Ember; Frost + Tide; Storm + Windborne; Garden + Venom; Starlight + Dusk; Carnival + Court; Archive + Lantern; Stone + Clockwork; Bulwark + Spirit; Melody + Gravity.

## 2. Major bonds (20)

Each step starts with a tag naming the kind of rule it is, so the variety is visible. Steps 4 and 6 are not flat bonuses: they build the board state or the counter that step 8 spends (stacks, pools, marks, Sprouts, Mass), so a bond grows into its capstone. The "stat package" is the plain number part that comes with the steps.

### 1. Hearth (warmth and food (stat engine)), house Hearth + Ember

- **Rule kinds:** [Wave-start buff] + [Timed pulse]. **Counter:** Warmth: hero healing and food eaten.
- **2:** [Wave-start buff] Hearty Meal: at the start of every wave all friends gain +12% damage and attack speed, fading over 20 s
- **4:** [Food drop + stacks] kills by Hearth friends drop a Dish (at most one per 8 s); the hero eats it for 3 Warmth, and every Feast (every 12 s) adds 1 more. Each Warmth gives the team +2% damage and attack speed for the wave (up to 10)
- **6:** [Overflow] at 10 Warmth the hero's overhealing becomes a team shield, and each Feast also fills every friend's skill bar by 10%
- **8:** [Summon] the Kitchen: a Chef spirit joins each wave and throws pies at the densest cluster; every pie that kills drops a Dish; Warmth caps at 20 and all Hearth stats double
- **Stat package:** Damage 4% per 2; step 4 AttackSpeed 8%; step 6 Health 15%; step 8 Damage 10% and AttackSpeed 10%
- **Members (9):** Teapot Pilgrim, Candle Snail, Bread Crab, Coil Kettle, Saffron Mechanist, Baker Mouse, Tea Dragon, Sundae Golem, Boba Tadpole

### 2. Ember (fire), house Hearth + Ember

- **Rule kinds:** [On-hit stacks] -> [Burst] -> [Summon]. **Counter:** Heat: enemies burning.
- **2:** [On-hit stacks] every Ember hit applies Burn (stacks to 3, damage over time)
- **4:** [Burst chain] at 3 Burn an enemy bursts (100% weapon damage, small radius) and gives 1 Burn to each neighbour, so a burst can set off the next one (an enemy bursts at most once per 2 s); each burst adds 1 Heat
- **6:** [Fuel] Burn stacks to 5; a burst at 5 stacks deals double and leaves a Firepit for 4 s that burns whoever crosses it; Heat no longer fades between bursts
- **8:** [Summon] at 50 Heat a Phoenix hatches for 15 s and flies through the Firepits, re-lighting them; at 100 Heat an Inferno makes every burst also burst its neighbours for 5 s
- **Stat package:** Damage 5% per 2; step 4 AttackSpeed 6%; step 6 Damage 10%; step 8 Crit 10%
- **Members (9):** Teapot Pilgrim, Candle Snail, Mailbox Imp, Dice Crab, Mask Pepper, Coil Kettle, Saffron Mechanist, Baker Mouse, Tea Dragon

### 3. Frost (ice), house Frost + Tide

- **Rule kinds:** [Stacking slow] -> [Freeze] -> [Summon]. **Counter:** Chill: enemies slowed.
- **2:** [Stacking slow] every Frost hit adds a Chill stack (-6% speed each, up to 5)
- **4:** [Freeze + shatter] at 5 Chill an enemy freezes for 1.5 s; the next hit shatters it for 150% weapon damage and the shards give 2 Chill to everything near it, so a crowd freezes in a chain
- **6:** [Ice floor] every shatter leaves an Ice patch for 3 s that adds Chill to anyone on it; Frost friends deal +30% to frozen enemies; frozen enemies that die rise as Ice Soldiers (up to 4, 12 s)
- **8:** [Wave-start debuff] permanent winter: every enemy starts each wave with 2 Chill; a blizzard every 20 s freezes every enemy that has 3+ Chill at once, and Ice patches double in size
- **Stat package:** Damage 3% per 2; step 4 Armor 2; step 6 Health 12%; step 8 Damage 15%
- **Members (9):** Snow Spoon, Fridge Wisp, Shell Postman, Coral Teacup, Anchor Snail, Sail Squid, The Impossible Tailor, Shovel Penguin, Specimen Slime

### 4. Tide (water and weather), house Frost + Tide

- **Rule kinds:** [Every Nth hit] -> [Cycle] -> [Summon]. **Counter:** Soak: enemies wet.
- **2:** [Every Nth hit] every 5th Tide hit splashes: enemies in the splash are Soaked and take +8% from all sources
- **4:** [Pools] every splash leaves a Tide pool for 4 s; enemies in a pool are Soaked again and drift slowly toward its centre, so crowds bunch up
- **6:** [Reactions] Soaked enemies react with other bonds: Burn makes Steam (enemies inside miss 30%), Chill freezes them at once, Charge shocks the whole pool; every Riptide (every 10 s) shoves all enemies back into the nearest pools
- **8:** [Summon] the Leviathan surfaces once a wave from the largest pool and sweeps the screen, Soaking everything; pools last twice as long
- **Stat package:** AttackRange 4% per 2; step 4 Pickup 10%; step 6 Damage 8%; step 8 Income 2
- **Members (9):** Snow Spoon, Shell Postman, Coral Teacup, Anchor Snail, Sail Squid, Balloon Mallet, Shanty Sea Lion, Bottle Gull, Specimen Slime

### 5. Storm (lightning), house Storm + Windborne

- **Rule kinds:** [Every Nth hit chain] -> [Charge] -> [Low health]. **Counter:** Charge: hits landed.
- **2:** [Every Nth hit] every 6th Storm hit chains lightning to 3 enemies (60% weapon damage)
- **4:** [Charge] chain hits leave 1 Charge; at 5 Charge an enemy discharges to everything near it for 50% weapon damage, and a discharge counts as a chain hit, so charge can ripple through a crowd
- **6:** [Lightning rod] every 15 s a rod lands on the densest cluster for 6 s: chains that pass near it jump to 3 more enemies and give 2 Charge to each
- **8:** [Aura] a storm cloud follows the hero and strikes the most charged cluster every 2 s; a strike uses up the Charge for double damage, and every discharge drops a small rod for 3 s
- **Stat package:** CompanionAttackSpeed 5% per 2; step 4 Crit 5%; step 6 ChainChance 5%; step 8 Damage 12%
- **Members (9):** Kettle Bat, Comet Kite, Ribbon Otter, Rain Heron, Raincoat Frog, Kite Dragon, Static Ferret, Storm Kitten, Volt Eel

### 6. Windborne (wind), house Storm + Windborne

- **Rule kinds:** [Movement] -> [Volley counter] -> [Summon]. **Counter:** Volleys: shots fired.
- **2:** [Movement] Tailwind: moving builds Momentum, +1% damage per second moving (up to 10%, lost when you stop for 2 s)
- **4:** [Wind lanes] every 10th volley pierces everything and leaves a wind lane for 3 s; friendly bullets that cross a lane gain +20% speed and +1 pierce
- **6:** [Momentum] at full Momentum Windborne friends fire +1 projectile per volley; a Wind Beast joins and runs along the lanes (drag a friend onto it to pick guard, shoot or sweep)
- **8:** [Timed pulse] a Gale every 8 s turns every lane into a cyclone that drags enemies along it, and gives all friends +20% speed for 4 s
- **Stat package:** BulletSpeed 6% per 2; step 4 AttackRange 8%; step 6 BulletPierce 1; step 8 Damage 10%
- **Members (9):** Kettle Bat, Comet Kite, The Folded Courier, Ribbon Otter, Spindle Wren, Paper Plane Pigeon, Kite Dragon, Static Ferret, Storm Kitten

### 7. Garden (plants), house Garden + Venom

- **Rule kinds:** [Permanent growth] -> [Summon plants]. **Counter:** Growth: waves survived.
- **2:** [Permanent growth] every wave survived, each friend gains +2% health and damage for the rest of the run
- **4:** [Summon + growth] seed pods sprout into Sprouts (up to 4) that shoot; each Sprout grows one stage every 2 waves (a Sapling roots the enemies in front of it), and step 2 growth also applies to Sprouts
- **6:** [Grove] Sprouts within 4 units link with vines; enemies crossing a vine are rooted for 1 s; the oldest plant becomes a Treant Guardian that taunts
- **8:** [Economy] the Treant grows into the Great Tree: it fruits every wave (choose gold, a hero heal or +damage) and every Sprout grows one stage at once
- **Stat package:** Health 6% per 2; step 4 HeartHealing 10%; step 6 Income 1; step 8 Health 15% and Damage 8%
- **Members (9):** Pear Moth, Mushroom Librarian, Citrus Toad, Seed Hedgehog, Grand Sprout, Paper Devil, Watering Snail, Yo-yo Squirrel, The First Seed

### 8. Venom (poison), house Garden + Venom

- **Rule kinds:** [Stacking DoT] -> [Death spread] -> [Rot]. **Counter:** Toxin: stacks on enemies.
- **2:** [Stacking DoT] every Venom hit applies Poison (stacks to 5, damage over time)
- **4:** [Death spread] poisoned enemies that die pass their stacks +1 to each neighbour, and poisoned enemies deal 10% less damage
- **6:** [Rot] at 8+ stacks an enemy Rots: it loses 3% of its max health a second (strong against bosses) and gives off a cloud that adds 1 stack a second to its neighbours, pushing them toward Rot
- **8:** [Aura] plague: poison spreads on its own between nearby enemies and slows them 20%; a Rotting enemy that dies bursts its cloud; at 200 Toxin a Pandemic gives every enemy +3 stacks
- **Stat package:** Damage 4% per 2; step 4 CollisionDamage 2; step 6 Crit 6%; step 8 Damage 14%
- **Members (9):** Pear Moth, Mushroom Librarian, Citrus Toad, Seed Hedgehog, Sock Cyclops, Mirror Newt, Ribbon Spider, Paper Devil, Watering Snail

### 9. Starlight (stars and luck), house Starlight + Dusk

- **Rule kinds:** [Crit trigger] -> [Economy] -> [Choice]. **Counter:** Wishes: critical hits.
- **2:** [Crit trigger] every crit has a chance to call a falling star onto a cluster
- **4:** [Stardust] falling stars leave Stardust for 4 s: friendly bullets passing through it gain +15% crit chance; every 5 crits make a Wish (spent at the shop on free rerolls)
- **6:** [Choice] pick a constellation for the run (Archer: range, Bear: health, Twins: companion damage), and stars that land in Stardust split into 3
- **8:** [Transform] Starfall: a comet falls on every crit, and once a wave all friends take a Celestial form for 8 s (+25% crit chance, +50% crit damage)
- **Stat package:** Crit 4% per 2; step 4 Luck 5%; step 6 Damage 8%; step 8 Crit 10%
- **Members (9):** Pillow Oracle, Ice Cream Owl, Moon Jelly, Star Sock, Eclipse Egg, Orbit Moth, Bubble Koi, The Sleeping Friend, Little Sprout

### 10. Dusk (shadow), house Starlight + Dusk

- **Rule kinds:** [Mark] -> [Summon] -> [Execute]. **Counter:** Dusk: enemies defeated under a mark.
- **2:** [Mark] at the wave start the first 10 enemies hit are marked: each loses 12% strength and gives it to your best friend
- **4:** [Passing mark] a marked enemy that dies passes its Mark to the nearest unmarked enemy, so marks travel through the crowd; marked enemies that die rise as Shades (up to 5)
- **6:** [Execute] marked enemies below 12% health die at once; each execute gives Shades +10% damage for the wave, and Shades mark what they hit
- **8:** [Timed] eclipse: after 8 s the weakest 5% of enemies fall every 3.5 s, and during the eclipse every enemy counts as marked
- **Stat package:** Damage 4% per 2; step 4 CompanionDamage 6%; step 6 ChampionDamage 8%; step 8 Damage 12%
- **Members (9):** Pillow Oracle, Ice Cream Owl, Moon Jelly, Eclipse Egg, Orbit Moth, The Forgotten Patchwork, The Sleeping Friend, Violet Remnant, Saddle Moth

### 11. Carnival (circus and chance), house Carnival + Court

- **Rule kinds:** [Random] -> [Economy] -> [Choice]. **Counter:** Applause: different skills cast within 5 s.
- **2:** [Random] every 5th hit rolls a die: damage burst, coin, slow, speed, crit or nothing
- **4:** [Jackpot] applause (different skills cast within 5 s) and dice rolls fill a jackpot; full: a coin shower and +15% team damage for 6 s; the die gains a 7th face, Encore (the last skill casts again)
- **6:** [Loaded dice] roll twice and keep the better face; every 3 waves cash out the jackpot or Double Down
- **8:** [Transform] Grand Finale: when the jackpot fills, all friends cast their skills together, and for 5 s the die rolls on every hit
- **Stat package:** Luck 5% per 2; step 4 Income 1; step 6 Discount 4%; step 8 Damage 10%
- **Members (9):** Jelly Swordsman, Brass Acorn, Button Beetle, Radish Jester, Crayon Gremlin, Return Captain, Carousel Pony, Ringmaster Jack, The Lost Carousel

### 12. Court (royalty and law), house Carnival + Court

- **Rule kinds:** [Choice] -> [Transform] -> [Rule]. **Counter:** Rank: stars of the best friend.
- **2:** [Choice] at each wave start choose 1 of 3 small edicts for that wave
- **4:** [Crown] the best-star friend is crowned and gains an extra skill; whenever it casts, every Court friend's skill bar fills 15%
- **6:** [Rule] decree: write one law for the run (a cause and an effect); edicts now offer 4 choices and the crowned friend's casts count as a cause
- **8:** [Rule] coronation: two laws, and a second friend is crowned
- **Stat package:** CompanionDamage 5% per 2; step 4 Discount 3%; step 6 RecruitDiscount 5%; step 8 Damage 10%
- **Members (9):** Jelly Swordsman, Brass Acorn, Button Beetle, Radish Jester, Return Captain, Dough Foreman, Velvet Enchantress, Carousel Pony, Court Herald

### 13. Archive (knowledge), house Archive + Lantern

- **Rule kinds:** [Mark] -> [Permanent growth] -> [Copy]. **Counter:** Records: species recorded.
- **2:** [Mark] the first hit marks a species: +12% damage to it; each run one random recorded-bonus family (damage, coin or page copy)
- **4:** [Records + pages] each new species recorded gives +1% damage for the run; the first kill of each new species drops a Page (a one-use copy of a friend's skill, cast on the largest cluster)
- **6:** [Chapter] 3 Pages bind into a Chapter that casts all three at once; recorded species take +10% damage from Pages
- **8:** [Debuff] complete set: every recorded species is permanently weakened 15%, and Pages drop twice as often
- **Stat package:** Damage 3% per 2; step 4 Crit 4%; step 6 Luck 5%; step 8 Damage 10%
- **Members (9):** Hood Wisp, Bookmark Golem, Thimble Moth, Reel Owl, Velvet Champion, Lens Owl, The Endless Index, Pinned Beetle, Paper Inventor

### 14. Lantern (lamplight), house Archive + Lantern

- **Rule kinds:** [Stillness] -> [Aura] -> [Summon]. **Counter:** Light: enemies in lamplight.
- **2:** [Stillness] Steady Flame: standing still for 2 s gives +15% range and damage until you move
- **4:** [Aura + wicks] enemies within 6 units of the hero are Lit and take +8% damage; Lit enemies that die leave a Wick that widens the light by 0.5 units for the wave (up to +3)
- **6:** [Summon] lamp wisps orbit the hero and shoot beams; while Steady Flame holds, the wisps fly out and stand as lamps that light their own area
- **8:** [Aura] a lighthouse: one huge lamp lights the arena; enemies in any light take +25%, and Wicks also power the lamp wisps
- **Stat package:** AttackRange 5% per 2; step 4 Damage 5%; step 6 ChampionDamage 8%; step 8 Crit 8%
- **Members (9):** Hood Wisp, Bookmark Golem, Thimble Moth, Reel Owl, Velvet Champion, Teacup Cricket, Lens Owl, Harbor Whale, Lamp Goat

### 15. Stone (earth), house Stone + Clockwork

- **Rule kinds:** [Reflect] -> [Collision] -> [Summon]. **Counter:** Weight: enemies knocked back.
- **2:** [Reflect] Thorn Skin: enemies that touch the hero take 30% of their own attack back
- **4:** [Collision] knocked-back enemies that hit others deal 100% weapon damage to them; an enemy hit by 3 collisions is Cracked (+15% damage taken)
- **6:** [Timed pulse] an earthquake every 12 s stuns enemies near the hero for 0.5 s and throws Cracked enemies into their neighbours, setting off more collisions
- **8:** [Summon] the Colossus: a stone golem ally fights for the wave and its stomps are earthquakes
- **Stat package:** Health 6% per 2; step 4 Knockback 10%; step 6 Armor 2; step 8 Damage 12%
- **Members (9):** Iron Locomotive, Fossil Armadillo, Tin Tick, Staple Mole, The Last Bell, Bristle Ball, Monster Marble, Top Beetle, Stitched Pangolin

### 16. Clockwork (metal and time), house Stone + Clockwork

- **Rule kinds:** [Time] -> [Echo] -> [Summon]. **Counter:** Ticks: skills cast.
- **2:** [Time] skill bars fill 20% faster
- **4:** [Echo] every 8th cast all skills of the bond fire twice (the second at half strength); each echo winds a Spring
- **6:** [Time stop] 1.5 s every 20 s (enemies freeze, friends keep acting); skills cast during a stop hit all at once when time resumes, and each Spring makes the next stop 0.1 s longer
- **8:** [Summon] overclock turrets: a gear turret per 8 casts (up to 3, they accept relics as upgrade slots); skill bars fill twice as fast for 10 s after each time stop
- **Stat package:** CompanionAttackSpeed 5% per 2; step 4 AttackSpeed 5%; step 6 MoveSpeed 8%; step 8 AttackSpeed 10%
- **Members (9):** Iron Locomotive, Fossil Armadillo, Tin Tick, Spring Duck, Staple Mole, Dream Compass, The Last Bell, Clockwork Piper, Top Beetle

### 17. Bulwark (protection), house Bulwark + Spirit

- **Rule kinds:** [Wave-start shield] -> [Damage sharing] -> [Summon wall] -> [Last stand]. **Counter:** Guard: damage absorbed for friends.
- **2:** [Wave-start shield] every friend starts each wave with a shield worth 15% of its max health
- **4:** [Damage sharing] a friend that is hit passes 25% of the damage to the nearest Bulwark friend, which takes it at -30%; absorbed damage fills Guard, and full Guard makes that friend slam a shockwave line ahead of it
- **6:** [Summon wall] a Bastion wall rises where the most friends stand every 15 s and blocks bullets and bodies for 6 s; damage the wall takes counts double toward Guard, and broken shields refill once while it stands
- **8:** [Last stand] once a wave the first friend that would fall stays at 1 HP and is untouchable for 3 s; the whole team gains +3 armor
- **Stat package:** Health 6% per 2; step 4 Armor 2; step 6 Health 12%; step 8 Armor 3 and Damage 8%
- **Members (9):** Tower Rook, Lifebuoy Hippo, Nurse Hen, Phoenix Chick, Dandelion Puff, Totem Pole, Brass Lion, Quilt Walrus, Plaster Panda

### 18. Spirit (souls and revival), house Bulwark + Spirit

- **Rule kinds:** [Death effect] -> [Revive] -> [Revenant] -> [Aura]. **Counter:** Souls: friends and enemies fallen.
- **2:** [Death effect] a friend that falls leaves a Spirit that fights for 6 s
- **4:** [Revive] fallen friends stand up 4 s sooner (11 s); Spirits heal the hero 1% a second, and enemies killed by Spirits give Souls
- **6:** [Revenant] the first friend that falls each wave rises at once with 50% health; every 5 Souls make Spirits last 1 s longer
- **8:** [Aura] the Spirit Realm: for 15 s each wave friends cannot fall (they drop to 1 HP and keep fighting); +1% damage per 10 Souls this wave
- **Stat package:** Health 5% per 2; step 4 HeartHealing 10%; step 6 CompanionDamage 8%; step 8 Damage 12%
- **Members (9):** Tower Rook, Lifebuoy Hippo, Nurse Hen, Phoenix Chick, Dandelion Puff, Totem Pole, Pumpkin Reaper, Bone Xylophone, Scarecrow Crow

### 19. Melody (sound and rhythm), house Melody + Gravity

- **Rule kinds:** [Every Nth cast] -> [Timed pulse] -> [Stillness] -> [Summon]. **Counter:** Beats: skills cast.
- **2:** [Every Nth cast] every 4th cast by any friend is a Beat: all friends gain +10% attack speed for 4 s
- **4:** [Timed pulse] a Chorus every 10 s: every friend casts a free small copy of its last skill; casts during a Beat count double toward the next Beat
- **6:** [Crescendo] Beats stack up to 3; standing still for 2 s makes the next Beat last twice as long and reach the whole arena
- **8:** [Summon] the Orchestra: three musician spirits join each wave and play damage, speed and shield notes on every Beat
- **Stat package:** CompanionAttackSpeed 6% per 2; step 4 AttackSpeed 6%; step 6 Crit 6%; step 8 Damage 10% and AttackSpeed 8%
- **Members (9):** Metronome, Harp Swan, Bagpipe Octopus, Anvil Ox, Sinkhole Worm, Planet Hamster, Cymbal Monkey, Choir Canary, Piano Crocodile

### 20. Gravity (space and weight), house Melody + Gravity

- **Rule kinds:** [Pull] -> [Clump bonus] -> [Summon hole] -> [Aura]. **Counter:** Mass: enemies pulled together.
- **2:** [Pull] every Gravity hit pulls the enemy 1 unit toward the hero (good against crowds)
- **4:** [Clump bonus] enemies within 2 units of three or more others take +12% damage; pulled enemies that slam into a clump deal 50% weapon damage to it and add 1 Mass
- **6:** [Summon hole] a Singularity opens every 12 s on the densest cluster: it drags and damages for 3 s, then collapses in a blast; every 10 Mass cuts 1 s off its cooldown
- **8:** [Aura] a gravity well follows the hero: enemies inside move 25% slower and bunch up, and every Singularity collapse leaves a smaller hole for 3 s
- **Stat package:** Damage 4% per 2; step 4 Knockback 10%; step 6 Damage 8%; step 8 Damage 14%
- **Members (9):** Metronome, Harp Swan, Bagpipe Octopus, Anvil Ox, Sinkhole Worm, Planet Hamster, Newton Apple, Bowling Sloth, Satellite Bee

## 3. Minor bonds (32)

Each minor is a small rule of its own. Steps 2 and 4 (and 6 for the seven-member ones). The minors that do not belong to a house can appear on companions of any house.

| Bond | Members | Rule | House |
|---|---|---|---|
| Sweet Tooth | 7: Teapot Pilgrim, Candle Snail, Mailbox Imp, Bread Crab, Baker Mouse, Sundae Golem, Boba Tadpole | [Drop] every 15 kills a Snack drops and heals the hero 3% (4: every 10 kills; 6: also +1 coin) | Hearth + Ember |
| Bouncing Ball | 5: Mailbox Imp, Bread Crab, Dice Crab, Mask Pepper, Boba Tadpole | [Projectile] shots bounce once (4: twice, each bounce keeps 70% damage) | Hearth + Ember |
| Gauntlet | 7: Snow Spoon, Fridge Wisp, Anchor Snail, Sail Squid, The Impossible Tailor, Balloon Mallet, Shovel Penguin | [Stacking] melee hits stack Grit: +2% damage each up to +10% (resets after 4 s idle; 4: up to 20%; 6: stacks never reset in a wave) | Frost + Tide |
| Sea Shanty | 5: Fridge Wisp, Shell Postman, Coral Teacup, Shanty Sea Lion, Bottle Gull | [Every Nth hit] every 4th attack by any friend gives all friends +3% attack speed for 4 s (4: every 3rd) | Frost + Tide |
| Paper Wings | 7: Kettle Bat, Comet Kite, The Folded Courier, Spindle Wren, Rain Heron, Paper Plane Pigeon, Kite Dragon | [Projectile] shots curve in a gentle spiral (+10% distance; 4: they return like a boomerang; 6: twice) | Storm + Windborne |
| Rain Script | 5: Ribbon Otter, Spindle Wren, Rain Heron, Raincoat Frog, Volt Eel | [Economy] every reroll gives +0.5% permanent damage (4: +1%) | Storm + Windborne |
| Quicksoles | 7: Pear Moth, Citrus Toad, Sock Cyclops, Mirror Newt, Ribbon Spider, Paper Devil, Yo-yo Squirrel | [Movement] +6% move speed and a short dash every 8 s (4: every 6 s; 6: the dash leaves a shock wave) | Garden + Venom |
| Root | 5: Mushroom Librarian, Seed Hedgehog, Grand Sprout, Watering Snail, The First Seed | [Stillness] standing still for 2 s: +20% health regeneration and +2 armor (4: +40% and +4) | Garden + Venom |
| Hats | 7: Pillow Oracle, Ice Cream Owl, Moon Jelly, Orbit Moth, The Forgotten Patchwork, Violet Remnant, Little Sprout | [Economy] the first recruit each shop is 25% cheaper (4: 40%; 6: free once a run) | Starlight + Dusk |
| Clover | 5: Star Sock, Eclipse Egg, Bubble Koi, Little Sprout, Saddle Moth | [Random] every 20 kills a lucky Clover drops: a free reroll (4: every 12 kills) | Starlight + Dusk |
| Masks | 7: Jelly Swordsman, Brass Acorn, Radish Jester, Return Captain, Dough Foreman, Velvet Enchantress, Court Herald | [Crit] +4% crit; every 5th crit deals double damage (4: every 4th; 6: every 3rd) | Carnival + Court |
| Big Top | 5: Button Beetle, Crayon Gremlin, Dough Foreman, Ringmaster Jack, The Lost Carousel | [Combo] hitting different enemies in a row builds a combo: +1% damage per link up to +10% (4: +20%) | Carnival + Court |
| Runes | 7: Hood Wisp, Bookmark Golem, Thimble Moth, Lens Owl, The Endless Index, Harbor Whale, Paper Inventor | [Every Nth hit] every 10th hit casts a rune strike for 80% weapon damage in a ring (4: every 8th; 6: every 6th) | Archive + Lantern |
| Exhibit Hall | 5: Reel Owl, Velvet Champion, Teacup Cricket, Pinned Beetle, Lamp Goat | [Survival] +6% max health; once per run the hero survives a fatal hit with 1 HP (4: also heals 20%) | Archive + Lantern |
| Horns | 7: Iron Locomotive, Fossil Armadillo, Spring Duck, Dream Compass, Bristle Ball, Monster Marble, Stitched Pangolin | [Every Nth hit] every 10th hit shoves enemies back and stuns them 0.3 s (4: every 8th; 6: every 6th) | Stone + Clockwork |
| Gears | 5: Tin Tick, Spring Duck, Staple Mole, Clockwork Piper, Top Beetle | [Every Nth hit] every 6th hit fires a bonus shot (4: every 4th) | Stone + Clockwork |
| Shield | 7: Tower Rook, Totem Pole, Brass Lion, Quilt Walrus, Pumpkin Reaper, Bone Xylophone, Scarecrow Crow | [Shield] every wave start each Shield member shields itself for 8% max health (4: also the nearest friend; 6: shield renews every 20 s) | Bulwark + Spirit |
| Mend | 5: Lifebuoy Hippo, Nurse Hen, Phoenix Chick, Dandelion Puff, Plaster Panda | [Heal] friends below 50% health heal 1% a second (4: 2% a second) | Bulwark + Spirit |
| Echo | 7: Metronome, Harp Swan, Bagpipe Octopus, Cymbal Monkey, Choir Canary, Piano Crocodile, Satellite Bee | [Repeat] every 6th cast repeats at 50% (4: every 4th; 6: every 3rd) | Melody + Gravity |
| Singularity | 5: Anvil Ox, Sinkhole Worm, Planet Hamster, Newton Apple, Bowling Sloth | [Pull] every 12th hit pulls enemies in a small ring toward the target (4: every 8th) | Melody + Gravity |
| Bastion | 5: Dice Crab, Shovel Penguin, Specimen Slime, Brass Lion, Plaster Panda | [Wave-start shield] the whole team starts each wave with a shield worth 6% max health (4: 10%) | any |
| Spiky Shell | 5: Teacup Cricket, Grand Sprout, Yo-yo Squirrel, The First Seed, Bowling Sloth | [Reflect] enemies that hit a friend take 40% of that damage back (4: 80%) | any |
| Unstoppable | 5: Saffron Mechanist, Monster Marble, Pumpkin Reaper, Bone Xylophone, Scarecrow Crow | [Immunity] members cannot be stunned or pushed; they charge into the nearest cluster after a kill (4: the charge stuns) | any |
| Monolith | 5: Balloon Mallet, Violet Remnant, Bristle Ball, Stitched Pangolin, Quilt Walrus | [Armor per enemy] +1 armor for each enemy within 4 units, up to +6 (4: up to +10) | any |
| Sniper | 5: The Folded Courier, Paper Plane Pigeon, Raincoat Frog, Storm Kitten, Paper Inventor | [Distance] +2% damage per unit of distance to the target, up to +16% (4: up to +30%) | any |
| Bludgeoner | 5: Tea Dragon, Harbor Whale, Sock Cyclops, Mirror Newt, The Last Bell | [Armor ignore] shots ignore 15% of armor (4: 30%) | any |
| Desperado | 5: Shanty Sea Lion, Carousel Pony, Ringmaster Jack, The Lost Carousel, Court Herald | [Every Nth hit] every 8th hit fires 5 extra shots at the nearest enemies for 40% weapon damage (4: every 6th) | any |
| Bladenado | 5: The Impossible Tailor, Static Ferret, Ribbon Spider, Lamp Goat, Cymbal Monkey | [Splash] hits deal 25% of their damage to every enemy within 2 units of the target (4: 50%) | any |
| Rapidfire | 5: Volt Eel, Bottle Gull, Dream Compass, The Sleeping Friend, Satellite Bee | [On-hit stacks] every hit gives +2% attack speed, up to 10 stacks, lost after 3 s idle (4: +3% per stack) | any |
| Demolitionist | 5: Coil Kettle, Sundae Golem, Crayon Gremlin, Velvet Enchantress, Newton Apple | [Stun] skill damage stuns for 0.3 s (4: 0.6 s, but only the first 10 enemies hit) | any |
| Cloud | 5: Mask Pepper, Star Sock, Bubble Koi, The Forgotten Patchwork, Saddle Moth | [Dodge] the team has a 6% chance to dodge a hit (4: 12%) | any |
| Multicaster | 5: The Endless Index, Pinned Beetle, Clockwork Piper, Choir Canary, Piano Crocodile | [Repeat] the first skill each wave is cast twice (4: the first two) | any |

## 4. Build recipes (examples with 10 companions; the hero joins its two minors)

| Build | Majors | Minors | What it plays like |
|---|---|---|---|
| Burning Court | Ember 8 + Carnival 4 | Desperado, Bladenado | stacked burns burst, dice bonuses chain |
| Frozen Shore | Frost 8 + Tide 6 | Gauntlet, Cloud | chilled, soaked enemies shatter in waves |
| Static Gale | Storm 6 + Windborne 8 | Sniper, Rapidfire | lightning chains over long volleys |
| Poison Grove | Garden 8 + Venom 6 | Root, Spiky Shell | growing allies and spreading poison |
| Night Library | Archive 6 + Dusk 6 | Runes, Bludgeoner | marked species and executions |
| Iron Fortress | Stone 8 + Bulwark 6 | Bastion, Monolith | walls, reflect and last stands |
| Ghost Choir | Spirit 8 + Melody 4 | Echo, Mend | fallen friends rise and sing |
| Black Hole Clock | Gravity 8 + Clockwork 6 | Singularity, Gears | holes pull enemies into clumps and bars fill fast |
| Lamp Court | Lantern 6 + Court 6 | Hats, Masks | light, decrees and crowns |
| Feast Fire | Hearth 8 + Ember 6 | Sweet Tooth, Ball | stats from meals and bouncing fire |
| Cross build: Wet Thunder | Tide 6 + Storm 6 | Rain Script, Demolitionist | soaked enemies conduct lightning |
| Cross build: Shadow Garden | Dusk 6 + Garden 6 | Clover, Cloud | shades grow from poisoned fallen |

## 5. Personal traits (16)

A trait only that companion has. The last four change a rule instead of adding power.

| Companion | Trait | Effect |
|---|---|---|
| Tea Dragon | Undying Flame | the coals it leaves stay for the whole run (up to 12 patches) and burn at +50% |
| Coil Kettle | Coil Shot | the third bounce of Spring Boil becomes a coil shot that chains to two more clusters |
| Sundae Golem | Cherry on Top | every Cherry it drops also adds Warmth and a coin |
| Kite Dragon | Gale Ribbon | each Ribbon Sweep leaves a wind lane for 6 s |
| Eclipse Egg | Dusk or Dawn | its form is bright after a wave with more crits and dark after a wave with more kills: bright gives +Wishes, dark gives +Dusk |
| The First Seed | Thorn Garden | its Sprout stays as a seed pod that grows each wave and finally fruits |
| Clockwork Piper | Tuning Fork | Tune the Clocks stacks up to 3 times (each stack +10% fill speed for 6 s) |
| The Endless Index | Marginalia | Last Page also records the skill, so a copied skill can be used once more at the next wave |
| Ringmaster Jack | Centre Stage | while its Spotlight is up, every skill cast adds Applause twice |
| The Lost Carousel | The Arena Spins | its horses become a lasting ring: enemies cannot stand inside the carousel while it spins |
| Harbor Whale | Sweeping Beam | its Fog Horn also sends the lighthouse beam around the whole arena once |
| The Last Bell | Third Toll | every third toll also rings every Clockwork member once (adds Ticks) |
| Phoenix Chick | Second Wind | a friend revived by its feather returns with 60% health and +20% damage for 8 s |
| Planet Hamster | Gravity Well | counts as two members for Gravity and pulls enemy bullets toward itself |
| Metronome | Perfect Pitch | every Beat of the Melody bond also refills 10% of every friend's skill bar |
| Tower Rook | Stronghold | counts as two members for Bulwark and Spirit and doubles the Bastion wall time |

## 6. Companion skills (120)

Bar, kind, what it does and which part of the creature it comes from. Rewritten skills (crowd or protection) are marked **R**.

### House: Hearth + Ember

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Teapot Pilgrim | Common Shooter | Ember, Hearth, Sweet Tooth | 6 s | **Steep** (Zone) | throws its teapot head: a steam pool for 4 s heals the hero and burns enemies in it. Bond tie: pool fills Warmth; leaves coals (Ember) | its teapot head |
| Candle Snail | Uncommon Controller | Hearth, Ember, Sweet Tooth | 9 s | **Wax Mine** (Trap or mine) | drips wax puddles that stick and ignite when an enemy steps on them. Bond tie: burning enemies add Heat | its candle-cake shell |
| Mailbox Imp | Common Support | Ember, Bouncing Ball, Sweet Tooth | 8 s | **Hot Delivery** (Team buff) | throws sealed hot letters: the 3 nearest friends +20% damage for 6 s, one letter explodes on the farthest enemy. Bond tie: explosion leaves coals | its envelope body and wax seal |
| Bread Crab | Common Guard | Hearth, Sweet Tooth, Bouncing Ball | 10 s | **Toast Shell** (Protection) **R** | shells up: taunts nearby enemies for 3 s while taking 40% less damage, then a crumb pulse. Bond tie: crumbs fill Warmth | its toast shell |
| Dice Crab | Common Support | Ember, Bouncing Ball, Bastion | 8 s | **Loaded Roll** (Random) | rolls its die shell: 1 to 6 sets a fire pulse (N x base), a 6 rolls twice. Bond tie: a 6 adds +10 Heat | its die shell |
| Mask Pepper | Common Support | Ember, Bouncing Ball, Cloud | 8 s | **Masquerade** (Swap) | swaps places with the hero (both teleport); a fire ring bursts at both spots and ignites every enemy in it. Bond tie: lit enemies feed Heat | its theatre mask |
| Coil Kettle | Mythic Controller | Hearth, Ember, Demolitionist | 9 s | **Spring Boil** (Chain) | bounces three times; each landing stuns 0.8 s and vents steam. Bond tie: third bounce chains to one more enemy | its spring-coil spout |
| Saffron Mechanist | Rare Guard | Hearth, Ember, Unstoppable | 10 s | **Pressure Valve** (Eruption) | a ring builds under it (telegraphed), then bursts in a steam blast that pushes enemies back. Bond tie: the blast lights enemies | its wrench and steam valve |
| Baker Mouse | Uncommon Shooter | Hearth, Ember, Sweet Tooth | 6 s | **Rolling Pin** (Projectile pattern) | throws a rolling pin that returns like a boomerang, flattening and slowing enemies both ways. Bond tie: dough heals the hero when 3+ are hit | its rolling pin and apron |
| Tea Dragon | Mythic Shooter | Hearth, Ember, Bludgeoner | 6 s | **Steam Breath** (Beam or channel) | a 2 s flame channel in a cone; each enemy hit burns and gives the hero 1 HP. Bond tie: coals stay between waves | its steaming teapot spout |
| Sundae Golem | Rainbow Controller | Hearth, Sweet Tooth, Demolitionist | 9 s | **Sprinkle Freeze** (Enemy debuff) | a scoop of freezing sprinkles: 2 s freeze in a circle; every 3rd cast drops a Cherry that heals the hero. Bond tie: cherry fills Warmth | its scoops, sprinkles and cherry |
| Boba Tadpole | Common Shooter | Hearth, Sweet Tooth, Bouncing Ball | 6 s | **Pearl Orbit** (Orbit) | tapioca pearls circle it for 5 s and burst on contact. Bond tie: a pearl that kills drops a Snack | its boba pearls |

### House: Frost + Tide

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Snow Spoon | Common Support | Frost, Tide, Gauntlet | 8 s | **Frost Scoop** (Mark) | scoops a wide frost arc: every enemy in the cone is marked and takes +15% from everyone for 5 s. Bond tie: chilled enemies count as marked | its spoon bowl |
| Fridge Wisp | Common Guard | Frost, Gauntlet, Sea Shanty | 10 s | **Ice Box** (Protection) **R** | opens its door and chills 3 friends: each gets a shield worth 12% max health and frost armor for 5 s. Bond tie: leaves a cold patch | its fridge door |
| Shell Postman | Common Shooter | Tide, Frost, Sea Shanty | 6 s | **Special Delivery** (Projectile pattern) | lobs a letter shell onto the densest cluster; it bursts and soaks the area. Bond tie: soaked enemies (Tide) | its stamp and mail satchel |
| Coral Teacup | Uncommon Support | Frost, Tide, Sea Shanty | 8 s | **Tea Time** (Protection) **R** | brews a cup: heals all friends in range 8% and the next reroll costs 1 less. Bond tie: wet enemies pay 1 extra coin | its teacup and coral antlers |
| Anchor Snail | Uncommon Controller | Frost, Tide, Gauntlet | 9 s | **Drag Anchor** (Pull or push) | drags its anchor in a line: every enemy on the line is pulled toward the hero. Bond tie: pulled enemies are soaked | its anchor and rope |
| Sail Squid | Uncommon Guard | Frost, Tide, Gauntlet | 10 s | **Ink Cloud** (Enemy debuff) | an ink cloud halves enemy shields and healing and lowers their damage 15% for 5 s. Bond tie: cloud is a puddle (Tide) | its sail cap and ink |
| The Impossible Tailor | Rainbow Striker | Frost, Gauntlet, Bladenado | 7 s | **Thread** (Tether) | sews a thread between itself and the hero for 5 s: enemies crossing the thread take damage and slow. Bond tie: chilled crossers shatter | its needles and thread |
| Balloon Mallet | Uncommon Guard | Tide, Gauntlet, Monolith | 10 s | **Balloon Pop** (Eruption) | swells (telegraphed) and pops: knock-back and a freezing mist. Bond tie: mist soaks enemies | its balloon body |
| Shovel Penguin | Rare Shooter | Frost, Gauntlet, Bastion | 6 s | **Ice Hole** (Trap or mine) | digs a wide hole: every enemy falling in is frozen in a bubble for 2.5 s. Bond tie: bubbled enemies count as chilled | its shovel |
| Shanty Sea Lion | Common Shooter | Tide, Sea Shanty, Desperado | 6 s | **Shanty** (Team buff) | plays its accordion: all friends +15% attack speed for 6 s. Bond tie: rhythm adds to Sea-shanty | its accordion |
| Bottle Gull | Uncommon Shooter | Tide, Sea Shanty, Rapidfire | 6 s | **Bottle Rain** (Summon) **R** | a rain of bottles bursts across a wide area. Bond tie: bottles soak enemies | its bottle and scarf |
| Specimen Slime | Rare Shooter | Frost, Tide, Bastion | 6 s | **Split Slime** (Clone or echo) | splits into two mini slimes that shoot too for 6 s. Bond tie: slimes are cold and slow | its slime in a jar |

### House: Storm + Windborne

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Kettle Bat | Common Support | Windborne, Storm, Paper Wings | 8 s | **Whistle Gale** (Weather) | whistles up a gale: enemies are blown away for 3 s. Bond tie: gale carries lightning (Storm) | its kettle belly and wings |
| Comet Kite | Common Support | Storm, Windborne, Paper Wings | 8 s | **Slipstream** (Time) | friends flying through its trail fill skill bars 20% faster for 5 s. Bond tie: trail is a wind lane | its tail ribbon |
| The Folded Courier | Rare Shooter | Windborne, Paper Wings, Sniper | 6 s | **Unfold** (Transform) | unfolds into a larger paper form for 5 s and fires paper blades. Bond tie: every blade is a volley | its folded paper wings |
| Ribbon Otter | Uncommon Guard | Storm, Windborne, Rain Script | 10 s | **Ribbon Wrap** (Tether) | ties a ribbon to the hero: enemies that hit the hero are zapped. Bond tie: zaps add Charge | its ribbons and satchel |
| Spindle Wren | Uncommon Support | Windborne, Paper Wings, Rain Script | 8 s | **Weave Wind** (Protection) **R** | weaves a thread shield around the 4 nearest friends (8% max health) and +10% projectile speed for 6 s. Bond tie: adds to Paper Wings | its spool body |
| Rain Heron | Rare Shooter | Storm, Rain Script, Paper Wings | 6 s | **Long Strike** (Projectile pattern) | a piercing spear shot through every enemy in a line. Bond tie: pierced enemies are soaked | its long beak |
| Paper Plane Pigeon | Common Shooter | Windborne, Paper Wings, Sniper | 6 s | **Squadron** (Summon) | releases 4 paper planes that circle and dive at enemies. Bond tie: every plane is a volley | its paper-plane wings |
| Raincoat Frog | Uncommon Shooter | Storm, Rain Script, Sniper | 6 s | **Rain Dance** (Weather) | summons a rain zone: enemies inside are wet and slowed 20%. Bond tie: wet enemies conduct (Storm) | its raincoat and boots |
| Kite Dragon | Rainbow Shooter | Storm, Windborne, Paper Wings | 6 s | **Ribbon Sweep** (Beam or channel) | a rotating ribbon beam sweeps 360 degrees for 3 s. Bond tie: personal trait: sweeps leave gale lanes | its long ribbon tail |
| Static Ferret | Common Striker | Storm, Windborne, Bladenado | 7 s | **Flair** (Chain) | lightning ricochets between up to 5 enemies. Bond tie: each jump adds Charge | its static-charged fur |
| Storm Kitten | Uncommon Shooter | Storm, Windborne, Sniper | 6 s | **Psionic Storm** (Zone) | a small storm cloud drifts through the densest cluster, growing each second. Bond tie: cloud strikes charged enemies | its storm-cloud body |
| Volt Eel | Rare Shooter | Storm, Rain Script, Rapidfire | 6 s | **Lightning Rod** (Trap or mine) | plants a rod: lightning hits every enemy within its radius each second for 4 s. Bond tie: rod adds Charge | its glowing coils |

### House: Garden + Venom

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Pear Moth | Uncommon Striker | Garden, Venom, Quicksoles | 7 s | **Pollen Dash** (Dash or leap) | dashes through enemies leaving poisoned pollen. Bond tie: pollen blooms (Garden) | its leaf wings and pear body |
| Mushroom Librarian | Common Controller | Garden, Venom, Root | 9 s | **Hush** (Enemy debuff) | spores silence enemy skills for 3 s. Bond tie: spores add poison | its book-page mushroom cap |
| Citrus Toad | Uncommon Support | Garden, Venom, Quicksoles | 8 s | **Sour Surprise** (Random) | squirts a random citrus: 1 of 4 effects (slow, blind, burst, coin). Bond tie: peels bloom | its orange-wedge body |
| Seed Hedgehog | Common Controller | Garden, Venom, Root | 9 s | **Seed Spikes** (Trap or mine) | plants seed mines that sprout thorn plants when stepped on. Bond tie: plants root enemies | its seed-sprouting spines |
| Sock Cyclops | Common Striker | Venom, Quicksoles, Bludgeoner | 7 s | **Sock Pile** (Summon) **R** | socks fall on the densest cluster; every sock stuns for 0.5 s. Bond tie: socks carry venom | its striped sock nightcap |
| Mirror Newt | Mythic Shooter | Venom, Quicksoles, Bludgeoner | 6 s | **Hall of Mirrors** (Protection) **R** | mirror decoys absorb the next 6 hits aimed at friends and send them back. Bond tie: decoys leave poison | its mirror scales |
| Ribbon Spider | Common Striker | Venom, Quicksoles, Bladenado | 7 s | **Web Wrap** (Poison) | spins a web zone: slow and stacking poison. Bond tie: poison stacks to 8 | its satin ribbon legs |
| Grand Sprout | Uncommon Guard | Garden, Root, Spiky Shell | 10 s | **Bulb Guard** (Taunt) | roots itself, taunts and grows thorns for 4 s. Bond tie: thorns bloom | its bulb body and leaf plates |
| Paper Devil | Uncommon Guard | Venom, Garden, Quicksoles | 10 s | **Dark Harvest** (Zone) | a dark ring around it drains every enemy inside for 4 s. Bond tie: drain adds Venom stacks | its folded wings and tail |
| Watering Snail | Uncommon Controller | Garden, Venom, Root | 9 s | **Rain Can** (Zone) | waters an area for 5 s: plants grow and enemies are slowed. Bond tie: growth counts a Garden wave | its watering-can body |
| Yo-yo Squirrel | Rare Shooter | Garden, Quicksoles, Spiky Shell | 6 s | **Yo-yo** (Projectile pattern) | a yo-yo that goes out and comes back, hitting twice. Bond tie: hits poison | its yo-yo |
| The First Seed | Rainbow Shooter | Garden, Root, Spiky Shell | 6 s | **Sprout** (Transform) | grows into a small tree for 6 s and shoots thorns. Bond tie: personal trait: the tree stays a pod | its bean-sprout leaves |

### House: Starlight + Dusk

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Pillow Oracle | Mythic Support | Starlight, Dusk, Hats | 8 s | **Lullaby** (Enemy debuff) **R** | a lullaby wave: every enemy in a big radius sleeps 1.5 s; waking hurts them a lot. Bond tie: sleepers are dusk-marked | its pillow body and night cap |
| Ice Cream Owl | Uncommon Controller | Dusk, Starlight, Hats | 9 s | **Brain Freeze** (Enemy debuff) | a cone of cold: every enemy hit loses 30% attack speed for 5 s. Bond tie: frozen cones feed Wishes | its scoops and cone |
| Moon Jelly | Rare Shooter | Starlight, Dusk, Hats | 6 s | **Moonbeam** (Beam or channel) | a 3 s beam; each enemy it crosses loses 15% damage. Bond tie: crit enemies add Wishes | its crescent body |
| Star Sock | Uncommon Shooter | Starlight, Clover, Cloud | 6 s | **Falling Star** (Eruption) | marks a spot (telegraphed) and a star crashes there. Bond tie: star fires on crits | its star patch |
| Eclipse Egg | Rainbow Support | Starlight, Dusk, Clover | 8 s | **Mega Form** (Transform) | hatches into a bigger form for the rest of the wave. Bond tie: form is dark or bright by bond | its cracked starry shell |
| Orbit Moth | Common Controller | Starlight, Dusk, Hats | 9 s | **Orbit** (Orbit) | four orbs circle it and explode on contact. Bond tie: orbs are stars or shadows | its ring and wrapped body |
| Bubble Koi | Common Support | Starlight, Clover, Cloud | 8 s | **Slow Bubble** (Time) | a big time bubble on the ground: every enemy inside moves at 20% speed for 3 s. Bond tie: bubble pops into a wish | its bubbles and fins |
| The Forgotten Patchwork | Rare Shooter | Dusk, Hats, Cloud | 6 s | **Patch Up** (Protection) **R** | sews wounds: heals the 3 lowest-health friends 15% and mends a fallen one 6 s sooner. Bond tie: minion carries shadow | its patches and thread |
| The Sleeping Friend | Rare Shooter | Starlight, Dusk, Rapidfire | 6 s | **Dream Mist** (Weather) | a mist zone: enemies inside miss 30%. Bond tie: mist hides friends too | its dream mist and night cap |
| Violet Remnant | Uncommon Guard | Dusk, Hats, Monolith | 10 s | **Haunt** (Protection) **R** | taunts nearby enemies and takes 50% less damage for 3 s, draining them while it lasts. Bond tie: drain feeds Dusk | its cloth ghost and thimble helm |
| Little Sprout | Uncommon Guard | Starlight, Hats, Clover | 10 s | **Moonbloom** (Protection) **R** | a bloom lights a circle: friends inside gain +10% crit and a 10% shield for 6 s. Bond tie: crits add Wishes | its leaf and bud |
| Saddle Moth | Rare Support | Dusk, Clover, Cloud | 8 s | **Rescue Swap** (Protection) **R** | swaps places with the lowest-health friend, taking its place and shielding it for 8%. Bond tie: swap leaves a shadow | its saddle and wings |

### House: Carnival + Court

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Jelly Swordsman | Common Guard | Carnival, Court, Masks | 10 s | **Jelly Wave** (Clone or echo) **R** | a wobbling wave shoves and slows every enemy in a cone. Bond tie: wave jiggles (Applause) | its jelly cube body and sword |
| Brass Acorn | Common Striker | Carnival, Court, Masks | 7 s | **Acorn Charge** (Dash or leap) | rolls through enemies in a line, knocking them aside. Bond tie: court rank counts | its brass acorn cap |
| Button Beetle | Common Support | Carnival, Court, Big Top | 8 s | **Button Bomb** (Trap or mine) | drops a button mine that stuns on contact. Bond tie: applause when it fires | its button shell |
| Radish Jester | Common Support | Carnival, Court, Masks | 8 s | **Joker Draw** (Random) | draws a card 1 to 9: the damage follows the card. Bond tie: a 9 adds Applause | its jester leaf hat and bells |
| Crayon Gremlin | Common Controller | Carnival, Big Top, Demolitionist | 9 s | **Scribble Pen** (Zone) | draws a crayon circle that enemies cannot cross for 3 s. Bond tie: drawing is a show | its crayon crown |
| Return Captain | Mythic Shooter | Court, Carnival, Masks | 6 s | **Return Command** (Time) | refunds 25% of the skill bar of the 3 nearest friends. Bond tie: rank refunds more | its captain hat and badge |
| Dough Foreman | Rare Guard | Court, Masks, Big Top | 10 s | **Rising Dough** (Eruption) | a dough mound rises (telegraphed) and heaves enemies aside. Bond tie: court rank counts | its dough bucket and apron |
| Velvet Enchantress | Uncommon Controller | Court, Masks, Demolitionist | 9 s | **Hex Cloud** (Enemy debuff) | a hex cloud: every enemy inside is harmless for 2 s. Bond tie: decree strengthens it | its witch hat and wings |
| Carousel Pony | Uncommon Shooter | Carnival, Court, Desperado | 6 s | **Standard** (Dash or leap) | throws a banner then charges to it, stunning on arrival. Bond tie: banner is a royal banner | its saddle and pole |
| Ringmaster Jack | Mythic Shooter | Carnival, Big Top, Desperado | 6 s | **Spotlight** (Mark) | a spotlight on the ground: every enemy in it is drawn toward its centre for 3 s. Bond tie: spotlight adds Applause | its box and top hat |
| The Lost Carousel | Rainbow Shooter | Carnival, Big Top, Desperado | 6 s | **Merry-go-round** (Orbit) | little horses circle it for 6 s and trample enemies. Bond tie: personal trait: the arena spins | its canopy and horses |
| Court Herald | Common Shooter | Court, Masks, Desperado | 6 s | **Decree Tax** (Economy) | enemies hit this cast pay 1 coin each. Bond tie: decree stronger with Rank | its trumpet and tabard |

### House: Archive + Lantern

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Hood Wisp | Common Shooter | Lantern, Archive, Runes | 6 s | **Fog Lure** (Pull or push) **R** | its hooded flame lures every enemy within 8 units toward it for 2 s. Bond tie: lit enemies glow | its hooded flame |
| Bookmark Golem | Rare Support | Archive, Lantern, Runes | 8 s | **Bookmark** (Mark) | bookmarks an enemy species: +12% damage from everyone to it. Bond tie: new species adds Records | its bookmark body |
| Thimble Moth | Rare Shooter | Archive, Lantern, Runes | 6 s | **Thread Beam** (Beam or channel) | a 2 s thread beam sews through enemies, slowing them. Bond tie: beam leaves a note | its thimble hat and thread |
| Reel Owl | Mythic Shooter | Archive, Lantern, Exhibit Hall | 6 s | **Sample & Remix** (Steal or copy) | records the hero last shot and replays it three times from the owl. Bond tie: replays are archived | its film reel |
| Velvet Champion | Rare Guard | Archive, Lantern, Exhibit Hall | 10 s | **Stand United** (Protection) **R** | tethers to the best damage dealer and takes 40% of the damage it would take for 6 s. Bond tie: tether glows | its cape and medal |
| Teacup Cricket | Uncommon Guard | Lantern, Exhibit Hall, Spiky Shell | 10 s | **Lid Shield** (Protection) **R** | raises its teacup lid: shields the 3 nearest friends for 3 s. Bond tie: lid is a lamp | its teacup shell |
| Lens Owl | Rare Shooter | Archive, Lantern, Runes | 6 s | **Lens Echo** (Clone or echo) | refracts its shot into 3 weaker shots. Bond tie: beams light enemies | its magnifier lens body |
| The Endless Index | Rainbow Shooter | Archive, Runes, Multicaster | 6 s | **Last Page** (Steal or copy) | copies the last skill a friend cast. Bond tie: personal trait: pages recorded | its book wings and pages |
| Harbor Whale | Mythic Shooter | Lantern, Runes, Bludgeoner | 6 s | **Fog Horn** (Weather) | a fog bank rolls in, enemies in it lose 20% damage and are lit. Bond tie: beam sweeps the fog | its lighthouse and bell |
| Pinned Beetle | Common Shooter | Archive, Exhibit Hall, Multicaster | 6 s | **Pinned Down** (Trap or mine) | scatters pins: every enemy walking over them is rooted for 1 s. Bond tie: pinned species are recorded | its horn and pinned tag |
| Paper Inventor | Mythic Shooter | Archive, Runes, Sniper | 6 s | **Fold Wall** (Protection) **R** | folds a paper wall that blocks bullets for 5 s and shields friends behind it. Bond tie: blueprints are archived | its folded paper body |
| Lamp Goat | Uncommon Striker | Lantern, Exhibit Hall, Bladenado | 7 s | **Lamp Oil** (Time) | its lamp burns brighter: friends in light fill skill bars 15% faster. Bond tie: light counts for Lantern | its hanging lantern |

### House: Stone + Clockwork

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Iron Locomotive | Mythic Controller | Clockwork, Stone, Horns | 9 s | **Full Steam** (Dash or leap) | charges in a straight line across the arena, bowling enemies over. Bond tie: rails glow | its steam engine |
| Fossil Armadillo | Uncommon Controller | Stone, Clockwork, Horns | 9 s | **Stone Wall** (Protection) **R** | a line of stone blocks bullets and bodies for 6 s. Bond tie: wall is stone | its fossil shell |
| Tin Tick | Common Controller | Clockwork, Stone, Gears | 9 s | **Countdown** (Time) | starts a timer that speeds up; at zero a burst. Bond tie: ticks add Clockwork | its tin tick body and key |
| Spring Duck | Rare Guard | Clockwork, Gears, Horns | 10 s | **Bounce Pad** (Pull or push) | a spring pad launches enemies upward and sideways. Bond tie: pad is a gear | its coil spring |
| Staple Mole | Common Support | Stone, Clockwork, Gears | 8 s | **Burrow Rescue** (Protection) **R** | burrows to a fallen friend and carries it back: it rises at once with 20% health (once a wave). Bond tie: dust slows enemies | its staple teeth |
| Dream Compass | Common Shooter | Clockwork, Horns, Rapidfire | 6 s | **Treasure Map** (Economy) **R** | points to hidden coins: coins equal to the wave number (max 20) drop around the hero. Bond tie: needle counts as a tick | its brass compass needle |
| The Last Bell | Rainbow Shooter | Stone, Clockwork, Bludgeoner | 6 s | **Toll** (Eruption) | each cast changes its note; the third note explodes in a ring. Bond tie: every third toll adds a tick | its bronze bell and crystal heart |
| Clockwork Piper | Mythic Summoner | Clockwork, Gears, Multicaster | 12 s | **Tune the Clocks** (Team buff) | plays a tune: all skill bars +20% fill speed for 6 s. Bond tie: personal trait: tunes stack | its pipe flute and gear crown |
| Bristle Ball | Uncommon Guard | Stone, Horns, Monolith | 10 s | **Burr Defense** (Protection) **R** | curls up: taunts for 3 s taking 50% less damage and reflecting 30%, then leaps on the largest group. Bond tie: stone plates | its burr shell |
| Monster Marble | Uncommon Guard | Stone, Horns, Unstoppable | 10 s | **Gravity** (Pull or push) | pulls all nearby enemies into a clump for 1.5 s. Bond tie: clumped enemies take stone | its galaxy swirls |
| Top Beetle | Rare Shooter | Stone, Clockwork, Gears | 6 s | **Spinning Top** (Chain) | bounces between enemies, spinning faster each time. Bond tie: each bounce adds a tick | its top body |
| Stitched Pangolin | Uncommon Guard | Stone, Horns, Monolith | 10 s | **Scale Armor** (Protection) **R** | its scales fly to the 5 nearest friends and shield them for 10% max health. Bond tie: scales are stone | its stitched scales |

### House: Bulwark + Spirit

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Tower Rook | Rare Guard | Bulwark, Spirit, Shield | 10 s | **Castle Wall** (Protection) | raises stone wall segments in front of the 4 friends nearest to enemies; they block bullets for 6 s. Bond tie: wall counts as a Bastion wall | a chess-rook tower with a face and tiny arms |
| Lifebuoy Hippo | Uncommon Guard | Bulwark, Spirit, Mend | 10 s | **Lifeline** (Protection) | throws its lifebuoy to the lowest-health friend: a shield that pulls it to safety; a fallen friend in reach rises 7 s sooner. Bond tie: the revive feeds Souls | a round hippo wearing a lifebuoy |
| Nurse Hen | Uncommon Support | Bulwark, Spirit, Mend | 8 s | **Ward Round** (Protection) | heals the 3 lowest friends for 20% and cleanses them; a fallen friend nearby rises 5 s sooner. Bond tie: healing adds Guard | a hen with a nurse cap and a red-cross satchel |
| Phoenix Chick | Mythic Shooter | Bulwark, Spirit, Mend | 6 s | **Rebirth Spark** (Protection) | fires sparks; when a friend falls a feather drops, and picking it up revives that friend at once with 30% health (once a wave). Bond tie: trait: Second Wind (see traits) | a downy phoenix chick with ember tail feathers |
| Dandelion Puff | Rare Support | Bulwark, Spirit, Mend | 8 s | **Wish Seeds** (Protection) | seeds drift onto 4 friends and heal 10% over 5 s; a seed on a fallen friend shortens its revive by 5 s. Bond tie: seeds become Spirits when they land on fallen friends | a dandelion puff with a tiny face |
| Totem Pole | Rare Support | Bulwark, Spirit, Shield | 8 s | **Totem Aura** (Protection) | plants a totem: friends within 5 units take 20% less damage for 6 s; the totem also draws 30% of enemy shots. Bond tie: the totem counts as a Bastion wall | a small carved totem pole with three stacked faces |
| Brass Lion | Uncommon Guard | Bulwark, Shield, Bastion | 10 s | **Roar Ring** (Protection) | a roar pushes everything in a cone and gives the 5 nearest friends 20% damage reduction for 5 s. Bond tie: reduction counts as damage shared | a brass door-knocker lion with a ring through its nose |
| Quilt Walrus | Common Guard | Bulwark, Shield, Monolith | 10 s | **Quilt Blanket** (Protection) | drapes a quilt: the 5 nearest friends gain a shield worth 15% of their max health for 6 s. Bond tie: shield renews Wave shields | a quilted walrus |
| Plaster Panda | Common Support | Bulwark, Mend, Bastion | 8 s | **Bandage Burst** (Protection) | bandages burst over an area and heal every friend inside 12% of max health. Bond tie: healing feeds Mend | a panda covered in bandages and plasters |
| Pumpkin Reaper | Uncommon Striker | Spirit, Shield, Unstoppable | 7 s | **Harvest Sweep** (Zone) | a scythe sweep around it hits everything within 3 units; every enemy slain pops a pumpkin that heals friends 2%. Bond tie: kills gather Souls | a pumpkin-headed reaper with a small scythe |
| Bone Xylophone | Common Shooter | Spirit, Shield, Unstoppable | 6 s | **Rattle Barrage** (Projectile pattern) | bone mallets bounce through the cluster; each bone that kills leaves a Skull that taunts for 3 s. Bond tie: skulls are Spirits | a walking skeleton xylophone |
| Scarecrow Crow | Uncommon Striker | Spirit, Shield, Unstoppable | 7 s | **Murder Dive** (Dash or leap) | a flock dives along a line: enemies hit flee for 1.5 s and each hit leaves a feather that boosts friends 3% damage. Bond tie: feathers drop when friends fall | a crow perched on a scarecrow pole |

### House: Melody + Gravity

| Companion | Rarity / role | Bonds | Bar | Skill | What it does | Comes from |
|---|---|---|---|---|---|---|
| Metronome | Rare Support | Melody, Gravity, Echo | 8 s | **Tempo** (Team buff) | sets a beat: friends' attacks on the beat deal +20% for 6 s. Bond tie: every beat adds to Beats | a pendulum metronome with legs |
| Harp Swan | Uncommon Support | Melody, Gravity, Echo | 8 s | **Soothing Strings** (Protection) | heals friends in a ring 8% a second for 4 s; enemies inside deal 15% less damage. Bond tie: melodies stack Beats | a swan whose wing is a harp |
| Bagpipe Octopus | Uncommon Controller | Melody, Gravity, Echo | 9 s | **Skirl** (Enemy debuff) | a wail stuns every enemy in a wide ring for 1 s and lowers their damage 10% for 5 s. Bond tie: wails fill Beats | an octopus playing a bagpipe with its arms |
| Anvil Ox | Uncommon Guard | Melody, Gravity, Singularity | 10 s | **Anvil Drop** (Protection) | drops an anvil on the densest cluster (stun) and gives friends nearby 25% damage reduction for 4 s. Bond tie: the drop makes a Singularity pull | an ox with an anvil on its back |
| Sinkhole Worm | Common Controller | Melody, Gravity, Singularity | 9 s | **Sinkhole** (Pull or push) | opens a hole that drags enemies in and slows them 40% for 3 s. Bond tie: a hole is a Singularity | a long friendly worm with a gaping mouth |
| Planet Hamster | Mythic Shooter | Melody, Gravity, Singularity | 6 s | **Orbit Rings** (Orbit) | rings of light orbit the hamster and pull in enemy bullets for 6 s, bursting when full. Bond tie: trait: Gravity Well (see traits) | a hamster with a small ringed planet on its back |
| Cymbal Monkey | Common Striker | Melody, Echo, Bladenado | 7 s | **Crash** (Eruption) | a clap stuns every enemy in a ring for 0.5 s and counts as a Beat. Bond tie: crash is a Beat | a wind-up monkey clapping two cymbals |
| Choir Canary | Common Shooter | Melody, Echo, Multicaster | 6 s | **Chorus Call** (Clone or echo) | calls two echoes that repeat the nearest friend's last two attacks. Bond tie: echo feeds Beats | a small yellow canary in a choir robe |
| Piano Crocodile | Rare Controller | Melody, Echo, Multicaster | 9 s | **Chord Slam** (Eruption) | slams a keyboard line: waves of damage travel along the line, one per second. Bond tie: chords are Beats | a crocodile whose jaws are piano keys |
| Newton Apple | Common Shooter | Gravity, Singularity, Demolitionist | 6 s | **Falling Fruit** (Eruption) | apples fall on clusters and split into smaller apples that roll. Bond tie: apples are small masses | a red apple with a small leaf hat |
| Bowling Sloth | Uncommon Guard | Gravity, Singularity, Spiky Shell | 10 s | **Strike** (Dash or leap) | rolls through a line, bowling enemies over, and shields the friends it passes (6%). Bond tie: bowling counts Mass | a sloth curled into a bowling ball |
| Satellite Bee | Rare Shooter | Gravity, Echo, Rapidfire | 6 s | **Satellite Strike** (Eruption) | marks a ground zone (solid filled) and a laser sweeps through it. Bond tie: the zone draws enemies together | a bee with satellite dishes on its wings |

## 7. Protection skills (27)

Because friends can fall, these keep them up. Shield, damage sharing, taunt with reduction, walls, area heals, faster revive.

| Companion | Skill | Protection it gives |
|---|---|---|
| Bread Crab | Toast Shell | shells up: taunts nearby enemies for 3 s while taking 40% less damage, then a crumb pulse |
| Fridge Wisp | Ice Box | opens its door and chills 3 friends: each gets a shield worth 12% max health and frost armor for 5 s |
| Coral Teacup | Tea Time | brews a cup: heals all friends in range 8% and the next reroll costs 1 less |
| Velvet Champion | Stand United | tethers to the best damage dealer and takes 40% of the damage it would take for 6 s |
| Teacup Cricket | Lid Shield | raises its teacup lid: shields the 3 nearest friends for 3 s |
| Spindle Wren | Weave Wind | weaves a thread shield around the 4 nearest friends (8% max health) and +10% projectile speed for 6 s |
| Fossil Armadillo | Stone Wall | a line of stone blocks bullets and bodies for 6 s |
| Staple Mole | Burrow Rescue | burrows to a fallen friend and carries it back: it rises at once with 20% health (once a wave) |
| Mirror Newt | Hall of Mirrors | mirror decoys absorb the next 6 hits aimed at friends and send them back |
| The Forgotten Patchwork | Patch Up | sews wounds: heals the 3 lowest-health friends 15% and mends a fallen one 6 s sooner |
| Paper Inventor | Fold Wall | folds a paper wall that blocks bullets for 5 s and shields friends behind it |
| Violet Remnant | Haunt | taunts nearby enemies and takes 50% less damage for 3 s, draining them while it lasts |
| Bristle Ball | Burr Defense | curls up: taunts for 3 s taking 50% less damage and reflecting 30%, then leaps on the largest group |
| Little Sprout | Moonbloom | a bloom lights a circle: friends inside gain +10% crit and a 10% shield for 6 s |
| Stitched Pangolin | Scale Armor | its scales fly to the 5 nearest friends and shield them for 10% max health |
| Saddle Moth | Rescue Swap | swaps places with the lowest-health friend, taking its place and shielding it for 8% |
| Tower Rook | Castle Wall | raises stone wall segments in front of the 4 friends nearest to enemies; they block bullets for 6 s |
| Lifebuoy Hippo | Lifeline | throws its lifebuoy to the lowest-health friend: a shield that pulls it to safety; a fallen friend in reach rises 7 s sooner |
| Nurse Hen | Ward Round | heals the 3 lowest friends for 20% and cleanses them; a fallen friend nearby rises 5 s sooner |
| Phoenix Chick | Rebirth Spark | fires sparks; when a friend falls a feather drops, and picking it up revives that friend at once with 30% health (once a wave) |
| Dandelion Puff | Wish Seeds | seeds drift onto 4 friends and heal 10% over 5 s; a seed on a fallen friend shortens its revive by 5 s |
| Totem Pole | Totem Aura | plants a totem: friends within 5 units take 20% less damage for 6 s; the totem also draws 30% of enemy shots |
| Brass Lion | Roar Ring | a roar pushes everything in a cone and gives the 5 nearest friends 20% damage reduction for 5 s |
| Quilt Walrus | Quilt Blanket | drapes a quilt: the 5 nearest friends gain a shield worth 15% of their max health for 6 s |
| Plaster Panda | Bandage Burst | bandages burst over an area and heal every friend inside 12% of max health |
| Harp Swan | Soothing Strings | heals friends in a ring 8% a second for 4 s; enemies inside deal 15% less damage |
| Anvil Ox | Anvil Drop | drops an anvil on the densest cluster (stun) and gives friends nearby 25% damage reduction for 4 s |

## 8. Heroes (36)

The hero joins its two minor bonds as a member (a 7-member minor reaches step 6 with 5 companions + the hero). Each hero has a mechanic, three build directions and four hero-only relics. The first ten heroes keep their current mechanic; 26 heroes get a new one.

### Lantern Knight

- **Weapon:** KeyCleaver (melee). **Mechanic:** LANTERN STRIDE: travel 12 units to charge a melee pulse within 2.4 units; deals 65% weapon damage, minimum once every 4s. Holds one pulse until an enemy is nearby. No charge in shops or while paused.
- **Minor bonds (hero counts as a member):** Gears, Bastion. **Starting friend:** Kettle Bat.
- **Build directions:**
  - A: Lantern + Hearth: a lamplit camp, stand still and regenerate
  - B: Clockwork + Stone: tempo and a stone wall
  - C: Lantern + Archive: marked and lit targets
- **Relics:** Wick Trimmer: the Stride pulse is 30% wider; Oil Flask: a charged pulse leaves a lamp for 4 s; Night Watch Coat: +3 armor while a pulse is held; Beacon Ring: friends in the pulse gain +10% damage for 4 s

### Postmaster

- **Weapon:** MailboxLauncher (projectile). **Mechanic:** PARCEL EXPRESS: collect 6 gold to charge an automatic parcel volley; deals 100% weapon damage, minimum once every 4s. Passive gold and income do not count.
- **Minor bonds (hero counts as a member):** Paper Wings, Sniper. **Starting friend:** Bookmark Golem.
- **Build directions:**
  - A: Archive + Lantern: recorded species pay parcels
  - B: Storm + Windborne: a volley of parcels
  - C: Garden + Venom: poisoned deliveries
- **Relics:** Express Stamp: parcels need 5 gold instead of 6; Registered Mail: a parcel marks the species it hits; Return to Sender: parcels that miss come back; Dead Letter Box: parcels leave a poison cloud

### Drowsy Seer

- **Weapon:** MoonseedOrb (projectile). **Mechanic:** DREAM BREATH: stand still for 2 seconds to chill enemies within 3 units and remove up to 3 nearby hostile bullets; once every 7s. Waits for nearby danger. Moving interrupts charge; grants no invulnerability.
- **Minor bonds (hero counts as a member):** Hats, Cloud. **Starting friend:** Candle Snail.
- **Build directions:**
  - A: Starlight + Dusk: sleeping marked enemies
  - B: Frost + Tide: a chilled, soaked dream
  - C: Bulwark + Spirit: dream guardians
- **Relics:** Pillow Talk: Dream Breath lasts 2 s longer; Night Cap: removing bullets heals the hero 2%; Lullaby Loop: breath sleeps enemies 1 s; Dream Catcher: removed bullets return as friendly shots

### The Root Oracle

- **Weapon:** BambooVase (projectile). **Mechanic:** Stationary in combat. Whole-arena pickup and a slowing ward. Walks in shops.
- **Minor bonds (hero counts as a member):** Root, Monolith. **Starting friend:** Mushroom Librarian.
- **Build directions:**
  - A: Garden + Venom: a poison grove
  - B: Frost + Tide: a winter ward
  - C: Stone + Clockwork: an unmovable fortress
- **Relics:** Deep Roots: the slowing ward is 25% wider; Root Cellar: pickups heal the hero 1%; Heartwood Charm: +15% max health while stationary; Sap Flask: the ward leaves sticky patches

### The Lone Jelly

- **Weapon:** InkyOctopus (projectile). **Mechanic:** Travels without roster friends; absorbs purchased friends' traits. Strong attacks and natural recovery support relic-focused builds.
- **Minor bonds (hero counts as a member):** Clover, Multicaster. **Starting friend:** Jelly Swordsman.
- **Build directions:**
  - A: Carnival + Court: borrowed traits and a crown
  - B: Tide + Frost: soaked and chilled
  - C: Spirit + Bulwark: absorbed spirits
- **Relics:** Sponge Tendril: absorbed traits are 20% stronger; Jelly Crown: +1 trait absorbed from the first friend; Slow Drift: natural recovery +50%; Glow Pearl: kills drop a pearl of a random bond

### Lady Loaded Dice

- **Weapon:** GlitchToybox (projectile). **Mechanic:** Collect 6 gold to burst nearby enemies. High luck and critical chance; low health.
- **Minor bonds (hero counts as a member):** Clover, Desperado. **Starting friend:** Dice Crab.
- **Build directions:**
  - A: Carnival + Court: a rigged table
  - B: Starlight + Dusk: lucky critical hits
  - C: Garden + Venom: loaded dice with poison
- **Relics:** Loaded Die: the burst triggers at 5 gold; Snake Eyes: a double 1 refunds the burst; Pocket Pips: +2% crit per gold collected this wave; House Edge: bursts deal +20% to groups of 10 or more

### The Tide Singer

- **Weapon:** RibbonHarp (projectile). **Mechanic:** Collect 8 gold to heal 3 health and push nearby enemies back.
- **Minor bonds (hero counts as a member):** Sea Shanty, Mend. **Starting friend:** Anchor Snail.
- **Build directions:**
  - A: Tide + Frost: a healing, slowing sea
  - B: Starlight + Dusk: tidal wishes
  - C: Melody + Gravity: a song that pulls
- **Relics:** Conch Horn: the push is 30% stronger; Salt Cup: the heal also reaches friends; Pearl Necklace: every 3rd gold gives a bonus pearl; Shanty Book: Sea Shanty needs one member fewer

### The Comet Courier

- **Weapon:** ClockCrescent (melee). **Mechanic:** Travel 10 units to release a slowing pulse. Moving charges it; shops do not.
- **Minor bonds (hero counts as a member):** Quicksoles, Rapidfire. **Starting friend:** Comet Kite.
- **Build directions:**
  - A: Starlight + Dusk: a trail of stars
  - B: Storm + Windborne: a lightning tail
  - C: Clockwork + Stone: runs that wind the clocks
- **Relics:** Tail Sparks: the slowing pulse leaves a star; Spare Wheels: moving 10% faster in the first 10 s; Cinder Trail: the pulse burns for 3 s; Horizon Map: the pulse charges 20% faster

### The Mirror Keeper

- **Weapon:** SnowGlobeOrb (projectile). **Mechanic:** Stand still for 2 seconds to reflect up to 3 nearby bullets. Moving resets charge.
- **Minor bonds (hero counts as a member):** Gauntlet, Spiky Shell. **Starting friend:** Violet Remnant.
- **Build directions:**
  - A: Frost + Tide: reflecting cold
  - B: Carnival + Court: reflections with applause
  - C: Bulwark + Spirit: a mirror shield
- **Relics:** Silver Backing: reflects 4 bullets instead of 3; Hand Mirror: reflected bullets crit; Prism Shard: reflected bullets split in two; Cracked Frame: reflect heals the hero 1%

### The Treasure Clerk

- **Weapon:** AcornSling (projectile). **Mechanic:** Every 10 collected gold grants 1 extra gold and a small healing pulse.
- **Minor bonds (hero counts as a member):** Clover, Bludgeoner. **Starting friend:** Brass Acorn.
- **Build directions:**
  - A: Garden + Venom: a fruitful tree
  - B: Archive + Lantern: ledger of finds
  - C: Starlight + Dusk: a lucky clerk
- **Relics:** Gold Stamp: every 8th gold gives an extra gold; Pouch Lining: the pulse heals 2% more; Stock Ticker: +1% damage per 10 gold held (max 15%); Acorn Cap: chests open one tier higher

### Verdant Matron

- **Weapon:** LeafCrossbow (projectile). **Mechanic (new):** SEED BASKET: every 8 coins collected plants a pod that grows into a Sprout ally for the wave (up to 3 alive)..
- **Minor bonds (hero counts as a member):** Root, Bastion. **Starting friend:** Little Sprout.
- **Build directions:**
  - A: Garden + Venom: a bitter grove
  - B: Hearth + Ember: a feast garden
  - C: Bulwark + Spirit: protective vines
- **Relics:** Sun Hat: Sprouts last the whole run; Watering Can: Sprouts heal friends 3% a second; Bramble Gloves: Sprouts have thorns; Pollen Pouch: a 4th Sprout may live

### Copper Cauldron

- **Weapon:** FireExtinguisher (projectile). **Mechanic (new):** SLOW BOIL: standing near 10 or more enemies for 3 s starts a boil: for 5 s the hero's shots are 30% faster and burn..
- **Minor bonds (hero counts as a member):** Sweet Tooth, Spiky Shell. **Starting friend:** Teacup Cricket.
- **Build directions:**
  - A: Hearth + Ember: a roaring stew
  - B: Carnival + Court: a bubbling show
  - C: Stone + Clockwork: a pressure cooker
- **Relics:** Lid Rattle: the boil starts at 8 enemies; Wooden Ladle: the boil also heals the hero 1% a second; Chilli Pot: burn stacks one more; Soup Ticket: kills during the boil drop a coin

### China Marionette

- **Weapon:** NeedleSinger (projectile). **Mechanic (new):** STRINGS: friends within 4 units of the hero are tied to the hero and take 20% less damage; the hero takes a part of it (up to 10% max health)..
- **Minor bonds (hero counts as a member):** Masks, Shield. **Starting friend:** Jelly Swordsman.
- **Build directions:**
  - A: Frost + Tide: porcelain ice
  - B: Carnival + Court: a puppet show
  - C: Bulwark + Spirit: a guarded stage
- **Relics:** Silk Thread: the radius is 5 units; Porcelain Powder: the hero takes 5% less of the shared damage; Stage Lights: tied friends gain +10% crit; Spare Doll: a fallen friend rises 3 s sooner

### Soap Skater

- **Weapon:** BubbleWand (projectile). **Mechanic (new):** SLIDE: moving builds Lather; at full Lather the next shot leaves a slippery trail that slows enemies 40% for 3 s..
- **Minor bonds (hero counts as a member):** Quicksoles, Rapidfire. **Starting friend:** Sail Squid.
- **Build directions:**
  - A: Frost + Tide: a rink
  - B: Garden + Venom: a slippery grove
  - C: Stone + Clockwork: clockwork skates
- **Relics:** Bubble Rink: the trail is 50% longer; Roller Wheels: Lather fills 25% faster; Foam Cannon: the trail blinds enemies 1 s; Rinse Cycle: the trail cleans friends of debuffs

### Velvet Regent

- **Weapon:** StarforgeClaymore (melee). **Mechanic (new):** ROYAL GUARD: when a friend falls the hero gains +25% damage for 8 s and the friend rises 5 s sooner..
- **Minor bonds (hero counts as a member):** Hats, Bastion. **Starting friend:** Button Beetle.
- **Build directions:**
  - A: Archive + Lantern: a recorded reign
  - B: Clockwork + Stone: a standing court
  - C: Carnival + Court: a crowned show
- **Relics:** Ermine Collar: +30% damage during Royal Guard; Scepter Tip: Royal Guard lasts 4 s longer; Page Boy: the friend rises 3 s sooner still; Regent's Seal: a revive also gives a 20% shield

### Button Cavalier

- **Weapon:** CorkscrewRapier (melee). **Mechanic (new):** CHARGE: every 6 s the hero dashes toward the largest cluster, dealing 100% weapon damage along the path..
- **Minor bonds (hero counts as a member):** Gears, Unstoppable. **Starting friend:** Tin Tick.
- **Build directions:**
  - A: Carnival + Court: a jousting show
  - B: Clockwork + Stone: an armoured charge
  - C: Bulwark + Spirit: a guardian knight
- **Relics:** Lance Tip: Charge hits twice; Spare Button: Charge every 5 s; Plume: Charge gives +20% speed for 3 s; Horse Blanket: Charge shields the hero 10%

### Night Maw

- **Weapon:** DrumstickClub (melee). **Mechanic (new):** DEVOUR: kills heal 1% max health; every 20 kills grows the Maw +5% size and damage until the end of the wave..
- **Minor bonds (hero counts as a member):** Gauntlet, Masks. **Starting friend:** Mask Pepper.
- **Build directions:**
  - A: Carnival + Court: a hungry mascot
  - B: Starlight + Dusk: devouring shadows
  - C: Storm + Windborne: a howling maw
- **Relics:** Sharp Stitch: Devour heals 1.5%; Second Jaw: the Maw grows every 15 kills; Red Scarf: +10% damage when above 70% health; Bone Bell: elites count as 5 kills

### Thorn Treasurer

- **Weapon:** GumballGatling (projectile). **Mechanic (new):** THORN PURSE: every 12 coins collected fires a ring of thorns that poisons what it hits..
- **Minor bonds (hero counts as a member):** Horns, Bludgeoner. **Starting friend:** Seed Hedgehog.
- **Build directions:**
  - A: Garden + Venom: a thorny fortune
  - B: Archive + Lantern: an inventory of thorns
  - C: Stone + Clockwork: a bank of stones
- **Relics:** Rose Clasp: thorns every 10 coins; Thorn Ledger: thorns crit; Gilt Prick: thorns also stun 0.3 s; Coin Spike: every 5th thorn drops a coin

### Citrus Marshal

- **Weapon:** SignpostSlammer (melee). **Mechanic (new):** SIGNPOST: plants a signpost every 10 s that enemies must walk around; the hero gains +3 armor beside it..
- **Minor bonds (hero counts as a member):** Quicksoles, Monolith. **Starting friend:** Citrus Toad.
- **Build directions:**
  - A: Garden + Venom: a sour grove
  - B: Hearth + Ember: a roadside kitchen
  - C: Stone + Clockwork: a fortified road
- **Relics:** Hard Hat: signposts last 6 s longer; Reflective Paint: signposts reflect bullets; Directions: signposts push enemies away; Wayfinder Map: two signposts at once

### Ivory Sovereign

- **Weapon:** DreamStamp (melee). **Mechanic (new):** DECREE: at the wave start choose 1 of 3 decrees that last the wave (more shop slots, +damage, or fewer enemies)..
- **Minor bonds (hero counts as a member):** Hats, Cloud. **Starting friend:** Ice Cream Owl.
- **Build directions:**
  - A: Archive + Lantern: written law
  - B: Frost + Tide: a cold court
  - C: Starlight + Dusk: fate and judgment
- **Relics:** Gavel: a 4th decree option; Royal Seal: a decree may last two waves; Scribe: the decree text also marks a species; Clemency: once a run a decree may be re-chosen

### Petal Citadel

- **Weapon:** RoseBouquet (projectile). **Mechanic (new):** BLOOM WALL: standing still grows petal walls that block enemy bullets; moving fades them..
- **Minor bonds (hero counts as a member):** Root, Bastion. **Starting friend:** Pear Moth.
- **Build directions:**
  - A: Garden + Venom: a poisoned bloom
  - B: Starlight + Dusk: night bloom
  - C: Storm + Windborne: petals on the wind
- **Relics:** Rose Window: walls reflect bullets; Thorn Trellis: walls hurt melee enemies; Stained Glass: walls grow 25% faster; Gate Key: a wall also shields friends

### Patchwork Host

- **Weapon:** ConfettiBlunderbuss (projectile). **Mechanic (new):** PATCHWORK: the hero copies the last skill any friend cast, at half strength..
- **Minor bonds (hero counts as a member):** Big Top, Multicaster. **Starting friend:** Sock Cyclops.
- **Build directions:**
  - A: Carnival + Court: a mixed show
  - B: Hearth + Ember: leftovers and fire
  - C: Archive + Lantern: copied pages
- **Relics:** Spare Patch: copies are at 60%; Needle Case: copies cost no bar; Quilting Frame: copies of heals reach all friends; Mending Thread: a copy of a shield stays 3 s longer

### Archive Curator

- **Weapon:** SleepCamera (projectile). **Mechanic (new):** CATALOGUE: each new enemy species seen gives +1% damage for the run and a coin..
- **Minor bonds (hero counts as a member):** Runes, Sniper. **Starting friend:** Fridge Wisp.
- **Build directions:**
  - A: Archive + Lantern: a complete collection
  - B: Stone + Clockwork: preserved specimens
  - C: Frost + Tide: pickled specimens
- **Relics:** Magnifier: marked species take +5%; Index Cards: species coins are doubled; Display Case: bosses count as 3 species; Camera Flash: a new species is stunned 1 s

### Penny Broker

- **Weapon:** DetonatorBox (projectile). **Mechanic (new):** HAGGLE: recruits cost 10% less; every purchase gives +3% damage for the next wave..
- **Minor bonds (hero counts as a member):** Gears, Bludgeoner. **Starting friend:** Mailbox Imp.
- **Build directions:**
  - A: Archive + Lantern: appraised goods
  - B: Hearth + Ember: a burning bargain
  - C: Stone + Clockwork: coin presses
- **Relics:** Fine Print: recruits cost 15% less; Receipt Roll: purchases stack to +9%; Pawn Ticket: a sold friend refunds 100%; Loose Change: kills drop a coin 10% of the time

### Storm Fencer

- **Weapon:** PlasmaTrident (projectile). **Mechanic (new):** RIPOSTE: 3 s after the hero is hit the next shot always crits and chains lightning..
- **Minor bonds (hero counts as a member):** Paper Wings, Desperado. **Starting friend:** Spring Duck.
- **Build directions:**
  - A: Storm + Windborne: a lightning duel
  - B: Clockwork + Stone: a metronome of steel
  - C: Starlight + Dusk: a duellist's fate
- **Relics:** Rapier Wire: Riposte chains to 4; Foil Guard: Riposte also blocks a bullet; Duelling Glove: Riposte after 2 s; Storm Cloak: +10% speed after a Riposte

### Tin Virtuoso

- **Weapon:** MusicBox (projectile). **Mechanic (new):** METRONOME: every 3 s a beat sounds; shots fired on the beat deal +40%..
- **Minor bonds (hero counts as a member):** Gears, Rapidfire. **Starting friend:** Staple Mole.
- **Build directions:**
  - A: Melody + Gravity: a march
  - B: Clockwork + Stone: a music box
  - C: Carnival + Court: a one-man orchestra
- **Relics:** Wind-up Key: beat every 2.5 s; Brass Valve: on-beat shots pierce; Sheet Music: friends also gain 15% on the beat; Tin Whistle: three on-beat shots in a row chain

### Moon Prowler

- **Weapon:** MoonScythe (melee). **Mechanic (new):** PROWL: standing still for 2 s makes the hero unseen; the next hit from stealth deals triple damage..
- **Minor bonds (hero counts as a member):** Masks, Bludgeoner. **Starting friend:** Star Sock.
- **Build directions:**
  - A: Starlight + Dusk: a moonlit ambush
  - B: Frost + Tide: a silent hunt
  - C: Spirit + Bulwark: a shadow guardian
- **Relics:** Velvet Paws: stealth after 1.5 s; Moon Dust: stealth shots mark; Night Collar: +20% crit from stealth; Silver Claw: stealth hits stun 0.5 s

### Supper Mortar

- **Weapon:** TeapotPopgun (projectile). **Mechanic (new):** SUPPER: each food pickup is lobbed as a shell that heals friends in an area and burns enemies..
- **Minor bonds (hero counts as a member):** Sweet Tooth, Mend. **Starting friend:** Bread Crab.
- **Build directions:**
  - A: Hearth + Ember: a banquet
  - B: Tide + Frost: soups and ice
  - C: Garden + Venom: a poisoned table
- **Relics:** Gravy Boat: shells are 30% larger; Dinner Bell: shells land twice; Spice Rack: shells apply burn; Napkin: shells cleanse friends

### Lantern Weaver

- **Weapon:** LanternStaff (projectile). **Mechanic (new):** WEAVE: lines of light join friends; enemies crossing a line take damage and are lit..
- **Minor bonds (hero counts as a member):** Runes, Monolith. **Starting friend:** Thimble Moth.
- **Build directions:**
  - A: Archive + Lantern: a loom of light
  - B: Starlight + Dusk: a net of stars
  - C: Melody + Gravity: strings that pull
- **Relics:** Golden Thread: lines last 50% longer; Shuttle: the hero joins the web; Spool of Dusk: lines slow enemies; Tassel: line damage crits

### Hourglass Stag

- **Weapon:** AlarmMortar (projectile). **Mechanic (new):** HOURGLASS: every 15 s time slows 40% around the hero for 3 s..
- **Minor bonds (hero counts as a member):** Gears, Cloud. **Starting friend:** Dream Compass.
- **Build directions:**
  - A: Clockwork + Stone: slow minutes
  - B: Frost + Tide: a frozen moment
  - C: Starlight + Dusk: a stretched night
- **Relics:** Fine Sand: every 12 s; Brass Rim: slow 55%; Glass Bulb: friends cast 20% faster inside; Antler Bell: the end of the slow stuns 0.5 s

### Rain Archivist

- **Weapon:** SquirtTrio (projectile). **Mechanic (new):** DOWNPOUR: rain falls over the arena each wave: it soaks enemies, and friends in the rain gain +10% speed..
- **Minor bonds (hero counts as a member):** Rain Script, Mend. **Starting friend:** Ribbon Otter.
- **Build directions:**
  - A: Tide + Frost: a flood and ice
  - B: Storm + Windborne: a thunder shower
  - C: Garden + Venom: a nourished grove
- **Relics:** Rain Gauge: rain starts 5 s sooner; Scroll Case: rain also clears debuffs; Wet Ink: soaked enemies take +5%; Umbrella Clasp: rain heals friends 1% a second

### Kite Admiral

- **Weapon:** KiteSpool (projectile). **Mechanic (new):** FLEET: every 8 s a kite fleet sails along a flight line hitting everything in it..
- **Minor bonds (hero counts as a member):** Paper Wings, Sniper. **Starting friend:** Paper Plane Pigeon.
- **Build directions:**
  - A: Storm + Windborne: a squall line
  - B: Starlight + Dusk: a comet fleet
  - C: Melody + Gravity: a sky orchestra
- **Relics:** Spyglass: the fleet is 30% longer; Flagship: the fleet carries a bomb; Rigging: the fleet every 6 s; Weather Chart: the fleet follows wind lanes

### Sundae Duchess

- **Weapon:** ScoopLauncher (projectile). **Mechanic (new):** SUNDAE: every 10 s she throws a scoop that freezes enemies and heals nearby friends 10%..
- **Minor bonds (hero counts as a member):** Sweet Tooth, Mend. **Starting friend:** Baker Mouse.
- **Build directions:**
  - A: Hearth + Ember: hot fudge
  - B: Frost + Tide: a double scoop
  - C: Bulwark + Spirit: a sweet shield
- **Relics:** Waffle Cone: the scoop is 30% larger; Cherry Bomb: the scoop explodes at the end; Sprinkles: the scoop crits; Extra Whip: the heal is 15%

### Bonsai Clocksmith

- **Weapon:** CuckooCannon (projectile). **Mechanic (new):** PRUNING: every wave survived adds +2% skill speed for the run; once a wave trim a friend's bar to refill it at once..
- **Minor bonds (hero counts as a member):** Gears, Multicaster. **Starting friend:** Top Beetle.
- **Build directions:**
  - A: Clockwork + Stone: a slow-growing machine
  - B: Garden + Venom: a fruiting clock
  - C: Archive + Lantern: a recorded tree
- **Relics:** Fine Shears: +3% a wave; Cuckoo Chime: trim refills two bars; Grafting Knife: trimmed friends gain +10% damage 5 s; Moss Gear: skill speed also grows between waves

### Juggling Jester

- **Weapon:** JugglingPins (projectile). **Mechanic (new):** JUGGLE: keeps up to 3 projectiles in the air; they bounce between enemies and return to the hero..
- **Minor bonds (hero counts as a member):** Big Top, Rapidfire. **Starting friend:** Lens Owl.
- **Build directions:**
  - A: Carnival + Court: the main act
  - B: Archive + Lantern: a lit stage
  - C: Melody + Gravity: a juggling rhythm
- **Relics:** Fourth Pin: 4 projectiles; Chalk Hands: pins crit; Bell Cap: pins stun on the 5th bounce; Acrobat Mat: pins heal the hero 1%

### Bell Diver

- **Weapon:** SonarHarpoon (projectile). **Mechanic (new):** DIVE: every 10 s the hero dives (untargetable for 2 s) and surfaces with a sonar pulse that marks enemies..
- **Minor bonds (hero counts as a member):** Rain Script, Bastion. **Starting friend:** Raincoat Frog.
- **Build directions:**
  - A: Tide + Frost: the deep
  - B: Spirit + Bulwark: a sunken guardian
  - C: Melody + Gravity: sonar and pressure
- **Relics:** Air Tank: untargetable 3 s; Depth Gauge: marked enemies take +8%; Brass Helmet: surfacing shields friends; Sonar Pearl: the pulse pushes enemies

## 9. Enemies, affixes and bosses

Late waves hold 100+ enemies, so enemy skills are built for crowds: auras, packs and area telegraphs (solid filled shapes on the ground). Friends are mortal, so many enemies hunt the weakest friend. Nights are when a kind first appears; kinds stay in later waves.

**Who has skills:** small enemies (most of every wave) have no skill, only a body: speed, health and how they move. Medium enemies are plain bodies with one trait, and only a few of them (8) have a skill. Large and special enemies, elites and bosses carry the skills. On screen at once: at most 2 large or special enemies on early nights, rising to 8 late; small enemies make up at least 80% of a wave.

### Small (no skill)

| Enemy | Nights | Role | Body | Look |
|---|---|---|---|---|
| Crumb Swarm | 1+ | Fodder | comes 30 to 60 at once; each dies in one hit | a crowd of tiny sprites; the giant has a crown of crumbs |
| Marcher | 1+ | Fodder | walks in tight ranks | ranks with matching stripes |
| Pillbug | 1+ | Fodder | rolls fast in straight lines | a rolling stripe on the ground, solid red |
| Flanker | 1+ | Fodder | enters behind the hero | a blue arrow on the ground |
| Decoy | 1+ | Trick | looks like a rare enemy; dies in one hit | a faint wobble |
| Blinker | 1+ | Trick | moves in short hops (already in the game) | a flash on the ground |
| Ambusher | 24+ | Trick | hard to see until 3 units away | a ripple in the ground |
| Phase Walker | 26+ | Hunter | walks through walls and friends | a wavering outline |

### Medium, plain (one body trait, no skill)

| Enemy | Nights | Role | Trait | Look |
|---|---|---|---|---|
| Grower | 1+ | Brute | larger and tougher each wave stage | grows visibly larger |
| Splitter | 1+ | Brute | splits in two when it dies (already in the game) | a seam on its body |
| Shield Wall | 18+ | Brute | immune to hits from the front | a big tower shield |
| Immune | 48+ | Brute | cannot be stunned or slowed | a metallic sheen |
| Thief | 14+ | Trick | grabs dropped coins and runs; killing it returns 110% | a sack on its back |
| Gluttonous Mouth | 30+ | Trick | eats dropped coins and pickups; kill it to get them back | a wide mouth |

### Medium with a skill (8)

| Enemy | Nights | Role | Skill | Look |
|---|---|---|---|---|
| Smasher | 1+ | Brute | a telegraphed slam: area damage and a short stun on friends | a filled red circle for 1 s |
| Sniper Doll | 5+ | Ranged | stands outside the arena and shoots the lowest-health friend; a thin line shows its target for 1 s first | a red laser line |
| Bomber | 10+ | Brute | swells, then bursts and leaves a poison pool on friends' ground | a swelling red body |
| Healer | 10+ | Support | heals the weakest enemies nearby for 5% a second | green motes |
| Banner Bearer | 12+ | Support | while alive every enemy within 6 units deals +20% damage; the banner drops on death and can be picked up by friends | a waving banner |
| Egg Layer | 16+ | Support | lays an egg that hatches into 3 small enemies after 5 s unless it is destroyed | a shaking egg |
| Kamikaze Candle | 28+ | Hunter | runs at the largest friend cluster and explodes in a wide area | a lit fuse |
| Charger | 12+ | Hunter | charges the farthest friend in a line that passes through everything; hitting a wall stuns it | a long red strip on the ground |

### Large and special (skills)

| Enemy | Nights | Role | Skill | Look |
|---|---|---|---|---|
| Stitcher | 1+ | Support | when another of its kind falls near it, it heals to full; kill the group together | stitches glow between members |
| Shielder | 1+ | Support | shields the group behind it (already in the game) | a dome over the group |
| Leech | 8+ | Hunter | latches onto a friend and drains it, healing itself; hits on the friend also hit the leech | a thread between the leech and the friend |
| Buffer | 10+ | Support | cheers: every enemy within 8 units gains +30% speed for 4 s | a horn note ring |
| Trapper | 14+ | Trick | lays spikes, cocoons or mines under friends' feet | small marked tiles |
| Marker | 15+ | Support | marks the hero: every enemy turns toward the hero | a target over the hero |
| Cone-brute | 16+ | Brute | its hits splash in a cone behind the target | a cone shape on the ground |
| Magnet Brute | 18+ | Brute | pulls friendly projectiles toward itself and absorbs them (a bullet sponge for projectile builds) | a horseshoe magnet and field lines |
| Mourner | 20+ | Support | when it dies every enemy within 8 units gains +40% speed for 5 s | a black veil |
| Silencer | 22+ | Control | a ring around it pauses the skill bars of friends inside for 3 s | a hushed ring on the ground |
| Time Eater | 22+ | Control | an aura halves the skill-bar fill speed of friends within 6 units | a pale clock face |
| Cleanser | 24+ | Control | removes buffs and shields from friends in an area | white sparks |
| Burrower | 26+ | Hunter | tunnels under the arena and bursts up beneath a friend cluster | a moving mound, then a filled circle |
| Gravity Imp | 28+ | Control | pulls friends together into a clump (bad against area attacks, good for the enemy) | a spiral on the ground |
| Shrieker | 30+ | Control | a scream makes friends flee 1 s and breaks stillness effects | sound rings |
| Rallier | 32+ | Support | when it falls below 50% health every enemy within 8 units gains a 15% shield | a raised fist |
| Mirror Imp | 34+ | Trick | reflects 30% of ranged damage while facing the hero | a mirror shield |
| Reviver | 36+ | Support | revives one fallen enemy within 8 units after 5 s; kill it first | a green cross over bodies |
| Summoner | 40+ | Support | calls up to 6 small helpers every 10 s | a glowing circle |
| Darkener | 40+ | Control | makes a dark zone: friends inside miss 30% of shots | a black cloud |
| Binder | 42+ | Control | chains two enemies: if one falls the other takes 40% of its health (Tethered in the game) | a chain between enemies |
| Drainer | 44+ | Control | drains coins or health of friends inside a zone | a pink spiral |
| Charmer | 46+ | Control | charms one friend for 3 s: it attacks other friends | hearts over the friend |
| Storm-eye | 50+ | Ranged | every 3rd attack chains lightning to 4 more targets | a crackling eye |
| Armour-grower | 55+ | Brute | grows and slams, then has armour for 4 s (Armored in the game) | plates growing on its body |
| Meteor Caller | 60+ | Ranged | calls meteors onto the largest friend cluster (solid filled circles, 1.5 s) | filled red circles |
| Tide Brute | 65+ | Control | floods the area under it: friends inside move 30% slower | a spreading blue puddle |
| Fire Brute | 30+ | Elemental | leaves flames on the ground that burn friends who cross them | embers on the ground |
| Ice Wisp | 30+ | Elemental | chills friends in a ring (-20% speed, stacking) | frosty ring |
| Storm Wisp | 40+ | Elemental | chains lightning between friends in contact with each other | blue arcs |
| Venom Wisp | 40+ | Elemental | poisons friends in a ring; the poison spreads between friends that touch | a green mist |

### Elite affixes (12; an elite gets 1 to 3)

| Affix | Effect |
|---|---|
| Swift | +30% speed |
| Armored | +armor, immune to knockback |
| Vampiric | heals 3% of damage dealt by all nearby enemies |
| Explosive | bursts on death in a filled red circle |
| Shielded | starts with a shield of 30% health |
| Splitting | splits into two on death |
| Burning | leaves burning ground behind |
| Frozen | chills friends it touches |
| Teleporting | blinks to the weakest friend every 6 s |
| Regenerating | heals 2% a second out of combat |
| Commanding | every enemy near it deals +10% |
| Reflective | reflects 20% of damage once a wave |

### Bosses (6, each with phases)

- **Pastry Tyrant (nights 10, 30):** phase 1: smash and crumb swarms; at 60% health it leaves the arena and returns "pumped up" with a rolling pin; at 25% it summons its entire kitchen of Stitchers
- **Clockwork Warden (20, 40):** phase 1: marks the largest friend cluster with a time bubble (skill bars frozen); at 50% it splits into two Wardens linked by a chain; at 20% a final countdown that kills everything in the arena unless the chain is cut
- **Storm Herald (25, 50):** phase 1: lightning rain on filled circles; at 60% it becomes immune for 5 s and sends Storm Wisps; at 25% the whole arena is charged and every hit chains
- **Hollow Choir (35, 60):** phase 1: silences friends in rings; at 50% three singers appear and must be killed together; at 20% a lullaby puts friends to sleep for 2 s
- **Gravity Maw (45, 70):** phase 1: pulls friends toward its mouth; at 50% it swallows friends and spits them back after 3 s; at 20% it collapses into a black hole that pulls the whole arena
- **Lantern Queen (55, 80):** phase 1: dark zones and summons; at 50% all light in the arena goes out except around friends; at 20% she charms the strongest friend for 6 s

## 10. Questions

- Death rule: friends rise after 15 s with 40% health and are full at the next wave. Is losing the wave when all friends are down at once right?
- The 24 new companions have designs (look, skill) but no art yet; approve the species list before drawing.
- Are 53 enemy kinds enough? Which tier is missing something?
- Hero mechanics: do 26 new mechanics seem right, or should some stay plain stat heroes?
