using UnityEngine;

namespace ArcaneBrigade
{
    public enum WaitingEncounter { None = -1, ReservedTable, BreathingPhone, LateReflection }

    // Optional, repeatable story interactions in the shop. No colliders, prices or build effects.
    [DefaultExecutionOrder(-55)]
    public sealed class WaitingShopEncounter : MonoBehaviour, ShopFocus.ISource
    {
        public static readonly Vector2 Anchor = new Vector2(4.3f, -4.55f); // the free low right (requirement 25/10/2026: beside the exit rift, between the stalls, it was cramped)
        public const float Reach = 1.65f;
        private ArenaController owner;
        private SpriteRenderer prop, reflection, tear;
        private GroundShadow shadow;
        private DreamSuspension suspension;
        private readonly Sprite[] artwork = new Sprite[3];
        private Sprite tearSprite;
        private AudioSource voice;
        private AudioClip ring;
        private GUIStyle prompt, caption;
        private WaitingEncounter active = WaitingEncounter.None;
        private float entered, nextRing;
        private int rang;
        private bool wasPaused;
        private Vector2 placedAnchor;
        private bool registeredLayout;
        private const string LayoutId="story:telephone";
        public WaitingEncounter Active => active;
        public bool Visible => prop != null && prop.enabled;
        public bool Near { get; private set; }
        public float AnimationClock { get; private set; }
        public int RingCount => rang;
        public int ReflectionUnitId { get; private set; }
        public int SpotCount => Visible && Allowed ? 1 : 0;
        public bool Spot(int index, out Vector2 position, out float reach)
        { position = placedAnchor; reach = Reach; return index == 0 && Visible && Allowed; }
        private bool Allowed => owner?.Session != null && owner.Session.State.Phase == SessionPhase.Shop && !owner.Session.SecretRoomOpen && owner.Session.State.OutsideStage != OutsideStageStatus.Reward && !owner.TransitionPending && !owner.HallExitPending && !owner.NightmareEndingVisible;
        // Only restore the encounter that belongs to the current Act I canon. Earlier table/mirror scenes stay retired.
        public static WaitingEncounter ForWave(int wave) => wave == 8 ? WaitingEncounter.BreathingPhone : WaitingEncounter.None;
        public static int ChapterFor(WaitingEncounter kind) => kind switch
        {
            WaitingEncounter.ReservedTable => 1,
            WaitingEncounter.BreathingPhone => System.Array.FindIndex(MienChoStory.Chapters, c => c.Wave == 8 && c.Shop),
            WaitingEncounter.LateReflection => 4,
            _ => -1
        };
        public void Configure(ArenaController controller)
        {
            owner = controller; ShopFocus.Register(this);
            string[] names = { "ReservedTable", "BreathingPhone", "LateReflection" };
            for (int i = 0; i < names.Length; i++) artwork[i] = Resources.Load<Sprite>("MienCho/Encounters/" + names[i]);
            artwork[(int)WaitingEncounter.BreathingPhone] = ActOneArtwork.Picture("Prop_StoryTelephone") ?? artwork[(int)WaitingEncounter.BreathingPhone];
            prop = Make("WaitingEncounterProp"); reflection = Make("WitnessInTheGlass"); tear = Make("ImpossibleTear");
            reflection.sharedMaterial = Resources.Load<Material>("MienCho/MirrorWitness");
            var texture = Texture2D.whiteTexture;
            tearSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f, texture.width);
            tear.sprite = tearSprite; tear.sharedMaterial = Resources.Load<Material>("MienCho/WaitingGlow");
            shadow = prop.gameObject.AddComponent<GroundShadow>(); shadow.PinCastToFoot = true;
            suspension = prop.gameObject.AddComponent<DreamSuspension>();
            voice = AudioMix.Route(gameObject.AddComponent<AudioSource>(), MixBus.Sfx); voice.playOnAwake = false; voice.spatialBlend = 0;
            ring = ComposeRing();
            Hide();
        }
        private SpriteRenderer Make(string label)
        {
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            return go.AddComponent<SpriteRenderer>();
        }
        private static AudioClip ComposeRing()
        {
            const int rate = 22050;
            var data = new float[rate];
            for (int i = 0; i < data.Length; i++)
            {
                float time = i / (float)rate;
                float gate = time < .27f || time > .4f && time < .67f ? 1 : 0;
                float envelope = gate * Mathf.Min(1, Mathf.Repeat(time, .4f) * 45) * Mathf.Exp(-Mathf.Repeat(time, .4f) * 7);
                data[i] = ((float)System.Math.Sin(time * 620 * Mathf.PI * 2) + (float)System.Math.Sin(time * 927 * Mathf.PI * 2) * .37f) * envelope * .16f;
            }
            var clip = AudioClip.Create("A Telephone With No Wire", data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private void Hide()
        {
            if (registeredLayout) { ShopLayout.Active?.Release(LayoutId); registeredLayout=false; }
            prop.enabled = reflection.enabled = tear.enabled = false; Near = false;
            shadow.Hide(); suspension.Hide(); voice.Stop();
        }
        private void Update()
        {
            if (owner?.Session == null || owner.Simulation == null) return;
            var state = owner.Session.State;
            var kind = ForWave(state.Wave);
            bool allowed = Allowed;
            if (!allowed || kind == WaitingEncounter.None) { Hide(); active = WaitingEncounter.None; return; }
            if (active != kind)
            {
                active = kind; entered = owner.PresentationClock; rang = 0; nextRing = entered + 1.5f;
                prop.sprite = artwork[(int)kind];
            }
            if (prop.sprite == null) { Hide(); return; }
            if (owner.PresentationPaused)
            {
                if (!wasPaused) voice.Pause();
                wasPaused = true; Near = false; return;
            }
            if (wasPaused) { voice.UnPause(); wasPaused = false; }
            AnimationClock = owner.PresentationClock;
            prop.enabled = true;
            float width = kind == WaitingEncounter.ReservedTable ? 3.15f : kind == WaitingEncounter.LateReflection ? 1.65f : 1.65f;
            placedAnchor=Anchor;
            float fit=1;
            if (ShopLayout.Active!=null)
            {
                registeredLayout=true;
                if (!ShopLayout.Active.Place(LayoutId,Anchor,ShopLayout.SizeAtWidth(prop.sprite,width),80,true,false,out placedAnchor,out fit))
                { prop.enabled=false; Near=false; shadow.Hide(); suspension.Hide(); voice.Stop(); return; }
                width*=fit;
            }
            prop.transform.localScale = Vector3.one * (width / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(prop.sprite).width));
            Vector3 hover = DreamLevitation.Offset(AnimationClock, 171 + (int)kind, NightmareRules.Depth(state.Wave), PresentationSettings.ReducedMotion, .75f);
            float tilt=DreamLevitation.Tilt(AnimationClock,171+(int)kind,PresentationSettings.ReducedMotion,.65f);
            if(ShopSceneryLanding.Active!=null){hover=ShopSceneryLanding.Active.Apply(hover,171+(int)kind);tilt=ShopSceneryLanding.Active.Tilt(tilt,171+(int)kind);}
            prop.transform.rotation = Quaternion.Euler(0,0,tilt);
            WorldSpriteGrounding.Place(prop, (Vector3)placedAnchor + hover);
            prop.sortingOrder = WorldSpriteGrounding.SortAt(placedAnchor.y);
            prop.color = Color.white;
            shadow.SortingBase = prop.sortingOrder - 5;
            shadow.Display(prop.sprite, placedAnchor, width * .8f, hover.y, .25f);
            suspension.Display(placedAnchor, (Vector3)placedAnchor + hover, width, AnimationClock, 171 + (int)kind, prop.sortingOrder - 1, 1, true);
            Near = ShopFocus.Has(this,0,owner.Simulation.Champion.Position);
            if (owner.Shop?.Nearby != null || owner.Shop?.Animating == true) Near = false;
            UpdateReflection(kind);
            voice.volume = PresentationSettings.ReducedAudio ? 0 : AudioMix.SourceGain(MixBus.Sfx) * .2f;
            if (kind == WaitingEncounter.BreathingPhone && Near && rang < 2 && AnimationClock >= nextRing && (state.MienCho?.EncounterMask & 2) != 2)
            {
                if (!PresentationSettings.ReducedAudio) voice.PlayOneShot(ring);
                rang++; nextRing = AnimationClock + 4;
            }
            if (Near && !owner.StoryConsumesInput && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton2))) TryInspect();
        }
        private void UpdateReflection(WaitingEncounter kind)
        {
            reflection.enabled = tear.enabled = kind == WaitingEncounter.LateReflection;
            ReflectionUnitId = 0;
            if (!reflection.enabled) return;
            UnitState witness = owner.Simulation.Champion;
            foreach (var unit in owner.Simulation.Units) if (unit.Alive && unit.Faction == Faction.Companion) { witness = unit; break; }
            ReflectionUnitId = witness.Id;
            Sprite[] frames = witness.Definition.AnimationFrames;
            reflection.sprite = frames.Length > 1 ? frames[1] : witness.Definition.Body;
            Bounds mirror = prop.bounds;
            reflection.transform.position = new Vector3(mirror.center.x, mirror.min.y + mirror.size.y * .56f, 0);
            reflection.transform.rotation=prop.transform.rotation;
            float fit = Mathf.Min(mirror.size.x * .53f / reflection.sprite.bounds.size.x, mirror.size.y * .43f / reflection.sprite.bounds.size.y);
            reflection.transform.localScale = Vector3.one * fit;
            reflection.flipX = witness.Facing.x >= 0;
            reflection.color = new Color(.48f, .75f, .86f, .65f);
            reflection.sortingOrder = prop.sortingOrder + 1;
            float drop = PresentationSettings.ReducedMotion ? .44f : Mathf.Repeat((AnimationClock - entered) * .13f, 1);
            tear.transform.position = reflection.transform.position + prop.transform.rotation*new Vector3(mirror.size.x * .11f, .25f - drop * .35f, 0);
            tear.transform.rotation=prop.transform.rotation;
            tear.transform.localScale = new Vector3(.055f, .13f, 1);
            tear.color = new Color(.63f, .85f, 1, .8f); tear.sortingOrder = prop.sortingOrder + 2;
        }
        public bool TryInspect()
        {
            if (!Allowed || !Near || !Visible || owner.PresentationPaused || active == WaitingEncounter.None) return false;
            var journal = owner.GetComponent<MienChoPresentation>();
            journal.Show(ChapterFor(active));
            if (!journal.Open) return false;
            var memory = owner.Session.State.MienCho;
            int previous = memory.EncounterMask; memory.EncounterMask |= 1 << (int)active;
            if (!owner.SaveNightmareBoundary()) memory.EncounterMask = previous;
            voice.Stop(); Near = false;
            return true;
        }
        private bool promptShown; private float promptAt;
        private void OnGUI()
        {
            if (!Visible || !Near || owner.PresentationPaused || owner.StoryConsumesInput) { promptShown = false; return; }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (GUI.Button(StoryPropInput.ClickRect(prop,Camera.main),GUIContent.none,GUIStyle.none)) TryInspect();
            // the shop's one prompt card, as the secret rooms' (requirement 25/10/2026); each thing has its own name and verb
            if (!promptShown) { promptShown = true; promptAt = Time.unscaledTime; }
            bool table = active == WaitingEncounter.ReservedTable, phone = active == WaitingEncounter.BreathingPhone;
            string title = table ? "THE RESERVED TABLE" : phone ? "THE RINGING TELEPHONE" : "THE LATE REFLECTION";
            string plain = table ? "Three chairs, two cups, and a name you have not met. A story page waits here." : phone ? "The wire is cut, yet it still rings. A story page waits here." : "Your reflection does not follow you. A story page waits here.";
            if (ShopPrompt.Draw(title, plain, table ? "SIT DOWN" : phone ? "ANSWER" : "LOOK CLOSER", Time.unscaledTime - promptAt)) TryInspect();
            GUI.matrix = previous;
        }
        private void OnDestroy()
        {
            if (registeredLayout) ShopLayout.Active?.Release(LayoutId);
            ShopFocus.Unregister(this);
            if (ring != null) Destroy(ring); if (tearSprite != null) Destroy(tearSprite);
        }
    }
}
