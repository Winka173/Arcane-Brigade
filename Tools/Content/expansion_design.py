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
    "TinTick": ("clock", "static-club", "C"), "SpringDuck": ("clock", "hare", "R"), "ButtonBeetle": ("clock", "regal", "C"),
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
    "CometKite": ("astral", "archer", "C"), "OrbitMoth": ("astral", "shoe", "C"), "SecretDreamLamb": ("astral", "hide", "R"),
    "DoughForeman": ("astral", "hat", "R"), "LittleSprout": ("astral", "sprout", "U"), "EclipseEgg": ("astral", "morning", "P"),
    "PaperPlanePigeon": ("astral", "paper-wings", "C", "new"),
    # tidal
    "ShellPostman": ("tidal", "sea-shanty", "C"), "CoralTeacup": ("tidal", "clover", "U"), "BubbleKoi": ("tidal", "clover", "C"),
    "AnchorSnail": ("tidal", "knight", "U"), "SailSquid": ("tidal", "resolve", "U"), "ReturnCaptain": ("tidal", "knight", "M"),
    "BalloonMallet": ("tidal", "big-top", "U"), "RibbonOtter": ("tidal", "rain-script", "U"),
    "RaincoatFrog": ("tidal", "rain-script", "U", "new"), "LighthouseWhale": ("tidal", "lantern-guild", "M", "new"), "KiteDragon": ("tidal", "paper-wings", "P", "new"),
    # oddity
    "JellySwordsman": ("oddity", "knight", "C"), "DiceCrab": ("oddity", "clover", "C"), "MaskPepper": ("oddity", "mask", "C"),
    "MirrorNewt": ("oddity", "resolve", "M"), "RibbonSpider": ("oddity", "hide", "C"), "VioletRemnant": ("oddity", "resolve", "U"),
    "SockCyclops": ("oddity", "shoe", "C"), "CrayonGremlin": ("oddity", "mask", "C"), "CarouselPony": ("oddity", "big-top", "U"),
    "JackInTheBox": ("oddity", "big-top", "M", "new"), "LostCarousel": ("oddity", "big-top", "P", "new"),
    # version 5 additions (second audit): Storm Fencer's Static Club, Tide Singer's Sea Shanty, Archive Curator's own Exhibit Hall
    "StaticFerret": ("clock", "static-club", "C", "new"), "StormKitten": ("astral", "static-club", "U", "new"), "VoltEel": ("tidal", "static-club", "R", "new"),
    "ShantySeaLion": ("tidal", "sea-shanty", "C", "new"), "BottleGull": ("hearth", "sea-shanty", "U", "new"),
    "PinnedBeetle": ("archive", "exhibit-hall", "C", "new"), "FossilArmadillo": ("frost", "exhibit-hall", "U", "new"), "SpecimenSlime": ("clock", "exhibit-hall", "R", "new"),
    "CourtHerald": ("archive", "hat", "C", "new"),
}
HEROES = {
    "LanternKnight": ["hearth", "clock"], "Postmaster": ["archive", "tidal"], "DrowsySeer": ["astral", "hearth"],
    "RootOracle": ["orchard", "frost"], "LoneJelly": ["oddity", "tidal"], "GlassDice": ["oddity", "orchard"],
    "TideSinger": ["tidal", "astral"], "CometRunner": ["astral", "clock"], "MirrorKeeper": ["frost", "oddity"],
    "TreasureAcorn": ["orchard", "archive"], "VerdantMatron": ["orchard", "hearth"], "CopperCauldron": ["hearth", "oddity"],
    "ChinaMarionette": ["frost", "oddity"], "SoapSkater": ["tidal", "frost"], "VelvetRegent": ["archive", "clock"],
    "ButtonCavalier": ["clock", "oddity"], "NightMaw": ["oddity", "astral"], "ThornTreasurer": ["orchard", "archive"],
    "CitrusMarshal": ["orchard", "hearth"], "IvorySovereign": ["archive", "frost"], "PetalCitadel": ["orchard", "astral"],
    "PatchworkHost": ["hearth", "oddity", "orchard"], "ArchiveCurator": ["clock", "frost"], "PennyBroker": ["archive", "hearth"],
    "StormFencer": ["clock", "astral"], "TinVirtuoso": ["clock", "oddity"], "MoonProwler": ["astral", "frost"],
    "SupperMortar": ["hearth", "tidal"], "LanternWeaver": ["astral", "archive"], "HourglassStag": ["clock", "tidal"],
    "RainArchivist": ["tidal", "orchard"],
    "KiteAdmiral": ["astral", "tidal"], "SundaeDuchess": ["frost", "hearth"], "BonsaiClocksmith": ["orchard", "clock"],
    "JugglingJester": ["oddity", "archive"], "BellDiver": ["tidal", "frost"],
}
NEW_HEROES = {"KiteAdmiral", "SundaeDuchess", "BonsaiClocksmith", "JugglingJester", "BellDiver"}
# minor bonds: threshold (all 2 now, Jade Nursery 3; the old 3/4 thresholds could never fill with one minor per companion)
MINOR_THRESHOLD = {"sprout": 3, "exhibit-hall": 2}
REMOVED = ["fruit", "ball"]
NEW_BONDS = {"paper-wings": "Paper Wings", "sweet-tooth": "Sweet Tooth", "big-top": "Big Top", "static-club": "Static Club", "sea-shanty": "Sea Shanty", "exhibit-hall": "Exhibit Hall"}
RARITY = {"C": "Common", "U": "Uncommon", "R": "Rare", "M": "Mythic", "P": "Rainbow"}
EXCLUSIVE = {"GrandSprout": "VerdantMatron", "LittleSprout": "VerdantMatron", "PinnedBeetle": "ArchiveCurator", "FossilArmadillo": "ArchiveCurator", "SpecimenSlime": "ArchiveCurator"}  # a hero's own exclusive friends

