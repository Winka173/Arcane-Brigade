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
        private GroundShadow grounded;
        private MarketLifecycle lifecycle;
        private DreamSuspension suspension;
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
            // 1.4 units tall: the door's arch stays under the middle stall's pedestal instead of rising into it
            float scale = 1.4f / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(view.sprite).height);
            transform.localScale = Vector3.one * scale;
            transform.rotation = Quaternion.Euler(0, 0, DreamLevitation.Tilt(owner.PresentationClock, 41, PresentationSettings.ReducedMotion, .45f));
            // Keep the visible foot suspended relative to the fixed threshold after frame changes.
            if (lifecycle == null) lifecycle = GetComponentInParent<MarketLifecycle>();
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
