# Art redo requests: characters and weapons (24/10/2026)

An audit of every hero, companion and weapon picture found two generations of art. 77 pieces are still in the first,
flat style: thick outline, flat fills, one soft shadow, few materials. The rest are in the later detailed storybook style:
layered painted texture, real materials (wool, brass, wood, enamel, paper), small props and costume detail. The flat
pieces look unfinished next to the others, so this request repaints them in the detailed style.

Review sheets from the audit: [heroes](../PreviewExports/ArtAudit/hero.jpg), [companions](../PreviewExports/ArtAudit/comp.jpg),
[weapons](../PreviewExports/ArtAudit/weap.jpg). Each sheet is sorted from least to most detailed. The number is an
edge-density score: the flat group scores 10–26, the detailed group 30–60.

## How to deliver

- Put the files in `Assets/ArcaneBrigade/Art/Playtest/Redo/`, then run **Arcane Brigade → Art → Put repainted characters
  and weapons in**. The builder swaps each one into the game and skips anything still missing, so the set can arrive in parts.
- **Character sheet:** `<Id>.png` is a 4×2 grid of eight equal cells, 256×256 or 320×320 per cell, real transparent RGBA,
  feet on the same baseline in every cell. Frame order, left to right then top to bottom: idle 1, idle 2 (a small breath),
  walk 1, walk 2, windup 1, windup 2 (gathering for an attack), release (the attack itself), recovery. The first cell is also
  the shop and roster portrait. Three-quarter view facing right. Draw no weapon unless it is part of the body; weapons are
  held separately in the game.
- **Weapon sheet:** `Weapon_<Id>.png` is one held weapon, pointing right, grip on the left, on a transparent canvas about
  700×300, with at least 12% empty margin.
- Keep each design: the same character or object, silhouette, colours and idea as its current picture (the Reference
  column). Repaint it in the detailed style, with materials, stitching, small accessories and soft painted light from the
  upper left. Readable at small size.
- Style references in the detailed style: characters `Assets/ArcaneBrigade/Art/Expansion/Characters8_v1.png` (Storm Kitten,
  Volt Eel, Shovel Penguin…), Juggling Jester, Lighthouse Whale and Bonsai Clocksmith. Weapons: Starforge Claymore, Dream
  Racket, Corkscrew Rapier and Twin Moon Staff.
- No text, ground, cast shadow, pools, glow, round auras or background. Our side's palette is amber, cream, brass, teal and moon
  tones. Red is only a material, such as wax, velvet or cloth.

## Shared prompt (characters)

> Use case: illustration-story. Production game character animation sheet for Arcane Brigade. The attached image is the
> current design of `<NAME>`; keep its identity, silhouette, colours and idea exactly, but repaint it in the detailed
> hand-painted storybook style of the second reference (layered painted texture, real materials, stitching, small
> accessories, warm ink contour, soft light from upper left). Draw ONE 4×2 grid of eight equal square cells, transparent RGBA,
> the character facing right in three-quarter view, feet on one shared baseline: idle, idle with a small breath, walk step,
> walk step, windup, deeper windup, the attack's release, recovery. Same size and position in every cell. No weapon, text,
> ground, cast shadow, glow or background. At least 12% transparent margin in every cell. Readable at tiny game scale.

## Shared prompt (weapons)

> Use case: illustration-story. Production game held-weapon sprite for Arcane Brigade. The attached image is the current
> design of `<NAME>`; keep its idea, shape and colours, but repaint it in the detailed hand-painted storybook style of the
> reference weapons (worn brass, lacquered wood, enamel, cloth wraps, small engraved details, warm ink contour, light from
> upper left). One weapon, pointing right, grip on the left, on a transparent canvas, at least 12% margin. No hand, text, glow,
> ground or background.

## Heroes (10)

These are the playable heroes still in the flat style. Their acting sheets (`Assets/ArcaneBrigade/Art/Acting/<Id>_Acting_v1.png`)
are in the same flat style. Repaint those too, in their existing layout, once the 4×2 sheet is approved.

| Hero | File | Reference (current) |
| --- | --- | --- |
| Root Oracle | `RootOracle.png` | `Assets/ArcaneBrigade/Art/FinalQuality/RootOracle_v1.png` |
| Lone Jelly | `LoneJelly.png` | `Assets/ArcaneBrigade/Art/FinalQuality/LoneJelly_v1.png` |
| Lantern Knight | `LanternKnight.png` | `Assets/ArcaneBrigade/Art/FinalQuality/LanternKnight_v1.png` |
| Drowsy Seer | `DrowsySeer.png` | `Assets/ArcaneBrigade/Art/FinalQuality/DrowsySeer_v1.png` |
| Postmaster | `Postmaster.png` | `Assets/ArcaneBrigade/Art/FinalQuality/Postmaster_v1.png` |
| Treasure Acorn | `TreasureAcorn.png` | `Assets/ArcaneBrigade/Art/FinalQuality/TreasureAcorn_v1.png` |
| Comet Runner | `CometRunner.png` | `Assets/ArcaneBrigade/Art/FinalQuality/CometRunner_v1.png` |
| Tide Singer | `TideSinger.png` | `Assets/ArcaneBrigade/Art/FinalQuality/TideSinger_v1.png` |
| Glass Dice | `GlassDice.png` | `Assets/ArcaneBrigade/Art/FinalQuality/GlassDice_v1.png` |
| Mirror Keeper | `MirrorKeeper.png` | `Assets/ArcaneBrigade/Art/FinalQuality/MirrorKeeper_v1.png` |

