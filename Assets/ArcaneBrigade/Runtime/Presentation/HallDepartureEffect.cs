using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class HallDepartureEffect : MonoBehaviour
    {
        private ArenaController owner;
        private SpriteRenderer[] motes;
        private Vector2 doorway;
        private float started;
        public static void Play(ArenaController controller, Vector2 exit)
        {
            var child = new GameObject("HallDepartureRibbon");
            HallDepartureEffect effect = child.AddComponent<HallDepartureEffect>();
            effect.owner = controller; effect.doorway = exit; effect.started = controller.PresentationClock;
            effect.motes = new SpriteRenderer[12];
            for (int index = 0; index < effect.motes.Length; index++)
            {
                var mote = new GameObject("DoorSpark_" + index); mote.transform.SetParent(child.transform, false);
                SpriteRenderer renderer = mote.AddComponent<SpriteRenderer>(); renderer.sprite = controller.Content.Arena.ImpactSprite;
                renderer.sortingOrder = 1100; effect.motes[index] = renderer;
            }
        }
        private void Update()
        {
            if (owner == null || owner.Simulation == null) { Destroy(gameObject); return; }
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            float age = owner.PresentationClock - started;
            if (age >= MarketMotion.DepartureDuration) { Destroy(gameObject); return; }
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            float fade = Mathf.Sin(Mathf.Clamp01(age / MarketMotion.DepartureDuration) * Mathf.PI);
            for (int index = 0; index < motes.Length; index++)
            {
                SpriteRenderer mote = motes[index];
                float progress = reduced ? 0.5f : Mathf.Repeat(age * 1.3f + index / (float)motes.Length, 1);
                Vector2 position = Vector2.Lerp(owner.Simulation.Champion.Position, doorway + Vector2.down * 0.65f, progress);
                position.x += reduced ? 0 : Mathf.Sin(index * 2.4f + progress * Mathf.PI) * (1 - progress) * 0.35f;
                mote.transform.position = position;
                if (mote.sprite != null) mote.transform.localScale = Vector3.one * (0.11f / mote.sprite.bounds.size.y);
                mote.color = new Color(0.67f, 0.93f, 0.92f, fade * (reduced ? 0.1f : 0.65f) * Mathf.Sin(progress * Mathf.PI));
            }
        }
    }
}
