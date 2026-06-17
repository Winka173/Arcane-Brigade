using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Small, world-anchored rests between fights; the real story actors remain the actors in these scenes.</summary>
    [DefaultExecutionOrder(110)]
    public sealed class ActOneRestingScenes : MonoBehaviour, ShopFocus.ISource
    {
        private ArenaController owner;
        private SpriteRenderer evidence, receipt;
        private FloatingStoryProp suspended;
        private UnitVisual hob, tally;
        private int chapter = -1, wave = -1;
        private string art;
        private Vector2 anchor;
        private bool visible;
        private float noticedAt = -1;
        private Vector2 hobLight;
        private GUIStyle caption;
        private CombatSimulation castSimulation;
        private string layoutId;
        public static ActOneRestingScenes Active { get; private set; }
        public int SpotCount => visible ? 1 : 0;
        private bool Rest => owner?.Session != null && owner.Simulation != null && owner.Session.State.Phase == SessionPhase.Shop && !owner.Session.State.Endless
            && !owner.Session.SecretRoomOpen && !owner.TransitionPending && !owner.HallExitPending && !owner.NightmareEndingVisible;
        public void Configure(ArenaController value)
        {
            owner = value; Active = this; ShopFocus.Register(this);
            evidence = Make("A record left in the room"); receipt = Make("The receipt Tally hides");
            receipt.sprite = ActOneArtwork.Picture("Prop_WaitingNote");
            suspended = evidence.gameObject.AddComponent<FloatingStoryProp>(); suspended.Configure(evidence, 721);
        }
        private SpriteRenderer Make(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.enabled = false; return renderer;
        }
        public bool Spot(int index, out Vector2 position, out float reach)
        { position = anchor; reach = 1.5f; return index == 0 && Rest && visible; }
        public static Vector2 PartyLightPosition(UnitState unit, Vector2 ordinary)
        {
            var scene = Active;
            return scene != null && scene.Rest && scene.visible && scene.IsActor(scene.hob,"Hob") && ReferenceEquals(scene.hob.State,unit) && scene.wave == 17
                ? scene.hobLight : ordinary;
        }
        private void LateUpdate()
        {
            if (!Rest) { ReleaseLayout(); visible = false; evidence.enabled = receipt.enabled = false; suspended.Display(Vector3.zero, owner?.PresentationClock ?? 0, 0); return; }
            var state = owner.Session.State;
            if (wave != state.Wave)
            {
                ReleaseLayout();
                wave = state.Wave; noticedAt = -1; hob = tally = null;
                art = wave == 12 ? "Prop_SewnReply" : wave == 15 ? "Prop_WetUmbrella" : wave == 17 ? "Prop_OrdinaryStool" : wave == 18 ? "Prop_WaitingNote" : null;
                chapter = System.Array.FindIndex(MienChoStory.Chapters, entry => entry.Wave == wave && entry.Shop);
                evidence.sprite = ActOneArtwork.Picture(art);
            }
            if (owner.PresentationPaused) { if (wave == 17 && visible) PerformRest(); return; }
            visible = evidence.sprite != null && chapter >= 0 && chapter < MienChoStory.Available(state);
            float height = wave == 15 ? 1.5f : wave == 17 ? .85f : .65f;
            var ink = evidence.sprite == null ? default : SpriteInkMetrics.BoundsOf(evidence.sprite);
            float scale = height / Mathf.Max(.01f, ink.height);
            var size = ink.size * scale;
            float fit = 1;
            anchor = wave == 15 ? new Vector2(-2.7f,-3.8f) : new Vector2(-4.6f,-1.8f);
            if (visible && ShopLayout.Active != null)
            {
                layoutId = "story:rest:" + wave;
                visible = ShopLayout.Active.Place(layoutId, anchor, size, 180, true, false, out anchor, out fit);
            }
            evidence.enabled = visible;
            if (visible)
            {
                evidence.transform.localScale = Vector3.one * scale * fit;
                evidence.sortingOrder = WorldSpriteGrounding.SortAt(anchor.y);
                suspended.Display(anchor, owner.PresentationClock, NightmareRules.Depth(wave));
            }
            receipt.enabled = false;
            if (wave == 17 && visible) PerformRest();
        }
        private void PerformRest()
        {
            if (!ReferenceEquals(castSimulation, owner.Simulation)) { castSimulation = owner.Simulation; hob = tally = null; }
            if (!IsActor(hob,"Hob")) hob = null;
            if (!IsActor(tally,"Tally")) tally = null;
            if (hob == null || tally == null)
                foreach (var actor in FindObjectsByType<UnitVisual>())
                {
                    if (IsActor(actor,"Hob")) hob = actor;
                    if (IsActor(actor,"Tally")) tally = actor;
                }
            if (hob != null)
            {
                // Seat the existing actor visually. Its simulation, equipment and roster entry are untouched.
                Vector2 seat = new Vector2(evidence.bounds.center.x,Mathf.Lerp(evidence.bounds.min.y,evidence.bounds.max.y,.88f));
                Vector2 destination = seat + Vector2.up * K8Motion.BodyHeight(hob.State) * .5f;
                hobLight = destination;
                Vector2 delta = destination - hob.State.Position;
                hob.transform.position = destination;
                hob.TransportShadow(delta, seat.y - anchor.y);
                var body = hob.transform.Find("BodyVisual")?.GetComponent<SpriteRenderer>();
                if (body != null)
                {
                    body.sprite = ActOneArtwork.Picture("Hob") ?? body.sprite;
                    body.transform.localRotation = evidence.transform.rotation;
                    body.sortingOrder = evidence.sortingOrder + 10;
                }
            }
            if (tally == null || receipt.sprite == null) return;
            Vector2 point = tally.transform.position;
            if (noticedAt < 0 && Vector2.Distance(owner.Simulation.Champion.Position, point) < 2.1f) noticedAt = owner.PresentationClock;
            float fold = noticedAt < 0 ? 0 : Mathf.Clamp01((owner.PresentationClock - noticedAt) / .65f);
            receipt.enabled = fold < 1;
            float width = .45f / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(receipt.sprite).width);
            receipt.transform.localScale = new Vector3(width * Mathf.Lerp(1,.12f,fold), width * Mathf.Lerp(1,.45f,fold), 1);
            receipt.transform.position = point + new Vector2(.28f, -.05f - fold * .16f);
            receipt.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(-13,24,fold));
            receipt.sortingOrder = WorldSpriteGrounding.SortAt(point.y) + 18;
        }
        private bool IsActor(UnitVisual actor,string name)
        {
            if (actor == null || !actor.gameObject.activeInHierarchy || actor.State?.Definition.DisplayName != name || actor.State.Faction != Faction.Companion) return false;
            foreach (var unit in owner.Simulation.Units) if (ReferenceEquals(unit,actor.State)) return true;
            return false;
        }
        private void ReleaseLayout()
        {
            if (layoutId != null) ShopLayout.Active?.Release(layoutId);
            layoutId = null;
        }
        private bool Inspect()
        {
            if (!Rest || !visible || owner.PresentationPaused || owner.StoryConsumesInput || owner.Details.Open || owner.Shop?.Animating == true || !ShopFocus.Has(this, 0, owner.Simulation.Champion.Position)) return false;
            owner.GetComponent<MienChoPresentation>().Show(chapter); return true;
        }
        private void Update()
        {
            if ((Application.isFocused || RunPaths.Diagnostic) && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0))) Inspect();
        }
        private float promptAt; private int promptFrame;
        private void OnGUI()
        {
            if (!Rest || !visible || owner.StoryConsumesInput || owner.PresentationPaused || Camera.main == null || !ShopFocus.Has(this,0,owner.Simulation.Champion.Position)) return;
            var matrix = ArcadeTypography.BeginViewport(Camera.main.pixelRect,1920,1080);
            if (GUI.Button(StoryPropInput.ClickRect(evidence,Camera.main),GUIContent.none,GUIStyle.none)) Inspect();
            // the shop's one prompt card (requirement 25/10/2026: every nearby thing uses the same card)
            string line = wave == 17 ? "HOB CHOSE THIS SEAT" : wave == 18 ? "TALLY'S PLACE IS EMPTY" : wave == 15 ? "THE UMBRELLA IS STILL WET" : "THE REPLY WAS SEWN SHUT";
            if (Time.frameCount - promptFrame > 3) promptAt = Time.unscaledTime;
            promptFrame = Time.frameCount;
            if (ShopPrompt.Draw(line, "Something was left here. A story page waits.", "EXAMINE", Time.unscaledTime - promptAt)) Inspect();
            GUI.matrix = matrix;
        }
        private void OnDestroy() { ReleaseLayout(); if (Active == this) Active = null; ShopFocus.Unregister(this); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() => Active = null;
    }
}
