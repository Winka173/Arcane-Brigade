using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Enemy roster v2 checks (Docs/Enemy_Roster_v2.md). Without art nothing new spawns and the older species keep
    /// their look; with in-memory stand-in enemies (an existing enemy's drawings) each new move does what the doc says in the
    /// simulation itself.</summary>
    public static class EnemyRosterChecks
    {
        private const float Tick = .05f;
        private static object Invoke(CombatSimulation simulation, string method, params object[] args) => typeof(CombatSimulation).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(simulation, args);

        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool valid, string caption) { if (!valid) throw new InvalidOperationException("ENEMY_ROSTER " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            WaitingEnemyRoster.Invalidate();
            WaitingEnemyRoster roster = WaitingEnemyRoster.Current;
            var made = new List<UnityEngine.Object>();
            try
            {
                Zones(Check);
                ArtGate(catalog, roster, Check);
                AuthoredMotionSet standIn = catalog.Enemies.First(enemy => enemy.AuthoredMotion != null && enemy.AuthoredMotion.Idle.Length >= 2 && enemy.AuthoredMotion.Walk.Length >= 2).AuthoredMotion;
                UnitDefinition Make(WaitingEnemyKind kind)
                {
                    EnemyRosterBuilder.Spec spec = EnemyRosterBuilder.Specs.First(item => item.Kind == kind);
                    var unit = ScriptableObject.CreateInstance<UnitDefinition>(); var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
                    EnemyRosterBuilder.Define(unit, weapon, spec, catalog, standIn);
                    made.Add(unit); made.Add(weapon);
                    return unit;
                }
                foreach (EnemyRosterBuilder.Spec spec in EnemyRosterBuilder.Specs)
                {
                    UnitDefinition unit = Make(spec.Kind);
                    Check(unit.Health > 0 && unit.Speed > 0 && unit.Weapon.Damage > 0 && unit.WaitingKind == spec.Kind && unit.SpecialEnemy == SpecialEnemyKind.None, "Stats measured from neighbouring enemies " + spec.Id + " hp " + unit.Health + " speed " + unit.Speed + " dmg " + unit.Weapon.Damage);
                }
                SpareChair(catalog, Make(WaitingEnemyKind.SpareChair), Check);
                WakingSitter(catalog, Make(WaitingEnemyKind.WakingSitter), Check);
                InkNotary(catalog, Make(WaitingEnemyKind.InkNotary), Check);
                SewnLetter(catalog, Make(WaitingEnemyKind.SewnLetter), Check);
                PartyLine(catalog, Make(WaitingEnemyKind.PartyLine), Check);
                DoorKnocker(catalog, Make(WaitingEnemyKind.DoorKnocker), Check);
                LampSnuffer(catalog, Make(WaitingEnemyKind.LampSnuffer), Check);
                LedgerHound(catalog, Make(WaitingEnemyKind.LedgerHound), Check);
                SwarmAndCaps(catalog, Make(WaitingEnemyKind.SpareChair), Check);
            }
            finally { foreach (UnityEngine.Object item in made) if (item != null) UnityEngine.Object.DestroyImmediate(item); }
            Directory.CreateDirectory("PreviewExports/EnemyRoster");
            File.WriteAllLines("PreviewExports/EnemyRoster/Checks.txt", notes);
            Debug.Log("ENEMY_ROSTER_CHECKS_PASSED " + notes.Count);
        }

        // ------------------------------------------------------------------ zones and the art gate
        private static void Zones(Action<bool, string> Check)
        {
            (WaitingEnemyKind kind, int from, int until)[] table =
            {
                (WaitingEnemyKind.SpareChair, 6, 10), (WaitingEnemyKind.SewnLetter, 6, 15), (WaitingEnemyKind.PartyLine, 8, 14),
                (WaitingEnemyKind.DoorKnocker, 11, 15), (WaitingEnemyKind.LampSnuffer, 9, 20), (WaitingEnemyKind.InkNotary, 16, 20), (WaitingEnemyKind.LedgerHound, 16, 20)
            };
            foreach (var row in table)
            {
                bool exact = true;
                for (int wave = 1; wave <= 20; wave++) exact &= WaitingEnemyRoster.InRegularWaves(row.kind, wave, false) == (wave >= row.from && wave <= row.until);
                Check(exact, "Zone waves " + row.kind + " " + row.from + "-" + row.until);
            }
            bool never = true;
            for (int wave = 1; wave <= 40; wave++) never &= !WaitingEnemyRoster.InRegularWaves(WaitingEnemyKind.WakingSitter, wave, false) && !WaitingEnemyRoster.InRegularWaves(WaitingEnemyKind.WakingSitter, wave, true);
            Check(never, "Waking Sitter never walks into an ordinary wave");
        }

        private static void ArtGate(RunCatalog catalog, WaitingEnemyRoster roster, Action<bool, string> Check)
        {
            foreach (EnemyRosterBuilder.Spec spec in EnemyRosterBuilder.Specs)
            {
                bool art = EnemyRosterBuilder.HasArt(spec.Id), listed = roster != null && roster.Enemies.Any(unit => unit != null && unit.Id == spec.Id);
                Check(listed == art, (art ? "Imported and listed " : "Waiting for art, kept out: ") + spec.Id);
                if (!listed) continue;
                UnitDefinition unit = roster.Enemies.First(item => item != null && item.Id == spec.Id);
                WaitingEnemyArt drawn = roster.Find(spec.Id);
                Check(unit.WaitingKind == spec.Kind && unit.Weapon != null && drawn != null && drawn.Normal == unit.AuthoredMotion && drawn.Evolved != null, "Unit, weapon and both forms registered " + spec.Id);
                foreach (AuthoredMotionSet motion in new[] { drawn.Normal, drawn.Evolved })
                {
                    Sprite[] poses = motion.Idle.Concat(motion.Walk).Concat(motion.Windup).Concat(motion.Release).Concat(motion.Recovery).Distinct().ToArray();
                    Check(poses.Length == 8 && poses.All(sprite => sprite != null && AssetDatabase.GetAssetPath(sprite) == EnemyRosterBuilder.ArtPath(spec.Id)), "Eight painted poses " + motion.name);
                }
            }
            for (int index = 0; index < WaitingEnemyRoster.RethemedIds.Length; index++)
            {
                string old = WaitingEnemyRoster.RethemedIds[index], id = WaitingEnemyRoster.RethemeArt[index];
                bool art = EnemyRosterBuilder.HasArt(id); WaitingEnemyArt entry = roster?.Find(old);
                UnitDefinition source = catalog.Enemies.First(unit => unit.Id == old);
                Check((entry != null) == art && source.WaitingKind == WaitingEnemyKind.None, (art ? "Re-theme ready " : "Re-theme waiting for art: ") + old + " -> " + id);
                Check(source.DisplayName != WaitingEnemyRoster.RethemeNames[index] && !AssetDatabase.GetAssetPath(source.Body).Contains("/Expansion/Enemies/"), "Source asset of " + old + " keeps its own look and name");
                if (entry == null) continue;
                foreach (int stage in new[] { 0, 1 })
                {
                    UnitDefinition dressed = UnityEngine.Object.Instantiate(source);
                    dressed.EvolutionStage = stage; float health = dressed.Health, speed = dressed.Speed;
                    WaitingEnemyRoster.ApplyArt(dressed, 12);
                    Check(dressed.DisplayName == WaitingEnemyRoster.RethemeNames[index] && dressed.AuthoredMotion == (stage == 0 ? entry.Normal : entry.Evolved) && AssetDatabase.GetAssetPath(dressed.Body) == EnemyRosterBuilder.ArtPath(id)
                        && dressed.Health == health && dressed.Speed == speed && dressed.Behavior == source.Behavior && dressed.EvolutionStage == stage, "Re-themed in runs, same behaviour and stats: " + old + " as " + WaitingEnemyRoster.RethemeNames[index] + " stage " + stage);
                    UnityEngine.Object.DestroyImmediate(dressed);
                }
            }
            Check(EnemyRosterBuilder.HasArt(EnemyRosterBuilder.StampId) == (roster != null && roster.IndebtedStamp != null), "Indebted stamp shown only once painted");
            Check(catalog.Enemies.All(unit => unit.WaitingKind == WaitingEnemyKind.None) && catalog.Enemies.Length == 28, "The catalog's 28 older species are untouched");
            for (int wave = 1; wave <= 20; wave++)
            {
                var session = new RunSession(catalog);
                try
                {
                    session.Start("LanternKnight", 7300 + wave); session.State.Wave = wave; session.State.Roster.Clear();
                    CheckpointCatalog arena = session.PrepareCombat();
                    foreach (UnitDefinition unit in arena.EnemyPool)
                    {
                        if (unit.WaitingKind != WaitingEnemyKind.None)
                            Check(EnemyRosterBuilder.HasArt(unit.Id) && WaitingEnemyRoster.InRegularWaves(unit.WaitingKind, wave, false), "Only painted enemies of this zone in wave " + wave + ": " + unit.Id);
                        int theme = Array.IndexOf(WaitingEnemyRoster.RethemedIds, unit.Id);
                        if (theme >= 0 && EnemyRosterBuilder.HasArt(WaitingEnemyRoster.RethemeArt[theme]))
                            Check(unit.DisplayName == WaitingEnemyRoster.RethemeNames[theme] && AssetDatabase.GetAssetPath(unit.Body) == EnemyRosterBuilder.ArtPath(WaitingEnemyRoster.RethemeArt[theme]), "New look in wave " + wave + ": " + unit.Id);
                        if (theme >= 0 && !EnemyRosterBuilder.HasArt(WaitingEnemyRoster.RethemeArt[theme]))
                            Check(unit.DisplayName != WaitingEnemyRoster.RethemeNames[theme] && (unit.Body == null || !AssetDatabase.GetAssetPath(unit.Body).Contains("/Expansion/Enemies/")), "Old look in wave " + wave + ": " + unit.Id);
                    }
                    bool none = !arena.EnemyPool.Any(unit => unit.WaitingKind != WaitingEnemyKind.None && !EnemyRosterBuilder.HasArt(unit.Id));
                    Check(none, "Wave " + wave + " spawns nothing that waits for art (" + arena.EnemyPool.Length + " in pool)");
                    foreach (EnemyRosterBuilder.Spec spec in EnemyRosterBuilder.Specs)
                        if (EnemyRosterBuilder.HasArt(spec.Id) && WaitingEnemyRoster.InRegularWaves(spec.Kind, wave, false))
                            Check(arena.EnemyPool.Any(unit => unit.Id == spec.Id), "Painted " + spec.Id + " joins wave " + wave);
                }
                finally { session.Dispose(); }
            }
            var sitCloser = new RunSession(catalog);
            try
            {
                sitCloser.Start("LanternKnight", 7399); sitCloser.State.Wave = 10; sitCloser.State.Roster.Clear();
                sitCloser.State.ActOne ??= new ActOneProgress();
                sitCloser.State.ActOne.SitCloser = ActOneProgress.Fighting; sitCloser.State.ActOne.SatWave = 10;
                CheckpointCatalog arena = sitCloser.PrepareCombat();
                bool sitters = arena.EnemyPool.Any(unit => unit.WaitingKind == WaitingEnemyKind.WakingSitter);
                Check(arena.KnockDeadline > 0 && sitters == EnemyRosterBuilder.HasArt("WakingSitter"), "SIT CLOSER wakes the seated travellers (Waking Sitters) once painted");
            }
            finally { sitCloser.Dispose(); }
        }

        // ------------------------------------------------------------------ the eight moves, in the simulation
        private static void SpareChair(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using (var f = new Fixture(catalog, 8))
            {
                Vector2 hero = f.Hero.Position;
                UnitState chair = f.Waiting(definition, hero + Vector2.left * 2.2f);
                f.Step(1);
                Check(chair.Waiting.Step == 1 && chair.EvolutionReleaseAt > f.Sim.Elapsed && f.Sim.WaitingMarks.Any(m => m.Kind == WaitingMarkKind.SeatRing && m.Source == chair), "Spare Chair inks a seat ring under the hero and winds up");
                float release = chair.Waiting.ReleaseAt; Check(Mathf.Abs(release - f.Sim.Elapsed - CombatSimulation.SeatTelegraph) < Tick + .001f, "Seat ring warns 1.0 s before it snaps");
                bool early = false;
                while (f.Sim.Elapsed + Tick < release - .001f) { f.Step(1); early |= f.Sim.HeroRootedUntil > f.Sim.Elapsed; }
                for (int guard = 0; guard < 3 && f.Sim.HeroRootedUntil <= f.Sim.Elapsed; guard++) f.Step(1);
                Check(!early && Mathf.Abs(f.Sim.Elapsed - release) < Tick + .001f, "The hold comes only when the ring snaps");
                Check(f.Sim.HeroRootedUntil > f.Sim.Elapsed && Mathf.Abs(f.Sim.HeroRootedUntil - f.Sim.Elapsed - CombatSimulation.SeatRoot) < .001f, "Still inside when it snaps: rooted 0.6 s");
                Vector2 held = f.Hero.Position; f.Step(6, Vector2.right);
                Check((f.Hero.Position - held).magnitude < .001f, "A rooted hero cannot walk");
                f.Step(8, Vector2.right);
                Check((f.Hero.Position - held).magnitude > .2f, "The hold ends after 0.6 s");
            }
            using (var f = new Fixture(catalog, 8))
            {
                Vector2 hero = f.Hero.Position;
                UnitState chair = f.Waiting(definition, hero + Vector2.left * 2.2f);
                f.Step(1);
                f.Hero.Position = f.Hero.PreviousPosition = chair.Waiting.Aim + new Vector2(0, chair.Waiting.Radius + .5f);
                f.Step(Mathf.CeilToInt(CombatSimulation.SeatTelegraph / Tick) + 2);
                Check(chair.Waiting.Step == 0 && f.Sim.HeroRootedUntil <= f.Sim.Elapsed, "Stepping out of the ring before it snaps escapes the hold");
            }
        }

        private static void WakingSitter(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 12);
            UnitState sitter = f.Waiting(definition, f.Hero.Position + Vector2.left * 3, float.PositiveInfinity);
            float free = f.Stride(Vector2.down);
            sitter.Waiting.NextAction = 0;
            f.Step(1);
            Check(sitter.Waiting.Step == 1 && !sitter.Waiting.Tethered, "Waking Sitter winds up its throw first");
            f.Step(18);
            Check(sitter.Waiting.Tethered && f.Sim.HeroTethered && f.Sim.WaitingMarks.Any(m => m.Kind == WaitingMarkKind.Thread), "The ink thread lands on the hero");
            float held = f.Stride(Vector2.down);
            Check(Mathf.Abs(held / free - CombatSimulation.TetherSlow) < .03f, "Taut thread slows the hero 30% (" + (held / free).ToString("0.00") + ")");
            float health = f.Hero.Health; f.Step(40);
            float lost = health - f.Hero.Health;
            Check(lost > 0 && lost <= definition.Weapon.Damage * .3f * 2 + .01f, "Thread damage is low (" + lost.ToString("0.0") + " in 2 s)");
            f.Hero.Position = f.Hero.PreviousPosition = sitter.Position + Vector2.right * 6.2f;
            f.Step(1);
            Check(!sitter.Waiting.Tethered && !f.Sim.HeroTethered, "Beyond 5 units the thread breaks");
            float loose = f.Stride(Vector2.down);
            Check(Mathf.Abs(loose / free - 1) < .03f, "Full speed once the thread is broken");
        }

        private static void InkNotary(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 17);
            float free = f.Stride(Vector2.down);
            UnitState stamped = f.Enemy(f.Arena.MeleeEnemy, f.Hero.Position + Vector2.left * 3);
            UnitState plain = f.Enemy(f.Arena.MeleeEnemy, f.Hero.Position + new Vector2(2, 3.5f));
            stamped.Health = plain.Health = 100000;
            UnitState notary = f.Waiting(definition, stamped.Position + Vector2.left);
            f.Step(1);
            Check(notary.Waiting.Step == 1 && notary.Waiting.TargetId == stamped.Id, "Ink Notary picks the enemy beside it and raises its stamp");
            f.Step(15);
            notary.Waiting.NextAction = float.PositiveInfinity;
            Check(stamped.Waiting != null && stamped.Waiting.Paid && (plain.Waiting == null || !plain.Waiting.Paid), "PAID lands on that ally only");
            float before = stamped.Health; f.Sim.WeaponDamageFixture(stamped, 100, DamageElement.Physical); float halved = before - stamped.Health;
            before = plain.Health; f.Sim.WeaponDamageFixture(plain, 100, DamageElement.Physical); float full = before - plain.Health;
            Check(full > 0 && Mathf.Abs(halved - full * CombatSimulation.PaidShare) < full * .01f + .01f && !stamped.Waiting.Paid, "PAID halves the next hit taken (" + halved.ToString("0.0") + " of " + full.ToString("0.0") + "), once");
            before = stamped.Health; f.Sim.WeaponDamageFixture(stamped, 100, DamageElement.Physical);
            Check(Mathf.Abs(before - stamped.Health - full) < full * .01f + .01f, "The hit after that is whole again");
            f.Step(12); // its recovery after stamping
            notary.Waiting.NextAction = 0; f.Step(1);
            WaitingMark patch = f.Sim.WaitingMarks.LastOrDefault(m => m.Kind == WaitingMarkKind.ClosedPatch);
            Check(notary.Waiting.Step == 2 && patch != null && (patch.A - f.Hero.Position).magnitude < .01f && f.Sim.HeroClosedUntil <= f.Sim.Elapsed, "CLOSED is stamped where the hero stands, after a wind-up");
            f.Step(16);
            notary.Waiting.NextAction = float.PositiveInfinity;
            Check(f.Sim.HeroClosedUntil > f.Sim.Elapsed && Mathf.Abs(f.Sim.HeroClosedUntil - f.Sim.Elapsed - CombatSimulation.ClosedSeconds) < .001f, "Standing in CLOSED ink: slowed for 3 s");
            float slowed = f.Stride(Vector2.down);
            Check(Mathf.Abs(slowed / free - CombatSimulation.ClosedSlow) < .03f, "CLOSED slows the hero 25% (" + (slowed / free).ToString("0.00") + ")");
            f.Hero.Position = f.Hero.PreviousPosition = patch.A + Vector2.right * 4; f.Step(1);
            float left = f.Sim.HeroClosedUntil - f.Sim.Elapsed;
            f.Step(Mathf.CeilToInt(left / Tick) + 1);
            Check(f.Sim.HeroClosedUntil <= f.Sim.Elapsed && Mathf.Abs(f.Stride(Vector2.down) / free - 1) < .03f, "Off the ink the slow wears off");
        }

        private static void SewnLetter(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 9);
            UnitState letter = f.Waiting(definition, f.Hero.Position + Vector2.left * 3);
            float whole = letter.Definition.Health;
            f.Sim.WeaponDamageFixture(letter, whole * 10, DamageElement.Physical);
            f.Step(1);
            UnitState[] halves = f.Sim.Units.Where(u => u.Alive && u.Definition.WaitingKind == WaitingEnemyKind.LetterHalf).ToArray();
            Check(!letter.Alive && halves.Length == 2 && halves.All(h => Mathf.Abs(h.Definition.Health - whole * CombatSimulation.HalfShare) < .01f && Mathf.Abs(h.Health - h.Definition.Health) < .01f), "Sewn Letter bursts into 2 halves of 35% HP");
            foreach (UnitState half in halves) f.Sim.WeaponDamageFixture(half, whole * 10, DamageElement.Physical);
            f.Step(1);
            UnitState[] stamps = f.Sim.Units.Where(u => u.Alive && u.Definition.WaitingKind == WaitingEnemyKind.LetterStamp).ToArray();
            Check(stamps.Length == 4 && stamps.All(s => Mathf.Abs(s.Definition.Health - whole * CombatSimulation.StampShare) < .01f), "Each half bursts into 2 stamps of 12% HP (4 in all)");
            foreach (UnitState stamp in stamps) f.Sim.WeaponDamageFixture(stamp, whole * 10, DamageElement.Physical);
            f.Step(2);
            Check(!f.Sim.Units.Any(u => u.Alive && u.Faction == Faction.Enemy && u.Definition.WaitingKind != WaitingEnemyKind.None), "Stamps do not split");
        }

        private static void PartyLine(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 10);
            UnitState phone = f.Waiting(definition, f.Hero.Position + Vector2.left * 4);
            Vector2 stand = phone.Position;
            f.Step(1);
            WaitingMark preview = f.Sim.WaitingMarks.FirstOrDefault(m => m.Kind == WaitingMarkKind.CordPreview && m.Source == phone);
            Check(phone.Waiting.Step == 1 && preview != null && !f.Sim.WaitingMarks.Any(m => m.Kind == WaitingMarkKind.Cord), "Party Line rings first; the cord's path is only dotted");
            Check(Mathf.Abs(phone.Waiting.ReleaseAt - f.Sim.Elapsed - CombatSimulation.CordRing) < Tick + .001f, "It rings 0.9 s");
            Vector2 axis = preview.B - preview.A;
            float t = Mathf.Clamp01(Vector2.Dot(f.Hero.Position - preview.A, axis) / axis.sqrMagnitude);
            f.Hero.Position = f.Hero.PreviousPosition = preview.A + axis * t;
            Check((f.Hero.Position - stand).magnitude > 2 && (preview.B - preview.A).magnitude > (f.Hero.Position - stand).magnitude, "The cord runs across the floor past the hero");
            float health = f.Hero.Health; float release = phone.Waiting.ReleaseAt;
            while (f.Sim.Elapsed + Tick < release - .001f) f.Step(1);
            Check(f.Hero.Health == health, "No damage from the cord while it only rings");
            f.Step(14);
            WaitingMark cord = f.Sim.WaitingMarks.FirstOrDefault(m => m.Kind == WaitingMarkKind.Cord && m.Source == phone);
            Check(cord != null && Mathf.Abs(cord.Until - cord.At - CombatSimulation.CordSeconds) < .001f && f.Hero.Health < health, "After the ring the pinned cord hurts, held 3 s");
            f.Step(Mathf.CeilToInt(2f / Tick));
            Check(f.Sim.WaitingMarks.Contains(cord), "The cord is still pinned before 3 s");
            f.Step(Mathf.CeilToInt(.6f / Tick));
            Check(!f.Sim.WaitingMarks.Contains(cord) && (phone.Position - stand).magnitude < .001f, "Cord gone after 3 s; the telephone never moved");
        }

        private static void DoorKnocker(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 12);
            UnitState knocker = f.Waiting(definition, f.Hero.Position + Vector2.left * 3.5f);
            f.Step(1);
            Check(knocker.Waiting.Step == 1, "Door Knocker leaps in");
            while (knocker.Waiting.Step == 1 && f.Sim.Elapsed < 3) f.Step(1);
            float landed = knocker.Waiting.StepAt;
            Check(knocker.Waiting.Step == 2 && (knocker.Position - f.Hero.Position).magnitude < 2.2f && f.Sim.WaitingMarks.Count(m => m.Kind == WaitingMarkKind.KnockRing && m.Source == knocker) == 3, "It lands beside the hero; three knock rings are marked");
            var waves = new Dictionary<float, float>();
            for (int tick = 0; tick < 60; tick++)
            {
                f.Step(1);
                foreach (ExpandedField field in f.Sim.ExpandedFields) if (field.Source == knocker && field.Kind == ExpandedFieldKind.Shockwave) waves[field.Created] = field.Radius;
            }
            float[] at = knocker.Waiting.KnockAt;
            Check(at.All(time => time > 0) && Mathf.Abs(at[0] - landed - CombatSimulation.KnockBeat) < Tick + .001f && Mathf.Abs(at[1] - at[0] - CombatSimulation.KnockBeat) < Tick + .001f && Mathf.Abs(at[2] - at[1] - CombatSimulation.KnockBeat) < Tick + .001f, "Three knocks 0.7 s apart");
            float[] radii = waves.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray();
            Check(radii.Length == 3 && radii[2] > radii[0] * 1.3f && Mathf.Abs(radii[0] - radii[1]) < .001f, "Each knock is a spreading wave; the third is biggest");
        }

        private static void LampSnuffer(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 12);
            Vector2 hero = f.Hero.Position;
            f.Sim.ConfigureWaitingLanternFixture(new[] { hero + new Vector2(-4, 2), hero + new Vector2(5, 3) });
            UnitState snuffer = f.Waiting(definition, hero + new Vector2(-3, 1));
            f.Step(Mathf.CeilToInt(6 / Tick));
            Check(snuffer.Waiting.Lantern == 0 && snuffer.Waiting.Step == 1 && f.Sim.LanternDimmed(0) && !f.Sim.LanternDimmed(1), "Lamp Snuffer drifts to the nearest lit lantern and dims it (" + f.Sim.LanternDimming(0).ToString("0.00") + ")");
            float dim = f.Sim.LanternDimming(0); f.Step(20);
            Check(f.Sim.LanternDimming(0) > dim || dim >= 1, "It keeps dimming while alive");
            f.Sim.WeaponDamageFixture(snuffer, snuffer.Health * 10, DamageElement.Physical);
            f.Step(Mathf.CeilToInt(3 / Tick));
            Check(!f.Sim.LanternDimmed(0), "Defeated, the lantern brightens again");
        }

        private static void LedgerHound(RunCatalog catalog, UnitDefinition definition, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 17);
            UnitState hound = f.Waiting(definition, f.Hero.Position + Vector2.left * 2.6f);
            float health = f.Hero.Health;
            f.Step(1);
            Check(hound.Waiting.Step == 1 && hound.Waiting.Count <= 1, "Ledger Hound starts circling the hero");
            var laid = new List<float>(); if (hound.Waiting.Count == 1) laid.Add(f.Sim.Elapsed);
            int count = hound.Waiting.Count; bool earlyHit = false;
            while (hound.Waiting.Slashes == 0 && f.Sim.Elapsed < 8)
            {
                f.Step(1);
                if (hound.Waiting.Step == 1 && hound.Waiting.Count > count) { count = hound.Waiting.Count; laid.Add(f.Sim.Elapsed); }
                if (hound.Waiting.Slashes == 0) earlyHit |= f.Hero.Health < health;
            }
            Check(laid.Count == 4 && Enumerable.Range(1, 3).All(i => Mathf.Abs(laid[i] - laid[i - 1] - CombatSimulation.TallyBeat) < Tick + .001f), "Four tally marks, one every 0.8 s");
            Check(!earlyHit && f.Sim.WaitingMarks.Count(m => m.Kind == WaitingMarkKind.Tally && m.Source == hound) == 4, "The four marks are harmless ink");
            WaitingMark slash = f.Sim.WaitingMarks.FirstOrDefault(m => m.Kind == WaitingMarkKind.Slash && m.Source == hound);
            Vector2 first = hound.Waiting.Tallies[0], last = hound.Waiting.Tallies[3];
            Check(slash != null && hound.Waiting.Slashes == 1 && f.Hero.Health < health && slash.Hit, "The fifth stroke is a slash that strikes");
            Vector2 along = (slash.B - slash.A).normalized;
            Check(Mathf.Abs(Vector2.Dot((first - slash.A).normalized, new Vector2(-along.y, along.x))) < .01f && Mathf.Abs(Vector2.Dot((last - slash.A).normalized, new Vector2(-along.y, along.x))) < .01f, "The slash runs along the marked line");
        }

        private static void SwarmAndCaps(RunCatalog catalog, UnitDefinition chair, Action<bool, string> Check)
        {
            using var f = new Fixture(catalog, 8);
            f.Arena.EnemyPool = new[] { chair, f.Arena.MeleeEnemy };
            bool clean = true;
            for (int index = 0; index < 24; index++) clean &= Invoke(f.Sim, "HerdDefinition") as UnitDefinition != chair && Invoke(f.Sim, "SwarmDefinition") as UnitDefinition != chair;
            Check(clean, "New enemies never fill in as swarm bodies");
            f.Enemy(chair, f.Hero.Position + new Vector2(-6, 3)); f.Enemy(chair, f.Hero.Position + new Vector2(-6, -3));
            Check(!(bool)Invoke(f.Sim, "WaitingSpawnBlocked", chair), "Two chairs: a third may come");
            f.Enemy(chair, f.Hero.Position + new Vector2(-7, 0));
            Check((bool)Invoke(f.Sim, "WaitingSpawnBlocked", chair), "Three chairs at once at most");
            Check(!(bool)Invoke(f.Sim, "WaitingSpawnBlocked", f.Arena.MeleeEnemy), "Older species are never held back");
        }

        private sealed class Fixture : IDisposable
        {
            private readonly RunSession session;
            public readonly CheckpointCatalog Arena;
            public readonly CombatSimulation Sim;
            public UnitState Hero => Sim.Champion;
            public Fixture(RunCatalog catalog, int wave)
            {
                session = new RunSession(catalog); session.Start("LanternKnight", 191026); session.State.Wave = wave; session.State.Roster.Clear();
                Arena = session.PrepareCombat(); Arena.Boss = null; Arena.EnemyBudget = 24;
                Arena.StoryCompanions = new UnitDefinition[0];
                Arena.EnableArenaToys = Arena.EnableBossArenaPhase = Arena.EnableSpectacles = Arena.EnableHeroMechanics = false;
                Arena.PassiveHeal = 0; Arena.Dodge = 0;
                Sim = new CombatSimulation(Arena); Sim.Start(191026);
                typeof(CombatSimulation).GetField("nextSpawn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Sim, float.PositiveInfinity);
                Sim.Champion.NextAttack = float.PositiveInfinity;
                Sim.Champion.Position = Sim.Champion.PreviousPosition = new Vector2(4.5f, 0);
                Sim.Champion.Health = Sim.Champion.Definition.Health;
            }
            public UnitState Enemy(UnitDefinition definition, Vector2 position)
            {
                UnitState enemy = Sim.AddFixture(definition, Faction.Enemy, position);
                enemy.NextAttack = enemy.NextContact = enemy.NextBehavior = float.PositiveInfinity;
                enemy.Awakened = true;
                if (enemy.Variant != null) enemy.Variant.NextAffix = enemy.Variant.NextSpecial = float.PositiveInfinity;
                return enemy;
            }
            public UnitState Waiting(UnitDefinition definition, Vector2 position, float next = 0)
            {
                UnitState enemy = Enemy(definition, position);
                enemy.Waiting = new WaitingEnemyState { Kind = definition.WaitingKind, NextAction = next };
                return enemy;
            }
            public void Step(int ticks, Vector2 movement = default) { for (int i = 0; i < ticks; i++) Sim.Step(Tick, movement); }
            /// <summary>How far the hero walks in one tick (then puts the hero back).</summary>
            public float Stride(Vector2 direction)
            {
                Vector2 start = Hero.Position; Sim.Step(Tick, direction);
                float stride = (Hero.Position - start).magnitude;
                Hero.Position = Hero.PreviousPosition = start;
                return stride;
            }
            public void Dispose() => session.Dispose();
        }
    }
}
