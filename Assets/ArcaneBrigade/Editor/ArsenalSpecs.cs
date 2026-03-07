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
            public string[] Pieces = new string[0];   // painted objects besides <Id>_Held_v1.png (Docs/Bizarre_Arsenal.md §5.1); effects are painted ink (ArsenalInk)
        }
        private static Spec S(string id, ArsenalWeapon a, WeaponKind k, WeaponMotion m, DamageElement e, float dmg, float range, float cd, float windup, string summary, params string[] pieces)
            => new Spec { Id = id, Arsenal = a, Kind = k, Motion = m, Element = e, Damage = dmg * TuningOf(id), Range = range, Cooldown = cd, Windup = windup, Summary = summary, Pieces = pieces, Width = WidthOf(id) };
        private const WeaponKind P = WeaponKind.Projectile, M = WeaponKind.Melee;

        /// <summary>Held size relative to the usual (fist-sized arms are drawn smaller so they never dwarf their bearer).</summary>
        private static readonly System.Collections.Generic.Dictionary<string, float> Widths = new System.Collections.Generic.Dictionary<string, float>
            { { "PawMitten", 0.62f }, { "BareKnuckles", 0.6f }, { "StarKnuckle", 0.65f }, { "VerdictGavel", 0.85f }, { "CatcherMitt", 0.75f } };
        public static float WidthOf(string id) => Widths.TryGetValue(id, out float w) ? w : 1f;

        /// <summary>Damage tuning from the build-balance weapon fixture (ArsenalBalanceProbe, 18/10/2026): each Arsenal weapon
        /// scaled toward the median of the older weapons of its kind (tier 1 wave 5 and tier 3 wave 15 averaged).</summary>
        private static readonly System.Collections.Generic.Dictionary<string, float> Tuning = new System.Collections.Generic.Dictionary<string, float>
            { { "BalloonBuddy", 0.98f }, { "BambooVase", 3.39f }, { "BareKnuckles", 2.84f }, { "BuzzerBlock", 1.9f }, { "CandyDrill", 1.23f }, { "CatcherMitt", 2.84f }, { "CherryFuseBomb", 1.35f }, { "ConfettiBlunderbuss", 2.01f }, { "CorkRifle", 0.71f }, { "DetonatorBox", 1.6f }, { "DowsingRod", 1.7f }, { "DragonLanternGlaive", 1.66f }, { "DreamRacket", 2.13f }, { "DrumstickClub", 1.54f }, { "FanfareHorn", 0.79f }, { "FirecrackerPinwheel", 2.16f }, { "FizzBottle", 2.6f }, { "FlySwatter", 1.33f }, { "GlitchToybox", 3.25f }, { "GrabberClaw", 2.14f }, { "GumballGatling", 2.11f }, { "HearthBrick", 1.9f }, { "HomeRunBat", 2.2f }, { "InkyOctopus", 3.5f }, { "KunaiFan", 1.65f }, { "LeekBaton", 1.39f }, { "PaperShuriken", 1.61f }, { "PawMitten", 2.66f }, { "PlungerPopper", 3.5f }, { "PlushShark", 2.27f }, { "RoseBouquet", 1.22f }, { "SaucerBlaster", 1.42f }, { "SignpostSlammer", 1.04f }, { "SquirtTrio", 1.8f }, { "StarKnuckle", 2.02f }, { "StarforgeClaymore", 0.67f }, { "TalismanIngot", 0.66f }, { "VerdictGavel", 3.5f }, { "WindupWrench", 1.51f } };
        public static float TuningOf(string id) => Tuning.TryGetValue(id, out float t) ? t : 1f;

        public static readonly Spec[] All =
        {
            S("CherryFuseBomb", ArsenalWeapon.CherryFuseBomb, P, WeaponMotion.Recoil, DamageElement.Ember, 11, 5.0f, 1.35f, 0.3f, "Lobs a fuse bomb: it lands, fizzes, then bursts.", "CherryBomb_f0", "CherryBomb_f1", "CherryBomb_f2"),
            S("PopperSticks", ArsenalWeapon.PopperSticks, P, WeaponMotion.Recoil, DamageElement.Ember, 9, 3.2f, 1.25f, 0.24f, "Throws 3 firecracker sticks that wait, then pop in a chain.", "PopperStick_f0", "PopperStick_f1"),
            S("DreamRacket", ArsenalWeapon.DreamRacket, P, WeaponMotion.Slash, DamageElement.Physical, 12, 5.0f, 1.0f, 0.2f, "Serves a ball that bounces to 4 enemies.", "FeltBall"),
            S("BambooVase", ArsenalWeapon.BambooVase, P, WeaponMotion.Recoil, DamageElement.Physical, 13, 4.6f, 1.3f, 0.26f, "The vase shatters; a line of bamboo springs up.", "BambooVaseShot", "BambooShoot_f0", "BambooShoot_f1", "BambooShoot_f2"),
            S("RoseBouquet", ArsenalWeapon.RoseBouquet, P, WeaponMotion.Recoil, DamageElement.Arcane, 12, 4.5f, 1.15f, 0.2f, "Thrown bouquet leaves a slowing ribbon of petals.", "BouquetShot"),
            S("HomeRunBat", ArsenalWeapon.HomeRunBat, M, WeaponMotion.Slash, DamageElement.Physical, 14, 1.5f, 1.2f, 0.28f, "Bats an enemy into the others."),
            S("CandyDrill", ArsenalWeapon.CandyDrill, P, WeaponMotion.Thrust, DamageElement.Physical, 11, 5.0f, 1.2f, 0.22f, "Pierces 4 enemies, then drills into the floor.", "CandyDrillShot"),
            S("InkyOctopus", ArsenalWeapon.InkyOctopus, P, WeaponMotion.Cast, DamageElement.Arcane, 10, 4.5f, 0.9f, 0.2f, "A plush octopus clings to an enemy and inks the floor.", "PlushOctopus_f0", "PlushOctopus_f1", "PlushOctopus_f2", "PlushOctopus_f3"),
            S("GrabberClaw", ArsenalWeapon.GrabberClaw, P, WeaponMotion.Thrust, DamageElement.Physical, 12, 4.0f, 1.3f, 0.22f, "Grabs an enemy, reels it in and flings it.", "ClawHead_f0", "ClawHead_f1"),
            S("StarforgeClaymore", ArsenalWeapon.StarforgeClaymore, M, WeaponMotion.Slam, DamageElement.Physical, 15, 2.2f, 1.6f, 0.42f, "Slow, huge sweep in front; the blade blocks shots."),
            S("DragonLanternGlaive", ArsenalWeapon.DragonLanternGlaive, M, WeaponMotion.Slash, DamageElement.Ember, 11, 2.0f, 0.95f, 0.22f, "Sweep, sweep, then a long thrust."),
            S("TwinMoonStaff", ArsenalWeapon.TwinMoonStaff, P, WeaponMotion.Recoil, DamageElement.Arcane, 10, 3.6f, 1.25f, 0.24f, "Thrown twin blade hovers, grinds, then returns."),
            S("PlushShark", ArsenalWeapon.PlushShark, P, WeaponMotion.Recoil, DamageElement.Frost, 13, 5.5f, 1.45f, 0.3f, "Thrown at the farthest enemy; chomps 3 on the way.", "PlushSharkShot_f0"),
            S("DrumstickClub", ArsenalWeapon.DrumstickClub, M, WeaponMotion.Slam, DamageElement.Physical, 13, 1.6f, 1.15f, 0.3f, "Slams the floor ahead; a shock runs forward."),
            S("PaperShuriken", ArsenalWeapon.PaperShuriken, P, WeaponMotion.Recoil, DamageElement.Physical, 9, 5.0f, 0.95f, 0.16f, "3 paper stars, each bouncing to 2 more enemies.", "ShurikenShot_f0"),
            S("SquirtTrio", ArsenalWeapon.SquirtTrio, P, WeaponMotion.Recoil, DamageElement.Frost, 9, 6.0f, 0.85f, 0.14f, "3 water jets that bounce off walls and slow.", "WaterJet"),
            S("ConfettiBlunderbuss", ArsenalWeapon.ConfettiBlunderbuss, P, WeaponMotion.Recoil, DamageElement.Physical, 16, 3.2f, 1.25f, 0.2f, "7 candy pellets in a short cone.", "CandyPellet"),
            S("PlungerPopper", ArsenalWeapon.PlungerPopper, P, WeaponMotion.Thrust, DamageElement.Physical, 12, 4.2f, 1.3f, 0.22f, "Sticks, pulls the enemy in, then pops it away.", "PlungerCup"),
            S("SignpostSlammer", ArsenalWeapon.SignpostSlammer, M, WeaponMotion.Slash, DamageElement.Physical, 12, 1.6f, 0.95f, 0.22f, "Every third swing plants a slowing road sign."),
            S("DetonatorBox", ArsenalWeapon.DetonatorBox, P, WeaponMotion.Cast, DamageElement.Ember, 11, 5.0f, 1.05f, 0.24f, "Plants 3 toy bombs, then sets them all off.", "ToyBomb"),
            S("FirecrackerPinwheel", ArsenalWeapon.FirecrackerPinwheel, P, WeaponMotion.Cast, DamageElement.Ember, 11, 4.0f, 1.2f, 0.22f, "Plants a pinwheel trap that sparks and bursts.", "Pinwheel_f0", "Pinwheel_f1", "Pinwheel_f2", "Pinwheel_f3"),
            S("GumballGatling", ArsenalWeapon.GumballGatling, P, WeaponMotion.Recoil, DamageElement.Physical, 4, 5.0f, 0.32f, 0.08f, "Rapid gumballs; fires faster as the barrel heats.", "Gumball_f0"),
            S("BalloonBuddy", ArsenalWeapon.BalloonBuddy, P, WeaponMotion.Cast, DamageElement.Arcane, 12, 5.0f, 1.4f, 0.24f, "A balloon floats into a crowd and pops.", "BalloonAnimal_f0"),
            S("GlitchToybox", ArsenalWeapon.GlitchToybox, P, WeaponMotion.Cast, DamageElement.Arcane, 12, 4.5f, 1.0f, 0.2f, "Each attack borrows a random weapon."),
            // research pass 3
            S("SaucerBlaster", ArsenalWeapon.SaucerBlaster, P, WeaponMotion.Recoil, DamageElement.Arcane, 12, 5.5f, 1.05f, 0.18f, "Fires a toy saucer that pierces 4 enemies.", "SaucerShot_f0"),
            S("FanfareHorn", ArsenalWeapon.FanfareHorn, P, WeaponMotion.Cast, DamageElement.Arcane, 12, 3.5f, 1.15f, 0.22f, "Blows three golden sound fins forward.", "SoundFin_f0"),
            S("WindupWrench", ArsenalWeapon.WindupWrench, M, WeaponMotion.Slash, DamageElement.Physical, 12, 1.5f, 0.95f, 0.2f, "Every third hit winds an enemy up; it spins dizzy.", "WindupKey_f0", "WindupKey_f1"),
            S("CorkRifle", ArsenalWeapon.CorkRifle, P, WeaponMotion.Recoil, DamageElement.Physical, 12, 8.0f, 1.55f, 0.35f, "A long cork shot through the whole line.", "CorkBullet"),
            S("CatcherMitt", ArsenalWeapon.CatcherMitt, P, WeaponMotion.Recoil, DamageElement.Physical, 11, 5.0f, 1.1f, 0.22f, "Catches enemy shots in front and throws them back.", "Fastball"),
            S("FizzBottle", ArsenalWeapon.FizzBottle, P, WeaponMotion.Recoil, DamageElement.Frost, 11, 4.5f, 1.25f, 0.24f, "The bottle bursts into a fizzing, slowing puddle."),
            S("LeekBaton", ArsenalWeapon.LeekBaton, M, WeaponMotion.Slash, DamageElement.Physical, 11, 1.7f, 0.85f, 0.18f, "A wide, stinging swing that pushes and slows."),
            S("DowsingRod", ArsenalWeapon.DowsingRod, P, WeaponMotion.Cast, DamageElement.Arcane, 11, 5.5f, 1.05f, 0.2f, "Finds the weakest enemy in reach and exposes it.", "TwigDart"),
            S("StarKnuckle", ArsenalWeapon.StarKnuckle, M, WeaponMotion.Thrust, DamageElement.Physical, 9, 1.2f, 0.55f, 0.1f, "Quick jabs; every fourth is a star uppercut.", "StarShard"),
            S("BuzzerBlock", ArsenalWeapon.BuzzerBlock, P, WeaponMotion.Cast, DamageElement.Arcane, 12, 4.0f, 1.15f, 0.2f, "A prank buzzer on the floor shocks a chain of enemies.", "BuzzerFloor_f0"),
            S("FlySwatter", ArsenalWeapon.FlySwatter, M, WeaponMotion.Slam, DamageElement.Physical, 12, 1.4f, 0.9f, 0.2f, "Swats flat and dazes; every third swat crushes the weak."),
            // research pass 4
            S("PawMitten", ArsenalWeapon.PawMitten, M, WeaponMotion.Thrust, DamageElement.Physical, 6, 1.0f, 0.42f, 0.08f, "Very quick, very short paw swipes; every fifth rakes three claw lines."),
            S("HearthBrick", ArsenalWeapon.HearthBrick, P, WeaponMotion.Recoil, DamageElement.Physical, 14, 4.2f, 1.35f, 0.3f, "Lobs a brick: it thuds down, dazing and shoving what it lands on."),
            S("VerdictGavel", ArsenalWeapon.VerdictGavel, M, WeaponMotion.Slam, DamageElement.Physical, 9, 1.3f, 1.05f, 0.26f, "Bangs a verdict down in front; luck and gathered coins make it heavier."),
            S("TalismanIngot", ArsenalWeapon.TalismanIngot, P, WeaponMotion.Recoil, DamageElement.Arcane, 6, 4.5f, 0.5f, 0.1f, "Throws charm-sealed gold ingots in fours; each pops in a small blast."),
            S("KunaiFan", ArsenalWeapon.KunaiFan, P, WeaponMotion.Recoil, DamageElement.Physical, 8, 6.0f, 1.1f, 0.16f, "Throws its three kunai one after another down one line.", "KunaiShot"),
            // our own takes on pass 4's two unconfirmed items
            S("FortuneGlobe", ArsenalWeapon.FortuneGlobe, P, WeaponMotion.Cast, DamageElement.Arcane, 10, 5.0f, 1.2f, 0.22f, "Three crystal motes wander out, growing larger and harder as they fly.", "GlobeMote"),
            S("TopiaryShears", ArsenalWeapon.TopiaryShears, M, WeaponMotion.Scissors, DamageElement.Physical, 10, 1.4f, 0.9f, 0.16f, "Two quick snips down a line ahead; every third trims the toughest enemy."),
            S("BareKnuckles", ArsenalWeapon.BareKnuckles, M, WeaponMotion.Thrust, DamageElement.Physical, 5, 0.95f, 0.38f, 0.06f, "No weapon at all: very fast punches; a critical punch pops the air ahead."),
        };
    }
}
