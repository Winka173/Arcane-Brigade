using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Gathers the sliced hero effect sheets (Art/Playtest/HeroEffects, sprites &lt;sheet&gt;_f0, _f1...) into
    /// Resources/HeroEffectArtLibrary so the game can draw them.</summary>
    public static class HeroEffectArtLibraryBuilder
    {
        private const string Folder = "Assets/ArcaneBrigade/Art/Playtest/HeroEffects/";
        private const string LibraryPath = "Assets/ArcaneBrigade/Resources/HeroEffectArtLibrary.asset";

        [MenuItem("Arcane Brigade/Art/Build hero effect art library")]
        public static void Build()
        {
            var library = AssetDatabase.LoadAssetAtPath<HeroEffectArtLibrary>(LibraryPath);
            if (library == null) { library = ScriptableObject.CreateInstance<HeroEffectArtLibrary>(); AssetDatabase.CreateAsset(library, LibraryPath); }
            var entries = new List<HeroEffectArtLibrary.Entry>();
            foreach (string path in Directory.GetFiles(Folder, "*.png").Select(p => p.Replace('\\', '/')).OrderBy(p => p))
            {
                Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => Index(s.name)).ToArray();
                if (frames.Length == 0) { Debug.Log("Hero effect art: no sprites in " + path); continue; }
                entries.Add(new HeroEffectArtLibrary.Entry { Key = Path.GetFileNameWithoutExtension(path), Frames = frames });
            }
            library.Entries = entries.ToArray();
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
            Debug.Log("HERO_EFFECT_ART " + entries.Count + " sheets, " + entries.Sum(e => e.Frames.Length) + " frames");
        }
        private static int Index(string name) { int cut = name.LastIndexOf("_f"); return cut >= 0 && int.TryParse(name.Substring(cut + 2), out int i) ? i : 0; }
    }
}
