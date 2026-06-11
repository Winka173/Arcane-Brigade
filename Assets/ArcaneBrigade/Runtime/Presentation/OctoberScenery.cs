using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class OctoberScenery : MonoBehaviour
    {
        private ArenaController owner;
        private OctoberArtwork art;
        private readonly SpriteRenderer[] eyes = new SpriteRenderer[8];
        public void Configure(ArenaController controller)
        {
            owner = controller; art = OctoberArtwork.Current;
            if (art == null) return;
            for (int i = 0; i < eyes.Length; i++)
            {
                var go = new GameObject("PaintedNightWatcher" + i); go.transform.SetParent(transform, false);
                eyes[i] = go.AddComponent<SpriteRenderer>(); eyes[i].sortingOrder = -38;
            }
        }
        private void LateUpdate()
        {
            if (art == null || owner.Session == null || owner.Simulation == null) return;
            bool visible = owner.Session.State.Wave >= 11 && !owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible;
            Vector2 half = owner.WorldBounds * .5f; float t = owner.PresentationClock;
            for (int i = 0; i < eyes.Length; i++)
            {
                SpriteRenderer eye = eyes[i]; eye.enabled = visible; if (!visible) continue;
                // the shop shows only things that can be used: no watchers there
                if (owner.Session.State.Phase == SessionPhase.Shop && !owner.Session.SecretRoomOpen) { eye.enabled = false; continue; }
                // Watchers belong to the rim and never cover the central combat lane; the arena layout keeps them off every prop.
                Vector2 ink = ArenaLayoutPlan.WatcherInk(i), foot = ArenaLayoutPlan.WatcherCenter(i, half) - Vector2.up * ink.y * .5f;
                if (!ArenaLayout.Place(owner, "watcher" + i, ref foot, out _)) { eye.enabled = false; continue; }
                Vector2 position = foot + Vector2.up * ink.y * .5f;
                float age = Mathf.Repeat(t * .45f + i * 1.19f, 8);
                int frame = PresentationSettings.ReducedMotion ? 2 : age < .5f ? 0 : age < .8f ? 1 : owner.Simulation.Champion.Position.x < position.x ? 2 : 3;
                eye.sprite = art.Watchers[(i % 3) * 4 + frame];
                float width = ArenaLayoutPlan.WatcherWidth(i);
                eye.transform.position = position;
                eye.transform.localScale = Vector3.one * (width / Mathf.Max(.01f, eye.sprite.bounds.size.x));
                eye.color = new Color(.38f, .52f, .7f, .4f); // Distant witnesses are blue; actual hostile eyes are red.
            }
        }
    }
    public sealed class OctoberDoor : MonoBehaviour
    {
        /// <summary>The next night's gate: an upright circle of runes hovering where the door stood. Two broken rings turn
        /// against each other with rune ticks and glyphs between them; as the hero comes near it turns faster, brightens and a
        /// swirl of light opens at its heart. Friendly colours (amber, cream, teal); never a flat ring or a filled disc.</summary>
        private void MagicCircle(Vector3 foot, int order, float alpha, bool near)
        {
            if (circleHolder == null)
            {
                circleHolder = new GameObject("Next-night magic circle");
                circleInk = new InkCanvas(circleHolder.transform, "Gate ink", order);
                circleLight = new InkCanvas(circleHolder.transform, "Gate light", order + 2, true);
            }
            circleInk.SetOrder(order); circleLight.SetOrder(order + 2);
            circleInk.Clear(); circleLight.Clear();
            bool reduced = PresentationSettings.ReducedMotion;
            float t = owner.PresentationClock, open = Mathf.SmoothStep(0, 1, opening), r = .7f;
            Vector2 c = (Vector2)foot + Vector2.up * .9f; // between the middle stall's price tag and the NEXT WAVE label
            float spin = reduced ? 0 : t * (18 + 50 * open), a = alpha;
            InkPalette gold = ArsenalInk.Gold, teal = ArsenalInk.Water;
            // the outer ring: six rune-bars with gaps, turning one way
            for (int i = 0; i < 6; i++)
            {
                float from = spin + i * 60 + 6, to = from + 46;
                circleInk.Stroke(InkCanvas.Arc(c, r, r, from, to, 14), InkStroke.Of(gold, .13f, a, i));
            }
            // rune ticks just inside it, and eight little glyphs on the inner ring
            circleLight.Ticks(c, r - .13f, 24, .09f, .035f, -spin * .5f, new Color(1f, .93f, .72f, (.55f + .35f * open) * a)); // rune ticks in light: ink vanished on the dark floor
            for (int i = 0; i < 4; i++)
            {
                float from = -spin * 1.4f + i * 90 + 12, to = from + 66;
                circleInk.Stroke(InkCanvas.Arc(c, r * .68f, r * .68f, from, to, 12), InkStroke.Of(teal, .09f, a, 10 + i));
            }
            for (int i = 0; i < 8; i++)
            {
                float ang = (-spin * 1.4f + i * 45 + 33) * Mathf.Deg2Rad;
                Vector2 p = c + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r * .84f, tangent = new Vector2(-Mathf.Sin(ang), Mathf.Cos(ang));
                circleInk.Stroke(InkCanvas.Line(p - tangent * .08f, p + tangent * .08f, .03f, 3, true, i), InkStroke.Of(ArsenalInk.Water, .055f, a, 20 + i));
            }
            // the heart: three arms of light turning in as it opens (light only, no filled disc)
            open = Mathf.Max(open, .3f); // a faint turning heart even from afar
            for (int k = 0; k < 3; k++)
                {
                    var path = new System.Collections.Generic.List<Vector2>();
                    for (int s = 0; s <= 16; s++)
                    {
                        float f = s / 16f, ang = (spin * 2.2f + k * 120 + f * 220) * Mathf.Deg2Rad;
                        path.Add(c + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r * .62f * (1 - f));
                    }
                    circleLight.GlowRibbon(path, 1, 0, .09f + .06f * open, new Color(1f, .86f, .55f, .75f * open * alpha));
                }
            // a soft breathing glow along the outer ring's bars (stronger when near)
            float breath = reduced ? .5f : .5f + .5f * Mathf.Sin(t * 2.4f);
            for (int i = 0; i < 6; i++)
            {
                float from = spin + i * 60 + 10, to = from + 38;
                circleLight.GlowRibbon(InkCanvas.Arc(c, r, r, from, to, 10), .8f, .8f, .16f, new Color(1f, .8f, .45f, (.3f + .4f * open) * (.7f + .3f * breath) * alpha));
            }
            circleInk.Commit(); circleLight.Commit();
        }

        private ArenaController owner; private SpriteRenderer view; private float opening;
        private static PaintedAnimationLibrary exitDoor;
        private GroundShadow grounded;
        private MarketLifecycle lifecycle;
        private DreamSuspension suspension;
        // the way on is a standing magic circle drawn in ink (requirement 24/10/2026: not a door), the old door art hidden
        private GameObject circleHolder; private InkCanvas circleInk, circleLight;
        private void OnDestroy() { if (circleHolder != null) Destroy(circleHolder); }
        public void Configure(ArenaController controller, SpriteRenderer renderer)
        {
            owner = controller; view = renderer;
            grounded = GetComponent<GroundShadow>(); if (grounded == null) grounded = gameObject.AddComponent<GroundShadow>();
            grounded.PinCastToFoot = true;
            // The door owns its pose and ground anchor. The generic shop actor must not overwrite them.
            GetComponent<ShopDisplayActor>().enabled = false;
            suspension = gameObject.AddComponent<DreamSuspension>();
        }
        private void LateUpdate()
        {
            var art = OctoberArtwork.Current; if (owner == null || art == null || view == null) return;
            bool near = Vector2.Distance(owner.Simulation.Champion.Position, transform.parent.position) < 2;
            if (!owner.PresentationPaused) opening = Mathf.MoveTowards(opening, near || owner.HallExitPending ? 1 : 0, Time.deltaTime * 2.5f);
            int frame = PresentationSettings.ReducedMotion ? near ? 2 : 0 : opening < .25f ? 0 : opening < .7f ? 1 : 2 + (Mathf.Sin(owner.PresentationClock * 2) > .6f ? 1 : 0);
            view.sprite = art.Door[frame];
            // the new painted exit (requirement 23/10/2026): closed far away, open with its lamp lit as the hero comes near
            exitDoor ??= Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            bool painted = exitDoor != null && exitDoor.HallDoor != null && exitDoor.HallDoor.Length == 2 && exitDoor.HallDoor[0] != null && exitDoor.HallDoor[0].texture.name == "Shop_ExitDoor";
            if (painted) view.sprite = exitDoor.HallDoor[opening < .5f ? 0 : 1];
            // 1.4 units tall (the new arched door 1.7): the arch stays under the middle stall's pedestal instead of rising into it
            float scale = (painted ? 1.7f : 1.4f) / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(view.sprite).height);
            transform.localScale = Vector3.one * scale;
            transform.rotation = Quaternion.Euler(0, 0, DreamLevitation.Tilt(owner.PresentationClock, 41, PresentationSettings.ReducedMotion, .45f));
            // Keep the visible foot suspended relative to the fixed threshold after frame changes.
            if (lifecycle == null) lifecycle = GetComponentInParent<MarketLifecycle>();
            Vector3 offset = lifecycle == null ? Vector3.zero : transform.parent.TransformVector(lifecycle.FixtureOffset);
            Vector3 hovering = DreamLevitation.Offset(owner.PresentationClock, 41, NightmareRules.Depth(owner.Session.State.Wave), PresentationSettings.ReducedMotion, .75f);
            WorldSpriteGrounding.Place(view, transform.parent.position + offset + hovering);
            view.sortingOrder = WorldSpriteGrounding.SortAt(transform.parent.position.y);
            float alpha = lifecycle == null ? 1 : lifecycle.FixtureVisibility;
            view.color = new Color(1, 1, 1, 0); // the door's art stays only as the gate's place
            grounded.Display(view.sprite, transform.parent.position, 1.35f, Mathf.Abs(offset.y) + hovering.y, 0);
            MagicCircle(transform.parent.position + offset + hovering, view.sortingOrder, alpha, near || owner.HallExitPending);
        }
    }
}
