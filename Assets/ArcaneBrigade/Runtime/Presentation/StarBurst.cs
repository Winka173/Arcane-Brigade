using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The flourish when a friend gains stars (requirement 27/10/2026): a small one for a half star (1.5, 2.5), a bigger one
    /// on top of the merge ceremony for 2 and 3 stars. Gold glints spiral out and up round a rising star, with the star count in a
    /// comic word; the half star is shorter and quieter. It draws itself and removes itself.</summary>
    public sealed class StarBurst : MonoBehaviour
    {
        public static int Alive;
        private void Awake() { Alive++; }
        private void OnDestroy() { Alive--; }
        private ArsenalInk ink;
        private string[] lines;
        private InkedText[] lineTexts;
        private Vector2 at;
        private float born, life, stars;
        private int glints;
        private bool merge;

        /// <param name="stars">1.5 and 2.5 are half stars; 2 and 3 are merges.</param>
        public static void Play(Vector2 at, float stars, bool merge, string[] lines = null)
        {
            var go = new GameObject("StarBurst");
            StarBurst burst = go.AddComponent<StarBurst>();
            burst.at = at; burst.stars = stars; burst.merge = merge;
            burst.born = Time.unscaledTime;
            burst.lines = lines;
            GrandStarCeremony.PlayLight(at, stars);
            bool three = stars >= 3f;
            burst.life = merge ? (three ? 1.9f : 1.5f) : 1.7f;
            burst.glints = PresentationSettings.ReducedEffects ? 4 : merge ? (three ? 22 : 14) : 18;
            string word = Mathf.Approximately(stars, Mathf.Floor(stars)) ? (int)stars + " STARS!" : stars.ToString("0.0") + " STARS";
            ImpactWords.Pop(word, at + Vector2.up * 1.25f, stars >= 3.5f ? GrandStarCeremony.Purple : three ? ImpactWords.Gold : ImpactWords.Cream, merge ? 3 : 2, merge ? 1.05f : 1.5f, merge ? -5f : 4f);
            CheckpointAudio.PlayInterfaceId(merge ? (three ? "up.star3" : "up.star2") : "up.star2", merge ? 0f : 0.08f);
        }

        private Sprite StarArt()
        {
            int next = Mathf.FloorToInt(stars) + 1; // the star it is on the way to
            return ArsenalArtwork.Frame(next >= 5 ? "StarUp_RainbowStar" : next == 4 ? "StarUp_PurpleStar" : next == 3 ? "StarUp_GoldStar" : "StarUp_SilverStar", 0);
        }

        private void Update()
        {
            if (ink == null) ink = new ArsenalInk(transform, 66, 2400); // above the shop's own sprites
            float age = Time.unscaledTime - born;
            ink.Begin();
            if (age >= life) { ink.End(); Destroy(gameObject); return; }
            bool still = PresentationSettings.ReducedMotion;
            InkPalette tint = stars >= 3.5f && !merge ? GrandStarCeremony.Amethyst : ArsenalInk.Gold;
            float k = age / life, out_ = 1 - (1 - k) * (1 - k) * (1 - k), fade = Mathf.Clamp01((1 - k) / .35f);
            float reach = merge ? (stars >= 3f ? 1.6f : 1.2f) : 1.5f;
            for (int i = 0; i < glints; i++)
            {
                // each glint leaves the star on its own spiral, a little later than the one before
                float delay = i * (merge ? .018f : .03f), q = Mathf.Clamp01((age - delay) / (life - delay));
                if (q <= 0) continue;
                float angle = (i * 360f / glints + (still ? 0 : q * 160f)) * Mathf.Deg2Rad, r = reach * (1 - (1 - q) * (1 - q)) * (.55f + .45f * ((i * 7) % 5) / 4f);
                Vector2 p = at + Vector2.up * (1.1f + q * .9f) + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .7f) * r;
                ink.TokenAt(i % 3 == 0 ? InkToken.Star : InkToken.Glint, tint, p, (merge ? .26f : .55f) * (1 - .4f * q), still ? 0 : q * 240f, (1 - q) * .95f);
            }
            // a half star shows the painted star it is on the way to, popping in with a halo, then the stats it just earned
            if (!merge)
            {
                Sprite art = StarArt();
                float pop = Mathf.Clamp01(age / .28f), over = 1 + 1.7f * Mathf.Pow(pop - 1, 3) + .7f * Mathf.Pow(pop - 1, 2);
                Vector2 starAt = at + Vector2.up * (1.45f + Mathf.Sin(age * 3f) * .05f);
                if (art != null) ink.Place(art, starAt, .95f * over, still ? 0 : Mathf.Sin(age * 2.2f) * 6f, new Color(1, 1, 1, .92f * fade), true);
                if (lines != null && lineTexts == null)
                {
                    lineTexts = new InkedText[lines.Length];
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lineTexts[i] = InkedText.Create(transform, "HalfStarStat" + i, ArcadeTypography.Punch, 64, .04f, 2420, .03f);
                        lineTexts[i].Text = lines[i];
                    }
                }
                if (lineTexts != null)
                    for (int i = 0; i < lineTexts.Length; i++)
                    {
                        float since = Mathf.Clamp01((age - .35f - i * .22f) / .3f);
                        lineTexts[i].transform.position = new Vector3(starAt.x, starAt.y + .85f + i * .5f + since * .15f, 0);
                        Color c = stars >= 3.5f ? GrandStarCeremony.Purple : stars >= 2.5f ? new Color(1f, .86f, .4f) : new Color(.85f, .95f, 1f);
                        c.a = since * fade; lineTexts[i].Color = c;
                    }
            }
            // the star itself rises, swells and settles; a half star is a smaller, paler one
            float size = (merge ? .7f : 1.35f) * (.7f + .3f * Mathf.Sin(Mathf.Clamp01(k * 3f) * Mathf.PI * .5f));
            ink.TokenAt(InkToken.Star, merge ? ArsenalInk.Gold : stars >= 3.5f ? GrandStarCeremony.Orchid : ArsenalInk.Cream, at + Vector2.up * (1.1f + out_ * .8f), size, still ? 0 : k * 90f, fade);
            if (!PresentationSettings.ReducedEffects) ink.Light(at + Vector2.up * (1.1f + out_ * .8f), (merge ? 1.8f : 4.2f) + out_ * 1.6f, stars >= 3.5f && !merge ? new Color(.74f, .5f, 1f) : new Color(1f, .86f, .45f), (merge ? .4f : .85f) * fade);
            ink.End();
        }
    }
}
