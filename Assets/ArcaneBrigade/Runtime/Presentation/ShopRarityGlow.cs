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
        private readonly LineRenderer[] rays = new LineRenderer[8];
        private Material rayMaterial;
        private SpriteRenderer target;
        private readonly SpriteRenderer[] motes = new SpriteRenderer[10];
        private float appearedAt = -1;
        public SpriteRenderer Target => target;
        public void Configure(ItemRarity rarity, ArenaController controller, float offset, SpriteRenderer merchandise)
        {
            target = merchandise;
            owner = controller; phase = offset; tint = RunModifierRules.RarityColor(rarity);
            strength = rarity == ItemRarity.Mythic ? 1.4f : rarity == ItemRarity.Rare ? 1.15f : 0.8f;
            rayMaterial = new Material(Shader.Find("Sprites/Default"));
            for (int index = 0; index < rays.Length; index++)
            {
                var child = new GameObject("RarityRay" + index); child.transform.SetParent(transform, false);
                LineRenderer ray = child.AddComponent<LineRenderer>(); rays[index] = ray;
                ray.sharedMaterial = rayMaterial; ray.useWorldSpace = false; ray.positionCount = 2;
                ray.numCapVertices = 4; ray.sortingOrder = 312; ray.startWidth = 0.055f; ray.endWidth = 0.005f;
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
            if (target == null) { Destroy(gameObject); return; }
            transform.position = target.bounds.center;
            float visibility = target.enabled && target.gameObject.activeInHierarchy ? target.color.a : 0;
            if (owner == null || owner.Simulation.Phase == RunPhase.Paused) return;
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            float time = owner.PresentationClock + phase;
            if (visibility <= 0) appearedAt = -1;
            else if (appearedAt < 0) appearedAt = owner.PresentationClock;
            float reveal = reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01((owner.PresentationClock - appearedAt) / 0.35f));
            float pulse = reduced ? 1 : 0.88f + Mathf.Sin(time * 2.6f) * 0.12f;
            float size = Mathf.Clamp(Mathf.Max(target.bounds.size.x, target.bounds.size.y), 0.3f, 1.6f);
            transform.localScale = Vector3.one * size * Mathf.Lerp(0.65f, 1, reveal);
            for (int index = 0; index < lights.Length; index++)
            {
                lights[index].sortingOrder = target.sortingOrder - 4 + index;
                lights[index].color = new Color(tint.r, tint.g, tint.b, (index == 0 ? 0.85f : 0.5f) * pulse * visibility * reveal);
                lights[index].transform.localScale = (index == 0 ? new Vector3(2.1f, 1.5f, 1) * pulse : new Vector3(0.42f, 2.2f, 1)) * strength;
                lights[index].transform.localPosition = index == 0 ? Vector3.zero : new Vector3(index == 1 ? -0.2f : 0.2f, 0.38f, 0);
                lights[index].transform.localRotation = Quaternion.Euler(0, 0, index == 0 ? 0 : (index == 1 ? -1 : 1) * (16 + (reduced ? 0 : Mathf.Sin(time * 1.4f) * 8)));
            }
            for (int index = 0; index < rays.Length; index++)
            {
                float angle = index * Mathf.PI / 4 + (reduced ? 0 : time * 0.16f);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                rays[index].SetPosition(0, direction * 0.28f);
                float flare = reduced ? 1 : 0.8f + Mathf.Sin(time * 3.2f + index * 0.9f) * 0.2f;
                rays[index].SetPosition(1, direction * (0.62f + strength * 0.26f) * pulse * flare);
                rays[index].sortingOrder = target.sortingOrder - 1;
                rays[index].startColor = new Color(tint.r, tint.g, tint.b, 0.72f * visibility * reveal * flare);
                rays[index].endColor = new Color(tint.r, tint.g, tint.b, 0.12f * visibility * reveal);
            }
            for (int index = 0; index < motes.Length; index++)
            {
                SpriteRenderer mote = motes[index]; mote.enabled = !reduced;
                float life = Mathf.Repeat(time * 0.38f + index * 0.137f, 1);
                float angle = time * 0.45f + index * 2.4f;
                mote.transform.localPosition = new Vector3(Mathf.Sin(angle) * (0.28f + life * 0.4f), -0.35f + life * 1.25f, 0);
                mote.transform.localScale = Vector3.one * (0.07f + Mathf.Sin(life * Mathf.PI) * 0.045f);
                mote.sortingOrder = target.sortingOrder - 1;
                mote.color = new Color(tint.r, tint.g, tint.b, Mathf.Sin(life * Mathf.PI) * visibility * reveal * 0.8f);
            }
        }
        private void OnDestroy() { if (rayMaterial != null) Destroy(rayMaterial); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { if (radial != null) Object.Destroy(radial); if (texture != null) Object.Destroy(texture); radial = null; texture = null; }
    }
}
