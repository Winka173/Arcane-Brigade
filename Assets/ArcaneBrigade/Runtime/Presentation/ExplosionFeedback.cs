using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Explosions you can feel: a warm flash, an inked shock ring at the real blast radius, paper-and-ink chips and
    /// a settling smudge of ink. Plays on Detonation events (fuse and echo blasts, hero area strikes), boss landings and
    /// boss deaths (and the closing blast of every boss finale), and every ordinary enemy kill as a short ink POP sized by
    /// the body, the overkill and the crit. Pops are pooled and rate-limited so hundreds of kills a second stay cheap.
    /// Presentation only: sizes come from the event, nothing here touches the simulation.</summary>
    public sealed class ExplosionFeedback : MonoBehaviour
    {
        private enum Kind { Blast, Burst, Smudge, Shock, Pop }
        private sealed class Blast
        {
            public Kind Kind;
            public Vector2 Position;
            public float Radius, Age = 10, Duration;
            public Color Color;
            public bool Important;
            public float Spin;
            public SpriteRenderer Flash, Ring, Rim, Core, Bloom;
            public readonly SpriteRenderer[] Chips = new SpriteRenderer[12];
            public readonly Vector2[] ChipVelocity = new Vector2[12];
            public readonly float[] ChipSpin = new float[12];
            public readonly SpriteRenderer[] Puffs = new SpriteRenderer[3];
        }
        private readonly List<Blast> pool = new List<Blast>();
        private Sprite glow, ring, chip, puff, white, spark;
        private Sprite[] deathBlast = System.Array.Empty<Sprite>();
        private Material bloom;
        private PaintedAnimationLibrary painted;
        public int ActiveCount { get; private set; }
        public const int Capacity = 120;
        /// <summary>Most enemy pops started per tenth of a second (the squashing body still shows for the rest).</summary>
        public const int PopsPerWindow = 26, ReducedPopsPerWindow = 9;
        private float popWindow;
        private int popsInWindow;
        public int PopsShown { get; private set; }
        public static ExplosionFeedback Active { get; private set; }
        private float skidWindow;
        private int skidsInWindow;
        private void OnDestroy() { if (Active == this) Active = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetActive() => Active = null;
        /// <summary>A puff of floor dust where a knocked-back body skids.</summary>
        public void Skid(Vector2 position, Vector2 direction)
        {
            if (PresentationSettings.ReducedEffects) return;
            if (Time.unscaledTime > skidWindow) { skidWindow = Time.unscaledTime + 0.1f; skidsInWindow = 0; }
            if (++skidsInWindow > 8) return;
            Spawn(Kind.Smudge, position - direction * 0.05f, 0.32f, new Color(0.66f, 0.58f, 0.47f), false);
        }

        private void Awake()
        {
            Active = this;
            glow = Make("ExplosionGlow", 128, (d, a) => Mathf.Pow(Mathf.Clamp01(1 - d), 1.6f));
            // the shock ring: a soft painted band with a darker inked outer edge
            ring = Make("ExplosionRing", 192, (d, a) => { float band = Mathf.Clamp01(1 - Mathf.Abs(d - 0.82f) / 0.16f); float ink = Mathf.Clamp01(1 - Mathf.Abs(d - 0.96f) / 0.04f); return Mathf.Max(band * band * 0.9f, ink); });
            chip = Make("ExplosionChip", 32, (d, a) => { float shape = 1 - Mathf.Clamp01((d * (1.1f + 0.35f * Mathf.Cos(a * 3))) - 0.45f) * 3; return Mathf.Clamp01(shape); });
            white = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * 0.5f, Texture2D.whiteTexture.width);
            bloom = Resources.Load<Material>("MienCho/WaitingGlow");
            painted = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            // a hot streak: a long thin diamond, brightest along its spine, drawn along each spark's flight
            spark = Make("ExplosionSpark", 64, (d, a) => { float x = Mathf.Abs(d * Mathf.Cos(a)), y = Mathf.Abs(d * Mathf.Sin(a)); return Mathf.Clamp01((1 - x - y * 3.4f) * 1.8f); });
            deathBlast = LoadDeathBlast();
            puff = Make("ExplosionSmudge", 96, (d, a) => { float wobble = 1 + 0.12f * Mathf.Sin(a * 5) + 0.06f * Mathf.Cos(a * 9); return Mathf.Clamp01((1 - d * wobble) * 1.6f) * (0.65f + 0.35f * Mathf.Clamp01(1 - d * 1.4f)); });
        }
        /// <summary>The painted golden blast every ordinary kill goes up in (Resources/Effects/DeathBlast_f0..3, one world
        /// unit wide). Loaded as textures so the import type never matters; empty when the art is missing.</summary>
        private static Sprite[] LoadDeathBlast()
        {
            var frames = new List<Sprite>();
            for (int i = 0; i < 4; i++)
            {
                var texture = Resources.Load<Texture2D>("Effects/DeathBlast_f" + i);
                if (texture == null) break;
                frames.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width, 0, SpriteMeshType.FullRect));
            }
            return frames.ToArray();
        }
        private static Sprite Make(string name, int size, System.Func<float, float, float> alpha)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half, dy = (y - half) / half;
                    float value = Mathf.Clamp01(alpha(Mathf.Sqrt(dx * dx + dy * dy), Mathf.Atan2(dy, dx)));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(value * 255));
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
        }

        public void Clear()
        {
            foreach (Blast blast in pool) { blast.Age = blast.Duration; Hide(blast); }
            ActiveCount = 0;
        }

        /// <summary>Called by the arena for each event; deathShown tells whether a death remnant was available.</summary>
        public void Observe(CombatEvent value, bool deathShown)
        {
            bool boss = value.Subject != null && value.Subject.Definition != null && value.Subject.Definition.IsBoss;
            switch (value.Kind)
            {
                case CombatEventKind.Detonation:
                    Spawn(Kind.Blast, value.Position, Mathf.Max(0.8f, value.Value), Tint(value.Element, value.Faction), true);
                    break;
                case CombatEventKind.BossLanded:
                    Spawn(Kind.Shock, value.Position, 2.6f, new Color(0.86f, 0.74f, 0.55f), true);
                    break;
                case CombatEventKind.Kill when boss:
                    Spawn(Kind.Shock, value.Position, 4.4f, new Color(1f, 0.92f, 0.74f), true);
                    Spawn(Kind.Blast, value.Position, 2.2f, new Color(1f, 0.82f, 0.48f), true);
                    break;
                case CombatEventKind.Kill when value.Faction == Faction.Enemy:
                    Pop(value.Subject?.Definition, value.Position, value.Critical, value.Value, value.Element, deathShown);
                    break;
            }
        }
        /// <summary>An ordinary enemy goes pop: a brief white flash, an ink burst sized by the body (bigger for overkill
        /// and crits), a thin snap ring, a spray of ink and paper chips and a small puff of smoke. ~0.35 s, no lingering.</summary>
        public void Pop(UnitDefinition unit, Vector2 position, bool critical, float overkill, DamageElement element, bool bodyShown = true)
        {
            if (Time.unscaledTime > popWindow) { popWindow = Time.unscaledTime + 0.1f; popsInWindow = 0; }
            bool budget = ++popsInWindow <= (PresentationSettings.ReducedEffects ? ReducedPopsPerWindow : PopsPerWindow);
            // a crit always lands; when no body could be shown the burst has to carry the death on its own
            if (!budget && !critical && bodyShown) return;
            float height = unit != null ? Mathf.Max(0.3f, unit.BodyHeight) : 0.8f;
            float force = 1f + (critical ? 0.45f : 0f) + Mathf.Clamp(overkill, 0f, 1.5f) * 0.35f;
            float radius = Mathf.Clamp(height * 0.6f, 0.36f, 1.15f) * force;
            Vector2 centre = position + Vector2.up * height * 0.3f;
            Color tint = critical ? new Color(1f, 0.93f, 0.7f) : Color.Lerp(new Color(1f, 0.86f, 0.62f), Tint(element, Faction.Companion), 0.35f);
            if (Spawn(Kind.Pop, centre, radius, tint, critical || !bodyShown)) PopsShown++;
        }
        /// <summary>The end of a boss finale: the body goes up in one big blast with a ground shock.</summary>
        public void BossFinale(Vector2 position, float height)
        {
            float size = Mathf.Clamp(height, 1.2f, 4f);
            Spawn(Kind.Shock, position, size * 1.6f, new Color(1f, 0.9f, 0.7f), true);
            Spawn(Kind.Blast, position, size * 0.95f, new Color(1f, 0.74f, 0.4f), true);
            Spawn(Kind.Pop, position + new Vector2(-0.3f, 0.2f) * size, size * 0.45f, new Color(1f, 0.86f, 0.6f), true);
            Spawn(Kind.Pop, position + new Vector2(0.35f, -0.1f) * size, size * 0.4f, new Color(1f, 0.8f, 0.5f), true);
        }
        private static Color Tint(DamageElement element, Faction faction)
        {
            if (faction == Faction.Enemy) return new Color(0.92f, 0.3f, 0.22f); // hostile blasts keep the ember red
            return element switch
            {
                DamageElement.Ember => new Color(1f, 0.55f, 0.24f),
                DamageElement.Frost => new Color(0.62f, 0.9f, 1f),
                DamageElement.Arcane => new Color(0.82f, 0.62f, 1f),
                _ => new Color(1f, 0.8f, 0.42f)
            };
        }

        private Blast Acquire(bool important)
        {
            Blast blast = pool.Find(candidate => candidate.Age >= candidate.Duration);
            if (blast != null) return blast;
            if (pool.Count >= Capacity)
            {
                if (!important) return null;
                Blast oldest = null;
                foreach (Blast candidate in pool) if (!candidate.Important && (oldest == null || candidate.Age > oldest.Age)) oldest = candidate;
                return oldest;
            }
            blast = new Blast();
            blast.Puffs[0] = Part("Smudge0", puff, 2430); blast.Puffs[1] = Part("Smudge1", puff, 2430); blast.Puffs[2] = Part("Smudge2", puff, 2430);
            blast.Rim = Part("InkRim", ring, 2441); blast.Ring = Part("ShockRing", ring, 2442); blast.Flash = Part("Flash", glow, 2450);
            blast.Core = Part("PaintedCore", null, 2448);
            blast.Bloom = Part("Bloom", white, 2452); if (bloom != null) blast.Bloom.sharedMaterial = bloom;
            for (int i = 0; i < blast.Chips.Length; i++) blast.Chips[i] = Part("Chip" + i, chip, 2460);
            pool.Add(blast);
            return blast;
        }
        private SpriteRenderer Part(string name, Sprite sprite, int order)
        {
            var child = new GameObject("Explosion" + name); child.transform.SetParent(transform, false);
            var view = child.AddComponent<SpriteRenderer>(); view.sprite = sprite; view.sortingOrder = order; view.enabled = false;
            return view;
        }

        private bool Spawn(Kind kind, Vector2 position, float radius, Color color, bool important)
        {
            Blast blast = Acquire(important);
            if (blast == null) return false;
            if (blast.Age < blast.Duration) Hide(blast); // stolen from a quieter effect: clear what it was showing
            blast.Kind = kind; blast.Position = position; blast.Radius = radius; blast.Color = color; blast.Important = important; blast.Age = 0;
            blast.Duration = kind == Kind.Shock ? 0.75f : kind == Kind.Smudge ? 0.6f : kind == Kind.Burst ? 0.38f : kind == Kind.Pop ? PopDuration : 0.55f;
            int seed = Mathf.Abs(Mathf.RoundToInt(position.x * 97 + position.y * 31)) + pool.IndexOf(blast) * 13;
            int spread = kind == Kind.Pop ? PopChips : blast.Chips.Length;
            for (int i = 0; i < blast.Chips.Length; i++)
            {
                float angle = (i + (seed % 7) * 0.13f) * Mathf.PI * 2f / spread + Mathf.Sin(seed + i) * 0.35f;
                float speed = (kind == Kind.Burst ? 2.4f : kind == Kind.Pop ? 4.6f : 3.4f) * Mathf.Sqrt(radius) * (0.7f + 0.3f * Mathf.Abs(Mathf.Sin(seed * 0.37f + i * 1.7f)));
                blast.ChipVelocity[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                blast.ChipSpin[i] = (i % 2 == 0 ? 1 : -1) * (360 + 180 * Mathf.Abs(Mathf.Sin(seed + i)));
                blast.Chips[i].color = i % 3 == 0 ? new Color(0.13f, 0.09f, 0.14f) : i % 3 == 1 ? Color.Lerp(color, Color.white, 0.35f) : new Color(0.93f, 0.86f, 0.72f);
            }
            blast.Spin = (seed % 360) * 1f;
            Draw(blast);
            return true;
        }
        public const float PopDuration = 0.44f;
        private const int PopChips = 8;
        /// <summary>An ordinary kill goes up in a small golden explosion: a white-hot snap, the painted blast punching past
        /// full size and settling, a snap ring, hot sparks streaking out and dragging to a stop, then one curl of smoke.
        /// No debris of any kind (no crockery, wood or metal chips). All from the shared pool, gone in under half a second.</summary>
        private void DrawPop(Blast blast)
        {
            float age = blast.Age, r = blast.Radius;
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float flashGain = PresentationSettings.ReducedFlashes ? 0.3f : 1f;
            blast.Rim.enabled = false;
            // the snap of the hit: a white-hot point blowing out in 60 ms
            float flashT = Mathf.Clamp01(age / 0.06f);
            blast.Flash.enabled = flashT < 1f;
            if (blast.Flash.enabled)
            {
                blast.Flash.sprite = glow;
                blast.Flash.transform.position = blast.Position;
                blast.Flash.transform.localScale = Vector3.one * (2f * r * Mathf.Lerp(0.6f, 1.4f, Ease(flashT)));
                blast.Flash.color = new Color(1f, 0.98f, 0.9f, (1 - flashT * flashT) * flashGain);
            }
            // warm light spilling over the crowd (stronger for crits and overkill)
            float bloomT = Mathf.Clamp01(age / (blast.Important ? 0.22f : 0.17f));
            blast.Bloom.enabled = bloom != null && bloomT < 1f && !sparse;
            if (blast.Bloom.enabled)
            {
                blast.Bloom.transform.position = blast.Position;
                blast.Bloom.transform.localScale = Vector3.one * (2.8f * r * Mathf.Lerp(0.7f, 1.1f, Ease(bloomT)));
                blast.Bloom.color = new Color(1f, 0.78f, 0.42f, (1 - bloomT) * (blast.Important ? 0.75f : 0.5f) * flashGain);
            }
            // the fireball: four painted frames in 0.3 s, punching past full size, holding, then burning out
            bool paintedBlast = deathBlast.Length > 0;
            Sprite[] frames = paintedBlast ? deathBlast : painted == null ? null : painted.Death;
            float coreT = Mathf.Clamp01(age / 0.3f);
            blast.Core.enabled = frames != null && frames.Length > 0 && coreT < 1f;
            if (blast.Core.enabled)
            {
                Sprite frame = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(coreT * frames.Length))];
                blast.Core.sprite = frame;
                blast.Core.transform.position = blast.Position;
                float punch = still ? 1f : coreT < 0.25f ? Mathf.Lerp(0.55f, 1.18f, Ease(coreT / 0.25f)) : Mathf.Lerp(1.18f, 1.05f, (coreT - 0.25f) / 0.75f);
                float inkWidth = paintedBlast ? 0.93f : Mathf.Max(0.01f, SpriteInkMetrics.BoundsOf(frame).width);
                blast.Core.transform.localScale = Vector3.one * (2.3f * r * punch / inkWidth);
                blast.Core.transform.rotation = Quaternion.Euler(0, 0, still ? 0 : blast.Spin % 40f - 20f);
                float alpha = Mathf.Clamp01((1 - coreT) * 2.5f);
                Color tint = paintedBlast ? Color.white : Color.Lerp(Color.white, blast.Color, 0.25f);
                if (PresentationSettings.ReducedFlashes) tint = Color.Lerp(tint, new Color(0.85f, 0.78f, 0.7f), 0.3f);
                blast.Core.color = new Color(tint.r, tint.g, tint.b, alpha);
            }
            // a thin ring snaps out once
            float ringT = Mathf.Clamp01(age / 0.2f);
            float ringAlpha = Mathf.Pow(1 - ringT, 1.5f) * 0.8f;
            blast.Ring.enabled = ringAlpha > 0.02f;
            if (blast.Ring.enabled)
            {
                blast.Ring.sprite = ring;
                blast.Ring.transform.position = blast.Position;
                blast.Ring.transform.localScale = Vector3.one * (2f * r * (still ? 1.2f : Mathf.Lerp(0.5f, 1.5f, Ease(ringT))));
                blast.Ring.color = new Color(1f, 0.9f, 0.62f, ringAlpha);
            }
            // hot sparks: white, then gold, then ember, streaking along their flight and dragging to a stop
            int shown = still ? 0 : sparse ? 3 : PopChips;
            float sparkT = Mathf.Clamp01(age / 0.32f);
            for (int i = 0; i < blast.Chips.Length; i++)
            {
                SpriteRenderer view = blast.Chips[i];
                view.enabled = i < shown && sparkT < 1f;
                if (!view.enabled) continue;
                view.sprite = spark;
                Vector2 velocity = blast.ChipVelocity[i];
                float travel = (1 - Mathf.Pow(1 - sparkT, 3f)) * 0.2f;
                view.transform.position = blast.Position + velocity * travel;
                view.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
                float speed = (1 - sparkT) * (1 - sparkT), size = Mathf.Sqrt(r);
                view.transform.localScale = new Vector3(Mathf.Lerp(0.1f, 0.62f, speed) * size, 0.1f * size * (1 - sparkT * 0.5f), 1);
                Color hot = sparkT < 0.3f ? Color.Lerp(new Color(1f, 1f, 0.92f), new Color(1f, 0.86f, 0.36f), sparkT / 0.3f)
                    : Color.Lerp(new Color(1f, 0.86f, 0.36f), new Color(1f, 0.42f, 0.16f), (sparkT - 0.3f) / 0.7f);
                view.color = new Color(hot.r, hot.g, hot.b, 1 - sparkT * sparkT);
            }
            // one curl of smoke once the fire is out
            float puffT = Mathf.Clamp01((age - 0.14f) / (blast.Duration - 0.14f));
            for (int i = 0; i < blast.Puffs.Length; i++)
            {
                SpriteRenderer view = blast.Puffs[i];
                view.enabled = i == 0 && puffT > 0 && puffT < 1;
                if (!view.enabled) continue;
                view.sprite = puff;
                view.transform.position = blast.Position + (still ? Vector2.zero : Vector2.up * puffT * 0.3f * r);
                view.transform.localScale = Vector3.one * (2f * r * (still ? 0.9f : Mathf.Lerp(0.6f, 1.1f, Ease(puffT))));
                view.color = new Color(0.16f, 0.11f, 0.17f, (1 - puffT) * 0.42f);
            }
        }

        public void Advance(float delta)
        {
            if (delta <= 0 || !float.IsFinite(delta)) return;
            ActiveCount = 0;
            foreach (Blast blast in pool)
            {
                if (blast.Age >= blast.Duration) continue;
                blast.Age += delta;
                if (blast.Age >= blast.Duration) { Hide(blast); continue; }
                ActiveCount++;
                Draw(blast);
            }
        }

        private void Draw(Blast blast)
        {
            if (blast.Kind == Kind.Pop) { DrawPop(blast); return; }
            float t = Mathf.Clamp01(blast.Age / blast.Duration);
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float flashGain = PresentationSettings.ReducedFlashes ? 0.35f : 1f;
            float r = blast.Radius;
            // flash: a hot core that blooms to the blast edge and is gone within a tenth of a second
            float flashT = Mathf.Clamp01(blast.Age / (blast.Kind == Kind.Shock ? 0.2f : 0.15f));
            bool flash = blast.Kind != Kind.Smudge && flashT < 1f;
            blast.Flash.enabled = flash;
            if (flash)
            {
                blast.Flash.transform.position = blast.Position;
                blast.Flash.transform.localScale = Vector3.one * (2f * r * Mathf.Lerp(0.55f, 1.05f, Ease(flashT)));
                blast.Flash.color = new Color(Mathf.Lerp(1f, blast.Color.r, flashT), Mathf.Lerp(0.97f, blast.Color.g, flashT), Mathf.Lerp(0.9f, blast.Color.b, flashT), (1 - flashT * flashT) * flashGain);
            }
            // bloom: the light of the blast spilling over everything nearby
            float bloomT = Mathf.Clamp01(blast.Age / (blast.Kind == Kind.Shock ? 0.35f : 0.24f));
            blast.Bloom.enabled = blast.Kind != Kind.Smudge && bloom != null && bloomT < 1f;
            if (blast.Bloom.enabled)
            {
                blast.Bloom.transform.position = blast.Position;
                blast.Bloom.transform.localScale = Vector3.one * (2.8f * r * Mathf.Lerp(0.7f, 1.1f, Ease(bloomT)));
                blast.Bloom.color = new Color(blast.Color.r, blast.Color.g * 0.92f, blast.Color.b * 0.8f, (1 - bloomT) * 0.75f * flashGain);
            }
            // a painted core: the game's own slam-dust (or death puff for kill bursts) blown up to the blast
            Sprite[] frames = painted == null ? null : blast.Kind == Kind.Burst ? painted.Death : painted.Dust;
            float coreLife = blast.Kind == Kind.Shock ? 0.5f : 0.36f;
            float coreT = Mathf.Clamp01(blast.Age / coreLife);
            blast.Core.enabled = blast.Kind != Kind.Smudge && frames != null && frames.Length > 0 && coreT < 1f;
            if (blast.Core.enabled)
            {
                Sprite frame = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(coreT * frames.Length))];
                blast.Core.sprite = frame;
                blast.Core.transform.position = blast.Position;
                float coreWidth = 2f * r * (blast.Kind == Kind.Burst ? 0.9f : 0.95f) * Mathf.Lerp(0.75f, 1.05f, Ease(coreT));
                blast.Core.transform.localScale = Vector3.one * (coreWidth / Mathf.Max(0.01f, frame.bounds.size.x));
                blast.Core.transform.rotation = Quaternion.Euler(0, 0, blast.Position.x * 47f % 360f);
                Color tint = Color.Lerp(Color.white, blast.Color, 0.35f);
                blast.Core.color = new Color(tint.r, tint.g, tint.b, Mathf.Clamp01((1 - coreT) * 1.6f));
            }
            // shock ring and its inked rim: out to just past the real radius, easing out
            bool ringed = blast.Kind != Kind.Smudge;
            float ringT = Mathf.Clamp01(blast.Age / (blast.Kind == Kind.Shock ? 0.55f : 0.36f));
            float ringScale = 2f * r * (still ? 1.05f : Mathf.Lerp(0.35f, 1.15f, Ease(ringT)));
            float ringAlpha = ringed ? Mathf.Pow(1 - ringT, 1.4f) : 0;
            blast.Ring.enabled = blast.Rim.enabled = ringAlpha > 0.01f;
            if (blast.Ring.enabled)
            {
                blast.Ring.transform.position = blast.Rim.transform.position = blast.Position;
                blast.Ring.transform.localScale = Vector3.one * ringScale;
                blast.Rim.transform.localScale = Vector3.one * ringScale * 1.03f;
                blast.Ring.color = new Color(Mathf.Lerp(blast.Color.r, 1f, 0.3f), Mathf.Lerp(blast.Color.g, 1f, 0.3f), Mathf.Lerp(blast.Color.b, 1f, 0.3f), ringAlpha);
                blast.Rim.color = new Color(0.11f, 0.07f, 0.12f, ringAlpha * 0.7f);
            }
            // chips: paper, ink and coloured flecks thrown outward, slowing and spinning, then gone
            bool chips = blast.Kind != Kind.Smudge && !still && !sparse;
            float chipT = Mathf.Clamp01(blast.Age / (blast.Duration * 0.85f));
            for (int i = 0; i < blast.Chips.Length; i++)
            {
                SpriteRenderer view = blast.Chips[i];
                view.enabled = chips && chipT < 1f && (blast.Kind != Kind.Burst || i < 7);
                if (!view.enabled) continue;
                float travel = (1 - Mathf.Pow(1 - chipT, 2.2f)) * blast.Duration * 0.55f;
                Vector2 at = blast.Position + blast.ChipVelocity[i] * travel + Vector2.up * Mathf.Sin(chipT * Mathf.PI) * 0.18f * Mathf.Sqrt(r);
                view.transform.position = at;
                view.transform.rotation = Quaternion.Euler(0, 0, blast.ChipSpin[i] * blast.Age);
                float size = (blast.Kind == Kind.Burst ? 0.18f : 0.26f) * Mathf.Sqrt(r) * (1 - chipT * 0.6f);
                view.transform.localScale = new Vector3(size * 1.5f, size, 1);
                Color c = view.color; view.color = new Color(c.r, c.g, c.b, 1 - chipT * chipT);
            }
            // a smudge of ink settles where it happened
            int puffs = blast.Kind == Kind.Burst ? 1 : sparse ? 1 : 3;
            float puffT = Mathf.Clamp01((blast.Age - (blast.Kind == Kind.Smudge ? 0 : 0.05f)) / (blast.Duration * 0.95f));
            for (int i = 0; i < blast.Puffs.Length; i++)
            {
                SpriteRenderer view = blast.Puffs[i];
                view.enabled = i < puffs && puffT > 0 && puffT < 1;
                if (!view.enabled) continue;
                float angle = i * 2.1f + blast.Position.x;
                Vector2 offset = i == 0 ? Vector2.zero : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r * 0.35f;
                view.transform.position = blast.Position + offset * (still ? 1 : Ease(puffT));
                view.transform.localScale = Vector3.one * (2f * r * (i == 0 ? 0.7f : 0.45f) * (still ? 1 : Mathf.Lerp(0.6f, 1.1f, Ease(puffT))));
                Color smudge = blast.Kind == Kind.Smudge ? blast.Color : new Color(0.14f, 0.1f, 0.17f);
                view.color = new Color(smudge.r, smudge.g, smudge.b, (1 - puffT) * (blast.Kind == Kind.Smudge ? 0.55f : 0.32f));
            }
        }
        private static float Ease(float t) => 1 - Mathf.Pow(1 - Mathf.Clamp01(t), 3);
        private static void Hide(Blast blast)
        {
            blast.Flash.enabled = blast.Ring.enabled = blast.Rim.enabled = blast.Core.enabled = blast.Bloom.enabled = false;
            foreach (SpriteRenderer view in blast.Chips) view.enabled = false;
            foreach (SpriteRenderer view in blast.Puffs) view.enabled = false;
        }
    }
}