## Companions (44)

Eight companions reuse a hero's picture almost unchanged, so in the game they look like the hero. Give each of them its own
design that follows its name; the hero keeps the original look. They are marked **own design**.

| Companion | File | Reference (current) | Note |
| --- | --- | --- | --- |
| Crayon Gremlin | `CrayonGremlin.png` | `Assets/ArcaneBrigade/Art/K10/CrayonGremlin.png` |  |
| Ribbon Spider | `RibbonSpider.png` | `Assets/ArcaneBrigade/Art/K10/RibbonSpider.png` |  |
| Pillow Oracle | `PillowOracle.png` | `Assets/ArcaneBrigade/Art/AnimationHD/BrigadeAnimation_v3.png` | **own design** (now copies Root Oracle) |
| Moon Jelly | `MoonJelly.png` | `Assets/ArcaneBrigade/Art/K10/MoonJelly.png` |  |
| Bread Crab | `BreadCrab.png` | `Assets/ArcaneBrigade/Art/K10/BreadCrab.png` |  |
| Radish Jester | `RadishJester.png` | `Assets/ArcaneBrigade/Art/K10/RadishJester.png` |  |
| Brass Acorn | `BrassAcorn.png` | `Assets/ArcaneBrigade/Art/K10/BrassAcorn.png` | **own design** (now copies Treasure Acorn) |
| Eclipse Egg | `EclipseEgg.png` | `Assets/ArcaneBrigade/Art/K10/EclipseEgg.png` |  |
| Shell Postman | `ShellPostman.png` | `Assets/ArcaneBrigade/Art/K10/ShellPostman.png` |  |
| Seed Hedgehog | `SeedHedgehog.png` | `Assets/ArcaneBrigade/Art/K10/SeedHedgehog.png` |  |
| Pear Moth | `PearMoth.png` | `Assets/ArcaneBrigade/Art/K10/PearMoth.png` |  |
| Spring Duck | `SpringDuck.png` | `Assets/ArcaneBrigade/Art/K10/SpringDuck.png` |  |
| Orbit Moth | `OrbitMoth.png` | `Assets/ArcaneBrigade/Art/K10/OrbitMoth.png` |  |
| Mask Pepper | `MaskPepper.png` | `Assets/ArcaneBrigade/Art/K10/MaskPepper.png` |  |
| Sock Cyclops | `SockCyclops.png` | `Assets/ArcaneBrigade/Art/K10/SockCyclops.png` |  |
| Citrus Toad | `CitrusToad.png` | `Assets/ArcaneBrigade/Art/K10/CitrusToad.png` |  |
| Kettle Bat | `KettleBat.png` | `Assets/ArcaneBrigade/Art/K10/KettleBat.png` |  |
| Star Sock | `StarSock.png` | `Assets/ArcaneBrigade/Art/K10/StarSock.png` |  |
| Fridge Wisp | `FridgeWisp.png` | `Assets/ArcaneBrigade/Art/K10/FridgeWisp.png` |  |
| Comet Kite | `CometKite.png` | `Assets/ArcaneBrigade/Art/K10/CometKite.png` | **own design** (now copies Comet Runner) |
| Sail Squid | `SailSquid.png` | `Assets/ArcaneBrigade/Art/K10/SailSquid.png` |  |
| Candle Snail | `CandleSnail.png` | `Assets/ArcaneBrigade/Art/AnimationHD/BrigadeAnimation_v3.png` |  |
| Anchor Snail | `AnchorSnail.png` | `Assets/ArcaneBrigade/Art/K10/AnchorSnail.png` |  |
| Bookmark Golem | `BookmarkGolem.png` | `Assets/ArcaneBrigade/Art/K10/BookmarkGolem.png` |  |
| Mailbox Imp | `MailboxImp.png` | `Assets/ArcaneBrigade/Art/AnimationHD/BrigadeAnimation_v3.png` | **own design** (now copies Postmaster) |
| Coral Teacup | `CoralTeacup.png` | `Assets/ArcaneBrigade/Art/K10/CoralTeacup.png` |  |
| Teapot Pilgrim | `TeapotPilgrim.png` | `Assets/ArcaneBrigade/Art/AnimationHD/BrigadeAnimation_v3.png` |  |
| Mirror Newt | `MirrorNewt.png` | `Assets/ArcaneBrigade/Art/K10/MirrorNewt.png` | **own design** (now copies Mirror Keeper) |
| Staple Mole | `StapleMole.png` | `Assets/ArcaneBrigade/Art/K10/StapleMole.png` |  |
| Bubble Koi | `BubbleKoi.png` | `Assets/ArcaneBrigade/Art/K10/BubbleKoi.png` | **own design** (now copies Tide Singer) |
| Ice Cream Owl | `IceCreamOwl.png` | `Assets/ArcaneBrigade/Art/K10/IceCreamOwl.png` |  |
| Jelly Swordsman | `JellySwordsman.png` | `Assets/ArcaneBrigade/Art/AnimationHD/HeroIdentity_v1.png` | **own design** (now copies Lone Jelly) |
| Snow Spoon | `SnowSpoon.png` | `Assets/ArcaneBrigade/Art/K10/SnowSpoon.png` |  |
| Dice Crab | `DiceCrab.png` | `Assets/ArcaneBrigade/Art/K10/DiceCrab.png` | **own design** (now copies Glass Dice) |
| Button Beetle | `ButtonBeetle.png` | `Assets/ArcaneBrigade/Art/K10/ButtonBeetle.png` |  |
| Mushroom Librarian | `MushroomLibrarian.png` | `Assets/ArcaneBrigade/Art/AnimationHD/BrigadeAnimation_v3.png` |  |
| Hood Wisp | `HoodWisp.png` | `Assets/ArcaneBrigade/Art/AnimationHD/HeroIdentity_v1.png` |  |
| Dream Compass | `DreamCompass.png` | `Assets/ArcaneBrigade/Art/K10/DreamCompass.png` |  |
| Tin Tick | `TinTick.png` | `Assets/ArcaneBrigade/Art/K10/TinTick.png` |  |
| Coil Kettle | `CoilKettle.png` | `Assets/ArcaneBrigade/Art/K16/CoilKettle_v1.png` |  |
| Monster Marble | `MonsterMarble.png` | `Assets/ArcaneBrigade/Art/October/Allies4.png` |  |
| Balloon Mallet | `BalloonMallet.png` | `Assets/ArcaneBrigade/Art/October/Allies4.png` |  |
| Reel Owl | `ReelOwl.png` | `Assets/ArcaneBrigade/Art/K16/ReelOwl_v1.png` |  |
| Thimble Moth | `ThimbleMoth.png` | `Assets/ArcaneBrigade/Art/K16/ThimbleMoth_v2.png` |  |

