using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Keeps the hero easy to find (play-test 20/10/2026, items 6 and 14; setting "Highlight hero", on by default):
    /// a painted lantern halo under the hero's feet in the hero's colour; props drawn in front of the hero fade to 35% while
    /// they cover it, and friends in front of it to 70%; when something still covers the hero, a faint copy of the hero shows
    /// through; and if the hero ever leaves the screen, an ink arrow at the edge points to it. The hero is also drawn a little
    /// larger and above nearby friends (K8Motion.BodyHeight, UnitVisual sort bias). Reduced Motion: fades are instant.</summary>
    [DefaultExecutionOrder(1000)] // apply after actors have updated their tint and portrait pieces
    public sealed class HeroFocus : MonoBehaviour
    {
        public const int SortBias = 12;          // one world unit of Y-sort: the hero draws over friends standing just in front
        public const float HeroScale = 1f;       // requirement 24/10/2026: the hero is no longer drawn larger; its friends' size, by shape (UnitVisual)
        public const float PropAlpha = .35f, FriendAlpha = .7f, FadeSeconds = .15f;
        private const int RingOrderOffset = -4, ShowThroughOrder = 1650, ArrowOrder = 2150;

        private sealed class Fade { public float Amount = 1, Target = 1, Base = 1, Written = -1; }
        private ArenaController owner;
        private InkCanvas ring, arrow;
        private SpriteRenderer showThrough;
        private float showThroughAlpha;
        private readonly List<SpriteRenderer> props = new List<SpriteRenderer>();
        private readonly Dictionary<SpriteRenderer, bool> classified = new Dictionary<SpriteRenderer, bool>();
        private readonly Dictionary<SpriteRenderer, Fade> fades = new Dictionary<SpriteRenderer, Fade>();
        private readonly List<SpriteRenderer> finished = new List<SpriteRenderer>();
        private float nextScan, nextForget;
        /// <summary>For checks: renderers currently faded (props and friends), and whether the hero shows through.</summary>
        public int Faded { get; private set; }
        public bool ShowingThrough => showThroughAlpha > .01f;
        /// <summary>0..1: how far the hero is hidden behind a prop or friend right now; the hero's gauge fades with it.</summary>
        public static float Covered { get; private set; }
        public bool RingVisible { get; private set; }

        public void Configure(ArenaController controller) { owner = controller; }

        private void EnsureParts()
        {
            if (ring != null) return;
            var root = new GameObject("Hero focus"); root.transform.SetParent(transform, false);
            ring = new InkCanvas(root.transform, "Hero halo", 900);
            arrow = new InkCanvas(root.transform, "Hero arrow", ArrowOrder);
            var ghost = new GameObject("Hero show-through"); ghost.transform.SetParent(root.transform, false);
            showThrough = ghost.AddComponent<SpriteRenderer>(); showThrough.sortingOrder = ShowThroughOrder; showThrough.enabled = false;
        }

        private void LateUpdate()
        {
            CombatSimulation simulation = owner != null ? owner.Simulation : null;
            UnitState hero = simulation?.Champion;
            UnitVisual view = null;
            bool on = PresentationSettings.HighlightHero && hero != null && hero.Alive && owner.TryView(hero.Id, out view) && view != null && view.isActiveAndEnabled && view.BodyRenderer != null && view.BodyRenderer.enabled;
            EnsureParts();
            if (!on) { Clear(); return; }
            SpriteRenderer body = view.BodyRenderer;
            Bounds covered = body.bounds; covered.extents = new Vector3(covered.extents.x * .8f, covered.extents.y * .9f, 1000);
            float delta = Time.unscaledDeltaTime;

            // props in front of the hero that cover it fade; friends in front fade a little
            if (Time.unscaledTime >= nextScan) Scan();
            foreach (var pair in fades) pair.Value.Target = 1;
            int heroOrder = body.sortingOrder, layer = body.sortingLayerID;
            bool occluded = false;
            // Shop wares stay readable; the merchant may fade when its body hides the hero.
            bool inShop = owner.Session != null && owner.Session.State.Phase == SessionPhase.Shop;
            foreach (SpriteRenderer r in props)
            {
                if (inShop && !IsShopkeeper(r)) continue;
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy || r.sortingLayerID != layer || r.sortingOrder <= heroOrder || r.sortingOrder >= 1600) continue;
                Vector3 size = r.bounds.size;
                if (size.x < .45f && size.y < .45f || size.x > 14 || size.y > 14 || r.color.a < .05f) continue;
                Bounds b = r.bounds; b.extents = new Vector3(b.extents.x, b.extents.y, 1000);
                if (!b.Intersects(covered)) continue;
                if (IsShopkeeper(r))
                {
                    // The animated portrait is cut in two. Fade the whole portrait together to avoid a visible seam.
                    foreach (SpriteRenderer piece in r.GetComponentInParent<ShopDisplayActor>().GetComponentsInChildren<SpriteRenderer>())
                        if (piece.enabled && piece.GetComponent<GroundShadow>() == null) Target(piece, PropAlpha);
                }
                else Target(r, PropAlpha);
                occluded = true;
            }
            // a chest that lands on a piece of the room stays in view: whatever prop it overlaps fades like one over the hero
            if (!inShop)
                foreach (PickupState loot in simulation.Pickups)
                {
                    if (loot.Kind != LootKind.Chest) continue;
                    var chest = new Bounds((Vector3)(loot.Position + Vector2.up * .35f), new Vector3(1f, .9f, 2000));
                    foreach (SpriteRenderer r in props)
                    {
                        if (r == null || !r.enabled || !r.gameObject.activeInHierarchy || r.sortingLayerID != layer || r.sortingOrder >= 1600 || r.color.a < .05f) continue;
                        Vector3 size = r.bounds.size;
                        if (size.x < .45f && size.y < .45f || size.x > 14 || size.y > 14) continue;
                        Bounds b = r.bounds; b.extents = new Vector3(b.extents.x, b.extents.y, 1000);
                        if (b.Intersects(chest)) Target(r, PropAlpha);
                    }
                }
            foreach (UnitVisual friend in owner.Views)
            {
                if (friend == null || friend == view || friend.State == null || friend.State.Faction != Faction.Companion || !friend.State.Alive) continue;
                SpriteRenderer fb = friend.BodyRenderer;
                if (fb == null || !fb.enabled || fb.sortingOrder <= heroOrder) continue;
                Bounds b = fb.bounds; b.extents = new Vector3(b.extents.x, b.extents.y, 1000);
                if (!b.Intersects(covered)) continue;
                Target(fb, FriendAlpha); occluded = true;
            }
            ApplyFades(delta);

            // a faint copy of the hero shows through whatever still covers it
            float want = occluded ? .32f : 0;
            showThroughAlpha = PresentationSettings.ReducedMotion ? want : Mathf.MoveTowards(showThroughAlpha, want, delta / FadeSeconds * .32f);
            showThrough.enabled = showThroughAlpha > .01f; Covered = showThroughAlpha / .32f;
            Color tint = HeroColour(hero);
            if (showThrough.enabled)
            {
                showThrough.sprite = body.sprite; showThrough.flipX = body.flipX; showThrough.sortingLayerID = layer;
                showThrough.transform.SetPositionAndRotation(body.transform.position, body.transform.rotation);
                showThrough.transform.localScale = body.transform.lossyScale;
                showThrough.color = new Color(Mathf.Lerp(tint.r, 1, .45f), Mathf.Lerp(tint.g, 1, .45f), Mathf.Lerp(tint.b, 1, .45f), showThroughAlpha);
            }

            PaintRing(hero, view, body, tint);
            PaintArrow(hero, view, tint);
        }

        private void Target(SpriteRenderer r, float alpha)
        {
            if (!fades.TryGetValue(r, out Fade f)) fades[r] = f = new Fade();
            f.Target = Mathf.Min(f.Target, alpha);
        }

        public static bool IsShopkeeper(SpriteRenderer renderer)
        {
            var actor = renderer != null ? renderer.GetComponentInParent<ShopDisplayActor>() : null;
            return actor != null && actor.IsMerchant;
        }

        private void ApplyFades(float delta)
        {
            Faded = 0; finished.Clear();
            float step = PresentationSettings.ReducedMotion ? 1 : delta / FadeSeconds;
            foreach (var pair in fades)
            {
                SpriteRenderer r = pair.Key; Fade f = pair.Value;
                if (r == null) { finished.Add(r); continue; }
                Color c = r.color;
                // another script set the colour since we last wrote it: that is the new base alpha
                if (f.Written < 0 || Mathf.Abs(c.a - f.Written) > .002f) f.Base = c.a;
                f.Amount = Mathf.MoveTowards(f.Amount, f.Target, step * (1 - Mathf.Min(f.Target, PropAlpha)));
                if (f.Target >= 1 && f.Amount >= .999f) { c.a = f.Base; r.color = c; finished.Add(r); continue; }
                c.a = f.Base * f.Amount; r.color = c; f.Written = c.a;
                if (f.Amount < .999f) Faded++;
            }
            foreach (SpriteRenderer r in finished) fades.Remove(r);
        }

        private void Scan()
        {
            nextScan = Time.unscaledTime + .5f;
            if (Time.unscaledTime >= nextForget) { classified.Clear(); nextForget = Time.unscaledTime + 10; }
            props.Clear();
            foreach (SpriteRenderer r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (!classified.TryGetValue(r, out bool prop))
                {
                    prop = r != showThrough && r.GetComponentInParent<UnitVisual>() == null && r.GetComponentInParent<CoinVisual>() == null && r.GetComponentInParent<ProjectileVisual>() == null;
                    classified[r] = prop;
                }
                if (prop) props.Add(r);
            }
        }

        private static Color HeroColour(UnitState hero)
        {
            HeroIdentityDefinition identity = HeroIdentities.For(hero.Definition.Id);
            return identity == null || identity.Color.a <= 0 ? ArcadeSkin.Gold : identity.Color;
        }

        /// <summary>The lantern halo: a soft glow ring and a broken, hand-inked rim on the floor, brighter while the hero moves.</summary>
        private void PaintRing(UnitState hero, UnitVisual view, SpriteRenderer body, Color tint)
        {
            ring.SetOrder(body.sortingOrder + RingOrderOffset);
            ring.Clear();
            Vector2 feet = view.transform.position;
            float width = Mathf.Clamp(K8Motion.BodyHeight(hero) * .55f, .38f, .9f), height = width * .36f;
            bool moving = (hero.Position - hero.PreviousPosition).sqrMagnitude > .00001f;
            float t = PresentationSettings.ReducedMotion ? 0 : Time.time;
            float glow = (moving ? .62f : .42f) + (PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(t * 2.4f) * .05f);
            Color light = Color.Lerp(tint, new Color(1, .93f, .7f), .45f);
            ring.GlowRibbon(InkCanvas.Arc(feet, width * 1.04f, height * 1.04f, 0, 360, 40), 1, 0, .16f, new Color(light.r, light.g, light.b, glow * .55f));
            var palette = new InkPalette(new Color(tint.r * .25f, tint.g * .22f, tint.b * .25f), Color.Lerp(tint, Color.white, .15f), Color.Lerp(tint, Color.white, .7f), light, ArcadeSkin.Gold);
            float spin = PresentationSettings.ReducedMotion ? 0 : t * 22;
            // two brush strokes that leave small gaps: a rim drawn by hand, not a plain ellipse
            for (int arc = 0; arc < 2; arc++)
            {
                float from = spin + arc * 180 + 12, to = from + 156;
                InkStroke s = InkStroke.Of(palette, .045f, moving ? .95f : .8f, arc * .37f);
                s.Taper = .6f;
                ring.Stroke(InkCanvas.Arc(feet, width, height, from, to, 22), s);
            }
            ring.Commit();
            RingVisible = true;
        }

        /// <summary>An ink arrow at the screen edge that points to the hero when the hero is off screen.</summary>
        private void PaintArrow(UnitState hero, UnitVisual view, Color tint)
        {
            arrow.Clear();
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic)
            {
                Vector2 at = view.transform.position + Vector3.up * K8Motion.BodyHeight(hero) * .5f;
                Vector2 centre = camera.transform.position;
                float halfH = camera.orthographicSize, halfW = halfH * camera.aspect;
                Vector2 offset = at - centre;
                if (Mathf.Abs(offset.x) > halfW - .2f || Mathf.Abs(offset.y) > halfH - .2f)
                {
                    float scale = Mathf.Min((halfW - .7f) / Mathf.Max(.001f, Mathf.Abs(offset.x)), (halfH - .7f) / Mathf.Max(.001f, Mathf.Abs(offset.y)));
                    Vector2 tip = centre + offset * Mathf.Min(1, scale), dir = offset.normalized, side = new Vector2(-dir.y, dir.x);
                    var palette = new InkPalette(new Color(.08f, .06f, .1f), tint, Color.Lerp(tint, Color.white, .7f), tint, ArcadeSkin.Gold);
                    InkStroke s = InkStroke.Of(palette, .09f, .95f, .2f);
                    arrow.Stroke(InkCanvas.Line(tip - dir * .55f, tip, .02f, 3), s);
                    arrow.Stroke(InkCanvas.Line(tip, tip - dir * .28f + side * .2f), s);
                    arrow.Stroke(InkCanvas.Line(tip, tip - dir * .28f - side * .2f), s);
                }
            }
            arrow.Commit();
        }

        private void Clear()
        {
            foreach (var pair in fades) pair.Value.Target = 1;
            ApplyFades(PresentationSettings.ReducedMotion ? 1 : Time.unscaledDeltaTime);
            if (ring != null) { ring.Hide(); arrow.Hide(); }
            if (showThrough != null) showThrough.enabled = false;
            showThroughAlpha = 0; Covered = 0; RingVisible = false;
        }

        private void OnDisable() { foreach (var pair in fades) if (pair.Key != null) { Color c = pair.Key.color; c.a = pair.Value.Base; pair.Key.color = c; } fades.Clear(); }
    }
}
