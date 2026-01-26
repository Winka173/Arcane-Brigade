using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K18ExperienceCapture : MonoBehaviour
    {
        private string root;
        private bool failed;
        private readonly List<string> notes = new List<string>();
        private ArenaController owner;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!RunPaths.K7Capture || Array.IndexOf(args, "-arcane-k18-experience") < 0 || FindAnyObjectByType<K18ExperienceCapture>() != null) return;
            int path = Array.IndexOf(args, "-capturePath"); if (path < 0 || path + 1 >= args.Length) return;
            var child = new GameObject("K18ExperienceCapture"); DontDestroyOnLoad(child);
            child.AddComponent<K18ExperienceCapture>().root = Path.GetFullPath(args[path + 1]);
        }
        private void Check(bool condition, string caption) { failed |= !condition; notes.Add((condition ? "PASS " : "FAIL ") + caption); }
        private void Log(string message, string trace, LogType kind)
        {
            if (kind != LogType.Error && kind != LogType.Exception && kind != LogType.Assert) return;
            failed = true; notes.Add("FAIL " + message); File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(1);
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), frame.EncodeToPNG()); Destroy(frame);
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(root); Application.runInBackground = true; Application.logMessageReceived += Log;
            yield return K7PlayerCapture.ResizeDiagnostic(3840, 2160, Check);
            var storage = new RunStorage(RunPaths.Root);
            bool reduced = Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k17-reduced") >= 0;
            storage.SaveProfile(new PlayerProfile { ReducedMotion = reduced, ReducedEffects = reduced, ReducedShake = reduced, ReducedFlashes = reduced });
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k19-encounters") >= 0)
            {
                yield return K19EncounterCapture.Run(Check, Capture);
                Check(!failed, "No native runtime errors"); File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(failed ? 1 : 0); yield break;
            }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (int wave in new[] { 5, 7, 9 })
            {
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 18077); session.State.Wave = wave;
                    session.State.Roster.Clear();
                    session.State.Roster.Add(new RosterEntry { UnitId = "ReelOwl", WeaponId = "MoonseedOrb" });
                    session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "AfterimageSabre" });
                    session.State.Roster.Add(new RosterEntry { UnitId = "CoilKettle", WeaponId = "SatelliteDisc" });
                    session.Validate(session.State);
                    storage.SaveRun(session.State);
                    Check(storage.LoadRun(session) != null, "Diagnostic run storage round-trip: " + storage.LastError);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                yield return new WaitForSecondsRealtime(0.1f);
                owner = FindAnyObjectByType<ArenaController>();
                Check(owner != null && owner.Session.State.Wave == wave, "Native fixture resumes wave " + wave);
                owner.Session.CombatCatalog.EnemyBudget = 0;
                owner.Simulation.Champion.NextAttack = float.PositiveInfinity;
                if (wave == 5)
                {
                    var enemies = new List<UnitDefinition>();
                    int index = 0;
                    foreach (UnitState ally in owner.Simulation.Units.Where(unit => unit.Faction == Faction.Companion).ToArray())
                    {
                        Vector2 point = new Vector2((index++ - 1) * 3.2f, -0.6f);
                        ally.Position = ally.PreviousPosition = point; ally.Definition.Speed = 0;
                        UnitDefinition definition = Instantiate(catalog.Enemies.First(unit => unit.Behavior == EnemyBehavior.Standard && unit.Weapon.Kind == WeaponKind.Contact));
                        enemies.Add(definition); definition.Health = 100000; definition.Speed = 0;
                        UnitState target = owner.Simulation.AddExpandedPresentationFixture(definition, point + Vector2.up * 1.8f, 100000);
                        target.NextAttack = target.NextContact = float.PositiveInfinity;
                    }
                    var observed = new HashSet<CarnivalWeapon>(); bool bloom = false, echo = false, orbit = false, returning = false;
                    for (int sample = 0; sample < 48; sample++)
                    {
                        yield return new WaitForSecondsRealtime(0.08f);
                        foreach (ProjectileState projectile in owner.Simulation.Projectiles)
                        {
                            if (projectile.Weapon == null) continue;
                            observed.Add(projectile.Weapon.Mechanic);
                            if (projectile.Weapon.Mechanic == CarnivalWeapon.SatelliteDisc)
                            {
                                orbit |= projectile.Age >= 0.55f && projectile.Age < 1.55f;
                                returning |= projectile.Returning;
                            }
                        }
                        bloom |= owner.Simulation.ExpandedFields.Any(field => field.ImpactWeapon != null && field.ImpactWeapon.Mechanic == CarnivalWeapon.BloomOrb);
                        echo |= owner.Simulation.ExpandedFields.Any(field => field.ImpactWeapon != null && field.ImpactWeapon.Mechanic == CarnivalWeapon.EchoBlade);
                        if (sample == 12) yield return Capture("variant-weapons-live");
                    }
                    Check(observed.Contains(CarnivalWeapon.BloomOrb) && bloom, "Native slow orb produces delayed bloom field");
                    Check(echo, "Native sword produces delayed afterimage strike");
                    Check(orbit && returning, "Native disc enters orbit and returns to owner");
                    owner.SetPresentationMoveTarget(new Vector2(1.4f, -0.5f)); yield return new WaitForSecondsRealtime(0.25f);
                    UnitVisual champion = FindObjectsByType<UnitVisual>().First(visual => visual.State.Faction == Faction.Champion);
                    Check(champion.GaitWeight > 0.1f, "Native movement smoothly gains gait weight");
                    yield return Capture("motion-running");
                    owner.SetPresentationMoveTarget(owner.Simulation.Champion.Position); yield return new WaitForSecondsRealtime(0.5f);
                    Check(champion.GaitWeight < 0.06f, "Native stopping settles gait without sliding forever");
                    yield return Capture("motion-settled");
                    foreach (UnitState ally in owner.Simulation.Units.Where(unit => unit.Faction == Faction.Companion))
                    {
                        ally.Definition.Weapon.ProjectileCount = 24; ally.Definition.Weapon.Cooldown = 0.2f;
                        ally.Definition.Weapon.ProjectilePierce = 64; ally.Definition.Weapon.ProjectileLifetime = 5;
                        ally.Definition.Weapon.ProjectileSpread = 0.025f; ally.NextAttack = owner.Simulation.Elapsed;
                    }
                    UnitDefinition shooter = Instantiate(owner.Session.CombatCatalog.RangedEnemy);
                    WeaponDefinition shot = Instantiate(shooter.Weapon); shooter.Weapon = shot;
                    shot.Kind = WeaponKind.Projectile; shot.Damage = 1; shot.Range = 20; shot.ProjectileSpeed = 1.2f; shot.ProjectileCount = 3; shot.Cooldown = 0.4f; shot.Windup = 0.12f;
                    shooter.Speed = 0; shooter.Behavior = EnemyBehavior.Standard; shooter.IsBoss = false;
                    enemies.Add(shooter);
                    owner.Simulation.AddExpandedPresentationFixture(shooter, new Vector2(0, 2.8f), 100000);
                    yield return new WaitForSecondsRealtime(2.5f); yield return Capture("crowded-projectile-hierarchy");
                    Check(EffectDensity.CrowdPressure > 0.3f, "Native dense projectile fixture engages automatic readability");
                    SpriteRenderer[] projectiles = FindObjectsByType<ProjectileVisual>().Select(visual => visual.GetComponent<SpriteRenderer>()).Where(view => view != null && view.enabled).ToArray();
                    RequireProjectileHierarchy(projectiles);
                    foreach (ProjectileState projectile in owner.Simulation.Projectiles) projectile.Remaining = 0;
                    foreach (UnitState unit in owner.Simulation.Units.Where(unit => unit.Faction == Faction.Enemy)) unit.Alive = false;
                    foreach (UnitDefinition definition in enemies) Destroy(definition);
                    Destroy(shot);
                }
                foreach (UnitState unit in owner.Simulation.Units) if (unit.Faction != Faction.Enemy) unit.NextAttack = float.PositiveInfinity;
                float scheduled = owner.Simulation.WaveDuration * 0.4f;
                while (owner.Simulation.Elapsed < scheduled + 0.7f) owner.Simulation.Step(0.05f, Vector2.zero);
                yield return null;
                MidwaveSpectacle show = owner.Simulation.Spectacle;
                Check(show != null && !show.Dangerous, "Event has visible safe warning before damage " + wave);
                if (show == null) continue;
                yield return Capture(show.Kind + "-warning");
                while (show.Age(owner.Simulation.Elapsed) < 2.2f) owner.Simulation.Step(0.05f, Vector2.zero);
                Vector2 framing = owner.Simulation.ClampPosition(show.Position + Vector2.up * 1.8f, owner.Simulation.Champion.Definition.Radius);
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = framing;
                owner.SetPresentationMoveTarget(framing);
                yield return new WaitForSecondsRealtime(0.3f);
                yield return null;
                Check(FindAnyObjectByType<MidwaveSpectacleVisual>().Visible, "Event renders authored props and warning ink " + show.Kind);
                yield return Capture(show.Kind + "-active");
                while (show.Active(owner.Simulation.Elapsed)) owner.Simulation.Step(0.05f, Vector2.zero);
                yield return null;
                Check(!FindAnyObjectByType<MidwaveSpectacleVisual>().Visible && show.RewardDrops <= 12, "Event clears with bounded loot " + show.Kind);
            }
            notes.Add("Isolated native visual fixtures use selected allies, inert high-health targets and accelerated simulation between event samples. They are not normal-run balance, audio approval or performance evidence.");
            Check(!failed, "No native runtime errors"); File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(failed ? 1 : 0);
        }
        private void RequireProjectileHierarchy(SpriteRenderer[] projectiles)
        {
            SpriteRenderer[] hostile = projectiles.Where(view => view.sortingOrder == 2400).ToArray();
            SpriteRenderer[] allied = projectiles.Where(view => view.sortingOrder == 2100).ToArray();
            Check(hostile.Length > 0 && allied.Length > 0, "Native hierarchy fixture includes both enemy and allied shots");
            Check(hostile.All(view => view.color.a > 0.9f) && allied.All(view => view.color.a < 0.73f), "Hostile shots remain solid and above dimmed allied shots");
        }
    }
}
