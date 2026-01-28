using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class NightmareAtmosphere : MonoBehaviour
    {
        private ArenaController owner;
        private readonly SpriteRenderer[] motes = new SpriteRenderer[18];
        public int VisibleCount { get; private set; }
        public void Configure(ArenaController controller)
        {
            owner = controller;
            Sprite paper = NightmareAssets.Current.Scenery[12];
            for (int index = 0; index < motes.Length; index++)
            {
                var child = new GameObject("DriftingReceipt" + index); child.transform.SetParent(transform, false);
                motes[index] = child.AddComponent<SpriteRenderer>(); motes[index].sprite = paper; motes[index].sortingOrder = -30;
            }
        }
        private void LateUpdate()
        {
            if (owner == null || owner.Session == null || owner.Simulation == null || owner.PresentationPaused) return;
            bool shop = owner.Session.State.Phase == SessionPhase.Shop;
            bool enabled = !PresentationSettings.ReducedMotion && !PresentationSettings.ReducedEffects && !NightmareHaunting.Quiet && !owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible;
            int depth = NightmareRules.Depth(owner.Session.State.Wave);
            VisibleCount = enabled ? Mathf.Min(motes.Length, 4 + depth * 3) : 0;
            float clock = owner.PresentationClock;
            for (int index = 0; index < motes.Length; index++)
            {
                SpriteRenderer mote = motes[index]; mote.enabled = index < VisibleCount;
                if (!mote.enabled) continue;
                float seed = index * 2.39996f;
                float span = shop ? 8 : owner.WorldBounds.x * .45f;
                float horizontal = Mathf.Sin(seed * 7.3f) * span;
                float vertical = Mathf.Repeat(clock * (.11f + index % 3 * .025f) + index * 1.73f, shop ? 9 : 20) - (shop ? 4.5f : 10);
                Vector2 position = new Vector2(horizontal + Mathf.Sin(clock * .3f + seed) * .25f, vertical);
                mote.transform.position = position;
                mote.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * .35f + seed) * 25);
                float scale = (.035f + index % 3 * .012f) / Mathf.Max(.01f, mote.sprite.bounds.size.x);
                mote.transform.localScale = new Vector3(scale, scale * 1.7f, 1);
                Color spill = NightmareLighting.Active != null ? NightmareLighting.Active.Spill(position) : Color.black;
                mote.color = new Color(.82f + spill.r * .1f, .72f + spill.g * .1f, .59f + spill.b * .1f, shop ? .18f : .08f);
            }
        }
    }
}
