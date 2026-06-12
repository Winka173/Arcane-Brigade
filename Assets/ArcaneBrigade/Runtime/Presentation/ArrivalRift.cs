using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The brigade arrives through a rift (requirement 24/10/2026): at the start of a night the same tear the shop showed
    /// opens in the air where the hero stands, the hero and friends spring out of it, and it stitches itself shut - a seam of
    /// light pulled closed by a few cross-stitches - and is gone. About 1.4 s; painted rim and swirl (Shop/Portals), the
    /// stitches in code.</summary>
    public sealed class ArrivalRift : MonoBehaviour
    {
        private const float Open = .28f, Hold = .95f, Close = 1.4f, Height = 2f;
        private SpriteRenderer body, swirl;
        private Transform swirlHolder;
        private Vector4 hole;
        private float born, turn;
        private InkCanvas stitches; private GameObject stitchHolder;
        /// <summary>When the latest rift is stitched shut (unscaled time); the wave stamp waits for it.</summary>
        public static float ClosesAt { get; private set; } = -10;

        public static ArrivalRift Play(MapTheme theme, Vector2 centre, int order)
        {
            var art = OctoberDoor.PortalArt(theme);
            if (art.body == null) return null;
            var rift = new GameObject("Arrival rift").AddComponent<ArrivalRift>();
            ClosesAt = Time.unscaledTime + Close;
            rift.transform.position = centre;
            rift.Build(art.body, art.swirl, OctoberDoor.PortalHoles[theme], order);
            return rift;
        }
        private void Build(Sprite bodySprite, Sprite swirlSprite, Vector4 h, int order)
        {
            hole = h; born = Time.time;
            body = new GameObject("Rim").AddComponent<SpriteRenderer>(); body.transform.SetParent(transform, false);
            body.sprite = bodySprite; body.sortingOrder = order;
            float scale = Height / Mathf.Max(.01f, bodySprite.bounds.size.y);
            body.transform.localScale = Vector3.one * scale;
            Bounds art = bodySprite.bounds;
            swirlHolder = new GameObject("Swirl holder").transform; swirlHolder.SetParent(body.transform, false);
            swirlHolder.localPosition = new Vector3(art.min.x + art.size.x * hole.x, art.min.y + art.size.y * hole.y, 0);
            Vector2 size = swirlSprite.bounds.size;
            swirlHolder.localScale = new Vector3(art.size.x * hole.z * 2.12f / size.x, art.size.y * hole.w * 2.12f / size.y, 1);
            swirl = new GameObject("Swirl").AddComponent<SpriteRenderer>(); swirl.transform.SetParent(swirlHolder, false);
            swirl.sprite = swirlSprite; swirl.sortingOrder = order - 1;
            stitchHolder = new GameObject("Rift stitches"); stitches = new InkCanvas(stitchHolder.transform, "Rift stitches", order + 2, true);
            Update();
        }
        private void Update()
        {
            float t = Time.time - born;
            if (t >= Close + .15f) { Destroy(stitchHolder); Destroy(gameObject); return; }
            bool still = PresentationSettings.ReducedMotion;
            // opening: it tears open from a slit; holding: it breathes; closing: it is pulled shut top to bottom
            float open = t < Open ? Mathf.SmoothStep(0, 1, t / Open) : t < Hold ? 1 : 1 - Mathf.SmoothStep(0, 1, (t - Hold) / (Close - Hold));
            transform.localScale = new Vector3(Mathf.Lerp(.15f, 1, Mathf.Sqrt(open)), Mathf.Max(.02f, open), 1);
            if (!still) turn += Time.deltaTime * Mathf.Lerp(90, 260, t < Hold ? 0 : (t - Hold) / (Close - Hold));
            swirl.transform.localRotation = Quaternion.Euler(0, 0, -turn);
            float a = Mathf.Clamp01(open * 1.6f);
            body.color = new Color(1, 1, 1, a); swirl.color = new Color(1.05f, 1.05f, 1.05f, a);
            // the closing seam: a bright line where the tear meets, crossed by stitches drawn shut
            stitches.Clear();
            if (t > Hold - .05f && t < Close + .15f)
            {
                float k = Mathf.Clamp01((t - Hold) / (Close - Hold)), fade = t > Close ? 1 - (t - Close) / .15f : 1;
                Vector2 c = transform.position, half = new Vector2(.75f, .9f) * (1 - .35f * k);
                Color seam = new Color(1f, .92f, .68f, .9f * fade);
                stitches.GlowRibbon(InkCanvas.Line(c - Vector2.up * half.y, c + Vector2.up * half.y), 1, 1, .05f, seam);
                for (int i = 0; i < 5; i++)
                {
                    float y = Mathf.Lerp(-half.y * .8f, half.y * .8f, i / 4f), w = Mathf.Lerp(.35f, .12f, k);
                    stitches.GlowRibbon(InkCanvas.Line(c + new Vector2(-w, y - .06f), c + new Vector2(w, y + .06f)), 1, 1, .035f, seam);
                }
            }
            stitches.Commit();
        }
    }
}
