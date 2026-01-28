using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class EnemyVariantFieldVisual : MonoBehaviour
    {
        private readonly SpriteRenderer[] wisps = new SpriteRenderer[8];
        public void Hide() { foreach (SpriteRenderer wisp in wisps) if (wisp != null) wisp.enabled = false; }
        public void Display(ExpandedField field, float time, bool reduced)
        {
            Hide();
            if (time < field.At || time > field.Until) return;
            int family = EnemyVariantVisual.Family(field.Source, time);
            if (field.Kind == ExpandedFieldKind.Tentacle && (field.Source.Variant.Affix == EnemyAffix.RedFlame || field.Source.Variant.Affix == EnemyAffix.TwinFlame || field.Source.Definition.SpecialEnemy == SpecialEnemyKind.RedFlameBerserker) || field.Source.Definition.SpecialEnemy == SpecialEnemyKind.TwinFlameFurnace) family = 2;
            float progress = Mathf.Clamp01((time - field.At) / Mathf.Max(0.05f, field.Until - field.At));
            int count = PresentationSettings.ReducedEffects ? 3 : field.Kind == ExpandedFieldKind.Shockwave ? 8 : 5;
            for (int index = 0; index < count; index++)
            {
                if (wisps[index] == null)
                {
                    var child = new GameObject("PaintedHostileWisp" + index); child.transform.SetParent(transform, false);
                    wisps[index] = child.AddComponent<SpriteRenderer>(); wisps[index].sortingOrder = 75;
                }
                SpriteRenderer wisp = wisps[index]; wisp.enabled = true;
                wisp.sprite = EnemyAffixLibrary.Effect(family, time + index * 0.071f, reduced);
                if (wisp.sprite == null) { wisp.enabled = false; continue; }
                float phase = index / (float)count * Mathf.PI * 2;
                Vector2 direction = new Vector2(Mathf.Cos(phase), Mathf.Sin(phase));
                if (field.Kind == ExpandedFieldKind.Tentacle) wisp.transform.position = Vector2.Lerp(field.Start, field.Position, (index + 0.5f) / count);
                else wisp.transform.position = field.Position + direction * (field.Kind == ExpandedFieldKind.Shockwave ? CarnivalHazards.ShockRadius(field, time) : field.Radius * (index % 2 == 0 ? 0.35f : 0.7f));
                float height = field.Kind == ExpandedFieldKind.Tentacle ? Mathf.Max(0.3f, field.Radius * 2.4f) : Mathf.Clamp(field.Radius * 0.55f, 0.35f, 0.8f);
                wisp.transform.localScale = Vector3.one * height / Mathf.Max(0.01f, wisp.sprite.bounds.size.y);
                wisp.transform.rotation = Quaternion.Euler(0, 0, reduced ? 0 : field.Kind == ExpandedFieldKind.Tentacle ? Mathf.Atan2(field.Direction.y, field.Direction.x) * Mathf.Rad2Deg - 90 : Mathf.Sin(time * 3 + index) * 12);
                wisp.color = new Color(1, 1, 1, (1 - progress * 0.65f) * 0.85f);
            }
        }
    }
}
