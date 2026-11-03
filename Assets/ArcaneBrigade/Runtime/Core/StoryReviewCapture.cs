using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Evidence for the Act I story fix (3/11/2026): journal pages of the re-anchored beats with their place and panel,
    /// the story companions standing in the shop, the Chair Porter hurling chairs, the Glasshouse's plants and the two new map
    /// elites. Run with -arcane-story-capture -capturePath DIR (isolated diagnostics save); quits when done.</summary>
    public sealed class StoryReviewCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-story-capture") >= 0;
        private string root;
        private RunCatalog catalog;
        private ArenaController owner;
        private readonly List<string> checks = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<StoryReviewCapture>() != null) return;
            var go = new GameObject("StoryReviewCapture"); DontDestroyOnLoad(go); go.AddComponent<StoryReviewCapture>();
        }

        private void Check(bool pass, string label) { checks.Add((pass ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks); }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG());
            Destroy(image);
        }
        private IEnumerator Load(int wave, bool shop)
        {
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 171026); run.State.Wave = wave; run.State.Phase = shop ? SessionPhase.Shop : SessionPhase.Combat;
                run.State.Gold = 1200; run.State.Roster.Clear();
                foreach (var u in catalog.Companions.Where(u => !RunSession.StorySpecies(u.Id) && Array.IndexOf(ActOneCanon.StoryIds, u.Id) < 0).Take(5))
                    run.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = u.Weapon.Id });
                new RunStorage(RunPaths.Root).SaveRun(run.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(.7f);
            owner = FindAnyObjectByType<ArenaController>();
            Check(owner?.Session != null, "Saved session loads at " + (shop ? "shop " : "wave ") + wave);
            yield return new WaitForSecondsRealtime(2.5f);
        }
        private IEnumerator Journal(int chapter)
        {
            yield return Load(MienChoStory.Chapters[chapter].Wave, true);
            var journal = owner.GetComponent<MienChoPresentation>();
            journal.Show(chapter); yield return new WaitForSecondsRealtime(.8f);
            yield return Shot("Journal_" + chapter.ToString("00"));
            journal.Close(); yield return new WaitForSecondsRealtime(.3f);
        }
        private IEnumerator Fight(int wave, string name, params float[] at)
        {
            yield return Load(wave, false);
            float started = Time.unscaledTime;
            foreach (float t in at)
            {
                while (Time.unscaledTime - started < t) { if (owner.Simulation?.Champion != null) owner.Simulation.Champion.Health = 1e6f; yield return null; }
                yield return Shot(name + "_" + Mathf.RoundToInt(t).ToString("00"));
            }
        }

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            catalog = Resources.Load<RunCatalog>("RunCatalog");
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"), "Real player saves remain isolated");
            for (int p = 0; p < ActOneCanon.PlacePanel.Length; p++)
                Check(Resources.Load<Texture2D>("MienCho/ActOne/" + ActOneCanon.PlacePanel[p]) != null, "Journal panel " + ActOneCanon.PlacePanel[p]);
            foreach (string chair in new[] { "ThrownChair_Dining", "ThrownChair_Rocking", "ThrownChair_Stool" }) Check(ActOneArtwork.Picture(chair) != null, "Chair Porter prop " + chair);
            Check(catalog.MapElites.Length >= 6 && catalog.MapElites[(int)MapTheme.Glasshouse] != null && catalog.MapElites[(int)MapTheme.Theatre] != null, "Glasshouse and Theatre elites");
            foreach (int chapter in new[] { 3, 4, 5, 8, 9, 11, 12, 14, 15, 16 }) yield return Journal(chapter);
            foreach (int wave in new[] { 4, 9, 18 }) { yield return Load(wave, true); yield return Shot("Shop_" + wave.ToString("00")); }
            yield return Fight(8, "ChairPorter", 8, 16, 24, 32);
            yield return Fight(10, "Glasshouse", 14);
            yield return Fight(11, "GlasshouseElite", 18, 34);
            yield return Fight(17, "TheatreElite", 18, 34);
            Check(true, "Capture finished");
            Application.Quit(checks.All(c => c.StartsWith("PASS ")) ? 0 : 1);
        }
    }
}
