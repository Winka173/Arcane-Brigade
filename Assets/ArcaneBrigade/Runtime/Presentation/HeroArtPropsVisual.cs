using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The painted objects of the transformations put to work (Docs/Hero_Effect_Art_Delivery.md, 24/10/2026): each
    /// sheet's frames follow what its object is doing - the crank turns with the gun, the barrels recoil out of step, the
    /// piston drives with the engine's speed, couplings stretch with the gap between carriages, citadel walls open a slit to
    /// shoot, pins tumble and balls squash, the hourglass tips as the debt comes due. Missing art leaves the old tokens.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private readonly List<UnitState> trainOrder = new List<UnitState>();
        private float mirrorReturnAt = -10;

        private SpriteRenderer Prop(string key, int frame, Vector2 at, float size, float angle = 0, bool flip = false, int order = 0, bool airborne = true)
        {
            Sprite s = HeroEffectArtLibrary.Frame(key, frame); if (s == null) return null;
            // size is the painted object's own size: the sheets keep a wide transparent margin round each object
            Rect ink = SpriteInkMetrics.BoundsOf(s); float cell = Mathf.Max(s.bounds.size.x, s.bounds.size.y), drawn = Mathf.Max(.05f, Mathf.Max(ink.width, ink.height));
            size *= cell / drawn;
            var r = Place(s, at, Vector2.one * size, still ? 0 : angle, new Color(1, 1, 1, strength), airborne);
            if (r != null) { r.flipX = flip; if (order != 0) r.sortingOrder = order; }
            return r;
        }
        private static bool HasArt(string key) => HeroEffectArtLibrary.Frames(key) != null;

        private void ArtProps()
        {
            UnitState hero = sim.Champion; HeroKit kit = sim.HeroKitInfo;
            Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float body = hero.Definition.BodyHeight, side = facing.x < -.05f ? -1 : 1, since = t - sim.HeroAwakenStarted;
            Vector2 feet = at - Vector2.up * body * .5f, head = at + Vector2.up * (body * .5f + .1f);
            Vector2 tip = UnitVisual.TryMuzzle(hero.Id, out Vector2 m) ? m : at + Vector2.up * body * .08f + facing * .5f;
            bool moving = (hero.Position - hero.PreviousPosition).sqrMagnitude > 1e-5f;
            int bodyOrder = 1000 - Mathf.RoundToInt(hero.Position.y * 12f);
            // the debt's hourglass over the stag, tipping as the debt comes due (any time, not only transformed)
            if (style.Id == "HourglassStag" && sim.HeroIdentityBankAge >= 0 && !sim.HeroAwakening)
            {
                float due = sim.HeroIdentityApproach == 0 ? 4 : sim.HeroIdentityApproach == 1 ? 3 : 5, k = Mathf.Clamp01(sim.HeroIdentityBankAge / due);
                Prop("HourglassStag_Hourglass", k < .55f ? 0 : k < .9f ? 1 : 2, head + Vector2.up * (.25f + Bob(1)), .34f + .06f * k);
            }
            if (!sim.HeroAwakening || kit == null) return;
            switch (kit.Action)
            {
                case KitAction.AcornGun: // the crank turns as the gun fires
                    Prop("TreasureAcorn_Crank", Mathf.FloorToInt(t * 14), tip - facing * .62f + Vector2.up * .04f, .44f, 0, side < 0);
                    break;
                case KitAction.Gatling: // the barrels recoil out of step
                {
                    Vector2 aim = sim.HeroAwakenAim.sqrMagnitude > .01f ? sim.HeroAwakenAim.normalized : facing;
                    Prop("Postmaster_Barrels", Mathf.FloorToInt(t * 15), tip - aim * .18f, .5f, Mathf.Atan2(aim.y, Mathf.Abs(aim.x)) * Mathf.Rad2Deg * side, side < 0);
                    break;
                }
                case KitAction.Engine: // the piston drives faster as the engine runs
                    Prop("CopperCauldron_PistonValve", Mathf.FloorToInt(t * (moving ? 12 : 5)), feet + new Vector2(side * (hero.Definition.Radius + .45f), body * .22f), .46f, 0, side < 0, bodyOrder + 1);
                    break;
                case KitAction.Train: // couplings between the carriages, slack, taut or stretched with the gap
                {
                    trainOrder.Clear();
                    foreach (UnitState f in sim.Units) if (f.Alive && f.Faction == Faction.Companion) trainOrder.Add(f);
                    trainOrder.Sort((a, b) => Vector2.Dot(a.Position - hero.Position, -facing).CompareTo(Vector2.Dot(b.Position - hero.Position, -facing)));
                    Vector2 prev = at;
                    foreach (UnitState f in trainOrder)
                    {
                        Vector2 here = DrawnAt(f), gap = here - prev; float d = gap.magnitude;
                        if (d > .2f && d < 2.4f)
                            Prop("CitrusMarshal_Coupling", d < .9f ? 0 : d < 1.5f ? 1 : 2, (prev + here) * .5f - Vector2.up * .2f, .42f, Mathf.Atan2(gap.y, Mathf.Abs(gap.x)) * Mathf.Rad2Deg * (gap.x < 0 ? -1 : 1), gap.x < 0);
                        prev = here;
                    }
                    break;
                }
                case KitAction.Citadel: // two layers of petal wall round the hero; a slit opens each time it shoots
                {
                    int f = since < .35f ? 2 : Mathf.Repeat(t, .5f) < .14f ? 1 : 0;
                    Prop("PetalCitadel_WallBack", f, at + Vector2.up * body * .25f, body * 2.2f, 0, side < 0, bodyOrder - 3, false);
                    Prop("PetalCitadel_WallFront", f, at - Vector2.up * body * .05f, body * 2f, 0, side < 0, bodyOrder + 3, false);
                    break;
                }
                case KitAction.Circus: // balls and pins juggled over the head: pins tumble, balls squash at the bottom of the throw
                    for (int i = 0; i < 4; i++)
                    {
                        float q = still ? i / 4f : Mathf.Repeat(t * 1.3f + i / 4f, 1);
                        Vector2 p = head + new Vector2(Mathf.Cos(q * Mathf.PI * 2) * .5f, .15f + Mathf.Abs(Mathf.Sin(q * Mathf.PI * 2)) * .6f);
                        if (i % 2 == 0) Prop("JugglingJester_Ball", Mathf.Abs(Mathf.Sin(q * Mathf.PI * 2)) < .12f ? 1 : 0, p, .3f);
                        else Prop("JugglingJester_Pin", Mathf.FloorToInt(t * 8 + i), p, .42f, still ? 0 : q * 720);
                    }
                    break;
                case KitAction.TimeGarden: // the gear in the bonsai's branch ticks half a tooth
                    Prop("BonsaiClocksmith_GearBranch", Mathf.FloorToInt(t * 4), sim.HeroAwakenPlot + new Vector2(.42f, .75f), .42f);
                    break;
                case KitAction.Kitchen: // the pot lid rattles up with each course
                    Prop("SupperMortar_PotLid", Mathf.Repeat(t, .25f) < .1f ? 1 : 0, at + Vector2.up * (body * .5f + .55f + .38f + Bob(1)), .38f);
                    break;
                case KitAction.Foreclosure: // the great hourglass tips over as time stops
                    Prop("HourglassStag_Hourglass", since < .45f ? 0 : since < 1f ? 1 : 2, head + Vector2.up * (.3f + Bob(1) * 2), .55f);
                    break;
            }
        }
    }
}
