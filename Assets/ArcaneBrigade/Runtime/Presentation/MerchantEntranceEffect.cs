using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MerchantEntranceEffect : MonoBehaviour
    {
        private ArenaController owner;
        private SpriteRenderer[] fragments;
        private float started;
        private bool special;
        public static void Play(ArenaController controller, Transform parent, Vector2 position, bool dream)
        {
            if (controller.Profile != null && controller.Profile.ReducedMotion) return;
            var child = new GameObject("MerchantEntranceConfetti"); child.transform.SetParent(parent, false); child.transform.localPosition = position + Vector2.down * 0.6f;
            MerchantEntranceEffect effect = child.AddComponent<MerchantEntranceEffect>(); effect.owner = controller; effect.started = controller.Simulation.VisualElapsed; effect.special = dream;
            effect.fragments = new SpriteRenderer[9];
            for (int index = 0; index < effect.fragments.Length; index++)
            {
                var fragment = new GameObject("FoldedDream_" + index); fragment.transform.SetParent(child.transform, false);
                SpriteRenderer renderer = fragment.AddComponent<SpriteRenderer>(); renderer.sprite = dream ? controller.Content.EggSprite : controller.Content.Arena.Coin; renderer.sortingOrder = 1150;
                effect.fragments[index] = renderer;
            }
        }
        private void Update()
        {
            if (owner == null || owner.Simulation == null) { Destroy(gameObject); return; }
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            float progress = Mathf.Clamp01((owner.Simulation.VisualElapsed - started) / 0.85f);
            for (int index = 0; index < fragments.Length; index++)
            {
                float angle = index * Mathf.PI * 2 / fragments.Length;
                SpriteRenderer fragment = fragments[index];
                fragment.transform.localPosition = new Vector2(Mathf.Cos(angle) * progress * 1.9f, Mathf.Sin(angle) * progress * 0.55f + Mathf.Sin(progress * Mathf.PI) * (special ? 1.5f : 0.85f));
                fragment.transform.localScale = Vector3.one * (0.17f / fragment.sprite.bounds.size.y) * (1 - progress * 0.5f);
                fragment.transform.localRotation = Quaternion.Euler(0, 0, index * 35 + progress * 150);
                fragment.color = new Color(1, 1, 1, 1 - progress);
            }
            if (progress >= 1) Destroy(gameObject);
        }
    }
}