# items: rarity moves (Docs/Content_Expansion_Proposal.md §7.2) and one shop theme each (Common items are shared by every
# hero; Prismatic items carry two themes), so a hero's fixed pool holds ~80 of the 200 items
ITEM_RARITY = {**{i: "Common" for i in ["k10-attackrange", "k10-bulletpierce", "k10-bulletsize", "k10-collisiondamage", "k10-damagecorrection",
                                         "k10-enemycount", "k10-healcorrection", "k10-hearthealing", "k10-knockback", "k10-relicdiscount",
                                         "k10-income", "k10-enemyspeed", "fortune-egg", "velvet-first-aid", "OctoberRelic0", "OctoberRelic2"]},
               **{i: "Uncommon" for i in ["k10-bodysize", "k10-bulletspeed", "k10-bulletsplit", "k10-chestquality", "k10-collisionfrequency",
                                           "k10-dodge", "k10-enemyhealth", "k10-meleedamage", "k10-shoprarity", "k10-speedcorrection"]},
               "brass-avalanche": "Rare", "long-atlas": "Rare", "CrimsonSeal": "Mythic", "CrimsonSpring": "Mythic",
               # version 4: the weight-0 Secret relics are weaker than Commons; Hungry Ledger and Mute Clock already scale like Mythics
               **{i: "Uncommon" for i in ["SecretCrown", "SecretMirror", "SecretWoodHeart", "SecretLetter", "SecretDream", "SecretHour"]},
               "CurioHungryLedger": "Mythic", "CurioMuteClock": "Mythic"}
PRICE = {"Common": (15, 32), "Uncommon": (24, 46), "Rare": (30, 60), "Mythic": (48, 75), "Rainbow": (200, 200)}
WEIGHT = {"Common": 100, "Uncommon": 60, "Rare": 25, "Mythic": 8, "Rainbow": 0}

def items(current):
    load = Counter(); rload = Counter(); out = {}
    order = sorted(current, key=lambda i: (len(i["themes"]) or 99, i["id"]))  # the most constrained first
    for it in order:
        rarity = ITEM_RARITY.get(it["id"], it["rarity"])
        lo, hi = PRICE[rarity]
        entry = dict(rarity=rarity, price=min(hi, max(lo, it["price"])), weight=WEIGHT[rarity] if it["id"] in ITEM_RARITY and it["weight"] > 0 else it["weight"])
        if rarity == "Common": entry["themes"] = []
        else:
            want = 2 if rarity == "Rainbow" else 1
            choices = [t for t in it["themes"] if t in MAIN] or list(MAIN)
            picked = sorted(choices, key=lambda t: (rload[(rarity, t)], load[t], MAIN.index(t)))[:want]
            if rarity in ("Mythic", "Rainbow") and rload[(rarity, picked[0])] > min(rload[(rarity, t)] for t in MAIN) + 1:
                picked = sorted(MAIN, key=lambda t: (rload[(rarity, t)], load[t]))[:want]  # an even spread beats the old theme
            if len(picked) < want: picked += [t for t in sorted(MAIN, key=lambda t: load[t]) if t not in picked][:want - len(picked)]
            for t in picked: load[t] += 1; rload[(rarity, t)] += 1
            entry["themes"] = picked + ["fam-" + f for f in families(it)]
        out[it["id"]] = entry
    return out, load

