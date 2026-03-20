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
            // pieces the new friends place (content expansion): Tea Dragon's teapot, The First Seed's hedge
            foreach (string file in new[] { "Assets/ArcaneBrigade/Art/Expansion/TeaDragon_Teapot_v1.png", "Assets/ArcaneBrigade/Art/Expansion/FirstSeed_Hedge_v1.png" })
            {
                if (!File.Exists(file)) continue;
                AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceSynchronousImport);
                if (AssetImporter.GetAtPath(file) is TextureImporter importer && importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.SaveAndReimport(); }
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file);
                if (sprite != null) pieces[Path.GetFileNameWithoutExtension(file).Replace("_v1", "")] = new SortedDictionary<int, Sprite> { [0] = sprite };
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
                bool created = weapon == null;
                if (weapon == null) { weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(weapon, path); }
                weapon.Id = spec.Id; weapon.Arsenal = spec.Arsenal; weapon.Kind = spec.Kind; weapon.Motion = spec.Motion; weapon.Element = spec.Element;
                weapon.Damage = spec.Damage; weapon.Range = spec.Range; weapon.Cooldown = spec.Cooldown; weapon.Windup = spec.Windup; weapon.Recovery = 0.24f;
                weapon.Sprite = pieces[spec.Id + "_Held"][0];
                string shot = spec.Pieces.Length > 0 ? spec.Pieces[0] : null;
                if (shot != null) { int cut = shot.LastIndexOf("_f", StringComparison.Ordinal); string key = cut > 0 ? shot.Substring(0, cut) : shot; weapon.ProjectileSprite = pieces[key][new List<int>(pieces[key].Keys)[0]]; }
                else weapon.ProjectileSprite = weapon.Sprite; // a thrown copy of the weapon itself (brick, ingot, bottle, twin-moon staff) flies as its own painting
                weapon.ProjectileVisualWidth = 0.32f; weapon.ProjectileColor = Color.white; weapon.ProjectileSpeed = 8; weapon.ProjectileLifetime = 2.4f;
                // fist-sized arms (paw, wraps, knuckle) are drawn smaller; mount and muzzle start at the usual distances and are
                // then calibrated per weapon, so only a new weapon gets them here
                weapon.VisualWidth = (spec.Kind == WeaponKind.Melee ? 1.1f : 0.95f) * spec.Width;
                if (created) { weapon.MountDistance = 0.34f; weapon.MuzzleDistance = 0.5f; }
                weapon.ConeDot = spec.Arsenal == ArsenalWeapon.DragonLanternGlaive ? 0.5f : 0.2f; weapon.SwingAngle = spec.Arsenal == ArsenalWeapon.StarforgeClaymore ? 120 : 70;
                weapon.BodyAttackTilt = spec.Kind == WeaponKind.Melee ? 14 : 8; weapon.PushDistance = 0.15f;
                weapon.Family = ExpandedWeapon.None; weapon.Mechanic = CarnivalWeapon.None; weapon.Signature = SignatureWeapon.None; weapon.SpecialOnly = false;
                weapon.GameplaySummary = spec.Summary;
                EditorUtility.SetDirty(weapon);
                if (!weapons.Contains(weapon)) weapons.Add(weapon);
                status.Add("LIVE " + spec.Id);
            }
            catalog.Weapons = weapons.ToArray(); EditorUtility.SetDirty(catalog);
            // hand-painted rarity frames replace the procedural ones, a rarity at a time, once all five of its parts exist
            foreach (string rarity in new[] { "Uncommon", "Rare", "Mythic", "Crimson", "Rainbow" })
            {
                string[] parts = { "Side", "Glow", "Crest", "Foot", "Corner" };
                const string painted = "Assets/ArcaneBrigade/Art/RarityFramePainted/", live = "Assets/ArcaneBrigade/Resources/RarityFrame/";
                if (!Array.TrueForAll(parts, part => File.Exists(painted + rarity + "_" + part + ".png"))) { status.Add("FRAME WAITING " + rarity); continue; }
                foreach (string part in parts)
                {
                    File.Copy(painted + rarity + "_" + part + ".png", live + rarity + "_" + part + ".png", true);
                    AssetDatabase.ImportAsset(live + rarity + "_" + part + ".png", ImportAssetOptions.ForceUpdate);
                }
                status.Add("FRAME PAINTED " + rarity);
            }
            MeasureInk(pieces);
            MeasureGaugeWindows();
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("PreviewExports/Arsenal");
            File.WriteAllLines("PreviewExports/Arsenal/Art_Status.txt", status);
            ApplyHeroes();
        }

        /// <summary>For every painted hero gauge pair (Gauge_&lt;Hero&gt;_Empty / _Full), measures the rows where the two paintings
        /// differ (the window the fill rises in) as fractions of the empty vessel's painted height, into HeroGaugeArtwork.</summary>
        private static void MeasureGaugeWindows()
        {
            const string path = "Assets/ArcaneBrigade/Resources/HeroGaugeArtwork.asset";
            var asset = AssetDatabase.LoadAssetAtPath<HeroGaugeArtwork>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<HeroGaugeArtwork>(); AssetDatabase.CreateAsset(asset, path); }
            var windows = new List<HeroGaugeArtwork.Window>();
            foreach (string empty in Directory.GetFiles("Assets/ArcaneBrigade/Art/Arsenal", "Gauge*_Empty_v1.png"))
            {
                // Gauge_<Hero> is keyed by the hero; the simplified Gauge2_<Hero> (content expansion) keeps its prefix
                string name = Path.GetFileName(empty).Replace("_Empty_v1.png", "");
                string hero = name.StartsWith("Gauge2_") ? name : name.Substring("Gauge_".Length);
                string full = empty.Replace("_Empty_v1.png", "_Full_v1.png");
                if (!File.Exists(full)) continue;
                var a = new Texture2D(2, 2, TextureFormat.RGBA32, false); var b = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!a.LoadImage(File.ReadAllBytes(empty)) || !b.LoadImage(File.ReadAllBytes(full)) || a.width != b.width || a.height != b.height) continue;
                    Color32[] pa = a.GetPixels32(), pb = b.GetPixels32(); int w = a.width, h = a.height;
                    int inkLow = h, inkHigh = -1, low = h, high = -1;
                    for (int y = 0; y < h; y++)
                    {
                        int differing = 0; bool ink = false;
                        for (int x = 0; x < w; x += 2)
                        {
                            Color32 ca = pa[y * w + x], cb = pb[y * w + x];
                            if (ca.a > 16) ink = true;
                            if (Mathf.Abs(ca.r - cb.r) + Mathf.Abs(ca.g - cb.g) + Mathf.Abs(ca.b - cb.b) > 120) differing++;
                        }
                        if (ink) { inkLow = Mathf.Min(inkLow, y); inkHigh = Mathf.Max(inkHigh, y); }
                        if (differing > 6) { low = Mathf.Min(low, y); high = Mathf.Max(high, y); }
                    }
                    if (inkHigh <= inkLow || high <= low) continue;
                    float span = inkHigh - inkLow;
                    windows.Add(new HeroGaugeArtwork.Window { Hero = hero, Low = (low - inkLow) / span, High = (high - inkLow) / span });
                }
                finally { UnityEngine.Object.DestroyImmediate(a); UnityEngine.Object.DestroyImmediate(b); }
            }
            asset.Windows = windows.ToArray();
            var log = new List<string>();
            asset.Orders = GaugeOrderBaker.Bake(log).ToArray(); // shaped gauges: each hero's own fill order
            File.AppendAllLines("PreviewExports/Arsenal/Art_Status.txt", log);
            EditorUtility.SetDirty(asset);
        }

        /// <summary>Records each Arsenal painting's ink box (its painted pixels, not the transparent canvas) in SpriteInkMetrics,
        /// merged with the entries already there, so shots, coins and pieces are sized by what is actually drawn.</summary>
        private static void MeasureInk(SortedDictionary<string, SortedDictionary<int, Sprite>> pieces)
        {
            const string path = "Assets/ArcaneBrigade/Resources/SpriteInkMetrics.asset";
            var metrics = AssetDatabase.LoadAssetAtPath<SpriteInkMetrics>(path); if (metrics == null) return;
            var entries = new Dictionary<Sprite, Rect>();
            foreach (SpriteInkMetrics.Entry entry in metrics.Entries ?? new SpriteInkMetrics.Entry[0]) if (entry.Sprite != null) entries[entry.Sprite] = entry.Ink;
            foreach (var frames in pieces.Values)
                foreach (Sprite sprite in frames.Values)
                {
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    try
                    {
                        if (!texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)))) continue;
                        Color32[] pixels = texture.GetPixels32();
                        int left = texture.width, right = 0, bottom = texture.height, top = 0;
                        for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                                if (pixels[y * texture.width + x].a >= 40) { left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1); }
                        if (right <= left || top <= bottom) continue;
                        float rx = sprite.rect.width / texture.width, ry = sprite.rect.height / texture.height, ppu = sprite.pixelsPerUnit;
                        entries[sprite] = new Rect((left * rx - sprite.pivot.x) / ppu, (bottom * ry - sprite.pivot.y) / ppu, (right - left) * rx / ppu, (top - bottom) * ry / ppu);
                    }
                    finally { UnityEngine.Object.DestroyImmediate(texture); }
                }
            var list = new List<SpriteInkMetrics.Entry>();
            foreach (var pair in entries) list.Add(new SpriteInkMetrics.Entry { Sprite = pair.Key, Ink = pair.Value });
            metrics.Entries = list.ToArray(); EditorUtility.SetDirty(metrics); SpriteInkMetrics.Invalidate();
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
