# Hero effect art requests, part 2 (24/10/2026)

Part 1 (`Hero_Effect_Art_Requests.md`) covered 12 heroes. This part covers the other 24, so every hero has one painted
physical object that moves with its skill. The code is already wired for every sheet below. A sheet starts playing in
the game as soon as it is imported. There is no extra integration step.

## Delivery

- Put sheets in `Assets/ArcaneBrigade/Art/Playtest/HeroEffects/` and name them `<HeroId>_<Item>.png`, exactly as in the
  table. Import with **Arcane Brigade → Art → Import hero effect sheets**, then **Arcane Brigade → Art → Build hero
  effect art library**.
- One horizontal strip per sheet. Equal square cells of 256×256, left to right, real transparent RGBA. Leave at least
  12% empty margin inside every cell. Frames must not overlap and need no cell dividers.
- Use the hero's own awakening art as the identity reference: `Assets/ArcaneBrigade/Art/Arsenal/<HeroId>_Awake_v1.png` or
  `<HeroId>_Awake_Held_v1.png`. Match its hand-painted storybook texture, warm ink contour and palette, with highlights
  from the upper left.
- Each item is a physical object, not an effect icon. Light, glow, steam, water, electricity, sparks and rain are drawn
  in code, so do not paint them. Also leave out the hero, hands, text, ground, cast shadows, pools, round auras and
  backgrounds.
- Keep the object the same size and position in every cell. Only the parts named in "What moves" change.
- Our side's palette is amber, cream, brass, teal and moon tones. Red is only a material, such as wax, velvet or cloth.
  Glowing red belongs to enemies.

## Shared prompt

Fill in `<HERO>`, `<OBJECT>`, `<N>` and `<MOTION>` from the table:

> Use case: illustration-story. Production game sprite animation sheet for Arcane Brigade.
> The attached image is the exact identity/design reference for `<HERO>`'s awakening art. Match its layered hand-painted
> storybook texture, warm confident ink contour, materials and palette, highlights from upper left. This is a physical
> painted object, not an effect icon.
> Draw ONE horizontal strip of `<N>` equal square cells, left to right, transparent RGBA, intended delivery `<N×256>`x256
> (256 px per cell). One `<OBJECT>` in each cell, same design, same size, same position across all frames. No hero,
> hands, text, labels, cell dividers, ground, cast shadow, pools, circular auras, glow, sparks or background.
> Animate ONLY: `<MOTION>`. All other detailing is identical in every cell.
> Use at least 12% completely transparent margin inside EVERY cell on all four sides. Isolate every frame without
> overlap. Real transparent background, no checkerboard painted in. Readable at tiny game scale.

## Items

