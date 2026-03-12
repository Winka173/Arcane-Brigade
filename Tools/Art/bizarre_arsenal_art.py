"""Writes Docs/Bizarre_Arsenal_Art_Worklist.json: the image requests for the Bizarre Arsenal (Docs/Bizarre_Arsenal.md §5).

Same job format as Docs/Art_Master_Worklist.json (name, path, transparent, refs, depends, mode, prompt), so the image
agent can run it the same way. Images are made by the painting pass, never procedurally.
Animation frames after _f0 are edits of the approved _f0 (depends on it, uses it as the reference).
Usage: python Tools/Art/bizarre_arsenal_art.py
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ART = "Assets/ArcaneBrigade/Art/Arsenal/"
ACT = "Assets/ArcaneBrigade/Art/Acting/"
# style references are the CURRENT art generation (Docs/Theme_Consistency_Audit.md #01): the Act I painted props and
# characters, not the older flat sticker-like weapon sheets
REFS = ["Assets/ArcaneBrigade/Art/FinalQuality/furniture-00_v1.png", "Assets/ArcaneBrigade/Art/AnimationHD/NightMarket_v1.png",
        "Assets/ArcaneBrigade/Resources/MienCho/ActOne/Postscript.png", "Assets/ArcaneBrigade/Resources/MienCho/ActOne/Prop_BoundaryGate.png"]
STYLE = ("Use case: style-transfer. Production raster art for Arcane Brigade, a storybook dream-world roguelite. "
         "The input images are STYLE REFERENCES ONLY, not edit targets: match THEIR generation exactly, the layered painterly "
         "storybook rendering with patterning and warm metallic sheen of the newer Act I art (never the older flat, thick-outlined, "
         "low-shading sticker look). Confident dark ink contours, rounded slightly crooked asymmetric hand-made forms. Palette: "
         "warm antique brass, amber, cream highlights, deep teal, midnight blue, burgundy/wine velvet as a material. Our side's "
         "objects and effects NEVER glow ember-red or danger-red (that is reserved for enemies); any red is a deep wine material. "
         "Simple, readable silhouette at gameplay size (about 40-60 px on a 1080p screen); a held item stays modest in size, never "
         "larger than its bearer. The dream's residents are living creatures, not toys; objects may be hand-made curios. "
         "Clearly drawn illustration, never photo, 3D render, PBR or plastic sheen. ")
ISOLATED = (" ONE complete isolated subject on a genuinely transparent RGBA background, centred, with 12% empty margin. "
            "No ground plane, cast shadow, external halo, lettering (unless the subject IS lettering), labels, UI, grid or watermark. No gore.")
HELD = (" Draw the weapon alone, lying horizontally and POINTING RIGHT (+X), its grip or handle at the LEFT about 20% across, "
        "as a companion would hold it out in front. 1024x512 canvas.")
SHOT = " Draw it travelling to the RIGHT (+X), motion implied by its shape only (no speed lines outside the object). 512x512 canvas."
ROUND = " Round or spinning object seen from slightly above. 512x512 canvas."
FLOOR = " Seen from the elevated three-quarter game view, lying on the floor (no floor drawn). 512x512 canvas."
FX = " A short combat effect in our side's colours (brass, amber, cream, teal, soft candy pastels; never hostile ember red or red-black). 768x768 canvas."
TILE = " A seamless horizontal tile: the left and right edges must join perfectly. 1024x256 canvas."

jobs = []
# Only painted OBJECTS are requested (Assets/ArcaneBrigade/Editor/ArsenalSpecs.cs Pieces): every effect around them (bursts,
# clouds, swipes, stamps, pools, cords, wakes, comic lettering) is painted ink in code (Runtime/Presentation/ArsenalInk.cs),
# like the hero mechanics, and a thrown copy of the weapon itself reuses its held painting. The number is how many frames.
OBJECTS = {"LanternBombShot": 1, "SnowGlobeShot": 1, "PinataShot": 1, "CandyBomblet": 1, "AcornBomb": 1,
           "GlobeMote": 1, "CherryBomb": 3, "PopperStick": 2, "FeltBall": 1, "BambooVaseShot": 1, "BambooShoot": 3, "BouquetShot": 1, "CandyDrillShot": 1,
           "PlushOctopus": 4, "ClawHead": 2, "PlushSharkShot": 1, "ShurikenShot": 1, "WaterJet": 1, "CandyPellet": 1, "PlungerCup": 1,
           "ToyBomb": 1, "Pinwheel": 4, "Gumball": 1, "BalloonAnimal": 1, "SaucerShot": 1, "SoundFin": 1, "WindupKey": 2,
           "CorkBullet": 1, "Fastball": 1, "TwigDart": 1, "StarShard": 1, "BuzzerFloor": 1, "KunaiShot": 1}


# a thrown copy must match the held painting exactly (the first reference is that held weapon)
LOOKS_LIKE = {"PlushOctopus": "InkyOctopus", "PlushSharkShot": "PlushShark"}
LOOKS_LIKE_PATH = {}


def add(name, subject, kind, frames=None):
    """frames: list of per-frame subjects; the first is drawn new, the rest are edits of it"""
    if not name.endswith("_Held"):
        if name not in OBJECTS: return  # an effect: painted ink in code
        if frames: frames = frames[:OBJECTS[name]]
    refs = ([ART + LOOKS_LIKE[name] + "_Held_v1.png"] if name in LOOKS_LIKE else [LOOKS_LIKE_PATH[name]] if LOOKS_LIKE_PATH.get(name) else []) + REFS
    shape = {"held": HELD, "shot": SHOT, "round": ROUND, "floor": FLOOR, "fx": FX, "tile": TILE}[kind]
    if not frames:
        jobs.append(dict(name=name, path=ART + name + "_v1.png", transparent=True, refs=refs, depends=None, mode="single",
                         prompt=STYLE + "Subject: " + subject + shape + ISOLATED))
        return
    first = name + "_f0"
    jobs.append(dict(name=first, path=ART + first + "_v1.png", transparent=True, refs=refs, depends=None, mode="single",
                     prompt=STYLE + "Subject: " + subject + " " + frames[0] + shape + ISOLATED))
    for i, step in enumerate(frames[1:], 1):
        n = f"{name}_f{i}"
        jobs.append(dict(name=n, path=ART + n + "_v1.png", transparent=True, refs=[ART + first + "_v1.png"], depends=first, mode="single",
                         prompt="Use case: precise-object-edit. EDIT THIS EXACT image (the approved frame 0) into the next animation frame. "
                                "Keep the same object, design, colours, scale, canvas size and centre position; change ONLY: " + step +
                                " Genuinely transparent RGBA, nothing else added."))


W = {
    "CherryFuseBomb": "a glossy cherry-black round bomb with two cherry leaves and a cream-and-wine striped fuse, held by a little brass loop handle",
    "PopperSticks": "a bundle of three wine-red paper firecracker sticks with gold labels and short fuses, tied with string",
    "DreamRacket": "a tennis racket with a cream-and-teal wrapped grip, brass frame and loose star-patterned strings",
    "BambooVase": "a clear glass vase holding three green bamboo stems with leaves, gripped at its neck",
    "RoseBouquet": "a bouquet of deep wine roses wrapped in cream paper with a teal bow",
    "HomeRunBat": "a wooden baseball bat with brass bands, a cream grip and a little star painted on the barrel",
    "CandyDrill": "a wine-and-cream striped candy drill / spiral lance with a brass grip",
    "InkyOctopus": "a pink plush octopus with button eyes and curling stitched arms, held by one arm",
    "GrabberClaw": "a spring grabber: a brass trigger handle, a coiled spring and a three-finger claw at the right end",
    "StarforgeClaymore": "a huge golden claymore with engraved glowing runes, still wearing a chip of the stone it was pulled from at the crossguard",
    "DragonLanternGlaive": "a long glaive whose head is a red-gold paper-lantern dragon biting a crescent blade, tassels on the shaft",
    "TwinMoonStaff": "a staff with a blade at each end: a sun-gold blade on the left end, a moon-silver blade on the right end, wrapped grip in the middle",
    "PlushShark": "a big stitched plush shark held by its tail, felt teeth, button eye",
    "DrumstickClub": "a giant fried-chicken drumstick club with golden crispy batter and a cream plastic bone handle",
    "PaperShuriken": "a fan of three multicoloured origami shuriken held between fingers-like paper clip",
    "SquirtTrio": "a clear water pistol with three nozzles, water sloshing visibly inside, teal and brass trim",
    "ConfettiBlunderbuss": "a bell-mouthed blunderbuss in brass and burgundy, candy stripes on the stock",
    "PlungerPopper": "a plunger launcher: a wine-red rubber suction cup on an elastic cord coiled round a brass handle",
    "SignpostSlammer": "a round road sign 'Dreamland Crossing' (blue face with a cream arrow) on a striped pole",
    "DetonatorBox": "a small wooden detonator box with a brass T-plunger on top and a coil of painted wire",
    "FirecrackerPinwheel": "a wine-and-gold paper pinwheel on a stick with tiny firecrackers on its vanes",
    "GumballGatling": "a gumball machine turned into a hand-crank gatling: glass globe full of multicoloured gumballs, brass barrels and crank",
    "BalloonBuddy": "a twisted balloon animal (a poodle) on a string tied to a brass ring handle",
    "GlitchToybox": "a jack-in-the-box with a brass crank, lid shut, slightly glitching colour fringe on its edges",
    "SaucerBlaster": "a blue ray gun shaped like a flying saucer, a cyan dome on top and a ring nozzle",
    "FanfareHorn": "a golden flared fanfare horn with a brown wooden mouthpiece and a tassel",
    "WindupWrench": "a big white wrench with a brass wind-up key on its handle",
    "CorkRifle": "a brown wooden hunting rifle that fires corks, a cork on a string at the muzzle",
    "CatcherMitt": "a brown leather baseball catcher's mitt worn on a little hand, a ball tucked in it",
    "FizzBottle": "a green glass soda bottle with a yellow-dotted label and a fizzing cap",
    "LeekBaton": "a long fresh spring onion / leek held like a baton, green leaves at the right end",
    "FlySwatter": "a wine-red fly swatter with a cream handle and a mesh of little stars",
    "DowsingRod": "a forked wooden dowsing twig with a little glowing cyan bead at its tip",
    "StarKnuckle": "a silver knuckle-duster glove with blunt star-shaped studs",
    "BuzzerBlock": "a brass-yellow buzzer block with a big wine-red push button",
    # research pass 4
    "PawMitten": "a soft blue-grey mitten shaped like an animal paw, pink felt pads and blunt cream claws, a wine velvet cuff",
    "HearthBrick": "a single old hearth brick, warm terracotta with soot at one end and a small teapot pressed into its face as a maker mark (no stars, no geometric or occult symbols)",
    "VerdictGavel": "a small judge's gavel in dark polished wood with brass bands",
    "TalismanIngot": "a small gold boat-shaped ingot with a cream paper charm strip pasted across it, the charm inked with a teal swirl (no readable text)",
    "KunaiFan": "three black kunai fanned out together, their grips wrapped in wine cloth with little tassels",
    "BareKnuckles": "a pair of rolled cream cloth hand-wraps with wine stitching, loosely tied, the kind wound round bare fists",
    # our own takes on pass 4's two unconfirmed items
    "FortuneGlobe": "a small fortune-teller's crystal globe on a brass claw stand with a wine velvet tassel, a violet-and-gold swirl of stars inside it, held by the stand's short handle",
    # bombs (18/10/2026): lobbed slowly, they burst in an area
    "PaperLanternBomb": "a round red-gold paper lantern bomb with a short cream fuse, hung from a short bamboo pole held at the left",
    "SnowGlobeGrenade": "a small snow globe with a tiny snowy pine inside, a brass base and a brass pull-ring on top, held by the ring",
    "PinataBomb": "a small five-point star pinata in wine, gold and teal paper fringe, on a short stick",
    "AcornCluster": "three plump acorns tied together with twine by their caps, each cap with a tiny fuse, held by the twine knot",
    "TopiaryShears": "a pair of long-bladed topiary shears with dark iron blades, wine-red wrapped handles and a brass pivot shaped like a little flower",
}
for wid, subject in W.items():
    add(wid + "_Held", subject, "held")

add("CherryBomb", "the cherry-black bomb lying on the floor, its candy-stripe fuse lit and sparking", "floor",
    ["fuse just lit, small spark", "fuse half burnt, bigger fizzing sparks, a red rim glows on the bomb", "fuse almost gone, sparks wild, red rim bright"])
add("CottonPop", "a explosion: a white cotton-cloud puff with black-and-gold confetti and a hand-lettered POP", "fx",
    ["tight bright puff just bursting", "puff fully open, confetti flying", "puff thinning, confetti falling", "last wisps and a few confetti flakes"])
add("PopperStick", "a red paper firecracker stick with a gold label lying on the floor, fuse fizzing", "floor", ["small spark", "bigger spark, stick shaking"])
add("PopperBurst", "a yellow-orange firecracker pop with ribbon streamers", "fx", ["bursting", "open with streamers", "fading puff"])
add("FeltBall", "a lime-green felt tennis ball with a white seam", "round")
add("BambooVaseShot", "the glass vase with bamboo tumbling through the air", "shot")
add("BambooShoot", "a green bamboo shoot springing out of the floor like a pop-up book, leaves unfurling", "floor",
    ["just breaking the floor, a small tip", "half grown with two leaves", "fully sprung, leaves open"])
add("GlassShards", "a few clear glass shards with glints, scattering", "fx")
add("BouquetShot", "the paper-wrapped rose bouquet spinning through the air", "shot")
add("PaperPetal", "a single red paper-cut rose petal", "round", ["a curled petal", "a flat petal seen from above", "a petal folded in half"])
add("HomeRunText", "the words HOME RUN! hand-lettered in cream with a dark ink outline and a star", "fx")
add("CandyDrillShot", "the red-and-white striped candy drill spinning point first", "shot")
add("SpiralCrack", "a spiral crack in the floor with candy shavings", "floor")
add("PlushOctopus", "the same pink PLUSH octopus as the held InkyOctopus weapon (Assets/ArcaneBrigade/Art/Arsenal/InkyOctopus_Held_v1.png): a stitched felt toy with cross-stitched button eyes, no hat, no bag, no clothes", "round",
    ["flying with arms trailing", "clinging, arms wrapped around (as if around an enemy, enemy not drawn)", "clinging, arms squeezing tighter", "hopping back, arms tucked"])
add("InkPool", "a dark violet-black ink splotch on the floor with a glossy edge", "floor")
add("ClawHead", "the three-finger grabber claw head on its spring", "shot", ["claw open", "claw snapped shut with a small spark"])
add("ClawSpring", "a brass coiled spring", "tile")
add("GoldLeafArc", "a wide crescent sweep made of folded gold leaf with tinsel stars, opening to the right", "fx")
add("DragonRibbon", "a ribbon of red-gold dragon scales", "tile")
add("LanternEmber", "a tiny red paper lantern ember glowing", "round")
add("TwinMoonSpin", "the two-bladed sun-and-moon staff spinning, seen from above", "round",
    ["horizontal", "turned 45 degrees", "vertical", "turned 135 degrees"])
add("PlushSharkShot", "the same stitched navy PLUSH shark as the held PlushShark weapon (Assets/ArcaneBrigade/Art/Arsenal/PlushShark_Held_v1.png), felt teeth and a button eye, no hat or clothes, diving forward, mouth open", "shot", ["mouth open wide", "mouth snapping shut"])
add("FloorRipple", "a water ripple V-wake on the floor", "floor")
add("BatterCrumbs", "a spray of golden fried-batter crumbs", "fx")
add("SteamPuff", "a soft warm steam puff", "fx", ["small", "full", "thinning"])
add("ShurikenShot", "a multicoloured origami shuriken spinning", "round", ["upright", "turned 45 degrees"])
add("PaperFlower", "a paper shuriken unfolded into a paper flower", "round")
add("WaterJet", "a short clear water jet blob with a faint rainbow", "shot")
add("StarDroplets", "star-shaped water droplets splashing", "fx")
add("Puddle", "a small clear water puddle", "floor")
add("CandyPellet", "a wrapped candy pellet", "shot")
add("CartoonCloud", "a cartoon muzzle cloud", "fx", ["tight puff", "big round cloud", "thinning cloud"])
add("ConfettiBurst", "a burst of confetti and candy wrappers", "fx")
add("PlungerCup", "a wine-red rubber suction cup", "shot")
add("ElasticBand", "a stretched red elastic cord", "tile")
add("PopText", "the word POP! hand-lettered with soap bubbles", "fx")
add("PlantedSign", "the round 'Dreamland Crossing' sign planted in the floor", "floor", ["standing still", "wobbling just after being planted"])
add("RoadStripes", "glowing cream road stripes with a running arrow", "tile")
add("ToyBomb", "a small round bomb with a wire plugged in, lying on the floor", "floor")
add("DetonatorWire", "a painted black-and-red wire", "tile")
add("CandyFlossCloud", "a candy-floss mushroom cloud raining tiny stars", "fx", ["rising", "full mushroom", "raining stars", "fading"])
add("Pinwheel", "the red-and-gold paper pinwheel planted in the floor", "floor",
    ["still", "turning slowly", "spinning fast with sparks", "spinning a blur with many sparks"])
add("PaperScraps", "red paper scraps flying", "fx")
add("BangText", "the word BANG! hand-lettered", "fx")
add("Gumball", "a shiny gumball", "round", ["cherry red", "mint teal", "lemon yellow", "grape purple"])
add("BalloonAnimal", "the twisted poodle balloon animal floating on its string", "round", ["string swaying left", "string swaying right"])
add("BalloonPop", "a balloon popping into a spinning deflating scrap, ribbons and an air ripple", "fx", ["the pop", "scrap spinning", "ribbons falling"])
add("JackBox", "the jack-in-the-box", "round", ["lid shut", "lid popping, spring rising", "a jester head springing out"])
add("TvStatic", "a small patch of colourful TV static", "fx")
add("SaucerShot", "a cyan flying saucer with a glowing dome", "round", ["lights on the rim lit", "the other rim lights lit"])
add("SaucerRing", "a thin cyan ring wake with tiny bubbles, seen from above", "round")
add("SoundFin", "a golden sound fin: a curved fan-shaped wave of brass musical shimmer opening to the right", "shot", ["narrow, just blown", "wider", "widest and fading"])
add("WindupKey", "a brass wind-up key", "round", ["upright", "turned 90 degrees"])
add("DizzyStars", "a little ring of three cartoon dizzy stars", "fx")
add("CorkBullet", "a cork flying point first with a short string", "shot")
add("CorkPop", "a cork-gun muzzle puff with a POP", "fx")
add("Fastball", "a white baseball with red stitches spinning", "round")
add("MittCatch", "a little THWAP of a ball landing in a mitt: a leather-brown burst with a dust ring", "fx")
add("SodaBottleShot", "the green soda bottle tumbling", "shot")
add("FizzPuddle", "a puddle of green soda fizzing with bubbles on the floor", "floor", ["just splashed, wide", "bubbling", "flattening, fewer bubbles"])
add("LeekSwipe", "a crescent swipe trail of fresh green leek streaks opening to the right", "fx")
add("SwatPrint", "a flat star-mesh swat print with a little dust puff", "floor")
add("TwigDart", "a small forked wooden twig flying point first, a cyan glint at its tip", "shot")
add("DowseMark", "a cyan dowsing ripple mark: a small glowing water-drop symbol", "fx")
add("StarShard", "a small silver star shard", "shot")
add("PunchStar", "a cartoon punch impact star, silver and cream", "fx")
add("BuzzerFloor", "the yellow buzzer block lying on the floor, red button up", "floor", ["button up, still", "button pressed, the block jumping with little lightning sparks"])
add("ZapChain", "a short zig-zag of friendly cyan-gold lightning, horizontal", "tile")
# research pass 4
add("PawSwipe", "three short curved claw-swipe streaks in cream and soft blue-grey, opening to the right", "fx")
add("BrickShot", "the terracotta hearth brick tumbling", "round", ["long side facing us", "turned end over end, short end facing us"])
add("BrickDust", "a puff of warm terracotta brick dust with a few chips", "fx", ["the thud, a tight low puff", "the puff spreading wide and low", "settling, a few chips left"])
add("GavelStamp", "a big golden gavel head coming down onto the floor", "fx",
    ["the gold gavel head raised high, about to fall", "the gavel head striking the floor with a cream impact ring", "the head lifting away, a ring of dust left"])
add("IngotShot", "the charm-sealed gold ingot spinning, the paper charm fluttering", "round", ["charm strip facing us", "turned 90 degrees, the charm strip trailing"])
add("IngotBlast", "a small burst of gold leaf and torn cream charm paper with teal ink flecks", "fx", ["tight bright burst", "open, paper scraps flying", "fading, a few flakes falling"])
add("KunaiShot", "a single black kunai flying point first, a wine tassel trailing", "shot")
add("KnucklePop", "a small pop of air ahead of a punch: a cream-and-teal ring burst with little star flecks", "fx", ["the pop, tight", "the ring opening", "the ring fading"])
# the bombs' thrown objects
add("LanternBombShot", "the red-gold paper lantern bomb alone, its fuse lit with a small cream spark, tumbling", "round")
add("SnowGlobeShot", "the little snow globe alone, snow swirling inside, tumbling", "round")
add("PinataShot", "the star pinata alone, paper fringe fluttering, tumbling", "round")
add("CandyBomblet", "a small round wrapped candy with a tiny lit fuse", "round")
add("AcornBomb", "a single plump acorn with a tiny lit fuse in its cap", "round")
add("GlobeMote", "a small violet crystal sphere with a swirl of gold stars inside it, a soft lilac sheen", "round")
add("FistJab", "a cartoon jab impact: a short cream star-burst with three speed ticks, opening to the right", "fx")

# every ranged weapon gets its own shot, redrawn in the current painted style (18/10/2026); each job takes the weapon itself as
# its first reference so the shot matches it. ProjectileVisual draws Shot_<WeaponId> whenever it exists.
SHOTS = {'TeapotPopgun': 'a hot tea drop with a curl of steam, amber with a cream highlight', 'MailboxLauncher': 'a small sealed envelope with a wine wax seal, flying flat', 'BellFork': 'a small brass bell chime ringing, a tiny clapper swinging', 'CrescentBow': 'a slim arrow with a crescent-moon arrowhead and cream fletching', 'RuneStaff': 'a violet rune stone glowing with one carved glyph', 'SleepCamera': 'a small dreamy photograph of a sleeping crescent moon, corners curling', 'TinFish': 'a little tin sardine with riveted fins, glinting', 'AlarmMortar': 'a small round brass alarm-clock shell with two little bells on top', 'MusicBox': 'a brass music-box pin drum note: a round golden note with a tiny comb tooth', 'ButtonBow': 'a large sewing button dart with four holes and a trailing thread', 'LeafCrossbow': 'a pointed green leaf bolt with a visible vein', 'InkPistol': 'a glossy midnight-blue ink drop with a gold nib-shaped tip', 'BubbleWand': 'a soap bubble with a rainbow sheen and a tiny star reflection', 'StarKazoo': 'a little paper-cut star with a buzzing kazoo wave line', 'ShellCannon': 'a spiral seashell cannonball, cream and coral', 'AcornSling': 'a polished acorn pellet with a brass cap', 'CrescentBoomerang': 'a wooden crescent boomerang painted with a teal stripe, spinning', 'DartCompass': 'a compass needle dart, red-and-cream needle with a brass pivot', 'LanternStaff': 'a small floating lantern flame in a brass cage', 'SnowGlobeOrb': 'a shard of a snow globe with a snowflake trapped inside', 'RibbonHarp': 'a curling golden harp string with a tiny wine ribbon bow, flying like a note', 'DreamOrbStaff': 'a swirling dream orb of violet and teal clouds with a gold rim', 'FireExtinguisher': 'a soft cream foam puff with bubbles', 'PlasmaTrident': 'a three-pronged bolt of teal light shaped like a small trident head', 'MoonFeatherFan': 'a silver moon feather drifting edge-first', 'ScrapMagnet': 'a horseshoe magnet bolt, red and silver, with little spark lines', 'NeedleSinger': 'a silver sewing needle trailing a short wine thread', 'DreamProjector': 'a single film frame showing a tiny star, with sprocket holes', 'MoonseedOrb': 'a pale glowing moon seed: a crescent-shaped seed pod with a tiny green sprout', 'SatelliteDisc': 'a small spinning tin satellite disc with a short antenna and a teal dish', 'CherryFuseBomb': 'the cherry-black bomb tumbling, candy-stripe fuse lit', 'PopperSticks': 'a bundle of three wine-red firecracker sticks tied with string, fuses lit, tumbling end over end', 'DreamRacket': 'a lime felt tennis ball with a star-shaped seam', 'BambooVase': 'the glass vase with bamboo, tumbling, a little water splashing out', 'RoseBouquet': 'the paper-wrapped rose bouquet spinning, petals loosening', 'CandyDrill': 'a spinning wine-and-cream striped candy drill bit, point first', 'GrabberClaw': 'the three-finger grabber claw head flying open, spring trailing', 'TwinMoonStaff': 'the sun-and-moon double blade spinning flat like a wheel', 'PaperShuriken': 'a multicoloured origami shuriken spinning', 'SquirtTrio': 'a clear water bullet with a faint rainbow and a droplet tail', 'ConfettiBlunderbuss': 'a twist-wrapped candy pellet with confetti flecks', 'PlungerPopper': 'a wine-red rubber suction cup flying cup-first on an elastic tail', 'GlitchToybox': 'a tiny jester head on a spring, popping forward', 'FizzBottle': 'the green soda bottle tumbling, cap fizzing', 'HearthBrick': 'a small terracotta hearth brick tumbling corner-first, soot-darkened on one face, warm amber ember cracks glowing along one edge, a pressed maker mark of a tiny teapot on its side (no stars, no geometric symbols), crumbs of brick dust trailing behind', 'TalismanIngot': 'the charm-sealed gold ingot spinning, paper charm fluttering', 'InkyOctopus': 'the pink plush octopus flying with arms trailing', 'PlushShark': 'the stitched navy plush shark diving mouth-first', 'DetonatorBox': 'a small round toy bomb with a wire tail, flying', 'FirecrackerPinwheel': 'a spinning paper pinwheel shedding sparks', 'GumballGatling': 'a glossy cherry gumball with a cream candy swirl wrapped round it and a few rainbow sugar sprinkles stuck on, followed in a short line by three smaller gumballs (mint, lemon, grape) shrinking behind it; no wrapper, no fuse', 'BalloonBuddy': 'the cream twisted poodle balloon floating, string trailing', 'SaucerBlaster': 'a little flying saucer with a glowing cyan dome', 'FanfareHorn': 'a golden fan-shaped sound fin of brass shimmer opening forward', 'CorkRifle': 'a cork flying point first with a short string', 'CatcherMitt': 'a white baseball with red stitches spinning', 'DowsingRod': 'a small forked twig flying point first, a cyan glint at its tip', 'BuzzerBlock': 'a small yellow buzzer with a red button, crackling', 'KunaiFan': 'a single black kunai flying point first, a wine tassel trailing'}
SHOT_REFS = {'TeapotPopgun': 'Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png', 'MailboxLauncher': 'Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png', 'BellFork': 'Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png', 'CrescentBow': 'Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png', 'RuneStaff': 'Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png', 'SleepCamera': 'Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png', 'TinFish': 'Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png', 'AlarmMortar': 'Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png', 'MusicBox': 'Assets/ArcaneBrigade/Art/QualityHD/ExpandedWeapons_v2.png', 'ButtonBow': 'Assets/ArcaneBrigade/Art/K10/ButtonBow.png', 'LeafCrossbow': 'Assets/ArcaneBrigade/Art/K10/LeafCrossbow.png', 'InkPistol': 'Assets/ArcaneBrigade/Art/K10/InkPistol.png', 'BubbleWand': 'Assets/ArcaneBrigade/Art/K10/BubbleWand.png', 'StarKazoo': 'Assets/ArcaneBrigade/Art/K10/StarKazoo.png', 'ShellCannon': 'Assets/ArcaneBrigade/Art/K10/ShellCannon.png', 'AcornSling': 'Assets/ArcaneBrigade/Art/K10/AcornSling.png', 'CrescentBoomerang': 'Assets/ArcaneBrigade/Art/K10/CrescentBoomerang.png', 'DartCompass': 'Assets/ArcaneBrigade/Art/K10/DartCompass.png', 'LanternStaff': 'Assets/ArcaneBrigade/Art/K10/LanternStaff.png', 'SnowGlobeOrb': 'Assets/ArcaneBrigade/Art/K10/SnowGlobeOrb.png', 'RibbonHarp': 'Assets/ArcaneBrigade/Art/K10/RibbonHarp.png', 'DreamOrbStaff': 'Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png', 'FireExtinguisher': 'Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png', 'PlasmaTrident': 'Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png', 'MoonFeatherFan': 'Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png', 'ScrapMagnet': 'Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png', 'NeedleSinger': 'Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png', 'DreamProjector': 'Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png', 'MoonseedOrb': 'Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png', 'SatelliteDisc': 'Assets/ArcaneBrigade/Art/K10/CrescentBoomerang.png', 'CherryFuseBomb': 'Assets/ArcaneBrigade/Art/Arsenal/CherryFuseBomb_Held_v1.png', 'PopperSticks': 'Assets/ArcaneBrigade/Art/Arsenal/PopperSticks_Held_v1.png', 'DreamRacket': 'Assets/ArcaneBrigade/Art/Arsenal/DreamRacket_Held_v1.png', 'BambooVase': 'Assets/ArcaneBrigade/Art/Arsenal/BambooVase_Held_v1.png', 'RoseBouquet': 'Assets/ArcaneBrigade/Art/Arsenal/RoseBouquet_Held_v1.png', 'CandyDrill': 'Assets/ArcaneBrigade/Art/Arsenal/CandyDrill_Held_v1.png', 'GrabberClaw': 'Assets/ArcaneBrigade/Art/Arsenal/GrabberClaw_Held_v1.png', 'TwinMoonStaff': 'Assets/ArcaneBrigade/Art/Arsenal/TwinMoonStaff_Held_v1.png', 'PaperShuriken': 'Assets/ArcaneBrigade/Art/Arsenal/PaperShuriken_Held_v1.png', 'SquirtTrio': 'Assets/ArcaneBrigade/Art/Arsenal/SquirtTrio_Held_v1.png', 'ConfettiBlunderbuss': 'Assets/ArcaneBrigade/Art/Arsenal/ConfettiBlunderbuss_Held_v1.png', 'PlungerPopper': 'Assets/ArcaneBrigade/Art/Arsenal/PlungerPopper_Held_v1.png', 'GlitchToybox': 'Assets/ArcaneBrigade/Art/Arsenal/GlitchToybox_Held_v1.png', 'FizzBottle': 'Assets/ArcaneBrigade/Art/Arsenal/FizzBottle_Held_v1.png', 'HearthBrick': 'Assets/ArcaneBrigade/Art/Arsenal/HearthBrick_Held_v1.png', 'TalismanIngot': 'Assets/ArcaneBrigade/Art/Arsenal/TalismanIngot_Held_v1.png', 'InkyOctopus': 'Assets/ArcaneBrigade/Art/Arsenal/InkyOctopus_Held_v1.png', 'PlushShark': 'Assets/ArcaneBrigade/Art/Arsenal/PlushShark_Held_v1.png', 'DetonatorBox': 'Assets/ArcaneBrigade/Art/Arsenal/DetonatorBox_Held_v1.png', 'FirecrackerPinwheel': 'Assets/ArcaneBrigade/Art/Arsenal/FirecrackerPinwheel_Held_v1.png', 'GumballGatling': 'Assets/ArcaneBrigade/Art/Arsenal/GumballGatling_Held_v1.png', 'BalloonBuddy': 'Assets/ArcaneBrigade/Art/Arsenal/BalloonBuddy_Held_v1.png', 'SaucerBlaster': 'Assets/ArcaneBrigade/Art/Arsenal/SaucerBlaster_Held_v1.png', 'FanfareHorn': 'Assets/ArcaneBrigade/Art/Arsenal/FanfareHorn_Held_v1.png', 'CorkRifle': 'Assets/ArcaneBrigade/Art/Arsenal/CorkRifle_Held_v1.png', 'CatcherMitt': 'Assets/ArcaneBrigade/Art/Arsenal/CatcherMitt_Held_v1.png', 'DowsingRod': 'Assets/ArcaneBrigade/Art/Arsenal/DowsingRod_Held_v1.png', 'BuzzerBlock': 'Assets/ArcaneBrigade/Art/Arsenal/BuzzerBlock_Held_v1.png', 'KunaiFan': 'Assets/ArcaneBrigade/Art/Arsenal/KunaiFan_Held_v1.png'}
for wid, subject in SHOTS.items():
    OBJECTS["Shot_" + wid] = 1
    LOOKS_LIKE_PATH["Shot_" + wid] = SHOT_REFS.get(wid)
    add("Shot_" + wid, "the shot fired by the weapon " + wid + " (the first reference shows that weapon; if it is a sheet of several weapons, " + wid + " is the one it was cut from): " + subject + ". Small and readable at gameplay size, its own shape and colours, unlike any other weapon's shot", "shot")

# animation art (Docs/Bizarre_Arsenal.md §5.2)
heroes = ["LanternKnight", "Postmaster", "DrowsySeer", "RootOracle", "LoneJelly", "GlassDice", "TideSinger", "CometRunner", "MirrorKeeper",
          "TreasureAcorn", "VerdantMatron", "CopperCauldron", "ChinaMarionette", "SoapSkater", "VelvetRegent", "ButtonCavalier", "NightMaw",
          "ThornTreasurer", "CitrusMarshal", "IvorySovereign", "PetalCitadel", "PatchworkHost", "ArchiveCurator", "PennyBroker", "StormFencer",
          "TinVirtuoso", "MoonProwler", "SupperMortar", "LanternWeaver", "HourglassStag", "RainArchivist"]
idle = ["neutral pose, breathing in", "chest risen, slight lift", "breathing out, slight settle", "neutral with a small blink or head tilt"]
walk = ["contact, left foot forward", "down, weight on left", "passing, right leg lifting", "up, highest point",
        "contact, right foot forward", "down, weight on right", "passing, left leg lifting", "up, highest point"]
# the ten original heroes have acting sheets; the twenty-one October heroes are drawn four to a sheet (Heroes1..6), in roster order
october = heroes[10:]
def hero_ref(hero):
    if hero not in october:
        return ACT + hero + "_Acting_v1.png", "the playable character in this acting sheet"
    i = october.index(hero)
    return (f"Assets/ArcaneBrigade/Art/October/Heroes{i // 4 + 1}.png",
            f"figure number {i % 4 + 1} of the four in this sheet (reading order), the playable character {hero}")
hero_jobs = []  # optional: written to their own list, not run with the arsenal (no importer reads them yet)
for hero in heroes:
    ref, who = hero_ref(hero)
    for clip, poses in (("Idle", idle), ("Walk", walk)):
        for i, pose in enumerate(poses):
            n = f"{hero}_{clip}_f{i}"
            hero_jobs.append(dict(name=n, path=ACT + n + "_v1.png", transparent=True, refs=[ref], depends=None if i == 0 else f"{hero}_{clip}_f0", mode="single",
                             prompt="Use case: precise-object-edit. Using " + who + " as the exact design reference "
                                    f"(same character, outfit, colours, proportions, ink style), draw ONE {clip.lower()} animation frame: {pose}. "
                                    "Facing right, elevated front three-quarter game view, whole body, same scale in every frame, feet on the same "
                                    "baseline at 85% canvas height, 768x768 canvas, genuinely transparent RGBA, no ground, shadow, text or UI."))
for i, step in enumerate(["coin face-on", "coin turned 30 degrees", "coin turned 60 degrees", "coin edge-on (thin brass rim)", "coin turned 120 degrees showing the back", "coin back nearly face-on"]):
    n = f"CoinSpin_f{i}"
    jobs.append(dict(name=n, path=ART + n + "_v1.png", transparent=True, refs=["Assets/ArcaneBrigade/Art/CombatVFX/Allied/AllySpark_v1.png"], depends=None if i == 0 else "CoinSpin_f0",
                     mode="single", prompt=STYLE + f"Subject: a brass dream coin with a moon stamp spinning: {step}." + ROUND + ISOLATED))


# hand-painted rarity description frames (replace the procedural Tools/Art/rarity_frame_art.py output, same names)
RF = "Assets/ArcaneBrigade/Art/RarityFramePainted/"  # moved over Resources/RarityFrame once all five parts of a rarity exist
RF_REFS = ["Assets/ArcaneBrigade/Art/AnimationHD/StrangeMerchants_v2.png", "Assets/ArcaneBrigade/Art/FinalQuality/furniture-00_v1.png", "Assets/ArcaneBrigade/Resources/MienCho/ActOne/Prop_BoundaryGate.png"]
rarities = {
    "Uncommon": ("moon-vine", "a silver-blue moonlit vine with small curled leaves and glinting dew drops",
                 "a crescent moon bud between pairs of opening silver-blue leaves, with three tiny stars above",
                 "two drooping silver-blue leaves with a dew drop", "a curled silver-blue vine tendril with one leaf"),
    "Rare": ("rune rail", "two carved violet stone rails with round rune medallions (glowing lavender glyphs) and small amethyst diamond studs between them",
             "a carved violet stone medallion with a glowing lavender rune-eye, flanked by two carved bars set with amethyst studs",
             "a violet rune medallion hanging on a cord with a lavender silk tassel", "a square amethyst stud set in carved violet stone"),
    "Mythic": ("gilded wings", "golden feathers cascading down, overlapping like a wing, along a beaded gold cord; the cord runs vertically at 77% of the width and the feathers sweep out to the LEFT",
               "a gilded sun crown with rays and a ruby, with a pair of golden wings spreading to both sides",
               "a gold teardrop jewel hanging between two small golden feathers", "a gold fleur-de-lis with two small golden feathers"),
    "Crimson": ("blood briar", "two black-red thorn stems wound around each other, with red roses and sharp thorns",
                "two curving dark-red horns rising either side of a glowing ruby and a red rose, dark leaves below",
                "a red rose bud hanging on a thorned stem", "a knot of thorned stem with a small red rose"),
    "Rainbow": ("celestial prism", "a silver chain of prism crystal shards (each shard holding a rainbow) and little white four-point stars",
                "a white crescent moon with a fan of rainbow prism crystals behind it and small stars hanging below",
                "a rainbow eight-point star pendant on a silver thread", "a white four-point star with two small rainbow prism shards"),
}
FRAME_STYLE = ("Use case: style-transfer. Production raster UI ornament for Arcane Brigade's item description frames (dream-world storybook "
               "roguelite). The input images are STYLE REFERENCES ONLY: match their newer layered painterly generation, patterning and "
               "metallic sheen. Hand-painted, confident dark ink contours, rich painterly "
               "shading with highlights, crafted like jewellery or carved trim, readable at small size. Never flat vector, never 3D render. "
               "Genuinely transparent RGBA background, nothing else drawn, no text, no frame or panel, no shadow under it. ")
for rarity, (theme, side, crest, foot, corner) in rarities.items():
    stem = "at 77% of the width (feathers extending left of it)" if rarity == "Mythic" else "down the exact middle"
    jobs.append(dict(name=rarity + "_Side", path=RF + rarity + "_Side.png", transparent=True, refs=RF_REFS, depends=None, mode="single",
                     prompt=FRAME_STYLE + f"Subject ({theme}): a vertical ornamental trim strip: {side}. Its main line runs {stem}. "
                     "It MUST tile seamlessly VERTICALLY: the top edge continues exactly into the bottom edge (repeat two motifs per tile). "
                     "Portrait 512x1024 canvas, the trim fills the full height and about 70% of the width."))
    jobs.append(dict(name=rarity + "_Glow", path=RF + rarity + "_Glow.png", transparent=True, refs=[RF + rarity + "_Side.png"], depends=rarity + "_Side", mode="single",
                     prompt="Use case: precise-object-edit. EDIT THIS EXACT strip: keep ONLY its glowing parts (gems, glyphs, dew, beads, crystal "
                            "cores, rose hearts) exactly where they are, brightened to a soft light, and make everything else fully transparent. "
                            "Same canvas size and alignment; it must still tile vertically. Genuinely transparent RGBA."))
    jobs.append(dict(name=rarity + "_Crest", path=RF + rarity + "_Crest.png", transparent=True, refs=RF_REFS, depends=None, mode="single",
                     prompt=FRAME_STYLE + f"Subject ({theme}): a crest that sits on the TOP edge of a wide panel, centred: {crest}. "
                     "Its seat (the line where it meets the panel's top edge) is at 62% of the canvas height. Landscape 1536x640 canvas."))
    jobs.append(dict(name=rarity + "_Foot", path=RF + rarity + "_Foot.png", transparent=True, refs=RF_REFS, depends=None, mode="single",
                     prompt=FRAME_STYLE + f"Subject ({theme}): a small pendant hanging DOWN from the bottom edge of a panel, centred: {foot}. "
                     "It hangs from the top 18% of the canvas. 768x384 canvas."))
    jobs.append(dict(name=rarity + "_Corner", path=RF + rarity + "_Corner.png", transparent=True, refs=RF_REFS, depends=None, mode="single",
                     prompt=FRAME_STYLE + f"Subject ({theme}): a corner cap for the TOP-LEFT corner of a panel: {corner}. Its centre bead sits at "
                     "16% across and 16% down; arms run along the top edge to the right and down the left edge. 512x512 canvas."))

# the star-up ceremony's rank emblems (18/10/2026): MergeReveal draws inked stars and a brush crown until these exist
STAR_REFS = ["Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Star_v1.png"] + REFS
for name, subject in (("StarUp_SilverStar", "a five-point rank star of polished silver with a cream enamel centre and a thin midnight ink contour, "
                                            "slightly rounded points, one small highlight; no face"),
                      ("StarUp_GoldStar", "a five-point rank star of burnished gold with an amber enamel centre and a thin dark-brown ink contour, "
                                          "slightly rounded points, a fine engraved border and one bright highlight; grander than a silver star; no face"),
                      ("StarUp_Crown", "a small storybook crown of burnished gold: three rounded points, the middle one tallest, a wine-red jewel on "
                                       "the middle point and teal jewels on the side points, an amber velvet band; seen from the front")):
    jobs.append(dict(name=name, path=ART + name + "_v1.png", transparent=True, refs=STAR_REFS, depends=None, mode="single",
                     prompt=STYLE + "Subject: " + subject + ROUND + ISOLATED))

# hero awakenings (18/10/2026, pilot: Postmaster, CopperCauldron, DrowsySeer): each hero's transformed piece and its own
# gauge vessel. The Full gauge is an exact edit of the Empty one (same framing), because the game reveals the Full one
# from the bottom up over the Empty one as the gauge fills.
GAUGE = (" A tall narrow upright gauge vessel seen from the front, about one third as wide as it is tall, standing straight, "
         "filling the canvas height except a 6% empty margin at top and bottom. Two small tier notches on its LEFT side at one "
         "third and two thirds of its height. 384x1024 canvas.")
AWAKE = {
    "Postmaster": dict(
        refs=["Assets/ArcaneBrigade/Art/FinalQuality/Postmaster_v1.png", "Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png"],
        piece=("Postmaster_Awake_Held", "SORTING GATLING, the Postmaster's mailbox launcher (in the second reference) rebuilt as a "
               "six-barrel parcel sorter: six brass mail-chute barrels bundled in a rotating cylinder at the front, a red mailbox body "
               "with a wax-seal badge, a small hand crank on the side, the corners of letters peeking out of the barrels", HELD),
        empty="a stitched leather mail sack standing upright, brass buckles and a strap, a tall postage-stamp-edged window down its "
              "front; the window is EMPTY, showing only the dark inside of the sack",
        full="the window is now stuffed to the top with envelopes and small parcels, glowing a warm lamplight amber"),
    "CopperCauldron": dict(
        refs=["Assets/ArcaneBrigade/Art/October/Heroes1.png", "Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png"],
        piece=("CopperCauldron_Engine", "STEAM ENGINE: a small storybook locomotive front that the copper kettle hero (in the first "
               "reference sheet) wears as it charges: a brass cowcatcher grille, a stubby copper smokestack, a round headlamp, "
               "rivets and a little whistle; side view facing RIGHT, no wheels, no ground", " Side view facing RIGHT. 768x512 canvas."),
        empty="a brass boiler pressure tube with a long glass window, copper rivets, a red line painted across the glass near the "
              "top, a little steam valve on its cap; the glass is EMPTY and dark",
        full="the glass is now filled to the top with glowing ember-orange steam and swirling heat"),
    "DrowsySeer": dict(
        refs=["Assets/ArcaneBrigade/Art/FinalQuality/DrowsySeer_v1.png", "Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png"],
        piece=("DrowsySeer_Awake_Held", "FULL MOON, the Drowsy Seer's moonseed orb staff (in the second reference sheet) in its "
               "full-moon form: a slender silver-blue staff topped by a full moon orb cradled in a crescent, sleepy star charms "
               "dangling from it, a soft lilac light painted inside the orb (no external halo)", HELD),
        empty="a slender lilac glass column framed in silver, three small crescent-moon charms at its tier notches and a star "
              "finial on top; the glass is EMPTY, a dark night blue",
        full="the glass is now filled to the top with luminous lilac moonlight and tiny drifting stars"),
}
for hero, a in AWAKE.items():
    name, subject, shape = a["piece"]
    jobs.append(dict(name=name, path=ART + name + "_v1.png", transparent=True, refs=a["refs"] + REFS, depends=None, mode="single",
                     prompt=STYLE + "Subject: " + subject + "." + shape + ISOLATED))
    empty = "Gauge_" + hero + "_Empty"
    jobs.append(dict(name=empty, path=ART + empty + "_v1.png", transparent=True, refs=a["refs"][:1] + REFS, depends=None, mode="single",
                     prompt=STYLE + "Subject: the gauge of the hero in the first reference, " + a["empty"] + "." + GAUGE + ISOLATED))
    full = "Gauge_" + hero + "_Full"
    jobs.append(dict(name=full, path=ART + full + "_v1.png", transparent=True, refs=[ART + empty + "_v1.png"], depends=empty, mode="single",
                     prompt="Use case: precise-object-edit. EDIT THIS EXACT gauge: keep the vessel's outline, frame, size, position "
                            "and canvas identical (it is laid over the empty one in the game), and change only what is inside: "
                            + a["full"] + ". Genuinely transparent RGBA background, same 384x1024 canvas."))

out = ROOT / "Docs" / "Bizarre_Arsenal_Art_Worklist.json"
out.write_text(json.dumps(jobs, indent=1, ensure_ascii=False), encoding="utf-8")
print(len(jobs), "jobs ->", out)
optional = ROOT / "Docs" / "Bizarre_Hero_Animation_Worklist.json"
optional.write_text(json.dumps(hero_jobs, indent=1, ensure_ascii=False), encoding="utf-8")
print(len(hero_jobs), "optional hero animation jobs ->", optional)
