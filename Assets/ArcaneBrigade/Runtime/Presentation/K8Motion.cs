using UnityEngine;

namespace ArcaneBrigade
{
    public static class K8Motion
    {
        public readonly struct Pose
        {
            public readonly float Angle;
            public readonly float Recoil;
            public readonly float Lean;
            public readonly float Squash;
            public readonly float Lift;
            public Pose(float angle, float recoil, float lean, float squash, float lift)
            {
                Angle = angle; Recoil = recoil; Lean = lean; Squash = squash; Lift = lift;
            }
        }

        public static float BodyHeight(UnitState unit)
        {
            float scale = Mathf.Max(0.01f, unit.Definition.BodyScale);
            return unit.Faction == Faction.Companion ? Mathf.Min(unit.Definition.BodyHeight / scale * 0.95f, 0.88f * CompanionTierScaling.Body(unit.Definition.CombatTier)) * scale : unit.Definition.BodyHeight;
        }

        public static float WeaponWidth(UnitState unit)
        {
            float width = unit.Definition.Weapon.VisualWidth * (unit.Faction != Faction.Enemy && unit.Definition.Weapon.Kind == WeaponKind.Projectile ? 1.45f : 1);
            // a held item stays modest, never much larger than its bearer (theme rule; 18/10/2026: melee was allowed 2.7x and heroes had no cap)
            float limit = unit.Definition.Weapon.Kind == WeaponKind.Melee ? 1.05f : 0.9f;
            if (unit.Faction == Faction.Enemy) return width;
            return Mathf.Min(width * (unit.Faction == Faction.Companion ? 0.95f : 1f), BodyHeight(unit) * limit);
        }

        public static bool Attacking(UnitState unit, float time)
        {
            float windup = unit.AttackWindup > 0 ? unit.AttackWindup : unit.Definition.Weapon.Windup;
            float age = time - unit.AttackStarted;
            return unit.Alive && age >= 0 && age < windup + unit.Definition.Weapon.Recovery;
        }

        public static int BodyFrame(UnitState unit, float time, bool reduced)
        {
            if (unit.Faction == Faction.Enemy && unit.Definition.AnimationFrames.Length >= 4 && !unit.Definition.IsBoss)
            {
                if (reduced) return 0;
                float attackAge = time - unit.AttackStarted;
                float prepare = unit.AttackWindup > 0 ? unit.AttackWindup : unit.Definition.Weapon.Windup;
                if (time - unit.LastHit < 0.18f) return 3;
                if (unit.TelegraphUntil > time || unit.ContactImpactAt > time || unit.PendingAttack && attackAge < prepare) return 1;
                if (Attacking(unit, time)) return 2;
                return 0;
            }
            if (!unit.Definition.IsBoss)
                return UnitVisual.AnimationFrame(time, unit.Id, (unit.Position - unit.PreviousPosition).sqrMagnitude > 0.000001f, reduced);
            float windup = unit.AttackWindup > 0 ? unit.AttackWindup : unit.Definition.Weapon.Windup;
            float age = time - unit.AttackStarted;
            if (unit.TelegraphUntil > time || unit.PendingAttack && age >= 0 && age < windup) return 2;
            if (unit.BossRecoveryUntil > time) return 1;
            if (!unit.Alive || time - unit.LastHit >= 0 && time - unit.LastHit < 0.18f || Attacking(unit, time) && age >= windup) return 3;
            return UnitVisual.AnimationFrame(time, unit.Id, false, reduced);
        }

