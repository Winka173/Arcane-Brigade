using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class K19EncounterCapture
    {
        public static IEnumerator Run(Action<bool, string> check, Func<string, IEnumerator> capture)
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 19077); session.State.Wave = 4;
                session.State.Roster.Clear();
                session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "SpoonSabre", Tier = 2 });
                session.State.Roster.Add(new RosterEntry { UnitId = "ReelOwl", WeaponId = "DreamOrbStaff", Tier = 3 });
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            yield return new WaitForSecondsRealtime(0.15f);
            ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            check(owner != null && owner.Simulation.Boss != null, "Native boss fixture resumes without loading UI");
            if (owner == null || owner.Simulation.Boss == null) yield break;
            owner.Session.CombatCatalog.EnemyBudget = 0;
            foreach (UnitState unit in owner.Simulation.Units) unit.NextAttack = unit.NextContact = float.PositiveInfinity;
            UnitState boss = owner.Simulation.Boss; boss.NextBossPattern = float.PositiveInfinity;
            boss.Position = boss.PreviousPosition = Vector2.up * 1.6f;
            boss.Definition.Speed = 0;
            owner.SetPresentationMoveTarget(owner.Simulation.Champion.Position);
            yield return new WaitForSecondsRealtime(1.3f);
            yield return capture("Boss-organs-intact");
            UnitVisual bossView = UnityEngine.Object.FindObjectsByType<UnitVisual>().First(view => view.State == boss);
            check(bossView.GetComponent<EvolutionAdornment>()?.PieceCount == 4, "Boss has two outlined breakable organs");
            UnitState ally = owner.Simulation.Units.First(unit => unit.Faction == Faction.Companion);
            UnitDefinition hitter = UnityEngine.Object.Instantiate(ally.Definition);
            WeaponDefinition blade = UnityEngine.Object.Instantiate(catalog.Weapon("SpoonSabre"));
            blade.Damage = boss.Definition.Health * 0.22f; blade.Cooldown = 1000; blade.Range = 4; blade.ConeDot = -1;
            blade.Mechanic = CarnivalWeapon.None; blade.Family = ExpandedWeapon.None; blade.Signature = SignatureWeapon.None;
            hitter.Weapon = blade; hitter.Speed = 0; hitter.CriticalChance = 0; hitter.Passive = PassiveKind.None;
            ally.Definition = hitter; ally.Position = ally.PreviousPosition = boss.Position + Vector2.left; ally.NextAttack = 0;
            ally.NextRoamTime = ally.RoamRestUntil = float.PositiveInfinity;
            yield return new WaitForSecondsRealtime(0.5f);
            check(boss.LeftPartBroken && !boss.RightPartBroken, "Native ally attack breaks left organ only");
            yield return capture("Boss-left-organ-break");
            ally.Position = ally.PreviousPosition = boss.Position + Vector2.right; ally.NextAttack = 0;
            yield return new WaitForSecondsRealtime(0.5f);
            check(boss.LeftPartBroken && boss.RightPartBroken && boss.Alive, "Native attacks break both organs without instantly killing boss");
            boss.NextBossPattern = 0;
            yield return new WaitForSecondsRealtime(1.1f);
            check(owner.Simulation.ExpandedFields.Any(field => field.Source == boss && field.Kind == ExpandedFieldKind.Shockwave), "Native exposed core announces shockwave");
            yield return capture("Boss-exposed-core");
            SceneManager.LoadScene("RunFrontend"); yield return null;
            UnityEngine.Object.Destroy(hitter); UnityEngine.Object.Destroy(blade);
            yield return new WaitForSecondsRealtime(0.15f);
            RunMenu menu = UnityEngine.Object.FindAnyObjectByType<RunMenu>();
            PlayerProfile profile = storage.LoadProfile();
            foreach (UnitDefinition unit in catalog.Companions) if (!profile.SeenUnits.Contains(unit.Id)) profile.SeenUnits.Add(unit.Id);
            foreach (UnitDefinition unit in catalog.Bosses) if (!profile.SeenBosses.Contains(unit.Id)) profile.SeenBosses.Add(unit.Id);
            foreach (ItemDefinition item in catalog.Items) if (!profile.SeenItems.Contains(item.Id)) profile.SeenItems.Add(item.Id);
            string untouched = JsonUtility.ToJson(profile);
            menu.ConfigureK7Diagnostics(catalog, profile); menu.ShowPage(RunMenu.Page.Museum);
            menu.Museum.SelectTab(2);
            menu.Museum.SelectExhibitForDiagnostics(Array.FindIndex(catalog.Companions, unit => unit.Id == "TeapotPilgrim"));
            for (int stars = 1; stars <= 3; stars++)
            {
                menu.Museum.SetPreview(stars, 0);
                yield return new WaitForSecondsRealtime(0.25f);
                UnitVisual view = UnityEngine.Object.FindObjectsByType<UnitVisual>().FirstOrDefault();
                check(view != null && view.State.Definition.CombatTier == stars, "Collection renders real tier " + stars);
                check(stars == 1 ? view.GetComponent<EvolutionAdornment>() == null : view.GetComponent<EvolutionAdornment>().PieceCount >= (stars == 2 ? 3 : 5), "Tier silhouette gains authored accessories " + stars);
                yield return capture("Collection-tier-" + stars);
            }
            foreach (int pose in new[] { 1, 2, 3 })
            {
                menu.Museum.SetPreview(3, pose); yield return new WaitForSecondsRealtime(0.18f);
                yield return capture("Collection-pose-" + pose);
            }
            check(JsonUtility.ToJson(profile) == untouched, "Collection preview grants no items, stats or discoveries");
            menu.Museum.SelectTab(1);
            foreach (int index in new[] { 0, 9, 18, 27, 36, 45, 54, 63, 67 })
            {
                menu.Museum.SelectExhibitForDiagnostics(index); yield return new WaitForSecondsRealtime(0.12f);
                yield return capture("Relics-" + index.ToString("D2"));
            }
            menu.Museum.SelectTab(4); yield return new WaitForSecondsRealtime(0.3f);
            yield return capture("Collection-boss-organs");
            menu.ShowPage(RunMenu.Page.Landing); yield return null;
            check(!UnityEngine.Object.FindObjectsByType<UnitVisual>().Any(), "Leaving collection releases all preview actors");
        }
    }
}
