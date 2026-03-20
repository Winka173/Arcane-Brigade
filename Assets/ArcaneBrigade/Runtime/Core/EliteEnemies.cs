using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Gatekeeper's dare (a shop visitor): over the first half of the dared wave, its elites walk in from the edge,
    /// each a tougher, larger, gilded copy of a regular enemy; each drops a chest when it falls.</summary>
    public sealed partial class CombatSimulation
    {
        private int elitesSpawned;
        public int ElitesSpawned => elitesSpawned;
        private void ProcessElites()
        {
            if (catalog.EliteCount <= elitesSpawned || Boss != null) return;
            float due = catalog.WaveDuration * (.12f + .38f * elitesSpawned / Mathf.Max(1, catalog.EliteCount - 1));
            if (Elapsed < due) return;
            UnitDefinition kind = null;
            if (catalog.EnemyPool != null) foreach (UnitDefinition d in catalog.EnemyPool) if (d != null && !d.IsBoss && (kind == null || d.Health > kind.Health)) kind = d;
            kind ??= catalog.MeleeEnemy;
            if (kind == null) return;
            elitesSpawned++;
            UnitDefinition elite = Object.Instantiate(kind);
            elite.Health = kind.Health * 4.5f; elite.Radius = kind.Radius * 1.2f; elite.BodyScale = kind.BodyScale * 1.3f;
            elite.Tint = Color.Lerp(kind.Tint, new Color(1f, .82f, .45f), .45f); elite.KnockbackResistance = Mathf.Max(kind.KnockbackResistance, .6f);
            elite.DisplayName = "Gilded " + kind.DisplayName;
            float angle = (float)spawnRandom.NextDouble() * Mathf.PI * 2;
            Vector2 at = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 8.5f, .6f);
            UnitState unit = Spawn(elite, Faction.Enemy, at); unit.Elite = true;
        }
        private void EliteFell(UnitState dead)
        {
            if (dead != null && dead.Elite) DropLoot(LootKind.Chest, dead.Position, 1);
        }
    }
}
