using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ShopMergePreview : MonoBehaviour
    {
        private ArenaController owner;
        private readonly List<int> identities = new List<int>();
        private readonly LineRenderer[] links = new LineRenderer[2];
        private Material material;
        private Vector2 stock;
        private int hintedToken = -1;
        private Material threadLight, chimeLight;
        private MeshRenderer chime;
        public int ApproachingCount => identities.Count;
        public static bool Ready(RunSession session, ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion && session.CanBuy(offer) && session.StarsUpOnBuy(offer);
        public static int ProjectedTier(RunSession session, ShopOffer offer)
        {
            return Ready(session, offer) ? session.ProjectTier(offer) : offer == null ? 1 : session.CopyTier(offer);
        }
        public void Configure(ArenaController controller)
        {
            owner = controller; material = new Material(Shader.Find("Sprites/Default"));
            // the thread and the chime are light (shaders); without them the old gold line stays
            Shader threadShader = Resources.Load<Shader>("Shaders/MergeThread"), chimeShader = Resources.Load<Shader>("Shaders/MergeChime");
            if (threadShader != null && threadShader.isSupported) threadLight = new Material(threadShader) { hideFlags = HideFlags.DontSave };
            if (chimeShader != null && chimeShader.isSupported)
            {
                chimeLight = new Material(chimeShader) { hideFlags = HideFlags.DontSave };
                var go = new GameObject("MergeChime"); go.transform.SetParent(transform, false);
                var mesh = new Mesh { hideFlags = HideFlags.DontSave };
                mesh.vertices = new[] { new Vector3(-.5f, -.5f), new Vector3(.5f, -.5f), new Vector3(-.5f, .5f), new Vector3(.5f, .5f) };
                mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
                mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                chime = go.AddComponent<MeshRenderer>(); chime.sharedMaterial = chimeLight; chime.sortingOrder = 475; chime.enabled = false;
            }
            for (int index = 0; index < links.Length; index++)
            {
                var child = new GameObject("MergePreviewThread_" + index); child.transform.SetParent(transform, false);
                LineRenderer line = child.AddComponent<LineRenderer>(); line.sharedMaterial = material;
                line.positionCount = threadLight != null ? 24 : 3; line.useWorldSpace = true; line.widthMultiplier = threadLight != null ? .34f : 0.035f; line.sortingOrder = 470;
                if (threadLight != null) { line.sharedMaterial = threadLight; line.textureMode = LineTextureMode.Stretch; }
                line.startColor = new Color(1, 0.84f, 0.38f, 0.45f); line.endColor = new Color(1, 0.84f, 0.38f, 0.08f);
                line.enabled = false; links[index] = line;
            }
        }
        public void Observe(WorldShop.Station station, bool blocked)
        {
            if (blocked || station == null) CheckpointAudio.CancelScheduledPrefix("ui.merge.hint");
            identities.Clear();
            if (!blocked && station != null && Ready(owner.Session, station.Offer))
            {
                if (hintedToken != station.Offer.Token) { CheckpointAudio.PlayInterfaceId("ui.merge.hint", 0.13f); hintedToken = station.Offer.Token; }
                stock = station.Position;
                foreach (UnitState unit in owner.Simulation.Units)
                {
                    if (unit.Faction != Faction.Companion || !unit.Alive || unit.Definition.Id != station.Offer.Id) continue;
                    identities.Add(unit.Id); if (identities.Count == 2) break;
                }
            }
            for (int index = 0; index < links.Length; index++) links[index].enabled = index < identities.Count && !PresentationSettings.ReducedMotion;
            if (chime != null) { chime.enabled = identities.Count > 0; chime.transform.position = stock + Vector2.up * .75f; chime.transform.localScale = new Vector3(3.6f, 3.6f, 1); }
        }
        public bool Destination(UnitState unit, out Vector2 destination)
        {
            int index = identities.IndexOf(unit.Id);
            destination = stock + new Vector2(index == 0 ? -0.9f : 0.9f, -0.45f);
            return index >= 0;
        }
        private void LateUpdate()
        {
            if (owner == null || owner.Simulation == null || owner.Simulation.Phase == RunPhase.Paused) return;
            if (threadLight != null) threadLight.SetFloat("_Clock", owner.PresentationClock);
            if (chimeLight != null) chimeLight.SetFloat("_Clock", owner.PresentationClock);
            for (int index = 0; index < identities.Count; index++)
            {
                UnitState unit = null;
                foreach (UnitState candidate in owner.Simulation.Units) if (candidate.Id == identities[index]) { unit = candidate; break; }
                if (unit == null) { links[index].enabled = false; continue; }
                Vector3 start = unit.Position + Vector2.up * 0.2f;
                Vector3 end = stock + Vector2.up * 0.55f;
                Vector3 bend = Vector3.Lerp(start, end, 0.5f) + Vector3.up * (0.16f + Mathf.Sin(owner.PresentationClock * 3) * 0.04f);
                if (threadLight == null) { links[index].SetPosition(0, start); links[index].SetPosition(1, bend); links[index].SetPosition(2, end); continue; }
                // a smooth arc that sags and sways like a thread of light
                bend += Vector3.up * (.55f + .06f * Mathf.Sin(owner.PresentationClock * 2 + index));
                for (int k = 0; k < 24; k++)
                {
                    float u = k / 23f;
                    links[index].SetPosition(k, Vector3.Lerp(Vector3.Lerp(start, bend, u), Vector3.Lerp(bend, end, u), u));
                }
            }
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
