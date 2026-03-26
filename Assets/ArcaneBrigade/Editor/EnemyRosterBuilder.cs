using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Enemy roster v2 (Docs/Enemy_Roster_v2.md, prompts in Tools/Art/enemies_art.py). Imports whatever enemy art has
    /// arrived in Art/Expansion/Enemies and registers it in Resources/WaitingEnemyRoster.asset; nothing else changes.
    ///  - &lt;Id&gt;_v1.png (1536x1536, 4x4: rows 1-2 normal form, rows 3-4 evolved form, poses idle / blink / walk left / walk
    ///    right / crouched wind-up / charged wind-up / release / hurt) for the eight new enemies: the enemy's unit, weapon and
    ///    motion assets are written to Data/Run/WaitingEnemies and it joins its zone's waves. Missing art: it stays out.
    ///  - TollTick / CoatRack / UnclaimedTrunk / HatboxHopper / SplinterImp_v1.png: the older species (CoinLeech, SporeBishop,
    ///    SmokingCoffin, HopperKeg, BristleImp) take the new look and name in runs (same id, behaviour and stats).
    ///  - SewnLetterPieces_v1.png (4x2: row 1 the letter half, row 2 the stamp; idle / flap or hop / wind-up / hurt).
    ///  - IndebtedStamp_v1.png (512 emblem): the receipt-ink stamp drawn on Indebted elites.
    /// Idempotent: run it again whenever a picture lands.</summary>
    public static class EnemyRosterBuilder
    {
        public const string ArtRoot = "Assets/ArcaneBrigade/Art/Expansion/Enemies/";
        public const string DataRoot = "Assets/ArcaneBrigade/Data/Run/WaitingEnemies";
        public const string RosterPath = "Assets/ArcaneBrigade/Resources/WaitingEnemyRoster.asset";
        public const string PiecesId = "SewnLetterPieces", StampId = "IndebtedStamp";

        public sealed class Spec
        {
            public string Id, Name, Copy;
            public WaitingEnemyKind Kind;
            public int Unlock;                         // the wave whose neighbours set its strength
            public float Health = 1, Speed = 1, Damage = 1; // shares of the neighbours' medians
            public float Height = 1.05f, Radius = .3f, Knockback = .2f;
            public BodyMaterial Material;
        }

        public static readonly Spec[] Specs =
        {
            new Spec { Id = "SpareChair", Name = "Spare Chair", Kind = WaitingEnemyKind.SpareChair, Unlock = 6, Height = 1.1f, Radius = .32f, Knockback = .25f, Material = BodyMaterial.Wood,
                Copy = "Unfolds a seat ring under you. Still inside when it snaps: pinned 0.6 s." },
            new Spec { Id = "WakingSitter", Name = "Waking Sitter", Kind = WaitingEnemyKind.WakingSitter, Unlock = 8, Health = .9f, Speed = .75f, Damage = .5f, Height = 1.35f, Material = BodyMaterial.Fabric,
                Copy = "Throws an ink thread: 30% slower until you get 5 paces away." },
            new Spec { Id = "InkNotary", Name = "Ink Notary", Kind = WaitingEnemyKind.InkNotary, Unlock = 16, Health = .85f, Speed = .9f, Damage = .8f, Height = 1.1f, Knockback = .15f, Material = BodyMaterial.Paper,
                Copy = "Stamps PAID on an ally (its next hit taken is halved) and CLOSED ink that slows you 25%." },
            new Spec { Id = "SewnLetter", Name = "Sewn Letter", Kind = WaitingEnemyKind.SewnLetter, Unlock = 6, Speed = .95f, Radius = .32f, Knockback = .1f, Material = BodyMaterial.Paper,
                Copy = "Bursts into two halves; each half into two stamps." },
            new Spec { Id = "PartyLine", Name = "Party Line", Kind = WaitingEnemyKind.PartyLine, Unlock = 8, Health = 1.25f, Speed = .4f, Knockback = .9f, Material = BodyMaterial.Metal,
                Copy = "Never moves. Rings, then pins a live cord across the floor for 3 s." },
            new Spec { Id = "DoorKnocker", Name = "Door Knocker", Kind = WaitingEnemyKind.DoorKnocker, Unlock = 11, Health = 1.2f, Speed = .9f, Height = 1.15f, Radius = .34f, Knockback = .5f, Material = BodyMaterial.Metal,
                Copy = "Lands beside you and knocks three times; each knock sends a wave, the third the widest." },
            new Spec { Id = "LampSnuffer", Name = "Lamp Snuffer", Kind = WaitingEnemyKind.LampSnuffer, Unlock = 9, Health = .8f, Speed = .85f, Damage = .8f, Height = 1f, Radius = .28f, Knockback = .1f, Material = BodyMaterial.Fabric,
                Copy = "Drifts to the nearest lantern and dims it until defeated." },
            new Spec { Id = "LedgerHound", Name = "Ledger Hound", Kind = WaitingEnemyKind.LedgerHound, Unlock = 16, Speed = 1.25f, Height = 1f, Material = BodyMaterial.Paper,
                Copy = "Circles you laying four tally marks; the fifth is a slash along them." },
        };

        public static string ArtPath(string id) => ArtRoot + id + "_v1.png";
        public static bool HasArt(string id) => File.Exists(ArtPath(id));

        [MenuItem("Arcane Brigade/Enemy roster v2/Import enemy art")]
        public static void Apply()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            if (catalog == null) throw new InvalidOperationException("RunCatalog missing");
            WaitingEnemyRoster roster = AssetDatabase.LoadAssetAtPath<WaitingEnemyRoster>(RosterPath);
            if (roster == null) { roster = ScriptableObject.CreateInstance<WaitingEnemyRoster>(); AssetDatabase.CreateAsset(roster, RosterPath); }
            var enemies = new List<UnitDefinition>(); var art = new List<WaitingEnemyArt>(); var waiting = new List<string>(); var rethemed = new List<string>();
            foreach (Spec spec in Specs)
            {
                if (!HasArt(spec.Id)) { waiting.Add(spec.Id); continue; }
                Sprite[] frames = ImportSpecies(ArtPath(spec.Id), "Enemy" + spec.Id);
                EnsureFolder();
                AuthoredMotionSet normal = Motion(spec.Id + "Motion", frames.Take(8).ToArray()), evolved = Motion(spec.Id + "EvolvedMotion", frames.Skip(8).Take(8).ToArray());
                UnitDefinition unit = LoadOrCreate<UnitDefinition>(DataRoot + "/" + spec.Id + ".asset");
                WeaponDefinition weapon = LoadOrCreate<WeaponDefinition>(DataRoot + "/" + spec.Id + "Attack.asset");
                Define(unit, weapon, spec, catalog, normal);
                EditorUtility.SetDirty(unit); EditorUtility.SetDirty(weapon);
                enemies.Add(unit);
                art.Add(new WaitingEnemyArt { Id = spec.Id, ArtId = spec.Id, DisplayName = "", Normal = normal, Evolved = evolved });
            }
            for (int index = 0; index < WaitingEnemyRoster.RethemedIds.Length; index++)
            {
                string id = WaitingEnemyRoster.RethemeArt[index], old = WaitingEnemyRoster.RethemedIds[index];
                if (!HasArt(id)) { waiting.Add(id); continue; }
                if (!catalog.Enemies.Any(enemy => enemy != null && enemy.Id == old)) { Debug.LogWarning("ENEMY_ROSTER re-theme target missing " + old); continue; }
                Sprite[] frames = ImportSpecies(ArtPath(id), "Enemy" + id);
                EnsureFolder();
                art.Add(new WaitingEnemyArt { Id = old, ArtId = id, DisplayName = WaitingEnemyRoster.RethemeNames[index], Normal = Motion(id + "Motion", frames.Take(8).ToArray()), Evolved = Motion(id + "EvolvedMotion", frames.Skip(8).Take(8).ToArray()) });
                rethemed.Add(old + "->" + id);
            }
            roster.LetterHalf = roster.LetterStamp = null;
            if (HasArt(PiecesId))
            {
                Sprite[] pieces = K25AtlasImporter.Import(ArtPath(PiecesId), "Enemy" + PiecesId, 4, 2, Enumerable.Range(0, 8).ToArray());
                if (pieces.Length != 8) throw new InvalidOperationException("SewnLetterPieces needs 8 sprites, found " + pieces.Length);
                EnsureFolder();
                roster.LetterHalf = PieceMotion("SewnLetterHalfMotion", pieces.Take(4).ToArray());
                roster.LetterStamp = PieceMotion("SewnLetterStampMotion", pieces.Skip(4).Take(4).ToArray());
            }
            else waiting.Add(PiecesId);
            roster.IndebtedStamp = null;
            if (HasArt(StampId)) roster.IndebtedStamp = ImportSingle(ArtPath(StampId));
            else waiting.Add(StampId);
            roster.Enemies = enemies.ToArray(); roster.Art = art.ToArray();
            EditorUtility.SetDirty(roster); AssetDatabase.SaveAssets(); WaitingEnemyRoster.Invalidate();
            Debug.Log("ENEMY_ROSTER_BUILT active=" + enemies.Count + " [" + string.Join(",", enemies.Select(unit => unit.Id)) + "] rethemed=" + rethemed.Count + " [" + string.Join(",", rethemed) + "] pieces=" + (roster.LetterHalf != null) + " indebtedStamp=" + (roster.IndebtedStamp != null));
            Debug.Log("ENEMY_ROSTER_WAITING_FOR_ART " + waiting.Count + " [" + string.Join(", ", waiting.Select(id => id + "_v1.png")) + "]");
        }

        /// <summary>Fills a new enemy's unit and weapon from its spec. Strength comes from the data: the median health, speed,
        /// contact and weapon damage of the six existing enemies whose unlock wave is nearest, times the spec's shares (the
        /// run then scales it by wave like every other enemy).</summary>
        public static void Define(UnitDefinition unit, WeaponDefinition weapon, Spec spec, RunCatalog catalog, AuthoredMotionSet normal)
        {
            UnitDefinition[] neighbours = catalog.Enemies.Where(enemy => enemy != null && !enemy.IsBoss && enemy.WaitingKind == WaitingEnemyKind.None && enemy.Weapon != null)
                .OrderBy(enemy => Mathf.Abs(enemy.UnlockWave - spec.Unlock)).ThenBy(enemy => enemy.Id, StringComparer.Ordinal).Take(6).ToArray();
            if (neighbours.Length == 0) throw new InvalidOperationException("No enemy to measure " + spec.Id + " against");
            float health = Median(neighbours.Select(enemy => enemy.Health)), speed = Median(neighbours.Select(enemy => enemy.Speed));
            float contact = Median(neighbours.Select(enemy => enemy.ContactDamage)), damage = Median(neighbours.Select(enemy => enemy.Weapon.Damage));
            weapon.name = spec.Id + "Attack"; weapon.Id = spec.Id + "Attack"; weapon.Kind = WeaponKind.Contact; weapon.Damage = Round(damage * spec.Damage);
            weapon.Range = 1; weapon.Windup = 1; weapon.Cooldown = 2; weapon.Recovery = .6f; weapon.PushDistance = 0;
            weapon.Sprite = null; weapon.ProjectileSprite = null; weapon.Element = DamageElement.Physical;
            unit.name = spec.Id; unit.Id = spec.Id; unit.DisplayName = spec.Name; unit.Description = spec.Copy;
            unit.WaitingKind = spec.Kind; unit.SpecialEnemy = SpecialEnemyKind.None; unit.Behavior = EnemyBehavior.Standard; unit.Shot = HostileShot.Straight;
            unit.Mutation = EnemyMutation.None; unit.EvolutionStage = 0; unit.IsBoss = false; unit.Weapon = weapon;
            unit.AuthoredMotion = normal; unit.Body = normal.Idle[0];
            unit.AnimationFrames = new[] { normal.Idle[0], normal.Idle[1], normal.Walk[0], normal.Walk[1] };
            unit.Health = Round(health * spec.Health); unit.Speed = Round(speed * spec.Speed);
            unit.ContactDamage = Round(contact * (spec.Kind == WaitingEnemyKind.WakingSitter ? .5f : 1)); unit.ContactInterval = 1;
            unit.BodyHeight = spec.Height; unit.BodyScale = 1; unit.Radius = spec.Radius; unit.KnockbackResistance = spec.Knockback;
            unit.UnlockWave = spec.Unlock; unit.Tint = Color.white; unit.Material = spec.Material; unit.Tags = new string[0];
        }

        private static float Round(float value) => Mathf.Round(value * 100) / 100;
        private static float Median(IEnumerable<float> values)
        {
            float[] sorted = values.OrderBy(value => value).ToArray();
            return sorted.Length % 2 == 1 ? sorted[sorted.Length / 2] : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) * .5f;
        }

        private static Sprite[] ImportSpecies(string path, string id)
        {
            // separate painted figures: slice by their own outlines (as K22/K25); a sheet whose figures touch falls back to the grid
            try
            {
                Sprite[] frames = K20BossAnatomyBuilder.ImportAtlas(path, id, true);
                if (frames.Length == 16) return frames;
            }
            catch (Exception exception) { Debug.LogWarning("ENEMY_ROSTER outline slicing failed for " + id + ", using the 4x4 grid: " + exception.Message); }
            Sprite[] grid = K25AtlasImporter.Import(path, id, 4, 4, Enumerable.Range(0, 16).ToArray());
            if (grid.Length != 16) throw new InvalidOperationException(id + " needs 16 poses, found " + grid.Length);
            return grid;
        }

        private static Sprite ImportSingle(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || importer.mipmapEnabled || !importer.alphaIsTransparency)
            {
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 1024;
                importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static AuthoredMotionSet Motion(string name, Sprite[] frames)
        {
            if (frames.Length != 8) throw new InvalidOperationException(name + " needs 8 poses");
            AuthoredMotionSet motion = LoadOrCreate<AuthoredMotionSet>(DataRoot + "/" + name + ".asset");
            motion.Idle = new[] { frames[0], frames[1] }; motion.Walk = new[] { frames[2], frames[3], frames[2], frames[3] };
            motion.Windup = new[] { frames[4], frames[5] }; motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }

        /// <summary>idle / flap (or hop) / wind-up / hurt</summary>
        private static AuthoredMotionSet PieceMotion(string name, Sprite[] frames)
        {
            AuthoredMotionSet motion = LoadOrCreate<AuthoredMotionSet>(DataRoot + "/" + name + ".asset");
            motion.Idle = new[] { frames[0], frames[0] }; motion.Walk = new[] { frames[1], frames[0], frames[1], frames[0] };
            motion.Windup = new[] { frames[2], frames[2] }; motion.Release = new[] { frames[2], frames[2] }; motion.Recovery = new[] { frames[3], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(DataRoot)) AssetDatabase.CreateFolder("Assets/ArcaneBrigade/Data/Run", "WaitingEnemies");
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
    }
}
