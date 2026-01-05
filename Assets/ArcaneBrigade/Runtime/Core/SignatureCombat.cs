using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private sealed class StampState
        {
            public UnitState Source;
            public UnitState Target;
            public int Count;
            public float Expires;
        }

        private sealed class SignaturePulse
        {
            public UnitState Source;
            public Vector2 Position;
            public Vector2 Direction;
            public float Damage;
            public float Radius;
            public float At;
            public CombatCause Cause;
        }

        private readonly Dictionary<long, StampState> stamps = new Dictionary<long, StampState>();
        private readonly List<long> expiredStamps = new List<long>();
        private readonly List<SignaturePulse> signaturePulses = new List<SignaturePulse>();
        public int PendingSignaturePulses => signaturePulses.Count;
        public int ActiveStampPairs => stamps.Count;
        public int BellWaves { get; private set; }
        public int UmbrellaOpenings { get; private set; }
        public int StampBursts { get; private set; }

        private void ClearSignatures()
        {
            stamps.Clear(); signaturePulses.Clear(); expiredStamps.Clear();
            BellWaves = UmbrellaOpenings = StampBursts = 0;
            foreach (UnitState unit in units) { unit.AttackBeat = unit.VisibleMarks = 0; unit.MarkVisibleUntil = 0; }
        }

        private void AdvanceSignatureBeat(UnitState unit, float damage, CombatCause cause)
        {
            WeaponDefinition weapon = unit.Definition.Weapon;
            if (unit.Faction == Faction.Enemy || weapon.Signature == SignatureWeapon.None || weapon.Signature == SignatureWeapon.Stamp) return;
            unit.AttackBeat = (unit.AttackBeat + 1) % Mathf.Max(2, weapon.SignatureBeats);
            if (unit.AttackBeat != 0) return;
            float radius = Mathf.Max(0.1f, weapon.SignatureRadius);
            if (weapon.Signature == SignatureWeapon.Umbrella && unit.Definition.CombatTier >= 2) radius *= 1.25f;
            signaturePulses.Add(new SignaturePulse { Source = unit, Position = unit.Position, Direction = unit.AttackDirection, Damage = damage * Mathf.Max(0, weapon.SignatureScale), Radius = radius, At = Elapsed + Mathf.Max(0.05f, weapon.SignatureDelay), Cause = cause.Child(weapon.Signature == SignatureWeapon.Bell ? "bell-wave" : "umbrella-open") });
            if (weapon.Signature == SignatureWeapon.Bell && unit.Definition.CombatTier >= 2)
                signaturePulses.Add(new SignaturePulse { Source = unit, Position = unit.Position, Direction = unit.AttackDirection, Damage = damage * Mathf.Max(0, weapon.SignatureScale) * 0.35f, Radius = radius, At = Elapsed + Mathf.Max(0.05f, weapon.SignatureDelay) + 0.3f, Cause = cause.Child("bell-echo") });
        }

        private void AddStamp(UnitState source, UnitState target, float damage, CombatCause cause)
        {
            if (!target.Alive || target.Faction != Faction.Enemy) return;
            WeaponDefinition weapon = source.Definition.Weapon;
            long key = ((long)source.Id << 32) | (uint)target.Id;
            if (!stamps.TryGetValue(key, out StampState mark))
            {
                mark = new StampState { Source = source, Target = target }; stamps.Add(key, mark);
            }
            if (mark.Expires <= Elapsed) mark.Count = 0;
            mark.Count++; mark.Expires = Elapsed + Mathf.Max(0.1f, weapon.MarkLifetime);
            target.VisibleMarks = mark.Count; target.MarkVisibleUntil = mark.Expires;
            events.Add(new CombatEvent(CombatEventKind.SignatureMark, target.Position, source.Faction, mark.Count, weapon: weapon, subjectId: target.Id, cause: cause.Child("stamp-mark")));
            if (mark.Count < Mathf.Max(2, weapon.SignatureBeats)) return;
            stamps.Remove(key); target.VisibleMarks = 0;
            StampBursts++;
            events.Add(new CombatEvent(CombatEventKind.SignatureBurst, target.Position, source.Faction, weapon.SignatureRadius, weapon: weapon, cause: cause.Child("stamp-burst")));
            ApplyDamage(target, damage * Mathf.Max(0, weapon.SignatureScale), source.Faction, "Dream stamp burst", weapon: weapon, cause: cause.Child("stamp-burst"));
            if (source.Definition.CombatTier < 2) return;
            UnitState neighbor = null; float best = weapon.SignatureRadius * weapon.SignatureRadius;
            foreach (UnitState candidate in units)
            {
                if (candidate == target || !candidate.Alive || candidate.Faction != Faction.Enemy) continue;
                float distance = (candidate.Position - target.Position).sqrMagnitude;
                if (distance >= best) continue;
                neighbor = candidate; best = distance;
            }
            if (neighbor == null) return;
            long neighborKey = ((long)source.Id << 32) | (uint)neighbor.Id;
            if (!stamps.TryGetValue(neighborKey, out StampState transferred))
            {
                transferred = new StampState { Source = source, Target = neighbor }; stamps.Add(neighborKey, transferred);
            }
            if (transferred.Expires <= Elapsed) transferred.Count = 0;
            transferred.Count = Mathf.Min(Mathf.Max(2, weapon.SignatureBeats) - 1, transferred.Count + 1);
            transferred.Expires = Elapsed + Mathf.Max(0.1f, weapon.MarkLifetime);
            neighbor.VisibleMarks = transferred.Count; neighbor.MarkVisibleUntil = transferred.Expires;
            events.Add(new CombatEvent(CombatEventKind.SignatureMark, neighbor.Position, source.Faction, transferred.Count, weapon: weapon, subjectId: neighbor.Id, cause: cause.Child("stamp-carbon")));
        }

        private void ProcessSignatures()
        {
            expiredStamps.Clear();
            foreach (KeyValuePair<long, StampState> pair in stamps)
                if (!pair.Value.Target.Alive || !pair.Value.Source.Alive || pair.Value.Expires <= Elapsed) expiredStamps.Add(pair.Key);
            foreach (long key in expiredStamps) stamps.Remove(key);
            foreach (UnitState unit in units)
            {
                unit.VisibleMarks = 0; unit.MarkVisibleUntil = 0;
            }
            foreach (StampState mark in stamps.Values)
                if (mark.Count > mark.Target.VisibleMarks) { mark.Target.VisibleMarks = mark.Count; mark.Target.MarkVisibleUntil = mark.Expires; }
            for (int index = signaturePulses.Count - 1; index >= 0; index--)
            {
                SignaturePulse pulse = signaturePulses[index];
                if (pulse.At > Elapsed) continue;
                signaturePulses.RemoveAt(index);
                if (!pulse.Source.Alive) continue;
                WeaponDefinition weapon = pulse.Source.Definition.Weapon;
                if (weapon.Signature == SignatureWeapon.Bell) BellWaves++; else UmbrellaOpenings++;
                events.Add(new CombatEvent(CombatEventKind.SignaturePulse, pulse.Position, pulse.Source.Faction, pulse.Radius, pulse.Direction, weapon, cause: pulse.Cause));
                foreach (UnitState target in units)
                {
                    Vector2 delta = target.Position - pulse.Position;
                    if (!target.Alive || target.Faction != Faction.Enemy || delta.sqrMagnitude > pulse.Radius * pulse.Radius) continue;
                    if (weapon.Signature == SignatureWeapon.Umbrella && Vector2.Dot(delta.normalized, pulse.Direction) < 0) continue;
                    ApplyDamage(target, pulse.Damage, pulse.Source.Faction, weapon.Id + " payoff", pulse.Direction, weapon, cause: pulse.Cause);
                    if (target.Alive && weapon.Signature == SignatureWeapon.Umbrella)
                    {
                        float resistance = target.Definition.IsBoss ? 0.2f : 1;
                        target.Position = ClampPosition(target.Position + delta.normalized * Mathf.Clamp(weapon.PushDistance, 0, 0.8f) * resistance, target.Definition.Radius);
                        spatial.Move(target);
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void SignatureBeatFixture(UnitState source) => AdvanceSignatureBeat(source, source.Definition.Weapon.Damage, NewCause(source.Id));
        public void StampFixture(UnitState source, UnitState target) => AddStamp(source, target, source.Definition.Weapon.Damage, NewCause(source.Id));
#endif
    }
}
