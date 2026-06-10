using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What fills a hero's gauge (content expansion, Docs/Content_Expansion_Proposal.md §0–1, "comfortable gauges").</summary>
    public enum GaugeSource
    {
        Hit, Crit, Kill, Distance, Coin, Hurt, Time, NearEnemies, FriendHit, FriendNear, FriendCrit, FriendDistinct, FriendBond,
        NearMiss, Block, Turn, Rear, Pinned, Debt, Chest, NewSpecies, Sprouts, FarHit,
        /// <summary>An enemy felled while frozen (Sundae Duchess) / a personal hit on a probed enemy (Bell Diver).</summary>
        FrozenKill, Probed
    }

    /// <summary>Comfortable gauges (18/10/2026): every gauge fills from ordinary play, a hero's own conditions only fill it
    /// faster, nothing drains it, and the rhythm stays between ~18s (a per-second cap, however many enemies) and ~35s (a base
    /// trickle, however the hero plays). Each tier needs <see cref="GaugeTier"/> points; three tiers transform the hero.</summary>
    public sealed partial class CombatSimulation
    {
        public const float GaugeTier = 10f, GaugeTrickle = 30f / 35f, GaugeCap = 30f / 18f, GaugePrimeSeconds = 1f;
        /// <summary>Requirement 24/10/2026: every gauge fills 40% faster (twice raised; the rhythm now ~13s to ~25s).</summary>
        public const float GaugeSpeed = 1.4f;
        private float gaugePending;

        /// <summary>Points per unit of each source for each hero (Docs/Content_Expansion_v5.md §1, "Nạp từ" and "Nhanh hơn khi").</summary>
        private static readonly Dictionary<string, (GaugeSource source, float points)[]> GaugeRecipes = new Dictionary<string, (GaugeSource, float)[]>
        {
            ["Postmaster"] = new[] { (GaugeSource.Hit, 1f) },
            ["SupperMortar"] = new[] { (GaugeSource.Hit, 1f) },
            ["TreasureAcorn"] = new[] { (GaugeSource.Kill, 1.5f) },
            ["CopperCauldron"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Hurt, 2.5f) },
            ["CitrusMarshal"] = new[] { (GaugeSource.Distance, .4f), (GaugeSource.Hit, 1f) },
            ["StormFencer"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.NearMiss, 2f) },
            ["SoapSkater"] = new[] { (GaugeSource.Distance, .5f), (GaugeSource.Turn, 1.5f) },
            ["ButtonCavalier"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.NearMiss, 2f) },
            ["GlassDice"] = new[] { (GaugeSource.Coin, 1f), (GaugeSource.Hit, 1f) },
            ["RainArchivist"] = new[] { (GaugeSource.Time, .4f), (GaugeSource.Hit, 1f), (GaugeSource.Chest, 4f) },
            ["ArchiveCurator"] = new[] { (GaugeSource.Kill, 1.5f), (GaugeSource.NewSpecies, 3f) },
            ["TinVirtuoso"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Crit, 1f) },
            ["MoonProwler"] = new[] { (GaugeSource.Distance, .5f), (GaugeSource.Rear, .5f) },
            ["DrowsySeer"] = new[] { (GaugeSource.Distance, .4f), (GaugeSource.Time, .4f) },
            ["HourglassStag"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Debt, .5f) },
            ["PennyBroker"] = new[] { (GaugeSource.Coin, 1f) },
            ["LanternKnight"] = new[] { (GaugeSource.FriendNear, .5f), (GaugeSource.Time, .4f) },
            ["RootOracle"] = new[] { (GaugeSource.Time, .4f), (GaugeSource.Hit, 1f), (GaugeSource.NearEnemies, .4f) },
            ["VerdantMatron"] = new[] { (GaugeSource.Time, .4f), (GaugeSource.Hit, 1f), (GaugeSource.Sprouts, .4f) },
            ["PetalCitadel"] = new[] { (GaugeSource.Time, .4f), (GaugeSource.Hit, 1f) },
            ["ChinaMarionette"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Hurt, 2.5f) },
            ["TideSinger"] = new[] { (GaugeSource.Time, .4f), (GaugeSource.Coin, 1f) },
            ["NightMaw"] = new[] { (GaugeSource.Kill, 1.5f) },
            ["LoneJelly"] = new[] { (GaugeSource.Kill, 1.5f) },
            ["PatchworkHost"] = new[] { (GaugeSource.FriendHit, .3f), (GaugeSource.FriendDistinct, .15f) },
            ["VelvetRegent"] = new[] { (GaugeSource.FriendHit, .3f), (GaugeSource.FriendBond, .15f) },
            ["IvorySovereign"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Hurt, 1.5f) },
            ["ThornTreasurer"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Hurt, 2.5f) },
            ["CometRunner"] = new[] { (GaugeSource.Distance, .6f) },
            ["MirrorKeeper"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Block, 2f) },
            ["LanternWeaver"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Pinned, .5f) },
            // the new heroes (Docs/Content_Expansion_v5.md)
            ["KiteAdmiral"] = new[] { (GaugeSource.FarHit, 1.5f), (GaugeSource.Hit, .5f), (GaugeSource.Time, .3f) }, // FarHit: any own hit beyond 4 units (the kite string too)
            ["BonsaiClocksmith"] = new[] { (GaugeSource.Hit, 1f), (GaugeSource.Time, .4f), (GaugeSource.Kill, .5f) },
            ["JugglingJester"] = new[] { (GaugeSource.FriendCrit, 1f), (GaugeSource.Hit, .5f) },
            ["BellDiver"] = new[] { (GaugeSource.NearEnemies, .5f), (GaugeSource.Hit, 1f), (GaugeSource.Probed, .5f) },
            // the sundae melts by itself (Proposal §1.2 "Tự tan theo thời gian"); blows taken and frozen kills melt it faster
            ["SundaeDuchess"] = new[] { (GaugeSource.Time, .5f), (GaugeSource.Hit, .5f), (GaugeSource.Hurt, 2f), (GaugeSource.FrozenKill, 2f) },
        };

        /// <summary>A gauge event: the hero's own recipe decides how much it is worth (nothing for a source it does not use).</summary>
        private void Fill(GaugeSource source, float units = 1)
        {
            if (!HeroIdentityEnabled || units <= 0 || !GaugeRecipes.TryGetValue(IdentityId ?? "", out var recipe)) return;
            foreach (var (s, points) in recipe) if (s == source) gaugePending += points * units;
        }
        private bool GaugeUses(GaugeSource source)
        {
            if (!GaugeRecipes.TryGetValue(IdentityId ?? "", out var recipe)) return false;
            foreach (var (s, _) in recipe) if (s == source) return true;
            return false;
        }

        /// <summary>Once per step: the trickle plus what the hero earned, never faster than the cap; nothing while transformed
        /// or resting.</summary>
        private void SettleGauge(float delta)
        {
            if (HeroAwakening || HeroAwakenCooling || awakenMeter >= 1) { gaugePending = 0; return; }
            float earned = Mathf.Min(gaugePending, Mathf.Max(0, (GaugeCap - GaugeTrickle) * delta));
            gaugePending = Mathf.Min(gaugePending - earned, GaugeTier); // a short lull may not bank a whole transformation
            Charge((GaugeTrickle * delta + earned) * GaugeSpeed);
        }

        /// <summary>Late game: a transformation's area strikes grow with the crowd around the hero (up to +50% at 40 enemies
        /// within 6 units), so they still matter in a sea of enemies.</summary>
        private float CrowdScale
        {
            get
            {
                if (Elapsed < crowdAt) return crowdScale;
                crowdAt = Elapsed + .25f; int n = 0;
                foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy && (u.Position - Champion.Position).sqrMagnitude <= 36) n++;
                crowdScale = 1 + .5f * Mathf.Clamp01((n - 10) / 30f);
                return crowdScale;
            }
        }
        private float crowdAt = -1, crowdScale = 1;
        /// <summary>More than 80 enemies: the transformation's secondary ink (ambient glyphs and links) is left out.</summary>
        private bool Crowded => liveEnemyCount > 80;
        private int liveEnemyCount;
    }
}
