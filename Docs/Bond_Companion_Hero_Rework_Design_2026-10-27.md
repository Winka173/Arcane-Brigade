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

## 11. Picks from the idea bank (27/10, owner said "follow the proposal")
Companions are immortal, so no pick relies on a friend falling; "revive" and "redirect" ideas become shields, tethers or hits absorbed by the hero's armour.

| Bond | Picked ideas (bank no.) | What is new about it |
|---|---|---|
| Hearth | 5 Campfire + 12 Jade Statues | camp you place and move; friends near a statue fill skill bars faster |
| Archive | 16 Records + 35 Mirage | every run one random recorded-bonus family (damage, coin, page copy) |
| Clockwork | 29 Echo + 8 Workbench | skills fire twice, weaker; the turret takes relics as slots |
| Frost | 11 Ice Soldiers + 32 Thunderclap | first enemies to die each wave rise as ice allies; a cold storm after 20 s |
| Garden | 6 Garden + 25 Sprouting | front plants root enemies, back plants shoot; friends grow while standing in it |
| Tide | 4 Weather | weather cycles every 20 s; each friend likes one weather |
| Starlight | 3 Constellation + 19 Mode | pick a constellation per run, then a mode at step 1 |
| Carnival | 7 Spotlight + 14 Gambit | moving light; cash out or Double Down every 3 waves |
| Court | 2 Decree | write a law (cause + effect); more steps unlock causes and effects |
| Brawl | 22 Stance + 33 Dawn | duelist/executioner/juggernaut stance; once per wave at half health the hero's brigade leaps back and stuns |
| Windborne | 20 Rider | one big beast; choose how it fights by dragging a friend onto it |
| Patchwork | 21 Mutation + 36 Fed | one chosen mutation per friend; duplicate copies feed stats instead of merging |
| Springstep | 34 Hermit | only active with exactly 1 or 4 different friends of the bond; strong but lopsided |
| Lantern | 27 Eclipse + 28 Doom | after 8 s the weakest enemy falls every 3.5 s; each marked enemy gives 12% strength to your best friend |
| Ember | 15 Souls (enemy kills only) + 9 Void Egg | kills feed Heat; an egg of coals hatches into a burning ally |
| Storm | 17 Underdog + 26 Hatching | luck rises as health falls; after N waves a friend becomes a storm form |

**Run-wide pick:** Power Up (D) and Patron (D) both go in: Power Up twice per run, Patron at waves 3, 6, 9.
**Enemy picks:** all 24 in section C of the bank, in three tiers by night: nights 1-15 (Stitcher, Flanker, Smasher, Grower, Decoy, Shielder, Splitter, Teleporter), 16-40 (Healer, Buffer, Bomber, Thief, Trapper, Marker, Cone-brute, Arriving-brute), 41+ (Summoner, Darkener, Binder, Drainer, Bribe, Immune, Storm-eye, Armour-grower). Bosses combine two or three and change at health thresholds.

Next: write the 96 companion skills in bond batches (start with Hearth, Frost, Ember so the first playable build has three elements).

## 12. Major and minor bonds: how many (27/10)
Assumption: "at most 10 companions" = at most 10 companions in one bond.

- **Major bond** (16, section 4): steps 2/4/6, its own mechanic. 6 to 10 companions each, 7.5 on average.
- **Minor bond** (16 more): steps 2/4 only, one small offence or utility effect, no object on the floor. 4 to 5 companions each. Candidates (old tags): Sweet-tooth, Gear, Rune, Hat, Mask, Horn, Hide, Paper-wings, Sea-shanty, Rain-script, Big-top, Ball, Gauntlet, Shoe, Clover, Root. Offence and utility only (companions are immortal).
- **Total 32 bonds** (old: 33).
- **Companions: keep 96.** Each has one major and one minor bond: 96 x 2 = 192 slots = 16 x 7.5 + 16 x 4.5. The 12 personal-trait companions keep their major bond and add the personal trait instead of a minor one.
- **Hero build** = 1 major + 1 or 2 minors (about 12 to 15 companions); 2 to 4 builds per hero; the pool of about 30 is the union. 36 heroes x 3 builds = about 108 builds over 16 x 16 = 256 possible pairs, so builds stay distinct and every bond serves about 7 builds.

