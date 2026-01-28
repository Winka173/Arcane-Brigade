# Shop focus, rarity and ground contact

## Changes

- Continue moved to its own lower-right lane at `(6.1, -3.9)` before the existing layout lift. Its interaction radius is 0.8, versus merchandise's 1.1. Stock/exit interaction regions are disjoint. Optional Keeper entry remains separate. The lower-right decorative shelf is omitted in shops so the exit is not buried in scenery.
- Eligible altar fusion uses one visible lower-left station rather than positioning it according to its index in the entire recipe catalog.
- Item and companion inspection now grows the merchandise about 18%, with a short settling bounce and warm outline. Reduced Motion uses a static 12% emphasis. Stand roots do not scale; merchandise feet remain on the painted cushion.
- The per-station approach state is now wired into the actual displayed actor; previously only the merchant received `Inspect`, leaving merchandise focus animation dormant. Idle merchandise no longer translates vertically off its cushion; breathing/pose changes and the foot-pinned focus bounce provide motion instead.
- Rarity uses 16 original painted VFX cells: four animated poses each for blue, purple, gold and rainbow. Crimson uses the gold structure with a crimson tint. Two sprite layers crossfade; a broader soft spill and motes reinforce the colour. The old segmented target-like rings are removed. Reduced settings stop decorative movement and/or soften brightness, retaining colour identity.
- Rarity follows the displayed sprite, including focus, entrance, stock departure, reroll and fading. Hidden stock disables and clears the glow. Item acquisition may retain its existing brief pickup effect.
- Secret-room portals keep their softer non-stock light treatment; the new oversized painted merchandise aura is not applied to environmental portal hints. Windows/curtains remain architectural background layers so they cannot cover the shopkeeper's face.
- Purchased companions no longer receive `RecruitedAllyRarity`. `ShopRarityGlow.CanDecorate` also rejects every `UnitVisual` target, preventing accidental reattachment to owned/combat actors. Rarity stats are not removed or nerfed.
- Ground placement uses visible ink contact, including scale/rotation and non-uniform parents. Foreground ordering uses the contact point. The three painted stand styles have cushion-specific support fractions instead of the old generic 78%-height estimate.
- Cushion support is calculated from the current stand pose and its fixed ground anchor, not its previous-frame transform. This removes a LateUpdate-order dependency that could separate merchandise from the cushion during focus. Native contact assertions run after LateUpdate and retain a 0.04-world-unit tolerance.
- Removed redundant fixed, offset dark puddles beneath stations; grounded actors keep their contact/soft/cast shadows. Directional shadows on shop fixtures and landmarks are pinned to the actual foot.
- Both old wall-mounted toy shelves have original freestanding cabinet replacements with visible supporting feet. Existing character/toy identities and nightmare variants are retained.
- October's door keeps its own frame selection but now follows shop entrance/departure fade/offset, anchors its threshold and owns a proper ground shadow. It no longer overwrites lifecycle fade with opaque white every frame.
- Character acting import preserves the 21 additional October champion entries instead of replacing the library with the original ten heroes. Preview checks accept actual clip lengths.

## Art provenance

Built-in image generation; transparent PNG alpha copied unchanged into the project. Unity handles slicing and imports. No other game's artwork or external audio is imported by this pass.

| Project asset | Generation source |
| --- | --- |
| `Assets/ArcaneBrigade/Art/ShopPresentation/RarityBloom_v1.png` | `exec-caa4cf25-4091-4b6f-9551-c1ea40689ccb.png` |
| `Assets/ArcaneBrigade/Art/ShopPresentation/GroundedCabinets_v1.png` | `exec-0c85d7c6-de0d-4811-b0f7-9e6302183865.png` |

Generation directory: `ArtSource/`. Runtime uses project copies only.

Rarity prompt: production four-column/four-row transparent VFX atlas; rows blue, purple, gold and rainbow, columns gather/blossom/full radiance/settle. Exquisite painted haunted-toyshop magic, feathered light rays, diamond sparkles and soft spill. Broad recognisable light but empty centre for merchandise. Organic wisps, not target circles/sigils. Equal centres, generous transparent separation, no characters, weapons, text or baked ground shadow.

Furniture prompt: reference `Art/Nightmare/NightmareProps_v2.png`, third-row two toy shelves. Redesign as two freestanding compact cabinets with physical feet, friendly walnut/gold and nightmare ivory-claw variants. Preserve bunny, rocking horse, castle globe, doll and star. Match crisp storybook outline/palette and gameplay perspective, no backdrop/baked shadow. Two equal isolated cells on genuine alpha with full feet/margins.

## Validation

- Unity entry point: `ArcaneBrigade.Editor.ShopPresentationBuilder.Gate`. Imports art, regenerates visible ink metrics, runs focused grounding/focus/rarity checks, preserves/checks all champion acting, animation/ensemble checks, October and story regressions, then builds Windows.
- Dedicated isolated native review: `Tools/Nightmare/Capture.ps1 -Shop -Player Builds/FullRun/ArcaneBrigade.exe -Output <root>/Final`; add `-Reduced` and use `<root>/Final-Reduced` for accessibility.
- Review covers actual approaches, all three rarity colours, item/ally focus with support contact, owned ally without aura, deliberate separate exit, departure and no stock aura in real combat.
- The generic K7 runner explicitly excludes this dedicated capture flag; there must not be two runners modifying the diagnostic scene/storage.
- Story capture expects five base shop landmarks, reflecting the deliberately omitted sixth decoration in the exit lane; the remaining nightmare landmarks and perimeter architecture are still checked.
- Final native findings, counts and artifact hashes belong in `PreviewExports/ShopPresentation/Gate_Summary.md`. Automated checks are not human visual approval or mobile-device certification.

## Broader animation scope remains separate

This shop delivery is not completion of individually redrawn animation for the whole expanded roster or all weapons. `PerformanceExpansionBuilder.Audit` exports the actual 31 heroes, 66 companions, 28 enemies and 57 weapons, reference boards and a source-backed job manifest under `PreviewExports/PerformanceExpansion`. It identifies 16 companion-acting sheets, 46 low-unique-pose locomotion sheets, 14 normal/evolved enemy reaction/death sheets, 12 boss sheets and 13 weapon sheets: **101 total**, correcting the previously misstated total of 95. The subsequent expanded-performance pass has its own import ledger and completion status; see `Expanded_Performance_Pass.md`. Existing October pose art remains active.
