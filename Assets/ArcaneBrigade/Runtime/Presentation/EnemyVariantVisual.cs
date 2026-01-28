using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class EnemyVariantVisual : MonoBehaviour
    {
        private readonly SpriteRenderer[] effects = new SpriteRenderer[4];
        private SpriteRenderer decoy;
        private SpriteRenderer soul;
        private GroundShadow soulShadow;
        public int VisibleEffects { get; private set; }
        public static int Family(UnitState unit, float time)
        {
            EnemyVariantState state = unit.Variant;
            EnemyAffix affix = state == null ? EnemyAffix.None : state.Affix;
            if (affix == EnemyAffix.TwinFlame) return state.AffixCasts % 2 == 0 ? 1 : 2;
            if (affix != EnemyAffix.None) return affix == EnemyAffix.BlackSmoke ? 0 : affix == EnemyAffix.BlueFlame ? 1 : affix == EnemyAffix.RedFlame ? 2 : 3;
            return unit.Definition.SpecialEnemy switch
            {
                SpecialEnemyKind.SmokeWraith or SpecialEnemyKind.SmokingCoffin or SpecialEnemyKind.AshSwallower => 0,
                SpecialEnemyKind.BlueFlameFiend or SpecialEnemyKind.LostLanternSoul => 1,
                SpecialEnemyKind.RedFlameBerserker or SpecialEnemyKind.EmberProcession => 2,
                SpecialEnemyKind.TwinFlameFurnace => (state?.SpecialCasts ?? 0) % 2 == 0 ? 1 : 2,
                _ => -1
            };
        }
        private SpriteRenderer Create(string label)
        {
            var child = new GameObject(label); child.transform.SetParent(transform, false);
            return child.AddComponent<SpriteRenderer>();
        }
        public void Display(UnitState unit, SpriteRenderer body, float time, bool reduced)
        {
            int family = unit.Faction == Faction.Enemy && !unit.Definition.IsBoss && unit.Alive ? Family(unit, time) : -1;
            bool enabled = family >= 0 && EnemyAffixLibrary.Current != null;
            VisibleEffects = 0;
            Rect ink = SpriteInkMetrics.BoundsOf(body.sprite);
            Vector3 center = body.transform.TransformPoint(ink.center);
            float height = Mathf.Max(0.2f, unit.Definition.BodyHeight);
            for (int index = 0; index < effects.Length; index++)
            {
                if (effects[index] == null && enabled) effects[index] = Create("AttachedEnemyAffix" + index);
                SpriteRenderer effect = effects[index];
                if (effect == null) continue;
                effect.enabled = enabled && (!(PresentationSettings.ReducedEffects || AnimationDetailBudget.Level > 0) || index < 2);
                if (!effect.enabled) continue;
                VisibleEffects++;
                float phase = time + unit.Id * 0.173f + index * 0.087f;
                effect.sprite = EnemyAffixLibrary.Effect(family, phase, reduced);
                if (effect.sprite == null) { effect.enabled = false; continue; }
                float breath = reduced ? 1 : 0.94f + Mathf.Sin(phase * (family == 2 ? 8 : 3)) * 0.07f;
                bool charging = unit.Variant != null && unit.Variant.ReleaseAt > time;
                float intensity = charging ? 1.2f : 1;
                float hit = !reduced && !PresentationSettings.ReducedFlashes ? Mathf.Clamp01(1 - (time - unit.LastHit) / 0.16f) * 0.1f : 0;
                effect.transform.position = center + new Vector3((index % 2 == 0 ? -1 : 1) * height * (index < 2 ? 0.43f : 0.3f), height * (index < 2 ? 0.15f : -0.22f), 0);
                float size = height * (index < 2 ? family == 0 ? 0.75f : 0.65f : 0.36f) * breath * (intensity + hit);
                effect.transform.localScale = Vector3.one * size / Mathf.Max(0.01f, effect.sprite.bounds.size.y);
                effect.transform.rotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Sin(phase * 2) * (family == 2 ? 7 : 12));
                effect.sortingOrder = body.sortingOrder + (index < 2 ? -1 : 1);
                float enter = reduced || unit.Variant == null ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01((time - unit.Variant.AssignedAt) / 0.35f));
                effect.color = new Color(1, 1, 1, (index < 2 ? 0.92f : 0.62f) * body.color.a * enter);
            }
            bool hasDecoy = enabled && unit.Variant != null && unit.Variant.DecoyUntil > time;
            if (decoy == null && hasDecoy) decoy = Create("SmokeFeintAfterimage");
            if (decoy != null)
            {
                decoy.enabled = hasDecoy;
                if (hasDecoy)
                {
                    decoy.sprite = body.sprite; decoy.flipX = body.flipX;
                    decoy.transform.position = unit.Variant.DecoyPosition;
                    decoy.transform.localScale = Vector3.one * height / Mathf.Max(0.01f, body.sprite.bounds.size.y);
                    decoy.sortingOrder = body.sortingOrder - 2;
                    decoy.color = new Color(0.22f, 0.15f, 0.36f, Mathf.Clamp01((unit.Variant.DecoyUntil - time) / 1.6f) * 0.45f);
                }
            }
            bool spirit = enabled && unit.Definition.SpecialEnemy == SpecialEnemyKind.LostLanternSoul;
            if (soul == null && spirit)
            {
                soul = Create("OrbitingLanternSoul"); soulShadow = soul.gameObject.AddComponent<GroundShadow>();
            }
            if (soul != null)
            {
                soul.enabled = spirit;
                if (spirit)
                {
                    soul.sprite = EnemyAffixLibrary.Effect(1, time, reduced);
                    Vector2 orbit = reduced ? Vector2.up * 0.5f : new Vector2(Mathf.Cos(time * 1.3f), Mathf.Sin(time * 1.3f)) * 0.65f;
                    soul.transform.position = unit.Position + orbit;
                    soul.transform.localScale = Vector3.one * 0.38f / Mathf.Max(0.01f, soul.sprite.bounds.size.y);
                    soul.sortingOrder = body.sortingOrder + 2;
                    soulShadow.Display(soul.sprite, unit.Position + orbit + Vector2.down * 0.25f, 0.25f, 0.25f, 0.16f);
                }
                else soulShadow.Hide();
            }
        }
    }
}
