using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Lists, for every weapon in the catalog, what it looks and sounds like in a fight: its shot art (painted key or
    /// sprite), the mote its hit cues are made of, its attack sound, its drawn size and where its shots leave from, and
    /// which of those it shares with other weapons (PreviewExports/WeaponIdentity/Audit.tsv, Shared.txt).</summary>
    public static class WeaponIdentityAudit
    {
        [MenuItem("Arcane Brigade/Weapons/Audit shared looks and sounds")]
        public static void Run()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            MethodInfo fire = typeof(ArcaneCueLibrary).GetMethod("FireId", BindingFlags.NonPublic | BindingFlags.Static);
            var rows = new List<string> { "weapon\tkind\tmotion\tfamily\tmechanic\tsignature\tarsenal\tshot\tmote\tsound\tvisual_width\tsprite_ink_w\tsprite_ink_h\tmount\tmuzzle\tmuzzle_offset\tprojectile_width\tsprite" };
            var shot = new Dictionary<string, List<string>>(); var mote = new Dictionary<string, List<string>>(); var sound = new Dictionary<string, List<string>>();
            foreach (WeaponDefinition w in catalog.Weapons.Where(w => w != null))
            {
                string shotKey = "-";
                if (w.Kind == WeaponKind.Projectile)
                {
                    var state = new ProjectileState { Faction = Faction.Companion, Weapon = w, Sprite = w.ProjectileSprite, Arsenal = w.Arsenal };
                    shotKey = CombatVfxArtwork.ProjectileKey(state, null) ?? ("sprite:" + (w.ProjectileSprite != null ? w.ProjectileSprite.name : "none"));
                }
                string moteKey = ArcadeEffects.MoteKey(w);
                var e = new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Companion, 0, Vector2.right, w, false);
                string soundKey = fire != null ? (string)fire.Invoke(null, new object[] { e, false }) : "?";
                Rect ink = w.Sprite != null ? SpriteInkMetrics.BoundsOf(w.Sprite) : default;
                rows.Add(string.Join("\t", w.Id, w.Kind, w.Motion, w.Family, w.Mechanic, w.Signature, w.Arsenal, shotKey, moteKey, soundKey,
                    w.VisualWidth.ToString("0.00"), ink.width.ToString("0.00"), ink.height.ToString("0.00"), w.MountDistance.ToString("0.00"), w.MuzzleDistance.ToString("0.00"), w.ProjectileMuzzleOffset.ToString("0.00"),
                    w.ProjectileVisualWidth.ToString("0.00"), w.Sprite != null ? AssetDatabase.GetAssetPath(w.Sprite) + "#" + w.Sprite.name : "none"));
                if (w.Kind == WeaponKind.Projectile) Add(shot, shotKey, w.Id);
                Add(mote, moteKey, w.Id); Add(sound, soundKey ?? "none", w.Id);
            }
            var shared = new List<string>();
            foreach (var (name, map) in new[] { ("SHOT", shot), ("MOTE", mote), ("SOUND", sound) })
                foreach (var pair in map.Where(p => p.Value.Count > 1).OrderByDescending(p => p.Value.Count))
                    shared.Add(name + "\t" + pair.Key + "\t" + pair.Value.Count + "\t" + string.Join(",", pair.Value));
            Directory.CreateDirectory("PreviewExports/WeaponIdentity");
            File.WriteAllLines("PreviewExports/WeaponIdentity/Audit.tsv", rows);
            File.WriteAllLines("PreviewExports/WeaponIdentity/Shared.txt", shared);
            Debug.Log("WEAPON_IDENTITY_AUDIT " + (rows.Count - 1));
        }
        private static void Add(Dictionary<string, List<string>> map, string key, string id) { if (!map.TryGetValue(key, out var list)) map[key] = list = new List<string>(); list.Add(id); }
    }
}
