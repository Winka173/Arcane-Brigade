using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K7QualityBuilder
    {
        private const string Root = "Assets/ArcaneBrigade/";
        [MenuItem("Arcane Brigade/K7/Quality rework apply check and build")]
        public static void GateAndBuild()
        {
            Apply(); K7Builder.Prepare();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(Root + "Resources/RunCatalog.asset");
            K7Checks.Run(); K7QualityChecks.Run(); K6Checks.Run(); K5Checks.Run();
            CheckpointChecks.Run(catalog.Arena); SquadCheckpointChecks.Run(catalog.Arena);
            K1Builder.CheckExisting(); K2Checks.Run(); K3Checks.Run(); K32Checks.Run(); K32AltarChecks.Run(); K3FinalChecks.Run();
            VisualQualityChecks.Run(); K4FinalChecks.Run(); K7BalanceDiagnostics.Run();
            FullRunBuilder.BuildWindows();
        }
        public static void Apply()
        {
            string[] friends = { "LanternKnight", "TeapotCourier", "SnailPorter", "MailboxGuard", "SleepyPatch", "BookLibrarian" };
            Dictionary<string, Sprite[]> friendly = Import("BrigadeAnimation_v3", friends, new[] { 0, 290, 550, 797, 1051, 1281, 1536 });
            string[] foes = { "DustImp", "InkSpitter", "MoonMagnet", "SleepMimic", "CurtainWisp", "DoorBearer" };
            Dictionary<string, Sprite[]> enemy = Import("NightmareEnemies_v2", foes, new[] { 0, 245, 451, 693, 941, 1210, 1536 });
            Dictionary<string, Sprite[]> effects = Import("PaintedEffects_v1", new[] { "Dust", "Slash", "Magic", "Death" }, new[] { 0, 384, 768, 1152, 1536 });
            Dictionary<string, Sprite[]> furniture = Import("ShopFixtures_v1", new[] { "Furniture" }, new[] { 0, 1536 }, new[] { 0f, 0.265f, 0.545f, 0.775f, 1f });
            string libraryPath = Root + "Resources/PaintedAnimationLibrary.asset";
            PaintedAnimationLibrary library = AssetDatabase.LoadAssetAtPath<PaintedAnimationLibrary>(libraryPath);
            if (library == null) { library = ScriptableObject.CreateInstance<PaintedAnimationLibrary>(); AssetDatabase.CreateAsset(library, libraryPath); }
            library.Dust = effects["Dust"]; library.Slash = effects["Slash"]; library.Magic = effects["Magic"]; library.Death = effects["Death"]; EditorUtility.SetDirty(library);
            library.ShopFixtures = furniture["Furniture"];
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(Root + "Resources/RunCatalog.asset");
            var assignments = new Dictionary<string, string> { { "LanternKnight", "LanternKnight" }, { "Postmaster", "MailboxGuard" }, { "DrowsySeer", "SnailPorter" }, { "TeapotPilgrim", "TeapotCourier" }, { "CandleSnail", "SnailPorter" }, { "JellySwordsman", "SleepyPatch" }, { "MailboxImp", "MailboxGuard" }, { "PillowOracle", "SleepyPatch" }, { "MushroomLibrarian", "BookLibrarian" }, { "RuneWisp", "BookLibrarian" }, { "HoodWisp", "BookLibrarian" } };
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions))
            {
                if (!assignments.TryGetValue(unit.Id, out string key)) throw new InvalidOperationException("Missing explicit animation identity " + unit.Id);
                Sprite[] frames = friendly[key];
                unit.AnimationFrames = frames; unit.Body = frames[0]; EditorUtility.SetDirty(unit);
            }
            for (int index = 0; index < catalog.Enemies.Length; index++)
            {
                UnitDefinition unit = catalog.Enemies[index];
                unit.AnimationFrames = enemy.TryGetValue(unit.Id, out Sprite[] frames) ? frames : enemy[foes[index % foes.Length]];
                unit.Body = unit.AnimationFrames[0]; unit.Tint = Color.white; EditorUtility.SetDirty(unit);
            }
            for (int index = 0; index < catalog.Bosses.Length; index++)
            {
                UnitDefinition boss = catalog.Bosses[index]; boss.AnimationFrames = enemy[foes[index % foes.Length]];
                boss.Body = boss.AnimationFrames[0]; boss.Tint = Color.white; EditorUtility.SetDirty(boss);
            }
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                if (weapon.Id == "RuneStaff") { weapon.ProjectileCount = 2; weapon.ProjectilePierce = 1; }
                if (weapon.Id == "CrescentBow") weapon.ProjectilePierce = 2;
                EditorUtility.SetDirty(weapon);
            }
            catalog.Arena.PickupRadius = Mathf.Max(catalog.Arena.PickupRadius, 1.7f);
            EditorUtility.SetDirty(catalog.Arena); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }
        internal static Dictionary<string, Sprite[]> Import(string file, string[] rows, int[] edges, float[] columns = null)
        {
            string path = Root + "Art/AnimationHD/" + file + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 3) throw new InvalidOperationException("Animation source must have true alpha");
            var factory = new SpriteDataProviderFactories(); factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> prior = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new List<SpriteRect>();
            for (int row = 0; row < rows.Length; row++)
            {
                int lower = texture.height - Mathf.RoundToInt(edges[row + 1] / 1536f * texture.height);
                int upper = texture.height - Mathf.RoundToInt(edges[row] / 1536f * texture.height);
                int cell = texture.width / 4; int bottom = upper; int top = lower;
                for (int vertical = lower; vertical < upper; vertical++)
                    for (int horizontal = 0; horizontal < texture.width; horizontal++)
                        if (pixels[vertical * texture.width + horizontal].a > 160) { bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical); }
                for (int frame = 0; frame < 4; frame++)
                {
                    string name = rows[row] + (frame == 0 ? "" : "_" + frame);
                    int left = columns == null ? frame * cell : Mathf.RoundToInt(columns[frame] * texture.width);
                    int right = columns == null ? (frame + 1) * cell : Mathf.RoundToInt(columns[frame + 1] * texture.width);
                    rectangles.Add(new SpriteRect { name = name, rect = new Rect(left, bottom, right - left, top - bottom + 1), alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f), spriteID = prior.TryGetValue(name, out GUID guid) ? guid : GUID.Generate() });
                }
            }
            provider.SetSpriteRects(rectangles.ToArray());
            var identities = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            identities.SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            Dictionary<string, Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
            return rows.ToDictionary(name => name, name => new[] { sprites[name], sprites[name + "_1"], sprites[name + "_2"], sprites[name + "_3"] });
        }
    }
}
