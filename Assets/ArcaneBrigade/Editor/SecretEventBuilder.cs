using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class SecretEventBuilder
    {
        private static T Asset<T>(string id) where T : ScriptableObject
        {
            string path = "Assets/ArcaneBrigade/Resources/" + id + ".asset";
            T value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value == null) { value = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(value, path); }
            return value;
        }
        [MenuItem("Arcane Brigade/Secrets/Build content")]
        public static void Build()
        {
            SecretEventAssets assets = Asset<SecretEventAssets>("SecretEventAssets");
            assets.Portals = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Secrets/Portals_v1.png", "SecretPortal", 4, 4, Enumerable.Range(0, 16).ToArray(), new[] { 0, 256, 512, 768, 1024 }, true, true);
            assets.Relics = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Secrets/Relics_v1.png", "SecretRelic", 4, 4, Enumerable.Range(0, 16).ToArray());
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var items = catalog.Items.Where(i => !i.Id.StartsWith("Secret", StringComparison.Ordinal)).ToList();
            void Relic(string id, string name, string copy, int art, ItemEffect effect, float value, ItemEffect second, float other)
            {
                items.Add(new ItemDefinition { Id = id, Name = name, Description = copy, Icon = assets.Relics[art], Price = 60,
                    AltarPrice = 6, ShopWeight = 0, UnlockWave = int.MaxValue, Rarity = ItemRarity.Rare, Stacking = StackPolicy.Unique, MaximumStacks = 1,
                    Modifiers = new[] { new StatModifier { Effect = effect, Scope = RunModifierRules.DefaultScope(effect), Value = value }, new StatModifier { Effect = second, Scope = RunModifierRules.DefaultScope(second), Value = other } } });
            }
            Relic("SecretWoodHeart", "The Abandoned Wooden Heart", "Maximum HP +18. Healing +0.3 HP/s. A small heart that kept knocking.", 0, ItemEffect.Health, 18, ItemEffect.Heal, .3f);
            Relic("SecretMirror", "The Glass Understudy", "Champion basic attacks echo at 35% weapon damage once every 4 seconds as a delayed strike. Critical chance +5%.", 1, ItemEffect.Crit, .05f, ItemEffect.AttackRange, .08f);
            Relic("SecretThread", "Living Contraband Thread", "Attack range +18%. Knockback +20%. A thread that ties the stage together.", 2, ItemEffect.AttackRange, .18f, ItemEffect.Knockback, .2f);
            Relic("SecretTicket", "Smuggler's Ticket", "Gold shop discount +8%. Luck +8%. Valid for one impossible passenger.", 3, ItemEffect.Discount, .08f, ItemEffect.Luck, .08f);
            Relic("SecretHour", "Hourglass Advance", "Team attack speed +18%. Team damage +10%. Borrowed copies expire at their stated term.", 4, ItemEffect.AttackSpeed, .18f, ItemEffect.Damage, .1f);
            Relic("SecretDream", "Sleeping-Lamb Brooch", "Maximum HP +12. Heart pickups heal +3 HP. Nothing is taken in return.", 5, ItemEffect.Health, 12, ItemEffect.HeartHealing, 3);
            Relic("SecretCrown", "Crown of the Empty Theatre", "Team damage +15%. Critical chance +8%. Applause follows you home.", 6, ItemEffect.Damage, .15f, ItemEffect.Crit, .08f);
            Relic("SecretLetter", "Dead-Letter Key", "Pickup radius +0.8. Treasure detection +10%. Someone outside the stage now knows your name.", 7, ItemEffect.Pickup, .8f, ItemEffect.TreasureDetection, .1f);
            catalog.Items = items.ToArray();
            var companions = catalog.Companions.Where(u => !u.Id.StartsWith("Secret", StringComparison.Ordinal)).ToList();
            string[] ids = { "SecretPatchBear", "SecretPaperBird", "SecretDreamLamb" };
            string[] names = { "The Forgotten Patchwork", "The Folded Courier", "The Sleeping Friend" };
            BodyMaterial[] materials = { BodyMaterial.Fabric, BodyMaterial.Paper, BodyMaterial.Glass };
            assets.Rescues = new UnitDefinition[3];
            for (int i = 0; i < 3; i++)
            {
                UnitDefinition source = companions.FirstOrDefault(u => u.Rarity < ItemRarity.Crimson && u.Material == materials[i]) ?? companions[i];
                UnitDefinition ally = Asset<UnitDefinition>(ids[i]); EditorUtility.CopySerialized(source, ally);
                ally.Id = ids[i]; ally.DisplayName = names[i]; ally.UnlockWave = int.MaxValue; ally.Price = 80; ally.Rarity = ItemRarity.Rare;
                ally.SpecialRecruit = true; ally.RecruitThemeLocked = true; ally.HeroMechanic = HeroMechanic.None;
                ally.Description = "A rescued living toy from a secret room. " + (i == 0 ? "Keeps its original fabric puppet animation and weapon." : i == 1 ? "Carries an undelivered thank-you." : "Returns to sleep after a temporary garden visit.");
                ally.CompanionModifiers = new[] { new StatModifier { Scope = ModifierScope.Team, Effect = i == 0 ? ItemEffect.Damage : i == 1 ? ItemEffect.AttackSpeed : ItemEffect.AttackRange, Value = .06f } };
                assets.Rescues[i] = ally; companions.Add(ally); EditorUtility.SetDirty(ally);
            }
            catalog.Companions = companions.ToArray();
            assets.Guardian = Asset<UnitDefinition>("SecretCabinetGuardian"); EditorUtility.CopySerialized(catalog.Enemies[0], assets.Guardian);
            assets.Guardian.Id = "SecretCabinetGuardian"; assets.Guardian.DisplayName = "The Lost-Property Warden";
            assets.Guardian.Body = assets.Portals[0]; assets.Guardian.AnimationFrames = new[] { assets.Portals[0], assets.Portals[1] };
            assets.Guardian.AuthoredMotion = null; assets.Guardian.Material = BodyMaterial.Wood; assets.Guardian.Behavior = EnemyBehavior.Standard;
            assets.Guardian.Health = 1200; assets.Guardian.BodyHeight = 2.3f; assets.Guardian.Speed = .6f;
            foreach (UnityEngine.Object a in new UnityEngine.Object[] { assets, assets.Guardian, catalog }) EditorUtility.SetDirty(a);
            AssetDatabase.SaveAssets();
            Debug.Log("SECRET_CONTENT_BUILT 8 portals, 16 states, 8 relics, 3 rescue variants");
        }
        public static void Verify() => SecretEventChecks.Run();
        public static void Gate()
        {
            SecretEventChecks.Run();
            ToyboxAudioChecks.Run();
            Player();
        }
        public static void Player()
        {
            Directory.CreateDirectory("Builds/Secrets");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Builds/Secrets/ArcaneBrigade.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Secret event player build failed");
            Debug.Log("SECRET_PLAYER_BUILT errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings);
        }
    }
}
