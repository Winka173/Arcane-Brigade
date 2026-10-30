using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum CompanionRole { Striker, Shooter, Guard, Support, Summoner, Controller }

    /// <summary>The design roster defines companion classes; legacy content falls back to its mechanics.
    /// Shown as the painted badge on its card and on the Combos page.</summary>
    public static class CompanionRoles
    {
        private static readonly Dictionary<string, CompanionRole> cache = new Dictionary<string, CompanionRole>();
        private static readonly Dictionary<string, Sprite> badges = new Dictionary<string, Sprite>();

        public static CompanionRole For(UnitDefinition unit)
        {
            if (unit == null) return CompanionRole.Striker;
            if (CompanionClassTuning.TryGetRole(unit.Id, out CompanionRole authored)) return authored;
            if (cache.TryGetValue(unit.Id, out CompanionRole known)) return known;
            string text = ((CombatSimulation.SkillCopy.TryGetValue(unit.Id, out string skill) ? skill : "") + " " + CompanionDescriptions.ShortTalent(unit)).ToLowerInvariant();
            CompanionRole role;
            if (text.Contains("summon") || text.Contains("spawns") || text.Contains("calls a") || text.Contains("helper")) role = CompanionRole.Summoner;
            else if (unit.Passive == PassiveKind.ChillAura || text.Contains("slow") || text.Contains("freeze") || text.Contains("chill") || text.Contains("stun") || text.Contains("pull")) role = CompanionRole.Controller;
            else if (unit.Passive == PassiveKind.Rally || unit.Passive == PassiveKind.Scavenge || text.Contains("friends gain") || text.Contains("other friends") || text.Contains("each friend") || text.Contains("your hero")) role = CompanionRole.Support;
            else if (unit.Weapon != null && unit.Weapon.Kind == WeaponKind.Projectile) role = CompanionRole.Shooter;
            else if (unit.Radius >= .36f || unit.Weapon != null && (unit.Weapon.Motion == WeaponMotion.Hammer || unit.Weapon.Motion == WeaponMotion.Slam)) role = CompanionRole.Guard;
            else role = CompanionRole.Striker;
            cache[unit.Id] = role; return role;
        }
        public static string Word(CompanionRole role) => role.ToString().ToUpperInvariant();
        /// <summary>What the role does, in a few words (the card's first line).</summary>
        public static string Tagline(CompanionRole role) => role switch
        {
            CompanionRole.Shooter => "Shooter: hits from far away.",
            CompanionRole.Guard => "Guard: stands in front and pushes back.",
            CompanionRole.Support => "Support: makes the others stronger.",
            CompanionRole.Summoner => "Summoner: brings helpers to the fight.",
            CompanionRole.Controller => "Controller: slows and holds enemies.",
            _ => "Striker: hits hard up close.",
        };
        public static Sprite Badge(CompanionRole role) => Load("Role" + role);
        public static Sprite ComboBadge => Load("RoleCombo");
        public static Sprite UnknownBadge => Load("RoleUnknown");
        private static Sprite Load(string name)
        {
            if (!badges.TryGetValue(name, out Sprite s)) { s = Resources.Load<Sprite>("UI/Roles/" + name); badges[name] = s; }
            return s;
        }
    }
}
