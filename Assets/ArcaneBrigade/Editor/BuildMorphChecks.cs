using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class BuildMorphChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool value, string label)
        {
            notes.Add((value ? "PASS " : "FAIL ") + label);
            Directory.CreateDirectory("PreviewExports/BuildMorphs"); File.WriteAllLines("PreviewExports/BuildMorphs/Focused_Checks.txt", notes);
            if (!value) throw new InvalidOperationException("BUILD_MORPH " + label);
        }
        private static object Invoke(object target, string method, params object[] arguments) => target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, arguments);
        public static void Run()
        {
            notes.Clear(); RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            Check(c != null && c.ExpansionRevision > 0, "Real run catalog enables forms");
            BuildMorphArtwork art = Resources.Load<BuildMorphArtwork>("BuildMorphArtwork");
            Check(art != null && art.Parts.Length == 24 && art.Parts.All(s => s != null) && art.Parts.Select(s => s.GetInstanceID()).Distinct().Count() == 24, "24 individually imported transparent attachments");
            var shapes = new HashSet<string>();
            foreach (BuildMorphDefinition d in BuildMorphs.All)
            {
                WeaponDefinition w = c.Weapons.FirstOrDefault(a => !a.SpecialOnly && BuildMorphs.For(a) == d.Kind);
                Check(w != null, "Recruitable weapon exists: " + d.Name);
                Check(ArcaneCueLibrary.Get(Cue(d.Kind)) != null, "Existing original Toybox sound: " + d.Name);
                CheckCraft(c, w, d.Kind);
                for (int stage = 1; stage <= 3; stage++)
                using (var f = new Fixture(c, w, d.Kind, stage))
                {
                    CombatSimulation s = f.Sim; UnitState source = s.Champion;
                    float first = s.MorphAttackFixture(source), second = s.MorphAttackFixture(source), third = s.MorphAttackFixture(source);
                    Check(first == 30 && second == 30 && Mathf.Abs(third - 21.6f) < .001f && s.BuildMorphReleases == 1, "Third attack redistributes one hit: " + d.Kind + "/" + stage);
                    MorphStrike[] strikes = s.MorphStrikes.ToArray();
                    Check(strikes.Length >= 1 && strikes.Length <= 8 && strikes.All(a => a.Cause.Depth == 1 && a.Cause.SourceUnitId == source.Id && a.MaxTargets == 2 + stage), "Bounded pattern with source attribution: " + d.Kind + "/" + stage);
                    float budget = strikes.Sum(a => a.Budget);
                    Check(Mathf.Abs(budget - 30 * (.6f + stage * .2f)) < .001f, "One conserved pattern budget: " + d.Kind + "/" + stage);
                    if (stage == 3) shapes.Add(string.Join(";", strikes.Select(a => a.Geometry + ":" + a.Position.ToString("F2") + ":" + a.End.ToString("F2") + ":" + a.At.ToString("F2"))));
                    float at = s.Elapsed, health = f.Targets.Sum(a => a.Health);
                    s.Pause(true); s.Step(.1f, Vector2.zero);
                    Check(s.Elapsed == at && !strikes.Any(a => a.Resolved), "Pause preserves staged attack: " + d.Kind + "/" + stage); s.Pause(false);
                    for (int tick = 0; tick < 24; tick++) s.Step(.05f, Vector2.zero);
                    float dealt = health - f.Targets.Sum(a => a.Health);
                    Check(dealt > 0 && dealt <= budget + .04f, "Real damage stays inside total budget: " + d.Kind + "/" + stage + " dealt=" + dealt + " budget=" + budget);
                    Check(s.MorphStrikes.Count == 0 && s.BuildMorphReleases == 1, "Pattern expires without recursive releases: " + d.Kind + "/" + stage);
                    Check(s.Contributions.Any(a => a.EffectId == "build-" + d.Kind.ToString().ToLowerInvariant() && a.Damage > 0 && a.Activations == 1), "Measured contribution has correct owner: " + d.Kind + "/" + stage);
                    int releases = s.BuildMorphReleases;
                    for (int i = 0; i < 10; i++) s.MorphAttackFixture(source);
                    Check(s.BuildMorphReleases == releases, "Time cooldown survives fast attack builds: " + d.Kind + "/" + stage);
                    s.EnterRest(); Check(s.MorphStrikes.Count == 0 && source.MorphAttacks == 0 && s.BuildMorphReleases == 0, "Rest clears pending form state: " + d.Kind + "/" + stage);
                }
            }
            Check(shapes.Count == 12, "All twelve masterwork patterns have different geometry and timing");
            CheckGeometry(c); CheckGuards(c); CheckLiveAttacks(c);
            Debug.Log("BUILD_MORPH_CHECKS " + notes.Count + " PASS");
        }
        private static string Cue(BuildMorphKind kind) => kind switch {
            BuildMorphKind.Thread => "fx.field.zipper", BuildMorphKind.Cinder => "wpn.cast.fire", BuildMorphKind.Clockwork => "sig.hammer", BuildMorphKind.Porcelain => "sig.tea", BuildMorphKind.Constellation => "fx.reaction.arcane", BuildMorphKind.Paper => "sig.umbrella", BuildMorphKind.Mirror => "sig.camera", BuildMorphKind.Chorus => "sig.musicbox", BuildMorphKind.Magnet => "wpn.magnet.pull", BuildMorphKind.Tide => "wpn.boomerang.return", BuildMorphKind.Frost => "wpn.cast.ice", _ => "sig.stamp" };
        private static void CheckCraft(RunCatalog c, WeaponDefinition w, BuildMorphKind kind)
        {
            using var session = new RunSession(c); session.Start("LanternKnight", 8015); session.State.Roster.Clear(); session.State.Items.Clear();
            BuildMorphProfile empty = session.MorphProfile(w, 3, true);
            Check(empty.Stage == 0 && empty.Relics == 0, "Tier alone cannot unlock a form: " + kind);
            ItemDefinition[] items = c.Items.Where(a => !a.Id.StartsWith("Secret") && BuildMorphs.Affinity(kind, a, true)).Take(6).ToArray();
            Check(items.Length >= 4, "At least four compatible relic identities for evolved carriers: " + kind);
            foreach (ItemDefinition item in items.Take(2)) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
            BuildMorphProfile p = session.MorphProfile(w, 1, true);
            Check(p.Relics == 2 && p.Stage >= 1, "Compatible relic identities unlock first form: " + kind);
            session.State.Items[0].Count = 99;
            Check(session.MorphProfile(w, 1, true).Points == p.Points, "Duplicate stacks cannot farm craft: " + kind);
            session.State.Items[0].Count = 1;
            session.State.Secrets.Stitches.Add(new SecretStitch { ItemId = items[0].Id, Pattern = 0 });
            Check(session.MorphProfile(w, 1, true).Relics == 1, "Altered secret item loses old craft identity: " + kind); session.State.Secrets.Stitches.Clear();
            foreach (ItemDefinition item in items.Skip(2)) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
            session.State.Roster.Add(new RosterEntry { UnitId = c.Companions.First(a => !a.Id.StartsWith("Secret")).Id, WeaponId = w.Id, Tier = 3 });
            string before = JsonUtility.ToJson(session.State);
            CheckpointCatalog compiled = session.PrepareCombat();
            Check(compiled.StartingCompanions[0].BuildMorphStage == 3 && compiled.StartingCompanions[0].BuildMorph == kind, "Real compiler applies masterwork to the assigned weapon: " + kind);
            Check(w.Damage == c.Weapon(w.Id).Damage && c.Companions[0].BuildMorphStage == 0, "Source definitions remain unchanged: " + kind);
            using var restored = new RunSession(c); restored.Restore(JsonUtility.FromJson<RunSnapshot>(before));
            CheckpointCatalog after = restored.PrepareCombat();
            Check(after.StartingCompanions[0].BuildMorphStage == 3 && after.StartingCompanions[0].BuildMorph == kind, "Save restores derived form without a new schema: " + kind);
            RunBuildReadModel model = RunBuildReadModel.Capture(restored, c, 100);
            Check(model.Links.Count(a => a.Name.StartsWith("Form / ")) == 12 && model.Companions[0].Explanation.Contains("BUILD FORM"), "Details expose all forms and this carrier's sources: " + kind);
        }
        private static void CheckGeometry(RunCatalog c)
        {
            using var f = new Fixture(c, c.Weapons.First(), BuildMorphKind.Frost, 3);
            UnitState enemy = f.Targets[0]; enemy.Position = new Vector2(0, 1);
            var strike = new MorphStrike { Geometry = MorphGeometry.Line, Position = Vector2.zero, End = Vector2.right * 3, Width = .1f };
            Check(!CombatSimulation.MorphContains(strike, enemy), "A line does not damage outside its visible lane");
            enemy.Position = Vector2.right; Check(CombatSimulation.MorphContains(strike, enemy), "A line catches a target on the lane");
            strike.Geometry = MorphGeometry.Ring; strike.Radius = 2; strike.Width = .2f; enemy.Position = Vector2.zero;
            Check(!CombatSimulation.MorphContains(strike, enemy), "Ring centre stays empty"); enemy.Position = Vector2.up * 2;
            Check(CombatSimulation.MorphContains(strike, enemy), "Ring border catches targets");
            strike.Geometry = MorphGeometry.Crescent; strike.Direction = Vector2.right; strike.Cone = .15f; enemy.Position = Vector2.left * 2;
            Check(!CombatSimulation.MorphContains(strike, enemy), "Crescent cannot hit behind its fan");
            strike.Geometry = MorphGeometry.Seal; strike.Radius = 1; enemy.Position = new Vector2(.8f, .8f);
            Check(CombatSimulation.MorphContains(strike, enemy), "Rectangular seal includes its corners");
        }
        private static void CheckGuards(RunCatalog c)
        {
            using var f = new Fixture(c, c.Weapons.First(), BuildMorphKind.Magnet, 3);
            var s = f.Sim; UnitState source = s.Champion, enemy = f.Targets[0];
            enemy.Definition.IsBoss = true; enemy.Definition.KnockbackResistance = 0;
            Vector2 bossPosition = enemy.Position;
            s.MorphAttackFixture(source); s.MorphAttackFixture(source); s.MorphAttackFixture(source);
            for (int i = 0; i < 20; i++) s.Step(.05f, Vector2.zero);
            Check(enemy.Position == bossPosition && enemy.ChillUntil == 0, "Form pulls and slows cannot move or immobilize bosses");
            s.Start(8015); Check(s.MorphStrikes.Count == 0 && s.BuildMorphReleases == 0, "New wave cannot leak previous pattern state");
            source = s.Champion; source.Definition.BuildMorphStage = 0;
            for (int i = 0; i < 10; i++) s.MorphAttackFixture(source);
            Check(s.BuildMorphReleases == 0, "Dormant builds retain ordinary combat");
        }
        private static void CheckLiveAttacks(RunCatalog c)
        {
            WeaponDefinition w = c.Weapons.First(a => BuildMorphs.For(a) == BuildMorphKind.Constellation);
            using var f = new Fixture(c, w, BuildMorphKind.Constellation, 3);
            f.Sim.Champion.Definition.Weapon.Cooldown = .2f; f.Sim.Champion.Definition.Weapon.Windup = .01f; f.Sim.Champion.Definition.Weapon.Range = 10;
            f.Sim.Champion.NextAttack = 0;
            for (int i = 0; i < 50; i++) f.Sim.Step(.05f, Vector2.zero);
            Check(f.Sim.BuildMorphReleases == 1, "Normal auto-attack path releases a form with no fixture trigger or extra input");
            Check(f.Sim.Contributions.Any(a => a.EffectId == "build-constellation" && a.Damage > 0), "Normal auto combat settles form damage in its ledger");
        }
        private sealed class Fixture : IDisposable
        {
            private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
            public CombatSimulation Sim;
            public readonly List<UnitState> Targets = new List<UnitState>();
            private T Copy<T>(T value) where T : UnityEngine.Object { T copy = UnityEngine.Object.Instantiate(value); owned.Add(copy); return copy; }
            public Fixture(RunCatalog c, WeaponDefinition weapon, BuildMorphKind kind, int stage)
            {
                CheckpointCatalog arena = Copy(c.Arena);
                arena.Champion = Copy(c.Champions[0]); arena.Champion.Weapon = Copy(weapon);
                arena.Champion.HeroMechanic = HeroMechanic.None; arena.Champion.Passive = PassiveKind.None; arena.Champion.CollisionDamage = 0;
                arena.Champion.BuildMorph = kind; arena.Champion.BuildMorphStage = stage;
                arena.Champion.Health = 10000; arena.Champion.CriticalChance = 0;
                arena.Boss = null; arena.StartingCompanions = new UnitDefinition[0]; arena.ExplicitStartingRoster = true;
                arena.Procs = new ActiveProc[0]; arena.RelicStacks = new int[0]; arena.RelicWeapons = new UnitDefinition[0];
                arena.EnableArenaToys = arena.EnableSpectacles = arena.EnableBossArenaPhase = arena.EnableHeroMechanics = arena.ExpandedContent = false;
                arena.EnemyBudget = 0; arena.RunWave = 2; arena.WaveDuration = 1000; arena.Armor = arena.PassiveHeal = 0;
                Sim = new CombatSimulation(arena); Sim.Start(8015); Sim.Champion.NextAttack = float.PositiveInfinity;
                for (int i = 0; i < 12; i++)
                {
                    UnitDefinition enemy = Copy(c.Enemies.First(a => a.Weapon.Kind == WeaponKind.Contact)); enemy.Weapon = Copy(enemy.Weapon);
                    enemy.IsBoss = false; enemy.Behavior = EnemyBehavior.Standard; enemy.Mutation = EnemyMutation.None; enemy.SpecialEnemy = SpecialEnemyKind.None;
                    enemy.Speed = enemy.CollisionDamage = enemy.ContactDamage = 0; enemy.Health = 10000; enemy.Radius = .22f; enemy.KnockbackResistance = 1;
                    enemy.BuildMorphStage = 0; enemy.BuildMorph = BuildMorphKind.None;
                    Vector2 at = i == 0 ? new Vector2(1.8f, 0) : new Vector2(1.8f, 0) + (Vector2)(Quaternion.Euler(0, 0, i * 32) * Vector2.right) * (.45f + i * .07f);
                    UnitState target = Sim.AddFixture(enemy, Faction.Enemy, at); target.NextAttack = target.NextContact = float.PositiveInfinity; Targets.Add(target);
                }
                Sim.Champion.AttackDirection = Vector2.right; Sim.Champion.AttackTargetId = Targets[0].Id;
            }
            public void Dispose() { foreach (UnityEngine.Object value in owned) UnityEngine.Object.DestroyImmediate(value); }
        }
    }
}
