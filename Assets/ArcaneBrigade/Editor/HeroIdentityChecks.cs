using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class HeroIdentityChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static string Root => "PreviewExports/HeroIdentity";
        private static void Check(bool valid, string label)
        {
            notes.Add((valid ? "PASS " : "FAIL ") + label);
            Directory.CreateDirectory(Root); File.WriteAllLines(Root + "/Checks.txt", notes);
            if (!valid) throw new InvalidOperationException("Hero identity: " + label);
        }
        public static void Run()
        {
            notes.Clear(); RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            Check(catalog.Champions.Length == HeroRulesV3Tuning.HeroCount, "36 playable heroes from section 8");
            CheckDescriptions(catalog);
            foreach (var definition in HeroRulesV3Catalog.Definitions)
            {
                using var session = new RunSession(catalog); session.Start(definition.Id, 641031);
                session.State.Wave = 12;
                var arena = session.PrepareCombat();
                arena.EnemyBudget = 0; arena.Boss = null; arena.WaveDuration = 90;
                arena.EnableArenaToys = arena.EnableSpectacles = false;
                Check(arena.Champion.Id == definition.Id && arena.Champion.Weapon != null, "Compiled identity " + definition.Id);
                Check(session.ActiveRoster.Count <= RulesV3Tuning.CompanionLimit, "Ten-companion deployment cap " + definition.Id);
                foreach (string minor in HeroRulesV3Catalog.MinorIds(definition.Id))
                    Check(session.BondCounts.TryGetValue(minor, out int members) && members >= 1, "Hero supplies minor membership " + definition.Id + " / " + minor);
                var model = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                Check(model.Stats[0].Name == "Hero health", "Health summary is preserved " + definition.Id);
                Check(model.Stats.Find(row => row.Name == "Hero damage").Summary == arena.Champion.Weapon.Damage.ToString("0"), "Damage summary matches compiled value " + definition.Id);
                var sim = new CombatSimulation(arena); sim.Start(641031);
                Check(sim.HeroRulesV3.HeroId == definition.Id, "Live v3 hero runtime " + definition.Id);
                Check(sim.Units.Where(unit => MortalityRuntime.IsFriend(unit)).All(unit => float.IsFinite(unit.Health) && unit.Health > 0), "Friends start mortal and healthy " + definition.Id);
                for (int tick = 0; tick < 120; tick++) sim.Step(.05f, Vector2.right);
                Check(sim.Units.All(unit => float.IsFinite(unit.Position.x) && float.IsFinite(unit.Position.y) && float.IsFinite(unit.Health)), "Finite simulation " + definition.Id);
                float elapsed = sim.Elapsed, mechanicTime = sim.HeroRulesV3.Time;
                sim.Pause(true); sim.Step(.05f, Vector2.one);
                Check(sim.Elapsed == elapsed && sim.HeroRulesV3.Time == mechanicTime, "Pause cannot advance hero mechanics " + definition.Id);
                session.RulesV3HeroProgress.Species.Add("identity-fixture");
                session.RulesV3HeroProgress.Relics.Add(definition.Relics[0].Id);
                session.RulesV3HeroProgress.WavesSurvived = 3;
                session.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State)));
                Check(session.RulesV3HeroProgress.Species.Contains("identity-fixture") && session.RulesV3HeroProgress.Relics.Contains(definition.Relics[0].Id) && session.RulesV3HeroProgress.WavesSurvived == 3, "Hero progression save round trip " + definition.Id);
            }
            // The design replaces the old awakening meter and approach mechanics with these crowd rules.
            HeroRulesV3Checks.Run();
            CheckTransactions(catalog);
            CheckPurchaseCoverage(catalog);
            CheckShopFeedback(catalog);
            File.WriteAllLines(Root + "/Checks.txt", notes);
            File.WriteAllText(Root + "/Verification.json", "{\"Passed\":true,\"Checks\":" + notes.Count + ",\"Heroes\":" + HeroRulesV3Tuning.HeroCount + "}");
            Debug.Log("HERO_IDENTITY_PASS " + notes.Count);
        }
        public static void CheckDescriptions(RunCatalog catalog)
        {
            var report = new System.Text.StringBuilder("# Hero mechanics from design section 8\n\n");
            var ids = new HashSet<string>();
            foreach (var hero in HeroRulesV3Catalog.Definitions)
            {
                Check(ids.Add(hero.Id) && catalog.Champions.Any(unit => unit.Id == hero.Id), "Unique installed identity " + hero.Id);
                Check(!string.IsNullOrWhiteSpace(hero.Mechanic) && hero.BuildDirections.Length == HeroRulesV3Tuning.BuildsPerHero, "Mechanic and three build directions " + hero.Id);
                Check(HeroRulesV3Catalog.MinorIds(hero.Id).Distinct().Count() == 2, "Two distinct hero minors " + hero.Id);
                Check(hero.Relics.Length == HeroRulesV3Tuning.RelicsPerHero, "Four relics " + hero.Id);
                foreach (var relic in hero.Relics)
                    Check(catalog.Item(relic.Id) != null && !string.IsNullOrWhiteSpace(relic.Description), "Installed described relic " + relic.Id);
                report.AppendLine("## " + hero.Name + "\n\n" + hero.Mechanic + "\n");
            }
            foreach(var u in catalog.Companions)
            {
                string copy=CompanionDescriptions.Describe(u,1,catalog.TierSupportMultiplier);
                Check(copy.Contains("SUPPORT WHILE OWNED")&&copy.Contains("PERSONAL PASSIVE")&&!copy.Contains("An original toy companion. Its listed support"),"Complete talent/support/passive copy "+u.Id);
                foreach(var m in u.CompanionModifiers??Array.Empty<StatModifier>())Check(copy.Contains(RunModifierRules.ValueText(m.Effect,m.Value)),"Signed support value present "+u.Id+" / "+m.Effect);
                for(int tier=1;tier<=3;tier++)Check(!string.IsNullOrWhiteSpace(CompanionDescriptions.Describe(u,tier,catalog.TierSupportMultiplier)),"Star description "+u.Id+" / "+tier);
                report.AppendLine("## "+u.DisplayName+" / "+u.Id+"\n\n"+copy+"\n");
            }
            foreach(var item in catalog.Items)
            {
                string copy=RelicDescriptions.Describe(item,1,catalog);
                Check(copy.StartsWith(RelicDescriptions.Category(item)),"Category visible "+item.Id);
                // The 22 October balance pass makes positive movement bonuses count at 60%; cards show the applied value.
                foreach(var m in item.EffectiveModifiers)if(m.Value!=0)
                {
                    float applied = m.Effect == ItemEffect.MoveSpeed ? CarnivalBalance.MoveBonus(m.Value) : m.Value;
                    string expected = RunModifierRules.WhoFor(m.Effect,m.Scope) + RunModifierRules.ModifierName(m.Effect) + " " + RunModifierRules.ValueText(m.Effect,applied);
                    Check(copy.Contains(expected),"Effective scoped item value present "+item.Id+" / "+m.Effect);
                }
                if(item.Mechanic!=RelicMechanic.None)Check(copy.Contains(RelicDescriptions.Mechanic(item.Mechanic)),"Special item mechanic present "+item.Id);
                if(item.SignatureRole!=SignatureRelicKind.None)Check(copy.Contains(RelicDescriptions.Signature(item.SignatureRole)),"Weapon synergy present "+item.Id);
                foreach(var proc in catalog.ProcRules.Where(p=>p.ItemId==item.Id))Check(copy.Contains(proc.Trigger==ProcTrigger.CoinCollected?"pick up a coin":proc.Trigger==ProcTrigger.ChestOpened?"open a chest":"enemy falls")&&copy.Contains(proc.Power.ToString("0.#"))&&!copy.Contains(proc.Trigger.ToString()),"Authored proc visible in player words "+item.Id);
                report.AppendLine("## "+item.Name+" / "+item.Id+"\n\n"+copy+"\n");
            }
            Directory.CreateDirectory(Root); File.WriteAllText(Root + "/Hero_Description_Audit.md", report.ToString());
        }
        private static void CheckTransactions(RunCatalog catalog)
        {
            using var session = new RunSession(catalog); session.Start("LanternKnight", 12);
            session.State.Phase = SessionPhase.Shop; session.State.Gold = 1000;
            var hero = HeroRulesV3Catalog.Find("LanternKnight");
            var offer = new ShopOffer { Token = ++session.State.NextOffer, Id = hero.Relics[0].Id, Kind = OfferKind.Item, Price = 1 };
            session.State.Offers.Add(offer);
            Check(RunTransaction.TryCommit(session, catalog, "hero relic", () => session.Buy(offer.Token), () => true, out _), "Hero relic transaction commits");
            Check(session.RulesV3HeroProgress.Relics.Contains(offer.Id), "Purchased hero relic reaches v3 progression");
            int gold = session.State.Gold, revision = session.ShopStatRevision;
            Check(!RunTransaction.TryCommit(session, catalog, "rollback", () => { session.State.Gold -= 20; return true; }, () => false, out _) && session.State.Gold == gold && session.ShopStatRevision == revision, "Failed save rolls back without false feedback");
        }
        private static void CheckShopFeedback(RunCatalog c)
        {
            using var s=new RunSession(c);s.Start("LanternKnight",12);s.State.Wave=8;s.State.Phase=SessionPhase.Shop;s.State.Gold=1000;
            var offer=new ShopOffer{Token=++s.State.NextOffer,Id="key-oil",Kind=OfferKind.Item,Price=1};s.State.Offers.Add(offer);
            Check(RunTransaction.TryCommit(s,c,"test damage",()=>s.Buy(offer.Token),()=>true,out _),"Damage purchase committed");
            Check(s.LastShopStatChanges.Any(d=>d.ActorKey=="hero"&&d.Label=="Damage"&&d.After>d.Before) && s.LastShopStatChanges.Any(d=>d.ActorKey!="hero"&&d.Label=="Damage"&&d.After>d.Before),"Damage changes attributed to hero and affected allies");
            var feedback=new ShopStatFeedback();feedback.Observe(s);Check(feedback.PendingActors>0,"Shop feedback queued");
            s.State.Phase=SessionPhase.Combat;feedback.Observe(s);Check(feedback.PendingActors==0,"All stat feedback removed before combat");
            s.State.Phase=SessionPhase.Shop;int revision=s.ShopStatRevision,gold=s.State.Gold;
            Check(!RunTransaction.TryCommit(s,c,"rollback",()=>{s.State.Gold-=20;return true;},()=>false,out _)&&s.State.Gold==gold&&s.ShopStatRevision==revision,"Failed save rolls back without false stat feedback");
            var negative=c.Items.First(i=>i.EffectiveModifiers.Any(m=>m.Value<0));var original=s.State.Items.ToArray();
            var before=RunBuildReadModel.Capture(s,c,s.State.Health).ShopStats;s.State.Items.Add(new ItemStack{Id=negative.Id,Count=1});
            var after=RunBuildReadModel.Capture(s,c,s.State.Health).ShopStats;
            Check(ShopStatSnapshot.Compare(before,after).Any(d=>d.After<d.Before),"Negative changes retained in feedback model");
        }
        private static void CheckPurchaseCoverage(RunCatalog c)
        {
            foreach(var item in c.Items)
            {
                using var s=new RunSession(c);s.Start("LanternKnight",553);s.State.Phase=SessionPhase.Shop;s.State.Wave=12;s.State.Gold=1000000;
                var offer=new ShopOffer{Id=item.Id,Token=++s.State.NextOffer,Kind=OfferKind.Item,Price=1};s.State.Offers.Add(offer);
                int drinks=s.State.OctoberDrinks,food=s.State.OctoberFood;
                Check(RunTransaction.TryCommit(s,c,"Coverage / "+item.Id,()=>s.Buy(offer.Token),()=>true,out string error),"Every item can produce a valid committed description/diff "+item.Id+" "+error);
                Check(s.State.OctoberDrinks==drinks+(item.Category==RelicCategory.Drink?1:0) && s.State.OctoberFood==food+(item.Category==RelicCategory.Food?1:0),"Drink/food category matches actual purchase counter "+item.Id);
                Check(s.LastShopStatChanges.Any(d=>d.Label=="Gold"&&d.After<d.Before),"Currency loss retained "+item.Id);
            }
        }
        public static void RefreshDescriptions()
        {
            RunCatalog c=Resources.Load<RunCatalog>("RunCatalog");
            foreach(var u in c.Companions){u.Description=CompanionDescriptions.Describe(u,1,c.TierSupportMultiplier);EditorUtility.SetDirty(u);}
            foreach(var item in c.Items)item.Description=RelicDescriptions.Describe(item,1,c);
            EditorUtility.SetDirty(c);AssetDatabase.SaveAssets();
            Run();
        }
        public static void VerifyAndBuild()
        {
            RefreshDescriptions();
            Directory.CreateDirectory("Builds/HeroIdentity");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ArcaneBrigade/Scenes/RunBoot.unity","Assets/ArcaneBrigade/Scenes/RunFrontend.unity","Assets/ArcaneBrigade/Scenes/RunArena.unity","Assets/ArcaneBrigade/Scenes/WaitingRoom.unity"},locationPathName="Builds/HeroIdentity/ArcaneBrigade.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Root+"/Build.txt","Result: "+report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Identity player build failed");
            GameReviewSnapshot.Export();
        }
    }
}