# version 4 (Docs/Content_Expansion_v4.md): each hero's signature minor bond (its whole membership joins the pool; at most two
# heroes per bond), item family, and starting friend
SIGNATURE = {"LanternKnight": "lantern-guild", "LanternWeaver": "lantern-guild", "Postmaster": "paper-wings", "KiteAdmiral": "paper-wings",
             "DrowsySeer": "hat", "IvorySovereign": "hat", "RootOracle": "root", "LoneJelly": "knight", "ButtonCavalier": "knight",
             "GlassDice": "clover", "TreasureAcorn": "clover", "TinVirtuoso": "morning", "HourglassStag": "morning", "CitrusMarshal": "hare",
             "TideSinger": "sea-shanty", "CometRunner": "shoe", "SoapSkater": "shoe", "MirrorKeeper": "resolve", "CopperCauldron": "resolve",
             "VerdantMatron": "sprout", "ChinaMarionette": "hide", "VelvetRegent": "regal", "NightMaw": "gauntlet", "ThornTreasurer": "horn",
             "PetalCitadel": "archer", "PatchworkHost": "big-top", "JugglingJester": "big-top", "ArchiveCurator": "rune", "StormFencer": "static-club",
             "PennyBroker": "gear", "BonsaiClocksmith": "gear", "MoonProwler": "mask", "SupperMortar": "sweet-tooth", "SundaeDuchess": "sweet-tooth",
             "RainArchivist": "rain-script", "BellDiver": "rain-script"}
FAMILY = {"KiteAdmiral": "ranged", "PetalCitadel": "ranged", "LanternWeaver": "ranged", "Postmaster": "marks", "BellDiver": "marks",
          "DrowsySeer": "shop", "IvorySovereign": "shop", "LoneJelly": "merge", "BonsaiClocksmith": "merge", "VerdantMatron": "merge",
          "ButtonCavalier": "melee", "MoonProwler": "melee", "GlassDice": "luck", "TreasureAcorn": "chest", "RainArchivist": "chest",
          "TideSinger": "drink", "SoapSkater": "freeze", "SundaeDuchess": "freeze", "CometRunner": "speed", "CitrusMarshal": "collision",
          "CopperCauldron": "collision", "RootOracle": "survival", "MirrorKeeper": "survival", "ThornTreasurer": "gold", "PennyBroker": "gold",
          "ChinaMarionette": "gold", "NightMaw": "food", "SupperMortar": "food", "LanternKnight": "team", "VelvetRegent": "team",
          "PatchworkHost": "team", "StormFencer": "crit", "TinVirtuoso": "crit", "JugglingJester": "crit", "HourglassStag": "delay",
          "ArchiveCurator": "kills"}
STARTER_CHANGES = {"MirrorKeeper": "VioletRemnant", "CopperCauldron": "TeacupCricket", "IvorySovereign": "IceCreamOwl", "MoonProwler": "StarSock", "LanternWeaver": "ThimbleMoth", "HourglassStag": "DreamCompass",
                   "RootOracle": "MushroomLibrarian", "DrowsySeer": "CandleSnail", "ArchiveCurator": "FridgeWisp"}
NEW_STARTERS = {"KiteAdmiral": "PaperPlanePigeon", "SundaeDuchess": "BakerMouse", "BonsaiClocksmith": "TopBeetle", "JugglingJester": "LensOwl", "BellDiver": "RaincoatFrog"}

