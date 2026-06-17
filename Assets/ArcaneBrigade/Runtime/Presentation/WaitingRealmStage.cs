using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class WaitingRealmStage : MonoBehaviour, ShopFocus.ISource
    {
        public static bool BedsideLullabyActive { get; private set; }
        private ArenaController owner;
        private SpriteRenderer chair;
        private FloatingStoryProp floatingChair;
        private AudioSource song;
        private AudioClip waiting, morning;
        private int songWave = -1;
        private bool dawnPlayed;
        // The warm chair (Docs/Mien_Cho_Act_One_Story.md): the traveller's own chair, one step closer to the exit each visit.
        private Sprite warm, fallback;
        private readonly Sprite[] offerings = new Sprite[4];
        private bool asking;
        private GUIStyle hand, plain, choice;
        public bool ChairVisible => chair != null && chair.enabled;
        public bool Asking => asking;
        private bool ShopChair => owner?.Session != null && owner.Session.State.Phase == SessionPhase.Shop && !owner.Session.State.Endless && owner.Session.State.Wave <= ActOneCanon.Waves;
        public Vector2 ChairAnchor => ShopChair ? ActOneCanon.ChairPosition(owner.Session.State.Wave) : ActOneCanon.ChairPosition(1);
        /// <summary>Painted width and height of the chair as drawn (ShopLayout reserves this at all 19 positions).</summary>
        // the older wide armchair is drawn a little smaller so it fits the same reserved spots as the warm chair
        private float ChairHeight => warm != null ? 1.9f : 1.8f; // bigger (requirement 22/10/2026: important things read at a glance)
        public Vector2 ChairSize
        {
            get
            {
                Sprite shown = chair != null && chair.sprite != null ? chair.sprite : warm != null ? warm : fallback;
                if (shown == null) return new Vector2(1.3f, 1.6f);
                Rect ink = SpriteInkMetrics.BoundsOf(shown);
                float scale = ChairHeight / Mathf.Max(.01f, ink.height);
                return ink.size * scale;
            }
        }
        public const float ChairReach = 1.9f; // was 1.35: the chair's option only showed on one exact spot (requirement 22/10/2026)
        public int SpotCount => 1;
        public bool Spot(int index, out Vector2 position, out float reach) { position = ChairAnchor; reach = ChairReach; return ShopChair && chair != null && chair.enabled && owner.Simulation?.Champion != null; }
        private bool Near => Spot(0, out _, out _) && ShopFocus.Has(this, 0, owner.Simulation.Champion.Position);
        /// <summary>True while the chair has the traveller's attention, so the shop does not also act on the same press.</summary>
        public bool Engaged => asking || Near && owner.Session.SitCloserAvailable;
        public void Configure(ArenaController value)
        {
            owner = value; ShopFocus.Register(this);
            var child = new GameObject("TheUnoccupiedChair"); child.transform.SetParent(transform, false);
            chair = child.AddComponent<SpriteRenderer>();
            warm = Resources.Load<Sprite>("MienCho/ActOne/WarmChair"); fallback = Resources.Load<Sprite>("MienCho/EmptyChair");
            string[] lying = { null, "WarmChair_Coins", "WarmChair_Jar", "WarmChair_Phone" };
            for (int i = 1; i < lying.Length; i++) offerings[i] = Resources.Load<Sprite>("MienCho/ActOne/" + lying[i]);
            chair.sprite = warm != null ? warm : fallback;
            floatingChair=child.AddComponent<FloatingStoryProp>();floatingChair.Configure(chair,241);
            song = AudioMix.Route(gameObject.AddComponent<AudioSource>(), MixBus.Music); song.playOnAwake = false; song.spatialBlend = 0;
            waiting = Compose(false); morning = Compose(true);
        }
        // A small authored phrase. The earlier version withholds its cadence; dawn answers it
        // with several slightly offset voices. This is synthesis, not a recorded orchestra.
        private static AudioClip Compose(bool dawn)
        {
            int[] notes = dawn ? new[] { 60, 64, 67, 69, 67, 64, 62, 60, 64, 67, 72, 71, 69, 67, 62, 60 } : new[] { 60, 63, 67, 68, 67, 63, 62, 0, 60, 63, 67, 68, 67, 63, 0, 0 };
            const int rate = 22050; const float step = .56f;
            float[] samples = new float[(int)((notes.Length * step + 1.4f) * rate)];
            for (int n = 0; n < notes.Length; n++)
            {
                if (notes[n] == 0) continue;
                double frequency = 440 * System.Math.Pow(2, (notes[n] - 69) / 12d);
                int start = (int)(n * step * rate);
                for (int i = 0; i < rate * 1.4f && start + i < samples.Length; i++)
                {
                    float t = i / (float)rate;
                    double phase = t * frequency * System.Math.PI * 2;
                    double note = System.Math.Sin(phase) * .5 + System.Math.Sin(phase * 2.003) * .16 + System.Math.Sin(phase * 3.99) * .045;
                    if (dawn) note += System.Math.Sin((t + n * .0017) * frequency * 1.003 * System.Math.PI * 2) * .11;
                    float envelope = Mathf.Min(1, t * 90) * Mathf.Exp(-t * 4.5f);
                    samples[start + i] += (float)note * envelope * .3f;
                }
            }
            var clip = AudioClip.Create(dawn ? "A Morning With Other Voices" : "The Withheld Last Verse", samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        private void Update()
        {
            if (owner?.Session == null || owner.Simulation == null) return;
            var state = owner.Session.State;
            BedsideLullabyActive = state.Phase == SessionPhase.Combat && state.ActOne?.SitCloser == ActOneProgress.Fighting;
            bool dawn = state.Nightmare.CustomerDefeated && !state.Endless;
            chair.enabled = chair.sprite != null && !owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible && state.Phase != SessionPhase.Defeat;
            if (chair.enabled)
            {
                Vector2 position = state.Phase == SessionPhase.Shop ? ChairAnchor : new Vector2(0, owner.WorldBounds.y * .5f - .8f);
                // in a fight the arena layout seats it first (so it always has room), near the far edge
                float fit = 1;
                if (state.Phase != SessionPhase.Shop) ArenaLayout.Place(owner, "chair", ref position, out fit);
                int lies = ShopChair ? ActOneCanon.ChairOffering(state.Wave, MienChoStory.Choice(state, 8)) : 0;
                if (ShopChair && GetComponent<ActOneCustomerVisits>()?.DeferOffering == true) lies = 0;
                Sprite shown = lies > 0 && offerings[lies] != null ? offerings[lies] : warm != null ? warm : fallback;
                if (chair.sprite != shown) chair.sprite = shown;
                chair.transform.localScale = Vector3.one * (ChairHeight * fit / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(chair.sprite).height));
                chair.sortingOrder = WorldSpriteGrounding.SortAt(position.y);
                // The unreachable seat is a narrative landmark, never a hidden threat.
                chair.color = dawn ? new Color(1, .95f, .86f) : Color.Lerp(new Color(.85f, .78f, .69f), new Color(.24f, .22f, .3f), NightmareRules.Depth(state.Wave) / 4f);
                // once the traveller has stood up out of it, it is only a chair: no warmth left in the wood
                if ((state.ActOne?.SitCloser ?? 0) == ActOneProgress.Won) chair.color = Color.Lerp(chair.color, new Color(.55f, .62f, .72f), .6f);
                floatingChair.Display(position,owner.PresentationClock,NightmareRules.Depth(state.Wave));
            }
            else floatingChair.Display(Vector3.zero,owner.PresentationClock,0);
            UpdateAsking();
            if (owner.PresentationPaused) { song.Pause(); return; }
            song.UnPause();
            song.volume = AudioMix.SourceGain(MixBus.Music) * PresentationSettings.MusicDuck * (PresentationSettings.ReducedAudio ? .1f : .16f);
            if (BedsideLullabyActive)
            {
                if (song.clip != waiting || !song.loop) { song.Stop(); song.clip = waiting; song.loop = true; song.Play(); }
                return;
            }
            if (song.loop) { song.Stop(); song.loop = false; }
            bool restPhrase = state.Phase == SessionPhase.Shop && (state.Wave == 1 || state.Wave == 8 || state.Wave == 16) && state.Wave != songWave;
            if (dawn && !dawnPlayed && state.Nightmare.EndingPage >= 2) { song.clip = morning; song.Play(); dawnPlayed = true; }
            else if (restPhrase && !owner.TransitionPending) { song.clip = waiting; song.Play(); songWave = state.Wave; }
            if (state.Phase == SessionPhase.Combat && song.isPlaying) song.Stop();
        }
        private void UpdateAsking()
        {
            if (!ShopChair || owner.PresentationPaused || owner.TransitionPending || owner.StoryConsumesInput || owner.Details.Open || !owner.Session.SitCloserAvailable) { asking = false; return; }
            if (!Application.isFocused && !RunPaths.Diagnostic) return;
            bool press = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Return);
            if (asking)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1) || !Near) asking = false;
                else if (press) Sit();
            }
            else if (Near && press) { asking = true; CheckpointAudio.PlayInterfaceId("ui.page"); }
        }
        public void AskForDiagnostics(bool value = true) => asking = value && RunPaths.Diagnostic;
        private void Sit()
        {
            asking = false;
            if (owner.AcceptSitCloser()) CheckpointAudio.PlayInterfaceId("sig.stamp", .2f);
        }
        private void OnGUI()
        {
            if (!ShopChair || chair == null || !chair.enabled || owner.TransitionPending || owner.PresentationPaused || owner.StoryConsumesInput || Camera.main == null) return;
            var state = owner.Session.State;
            bool available = owner.Session.SitCloserAvailable;
            if (!Near && !asking) return;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (hand == null)
            {
                hand = ArcadeTypography.Style(30); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleCenter; hand.wordWrap = true;
                plain = ArcadeTypography.Style(20); plain.alignment = TextAnchor.UpperCenter; plain.wordWrap = true;
                choice = ArcadeTypography.Style(22, true); choice.alignment = TextAnchor.MiddleCenter;
            }
            Vector3 point = Camera.main.WorldToViewportPoint(ChairAnchor);
            float x = point.x * 1920, y = (1 - point.y) * 1080;
            if (!asking)
            {
                // before the eighth shop it only answers with the shopkeeper's warning
                string said = !available ? (state.ActOne?.SitCloser ?? 0) == ActOneProgress.Won ? "only a chair now." : "not that one." : "sit down?";
                Rect tag = new Rect(Mathf.Clamp(x - 180, 20, 1540), Mathf.Clamp(y + 30, 180, 950), 360, 58);
                ArcadeSkin.Ticket(tag, ArcadeSkin.Paper, ArcadeSkin.Wax);
                hand.normal.textColor = new Color(.25f, .19f, .14f);
                GUI.Label(tag, said, hand);
                if (available && GUI.Button(tag, GUIContent.none, GUIStyle.none)) asking = true;
            }
            else
            {
                // a card over the shop (requirement 22/10/2026: price tags showed through it and its text ran to the rims): dimmed room,
                // above the HUD and the tags, every line inside the paper's padded area, the painted buttons apart
                GUI.depth = -60;
                RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, .5f));
                Rect card = new Rect(470, 300, 980, 500);
                Color popKeep = CardPop.Apply("warm-chair", card);
                ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Wax);
                Rect inner = ArcadeSkin.Content(card, KitFrame.Card);
                ArcadeSkin.Seal(new Vector2(inner.xMax - 30, inner.y + 26), 46, "!");
                hand.normal.textColor = new Color(.25f, .19f, .14f);
                GUI.Label(new Rect(inner.x + 10, inner.y + 6, inner.width - 80, 140), "THE SHOPKEEPER: 'Not that one. Please. If you sit in it, you are in there alone with it — and if it keeps you, you do not get up.'", hand);
                plain.normal.textColor = new Color(.18f, .14f, .1f);
                GUI.Label(new Rect(inner.x + 10, inner.y + 160, inner.width - 20, 120),
                    "HIDDEN FIGHT · THE NIGHT YOU HID\nThe Bedside and the travellers it wakes fight like wave " + (state.Wave + ActOneCanon.SitWaveBonus) + ". Defeat it before the knocking stops (" + Mathf.RoundToInt(ActOneCanon.KnockSeconds) + " s).\nIf you lose, the journey ends here. Only once per journey.", plain);
                float buttonsY = inner.yMax - 92;
                if (ArcadeSkin.Button(new Rect(inner.center.x - 350, buttonsY, 320, 88), "SIT DOWN", KitButton.Danger, true, choice) && !CardPop.Closing("warm-chair")) CardPop.CloseThen("warm-chair", Sit);
                if (ArcadeSkin.Button(new Rect(inner.center.x + 30, buttonsY, 320, 88), "STEP AWAY", KitButton.Secondary, true, choice) && !CardPop.Closing("warm-chair")) CardPop.CloseThen("warm-chair", () => asking = false);
                GUI.color = popKeep;
            }
            GUI.matrix = previous;
        }
        private void OnDestroy() { BedsideLullabyActive = false; ShopFocus.Unregister(this); if (waiting != null) Destroy(waiting); if (morning != null) Destroy(morning); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetMusic() => BedsideLullabyActive = false;
    }
}
