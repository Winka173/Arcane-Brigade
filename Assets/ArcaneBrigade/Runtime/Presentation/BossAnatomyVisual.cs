using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BossAnatomyVisual : MonoBehaviour
    {
        private readonly SpriteRenderer[] organs = new SpriteRenderer[3];
        private readonly SpriteRenderer[] shadows = new SpriteRenderer[3];
        private readonly SpriteRenderer[] scars = new SpriteRenderer[3];
        private readonly Vector3[] origins = new Vector3[3];
        private readonly float[] sizes = new float[3];
        private readonly bool[] brokenParts = new bool[3];
        private BossAnatomyEntry anatomy;
        private SpriteRenderer body;
        private float artworkHeight;
        public int PartCount => anatomy == null ? 0 : anatomy.Organs.Length;
        public int VisibleScars { get; private set; }
        public void Build(UnitState unit, SpriteRenderer reference)
        {
            Clear(); body = reference; anatomy = BossAnatomy.For(unit.Definition);
            if (unit.Definition.Id == "OutsideStageKeeper") body.sprite = ActOneArtwork.Picture("Keeper_BodyShell") ?? body.sprite;
            // Canon bosses are drawn as complete bodies; stand-in organs must not cover the new silhouette.
            if (ActOneArtwork.Has(unit.Definition)) { anatomy = null; return; }
            if (anatomy == null) return;
            Rect ink = SpriteInkMetrics.BoundsOf(body.sprite);
            artworkHeight = ink.height;
            for (int part = 0; part < 3; part++)
            {
                BossOrganDefinition definition = anatomy.Organs[part];
                origins[part] = ink.center + definition.Offset * ink.height;
                sizes[part] = definition.Height * ink.height;
                organs[part] = Create("BossPart-" + definition.Id, body.transform, definition.Rest);
                shadows[part] = Create("PartContactShadow-" + part, body.transform, definition.Rest);
                scars[part] = Create("BrokenSocket-" + part, body.transform, definition.Scar);
                scars[part].enabled = false;
            }
        }
        private static SpriteRenderer Create(string label, Transform parent, Sprite sprite)
        {
            var child = new GameObject(label, typeof(SpriteRenderer)); child.transform.SetParent(parent, false);
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>(); renderer.sprite = sprite; return renderer;
        }
        public void Display(UnitState unit, float time, int order, bool reduced)
        {
            if (anatomy == null) return;
            VisibleScars = 0;
            float compensation = unit.Definition.BodyHeight / Mathf.Max(0.001f, body.transform.localScale.y * artworkHeight);
            for (int part = 0; part < 3; part++)
            {
                BossOrganDefinition definition = anatomy.Organs[part];
                bool broken = BossPartRules.Broken(unit, part);
                brokenParts[part] = broken;
                float brokenAge = time - BossPartRules.BrokenAt(unit, part);
                float hurtAge = time - unit.LastHit;
                bool woundHit = BossPartRules.HitZone(unit.HitDirection) == part;
                bool active = unit.ActiveBossOrgan == part && !broken;
                bool winding = active && unit.TelegraphUntil > time;
                float charge = winding ? Mathf.Clamp01(1 - (unit.TelegraphUntil - time) / BossEncounterRules.Windup(unit)) : 0;
                float releaseAge = time - unit.BossOrganReleaseAt;
                float release = active && releaseAge >= 0 && releaseAge < 0.5f ? Mathf.Sin(releaseAge / 0.5f * Mathf.PI) : 0;
                SpriteRenderer organ = organs[part]; SpriteRenderer scar = scars[part]; SpriteRenderer shadow = shadows[part];
                Sprite image = release > 0.1f && definition.Release != null ? definition.Release : winding && charge > 0.35f || release > 0.1f ? definition.Charged : definition.Rest;
                if (unit.Definition.Id == "OutsideStageKeeper")
                {
                    image = ActOneArtwork.KeeperPart(part, broken, image);
                    scar.sprite = ActOneArtwork.KeeperPart(part, true, definition.Scar);
                }
                organ.sprite = image; shadow.sprite = image;
                float breathe = reduced ? 0 : Mathf.Sin(time * (part == 2 ? 2.2f : 1.7f) + part * 1.8f) * 0.025f;
                int act = AnimationTiming.BossAct(unit.Health, unit.Definition.Health);
                float phaseFloat = reduced || winding || release > 0 ? 0 : Mathf.Sin(time * .9f + part * 2.2f) * act * .015f;
                float recoil = reduced || hurtAge < 0 || hurtAge > 0.24f || !woundHit ? 0 : Mathf.Sin(hurtAge / 0.24f * Mathf.PI) * 0.09f;
                Vector2 outward = part == 0 ? Vector2.left : part == 1 ? Vector2.right : Vector2.down;
                float inkHeight = sizes[part] / Mathf.Max(0.01f, definition.Height);
                Vector3 flight = (BossPartRules.AnimatedOffset(unit, part) - definition.Offset) * inkHeight;
                Vector3 local = origins[part] + flight + (Vector3)outward * sizes[part] * (recoil + (reduced ? 0 : release * 0.12f - charge * 0.07f));
                local += new Vector3(part == 0 ? -phaseFloat : phaseFloat, phaseFloat, 0) * sizes[part];
                float scale = (unit.Definition.Id == "OutsideStageKeeper" || unit.Definition.Id == NightmareRules.CustomerId ? 1 : 1.25f) * sizes[part] / Mathf.Max(0.01f, image.bounds.size.y);
                organ.transform.localPosition = local * compensation;
                organ.transform.localScale = Vector3.one * scale * compensation * (1 + breathe + (reduced ? 0 : charge * 0.09f));
                organ.transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : (part == 0 ? -1 : 1) * (Mathf.Sin(time * 2 + part) * 2 + release * 12));
                organ.sortingOrder = order + 7;
                float damage = Mathf.Clamp01(BossPartRules.Damage(unit, part) / BossPartRules.Durability(unit, part));
                organ.color = Color.Lerp(Color.white, new Color(0.85f, 0.55f, 0.65f), damage * 0.6f);
                if (!PresentationSettings.ReducedFlashes && woundHit && hurtAge >= 0 && hurtAge < 0.1f) organ.color = new Color(1, 0.78f, 0.7f);
                organ.enabled = unit.Alive && (!broken || !reduced && brokenAge < 0.65f);
                shadow.enabled = unit.Alive && !broken; shadow.sortingOrder = order + 6;
                shadow.transform.localPosition = (local + new Vector3(0.015f, -0.02f) * sizes[part]) * compensation;
                shadow.transform.localRotation = organ.transform.localRotation; shadow.transform.localScale = organ.transform.localScale * 1.04f;
                shadow.color = new Color(0.09f, 0.03f, 0.13f, 0.7f);
                scar.enabled = unit.Alive && broken; scar.sortingOrder = order + 5;
                scar.transform.localPosition = origins[part] * compensation; scar.transform.localScale = Vector3.one * sizes[part] * compensation * 0.73f / Mathf.Max(0.01f, scar.sprite.bounds.size.y);
                scar.color = new Color(0.7f, 0.65f, 0.7f);
                if (scar.enabled) VisibleScars++;
                if (!broken || !organ.enabled) continue;
                organ.transform.localPosition += ((Vector3)outward * brokenAge * 1.2f + Vector3.down * brokenAge * brokenAge * 2) / Mathf.Max(0.01f, body.transform.localScale.y);
                organ.transform.localRotation = Quaternion.Euler(0, 0, (part == 0 ? -1 : 1) * brokenAge * 220);
                Color fade = organ.color; fade.a = 1 - Mathf.Clamp01(brokenAge / 0.65f); organ.color = fade;
            }
            if (BossPartRules.Has(unit, BossOrganRole.Locomotion, true)) body.transform.localRotation *= Quaternion.Euler(0, 0, -7);
        }
        public void Fade(float alpha)
        {
            foreach (SpriteRenderer renderer in organs) FadePart(renderer, alpha);
            foreach (SpriteRenderer renderer in shadows) FadePart(renderer, alpha);
            foreach (SpriteRenderer renderer in scars) FadePart(renderer, alpha);
        }
        public void CopyDeathParts(Transform target)
        {
            if (anatomy == null) return;
            for (int part = 0; part < organs.Length; part++)
            {
                SpriteRenderer source = brokenParts[part] ? scars[part] : organs[part];
                SpriteRenderer fragment = Create("BossDeathPart-" + part, target, source.sprite);
                fragment.transform.localPosition = source.transform.localPosition;
                fragment.transform.localRotation = source.transform.localRotation;
                fragment.transform.localScale = source.transform.localScale;
                fragment.color = source.color; fragment.sortingOrder = 2307;
                fragment.gameObject.AddComponent<BossPartDeathScatter>().Configure(fragment, part);
            }
        }
        private static void FadePart(SpriteRenderer renderer, float alpha) { if (renderer == null) return; Color color = renderer.color; color.a *= alpha; renderer.color = color; }
        public void Clear()
        {
            foreach (SpriteRenderer renderer in organs) Release(renderer);
            foreach (SpriteRenderer renderer in shadows) Release(renderer);
            foreach (SpriteRenderer renderer in scars) Release(renderer);
            anatomy = null;
        }
        private static void Release(SpriteRenderer renderer)
        {
            if (renderer == null) return;
            renderer.enabled = false;
            if (Application.isPlaying) Destroy(renderer.gameObject); else DestroyImmediate(renderer.gameObject);
        }
        private void OnDestroy() => Clear();
    }
}
