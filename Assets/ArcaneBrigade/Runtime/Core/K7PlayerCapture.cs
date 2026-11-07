using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K7PlayerCapture : MonoBehaviour
    {
        private readonly List<string> notes = new List<string>();
        private readonly List<string> metrics = new List<string>();
        private bool failed;
        private string root;
        private ArenaController owner;
        private bool bossPortraitCapture;
#if UNITY_STANDALONE_WIN
        [StructLayout(LayoutKind.Sequential)]
        private struct WindowRect { public int Left; public int Top; public int Right; public int Bottom; }
        [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr handle, out WindowRect rectangle);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr handle, out WindowRect rectangle);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr handle, IntPtr after, int left, int top, int width, int height, uint flags);
        private delegate bool WindowVisitor(IntPtr handle, IntPtr parameter);
        [DllImport("user32.dll")] private static extern bool EnumWindows(WindowVisitor visitor, IntPtr parameter);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr handle, out uint process);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr handle, int command);
        private static IntPtr GameWindow()
        {
            IntPtr found = IntPtr.Zero; int identity = System.Diagnostics.Process.GetCurrentProcess().Id;
            EnumWindows((handle, parameter) =>
            {
                GetWindowThreadProcessId(handle, out uint process);
                if (process == identity && GetClientRect(handle, out WindowRect client) && client.Right > 100 && client.Bottom > 100) { found = handle; return false; }
                return true;
            }, IntPtr.Zero);
            return found;
        }
