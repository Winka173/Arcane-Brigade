using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native screenshots of the Act I story systems: the warm chair in the shop, its warning, a journal choice
    /// and the night you hid. Uses the isolated diagnostics save. Run with -arcane-actone-capture -capturePath DIR.</summary>
    public sealed class ActOneCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-actone-capture") >= 0;
        private string root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<ActOneCapture>() != null) return;
            var go = new GameObject("ActOneCapture"); DontDestroyOnLoad(go); go.AddComponent<ActOneCapture>();
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
            // A: shop 8, the Last Customer's offer, then the long table with a stranger and the travelling four
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 161059); session.State.Wave = 8; session.State.Phase = SessionPhase.Shop; session.State.Gold = 2000;
                session.State.Roster.Clear();
                foreach (string id in new[] { "BrassAcorn", "PillowOracle", "HoodWisp" }) if (catalog.Unit(id) != null) session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = catalog.Unit(id).Weapon.Id });
                session.State.MienCho.ReadMask = (1 << MienChoStory.Available(session.State)) - 1;
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2.8f);
            var first = FindAnyObjectByType<ArenaController>();
            yield return Shot("05-customer-offer");
            first.SellMemory(first.Session.State.Roster[0].UnitId); yield return new WaitForSecondsRealtime(.6f);
            first.ToggleDetails(); first.Details.SelectTab(1); yield return new WaitForSecondsRealtime(.5f); yield return Shot("06-brigade-stranger-and-four");
            first.ToggleDetails(); yield return new WaitForSecondsRealtime(.2f);
            // B: shop 10, the warm chair
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 161060); session.State.Wave = 10; session.State.Phase = SessionPhase.Shop; session.State.Gold = 2000;
                session.State.MienCho.ReadMask = (1 << MienChoStory.Available(session.State)) - 1;
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2.8f);
            var owner = FindAnyObjectByType<ArenaController>();
            var chair = owner.GetComponent<WaitingRealmStage>();
            yield return Shot("00-shop-wave10-chair");
            var seat = GameObject.Find("TheUnoccupiedChair")?.GetComponent<SpriteRenderer>();
            File.WriteAllText(Path.Combine(root, "chair.txt"), seat == null ? "missing" : "anchor=" + chair.ChairAnchor + " position=" + seat.transform.position + " enabled=" + seat.enabled + " sprite=" + (seat.sprite == null ? "null" : seat.sprite.name) + " bounds=" + seat.bounds + " order=" + seat.sortingOrder + " color=" + seat.color + " scale=" + seat.transform.lossyScale);
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = chair.ChairAnchor + new Vector2(-.8f, -.5f);
            yield return new WaitForSecondsRealtime(.5f); yield return Shot("01-chair-near");
            chair.AskForDiagnostics(); yield return new WaitForSecondsRealtime(.3f); yield return Shot("02-chair-warning");
            chair.AskForDiagnostics(false);
            var journal = owner.GetComponent<MienChoPresentation>();
            journal.Show(8); yield return new WaitForSecondsRealtime(.4f); yield return Shot("03-journal-telephone-choice");
            journal.Close(); yield return new WaitForSecondsRealtime(.3f);
            owner.AcceptSitCloser();
            float until = Time.realtimeSinceStartup + 12;
            while ((owner.TransitionPending || owner.Simulation.Boss == null) && Time.realtimeSinceStartup < until) yield return null;
            yield return new WaitForSecondsRealtime(4f); yield return Shot("04-the-night-you-hid");
            Application.Quit(0);
        }
    }
}
