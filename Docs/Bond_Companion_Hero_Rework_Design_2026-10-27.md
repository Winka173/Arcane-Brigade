# Rework: bond, companion, hero (design draft, 27/10/2026)

Status: DRAFT for the owner's approval. Nothing here is implemented yet. Decisions already made by the owner are marked **[decided]**.

## 1. Why
- 96 companions, 33 bonds, almost all bonds are "+x% a stat" at 3/6/9.
- Only 26 companions have a skill of their own; the rest share a few templates ("every 4 s heal 1.5 HP", "every 4 s find 1 gold", "every 4 s chill"). 21 companions carry the catch-all bond Unbroken.
- Heroes do not push the player toward different builds.

## 2. What TFT's last five sets teach (sets 14 to 18; sources in the chat reply)
Bonds that **make things in the arena** (plants, black holes, pixies, hexes), bonds with **their own counter fed by an event** (takedowns, losses, healing), **a choice per run** (constellation, rider), **transformation**, about **one trait in three belongs to a single champion**, and **one set-wide mechanic outside the units** (Wisps, Realm of the Gods, Power Ups).

## 3. Decisions **[decided]**
1. Heroes and companions are reworked together.
2. About 16 bonds, each with its own mechanic at 2 / 4 / 6 (every step adds a new behaviour, not only a bigger number).
3. Every companion keeps its name, art and weapon and gets **its own skill** with a **skill bar that fills with time only**; it casts when full. Fill time depends on the role; bonds and relics can speed it up.
4. Bonds **make objects on the floor** and each bond has **its own counter** fed by an event.
5. About 12 companions are **personal-trait** units (a trait only they have).
6. No formations: companions keep following the hero as now.
7. Each hero has **2 to 4 build directions**, a **companion pool of about 30** (a companion appears for about 8 to 10 heroes), **6 hero-only relics** tied to those directions. The hero card lists the directions as suggestions; they form by buying. Buying and upgrading companions in the shop stays as it is.

## 4. The sixteen bonds (all 33 old bonds are folded in; Ember and Storm added 27/10 so the elements are varied)

| # | Bond | Folds in | Object it makes | Counter (fed by) | 2 | 4 | 6 |
|---|---|---|---|---|---|---|---|
| 1 | Hearth | hearth, sweet-tooth, fruit (warmth, food, healing only; combat fire is Ember) | Campfire | Warmth (healing done) | a campfire every 15 s; standing in it regenerates | Warmth full: a Feast heals everyone and +10% damage for 8 s | campfires scorch enemies; Feast fills twice as fast |
| 2 | Archive | archive, exhibit-hall, rune | Pinned Note on an enemy | Records (species marked) | first hit marks a species: +12% damage to it | each new species recorded: +1% damage for the run | recorded species pay a coin and drop a Page (copies a skill) |
| 3 | Clockwork | clock, gear | Gear Turret | Ticks (skills cast) | skill bars fill 20% faster | every 8 casts a turret rises | 1.5 s time stop every 20 s |
| 4 | Frost | frost | Ice Patch | Chill (enemies slowed) | slowed enemies take +8% | frozen enemies shatter for area damage | a blizzard zone follows the hero |
| 5 | Garden | orchard, root, sprout, clover | Seed Pod (grows each wave) | Growth (waves survived) | a pod each wave | pods flower: gold, heal or thorns | the oldest tree becomes a Guardian that fights |
| 6 | Tide | tidal, sea-shanty, rain-script | Puddle | Soak (enemies wet) | puddles soak enemies | soaked enemies take +10% from every source and slow | tide wave sweeps the screen every 12 s |
| 7 | Starlight | astral, morning | Falling Star zone | Wishes (critical hits) | a star falls where the hero last crit | wishes buy a free reroll at the next shop | a comet shower every 15 s |
| 8 | Carnival | oddity, mask, big-top, ball | Prize Wheel | Applause (different skills cast within 5 s) | a wheel appears; stepping on it gives a random buff | applause fills it: jackpot | the wheel stays and spins on its own |
| 9 | Court | regal, hat, knight | Royal Banner | Rank (stars of the best friend) | banner: friends inside get +10% damage | the best-star friend is crowned (extra skill) | decree: choose one law for the run |
| 10 | Brawl | gauntlet, horn, resolve (split by shape) | Ring (a floor circle) | Grit (damage taken) | melee inside the ring hit +15% | Grit turns into damage dealt | last stand: a brawler survives a fatal hit once a wave |
| 11 | Windborne | archer, paper-wings | Wind Lane | Volleys (shots fired) | shots through a lane fly farther | every 10th volley pierces everything | a gale lane pushes enemies back |
| 12 | Patchwork | hide | Patch (a revive token) | Stitches (friends hurt) | hurt friends are mended slowly | a fallen friend returns once a wave | patches also shield the hero |
| 13 | Springstep | hare, shoe | Spring Pad | Hops (distance run) | +move speed; pads launch the hero | landing damages | a dash leaves a trail |
| 14 | Lantern | lantern-guild | Lamp | Light (enemies in lamplight) | lamps light a zone: +attack range | enemies in light take more | lamps follow the hero |
| 15 | Ember (fire) | new; Mask Pepper, Tea Dragon, Lamp Goat, Coil Kettle, Paper Devil | Ember (coals left on the floor) | Heat (burning enemies) | hits set enemies alight | fire jumps to nearby enemies; coals stay and burn | an inferno at 100 Heat: everything alight takes triple burn |
| 16 | Storm (lightning) | static-club (Static Ferret, Storm Kitten, Volt Eel) | Charged Rod | Charge (hits landed) | every 6th hit chains lightning | rods link and arc between them | a thunderclap stuns all charged enemies |

