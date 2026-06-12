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
        private ArenaController owner; private SpriteRenderer view; private float opening;
        private static PaintedAnimationLibrary exitDoor;
        private GroundShadow grounded;
        private MarketLifecycle lifecycle;
        private DreamSuspension suspension;
        /// <summary>The way on is a painted rift hanging in the air, a tear in space, one for each map (requirement 24/10/2026; art:
        /// Docs/Playtest_Art_Prompts/25_Shop_Portals.txt): Resources/Shop/Portals/{Map}_Body (the torn rim, its hole empty) and
        /// {Map}_Swirl (a round four-armed vortex). The swirl turns behind the gate inside its opening - slowly far away, faster,
        /// larger and brighter as the hero comes near. Until a map's portal is painted, the arched door stands in.</summary>
        private static readonly System.Collections.Generic.Dictionary<MapTheme, (Sprite body, Sprite swirl)> portals = new System.Collections.Generic.Dictionary<MapTheme, (Sprite, Sprite)>();
        /// <summary>Each gate's opening in its body art: centre and half size, as shares of the art's width and height (from its
        /// bottom-left), measured from the painted hole.</summary>
        public static readonly System.Collections.Generic.Dictionary<MapTheme, Vector4> PortalHoles = new System.Collections.Generic.Dictionary<MapTheme, Vector4>
        {
            [MapTheme.WaitingRoom] = new Vector4(0.529f, 0.480f, 0.327f, 0.389f),
            [MapTheme.PostOffice] = new Vector4(0.512f, 0.493f, 0.293f, 0.388f),
            [MapTheme.LampHall] = new Vector4(0.502f, 0.522f, 0.461f, 0.431f),
            [MapTheme.ArchiveVault] = new Vector4(0.528f, 0.529f, 0.312f, 0.367f),
        };
        public const float PortalHeight = 2.2f;
        /// <summary>A map's rift art (null when it is not painted yet): the arrival rift uses it too.</summary>
        public static (Sprite body, Sprite swirl) PortalArt(MapTheme theme) => Portal(theme);
        private static (Sprite body, Sprite swirl) Portal(MapTheme theme)
        {
            if (portals.TryGetValue(theme, out var p)) return p;
            Sprite body = Resources.Load<Sprite>("Shop/Portals/" + theme + "_Body"), swirl = Resources.Load<Sprite>("Shop/Portals/" + theme + "_Swirl");
            return portals[theme] = body != null && swirl != null && PortalHoles.ContainsKey(theme) ? (body, swirl) : (null, null);
        }
        private Transform swirlHolder; private SpriteRenderer swirlView; private float swirlTurn;
        private GameObject riftInkHolder; private InkCanvas riftInk, riftLight;
        private void OnDestroy() { if (riftInkHolder != null) Destroy(riftInkHolder); }
        /// <summary>The rift's breath, in ink (requirement 24/10/2026): small things of its map drawn in spiralling to the hole (stars
        /// and gilt flecks, paper scraps, flame motes, pages and ink), and a few wisps of light leaking from the torn rim, curling
        /// out and fading. Faster and thicker as the hero comes near. Never a ring, a disc or a pool.</summary>
        private void RiftBreath(MapTheme map, Vector2 centre, Vector2 half, int order, float shown)
        {
            if (riftInkHolder == null)
            {
                riftInkHolder = new GameObject("Rift breath");
                riftInk = new InkCanvas(riftInkHolder.transform, "Rift motes", order + 1);
                riftLight = new InkCanvas(riftInkHolder.transform, "Rift light", order + 3, true);
            }
            riftInk.SetOrder(order + 1); riftLight.SetOrder(order + 3);
            riftInk.Clear(); riftLight.Clear();
            bool still = PresentationSettings.ReducedMotion;
            float t = owner.PresentationClock, pull = Mathf.Lerp(.35f, 1.1f, opening);
            InkPalette ink = map == MapTheme.PostOffice ? ArsenalInk.Cream : map == MapTheme.ArchiveVault ? ArsenalInk.Water : ArsenalInk.Gold;
            Color glow = map == MapTheme.WaitingRoom ? new Color(.6f, .95f, .9f) : map == MapTheme.PostOffice ? new Color(1f, .9f, .75f) : map == MapTheme.LampHall ? new Color(1f, .8f, .45f) : new Color(.85f, 1f, .95f);
            int count = still ? 6 : Mathf.RoundToInt(Mathf.Lerp(9, 16, opening));
            for (int i = 0; i < count; i++)
            {
                // each mote starts out beyond the rim and spirals in, shrinking as it is swallowed
                float life = Mathf.Repeat((still ? .3f : t * pull * .55f) + i / (float)count, 1);
                float r = Mathf.Lerp(1.9f, .15f, life * life), a = (i * 137.5f + life * 260) * Mathf.Deg2Rad;
                Vector2 p = centre + new Vector2(Mathf.Cos(a) * half.x * r, Mathf.Sin(a) * half.y * r);
                Vector2 tangent = new Vector2(-Mathf.Sin(a) * half.x, Mathf.Cos(a) * half.y).normalized;
                float size = .07f * (1 - .6f * life), alpha = Mathf.Sin(life * Mathf.PI) * shown;
                if (map == MapTheme.PostOffice || map == MapTheme.ArchiveVault)
                    riftInk.Stroke(InkCanvas.Line(p - tangent * size, p + tangent * size, 0, 0, false, i), InkStroke.Of(ink, size * 1.1f, alpha, i)); // a scrap of paper
                else
                {
                    riftLight.GlowRibbon(InkCanvas.Line(p - tangent * size * 1.6f, p + tangent * size * .4f, 0, 0, false, i), .2f, 1, size * .9f, new Color(glow.r, glow.g, glow.b, .9f * alpha)); // a mote with a short tail
                    if (map == MapTheme.WaitingRoom && i % 3 == 0) { riftLight.GlowRibbon(InkCanvas.Line(p - Vector2.right * size, p + Vector2.right * size), 1, 1, size * .3f, new Color(1, .95f, .8f, alpha)); riftLight.GlowRibbon(InkCanvas.Line(p - Vector2.up * size, p + Vector2.up * size), 1, 1, size * .3f, new Color(1, .95f, .8f, alpha)); } // a star
                }
            }
            // light leaking from the rim: short curls that rise out of the tear and fade
            int wisps = still ? 2 : opening > .5f ? 5 : 3;
            for (int i = 0; i < wisps; i++)
            {
                float life = Mathf.Repeat((still ? .5f : t * .35f) + i / (float)wisps, 1), a = (i * 97 + 40) * Mathf.Deg2Rad;
                Vector2 rim = centre + new Vector2(Mathf.Cos(a) * half.x * 1.02f, Mathf.Sin(a) * half.y * 1.02f), outward = (rim - centre).normalized;
                Vector2 tip = rim + outward * (.15f + .35f * life) + Vector2.up * .2f * life + new Vector2(-outward.y, outward.x) * Mathf.Sin(life * 5 + i) * .08f;
                riftLight.GlowRibbon(InkCanvas.Lob(rim, tip, .1f, 8), 1, 0, .05f * (1 - life * .5f), new Color(glow.r, glow.g, glow.b, (.55f + .3f * opening) * Mathf.Sin(life * Mathf.PI) * shown));
            }
            // a few fragments of the rim's own stuff wheel slowly round it, turning over and catching the light
            for (int i = 0; i < 5; i++)
            {
                float a = (still ? i * 72 : t * (14 + 10 * opening) + i * 72) * Mathf.Deg2Rad, flip = still ? 1 : Mathf.Abs(Mathf.Cos(t * 1.7f + i * 1.3f));
                Vector2 p = centre + new Vector2(Mathf.Cos(a) * half.x * 1.35f, Mathf.Sin(a) * half.y * 1.2f + Mathf.Sin(t * 1.1f + i) * .05f);
                Vector2 along = new Vector2(Mathf.Cos(a + 1.2f + i), Mathf.Sin(a + 1.2f + i)) * (.07f + .05f * flip);
                InkPalette bit = map == MapTheme.WaitingRoom ? ArsenalInk.Gold : map == MapTheme.PostOffice ? (i % 2 == 0 ? ArsenalInk.Cream : ArsenalInk.Wine) : map == MapTheme.LampHall ? ArsenalInk.Glass : ArsenalInk.Cream;
                float thick = map == MapTheme.PostOffice || map == MapTheme.ArchiveVault ? .11f : .07f; // stamps and pages are broad, shards slim
                riftInk.Stroke(InkCanvas.Line(p - along, p + along, 0, 0, false, 40 + i), InkStroke.Of(bit, thick, .95f * shown, 40 + i));
                if (flip > .85f) riftLight.GlowRibbon(InkCanvas.Line(p - along * .6f, p + along * .6f), 1, 1, .04f, new Color(1f, .95f, .8f, (flip - .85f) * 5 * .8f * shown)); // the glint as it faces you
            }
            // now and then a crack of light runs along the torn rim, as if it were still tearing
            float crackClock = still ? 0 : t / 1.6f;
            float crackLife = Mathf.Repeat(crackClock, 1) / .35f;
            if (!still && crackLife < 1)
            {
                int which = Mathf.FloorToInt(crackClock);
                float from = (which * 137) % 360, span = 50 * Mathf.SmoothStep(0, 1, crackLife);
                var rim = new System.Collections.Generic.List<Vector2>();
                for (int k = 0; k <= 8; k++)
                {
                    float ang = (from + span * k / 8f) * Mathf.Deg2Rad, jag = k % 2 == 0 ? 1.02f : .97f;
                    rim.Add(centre + new Vector2(Mathf.Cos(ang) * half.x * jag, Mathf.Sin(ang) * half.y * jag));
                }
                riftLight.GlowRibbon(rim, 1, .3f, .05f, new Color(1f, .93f, .7f, Mathf.Sin(crackLife * Mathf.PI) * .95f * shown));
            }
            riftInk.Commit(); riftLight.Commit();
        }
        private void Swirl(Sprite swirl, Vector4 hole, float shown)
        {
            if (swirlHolder == null)
            {
                swirlHolder = new GameObject("Portal swirl").transform; swirlHolder.SetParent(view.transform, false);
                swirlView = new GameObject("Swirl").AddComponent<SpriteRenderer>(); swirlView.transform.SetParent(swirlHolder, false);
            }
            swirlHolder.gameObject.SetActive(true);
            Bounds art = view.sprite.bounds; // in the body's local space
            Vector3 centre = new Vector3(art.min.x + art.size.x * hole.x, art.min.y + art.size.y * hole.y, 0);
            swirlHolder.localPosition = centre;
            float pulse = PresentationSettings.ReducedMotion ? 1 : 1 + .03f * Mathf.Sin(owner.PresentationClock * 2.4f);
            float grow = Mathf.Lerp(1, 1.08f, opening) * pulse; // a touch larger when it opens (the gate's rim hides the edge)
            Vector2 size = swirl.bounds.size;
            swirlHolder.localScale = new Vector3(art.size.x * hole.z * 2.12f / size.x * grow, art.size.y * hole.w * 2.12f / size.y * grow, 1); // the opening, a little over
            if (!PresentationSettings.ReducedMotion) swirlTurn += Time.deltaTime * Mathf.Lerp(28, 120, opening); // turns faster as the hero comes near
            swirlView.sprite = swirl; swirlView.transform.localRotation = Quaternion.Euler(0, 0, -swirlTurn);
            swirlView.sortingOrder = view.sortingOrder - 1; // behind the gate: its rim frames the vortex
            float lit = Mathf.Lerp(.82f, 1.1f, opening);
            swirlView.color = new Color(lit, lit, lit, shown);
            Vector2 half = new Vector2(art.size.x * hole.z, art.size.y * hole.w) * view.transform.lossyScale.x;
            RiftBreath(currentMap, swirlHolder.position, half, view.sortingOrder, shown);
        }
        private MapTheme currentMap;
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
            if (lifecycle == null) lifecycle = GetComponentInParent<MarketLifecycle>();
            // the next night's map decides the portal (it shows where the way leads)
            MapTheme next = MapIdentity.For(owner.Session.State.Wave + 1); currentMap = next;
            var portal = Portal(next);
            if (portal.body != null)
            {
                view.sprite = portal.body;
                transform.localScale = Vector3.one * (PortalHeight / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(view.sprite).height));
                bool still = PresentationSettings.ReducedMotion;
                transform.rotation = Quaternion.Euler(0, 0, still ? 0 : Mathf.Sin(owner.PresentationClock * .9f) * 1.5f); // it sways a little in the air
                float lift = .2f + (still ? 0 : Mathf.Sin(owner.PresentationClock * 1.4f) * .06f);
                Vector3 seat = transform.parent.position + (lifecycle == null ? Vector3.zero : transform.parent.TransformVector(lifecycle.FixtureOffset)) + Vector3.up * lift;
                WorldSpriteGrounding.Place(view, seat); // it hangs in the air: a rift, not a door
                view.sortingOrder = WorldSpriteGrounding.SortAt(transform.parent.position.y);
                float shown = lifecycle == null ? 1 : lifecycle.FixtureVisibility;
                view.color = new Color(1, 1, 1, shown);
                grounded.Display(view.sprite, transform.parent.position, 1.5f, lift, .22f * shown); // a soft shadow on the floor below it
                Swirl(portal.swirl, PortalHoles[next], shown);
                return;
            }
            if (swirlHolder != null) swirlHolder.gameObject.SetActive(false);
            if (riftInk != null) { riftInk.Hide(); riftLight.Hide(); }
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
            Vector3 offset = lifecycle == null ? Vector3.zero : transform.parent.TransformVector(lifecycle.FixtureOffset);
            Vector3 hovering = DreamLevitation.Offset(owner.PresentationClock, 41, NightmareRules.Depth(owner.Session.State.Wave), PresentationSettings.ReducedMotion, .75f);
            WorldSpriteGrounding.Place(view, transform.parent.position + offset + hovering);
            view.sortingOrder = WorldSpriteGrounding.SortAt(transform.parent.position.y);
            float alpha = lifecycle == null ? 1 : lifecycle.FixtureVisibility;
            view.color = new Color(1, 1, 1, alpha);
            grounded.Display(view.sprite, transform.parent.position, 1.35f, Mathf.Abs(offset.y) + hovering.y, .32f * alpha);
            suspension.Display(transform.parent.position, transform.parent.position + hovering, 1.35f, owner.PresentationClock, 41, view.sortingOrder - 1, alpha, true);
        }
    }
}
