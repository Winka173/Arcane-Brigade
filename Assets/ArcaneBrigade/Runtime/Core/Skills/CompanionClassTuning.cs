namespace ArcaneBrigade
{
    // Design sections 0 and 1: authored roster classes determine bars and defenses; every house holds every class
    // (Docs/Bond_Steps_Proposal_2026-10-28.md section 6).
    public static class CompanionClassTuning
    {
        public const float ShooterBar = 6, StrikerBar = 7, SupportBar = 8, ControllerBar = 9, GuardBar = 10, SummonerBar = 12;
        public static bool TryGetRole(string id, out CompanionRole role)
        {
            switch (id)
            {
                case "TeapotPilgrim":
                case "HoodWisp":
                case "ShellPostman":
                case "ReelOwl":
                case "LensOwl":
                case "ShovelPenguin":
                case "RainHeron":
                case "KiteDragon":
                case "StormKitten":
                case "ShantySeaLion":
                case "MoonJelly":
                case "StarSock":
                case "DreamCompass":
                case "RainbowBell":
                case "SecretDreamLamb":
                case "ReturnCaptain":
                case "YoyoSquirrel":
                case "FirstSeed":
                case "JackInTheBox":
                case "CourtHerald":
                case "NewtonApple":
                case "SatelliteBee":
                case "BoneXylophone":
                case "TeaDragon":
                    role = CompanionRole.Shooter; return true;
                case "CandleSnail":
                case "KettleBat":
                case "AnchorSnail":
                case "CoilKettle":
                case "SundaeGolem":
                case "MushroomLibrarian":
                case "TinTick":
                case "IceCreamOwl":
                case "BubbleKoi":
                case "CrayonGremlin":
                case "VelvetEnchantress":
                case "PaperDevil":
                case "MonsterMarble":
                case "BagpipeOctopus":
                case "SinkholeWorm":
                case "PianoCrocodile":
                case "EndlessIndex":
                case "ScarecrowCrow":
                case "VoltEel":
                case "SailSquid":
                    role = CompanionRole.Controller; return true;
                case "MailboxImp":
                case "BookmarkGolem":
                case "SnowSpoon":
                case "CometKite":
                case "CoralTeacup":
                case "DiceCrab":
                case "SpindleWren":
                case "ButtonBeetle":
                case "StapleMole":
                case "CitrusToad":
                case "RadishJester":
                case "EclipseEgg":
                case "SecretPatchBear":
                case "PaperInventor":
                case "LittleSprout":
                case "NurseHen":
                case "PhoenixChick":
                case "DandelionPuff":
                case "PlasterPanda":
                case "Metronome":
                case "HarpSwan":
                case "ThimbleMoth":
                case "SpringDuck":
                case "WateringSnail":
                    role = CompanionRole.Support; return true;
                case "BreadCrab":
                case "FridgeWisp":
                case "SaffronMechanist":
                case "VelvetChampion":
                case "BalloonMallet":
                case "TeacupCricket":
                case "RibbonOtter":
                case "DoughForeman":
                case "VioletRemnant":
                case "GrandSprout":
                case "BristleBall":
                case "StitchedPangolin":
                case "TowerRook":
                case "LifebuoyHippo":
                case "BrassLion":
                case "QuiltWalrus":
                case "AnvilOx":
                case "BowlingSloth":
                case "LighthouseWhale":
                case "PillowOracle":
                case "FossilArmadillo":
                case "RaincoatFrog":
                case "SeedHedgehog":
                case "BrassAcorn":
                    role = CompanionRole.Guard; return true;
                case "PearMoth":
                case "MaskPepper":
                case "RainbowTailor":
                case "SecretPaperBird":
                case "BakerMouse":
                case "StaticFerret":
                case "JellySwordsman":
                case "SockCyclops":
                case "RibbonSpider":
                case "CarouselPony":
                case "LampGoat":
                case "TopBeetle":
                case "PumpkinReaper":
                case "CymbalMonkey":
                case "SaddleMoth":
                case "IronLocomotive":
                    role = CompanionRole.Striker; return true;
                case "BobaTadpole":
                case "PaperPlanePigeon":
                case "BottleGull":
                case "MirrorNewt":
                case "ClockworkPiper":
                case "LostCarousel":
                case "SpecimenSlime":
                case "PlanetHamster":
                case "ChoirCanary":
                case "PinnedBeetle":
                case "TotemPole":
                case "OrbitMoth":
                    role = CompanionRole.Summoner; return true;
                default: role = CompanionRole.Striker; return false;
            }
        }
        public static float Bar(CompanionRole role)
        {
            switch (role)
            {
                case CompanionRole.Shooter: return ShooterBar;
                case CompanionRole.Striker: return StrikerBar;
                case CompanionRole.Support: return SupportBar;
                case CompanionRole.Controller: return ControllerBar;
                case CompanionRole.Guard: return GuardBar;
                case CompanionRole.Summoner: return SummonerBar;
                default: return StrikerBar;
            }
        }
    }
}
