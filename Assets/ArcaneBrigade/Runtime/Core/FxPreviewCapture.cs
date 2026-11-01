using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Review clips of the light effects: -arcane-fx-preview DIR [-rules a,b,c] plays a real wave-12 fight (isolated
    /// diagnostics save, the hero cannot fall) and records each rule for six seconds at a steady 30 frames a second (frames as
    /// PNG, DIR/&lt;rule&gt;/0000.png...), the effect dropped on an enemy in the crowd every .9 s. "comet-runner" records the Comet
    /// Runner's real METEOR SHOWER instead.</summary>
    public sealed class FxPreviewCapture : MonoBehaviour
    {
        private static string Arg(string key) { string[] a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }
        public static bool Requested => Arg("-arcane-fx-preview") != null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<FxPreviewCapture>() != null) return;
            var go = new GameObject("FxPreviewCapture"); DontDestroyOnLoad(go); go.AddComponent<FxPreviewCapture>();
        }

        private IEnumerator Start()
        {
            string root = Path.GetFullPath(Arg("-arcane-fx-preview")); Directory.CreateDirectory(root);
            Application.runInBackground = true;
            // never the player's own save: the capture runs only on the isolated diagnostics folder
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { File.AppendAllText(Path.Combine(root, "log.txt"), "refused: not an isolated save\n"); Application.Quit(1); yield break; }
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (string rule in (Arg("-rules") ?? "falling-star,comet-runner,boss-warning").Split(','))
            {
                bool comet = rule == "comet-runner", warning = rule == "boss-warning";
                using (var run = new RunSession(catalog))
                {
                    run.Start(comet ? "CometRunner" : "LanternKnight", 11011); run.State.Wave = warning ? 4 : 11; run.State.Phase = SessionPhase.Combat;
                    new RunStorage(RunPaths.Root).SaveRun(run.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3f);
                var owner = FindAnyObjectByType<ArenaController>();
                if (owner == null) { File.AppendAllText(Path.Combine(root, "log.txt"), rule + ": no arena\n"); continue; }
                float until = Time.realtimeSinceStartup + 10;
                while (owner.Simulation.Units.Count(u => u.Alive && u.Faction == Faction.Enemy) < 12 && Time.realtimeSinceStartup < until) yield return null;
                if (warning) { float wait = Time.realtimeSinceStartup + 8; while (owner.Simulation.Boss == null && Time.realtimeSinceStartup < wait) yield return null; }
                var director = comet || warning ? null : new FxDirector(new GameObject("FxPreview").transform);
                if (comet) owner.Simulation.PrimeAwakeningForCapture();
                string dir = Path.Combine(root, rule); Directory.CreateDirectory(dir);
                Time.captureFramerate = 30;
                float next = 0; var random = new System.Random(7);
                for (int frame = 0; frame < 180; frame++)
                {
                    owner.Simulation.Champion.Health = owner.Simulation.Champion.Definition.Health;
                    if (comet) { float a = frame / 30f * 1.6f; owner.Simulation.Champion.PreviousPosition = owner.Simulation.Champion.Position; owner.Simulation.Champion.Position = new Vector2(Mathf.Cos(a) * 3.2f, Mathf.Sin(a) * 1.8f - .5f); } // the hero runs a loop so the stars fall along its trail
                    float now = owner.Simulation.Elapsed;
                    if (warning && frame % 60 == 0 && owner.Simulation.Boss != null && owner.Simulation.Boss.Alive) owner.Simulation.BossTelegraphCaptureFixture(); // a fresh warning every 2 s
                    if (director != null)
                    {
                        if (now >= next)
                        {
                            var foes = owner.Simulation.Units.Where(u => u.Alive && u.Faction == Faction.Enemy && Mathf.Abs(u.Position.x) < 7 && Mathf.Abs(u.Position.y) < 4).ToList();
                            Vector2 spot = foes.Count > 0 ? foes[random.Next(foes.Count)].Position : owner.Simulation.Champion.Position + new Vector2((float)random.NextDouble() * 6 - 3, (float)random.NextDouble() * 3 - 1.5f);
                            director.Spawn(new FxMoment(rule, spot, 1.15f), now);
                            next = now + .9f;
                        }
                        director.Draw(now);
                    }
                    yield return new WaitForEndOfFrame();
                    Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
                    File.WriteAllBytes(Path.Combine(dir, frame.ToString("0000") + ".png"), image.EncodeToPNG()); Destroy(image);
                }
                Time.captureFramerate = 0;
                File.AppendAllText(Path.Combine(root, "log.txt"), rule + ": 180 frames, awakening " + owner.Simulation.HeroAwakening + ", glyphs with fx " + owner.Simulation.HeroIdentityGlyphs.Count(g => g.Fx != null) + "\n");
            }
            Application.Quit(0);
        }
    }
}