### 12b. Owner change (27/10): major 8, minor 6 or 4
- Major bond: 8 companions each (16 x 8 = 128 slots).
- Minor bond: 8 minors with 6 companions (the broad ones: damage, speed, utility used by many heroes) and 8 minors with 4 (the narrow, sharper ones) = 80 slots.
- Total 208 slots for 96 companions: every companion has one major and one minor (192), and **16 more slots go to a third bond on 16 companions** (Mythic and Rainbow first, then Rare), so no new art is needed and the roster stays at 96. The 12 personal-trait companions keep major + minor + personal trait.
- If the owner prefers a clean two-bonds rule instead, the alternative is 8 new companions (104 x 2 = 208); that needs 8 new paintings.

## 13. Bond roster: which companion is in which bond (27/10, generated by an optimiser from the old tags, shapes, names and roles)
Major bonds: 8 companions each. Minor bonds: 8 with 6 and 8 with 4. 16 companions (Mythic, Rainbow and Rare first) carry a third bond. A companion has 1 or 2 major bonds. Six slots were filled without a thematic reason (marked `?`) and should be reviewed with the skill writing.

### By bond
**Major**

- **Hearth** (8): Candle Snail, Bread Crab, Velvet Enchantress, Teacup Cricket, Baker Mouse, Tea Dragon, Sundae Golem, Bottle Gull
- **Archive** (8): Mailbox Imp, Bookmark Golem, Thimble Moth, The Forgotten Patchwork, Velvet Champion, Lens Owl, The Endless Index, Pinned Beetle
- **Clockwork** (8): Tin Tick, Spring Duck, Staple Mole, The Last Bell, Clockwork Piper, Grand Sprout, Top Beetle, Specimen Slime
- **Frost** (8): Snow Spoon, Ice Cream Owl, The Impossible Tailor, Iron Locomotive, Monster Marble, Shovel Penguin, Boba Tadpole, Fossil Armadillo
- **Garden** (8): Mushroom Librarian, Radish Jester, Seed Hedgehog, Paper Inventor, Grand Sprout, Little Sprout, Watering Snail, The First Seed
- **Tide** (8): Coral Teacup, Bubble Koi, Anchor Snail, Sail Squid, Balloon Mallet, Ribbon Otter, Raincoat Frog, Shanty Sea Lion
- **Starlight** (8): Pillow Oracle, Moon Jelly, Eclipse Egg, Orbit Moth, The Sleeping Friend, Dough Foreman, Little Sprout, Storm Kitten
- **Carnival** (8): Dice Crab, Mask Pepper, Crayon Gremlin, Mirror Newt, Ribbon Spider, Violet Remnant, Ringmaster Jack, The Lost Carousel
- **Court** (8): Brass Acorn, Button Beetle, Eclipse Egg, Orbit Moth, Return Captain, Velvet Enchantress, Carousel Pony, Court Herald
- **Brawl** (8): Sail Squid, Sock Cyclops, Ribbon Spider, The Impossible Tailor, Saffron Mechanist, Velvet Champion, Violet Remnant, Bristle Ball
- **Windborne** (8): Kettle Bat, Comet Kite, Dream Compass, The Folded Courier, Spindle Wren, Yo-yo Squirrel, Paper Plane Pigeon, Kite Dragon
- **Patchwork** (8): Pillow Oracle, Jelly Swordsman, Kettle Bat, Snow Spoon, The Forgotten Patchwork, The Sleeping Friend, Stitched Pangolin, Saddle Moth
- **Springstep** (8): Hood Wisp, Spring Duck, Fridge Wisp, Citrus Toad, Moon Jelly, Star Sock, Dream Compass, Yo-yo Squirrel
- **Lantern** (8): Pear Moth?, Shell Postman, The Last Bell, Teacup Cricket, Lens Owl, Lamp Goat, Harbor Whale, Volt Eel?
- **Ember** (8): Teapot Pilgrim, Candle Snail, Mask Pepper, Coil Kettle, Saffron Mechanist, Paper Devil, Tea Dragon, Lamp Goat
- **Storm** (8): Comet Kite, Coil Kettle, Reel Owl, Rain Heron, Kite Dragon, Static Ferret, Storm Kitten, Volt Eel

**Minor (6)**

