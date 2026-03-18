"""Content expansion (Docs/Content_Expansion_Proposal.md, version 3): the source of truth for every companion's two bonds and
rarity, every hero's two themes, and the bond list. Writes Tools/Content/expansion.json for ContentExpansionBuilder and prints
the pool report (each hero's fixed companions and the minor bonds that can activate in it)."""
import json, pathlib
from collections import Counter, defaultdict

ROOT = pathlib.Path(__file__).resolve().parents[2]
MAIN = ["hearth", "archive", "clock", "frost", "orchard", "astral", "tidal", "oddity"]

# companion: (main theme, minor bond, rarity, new?)  — rarity C U R M P (Prismatic)
C = {
    # hearth
    "TeapotPilgrim": ("hearth", "lantern-guild", "C"), "CandleSnail": ("hearth", "lantern-guild", "U"), "TeacupCricket": ("hearth", "lantern-guild", "U"),
    "BreadCrab": ("hearth", "gauntlet", "C"), "KettleBat": ("hearth", "gauntlet", "C"), "CoilKettle": ("hearth", "gear", "M"),
    "SaffronMechanist": ("hearth", "gear", "R"), "VelvetEnchantress": ("hearth", "regal", "U"),
    "BakerMouse": ("hearth", "sweet-tooth", "U", "new"), "TeaDragon": ("hearth", "morning", "M", "new"), "SundaeGolem": ("hearth", "sweet-tooth", "P", "new"),
    # archive
    "MailboxImp": ("archive", "hat", "C"), "BrassAcorn": ("archive", "regal", "C"), "BookmarkGolem": ("archive", "rune", "R"),
    "ThimbleMoth": ("archive", "hide", "R"), "SecretPatchBear": ("archive", "hide", "R"), "VelvetChampion": ("archive", "regal", "R"),
    "SpindleWren": ("archive", "paper-wings", "U"), "ReelOwl": ("archive", "rune", "M"), "SecretPaperBird": ("archive", "paper-wings", "R"),
    "LensOwl": ("archive", "rune", "R", "new"), "EndlessIndex": ("archive", "rune", "P", "new"),
    # clock
    "TinTick": ("clock", "gear", "C"), "SpringDuck": ("clock", "hare", "R"), "ButtonBeetle": ("clock", "regal", "C"),
    "StapleMole": ("clock", "gear", "C"), "RainbowBell": ("clock", "morning", "P"), "ClockworkPiper": ("clock", "morning", "M"),
    "GrandSprout": ("clock", "sprout", "U"), "DreamCompass": ("clock", "archer", "C"),
    "LampGoat": ("clock", "horn", "U", "new"), "TopBeetle": ("clock", "gear", "R", "new"),
    # frost
    "HoodWisp": ("frost", "root", "C"), "SnowSpoon": ("frost", "sweet-tooth", "C"), "IceCreamOwl": ("frost", "sweet-tooth", "U"),
    "FridgeWisp": ("frost", "shoe", "C"), "RainbowTailor": ("frost", "hide", "P"), "IronLocomotive": ("frost", "resolve", "M"),
    "MonsterMarble": ("frost", "mask", "U"), "ShovelPenguin": ("frost", "gauntlet", "R", "new"), "BobaTadpole": ("frost", "sweet-tooth", "C", "new"),
    "RainHeron": ("frost", "rain-script", "R", "new"),
    # orchard
    "MushroomLibrarian": ("orchard", "clover", "C"), "PearMoth": ("orchard", "morning", "U"), "CitrusToad": ("orchard", "hare", "U"),
    "RadishJester": ("orchard", "mask", "C"), "SeedHedgehog": ("orchard", "root", "C"), "PaperInventor": ("orchard", "gear", "M"),
    "PaperDevil": ("orchard", "horn", "U"), "BristleBall": ("orchard", "horn", "U"),
    "WateringSnail": ("orchard", "sprout", "U", "new"), "YoyoSquirrel": ("orchard", "hare", "R", "new"), "FirstSeed": ("orchard", "root", "P", "new"),
    # astral
    "PillowOracle": ("astral", "hat", "M"), "MoonJelly": ("astral", "archer", "R"), "StarSock": ("astral", "shoe", "U"),
    "CometKite": ("astral", "archer", "C"), "OrbitMoth": ("astral", "hare", "C"), "SecretDreamLamb": ("astral", "hide", "R"),
    "DoughForeman": ("astral", "hat", "R"), "LittleSprout": ("astral", "sprout", "U"), "EclipseEgg": ("astral", "morning", "P"),
    "PaperPlanePigeon": ("astral", "paper-wings", "C", "new"),
    # tidal
    "ShellPostman": ("tidal", "lantern-guild", "C"), "CoralTeacup": ("tidal", "clover", "U"), "BubbleKoi": ("tidal", "clover", "C"),
    "AnchorSnail": ("tidal", "knight", "U"), "SailSquid": ("tidal", "resolve", "U"), "ReturnCaptain": ("tidal", "knight", "M"),
    "BalloonMallet": ("tidal", "big-top", "U"), "RibbonOtter": ("tidal", "rain-script", "U"),
    "RaincoatFrog": ("tidal", "rain-script", "U", "new"), "LighthouseWhale": ("tidal", "lantern-guild", "M", "new"), "KiteDragon": ("tidal", "paper-wings", "P", "new"),
    # oddity
    "JellySwordsman": ("oddity", "knight", "C"), "DiceCrab": ("oddity", "clover", "C"), "MaskPepper": ("oddity", "mask", "C"),
    "MirrorNewt": ("oddity", "resolve", "M"), "RibbonSpider": ("oddity", "hide", "C"), "VioletRemnant": ("oddity", "resolve", "U"),
    "SockCyclops": ("oddity", "shoe", "C"), "CrayonGremlin": ("oddity", "mask", "C"), "CarouselPony": ("oddity", "big-top", "U"),
    "JackInTheBox": ("oddity", "big-top", "M", "new"), "LostCarousel": ("oddity", "big-top", "P", "new"),
}
HEROES = {
    "LanternKnight": ["hearth", "clock"], "Postmaster": ["archive", "tidal"], "DrowsySeer": ["astral", "frost"],
    "RootOracle": ["orchard", "astral"], "LoneJelly": ["oddity", "tidal"], "GlassDice": ["oddity", "hearth"],
    "TideSinger": ["tidal", "astral"], "CometRunner": ["astral", "clock"], "MirrorKeeper": ["frost", "oddity"],
    "TreasureAcorn": ["orchard", "archive"], "VerdantMatron": ["orchard", "hearth"], "CopperCauldron": ["hearth", "oddity"],
    "ChinaMarionette": ["frost", "oddity"], "SoapSkater": ["tidal", "frost"], "VelvetRegent": ["archive", "clock"],
    "ButtonCavalier": ["clock", "oddity"], "NightMaw": ["oddity", "astral"], "ThornTreasurer": ["orchard", "archive"],
    "CitrusMarshal": ["orchard", "hearth"], "IvorySovereign": ["archive", "frost"], "PetalCitadel": ["orchard", "astral"],
    "PatchworkHost": ["hearth", "oddity", "orchard"], "ArchiveCurator": ["archive", "clock"], "PennyBroker": ["archive", "hearth"],
    "StormFencer": ["clock", "astral"], "TinVirtuoso": ["clock", "oddity"], "MoonProwler": ["astral", "frost"],
    "SupperMortar": ["hearth", "tidal"], "LanternWeaver": ["astral", "archive"], "HourglassStag": ["clock", "tidal"],
    "RainArchivist": ["tidal", "archive"],
    "KiteAdmiral": ["astral", "tidal"], "SundaeDuchess": ["frost", "hearth"], "BonsaiClocksmith": ["orchard", "clock"],
    "JugglingJester": ["oddity", "orchard"], "BellDiver": ["tidal", "frost"],
}
NEW_HEROES = {"KiteAdmiral", "SundaeDuchess", "BonsaiClocksmith", "JugglingJester", "BellDiver"}
# minor bonds: threshold (all 2 now, Jade Nursery 3; the old 3/4 thresholds could never fill with one minor per companion)
MINOR_THRESHOLD = {"sprout": 3}
REMOVED = ["fruit", "ball"]
NEW_BONDS = {"paper-wings": "Paper Wings", "sweet-tooth": "Sweet Tooth", "big-top": "Big Top"}
RARITY = {"C": "Common", "U": "Uncommon", "R": "Rare", "M": "Mythic", "P": "Rainbow"}
EXCLUSIVE = {"GrandSprout", "LittleSprout"}  # Verdant Matron's own sprouts