# item families (version 4): an item's family follows its category (food, drink) or its first positive effect
EFFECTS = ["Damage", "AttackSpeed", "Health", "MoveSpeed", "Pickup", "Roam", "Interceptor", "Heal", "Crit", "Projectiles", "Interest", "Armor",
           "ChampionDamage", "CompanionDamage", "CompanionAttackSpeed", "Discount", "Luck", "TreasureDetection", "ScoreMultiplier", "CollisionDamage",
           "MeleeDamage", "RangedDamage", "AttackRange", "Dodge", "Income", "BulletPierce", "BulletSplit", "BulletBounce", "BulletSize", "BulletSpeed",
           "BulletDistance", "Knockback", "BodySize", "CompanionSize", "EnemyCount", "EnemyHealth", "EnemySpeed", "HeartHealing", "ChestQuality",
           "RecruitDiscount", "RelicDiscount", "ShopRarity", "HealthCorrection", "DamageCorrection", "SpeedCorrection", "ArmorCorrection",
           "HealCorrection", "CollisionFrequency", "ChestReach"]
EFFECT_FAMILY = {**{e: "gold" for e in ["Income", "Interest", "Discount", "RelicDiscount", "RecruitDiscount", "Pickup", "ScoreMultiplier"]},
                 **{e: "chest" for e in ["Luck", "TreasureDetection", "ChestQuality", "ChestReach"]},
                 **{e: "ranged" for e in ["RangedDamage", "AttackRange", "Projectiles", "BulletPierce", "BulletSplit", "BulletBounce", "BulletSize", "BulletSpeed", "BulletDistance"]},
                 **{e: "melee" for e in ["MeleeDamage", "Knockback"]},
                 **{e: "collision" for e in ["CollisionDamage", "CollisionFrequency", "BodySize", "CompanionSize"]},
                 **{e: "crit" for e in ["Crit", "DamageCorrection", "ChampionDamage"]},
                 **{e: "team" for e in ["CompanionDamage", "CompanionAttackSpeed", "Interceptor", "Damage", "AttackSpeed"]},
                 **{e: "survival" for e in ["Health", "Heal", "Armor", "HeartHealing", "Dodge", "HealthCorrection", "ArmorCorrection", "HealCorrection"]},
                 **{e: "speed" for e in ["MoveSpeed", "Roam", "SpeedCorrection", "EnemySpeed"]},
                 **{e: "shop" for e in ["ShopRarity", "EnemyCount", "EnemyHealth"]}}

MECHANIC_FAMILY = {"CrowdedTape": "team", "CoinMending": "gold", "CompoundInterest": "gold", "RetortVolley": "ranged", "ExposedRecovery": "survival",
    "TreasureLease": "chest", "PrismEcho": "ranged", "RerollFeather": "shop", "ChestLightning": "chest", "DrinkVitality": "drink", "DrinkTempo": "drink",
    "PanicMagnet": "gold", "ChestLuck": "chest", "ChaosIncome": "luck", "HoardPower": "gold", "HalfHealthSeed": "survival", "StillRecovery": "survival",
    "CoinLightning": "gold", "TurnDiscount": "shop", "CloseFollowers": "team", "WaveGrowth": "kills", "MeleeToCollision": "collision",
    "SlowToArmor": "freeze", "DodgeMending": "survival", "DamageToIncome": "gold", "ChestOnlyRecovery": "chest", "SorrowPulse": "delay",
    "BleedingAlarm": "delay", "CollisionOnly": "collision", "SoloOverdrive": "melee", "FreshLotus": "survival", "EggGift": "shop",
    "CompanionGift": "merge", "FragilePower": "crit", "StatFloor": "team", "LuckToHealth": "luck", "GambleRecovery": "luck", "FoodGrowth": "food",
    "FreeFollowers": "merge", "IncomePoison": "delay", "HurtGrowth": "survival", "StillPower": "survival", "StaticFollowers": "team",
    "VoidPulse": "delay", "SixWeapons": "melee"}
MECHANICS = ["None", "CrowdedTape", "CoinMending", "CompoundInterest", "RetortVolley", "ExposedRecovery", "TreasureLease", "PrismEcho", "RerollFeather",
    "ChestLightning", "DrinkVitality", "DrinkTempo", "PanicMagnet", "ChestLuck", "ChaosIncome", "HoardPower", "HalfHealthSeed", "StillRecovery",
    "CoinLightning", "TurnDiscount", "CloseFollowers", "WaveGrowth", "MeleeToCollision", "SlowToArmor", "DodgeMending", "DamageToIncome",
    "ChestOnlyRecovery", "SorrowPulse", "BleedingAlarm", "CollisionOnly", "SoloOverdrive", "FreshLotus", "EggGift", "CompanionGift", "FragilePower",
    "StatFloor", "LuckToHealth", "GambleRecovery", "FoodGrowth", "FreeFollowers", "IncomePoison", "HurtGrowth", "StillPower", "StaticFollowers",
    "VoidPulse", "SixWeapons"]
