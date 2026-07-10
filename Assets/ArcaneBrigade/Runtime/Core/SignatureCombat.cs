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
            foreach (UnitState unit in StableUnits) { unit.AttackBeat = unit.VisibleMarks = 0; unit.MarkVisibleUntil = unit.ExposedUntil = 0; }
        }

        private void AdvanceSignatureBeat(UnitState unit, float damage, CombatCause cause, int charge = 1)
        {
            WeaponDefinition weapon = unit.Definition.Weapon;
            if (unit.Faction == Faction.Enemy || weapon.Signature == SignatureWeapon.None || weapon.Signature == SignatureWeapon.Stamp) return;
            // every attack carries a lighter volley (was: every Nth attack a full one); same damage over time
            int beats = Mathf.Max(2, weapon.SignatureBeats);
            float share = Mathf.Clamp(charge, 1, beats) / (float)beats;
            unit.AttackBeat = 0;
            damage *= share;
            float radius = Mathf.Max(0.1f, weapon.SignatureRadius);
            if (weapon.Signature == SignatureWeapon.Umbrella && unit.Definition.CombatTier >= 2) radius *= 1.25f;
            signaturePulses.Add(new SignaturePulse { Source = unit, Position = unit.Position, Direction = unit.AttackDirection, Damage = damage * Mathf.Max(0, weapon.SignatureScale), Radius = radius, At = Elapsed + Mathf.Max(0.05f, weapon.SignatureDelay), Cause = new CombatCause(cause.RootId, unit.Id, weapon.Signature == SignatureWeapon.Bell ? "bell-wave" : "umbrella-open", cause.Depth + 1) });
            if (weapon.Signature == SignatureWeapon.Bell && unit.Definition.CombatTier >= 2)
                signaturePulses.Add(new SignaturePulse { Source = unit, Position = unit.Position, Direction = unit.AttackDirection, Damage = damage * Mathf.Max(0, weapon.SignatureScale) * 0.35f, Radius = radius, At = Elapsed + Mathf.Max(0.05f, weapon.SignatureDelay) + 0.3f, Cause = new CombatCause(cause.RootId, unit.Id, "bell-echo", cause.Depth + 1) });
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
            mark.Count += catalog.ReflectiveInkStacks > 0 ? 2 : 1;
            mark.Expires = Elapsed + Mathf.Max(0.1f, weapon.MarkLifetime * (catalog.ReflectiveInkStacks > 0 ? 0.65f : 1));
            target.VisibleMarks = mark.Count; target.MarkVisibleUntil = mark.Expires;
            events.Add(new CombatEvent(CombatEventKind.SignatureMark, target.Position, source.Faction, mark.Count, weapon: weapon, subjectId: target.Id, cause: cause.Child("stamp-mark")));
            if (mark.Count < Mathf.Max(2, weapon.SignatureBeats)) return;
            stamps.Remove(key); target.VisibleMarks = 0;
            StampBursts++;
            RecordActivation(cause.Child("stamp-burst"));
            if (catalog.ReflectiveInkStacks > 0 || catalog.OfficeLensStacks > 0)
            {
                target.ExposedUntil = Elapsed + 1.8f;
                RecordActivation(cause.Child(catalog.ReflectiveInkStacks > 0 ? "reflective-ink" : "office-lens-exposure"));
            }
            if (catalog.CarbonRibbon)
            {
                foreach (PickupState pickup in pickups)
                    if (pickup.Kind == LootKind.Coin && (pickup.Position - target.Position).sqrMagnitude <= 6.25f) pickup.Attracting = true;
                RecordActivation(cause.Child("carbon-collector"));
            }
            events.Add(new CombatEvent(CombatEventKind.SignatureBurst, target.Position, source.Faction, weapon.SignatureRadius, weapon: weapon, cause: cause.Child("stamp-burst")));
            ApplyDamage(target, damage * Mathf.Max(0, weapon.SignatureScale), source.Faction, "Dream stamp burst", weapon: weapon, cause: cause.Child("stamp-burst"));
            if (source.Definition.CombatTier < 2) return;
            UnitState neighbor = null; float best = weapon.SignatureRadius * weapon.SignatureRadius;
            foreach (UnitState candidate in StableUnits)
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
            foreach (UnitState unit in StableUnits)
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
                RecordActivation(pulse.Cause);
                SignatureVolley(pulse, weapon);
            }
        }

        /// <summary>A bell's wave and an umbrella's opening are thrown, not radiated from the wielder: a fan of painted chimes
        /// (bell) or rain drops (umbrella) leaves the wielder's front along its facing. Each foe is struck once per volley; the
        /// shots pass through a few foes and die after about the old payoff reach, so the cadence and damage keep their size
        /// while nothing rings round the wielder's own body.</summary>
        private void SignatureVolley(SignaturePulse pulse, WeaponDefinition weapon)
        {
            UnitState source = pulse.Source;
            bool bell = weapon.Signature == SignatureWeapon.Bell, echo = pulse.Cause.ItemId == "bell-echo";
            Vector2 aim = pulse.Direction;
            UnitState target = UnitWithId(source.AttackTargetId);
            if (target != null && CanHit(source.Faction, target)) aim = target.Position - source.Position;
            if (aim.sqrMagnitude < 0.0001f)
            {
                float best = float.PositiveInfinity;
                foreach (UnitState enemy in StableUnits)
                {
                    float distance = (enemy.Position - source.Position).sqrMagnitude;
                    if (CanHit(source.Faction, enemy) && distance < best) { best = distance; aim = enemy.Position - source.Position; }
                }
            }
            aim = aim.sqrMagnitude < 0.0001f ? Vector2.right : aim.normalized;
            Vector2 muzzle = source.Position + aim * Mathf.Min(0.3f, source.Definition.Radius);
            int count = bell ? (echo ? 3 : 5) : 5;
            float spread = (bell ? 11f : 22f) * Mathf.Deg2Rad, speed = bell ? 6.5f : 7.5f;
            float reach = bell ? pulse.Radius * 1.5f : pulse.Radius * 1.15f;
            var volley = new HashSet<int>();
            events.Add(new CombatEvent(CombatEventKind.SignaturePulse, muzzle, source.Faction, 0.45f, aim, weapon, cause: pulse.Cause));
            for (int index = 0; index < count && projectiles.Count < 4096; index++)
            {
                Vector2 heading = Quaternion.Euler(0f, 0f, (index - (count - 1) * 0.5f) * spread * Mathf.Rad2Deg) * aim;
                projectiles.Add(new ProjectileState
                {
                    Id = ++nextId, Faction = source.Faction, Position = muzzle, PreviousPosition = source.Position, LaunchSegment = true,
                    Velocity = heading * speed, LaunchVelocity = heading * speed, Damage = pulse.Damage, Remaining = reach / speed,
                    Color = bell ? new Color(1f, 0.85f, 0.45f) : new Color(0.5f, 0.9f, 1f), Element = weapon.Element,
                    SourceName = weapon.Id + " payoff", Sprite = weapon.ProjectileSprite, VisualWidth = bell ? (echo ? 0.32f : 0.42f) : 0.26f,
                    Radius = bell ? 0.16f : 0.12f, SourceUnitId = source.Id, Cause = pulse.Cause, Weapon = weapon,
                    Volley = volley, ExtraPierce = bell ? 2 : 1, SignatureShot = true, IsFragment = true
                });
            }
        }

        private void OnSignatureShotHit(ProjectileState shot, UnitState target)
        {
            if (!target.Alive || shot.Weapon == null || shot.Weapon.Signature != SignatureWeapon.Bell) return;
            if (target.VisibleMarks > 0 && catalog.ChorusStacks > 0 && shot.Cause.ItemId == "bell-wave")
                ApplyDamage(target, shot.Damage * Mathf.Min(0.5f, catalog.ChorusStacks * 0.25f), shot.Faction, "Chorus marked target", cause: shot.Cause.Child("scratch-record"));
            if (target.Alive && catalog.CarbonRibbon)
            {
                target.ExposedUntil = Mathf.Max(target.ExposedUntil, Elapsed + 1.5f);
                RecordActivation(shot.Cause.Child("carbon-exposure"));
            }
        }

#if UNITY_EDITOR
        public void SignatureBeatFixture(UnitState source) => AdvanceSignatureBeat(source, source.Definition.Weapon.Damage, NewCause(source.Id));
        public void StampFixture(UnitState source, UnitState target) => AddStamp(source, target, source.Definition.Weapon.Damage, NewCause(source.Id));
#endif
    }
}
