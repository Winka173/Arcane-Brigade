using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws the bosses' own moves (BossTrademarks): during the wind-up the path is inked on the floor (a lane, an arc,
    /// an oval belt, a row of marks), then the piece itself travels - painted BossProp_* art when it exists, an inked token
    /// until then. Bars, sparks and the spotlight are ink and light only. Enemy colours (wine, bile, ember), never our lantern.</summary>
    public sealed class BossTrademarkVisual : MonoBehaviour
    {
        private ArsenalInk ink;
        private bool drawn;
        private readonly List<Vector2> path = new List<Vector2>(48);
        private static readonly InkPalette Bile = new InkPalette(new Color(.2f, .04f, .1f), new Color(.62f, .14f, .32f), new Color(.98f, .68f, .76f), new Color(.95f, .35f, .45f), new Color(.98f, .72f, .35f));
        private static readonly InkPalette Wax = new InkPalette(new Color(.25f, .1f, .08f), new Color(.92f, .7f, .45f), new Color(1, .94f, .8f), new Color(1, .7f, .45f), new Color(.6f, .14f, .2f));
        private static readonly InkPalette RedactInk = new InkPalette(new Color(.05f, .02f, .05f), new Color(.2f, .05f, .12f), new Color(.6f, .2f, .3f), new Color(.5f, .1f, .2f), new Color(.9f, .3f, .3f));
        // what the last frame showed (read by checks)
        public int Paths { get; private set; }
        public int Pieces { get; private set; }

        public void Display(CombatSimulation simulation)
        {
            Paths = Pieces = 0;
            bool any = false;
            if (simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused))
                foreach (ExpandedField f in simulation.ExpandedFields) if (f.Kind == ExpandedFieldKind.Roller) { any = true; break; }
            if (!any) { if (drawn) { ink.Begin(); ink.End(); drawn = false; } return; }
            if (ink == null) ink = new ArsenalInk(transform);
            ink.Begin(); drawn = true;
            foreach (ExpandedField f in simulation.ExpandedFields) if (f.Kind == ExpandedFieldKind.Roller) Draw(f, simulation.Elapsed);
            ink.End();
        }

        private void Draw(ExpandedField f, float now)
        {
            bool still = PresentationSettings.ReducedMotion;
            float lead = Mathf.Max(.05f, f.At - f.Created), charge = Mathf.Clamp01((now - f.Created) / lead);
            bool warning = now < f.At;
            float fade = Mathf.Clamp01((f.Until - now) / .35f) * (f.Resolved && f.Damage > 0 ? .55f : 1);
            float seed = f.Style * 41 + f.Start.x * 7;
            // the path, inked over the floor while the boss winds up (and kept faint while the piece runs it)
            float pathAlpha = warning ? Mathf.SmoothStep(0, 1, charge) * .8f : .28f * fade;
            DrawPath(f, pathAlpha, seed);
            Paths++;
            if (warning && f.Motion != TrademarkMotion.Arc && f.Motion != TrademarkMotion.Orbit && f.Motion != TrademarkMotion.Home && f.Motion != TrademarkMotion.Rise) return;
            float alpha = warning ? Mathf.SmoothStep(0, 1, charge) * .6f : fade;
            Vector2 at = f.Position;
            float t = Mathf.Max(0, now - f.At);
            // painted frames: the sheep's hop animates; each suitcase on the belt keeps its own look
            Sprite[] frames = BossTrademarks.Prop(f.Style) is string name ? ArsenalArtwork.Frames(name) : null;
            Sprite painted = frames == null ? null : f.Style == 9 ? frames[Mathf.Abs(Mathf.RoundToInt(f.Phase * 10)) % frames.Length]
                : still ? frames[0] : frames[Mathf.Abs(Mathf.FloorToInt(now * 6)) % frames.Length];
            Pieces++;
            switch (f.Style)
            {
                case 0: Piece(painted, InkToken.Envelope, Bile, at, .9f, still ? 0 : now * 540, alpha); break; // a letter spinning on its way
                case 1:
                {
                    BossTrademarks.Bar(f, out Vector2 a, out Vector2 b);
                    ink.FloorStroke(a, b, RedactInk, f.Radius * 2.2f, 1, 0, alpha * .95f, still ? 0 : .04f, seed);
                    ink.Stroke(a + Vector2.up * .05f, b + Vector2.up * .05f, Bile, f.Radius * .5f, 1, 0, alpha * .7f);
                    break;
                }
                case 2: // a sheep, hopping as it runs
                {
                    float hop = still ? 0 : Mathf.Abs(Mathf.Sin(t * 9 + f.Start.x)) * .32f;
                    Piece(painted, InkToken.Puff, ArsenalInk.Cotton, at + Vector2.up * hop, 1.05f, f.Direction.x < 0 ? 180 : 0, alpha, f.Direction.x < 0);
                    break;
                }
                case 3: // a crest of melting wax, the bar under it dripping
                {
                    BossTrademarks.Bar(f, out Vector2 a, out Vector2 b);
                    ink.FloorStroke(a, b, Wax, f.Radius * 2.4f, 1, 0, alpha, still ? 0 : .06f, seed);
                    if (painted != null) ink.Place(painted, at + Vector2.up * .25f, f.Length * 1.05f, 0, new Color(1, 1, 1, alpha), true);
                    else for (int i = 0; i < 3; i++) ink.TokenAt(InkToken.Drop, Wax, Vector2.Lerp(a, b, (i + .5f) / 3) + Vector2.up * (.3f + (still ? 0 : Mathf.Sin(now * 5 + i) * .08f)), .38f, 0, alpha);
                    break;
                }
                case 4: // a spark running along its fuse
                    ink.Stroke(f.Start, at, Bile, .05f, 1, 0, .35f * alpha);
                    ink.Light(at, .9f, new Color(1, .55f, .3f), .6f * alpha);
                    ink.TokenAt(InkToken.Spark, Bile, at, .45f, still ? 0 : now * 900, alpha);
                    break;
                case 5: // the stage light: a pale wash on the floor, brighter as the drop comes
                {
                    float left = Mathf.Clamp01((f.At + f.Phase - now) / f.Phase);
                    if (warning) BossTelegraph.Rim(ink, at, f.Radius + .1f, f.Radius + .1f, charge, seed, .9f); // where the light will drop: its solid ground and rim
                    else ink.Wash(at, f.Radius * 1.5f, new Color(1, .93f, .75f), (.42f + (1 - left) * .25f) * fade, false);
                    ink.Light(at + Vector2.up * .2f, f.Radius * 2.2f, new Color(1, .9f, .7f), .3f * fade);
                    if (!warning && !f.Spent) ink.FloorStroke(at + new Vector2(-.35f, -.1f), at + new Vector2(.35f, -.1f), Bile, .07f, 1, 0, (1 - left) * fade);
                    break;
                }
                case 6: // the pendulum: a cord from its pivot and a heavy shadow weight
                    ink.Cord(f.Center, at, Bile, .07f, false, alpha);
                    Piece(painted, InkToken.Moon, ArsenalInk.Midnight, at, 1.55f, Vector2.SignedAngle(Vector2.down, at - f.Center), alpha);
                    break;
                case 7: Piece(painted, InkToken.Coin, ArsenalInk.Gold, at, .95f, still ? 0 : -t * 400 * Mathf.Sign(f.Direction.x == 0 ? 1 : f.Direction.x), alpha); break; // a coin rolling
                case 8: // an organ pipe rising out of the floor, then sinking
                {
                    float life = Mathf.Clamp01((now - f.At) / Mathf.Max(.05f, f.Until - f.At));
                    float rise = warning ? 0 : Mathf.Sin(life * Mathf.PI);
                    Cross(at, .4f, .55f * (warning ? charge : 1), seed);
                    if (rise <= 0) break;
                    if (painted != null) ink.Place(painted, at + Vector2.up * rise * .7f, 1.5f * rise, 0, new Color(1, 1, 1, alpha), true);
                    else { ink.Stroke(at, at + Vector2.up * 1.4f * rise, Wax, .34f, 1, 0, alpha); ink.TokenAt(InkToken.Note, Bile, at + Vector2.up * (1.4f * rise + .2f), .42f, 0, alpha * rise); }
                    break;
                }
                case 9: Piece(painted, InkToken.Patch, ArsenalInk.Leather, at, 1.05f, still ? 0 : Mathf.Sin(now * 6 + f.Phase) * 6, alpha); break; // a suitcase on the belt
            }
        }

        private void Piece(Sprite painted, InkToken token, InkPalette palette, Vector2 at, float size, float angle, float alpha, bool flip = false)
        {
            if (painted != null) ink.Place(painted, at, size, flip ? 0 : angle, new Color(1, 1, 1, alpha), true, 1, flip);
            else ink.TokenAt(token, palette, at, size * .8f, angle, alpha);
        }

        private void DrawPath(ExpandedField f, float alpha, float seed)
        {
            if (alpha <= .01f) return;
            switch (f.Motion)
            {
                case TrademarkMotion.Straight:
                case TrademarkMotion.Return:
                case TrademarkMotion.Fuse:
                case TrademarkMotion.Bounce:
                {
                    float reach = f.Motion == TrademarkMotion.Bounce ? f.Speed * .9f : f.Motion == TrademarkMotion.Fuse || f.Motion == TrademarkMotion.Return ? f.Speed * f.Phase : f.Speed * (f.Until - f.At);
                    Vector2 end = f.Start + f.Direction * reach;
                    if (f.Length > 0)
                    {
                        // a bar: inked where it starts and a light lane where its open side will pass
                        Vector2 across = new Vector2(-f.Direction.y, f.Direction.x) * f.Length * .5f;
                        BossTelegraph.LaneGround(ink, f.Start, end, f.Length * .5f, Mathf.Clamp01(alpha / .8f), alpha);

                    }
                    else if (f.Motion == TrademarkMotion.Fuse) ink.FloorStroke(f.Start, end, Wax, .07f, 1, 0, alpha, .05f, seed);
                    else
                    {
                        // the piece's lane: two red inked edges and an arrowhead, outline only (play-test item 22)
                        Vector2 n = new Vector2(-f.Direction.y, f.Direction.x) * (f.Radius + .12f);
                        float charge = Mathf.Clamp01(alpha / .8f);
                        BossTelegraph.LaneGround(ink, f.Start, end, f.Radius + .12f, charge, alpha);

                        BossTelegraph.Arrow(ink, end, f.Direction, f.Radius + .35f, charge, alpha);
                    }
                    break;
                }
                case TrademarkMotion.Home:
                    Cross(f.Start, .45f, alpha * .6f, seed);
                    break;
                case TrademarkMotion.Rise:
                    Cross(f.Start, .38f, alpha * .7f, seed);
                    break;
                case TrademarkMotion.Arc:
                case TrademarkMotion.Orbit:
                {
                    path.Clear();
                    int samples = f.Motion == TrademarkMotion.Arc ? 18 : 36;
                    for (int i = 0; i <= samples; i++)
                    {
                        float u = i / (float)samples;
                        Vector2 p = f.Motion == TrademarkMotion.Arc
                            ? f.Center + new Vector2(Mathf.Sin(Mathf.Lerp(-Mathf.Abs(f.Phase), Mathf.Abs(f.Phase), u)), -Mathf.Cos(Mathf.Lerp(-Mathf.Abs(f.Phase), Mathf.Abs(f.Phase), u))) * f.Reach
                            : f.Center + new Vector2(Mathf.Cos(u * Mathf.PI * 2) * f.Reach, Mathf.Sin(u * Mathf.PI * 2) * f.Reach * .62f);
                        path.Add(p);
                    }
                    for (int i = 0; i + 1 < path.Count; i += 2) ink.FloorStroke(path[i], path[i + 1], BossTelegraph.Danger, .09f, 1, 0, alpha * .8f, 0, seed + i);
                    break;
                }
            }
        }

        /// <summary>An inked X on the floor (never a pool).</summary>
        private void Cross(Vector2 at, float arm, float alpha, float seed)
        {
            ink.FloorStroke(at + new Vector2(-arm, -arm * .5f), at + new Vector2(arm, arm * .5f), BossTelegraph.Danger, .09f, 1, 0, alpha, .03f, seed);
            ink.FloorStroke(at + new Vector2(-arm, arm * .5f), at + new Vector2(arm, -arm * .5f), BossTelegraph.Danger, .09f, 1, 0, alpha, .03f, seed + 1);
        }
        private void OnDestroy() { ink?.Destroy(); }
    }
}
