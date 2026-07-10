using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What the shop visitors leave on the next wave (RunSession.PrepareVisitors). All of it lasts this wave only.</summary>
    [Serializable]
    public sealed class VisitorWave
    {
        public float FriendTempo = 1, EnemySpeed = 1, HeroSpeed = 1, CoinSeconds, ShadowSeconds, SandSeconds;
        public int CoinPlus, Matches;
        public bool Lamps, Puddles, Candy, Toy, GaugeStart, Cloak, Key, Umbrella;
        public string SlowSpecies = "", Species = "";
    }

    /// <summary>The visitors' traces in a fight: four corner lamps, four puddles, sweets that lure enemies, matches that flare, a
    /// lost toy wandering beside the hero, a borrowed shadow that enemies chase, an umbrella, a stitched cloak, a key, sand.</summary>
    public sealed partial class CombatSimulation
    {
        private VisitorWave Visit => catalog.Visitor ?? noVisit;
        private static readonly VisitorWave noVisit = new VisitorWave();
        private float visitorCandyAt, visitorTickAt; private int visitorMatchesLeft; private bool visitorCloakUsed;
        public float VisitorToySeconds { get; private set; }
        public int VisitorSpeciesKills { get; private set; }
        public bool VisitorKeyUsed { get; private set; }
        private ArsenalProp visitorToy, visitorShadow;

        private void ResetVisitors()
        {
            VisitorToySeconds = 0; VisitorSpeciesKills = 0; VisitorKeyUsed = false; visitorCloakUsed = false;
            visitorCandyAt = 10; visitorTickAt = 0; visitorMatchesLeft = Visit.Matches; visitorToy = visitorShadow = null;
            if (Visit.GaugeStart && HeroIdentityEnabled) identityCharge = Mathf.Max(identityCharge, HeroIdentityRequired);
            Vector2 half = catalog.ArenaSize * .5f - Vector2.one * 1.4f; half = Vector2.Min(half, new Vector2(8, 5));
            for (int i = 0; i < 4; i++)
            {
                Vector2 corner = new Vector2(i % 2 == 0 ? -half.x : half.x, i < 2 ? -half.y : half.y);
                if (Visit.Lamps) { ArsenalProp lamp = VisitorProp(ArsenalPropKind.VisitorLamp, corner, 1.3f); lamp.Order = i; }
                if (Visit.Puddles) VisitorProp(ArsenalPropKind.Puddle, corner * .55f, 1.3f);
            }
            if (Visit.Toy) visitorToy = VisitorProp(ArsenalPropKind.Toy, Champion.Position + Vector2.right * 1.2f, .3f);
            if (Visit.ShadowSeconds > 0) { visitorShadow = VisitorProp(ArsenalPropKind.Shadow, Champion.Position, .4f); visitorShadow.Until = Elapsed + Visit.ShadowSeconds; }
        }
        private ArsenalProp VisitorProp(ArsenalPropKind kind, Vector2 at, float radius)
        {
            ArsenalProp p = PlaceProp(kind, Champion, ClampPosition(at, .3f), 0, radius, false, NewCause(Champion.Id).Child("visitor"));
            p.Until = float.PositiveInfinity; return p;
        }
        private float VisitorCycle(UnitState unit) => unit.Faction == Faction.Companion ? 1f / Mathf.Max(.5f, Visit.FriendTempo) : 1;
        private float VisitorEnemyScale(UnitState unit) => unit.Faction == Faction.Enemy ? Visit.EnemySpeed : 1;
        private float VisitorHeroScale => Visit.HeroSpeed;
        private int VisitorCoins => Visit.CoinPlus > 0 && (Visit.CoinSeconds <= 0 || Elapsed <= Visit.CoinSeconds) ? Visit.CoinPlus : 0;
        private float VisitorSpawnScale => Visit.SandSeconds > 0 && Elapsed < Visit.SandSeconds ? 1.15f : 1;
        /// <summary>Lured or chasing a shadow: where an enemy walks instead of the hero.</summary>
        private bool VisitorLure(UnitState enemy, out Vector2 at)
        {
            at = default;
            if (enemy.LureUntil > Elapsed) { at = enemy.LurePoint; return true; }
            if (visitorShadow != null && Elapsed < visitorShadow.Until && !enemy.Definition.IsBoss) { at = visitorShadow.Position; return true; }
            return false;
        }
        /// <summary>The umbrella brushes aside one in five hostile shots that reach the hero's side.</summary>
        private bool VisitorUmbrella(ProjectileState shot)
        {
            if (!Visit.Umbrella || shot.Faction != Faction.Enemy || shot.UmbrellaChecked || (shot.Position - Champion.Position).sqrMagnitude > 1.44f) return false;
            shot.UmbrellaChecked = true;
            if (combatRandom.Next(5) != 0) return false;
            events.Add(new CombatEvent(CombatEventKind.Proc, shot.Position, Faction.Champion, 0, subject: Champion, cause: NewCause(Champion.Id).Child("visitor-umbrella")));
            return true;
        }

#if UNITY_EDITOR
        /// <summary>Checks: end a short fixture fight as a win so its visitor rewards settle.</summary>
        public void ForceVictoryForChecks() { if (Phase == RunPhase.Combat) Phase = RunPhase.Victory; }
#endif
        private void ProcessVisitors(float delta)
        {
            if (catalog.Visitor == null) return;
            VisitorWave v = Visit;
            int count = events.Count;
            for (int i = 0; i < count; i++)
            {
                CombatEvent e = events[i];
                if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy && e.Subject != null)
                {
                    if (v.Species.Length > 0 && e.Subject.Definition.Id == v.Species) VisitorSpeciesKills++;
                    if (visitorMatchesLeft > 0)
                    {
                        // a match: the fallen enemy flares and burns those beside it
                        visitorMatchesLeft--;
                        CombatCause flare = NewCause(Champion.Id).Child("visitor-match");
                        foreach (UnitState u in units.ToArray())
                            if (CanHit(Faction.Champion, u) && (u.Position - e.Position).sqrMagnitude <= 1.44f)
                                ApplyWeaponDamage(u, Champion.Definition.Weapon.Damage * .8f, Faction.Champion, DamageElement.Ember, "Match", u.Position - e.Position, cause: flare);
                        events.Add(new CombatEvent(CombatEventKind.Proc, e.Position, Faction.Champion, 0, cause: flare));
                    }
                }
                if (e.Kind == CombatEventKind.ChestOpened && v.Key && !VisitorKeyUsed) VisitorKeyUsed = true;
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null && e.Subject.Alive && v.SlowSpecies.Length > 0 && e.Cause.Depth == 0
                    && UnitWithId(e.Cause.SourceUnitId) is UnitState friend && friend.Faction == Faction.Companion && friend.Definition.Id == v.SlowSpecies)
                    KitSlowEnemy(e.Subject, .2f, 1.5f);
            }
            if (v.Candy && Elapsed >= visitorCandyAt)
            {
                // a sweet drops near the hero; the enemies around crowd it for 2s
                visitorCandyAt = Elapsed + 10;
                float a = (float)combatRandom.NextDouble() * Mathf.PI * 2;
                ArsenalProp sweet = VisitorProp(ArsenalPropKind.Candy, Champion.Position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 2.2f, .5f); sweet.Until = Elapsed + 2.5f;
                foreach (UnitState u in StableUnits) if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && (u.Position - sweet.Position).sqrMagnitude <= 16) { u.LureUntil = Elapsed + 2; u.LurePoint = sweet.Position; }
            }
            if (Elapsed >= visitorTickAt)
            {
                visitorTickAt = Elapsed + .5f;
                foreach (ArsenalProp p in arsenalProps)
                {
                    if (p.Kind == ArsenalPropKind.VisitorLamp)
                        foreach (UnitState u in StableUnits) if (CanHit(Faction.Champion, u) && (u.Position - p.Position).sqrMagnitude <= p.Radius * p.Radius) { u.BurnUntil = Mathf.Max(u.BurnUntil, Elapsed + 2); u.BurnCause = p.Cause.Child("visitor-lamp"); }
                    if (p.Kind == ArsenalPropKind.Puddle)
                        foreach (UnitState u in StableUnits) if (CanHit(Faction.Champion, u) && (u.Position - p.Position).sqrMagnitude <= p.Radius * p.Radius) u.KitWetUntil = Mathf.Max(u.KitWetUntil, Elapsed + 2);
                }
            }
            if (visitorToy != null)
            {
                // the lost toy wanders slowly; it only finds its way while the hero walks with it
                Vector2 drift = new Vector2(Mathf.Cos(Elapsed * .37f + 1), Mathf.Sin(Elapsed * .29f)) * 1.1f * delta;
                visitorToy.Position = ClampPosition(visitorToy.Position + drift, .3f);
                if ((visitorToy.Position - Champion.Position).sqrMagnitude <= 9) VisitorToySeconds += delta;
            }
            if (visitorShadow != null && Elapsed < visitorShadow.Until)
                visitorShadow.Position = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(Elapsed * 1.3f), Mathf.Sin(Elapsed * 1.3f)) * 3.5f, .3f);
            if (v.Cloak && !visitorCloakUsed && Champion.Health < Champion.Definition.Health * .25f)
            {
                // the stitched cloak: once, the enemies around the hero are thrown back
                visitorCloakUsed = true;
                foreach (UnitState u in StableUnits)
                {
                    Vector2 off = u.Position - Champion.Position;
                    if (!CanHit(Faction.Champion, u) || u.Definition.IsBoss || off.sqrMagnitude > 9) continue;
                    u.Position = ClampPosition(u.Position + (off.sqrMagnitude > .0001f ? off.normalized : Vector2.right) * OctoberBalance.Push(u.Definition, 2f), u.Definition.Radius); spatial.Move(u); Sleep(u, .5f);
                }
                events.Add(new CombatEvent(CombatEventKind.Proc, Champion.Position, Faction.Champion, 0, subject: Champion, cause: NewCause(Champion.Id).Child("visitor-cloak")));
            }
        }
    }
}
