using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Friends remain in the simulation while fallen; only active friends act or receive damage.</summary>
    public sealed class MortalityRuntime
    {
        private sealed class FriendState
        {
            public UnitState Unit;
            public bool Fallen;
            public float RiseAt, Shield, ShieldCapacity, ShieldUntil, Reduction, ReductionUntil;
        }

        private readonly List<FriendState> friends = new List<FriendState>(MortalityTuning.TeamCapacity);
        private readonly Dictionary<int, FriendState> byId = new Dictionary<int, FriendState>(MortalityTuning.TeamCapacity);
        public event Action<UnitState> ShieldBroken;
        public event Action<UnitState,float> ShieldAbsorbed;
        public event Action<FriendMortalityEvent> Fell;
        public event Action<FriendMortalityEvent> Rose;
        public event Action<FriendWaveLostEvent> WaveLost;
        public float Elapsed { get; private set; }
        public bool Lost { get; private set; }
        public int LivingCount { get; private set; }
        public int FriendCount => friends.Count;
        public UnitState FriendAt(int index) => friends[index].Unit;

        public static bool IsFriend(UnitState unit) => unit != null && unit.Faction != Faction.Enemy;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static float MaxHealth(UnitState unit)
        {
            float health = unit != null && unit.Definition != null ? unit.Definition.Health : MortalityTuning.DefaultMaxHealth;
            return Finite(health) ? Mathf.Max(MortalityTuning.MinimumMaxHealth, health) : MortalityTuning.DefaultMaxHealth;
        }

        /// <summary>Each wave begins with the entire deployed team standing at full health.</summary>
        public void ResetWave(IEnumerable<UnitState> units)
        {
            friends.Clear(); byId.Clear(); Elapsed = 0; Lost = false; LivingCount = 0;
            if (units == null) return;
            foreach (UnitState unit in units) RegisterFriend(unit);
        }

        public void RegisterFriend(UnitState unit)
        {
            if (!IsFriend(unit) || byId.ContainsKey(unit.Id)) return;
            var state = new FriendState { Unit = unit };
            friends.Add(state); byId.Add(unit.Id, state);
            unit.BrokenArmorPoints = 0;
            unit.Health = MaxHealth(unit); unit.Alive = true; unit.PendingAttack = false;
            LivingCount++;
        }

        /// <summary>One point per armour-breaking hit, retained through falls and revivals until wave reset.</summary>
        public void BreakArmor(UnitState unit, float points = FriendDefenseTuning.ArmorBreakPerHit)
        {
            if (unit != null && unit.Alive && byId.ContainsKey(unit.Id) && Finite(points) && points > 0)
                unit.BrokenArmorPoints += points;
        }
        public static float ArmorPoints(UnitState unit, float extraPoints = 0)
            => unit == null ? 0 : Mathf.Max(0, unit.Definition.FriendArmorPoints + extraPoints - unit.BrokenArmorPoints);

        public bool IsFallen(UnitState unit) => unit != null && byId.TryGetValue(unit.Id, out FriendState state) && state.Fallen;
        public float RiseAt(UnitState unit) => unit != null && byId.TryGetValue(unit.Id, out FriendState state) && state.Fallen ? state.RiseAt : 0;

        /// <summary>Call after the combat damage pipeline has reduced a friend's health to zero.</summary>
        public bool OnLethal(UnitState unit, int sourceId = 0)
        {
            if (!IsFriend(unit) || !byId.TryGetValue(unit.Id, out FriendState state)) return false;
            if (state.Fallen || Lost) return true;
            if (unit.Health > 0) return false;
            state.Fallen = true; state.RiseAt = Elapsed + MortalityTuning.RiseSeconds;
            state.Shield = state.Reduction = 0;
            unit.Health = 0; unit.Alive = false; unit.PendingAttack = false;
            unit.AttackTargetId = unit.TacticalTargetId = 0; unit.Velocity = Vector2.zero;
            LivingCount--;
            Fell?.Invoke(new FriendMortalityEvent(unit, sourceId, Elapsed, MaxHealth(unit), state.RiseAt));
            if (LivingCount == 0)
            {
                Lost = true;
                WaveLost?.Invoke(new FriendWaveLostEvent(Elapsed, friends.Count, unit.Id));
            }
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (Lost || !Finite(deltaTime) || deltaTime <= 0) return;
            Elapsed += deltaTime;
            for (int i = 0; i < friends.Count; i++)
            {
                FriendState state = friends[i];
                if (state.Fallen && Elapsed >= state.RiseAt) Rise(state, MortalityTuning.RiseHealthFraction);
            }
        }

        private void Rise(FriendState state, float healthFraction)
        {
            if (!state.Fallen || Lost) return;
            UnitState unit = state.Unit;
            state.Fallen = false; state.RiseAt = 0;
            unit.Health = MaxHealth(unit) * Mathf.Clamp01(healthFraction); unit.Alive = true;
            unit.PendingAttack = false; unit.SleepUntil = unit.FrozenUntil = unit.BurnUntil = unit.ChillUntil = 0;
            LivingCount++;
            Rose?.Invoke(new FriendMortalityEvent(unit, 0, Elapsed, MaxHealth(unit), 0));
        }

        /// <summary>Protection intercepts incoming health damage before the ordinary combat hit bookkeeping.</summary>
        public float ModifyIncomingDamage(UnitState unit, float damage)
        {
            if (!Finite(damage) || damage <= 0 || Lost || unit == null || !byId.TryGetValue(unit.Id, out FriendState state) || state.Fallen) return 0;
            if (state.ReductionUntil > Elapsed) damage *= 1 - state.Reduction;
            if (state.ShieldUntil > Elapsed)
            {
                float absorbed = Mathf.Min(state.Shield, damage);
                state.Shield -= absorbed; damage -= absorbed;
                if(absorbed > 0)ShieldAbsorbed?.Invoke(unit,absorbed);
                if(absorbed > 0 && state.Shield <= 0)ShieldBroken?.Invoke(unit);
            }
            return damage;
        }

        public float ShieldCapacity(UnitState unit) => unit!=null && byId.TryGetValue(unit.Id,out FriendState state) ? state.ShieldCapacity : 0;
        public float ShieldAmount(UnitState unit) => unit!=null && byId.TryGetValue(unit.Id,out FriendState state) && state.ShieldUntil>Elapsed ? state.Shield : 0;

        public float ApplyDamage(UnitState unit, float damage, int sourceId = 0)
        {
            float applied = ModifyIncomingDamage(unit, damage);
            if (applied <= 0) return 0;
            applied = Mathf.Min(unit.Health, applied);
            unit.Health = Mathf.Max(0, unit.Health - applied); unit.LastHit = Elapsed;
            if (unit.Health <= 0) OnLethal(unit, sourceId);
            return applied;
        }

        public void DamageArea(Vector2 position, float radius, float damage, int sourceId = 0)
        {
            if (!Finite(radius) || radius < 0) return;
            float square = radius * radius;
            for (int i = 0; i < friends.Count; i++)
            {
                UnitState unit = friends[i].Unit;
                if (unit.Alive && (unit.Position - position).sqrMagnitude <= square) ApplyDamage(unit, damage, sourceId);
            }
        }

        public float Heal(UnitState unit, float amount)
        {
            if (Lost || !Finite(amount) || amount <= 0 || unit == null || !byId.TryGetValue(unit.Id, out FriendState state) || state.Fallen) return 0;
            float before = unit.Health;
            unit.Health = Mathf.Min(MaxHealth(unit), unit.Health + amount);
            return unit.Health - before;
        }

        public void Shield(UnitState unit, float healthFraction, float duration)
        {
            if (unit == null || !Finite(healthFraction) || !Finite(duration) || !byId.TryGetValue(unit.Id, out FriendState state) || state.Fallen) return;
            state.Shield = Mathf.Max(state.ShieldUntil > Elapsed ? state.Shield : 0, MaxHealth(unit) * Mathf.Clamp01(healthFraction));
            state.ShieldCapacity=state.Shield;
            state.ShieldUntil = Mathf.Max(state.ShieldUntil, Elapsed + Mathf.Max(0, duration));
        }

        public void ReduceDamage(UnitState unit, float fraction, float duration)
        {
            if (unit == null || !Finite(fraction) || !Finite(duration) || !byId.TryGetValue(unit.Id, out FriendState state) || state.Fallen) return;
            state.Reduction = Mathf.Max(state.ReductionUntil > Elapsed ? state.Reduction : 0, Mathf.Clamp(fraction, 0, MortalityTuning.MaximumDamageReduction));
            state.ReductionUntil = Mathf.Max(state.ReductionUntil, Elapsed + Mathf.Max(0, duration));
        }

        public bool ReviveSooner(UnitState unit, float seconds)
        {
            if (Lost || unit == null || !Finite(seconds) || !byId.TryGetValue(unit.Id, out FriendState state) || !state.Fallen) return false;
            state.RiseAt = Mathf.Max(Elapsed, state.RiseAt - Mathf.Max(0, seconds));
            if (state.RiseAt <= Elapsed) Rise(state, MortalityTuning.RiseHealthFraction);
            return true;
        }

        public bool ReviveNow(UnitState unit, float healthFraction = MortalityTuning.RiseHealthFraction)
        {
            if (Lost || unit == null || !Finite(healthFraction) || healthFraction <= 0 || !byId.TryGetValue(unit.Id, out FriendState state) || !state.Fallen) return false;
            Rise(state, healthFraction); return true;
        }

        public void Cleanse(UnitState unit)
        {
            if (!IsFriend(unit)) return;
            unit.SleepUntil = unit.FrozenUntil = unit.BurnUntil = unit.ChillUntil = unit.AnchorUntil = 0;
        }
        public void RemoveProtection(UnitState unit)
        {
            if(unit!=null && byId.TryGetValue(unit.Id,out FriendState state))
                state.Shield = state.ShieldUntil = state.Reduction = state.ReductionUntil = 0;
        }
        public void DefeatAll(int sourceId=0)
        {
            if(Lost)return;Lost=true;LivingCount=0;
            for(int i=0;i<friends.Count;i++)
            {
                FriendState state=friends[i];if(state.Fallen)continue;
                state.Fallen=true;state.Unit.Health=0;state.Unit.Alive=false;state.Unit.PendingAttack=false;
                Fell?.Invoke(new FriendMortalityEvent(state.Unit,sourceId,Elapsed,MaxHealth(state.Unit),0));
            }
            WaveLost?.Invoke(new FriendWaveLostEvent(Elapsed,friends.Count,sourceId));
        }

        /// <summary>Hunters choose from the bounded friend roster, never scan the enemy population.</summary>
        public UnitState WeakestFriend()
        {
            UnitState best = null; float share = float.PositiveInfinity;
            for (int i = 0; i < friends.Count; i++)
            {
                UnitState unit = friends[i].Unit;
                if (!unit.Alive) continue;
                float current = unit.Health / MaxHealth(unit);
                if (current < share || current == share && (best == null || unit.Id < best.Id)) { best = unit; share = current; }
            }
            return best;
        }
    }
}