        public static Pose Attack(WeaponDefinition weapon, float age, float windup, bool reduced)
        {
            if (reduced || age < 0 || age >= windup + weapon.Recovery) return default;
            bool preparing = age < windup;
            float charge = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / Mathf.Max(0.001f, windup)));
            float recovery = Mathf.Clamp01((age - windup) / Mathf.Max(0.001f, weapon.Recovery));
            float release = Mathf.SmoothStep(0, 1, recovery / 0.18f);
            float settle = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((recovery - 0.18f) / 0.82f));
            float angle = preparing ? -weapon.SwingAngle * charge : Mathf.Lerp(-weapon.SwingAngle, weapon.SwingAngle, release) * settle;
            float recoil = preparing ? 0.06f * charge : Mathf.Lerp(0.06f, -0.08f, release) * settle;
            float lift = 0;
            WeaponMotion motion = weapon.Motion;
            // a shot's heft (RangedFeel): a gumball flick barely kicks, a cannon shoves the holder back and its barrel climbs
            float kick = weapon.Kind == WeaponKind.Projectile ? weapon.RecoilDistance * RangedFeel.RecoilScale(weapon) : weapon.RecoilDistance;
            if (weapon.Kind == WeaponKind.Projectile)
            {
                angle = preparing ? -8 * charge : Mathf.Lerp(-8, RangedFeel.TiltDegrees(weapon), release) * settle;
                recoil = preparing ? -0.025f * charge : Mathf.Lerp(-0.025f, kick, release) * settle;
            }
            if (motion == WeaponMotion.Thrust || motion == WeaponMotion.Stamp || motion == WeaponMotion.Iron)
            {
                angle = 0;
                float reach = motion == WeaponMotion.Stamp ? 0.22f : motion == WeaponMotion.Iron ? 0.13f : 0.24f;
                recoil = preparing ? 0.10f * charge : Mathf.Lerp(0.10f, -reach, release) * settle;
            }
            if (motion == WeaponMotion.Slam || motion == WeaponMotion.Hammer)
            {
                angle = preparing ? -75 * charge : Mathf.Lerp(-75, 38, release) * settle;
                lift = preparing ? 0.07f * charge : Mathf.Lerp(0.07f, -0.035f, release) * settle;
            }
            if (motion == WeaponMotion.Cast)
            {
                angle = preparing ? -24 * charge : Mathf.Lerp(-24, 14, release) * settle;
                lift = preparing ? 0.04f * charge : Mathf.Lerp(0.04f, 0.02f, release) * settle;
            }
            if (motion == WeaponMotion.Bow)
            {
                angle = 0;
                recoil = preparing ? 0.06f * charge : Mathf.Lerp(0.06f, kick, release) * settle;
            }
            if (motion == WeaponMotion.Bell || weapon.Family == ExpandedWeapon.Alarm)
                angle = preparing ? -16 * charge : Mathf.Lerp(-16, Mathf.Sin(recovery * Mathf.PI * 6) * 16, release) * settle;
            if (motion == WeaponMotion.Scissors)
                angle = preparing ? -28 * charge : Mathf.Lerp(-28, Mathf.Sin(recovery * Mathf.PI * 3) * 22, release) * settle;
            if (motion == WeaponMotion.Camera)
            {
                angle = preparing ? -5 * charge : Mathf.Lerp(-5, 6, release) * settle;
                recoil = preparing ? -0.03f * charge : Mathf.Lerp(-0.03f, 0.08f, release) * settle;
            }
            if (motion == WeaponMotion.Music)
            {
                angle = preparing ? 12 * charge : Mathf.Lerp(12, Mathf.Sin(recovery * Mathf.PI * 2) * 13, release) * settle;
                lift = preparing ? 0.035f * charge : Mathf.Lerp(0.035f, 0.025f, release) * settle;
            }
            if (weapon.Family == ExpandedWeapon.Tea)
            {
                angle = preparing ? -30 * charge : Mathf.Lerp(-30, 35, release) * settle;
                lift = 0.025f * (preparing ? charge : settle);
            }
            if (weapon.Family == ExpandedWeapon.Fish)
            {
                angle = preparing ? -42 * charge : Mathf.Lerp(-42, 28, release) * settle;
                recoil = preparing ? 0.08f * charge : Mathf.Lerp(0.08f, -0.12f, release) * settle;
            }
            float lean = weapon.BodyAttackTilt * (preparing ? -0.4f * charge : Mathf.Lerp(-0.4f, 1, release) * settle);
            if (weapon.Mechanic == CarnivalWeapon.SpringHammer)
            {
                recoil = preparing ? charge * 0.22f : Mathf.Lerp(0.22f, -0.32f, release) * settle;
                angle = preparing ? -48 * charge : Mathf.Lerp(-48, 30, release) * settle;
                lift = preparing ? charge * 0.1f : -0.08f * release * settle;
            }
            else if (weapon.Mechanic == CarnivalWeapon.RibbonWheel)
                angle = preparing ? -25 * charge : Mathf.Lerp(-25, recovery * 300, release) * settle;
            else if (weapon.Mechanic == CarnivalWeapon.YoYo)
            { angle = preparing ? -38 * charge : Mathf.Lerp(-38, 65, release) * settle; recoil = -0.15f * release * settle; }
            else if (weapon.Mechanic == CarnivalWeapon.Sewing)
                lift = preparing ? 0 : Mathf.Sin(recovery * Mathf.PI * 12) * 0.035f * settle;
            else if (weapon.Mechanic == CarnivalWeapon.Projector)
            { angle = preparing ? -8 * charge : Mathf.Lerp(-8, 5, release) * settle; recoil = -0.05f * release * settle; }
            else if (weapon.Mechanic == CarnivalWeapon.Magnet)
                recoil = preparing ? -0.04f * charge : 0.14f * settle;
            if (!preparing)
            {
                float weight = motion == WeaponMotion.Hammer || motion == WeaponMotion.Slam || motion == WeaponMotion.Stamp ? 1 : motion == WeaponMotion.Recoil ? .25f + .5f * RangedFeel.Heft(weapon) : .25f;
                float ring = AnimationTiming.Settle(age - windup, Mathf.Min(.3f, weapon.Recovery), 3);
                angle += ring * weight * 4;
                recoil += ring * weight * .018f;
            }
            return new Pose(angle, recoil, lean, preparing ? -0.025f * charge : Mathf.Lerp(-0.025f, 0.055f, release) * settle, lift);
        }

        private static Sprite star;
        private static Texture2D starTexture;
        public static Sprite TierStar()
        {
            if (star != null) return star;
            starTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false) { name = "K8MotionTierStar", filterMode = FilterMode.Bilinear };
            var points = new Vector2[10];
            for (int index = 0; index < points.Length; index++)
            {
                float angle = Mathf.PI * 0.5f + index * Mathf.PI / 5;
                points[index] = new Vector2(16, 16) + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (index % 2 == 0 ? 14 : 6);
            }
            var pixels = new Color[1024];
            for (int row = 0; row < 32; row++)
            for (int column = 0; column < 32; column++)
            {
                Vector2 point = new Vector2(column + 0.5f, row + 0.5f);
                bool inside = false;
                for (int index = 0, previous = 9; index < 10; previous = index++)
                {
                    Vector2 first = points[index], second = points[previous];
                    if ((first.y > point.y) != (second.y > point.y) && point.x < (second.x - first.x) * (point.y - first.y) / (second.y - first.y) + first.x) inside = !inside;
                }
                pixels[row * 32 + column] = inside ? Color.white : Color.clear;
            }
            starTexture.SetPixels(pixels); starTexture.Apply(false, true);
            star = Sprite.Create(starTexture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
            return star;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            if (star != null) Object.Destroy(star);
            if (starTexture != null) Object.Destroy(starTexture);
            star = null; starTexture = null;
        }
    }
}