Numbers are placeholders for balance. Every bond has its own counter name, colour and icon so the HUD shows what each one is doing.

## 5. Skills
- Role sets the bar's fill time: Shooter 6 s, Striker 7 s, Support 8 s, Controller 9 s, Guard 10 s, Summoner 12 s.
- Fill time is the "personality dial": fast and small vs slow and big, within the role.
- Each of the 96 companions gets a **unique** skill built from its shape and name (a Teapot makes steam, a Snail slows and leaves a slime trail, a Moth blinds...). Authoring is done in bond batches of 6 to 12 so the owner can review.
- A skill can also read the bond counter of its bond (for example a Hearth friend casts a bigger heal with more Warmth).

## 6. Personal-trait companions (about 12)
Mostly Mythic and Rainbow. Examples: Clockwork Piper (time tunes), The Endless Index (copies the last skill), The First Seed (thorn garden), Ringmaster Jack (attention), Kite Dragon (wind sweeps), The Lost Carousel (carousel arena). The list is drafted per bond when the bond is authored.

## 7. Heroes: 2 to 4 build directions each (28 playable heroes + 8 coming soon)
A direction = 2 or 3 bonds + 1 hero-only relic line + a hero passive that rewards it. Draft examples:

| Hero | Direction A | Direction B | Direction C |
|---|---|---|---|
| Lantern Knight | Lantern + Hearth (lamplit camp) | Lantern + Clockwork (tempo) | Lantern + Brawl (bright vanguard) |
| Tide Singer | Tide + Starlight (wet comets) | Tide + Windborne (volley storm) | Tide + Hearth (healing sea) |
| Root Oracle | Garden + Hearth (feast garden) | Garden + Patchwork (undying trees) | Garden + Frost (winter grove) |
| Glass Dice | Carnival + Court (rigged table) | Carnival + Starlight (lucky crits) | |

The hero's pool (about 30 companions) is the union of its directions' bonds. Hero-only relics (6 per hero, about 170 in all) each push one direction.

## 8. Reused systems
Shop and merge (stars), `HeroRelicPools`, `K11HeroRules.Eligible`, duets/combos, `CompanionSkills`, the skill-copy text, `MetaProgression`. New: skill bar, floor objects, bond counters, bond variants.

## 9. Order of work (each phase ends with a build the owner can play)
1. Engine: skill bar (time-only), floor-object system, bond counters, HUD for the bars and counters. Test with 3 bonds and 10 companions.
2. The 14 bonds (rules, art for objects in code ink, icons).
3. Companion skills in batches by bond (96 in all), with the 12 personal traits.
4. Hero builds: directions, pools, hero-only relics, hero card, passives.
5. Balance pass, checks, save migration (old saves keep working), capture videos.

## 10. Open questions for the owner
- Art for the 14 bond icons and floor objects: code ink (as all effects are) or painted?
- Should the old duets/combos be folded into bonds or stay as an extra layer?
