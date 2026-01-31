using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private sealed class PoisonNote { public UnitState Target, Source; public float At, Damage; public CombatCause Cause; }
        private readonly List<PoisonNote> octoberPoison = new List<PoisonNote>();
        private readonly Dictionary<string, List<UnitState>> octoberTalents = new Dictionary<string, List<UnitState>>();
        private readonly List<UnitState> noOctoberUnits = new List<UnitState>();
        private float octoberHurtAt = -100, octoberRebateAt, octoberTickAt, octoberMoveTime, octoberResponseAt, octoberSpiritAt;
        private int octoberInjuries, octoberHearts, octoberChests;
        private bool octoberRecoveryUsed;
        private int octoberDodges;
        public int OctoberHeartsCollected => octoberHearts;
        public int OctoberChestsOpened => octoberChests;
        public int OctoberInjuries => octoberInjuries;
        public int OctoberDodges => octoberDodges;
        private string ReferenceHero => Champion?.Definition.ReferenceHero ?? "";
        private List<UnitState> OctoberUnits(string talent) => octoberTalents.TryGetValue(talent, out List<UnitState> list) ? list : noOctoberUnits;
        private bool HasOctober(string talent) => OctoberUnits(talent).Count > 0;
        private bool OctoberTrait(UnitState u, string talent) => u.Definition.ReferenceCompanion == talent || u == Champion && System.Array.IndexOf(catalog.OctoberMemories, talent) >= 0;
        private void ResetOctoberCombat()
        {
            octoberPoison.Clear(); octoberTalents.Clear(); octoberDodges = octoberInjuries = octoberHearts = octoberChests = 0;
            octoberRecoveryUsed = false; octoberHurtAt = -100; octoberRebateAt = octoberTickAt = octoberMoveTime = octoberResponseAt = octoberSpiritAt = 0;
            foreach (UnitState u in units)
            {
                u.MountId = u.RiderId = u.OctoberAttacks = u.OctoberKills = 0; u.OctoberNext = 0;
                string name = u.Definition.ReferenceCompanion;
                if (u.Faction != Faction.Companion || string.IsNullOrEmpty(name)) continue;
                if (!octoberTalents.ContainsKey(name)) octoberTalents.Add(name, new List<UnitState>());
                octoberTalents[name].Add(u);
            }
            foreach (string name in catalog.OctoberMemories ?? System.Array.Empty<string>())
            {
                if (string.IsNullOrEmpty(name)) continue;
                if (!octoberTalents.ContainsKey(name)) octoberTalents.Add(name, new List<UnitState>());
                if (!octoberTalents[name].Contains(Champion)) octoberTalents[name].Add(Champion);
            }
            foreach (UnitState pony in OctoberUnits("Pony"))
            {
                if (pony == Champion) continue;
                UnitState rider = null; float best = float.PositiveInfinity;
                foreach (UnitState u in units)
                {
                    if (u.Faction != Faction.Companion || u == pony || u.MountId != 0 || u.Definition.ReferenceCompanion == "Pony") continue;
                    float distance = (u.Position - pony.Position).sqrMagnitude;
                    if (distance < best) { best = distance; rider = u; }
                }
                if (rider != null) { rider.MountId = pony.Id; pony.RiderId = rider.Id; }
            }
        }
        private float OctoberCycle(UnitState unit)
        {
            if (!catalog.EnableHeroMechanics || unit.Faction == Faction.Enemy) return 1;
            float bonus = HasOctober("Athlete") && Elapsed - octoberHurtAt < 4 ? .1f : 0;
            if (OctoberTrait(unit, "Fierce Fighter") || OctoberTrait(unit, "\"Bullied\" Sophia")) bonus += Mathf.Min(.2f, unit.OctoberAttacks * .01f);
            if (OctoberTrait(unit, "Hopping Zombie")) bonus += Mathf.Min(.15f, Kills / 8 * .01f);
            if (System.Array.IndexOf(unit.Definition.Tags, "gauntlet") >= 0 && HasOctober("Masked Wrestler")) bonus += Mathf.Min(.15f, octoberInjuries * .01f);
            if (unit.Faction == Faction.Champion && HasOctober("Evil")) { int kills = 0; foreach (UnitState u in OctoberUnits("Evil")) kills += u.OctoberKills; bonus += Mathf.Min(.2f, kills / 8 * .02f); }
            return 1 / (1 + bonus);
        }
        private float OctoberAttackDamage(UnitState source, float damage)
        {
            if (!catalog.EnableHeroMechanics || source.Faction == Faction.Enemy) return damage;
            if (OctoberTrait(source, "\"Triumphant Return\" Daniel") && Elapsed >= 30) damage *= 3;
            if (source.Faction == Faction.Champion && ReferenceHero == "Frosty") damage *= 1 + Mathf.Min(1.2f, CompanionCount * .12f);
            if (ReferenceHero == "Box Shell" && Elapsed >= 25 && source.Faction != Faction.Enemy) damage *= source.Definition.Weapon.Kind == WeaponKind.Melee ? 1.4f : .75f;
            return damage;
        }
        private bool ResolveOctoberAttack(UnitState source, float damage, CombatCause cause)
        {
            if (IdentityMortar(source, damage)) return true;
            if (!catalog.EnableHeroMechanics || source != Champion || ReferenceHero != "Burgerboo") return false;
            UnitState target = UnitWithId(source.AttackTargetId);
            if (target != null && !CanHit(source.Faction, target)) target = null;
            if (target == null) return true;
            int count = Mathf.Min(2, Mathf.Max(1, source.Definition.Weapon.ProjectileCount));
            for (int i = 0; i < count; i++) Field(ExpandedFieldKind.Fuse, source, target.Position + Vector2.right * (i - (count - 1) * .5f) * .9f, source.AttackDirection, 1.1f, damage / count, .85f, .3f, cause);
            return true;
        }
        private float OctoberIncoming(float damage)
        {
            if (!catalog.EnableHeroMechanics) return damage;
            if (ReferenceHero == "Card Queen") return 1;
            if (HasOctober("\"Colorful Fantasy\" Pinky") && Elapsed - octoberHurtAt >= 6) damage *= .75f;
            return damage;
        }
        private bool FollowOctoberMount(UnitState rider)
        {
            if (rider.MountId <= 0) return false;
            UnitState mount = UnitWithId(rider.MountId);
            if (mount != null && !mount.Alive) mount = null;
            if (mount == null) { rider.MountId = 0; rider.BodyLift = 0; return false; }
            rider.Position = ClampPosition(mount.Position + Vector2.up * mount.Definition.BodyHeight * .35f, rider.Definition.Radius);
            rider.BodyLift = mount.Definition.BodyHeight * .25f; rider.Velocity = mount.Velocity; return true;
        }
        private Vector2 OctoberEnemyTarget(UnitState enemy)
        {
            if (!enemy.Definition.IsBoss && catalog.EnableHeroMechanics && Mathf.Repeat(Elapsed, 6) < 2)
                foreach (UnitState decoy in OctoberUnits("\"Unstoppable\" Paul")) if (decoy.Alive && (decoy.Position - enemy.Position).sqrMagnitude < 64) return decoy.Position;
            return Champion.Position;
        }
        private void OctoberEcho(UnitState source, float scale, CombatCause cause, int count = 1)
        {
            if (!source.Alive || Elapsed < source.OctoberNext || projectiles.Count >= 1000) return;
            UnitState target = FindTarget(source, Mathf.Min(8, Mathf.Max(3, source.Definition.Weapon.Range)));
            if (target == null) return;
            source.OctoberNext = Elapsed + .7f;
            Vector2 direction = (target.Position - source.Position).normalized;
            count = Mathf.Clamp(count, 1, 3);
            for (int i = 0; i < count; i++)
            projectiles.Add(new ProjectileState { Id = ++nextId, SourceUnitId = source.Id, SourceName = source.Definition.DisplayName + " response", Cause = new CombatCause(cause.RootId, source.Id, "companion-response", 1),
                Faction = source.Faction, Position = source.Position, PreviousPosition = source.Position, Velocity = Quaternion.Euler(0, 0, (i - (count - 1) * .5f) * 18) * direction * 6, Remaining = 1.4f, Damage = source.Definition.Weapon.Damage * scale / count,
                Color = new Color(.45f, .95f, .88f), Element = source.Definition.Weapon.Element, Sprite = catalog.Projectile, VisualWidth = .18f, Radius = .06f, IsFragment = true });
        }
        private void OctoberWeave(UnitState source, CombatCause cause)
        {
            if (morphStrikes.Count > 85) return;
            var pins = new List<UnitState>(3); Vector2 point = source.Position;
            for (int i = 0; i < 3; i++)
            {
                UnitState next = null; float best = i == 0 ? 36 : 9;
                foreach (UnitState enemy in units)
                {
                    float distance = (enemy.Position - point).sqrMagnitude;
                    if (CanHit(source.Faction, enemy) && !pins.Contains(enemy) && distance < best) { next = enemy; best = distance; }
                }
                if (next == null) break; pins.Add(next); point = next.Position;
            }
            float budget = source.Definition.Weapon.Damage * .45f / Mathf.Max(1, pins.Count); point = source.Position;
            for (int i = 0; i < pins.Count; i++)
            {
                Vector2 end = pins[i].Position; float delay = .18f + i * .08f;
                morphStrikes.Add(new MorphStrike { Kind = BuildMorphKind.Thread, Geometry = MorphGeometry.Line, Source = source, Position = point, End = end,
                    Direction = (end - point).normalized, Width = .12f, Created = Elapsed, At = Elapsed + delay, Until = Elapsed + delay + .34f,
                    Budget = budget, MaxTargets = 1, Stage = 1, Cause = cause.Child("lantern-weave") });
                point = end;
            }
        }
        private void ProcessOctoberCombat(float delta, Vector2 movement)
        {
            if (!catalog.EnableHeroMechanics || !Champion.Alive || Phase != RunPhase.Combat) return;
            int original = events.Count;
            for (int i = 0; i < original; i++)
            {
                CombatEvent e = events[i]; UnitState source = UnitWithId(e.Cause.SourceUnitId);
                if (e.Kind == CombatEventKind.Fire && e.Faction != Faction.Enemy && e.Cause.Depth == 0)
                {
                    if (source != null) source.OctoberAttacks = Mathf.Min(1000, source.OctoberAttacks + 1);
                    if (e.Faction == Faction.Champion)
                    {
                        foreach (UnitState mage in OctoberUnits("Grey Mage")) OctoberEcho(mage, .65f, e.Cause);
                        if (!HeroIdentityEnabled && ReferenceHero == "Billy" && e.Critical)
                            foreach (UnitState ally in units) if (ally.Faction == Faction.Companion && combatRandom.NextDouble() < .5) OctoberEcho(ally, 1, e.Cause);
                    }
                    if (source != null && OctoberTrait(source, "\"Naming Demon\" Yan") && source.OctoberAttacks % 3 == 0) OctoberEcho(source, .25f, e.Cause);
                    if (source != null && OctoberTrait(source, "Original:TeacupCricket") && source.OctoberAttacks % 5 == 0) OctoberEcho(source, .4f, e.Cause);
                    if (!HeroIdentityEnabled && source == Champion && ReferenceHero == "Original:LanternWeaver" && source.OctoberAttacks % 4 == 0)
                        OctoberWeave(source, e.Cause);
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Cause.Depth == 0 && source != null && source.Faction != Faction.Enemy && e.Subject != null)
                {
                    if (OctoberTrait(source, "Camo Shroom") && !e.Subject.Definition.IsBoss) e.Subject.ChillUntil = Mathf.Max(e.Subject.ChillUntil, Elapsed + .4f);
                    if (OctoberTrait(source, "Attractive Witch") && !e.Subject.Definition.IsBoss) { e.Subject.Position = ClampPosition(Vector2.MoveTowards(e.Subject.Position, source.Position, OctoberBalance.Push(e.Subject.Definition, .25f)), e.Subject.Definition.Radius); spatial.Move(e.Subject); }
                    if ((OctoberTrait(source, "Mummy") || ReferenceHero == "Gulpy" && (source == Champion || System.Array.IndexOf(source.Definition.Tags, "archive") >= 0)) && octoberPoison.Count < 128)
                        octoberPoison.Add(new PoisonNote { Source = source, Target = e.Subject, At = Elapsed + 1, Damage = source.Definition.Weapon.Damage * .25f, Cause = e.Cause.Child("companion-poison") });
                    if (OctoberTrait(source, "Fierce Fighter") && source.OctoberAttacks % 3 == 0 && Elapsed >= source.OctoberNext) { source.OctoberNext = Elapsed + 1; Field(ExpandedFieldKind.Echo, source, e.Position, e.Direction, .8f, source.Definition.Weapon.Damage * .15f, .1f, .2f, e.Cause.Child("fighter-blast")); }
                    if (OctoberTrait(source, "TV Head") && e.Critical && Elapsed >= octoberRebateAt && combatRandom.NextDouble() < .25) { octoberRebateAt = Elapsed + .8f; Gold = RunSession.BoundedCounter((long)Gold + 1); }
                }
                if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy)
                {
                    if (source != null) source.OctoberKills++;
                    if (ReferenceHero == "Omnom") Heal(1, e.Cause.Child("night-maw-meal"));
                    if (source != null && OctoberTrait(source, "Peach") && e.Cause.Depth == 0 && combatRandom.NextDouble() < .3) Heal(1, e.Cause.Child("peach-mercy"));
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Champion)
                {
                    octoberInjuries++; octoberHurtAt = Elapsed;
                    if (HasOctober("Clown") && Elapsed >= octoberSpiritAt) { octoberSpiritAt = Elapsed + 1; Heal(1, e.Cause.Child("clown-comfort")); }
                    if (HasOctober("\"Misfortune\" Emily") && Elapsed >= octoberRebateAt) { octoberRebateAt = Elapsed + 2; Gold = RunSession.BoundedCounter((long)Gold + 3); }
                    if (HasOctober("\"Enduring Suffering\" Dorothy") && !octoberRecoveryUsed && ReferenceHero != "Card Queen") { octoberRecoveryUsed = true; Champion.Definition.Health = Mathf.Max(1, Champion.Definition.Health - 2); Heal(Champion.Definition.Health, e.Cause.Child("dorothy-recovery")); }
                    if (ReferenceHero == "The Big Pot") catalog.PassiveHeal = Mathf.Min(6, catalog.PassiveHeal + .15f);
                    if (ReferenceHero == "Porcelain Doll") Champion.Definition.CollisionDamage = Mathf.Min(80, Champion.Definition.CollisionDamage + 1);
                    if (!HeroIdentityEnabled && (ReferenceHero == "Card Queen" || ReferenceHero == "Sassy Senior")) Field(ExpandedFieldKind.Echo, Champion, Champion.Position, Vector2.zero, 3, Champion.Definition.Weapon.Damage * 2, .08f, .2f, e.Cause.Child("royal-retort"));
                }
                if (e.Kind == CombatEventKind.Dodge)
                {
                    octoberDodges++;
                    if (ReferenceHero == "Sir Purrcival") catalog.TreasureDetection = Mathf.Min(3, catalog.TreasureDetection + .02f);
                    if (!HeroIdentityEnabled && ReferenceHero == "Anby") { Champion.OctoberNext = 0; OctoberEcho(Champion, 2, e.Cause); }
                }
                if (e.Kind == CombatEventKind.Pickup && e.Loot == LootKind.Heart)
                {
                    octoberHearts++;
                    if (HasOctober("Original:RibbonOtter")) Heal(1, e.Cause.Child("otter-ribbon"));
                }
                if (e.Kind == CombatEventKind.ChestOpened)
                {
                    octoberChests++;
                    if (ReferenceHero == "Original:RainArchivist" || ReferenceHero == "Gulpy") Heal(4, e.Cause.Child("rain-receipt"));
                    if (!HeroIdentityEnabled && ReferenceHero == "Original:RainArchivist" && octoberChests % 3 == 0) { Champion.OctoberNext = 0; OctoberEcho(Champion, 1, e.Cause, 3); }
                }
            }
            for (int i = octoberPoison.Count - 1; i >= 0; i--)
            {
                PoisonNote p = octoberPoison[i]; if (!p.Target.Alive || !p.Source.Alive) { octoberPoison.RemoveAt(i); continue; }
                if (Elapsed < p.At) continue; octoberPoison.RemoveAt(i); ApplyDamage(p.Target, p.Damage, p.Source.Faction, "Living toy poison", cause: p.Cause);
            }
            if (movement.sqrMagnitude > .01f) octoberMoveTime += delta;
            if (ReferenceHero == "Nekomata" && octoberMoveTime >= 3) { octoberMoveTime -= 3; Gold = RunSession.BoundedCounter((long)Gold + 2); }
            if (Elapsed >= octoberTickAt)
            {
                octoberTickAt = Elapsed + 1;
                if (ReferenceHero == "Omnom") ApplyDamage(Champion, 1, Faction.Enemy, "Night Maw hunger", cause: NewCause(Champion.Id).Child("hunger"), injectedDamage: true, healthCost: true);
                if (ReferenceHero == "Frosty" && movement.sqrMagnitude < .001f) Heal(Champion.Definition.Health * .05f, NewCause(Champion.Id).Child("frost-rest"));
                if (!HeroIdentityEnabled && ReferenceHero == "Steel Shell" && Mathf.FloorToInt(Elapsed) % 3 == 0) Field(ExpandedFieldKind.Echo, Champion, Champion.Position, Vector2.zero, 2.2f, Champion.Definition.CollisionDamage, .25f, .3f, NewCause(Champion.Id).Child("steel-resonance"));
                if (!HeroIdentityEnabled && ReferenceHero == "Original:HourglassStag" && Mathf.FloorToInt(Elapsed) % 6 == 0)
                {
                    Field(ExpandedFieldKind.Echo, Champion, Champion.Position, Vector2.zero, 2.5f, Champion.Definition.Weapon.Damage * .65f, .1f, .35f, NewCause(Champion.Id).Child("hourglass-ring"));
                    foreach (UnitState enemy in units) if (enemy.Faction == Faction.Enemy && !enemy.Definition.IsBoss && (enemy.Position - Champion.Position).sqrMagnitude < 9) enemy.ChillUntil = Elapsed + .7f;
                }
                foreach (UnitState u in OctoberUnits("Domestic Spiked Ball")) u.Definition.CollisionDamage = (18 + Mathf.Min(20, octoberInjuries)) * 1.08f;
            }
            foreach (UnitState nurse in OctoberUnits("Young Lady"))
                if (Elapsed >= nurse.OctoberNext) { nurse.OctoberNext = Elapsed + 5; DropLoot(LootKind.Heart, nurse.Position, 1); }
            if (HasOctober("\"Overtime King\" Philip") && Elapsed >= octoberResponseAt) { octoberResponseAt = Elapsed + 5; ApplyDamage(Champion, .5f, Faction.Enemy, "Overtime exhaustion", cause: NewCause(Champion.Id).Child("overtime-cost"), injectedDamage: true, healthCost: true); }
        }
    }
}
