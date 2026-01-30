using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    [Serializable] public sealed class AchievementMetric { public string Id; public float Value; }
    public sealed class JourneyAchievement
    {
        public string Id, Name, Description, Metric;
        public float Goal;
    }
    public sealed partial class RunSnapshot
    {
        public List<AchievementMetric> AchievementEvidence = new List<AchievementMetric>();
    }
    public static class RunAchievements
    {
        public static List<JourneyAchievement> Definitions(RunCatalog catalog)
        {
            var entries = new List<JourneyAchievement>();
            void Add(string id, string name, string description, string metric, float goal) => entries.Add(new JourneyAchievement { Id = "journal-" + id, Name = name, Description = description, Metric = metric, Goal = goal });
            foreach (UnitDefinition u in catalog.Champions)
            {
                Add("dawn-" + u.Id, u.DisplayName + " / First dawn", "Finish a normal journey with " + u.DisplayName + ".", "win-" + u.Id, 1);
                Add("master-" + u.Id, u.DisplayName + " / Night keeper", "Finish Sleepless or harder with " + u.DisplayName + ".", "master-" + u.Id, 1);
            }
            for (int i = 0; i < RunMeta.DifficultyNames.Length; i++) Add("difficulty-" + i, RunMeta.DifficultyNames[i] + " / Dawn", "Finish a normal journey at this difficulty or harder.", "difficulty", i + 1);
            int[] scores = { 10000, 50000, 100000, 250000, 500000, 1000000 };
            foreach (int goal in scores) Add("score-" + goal, "Receipt worth " + goal.ToString("N0"), "Earn this score in one journey. Endless counts.", "score", goal);
            foreach (int goal in new[] { 200, 500, 800 }) Add("gold-" + goal, "A purse of " + goal, "Collect this much combat gold in one wave.", "battle-gold", goal);
            // Stable IDs preserve earned records while the new goals fit the fifteen-body cap.
            foreach (int previousGoal in new[] { 10, 15, 20 })
            {
                int goal = previousGoal == 10 ? 8 : previousGoal == 15 ? 12 : 15;
                Add("party-" + previousGoal, goal + " names on the ticket", "Own this many companions at one time.", "party", goal);
            }
            Add("health", "A well stitched heart", "Reach 120 maximum HP.", "health", 120);
            Add("fragile", "One last thread", "Enter combat with at most 1 maximum HP.", "one-health", 1);
            Add("armor", "Porcelain fortress", "Reach 0.75 armor before the incoming damage cap.", "armor", .75f);
            Add("regen", "Warm tea forever", "Reach 5 passive HP regeneration per second.", "regen", 5);
            Add("reach", "Across the counter", "Reach 5 units of compiled champion weapon range.", "range", 5);
            Add("projectiles", "A pocket of stars", "Reach 6 compiled projectiles.", "projectiles", 6);
            Add("crit", "Every seam exposed", "Reach 100% critical chance.", "crit", 1);
            Add("damage", "Two promises", "Reach +100% champion damage before diminishing returns.", "damage", 1);
            Add("luck", "A four leaf receipt", "Reach +50% luck.", "luck", .5f);
            Add("dodge", "Between the raindrops", "Reach 60% dodge before the dodge cap.", "dodge", .6f);
            Add("tempo", "A quicker clock", "Reach +70% attack speed before diminishing returns.", "tempo", .7f);
            Add("speed", "Wind in the ribbons", "Reach 6.5 compiled movement speed.", "speed", 6.5f);
            Add("income", "The shop pays you", "Reach 80 wave income.", "income", 80);
            Add("score-bonus", "A signed receipt", "Reach +20% score multiplier.", "score-bonus", .2f);
            Add("body", "Room to grow", "Increase champion body size by at least 20%.", "body", 1.2f);
            Add("wave12", "Still open at midnight", "Clear wave 12.", "cleared", 12);
            Add("wave30", "The long staircase", "Clear endless wave 30.", "endless-cleared", 30);
            Add("pony", "A seat for a friend", "Enter combat with a Pony and another ally.", "rider", 1);
            Add("nursery", "Your first leaf", "Buy a nursery Sprout.", "sprouts", 1);
            Add("memory", "A room of borrowed voices", "Absorb 8 companion memories in one run.", "memories", 8);
            Add("keeper", "A hand released", "Defeat the Outside Keeper.", "keeper", 1);
            Add("farewell", "Leave a light", "Complete a nightmare farewell.", "farewell", 1);
            Add("three-star", "One weapon remembered", "Own a three star companion.", "three-star", 1);
            Add("bonds", "A well woven brigade", "Activate 5 different bonds together.", "bonds", 5);
            return entries;
        }
        public static void Mark(List<AchievementMetric> evidence, string id, float value)
        {
            if (!float.IsFinite(value) || value < 0) return;
            AchievementMetric metric = evidence.Find(m => m.Id == id);
            if (metric == null) evidence.Add(new AchievementMetric { Id = id, Value = value }); else metric.Value = Mathf.Max(metric.Value, value);
        }
        public static float Progress(List<AchievementMetric> evidence, JourneyAchievement definition) => Mathf.Min(definition.Goal, evidence?.Find(m => m.Id == definition.Metric)?.Value ?? 0);
        public static void Observe(RunSession session, RunCatalog catalog, CheckpointCatalog compiled)
        {
            RunSnapshot r = session.State; r.AchievementEvidence ??= new List<AchievementMetric>();
            void Set(string id, float value) => Mark(r.AchievementEvidence, id, value);
            Set("party", r.Roster.Count); Set("sprouts", r.LeafPurchases); Set("memories", r.AbsorbedSkills?.Count ?? 0);
            Set("three-star", r.Roster.Exists(e => e.Tier == 3) ? 1 : 0);
            Set("rider", r.Roster.Count > 1 && r.Roster.Exists(e => catalog.Unit(e.UnitId).ReferenceCompanion == "Pony") ? 1 : 0);
            Set("health", compiled.Champion.Health); Set("one-health", compiled.Champion.Health <= 1 ? 1 : 0);
            Set("armor", session.Bonus(ItemEffect.Armor)); Set("regen", compiled.PassiveHeal); Set("range", compiled.Champion.Weapon.Range);
            Set("projectiles", compiled.Champion.Weapon.ProjectileCount); Set("crit", compiled.Champion.CriticalChance);
            Set("damage", session.Bonus(ItemEffect.Damage, ModifierScope.Champion) + session.Bonus(ItemEffect.ChampionDamage, ModifierScope.Champion));
            Set("luck", session.Bonus(ItemEffect.Luck)); Set("dodge", session.Bonus(ItemEffect.Dodge)); Set("tempo", session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion));
            Set("speed", compiled.Champion.Speed); Set("income", r.LastIncome); Set("score-bonus", session.Bonus(ItemEffect.ScoreMultiplier));
            Set("body", compiled.Champion.BodyHeight / Mathf.Max(.01f, catalog.Unit(r.ChampionId).BodyHeight));
            int bonds = 0; foreach (BondDefinition b in catalog.Bonds) if (session.BondCounts.TryGetValue(b.Id, out int count) && b.ActiveLevel(count) > 0) bonds++;
            Set("bonds", bonds);
        }
        public static void Credit(PlayerProfile profile, RunSnapshot run, RunCatalog catalog)
        {
            if (catalog == null) return;
            profile.AchievementEvidence ??= new List<AchievementMetric>();
            foreach (AchievementMetric m in run.AchievementEvidence ?? new List<AchievementMetric>()) Mark(profile.AchievementEvidence, m.Id, m.Value);
            void Set(string id, float value) => Mark(profile.AchievementEvidence, id, value);
            bool won = run.Phase == SessionPhase.Victory && !run.Endless;
            if (won) { Set("win-" + run.ChampionId, 1); Set("difficulty", run.Difficulty + 1); if (run.Difficulty >= 4) Set("master-" + run.ChampionId, 1); }
            int cleared = run.Phase == SessionPhase.Shop || run.Phase == SessionPhase.Victory ? run.Wave : run.Wave - 1;
            Set("cleared", cleared); if (run.Endless) Set("endless-cleared", cleared);
            Set("score", RunMeta.Score(run)); Set("keeper", run.OutsideStage == OutsideStageStatus.Reward || run.OutsideStage == OutsideStageStatus.Completed ? 1 : 0);
            Set("farewell", run.Nightmare?.Ending == NightmareEndingStage.Complete ? 1 : 0);
            foreach (JourneyAchievement d in Definitions(catalog)) if (Progress(profile.AchievementEvidence, d) >= d.Goal) RunMeta.Add(profile.Achievements, d.Id);
        }
    }
}
