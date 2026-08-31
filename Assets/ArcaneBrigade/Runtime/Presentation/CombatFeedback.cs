using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CombatFeedback : MonoBehaviour
    {
        private float lastDodgeWord = -10;
        private sealed class Spark
        {
            public SpriteRenderer View;
            public Vector2 Position;
            public Vector2 Velocity;
            public Color Color;
            public float Age;
            public float Duration;
            public float Size;
            public bool Important;
            public Vector2 Shape;
            public Sprite[] Frames;   // painted spark frames (AllySpark / HeroHurt), played over the spark's short life
            public float Ink = 1;
        }

        private readonly List<Spark> sparks = new List<Spark>();
        private sealed class DamageNumber
        {
            public TextMesh View;
            public Vector2 Position;
            public Color Color;
            public float Age = 1f;
            public bool Champion;
            public TextMesh Outline;
            public TextMesh[] Outlines = new TextMesh[0]; // four-way ink outline plus a drop shadow
            public Vector2 Velocity;
            public float Scale = 1f;
            public int SubjectId;
            public float Amount;
            public bool Critical;
            public float Life = 0.55f;
            public float Shake;   // a long-range critical's number trembles (RangedFeel.LongShot)
            public int Kind;      // 0 damage, 1 healing, 2 a word or a stat (rises gently instead of being thrown)
        }
        private readonly List<DamageNumber> numbers = new List<DamageNumber>();
        private Font font;
        private Sprite sprite;
        private int sequence;
        private TactileFeedback tactile;
        public void Configure(Sprite source)
        {
            sprite = source; font = ArcadeTypography.Numbers != null ? ArcadeTypography.Numbers : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (tactile == null) tactile = gameObject.AddComponent<TactileFeedback>();
            tactile.Configure();
        }

        public void Clear()
        {
            foreach (Spark spark in sparks) { spark.Age = spark.Duration; spark.View.gameObject.SetActive(false); }
            foreach (DamageNumber number in numbers) { number.Age = 1f; number.View.gameObject.SetActive(false); }
            sequence = 0;
            if (tactile != null) tactile.Clear();
        }

        public void Play(CombatEvent combatEvent)
        {
            if (tactile != null) tactile.Play(combatEvent);
            bool hit = combatEvent.Kind == CombatEventKind.Hit;
            bool kill = combatEvent.Kind == CombatEventKind.Kill;
            bool pickup = combatEvent.Kind == CombatEventKind.Pickup;
            if ((hit || kill) && combatEvent.Faction == Faction.Companion) return;
            if (hit && combatEvent.Value > 0f) ShowDamage(combatEvent);
            // requirement 24/10/2026: healing reads as a green +N over the hero, a dodge as a small word
            if (combatEvent.Kind == CombatEventKind.Heal && combatEvent.Faction == Faction.Champion && combatEvent.Value >= .5f) ShowHeal(combatEvent);
            // one "Dodge" at a time: a burst of dodged hits shows the word once, not stacked
            if (combatEvent.Kind == CombatEventKind.Dodge && combatEvent.Faction == Faction.Champion && Time.unscaledTime - lastDodgeWord > .6f) { lastDodgeWord = Time.unscaledTime; Label(combatEvent.Position + Vector2.up * .7f, "Dodge", new Color(.55f, 1f, .9f), .62f); }
            if (sprite == null || PresentationSettings.ReducedMotion || (!hit && !kill && !pickup) || hit && combatEvent.Value <= 0) return;
            bool important = combatEvent.Faction == Faction.Champion || combatEvent.Critical;
            bool heavy = combatEvent.Weapon != null && (combatEvent.Weapon.Motion == WeaponMotion.Hammer || combatEvent.Weapon.Motion == WeaponMotion.Slam);
            int count = kill ? combatEvent.Critical ? 10 : 7 : pickup ? 2 : heavy ? 6 : important ? 5 : 3;
            if (PresentationSettings.ReducedEffects && !important) count = Mathf.Max(1, count / 2);
            Color color = pickup ? new Color(1f, 0.78f, 0.3f) : combatEvent.Faction == Faction.Champion && hit ? new Color(1f, 0.34f, 0.33f) : new Color(1f, 0.94f, 0.75f);
            if (hit && combatEvent.Faction != Faction.Champion)
            {
                if (combatEvent.Element == DamageElement.Ember) color = new Color(1, 0.48f, 0.23f);
                else if (combatEvent.Element == DamageElement.Frost) color = new Color(0.55f, 0.9f, 1);
                else if (combatEvent.Element == DamageElement.Arcane) color = new Color(0.85f, 0.64f, 1);
            }
            for (int index = 0; index < count; index++)
            {
                Spark spark = null;
                foreach (Spark candidate in sparks) if (candidate.Age >= candidate.Duration) { spark = candidate; break; }
                if (spark == null)
                {
                    if (sparks.Count >= EffectDensity.Limit(96))
                    {
                        if (!important) break;
                        spark = sparks.Find(candidate => !candidate.Important);
                        if (spark == null) break;
                    }
                }
                if (spark == null)
                {
                    var child = new GameObject("ImpactSpark");
                    child.transform.SetParent(transform, false);
                    spark = new Spark { View = child.AddComponent<SpriteRenderer>() };
                    spark.View.sprite = sprite;
                    spark.View.sortingOrder = 2500;
                    sparks.Add(spark);
                }
                float angle = hit && combatEvent.Direction.sqrMagnitude > 0.001f
                    ? Mathf.Atan2(combatEvent.Direction.y, combatEvent.Direction.x) + (index - (count - 1) * 0.5f) * 0.45f
                    : index * Mathf.PI * 2f / count + sequence * 0.73f;
                spark.Position = combatEvent.Position;
                spark.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (kill ? 2.4f : 1.4f);
                spark.Color = color;
                spark.Age = 0f;
                spark.Duration = kill ? 0.34f : 0.20f;
                spark.Size = kill ? 0.09f : 0.065f;
                spark.Important = important;
                // a painted spark, not a flat disc: brass motes for our hits, the hurt splash when the hero is struck
                // the hero's hurt is a code-drawn spark in its own colour (requirement 22/10/2026: no painted red splash on the hostile side)
                spark.Frames = combatEvent.Faction == Faction.Champion && hit ? null : CombatVfxArtwork.Frames("AllySpark");
                Sprite first = spark.Frames != null && spark.Frames.Length > 0 ? spark.Frames[(sequence + index) % spark.Frames.Length] : sprite;
                spark.View.sprite = first;
                spark.Ink = Mathf.Max(0.01f, spark.Frames != null ? SpriteInkMetrics.BoundsOf(first).width : sprite.bounds.size.x);
                if (spark.Frames != null) { spark.Size *= 2.6f; spark.Color = Color.Lerp(Color.white, color, 0.45f); }
                bool sliver = hit && (combatEvent.Weapon == null || combatEvent.Weapon.Kind == WeaponKind.Melee || combatEvent.Weapon.Motion == WeaponMotion.Bow);
                spark.Shape = sliver ? new Vector2(1.7f, 0.45f) : Vector2.one;
                spark.View.transform.position = spark.Position;
                spark.View.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                spark.View.transform.localScale = new Vector3(spark.Shape.x, spark.Shape.y, 1) * (spark.Size / spark.Ink);
                spark.View.color = spark.Color;
                spark.View.gameObject.SetActive(true);
            }
            sequence++;
        }

        /// <summary>The element colour of a hit's source, when it has one (set by the rules ink, which knows the bonds).</summary>
        public static System.Func<CombatEvent, Color?> ElementTint;

        private void ShowDamage(CombatEvent combatEvent)
        {
            if (!PresentationSettings.DamageNumbers) return;
            if (combatEvent.SubjectId != 0)
            {
                DamageNumber existing = numbers.Find(candidate => candidate.Kind == 0 && candidate.Age < 0.3f && candidate.SubjectId == combatEvent.SubjectId && candidate.Critical == combatEvent.Critical);
                if (existing != null)
                {
                    existing.Amount += combatEvent.Value;
                    existing.View.text = Mathf.CeilToInt(existing.Amount) + (existing.Critical ? "!" : "");
                    foreach (TextMesh outline in existing.Outlines) outline.text = existing.View.text;
                    existing.Age = Mathf.Min(existing.Age, 0.04f); // a merged number pops again
                    if (RangedFeel.LongShot(combatEvent)) existing.Shake = 1;
                    return;
                }
            }
            bool champion = combatEvent.Faction == Faction.Champion;
            // Readability: ordinary companion/champion hits stay small and few; crits and damage taken always read.
            // requirement 25/10/2026: the numbers are the satisfaction - many more ordinary hits stay shown (was 12)
            if (!champion && !combatEvent.Critical && numbers.Count(candidate => candidate.Age < candidate.Life && !candidate.Critical && !candidate.Champion) >= 28) return;
            DamageNumber number = Take(champion);
            if (number == null) return;
            number.Kind = 0;
            number.Champion = champion;
            number.SubjectId = combatEvent.SubjectId; number.Amount = combatEvent.Value; number.Critical = combatEvent.Critical;
            number.Position = combatEvent.Position + new Vector2((sequence % 5 - 2) * 0.16f, 0.5f + (sequence % 3) * 0.14f); // fanned so fast repeats do not stack
            // size and warmth follow how much of the target's life the hit took
            float maxHealth = combatEvent.Subject != null && combatEvent.Subject.Definition != null ? combatEvent.Subject.Definition.Health : 100f;
            float weight = Mathf.Sqrt(Mathf.Clamp01(combatEvent.Value / Mathf.Max(1f, maxHealth)));
            number.Color = champion ? new Color(1f, 0.4f, 0.35f) : combatEvent.Critical ? new Color(1f, 0.85f, 0.22f) : Color.Lerp(Color.white, new Color(1f, 0.74f, 0.42f), Mathf.Clamp01((weight - 0.45f) * 2f));
            // our damage reads in the element that dealt it (fire orange, frost blue, venom green...); a crit is that colour, brighter
            if (!champion && ElementTint != null && ElementTint(combatEvent) is Color element) number.Color = combatEvent.Critical ? Color.Lerp(element, Color.white, .3f) : element;
            number.Age = 0f;
            number.Life = champion ? 0.75f : combatEvent.Critical ? 0.9f : 0.6f;
            float size = (champion ? 1.1f : combatEvent.Critical ? 1.45f : 0.78f) * (1f + (champion ? 0 : 0.85f * weight));
            number.Shake = !champion && RangedFeel.LongShot(combatEvent) ? 1 : 0;
            if (number.Shake > 0) size *= 1.35f; // a long shot's critical reads bigger
            number.Scale = size;
            float side = ((sequence % 5) - 2) * 0.28f + (combatEvent.Direction.x != 0 ? Mathf.Sign(combatEvent.Direction.x) * 0.25f : 0);
            number.Velocity = new Vector2(side, combatEvent.Critical ? 2.6f : 2.1f);
            number.View.characterSize = 0.055f * size;
            number.View.text = Mathf.CeilToInt(combatEvent.Value) + (combatEvent.Critical ? "!" : "");
            foreach (TextMesh outline in number.Outlines) { outline.characterSize = number.View.characterSize; outline.text = number.View.text; outline.color = new Color(0.06f, 0.035f, 0.10f); }
            number.View.color = number.Color;
            number.View.transform.position = new Vector3(number.Position.x, number.Position.y, -2f);
            number.View.transform.localScale = Vector3.one;
            number.View.gameObject.SetActive(true);
        }

        /// <summary>A free number from the pool (a priority one may take an ordinary one's place), or a new one.</summary>
        private DamageNumber Take(bool priority)
        {
            DamageNumber number = numbers.Find(candidate => candidate.Age >= candidate.Life);
            if (number == null && numbers.Count >= 64)
            {
                if (!priority) return null;
                number = numbers.Find(candidate => !candidate.Champion) ?? numbers[0];
            }
            if (number != null) return number;
            {
                var child = new GameObject("DamageNumber");
                child.transform.SetParent(transform, false);
                TextMesh view = child.AddComponent<TextMesh>();
                view.font = font;
                view.fontSize = 48;
                view.characterSize = 0.055f;
                view.anchor = TextAnchor.MiddleCenter;
                view.fontStyle = FontStyle.Bold;
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = font.material;
                renderer.sortingOrder = 2600;
                Vector3[] offsets = { new Vector3(0.022f, 0, 0.01f), new Vector3(-0.022f, 0, 0.01f), new Vector3(0, 0.022f, 0.01f), new Vector3(0, -0.022f, 0.01f), new Vector3(0.03f, -0.04f, 0.02f) };
                var outlines = new TextMesh[offsets.Length];
                for (int o = 0; o < offsets.Length; o++)
                {
                    var outlineObject = new GameObject(o < 4 ? "DamageOutline" : "DamageShadow");
                    outlineObject.transform.SetParent(child.transform, false);
                    outlineObject.transform.localPosition = offsets[o];
                    TextMesh outline = outlineObject.AddComponent<TextMesh>();
                    outline.font = font;
                    outline.fontSize = view.fontSize;
                    outline.characterSize = view.characterSize;
                    outline.anchor = view.anchor;
                    outline.fontStyle = view.fontStyle;
                    MeshRenderer outlineRenderer = outlineObject.GetComponent<MeshRenderer>();
                    outlineRenderer.sharedMaterial = font.material;
                    outlineRenderer.sortingOrder = o < 4 ? 2599 : 2598;
                    outlines[o] = outline;
                }
                number = new DamageNumber { View = view, Outline = outlines[0], Outlines = outlines };
                numbers.Add(number);
            }
            return number;
        }

        private void ShowHeal(CombatEvent combatEvent)
        {
            if (!PresentationSettings.DamageNumbers) return;
            DamageNumber merged = numbers.Find(candidate => candidate.Kind == 1 && candidate.Age < .45f);
            if (merged != null)
            {
                merged.Amount += combatEvent.Value; merged.View.text = "+" + Mathf.CeilToInt(merged.Amount);
                foreach (TextMesh outline in merged.Outlines) outline.text = merged.View.text;
                merged.Age = Mathf.Min(merged.Age, .04f); return;
            }
            DamageNumber number = Word(combatEvent.Position + Vector2.up * .75f, "+" + Mathf.CeilToInt(combatEvent.Value), new Color(.45f, 1f, .55f), .95f);
            if (number != null) { number.Kind = 1; number.Amount = combatEvent.Value; }
        }

        /// <summary>A short word or stat rising gently over a spot (a dodge, healing, a stat gained in a wave).</summary>
        public void Label(Vector2 at, string text, Color color, float size, float life = .9f) => Word(at, text, color, size, life);
        private DamageNumber Word(Vector2 at, string text, Color color, float size, float life = .9f)
        {
            if (!PresentationSettings.DamageNumbers || string.IsNullOrEmpty(text)) return null;
            DamageNumber number = Take(true); if (number == null) return null;
            number.Kind = 2; number.Champion = true; number.SubjectId = 0; number.Amount = 0; number.Critical = false; number.Shake = 0;
            // labels arriving together stack upward instead of on top of one another
            int near = 0; foreach (DamageNumber other in numbers) if (other != number && other.Kind != 0 && other.Age < .35f && (other.Position - at).sqrMagnitude < .6f) near++;
            number.Position = at + Vector2.up * (near * .26f);
            number.Color = color; number.Age = 0; number.Life = life; number.Scale = size;
            number.Velocity = new Vector2(0, .9f);
            number.View.characterSize = .055f * size; number.View.text = text;
            foreach (TextMesh outline in number.Outlines) { outline.characterSize = number.View.characterSize; outline.text = text; outline.color = new Color(.06f, .035f, .10f); }
            number.View.color = color;
            number.View.transform.position = new Vector3(number.Position.x, number.Position.y, -2f);
            number.View.transform.localScale = Vector3.one;
            number.View.gameObject.SetActive(true);
            return number;
        }

        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0 || !float.IsFinite(deltaTime)) return;
            if (tactile != null) tactile.Advance(deltaTime);
            foreach (DamageNumber number in numbers)
            {
                if (number.Age >= number.Life) continue;
                number.Age += deltaTime;
                if (!PresentationSettings.ReducedMotion)
                {
                    // thrown up and to the side, then falling: an arc instead of a slow lift
                    number.Position += number.Velocity * deltaTime;
                    if (number.Kind == 0) number.Velocity += Vector2.down * 6.5f * deltaTime; else number.Velocity *= 1f - Mathf.Min(1f, deltaTime * 1.6f); // words rise and ease
                    number.Velocity.x *= 1f - Mathf.Min(1f, deltaTime * 2.5f);
                }
                float tremble = number.Shake > 0 && !PresentationSettings.ReducedMotion ? Mathf.Sin(number.Age * 70f) * 0.07f * number.Shake * Mathf.Clamp01(1f - number.Age / 0.45f) : 0f;
                number.View.transform.position = new Vector3(number.Position.x + tremble, number.Position.y, -2f);
                float pop = PresentationSettings.ReducedMotion ? 0 : Mathf.Clamp01(1f - number.Age / 0.1f);
                number.View.transform.localScale = new Vector3(1f + 0.55f * pop, 1f + 0.35f * pop, 1);
                number.View.color = new Color(number.Color.r, number.Color.g, number.Color.b, Mathf.Clamp01((1f - number.Age / number.Life) * 3f));
                foreach (TextMesh outline in number.Outlines) outline.color = new Color(0.06f, 0.035f, 0.10f, number.View.color.a * 0.92f);
                if (number.Age >= number.Life) number.View.gameObject.SetActive(false);
            }
            foreach (Spark spark in sparks)
            {
                if (spark.Age >= spark.Duration) continue;
                if (PresentationSettings.ReducedMotion) { spark.Age = spark.Duration; spark.View.gameObject.SetActive(false); continue; }
                spark.Age += deltaTime;
                float remaining = Mathf.Clamp01(1f - spark.Age / spark.Duration);
                spark.Position += spark.Velocity * deltaTime;
                spark.View.transform.position = spark.Position;
                if (spark.Frames != null && spark.Frames.Length > 1) spark.View.sprite = spark.Frames[Mathf.Min(spark.Frames.Length - 1, Mathf.FloorToInt((1 - remaining) * spark.Frames.Length))];
                spark.View.transform.localScale = new Vector3(spark.Shape.x, spark.Shape.y, 1) * (spark.Size * (0.45f + 0.55f * remaining) / spark.Ink);
                spark.View.color = new Color(spark.Color.r, spark.Color.g, spark.Color.b, remaining);
                if (remaining <= 0f) spark.View.gameObject.SetActive(false);
            }
        }
    }
}