#endif
        public static void ShowDiagnosticWindow()
        {
            if (!RunPaths.K7Capture) return;
#if UNITY_STANDALONE_WIN
            IntPtr handle = GameWindow(); if (handle != IntPtr.Zero) ShowWindow(handle, 4);
#endif
        }
        private IEnumerator Resize(int width, int height) => ResizeDiagnostic(width, height, Check);
        public static IEnumerator ResizeDiagnostic(int width, int height, Action<bool, string> verify)
        {
#if UNITY_STANDALONE_WIN
            IntPtr gameWindow = GameWindow(); if (gameWindow != IntPtr.Zero) ShowWindow(gameWindow, 4);
#endif
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(1);
#if UNITY_STANDALONE_WIN
            if (Screen.width != width || Screen.height != height)
            {
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                {
                    IntPtr handle = GameWindow();
                    if (handle != IntPtr.Zero && GetClientRect(handle, out WindowRect client) && GetWindowRect(handle, out WindowRect outer))
                    {
                        float scaleX = (client.Right - client.Left) / (float)Screen.width;
                        float scaleY = (client.Bottom - client.Top) / (float)Screen.height;
                        SetWindowPos(handle, IntPtr.Zero, 0, 0, Mathf.RoundToInt(width * scaleX) + outer.Right - outer.Left - client.Right + client.Left, Mathf.RoundToInt(height * scaleY) + outer.Bottom - outer.Top - client.Bottom + client.Top, 0x0056);
                    }
                }
                yield return new WaitForSecondsRealtime(1);
            }
#endif
            verify(Screen.width == width && Screen.height == height, "Requested render surface " + width + "x" + height + "; actual=" + Screen.width + "x" + Screen.height);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-localization-capture") >= 0) return;
            if (OctoberCapture.Requested) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-secret-capture") >= 0) return;
            if (BuildMorphCapture.Requested) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-nightmare-capture") >= 0) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-animation-capture") >= 0) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-shop-presentation-capture") >= 0) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k11") >= 0) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k7-shop-video") >= 0) return;
            if (StarVideoCapture.Requested) return;
            if (!RunPaths.K7Capture || FindAnyObjectByType<K7PlayerCapture>() != null) return;
            var runner = new GameObject("K7PlayerCapture"); DontDestroyOnLoad(runner); runner.AddComponent<K7PlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) failed = true; }
        private void Check(bool condition, string caption) { notes.Add((condition ? "PASS " : "FAIL ") + caption); failed |= !condition; }
        private void Update()
        {
            if (owner != null && owner.Session.State.Phase == SessionPhase.Combat)
            {
                float time = owner.Simulation.Elapsed;
                owner.SetPresentationMoveTarget(bossPortraitCapture && owner.Simulation.Boss != null ? owner.Simulation.Boss.Position + Vector2.down * 2 : new Vector2(Mathf.Cos(time * 0.37f) * 5, Mathf.Sin(time * 0.37f) * 3));
            }
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG());
            Check(texture.width == Screen.width && texture.height == Screen.height, "Native full-resolution frame " + name);
            Color32[] pixels = texture.GetPixels32(); int visible = 0;
            for (int pixel = 0; pixel < pixels.Length; pixel += 431) if (pixels[pixel].r + pixels[pixel].g + pixels[pixel].b > 25) visible++;
            Check(visible > 20, "Frame contains rendered content not blank black " + name);
            Destroy(texture); yield return new WaitForSecondsRealtime(0.15f);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || index < 0 || index + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[index + 1]); Directory.CreateDirectory(root);
            bool video = Array.IndexOf(args, "-arcane-k7-video") >= 0;
            yield return Resize(video ? 1920 : 3840, video ? 1080 : 2160);
            RunCatalog content = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            if (!video)
            {
                SceneManager.LoadScene("RunFrontend"); yield return new WaitForSecondsRealtime(0.4f);
                RunMenu menu = FindAnyObjectByType<RunMenu>();
                menu.ConfigureK7Diagnostics(content, new PlayerProfile()); menu.ShowPage(RunMenu.Page.Landing);
                yield return new WaitForSecondsRealtime(0.2f);
                yield return Shot("00-main-menu");
                yield return new WaitForSecondsRealtime(0.25f); menu.NavigateForDiagnostics(1, true);
                yield return new WaitForSecondsRealtime(0.2f);
                Check(menu.CurrentPage == RunMenu.Page.Museum, "Actual GUI navigation skips disabled resume and submits discovered museum without mouse");
                if (Array.IndexOf(args, "-arcane-k8") >= 0)
                {
                    menu.ConfigureK7Diagnostics(content, new PlayerProfile()); menu.ShowPage(RunMenu.Page.Roster);
                    yield return new WaitForSecondsRealtime(0.4f); yield return Shot("00-roster-locked");
                    menu.ConfigureK7Diagnostics(content, new PlayerProfile { BestWave = 20 }); menu.ShowPage(RunMenu.Page.Roster);
                    yield return new WaitForSecondsRealtime(0.4f); yield return Shot("00-roster-unlocked");
                    Check(content.Champions.Length == 3 && content.Bosses.Length == 5, "K8 authored distinct champion and boss identities loaded");
                }
                foreach (int bundle in new[] { 0, 1, 2 })
                {
                    var profile = new PlayerProfile { Cosmetic = bundle, BestWave = 10, HighReadability = bundle == 2, ReducedMotion = bundle == 2 };
                    menu.ConfigureK7Diagnostics(content, profile); menu.ShowPage(RunMenu.Page.Settings); yield return new WaitForSecondsRealtime(0.25f);
                    yield return Shot("01-settings-bundle-" + bundle);
                }
                foreach (Vector2 size in new[] { new Vector2(2560, 1080), new Vector2(1600, 1000), new Vector2(1334, 750) })
                {
                    yield return Resize((int)size.x, (int)size.y);
                    menu.ShowPage(RunMenu.Page.Landing); yield return Shot("02-aspect-" + (int)size.x + "x" + (int)size.y);
                }
                yield return Resize(3840, 2160);
            }
            int[] counts = video ? new[] { 24 } : new[] { 1, 56, 100 };
            if (!video && Array.IndexOf(args, "-arcane-k10") >= 0) counts = new[] { 1, 39, 56, 100 };
            foreach (int count in counts)
            {
                using (var session = new RunSession(content))
                {
                    session.Start(content.Champions[0].Id, 7707); session.State.Wave = count == 1 ? 1 : count == 100 ? 31 : 11;
                    session.State.Endless = count == 100; session.State.StartHall = count == 100 ? 21 : 1;
                    session.State.Roster.Clear();
                    for (int rosterIndex = 0; rosterIndex < count; rosterIndex++)
                        session.State.Roster.Add(new RosterEntry { UnitId = content.Companions[Array.IndexOf(args, "-arcane-k10") >= 0 ? rosterIndex % content.Companions.Length : rosterIndex / content.Weapons.Length % content.Companions.Length].Id, WeaponId = content.Weapons[rosterIndex % content.Weapons.Length].Id, Tier = count == 100 ? 2 : 1 });
                    foreach (ItemDefinition item in content.Items) if (item.SignatureRole != SignatureRelicKind.None) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                    session.Validate(session.State); storage.SaveRun(session.State);
                }
                storage.SaveProfile(new PlayerProfile()); RunLaunch.Resume = true;
                SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.8f);
                owner = FindAnyObjectByType<ArenaController>();
                Check(owner.Session.State.Roster.Count == count, "Actual uncapped rendered roster " + count);
                yield return new WaitForSecondsRealtime(3);
                if (video)
                {
                    K7FramebufferRecorder recorder = gameObject.AddComponent<K7FramebufferRecorder>(); recorder.Begin(15);
                    File.WriteAllText(Path.Combine(root, "VideoReady.txt"), "Only this game window may be captured; nativeTimeScale=" + Time.timeScale);
                    yield return new WaitForSecondsRealtime(4);
                    foreach (LineRenderer line in FindObjectsByType<LineRenderer>())
                        if (line.enabled && line.loop && line.positionCount >= 24)
                            notes.Add("Visible circular line: " + line.name + "; bounds=" + line.bounds + "; parent=" + line.transform.parent.name);
                    yield return new WaitForSecondsRealtime(18);
                    Check(recorder.Passed, "Realtime Unity framebuffer movie completed without OS overlays");
                }
                else
                {
                    using (var probe = new K7NativeMetrics())
                    {
                        probe.Begin(); double start = Time.realtimeSinceStartupAsDouble;
                        while (Time.realtimeSinceStartupAsDouble - start < 12) { yield return null; probe.Record(owner.Simulation); }
                        metrics.Add("Surface=" + Screen.width + "x" + Screen.height + "; development=" + Debug.isDebugBuild + "; " + probe.Write(root, "roster-" + count));
                    }
                }
                yield return Shot("03-combat-roster-" + count);
                Check(Time.timeScale == 1, "No timeScale acceleration in native run " + count);
                Check(owner.Session.State.Phase == SessionPhase.Combat, "Measurement remained in live combat " + count);
                owner.ToggleDetails(); yield return new WaitForSecondsRealtime(0.3f); yield return Shot("04-details-roster-" + count);
                if (count == 39 && Array.IndexOf(args, "-arcane-k10") >= 0)
                {
                    Check(owner.Content.Companions.Length == 39 && owner.Content.Weapons.Length == 40, "K10 full roster and 24 new weapon definitions loaded in player");
                    Check(GlossaryUI.Show("split") && GlossaryUI.Open, "K10 contextual term route opens glossary in player");
                    yield return Shot("04-k10-glossary"); GlossaryUI.Close();
                }
                owner.ToggleDetails();
                owner = null;
            }
            if (!video && Array.IndexOf(args, "-arcane-k8") >= 0)
            {
                for (int bossIndex = 0; bossIndex < content.Bosses.Length; bossIndex++)
                {
                    using (var session = new RunSession(content))
                    {
                        session.Start(content.Champions[0].Id, 88100 + bossIndex); session.State.Wave = (bossIndex + 1) * 4;
                        session.State.Roster.Clear();
                        for (int companion = 0; companion < 7; companion++) session.State.Roster.Add(new RosterEntry { UnitId = content.Companions[companion].Id, WeaponId = content.Weapons[companion].Id, Tier = 2 });
                        session.Validate(session.State); storage.SaveRun(session.State);
                    }
                    RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(1.2f);
                    owner = FindAnyObjectByType<ArenaController>();
                    Check(owner.Simulation.Boss != null && owner.Simulation.Boss.Definition.Body.texture.name == "BossesAndProps_v1", "Native real boss uses distinct imported source " + bossIndex);
                    bossPortraitCapture = true;
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = owner.Simulation.Boss.Position + Vector2.down * 2;
                    FindAnyObjectByType<ArenaCamera>().Snap();
                    yield return new WaitForSecondsRealtime(0.45f);
                    yield return Shot("17-boss-" + bossIndex);
                    bossPortraitCapture = false;
                    owner = null;
                }
            }
            if (!video)
            {
                using (var session = new RunSession(content))
                {
                    session.Start(content.Champions[0].Id, 4313); session.State.Phase = SessionPhase.Shop;
                    session.State.Gold = 10000; session.State.Roster.Clear(); session.State.Offers.Clear(); session.State.SpecialOffers.Clear();
                    string identity = content.Companions[0].Id;
                    for (int indexRoster = 0; indexRoster < 2; indexRoster++) session.State.Roster.Add(new RosterEntry { UnitId = identity, WeaponId = content.Weapons[indexRoster].Id });
                    session.State.Offers.Add(new ShopOffer { Token = 1, Id = identity, Kind = OfferKind.Companion, Price = 1 });
                    session.State.Offers.Add(new ShopOffer { Token = 2, Id = content.Companions[1].Id, Kind = OfferKind.Companion, Price = 1 });
                    session.State.Offers.Add(new ShopOffer { Token = 3, Id = content.Items[0].Id, Kind = OfferKind.Item, Price = 1 });
                    session.State.NextOffer = 3; session.Validate(session.State); storage.SaveRun(session.State);
                    if (RunPaths.K7B)
                    {
                        session.State.Eggs = 10; session.State.SpecialShopVisited = true; // gold fixtures first; the dream stage is shown below
                        for (int rarity = 1; rarity <= 3; rarity++)
                        {
                            ItemDefinition item = Array.Find(content.Items, candidate => candidate.Rarity == (ItemRarity)rarity && !Array.Exists(candidate.EffectiveModifiers, modifier => modifier.Effect == ItemEffect.Health));
                            session.State.SpecialOffers.Add(new ShopOffer { Token = ++session.State.NextOffer, Id = item.Id, Kind = OfferKind.Item, Price = rarity == 2 ? 5 : 1, Currency = rarity == 2 ? ShopCurrency.Health : ShopCurrency.Eggs });
                        }
                        session.Validate(session.State); storage.SaveRun(session.State);
                    }
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.8f);
                owner = FindAnyObjectByType<ArenaController>();
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-3, 0.5f);
                yield return new WaitForSecondsRealtime(0.2f); yield return Shot("05-shop-entry");
                int entryLines = owner.Shop.Dialogue.SpokenCount;
                Check(owner.Shop.Dialogue.LastMoment == MerchantMoment.Enter, "Merchant entry speaks without popup or purchase");
                if (Array.IndexOf(args, "-arcane-k9") >= 0)
                    Check(owner.Shop.PlayedSpeechSyllables > 0, "K9 merchant cadence actually dispatches syllables on dedicated audio lane");
                yield return new WaitForSecondsRealtime(2.35f); yield return Shot("05-shop-focus");
                Check(owner.Shop.Dialogue.LastMoment == MerchantMoment.Inspect && owner.Shop.Dialogue.SpokenCount > entryLines, "Approaching an offer produces merchant conversation automatically");
                Check(owner.Shop.Interact(), "Actual shop proximity purchase commits and queues merge");
                Check(owner.Shop.Dialogue.LastMoment == MerchantMoment.Buy, "Only committed purchase produces merchant sale dialogue");
                yield return new WaitForSecondsRealtime(0.15f); yield return Shot("06-merge-gather");
                yield return new WaitForSecondsRealtime(0.28f); yield return Shot("07-merge-payoff");
                yield return new WaitForSecondsRealtime(0.7f);
                Check(owner.Shop.Reveal.CompletedCount == 1 && owner.Session.State.Roster.Count == 1 && owner.Session.State.Roster[0].Tier == 2, "Merge completes from actual committed roster not fake message");
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(0, 0.5f);
                yield return new WaitForSecondsRealtime(0.15f);
                Check(owner.Shop.Interact(), "Actual unmerged recruit purchase commits");
                yield return new WaitForSecondsRealtime(0.12f); yield return Shot("08-recruit-hop");
                yield return new WaitForSecondsRealtime(0.9f); yield return Shot("09-recruit-walkout");
                if (Array.IndexOf(args, "-arcane-k9") >= 0)
                {
                    Check(Array.TrueForAll(content.Companions, unit => unit.CompanionModifiers.Length >= 2), "K9 authored positive and negative companion traits loaded in player");
                    owner.ToggleDetails(); owner.Details.SelectTab(1);
                    yield return new WaitForSecondsRealtime(0.3f); yield return Shot("09-k9-companion-tradeoffs");
                    int spoken = owner.Shop.PlayedSpeechSyllables;
                    yield return new WaitForSecondsRealtime(0.2f);
                    Check(owner.Shop.PlayedSpeechSyllables == spoken, "K9 paused inventory does not emit dialogue audio");
                    owner.ToggleDetails();
                }
                if (RunPaths.K7B)
                {
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(3, 0.5f);
                    owner.Session.State.Gold = 0; yield return new WaitForSecondsRealtime(0.15f);
                    int receipts = owner.Session.State.NextReceipt;
                    Check(!owner.Shop.Interact() && owner.Shop.DeniedCount == 1 && owner.Session.State.NextReceipt == receipts, "Insufficient gold shakes/flashes without committing or consuming item");
                    yield return new WaitForSecondsRealtime(0.12f); yield return Shot("10-denied-purchase");
                    owner.Session.State.Gold = 10000; Check(owner.Shop.Interact(), "Actual item purchase commits");
                    yield return new WaitForSecondsRealtime(0.12f); yield return Shot("11-item-flight");
                    yield return new WaitForSecondsRealtime(1); Check(owner.Shop.ItemPickup.CompletedCount == 1, "Committed item arrives at player with animated flight");
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-4.8f, 2.8f);
                    yield return new WaitForSecondsRealtime(0.15f); Check(owner.Shop.Interact(), "Dedicated machine rerolls actual stock");
                    yield return new WaitForSecondsRealtime(0.12f); yield return Shot("12-reroll-machine");
                    owner.Session.State.SpecialShopVisited = false; owner.Shop.Display(); // fixture: back to the dream stage (it comes first in a real night)
                    yield return new WaitForSecondsRealtime(0.15f); Check(owner.Shop.Special, "Dream market stage holds the special stock (two stages, no switch station)");
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = Vector2.zero;
                    yield return new WaitForSecondsRealtime(0.3f); yield return Shot("13-special-rarity-lights");
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(0, 0.5f);
                    float healthBefore = owner.Session.State.Health; yield return new WaitForSecondsRealtime(0.15f);
                    Check(owner.Shop.Interact() && owner.Session.State.Health == healthBefore - 5 && owner.Session.State.Health >= 1, "Special item charges currentHP not maxHP and leaves at least1HP");
                    yield return new WaitForSecondsRealtime(0.15f); yield return Shot("14-special-hp-purchase");
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-3, 0.5f);
                    int eggsBefore = owner.Session.State.Eggs; yield return new WaitForSecondsRealtime(0.15f);
                    Check(owner.Shop.Interact() && owner.Session.State.Eggs == eggsBefore - 1, "Special merchant also charges separate egg currency");
                    yield return new WaitForSecondsRealtime(0.85f);
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(0, -4.3f);
                    yield return new WaitForSecondsRealtime(0.2f); Check(owner.Shop.Interact() && !owner.HallExitPending, "Dream exit leads on to the gold shop, not the next wave");
                    yield return new WaitForSecondsRealtime(MarketMotion.HandoffDuration + 0.3f); Check(!owner.Shop.Special && owner.Session.State.SpecialShopVisited, "Gold shop follows the dream market");
                    yield return new WaitForSecondsRealtime(0.2f); Check(owner.Shop.Interact() && owner.HallExitPending, "Downward door starts committed hall travel presentation");
                    yield return new WaitForSecondsRealtime(0.3f); yield return Shot("15-downward-exit");
                    yield return new WaitForSecondsRealtime(1.5f); Check(owner.Session.State.Wave == 2 && owner.Session.State.Phase == SessionPhase.Combat, "Curtain finishes into actual wave2 without duplicate advance");
                    yield return Shot("16-next-wave");
                }
                owner = null;
            }
            notes.Add("Windows fixture seed7707, native resolution3840x2160 (video1920x1080), timeScale1, movement-only circle driver, seeded1/56/100 (K10:1/39/56/100) unique body+weapon roster and relics. No HP/invulnerability/weapon/enemy timer adjustments. Native instrumentation captures real Unity frame/memory/GC/GPU where counters exist; NA means unavailable, not zero-cost. CSV/summary measured before screenshots and without video encoder. Not natural20hall balance/human feel/device approval. Aspect screenshots are desktop resizes, not device safe-area evidence. Video flag leaves22seconds for external game-window-only60fps recording; separate audio audition, no microphone/desktop capture.");
            metrics.Insert(0, "Device=" + SystemInfo.graphicsDeviceName + "; backend=" + SystemInfo.graphicsDeviceType + "; CPU=" + SystemInfo.processorType + "; RAM=" + SystemInfo.systemMemorySize + "MiB; OS=" + SystemInfo.operatingSystem);
            File.WriteAllLines(Path.Combine(root, "Native_Metrics.txt"), metrics); File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes);
            Debug.Log(failed ? "K7_NATIVE_CAPTURE_FAILED" : "K7_NATIVE_CAPTURE_PASSED"); Application.Quit(failed ? 1 : 0);
        }
    }
}
