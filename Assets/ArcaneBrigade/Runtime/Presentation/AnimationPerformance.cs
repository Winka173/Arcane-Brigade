using UnityEngine;

namespace ArcaneBrigade
{
    public static class AnimationTiming
    {
        public static float Pulse(float age, float duration) => age < 0 || age >= duration || duration <= 0 ? 0 : Mathf.Sin(age / duration * Mathf.PI);
        public static float Settle(float age, float duration, float oscillations = 2) => age < 0 || age >= duration || duration <= 0 ? 0 : Mathf.Sin(age / duration * Mathf.PI * oscillations) * Mathf.Pow(1 - age / duration, 2);
        public static float Blend(float rate, float delta) => 1 - Mathf.Exp(-rate * Mathf.Clamp(delta, 0, .05f));
        public static float Phase(string identity)
        {
            uint hash = 2166136261;
            foreach (char character in identity ?? string.Empty) hash = unchecked((hash ^ character) * 16777619);
            return (hash % 4093) / 4093f;
        }
        public static int BossAct(float health, float maximum) => maximum <= 0 ? 0 : health <= maximum / 3 ? 2 : health <= maximum * 2 / 3 ? 1 : 0;
    }

    public sealed class AnimationPerformance
    {
        private Vector2 inertia;
        private Vector2 previousVelocity;
        private float previousTime = -1;
        private float restStarted;
        private float supportAt = -100;
        private float hurtAt = -100;
        private float health;
        private bool moving;
        private float identity;
        private float turn;
        private float previousFacing;
        public Vector2 BodyOffset { get; private set; }
        public float BodyTurn { get; private set; }
        public float GripTurn { get; private set; }
        public float GripLift { get; private set; }
        public float Gesture { get; private set; }
        public float GesturePhase { get; private set; } = -1;
        public float Weight { get; private set; }
        public void Reset(UnitState unit)
        {
            inertia = previousVelocity = Vector2.zero; previousTime = -1; restStarted = 0;
            health = unit.Health; supportAt = hurtAt = -100; moving = false; turn = 0;
            identity = AnimationTiming.Phase(unit.Definition.Id) * 13 + unit.Id * .719f;
            previousFacing = Mathf.Atan2(unit.Facing.y, unit.Facing.x) * Mathf.Rad2Deg;
            ClearPose();
        }
        private void ClearPose() { BodyOffset = Vector2.zero; BodyTurn = GripTurn = GripLift = Gesture = Weight = 0; GesturePhase = -1; }
        public void Sample(UnitState unit, float time, bool reduced, bool attacking)
        {
            float delta = previousTime < 0 ? 0 : Mathf.Clamp(time - previousTime, 0, .05f);
            if (previousTime >= 0 && time == previousTime) { if (reduced) ClearPose(); return; }
            if (time < previousTime) Reset(unit);
            previousTime = time;
            Vector2 velocity = Vector2.ClampMagnitude(unit.Velocity, 12);
            bool travelling = velocity.sqrMagnitude > .04f || (unit.Position - unit.PreviousPosition).sqrMagnitude > .00001f;
            if (moving && !travelling) restStarted = time;
            if (unit.Health > health + .001f) supportAt = time;
            if (unit.Health < health - .001f) hurtAt = time;
            health = unit.Health; moving = travelling;
            Vector2 acceleration = delta > 0 ? Vector2.ClampMagnitude((velocity - previousVelocity) / delta, 35) : Vector2.zero;
            previousVelocity = velocity;
            inertia = Vector2.Lerp(inertia, acceleration * -.0014f, AnimationTiming.Blend(13, delta));
            float facing = Mathf.Atan2(unit.Facing.y, unit.Facing.x) * Mathf.Rad2Deg;
            float difference = Mathf.Clamp(Mathf.DeltaAngle(previousFacing, facing), -45, 45);
            turn = Mathf.Lerp(turn, difference * -.1f, AnimationTiming.Blend(16, delta)); previousFacing = facing;
            ClearPose();
            if (reduced || !unit.Alive) { inertia = Vector2.zero; turn = 0; return; }
            float size = Mathf.Min(1.3f, K8Motion.BodyHeight(unit));
            float stiffness = unit.Definition.Material == BodyMaterial.Metal || unit.Definition.Material == BodyMaterial.Wood ? .45f : 1;
            BodyOffset = Vector2.ClampMagnitude(inertia, .045f) * size * stiffness;
            BodyTurn = (Mathf.Clamp(-velocity.x * .6f, -4, 4) + turn) * stiffness;
            GripTurn = Mathf.Clamp(velocity.x * -1.1f + turn * 1.5f, -7, 7);
            GripLift = Mathf.Clamp(inertia.y, -.025f, .025f);
            float idleAge = time - restStarted;
            float gestureAge = Mathf.Repeat(time + identity, 8.7f);
            Gesture = !travelling && !attacking && idleAge > .65f ? AnimationTiming.Pulse(gestureAge, 1.3f) : 0;
            if (Gesture > 0) GesturePhase = gestureAge / 1.3f;
            int acting = Mathf.FloorToInt(identity) % 4;
            if (Gesture > 0)
            {
                BodyTurn += Gesture * (acting == 0 ? -3 : acting == 1 ? 2 : 0);
                BodyOffset += new Vector2(acting == 2 ? Mathf.Sin(gestureAge * 5) * .016f : 0, acting == 3 ? Gesture * -.018f : Gesture * .012f) * size;
                GripTurn += Gesture * (acting % 2 == 0 ? 6 : -4);
                if (unit.Faction == Faction.Champion)
                {
                    switch (unit.Definition.HeroMechanic)
                    {
                        case HeroMechanic.LanternStride: GripTurn += Gesture * -5; BodyTurn += Gesture * 1.5f; break;
                        case HeroMechanic.ParcelExpress: BodyOffset += Vector2.down * Gesture * .018f; GripLift += Gesture * .018f; break;
                        case HeroMechanic.DreamBreath: BodyTurn += Gesture * Mathf.Sin(gestureAge * 2) * 2; break;
                        case HeroMechanic.RootedOracle: GripLift += Gesture * .03f; break;
                        case HeroMechanic.SoloMaw: Weight += Gesture * .012f; break;
                        case HeroMechanic.GlassGambler: BodyTurn += Mathf.Round(Gesture * 3) * .5f; break;
                        case HeroMechanic.TideCollector: BodyOffset += Vector2.up * Gesture * .025f; break;
                        case HeroMechanic.ClockRunner: GripTurn += Mathf.Round(Gesture * 4) * .6f; break;
                        case HeroMechanic.MirrorGuard: BodyTurn -= Gesture * 2; GripTurn += Gesture * 3; break;
                        case HeroMechanic.TreasureTax: BodyOffset += Vector2.right * Gesture * .02f; break;
                    }
                }
            }
            float healed = AnimationTiming.Pulse(time - supportAt, .65f);
            float injured = AnimationTiming.Pulse(time - hurtAt, .25f);
            BodyOffset += Vector2.up * healed * size * .025f;
            GripTurn += injured * Mathf.Clamp(unit.HitDirection.x * 8, -8, 8);
            if (unit.Faction == Faction.Enemy && !attacking && !unit.Definition.IsBoss)
            {
                float charge = unit.ContactImpactAt > time ? AnimationTiming.Pulse(.35f - (unit.ContactImpactAt - time), .35f) : 0;
                Weight = charge * .035f;
                BodyTurn -= unit.Facing.x * charge * 4;
                if (unit.Definition.Behavior == EnemyBehavior.CoinThief && unit.StolenCoins > 0) BodyTurn -= unit.Facing.x * 4;
                if (unit.Definition.Behavior == EnemyBehavior.Charger) BodyOffset -= unit.Facing.normalized * charge * .02f;
                if (unit.Definition.Behavior == EnemyBehavior.Hopper && unit.BodyLift > 0) BodyTurn += Mathf.Sin(time * 5 + identity) * 2;
            }
            if (unit.Faction == Faction.Champion && unit.Health < unit.Definition.Health * .25f && !travelling && !attacking) BodyOffset += Vector2.down * .015f;
            if (attacking) { BodyOffset *= .3f; BodyTurn *= .3f; GripTurn *= .2f; }
        }
        public static Vector4 WeaponFlex(WeaponDefinition weapon, float age, float windup, bool reduced)
        {
            if (reduced || age < 0 || age >= windup + weapon.Recovery) return Vector4.zero;
            float charge = age < windup ? Mathf.SmoothStep(0, 1, age / Mathf.Max(.01f, windup)) : 0;
            float release = AnimationTiming.Settle(age - windup, .28f, 4);
            float bend = weapon.Motion == WeaponMotion.Bow ? charge * .035f + release * .018f : 0;
            float shutter = weapon.Motion == WeaponMotion.Camera ? AnimationTiming.Pulse(age - windup, .12f) * .04f : 0;
            float hinge = weapon.Motion == WeaponMotion.Scissors ? (charge * -.035f + AnimationTiming.Settle(age - windup, .26f) * .045f) : 0;
            return new Vector4(bend, shutter, hinge, 0);
        }
    }
}
