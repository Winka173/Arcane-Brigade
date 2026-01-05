using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K4FinalBuilder
    {
        [MenuItem("Arcane Brigade/K4/Apply full signature slice check and build")]
        public static void ApplyAndBuild() { Apply(); K4FinalChecks.Run(); FullRunBuilder.BuildWindows(); }

        public static void Apply()
        {
            K4SignatureBuilder.Apply();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Dictionary<string, Sprite> icons = ImportIcons();
            if (catalog.SignatureRevision == 0)
            {
                var items = new List<ItemDefinition>(catalog.Items)
                {
                    Relic("harvest-receipt", "Harvest Receipt", "Coin logical pickup → một nhịp cho Bell gần trong 6 units, cooldown 0.35s. Stack 2 cho hai nhịp, không sinh coin. Hút loot -0.15 mỗi stack.", SignatureRelicKind.HarvestReceipt, 28, ItemRarity.Uncommon, ItemEffect.Pickup, ModifierScope.Run, -0.15f, 2),
                    Relic("reflective-ink", "Reflective Ink", "Stamp thêm một dấu mỗi hit; expiry ×0.65. Burst mở weakness 1.8s cho Office Lens. Không tăng damage burst trực tiếp. Duy nhất.", SignatureRelicKind.ReflectiveInk, 32, ItemRarity.Uncommon, ItemEffect.Damage, ModifierScope.Team, 0, 1),
                    Relic("carbon-ribbon", "Carbon Ribbon", "Bell wave mở weakness 1.5s; Stamp burst kéo coin trong 2.5 units. Không copy damage/death/loot, không tự truyền; Crit team -4%. Duy nhất.", SignatureRelicKind.CarbonRibbon, 40, ItemRarity.Rare, ItemEffect.Crit, ModifierScope.Team, -0.04f, 1),
                    Relic("office-lens", "Office Lens", "Crit ally +15%, attack speed -5%. Stamp burst mở weakness 1.8s: crit vào mục tiêu exposed thêm 30% damage (stack tăng tới 50%). Không tạo burst từ crit.", SignatureRelicKind.OfficeLens, 38, ItemRarity.Rare, ItemEffect.Crit, ModifierScope.Companions, 0.15f, 5)
                };
                items[items.Count - 1].Modifiers = new[] { new StatModifier { Effect = ItemEffect.Crit, Scope = ModifierScope.Companions, Value = 0.15f }, new StatModifier { Effect = ItemEffect.AttackSpeed, Scope = ModifierScope.Companions, Value = -0.05f } };
                catalog.Items = items.ToArray();
                var links = new List<BuildLinkRule>(catalog.BuildLinks)
                {
                    new BuildLinkRule { Id = "signature-harvest", Name = "MÁY THU HOẠCH", ItemIds = new[] { "harvest-receipt", "carbon-ribbon" }, Description = "Enemy kill → coin → logical pickup → Receipt (0.35s) → Bell cadence → delayed wave. Bell + Receipt đã hoạt động, không cần đủ set. Carbon nối wave → weakness (Lens mới tăng crit damage); Stamp burst → hút coin phạm vi 2.5. Pickup không tự sinh tiền, secondary không charge prop/proc." },
                    new BuildLinkRule { Id = "signature-office", Name = "VĂN PHÒNG HOÀN TRẢ", ItemIds = new[] { "reflective-ink", "office-lens", "carbon-ribbon" }, Description = "Stamp → Ink (dấu nhanh hơn nhưng ngắn hơn) → threshold burst → weakness 1.8s → Lens hưởng crit. Stamp + Ink đã có payoff cadence; Lens hữu ích với mọi ally crit trước khi đủ build. Carbon nối Stamp burst → coin collection cho Harvest. Dấu truyền tier không tự burst, crit không tạo stamp lại." }
                };
                catalog.BuildLinks = links.ToArray(); catalog.SignatureRevision = 1;
            }
            foreach (ItemDefinition item in catalog.Items) if (icons.TryGetValue(item.Id, out Sprite icon)) item.Icon = icon;
            RunModifierRules.Validate(catalog); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }

        private static ItemDefinition Relic(string id, string name, string description, SignatureRelicKind role, int price, ItemRarity rarity, ItemEffect effect, ModifierScope scope, float value, int max)
        {
            return new ItemDefinition { Id = id, Name = name, Description = description, SignatureRole = role, Price = price, Rarity = rarity, ShopWeight = 55, Pools = ShopPool.Both, AltarPrice = 3, UnlockWave = 2, Stacking = max == 1 ? StackPolicy.Unique : StackPolicy.Additive, MaximumStacks = max, Modifiers = new[] { new StatModifier { Effect = effect, Scope = scope, Value = value } } };
        }

        private static Dictionary<string, Sprite> ImportIcons()
        {
            const string path = "Assets/ArcaneBrigade/Art/QualityHD/SignatureRelics_v1.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.isReadable = true; importer.SaveAndReimport();
            var factory = new SpriteDataProviderFactories(); factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> existing = provider.GetSpriteRects().ToDictionary(rect => rect.name, rect => rect.spriteID);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Color32[] pixels = texture.GetPixels32();
            string[] names = { "harvest-receipt", "reflective-ink", "carbon-ribbon", "office-lens" };
            var rects = new List<SpriteRect>();
            for (int index = 0; index < names.Length; index++)
            {
                int split = Mathf.RoundToInt(texture.width * 0.54f);
                int minX = index % 2 == 0 ? 0 : split; int maxX = index % 2 == 0 ? split : texture.width;
                int minY = index < 2 ? texture.height / 2 : 0; int maxY = index < 2 ? texture.height : texture.height / 2;
                int left = maxX; int right = minX; int bottom = maxY; int top = minY;
                for (int vertical = minY; vertical < maxY; vertical++)
                    for (int horizontal = minX; horizontal < maxX; horizontal++)
                        if (pixels[vertical * texture.width + horizontal].a > 32)
                        {
                            left = Mathf.Min(left, horizontal); right = Mathf.Max(right, horizontal);
                            bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical);
                        }
                left = Mathf.Max(minX, left - 2); right = Mathf.Min(maxX - 1, right + 2); bottom = Mathf.Max(minY, bottom - 2); top = Mathf.Min(maxY - 1, top + 2);
                rects.Add(new SpriteRect { name = names[index], rect = new Rect(left, bottom, right - left + 1, top - bottom + 1), pivot = new Vector2(0.5f, 0.5f), alignment = SpriteAlignment.Center, spriteID = existing.TryGetValue(names[index], out GUID guid) ? guid : GUID.Generate() });
            }
            provider.SetSpriteRects(rects.ToArray()); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID))); provider.Apply(); importer.SaveAndReimport();
            importer.isReadable = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
        }
    }
}
