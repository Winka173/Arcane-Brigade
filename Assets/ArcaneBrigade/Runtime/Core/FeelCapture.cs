using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native evidence for combat feel: explosion frames in a live arena, plus a log of camera shake, hit holds and
    /// slow motion over a real crowded fight. Run with -arcane-feel-capture -capturePath DIR (isolated diagnostics save).</summary>
    public sealed class FeelCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-feel-capture") >= 0;
        private string root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<FeelCapture>() != null) return;
            var go = new GameObject("FeelCapture"); DontDestroyOnLoad(go); go.AddComponent<FeelCapture>();
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG());
            Destroy(image);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171101); session.State.Wave = 9; session.State.Phase = SessionPhase.Combat;
                session.State.MienCho.ReadMask = (1 << MienChoStory.Available(session.State)) - 1;
                session.State.Roster.Clear();
                foreach (var unit in catalog.Companions)
                {
                    if (session.State.Roster.Count >= 10) break;
                    if (unit.GiftOnly || unit.SpecialRecruit || unit.Weapon == null) continue;
                    session.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = unit.Weapon.Id });
                }
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3f);
            var owner = FindAnyObjectByType<ArenaController>();
            var blasts = owner.GetComponent<ExplosionFeedback>();
            var log = new List<string>();
            // A: a hero area blast and a crit-kill burst, frame by frame
            Vector2 hero = owner.Simulation.Champion.Position;
            blasts.Observe(new CombatEvent(CombatEventKind.Detonation, hero + new Vector2(1.6f, 0.4f), Faction.Champion, 1.8f, Vector2.right, element: DamageElement.Ember), true);
            blasts.Observe(new CombatEvent(CombatEventKind.Detonation, hero + new Vector2(-1.8f, -0.6f), Faction.Champion, 1.2f, Vector2.left, element: DamageElement.Frost), true);
            float started = Time.realtimeSinceStartup;
            foreach (float t in new[] { 0.04f, 0.12f, 0.24f, 0.42f })
            {
                while (Time.realtimeSinceStartup - started < t) yield return null;
                yield return Shot("A-blast-" + Mathf.RoundToInt(t * 1000) + "ms");
            }
            // B: a boss falling: shock ring, big flash
            yield return new WaitForSecondsRealtime(0.6f);
            var boss = new CombatEvent(CombatEventKind.BossLanded, hero + new Vector2(0, 1.2f), Faction.Enemy, 0, Vector2.down);
            blasts.Observe(boss, true);
            started = Time.realtimeSinceStartup;
            foreach (float t in new[] { 0.06f, 0.2f, 0.4f })
            {
                while (Time.realtimeSinceStartup - started < t) yield return null;
                yield return Shot("B-shock-" + Mathf.RoundToInt(t * 1000) + "ms");
            }
            // D: ordinary deaths: every enemy near the hero is left on its last breath, the brigade's real hits finish them,
            // and the pops are photographed as they happen (no bowls, no lying bodies: flash, squash, ink burst, gone)
            yield return new WaitForSecondsRealtime(0.8f);
            var deaths = owner.GetComponent<DeathFeedback>();
            int popsBefore = blasts.PopsShown;
            for (int round = 0; round < 3; round++)
            {
                hero = owner.Simulation.Champion.Position;
                foreach (UnitState unit in owner.Simulation.Units)
                    if (unit.Alive && unit.Faction == Faction.Enemy && !unit.Definition.IsBoss && Vector2.Distance(unit.Position, hero) < 7f) unit.Health = Mathf.Min(unit.Health, 0.01f);
                started = Time.realtimeSinceStartup;
                foreach (float t in new[] { 0.05f, 0.12f, 0.22f })
                {
                    while (Time.realtimeSinceStartup - started < t) yield return null;
                    yield return Shot("D-pops-" + round + "-" + Mathf.RoundToInt(t * 1000) + "ms");
                }
                log.Add("pops round " + round + ": explosions active " + blasts.ActiveCount + ", death remnants active " + deaths.ActiveCount);
                yield return new WaitForSecondsRealtime(0.6f);
            }
            log.Add("enemy pops shown " + (blasts.PopsShown - popsBefore));
            // E: a staged crit pop of a large body and the closing blast of a boss finale, frame by frame
            hero = owner.Simulation.Champion.Position;
            blasts.Pop(catalog.Unit("UndeliveredKing") ?? owner.Simulation.Champion.Definition, hero + new Vector2(2.2f, 0.6f), true, 1.4f, DamageElement.Ember);
            blasts.BossFinale(hero + new Vector2(-2.6f, 0.4f), 2.4f);
            UnitDefinition ordinary = null;
            foreach (UnitState unit in owner.Simulation.Units) if (unit.Faction == Faction.Enemy && !unit.Definition.IsBoss && !ExplosionFeedback.Drifts(unit.Definition) && unit.Definition.Radius < 0.4f && unit.Definition.BodyHeight < 1.4f) { ordinary = unit.Definition; break; } // a body that burns (things of air come apart as vapour)
            blasts.Pop(ordinary, hero + new Vector2(0.6f, -1.9f), false, 0.1f, DamageElement.Physical);
            blasts.Pop(ordinary, hero + new Vector2(1.9f, -1.6f), false, 1.2f, DamageElement.Frost);
            // an elite's big blast with its pillar of light, and a thing of air coming apart into vapour
            blasts.Pop(ordinary, hero + new Vector2(-1.4f, -1.7f), false, 0.3f, DamageElement.Arcane, true, true);
            blasts.Pop(catalog.Unit("CurtainWisp") ?? ordinary, hero + new Vector2(3.4f, -0.4f), false, 0.2f, DamageElement.Arcane);
            started = Time.realtimeSinceStartup;
            foreach (float t in new[] { 0.04f, 0.12f, 0.25f, 0.45f, 0.7f, 1.0f })
            {
                while (Time.realtimeSinceStartup - started < t) yield return null;
                yield return Shot("E-crit-and-boss-" + Mathf.RoundToInt(t * 1000) + "ms");
            }
            // C: twelve seconds of the real fight: how hard and how often the screen moves and holds
            float peak = 0, sum = 0; int frames = 0, holdFrames = 0, holds = 0; bool wasHeld = false;
            started = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - started < 12f && owner.Simulation.Phase == RunPhase.Combat)
            {
                owner.Simulation.Champion.Health = owner.Simulation.Champion.Definition.Health;
                float offset = owner.CameraAccent.magnitude; peak = Mathf.Max(peak, offset); sum += offset; frames++;
                bool held = owner.Feel.Frozen; if (held) holdFrames++; if (held && !wasHeld) holds++; wasHeld = held;
                yield return null;
            }
            yield return Shot("C-live-fight");
            log.Add("frames " + frames + " peak shake " + peak.ToString("0.000") + " (max " + CombatFeel.MaxShake + ") mean " + (sum / Mathf.Max(1, frames)).ToString("0.0000"));
            log.Add("holds " + holds + " held frames " + holdFrames + " (" + (100f * holdFrames / Mathf.Max(1, frames)).ToString("0.0") + "% of frames)");
            log.Add("live explosions active at end " + blasts.ActiveCount + ", enemies alive " + owner.Simulation.Units.Count);
            File.WriteAllLines(Path.Combine(root, "Feel.txt"), log);
            Application.Quit(0);
        }
    }
}
