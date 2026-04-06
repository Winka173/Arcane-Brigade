using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A boss's contact shape matches its painted body (play-test item 22): an ellipse about 85% of the body's ink box
    /// (SpriteInkMetrics), placed where UnitVisual draws it, organs included. Touching the boss art always hurts; standing visibly
    /// clear of it never does. The old collision circle still counts too, so the shape never shrinks.</summary>
    public static class BossBodyShape
    {
        public const float Share = .85f;
        /// <summary>A boss's contact lands far more often on its painted body than on the old small circle, so each touch hurts
        /// less: the damage a boss wave deals stays where the bot sweeps had it (phase D slope).</summary>
        public const float ContactDamageScale = .4f;
        public struct Shape
        {
            public Vector2 Centre;  // from the boss's position, facing right
            public Vector2 Half;    // half width and half height of the contact ellipse
            public Rect Painted;    // the painted ink box, from the boss's position, facing right
            public bool Valid;
        }
        private static readonly Dictionary<UnitDefinition, (float height, Shape shape)> cache = new Dictionary<UnitDefinition, (float, Shape)>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() => cache.Clear();

        /// <summary>The painted body of a boss as UnitVisual draws it: the idle frame scaled to BodyHeight, feet at the bottom of
        /// that height (canon actors: their ink's foot), the Keeper's shell, plus BossAnatomy organs around the core.</summary>
        public static Shape Of(UnitDefinition boss)
        {
            if (boss == null) return default;
            if (cache.TryGetValue(boss, out var hit) && Mathf.Approximately(hit.height, boss.BodyHeight)) return hit.shape;
            Shape shape = Measure(boss);
            cache[boss] = (boss.BodyHeight, shape);
            return shape;
        }

        private static Shape Measure(UnitDefinition boss)
        {
            Sprite sprite = boss.Body; float height = Mathf.Max(.01f, boss.BodyHeight);
            bool canon = false; float reference = 0;
            ActOneArtEntry entry = ActOneArtwork.Has(boss) ? ActOneArtwork.Find(ActOneArtwork.Identity(boss)) : null;
            if (entry != null && entry.Frames != null && entry.Frames.Length > 0 && entry.Motion != null) { canon = true; sprite = entry.Frames[0]; reference = entry.Motion.ReferenceHeight; }
            if (boss.Id == "OutsideStageKeeper") sprite = ActOneArtwork.Picture("Keeper_BodyShell") ?? sprite;
            if (sprite == null) return new Shape { Centre = Vector2.zero, Half = Vector2.one * boss.Radius, Painted = new Rect(-boss.Radius, -boss.Radius, boss.Radius * 2, boss.Radius * 2) };
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            float scale = height / Mathf.Max(.01f, canon && reference > 0 ? reference : sprite.bounds.size.y);
            float lift = canon ? -height * .5f - ink.yMin * scale : height * (sprite.pivot.y / Mathf.Max(1, sprite.rect.height) - .5f);
            Rect box = Rect.MinMaxRect(ink.xMin * scale, ink.yMin * scale + lift, ink.xMax * scale, ink.yMax * scale + lift);
            BossAnatomyEntry anatomy = canon ? null : BossAnatomy.For(boss);
            if (anatomy != null && anatomy.Organs != null)
                foreach (BossOrganDefinition organ in anatomy.Organs)
                {
                    if (organ == null || organ.Rest == null) continue;
                    Rect rest = SpriteInkMetrics.BoundsOf(organ.Rest);
                    float size = organ.Height * ink.height, aspect = rest.width / Mathf.Max(.01f, rest.height);
                    Vector2 centre = (ink.center + organ.Offset * ink.height) * scale + Vector2.up * lift;
                    Vector2 half = new Vector2(size * aspect, size) * .5f * scale;
                    box = Rect.MinMaxRect(Mathf.Min(box.xMin, centre.x - half.x), Mathf.Min(box.yMin, centre.y - half.y), Mathf.Max(box.xMax, centre.x + half.x), Mathf.Max(box.yMax, centre.y + half.y));
                }
            return new Shape { Centre = box.center, Half = box.size * .5f * Share, Painted = box, Valid = true };
        }

        /// <summary>The contact ellipse's centre in the world (mirrored when the boss faces left, as its art is).</summary>
        public static Vector2 Centre(UnitState boss, Shape shape) => boss.Position + new Vector2(boss.Facing.x < -.1f ? -shape.Centre.x : shape.Centre.x, shape.Centre.y + boss.BodyLift);

        /// <summary>Does a body of this radius at this point touch the boss? Its painted ellipse grown by the radius, or its old circle.</summary>
        public static bool Touches(UnitState boss, Vector2 point, float radius)
        {
            float circle = boss.Definition.Radius + radius + .05f;
            if ((boss.Position - point).sqrMagnitude <= circle * circle) return true;
            if (MapIdentity.Off("contact")) return false;
            Shape shape = Of(boss.Definition);
            if (!shape.Valid) return false;
            Vector2 d = point - Centre(boss, shape);
            float ax = shape.Half.x + radius, ay = shape.Half.y + radius;
            return d.x * d.x / (ax * ax) + d.y * d.y / (ay * ay) <= 1;
        }
    }
}

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        /// <summary>For captures (play-test item 22): the boss winds up an aimed fan while a slam, a lash and a shockwave wait in
        /// their warnings around the hero, so every telegraph shape is on screen at once.</summary>
        public void BossTelegraphCaptureFixture()
        {
            UnitState boss = Boss; if (boss == null || !boss.Alive || Champion == null) return;
            boss.EntranceUntil = 0; boss.ActiveBossSignature = -1; boss.ActiveTrademark = false;
            boss.TelegraphDirection = (Champion.Position - boss.Position).normalized; boss.TelegraphUntil = Elapsed + 1.6f;
            boss.NextBossPattern = Elapsed + 30; boss.BossRecoveryUntil = Elapsed + 30;
            Vector2 hero = Champion.Position, side = new Vector2(-boss.TelegraphDirection.y, boss.TelegraphDirection.x);
            Danger(ExpandedFieldKind.FallingProp, boss, hero + side * 2.6f + Vector2.up * .6f, .95f, 2.2f, .4f);
            Danger(ExpandedFieldKind.Tentacle, boss, hero - side * 3.2f, .4f, 2.4f, .45f);
            Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 4.5f, 2.6f, 1.4f);
        }
    }
}
