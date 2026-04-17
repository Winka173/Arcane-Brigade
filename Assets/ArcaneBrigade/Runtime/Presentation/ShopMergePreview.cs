using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ShopMergePreview : MonoBehaviour
    {
        private ArenaController owner;
        private readonly List<int> identities = new List<int>();
        private readonly LineRenderer[] links = new LineRenderer[2];
        private Material material;
        private Vector2 stock;
        private int hintedToken = -1;
        public int ApproachingCount => identities.Count;
        public static bool Ready(RunSession session, ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion && offer.Tier < 3
            && session.CanBuy(offer) && session.State.Roster.FindAll(entry => entry.UnitId == offer.Id && entry.Tier == Mathf.Clamp(offer.Tier, 1, 3)).Count >= CompanionDeployment.MergeCopies - 1;
        public static int ProjectedTier(RunSession session, ShopOffer offer)
        {
            if (!Ready(session, offer)) return offer == null ? 1 : Mathf.Clamp(offer.Tier, 1, 3);
            var counts = new int[4];
            foreach (RosterEntry entry in session.State.Roster) if (entry.UnitId == offer.Id) counts[Mathf.Clamp(entry.Tier, 1, 3)]++;
            int result = Mathf.Clamp(offer.Tier, 1, 3); counts[result]++;
            for (int tier = 1; tier < 3; tier++)
                if (counts[tier] >= CompanionDeployment.MergeCopies) { counts[tier + 1] += counts[tier] / CompanionDeployment.MergeCopies; counts[tier] %= CompanionDeployment.MergeCopies; result = tier + 1; }
            return result;
        }
        public void Configure(ArenaController controller)
        {
            owner = controller; material = new Material(Shader.Find("Sprites/Default"));
            for (int index = 0; index < links.Length; index++)
            {
                var child = new GameObject("MergePreviewThread_" + index); child.transform.SetParent(transform, false);
                LineRenderer line = child.AddComponent<LineRenderer>(); line.sharedMaterial = material;
                line.positionCount = 3; line.useWorldSpace = true; line.widthMultiplier = 0.035f; line.sortingOrder = 470;
                line.startColor = new Color(1, 0.84f, 0.38f, 0.45f); line.endColor = new Color(1, 0.84f, 0.38f, 0.08f);
                line.enabled = false; links[index] = line;
            }
        }
        public void Observe(WorldShop.Station station, bool blocked)
        {
            if (blocked || station == null) CheckpointAudio.CancelScheduledPrefix("ui.merge.hint");
            identities.Clear();
            if (!blocked && station != null && Ready(owner.Session, station.Offer))
            {
                if (hintedToken != station.Offer.Token) { CheckpointAudio.PlayInterfaceId("ui.merge.hint", 0.13f); hintedToken = station.Offer.Token; }
                stock = station.Position;
                foreach (UnitState unit in owner.Simulation.Units)
                {
                    if (unit.Faction != Faction.Companion || !unit.Alive || unit.Definition.Id != station.Offer.Id) continue;
                    identities.Add(unit.Id); if (identities.Count == 2) break;
                }
            }
            for (int index = 0; index < links.Length; index++) links[index].enabled = index < identities.Count && !PresentationSettings.ReducedMotion;
        }
        public bool Destination(UnitState unit, out Vector2 destination)
        {
            int index = identities.IndexOf(unit.Id);
            destination = stock + new Vector2(index == 0 ? -0.9f : 0.9f, -0.45f);
            return index >= 0;
        }
        private void LateUpdate()
        {
            if (owner == null || owner.Simulation == null || owner.Simulation.Phase == RunPhase.Paused) return;
            for (int index = 0; index < identities.Count; index++)
            {
                UnitState unit = null;
                foreach (UnitState candidate in owner.Simulation.Units) if (candidate.Id == identities[index]) { unit = candidate; break; }
                if (unit == null) { links[index].enabled = false; continue; }
                Vector3 start = unit.Position + Vector2.up * 0.2f;
                Vector3 end = stock + Vector2.up * 0.55f;
                links[index].SetPosition(0, start); links[index].SetPosition(1, Vector3.Lerp(start, end, 0.5f) + Vector3.up * (0.16f + Mathf.Sin(owner.PresentationClock * 3) * 0.04f)); links[index].SetPosition(2, end);
            }
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
