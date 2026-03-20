using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A Mythic or Prismatic offer arrives under a cloth (19/10/2026): it lands covered, the cloth trembles once
    /// (the held beat), then is whisked up and away while gold leaf - or, for a Prismatic, leaf in every colour - falls over
    /// the revealed piece. Painted RareCover_Mythic / RareCover_Prismatic art drapes it; a drawn patch stands in until the art
    /// exists. Falling and lifting only, nothing radiating from the stand. Reduced motion: the cloth simply fades.</summary>
    [DefaultExecutionOrder(520)]
    public sealed class RareUnveil : MonoBehaviour
    {
        public const float HoldAfterLanding = 0.55f, LiftSeconds = 0.6f, LeafSeconds = 1.3f;
        private SpriteRenderer target, cloth;
        private readonly SpriteRenderer[] leaves = new SpriteRenderer[9];
        private ArenaController owner;
        private float age, revealAt;
        private bool prismatic, cued;
        public bool Covered => age < revealAt;
        public bool Finished => age > revealAt + Mathf.Max(LiftSeconds, LeafSeconds);
        public static bool Unveils(ItemRarity rarity) => rarity == ItemRarity.Mythic || rarity == ItemRarity.Rainbow;
        public static Sprite ClothSprite(bool prismatic) => ArsenalArtwork.Frame(prismatic ? "RareCover_Prismatic" : "RareCover_Mythic", 0)
            ?? MechanicInk.Drawn(InkToken.Patch, prismatic ? ArsenalInk.Candy : ArsenalInk.Wine);

        /// <summary>landing: seconds until the stand lands (MarketMotion); the cloth lifts HoldAfterLanding later.</summary>
        public void Configure(ArenaController controller, SpriteRenderer merchandise, ItemRarity rarity, float landing)
        {
            owner = controller; target = merchandise; prismatic = rarity == ItemRarity.Rainbow;
            revealAt = landing + HoldAfterLanding;
            cloth = Child("RareCoverCloth", ClothSprite(prismatic));
            for (int index = 0; index < leaves.Length; index++)
                leaves[index] = Child("RareCoverLeaf" + index, MechanicInk.Drawn(index % 3 == 2 ? InkToken.Glint : InkToken.Leaf, prismatic ? Prismatic(index) : ArsenalInk.Gold));
            CheckpointAudio.PlayInterfaceId("shop.ticket.sway", revealAt - 0.3f);
        }
        private static InkPalette Prismatic(int index)
        {
            InkPalette[] party = { ArsenalInk.Gold, ArsenalInk.Candy, ArsenalInk.Water, ArsenalInk.Green, ArsenalInk.Violet, ArsenalInk.Cyan };
            return party[index % party.Length];
        }
        private SpriteRenderer Child(string name, Sprite sprite)
        {
            var child = new GameObject(name); child.transform.SetParent(transform, false);
            var view = child.AddComponent<SpriteRenderer>(); view.sprite = sprite; view.enabled = false; return view;
        }

        private void LateUpdate()
        {
            if (target == null) { Destroy(gameObject); return; }
            bool paused = owner != null && owner.Simulation != null && owner.Simulation.Phase == RunPhase.Paused;
            if (!paused) age += Time.unscaledDeltaTime;
            if (age >= revealAt && !cued) { cued = true; CheckpointAudio.PlayInterfaceId("up.rarity.gold"); if (prismatic) CheckpointAudio.PlayInterfaceId("up.rarity.purple", 0.12f); }
            Bounds bounds = target.bounds;
            float width = Mathf.Max(0.5f, Mathf.Max(bounds.size.x, bounds.size.y * 0.8f)) * 1.35f;
            int order = target.sortingOrder;
            bool reduced = PresentationSettings.ReducedMotion;
            // the cloth: drapes the stand while it flies in and sits; one tremble; then lifts, tilts and thins away
            float lift = Mathf.Clamp01((age - revealAt) / LiftSeconds);
            cloth.enabled = target.enabled && lift < 1;
            if (cloth.enabled && cloth.sprite != null)
            {
                float tremble = reduced ? 0 : Mathf.Exp(-Mathf.Pow((age - (revealAt - 0.28f)) * 9, 2)) * Mathf.Sin(age * 70) * 4;
                float sway = reduced ? 0 : Mathf.Sin(age * 2.3f) * 1.5f;
                float rise = reduced ? 0 : lift * lift * 1.1f;
                cloth.transform.position = bounds.center + Vector3.up * (bounds.size.y * 0.05f + rise) + Vector3.right * (reduced ? 0 : lift * 0.25f);
                cloth.transform.rotation = Quaternion.Euler(0, 0, tremble + sway - (reduced ? 0 : lift * 18));
                float scale = width / Mathf.Max(0.01f, cloth.sprite.bounds.size.x);
                cloth.transform.localScale = new Vector3(scale * (1 - lift * 0.35f), scale * (1 + lift * 0.15f), 1);
                cloth.sortingOrder = order + 6;
                cloth.color = new Color(1, 1, 1, 1 - lift * lift);
            }
            // the leaf: falls from just above the piece, drifting side to side, never flung outward
            float leafTime = age - revealAt;
            for (int index = 0; index < leaves.Length; index++)
            {
                SpriteRenderer leaf = leaves[index];
                float start = index * 0.07f, t = (leafTime - start) / LeafSeconds;
                leaf.enabled = target.enabled && !reduced && t > 0 && t < 1;
                if (!leaf.enabled || leaf.sprite == null) continue;
                float column = (index / (float)(leaves.Length - 1) - 0.5f) * width * 0.9f;
                float drift = Mathf.Sin(t * 6.3f + index * 1.7f) * 0.12f;
                leaf.transform.position = new Vector3(bounds.center.x + column + drift, bounds.max.y + 0.35f - t * (bounds.size.y + 0.5f), 0);
                leaf.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 5 + index) * 40 + index * 37);
                leaf.transform.localScale = Vector3.one * (0.16f / Mathf.Max(0.01f, leaf.sprite.bounds.size.x)) * (index % 3 == 2 ? 0.8f : 1);
                leaf.sortingOrder = order + 7;
                leaf.color = new Color(1, 1, 1, Mathf.Clamp01((1 - t) * 3));
            }
            if (Finished) Destroy(gameObject);
        }
    }
}
