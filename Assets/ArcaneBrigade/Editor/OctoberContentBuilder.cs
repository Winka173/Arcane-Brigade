using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class OctoberContentBuilder
    {
        private static T Asset<T>(string id) where T : ScriptableObject
        {
            string p = "Assets/ArcaneBrigade/Resources/October/" + id + ".asset";
            T a = AssetDatabase.LoadAssetAtPath<T>(p);
            if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, p); }
            return a;
        }
        private static Sprite[] Sheet(string kind, int number) => K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/October/" + kind + number + ".png", "October" + kind + number, 4, 4, Enumerable.Range(0, 16).ToArray(), new[] { 0, 256, 512, 768, 1024 }, true);
        private static void Art(UnitDefinition u, Sprite[] frames)
        {
            u.Body = frames[0]; u.AnimationFrames = frames;
            AuthoredMotionSet m = Asset<AuthoredMotionSet>(u.Id + "Motion");
            m.Idle = new[] { frames[0], frames[0] }; m.Walk = new[] { frames[0], frames[1], frames[0], frames[1] };
            m.Windup = new[] { frames[2], frames[2] }; m.Release = new[] { frames[3], frames[3] }; m.Recovery = new[] { frames[3], frames[0] };
            u.AuthoredMotion = m; EditorUtility.SetDirty(m);
        }
        [MenuItem("Arcane Brigade/October/Build all content")]
        public static void Build()
        {
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog"); OctoberRoster manifest = OctoberContent.Roster;
            OctoberArtwork scene = Asset<OctoberArtwork>("Artwork");
            Sprite[] watchers = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/October/Watchers.png", "OctoberWatchers", 4, 4, Enumerable.Range(0, 16).ToArray(), new[] { 0, 265, 455, 690, 1024 }, true);
            scene.Watchers = watchers.Take(12).ToArray(); scene.Door = watchers.Skip(12).Take(4).ToArray();
            scene.Keeper = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/October/Keeper.png", "OctoberKeeper", 4, 1, Enumerable.Range(0, 4).ToArray());
            string floorPath = "Assets/ArcaneBrigade/Art/October/NightmareFloor.png";
            AssetDatabase.ImportAsset(floorPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter floorImport = (TextureImporter)AssetImporter.GetAtPath(floorPath);
            floorImport.textureType = TextureImporterType.Default; floorImport.alphaSource = TextureImporterAlphaSource.None;
            floorImport.maxTextureSize = 4096; floorImport.wrapMode = TextureWrapMode.Clamp; floorImport.SaveAndReimport();
            scene.Floor = AssetDatabase.LoadAssetAtPath<Texture2D>(floorPath); EditorUtility.SetDirty(scene);
            OutsideStageAssets outside = OutsideStageAssets.Current;
            outside.Boss.Body = scene.Keeper[0]; outside.Boss.AnimationFrames = scene.Keeper;
            Art(outside.Boss, scene.Keeper); EditorUtility.SetDirty(outside.Boss);
            var heroes = c.Champions.Where(u => !manifest.newHeroes.Any(r => r.id == u.Id)).ToList();
            Sprite[][] heroSheets = Enumerable.Range(1, 6).Select(i => Sheet("Heroes", i)).ToArray();
            Sprite[][] reactions = Enumerable.Range(1, 6).Select(i => Sheet("Reactions", i)).ToArray();
            CharacterActingLibrary acting = Resources.Load<CharacterActingLibrary>("CharacterActingLibrary");
            var entries = acting.Entries.Where(e => !manifest.newHeroes.Any(r => r.id == e.Id)).ToList();
            for (int i = 0; i < manifest.newHeroes.Length; i++)
            {
                OctoberUnitRow r = manifest.newHeroes[i]; UnitDefinition u = Asset<UnitDefinition>(r.id);
                EditorUtility.CopySerialized(c.Champions[0], u);
                u.Id = r.id; u.DisplayName = r.name; u.ReferenceHero = r.reference == "Original" ? "Original:" + r.id : r.reference;
                u.Description = OctoberContent.HeroCopy(u.ReferenceHero); u.HeroMechanic = HeroMechanic.None;
                u.Weapon = c.Weapon(r.weapon); if (u.Weapon == null) throw new InvalidOperationException("Missing weapon " + r.weapon);
                u.RecruitThemes = new[] { r.theme, r.theme == "archive" ? "clock" : r.theme == "orchard" ? "hearth" : "oddity" };
                u.Tags = u.RecruitThemes; u.ChampionModifiers = Array.Empty<StatModifier>(); u.CompanionModifiers = Array.Empty<StatModifier>();
                u.Health = 85 + i % 4 * 10; u.Speed = 4.5f; u.Radius = .25f; u.BodyHeight = .85f; u.Tint = Color.white;
                u.UnlockWave = i < 8 ? 3 + i : i < 18 ? 8 + (i - 8) / 2 : 14 + i - 18;
                u.Passive = PassiveKind.None; u.BuildMorphStage = 0; u.Material = i % 3 == 0 ? BodyMaterial.Fabric : i % 3 == 1 ? BodyMaterial.Metal : BodyMaterial.Wood;
                Art(u, heroSheets[i / 4].Skip(i % 4 * 4).Take(4).ToArray()); heroes.Add(u); EditorUtility.SetDirty(u);
                Sprite[] reaction = reactions[i / 4].Skip(i % 4 * 4).Take(4).ToArray();
                entries.Add(new CharacterActingEntry { Id = u.Id, ReferenceHeight = u.Body.bounds.size.y, Curious = new[] { u.Body, reaction[0], reaction[0], u.Body }, Cheer = new[] { u.Body, reaction[1], reaction[1], u.Body }, Hurt = new[] { reaction[2], reaction[2], u.Body }, Defeat = new[] { reaction[2], reaction[3], reaction[3] } });
            }
            acting.Entries = entries.ToArray(); EditorUtility.SetDirty(acting);
            foreach (OctoberUnitRow row in manifest.heroes)
            {
                UnitDefinition u = heroes.Find(a => a.Id == row.id);
                u.ReferenceHero = row.reference == "Original" ? "Original:" + row.id : row.reference;
                if (row.@new) u.Description = OctoberContent.HeroCopy(u.ReferenceHero);
                EditorUtility.SetDirty(u);
            }
            c.Champions = heroes.ToArray();
            var allies = c.Companions.Where(u => !manifest.newAllies.Any(r => r.id == u.Id)).ToList();
            Sprite[][] allySheets = Enumerable.Range(1, 5).Select(i => Sheet("Allies", i)).ToArray();
            for (int i = 0; i < manifest.newAllies.Length; i++)
            {
                OctoberUnitRow r = manifest.newAllies[i]; UnitDefinition u = Asset<UnitDefinition>(r.id);
                EditorUtility.CopySerialized(c.Companions[i % 8], u);
                u.Id = r.id; u.DisplayName = r.name; u.ReferenceHero = ""; u.ExclusiveHero = "";
                u.RecruitThemes = Array.Empty<string>(); u.Tags = new[] { new[] { "hearth", "clock", "archive", "orchard", "tidal", "astral", "frost", "oddity" }[i % 8] };
                u.CompanionModifiers = Array.Empty<StatModifier>(); u.ChampionModifiers = Array.Empty<StatModifier>();
                u.UnlockWave = 2 + i % 5; u.Rarity = i < 7 ? ItemRarity.Rare : ItemRarity.Uncommon;
                u.SpecialRecruit = i < 7; u.RecruitThemeLocked = true; u.Passive = PassiveKind.None; u.BodyHeight = .75f; u.BodyScale = 1;
                u.Health = 100; u.Speed = 4; u.Radius = .22f; u.BuildMorphStage = 0; u.Weapon = c.Weapons.First(w => !w.SpecialOnly && w.Kind != WeaponKind.Contact);
                u.Price = 24 + i % 4 * 8; u.Tint = Color.white;
                Art(u, allySheets[i / 4].Skip(i % 4 * 4).Take(4).ToArray()); allies.Add(u); EditorUtility.SetDirty(u);
            }
            foreach (OctoberUnitRow row in manifest.allies)
            {
                UnitDefinition u = allies.Find(a => a.Id == row.id); u.ReferenceCompanion = row.reference == "Original" ? "Original:" + row.id : row.reference;
                var tags = u.Tags.ToList();
                for (int i = 0; i < OctoberContent.BondIds.Length; i++) if (row.bond.Contains(OctoberContent.ReferenceBonds[i]) && !tags.Contains(OctoberContent.BondIds[i])) tags.Add(OctoberContent.BondIds[i]);
                u.Tags = tags.ToArray(); u.Description = OctoberCompanions.Copy(u.ReferenceCompanion);
                u.ExclusiveHero = row.reference == "Little Cabbage" || row.reference == "Big Cabbage" ? "Jade Cabbage" : "";
                if (u.Id == "CarouselPony") { u.SpecialRecruit = true; u.Tags = new[] { "clock", "tidal", "knight" }; u.Weapon = c.Weapon("InkPistol"); }
                if (u.Id == "LittleSprout" || u.Id == "GrandSprout" || u.Id == "BristleBall") u.CollisionDamage = u.Id == "LittleSprout" ? 12 : 24;
                EditorUtility.SetDirty(u);
            }
            c.Companions = allies.ToArray();
            OctoberRecipes.Install(c);
            var bonds = c.Bonds.Where(b => !OctoberContent.BondIds.Contains(b.Id) && b.Id != "lantern-guild" && b.Id != "rain-script").ToList();
            Sprite[] bondArt = Sheet("Bonds", 1).Concat(Sheet("Bonds", 2)).ToArray();
            ItemEffect[] effects = { ItemEffect.Dodge, ItemEffect.AttackRange, ItemEffect.Health, ItemEffect.AttackSpeed, ItemEffect.Damage, ItemEffect.Luck, ItemEffect.Heal, ItemEffect.Income, ItemEffect.Crit, ItemEffect.Crit, ItemEffect.Discount, ItemEffect.CollisionDamage, ItemEffect.CollisionDamage, ItemEffect.ChampionDamage, ItemEffect.EnemyCount, ItemEffect.ChestQuality, ItemEffect.Armor, ItemEffect.ScoreMultiplier, ItemEffect.MoveSpeed };
            float[] values = { .08f, .12f, 8, .08f, .15f, .1f, .3f, 4, .06f, .08f, .1f, 5, 2, .15f, .06f, .1f, .12f, .2f, .08f };
            for (int i = 0; i < effects.Length; i++) bonds.Add(new BondDefinition { Id = OctoberContent.BondIds[i], Name = OctoberContent.BondNames[i], Threshold = i == 4 ? 4 : i == 10 ? 5 : i == 3 || i == 13 || i == 14 || i == 15 ? 3 : 2, Effect = effects[i], Value = values[i], Icon = bondArt[i] });
            bonds.Add(new BondDefinition { Id = "lantern-guild", Name = "Lantern Guild", Threshold = 2, Effect = ItemEffect.AttackRange, Value = .1f, Icon = bondArt[19] });
            bonds.Add(new BondDefinition { Id = "rain-script", Name = "Rain Script", Threshold = 2, Effect = ItemEffect.Heal, Value = .35f, Icon = bondArt[20] });
            c.Bonds = bonds.ToArray();
            foreach (string id in new[] { "LanternWeaver", "HourglassStag" }) { UnitDefinition u = c.Unit(id); u.RecruitThemes = u.RecruitThemes.Concat(new[] { "lantern-guild" }).Distinct().ToArray(); EditorUtility.SetDirty(u); }
            foreach (UnitDefinition u in c.Companions.Where(u => u.Id == "TeacupCricket" || u.Id == "SpindleWren")) { u.Tags = u.Tags.Concat(new[] { "lantern-guild" }).Distinct().ToArray(); EditorUtility.SetDirty(u); }
            c.Unit("RibbonOtter").Tags = new[] { "tidal", "rain-script" }; EditorUtility.SetDirty(c.Unit("RibbonOtter"));
            var items = c.Items.Where(i => !i.Id.StartsWith("OctoberRelic") && string.IsNullOrEmpty(i.ReferenceName)).ToList();
            ItemDefinition[] references = ReferenceRelicCatalog.Create();
            foreach (ItemDefinition item in references)
            {
                ItemDefinition prior = c.Items.FirstOrDefault(i => i.ReferenceName == item.ReferenceName); if (prior?.Icon != null) item.Icon = prior.Icon;
            }
            for (int sheet = 3; sheet <= 5; sheet++)
            {
                Sprite[] art = Sheet("Relics", sheet);
                for (int cell = 0; cell < 16 && (sheet - 1) * 16 + cell < references.Length; cell++) references[(sheet - 1) * 16 + cell].Icon = art[cell];
                if (sheet == 5)
                    for (int i = 0; i < 4; i++) items.Add(new ItemDefinition { Id = "OctoberRelic" + i, Name = new[] { "Lantern Stitch", "Rain Receipt", "Clockseed Brooch", "Porcelain Promise" }[i], Description = new[] { "Team reach +10%. Team attack speed -5%.", "Regenerate 0.5 HP per second. Wave income -2.", "Team contact damage +5. Team movement speed -4%.", "Hearts heal 2 more HP. Maximum HP +10." }[i], Icon = art[12 + i], Price = 26 + i * 4, AltarPrice = 3, ShopWeight = 65, MaximumStacks = 4, UnlockWave = 3, Rarity = ItemRarity.Rare, Pools = i % 2 == 0 ? ShopPool.Gold : ShopPool.Eggs, Modifiers = new[] { new StatModifier { Effect = new[] { ItemEffect.AttackRange, ItemEffect.Heal, ItemEffect.CollisionDamage, ItemEffect.HeartHealing }[i], Scope = i == 1 || i == 3 ? ModifierScope.Champion : ModifierScope.Team, Value = new[] { .1f, .5f, 5, 2 }[i] }, new StatModifier { Effect = new[] { ItemEffect.AttackSpeed, ItemEffect.Income, ItemEffect.MoveSpeed, ItemEffect.Health }[i], Scope = i == 1 ? ModifierScope.Run : i == 3 ? ModifierScope.Champion : ModifierScope.Team, Value = new[] { -.05f, -2, -.04f, 10 }[i] } } });
            }
            if (references.Any(i => i.Icon == null)) throw new InvalidOperationException("All 76 reference relics need original art");
            items.AddRange(references); c.Items = items.ToArray();
            foreach (ItemDefinition item in c.Items) item.ShopThemes = OctoberRecipes.ItemThemes(item);
            c.FirstWaveSeconds = c.WaveSeconds = 45; c.WaveIncrement = 0; c.TierDamageMultiplier = 2.7f;
            EditorUtility.SetDirty(c); AssetDatabase.SaveAssets();
            Debug.Log("OCTOBER_CONTENT " + c.Champions.Length + " heroes / " + c.Companions.Length + " allies / " + c.Items.Length + " relics / " + c.Bonds.Length + " bonds");
        }
        public static void Verify() { Build(); OctoberChecks.Run(); }
        public static void BuildWindows() { Build(); Regressions(); OctoberAudit.Export(); FullRunBuilder.BuildWindows(); }
        public static void Regressions()
        {
            OctoberChecks.Run(); SecretEventChecks.Run(); NightmareChecks.Run(); NightmareVisualChecks.Run(); NightmareFarewellChecks.Run();
        }
    }
}
