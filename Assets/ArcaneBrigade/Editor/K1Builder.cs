using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K1Builder
    {
        private const string Root = "Assets/ArcaneBrigade/";
        public static void ApplyAndCheck()
        {
            Directory.CreateDirectory("PreviewExports/K1");
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(Root + "Resources/RunCatalog.asset");
            catalog.WorldSize = new Vector2(34, 22);
            catalog.CombatViewHeight = 5.4f;
            catalog.RestViewHeight = 6f;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            BuildMap(catalog);
            CheckExisting();
        }

        private static void BuildMap(RunCatalog catalog)
        {
            EditorSceneManager.OpenScene(Root + "Scenes/RunArena.unity");
            foreach (SpriteRenderer renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
                if (renderer.sortingOrder < 0) renderer.gameObject.SetActive(false);
            GameObject old = GameObject.Find("K1World");
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            var world = new GameObject("K1World");
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/Presentation/NightmareFloor_v1.png");
            string cropPath = Root + "Art/Presentation/K1Floor.asset";
            Sprite floor = AssetDatabase.LoadAssetAtPath<Sprite>(cropPath);
            if (floor == null)
            {
                floor = Sprite.Create(texture, new Rect(texture.width * 0.18f, texture.height * 0.18f, texture.width * 0.64f, texture.height * 0.64f), new Vector2(0.5f, 0.5f), 100);
                AssetDatabase.CreateAsset(floor, cropPath);
            }
            Place(world.transform, "Floor", floor, Vector2.zero, catalog.WorldSize, new Color(0.75f, 0.75f, 0.85f), -100);
            Sprite block = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Checkpoint/Square.png");
            if (block == null) throw new InvalidOperationException("Boundary square sprite missing");
            Vector2 half = catalog.WorldSize * 0.5f;
            for (int side = 0; side < 4; side++)
            {
                bool horizontal = side < 2;
                float length = horizontal ? catalog.WorldSize.x : catalog.WorldSize.y;
                int count = Mathf.CeilToInt(length / 0.9f);
                for (int index = 0; index < count; index++)
                {
                    float along = -length * 0.5f + (index + 0.5f) * length / count;
                    float jitter = Mathf.Sin(index * 2.17f + side) * 0.10f;
                    Vector2 position = horizontal ? new Vector2(along, (side == 0 ? -half.y : half.y) + jitter) : new Vector2((side == 2 ? -half.x : half.x) + jitter, along);
                    Vector2 size = horizontal ? new Vector2(length / count + 0.12f, 0.65f) : new Vector2(0.65f, length / count + 0.12f);
                    Place(world.transform, "Boundary_" + side + "_" + index, block, position, size, index % 3 == 0 ? new Color(0.29f, 0.20f, 0.39f) : new Color(0.20f, 0.14f, 0.29f), -80);
                }
            }
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        private static void Place(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, Color color, int order)
        {
            SpriteRenderer renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.transform.position = position;
            renderer.sprite = sprite;
            renderer.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
            renderer.color = color;
            renderer.sortingOrder = order;
        }

        public static void CheckExisting()
        {
            Directory.CreateDirectory("PreviewExports/K1");
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(Root + "Resources/RunCatalog.asset");
            FullRunChecks.Run(catalog);
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            require(catalog.WorldSize.x > catalog.CombatViewHeight * 2 * 16 / 9 && catalog.WorldSize.y > catalog.CombatViewHeight * 2, "World exceeds viewport in both axes");
            foreach (Vector2 corner in new[] { new Vector2(-17, -11), new Vector2(17, -11), new Vector2(-17, 11), new Vector2(17, 11) })
            {
                Vector2 center = ArenaCamera.ClampCenter(corner, catalog.WorldSize, catalog.CombatViewHeight, 16f / 9f);
                require(Mathf.Abs(center.x) + catalog.CombatViewHeight * 16f / 9f <= 17.001f && Mathf.Abs(center.y) + catalog.CombatViewHeight <= 11.001f, "Camera clamps " + corner);
            }
            require(ArenaCamera.ClampCenter(Vector2.one * 100, Vector2.one, 6, 16f / 9f) == Vector2.zero, "Small arena stays centered");
            var gate = new RunPauseGate { Manual = true, Details = true };
            gate.Details = false;
            require(gate.Paused, "Closing Details preserves manual pause");
            gate.Manual = false; gate.FocusLost = true; gate.Navigation = true; gate.Navigation = false;
            require(gate.Paused, "Closing navigation preserves focus pause");
            gate.FocusLost = false; gate.Backgrounded = true; gate.ResetSession();
            require(gate.Paused, "Restart preserves background pause");
            gate.Backgrounded = false;
            require(!gate.Paused, "Pause clears only after all owners clear");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 131026);
                string before = JsonUtility.ToJson(session.State);
                RunBuildReadModel model = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                require(before == JsonUtility.ToJson(session.State) && model.Stats.Count >= 10, "Details capture is read-only including RNG");
                session.State.SchemaRevision = 0;
                session.State.SpecialOffers = null;
                session.Restore(session.State);
                require(session.State.SchemaRevision == RunSnapshot.CurrentSchemaRevision && session.State.SpecialOffers != null, "Legacy schema migrates without dropping IDs");
                RunSnapshot future = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                future.SchemaRevision = RunSnapshot.CurrentSchemaRevision + 1;
                bool rejected = false;
                try { session.Restore(future); } catch (InvalidOperationException) { rejected = true; }
                require(rejected, "Unknown future schema rejected");
                string root = Path.Combine(Application.temporaryCachePath, "ArcaneK1Checks", Guid.NewGuid().ToString("N"));
                var storage = new RunStorage(root);
                storage.SaveRun(session.State); storage.SaveRun(session.State);
                File.WriteAllText(Path.Combine(root, "run.json"), "corrupt");
                require(storage.LoadRun(session) != null, "Checksum failure recovers backup");
                string blocked = Path.Combine(root, "blocked");
                File.WriteAllText(blocked, "fixture");
                bool writeRejected = false;
                try { new RunStorage(blocked).SaveRun(session.State); } catch (IOException) { writeRejected = true; }
                require(writeRejected, "Failed save rejected without touching backup");
                session.State.Phase = SessionPhase.Shop;
                session.State.Gold = 100;
                session.Reroll();
                string transactionBefore = JsonUtility.ToJson(session.State);
                RunSnapshot rollback = JsonUtility.FromJson<RunSnapshot>(transactionBefore);
                session.Reroll();
                try { new RunStorage(blocked).SaveRun(session.State); } catch (IOException) { session.Restore(rollback); }
                require(transactionBefore == JsonUtility.ToJson(session.State), "Failed transaction restores gold, stock and RNG");
                using (var walking = new RunSession(catalog))
                {
                    walking.Start(catalog.Champions[0].Id, 131026);
                    var simulation = new CombatSimulation(walking.PrepareCombat());
                    foreach (Vector2 direction in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), Vector2.one })
                    {
                        simulation.Start(131026); simulation.EnterRest();
                        for (int tick = 0; tick < 600; tick++) simulation.StepRest(1f / 60, direction);
                        Vector2 reached = simulation.Champion.Position;
                        require(Mathf.Abs(reached.x) > catalog.WorldSize.x * 0.5f - 1 && Mathf.Abs(reached.y) > catalog.WorldSize.y * 0.5f - 1, "Movement reaches corner " + direction);
                    }
                }
                session.State.Roster.Clear();
                // 64 friend/weapon pairs: far past any UI slot count, without the full companions x weapons table (thousands of
                // entries made Capture, quadratic in roster size, run for over 30 minutes)
                const int largeRoster = 64;
                foreach (UnitDefinition unit in catalog.Companions)
                    foreach (WeaponDefinition weapon in catalog.Weapons) if (session.State.Roster.Count < largeRoster) session.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = weapon.Id });
                session.Restore(session.State);
                int shown = RunBuildReadModel.Capture(session, catalog, session.State.Health).Companions.Count;
                require(session.State.Roster.Count >= Mathf.Min(largeRoster, catalog.Companions.Length * catalog.Weapons.Length) && shown >= session.State.Roster.Count, "Large roster Details has no UI slot cap; the travelling story friend is listed too (" + shown + " of " + session.State.Roster.Count + ")");
            }
            foreach (Font font in new[] { ArcadeTypography.Body, ArcadeTypography.Heading, ArcadeTypography.Numbers })
            {
                require(font != null, "Font resource loads");
                string glyphs = "Đđ Ăă Ââ Êê Ôô Ơơ Ưư Ắằ Ấầ Ếề Ốồ Ớờ Ứừ ộ ệ ỹ ỷ ự 0123456789";
                font.RequestCharactersInTexture(glyphs, 24);
                bool all = true;
                foreach (char glyph in glyphs) if (!char.IsWhiteSpace(glyph) && !font.HasCharacter(glyph)) all = false;
                require(all, "Vietnamese glyph coverage " + font.name);
            }
            File.WriteAllLines("PreviewExports/K1/K0_K1_Checks.txt", results);
            Debug.Log("ARCANE_K0_K1_CHECKS_PASSED " + results.Count);
        }
    }
}
