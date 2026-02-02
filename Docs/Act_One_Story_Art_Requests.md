# Act I story rework — art requests

For the image agent. Story canon: [Mien_Cho_Act_One_Story.md](Mien_Cho_Act_One_Story.md). Everything here is new art the rework needs; anything not listed reuses existing sprites (Hob = TeapotPilgrim, Latch = HoodWisp, Postscript = ShellPostman, Tally = PaperDevil, the Keeper = OutsideStageAssets boss, the Shopkeeper = the existing merchant, the menu armchair = `Resources/MienCho/EmptyChair.png`, telephone = `Encounters/BreathingPhone.png`, lanterns = `Lanterns/*`).

## Shared prompt (prefix every object request with this)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: [SUBJECT BELOW]

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Style references: `Assets/ArcaneBrigade/Art/FinalQuality/furniture-00_v1.png`, `Assets/ArcaneBrigade/Art/AnimationHD/NightMarket_v1.png`.

Delivery: single sprites go to `Assets/ArcaneBrigade/Resources/MienCho/ActOne/` with the exact file names below (the existing importer makes every PNG there a Single sprite, PPU 256). Boss atlases go to `Assets/ArcaneBrigade/Art/ActOneBosses/`. Record every image in `Docs/Act_One_Story_Image_Prompts.json` using the same shape as `Docs/Act_One_Image_Prompts.json` (`name, path, source, prompt, transparent, sha256, size`).

Story rules that affect the art:
- Companions are living creatures, never toys. The Ones Who Sat Down are former travellers, not monsters.
- No text painted into any image (receipts, boards, letters are blank; the UI writes the words).
- No gore and no harm to children shown. Fear comes from implication: dust, stillness, a chair, a door, small shoes.

---

## Batch 1 — needed first (shop and story beats)

| File | Subject |
| --- | --- |
| `WarmChair.png` | A small, plain child's wooden bedside chair, slightly crooked, worn honey-brown wood with one repaired leg, a folded burgundy-and-cream quilted blanket on the seat, turned a little toward the viewer. Humble and homely, not ornate — it must read as "a child's chair from a bedroom", clearly different from the ornate shop furniture. Faint warm tone on the seat painted into the wood, no glow halo. |
| `WarmChair_Coins.png` | Edit of the approved `WarmChair.png`: three odd antique coins of different sizes resting on the folded blanket. Nothing else changes. |
| `WarmChair_Jar.png` | Edit of `WarmChair.png`: a small stoppered glass jar on the blanket, with a faint pale ripple inside like a trapped whisper. Nothing else changes. |
| `WarmChair_Phone.png` | Edit of `WarmChair.png`: an old telephone receiver lying on the blanket, its curled cord trailing off the seat edge and ending in nothing. |
| `SeatedOne_A.png` | A former traveller sitting perfectly still on their own mismatched chair (a tall dining chair), seen from behind and slightly to the side, facing away from the viewer. Their body is a gentle dream-form (rounded storybook figure similar in proportion to the playable heroes, no clear face visible), coat and shoulders covered in a thick layer of grey dust, hands resting in the lap. Melancholy, eerie stillness. |
| `SeatedOne_B.png` | Same family: a small rounded traveller on a station bench seat with a suitcase at their feet, dusty, facing away. |
| `SeatedOne_C.png` | Same family: a tall thin traveller on a rocking chair that does not rock, a shawl gone grey with dust, facing away. |
| `SeatedOne_D.png` | Same family: a traveller on a low footstool, knees drawn up, a hat in their hands, dust on the brim, facing away. |
| `SeatedOne_E.png` | Same family: a traveller in a worn office chair, one shoe missing, dust, facing away. |
| `SeatedOne_A_Turned.png` | Edit of `SeatedOne_A.png`: only the head turned over the shoulder toward the viewer; the face is a soft, featureless dusty oval with two faint dark hollows. Body, chair, dust and anchor unchanged. Used once in the whole act. |
| `LastCustomer_BorrowedFace.png` | Edit of the existing Last Customer sprite (`NightmareAssets.Customer`): his face replaced by a kind, gentle, rain-damp face that clearly does not belong to him (slightly too small for his head, edges not quite meeting). Everything else unchanged. Also produce the same edit for every frame in `NightmareAssets.CustomerFrames` (preserve pose and anchor). |
| `LastCustomer_Blank.png` | Edit of the existing Last Customer sprite: the face is a smooth, empty pale surface with no features (his own sold face). Single frame, used after his defeat. |
| `Tally.png` | Edit of the existing PaperDevil companion sprite: neat carved tally marks in groups of five across its body, like cuts in old paper. Nothing else changes. Also edit its animation frames the same way. |

## Batch 2 — story bosses

Each boss: first one approved base sprite, then animation frames edited from it, matching the frame count and layout of the Keeper's frames (`OutsideStageAssets.BossFrames`, 16 frames). Large silhouette readable at boss size.

