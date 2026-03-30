using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Balance round 6 (Docs/Balance_Pass_2026-10-19.md): weak heroes buffed through their own mechanics.</summary>
    public sealed partial class CombatSimulation
    {
        private float dreamSwapAt, royalGuardUntil, royalMendAt;
        private int kitchenCourse;
        public const float DreamSwapHealth = .4f, DreamSwapCooldown = 8f, RoyalGuardSeconds = .5f, RoyalMendCooldown = 3f, LeadGuardRadius = 2f;

        /// <summary>Drowsy Seer: below 40% HP the hero trades places with its farthest dream (3 s back on its route), every 8 s.</summary>
        private void DreamSwap()
        {
            if (IdentityId != "DrowsySeer" || awakenPath.Count < 2 || Elapsed < dreamSwapAt || Champion.Health >= Champion.Definition.Health * DreamSwapHealth) return;
            Vector2 from = Champion.Position, to = PathAgo(3);
            if ((to - from).sqrMagnitude < .25f) return;
            dreamSwapAt = Elapsed + DreamSwapCooldown;
            Champion.Position = Champion.PreviousPosition = to;
            IdentityGlyph(MorphGeometry.Line, from, to, 0, .3f, role: HeroGlyphRole.Lob); // a short ink trail
        }

        /// <summary>Ivory Sovereign: each blow costs 1 royal HP, then 0.5 s of royal guard ignores further blows (a crown glint).</summary>
        private float RoyalGuard()
        {
            if (Elapsed < royalGuardUntil) return 0;
            royalGuardUntil = Elapsed + RoyalGuardSeconds;
            events.Add(new CombatEvent(CombatEventKind.Dodge, Champion.Position, Faction.Champion, subject: Champion));
            return 1;
        }
        /// <summary>Ivory Sovereign: an enemy executed by a Verdict line restores 1 royal HP, at most once every 3 s.</summary>
        private void RoyalMend()
        {
            if (ReferenceHero != "Card Queen" || Elapsed < royalMendAt || Champion.Health >= Champion.Definition.Health) return;
            royalMendAt = Elapsed + RoyalMendCooldown;
            Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + 1);
        }

        /// <summary>Supper Mortar: every sixth course is a dessert served on the enemy nearest the hero, knocking it away from the hero.</summary>
        private UnitState DessertCourse()
        {
            if (++kitchenCourse % 6 != 0) return null;
            UnitState near = IdentityTarget(Champion.Position, 4);
            return near;
        }
        private void ShoveFromHero(UnitState enemy, float distance)
        {
            if (enemy == null || !enemy.Alive || enemy.Definition.IsBoss) return;
            Vector2 away = enemy.Position - Champion.Position;
            if (away.sqrMagnitude < .0001f) away = Vector2.right;
            enemy.Position = ClampPosition(enemy.Position + away.normalized * distance * (1 - Mathf.Clamp01(enemy.Definition.KnockbackResistance)), enemy.Definition.Radius);
        }

        /// <summary>Patchwork Host: the lead friend strikes enemies within 2 tiles of the hero first.</summary>
        private UnitState LeadGuardTarget(UnitState unit, float reach)
        {
            if (unit != identityLead || IdentityId != "PatchworkHost") return null;
            UnitState best = null; float bestDistance = LeadGuardRadius * LeadGuardRadius;
            foreach (UnitState u in units)
            {
                if (!u.Alive || !CanHit(unit.Faction, u)) continue;
                float toHero = (u.Position - Champion.Position).sqrMagnitude;
                if (toHero > bestDistance || (u.Position - unit.Position).magnitude > reach + u.Definition.Radius) continue;
                best = u; bestDistance = toHero;
            }
            return best;
        }
    }
}
