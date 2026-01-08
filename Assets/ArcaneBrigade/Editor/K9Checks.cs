using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K9Checks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K9: " + caption); notes.Add("PASS " + caption); };
            RunCatalog original = Resources.Load<RunCatalog>("RunCatalog");
            foreach (UnitDefinition companion in original.Companions)
                require(companion.CompanionModifiers.Any(modifier => modifier.Value > 0) && companion.CompanionModifiers.Any(modifier => modifier.Value < 0), companion.Id + " has an authored benefit and tradeoff");
            RunCatalog catalog = UnityEngine.Object.Instantiate(original);
            catalog.Companions = original.Companions.Select(UnityEngine.Object.Instantiate).ToArray();
            try
            {
                foreach (UnitDefinition companion in catalog.Companions) companion.CompanionModifiers = new StatModifier[0];
                UnitDefinition source = catalog.Companions[0];
                source.CompanionModifiers = new[]
                {
                    new StatModifier { Effect = ItemEffect.Health, Scope = ModifierScope.Champion, Value = 5 },
                    new StatModifier { Effect = ItemEffect.MoveSpeed, Scope = ModifierScope.Champion, Value = -0.05f },
                    new StatModifier { Effect = ItemEffect.Damage, Scope = ModifierScope.Companions, Value = 0.08f }
                };
                var session = new RunSession(catalog); session.Start(catalog.Champions[0].Id, 27191);
                session.State.Roster.Clear();
                float championBase = session.Bonus(ItemEffect.Damage, ModifierScope.Champion);
                float companionBase = session.Bonus(ItemEffect.Damage, ModifierScope.Companions);
                session.State.Roster.Add(new RosterEntry { UnitId = source.Id, WeaponId = catalog.Weapons[0].Id, Tier = 1 });
                require(Mathf.Approximately(session.Bonus(ItemEffect.Health), 5), "Companion grants player HP through real stat pipeline");
                require(Mathf.Approximately(session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion), -0.05f), "Negative player modifier retained");
                require(Mathf.Approximately(session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Companions), 0), "Player debuff does not leak to companions");
                require(Mathf.Approximately(session.Bonus(ItemEffect.Damage, ModifierScope.Champion), championBase), "Companion damage buff does not leak to player");
                require(Mathf.Approximately(session.Bonus(ItemEffect.Damage, ModifierScope.Companions), companionBase + 0.08f), "Companion scope affects actual team damage");
                session.State.Roster[0].Tier = 3;
                require(Mathf.Approximately(session.Bonus(ItemEffect.Health), 20), "Tier three scales support by four, not nine");
                var restored = new RunSession(catalog); restored.Restore(session.State);
                require(Mathf.Approximately(restored.Bonus(ItemEffect.Health), 20), "Restore derives modifiers once without duplicate stacking");
                session.State.Health = 10000;
                restored.Restore(session.State);
                require(Mathf.Approximately(restored.State.Health, catalog.Unit(session.State.ChampionId).Health + 20), "Restore clamps HP after current companion balance changes without healing missing HP");
                require(session.State.Health == 10000, "Restore health reconciliation does not mutate caller snapshot");
                require(RunBuildReadModel.Capture(restored, catalog, restored.State.Health).Companions[0].Explanation.Contains("ĐẶC TÍNH HỖ TRỢ"), "Roster discloses actual benefits and tradeoffs");
                session.State.Roster.Clear();
                session.State.Phase = SessionPhase.Shop;
                session.State.Gold = 100;
                session.State.Health = 10;
                session.State.Offers.Clear();
                session.State.Offers.Add(new ShopOffer { Token = 1, Kind = OfferKind.Companion, Id = source.Id, Price = 5, Currency = ShopCurrency.Gold });
                require(session.Buy(1) && Mathf.Approximately(session.State.Health, 15), "Buying HP companion preserves missing HP rather than fully healing");
                float health = session.State.Health;
                require(!session.Buy(1) && session.State.Health == health, "Repeated purchase does not reapply health gain");
                session.State.SpecialOffers.Add(new ShopOffer { Token = 2, Kind = OfferKind.Companion, Id = source.Id, Price = 20, Currency = ShopCurrency.Health });
                require(!session.Buy(2) && session.State.Health == health, "Future HP buff cannot finance unaffordable blood payment");
                source.CompanionModifiers[0].Value = -1000;
                session.State.Offers.Add(new ShopOffer { Token = 3, Kind = OfferKind.Companion, Id = source.Id, Price = 0, Currency = ShopCurrency.Gold });
                require(session.Buy(3) && session.State.Health >= 1, "Health tradeoff cannot kill player inside shop");
                source.CompanionModifiers[0].Value = 5;
                session.State.Roster.Clear(); session.State.Health = 10;
                session.State.Roster.Add(new RosterEntry { UnitId = source.Id, WeaponId = catalog.Weapons[0].Id });
                session.State.Roster.Add(new RosterEntry { UnitId = source.Id, WeaponId = catalog.Weapons[1].Id });
                session.State.Offers.Add(new ShopOffer { Token = 4, Kind = OfferKind.Companion, Id = source.Id, Price = 0, Currency = ShopCurrency.Gold });
                require(session.Buy(4) && session.State.Roster.Count == 1 && session.State.Roster[0].Tier == 2, "Three source units actually merge on purchase");
                require(Mathf.Approximately(session.Bonus(ItemEffect.Health), 10), "Merge removes three old support sources and adds one doubled source");
                require(Mathf.Approximately(session.State.Health, 10), "Merge does not grant discarded third-unit HP");
                require(source.Tags.All(tag => session.BondCounts[tag] == 1), "Merged companion remains one body for bonds");
                var dialogue = new MerchantDialogue(); dialogue.Speak(false, MerchantMoment.Enter, 10);
                require(dialogue.VisibleGlyphs(9, false) == 0 && dialogue.VisibleGlyphs(10.1f, false) == 5, "Speech cadence uses simulation time");
                require(dialogue.IsSpeaking(10.1f) && !dialogue.IsSpeaking(14), "Speaking pose ends after actual text reveal");
                require(dialogue.VisibleGlyphs(10, true) == dialogue.CurrentLine.Length, "Reduced motion reveals dialogue immediately");
                require(LivingUI.Reveal(-1, false) == 0 && LivingUI.Reveal(1, false) == 1 && LivingUI.Reveal(0, true) == 1, "UI reveal bounded with reduced motion bypass");
                require(LivingUI.Press(0.1f, false) > 0 && LivingUI.Press(1, false) == 0 && LivingUI.Press(0.1f, true) == 0, "Button compression settles and respects reduced motion");
                var bond = new BondDefinition { Id = "motion", Threshold = 2, Value = 0.1f };
                var feedback = new RunBondFeedback(); var counts = new Dictionary<string, int> { { bond.Id, 1 } };
                feedback.Observe("test", new[] { bond }, counts, 1);
                require(feedback.Burst(bond.Id, 1.1f) == 0, "Loading does not fabricate bond bounce");
                counts[bond.Id] = 2; feedback.Observe("test", new[] { bond }, counts, 2);
                require(feedback.Burst(bond.Id, 2.1f) > 0 && feedback.Burst(bond.Id, 3) == 0, "Actual bond activation bounces and settles");
                feedback.Observe("test", new[] { bond }, counts, 0);
                require(!feedback.Flashing(bond.Id, 0), "New wave resetting visual clock cannot prolong previous shop bond flash");
            }
            finally
            {
                foreach (UnitDefinition companion in catalog.Companions) UnityEngine.Object.DestroyImmediate(companion);
                UnityEngine.Object.DestroyImmediate(catalog);
            }
            Directory.CreateDirectory("PreviewExports/K9"); File.WriteAllLines("PreviewExports/K9/Focused_Checks.txt", notes);
            Debug.Log("K9_FOCUSED_CHECKS_PASSED " + notes.Count);
        }
    }
}
