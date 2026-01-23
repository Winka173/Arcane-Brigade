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
        public void Configure(SpriteRenderer renderer, Sprite[] animation, float bodyHeight, ArenaController controller, float offset)
        {
            view = renderer; frames = animation; height = bodyHeight; owner = controller; phase = offset;
            origin = transform.localPosition;
        }
        public void Celebrate() { cheerUntil = owner.PresentationClock + 0.8f; }
        public void Reject() { denialUntil = owner.PresentationClock + 0.55f; }
        public void LookAt(float direction) { attention = Mathf.Clamp(direction, -1, 1); }
        public void SetMerchantMood(bool special)
        {
            dreamMerchant = special; merchantActor = true;
            if (special)
            {
                emergence = new Material(Resources.Load<Shader>("MarketEmergence"));
                view.sharedMaterial = emergence;
            }
        }
        private void OnDestroy() { if (emergence != null) Destroy(emergence); }
        public void Inspect(bool active) { inspecting = active; }
        public void SetPedestalDisplay() { pedestalDisplay = true; }
        public void Speak(bool active) { speaking = active; }
        public void SetCompanionDisplay(bool companion)
        {
            companionDisplay = companion;
            if (companion) { height *= 1.3f; origin += Vector3.up * 0.12f; }
        }
        public void SetFloorFixture()
        {
            floorFixture = true;
            emergence = new Material(Resources.Load<Shader>("MarketEmergence")); view.sharedMaterial = emergence;
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
            bool cheer = owner.PresentationClock < cheerUntil;
            bool denied = owner.PresentationClock < denialUntil;
            if (frames != null && frames.Length > 0)
            {
                int frame = frames.Length == 4 ? UnitVisual.AnimationFrame(time, 0, false, reduced) : reduced ? 0 : Mathf.FloorToInt(time * (cheer ? 10 : 1.8f)) % frames.Length;
                if (frames.Length == 4 && cheer && !reduced) frame = 2 + Mathf.FloorToInt(time * 8) % 2;
                if (frames.Length == 4 && denied && !reduced) frame = 1;
                else if (frames.Length == 4 && inspecting && !cheer && !reduced) frame = 2;
                if (frames.Length == 4 && speaking && !cheer && !denied && !reduced) frame = 2 + Mathf.FloorToInt(owner.PresentationClock * 9.5f) % 2;
                if (pedestalDisplay) frame = reduced ? 0 : Mathf.FloorToInt(time * 3) % frames.Length;
                view.sprite = frames[frame];
            }
            if (view.sprite == null) return;
            float breathing = reduced ? 0 : Mathf.Sin(time * (companionDisplay ? 2.1f : 2.5f)) * (companionDisplay ? 0.065f : 0.04f);
            if (speaking && !reduced) breathing += Mathf.Sin(owner.PresentationClock * 19) * 0.012f;
            float scale = Mathf.Min(height / view.sprite.bounds.size.y, height * 1.2f / view.sprite.bounds.size.x);
            float age = owner.PresentationClock - arrivalStarted - arrivalDelay;
            float duration = merchantActor ? 0.8f : 0.55f;
            float arrival = Mathf.Clamp01(age / duration);
            float travel = departing ? arrival : 1 - arrival;
            Vector3 displacement;
            if (floorFixture) displacement = Vector3.down * height * Mathf.SmoothStep(0, 1, travel);
            else if (merchantActor) displacement = Vector3.up * (dreamMerchant ? -height * Mathf.SmoothStep(0, 1, travel) : 6 * travel * travel);
            else displacement = launchOffset * Mathf.SmoothStep(0, 1, travel) + Vector3.up * Mathf.Sin(arrival * Mathf.PI) * 1.2f;
            float settle = !departing && !reduced ? MarketMotion.Settle(age, duration, merchantActor ? 0.15f : 0.09f) : 0;
            if (reduced) displacement = Vector3.zero;
            transform.localScale = new Vector3(scale * (1 - breathing * 0.4f + settle), scale * (1 + breathing - settle), 1);
            transform.localPosition = origin + displacement + new Vector3(reduced ? 0 : attention * 0.035f + (denied ? Mathf.Sin(time * 45) * 0.04f : 0), reduced ? 0 : Mathf.Sin(time * (dreamMerchant ? 1.4f : 2)) * (merchantActor || companionDisplay ? 0.035f : 0.11f) + (cheer ? dreamMerchant ? -0.06f : Mathf.Abs(Mathf.Sin(time * 10)) * 0.13f : 0), 0);
            Color tint = view.color; tint.a = departing ? 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.72f, 1, arrival)) : age < 0 ? 0 : reduced ? arrival : 1; view.color = tint;
            transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Sin(time * 1.7f) * 2.5f + attention * -2 + (dreamMerchant && cheer ? -5 : 0));
            view.sortingOrder = 1000 - Mathf.RoundToInt(transform.position.y * 12);
            if (grounded == null) { grounded = gameObject.AddComponent<GroundShadow>(); grounded.Configure(view.sprite); }
            Vector3 ground = transform.parent.TransformPoint(origin - Vector3.up * height * (view.sprite.pivot.y / view.sprite.rect.height));
            if (emergence != null) emergence.SetFloat("_FloorY", !reduced && travel > 0 ? ground.y : -10000);
            grounded.Display(view.sprite, ground, height * 0.72f, Mathf.Abs(displacement.y), 0.34f * view.color.a);
        }
    }
}
