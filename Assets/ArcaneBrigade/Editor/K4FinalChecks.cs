using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K4FinalChecks
    {
        [MenuItem("Arcane Brigade/K4/Check full signature slice and build")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }
        public static void Run()
        {
            K4SignatureChecks.Run(); K4MergeChecks.Run(); K4ArenaChecks.Run(); K4SandboxChecks.Run();
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException("K4 final: " + name); results.Add("PASS " + name); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            require(catalog.SignatureRevision == 1, "Full signature revision enabled");
            string content = JsonUtility.ToJson(catalog); K4FinalBuilder.Apply();
            require(content == JsonUtility.ToJson(catalog), "Full migration idempotent with authored tuning preserved");
            foreach (string id in new[] { "harvest-receipt", "reflective-ink", "carbon-ribbon", "office-lens" })
                require(catalog.Items.Count(item => item.Id == id) == 1 && catalog.Item(id).Icon != null && catalog.Item(id).SignatureRole != SignatureRelicKind.None, "Unique authored relic and HD icon " + id);
            require(catalog.Item("carbon-ribbon").EffectiveModifiers.Any(modifier => modifier.Value < 0) && catalog.Item("harvest-receipt").EffectiveModifiers.Any(modifier => modifier.Value < 0), "Connector and harvester have visible real trade-offs");
            var relicImporter = (TextureImporter)AssetImporter.GetAtPath("Assets/ArcaneBrigade/Art/QualityHD/SignatureRelics_v1.png");
            require(relicImporter.alphaIsTransparency && !relicImporter.isReadable && relicImporter.textureCompression == TextureImporterCompression.Uncompressed && catalog.Items.Where(item => item.SignatureRole != SignatureRelicKind.None && item.SignatureRole <= SignatureRelicKind.OfficeLens).All(item => item.Icon.rect.width > 200 && item.Icon.rect.height > 200), "HD relic imports preserve alpha and full readable icon bounds without CPU copy");
            using (var fixture = new Fixture(catalog, "BellFork", "harvest-receipt"))
            {
                require(SignatureBuildGraph.Status("signature-harvest", fixture.Session, catalog).Contains("ACTIVE"), "Two components activate harvest before completing set");
                for (int index = 0; index < 3; index++) fixture.Simulation.LootFixture(LootKind.Coin, Vector2.zero, 2);
                fixture.Tick(24);
                require(fixture.Simulation.Gold == 6 && fixture.Simulation.HarvestBeats == 1, "Coin credits once; same-tick collector bursts share finite cooldown");
                require(fixture.Source.AttackBeat == 1, "Receipt charges actual bell cadence, not cosmetic counter");
                fixture.Simulation.SignatureBeatFixture(fixture.Source); fixture.Simulation.SignatureBeatFixture(fixture.Source);
                UnitState enemy = fixture.Enemy(fixture.Source.Position + Vector2.right);
                fixture.Tick(15);
                require(enemy.Health < 1000 && fixture.Simulation.BellWaves == 1, "Harvest charge feeds delayed real wave");
                require(fixture.Simulation.Gold == 6 && fixture.Simulation.HarvestBeats == 1, "Harvest wave cannot farm coin or recurse pickup");
                require(fixture.Simulation.Contributions.Sum(entry => entry.Coins) == 6 && fixture.Simulation.Contributions.Any(entry => entry.EffectId == "harvest-receipt" && entry.Activations == 1), "Credit and collector activations are measured once");
            }
            using (var fixture = new Fixture(catalog, "DreamStamp", "reflective-ink", "office-lens"))
            {
                require(SignatureBuildGraph.Status("signature-office", fixture.Session, catalog).Contains("ACTIVE"), "Office graph reads actual weapon and owned relic roles");
                UnitState target = fixture.Enemy(fixture.Source.Position + Vector2.right);
                fixture.Simulation.StampFixture(fixture.Source, target);
                require(target.VisibleMarks == 2 && target.MarkVisibleUntil < fixture.Source.Definition.Weapon.MarkLifetime, "Ink advances marks but shortens actual expiry");
                fixture.Simulation.StampFixture(fixture.Source, target);
                require(fixture.Simulation.StampBursts == 1 && target.ExposedUntil > fixture.Simulation.Elapsed, "Ink threshold creates burst and real weakness window");
                fixture.Source.Definition.CriticalChance = 1; fixture.Source.PendingAttack = true; fixture.Source.AttackTargetId = target.Id;
                fixture.Source.AttackDirection = Vector2.right; fixture.Source.ImpactTime = fixture.Simulation.Elapsed + 0.01f;
                fixture.Tick(1);
                require(fixture.Simulation.Contributions.Any(entry => entry.EffectId == "office-lens" && entry.Damage > 0), "Actual critical hit consumes Lens damage benefit without second burst");
                require(Mathf.Abs(fixture.Simulation.Contributions.Sum(entry => entry.Damage) - (1000 - target.Health)) < 0.001f, "Primary, burst and lens bonus split equal actual enemy HP loss");
                require(fixture.Simulation.StampBursts == 1, "Critical bonus cannot recursively stamp or burst");
                string before = JsonUtility.ToJson(fixture.Session.State);
                RunBuildReadModel model = RunBuildReadModel.Capture(fixture.Session, catalog, fixture.Simulation.Champion.Health, fixture.Simulation);
                require(before == JsonUtility.ToJson(fixture.Session.State) && model.Links.Any(row => row.Name == "VĂN PHÒNG HOÀN TRẢ" && row.Explanation.Contains("HALL HIỆN TẠI")), "Live contribution graph is read-only and explicitly unsettled");
                require(model.Companions[0].Explanation.Contains("hit 2 dấu") && model.Companions[0].Explanation.Contains((fixture.Source.Definition.Weapon.MarkLifetime * 0.65f).ToString("0.##") + "s"), "Details uses effective Ink mark count and expiry rather than base tooltip");
            }
            using (var fixture = new Fixture(catalog, "DreamStamp", "office-lens"))
            {
                UnitState target = fixture.Enemy(fixture.Source.Position + Vector2.right);
                for (int index = 0; index < 3; index++) fixture.Simulation.StampFixture(fixture.Source, target);
                require(fixture.Simulation.Contributions.Any(entry => entry.EffectId == "office-lens-exposure") && !fixture.Simulation.Contributions.Any(entry => entry.EffectId == "reflective-ink"), "Lens-only exposure never credits unowned Ink");
            }
            using (var fixture = new Fixture(catalog, "BellFork", "harvest-receipt"))
            {
                fixture.Simulation.SignatureBeatFixture(fixture.Source); fixture.Simulation.SignatureBeatFixture(fixture.Source);
                fixture.Simulation.LootFixture(LootKind.Coin, Vector2.zero, 1);
                bool valid = false;
                for (int index = 0; index < 70; index++)
                {
                    fixture.Tick(1);
                    foreach (CombatEvent value in fixture.Simulation.Events)
                        if (value.Kind == CombatEventKind.SignaturePulse) valid = value.Cause.Depth == 1 && value.Cause.SourceUnitId == fixture.Source.Id;
                }
                require(valid, "Harvest-completed Bell cadence preserves depth-one payoff and real source");
            }
            using (var fixture = new Fixture(catalog, "DreamStamp", "carbon-ribbon"))
            {
                UnitState target = fixture.Enemy(fixture.Source.Position + Vector2.right);
                fixture.Simulation.LootFixture(LootKind.Coin, target.Position + Vector2.right * 1.5f, 2);
                fixture.Simulation.LootFixture(LootKind.Coin, new Vector2(12, 0), 2);
                for (int index = 0; index < 3; index++) fixture.Simulation.StampFixture(fixture.Source, target);
                require(fixture.Simulation.Pickups[0].Attracting && !fixture.Simulation.Pickups[1].Attracting, "Connector alone with Stamp pulls only eligible local coin");
                require(fixture.Simulation.Pickups.Count == 2 && fixture.Simulation.Gold == 0, "Connector does not invent or prematurely credit currency");
            }
            using (var fixture = new Fixture(catalog, "BellFork", "carbon-ribbon"))
            {
                UnitState target = fixture.Enemy(fixture.Source.Position + Vector2.right);
                for (int index = 0; index < 3; index++) fixture.Simulation.SignatureBeatFixture(fixture.Source);
                fixture.Tick(15);
                require(target.ExposedUntil > fixture.Simulation.Elapsed && fixture.Simulation.StampBursts == 0, "Carbon bell branch exposes but never fabricates a stamp burst");
            }
            using (var fixture = new Fixture(catalog, "DreamStamp"))
            {
                UnitState target = fixture.Enemy(new Vector2(5, 0)); target.Health = 3;
                fixture.Simulation.WeaponDamageFixture(target, 500, DamageElement.Physical);
                fixture.Simulation.WeaponDamageFixture(target, 500, DamageElement.Physical);
                require(fixture.Simulation.Contributions.Sum(entry => entry.Damage) == 3 && fixture.Simulation.Kills == 1, "Ledger excludes overkill and repeated death damage");
                fixture.Simulation.Champion.Health = fixture.Simulation.Champion.Definition.Health - 1;
                fixture.Simulation.LootFixture(LootKind.Heart, Vector2.zero, 20); fixture.Tick(45);
                require(Mathf.Abs(fixture.Simulation.Contributions.Sum(entry => entry.Healing) - 1) < 0.001f, "Ledger excludes overheal");
                fixture.Combat.WaveDuration = fixture.Simulation.Elapsed + 0.01f; fixture.Tick(1);
                fixture.Session.Settle(fixture.Simulation); string settled = JsonUtility.ToJson(fixture.Session.State);
                fixture.Session.Settle(fixture.Simulation);
                require(settled == JsonUtility.ToJson(fixture.Session.State) && fixture.Session.State.Contributions.Sum(entry => entry.Damage) == 3, "Contribution settles exactly once with normal phase guard");
                string root = Path.Combine(Application.temporaryCachePath, "ArcaneK4Final", Guid.NewGuid().ToString("N"));
                var storage = new RunStorage(root); storage.SaveRun(fixture.Session.State);
                using (var loaded = new RunSession(catalog))
                {
                    loaded.Restore(storage.LoadRun(loaded));
                    require(JsonUtility.ToJson(loaded.State) == settled, "Contribution rows persist in schema2 without RNG reroll");
                    RunSnapshot legacy = JsonUtility.FromJson<RunSnapshot>(settled); legacy.Contributions = null;
                    loaded.Restore(legacy); require(loaded.State.Contributions.Count == 0, "Older schema2 without ledger migrates read-only safely");
                }
                RunSnapshot invalid = JsonUtility.FromJson<RunSnapshot>(settled); invalid.Contributions[0].Damage = float.NaN;
                bool rejected = false; try { fixture.Session.Validate(invalid); } catch (InvalidOperationException) { rejected = true; }
                require(rejected, "Invalid contribution floats cannot enter save");
            }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 13104); session.State.Wave = 4;
                CheckpointCatalog combat = session.PrepareCombat();
                require(combat.EnableArenaToys && combat.EnableBossArenaPhase && combat.RunWave == 4 && combat.Boss != null, "Authored boss wave enables deterministic toys and arena phase");
                var simulation = new CombatSimulation(combat); simulation.Start(combat.Seed);
                require(simulation.ArenaToys.Count >= 2 && simulation.ArenaToys[0].Kind == ArenaToyKind.StageBell && simulation.ArenaToys[1].Kind == ArenaToyKind.ToyCart, "Ordinary Start automatically seeds original toy pair at later wave");
                simulation.EnterRest(); require(simulation.ArenaToys.Count == 0, "Rest removes toys and all pending arena warnings");
            }
            Directory.CreateDirectory("PreviewExports/K4/Final"); File.WriteAllLines("PreviewExports/K4/Final/Checks.txt", results);
            Debug.Log("ARCANE_K4_FINAL_CHECKS_PASSED " + results.Count);
        }

        private sealed class Fixture : IDisposable
        {
            public readonly RunSession Session;
            public readonly CombatSimulation Simulation;
            public readonly CheckpointCatalog Combat;
            public readonly UnitState Source;
            private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
            public Fixture(RunCatalog catalog, string weapon, params string[] items)
            {
                Session = new RunSession(catalog); Session.Start(catalog.Champions[0].Id, 13104);
                Session.State.Roster.Clear(); Session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[0].Id, WeaponId = weapon });
                foreach (string id in items) Session.State.Items.Add(new ItemStack { Id = id, Count = 1 });
                Combat = Session.PrepareCombat(); Combat.EnemyBudget = 0; Combat.PassiveHeal = 0; Combat.Boss = null;
                Combat.EnableArenaToys = Combat.EnableBossArenaPhase = false; Combat.WaveDuration = 1000; Combat.ChestInterval = 1000;
                Combat.CompanionPatrolSpeed = Combat.CompanionAcceleration = 0; Combat.ExtendedLoot = false;
                Simulation = new CombatSimulation(Combat); Simulation.Start(13104);
                foreach (UnitState unit in Simulation.Units) unit.NextAttack = unit.NextPassive = float.PositiveInfinity;
                Source = Simulation.Units.First(unit => unit.Faction == Faction.Companion);
                Source.Position = Source.PreviousPosition = Vector2.zero; Source.AttackDirection = Vector2.right;
            }
            public UnitState Enemy(Vector2 position)
            {
                UnitDefinition definition = UnityEngine.Object.Instantiate(Combat.MeleeEnemy); assets.Add(definition);
                definition.Health = 1000; definition.Speed = 0; definition.ContactDamage = 0;
                UnitState target = Simulation.AddFixture(definition, Faction.Enemy, position);
                target.NextAttack = target.NextContact = target.NextPassive = float.PositiveInfinity; return target;
            }
            public void Tick(int count) { for (int index = 0; index < count; index++) Simulation.Step(1f / 60f, Vector2.zero); }
            public void Dispose() { foreach (UnityEngine.Object asset in assets) UnityEngine.Object.DestroyImmediate(asset); Session.Dispose(); }
        }
    }
}
