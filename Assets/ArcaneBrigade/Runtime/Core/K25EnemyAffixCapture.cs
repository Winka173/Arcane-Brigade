using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class K25EnemyAffixCapture
    {
        public static IEnumerator Run(Action<bool, string> check, Func<string, IEnumerator> capture)
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            EnemyAffixLibrary library = EnemyAffixLibrary.Current;
            check(library != null && library.Species.Length == 8, "Native eight specialist species and affix atlas");
            foreach (SpecialEnemyArt art in library.Species)
            {
                using (var session = new RunSession(catalog)) { session.Start("LanternKnight", 251077); session.State.Roster.Clear(); session.State.Wave = 20; storage.SaveRun(session.State); }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
                owner.Simulation.ConfigureEnemyVariantCapture();
                foreach (UnitState unit in owner.Simulation.Units) if (unit.Faction == Faction.Enemy) unit.Alive = false;
                owner.Simulation.Champion.NextAttack = float.PositiveInfinity;
                UnitDefinition source = catalog.Enemies.First(unit => unit.Id == art.Id);
                UnitDefinition normal = UnityEngine.Object.Instantiate(source), evolved = UnityEngine.Object.Instantiate(source);
                EnemyAffixLibrary.ApplyArt(normal, source.UnlockWave); EnemyAffixLibrary.ApplyArt(evolved, 30);
                normal.BodyHeight = 1.1f; evolved.BodyHeight = 1.21f; normal.Speed = evolved.Speed = 0;
                UnitState first = owner.Simulation.AddExpandedPresentationFixture(normal, new Vector2(-1.8f, 0.8f), 100000);
                UnitState second = owner.Simulation.AddExpandedPresentationFixture(evolved, new Vector2(1.8f, 0.8f), 100000);
                Freeze(first); Freeze(second);
                yield return new WaitForSecondsRealtime(2.6f);
                UnitVisual view = UnityEngine.Object.FindObjectsByType<UnitVisual>().First(visual => visual.State == second);
                check(view.DisplayedBody.texture == evolved.Body.texture && !view.WeaponRenderer.gameObject.activeInHierarchy, "Native unique body art, no held weapon " + art.Id);
                check(view.GetComponent<EnemyVariantVisual>().VisibleEffects > 0, "Native attached species smoke or flame " + art.Id);
                yield return capture(art.Id + "-forms");
                first.Alive = false; second.Variant.NextSpecial = 0;
                owner.Simulation.Step(0.05f, Vector2.zero); yield return null;
                check(second.Variant.ReleaseAt > owner.Simulation.Elapsed && second.Variant.SpecialCasts == 0, "Native specialist body anticipation " + art.Id);
                yield return capture(art.Id + "-windup");
                yield return new WaitForSecondsRealtime(1.25f);
                check(second.Variant.SpecialCasts == 1, "Native specialist real release " + art.Id);
                check(owner.Simulation.Projectiles.Any(shot => shot.SourceUnitId == second.Id) || owner.Simulation.ExpandedFields.Any(field => field.Source == second) || second.Variant.TravelEnd > owner.Simulation.Elapsed || second.Variant.DecoyUntil > owner.Simulation.Elapsed, "Native actual special outcome " + art.Id);
                yield return capture(art.Id + "-attack");
                check(view.BodyRenderer.bounds.size.y <= evolved.BodyHeight * 1.3f, "Native stable pose size " + art.Id);
                SceneManager.LoadScene("RunFrontend"); yield return null;
                UnityEngine.Object.Destroy(normal); UnityEngine.Object.Destroy(evolved);
            }
            foreach (EnemyAffix affix in Enum.GetValues(typeof(EnemyAffix)))
            {
                if (affix == EnemyAffix.None) continue;
                using (var session = new RunSession(catalog)) { session.Start("LanternKnight", 251077); session.State.Roster.Clear(); session.State.Wave = 20; storage.SaveRun(session.State); }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>(); owner.Simulation.ConfigureEnemyVariantCapture();
                foreach (UnitState unit in owner.Simulation.Units) if (unit.Faction == Faction.Enemy) unit.Alive = false;
                owner.Simulation.Champion.NextAttack = float.PositiveInfinity;
                UnitDefinition definition = UnityEngine.Object.Instantiate(catalog.Enemies.First(unit => unit.Id == "BristleImp"));
                EnemyEvolutionLibrary.Apply(definition, 20); definition.BodyHeight = 1.15f; definition.Speed = 0;
                UnitState enemy = owner.Simulation.AddExpandedPresentationFixture(definition, new Vector2(1.8f, 0.8f), definition.Health);
                Freeze(enemy); enemy.Variant.Affix = affix;
                if (affix == EnemyAffix.Blackfire) enemy.Health = definition.Health * 0.35f;
                yield return new WaitForSecondsRealtime(2.6f);
                UnitVisual view = UnityEngine.Object.FindObjectsByType<UnitVisual>().First(visual => visual.State == enemy);
                check(enemy.Definition.EvolutionStage == 1 && view.DisplayedBody.texture.name == "BristleImp_v1" && view.GetComponent<EnemyVariantVisual>().VisibleEffects > 0, "Affix layered on existing evolved body " + affix);
                yield return capture(affix + "-overlay");
                enemy.Variant.NextAffix = 0; owner.Simulation.Step(0.05f, Vector2.zero); yield return null;
                check(enemy.Variant.ReleaseAt > owner.Simulation.Elapsed, "Native affix body warning " + affix);
                yield return capture(affix + "-windup");
                yield return new WaitForSecondsRealtime(1.35f);
                check(enemy.Variant.AffixCasts == 1, "Native affix actual release " + affix);
                yield return capture(affix + "-action");
                while (enemy.Variant.TravelEnd > owner.Simulation.Elapsed) yield return null;
                UnitState hero = owner.Simulation.Champion;
                hero.Position = hero.PreviousPosition = owner.Simulation.ClampPosition(enemy.Position + Vector2.left * 0.65f, hero.Definition.Radius);
                hero.Definition.Weapon.Range = 15; hero.Definition.Weapon.Damage = enemy.Health * 3;
                hero.AttackDirection = (enemy.Position - hero.Position).normalized; hero.AttackTargetId = enemy.Id;
                hero.PendingAttack = true; hero.ImpactTime = 0;
                yield return new WaitForSecondsRealtime(0.15f);
                check(!enemy.Alive && UnityEngine.Object.FindObjectsByType<EnemyAffixDeparture>().Length > 0, "Native death dissipates attached smoke or fire " + affix);
                yield return capture(affix + "-death");
                yield return new WaitForSecondsRealtime(1);
                check(UnityEngine.Object.FindObjectsByType<EnemyAffixDeparture>().Length == 0, "Native affix death effects clean up " + affix);
                SceneManager.LoadScene("RunFrontend"); yield return null; UnityEngine.Object.Destroy(definition);
            }
            check(catalog.Enemies.All(unit => unit.EvolutionStage == 0), "Native scenarios preserve source catalog");
            foreach (int wave in new[] { 9, 13 })
            {
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 251077); session.State.Roster.Clear(); session.State.Wave = wave - 1;
                    session.State.Phase = SessionPhase.Shop; storage.SaveRun(session.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
                yield return new WaitForSecondsRealtime(2.6f);
                owner.NextWave();
                StageTransfer transfer = UnityEngine.Object.FindAnyObjectByType<StageTransfer>();
                float deadline = Time.realtimeSinceStartup + 12;
                while (transfer != null && transfer.Active && (PresentationSettings.ReducedMotion || transfer.ElapsedSeconds < 0.7f) && Time.realtimeSinceStartup < deadline) yield return null;
                transfer = UnityEngine.Object.FindAnyObjectByType<StageTransfer>();
                check(PresentationSettings.ReducedMotion ? (transfer == null || !transfer.Active) && owner.Session.State.Wave == wave && owner.Simulation.Phase == RunPhase.Combat : transfer != null && transfer.Kind == StageTransfer.CarrierFor(wave), "Native distinct carrier " + wave);
                yield return capture("transfer-" + wave + "-pickup");
                while (transfer != null && transfer.Active && transfer.ElapsedSeconds < 1.55f && Time.realtimeSinceStartup < deadline) yield return null;
                yield return capture("transfer-" + wave + "-throw");
                while (transfer != null && transfer.Active && Time.realtimeSinceStartup < deadline) yield return null;
                check(owner.Simulation.Phase == RunPhase.Combat && owner.Session.State.Wave == wave && owner.Session.State.OutsideTransfers == 1 && !owner.TransitionPending, "Carrier resumes the same arena once " + wave);
                yield return capture("transfer-" + wave + "-land");
                SceneManager.LoadScene("RunFrontend"); yield return null;
            }
        }
        private static void Freeze(UnitState unit)
        {
            unit.NextAttack = unit.NextContact = unit.NextBehavior = unit.NextEvolutionAttack = float.PositiveInfinity;
            unit.Variant.NextSpecial = unit.Variant.NextAffix = float.PositiveInfinity; unit.Awakened = true;
        }
    }
}
