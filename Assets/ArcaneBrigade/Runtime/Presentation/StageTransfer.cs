using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum StageCarrier { Hand, CarpetTongue, KeyholeEye }

    public sealed class StageTransfer : MonoBehaviour
    {
        private ArenaController owner;
        private Action completed;
        private SpriteRenderer courier;
        private Sprite[] frames;
        private UnitVisual[] passengers;
        private Vector2 origin;
        private float age;
        private bool active;
        private GroundShadow grounded;
        private bool outsideHand;
        private readonly SpriteRenderer[] trail = new SpriteRenderer[6];
        private Sprite[] dust;
        private SpriteRenderer giant; // the Keeper itself, looming behind whichever part of it is carrying the shop
        public StageCarrier Kind { get; private set; }
        public bool Active => active;
        public float ElapsedSeconds => age;
        public Vector2 VisualPlayerPosition => PresentationSettings.ReducedMotion ? origin : PassengerPosition(origin, age, Kind);
        // Act I moves to a new zone at waves 6, 11 and 16; endless keeps changing every five waves.
        public static bool ChangesMap(int wave) => wave > 1 && (wave - 1) % 5 == 0;
        public static StageCarrier CarrierFor(int wave) => (StageCarrier)(Mathf.Max(0, (wave - 6) / 5) % 3);

        public void Configure(ArenaController controller, Action onComplete)
        {
            owner = controller; completed = onComplete; active = true;
            origin = owner.Simulation.Champion.Position;
            CarnivalStageAssets assets = Resources.Load<CarnivalStageAssets>("CarnivalStageAssets");
            frames = assets != null ? assets.Courier : new Sprite[0];
            OutsideStageAssets outside = OutsideStageAssets.Current;
            if (outside != null && outside.BossFrames.Length == 16)
            {
                outsideHand = true;
                Kind = owner.Session.State.OutsideStage == OutsideStageStatus.Fighting ? StageCarrier.Hand : CarrierFor(owner.Session.State.Wave);
                int first = Kind == StageCarrier.CarpetTongue ? 8 : Kind == StageCarrier.KeyholeEye ? 12 : 4;
                frames = new[] { outside.BossFrames[first], outside.BossFrames[first + 1], outside.BossFrames[first + 2] };
            }
            passengers = FindObjectsByType<UnitVisual>();
            CarrierArtwork artwork = CarrierArtwork.Current;
            if (artwork != null && artwork.For(Kind)?.Length == 3) { frames = artwork.For(Kind); outsideHand = true; }
            Sprite carrier = ActOneArtwork.Picture(Kind == StageCarrier.Hand ? "Keeper_Hand_Intact" : Kind == StageCarrier.CarpetTongue ? "Keeper_Carpet_Intact" : "Keeper_Eye_Open");
            if (carrier != null)
            {
                string stem = Kind == StageCarrier.Hand ? "Keeper_Hand_" : Kind == StageCarrier.CarpetTongue ? "Keeper_Carpet_" : "Keeper_Eye_";
                Sprite closed = Kind == StageCarrier.KeyholeEye ? ActOneArtwork.Picture(stem + "Closed") ?? carrier : carrier;
                Sprite charged = ActOneArtwork.Picture(stem + "Charged") ?? carrier;
                Sprite released = Kind == StageCarrier.KeyholeEye ? carrier : ActOneArtwork.Picture(stem + "Release") ?? carrier;
                frames = new[] { closed, charged, released }; outsideHand = true;
            }
            courier = gameObject.AddComponent<SpriteRenderer>(); courier.sortingOrder = 830; // in front of the Keeper's looming body (820)
            grounded = gameObject.AddComponent<GroundShadow>();
            PaintedAnimationLibrary painted = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            dust = painted != null ? painted.Dust : null;
            for (int index = 0; index < trail.Length; index++)
            {
                var child = new GameObject("CarrierLandingDust" + index);
                child.transform.SetParent(owner.transform, false); trail[index] = child.AddComponent<SpriteRenderer>();
                trail[index].sortingOrder = 420;
            }
            CheckpointAudio.PlayInterfaceId("shop.descend");
            CheckpointAudio.PlayInterfaceId("boss.telegraph");
            // the hand, the carpet and the eye are one creature: its body shows for a moment above the room, dark and enormous
            Sprite body = Resources.Load<Sprite>("MienCho/ActOne/Keeper_Silhouette") ?? (outside != null && outside.BossFrames.Length > 0 ? outside.BossFrames[0] : null);
            body = null; // retired 19/10/2026: the faint Keeper over the top of the screen read as a blur bug
            if (body != null && owner.Session.State.OutsideStage != OutsideStageStatus.Fighting)
            {
                var looming = new GameObject("TheKeeperLooms"); looming.transform.SetParent(owner.transform, false);
                giant = looming.AddComponent<SpriteRenderer>(); giant.sprite = body; giant.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.DontSave }; giant.sortingOrder = 820; giant.color = Color.clear; // above the night fog (800) so it reads as a shape, under every body
            }
        }

        public static Vector2 PassengerPosition(Vector2 start, float seconds)
            => PassengerPosition(start, seconds, StageCarrier.Hand);

        public static Vector2 PassengerPosition(Vector2 start, float seconds, StageCarrier kind)
        {
            Vector2 destination = new Vector2(0, -0.2f);
            Vector2 release = kind == StageCarrier.KeyholeEye ? Vector2.Lerp(start, destination, 0.5f) + Vector2.up * 1.5f : Vector2.Lerp(start, destination, 0.75f) + Vector2.up;
            if (seconds < 0.4f) return start;
            if (seconds < 1.3f)
            {
                float progress = Mathf.SmoothStep(0, 1, (seconds - 0.4f) / 0.9f);
                Vector2 pickup = Vector2.Lerp(start, release, progress);
                if (kind == StageCarrier.CarpetTongue) pickup += Vector2.right * Mathf.Sin(progress * Mathf.PI) * 0.7f;
                return pickup;
            }
            float flight = Mathf.Clamp01((seconds - 1.3f) / 0.9f);
            return Vector2.Lerp(release, destination, flight) + Vector2.up * Mathf.Sin(flight * Mathf.PI) * (kind == StageCarrier.KeyholeEye ? 2.1f : 1.4f);
        }

        private void Update()
        {
            if (!active || owner == null || owner.PresentationPaused) return;
            float previousAge = age;
            age += Mathf.Clamp(Time.unscaledDeltaTime, 0, 0.05f);
            if (!PresentationSettings.ReducedMotion)
            {
                if (previousAge < .4f && age >= .4f) CheckpointAudio.PlayInterfaceId("boss.attack");
                if (previousAge < 1.3f && age >= 1.3f) CheckpointAudio.PlayInterfaceId("voice.dream.gasp");
                if (previousAge < 2.2f && age >= 2.2f) CheckpointAudio.PlayInterfaceId("boss.enter");
            }
            bool reduced = PresentationSettings.ReducedMotion;
            for (int index = 0; index < trail.Length; index++)
            {
                SpriteRenderer cloud = trail[index];
                float birth = index < 3 ? .38f + index * .035f : 2.15f + (index - 3) * .035f;
                float progress = Mathf.Clamp01((age - birth) / .4f);
                cloud.enabled = !reduced && !PresentationSettings.ReducedEffects && dust != null && dust.Length > 0 && age >= birth && progress < 1;
                if (!cloud.enabled) continue;
                cloud.sprite = dust[Mathf.Min(dust.Length - 1, Mathf.FloorToInt(progress * dust.Length))];
                Vector2 basePosition = index < 3 ? origin : new Vector2(0, -.2f);
                cloud.transform.position = basePosition + new Vector2((index % 3 - 1) * (.3f + progress * .5f), -.3f + progress * .15f);
                cloud.transform.rotation = Quaternion.identity;
                float width = (.45f + progress * .6f) / Mathf.Max(.01f, cloud.sprite.bounds.size.x);
                cloud.transform.localScale = Vector3.one * width;
                cloud.color = new Color(.72f, .65f, .77f, (1 - progress) * .38f);
            }
            if (frames.Length > 0)
            {
                int pose = Kind == StageCarrier.KeyholeEye ? age < 0.6f ? 0 : age < 1 ? 1 : age < 1.3f ? 2 : 0 : Kind == StageCarrier.CarpetTongue ? age < 0.35f ? 0 : age < 0.85f ? 1 : age < 1.3f ? 0 : 2 : age < 0.45f ? 0 : age < 1.3f ? 1 : 2;
                courier.sprite = frames[outsideHand ? reduced ? 0 : pose : reduced ? 0 : 2 + Mathf.FloorToInt(age * 7) % 4];
                float width = outsideHand ? Kind == StageCarrier.KeyholeEye ? 4.2f : Kind == StageCarrier.CarpetTongue ? 5.8f : 7.2f : 3.8f;
                courier.transform.localScale = Vector3.one * (width / Mathf.Max(0.01f, courier.sprite.bounds.size.x));
                if (!reduced)
                {
                    // secondary motion: a squash as it grabs the passenger (0.4 s), then a slow breathing stretch while it carries
                    float grab = Mathf.Exp(-Mathf.Pow((age - 0.42f) / 0.08f, 2)), carry = Mathf.Clamp01((age - 0.5f) / 0.3f) * Mathf.Sin(age * 9) * 0.03f;
                    courier.transform.localScale = Vector3.Scale(courier.transform.localScale, new Vector3(1 + 0.08f * grab - carry, 1 - 0.1f * grab + carry, 1));
                }
                Vector2 passenger = PassengerPosition(origin, age, Kind);
                Vector2 approach = Kind == StageCarrier.KeyholeEye ? Vector2.up * 6 : Kind == StageCarrier.CarpetTongue ? Vector2.down * 6 : Vector2.left * 8;
                Vector2 position = age < 0.4f ? Vector2.Lerp(origin + approach, origin, Mathf.SmoothStep(0, 1, age / 0.4f)) : age < 1.3f ? passenger : Vector2.Lerp(PassengerPosition(origin, 1.3f, Kind), new Vector2(9, 4), Mathf.Clamp01((age - 1.3f) / 1.1f));
                float offset = Kind == StageCarrier.KeyholeEye ? 1.1f : outsideHand ? 1.25f : 0.45f;
                transform.position = reduced ? origin + Vector2.up : position + Vector2.down * offset;
                float rotation = Kind == StageCarrier.KeyholeEye ? Mathf.Sin(age * 2) * 5 : age < 1.3f ? Mathf.Sin(age * 2) * -8 : Mathf.Lerp(-8, Kind == StageCarrier.CarpetTongue ? -35 : 22, Mathf.Clamp01((age - 1.3f) / 0.5f));
                transform.rotation = Quaternion.Euler(0, 0, outsideHand && !reduced ? rotation : 0);
                courier.color = new Color(1, 1, 1, Mathf.Clamp01((2.6f - age) * 2));
                grounded.Configure(courier.sprite);
                grounded.Display(courier.sprite, (Vector2)transform.position - Vector2.up * (reduced ? 0 : 1.1f), width, reduced ? 0 : 1.1f, 0.22f * courier.color.a);
            }
            if (giant != null && Camera.main != null)
            {
                Camera view = Camera.main; float top = view.transform.position.y + view.orthographicSize, wide = view.orthographicSize * view.aspect * 2.1f;
                float share = Mathf.Clamp01(age / 0.5f) * Mathf.Clamp01((2.5f - age) / 0.5f);
                // measured by its painted ink, not the sprite's transparent margins: a quarter of it hangs into the room at
                // first, rising to two fifths as it carries the shop
                Rect ink = SpriteInkMetrics.BoundsOf(giant.sprite);
                float scale = wide / Mathf.Max(0.01f, ink.width), hang = reduced ? 0.33f : Mathf.Lerp(0.25f, 0.4f, Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 1.6f)));
                giant.transform.localScale = Vector3.one * scale;
                float bottom = top - ink.height * scale * hang;
                giant.transform.position = new Vector3(view.transform.position.x - ink.center.x * scale, bottom - ink.yMin * scale, 0);
                giant.color = new Color(0.32f, 0.26f, 0.42f, 0.55f * share); // night violet: dark enough to be a shape behind the room, light enough to read on its floor
            }
            if (age < (reduced ? 0.6f : 2.6f)) return;
            if (giant != null) Destroy(giant.gameObject);
            active = false;
            Vector2 destination = new Vector2(0, -0.2f);
            foreach (UnitState unit in owner.Simulation.Units)
                if (unit.Alive && unit.Faction != Faction.Enemy)
                {
                    Vector2 relative = unit.Position - origin;
                    unit.Position = unit.PreviousPosition = owner.Simulation.ClampPosition(destination + relative, unit.Definition.Radius);
                }
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = destination;
            RestorePassengers(); completed?.Invoke(); Destroy(gameObject);
        }

        private void LateUpdate()
        {
            if (!active || PresentationSettings.ReducedMotion) return;
            Vector2 offset = PassengerPosition(origin, age, Kind) - origin;
            foreach (UnitVisual passenger in passengers)
                if (passenger != null && passenger.State != null && passenger.State.Alive && passenger.State.Faction != Faction.Enemy)
                {
                    passenger.transform.position = (Vector3)(passenger.State.Position + offset);
                    float grip = Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - .4f) / .6f));
                    float landing = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - 1.6f) / .6f));
                    passenger.transform.rotation = Quaternion.Euler(0, 0, grip * landing * (Kind == StageCarrier.CarpetTongue ? -14 : 9));
                    float lift = age < 0.4f ? 0 : age < 1.3f ? Mathf.SmoothStep(0, 1, (age - 0.4f) / 0.9f) : Mathf.Max(0, 1 - (age - 1.3f) / 0.9f) + Mathf.Sin(Mathf.Clamp01((age - 1.3f) / 0.9f) * Mathf.PI) * 1.4f;
                    passenger.TransportShadow(offset, lift);
                }
        }
        private void RestorePassengers()
        {
            if (passengers == null) return;
            foreach (UnitVisual passenger in passengers) if (passenger != null) passenger.transform.rotation = Quaternion.identity;
        }
        private void OnDestroy()
        {
            RestorePassengers();
            foreach (SpriteRenderer cloud in trail) if (cloud != null) Destroy(cloud.gameObject);
            if (giant != null) Destroy(giant.gameObject);
        }
    }
}
