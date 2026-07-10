using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Mythic companions' own skills (Docs/Content_Expansion_Proposal.md §4.4, 18/10/2026): a Mythic friend must do
    /// something no other friend does. Each skill runs from the friend's own position or blows (nothing radiates from the
    /// hero), and its moment is inked with the identity strike glyphs.</summary>
    public sealed partial class CombatSimulation
    {
        private readonly Dictionary<int, float> skillAt = new Dictionary<int, float>();
        private readonly Dictionary<int, int> skillCount = new Dictionary<int, int>();
        private static readonly List<UnitState> skillTargets = new List<UnitState>();

        /// <summary>Companion skill ids, for checks and descriptions.</summary>
        public static readonly string[] CompanionSkillIds = { "PillowOracle", "ReelOwl", "ClockworkPiper", "CoilKettle", "IronLocomotive", "PaperInventor", "ReturnCaptain", "MirrorNewt",
            "TeaDragon", "LighthouseWhale", "JackInTheBox", "KiteDragon", "SundaeGolem", "LostCarousel", "EndlessIndex", "FirstSeed", "EclipseEgg",
            "StaticFerret", "StormKitten", "VoltEel", "ShantySeaLion", "PinnedBeetle", "FossilArmadillo", "CourtHerald" };
        public int CompanionSkillCasts { get; private set; }

        private bool SkillReady(UnitState u, float period)
        {
            if (skillAt.TryGetValue(u.Id, out float at) && Elapsed < at) return false;
            skillAt[u.Id] = Elapsed + period; return true;
        }
        private UnitState Strongest(Vector2 origin, float range)
        {
            UnitState best = null;
            foreach (UnitState u in StableUnits)
                if (CanHit(Faction.Champion, u) && (u.Position - origin).sqrMagnitude <= range * range && (best == null || u.Health > best.Health)) best = u;
            return best;
        }
        private UnitState Nearest(Vector2 origin, float range)
        {
            UnitState best = null; float d = range * range;
            foreach (UnitState u in StableUnits) if (CanHit(Faction.Champion, u)) { float s = (u.Position - origin).sqrMagnitude; if (s <= d) { d = s; best = u; } }
            return best;
        }
        /// <summary>Sleep or freeze: the enemy neither moves (EnemySleepScale) nor attacks for `seconds`; bosses only briefly.</summary>
        private void Sleep(UnitState enemy, float seconds)
        {
            if (enemy.Definition.IsBoss) seconds = Mathf.Min(seconds, .3f);
            enemy.SleepUntil = Mathf.Max(enemy.SleepUntil, Elapsed + seconds);
            enemy.NextAttack = Mathf.Max(enemy.NextAttack, Elapsed + seconds); enemy.NextContact = Mathf.Max(enemy.NextContact, Elapsed + seconds);
        }
        private float EnemySleepScale(UnitState unit) => unit.Faction == Faction.Enemy && unit.SleepUntil > Elapsed ? 0 : 1;
        private float CompanionHaste(UnitState unit) => unit.Faction == Faction.Companion && unit.HasteUntil > Elapsed ? 1 / 1.3f : 1;
        /// <summary>Damage our friends deal to an anchored enemy (Return Captain).</summary>
        private float CompanionSkillDamage(UnitState target, float damage, Faction source, CombatCause cause)
        {
            if (target.Faction != Faction.Enemy || source == Faction.Enemy) return damage;
            UnitState attacker = UnitWithId(cause.SourceUnitId);
            if (attacker == null || attacker.Faction != Faction.Companion) return damage;
            if (target.AnchorUntil > Elapsed) damage *= 1.2f;
            if (catalog.BondRecordedDamage > 0 && Recorded(target)) damage *= 1 + catalog.BondRecordedDamage; // Exhibit Hall
            return damage;
        }
        private void Skill(UnitState u, MorphGeometry shape, Vector2 at, Vector2 end, float radius)
        {
            CompanionSkillCasts++;
            IdentityGlyph(shape, at, end, radius, .45f, 0, shape == MorphGeometry.Line ? HeroGlyphRole.Strike : HeroGlyphRole.Mark);
        }

        private void ProcessCompanionSkills()
        {
            int count = events.Count;
            for (int i = 0; i < count; i++)
            {
                CombatEvent e = events[i];
                if (e.Cause.Depth != 0) continue;
                UnitState actor = UnitWithId(e.Cause.SourceUnitId);
                if (actor == null || actor.Faction != Faction.Companion || !actor.Alive) continue;
                string id = actor.Definition.Id;
                if (e.Kind == CombatEventKind.Fire)
                {
                    skillCount.TryGetValue(actor.Id, out int n); skillCount[actor.Id] = ++n;
                    UnitState target = UnitWithId(actor.AttackTargetId);
                    if (id == "ReelOwl" && n % 4 == 0 && target != null && target.Alive)
                    {
                        // the reel replays the strongest friend nearby: its blow lands again on the owl's target
                        float best = 0;
                        foreach (UnitState f in StableUnits) if (f.Alive && f.Faction == Faction.Companion && f != actor && (f.Position - actor.Position).sqrMagnitude <= 9) best = Mathf.Max(best, f.Definition.Weapon.Damage);
                        if (best > 0) { ApplyWeaponDamage(target, best * .8f, Faction.Companion, DamageElement.Arcane, "Reel Owl", (target.Position - actor.Position).normalized, cause: NewCause(actor.Id).Child("reel-owl")); Skill(actor, MorphGeometry.Line, actor.Position, target.Position, 0); }
                    }
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null && catalog.BondChainChance > 0 && combatRandom.NextDouble() < catalog.BondChainChance)
                {
                    // Static Club: the hit arcs on to the nearest other enemy for half the friend's weapon damage
                    UnitState next = null; float best = 2.5f * 2.5f;
                    foreach (UnitState u in StableUnits) if (u != e.Subject && CanHit(Faction.Champion, u)) { float d = (u.Position - e.Subject.Position).sqrMagnitude; if (d < best) { best = d; next = u; } }
                    if (next != null)
                    {
                        ApplyWeaponDamage(next, actor.Definition.Weapon.Damage * .5f, Faction.Companion, DamageElement.Arcane, "Static Club", (next.Position - e.Subject.Position).normalized, cause: NewCause(actor.Id).Child("static-club"));
                        IdentityGlyph(MorphGeometry.Line, e.Subject.Position, next.Position, 0, .25f, 0, HeroGlyphRole.Strike);
                    }
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null && e.Subject.Alive && id == "CoilKettle" && !e.Subject.CoilStruck)
                {
                    // the first blow on each new enemy springs it up: a short stun
                    e.Subject.CoilStruck = true; Sleep(e.Subject, .4f); Skill(actor, MorphGeometry.Disc, e.Subject.Position, e.Subject.Position, .35f);
                }
            }
            ProcessNewCompanionSkills();
            foreach (UnitState u in StableUnits)
            {
                if (!u.Alive || u.Faction != Faction.Companion) continue;
                switch (u.Definition.Id)
                {
                    case "PillowOracle":
                        if (Strongest(u.Position, 5) is UnitState sleeper && SkillReady(u, 8)) { Sleep(sleeper, 1.5f); Skill(u, MorphGeometry.Disc, sleeper.Position, sleeper.Position, .45f); }
                        break;
                    case "ClockworkPiper":
                        if (Nearest(u.Position, 7) != null && SkillReady(u, 6))
                        {
                            foreach (UnitState f in StableUnits) if (f.Alive && f.Faction == Faction.Companion && (f.Position - u.Position).sqrMagnitude <= 9) f.HasteUntil = Elapsed + 2;
                            Skill(u, MorphGeometry.Disc, u.Position, u.Position, .4f);
                        }
                        break;
                    case "IronLocomotive":
                        if (Nearest(u.Position, 4f) is UnitState ahead && SkillReady(u, 7)) FrostWhistle(u, ahead);
                        break;
                    case "PaperInventor":
                        if (!towerBuilt.Contains(u.Id) && Nearest(u.Position, 5f) is UnitState first) PaperTower(u, first);
                        break;
                    case "ReturnCaptain":
                        if (Strongest(u.Position, 6) is UnitState anchored && SkillReady(u, 5)) { anchored.AnchorUntil = Elapsed + 5; Skill(u, MorphGeometry.Disc, anchored.Position, anchored.Position, .4f); }
                        break;
                    case "MirrorNewt":
                        for (int p = projectiles.Count - 1; p >= 0; p--)
                        {
                            ProjectileState shot = projectiles[p];
                            if (shot.Faction != Faction.Enemy || shot.MirrorChecked || (shot.Position - u.Position).sqrMagnitude > .6f * .6f) continue;
                            shot.MirrorChecked = true;
                            if (combatRandom.NextDouble() >= .25) continue;
                            projectiles.RemoveAt(p);
                            UnitState back = Nearest(u.Position, 7);
                            if (back != null) ApplyWeaponDamage(back, Mathf.Max(shot.Damage, u.Definition.Weapon.Damage), Faction.Companion, shot.Element, "Mirror Newt", (back.Position - u.Position).normalized, cause: NewCause(u.Id).Child("mirror-newt"));
                            Skill(u, MorphGeometry.Line, u.Position, back != null ? back.Position : u.Position + Vector2.up, 0);
                        }
                        break;
                }
            }
        }
    }
}
