using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // Shared by the Waiting Room and the Post Office (Docs/Map_Mechanics_2026-10-25.md sections 1 and 2): a prop with a health
    // bar that our side's shots and blades break (the called door, the giant parcel), and the small helpers both maps' rules use
    // (the hero hit, an ordinary enemy's health, a stun, spilled gold).

    public sealed partial class MapEnvironmentState
    {
        /// <summary>A prop with health that our shots and blades break (requirement 25/10/2026: the called door, the giant parcel):
        /// where its body is, how big, its health, and its last blow (for the presentation's hit ink and number).</summary>
        public sealed class StruckProp
        {
            public Vector2 Position, HitFrom;
            public float Radius = .9f, Health, MaxHealth, HitAt = -100, LastDamage;
            public int Hits;
            /// <summary>The attacks that already struck it (one blow per attack, however many of its shots cross it).</summary>
            public readonly HashSet<int> Roots = new HashSet<int>();
            public float Share => MaxHealth > 0 ? Mathf.Clamp01(Health / MaxHealth) : 0;
            public void Reset(Vector2 at, float radius, float health)
            {
                Position = at; HitFrom = at; Radius = radius; Health = MaxHealth = Mathf.Max(1, health);
                HitAt = -100; LastDamage = 0; Hits = 0; Roots.Clear();
            }
        }
    }

    public static partial class MapEnvironmentRules
    {
        /// <summary>A blade reaches a prop a little past its weapon's range (its swing sweeps the prop's face).</summary>
        public const float RoomBladeSlack = .35f;
    }

    public sealed partial class CombatSimulation
    {
        /// <summary>The hero hit (spec 25/10/2026: "hero hit" means the hero's weapon damage).</summary>
        private float RoomHeroHit => Champion != null && Champion.Definition.Weapon != null ? Champion.Definition.Weapon.Damage : 10;

        /// <summary>An ordinary enemy's health this wave: the mean of the wave's non-boss kinds.</summary>
        private float RoomOrdinaryHealth()
        {
            float sum = 0; int n = 0;
            if (catalog.EnemyPool != null)
                foreach (UnitDefinition d in catalog.EnemyPool)
                    if (d != null && !d.IsBoss) { sum += d.Health; n++; }
            if (n > 0) return sum / n;
            return catalog.MeleeEnemy != null ? catalog.MeleeEnemy.Health : 30;
        }

        /// <summary>Our side's blows on a prop this step (requirement 25/10/2026: breakable by shots or blades from any range):
        /// every shot crossing it strikes it once (and a shot that does not pierce stops there); a blade swung within its reach
        /// strikes it; and each attack the hero lets fly while the prop is inside its weapon's range strikes it for the hero hit,
        /// whatever the attack was aimed at. One blow per attack. Returns the damage dealt.</summary>
        private float RoomStrikeProp(MapEnvironmentState.StruckProp prop, int victimId)
        {
            if (prop.Health <= 0) return 0;
            float dealt = 0, boost = 1 + MapBoostValue(MapStat.Damage); // the map's damage boosts count on props too
            for (int i = 0; i < events.Count; i++)
            {
                CombatEvent e = events[i];
                if (e.Kind != CombatEventKind.Fire || e.Faction == Faction.Enemy || e.Subject == null || e.Weapon == null || e.Cause.RootId == 0) continue;
                UnitState by = e.Subject; bool hero = by == Champion;
                float reach;
                if (e.Weapon.Kind == WeaponKind.Melee) reach = e.Weapon.Range + prop.Radius + MapEnvironmentRules.RoomBladeSlack;
                else if (hero && e.Weapon.Kind == WeaponKind.Projectile) reach = e.Weapon.Range + prop.Radius;
                else continue; // a friend's shot strikes only by crossing it (below)
                if ((prop.Position - by.Position).sqrMagnitude > reach * reach || !prop.Roots.Add(e.Cause.RootId)) continue;
                float hit = (hero ? RoomHeroHit : e.Weapon.Damage) * (e.Critical ? 1.5f : 1) * boost;
                RoomPropHit(prop, hit, by.Position); dealt += hit;
            }
            for (int i = 0; i < projectiles.Count; i++)
            {
                ProjectileState p = projectiles[i];
                if (p.Faction == Faction.Enemy || p.Damage <= 0 || p.Victims.Contains(victimId)) continue;
                Vector2 from = p.PreviousPosition;
                if ((p.Position - from).sqrMagnitude > 9) from = p.Position; // a teleported or orbiting shot: only where it is now
                if (SegmentCircle(from, p.Position, prop.Position, prop.Radius + p.Radius) < 0) continue;
                p.Victims.Add(victimId);
                bool pierces = p.Arsenal != ArsenalWeapon.None || p.Returning || p.ExtraPierce > 0 || p.Weapon != null && p.Weapon.ProjectilePierce > 0;
                if (!pierces) { p.Remaining = 0; p.TravelLeft = 0; } // it stops in the prop
                if (p.Cause.RootId != 0 && !prop.Roots.Add(p.Cause.RootId)) continue; // that attack already struck it
                float hit = p.Damage * boost;
                RoomPropHit(prop, hit, from); dealt += hit;
            }
            if (prop.Roots.Count > 256) prop.Roots.Clear();
            return dealt;
        }
        private void RoomPropHit(MapEnvironmentState.StruckProp prop, float hit, Vector2 from)
        {
            prop.Health = Mathf.Max(0, prop.Health - hit);
            prop.HitAt = Elapsed; prop.HitFrom = from; prop.LastDamage = hit; prop.Hits++;
        }

        /// <summary>A map's stun: the enemy neither moves nor attacks for a while (bosses for their own, shorter time).</summary>
        private void RoomStun(UnitState u, float seconds, float bossSeconds)
        {
            float s = u.Definition.IsBoss ? bossSeconds : seconds;
            u.SleepUntil = Mathf.Max(u.SleepUntil, Elapsed + s);
            u.NextAttack = Mathf.Max(u.NextAttack, Elapsed + s); u.NextContact = Mathf.Max(u.NextContact, Elapsed + s);
            if (!u.Definition.IsBoss) u.PendingAttack = false; // a blow being wound up is dropped
        }

        /// <summary>Spills this much gold (in gold, whatever a coin is worth this run) as a few coins round a spot.</summary>
        private void RoomSpillGold(Vector2 at, int gold, int pieces)
        {
            if (gold <= 0) return;
            pieces = Mathf.Max(1, pieces);
            float perCoin = Mathf.Max(.01f, KitCoinScale * Mathf.Max(.01f, catalog.CoinValueScale));
            int coins = Mathf.Max(pieces, Mathf.RoundToInt(gold / perCoin));
            for (int i = 0; i < pieces; i++)
            {
                float a = i * 2.399963f;
                DropLoot(LootKind.Coin, at + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * .3f, coins / pieces + (i == 0 ? coins % pieces : 0));
            }
        }

        /// <summary>A copy of the units to walk while blows may add or remove some (a split, a summon).</summary>
        private readonly List<UnitState> roomUnits = new List<UnitState>();
        private List<UnitState> RoomUnitsSnapshot() { roomUnits.Clear(); roomUnits.AddRange(units); return roomUnits; }
    }
}
