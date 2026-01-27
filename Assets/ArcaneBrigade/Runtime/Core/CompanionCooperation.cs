using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private float nextCooperation;
        private float nextFanCooperation;
        private float nextHammerCooperation;
        private float nextCollectCooperation;
        private UnitState cooperationCollector;
        private CombatCause cooperationCollectCause;
        private float cooperationCollectUntil;
        private readonly HashSet<int> cooperationCollected = new HashSet<int>();
        public int CooperationPayoffs { get; private set; }

        private static bool CooperationAlly(UnitState unit)
        {
            return unit != null && unit.Alive && (unit.Faction == Faction.Champion || unit.Faction == Faction.Companion);
        }

        private void ClearCooperation()
        {
            nextCooperation = nextFanCooperation = nextHammerCooperation = nextCollectCooperation = 0;
            cooperationCollector = null;
            cooperationCollected.Clear();
            cooperationCollectCause = default;
            cooperationCollectUntil = 0;
            CooperationPayoffs = 0;
        }

        private void ProcessCooperation()
        {
            if (Elapsed < cooperationCollectUntil && CooperationAlly(cooperationCollector)) return;
            cooperationCollector = null;
            cooperationCollected.Clear();
        }

        private void ResolveCooperationAttack(UnitState source, float damage, CombatCause cause)
        {
            if (Phase != RunPhase.Combat || !CooperationAlly(source) || cause.Depth != 0 || cause.RootId == 0 || Elapsed < nextCooperation) return;
            WeaponDefinition weapon = source.Definition.Weapon;
            bool fan = weapon.Id == "PaperFan" || weapon.Id == "MoonFeatherFan";
            bool hammer = weapon.Family == ExpandedWeapon.Hammer || weapon.Mechanic == CarnivalWeapon.SpringHammer;
            if ((fan && Elapsed >= nextFanCooperation) || (hammer && Elapsed >= nextHammerCooperation))
            {
                ProjectileState selected = null;
                float best = Mathf.Pow(Mathf.Min(weapon.Range, 3f), 2);
                foreach (ProjectileState shot in projectiles)
                {
                    if (shot.Faction == Faction.Enemy || shot.SourceUnitId == source.Id || shot.IsFragment || shot.Cause.Depth != 0 || shot.Cause.RootId == 0 || shot.Remaining <= 0 || shot.Damage <= 0 || shot.Weapon == null) continue;
                    UnitState owner = units.Find(candidate => candidate.Id == shot.SourceUnitId);
                    if (!CooperationAlly(owner)) continue;
                    bool ingredient = fan ? shot.Element == DamageElement.Ember : shot.Weapon.Mechanic == CarnivalWeapon.BloomOrb || shot.Weapon.Id == "DreamOrbStaff";
                    Vector2 offset = shot.Position - source.Position;
                    float distance = offset.sqrMagnitude;
                    if (!ingredient || Vector2.Dot(offset.normalized, source.AttackDirection) < weapon.ConeDot || distance > best || distance == best && selected != null && shot.Id > selected.Id) continue;
                    selected = shot;
                    best = distance;
                }
                if (selected != null && projectiles.Count <= 4093)
                {
                    SpreadCooperationProjectile(source, selected, fan);
                    if (fan) nextFanCooperation = Elapsed + 2.5f;
                    else nextHammerCooperation = Elapsed + 2.5f;
                    return;
                }
            }
            if (weapon.Kind != WeaponKind.Melee || weapon.Family == ExpandedWeapon.Fish || weapon.Mechanic == CarnivalWeapon.SatelliteDisc || weapon.Mechanic == CarnivalWeapon.YoYo || cooperationCollector == source || !CooperationAlly(cooperationCollector) || Elapsed >= cooperationCollectUntil) return;
            int victims = 0;
            CombatCause sweep = cause.Child("cooperation-collected-sweep");
            foreach (UnitState target in units)
            {
                Vector2 offset = target.Position - source.Position;
                if (!cooperationCollected.Contains(target.Id) || !CanHit(source.Faction, target) || offset.magnitude > Mathf.Min(weapon.Range, 3f) + target.Definition.Radius || Vector2.Dot(offset.normalized, source.AttackDirection) < weapon.ConeDot) continue;
                ApplyDamage(target, Mathf.Max(0, damage) * 0.25f, source.Faction, "Collected sweep", source.AttackDirection, cause: sweep);
                if (++victims >= 3) break;
            }
            if (victims == 0) return;
            RecordActivation(sweep);
            RecordActivation(cooperationCollectCause.Child("cooperation-collect-assist"));
            events.Add(new CombatEvent(CombatEventKind.Support, source.Position, source.Faction, direction: source.AttackDirection, weapon: weapon, subjectId: source.Id, subject: source, cause: sweep));
            cooperationCollector = null;
            cooperationCollected.Clear();
            nextCooperation = Elapsed + 0.45f;
            CooperationPayoffs++;
        }

        private void SpreadCooperationProjectile(UnitState helper, ProjectileState original, bool fan)
        {
            string recipe = fan ? "cooperation-fanned-fire" : "cooperation-shattered-orb";
            CombatCause cause = original.Cause.Child(recipe);
            Vector2 heading = helper.AttackDirection.sqrMagnitude > 0.001f ? helper.AttackDirection.normalized : Vector2.right;
            projectiles.Remove(original);
            for (int branch = 0; branch < 3; branch++)
            {
                Vector2 velocity = (Vector2)(Quaternion.Euler(0, 0, (branch - 1) * (fan ? 28f : 38f)) * heading) * Mathf.Clamp(original.Velocity.magnitude, 3f, 7f);
                var fragment = new ProjectileState
                {
                    Id = ++nextId, Faction = original.Faction, SourceUnitId = original.SourceUnitId,
                    Position = original.Position, PreviousPosition = original.Position, Velocity = velocity,
                    Damage = original.Damage / 3f, Remaining = Mathf.Min(original.Remaining, 1.1f),
                    Color = original.Color, Element = original.Element, SourceName = recipe,
                    Sprite = original.Sprite, VisualWidth = Mathf.Clamp(original.VisualWidth * 0.65f, 0.15f, 0.45f),
                    Radius = Mathf.Clamp(original.Radius * 0.65f, 0.04f, 0.12f), Critical = original.Critical,
                    Cause = cause, IsFragment = true, RandomBounces = 0
                };
                fragment.Victims.UnionWith(original.Victims);
                projectiles.Add(fragment);
            }
            RecordActivation(cause);
            RecordActivation(new CombatCause(original.Cause.RootId, helper.Id, recipe + "-assist", 1));
            events.Add(new CombatEvent(CombatEventKind.ProjectileSplit, original.Position, original.Faction, direction: heading, weapon: original.Weapon, cause: cause));
            events.Add(new CombatEvent(CombatEventKind.Support, helper.Position, helper.Faction, subjectId: helper.Id, subject: helper, cause: new CombatCause(original.Cause.RootId, helper.Id, recipe + "-assist", 1)));
            nextCooperation = Elapsed + 0.45f;
            CooperationPayoffs++;
        }

        private void RegisterCooperationPull(UnitState source, UnitState target, Vector2 before, CombatCause cause)
        {
            if (Phase != RunPhase.Combat || !CooperationAlly(source) || !target.Alive || target.Definition.IsBoss || cause.Depth != 0 || (target.Position - before).sqrMagnitude < 0.0001f) return;
            if (cooperationCollector == source && cooperationCollectCause.RootId == cause.RootId && Elapsed < cooperationCollectUntil)
            {
                if (cooperationCollected.Count < 6) cooperationCollected.Add(target.Id);
                return;
            }
            if (Elapsed < nextCollectCooperation) return;
            cooperationCollected.Clear();
            cooperationCollected.Add(target.Id);
            cooperationCollector = source;
            cooperationCollectCause = cause;
            cooperationCollectUntil = Elapsed + 0.95f;
            nextCollectCooperation = Elapsed + 2.5f;
        }
    }
}
