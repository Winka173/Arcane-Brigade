using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The puff a friend leaves when it vanishes for its star-up and returns in once the animation ends (requirement 27/10/2026):
    /// soft cream puffs roll outward, little glints lift off it and a warm light flashes. Code ink; it removes itself.</summary>
    public sealed class VanishPoof : MonoBehaviour
    {
        private ArsenalInk ink;
        private Vector2 at;
        private float born;
        private bool arriving;
        private const float Life = .6f;

        public static void Play(Vector2 at, bool arriving)
        {
            var go = new GameObject("VanishPoof");
            VanishPoof p = go.AddComponent<VanishPoof>();
            p.at = at; p.arriving = arriving; p.born = Time.unscaledTime;
            CheckpointAudio.PlayInterfaceId(arriving ? "up.merge" : "up.merge.gather", 0f);
        }

        private void Update()
        {
            if (ink == null) ink = new ArsenalInk(transform, 66, 2400);
            float age = Time.unscaledTime - born;
            ink.Begin();
            if (age >= Life) { ink.End(); ink.Destroy(); Destroy(gameObject); return; }
            // arriving runs the same picture backwards: the puffs gather in as the friend appears
            float k = age / Life, q = arriving ? 1 - k : k, fade = arriving ? Mathf.Clamp01(k * 2.2f) * Mathf.Clamp01((1 - k) * 3f + .3f) : Mathf.Clamp01((1 - k) * 1.6f);
            int n = PresentationSettings.ReducedEffects ? 5 : 11;
            for (int i = 0; i < n; i++)
            {
                float a = (i * 360f / n + 17f * (i % 3)) * Mathf.Deg2Rad, r = (.25f + .8f * (1 - (1 - q) * (1 - q))) * (.8f + .4f * ((i * 7) % 5) / 4f);
                Vector2 p = at + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * .75f + q * .25f);
                ink.TokenAt(InkToken.Puff, ArsenalInk.Cotton, p, .5f * (.7f + .6f * q), i * 40f, .85f * fade);
            }
            for (int i = 0; i < n / 2; i++)
            {
                float a = (i * 360f / (n / 2) + 40f) * Mathf.Deg2Rad, r = .3f + 1.1f * q;
                ink.TokenAt(i % 2 == 0 ? InkToken.Glint : InkToken.Star, ArsenalInk.Gold, at + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * .8f + q * .5f), .26f, q * 200f, fade);
            }
            ink.Light(at, 2.2f + q * 1.2f, new Color(1f, .92f, .7f), .7f * fade * (1 - Mathf.Abs(.5f - k)));
            ink.End();
        }
    }
}
