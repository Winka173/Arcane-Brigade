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
        private float attention;
        private bool dreamMerchant;
        private bool inspecting;
        private bool speaking;
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
        public void SetMerchantMood(bool special) { dreamMerchant = special; }
        public void Inspect(bool active) { inspecting = active; }
        public void Speak(bool active) { speaking = active; }
        public void Arrive(float delay) { arrivalStarted = owner.PresentationClock; arrivalDelay = delay; }
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
                view.sprite = frames[frame];
            }
            if (view.sprite == null) return;
            float breathing = reduced ? 0 : Mathf.Sin(time * 2.5f) * 0.04f;
            if (speaking && !reduced) breathing += Mathf.Sin(owner.PresentationClock * 19) * 0.012f;
            float scale = Mathf.Min(height / view.sprite.bounds.size.y, height * 1.2f / view.sprite.bounds.size.x);
            float arrival = reduced ? 1 : Mathf.Clamp01((owner.PresentationClock - arrivalStarted - arrivalDelay) / 0.42f);
            float settle = Mathf.Sin(arrival * Mathf.PI) * 0.12f;
            transform.localScale = new Vector3(scale * (1 - breathing * 0.4f + settle), scale * (1 + breathing - settle), 1) * Mathf.SmoothStep(0.05f, 1, arrival);
            transform.localPosition = origin + new Vector3(reduced ? 0 : attention * 0.035f + (denied ? Mathf.Sin(time * 45) * 0.04f : 0), reduced ? 0 : Mathf.Sin(time * (dreamMerchant ? 1.4f : 2)) * 0.035f + (cheer ? dreamMerchant ? -0.06f : Mathf.Abs(Mathf.Sin(time * 10)) * 0.13f : 0) + (1 - arrival) * 0.65f, 0);
            transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Sin(time * 1.7f) * 2.5f + attention * -2 + (dreamMerchant && cheer ? -5 : 0));
            view.sortingOrder = 1000 - Mathf.RoundToInt(transform.position.y * 12);
            if (grounded == null) { grounded = gameObject.AddComponent<GroundShadow>(); grounded.Configure(view.sprite); }
            Vector3 ground = transform.parent.TransformPoint(origin - Vector3.up * height * (view.sprite.pivot.y / view.sprite.rect.height));
            grounded.Display(view.sprite, ground, height * 0.72f, (1 - arrival) * 0.65f, 0.34f);
        }
    }
}
