using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>How our shots fly (18/10/2026): Bizarre Brigade-style patterns instead of every shot going dead straight.
    /// Sideways patterns (wave, helix, flutter, zigzag, spiral, curve) are drawn: the shot is hit-tested on its true line and
    /// shown swaying round it (PatternOffset), so a pattern never makes a shot miss. Speed patterns (accelerate, stutter)
    /// and the orbit loop steer the shot itself. Shots with their own motion (boomerangs, yo-yos, bounces) are left alone.</summary>
    public enum ShotPattern { None, Wave, Curve, Stutter, Accelerate, Helix, Orbit, Flutter, Spiral, Zigzag }

    public static class ShotPatterns
    {
        private static readonly Dictionary<string, ShotPattern> Table = new Dictionary<string, ShotPattern>
        {
            ["TeapotPopgun"] = ShotPattern.Wave, ["MusicBox"] = ShotPattern.Wave, ["RibbonHarp"] = ShotPattern.Wave, ["LanternStaff"] = ShotPattern.Wave,
            ["GumballGatling"] = ShotPattern.Wave, ["SaucerBlaster"] = ShotPattern.Wave, ["InkyOctopus"] = ShotPattern.Wave,
            ["MailboxLauncher"] = ShotPattern.Curve, ["AcornSling"] = ShotPattern.Curve, ["CatcherMitt"] = ShotPattern.Curve,
            ["BellFork"] = ShotPattern.Stutter, ["FanfareHorn"] = ShotPattern.Stutter,
            ["CrescentBow"] = ShotPattern.Accelerate, ["PlasmaTrident"] = ShotPattern.Accelerate, ["CorkRifle"] = ShotPattern.Accelerate, ["InkPistol"] = ShotPattern.Accelerate,
            ["RuneStaff"] = ShotPattern.Helix, ["KunaiFan"] = ShotPattern.Helix, ["ButtonBow"] = ShotPattern.Helix,
            ["DartCompass"] = ShotPattern.Orbit, ["DreamOrbStaff"] = ShotPattern.Orbit,
            ["SleepCamera"] = ShotPattern.Flutter, ["BubbleWand"] = ShotPattern.Flutter, ["FireExtinguisher"] = ShotPattern.Flutter, ["MoonFeatherFan"] = ShotPattern.Flutter,
            ["LeafCrossbow"] = ShotPattern.Flutter, ["ConfettiBlunderbuss"] = ShotPattern.Flutter,
            ["ShellCannon"] = ShotPattern.Spiral, ["SnowGlobeOrb"] = ShotPattern.Spiral, ["TalismanIngot"] = ShotPattern.Spiral,
            ["StarKazoo"] = ShotPattern.Zigzag,
        };
        public static ShotPattern For(WeaponDefinition weapon) => weapon != null && Table.TryGetValue(weapon.Id, out ShotPattern p) ? p : ShotPattern.None;
        public static IEnumerable<KeyValuePair<string, ShotPattern>> All => Table;

        /// <summary>Steer one shot for this step. The first call fixes its axis and speed; the side of a helix or curve
        /// alternates shot by shot.</summary>
        public static void Steer(ProjectileState shot, float delta)
        {
            if (shot.Pattern == ShotPattern.None || shot.Returning) return;
            if (shot.PatternSpeed <= 0)
            {
                shot.PatternSpeed = shot.Velocity.magnitude; if (shot.PatternSpeed < 0.01f) { shot.Pattern = ShotPattern.None; return; }
                shot.PatternAxis = shot.Velocity / shot.PatternSpeed; shot.PatternSide = (shot.Id & 1) == 0 ? 1 : -1;
            }
            shot.PatternAge += delta;
            float t = shot.PatternAge, v = shot.PatternSpeed, forward = 1;
            Vector2 axis = shot.PatternAxis, normal = new Vector2(-axis.y, axis.x) * shot.PatternSide;
            // sideways patterns: how far (world units) the drawn shot strays from its true line; it eases in from the muzzle
            float d = 0, ease = Mathf.Min(1, t * 6);
            switch (shot.Pattern)
            {
                case ShotPattern.Wave: d = .22f * Mathf.Sin(t * 10); break;                              // a gentle sine sway
                case ShotPattern.Helix: d = .25f * Mathf.Sin(t * 13); break;                             // pairs of shots twist round each other
                case ShotPattern.Flutter: d = .3f * Mathf.Sin(t * 4.5f) + .05f * Mathf.Sin(t * 17); break; // drifting like a leaf
                case ShotPattern.Zigzag: d = .18f * (Mathf.PingPong(t / 0.12f, 2) - 1); break;
                case ShotPattern.Spiral: d = .22f * Mathf.Min(1, t * 2.5f) * Mathf.Sin(t * 16); break;     // a corkscrew seen side-on
                case ShotPattern.Curve: d = t < 0.6f ? .45f * Mathf.Sin(Mathf.PI * t / 0.6f) : 0; break;  // bows out and back onto the line
                case ShotPattern.Accelerate: forward = Mathf.Lerp(0.45f, 1.55f, Mathf.Clamp01(t / 0.45f)); break;
                case ShotPattern.Stutter: forward = Mathf.Repeat(t, 0.3f) < 0.18f ? 1.45f : 0.3f; break;
                case ShotPattern.Orbit:
                    // a quick loop in front of the muzzle, then away down the aim
                    if (t < 0.32f) { float a = t / 0.32f * Mathf.PI * 2; shot.Velocity = (Vector2)(Quaternion.Euler(0, 0, a * Mathf.Rad2Deg * shot.PatternSide) * axis) * v * 1.1f; return; }
                    break;
            }
            shot.PatternOffset = normal * d * ease;
            if (shot.Pattern == ShotPattern.Accelerate || shot.Pattern == ShotPattern.Stutter || shot.Pattern == ShotPattern.Orbit)
                shot.Velocity = axis * v * forward;
        }
    }
}
