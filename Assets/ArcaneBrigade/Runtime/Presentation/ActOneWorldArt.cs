using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>World-anchored canon props, seated travellers and zone floor art. No camera-relative scenery.</summary>
    [DefaultExecutionOrder(80)]
    public sealed class ActOneWorldArt : MonoBehaviour
    {
        private ArenaController owner;
        private Material floor;
        private int sceneKey = int.MinValue;
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly List<(FloatingStoryProp prop, Vector2 anchor)> floating = new List<(FloatingStoryProp, Vector2)>();
        private readonly List<(SpriteRenderer view, Vector2 anchor, bool dusty)> surfaces = new List<(SpriteRenderer, Vector2, bool)>();
        private static string[][] ZoneProps => ArenaLayoutPlan.ZoneProps; // the arena layout plans their footprints
        private static readonly string[] Floors = { "Floor_Crossing", "Floor_Platform", "Floor_House", "Floor_PerfectRoom" };
        /// <summary>Requirement 25/10/2026: the Act I floor follows the map, not the story zone (the four old floors keep their maps:
        /// the Crossing was the Waiting Room's, the Platform the Post Office's, the House the Lamp Hall's, the Perfect Room the
        /// Archive Vault's). A new map's floor (Resources/MienCho/ActOne, 1256x1256 seamless) falls back to the zone's floor until painted.</summary>
        public static string MapFloor(MapTheme map) => map switch
        {
            MapTheme.WaitingRoom => "Floor_Crossing",
            MapTheme.PostOffice => "Floor_Platform",
            MapTheme.Glasshouse => "Floor_Glasshouse",
            MapTheme.LampHall => "Floor_House",
            MapTheme.Theatre => "Floor_Theatre",
            _ => "Floor_PerfectRoom"
        };
        public void Configure(ArenaController value)
        {
            owner = value;
            if (GetComponent<ActOneRestingScenes>() == null) gameObject.AddComponent<ActOneRestingScenes>().Configure(value);
            if (GetComponent<ActOneCustomerVisits>() == null) gameObject.AddComponent<ActOneCustomerVisits>().Configure(value);
        }
        private void LateUpdate()
        {
            if (owner?.Session == null || owner.Simulation == null) return;
            var state = owner.Session.State;
            if (state.Endless || state.Wave > 20 || owner.Session.SecretRoomOpen) { Clear(); sceneKey = int.MinValue; return; }
            if (owner.PresentationPaused) return;
            bool shop = state.Phase == SessionPhase.Shop;
            int zone = ActOneCanon.Zone(state.Wave);
            bool hidden = state.ActOne?.SitCloser == ActOneProgress.Fighting;
            string floorName = hidden ? "Floor_NightYouHid" : MapFloor(MapIdentity.For(state.Wave));
            Sprite tile = ActOneArtwork.Picture(floorName) ?? ActOneArtwork.Picture(Floors[zone]);
            if (floor == null)
            {
                var ground = GameObject.Find("HD_TiledFloor");
                // K8 owns this per-scene material; retain its handle so later endless/secret-room updates still apply.
                if (ground != null) floor = ground.GetComponent<MeshRenderer>()?.sharedMaterial;
            }
            if (tile != null && floor != null)
            {
                floor.SetTexture("_MainTex", tile.texture); floor.SetTexture("_NextTex", tile.texture);
                floor.SetFloat("_FloorBlend", 0); floor.SetFloat("_NightmareStrength", 0);
            }
            int key = state.Wave * 10 + (shop ? 1 : 0) + (hidden ? 2 : 0);
            if (!shop) key = unchecked(key * 31 + owner.WorldBounds.GetHashCode()); // the arena (and its layout) is settled
            if (key != sceneKey)
            {
                Clear(); sceneKey = key;
                if (state.Phase == SessionPhase.Defeat) return;
                // in a fight each one asks the arena layout for its spot (and size); one with no room sits the wave out
                if (!shop)
                    for (int i = 0; i < ZoneProps[zone].Length; i++)
                        Planned(ZoneProps[zone][i], "prop" + i, ArenaLayoutPlan.ZonePropWish(i), ArenaLayoutPlan.ZonePropHeight(i), true);
                int seated = shop ? Mathf.Clamp((state.Wave - 8) / 2, 0, 5) : ArenaLayoutPlan.SeatedCount(zone);
                for (int i = 0; i < seated; i++)
                    if (shop) Add(ArenaLayoutPlan.SeatedName(i), new Vector2((i % 2 == 0 ? -1 : 1) * 8.4f, 3.8f - (i / 2) * 2.4f), ArenaLayoutPlan.SeatedHeight, false);
                    else Planned(ArenaLayoutPlan.SeatedName(i), "seated" + i, ArenaLayoutPlan.SeatedWish(i), ArenaLayoutPlan.SeatedHeight, false);
            }
            foreach (var pair in floating) pair.prop.Display(pair.anchor, owner.PresentationClock, NightmareRules.Depth(state.Wave));
            foreach (var surface in surfaces)
            {
                float light = SharedLightField.Active?.SurfaceVisibility(surface.anchor) ?? 1;
                Color tint = surface.dusty ? new Color(.8f, .78f, .85f) : Color.white;
                surface.view.color = new Color(tint.r * light, tint.g * light, tint.b * light, 1);
            }
        }
        private void Planned(string name, string id, Vector2 anchor, float height, bool hover)
        {
            Sprite image = ActOneArtwork.Picture(name); if (image == null) return;
            Rect ink = SpriteInkMetrics.BoundsOf(image);
            Vector2 drawn = ink.size * (height / Mathf.Max(.01f, ink.height));
            if (!ArenaLayout.Place(owner, id, drawn, ref anchor, out float fit)) return;
            Add(name, anchor, hover ? height * fit : height, hover); // standing figures keep their size; only floating props shrink to fit
        }
        private void Add(string name, Vector2 anchor, float height, bool hover)
        {
            Sprite image = ActOneArtwork.Picture(name); if (image == null) return;
            var go = new GameObject("ActOne-" + name); go.transform.SetParent(transform, false); objects.Add(go);
            var view = go.AddComponent<SpriteRenderer>(); view.sprite = image;
            surfaces.Add((view, anchor, name.StartsWith("SeatedOne_", System.StringComparison.Ordinal)));
            float scale = height / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(image).height);
            go.transform.localScale = Vector3.one * scale;
            go.transform.position = (Vector3)anchor - go.transform.TransformVector(WorldSpriteGrounding.Foot(image));
            view.sortingOrder = WorldSpriteGrounding.SortAt(anchor.y);
            if (hover)
            {
                var prop = go.AddComponent<FloatingStoryProp>(); prop.Configure(view, 400 + objects.Count * 13);
                floating.Add((prop, anchor));
            }
            else
            {
                var shadow = go.AddComponent<GroundShadow>(); shadow.Configure(image);
                shadow.Display(image, anchor, height * .7f, 0, .22f);
            }
        }
        private void Clear()
        {
            foreach (var go in objects) if (go != null) { ShopLayout.Active?.Release("story:" + go.name); go.SetActive(false); Destroy(go); }
            objects.Clear(); floating.Clear(); surfaces.Clear();
        }
        private void OnDestroy() => Clear();
    }
}