EFFECT_FAMILY.update({"Luck": "luck", "ShopRarity": "shop", "Discount": "shop", "RelicDiscount": "shop", "RecruitDiscount": "shop",
                      "Interceptor": "survival", "Knockback": "melee"})
PENALTY = {"EnemySpeed", "EnemyCount", "EnemyHealth", "ScoreMultiplier"}

FAMILY_OVERRIDE = {  # third audit (Docs/Content_Expansion_v5.md §8): families decided by what the item does
    "CurioGlassLotus": ["crit"], "CurioLastHopeMotor": ["crit"], "CurioWanderLetter": ["crit"], "CurioMuteClock": ["speed"],
    "long-atlas": ["team"], "reflective-ink": ["marks"], "carbon-ribbon": ["marks"], "thermal-ink": ["marks"], "office-lens": ["marks", "crit"],
    "chest-thunder": ["chest"], "brass-avalanche": ["kills"], "CurioHungryLedger": ["kills"], "CurioBurdenCollar": ["collision"],
    "RainbowHeart": ["merge"], "CurioGambleSandwich": ["food", "luck"], "CurioFortunePear": ["food", "luck"], "CurioSleepyMist": ["drink", "freeze"],
    "CurioCrookedEight": ["luck"], "CurioTornAlmanac": ["luck"], "CurioPocketSun": ["drink", "luck"], "CurioPanicBell": ["luck"], "CurioManyDoorKey": ["luck", "chest"],
    "CurioVenomPass": ["delay"], "CurioSorrowLantern": ["delay"], "CurioVoidDoubloon": ["delay"], "CurioWishWick": ["merge"],
    "RainbowThread": ["melee"], "CurioStoneCrust": ["food", "melee"], "K26Defense3": ["speed"], "CurioCourageJuice": ["drink", "speed", "melee"],
    "K26Defense2": ["speed"],
}

def families(item):
    if item["id"] in FAMILY_OVERRIDE: return FAMILY_OVERRIDE[item["id"]]
    out = []
    if item["category"] == 2: out.append("food")
    if item["category"] == 1: out.append("drink")
    mech = int(item["mechanic"])
    if mech > 0 and MECHANICS[mech] in MECHANIC_FAMILY: out.append(MECHANIC_FAMILY[MECHANICS[mech]])
    for name, value in item.get("mods", []):
        if name == "EnemySpeed" and value < 0: out.append("freeze")
        elif value > 0 and name not in PENALTY and name in EFFECT_FAMILY: out.append(EFFECT_FAMILY[name])
    out = list(dict.fromkeys(out))[:3]
    return out or ["team"]

def family(item):
    if item["category"] == 2: return "food"  # every food feeds the food heroes (FoodPotency), whatever else it does
    if item["category"] == 1: return "drink"
    mech = int(item["mechanic"])
    if mech > 0 and MECHANICS[mech] in MECHANIC_FAMILY: return MECHANIC_FAMILY[MECHANICS[mech]]
    for name, value in item.get("mods", []):
        if name == "EnemySpeed" and value < 0: return "freeze"
        if value > 0 and name not in PENALTY and name in EFFECT_FAMILY: return EFFECT_FAMILY[name]
    return "team"

