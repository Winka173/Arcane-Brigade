using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K12Builder
    {
        public static void Build()
        {
            Directory.CreateDirectory("PreviewExports/K12");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            AddBrute(catalog, "FurnaceToad", "Furnace Toad", 4, true);
            AddBrute(catalog, "WardrobeCrab", "Wardrobe Crab", 5, false);
            foreach (UnitDefinition unit in catalog.Companions)
            {
                unit.SpecialRecruit = unit.Id == "EclipseEgg" || unit.Id == "PillowOracle";
                EditorUtility.SetDirty(unit);
            }
            K11EnglishBuilder.Apply(); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            K12Checks.Run(); K11Checks.Run();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K12/Windows_Build.txt", true);
            Debug.Log("K12_CORE_CANDIDATE_BUILT");
        }

        private static void AddBrute(RunCatalog catalog, string id, string name, int wave, bool ranged)
        {
            string path = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
            UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
            if (unit == null) { unit = ScriptableObject.CreateInstance<UnitDefinition>(); AssetDatabase.CreateAsset(unit, path); }
            UnitDefinition template = catalog.Enemies.First(enemy => (enemy.Weapon.Kind == WeaponKind.Projectile) == ranged);
            unit.Id = id; unit.DisplayName = name; unit.AnimationFrames = K11ArtBuilder.Import(id + "_v1", "K12"); unit.Body = unit.AnimationFrames[0];
            unit.Weapon = template.Weapon; unit.Health = ranged ? 180 : 240; unit.Speed = ranged ? 1.8f : 2.1f;
            unit.BodyHeight = ranged ? 1.65f : 1.9f; unit.Radius = ranged ? 0.48f : 0.55f;
            unit.ContactDamage = 8; unit.ContactInterval = 0.9f; unit.UnlockWave = wave; unit.KnockbackResistance = ranged ? 0.65f : 0.75f;
            unit.Description = ranged ? "Heavy spitter · resists knockback" : "Heavy biter · resists knockback";
            unit.Tint = Color.white;
            if (!catalog.Enemies.Contains(unit)) catalog.Enemies = catalog.Enemies.Concat(new[] { unit }).ToArray();
            EditorUtility.SetDirty(unit);
        }
    }
}
