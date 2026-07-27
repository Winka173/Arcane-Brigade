using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the repainted characters and weapons in (Docs/Art_Redo_Requests.md, 24/10/2026). A character sheet
    /// Art/Playtest/Redo/&lt;Id&gt;.png is a 4×2 grid of eight frames (idle, idle, walk, walk, windup, windup, release,
    /// recovery): it becomes the unit's body, its four-frame cycle and its authored motion. A weapon sheet
    /// Art/Playtest/Redo/Weapon_&lt;Id&gt;.png is one held weapon pointing right: it becomes the weapon's sprite. Missing sheets
    /// are skipped, so the set can arrive in parts.</summary>
    public static class ArtRedoBuilder
    {
        private const string Folder = "Assets/ArcaneBrigade/Art/Playtest/Redo/";

        [MenuItem("Arcane Brigade/Art/Put repainted characters and weapons in")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder.TrimEnd('/'))) { Debug.Log("ART_REDO no folder"); return; }
            var units = new Dictionary<string, UnitDefinition>(); var weapons = new Dictionary<string, WeaponDefinition>();
            var allUnits = new List<UnitDefinition>(); var allWeapons = new List<WeaponDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources" }))
            { var u = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid)); if (u == null || string.IsNullOrEmpty(u.Id)) continue; allUnits.Add(u); if (!units.ContainsKey(u.Id)) units[u.Id] = u; }
            foreach (string guid in AssetDatabase.FindAssets("t:WeaponDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources" }))
            { var w = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetDatabase.GUIDToAssetPath(guid)); if (w == null || string.IsNullOrEmpty(w.Id)) continue; allWeapons.Add(w); if (!weapons.ContainsKey(w.Id)) weapons[w.Id] = w; }
            var actingLibrary = AssetDatabase.LoadAssetAtPath<CharacterActingLibrary>("Assets/ArcaneBrigade/Resources/CharacterActingLibrary.asset");
            var actingEntries = actingLibrary == null ? new List<CharacterActingEntry>() : actingLibrary.Entries.ToList();
            var performance = AssetDatabase.LoadAssetAtPath<ExpandedPerformanceLibrary>("Assets/ArcaneBrigade/Resources/ExpandedPerformanceLibrary.asset");
            int characters = 0, arms = 0, actingSheets = 0;
            foreach (string path in Directory.GetFiles(Folder, "*.png").Select(p => p.Replace('\\', '/')).OrderBy(p => p, System.StringComparer.Ordinal))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                if (name.EndsWith("_Acting", System.StringComparison.Ordinal))
                {
                    string id = name.Substring(0, name.Length - "_Acting".Length);
                    if (!units.ContainsKey(id)) throw new System.InvalidOperationException("Unknown acting unit " + id);
                    Sprite[] poses = Grid(path, name, 4, 3);
                    if (poses == null) throw new System.InvalidOperationException("Invalid acting grid " + name);
                    var entry = actingEntries.FirstOrDefault(e => e.Id == id);
                    if (entry == null) { entry = new CharacterActingEntry { Id = id }; actingEntries.Add(entry); }
                    entry.Curious = poses.Take(4).ToArray(); entry.Cheer = poses.Skip(4).Take(4).ToArray();
                    entry.Hurt = poses.Skip(8).Take(2).ToArray(); entry.Defeat = poses.Skip(10).Take(2).ToArray();
                    entry.ReferenceHeight = poses[0].bounds.size.y;
                    actingSheets++;
                    continue;
                }
                if (name.StartsWith("Weapon_"))
                {
                    if (!weapons.TryGetValue(name.Substring(7), out WeaponDefinition w)) { Debug.Log("ART_REDO no weapon " + name); continue; }
                    Sprite s = Single(path); if (s == null) continue;
                    foreach (var definition in allWeapons.Where(candidate => candidate.Id == w.Id))
                    { definition.Sprite = s; EditorUtility.SetDirty(definition); }
                    arms++;
                    continue;
                }
                if (!units.TryGetValue(name, out UnitDefinition unit)) { Debug.Log("ART_REDO no unit " + name); continue; }
                Sprite[] frames = Grid(path, name, 4, 2); if (frames == null) continue;
                string motionPath = "Assets/ArcaneBrigade/Resources/" + name + "RedoMotion.asset";
                var motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(motionPath);
                if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, motionPath); }
                motion.Idle = new[] { frames[0], frames[1] }; motion.Walk = new[] { frames[2], frames[3], frames[2], frames[3] };
                motion.Windup = new[] { frames[4], frames[5] }; motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
                EditorUtility.SetDirty(motion);
                unit.Body = frames[0]; unit.AuthoredMotion = motion; unit.AnimationFrames = new[] { frames[0], frames[1], frames[2], frames[3] };
                // Expanded gait and acting can override AuthoredMotion during rendering. Rebind those too so
                // walking, reactions and shop gestures never switch back to the old flat/copy-of-a-hero art.
                var gait = performance == null ? null : performance.Find("locomotion", name);
                if (gait != null)
                {
                    gait.Frames = new[] { frames[0], frames[1], frames[2], frames[3], frames[2], frames[5], frames[6], frames[7] };
                    gait.ReferenceHeight = motion.ReferenceHeight;
                }
                foreach (var old in actingEntries) if (old.Alias == name) old.Alias = "";
                var acting = actingEntries.FirstOrDefault(e => e.Id == name);
                if (acting == null) { acting = new CharacterActingEntry { Id = name }; actingEntries.Add(acting); }
                acting.ReferenceHeight = motion.ReferenceHeight;
                acting.Curious = new[] { frames[0], frames[1] }; acting.Cheer = new[] { frames[6], frames[7] };
                acting.Hurt = new[] { frames[5] }; acting.Defeat = new[] { frames[7] };
                foreach (var definition in allUnits.Where(candidate => candidate.Id == unit.Id))
                {
                    definition.Body = unit.Body; definition.AuthoredMotion = motion; definition.AnimationFrames = unit.AnimationFrames;
                    EditorUtility.SetDirty(definition);
                }
                characters++;
            }
            if (actingLibrary != null) { actingLibrary.Entries = actingEntries.ToArray(); EditorUtility.SetDirty(actingLibrary); }
            if (performance != null) EditorUtility.SetDirty(performance);
            AssetDatabase.SaveAssets();
            ExpandedPerformanceLibrary.Invalidate();
            SpriteInkMetricsBuilder.Build();
            Debug.Log("ART_REDO characters " + characters + ", weapons " + arms + ", acting " + actingSheets);
        }

        [System.Serializable] private sealed class Manifest { public Job[] jobs; }
        [System.Serializable] private sealed class Job { public string name; public string path; public string kind; }

        /// <summary>Checks the actual imported sprites and every renderer override against the delivery manifest.</summary>
        public static void BuildAndVerify()
        {
            Build();
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("Docs/Art_Redo_Manifest.json"));
            var units = AssetDatabase.FindAssets("t:UnitDefinition")
                .Select(g => AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(u => u != null).ToArray();
            var weapons = AssetDatabase.FindAssets("t:WeaponDefinition")
                .Select(g => AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(w => w != null).ToArray();
            var acting = AssetDatabase.LoadAssetAtPath<CharacterActingLibrary>("Assets/ArcaneBrigade/Resources/CharacterActingLibrary.asset");
            var performance = AssetDatabase.LoadAssetAtPath<ExpandedPerformanceLibrary>("Assets/ArcaneBrigade/Resources/ExpandedPerformanceLibrary.asset");
            var lines = new List<string>();
            int total = 0;
            foreach (var job in manifest.jobs)
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(job.path).OfType<Sprite>().ToArray();
                int expected = job.kind == "weapon" ? 1 : job.kind == "acting" ? 12 : 8;
                if (sprites.Length != expected) throw new System.Exception("Wrong sprite count: " + job.name);
                if (sprites.Any(s => SpriteInkMetrics.BoundsOf(s).width <= 0 || SpriteInkMetrics.BoundsOf(s).height <= 0))
                    throw new System.Exception("Missing painted bounds: " + job.name);
                if (job.kind != "weapon" && sprites.Any(s => s.rect.width != 256 || s.rect.height != 256))
                    throw new System.Exception("Wrong cell size: " + job.name);
                if (job.kind == "weapon")
                {
                    var matching = weapons.Where(w => w.Id == job.name.Substring(7)).ToArray();
                    if (matching.Length == 0) throw new System.Exception("Missing weapon " + job.name);
                    foreach (var weapon in matching) CheckSprites(job.path, new[] { weapon.Sprite });
                }
                else
                {
                    string id = job.kind == "acting" ? job.name.Substring(0, job.name.Length - 7) : job.name;
                    var entry = acting.Find(id);
                    if (entry == null || entry.Id != id) throw new System.Exception("Missing exact acting identity: " + id);
                    string actingPath = File.Exists(Folder + id + "_Acting.png") ? Folder + id + "_Acting.png" : Folder + id + ".png";
                    CheckSprites(actingPath, entry.Curious.Concat(entry.Cheer).Concat(entry.Hurt).Concat(entry.Defeat));
                    if (job.kind == "character")
                    {
                        var matching = units.Where(u => u.Id == id).ToArray();
                        if (matching.Length == 0) throw new System.Exception("Missing unit " + id);
                        foreach (var unit in matching)
                        {
                            CheckSprites(job.path, new[] { unit.Body }.Concat(unit.AnimationFrames));
                            var motion = unit.AuthoredMotion;
                            if (motion == null) throw new System.Exception("Missing authored motion: " + id);
                            CheckSprites(job.path, motion.Idle.Concat(motion.Walk).Concat(motion.Windup).Concat(motion.Release).Concat(motion.Recovery));
                        }
                        var gait = performance.Find("locomotion", id);
                        if (gait != null) CheckSprites(job.path, gait.Frames);
                    }
                }
                total += sprites.Length;
                lines.Add(job.name + " " + expected + " sprites OK");
            }
            lines.Insert(0, "Unity import verified: " + manifest.jobs.Length + " images / " + total + " sprites. Body, authored motion, expanded gait, acting and weapon references point to delivered art. 256px character cells; alpha enabled; no mipmaps; bilinear; clamp.");
            File.WriteAllLines("Docs/Art_Redo_Unity_Verification.txt", lines);
            Debug.Log("ART_REDO_VERIFIED images=" + manifest.jobs.Length + " sprites=" + total);
        }

        private static void CheckSprites(string path, IEnumerable<Sprite> sprites)
        {
            var frames = sprites.ToArray();
            if (frames.Length == 0 || frames.Any(s => s == null || AssetDatabase.GetAssetPath(s) != path))
                throw new System.Exception("Stale or missing sprite reference; expected " + path);
        }

        private static TextureImporter Prepare(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 256; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.compressionQuality = 80;
            return importer;
        }
        private static Sprite Single(string path)
        {
            var importer = Prepare(path); importer.spriteImportMode = SpriteImportMode.Single;
            // the pivot is the weapon's grip (set from the old painting's grip, 24/10/2026); only a fresh sheet starts centred
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); if (settings.spriteAlignment != (int)SpriteAlignment.Custom) settings.spriteAlignment = (int)SpriteAlignment.Center; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static Sprite[] Grid(string path, string id, int cols, int rows)
        {
            var importer = Prepare(path); importer.spriteImportMode = SpriteImportMode.Multiple; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || texture.width % cols != 0 || texture.height % rows != 0) { Debug.Log("ART_REDO bad grid " + path); return null; }
            int w = texture.width / cols, h = texture.height / rows;
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rect => rect.name, rect => rect.spriteID);
            var cells = new List<SpriteRect>();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    string name = id + "_Redo_" + (r * cols + c).ToString("D2");
                    cells.Add(new SpriteRect { name = name, rect = new Rect(c * w, texture.height - (r + 1) * h, w, h), alignment = SpriteAlignment.BottomCenter, pivot = new Vector2(.5f, 0), spriteID = previous.TryGetValue(name, out GUID guid) ? guid : GUID.Generate() });
                }
            provider.SetSpriteRects(cells.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(cells.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            return sprites.Length == cols * rows ? sprites : null;
        }
    }
}
