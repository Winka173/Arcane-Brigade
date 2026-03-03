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
REFS = ["Assets/ArcaneBrigade/Art/QualityHD/Weapons_v2.png", "Assets/ArcaneBrigade/Art/K14/ClockworkArsenal_v3.png",
        "Assets/ArcaneBrigade/Art/CombatVFX/Allied/AllySpark_v1.png"]
STYLE = ("Use case: style-transfer. Production raster art for Arcane Brigade, a storybook dream-toy roguelite. "
         "The input sheets are STYLE REFERENCES ONLY, not edit targets. Match confident dark ink contours, rounded slightly "
         "crooked asymmetric toy forms, rich painterly storybook shading, warm antique brass, burgundy velvet, deep teal and "
         "midnight blue, cream highlights. Readable silhouette at gameplay size (about 40-60 px tall on a 1080p screen). "
         "Clearly drawn illustration, never photo, 3D render, PBR or plastic sheen. ")
ISOLATED = (" ONE complete isolated subject on a genuinely transparent RGBA background, centred, with 12% empty margin. "
            "No ground plane, cast shadow, external halo, lettering (unless the subject IS lettering), labels, UI, grid or watermark. No gore.")
HELD = (" Draw the weapon alone, lying horizontally and POINTING RIGHT (+X), its grip or handle at the LEFT about 20% across, "
        "as a companion would hold it out in front. 1024x512 canvas.")
SHOT = " Draw it travelling to the RIGHT (+X), motion implied by its shape only (no speed lines outside the object). 512x512 canvas."
ROUND = " Round or spinning object seen from slightly above. 512x512 canvas."
FLOOR = " Seen from the elevated three-quarter game view, lying on the floor (no floor drawn). 512x512 canvas."
FX = " A short combat effect, friendly colours (brass, amber, cream, teal, candy colours; never hostile ember red-black). 768x768 canvas."
TILE = " A seamless horizontal tile: the left and right edges must join perfectly. 1024x256 canvas."

jobs = []


def add(name, subject, kind, frames=None):
    """frames: list of per-frame subjects; the first is drawn new, the rest are edits of it"""
    shape = {"held": HELD, "shot": SHOT, "round": ROUND, "floor": FLOOR, "fx": FX, "tile": TILE}[kind]
    if not frames:
        jobs.append(dict(name=name, path=ART + name + "_v1.png", transparent=True, refs=REFS, depends=None, mode="single",
                         prompt=STYLE + "Subject: " + subject + shape + ISOLATED))
        return
    first = name + "_f0"
    jobs.append(dict(name=first, path=ART + first + "_v1.png", transparent=True, refs=REFS, depends=None, mode="single",
                     prompt=STYLE + "Subject: " + subject + " " + frames[0] + shape + ISOLATED))
    for i, step in enumerate(frames[1:], 1):
        n = f"{name}_f{i}"
        jobs.append(dict(name=n, path=ART + n + "_v1.png", transparent=True, refs=[ART + first + "_v1.png"], depends=first, mode="single",
                         prompt="Use case: precise-object-edit. EDIT THIS EXACT image (the approved frame 0) into the next animation frame. "
                                "Keep the same object, design, colours, scale, canvas size and centre position; change ONLY: " + step +
                                " Genuinely transparent RGBA, nothing else added."))


W = {
    "CherryFuseBomb": "a glossy cherry-black toy bomb with two cherry leaves and a candy-stripe fuse, held by a little brass loop handle",
    "PopperSticks": "a bundle of three red paper firecracker sticks with gold labels and short fuses, tied with string",
    "DreamRacket": "a toy tennis racket with a cream-and-teal wrapped grip, brass frame and loose star-patterned strings",
    "BambooVase": "a clear glass vase holding three green bamboo stems with leaves, gripped at its neck",
    "RoseBouquet": "a bouquet of red roses wrapped in white paper with a red bow",
    "HomeRunBat": "a wooden toy baseball bat with brass bands, a cream grip and a little star painted on the barrel",
    "CandyDrill": "a red-and-white striped candy drill / spiral lance with a brass grip",
    "InkyOctopus": "a pink plush toy octopus with button eyes and curling stitched arms, held by one arm",
    "GrabberClaw": "a toy spring grabber: a brass trigger handle, a coiled spring and a three-finger claw at the right end",
    "StarforgeClaymore": "a huge golden claymore with engraved glowing runes, still wearing a chip of the toy stone it was pulled from at the crossguard",
    "DragonLanternGlaive": "a long glaive whose head is a red-gold paper-lantern dragon biting a crescent blade, tassels on the shaft",
    "TwinMoonStaff": "a staff with a blade at each end: a sun-gold blade on the left end, a moon-silver blade on the right end, wrapped grip in the middle",
    "PlushShark": "a big stitched plush shark held by its tail, felt teeth, button eye",
    "DrumstickClub": "a giant toy fried-chicken drumstick club with golden crispy batter and a cream plastic bone handle",
    "PaperShuriken": "a fan of three multicoloured origami shuriken held between fingers-like paper clip",
    "SquirtTrio": "a clear toy water pistol with three nozzles, water sloshing visibly inside, teal and brass trim",
    "ConfettiBlunderbuss": "a toy bell-mouthed blunderbuss in brass and burgundy, candy stripes on the stock",
    "PlungerPopper": "a toy plunger launcher: a red rubber suction cup on an elastic cord coiled round a brass handle",
    "SignpostSlammer": "a round toy road sign 'Dreamland Crossing' (blue face with a cream arrow) on a striped pole",
    "DetonatorBox": "a small wooden detonator box with a brass T-plunger on top and a coil of painted wire",
    "FirecrackerPinwheel": "a red-and-gold paper pinwheel on a stick with tiny firecrackers on its vanes",
    "GumballGatling": "a gumball machine turned into a hand-crank gatling: glass globe full of multicoloured gumballs, brass barrels and crank",
    "BalloonBuddy": "a twisted balloon animal (a poodle) on a string tied to a brass ring handle",
    "GlitchToybox": "a jack-in-the-box with a brass crank, lid shut, slightly glitching colour fringe on its edges",
    "SaucerBlaster": "a blue toy ray gun shaped like a flying saucer, a cyan dome on top and a ring nozzle",
    "FanfareHorn": "a golden flared fanfare horn with a brown wooden mouthpiece and a tassel",
    "WindupWrench": "a big white toy wrench with a brass wind-up key on its handle",
    "CorkRifle": "a brown wooden toy hunting rifle that fires corks, a cork on a string at the muzzle",
    "CatcherMitt": "a brown leather baseball catcher's mitt worn on a little hand, a ball tucked in it",
    "FizzBottle": "a green glass soda bottle with a yellow-dotted label and a fizzing cap",
    "LeekBaton": "a long fresh spring onion / leek held like a baton, green leaves at the right end",
    "FlySwatter": "a red toy fly swatter with a cream handle and a mesh of little stars",
}
for wid, subject in W.items():
    add(wid + "_Held", subject, "held")

