using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The material layer of the hero skills (owner 24/10/2026, Docs/Hero_Effect_Suggestions.md): water and gel that
    /// bend what is behind them, the push of a blow, mirror panes that reflect the room, and steam with a swirling body.
    /// Pieces are pooled sprites drawn with the HeroRefract / HeroMirror / HeroSteam shaders. All refraction shares one
    /// screen grab a frame; with Reduced Effects, a weak device or a crowd the layer is left out and the painted ink stands
    /// alone.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private const int WarpOrder = AirOrder - 2; // over the bodies and the floor, under the air ink and tokens
        private readonly List<SpriteRenderer> warps = new List<SpriteRenderer>();
        private int warpUsed;
        private MaterialPropertyBlock warpBlock;
        private static Material refractMaterial, mirrorMaterial, steamMaterial;
        private static Sprite blobMask, wedgeMask, crestMask;
        private int mirrorStored; private float mirrorRippleAt = -10;

        private bool WarpReady => !PresentationSettings.ReducedEffects && !sparse && SystemInfo.graphicsShaderLevel >= 30 && EffectDensity.CrowdPressure < .7f;

        private static Material Load(ref Material slot, string shader)
        {
            if (slot == null && Resources.Load<Shader>(shader) is Shader s && s.isSupported) slot = new Material(s) { name = shader, hideFlags = HideFlags.DontSave };
            return slot;
        }
        private static Sprite Mask(ref Sprite slot, string name, int w, int h, System.Func<float, float, float> alpha)
        {
            if (slot != null) return slot;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = name, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                px[y * w + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha((x + .5f) / w * 2 - 1, (y + .5f) / h * 2 - 1)) * 255));
            tex.SetPixels32(px); tex.Apply(false, true);
            return slot = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), Mathf.Max(w, h), 0, SpriteMeshType.FullRect);
        }
        private static Sprite Blob => Mask(ref blobMask, "Hero warp blob", 64, 64, (u, v) => Mathf.Exp(-(u * u + v * v) * 2.6f) - .07f);
        /// <summary>A soft wedge pointing along +x: narrow at its root, wide and fading at its tip.</summary>
        private static Sprite Wedge => Mask(ref wedgeMask, "Hero warp wedge", 64, 48, (u, v) => { float k = (u + 1) * .5f; float half = Mathf.Lerp(.15f, 1, k); return Mathf.Exp(-(v / half) * (v / half) * 2.2f) * Mathf.Sin(k * Mathf.PI) * 1.2f; });
        /// <summary>A wave's crest: a band thick along its top, thinning down its face.</summary>
        private static Sprite Crest => Mask(ref crestMask, "Hero warp crest", 96, 48, (u, v) => { float top = Mathf.Exp(-(v - .35f) * (v - .35f) * 9); return Mathf.Max(top, (1 - Mathf.Abs(v + .2f)) * .55f) * (1 - u * u); });

        private SpriteRenderer NextWarp()
        {
            if (warpUsed == warps.Count)
            {
                var go = new GameObject("Hero skill material " + warpUsed); go.transform.SetParent(transform, false);
                warps.Add(go.AddComponent<SpriteRenderer>());
            }
            var r = warps[warpUsed++]; r.enabled = true; return r;
        }
        private void BeginWarps() { warpUsed = 0; }
        private void EndWarps() { for (int i = warpUsed; i < warps.Count; i++) warps[i].enabled = false; }

        /// <summary>A refracting piece: mask stretched to size, turned by angle; tint.a is how strongly it shows.</summary>
        private void Refract(Sprite mask, Vector2 at, Vector2 size, float angle, Color tint, float bend, float flowX = 0, float flowY = 0)
        {
            if (!WarpReady || tint.a <= .01f || Load(ref refractMaterial, "HeroRefract") == null) return;
            SpriteRenderer r = NextWarp(); r.sharedMaterial = refractMaterial; r.sprite = mask; r.color = A(tint, tint.a * strength); r.sortingOrder = WarpOrder;
            r.flipX = r.flipY = false;
            Vector2 b = mask.bounds.size; r.transform.position = at; r.transform.rotation = Quaternion.Euler(0, 0, angle);
            r.transform.localScale = new Vector3(size.x / Mathf.Max(.001f, b.x), size.y / Mathf.Max(.001f, b.y), 1);
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear();
            warpBlock.SetFloat("_Strength", still ? bend * .3f : bend); warpBlock.SetVector("_Flow", new Vector4(flowX, flowY, 0, 0));
            r.SetPropertyBlock(warpBlock);
        }
        /// <summary>A material layer laid exactly over a sprite already drawn (a jelly, a mirror pane).</summary>
        private SpriteRenderer Overlay(SpriteRenderer source, Material material, Color tint)
        {
            if (!WarpReady || source == null || !source.enabled || source.sprite == null || material == null) return null;
            SpriteRenderer r = NextWarp(); r.sharedMaterial = material; r.sprite = source.sprite; r.color = A(tint, tint.a * strength);
            r.flipX = source.flipX; r.flipY = source.flipY; r.sortingOrder = source.sortingOrder + 1;
            r.transform.position = source.transform.position; r.transform.rotation = source.transform.rotation; r.transform.localScale = source.transform.lossyScale;
            return r;
        }
        /// <summary>A puff of steam with body (HeroSteam): falls back to the painted puff token when the layer is off.</summary>
        private void SteamPuff(Vector2 at, float size, float alpha)
        {
            if (!WarpReady || Load(ref steamMaterial, "HeroSteam") == null) { SigToken(InkToken.Puff, at, size, 0, alpha, true, 0, ArsenalInk.Cotton); return; }
            SpriteRenderer r = NextWarp(); r.sharedMaterial = steamMaterial; r.sprite = Blob; r.color = new Color(.97f, .95f, .9f, alpha * strength); r.sortingOrder = WarpOrder + 1;
            r.flipX = r.flipY = false;
            float s = size * sigScale * 1.6f / Mathf.Max(.001f, Blob.bounds.size.x);
            r.transform.position = at; r.transform.rotation = Quaternion.identity; r.transform.localScale = new Vector3(s, s, 1);
            r.SetPropertyBlock(null);
        }

        // ------------------------------------------------------------------ where the layer is used
        /// <summary>Water pushed ahead of a wave or a diving bell, bending the floor and anyone in it.</summary>
        private void WaterPush(Vector2 at, Vector2 facing, float length, float width, float alpha)
        {
            float a = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            Refract(Wedge, at + facing * length * .45f, new Vector2(length, width), a, new Color(.72f, .92f, 1f, alpha), .028f, facing.x * .8f, facing.y * .8f);
        }
        /// <summary>A wave's body under its crest: clear water bending what it passes over.</summary>
        private void WaveBody(Vector2 at, Vector2 facing, float alpha)
        {
            float a = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            Refract(Crest, at, new Vector2(2.6f, 1.25f), a - 90, new Color(.7f, .9f, 1f, alpha), .03f, facing.x, facing.y);
        }
        /// <summary>The push of a heavy blow or a finale: the air (or water) bulges out from where it lands.</summary>
        private void Shock(Vector2 at, float p, float size = 1.4f)
        {
            if (p >= 1) return;
            float e = still ? .6f : OutCubic(p);
            Refract(Blob, at, Vector2.one * size * (.5f + .7f * e), 0, new Color(1, .97f, .9f, (1 - p) * .9f), .035f * (1 - p));
        }
        /// <summary>A gel body: what is behind and inside the jelly wobbles as through jelly.</summary>
        private void Gel(SpriteRenderer jelly)
        {
            if (Load(ref refractMaterial, "HeroRefract") == null) return;
            SpriteRenderer r = Overlay(jelly, refractMaterial, new Color(.75f, 1f, .92f, .6f));
            if (r == null) return;
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear(); warpBlock.SetFloat("_Strength", still ? .004f : .012f); warpBlock.SetFloat("_Scale", 7);
            r.SetPropertyBlock(warpBlock);
        }
        /// <summary>A mirror pane reflecting the room round it, rippling when it has just caught a shot.</summary>
        private void MirrorPane(SpriteRenderer pane)
        {
            if (Load(ref mirrorMaterial, "HeroMirror") == null) return;
            SpriteRenderer r = Overlay(pane, mirrorMaterial, new Color(.85f, .92f, 1f, .9f));
            if (r == null) return;
            Camera cam = Camera.main; Vector3 c = cam != null ? cam.WorldToViewportPoint(pane.transform.position) : new Vector3(.5f, .5f, 0);
            int stored = sim.HeroIdentityStored; if (stored > mirrorStored) mirrorRippleAt = t; mirrorStored = stored;
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear();
            warpBlock.SetVector("_Center", new Vector4(c.x, c.y, 0, 0)); warpBlock.SetFloat("_Ripple", still ? 0 : Mathf.Clamp01(1 - (t - mirrorRippleAt) / .4f) + .15f);
            r.SetPropertyBlock(warpBlock);
        }
    }
}
