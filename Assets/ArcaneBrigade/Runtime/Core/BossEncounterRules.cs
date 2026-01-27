using UnityEngine;

namespace ArcaneBrigade
{
    public static class BossEncounterRules
    {
        public const float HealthMultiplier = 1.6f;
        public const float DamageMultiplier = 1.18f;
        public const float SpeedMultiplier = 1.08f;
        public const float CollisionScale = 1.45f;
        public const float SignatureWindup = 1.15f;
        public static float BodyScale(int pattern) => pattern == 6 || pattern == 9 ? 2.3f : 2.1f;
        public static bool IsSignature(int volley) => volley % 4 == 3;
        public static bool Leaps(int pattern, int variation) => pattern == 2 || pattern == 6 || pattern == 9 || pattern == 5 && variation == 1;
        public static float Windup(UnitState boss) => boss.Definition.Id == "OutsideStageKeeper" ? 1.35f : boss.ActiveBossSignature >= 0 ? SignatureWindup : 0.85f;
        public static string SignatureName(int pattern, int variation)
        {
            string[] names = pattern switch
            {
                0 => new[] { "Royal Stampede", "Dead Letter Cross" },
                1 => new[] { "Quill Guillotine", "Spilled Archive" },
                2 => new[] { "Pillow Drop", "Nightmare Hop" },
                3 => new[] { "Anchor Rake", "Melting Broadside" },
                4 => new[] { "Shutter Cage", "Fuel Rain" },
                5 => new[] { "Curtain Call", "Stage Dive" },
                6 => new[] { "Cradle Crash", "Shadow Lullaby" },
                7 => new[] { "Vault Scissor", "Bank Run" },
                8 => new[] { "Bass Procession", "Falling Requiem" },
                _ => new[] { "Lost Luggage Drop", "Leviathan Landing" }
            };
            return names[Mathf.Clamp(variation, 0, 1)];
        }
    }
}
