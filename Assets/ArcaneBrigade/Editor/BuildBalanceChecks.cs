using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class BuildBalanceChecks
    {
        public static void Run()
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var notes = new List<string>();
            void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); notes.Add("PASS " + label); }
            var roll = typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic);
            using (var host = new RunSession(catalog))
            {
                host.Start("PatchworkHost", 77138);
                var early = host.PrepareCombat();
                float earlyHealth = early.MeleeEnemy.Health, earlyBudget = early.EnemyBudget;
                host.State.Wave = 20;
                var bonus = typeof(RunSession).GetMethod("OctoberBonus", BindingFlags.Instance | BindingFlags.NonPublic);
                float lateHealth = (float)bonus.Invoke(host, new object[] { ItemEffect.EnemyHealth, ModifierScope.Run });
                float lateCount = (float)bonus.Invoke(host, new object[] { ItemEffect.EnemyCount, ModifierScope.Run });
                host.State.Wave = 1;
                float earlyBonus = (float)bonus.Invoke(host, new object[] { ItemEffect.EnemyHealth, ModifierScope.Run });
                Check(earlyBonus < lateHealth && Mathf.Approximately(lateHealth, .35f) && Mathf.Approximately(lateCount, .2f) && earlyHealth > 0 && earlyBudget > 0, "Broad-pool hero gets setup time while retaining full late drawback");
            }
            foreach (var hero in catalog.Champions)
            foreach (int seed in new[] { 77138, 161026 })
            using (var run = new RunSession(catalog))
            {
                run.Start(hero.Id, seed);
                for (int wave = 1; wave <= 5; wave++)
                {
                    run.State.Wave = wave; run.State.Phase = SessionPhase.Shop; run.State.Gold = 100000;
                    roll.Invoke(run, new object[] { true });
                    for (int refresh = 0; refresh < 4; refresh++)
                    {
                        var offer = run.State.Offers[1];
                        Check(offer.Kind == OfferKind.Item && ShopFoundationRules.Useful(hero, catalog.Item(offer.Id)) && run.CanBuy(offer), "Useful purchasable early stock " + hero.Id + "/" + seed + "/" + wave + "/" + refresh);
                        if (refresh < 3) Check(run.Reroll(), "Paid refresh remains available " + hero.Id + "/" + wave + "/" + refresh);
                    }
                }
            }
            foreach (var hero in catalog.Champions)
            for (int approach = 0; approach < 3; approach++)
            {
                string original = JsonUtility.ToJson(hero.Weapon);
                using var run = new RunSession(catalog); run.Start(hero.Id, 77138, 2);
                run.State.Wave = 10; run.State.HeroIdentity.Approach = approach;
                float before = run.PrepareCombat().Champion.Weapon.Damage;
                run.State.Items.Add(new ItemStack { Id = "key-oil", Count = 1 });
                var arena = run.PrepareCombat(); float after = arena.Champion.Weapon.Damage, cooldown = arena.Champion.Weapon.Cooldown;
                Check(after > before && float.IsFinite(after), "Damage item improves hero attack " + hero.Id + "/" + approach);
                run.State.Items.Add(new ItemStack { Id = "clock-spring", Count = 4 });
                arena = run.PrepareCombat();
                Check(arena.Champion.Weapon.Cooldown <= cooldown && arena.Champion.Weapon.Cooldown >= .28f, "Speed investment respects cooldown floor " + hero.Id + "/" + approach);
                float damage = arena.Champion.Weapon.Damage, speed = arena.Champion.Weapon.Cooldown;
                arena = run.PrepareCombat();
                Check(Mathf.Approximately(arena.Champion.Weapon.Damage, damage) && Mathf.Approximately(arena.Champion.Weapon.Cooldown, speed), "Rebuilding does not stack hero power " + hero.Id + "/" + approach);
                Check(JsonUtility.ToJson(hero.Weapon) == original, "Authored weapon unchanged " + hero.Id + "/" + approach);
                var saved = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run.State));
                using var restored = new RunSession(catalog); restored.Restore(saved);
                Check(Mathf.Approximately(restored.PrepareCombat().Champion.Weapon.Damage, damage), "Save/load retains compiled investment " + hero.Id + "/" + approach);
            }
            ModeBalanceChecks.Run();
            BossReliefChecks.Run();
            RecruitmentChecks.Run();
            Directory.CreateDirectory("PreviewExports/BuildBalance");
            File.WriteAllLines("PreviewExports/BuildBalance/Checks.txt", notes);
            Debug.Log("BUILD_BALANCE_CHECKS " + notes.Count + " PASS");
        }
    }
}
