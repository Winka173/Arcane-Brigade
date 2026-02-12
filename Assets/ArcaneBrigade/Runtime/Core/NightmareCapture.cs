using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class NightmareCapture : MonoBehaviour
    {
        private string root;
        private bool failed;
        private ArenaController finalOwner;
        private void Update()
        {
            if (finalOwner != null && finalOwner.Session.State.Phase == SessionPhase.Combat) finalOwner.Simulation.Champion.Health = finalOwner.Simulation.Champion.Definition.Health;
        }
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K7Capture || Array.IndexOf(Environment.GetCommandLineArgs(),"-arcane-nightmare-capture") < 0 || FindAnyObjectByType<NightmareCapture>() != null) return;
            var runner = new GameObject("NightmareNativeCapture"); DontDestroyOnLoad(runner); runner.AddComponent<NightmareCapture>();
        }
        private void Check(bool valid,string text)
        {
            failed |= !valid; notes.Add((valid ? "PASS " : "FAIL ") + text); File.WriteAllLines(Path.Combine(root,"Native_Checks.txt"),notes);
        }
        private void Log(string message,string trace,LogType kind)
        {
            if (kind != LogType.Exception && kind != LogType.Error && kind != LogType.Assert) return;
            Check(false,message); Application.Quit(1);
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
            Color32[] pixels = frame.GetPixels32(); int bright = 0;
            for (int index = 0; index < pixels.Length; index += 347) if (pixels[index].r + pixels[index].g + pixels[index].b > 45) bright++;
            Check(bright > 200,"Nonblank native frame " + name);
            File.WriteAllBytes(Path.Combine(root,name + ".png"),frame.EncodeToPNG()); Destroy(frame);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args,"-capturePath"); if (path < 0 || path + 1 >= args.Length) yield break;
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root); Application.runInBackground = true; Application.logMessageReceived += Log;
            bool reduced = Array.IndexOf(args,"-arcane-nightmare-reduced") >= 0;
            var storage = new RunStorage(RunPaths.Root); storage.SaveProfile(new PlayerProfile { ReducedMotion = reduced,ReducedEffects = reduced,ReducedFlashes = reduced });
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            yield return new WaitForSecondsRealtime(1);
            foreach (int wave in new[] { 1,4,5,8,9,12,13,17,19 })
            {
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight",271015); session.State.Wave = wave; session.State.Phase = SessionPhase.Shop; session.State.Gold = 1000;
                    session.Reroll(); session.State.Secrets.RescuedToy = wave >= 9; session.State.Secrets.LetterSent = wave >= 13;
                    session.State.Nightmare.Choices[0] = 0; session.State.Nightmare.ChoiceMask = session.State.Nightmare.ResolvedMask = 1;
                    storage.SaveRun(session.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(3);
                ArenaController owner = FindAnyObjectByType<ArenaController>();
                // the shop layout leaves out what has no free spot in this small room, so count what it kept
                Check(owner.GetComponent<NightmarePresentation>().VisibleLandmarks >= 3,"Actual illustrated depth landmarks with clear shop exit " + wave);
                Check(owner.GetComponent<NightmareGrandStage>().VisibleCount >= NightmareGrandStage.Capacity / 2, "Persistent world scenery is present from early waves " + wave);
                Check(owner.Shop.Dialogue.CurrentLine != null,"Merchant has depth dialogue " + wave);
                NightmareLighting lighting = owner.GetComponent<NightmareLighting>();
                Check(lighting != null && Shader.GetGlobalFloat("_DreamEnabled") == 1, "Native floor lighting enabled " + wave);
                Check(Mathf.Abs(lighting.Depth - NightmareRules.Depth(wave)) < .35f, "Native lighting follows depth " + wave);
                Vector2 lamp = NightmareLighting.LandmarkPosition(1, true, owner.WorldBounds * .5f) + Vector2.up * .6f;
                Color spill = lighting.Spill(lamp);
                Check(spill.r > spill.b && spill.r > .5f, "Warm lamp contributes local light " + wave);
                Check(owner.GetComponent<NightmareAtmosphere>().VisibleCount == (reduced || wave == 19 ? 0 : Mathf.Min(18, 4 + NightmareRules.Depth(wave) * 3)), "Bounded atmosphere respects Reduced " + wave);
                Check(owner.GetComponent<NightmareHaunting>().Density > 0 && owner.GetComponent<NightmareHaunting>().Density <= .14f, "Actual low fog uses a bounded density " + wave);
                Check(owner.GetComponentsInChildren<NightmarePropPuppet>().Length == 8, "Articulated landmarks are bound " + wave);
                yield return Shot("depth-" + wave);
                if (wave == 19)
                {
                    Check(NightmareHaunting.Quiet, "Last-shop quiet beat precedes the Customer");
                    yield return new WaitForSecondsRealtime(1.5f);
                    Check(!NightmareHaunting.Quiet, "Quiet beat ends without blocking shopping or controls");
                    yield return Shot("last-bell-after");
                }
                if (wave == 9)
                {
                    foreach (WorldShop.Station station in owner.Shop.Stations)
                        if (station.Offer != null)
                        {
                            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = station.Position + Vector2.down * .35f;
                            break;
                        }
                    yield return new WaitForSecondsRealtime(.3f);
                    Check(owner.Shop.Nearby?.Offer != null, "Native offer focus for tooltip layout");
                    yield return Shot("shop-tooltip");
                    owner.TogglePause(); float lightClock = Shader.GetGlobalFloat("_DreamClock"); float fogClock = owner.GetComponent<NightmareHaunting>().Clock;
                    yield return new WaitForSecondsRealtime(.2f);
                    Check(Shader.GetGlobalFloat("_DreamClock") == lightClock, "Pause freezes lighting and decorative clock");
                    Check(owner.GetComponent<NightmareHaunting>().Clock == fogClock, "Pause freezes drifting fog");
                    owner.TogglePause();
                }
                if (wave == 4 || wave == 8 || wave == 12)
                {
                    owner.NextWave();
                    StageTransfer transfer = FindAnyObjectByType<StageTransfer>();
                    Check(transfer != null && transfer.Active, "Actual next-wave carrier begins " + wave);
                    yield return new WaitForSecondsRealtime(reduced ? .1f : .65f);
                    yield return Shot("carrier-grip-" + wave);
                    yield return new WaitForSecondsRealtime(reduced ? .15f : .85f);
                    yield return Shot("carrier-flight-" + wave);
                    yield return new WaitForSecondsRealtime(reduced ? .5f : 1.3f);
                    Check(!owner.TransitionPending && owner.Session.State.Wave == wave + 1, "Carrier completes seamless map transfer " + wave);
                }
                SceneManager.LoadScene("RunFrontend"); yield return null;
            }
            using (var session = new RunSession(catalog)) { session.Start("LanternKnight",271015); session.State.Wave = catalog.NormalWaves; storage.SaveRun(session.State); }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(.2f);
            yield return Shot("customer-entrance");
            yield return new WaitForSecondsRealtime(1.2f);
            ArenaController final = FindAnyObjectByType<ArenaController>();
            Check(final.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId,"Actual final Customer, no secret gate");
            finalOwner = final; final.Simulation.Champion.NextAttack = 1000;
            foreach (UnitState ally in final.Simulation.Units) if (ally.Faction == Faction.Companion) ally.NextAttack = 1000;
            yield return Shot("customer-arrival");
            Check(final.GetComponent<NightmareGrandStage>().VisibleCount == NightmareGrandStage.Capacity && final.GetComponent<NightmareGrandStage>().Grandeur > .8f, "Wave twenty retains damaged world scenery");
            yield return new WaitForSecondsRealtime(2.4f); yield return Shot("customer-stamp");
            final.Simulation.DefeatCustomerCapture();
            yield return new WaitForSecondsRealtime(.25f);
            Check(final.GetComponent<DeathFeedback>().ActiveCount > 0, "Real fixed-step kill dispatches boss death presentation");
            yield return Shot("customer-death-0");
            yield return new WaitForSecondsRealtime(.65f); yield return Shot("customer-death-1");
            yield return new WaitForSecondsRealtime(.8f); yield return Shot("customer-death-2");
            yield return new WaitForSecondsRealtime(1.1f);
            Check(final.Session.State.Phase == SessionPhase.Victory && final.Session.State.Nightmare.CustomerDefeated,"Native damage, settlement and dawn transition");
            string farewellFork = JsonUtility.ToJson(final.Session.State);
            Check(final.Session.EndingChoicePending, "Actual victory opens a physical farewell choice");
            yield return new WaitForSecondsRealtime(1.4f); yield return Shot("farewell-choice");
            final.TogglePause(); Check(!final.ChooseNightmareEnding(NightmareFarewell.CloseShop), "Paused farewell cannot commit"); final.TogglePause();
            Check(final.ChooseNightmareEnding(NightmareFarewell.CloseShop), "Close-shop choice persists through controller boundary");
            for (int page = 0; page < 4; page++)
            {
                yield return new WaitForSecondsRealtime(1.4f);
                Check(final.NightmareEndingVisible && !final.ResultsReady,"Ending blocks results until completed " + page);
                yield return Shot("ending-" + page);
                if (page == 2)
                {
                    yield return new WaitForSecondsRealtime(2);
                    yield return Shot("party-departure");
                }
                final.AdvanceNightmareEnding();
                if (page == 2) Check(final.GetComponent<NightmarePresentation>().DeparturePartyCount >= 1, "Actual saved party participates in dawn departure");
                if (page == 0)
                {
                    final.TogglePause(); float pausedClock = final.PresentationClock;
                    yield return new WaitForSecondsRealtime(.3f);
                    Check(final.PresentationClock == pausedClock,"Ending pause freezes narrative clock");
                    final.AdvanceNightmareEnding(); Check(final.Session.State.Nightmare.EndingPage == 1,"Paused ending cannot advance");
                    final.TogglePause();
                }
                if (page == 1)
                {
                    SceneManager.LoadScene("RunFrontend"); yield return null;
                    RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                    final = FindAnyObjectByType<ArenaController>(); finalOwner = final;
                    yield return new WaitForSecondsRealtime(.3f);
                    Check(final.Session.State.Nightmare.EndingPage == 2 && final.Session.EndingPending,"Actual scene reload resumes saved ending page");
                }
            }
            Check(final.ResultsReady && !final.Session.EndingPending,"Results revealed after complete ending");
            Check(storage.LoadProfile().NightmareShopClosed, "Closing is remembered without a stat reward");
            int winsBeforePromise = storage.LoadProfile().Wins;
            storage.SaveRun(JsonUtility.FromJson<RunSnapshot>(farewellFork));
            SceneManager.LoadScene("RunFrontend"); yield return null;
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            final = FindAnyObjectByType<ArenaController>(); finalOwner = final;
            yield return new WaitForSecondsRealtime(1.4f);
            Check(final.ChooseNightmareEnding(NightmareFarewell.LeaveLight), "Alternative light promise persists");
            for (int page = 0; page < 5; page++)
            {
                yield return new WaitForSecondsRealtime(page == 4 ? 2.8f : 1.4f);
                Check(final.Session.EndingPending && !final.ResultsReady, "Promise branch retains dawn and epilogue " + page);
                yield return Shot("promise-ending-" + page); final.AdvanceNightmareEnding();
                if (page == 3)
                {
                    Check(!storage.LoadProfile().NightmareLampPromise, "Promise is not credited before afterword completes");
                    SceneManager.LoadScene("RunFrontend"); yield return null;
                    RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
                    final = FindAnyObjectByType<ArenaController>(); finalOwner = final;
                    yield return new WaitForSecondsRealtime(.3f);
                    Check(final.Session.State.Nightmare.EndingPage == 4 && final.Session.State.Nightmare.Farewell == NightmareFarewell.LeaveLight, "Afterword restores beyond legacy page four");
                }
            }
            Check(final.ResultsReady && storage.LoadProfile().NightmareLampPromise, "Promise remembered only after real dawn and afterword");
            Check(storage.LoadProfile().Wins == winsBeforePromise, "Replayed diagnostic fork cannot double-credit a win");
            Application.logMessageReceived -= Log; Application.Quit(failed ? 1 : 0);
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
    }
}
