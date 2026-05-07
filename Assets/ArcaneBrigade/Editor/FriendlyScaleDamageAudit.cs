using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class FriendlyScaleDamageAudit
    {
        public static void Run()
        {
            string label = Environment.GetCommandLineArgs().SkipWhile(x => x != "-audit-label").Skip(1).FirstOrDefault() ?? "Before";
            string root = "PreviewExports/FriendlyScaleDamage/" + label;
            Directory.CreateDirectory(root);
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            SpriteInkMetricsBuilder.Build();
            var rows = new List<string> { "role\tid\trarity\tauthoredHeight\treferenceHeight\tinkHeight\tinkWidth\tdrawnHeight\tdrawnWidth\tweapon\tdamage\tcooldown" };
            foreach (bool friend in new[] { false, true })
                foreach (var source in friend ? catalog.Companions : catalog.Champions)
                {
                    Sprite sprite = source.AuthoredMotion != null ? source.AuthoredMotion.Idle[0] : source.Body;
                    Rect ink = SpriteInkMetrics.BoundsOf(sprite);
                    float reference = source.AuthoredMotion != null ? source.AuthoredMotion.ReferenceHeight : sprite.bounds.size.y;
                    var state = new UnitState { Definition = source, Faction = friend ? Faction.Companion : Faction.Champion };
                    float height = K8Motion.BodyHeight(state);
                    reference = Mathf.Max(ink.height, ink.width / (friend ? 1.5f : 1.4f));
                    float scale = height / Mathf.Max(.01f, reference);
                    rows.Add(string.Join("\t", friend ? "companion" : "hero", source.Id, source.Rarity, F(source.BodyHeight), F(reference), F(ink.height), F(ink.width), F(ink.height * scale), F(ink.width * scale), source.Weapon.Id, F(source.Weapon.Damage), F(source.Weapon.Cooldown)));
                }
            File.WriteAllLines(root + "/Sizes.tsv", rows);
            // Uses the current production combat fixture, without invoking builders or changing source stats.
            var fight = typeof(BuildBalanceDiagnostics).GetMethod("Fight", BindingFlags.Static | BindingFlags.NonPublic);
            rows = new List<string> { "role\tid\tweapon\trarity\ttier\twave\tseed\tdamage\tseconds\tcompiledHit\tcooldown" };
            foreach (bool hero in new[] { false, true })
                foreach (var source in hero ? catalog.Champions : catalog.Companions)
                    foreach (int tier in hero ? new[] { 1 } : new[] { 1, 2, 3 })
                        foreach (int seed in new[] { 77138, 33071 })
                        {
                            using var run = new RunSession(catalog);
                            run.Start(hero ? source.Id : "LanternKnight", seed);
                            run.State.Wave = 10; run.State.Roster.Clear();
                            if (!hero) run.State.Roster.Add(new RosterEntry { UnitId = source.Id, WeaponId = source.Weapon.Id, Tier = tier });
                            var arena = run.PrepareCombat(); arena.Boss = null; arena.StoryCompanions = Array.Empty<UnitDefinition>();
                            arena.EnableHeroMechanics = arena.EnableHeroIdentities = false;
                            if (!hero) { arena.Champion.Weapon.Damage = 0; arena.Champion.CollisionDamage = 0; }
                            var built = hero ? arena.Champion : arena.Companion;
                            // Fight builds the actual roster; select the matching compiled friend, not the fallback companion.
                            if (!hero && arena.StartingCompanions != null) built = arena.StartingCompanions.FirstOrDefault(u => u.Id == source.Id) ?? built;
                            var sim = (CombatSimulation)fight.Invoke(null, new object[] { arena, seed, 18f, false });
                            float dealt = sim.Contributions.Where(c => c.UnitId == source.Id).Sum(c => c.Damage);
                            rows.Add(string.Join("\t", hero ? "hero" : "companion", source.Id, source.Weapon.Id, source.Rarity, tier, 10, seed, F(dealt), F(sim.Elapsed), F(built.Weapon.Damage), F(built.Weapon.Cooldown)));
                        }
            File.WriteAllLines(root + "/Damage.tsv", rows);
            Debug.Log("FRIENDLY_SCALE_DAMAGE_AUDIT " + label + " " + rows.Count);
        }
        private static string F(float x) => x.ToString("0.####", CultureInfo.InvariantCulture);
        public static void Weapons()
        {
            string label = Environment.GetCommandLineArgs().SkipWhile(x => x != "-audit-label").Skip(1).FirstOrDefault() ?? "Weapons";
            string root = "PreviewExports/FriendlyScaleDamage/" + label;
            Directory.CreateDirectory(root);
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var fight = typeof(BuildBalanceDiagnostics).GetMethod("Fight", BindingFlags.Static | BindingFlags.NonPublic);
            var rows = new List<string> { "weapon\tkind\ttier\tseed\tdamage\tseconds\tcompiledHit\tcooldown" };
            foreach (var weapon in catalog.Weapons)
                foreach (int tier in new[] { 1, 2, 3 })
                    foreach (int seed in new[] { 77138, 33071 })
                    {
                        using var run = new RunSession(catalog); run.Start("LanternKnight", seed);
                        run.State.Wave = 10; run.State.Roster.Clear();
                        run.State.Roster.Add(new RosterEntry { UnitId = "JellySwordsman", WeaponId = weapon.Id, Tier = tier });
                        var arena = run.PrepareCombat(); arena.Boss = null; arena.StoryCompanions = Array.Empty<UnitDefinition>();
                        arena.EnableHeroMechanics = arena.EnableHeroIdentities = false;
                        arena.Champion.Weapon.Damage = 0; arena.Champion.CollisionDamage = 0;
                        var built = arena.StartingCompanions.First(u => u.Id == "JellySwordsman");
                        var sim = (CombatSimulation)fight.Invoke(null, new object[] { arena, seed, 18f, false });
                        float dealt = sim.Contributions.Where(c => c.UnitId == "JellySwordsman").Sum(c => c.Damage);
                        rows.Add(string.Join("\t", weapon.Id, weapon.Kind, tier, seed, F(dealt), F(sim.Elapsed), F(built.Weapon.Damage), F(built.Weapon.Cooldown)));
                    }
            File.WriteAllLines(root + "/Damage.tsv", rows);
            Debug.Log("FRIENDLY_WEAPONS_AUDIT " + rows.Count);
        }
        public static void VerifyAndMeasure()
        {
            Weapons();
            VerifyPresentation();
        }
        public static void VerifyPresentation()
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var rows = new List<string> { "role\tid\ttier\treduced\tphase\tpaintedHeight\tpaintedWidth" };
            var pictures = new List<string> { "role\tid\tpath\tx\ty\twidth\theight\ttextureWidth\ttextureHeight\tworldWidth\tworldHeight" };
            foreach (bool friend in new[] { false, true })
                foreach (var source in friend ? catalog.Companions : catalog.Champions)
                    for (int tier = 1; tier <= (friend ? 3 : 1); tier++)
                    {
                        var definition = UnityEngine.Object.Instantiate(source);
                        definition.CombatTier = tier;
                        if (friend) definition.BodyHeight *= CompanionTierScaling.Body(tier);
                        var state = new UnitState { Definition = definition, Faction = friend ? Faction.Companion : Faction.Champion, Id = 9, Alive = true, Health = definition.Health, Facing = Vector2.right, LastHit = -100, AttackStarted = -100 };
                        var go = new GameObject("SizingAudit"); var view = go.AddComponent<UnitVisual>();
                        view.CreateHierarchy(catalog.Arena.Coin); view.Bind(state, catalog.Arena, true);
                        foreach (bool reduced in new[] { false, true })
                            for (int phase = 0; phase < 4; phase++)
                            {
                                view.ReducedMotion = reduced;
                                state.PreviousPosition = phase == 1 ? Vector2.left * .03f : Vector2.zero;
                                state.AttackWindup = definition.Weapon.Windup;
                                state.AttackStarted = phase < 2 ? -100 : 10 - (phase == 2 ? definition.Weapon.Windup * .5f : definition.Weapon.Windup + .03f);
                                state.PendingAttack = phase == 2;
                                view.Display(10, 1);
                                Rect ink = SpriteInkMetrics.BoundsOf(view.DisplayedBody);
                                Vector3 size = view.BodyRenderer.transform.localScale;
                                float h = ink.height * size.y, w = ink.width * size.x;
                                if (!float.IsFinite(h) || !float.IsFinite(w) || h < .2f || h > K8Motion.BodyHeight(state) * 1.65f || w > K8Motion.BodyHeight(state) * 2.2f)
                                    throw new InvalidOperationException("Friendly render size outlier " + source.Id + " " + tier + " " + phase + " " + h + " " + w);
                                rows.Add(string.Join("\t", friend ? "companion" : "hero", source.Id, tier, reduced, phase, F(h), F(w)));
                                if (tier == 1 && !reduced && phase == 0)
                                {
                                    Sprite sprite = view.DisplayedBody;
                                    Rect rect = sprite.rect;
                                    pictures.Add(string.Join("\t", friend ? "companion" : "hero", source.Id, UnityEditor.AssetDatabase.GetAssetPath(sprite.texture), F(rect.x), F(rect.y), F(rect.width), F(rect.height), sprite.texture.width, sprite.texture.height, F(w), F(h)));
                                }
                            }
                        float normal = K8Motion.BodyHeight(state);
                        definition.BodyHeight *= 1.5f; definition.BodyScale = 1.5f;
                        if (Mathf.Abs(K8Motion.BodyHeight(state) - normal * 1.5f) > .001f) throw new InvalidOperationException("Size bonus lost " + source.Id);
                        UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(definition);
                    }
            Directory.CreateDirectory("PreviewExports/FriendlyScaleDamage/Presentation");
            File.WriteAllLines("PreviewExports/FriendlyScaleDamage/Presentation/Poses.tsv", rows);
            File.WriteAllLines("PreviewExports/FriendlyScaleDamage/Presentation/Pictures.tsv", pictures);
            Debug.Log("FRIENDLY_PRESENTATION_VERIFIED " + (rows.Count - 1));
        }
    }
}
