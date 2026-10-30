using System;
using System.Collections.Generic;

namespace ArcaneBrigade.Editor
{
    public sealed class BalanceSimSource
    {
        public string Kind, Id;
        public int Step;
        public double Damage;
    }

    internal static class BalanceSimTelemetryTuning
    {
        public static readonly int[] StepsDescending = { 8, 6, 4, 2 };
    }

    /// <summary>Reads effective hostile HP loss from the runtime ledger; no per-hit simulation work is added.</summary>
    public sealed class BalanceSimTelemetry
    {
        private readonly CombatSimulation simulation;
        private readonly Dictionary<int, float> fallenAt = new Dictionary<int, float>();
        private readonly List<BalanceSimSource> sources = new List<BalanceSimSource>();
        private bool finished;
        public IReadOnlyList<BalanceSimSource> Sources => sources;
        public double TotalDamage { get; private set; }
        public int Falls { get; private set; }
        public double DownSeconds { get; private set; }
        public int PeakEnemies { get; private set; }

        public BalanceSimTelemetry(CombatSimulation simulation)
        {
            this.simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            if (simulation.Mortality == null) throw new InvalidOperationException("Start combat before attaching balance telemetry.");
            simulation.Mortality.Fell += OnFell;
            simulation.Mortality.Rose += OnRose;
            Sample();
        }

        private void OnFell(FriendMortalityEvent value)
        {
            Falls++;
            fallenAt[value.FriendId] = value.Time;
        }

        private void OnRose(FriendMortalityEvent value)
        {
            if (!fallenAt.TryGetValue(value.FriendId, out float at)) return;
            DownSeconds += Math.Max(0, value.Time - at);
            fallenAt.Remove(value.FriendId);
        }

        public void Sample()
        {
            int alive = 0;
            for (int index = 0; index < simulation.Units.Count; index++)
            {
                UnitState unit = simulation.Units[index];
                if (unit.Alive && unit.Faction == Faction.Enemy) alive++;
            }
            PeakEnemies = Math.Max(PeakEnemies, alive);
        }

        public void Finish()
        {
            if (finished) return;
            finished = true;
            Sample();
            foreach (float at in fallenAt.Values) DownSeconds += Math.Max(0, simulation.Mortality.Elapsed - at);
            fallenAt.Clear();
            simulation.Mortality.Fell -= OnFell;
            simulation.Mortality.Rose -= OnRose;
            var grouped = new Dictionary<string, BalanceSimSource>(StringComparer.Ordinal);
            foreach (ContributionEntry entry in simulation.Contributions)
            {
                if (entry.Damage <= 0) continue;
                BalanceSimSource row = Classify(entry);
                string key = row.Kind + "/" + row.Id + "/" + row.Step;
                if (!grouped.TryGetValue(key, out BalanceSimSource aggregate))
                {
                    aggregate = row;
                    grouped.Add(key, aggregate);
                    sources.Add(aggregate);
                }
                aggregate.Damage += entry.Damage;
                TotalDamage += entry.Damage;
            }
            sources.Sort((left, right) =>
            {
                int order = string.CompareOrdinal(left.Kind, right.Kind);
                if (order == 0) order = string.CompareOrdinal(left.Id, right.Id);
                return order == 0 ? left.Step.CompareTo(right.Step) : order;
            });
        }

        private BalanceSimSource Classify(ContributionEntry entry)
        {
            string effect = entry.EffectId ?? "primary";
            if (effect.StartsWith("bond:", StringComparison.Ordinal))
            {
                string rule = effect.Substring("bond:".Length);
                string bond = BondForRule(rule);
                return new BalanceSimSource { Kind = "bond", Id = bond + "/" + rule, Step = ActiveStep(simulation.Bonds, bond) };
            }
            if (effect.StartsWith("bond-", StringComparison.Ordinal))
                return new BalanceSimSource { Kind = "bond", Id = "unattributed/" + effect };
            if (effect.StartsWith("skill-", StringComparison.Ordinal) || effect == "v3-reflect")
                return new BalanceSimSource { Kind = "companion_skill", Id = entry.UnitId + "/" + effect };
            bool hero = simulation.Champion != null && entry.UnitId == simulation.Champion.Definition.Id;
            if (hero && effect != "primary" && effect.StartsWith("hero-", StringComparison.Ordinal))
                return new BalanceSimSource { Kind = "hero", Id = entry.UnitId + "/" + effect };
            // Secondary weapon effects retain their effect ID; runtime stat amplification stays with its damaging source.
            return new BalanceSimSource { Kind = "weapon", Id = entry.UnitId + "/" + entry.WeaponId + "/" + effect };
        }

        public static int ActiveStep(BondRuntime bonds, string id)
        {
            if (bonds == null) return 0;
            foreach (int step in BalanceSimTelemetryTuning.StepsDescending) if (bonds.Active(id, step)) return step;
            return 0;
        }

        /// <summary>Maps emitted runtime rule IDs to their canonical catalog bond, preserving unknown rules explicitly.</summary>
        public static string BondForRule(string rule)
        {
            switch (rule)
            {
                case "Phoenix": return "ember";
                case "IceSoldier": return "frost";
                case "Shade": return "dusk";
                case "LampWisp": return "lantern-guild";
                case "Chef": return "hearth";
                case "Leviathan": return "tidal";
                case "WindBeast": return "windborne";
                case "Sprout": case "Sapling": case "Treant": case "GreatTree": return "orchard";
                case "Colossus": return "stone";
                case "Turret": return "clock";
                case "Spirit": return "spirit";
                case "Orchestra": return "melody";
                case "burn": case "burn-burst": case "inferno-burst": return "ember";
                case "poison": return "venom";
                case "shatter": return "frost";
                case "storm-chain": case "discharge": case "storm-cloud": return "static-club";
                case "pool-shock": case "leviathan": return "tidal";
                case "execute": case "eclipse": return "dusk";
                case "falling-star": return "astral";
                case "carnival-burst": return "oddity";
                case "court-law-burst": return "regal";
                case "stone-collision": return "stone";
                case "singularity-drag": case "singularity-collapse": case "after-hole": case "gravity-clump": return "gravity";
                case "minor-crush": return "singularity";
                case "bladenado": case "spinning-blade": return "bladenado";
                // The runtime combines Stone and Spiky Shell into one emitted amount.
                case "thorn-reflect": return "shared-stone-spiky-shell";
                case "candy-volley": return "sweet-tooth";
                case "rune": return "rune";
                case "mask-shard": return "mask";
                case "orchestra-damage-note": return "melody";
                case "haymaker": return "gauntlet";
                case "shanty-splash": return "sea-shanty";
                case "guard-shockwave": return "bulwark";
                case "shield-burst-damage": return "shield";
                case "paper-crane": return "paper-wings";
                case "desperado-volley": return "desperado";
                case "sniper-carry": return "sniper";
                case "gear": return "gear";
                default: return "unattributed";
            }
        }
    }
}
