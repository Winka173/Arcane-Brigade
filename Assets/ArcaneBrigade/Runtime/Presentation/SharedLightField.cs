using UnityEngine;
using System.Collections.Generic;

namespace ArcaneBrigade
{
    // Every actor contributes. Small GPU batches merge into one low-resolution world-space field.
    // Maximum blending bounds brightness; combat telegraphs and UI are never masked.
    public sealed class SharedLightField : MonoBehaviour
    {
        public const int Capacity = 16;
        public const float HeroRadius = 6.4f;
        private ArenaController owner;
        private readonly Vector4[] lights = new Vector4[Capacity];
        private readonly Vector4[] colors = new Vector4[Capacity];
        private RenderTexture field;
        private RenderTexture scratch;
        private readonly List<Vector4> allLights = new List<Vector4>();
        private readonly List<Color> allColors = new List<Color>();
        private Material painter;
        private int count;
        private static readonly Color[] Palette = { new Color(1,.84f,.55f), new Color(.65f,.9f,.85f), new Color(.73f,.84f,1), new Color(1,.94f,.76f), new Color(.9f,.77f,.5f) };
        public int SourceCount => count;
        public int RenderPasses { get; private set; }
        public static SharedLightField Active { get; private set; }
        public void Configure(ArenaController value)
        {
            owner = value;
            var source = Resources.Load<Material>("MienCho/SharedLight");
            if (source == null || source.shader == null || !source.shader.isSupported) return;
            painter = new Material(source);
            field = new RenderTexture(256, 192, 0, RenderTextureFormat.ARGB32) { name = "Shared companionship light", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            field.Create();
            scratch = new RenderTexture(field.descriptor) { name = "Shared light accumulation", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp }; scratch.Create();
            Active = this;
        }
        public static float Influence(Vector2 offset, float radius)
        {
            float t = Mathf.Clamp01(1 - offset.sqrMagnitude / (radius * radius));
            return t * t;
        }
        public static Color IdentityColor(string id)
        {
            uint hash = 2166136261;
            foreach (char c in id ?? "") hash = (hash ^ c) * 16777619;
            return Palette[hash % (uint)Palette.Length];
        }
        private void Add(Vector2 position, float radius, Color tint)
        {
            allLights.Add(new Vector4(position.x, position.y, radius, 1)); allColors.Add(tint); count++;
        }
        private void LateUpdate()
        {
            if (owner?.Simulation == null || field == null) return;
            if (owner.PresentationPaused && count > 0) return;
            count = 0; allLights.Clear(); allColors.Clear();
            Add(owner.VisualChampionPosition, HeroRadius, new Color(.68f, .91f, .87f));
            float interpolation = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            foreach (var unit in owner.Simulation.Units)
                if (unit.Alive && unit.Faction == Faction.Companion)
                    Add(ActOneRestingScenes.PartyLightPosition(unit, Vector2.Lerp(unit.PreviousPosition, unit.Position, interpolation)), 3.4f, IdentityColor(unit.Definition.Id));
            Vector2 size = owner.WorldBounds + Vector2.one * 12;
            painter.SetVector("_PartyRect", new Vector4(-size.x / 2, -size.y / 2, size.x, size.y));
            var previousTarget=RenderTexture.active;
            try
            {
                RenderPasses = 0;
                for (int start = 0; start < count; start += Capacity)
                {
                    int batch = Mathf.Min(Capacity, count - start);
                    for (int j = 0; j < batch; j++) { lights[j] = allLights[start+j]; colors[j] = allColors[start+j]; }
                    painter.SetVectorArray("_PartyLights", lights); painter.SetVectorArray("_PartyColors", colors); painter.SetInt("_PartyCount", batch);
                    painter.SetFloat("_Accumulate", start == 0 ? 0 : 1);
                    Graphics.Blit(start == 0 ? Texture2D.blackTexture : (Texture)field, scratch, painter);
                    var swap = field; field = scratch; scratch = swap; RenderPasses++;
                }
            }
            finally {RenderTexture.active=previousTarget;}
            Shader.SetGlobalTexture("_SharedLightField", field);
            Shader.SetGlobalVector("_SharedLightRect", new Vector4(-size.x / 2, -size.y / 2, size.x, size.y));
            Shader.SetGlobalFloat("_SharedLightEnabled", owner.Session.SecretRoomOpen ? 0 : 1);
        }
        public float Visibility(UnitState unit, float clock)
        {
            if (owner?.Session == null || owner.Session.SecretRoomOpen || owner.Session.State.Phase != SessionPhase.Combat || owner.Session.State.Nightmare.CustomerDefeated && !owner.Session.State.Endless) return 1;
            if (unit.Faction != Faction.Enemy) return 1;
            // Fully reveal danger before contact and throughout attack windups/releases.
            float safeRange = 4.3f + unit.Definition.Radius;
            if ((unit.Position - owner.Simulation.Champion.Position).sqrMagnitude < safeRange * safeRange || unit.PendingAttack || unit.TelegraphUntil > clock || unit.ContactImpactAt > clock || clock - unit.AttackStarted < unit.Definition.Weapon.Windup + unit.Definition.Weapon.Recovery + .65f) return 1;
            float light = Mathf.Max(Strength(unit.Position), NightLanternField.Active?.Illumination(unit.Position) ?? 0);
            float depth = NightmareRules.Depth(owner.Session.State.Wave) / 4f;
            // the dark dims a far enemy but never swallows it: its shape still reads at the edge of the light (at .16 the horde
            // past the lamp was a black smudge)
            float minimum = unit.Definition.IsBoss ? .8f : PresentationSettings.HighReadability ? .66f : .48f;
            return Mathf.Lerp(1, Mathf.Lerp(minimum, 1, Mathf.SmoothStep(0, 1, light * 1.7f)), depth);
        }
        public float Strength(Vector2 position)
        {
            float value = 0;
            for (int i = 0; i < count; i++) value = Mathf.Max(value, Influence(position - (Vector2)allLights[i], allLights[i].z));
            return value;
        }
        public Color ColorAt(Vector2 position)
        {
            Color result = Color.black;
            for (int i = 0; i < count; i++)
            {
                Color tint = allColors[i] * Influence(position - (Vector2)allLights[i], allLights[i].z);
                result.r = Mathf.Max(result.r, tint.r); result.g = Mathf.Max(result.g, tint.g); result.b = Mathf.Max(result.b, tint.b);
            }
            return result;
        }
        public float SurfaceVisibility(Vector2 position)
        {
            if (owner?.Session == null || owner.Session.SecretRoomOpen) return 1;
            if (owner.Session.State.Phase == SessionPhase.Shop) return 1;
            var light = NightmareLighting.Active;
            if (light == null) return 1;
            return Mathf.Clamp(light.Ambient + Mathf.Max(Strength(position), NightLanternField.Active?.Illumination(position) ?? 0) * .9f + light.Spill(position).maxColorComponent * .3f, PresentationSettings.HighReadability ? .35f : .025f, 1);
        }
        private void OnDestroy()
        {
            if (Active == this) { Active = null; Shader.SetGlobalFloat("_SharedLightEnabled", 0); }
            if (field != null) { field.Release(); Destroy(field); }
            if (scratch != null) { scratch.Release(); Destroy(scratch); }
            if (painter != null) Destroy(painter);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Active = null; Shader.SetGlobalFloat("_SharedLightEnabled", 0); }
    }
}
