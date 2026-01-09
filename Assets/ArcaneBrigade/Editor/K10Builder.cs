using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K10Builder
    {
        private sealed class CharacterSpec
        {
            public string Id; public string Name; public int Index;
            public CharacterSpec(string id, string name, int index) { Id = id; Name = name; Index = index; }
        }
        private sealed class WeaponSpec
        {
            public string Id; public string Name; public WeaponMotion Motion; public int Index;
            public WeaponSpec(string id, string name, WeaponMotion motion, int index) { Id = id; Name = name; Motion = motion; Index = index; }
        }
        private static readonly CharacterSpec[] Characters =
        {
            new CharacterSpec("BrassAcorn", "Hạt Dẻ Bưu Tá", 0),
            new CharacterSpec("BreadCrab", "Cua Bánh Mì", 1),
            new CharacterSpec("KettleBat", "Dơi Ấm Đun", 2),
            new CharacterSpec("BookmarkGolem", "Golem Kẹp Sách", 3),
            new CharacterSpec("TinTick", "Ve Hộp Thiếc", 4),
            new CharacterSpec("SpringDuck", "Vịt Lò Xo", 5),
            new CharacterSpec("ButtonBeetle", "Bọ Cúc Áo", 6),
            new CharacterSpec("StapleMole", "Chuột Dập Ghim", 7),
            new CharacterSpec("SnowSpoon", "Muỗng Tuyết", 8),
            new CharacterSpec("IceCreamOwl", "Cú Kem", 9),
            new CharacterSpec("FridgeWisp", "Ma Tủ Lạnh", 10),
            new CharacterSpec("PearMoth", "Bướm Lê", 11),
            new CharacterSpec("CitrusToad", "Cóc Cam", 12),
            new CharacterSpec("RadishJester", "Củ Cải Hề", 13),
            new CharacterSpec("SeedHedgehog", "Nhím Hạt", 14),
            new CharacterSpec("MoonJelly", "Sứa Trăng", 15),
            new CharacterSpec("StarSock", "Tất Sao", 16),
            new CharacterSpec("CometKite", "Diều Sao Chổi", 17),
            new CharacterSpec("EclipseEgg", "Trứng Nhật Thực", 18),
            new CharacterSpec("OrbitMoth", "Bướm Quỹ Đạo", 19),
            new CharacterSpec("DreamCompass", "La Bàn Mộng", 20),
            new CharacterSpec("ShellPostman", "Ốc Bưu Tá", 21),
            new CharacterSpec("CoralTeacup", "Tách San Hô", 22),
            new CharacterSpec("BubbleKoi", "Cá Bong Bóng", 23),
            new CharacterSpec("AnchorSnail", "Ốc Mỏ Neo", 24),
            new CharacterSpec("SailSquid", "Mực Cánh Buồm", 25),
            new CharacterSpec("SockCyclops", "Tất Độc Nhãn", 26),
            new CharacterSpec("DiceCrab", "Cua Xúc Xắc", 27),
            new CharacterSpec("MaskPepper", "Ớt Mặt Nạ", 28),
            new CharacterSpec("CrayonGremlin", "Quỷ Sáp Màu", 29),
            new CharacterSpec("MirrorNewt", "Thằn Lằn Gương", 30),
            new CharacterSpec("RibbonSpider", "Nhện Ruy Băng", 31),
        };
        private static readonly WeaponSpec[] Arms =
        {
            new WeaponSpec("SpoonSabre", "Spoon Sabre", WeaponMotion.Slash, 0),
            new WeaponSpec("ToastCleaver", "Toast Cleaver", WeaponMotion.Slam, 1),
            new WeaponSpec("LollipopLance", "Lollipop Lance", WeaponMotion.Thrust, 2),
            new WeaponSpec("PaperFan", "Paper Fan", WeaponMotion.Slash, 3),
            new WeaponSpec("CorkscrewRapier", "Corkscrew Rapier", WeaponMotion.Thrust, 4),
            new WeaponSpec("TeabagFlail", "Teabag Flail", WeaponMotion.Slam, 5),
            new WeaponSpec("UmbrellaHook", "Umbrella Hook", WeaponMotion.Slash, 6),
            new WeaponSpec("PizzaWheel", "Pizza Wheel", WeaponMotion.Slash, 7),
            new WeaponSpec("MoonScythe", "Moon Scythe", WeaponMotion.Slash, 8),
            new WeaponSpec("ForkTrident", "Fork Trident", WeaponMotion.Thrust, 9),
            new WeaponSpec("MaracaHammer", "Maraca Hammer", WeaponMotion.Slam, 10),
            new WeaponSpec("TapeWhip", "Tape Whip", WeaponMotion.Slash, 11),
            new WeaponSpec("ButtonBow", "Button Bow", WeaponMotion.Bow, 12),
            new WeaponSpec("LeafCrossbow", "Leaf Crossbow", WeaponMotion.Bow, 13),
            new WeaponSpec("InkPistol", "Ink Pistol", WeaponMotion.Recoil, 14),
            new WeaponSpec("BubbleWand", "Bubble Wand", WeaponMotion.Cast, 15),
            new WeaponSpec("StarKazoo", "Star Kazoo", WeaponMotion.Cast, 16),
            new WeaponSpec("ShellCannon", "Shell Cannon", WeaponMotion.Recoil, 17),
            new WeaponSpec("AcornSling", "Acorn Sling", WeaponMotion.Bow, 18),
            new WeaponSpec("CrescentBoomerang", "Crescent Boomerang", WeaponMotion.Recoil, 19),
            new WeaponSpec("DartCompass", "Dart Compass", WeaponMotion.Recoil, 20),
            new WeaponSpec("LanternStaff", "Lantern Staff", WeaponMotion.Cast, 21),
            new WeaponSpec("SnowGlobeOrb", "Snow Globe Orb", WeaponMotion.Cast, 22),
            new WeaponSpec("RibbonHarp", "Ribbon Harp", WeaponMotion.Cast, 23),
        };
        private static readonly string[] Tags = { "hearth", "archive", "clock", "frost", "orchard", "astral", "tidal", "oddity" };
        private static readonly string[] BondNames = { "Bếp Ấm", "Thư Viện", "Bộ Máy", "Sương Giá", "Vườn Mộng", "Tinh Tú", "Thuỷ Triều", "Đồ Dị" };
        private static StatModifier Modifier(ItemEffect effect, float value, ModifierScope? scope = null)
            => new StatModifier { Effect = effect, Value = value, Scope = scope ?? RunModifierRules.DefaultScope(effect) };

        private static T Asset<T>(string id) where T : ScriptableObject
        {
            string path = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); }
            return asset;
        }

        public static void Apply()
        {
            K9Builder.Apply();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (UnitDefinition champion in catalog.Champions)
            {
                var modifiers = champion.ChampionModifiers.Where(modifier => modifier.Effect < ItemEffect.ScoreMultiplier).ToList();
                if (champion.Id == "LanternKnight") { modifiers.Add(Modifier(ItemEffect.CollisionDamage, 2, ModifierScope.Champion)); modifiers.Add(Modifier(ItemEffect.BulletBounce, 0.08f, ModifierScope.Companions)); modifiers.Add(Modifier(ItemEffect.EnemyHealth, 0.08f)); }
                if (champion.Id == "Postmaster") { modifiers.Add(Modifier(ItemEffect.Income, 5)); modifiers.Add(Modifier(ItemEffect.BulletSplit, 1, ModifierScope.Champion)); modifiers.Add(Modifier(ItemEffect.EnemyCount, 0.08f)); }
                if (champion.Id == "DrowsySeer") { modifiers.Add(Modifier(ItemEffect.Dodge, 0.06f)); modifiers.Add(Modifier(ItemEffect.BulletPierce, 1, ModifierScope.Champion)); modifiers.Add(Modifier(ItemEffect.EnemySpeed, 0.05f)); }
                champion.ChampionModifiers = modifiers.ToArray(); EditorUtility.SetDirty(champion);
            }
            var weapons = catalog.Weapons.ToList();
            foreach (WeaponSpec spec in Arms)
            {
                WeaponDefinition weapon = Asset<WeaponDefinition>(spec.Id);
                weapon.Id = spec.Id; weapon.Sprite = Import(spec.Id, false)[0]; weapon.Motion = spec.Motion;
                weapon.Kind = spec.Index < 12 ? WeaponKind.Melee : WeaponKind.Projectile;
                weapon.Element = (DamageElement)(spec.Index % 4);
                weapon.Damage = spec.Index < 12 ? 12 + spec.Index % 4 * 2 : 9 + spec.Index % 3 * 2;
                weapon.Range = spec.Index < 12 ? 1.6f + spec.Index % 4 * 0.25f : 7;
                weapon.Cooldown = 0.65f + spec.Index % 5 * 0.12f;
                weapon.Windup = spec.Motion == WeaponMotion.Slam ? 0.24f : 0.15f;
                weapon.Recovery = 0.18f; weapon.VisualWidth = spec.Index < 12 ? 0.76f : 0.64f;
                weapon.HasMuzzleOffset = true;
                weapon.ProjectileMuzzleOffset = (Vector2.Scale(weapon.Sprite.rect.size, new Vector2(0.94f, 0.5f)) - weapon.Sprite.pivot) / weapon.Sprite.pixelsPerUnit * (weapon.VisualWidth / weapon.Sprite.bounds.size.x);
                weapon.ProjectileSpeed = 4.5f + spec.Index % 4;
                weapon.ProjectileLifetime = 2.6f;
                weapon.ProjectileCount = spec.Id == "ShellCannon" ? 3 : spec.Id == "RibbonHarp" ? 2 : 1;
                weapon.ProjectilePierce = spec.Id == "LeafCrossbow" || spec.Id == "DartCompass" ? 2 : 0;
                weapon.ProjectileSplit = spec.Id == "SnowGlobeOrb" ? 2 : 0;
                weapon.ProjectileBounceChance = spec.Id == "BubbleWand" ? 0.35f : 0;
                weapon.ProjectileRadius = spec.Id == "ShellCannon" ? 0.085f : 0.055f;
                weapon.ProjectileVisualWidth = weapon.ProjectileRadius * 4;
                weapon.ProjectileSprite = spec.Id == "CrescentBoomerang" || spec.Id == "DartCompass" ? weapon.Sprite : catalog.ProjectileDisc;
                weapon.ProjectileColor = ArcadeEffects.ElementColor(weapon.Element);
                weapon.Family = spec.Id == "CrescentBoomerang" ? ExpandedWeapon.Fish : spec.Id == "RibbonHarp" ? ExpandedWeapon.MusicBox : spec.Id == "TapeWhip" ? ExpandedWeapon.Scissors : spec.Id == "MaracaHammer" ? ExpandedWeapon.Hammer : ExpandedWeapon.None;
                weapon.SwingAngle = spec.Motion == WeaponMotion.Thrust ? 20 : spec.Motion == WeaponMotion.Slam ? 100 : 80;
                weapon.PushDistance = spec.Motion == WeaponMotion.Slam ? 0.6f : 0.25f;
                weapon.BodyAttackTilt = spec.Motion == WeaponMotion.Slam ? 28 : 16;
                weapon.MountDistance = 0.34f; weapon.MuzzleDistance = 0.55f; weapon.ConeDot = spec.Motion == WeaponMotion.Thrust ? 0.8f : 0.15f;
                if (!weapons.Contains(weapon)) weapons.Add(weapon);
                EditorUtility.SetDirty(weapon);
            }
            catalog.Weapons = weapons.ToArray();
            var companions = catalog.Companions.ToList();
            foreach (CharacterSpec spec in Characters)
            {
                UnitDefinition unit = Asset<UnitDefinition>(spec.Id);
                unit.Id = spec.Id; unit.DisplayName = spec.Name;
                unit.AnimationFrames = Import(spec.Id, true); unit.Body = unit.AnimationFrames[0];
                unit.Weapon = catalog.Weapon(Arms[spec.Index % Arms.Length].Id);
                unit.BodyHeight = 0.68f + spec.Index % 4 * 0.045f; unit.Radius = 0.22f; unit.Speed = 4.2f + spec.Index % 3 * 0.2f;
                unit.Health = 100; unit.Price = 18 + spec.Index % 5 * 3; unit.UnlockWave = spec.Index < 10 ? 1 : spec.Index < 22 ? 3 : 6;
                unit.CriticalChance = 0.05f; unit.Passive = (PassiveKind)(spec.Index % 5); unit.PassiveStrength = 0.5f;
                ItemEffect benefit = (ItemEffect)((int)ItemEffect.ScoreMultiplier + spec.Index % K10StatRules.Names.Length);
                float amount = benefit == ItemEffect.Income ? 2 : benefit == ItemEffect.BulletPierce || benefit == ItemEffect.BulletSplit ? 1 : benefit == ItemEffect.MeleeDamage || benefit == ItemEffect.RangedDamage || benefit == ItemEffect.CollisionDamage || benefit == ItemEffect.HeartHealing ? 1 : 0.025f;
                unit.CompanionModifiers = new[] { Modifier(benefit, amount), Modifier(ItemEffect.Health, -1) };
                if (benefit == ItemEffect.EnemyCount || benefit == ItemEffect.EnemyHealth || benefit == ItemEffect.EnemySpeed)
                    unit.CompanionModifiers = new[] { Modifier(benefit, 0.1f), Modifier(ItemEffect.ScoreMultiplier, 0.1f), Modifier(ItemEffect.Health, -1) };
                unit.Description = "Tự đánh, bất tử; đặc tính giữ khi còn trong roster của lượt. " + RunModifierRules.DescribeCompanion(unit, 1);
                if (!companions.Contains(unit)) companions.Add(unit);
                EditorUtility.SetDirty(unit);
            }
            catalog.Companions = companions.ToArray();
            for (int index = 0; index < catalog.Companions.Length; index++)
            {
                string tag = catalog.Companions[index].Id switch
                {
                    "TeapotPilgrim" or "CandleSnail" or "BreadCrab" or "KettleBat" or "SockCyclops" => "hearth",
                    "MailboxImp" or "BrassAcorn" or "BookmarkGolem" or "DreamCompass" or "CrayonGremlin" => "archive",
                    "TinTick" or "SpringDuck" or "ButtonBeetle" or "StapleMole" or "EclipseEgg" => "clock",
                    "HoodWisp" or "SnowSpoon" or "IceCreamOwl" or "FridgeWisp" => "frost",
                    "MushroomLibrarian" or "PearMoth" or "CitrusToad" or "RadishJester" or "SeedHedgehog" => "orchard",
                    "PillowOracle" or "MoonJelly" or "StarSock" or "CometKite" or "OrbitMoth" => "astral",
                    "ShellPostman" or "CoralTeacup" or "BubbleKoi" or "AnchorSnail" or "SailSquid" => "tidal",
                    _ => "oddity"
                };
                catalog.Companions[index].Tags = new[] { tag };
                EditorUtility.SetDirty(catalog.Companions[index]);
            }
            var bonds = new List<BondDefinition>();
            for (int index = 0; index < Tags.Length; index++)
            {
                BondDefinition prior = Array.Find(catalog.Bonds, bond => bond.Id == Tags[index]);
                bonds.Add(prior ?? new BondDefinition { Id = Tags[index], Name = BondNames[index], Threshold = 2, Effect = index == 4 ? ItemEffect.Luck : index == 5 ? ItemEffect.BulletSize : index == 6 ? ItemEffect.BulletBounce : ItemEffect.Dodge, Value = 0.04f, ExtraBonuses = new[] { new BondThresholdBonus { Threshold = 4, Value = 0.04f }, new BondThresholdBonus { Threshold = 6, Value = 0.08f } } });
                if (index >= 4) bonds[index].Icon = Import(new[] { "BondOrchard", "BondAstral", "BondTidal", "BondOddity" }[index - 4], false)[0];
            }
            catalog.Bonds = bonds.ToArray();
            var items = catalog.Items.ToList();
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
            {
                if (effect < ItemEffect.ScoreMultiplier) continue;
                string id = "k10-" + effect.ToString().ToLowerInvariant();
                ItemDefinition item = items.Find(candidate => candidate.Id == id) ?? new ItemDefinition { Id = id };
                item.Name = "Khế ước · " + K10StatRules.Name(effect);
                float amount = effect == ItemEffect.Income ? 4 : effect == ItemEffect.BulletPierce || effect == ItemEffect.BulletSplit ? 1 : effect == ItemEffect.MeleeDamage || effect == ItemEffect.RangedDamage || effect == ItemEffect.CollisionDamage || effect == ItemEffect.HeartHealing ? 2 : 0.08f;
                item.Modifiers = new[] { Modifier(effect, amount), Modifier(ItemEffect.Health, -1) };
                if (effect == ItemEffect.EnemyCount || effect == ItemEffect.EnemyHealth || effect == ItemEffect.EnemySpeed)
                    item.Modifiers = new[] { Modifier(effect, 0.1f), Modifier(ItemEffect.ScoreMultiplier, 0.2f), Modifier(ItemEffect.Health, -1) };
                item.Effect = effect; item.Value = amount; item.Price = 18 + ((int)effect % 4) * 3;
                item.Rarity = (ItemRarity)(1 + (int)effect % 3); item.Icon = catalog.Weapon(Arms[((int)effect - (int)ItemEffect.ScoreMultiplier) % Arms.Length].Id).Sprite;
                item.UnlockWave = effect == ItemEffect.BulletSplit || effect == ItemEffect.BulletBounce ? 3 : 1;
                item.ShopWeight = 50; item.Pools = ShopPool.Both; item.AltarPrice = 2 + (int)item.Rarity; item.AltarCurrency = (int)effect % 3 == 0 ? ShopCurrency.Health : ShopCurrency.Eggs;
                item.Description = "Cộng dồn trong lượt, qua wave và vô tận; lượt mới reset.\n" + RunModifierRules.Describe(item, 1);
                if (!items.Contains(item)) items.Add(item);
            }
            catalog.Items = items.ToArray(); catalog.StatRevision = 1;
            RunModifierRules.Validate(catalog); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            Debug.Log("K10_CONTENT_APPLIED companions=" + catalog.Companions.Length + " weapons=" + catalog.Weapons.Length);
        }

        private static Sprite[] Import(string id, bool character)
        {
            string path = "Assets/ArcaneBrigade/Art/K10/" + id + ".png";
            if (!File.Exists(path)) throw new InvalidOperationException("Missing original K10 art: " + id);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.filterMode = FilterMode.Bilinear; importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7, compressionQuality = 100 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6, compressionQuality = 100 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6, compressionQuality = 100 });
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 4) throw new InvalidOperationException("K10 requires real alpha: " + id);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var prior = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new List<SpriteRect>(); int count = character ? 4 : 1;
            int bottom = texture.height, top = 0;
            for (int vertical = 0; vertical < texture.height; vertical++)
                for (int horizontal = 0; horizontal < texture.width; horizontal++)
                    if (pixels[vertical * texture.width + horizontal].a > 128) { bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical); }
            for (int index = 0; index < count; index++)
            {
                int left = index * texture.width / count, right = (index + 1) * texture.width / count;
                if (!character)
                {
                    left = texture.width; right = 0;
                    for (int vertical = bottom; vertical <= top; vertical++)
                        for (int horizontal = 0; horizontal < texture.width; horizontal++)
                            if (pixels[vertical * texture.width + horizontal].a > 128) { left = Mathf.Min(left, horizontal); right = Mathf.Max(right, horizontal + 1); }
                }
                string name = id + (index == 0 ? "" : "_" + index);
                rectangles.Add(new SpriteRect { name = name, rect = new Rect(left, bottom, right - left, top - bottom + 1), alignment = SpriteAlignment.Custom, pivot = character ? Vector2.one * 0.5f : new Vector2(0.18f, 0.5f), spriteID = prior.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() });
            }
            provider.SetSpriteRects(rectangles.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
            return rectangles.Select(rectangle => sprites[rectangle.name]).ToArray();
        }

        public static void Build()
        {
            Apply(); K10Checks.Run(); K9Checks.Run();
            K8Builder.Check();
            K7BalanceDiagnostics.Run();
            Directory.CreateDirectory("PreviewExports/K10");
            File.Copy("PreviewExports/K7/Movement_Bot_Balance.csv", "PreviewExports/K10/Movement_Bot_Balance.csv", true);
            FullRunBuilder.BuildWindows();
            Directory.CreateDirectory("PreviewExports/K10");
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K10/Windows_Build.txt", true);
            Debug.Log("K10_WINDOWS_CANDIDATE_BUILT");
        }

        public static void FinalizeCandidate()
        {
            Apply(); K10Checks.Run(); K9Checks.Run(); K8Builder.Check();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K10/Windows_Build.txt", true);
            Debug.Log("K10_FINAL_CANDIDATE_BUILT");
        }
    }
}
