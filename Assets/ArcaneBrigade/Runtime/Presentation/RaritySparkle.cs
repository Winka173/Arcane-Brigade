using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A high-rarity friend glints (19/10/2026): small inked stars twinkle on its body now and then - one at a
    /// time for a Rare, a pair for a Mythic, three in shifting colours for a Prismatic. Each glint swells and fades where it
    /// is; nothing radiates outward. Reduced effects keeps a single slow glint; reduced motion holds them still.</summary>
    [DefaultExecutionOrder(560)]
    public sealed class RaritySparkle : MonoBehaviour
    {
        private UnitVisual view;
        private SpriteRenderer body;
        private SpriteRenderer[] glints = new SpriteRenderer[0];
        private ItemRarity rarity;
        private float seed;
        public static int Count(ItemRarity rarity) => rarity == ItemRarity.Rainbow ? 3 : rarity == ItemRarity.Mythic ? 2 : rarity == ItemRarity.Rare ? 1 : 0;
        private static readonly InkPalette[] Prismatic = { ArsenalInk.Gold, ArsenalInk.Candy, ArsenalInk.Water, ArsenalInk.Green, ArsenalInk.Violet };

        public static void Ensure(UnitVisual view, UnitState state)
        {
            ItemRarity rarity = state != null && state.Faction == Faction.Companion && state.Definition != null ? state.Definition.Rarity : ItemRarity.Common;
            RaritySparkle sparkle = view.GetComponent<RaritySparkle>();
            if (Count(rarity) == 0) { if (sparkle != null) sparkle.Clear(); return; }
            if (sparkle == null) sparkle = view.gameObject.AddComponent<RaritySparkle>();
            sparkle.Configure(view, rarity, state.Id);
        }
        private void Configure(UnitVisual owner, ItemRarity value, int id)
        {
            if (view == owner && rarity == value && glints.Length == Count(value)) return;
            Clear(); view = owner; rarity = value; seed = id * .618f;
            foreach (SpriteRenderer part in owner.GetComponentsInChildren<SpriteRenderer>(true)) if (part.name == "BodyVisual") { body = part; break; }
            glints = new SpriteRenderer[Count(value)];
            for (int i = 0; i < glints.Length; i++)
            {
                var child = new GameObject("RarityGlint" + i); child.transform.SetParent(transform, false);
                glints[i] = child.AddComponent<SpriteRenderer>();
            }
        }
        private void Clear()
        {
            foreach (SpriteRenderer glint in glints) if (glint != null) Destroy(glint.gameObject);
            glints = new SpriteRenderer[0]; rarity = ItemRarity.Common;
        }
        private void LateUpdate()
        {
            if (body == null || !body.enabled || body.sprite == null || body.color.a < .2f) { foreach (SpriteRenderer g in glints) if (g != null) g.enabled = false; return; }
            bool still = PresentationSettings.ReducedMotion, calm = PresentationSettings.ReducedEffects;
            Bounds bounds = body.bounds;
            float now = Time.time;
            for (int i = 0; i < glints.Length; i++)
            {
                SpriteRenderer glint = glints[i];
                if (calm && i > 0) { glint.enabled = false; continue; }
                // each glint has its own slow cycle: it appears at a fresh spot on the body, swells, and fades
                float period = calm ? 2.6f : 1.5f + i * .35f, cycle = (now + seed * 3 + i * .53f) / period;
                int round = Mathf.FloorToInt(cycle); float t = cycle - round;
                float life = Mathf.Clamp01(t / .45f);
                float strength = still ? .8f : Mathf.Sin(life * Mathf.PI);
                glint.enabled = strength > .02f;
                if (!glint.enabled) continue;
                float u = Hash(round * 7 + i * 13 + seed), v = Hash(round * 11 + i * 5 + seed * 2);
                Vector3 at = new Vector3(Mathf.Lerp(bounds.min.x + bounds.size.x * .18f, bounds.max.x - bounds.size.x * .18f, u), Mathf.Lerp(bounds.min.y + bounds.size.y * .3f, bounds.max.y - bounds.size.y * .08f, v), 0);
                InkPalette palette = rarity == ItemRarity.Rainbow ? Prismatic[(round + i) % Prismatic.Length] : rarity == ItemRarity.Mythic ? ArsenalInk.Gold : ArsenalInk.Silver;
                glint.sprite = MechanicInk.Token(InkToken.Glint, palette);
                glint.transform.position = at;
                glint.transform.rotation = Quaternion.Euler(0, 0, still ? 0 : life * 70 + i * 30);
                float size = Mathf.Clamp(bounds.size.y * .16f, .12f, .26f) * (rarity == ItemRarity.Rainbow ? 1.15f : 1);
                glint.transform.localScale = Vector3.one * (size * (.55f + .45f * strength) / Mathf.Max(.01f, glint.sprite.bounds.size.x)) / Mathf.Max(.01f, transform.lossyScale.x);
                glint.sortingOrder = body.sortingOrder + 12;
                glint.color = new Color(1, 1, 1, strength);
            }
        }
        private static float Hash(float n) => Mathf.Repeat(Mathf.Sin(n * 12.9898f) * 43758.5453f, 1f);
    }
}
