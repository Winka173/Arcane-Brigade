using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class K20BossAnatomyCapture
    {
        public static IEnumerator Run(Action<bool, string> check, Func<string, IEnumerator> capture)
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            for (int index = 0; index < catalog.Bosses.Length; index++)
            {
                UnitDefinition source = catalog.Bosses[index];
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 20077); session.State.Wave = 4;
                    session.State.Roster.Clear(); session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "SpoonSabre" });
                    storage.SaveRun(session.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                yield return new WaitForSecondsRealtime(0.12f);
                ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
                check(owner != null && owner.Simulation.Boss != null, "Boss fixture loads " + source.Id);
                if (owner == null || owner.Simulation.Boss == null) yield break;
                owner.Session.CombatCatalog.EnemyBudget = 0; owner.Session.CombatCatalog.EnableBossArenaPhase = false;
                CombatSimulation simulation = owner.Simulation;
                UnitState boss = simulation.Boss;
                UnitDefinition template = UnityEngine.Object.Instantiate(source);
                template.Health = boss.Definition.Health; template.BodyHeight *= BossEncounterRules.BodyScale(source.BossPattern); template.Radius *= BossEncounterRules.CollisionScale; template.Speed = 0;
                boss.Definition = template; boss.Health = template.Health; boss.EntranceUntil = 0;
                boss.Position = boss.PreviousPosition = new Vector2(0, 0.7f); boss.NextBossPattern = float.PositiveInfinity;
                foreach (UnitState unit in simulation.Units) unit.NextAttack = unit.NextContact = float.PositiveInfinity;
                UnitVisual view = UnityEngine.Object.FindObjectsByType<UnitVisual>().First(candidate => candidate.State == boss);
                view.Bind(boss, owner.Session.CombatCatalog);
                owner.SetPresentationMoveTarget(Vector2.zero);
                yield return new WaitForSecondsRealtime(2.5f);
                BossAnatomyVisual anatomy = view.GetComponent<BossAnatomyVisual>();
                check(anatomy != null && anatomy.PartCount == 3, "Three authored attachments " + source.Id);
                check(view.GetComponentsInChildren<SpriteRenderer>().Where(renderer => renderer.name.StartsWith("BossPart-", StringComparison.Ordinal)).Select(renderer => renderer.sprite).Distinct().Count() == 3, "Distinct native part sprites " + source.Id);
                yield return capture(source.Id + "-intact");
                boss.BossVolley = index % 3; boss.NextBossPattern = simulation.Elapsed;
                for (int step = 0; step < 6; step++) simulation.Step(0.05f, Vector2.zero);
                check(boss.TelegraphUntil > simulation.Elapsed && boss.ActiveBossOrgan >= 0, "Live organ windup " + source.Id + " at=" + simulation.Elapsed.ToString("F2") + " until=" + boss.TelegraphUntil.ToString("F2") + " part=" + boss.ActiveBossOrgan);
                yield return null;
                yield return capture(source.Id + "-windup");
                yield return new WaitForSecondsRealtime(0.65f);
                check(boss.BossOrganReleaseAt >= 0, "Live organ release " + source.Id);
                boss.NextBossPattern = float.PositiveInfinity;
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k21-bosses") >= 0)
                {
                    boss.BossVolley = 3; boss.NextBossPattern = simulation.Elapsed;
                    simulation.Step(0.05f, Vector2.zero);
                    check(boss.ActiveBossSignature == 0 && boss.TelegraphUntil > simulation.Elapsed, "Native signature windup " + source.Id);
                    yield return capture(source.Id + "-signature-charge");
                    while (boss.TelegraphUntil > 0 && boss.Alive) yield return null;
                    boss.NextBossPattern = float.PositiveInfinity;
                    yield return new WaitForSecondsRealtime(0.4f);
                    check(simulation.ExpandedFields.Any(field => field.Source == boss), "Native non-projectile signature " + source.Id);
                    if (BossEncounterRules.Leaps(source.BossPattern, 0)) check(boss.BodyLift > 0.2f, "Native boss airborne " + source.Id);
                    view.Display(simulation.Elapsed, 1);
                    check(view.BodyRenderer.bounds.size.y <= template.BodyHeight * 1.12f, "Release pose keeps stable boss height " + source.Id + " actual=" + view.BodyRenderer.bounds.size.y.ToString("F2") + " expected=" + template.BodyHeight.ToString("F2"));
                    yield return capture(source.Id + "-signature-action");
                    yield return new WaitForSecondsRealtime(2.5f);
                    boss.NextBossPattern = float.PositiveInfinity;
                }
                UnitState ally = simulation.Units.First(unit => unit.Faction == Faction.Companion);
                UnitDefinition hitter = UnityEngine.Object.Instantiate(ally.Definition);
                WeaponDefinition blade = UnityEngine.Object.Instantiate(catalog.Weapon("SpoonSabre"));
                blade.Damage = template.Health * 0.19f; blade.Cooldown = 1000; blade.Range = 5; blade.ConeDot = -1;
                blade.Mechanic = CarnivalWeapon.None; blade.Family = ExpandedWeapon.None; blade.Signature = SignatureWeapon.None;
                hitter.Weapon = blade; hitter.Speed = 0; hitter.CriticalChance = 0; hitter.Passive = PassiveKind.None;
                ally.Definition = hitter; ally.NextRoamTime = ally.RoamRestUntil = float.PositiveInfinity;
                for (int part = 0; part < 3; part++)
                {
                    Vector2 offset = part == 0 ? Vector2.left : part == 1 ? Vector2.right : Vector2.down;
                    ally.Position = ally.PreviousPosition = boss.Position + offset;
                    ally.PendingAttack = false; ally.NextAttack = 0;
                    yield return new WaitForSecondsRealtime(0.5f);
                    check(BossPartRules.Broken(boss, part) && boss.Alive, "Real companion attack detaches " + source.Id + "/" + part);
                }
                yield return new WaitForSecondsRealtime(0.75f);
                check(anatomy.VisibleScars == 3, "Three persistent sockets after fragments depart " + source.Id);
                yield return capture(source.Id + "-broken");
                if (index == 0)
                {
                    blade.Damage = template.Health * 2; ally.NextAttack = 0;
                    yield return new WaitForSecondsRealtime(0.45f);
                    check(!boss.Alive, "Native boss defeat with anatomy");
                    check(UnityEngine.Object.FindObjectsByType<BossPartDeathScatter>().Length == 3, "Death remnant carries three scatter pieces");
                    yield return capture(source.Id + "-death");
                }
                SceneManager.LoadScene("RunFrontend"); yield return null;
                UnityEngine.Object.Destroy(template); UnityEngine.Object.Destroy(hitter); UnityEngine.Object.Destroy(blade);
            }
            yield return new WaitForSecondsRealtime(0.12f);
            RunMenu menu = UnityEngine.Object.FindAnyObjectByType<RunMenu>();
            PlayerProfile profile = storage.LoadProfile();
            foreach (UnitDefinition unit in catalog.Bosses) if (!profile.SeenBosses.Contains(unit.Id)) profile.SeenBosses.Add(unit.Id);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k21-bosses") >= 0)
            {
                foreach (UnitDefinition unit in catalog.Companions) if (!profile.SeenUnits.Contains(unit.Id)) profile.SeenUnits.Add(unit.Id);
                foreach (ItemDefinition item in catalog.Items) if (!profile.SeenItems.Contains(item.Id)) profile.SeenItems.Add(item.Id);
                foreach (WeaponDefinition weapon in catalog.Weapons) if (!profile.SeenWeapons.Contains(weapon.Id)) profile.SeenWeapons.Add(weapon.Id);
            }
            string before = JsonUtility.ToJson(profile);
            menu.ConfigureK7Diagnostics(catalog, profile); menu.ShowPage(RunMenu.Page.Museum); menu.Museum.SelectTab(4); menu.Museum.SetPreview(1, 2);
            yield return new WaitForSecondsRealtime(0.3f); yield return capture("Museum-anatomy-controls");
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k21-bosses") >= 0)
            {
                foreach (int tab in new[] { 0, 1, 2, 3 })
                {
                    menu.Museum.SelectTab(tab); yield return null;
                    menu.Museum.SelectExhibitForDiagnostics(tab == 1 ? catalog.Items.Length - 1 : 0);
                    yield return new WaitForSecondsRealtime(0.2f); yield return capture("Collection-tab-" + tab);
                }
            }
            check(JsonUtility.ToJson(profile) == before, "Anatomy collection preview cannot mutate progress");
            menu.ShowPage(RunMenu.Page.Landing); yield return null;
            check(!UnityEngine.Object.FindObjectsByType<BossAnatomyVisual>().Any(), "Anatomy preview cleanup");
        }
    }
}
