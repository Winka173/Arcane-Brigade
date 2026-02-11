# Voice system and speech boxes

The owner asked for two things:
1. Dialogue should appear as a speech box next to whoever is talking, not in a fixed corner. Companions should also say something when they join or rank up.
2. Like the shopkeeper in *Bizarre Brigade*, the shopkeeper and the bosses should speak short voiced lines, with plenty of variants.

This page covers what we could confirm about Bizarre Brigade, how the system works, and every trigger. The recorded script, with exact file names and an studio brief for each character, is in `Docs/Audio/Voice_Prompts.md`. That file is generated from `Runtime/Presentation/VoiceLines.cs`.

---

## 1. Research: Bizarre Brigade (Zenless Zone Zero)

**Confirmed (with sources)**
- *Bizarre Brigade* is a roguelike survivor arcade minigame in the Godfinger Arcade on Sixth Street. It shipped in ZZZ **v1.2 on 25 Sept 2024**. v1.4 added saving mid-run, and v1.5 added the two-player "Shared Dream" mode.
  - [RPG Site, v1.2 launch](https://rpgsite.net/news/16297-zenless-zone-zero-version-12-launches-on-september-25-adding-caesar-burnice-red-moccus-bizarre-brigade-roguelike-minigame)
  - [Fandom: Bizarre Brigade](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade) and its [change history](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Change_History)
- A run works like this:
  - You pick a "Dream Seeker" and a difficulty (3, 6 or 10 waves) and fight "Nightmares" in timed waves.
  - A **Rest Phase** follows each wave, with a walk-in **Store** that sells Companions and Relics.
  - You refresh stock by walking up to a small sign with rotating arrows. The refresh price rises each time.
  - A "Dream Eater's Altar" handles companion merging.
  - Sources: [Game8 guide](https://game8.co/games/Zenless-Zone-Zero/archives/475381), [Fandom Relics](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics), [Fandom Companions](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions)
- The only written mention of a shopkeeper is in Nicole's Dream Seeker effect: *"Nicole asked the shopkeeper to bring out some super cheap relics…"* ([Fandom Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers)).

**Not confirmed by any readable source**
- No wiki, guide or article we could read lists the shopkeeper's voice lines, voice actor, triggers, cooldowns or subtitle style. The same goes for any boss or ally barks.
- Reddit and some guide sites were unreachable. Video walkthroughs exist, but we could not watch them:
  - [full-clear guide](https://www.youtube.com/watch?v=QILPyLMPuVQ)
  - [another walkthrough](https://www.youtube.com/watch?v=IvAH8jnNW-Y)
- **The owner's own play experience is therefore the reference for behaviour:** a deep, very cool voice that says a few words on every purchase.
- Anything below that goes beyond this is our design, informed by genre convention. It is not a copy of ZZZ. Someone with the game can confirm details from gameplay footage and adjust the numbers in `VoiceDirector.Cooldown`.

**Shopkeeper bark conventions in comparable games**
- **Hades, Charon:**
  - Charon speaks only in groans of different intonations ([Hades wiki](https://hades.fandom.com/wiki/Charon)).
  - A wordless voice can fire on every purchase without wearing out. That is why our generic companions use a wordless bank.
- **Enter the Gungeon, Bello:**
  - Bello's text bubbles appear above his head ([Bello](https://enterthegungeon.wiki.gg/wiki/Bello), [Shop](https://enterthegungeon.wiki.gg/wiki/Shop)).
  - His reactions escalate as the player misbehaves. In-world speakers get bubbles at the speaker.
- **Slay the Spire, Merchant:** short bubbles on entering the shop, on a purchase and when you lack gold ([wiki](https://slay-the-spire.fandom.com/wiki/Merchant)).
- **General practice:**
  - Bark on meaningful events: entering, buying, a rare purchase, too little money, rerolling, leaving.
  - Keep repeatable barks to **2-6 words** (under about 1.5 s) so rapid buying is not talked over.
  - Use several variants per trigger, drawn from a **shuffle bag** with no immediate repeats.
  - Let important lines outrank chatter.
  - Put cooldowns on lines that would otherwise spam.

**What we took from this**
- The shopkeeper speaks on **every committed action**: enter, purchase, rare purchase, companion purchase, reroll, can't afford, blood price and leave.
  - It uses a short voiced bark, and the subtitle in its speech box is exactly the words heard.
- The long, depth-aware story lines from `NightmareDialogue` still appear in two places:
  - after the welcome bark, on entering;
  - as unvoiced asides when you walk up to a stand.
- Bosses bark on entering, during attacks (on a cooldown), when hurt (on a cooldown), at the half-way turn, near the end and when they fall.

---

## 2. How it works

| Piece | File | Job |
| --- | --- | --- |
| Voice script | `Runtime/Presentation/VoiceLines.cs` | The single table of every voiced line. Each entry is `speaker/trigger_NN` with its text and a delivery note. The prompt doc is generated from it. |
| VoiceDirector | `Runtime/Presentation/VoiceDirector.cs` | Picks lines from a shuffle bag per bank, with no immediate repeats. Applies per-trigger cooldowns and loads clips lazily. Plays one channel per speaker with priorities, a light music duck, the Voices volume and the Reduced Audio Clutter setting. It also pauses with the game and stops when the window loses focus. |
| Speech boxes | `Runtime/Presentation/SpeechBubbles.cs` | Every spoken line is drawn as a paper ticket in the game's skin (`ArcadeSkin.Ticket`). See the next table for its behaviour. |
| Who speaks when | `Runtime/Presentation/SpeechBubbleCast.cs` | Watches state without changing it: the roster (joins and star-ups), the travelling four, the boss, the warm chair, and quiet moments in the shop. It queues lines with polite gaps. |
| Shopkeeper | `MerchantDialogue.cs`, `WorldShop.cs` | `MerchantDialogue.Speak` picks the bark for each moment. The box hangs beside the merchant's head (`WorldShop.MerchantMouth`). The shop draws the boxes under its tooltips. |
| Murmur fallback | `MerchantVoice.cs` | The original syllable murmurs still play when no clip exists. They go silent while a real clip plays, and the laugh/hmm gestures defer to the voice. |
| Fight lines | `NightmarePresentation.cs` | The Last Customer's six scripted beats are spoken by their own speakers. Shop whispers become the left-edge caption. |
| Banter | `OctoberDialogue.cs` | Every 16 s in the shop, the hero or a companion says their line above their own head. It waits for joins, star-ups and the journal. |
| Settings | `PlayerProfile.VoiceVolume`, `PresentationSettings.VoiceVolume / VoiceDuck`, `RunMenu` "Voices" slider | Older saves load with Voices at 100 %. |

How the speech boxes behave:

| Behaviour | Detail |
| --- | --- |
| Look | Spoken words are in the handwriting font (`ArcadeTypography.Hand`), or the body font with High-Contrast UI. The speaker's name sits in small capitals on top. |
| Colour by speaker | Shopkeeper: cream with an amber stamp. Dream altar: lilac. Companions: teal. The four story companions: gold. Hero: gold. Bosses: dark paper with an ember stamp. |
| Tail | A tail points at the speaker. |
| Placement | The box goes above the speaker by default (the shopkeeper prefers its right side). It is clamped to the screen and avoids other boxes and the HUD furniture, so boxes never cover each other. |
| Lifetime | Short: 2.4-6.5 s, scaled by length and at least as long as the clip. |
| Reveal | Text types in at 58 characters a second, without words jumping between lines. With Reduced Motion, the text appears whole and the box does not pop. |
| Crowds | At most five boxes at once. The least important and oldest are dropped first. |
| Off-screen speakers | Someone not in the arena (the shopkeeper during the final fight) speaks from a tail-less box at the top centre. |
| Captions | Narration with no speaker, and the whisper of the One Who Stays (who in canon always speaks from the left), is a quiet handwritten caption at the left edge, not a box. |
| Story pages | Ending pages and journal pages stay full-screen text, as before. |

### Clips

- **Path:** `Assets/ArcaneBrigade/Resources/Audio/Voice/<Speaker>/<trigger>_NN.wav` (or `.ogg`), for example `Voice/Shopkeeper/purchase_03.wav`.
- **Speaker folders:** `Shopkeeper`, `DreamMother`, `Keeper`, `ChairPorter`, `Door`, `LastCustomer`, `Bedside`, `Hob`, `Latch`, `Postscript`, `Tally`, `Companion`.
- **Missing clips are expected.** The line still shows as a subtitle (`VoiceDirector.SilentLines` counts them). The shop keeps its murmurs, and nothing is logged.
- **Adding or changing a line:**
  1. Edit `VoiceLines.cs`.
  2. Run `powershell -File Tools/Audio/Generate_Voice_Prompts.ps1`.
  3. Record the new file names from the updated table.
- **Never reorder a bank once it has been recorded.** Numbers are assigned in order, so new lines go at the end.

### Mixing

- **Volume:** `Voices` slider × `CheckpointAudio.BusTrim(AudioBus.Voice)` (0.85) × master. Reduced Audio Clutter multiplies by a further 0.85.
- **Ducking:** while a clip plays, `PresentationSettings.MusicDuck` drops the music to 72 %. A danger duck (48 %) still wins.
- **Priorities:** `Chatter 0 < Normal 1 < Important 2 < Story 3`. A lower-priority line never talks over a higher one from another speaker. The same speaker replaces its own line unless the newer one is less important. Repeated identical triggers (rapid purchases) let the clip already playing finish, instead of stacking a new line over it.
- **Reduced Audio Clutter:** only lines of priority *Important* or higher are voiced. Every line is still subtitled.

---

## 3. Triggers

Cooldowns are per speaker and trigger, in unscaled seconds (`VoiceDirector.Cooldown`).

### Shopkeeper (`Shopkeeper`, normal shop) and dream altar (`DreamMother`, special shop)

| Trigger | When | Priority | Cooldown | Box text |
| --- | --- | --- | --- | --- |
| `enter` | The merchant arrives for the wave | Important | - | Bark, then the shop's depth-aware line |
| `story` 01 / 03 | Entering shop 1 / shop 13 (instead of `enter`) | Story | once | The canon line |
| `purchase` | A common or uncommon relic is bought | Normal | 1.2 s (repeats let the clip finish) | Bark |
| `purchase_rare` | A Rare or better relic is bought | Important | - | Bark |
| `purchase_companion` | A companion is recruited | Normal | - | Bark (the new companion speaks about 1.4 s later, after its arrival reveal) |
| `blood` | An offer is paid in HP | Normal | - | Bark |
| `cant_afford` | Any denied purchase, reroll or fusion | Normal | 1.4 s | Bark |
| `reroll` | The stock is refreshed | Normal | 1.6 s | Bark |
| `fuse` (DreamMother only) | Altar fusion | Normal | - | Bark |
| `leave` | Walking through the exit to the next wave | Important | - | Bark |
| `idle` | Nothing near, nothing said for 15 s | Chatter | 22 s | Bark |
| `chair` | The hero comes within 1.9 m of the warm chair (the shopkeeper never looks at it) | Important | 14 s | Bark |
| `story` 02 | The first approach to the chair from shop 8 to 19, once per run | Story | once | "If you ever sat in it... no. Never mind." |
| `story` 04 | Shop 16, after Hob is pulled up: the key | Story | once | Canon line |
| `story` 05 | Final fight, beat 2 (off screen, top centre) | Story | once | Canon line |
| *(inspect)* | Walking up to a stand | text only | 1.6-2.2 s | Depth-aware aside, murmured |

### Bosses (`Keeper`, `ChairPorter`, `Door`, `LastCustomer`, `Bedside`)

| Trigger | When | Priority | Cooldown |
| --- | --- | --- | --- |
| `enter` | 1.1 s after the boss appears | Important | - |
| `attack` | A new volley or signature skill starts, while it is not already talking (not the Customer) | Chatter | 7.5 s |
| `hurt` | Each further 12 % of health lost (not the Customer) | Normal | 9 s |
| `phase` | Health first drops to 50 % | Important | - |
| `story` | Health first drops to 22 % (not the Customer) | Important | - |
| `defeat` | The boss falls (box held 4.6 s at its last position) | Story | - |

- **The Last Customer's fight** keeps its six scripted beats, one per two volleys:
  1. Customer `story_01`
  2. Shopkeeper `story_05`
  3. Customer `story_02`
  4. Hob `story_01`
  5. Customer `story_03`
  6. "Then we pull." above the hero
- **Shop 19 after SIT CLOSER was won:**
  - Customer `story_04` ("...You did. Who paid?"), off screen.
  - Hob `story_02` ("Nobody. We pulled.").
- **Bosses are identified by their story name** (`VoiceDirector.BossSpeaker`). Endless and other bosses stay silent.

### Companions

| Who | Trigger | When | Text |
| --- | --- | --- | --- |
| Hob, Latch, Postscript, Tally | `join` | Shop 1 / 5 / 6 / 11, 3.4 s after the welcome | The first time, their canon first words. After that, a random line from their bank |
| Latch | `starup` | Shop 11 (the four reach tier 2) | Bank |
| Hob | `starup` 02 | Shop 16, after being pulled up | "Warmer now. Stay close." |
| Tally | `story` 01 | Shop 19, back with the brigade | "I gave the list back." |
| Any other companion | `join` (wordless `Companion/join_NN`) | A new tier-1 copy appears in the roster (purchase, rescue, gift), once its arrival or merge reveal has finished | Their own first words (`CompanionBanter` first line, or their story-arc line; otherwise the biography from `CompanionVoice`, at most 90 characters). Otherwise a short welcome. Strangers say they have not met you. |
| Any other companion | `starup` (wordless `Companion/starup_NN`) | A 2★ / 3★ copy appears (merge), 1.1 s after the merge reveal | One of six 2★ or five 3★ lines. The name tag shows the stars |

- **Gaps:** companion lines keep at least 2.6 s between each other. Shop banter waits 4 s after any companion line. Lines that cannot find their speaker within their patience window are dropped.
- **Resuming a save mid-shop** does not replay that shop's arrival lines.

---

## 4. Counts

The script has 256 lines in total:

| Speaker | Lines | Breakdown |
| --- | --- | --- |
| Shopkeeper | 81 | 10 enter, 12 purchase, 8 rare, 6 companion, 8 reroll, 8 can't afford, 8 leave, 6 idle, 6 chair, 4 blood, 5 story |
| DreamMother | 24 | |
| Keeper | 22 | |
| Chair Porter | 22 | |
| Door | 22 | |
| Last Customer | 24 | |
| Bedside | 22 | |
| Hob | 8 | |
| Latch | 6 | |
| Postscript | 6 | |
| Tally | 7 | |
| Companion (wordless) | 12 | |

---

## 5. Verification and capture

- **Compile:** Roslyn, against the runtime assemblies.
- **Unity checks:** `ArcaneBrigade.Editor.ActOneStoryChecks.Run` / `RunWide`.
- **Native capture:** `ArcaneBrigade.exe -arcane-voice-capture -capturePath DIR` (`Runtime/Core/VoiceCapture.cs`). It writes these screenshots, plus `voice-capture.txt` with every box's speaker, rectangle and text:

| Screenshot | Shows |
| --- | --- |
| `00-shop-enter` | The shopkeeper's welcome |
| `01-postscript-joins` | Postscript's first words |
| `02-purchase-shopkeeper` | The shopkeeper reacting to a purchase |
| `04-star-up` | A companion ranking up |
| `03-companion-joins` | A new companion joining |
| `05-chair-warning` | The warm-chair warning |
| `06-cant-afford` | A denied purchase |
| `07-keeper-enter` | The Keeper entering |
| `08-keeper-attack` | A Keeper attack bark |
| `09-keeper-phase` | The Keeper's half-way line |

## 6. Possible core hooks (not required, not implemented)

Everything above only observes existing state. These hooks in core code would make the timing tighter:

- **A `BossTelegraphStarted` event from `CombatSimulation`.** Attack barks would land exactly on the wind-up, instead of on a volley or skill counter change.
- **A `CompanionRecruited(unitId, source)` event from `RunSession`.** Rescue or gift joins could get their own lines (for example "You came back for me"), instead of the generic join.
- **A pause-aware presentation clock exposed to UI.** Boxes currently use unscaled time and simply stay hidden while the game is paused.
