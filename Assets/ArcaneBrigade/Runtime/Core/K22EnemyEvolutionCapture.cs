using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class K22EnemyEvolutionCapture
    {
        public static IEnumerator Run(Action<bool, string> check, Func<string, IEnumerator> capture)
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            EnemyEvolutionLibrary library = EnemyEvolutionLibrary.Current;
            check(library != null && library.Species.Length == 20, "Native library has all twenty species");
            if (library == null) yield break;
            var storage = new RunStorage(RunPaths.Root);
            foreach (EnemyEvolutionEntry entry in library.Species)
            {
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 22077); session.State.Roster.Clear(); session.State.Wave = 13; storage.SaveRun(session.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
                owner.Session.CombatCatalog.EnemyBudget = 0;
                owner.Session.CombatCatalog.EnableArenaToys = owner.Session.CombatCatalog.EnableBossArenaPhase = owner.Session.CombatCatalog.EnableSpectacles = false;
                CombatSimulation simulation = owner.Simulation;
                foreach (UnitState spawned in simulation.Units) if (spawned.Faction == Faction.Enemy) spawned.Alive = false;
                simulation.Champion.NextAttack = float.PositiveInfinity; owner.SetPresentationMoveTarget(Vector2.zero);
                UnitDefinition source = catalog.Enemies.First(unit => unit.Id == entry.Id);
                UnitDefinition normal = UnityEngine.Object.Instantiate(source), mutant = UnityEngine.Object.Instantiate(source);
                EnemyEvolutionLibrary.Apply(normal, 1); EnemyEvolutionLibrary.Apply(mutant, 13);
                normal.Speed = mutant.Speed = 0;
                normal.BodyHeight = Mathf.Max(1.1f, normal.BodyHeight); mutant.BodyHeight = normal.BodyHeight * 1.12f;
                UnitState first = simulation.AddExpandedPresentationFixture(normal, new Vector2(-1.8f, 0.8f), 100000);
                UnitState second = simulation.AddExpandedPresentationFixture(mutant, new Vector2(1.8f, 0.8f), 100000);
                first.NextAttack = first.NextContact = first.NextBehavior = first.NextEvolutionAttack = float.PositiveInfinity;
                second.NextAttack = second.NextContact = second.NextBehavior = second.NextEvolutionAttack = float.PositiveInfinity;
                first.Awakened = second.Awakened = true;
                yield return new WaitForSecondsRealtime(2.6f);
                UnitVisual firstView = UnityEngine.Object.FindObjectsByType<UnitVisual>().First(view => view.State == first);
                UnitVisual secondView = UnityEngine.Object.FindObjectsByType<UnitVisual>().First(view => view.State == second);
                check(firstView.DisplayedBody.texture == secondView.DisplayedBody.texture && firstView.DisplayedBody != secondView.DisplayedBody, "Native different normal/mutated drawings " + entry.Id);
                check(!firstView.WeaponRenderer.gameObject.activeInHierarchy && !secondView.WeaponRenderer.gameObject.activeInHierarchy, "Enemy attacks with its body, not a held weapon " + entry.Id);
                yield return capture(entry.Id + "-forms");
                first.Alive = false;
                if (entry.Ability == EnemyMutation.CoinBarrage) simulation.AddLootPresentationFixture(LootKind.Coin, second.Position + Vector2.right * 0.5f, 7);
                second.NextEvolutionAttack = 0;
                simulation.Step(0.05f, Vector2.zero); yield return null;
                check(second.EvolutionReleaseAt > simulation.Elapsed && second.EvolutionCasts == 0, "Native mutation anticipation " + entry.Id);
                yield return capture(entry.Id + "-windup");
                float deadline = Time.realtimeSinceStartup + 3;
                while (second.EvolutionCasts == 0 && Time.realtimeSinceStartup < deadline) yield return null;
                check(second.EvolutionCasts == 1, "Native mutation actually releases " + entry.Id);
                yield return new WaitForSecondsRealtime(0.28f);
                bool outcome = simulation.ExpandedFields.Any(field => field.Source == second) || simulation.Projectiles.Any(projectile => projectile.SourceUnitId == second.Id) || second.EvolutionTravelUntil > simulation.Elapsed;
                check(outcome, "Native body attack outcome " + entry.Id);
                if (entry.Ability == EnemyMutation.CoinBarrage) check(second.StolenCoins == 7, "Native loose coins fuel the barrage");
                yield return capture(entry.Id + "-action");
                check(secondView.BodyRenderer.bounds.size.y <= mutant.BodyHeight * 1.3f, "Mutation pose remains readable in frame " + entry.Id + " actual=" + secondView.BodyRenderer.bounds.size.y.ToString("F3") + " expected<=" + (mutant.BodyHeight * 1.3f).ToString("F3"));
                SceneManager.LoadScene("RunFrontend"); yield return null;
                UnityEngine.Object.Destroy(normal); UnityEngine.Object.Destroy(mutant);
            }
            check(catalog.Enemies.All(unit => unit.EvolutionStage == 0), "Native preview never changes source catalog");
        }
    }
}