# bond effects (version 4 §3): one effect per bond, only two hero-survival bonds (Hearth, Rootkin); value per level
BOND_EFFECTS = {"hearth": ("Heal", .15), "archive": ("Damage", .08), "clock": ("AttackSpeed", .07), "frost": ("EnemySpeed", -.05),
                "orchard": ("Luck", .04), "astral": ("BulletSize", .08), "tidal": ("BulletBounce", .08), "oddity": ("Crit", .05),
                "hare": ("Knockback", .15), "archer": ("RangedDamage", 3), "root": ("Health", 8), "gauntlet": ("MeleeDamage", 4),
                "regal": ("CompanionDamage", .08), "clover": ("ShopRarity", .08), "gear": ("Income", 4), "hat": ("Interest", 2),
                "horn": ("BulletPierce", 1), "sprout": ("Discount", .06), "knight": ("CollisionDamage", 5), "morning": ("ChampionDamage", .1),
                "rune": ("BulletSplit", 1), "mask": ("ChestQuality", .1), "hide": ("Pickup", .4), "resolve": ("DamageCorrection", .06),
                "shoe": ("MoveSpeed", .08), "lantern-guild": ("AttackRange", .1), "rain-script": ("BulletSpeed", .1),
                "paper-wings": ("BulletDistance", .12), "sweet-tooth": ("CompanionAttackSpeed", .07), "big-top": ("RecruitDiscount", .08),
                "static-club": ("ChainChance", .04), "sea-shanty": ("RelicDiscount", .06), "exhibit-hall": ("RecordedDamage", .1)}

NEW_ITEMS = {  # Docs/Content_Expansion_v5.md §6b (final): id -> (rarity, themes, families)
    "TallyChalk": ("Common", [], []), "CoinSash": ("Common", [], []), "FishCracker": ("Common", [], []),
    "PostageStamp": ("Uncommon", ["archive"], ["marks"]), "FrostSalt": ("Uncommon", ["frost"], ["freeze"]),
    "RefundSlip": ("Uncommon", ["hearth"], ["shop"]), "WhetstoneWafer": ("Uncommon", ["oddity"], ["food", "melee"]),
    "LateFee": ("Uncommon", ["clock"], ["delay"]),
    "HagglersAbacus": ("Rare", ["archive"], ["shop"]), "SlipstreamScarf": ("Rare", ["astral"], ["speed"]), "BrainFreeze": ("Rare", ["frost"], ["freeze"]),
    "WindChime": ("Rare", ["clock"], ["crit"]), "SpecimenPin": ("Rare", ["archive"], ["kills"]), "StarPolish": ("Rare", ["orchard"], ["merge"]),
    "KiteTailRibbon": ("Mythic", ["astral"], ["ranged"]), "MeltingSundae": ("Mythic", ["frost"], ["freeze"]), "CuckooPendulum": ("Mythic", ["clock"], ["crit"]),
    "DivingBellLantern": ("Mythic", ["tidal"], ["marks"]), "TwinTicket": ("Mythic", ["archive"], ["merge"]), "TreasureMimic": ("Mythic", ["orchard"], ["chest"]),
    "DuelistsRibbon": ("Mythic", ["oddity"], ["melee"]), "FrostLedger": ("Mythic", ["frost"], ["freeze", "kills"]), "UndertowAnchor": ("Mythic", ["tidal"], ["team"]),
    "FunhouseMirror": ("Mythic", ["oddity"], ["team"]), "SpliceKit": ("Mythic", ["hearth"], ["merge"]),
    "TheEndlessEncore": ("Rainbow", ["oddity", "tidal"], ["team"]), "MoonClockKey": ("Rainbow", ["astral", "clock"], ["team"]),
    "MirrorOfTwins": ("Rainbow", ["hearth", "oddity"], ["team"]), "StorybookEnding": ("Rainbow", ["orchard", "tidal"], ["team"]),
    "DreamLoom": ("Rainbow", ["frost", "archive"], ["team"]),
}
STORY = {"TeapotPilgrim", "HoodWisp", "ShellPostman", "PaperDevil"}
# rival-bond rule (third audit): on a shared theme pair, each hero's pool drops the partner's signature members of rarity C/U/R
RIVAL_EXEMPT = set()

def rivals(hero):
    out = []
    if hero in RIVAL_EXEMPT: return out
    for other, themes in HEROES.items():
        if other != hero and sorted(themes) == sorted(HEROES[hero]) and SIGNATURE[other] != SIGNATURE[hero]: out.append(SIGNATURE[other])
    return out

