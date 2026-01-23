using UnityEngine;

namespace ArcaneBrigade
{
    public static class HostileBallistics
    {
        public static Vector2 Velocity(HostileShot motion, Vector2 launch, float age, int identity)
        {
            float speed = Mathf.Min(2.4f, launch.magnitude);
            float turn = motion switch
            {
                HostileShot.Weave => Mathf.Sin(age * 3.2f) * 26,
                HostileShot.Hook => Mathf.Min(age, 1.8f) * (identity % 2 == 0 ? 28 : -28),
                HostileShot.Drift => Mathf.Sin(age * 1.4f) * 48,
                _ => 0
            };
            if (motion == HostileShot.Accelerate) speed *= Mathf.Lerp(0.35f, 1, Mathf.Clamp01(age / 1.5f));
            return (Vector2)(Quaternion.Euler(0, 0, turn) * launch.normalized) * speed;
        }

        public static void Advance(ProjectileState projectile, float delta)
        {
            if (projectile.HostileMotion == HostileShot.Straight) return;
            if (projectile.LaunchVelocity.sqrMagnitude < 0.0001f) projectile.LaunchVelocity = projectile.Velocity;
            projectile.HostileAge += delta;
            projectile.Velocity = Velocity(projectile.HostileMotion, projectile.LaunchVelocity, projectile.HostileAge, projectile.Id);
        }
    }
}
