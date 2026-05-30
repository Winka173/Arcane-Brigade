using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ProjectileVisual : MonoBehaviour
    {
        private GroundShadow grounded;
        private SpriteRenderer halo;
        private SpriteRenderer core;
        private LineRenderer streak;
        private LineRenderer motif;
        private readonly SpriteRenderer[] petals = new SpriteRenderer[6]; // painted companions of a bloom orb / music-box shot
        private Material material;
        private ArenaController owner;
        private readonly Vector2[] history = new Vector2[4];
        private int identity = -1;
        private string paintedKey;
        private float sampledAt = -100;

        public void Configure(Sprite disc)
        {
            if (halo != null) return;
            halo = Child("Halo", disc);
            core = Child("Core", disc);
            var child = new GameObject("MotionStreak");
            child.transform.SetParent(transform, false);
            streak = child.AddComponent<LineRenderer>();
            streak.positionCount = 4;
            streak.numCapVertices = 6;
            streak.numCornerVertices = 6;
            material = new Material(Shader.Find("Sprites/Default"));
            streak.sharedMaterial = material;
            streak.sortingOrder = 1800;
            var glyph = new GameObject("ProjectileMotif"); glyph.transform.SetParent(transform, false);
            motif = glyph.AddComponent<LineRenderer>(); motif.sharedMaterial = material; motif.sortingOrder = 1804;
            motif.numCapVertices = 4; motif.numCornerVertices = 4; motif.widthMultiplier = 0.035f;
            for (int i = 0; i < petals.Length; i++) { var petal = new GameObject("ProjectilePetal" + i); petal.transform.SetParent(transform, false); petals[i] = petal.AddComponent<SpriteRenderer>(); petals[i].sortingOrder = 1805; petals[i].enabled = false; }
        }

        private SpriteRenderer Child(string name, Sprite sprite)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            SpriteRenderer view = child.AddComponent<SpriteRenderer>();
            view.sprite = sprite;
            view.sortingOrder = name == "Core" ? 1803 : 1801;
            return view;
        }

        /// <summary>Our shots are drawn larger than their authored width (presentation only; hit radius unchanged) so a volley
        /// reads as satisfying next to the enemy's 0.36 minimum, but never past 0.62.</summary>
        public static float Allied(float width) => Mathf.Clamp(width * 1.35f, .32f, .62f); // no shot of ours larger than 0.62 (thrown boomerangs and yo-yos were drawn ~1 unit)
        public void Display(ProjectileState state, SpriteRenderer body, Vector2 position, float time)
        {
            // owner 24/10/2026: Postmaster's parcels and Treasure Acorn's acorns wear a glowing shader rim that follows their shape
            // (no round glow disc behind a shot)
            string cause = state.Cause.ItemId;
            if (cause == "hero-identity-Postmaster") SkillRim.For(body)?.Show(new Color(1f, .45f, .35f, .95f), .035f, float.NaN);
            else if (cause == "hero-identity-TreasureAcorn") SkillRim.For(body)?.Show(new Color(1f, .82f, .35f, .95f), .035f, float.NaN);
            bool hostile = state.Faction == Faction.Enemy;
            if (owner == null) owner = GetComponentInParent<ArenaController>();
            bool newShot = identity != state.Id || time < sampledAt;
            // Classify once: a bullet keeps its appearance after its shooter dies, without a unit search every frame.
            if (newShot) paintedKey = CombatVfxArtwork.ProjectileKey(state, owner?.Simulation);
            Sprite painted = CombatVfxArtwork.Frame(paintedKey, time, state.Id);
            // a weapon's own painted shot (Art/Arsenal/Shot_<WeaponId>) wins over any shared one: no two weapons fire the same thing
            Sprite own = !hostile && state.Weapon != null && !state.IsFragment ? ArsenalArtwork.Frame("Shot_" + state.Weapon.Id, 0) : null;
            if (own != null) painted = own;
            if (painted != null)
            {
                body.sprite = painted;
                float inkWidth = Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(painted).width);
                float paintedWidth = hostile ? Mathf.Max(.36f, state.VisualWidth) : Allied(state.VisualWidth > 0 ? state.VisualWidth : .22f);
                body.transform.localScale = Vector3.one * (paintedWidth / inkWidth);
                body.color = new Color(1, 1, 1, body.color.a);
            }
            if (newShot)
            {
                identity = state.Id; sampledAt = time;
                for (int index = 0; index < history.Length; index++) history[index] = position;
            }
            if (time - sampledAt >= .016f)
            {
                for (int index = history.Length - 1; index > 0; index--) history[index] = history[index - 1];
                history[0] = position; sampledAt = time;
            }
            bool bodyFragment = hostile && state.Sprite != null && (state.Sprite.texture.name == "BodyFragments_v1" || state.Sprite.texture.name == "EnemyAffixEffects_v1");
            if (grounded == null) { grounded = gameObject.AddComponent<GroundShadow>(); grounded.Configure(body.sprite); }
            grounded.Display(body.sprite, position + Vector2.down * 0.18f, Mathf.Max(0.13f, state.VisualWidth), 0.18f, hostile ? 0.22f : 0.13f);
            int order = hostile ? 2400 : 2100;
            body.sortingOrder = order;
            if (hostile)
            {
                Color danger = Color.Lerp(body.color, new Color(1, 0.24f, 0.42f, body.color.a), painted != null ? 0 : bodyFragment ? 0.2f : 0.65f);
                body.color = danger;
            }
            halo.sortingOrder = order - 1; core.sortingOrder = order + 1;
            streak.sortingOrder = order - 2; motif.sortingOrder = order + 2;
            if (!PresentationSettings.ReducedMotion && state.Weapon != null && state.Weapon.Family == ExpandedWeapon.Fish)
                body.transform.rotation = Quaternion.Euler(0, 0, time * 650 + state.Id * 31);
            bool arrow = state.Sprite != null && state.Sprite.name == "ArrowBolt";
            bool magic = hostile || !arrow && (state.Element == DamageElement.Arcane || state.Element == DamageElement.Ember);
            bool authored = painted != null || bodyFragment || MechanicInk.IsInk(body.sprite) || state.Sprite != null && (state.Sprite.texture.name == "DreamProjectiles_v1" || state.Sprite.texture.name == "CarnivalProjectiles_v1");
            halo.enabled = core.enabled = magic && !authored && (hostile || EffectDensity.CosmeticProjectile(state.Id));
            halo.enabled = false; // no round glow disc behind any shot (owner 17/10 ours, 23/10/2026 the enemy's red disc too); the trail tells them apart
            if (state.Weapon != null && (state.Weapon.Family == ExpandedWeapon.Fish || state.Weapon.Mechanic == CarnivalWeapon.SatelliteDisc)) halo.enabled = core.enabled = false;
            float width = hostile ? (state.VisualWidth > 0 ? state.VisualWidth : 0.23f) : Allied(state.VisualWidth > 0 ? state.VisualWidth : 0.22f);
            if (hostile && body.sprite != null && painted == null)
                body.transform.localScale = Vector3.one * (width / Mathf.Max(0.01f, body.sprite.bounds.size.x));
            if (magic)
            {
                float pulse = PresentationSettings.ReducedMotion ? 1 : 1f + Mathf.Sin(time * 18f + state.Id) * 0.08f;
                float local = width / Mathf.Max(0.001f, Mathf.Abs(body.transform.localScale.x)) / Mathf.Max(0.01f, halo.sprite.bounds.size.x);
                halo.transform.localScale = Vector3.one * local * (hostile ? 2.15f : 1.85f) * pulse;
                core.transform.localScale = Vector3.one * local * 0.45f;
                halo.color = hostile ? new Color(1f, 0.12f, 0.45f, 0.6f) : new Color(state.Color.r, state.Color.g, state.Color.b, 0.18f);
                core.color = new Color(1f, 0.96f, 0.89f, hostile ? 0.95f : 0.9f * Mathf.Lerp(1, 0.65f, EffectDensity.CrowdPressure));
            }
            streak.enabled = hostile || !PresentationSettings.ReducedMotion && EffectDensity.CosmeticProjectile(state.Id) && (arrow || magic || MechanicInk.IsInk(body.sprite));
            streak.widthMultiplier = arrow || state.Weapon != null && state.Weapon.Mechanic == CarnivalWeapon.Sewing ? 0.024f : 0.065f;
            // One danger hue for every hostile shot so it never reads like an allied ember bolt.
            Color trail = hostile ? painted != null ? new Color(.8f, .18f, .08f) : new Color(1f, 0.16f, 0.48f) : painted != null ? new Color(.9f, .72f, .4f) : state.Color;
            streak.startColor = new Color(trail.r, trail.g, trail.b, 0.03f);
            streak.endColor = new Color(trail.r, trail.g, trail.b, hostile ? 0.8f : EffectDensity.AlliedAlpha);
            for (int index = 0; index < history.Length; index++)
            {
                Vector2 point = PresentationSettings.ReducedMotion ? position - state.Velocity.normalized * (.24f * (1 - index / 3f)) : index == 3 ? position : history[3 - index];
                point = position + Vector2.ClampMagnitude(point - position, arrow ? .6f : .38f);
                streak.SetPosition(index, point);
            }
            if (!hostile && magic && !authored && !PresentationSettings.ReducedMotion)
            {
                core.transform.localRotation = Quaternion.Euler(0, 0, time * 65 + state.Id * 31);
                core.transform.localScale = new Vector3(1 + Mathf.Sin(time * 5 + state.Id) * .08f, 1 - Mathf.Sin(time * 5 + state.Id) * .08f, 1) * (width / Mathf.Max(.001f, body.transform.localScale.x) / Mathf.Max(.01f, core.sprite.bounds.size.x) * .45f);
            }
            // no outline glyphs: a bloom orb is ringed by six painted leaves turning slowly, a music-box shot trails painted notes
            ExpandedWeapon family = state.Weapon == null ? ExpandedWeapon.None : state.Weapon.Family;
            motif.enabled = false;
            bool bloom = painted == null && state.Weapon != null && state.Weapon.Mechanic == CarnivalWeapon.BloomOrb;
            bool notes = painted == null && family == ExpandedWeapon.MusicBox;
            Sprite petalArt = bloom ? CombatVfxArtwork.Frame("LeafBolt", 0) : notes ? CombatVfxArtwork.Frame("MusicNote", 0) : null;
            int count = petalArt == null || !EffectDensity.CosmeticProjectile(state.Id) ? 0 : bloom ? (PresentationSettings.ReducedEffects ? 3 : 6) : 3;
            bool still = PresentationSettings.ReducedMotion;
            Vector2 heading = state.Velocity.sqrMagnitude > .0001f ? state.Velocity.normalized : Vector2.right;
            for (int index = 0; index < petals.Length; index++)
            {
                SpriteRenderer petal = petals[index]; petal.enabled = index < count; if (!petal.enabled) continue;
                petal.sprite = petalArt;
                Vector2 at; float size, angle;
                if (bloom)
                {
                    float turn = (still ? 0 : time * 1.6f) + index * Mathf.PI * 2 / count;
                    float reach = Mathf.Max(.12f, state.VisualWidth) * (.62f + (still ? 0 : Mathf.Sin(time * 4 + index) * .06f));
                    at = position + new Vector2(Mathf.Cos(turn), Mathf.Sin(turn)) * reach; size = Mathf.Max(.09f, state.VisualWidth) * .42f; angle = turn * Mathf.Rad2Deg - 90;
                }
                else
                {
                    float back = .18f + index * .17f; Vector2 side = new Vector2(-heading.y, heading.x);
                    at = position - heading * back + side * (still ? 0 : Mathf.Sin(time * 9 + index * 2.1f) * .07f); size = .13f - index * .02f; angle = still ? 0 : Mathf.Sin(time * 6 + index) * 18;
                }
                petal.transform.position = at; petal.transform.rotation = Quaternion.Euler(0, 0, angle);
                petal.transform.localScale = Vector3.one * (size / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(petalArt).width) / Mathf.Max(.0001f, Mathf.Abs(transform.lossyScale.x)));
                petal.color = bloom ? new Color(.75f, 1, .88f, EffectDensity.AlliedAlpha) : new Color(1, .95f, .8f, EffectDensity.AlliedAlpha * (1 - index * .25f));
            }
            if (state.Tint.a > 0) // a tinted shot (Drowsy Seer's dreams): the dream colour, faded; untinted shots are unchanged
            {
                body.color = new Color(state.Tint.r, state.Tint.g, state.Tint.b, body.color.a * state.Tint.a);
                streak.startColor = new Color(state.Tint.r, state.Tint.g, state.Tint.b, streak.startColor.a * state.Tint.a);
                streak.endColor = new Color(state.Tint.r, state.Tint.g, state.Tint.b, streak.endColor.a * state.Tint.a);
            }
        }

        private void OnDestroy() { if (material != null) Destroy(material); }
        public void SetFade(float alpha)
        {
            if (grounded != null) grounded.Fade(alpha);
            if (halo == null) return;
            Color haloColor = halo.color; haloColor.a *= alpha; halo.color = haloColor;
            Color coreColor = core.color; coreColor.a *= alpha; core.color = coreColor;
            Color start = streak.startColor; start.a *= alpha; streak.startColor = start;
            Color end = streak.endColor; end.a *= alpha; streak.endColor = end;
            Color glyph = motif.startColor; glyph.a *= alpha; motif.startColor = motif.endColor = glyph;
        }
        public void Hide() { if (halo != null) { halo.enabled = core.enabled = streak.enabled = motif.enabled = false; foreach (SpriteRenderer petal in petals) if (petal != null) petal.enabled = false; } if (grounded != null) grounded.Hide(); }
    }
}
