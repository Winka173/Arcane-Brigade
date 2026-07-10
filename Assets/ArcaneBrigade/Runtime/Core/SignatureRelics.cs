using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private float nextHarvestBeat;
        public int HarvestBeats { get; private set; }

        private void ProcessSignatureRelics()
        {
            if (catalog.HarvestStacks <= 0 || Elapsed < nextHarvestBeat || !Champion.Alive) return;
            int count = events.Count;
            for (int index = 0; index < count; index++)
            {
                CombatEvent trigger = events[index];
                if (trigger.Kind != CombatEventKind.Pickup || trigger.Loot != LootKind.Coin || trigger.Cause.Depth != 0) continue;
                UnitState bell = null; float best = 36;
                foreach (UnitState source in StableUnits)
                {
                    if (!source.Alive || source.Faction == Faction.Enemy || source.Definition.Weapon.Signature != SignatureWeapon.Bell) continue;
                    float distance = (source.Position - Champion.Position).sqrMagnitude;
                    if (distance >= best) continue;
                    bell = source; best = distance;
                }
                if (bell == null) return;
                nextHarvestBeat = Elapsed + 0.35f;
                CombatCause cause = new CombatCause(trigger.Cause.RootId, bell.Id, "harvest-receipt", 1);
                AdvanceSignatureBeat(bell, bell.Definition.Weapon.Damage, cause, Mathf.Min(2, catalog.HarvestStacks));
                HarvestBeats++; RecordActivation(cause);
                events.Add(new CombatEvent(CombatEventKind.Proc, bell.Position, bell.Faction, cause: cause));
                return;
            }
        }
    }
}