- **Sweet-tooth** (6): Candle Snail, Bread Crab, Dough Foreman, Baker Mouse, Sundae Golem, Boba Tadpole
- **Rune** (6): Bookmark Golem, Tin Tick, Ice Cream Owl, Pear Moth, Lens Owl, The Endless Index
- **Hat** (6): Bookmark Golem, Ice Cream Owl, Thimble Moth, Reel Owl, Return Captain, Court Herald
- **Mask** (6): Teapot Pilgrim, Brass Acorn, Tin Tick, Radish Jester, Balloon Mallet, Monster Marble
- **Horn** (6): Clockwork Piper?, Bristle Ball, Paper Devil, Monster Marble, Carousel Pony, Lamp Goat
- **Gauntlet** (6): Mailbox Imp, Jelly Swordsman, Bread Crab, Fridge Wisp, Shovel Penguin, Static Ferret
- **Paper-wings** (6): Comet Kite, Anchor Snail?, The Folded Courier, Spindle Wren, Paper Plane Pigeon, Kite Dragon
- **Shoe** (6): Spring Duck, Fridge Wisp, Citrus Toad, Star Sock, Sock Cyclops, Yo-yo Squirrel

**Minor (4)**

- **Sea-shanty** (4): Harbor Whale, Shanty Sea Lion, Bottle Gull, Saddle Moth?
- **Rain-script** (4): Iron Locomotive?, Ribbon Otter, Rain Heron, Raincoat Frog
- **Big-top** (4): Crayon Gremlin, Mirror Newt, Ringmaster Jack, The Lost Carousel
- **Ball** (4): Mailbox Imp, Shell Postman, Dice Crab, Balloon Mallet
- **Clover** (4): Mushroom Librarian, Staple Mole, Seed Hedgehog, Bubble Koi
- **Root** (4): Hood Wisp, Button Beetle, Watering Snail, The First Seed
- **Gear** (4): Pillow Oracle, Coral Teacup, Paper Inventor, Top Beetle
- **Exhibit-hall** (4): Pinned Beetle, Fossil Armadillo, Specimen Slime, Stitched Pangolin

### By companion

