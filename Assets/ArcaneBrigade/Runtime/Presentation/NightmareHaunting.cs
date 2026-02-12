using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(-30)]
    public sealed class NightmareHaunting : MonoBehaviour
    {
        private ArenaController owner;
        private MeshRenderer fog;
        private Material material;
        private Mesh mesh;
        private SpriteRenderer reserved;
        private int wave = -1;
        private SessionPhase phase;
        private float entered;
        public static NightmareHaunting Active { get; private set; }
        public static bool Quiet => Active != null && Active.Silence;
        public bool Silence { get; private set; }
        public float Density => material != null ? material.GetFloat("_FogAmount") : 0;
        public float Clock => material != null ? material.GetFloat("_FogClock") : 0;
        public static float FogFor(int depth, bool reduced) => reduced ? .025f : Mathf.Lerp(.015f, .14f, Mathf.Clamp01(depth / 4f));
        public void Configure(ArenaController controller)
        {
            owner = controller; Active = this;
            GameObject plane = new GameObject("LowDreamMist");
            plane.name = "LowDreamMist"; plane.transform.SetParent(transform, false);
            mesh = new Mesh { name = "DreamMistPlane" };
            mesh.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(.5f, .5f, 0), new Vector3(-.5f, .5f, 0) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 }; mesh.RecalculateBounds();
            plane.AddComponent<MeshFilter>().sharedMesh = mesh;
            fog = plane.AddComponent<MeshRenderer>(); fog.sortingOrder = -25;
            material = new Material(Resources.Load<Shader>("NightmareFog")); fog.sharedMaterial = material;
            var tag = new GameObject("APlaceReservedForYou"); tag.transform.SetParent(transform, false);
            reserved = tag.AddComponent<SpriteRenderer>(); reserved.sortingOrder = -33;
        }
        private void Update()
        {
            if (owner == null || owner.Session == null || owner.Simulation == null) return;
            if (owner.PresentationPaused) return;
            RunSnapshot state = owner.Session.State;
            if (wave != state.Wave || phase != state.Phase) { wave = state.Wave; phase = state.Phase; entered = owner.PresentationClock; }
            Silence = wave == 19 && state.Phase == SessionPhase.Shop && !owner.Session.SecretRoomOpen && owner.PresentationClock - entered < 4;
            bool ending = state.Nightmare.CustomerDefeated && !state.Endless;
            fog.enabled = !owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible;
            fog.transform.localScale = new Vector3(owner.WorldBounds.x, owner.WorldBounds.y, 1);
            int depth = NightmareRules.Depth(state.Wave);
            // the little silhouette sits on the reserved place itself, wherever the shop layout put it this visit
            bool tagged = depth >= 2 && state.Phase == SessionPhase.Shop && !owner.Session.SecretRoomOpen;
            Vector2 place = NightmareLighting.LandmarkPosition(5, true, owner.WorldBounds * .5f);
            if (tagged && ShopLayout.Active != null) tagged = ShopLayout.Active.TryFoot("landmark5", out place);
            reserved.enabled = tagged;
            if (reserved.enabled)
            {
                reserved.sprite = owner.Simulation.Champion.Definition.Body;
                reserved.transform.position = place + Vector2.up * .17f;
                reserved.transform.localScale = Vector3.one * (.27f / Mathf.Max(.01f, reserved.sprite.bounds.size.x));
                reserved.color = new Color(.12f, .08f, .19f, .72f);
            }
            float amount = ending ? .008f : FogFor(depth, PresentationSettings.ReducedEffects);
            if (PresentationSettings.HighReadability) amount *= .45f;
            material.SetFloat("_FogAmount", amount);
            material.SetFloat("_ShadowAmount", ending || PresentationSettings.ReducedEffects ? 0 : depth * .024f);
            material.SetFloat("_FogClock", PresentationSettings.ReducedMotion || Silence ? 0 : owner.PresentationClock);
            material.SetVector("_RoomHalf", owner.WorldBounds * .5f);
            material.SetVector("_PlayerAnchor", owner.VisualChampionPosition);
            material.SetVector("_PlayerDrift", Vector2.ClampMagnitude(owner.Simulation.Champion.Velocity, 6));
            bool customer = owner.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId && !ending;
            material.SetFloat("_Ceremony", customer ? PresentationSettings.ReducedEffects ? .25f : 1 : 0);
        }
        public static Vector2 UnsettledPosition(int index, int seed, int wave, float time, Vector2 position, Vector2 player, Vector2 facing)
        {
            int depth = NightmareRules.Depth(wave);
            if (depth < 1 || index != 0 && index != 3) return position;
            float sign = ((seed ^ wave * 397 ^ index * 17) & 1) == 0 ? -1 : 1;
            position.x += sign * depth * .045f;
            if (depth >= 2 && !PresentationSettings.ReducedMotion && Vector2.Distance(position, player) > 3 && Vector2.Dot(facing, position - player) < 0)
            {
                float phase = Mathf.Repeat(time + index * 7, 23);
                if (phase > 20) position.x += sign * Mathf.Sin((phase - 20) / 3 * Mathf.PI) * .12f;
            }
            return position;
        }
        private void OnDestroy()
        {
            if (Active == this) Active = null;
            if (material != null) Destroy(material);
            if (mesh != null) Destroy(mesh);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => Active = null;
    }
}
