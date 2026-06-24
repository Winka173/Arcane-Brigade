using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Glasshouse's weather layers (requirement 25/10/2026: sun shafts, rain streaks and puddle glints drawn by a
    /// shader): two view-sized quads drawn with Resources/Shaders/MapGlasshouseWeather, one on the floor and one in the air.
    /// MapAtmosphere.AirGlasshouse shows them every frame it draws; they hide themselves the frame it stops (another map, the shop,
    /// Reduced Effects). Without shader support the atmosphere paints its own sprite fallback instead.</summary>
    public sealed class MapGlasshouseSky : MonoBehaviour
    {
        private static Shader shader; private static bool looked; private static Sprite white;
        private SpriteRenderer floor, air; private Material floorMaterial, airMaterial;
        private int shownFrame = -10;

        public static bool Supported
        {
            get
            {
                if (!looked) { looked = true; shader = Resources.Load<Shader>("Shaders/MapGlasshouseWeather"); }
                return shader != null && shader.isSupported && SystemInfo.graphicsShaderLevel >= 30;
            }
        }

        /// <summary>Draws both layers over the view this frame: sun and rain 0..1, the clock (0 holds the pattern still).</summary>
        public void Show(Vector2 centre, Vector2 size, float sun, float rain, float clock, bool calm, int floorOrder, int airOrder)
        {
            if (!Supported) return;
            Layer(ref floor, ref floorMaterial, 0, centre, size, sun, rain, clock, calm, floorOrder);
            Layer(ref air, ref airMaterial, 1, centre, size, sun, rain, clock, calm, airOrder);
            shownFrame = Time.frameCount;
        }

        private void Layer(ref SpriteRenderer r, ref Material m, int layer, Vector2 centre, Vector2 size, float sun, float rain, float clock, bool calm, int order)
        {
            if (white == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "Glasshouse weather quad", hideFlags = HideFlags.DontSave };
                var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(px); tex.Apply(false, true);
                white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 4, 0, SpriteMeshType.FullRect);
            }
            if (r == null)
            {
                var go = new GameObject(layer == 0 ? "Glasshouse weather floor" : "Glasshouse weather air"); go.transform.SetParent(transform, false);
                r = go.AddComponent<SpriteRenderer>(); r.sprite = white;
                m = new Material(shader) { name = go.name, hideFlags = HideFlags.DontSave };
                m.SetFloat("_Layer", layer); r.sharedMaterial = m;
            }
            r.enabled = true; r.sortingOrder = order; r.color = Color.white;
            r.transform.position = new Vector3(centre.x, centre.y, 0); r.transform.rotation = Quaternion.identity;
            r.transform.localScale = new Vector3(size.x, size.y, 1); // the quad's sprite is one unit square
            m.SetFloat("_Sun", sun); m.SetFloat("_Rain", rain); m.SetFloat("_Clock", clock); m.SetFloat("_Calm", calm ? 1 : 0);
        }

        private void LateUpdate()
        {
            if (Time.frameCount - shownFrame <= 1) return;
            if (floor != null) floor.enabled = false;
            if (air != null) air.enabled = false;
        }

        private void OnDestroy()
        {
            if (floor != null) Destroy(floor.gameObject);
            if (air != null) Destroy(air.gameObject);
            if (floorMaterial != null) Destroy(floorMaterial);
            if (airMaterial != null) Destroy(airMaterial);
        }
    }
}
