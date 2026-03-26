using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The combat arena layout: real art fits the planned footprints, no two visible combat props overlap, every
    /// one stays inside the arena (watchers at its rim), the story props always show, and toys and spectacles keep off them.</summary>
    public static class ArenaLayoutChecks
    {
        public static void Run()
        {
            var notes = new List<string>(); int count = 0;
            void Check(bool value, string label) { if (!value) throw new InvalidOperationException("Arena layout: " + label); count++; }
            void Require(bool value, Func<string> label) { if (!value) throw new InvalidOperationException("Arena layout: " + label()); count++; } // costly captions only on failure
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            Check(catalog != null, "Run catalog loads");

            // 1. the planner's footprints hold the real painted art at its drawn size (every frame it can wear)
            void Fits(Sprite sprite, float scale, Vector2 nominal, string label)
            {
                if (sprite == null) return;
                Vector2 drawn = SpriteInkMetrics.BoundsOf(sprite).size * scale;
                notes.Add("art " + label + " " + sprite.name + " drawn " + drawn.ToString("0.00") + " nominal " + nominal.ToString("0.00"));
                Check(drawn.x <= nominal.x * 1.04f + .02f && drawn.y <= nominal.y * 1.04f + .02f, "Footprint holds the art " + label + " " + sprite.name + " drawn " + drawn.ToString("0.00") + " > " + nominal.ToString("0.00"));
            }
            float AtHeight(Sprite sprite, float height) => sprite == null ? 0 : height / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(sprite).height);
            foreach (string[] zone in ArenaLayoutPlan.ZoneProps)
                for (int i = 0; i < zone.Length; i++)
                {
                    Sprite prop = ActOneArtwork.Picture(zone[i]); float height = ArenaLayoutPlan.ZonePropHeight(i);
                    Check(prop != null, "Zone prop art exists " + zone[i]);
                    Fits(prop, AtHeight(prop, height), new Vector2(height * ArenaLayoutPlan.Aspect(zone[i]), height), "prop");
                }
            for (int i = 0; i < 5; i++)
            {
                Sprite seated = ActOneArtwork.Picture(ArenaLayoutPlan.SeatedName(i));
                Fits(seated, AtHeight(seated, ArenaLayoutPlan.SeatedHeight), new Vector2(ArenaLayoutPlan.SeatedHeight * ArenaLayoutPlan.Aspect(ArenaLayoutPlan.SeatedName(i)), ArenaLayoutPlan.SeatedHeight), "seated");
            }
            foreach (string name in new[] { "MienCho/ActOne/WarmChair", "MienCho/ActOne/WarmChair_Coins", "MienCho/ActOne/WarmChair_Jar", "MienCho/ActOne/WarmChair_Phone" })
            { Sprite chair = Resources.Load<Sprite>(name); Fits(chair, AtHeight(chair, 1.55f), ArenaLayoutPlan.ChairInk, "chair"); }
            // (the older wide armchair is only a fallback when the warm chair art is missing; the layout's fit shrinks it into place)
            var assets = NightmareAssets.Current; var cabinets = GroundedSceneryArtwork.Current; var redrawn = FinalQualityArtwork.Current;
            Check(assets != null && assets.Scenery.Length >= 15, "Landmark art loads");
            var arts = new Dictionary<int, int[]> { { 0, new[] { 0, 1, 2 } }, { 1, new[] { 3 } }, { 2, new[] { 4, 5, 6 } }, { 3, new[] { 8, 9 } }, { 4, new[] { 10, 11 } }, { 6, new[] { 13 } }, { 7, new[] { 14 } } };
            foreach (var pair in arts)
                foreach (int art in pair.Value)
                {
                    var pictures = new List<Sprite> { assets.Scenery[art] };
                    if ((art == 8 || art == 9) && cabinets != null && cabinets.Cabinets.Length == 2) pictures.Add(cabinets.Cabinets[art - 8]);
                    if (redrawn != null) for (float time = 0; time < 9; time += .1f) pictures.Add(redrawn.Frame("landmark-" + art, time, false));
                    foreach (Sprite picture in pictures.Where(p => p != null).Distinct())
                        Fits(picture, ArenaLayoutPlan.LandmarkWidth(pair.Key) / Mathf.Max(.01f, picture.bounds.size.x), ArenaLayoutPlan.LandmarkInk(pair.Key), "landmark" + pair.Key);
                }
            var architecture = WaitingSceneryArtwork.Current;
            Check(architecture != null && architecture.Frames.Length == 12, "Architecture art loads");
            for (int i = 0; i < 4; i++)
                for (int damage = 0; damage < 3; damage++)
                {
                    Sprite frame = architecture.Frame(i, damage);
                    Fits(frame, ArenaLayoutPlan.ArchitectureWidth(i) / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(frame).width), ArenaLayoutPlan.ArchitectureInk(i), "architecture" + i);
                }
            var october = OctoberArtwork.Current;
            if (october != null && october.Watchers != null && october.Watchers.Length == 12)
                for (int i = 0; i < 3; i++)
                    for (int frame = 0; frame < 4; frame++)
                    {
                        Sprite eye = october.Watchers[i * 4 + frame];
                        Fits(eye, ArenaLayoutPlan.WatcherWidth(i) / Mathf.Max(.01f, eye.bounds.size.x), ArenaLayoutPlan.WatcherInk(i), "watcher" + i);
                    }
            foreach (string state in new[] { "Intact", "Cracked", "Broken" })
            {
                // the lamp's painted pixels about its floor contact (uv .40, .025), per unit of full sprite height
                Sprite lamp = Resources.Load<Sprite>("MienCho/Lanterns/" + state); Check(lamp != null, "Lantern art loads " + state);
                var pixels = new Texture2D(2, 2); pixels.LoadImage(File.ReadAllBytes("Assets/ArcaneBrigade/Resources/MienCho/Lanterns/" + state + ".png"));
                Color32[] colors = pixels.GetPixels32(); int w = pixels.width, h = pixels.height, x0 = w, x1 = -1, y1 = -1;
                for (int i = 0; i < colors.Length; i++) if (colors[i].a > 20) { int x = i % w, y = i / w; x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x); y1 = Mathf.Max(y1, y); }
                UnityEngine.Object.DestroyImmediate(pixels);
                float left = (x0 - w * .40f) / h, right = (x1 + 1 - w * .40f) / h, top = (y1 + 1 - h * .025f) / h;
                notes.Add("art lantern " + state + " left " + left.ToString("0.000") + " right " + right.ToString("0.000") + " top " + top.ToString("0.000"));
                Check(left >= .102f - .5553f * .5f - .01f && right <= .102f + .5553f * .5f + .01f && top <= .961f + .01f, "Lantern footprint holds its painted body " + state + " " + left + " / " + right + " / " + top);
            }

            // 2. every combat arena of Act I (four zones, keepsakes or not, watchers from wave 11), endless and the special fights
            var sizes = new[] { catalog.WorldSize, new Vector2(24, 13), new Vector2(28, 20) };
            int plans = 0, hidden = 0, shrunk = 0;
            foreach (Vector2 size in sizes)
                foreach (int seed in size == catalog.WorldSize ? new[] { 161026, 171026, 131026, 7, -90210 } : new[] { 161026, 7 })
                    foreach (int wave in Enumerable.Range(1, 20).Concat(new[] { 21, 26, 33 }))
                        for (int flags = 0; flags < 4; flags++)
                        {
                            var scenery = new ArenaScenery(size, wave, seed, wave <= ActOneCanon.Waves, (flags & 1) != 0, (flags & 2) != 0);
                            ArenaLayoutPlan plan = ArenaLayoutPlan.For(scenery); plans++;
                            string where = " " + scenery;
                            Check(ArenaLayoutPlan.For(new ArenaScenery(size, wave, seed, wave <= ActOneCanon.Waves, (flags & 1) != 0, (flags & 2) != 0)) == plan, "One plan per arena" + where);
                            var shown = plan.Spots.Where(s => s.Visible).ToList();
                            Rect arena = Rect.MinMaxRect(-size.x * .5f, -size.y * .5f, size.x * .5f, size.y * .5f);
                            Rect inner = Rect.MinMaxRect(plan.Bounds.xMin + ArenaLayoutPlan.RimBand, plan.Bounds.yMin + ArenaLayoutPlan.RimBand, plan.Bounds.xMax - ArenaLayoutPlan.RimBand, plan.Bounds.yMax - ArenaLayoutPlan.RimBand);
                            foreach (var spot in shown)
                            {
                                Rect rect = spot.Footprint;
                                Require(rect == spot.FootprintAt(spot.Foot, spot.Fit), () => "Footprint matches the planned spot " + spot.Id + where);
                                Require(rect.xMin >= arena.xMin && rect.xMax <= arena.xMax && rect.yMin >= arena.yMin && rect.yMax <= arena.yMax, () => "Inside the arena " + spot.Id + " " + rect + where);
                                if (spot.Id.StartsWith("watcher")) Require(!rect.Overlaps(inner), () => "Watcher keeps to the rim " + spot.Id + where);
                                Require((spot.Foot - spot.Wish).magnitude <= ArenaLayoutPlan.Reach + .01f, () => "Stays near its wish " + spot.Id + where);
                                if (spot.Fit < 1) { shrunk++; Require(spot.Shrink, () => "Only story props come in smaller " + spot.Id + where); }
                            }
                            for (int a = 0; a < shown.Count; a++)
                                for (int b = a + 1; b < shown.Count; b++)
                                    Require(!shown[a].Footprint.Overlaps(shown[b].Footprint), () => "No overlap " + shown[a].Id + " " + shown[a].Footprint + " / " + shown[b].Id + " " + shown[b].Footprint + where);
                            Check(plan.Reserved.Count == shown.Count, "Every visible prop is reserved for the simulation" + where);
                            // what must be there: the chair, every landmark, every lantern; in the full-size arena also the zone props and all architecture
                            foreach (var spot in plan.Spots)
                            {
                                bool story = spot.Id == "chair" || spot.Id.StartsWith("landmark") || spot.Id.StartsWith("lantern");
                                bool main = size == catalog.WorldSize;
                                if (story || main && (spot.Id.StartsWith("prop") || spot.Id.StartsWith("architecture") || spot.Id.StartsWith("seated")))
                                    Require(spot.Visible, () => "Always shown " + spot.Id + where + "\n" + string.Join("\n", plan.Describe()));
                                if (main && spot.Id.StartsWith("lantern")) Require(spot.Fit == 1, () => "Lanterns keep their full size in the main arena " + spot.Id + where + "\n" + string.Join("\n", plan.Describe()));
                                if (!spot.Visible) hidden++;
                            }
                            Check(plan.Spots.Count(s => s.Id.StartsWith("watcher")) == (wave >= 11 ? 8 : 0), "Watchers only from wave 11" + where);
                            Check(plan.Spots.Count(s => s.Id.StartsWith("prop")) == (wave <= 20 ? ArenaLayoutPlan.ZoneProps[ActOneCanon.Zone(wave)].Length : 0), "Zone props only in Act I" + where);
                            Check(plan.TryGet("landmark6", out _) == ((flags & 1) != 0) && plan.TryGet("landmark7", out _) == ((flags & 2) != 0), "Keepsakes only once rescued or sent" + where);
                            if (seed == 161026 && flags == 3 && size == catalog.WorldSize && (wave == 1 || wave == 8 || wave == 13 || wave == 18)) notes.AddRange(plan.Describe());
                        }
            notes.Add("plans " + plans + " left out " + hidden + " shrunk " + shrunk);

            // 3. the simulation plans the very same layout from its catalog, and its toys and spectacles keep off it
            int toys = 0;
            foreach (int seed in new[] { 161026, 171026, 7 })
                for (int wave = 1; wave <= 20; wave++)
                    for (int flags = 0; flags < 4; flags += 3)
                    {
                        using var session = new RunSession(catalog); session.Start("LanternKnight", seed);
                        session.State.Wave = wave; session.State.Secrets.RescuedToy = flags != 0; session.State.Secrets.LetterSent = flags != 0;
                        CheckpointCatalog combat = session.PrepareCombat();
                        Check(combat.Scenery.HasValue, "Combat catalog carries the arena scenery " + wave);
                        combat.EnableArenaToys = true;
                        var simulation = new CombatSimulation(combat); simulation.Start(combat.Seed);
                        ArenaLayoutPlan plan = ArenaLayoutPlan.For(ArenaScenery.For(session.State, combat.ArenaSize));
                        Check(simulation.ArenaLayout == plan, "Simulation and presentation share one plan " + wave + " / " + seed);
                        foreach (ArenaToyState toy in simulation.ArenaToys)
                        {
                            toys++;
                            Require(!plan.Blocks(toy.Position, CombatSimulation.ArenaToyFootprint), () => "Toy spawns clear of every prop " + toy.Kind + " " + toy.Position + " wave " + wave + " seed " + seed);
                        }
                        // a spectacle comes only on its odd, boss-free waves
                        if (combat.EnableSpectacles && MidwaveSpectacle.Eligible(wave, combat.Boss != null))
                        foreach (SpectacleKind kind in Enum.GetValues(typeof(SpectacleKind)))
                            foreach (Vector2 wish in new[] { Vector2.zero, new Vector2(-7, 4), new Vector2(9, -5), new Vector2(3, 7), new Vector2(-15, 0), new Vector2(12, 8) })
                            {
                                Rect sweep = MidwaveSpectacle.Sweep(kind);
                                Vector2 rest = MidwaveSpectacle.ClearOrigin(kind, wish, plan, combat.ArenaSize) + MidwaveSpectacle.RestOffset(kind);
                                Require(!plan.Blocks(new Rect(sweep.position + rest, sweep.size)), () => "Spectacle waits clear of every prop " + kind + " " + wish + " wave " + wave + " seed " + seed);
                            }
                    }
            Check(toys > 100, "Toys were actually placed " + toys);
            notes.Add("toys " + toys);
            Directory.CreateDirectory("PreviewExports/ArenaLayout");
            File.WriteAllLines("PreviewExports/ArenaLayout/Checks.txt", notes);
            Debug.Log("ARENA_LAYOUT_CHECKS_PASSED " + count);
        }
    }
}
