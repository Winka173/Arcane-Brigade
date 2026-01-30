using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class FinalQualityArtBuilder
    {
        private const string Root = "PreviewExports/FinalQuality";
        public static void Prepare()
        {
            Directory.CreateDirectory(Root + "/References"); Directory.CreateDirectory("Assets/ArcaneBrigade/Art/FinalQuality");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var jobs = new List<PerformanceExpansionBuilder.ArtJob>();
            foreach (UnitDefinition hero in catalog.Champions.Take(10))
            {
                var job = new PerformanceExpansionBuilder.ArtJob { Id = hero.Id, Kind = "hero-final", Columns = 4, Rows = 3, FramesPerActor = 12,
                    Actors = new[] { Describe(hero.Id, hero.Body) }, Reference = Root + "/References/" + hero.Id + ".png", Output = "Assets/ArcaneBrigade/Art/FinalQuality/" + hero.Id + "_v1.png" };
                PerformanceExpansionBuilder.ReferenceBoard(job); jobs.Add(job);
            }
            var props = NightmareAssets.Current.Scenery.Select((sprite, index) => Describe("landmark-" + index, sprite)).Concat(NightmareAssets.Current.GrandStage.Select((sprite, index) => Describe("architecture-" + index, sprite))).ToArray();
            for (int first = 0; first < props.Length; first += 4)
            {
                string id = "furniture-" + (first / 4).ToString("00");
                var job = new PerformanceExpansionBuilder.ArtJob { Id = id, Kind = "furniture-final", Columns = 4, Rows = 4, FramesPerActor = 4, Actors = props.Skip(first).Take(4).ToArray(), Reference = Root + "/References/" + id + ".png", Output = "Assets/ArcaneBrigade/Art/FinalQuality/" + id + "_v1.png" };
                PerformanceExpansionBuilder.ReferenceBoard(job); jobs.Add(job);
            }
            File.WriteAllText(Root + "/Art_Manifest.json", JsonUtility.ToJson(new PerformanceExpansionBuilder.Manifest { Jobs = jobs.ToArray() }, true));
            Debug.Log("FINAL_QUALITY_ART_PREPARED " + jobs.Count);
        }
        private static PerformanceExpansionBuilder.ActorSource Describe(string id, Sprite sprite) => new PerformanceExpansionBuilder.ActorSource { Id = id, Source = AssetDatabase.GetAssetPath(sprite), Sprite = sprite.name, Image = sprite };
        public static void Import()
        {
            var manifest = JsonUtility.FromJson<PerformanceExpansionBuilder.Manifest>(File.ReadAllText(Root + "/Art_Manifest.json"));
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var props = new List<FinalQualityArtwork.Entry>();
            foreach (PerformanceExpansionBuilder.ArtJob job in manifest.Jobs)
            {
                if (!File.Exists(job.Output)) throw new System.InvalidOperationException("Missing final art " + job.Id);
                bool hero = job.Kind == "hero-final";
                Sprite[] sprites = K25AtlasImporter.Import(job.Output, "Final" + job.Id.Replace("-", ""), job.Columns, job.Rows, Enumerable.Range(0, job.Actors.Length * job.FramesPerActor).ToArray(), Enumerable.Range(0, job.Rows + 1).Select(row => row * 1024 / job.Rows).ToArray(), true, hero);
                PerformanceSpriteOutline.Apply(job.Output, !hero && job.Id != "furniture-04"); sprites = AssetDatabase.LoadAllAssetsAtPath(job.Output).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
                if (hero)
                {
                    UnitDefinition unit = catalog.Champions.First(candidate => candidate.Id == job.Id);
                    string path = "Assets/ArcaneBrigade/Art/FinalQuality/" + job.Id + "Motion.asset";
                    AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(path);
                    if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, path); }
                    motion.Idle = sprites.Take(2).ToArray(); motion.Walk = sprites.Skip(2).Take(3).ToArray(); motion.Windup = new[] { sprites[5] }; motion.Release = new[] { sprites[6] }; motion.Recovery = new[] { sprites[7], sprites[7] };
                    unit.AuthoredMotion = motion; unit.Body = sprites[0]; unit.AnimationFrames = sprites.Take(4).ToArray();
                    var gait = ExpandedPerformanceLibrary.Current.Find("locomotion", unit.Id);
                    if (gait != null) { gait.Frames = sprites.Take(8).ToArray(); gait.ReferenceHeight = sprites[0].bounds.size.y; }
                    CharacterActingEntry acting = CharacterActingLibrary.Current.Find(unit.Id);
                    acting.Curious = new[] { sprites[8] }; acting.Cheer = new[] { sprites[9] }; acting.Hurt = new[] { sprites[10] }; acting.Defeat = new[] { sprites[11] }; acting.ReferenceHeight = sprites[0].bounds.size.y;
                    EditorUtility.SetDirty(unit); EditorUtility.SetDirty(motion);
                }
                else for (int index = 0; index < job.Actors.Length; index++) props.Add(new FinalQualityArtwork.Entry { Id = job.Actors[index].Id, Frames = sprites.Skip(index * 4).Take(4).ToArray() });
            }
            const string artworkPath = "Assets/ArcaneBrigade/Resources/FinalQualityArtwork.asset";
            FinalQualityArtwork artwork = AssetDatabase.LoadAssetAtPath<FinalQualityArtwork>(artworkPath);
            if (artwork == null) { artwork = ScriptableObject.CreateInstance<FinalQualityArtwork>(); AssetDatabase.CreateAsset(artwork, artworkPath); }
            artwork.Props = props.ToArray(); EditorUtility.SetDirty(artwork); EditorUtility.SetDirty(ExpandedPerformanceLibrary.Current); EditorUtility.SetDirty(CharacterActingLibrary.Current);
            AssetDatabase.SaveAssets(); SpriteInkMetricsBuilder.Build();
            Debug.Log("FINAL_QUALITY_ART_IMPORTED " + manifest.Jobs.Length + " sheets; " + props.Count + " props; 10 heroes");
        }
    }
}
