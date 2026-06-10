using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Requirement 23/10/2026: Wild Stitch (the "hide" bond Porcelain Doll's innate counts) had only two friends in the game,
    /// one of them a prismatic gift. Two more join it once their paintings exist: each sheet at Art/Playtest/Companions/&lt;Id&gt;.png
    /// is a 2x2 grid (neutral, breathing wind-up, walking step, attack) and is sliced here. Without a sheet the friend is not
    /// added (no borrowed body).</summary>
    public static class WildStitchFriendsBuilder
    {
        private const string ArtFolder = "Assets/ArcaneBrigade/Art/Playtest/Companions/";

        [MenuItem("Arcane Brigade/Build Wild Stitch Friends")]
        public static void Build()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            UnitDefinition template = catalog.Companions.First(u => u.Id == "RibbonSpider");
            var specs = new (string id, string name, string text, string[] tags, ItemRarity rarity, int price, WeaponKind kind, PassiveKind passive, float strength, StatModifier[] mods)[]
            {
                ("StitchedPangolin", "Stitched Pangolin", "A pangolin of patched leather scales. Every 4s heals the hero; hero regeneration +0.3.",
                    new[] { "frost", "hide" }, ItemRarity.Uncommon, 30, WeaponKind.Melee, PassiveKind.HealChampion, .6f,
                    new[] { new StatModifier { Effect = ItemEffect.Heal, Scope = ModifierScope.Champion, Value = .3f } }),
                ("SaddleMoth", "Saddle Moth", "A moth wearing a little leather saddle. Hero contact damage +3; +2% damage per friend.",
                    new[] { "oddity", "hide" }, ItemRarity.Rare, 45, WeaponKind.Projectile, PassiveKind.Rally, 2f,
                    new[] { new StatModifier { Effect = ItemEffect.CollisionDamage, Scope = ModifierScope.Champion, Value = 3 } }),
            };
            int added = 0;
            foreach (var s in specs)
            {
                string sheet = ArtFolder + s.id + ".png";
                Sprite[] frames = Slice(sheet, s.id);
                if (frames == null) { Debug.Log("Wild Stitch friends: no sheet for " + s.id + " yet; skipped."); continue; }
                string path = "Assets/ArcaneBrigade/Data/Run/" + s.id + ".asset";
                UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
                if (unit == null) { unit = ScriptableObject.CreateInstance<UnitDefinition>(); AssetDatabase.CreateAsset(unit, path); }
                EditorUtility.CopySerialized(template, unit);
                unit.name = s.id; unit.Id = s.id; unit.DisplayName = s.name; unit.Description = s.text; unit.Tags = s.tags; unit.Rarity = s.rarity; unit.Price = s.price;
                unit.ReferenceCompanion = ""; unit.Passive = s.passive; unit.PassiveStrength = s.strength; unit.CompanionModifiers = s.mods;
                unit.Body = frames[0]; unit.AnimationFrames = frames; unit.AuthoredMotion = Motion(s.id + "Motion", frames);
                unit.Weapon = catalog.Weapons.First(w => w.Kind == s.kind && !w.SpecialOnly);
                unit.GiftOnly = false; unit.SpecialRecruit = false; unit.ExclusiveHero = ""; unit.UnlockWave = 1;
                EditorUtility.SetDirty(unit);
                if (!catalog.Companions.Contains(unit)) catalog.Companions = catalog.Companions.Concat(new[] { unit }).ToArray();
                added++;
            }
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Wild Stitch friends: " + added + " in the catalog.");
        }

        /// <summary>Cuts a 2x2 sheet into its four poses (reading order); null when the sheet is not there yet.</summary>
        private static Sprite[] Slice(string path, string id)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return null;
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int w = texture.width / 2, h = texture.height / 2;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple; importer.alphaIsTransparency = true;
#pragma warning disable 618
            importer.spritesheet = Enumerable.Range(0, 4).Select(i => new SpriteMetaData
            {
                name = id + "_" + i, rect = new Rect((i % 2) * w, (1 - i / 2) * h, w, h), alignment = (int)SpriteAlignment.BottomCenter, pivot = new Vector2(.5f, 0)
            }).ToArray();
#pragma warning restore 618
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray() is Sprite[] all && all.Length == 4 ? all : null;
        }
        private static AuthoredMotionSet Motion(string id, Sprite[] frames)
        {
            string path = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
            AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(path);
            if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, path); }
            motion.Idle = new[] { frames[0], frames[1] }; motion.Walk = new[] { frames[0], frames[2], frames[0], frames[2] };
            motion.Windup = new[] { frames[1], frames[2] }; motion.Release = new[] { frames[3], frames[3] }; motion.Recovery = new[] { frames[2], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }
    }
}
