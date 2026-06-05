using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Screens of the front menus for review (24/10/2026): -arcane-menu-review -capturePath DIR. The boot loading screen,
    /// the main menu, hero select, How to Play, Settings and the Collection, one shot each, then quits.</summary>
    public sealed class MenuReviewCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-menu-review") >= 0;
        private string root;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<MenuReviewCapture>() != null) return;
            var go = new GameObject("MenuReviewCapture"); DontDestroyOnLoad(go); go.AddComponent<MenuReviewCapture>();
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-capturePath");
            root = at >= 0 && at + 1 < args.Length ? args[at + 1] : Path.Combine(Application.persistentDataPath, "MenuReview");
            Directory.CreateDirectory(root);
            yield return new WaitForSecondsRealtime(.5f);
            yield return Shot("01-loading");
            yield return new WaitForSecondsRealtime(2.6f);
            yield return Shot("02-main-menu");
            RunMenu menu = FindAnyObjectByType<RunMenu>();
            if (menu != null)
            {
                menu.ShowPage(RunMenu.Page.Roster); yield return new WaitForSecondsRealtime(.8f); yield return Shot("03-hero-select");
                menu.ShowPage(RunMenu.Page.Help); yield return new WaitForSecondsRealtime(1.2f); yield return Shot("04-how-to-play");
                menu.ShowPage(RunMenu.Page.Settings); yield return new WaitForSecondsRealtime(.6f); yield return Shot("05-settings");
                menu.ShowPage(RunMenu.Page.Museum); yield return new WaitForSecondsRealtime(.6f); yield return Shot("06-collection");
                // the Waiting Room and its Lamp Tree (a few lanterns lit, one fork taken, the third branch still locked)
                var meta = new PlayerProfile { TutorialSeen = true, BestWave = 40 }; MetaProgression.Ensure(meta);
                meta.Meta.LampOil = 2000; meta.Meta.Keepsakes = 40;
                foreach (string id in new[] { "hero.wick", "hero.stride", "hero.reach", "hero.spark", "hero.wick2", "friends.stork", "friends.cradle" }) MetaProgression.Light(meta, id);
                meta.Meta.LampOil = 180; meta.Meta.Keepsakes = 7;
                menu.ConfigureHubDiagnostics(meta);
                menu.ShowPage(RunMenu.Page.Hub); menu.Hub.Approach(MetaStation.Lantern); yield return new WaitForSecondsRealtime(.8f); yield return Shot("07-hub");
                menu.Hub.OpenStation(MetaStation.Lantern); yield return new WaitForSecondsRealtime(.6f); yield return Shot("08-lamp-tree");
            }
            Application.Quit(0);
        }
    }
}
