using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K0BaselineBuilder
    {
        [Serializable] private sealed class Entry { public string Path; public long Bytes; public string Sha256; }
        [Serializable] private sealed class Manifest
        {
            public string UnityVersion;
            public string CatalogVersion;
            public string SavePolicy;
            public string[] UnitIds;
            public string[] WeaponIds;
            public string[] ItemIds;
            public List<Entry> Files = new List<Entry>();
        }

        [MenuItem("Arcane Brigade/K0/Capture protected baseline")]
        public static void Capture()
        {
            string root = "PreviewExports/K0";
            Directory.CreateDirectory(root);
            string path = root + "/BaselineManifest.json";
            if (File.Exists(path)) throw new InvalidOperationException("Protected baseline already exists; it will not be overwritten.");
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            FullRunChecks.Run(catalog);
            File.Copy("PreviewExports/FullRun_Checks.txt", root + "/Baseline_Checks.txt", false);
            var manifest = new Manifest
            {
                UnityVersion = Application.unityVersion, CatalogVersion = catalog.Version,
                SavePolicy = "No real player save read or written. Regression fixtures use isolated temporary folders. Source_Before_K1.zip preserves the pre-change workspace.",
                UnitIds = catalog.Champions.Concat(catalog.Companions).Select(unit => unit.Id).ToArray(),
                WeaponIds = catalog.Weapons.Select(weapon => weapon.Id).ToArray(), ItemIds = catalog.Items.Select(item => item.Id).ToArray()
            };
            var paths = Directory.GetFiles("Assets/ArcaneBrigade", "*", SearchOption.AllDirectories).Concat(Directory.GetFiles("Packages")).Concat(Directory.GetFiles("ProjectSettings"));
            foreach (string source in paths.OrderBy(value => value))
                using (SHA256 hash = SHA256.Create())
                using (FileStream stream = File.OpenRead(source))
                    manifest.Files.Add(new Entry { Path = source.Replace('\\', '/'), Bytes = stream.Length, Sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant() });
            string player = "Builds/FullRun/ArcaneBrigade.exe";
            if (File.Exists(player))
                using (SHA256 hash = SHA256.Create())
                using (FileStream stream = File.OpenRead(player))
                    manifest.Files.Add(new Entry { Path = player, Bytes = stream.Length, Sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant() });
            File.WriteAllText(path, JsonUtility.ToJson(manifest, true));
            Debug.Log("ARCANE_K0_BASELINE_PASSED " + manifest.Files.Count);
        }
    }
}