def pool(hero, live_only=False):
    return [c for c, v in C.items() if (not live_only or len(v) < 4)
            and ((v[0] in HEROES[hero] and c not in EXCLUSIVE) or (c in EXCLUSIVE and hero == "VerdantMatron"))]

def report(live_only):
    problems = []
    minors_in_pools = Counter()
    for h in HEROES:
        p = pool(h, live_only)
        counts = Counter(C[c][1] for c in p)
        active = sorted(b for b, n in counts.items() if n >= MINOR_THRESHOLD.get(b, 2))
        for b in active: minors_in_pools[b] += 1
        rar = Counter(C[c][2] for c in p)
        line = f"{h:17} {'+'.join(HEROES[h]):22} companions {len(p):2}  M{rar['M']} P{rar['P']}  minors {len(active)}: {', '.join(active)}"
        print(line)
        if not live_only:
            if not (18 <= len(p) <= 34): problems.append(h + " pool size " + str(len(p)))
            if not (4 <= len(active)): problems.append(h + " active minors " + str(len(active)))
            if rar['M'] < 2 or rar['P'] < 2: problems.append(h + " rarity M/P " + str((rar['M'], rar['P'])))
    if not live_only:
        for b in set(v[1] for v in C.values()):
            # Jade Nursery is Verdant Matron's own bond
            if b != "sprout" and minors_in_pools[b] < 5: problems.append("minor " + b + " active in only " + str(minors_in_pools[b]) + " pools")
    return problems

def main():
    assert len(C) == 85, len(C)
    themes = Counter(v[0] for v in C.values()); minors = Counter(v[1] for v in C.values()); rar = Counter(v[2] for v in C.values())
    print("themes", dict(themes)); print("minors", len(minors), dict(sorted(minors.items(), key=lambda x: -x[1]))); print("rarity", dict(rar))
    for t in MAIN:
        r = Counter(C[c][2] for c in C if C[c][0] == t); print(" ", t, dict(r))
    print("hero theme use", Counter(t for v in HEROES.values() for t in v))
    print("\n== all 85 ==");  problems = report(False)
    print("\n== live now (66, new ones wait for art) =="); report(True)
    print("\nPROBLEMS:", problems or "none")
    out = dict(companions={c: dict(theme=v[0], minor=v[1], rarity=RARITY[v[2]], new=len(v) > 3) for c, v in C.items()},
               heroes=HEROES, newHeroes=sorted(NEW_HEROES), minorThreshold=MINOR_THRESHOLD, removedBonds=REMOVED, newBonds=NEW_BONDS)
    (ROOT / "Tools" / "Content" / "expansion.json").write_text(json.dumps(out, indent=1), encoding="utf-8")

if __name__ == "__main__":
    main()