| Base file | Subject |
| --- | --- |
| `ChairPorter.png` | THE CHAIR PORTER (wave 10). A tall, stooped, spindly porter creature in a faded railway porter's coat and cap, long arms, carrying on its back a teetering tower of mismatched chairs strapped together with luggage straps; on several chairs sit small dusty travellers facing away; the chair at the very top is empty. Weary, menacing, enormous. |
| `DoorThatStayedShut.png` | THE DOOR THAT STAYED SHUT (wave 15). A living front door of a family house standing upright: painted teal wood, brass letterbox slot forming a grim mouth, two heavy door chains swinging like arms, a brass knocker, a peephole like an eye, small pencil height marks on the lower frame. Rooted on its own threshold step. |
| `TheBedside.png` | THE BEDSIDE (hidden boss). The Warm Chair grown huge and wrong: the same small wooden bedside chair stretched tall like a figure, its folded blanket draped like a shawl over a back that curves forward as if leaning closer, chair legs bent like long knees, a soft dark hollow where a face would be. Tender and terrifying. |

## Batch 3 — four zones and hidden arena

Floors are seamless tileable top-down textures (not isolated objects): replace the "Output" block with
`Output: seamless tileable square floor texture, viewed from above at the same angle as the existing arena floors (Art/K13/CarnivalStone_v1.png, Art/October/NightmareFloor.png), 2048x2048, no objects, no shadows, no text.`

| File | Subject |
| --- | --- |
| `Floor_Crossing.png` | Worn antique shop-front flagstones with inlaid brass lines, teal and midnight tones (waves 1–5). |
| `Floor_Platform.png` | Old railway platform boards with a faded cream safety edge stripe and scattered dust (waves 6–10). |
| `Floor_House.png` | A childhood home's wooden floor with a worn burgundy runner rug, scuffs and small footprints in dust (waves 11–15). |
| `Floor_PerfectRoom.png` | An immaculate soft cream bedroom carpet, too clean, faint regular pattern (waves 16–20). |
| `Floor_NightYouHid.png` | A child's bedroom floor at night: wooden boards, a small rug, a sliver of light under where a door would be (hidden fight). |

Zone props (isolated objects, shared prompt):

| File | Subject |
| --- | --- |
| `Prop_BoundaryGate.png` | Ornate brass boundary gate in a stone arch with a blank notice board bolted to it. |
| `Prop_PlatformBench.png` | A long station bench (empty) with iron legs. |
| `Prop_DepartureBoard.png` | Hanging antique departure board with blank flap rows. |
| `Prop_PhoneBooth.png` | Antique wooden platform telephone booth, door ajar. |
| `Prop_DiningTable.png` | Family dining table set for two: one bowl with steam, one empty place before an empty chair. |
| `Prop_NailedLetterbox.png` | Wall letterbox nailed shut with crooked nails. |
| `Prop_InwardWindow.png` | A house window frame whose glass shows the room's own interior instead of outside. |
| `Prop_FrontDoorMarks.png` | (scenery, not the boss) A closed teal front door with pencil height marks on the frame and a chain lock. |
| `Prop_SingleBed.png` | A perfect child's single bed, crisp sheets, one pillow. |
| `Prop_Nightstand.png` | Nightstand with one cup of water at exactly the right place. |
| `Prop_ReceiptChair.png` | The Customer's throne: a chair built from layered paper receipts and brass tacks. |
| `Prop_Ledger.png` | The Customer's huge open ledger on a lectern, blank pages. |

## Batch 4 — items and journal

Items (isolated, shared prompt; also used as relic/journal icons):

| File | Subject |
| --- | --- |
| `Item_SewnLetter.png` | An envelope opened and sewn shut again with coarse thread. |
| `Item_Umbrella.png` | A closed black umbrella, still wet, droplets on the canopy. |
| `Item_ThursdayShoes.png` | A small pair of child's shoes, laces already tied, side by side (relic). |
| `Item_Receipt.png` | A long curled blank paper receipt with a wax-red stamp mark. |
| `Item_ExitKey.png` | An old brass key with a tag (blank). |

Journal panels (16:9 painted scenes, 1672x941, same treatment as `Resources/MienCho/WithheldDawn.png`; full background allowed, no text):

| File | Scene |
| --- | --- |
| `Panel_Crossing.png` | The shop interior at night; a small wooden chair with a blanket in a far corner, turned toward the viewer; eight lanterns beyond the window. |
| `Panel_Platform.png` | An endless station platform lined with dusty seated travellers facing the empty rails. |
| `Panel_House.png` | A childhood hallway, front door with height marks, rain light under the door, tiny shoes by the mat. |
| `Panel_PerfectRoom.png` | A perfect bedroom with one bed; dusty seated figures line the walls facing the bed; one empty chair. |
| `Panel_Dawn.png` | Real dawn over the shop roof; companions around an ordinary table; one plain chair pulled out. |
| `Panel_Thursday.png` | Morning doormat in a modern apartment: an envelope with no stamp. |

Menu variant (edit, keep composition pixel-aligned):

| File | Subject |
| --- | --- |
| `Resources/MienCho/WaitingRoom_ChairTurned.png` | Edit of the menu art `Resources/MienCho/WaitingRoom.png`: only the red armchair on the left is rotated to face the viewer directly, with a fresh dent in the cushion as if someone just sat there. Everything else identical (the menu hotspots are mapped to this composition). |