## Weapons (23)

| Weapon | File | Reference (current) |
| --- | --- | --- |
| Book Paddle | `Weapon_BookPaddle.png` | `Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png` |
| Bell Fork | `Weapon_BellFork.png` | `Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png` |
| Rubber Hammer | `Weapon_RubberHammer.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Key Cleaver | `Weapon_KeyCleaver.png` | `Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png` |
| Mailbox Launcher | `Weapon_MailboxLauncher.png` | `Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png` |
| Umbrella Lance | `Weapon_UmbrellaLance.png` | `Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png` |
| Dream Stamp | `Weapon_DreamStamp.png` | `Assets/ArcaneBrigade/Art/QualityHD/DreamStamp_v1.png` |
| Spring Knocker | `Weapon_SpringKnocker.png` | `Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png` |
| Rune Staff | `Weapon_RuneStaff.png` | `Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png` |
| Dream Iron | `Weapon_DreamIron.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Ribbon Wheel | `Weapon_RibbonWheel.png` | `Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png` |
| Teapot Popgun | `Weapon_TeapotPopgun.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Scrap Magnet | `Weapon_ScrapMagnet.png` | `Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png` |
| Cloud Scissors | `Weapon_CloudScissors.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Alarm Mortar | `Weapon_AlarmMortar.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Clockwork Yo Yo | `Weapon_ClockworkYoYo.png` | `Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png` |
| Music Box | `Weapon_MusicBox.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Sleep Camera | `Weapon_SleepCamera.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Tin Fish | `Weapon_TinFish.png` | `Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png` |
| Pizza Wheel | `Weapon_PizzaWheel.png` | `Assets/ArcaneBrigade/Art/K10/PizzaWheel.png` |
| Dream Projector | `Weapon_DreamProjector.png` | `Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png` |
| Fire Extinguisher | `Weapon_FireExtinguisher.png` | `Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png` |
| Needle Singer | `Weapon_NeedleSinger.png` | `Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png` |

Totals: 54 character sheets (8 frames each) and 23 weapons. At 256 px per cell that comes to roughly 20–30 MB of
source PNG. Unity compresses these (ASTC/BC7), which keeps the build inside its size budget.
