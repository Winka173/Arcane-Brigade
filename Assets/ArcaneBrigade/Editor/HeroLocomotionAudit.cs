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
    /// <summary>Audits the hero poses selected by UnitVisual, rather than counting legacy motion arrays.</summary>
    public static class HeroLocomotionAudit
    {
        const string Root = "PreviewExports/HeroLocomotion";
        sealed class Row
        {
            public string Id, Source;
            public Sprite[] Idle, Walk;
            public float Height;
        }

        public static void Run()
        {
            Directory.CreateDirectory(Root);
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var notes = new List<string>();
            var rows = new List<Row>();
            var table = new List<string> { "Hero\tRuntimeSource\tIdleDistinct\tWalkDistinct\tIdleObserved\tWalkObserved\tIdleSprites\tWalkSprites" };
            void Check(bool ok, string text)
            {
                notes.Add((ok ? "PASS " : "FAIL ") + text);
                File.WriteAllLines(Root + "/Checks.txt", notes);
                if (!ok) throw new InvalidOperationException(text);
            }
            var reducedProperty = typeof(PresentationSettings).GetProperty("ReducedMotion");
            bool previousReduced = PresentationSettings.ReducedMotion;
            try
            {
                reducedProperty.SetValue(null, false);
                Check(catalog.Champions.Length == 31, "Audit the entire current 31-hero roster");
                foreach (var hero in catalog.Champions)
                {
                    var gait = ExpandedPerformanceLibrary.Current?.Find("locomotion", hero.Id);
                    var motion = hero.AuthoredMotion;
                    var row = new Row { Id = hero.Id,
                        Source = gait != null ? "ExpandedPerformance/" + hero.Id : AssetDatabase.GetAssetPath(motion),
                        Idle = gait != null ? gait.Frames.Take(2).ToArray() : motion?.Idle,
                        Walk = gait != null ? gait.Frames.Skip(2).Take(3).ToArray() : motion?.Walk,
                        Height = gait != null ? gait.ReferenceHeight : motion != null ? motion.ReferenceHeight : 1 };
                    Check(row.Idle != null && row.Idle.All(s => s != null) && row.Idle.Distinct().Count() >= 2, hero.Id + " has distinct painted idle poses");
                    Check(row.Walk != null && row.Walk.All(s => s != null) && row.Walk.Distinct().Count() >= 3, hero.Id + " has distinct painted walk poses");
                    Check(!row.Idle.Intersect(row.Walk).Any(), hero.Id + " walking uses separate drawings from idle");
                    Check(!rows.Any(r => r.Idle.Intersect(row.Idle).Any() || r.Walk.Intersect(row.Walk).Any()), hero.Id + " does not borrow another hero's locomotion sprites");

                    var definition = UnityEngine.Object.Instantiate(hero);
                    var probeWeapon = UnityEngine.Object.Instantiate(hero.Weapon);
                    // Isolate body animation from edit-mode weapon effect initialization. No source assets are changed.
                    probeWeapon.Id = "HeroLocomotionProbe"; probeWeapon.Motion = WeaponMotion.Recoil;
                    definition.Weapon = probeWeapon;
                    var state = new UnitState { Id = 101, Definition = definition, Faction = Faction.Champion,
                        Health = definition.Health, Alive = true, Facing = Vector2.right, AttackDirection = Vector2.right,
                        LastHit = -100, LastSupport = -100, AttackStarted = -100 };
                    var view = UnityEngine.Object.Instantiate(catalog.Arena.UnitPrefab).GetComponent<UnitVisual>();
                    try
                    {
                        view.Bind(state, catalog.Arena, true);
                        view.PreviewActing(-1, 0, false);
                        var idleSeen = new HashSet<Sprite>(); var walkSeen = new HashSet<Sprite>();
                        for (int tick = 0; tick < 220; tick++)
                        {
                            view.Display(10 + tick * .025f, 1);
                            idleSeen.Add(view.BodyRenderer.sprite);
                        }
                        Check(row.Idle.All(idleSeen.Contains) && idleSeen.All(row.Idle.Contains), hero.Id + " actual UnitVisual cycles every idle pose");
                        state.PreviousPosition = Vector2.left * .05f; state.Velocity = Vector2.right * 2;
                        for (int tick = 0; tick < 60; tick++)
                        {
                            view.Display(16 + tick * .025f, 1);
                            walkSeen.Add(view.BodyRenderer.sprite);
                        }
                        Check(row.Walk.All(walkSeen.Contains) && walkSeen.All(row.Walk.Contains), hero.Id + " actual UnitVisual cycles every walk pose");
                        view.Display(18, 1); var paused = view.BodyRenderer.sprite;
                        for (int i = 0; i < 5; i++) view.Display(18, 1);
                        Check(view.BodyRenderer.sprite == paused, hero.Id + " does not advance frames while simulation time is paused");
                        state.PreviousPosition = state.Position; state.Velocity = Vector2.zero;
                        view.Display(19, 1);
                        Check(row.Idle.Contains(view.BodyRenderer.sprite), hero.Id + " returns to idle after movement stops");
                        reducedProperty.SetValue(null, true);
                        view.ReducedMotion = true;
                        var reducedSeen = new HashSet<Sprite>();
                        for (int tick = 0; tick < 20; tick++) { view.Display(20 + tick * .1f, 1); reducedSeen.Add(view.BodyRenderer.sprite); }
                        Check(reducedSeen.Count == 1 && !reducedSeen.Contains(null), hero.Id + " Reduced Motion deliberately keeps a stable complete pose");
                        reducedProperty.SetValue(null, false);
                        table.Add(string.Join("\t", hero.Id, row.Source, row.Idle.Distinct().Count(), row.Walk.Distinct().Count(), idleSeen.Count, walkSeen.Count,
                            string.Join(";", row.Idle.Select(PathOf)), string.Join(";", row.Walk.Select(PathOf))));
                    }
                    finally
                    {
                        reducedProperty.SetValue(null, false);
                        UnityEngine.Object.DestroyImmediate(view.gameObject);
                        UnityEngine.Object.DestroyImmediate(probeWeapon); UnityEngine.Object.DestroyImmediate(definition);
                    }
                    rows.Add(row);
                }
                File.WriteAllLines(Root + "/Coverage.tsv", table);
                for (int first = 0; first < rows.Count; first += 8) Render(rows.Skip(first).Take(8).ToArray(), first / 8);
                File.WriteAllText(Root + "/Verification.json", "{\"Passed\":true,\"Heroes\":" + rows.Count + ",\"IdlePoses\":" + rows.Sum(r => r.Idle.Distinct().Count()) + ",\"WalkPoses\":" + rows.Sum(r => r.Walk.Distinct().Count()) + ",\"MissingIdleOrWalk\":0,\"Checks\":" + notes.Count + "}");
                Debug.Log("HERO_LOCOMOTION_AUDIT " + rows.Count + " heroes; " + notes.Count + " checks PASS; no missing idle/walk");
            }
            finally { reducedProperty.SetValue(null, previousReduced); }
        }

        static string PathOf(Sprite sprite) => AssetDatabase.GetAssetPath(sprite) + "#" + sprite.name;

        static void Render(Row[] rows, int page)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            var target = new RenderTexture(1200, 1600, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(1200, 1600, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            var material = new Material(Shader.Find("Sprites/Default"));
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            try
            {
                var cameraObject = new GameObject("HeroLocomotionCamera"); SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.AddComponent<Camera>(); camera.scene = scene; camera.orthographic = true;
                camera.orthographicSize = 8; camera.transform.position = Vector3.back * 10;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.045f, .065f, .095f);
                camera.cullingMask = 1 << 31; camera.targetTexture = target;
                void Label(string text, Vector3 position, float size)
                {
                    var go = new GameObject("Caption"); go.layer = 31; SceneManager.MoveGameObjectToScene(go, scene);
                    var label = go.AddComponent<TextMesh>(); label.font = font; label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
                    label.text = text; label.fontSize = 48; label.characterSize = size; label.anchor = TextAnchor.MiddleCenter;
                    label.color = new Color(.9f, .86f, .72f); go.transform.position = position;
                }
                string[] titles = { "IDLE 0", "IDLE 1", "WALK 0", "WALK 1", "WALK 2" };
                for (int column = 0; column < 5; column++) Label(titles[column], new Vector3((column - 2) * 2.1f, 7.55f, -1), .07f);
                for (int index = 0; index < rows.Length; index++)
                {
                    var row = rows[index]; float y = 6.65f - index * 1.85f;
                    var frames = row.Idle.Take(2).Concat(row.Walk.Take(3)).ToArray();
                    for (int column = 0; column < frames.Length; column++)
                    {
                        var go = new GameObject(row.Id + column); go.layer = 31; SceneManager.MoveGameObjectToScene(go, scene);
                        var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = frames[column]; renderer.sharedMaterial = material;
                        float scale = 1.28f / Mathf.Max(.01f, row.Height); go.transform.localScale = Vector3.one * scale;
                        go.transform.position = new Vector3((column - 2) * 2.1f, y, 0) - Vector3.Scale(frames[column].bounds.center, go.transform.localScale);
                    }
                    Label(row.Id, new Vector3(0, y - .83f, -1), .065f);
                }
                camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(Root + "/Hero_Poses_" + page + ".png", image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; target.Release(); UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(material); EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
