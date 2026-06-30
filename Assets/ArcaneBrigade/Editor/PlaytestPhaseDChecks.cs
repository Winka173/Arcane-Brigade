using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Play-test review phase D (Docs/Playtest_Review_2026-10-20.md, items 22 and 24): boss telegraphs as red ink
    /// outlines covering every boss move, boss contact shapes on the painted body, the four map identities and their living
    /// elements. RunBatch runs these and the existing suites the phase touches, in one launch.</summary>
    public static class PlaytestPhaseDChecks
    {
        public static void RunBatch()
        {
            foreach (string folder in new[] { "K22", "K24", "K25", "PhaseD" }) Directory.CreateDirectory("PreviewExports/" + folder); // suites that write their notes there
            var lines = new List<string>(); int failed = 0;
            foreach (var (name, run) in new (string, Action)[]
            {
                ("PhaseD.BossShapes", BossShapes),
                ("PhaseD.BossTelegraphs", BossTelegraphs),
                ("PhaseD.MapIdentity", Maps),
                ("PhaseD.MapElements", MapElements),
                ("FeelTelegraph", FeelTelegraphChecks.Run),
                ("EnemyRoster", EnemyRosterChecks.Run),
                ("K25EnemyAffix", K25EnemyAffixChecks.Run),
                ("K22EnemyEvolution", K22EnemyEvolutionChecks.Run),
                ("K21BossEncounter", K21BossEncounterChecks.Run),
                ("K20BossAnatomy", K20BossAnatomyChecks.Run),
                ("K24OutsideStage", K24OutsideStageChecks.Run),
                ("CopyAudit", CopyAuditChecks.Run),
                ("BuildSizePolicy", BuildSizeChecks.Policy),
            })
            {
                try { run(); lines.Add("PASS " + name); }
                catch (Exception error)
                {
                    failed++; Exception root = error.InnerException ?? error;
                    string[] stack = (root.StackTrace ?? "").Split('\n');
                    lines.Add("FAIL " + name + ": " + root.Message + (root is InvalidOperationException ? "" : " @ " + string.Join(" | ", stack, 0, Math.Min(3, stack.Length)).Trim()));
                }
                Debug.Log("SUITE_RESULT " + lines[lines.Count - 1]);
            }
            Directory.CreateDirectory("PreviewExports/PhaseD");
            File.WriteAllLines("PreviewExports/PhaseD/Checks.txt", lines.Concat(notes));
            Debug.Log("PHASED_CHECKS " + (lines.Count - failed) + "/" + lines.Count + " PASS\n" + string.Join("\n", lines));
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }

        private static readonly List<string> notes = new List<string>();
        private static void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("PhaseD " + caption); }
        private static RunCatalog Catalog => Resources.Load<RunCatalog>("RunCatalog");
        private static readonly MethodInfo build = typeof(RunSession).GetMethod("BuildEnemy", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>Every boss the game fields: the ten catalog bosses (endless), the Act I story bosses (waves 5, 10, 15, 20),
        /// the Keeper and the Last Customer, each built as a run builds it (fresh per fight: a session's clones die with it).</summary>
        private sealed class BossCase { public string Name; public int Wave; public Func<RunSession, UnitDefinition> Build; }
        private static List<BossCase> AllBosses()
        {
            var list = new List<BossCase>();
            foreach (UnitDefinition source in Catalog.Bosses) { UnitDefinition s = source; list.Add(new BossCase { Name = "catalog " + s.Id, Wave = 12, Build = run => (UnitDefinition)build.Invoke(run, new object[] { s, 1f }) }); }
            foreach (int wave in new[] { 5, 10, 15, 20 }) list.Add(new BossCase { Name = "story w" + wave, Wave = wave });
            if (OutsideStageAssets.Current?.Boss != null) list.Add(new BossCase { Name = "Keeper", Wave = 17, Build = run => (UnitDefinition)build.Invoke(run, new object[] { OutsideStageAssets.Current.Boss, 1f }) });
            if (NightmareAssets.Current?.Customer != null) list.Add(new BossCase { Name = "Last Customer", Wave = 19, Build = run => (UnitDefinition)build.Invoke(run, new object[] { NightmareAssets.Current.Customer, 1f }) });
            return list;
        }

        private static CombatSimulation Fight(RunSession session, BossCase c)
        {
            session.Start("LanternKnight", 22022); session.State.Roster.Clear(); session.State.Wave = c.Wave; session.State.Phase = SessionPhase.Combat;
            CheckpointCatalog arena = session.PrepareCombat();
            UnitDefinition boss = c.Build?.Invoke(session) ?? arena.Boss;
            if (boss == null) return null;
            arena.Boss = boss; arena.EnemyBudget = 0; arena.BossSwarmBudget = 0; arena.MapEnvironment = -1; arena.Dodge = 0;
            arena.EnableBossArenaPhase = arena.EnableArenaToys = arena.EnableSpectacles = false;
            arena.StartingCompanions = Array.Empty<UnitDefinition>(); arena.StoryCompanions = Array.Empty<UnitDefinition>();
            var sim = new CombatSimulation(arena); sim.Start(22022);
            sim.Champion.NextAttack = float.PositiveInfinity;
            return sim;
        }

        /// <summary>Item 22 (2): each boss's contact ellipse is ~85% of its painted body; touching the art always hurts, standing
        /// visibly clear never does (a real fight, hero standing still, boss held in place).</summary>
        public static void BossShapes()
        {
            foreach (BossCase c in AllBosses())
            {
                using var session = new RunSession(Catalog);
                CombatSimulation probe = Fight(session, c);
                Require(probe != null, "Boss present: " + c.Name);
                UnitDefinition boss = probe.Boss.Definition; string name = c.Name + " " + boss.Id;
                BossBodyShape.Shape shape = BossBodyShape.Of(boss);
                Require(shape.Valid, "Painted body measured: " + name);
                Require(Mathf.Abs(shape.Half.x - shape.Painted.width * .5f * BossBodyShape.Share) < .001f && Mathf.Abs(shape.Half.y - shape.Painted.height * .5f * BossBodyShape.Share) < .001f, "Contact ellipse is 85% of the painted body: " + name);
                Require(shape.Half.x > boss.Radius && shape.Half.y > boss.Radius * .8f, "Contact shape larger than the old collision circle: " + name + " half=" + shape.Half + " radius=" + boss.Radius);
                notes.Add("NOTE " + name + " painted " + shape.Painted.size.ToString("F2") + " contact half " + shape.Half.ToString("F2") + " (old radius " + boss.Radius.ToString("F2") + ")");
                Vector2 ellipse = shape.Centre;
                var inside = new[] { ellipse + Vector2.right * shape.Half.x * .9f, ellipse + Vector2.left * shape.Half.x * .9f, ellipse + Vector2.up * shape.Half.y * .9f, ellipse + Vector2.down * shape.Half.y * .9f };
                foreach (Vector2 at in inside) Require(Hurts(c, at), "Touching the painted body hurts: " + name + " at " + at.ToString("F2"));
                float hero = probe.Champion.Definition.Radius;
                Rect art = shape.Painted;
                var clear = new[] { new Vector2(art.xMax + hero + .15f, art.center.y), new Vector2(art.xMin - hero - .15f, art.center.y), new Vector2(art.center.x, art.yMax + hero + .15f), new Vector2(art.center.x, art.yMin - hero - .15f) };
                foreach (Vector2 at in clear) Require(!Hurts(c, at), "Standing clear of the art never hurts: " + name + " at " + at.ToString("F2"));
                HitsLandOnBody(probe, shape, name);
            }
        }

        private static readonly MethodInfo inBody = typeof(CombatSimulation).GetMethod("InBody", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo bodySegment = typeof(CombatSimulation).GetMethod("BodySegment", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo inArc = typeof(CombatSimulation).GetMethod("InArc", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly PropertyInfo hitPadding = typeof(CombatSimulation).GetProperty("HitPadding", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>Play-test B: hero and friend shots, swings and blasts hit the painted body ellipse, not only the old circle;
        /// ordinary bodies keep their circle; spatial queries pad far enough to find the painted body.</summary>
        private static void HitsLandOnBody(CombatSimulation sim, BossBodyShape.Shape shape, string name)
        {
            Require(inBody != null && bodySegment != null && inArc != null && hitPadding != null, "Body hit helpers present");
            UnitState b = sim.Boss; b.Position = b.PreviousPosition = Vector2.zero; b.Facing = Vector2.right; b.BodyLift = 0;
            float radius = b.Definition.Radius;
            Vector2 c = BossBodyShape.Centre(b, shape);
            Vector2 dir = Mathf.Abs(c.x + shape.Half.x) >= Mathf.Abs(c.x - shape.Half.x) ? Vector2.right : Vector2.left;
            Vector2 body = c + dir * shape.Half.x * .9f; // on the painted body, past the old circle
            Require(body.magnitude > radius + .05f, "Probe point lies past the old circle: " + name + " at " + body.ToString("F2"));
            Require((bool)inBody.Invoke(null, new object[] { b, body, 0f }), "A blast on the painted body hits: " + name);
            Require(!(bool)inBody.Invoke(null, new object[] { b, c + dir * (shape.Painted.width + 1), 0f }), "A blast clear of the art misses: " + name);
            float fraction = (float)bodySegment.Invoke(null, new object[] { b, body + dir * 4, body, .1f });
            Require(fraction > 0 && fraction < 1, "A shot flying at the painted body hits it on the way in: " + name + " f=" + fraction);
            Vector2 high = c + Vector2.up * (shape.Painted.height + 1);
            Require((float)bodySegment.Invoke(null, new object[] { b, high + dir * 6, high - dir * 6, .1f }) < 0, "A shot passing above the art misses: " + name);
            Require((bool)inArc.Invoke(null, new object[] { b, body + dir * 1.5f, -dir, 1.6f, .5f }), "A swing at the painted body hits: " + name);
            Require(!(bool)inArc.Invoke(null, new object[] { b, body + dir * 1.5f, -dir, .3f, .5f }), "A swing short of the painted body misses: " + name);
            Require(!(bool)inArc.Invoke(null, new object[] { b, body + dir * 1.5f, dir, 1.6f, .5f }), "A swing facing away misses: " + name);
            float padding = (float)hitPadding.GetValue(sim);
            Require(padding >= BossBodyShape.Reach(b.Definition) - .001f && padding >= (body - b.Position).magnitude && padding < 12, "Hit queries pad to the painted body: " + name + " pad=" + padding);
            UnitState hero = sim.Champion; float r = hero.Definition.Radius; Vector2 side = hero.Position + Vector2.right * (r + .5f);
            Require((bool)inBody.Invoke(null, new object[] { hero, side, .51f }) && !(bool)inBody.Invoke(null, new object[] { hero, side, .49f }), "Ordinary bodies keep their circle");
        }
        private static bool Hurts(BossCase c, Vector2 offset)
        {
            using var session = new RunSession(Catalog);
            CombatSimulation sim = Fight(session, c);
            UnitState b = sim.Boss; b.EntranceUntil = 0; b.Definition.Speed = 0;
            b.NextBossPattern = float.PositiveInfinity; b.TelegraphUntil = 0; b.BossRecoveryUntil = float.PositiveInfinity;
            float ready = b.NextContact; bool wound = false; // a boss's first contact is ready 1.45 s into the fight
            for (int i = 0; i < 50; i++)
            {
                b.Position = b.PreviousPosition = Vector2.zero; b.Facing = Vector2.right; b.BodyLift = 0;
                sim.Champion.Position = sim.Champion.PreviousPosition = offset;
                sim.Champion.Health = 1e7f; // the hero cannot fall while the elements are measured
                sim.Step(.05f, Vector2.zero);
                wound |= b.ContactImpactAt > 0 || b.NextContact > ready + .01f;
            }
            return wound;
        }

        /// <summary>Item 22 (1): every boss move in its warning is drawn (a rim, or lane edges with an arrowhead), in outline only.
        /// Each boss fights 24 s against a hero standing off to the side; on every frame each warning area must be inked.</summary>
        public static void BossTelegraphs()
        {
            string source = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/BossTelegraph.cs");
            Require(!source.Contains(".Pool(") && !source.Contains(".Wash(") && !source.Contains(".Cone(") && !source.Contains(".Vignette(") && !source.Contains("Sector("), "Telegraphs are outlines only (no fill, no pool)");
            string trademark = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/BossTrademarkVisual.cs");
            Require(trademark.Contains("BossTelegraph.DashedLine") && trademark.Contains("if (warning) BossTelegraph.Rim"), "Trademark lanes and the stage light's warning use the same outlines");
            var host = new GameObject("PhaseDTelegraph");
            try
            {
                BossTelegraph telegraph = host.AddComponent<BossTelegraph>();
                foreach (BossCase c in AllBosses())
                {
                    using var session = new RunSession(Catalog);
                    CombatSimulation sim = Fight(session, c); string name = c.Name;
                    Require(sim != null, "Boss present: " + name);
                    UnitState b = sim.Boss; b.EntranceUntil = 0; b.Position = b.PreviousPosition = new Vector2(-2, 0);
                    int warned = 0, windups = 0;
                    for (int step = 0; step < 480; step++)
                    {
                        sim.Champion.Health = 1e7f; // the hero cannot fall while the elements are measured
                        sim.Champion.Position = sim.Champion.PreviousPosition = new Vector2(4.5f, 2 * Mathf.Sin(step * .02f));
                        sim.Step(.05f, Vector2.zero);
                        if (sim.Phase != RunPhase.Combat) break;
                        telegraph.Display(sim);
                        int fields = sim.ExpandedFields.Count(f => f.Source != null && f.Source.Definition.IsBoss && f.Kind != ExpandedFieldKind.Roller && CarnivalHazards.IsHostile(f.Kind) && sim.Elapsed < f.At);
                        Require(telegraph.Rims + telegraph.Lanes + telegraph.Marks >= fields, "Every boss area in its warning is inked: " + name + " t=" + sim.Elapsed.ToString("F2") + " areas=" + fields);
                        warned += fields;
                        if (b.Alive && b.TelegraphUntil > sim.Elapsed) { Require(telegraph.Windups >= 1, "The boss's own wind-up is inked: " + name); windups++; }
                    }
                    Require(windups > 0, "The boss wound up at least once in 24 s: " + name);
                    notes.Add("NOTE telegraph " + name + ": wind-up frames " + windups + ", warned areas " + warned);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
        }

        /// <summary>Item 24 (six maps since 25/10/2026): map identities by wave, the pool themed to the map's family, its elite
        /// affix, the living elements only in ordinary waves, painted props in Resources, one guide line per element.</summary>
        public static void Maps()
        {
            MapTheme[] road = { MapTheme.WaitingRoom, MapTheme.PostOffice, MapTheme.Glasshouse, MapTheme.LampHall, MapTheme.Theatre, MapTheme.ArchiveVault };
            int[] firsts = { 1, 5, 9, 12, 15, 18 }, lasts = { 4, 8, 11, 14, 17, 20 };
            for (int leg = 0; leg < road.Length; leg++)
                for (int wave = firsts[leg]; wave <= lasts[leg]; wave++)
                    Require(MapIdentity.For(wave) == road[leg] && MapIdentity.For(wave + 20) == road[leg] && MapIdentity.FirstWave(wave) == firsts[leg] && MapIdentity.FirstWave(wave + 20) == firsts[leg] + 20 && MapIdentity.LastWave(wave) == lasts[leg],
                        "Six maps on the road (1-4, 5-8, 9-11, 12-14, 15-17, 18-20; endless repeats): wave " + wave);
            for (int wave = 2; wave <= 20; wave++) Require(!StageTransfer.ChangesMap(wave) || !ActOneCanon.BossWave(wave), "The carry never lands on a story boss night: wave " + wave);
            Require(Enum.GetValues(typeof(MapTheme)).Cast<MapTheme>().Select(MapIdentity.Affix).Distinct().Count() == 5, "Each map has an elite affix (five affixes for six maps)");
            var known = new HashSet<string>(Catalog.Enemies.Select(u => u.Id).Concat(WaitingEnemyRoster.NewIds));
            foreach (MapTheme map in Enum.GetValues(typeof(MapTheme)))
                foreach (string id in MapIdentity.Family(map)) Require(known.Contains(id), "Family member exists in the Waiting Realm roster: " + map + " " + id);
            for (int wave = 1; wave <= 20; wave++)
            {
                using var session = new RunSession(Catalog);
                session.Start("LanternKnight", 24000 + wave); session.State.Wave = wave; session.State.Roster.Clear();
                CheckpointCatalog arena = session.PrepareCombat();
                MapTheme map = MapIdentity.For(wave);
                bool boss = ActOneCanon.BossWave(wave);
                Require(arena.MapThemed == (int)map, "Wave " + wave + " belongs to " + map);
                Require(arena.MapEnvironment == (boss || wave < 2 ? -1 : (int)map), "Living elements only in ordinary waves: wave " + wave);
                int family = arena.EnemyPool.Count(u => MapIdentity.InFamily(map, u.Id));
                var carry = arena.EnemyPool.Where(u => !MapIdentity.InFamily(map, u.Id)).Select(u => u.Id).Distinct().ToList();
                int distinctFamily = arena.EnemyPool.Where(u => MapIdentity.InFamily(map, u.Id)).Select(u => u.Id).Distinct().Count();
                if (distinctFamily >= 2)
                {
                    Require(family * 2 >= arena.EnemyPool.Length && carry.Count <= 4, "Mostly the map's family, a few carry-overs: wave " + wave + " family " + family + "/" + arena.EnemyPool.Length + " carry " + carry.Count);
                }
                if (wave == 1) Require(arena.EnemyPool.All(u => u.Weapon.Kind == WeaponKind.Contact), "Wave one stays contact-only");
                notes.Add("NOTE wave " + wave + " " + map + ": family " + string.Join("/", arena.EnemyPool.Where(u => MapIdentity.InFamily(map, u.Id)).Select(u => u.Id).Distinct()) + " | carry " + string.Join("/", carry));
            }
            string[] props =
            {
                "WaitingRoom/Board_Idle", "WaitingRoom/Board_Flash", "WaitingRoom/Door_Closed", "WaitingRoom/Door_Ajar", "WaitingRoom/Door_Open", "WaitingRoom/Chair_Stand",
                "PostOffice/Chute", "PostOffice/Letter_A", "PostOffice/Letter_B", "PostOffice/Letter_C", "PostOffice/Letter_Gold", "PostOffice/Stamp", "PostOffice/Scrap_A", "PostOffice/Scrap_B", "PostOffice/Scrap_C", "PostOffice/Scrap_D",
                "PostOffice/Parcel_Intact", "PostOffice/Parcel_Cracked", "PostOffice/Parcel_Burst",
                "LampHall/Lamp_Lit", "LampHall/Lamp_Snuffed", "LampHall/LightGlow", "LampHall/Hand_Reach", "LampHall/Hand_Snuff",
                "ArchiveVault/Clock_Idle", "ArchiveVault/Shelf_Straight",
            };
            foreach (string prop in props)
            {
                Sprite sprite = Resources.Load<Sprite>("Environment/" + prop);
                Require(sprite != null, "Painted prop in Resources: " + prop);
                Require(Mathf.Max(sprite.texture.width, sprite.texture.height) <= 512, "Prop sized to its on-screen need: " + prop);
            }
            foreach (MapIdentity.Element element in Enum.GetValues(typeof(MapIdentity.Element)))
            {
                PlayerGuide.Tip tip = PlayerGuide.Tips.FirstOrDefault(t => t.Id == MapIdentity.TipId(element));
                Require(tip != null && !string.IsNullOrEmpty(tip.Line) && tip.Line.Length <= 110, "One short guide line: " + element);
                bool artPending = element == MapIdentity.Element.Blooms || element == MapIdentity.Element.Spotlight; // the new maps' props are still to be painted
                if (artPending && MapIdentity.Sprite(MapIdentity.Prop(element)) == null) notes.Add("NOTE guide icon not painted yet: " + element);
                else Require(MapIdentity.Sprite(MapIdentity.Prop(element)) != null, "Guide icon painted: " + element);
            }
            string visual = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/MapEnvironmentVisual.cs");
            foreach (string map in Directory.GetFiles("Assets/ArcaneBrigade/Runtime/Presentation/Maps", "*.cs")) visual += File.ReadAllText(map); // each map draws in its own file
            Require(!visual.Contains(".Pool(") && !visual.Contains(".Wash(") && !visual.Contains(".Vignette(") && !visual.Contains(".Cone("), "Map elements draw no ground pools");
        }

        private static readonly List<RunSession> sessions = new List<RunSession>();
        private static readonly MethodInfo mapDamage = typeof(CombatSimulation).GetMethod("MapDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo mapCoins = typeof(CombatSimulation).GetMethod("MapCoins", BindingFlags.Instance | BindingFlags.NonPublic);

        private static CombatSimulation MapFight(int wave)
        {
            var session = new RunSession(Catalog); sessions.Add(session);
            session.Start("LanternKnight", 24100 + wave); session.State.Wave = wave; session.State.Roster.Clear(); session.State.Phase = SessionPhase.Combat;
            CheckpointCatalog arena = session.PrepareCombat();
            Require(arena.MapEnvironment == (int)MapIdentity.For(wave), "Living elements run in wave " + wave);
            var sim = new CombatSimulation(arena); sim.Start(24100 + wave);
            return sim;
        }
        private static void Run(CombatSimulation sim, float seconds, Func<int, Vector2> move = null)
        {
            for (int i = 0; i < seconds * 20 && sim.Phase == RunPhase.Combat; i++)
            {
                sim.Champion.Health = 1e7f; // the hero cannot fall while the elements are measured
                sim.Step(.05f, move?.Invoke(i) ?? new Vector2(Mathf.Cos(i * .01f), Mathf.Sin(i * .013f)) * .5f);
            }
        }

        /// <summary>Item 24: each map's elements appear, telegraph and do what their guide line says (mildly).</summary>
        public static void MapElements()
        {
            try { MapElementsInner(); }
            finally { foreach (RunSession s in sessions) s.Dispose(); sessions.Clear(); }
        }
        private static void MapElementsInner()
        {
            // requirement 25/10/2026: six maps, each with its own living elements (Docs/Map_Mechanics_2026-10-25.md); every map runs
            // a long stretch of its first night without fault, is the map its wave says, and lends the hero at least one boost
            int[] firsts = { 1, 5, 9, 12, 15, 18 };
            foreach (int wave in firsts)
            {
                CombatSimulation sim = MapFight(wave);
                bool boosted = false;
                for (int i = 0; i < 40 * 20 && sim.Phase == RunPhase.Combat; i++)
                {
                    sim.Champion.Health = 1e7f; // the hero cannot fall while the elements are measured
                    MapEnvironmentState s = sim.MapEnvironment;
                    if (s != null && s.Boosts.Count > 0) boosted = true;
                    // walk a slow circle so pickups, lights and props come within reach
                    float a = i * .02f; Vector2 move = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    sim.Step(.05f, move);
                }
                Require(sim.MapEnvironment != null && sim.MapEnvironment.Theme == MapIdentity.For(wave), "Wave " + wave + " plays " + MapIdentity.For(wave));
                Require(boosted || wave == 15, "A map boost reaches the hero on " + MapIdentity.For(wave)); // the Theatre's light is a standing boost
            }
        }
    }
}
