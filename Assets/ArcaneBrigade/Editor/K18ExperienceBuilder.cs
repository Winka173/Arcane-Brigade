using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K18ExperienceBuilder
    {
        [MenuItem("Arcane Brigade/K18/Build experience expansion")]
        public static void Build()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            AddWeapon(catalog, "MoonseedOrb", "DreamOrbStaff", CarnivalWeapon.BloomOrb, "One slow orb blooms after impact.");
            AddWeapon(catalog, "AfterimageSabre", "SpoonSabre", CarnivalWeapon.EchoBlade, "Slash now. A ghost slash follows.");
            AddWeapon(catalog, "SatelliteDisc", "CrescentBoomerang", CarnivalWeapon.SatelliteDisc, "Circles one target, then returns.");
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            SpriteInkMetricsBuilder.Build(); Verify();
        }
        private static void AddWeapon(RunCatalog catalog, string id, string sourceId, CarnivalWeapon mechanic, string summary)
        {
            string path = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
            WeaponDefinition weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            WeaponDefinition source = catalog.Weapon(sourceId);
            if (weapon == null) { weapon = UnityEngine.Object.Instantiate(source); AssetDatabase.CreateAsset(weapon, path); }
            else EditorUtility.CopySerialized(source, weapon);
            weapon.Id = id; weapon.name = id; weapon.Mechanic = mechanic; weapon.GameplaySummary = summary;
            weapon.HomingOutbound = false;
            if (mechanic == CarnivalWeapon.BloomOrb)
            {
                weapon.Family = ExpandedWeapon.None; weapon.Signature = SignatureWeapon.None; weapon.SpecialOnly = true;
                weapon.ProjectileCount = 1; weapon.ProjectileSplit = 0; weapon.ProjectilePierce = 0;
                weapon.ProjectileSpeed = 2.8f; weapon.ProjectileLifetime = 3.5f; weapon.ProjectileRadius = 0.18f; weapon.ProjectileVisualWidth = 0.65f;
                weapon.ProjectileColor = new Color(0.5f, 0.9f, 0.8f); weapon.Damage *= 0.85f; weapon.Cooldown *= 1.2f;
            }
            else if (mechanic == CarnivalWeapon.EchoBlade)
            {
                weapon.Family = ExpandedWeapon.None; weapon.Signature = SignatureWeapon.None; weapon.SpecialOnly = false;
                weapon.Element = DamageElement.Arcane;
            }
            else
            {
                weapon.Family = ExpandedWeapon.Fish; weapon.Signature = SignatureWeapon.None; weapon.SpecialOnly = false;
                weapon.ProjectilePierce = 5; weapon.ProjectileSplit = 0; weapon.ProjectileCount = 1;
                weapon.ProjectileLifetime = 4; weapon.ProjectileSpeed = 4.2f; weapon.ProjectileColor = new Color(1, 0.75f, 0.3f);
                weapon.Damage *= 0.8f; weapon.Cooldown *= 1.25f;
            }
            if (!catalog.Weapons.Any(value => value.Id == id)) catalog.Weapons = catalog.Weapons.Concat(new[] { weapon }).ToArray();
            EditorUtility.SetDirty(weapon);
        }
        public static void Verify()
        {
            Directory.CreateDirectory("PreviewExports/K18");
            Checks(); SpriteInkMetricsBuilder.Audit();
            ShopLayerChecks.Verify();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K18/Windows_Build.txt", true);
        }
        public static void Campaign() => K17CampaignDiagnostics.RunInto("PreviewExports/K18");
        public static void Checks()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption)
            {
                if (!condition) throw new InvalidOperationException("K18 " + caption);
                notes.Add("PASS " + caption);
            }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var damageReport = new List<string> { "variant,source,variant_dps,source_dps,ratio" };
            foreach (string id in new[] { "MoonseedOrb", "AfterimageSabre", "SatelliteDisc" })
            {
                Require(catalog.Weapon(id) != null, "Authored variant exists " + id);
                Require(SignatureDescription.Describe(catalog.Weapon(id), 1).Contains(catalog.Weapon(id).GameplaySummary), "Collection and sandbox explain the new mechanic " + id);
                using var sample = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon(id), 1, 8);
                float damage = sample.Run(5);
                Require(damage > 0, "Variant deals actual attributed damage " + id);
                Require(sample.Simulation.Champion.Alive, "Variant cannot hurt its player " + id);
                Require(sample.Simulation.Contributions.All(entry => entry.Damage >= 0), "Variant contribution values remain nonnegative " + id);
                string source = id == "MoonseedOrb" ? "DreamOrbStaff" : id == "AfterimageSabre" ? "SpoonSabre" : "CrescentBoomerang";
                using var baseline = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon(source), 1, 8);
                float sourceDamage = baseline.Run(5);
                damageReport.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2:F3},{3:F3},{4:F3}", id, source, damage, sourceDamage, damage / Mathf.Max(0.001f, sourceDamage)));
            }
            File.WriteAllLines("PreviewExports/K18/Weapon_DPS.csv", damageReport);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 18077); session.State.Phase = SessionPhase.Shop; session.State.Gold = 100000;
                string id = session.State.Roster[0].UnitId;
                session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = "BubbleWand" });
                session.State.MergeDrought.Add(new MergeDroughtEntry { UnitId = id, Misses = 4 });
                session.State.Offers.Clear();
                Require(session.Reroll(), "Pity refresh remains paid");
                Require(session.State.Offers.Any(offer => offer.Kind == OfferKind.Companion && offer.Id == id), "Eligible ally slot favors drought-ready merge");
                using var restored = new RunSession(catalog); restored.Restore(session.State);
                Require(restored.MergeMisses(id) == session.MergeMisses(id), "Merge drought persists through save and restore");
                session.State.Wave = 6;
                ItemDefinition dreamItem = catalog.Items.First(item => (item.Pools & ShopPool.Eggs) != 0 && item.UnlockWave <= 6);
                session.State.SpecialOffers.Clear();
                session.State.SpecialOffers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = dreamItem.Id, Currency = ShopCurrency.Eggs, Price = 1 });
                int normalMisses = session.MergeMisses(id);
                Require(session.RerollAltar() && session.MergeMisses(id) == normalMisses, "Refreshing dream market cannot advance normal-market merge assistance");
                session.State.NewRecruitsThisWave = 3;
                Require(session.Reroll() && session.State.Offers.All(offer => offer.Kind == OfferKind.Item), "Pity cannot bypass three-body quota");
                ShopOffer itemOffer = session.State.Offers[0];
                string preview = ShortOfferCopy.PurchaseDelta(session, catalog, itemOffer);
                Require(preview.Contains("Owned:") && preview.Length < 240, "Default item preview is compact and delta-based");
                string snapshot = JsonUtility.ToJson(session.State);
                ShortOfferCopy.PurchaseDelta(session, catalog, itemOffer);
                Require(snapshot == JsonUtility.ToJson(session.State), "Reading purchase preview changes neither RNG nor save state");
                session.State.Gold = 0;
                string drought = JsonUtility.ToJson(session.State);
                Require(!session.Reroll() && JsonUtility.ToJson(session.State) == drought, "Failed reroll cannot advance assistance");
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 18077); session.State.Phase = SessionPhase.Shop; session.State.Gold = 100000;
                UnitDefinition unit = catalog.Companions.First(value => !value.SpecialRecruit && value.CompanionModifiers.Any(modifier => modifier.Value != 0));
                WeaponDefinition[] arms = catalog.Weapons.Where(weapon => !weapon.SpecialOnly).Take(4).ToArray();
                session.State.Roster.Clear();
                for (int index = 0; index < 4; index++) session.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = arms[index].Id, Tier = index < 2 ? 1 : 2 });
                var offer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = unit.Id, Price = 1 };
                session.State.Offers.Clear(); session.State.Offers.Add(offer);
                StatModifier modifier = unit.CompanionModifiers.First(value => value.Value != 0);
                float before = session.Bonus(modifier.Effect, modifier.Scope);
                string text = ShortOfferCopy.PurchaseDelta(session, catalog, offer);
                Require(text.Contains("3 stars"), "Purchase preview detects a cascading three-star merge");
                Require(text.Contains(RunModifierRules.ValueText(modifier.Effect, modifier.Value * -2)), "Merge preview shows net support reduction, not fictitious addition");
                session.State.MergeDrought.Add(new MergeDroughtEntry { UnitId = unit.Id, Misses = 6 });
                Require(session.Buy(offer.Token), "Projected cascade actually commits");
                Require(Mathf.Abs(session.Bonus(modifier.Effect, modifier.Scope) - before - modifier.Value * -2) < 0.001f, "Projected support delta matches committed merge");
                Require(session.MergeMisses(unit.Id) == 0, "Merge assistance resets when merge completes");
            }
            foreach (int wave in new[] { 3, 4, 5, 10, 20 })
            {
                float duration = 45;
                Require(WaveCadence.Envelope(wave, duration * 0.05f, duration) > WaveCadence.Envelope(wave, duration * 0.5f, duration), "Opener is lighter than climax " + wave);
                Require(WaveCadence.Envelope(wave, duration * 0.75f, duration) > WaveCadence.Envelope(wave, duration * 0.5f, duration) * 1.7f, "Recovery after climax " + wave);
            }
            foreach (float duration in new[] { 35f, 45f, 55f })
            {
                float oldBudget = 0, newBudget = 0;
                for (float time = 0; time < duration; time += 0.01f)
                {
                    oldBudget += 0.01f / (WaveCadence.IntervalMultiplier(5, time) * (1 - time / duration * 0.4f));
                    newBudget += 0.01f / WaveCadence.Envelope(5, time, duration);
                }
                Require(newBudget / oldBudget > 0.94f && newBudget / oldBudget < 1.06f, "Pacing retains expected spawn and coin volume for " + duration + " seconds");
            }
            Require(!MidwaveSpectacle.Eligible(3, false) && !MidwaveSpectacle.Eligible(5, true), "No early-wave or boss event overload");
            Require(new[] { 5, 7, 9 }.Select(wave => MidwaveSpectacle.Select(wave, 171077)).Distinct().Count() == 3, "Three event types rotate deterministically");
            foreach (int wave in new[] { 5, 7, 9 })
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171077); session.State.Wave = wave;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.WaveDuration = 35;
                var simulation = new CombatSimulation(arena); simulation.Start(171077);
                for (int tick = 0; tick < 930; tick++) simulation.Step(1f / 60, Vector2.zero);
                Require(simulation.Spectacle != null && simulation.Spectacle.Active(simulation.Elapsed), "Event starts in actual simulation " + wave);
                MidwaveSpectacle show = simulation.Spectacle;
                float health = simulation.Champion.Health;
                while (show.Age(simulation.Elapsed) < 1.8f)
                {
                    simulation.Champion.Position = show.Position; simulation.Step(1f / 60, Vector2.zero);
                }
                Require(simulation.Champion.Health == health && !show.Dangerous, "Event warning never damages player " + show.Kind);
                simulation.Pause(true); float paused = simulation.Elapsed;
                simulation.Step(0.05f, Vector2.zero);
                Require(simulation.Elapsed == paused, "Pause freezes event timeline " + show.Kind);
                simulation.Pause(false);
                while (show.Age(simulation.Elapsed) < 2.25f)
                {
                    simulation.Champion.Position = show.Position; simulation.Step(1f / 60, Vector2.zero);
                }
                float struck = simulation.Champion.Health;
                Require(struck < health && health - struck <= 5.01f, "Event contact is real, small and capped " + show.Kind);
                simulation.Champion.Position = show.Position; simulation.Step(0.05f, Vector2.zero);
                Require(simulation.Champion.Health >= struck && show.NextThreat > simulation.Elapsed, "Event contact has a one-second cooldown " + show.Kind);
                float elapsed = simulation.Elapsed; simulation.StepRest(0.05f, Vector2.zero);
                Require(simulation.Elapsed == elapsed, "Event cannot advance through invalid rest stepping " + wave);
                for (int tick = 0; tick < 700; tick++) simulation.Step(1f / 60, Vector2.zero);
                Require(simulation.Spectacle.RewardDrops <= 12 && !simulation.Spectacle.Active(simulation.Elapsed), "Event expires with bounded reward budget " + wave);
                simulation.EnterRest(); Require(simulation.Spectacle == null, "Shop clears event hazards " + wave);
            }
            float alpha;
            EffectDensity.SetSceneLoad(0, 0); alpha = EffectDensity.AlliedAlpha;
            EffectDensity.SetSceneLoad(250, 150); Require(EffectDensity.AlliedAlpha < alpha && EffectDensity.AlliedAlpha > 0.2f, "Dense combat dims only allied cosmetic layers");
            EffectDensity.SetSceneLoad(0, 0);
            float scale = Time.timeScale;
            var feel = new CombatFeel(); feel.Play(Array.Empty<CombatEvent>(), new Dictionary<int, UnitVisual>(), 0);
            Require(Time.timeScale == scale, "Impact presentation cannot pause global gameplay clock");
            float gait = 0;
            for (int frame = 0; frame < 20; frame++)
            {
                float next = UnitVisual.BlendGait(gait, 1, 1f / 60);
                Require(next >= gait && next - gait < 0.25f, "Continuous idle-to-run transition " + frame); gait = next;
            }
            Require(gait > 0.95f, "Run gait reaches full weight promptly");
            for (int frame = 0; frame < 25; frame++) gait = UnitVisual.BlendGait(gait, 0, 1f / 60);
            Require(gait < 0.03f, "Run-to-idle settles without indefinite wobble");
            File.WriteAllLines("PreviewExports/K18/Focused_Checks.txt", notes); Debug.Log("K18_EXPERIENCE_CHECKS_PASSED " + notes.Count);
        }
    }
}
