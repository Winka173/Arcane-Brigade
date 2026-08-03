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

## 12c. Owner change (27/10): bond steps 2/4/6/8, team of 10, and what it means for the roster
- **Major bond steps: 2 / 4 / 6 / 8.** Step 8 needs 8 different companions of the bond.
- **Minor bond steps:** 4-companion minors 2 / 4; 6-companion minors 2 / 4 / 6.
- **A team has at most 10 companions and a run lasts about 20 waves.** Every companion counts in 2 or 3 bonds, so 10 companions give 20 to 30 bond points. Target builds: `8+4+2+4+2`, `6+6+2+2+2`, `8+6+2+2+2` (each sum is 18 to 20).
- **What the numbers say:** `8+6` with 10 companions needs at least 4 companions who are in both major bonds. The first optimiser run (section 13 before this change) shared at most 2 between any two majors, so those builds were impossible. The roster is therefore laid out in **houses**.

### Houses (the design that makes 8+6 builds possible)
- 16 majors form 8 **twin pairs**. Each pair is a house of 12 companions: 4 are in both majors (shared), 4 only in the first, 4 only in the second. 8 houses x 12 = 96 companions exactly.
- Each house has two minors (one of 6 companions, one of 4), so 8 x (6+4) = 80 minor slots as in 12b, and every single-major companion has a minor.
- A team of 10 inside one house can reach `8+6` (4 shared + 4 first-only + 2 second-only) plus the minors on top. Builds that mix houses give a 6 or lower in each major, which is the intended trade-off.
- 36 heroes x 2 to 4 builds: every hero direction is a house (or two houses). Each companion is in one house, 3 heroes of about 11 take a given house as the main direction.

### Step 8 (capstone) of each major bond (first drafts)

- **Hearth**: every campfire becomes a hearth kitchen: a Feast every 10 s and its dish is chosen by the best star
- **Ember**: a wildfire: all fire in the arena spreads on its own and burn damage doubles
- **Frost**: permanent winter: the whole arena is an ice patch, enemies are always chilled
- **Tide**: a tidal wave keeps flooding: soak never wears off while the wave runs
- **Storm**: a permanent storm cloud follows the hero and strikes the most charged enemy every 2 s
- **Windborne**: a gale: every volley pushes all enemies back and pierces
- **Garden**: the Great Tree: a giant tree grows in the arena, pods fruit every 5 s
- **Patchwork**: one companion becomes the Quilt: every mutation of the brigade also applies to it, doubled
- **Starlight**: the constellation completes: its blessing is doubled and a comet falls on each critical
- **Court**: coronation: two decree laws instead of one
- **Carnival**: the wheel always lands on jackpot once a wave
- **Brawl**: champion ring: the strongest melee friend becomes a giant for 8 s every wave
- **Archive**: complete set: every recorded species is permanently weakened 15%
- **Lantern**: a lighthouse: one huge lamp lights the whole arena; enemies in light take +25%
- **Clockwork**: overclock: every skill bar fills twice as fast for 10 s after each time stop
- **Springstep**: spring dance: pads chain, each landing launches the hero to the next pad with a shockwave

## 13. Bond roster by house (27/10, optimiser with the house constraint; `?` = no thematic reason, to be reshaped when the skill is written)

Per house: 4 shared (in both majors), 4 only in the first major, 4 only in the second. Minors: one of 6, one of 4.

### House 1: Hearth + Ember (minors: Sweet-tooth 6, Ball 4)

- **Shared:** Candle Snail [Sweet-tooth], Coil Kettle, Saffron Mechanist, Tea Dragon
- **Only Hearth:** Bread Crab [Sweet-tooth], Baker Mouse [Sweet-tooth], Sundae Golem [Sweet-tooth], Boba Tadpole [Sweet-tooth,Ball]
- **Only Ember:** Teapot Pilgrim [Sweet-tooth], Mailbox Imp [Ball]?, Dice Crab [Ball]?, Mask Pepper [Ball]?

### House 2: Frost + Tide (minors: Gauntlet 6, Sea-shanty 4)

