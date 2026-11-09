using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a defeated body does on screen. Ordinary enemies POP: a white hit flash, the body squashes against
    /// the blow and bursts within a fifth of a second (ExplosionFeedback paints the ink burst around it) — no lingering
    /// ragdoll, dissolve or flying plates. Bosses keep their long finale and end in a big blast; the hero keeps a fall.</summary>
    public sealed class DeathFeedback : MonoBehaviour
    {
        public const float DurationBoss = 2.4f;
        /// <summary>Length of an ordinary enemy's squash-and-burst (before force scaling).</summary>
        public const float PopDuration = 0.2f;
        /// <summary>An enemy with death art acts its death inside its blast (requirement 09/11/2026: together, no waiting, nothing
        /// left after): struck for StruckSeconds, slumped until ActedDeathLead, then burst and gone by ActedDeathDuration, before the
        /// blast's own 0.3 s fireball is out.</summary>
        public const float StruckSeconds = 0.07f, ActedDeathLead = 0.16f, ActedDeathDuration = 0.28f;
        /// <summary>When, inside the boss finale, the body goes up in its final blast.</summary>
        public const float BossBlastAt = 1.38f;
        private static readonly int FlashId = Shader.PropertyToID("_Flash"), UvRectId = Shader.PropertyToID("_UvRect"), DissolveId = Shader.PropertyToID("_Dissolve");
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
            public float Force = 1; // crit kills and overkill launch the body further and spin it harder
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
            public bool Pop;          // an ordinary enemy: squash and burst, then gone
            public bool BossBlasted;  // the finale's closing blast has gone off
            public float Height;
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
            if (pool.Count >= 200) // enough that every enemy on screen at the end of a night plays its own defeat (64 left most to a plain fade)
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
        /// <summary>Returns false when every remnant is busy (the caller then shows a cheap burst instead of nothing).</summary>
        /// <param name="quiet">A night's last enemies leaving when it is won: the species' own defeat (no burst, no affix departure).</param>
        public bool Play(UnitVisual visual, UnitState state, float force = 1f, bool quiet = false)
        {
            if (visual == null || state == null || state.Definition == null) return false;
            bool important = state.Definition.IsBoss || state.Faction == Faction.Champion;
            Remnant remnant = Acquire(important);
            if (remnant == null) return false;
            remnant.Force = PresentationSettings.ReducedMotion ? 1f : Mathf.Clamp(force, 1f, 3f);
            if (!quiet && state.Faction == Faction.Enemy && !state.Definition.IsBoss) EnemyAffixDeparture.Play(state, visual.BodyRenderer);
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
            // a reaction sheet holds calm, struck, rearing and slumped poses: an enemy death is struck then slumped (a boss keeps rearing, slumped)
            if (reaction != null) remnant.DefeatFrames = state.Definition.IsBoss ? new[] { reaction.Frames[2], reaction.Frames[3] } : new[] { reaction.Frames[1], reaction.Frames[3] };
            remnant.Species = state.Faction == Faction.Enemy && !state.Definition.IsBoss;
            remnant.Gesture = EnsembleMotion.For(state.Definition);
            remnant.Evolution = state.Definition.EvolutionStage;
            remnant.SpeciesIdentity = AnimationTiming.Phase(state.Definition.Id);
            remnant.Pop = remnant.Species && !quiet;
            remnant.BossBlasted = false;
            remnant.Height = Mathf.Max(0.3f, state.Definition.BodyHeight);
            // a pop holds the struck (hurt) pose; the long painted collapse belongs to heroes and bosses
            bool acted = remnant.Pop && reaction != null && !PresentationSettings.ReducedMotion;
            if (remnant.Pop && !acted && remnant.DefeatFrames != null && remnant.DefeatFrames.Length > 0) remnant.DefeatFrames = new[] { remnant.DefeatFrames[0] };
            remnant.Duration = remnant.Boss ? DurationBoss : remnant.Pop ? (acted ? ActedDeathDuration : PopDuration * (1f + 0.15f * (remnant.Force - 1f))) : state.Faction == Faction.Champion ? 0.95f : 0.65f + 0.12f * (remnant.Force - 1f);
            remnant.Age = 0;
            foreach (SpriteRenderer fragment in remnant.Fragments) { fragment.sprite = fragmentSprite; fragment.enabled = false; fragment.sortingOrder = remnant.Body.sortingOrder + 5; }
            if (!remnant.Pop) { remnant.Properties.Clear(); remnant.Body.SetPropertyBlock(remnant.Properties); }
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
            return true;
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
                if (!remnant.Boss) { foreach (BossPartDeathScatter debris in remnant.BossDebris) if (debris != null) debris.Step(delta); }
                else if (!remnant.BossBlasted && remnant.Age >= BossBlastAt)
                {
                    // the finale ends with the body going up: a big blast as it starts to fade
                    remnant.BossBlasted = true;
                    ExplosionFeedback.Active?.BossFinale(remnant.Position + Vector2.up * remnant.Height * 0.35f, remnant.Height);
                }
            }
        }
        private static void Draw(Remnant remnant)
        {
            if (remnant.Boss) { remnant.Finale.Draw(remnant.Age); return; }
            if (remnant.Pop) { DrawPop(remnant); return; }
            bool authoredDefeat = !PresentationSettings.ReducedMotion && remnant.DefeatFrames != null && remnant.DefeatFrames.Length > 0;
            if (authoredDefeat)
                remnant.Body.sprite = CharacterActingEntry.Frame(remnant.DefeatFrames, remnant.Age / Mathf.Max(.01f, remnant.Duration * .65f));
            float progress = Mathf.Clamp01(remnant.Age / remnant.Duration);
            float kick = Mathf.Sin(Mathf.Clamp01(progress / 0.25f) * Mathf.PI);
            float collapse = Mathf.SmoothStep(0, 1, Mathf.Clamp01((progress - 0.16f) / 0.52f));
            if (remnant.Material == BodyMaterial.Metal || remnant.Material == BodyMaterial.Wood) collapse *= 0.3f;
            if (remnant.Material == BodyMaterial.Glass) collapse = Mathf.Clamp01(collapse * 1.5f);
            float alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((progress - 0.45f) / 0.5f));
            remnant.Root.position = (Vector3)(remnant.Position + remnant.Direction * (0.22f * remnant.Force * Mathf.SmoothStep(0, 1, progress)));
            float pivotOffset = (remnant.Body.sprite.pivot.y / remnant.Body.sprite.rect.height - remnant.SourcePivot) * remnant.SourceHeight * remnant.BodyScale.y;
            remnant.Body.transform.localPosition = remnant.BodyPosition + Vector3.up * (pivotOffset + kick * 0.17f * remnant.Weight * Mathf.Sqrt(remnant.Force) - collapse * 0.09f);
            Vector2 shape = MaterialDeathPose.Shape(remnant.Material, collapse);
            if (authoredDefeat) shape = Vector2.Lerp(Vector2.one, shape, .22f);
            float poseScale = remnant.SourceHeight / Mathf.Max(.01f, remnant.Body.sprite.bounds.size.y);
            remnant.Body.transform.localScale = Vector3.Scale(remnant.BodyScale, new Vector3(shape.x * poseScale, shape.y * poseScale, 1));
            float materialTilt = Mathf.Lerp(1f, remnant.Force, 0.8f) * (remnant.Material == BodyMaterial.Metal ? progress * 70 : remnant.Material == BodyMaterial.Fabric ? Mathf.Sin(progress * Mathf.PI) * 12 : kick * 14 + collapse * 25);
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
        /// <summary>An ordinary enemy's death: struck white, squashed flat against the blow, then swelling and bursting
        /// apart while the ink burst (ExplosionFeedback.Pop) blooms over it. Done within ~0.2 s; no body is left lying.</summary>
        private static void DrawPop(Remnant remnant)
        {
            bool still = PresentationSettings.ReducedMotion;
            // an acted death: the struck pose under the blow, then slumped where it fell until its blast; otherwise one struck pose
            bool acted = !still && remnant.DefeatFrames != null && remnant.DefeatFrames.Length > 1 && remnant.DefeatFrames[1] != null;
            if (remnant.DefeatFrames != null && remnant.DefeatFrames.Length > 0 && remnant.DefeatFrames[0] != null && !still)
                remnant.Body.sprite = acted && remnant.Age >= StruckSeconds ? remnant.DefeatFrames[1] : remnant.DefeatFrames[0];
            float t = Mathf.Clamp01(remnant.Age / Mathf.Max(0.01f, remnant.Duration));
            float hold = acted ? ActedDeathLead / ActedDeathDuration : 0.38f; // share of the pop spent under the hit (and slumped, when acted)
            float squash = t < hold ? Mathf.Sin(t / hold * Mathf.PI * 0.5f) : 1f;
            float burst = t < hold ? 0f : (t - hold) / (1f - hold);
            float punch = Mathf.Sqrt(remnant.Force);
            float give = acted ? 0.35f : 1f; // a slumped pose already lies low: only a little squash on top of it
            float sx = still ? 1f : Mathf.Lerp(1f, 1f + 0.26f * punch * give, squash) + 0.16f * burst * punch;
            float sy = still ? 1f : Mathf.Lerp(1f, 1f - 0.3f * give, squash) + 0.26f * burst * punch;
            float alpha = 1f - Mathf.Pow(burst, 0.55f); // gone fast: the explosion, not a swelling white body, carries the burst
            float poseScale = remnant.SourceHeight / Mathf.Max(.01f, remnant.Body.sprite.bounds.size.y);
            // squash about the feet, swell about the middle as it comes apart
            float pivot = remnant.Body.sprite.pivot.y / remnant.Body.sprite.rect.height;
            float feet = pivot * remnant.SourceHeight * remnant.BodyScale.y * (Mathf.Lerp(1f, 0.7f, squash) - 1f) * (1f - burst);
            float pivotOffset = (pivot - remnant.SourcePivot) * remnant.SourceHeight * remnant.BodyScale.y;
            Vector2 knock = still ? Vector2.zero : remnant.Direction * (0.07f * punch * squash);
            remnant.Root.position = (Vector3)(remnant.Position + knock);
            remnant.Body.transform.localPosition = remnant.BodyPosition + Vector3.up * (pivotOffset + feet);
            remnant.Body.transform.localScale = Vector3.Scale(remnant.BodyScale, new Vector3(sx * poseScale, sy * poseScale, 1));
            remnant.Body.transform.localRotation = remnant.BodyRotation * Quaternion.Euler(0, 0, still ? 0 : (remnant.Direction.x < 0 ? 1 : -1) * 6f * squash * (1f - burst));
            // the hit flash: full white on the blow (a soft warm tint with reduced flashes), then it catches fire as it bursts
            float flash = PresentationSettings.ReducedFlashes ? 0.25f * (1f - t) : t < hold ? 1f : 0.45f * (1f - burst);
            remnant.Properties.Clear();
            Rect rectangle = remnant.Body.sprite.rect; Texture texture = remnant.Body.sprite.texture;
            remnant.Properties.SetVector(UvRectId, new Vector4(rectangle.x / texture.width, rectangle.y / texture.height, rectangle.width / texture.width, rectangle.height / texture.height));
            remnant.Properties.SetVector(DissolveId, Vector4.zero);
            remnant.Properties.SetFloat(FlashId, flash);
            remnant.Body.SetPropertyBlock(remnant.Properties);
            Color body = t < hold || PresentationSettings.ReducedFlashes ? Color.Lerp(PresentationSettings.ReducedFlashes ? remnant.BodyColor : Color.white, remnant.BodyColor, burst) : Color.Lerp(new Color(1f, 0.62f, 0.3f), remnant.BodyColor * new Color(1f, 0.5f, 0.3f), burst); body.a = alpha; remnant.Body.color = body;
            remnant.Shadow.transform.localScale = remnant.ShadowScale * (1f + 0.25f * squash - 0.6f * burst);
            Color shadow = remnant.ShadowColor; shadow.a *= alpha; remnant.Shadow.color = shadow;
            remnant.Grounded.Display(remnant.Body.sprite, new Vector2(remnant.Root.position.x, remnant.Position.y - remnant.BodyScale.y * remnant.SourceHeight * 0.5f), remnant.BodyScale.x * poseScale * remnant.Body.sprite.bounds.size.x * 0.75f, 0, 0.34f * alpha);
            // whatever it held is knocked loose and goes with the body
            remnant.Grip.localPosition = remnant.GripPosition + (still ? Vector3.zero : new Vector3(remnant.Direction.x * 0.18f * burst, 0.12f * burst, 0));
            remnant.Grip.localRotation = remnant.GripRotation * Quaternion.Euler(0, 0, still ? 0 : burst * 70 * (remnant.Direction.x < 0 ? -1 : 1));
            float held = alpha * alpha;
            SetAlpha(remnant.Weapon, held); SetAlpha(remnant.Hand, held); SetAlpha(remnant.DrawHand, held);
            foreach (SpriteRenderer fragment in remnant.Fragments) fragment.enabled = false;
        }
        private static void SetAlpha(SpriteRenderer renderer, float alpha) { Color color = renderer.color; color.a = alpha; renderer.color = color; }
    }
}
