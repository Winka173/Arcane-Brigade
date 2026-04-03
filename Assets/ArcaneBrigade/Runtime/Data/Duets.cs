using UnityEngine;

namespace ArcaneBrigade
{
    public enum DuetEffect { Damage, Speed, Reach, Knockback }
    public sealed class Duet { public string A, B, Family, Line; public DuetEffect Effect; }
    /// <summary>Pair combos (play-test 20/10/2026, item 26; list in Docs/Playtest_Review_2026-10-20.md): two named friends
    /// fielded together both gain one bonus. Offence and utility only (friends are immortal: no healing, armor or dodge).</summary>
    public static class Duets
    {
        public const float DamageBonus = 1.15f, SpeedBonus = 1.12f, ReachBonus = 1.15f, KnockbackBonus = 1.25f;
        public static readonly Duet[] All =
        {
            new Duet { A = "PinnedBeetle", B = "MailboxImp", Family = "archive", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "MailboxImp", B = "ThimbleMoth", Family = "archive", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "ThimbleMoth", B = "SpindleWren", Family = "archive", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "SpindleWren", B = "BrassAcorn", Family = "archive", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "BrassAcorn", B = "BookmarkGolem", Family = "archive", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "BookmarkGolem", B = "CourtHerald", Family = "archive", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "CourtHerald", B = "SecretPatchBear", Family = "archive", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "SecretPatchBear", B = "SecretPaperBird", Family = "archive", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "SecretPaperBird", B = "VelvetChampion", Family = "archive", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "VelvetChampion", B = "ReelOwl", Family = "archive", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "ReelOwl", B = "LensOwl", Family = "archive", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "LensOwl", B = "EndlessIndex", Family = "archive", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "EndlessIndex", B = "PinnedBeetle", Family = "archive", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "MoonJelly", B = "PillowOracle", Family = "astral", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "PillowOracle", B = "SecretDreamLamb", Family = "astral", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "SecretDreamLamb", B = "EclipseEgg", Family = "astral", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "EclipseEgg", B = "PaperPlanePigeon", Family = "astral", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "PaperPlanePigeon", B = "StarSock", Family = "astral", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "StarSock", B = "LittleSprout", Family = "astral", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "LittleSprout", B = "StormKitten", Family = "astral", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "StormKitten", B = "CometKite", Family = "astral", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "CometKite", B = "DoughForeman", Family = "astral", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "DoughForeman", B = "OrbitMoth", Family = "astral", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "OrbitMoth", B = "MoonJelly", Family = "astral", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "DreamCompass", B = "SpecimenSlime", Family = "clock", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "SpecimenSlime", B = "StapleMole", Family = "clock", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "StapleMole", B = "SpringDuck", Family = "clock", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "SpringDuck", B = "LampGoat", Family = "clock", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "LampGoat", B = "RainbowBell", Family = "clock", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "RainbowBell", B = "ButtonBeetle", Family = "clock", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "ButtonBeetle", B = "GrandSprout", Family = "clock", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "GrandSprout", B = "TinTick", Family = "clock", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "TinTick", B = "TopBeetle", Family = "clock", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "TopBeetle", B = "ClockworkPiper", Family = "clock", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "ClockworkPiper", B = "StaticFerret", Family = "clock", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "StaticFerret", B = "DreamCompass", Family = "clock", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "FossilArmadillo", B = "ShovelPenguin", Family = "frost", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "ShovelPenguin", B = "RainbowTailor", Family = "frost", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "RainbowTailor", B = "MonsterMarble", Family = "frost", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "MonsterMarble", B = "RainHeron", Family = "frost", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "RainHeron", B = "IronLocomotive", Family = "frost", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "IronLocomotive", B = "HoodWisp", Family = "frost", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "HoodWisp", B = "FridgeWisp", Family = "frost", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "FridgeWisp", B = "SnowSpoon", Family = "frost", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "SnowSpoon", B = "IceCreamOwl", Family = "frost", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "IceCreamOwl", B = "BobaTadpole", Family = "frost", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "BobaTadpole", B = "FossilArmadillo", Family = "frost", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "BreadCrab", B = "CoilKettle", Family = "hearth", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "CoilKettle", B = "TeapotPilgrim", Family = "hearth", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "TeapotPilgrim", B = "TeaDragon", Family = "hearth", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "TeaDragon", B = "VelvetEnchantress", Family = "hearth", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "VelvetEnchantress", B = "BottleGull", Family = "hearth", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "BottleGull", B = "BakerMouse", Family = "hearth", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "BakerMouse", B = "KettleBat", Family = "hearth", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "KettleBat", B = "SaffronMechanist", Family = "hearth", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "SaffronMechanist", B = "CandleSnail", Family = "hearth", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "CandleSnail", B = "SundaeGolem", Family = "hearth", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "SundaeGolem", B = "TeacupCricket", Family = "hearth", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "TeacupCricket", B = "BreadCrab", Family = "hearth", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "CarouselPony", B = "DiceCrab", Family = "oddity", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "DiceCrab", B = "RibbonSpider", Family = "oddity", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "RibbonSpider", B = "JellySwordsman", Family = "oddity", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "JellySwordsman", B = "MaskPepper", Family = "oddity", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "MaskPepper", B = "MirrorNewt", Family = "oddity", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "MirrorNewt", B = "SockCyclops", Family = "oddity", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "SockCyclops", B = "JackInTheBox", Family = "oddity", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "JackInTheBox", B = "CrayonGremlin", Family = "oddity", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "CrayonGremlin", B = "VioletRemnant", Family = "oddity", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "VioletRemnant", B = "LostCarousel", Family = "oddity", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "LostCarousel", B = "CarouselPony", Family = "oddity", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "MushroomLibrarian", B = "PaperInventor", Family = "orchard", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "PaperInventor", B = "CitrusToad", Family = "orchard", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "CitrusToad", B = "PaperDevil", Family = "orchard", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "PaperDevil", B = "RadishJester", Family = "orchard", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "RadishJester", B = "PearMoth", Family = "orchard", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "PearMoth", B = "SeedHedgehog", Family = "orchard", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "SeedHedgehog", B = "WateringSnail", Family = "orchard", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "WateringSnail", B = "YoyoSquirrel", Family = "orchard", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "YoyoSquirrel", B = "BristleBall", Family = "orchard", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "BristleBall", B = "FirstSeed", Family = "orchard", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "FirstSeed", B = "MushroomLibrarian", Family = "orchard", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "BalloonMallet", B = "CoralTeacup", Family = "tidal", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "CoralTeacup", B = "AnchorSnail", Family = "tidal", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "AnchorSnail", B = "LighthouseWhale", Family = "tidal", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "LighthouseWhale", B = "KiteDragon", Family = "tidal", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "KiteDragon", B = "RibbonOtter", Family = "tidal", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "RibbonOtter", B = "SailSquid", Family = "tidal", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "SailSquid", B = "ShellPostman", Family = "tidal", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "ShellPostman", B = "VoltEel", Family = "tidal", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
            new Duet { A = "VoltEel", B = "BubbleKoi", Family = "tidal", Effect = DuetEffect.Speed, Line = "both attack 12% faster" },
            new Duet { A = "BubbleKoi", B = "ReturnCaptain", Family = "tidal", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "ReturnCaptain", B = "RaincoatFrog", Family = "tidal", Effect = DuetEffect.Damage, Line = "both hit 15% harder" },
            new Duet { A = "RaincoatFrog", B = "ShantySeaLion", Family = "tidal", Effect = DuetEffect.Knockback, Line = "both push 25% harder" },
            new Duet { A = "ShantySeaLion", B = "BalloonMallet", Family = "tidal", Effect = DuetEffect.Reach, Line = "both reach 15% farther" },
        };
        public static System.Collections.Generic.IEnumerable<Duet> For(string id) { foreach (Duet d in All) if (d.A == id || d.B == id) yield return d; }
        public static string Partner(Duet d, string id) => d.A == id ? d.B : d.A;
        /// <summary>Whether both friends of a duet are in the brigade now.</summary>
        public static bool Active(RunSession session, Duet d) => session.OwnsCompanionSpecies(d.A) && session.OwnsCompanionSpecies(d.B);
        /// <summary>A friend's weapon with the bonus of every duet it completes (compiled each battle).</summary>
        public static void Apply(RunSession session, UnitDefinition source, WeaponDefinition weapon)
        {
            if (source == null || weapon == null) return;
            foreach (Duet d in For(source.Id))
            {
                if (!Active(session, d)) continue;
                switch (d.Effect)
                {
                    case DuetEffect.Damage: weapon.Damage *= DamageBonus; break;
                    case DuetEffect.Speed: weapon.Cooldown /= SpeedBonus; break;
                    case DuetEffect.Reach: weapon.Range *= ReachBonus; break;
                    case DuetEffect.Knockback: weapon.PushDistance = Mathf.Min(.45f, weapon.PushDistance * KnockbackBonus); break;
                }
            }
        }
    }
}
