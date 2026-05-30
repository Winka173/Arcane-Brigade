using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The hero's own body as its skill begins and while it empowers itself (owner 24/10/2026: "if the hero buffs
    /// itself, it must show on the hero"). Every transformation opens with a power-up: two silhouettes of the hero in its
    /// colour swell out from behind it and fade, and strokes of light run up beside its body. While a transformation
    /// strengthens the hero itself (speed, attack speed, crits, toughness, healing, a bigger body) the hero keeps an aura:
    /// a stronger rim, its own tokens rising round it and fine strokes of light climbing beside it. No discs, no rings, no
    /// wash over the floor: the hero's own shape and its own tokens.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private SpriteRenderer[] powerEchoes;
        private MaterialPropertyBlock powerBlock;

        /// <summary>A transformation that makes the hero itself stronger, not only its strikes or its friends.</summary>
        private static bool SelfEmpowering(HeroKit kit)
        {
            if (kit == null) return false;
            if (kit.HeroDamageTaken < 1 || kit.HeroMoveSpeed > 0 || kit.HeroAttackSpeed > 0 || kit.HeroCrit > 0) return true;
            switch (kit.Action)
            {
                case KitAction.Treant: case KitAction.Kintsugi: case KitAction.Spring: case KitAction.HighTide: case KitAction.Feast: case KitAction.Throne: case KitAction.Citadel: return true;
                default: return false;
            }
        }

        private void PowerUp()
        {
            UnitState hero = sim.Champion; HeroKit kit = sim.HeroKitInfo;
            UnitVisual view = UnitVisual.Live(hero.Id);
            SpriteRenderer body = view != null ? view.BodyRenderer : null;
            float age = t - sim.HeroAwakenStarted;
            bool awake = sim.HeroAwakening;
            HidePowerEchoes(awake && !still && body != null && age < .7f ? 2 : 0);
            if (!awake || body == null || body.sprite == null) return;
            float height = hero.Definition.BodyHeight;
            Vector2 at = DrawnAt(hero), feet = at - Vector2.up * height * .5f;
            // the silhouette wears the hero's body colour, never near-white (a pale palette, Soap Skater's, read as a sheet of light)
            Color glow = Color.Lerp(ink.Body, ink.Glow, .3f);

            // the power-up: the hero's silhouette swells out twice from behind it
            if (!still && age < .7f)
            {
                EnsurePowerEchoes(body);
                for (int i = 0; i < 2; i++)
                {
                    float k = Mathf.Clamp01((age - i * .14f) / .5f);
                    SpriteRenderer e = powerEchoes[i];
                    e.enabled = k > 0 && k < 1;
                    if (!e.enabled) continue;
                    e.sprite = body.sprite; e.flipX = body.flipX; e.sortingLayerID = body.sortingLayerID; e.sortingOrder = body.sortingOrder - 1;
                    e.transform.SetPositionAndRotation(body.transform.position, body.transform.rotation);
                    e.transform.localScale = body.transform.lossyScale * (1 + .45f * OutCubic(k));
                    powerBlock ??= new MaterialPropertyBlock(); body.GetPropertyBlock(powerBlock); // the body's own sprite rectangle and shading
                    powerBlock.SetFloat("_Flash", 1); powerBlock.SetColor("_FlashColor", glow); powerBlock.SetFloat("_FlashLine", 9);
                    e.SetPropertyBlock(powerBlock);
                    e.color = new Color(1, 1, 1, (calm ? .25f : .45f) * (1 - k) * strength);
                }
                // strokes of light shooting up beside the body
                if (!sparse)
                    for (int s = 0; s < 6; s++)
                    {
                        float q = Mathf.Clamp01((age - s * .05f) / .4f); if (q <= 0 || q >= 1) continue;
                        float side = s % 2 == 0 ? -1 : 1, x = side * height * (.32f + .08f * (s % 3));
                        Vector2 from = feet + new Vector2(x, height * .1f + height * .9f * OutCubic(q) * .6f), to = from + Vector2.up * height * (.35f + .25f * (1 - q));
                        Soft(air, InkCanvas.Line(from, to), 1, 0, .045f, (1 - q) * .9f);
                    }
            }
            if (!SelfEmpowering(kit)) return;

            // the aura while it lasts: the hero's tokens rising round it, fine strokes climbing beside it
            int motes = sparse ? 3 : 6;
            for (int i = 0; i < motes; i++)
            {
                float phase = still ? (i + .5f) / motes : Mathf.Repeat(t * .7f + i / (float)motes, 1);
                float sway = Mathf.Sin(i * 2.1f + phase * 5) * height * .42f;
                Vector2 p = feet + new Vector2(sway, phase * height * 1.35f);
                Token(style.Token, p, .13f * (1 - .3f * phase), still ? 0 : phase * 160 + i * 40, Mathf.Sin(phase * Mathf.PI) * .85f, true);
            }
            if (!sparse && !still)
                for (int i = 0; i < 4; i++)
                {
                    float phase = Mathf.Repeat(t * 1.3f + i * .27f, 1), side = i % 2 == 0 ? -1 : 1;
                    Vector2 from = feet + new Vector2(side * height * (.36f + .06f * i), phase * height * 1.1f);
                    Soft(air, InkCanvas.Line(from, from + Vector2.up * height * .22f), 1, 0, .028f, Mathf.Sin(phase * Mathf.PI) * .7f);
                }
        }

        private void EnsurePowerEchoes(SpriteRenderer body)
        {
            if (powerEchoes != null) return;
            powerEchoes = new SpriteRenderer[2];
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject("Hero power-up silhouette " + i); go.transform.SetParent(transform, false);
                powerEchoes[i] = go.AddComponent<SpriteRenderer>(); powerEchoes[i].sharedMaterial = body.sharedMaterial; powerEchoes[i].enabled = false;
            }
        }
        private void HidePowerEchoes(int keep)
        {
            if (powerEchoes == null || keep > 0) return;
            foreach (SpriteRenderer e in powerEchoes) if (e != null) e.enabled = false;
        }
    }
}
