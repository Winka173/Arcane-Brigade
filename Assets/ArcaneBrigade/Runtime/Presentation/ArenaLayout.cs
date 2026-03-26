using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The presentation's door to <see cref="ArenaLayoutPlan"/>: every fixed combat prop (warm chair, landmarks,
    /// Act I zone props and seated travellers, lanterns, dream architecture, distant watchers) asks here where it stands
    /// instead of writing its own position, so no two of them overlap. The shop keeps using ShopLayout; outside combat
    /// (shop, secret rooms) there is no plan and callers keep their own wish.</summary>
    public static class ArenaLayout
    {
        /// <summary>This fight's plan, or null in the shop and secret rooms.</summary>
        public static ArenaLayoutPlan Combat(ArenaController owner)
        {
            if (owner?.Session == null || owner.Session.State.Phase == SessionPhase.Shop || owner.Session.SecretRoomOpen) return null;
            return ArenaLayoutPlan.For(ArenaScenery.For(owner.Session.State, owner.WorldBounds));
        }
        /// <summary>Where the prop stands this fight. False when it sits this wave out (hide it). Fit is its planned size
        /// multiplier; foot and fit stay as given when there is no plan or the prop is not part of it.</summary>
        public static bool Place(ArenaController owner, string id, ref Vector2 foot, out float fit)
        {
            fit = 1;
            ArenaLayoutPlan plan = Combat(owner);
            if (plan == null || !plan.TryGet(id, out ArenaLayoutPlan.Spot spot)) return true;
            if (!spot.Visible) return false;
            foot = spot.Foot; fit = spot.Fit; return true;
        }
        /// <summary>As Place, for art whose ink at full size is drawn: fit also keeps it inside its planned footprint.</summary>
        public static bool Place(ArenaController owner, string id, Vector2 drawn, ref Vector2 foot, out float fit)
        {
            fit = 1;
            ArenaLayoutPlan plan = Combat(owner);
            if (plan == null || !plan.TryGet(id, out ArenaLayoutPlan.Spot spot)) return true;
            if (!spot.Visible) return false;
            foot = spot.Foot; fit = spot.Scale(drawn); return true;
        }
        /// <summary>Planned floor contact of a prop this fight (its wish when there is no plan).</summary>
        public static Vector2 Foot(ArenaController owner, string id, Vector2 wish)
        {
            Place(owner, id, ref wish, out _); return wish;
        }
        /// <summary>Painted ink size of a sprite drawn at this scale.</summary>
        public static Vector2 Drawn(Sprite sprite, float scale) => sprite == null ? Vector2.zero : SpriteInkMetrics.BoundsOf(sprite).size * scale;
    }
}
