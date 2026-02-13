using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ArtMasterChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool pass, string label)
        {
            notes.Add((pass ? "PASS " : "FAIL ") + label);
            Directory.CreateDirectory("PreviewExports/ArtMaster"); File.WriteAllLines("PreviewExports/ArtMaster/Checks.txt", notes);
            if (!pass) throw new InvalidOperationException(label);
        }
        public static void Run() => RunArtworkChecks(true);
        private static void RunArtworkChecks(bool requireCustomerHD)
        {
            notes.Clear();
            var art = Resources.Load<ActOneArtwork>("ActOneArtwork"); var vfx = Resources.Load<CombatVfxArtwork>("CombatVfxArtwork");
            Check(art != null && vfx != null, "Both runtime artwork libraries are present");
            foreach (var entry in art.Entries)
                Check(entry.Frames.Length > 0 && entry.Frames.All(s => s != null && SpriteInkMetrics.BoundsOf(s).height > .01f), "Imported story image and nonempty ink: " + entry.Name);
            foreach (var entry in vfx.Entries)
                Check(entry.Frames.Length > 0 && entry.Frames.All(s => s != null && SpriteInkMetrics.BoundsOf(s).height > .01f), "Imported combat image and nonempty ink: " + entry.Name);
            foreach (string name in new[] { "ChairPorter", "DoorThatStayedShut", "TheBedside", "LastCustomer_Pleasant", "LastCustomer_BorrowedFace" })
            {
                var entry = ActOneArtwork.Find(name);
                Check(entry?.Frames.Length == 16 && entry.Frames.Distinct().Count() == 16 && entry.Motion != null, "Sixteen separate poses, including anticipation/release: " + name);
                if (requireCustomerHD || !name.StartsWith("LastCustomer_", StringComparison.Ordinal))
                    Check(entry.Frames.Select(s => s.texture).Distinct().Count() == 16 && entry.Frames.All(s => Mathf.Max(s.rect.width, s.rect.height) >= 1024), "Large boss uses sixteen full-resolution images, not small atlas cells: " + name);
                else
                    Check(entry.Frames.Select(s => s.texture).Distinct().Count() == 1, "Available Customer uses its complete sixteen-pose atlas while HD deliveries are pending: " + name);
                Check(entry.Motion.IllustratedHeight > 0 && entry.Motion.Windup[0] != entry.Motion.Release[0], "Stable scale and distinct attack poses: " + name);
            }
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (string name in new[] { "Hob", "Latch", "Postscript", "Tally" })
            {
                var companion = ActOneArtwork.Find(name);
                Check(companion?.Frames.Length == 4 && companion.Frames.Distinct().Count() == 4,
                    "Companion atlas imports four whole separate poses: " + name);
                Check(companion.Motion.Windup[0] != companion.Motion.Release[0],
                    "Companion anticipation and attack use different authored poses: " + name);
            }
            Sprite[] ordinary = ActOneCanon.StoryIds.Select(id => catalog.Unit(id).Body).ToArray();
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 171010); run.State.Wave = 17;
                var combat = run.PrepareCombat();
                Check(combat.StoryCompanions.Length == 4, "All four canon companions enter the late combat");
                foreach (var unit in combat.StoryCompanions)
                    Check(unit.Body == ActOneArtwork.Picture(unit.DisplayName) && unit.AuthoredMotion != null, "Actual story build selects its own art: " + unit.DisplayName);
                Check(ordinary.SequenceEqual(ActOneCanon.StoryIds.Select(id => catalog.Unit(id).Body)), "Building the story party preserves ordinary species art");
            }
            foreach (var pair in new[] { ("CrescentBow", "ArrowBolt"), ("RuneStaff", "RuneOrb"), ("BellFork", "BellChime"), ("AcornSling", "AcornPellet"), ("BubbleWand", "Bubble"), ("ButtonBow", "ButtonDart"), ("InkPistol", "InkBullet"), ("LanternStaff", "LanternEmber"), ("LeafCrossbow", "LeafBolt"), ("MailboxLauncher", "Letter"), ("MusicBox", "MusicNote"), ("ShellCannon", "ShellShot"), ("SnowGlobeOrb", "GlobeShard"), ("StarKazoo", "Star"), ("TeapotPopgun", "TeaDrop"), ("TinFish", "TinFish"), ("DartCompass", "CompassDart"), ("ScrapMagnet", "MagnetBolt"), ("DreamProjector", "FilmFrame") })
            {
                var weapon = catalog.Weapon(pair.Item1);
                Check(weapon != null, "Real catalog weapon: " + pair.Item1);
                var image = CombatVfxArtwork.Projectile(new ProjectileState { Faction = Faction.Companion, Weapon = weapon }, null, .2f);
                Check(image != null && CombatVfxArtwork.Frames(pair.Item2).Contains(image), "Weapon selects its painted projectile: " + pair.Item1);
            }
            Check(CombatVfxArtwork.Projectile(new ProjectileState { Faction = Faction.Enemy }, null, 0) == CombatVfxArtwork.Frame("HostileBolt", 0), "Unclassified hostile shots use enemy art");
            Check(CombatVfxArtwork.Projectile(new ProjectileState { Faction = Faction.Champion, Cause = new CombatCause(1, 1, "hero-identity-spark") }, null, 0) == null, "Dedicated hero mechanic tokens keep ownership of their projectile");
            var parent = new GameObject("Art master field test");
            try
            {
                var painting = new CombatFieldPainting(parent.transform, 0);
                var field = new ExpandedField { Kind = ExpandedFieldKind.OfficeHazard, Radius = 2, Position = new Vector2(3, -2), At = 1, Until = 3 };
                Check(painting.Display(field, .5f), "Office hazard has a visible warning before damage");
                var view = parent.GetComponentInChildren<SpriteRenderer>();
                Check(parent.GetComponentsInChildren<SpriteRenderer>().Any(v => v.enabled && v.sprite == CombatVfxArtwork.Frame("TelegraphCircle", 0)), "Warning uses the danger telegraph, not a friendly field");
                Check(painting.Display(field, 1.5f) && view.sprite == CombatVfxArtwork.Frames("OfficeHazard").Last(), "Office hazard switches to its impact painting");
                Rect ink = SpriteInkMetrics.BoundsOf(view.sprite);
                Vector3 centre = view.transform.TransformPoint(ink.center), left = view.transform.TransformPoint(new Vector2(ink.xMin, ink.center.y));
                Check(Vector2.Distance(centre, field.Position) < .001f && Mathf.Abs(Vector2.Distance(left, centre) - field.Radius) < .001f, "Illustrated field centre and edge match the real damage radius despite PNG padding");
                Check(!painting.Display(field, 4) && !view.enabled, "Expired fields leave no invisible live painting");
            }
            finally { UnityEngine.Object.DestroyImmediate(parent); }
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 171011);
                var sim = new CombatSimulation(run.PrepareCombat()); sim.Start(171011);
                var enemy = sim.AddFixture(catalog.Enemies[0], Faction.Enemy, new Vector2(3, 0));
                var strike = new ExpandedField { Kind = ExpandedFieldKind.Strike, Source = enemy, Start = enemy.Position, Position = enemy.Position, Direction = Vector2.right, Radius = 2, At = 1, Until = 3 };
                ((IList<ExpandedField>)sim.ExpandedFields).Add(strike);
                var go = new GameObject("Enemy strike readability test");
                try
                {
                    var visual = go.AddComponent<ExpandedVisual>(); visual.Display(sim);
                    var line = go.transform.Find("ExpandedFieldCue").GetComponent<LineRenderer>();
                    Check(line.enabled && line.positionCount > 2 && line.startColor.r > line.startColor.g, "Enemy strike keeps a visible warning outside the carnival hazard family");
                    strike.At = -1; visual.Display(sim);
                    Check(line.enabled && line.startColor.r > line.startColor.g * 2, "Active enemy strike remains visible and ember red");
                }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
            foreach (string token in new[] { "Knot", "Puff", "Fang", "Thorn", "Patch", "Dumpling", "Seal", "Moon", "Star", "Shard", "Sand" })
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeroMechanicArtImporter.Folder + token + "_v1.png");
                Check(sprite != null && MechanicEffectArtwork.Current.PaintedToken((int)Enum.Parse<InkToken>(token)) == sprite, "Actual mechanic token selects imported art: " + token);
            }
            Check(MechanicEffectArtwork.Current.Form(BuildMorphKind.Porcelain) == ActOneArtwork.Picture("Form_TeaSpill"), "The tea field uses its amber spill rather than broken crockery");
            Check(ActOneCanon.ThursdayShoes.Icon == ActOneArtwork.Picture("Item_ThursdayShoes"), "Hidden reward uses the small Thursday shoes illustration");
            Check(PacingArtwork.Flare(catalog) == ActOneArtwork.Picture("Pickup_LanternFlare"), "Lantern flare pickup selects its own illustration");
            Check(PacingArtwork.Bell(catalog) == ActOneArtwork.Picture("Pickup_ShopkeeperBell"), "Shopkeeper bell pickup selects its own illustration");
            Debug.Log("ART_MASTER_CHECKS " + notes.Count + " PASS");
        }
        public static void VerifyAndBuild()
        {
            Build(true);
        }
        public static void BuildPreview() => Build(false);
        public static void VerifyAvailableAndBuild() => Build(true, false);
        private static void Build(bool complete, bool requireCustomerHD = true)
        {
            ArtMasterImporter.Import();
            if (complete) { RunArtworkChecks(requireCustomerHD); MienChoChecks.Run(); WaitingActOneChecks.Run(); HeroIdentityChecks.Run(); }
            const string output = "Builds/ArtMaster"; Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "RunBoot", "RunFrontend", "RunArena", "WaitingRoom" }.Select(s => "Assets/ArcaneBrigade/Scenes/" + s + ".unity").ToArray(),
                locationPathName = output + "/ArcaneBrigade.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            File.WriteAllText("PreviewExports/ArtMaster/Build.txt", "Scope: " + (requireCustomerHD ? "Complete HD delivery" : "Available artwork; Customer HD frames and two companion atlas edge repairs pending image-generation quota") + "\nResult: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nBytes: " + report.summary.totalSize + "\nPlayer: " + output + "/ArcaneBrigade.exe\n");
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0) throw new InvalidOperationException("Art master Windows build failed");
            Directory.CreateDirectory(output + "/ThirdPartyNotices");
            foreach (string source in Directory.GetFiles("Assets/ArcaneBrigade/Resources/Fonts", "*OFL.txt")) File.Copy(source, output + "/ThirdPartyNotices/" + Path.GetFileName(source), true);
            const string audio = "Assets/ArcaneBrigade/Resources/Audio/Licensed";
            if (Directory.Exists(audio)) foreach (string source in Directory.GetFiles(audio, "*.txt")) File.Copy(source, output + "/ThirdPartyNotices/" + Path.GetFileName(source), true);
        }
    }
}
