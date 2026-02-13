using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native probe of the dream (special) shop as the game really rolls it. Run with -arcane-special-probe
    /// -capturePath DIR [-probe-wave N] [-probe-run COPY_OF_RUN_JSON] (isolated diagnostics save): stands the hero at each special offer and presses
    /// interact, then at the way out, logging every gate that can swallow the press (secret invitation focus, warm
    /// chair, Customer's card, outside stage, market animation, rest phase) and the departure over the next seconds.</summary>
    public sealed class SpecialShopProbe : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-special-probe") >= 0;
        private string root;
        private readonly List<string> log = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<SpecialShopProbe>() != null) return;
            var go = new GameObject("SpecialShopProbe"); DontDestroyOnLoad(go); go.AddComponent<SpecialShopProbe>();
        }
        private static string Arg(string key) { string[] a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private void Write() => File.WriteAllLines(Path.Combine(root, "SpecialShop.txt"), log);
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception) { log.Add("ERROR " + message + " | " + trace.Split('\n')[0]); Write(); } }

        private string Gates(ArenaController owner)
        {
            var shop = owner.Shop; var s = owner.Session;
            var chair = owner.GetComponent<WaitingRealmStage>(); var card = owner.GetComponent<CustomerOffer>(); var outside = owner.GetComponent<OutsideStageChoice>();
            object leaving = typeof(WorldShop).GetField("leavingSpecial", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(shop);
            return $"phase={s.State.Phase} sim={owner.Simulation.Phase} special={shop.Special} visited={s.State.SpecialShopVisited} leaving={leaving} animating={shop.Animating}"
                + $" nearby={(shop.Nearby == null ? "-" : shop.Nearby.Action)} secretFocus={owner.SecretRoom?.Focused} secretRoom={s.SecretRoomOpen} chair={chair?.Engaged}"
                + $" card={card?.Open} outside={outside?.Focused} transition={owner.TransitionPending} hallExit={owner.HallExitPending} paused={owner.PresentationPaused}"
                + $" eggs={s.State.Eggs} gold={s.State.Gold} notice=\"{s.Notice}\"";
        }
        private IEnumerator Start()
        {
            root = Arg("-capturePath"); if (root == null) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(root); Directory.CreateDirectory(root); Application.runInBackground = true; Application.logMessageReceived += OnLog;
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            int wave = int.TryParse(Arg("-probe-wave"), out int w) ? w : 5;
            string supplied = Arg("-probe-run"); // a copy of a real run.json: reproduce that exact run (the real save is never touched)
            if (supplied != null)
            {
                File.Copy(supplied, Path.Combine(RunPaths.Root, "run.json"), true);
                using (var check = new RunSession(catalog)) log.Add("loaded supplied run: " + (storage.LoadRun(check) != null ? "ok" : "FAILED " + storage.LastError));
            }
            else
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171505); session.State.Wave = wave; session.State.Phase = SessionPhase.Shop; session.State.Eggs = 12; session.State.Gold = 60;
                typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(session, new object[] { false });
                typeof(RunSession).GetMethod("RollSpecialOffers", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(session, null);
                session.State.SpecialShopVisited = false;
                foreach (ShopOffer offer in session.State.SpecialOffers) log.Add($"special offer {offer.Kind} {offer.Id} t{offer.Tier} {offer.Price} {offer.Currency} canBuy={session.CanBuy(offer)}");
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            ArenaController owner = null;
            for (float t = 0; t < 8 && (owner == null || owner.Shop == null || owner.Simulation == null); t += .1f) { yield return new WaitForSecondsRealtime(.1f); owner = FindAnyObjectByType<ArenaController>(); }
            if (owner == null) { log.Add("NO ARENA"); Write(); Application.Quit(1); yield break; }
            for (int i = 0; i < 8; i++) { yield return new WaitForSecondsRealtime(.5f); log.Add($"enter +{(i + 1) * .5f:0.0}s " + Gates(owner)); }
            yield return Shot("00-special-shop");
            var field = typeof(WorldShop).GetField("stations", BindingFlags.Instance | BindingFlags.NonPublic);
            var stations = new List<WorldShop.Station>((IEnumerable<WorldShop.Station>)field.GetValue(owner.Shop));
            foreach (var st in stations) log.Add($"station {st.Action} at {st.Position} reach {st.Reach} offer={(st.Offer == null ? "-" : st.Offer.Id + (st.Offer.Sold ? " SOLD" : ""))}");
            int shot = 1;
            foreach (var st in stations)
            {
                if (st.Offer == null) continue;
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = st.Position + Vector2.down * .4f;
                yield return new WaitForSecondsRealtime(.6f);
                log.Add($"at {st.Offer.Id}: before " + Gates(owner));
                bool done = owner.Shop.Interact();
                yield return new WaitForSecondsRealtime(.4f);
                log.Add($"at {st.Offer.Id}: interact={done} sold={st.Offer.Sold} after " + Gates(owner));
                yield return Shot((shot++).ToString("00") + "-buy-" + st.Offer.Id);
                yield return new WaitForSecondsRealtime(1.2f);
            }
            var exit = stations.Find(s => s.Action == "NEXT HALL");
            if (exit != null)
            {
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = exit.Position + Vector2.up * .3f;
                yield return new WaitForSecondsRealtime(.6f);
                log.Add("at exit: before " + Gates(owner));
                log.Add("at exit: interact=" + owner.Shop.Interact());
                for (int i = 0; i < 16; i++)
                {
                    yield return new WaitForSecondsRealtime(.5f);
                    log.Add($"leave +{(i + 1) * .5f:0.0}s " + Gates(owner));
                    if (i % 4 == 3) yield return Shot((shot++).ToString("00") + "-leave-" + (i + 1) / 2 + "s");
                }
            }
            Write(); Application.Quit(0);
        }
        private void OnDestroy() => Application.logMessageReceived -= OnLog;
    }
}
