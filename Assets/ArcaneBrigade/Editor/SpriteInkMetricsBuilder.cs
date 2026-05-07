using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class SpriteInkMetricsBuilder
    {
        public static void Campaign() => K17CampaignDiagnostics.RunInto("PreviewExports/InkAndBalance");
        public static void Verify()
        {
            Build();
            Audit();
            ShopLayerChecks.Verify();
        }
        public static void Audit()
        {
            Directory.CreateDirectory("PreviewExports/InkAndBalance");
            var notes = new List<string>();
            void Require(bool condition, string caption)
            {
                if (!condition) throw new System.InvalidOperationException(caption);
                notes.Add("PASS " + caption);
            }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                if (weapon.Sprite == null || weapon.Kind == WeaponKind.Contact) continue;
                Rect ink = SpriteInkMetrics.BoundsOf(weapon.Sprite);
                for (int tier = 1; tier <= 3; tier++)
                {
                    Vector2 shape = UnitVisual.TierWeaponShape(weapon, tier);
                    Vector3 scale = UnitVisual.FitCompanionWeapon(new Vector3(shape.x, shape.y, 1) * (weapon.VisualWidth / weapon.Sprite.bounds.size.x), ink, 1, weapon.Kind == WeaponKind.Melee);
                    float longest = Mathf.Max(ink.width * scale.x, ink.height * scale.y);
                    float minimum = weapon.Kind == WeaponKind.Melee ? 1.05f : 0.65f;
                    float maximum = weapon.Kind == WeaponKind.Melee ? 1.75f : 0.95f;
                    Require(longest >= minimum - 0.001f && longest <= maximum + 0.001f, "Painted weapon size " + weapon.Id + " tier " + tier);
                }
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171077);
                float previousHealth = 0, previousRate = 0;
                for (int wave = 1; wave <= 30; wave++)
                {
                    session.State.Wave = wave;
                    CheckpointCatalog combat = session.PrepareCombat();
                    float health = CarnivalBalance.EnemyHealth(wave) * (1 + (wave - 1) * 0.1f) * (1 + Mathf.Max(0, wave - 10) * 0.055f);
                    float rate = 1 / CarnivalBalance.SpawnInterval(wave);
                    Require(wave == 1 || health >= previousHealth && health <= previousHealth * 1.25f, "No abrupt health cliff wave " + wave);
                    Require(wave == 1 || rate >= previousRate && rate <= previousRate * 1.2f, "No abrupt spawn-rate cliff wave " + wave);
                    Require(combat.EnemyBudget > 0 && combat.SpawnInterval >= 0.25f, "Bounded enemy pressure wave " + wave);
                    if (combat.Boss != null)
                    {
                        UnitDefinition source = catalog.Bosses.First(boss => boss.Id == combat.Boss.Id);
                        float baseline = source.Health * (1 + (wave - 1) * 0.1f) * catalog.BossHealthMultiplier * 10 * CarnivalBalance.EnemyHealth(wave);
                        Require(combat.Boss.Health < baseline * (1 + Mathf.Max(0, wave - 10) * 0.055f), "Boss bulk below prior x10 boost wave " + wave);
                    }
                    notes.Add("Wave " + wave + ": interval=" + combat.SpawnInterval.ToString("F3") + ", cap=" + combat.EnemyBudget + ", normalHP=" + health.ToString("F2"));
                    previousHealth = health; previousRate = rate;
                }
            }
            File.WriteAllLines("PreviewExports/InkAndBalance/Focused_Checks.txt", notes);
            Debug.Log("INK_BALANCE_CHECKS_PASSED " + notes.Count(note => note.StartsWith("PASS ")));
        }
        public static void Build()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var sprites = new HashSet<Sprite>();
            foreach (WeaponDefinition weapon in catalog.Weapons) if (weapon.Sprite != null) sprites.Add(weapon.Sprite);
            foreach (ItemDefinition item in catalog.Items) if (item.Icon != null) sprites.Add(item.Icon);
            foreach (UnitDefinition unit in catalog.Companions)
            {
                if (unit.Body != null) sprites.Add(unit.Body);
                foreach (Sprite frame in unit.AnimationFrames) if (frame != null) sprites.Add(frame);
            }
            // Friendly sizing needs actual painted bounds for every alternate pose family,
            // including heroes and expansion sheets (which have generous transparent cells).
            var friendlyAtlases = new HashSet<string>();
            void AddAtlas(Sprite sprite)
            {
                if (sprite == null) return;
                string path = AssetDatabase.GetAssetPath(sprite);
                if (!friendlyAtlases.Add(path)) return;
                foreach (Sprite frame in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()) sprites.Add(frame);
            }
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions))
            {
                AddAtlas(unit.Body);
                if (unit.AuthoredMotion != null)
                    foreach (var frames in new[] { unit.AuthoredMotion.Idle, unit.AuthoredMotion.Walk, unit.AuthoredMotion.Windup, unit.AuthoredMotion.Release, unit.AuthoredMotion.Recovery })
                        if (frames != null) foreach (Sprite frame in frames) AddAtlas(frame);
            }
            if (CharacterActingLibrary.Current != null)
                foreach (var entry in CharacterActingLibrary.Current.Entries)
                    foreach (var frames in new[] { entry.Curious, entry.Cheer, entry.Hurt, entry.Defeat })
                        if (frames != null) foreach (Sprite frame in frames) AddAtlas(frame);
            if (ExpandedPerformanceLibrary.Current != null)
                foreach (var entry in ExpandedPerformanceLibrary.Current.Entries.Where(e => e.Kind == "locomotion"))
                    foreach (Sprite frame in entry.Frames) AddAtlas(frame);
            foreach (Sprite stand in Resources.Load<CarnivalStageAssets>("CarnivalStageAssets").Stands) sprites.Add(stand);
            NightmareAssets nightmare = NightmareAssets.Current;
            if (nightmare != null) foreach (Sprite sprite in nightmare.Scenery) if (sprite != null) sprites.Add(sprite);
            GroundedSceneryArtwork scenery = GroundedSceneryArtwork.Current;
            if (scenery != null) foreach (Sprite sprite in scenery.Cabinets) if (sprite != null) sprites.Add(sprite);
            var entries = new List<SpriteInkMetrics.Entry>();
            foreach (var group in sprites.GroupBy(sprite => AssetDatabase.GetAssetPath(sprite.texture)))
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!texture.LoadImage(File.ReadAllBytes(group.Key))) throw new System.InvalidOperationException("Cannot read sprite ink " + group.Key);
                    Color32[] pixels = texture.GetPixels32();
                    foreach (Sprite sprite in group)
                    {
                        Rect cell = sprite.rect;
                        float ratioX = (float)texture.width / sprite.texture.width;
                        float ratioY = (float)texture.height / sprite.texture.height;
                        int left = Mathf.CeilToInt(cell.xMax * ratioX), bottom = Mathf.CeilToInt(cell.yMax * ratioY), right = Mathf.FloorToInt(cell.xMin * ratioX), top = Mathf.FloorToInt(cell.yMin * ratioY);
                        for (int vertical = Mathf.FloorToInt(cell.yMin * ratioY); vertical < Mathf.CeilToInt(cell.yMax * ratioY); vertical++)
                            for (int horizontal = Mathf.FloorToInt(cell.xMin * ratioX); horizontal < Mathf.CeilToInt(cell.xMax * ratioX); horizontal++)
                                if (pixels[vertical * texture.width + horizontal].a >= 40)
                                {
                                    left = Mathf.Min(left, horizontal); right = Mathf.Max(right, horizontal + 1);
                                    bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical + 1);
                                }
                        if (right <= left || top <= bottom) throw new System.InvalidOperationException("Empty sprite ink " + sprite.name);
                        float scale = sprite.pixelsPerUnit;
                        entries.Add(new SpriteInkMetrics.Entry { Sprite = sprite, Ink = new Rect((left / ratioX - cell.x - sprite.pivot.x) / scale, (bottom / ratioY - cell.y - sprite.pivot.y) / scale, (right - left) / ratioX / scale, (top - bottom) / ratioY / scale) });
                    }
                }
                finally { Object.DestroyImmediate(texture); }
            }
            const string path = "Assets/ArcaneBrigade/Resources/SpriteInkMetrics.asset";
            SpriteInkMetrics data = AssetDatabase.LoadAssetAtPath<SpriteInkMetrics>(path);
            if (data == null) { data = ScriptableObject.CreateInstance<SpriteInkMetrics>(); AssetDatabase.CreateAsset(data, path); }
            data.Entries = entries.OrderBy(entry => entry.Sprite.name).ThenBy(entry => AssetDatabase.GetAssetPath(entry.Sprite)).ToArray();
            EditorUtility.SetDirty(data); AssetDatabase.SaveAssets(); SpriteInkMetrics.Invalidate();
            Debug.Log("SPRITE_INK_METRICS_BUILT " + entries.Count);
        }
    }
}
