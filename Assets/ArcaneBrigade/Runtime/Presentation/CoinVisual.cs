using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CoinVisual : MonoBehaviour
    {
        private SpriteRenderer shadow;
        private SpriteRenderer glint;
        private GroundShadow grounded;
        private SpriteRenderer chestGlow;
        private DreamSuspension suspension;
        public void Display(PickupState state, SpriteRenderer renderer, Sprite disc, Sprite spark, float time, float interpolation)
        {
            if (shadow == null)
            {
                shadow = Create("CoinShadow");
                chestGlow = ShopFloorShadow.Create(transform, Vector2.zero, 1, .6f);
                chestGlow.name = "ChestGroundGlow";
                glint = Create("CoinGlint");
            }
            float age = Mathf.Max(0, time - state.SpawnTime);
            float landing = Mathf.Clamp01(age / 0.26f);
            float height = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(landing * Mathf.PI) * (state.Kind == LootKind.Chest ? 0.1f : 0.32f);
            if (state.Kind != LootKind.Coin && state.Kind != LootKind.Chest && !PresentationSettings.ReducedMotion) height += Mathf.Sin(age * 3 + state.Id) * 0.025f;
            Vector2 ground = Vector2.Lerp(state.PreviousPosition, state.Position, interpolation);
            renderer.transform.position = ground + Vector2.up * height;
            float spin = state.Kind == LootKind.Coin && !PresentationSettings.ReducedMotion ? 0.42f + Mathf.Abs(Mathf.Cos(age * (state.Attracting ? 25 : 8) + state.Id)) * 0.58f : 1;
            float width = state.Kind == LootKind.Chest ? 0.78f : state.Kind == LootKind.Coin ? 0.24f + Mathf.Min(0.12f, Mathf.Max(0, state.Value - 1) * 0.015f) : state.Kind >= LootKind.Flare ? 0.56f : 0.4f;
            float scale = width / renderer.sprite.bounds.size.x;
            renderer.transform.localScale = new Vector3(scale * spin, scale, 1);
            // painted spin frames when they exist (Art/Arsenal/CoinSpin); otherwise the squash turns, its rim darkening edge-on
            Sprite[] turn = state.Kind == LootKind.Coin && !PresentationSettings.ReducedMotion ? ArsenalArtwork.Frames("CoinSpin") : null;
            if (turn != null) { renderer.sprite = turn[Mathf.Abs(Mathf.FloorToInt(age * (state.Attracting ? 24 : 10) + state.Id)) % turn.Length]; renderer.transform.localScale = new Vector3(width / renderer.sprite.bounds.size.x, width / renderer.sprite.bounds.size.x, 1); }
            if (state.Kind == LootKind.Chest && !PresentationSettings.ReducedMotion)
            {
                float anticipation = AnimationTiming.Pulse(Mathf.Repeat(age + state.Id * .31f, 4.8f), .45f);
                renderer.transform.localScale = Vector3.Scale(renderer.transform.localScale, new Vector3(1 + anticipation * .018f, 1 - anticipation * .025f, 1));
            }
            renderer.transform.rotation = Quaternion.Euler(0, 0, state.Attracting && !PresentationSettings.ReducedMotion ? Mathf.Sin(age * 12) * 12 : 0);
            if (state.Kind != LootKind.Coin && !state.Attracting)
            {
                Vector3 hover = DreamLevitation.Offset(time, state.Id, 0, PresentationSettings.ReducedMotion, state.Kind == LootKind.Chest ? .7f : .4f);
                renderer.transform.position += hover;
                height += hover.y;
                renderer.transform.rotation *= Quaternion.Euler(0, 0, DreamLevitation.Tilt(time, state.Id, PresentationSettings.ReducedMotion, .8f));
                if (suspension == null) suspension = gameObject.AddComponent<DreamSuspension>();
                suspension.Display(ground, (Vector3)ground + hover, width, time, state.Id, 1699, 1, true);
            }
            else if (suspension != null) suspension.Hide();
            float edge = state.Kind == LootKind.Coin && turn == null && !PresentationSettings.ReducedMotion ? Mathf.InverseLerp(0.42f, 1f, spin) : 1;
            renderer.color = Color.Lerp(new Color(0.72f, 0.6f, 0.45f), Color.white, edge);
            renderer.sortingOrder = 1700;
            shadow.gameObject.SetActive(true); shadow.sprite = disc;
            shadow.transform.position = ground;
            Vector3 parentScale = renderer.transform.lossyScale;
            shadow.transform.localScale = new Vector3(width * 0.92f / parentScale.x, width * 0.29f / parentScale.y, 1) / disc.bounds.size.x;
            shadow.color = new Color(0.04f, 0.025f, 0.08f, 0.30f * (1 - height));
            shadow.sortingOrder = 490;
            if (grounded == null) { grounded = gameObject.AddComponent<GroundShadow>(); grounded.Configure(renderer.sprite); }
            grounded.Display(renderer.sprite, ground, width * 0.95f, height, 0.3f);
            shadow.gameObject.SetActive(false);
            bool glows = state.Kind == LootKind.Chest || state.Kind >= LootKind.Flare;
            chestGlow.gameObject.SetActive(glows);
            if (glows)
            {
                chestGlow.transform.position = ground;
                chestGlow.transform.localScale = new Vector3(1.8f / parentScale.x, .8f / parentScale.y, 1) / chestGlow.sprite.bounds.size.x;
                chestGlow.sortingOrder = 488;
                chestGlow.color = state.Kind == LootKind.Bell ? new Color(.95f, .78f, .42f, .55f + (PresentationSettings.ReducedFlashes ? 0 : .12f * Mathf.Sin(age * 3))) : state.Kind == LootKind.Flare ? new Color(1f, .82f, .45f, .62f + (PresentationSettings.ReducedFlashes ? 0 : .18f * Mathf.Sin(age * 4))) : new Color(1, .7f, .2f, PresentationSettings.ReducedFlashes ? .6f : .6f + .08f * Mathf.Sin(age * 2));
            }
            float pulse = PresentationSettings.ReducedFlashes || PresentationSettings.ReducedEffects ? 0 : Mathf.Pow(Mathf.Max(0, Mathf.Cos(age * 4 + state.Id * 1.3f)), 12);
            glint.gameObject.SetActive(pulse > 0.05f); glint.sprite = spark;
            glint.transform.position = ground + new Vector2(0.06f * spin, height + 0.07f);
            float glintSize = (0.04f + pulse * 0.065f) / spark.bounds.size.x;
            glint.transform.localScale = new Vector3(glintSize / parentScale.x, glintSize / parentScale.y, 1);
            glint.color = new Color(1, 0.96f, 0.7f, pulse);
            glint.sortingOrder = 1701;
        }
        private SpriteRenderer Create(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            return child.AddComponent<SpriteRenderer>();
        }
        public void Hide()
        {
            if (shadow != null) shadow.gameObject.SetActive(false);
            if (glint != null) glint.gameObject.SetActive(false);
            if (grounded != null) grounded.Hide();
            if (chestGlow != null) chestGlow.gameObject.SetActive(false);
            if (suspension != null) suspension.Hide();
        }
        public void SetFade(SpriteRenderer renderer, float alpha)
        {
            Color body = renderer.color; body.a *= alpha; renderer.color = body;
            if (shadow != null) { Color color = shadow.color; color.a *= alpha; shadow.color = color; }
            if (glint != null) { Color color = glint.color; color.a *= alpha; glint.color = color; }
            if (grounded != null) grounded.Fade(alpha);
            if (chestGlow != null) { Color c = chestGlow.color; c.a *= alpha; chestGlow.color = c; }
        }
    }
}
