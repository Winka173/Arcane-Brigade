# Identity-specific character acting

## Runtime delivery

Ten playable champions now have twelve additional original painted poses each: four curiosity/look/blink poses, four celebration poses, two hurt poses and two tired/defeat poses. They are additional acting, not replacements for the existing walk and attack animation. Lantern Knight's existing twelve-pose authored combat animation remains intact.

Priority is defeat > actual hurt > actual attack/movement > celebration > curiosity > existing idle. Hurt acting follows `LastHit`; celebrations follow the existing real wave-clear event. Rest gestures use independent deterministic actor phases. No combat/shop random stream, simulation timing, movement control, damage, economy or save data is changed.

The five companion bodies which actually share the corresponding champion identity also use these poses: DiceCrab, BubbleKoi, CometKite, MirrorNewt and BrassAcorn. Unrelated companions do not inherit another character's face or costume. Reduced Motion retains the existing stable pose rather than cycling the new acting sheets.

Collection previews add LOOK, CHEER, FLINCH and TIRED only for compatible bodies. The controls occupy two rows; star controls and the description move down with actual padding. Enter/A cycles the available pose count, including boss PHASE. Preview overrides are restricted to isolated collection actors or diagnostic mode and are cleared on reuse.

Death feedback normalizes source/frame height and foot pivot when changing pose so differing atlas pixels-per-unit cannot enlarge the corpse or move its grounded shadow. Painted defeat frames receive only 22% of the existing material squash and 20% of its tilt: the tired expression remains readable instead of being flattened into a pancake. Species without the new art retain their existing material treatment. This is presentation-only; companions retain their existing invulnerability.

## Mechanical and shop follow-through

In addition to separately hinged scissors, three inspected source weapons receive explicit region rigs: SleepCamera's extending lens, MusicBox's winding key and MailboxLauncher's hinged flag. Their parent weapon, collision, projectile emission and timing stay unchanged. The source shader hides only the articulated region; the moving part receives the original full UV bounds. Reduced Motion restores the intact source. Runtime-owned cropped sprites are disposed with the actor.

Gold and dream merchants have distinct event-timed anticipation, flourish and recovery rather than an indefinitely looping purchase wobble. Refusal settles independently. Their existing painted identities, prices, merchandise/aura parenting, carpet and speech are retained. This is procedural source articulation, not a newly painted hand-to-item grabbing clip.

## Original art and prompts

Built-in image generation, reference-guided identity preservation; transparent alpha retained. All selected PNGs are in `Assets/ArcaneBrigade/Art/Acting/`, not external-only dependencies. Common prompt specification:

> Twelve full-body poses, four columns by three rows, generously padded transparent cells, consistent grounded baseline, original clean ink hand-painted toyshop style. Row one: neutral, look left, look right, blink. Row two: celebration preparation, lift, peak, settle. Row three: hurt recoil, recovery, tired slump, eyes closed but alive. Preserve the reference identity and accessories throughout. No arms, hands, weapons, ground, text, shadows or targeting halos.

Reference and identity constraints:

| Output prefix | Reference | Identity constraint | Accepted generation |
| --- | --- | --- | --- |
| LanternKnight | `Art/K16/LanternKnight_v1.png` | Teal lantern, ivory face panel, flame, blue cape, gold clasp, black feet | `exec-7fad8743-53cb-4dc4-9e40-c5117f7da7eb.png` |
| Postmaster | `Art/AnimationHD/HeroIdentity_v1.png`, row 2 | Yellow mail-box, red flag, envelope marking, brown boots | `exec-2081276e-1a02-472b-ba8c-467c81d1a968.png` |
| DrowsySeer | Same reference, row 3 | Lavender stitched cushion, crescent forehead, purple sleep cap, yellow star, ruffle, teal feet | `exec-83672440-88b8-43d3-a63a-761fe11eabcd.png` |
| RootOracle | `Art/AnimationHD/BrigadeAnimation_v3.png`, row 5 | Purple patched moon-hood doll, short ivory eyes, dark feet; not a new animal | `exec-94a090eb-e3b1-439a-b95f-7d89ea7be92f.png` |
| LoneJelly | HeroIdentity, bottom row | Coral crowned jelly king, angry brows, scalloped body, brown boots; no tentacles | `exec-594e9b22-9671-4f73-a04c-41bdafc15e8a.png` |
| GlassDice | `Art/K10/DiceCrab.png` | Cream dice, lavender/coral pips, teal boot-feet, brass joints; express through pips, no human face | `exec-dd6a67ef-2d6d-4092-beb2-5c5c53a5e4fc.png` |
| TideSinger | `Art/K10/BubbleKoi.png` | Mint koi-bird, cream belly, coral fins, amber eyes, tiny golden feet | `exec-014fa487-4c1e-4633-901e-9ce96fcaa9f5.png` |
| CometRunner | `Art/K10/CometKite.png` | Coral diamond kite, ivory star, purple folded feet, mustard ribbon tail | `exec-314a2473-b7b2-41e7-91a6-49367c948550.png` |
| MirrorKeeper | `Art/K10/MirrorNewt.png` | One-eyed turquoise mirror, ornate lavender frame, peach gems, golden feet; no second eye/mouth | `exec-6fa9607e-4a4e-40dd-bd5a-57d54b21f1b7.png` |
| TreasureAcorn | `Art/K10/BrassAcorn.png` | Brass acorn, green envelope hat, round seal, oval eyes, brown boots; no mouth | `exec-5cac7c18-79ad-40ef-bf01-c146f846a162.png` |

Each output has suffix `_Acting_v1.png`. Source generation directory: `ArtSource/`. Runtime uses only project copies. Unity's connected-component importer preserves detached flames/feet and the complete pose margins; it does not assume generated cells are pixel-perfect.

## Verification and boundaries

Canonical import/build gate: `ArcaneBrigade.Editor.AnimationPolishBuilder.Gate`. Focused acting checks cover all 120 complete frames, foot pivots, margins, priority, aliases, cue endpoints and accessibility. Native animation captures cover all ten identities, both ends of each acting cue, real shop/pause/start-stop, weapon mechanisms, pool reuse, boss acts, enemies and real collection layout. The separate story capture checks final Customer and both ending branches.

The native actor board is a controlled visual fixture, not a natural full campaign. Passing assertions does not certify subjective smoothness, audio listening, device performance or every possible crowded-endless situation. This pass completes the defined champion acting set and targeted mechanical rigs; it does not claim bespoke hand-painted clips for every companion, enemy evolution, merchant grabbing action or every weapon in the catalog. Those retain their existing authored/procedural animation rather than unsafe arbitrary anatomical cuts.
