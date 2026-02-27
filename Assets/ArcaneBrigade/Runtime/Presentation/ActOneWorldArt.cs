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
        private static readonly string[][] ZoneProps = {
            new[] { "Prop_BoundaryGate", "Prop_Ledger" },
            new[] { "Prop_PlatformBench", "Prop_DepartureBoard", "Prop_PhoneBooth" },
            new[] { "Prop_DiningTable", "Prop_NailedLetterbox", "Prop_InwardWindow", "Prop_FrontDoorMarks" },
            new[] { "Prop_SingleBed", "Prop_Nightstand", "Prop_ReceiptChair", "Prop_Ledger" }
        };
        private static readonly string[] Floors = { "Floor_Crossing", "Floor_Platform", "Floor_House", "Floor_PerfectRoom" };
        public void Configure(ArenaController value)
        {
            owner = value;
            if (GetComponent<ActOneRestingScenes>() == null) gameObject.AddComponent<ActOneRestingScenes>().Configure(value);
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
            string floorName = hidden ? "Floor_NightYouHid" : Floors[zone];
            Sprite tile = ActOneArtwork.Picture(floorName);
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
            if (key != sceneKey)
            {
                Clear(); sceneKey = key;
                if (state.Phase == SessionPhase.Defeat) return;
                if (!shop)
                    for (int i = 0; i < ZoneProps[zone].Length; i++)
                        Add(ZoneProps[zone][i], new Vector2((i % 2 == 0 ? -1 : 1) * 8.2f, 3.2f - i * 2.2f), i == 0 ? 2.8f : 1.8f, true);
                int seated = shop ? Mathf.Clamp((state.Wave - 8) / 2, 0, 5) : zone == 0 ? 0 : zone + 2;
                for (int i = 0; i < seated; i++)
                    Add("SeatedOne_" + (char)('A' + i % 5), new Vector2((i % 2 == 0 ? -1 : 1) * (shop ? 8.4f : 10f), 3.8f - (i / 2) * 2.4f), 1.65f, false);
                if (shop && (state.Wave == 8 || state.Wave == 12 || state.Wave == 13))
                    Add(state.Wave >= 13 ? "LastCustomer_BorrowedFace" : "LastCustomer_Pleasant", ActOneCanon.ChairPosition(state.Wave) + Vector2.left * 1.6f, 2.6f, false);
            }
            foreach (var pair in floating) pair.prop.Display(pair.anchor, owner.PresentationClock, NightmareRules.Depth(state.Wave));
            foreach (var surface in surfaces)
            {
                float light = SharedLightField.Active?.SurfaceVisibility(surface.anchor) ?? 1;
                Color tint = surface.dusty ? new Color(.8f, .78f, .85f) : Color.white;
                surface.view.color = new Color(tint.r * light, tint.g * light, tint.b * light, 1);
            }
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
            foreach (var go in objects) if (go != null) { go.SetActive(false); Destroy(go); }
            objects.Clear(); floating.Clear(); surfaces.Clear();
        }
        private void OnDestroy() => Clear();
    }
}
