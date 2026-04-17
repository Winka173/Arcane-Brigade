namespace ArcaneBrigade
{
    public static class SignatureBuildGraph
    {
        public static string Status(string id, RunSession session, RunCatalog catalog)
            => IsActive(id, session, catalog) ? "BRANCH ACTIVE · full set not required" : "INACTIVE · missing weapon or connecting relic";
        public static bool IsActive(string id, RunSession session, RunCatalog catalog)
        {
            bool bell = catalog.Unit(session.State.ChampionId).Weapon.Signature == SignatureWeapon.Bell;
            bool stamp = catalog.Unit(session.State.ChampionId).Weapon.Signature == SignatureWeapon.Stamp;
            foreach (RosterEntry entry in session.ActiveRoster)
            {
                SignatureWeapon family = catalog.Weapon(entry.WeaponId).Signature;
                bell |= family == SignatureWeapon.Bell; stamp |= family == SignatureWeapon.Stamp;
            }
            bool active = id == "signature-harvest" ? bell && session.StackCount("harvest-receipt") > 0 : stamp && (session.StackCount("reflective-ink") > 0 || session.StackCount("office-lens") > 0);
            bool tea = false, hammer = false, music = false, bow = false;
            foreach (RosterEntry entry in session.ActiveRoster)
            {
                WeaponDefinition weapon = catalog.Weapon(entry.WeaponId);
                tea |= weapon.Family == ExpandedWeapon.Tea; hammer |= weapon.Family == ExpandedWeapon.Hammer;
                music |= weapon.Family == ExpandedWeapon.MusicBox; bow |= weapon.Motion == WeaponMotion.Bow;
            }
            WeaponDefinition main = catalog.Unit(session.State.ChampionId).Weapon;
            tea |= main.Family == ExpandedWeapon.Tea; hammer |= main.Family == ExpandedWeapon.Hammer;
            music |= main.Family == ExpandedWeapon.MusicBox; bow |= main.Motion == WeaponMotion.Bow;
            if (id == "signature-tea") active = tea && (hammer || session.StackCount("tea-coaster") > 0);
            if (id == "signature-chorus") active = music && session.StackCount("scratch-record") > 0;
            if (id == "signature-treasure") active = bow && session.StackCount("treasure-ribbon") > 0;
            if (id == "signature-toys") active = catalog.SignatureRevision > 0 && session.StackCount("patch-spring") > 0;
            return active;
        }
    }
}
