using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11MeleeChecks
    {
        public static void Build()
        {
            K11EnglishBuilder.Apply(); Run(); K11Checks.Run(); K11ThemeChecks.Run(); K8MotionChecks.Run();
            K7BalanceDiagnostics.Run(); FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K11/Windows_Build.txt", true);
            File.Copy("PreviewExports/K7/Movement_Bot_Balance.csv", "PreviewExports/K11/Movement_Bot_Balance.csv", true);
            Debug.Log("K11_MELEE_CANDIDATE_BUILT");
        }
        public static void FinalizeCandidate()
        {
            K11EnglishBuilder.Apply(); Run(); K11Checks.Run(); K11ThemeChecks.Run(); K8MotionChecks.Run();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K11/Windows_Build.txt", true);
            Debug.Log("K11_MELEE_FINAL_CANDIDATE_BUILT");
        }

        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException(caption); notes.Add("PASS " + caption); };
            string original = JsonUtility.ToJson(catalog);
            UnitDefinition species = catalog.Companions.First(unit => unit.Passive != PassiveKind.Rally);
            foreach (WeaponDefinition authored in catalog.Weapons)
            {
                string weaponOriginal = JsonUtility.ToJson(authored);
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 11217);
                    session.State.Roster.Clear(); session.State.Roster.Add(new RosterEntry { UnitId = species.Id, WeaponId = authored.Id, Tier = 1 });
                    CheckpointCatalog combat = session.PrepareCombat(); UnitDefinition unit = combat.StartingCompanions[0];
                    float damage = Mathf.Max(0, authored.Damage + session.Bonus(authored.Kind == WeaponKind.Projectile ? ItemEffect.RangedDamage : ItemEffect.MeleeDamage, ModifierScope.Companions)) * session.DamageMultiplier(species, 1, true) * K10StatRules.Factor(session.Bonus(ItemEffect.DamageCorrection, ModifierScope.Companions));
                    bool melee = authored.Kind == WeaponKind.Melee;
                    require(Mathf.Approximately(unit.Weapon.Damage, damage * (melee ? 1.5f : 1)), "Compiled damage multiplier " + authored.Id);
                    require(Mathf.Approximately(unit.Weapon.Range, authored.Range * K10StatRules.Factor(session.Bonus(ItemEffect.AttackRange, ModifierScope.Companions)) * (melee ? 1.35f : 1)), "Actual range multiplier " + authored.Id);
                    require(Mathf.Approximately(unit.Weapon.VisualWidth, authored.VisualWidth * (melee ? 1.55f : 1)), "Authored visual multiplier " + authored.Id);
                    float firstDamage = unit.Weapon.Damage;
                    require(Mathf.Approximately(session.PrepareCombat().StartingCompanions[0].Weapon.Damage, firstDamage), "Recompilation never stacks balance buff " + authored.Id);
                    require(JsonUtility.ToJson(authored) == weaponOriginal, "Source weapon not mutated " + authored.Id);
                }
            }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 11217); session.State.Wave = 2;
                session.State.Roster.Clear(); session.State.Roster.Add(new RosterEntry { UnitId = species.Id, WeaponId = "SpoonSabre", Tier = 1 });
                CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0; combat.Boss = null; combat.WaveDuration = 60; combat.EnableArenaToys = false; combat.EnableHeroMechanics = false; combat.PassiveHeal = 0;
                var simulation = new CombatSimulation(combat); simulation.Start(11217);
                simulation.Champion.NextAttack = 100;
                UnitState ally = simulation.Units.First(unit => unit.Faction == Faction.Companion);
                require(Mathf.Approximately(ally.Definition.Speed, species.Speed * K10StatRules.Factor(session.Bonus(ItemEffect.SpeedCorrection, ModifierScope.Companions)) * 1.3f), "Melee movement speed rises30 percent");
                require(CompanionMotion.Leash(ally, combat) == combat.CompanionLeash + 2, "Melee leash adds2 units beyond ranged leash");
                require(K8Motion.WeaponWidth(ally) > species.BodyHeight, "Melee display can exceed body width");
                ally.Position = ally.PreviousPosition = Vector2.zero;
                UnitState enemy = simulation.AddFixture(combat.MeleeEnemy, Faction.Enemy, new Vector2(6, 0));
                enemy.Definition.Speed = 0; enemy.NextAttack = enemy.NextContact = 100; enemy.Health = 20;
                float furthest = 0;
                bool pursued = false;
                var dustRoot = new GameObject("MeleePursuitDustCheck"); FootstepDust dust = dustRoot.AddComponent<FootstepDust>(); dust.Configure(combat.ImpactSprite);
                for (int tick = 0; tick < 600 && enemy.Alive; tick++)
                {
                    simulation.Step(1f / 60, Vector2.zero);
                    dust.Display(simulation, 1f / 60); pursued |= ally.TravelMode == CompanionTravelMode.Pursuit;
                    furthest = Mathf.Max(furthest, Vector2.Distance(ally.Position, simulation.Champion.Position));
                    require(Vector2.Distance(ally.Position, simulation.Champion.Position) <= CompanionMotion.Leash(ally, combat) + 0.001f, "Pursuit remains bounded tick" + tick);
                }
                require(furthest > combat.CompanionLeash && !enemy.Alive, "Melee leaves ranged patrol radius and actually kills distant enemy");
                require(pursued && dust.ActivePursuitPuffs > 0, "Pursuit mode emits live painted foot smoke and speed trails");
                int active = dust.ActivePuffs; simulation.Pause(true); dust.Display(simulation, 0.2f);
                require(dust.ActivePuffs == active, "Pause freezes foot smoke"); simulation.Pause(false);
                var settings = new PlayerProfile { ReducedMotion = PresentationSettings.ReducedMotion, ShowDamageNumbers = PresentationSettings.DamageNumbers, MusicVolume = PresentationSettings.MusicVolume, SfxVolume = PresentationSettings.SfxVolume, ReducedAudio = PresentationSettings.ReducedAudio, ShowLootIndicators = PresentationSettings.LootIndicators, HighReadability = PresentationSettings.HighReadability, Cosmetic = ArcadeSkin.Bundle, Volume = AudioListener.volume };
                try { PresentationSettings.Apply(new PlayerProfile { ReducedMotion = true }); dust.Display(simulation, 0.1f); require(dust.ActivePuffs == 0, "Reduced motion suppresses foot smoke/trails"); }
                finally { PresentationSettings.Apply(settings); UnityEngine.Object.DestroyImmediate(dustRoot); }
                simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(-8, 0);
                for (int tick = 0; tick < 240; tick++) simulation.Step(1f / 60, Vector2.zero);
                require(Vector2.Distance(ally.Position, simulation.Champion.Position) <= CompanionMotion.Leash(ally, combat), "Melee returns with relocated player");
                require(ally.TravelMode == CompanionTravelMode.Patrol, "Without enemy, pursuit returns to ordinary patrol");
            }
            foreach (int wave in new[] { 1, catalog.NormalWaves, catalog.NormalWaves + 1 })
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 11217); session.State.Wave = wave; session.State.Endless = wave > catalog.NormalWaves;
                    CheckpointCatalog combat = session.PrepareCombat(); combat.Boss = null; combat.EnemyBudget = 0; combat.WaveDuration = 0.05f; combat.PassiveHeal = 0;
                    var simulation = new CombatSimulation(combat); simulation.Start(11217); simulation.Champion.Health = 1;
                    for (int tick = 0; tick < 10 && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(1f / 60, Vector2.zero);
                    session.Settle(simulation);
                    require(simulation.Phase == RunPhase.Victory && Mathf.Approximately(session.State.Health, combat.Champion.Health), "Full wave-completion heal including final/endless wave" + wave);
                    require(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State)).Health == session.State.Health, "Full heal saved at wave boundary" + wave);
                }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 11217); CheckpointCatalog combat = session.PrepareCombat();
                var simulation = new CombatSimulation(combat); simulation.Start(11217); simulation.DamageFixture(simulation.Champion, 100000); session.Settle(simulation);
                require(session.State.Phase == SessionPhase.Defeat && session.State.Health <= 0, "Defeat does not revive champion");
            }
            require(JsonUtility.ToJson(catalog) == original, "Checks leave authored catalog intact");
            CheckRecruitBalance(catalog, require);
            CheckTierGrowth(catalog, species, require);
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Concat(catalog.Bosses))
                require(!K11EnglishBuilder.NeedsTranslation(unit.DisplayName) && !K11EnglishBuilder.NeedsTranslation(unit.Description), "English unit content " + unit.Id);
            foreach (ItemDefinition item in catalog.Items)
                require(!K11EnglishBuilder.NeedsTranslation(item.Name) && !K11EnglishBuilder.NeedsTranslation(item.Description), "English relic content " + item.Id);
            File.WriteAllLines("PreviewExports/K11/Melee_Checks.txt", notes);
            Debug.Log("K11_MELEE_CHECKS_PASSED " + notes.Count);
        }

        private static void CheckRecruitBalance(RunCatalog catalog, Action<bool, string> require)
        {
            foreach (UnitDefinition hero in catalog.Champions)
                using (var session = new RunSession(catalog))
                {
                    session.Start(hero.Id, 11217); session.State.Phase = SessionPhase.Shop;
                    UnitDefinition[] pool = catalog.Companions.Where(unit => K11HeroRules.Eligible(hero, unit)).ToArray();
                    for (int recruit = 0; recruit < 60; recruit++)
                    {
                        session.State.Gold = 10000;
                        session.State.Offers.Clear();
                        var offer = new ShopOffer { Token = ++session.State.NextOffer, Id = pool[recruit % pool.Length].Id, Kind = OfferKind.Companion, Price = 1 };
                        session.State.Offers.Add(offer);
                        require(session.Buy(offer.Token), "Actual balanced purchase " + hero.Id + "/" + recruit);
                        int melee = session.State.Roster.Count(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Melee);
                        int ranged = session.State.Roster.Count(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Projectile);
                        require(Math.Abs(melee - ranged) <= (recruit < pool.Length * 2 - 1 ? 1 : 3), "Adaptive roster balance; merge fixture allows temporary imbalance " + hero.Id + "/" + recruit);
                    }
                    float before = session.State.Health;
                    session.State.SpecialOffers.Clear(); var blood = new ShopOffer { Token = ++session.State.NextOffer, Id = catalog.Items[0].Id, Kind = OfferKind.Item, Currency = ShopCurrency.Health, Price = 3 };
                    session.State.SpecialOffers.Add(blood);
                    require(session.Buy(blood.Token) && session.State.Health < before, "Blood purchase still spends current HP " + hero.Id);
                    float spent = session.State.Health; session.PrepareCombat();
                    require(session.State.Health == spent, "Recompile is not a free full heal " + hero.Id);
                }
        }

        private static void CheckTierGrowth(RunCatalog catalog, UnitDefinition species, Action<bool, string> require)
        {
            foreach (WeaponDefinition authored in catalog.Weapons)
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 11217); session.State.Roster.Clear();
                    var entry = new RosterEntry { UnitId = species.Id, WeaponId = authored.Id, Tier = 1 }; session.State.Roster.Add(entry);
                    float previousBody = 0, previousRange = 0, previousWidth = 0, previousBullet = 0;
                    for (int tier = 1; tier <= 3; tier++)
                    {
                        entry.Tier = tier;
                        CheckpointCatalog combat = session.PrepareCombat(); UnitDefinition applied = combat.StartingCompanions[0];
                        float body = K8Motion.BodyHeight(new UnitState { Definition = applied, Faction = Faction.Companion });
                        if (tier > 1)
                        {
                            require(body > previousBody && applied.Weapon.VisualWidth > previousWidth && applied.Weapon.Range > previousRange, "Tier grows actual body, weapon and range " + authored.Id + "/" + tier);
                            if (authored.Kind == WeaponKind.Projectile) require(applied.Weapon.ProjectileRadius > previousBullet, "Tier grows bullet collision radius " + authored.Id + "/" + tier);
                        }
                        previousBody = body; previousWidth = applied.Weapon.VisualWidth; previousRange = applied.Weapon.Range; previousBullet = applied.Weapon.ProjectileRadius;
                        string saved = JsonUtility.ToJson(session.State);
                        RunBuildReadModel model = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                        require(model.Companions[0].CombatValues["Damage"] == applied.Weapon.Damage.ToString("0.###") && model.Companions[0].CombatValues["Speed"] == applied.Speed.ToString("0.###") && model.Companions[0].Explanation.Contains("Range: " + applied.Weapon.Range.ToString("0.0")), "Ledger reflects compiled melee/tier values " + authored.Id + "/" + tier);
                        require(JsonUtility.ToJson(session.State) == saved, "Ledger remains read-only " + authored.Id + "/" + tier);
                    }
                }
            using (var sandbox = new WeaponSandbox())
            {
                var profile = new PlayerProfile(); profile.SeenWeapons.Add("SpoonSabre");
                sandbox.Open(catalog, profile, weaponId: "SpoonSabre", tier: 3, createPresentation: false);
                WeaponDefinition authored = catalog.Weapon("SpoonSabre");
                require(Mathf.Approximately(sandbox.Attacker.Definition.Weapon.Range, authored.Range * MeleeBalance.Range * 1.36f), "Sandbox uses same melee and star range rules");
            }
        }
    }
}
