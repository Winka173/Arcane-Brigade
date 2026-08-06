using UnityEngine;
namespace ArcaneBrigade
{
    // Section 1b: class defenses, stars and the hero armour-item share are points.
    public static class FriendDefenseTuning
    {
        public const float ArmorPerStar = 2, HealthPerStar = .4f, ItemShare = .5f, ArmorBreakPerHit = 1;
        public const float GuardArmor = 12, StrikerArmor = 6, ControllerArmor = 5, SummonerArmor = 4, SupportArmor = 3, ShooterArmor = 2;
        public const float GuardHealth = 1.6f, StrikerHealth = 1.2f, ControllerHealth = 1, SummonerHealth = .9f, SupportHealth = .9f, ShooterHealth = .8f;
        public static void ClassStats(string id, out float armor, out float health)
        {
            switch(id)
            {
                case "TeapotPilgrim":
                case "HoodWisp":
                case "ShellPostman":
                case "ThimbleMoth":
                case "ReelOwl":
                case "SecretPaperBird":
                case "BakerMouse":
                case "TeaDragon":
                case "LensOwl":
                case "EndlessIndex":
                case "ShovelPenguin":
                case "BobaTadpole":
                case "RainHeron":
                case "PaperPlanePigeon":
                case "RaincoatFrog":
                case "LighthouseWhale":
                case "KiteDragon":
                case "StormKitten":
                case "VoltEel":
                case "ShantySeaLion":
                case "BottleGull":
                case "PinnedBeetle":
                case "MoonJelly":
                case "StarSock":
                case "DreamCompass":
                case "MirrorNewt":
                case "RainbowBell":
                case "SecretPatchBear":
                case "SecretDreamLamb":
                case "PaperInventor":
                case "ReturnCaptain":
                case "CarouselPony":
                case "TopBeetle":
                case "YoyoSquirrel":
                case "FirstSeed":
                case "JackInTheBox":
                case "LostCarousel":
                case "SpecimenSlime":
                case "CourtHerald":
                case "PhoenixChick":
                case "BoneXylophone":
                case "PlanetHamster":
                case "ChoirCanary":
                case "NewtonApple":
                case "SatelliteBee":
                    armor = ShooterArmor; health = ShooterHealth; return;
                case "CandleSnail":
                case "AnchorSnail":
                case "CoilKettle":
                case "IronLocomotive":
                case "SundaeGolem":
                case "FossilArmadillo":
                case "MushroomLibrarian":
                case "TinTick":
                case "IceCreamOwl":
                case "SeedHedgehog":
                case "OrbitMoth":
                case "CrayonGremlin":
                case "VelvetEnchantress":
                case "WateringSnail":
                case "BagpipeOctopus":
                case "SinkholeWorm":
                case "PianoCrocodile":
                    armor = ControllerArmor; health = ControllerHealth; return;
                case "MailboxImp":
                case "KettleBat":
                case "BookmarkGolem":
                case "SnowSpoon":
                case "CometKite":
                case "CoralTeacup":
                case "DiceCrab":
                case "MaskPepper":
                case "SpindleWren":
                case "PillowOracle":
                case "ButtonBeetle":
                case "StapleMole":
                case "CitrusToad":
                case "RadishJester":
                case "EclipseEgg":
                case "BubbleKoi":
                case "SaddleMoth":
                case "NurseHen":
                case "DandelionPuff":
                case "TotemPole":
                case "PlasterPanda":
                case "Metronome":
                case "HarpSwan":
                    armor = SupportArmor; health = SupportHealth; return;
                case "BreadCrab":
                case "FridgeWisp":
                case "SailSquid":
                case "SaffronMechanist":
                case "VelvetChampion":
                case "BalloonMallet":
                case "TeacupCricket":
                case "RibbonOtter":
                case "JellySwordsman":
                case "SpringDuck":
                case "DoughForeman":
                case "VioletRemnant":
                case "GrandSprout":
                case "BristleBall":
                case "PaperDevil":
                case "LittleSprout":
                case "MonsterMarble":
                case "StitchedPangolin":
                case "TowerRook":
                case "LifebuoyHippo":
                case "BrassLion":
                case "QuiltWalrus":
                case "AnvilOx":
                case "BowlingSloth":
                    armor = GuardArmor; health = GuardHealth; return;
                case "PearMoth":
                case "RainbowTailor":
                case "StaticFerret":
                case "BrassAcorn":
                case "SockCyclops":
                case "RibbonSpider":
                case "LampGoat":
                case "PumpkinReaper":
                case "ScarecrowCrow":
                case "CymbalMonkey":
                    armor = StrikerArmor; health = StrikerHealth; return;
                case "ClockworkPiper":
                    armor = SummonerArmor; health = SummonerHealth; return;
                default: armor = 0; health = 1; return;
            }
        }
        public static float Armor(string id, int stars)
        { ClassStats(id, out float armor, out float health); return armor + Mathf.Max(0, stars - 1) * ArmorPerStar; }
        public static float Health(string id, int stars)
        { ClassStats(id, out float armor, out float health); return health * (1 + Mathf.Max(0, stars - 1) * HealthPerStar); }
        public static float SharedItemPoints(float armorValue)
        { return Mathf.Floor(Mathf.Max(0, armorValue / RunModifierRules.ArmorPoint) * ItemShare); }
    }
}
