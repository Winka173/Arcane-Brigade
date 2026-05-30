using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Balance round 6 (Docs/Balance_Pass_2026-10-19.md): weak heroes buffed through their own mechanics.</summary>
    public sealed partial class CombatSimulation
    {
        private float dreamSwapAt, royalGuardUntil, royalMendAt;
        private int kitchenCourse;
        private float dreamShotAt;
        public const float DreamShotShare = .35f, DreamShotCadence = 1.3f;
        private Color DreamTint { get { Color c = HeroIdentities.For(IdentityId).Color; return new Color(c.r, c.g, c.b, .55f); } }

        /// <summary>Drowsy Seer, Full Moon (round 6b): each dream also fires a weaker copy of the hero's basic attack (35% damage,
        /// same weapon look) at the enemy nearest to that dream, 1.3x slower than the hero's own weapon.</summary>
        private void DreamShots()
        {
            if (Elapsed < dreamShotAt) return;
            WeaponDefinition w = Champion.Definition.Weapon;
            dreamShotAt = Elapsed + Mathf.Max(.3f, w.Cooldown * DreamShotCadence);
            float damage = IdentityPower * DreamShotShare * ApproachScale;
            for (int i = 0; i < 3; i++)
            {
                Vector2 from = awakenEchoes[i];
                UnitState target = IdentityTarget(from, Mathf.Max(2.5f, w.Range));
                if (target == null) continue;
                awakenEchoFired[i] = Elapsed;
                if (w.Kind == WeaponKind.Projectile) IdentityShot(from, target.Position - from, damage, Mathf.Min(3, w.ProjectileCount), element: w.Element, sprite: w.ProjectileSprite, tint: DreamTint);
                else IdentityHit(MorphGeometry.Crescent, from, target.Position, Mathf.Clamp(Vector2.Distance(from, target.Position) + .3f, .8f, 2.2f), damage, element: w.Element);
            }
        }
        public const float DreamSwapHealth = .4f, DreamSwapCooldown = 8f, RoyalGuardSeconds = .5f, RoyalMendCooldown = 3f, LeadGuardRadius = 2f;

        /// <summary>Drowsy Seer: below 40% HP the hero trades places with its farthest dream (3 s back on its route), every 8 s.</summary>
        private void DreamSwap()
        {
            if (IdentityId != "DrowsySeer" || awakenPath.Count < 2 || Elapsed < dreamSwapAt || Champion.Health >= Champion.Definition.Health * DreamSwapHealth) return;
            Vector2 from = Champion.Position, to = PathAgo(3);
            if ((to - from).sqrMagnitude < .25f) return;
            dreamSwapAt = Elapsed + DreamSwapCooldown;
            Champion.Position = Champion.PreviousPosition = to;
            heroSlipGuardUntil = Elapsed + .6f; // the swap may land among enemies (HeroBodyGuard)
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

        /// <summary>The finale as a transformation ends: the hero's sign falls on the nearest enemy and bursts (1.2 W shared by
        /// up to five within 1.3 units).</summary>
        public const float AwakenFinaleBudget = 1.2f;
        private void AwakenFinale()
        {
            UnitState foe = IdentityTarget(Champion.Position, 7); if (foe == null) return;
            var g = IdentityHit(MorphGeometry.Disc, foe.Position, Champion.Position, 1.3f, IdentityPower * AwakenFinaleBudget * ApproachScale * CrowdScale, .4f, 5);
            if (g != null) { g.Big = true; g.Finale = true; }
        }

        // ---- Lone Jelly, Mitosis (owner 23/10/2026): the two jellies are fighters of their own. Each walks to its own enemy
        // (within 4 units of the hero, never farther than 3.2 from it) and comes back to its side of the hero when there is
        // none; their organs land on the enemy itself (bite, gland or threadwork by the approach).
        private readonly Vector2[] jellies = new Vector2[2];
        private float jelliesSince = -1;
        public const float JellySpeed = 4.2f, JellyLeash = 6f; // 24/10: the twins roam further to hunt (3.2 / 3.2)
        private void Mitosis(float delta)
        {
            if (jelliesSince != awakenStarted)
            {
                jelliesSince = awakenStarted; Vector2 side = Perpendicular() * 1.1f;
                jellies[0] = ClampPosition(Champion.Position + side, .2f); jellies[1] = ClampPosition(Champion.Position - side, .2f);
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 home = Champion.Position + new Vector2(i == 0 ? 1.1f : -1.1f, -.15f);
                UnitState near = IdentityTarget(jellies[i], 6);
                Vector2 want = near != null && (near.Position - Champion.Position).sqrMagnitude < 36
                    ? near.Position + ((jellies[i] - near.Position).sqrMagnitude > .0001f ? (jellies[i] - near.Position).normalized : Vector2.right) * .9f : home;
                if ((jellies[i] - Champion.Position).sqrMagnitude > JellyLeash * JellyLeash) want = home;
                jellies[i] = ClampPosition(Vector2.MoveTowards(jellies[i], want, JellySpeed * delta), .2f);
            }
            awakenEchoes[0] = jellies[0]; awakenEchoes[1] = jellies[1]; awakenEchoes[2] = Champion.Position;
            if (Elapsed < awakenEchoAt) return;
            awakenEchoAt = Elapsed + .3f;
            int who = awakenRound++ % 3; Vector2 from = awakenEchoes[who];
            UnitState target = IdentityTarget(from, 5); if (target == null) return;
            int organ = Mathf.Clamp(IdentityApproach, 0, 2); float delay = organ == 0 ? .16f : organ == 1 ? .3f : .2f;
            var hit = IdentityHit(MorphGeometry.Disc, target.Position, from, .6f, IdentityPower * .85f * ApproachScale, delay, 2,
                organ == 1 ? DamageElement.Frost : DamageElement.Physical, slow: organ == 1 ? .5f : organ == 2 ? .35f : 0);
            if (hit == null) return;
            hit.Painted = true; // the organ's own motion is drawn at the enemy (HeroGlyphRole.Organ)
            IdentityGlyph(MorphGeometry.Disc, target.Position, from, .6f, .45f, delay, HeroGlyphRole.Organ);
            awakenEchoFired[who] = Elapsed;
        }
    }
}
