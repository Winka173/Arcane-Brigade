using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Two touches round a hero's skill (requirement 24/10/2026): the weapon charging while the gauge flashes full, and
    /// afterimages behind a hero whose transformation dashes, skates, rides or slips. The hero's own shape and its weapon,
    /// no discs or rings.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private SpriteRenderer[] trail;
        private readonly Vector2[] trailAt = new Vector2[4];
        private readonly float[] trailBorn = { -9, -9, -9, -9 };
        private int trailNext;
        private Vector2 lastDrawn; private float lastTrailAt = -9; private bool drawnOnce;

        private static bool Dashing(KitAction a) => a == KitAction.Eclipse || a == KitAction.Meteors || a == KitAction.Rink || a == KitAction.HighTide || a == KitAction.Engine || a == KitAction.DivingBell || a == KitAction.Train;

        private void Flourish()
        {
            UnitState hero = sim.Champion;
            UnitVisual view = UnitVisual.Live(hero.Id);
            Color glow = Color.Lerp(ink.Body, ink.Glow, .3f); // the hero's body colour, as the power-up's silhouettes
            ChargeWeapon(hero, view, glow);
            Afterimages(hero, view, sim.HeroKitInfo, sim.HeroAwakening, glow);
        }

        /// <summary>While the gauge flashes full, the weapon gathers the hero's light: a rim that thickens and sparks running
        /// from the grip to the tip.</summary>
        private void ChargeWeapon(UnitState hero, UnitVisual view, Color glow)
        {
            if (!sim.HeroAwakenPrimed || view == null) return;
            SpriteRenderer weapon = view.WeaponRenderer;
            if (weapon == null || !weapon.enabled || weapon.sprite == null) return;
            float k = Mathf.Clamp01(Mathf.Repeat(t, 1f));
            SkillRim.For(weapon)?.Show(A(glow, (.5f + .5f * k) * strength), .018f + .03f * k, float.NaN, .28f, style.Brush == InkBrush.Bolt ? 1 : 0);
            if (sparse || !UnitVisual.TryMuzzle(hero.Id, out Vector2 tip)) return;
            Vector2 grip = weapon.transform.position;
            for (int i = 0; i < 3; i++)
            {
                float f = still ? (i + 1) / 4f : Mathf.Repeat(t * 1.8f + i / 3f, 1);
                Token(InkToken.Glint, Vector2.Lerp(grip, tip, f), .12f + .08f * f, still ? 0 : t * 300 + i * 90, Mathf.Sin(f * Mathf.PI), true, 0, GlintPalette(ink));
            }
        }

        /// <summary>Behind a hero that dashes, skates, rides or slips: three silhouettes in its colour, fading in a quarter
        /// second; a slip (a jump of more than a body) leaves them strung along the jump.</summary>
        private void Afterimages(UnitState hero, UnitVisual view, HeroKit kit, bool awake, Color glow)
        {
            SpriteRenderer body = view != null ? view.BodyRenderer : null;
            Vector2 now = DrawnAt(hero);
            if (!drawnOnce) { lastDrawn = now; drawnOnce = true; }
            if (body != null && body.sprite != null && !still && !sparse && awake && kit != null && Dashing(kit.Action))
            {
                EnsureTrail(body);
                float jump = (now - lastDrawn).magnitude;
                if (jump > hero.Definition.BodyHeight * .9f)
                    for (int i = 1; i <= 3; i++) Drop(Vector2.Lerp(lastDrawn, now, i / 4f), t - (3 - i) * .05f);
                else if (jump > .004f && t - lastTrailAt > .07f) Drop(lastDrawn, t);
            }
            lastDrawn = now;
            if (trail == null) return;
            for (int i = 0; i < trail.Length; i++)
            {
                SpriteRenderer e = trail[i]; float k = (t - trailBorn[i]) / .25f;
                e.enabled = body != null && k >= 0 && k < 1;
                if (!e.enabled) continue;
                e.sprite = body.sprite; e.flipX = body.flipX; e.sortingLayerID = body.sortingLayerID; e.sortingOrder = body.sortingOrder - 1;
                e.transform.SetPositionAndRotation((Vector3)trailAt[i] + (body.transform.position - (Vector3)now), body.transform.rotation);
                e.transform.localScale = body.transform.lossyScale;
                powerBlock ??= new MaterialPropertyBlock(); body.GetPropertyBlock(powerBlock);
                powerBlock.SetFloat("_Flash", 1); powerBlock.SetColor("_FlashColor", glow); powerBlock.SetFloat("_FlashLine", 9);
                e.SetPropertyBlock(powerBlock);
                e.color = new Color(1, 1, 1, .45f * (1 - k) * strength);
            }
        }
        private void Drop(Vector2 at, float born) { trailAt[trailNext] = at; trailBorn[trailNext] = born; trailNext = (trailNext + 1) % trailAt.Length; lastTrailAt = t; }
        private void EnsureTrail(SpriteRenderer body)
        {
            if (trail != null) return;
            trail = new SpriteRenderer[trailAt.Length];
            for (int i = 0; i < trail.Length; i++)
            {
                var go = new GameObject("Hero afterimage " + i); go.transform.SetParent(transform, false);
                trail[i] = go.AddComponent<SpriteRenderer>(); trail[i].sharedMaterial = body.sharedMaterial; trail[i].enabled = false;
            }
        }
    }
}
