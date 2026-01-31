# UI "Lantern & Letter" — rework for Miền Chờ (16/10/2026)

Replaces the "Midnight Carnival Poster" UI, which no longer fits the story in [Mien_Cho_Story_And_Shared_Light.md](Mien_Cho_Story_And_Shared_Light.md). English is the primary UI language.

## Language

| Element | Meaning in the story | UI rule |
| --- | --- | --- |
| Lantern amber (`ArcadeSkin.Accent`) | your light, your company | selection, primary buttons, HP, focus, keylines |
| Candle cream (`Cream`) | warmth, readable text | body text |
| Moon (`Teal`) | quiet secondary information | companions/info tags |
| Ember red (`Danger`) | red eyes coming out of the night | **hostile only**: enemy shots, boss bar, boss cut-in. Never used for buttons or HP |
| Letter paper (`Paper`) | unanswered letters, notes | speech bubbles, labels, slips |
| Stitched seams (`Divider`) | letters opened and sewn shut again | dividers, panel seams, header underline |

Shapes: rounded night-glass panels with a thin lantern keyline (no slanted tickets, halftone or offset poster shadows). Motion: slow breathing light and a soft beat swell instead of bounces. Cosmetic bundles change the lantern colour (moon / rose), never to red.

## Typography

- **Young Serif** (SIL OFL 1.1, `Resources/Fonts/YoungSerif-Regular.ttf`, licence `YoungSerif-OFL.txt`) via `ArcadeTypography.Display`: logo, page titles, wave label, cut-ins, postmark number. Title Case reads best (“Who Steps Through Tonight?”).
- **Be Vietnam Pro** Medium/Black: buttons, HUD labels, body (legible at small sizes in dense combat; keeps Vietnamese glyphs available).
- **Noto Sans Mono** Bold: numbers and eyebrows.
- Coiny (carnival display face) is no longer used.

## Screens

- **Main menu = the Waiting Room** (`MienCho/WaitingRoom.png`): the lit door → NEW JOURNEY, the empty chair → CONTINUE, the fallen letter → RECORDS; the hero stands in the room with their own light. Collection / Settings / How to play live in the bottom row, which mirrors every entry in the same order as before (navigation checks rely on it).
- Sub-pages use the dimmed Waiting Room; titles: “Who Steps Through Tonight?”, “Where You Left Off”, “Begin Again?”, “Letters You Kept”, “Everyone You Have Met”, “Light, Sound & Comfort”, “How the Night Works”. Help text rewritten in the new voice with unchanged facts.
- **HUD:** HP bar is lantern-coloured; wave label in serif; pause “The night waits with you”; results “Dawn Has Come” / “Until You Return”; wave clear “The dark draws back”; defeat “Your light goes out…”.
- **Moments of light** (`StageCutIn`): boss “comes out of the dark”, boss defeated “The Night Recedes”, 3★ “shines brighter”, bond level “they stand closer together”. A dark band fades up with a light sweep; play never stops.
- **Wave start** (`WaveStamp`): a round postmark “WAVE n · MIEN CHO” pressed onto the night.
- **Low health:** the night closes in from the edges with a faint ember pulse (melody stems still fall away).
- **First-run coach:** letter-paper notes from the hero; copy mentions red eyes in the dark and that it grows darker deeper down.

Every screen that draws through `ArcadeSkin` (journal, waiting room, events, museum, invoice, shop tooltips) inherits the new look because the public API is unchanged.

## Pass 2 — consistency check and Miền Chờ-only HUD

**Fixed:** ten UI files still hard-coded the old carnival purple / neon teal / pink (menu roster cards and detail panel, HUD panels and boss-bar highlight, Brigade panel, Collection, achievements, loot markers, shop stat feedback, glossary hover, weapon-sandbox background, stat glyphs). They now use skin tokens: `Card` (night blue), `CardLit` (warmed by your light), `Deep`, `Better` / `Worse` (a soft warning, not the hostile red).

**New, specific to this game** (`Runtime/UI/NightHud.cs`):
- **The road to dawn** — one notch per wave of the campaign (`catalog.NormalWaves`), lit behind you, a breathing lantern at your wave, small letters at the waves where journal chapters open (lit once opened, from `MienChoStory`), and dawn waiting at the end: “DAWN IN n WAVES”, “LAST WAVE BEFORE DAWN”; endless shows “THE NIGHT DOES NOT END”. Under the timer in combat, under the wave label in the shop.
- **Red eyes at the edge** — enemies outside the view (within 24 units) appear as a pair of red eyes on the screen edge in their direction, nearest one per direction, brighter when closer, larger for bosses, blinking now and then (no blinking with Reduced Motion). Visible enemies are left to the world's own eyes, so nothing is doubled.
- **Light ring** — one small flame per companion in an arc under the hero portrait (15 shown, “+n” beyond), flickering softly: the shared light made visible.

Still in another session's uncommitted files and therefore not changed here: hard-coded panel colours in `WaitingRoomController.cs`, `SecretEventRoom.cs`, `NightmarePresentation.cs`, `WaitingShopEncounter.cs`, `OutsideStageChoice.cs`, `WorldShop.cs`.

## Pass 3 — Lantern & Letter+ (A1), the long table (B11), a word after defeat (B9), handwritten lines (B3)

**A1 · letters, not panels**
- **Torn paper:** every `ArcadeSkin.Ticket` ends in an irregular torn bottom edge (`TornEdge`, a 48×12 repeating tooth texture) instead of a stitched line.
- **Wax seal:** `ArcadeSkin.Seal(centre, size, letter)` draws an oxblood (`Wax`) poured-wax disc with an embossed serif letter. The focused room label in the Waiting Room carries a “B” seal.
- **Handwriting:** a fourth face, **Caveat** (SIL OFL, static instance at weight 520, `Resources/Fonts/Caveat-Hand.ttf`, `ArcadeTypography.Hand`), is used only for words someone *wrote*: the Waiting Room tagline and hints (“someone is waiting for you…”), the first-run coach notes, companion lines. Rules and numbers never use it.

**B11 · the Brigade panel is a long dining table**
- In the Companions tab the top of the panel is a candle-lit table: one seat per species (up to 14; “+n MORE AT THE TABLE” beyond), back row behind the table, front row before it, “×n” for duplicates.
- Clicking a guest selects their row (the list below scrolls to it); they lift and glow, and a torn-paper speech bubble shows “Name · first summary line” and their own line in handwriting. The list remains below for keyboard / controller use.

**B3 · each companion has their own handwritten line**
- `Runtime/UI/CompanionVoice.cs`: story companions speak their authored banter for the current stretch (`CreatureLives.Banter`); every other species speaks the last sentence of its biography in the first person (“They want…” → “I want…”), with a small table of fixed lines where the pronoun swap would misread (`RainbowTailor`, `SaffronMechanist`, `SpindleWren`, `MonsterMarble`).
- Shown above the facts in the Brigade detail panel, and in the shop tooltip of a companion offer (truncated to fit before BUY).

**B9 · a word after defeat**
- The defeat result no longer only says “Your light goes out…”: a torn note from one companion (chosen deterministically from the run id) with their face and a short comforting line in handwriting (“The door is still there. So are we.”). With no companions, the hero speaks.

## Open

- `WorldShop.cs` and `RunUICopy.cs` remain under another session's uncommitted work; the price slips restyle through `ArcadeSkin`.
