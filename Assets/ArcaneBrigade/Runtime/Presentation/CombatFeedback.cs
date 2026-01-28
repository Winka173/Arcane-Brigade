using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CombatFeedback : MonoBehaviour
    {
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
            public int SubjectId;
            public float Amount;
            public bool Critical;
            public float Life = 0.55f;
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
                bool sliver = hit && (combatEvent.Weapon == null || combatEvent.Weapon.Kind == WeaponKind.Melee || combatEvent.Weapon.Motion == WeaponMotion.Bow);
                spark.Shape = sliver ? new Vector2(1.7f, 0.45f) : Vector2.one;
                spark.View.transform.position = spark.Position;
                spark.View.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                spark.View.transform.localScale = new Vector3(spark.Shape.x, spark.Shape.y, 1) * (spark.Size / Mathf.Max(0.01f, sprite.bounds.size.x));
                spark.View.color = color;
                spark.View.gameObject.SetActive(true);
            }
            sequence++;
        }

        private void ShowDamage(CombatEvent combatEvent)
        {
            if (!PresentationSettings.DamageNumbers) return;
            if (combatEvent.SubjectId != 0)
            {
                DamageNumber existing = numbers.Find(candidate => candidate.Age < 0.3f && candidate.SubjectId == combatEvent.SubjectId && candidate.Critical == combatEvent.Critical);
                if (existing != null)
                {
                    existing.Amount += combatEvent.Value;
                    existing.View.text = Mathf.CeilToInt(existing.Amount) + (existing.Critical ? "!" : "");
                    existing.Outline.text = existing.View.text;
                    return;
                }
            }
            bool champion = combatEvent.Faction == Faction.Champion;
            // Readability: ordinary companion/champion hits stay small and few; crits and damage taken always read.
            if (!champion && !combatEvent.Critical && numbers.Count(candidate => candidate.Age < candidate.Life && !candidate.Critical && !candidate.Champion) >= 12) return;
            DamageNumber number = numbers.Find(candidate => candidate.Age >= candidate.Life);
            if (number == null && numbers.Count >= 32)
            {
                if (!champion) return;
                number = numbers.Find(candidate => !candidate.Champion) ?? numbers[0];
            }
            if (number == null)
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
                var outlineObject = new GameObject("DamageOutline");
                outlineObject.transform.SetParent(child.transform, false);
                outlineObject.transform.localPosition = new Vector3(0.025f, -0.025f, 0.01f);
                TextMesh outline = outlineObject.AddComponent<TextMesh>();
                outline.font = font;
                outline.fontSize = view.fontSize;
                outline.characterSize = view.characterSize;
                outline.anchor = view.anchor;
                outline.fontStyle = view.fontStyle;
                MeshRenderer outlineRenderer = outlineObject.GetComponent<MeshRenderer>();
                outlineRenderer.sharedMaterial = font.material;
                outlineRenderer.sortingOrder = 2599;
                number = new DamageNumber { View = view, Outline = outline };
                numbers.Add(number);
            }
            number.Champion = champion;
            number.SubjectId = combatEvent.SubjectId; number.Amount = combatEvent.Value; number.Critical = combatEvent.Critical;
            number.Position = combatEvent.Position + new Vector2((sequence % 3 - 1) * 0.12f, 0.55f);
            number.Color = champion ? new Color(1f, 0.4f, 0.35f) : combatEvent.Critical ? new Color(1f, 0.85f, 0.22f) : Color.white;
            number.Age = 0f;
            number.Life = champion ? 0.7f : combatEvent.Critical ? 0.8f : 0.45f;
            float size = champion ? 1.1f : combatEvent.Critical ? 1.45f : 0.72f;
            number.View.characterSize = number.Outline.characterSize = 0.055f * size;
            number.View.text = Mathf.CeilToInt(combatEvent.Value) + (combatEvent.Critical ? "!" : "");
            number.Outline.text = number.View.text;
            number.Outline.color = new Color(0.06f, 0.035f, 0.10f);
            number.View.color = number.Color;
            number.View.transform.position = new Vector3(number.Position.x, number.Position.y, -2f);
            number.View.transform.localScale = Vector3.one;
            number.View.gameObject.SetActive(true);
        }

        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0 || !float.IsFinite(deltaTime)) return;
            if (tactile != null) tactile.Advance(deltaTime);
            foreach (DamageNumber number in numbers)
            {
                if (number.Age >= number.Life) continue;
                number.Age += deltaTime;
                if (!PresentationSettings.ReducedMotion) number.Position += Vector2.up * deltaTime * 0.7f;
                number.View.transform.position = new Vector3(number.Position.x, number.Position.y, -2f);
                number.View.transform.localScale = Vector3.one * (1f + (PresentationSettings.ReducedMotion ? 0 : 0.35f * Mathf.Clamp01(1f - number.Age / 0.08f)));
                number.View.color = new Color(number.Color.r, number.Color.g, number.Color.b, Mathf.Clamp01((1f - number.Age / number.Life) * 2.5f));
                number.Outline.color = new Color(0.06f, 0.035f, 0.10f, number.View.color.a);
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
                spark.View.transform.localScale = new Vector3(spark.Shape.x, spark.Shape.y, 1) * (spark.Size * remaining / Mathf.Max(0.01f, sprite.bounds.size.x));
                spark.View.color = new Color(spark.Color.r, spark.Color.g, spark.Color.b, remaining);
                if (remaining <= 0f) spark.View.gameObject.SetActive(false);
            }
        }
    }
}
