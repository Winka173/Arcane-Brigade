using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class CharacterActingBuilder
    {
        public static readonly string[] Heroes = { "LanternKnight", "Postmaster", "DrowsySeer", "RootOracle", "LoneJelly", "GlassDice", "TideSinger", "CometRunner", "MirrorKeeper", "TreasureAcorn" };
        private static readonly string[] Aliases = { "", "", "", "", "", "DiceCrab", "BubbleKoi", "CometKite", "MirrorNewt", "BrassAcorn" };
        [MenuItem("Arcane Brigade/Animation/Import character acting")]
        public static void Build()
        {
            const string path = "Assets/ArcaneBrigade/Resources/CharacterActingLibrary.asset";
            CharacterActingLibrary library = AssetDatabase.LoadAssetAtPath<CharacterActingLibrary>(path);
            if (library == null) { library = ScriptableObject.CreateInstance<CharacterActingLibrary>(); AssetDatabase.CreateAsset(library, path); }
            CharacterActingEntry[] extras = library.Entries.Where(entry => !Heroes.Contains(entry.Id)).ToArray();
            library.Entries = new CharacterActingEntry[Heroes.Length];
            for (int index = 0; index < Heroes.Length; index++)
            {
                string id = Heroes[index];
                Sprite[] frames = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Acting/" + id + "_Acting_v1.png", id + "Acting", 4, 3, Enumerable.Range(0, 12).ToArray(), new[] { 0, 341, 683, 1024 }, true);
                library.Entries[index] = new CharacterActingEntry { Id = id, Alias = Aliases[index], Curious = frames.Take(4).ToArray(), Cheer = frames.Skip(4).Take(4).ToArray(), Hurt = frames.Skip(8).Take(2).ToArray(), Defeat = frames.Skip(10).Take(2).ToArray() };
            }
            library.Entries = library.Entries.Concat(extras).ToArray();
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); int count = 0;
            void Check(bool value, string message) { if (!value) throw new InvalidOperationException("Character acting: " + message); count++; }
            foreach (UnitDefinition hero in catalog.Champions)
            {
                CharacterActingEntry entry = library.Find(hero.Id);
                Check(entry != null, "Every champion has its own acting " + hero.Id);
                foreach (Sprite frame in entry.Curious.Concat(entry.Cheer).Concat(entry.Hurt).Concat(entry.Defeat)) Check(frame != null && frame.rect.width > 16 && frame.rect.height > 16, "Complete painted pose " + hero.Id);
                Check(entry.Select(true, -1, false, -1, -1, false, false) == null, "Normal animation remains fallback " + hero.Id);
                Check(entry.Select(true, .05f, true, -1, -1, false, false) == entry.Hurt[0], "Actual hurt overrides attack acting " + hero.Id);
                Check(entry.Select(true, -1, true, .2f, .2f, false, false) == null, "No celebration masks a real attack " + hero.Id);
                Check(entry.Select(true, -1, false, .2f, .2f, true, false) == null, "No idle acting masks locomotion " + hero.Id);
                Check(entry.Select(false, -1, false, -1, -1, false, false) == entry.Defeat[0], "Authored defeat pose " + hero.Id);
                Check(entry.Select(true, .05f, false, .2f, .2f, false, true) == null, "Reduced motion uses existing stable pose " + hero.Id);
                Check(entry.Preview(0, -1) == entry.Curious[0] && entry.Preview(0, 2) == entry.Curious[3], "Preview endpoints clamp " + hero.Id);
                Check(entry.Preview(1, .6f) == entry.Cheer[Mathf.Min(entry.Cheer.Length - 1, Mathf.FloorToInt(.6f * entry.Cheer.Length))] && entry.Preview(2, .8f) == entry.Hurt[Mathf.Min(entry.Hurt.Length - 1, Mathf.FloorToInt(.8f * entry.Hurt.Length))] && entry.Preview(3, .8f) == entry.Defeat[Mathf.Min(entry.Defeat.Length - 1, Mathf.FloorToInt(.8f * entry.Defeat.Length))], "All collection cues select their authored frames " + hero.Id);
                Check(entry.Preview(-1, .5f) == null, "Cleared preview returns to live pose " + hero.Id);
                Check(entry.Select(true, .18f, false, .7f, 1, false, false) == null, "Acting windows have no lingering pose " + hero.Id);
                Check(string.IsNullOrEmpty(entry.Alias) || library.Find(entry.Alias) == entry, "Only matching body companion aliases " + hero.Id);
                foreach (Sprite frame in entry.Curious.Concat(entry.Cheer).Concat(entry.Hurt).Concat(entry.Defeat))
                    Check(frame.pivot.y == 0 && frame.rect.xMin > 0 && frame.rect.yMin > 0 && frame.rect.xMax < frame.texture.width && frame.rect.yMax < frame.texture.height, "Grounded complete source framing " + frame.name);
            }
            Directory.CreateDirectory("PreviewExports/CharacterActing");
            File.WriteAllText("PreviewExports/CharacterActing/Focused_Checks.txt", count + " PASS; ten identity-specific twelve-pose sheets; priority, locomotion, source margins, collection endpoints, fallback and reduced checks.\n");
            Debug.Log("CHARACTER_ACTING_CHECKS " + count + " PASS");
        }
        public static void Gate() { Build(); AnimationPolishBuilder.Gate(); }
    }
}
