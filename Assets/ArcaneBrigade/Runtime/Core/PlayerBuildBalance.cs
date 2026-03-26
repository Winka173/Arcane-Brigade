using System.Collections.Generic;

namespace ArcaneBrigade
{
    /// <summary>Hero offence is worth several ordinary bodies; relic investment can carry a smaller brigade.
    /// Applied to compiled damage, never to weapon assets or saved stacks.
    /// Balance pass round 3 (Docs/Balance_Pass_2026-10-19.md): per-hero power and vitality tuned from 186-campaign sweeps (all 31 heroes
    /// at Wandering and Nightmare, three seeds) toward a 55–80% / 30–60% win band. Vitality scales the hero's base maximum HP
    /// (before items, bonds and corrections); both live here so no asset builder can revert them.</summary>
    public static class PlayerBuildBalance
    {
        // before round 3: LoneJelly 6.5, GlassDice 3.6, RootOracle 3.6, PatchworkHost 4.5, everyone else 3; vitality 1 for all.
        // Round 3 baseline wins (Wandering + Nightmare, 6 runs): 6/6 Postmaster, RootOracle, GlassDice, MirrorKeeper, ChinaMarionette,
        // SoapSkater, ButtonCavalier, HourglassStag; 0/6 LoneJelly, IvorySovereign, ArchiveCurator; 1/6 DrowsySeer, CometRunner, PatchworkHost.
        // Trimming the 6/6 heroes (vitality x0.7, power 2.3) changed none of their results, so those trims were reverted: their
        // strength comes from the brigade they build, not from the hero's own HP or weapon (Docs/Balance_Pass_2026-10-19.md, round 3).
        private static readonly Dictionary<string, float> Power = new Dictionary<string, float>
        {
            ["LoneJelly"] = 6.5f, ["GlassDice"] = 3.6f, ["RootOracle"] = 3.6f, ["PatchworkHost"] = 5f,
            ["ArchiveCurator"] = 4f, ["IvorySovereign"] = 4f, ["DrowsySeer"] = 4f, ["CometRunner"] = 3.6f,
            ["TideSinger"] = 3.8f, ["NightMaw"] = 3.3f, ["PetalCitadel"] = 3.8f, ["StormFencer"] = 3.3f, ["SupperMortar"] = 3.3f,
            // the five expansion heroes (step 4c, 19/10/2026): start in the band of the newest heroes; balance by their mechanics
            ["KiteAdmiral"] = 3.3f, ["SundaeDuchess"] = 3.3f, ["BonsaiClocksmith"] = 3.3f, ["JugglingJester"] = 3.3f, ["BellDiver"] = 3.3f,
        };
        private static readonly Dictionary<string, float> Vitality = new Dictionary<string, float>
        {
            ["LoneJelly"] = 1.5f, ["ArchiveCurator"] = 1.3f,
            ["DrowsySeer"] = 1.5f, ["TideSinger"] = 1.5f, ["CometRunner"] = 1.45f, ["PatchworkHost"] = 1.45f,
            ["NightMaw"] = 1.3f, ["PetalCitadel"] = 1.45f, ["StormFencer"] = 1.3f, ["SupperMortar"] = 1.3f,
            ["VerdantMatron"] = 1.15f, ["CopperCauldron"] = 1.15f, ["MoonProwler"] = 1.15f,
            ["KiteAdmiral"] = 1.3f, ["SundaeDuchess"] = 1.3f, ["BonsaiClocksmith"] = 1.3f, ["JugglingJester"] = 1.3f, ["BellDiver"] = 1.15f,
        };
        /// <summary>Card Queen (Ivory Sovereign) royal health: each injury costs exactly 1. 12 before round 3, 16 in round 3 (still 1/6).</summary>
        public const int CardQueenHealth = 18;
        public static float HeroPower(string hero) => hero != null && Power.TryGetValue(hero, out float value) ? value : 3f;
        public static float HeroVitality(string hero) => hero != null && Vitality.TryGetValue(hero, out float value) ? value : 1f;
        public static float DamageSlope(bool companion) => companion ? .3f : .6f;
        public static float AttackSpeedSlope(bool companion) => companion ? .12f : .3f;
        public static float AttackSpeedCap(bool companion) => companion ? 2.2f : 4f;
    }
}
