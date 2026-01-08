using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K5Builder
    {
        public static void ApplyAndCheck() { Apply(); K5Checks.Run(); }
        public static void GateAndBuild()
        {
            Apply(); K5Checks.Run();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            CheckpointChecks.Run(catalog.Arena); SquadCheckpointChecks.Run(catalog.Arena); K1Builder.CheckExisting();
            K2Checks.Run(); K3Checks.Run(); K32Checks.Run(); K32AltarChecks.Run(); K3FinalChecks.Run();
            VisualQualityChecks.Run(); K4FinalChecks.Run(); FullRunBuilder.BuildWindows();
        }
        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            string[] weaponIds = { "CloudScissors", "SleepCamera", "TinFish", "DreamIron", "RubberHammer", "AlarmMortar", "MusicBox", "TeapotPopgun", "CrescentBow" };
            Dictionary<string, Sprite> weapons = Import("ExpandedWeapons_v2", weaponIds, 3, new[] { 0f, 0.333f, 0.667f, 1f }, false);
            string[] propIds = { "StageBell", "ToyCart", "InkBottle", "HangingHammer", "SleepMirror", "FloorMusicBox", "tea-coaster", "scratch-record", "treasure-ribbon", "patch-spring", "thermal-ink", "repair-coupon" };
            Dictionary<string, Sprite> props = Import("ExpandedProps_v1", propIds, 3, new[] { 0f, 0.26f, 0.55f, 0.76f, 1f }, true);
            string[] enemyIds = { "DoorBearer", "MoonMagnet", "SleepMimic", "CurtainWisp" };
            Dictionary<string, Sprite> enemies = Import("ExpandedEnemies_v1", enemyIds, 2, new[] { 0f, 0.55f, 1f }, true);
            catalog.ArenaPropSprites = propIds.Take(6).Select(id => props[id]).ToArray();
            if (catalog.ExpansionRevision == 0)
            {
                var expanded = new List<WeaponDefinition>(catalog.Weapons);
                ExpandedWeapon[] families = { ExpandedWeapon.Scissors, ExpandedWeapon.Camera, ExpandedWeapon.Fish, ExpandedWeapon.Iron, ExpandedWeapon.Hammer, ExpandedWeapon.Alarm, ExpandedWeapon.MusicBox, ExpandedWeapon.Tea, ExpandedWeapon.TetherBow };
                WeaponMotion[] motions = { WeaponMotion.Scissors, WeaponMotion.Camera, WeaponMotion.Recoil, WeaponMotion.Iron, WeaponMotion.Hammer, WeaponMotion.Recoil, WeaponMotion.Music, WeaponMotion.Cast, WeaponMotion.Bow };
                for (int index = 0; index < weaponIds.Length; index++)
                {
                    WeaponDefinition weapon = catalog.Weapon(weaponIds[index]);
                    bool created = weapon == null;
                    if (created) { weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); weapon.Id = weaponIds[index]; AssetDatabase.CreateAsset(weapon, "Assets/ArcaneBrigade/Data/Run/" + weapon.Id + ".asset"); expanded.Add(weapon); }
                    weapon.Family = families[index]; weapon.Motion = motions[index]; weapon.Sprite = weapons[weapon.Id];
                    if (created)
                    {
                        bool melee = index == 0 || index == 3 || index == 4;
                        weapon.Kind = melee ? WeaponKind.Melee : WeaponKind.Projectile;
                        weapon.Damage = index == 4 ? 13 : index == 5 ? 18 : index == 1 ? 6 : 10;
                        weapon.Range = melee ? 2.1f : index == 1 ? 4.4f : 7f;
                        weapon.Cooldown = index == 5 ? 1.8f : index == 1 ? 1.25f : 0.95f;
                        weapon.Windup = index == 4 ? 0.23f : 0.17f; weapon.Recovery = index == 4 ? 0.32f : 0.24f;
                        weapon.ProjectileSpeed = index == 6 ? 5f : 8f; weapon.ProjectileLifetime = 3f;
                        weapon.VisualWidth = 0.85f; weapon.BodyAttackTilt = index == 4 ? 26 : 12;
                        weapon.Element = DamageElement.Physical; weapon.ProjectileColor = index == 2 ? new Color(0.35f, 0.95f, 0.8f) : new Color(1f, 0.8f, 0.35f);
                        weapon.ProjectileSprite = catalog.ProjectileDisc;
                    }
                    weapon.HasMuzzleOffset = true;
                    weapon.ProjectileMuzzleOffset = new Vector2(weapon.VisualWidth * 0.7f, 0);
                    EditorUtility.SetDirty(weapon);
                }
                catalog.Weapons = expanded.ToArray();
                var roster = new List<UnitDefinition>(catalog.Enemies);
                for (int index = 0; index < enemyIds.Length; index++)
                {
                    UnitDefinition enemy = UnityEngine.Object.Instantiate(catalog.Enemies[0]); enemy.Id = enemyIds[index]; enemy.name = enemy.Id;
                    enemy.DisplayName = enemy.Id; enemy.Body = enemies[enemy.Id]; enemy.Behavior = (EnemyBehavior)(index + 1);
                    enemy.Material = index == 0 ? BodyMaterial.Wood : index == 1 ? BodyMaterial.Metal : index == 2 ? BodyMaterial.Wax : BodyMaterial.Fabric;
                    enemy.UnlockWave = 2 + index * 2; enemy.Health = index == 0 ? 75 : 48;
                    enemy.Speed = index == 0 ? 1.3f : index == 2 ? 2.8f : 1.7f; enemy.ContactDamage = 5;
                    enemy.Description = index == 0 ? "Front guard halves directional hits; flank to break through." : index == 1 ? "Pulls nearby coins briefly; never destroys or steals your wallet." : index == 2 ? "Sleeps until approached or hit, then warns before moving." : "Warns a small curtain hazard at your previous position; move out.";
                    AssetDatabase.CreateAsset(enemy, "Assets/ArcaneBrigade/Data/Run/" + enemy.Id + ".asset"); roster.Add(enemy);
                }
                catalog.Enemies = roster.ToArray();
                for (int index = 0; index < catalog.Bosses.Length; index++)
                {
                    catalog.Bosses[index].Motif = (BossMotif)(index % 3);
                    catalog.Bosses[index].Material = index % 3 == 0 ? BodyMaterial.Fabric : index % 3 == 1 ? BodyMaterial.Paper : BodyMaterial.Wood;
                    EditorUtility.SetDirty(catalog.Bosses[index]);
                }
                var items = new List<ItemDefinition>(catalog.Items);
                items.Add(Relic("tea-coaster", "Tea Coaster", "Tea radius +40%; field damage -20%. Burn lets Hammer's third hit release steam. No set required.", SignatureRelicKind.TeaCoaster, ItemEffect.MoveSpeed, -0.02f, 1));
                items.Add(Relic("scratch-record", "Scratch Record", "Every third Music Box note echoes once after 0.3s (35%/70% damage). Attack speed -4% per stack.", SignatureRelicKind.ScratchRecord, ItemEffect.AttackSpeed, -0.04f, 2));
                items.Add(Relic("treasure-ribbon", "Treasure Ribbon", "Chest heals 3 per stack and buffs bow damage 15% per stack for 4s, max 45%. Damage outside chests -3% per stack.", SignatureRelicKind.TreasureRibbon, ItemEffect.Damage, -0.03f, 3));
                items.Add(Relic("patch-spring", "Patch Spring", "One automatic prop charge every 3s; no secondary charge recursion. Companion attack speed -6%.", SignatureRelicKind.PatchSpring, ItemEffect.CompanionAttackSpeed, -0.06f, 1));
                items.Add(Relic("thermal-ink", "Thermal Ink", "Tea/Iron heat on marked enemies adds 40% field damage, target cooldown 0.9s. Crit -3%. Does not add marks.", SignatureRelicKind.ThermalInk, ItemEffect.Crit, -0.03f, 1));
                items.Add(Relic("repair-coupon", "Repair Coupon", "Prop kill heals 1/2/3, shared 0.6s cooldown. Pickup radius -0.1 per stack. No coin/proc loop.", SignatureRelicKind.RepairCoupon, ItemEffect.Pickup, -0.1f, 3));
                catalog.Items = items.ToArray();
                var links = new List<BuildLinkRule>(catalog.BuildLinks);
                links.Add(Link("signature-tea", "TRÀ ÁC MỘNG", new[] { "tea-coaster", "thermal-ink" }, "Tea + Hammer works via burning targets and third-hit steam; Coaster widens weaker tea; Thermal links marks to heat with cooldown."));
                links.Add(Link("signature-chorus", "DÀN HỢP XƯỚNG", new[] { "scratch-record", "carbon-ribbon" }, "Music Box + Record echoes every third note. Music extends live marks; Carbon bell exposes targets. Each echo ends, never echoes itself."));
                links.Add(Link("signature-treasure", "ĐOÀN KHO BÁU", new[] { "treasure-ribbon", "harvest-receipt" }, "Bow + Ribbon gains a short chest window, healing without a set. Receipt connects separate coin pickups to bell cadence."));
                links.Add(Link("signature-toys", "XƯỞNG ĐỒ CHƠI", new[] { "patch-spring", "repair-coupon" }, "Spring + arena props charges automatically; Coupon turns prop kills into capped healing. Swept cart cannot hit twice."));
                catalog.BuildLinks = links.ToArray(); catalog.ExpansionRevision = 1;
            }
            foreach (WeaponDefinition weapon in catalog.Weapons)
                if (weapons.TryGetValue(weapon.Id, out Sprite sprite)) { weapon.Sprite = sprite; EditorUtility.SetDirty(weapon); }
            foreach (ItemDefinition item in catalog.Items) if (props.TryGetValue(item.Id, out Sprite icon)) item.Icon = icon;
            if (catalog.ContentRevision == 0) catalog.Item("scratch-record").Description = "Every third Music Box note echoes once after 0.3s (35%/70% damage). Bell primary wave deals 25%/50% extra to live marked targets; echo never recurses. Attack speed -4% per stack.";
            RunModifierRules.Validate(catalog); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }

        private static ItemDefinition Relic(string id, string name, string description, SignatureRelicKind role, ItemEffect penalty, float value, int max)
        {
            return new ItemDefinition { Id = id, Name = name, Description = description, SignatureRole = role, Price = 32, UnlockWave = 2, Rarity = ItemRarity.Uncommon, ShopWeight = 55, AltarPrice = 3, Pools = ShopPool.Both, Stacking = max == 1 ? StackPolicy.Unique : StackPolicy.Additive, MaximumStacks = max, Modifiers = new[] { new StatModifier { Effect = penalty, Scope = RunModifierRules.DefaultScope(penalty), Value = value } } };
        }
        private static BuildLinkRule Link(string id, string name, string[] items, string description) => new BuildLinkRule { Id = id, Name = name, ItemIds = items, Description = description };

        private static Dictionary<string, Sprite> Import(string file, string[] names, int columns, float[] rows, bool centered)
        {
            string path = "Assets/ArcaneBrigade/Art/QualityHD/" + file + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.isReadable = true; importer.SaveAndReimport();
            var factory = new SpriteDataProviderFactories(); factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> existing = provider.GetSpriteRects().ToDictionary(rect => rect.name, rect => rect.spriteID);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            var rectangles = new List<SpriteRect>();
            for (int index = 0; index < names.Length; index++)
            {
                int minX = texture.width * (index % columns) / columns; int maxX = texture.width * (index % columns + 1) / columns;
                int minY = Mathf.RoundToInt(texture.height * (1 - rows[index / columns + 1])); int maxY = Mathf.RoundToInt(texture.height * (1 - rows[index / columns]));
                int left = maxX, right = minX, bottom = maxY, top = minY;
                for (int vertical = minY; vertical < maxY; vertical++)
                    for (int horizontal = minX; horizontal < maxX; horizontal++)
                        if (pixels[vertical * texture.width + horizontal].a > 32)
                        { left = Mathf.Min(left, horizontal); right = Mathf.Max(right, horizontal); bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical); }
                if (right < left || top < bottom) throw new InvalidOperationException("Empty K5 sprite " + names[index]);
                rectangles.Add(new SpriteRect { name = names[index], rect = new Rect(left, bottom, right - left + 1, top - bottom + 1), pivot = centered ? new Vector2(0.5f, 0.5f) : new Vector2(index == 8 ? 0.5f : 0.22f, 0.5f), alignment = SpriteAlignment.Custom, spriteID = existing.TryGetValue(names[index], out GUID guid) ? guid : GUID.Generate() });
            }
            provider.SetSpriteRects(rectangles.ToArray()); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID))); provider.Apply(); importer.SaveAndReimport();
            importer.isReadable = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
        }
    }
}
