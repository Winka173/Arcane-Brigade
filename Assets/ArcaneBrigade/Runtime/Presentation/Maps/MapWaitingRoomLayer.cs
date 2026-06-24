using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Waiting Room's and the Post Office's own layer over the map (requirement 25/10/2026): an ink canvas in front
    /// of the bodies (health bars, a stamp's burning ticks, the clock's glow ticks), an ink canvas sorted just over one standing
    /// prop (a door's cracks, a parcel's splits) and soft shader quads (the ceiling dust's light shafts, the paper wind). A frame
    /// that does not draw it hides it, so nothing is left behind when the map or the phase changes.</summary>
    public sealed class MapWaitingRoomLayer : MonoBehaviour
    {
        public const int FrontOrder = 2162, WindOrder = 2124, ShaftOrder = 2128;
        private InkCanvas front, prop;
        private readonly List<SpriteRenderer> quads = new List<SpriteRenderer>();
        private int quadsUsed, drawnFrame = -1;
        private bool hidden = true;
        private MaterialPropertyBlock block;
        private static Sprite unit;
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        public InkCanvas Front => front;
        public InkCanvas Prop => prop;

        public static MapWaitingRoomLayer Under(Transform parent)
        {
            var go = new GameObject("Map paper layer"); go.transform.SetParent(parent, false);
            return go.AddComponent<MapWaitingRoomLayer>();
        }

        public void Begin()
        {
            front ??= new InkCanvas(transform, "Map paper front ink", FrontOrder);
            prop ??= new InkCanvas(transform, "Map paper prop ink", 1000);
            front.Clear(); prop.Clear(); quadsUsed = 0; drawnFrame = Time.frameCount; hidden = false;
        }
        /// <summary>Sorts the prop canvas just over a standing prop drawn at this spot (MapEnvironmentVisual.Put's order).</summary>
        public void PropOver(Vector2 at) => prop.SetOrder(1000 - Mathf.RoundToInt((at.y + .35f) * 12) + 2);
        public void End()
        {
            front.Commit(); prop.Commit();
            for (int i = quadsUsed; i < quads.Count; i++) quads[i].enabled = false;
        }
        private void LateUpdate()
        {
            if (hidden || drawnFrame == Time.frameCount) return;
            front?.Hide(); prop?.Hide();
            foreach (SpriteRenderer q in quads) q.enabled = false;
            hidden = true;
        }

        /// <summary>A shader from Resources/Shaders, or null when it is missing or the device cannot run it (the caller then
        /// draws its ink version).</summary>
        public static Material Shader(string name)
        {
            if (materials.TryGetValue(name, out Material m)) return m;
            m = Resources.Load<Shader>("Shaders/" + name) is Shader s && s.isSupported ? new Material(s) { name = name, hideFlags = HideFlags.DontSave } : null;
            materials[name] = m;
            return m;
        }
        /// <summary>A quad of `size` world units at `at`, painted by `material` (uv 0..1 across it; the colour is its tint).
        /// The shaders read _Clock (seconds, frozen under Reduced Motion), _Seed and _Extra.</summary>
        public bool Quad(Material material, Vector2 at, Vector2 size, float angle, Color colour, int order, float clock, float seed, Vector4 extra = default)
        {
            if (material == null || colour.a <= .004f) return false;
            if (quadsUsed == quads.Count)
            {
                var go = new GameObject("Map paper quad"); go.transform.SetParent(transform, false);
                quads.Add(go.AddComponent<SpriteRenderer>());
            }
            SpriteRenderer r = quads[quadsUsed++];
            r.enabled = true; r.sharedMaterial = material; r.sprite = Unit; r.color = colour; r.sortingOrder = order;
            r.transform.position = new Vector3(at.x, at.y, 0); r.transform.rotation = Quaternion.Euler(0, 0, angle);
            r.transform.localScale = new Vector3(size.x, size.y, 1);
            block ??= new MaterialPropertyBlock(); block.Clear();
            block.SetFloat("_Clock", clock); block.SetFloat("_Seed", seed); block.SetVector("_Extra", extra);
            r.SetPropertyBlock(block);
            return true;
        }
        private static Sprite Unit
        {
            get
            {
                if (unit != null) return unit;
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "Map paper quad", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(px); tex.Apply(false, true);
                return unit = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 4, 0, SpriteMeshType.FullRect);
            }
        }

        // ------------------------------------------------------------------ ink pieces both maps use
        private static readonly Color BarInk = new Color(.09f, .06f, .09f), BarTrack = new Color(.2f, .15f, .17f), BarBrass = new Color(.86f, .66f, .32f);
        /// <summary>A prop's health bar in ink (requirement 25/10/2026: the door and the parcel show their health): a brass-capped
        /// dark track and the health left in the prop's colour, a pale flash on the part just lost.</summary>
        public void HealthBar(Vector2 centre, float width, float share, float lostFlash, Color body, float alpha)
        {
            if (alpha <= .01f) return;
            Vector2 a = centre - new Vector2(width * .5f, 0), b = centre + new Vector2(width * .5f, 0);
            front.Stroke(InkCanvas.Line(a - new Vector2(.05f, 0), b + new Vector2(.05f, 0)), new InkStroke { Width = .24f, Head = 1, Tail = 0, Taper = .1f, Alpha = .92f * alpha, Ink = BarInk, Body = BarTrack, Light = BarBrass, LightOffset = .32f });
            if (lostFlash > .01f) front.Stroke(InkCanvas.Line(a, b), new InkStroke { Width = .15f, Head = Mathf.Clamp01(share + .12f), Tail = 0, Taper = .05f, Alpha = lostFlash * alpha, Ink = BarInk, Body = new Color(1, .96f, .86f), NoLight = true });
            if (share > .005f)
                front.Stroke(InkCanvas.Line(a, b), new InkStroke { Width = .15f, Head = Mathf.Clamp01(share), Tail = 0, Taper = .05f, Alpha = alpha, Ink = BarInk, Body = body, Light = Color.Lerp(body, Color.white, .55f), LightOffset = .3f });
            // brass caps at both ends
            front.Sector(a - new Vector2(.08f, 0), .08f, 0, 360, new Color(BarBrass.r, BarBrass.g, BarBrass.b, alpha), new Color(BarBrass.r * .7f, BarBrass.g * .7f, BarBrass.b * .7f, alpha));
            front.Sector(b + new Vector2(.08f, 0), .08f, 0, 360, new Color(BarBrass.r, BarBrass.g, BarBrass.b, alpha), new Color(BarBrass.r * .7f, BarBrass.g * .7f, BarBrass.b * .7f, alpha));
        }
        /// <summary>Thin timing line under a health bar (how long until something happens), shrinking from the right.</summary>
        public void TimeLine(Vector2 centre, float width, float share, Color colour, float alpha)
        {
            if (share <= .005f || alpha <= .01f) return;
            Vector2 a = centre - new Vector2(width * .5f, 0), b = centre + new Vector2(width * .5f, 0);
            front.Stroke(InkCanvas.Line(a, b), new InkStroke { Width = .07f, Head = Mathf.Clamp01(share), Tail = 0, Taper = .2f, Alpha = alpha, Ink = BarInk, Body = colour, NoLight = true });
        }
        /// <summary>Cracks spreading over a prop's face (requirement 25/10/2026: cracks are ink, not art): stage 1..3 adds more,
        /// longer and branching cracks from a fixed seed so they never jump between frames.</summary>
        public void Cracks(Vector2 centre, Vector2 half, int stage, int seed, float alpha)
        {
            if (stage <= 0 || alpha <= .01f) return;
            var deep = new InkStroke { Width = .06f, Head = 1, Tail = 0, Taper = 1.2f, Alpha = alpha, Ink = new Color(.07f, .04f, .05f), InkOnly = true };
            var edge = new InkStroke { Width = .025f, Head = 1, Tail = 0, Taper = 1.2f, Alpha = .7f * alpha, Ink = new Color(1, .93f, .78f), InkOnly = true };
            int lines = stage == 1 ? 2 : stage == 2 ? 4 : 7;
            Vector2 heart = centre + new Vector2(H(seed, 1) - .5f, H(seed, 2) - .5f) * half * .5f;
            for (int i = 0; i < lines; i++)
            {
                float a = (H(seed, 10 + i) * 360 + i * 51) * Mathf.Deg2Rad;
                float reach = (.45f + .55f * H(seed, 20 + i)) * (stage == 3 ? 1 : stage == 2 ? .8f : .6f);
                Vector2 end = heart + new Vector2(Mathf.Cos(a) * half.x, Mathf.Sin(a) * half.y) * reach;
                prop.Stroke(InkCanvas.Line(heart, end, .05f, 3, true, seed + i), deep);
                prop.Stroke(InkCanvas.Line(heart + new Vector2(.03f, .03f), end + new Vector2(.03f, .03f), .05f, 3, true, seed + i), edge);
                if (stage >= 2 && i % 2 == 0)
                {
                    Vector2 mid = Vector2.Lerp(heart, end, .55f); float b = a + (H(seed, 30 + i) > .5f ? .7f : -.7f);
                    prop.Stroke(InkCanvas.Line(mid, mid + new Vector2(Mathf.Cos(b) * half.x, Mathf.Sin(b) * half.y) * reach * .45f, .03f, 3, true, seed + 7 * i), deep);
                }
            }
        }
        private static float H(int i, int k) => Mathf.Repeat(Mathf.Sin(i * 12.9898f + k * 78.233f) * 43758.5453f, 1);
    }
}