# locked weapons (final audit): every Mythic and Prismatic friend always carries its own weapon, which leaves everyone else's roll
LOCKED_WEAPON = {"PillowOracle": "FortuneGlobe", "ReelOwl": "DreamRacket", "ClockworkPiper": "WindupWrench", "CoilKettle": "CatcherMitt",
                 "IronLocomotive": "SnowGlobeGrenade", "PaperInventor": "GrabberClaw", "ReturnCaptain": "PlushShark", "MirrorNewt": "KunaiFan",
                 "RainbowBell": "BuzzerBlock", "RainbowTailor": "TopiaryShears", "EclipseEgg": "SaucerBlaster",
                 "TeaDragon": "HearthBrick", "LighthouseWhale": "PaperLanternBomb", "JackInTheBox": "PinataBomb", "KiteDragon": "TwinMoonStaff",
                 "SundaeGolem": "StarKnuckle", "LostCarousel": "CherryFuseBomb", "EndlessIndex": "PaperShuriken", "FirstSeed": "AcornCluster"}

def pool(hero, live_only=False):
    sig = SIGNATURE.get(hero); rival = rivals(hero)
    return [c for c, v in C.items() if (not live_only or len(v) < 4)
            and (((v[0] in HEROES[hero] or v[1] == sig) and c not in EXCLUSIVE) or EXCLUSIVE.get(c) == hero)
            and not (v[1] in rival and v[1] != sig and v[2] in "CUR")]

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
            if not (18 <= len(p) <= (36 if h == "PatchworkHost" else 28)): problems.append(h + " pool size " + str(len(p)))
            if not (4 <= len(active)): problems.append(h + " active minors " + str(len(active)))
            if rar['M'] < 2 or rar['P'] < 2: problems.append(h + " rarity M/P " + str((rar['M'], rar['P'])))
    if not live_only:
        # version 5: heroes on the same theme pair differ by at least 3 companions and by item family; families hold at most 3 heroes
        for fam, n in Counter(FAMILY.values()).items():
            if n > 3: problems.append("family " + fam + " has " + str(n) + " heroes")
        if Counter(SIGNATURE.values()).most_common(1)[0][1] > 2: problems.append("a signature bond serves more than two heroes")
        hs = list(HEROES)
        for i, a in enumerate(hs):
            for b in hs[i + 1:]:
                if sorted(HEROES[a]) == sorted(HEROES[b]):
                    diff = len(set(pool(a)) ^ set(pool(b)))
                    if diff < 6 or FAMILY[a] == FAMILY[b]: problems.append(a + "/" + b + " share a pair: " + str(diff) + " companions apart, families " + FAMILY[a] + "/" + FAMILY[b])
        for b in set(v[1] for v in C.values()):
            # Jade Nursery is Verdant Matron's own bond
            if b not in ("sprout", "exhibit-hall") and minors_in_pools[b] < 5: problems.append("minor " + b + " active in only " + str(minors_in_pools[b]) + " pools")
    return problems

