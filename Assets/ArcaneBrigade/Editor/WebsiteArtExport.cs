using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Lists the art the game draws for every hero, friend, boss and relic (the catalogue's own sprites), so the website
    /// uses exactly what players see: -executeMethod ArcaneBrigade.Editor.WebsiteArtExport.Run -websiteArt FILE.json. Each line names
    /// the source image and the sprite's rectangle in it (pixels, origin bottom left).</summary>
    public static class WebsiteArtExport
    {
        [System.Serializable] private sealed class Entry { public string Kind, Id, Name, Path; public Rect Rect; public int Width, Height; }
        [System.Serializable] private sealed class Sheet { public List<Entry> Entries = new List<Entry>(); }

        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int at = System.Array.IndexOf(args, "-websiteArt");
            string output = at >= 0 && at + 1 < args.Length ? args[at + 1] : "WebsiteArt.json";
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var sheet = new Sheet();
            void Add(string kind, string id, string name, Sprite sprite)
            {
                if (sprite == null || sprite.texture == null) return;
                string path = AssetDatabase.GetAssetPath(sprite.texture);
                if (string.IsNullOrEmpty(path)) return;
                sheet.Entries.Add(new Entry { Kind = kind, Id = id, Name = name, Path = Path.GetFullPath(path), Rect = sprite.rect, Width = sprite.texture.width, Height = sprite.texture.height });
            }
            foreach (UnitDefinition u in catalog.Champions) Add("hero", u.Id, u.DisplayName, u.Body);
            foreach (UnitDefinition u in catalog.Companions) Add("friend", u.Id, u.DisplayName, u.Body);
            foreach (UnitDefinition u in catalog.Bosses) Add("boss", u.Id, u.DisplayName, u.Body);
            foreach (UnitDefinition u in catalog.Enemies) Add("enemy", u.Id, u.DisplayName, u.Body);
            foreach (ItemDefinition i in catalog.Items) Add("relic", i.Id, i.Name, i.Icon);
            File.WriteAllText(output, JsonUtility.ToJson(sheet, true));
            Debug.Log("Website art: " + sheet.Entries.Count + " sprites");
        }
    }
}
