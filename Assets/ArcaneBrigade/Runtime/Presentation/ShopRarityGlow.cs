using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(500)]
    public sealed class ShopRarityGlow : MonoBehaviour
    {
        private static Sprite radial;
        private static Texture2D texture;
        private readonly SpriteRenderer[] lights = new SpriteRenderer[3];
        private Color tint;
        private ArenaController owner;
        private float phase;
        private float strength;
        private readonly SpriteRenderer[] painted = new SpriteRenderer[2];
        private ShopRarityArtwork artwork;
        private ItemRarity rarity;
        private ShopDisplayActor inspected;
        public bool StockPresentation { get; private set; }
        private SpriteRenderer target;
        private readonly SpriteRenderer[] motes = new SpriteRenderer[10];
        private float appearedAt = -1;
        private bool prismatic;
        public SpriteRenderer Target => target;
        public bool UsesPaintedAura => StockPresentation && artwork != null && artwork.PerRarity >= 4;
        /// <summary>One bloom cycle lasts the same whatever the painted frame count, so more frames only make it smoother.</summary>
        public const float CycleSeconds = 1.65f;
        public static bool CanDecorate(SpriteRenderer merchandise) => merchandise != null && merchandise.GetComponentInParent<UnitVisual>() == null;
        public static float AuraWidth(float bodySize, float focus, ItemRarity rarity) => Mathf.Clamp(bodySize, .45f, 1.65f) * (rarity >= ItemRarity.Mythic ? 2.8f : rarity == ItemRarity.Rare ? 2.55f : 2.3f) * (1 + Mathf.Clamp01(focus) * .18f);
        public void Configure(ItemRarity rarity, ArenaController controller, float offset, SpriteRenderer merchandise, bool stockPresentation = true)
        {
            target = merchandise;
            if (!CanDecorate(target)) { if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject); return; }
            this.rarity = rarity; StockPresentation = stockPresentation; artwork = ShopRarityArtwork.Current; inspected = target.GetComponent<ShopDisplayActor>();
            prismatic = rarity == ItemRarity.Rainbow;
            owner = controller; phase = offset; tint = RunModifierRules.RarityColor(rarity);
            strength = rarity >= ItemRarity.Crimson ? 1.55f : rarity == ItemRarity.Mythic ? 1.4f : rarity == ItemRarity.Rare ? 1.15f : 0.8f;
            for (int index = 0; index < painted.Length; index++)
            {
                var child = new GameObject("PaintedRarityBloom" + index); child.transform.SetParent(transform, false);
                painted[index] = child.AddComponent<SpriteRenderer>();
            }
            if (radial == null)
            {
                texture = new Texture2D(96, 96, TextureFormat.RGBA32, false) { name = "NightMarketSoftLight", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[96 * 96];
                for (int vertical = 0; vertical < 96; vertical++)
                    for (int horizontal = 0; horizontal < 96; horizontal++)
                    {
                        float distance = Vector2.Distance(new Vector2(horizontal, vertical), new Vector2(47.5f, 47.5f)) / 47.5f;
                        pixels[vertical * 96 + horizontal] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - distance), 2.2f));
                    }
                texture.SetPixels(pixels); texture.Apply(false, true);
                radial = Sprite.Create(texture, new Rect(0, 0, 96, 96), new Vector2(0.5f, 0.5f), 96);
            }
            for (int index = 0; index < lights.Length; index++)
            {
                var child = new GameObject("RarityLight" + index); child.transform.SetParent(transform, false);
                lights[index] = child.AddComponent<SpriteRenderer>(); lights[index].sprite = radial; lights[index].sortingOrder = 309 + index;
            }
            for (int index = 0; index < motes.Length; index++)
            {
                var child = new GameObject("RarityMote" + index); child.transform.SetParent(transform, false);
                motes[index] = child.AddComponent<SpriteRenderer>(); motes[index].sprite = radial;
            }
        }
        private void LateUpdate()
        {
            if (target == null || !CanDecorate(target)) { Destroy(gameObject); return; }
            transform.position = target.bounds.center;
            float visibility = target.enabled && target.gameObject.activeInHierarchy ? target.color.a : 0;
            foreach (SpriteRenderer view in painted) if (view != null) view.enabled = visibility > 0 && UsesPaintedAura;
            foreach (SpriteRenderer view in lights) if (view != null) view.enabled = visibility > 0;
            if (visibility <= 0)
            {
                foreach (SpriteRenderer view in painted) if (view != null) { Color color = view.color; color.a = 0; view.color = color; }
                foreach (SpriteRenderer view in lights) if (view != null) { Color color = view.color; color.a = 0; view.color = color; }
                foreach (SpriteRenderer view in motes) if (view != null) { view.enabled = false; Color color = view.color; color.a = 0; view.color = color; }
                appearedAt = -1; return;
            }
            if (owner == null || owner.Simulation.Phase == RunPhase.Paused) return;
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            bool soft = owner.Profile != null && (owner.Profile.ReducedEffects || owner.Profile.ReducedFlashes);
            float time = owner.PresentationClock + phase;
            if (prismatic) tint = Color.Lerp(new Color(0.6f, 1, 0.9f), Color.HSVToRGB(Mathf.Repeat(time * 0.045f, 1), 0.45f, 1), 0.65f);
            if (visibility <= 0) appearedAt = -1;
            else if (appearedAt < 0) appearedAt = owner.PresentationClock;
            float reveal = reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01((owner.PresentationClock - appearedAt) / 0.35f));
            float pulse = reduced || soft ? 1 : 0.94f + Mathf.Sin(time * 1.7f) * 0.06f;
            float size = Mathf.Clamp(Mathf.Max(target.bounds.size.x, target.bounds.size.y), 0.3f, 1.6f);
            Vector3 inherited = transform.parent == null ? Vector3.one : transform.parent.lossyScale;
            transform.localScale = new Vector3(size / Mathf.Max(.001f, Mathf.Abs(inherited.x)), size / Mathf.Max(.001f, Mathf.Abs(inherited.y)), 1) * Mathf.Lerp(.8f, 1, reveal);
            transform.rotation = Quaternion.identity;
            float focus = inspected == null ? 0 : inspected.FocusAmount;
            // review 31/10/2026 (the bloom stepped: four painted frames swapped in turn): no frame swapping at all - two different
            // paintings of the bloom stay on, each swaying, breathing and licking upward on its own slow rhythm while their
            // strength trades back and forth, so the flames move continuously at the screen's own frame rate
            int frames = artwork != null && artwork.PerRarity > 0 ? artwork.PerRarity : 4;
            float trade = reduced ? .5f : .5f + .5f * Mathf.Sin(time * 1.25f + phase);
            for (int index = 0; index < painted.Length; index++)
            {
                SpriteRenderer view = painted[index]; if (!view.enabled) continue;
                view.sprite = artwork.Frame(rarity, index == 0 ? 0 : frames / 2);
                float width = AuraWidth(size, focus, rarity) / Mathf.Max(.01f, size);
                float paintedScale = width / Mathf.Max(.01f, view.sprite.bounds.size.x);
                float beat = reduced ? 0 : Mathf.Sin(time * (2.3f + index * .6f) + phase + index * 1.7f);
                float lick = reduced ? 0 : Mathf.Sin(time * (3.7f + index * .9f) + index * 2.3f) * .5f + Mathf.Sin(time * 6.1f + index) * .25f;
                view.transform.localScale = new Vector3(paintedScale * (1 + beat * .025f - lick * .02f), paintedScale * (1 + beat * .03f + lick * .07f), 1);
                view.transform.localPosition = new Vector3(reduced ? 0 : Mathf.Sin(time * 1.1f + index * 2) * .02f, -.08f + lick * .035f, 0);
                view.transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Sin(time * (.8f + index * .35f) + phase + index) * (2.6f + index));
                view.sortingOrder = target.sortingOrder - 2 + index;
                Color color = rarity == ItemRarity.Crimson ? new Color(1, .2f, .2f) : Color.white;
                float share = index == 0 ? .45f + .4f * trade : .45f + .4f * (1 - trade);
                color.a = share * visibility * reveal * (soft ? .5f : .8f) * (1 + focus * .08f);
                view.color = color;
            }
            for (int index = 0; index < lights.Length; index++)
            {
                lights[index].sortingOrder = target.sortingOrder - 4 + index;
                lights[index].color = new Color(tint.r, tint.g, tint.b, (index == 0 ? StockPresentation ? .37f : .25f : StockPresentation ? .13f : .08f) * (soft ? .65f : 1) * pulse * visibility * reveal * (1 + focus * .25f));
                lights[index].transform.localScale = (index == 0 ? new Vector3(StockPresentation ? 2.7f : 2.1f, StockPresentation ? 1.8f : 1.5f, 1) * pulse : Vector3.one * (StockPresentation ? .9f : .65f)) * strength;
                lights[index].transform.localPosition = index == 0 ? Vector3.zero : new Vector3(index == 1 ? -0.2f : 0.2f, 0.38f, 0);
                lights[index].transform.localRotation = Quaternion.Euler(0, 0, index == 0 ? 0 : (index == 1 ? -1 : 1) * (16 + (reduced ? 0 : Mathf.Sin(time * 1.4f) * 8)));
            }
            for (int index = 0; index < motes.Length; index++)
            {
                SpriteRenderer mote = motes[index]; mote.enabled = !reduced && (!soft || index < 3);
                float life = Mathf.Repeat(time * 0.38f + index * 0.137f, 1);
                float angle = time * 0.45f + index * 2.4f;
                mote.transform.localPosition = new Vector3(Mathf.Sin(angle) * (0.28f + life * 0.4f), -0.35f + life * 1.25f, 0);
                mote.transform.localScale = Vector3.one * (0.07f + Mathf.Sin(life * Mathf.PI) * 0.045f);
                mote.sortingOrder = target.sortingOrder - 1;
                mote.color = new Color(tint.r, tint.g, tint.b, Mathf.Sin(life * Mathf.PI) * visibility * reveal * 0.8f);
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { if (radial != null) Object.Destroy(radial); if (texture != null) Object.Destroy(texture); radial = null; texture = null; }
    }
}
