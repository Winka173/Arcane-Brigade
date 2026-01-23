using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ShopStockRetirement : MonoBehaviour
    {
        private ArenaController owner;
        private SpriteRenderer view;
        private GroundShadow shadow;
        private Vector3 origin;
        private Vector3 scale;
        private float started;
        public static void Play(ArenaController controller, SpriteRenderer source, ItemRarity rarity = ItemRarity.Common)
        {
            if (source == null || source.sprite == null || controller.Profile != null && controller.Profile.ReducedMotion) return;
            var child = new GameObject("RetiringStock"); child.transform.SetParent(controller.transform, true);
            child.transform.position = source.transform.position; child.transform.localScale = source.transform.lossyScale; child.transform.rotation = source.transform.rotation;
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = source.sprite; renderer.color = source.color; renderer.sortingOrder = source.sortingOrder;
            ShopStockRetirement motion = child.AddComponent<ShopStockRetirement>(); motion.owner = controller; motion.view = renderer; motion.origin = child.transform.position; motion.scale = child.transform.localScale; motion.started = controller.PresentationClock;
            motion.shadow = child.AddComponent<GroundShadow>();
            if (rarity != ItemRarity.Common)
            {
                var glow = new GameObject("RetiringStockRarity"); glow.transform.SetParent(controller.transform, false);
                glow.AddComponent<ShopRarityGlow>().Configure(rarity, controller, 0, renderer);
            }
        }
        private void Update()
        {
            if (owner == null || owner.Simulation == null) { Destroy(gameObject); return; }
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            float progress = Mathf.Clamp01((owner.PresentationClock - started) / 0.24f);
            transform.position = origin + Vector3.down * progress * 0.35f;
            transform.localScale = scale * (1 - Mathf.SmoothStep(0, 1, progress) * 0.7f);
            Color color = view.color; color.a = 1 - progress; view.color = color;
            shadow.Display(view.sprite, new Vector2(origin.x, origin.y - view.bounds.size.y * 0.5f), view.bounds.size.x * 0.8f, 0, 0.32f * color.a);
            if (progress >= 1 || owner.Session.State.Phase != SessionPhase.Shop) Destroy(gameObject);
        }
    }
}
