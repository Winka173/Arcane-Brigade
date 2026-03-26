using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace ArcaneBrigade.Editor
{
    // Read-only build-content report: everything the Windows player ships (build scenes + every Resources asset and their
    // dependencies), one row per object. Texture rows carry imported size/format/mips/storage bytes; sprite rows carry mesh
    // and physics-shape sizes (they live in resources.assets); every row names the Resources roots that pull it in.
    // Batch: -executeMethod ArcaneBrigade.Editor.BuildSizeReport.Run -reportOut <file.tsv>
    public static class BuildSizeReport
    {
        private static readonly string[] Scenes =
        {
            "Assets/ArcaneBrigade/Scenes/RunBoot.unity", "Assets/ArcaneBrigade/Scenes/RunFrontend.unity",
            "Assets/ArcaneBrigade/Scenes/RunArena.unity", "Assets/ArcaneBrigade/Scenes/WaitingRoom.unity"
        };

        [MenuItem("Arcane Brigade/Reports/Build size report")]
        public static void Run()
        {
            string output = "PreviewExports/BuildSize/BuildSizeReport.tsv";
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-reportOut") output = args[i + 1];
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            MethodInfo storage = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TextureUtil")?
                .GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // roots: build scenes + every asset inside a Resources folder
            var roots = new List<string>(Scenes.Where(File.Exists));
            foreach (string path in AssetDatabase.GetAllAssetPaths())
                if (path.StartsWith("Assets/") && path.Contains("/Resources/") && !AssetDatabase.IsValidFolder(path) && !path.EndsWith(".cs")) roots.Add(path);

            var usedBy = new Dictionary<string, List<string>>();
            foreach (string root in roots)
                foreach (string dep in AssetDatabase.GetDependencies(root, true))
                {
                    if (!usedBy.TryGetValue(dep, out List<string> list)) usedBy[dep] = list = new List<string>();
                    if (dep != root || list.Count == 0) list.Add(dep == root ? "(self)" : root);
                }

            var text = new StringBuilder();
            text.AppendLine("kind\tpath\tname\ttype\twidth\theight\tformat\tmips\treadable\tstorage\truntime\tverts\tindices\tphysShapes\tphysPoints\tspriteRect\tppu\tboundsW\tboundsH\tmeshType\tusedByCount\tusedBy");
            int count = 0;
            foreach (KeyValuePair<string, List<string>> entry in usedBy.OrderBy(e => e.Key))
            {
                string path = entry.Key;
                if (path.EndsWith(".cs") || path.EndsWith(".unity") || AssetDatabase.IsValidFolder(path)) continue;
                string by = string.Join("|", entry.Value.Distinct().Take(6).Select(p => p.Replace("Assets/ArcaneBrigade/", "")));
                int byCount = entry.Value.Distinct().Count();
                UnityEngine.Object[] objects;
                try { objects = AssetDatabase.LoadAllAssetsAtPath(path); } catch { continue; }
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                string meshType = importer == null ? "" : new TextureImporterSettings().Let(settings => importer.ReadTextureSettings(settings)).spriteMeshType.ToString();
                foreach (UnityEngine.Object o in objects)
                {
                    if (o == null) continue;
                    long runtime = Profiler.GetRuntimeMemorySizeLong(o);
                    if (o is Texture2D t)
                    {
                        long bytes = storage != null ? Convert.ToInt64(storage.Invoke(null, new object[] { t })) : runtime;
                        text.Append("tex\t").Append(path).Append('\t').Append(t.name).Append("\tTexture2D\t").Append(t.width).Append('\t').Append(t.height).Append('\t')
                            .Append(t.format).Append('\t').Append(t.mipmapCount).Append('\t').Append(t.isReadable ? 1 : 0).Append('\t').Append(bytes).Append('\t').Append(runtime)
                            .Append("\t\t\t\t\t\t\t\t\t").Append(meshType).Append('\t').Append(byCount).Append('\t').Append(by).AppendLine();
                    }
                    else if (o is Sprite s)
                    {
                        int points = 0, shapes = s.GetPhysicsShapeCount();
                        for (int i = 0; i < shapes; i++) points += s.GetPhysicsShapePointCount(i);
                        Rect r = s.rect;
                        text.Append("sprite\t").Append(path).Append('\t').Append(s.name).Append("\tSprite\t\t\t\t\t\t\t").Append(runtime).Append('\t')
                            .Append(s.vertices.Length).Append('\t').Append(s.triangles.Length).Append('\t').Append(shapes).Append('\t').Append(points).Append('\t')
                            .Append($"{r.x:0},{r.y:0},{r.width:0},{r.height:0}").Append('\t').Append(s.pixelsPerUnit.ToString("0.###")).Append('\t')
                            .Append(s.bounds.size.x.ToString("0.###")).Append('\t').Append(s.bounds.size.y.ToString("0.###")).Append('\t').Append(meshType).Append('\t')
                            .Append(byCount).Append('\t').Append(by).AppendLine();
                    }
                    else
                    {
                        string extra = o is Mesh m ? m.vertexCount + "\t" + m.triangles.Length : o is AnimationClip c ? "\t" + c.length.ToString("0.##") : "\t";
                        text.Append("obj\t").Append(path).Append('\t').Append(o.name.Replace('\t', ' ')).Append('\t').Append(o.GetType().Name)
                            .Append("\t\t\t\t\t\t\t").Append(runtime).Append('\t').Append(extra).Append("\t\t\t\t\t\t\t\t").Append(byCount).Append('\t').Append(by).AppendLine();
                    }
                }
                if (++count % 100 == 0) { EditorUtility.UnloadUnusedAssetsImmediate(); }
            }
            File.WriteAllText(output, text.ToString());
            Debug.Log("BuildSizeReport: " + count + " assets, target " + EditorUserBuildSettings.activeBuildTarget + " -> " + output);
        }

        private static T Let<T>(this T value, Action<T> action) { action(value); return value; }
    }
}
