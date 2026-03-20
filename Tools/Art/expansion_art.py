"""Writes Docs/Expansion_Art_Worklist.json: the image requests for the content expansion (Docs/Content_Expansion_Proposal.md,
version 5, Docs/Content_Expansion_v5.md): 5 new heroes and 28 new companions (character sheets), the new heroes' transformed pieces and gauges, the
simplified gauges of the 31 existing heroes, 13 new weapons, 30 item icons and 6 bond icons.

Same job format as Docs/Bizarre_Arsenal_Art_Worklist.json (name, path, transparent, refs, depends, mode, prompt). Images are
made by the painting pass, never procedurally. A Full gauge is an exact edit of its Empty gauge.
Usage: python Tools/Art/expansion_art.py
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ART = "Assets/ArcaneBrigade/Art/Arsenal/"
EXP = "Assets/ArcaneBrigade/Art/Expansion/"
REFS = ["Assets/ArcaneBrigade/Art/FinalQuality/furniture-00_v1.png", "Assets/ArcaneBrigade/Art/AnimationHD/NightMarket_v1.png",
        "Assets/ArcaneBrigade/Resources/MienCho/ActOne/Postscript.png", "Assets/ArcaneBrigade/Resources/MienCho/ActOne/Prop_BoundaryGate.png"]
CHAR_REFS = ["Assets/ArcaneBrigade/Art/October/Heroes4.png", "Assets/ArcaneBrigade/Art/October/Allies3.png",
             "Assets/ArcaneBrigade/Art/October/Allies5.png"]
ICON_REFS = ["Assets/ArcaneBrigade/Art/October/Relics5.png", "Assets/ArcaneBrigade/Art/K26/Relics1_v1.png"]
BOND_REFS = ["Assets/ArcaneBrigade/Art/October/Bonds1.png", "Assets/ArcaneBrigade/Art/October/Bonds2.png"]
STYLE = ("Use case: style-transfer. Production raster art for Arcane Brigade, a storybook dream-world roguelite. "
         "The input images are STYLE REFERENCES ONLY, not edit targets: match THEIR generation exactly, the layered painterly "
         "storybook rendering with patterning and warm metallic sheen. Confident dark ink contours, rounded slightly crooked "
         "asymmetric hand-made forms. Palette: warm antique brass, amber, cream highlights, deep teal, midnight blue, burgundy/wine "
         "velvet as a material. Our side's objects NEVER glow ember-red or danger-red (reserved for enemies); any red is a deep wine "
         "material. Simple, readable silhouette at gameplay size. Clearly drawn illustration, never photo, 3D render, PBR or plastic sheen. ")
ISOLATED = (" ONE complete isolated subject on a genuinely transparent RGBA background, centred, with 12% empty margin. "
            "No ground plane, cast shadow, external halo, lettering, labels, UI, grid or watermark. No gore.")
HELD = (" Draw the weapon alone, lying horizontally and POINTING RIGHT (+X), its grip or handle at the LEFT about 20% across, "
        "as a companion would hold it out in front. 1024x512 canvas.")
SHOT = " Draw it travelling to the RIGHT (+X), motion implied by its shape only (no speed lines outside the object). 512x512 canvas."
ROUND = " Round or spinning object seen from slightly above. 512x512 canvas."
FLOOR = " Seen from the elevated three-quarter game view, standing on the floor (no floor drawn). 512x512 canvas."
PROP = " Draw the object alone, seen from the side or front as described, no hands, no character. 768x512 canvas."
ICON = (" An inventory icon: ONE object, front view, filling about 70% of a 512x512 canvas, readable at 48 px. Original design, "
        "no logos, no text.")
GAUGE = (" A tall narrow upright gauge vessel seen from the front, about one third as wide as it is tall, standing straight, "
         "filling the canvas height except a 6% empty margin at top and bottom. 384x1024 canvas.")
SHEET = ("A production hand-painted 2D CHARACTER ANIMATION SPRITE SHEET for the same game as the reference sheets (match their "
         "characters' rendering, proportions and ink line exactly). Exactly 4 columns by {rows} rows of completely isolated full-body "
         "sprites on a truly transparent background. Each ROW is one distinct character. Every COLUMN is an animation pose of the "
         "SAME character: column 1 idle relaxed, column 2 walking with left leg forward and a slight lean, column 3 attack "
         "anticipation braced back with a determined face, column 4 attack release reaching forward with an expressive face. "
         "NO held weapons (weapons are separate game objects). No ground, no shadows, no glow outside the silhouette, no text, no "
         "grid. SAME scale and identity across all four poses in a row. Living storybook creatures with hand-made curio details. "
         "Front three-quarter view facing right, full body and feet visible. Sprites use at most 70% of the cell width and 75% of "
         "the cell height, with large transparent margins; never overlap. 1536x{h} canvas. Row designs in order: ")
jobs = []


def single(path, name, refs, prompt, depends=None):
    jobs.append(dict(name=name, path=path, transparent=True, refs=refs, depends=depends, mode="single", prompt=prompt))


def edit_full(path_empty, name, inside, canvas="384x1024"):
    single(path_empty.replace("_Empty_v1.png", "_Full_v1.png"), name.replace("_Empty", "_Full"), [path_empty],
           "Use case: precise-object-edit. EDIT THIS EXACT gauge: keep the vessel's outline, frame, size, position and canvas "
           "identical (it is laid over the empty one in the game), and change only what is inside: " + inside +
           ". Genuinely transparent RGBA background, same " + canvas + " canvas.", depends=name)


# ---------------------------------------------------------------- 1. characters (5 heroes, 19 companions, 4 extra forms)
CHARACTERS = [
    # heroes
    ("KiteAdmiral", "a small grey mouse admiral in a midnight-blue captain's coat with brass buttons and a tricorn hat, a folded paper box kite strapped to its back"),
    ("SundaeDuchess", "a porcelain doll duchess with a swirled vanilla-and-strawberry ice-cream hairdo, a wafer tiara and a pastel cream gown"),
    ("BonsaiClocksmith", "an old tortoise clocksmith whose shell is a tiny bonsai garden with a brass clock face set into it, round spectacles and a leather tool apron"),
    ("JugglingJester", "a lanky jester sewn from patchwork felt, a two-pointed bell hat, diamond-pattern tights and a painted grin"),
    ("BellDiver", "a frog in an old brass diving helmet with a round glass port, a canvas diving suit and heavy lead boots"),
    # companions
    ("PaperPlanePigeon", "a plump pigeon partly folded from cream paper, with a little postman scarf"),
    ("BobaTadpole", "a round tadpole sitting in a lidded milk-tea cup with dark tapioca pearls, a striped straw like an antenna"),
    ("BakerMouse", "a mouse baker with a flour-dusted apron, a tall chef hat and rolled sleeves"),
    ("WateringSnail", "a snail whose shell is a little teal watering can with a rose spout"),
    ("RaincoatFrog", "a frog in a mustard-yellow raincoat and a sou'wester hat, rain-spotted boots"),
    ("LampGoat", "a small goat with curled ivory horns, a paper lantern hanging from one horn"),
    ("YoyoSquirrel", "a nimble chestnut squirrel with a striped bushy tail and a little sporty headband"),
    ("ShovelPenguin", "a sturdy penguin with a knitted teal scarf and work mittens"),
    ("TopBeetle", "a round beetle whose shell is painted like a spinning top, a brass tip on its back"),
    ("LensOwl", "an owl scholar with a brass magnifier over one eye and a square scholar's cap"),
    ("RainHeron", "a tall slate-blue heron with ink-stained feathers and a scroll satchel"),
    ("TeaDragon", "a small dragon shaped like a celadon teapot, a spout snout, a lid crest and curling steam whiskers (Mythic: rich detail)"),
    ("LighthouseWhale", "HARBOR WHALE: a small floating whale with a tiny striped lighthouse on its back, a lamp in its top, barnacle freckles (Mythic)"),
    ("JackInTheBox", "RINGMASTER JACK: a jack-in-the-box ringmaster: a striped wooden box base with a springy ringmaster popping out, top hat and moustache (Mythic)"),
    ("KiteDragon", "a long floating paper kite dragon with bamboo ribs, painted scales and a ribbon tail, iridescent rainbow sheen on its edges (Prismatic: the rarest)"),
    ("SundaeGolem", "a big friendly golem built of ice-cream scoops in a waffle-cone body with a wine-cherry on top, iridescent rainbow sprinkles (Prismatic)"),
    ("LostCarousel", "a walking gilded carousel horse with a brass pole through its back and a fragment of striped carousel canopy as a hat, iridescent rainbow trim (Prismatic)"),
    ("EndlessIndex", "a floating moth whose wings are open book pages tabbed with index cards, iridescent rainbow page edges (Prismatic)"),
    ("FirstSeed", "an ancient glowing seed sprite: an acorn-like seed with a single sprout on top and tiny root legs, bark carvings, iridescent rainbow veins (Prismatic)"),
    # extra forms
    ("EclipseEgg_Ranged", "Eclipse Egg (the reference egg ally) hatched into a ranged form: a little starlit bird with an eggshell cap"),
    ("EclipseEgg_Melee", "Eclipse Egg hatched into a melee form: a small armoured night lizard with an eggshell helmet"),
    ("EclipseEgg_Support", "Eclipse Egg hatched into a support form: a soft moon moth with eggshell wings"),
    ("FirstSeed_Sapling", "a small fighting sapling grown by The First Seed: a twig sprite with leaf hands and root feet"),
    # version 5 additions (Docs/Content_Expansion_v5.md §6)
    ("StaticFerret", "a sleek ferret whose fur stands on end with static, little copper earrings crackling"),
    ("StormKitten", "a small kitten whose fluffy fur is a little grey storm cloud, tiny sparks on its whiskers"),
    ("VoltEel", "a coiled teal eel with glowing yellow stripes, a tiny conductor's cap (Rare)"),
    ("ShantySeaLion", "a young sea lion singing a sea shanty, a sailor's neckerchief and a small squeezebox"),
    ("BottleGull", "a seagull carrying a corked message bottle under one wing, a striped scarf"),
    ("PinnedBeetle", "a living museum beetle with a brass specimen pin through its little label tag, glass-case shine on its shell"),
    ("FossilArmadillo", "a small armadillo whose banded shell is fossil stone with an ammonite, a museum tag on a string"),
    ("SpecimenSlime", "a slime kept in a corked specimen jar it carries around, a museum label on the jar (Rare)"),
    ("CourtHerald", "a small herald rabbit in a velvet tabard with a long brass trumpet slung on its back"),
]
for sheet in range(0, len(CHARACTERS), 4):
    rows = CHARACTERS[sheet:sheet + 4]
    n = sheet // 4 + 1
    path = EXP + f"Characters{n}_v1.png"
    h = 1536 if len(rows) == 4 else 1152
    single(path, f"Characters{n}", CHAR_REFS, SHEET.format(rows=len(rows), h=h) +
           "; ".join(f"ROW {i + 1} ({cid}): {desc}" for i, (cid, desc) in enumerate(rows)) + ".")

# ---------------------------------------------------------------- 2. new heroes: transformed piece + gauge
NEW_HEROES = {
    "KiteAdmiral": ("KiteAdmiral_Awake", "FLEET KITE: one small paper box kite with a bamboo frame, sailcloth panels in teal and cream and a ribbon tail, flying on its own (the game shows four of them above the admiral's ranged friends); side view facing right", PROP,
                    "a paper kite standing on its tail with a long window down its body and a small brass wind arrow on top; the window is EMPTY", "the window is now filled with swirling teal wind and paper-scrap gusts"),
    "SundaeDuchess": ("SundaeDuchess_Awake", "GIANT SCOOP: one huge ice-cream scoop (vanilla with strawberry ripple) on a wafer, as it falls from the sky; front view", PROP,
                      "a tall sundae glass with a long clear window; the glass is FULL of ice cream to the brim (this is the starting state: it melts down in the game)", "the same glass now EMPTY except a little melted puddle at the bottom"),
    "BonsaiClocksmith": ("BonsaiClocksmith_Awake", "TIME GARDEN: a small bonsai tree in a shallow brass tray whose canopy holds a clock face with hands, planted in front of the hero; front view", PROP,
                         "a tall brass clock case with a long window showing a clock dial at the top and a pendulum track below; the window is EMPTY and dark", "the window is now filled with glowing amber clockwork light and little bonsai leaves"),
    "JugglingJester": ("JugglingJester_Awake_Held", "GRAND CIRCUS CLUBS: two crossed golden juggling clubs with wine-and-cream stripes and little bells", HELD,
                       "a tall striped circus pole with FIVE identical empty ball cups stacked one above the other with even gaps", "each of the five cups now holds a glowing juggling ball, each a different pastel colour"),
    "BellDiver": ("BellDiver_Awake", "DIVING BELL: a round brass diving bell with riveted plates, three round glass ports and a hose on top, seen from the side facing right", PROP,
                  "a brass depth gauge column with a long glass tube marked with fathom lines and a little bell on top; the tube is EMPTY", "the tube is now filled to the top with deep teal sea water and rising bubbles"),
}
for hero, (piece, subject, shape, empty, full) in NEW_HEROES.items():
    ref = EXP + "Characters1_v1.png" if hero in ("KiteAdmiral", "SundaeDuchess", "BonsaiClocksmith", "JugglingJester") else EXP + "Characters2_v1.png"
    single(ART + piece + "_v1.png", piece, [ref] + REFS, STYLE + "Subject: " + subject + ". The first reference shows the hero (row " + hero + ")." + shape + ISOLATED,
           depends="Characters1" if "Characters1" in ref else "Characters2")
    e = ART + "Gauge_" + hero + "_Empty_v1.png"
    single(e, "Gauge_" + hero + "_Empty", [ref] + REFS, STYLE + "Subject: the gauge of the hero " + hero + " in the first reference, " + empty + "." + GAUGE + ISOLATED,
           depends="Characters1" if "Characters1" in ref else "Characters2")
    edit_full(e, "Gauge_" + hero + "_Empty", full)

# ---------------------------------------------------------------- 3. shaped gauges for all 36 heroes (Gauge2_*, approved 18/10/2026)
# Each hero's gauge has its own shape (a moon, a dial, a stamp sheet, an hourglass...). The game reveals the Full painting over
# the Empty one in the hero's own order (sweeping, blooming, piece by piece...), computed from the difference between the two
# paintings, so the pair must match exactly outside the fill area (Tools/Art/hero_gauges_v3.json; mode is for the code).
GAUGE3 = (" A simple, readable HUD gauge: one clear silhouette, a modest frame with at most two or three small ornaments, readable "
          "at 120 px. The FILL AREA is one clearly bounded region (a glass, a recess, a page, a set of compartments); everything "
          "outside it is frame. When the gauge has separate compartments, leave a clear gap between them (at least 4% of the canvas) "
          "so each reads as its own piece. No numbers, no letters, no text. Seen from the front, centred, ")
g3 = json.loads((ROOT / "Tools" / "Art" / "hero_gauges_v3.json").read_text(encoding="utf-8"))
for hero, g in g3.items():
    canvas = g["frame"].split(". ")[-1].replace(" canvas", "")
    e = ART + "Gauge2_" + hero + "_Empty_v1.png"
    single(e, "Gauge2_" + hero + "_Empty", [g["ref"]] + REFS,
           STYLE + "Subject: the gauge of the hero in the first reference (in a sheet, the hero named " + hero + "): " + g["empty"] + "."
           + GAUGE3 + g["frame"] + "." + ISOLATED)
    edit_full(e, "Gauge2_" + hero + "_Empty", g["full"], canvas)

# ---------------------------------------------------------------- 4. thirteen new weapons (held + painted pieces; effects are ink in code)
WEAPONS = {
    "KiteSpool": ("a wooden kite-string spool with a brass crank, a coil of cream string and a small folded paper kite clipped to it",
                  [("KiteShot", "a paper box kite flying, teal and cream panels, bamboo frame, short ribbon tail", SHOT)]),
    "ScoopLauncher": ("a big brass ice-cream scoop with a spring lever and a wine-velvet grip", [("Scoop", None, ROUND)]),
    "CuckooCannon": ("a carved walnut cuckoo-clock shaped into a short cannon, a clock face on its side and a brass barrel",
                     [("PendulumShot", "a brass clock pendulum bob flying", SHOT), ("CuckooBird", "a little carved wooden cuckoo bird diving, wings back", SHOT)]),
    "JugglingPins": ("one painted juggling club with wine-and-cream stripes and a small bell", []),
    "SonarHarpoon": ("a brass harpoon gun with a small sonar bell on top and a coiled rope", [("HarpoonShot", "a brass-tipped harpoon with a short trailing rope", SHOT)]),
    "YoYo": ("a wooden yo-yo with a teal string looped round a finger ring", [("YoyoSpin", "the same wooden yo-yo spinning, seen face-on", ROUND)]),
    "RollingPin": ("a long wooden rolling pin with turned handles, dusted with flour", []),
    "BobaStraw": ("a lidded milk-tea cup with a fat striped straw sticking out, held like a blowgun", [("BobaPearl", "a single glossy dark tapioca pearl", ROUND)]),
    "PaperPlaneSquadron": ("a brass launcher clip holding three folded paper planes side by side", [("PaperPlane", "one folded cream paper plane with a teal stripe", SHOT)]),
    "WateringCan": ("a teal tin watering can with a brass rose spout", []),
    "SnowShovel": ("a short snow shovel with a wooden handle and a dented teal blade", [("SnowClump", "a thrown clump of packed snow with a few ice crystals", SHOT)]),
    "SunLens": ("a brass magnifying glass with a carved wooden handle", []),
    "SpinningTop": ("a wooden spinning-top launcher with a pull cord and a brass trigger ring", [("SpinTop", "a painted wooden spinning top with a brass tip, seen spinning from slightly above", ROUND)]),
}
SCOOPS = ["a vanilla ice-cream scoop", "a strawberry ice-cream scoop (wine-pink, not red)", "a chocolate ice-cream scoop"]
for wid, (held, pieces) in WEAPONS.items():
    single(ART + wid + "_Held_v1.png", wid + "_Held", REFS, STYLE + "Subject: " + held + "." + HELD + ISOLATED)
    for name, subject, shape in pieces:
        if name == "Scoop":
            first = name + "_f0"
            single(ART + first + "_v1.png", first, REFS, STYLE + "Subject: " + SCOOPS[0] + "." + shape + ISOLATED)
            for i, s in enumerate(SCOOPS[1:], 1):
                single(ART + f"{name}_f{i}_v1.png", f"{name}_f{i}", [ART + first + "_v1.png"],
                       "Use case: precise-object-edit. EDIT THIS EXACT image: keep the same scoop shape, size, canvas and position; change ONLY its flavour into " + s + ". Genuinely transparent RGBA.", depends=first)
            continue
        single(ART + name + "_v1.png", name, [ART + wid + "_Held_v1.png"] + REFS,
               STYLE + "Subject: " + subject + " (it must match the held weapon in the first reference)." + shape + ISOLATED, depends=wid + "_Held")

# rarity pieces: the companions' own skills that paint an object
for name, subject, shape in [("PaperInventor_Tower", "a little folded-paper turret on three paper legs with a cardboard barrel", FLOOR),
                             ("LostCarousel_Horse", "a gilded carousel horse on a brass pole, mid-gallop", PROP),
                             ("TeaDragon_Teapot", "a celadon teapot set down on the floor, its lid rattling with steam", FLOOR),
                             ("FirstSeed_Hedge", "a short hedge wall of thorny brambles with small white flowers, seen from the side", PROP)]:
    single(EXP + name + "_v1.png", name, REFS, STYLE + "Subject: " + subject + "." + shape + ISOLATED)

# ---------------------------------------------------------------- 5. thirty item icons
ITEMS = {  # version 5 final (Docs/Content_Expansion_v5.md §6b): id -> (tier, look)
    "TallyChalk": ("Common", "a stick of chalk beside five tally marks scratched on a slate"),
    "CoinSash": ("Common", "a runner's sash with little coin pockets"), "FishCracker": ("Common", "a fish-shaped cracker biscuit"),
    "PostageStamp": ("Uncommon", "a single postage stamp with a wax seal stamped on it"), "FrostSalt": ("Uncommon", "a small glass shaker of glittering frost salt"),
    "RefundSlip": ("Uncommon", "a paper refund slip with a stamped coin seal and a torn perforated edge, no readable text"),
    "WhetstoneWafer": ("Uncommon", "a wafer biscuit shaped like a whetstone with a sugar-sharpened edge"), "LateFee": ("Uncommon", "an overdue library card with a wine-coloured late stamp, no readable text"),
    "HagglersAbacus": ("Rare", "a small brass abacus with coin-shaped beads"), "SlipstreamScarf": ("Rare", "a long streaming silk scarf with a brass pin"),
    "BrainFreeze": ("Rare", "an ice-cream cone frozen solid with frost cracks"), "WindChime": ("Rare", "a brass wind chime with three tubes and a paper wind tail"),
    "SpecimenPin": ("Rare", "a long brass specimen pin with a jewelled head stuck through a little paper label"), "StarPolish": ("Rare", "a small tin of star polish with a cloth and a shining star on the lid"),
    "KiteTailRibbon": ("Mythic", "a long kite-tail ribbon with paper bows"), "MeltingSundae": ("Mythic", "a sundae glass melting over its rim, a frost crystal on the spoon"),
    "CuckooPendulum": ("Mythic", "a brass pendulum with a tiny carved cuckoo on its bob"), "DivingBellLantern": ("Mythic", "a small brass diving-bell lantern with a round glass port"),
    "TwinTicket": ("Mythic", "two joined circus tickets with a perforated line"), "TreasureMimic": ("Mythic", "a little treasure chest with teeth and a curious eye peeking from under the lid"),
    "DuelistsRibbon": ("Mythic", "a duelling sword knot ribbon in wine and gold"), "FrostLedger": ("Mythic", "a ledger book bound in ice with frosted pages"),
    "UndertowAnchor": ("Mythic", "a small brass anchor wrapped in a curling wave of sea water"), "FunhouseMirror": ("Mythic", "a wavy funhouse mirror in a striped carnival frame"),
    "SpliceKit": ("Mythic", "a sewing splice kit: three coloured threads twisted into one star-shaped knot, needles in a felt case"),
    "TheEndlessEncore": ("Prismatic", "a theatre curtain tassel that loops into an infinity knot"), "MoonClockKey": ("Prismatic", "a large clock key whose bow is a crescent moon"),
    "MirrorOfTwins": ("Prismatic", "a hand mirror whose glass shows a second, smaller reflection"),
    "StorybookEnding": ("Prismatic", "a closed storybook with a gold ribbon bookmark and a happy-ending seal"), "DreamLoom": ("Prismatic", "a tiny wooden loom weaving a rainbow thread"),
}
for iid, (tier, desc) in ITEMS.items():
    note = " (Mythic: richer detail, a faint gold rim)" if tier == "Mythic" else " (Prismatic, the rarest: iridescent rainbow sheen on the object itself)" if tier == "Prismatic" else ""
    single(EXP + "Item_" + iid + "_v1.png", "Item_" + iid, ICON_REFS, STYLE + "Subject: " + desc + " (" + tier + " item)" + note + "." + ICON + ISOLATED)

# ---------------------------------------------------------------- 6. six bond icons
for bid, desc in {"PaperWings": "a pair of folded paper wings pinned together with a brass pin",
                  "SweetTooth": "a wrapped candy and a small ice-cream cone crossed",
                  "BigTop": "a little striped circus tent with a pennant",
                  "StaticClub": "a small lightning bolt badge crackling between two copper studs",
                  "SeaShanty": "a squeezebox accordion with a rope knot and a seashell",
                  "ExhibitHall": "a museum display bell jar with a pinned paper specimen inside"}.items():
    single(EXP + "Bond_" + bid + "_v1.png", "Bond_" + bid, BOND_REFS,
           STYLE + "Subject: a bond emblem: " + desc + ". Same visual size and treatment as the emblems in the references, readable at 32 px."
           " 512x512 canvas." + ISOLATED)

# ---------------------------------------------------------------- 7. shop visitors (45, approved 18/10/2026; Docs/Events_Ideas.md)
import importlib.util
_spec = importlib.util.spec_from_file_location("visitors_art", ROOT / "Tools" / "Art" / "visitors_art.py")
_visitors = importlib.util.module_from_spec(_spec); _spec.loader.exec_module(_visitors)
jobs.extend(_visitors.jobs(CHAR_REFS + ["Assets/ArcaneBrigade/Art/AnimationHD/StrangeMerchants_v2.png"]))

# ---------------------------------------------------------------- 8. the bosses' own moves (BossTrademarks, 19/10/2026) and rare covers
# Enemy objects: wine, bile-purple, tarnished brass and ember are allowed here (they belong to the bosses, not to our side).
ENEMY = (" This is an object thrown by a nightmare boss: it may use deep wine, bile-purple, tarnished brass and a little ember-orange,"
         " slightly sinister but still storybook, never gory. Readable at small gameplay size (about 90 px on screen).")
K = "Assets/ArcaneBrigade/Art/"
for name, refs, subject, shape in [
    ("BossProp_Letter_f0", [], "a sealed letter flying end over end: a thick cream envelope gone yellow, a cracked wine-red wax seal with a crown, a torn corner and a faded stamp, no readable writing", " Seen flat from the front, slightly tilted. 512x512 canvas."),
    ("BossProp_Sheep_f0", [], "a dream sheep made of grey-lilac cloud wool running in a hop, little black legs tucked, sleepy half-closed eyes, a tiny brass bell on its collar, facing RIGHT", " Side view facing right (+X), all four feet off the ground mid-hop. 512x512 canvas."),
    ("BossProp_Sheep_f1", [], "the SAME dream sheep landing from its hop, legs stretched down, wool squashed slightly, facing RIGHT", " Side view facing right (+X), same size and design as the first frame. 512x512 canvas."),
    ("BossProp_WaxCrest_f0", [K + "K16/WaxAdmiral_v2.png"], "a rolling crest of melting candle wax like a small breaking wave, cream and amber wax with drips and two half-buried burnt wicks, belonging to the wax ship-captain boss in the first reference", " A wide low crest seen from the side, about three times as wide as tall, flat bottom edge. 768x384 canvas."),
    ("BossProp_Pendulum_f0", [], "a heavy pendulum weight shaped like a crescent-moon cradle carved from dark midnight wood with a tarnished brass rim and a small hook on top where the cord ties", " Hanging straight down, hook at the top centre. 512x512 canvas."),
    ("BossProp_Coin_f0", [K + "K17/CoinWidow_v2.png"], "a huge old coin rolling on its edge, tarnished gold with a veiled widow's profile stamped on its face and a worn milled rim, belonging to the coin widow boss in the first reference", " Seen face-on, perfectly round. 512x512 canvas."),
    ("BossProp_OrganPipe_f0", [], "a single organ pipe of dented brass rising out of the floor, its mouth a dark slot with a faint bile-purple breath, a small wine ribbon tied round it", " Standing upright, about three times as tall as wide, the bottom edge cut flat where it meets the floor. 384x768 canvas."),
    ("BossProp_Suitcase_f0", [], "a battered old suitcase with a wine leather body, brass corners and clasp, a few travel stickers and a paper luggage tag on a string, no readable text", " Seen from the elevated three-quarter game view, standing on its side. 512x512 canvas."),
    ("BossProp_Suitcase_f1", [], "a battered old hatbox-style trunk in teal-black leather with brass studs, a strap and a paper luggage tag, no readable text", " Seen from the elevated three-quarter game view. 512x512 canvas."),
]:
    single(ART + name + "_v1.png", name, refs + REFS, STYLE + "Subject: " + subject + "." + ENEMY + shape + ISOLATED)
# a Mythic or Prismatic shop offer arrives under a cloth that is whisked away (RareUnveil)
for name, subject in [("RareCover_Mythic", "a draped velvet cloth in deep wine with a heavy gold fringe and a small gold star clasp at the top, falling in soft folds over something hidden underneath (the hidden object is not visible, only the cloth's rounded hump)"),
                      ("RareCover_Prismatic", "a draped silk cloth that shimmers through soft pastel colours (rose, mint, sky, lilac, butter) like mother-of-pearl, with a pearl clasp at the top, falling in soft folds over something hidden (only the cloth's rounded hump shows)")]:
    single(ART + name + "_v1.png", name, REFS, STYLE + "Subject: " + subject + ". Seen from the front, about as wide as tall, flat bottom hem. 512x512 canvas." + ISOLATED)

# ---------------------------------------------------------------- 9. enemy roster review (approved 19/10/2026, Docs/Enemy_Roster_v2.md)
_espec = importlib.util.spec_from_file_location("enemies_art", ROOT / "Tools" / "Art" / "enemies_art.py")
_enemies = importlib.util.module_from_spec(_espec); _espec.loader.exec_module(_enemies)
_enemies.jobs(["Assets/ArcaneBrigade/Art/K22/BurrowBell_v1.png", "Assets/ArcaneBrigade/Art/K25/LostLanternSoul_v1.png",
               "Assets/ArcaneBrigade/Art/K22/BroomSkater_v1.png"], single)

out = ROOT / "Docs" / "Expansion_Art_Worklist.json"
out.write_text(json.dumps(jobs, indent=1, ensure_ascii=False), encoding="utf-8")
print(len(jobs), "jobs ->", out)
