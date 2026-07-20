using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Exports every companion's and hero's body picture to PNG files (batch: -executeMethod ArcaneBrigade.Editor.UnitPortraitDump.Run
    /// -dumpPath DIR), for design reviews of how a creature looks against its role, bonds and skill.</summary>
    public static class UnitPortraitDump
    {
        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-dumpPath");
            string dir = i >= 0 && i + 1 < args.Length ? args[i + 1] : Path.Combine(Application.dataPath, "..", "portraits");
            Directory.CreateDirectory(dir);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("id	path	x	y	w	h	texture_height");
            foreach (UnitDefinition u in catalog.Companions) Row(sb, u);
            foreach (UnitDefinition u in catalog.Champions) Row(sb, u);
            File.WriteAllText(Path.Combine(dir, "portraits.tsv"), sb.ToString());
            Debug.Log("PORTRAITDUMP rows -> " + dir);
        }

        /// <summary>Where each body picture lives on disk and its rectangle in that file (no graphics device is needed).</summary>
        private static void Row(System.Text.StringBuilder sb, UnitDefinition u)
        {
            Sprite s = u.Body;
            if (s == null || s.texture == null) return;
            Rect r = s.textureRect;
            sb.AppendLine(u.Id + "	" + UnityEditor.AssetDatabase.GetAssetPath(s.texture) + "	" + r.x + "	" + r.y + "	" + r.width + "	" + r.height + "	" + s.texture.height);
        }
    }
}
