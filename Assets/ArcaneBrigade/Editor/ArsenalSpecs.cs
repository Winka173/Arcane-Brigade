namespace ArcaneBrigade.Editor
{
    /// <summary>Tier-1 numbers and descriptions of the Bizarre Arsenal (Docs/Bizarre_Arsenal.md §1), shared by ArsenalBuilder
    /// (which turns them into weapon assets once their art exists) and ArsenalChecks (which runs them in fixtures).</summary>
    public static class ArsenalSpecs
    {
        public sealed class Spec
        {
            public string Id; public ArsenalWeapon Arsenal; public WeaponKind Kind; public WeaponMotion Motion; public DamageElement Element;
            public float Damage, Range, Cooldown, Windup, Width = 1f, Projectile = 0.3f; public string Summary;
            public string[] Pieces = new string[0];   // art pieces besides <Id>_Held_v1.png (Docs/Bizarre_Arsenal.md §5.1)
        }
        private static Spec S(string id, ArsenalWeapon a, WeaponKind k, WeaponMotion m, DamageElement e, float dmg, float range, float cd, float windup, string summary, params string[] pieces)
            => new Spec { Id = id, Arsenal = a, Kind = k, Motion = m, Element = e, Damage = dmg, Range = range, Cooldown = cd, Windup = windup, Summary = summary, Pieces = pieces };
        private const WeaponKind P = WeaponKind.Projectile, M = WeaponKind.Melee;

        public static readonly Spec[] All =
        {
            S("CherryFuseBomb", ArsenalWeapon.CherryFuseBomb, P, WeaponMotion.Recoil, DamageElement.Ember, 11, 5.0f, 1.35f, 0.3f, "Lobs a fuse bomb: it lands, fizzes, then bursts.", "CherryBomb_f0", "CherryBomb_f1", "CherryBomb_f2", "CottonPop_f0", "CottonPop_f1", "CottonPop_f2", "CottonPop_f3"),
            S("PopperSticks", ArsenalWeapon.PopperSticks, P, WeaponMotion.Recoil, DamageElement.Ember, 9, 3.2f, 1.25f, 0.24f, "Throws 3 firecracker sticks that wait, then pop in a chain.", "PopperStick_f0", "PopperStick_f1", "PopperBurst_f0", "PopperBurst_f1", "PopperBurst_f2"),
            S("DreamRacket", ArsenalWeapon.DreamRacket, P, WeaponMotion.Slash, DamageElement.Physical, 12, 5.0f, 1.0f, 0.2f, "Serves a ball that bounces to 4 enemies.", "FeltBall"),
            S("BambooVase", ArsenalWeapon.BambooVase, P, WeaponMotion.Recoil, DamageElement.Physical, 13, 4.6f, 1.3f, 0.26f, "The vase shatters; a line of bamboo springs up.", "BambooVaseShot", "BambooShoot_f0", "BambooShoot_f1", "BambooShoot_f2", "GlassShards"),
            S("RoseBouquet", ArsenalWeapon.RoseBouquet, P, WeaponMotion.Recoil, DamageElement.Arcane, 12, 4.5f, 1.15f, 0.2f, "Thrown bouquet leaves a slowing ribbon of petals.", "BouquetShot", "PaperPetal_f0", "PaperPetal_f1", "PaperPetal_f2"),
            S("HomeRunBat", ArsenalWeapon.HomeRunBat, M, WeaponMotion.Slash, DamageElement.Physical, 14, 1.5f, 1.2f, 0.28f, "Bats an enemy into the others.", "HomeRunText"),
            S("CandyDrill", ArsenalWeapon.CandyDrill, P, WeaponMotion.Thrust, DamageElement.Physical, 11, 5.0f, 1.2f, 0.22f, "Pierces 4 enemies, then drills into the floor.", "CandyDrillShot", "SpiralCrack"),
            S("InkyOctopus", ArsenalWeapon.InkyOctopus, P, WeaponMotion.Cast, DamageElement.Arcane, 10, 4.5f, 0.9f, 0.2f, "A plush octopus clings to an enemy and inks the floor.", "PlushOctopus_f0", "PlushOctopus_f1", "PlushOctopus_f2", "PlushOctopus_f3", "InkPool"),
            S("GrabberClaw", ArsenalWeapon.GrabberClaw, P, WeaponMotion.Thrust, DamageElement.Physical, 12, 4.0f, 1.3f, 0.22f, "Grabs an enemy, reels it in and flings it.", "ClawHead_f0", "ClawHead_f1", "ClawSpring"),
            S("StarforgeClaymore", ArsenalWeapon.StarforgeClaymore, M, WeaponMotion.Slam, DamageElement.Physical, 15, 2.2f, 1.6f, 0.42f, "Slow, huge sweep in front; the blade blocks shots.", "GoldLeafArc"),
            S("DragonLanternGlaive", ArsenalWeapon.DragonLanternGlaive, M, WeaponMotion.Slash, DamageElement.Ember, 11, 2.0f, 0.95f, 0.22f, "Sweep, sweep, then a long thrust.", "DragonRibbon", "LanternEmber"),
            S("TwinMoonStaff", ArsenalWeapon.TwinMoonStaff, P, WeaponMotion.Recoil, DamageElement.Arcane, 10, 3.6f, 1.25f, 0.24f, "Thrown twin blade hovers, grinds, then returns.", "TwinMoonSpin_f0", "TwinMoonSpin_f1", "TwinMoonSpin_f2", "TwinMoonSpin_f3"),
            S("PlushShark", ArsenalWeapon.PlushShark, P, WeaponMotion.Recoil, DamageElement.Frost, 13, 5.5f, 1.45f, 0.3f, "Thrown at the farthest enemy; chomps 3 on the way.", "PlushSharkShot_f0", "PlushSharkShot_f1", "FloorRipple"),
            S("DrumstickClub", ArsenalWeapon.DrumstickClub, M, WeaponMotion.Slam, DamageElement.Physical, 13, 1.6f, 1.15f, 0.3f, "Slams the floor ahead; a shock runs forward.", "BatterCrumbs", "SteamPuff_f0", "SteamPuff_f1", "SteamPuff_f2"),
            S("PaperShuriken", ArsenalWeapon.PaperShuriken, P, WeaponMotion.Recoil, DamageElement.Physical, 9, 5.0f, 0.95f, 0.16f, "3 paper stars, each bouncing to 2 more enemies.", "ShurikenShot_f0", "ShurikenShot_f1", "PaperFlower"),
            S("SquirtTrio", ArsenalWeapon.SquirtTrio, P, WeaponMotion.Recoil, DamageElement.Frost, 9, 6.0f, 0.85f, 0.14f, "3 water jets that bounce off walls and slow.", "WaterJet", "StarDroplets", "Puddle"),
            S("ConfettiBlunderbuss", ArsenalWeapon.ConfettiBlunderbuss, P, WeaponMotion.Recoil, DamageElement.Physical, 16, 3.2f, 1.25f, 0.2f, "7 candy pellets in a short cone.", "CandyPellet", "CartoonCloud_f0", "CartoonCloud_f1", "CartoonCloud_f2", "ConfettiBurst"),
            S("PlungerPopper", ArsenalWeapon.PlungerPopper, P, WeaponMotion.Thrust, DamageElement.Physical, 12, 4.2f, 1.3f, 0.22f, "Sticks, pulls the enemy in, then pops it away.", "PlungerCup", "ElasticBand", "PopText"),
            S("SignpostSlammer", ArsenalWeapon.SignpostSlammer, M, WeaponMotion.Slash, DamageElement.Physical, 12, 1.6f, 0.95f, 0.22f, "Every third swing plants a slowing road sign.", "PlantedSign_f0", "PlantedSign_f1", "RoadStripes"),
            S("DetonatorBox", ArsenalWeapon.DetonatorBox, P, WeaponMotion.Cast, DamageElement.Ember, 11, 5.0f, 1.05f, 0.24f, "Plants 3 toy bombs, then sets them all off.", "ToyBomb", "DetonatorWire", "CandyFlossCloud_f0", "CandyFlossCloud_f1", "CandyFlossCloud_f2", "CandyFlossCloud_f3"),
            S("FirecrackerPinwheel", ArsenalWeapon.FirecrackerPinwheel, P, WeaponMotion.Cast, DamageElement.Ember, 11, 4.0f, 1.2f, 0.22f, "Plants a pinwheel trap that sparks and bursts.", "Pinwheel_f0", "Pinwheel_f1", "Pinwheel_f2", "Pinwheel_f3", "PaperScraps", "BangText"),
            S("GumballGatling", ArsenalWeapon.GumballGatling, P, WeaponMotion.Recoil, DamageElement.Physical, 4, 5.0f, 0.32f, 0.08f, "Rapid gumballs; fires faster as the barrel heats.", "Gumball_f0", "Gumball_f1", "Gumball_f2", "Gumball_f3"),
            S("BalloonBuddy", ArsenalWeapon.BalloonBuddy, P, WeaponMotion.Cast, DamageElement.Arcane, 12, 5.0f, 1.4f, 0.24f, "A balloon floats into a crowd and pops.", "BalloonAnimal_f0", "BalloonAnimal_f1", "BalloonPop_f0", "BalloonPop_f1", "BalloonPop_f2"),
            S("GlitchToybox", ArsenalWeapon.GlitchToybox, P, WeaponMotion.Cast, DamageElement.Arcane, 12, 4.5f, 1.0f, 0.2f, "Each attack borrows a random weapon.", "JackBox_f0", "JackBox_f1", "JackBox_f2", "TvStatic"),
        };
    }
}
