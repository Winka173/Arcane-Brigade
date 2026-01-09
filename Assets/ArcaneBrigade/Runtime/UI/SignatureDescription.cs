namespace ArcaneBrigade
{
    public static class SignatureDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier, bool reflectiveInk = false)
        {
            if (weapon.Family != ExpandedWeapon.None) return ExpandedDescription.Describe(weapon, tier);
            if (weapon.Signature == SignatureWeapon.None) return weapon.Kind == WeaponKind.Projectile ? weapon.ProjectileCount + " projectiles per attack · pierces " + weapon.ProjectilePierce + " additional targets.\nOne projectile cannot hit the same target twice." : "";
            string details = weapon.Signature == SignatureWeapon.Stamp
                ? "One target: " + weapon.SignatureBeats + " same-source marks → burst for " + (weapon.SignatureScale * 100).ToString("0") + "% damage. Each strike adds " + (reflectiveInk ? 2 : 1) + " marks; expires after " + UnityEngine.Mathf.Max(0.1f, weapon.MarkLifetime * (reflectiveInk ? 0.65f : 1)).ToString("0.##") + " seconds without a hit."
                : "Every " + weapon.SignatureBeats + " attacks → " + (weapon.Signature == SignatureWeapon.Bell ? "circular wave" : "forward umbrella burst") + " sau " + weapon.SignatureDelay.ToString("0.##") + " seconds, " + (weapon.SignatureScale * 100).ToString("0") + "% damage; radius " + weapon.SignatureRadius.ToString("0.##") + ".";
            if (tier >= 2) details += weapon.Signature == SignatureWeapon.Stamp ? "\nTier 2+: transfers at most one mark to a nearby target; cannot trigger another burst recursively." : weapon.Signature == SignatureWeapon.Bell ? "\nTier 2+: one weaker 35% echo after 0.3 seconds; echoes cannot create echoes." : "\nTier 2+: umbrella burst radius ×1.25, without added bullet interception.";
            return details + "\nSpecial attacks do not trigger item procs; beats and marks reset each wave.";
        }
    }
}