- **Shared:** Coral Teacup [Sea-shanty]?, Anchor Snail?, Sail Squid [Gauntlet]?, Iron Locomotive?
- **Only Frost:** Snow Spoon [Gauntlet]?, Fridge Wisp [Gauntlet], The Impossible Tailor [Gauntlet], Shovel Penguin [Gauntlet]
- **Only Tide:** Shell Postman [Sea-shanty], Balloon Mallet [Gauntlet], Shanty Sea Lion [Sea-shanty], Bottle Gull [Sea-shanty]

### House 3: Storm + Windborne (minors: Paper-wings 6, Rain-script 4)

- **Shared:** Comet Kite [Paper-wings], Kite Dragon [Paper-wings], Static Ferret?, Storm Kitten?
- **Only Storm:** Ribbon Otter [Rain-script]?, Rain Heron [Rain-script], Raincoat Frog [Rain-script]?, Volt Eel [Rain-script]?
- **Only Windborne:** Kettle Bat [Paper-wings]?, The Folded Courier [Paper-wings], Spindle Wren [Paper-wings], Paper Plane Pigeon [Paper-wings]

### House 4: Garden + Patchwork (minors: Mask 6, Root 4)

- **Shared:** Mushroom Librarian?, Radish Jester [Mask]?, Seed Hedgehog [Root]?, The Sleeping Friend?
- **Only Garden:** Paper Inventor [Mask]?, Grand Sprout [Root], Watering Snail [Root], The First Seed [Root]
- **Only Patchwork:** Pillow Oracle [Mask]?, The Forgotten Patchwork [Mask]?, Stitched Pangolin [Mask]?, Saddle Moth [Mask]?

### House 5: Starlight + Court (minors: Hat 6, Clover 4)

- **Shared:** Eclipse Egg, Orbit Moth, Dough Foreman, Court Herald [Hat]?
- **Only Starlight:** Staple Mole [Clover]?, Moon Jelly [Clover]?, Bubble Koi [Clover], Little Sprout [Hat]?
- **Only Court:** Brass Acorn [Hat], Ice Cream Owl [Hat], Return Captain [Hat], Velvet Enchantress [Hat,Clover]?

### House 6: Carnival + Brawl (minors: Horn 6, Big-top 4)

- **Shared:** Jelly Swordsman [Horn]?, Crayon Gremlin [Big-top], Ribbon Spider, Violet Remnant
- **Only Carnival:** Mirror Newt [Big-top], Carousel Pony [Horn], Ringmaster Jack [Big-top], The Lost Carousel [Big-top]
- **Only Brawl:** Bristle Ball [Horn], Paper Devil [Horn], Monster Marble [Horn], Lamp Goat [Horn]

### House 7: Archive + Lantern (minors: Rune 6, Exhibit-hall 4)

- **Shared:** Thimble Moth?, Reel Owl?, Velvet Champion?, Lens Owl [Rune]
- **Only Archive:** Bookmark Golem [Rune,Exhibit-hall], Pear Moth [Rune], The Endless Index [Rune], Pinned Beetle [Exhibit-hall]
- **Only Lantern:** Hood Wisp [Rune], Teacup Cricket [Exhibit-hall]?, Harbor Whale [Rune]?, Fossil Armadillo [Exhibit-hall]?

### House 8: Clockwork + Springstep (minors: Shoe 6, Gear 4)

- **Shared:** Spring Duck [Gear], Dream Compass, The Last Bell?, Top Beetle [Gear]
- **Only Clockwork:** Tin Tick [Gear], Button Beetle [Shoe]?, Clockwork Piper [Gear], Specimen Slime [Shoe]?
- **Only Springstep:** Citrus Toad [Shoe], Star Sock [Shoe], Sock Cyclops [Shoe]?, Yo-yo Squirrel [Shoe]


