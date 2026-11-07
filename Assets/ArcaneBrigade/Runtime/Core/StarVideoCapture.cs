using System.Linq;
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
    /// <summary>Records what the listener hears (the game's whole mix) into a 16-bit stereo WAV beside the video, so the clip can carry
    /// sound: each frame it reads the newest samples the listener output (the listener's object carries several audio sources, so a
    /// filter cannot sit on it).</summary>
    public sealed class StarVideoAudioTap : MonoBehaviour
    {
        private readonly List<short> samples = new List<short>(48000 * 2 * 120);
        private readonly float[] left = new float[4096], right = new float[4096];
        private int rate = 48000;
        private bool recording;
        private double carry;

        public void Begin() { rate = AudioSettings.outputSampleRate; samples.Clear(); recording = true; carry = 0; }

        private void Update()
        {
            if (!recording) return;
            carry += Time.unscaledDeltaTime * rate;
            int n = Mathf.Min(left.Length, (int)carry); carry -= n;
            if (n <= 0) return;
            AudioListener.GetOutputData(left, 0); AudioListener.GetOutputData(right, 1);
            for (int i = left.Length - n; i < left.Length; i++) { samples.Add((short)(Mathf.Clamp(left[i], -1f, 1f) * 32767f)); samples.Add((short)(Mathf.Clamp(right[i], -1f, 1f) * 32767f)); }
        }

        public void Save(string path)
        {
            recording = false;
            using (var stream = new FileStream(path, FileMode.Create))
            using (var w = new BinaryWriter(stream))
            {
                int bytes = samples.Count * 2;
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + bytes); w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                w.Write(16); w.Write((short)1); w.Write((short)2); w.Write(rate); w.Write(rate * 4); w.Write((short)4); w.Write((short)16);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data")); w.Write(bytes);
                foreach (short v in samples) w.Write(v);
            }
        }
    }

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
            style.normal.textColor = new Color(0, 0, 0, .8f); LocalizedGUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), caption, style);
            style.normal.textColor = new Color(1f, .9f, .6f); LocalizedGUI.Label(r, caption, style);
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
            RunSnapshot state = owner.Session.State;
            state.Gold = 99999;

            // the hero comes to the stall and buys a copy of the friend, exactly as in play: the roster is set to what the player would
            // hold before the purchase, the stall offers that friend at the star they own, the hero walks up and presses the key
            void Friends(int count)
            {
                // other friends in the brigade, who cheer after a high star-up
                foreach (UnitDefinition other in catalog.Companions)
                {
                    if (count <= 0) break;
                    if (other.Id == unit || other.SpecialRecruit || other.ReferenceCompanion == "Evil") continue;
                    state.Roster.Add(new RosterEntry { UnitId = other.Id, WeaponId = weapon, Tier = 1 }); count--;
                }
            }
            IEnumerator Buy(string words, int owned, int ownedTier, int offerTier, float wait, int friends = 0)
            {
                caption = words; state.NewRecruitsThisWave = 0; // the wave's copy quota would stop a video of seven purchases
                state.Roster.Clear();
                for (int i = 0; i < owned; i++) state.Roster.Add(Entry(ownedTier));
                Friends(friends);
                if (state.Offers.Count == 0) state.Offers.Add(new ShopOffer { Token = ++state.NextOffer });
                ShopOffer offer = state.Offers[0];
                offer.Kind = OfferKind.Companion; offer.Id = unit; offer.Tier = offerTier; offer.Price = 1; offer.Sold = false; offer.Currency = ShopCurrency.Gold;
                owner.Shop.Display(); yield return null; yield return null;
                WorldShop.Station stall = owner.Shop.Stations.FirstOrDefault(st => st.Offer == offer);
                if (stall == null) { caption = "(no stall)"; yield return new WaitForSecondsRealtime(wait); yield break; }
                Vector2 from = stall.Position + new Vector2(-4.2f, -2.2f), to = stall.Position + new Vector2(0f, -.8f);
                for (float t = 0; t < 1.3f; t += Time.unscaledDeltaTime)
                {
                    Vector2 p = Vector2.Lerp(from, to, Mathf.SmoothStep(0, 1, t / 1.3f));
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = p;
                    yield return null;
                }
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = to;
                for (int i = 0; i < 30 && owner.Shop.Nearby != stall; i++) yield return null;
                yield return new WaitForSecondsRealtime(.4f);
                for (int tries = 0; tries < 80 && !offer.Sold; tries++) { owner.Shop.Interact(); yield return new WaitForSecondsRealtime(.1f); } // the key is ignored while a ceremony still plays
                yield return new WaitForSecondsRealtime(wait);
            }

            var recorder = gameObject.AddComponent<K7FramebufferRecorder>(); recorder.Begin(125);
            AudioListener listener = FindAnyObjectByType<AudioListener>();
            StarVideoAudioTap tap = listener != null ? gameObject.AddComponent<StarVideoAudioTap>() : null; tap?.Begin();
            File.WriteAllText(Path.Combine(RunPaths.Root, "StarVideoReady.txt"), "recording");
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Buy("1.5 stars: you own one, buy a second", 1, 1, 1, 3.4f);
            yield return Buy("2 stars: the third copy merges", 2, 1, 1, 5.0f);
            yield return Buy("2.5 stars: you own one 2-star, buy a second", 1, 2, 2, 3.4f);
            yield return Buy("3 stars: the third 2-star copy merges", 2, 2, 2, 8.5f, 4);
            yield return Buy("3.5 stars: you own one 3-star, buy a second", 1, 3, 3, 3.4f, 4);
            yield return Buy("4 stars (purple): the third 3-star copy merges", 2, 3, 3, 13.5f, 4);
            caption = "5 stars: a Starlight Thread from a special event";
            state.Roster.Clear(); state.Roster.Add(Entry(4)); Friends(4);
            yield return new WaitForSecondsRealtime(1.0f);
            owner.GiveStarlightThread();
            yield return new WaitForSecondsRealtime(21f);
            caption = "The brigade: 1, 2, 3, 4 and 5 stars side by side";
            { int[] tiers = { 2, 3, 4, 1 }; int slot = 0; foreach (RosterEntry e in state.Roster) if (e.UnitId != unit && slot < tiers.Length) e.Tier = tiers[slot++]; }
            owner.RefreshBrigade(); yield return new WaitForSecondsRealtime(9f);
            caption = "";
            while (!recorder.Complete) yield return null;
            int videoArg = Array.IndexOf(args, "-videoPath");
            if (tap != null && videoArg >= 0 && videoArg + 1 < args.Length) tap.Save(args[videoArg + 1] + ".wav");
            File.WriteAllText(Path.Combine(RunPaths.Root, "StarVideoDone.txt"), recorder.Passed ? "PASS" : "FAIL");
            Application.Quit(recorder.Passed ? 0 : 1);
        }
    }
}
