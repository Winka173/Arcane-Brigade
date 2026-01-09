using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8ContentChecks
    {
        public static void Run()
        {
            RunCatalog content = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Require(content != null && content.ContentRevision == 1, "Apply content before checks");
            string catalogBefore = JsonUtility.ToJson(content);
            string[] linkDescriptions = content.BuildLinks.Select(link => link.Description).ToArray();
            string[] unitsBefore = content.Champions.Concat(content.Companions).Select(JsonUtility.ToJson).ToArray();
            K8ContentBuilder.Apply();
            Require(linkDescriptions.SequenceEqual(content.BuildLinks.Select(link => link.Description)), "Detailed build link descriptions preserved");
            Require(catalogBefore == JsonUtility.ToJson(content) && unitsBefore.SequenceEqual(content.Champions.Concat(content.Companions).Select(JsonUtility.ToJson)), "Idempotent builder");
            RunModifierRules.Validate(content);
            Require(content.Recipes.Length == 4 && content.Recipes.Select(recipe => recipe.ResultUnit).Distinct().Count() == 4, "Four distinct fusion outcomes");
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
                Require(RunModifierRules.ModifierName(effect) != effect.ToString(), "Localized modifier " + effect);
            Require(RunModifierRules.ScopeName(ModifierScope.Champion) == "Champion", "Localized champion scope");
            CheckBonds(content);
            CheckTiersAndScopes(content);
            CheckChampions(content);
            CheckRecipes(content);
            Debug.Log("ARCANE_K8_CONTENT_CHECKS_PASS");
        }

        private static void CheckBonds(RunCatalog content)
        {
            var legacy = new BondDefinition { Threshold = 2, Value = 0.08f };
            Require(legacy.ActiveLevel(5) == 2 && legacy.NextThreshold(5) == 6 && Near(legacy.Reward(5), 0.16f), "Default repeating threshold fixture unchanged");
            foreach (BondDefinition bond in content.Bonds)
            {
                Require(bond.ExtraBonuses.Length == 2 && bond.ExtraBonuses[0].Threshold == 4 && bond.ExtraBonuses[1].Threshold == 6, bond.Id + " explicit milestones");
                Require(Near(bond.Reward(2), bond.Value) && Near(bond.Reward(4), bond.Value * 3) && Near(bond.Reward(6), bond.Value * 6), bond.Id + " cumulative 2/4/6 reward");
                Require(Near(bond.Reward(3), bond.Reward(2)) && Near(bond.Reward(5), bond.Reward(4)), bond.Id + " body counts, not tier counts");
                Require(bond.NextThreshold(3) == 4 && bond.ActiveLevel(6) == 3, bond.Id + " readable activation API");
            }
        }

        private static void CheckTiersAndScopes(RunCatalog content)
        {
            WeaponDefinition ranged = content.Weapons.First(weapon => weapon.Kind == WeaponKind.Projectile);
            WeaponDefinition melee = content.Weapons.First(weapon => weapon.Kind == WeaponKind.Melee);
            using (var session = new RunSession(content))
            {
                session.Start(content.Champions[0].Id, 808);
                session.State.Roster.Clear();
                session.State.Roster.Add(new RosterEntry { UnitId = content.Companions[0].Id, WeaponId = ranged.Id, Tier = 3 });
                session.State.Roster.Add(new RosterEntry { UnitId = content.Companions[1].Id, WeaponId = melee.Id, Tier = 2 });
                session.State.Items.Add(new ItemStack { Id = "royal-key", Count = 2 });
                session.State.Items.Add(new ItemStack { Id = "brigade-banner", Count = 3 });
                string rangedBefore = JsonUtility.ToJson(ranged);
                string meleeBefore = JsonUtility.ToJson(melee);
                CheckpointCatalog combat = session.PrepareCombat();
                UnitDefinition archer = combat.StartingCompanions[0];
                UnitDefinition fighter = combat.StartingCompanions[1];
                Require(archer.Weapon.ProjectileCount == ranged.ProjectileCount + 2 && archer.Weapon.ProjectilePierce == ranged.ProjectilePierce + 2, "Tier three companion gains multishot and pierce");
                Require(Near(fighter.Weapon.Range, melee.Range * 1.1f) && Near(fighter.Weapon.ConeDot, Mathf.Max(-1, melee.ConeDot - 0.1f)), "Tier two melee gains reach and arc");
                float expectedCooldown = melee.Cooldown / 1.12f / Mathf.Max(0.1f, 1 + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions) + session.Bonus(ItemEffect.CompanionAttackSpeed));
                Require(Near(fighter.Weapon.Cooldown, expectedCooldown), "Melee tempo composes with scoped attack speed");
                Require(Near(session.Bonus(ItemEffect.ChampionDamage), 0.44f) && Near(session.Bonus(ItemEffect.ChampionDamage, ModifierScope.Companions), 0), "Champion damage exact scope fixture");
                Require(Near(session.Bonus(ItemEffect.CompanionDamage, ModifierScope.Champion), 0), "Companion damage cannot leak to champion");
                Require(combat.Champion.CombatTier == 1 && Near(combat.Champion.Weapon.Range, content.Champions[0].Weapon.Range), "Champion never receives companion tier reach");
                Require(rangedBefore == JsonUtility.ToJson(ranged) && meleeBefore == JsonUtility.ToJson(melee), "Tier rules clone weapons without modifying source assets");
                Require(session.BondCounts.Values.All(count => count <= 2), "Tier three body still counts as one bond member");
            }
            RunCatalog legacy = UnityEngine.Object.Instantiate(content);
            legacy.ContentRevision = 0;
            try
            {
                using (var session = new RunSession(legacy))
                {
                    session.Start(legacy.Champions[0].Id, 809);
                    session.State.Roster[0].Tier = 3;
                    session.State.Roster[0].WeaponId = ranged.Id;
                    WeaponDefinition built = session.PrepareCombat().StartingCompanions[0].Weapon;
                    Require(built.ProjectileCount == ranged.ProjectileCount && built.ProjectilePierce == ranged.ProjectilePierce, "Revision zero retains scalar tier attack fixture");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(legacy); }
        }

        private static void CheckChampions(RunCatalog content)
        {
            foreach (UnitDefinition champion in content.Champions)
            {
                using (var session = new RunSession(content))
                {
                    session.Start(champion.Id, 810);
                    session.State.Roster.Clear();
                    CheckpointCatalog combat = session.PrepareCombat();
                    Require(champion.ChampionModifiers.Length >= 3, champion.Id + " authored identity rules");
                    if (champion.Id == "LanternKnight")
                        Require(Near(session.Bonus(ItemEffect.CompanionDamage), 0.25f) && Near(session.Bonus(ItemEffect.CompanionAttackSpeed), 0.15f) && Near(combat.CompanionLeash, content.Arena.CompanionLeash + 0.6f), "Leader boosts companions and patrol only");
                    if (champion.Id == "Postmaster")
                    {
                        Require(combat.Champion.Weapon.ProjectileCount == champion.Weapon.ProjectileCount + 1 && Near(combat.PickupRadius, content.Arena.PickupRadius + 0.5f), "Postmaster extra shot and collection radius");
                        Require(Near(session.Bonus(ItemEffect.Projectiles, ModifierScope.Companions), 0), "Postmaster extra shot cannot leak to allies");
                        session.State.Phase = SessionPhase.Shop;
                        session.State.Gold = 1000;
                        Require(session.Reroll(), "Postmaster rolls shop");
                        foreach (ShopOffer offer in session.State.Offers)
                        {
                            int price = offer.Kind == OfferKind.Companion ? content.Unit(offer.Id).Price : content.Item(offer.Id).Price;
                            Require(offer.Price == Mathf.Max(1, Mathf.CeilToInt(price * 0.85f)), "Postmaster discount applies on actual offers");
                        }
                        Require(session.State.Gold == 1000 - content.RerollPrice, "Champion discount does not reduce reroll fee");
                    }
                    if (champion.Id == "DrowsySeer")
                        Require(Near(combat.Armor, 0.35f) && Near(combat.PassiveHeal, 0.6f) && Near(combat.Champion.CriticalChance, Mathf.Clamp01(champion.CriticalChance + 0.15f)), "Seer armor regeneration and crit reach combat catalog");
                    combat.EnemyBudget = 0;
                    combat.Boss = null;
                    combat.WaveDuration = 1000;
                    combat.ChestInterval = 1000;
                    combat.EnableArenaToys = false;
                    var simulation = new CombatSimulation(combat);
                    simulation.Start(810);
                    simulation.Champion.Health -= 20;
                    float healthBefore = simulation.Champion.Health;
                    simulation.Step(0.05f, Vector2.zero);
                    if (champion.Id == "Postmaster") Require(simulation.Gold == Mathf.RoundToInt(champion.PassiveStrength), "Postmaster supported passive earns actual gold");
                    if (champion.Id == "DrowsySeer") Require(simulation.Champion.Health > healthBefore + 0.02f, "Seer supported healing restores actual health");
                }
            }
        }

        private static void CheckRecipes(RunCatalog content)
        {
            foreach (AltarRecipe recipe in content.Recipes)
                using (var session = new RunSession(content))
                {
                    session.Start(content.Champions[0].Id, 811);
                    session.State.Phase = SessionPhase.Shop;
                    session.State.Roster.Clear();
                    session.State.Eggs = recipe.EggCost;
                    session.State.Roster.Add(new RosterEntry { UnitId = recipe.FirstUnit, WeaponId = content.Weapons[0].Id, Tier = recipe.MinimumMaterialTier });
                    Require(!session.CanAltar(recipe.Id), recipe.Id + " refuses missing second body");
                    session.State.Roster.Add(new RosterEntry { UnitId = recipe.SecondUnit, WeaponId = content.Weapons[1].Id, Tier = recipe.MinimumMaterialTier });
                    uint randomBefore = session.State.WeaponRandom;
                    string stateBefore = JsonUtility.ToJson(session.State);
                    Require(session.ActiveRecipes().Any(active => active.Id == recipe.Id) && session.RecipePreview(recipe.Id).Contains("READY"), recipe.Id + " active preview");
                    Require(stateBefore == JsonUtility.ToJson(session.State), "Preview is side effect free");
                    Require(session.Altar(recipe.Id) && session.State.Eggs == 0 && session.State.Roster.Count == 1, recipe.Id + " consumes exactly two bodies and egg cost");
                    RosterEntry result = session.State.Roster[0];
                    Require(result.UnitId == recipe.ResultUnit && result.Tier == recipe.ResultTier && content.Weapon(result.WeaponId) != null && session.State.WeaponRandom != randomBefore, recipe.Id + " random legal result weapon and exact tier");
                    Require(session.State.AcquiredVariants.Any(entry => entry.UnitId == result.UnitId && entry.WeaponId == result.WeaponId && entry.Tier == result.Tier), "Fusion journals acquired variant");
                    session.PrepareCombat();
                    Require(session.BondCounts.Values.All(count => count == 1), "Fusion recounts result bonds");
                }
        }

        private static bool Near(float actual, float expected) => Mathf.Abs(actual - expected) < 0.0001f;
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("K8 content: " + message);
            Debug.Log("K8 content PASS: " + message);
        }
    }
}
