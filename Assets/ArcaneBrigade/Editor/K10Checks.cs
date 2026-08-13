using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K10Checks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K10: " + caption); notes.Add("PASS " + caption); };
            RunCatalog original = Resources.Load<RunCatalog>("RunCatalog");
            var catalog = UnityEngine.Object.Instantiate(original);
            catalog.StatRevision = 1;
            catalog.Companions = original.Companions.Select(UnityEngine.Object.Instantiate).ToArray();
            catalog.Champions = original.Champions.Select(UnityEngine.Object.Instantiate).ToArray();
            try
            {
                foreach (UnitDefinition unit in catalog.Companions) unit.CompanionModifiers = Array.Empty<StatModifier>();
                foreach (UnitDefinition unit in catalog.Champions) unit.ChampionModifiers = unit.ChampionModifiers.Where(modifier => modifier.Effect < ItemEffect.ScoreMultiplier).ToArray();
                foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
                    if (effect >= ItemEffect.ScoreMultiplier) require(RunModifierRules.ModifierName(effect) != "Invalid effect", "Localized stat " + effect);
                var item = new ItemDefinition { Id = "k10-fixture", Modifiers = new[]
                {
                    Modifier(ItemEffect.BulletPierce, 3), Modifier(ItemEffect.BulletSplit, 2), Modifier(ItemEffect.BulletSize, 0.5f), Modifier(ItemEffect.BulletBounce, 0.2f), Modifier(ItemEffect.BulletSpeed, 0.5f), Modifier(ItemEffect.AttackRange, 0.25f), Modifier(ItemEffect.RangedDamage, 4), Modifier(ItemEffect.Dodge, 0.15f), Modifier(ItemEffect.Income, 5), Modifier(ItemEffect.BodySize, 0.2f), Modifier(ItemEffect.HeartHealing, 3), Modifier(ItemEffect.ChestQuality, 0.2f), Modifier(ItemEffect.ScoreMultiplier, 0.2f)
                } };
                catalog.Items = original.Items.Concat(new[] { item }).ToArray();
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 10101);
                    session.State.Items.Add(new ItemStack { Id = item.Id, Count = 2 });
                    CheckpointCatalog combat = session.PrepareCombat();
                    WeaponDefinition raw = catalog.Weapon(session.State.Roster[0].WeaponId);
                    WeaponDefinition actual = combat.StartingCompanions[0].Weapon;
                    require(actual.ProjectilePierce == raw.ProjectilePierce + 6, "Pierce derives from stacked run modifiers");
                    require(actual.ProjectileSplit == 4, "Split derives from stacked run modifiers");
                    require(Mathf.Approximately(actual.ProjectileRadius, raw.ProjectileRadius * 2), "Bullet collision size grows with visual size");
                    require(Mathf.Approximately(actual.ProjectileVisualWidth, raw.ProjectileVisualWidth * 2), "Bullet visual width matches same factor");
                    require(Mathf.Approximately(actual.ProjectileBounceChance, 0.4f), "Bounce is a probability rather than a fixed count");
                    require(Mathf.Approximately(actual.ProjectileSpeed, raw.ProjectileSpeed * 2), "Projectile speed modifier applied");
                    require(Mathf.Approximately(actual.ProjectileLifetime, Mathf.Min(raw.ProjectileLifetime * 1.5f / 2, 9 / Mathf.Max(1, actual.ProjectileSpeed))), "Range increases flight distance without speed double-counting (within the 9-unit flight cap)");
                    require(Mathf.Approximately(combat.Dodge, 0.3f), "Dodge compiled into simulation catalog");
                    require(Mathf.Approximately(combat.HeartHealing, 6), "Heart bonus compiled into simulation catalog");
                    require(Mathf.Approximately(combat.Champion.Radius, catalog.Champions[0].Radius * 1.4f), "Body scale changes actual hitbox as well as render");
                    require(actual.Damage > raw.Damage, "Flat ranged damage participates in actual weapon damage");
                    var splitSimulation = new CombatSimulation(combat); splitSimulation.Start(combat.Seed);
                    UnitState splitTarget = splitSimulation.AddFixture(combat.MeleeEnemy, Faction.Enemy, new Vector2(1, 0)); splitTarget.Health = 1000;
                    ProjectileState splitProjectile = splitSimulation.K8CombatProjectileFixture(splitSimulation.Champion, Vector2.zero, Vector2.right * 20, 5, split: true);
                    splitProjectile.SplitCount = 4;
                    splitSimulation.Step(0.1f, Vector2.zero);
                    require(splitSimulation.Projectiles.Count(projectile => projectile.IsFragment) == 4, "Actual swept impact creates configured four split children");
                    require(splitSimulation.Projectiles.Where(projectile => projectile.IsFragment).All(projectile => !projectile.SplitOnImpact && projectile.SplitCount == 0), "Split descendants never recursively split");
                    float lifetime = splitProjectile.Remaining;
                    require(splitSimulation.Projectiles.Where(projectile => projectile.IsFragment).All(projectile => projectile.Remaining <= lifetime), "Split does not reset projectile lifetime");
                    session.State.BuildScoreMultiplier = 1;
                    int normalScore = RunMeta.Score(session.State, 10);
                    session.State.BuildScoreMultiplier = 2;
                    require(RunMeta.Score(session.State, 10) == normalScore, "The ranking number is the farthest night: a stored score multiplier no longer changes it");
                    float bonus = session.Bonus(ItemEffect.BulletSplit);
                    session.State.Phase = SessionPhase.Shop; session.ContinueWave();
                    require(session.Bonus(ItemEffect.BulletSplit) == bonus, "Purchased stats persist across wave transition");
                    var restored = new RunSession(catalog); restored.Restore(session.State);
                    require(restored.Bonus(ItemEffect.BulletSplit) == bonus, "Purchased stats survive save restoration once");
                    restored.State.Phase = SessionPhase.Victory; restored.EnterEndless();
                    require(restored.Bonus(ItemEffect.BulletSplit) == bonus, "Endless retains existing stat build");
                    restored.Start(catalog.Champions[0].Id, 10101);
                    require(restored.Bonus(ItemEffect.BulletSplit) == 0, "New run resets acquired stats");
                    session.State.Wave = 20;
                    float lateHealth = session.PrepareCombat().MeleeEnemy.Health;
                    session.State.Wave = 3;
                    require(lateHealth > session.PrepareCombat().MeleeEnemy.Health, "Late enemies gain health rather than only visual size");
                    require(RunBuildReadModel.Capture(session, catalog, 5).Stats.Any(row => row.Name == "Split shots"), "Details expose hidden projectile stat types");
                    item.Modifiers = new[] { Modifier(ItemEffect.CollisionDamage, 10) };
                    combat = session.PrepareCombat();
                    var simulation = new CombatSimulation(combat); simulation.Start(combat.Seed);
                    UnitState enemy = simulation.AddFixture(combat.MeleeEnemy, Faction.Enemy, Vector2.zero); enemy.Health = 100;
                    simulation.Champion.Position = simulation.Champion.PreviousPosition = Vector2.zero;
                    for (int step = 0; step < 16; step++) simulation.Step(1f / 60, Vector2.zero);
                    require(enemy.Health < 100, "Reciprocal collision damages actual nearby enemy after its contact windup");
                    item.Modifiers = new[] { Modifier(ItemEffect.BulletSplit, 100), Modifier(ItemEffect.BulletSize, 100), Modifier(ItemEffect.Dodge, 100) };
                    combat = session.PrepareCombat();
                    require(combat.StartingCompanions[0].Weapon.ProjectileSplit == 6 && combat.Dodge == 0.6f, "Technical split/dodge caps apply without deleting owned stacks");
                    require(Mathf.Approximately(combat.StartingCompanions[0].Weapon.ProjectileRadius, raw.ProjectileRadius * 4), "Bullet size cap keeps collision bounded");
                }
                if (original.StatRevision > 0)
                {
                    require(original.Champions.Where(unit => unit.HeroMechanic != HeroMechanic.None).All(unit => unit.ChampionModifiers.Any(modifier => modifier.Effect >= ItemEffect.ScoreMultiplier)), "All ten original protagonists have authored new starting-stat identities");
                    foreach (UnitDefinition protagonist in original.Champions.Where(unit => unit.HeroMechanic != HeroMechanic.None))
                    {
                        var starting = new RunSession(original); starting.Start(protagonist.Id, 7707);
                        RunBuildReadModel startingModel = RunBuildReadModel.Capture(starting, original, 5);
                        require(startingModel.Stats.Any(row => row.Explanation.Contains("personal trait") && row.Explanation.Contains(RunUICopy.UnitName(protagonist))), "New stat sources include protagonist identity " + protagonist.Id);
                    }
                    foreach (string term in new[] { "split", "pierce", "bounce", "armor", "bond", "scope", "merge", "altar", "reroll", "correction", "champion", "companions" })
                        require(GlossaryUI.Find(term) != null && GlossaryUI.Find(term).Description.Length > 20, "Term has a one-sentence explanation: " + term); // play-test 20/10/2026: one short sentence
                    foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
                        require(GlossaryUI.Find(RunModifierRules.ModifierName(effect)) != null, "All stat labels have definitions: " + effect);
                    require(GlossaryUI.Show("split") && GlossaryUI.Open, "Click route opens contextual definition");
                    GlossaryUI.Close(); require(!GlossaryUI.Open && !GlossaryUI.Show("unknown-term"), "Dismiss clears glossary without fake unknown explanation");
                    require(original.Companions.Count(unit => unit.Body != null && unit.Body.texture.name == unit.Id && File.Exists("Assets/ArcaneBrigade/Art/K10/" + unit.Id + ".png")) == 32 && original.Weapons.Count(weapon => File.Exists("Assets/ArcaneBrigade/Art/K10/" + weapon.Id + ".png")) == 24, "32 new species and 24 new weapon identities loaded");
                    // the eight theme bonds; the small trait bonds added later (hare, archer, root...) are two or three species by design
                    foreach (BondDefinition bond in original.Bonds.Where(b => new[] { "hearth", "archive", "clock", "frost", "orchard", "astral", "tidal", "oddity" }.Contains(b.Id)))
                    {
                        int species = original.Companions.Count(unit => unit.Tags.Contains(bond.Id));
                        require(species >= 4, "Bond has at least 4 species: " + bond.Id + " (" + species + ")");
                    }
                    foreach (UnitDefinition unit in original.Companions.Where(unit => unit.Body != null && unit.Body.texture.name != null && unit.Body.texture.name == unit.Id))
                        require(unit.AnimationFrames.Length == 4 && unit.AnimationFrames.Distinct().Count() == 4, "Distinct imported animation frames " + unit.Id);
                    require(original.Items.Where(item => item.Id.StartsWith("k10-")).SelectMany(item => item.EffectiveModifiers).Select(modifier => modifier.Effect).Distinct().Count() >= K10StatRules.Names.Length, "All added stat families have purchasable content");
                    require(original.Bonds.Where(bond => bond.Id == "orchard" || bond.Id == "astral" || bond.Id == "tidal" || bond.Id == "oddity").All(bond => bond.Icon.texture.name.StartsWith("Bond")), "New systems use dedicated icons rather than friendly portraits");
                }
            }
            finally { foreach (UnitDefinition unit in catalog.Companions.Concat(catalog.Champions)) UnityEngine.Object.DestroyImmediate(unit); UnityEngine.Object.DestroyImmediate(catalog); }
            Directory.CreateDirectory("PreviewExports/K10"); File.WriteAllLines("PreviewExports/K10/Focused_Checks.txt", notes);
            Debug.Log("K10_FOCUSED_CHECKS_PASSED " + notes.Count);
        }
        private static StatModifier Modifier(ItemEffect effect, float value) => new StatModifier { Effect = effect, Value = value, Scope = RunModifierRules.DefaultScope(effect) };
    }
}
