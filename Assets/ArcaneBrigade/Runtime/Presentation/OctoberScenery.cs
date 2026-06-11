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
        /// <summary>The way on is a painted portal standing on the floor, one for each map, looping its idle frames far away and its
        /// open frames as the hero comes near (requirement 24/10/2026; art: Docs/Playtest_Art_Prompts/25_Shop_Portals.txt).
        /// Resources/Shop/Portals/{Map}: 16 frames, 0-7 idle, 8-15 open. Until a map's portal is painted, the arched door stands in.</summary>
        private static readonly System.Collections.Generic.Dictionary<MapTheme, Sprite[]> portals = new System.Collections.Generic.Dictionary<MapTheme, Sprite[]>();
        public const int PortalFrames = 8;
        public const float PortalFps = 10, PortalHeight = 2f;
        private static Sprite[] Portal(MapTheme theme)
        {
            if (portals.TryGetValue(theme, out Sprite[] frames)) return frames;
            Sprite[] all = Resources.LoadAll<Sprite>("Shop/Portals/" + theme);
            System.Array.Sort(all, (x, y) => Number(x.name).CompareTo(Number(y.name)));
            return portals[theme] = all.Length >= PortalFrames * 2 ? all : null;
            static int Number(string name) { int i = name.Length; while (i > 0 && char.IsDigit(name[i - 1])) i--; return int.TryParse(name.Substring(i), out int n) ? n : 0; }
        }
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
            Sprite[] portal = Portal(MapIdentity.For(owner.Session.State.Wave + 1));
            if (portal != null)
            {
                int loop = PresentationSettings.ReducedMotion ? 0 : (int)(owner.PresentationClock * PortalFps) % PortalFrames;
                view.sprite = portal[(opening >= .5f ? PortalFrames : 0) + loop];
                transform.localScale = Vector3.one * (PortalHeight / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(view.sprite).height));
                transform.rotation = Quaternion.identity;
                Vector3 seat = transform.parent.position + (lifecycle == null ? Vector3.zero : transform.parent.TransformVector(lifecycle.FixtureOffset));
                WorldSpriteGrounding.Place(view, seat); // it stands on the floor (no hovering)
                view.sortingOrder = WorldSpriteGrounding.SortAt(transform.parent.position.y);
                float shown = lifecycle == null ? 1 : lifecycle.FixtureVisibility;
                view.color = new Color(1, 1, 1, shown);
                grounded.Display(view.sprite, transform.parent.position, 1.5f, 0, .32f * shown);
                return;
            }
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
