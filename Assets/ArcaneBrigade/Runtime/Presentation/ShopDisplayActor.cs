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
        private ArenaController owner;
        public void Configure(SpriteRenderer renderer, Sprite[] animation, float bodyHeight, ArenaController controller, float offset)
        {
            view = renderer; frames = animation; height = bodyHeight; owner = controller; phase = offset;
            origin = transform.localPosition;
        }
        public void Celebrate() { cheerUntil = Time.time + 0.8f; }
        private void Update()
        {
            if (view == null || owner == null || owner.Simulation.Phase == RunPhase.Paused) return;
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            float time = owner.Simulation.VisualElapsed + phase;
            bool cheer = Time.time < cheerUntil;
            if (frames != null && frames.Length > 0)
            {
                int frame = frames.Length == 4 ? UnitVisual.AnimationFrame(time, 0, false, reduced) : reduced ? 0 : Mathf.FloorToInt(time * (cheer ? 10 : 1.8f)) % frames.Length;
                if (frames.Length == 4 && cheer && !reduced) frame = 2 + Mathf.FloorToInt(time * 8) % 2;
                view.sprite = frames[frame];
            }
            if (view.sprite == null) return;
            float breathing = reduced ? 0 : Mathf.Sin(time * 2.5f) * 0.04f;
            float scale = Mathf.Min(height / view.sprite.bounds.size.y, height * 1.2f / view.sprite.bounds.size.x);
            transform.localScale = new Vector3(scale * (1 - breathing * 0.4f), scale * (1 + breathing), 1);
            transform.localPosition = origin + Vector3.up * (reduced ? 0 : Mathf.Sin(time * 2) * 0.035f + (cheer ? Mathf.Abs(Mathf.Sin(time * 10)) * 0.13f : 0));
            transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Sin(time * 1.7f) * 2.5f);
            view.sortingOrder = 1000 - Mathf.RoundToInt(transform.position.y * 12);
        }
    }
}