## 14. Owner change (27/10): redraw allowed, 1 to 3 bonds per companion, 1 to 2 per hero
- Companions, and where needed heroes, enemies and items, may be redrawn so the look fits the house, the bond and the skill. A companion has 1 to 3 bonds (the roster above stays the starting point).
- A **hero has 1 to 2 bonds and counts as a member of them** (so a team of 10 companions plus the hero can reach 8 with 7 companions). Hero directions (2 to 4 builds) are the houses of the hero's bonds plus one neighbour house.
- Art is replaced in place at the same size (build-size budget), delivered house by house (12 companions + that house's heroes + its object/icon art), so every batch ends in a playable build.
- Each house gets one **visual language** (trim, colour accent, ornament) so a bond is readable from the silhouette; bond objects and icons stay code ink.

### 14b. Owner decisions (27/10)
- **Redraw all 96 companions and all heroes** to unify style: some companions and heroes carry far more detail than others. Enemies and items only if they clash.
- **Heroes get minor bonds only (1 to 2)**, never a major bond, so a hero's 2 to 4 build directions stay open. A hero counts as a member of its minor bonds (a 6-minor needs 5 companions and the hero).
- Order: style pilot (3 sheets: one common companion, one mythic companion, one hero) to lock the style; then house by house (12 companions + that house's heroes). Brief: `Docs/Style_Brief_2026-10-27.md`.

## 15. Owner decision (27/10): option B, a smaller game
- **6 houses, 72 companions, 12 major bonds, 12 minor bonds, 24 heroes, 4 hero-only relics per hero (96).**
- Houses kept: 1 Hearth + Ember, 2 Frost + Tide, 3 Storm + Windborne, 5 Starlight + Court, 6 Carnival + Brawl, 7 Archive + Lantern (renumber 1 to 6 when implemented). Minors per house: Sweet-tooth/Ball, Gauntlet/Sea-shanty, Paper-wings/Rain-script, Hat/Clover, Horn/Big-top, Rune/Exhibit-hall. Bond counts: 12 majors x 8 = 96 slots, 12 minors (6 of 6, 6 of 4) = 60 slots; 72 companions x 2 + 12 third bonds = 156.
- **Dropped: house 4 (Garden + Patchwork) and house 8 (Clockwork + Springstep), 24 companions.** Garden, Patchwork, Clockwork and Springstep bonds, Mask, Root, Shoe and Gear minors go; the old Unbroken bond and unused stat-bonds are deleted with them. Dropped companions: Mushroom Librarian, Radish Jester, Seed Hedgehog, The Sleeping Friend, Paper Inventor, Grand Sprout, Watering Snail, The First Seed, Pillow Oracle, The Forgotten Patchwork, Stitched Pangolin, Saddle Moth, Spring Duck, Dream Compass, The Last Bell, Top Beetle, Tin Tick, Button Beetle, Clockwork Piper, Specimen Slime, Citrus Toad, Star Sock, Sock Cyclops, Yo-yo Squirrel.
- **Heroes 36 to 24:** the 8 coming-soon heroes (Supper Mortar, Hourglass Stag, Rain Archivist, Kite Admiral, Sundae Duchess, Bonsai Clocksmith, Juggling Jester, Bell Diver) wait for a later release, plus 4 playable heroes whose theme belonged to the dropped houses (proposal: Verdant Matron, Petal Citadel, Patchwork Host, Thorn Treasurer; owner to confirm).
- **Migration:** saves that own a dropped companion or hero keep a harmless stub (refund its material value) so nothing breaks.
- **Art and effects:** redraw only the 72 companions and 24 heroes that stay; skill effects are code ink, art only for floor objects (summoned tree, turret, banner...).

### 15b. Owner confirmation (27/10)
The 4 extra heroes (Verdant Matron, Petal Citadel, Patchwork Host, Thorn Treasurer) are dropped. Old saves do not matter: no stubs, no refunds, no migration for dropped companions or heroes. Art for the 5 remaining houses and the 23 heroes went to painting in parallel the same day (house 1 earlier).

## 16. House 1 skills: Hearth + Ember (first draft, 27/10)
Bars fill with time only (Shooter 6 s, Controller 9 s, Guard 10 s, Support 8 s). All effects are code ink; only the "Object" column needs art. Companions are immortal, so nothing here heals or shields companions: healing goes to the hero, buffs are offence or utility. Bond counters: **Warmth** (Hearth, healing done) and **Heat** (Ember, enemies burning).

| Companion | Bars | Skill | What it does | Bond tie-in | Object (art) |
|---|---|---|---|---|---|
| Teapot Pilgrim | 6 s | Steep | throws a kettle; the steam pool heals the hero and burns enemies standing in it for 4 s | Hearth: pool also fills Warmth; Ember: pool leaves coals | none |
| Tea Dragon (Mythic) | 6 s | Steam Breath | a cone of steam; each enemy hit burns and gives the hero 1 HP; every 3rd breath is a long flame | personal trait: its flames never go out between waves (coals stay) | none |
| Baker Mouse | 6 s | Rolling Pin | rolls a dough ball across the field; flattens and slows what it passes | Hearth: dough heals the hero when it hits 3+ enemies | none |
| Boba Tadpole | 6 s | Pearl Volley | spits 6 tapioca pearls in a fan; each bounces once | Hearth: a pearl that kills drops a Snack (small Warmth) | none |
| Candle Snail | 9 s | Wax Trail | leaves a wax trail for 5 s; enemies on it slow 30% and take burn | Ember: the head of the trail lights, burning enemies add Heat | none |
| Coil Kettle (Mythic) | 9 s | Spring Boil | bounces three times; each landing stuns 0.8 s and vents steam | personal trait: the third bounce is a coil-shot that chains to one more enemy | none |
| Sundae Golem (Rainbow) | 9 s | Sprinkle Freeze | a scoop of freezing sprinkles, 2 s freeze in a circle | every 3rd cast drops a Cherry that heals the hero; personal trait: cherry also adds Warmth | cherry pickup (small art) |
| Saffron Mechanist (Rare) | 10 s | Pressure Valve | taunts nearby enemies while pressure builds, then a steam blast pushes them back | Ember: the blast lights enemies | none |
| Bread Crab | 10 s | Toast Shell | taunts for 3 s then a crumb pulse | Hearth: crumbs fill Warmth per enemy hit | none |
| Mailbox Imp (now a flying envelope imp) | 8 s | Hot Delivery | throws sealed hot letters (its own wax seal and envelope): the 3 nearest friends gain +20% damage for 6 s and one letter explodes on the farthest enemy | Ember: the explosion leaves coals | none: the letter is a piece of its own body |
| Dice Crab | 8 s | Loaded Roll | rolls a glowing die: 1 to 6 sets a fire pulse (N x base); a 6 rolls twice | Ember: a 6 adds +10 Heat | none |
| Mask Pepper | 8 s | Spicy Face | switches mask: Angry (+15% attack speed to friends 5 s) or Cheer (+10% crit 5 s), then a fire ring lights nearby enemies | Ember: lit enemies feed Heat | none |

Hearth object: **campfire** (art, one prop). Ember object: **coals** (code ink on the floor).

## 17. Version C (28/10): 16 elemental majors, 96 companions, 36 heroes (supersedes sections 15 and 13)
Owner changes: Hearth must buff stats (not only support); Brawl (wrestling) is replaced by an element; 4 more bonds; more heroes and companions; then rebalance.

- **16 major bonds, 8 houses** (Brawl, Springstep and Patchwork are gone): Hearth + Ember, Frost + Tide, Storm + Windborne, **Garden + Venom**, **Starlight + Dusk**, **Carnival + Court**, Archive + Lantern, **Stone + Clockwork**. New: Venom (poison), Dusk (shadow), Stone (earth), Garden (plant) back, Clockwork (metal and time) back.
- **16 minor bonds** (6 of 6, so 8 of 6 and 8 of 4), two per house: Sweet-tooth/Ball, Gauntlet/Sea-shanty, Paper-wings/Rain-script, Shoe/Root, Hat/Clover, Mask/Big-top, Rune/Exhibit-hall, Horn/Gear.
- **96 companions** (12 per house: 4 shared, 4 + 4), **36 heroes** (the 8 coming-soon heroes join and the 4 cut heroes return: Verdant Matron, Petal Citadel and Thorn Treasurer anchor the Garden house, Patchwork Host the Dusk/Court side), **4 hero-only relics each = 144**.
- **Hearth is a stat engine:** 2: +8% damage and +8% skill speed to the whole team; 4: each campfire gives +12% damage and +10% max health to the hero inside it, Warmth full = a Feast (+15% damage 8 s and a hero heal); 6: campfire radius doubles and the team also gains +10% attack speed; 8: the hearth kitchen: a permanent campfire at the hero, all Hearth stats doubled, a Feast every 10 s. Healing stays on the hero (companions are immortal).
- **Bond colours (body palette):** Hearth apricot, Ember crimson, Frost ice blue, Tide teal, Storm slate + electric yellow, Windborne pearl + lilac, Garden leaf green + pink, Venom chartreuse + violet, Starlight indigo, Dusk charcoal + plum, Carnival magenta + cream, Court royal purple, Archive parchment brown, Lantern amber + olive, Stone sand-ochre + grey-brown, Clockwork gunmetal + brass.
- **Uniqueness rule:** no two characters share a species or object (one snail, one owl, one cat, one frog...); 19 redesigns plus 12 new hero designs follow it.
- **Balance plan:** every major bond gets a power budget (step 2 about 4%, 4 about 12%, 6 about 25%, 8 about 45% of a base build) tested with a simulation of the 8+6 / 6+6 / 8+4 team shapes over waves 1 to 20; Hearth stats and Venom damage-over-time are tuned against Ember and Storm; hero relic power tuned per hero direction; a nightly run of the simulation over all 16 x 16 pairs flags builds more than 20% above the median.

### Roster by house (final)
Primary bond (first major) gives the body colour.

#### Hearth + Ember (minors: Sweet-tooth 6, Ball 4)

- **Shared:** Candle Snail [Sweet-tooth], Coil Kettle, Saffron Mechanist, Tea Dragon
- **Only Hearth:** Bread Crab [Sweet-tooth], Baker Mouse [Sweet-tooth], Sundae Golem [Sweet-tooth], Boba Tadpole [Sweet-tooth,Ball]
- **Only Ember:** Teapot Pilgrim [Sweet-tooth], Mailbox Imp [Ball], Dice Crab [Ball], Mask Pepper [Ball]

#### Frost + Tide (minors: Gauntlet 6, Sea-shanty 4)

- **Shared:** Coral Teacup [Sea-shanty], Anchor Snail, Sail Squid [Gauntlet], Iron Locomotive
- **Only Frost:** Snow Spoon [Gauntlet], Fridge Wisp [Gauntlet], The Impossible Tailor [Gauntlet], Shovel Penguin [Gauntlet]
- **Only Tide:** Shell Postman [Sea-shanty], Balloon Mallet [Gauntlet], Shanty Sea Lion [Sea-shanty], Bottle Gull [Sea-shanty]

#### Storm + Windborne (minors: Paper-wings 6, Rain-script 4)

- **Shared:** Comet Kite [Paper-wings], Kite Dragon [Paper-wings], Static Ferret, Storm Kitten
- **Only Storm:** Ribbon Otter [Rain-script], Rain Heron [Rain-script], Raincoat Frog [Rain-script], Volt Eel [Rain-script]
- **Only Windborne:** Kettle Bat [Paper-wings], The Folded Courier [Paper-wings], Spindle Wren [Paper-wings], Paper Plane Pigeon [Paper-wings]

#### Garden + Venom (minors: Shoe 6, Root 4)

- **Shared:** Mushroom Librarian, Seed Hedgehog [Root], Paper Inventor, Watering Snail [Root]
- **Only Garden:** Citrus Toad [Shoe], Grand Sprout [Root], Yo-yo Squirrel [Shoe], The First Seed [Root]
- **Only Venom:** Sock Cyclops [Shoe], Mirror Newt [Shoe], Ribbon Spider [Shoe], Paper Devil [Shoe]

#### Starlight + Dusk (minors: Hat 6, Clover 4)

- **Shared:** Pillow Oracle, Eclipse Egg, Orbit Moth [Hat], The Sleeping Friend
- **Only Starlight:** Moon Jelly [Hat], Star Sock [Clover], Bubble Koi [Clover], Little Sprout [Hat,Clover]
- **Only Dusk:** Ice Cream Owl [Hat], The Forgotten Patchwork [Hat], Violet Remnant [Hat], Saddle Moth [Clover]

#### Carnival + Court (minors: Mask 6, Big-top 4)

- **Shared:** Jelly Swordsman, Brass Acorn [Mask], Button Beetle, Carousel Pony
- **Only Carnival:** Radish Jester [Mask,Big-top], Crayon Gremlin [Big-top], Ringmaster Jack [Big-top], The Lost Carousel [Big-top]
- **Only Court:** Return Captain [Mask], Dough Foreman [Mask], Velvet Enchantress [Mask], Court Herald [Mask]

#### Archive + Lantern (minors: Rune 6, Exhibit-hall 4)

- **Shared:** Thimble Moth, Reel Owl, Velvet Champion, Lens Owl [Rune]
- **Only Archive:** Bookmark Golem [Rune,Exhibit-hall], Pear Moth [Rune], The Endless Index [Rune], Pinned Beetle [Exhibit-hall]
- **Only Lantern:** Hood Wisp [Rune], Teacup Cricket [Exhibit-hall], Harbor Whale [Rune], Fossil Armadillo [Exhibit-hall]

#### Stone + Clockwork (minors: Horn 6, Gear 4)

- **Shared:** Staple Mole, The Last Bell, Top Beetle [Gear], Specimen Slime [Horn]
- **Only Stone:** Bristle Ball [Horn], Monster Marble [Horn], Lamp Goat [Horn], Stitched Pangolin [Horn]
- **Only Clockwork:** Tin Tick [Gear], Spring Duck [Gear], Dream Compass [Horn], Clockwork Piper [Gear]


### 17b. Review fixes (28/10)
- Swaps for fit: Iron Locomotive to Stone + Clockwork (a steam train, primary Clockwork) and Specimen Slime to Frost + Tide; Fossil Armadillo to Stone and Lamp Goat to Lantern (a goat carrying a lamp); Pear Moth to Garden + Venom and Paper Inventor to Archive.
- Palettes: Stone granite grey with moss (not sand-ochre, which looked like Archive parchment), Dusk charcoal-black with violet glints (not plum, which looked like Court purple).

### 17c. Rule for every skill (28/10)
A skill and the object it makes must come from the creature's own look (its shell, hat, satchel, seal, jar...), so the art is a piece of the creature and easy to paint. Effects stay code ink. When a skill table row names an object, the object is a recognisable part of that creature.

## 18. Owner changes (28/10): varied mechanics and one spare member
- **Spare member rule:** a bond that needs 8 to reach its top step has 9 members; 6 needs 7; 4 needs 5. Houses are now 6 shared + 3 + 3 companions (majors 9 each), minors 7 and 5. Each companion has 2 or 3 bonds (48 and 48).
- **Mechanics must be varied, not floor objects:** every bond mixes several rule types: on-hit stacks that burst (Ember), stacking slow then freeze (Frost), every Nth hit chain lightning (Storm), poison that spreads on death (Venom), permanent growth and summoned plants (Garden), execute (Dusk), echo and time (Clockwork), reflect and collisions (Stone), random die and combos (Carnival), edicts and laws (Court), stillness and movement (Lantern, Windborne), economy (Starlight, Archive), summons (ten bonds have one). Minor bonds are mini mechanics too (every Nth hit, combo, boomerang shots, stillness, a survived fatal hit). Full text in `Docs/Review_Pack_Bonds_Skills_Heroes_Enemies_2026-10-28.md` section 2.
- **Immortality of companions is under review by the owner;** see the question in the review pack. Bonds that need a death rule are written so they can work either way.
