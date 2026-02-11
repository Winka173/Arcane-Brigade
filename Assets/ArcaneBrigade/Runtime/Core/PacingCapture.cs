using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native evidence for the pacing pass: an ink-bloater chain, a fuse runner, a surge's warning and wall, the
    /// Lantern Flare, the shopkeeper's bell, a chest receipt and the end-of-wave sweep.
    /// Run with -arcane-pacing-capture -capturePath DIR (isolated diagnostics save).</summary>
    public sealed class PacingCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-pacing-capture") >= 0;
        private string root;
        private ArenaController owner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<PacingCapture>() != null) return;
            var go = new GameObject("PacingCapture"); DontDestroyOnLoad(go); go.AddComponent<PacingCapture>();
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG());
            Destroy(image);
        }
        private void Update()
        {
            // the traveller is not the subject here: keep the light burning
            if (owner != null && owner.Simulation != null && owner.Simulation.Champion != null && owner.Simulation.Phase == RunPhase.Combat)
                owner.Simulation.Champion.Health = owner.Simulation.Champion.Definition.Health;
        }
        private IEnumerator Frames(string name, params float[] times)
        {
            float started = Time.realtimeSinceStartup;
            foreach (float t in times)
            {
                while (Time.realtimeSinceStartup - started < t) yield return null;
                yield return Shot(name + "-" + Mathf.RoundToInt(t * 1000) + "ms");
            }
        }
        private Vector2 Hero => owner.Simulation.Champion.Position;
        private void Thin(float radius)
        {
            foreach (UnitState unit in owner.Simulation.Units)
                if (unit.Alive && unit.Faction == Faction.Enemy && !unit.Definition.IsBoss && Vector2.Distance(unit.Position, Hero) < radius) unit.Position += (unit.Position - Hero).normalized * 9f;
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
                session.Start("LanternKnight", 171202); session.State.Wave = 7; session.State.Phase = SessionPhase.Combat;
                session.State.MienCho.ReadMask = (1 << MienChoStory.Available(session.State)) - 1;
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2.5f);
            owner = FindAnyObjectByType<ArenaController>();
            CombatSimulation sim = owner.Simulation;
            var log = new List<string>();
            log.Add("wave length " + sim.WaveDuration + " s");

            // A: an ink-bloater chain: one is popped, the burst takes the next, three links at most
            Thin(6);
            yield return new WaitForSecondsRealtime(0.3f);
            var chain = new List<UnitState>();
            for (int i = 0; i < 5; i++) chain.Add(sim.PacingFixtureRole(SwarmRole.Bloater, Hero + new Vector2(2.4f + i * 1.05f, 0.4f * (i % 2)), 30f));
            yield return Shot("A-bloaters-waiting");
            int killsBefore = sim.Kills;
            sim.PacingFixtureKill(chain[0]);
            yield return Frames("A-chain", 0.06f, 0.18f, 0.32f, 0.5f);
            int popped = 0; foreach (UnitState unit in chain) if (!unit.Alive) popped++;
            log.Add("bloater chain: " + popped + " of 5 burst (chain depth cap " + SwarmPacingRules.ChainDepth + "), kills +" + (sim.Kills - killsBefore));

            // B: a fuse runner sprints in, stops, swells, blows
            yield return new WaitForSecondsRealtime(0.4f);
            Thin(6);
            UnitState fuse = sim.PacingFixtureRole(SwarmRole.Fuse, Hero + new Vector2(5.5f, 0.6f), 400f);
            float waited = 0;
            while (fuse.FuseAt <= 0 && waited < 4f) { waited += Time.deltaTime; yield return null; }
            log.Add("fuse runner lit after " + waited.ToString("0.00") + " s");
            yield return Frames("B-fuse", 0.05f, 0.4f, 0.72f, 0.85f);
            log.Add("fuse runner alive after blast: " + fuse.Alive);

            // C: a surge: the warning at the edge, then the wall walking in
            yield return new WaitForSecondsRealtime(0.5f);
            sim.PacingFixtureSurge(1.0f);
            int surges = sim.SurgesFired;
            yield return Frames("C-surge", 0.2f, 0.75f, 1.35f, 2.4f);
            log.Add("surge fired: " + (sim.SurgesFired > surges) + " towards " + sim.SurgeDirection);

            // D: the Lantern Flare
            yield return new WaitForSecondsRealtime(0.4f);
            int alive = 0; foreach (UnitState unit in sim.Units) if (unit.Alive && unit.Faction == Faction.Enemy) alive++;
            sim.PacingFixtureLoot(LootKind.Flare, Hero + new Vector2(0.4f, 0), 1);
            yield return Shot("D-flare-pickup");
            float flareStarted = Time.realtimeSinceStartup;
            int after = alive;
            while (Time.realtimeSinceStartup - flareStarted < 2f) { yield return null; bool fired = false; foreach (CombatEvent e in sim.Events) if (e.Kind == CombatEventKind.Flare) fired = true; if (fired) break; }
            yield return Frames("D-flare", 0.04f, 0.16f, 0.35f);
            after = 0; foreach (UnitState unit in sim.Units) if (unit.Alive && unit.Faction == Faction.Enemy) after++;
            log.Add("lantern flare: enemies " + alive + " -> " + after);

            // E: the bell: coins all over the hall come running
            yield return new WaitForSecondsRealtime(0.6f);
            for (int i = 0; i < 40; i++) sim.PacingFixtureLoot(LootKind.Coin, Hero + new Vector2(Mathf.Cos(i * 0.7f), Mathf.Sin(i * 0.7f)) * (5 + i % 4), 1);
            yield return new WaitForSecondsRealtime(0.5f);
            int goldBefore = sim.Gold;
            sim.PacingFixtureLoot(LootKind.Bell, Hero + new Vector2(0.3f, 0.2f), 1);
            yield return Frames("E-bell", 0.45f, 0.75f, 1.2f);
            log.Add("bell: gold +" + (sim.Gold - goldBefore));

            // F: a chest and its receipt
            yield return new WaitForSecondsRealtime(0.4f);
            sim.PacingFixtureLoot(LootKind.Chest, Hero + new Vector2(0.5f, 0.3f), 9);
            yield return Frames("F-chest", 0.4f, 0.75f, 1.15f, 1.6f);

            // G: the wave is won with coins on the floor: they fly home and are counted
            for (int i = 0; i < 36; i++) sim.PacingFixtureLoot(LootKind.Coin, Hero + new Vector2(Mathf.Cos(i * 0.9f), Mathf.Sin(i * 0.9f)) * (3 + i % 5), 2);
            yield return new WaitForSecondsRealtime(0.4f);
            sim.PacingFixtureWin();
            yield return Frames("G-sweep", 0.15f, 0.4f, 0.8f, 1.4f);
            log.Add("sweep: " + sim.SweptCoins + " pieces, +" + sim.SweptGold + " gold");
            File.WriteAllLines(Path.Combine(root, "Pacing.txt"), log);
            Application.Quit(0);
        }
    }
}