| Hero | File | N | Object | What moves (frame by frame) | Where the game shows it |
| --- | --- | --- | --- | --- | --- |
| Lantern Knight | `LanternKnight_LighthouseLens.png` | 4 | Brass lighthouse lens housing with glass prisms, seen from the side | The lens drum turns a quarter per frame; the prisms catch a different face each time | On the lamp during Lighthouse, turning with the beam |
| Drowsy Seer | `DrowsySeer_MoonPhase.png` | 4 | A small porcelain moon charm on a ribbon | Crescent, half, gibbous, full | Over its head, waxing as the gauge fills |
| Root Oracle | `RootOracle_RootTip.png` | 3 | A gnarled root tip with bark and thorns, pointing up | Just breaking the floor; risen with thorns spread; drawing back | Where its roots strike and where the Living Tree steps |
| Lone Jelly | `LoneJelly_Organs.png` | 3 | A remembered organ floating inside gel | A tiny biting mouth; a frost gland; a stitched limb | Inside the jellies and the hero, matching its approach |
| Glass Dice | `GlassDice_Die.png` | 6 | One glass die with red pips, three-quarter view | Faces 1 to 6 on top | Over its shoulder showing the next face; the dice it rolls |
| Tide Singer | `TideSinger_WaveCrest.png` | 3 | A curling wave crest of painted water with foam | Rising; curling over; breaking into foam | The crest of its waves and of High Tide |
| Comet Runner | `CometRunner_Comet.png` | 3 | A comet head: a rock wrapped in a short flaring tail | The tail long, short, long, flickering in shape | Its meteors falling and its finale |
| Verdant Matron | `VerdantMatron_Bud.png` | 3 | A flower bud on a short stem with two leaves | Closed; half open; in full bloom | On its sprouts, opening toward the next bloom |
| China Marionette | `ChinaMarionette_Shard.png` | 3 | A porcelain shard with a gold-filled crack | Three different shard shapes | Its shards around it and the counterpieces it throws |
| Soap Skater | `SoapSkater_IceChips.png` | 3 | A small spray of ice shavings off a skate blade | Thrown up; at its height; falling | Behind it on sharp turns and on the Ice Rink |
| Velvet Regent | `VelvetRegent_Banner.png` | 3 | A small royal pennant on a gilded pole | Furled; waving; raised high | Over the friends it commands and knights |
| Button Cavalier | `ButtonCavalier_Buttons.png` | 3 | One ornate brass coat button | Still; tilted catching light; popping off | On its chest counting poise; buttons popping on a riposte |
| Night Maw | `NightMaw_Jaw.png` | 3 | A pair of night-dark jaws with cream teeth and a wine-red tongue | Closed; wide open; snapping shut | Its bites and The Feast's mouth |
| Thorn Treasurer | `ThornTreasurer_ThornCrown.png` | 4 | A small crown of brass and thorns | One, two, three, four thorns grown | Over its head, one thorn per thorn held |
| Ivory Sovereign | `IvorySovereign_Decree.png` | 3 | An ivory parchment decree with a violet wax seal | Rolled; half unrolled; fully open | In its hand, and unrolling over the field for a decree |
| Patchwork Host | `PatchworkHost_Curtain.png` | 3 | A small patchwork stage curtain on a brass rail | Closed; parting; open | Over the featured friend during Showtime |
| Archive Curator | `ArchiveCurator_Book.png` | 4 | A thick old archive book, three-quarter view | Closed; opening; open; a page lifting | Beside it while waiting; open during Living Exhibit |
| Penny Broker | `PennyBroker_Receipt.png` | 3 | A paper receipt slip | Printing out of a slot; torn off; stamped | Each receipt strike it fires |
| Storm Fencer | `StormFencer_Sheath.png` | 3 | Its scabbard, side view | Plain; charged with blue-white runes lit; runes blazing | At its hip, lighting up as static gathers |
| Tin Virtuoso | `TinVirtuoso_MusicStand.png` | 3 | A small tin music stand with a score | The score's page turning in three steps | Beside it, turning a page on every phrase |
| Moon Prowler | `MoonProwler_ShadowCat.png` | 3 | A cat-like shadow silhouette in deep blue ink | Crouched; slinking; pouncing | The shadow that slips behind its target |
| Lantern Weaver | `LanternWeaver_Lantern.png` | 3 | A small paper lantern on a thread loop | Unlit; lit (painted warm paper only, no glow); swaying lit | Hanging on the threads between pinned targets |
| Rain Archivist | `RainArchivist_Cloud.png` | 3 | A small painted storm cloud | Gathering; full and heavy; dark with a lit edge | Over the spot its rain falls on |
| Sundae Duchess | `SundaeDuchess_Cherry.png` | 2 | A glossy cherry with a stem | Round; squashed on landing | On top of each scoop that lands |

Totals: 24 sheets, 78 frames (sum of the per-sheet requests). At 256 px per cell the set comes to roughly 2–3 MB, which is inside the build-size
budget.

## Delivered 24/10/2026

All 24 sheets / 78 frames made, normalized to 256px cells and imported into Unity. See [delivery](Hero_Effect_Art_Delivery_2.md), [contact sheet](../PreviewExports/HeroEffects/ContactSheet2.jpg), [prompts and refs](Hero_Effect_Art_Manifest_2.json), [alpha/layout verification](Hero_Effect_Art_Verification_2.json) and [Unity import verification](Hero_Effect_Art_Unity_Verification_2.txt).
