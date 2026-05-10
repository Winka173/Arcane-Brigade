using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ShopDisplayActor : MonoBehaviour
    {
        private SpriteRenderer view;
        private Sprite[] frames;
        private float height;
        private float phase;
        private Vector3 origin;
        private float cheerUntil;
        private float denialUntil;
        private float arrivalStarted = -100;
        private float arrivalDelay;
        private Vector3 launchOffset;
        private bool merchantActor;
        public bool IsMerchant => merchantActor;
        private bool canonMerchant;
        private bool departing;
        private Material emergence;
        private float attention;
        private bool dreamMerchant;
        private bool inspecting;
        private bool speaking;
        private bool companionDisplay;
        private bool floorFixture;
        private bool pedestalDisplay;
        private ArenaController owner;
        private GroundShadow grounded;
        private bool merchandise;
        private ShopDisplayActor support;
        private Vector3 presentationOffset;
        private Vector3 travelOffset;
        private MerchantPortraitRig portrait;
        private float inspectionAt = -100;
        private float previousTime;
        private float interest;
        private DreamSuspension suspension;
        private bool standing;
        /// <summary>A visitor stands on the ground with its shadow (owner, 22/10/2026): no dream levitation, no hanging strings.</summary>
        public void SetStanding() { standing = true; }
        private Vector3 HoverOffset => owner == null || standing ? Vector3.zero : DreamLevitation.Offset(owner.PresentationClock, phase + (merchantActor ? 13 : 7), NightmareRules.Depth(owner.Session.State.Wave), PresentationSettings.ReducedMotion, merchantActor ? .85f : 1);
        private EnsembleClip handoff;
        private float handoffAt = -100;
        public bool PaintedHandoffActive => handoff != null && owner != null && owner.PresentationClock - handoffAt >= 0 && owner.PresentationClock - handoffAt < 1.05f;
        public SpriteRenderer View => view;
        public float FocusAmount => interest;
        private MaterialPropertyBlock focusProperties;
        public static float FocusScale(float focus, float age, bool reduced) => 1 + Mathf.Clamp01(focus) * (reduced ? .12f : .18f) + (reduced ? 0 : AnimationTiming.Settle(age, .48f, 2) * .075f);
        public void MountMerchandise(ShopDisplayActor pedestal = null)
        {
            merchandise = true;
            if (pedestal != null) support = pedestal;
            origin.y = 0.4f + height * view.sprite.pivot.y / view.sprite.rect.height;
        }
        public Vector3 SeatPosition
        {
            get
            {
                if (pedestalDisplay)
                {
                    Vector3 ground = transform.parent.TransformPoint(origin - Vector3.up * height * view.sprite.pivot.y / view.sprite.rect.height);
                    return ground + transform.parent.TransformVector(HoverOffset) + transform.TransformVector(WorldSpriteGrounding.Seat(view.sprite) - WorldSpriteGrounding.Foot(view.sprite));
                }
                return transform.TransformPoint(WorldSpriteGrounding.Seat(view.sprite)) - transform.parent.TransformVector(travelOffset);
            }
        }
        private SpriteRenderer heldWeapon, heldHand;
        /// <summary>The friend on its stall holds the weapon it comes with, at its side as in combat (sized by the weapon's painted
        /// ink against the body, the hand on the grip).</summary>
        public void HoldWeapon(Sprite weapon, Sprite hand, bool melee)
        {
            if (view == null || view.sprite == null || weapon == null) return;
            if (heldWeapon == null)
            {
                var w = new GameObject("HeldWeapon"); w.transform.SetParent(view.transform, false); heldWeapon = w.AddComponent<SpriteRenderer>();
                var h = new GameObject("HeldHand"); h.transform.SetParent(w.transform, false); heldHand = h.AddComponent<SpriteRenderer>();
            }
            heldWeapon.sprite = weapon; heldHand.sprite = hand;
            Bounds body = view.sprite.bounds; Rect ink = SpriteInkMetrics.BoundsOf(weapon);
            float scale = body.size.y * (melee ? .62f : .5f) / Mathf.Max(.01f, Mathf.Max(ink.width, ink.height));
            heldWeapon.transform.localScale = Vector3.one * scale;
            heldWeapon.transform.localPosition = new Vector3(body.center.x + body.extents.x * .62f, body.center.y - body.extents.y * .18f, 0);
            heldWeapon.transform.localRotation = Quaternion.Euler(0, 0, melee ? 32 : 8);
            if (hand != null) heldHand.transform.localScale = Vector3.one * (body.size.x * .16f / Mathf.Max(.01f, hand.bounds.size.x) / scale);
        }

        private void LateUpdate()
        {
            if (heldWeapon != null && view != null)
            {
                heldWeapon.sortingOrder = view.sortingOrder + 1; heldHand.sortingOrder = view.sortingOrder + 2;
                heldWeapon.color = heldHand.color = view.color;
            }
            if (view != null && view.sprite != null && (floorFixture || pedestalDisplay || merchantActor) && GetComponent<OctoberDoor>() == null)
            {
                Vector3 ground = transform.parent.TransformPoint(origin - Vector3.up * height * view.sprite.pivot.y / view.sprite.rect.height);
                Vector3 hover = transform.parent.TransformVector(HoverOffset);
                WorldSpriteGrounding.Place(view, ground + transform.parent.TransformVector(travelOffset) + hover);
                view.sortingOrder = WorldSpriteGrounding.SortAt(ground.y);
                if (grounded != null) grounded.Display(view.sprite, ground, height * .72f, Mathf.Abs(travelOffset.y) + hover.y, .34f * view.color.a);
                if (!standing && suspension == null) suspension = gameObject.AddComponent<DreamSuspension>();
                if (!standing) suspension.Display(ground, ground + hover, height * .72f, owner.PresentationClock, phase, view.sortingOrder - 1, view.color.a * Mathf.Clamp01(1 - travelOffset.magnitude), !dreamMerchant);
            }
            if (!merchandise || support == null || view.sprite == null) return;
            WorldSpriteGrounding.Place(view, support.SeatPosition + transform.parent.TransformVector(presentationOffset));
            if (grounded != null) grounded.Display(view.sprite, support.SeatPosition, height * 0.72f, Mathf.Abs(presentationOffset.y), 0.34f * view.color.a);
        }
        public void Configure(SpriteRenderer renderer, Sprite[] animation, float bodyHeight, ArenaController controller, float offset)
        {
            view = renderer; frames = animation; height = bodyHeight; owner = controller; phase = offset;
            Material painted = Resources.Load<Material>("UnitFlash");
            if (painted != null) view.sharedMaterial = painted;
            origin = transform.localPosition;
        }
        public void Celebrate() { cheerUntil = owner.PresentationClock + 0.8f; if (merchantActor) handoffAt = owner.PresentationClock; }
        public void SetMerchantIdentity(string identity)
        {
            canonMerchant = identity == "GiantGoldMerchant" && ActOneArtwork.Picture("Shopkeeper_Idle") != null;
            handoff = canonMerchant ? null : EnsembleArtwork.Current?.Merchant(identity);
        }
        public void Reject() { denialUntil = owner.PresentationClock + 0.55f; }
        public void LookAt(float direction) { attention = Mathf.Clamp(direction, -1, 1); }
        public void SetMerchantMood(bool special)
        {
            dreamMerchant = special; merchantActor = true;
        }
        private void OnDestroy() { if (emergence != null) Destroy(emergence); }
        public void Inspect(bool active)
        {
            if (active && !inspecting && owner != null) inspectionAt = owner.PresentationClock;
            inspecting = active;
        }
        public void SetPedestalDisplay() { pedestalDisplay = true; }
        public void Speak(bool active) { speaking = active; }
        public void SetCompanionDisplay(bool companion)
        {
            companionDisplay = companion;
            if (companion) height *= 1.3f;
            if (merchandise) MountMerchandise();
        }
        public void SetFloorFixture()
        {
            floorFixture = true;
        }
        public void Arrive(float delay) { Arrive(delay, transform.parent.TransformPoint(origin + Vector3.up * 2)); }
        public void Arrive(float delay, Vector3 launch)
        {
            departing = false; arrivalStarted = owner.PresentationClock; arrivalDelay = delay;
            launchOffset = transform.parent.InverseTransformPoint(launch) - origin;
        }
        public void Depart(Vector3 destination, float delay = 0)
        {
            departing = true; arrivalStarted = owner.PresentationClock; arrivalDelay = delay;
            launchOffset = transform.parent.InverseTransformPoint(destination) - origin;
        }
        private void Update()
        {
            if (view == null || owner == null || owner.Simulation.Phase == RunPhase.Paused) return;
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            float time = owner.PresentationClock + phase;
            float delta = Mathf.Clamp(owner.PresentationClock - previousTime, 0, .05f); previousTime = owner.PresentationClock;
            interest = reduced ? inspecting ? 1 : 0 : Mathf.Lerp(interest, inspecting ? 1 : 0, AnimationTiming.Blend(12, delta));
            bool cheer = owner.PresentationClock < cheerUntil;
            bool denied = owner.PresentationClock < denialUntil;
            if (frames != null && frames.Length > 0)
            {
                int frame = frames.Length == 4 ? UnitVisual.AnimationFrame(time, 0, false, reduced) : reduced ? 0 : Mathf.FloorToInt(time * (cheer ? 10 : 1.8f)) % frames.Length;
                if (frames.Length == 4 && cheer && !reduced) frame = 2 + Mathf.FloorToInt(time * 8) % 2;
                if (frames.Length == 4 && denied && !reduced) frame = 1;
                else if (frames.Length == 4 && inspecting && !cheer && !reduced) frame = 2;
                if (frames.Length == 4 && speaking && !cheer && !denied && !reduced) frame = 2 + Mathf.FloorToInt(owner.PresentationClock * 9.5f) % 2;
                if (canonMerchant) frame = denied ? 3 : cheer ? 1 : inspecting ? 2 : !reduced && Mathf.Repeat(time, 12) > 10 ? 1 : 0;
                if (pedestalDisplay) frame = reduced ? 0 : Mathf.FloorToInt(time * 3) % frames.Length;
                view.sprite = frames[frame];
            }
            if (view.sprite == null) return;
            bool paintedHandoff = merchantActor && PaintedHandoffActive && !reduced;
            if (paintedHandoff) view.sprite = handoff.At((owner.PresentationClock - handoffAt) / 1.05f);
            float breathing = reduced || floorFixture || pedestalDisplay ? 0 : Mathf.Sin(time * (companionDisplay ? 2.1f : 2.5f)) * (companionDisplay ? 0.065f : 0.04f);
            if (speaking && !reduced) breathing += Mathf.Sin(owner.PresentationClock * 19) * 0.012f;
            float scale = Mathf.Min(height / view.sprite.bounds.size.y, height * 1.2f / view.sprite.bounds.size.x);
            float age = owner.PresentationClock - arrivalStarted - arrivalDelay;
            float duration = merchantActor ? 0.8f : 0.55f;
            float arrival = Mathf.Clamp01(age / duration);
            float travel = departing ? arrival : 1 - arrival;
            Vector3 displacement;
            if (floorFixture) displacement = Vector3.up * 1.6f * travel * travel; // drops onto the floor and settles
            else if (merchantActor) displacement = dreamMerchant
                ? Vector3.up * 3.4f * Mathf.SmoothStep(0, 1, travel) + Vector3.right * Mathf.Sin(travel * Mathf.PI * 2) * .45f * travel // drifts down like a dream
                : Vector3.up * 6 * travel * travel;
            else displacement = launchOffset * Mathf.SmoothStep(0, 1, travel) + Vector3.up * Mathf.Sin(arrival * Mathf.PI) * 1.2f;
            float settle = !departing && !reduced ? MarketMotion.Settle(age, duration, merchantActor ? 0.15f : 0.09f) : 0;
            if (reduced) displacement = Vector3.zero;
            travelOffset = displacement;
            transform.localScale = new Vector3(scale * (1 - breathing * 0.4f + settle), scale * (1 + breathing - settle), 1);
            if (merchandise)
            {
                transform.localScale *= FocusScale(interest, owner.PresentationClock - inspectionAt, reduced);
                focusProperties ??= new MaterialPropertyBlock();
                view.GetPropertyBlock(focusProperties); SpriteLighting.Apply(view, focusProperties, false, true);
                focusProperties.SetColor("_RimColor", new Color(1, .96f, .8f));
                focusProperties.SetFloat("_RimAmount", Mathf.Lerp(.07f, .7f, interest));
                focusProperties.SetFloat("_Flash", interest * .055f);
                view.SetPropertyBlock(focusProperties);
            }
            transform.localPosition = origin + displacement + new Vector3(reduced ? 0 : attention * 0.035f + (denied ? Mathf.Sin(time * 45) * 0.04f : 0), reduced || merchandise || floorFixture || pedestalDisplay ? 0 : Mathf.Sin(time * (dreamMerchant ? 1.4f : 2)) * (merchantActor || companionDisplay ? 0.035f : 0.11f) + (cheer ? dreamMerchant ? -0.06f : Mathf.Abs(Mathf.Sin(time * 10)) * 0.13f : 0), 0);
            presentationOffset = transform.localPosition - origin;
            Color tint = view.color; tint.a = departing ? 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.72f, 1, arrival)) : age < 0 ? 0 : reduced ? arrival : dreamMerchant ? Mathf.Clamp01(arrival * 1.8f) : 1; view.color = tint;
            transform.localRotation = Quaternion.Euler(0, 0, reduced || floorFixture || pedestalDisplay ? 0 : Mathf.Sin(time * 1.7f) * 2.5f + attention * -2 + (dreamMerchant && cheer ? -5 : 0));
            if (floorFixture || pedestalDisplay || merchantActor) transform.localRotation *= Quaternion.Euler(0, 0, DreamLevitation.Tilt(owner.PresentationClock, phase + (merchantActor ? 13 : 7), reduced, merchantActor ? .55f : .7f));
            view.sortingOrder = merchandise || pedestalDisplay
                ? 1000 - Mathf.RoundToInt(transform.parent.position.y * 12) + (merchandise ? 8 : 0)
                : 1000 - Mathf.RoundToInt(transform.position.y * 12);
            if (grounded == null) { grounded = gameObject.AddComponent<GroundShadow>(); grounded.PinCastToFoot = true; grounded.Configure(view.sprite); }
            Vector3 ground = transform.parent.TransformPoint(origin - Vector3.up * height * (view.sprite.pivot.y / view.sprite.rect.height));
            if (emergence != null) emergence.SetFloat("_FloorY", !reduced && travel > 0 ? ground.y : -10000);
            grounded.Display(view.sprite, ground, height * 0.72f, Mathf.Abs(displacement.y), 0.34f * view.color.a);
            if (merchantActor)
            {
                if (portrait == null) portrait = gameObject.AddComponent<MerchantPortraitRig>();
                portrait.Display(view, time, speaking, owner.PresentationClock - (cheerUntil - .8f), owner.PresentationClock - (denialUntil - .55f), dreamMerchant, attention, reduced || paintedHandoff);
            }
        }
    }
}