def main():
    assert len(C) == 94, len(C)
    themes = Counter(v[0] for v in C.values()); minors = Counter(v[1] for v in C.values()); rar = Counter(v[2] for v in C.values())
    print("themes", dict(themes)); print("minors", len(minors), dict(sorted(minors.items(), key=lambda x: -x[1]))); print("rarity", dict(rar))
    for t in MAIN:
        r = Counter(C[c][2] for c in C if C[c][0] == t); print(" ", t, dict(r))
    print("hero theme use", Counter(t for v in HEROES.values() for t in v))
    print("\n== all 85 ==");  problems = report(False)
    print("\n== live now (66, new ones wait for art) =="); report(True)
    current = json.loads((ROOT / "Tools" / "Content" / "catalog_before.json").read_text(encoding="utf-8"))
    item_plan, load = items(current["items"])
    for i, (r, th, fams) in NEW_ITEMS.items():
        family_only = r not in ("Common", "Rainbow") and fams and fams != ["team"]
        item_plan[i] = dict(rarity=r, price=0, weight=WEIGHT[r], themes=([] if family_only else th) + ["fam-" + f for f in fams], new=True)
    mythic = [x for x in C if C[x][2] in "MP"]
    if sorted(mythic) != sorted(LOCKED_WEAPON) or len(set(LOCKED_WEAPON.values())) != len(LOCKED_WEAPON): problems.append("locked weapons: one each, all different")
    assert len(item_plan) == 200, len(item_plan)
    fam_count = Counter(t for v in item_plan.values() if v["rarity"] != "Common" for t in v["themes"] if t.startswith("fam-"))
    print("non-Common items per family", dict(sorted(fam_count.items(), key=lambda x: -x[1])))
    for f in set(FAMILY.values()):
        need = 3 if list(FAMILY.values()).count(f) == 1 else 4
        if fam_count["fam-" + f] < need: problems.append("family " + f + " has only " + str(fam_count["fam-" + f]) + " items")
    hl = (ROOT / "Assets" / "ArcaneBrigade" / "Runtime" / "Data" / "HeroLoadout.cs").read_text(encoding="utf-8")
    import re
    starters = dict(re.findall(r'\["(\w+)"\] = new Entry\("\w+", (?:null|"\w+"), "(\w+)", "\w+"\)', hl)); starters.update(STARTER_CHANGES); starters.update(NEW_STARTERS)
    info = {c["id"]: c for c in current["companions"]}
    for h, f in starters.items():
        if f in STORY or (f in info and (info[f]["special"] or info[f]["gift"])) or f not in pool(h): problems.append("starter " + h + " " + f)
    if len(set(starters.values())) != len(starters): problems.append("two heroes share a starter")
    for h in HEROES:
        prism = sum(1 for v in item_plan.values() if v["rarity"] == "Rainbow" and set(v["themes"]) & set(HEROES[h]))
        if prism < 2: problems.append(h + " sees only " + str(prism) + " Prismatic items")
    print("item rarity", Counter(v["rarity"] for v in item_plan.values()), "themed per theme", dict(load))
    for h in HEROES:
        n = sum(1 for v in item_plan.values() if not v["themes"] or set(v["themes"]) & set(HEROES[h] + ["fam-" + FAMILY[h]]))
        if not (60 <= n <= (130 if h == "PatchworkHost" else 120)): problems.append(h + " item pool " + str(n))
    sizes = {h: sum(1 for v in item_plan.values() if not v["themes"] or set(v["themes"]) & set(HEROES[h] + ["fam-" + FAMILY[h]])) for h in HEROES}
    print("item pool sizes", min(sizes.values()), max(sizes.values()), "families", Counter(family(i) for i in current["items"] if item_plan[i["id"]]["rarity"] != "Common"))
    # flat lists for Unity's JsonUtility (ContentExpansionBuilder)
    out_lists = dict(
        rivalList=[dict(id=h, bonds=rivals(h)) for h in HEROES],
        lockedWeapons=[dict(id=c, weapon=w) for c, w in LOCKED_WEAPON.items()],
        companionList=[dict(id=c, theme=v[0], minor=v[1], rarity=RARITY[v[2]], isNew=len(v) > 3) for c, v in C.items()],
        heroList=[dict(id=h, themes=t + [SIGNATURE[h], "fam-" + FAMILY[h]] + ["rival-" + b for b in rivals(h)], isNew=h in NEW_HEROES,
                       starter=STARTER_CHANGES.get(h, NEW_STARTERS.get(h, ""))) for h, t in HEROES.items()],
        itemList=[dict(id=i, **{k: w for k, w in v.items() if k != "new"}, isNew=v.get("new", False)) for i, v in item_plan.items()],
        minorThresholds=[dict(id=b, threshold=MINOR_THRESHOLD.get(b, 2)) for b in sorted(set(v[1] for v in C.values()))],
        removedBonds=REMOVED, newBondList=[dict(id=b, name=n) for b, n in NEW_BONDS.items()],
        bondEffects=[dict(id=b, effect=e, value=v) for b, (e, v) in BOND_EFFECTS.items()])
    (ROOT / "Tools" / "Content" / "expansion_lists.json").write_text(json.dumps(out_lists, indent=1), encoding="utf-8")
    assert len(BOND_EFFECTS) == 33 and len(set(e for e, v in BOND_EFFECTS.values())) == 33, "every bond its own effect"
    print("PROBLEMS:", problems or "none")
    out = dict(items=item_plan, companions={c: dict(theme=v[0], minor=v[1], rarity=RARITY[v[2]], new=len(v) > 3) for c, v in C.items()},
               heroes=HEROES, newHeroes=sorted(NEW_HEROES), minorThreshold=MINOR_THRESHOLD, removedBonds=REMOVED, newBonds=NEW_BONDS)
    (ROOT / "Tools" / "Content" / "expansion.json").write_text(json.dumps(out, indent=1), encoding="utf-8")

if __name__ == "__main__":
    main()
