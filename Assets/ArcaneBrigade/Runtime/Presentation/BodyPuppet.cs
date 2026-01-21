using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BodyPuppet : MonoBehaviour
    {
        private sealed class Parts { public Sprite Upper; public Sprite Lower; public Vector2 UpperOffset; public Vector2 LowerOffset; }
        private static readonly Dictionary<Sprite, Parts> cache = new Dictionary<Sprite, Parts>();
        private SpriteRenderer upper;
        private SpriteRenderer lower;
        private MaterialPropertyBlock properties;
        public void Display(SpriteRenderer reference, UnitState state, float time, bool reduced)
        {
            if (properties == null) properties = new MaterialPropertyBlock();
            if (state.Definition.AnimationFrames.Length < 4)
            {
                reference.enabled = true;
                if (upper != null) { upper.enabled = false; lower.enabled = false; }
                return;
            }
            if (upper == null) { upper = Create("UpperBody"); lower = Create("LowerBody"); }
            bool moving = (state.Position - state.PreviousPosition).sqrMagnitude > 0.000001f;
            bool attacking = K8Motion.Attacking(state, time);
            bool reacting = attacking || time - state.LastHit < 0.18f || !state.Alive;
            if (reacting) moving = false;
            int legFrame = UnitVisual.AnimationFrame(time * (state.TravelMode == CompanionTravelMode.Pursuit ? 1.45f : 1), state.Id, moving, reduced);
            int idleFrame = reacting ? 0 : UnitVisual.AnimationFrame(time, state.Id, false, reduced);
            int headFrame = idleFrame == 1 ? 1 : legFrame;
            if (state.Faction == Faction.Enemy) headFrame = legFrame = K8Motion.BodyFrame(state, time, reduced);
            Parts head = GetParts(state.Definition.AnimationFrames[headFrame]);
            Parts feet = GetParts(state.Definition.AnimationFrames[legFrame]);
            upper.sprite = head.Upper; lower.sprite = feet.Lower;
            upper.transform.localPosition = head.UpperOffset;
            lower.transform.localPosition = feet.LowerOffset;
            if (reference.flipX)
            {
                upper.transform.localPosition = new Vector3(-head.UpperOffset.x, head.UpperOffset.y, 0);
                lower.transform.localPosition = new Vector3(-feet.LowerOffset.x, feet.LowerOffset.y, 0);
            }
            var profile = new SpeciesMotionProfile(state.Definition);
            float hitAge = time - state.LastHit;
            float recoil = reduced || hitAge < 0 || hitAge > 0.32f ? 0 : Mathf.Sin(hitAge / 0.32f * Mathf.PI) * Mathf.Exp(-hitAge * 6);
            float gesturePhase = Mathf.Repeat(time + state.Id * 0.73f, 6.4f);
            float gesture = reduced || reacting || moving || gesturePhase > 0.85f ? 0 : Mathf.Sin(gesturePhase / 0.85f * Mathf.PI);
            float cadence = profile.IdleCadence;
            float breathing = reduced || reacting ? 0 : Mathf.Sin(time * cadence + state.Id * 0.8f) * (state.Definition.Material == BodyMaterial.Metal ? 0.012f : 0.025f);
            if (!reduced && !reacting && state.Definition.Id == "CoilKettle") breathing += Mathf.Sin(time * 4.5f) * 0.008f;
            if (!reduced && !reacting && state.Definition.Id == "ThimbleMoth") breathing += Mathf.Sin(time * 7 + state.Id) * 0.012f;
            if (!reduced && !reacting && state.Definition.Id == "ReelOwl") gesture *= 1.5f;
            upper.transform.localScale = new Vector3(1 - breathing * 0.3f, 1 + breathing, 1);
            upper.transform.localPosition += new Vector3(reduced || reacting ? 0 : Mathf.Sin(time * 1.8f + state.Id) * 0.012f, breathing * 0.18f, 0);
            Vector2 hitDirection = state.HitDirection.sqrMagnitude > 0 ? state.HitDirection.normalized : Vector2.left;
            upper.transform.localPosition += (Vector3)(hitDirection * recoil * profile.Recoil) + Vector3.up * gesture * profile.IdleGesture;
            if (!reduced && gesture > 0)
            {
                float direction = reference.flipX ? -1 : 1;
                if (profile.GestureKind == 0) upper.transform.localPosition += Vector3.right * direction * gesture * 0.025f;
                else if (profile.GestureKind == 1) upper.transform.localScale = Vector3.Scale(upper.transform.localScale, new Vector3(1 + gesture * 0.025f, 1 - gesture * 0.025f, 1));
                else if (profile.GestureKind == 2) lower.transform.localPosition += Vector3.down * gesture * 0.02f;
                else upper.transform.localPosition += Vector3.right * Mathf.Sin(gesturePhase * 14) * gesture * 0.018f;
            }
            float windup = state.AttackWindup > 0 ? state.AttackWindup : state.Definition.Weapon.Windup;
            K8Motion.Pose pose = K8Motion.Attack(state.Definition.Weapon, time - state.AttackStarted, windup, reduced);
            upper.transform.localPosition += Vector3.up * pose.Lift * 0.25f;
            float supportAge = time - state.LastSupport;
            float support = reduced || reacting || supportAge < 0 || supportAge >= 0.5f ? 0 : Mathf.Sin(supportAge / 0.5f * Mathf.PI);
            upper.transform.localPosition += Vector3.up * support * 0.035f;
            float bossCharge = reduced || !state.Definition.IsBoss || state.TelegraphUntil <= time ? 0 : Mathf.SmoothStep(0, 1, 1 - (state.TelegraphUntil - time) / 0.85f);
            if (bossCharge > 0)
            {
                float flourish = Mathf.Sin(bossCharge * Mathf.PI);
                bool office = state.Definition.Motif == BossMotif.Office;
                upper.transform.localPosition += new Vector3(office ? flourish * 0.025f : 0, flourish * (office ? 0.045f : 0.08f), 0);
                upper.transform.localScale = Vector3.Scale(upper.transform.localScale, new Vector3(1 + flourish * (office ? 0.06f : -0.035f), 1 + flourish * (office ? -0.025f : 0.05f), 1));
            }
            float turn = reduced || reacting ? 0 : Mathf.Sin(time * (moving ? profile.GaitCadence * 0.6f : cadence) + state.Id) * profile.HeadTurn;
            upper.transform.localRotation = Quaternion.Euler(0, 0, turn + (pose.Lean * 0.4f + support * 4 + gesture * profile.HeadTurn * 2 + recoil * 7) * (reference.flipX ? -1 : 1));
            float gait = reduced || !moving ? 0 : Mathf.Sin(time * profile.GaitCadence * (state.TravelMode == CompanionTravelMode.Pursuit ? 1.45f : 1) + state.Id) * profile.GaitAmplitude;
            float squash = reduced ? 0 : pose.Squash * 0.4f + recoil * profile.LowerSquash - gesture * profile.LowerSquash * 0.5f;
            lower.transform.localScale = new Vector3(1 + gait + squash, 1 - gait * 0.6f - squash, 1);
            if (!reduced && moving)
            {
                float footLift = state.Definition.Material == BodyMaterial.Glass ? Mathf.Sin(time * cadence + state.Id) * 0.018f : Mathf.Max(0, Mathf.Sin(time * profile.GaitCadence + state.Id)) * profile.GaitAmplitude * 0.6f;
                lower.transform.localPosition += Vector3.up * footLift;
            }
            lower.transform.localRotation = Quaternion.Euler(0, 0, state.Definition.Material == BodyMaterial.Glass || reduced ? 0 : gait * 28);
            upper.sharedMaterial = lower.sharedMaterial = reference.sharedMaterial;
            upper.color = lower.color = reference.color;
            upper.flipX = lower.flipX = reference.flipX;
            upper.sortingOrder = reference.sortingOrder + 1; lower.sortingOrder = reference.sortingOrder;
            reference.GetPropertyBlock(properties); upper.SetPropertyBlock(properties); lower.SetPropertyBlock(properties);
            reference.enabled = false; upper.enabled = lower.enabled = true;
        }
        private SpriteRenderer Create(string caption)
        {
            var child = new GameObject(caption); child.transform.SetParent(transform, false); return child.AddComponent<SpriteRenderer>();
        }
        private static Parts GetParts(Sprite frame)
        {
            if (cache.TryGetValue(frame, out Parts result)) return result;
            Rect full = frame.rect; float split = full.y + full.height * 0.37f;
            Rect top = new Rect(full.x, split - 1, full.width, full.yMax - split + 1);
            Rect bottom = new Rect(full.x, full.y, full.width, split - full.y + 1);
            Vector2 origin = full.position + frame.pivot;
            result = new Parts { Upper = Sprite.Create(frame.texture, top, new Vector2(0.5f, 0.5f), frame.pixelsPerUnit, 0, SpriteMeshType.FullRect), Lower = Sprite.Create(frame.texture, bottom, new Vector2(0.5f, 0.5f), frame.pixelsPerUnit, 0, SpriteMeshType.FullRect), UpperOffset = (top.center - origin) / frame.pixelsPerUnit, LowerOffset = (bottom.center - origin) / frame.pixelsPerUnit };
            cache.Add(frame, result); return result;
        }
        public void Fade(float alpha)
        {
            if (upper == null) return;
            Color color = upper.color; color.a *= alpha; upper.color = lower.color = color;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            foreach (Parts parts in cache.Values) { Object.Destroy(parts.Upper); Object.Destroy(parts.Lower); }
            cache.Clear();
        }
    }
}
