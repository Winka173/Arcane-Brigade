using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Second mechanics for the transformations that had too little to show (requirement 24/10/2026, the 36-hero review): each
    /// adds a strike the presentation can paint. Drops of Kind 2+ mark where they land (HeroKitBoostVisual draws them):
    /// 2 a root spike, 3 a pollen burst, 4 a mirror's gathered beam, 5 a sleep cloud, 6 a kite's dive.</summary>
    public sealed partial class CombatSimulation
    {
        public const int DropRootSpike = 2, DropPollen = 3, DropMirrorBeam = 4, DropSleep = 5, DropKiteDive = 6;
        private float extraAt, extraAt2;
        /// <summary>Glass Dice: the three faces of the last roll and when it was thrown (the dice drawn over the hero).</summary>
        public readonly int[] HeroAwakenDice = { 1, 1, 1 };
        public float HeroAwakenDiceAt = -10;

        private void KitDrop(Vector2 at, float delay, int kind) => awakenDrops.Add(new AwakenDrop { At = at, Born = Elapsed, LandAt = Elapsed + delay, Kind = kind });
        private UnitState KitNearest(Vector2 at, float range, UnitState not = null)
        {
            UnitState best = null; float d = range * range;
            foreach (UnitState u in StableUnits) { if (u == not || !CanHit(Faction.Champion, u)) continue; float e = (u.Position - at).sqrMagnitude; if (e < d) { d = e; best = u; } }
            return best;
        }

        private void KitExtras(HeroKit k, UnitState target)
        {
            if (Elapsed < extraAt) return;
            switch (k.Action)
            {
                case KitAction.Treant: // roots break through under the nearest foe and lash it
                {
                    extraAt = Elapsed + 1.4f; UnitState u = KitNearest(Champion.Position, 5.5f); if (u == null) break;
                    Hit(MorphGeometry.Disc, u.Position, u.Position, .75f, .6f, .35f); KitDrop(u.Position, .35f, DropRootSpike);
                    break;
                }
                case KitAction.Spring: // a bloom puffs pollen over the foe nearest a sprout (or the hero)
                {
                    extraAt = Elapsed + .9f; UnitState u = KitNearest(Champion.Position, 5f); if (u == null) break;
                    Hit(MorphGeometry.Disc, u.Position, u.Position, .7f, .32f, .25f, slow: .3f); KitDrop(u.Position, .25f, DropPollen);
                    break;
                }
                case KitAction.Mirrors: // the mirrors gather light and throw it as one beam at the foe in front
                {
                    extraAt = Elapsed + 2.2f; if (target == null) break;
                    Vector2 dir = (target.Position - Champion.Position).normalized;
                    Hit(MorphGeometry.Line, Champion.Position + dir * .7f, Champion.Position + dir * 6.5f, 0, .9f, .2f); KitDrop(Champion.Position + dir * .7f, .2f, DropMirrorBeam);
                    break;
                }
                case KitAction.Dreams: // where a dream walks, a sleepy cloud slows what wanders in
                {
                    extraAt = Elapsed + 1.6f; Vector2 at = awakenEchoes[(int)(Elapsed * 3) % 3];
                    Hit(MorphGeometry.Disc, at, at, 1.1f, .15f, .2f, DamageElement.Arcane, slow: .6f); KitDrop(at, .2f, DropSleep);
                    break;
                }
                case KitAction.KiteFleet: // a kite dives on the foe nearest one of its friends
                {
                    extraAt = Elapsed + 1.3f; UnitState f = awakenFleet.Count > 0 ? awakenFleet[(int)(Elapsed * 7) % awakenFleet.Count] : Champion;
                    UnitState u = KitNearest(f != null ? f.Position : Champion.Position, 6f); if (u == null) break;
                    Hit(MorphGeometry.Disc, u.Position, u.Position, .65f, .55f, .45f); KitDrop(u.Position, .45f, DropKiteDive);
                    break;
                }
                case KitAction.Parade: // every four seconds the whole knighted brigade swings at once
                {
                    extraAt = Elapsed + 4f;
                    foreach (UnitState f in StableUnits)
                    {
                        if (!f.Alive || f.Faction != Faction.Companion) continue;
                        UnitState u = KitNearest(f.Position, 3.5f); if (u == null) continue;
                        Hit(MorphGeometry.Crescent, f.Position, u.Position, 1.1f, .3f, .1f);
                    }
                    break;
                }
                case KitAction.Kintsugi: // a gold seam stitches the two foes nearest each other, then bursts
                {
                    extraAt = Elapsed + 1.2f; UnitState a = KitNearest(Champion.Position, 5f); UnitState b = a != null ? KitNearest(a.Position, 3f, a) : null;
                    if (a != null && b != null) Hit(MorphGeometry.Line, a.Position, b.Position, 0, .55f, .45f, DamageElement.Ember);
                    break;
                }
                case KitAction.Citadel: // foes that press against the petal walls are cut by them
                {
                    extraAt = Elapsed + .6f; int cut = 0;
                    foreach (UnitState u in StableUnits)
                    {
                        if (cut >= 3 || !CanHit(Faction.Champion, u)) continue;
                        float d = (u.Position - Champion.Position).magnitude; if (d < 1.1f || d > 2.1f) continue;
                        Vector2 across = Vector2.Perpendicular((u.Position - Champion.Position).normalized) * .45f;
                        Hit(MorphGeometry.Line, u.Position - across, u.Position + across, 0, .22f, .05f); cut++;
                    }
                    break;
                }
            }
        }
    }
}
