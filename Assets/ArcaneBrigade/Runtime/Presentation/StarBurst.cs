using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The flourish when a friend gains stars (requirement 27/10/2026): a small one for a half star (1.5, 2.5), a bigger one
    /// on top of the merge ceremony for 2 and 3 stars. Gold glints spiral out and up round a rising star, with the star count in a
    /// comic word; the half star is shorter and quieter. It draws itself and removes itself.</summary>
    public sealed class StarBurst : MonoBehaviour
    {
        private ArsenalInk ink;
        private Vector2 at;
        private float born, life, stars;
        private int glints;
        private bool merge;

        /// <param name="stars">1.5 and 2.5 are half stars; 2 and 3 are merges.</param>
        public static void Play(Vector2 at, float stars, bool merge)
        {
            var go = new GameObject("StarBurst");
            StarBurst burst = go.AddComponent<StarBurst>();
            burst.at = at; burst.stars = stars; burst.merge = merge;
            burst.born = Time.unscaledTime;
            bool three = stars >= 3f;
            burst.life = merge ? (three ? 1.9f : 1.5f) : 1.0f;
            burst.glints = PresentationSettings.ReducedEffects ? 4 : merge ? (three ? 22 : 14) : 6;
            string word = Mathf.Approximately(stars, Mathf.Floor(stars)) ? (int)stars + " STARS!" : stars.ToString("0.0") + " STARS";
            ImpactWords.Pop(word, at + Vector2.up * 1.25f, three ? ImpactWords.Gold : ImpactWords.Cream, merge ? 3 : 2, merge ? 1.05f : .8f, merge ? -5f : 4f);
            CheckpointAudio.PlayInterfaceId(merge ? (three ? "up.star3" : "up.star2") : "up.star2", merge ? 0f : 0.08f);
        }

        private void Update()
        {
            if (ink == null) ink = new ArsenalInk(transform);
            float age = Time.unscaledTime - born;
            ink.Begin();
            if (age >= life) { ink.End(); Destroy(gameObject); return; }
            bool still = PresentationSettings.ReducedMotion;
            float k = age / life, out_ = 1 - (1 - k) * (1 - k) * (1 - k), fade = Mathf.Clamp01((1 - k) / .35f);
            float reach = merge ? (stars >= 3f ? 1.6f : 1.2f) : .75f;
            for (int i = 0; i < glints; i++)
            {
                // each glint leaves the star on its own spiral, a little later than the one before
                float delay = i * (merge ? .018f : .03f), q = Mathf.Clamp01((age - delay) / (life - delay));
                if (q <= 0) continue;
                float angle = (i * 360f / glints + (still ? 0 : q * 160f)) * Mathf.Deg2Rad, r = reach * (1 - (1 - q) * (1 - q)) * (.55f + .45f * ((i * 7) % 5) / 4f);
                Vector2 p = at + Vector2.up * (1.1f + q * .9f) + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .7f) * r;
                ink.TokenAt(i % 3 == 0 ? InkToken.Star : InkToken.Glint, ArsenalInk.Gold, p, (merge ? .26f : .2f) * (1 - .4f * q), still ? 0 : q * 240f, (1 - q) * .95f);
            }
            // the star itself rises, swells and settles; a half star is a smaller, paler one
            float size = (merge ? .7f : .42f) * (.7f + .3f * Mathf.Sin(Mathf.Clamp01(k * 3f) * Mathf.PI * .5f));
            ink.TokenAt(InkToken.Star, merge ? ArsenalInk.Gold : ArsenalInk.Cream, at + Vector2.up * (1.1f + out_ * .8f), size, still ? 0 : k * 90f, fade);
            if (merge && !PresentationSettings.ReducedEffects) ink.Light(at + Vector2.up * (1.1f + out_ * .8f), 1.8f + out_ * 1.2f, new Color(1f, .86f, .45f), .4f * fade);
            ink.End();
        }
    }
}
