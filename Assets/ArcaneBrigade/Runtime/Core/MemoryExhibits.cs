using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Balance round 5 (Docs/Balance_Pass_2026-10-19.md): Archive Curator ("Wise") absorbs bought friends as memories
    /// instead of bodies, and was left fighting nearly alone (bot win 1/6, dead at the wave-5 boss). Its last two distinct
    /// memories now step out as paper exhibits each battle: one-star bodies with their own weapon, after the roster (so every
    /// roster-indexed reader stays aligned). They are not roster entries: no bonds, no merges, nothing saved.</summary>
    public sealed partial class RunSession
    {
        public const int MemoryExhibitCount = 2; // 3 gave 6/6 (above the band)
        public List<string> MemoryExhibitIds()
        {
            var ids = new List<string>();
            if (OctoberHero != "Wise" || State.AbsorbedSkills == null) return ids;
            for (int i = State.AbsorbedSkills.Count - 1; i >= 0 && ids.Count < MemoryExhibitCount; i--)
            {
                string id = State.AbsorbedSkills[i];
                if (!ids.Contains(id) && catalog.Unit(id) != null) ids.Add(id);
            }
            return ids;
        }
        private void AddMemoryExhibits()
        {
            List<string> ids = MemoryExhibitIds();
            if (ids.Count == 0) return;
            var bodies = new List<UnitDefinition>(CombatCatalog.StartingCompanions);
            foreach (string id in ids)
            {
                UnitDefinition unit = catalog.Unit(id);
                WeaponDefinition weapon = unit.Weapon != null ? unit.Weapon : catalog.Weapons[0];
                bodies.Add(BuildUnit(unit, weapon, 1, true));
            }
            CombatCatalog.StartingCompanions = bodies.ToArray();
        }
    }
}
