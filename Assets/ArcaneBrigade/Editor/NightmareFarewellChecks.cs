using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class NightmareFarewellChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool value, string text) { if (!value) throw new InvalidOperationException("Farewell: " + text); notes.Add("PASS " + text); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            Check(NightmareAssets.Current.ManFrames.Length == 4 && NightmareAssets.Current.PromiseIllustration != null, "Original post-contract man and afterword art imported");
            foreach (UnitDefinition champion in catalog.Champions)
                foreach (NightmareFarewell choice in new[] { NightmareFarewell.CloseShop, NightmareFarewell.LeaveLight })
                {
                    using var session = new RunSession(catalog); session.Start(champion.Id, 271015);
                    Check(!session.ChooseFarewell(choice), "Cannot choose before boss victory " + champion.Id + choice);
                    session.State.Wave = catalog.NormalWaves; session.State.Phase = SessionPhase.Victory;
                    session.State.Nightmare.CustomerDefeated = true; session.State.Nightmare.Ending = NightmareEndingStage.Dawn;
                    uint shop = session.State.ShopRandom, weapon = session.State.WeaponRandom; int gold = session.State.Gold;
                    Check(session.EndingChoicePending && !session.AdvanceEnding(), "Choice cannot be skipped " + champion.Id + choice);
                    Check(session.ChooseFarewell(choice) && !session.ChooseFarewell(choice), "Choice commits once " + champion.Id + choice);
                    int pages = choice == NightmareFarewell.LeaveLight ? 5 : 4;
                    for (int page = 0; page < pages; page++)
                    {
                        using var restored = new RunSession(catalog); restored.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State)));
                        Check(restored.State.Nightmare.Farewell == choice && restored.State.Nightmare.EndingPage == page && restored.EndingPending, "Choice and page restore " + champion.Id + choice + page);
                        session.EnterEndless(); Check(!session.State.Endless, "Cannot bypass branch epilogue " + champion.Id + choice + page);
                        Check(session.AdvanceEnding(), "Branch advances " + champion.Id + choice + page);
                    }
                    Check(!session.EndingPending && session.State.Nightmare.EndingPage == pages && !session.AdvanceEnding(), "Complete once " + champion.Id + choice);
                    Check(session.State.ShopRandom == shop && session.State.WeaponRandom == weapon && session.State.Gold == gold, "Story choice does not spend currency or RNG " + champion.Id + choice);
                    session.Validate(session.State);
                    session.EnterEndless(); session.Validate(session.State);
                    Check(session.State.Endless, "Endless remains separate after either ending " + champion.Id + choice);
                }
            using (var legacy = new RunSession(catalog))
            {
                legacy.Start(catalog.Champions[0].Id, 42); legacy.State.Phase = SessionPhase.Victory;
                legacy.State.Nightmare = new NightmareProgress { CustomerDefeated = true, Ending = NightmareEndingStage.Dawn, EndingPage = 2 };
                legacy.Validate(legacy.State);
                Check(!legacy.EndingChoicePending && legacy.AdvanceEnding() && legacy.AdvanceEnding() && !legacy.EndingPending, "Legacy in-progress dawn is not forced backward");
            }
            using (var invalid = new RunSession(catalog))
            {
                invalid.Start(catalog.Champions[0].Id, 42);
                invalid.State.Nightmare.Farewell = NightmareFarewell.LeaveLight;
                bool rejected = false; try { invalid.Validate(invalid.State); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "Forged promise before victory rejected");
            }
            Check(NightmareHaunting.FogFor(0, false) < NightmareHaunting.FogFor(4, false) && NightmareHaunting.FogFor(4, false) <= .14f, "Bounded progressively thicker fog");
            Check(NightmareHaunting.FogFor(4, true) < .03f, "Reduced effects protect visibility");
            Directory.CreateDirectory("PreviewExports/Nightmare/Farewell");
            File.WriteAllLines("PreviewExports/Nightmare/Farewell/Focused_Checks.txt", notes);
            Debug.Log("NIGHTMARE_FAREWELL_CHECKS " + notes.Count + " PASS");
        }
    }
}
