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
            Application.runInBackground = true; // keeps shooting when the window is not in front
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
                if (RunPaths.K7Capture)
                {
                    // a locked hero shows its condition (requirement 27/10/2026)
                    menu.SelectChampionForDiagnostics(17); yield return new WaitForSecondsRealtime(.6f); yield return Shot("03b-hero-locked");
                }
                menu.ShowPage(RunMenu.Page.Help); yield return new WaitForSecondsRealtime(1.2f); yield return Shot("04-how-to-play");
                menu.ShowPage(RunMenu.Page.Settings); yield return new WaitForSecondsRealtime(.6f); yield return Shot("05-settings");
                menu.ShowPage(RunMenu.Page.Museum); yield return new WaitForSecondsRealtime(.6f); yield return Shot("06-collection");
                // the Waiting Room and its Lamp Tree (a few lanterns lit, one fork taken, the third branch still locked)
                var meta = new PlayerProfile { TutorialSeen = true, BestWave = 40 }; MetaProgression.Ensure(meta);
                meta.Meta.LampOil = 2000; meta.Meta.Keepsakes = 40;
                foreach (string id in new[] { "hero.wick", "hero.wick", "hero.wick", "hero.stride", "hero.reach", "hero.spark", "hero.wick2", "friends.stork", "friends.stork", "friends.cradle", "friends.stork2" }) MetaProgression.Light(meta, id);
                meta.Meta.WonMask = 0b11111; // five difficulties won: the later branches open too
                foreach (string id in new[] { "shield.plate", "shield.plate", "shield.heart", "forge.edge", "hunt.snare", "shop.haggle" }) MetaProgression.Light(meta, id);
                meta.Meta.LampOil = 180; meta.Meta.Keepsakes = 7;
                menu.ConfigureHubDiagnostics(meta);
                menu.ShowPage(RunMenu.Page.Hub); menu.Hub.Approach(MetaStation.Lantern); yield return new WaitForSecondsRealtime(.8f); yield return Shot("07-hub");
                menu.Hub.OpenStation(MetaStation.Lantern); yield return new WaitForSecondsRealtime(.6f); yield return Shot("08-lamp-tree");
                menu.Hub.PickLantern("shield.plate"); yield return new WaitForSecondsRealtime(.3f); yield return Shot("09-lamp-card");
                menu.Hub.PickLantern("forge.crown"); yield return new WaitForSecondsRealtime(.3f); yield return Shot("10-lamp-card-right");
                menu.Hub.FocusLampBranch(0); yield return new WaitForSecondsRealtime(.3f); yield return Shot("10b-lamp-branch"); // one branch opened across the panel
                menu.Hub.PickLantern("hero.wick"); yield return new WaitForSecondsRealtime(.3f); yield return Shot("10c-lamp-branch-card"); menu.Hub.FocusLampBranch(-1);
                // the Postbox (two letters read, one open) and the Wardrobe's arrows
                meta.SeenBosses.Add("Boss"); meta.Wins = 1; meta.BestWave = 40;
                foreach (MetaLetter l in MetaProgression.Letters) MetaProgression.ReadLetter(meta, l.Id);
                menu.Hub.Close(); menu.Hub.OpenStation(MetaStation.Postbox); yield return new WaitForSecondsRealtime(.6f); yield return Shot("11-postbox");
                menu.Hub.Close(); menu.Hub.OpenStation(MetaStation.Wardrobe); yield return new WaitForSecondsRealtime(.6f); yield return Shot("12-wardrobe");
            }
            Application.Quit(0);
        }
    }
}
