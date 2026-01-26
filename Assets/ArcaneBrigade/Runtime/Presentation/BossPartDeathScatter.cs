using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BossPartDeathScatter : MonoBehaviour
    {
        private Vector3 initial;
        private Color tint;
        private SpriteRenderer sprite;
        private Vector3 velocity;
        private float age;
        public void Configure(SpriteRenderer renderer, int part)
        {
            sprite = renderer; initial = transform.localPosition; tint = sprite.color;
            velocity = part == 0 ? new Vector3(-0.9f, 0.7f) : part == 1 ? new Vector3(0.9f, 0.7f) : new Vector3(0, 1.1f);
        }
        public void Step(float delta)
        {
            age += delta;
            bool reduced = PresentationSettings.ReducedMotion;
            transform.localPosition = initial + (reduced ? Vector3.zero : velocity * age + Vector3.down * age * age * 1.6f);
            transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : velocity.x * age * 170);
            Color fade = tint; fade.a *= Mathf.Clamp01(1 - age / 0.85f); sprite.color = fade;
        }
    }
}
