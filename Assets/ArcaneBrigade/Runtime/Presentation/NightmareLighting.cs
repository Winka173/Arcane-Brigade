using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class NightmareLighting : MonoBehaviour
    {
        private ArenaController owner;
        private readonly Vector4[] sources = new Vector4[4];
        private readonly Color[] colors = new Color[4];
        private static readonly int[] LightIds = { Shader.PropertyToID("_DreamLight0"), Shader.PropertyToID("_DreamLight1"), Shader.PropertyToID("_DreamLight2"), Shader.PropertyToID("_DreamLight3") };
        private static readonly int[] ColorIds = { Shader.PropertyToID("_DreamColor0"), Shader.PropertyToID("_DreamColor1"), Shader.PropertyToID("_DreamColor2"), Shader.PropertyToID("_DreamColor3") };
        public static NightmareLighting Active { get; private set; }
        public float Depth { get; private set; }
        public float Dawn { get; private set; }
        public float Ambient => AmbientFor(Depth, Dawn);
        public static float AmbientFor(float depth, float dawn)
        {
            float d=Mathf.Clamp(depth,0,4);
            float night=d<1?Mathf.Lerp(.92f,.58f,d):d<2?Mathf.Lerp(.58f,.27f,d-1):d<3?Mathf.Lerp(.27f,.075f,d-2):Mathf.Lerp(.075f,.018f,d-3);
            return Mathf.Lerp(night,1.08f,Mathf.Clamp01(dawn));
        }
        public static float Falloff(Vector2 offset, float radius)
        {
            if (radius <= 0) return 0;
            return Mathf.Exp(-offset.sqrMagnitude * 2.7f / (radius * radius));
        }
        public static Vector2 LandmarkPosition(int index, bool shop, Vector2 half) => index switch
        {
            0 => shop ? new Vector2(6.5f, 1) : half - Vector2.one * 2,
            1 => shop ? new Vector2(-6.8f, -3.7f) : new Vector2(-half.x + 2, -half.y + 2),
            2 => shop ? new Vector2(-6.8f, .5f) : new Vector2(-half.x + 2, half.y - 2),
            3 => shop ? new Vector2(6.5f, -3.7f) : new Vector2(half.x - 2, -half.y + 2),
            4 => new Vector2(0, shop ? 1.4f : half.y - 2),
            5 => new Vector2(6.5f, -1),
            6 => new Vector2(-6, -3.8f),
            _ => new Vector2(-5.9f, -3.4f)
        };
        public void Configure(ArenaController controller) { owner = controller; Active = this; }
        private void LateUpdate()
        {
            if (owner == null || owner.Session == null || owner.Simulation == null) return;
            if (owner.PresentationPaused) return;
            bool shop = owner.Session.State.Phase == SessionPhase.Shop;
            bool reduced = PresentationSettings.ReducedMotion;
            bool dawn = owner.Session.State.Nightmare.CustomerDefeated && !owner.Session.State.Endless;
            float blend = reduced ? 1 : 1 - Mathf.Exp(-Time.deltaTime * .9f);
            Depth = Mathf.Lerp(Depth, NightmareRules.Depth(owner.Session.State.Wave), blend);
            Dawn = Mathf.Lerp(Dawn, dawn ? 1 : 0, blend);
            Vector2 half = owner.WorldBounds * .5f;
            Vector2 lamp = LandmarkPosition(1, shop, half) + Vector2.up * .6f;
            Vector2 window = LandmarkPosition(2, shop, half);
            Vector2 player = owner.VisualChampionPosition;
            float breath = reduced || PresentationSettings.ReducedFlashes ? 1 : 1 + Mathf.Sin(owner.PresentationClock * 1.15f) * .025f;
            float retreat = shop ? 1 : Mathf.Lerp(1, .08f, Depth / 4 * (1 - Dawn));
            sources[0] = new Vector4(lamp.x, lamp.y, 4.1f, .8f * breath * retreat);
            if (StagePerformance.Active != null && !reduced) sources[0].x += StagePerformance.Active.Impact * .12f;
            sources[1] = new Vector4(window.x, window.y, 4.6f, Mathf.Lerp(.38f, .18f, Depth / 4) * retreat);
            sources[2] = new Vector4(player.x, player.y, 3.6f, .12f);
            sources[3] = new Vector4(0, -1, 6.5f, shop ? .3f : 0);
            colors[0] = new Color(1, .6f, .23f);
            colors[1] = Color.Lerp(new Color(.75f, .83f, 1), new Color(.48f, .45f, .85f), Depth / 4);
            colors[2] = new Color(.55f, .85f, .78f);
            colors[3] = owner.Shop != null && owner.Shop.Special ? new Color(.78f, .46f, .88f) : new Color(1, .7f, .38f);
            if (!shop && StagePerformance.Active != null && StagePerformance.Active.Light.w > 0)
            {
                sources[3] = StagePerformance.Active.Light; colors[3] = StagePerformance.Active.LightColor;
            }
            if (owner.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId && !dawn)
            {
                Vector2 witness = owner.Simulation.Boss.Position + Vector2.up * 2;
                sources[3] = new Vector4(witness.x, witness.y, 9, .36f * breath);
                colors[3] = new Color(.85f, .36f, .24f);
            }
            Shader.SetGlobalFloat("_DreamEnabled", owner.Session.SecretRoomOpen ? 0 : 1);
            Shader.SetGlobalFloat("_DreamDepth", Depth);
            Shader.SetGlobalFloat("_DreamDawn", Dawn);
            Shader.SetGlobalFloat("_DreamAmbient", shop ? Mathf.Max(.5f, Ambient) : PresentationSettings.HighReadability ? Mathf.Max(.24f, Ambient) : Ambient);
            Shader.SetGlobalFloat("_DreamClock", reduced ? 0 : owner.PresentationClock);
            for (int index = 0; index < sources.Length; index++)
            {
                Shader.SetGlobalVector(LightIds[index], sources[index]);
                Shader.SetGlobalColor(ColorIds[index], colors[index]);
            }
        }
        public Color Spill(Vector2 position)
        {
            if (owner == null || owner.Session.SecretRoomOpen) return Color.black;
            Color light = Color.black;
            for (int index = 0; index < sources.Length; index++)
                light += colors[index] * (Falloff(position - (Vector2)sources[index], sources[index].z) * sources[index].w);
            if (SharedLightField.Active != null) light += SharedLightField.Active.ColorAt(position) * .55f;
            return light;
        }
        public Vector2 ShadowDirection(Vector2 position)
        {
            Vector2 direction = new Vector2(.8f, -.35f) * .035f;
            for (int index = 0; index < sources.Length; index++)
            {
                if (index == 2) continue;
                Vector2 offset = position - (Vector2)sources[index];
                float strength = Falloff(offset, sources[index].z) * sources[index].w;
                if (offset.sqrMagnitude > .02f) direction += offset.normalized * strength;
            }
            return direction.sqrMagnitude > .001f ? direction.normalized : new Vector2(.8f, -.35f);
        }
        private void OnDisable()
        {
            if (Active != this) return;
            Active = null; Shader.SetGlobalFloat("_DreamEnabled", 0);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { Active = null; Shader.SetGlobalFloat("_DreamEnabled", 0); }
    }
}
