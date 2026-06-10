using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Requirement 23/10/2026: the delivered playtest art goes in at once - the shop's new exit door, twelve map enemies
    /// (three per map, small / medium / large, each a 4x4 sheet: normal form then mutated), the Lone and family relic icons and
    /// the two Wild Stitch friends.</summary>
    public static class PlaytestArtIntegrationBuilder
    {
        private const string EnemyArt = "Assets/ArcaneBrigade/Art/Playtest/MapEnemies/";
        private struct Spec { public string Id, Name, Template, MutationName, Summary; public float Height, Health, Speed; public int Unlock; public EnemyMutation Ability; }
        private static Spec S(string id, string name, string template, float height, float health, float speed, int unlock, EnemyMutation ability, string mutation, string summary)
            => new Spec { Id = id, Name = name, Template = template, Height = height, Health = health, Speed = speed, Unlock = unlock, Ability = ability, MutationName = mutation, Summary = summary };
        public static readonly string[] MapEnemyIds = { "TicketMoth", "UmbrellaStalker", "SofaGlutton", "StampTick", "SealHound", "SackGolem", "WickImp", "LampshadeJelly", "ChandelierCrawler", "ClipMite", "InkToad", "CabinetMimic" };

        [MenuItem("Arcane Brigade/Integrate playtest art")]
        public static void Build()
        {
            LoneRelicsBuilder.Build();
            FamilyRelicsBuilder.Build();
            WildStitchFriendsBuilder.Build();
            Door();
            Enemies();
            AssetDatabase.SaveAssets();
            Debug.Log("PLAYTEST_ART_INTEGRATED");
        }

        private static void Door()
        {
            const string path = EnemyArt + "Shop_ExitDoor.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) { Debug.Log("Exit door: no art"); return; }
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int w = texture.width / 2, h = texture.height;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple; importer.alphaIsTransparency = true; importer.spritePixelsPerUnit = 256;
#pragma warning disable 618
            importer.spritesheet = new[]
            {
                new SpriteMetaData { name = "ShopExitDoor_0", rect = new Rect(0, 0, w, h), alignment = (int)SpriteAlignment.BottomCenter, pivot = new Vector2(.5f, 0) },
                new SpriteMetaData { name = "ShopExitDoor_1", rect = new Rect(w, 0, w, h), alignment = (int)SpriteAlignment.BottomCenter, pivot = new Vector2(.5f, 0) },
            };
#pragma warning restore 618
            importer.SaveAndReimport();
            Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            var art = AssetDatabase.LoadAssetAtPath<PaintedAnimationLibrary>("Assets/ArcaneBrigade/Resources/PaintedAnimationLibrary.asset");
            if (frames.Length == 2 && art != null) { art.HallDoor = frames; EditorUtility.SetDirty(art); Debug.Log("Exit door: replaced"); }
        }

        private static void Enemies()
        {
            var specs = new[]
            {
                // the Waiting Room
                S("TicketMoth", "Ticket Moth", "HushMite", .6f, 30, 1.25f, 2, EnemyMutation.MothMolting, "Torn Stubs", "Splits into paper stubs that flutter at you."),
                S("UmbrellaStalker", "Umbrella Stalker", "ChargerRam", 1.15f, 70, 1.1f, 3, EnemyMutation.CombRake, "Canopy Rake", "Opens its canopies and rakes the ground ahead."),
                S("SofaGlutton", "Sofa Glutton", "WardrobeCrab", 1.8f, 220, .95f, 4, EnemyMutation.CabinetScissor, "Spring Jaw", "Its cushions snap shut like scissors."),
                // the Post Office
                S("StampTick", "Stamp Tick", "HushMite", .55f, 34, 1.3f, 6, EnemyMutation.SootPounce, "Postmark Pounce", "Pounces where you stood."),
                S("SealHound", "Seal Hound", "ChargerRam", 1.0f, 85, 1.2f, 7, EnemyMutation.RamBackstep, "Seal Snap", "Steps back, then snaps forward."),
                S("SackGolem", "Sack Golem", "FurnaceToad", 1.75f, 240, .9f, 8, EnemyMutation.KegRebound, "Parcel Burst", "Bounces back from a hit and bursts at you."),
                // the Lamp Hall
                S("WickImp", "Wick Imp", "BristleImp", .6f, 38, 1.3f, 11, EnemyMutation.BristleRush, "Triple Wick", "Lowers its flames and rushes."),
                S("LampshadeJelly", "Lampshade Jelly", "CurtainWisp", .95f, 80, 1.0f, 12, EnemyMutation.CurtainCollapse, "Tassel Chains", "Drops its shade and lashes with tassels."),
                S("ChandelierCrawler", "Chandelier Crawler", "WardrobeCrab", 1.85f, 260, .9f, 13, EnemyMutation.BellEruption, "Crystal Crown", "Shatters a ring of crystal around itself."),
                // the Archive Vault
                S("ClipMite", "Clip Mite", "HushMite", .55f, 36, 1.3f, 16, EnemyMutation.RuneSpiral, "Pin Spiral", "Unwinds a slow spiral of pins."),
                S("InkToad", "Ink Toad", "BlastPuffer", 1.0f, 90, 1.0f, 17, EnemyMutation.PufferEcho, "Ink Spill", "Spills ink that bursts a moment later."),
                S("CabinetMimic", "Cabinet Mimic", "WardrobeCrab", 1.9f, 280, .9f, 18, EnemyMutation.CabinetScissor, "Drawer Jaws", "Every drawer opens into a biting mouth."),
            };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var library = AssetDatabase.LoadAssetAtPath<EnemyEvolutionLibrary>("Assets/ArcaneBrigade/Resources/EnemyEvolutionLibrary.asset");
            var species = new List<EnemyEvolutionEntry>((library.Species ?? new EnemyEvolutionEntry[0]).Where(e => e != null && !MapEnemyIds.Contains(e.Id)));
            var enemies = new List<UnitDefinition>(catalog.Enemies);
            int added = 0;
            foreach (Spec s in specs)
            {
                string sheet = EnemyArt + "Enemy_" + s.Id + ".png";
                if (AssetImporter.GetAtPath(sheet) == null) { Debug.Log("Map enemy " + s.Id + ": no art"); continue; }
                Sprite[] frames = K20BossAnatomyBuilder.ImportAtlas(sheet, s.Id + "Evolution", true);
                AuthoredMotionSet normal = Motion(s.Id, frames.Take(8).ToArray(), false), mutated = Motion(s.Id, frames.Skip(8).ToArray(), true);
                UnitDefinition template = catalog.Enemies.FirstOrDefault(e => e != null && e.Id == s.Template);
                if (template == null) { Debug.Log("Map enemy " + s.Id + ": template " + s.Template + " missing"); continue; }
                string path = "Assets/ArcaneBrigade/Data/Run/" + s.Id + ".asset";
                UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
                if (unit == null) { unit = ScriptableObject.CreateInstance<UnitDefinition>(); AssetDatabase.CreateAsset(unit, path); }
                EditorUtility.CopySerialized(template, unit);
                unit.name = s.Id; unit.Id = s.Id; unit.DisplayName = s.Name; unit.Description = s.Summary;
                unit.BodyHeight = s.Height; unit.Radius = template.Radius * s.Height / Mathf.Max(.1f, template.BodyHeight); unit.Health = s.Health; unit.Speed = s.Speed; unit.UnlockWave = s.Unlock;
                unit.Body = normal.Idle[0]; unit.AuthoredMotion = normal; unit.AnimationFrames = new[] { normal.Idle[0], normal.Idle[1], normal.Walk[0], normal.Walk[1] };
                unit.EvolutionStage = 0; unit.Mutation = EnemyMutation.None; unit.Tint = Color.white;
                EditorUtility.SetDirty(unit);
                if (!enemies.Contains(unit)) enemies.Add(unit);
                species.Add(new EnemyEvolutionEntry { Id = s.Id, MutationName = s.MutationName, Summary = s.Summary, Ability = s.Ability, Normal = normal, Mutated = mutated });
                added++;
            }
            catalog.Enemies = enemies.ToArray(); library.Species = species.ToArray();
            EditorUtility.SetDirty(catalog); EditorUtility.SetDirty(library); EnemyEvolutionLibrary.Invalidate();
            Debug.Log("Map enemies: " + added + " added");
        }
        private static AuthoredMotionSet Motion(string id, Sprite[] frames, bool mutated)
        {
            string path = "Assets/ArcaneBrigade/Resources/" + id + (mutated ? "MutatedMotion" : "Motion") + ".asset";
            AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(path);
            if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, path); }
            motion.Idle = frames.Take(2).ToArray(); motion.Walk = new[] { frames[2], frames[3], frames[2], frames[3] };
            motion.Windup = frames.Skip(4).Take(2).ToArray(); motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }
    }
}
