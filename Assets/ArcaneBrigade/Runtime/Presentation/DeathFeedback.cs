using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class DeathFeedback : MonoBehaviour
    {
        public const float DurationBoss = 2.4f;
        private sealed class Remnant
        {
            public Transform Root;
            public Transform Grip;
            public SpriteRenderer Body;
            public SpriteRenderer Shadow;
            public GroundShadow Grounded;
            public SpriteRenderer Weapon;
            public SpriteRenderer Hand;
            public SpriteRenderer DrawHand;
            public readonly SpriteRenderer[] Fragments = new SpriteRenderer[6];
            public Vector3 BodyPosition;
            public Vector3 BodyScale;
            public Quaternion BodyRotation;
            public Vector3 GripPosition;
            public Quaternion GripRotation;
            public Vector3 ShadowScale;
            public Color BodyColor;
            public Color ShadowColor;
            public Vector2 Direction;
            public Vector2 Position;
            public float Age = 10;
            public float Duration;
            public float Weight;
            public bool Important;
            public BodyMaterial Material;
            public int Identity;
            public bool Boss;
            public Sprite[] DefeatFrames;
            public float SourceHeight;
            public float SourcePivot;
            public bool Species;
            public EnsembleGesture Gesture;
            public int Evolution;
            public float SpeciesIdentity;
            public readonly MaterialPropertyBlock Properties = new MaterialPropertyBlock();
            public BossFinaleVisual Finale;
            public BossPartDeathScatter[] BossDebris = System.Array.Empty<BossPartDeathScatter>();
        }
        private readonly List<Remnant> pool = new List<Remnant>();
        private Sprite fragmentSprite;
        private Sprite payoutSprite;
        private PaintedAnimationLibrary painted;
        public int ActiveCount { get; private set; }
        public void Configure(Sprite sprite, Sprite coin = null, PaintedAnimationLibrary effects = null) { fragmentSprite = sprite; payoutSprite = coin; painted = effects; }
        private static SpriteRenderer Part(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.AddComponent<SpriteRenderer>();
        }
        private Remnant Acquire(bool important)
        {
            Remnant remnant = pool.Find(candidate => candidate.Age >= candidate.Duration);
            if (remnant != null) return remnant;
            if (pool.Count >= 64)
            {
                if (!important) return null;
                Remnant oldest = null;
                foreach (Remnant candidate in pool) if (!candidate.Important && (oldest == null || candidate.Age > oldest.Age)) oldest = candidate;
                return oldest;
            }
            var root = new GameObject("DeathRemnant").transform;
            root.SetParent(transform, false);
            var grip = new GameObject("FallingGrip").transform;
            grip.SetParent(root, false);
            remnant = new Remnant { Root = root, Grip = grip, Body = Part("Body", root), Shadow = Part("Shadow", root), Weapon = Part("Weapon", grip), Hand = Part("Hand", grip), DrawHand = Part("DrawHand", grip) };
            remnant.Grounded = root.gameObject.AddComponent<GroundShadow>();
            for (int index = 0; index < remnant.Fragments.Length; index++) remnant.Fragments[index] = Part("PaperChip_" + index, root);
            pool.Add(remnant);
            return remnant;
        }
        public void Play(UnitVisual visual, UnitState state)
        {
            if (visual == null || state == null || state.Definition == null) return;
            bool important = state.Definition.IsBoss || state.Faction == Faction.Champion;
            Remnant remnant = Acquire(important);
            if (remnant == null) return;
            if (state.Faction == Faction.Enemy && !state.Definition.IsBoss) EnemyAffixDeparture.Play(state, visual.BodyRenderer);
            foreach (BossPartDeathScatter debris in remnant.BossDebris)
                if (debris != null) { debris.gameObject.SetActive(false); debris.transform.SetParent(null); Destroy(debris.gameObject); }
            visual.CopyDeathPose(remnant.Body, remnant.Shadow, remnant.Weapon, remnant.Hand, remnant.DrawHand, remnant.Grip);
            remnant.BossDebris = state.Definition.IsBoss ? remnant.Body.GetComponentsInChildren<BossPartDeathScatter>() : System.Array.Empty<BossPartDeathScatter>();
            if (important)
            {
                remnant.Body.sortingOrder = 2300;
                remnant.Weapon.sortingOrder = 2302;
                remnant.Hand.sortingOrder = 2303;
                remnant.DrawHand.sortingOrder = 2304;
            }
            remnant.Position = state.Position;
            remnant.Root.position = state.Position;
            remnant.BodyPosition = remnant.Body.transform.localPosition;
            remnant.BodyScale = remnant.Body.transform.localScale;
            remnant.SourceHeight = remnant.Body.sprite.bounds.size.y;
            remnant.SourcePivot = remnant.Body.sprite.pivot.y / remnant.Body.sprite.rect.height;
            remnant.BodyRotation = remnant.Body.transform.localRotation;
            remnant.BodyColor = state.Definition.Tint;
            remnant.ShadowColor = remnant.Shadow.color;
            remnant.ShadowScale = remnant.Shadow.transform.localScale;
            remnant.GripPosition = remnant.Grip.localPosition;
            remnant.GripRotation = remnant.Grip.localRotation;
            remnant.Direction = state.HitDirection.sqrMagnitude > 0.001f ? state.HitDirection.normalized : Vector2.up;
            remnant.Weight = state.Definition.IsBoss ? 1.6f : state.Faction == Faction.Champion ? 1.3f : 1;
            remnant.Important = important;
            remnant.Material = state.Definition.Material;
            remnant.Identity = state.Definition.BossPattern + state.Id % 3;
            remnant.Boss = state.Definition.IsBoss;
            remnant.DefeatFrames = visual.ActingArt?.Defeat;
            ExpandedPerformanceEntry reaction = ExpandedPerformanceLibrary.Reaction(state.Definition);
            if (reaction != null) remnant.DefeatFrames = new[] { reaction.Frames[2], reaction.Frames[3] };
            remnant.Species = state.Faction == Faction.Enemy && !state.Definition.IsBoss;
            remnant.Gesture = EnsembleMotion.For(state.Definition);
            remnant.Evolution = state.Definition.EvolutionStage;
            remnant.SpeciesIdentity = AnimationTiming.Phase(state.Definition.Id);
            remnant.Duration = remnant.Boss ? DurationBoss : state.Faction == Faction.Champion ? 0.95f : 0.65f;
            remnant.Age = 0;
            foreach (SpriteRenderer fragment in remnant.Fragments) { fragment.sprite = fragmentSprite; fragment.enabled = false; fragment.sortingOrder = remnant.Body.sortingOrder + 5; }
            if (remnant.Finale != null) remnant.Finale.Hide();
            if (remnant.Boss)
            {
                if (remnant.Finale == null) remnant.Finale = new BossFinaleVisual(remnant.Root);
                remnant.Finale.Capture(state, remnant.Body, remnant.Shadow, remnant.Grounded, remnant.Grip, remnant.Weapon, remnant.Hand, remnant.DrawHand, remnant.BossDebris, fragmentSprite, payoutSprite, painted);
            }
            remnant.Root.gameObject.SetActive(true);
            Draw(remnant);
            ActiveCount = 0;
            foreach (Remnant candidate in pool) if (candidate.Age < candidate.Duration) ActiveCount++;
        }
        public void Clear()
        {
            foreach (Remnant remnant in pool) { remnant.Age = remnant.Duration; remnant.Root.gameObject.SetActive(false); }
            ActiveCount = 0;
        }
        public void Advance(float delta)
        {
            if (delta <= 0 || !float.IsFinite(delta)) return;
            ActiveCount = 0;
            foreach (Remnant remnant in pool)
            {
                if (remnant.Age >= remnant.Duration) continue;
                remnant.Age += delta;
                if (remnant.Age >= remnant.Duration) { remnant.Root.gameObject.SetActive(false); continue; }
                ActiveCount++;
                Draw(remnant);
                if (!remnant.Boss) foreach (BossPartDeathScatter debris in remnant.BossDebris) if (debris != null) debris.Step(delta);
            }
        }
        private static void Draw(Remnant remnant)
        {
            if (remnant.Boss) { remnant.Finale.Draw(remnant.Age); return; }
            bool authoredDefeat = !PresentationSettings.ReducedMotion && remnant.DefeatFrames != null && remnant.DefeatFrames.Length > 0;
            if (authoredDefeat)
                remnant.Body.sprite = CharacterActingEntry.Frame(remnant.DefeatFrames, remnant.Age / Mathf.Max(.01f, remnant.Duration * .65f));
            float progress = Mathf.Clamp01(remnant.Age / remnant.Duration);
            float kick = Mathf.Sin(Mathf.Clamp01(progress / 0.25f) * Mathf.PI);
            float collapse = Mathf.SmoothStep(0, 1, Mathf.Clamp01((progress - 0.16f) / 0.52f));
            if (remnant.Material == BodyMaterial.Metal || remnant.Material == BodyMaterial.Wood) collapse *= 0.3f;
            if (remnant.Material == BodyMaterial.Glass) collapse = Mathf.Clamp01(collapse * 1.5f);
            float alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((progress - 0.45f) / 0.5f));
            remnant.Root.position = (Vector3)(remnant.Position + remnant.Direction * (0.22f * Mathf.SmoothStep(0, 1, progress)));
            float pivotOffset = (remnant.Body.sprite.pivot.y / remnant.Body.sprite.rect.height - remnant.SourcePivot) * remnant.SourceHeight * remnant.BodyScale.y;
            remnant.Body.transform.localPosition = remnant.BodyPosition + Vector3.up * (pivotOffset + kick * 0.17f * remnant.Weight - collapse * 0.09f);
            Vector2 shape = MaterialDeathPose.Shape(remnant.Material, collapse);
            if (authoredDefeat) shape = Vector2.Lerp(Vector2.one, shape, .22f);
            float poseScale = remnant.SourceHeight / Mathf.Max(.01f, remnant.Body.sprite.bounds.size.y);
            remnant.Body.transform.localScale = Vector3.Scale(remnant.BodyScale, new Vector3(shape.x * poseScale, shape.y * poseScale, 1));
            float materialTilt = remnant.Material == BodyMaterial.Metal ? progress * 70 : remnant.Material == BodyMaterial.Fabric ? Mathf.Sin(progress * Mathf.PI) * 12 : kick * 14 + collapse * 25;
            if (authoredDefeat) materialTilt *= .2f;
            remnant.Body.transform.localRotation = remnant.BodyRotation * Quaternion.Euler(0, 0, (remnant.Direction.x < 0 ? -1 : 1) * materialTilt);
            if (remnant.Species)
            {
                Vector3 acting = SpeciesDefeatPerformance.Drift(remnant.Gesture, progress, remnant.SpeciesIdentity, remnant.Evolution, PresentationSettings.ReducedMotion);
                remnant.Body.transform.localPosition += new Vector3(acting.x, acting.y, 0);
                remnant.Body.transform.localRotation = remnant.BodyRotation * Quaternion.Euler(0, 0, acting.z);
                remnant.Body.transform.localScale = Vector3.Scale(remnant.BodyScale, new Vector3(Mathf.Lerp(1, shape.x, .22f) * poseScale, Mathf.Lerp(1, shape.y, .22f) * poseScale, 1));
                remnant.Properties.Clear();
                Rect rectangle = remnant.Body.sprite.rect; Texture texture = remnant.Body.sprite.texture;
                remnant.Properties.SetVector("_UvRect", new Vector4(rectangle.x / texture.width, rectangle.y / texture.height, rectangle.width / texture.width, rectangle.height / texture.height));
                remnant.Properties.SetVector("_Dissolve", SpeciesDefeatPerformance.Dissolve(remnant.Gesture, progress, remnant.SpeciesIdentity, remnant.Evolution, PresentationSettings.ReducedMotion));
                remnant.Body.SetPropertyBlock(remnant.Properties);
            }
            remnant.Body.color = PresentationSettings.ReducedFlashes ? remnant.BodyColor : Color.Lerp(Color.white, remnant.BodyColor, Mathf.Clamp01(progress / 0.09f));
            Color bodyColor = remnant.Body.color; bodyColor.a = alpha; remnant.Body.color = bodyColor;
            remnant.Shadow.transform.localScale = remnant.ShadowScale * (1 - collapse * 0.45f);
            Color shadowColor = remnant.ShadowColor; shadowColor.a *= alpha; remnant.Shadow.color = shadowColor;
            remnant.Grounded.Display(remnant.Body.sprite, new Vector2(remnant.Root.position.x, remnant.Position.y - remnant.BodyScale.y * remnant.SourceHeight * 0.5f), remnant.BodyScale.x * poseScale * remnant.Body.sprite.bounds.size.x * 0.75f, kick * 0.17f, 0.34f * alpha);
            remnant.Grip.localPosition = remnant.GripPosition + new Vector3(remnant.Direction.x * progress * 0.22f, Mathf.Sin(progress * Mathf.PI) * 0.18f - progress * 0.20f, 0);
            remnant.Grip.localRotation = remnant.GripRotation * Quaternion.Euler(0, 0, progress * 80 * (remnant.Direction.x < 0 ? -1 : 1));
            SetAlpha(remnant.Weapon, alpha); SetAlpha(remnant.Hand, alpha); SetAlpha(remnant.DrawHand, alpha);
            for (int index = 0; index < remnant.Fragments.Length; index++)
            {
                SpriteRenderer fragment = remnant.Fragments[index];
                fragment.enabled = !PresentationSettings.ReducedMotion && (!PresentationSettings.ReducedEffects || index < 2) && progress > 0.2f && fragment.sprite != null;
                if (!fragment.enabled) continue;
                float travel = Mathf.Clamp01((progress - 0.2f) / 0.8f);
                float angle = index * 2.399963f + remnant.Identity * 0.7f;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                fragment.transform.localPosition = remnant.BodyPosition + (Vector3)(direction * (travel * 0.65f * remnant.Weight) + Vector2.up * Mathf.Sin(travel * Mathf.PI) * 0.20f);
                float size = (0.05f + index % 3 * 0.015f) * (1 - travel * 0.4f);
                fragment.transform.localScale = new Vector3(size, size * 0.45f, 1) / fragment.sprite.bounds.size.x;
                if (remnant.Material == BodyMaterial.Metal) fragment.transform.localScale = new Vector3(size, size, 1) / fragment.sprite.bounds.size.x;
                if (remnant.Material == BodyMaterial.Wax) fragment.transform.localScale = new Vector3(size * 1.4f, size * 0.2f, 1) / fragment.sprite.bounds.size.x;
                if (remnant.Material == BodyMaterial.Fabric) fragment.transform.localScale = new Vector3(size * 0.4f, size * 1.8f, 1) / fragment.sprite.bounds.size.x;
                fragment.transform.localRotation = Quaternion.Euler(0, 0, index * 39 + travel * 170);
                Color color = index % 2 == 0 ? remnant.BodyColor : new Color(0.79f, 0.70f, 0.89f);
                if (remnant.Material == BodyMaterial.Metal) color = index % 2 == 0 ? new Color(1f, 0.78f, 0.32f) : new Color(0.65f, 0.93f, 0.9f);
                if (remnant.Material == BodyMaterial.Glass) color = new Color(0.65f, 1f, 0.94f);
                color.a = 1 - travel;
                fragment.color = color;
            }
        }
        private static void SetAlpha(SpriteRenderer renderer, float alpha) { Color color = renderer.color; color.a = alpha; renderer.color = color; }
    }
}
