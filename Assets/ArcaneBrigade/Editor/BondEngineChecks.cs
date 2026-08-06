using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Deterministic design v5 checks with 100-enemy fixtures and an unrelated hero.</summary>
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
                UnitDefinition heroDefinition = Asset<UnitDefinition>(); heroDefinition.Weapon = weapon; heroDefinition.Health = 100;
                UnitDefinition enemy = Asset<UnitDefinition>(); enemy.Weapon = weapon; enemy.Health = 10000;
                Hero = new UnitState { Id = 1, Faction = Faction.Champion, Definition = heroDefinition, Health = 100 };
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
            CatalogAndThresholds(); SignatureStats(); MemberScope(); StackRules(); ResourcePayoffs(); ChangedRuleRegressions(); ResetState();
            Debug.Log("BondEngineChecks: PASS (design v5 stats, scope, resources and 100-enemy fixtures)");
        }
        private static void CatalogAndThresholds()
        {
            BondDefinition[] definitions = BondV3Catalog.CreateDefinitions();
            Require(definitions.Length == BondTuning.MajorCount + BondTuning.MinorCount, "52 bonds");
            var counts = new Dictionary<string, int>(); int roster = 0;
            foreach (string id in BondV3Catalog.CompanionIds)
            {
                roster++; var unique = new HashSet<string>();
                foreach (string tag in BondV3Catalog.Tags(id))
                { Require(unique.Add(tag), "No duplicate tag for " + id); counts[tag] = counts.TryGetValue(tag, out int n) ? n + 1 : 1; }
            }
            Require(roster == BondTuning.RosterCount, "120 companions");
            foreach (BondDefinition definition in definitions)
            {
                Require(counts.TryGetValue(definition.Id, out int total), "Every bond has members");
                bool major = BondV3Catalog.IsMajor(definition.Id);
                Require(major ? total == 9 : total == 5 || total == 7, "Membership for " + definition.Id);
                for (int count = 0; count <= 10; count++)
                {
                    var runtime = new BondRuntime(definitions, new Dictionary<string, int> { [definition.Id] = count });
                    foreach (int step in new[] { 2, 4, 6, 8 })
                        Require(runtime.Active(definition.Id, step) == (count >= step && (major || step <= (total == 7 ? 6 : 4))), "Authored threshold " + definition.Id + "/" + count + "/" + step);
                }
                Near(definition.Reward(total), 0, "Legacy team stat package removed");
                foreach (BondStep step in definition.Steps) Near(step.Value, 0, "Steps are rule gates, not old team stats");
            }
        }
        private static void SignatureStats()
        {
            var coverage = new HashSet<string>();
            foreach (BondV5StatLine line in BondTuning.V5Stats)
            {
                if (line.Members) coverage.Add(line.Id);
                Near(line.At(1), 0, "No stat below first step");
                for (int i = 0; i < line.Values.Length; i++)
                {
                    int step = (i + 1) * 2;
                    Near(line.At(step), line.Values[i], "Stat at exact threshold");
                    Near(line.At(step + 1), line.Values[i], "Odd counts retain current stat");
                    if (i > 0) Require(line.Values[i] >= line.Values[i - 1], "Stats grow with depth");
                }
                if (!line.Members)
                {
                    Require(BondV3Catalog.IsMajor(line.Id), "Minor stats never leak to team");
                    Near(line.At(4), 0, "Team shares start at six");
                }
            }
            Require(coverage.Count == 52, "Every bond grants signature stats");
            Near(BondTuning.Stat("ember", ItemEffect.Damage, 2, true), .08f, "Ember member at two");
            Near(BondTuning.Stat("ember", ItemEffect.Damage, 4, false), 0, "No team signature at four");
            Near(BondTuning.Stat("ember", ItemEffect.Damage, 6, true), .36f, "Member adds team share at six");
            Near(BondTuning.Stat("ember", ItemEffect.Damage, 8, false), .12f, "Team share at eight");
            Near(BondTuning.Stat("frost", ItemEffect.Armor, 8, true), 19, "Frost points include team share");
            Near(BondTuning.Stat("stone", ItemEffect.Armor, 6, true), 14, "Stone signature armor");
            Near(BondTuning.Stat("bulwark", ItemEffect.Health, 4, true), .12f, "Bulwark secondary signature");
            Near(BondTuning.Stat("tidal", ItemEffect.SkillHaste, 8, true), .45f, "Tide member haste");
            Near(BondTuning.Stat("tidal", ItemEffect.Health, 8, false), .12f, "Tide team health");
            Near(BondTuning.Stat("hearth", ItemEffect.AttackSpeed, 8, true), 1, "Kitchen doubles Hearth signature and share");
            Near(BondTuning.Stat("root", ItemEffect.Armor, 4, true), 8, "Root member armor");
            Near(BondTuning.Stat("shield", ItemEffect.Armor, 6, true), 10, "Shield member armor");
            Near(BondTuning.Stat("bastion", ItemEffect.Armor, 4, true), 10, "Bastion member armor");
            Near(BondTuning.Stat("spiky-shell", ItemEffect.Armor, 4, false), 0, "Minor stats stay on members");
            var counts = new Dictionary<string, int> { ["ember"] = 6, ["bastion"] = 4 };
            Near(BondV3Catalog.Stat(ItemEffect.Damage, counts, new[] { "bastion" }), .06f, "Unrelated member receives team share");
            Near(BondV3Catalog.Stat(ItemEffect.Armor, counts, new[] { "bastion" }), 10, "Hero can count as a minor member");
            Near(BondV3Catalog.Stat(ItemEffect.Armor, counts, Array.Empty<string>()), 0, "Unrelated hero gets no minor signature");
        }
        private static void MemberScope()
        {
            foreach (string id in new[] { "ember", "frost", "venom", "tidal" })
                foreach (int step in new[] { 2, 4 }) using (var fight = new Fight(id, step))
                {
                    int hits = id == "tidal" ? BondTuning.TideEvery : 1;
                    for (int i = 0; i < hits; i++) fight.Hit(attacker: fight.Hero);
                    BondEnemyState state = fight.Runtime.State(fight.Enemies[0]);
                    bool applied = id == "ember" ? state.Burn > 0 : id == "frost" ? state.Chill > 0 : id == "venom" ? state.Poison > 0 : state.SoakedUntil > 0;
                    Require(applied == (step >= 4), id + " step two members, step four whole team");
                    for (int i = 0; i < hits; i++) fight.Hit();
                    state = fight.Runtime.State(fight.Enemies[0]);
                    Require(id == "ember" ? state.Burn > 0 : id == "frost" ? state.Chill > 0 : id == "venom" ? state.Poison > 0 : state.SoakedUntil > 0, id + " member rule remains active");
                }
            using (var fight = new Fight("hearth", 2))
            {
                Require(fight.Runtime.ModifyHit(fight.Friend, fight.Enemies[0], 10, false) > 10, "Meal buffs member");
                Near(fight.Runtime.ModifyHit(fight.Hero, fight.Enemies[0], 10, false), 10, "Meal does not buff unrelated hero");
            }
        }
        private static void StackRules()
        {
            using (var fight = new Fight("ember", 4))
            {
                fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                fight.Hit(); fight.Hit(); Require(fight.Fired("burn-burst") == 0, "Burn builds before burst"); fight.Hit();
                Require(fight.Fired("burn-burst") == 1, "Three stacks burst");
                Require(fight.Runtime.Heat > 0, "Bursts build Heat");
                Require(fight.Runtime.State(fight.Enemies[1]).Burn > 0, "Burst spreads Burn");
                int bursts = fight.Fired("burn-burst");
                for (int i = 0; i < 6; i++) fight.Hit();
                Require(fight.Fired("burn-burst") == bursts, "Burst cooldown prevents same-frame loops");
            }
            using (var fight = new Fight("venom", 4))
            {
                fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                fight.Hit(); fight.Kill(0);
                Require(fight.Runtime.State(fight.Enemies[1]).Poison >= 2, "Death spreads stacks plus one");
            }
            using (var fight = new Fight("frost", 4))
            {
                for (int i = 0; i < BondTuning.ChillStacks; i++) fight.Hit();
                Require(fight.Runtime.Frozen(fight.Enemies[0]), "Chill builds to Freeze");
                fight.Enemies[1].Position = fight.Enemies[0].Position + Vector2.right;
                fight.Hit();
                Require(fight.Fired("shatter") > 0, "Next hit shatters");
                Require(fight.Runtime.State(fight.Enemies[1]).Chill >= 2, "Shards feed neighbouring Chill");
            }
        }
        private static void ResourcePayoffs()
        {
            using (var fight = new Fight("ember", 8))
            {
                Require(!fight.Spawns.Any(s => s.Kind == BondSpawnKind.Phoenix), "Phoenix requires Heat");
                for (int i = 0; i < BondV5Tuning.PhoenixHeat - 1; i++)
                    for (int hit = 0; hit < BondV5Tuning.HighBurnStacks; hit++) fight.Hit(i);
                fight.Runtime.Tick(.01f);
                Require(!fight.Spawns.Any(s => s.Kind == BondSpawnKind.Phoenix), "No Phoenix below fifty Heat");
                for (int hit = 0; hit < BondV5Tuning.HighBurnStacks; hit++) fight.Hit(BondV5Tuning.PhoenixHeat - 1);
                fight.Runtime.Tick(.01f);
                Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.Phoenix) == 1, "Fifty Heat hatches Phoenix");
                fight.Runtime.Tick(.01f);
                Require(fight.Spawns.Count(s => s.Kind == BondSpawnKind.Phoenix) == 1, "Phoenix hatches once per wave");
            }
            foreach (int step in new[] { 2, 4, 6 }) using (var fight = new Fight("sweet-tooth", step))
            {
                for (int i = 0; i < BondV5Tuning.SugarCap - 1; i++) fight.Runtime.OnPickup(BondSpawnKind.Snack);
                Require(fight.Runtime.Sugar == BondV5Tuning.SugarCap - 1, "Snacks build Sugar");
                Require(fight.Fired("sugar-rush") == 0, "Sugar payoff waits for full resource");
                fight.Runtime.OnPickup(BondSpawnKind.Snack);
                Require(fight.Runtime.Sugar == (step >= 4 ? 0 : BondV5Tuning.SugarCap), "Upper step spends Sugar");
                Require(fight.Fired("sugar-rush") == (step >= 4 ? 1 : 0), "Sugar Rush payoff");
                if (step >= 4) Require(fight.Runtime.SkillFillBonusFor(fight.Friend) > 0, "Sugar Rush fills time-based bars faster");
                fight.Reset(); Require(fight.Runtime.Sugar == 0, "Sugar resets each wave");
            }
            using (var fight = new Fight("hearth", 6))
            {
                for (int i = 0; i < 4; i++) fight.Runtime.OnPickup(BondSpawnKind.Dish);
                Near(fight.Runtime.Warmth, BondV5Tuning.WarmthCap, "Dish Warmth has a cap");
                fight.Runtime.OnOverheal(fight.Hero, 10);
                Require(fight.Runtime.SupportRequests.Any(r => r.Rule == "warmth-overflow"), "Full Warmth turns overheal into shields");
            }
        }
        private static void ChangedRuleRegressions()
        {
            using (var fight = new Fight("hearth", 6))
            {
                fight.Runtime.Tick(BondTuning.FeastPeriod);
                Require(fight.Runtime.SupportRequests.Any(r => r.Kind == BondSupportKind.SkillFill && r.Rule == "feast-fill"), "Every step-six Feast fills bars before ten Warmth");
            }
            using (var fight = new Fight("archive", 2))
            {
                fight.Hit(attacker: fight.Hero);
                Require(fight.Runtime.Records == 0, "Archive two records only its members' hits");
                fight.Hit(); Require(fight.Runtime.Records == 1, "Archive member records species");
                Near(fight.Runtime.RunDamageBonus, 0, "Archive two does not grant permanent growth");
            }
            using (var fight = new Fight("archive", 4))
            {
                fight.Hit(attacker: fight.Hero);
                Near(fight.Runtime.RunDamageBonus, BondTuning.RecordGrowth, "New species grants run growth exactly once");
                fight.Hit(); Near(fight.Runtime.RunDamageBonus, BondTuning.RecordGrowth, "Repeated species does not grow again");
            }
            using (var fight = new Fight("regal", 8))
            {
                BondSupportRequest choice = fight.Runtime.SupportRequests.First(r => r.Rule == "court-edict");
                Near(choice.Amount, 4, "Court six offers four edicts");
            }
            using (var fight = new Fight("sea-shanty", 2))
            {
                for (int i = 0; i < 4; i++) fight.Hit();
                Near(fight.Runtime.AttackSpeedBonusFor(fight.Friend), .03f, "Four attacks build a Verse");
                for (int i = 0; i < 4; i++) fight.Hit();
                Near(fight.Runtime.AttackSpeedBonusFor(fight.Friend), .06f, "Verses stack");
            }
        }
        private static void ResetState()
        {
            using (var fight = new Fight("ember", 8, "venom", "frost"))
            {
                fight.Hit(); fight.Runtime.OnCast(fight.Friend); fight.Runtime.OnHeal(5);
                fight.Reset();
                Require(fight.Runtime.Heat == 0 && fight.Runtime.Toxin == 0 && fight.Runtime.Warmth == 0 && fight.Runtime.Ticks == 0, "Wave resources reset");
                Require(!fight.Spawns.Any(s => s.Kind == BondSpawnKind.Phoenix), "Phoenix waits for Heat");
                var late = new UnitState { Id = 1000, Faction = Faction.Enemy, Definition = fight.Enemies[0].Definition, Health = 100 };
                fight.Runtime.RegisterEnemy(late); Require(fight.Runtime.State(late).Chill == BondTuning.WinterStacks, "Late spawns receive winter");
                fight.Runtime.ClearSpawnRequests(); Require(fight.Runtime.SpawnRequests.Count == 0, "Spawn buffer can be acknowledged");
            }
        }
    }
}
