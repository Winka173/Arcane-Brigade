using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class K8ArenaScenery : MonoBehaviour
    {
        private ArenaController owner;
        private Material floor;
        private readonly List<SpriteRenderer> props = new List<SpriteRenderer>();
        private int theme = -1;
        private Color palette = Palettes[0];
        private static Sprite paper;
        private static Texture2D paperTexture;
        private static readonly Color[] Palettes = { new Color(0.11f, 0.23f, 0.3f), new Color(0.19f, 0.19f, 0.28f), new Color(0.13f, 0.27f, 0.26f) };

        public void Configure(ArenaController controller)
        {
            owner = controller;
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            if (art == null || art.EnvironmentProps == null || art.EnvironmentProps.Length < 4) return;
            Vector2 bounds = owner.WorldBounds * 0.5f;
            for (int index = 0; index < 12; index++)
            {
                bool horizontal = index < 8;
                float along = horizontal ? -0.75f + index % 4 * 0.5f : -0.5f + index % 2;
                Vector2 position = horizontal ? new Vector2(bounds.x * along, (index < 4 ? 1 : -1) * (bounds.y - 0.85f)) : new Vector2((index < 10 ? -1 : 1) * (bounds.x - 0.8f), bounds.y * along);
                var child = new GameObject("DreamLandmark_" + index); child.transform.SetParent(transform, false); child.transform.localPosition = position;
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = art.EnvironmentProps[index % 4];
                float height = index % 4 == 0 ? 2.15f : 1.5f;
                child.transform.localScale = Vector3.one * (height / renderer.sprite.bounds.size.y);
                renderer.sortingOrder = 1000 - Mathf.RoundToInt(position.y * 12); props.Add(renderer);
                child.AddComponent<StaticGroundShadow>();
                ShopFloorShadow.Create(transform, position + Vector2.down * height * 0.44f, height * 0.7f, 0.38f);
            }
            for (int index = 0; index < 18; index++)
            {
                float angle = index * 2.399963f;
                float radius = 6.8f + index % 4 * 1.25f;
                Vector2 position = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * 0.68f);
                position.x = Mathf.Clamp(position.x, -bounds.x + 1.2f, bounds.x - 1.2f);
                position.y = Mathf.Clamp(position.y, -bounds.y + 1.2f, bounds.y - 1.2f);
                var child = new GameObject("DreamGarden_" + index); child.transform.SetParent(transform, false); child.transform.localPosition = position;
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = art.DreamGardenProps != null && art.DreamGardenProps.Length == 4 ? art.DreamGardenProps[index % 4] : art.EnvironmentProps[(index + 1) % 4];
                float height = index % 3 == 0 ? 1.25f : 0.82f;
                child.transform.localScale = Vector3.one * (height / renderer.sprite.bounds.size.y);
                renderer.sortingOrder = 1000 - Mathf.RoundToInt(position.y * 12); props.Add(renderer);
                child.AddComponent<StaticGroundShadow>();
                child.AddComponent<LivingScenery>();
            }
            GameObject ground = GameObject.Find("HD_TiledFloor");
            if (ground != null) floor = ground.GetComponent<MeshRenderer>().material;
        }

        private void Update()
        {
            if (owner == null || owner.Session == null || owner.Simulation == null) return;
            int next = (owner.Session.State.Wave - 1) / 4 % Palettes.Length;
            if (theme != next)
            {
                theme = next;
            }
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            Color target = Color.Lerp(Palettes[theme], new Color(0.28f, 0.17f, 0.37f), owner.Shop != null && owner.Shop.Special ? 0.55f : 0);
            palette = Color.Lerp(palette, target, reduced ? 1 : 1 - Mathf.Exp(-Time.deltaTime * 0.8f));
            if (floor != null) { floor.SetColor("_BaseColor", palette); floor.SetFloat("_Detail", 0.28f); }
            foreach (SpriteRenderer prop in props) prop.color = Color.Lerp(new Color(0.58f, 0.62f, 0.65f), palette, 0.42f);
        }

        public static void CreateMarketFloor(Transform parent, bool special)
        {
            Color thread = special ? new Color(0.71f, 0.46f, 0.8f, 0.65f) : new Color(0.91f, 0.8f, 0.51f, 0.65f);
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            if (art != null && art.MarketDecor != null && art.MarketDecor.Length >= 4)
            {
                var rug = new GameObject("PaintedMarketRug"); rug.transform.SetParent(parent, false); rug.transform.localPosition = new Vector2(0, 0.72f);
                SpriteRenderer renderer = rug.AddComponent<SpriteRenderer>(); renderer.sprite = art.MarketRugs != null && art.MarketRugs.Length == 2 ? art.MarketRugs[special ? 1 : 0] : art.MarketDecor[special ? 1 : 0]; renderer.sortingOrder = -77;
                rug.transform.localScale = Vector3.one * (10.5f / renderer.sprite.bounds.size.x);
                var garland = new GameObject("MarketPennants"); garland.transform.SetParent(parent, false); garland.transform.localPosition = new Vector2(0, 4.65f);
                SpriteRenderer flags = garland.AddComponent<SpriteRenderer>(); flags.sprite = art.MarketDecor[special ? 3 : 2]; flags.sortingOrder = 300;
                garland.transform.localScale = Vector3.one * (3.7f / flags.sprite.bounds.size.x);
                garland.AddComponent<LivingScenery>();
            }
            for (int step = 0; step < 3; step++)
            {
                float height = -3.3f - step * 0.24f;
                SpriteRenderer arrow = Panel(parent, new Vector2(0, height), new Vector2(0.32f, 0.055f), thread, -75);
                arrow.transform.localRotation = Quaternion.Euler(0, 0, -35);
                SpriteRenderer other = Panel(parent, new Vector2(0.26f, height), new Vector2(0.32f, 0.055f), thread, -75);
                other.transform.localRotation = Quaternion.Euler(0, 0, 35);
            }
        }

        private static SpriteRenderer Panel(Transform parent, Vector2 position, Vector2 size, Color color, int order)
        {
            if (paper == null)
            {
                paperTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
                paperTexture.SetPixel(0, 0, Color.white); paperTexture.Apply(false, true);
                paper = Sprite.Create(paperTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
            }
            var child = new GameObject("MarketCloth"); child.transform.SetParent(parent, false); child.transform.localPosition = position; child.transform.localScale = size;
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = paper; renderer.color = color; renderer.sortingOrder = order; return renderer;
        }
        private void OnDestroy() { if (floor != null) Destroy(floor); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { if (paper != null) Object.Destroy(paper); if (paperTexture != null) Object.Destroy(paperTexture); paper = null; paperTexture = null; }
    }
}