| Companion | Rarity | Role | Major | Minor |
|---|---|---|---|---|
| Teapot Pilgrim | Common | Shooter | Ember | Mask |
| Candle Snail | Uncommon | Controller | Hearth, Ember | Sweet-tooth |
| Mailbox Imp | Common | Support | Archive | Gauntlet, Ball |
| Pillow Oracle | Mythic | Support | Starlight, Patchwork | Gear |
| Mushroom Librarian | Common | Controller | Garden | Clover |
| Hood Wisp | Common | Shooter | Springstep | Root |
| Jelly Swordsman | Common | Guard | Patchwork | Gauntlet |
| Brass Acorn | Common | Striker | Court | Mask |
| Bread Crab | Common | Guard | Hearth | Sweet-tooth, Gauntlet |
| Kettle Bat | Common | Support | Windborne, Patchwork |  |
| Bookmark Golem | Rare | Support | Archive | Rune, Hat |
| Tin Tick | Common | Controller | Clockwork | Rune, Mask |
| Spring Duck | Rare | Guard | Clockwork, Springstep | Shoe |
| Button Beetle | Common | Support | Court | Root |
| Staple Mole | Common | Support | Clockwork | Clover |
| Snow Spoon | Common | Support | Frost, Patchwork |  |
| Ice Cream Owl | Uncommon | Controller | Frost | Rune, Hat |
| Fridge Wisp | Common | Guard | Springstep | Gauntlet, Shoe |
| Pear Moth | Uncommon | Striker | Lantern | Rune |
| Citrus Toad | Uncommon | Support | Springstep | Shoe |
| Radish Jester | Common | Support | Garden | Mask |
| Seed Hedgehog | Common | Controller | Garden | Clover |
| Moon Jelly | Rare | Shooter | Starlight, Springstep |  |
| Star Sock | Uncommon | Shooter | Springstep | Shoe |
| Comet Kite | Common | Support | Windborne, Storm | Paper-wings |
| Eclipse Egg | Rainbow | Support | Starlight, Court |  |
| Orbit Moth | Common | Controller | Starlight, Court |  |
| Dream Compass | Common | Shooter | Windborne, Springstep |  |
| Shell Postman | Common | Shooter | Lantern | Ball |
| Coral Teacup | Uncommon | Support | Tide | Gear |
| Bubble Koi | Common | Support | Tide | Clover |
| Anchor Snail | Uncommon | Controller | Tide | Paper-wings? |
| Sail Squid | Uncommon | Guard | Tide, Brawl |  |
| Sock Cyclops | Common | Striker | Brawl | Shoe |
| Dice Crab | Common | Support | Carnival | Ball |
| Mask Pepper | Common | Support | Carnival, Ember |  |
| Crayon Gremlin | Common | Controller | Carnival | Big-top |
| Mirror Newt | Mythic | Shooter | Carnival | Big-top |
| Ribbon Spider | Common | Striker | Carnival, Brawl |  |
| Coil Kettle | Mythic | Controller | Ember, Storm |  |
| Thimble Moth | Rare | Shooter | Archive | Hat |
| Reel Owl | Mythic | Shooter | Storm | Hat |
| The Impossible Tailor | Rainbow | Striker | Frost, Brawl |  |
| The Last Bell | Rainbow | Shooter | Clockwork, Lantern |  |
| The Forgotten Patchwork | Rare | Shooter | Archive, Patchwork |  |
| The Folded Courier | Rare | Shooter | Windborne | Paper-wings |
| The Sleeping Friend | Rare | Shooter | Starlight, Patchwork |  |
| Saffron Mechanist | Rare | Guard | Brawl, Ember |  |
| Clockwork Piper | Mythic | Summoner | Clockwork | Horn? |
| Velvet Champion | Rare | Guard | Archive, Brawl |  |
| Paper Inventor | Mythic | Shooter | Garden | Gear |
| Return Captain | Mythic | Shooter | Court | Hat |
| Dough Foreman | Rare | Guard | Starlight | Sweet-tooth |
| Iron Locomotive | Mythic | Controller | Frost | Rain-script? |
| Violet Remnant | Uncommon | Guard | Carnival, Brawl |  |
| Velvet Enchantress | Uncommon | Controller | Hearth, Court |  |
| Grand Sprout | Uncommon | Guard | Clockwork, Garden |  |
| Bristle Ball | Uncommon | Guard | Brawl | Horn |
| Paper Devil | Uncommon | Guard | Ember | Horn |
| Balloon Mallet | Uncommon | Guard | Tide | Mask, Ball |
| Little Sprout | Uncommon | Guard | Garden, Starlight |  |
| Monster Marble | Uncommon | Guard | Frost | Mask, Horn |
| Carousel Pony | Uncommon | Shooter | Court | Horn |
| Teacup Cricket | Uncommon | Guard | Hearth, Lantern |  |
| Ribbon Otter | Uncommon | Guard | Tide | Rain-script |
| Spindle Wren | Uncommon | Support | Windborne | Paper-wings |
| Baker Mouse | Uncommon | Shooter | Hearth | Sweet-tooth |
| Tea Dragon | Mythic | Shooter | Hearth, Ember |  |
| Sundae Golem | Rainbow | Controller | Hearth | Sweet-tooth |
| Lens Owl | Rare | Shooter | Archive, Lantern | Rune |
| The Endless Index | Rainbow | Shooter | Archive | Rune |
| Lamp Goat | Uncommon | Striker | Lantern, Ember | Horn |
| Top Beetle | Rare | Shooter | Clockwork | Gear |
| Shovel Penguin | Rare | Shooter | Frost | Gauntlet |
| Boba Tadpole | Common | Shooter | Frost | Sweet-tooth |
| Rain Heron | Rare | Shooter | Storm | Rain-script |
| Watering Snail | Uncommon | Controller | Garden | Root |
| Yo-yo Squirrel | Rare | Shooter | Windborne, Springstep | Shoe |
| The First Seed | Rainbow | Shooter | Garden | Root |
| Paper Plane Pigeon | Common | Shooter | Windborne | Paper-wings |
| Raincoat Frog | Uncommon | Shooter | Tide | Rain-script |
| Harbor Whale | Mythic | Shooter | Lantern | Sea-shanty |
| Kite Dragon | Rainbow | Shooter | Windborne, Storm | Paper-wings |
| Ringmaster Jack | Mythic | Shooter | Carnival | Big-top |
| The Lost Carousel | Rainbow | Shooter | Carnival | Big-top |
| Static Ferret | Common | Striker | Storm | Gauntlet |
| Storm Kitten | Uncommon | Shooter | Starlight, Storm |  |
| Volt Eel | Rare | Shooter | Lantern, Storm |  |
| Shanty Sea Lion | Common | Shooter | Tide | Sea-shanty |
| Bottle Gull | Uncommon | Shooter | Hearth | Sea-shanty |
| Pinned Beetle | Common | Shooter | Archive | Exhibit-hall |
| Fossil Armadillo | Uncommon | Controller | Frost | Exhibit-hall |
| Specimen Slime | Rare | Shooter | Clockwork | Exhibit-hall |
| Court Herald | Common | Shooter | Court | Hat |
| Stitched Pangolin | Uncommon | Guard | Patchwork | Exhibit-hall |
| Saddle Moth | Rare | Support | Patchwork | Sea-shanty? |
