using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        /// <summary>Lucid rites performed this run (at most RunSession.LucidMax).</summary>
        public int LucidRites;
        /// <summary>Drinks owned when this shop opened (Red Scale: each drink bought here adds 10% monsters to the next fight).</summary>
        public int DrinksBeforeShop;
    }

    /// <summary>The fourth form (owner, 22/10/2026): a 3-star friend can wake LUCID at the dream shop's altar from wave 12,
    /// twice a run, for eggs and gold. A lucid friend hits harder, its weapon grows (more reach for blades, one more pierce
    /// for shots) and it counts one more member in its main bond. Drawn in code (LucidAura); no new painting per friend.</summary>
    public sealed partial class RunSession
    {
        public const int LucidWave = 12, LucidMax = 2;
        public const float LucidDamage = 1.3f, LucidBody = 1.1f, LucidReach = 1.2f;
        public int LucidEggCost => Mathf.CeilToInt((4 + 2 * State.LucidRites) * SpecialStagePriceFactor);
        public int LucidGoldCost => ScaledGold(60);

        /// <summary>Roster indexes of the deployed 3-star friends that are not lucid yet.</summary>
        public List<int> LucidCandidates()
        {
            var result = new List<int>();
            List<RosterEntry> deployed = ActiveRoster;
            for (int i = 0; i < State.Roster.Count; i++)
            {
                RosterEntry entry = State.Roster[i];
                if (entry.Tier >= 3 && !entry.Lucid && deployed.Contains(entry) && catalog.Unit(entry.UnitId) != null) result.Add(i);
            }
            return result;
        }
        /// <summary>The altar stands in the dream shop: wave 12 on, rites left, and someone to wake.</summary>
        public bool LucidAltarOpen => State.Phase == SessionPhase.Shop && State.Wave >= LucidWave && State.LucidRites < LucidMax && !SecretRoomOpen && LucidCandidates().Count > 0;
        public bool CanLucid(int rosterIndex) => LucidAltarOpen && LucidCandidates().Contains(rosterIndex) && State.Eggs >= LucidEggCost && State.Gold >= LucidGoldCost;

        public bool Lucid(int rosterIndex)
        {
            if (!CanLucid(rosterIndex)) { Notice = "The altar needs " + LucidEggCost + " eggs and " + LucidGoldCost + " gold."; return false; }
            State.Eggs -= LucidEggCost; State.Gold -= LucidGoldCost;
            State.Roster[rosterIndex].Lucid = true;
            State.LucidRites++;
            RecountBonds();
            Notice = RunUICopy.UnitName(catalog.Unit(State.Roster[rosterIndex].UnitId)) + " wakes LUCID.";
            return true;
        }

        /// <summary>A lucid friend's body as it deploys: more damage, a bigger body, a grown weapon.</summary>
        private static void ApplyLucid(UnitDefinition unit)
        {
            unit.Lucid = true;
            unit.BodyHeight *= LucidBody;
            if (unit.Weapon == null) return;
            unit.Weapon.Damage *= LucidDamage;
            if (unit.Weapon.Kind == WeaponKind.Melee) { unit.Weapon.Range *= LucidReach; unit.Weapon.SwingAngle *= 1.25f; }
            else { unit.Weapon.ProjectilePierce += 1; unit.Weapon.ProjectileRadius *= 1.2f; }
        }
    }
}