add("CherryBomb", "the cherry-black toy bomb lying on the floor, its candy-stripe fuse lit and sparking", "floor",
    ["fuse just lit, small spark", "fuse half burnt, bigger fizzing sparks, a red rim glows on the bomb", "fuse almost gone, sparks wild, red rim bright"])
add("CottonPop", "a toy explosion: a white cotton-cloud puff with black-and-gold confetti and a hand-lettered POP", "fx",
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
add("PlushOctopus", "the pink plush octopus", "round",
    ["flying with arms trailing", "clinging, arms wrapped around (as if around an enemy, enemy not drawn)", "clinging, arms squeezing tighter", "hopping back, arms tucked"])
add("InkPool", "a dark violet-black ink splotch on the floor with a glossy edge", "floor")
add("ClawHead", "the three-finger toy grabber claw head on its spring", "shot", ["claw open", "claw snapped shut with a small spark"])
add("ClawSpring", "a brass coiled spring", "tile")
add("GoldLeafArc", "a wide crescent sweep made of folded gold leaf with tinsel stars, opening to the right", "fx")
add("DragonRibbon", "a ribbon of red-gold dragon scales", "tile")
add("LanternEmber", "a tiny red paper lantern ember glowing", "round")
add("TwinMoonSpin", "the two-bladed sun-and-moon staff spinning, seen from above", "round",
    ["horizontal", "turned 45 degrees", "vertical", "turned 135 degrees"])
add("PlushSharkShot", "the plush shark diving forward, mouth open", "shot", ["mouth open wide", "mouth snapping shut"])
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
add("PlungerCup", "a red rubber suction cup", "shot")
add("ElasticBand", "a stretched red elastic cord", "tile")
add("PopText", "the word POP! hand-lettered with soap bubbles", "fx")
add("PlantedSign", "the round 'Dreamland Crossing' sign planted in the floor", "floor", ["standing still", "wobbling just after being planted"])
add("RoadStripes", "glowing cream road stripes with a running arrow", "tile")
add("ToyBomb", "a small round toy bomb with a wire plugged in, lying on the floor", "floor")
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
add("SaucerShot", "a cyan toy flying saucer with a glowing dome", "round", ["lights on the rim lit", "the other rim lights lit"])
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
for hero in heroes:
    ref, who = hero_ref(hero)
    for clip, poses in (("Idle", idle), ("Walk", walk)):
        for i, pose in enumerate(poses):
            n = f"{hero}_{clip}_f{i}"
            jobs.append(dict(name=n, path=ACT + n + "_v1.png", transparent=True, refs=[ref], depends=None if i == 0 else f"{hero}_{clip}_f0", mode="single",
                             prompt="Use case: precise-object-edit. Using " + who + " as the exact design reference "
                                    f"(same character, outfit, colours, proportions, ink style), draw ONE {clip.lower()} animation frame: {pose}. "
                                    "Facing right, elevated front three-quarter game view, whole body, same scale in every frame, feet on the same "
                                    "baseline at 85% canvas height, 768x768 canvas, genuinely transparent RGBA, no ground, shadow, text or UI."))
for i, step in enumerate(["coin face-on", "coin turned 30 degrees", "coin turned 60 degrees", "coin edge-on (thin brass rim)", "coin turned 120 degrees showing the back", "coin back nearly face-on"]):
    n = f"CoinSpin_f{i}"
    jobs.append(dict(name=n, path=ART + n + "_v1.png", transparent=True, refs=["Assets/ArcaneBrigade/Art/CombatVFX/Allied/AllySpark_v1.png"], depends=None if i == 0 else "CoinSpin_f0",
                     mode="single", prompt=STYLE + f"Subject: a brass dream coin with a moon stamp spinning: {step}." + ROUND + ISOLATED))

out = ROOT / "Docs" / "Bizarre_Arsenal_Art_Worklist.json"
out.write_text(json.dumps(jobs, indent=1, ensure_ascii=False), encoding="utf-8")
print(len(jobs), "jobs ->", out)
