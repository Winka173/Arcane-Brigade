using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>How hard each transformation hits (requirement 24/10/2026: a transformation must feel stronger than the hero's
    /// ordinary attacks - Postmaster's gatling fired 20-damage shots where its normal volley made 2x50). Measured with the damage
    /// bench (HeroMechanicCapture -mechanic-dps: the hero and four friends against 1, 3 and 8 targets, six seconds ordinary,
    /// six transformed) and set so the brigade's damage while transformed is about x1.8 its ordinary damage in each case.
    /// Hero: every blow of the hero's while transformed (up to x5). Team: friends' blows during it (buff heroes, and the rest past x5).
    /// Values for 1, 3 and 8 enemies, blended by the enemies alive.</summary>
    public static class AwakenTuning
    {
        public const float TargetRatio = 1.8f;
        private static readonly Dictionary<string, float[]> hero = new Dictionary<string, float[]>
        {
            ["ArchiveCurator"] = new[] { 3.13f, 3.60f, 5.00f },
            ["BellDiver"] = new[] { 3.49f, 5.00f, 5.00f },
            ["BonsaiClocksmith"] = new[] { 2.47f, 2.69f, 3.71f },
            ["ButtonCavalier"] = new[] { 1.81f, 2.52f, 2.91f },
            ["ChinaMarionette"] = new[] { 2.03f, 2.68f, 3.00f },
            ["CitrusMarshal"] = new[] { 5.00f, 3.71f, 3.63f },
            ["CometRunner"] = new[] { 2.56f, 3.72f, 2.91f },
            ["CopperCauldron"] = new[] { 4.22f, 3.40f, 4.94f },
            ["DrowsySeer"] = new[] { 1.13f, 1.24f, 1.46f },
            ["GlassDice"] = new[] { 1.33f, 1.92f, 2.30f },
            ["HourglassStag"] = new[] { 0.51f, 0.83f, 1.28f },
            ["IvorySovereign"] = new[] { 1.93f, 3.01f, 5.00f },
            ["JugglingJester"] = new[] { 1.95f, 2.72f, 2.45f },
            ["KiteAdmiral"] = new[] { 2.80f, 2.04f, 1.83f },
            ["LanternKnight"] = new[] { 1.77f, 1.63f, 2.38f },
            ["LanternWeaver"] = new[] { 2.62f, 3.05f, 5.00f },
            ["LoneJelly"] = new[] { 0.46f, 0.62f, 0.78f },
            ["MirrorKeeper"] = new[] { 2.29f, 2.86f, 4.71f },
            ["MoonProwler"] = new[] { 1.87f, 1.86f, 3.29f },
            ["NightMaw"] = new[] { 1.94f, 2.36f, 2.16f },
            ["PatchworkHost"] = new[] { 1.06f, 1.43f, 2.12f },
            ["PennyBroker"] = new[] { 2.91f, 3.13f, 2.92f },
            ["PetalCitadel"] = new[] { 1.31f, 1.98f, 3.92f },
            ["Postmaster"] = new[] { 4.09f, 4.00f, 3.32f },
            ["RainArchivist"] = new[] { 1.31f, 1.76f, 2.86f },
            ["RootOracle"] = new[] { 1.80f, 2.06f, 1.84f },
            ["SoapSkater"] = new[] { 1.80f, 2.41f, 4.47f },
            ["StormFencer"] = new[] { 2.06f, 2.06f, 1.84f },
            ["SundaeDuchess"] = new[] { 2.10f, 2.69f, 2.69f },
            ["SupperMortar"] = new[] { 4.89f, 5.00f, 5.00f },
            ["ThornTreasurer"] = new[] { 2.29f, 2.94f, 4.83f },
            ["TideSinger"] = new[] { 2.93f, 3.23f, 3.66f },
            ["TinVirtuoso"] = new[] { 1.47f, 1.20f, 0.94f },
            ["VerdantMatron"] = new[] { 1.63f, 2.49f, 3.39f },
        };
        private static readonly Dictionary<string, float[]> team = new Dictionary<string, float[]>
        {
            ["ArchiveCurator"] = new[] { 1.00f, 1.00f, 1.36f },
            ["BellDiver"] = new[] { 1.00f, 1.15f, 1.16f },
            ["CitrusMarshal"] = new[] { 1.05f, 1.00f, 1.00f },
            ["IvorySovereign"] = new[] { 1.00f, 1.00f, 1.10f },
            ["LanternWeaver"] = new[] { 1.00f, 1.00f, 1.17f },
            ["SupperMortar"] = new[] { 1.00f, 1.43f, 1.69f },
            ["VelvetRegent"] = new[] { 1.19f, 1.28f, 1.24f },
        };
        public static float Hero(string id, int enemies) => Blend(hero, id, enemies);
        public static float Team(string id, int enemies) => Blend(team, id, enemies);
        private static float Blend(Dictionary<string, float[]> table, string id, int enemies)
        {
            if (id == null || !table.TryGetValue(id, out float[] v)) return 1;
            if (enemies <= 1) return v[0];
            if (enemies <= 3) return Mathf.Lerp(v[0], v[1], (enemies - 1) / 2f);
            return Mathf.Lerp(v[1], v[2], Mathf.Clamp01((enemies - 3) / 5f));
        }
    }
}
