using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K9Builder
    {
        private static StatModifier Modifier(ItemEffect effect, float value, ModifierScope? scope = null)
            => new StatModifier { Effect = effect, Value = value, Scope = scope ?? RunModifierRules.DefaultScope(effect) };

        [MenuItem("Arcane Brigade/K9/Apply companion characteristics")]
        public static void Apply()
        {
            K8Builder.Apply();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (UnitDefinition unit in catalog.Companions)
            {
                unit.CompanionModifiers = unit.Id switch
                {
                    "TeapotPilgrim" => new[] { Modifier(ItemEffect.Heal, 0.04f), Modifier(ItemEffect.MoveSpeed, -0.003f, ModifierScope.Champion) },
                    "CandleSnail" => new[] { Modifier(ItemEffect.Armor, 0.025f), Modifier(ItemEffect.MoveSpeed, -0.005f, ModifierScope.Champion) },
                    "MailboxImp" => new[] { Modifier(ItemEffect.Pickup, 0.08f), Modifier(ItemEffect.Luck, -0.01f) },
                    "PillowOracle" => new[] { Modifier(ItemEffect.Health, 3f), Modifier(ItemEffect.AttackSpeed, -0.005f, ModifierScope.Champion) },
                    "MushroomLibrarian" => new[] { Modifier(ItemEffect.Luck, 0.025f), Modifier(ItemEffect.CompanionAttackSpeed, -0.005f) },
                    "HoodWisp" => new[] { Modifier(ItemEffect.Crit, 0.01f), Modifier(ItemEffect.Health, -1f) },
                    "JellySwordsman" => new[] { Modifier(ItemEffect.CompanionDamage, 0.025f), Modifier(ItemEffect.MoveSpeed, -0.005f, ModifierScope.Companions) },
                    _ => throw new InvalidOperationException("K9: missing authored companion traits: " + unit.Id)
                };
                EditorUtility.SetDirty(unit);
            }
            RunModifierRules.Validate(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("K9_COMPANION_CHARACTERISTICS_APPLIED");
        }

        [MenuItem("Arcane Brigade/K9/Check")]
        public static void Check()
        {
            Apply(); K9Checks.Run(); K8Builder.Check();
            Debug.Log("K9_EDITOR_REGRESSION_PASSED");
        }

        public static void Build()
        {
            Check();
            K7BalanceDiagnostics.Run();
            Directory.CreateDirectory("PreviewExports/K9");
            File.Copy("PreviewExports/K7/Movement_Bot_Balance.csv", "PreviewExports/K9/Movement_Bot_Balance.csv", true);
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K9/Windows_Build.txt", true);
            Debug.Log("K9_WINDOWS_CANDIDATE_BUILT");
        }

        public static void FinalizeCandidate()
        {
            Check(); FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K9/Windows_Build.txt", true);
            Debug.Log("K9_FINAL_CANDIDATE_BUILT");
        }
    }
}
