using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The ceremonies past three stars (requirement 27/10/2026), played over the merge ceremony: the 4th star is purple (10 s:
    /// stars gather, a violet pillar, a flash, rings, orbiting stars, a sigil on the floor, falling petals and moons) and the 5th star is
    /// rainbow (16 s: the same with every colour, three flashes, rays, a rainbow arch, comets and fireworks). All code ink.</summary>
    public sealed class GrandStarCeremony : MonoBehaviour
    {
        public static readonly Color Purple = new Color(.72f, .5f, 1f);
        /// <summary>The 4th star's two purples, bright enough to read on the teal floor.</summary>
        public static readonly InkPalette Amethyst = new InkPalette(new Color(.16f, .06f, .32f), new Color(.68f, .42f, 1f), new Color(.93f, .85f, 1f), new Color(.78f, .55f, 1f), new Color(1f, .72f, .95f)),
            Orchid = new InkPalette(new Color(.2f, .05f, .3f), new Color(.92f, .5f, 1f), new Color(1f, .88f, 1f), new Color(.95f, .6f, 1f), new Color(.75f, .6f, 1f));
        private ArsenalInk ink;
        private Vector2 at;
        private float born, life;
        private int tier, said, heard;
        private readonly List<Vector2> path = new List<Vector2>();

        public static float Duration(int tier) => tier >= 5 ? 16f : 10f;

        /// <summary>A palette of one hue (0 to 1): the rainbow's colours.</summary>
        public static InkPalette Hue(float h)
        {
            h -= Mathf.Floor(h);
            return new InkPalette(Color.HSVToRGB(h, .7f, .22f), Color.HSVToRGB(h, .62f, 1f), Color.HSVToRGB(h, .18f, 1f), Color.HSVToRGB(h, .5f, 1f), Color.HSVToRGB((h + .08f) % 1f, .75f, 1f));
        }

        public static void Play(Vector2 at, int tier)
        {
            var go = new GameObject("GrandStarCeremony");
            GrandStarCeremony c = go.AddComponent<GrandStarCeremony>();
            c.at = at; c.tier = Mathf.Clamp(tier, 4, 5); c.born = Time.unscaledTime; c.life = Duration(c.tier);
        }

        private InkPalette Pal(float h, int i = 0) => tier >= 5 ? Hue(h) : (i % 3 == 2 ? Orchid : Amethyst);
        private Color Tone(float h, float s = .45f) => tier >= 5 ? Color.HSVToRGB(h - Mathf.Floor(h), s, 1f) : Color.Lerp(Purple, Color.white, s * .5f);
        private static float Rand(int i, int salt) { float v = Mathf.Sin(i * 12.9898f + salt * 78.233f) * 43758.5453f; return v - Mathf.Floor(v); }

        private void Word(float age, int slot, float when, string text, float h, float scale)
        {
            if (age < when || (said & (1 << slot)) != 0) return;
            said |= 1 << slot;
            ImpactWords.Pop(text, at + Vector2.up * 1.7f, Tone(h, .2f), 3, scale, -4f);
        }
        private void Sound(float age, float when, string id, int bit)
        {
            if (age < when || (heard & (1 << bit)) != 0) return;
            heard |= 1 << bit; CheckpointAudio.PlayInterfaceId(id);
        }

        private void Update()
        {
            if (ink == null) ink = new ArsenalInk(transform, 66, 2410);
            float age = Time.unscaledTime - born;
            ink.Begin();
            if (age >= life) { ink.End(); Destroy(gameObject); return; }
            bool still = PresentationSettings.ReducedMotion, calm = PresentationSettings.ReducedEffects, rainbow = tier >= 5;
            float fade = Mathf.Clamp01(age / .4f) * Mathf.Clamp01((life - age) / 1.8f), spin = still ? 0 : age;
            float hue0 = still ? 0 : age * .18f;
            Vector2 heart = at + Vector2.up * 1.5f;

            // 1. the gathering: stars and moons spiral in, the floor and the air darken and tint
            float gather = Mathf.Clamp01(age / 2.4f);
            ink.Vignette(at, 16f, new Color(.12f, .05f, .24f), 0f, .38f * fade);
            Color zone = Tone(hue0 + .2f);
            ink.FloorZone(at + Vector2.down * .1f, 2.6f * gather + .4f, 1.1f * gather + .2f, new Color(zone.r, zone.g, zone.b, .5f * fade), new Color(zone.r, zone.g, zone.b, 0));
            if (age < 2.6f)
                for (int i = 0; i < (calm ? 10 : 24); i++)
                {
                    float q = Mathf.Clamp01((age - i * .05f) / 1.9f); if (q <= 0) continue;
                    float ang = (i * 137.5f + (1 - q) * 420f) * Mathf.Deg2Rad, r = (1 - q * q) * (3.4f + Rand(i, 1) * 1.6f);
                    ink.TokenAt(i % 3 == 0 ? InkToken.Moon : i % 3 == 1 ? InkToken.Star : InkToken.Glint, Pal(hue0 + i * .09f, i), heart + new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r * .6f), .2f + .14f * q, spin * 90f, fade * (1 - q * .3f));
                }

            // 2. the pillar of light rises (1.2 s to 2.6 s) and the flash tears it open
            float rise = Mathf.Clamp01((age - 1.2f) / 1.4f), flash = age >= 2.6f ? Mathf.Clamp01(1 - (age - 2.6f) / .7f) : 0;
            if (rise > 0 && age < 3.6f)
                ink.Stroke(at, at + Vector2.up * 7f * rise, Pal(hue0), .5f + 1.1f * rise + flash * 1.4f, 1f, 0f, fade * (age < 2.6f ? .9f : Mathf.Clamp01(1 - (age - 2.6f))));
            ink.Light(heart, 2f + 5f * rise + flash * 8f, Tone(hue0, .6f), (.35f * rise + flash * .8f) * fade);
            Sound(age, 2.55f, "up.merge", 0); Sound(age, 2.6f, "up.star3", 1);
            Word(age, 0, 2.6f, rainbow ? "5 STARS!" : "4 STARS!", hue0, 1.5f);

            // 3. rings roll out across the floor, shards fly (from 2.6 s)
            for (int ring = 0; ring < (rainbow ? 7 : 3); ring++)
            {
                float q = (age - 2.6f - ring * (rainbow ? .16f : .28f)) / 1.8f; if (q <= 0 || q >= 1) continue;
                float r = 1f + 6.5f * (1 - (1 - q) * (1 - q));
                ink.FloorArc(at, r, r * .45f, 0, 360, Pal(hue0 + ring * .14f, ring), .2f * (1 - q * .5f), (1 - q) * fade);
            }
            if (age > 2.6f && age < 4.6f)
                for (int i = 0; i < (calm ? 12 : 36); i++)
                {
                    float q = (age - 2.6f) / 2f, ang = Rand(i, 2) * 6.283f, sp = 1.8f + Rand(i, 3) * 4.2f;
                    Vector2 p = heart + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .8f) * sp * q * 1.3f + Vector2.down * 2.4f * q * q;
                    ink.TokenAt(i % 4 == 0 ? InkToken.Star : InkToken.Shard, Pal(hue0 + i * .037f, i), p, .24f * (1 - q * .5f), spin * 300f + i * 37, (1 - q) * fade);
                }

            // 4. the long glory (2.6 s to the end): orbiting stars, a sigil, falling petals, bolts
            if (age > 2.6f)
            {
                float g = Mathf.Clamp01((age - 2.6f) / .8f) * fade;
                for (int i = 0; i < tier; i++)
                {
                    float a = (spin * 70f + i * 360f / tier) * Mathf.Deg2Rad;
                    Vector2 p = heart + new Vector2(Mathf.Cos(a) * 1.25f, Mathf.Sin(a) * .5f + .25f * Mathf.Sin(spin * 2 + i));
                    ink.TokenAt(InkToken.Star, Pal(hue0 + i * .2f, i), p, .5f + .08f * Mathf.Sin(spin * 4 + i), spin * 120f, g);
                    ink.Light(p, 1.1f, Tone(hue0 + i * .2f), .3f * g);
                }
                ink.TokenAt(InkToken.Star, Pal(hue0 + .5f), heart + Vector2.up * 1.3f, .9f + .06f * Mathf.Sin(spin * 3), 0, g);
                ink.Light(heart + Vector2.up * 1.3f, 2.4f, Tone(hue0 + .5f, .7f), .4f * g);
                for (int s = 0; s < 6; s++)
                {
                    float a = spin * 36f + s * 60f;
                    ink.FloorArc(at, 2.1f, .95f, a, a + 38f, Pal(hue0 + s * .16f, s), .16f, .85f * g);
                    ink.FloorArc(at, 3.1f, 1.4f, a + 7f, a + 31f, Pal(hue0 + .5f + s * .16f, s), .12f, .7f * g);
                }
                for (int s = 0; s < 8; s++)
                {
                    float a = (-spin * 25f + s * 45f) * Mathf.Deg2Rad;
                    ink.TokenAt(InkToken.Seal, Pal(hue0 + s * .125f, s), at + new Vector2(Mathf.Cos(a) * 2.6f, Mathf.Sin(a) * 1.15f), .3f, 0, .9f * g);
                }
                for (int i = 0; i < (calm ? 10 : 34); i++)
                {
                    float cycle = 3.6f + Rand(i, 4) * 2.4f, q = ((age - 2.6f) / cycle + Rand(i, 5)) % 1f;
                    Vector2 p = new Vector2(at.x + (Rand(i, 6) - .5f) * 9f + Mathf.Sin(q * 6 + i) * .5f, at.y + 5.5f - q * 6.8f);
                    ink.TokenAt(i % 3 == 0 ? InkToken.Moon : i % 3 == 1 ? InkToken.Petal : InkToken.Note, Pal(hue0 + i * .061f, i), p, .24f, q * 200f + i * 30, g * Mathf.Sin(q * Mathf.PI));
                }
                for (int i = 0; i < 3; i++)
                {
                    float step = (age - 2.6f) * .9f + i * .33f, q = step % 1f; if (q > .22f) continue;
                    ink.TokenAt(InkToken.Bolt, Pal(hue0 + i * .3f, i), heart + new Vector2((Rand(i + (int)step * 3, 7) - .5f) * 6f, Rand(i + (int)step, 8) * 2f - .5f), .6f, Rand(i, 9) * 40f - 20f, g * (1 - q / .22f));
                }
            }
            Word(age, 1, 4.5f, rainbow ? "RAINBOW" : "AWAKENED", hue0 + .3f, 1.2f); Sound(age, 4.5f, "up.rarity.gold", 2);
            Word(age, 2, 6.5f, rainbow ? "LEGEND" : "PURPLE STAR", hue0 + .6f, 1.2f); Sound(age, 6.5f, "up.star3", 3);

            // 5. the rainbow's extra: rays, an arch, comets, a second and third flash, the fireworks of the end
            if (rainbow)
            {
                float arch = Mathf.Clamp01((age - 6f) / 3f);
                if (arch > 0)
                    for (int band = 0; band < 7; band++)
                    {
                        path.Clear();
                        float r = 2.6f + band * .16f;
                        for (int k = 0; k <= 28; k++) { float a = Mathf.Lerp(Mathf.PI, 0, k / 28f); path.Add(at + new Vector2(Mathf.Cos(a) * r * 1.6f, Mathf.Sin(a) * r + .3f)); }
                        ink.Ribbon(path, Hue(band / 7f), .13f, arch, 0f, fade, !calm);
                    }
                float grow = Mathf.Clamp01((age - 3.2f) / 1.2f);
                for (int ray = 0; ray < 7; ray++)
                {
                    float a = (spin * 24f + ray * 360f / 7f) * Mathf.Deg2Rad;
                    ink.Stroke(heart, heart + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 5.5f * grow, Hue(ray / 7f + hue0), .34f, 1f, .1f, .55f * fade);
                }
                if (age > 3f)
                    for (int i = 0; i < 6; i++)
                    {
                        float q = ((age - 3f) / 1.5f + i * .19f) % 1f;
                        Vector2 from = at + new Vector2(-6f + Rand(i, 10) * 3f, 5.5f - Rand(i, 11) * 1.5f), to = from + new Vector2(11f, -5f), p = Vector2.Lerp(from, to, q);
                        float a = fade * Mathf.Sin(q * Mathf.PI);
                        ink.Stroke(p - (to - from).normalized * 1.6f, p, Hue(i / 6f + hue0), .16f, 1f, 0f, .8f * a);
                        ink.TokenAt(InkToken.Star, Hue(i / 6f + hue0), p, .3f, spin * 200f, .9f * a);
                    }
                foreach (float when in new[] { 6f, 9.5f })
                {
                    float f = 1 - (age - when) / .6f;
                    if (age >= when && f > 0) ink.Light(heart, 12f * f + 4f, Tone(hue0, .5f), .7f * f * fade);
                }
                Word(age, 3, 9.5f, "BEYOND STARS", hue0, 1.2f); Sound(age, 6f, "up.star3", 4); Sound(age, 9.5f, "up.rarity.gold", 5);
                if (age > 11.5f && age < 15f)
                    for (int burst = 0; burst < 4; burst++)
                    {
                        float q = (age - 11.5f - burst * .7f) / 1.4f; if (q <= 0 || q >= 1) continue;
                        Vector2 c = at + new Vector2(-4.2f + burst * 2.8f, 2.6f + (burst % 2) * 1.4f);
                        for (int k = 0; k < 14; k++)
                        {
                            float a = k * 25.7f * Mathf.Deg2Rad, r = 1.7f * (1 - (1 - q) * (1 - q));
                            ink.TokenAt(k % 2 == 0 ? InkToken.Spark : InkToken.Glint, Hue(k / 14f + burst * .25f), c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r + Vector2.down * q * q * .6f, .24f, a * 57f, (1 - q) * fade);
                        }
                        ink.Light(c, 1.6f * (1 - q), Tone(burst * .25f), .5f * (1 - q) * fade);
                    }
            }
            ink.End();
        }

        private void OnDestroy() { ink?.Destroy(); }
    }
}
