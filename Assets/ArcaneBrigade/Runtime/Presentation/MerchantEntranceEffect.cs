using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MerchantEntranceEffect : MonoBehaviour
    {
        private ArenaController owner;
        private SpriteRenderer[] fragments;
        private float started;
        private bool special;
        private SpriteRenderer landingDust;
        public static void Play(ArenaController controller, Transform parent, Vector2 position, bool dream)
        {
            if (controller.Profile != null && controller.Profile.ReducedMotion) return;
            var child = new GameObject("MerchantEntranceConfetti"); child.transform.SetParent(parent, false); child.transform.localPosition = position + Vector2.down * 0.6f;
            MerchantEntranceEffect effect = child.AddComponent<MerchantEntranceEffect>(); effect.owner = controller; effect.started = controller.PresentationClock + MarketMotion.MerchantArrivalDelay + (dream ? 0.2f : 0.8f); effect.special = dream;
            var dust = new GameObject("MerchantGroundRipple"); dust.transform.SetParent(child.transform, false);
            effect.landingDust = dust.AddComponent<SpriteRenderer>(); effect.landingDust.sprite = controller.Content.Arena.ImpactSprite;
            effect.landingDust.sortingOrder = 460;
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
            float elapsed = owner.PresentationClock - started;
            float progress = Mathf.Clamp01(elapsed / 0.85f);
            if (landingDust.sprite != null)
            {
                float width = Mathf.Lerp(0.6f, 5.8f, Mathf.Sqrt(progress));
                landingDust.transform.localScale = new Vector3(width / landingDust.sprite.bounds.size.x, width * 0.23f / landingDust.sprite.bounds.size.y, 1);
                landingDust.color = special ? new Color(0.65f, 0.44f, 0.96f, elapsed < 0 ? 0 : (1 - progress) * 0.32f) : new Color(0.91f, 0.79f, 0.59f, elapsed < 0 ? 0 : (1 - progress) * 0.26f);
            }
            for (int index = 0; index < fragments.Length; index++)
            {
                float angle = index * Mathf.PI * 2 / fragments.Length;
                SpriteRenderer fragment = fragments[index];
                fragment.transform.localPosition = new Vector2(Mathf.Cos(angle) * progress * 1.9f, Mathf.Sin(angle) * progress * 0.55f + Mathf.Sin(progress * Mathf.PI) * (special ? 1.5f : 0.85f));
                fragment.transform.localScale = Vector3.one * (0.17f / fragment.sprite.bounds.size.y) * (1 - progress * 0.5f);
                fragment.transform.localRotation = Quaternion.Euler(0, 0, index * 35 + progress * 150);
                fragment.color = new Color(1, 1, 1, elapsed < 0 ? 0 : 1 - progress);
            }
            if (progress >= 1) Destroy(gameObject);
        }
    }
}
