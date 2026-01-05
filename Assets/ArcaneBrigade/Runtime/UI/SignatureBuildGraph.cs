namespace ArcaneBrigade
{
    public static class SignatureBuildGraph
    {
        public static string Status(string id, RunSession session, RunCatalog catalog)
        {
            bool bell = catalog.Unit(session.State.ChampionId).Weapon.Signature == SignatureWeapon.Bell;
            bool stamp = catalog.Unit(session.State.ChampionId).Weapon.Signature == SignatureWeapon.Stamp;
            foreach (RosterEntry entry in session.State.Roster)
            {
                SignatureWeapon family = catalog.Weapon(entry.WeaponId).Signature;
                bell |= family == SignatureWeapon.Bell; stamp |= family == SignatureWeapon.Stamp;
            }
            bool active = id == "signature-harvest" ? bell && session.StackCount("harvest-receipt") > 0 : stamp && (session.StackCount("reflective-ink") > 0 || session.StackCount("office-lens") > 0);
            return active ? "NHÁNH CƠ BẢN ACTIVE · không cần đủ set" : "TIỀM NĂNG · thiếu weapon hoặc bộ nối";
        }
    }
}
