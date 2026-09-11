using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Builds Resources/CartoonFxLibrary.asset from the Cartoon FX Remaster Free pack (Assets/JMO Assets): the
    /// element hits, element bursts and comic words the rules effects play. Only billboard effects that read from the
    /// game's top-down camera are listed (no flat ground decals, no 3D meshes, no lit or looping effects).
    /// Runs with RulesVfxLibraryBuilder.Run, or alone with -executeMethod ArcaneBrigade.Editor.CartoonFxLibraryBuilder.Run.</summary>
    public static class CartoonFxLibraryBuilder
    {
        private const string Pack = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/", Output = "Assets/ArcaneBrigade/Resources/CartoonFxLibrary.asset";
        private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>
        {
            { "hit_fire", "Fire/CFXR3 Hit Fire B (Air)" },
            { "hit_ice", "Ice/CFXR3 Hit Ice B (Air)" },
            { "hit_storm", "Electric/CFXR3 Hit Electric C (Air)" },
            { "hit_light", "Light/CFXR3 Hit Light B (Air)" },
            { "hit_misc", "Misc/CFXR3 Hit Misc A" },
            { "hit_smoke", "Misc/CFXR3 Hit Misc F Smoke" },
            { "hit_crit", "Impacts/CFXR Hit A (Red)" },
            { "hit_cross", "Sword Trails/Plain/CFXR4 Sword Hit PLAIN (Cross)" },
            { "burst_fire", "Explosions/CFXR3 Fire Explosion B" },
            { "burst_storm", "Impacts/CFXR Impact Glowing HDR (Blue)" },
            { "burst_water", "Liquids/CFXR Water Splash (Smaller)" },
            { "burst_poof", "Misc/CFXR Magic Poof" },
            { "burst_boom", "Explosions/CFXR Explosion 1" },
            { "burst_flash", "Misc/CFXR Flash" },
            { "burst_souls", "Eerie/CFXR2 Souls Escape" },
            { "word_pow", "Texts/CFXR _POW_" },
            { "word_boom", "Texts/CFXR _BOOM_" },
            { "word_wham", "Texts/CFXR2 _WHAM_ 3" },
            { "word_frozen", "Texts/CFXR4 _FROZEN_" },
            { "word_poisoned", "Texts/CFXR4 _POISONED_" },
        };

        public static void Run()
        {
            if (!Directory.Exists("Assets/JMO Assets")) { Debug.Log("CartoonFxLibraryBuilder: pack not imported"); return; }
            var library = AssetDatabase.LoadAssetAtPath<CartoonFxLibrary>(Output);
            if (library == null) { library = ScriptableObject.CreateInstance<CartoonFxLibrary>(); AssetDatabase.CreateAsset(library, Output); }
            library.Entries.Clear();
            foreach (var pair in Keys)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Pack + pair.Value + ".prefab");
                if (prefab == null) { Debug.LogWarning("CartoonFxLibraryBuilder: missing " + pair.Value); continue; }
                library.Entries.Add(new CartoonFxLibrary.Entry { Key = pair.Key, Prefab = prefab });
            }
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
            Debug.Log("CartoonFxLibraryBuilder: " + library.Entries.Count + " effects");
        }
    }
}
