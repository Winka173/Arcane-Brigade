using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BossFinaleVisual
    {
        private enum Treatment { Stage, Office, Dream, Toy, Customer }
        private sealed class Pose
        {
            public SpriteRenderer View;
            public Vector3 Position;
            public Vector3 Scale;
            public Quaternion Rotation;
            public Color Color;
            public Pose(SpriteRenderer view)
            {
                View = view; Position = view.transform.localPosition; Scale = view.transform.localScale;
                Rotation = view.transform.localRotation; Color = view.color;
            }
            public void Fade(float alpha) { Color tint = Color; tint.a *= alpha; View.color = tint; }
        }
        private readonly Transform root;
        private readonly SpriteRenderer[] chips = new SpriteRenderer[12];
        private readonly SpriteRenderer[] smoke = new SpriteRenderer[4];
        private readonly SpriteRenderer[] embers = new SpriteRenderer[4];
        private readonly SpriteRenderer[] payout = new SpriteRenderer[12];
        private Pose body;
        private Pose shadow;
        private Pose[] hands;
        private Pose[] parts;
        private GroundShadow grounded;
        private Transform grip;
        private Vector3 gripPosition;
        private Quaternion gripRotation;
        private Vector3 origin;
        private Vector3 center;
        private BodyMaterial material;
        private Treatment treatment;
        private float height;
        private float sign;
        private int identity;
        private Sprite[] dustFrames;
        private Sprite[] emberFrames;
        private Color accent;
        private ExpandedPerformanceEntry paintedDefeat;
        private float capturedHeight;
        public BossFinaleVisual(Transform parent)
        {
            root = parent;
            Build(chips, "BossMaterialChip", 2310);
            Build(smoke, "BossDissipatingSmoke", 2308);
            Build(embers, "BossExtinguishingEmber", 2309);
            Build(payout, "BossPayoutPresentation", 2320);
        }
        private void Build(SpriteRenderer[] renderers, string label, int order)
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                var child = new GameObject(label + "-" + index);
                child.transform.SetParent(root, false);
                renderers[index] = child.AddComponent<SpriteRenderer>();
                renderers[index].sortingOrder = order; renderers[index].enabled = false;
            }
        }
        public void Capture(UnitState state, SpriteRenderer bodyView, SpriteRenderer shadowView, GroundShadow ground, Transform gripRoot,
            SpriteRenderer weapon, SpriteRenderer hand, SpriteRenderer drawHand, BossPartDeathScatter[] debris, Sprite chip, Sprite coin, PaintedAnimationLibrary painted)
        {
            body = new Pose(bodyView); shadow = new Pose(shadowView); grounded = ground;
            paintedDefeat = ExpandedPerformanceLibrary.Reaction(state.Definition);
            capturedHeight = paintedDefeat != null && System.Array.IndexOf(paintedDefeat.Frames, bodyView.sprite) >= 0 ? paintedDefeat.ReferenceHeight : bodyView.sprite.bounds.size.y;
            hands = new[] { new Pose(weapon), new Pose(hand), new Pose(drawHand) };
            grip = gripRoot; gripPosition = grip.localPosition; gripRotation = grip.localRotation;
            origin = root.position; center = body.Position;
            height = Mathf.Max(0.5f, state.Definition.BodyHeight); material = state.Definition.Material;
            sign = state.HitDirection.x < 0 ? -1 : 1; identity = state.Id;
            string id = state.Definition.Id ?? string.Empty;
            treatment = id == NightmareRules.CustomerId ? Treatment.Customer : id == "SleepCollector" || id == "UmbraCradle" ? Treatment.Dream :
                state.Definition.Motif == BossMotif.Office ? Treatment.Office : state.Definition.Motif == BossMotif.Toys ? Treatment.Toy : Treatment.Stage;
            accent = treatment == Treatment.Office ? new Color(0.95f, 0.71f, 0.45f) : treatment == Treatment.Dream ? new Color(0.65f, 0.58f, 0.96f) :
                treatment == Treatment.Toy ? new Color(0.45f, 0.91f, 0.81f) : new Color(1, 0.46f, 0.26f);
            parts = new Pose[debris.Length];
            for (int index = 0; index < debris.Length; index++)
            {
                debris[index].transform.SetParent(root, true);
                parts[index] = new Pose(debris[index].GetComponent<SpriteRenderer>());
            }
            dustFrames = painted == null ? null : painted.Dust;
            emberFrames = painted == null ? null : painted.Magic;
            Sprite star = K8Motion.TierStar();
            foreach (SpriteRenderer view in chips) view.sprite = chip != null ? chip : star;
            if (treatment == Treatment.Customer && NightmareAssets.Current != null)
                foreach (SpriteRenderer view in chips) view.sprite = NightmareAssets.Current.Scenery[14];
            foreach (SpriteRenderer view in smoke) view.sprite = chip != null ? chip : star;
            foreach (SpriteRenderer view in embers) view.sprite = star;
            foreach (SpriteRenderer view in payout) view.sprite = coin != null ? coin : star;
        }
        public void Hide()
        {
            foreach (SpriteRenderer view in chips) view.enabled = false;
            foreach (SpriteRenderer view in smoke) view.enabled = false;
            foreach (SpriteRenderer view in embers) view.enabled = false;
            foreach (SpriteRenderer view in payout) view.enabled = false;
        }
        public void Draw(float age)
        {
            float progress = Mathf.Clamp01(age / DeathFeedback.DurationBoss);
            bool reduced = PresentationSettings.ReducedMotion;
            float collapse = Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - 0.12f) / 1.35f));
            float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - 1.45f) / 0.95f));
            root.position = origin;
            Vector2 shape = MaterialDeathPose.Shape(material, collapse);
            float tilt = material == BodyMaterial.Metal || material == BodyMaterial.Wood ? collapse * 76 : collapse * 18;
            if (treatment == Treatment.Office) { shape.x *= 1 + collapse * 0.22f; shape.y *= 1 - collapse * 0.35f; tilt = collapse * 24; }
            if (treatment == Treatment.Dream) { shape *= 1 - collapse * 0.24f; tilt = Mathf.Sin(collapse * Mathf.PI) * 7; }
            if (treatment == Treatment.Toy) tilt = collapse * 82;
            if (treatment == Treatment.Customer)
            {
                float release = Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - .45f) / 1.25f));
                shape = new Vector2(1 - release * .08f, 1 - release * .18f);
                tilt = release * 14;
                if (NightmareAssets.Current != null)
                    body.View.sprite = NightmareAssets.Current.CustomerFrames[7];
            }
            float kick = Mathf.Sin(Mathf.Clamp01(age / 0.34f) * Mathf.PI);
            float paintedScale = 1;
            if (!reduced && paintedDefeat != null)
            {
                body.View.sprite = paintedDefeat.Frames[age < .3f ? 2 : 3];
                paintedScale = capturedHeight / Mathf.Max(.01f, paintedDefeat.ReferenceHeight);
                shape = Vector2.Lerp(Vector2.one, shape, .15f);
                tilt *= .12f;
            }
            body.View.transform.localPosition = body.Position + (reduced ? Vector3.zero : Vector3.up * height * (kick * 0.045f - collapse * 0.19f));
            body.View.transform.localScale = reduced ? body.Scale : Vector3.Scale(body.Scale, new Vector3(shape.x * paintedScale, shape.y * paintedScale, 1));
            body.View.transform.localRotation = body.Rotation * Quaternion.Euler(0, 0, reduced ? 0 : sign * tilt);
            body.Fade(fade);
            shadow.View.transform.localScale = shadow.Scale * (reduced ? 1 : 1 - collapse * 0.4f); shadow.Fade(fade);
            if (body.View.sprite != null) grounded.Display(body.View.sprite, new Vector2(origin.x, origin.y - height * 0.5f), height * 0.7f, 0, 0.36f * fade);
            grip.localPosition = gripPosition + (reduced ? Vector3.zero : new Vector3(sign * collapse * height * 0.13f, -collapse * height * 0.25f, 0));
            grip.localRotation = gripRotation * Quaternion.Euler(0, 0, reduced ? 0 : sign * collapse * 95);
            foreach (Pose pose in hands) pose.Fade(fade);
            DrawParts(age, fade, reduced);
            DrawChips(age, reduced);
            DrawDissipation(age, reduced);
            DrawPayout(age, reduced);
            if (progress >= 1) Hide();
        }
        private void DrawParts(float age, float fade, bool reduced)
        {
            for (int index = 0; index < parts.Length; index++)
            {
                Pose pose = parts[index];
                float travel = Mathf.Clamp01((age - index * 0.07f) / 1.45f);
                Vector3 offset = pose.Position - center;
                bool retract = treatment == Treatment.Dream || material == BodyMaterial.Wax || material == BodyMaterial.Fabric && treatment != Treatment.Toy;
                Vector3 position = retract ? Vector3.Lerp(pose.Position, center, Mathf.SmoothStep(0, 1, travel)) :
                    pose.Position + new Vector3((index == 0 ? -1 : index == 1 ? 1 : sign * 0.3f) * travel * height * 0.5f,
                        (Mathf.Sin(travel * Mathf.PI) * 0.32f - travel * travel * 0.55f) * height, 0);
                if (treatment == Treatment.Office) position = pose.Position + new Vector3(offset.x * travel * 0.3f, -height * travel * 0.3f, 0);
                pose.View.transform.localPosition = reduced ? pose.Position : position;
                pose.View.transform.localScale = reduced ? pose.Scale : pose.Scale * (retract ? 1 - travel * 0.88f : 1 - travel * 0.2f);
                pose.View.transform.localRotation = pose.Rotation * Quaternion.Euler(0, 0, reduced ? 0 : (index % 2 == 0 ? -1 : 1) * travel * (treatment == Treatment.Toy ? 260 : 50));
                pose.Fade(fade * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - 0.85f) / 1.05f))));
            }
        }
        private void DrawChips(float age, bool reduced)
        {
            for (int index = 0; index < chips.Length; index++)
            {
                SpriteRenderer view = chips[index];
                float travel = Mathf.Clamp01((age - 0.18f - index * 0.015f) / 1.75f);
                view.enabled = !reduced && (!PresentationSettings.ReducedEffects || index < 4) && travel > 0 && travel < 1;
                if (!view.enabled) continue;
                float angle = index * 2.399963f + identity * 0.7f;
                Vector3 drift = new Vector3(Mathf.Cos(angle) * travel * 0.7f, Mathf.Sin(angle) * travel * 0.22f + Mathf.Sin(travel * Mathf.PI) * 0.25f - travel * travel * 0.4f, 0);
                if (treatment == Treatment.Dream) drift = new Vector3(Mathf.Sin(angle + travel * 3) * travel * 0.35f, travel * 0.7f, 0);
                if (treatment == Treatment.Office) drift.x += Mathf.Sin(travel * 8 + index) * travel * 0.12f;
                if (treatment == Treatment.Customer) drift = new Vector3(Mathf.Sin(angle) * travel * .55f, travel * .85f + Mathf.Sin(travel * Mathf.PI) * .15f, 0);
                view.transform.localPosition = center + drift * height;
                float size = height * (0.028f + index % 3 * 0.009f) * (1 - travel * 0.45f);
                Vector2 aspect = material == BodyMaterial.Wax ? new Vector2(1.6f, 0.25f) : material == BodyMaterial.Fabric ? new Vector2(0.45f, 1.6f) :
                    treatment == Treatment.Office ? new Vector2(1.5f, 0.65f) : Vector2.one;
                Scale(view, size * aspect.x, size * aspect.y);
                view.transform.localRotation = Quaternion.Euler(0, 0, index * 41 + travel * (treatment == Treatment.Toy ? 420 : 130));
                Color tint = Color.Lerp(body.Color, accent, index % 2 == 0 ? 0.25f : 0.8f); tint.a = (1 - travel) * 0.85f; view.color = tint;
            }
        }
        private void DrawDissipation(float age, bool reduced)
        {
            for (int index = 0; index < smoke.Length; index++)
            {
                SpriteRenderer view = smoke[index];
                float travel = Mathf.Clamp01((age - index * 0.09f) / 1.9f);
                view.enabled = (!PresentationSettings.ReducedEffects || index < 2) && age >= index * 0.09f && travel < 1;
                if (!view.enabled) continue;
                Frame(view, dustFrames, travel);
                view.transform.localPosition = center + new Vector3((index - 1.5f) * height * 0.13f, height * (0.18f + (reduced ? 0 : travel * 0.38f)), 0);
                float size = height * (0.22f + (reduced ? 0 : travel * 0.35f)); Scale(view, size, size * 0.7f);
                Color tint = Color.Lerp(new Color(0.25f, 0.22f, 0.3f), accent, treatment == Treatment.Dream ? 0.65f : 0.2f);
                tint.a = (1 - travel) * (1 - travel) * 0.38f; view.color = tint;
            }
            for (int index = 0; index < embers.Length; index++)
            {
                SpriteRenderer view = embers[index]; float travel = Mathf.Clamp01(age / (0.55f + index * 0.13f));
                view.enabled = !PresentationSettings.ReducedEffects && travel < 1;
                if (!view.enabled) continue;
                Frame(view, emberFrames, travel);
                view.transform.localPosition = center + new Vector3((index - 1.5f) * height * 0.15f, height * (0.25f + (reduced ? 0 : travel * 0.17f)), 0);
                float size = height * 0.12f * (1 - travel * 0.85f); Scale(view, size * 0.65f, size);
                Color tint = treatment == Treatment.Dream ? accent : new Color(1, 0.64f, 0.28f);
                tint.a = (1 - travel) * (PresentationSettings.ReducedFlashes ? 0.3f : 0.65f); view.color = tint;
            }
        }
        private void DrawPayout(float age, bool reduced)
        {
            for (int index = 0; index < payout.Length; index++)
            {
                SpriteRenderer view = payout[index];
                float localAge = age - 0.72f - index % 3 * 0.045f;
                float travel = Mathf.Clamp01(localAge / 1.5f);
                view.enabled = (!PresentationSettings.ReducedEffects || index < 5) && localAge >= 0 && travel < 1;
                if (!view.enabled) continue;
                float spread = (index / (float)(payout.Length - 1) - 0.5f) * 2;
                Vector3 offset = reduced ? new Vector3(spread * 0.35f, 0.18f, 0) :
                    new Vector3(spread * (0.12f + travel * 0.75f), 0.12f + Mathf.Sin(travel * Mathf.PI) * (0.4f + index % 3 * 0.08f) - travel * 0.32f, 0);
                view.transform.localPosition = center + offset * height;
                float size = Mathf.Clamp(height * 0.085f, 0.14f, 0.3f);
                float spin = reduced ? 1 : 0.4f + Mathf.Abs(Mathf.Cos(travel * 12 + index)) * 0.6f;
                Scale(view, size * spin, size);
                view.transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : spread * travel * 95);
                float fade = Mathf.Clamp01(localAge / 0.12f) * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((travel - 0.55f) / 0.45f)));
                view.color = new Color(1, 0.82f, 0.35f, fade * (PresentationSettings.ReducedFlashes ? 0.75f : 1));
            }
        }
        private static void Scale(SpriteRenderer view, float width, float height)
        {
            if (view.sprite == null) return;
            view.transform.localScale = new Vector3(width / Mathf.Max(0.001f, view.sprite.bounds.size.x), height / Mathf.Max(0.001f, view.sprite.bounds.size.y), 1);
        }
        private static void Frame(SpriteRenderer view, Sprite[] frames, float progress)
        {
            if (frames == null || frames.Length == 0) return;
            Sprite frame = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(progress * frames.Length))];
            if (frame != null) view.sprite = frame;
        }
    }
}
