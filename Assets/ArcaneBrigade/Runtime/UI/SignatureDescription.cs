namespace ArcaneBrigade
{
    public static class SignatureDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier, bool reflectiveInk = false)
        {
            if (weapon.Mechanic == CarnivalWeapon.BloomOrb) return weapon.GameplaySummary + "\nOne delayed bloom per primary hit; fragments cannot bloom again.";
            if (weapon.Mechanic == CarnivalWeapon.EchoBlade) return weapon.GameplaySummary + "\nThe echo stays where the first slash was cast.";
            if (weapon.Mechanic == CarnivalWeapon.SatelliteDisc) return weapon.GameplaySummary + "\nEach target takes one hit per throw, including the return path.";
            if (weapon.Id == "DreamOrbStaff") return "Dream elite weapon. One giant, slow orb; pierces three targets before tier/stat bonuses.";
            if (weapon.Id == "FireExtinguisher") return "Six weak foam pellets in a short cone. Reach is limited by their short lifetime.";
            if (weapon.Id == "PlasmaTrident") return "Three plasma bolts; each pierces one additional target before tier/stat bonuses.";
            if (weapon.Id == "MoonFeatherFan") return "Four weak feathers spread wide. Each pierces one additional target before tier/stat bonuses.";
            if (weapon.Id == "ClockCrescent") return "Dream elite weapon. Homes in on a target it has not hit yet, then returns on a hit or after 0.7s. Each target takes one hit per throw.";
            if (weapon.Id == "ParasolNeedle") return "Long, narrow thrust. Position to line up enemies; melee range and damage bonuses apply.";
            if (weapon.Arsenal != ArsenalWeapon.None) return ArsenalDescription.Describe(weapon, tier);
            if (weapon.Family != ExpandedWeapon.None) return ExpandedDescription.Describe(weapon, tier);
            if (weapon.Signature == SignatureWeapon.None) return weapon.Kind == WeaponKind.Projectile ? weapon.ProjectileCount + " projectiles per attack · pierces " + weapon.ProjectilePierce + " additional targets.\nOne projectile cannot hit the same target twice." : "";
            string details = weapon.Signature == SignatureWeapon.Stamp
                ? "One target: " + weapon.SignatureBeats + " same-source marks → burst for " + (weapon.SignatureScale * 100).ToString("0") + "% damage. Each strike adds " + (reflectiveInk ? 2 : 1) + " marks; expires after " + UnityEngine.Mathf.Max(0.1f, weapon.MarkLifetime * (reflectiveInk ? 0.65f : 1)).ToString("0.##") + " seconds without a hit."
                : "Every attack → " + (weapon.Signature == SignatureWeapon.Bell ? "a fan of 5 chimes that pierce 2 more foes" : "a fan of 5 rain drops that pierce 1 more foe") + " after " + weapon.SignatureDelay.ToString("0.##") + " seconds, " + (weapon.SignatureScale * 100f / UnityEngine.Mathf.Max(2, weapon.SignatureBeats)).ToString("0") + "% damage each, each foe struck once per volley; reach " + (weapon.SignatureRadius * (weapon.Signature == SignatureWeapon.Bell ? 1.5f : 1.15f)).ToString("0.#") + ".";
            if (tier >= 2) details += weapon.Signature == SignatureWeapon.Stamp ? "\nTier 2+: transfers at most one mark to a nearby target; cannot trigger another burst recursively." : weapon.Signature == SignatureWeapon.Bell ? "\nTier 2+: a weaker echo of 3 chimes at 35% after 0.3 seconds; echoes cannot create echoes." : "\nTier 2+: drops fly 25% farther, without added bullet interception.";
            return details + "\nSpecial attacks do not set off item effects; beats and marks reset each wave.";
        }
    }
}
