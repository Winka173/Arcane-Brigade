using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws the weight of our blows (ImpactRules): a flung body spinning along an arc with its shadow on the floor
    /// and a short inked trail (a body flung by a shot skims low and flat); a body pinned to the arena's edge by a pointed
    /// shot hangs there still with the shot through it; a living enemy pinned to the edge is pressed against it (UnitVisual)
    /// with strain strokes behind it; a dark ink splat where something hits the edge; an inked jolt where a flung body bowls
    /// into an enemy; scuffed skid marks behind a heavily shoved enemy; and a big inked STRIKE! when one flung chain fells
    /// three. All painted ink, nothing radiating from our side. Reduced motion keeps the bodies upright and drops the trails.</summary>
    public sealed class ImpactVisual : MonoBehaviour
    {
        private sealed class Mark { public int Kind; public Vector2 At, Direction; public float Born, Life, Size; }
        private const int SplatMark = 0, JoltMark = 1, SkidMark = 2, StrikeMark = 3;
        public const float StrikeLife = 1.1f;
        private ArsenalInk ink;
        private bool drawn;
        private float seenStep = -1;
        private readonly List<Mark> marks = new List<Mark>(32);
        private readonly List<InkedText> strikeLabels = new List<InkedText>(3);
        private readonly Dictionary<int, WeaponDefinition> launchedBy = new Dictionary<int, WeaponDefinition>();
        private static readonly InkPalette Splat = new InkPalette(new Color(.06f, .04f, .08f), new Color(.2f, .12f, .2f), new Color(.55f, .45f, .55f), new Color(.4f, .3f, .4f), new Color(.85f, .7f, .4f));
        // what the last frame showed (read by checks)
        public int Bodies { get; private set; }
        public int PinnedBodies { get; private set; }
        public int PinnedEnemies { get; private set; }
        public int Strikes { get; private set; }
        public int Marks => marks.Count;

        /// <summary>How high a flung body rides its arc: a full arc for a blow, a low flat skim for a shot, none when pinned.</summary>
        public static float BodyLift(FlungBody body, float now, bool still)
        {
            if (body == null || still || body.Pinned) return 0;
            float arc = Mathf.Sin(body.Progress(now) * Mathf.PI);
            return body.Low ? arc * .18f : arc * (.6f + .9f * body.Weight);
        }
        public static float BodySize(FlungBody body) => Mathf.Max(.35f, body.Definition != null ? body.Definition.BodyHeight : .5f) * 1.05f;

        public void Display(CombatSimulation simulation) => Display(simulation, Time.time);

        public void Display(CombatSimulation simulation, float now)
        {
            Bodies = PinnedBodies = PinnedEnemies = Strikes = 0;
            bool live = simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (live && simulation.Elapsed != seenStep)
            {
                seenStep = simulation.Elapsed;
                foreach (CombatEvent value in simulation.Events)
                {
                    if (value.Kind == CombatEventKind.WallSplat) Add(SplatMark, value.Position, value.Direction, 1.3f, value.Cause.ItemId != null && value.Cause.ItemId.EndsWith("pinned") ? .4f : .55f, now);
                    else if (value.Kind == CombatEventKind.BodyImpact) Add(JoltMark, value.Position, value.Direction, .35f, .45f + value.Value * .25f, now);
                    else if (value.Kind == CombatEventKind.Knockback && value.Value >= .35f) Add(SkidMark, value.Position, value.Direction, .5f, value.Value, now);
                    else if (value.Kind == CombatEventKind.Strike) Add(StrikeMark, value.Position, value.Direction, StrikeLife, value.Value, now);
                    else if (value.Kind == CombatEventKind.BodyLaunched && value.Cause.RootId != 0 && value.Weapon != null)
                    {
                        if (launchedBy.Count >= 64) launchedBy.Clear();
                        launchedBy[value.Cause.RootId] = value.Weapon; // the shot that flung (and may pin) this body
                    }
                }
            }
            if (!live) { marks.Clear(); launchedBy.Clear(); }
            marks.RemoveAll(m => now - m.Born > m.Life || now < m.Born);
            bool pinnedAlive = false;
            if (live) foreach (UnitState unit in simulation.Units) if (unit.Alive && unit.Faction == Faction.Enemy && unit.PinnedUntil > simulation.Elapsed) { pinnedAlive = true; break; }
            if (!live || simulation.Flung.Count == 0 && marks.Count == 0 && !pinnedAlive)
            {
                if (drawn) { ink.Begin(); ink.End(); drawn = false; }
                HideLabels(0);
                return;
            }
            if (ink == null) ink = new ArsenalInk(transform);
            ink.Begin(); drawn = true;
            bool still = PresentationSettings.ReducedMotion;
            int labels = 0;
            foreach (Mark m in marks)
            {
                float t = Mathf.Clamp01((now - m.Born) / m.Life), fade = 1 - t * t;
                Vector2 dir = m.Direction.sqrMagnitude > .001f ? m.Direction.normalized : Vector2.right;
                switch (m.Kind)
                {
                    case SplatMark: ink.Pool(m.At, m.Size * (still ? 1 : Mathf.Lerp(.6f, 1, Mathf.Clamp01(t * 6))), 1, Splat, m.At.x * 13, .8f * fade, .35f); break; // splat on the edge
                    case JoltMark: ink.Hit(ArsenalInk.Cream, InkToken.Star, m.At, dir, t, true, m.At.y * 7); break;                                                   // a bowled jolt
                    case SkidMark: // two scuffed skid strokes behind the shoved enemy
                    {
                        Vector2 across = new Vector2(-dir.y, dir.x) * .12f;
                        for (int s = -1; s <= 1; s += 2)
                            ink.FloorStroke(m.At - dir * (m.Size + .35f) + across * s, m.At - dir * .1f + across * s, Splat, .05f, 1, 0, .55f * fade, .02f, m.At.x + s);
                        // V11: a little inked dust kicked up where the shove began, drifting back and up as it thins
                        if (!still)
                            for (int k = 0; k < 3; k++)
                            {
                                Vector2 puff = m.At - dir * (m.Size + .3f) + across * (k - 1) * 1.6f + (-dir * .25f + Vector2.up * .3f) * t + Vector2.up * k * .05f;
                                ink.TokenAt(InkToken.Puff, ArsenalInk.Dust, puff, (.16f + .08f * k) * (.7f + .6f * t), k * 40, .5f * (1 - t));
                            }
                        break;
                    }
                    case StrikeMark:
                        if (labels < 3) DrawStrike(m, t, still, labels++);
                        break;
                }
            }
            HideLabels(labels);
            if (live) DrawPinnedEnemies(simulation, still);
            foreach (FlungBody body in simulation.Flung)
            {
                float p = body.Progress(simulation.Elapsed);
                Sprite sprite = Pose(body.Definition);
                if (sprite == null) continue;
                Bodies++;
                float lift = BodyLift(body, simulation.Elapsed, still), size = BodySize(body);
                Vector2 heading = body.Velocity.sqrMagnitude > .0001f ? body.Velocity.normalized : Vector2.right;
                if (body.Pinned)
                {
                    // pinned to the edge by a pointed shot: hanging still, a little tilted, the shot through it into the wall
                    PinnedBodies++;
                    Vector2 hang = body.Position + Vector2.up * size * .35f;
                    float alpha = 1 - Mathf.Clamp01((p - .85f) / .15f);
                    ink.Wash(body.Position, size * .4f, new Color(0, 0, 0, 1), .3f, false);
                    ink.Place(sprite, hang, size, heading.x < 0 ? 12 : -12, new Color(1, 1, 1, alpha), true);
                    launchedBy.TryGetValue(body.Cause.RootId, out WeaponDefinition shot);
                    Sprite through = RangedFeel.ShotSprite(shot);
                    float angle = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg;
                    if (through != null && shot != null)
                    {
                        float width = ProjectileVisual.Allied(shot.ProjectileVisualWidth), inkWidth = Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(through).width);
                        float boundsMax = Mathf.Max(through.bounds.size.x, through.bounds.size.y);
                        ink.Place(through, hang + heading * size * .15f, width * boundsMax / inkWidth, angle, new Color(1, 1, 1, alpha), true);
                    }
                    else ink.TokenAt(InkToken.Spear, ArsenalInk.Silver, hang + heading * size * .15f, .38f, angle, alpha);
                    ink.Stroke(hang + heading * size * .35f + new Vector2(-heading.y, heading.x) * .14f, hang + heading * size * .5f, Splat, .03f, 1, 0, .6f * alpha); // the crack it drove into the edge
                    continue;
                }
                ink.Wash(body.Position, size * .45f * (1 - lift * .2f), new Color(0, 0, 0, 1), .35f, false); // its shadow stays on the floor
                Vector2 at = body.Position + Vector2.up * (lift + size * .35f);
                if (!still && body.Velocity.sqrMagnitude > 1) ink.Streaks(at, -heading, .5f * (1 - p));
                // a body flung by a shot skims flat and tumbles less; one flung by a blow wheels high
                float spin = body.Spin * (simulation.Elapsed - body.Started) * (body.Low ? .35f : 1);
                if (body.Low && !still && body.Velocity.sqrMagnitude > .5f) ink.FloorStroke(body.Position - heading * .5f, body.Position - heading * .1f, Splat, .06f, 1, 0, .35f * (1 - p), .02f, body.UnitId);
                ink.Place(sprite, at, size, still ? 0 : spin, new Color(1, 1, 1, 1 - Mathf.Clamp01((p - .85f) / .15f)), true);
            }
            ink.End();
        }

        private void DrawPinnedEnemies(CombatSimulation simulation, bool still)
        {
            foreach (UnitState unit in simulation.Units)
            {
                if (!unit.Alive || unit.Faction != Faction.Enemy || unit.PinnedUntil <= simulation.Elapsed) continue;
                PinnedEnemies++;
                // strain strokes on its far side, toward the edge it is pressed against
                Vector2 into = unit.HitDirection.sqrMagnitude > .001f ? unit.HitDirection.normalized : Vector2.right, across = new Vector2(-into.y, into.x);
                float r = Mathf.Max(.2f, unit.Definition.Radius), left = Mathf.Clamp01((unit.PinnedUntil - simulation.Elapsed) / ImpactRules.PinSeconds);
                Vector2 at = UnitVisual.TryDrawn(unit.Id, out Vector2 drawnAt) ? drawnAt : unit.Position;
                for (int s = -1; s <= 1; s++)
                {
                    Vector2 from = at + into * r * .9f + across * (s * r * .45f);
                    ink.Stroke(from, from + into * .18f + across * (s * .06f), Splat, .03f, 1, 0, .7f * left);
                }
                if (!still) ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, at + into * r + Vector2.up * .2f, .16f, 0, .8f * left);
            }
        }

        private void DrawStrike(Mark m, float t, bool still, int slot)
        {
            // STRIKE!: a gold ink burst with coins knocked loose, and the word stamped over it, popping then lifting away
            ink.Hit(ArsenalInk.Gold, InkToken.Coin, m.At, Vector2.up, Mathf.Clamp01(t * 1.6f), true, m.At.x * 11);
            ink.Light(m.At + Vector2.up * .5f, 1.4f * (1 - t), ArsenalInk.Gold.Glow, .35f * (1 - t));
            while (strikeLabels.Count <= slot)
            {
                InkedText created = InkedText.Create(transform, "StrikeLabel", ArcadeTypography.Punch, 64, .05f, 2450, .035f);
                strikeLabels.Add(created);
            }
            InkedText label = strikeLabels[slot];
            label.gameObject.SetActive(true);
            label.Text = "STRIKE!";
            float pop = still ? 1 : t < .12f ? Mathf.Lerp(1.6f, 1, t / .12f) : 1;
            label.Color = new Color(1f, .86f, .32f, 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - .65f) / .35f)));
            label.transform.position = new Vector3(m.At.x, m.At.y + .9f + (still ? 0 : t * .5f), -2f);
            label.transform.localScale = Vector3.one * pop * (1 + .08f * Mathf.Min(3, m.Size - 3));
            label.transform.rotation = Quaternion.Euler(0, 0, still ? 0 : -6);
            Strikes++;
        }
        private void HideLabels(int from) { for (int i = from; i < strikeLabels.Count; i++) if (strikeLabels[i] != null) strikeLabels[i].gameObject.SetActive(false); }

        /// <summary>The hurt pose when the body has painted frames, else its standing drawing.</summary>
        private static Sprite Pose(UnitDefinition unit)
        {
            if (unit == null) return null;
            AuthoredMotionSet motion = unit.AuthoredMotion;
            if (motion != null && motion.Recovery != null && motion.Recovery.Length > 0 && motion.Recovery[0] != null) return motion.Recovery[0];
            return unit.Body;
        }
        private void Add(int kind, Vector2 at, Vector2 direction, float life, float size, float now)
        {
            if (marks.Count >= 32)
            {
                int ordinary = marks.FindIndex(m => m.Kind != StrikeMark); // a STRIKE! outlives the splats around it
                marks.RemoveAt(ordinary >= 0 ? ordinary : 0);
            }
            marks.Add(new Mark { Kind = kind, At = at, Direction = direction, Born = now, Life = life, Size = size });
        }
        private void OnDestroy() { if (Application.isPlaying) ink?.Destroy(); }
    }
}
