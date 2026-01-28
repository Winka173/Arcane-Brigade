using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade.Editor
{
    public static class PerformanceExpansionBuilder
    {
        [Serializable] public sealed class ActorSource
        {
            public string Id;
            public string Material;
            public string Source;
            public string Sprite;
            public int Evolution;
            public int ExistingCombatPoses;
            public bool Boss;
            [NonSerialized] public UnityEngine.Sprite Image;
        }
        [Serializable] public sealed class ArtJob
        {
            public string Id;
            public string Kind;
            public int Columns;
            public int Rows;
            public int FramesPerActor;
            public string Reference;
            public string Output;
            public ActorSource[] Actors;
        }
        [Serializable] public sealed class Manifest
        {
            public int Heroes;
            public int Companions;
            public int Enemies;
            public int Weapons;
            public ArtJob[] Jobs;
        }
        private const string Root = "PreviewExports/PerformanceExpansion";
        private static ActorSource Describe(UnitDefinition unit, int evolution = 0)
        {
            AuthoredMotionSet motion = unit.AuthoredMotion;
            Sprite[] poses = motion == null ? unit.AnimationFrames : motion.Idle.Concat(motion.Walk).Concat(motion.Windup).Concat(motion.Release).Concat(motion.Recovery).ToArray();
            return new ActorSource { Id = unit.Id, Material = unit.Material.ToString(), Evolution = evolution, Boss = unit.IsBoss, Image = unit.Body, Source = AssetDatabase.GetAssetPath(unit.Body), Sprite = unit.Body.name, ExistingCombatPoses = poses.Where(sprite => sprite != null).Distinct().Count() };
        }
        public static void Audit()
        {
            Directory.CreateDirectory(Root + "/References");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var jobs = new List<ArtJob>();
            void Group(string kind, IEnumerable<ActorSource> actors, int size, int frames, int columns, int rows)
            {
                ActorSource[] sources = actors.ToArray();
                for (int first = 0; first < sources.Length; first += size)
                {
                    string id = kind + "-" + (first / size).ToString("00");
                    var job = new ArtJob { Id = id, Kind = kind, Columns = columns, Rows = rows, FramesPerActor = frames, Actors = sources.Skip(first).Take(size).ToArray(), Reference = Root + "/References/" + id + ".png", Output = "Assets/ArcaneBrigade/Art/PerformanceExpansion/" + id + "_v1.png" };
                    ReferenceBoard(job); jobs.Add(job);
                }
            }
            CharacterActingLibrary acting = CharacterActingLibrary.Current;
            Group("companions", catalog.Companions.Where(unit => acting.Find(unit.Id) == null).Select(unit => Describe(unit)), 4, 4, 4, 4);
            Group("locomotion", catalog.Champions.Concat(catalog.Companions).Select(unit => Describe(unit)).Where(actor => actor.ExistingCombatPoses < 8), 2, 8, 4, 4);
            var enemies = new List<ActorSource>();
            foreach (UnitDefinition original in catalog.Enemies)
            {
                enemies.Add(Describe(original));
                UnitDefinition evolved = UnityEngine.Object.Instantiate(original);
                EnemyEvolutionLibrary.Apply(evolved, 100); EnemyAffixLibrary.ApplyArt(evolved, 100);
                enemies.Add(Describe(evolved, 1)); UnityEngine.Object.DestroyImmediate(evolved);
            }
            Group("enemies", enemies, 4, 4, 4, 4);
            Group("bosses", catalog.Bosses.Concat(new[] { NightmareAssets.Current.Customer, OutsideStageAssets.Current.Boss }).Select(unit => Describe(unit)), 1, 4, 2, 2);
            Group("weapons", catalog.Weapons.Where(weapon => weapon.Sprite != null && EnsembleArtwork.Current.Weapon(weapon.Id) == null).Select(weapon => new ActorSource { Id = weapon.Id, Material = weapon.Motion + "/" + weapon.Family, Image = weapon.Sprite, Source = AssetDatabase.GetAssetPath(weapon.Sprite), Sprite = weapon.Sprite.name }), 4, 4, 4, 4);
            var manifest = new Manifest { Heroes = catalog.Champions.Length, Companions = catalog.Companions.Length, Enemies = catalog.Enemies.Length, Weapons = catalog.Weapons.Length, Jobs = jobs.ToArray() };
            File.WriteAllText(Root + "/Manifest.json", JsonUtility.ToJson(manifest, true));
            File.WriteAllLines(Root + "/Coverage.tsv", new[] { "Job\tKind\tId\tEvolution\tExistingCombatPoses\tSource\tSprite" }.Concat(jobs.SelectMany(job => job.Actors.Select(actor => job.Id + "\t" + job.Kind + "\t" + actor.Id + "\t" + actor.Evolution + "\t" + actor.ExistingCombatPoses + "\t" + actor.Source + "\t" + actor.Sprite))));
            Debug.Log("PERFORMANCE_EXPANSION_AUDIT " + jobs.Count + " art jobs; " + manifest.Heroes + " heroes, " + manifest.Companions + " companions, " + manifest.Enemies + " enemies, " + manifest.Weapons + " weapons");
        }
        private static void ReferenceBoard(ArtJob job)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            RenderTexture previous = RenderTexture.active;
            var target = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            var material = new Material(Shader.Find("Sprites/Default"));
            try
            {
                var cameraObject = new GameObject("ReferenceCamera"); SceneManager.MoveGameObjectToScene(cameraObject, scene);
                Camera camera = cameraObject.AddComponent<Camera>(); camera.scene = scene; camera.orthographic = true; camera.orthographicSize = 3; camera.transform.position = Vector3.back * 10; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear; camera.cullingMask = 1 << 31; camera.targetTexture = target;
                for (int index = 0; index < job.Actors.Length; index++)
                {
                    ActorSource actor = job.Actors[index];
                    var child = new GameObject(actor.Id); child.layer = 31; SceneManager.MoveGameObjectToScene(child, scene);
                    SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = actor.Image; renderer.sharedMaterial = material;
                    float scale = Mathf.Min(2.45f / actor.Image.bounds.size.x, 2.3f / actor.Image.bounds.size.y);
                    child.transform.localScale = Vector3.one * scale;
                    Vector3 centre = job.Actors.Length == 1 ? Vector3.zero : new Vector3(index % 2 == 0 ? -1.5f : 1.5f, index < 2 ? 1.5f : -1.5f, 0);
                    if (job.Actors.Length == 1) child.transform.localScale *= 2;
                    child.transform.position = centre - Vector3.Scale(actor.Image.bounds.center, child.transform.localScale);
                }
                camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0); image.Apply(); File.WriteAllBytes(job.Reference, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(image); target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(material); EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
