using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Deterministic rules-only checks: no scene, art or random seed required.</summary>
    public static class BondEngineChecks
    {
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("BondEngine: " + message); }
        private static void Near(float actual, float expected, string message)
        { Require(Mathf.Abs(actual - expected) < .001f, message + " (" + actual + " vs " + expected + ")"); }

        private sealed class Fight : IDisposable
        {
            public readonly BondRuntime Runtime;
            public readonly UnitState Hero, Friend;
            public readonly List<UnitState> Enemies = new List<UnitState>();
            public readonly List<BondRuleEvent> Events = new List<BondRuleEvent>();
            public readonly List<BondDamageRequest> Damage = new List<BondDamageRequest>();
            public readonly List<BondSpawnRequest> Spawns = new List<BondSpawnRequest>();
            private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
            private T Asset<T>() where T : ScriptableObject { T value = ScriptableObject.CreateInstance<T>(); assets.Add(value); return value; }
            public Fight(string id, int count, params string[] additional)
            {
                WeaponDefinition weapon = Asset<WeaponDefinition>(); weapon.Damage = 10; weapon.Kind = WeaponKind.Melee;
                UnitDefinition ally = Asset<UnitDefinition>(); ally.Weapon = weapon; ally.Health = 100; ally.CombatTier = 3;
                ally.Tags = new[] { id }.Concat(additional).ToArray();
                UnitDefinition enemy = Asset<UnitDefinition>(); enemy.Weapon = weapon; enemy.Health = 10000;
                Hero = new UnitState { Id = 1, Faction = Faction.Champion, Definition = ally, Health = 100 };
                Friend = new UnitState { Id = 2, Faction = Faction.Companion, Definition = ally, Health = 100 };
                for (int i = 0; i < 100; i++) Enemies.Add(new UnitState { Id = 3 + i, Faction = Faction.Enemy, Definition = enemy, Health = enemy.Health, Position = new Vector2(1 + i * 4, 0) });
                var definitions = new List<BondDefinition>(); var members = new Dictionary<string, int>();
                foreach (string bond in ally.Tags)
                {
                    definitions.Add(new BondDefinition { Id = bond, Steps = new[] { new BondStep { Count = 2 }, new BondStep { Count = 4 }, new BondStep { Count = 6 }, new BondStep { Count = 8 } } });
                    members[bond] = count;
                }
                Runtime = new BondRuntime(definitions, members);
                Runtime.RuleTriggered += value => Events.Add(value);
                Runtime.DamageRequested += value => Damage.Add(value);
                Runtime.SpawnRequested += value => Spawns.Add(value);
                Reset();
            }
            public void Reset() => Runtime.ResetWave(Hero, new[] { Hero, Friend }.Concat(Enemies));
            public float Hit(int index = 0, bool crit = false, UnitState attacker = null)
            {
                UnitState target = Enemies[index]; attacker = attacker ?? Friend;
                float damage = Runtime.ModifyHit(attacker, target, 10, crit);
                target.Health = Mathf.Max(0, target.Health - damage);
                Runtime.OnHit(attacker, target, damage, crit);
                if (target.Health <= 0) Kill(index);
                return damage;
            }
            public void Kill(int index) { Enemies[index].Alive = false; Enemies[index].Health = 0; Runtime.OnKill(Enemies[index]); }
            public int Fired(string rule) => Events.Count(value => value.Rule == rule);
            public void Dispose() { foreach (UnityEngine.Object asset in assets) UnityEngine.Object.DestroyImmediate(asset); }
        }

        public static void Run()
        {
            Require(BondBuildRecipes.All.Length == 12, "Twelve section 4 build recipes");
            V3CatalogAndRules(); ThresholdsAndPeriodicHits(); Ember(); Frost(); Storm(); Venom(); Dusk(); MinorState(); Resets();
            Debug.Log("BondEngineChecks: PASS (100-enemy deterministic fixtures; all requested steps and reset rules)");
        }
        private static void V3CatalogAndRules()
        {
            BondDefinition[] definitions = BondV3Catalog.CreateDefinitions();
            Require(definitions.Length == BondTuning.MajorCount + BondTuning.MinorCount, "V3 has 52 bonds");
            var members = new Dictionary<string, int>(); int roster = 0;
            foreach (string id in BondV3Catalog.CompanionIds)
            {
                roster++; string[] tags = BondV3Catalog.Tags(id); var unique = new HashSet<string>();
                foreach (string tag in tags) { Require(unique.Add(tag), "No duplicate membership for " + id); members[tag] = members.TryGetValue(tag, out int n) ? n + 1 : 1; }
            }
            Require(roster == BondTuning.RosterCount, "All 120 companions have canonical memberships");
            foreach (BondDefinition definition in definitions)
            {
                Require(members.TryGetValue(definition.Id, out int total), "Every bond has members");
                bool major = BondV3Catalog.IsMajor(definition.Id);
                Require(major ? total == BondTuning.MajorMembers : total == BondTuning.SmallMinorMembers || total == BondTuning.LargeMinorMembers, "One spare member for " + definition.Id);
                var runtime = new BondRuntime(definitions, new Dictionary<string, int> { [definition.Id] = total });
                Require(runtime.Active(definition.Id) && runtime.Active(definition.Id, 4), "First two steps for " + definition.Id);
                Require(runtime.Active(definition.Id, 6) == (major || total == BondTuning.LargeMinorMembers), "Third step for " + definition.Id);
                Require(runtime.Active(definition.Id, 8) == major, "Fourth step only on majors");
            }
            using (var fight = new Fight("tidal", 4))
            {
                for (int i = 0; i < BondTuning.TideEvery; i++) fight.Hit();
                Require(fight.Runtime.State(fight.Enemies[0]).SoakedUntil > 0, "Fifth Tide hit soaks an area");
                fight.Runtime.Tick(BondTuning.RiptidePeriod);
                foreach (UnitState enemy in fight.Enemies) Require(fight.Runtime.State(enemy).SoakedUntil > fight.Runtime.Time, "Riptide covers 100 enemies");
            }
            using (var fight = new Fight("bulwark", 8))
            {
                Require(fight.Runtime.SupportRequests.Count(r => r.Kind == BondSupportKind.Shield) == 2, "Bulwark shields the hero and companion");
                Require(fight.Runtime.TryPreventFall(fight.Friend), "First friend gets a last stand");
                Require(!fight.Runtime.TryPreventFall(fight.Hero), "Last stand only once per wave");
                fight.Reset(); Require(fight.Runtime.TryPreventFall(fight.Hero), "Last stand renews next wave");
            }
            using (var fight = new Fight("spirit", 6))
            {
                fight.Runtime.OnFriendFell(fight.Friend); fight.Runtime.OnFriendFell(fight.Hero);
                Require(fight.Runtime.SupportRequests.Count(r => r.Kind == BondSupportKind.Revive) == 1, "Only first fall gets Revenant");
                Near(fight.Runtime.ReviveTimeReduction, BondTuning.SpiritReviveReduction, "Spirit shortens revival by four seconds");
            }
            using (var fight = new Fight("melody", 6))
            {
                fight.Runtime.Tick(BondTuning.StillSeconds);
                for (int i = 0; i < BondTuning.MelodyEvery; i++) fight.Runtime.OnCast(fight.Friend);
                Require(fight.Runtime.Beats == 1, "Fourth cast gives a Beat");
                fight.Runtime.Tick(BondTuning.BeatSeconds);
                Require(fight.Runtime.AttackSpeedBonus > 0, "Stillness doubles Beat duration");
            }
            using (var fight = new Fight("orchard", 2))
            {
                fight.Runtime.OnWaveSurvived(); BondRunProgress progress = fight.Runtime.RunProgress;
                var next = new BondRuntime(definitions, new Dictionary<string, int> { ["orchard"] = 2 }, progress);
                Near(next.RunDamageBonus, BondTuning.GrowthPerWave, "Garden growth survives runtime replacement");
                Near(next.RunHealthBonus, BondTuning.GrowthPerWave, "Garden also grows health");
            }
            using (var fight = new Fight("gravity", 8))
            {
                fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                fight.Enemies[2].Position = fight.Enemies[0].Position + Vector2.up;
                fight.Enemies[3].Position = fight.Enemies[0].Position - Vector2.up;
                fight.Runtime.Tick(.1f);
                Require(fight.Runtime.State(fight.Enemies[0]).ClusterNeighbors >= BondTuning.ClusterMinimum, "Clump count is cached across 100 enemies");
                Require(fight.Runtime.SpeedScale(fight.Enemies[0]) < 1, "Gravity well slows nearby enemies");
            }
        }
        private static void ThresholdsAndPeriodicHits()
        {
            foreach (string id in new[] { "ember", "frost", "static-club", "venom", "dusk", "gauntlet", "horn", "gear", "rune", "sea-shanty", "mask", "big-top", "root", "lantern-guild", "clover", "sweet-tooth" })
                for (int count = 0; count <= 9; count++) using (var fight = new Fight(id, count))
                    foreach (int step in new[] { 2, 4, 6, 8 }) Require(fight.Runtime.Active(id, step) == (count >= step), id + " threshold " + count + "/" + step);
            foreach (string id in new[] { "horn", "gear", "rune", "sea-shanty" })
                for (int count = 0; count <= 8; count++) using (var fight = new Fight(id, count))
                {
                    int every = id == "horn" ? (count >= 6 ? BondTuning.HornSixEvery : count >= 4 ? BondTuning.HornFourEvery : BondTuning.HornEvery)
                        : id == "rune" ? (count >= 6 ? BondTuning.RuneSixEvery : count >= 4 ? BondTuning.RuneFourEvery : BondTuning.RuneEvery)
                        : id == "gear" ? (count >= 4 ? BondTuning.GearFourEvery : BondTuning.GearEvery)
                        : count >= 4 ? BondTuning.ShantyFourEvery : BondTuning.ShantyEvery;
                    fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                    int shots = 0; fight.Runtime.BonusShotRequested += (source, damage) => { shots++; Near(damage, 10, "Gear weapon damage"); };
                    for (int i = 0; i < 100; i++) { fight.Hit(); Require(fight.Fired(id) == (count >= 2 ? (i + 1) / every : 0), id + " exact trigger boundary " + (i + 1)); }
                    if (id == "gear") Require(shots == (count >= 2 ? 100 / every : 0), "Bonus shots requested");
                    if (id == "rune" && count >= 2) foreach (var hit in fight.Damage) Near(hit.Damage, 10 * BondTuning.RuneDamage, "Rune damage");
                    if (id == "horn" && count >= 2) { Near(fight.Enemies[0].SleepUntil, BondTuning.HornStun, "Horn stun"); Require(fight.Runtime.Weight > 0, "Horn knockback"); Require(fight.Runtime.BlocksAction(fight.Enemies[0]), "Stun blocks pending actions"); }
                    if (id == "sea-shanty" && count >= 2) { Near(fight.Runtime.AttackSpeedBonus, BondTuning.ShantySpeed, "Shanty buff"); fight.Runtime.Tick(BondTuning.ShantySeconds); Near(fight.Runtime.AttackSpeedBonus, 0, "Shanty expires"); }
                }
            var sparse = new BondDefinition { Id = "ember", Steps = new[] { new BondStep { Count = 6 } } };
            var runtime = new BondRuntime(new[] { sparse }, new Dictionary<string, int> { ["ember"] = 8 });
            Require(runtime.Active("ember", 6) && !runtime.Active("ember", 4) && !runtime.Active("ember", 8), "Explicit authored steps gate rules");
            runtime = new BondRuntime(new[] { new BondDefinition { Id = "gear" } }, new Dictionary<string, int> { ["GEAR"] = 4 });
            Require(runtime.Active("gear", 4) && !runtime.Active("gear", 6), "Legacy minor count gates and case-insensitive counts");
        }
        private static void Ember()
        {
            for (int count = 0; count <= 8; count++) using (var fight = new Fight("ember", count))
            {
                fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                fight.Hit(); fight.Hit(); Require(fight.Fired("burn-burst") == 0, "No early burn burst"); fight.Hit();
                Require(fight.Fired("burn-burst") == (count >= 4 ? 1 : 0), "Ember step 4");
                Require(fight.Runtime.State(fight.Enemies[0]).Burn == (count < 2 ? 0 : count < 4 ? BondTuning.BurnStacks : BondTuning.BurnAfterBurst), "Burn cap and consumption");
                if (count >= 4) { Require(fight.Runtime.State(fight.Enemies[1]).Burn == 1, "Burn spreads one stack"); Near(fight.Damage[0].Damage, 10 * BondTuning.BurstDamage, "Burn burst weapon scaling"); }
                int patches = 0; fight.Runtime.EmbersCreated += (position, life) => { patches++; Near(life, BondTuning.EmberSeconds, "Ember lifetime"); };
                fight.Kill(0); Require(patches == (count >= 6 ? 1 : 0), "Ember death step 6");
                Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.Phoenix) == (count >= 8 ? 1 : 0), "Phoenix once at wave start");
            }
            using (var fight = new Fight("ember", 8))
            {
                for (int i = 0; i < 99; i++) fight.Hit(i);
                fight.Runtime.Tick(.1f); Require(fight.Fired("inferno") == 0, "Inferno waits for 100 burning enemies");
                fight.Hit(99);
                Require(fight.Runtime.Heat == BondTuning.InfernoHeat, "Heat counts live burning enemies");
                fight.Damage.Clear(); fight.Runtime.Tick(1); Require(fight.Fired("inferno") == 1, "Inferno at 100 Heat");
                Near(fight.Damage[0].Damage, 10 * BondTuning.BurnDps * BondTuning.InfernoMultiplier, "Inferno triples burn");
                fight.Runtime.Tick(BondTuning.InfernoSeconds); Require(fight.Fired("inferno") == 1, "Inferno once per wave");
                Require(fight.Runtime.Heat == 0, "Burn expires");
            }
            using (var fight = new Fight("ember", 6))
            {
                fight.Hit(); fight.Kill(0); fight.Enemies[1].Position = fight.Enemies[0].Position;
                fight.Runtime.Tick(.1f); Require(fight.Runtime.State(fight.Enemies[1]).Burn > 0, "Crossing embers ignites");
            }
            using (var fight = new Fight("ember", 2))
            {
                fight.Hit(); fight.Damage.Clear(); fight.Runtime.Tick(BondTuning.BurnSeconds + 1);
                Near(fight.Damage.Sum(hit => hit.Damage), 10 * BondTuning.BurnDps * BondTuning.BurnSeconds, "Burn clamps to remaining duration");
            }
        }
        private static void Frost()
        {
            for (int count = 0; count <= 8; count++) using (var fight = new Fight("frost", count))
            {
                int start = count >= 8 ? BondTuning.WinterStacks : 0;
                Require(fight.Runtime.State(fight.Enemies[0]).Chill == start, "Winter initial stacks");
                for (int i = start; i < BondTuning.ChillStacks; i++) fight.Hit();
                Require(fight.Runtime.Frozen(fight.Enemies[0]) == (count >= 4), "Frost freezes at 5 and step 4");
                if (count >= 4)
                {
                    Near(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[0], 10, false), 10 * (1 + BondTuning.FrozenBonus), "Frozen next hit bonus");
                    Near(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[0], 10, false), 10, "Frozen bonus consumed once");
                    fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                }
                fight.Kill(0); Require(fight.Fired("shatter") == (count >= 4 ? 1 : 0), "Frozen death shatters");
                if (count >= 4) Near(fight.Damage.Last().Damage, 10 * BondTuning.ShatterDamage, "Shatter damage");
                for (int i = 1; i < 10; i++) fight.Kill(i);
                Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.IceSoldier) == (count >= 6 ? BondTuning.IceSoldierLimit : 0), "Ice soldiers limited per wave");
                if (count >= 8) { fight.Runtime.Tick(BondTuning.BlizzardPeriod); Require(fight.Fired("blizzard") == 1, "Blizzard cadence"); var late = fight.Enemies[99]; Require(fight.Runtime.Frozen(late), "Blizzard freezes live enemies"); }
            }
            using (var fight = new Fight("frost", 2)) { fight.Hit(); Near(fight.Runtime.SpeedScale(fight.Enemies[0]), 1 - BondTuning.ChillSlow, "Chill speed per stack"); }
        }
        private static void Storm()
        {
            for (int count = 0; count <= 8; count++) using (var fight = new Fight("static-club", count))
            {
                fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                fight.Hero.Health = 49;
                for (int i = 0; i < BondTuning.ThunderHits; i++) fight.Hit();
                Require(fight.Fired("storm-chain") == (count >= 2 ? BondTuning.ThunderHits / BondTuning.StormEvery : 0), "Storm chain cadence");
                Require(fight.Damage.Count(d => d.Rule == "storm-chain") == (count >= 2 ? BondTuning.ThunderHits / BondTuning.StormEvery * BondTuning.ChainTargets : 0), "Chain hits exactly 3 other enemies");
                foreach (var hit in fight.Damage.Where(d => d.Rule == "storm-chain")) Near(hit.Damage, 10 * BondTuning.ChainDamage, "Chain damage");
                Require(fight.Fired("discharge") == (count >= 4 ? BondTuning.ThunderHits / BondTuning.DischargeStacks : 0), "Discharge at five charges");
                foreach (var hit in fight.Damage.Where(d => d.Rule == "discharge")) Near(hit.Damage, 10 * BondTuning.DischargeDamage, "Discharge damage");
                Require(fight.Fired("thunderclap") == (count >= 6 ? 1 : 0), "Underdog twenty-hit streak");
                Near(fight.Runtime.CriticalBonus, count >= 6 ? BondTuning.UnderdogBonus : 0, "Underdog crit");
                Near(fight.Runtime.LuckBonus, count >= 6 ? BondTuning.UnderdogBonus : 0, "Underdog luck");
                fight.Hero.Health = 50; Near(fight.Runtime.CriticalBonus, 0, "Underdog strictly below 50%");
                fight.Runtime.Tick(BondTuning.CloudPeriod); Require(fight.Fired("storm-cloud") == (count >= 8 ? 1 : 0), "Storm cloud period");
            }
            using (var fight = new Fight("static-club", 6))
            {
                fight.Hero.Health = 49;
                for (int i = 0; i < BondTuning.ThunderHits - 1; i++) fight.Hit();
                fight.Hero.Health = 50; fight.Runtime.Tick(.1f); fight.Hero.Health = 49; fight.Hit();
                Require(fight.Fired("thunderclap") == 0, "Healing breaks Underdog streak");
            }
        }
        private static void Venom()
        {
            for (int count = 0; count <= 8; count++) using (var fight = new Fight("venom", count))
            {
                fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                for (int i = 0; i < 12; i++) fight.Hit();
                Require(fight.Runtime.State(fight.Enemies[0]).Poison == (count >= 2 ? BondTuning.PoisonHitCap : 0), "Poison direct hit cap");
                for (int i = 0; i < 5; i++) fight.Hit(1);
                fight.Kill(0);
                Require(fight.Runtime.State(fight.Enemies[1]).Poison == (count >= 4 ? 10 : count >= 2 ? 5 : 0), "Death spread can reach Rot");
                fight.Damage.Clear(); fight.Runtime.Tick(1);
                float expected = count >= 2 ? 10 * (count >= 4 ? 10 : 5) * BondTuning.PoisonDps : 0;
                if (count >= 6) expected += fight.Enemies[1].Definition.Health * BondTuning.RotMaxHealthDps;
                Near(fight.Damage.Sum(d => d.Damage), expected, "Poison and Rot damage per second");
                if (count >= 8)
                {
                    fight.Enemies[2].Position = fight.Enemies[1].Position + Vector2.right;
                    fight.Runtime.Tick(1); Require(fight.Runtime.State(fight.Enemies[2]).Poison > 0, "Plague spreads autonomously");
                    Near(fight.Runtime.SpeedScale(fight.Enemies[2]), 1 - BondTuning.PlagueSlow, "Plague slow");
                }
            }
            using (var fight = new Fight("venom", 6))
            {
                fight.Hit(); BondEnemyState state = fight.Runtime.State(fight.Enemies[0]);
                state.Poison = BondTuning.RotStacks - 1; fight.Damage.Clear(); fight.Runtime.Tick(1);
                Near(fight.Damage.Single().Damage, 10 * (BondTuning.RotStacks - 1) * BondTuning.PoisonDps, "No Rot at seven stacks");
                state.Poison = BondTuning.RotStacks; fight.Damage.Clear(); fight.Runtime.Tick(1);
                Near(fight.Damage.Single().Damage, 10 * BondTuning.RotStacks * BondTuning.PoisonDps + fight.Enemies[0].Definition.Health * BondTuning.RotMaxHealthDps, "Rot starts at eight stacks");
            }
        }
        private static void Dusk()
        {
            for (int count = 0; count <= 8; count++) using (var fight = new Fight("dusk", count))
            {
                int steals = 0; fight.Runtime.StrengthStolen += (enemy, friend, fraction) => { steals++; Require(friend == fight.Friend, "Strongest friend receives strength"); Near(fraction, BondTuning.StrengthSteal, "Steal fraction"); };
                for (int i = 0; i < 12; i++) fight.Hit(i);
                Require(steals == (count >= 2 ? BondTuning.DuskMarks : 0), "Dusk first ten distinct targets");
                Near(fight.Runtime.OutgoingScale(fight.Enemies[0]), count >= 2 ? 1 - BondTuning.StrengthSteal : 1, "Marked enemy weakened");
                if (count >= 2) Near(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[0], 10, false), 10 + 10 * BondTuning.DuskMarks * BondTuning.StrengthSteal, "Strength given to best friend");
                fight.Enemies[0].Health = fight.Enemies[0].Definition.Health * BondTuning.ExecuteHealth - 1;
                fight.Hit(); Require(fight.Damage.Any(d => d.Rule == "execute") == (count >= 6), "Execute threshold");
                fight.Damage.Clear(); fight.Runtime.CheckExecute(fight.Enemies[0], null);
                Require(fight.Damage.Any(d => d.Rule == "execute") == (count >= 6), "Execute also handles generated damage");
                fight.Kill(0); fight.Runtime.OnKill(fight.Enemies[0]);
                Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.Shade) == (count >= 4 ? 1 : 0), "Shade once per marked death");
                fight.Runtime.Tick(BondTuning.EclipseStart);
                Require(fight.Damage.Count(d => d.Rule == "eclipse") == (count >= 8 ? Mathf.CeilToInt(99 * BondTuning.EclipseFraction) : 0), "Eclipse weakest five percent");
            }
        }
        private static void MinorState()
        {
            for (int count = 0; count <= 8; count++)
            {
                using (var fight = new Fight("mask", count))
                {
                    int every = count >= 6 ? BondTuning.MaskSixEvery : count >= 4 ? BondTuning.MaskFourEvery : BondTuning.MaskEvery;
                    for (int i = 1; i <= 30; i++) { Near(fight.Hit(0, false), 10, "Noncrit does not consume Mask"); Near(fight.Hit(0, true), count >= 2 && i % every == 0 ? 10 * BondTuning.MaskMultiplier : 10, "Mask Nth crit " + count + "/" + i); }
                }
                using (var fight = new Fight("big-top", count))
                {
                    for (int i = 0; i < 30; i++) fight.Hit(i);
                    float bonus = count < 2 ? 0 : count >= 4 ? BondTuning.ComboHighCap : BondTuning.ComboCap;
                    Near(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[29], 10, false), 10 * (1 + bonus), "Combo cap");
                    fight.Hit(29); Near(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[29], 10, false), 10, "Repeated enemy breaks combo");
                }
                using (var fight = new Fight("gauntlet", count))
                {
                    for (int i = 0; i < 30; i++) fight.Hit();
                    float bonus = count < 2 ? 0 : count >= 4 ? BondTuning.GritHighCap : BondTuning.GritCap;
                    Near(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[0], 10, false), 10 * (1 + bonus), "Grit cap");
                    fight.Runtime.Tick(BondTuning.GritIdle);
                    Near(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[0], 10, false), 10 * (1 + (count >= 6 ? bonus : 0)), "Grit idle reset and step 6 retention");
                }
                using (var fight = new Fight("root", count, "lantern-guild"))
                {
                    fight.Runtime.Tick(BondTuning.StillSeconds - .1f); Near(fight.Runtime.RangeBonus, 0, "Stillness waits two seconds");
                    fight.Runtime.Tick(.1f);
                    Near(fight.Runtime.RangeBonus, count >= 2 ? BondTuning.LanternBonus : 0, "Lantern stillness range");
                    Near(fight.Runtime.RegenBonus, count >= 4 ? BondTuning.RootHighRegen : count >= 2 ? BondTuning.RootRegen : 0, "Root regeneration");
                    Near(fight.Runtime.ArmorBonus, count >= 4 ? BondTuning.RootHighArmor : count >= 2 ? BondTuning.RootArmor : 0, "Root armor");
                    float lit = count >= 8 ? BondTuning.LighthouseBonus : count >= 4 ? BondTuning.LitBonus : 0;
                    Near(fight.Runtime.ModifyHit(fight.Hero, fight.Enemies[0], 10, false), 10 * (1 + (count >= 2 ? BondTuning.LanternBonus : 0)) * (1 + lit), "Lantern damage and aura");
                    Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.LampWisp) == (count >= 6 ? 1 : 0), "Lantern wisp at six");
                    fight.Runtime.Moving = true; fight.Runtime.Tick(.1f); Near(fight.Runtime.ArmorBonus, 0, "Moving clears Root"); Near(fight.Runtime.RangeBonus, 0, "Moving clears Lantern");
                }
                using (var fight = new Fight("clover", count, "sweet-tooth"))
                {
                    for (int i = 0; i < 100; i++) fight.Kill(i);
                    Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.Clover) == (count >= 2 ? 100 / (count >= 4 ? BondTuning.CloverFourEvery : BondTuning.CloverEvery) : 0), "Clover drop cadence");
                    Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.Snack) == (count >= 2 ? 100 / (count >= 4 ? BondTuning.SnackFourEvery : BondTuning.SnackEvery) : 0), "Snack drop cadence");
                    foreach (BondSpawnRequest spawn in fight.Spawns)
                    { if (spawn.Kind == BondSpawnKind.Snack) { Near(spawn.HealMaxHealthFraction, BondTuning.SnackHeal, "Snack heals on collection"); Require(spawn.Coins == (count >= 6 ? 1 : 0), "Snack coin at six"); } else Require(spawn.FreeRerolls == 1, "Clover grants free reroll on collection"); }
                }
            }
        }
        private static void Resets()
        {
            using (var fight = new Fight("ember", 8, "venom", "static-club", "frost", "mask", "root", "dusk"))
            {
                fight.Hit(); fight.Runtime.OnCast(fight.Friend); fight.Runtime.OnCast(fight.Friend);
                Require(fight.Runtime.Ticks == 2 && fight.Runtime.Applause == 1, "Applause counts distinct casts");
                fight.Runtime.OnHeal(5); fight.Runtime.OnVolley(); fight.Runtime.OnSoak(); fight.Runtime.OnRecord();
                fight.Runtime.Tick(BondTuning.ApplauseWindow + .1f); Require(fight.Runtime.Applause == 0, "Applause expires");
                fight.Reset();
                Require(fight.Runtime.SpawnRequests.Count == 1 && fight.Runtime.SpawnRequests[0].Kind == BondSpawnKind.Phoenix, "Startup requests buffered and replaced on reset");
                fight.Runtime.ClearSpawnRequests(); Require(fight.Runtime.SpawnRequests.Count == 0, "Arena can acknowledge spawn requests");
                Require(fight.Runtime.Heat == 0 && fight.Runtime.Toxin == 0 && fight.Runtime.Charge == 0 && fight.Runtime.Warmth == 0 && fight.Runtime.Ticks == 0 && fight.Runtime.Volleys == 0 && fight.Runtime.Soak == 0 && fight.Runtime.Records == 0 && fight.Runtime.Wishes == 0 && fight.Runtime.Weight == 0, "All wave counters reset");
                Require(fight.Runtime.Chill == 100 && fight.Runtime.Rank == 3, "Winter and Rank recomputed on reset");
                Require(!fight.Runtime.State(fight.Enemies[0]).Marked, "Dusk marks reset");
                var newEnemy = new UnitState { Id = 1000, Faction = Faction.Enemy, Definition = fight.Enemies[0].Definition, Health = 100 };
                fight.Runtime.RegisterEnemy(newEnemy); Require(fight.Runtime.State(newEnemy).Chill == BondTuning.WinterStacks, "Late spawns start chilled");
            }
        }
    }
}
