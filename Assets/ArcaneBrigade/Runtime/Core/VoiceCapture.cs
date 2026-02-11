using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native screenshots of the speech boxes and voice barks: the shopkeeper's welcome, a story companion joining,
    /// a purchase, a companion joining and ranking up, the warm-chair warning and the Keeper's lines. Isolated diagnostics
    /// save. Run with -arcane-voice-capture -capturePath DIR.</summary>
    public sealed class VoiceCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-voice-capture") >= 0;
        private string root;
        private readonly List<string> log = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<VoiceCapture>() != null) return;
            var go = new GameObject("VoiceCapture"); DontDestroyOnLoad(go); go.AddComponent<VoiceCapture>();
        }

        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG());
            Destroy(image);
            var bubbles = new List<string>();
            foreach (SpeechBubble bubble in SpeechBubbles.Active) bubbles.Add(bubble.Speaker + (bubble.Drawn ? " @" + bubble.Placed : " (hidden)") + ": " + bubble.Text.Replace("\n", " / "));
            ArenaController arena = FindAnyObjectByType<ArenaController>();
            log.Add(name + " · " + bubbles.Count + " boxes · " + string.Join(" | ", bubbles) + " · mouth=" + (arena?.Shop?.MerchantMouth()?.ToString() ?? "none") + " cam=" + (Camera.main != null) + " quiet=" + NightmareHaunting.Quiet);
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
            log.Add("voice lines: " + VoiceLines.All.Count + " (shopkeeper " + VoiceLines.Count(VoiceLines.Shopkeeper) + ")");

            // A: shop 6, Postscript joins; one companion already has two copies so the third merges into two stars
            string starId = null, joinId = null;
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171017); session.State.Wave = 6; session.State.Phase = SessionPhase.Shop; session.State.Gold = 5000;
                session.State.MienCho.ReadMask = (1 << MienChoStory.Available(session.State)) - 1;
                for (int roll = 0; roll < 30 && session.State.Offers.FindAll(o => o.Kind == OfferKind.Companion).Count < 2; roll++) session.Reroll();
                List<ShopOffer> companions = session.State.Offers.FindAll(o => o.Kind == OfferKind.Companion);
                if (companions.Count > 0)
                {
                    starId = companions[companions.Count - 1].Id;
                    if (companions.Count > 1 && companions[0].Id != starId) joinId = companions[0].Id;
                    UnitDefinition unit = catalog.Unit(starId);
                    var weapons = new List<string>();
                    foreach (WeaponDefinition weapon in catalog.Weapons) if (weapon.Kind == unit.Weapon.Kind && !weapons.Contains(weapon.Id) && weapons.Count < 2) weapons.Add(weapon.Id);
                    session.State.Roster.RemoveAll(entry => entry.UnitId == starId || entry.UnitId == joinId);
                    foreach (string weapon in weapons) session.State.Roster.Add(new RosterEntry { UnitId = starId, WeaponId = weapon, Tier = 1 });
                }
                session.State.Gold = 5000;
                storage.SaveRun(session.State);
            }
            log.Add("star=" + starId + " join=" + joinId);
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2.1f);
            var owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { log.Add("no arena"); Finish(); yield break; }
            log.Add("roster after load: " + owner.Session.State.Roster.Count + " notice=" + owner.Session.Notice + " save=" + owner.SaveError);
            yield return Shot("00-shop-enter");
            // a resumed save does not replay arrivals, so Postscript's first words are asked for directly
            yield return new WaitForSecondsRealtime(1.2f);
            var party = owner.GetComponent<SpeechBubbleCast>();
            log.Add("postscript says: " + (party != null && party.DiagnosticStory(2, "join")));
            yield return new WaitForSecondsRealtime(.5f); yield return Shot("01-postscript-joins");
            float until;

            WorldShop shop = owner.Shop;
            yield return new WaitForSecondsRealtime(2.6f);
            if (starId != null) { yield return Buy(owner, starId); yield return new WaitForSecondsRealtime(.35f); yield return Shot("02-purchase-shopkeeper"); yield return WaitFor("Companion:" + starId, 5); yield return new WaitForSecondsRealtime(.3f); yield return Shot("04-star-up"); }
            // find a second species on the shelf to recruit
            yield return new WaitForSecondsRealtime(3f);
            for (int roll = 0; roll < 40 && joinId == null; roll++)
            {
                foreach (ShopOffer offer in owner.Session.State.Offers) if (offer.Kind == OfferKind.Companion && !offer.Sold && offer.Id != starId && owner.Session.CanBuy(offer)) joinId = offer.Id;
                if (joinId == null) { owner.Session.State.Gold = 5000; owner.RerollShop(); shop.Display(); yield return new WaitForSecondsRealtime(.15f); }
            }
            log.Add("join=" + joinId);
            yield return new WaitForSecondsRealtime(2.4f);
            if (joinId != null) { yield return Buy(owner, joinId); yield return WaitFor("Companion:" + joinId, 4); yield return new WaitForSecondsRealtime(.4f); yield return Shot("03-companion-joins"); }
            log.Add("roster: " + string.Join(", ", owner.Session.State.Roster.ConvertAll(entry => entry.UnitId + "*" + entry.Tier)));

            // the warm chair: the shopkeeper does not look at it
            yield return new WaitForSecondsRealtime(2.5f);
            WaitingRealmStage chair = owner.GetComponent<WaitingRealmStage>();
            if (chair != null) { owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = chair.ChairAnchor + new Vector2(-1.2f, -.4f); yield return new WaitForSecondsRealtime(.9f); yield return Shot("05-chair-warning"); }
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-2, -3);

            // can't afford
            yield return new WaitForSecondsRealtime(2.2f);
            owner.Session.State.Gold = 0;
            WorldShop.Station reroll = null; foreach (WorldShop.Station station in shop.Stations) if (station.Action == "REROLL") reroll = station;
            if (reroll != null) { owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = reroll.Position; yield return new WaitForSecondsRealtime(.3f); shop.Interact(); yield return new WaitForSecondsRealtime(.45f); yield return Shot("06-cant-afford"); }

            // B: shop 4 → wave 5, the Keeper speaks
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171018); session.State.Wave = 4; session.State.Phase = SessionPhase.Shop; session.State.Gold = 50;
                session.State.MienCho.ReadMask = (1 << MienChoStory.Available(session.State)) - 1;
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2.4f);
            owner = FindAnyObjectByType<ArenaController>();
            owner.NextWave();
            until = Time.realtimeSinceStartup + 30;
            while ((owner.TransitionPending || owner.Simulation.Boss == null || owner.Session.State.Phase != SessionPhase.Combat) && Time.realtimeSinceStartup < until) yield return null;
            log.Add("boss: " + (owner.Simulation.Boss == null ? "none" : owner.Simulation.Boss.Definition.DisplayName) + " wave " + owner.Session.State.Wave);
            yield return WaitFor(VoiceLines.Keeper, 6); yield return new WaitForSecondsRealtime(.3f); yield return Shot("07-keeper-enter");
            var cast = owner.GetComponent<SpeechBubbleCast>();
            if (owner.Simulation.Boss != null) log.Add("boss at " + owner.Simulation.Boss.Position + " head " + SpeechBubbles.Head(owner.Simulation.Boss) + " camera " + Camera.main?.transform.position + " size " + Camera.main?.orthographicSize + " screen " + Camera.main?.WorldToScreenPoint(SpeechBubbles.Head(owner.Simulation.Boss)));
            yield return new WaitForSecondsRealtime(2.8f);
            if (cast != null) { cast.DiagnosticBoss("attack"); yield return new WaitForSecondsRealtime(.4f); yield return Shot("08-keeper-attack"); }
            if (owner.Simulation.Boss != null) { owner.Simulation.Boss.Health = owner.Simulation.Boss.Definition.Health * .45f; yield return new WaitForSecondsRealtime(.6f); yield return Shot("09-keeper-phase"); }
            log.Add("director: played=" + (VoiceDirector.Active?.PlayedClips ?? -1) + " silent=" + (VoiceDirector.Active?.SilentLines ?? -1) + " joins=" + cast?.Joins + " starups=" + cast?.StarUps + " boss=" + cast?.BossLines);
            Finish();
        }

        private IEnumerator WaitFor(string speaker, float seconds)
        {
            float until = Time.realtimeSinceStartup + seconds;
            while (!SpeechBubbles.Showing(speaker) && Time.realtimeSinceStartup < until) yield return null;
            if (!SpeechBubbles.Showing(speaker)) log.Add("timeout waiting for " + speaker);
        }

        private IEnumerator Buy(ArenaController owner, string id)
        {
            WorldShop.Station target = null;
            foreach (WorldShop.Station station in owner.Shop.Stations) if (station.Offer != null && !station.Offer.Sold && station.Offer.Id == id) target = station;
            if (target == null) { log.Add("no stand for " + id); yield break; }
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = target.Position;
            yield return new WaitForSecondsRealtime(.35f);
            bool bought = owner.Shop.Interact();
            log.Add("buy " + id + " = " + bought + " · " + owner.Session.Notice + " · line: " + owner.Shop.Dialogue.CurrentLine);
        }

        private void Finish()
        {
            File.WriteAllLines(Path.Combine(root, "voice-capture.txt"), log);
            Application.Quit(0);
        }
    }
}
