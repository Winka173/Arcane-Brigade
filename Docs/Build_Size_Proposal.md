# Build size: measurement and proposal

Measured 19/10/2026, Unity 6000.4.8f1. This is a proposal only: no import setting or art has been changed.

- Per-texture data: `Docs/Build_Size_Textures.csv` (1195 build textures: today's size and format, the measured sprite cell or ink size, the largest on-screen size, the proposed PC and phone size, and the projected MB).
- Re-measure with **Arcane Brigade > Reports > Build size report**, or in batch mode: `-executeMethod ArcaneBrigade.Editor.BuildSizeReport.Run -reportOut <file.tsv>`. It is read-only. It lists every texture and sprite object pulled in by the four build scenes and the Resources folders.

## 1. Where the 4.5 GB goes today (Windows build)

| File | Size | Content |
|---|---|---|
| `resources.assets.resS` | 4.07 GB | Pixel data of 1127 textures |
| `resources.assets` | 307 MB | 254 MB inline pixels of the 79 *readable* textures; 50 MB sprite meshes (4810 Tight sprites, 1.8 M vertices, 0.57 M physics-shape points); 2.5 MB fonts |
| `resources.resource` | 63 MB | Audio (already Vorbis) |
| Engine, Managed, D3D12, MonoBleedingEdge | ~90 MB | Code and runtime |

Textures are 97% of the build: 4340 MB in the current import. **704 of 1196 are stored as raw RGBA32 (3770 MB).**

### Why they are uncompressed

| Cause | Textures | MB today |
|---|---|---|
| A builder sets **Uncompressed** (CheckpointBuilder, K25AtlasImporter, RarityFrameArtImporter, K10 and others) | 208 | 1286 |
| CompressedHQ is set, but a side is **not a multiple of 4** (1254, 887, 1774, 1145, 1374, 1227…). Unity silently falls back to RGBA32. | 307 | 1954 |
| CompressedHQ is set, but the texture is **NPOT with mipmaps** (384x1024, 768², 768x512, 1024x640, 1536x1024). Unity also keeps these as RGBA32; every compressed NPOT texture in the build has mipmaps off. | 189 | 529 |

On top of that, the art is stored far larger than it is ever drawn:
- 1254² images for shots drawn at 114–181 px.
- 887x1774 rarity-frame strips drawn 94 px wide.
- 1254² pickups drawn at 133 px.
- A full 1254² RGBA32 texture for every frame of the Act One bosses and the Last Customer: 82 frames, 650 MB.

### Largest groups today

| Group | Files | MB |
|---|---|---|
| Bosses: Act One boss frames, Last Customer frames, PerformanceExpansion bosses, K21, Keeper | 118 | 827 |
| Shots, projectiles, thrown props (Arsenal, CombatVFX shots) | 201 | 653 |
| Hero / companion / enemy sprite sheets | 195 | 644 |
| Act One props, pickups, items, travellers | 62 | 358 |
| Combat VFX: rings, flashes, puffs, hazards | 53 | 346 |
| Held and awakened weapons | 147 | 298 |
| Hero gauges and order maps | 108 | 257 |
| Rarity frame pieces | 25 | 200 |
| Unused in play | 81 | 196 |
| Backdrops, floors, story panels | 32 | 194 |

## 2. Largest on-screen size per group

The UI is all OnGUI. The 1920x1080 design space scales by 1.0 at 1080p, 1.33 at 1440p and 1.0 on a 2400x1080 phone (pillarboxed, 16:9). The menu space is 1280x720.

World sprites use the orthographic camera at half-height 3.9 in combat, 4.8 in the shop, 6.5 for the Last Customer, 7 for the Keeper and 6.94 in the waiting room. In combat that is **185 px per unit at 1440p and 138 px per unit at 1080p**. Gameplay never zooms in further.

The target size below is the source size the image needs. For PC it is the largest draw at 1440p × 1.35; for phone it is the largest draw at 1080p × 1.1. Nothing is ever proposed larger than today's source.

| Group | Largest draw, 1440p / phone (px) | Measured on | PC target | Phone target |
|---|---|---|---|---|
| Hero / companion / enemy sheets | ~200 normal; ≤400 with body-size items, mutation or empowered (WardrobeCrab 511) / 300 | Cell height | cell ≤540: **unchanged** (cells are 240–330 today) | unchanged (~330) |
| Bosses (PerformanceExpansion, K21, Act One) | ≤1080 (ChairPorter, SuitcaseLeviathan) / 810 | Cell height or ink | **unchanged** (cells ≤1254 or smaller) | ≤1024 |
| Last Customer frames (camera 6.5) | 577 / 433 | Ink height | ~780 | ~480 |
| Keeper body / carrier parts | 542 / 407; carriers 1329 / 997 | Rect | 730 / unchanged | 450 / 1024 |
| Boss and Keeper organs | ≤392 / 294 | Rect | 530 | 320 |
| Held weapons (companion melee tier 3) | 528 / 396 | Longest ink side | 712 | 436 |
| Awakened weapons | 591 / 443 | Longest ink side | 800 | 488 |
| Allied shots / hostile shots / Arsenal thrown | 114 / 181 / ~256 (phone 86 / 136 / 192) | Longest ink side | 154 / 244 / 346 | 96 / 150 / 212 |
| Boss props (trademarks) | 406 / 305 | Longest ink side | 548 | 336 |
| Telegraph ring / lane, death flash | ≤1170 (screen-capped) / 880 | Longest ink side | ~1536: unchanged | ~1000 |
| Death ring, ink puff | 960, 645 / 720, 485 | Longest ink side | 1296, 870 | 792, 534 |
| Hazards, shadow pool | ~440 / 330 | Longest ink side | 600 | 365 |
| Hero hurt, chips | 169, 110 / 127, 83 | Longest ink side | 228, 148 | 140, 91 |
| Hero gauges (world) | 166 / 124 | Ink height | 224 | 136 |
| Gauge order maps | same as the gauge | — | same size as its gauge | same as its gauge |
| Rarity strip (Side, Glow) | 94x188 / 70x141 | Width | **128x256** (keep 1:2, tileable) | 80x160 |
| Rarity corner / crest / foot | 80 / 472 / 236 (phone 60 / 443 / 221) | — | 108 / 640 / 320 | 66 / 488 / 243 |
| Item icons | 252 / 189 (museum detail, shop card 192) | — | 340 | 208 |
| Relic sheets | 332 / 249 | Cell | 448 | 274 |
| Bond icons | 475 / 356 (level cut-in) | — | 640 | 392 |
| Stat icons | 72 / 54 | — | keep ≥128 | 128 |
| Act One zone props | 517 / 388 | Ink height | 700 | 427 |
| Pickups, memories / evidence items | 133 / 167 (phone 100 / 125) | — | 180 / 225 | 110 / 138 |
| Hob, Latch, Postscript, Tally (Lives page) | 627 / 470 | — | 846 | 517 |
| Seated travellers, chairs, tea spill | 305–400 / 228–300 | — | 412–540 | 250–330 |
| Story panels / ending panels | 960 / 1392 wide (phone 720 / 1044) | — | 1296 / unchanged | 792 / 1148 |
| Full-screen WaitingRoom, Journey backdrops, NightmareEnding | full screen | — | **unchanged** (already *below* 1440p) | unchanged |
| Floors | one span 1477x923 / 1108 | — | unchanged | ~1220 wide |
| Map architecture, furniture, visitors | 591 / 427 (phone 443 / 320) | — | 800 / 576 | 487 / 352 |

Heroes, companions, enemies, bosses and full-screen backdrops keep their current pixels on PC. Only compression changes there; BC7 is visually lossless for this painted art. The loss of resolution falls on art that is drawn small: shots, VFX, icons, frames, gauges, pickups, props and per-frame boss textures that are larger than any draw.

## 3. Per-platform import policy (one source, overrides per platform)

| | Standalone (PC) | Android / iOS |
|---|---|---|
| Format | BC7 (sRGB) | ASTC 6x6. ASTC 8x8 for soft VFX, floors and backdrops. |
| Max size | The source size (resampled to the PC target) | Power-of-two override ≈ phone target (Unity steps are powers of two, so phone size is +0–15% over the exact target) |
| Mipmaps | Keep the current choice. A texture with mipmaps must be **power of two**, or Unity will not compress it. | Same |
| Sizes | Every side a multiple of 4 (pad with transparent pixels: lossless) | Same |
| Readable | **Off** for all 79. Today they double RAM and sit inline in `resources.assets`. Runtime code only reads screenshots. | Off |
| Sprite mesh | Tight, with lower tessellation detail on large VFX and bosses; **physics shape off** (nothing uses colliders) | Same |

Enforce this with one `AssetPostprocessor`, so that the builders listed above can no longer force Uncompressed, readable or 4096 settings.

**Exceptions:**
- **Gauge order maps** (`Art/Arsenal/Generated/Gauge2_*_Order.png`) are data read by GaugeReveal. They need R8 (or RGBA32), linear, point filtering, no mipmaps and no block compression, resampled with nearest-neighbour at exactly the size of their gauge.
  - **Today's import is already wrong**: BC7, sRGB and mipmaps, against what GaugeOrderBaker intends. Rebake them with GaugeOrderBaker after the gauges are resized.
- **Fonts:** dynamic TTF (Alpha8 atlases). Leave them.
- **Checkpoint Disc and Square** (tiny). Leave them.

## 4. Scenario table (MB)

Columns: textures, then the whole app (textures + sprite meshes + audio + engine).

| Scenario | PC textures | **PC app** | Phone textures | **Phone app** |
|---|---|---|---|---|
| **A** today | 4340 | **≈4.5 GB** | (same art: not shippable) | ≈4.4 GB |
| **B** compression only (BC7 / ASTC 6x6 at today's sizes, sides fixed to a multiple of 4 or power of two) | 1504 | **≈1.7 GB** | 680 | ≈0.83 GB |
| **C** B + resample to display size + trim padding of single images | 805 | **≈1.0 GB** | 280 | ≈0.43 GB |
| **D** C + drop unused art | 755 | **≈0.95 GB** | 257 | ≈0.40 GB |
| **E** D + phone tuning (ASTC 8x8 on soft VFX, floors and backdrops; music Vorbis q 0.5; lower sprite tessellation) | 755 | ≈0.95 GB | 247 | **≈0.35 GB** |
| **E+** E + repack atlas sheets without their transparent padding (sheets are 30–40% empty) | 591 | **≈0.78 GB** | 182 | **≈0.29 GB** |

Non-texture parts:
- PC: sprite meshes ~42 MB (B: 50), audio 63 MB, engine ~90 MB.
- Phone: sprite meshes ~20–40 MB, audio 45–63 MB, IL2CPP arm64 engine ~45 MB (APK).

**Realistic budgets:**
- **PC ≤ 1 GB is reached at C/D (~0.95 GB)** with heroes and bosses untouched. Repacking the sheets brings it to ~0.78 GB.
- **Phone ≈ 300 MB needs E+.** Without repacking the sheets, the phone lands at ~350 MB. Hero/companion/enemy sheets are then the largest phone item (121 MB → 77 MB repacked).

### Per-group projection (MB)

| Group | Files | A today | B | C | D (PC) | E+ repack (PC) | B phone | E phone | E+ phone |
|---|---|---|---|---|---|---|---|---|---|
| Bosses (bodies, frame sequences) | 118 | 827 | 208 | 137 | 137 | 134 | 93 | 38 | 36 |
| Shots, projectiles, thrown props | 201 | 653 | 285 | 19 | 19 | 18 | 127 | 3 | 3 |
| Hero / companion / enemy sheets | 195 | 644 | 307 | 301 | 301 | 193 | 136 | 121 | 77 |
| Act One props / pickups / items / travellers | 62 | 358 | 100 | 22 | 22 | 20 | 44 | 5 | 5 |
| Combat VFX | 53 | 346 | 98 | 26 | 26 | 26 | 43 | 4 | 4 |
| Held + awakened weapons | 147 | 298 | 137 | 70 | 70 | 52 | 61 | 22 | 14 |
| Hero gauges + order maps | 108 | 257 | 74 | 17 | 17 | 17 | 44 | 7 | 7 |
| Rarity frame pieces | 25 | 200 | 50 | 1 | 1 | 1 | 22 | 0 | 0 |
| Unused in play (dropped in D) | 81 | 196 | 50 | 50 | 0 | 0 | 22 | 0 | 0 |
| Backdrops, floors, story panels | 32 | 194 | 61 | 55 | 55 | 55 | 27 | 12 | 12 |
| Icons (items, relics, stats, bonds) | 100 | 131 | 39 | 25 | 25 | 20 | 17 | 8 | 6 |
| Other sheets (kept size) | 36 | 104 | 41 | 40 | 40 | 27 | 18 | 9 | 6 |
| Scenery, shop stands, visitors | 21 | 67 | 31 | 30 | 30 | 17 | 14 | 13 | 7 |
| Boss / Keeper organs | 16 | 64 | 24 | 13 | 13 | 11 | 11 | 4 | 4 |
| **Total** | 1195 | 4340 | 1504 | 805 | 755 | 591 | 680 | 247 | 182 |

## 5. Unused art that ships today

Everything in a Resources folder ships, along with everything it references.

| Art | Files | Today | Why it is unused |
|---|---|---|---|
| `Art/Arsenal/Gauge_*_Empty/Full_v1` | 72 | 144 MB | All 36 heroes have Gauge2. `HeroAwakeningVisual.cs:55-57` only falls back to the old gauges when Gauge2 is missing. They are still listed in ArsenalArtwork. |
| `ActOneBosses/*_Atlas`, `LastCustomer_{BorrowedFace,Pleasant}_Atlas` | 5 | 30 MB | Referenced **only** by `Resources/SpriteInkMetrics.asset`; play uses the single-frame textures. |
| `AnimationHD/ShopFixtures_v1` | 1 | 6 MB | Shop fixture support is disabled (`WorldShop.cs:604`). |
| `MienCho/OpenDawn`, `Encounters/BreathingPhone`, `LostShop_v1` | 3 | 16.5 MB | Replaced by Panel_Dawn and Prop_StoryTelephone; LostShop is only a fallback for WaitingRoom. Remove the code fallbacks with them. |

Also:
- **SpriteInkMetrics.asset sits in Resources and references 731 textures.** It should only bake sprites that something else uses, or it will keep dead art in every build.
- Cells inside atlases are never shown (StrangeMerchants ReceiptMaw/EggOracle frames). They only go away with a repack.

## 6. Images to check by eye, or redraw at the smaller size

Resampling to 1.35× the largest draw keeps what the player sees today: the GPU already shows these images that small, and an offline Lanczos resample with premultiplied alpha is cleaner than runtime minification. These images need a look anyway:

- **Rarity frame pieces** (887 → 128 wide, about 7×):
  - Fine gold filigree may turn to mush.
  - Side/Glow must stay seamless when tiled vertically and keep exactly 1:2.
  - A redraw at 128x256 with bolder lines is the safe route.
- **Images with lettering:**
  - Arsenal `HomeRunText`.
  - Expansion `Item_PostageStamp`, `Item_RefundSlip`, `Item_LateFee`, `Item_TwinTicket`.
  - Act One `Item_Receipt`, `Item_SewnLetter`, `Prop_WaitingNote`, `Prop_BurntContract`, `Prop_SewnReply`, `Memory_*`.
  - Any text meant to be read needs a redraw with larger lettering at the target size.
- **Stat icons** (drawn at 54–72 px): keep them at ≥128. Bolder silhouettes would read better than today.
- **Thin-line shots** (`Thread_v1`, ArrowBolt, KunaiShot, TwigDart, HarpoonShot): lines approach 1 px. This is the same as today on screen, but worth a glance.
- **Already too small for 1440p (candidates to repaint larger, ~+20 MB BC7):**
  - `MienCho/WaitingRoom` and `WaitingRoom_ChairTurned` (1672x941, full screen).
  - `Journey/*_Painted_v1` (1448x1086, full height).
  - `NightmareEnding_v1` (768x512 quarters stretched to 1392x928).

## 7. Code and data that must follow a resample

- **Sprite rects in multi-sprite `.meta` files:** scale them by the same factor.
- **Pixels per unit:** scale it by the same factor, so world sizes, `SpriteInkMetrics` ink rects and `HeroAwakeningVisual` cuts stay valid. Rebake SpriteInkMetrics afterwards.
- **Trimming single images:**
  - Crop to the alpha bounding box plus 4 px and shift the pivot so the image does not move.
  - A frame sequence uses one shared crop for the whole sequence.
  - Unit bodies are scaled from the full rect (`UnitVisual.cs:474-481`), so set `AuthoredMotion.IllustratedHeight` to the old height before cropping.
- **`PaintedStatIcons.asset` Ink/BadgeInk rects are in pixels.** Scale them too. They are dormant today because the Stat_* files win.
- **`NightmarePresentation.cs:197`** draws the whole `Panel_Thursday` texture. Never pack it into an atlas.
- **Gauge order maps:** rebake with GaugeOrderBaker at the new gauge size.

## 8. Suggested order of work

1. Import policy as an AssetPostprocessor with Standalone, Android and iOS overrides: sides a multiple of 4 or power of two, readable off, physics shape off. Then fix the builders that force Uncompressed or readable.
   - Gets to ~1.7 GB on PC.
2. Drop the unused art (section 5) and prune SpriteInkMetrics.
3. Resample tool: Lanczos with premultiplied alpha, rescaling meta rects and pixels per unit, then rebake SpriteInkMetrics and the gauge order maps. Use the targets in `Build_Size_Textures.csv`.
   - Gets to ~0.95 GB on PC and ~0.35 GB on phone.
4. Phone tuning (ASTC 8x8 for soft art, music quality, tessellation), then repack the sheets without padding.
   - Gets to ~0.78 GB on PC and ~0.29 GB on phone.
