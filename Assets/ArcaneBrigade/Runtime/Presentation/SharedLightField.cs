using UnityEngine;

namespace ArcaneBrigade
{
    // One low-resolution world-space light field. Sixteen actors do not require sixteen
    // full-screen lights or a camera per actor. Combat telegraphs and UI are never masked.
    public sealed class SharedLightField : MonoBehaviour
    {
        public const int Capacity = 16;
        public const float HeroRadius = 6.4f;
        private ArenaController owner;
        private readonly Vector4[] lights = new Vector4[Capacity];
        private readonly Vector4[] colors = new Vector4[Capacity];
        private RenderTexture field;
        private Material painter;
        private int count;
        public int SourceCount => count;
        public static SharedLightField Active { get; private set; }
        public void Configure(ArenaController value)
        {
            owner = value;
            var source = Resources.Load<Material>("MienCho/SharedLight");
            if (source == null || source.shader == null || !source.shader.isSupported) return;
            painter = new Material(source);
            field = new RenderTexture(256, 192, 0, RenderTextureFormat.ARGB32) { name = "Shared companionship light", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            field.Create();
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
            return Color.HSVToRGB((hash % 1000) / 1000f, .26f, 1);
        }
        private void Add(Vector2 position, float radius, Color tint)
        {
            if (count >= Capacity) return;
            lights[count] = new Vector4(position.x, position.y, radius, 1);
            colors[count++] = tint;
        }
        private void LateUpdate()
        {
            if (owner?.Simulation == null || field == null) return;
            if (owner.PresentationPaused && count > 0) return;
            count = 0;
            Add(owner.VisualChampionPosition, HeroRadius, new Color(.68f, .91f, .87f));
            float interpolation = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            foreach (var unit in owner.Simulation.Units)
                if (unit.Alive && unit.Faction == Faction.Companion)
                    Add(Vector2.Lerp(unit.PreviousPosition, unit.Position, interpolation), 3.4f, IdentityColor(unit.Definition.Id));
            Vector2 size = owner.WorldBounds + Vector2.one * 12;
            painter.SetVectorArray("_PartyLights", lights);
            painter.SetVectorArray("_PartyColors", colors);
            painter.SetInt("_PartyCount", count);
            painter.SetVector("_PartyRect", new Vector4(-size.x / 2, -size.y / 2, size.x, size.y));
            var previousTarget=RenderTexture.active;
            try {Graphics.Blit(Texture2D.blackTexture,field,painter);}
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
            float minimum = unit.Definition.IsBoss ? .8f : PresentationSettings.HighReadability ? .62f : .16f;
            return Mathf.Lerp(1, Mathf.Lerp(minimum, 1, Mathf.SmoothStep(0, 1, light * 1.7f)), depth);
        }
        public float Strength(Vector2 position)
        {
            float value = 0;
            for (int i = 0; i < count; i++) value = Mathf.Max(value, Influence(position - (Vector2)lights[i], lights[i].z));
            return value;
        }
        public Color ColorAt(Vector2 position)
        {
            Color result = Color.black;
            for (int i = 0; i < count; i++)
            {
                Color tint = (Color)colors[i] * Influence(position - (Vector2)lights[i], lights[i].z);
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
            return Mathf.Clamp(light.Ambient + Mathf.Max(Strength(position), NightLanternField.Active?.Illumination(position) ?? 0) * .9f + light.Spill(position).maxColorComponent * .3f, PresentationSettings.HighReadability ? .35f : .1f, 1);
        }
        private void OnDestroy()
        {
            if (Active == this) { Active = null; Shader.SetGlobalFloat("_SharedLightEnabled", 0); }
            if (field != null) { field.Release(); Destroy(field); }
            if (painter != null) Destroy(painter);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Active = null; Shader.SetGlobalFloat("_SharedLightEnabled", 0); }
    }
}
