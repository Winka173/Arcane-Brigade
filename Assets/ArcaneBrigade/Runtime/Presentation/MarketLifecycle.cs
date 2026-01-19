using System.Linq;
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
        private MarketCarpetRoll carpet;
        private SpriteRenderer[] wisps;
        private ShopDisplayActor[] fixtures;
        private Vector3[] fixtureOrigins;
        private SpriteRenderer[] floorDecor;
        private Color[] floorColors;
        public bool Busy => departing || owner != null && owner.PresentationClock - started < MarketMotion.ArrivalDuration;
        public void Configure(ArenaController controller, bool special, bool entrance)
        {
            owner = controller; dream = special; started = owner.PresentationClock - (entrance ? 0 : 2);
            if (entrance) CheckpointAudio.PlayInterfaceId("shop.carpet.open");
            rug = transform.Find("PaintedMarketRug");
            if (rug != null) { carpet = rug.gameObject.AddComponent<MarketCarpetRoll>(); carpet.Configure(rug.GetComponent<SpriteRenderer>()); }
            fixtures = GetComponentsInChildren<ShopDisplayActor>(); fixtureOrigins = new Vector3[fixtures.Length];
            for (int index = 0; index < fixtures.Length; index++) fixtureOrigins[index] = fixtures[index].transform.localPosition;
            floorDecor = GetComponentsInChildren<SpriteRenderer>().Where(renderer => renderer.GetComponentInParent<ShopDisplayActor>() == null && (renderer.gameObject.name == "ShopFloorContact" || renderer.gameObject.name == "MarketPennants" || renderer.sortingOrder == -75)).ToArray();
            floorColors = floorDecor.Select(renderer => renderer.color).ToArray();
            if (!dream) return;
            wisps = new SpriteRenderer[8];
            for (int index = 0; index < wisps.Length; index++)
            {
                var child = new GameObject("DreamVeil_" + index); child.transform.SetParent(transform, false);
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = owner.Content.Arena.ImpactSprite;
                renderer.sortingOrder = 450; renderer.color = new Color(0.66f, 0.42f, 0.92f, 0.1f); wisps[index] = renderer;
            }
        }
        public void Depart()
        {
            if (departing) return;
            departing = true; started = owner.PresentationClock;
            CheckpointAudio.CancelScheduledPrefix("shop.");
            CheckpointAudio.CancelScheduledPrefix("up.rarity.");
            CheckpointAudio.PlayInterfaceId("shop.carpet.roll", MarketMotion.CarpetDepartureDelay);
            CheckpointAudio.PlayInterfaceId(dream ? "shop.keeper.exit.dream" : "shop.keeper.exit.gold", MarketMotion.MerchantDepartureDelay);
        }
        public void Reenter() { departing = false; started = owner.PresentationClock; transform.localScale = Vector3.one; transform.localPosition = Vector3.zero; }
        private void Update()
        {
            if (owner == null || owner.Simulation == null) return;
            float elapsed = owner.PresentationClock - started;
            float progress = MarketMotion.Progress(elapsed, departing ? MarketMotion.CarpetDepartureDelay : 0, departing ? 0.75f : 0.9f);
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            if (carpet != null) carpet.Display(reduced ? 1 : Mathf.SmoothStep(0, 1, departing ? 1 - progress : progress), departing ? 1 - progress : 1);
            for (int index = 0; index < floorDecor.Length; index++)
            {
                if (floorDecor[index] == null) continue;
                Color color = floorColors[index]; color.a *= departing ? 1 - progress : progress;
                floorDecor[index].color = color;
            }
            for (int index = 0; index < fixtures.Length; index++)
            {
                if (fixtures[index].enabled) continue;
                float rise = departing ? 1 - progress : progress;
                fixtures[index].transform.localPosition = fixtureOrigins[index] + Vector3.down * (reduced ? 0 : (1 - rise) * 0.65f);
                SpriteRenderer renderer = fixtures[index].GetComponent<SpriteRenderer>();
                Color tint = renderer.color; tint.a = rise; renderer.color = tint;
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
