using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ShopPickupEffect : MonoBehaviour
    {
        private sealed class Flight { public SpriteRenderer View; public GroundShadow Shadow; public ShopRarityGlow Glow; public Vector2 Origin; public float Age = 2; }
        private readonly List<Flight> flights = new List<Flight>();
        private ArenaController owner;
        public int CompletedCount { get; private set; }
        public int ActiveCount { get { int count = 0; foreach (Flight flight in flights) if (flight.Age < 0.85f) count++; return count; } }
        public void Configure(ArenaController controller) { owner = controller; }
        public void Play(Sprite sprite, Vector2 origin, ItemRarity rarity = ItemRarity.Common)
        {
            if (sprite == null) return;
            Flight flight = flights.Find(candidate => candidate.Age >= 0.85f);
            if (flight == null)
            {
                var child = new GameObject("CommittedItemFlight"); child.transform.SetParent(transform, false);
                flight = new Flight { View = child.AddComponent<SpriteRenderer>() }; flight.View.sortingOrder = 2400; flights.Add(flight);
                flight.Shadow = child.AddComponent<GroundShadow>();
            }
            flight.View.sprite = sprite; flight.Origin = origin; flight.Age = 0; flight.View.enabled = true;
            if (flight.Glow != null) Destroy(flight.Glow.gameObject);
            if (rarity != ItemRarity.Common)
            {
                var glow = new GameObject("PurchasedItemRarity"); glow.transform.SetParent(transform, false);
                flight.Glow = glow.AddComponent<ShopRarityGlow>();
                flight.Glow.Configure(rarity, owner, 0, flight.View);
            }
        }
        private void Update()
        {
            if (owner == null || owner.Simulation.Phase == RunPhase.Paused) return;
            foreach (Flight flight in flights)
            {
                if (flight.Age >= 0.85f) continue;
                flight.Age += Mathf.Min(Time.deltaTime, 0.05f);
                float progress = Mathf.Clamp01(flight.Age / 0.85f);
                Vector2 destination = owner.VisualChampionPosition + Vector2.up * 0.3f;
                Vector2 position = Vector2.Lerp(flight.Origin, destination, Mathf.SmoothStep(0, 1, progress));
                Vector2 ground = position - Vector2.up * 0.35f;
                bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
                if (!reduced) position.y += Mathf.Sin(progress * Mathf.PI) * 1.1f;
                flight.View.transform.position = position;
                flight.View.transform.localScale = Vector3.one * (0.8f / flight.View.sprite.bounds.size.y) * (1 - progress * 0.6f);
                flight.View.transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Sin(progress * Mathf.PI) * 16);
                flight.View.color = new Color(1, 1, 1, 1 - Mathf.Clamp01((progress - 0.8f) * 5));
                flight.Shadow.Display(flight.View.sprite, ground, 0.65f * (1 - progress * 0.6f), Mathf.Max(0, position.y - ground.y), 0.3f * flight.View.color.a);
                if (progress >= 1) { flight.View.enabled = false; flight.Shadow.Hide(); CompletedCount++; }
            }
        }
    }
}
