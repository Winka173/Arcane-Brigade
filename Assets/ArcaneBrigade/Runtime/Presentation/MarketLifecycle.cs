using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MarketLifecycle : MonoBehaviour
    {
        private ArenaController owner;
        private float started;
        private bool departing;
        private bool dream;
        private Transform rug;
        private Vector3 rugScale;
        private Vector3 rugPosition;
        private SpriteRenderer[] wisps;
        public bool Busy => departing || owner != null && owner.PresentationClock - started < 0.65f;
        public void Configure(ArenaController controller, bool special, bool entrance)
        {
            owner = controller; dream = special; started = owner.PresentationClock - (entrance ? 0 : 1);
            rug = transform.Find("PaintedMarketRug");
            if (rug != null) { rugScale = rug.localScale; rugPosition = rug.localPosition; }
            if (!dream) return;
            wisps = new SpriteRenderer[8];
            for (int index = 0; index < wisps.Length; index++)
            {
                var child = new GameObject("DreamVeil_" + index); child.transform.SetParent(transform, false);
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = owner.Content.Arena.ImpactSprite;
                renderer.sortingOrder = 450; renderer.color = new Color(0.66f, 0.42f, 0.92f, 0.1f); wisps[index] = renderer;
            }
        }
        public void Depart() { if (departing) return; departing = true; started = owner.PresentationClock; }
        public void Reenter() { departing = false; started = owner.PresentationClock; transform.localScale = Vector3.one; transform.localPosition = Vector3.zero; }
        private void Update()
        {
            if (owner == null || owner.Simulation == null) return;
            float progress = Mathf.Clamp01((owner.PresentationClock - started) / (departing ? 0.78f : 0.65f));
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            if (departing)
            {
                float fold = Mathf.SmoothStep(0, 1, progress);
                transform.localScale = new Vector3(1 - fold * 0.92f, 1 - fold * 0.99f, 1);
                transform.localPosition = new Vector3(Mathf.Sin(progress * Mathf.PI * 2) * (reduced ? 0 : 0.15f), fold * (dream ? 2.7f : 1.8f), 0);
            }
            else if (rug != null)
            {
                float unfurl = Mathf.SmoothStep(0.02f, 1, progress);
                rug.localScale = Vector3.Scale(rugScale, new Vector3(1, unfurl, 1));
                rug.localPosition = rugPosition + Vector3.up * (1 - unfurl) * 1.1f;
            }
            if (wisps == null) return;
            for (int index = 0; index < wisps.Length; index++)
            {
                float phase = owner.PresentationClock * (reduced ? 0 : 0.4f) + index * 0.83f;
                SpriteRenderer wisp = wisps[index];
                wisp.transform.localPosition = new Vector3(Mathf.Cos(phase) * 4.8f, 0.5f + Mathf.Sin(phase * 1.3f) * 2.1f, 0);
                if (wisp.sprite != null) wisp.transform.localScale = Vector3.one * (2.8f / wisp.sprite.bounds.size.y);
                wisp.color = new Color(0.64f, 0.4f, 0.92f, (0.07f + (reduced ? 0 : Mathf.Sin(phase * 2) * 0.025f)) * (departing ? 1 - progress : progress));
            }
        }
    }
}
