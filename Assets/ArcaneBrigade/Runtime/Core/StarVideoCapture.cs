using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Records every star-up flourish in the real shop: the half star (1.5), 2 stars, 2.5, 3 stars and the two that go past
    /// it (the altar's fusion and the lucid rite), each with a caption. Run with -arcane-k7-capture -arcane-star-video -capturePath DIR
    /// -ffmpegPath ffmpeg.exe -videoPath out.mp4 at 1920x1080 (the framebuffer recorder needs it).</summary>
    public sealed class StarVideoCapture : MonoBehaviour
    {
        public static bool Requested => RunPaths.K7Capture && Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-star-video") >= 0;
        private string caption = "";
        private GUIStyle style;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<StarVideoCapture>() != null) return;
            var go = new GameObject("StarVideoCapture"); DontDestroyOnLoad(go); go.AddComponent<StarVideoCapture>();
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(caption)) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            Matrix4x4 keep = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            Rect r = new Rect(360, 40, 1200, 70);
            style.normal.textColor = new Color(0, 0, 0, .8f); GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), caption, style);
            style.normal.textColor = new Color(1f, .9f, .6f); GUI.Label(r, caption, style);
            GUI.matrix = keep;
        }

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            Application.runInBackground = true;
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            string unit = "TeapotPilgrim", weapon = "KeyCleaver";
            if (Array.Find(catalog.Companions, u => u.Id == unit) == null) unit = catalog.Companions[0].Id;
            if (catalog.Weapon(weapon) == null) weapon = catalog.Weapons[0].Id;
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 271027); session.State.Wave = 3; session.State.Phase = SessionPhase.Shop;
                session.State.Roster.Clear(); session.State.Roster.Add(new RosterEntry { UnitId = unit, WeaponId = weapon, Tier = 3 });
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(2.6f);
            var owner = FindAnyObjectByType<ArenaController>();
            if (owner == null || owner.Shop == null || owner.Shop.Reveal == null) { Application.Quit(1); yield break; }
            MergeReveal reveal = owner.Shop.Reveal;
            Vector2 at = owner.Simulation.Champion.Position + Vector2.up * .8f + Vector2.right * 1.6f; // beside the hero, clear of the shopkeeper
            RosterEntry Entry(int tier) => new RosterEntry { UnitId = unit, WeaponId = weapon, Tier = tier };
            List<RosterEntry> Materials(int tier, int count) { var list = new List<RosterEntry>(); for (int i = 0; i < count; i++) list.Add(Entry(tier)); return list; }

            var recorder = gameObject.AddComponent<K7FramebufferRecorder>(); recorder.Begin(53);
            File.WriteAllText(Path.Combine(RunPaths.Root, "StarVideoReady.txt"), "recording");
            yield return new WaitForSecondsRealtime(1.2f);
            caption = "1.5 stars: two of three copies"; StarBurst.Play(at, 1.5f, false); yield return new WaitForSecondsRealtime(3.0f);
            caption = "2 stars: three copies merge"; reveal.Stage(Entry(2), Materials(1, 3), at); yield return new WaitForSecondsRealtime(5.0f);
            caption = "2.5 stars: two of three 2-star copies"; StarBurst.Play(at, 2.5f, false); yield return new WaitForSecondsRealtime(3.0f);
            caption = "3 stars: three 2-star copies merge"; reveal.Stage(Entry(3), Materials(2, 3), at); yield return new WaitForSecondsRealtime(7.0f);
            caption = "3.5 stars: two of three 3-star copies"; StarBurst.Play(at, 3.5f, false); yield return new WaitForSecondsRealtime(3.0f);
            caption = "4 stars (purple): three 3-star copies merge"; reveal.Stage(Entry(4), Materials(3, 3), at); yield return new WaitForSecondsRealtime(11.0f);
            caption = "5 stars (rainbow): a Rainbow Thread"; reveal.Stage(Entry(5), Materials(4, 3), at); yield return new WaitForSecondsRealtime(17.5f);
            caption = "";
            while (!recorder.Complete) yield return null;
            File.WriteAllText(Path.Combine(RunPaths.Root, "StarVideoDone.txt"), recorder.Passed ? "PASS" : "FAIL");
            Application.Quit(recorder.Passed ? 0 : 1);
        }
    }
}
