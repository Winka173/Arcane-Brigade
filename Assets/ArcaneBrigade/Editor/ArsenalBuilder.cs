using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The Bizarre Arsenal (Docs/Bizarre_Arsenal.md). Gives every playable character its own weapon from HeroLoadout
    /// (the interim one while its own new weapon still waits for art). Re-runnable; changes nothing that already matches.</summary>
    public static class ArsenalBuilder
    {
        private const string Art = "Assets/ArcaneBrigade/Art/Arsenal/";

        /// <summary>Wires every Arsenal weapon whose painted art exists (held sprite plus every listed piece), fills ArsenalArtwork
        /// from whatever art is there, then gives heroes their loadouts. A weapon still waiting for art stays out of the game.</summary>
        [MenuItem("Arcane Brigade/Arsenal/Apply (weapons whose art exists, then heroes)")]
        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var status = new List<string>();
            var pieces = new SortedDictionary<string, SortedDictionary<int, Sprite>>(StringComparer.Ordinal);
            if (Directory.Exists(Art))
                foreach (string file in Directory.GetFiles(Art, "*_v1.png"))
                {
                    string name = Path.GetFileNameWithoutExtension(file); name = name.Substring(0, name.Length - 3);
                    int frame = 0; int cut = name.LastIndexOf("_f", StringComparison.Ordinal);
                    if (cut > 0 && int.TryParse(name.Substring(cut + 2), out frame)) name = name.Substring(0, cut); else frame = 0;
                    AssetDatabase.ImportAsset(file.Replace(Path.DirectorySeparatorChar, '/'), ImportAssetOptions.ForceSynchronousImport);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file.Replace(Path.DirectorySeparatorChar, '/'));
                    if (sprite == null) continue;
                    if (!pieces.TryGetValue(name, out var frames)) pieces[name] = frames = new SortedDictionary<int, Sprite>();
                    frames[frame] = sprite;
                }
            const string artworkPath = "Assets/ArcaneBrigade/Resources/ArsenalArtwork.asset";
            ArsenalArtwork artwork = AssetDatabase.LoadAssetAtPath<ArsenalArtwork>(artworkPath);
            if (artwork == null) { artwork = ScriptableObject.CreateInstance<ArsenalArtwork>(); AssetDatabase.CreateAsset(artwork, artworkPath); }
            var entries = new List<CombatVfxEntry>();
            foreach (var pair in pieces) entries.Add(new CombatVfxEntry { Name = pair.Key, Frames = new List<Sprite>(pair.Value.Values).ToArray() });
            artwork.Entries = entries.ToArray(); EditorUtility.SetDirty(artwork);

            bool Has(string piece)
            {
                int cut = piece.LastIndexOf("_f", StringComparison.Ordinal);
                string name = cut > 0 && int.TryParse(piece.Substring(cut + 2), out int frame) ? piece.Substring(0, cut) : piece;
                int index = cut > 0 && int.TryParse(piece.Substring(cut + 2), out int f) ? f : 0;
                return pieces.TryGetValue(name, out var frames) && frames.ContainsKey(index);
            }
            var weapons = new List<WeaponDefinition>(catalog.Weapons);
            foreach (ArsenalSpecs.Spec spec in ArsenalSpecs.All)
            {
                var missing = new List<string>();
                if (!Has(spec.Id + "_Held")) missing.Add(spec.Id + "_Held");
                foreach (string piece in spec.Pieces) if (!Has(piece)) missing.Add(piece);
                if (missing.Count > 0) { status.Add("WAITING " + spec.Id + ": " + string.Join(", ", missing)); continue; }
                string path = "Assets/ArcaneBrigade/Data/Run/" + spec.Id + ".asset";
                WeaponDefinition weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
                if (weapon == null) { weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(weapon, path); }
                weapon.Id = spec.Id; weapon.Arsenal = spec.Arsenal; weapon.Kind = spec.Kind; weapon.Motion = spec.Motion; weapon.Element = spec.Element;
                weapon.Damage = spec.Damage; weapon.Range = spec.Range; weapon.Cooldown = spec.Cooldown; weapon.Windup = spec.Windup; weapon.Recovery = 0.24f;
                weapon.Sprite = pieces[spec.Id + "_Held"][0];
                string shot = spec.Pieces.Length > 0 ? spec.Pieces[0] : null;
                if (shot != null) { int cut = shot.LastIndexOf("_f", StringComparison.Ordinal); string key = cut > 0 ? shot.Substring(0, cut) : shot; weapon.ProjectileSprite = pieces[key][new List<int>(pieces[key].Keys)[0]]; }
                weapon.ProjectileVisualWidth = 0.32f; weapon.ProjectileColor = Color.white; weapon.ProjectileSpeed = 8; weapon.ProjectileLifetime = 2.4f;
                weapon.VisualWidth = spec.Kind == WeaponKind.Melee ? 1.1f : 0.95f; weapon.MountDistance = 0.34f; weapon.MuzzleDistance = 0.6f;
                weapon.ConeDot = spec.Arsenal == ArsenalWeapon.DragonLanternGlaive ? 0.5f : 0.2f; weapon.SwingAngle = spec.Arsenal == ArsenalWeapon.StarforgeClaymore ? 120 : 70;
                weapon.BodyAttackTilt = spec.Kind == WeaponKind.Melee ? 14 : 8; weapon.PushDistance = 0.15f;
                weapon.Family = ExpandedWeapon.None; weapon.Mechanic = CarnivalWeapon.None; weapon.Signature = SignatureWeapon.None; weapon.SpecialOnly = false;
                weapon.GameplaySummary = spec.Summary;
                EditorUtility.SetDirty(weapon);
                if (!weapons.Contains(weapon)) weapons.Add(weapon);
                status.Add("LIVE " + spec.Id);
            }
            catalog.Weapons = weapons.ToArray(); EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("PreviewExports/Arsenal");
            File.WriteAllLines("PreviewExports/Arsenal/Art_Status.txt", status);
            ApplyHeroes();
        }

        [MenuItem("Arcane Brigade/Arsenal/Apply hero weapons")]
        public static void ApplyHeroes()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var log = new List<string>();
            foreach (UnitDefinition hero in catalog.Champions)
            {
                string id = HeroLoadout.Weapon(catalog, hero.Id);
                if (id == null) { log.Add("no loadout " + hero.Id); continue; }
                WeaponDefinition weapon = catalog.Weapon(id) ?? throw new InvalidOperationException("Hero weapon missing from catalog: " + hero.Id + " " + id);
                if (hero.Weapon == weapon) continue;
                log.Add(hero.Id + ": " + (hero.Weapon != null ? hero.Weapon.Id : "-") + " -> " + id);
                hero.Weapon = weapon; EditorUtility.SetDirty(hero);
            }
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("PreviewExports/Arsenal");
            File.WriteAllLines("PreviewExports/Arsenal/Hero_Weapons.txt", log);
            Debug.Log("ARSENAL_HEROES\n" + string.Join("\n", log));
        }
    }
}
