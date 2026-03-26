using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The Bizarre Arsenal in real simulation fixtures, without art: every weapon does what Docs/Bizarre_Arsenal.md says,
    /// and an enemy standing right behind the wielder is never struck (nothing radiates from the wielder's body).</summary>
    public static class ArsenalChecks
    {
        private static readonly List<string> results = new List<string>();
        private static void Require(bool condition, string name) { if (!condition) throw new InvalidOperationException("Arsenal: " + name); results.Add("PASS " + name); }

        [MenuItem("Arcane Brigade/Arsenal/Run checks")]
        public static void Run()
        {
            results.Clear();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Require(ArsenalSpecs.All.Length == 60 && ArsenalSpecs.All.Select(s => s.Id).Distinct().Count() == 60, "60 distinct weapons (47 plus the 13 of the content expansion)");
            Require(ArsenalSpecs.All.All(s => catalog.Weapon(s.Id) == null || catalog.Weapon(s.Id).Sprite != null), "a weapon is only in the catalog with its art");
            Loadouts(catalog);
            InkMoments();
            OwnLooks(catalog);
            foreach (ArsenalSpecs.Spec spec in ArsenalSpecs.All)
                using (var f = new Fixture(catalog, spec)) Weapon(f, spec);
            Directory.CreateDirectory("PreviewExports/Arsenal");
            File.WriteAllLines("PreviewExports/Arsenal/Checks.txt", results);
            Debug.Log("ARSENAL_CHECKS_PASSED " + results.Count);
        }

        private static void Loadouts(RunCatalog catalog)
        {
            var now = catalog.Champions.Select(h => HeroLoadout.Weapon(catalog, h.Id)).ToList();
            Require(now.All(id => id != null) && now.Distinct().Count() == catalog.Champions.Length, "every hero carries a different weapon now");
            Require(HeroLoadout.Heroes.Values.Select(e => e.Weapon).Distinct().Count() == HeroLoadout.Heroes.Count, "every hero's own weapon is different");
            Require(catalog.Champions.All(h => h.Weapon != null && h.Weapon.Id == HeroLoadout.Weapon(catalog, h.Id)), "hero assets carry their loadout weapon");
            Require(HeroLoadout.Heroes.Values.Select(e => e.Friend).Distinct().Count() == HeroLoadout.Heroes.Count && HeroLoadout.Heroes.Values.Select(e => e.FriendWeapon).Distinct().Count() == HeroLoadout.Heroes.Count, "starting friends and their weapons all differ");
            foreach (UnitDefinition hero in catalog.Champions)
            {
                RosterEntry start = K11HeroRules.Starter(catalog, hero);
                HeroLoadout.Entry plan = HeroLoadout.Heroes[hero.Id];
                Require(start.UnitId == plan.Friend && start.WeaponId == plan.FriendWeapon, "starting friend " + hero.Id + " = " + plan.Friend + " with " + plan.FriendWeapon);
                Require(plan.FriendWeapon != HeroLoadout.Weapon(catalog, hero.Id), "friend's weapon differs from the hero's " + hero.Id);
            }
        }

        /// <summary>Every effect is painted ink (ArsenalInk): each moment draws something layered all through its life and
        /// nothing once it is over; the room pieces (pools, cords, spirals, stripes, wakes) draw too.</summary>
        private static void InkMoments()
        {
            var host = new GameObject("ArsenalInkCheck");
            try
            {
                var ink = new ArsenalInk(host.transform);
                foreach (ArsenalInk.Moment m in Enum.GetValues(typeof(ArsenalInk.Moment)))
                {
                    int least = int.MaxValue;
                    foreach (float p in new[] { 0.05f, 0.3f, 0.6f, 0.9f })
                    {
                        ink.Begin(); ink.Draw(m, Vector2.zero, Vector2.right, p, 1f, 17, Texture2DSprite()); least = Mathf.Min(least, ink.Primitives); ink.End();
                    }
                    Require(least >= 3, "ink moment " + m + " is layered all through its life (" + least + " pieces at least)");
                }
                ink.Begin();
                ink.Pool(Vector2.zero, 0.8f, 1, ArsenalInk.Green, 3); int pool = ink.Primitives;
                ink.Spiral(Vector2.zero, 0.45f, 0.5f, 1, 3); ink.Coil(Vector2.zero, Vector2.one); ink.Cord(Vector2.zero, Vector2.right, ArsenalInk.Wine, 0.06f, false);
                ink.Stripes(Vector2.left, Vector2.right, 0.25f, 1); ink.Wake(Vector2.zero, Vector2.right, ArsenalInk.Cyan, 1, 1, false); ink.Orbit(Vector2.zero, 0.26f, 1, InkToken.Star, ArsenalInk.Gold, 3);
                Require(pool >= 5 && ink.Primitives > pool + 10, "ink room pieces draw (pool " + pool + ", all " + ink.Primitives + ")");
                ink.End(); ink.Destroy();
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
        }
        /// <summary>No weapon borrows another's look or sound (18/10/2026): every melee weapon has its own strike, every weapon
        /// its own hit palette/token, its own (cue, pitch) voice and, if ranged, its own requested shot painting.</summary>
        private static void OwnLooks(RunCatalog catalog)
        {
            var weapons = catalog.Weapons.Where(w => w != null).GroupBy(w => w.Id).Select(g => g.First()).ToList();
            foreach (WeaponDefinition w in weapons.Where(w => w.Kind == WeaponKind.Melee))
                Require(WeaponInkStyle.Strikes.ContainsKey(w.Id), "melee weapon has its own strike " + w.Id);
            var strikes = WeaponInkStyle.Strikes.Values.Select(v => v.Form + "/" + v.Palette + "/" + v.Token).ToList();
            Require(strikes.Distinct().Count() == strikes.Count, "no two melee weapons share a strike (" + strikes.Count + ")");
            var looks = weapons.Select(w => { var l = WeaponInkStyle.Look(w); return (w.Kind == WeaponKind.Melee ? WeaponInkStyle.Strikes[w.Id].Form.ToString() : "shot") + "/" + l.paletteName + "/" + l.token; }).ToList();
            Require(looks.Distinct().Count() == looks.Count, "no two weapons share a hit look (" + looks.Count + ")");
            var voices = weapons.Select(w => ArcaneCueLibrary.WeaponFireCue(w) + "@" + ArcaneCueLibrary.WeaponPitch(new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Companion, 0, Vector2.right, w)).ToString("0.000")).ToList();
            Require(voices.Distinct().Count() == voices.Count, "no two weapons share a voice (" + voices.Count + " weapons, " + voices.Select(v => v.Split('@')[0]).Distinct().Count() + " cues)");
            string worklist = File.ReadAllText("Docs/Bizarre_Arsenal_Art_Worklist.json");
            // thrown weapons fly their own painted object instead of a Shot_ painting (each one is unique to its weapon)
            var thrown = new Dictionary<string, string> { ["PaperLanternBomb"] = "LanternBombShot", ["SnowGlobeGrenade"] = "SnowGlobeShot", ["PinataBomb"] = "PinataShot", ["AcornCluster"] = "AcornBomb", ["FortuneGlobe"] = "GlobeMote",
                // content expansion: their own pieces, or the weapon itself thrown/held out (pins, rolling pin, can, lens)
                ["KiteSpool"] = "KiteShot", ["ScoopLauncher"] = "Scoop", ["CuckooCannon"] = "PendulumShot", ["SonarHarpoon"] = "HarpoonShot", ["YoYo"] = "YoyoSpin",
                ["BobaStraw"] = "BobaPearl", ["PaperPlaneSquadron"] = "PaperPlane", ["SnowShovel"] = "SnowClump", ["SpinningTop"] = "SpinTop",
                ["JugglingPins"] = "JugglingPins_Held", ["RollingPin"] = "RollingPin_Held", ["WateringCan"] = "WateringCan_Held", ["SunLens"] = "SunLens_Held" };
            foreach (WeaponDefinition w in weapons.Where(w => w.Kind == WeaponKind.Projectile))
                Require(thrown.TryGetValue(w.Id, out string own) ? ArsenalArtwork.Frames(own) != null : worklist.Contains("\"Shot_" + w.Id + "\""), "ranged weapon has its own shot painting " + w.Id);
            Require(thrown.Values.Distinct().Count() == thrown.Count, "every thrown weapon flies its own object");
        }
        private static Sprite heldStandIn;
        private static Sprite Texture2DSprite() => heldStandIn != null ? heldStandIn : heldStandIn = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);

        private static void Weapon(Fixture f, ArsenalSpecs.Spec spec)
        {
            CombatSimulation sim = f.Simulation; UnitState me = f.Me; float w = spec.Damage;
            UnitState behind = f.Enemy(new Vector2(-1.5f, 0));
            string id = spec.Id;
            switch (spec.Arsenal)
            {
                case ArsenalWeapon.CherryFuseBomb:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0)), side = f.Enemy(new Vector2(3, 0.7f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.5f);
                    Require(t.Health == f.Full, id + " the bomb is still in the air");
                    f.Tick(1.0f);
                    Require(t.Health < f.Full && side.Health < f.Full && sim.ArsenalBursts >= 1, id + " bursts where it landed and hits beside the target");
                    break;
                }
                case ArsenalWeapon.PopperSticks:
                case ArsenalWeapon.PaperLanternBomb:
                case ArsenalWeapon.SnowGlobeGrenade:
                case ArsenalWeapon.PinataBomb:
                case ArsenalWeapon.AcornCluster:
                {
                    // a slow lob that lands by the target and bursts in an area
                    UnitState t = f.Enemy(new Vector2(3, 0)), side = f.Enemy(new Vector2(3.2f, 0.6f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.3f);
                    Require(t.Health == f.Full && sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Lob), id + " is still in the air");
                    f.Tick(2f);
                    Require(t.Health < f.Full && side.Health < f.Full, id + " lands and bursts in an area");
                    if (spec.Arsenal == ArsenalWeapon.SnowGlobeGrenade) Require(t.ChillUntil > sim.Elapsed - 1.6f, id + " the burst slows");
                    if (spec.Arsenal == ArsenalWeapon.PinataBomb) Require(sim.Contributions.Any(c => c.EffectId == "candy-bomblet") || f.Full - t.Health > w * 1.2f, id + " candy bomblets pop round it");
                    break;
                }
                case ArsenalWeapon.DreamRacket:
                {
                    var row = Enumerable.Range(0, 4).Select(i => f.Enemy(new Vector2(2.5f + i * 1.4f, i % 2 * 0.5f))).ToList();
                    sim.ArsenalAttackFixture(me, row[0]); f.Tick(2f);
                    Require(row.Count(e => e.Health < f.Full) >= 3, id + " the ball bounces through the group");
                    break;
                }
                case ArsenalWeapon.BambooVase:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0)), beyond = f.Enemy(new Vector2(4.6f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(1.2f);
                    Require(t.Health < f.Full && beyond.Health < f.Full, id + " bamboo springs up in a line past the target");
                    break;
                }
                case ArsenalWeapon.RoseBouquet:
                {
                    UnitState t = f.Enemy(new Vector2(4, 0)), path = f.Enemy(new Vector2(2, 0.45f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.6f);
                    Require(sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Petals), id + " petals left along the path");
                    f.Tick(0.8f);
                    Require(path.Health < f.Full, id + " the petal ribbon hurts what stands in it");
                    break;
                }
                case ArsenalWeapon.HomeRunBat:
                {
                    UnitState t = f.Enemy(new Vector2(1.2f, 0)), pin = f.Enemy(new Vector2(3.2f, 0));
                    UnitState clipped = f.Enemy(new Vector2(0.9f, 0.7f));
                    sim.ArsenalAttackFixture(me, t);
                    Require(clipped.Health < f.Full, id + " the swing clips everything in its arc");
                    f.Tick(0.6f);
                    Require(t.Position.x > 2.2f && pin.Health < f.Full, id + " the batted enemy flies into another");
                    break;
                }
                case ArsenalWeapon.CandyDrill:
                {
                    var row = Enumerable.Range(0, 5).Select(i => f.Enemy(new Vector2(1.6f + i * 0.8f, 0))).ToList();
                    sim.ArsenalAttackFixture(me, row[0]); f.Tick(0.9f);
                    Require(row.Take(4).All(e => e.Health < f.Full) && sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Drill), id + " pierces four, then drills in");
                    break;
                }
                case ArsenalWeapon.InkyOctopus:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(1.5f);
                    float mid = t.Health;
                    f.Tick(1.5f);
                    Require(mid < f.Full && t.Health < mid, id + " clings and keeps hurting");
                    f.Tick(1.5f);
                    Require(!sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Octopus), id + " comes home after 3 s");
                    break;
                }
                case ArsenalWeapon.GrabberClaw:
                {
                    UnitState t = f.Enemy(new Vector2(3.5f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.75f);
                    Require(t.Health < f.Full && t.Position.x < 2.4f, id + " reels the enemy in");
                    break;
                }
                case ArsenalWeapon.StarforgeClaymore:
                {
                    UnitState t = f.Enemy(new Vector2(1.6f, 0)), flank = f.Enemy(new Vector2(0.15f, 1.6f));
                    ProjectileState shot = sim.ProjectileFixtureShot(new Vector2(1.2f, 0.4f), Vector2.left * 0.5f);
                    me.PendingAttack = true; me.AttackDirection = Vector2.right; me.ImpactTime = sim.Elapsed + 1;
                    f.Tick(0.1f);
                    Require(!sim.Projectiles.Contains(shot), id + " the blade blocks a shot during its wind-up");
                    me.PendingAttack = false;
                    sim.ArsenalAttackFixture(me, t);
                    Require(t.Health < f.Full && flank.Health < f.Full, id + " sweeps 200 degrees in front");
                    break;
                }
                case ArsenalWeapon.DragonLanternGlaive:
                {
                    UnitState t = f.Enemy(new Vector2(1.4f, 0)), far = f.Enemy(new Vector2(2.9f, 0.1f));
                    sim.ArsenalAttackFixture(me, t);
                    Require(t.Health < f.Full && far.Health < f.Full, id + " every attack sweeps and thrusts through the line");
                    break;
                }
                case ArsenalWeapon.TwinMoonStaff:
                {
                    UnitState t = f.Enemy(new Vector2(3.6f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(3f);
                    Require(t.Health < f.Full - w * 0.6f && !sim.Projectiles.Any(p => p.Arsenal == ArsenalWeapon.TwinMoonStaff), id + " grinds at the end and comes back");
                    break;
                }
                case ArsenalWeapon.PlushShark:
                {
                    UnitState near = f.Enemy(new Vector2(1.6f, 0)), far = f.Enemy(new Vector2(5, 0));
                    sim.ArsenalAttackFixture(me, near); f.Tick(1.2f);
                    Require(far.Health < f.Full && near.Health < f.Full, id + " thrown at the farthest, chomping on the way");
                    f.Tick(2f);
                    Require(!sim.Projectiles.Any(p => p.Arsenal == ArsenalWeapon.PlushShark), id + " swims home");
                    break;
                }
                case ArsenalWeapon.DrumstickClub:
                {
                    UnitState t = f.Enemy(new Vector2(1.3f, 0)), ahead = f.Enemy(new Vector2(3.4f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.5f);
                    Require(t.Health < f.Full && ahead.Health < f.Full, id + " slam ahead and a shock running forward");
                    break;
                }
                case ArsenalWeapon.PaperShuriken:
                {
                    var group = Enumerable.Range(0, 5).Select(i => f.Enemy(new Vector2(3 + i % 3 * 0.9f, -1 + i * 0.5f))).ToList();
                    sim.ArsenalAttackFixture(me, group[0]); f.Tick(1.5f);
                    Require(group.Count(e => e.Health < f.Full) >= 3, id + " stars bounce between enemies");
                    break;
                }
                case ArsenalWeapon.SquirtTrio:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.6f);
                    Require(t.Health < f.Full && t.ChillUntil > sim.Elapsed, id + " soaks and slows");
                    break;
                }
                case ArsenalWeapon.ConfettiBlunderbuss:
                {
                    UnitState t = f.Enemy(new Vector2(1.6f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.5f);
                    Require(f.Full - t.Health > w * 0.26f * 2.5f, id + " several pellets land up close");
                    break;
                }
                case ArsenalWeapon.PlungerPopper:
                {
                    UnitState t = f.Enemy(new Vector2(3.2f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.5f);
                    float stuckAt = t.Position.x;
                    f.Tick(0.5f);
                    Require(f.Full - t.Health >= w * 1.1f && stuckAt < 3.2f, id + " pulls in, then pops");
                    break;
                }
                case ArsenalWeapon.SignpostSlammer:
                {
                    UnitState t = f.Enemy(new Vector2(1.3f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.4f);
                    Require(sim.ArsenalProps.Count(p => p.Kind == ArsenalPropKind.Sign) == 1 && t.ChillUntil > sim.Elapsed, id + " each swing plants a slowing sign");
                    sim.ArsenalAttackFixture(me, t);
                    Require(sim.ArsenalProps.Count(p => p.Kind == ArsenalPropKind.Sign) == 1, id + " only the newest sign stands");
                    break;
                }
                case ArsenalWeapon.DetonatorBox:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.4f);
                    Require(t.Health == f.Full && sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.ToyBomb), id + " the bomb is stuck on and ticking");
                    t.Position = new Vector2(4.2f, 1.2f); f.Tick(0.1f);
                    Require(sim.ArsenalProps.Where(p => p.Kind == ArsenalPropKind.ToyBomb).All(p => (p.Position - t.Position).sqrMagnitude < 0.01f), id + " the bomb sticks to the enemy as it moves");
                    f.Tick(0.7f);
                    Require(f.Full - t.Health >= w * 1.2f * 0.99f, id + " a second later it goes off");
                    break;
                }
                case ArsenalWeapon.FirecrackerPinwheel:
                {
                    UnitState t = f.Enemy(new Vector2(6.5f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.5f);
                    ArsenalProp wheel = sim.ArsenalProps.FirstOrDefault(p => p.Kind == ArsenalPropKind.Pinwheel);
                    Require(t.Health == f.Full && wheel != null && wheel.Position.x <= spec.Range + 0.01f, id + " the pinwheel lands within its throw and waits");
                    t.Position = wheel.Position + new Vector2(0.4f, 0); f.Tick(1f);
                    Require(t.Health < f.Full, id + " an enemy close by sets it off");
                    UnitState still = f.Enemy(new Vector2(3, 1));
                    sim.ArsenalAttackFixture(me, still); f.Tick(1.5f);
                    Require(still.Health < f.Full, id + " planted by a still target, it goes off");
                    break;
                }
                case ArsenalWeapon.GumballGatling:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    me.AttackStarted = sim.Elapsed; me.NextAttack = sim.Elapsed + spec.Cooldown;
                    for (int i = 0; i < 8; i++) { me.AttackStarted = sim.Elapsed; me.NextAttack = sim.Elapsed + spec.Cooldown; sim.ArsenalAttackFixture(me, t); }
                    Require(me.NextAttack - me.AttackStarted < spec.Cooldown * 0.75f, id + " a hot barrel fires sooner");
                    break;
                }
                case ArsenalWeapon.BalloonBuddy:
                {
                    var crowd = Enumerable.Range(0, 3).Select(i => f.Enemy(new Vector2(3.2f, -0.4f + i * 0.4f))).ToList();
                    sim.ArsenalAttackFixture(me, crowd[0]); f.Tick(3.4f);
                    Require(crowd.Count(e => e.Health < f.Full) >= 2, id + " floats into the crowd and pops");
                    break;
                }
                case ArsenalWeapon.GlitchToybox:
                {
                    UnitState t = f.Enemy(new Vector2(1.2f, 0));
                    var seen = new HashSet<string>();
                    for (int i = 0; i < 6; i++) { sim.ArsenalAttackFixture(me, t); if (me.Borrowed != null) seen.Add(me.Borrowed.Id); f.Tick(0.6f); }
                    Require(seen.Count >= 2 && seen.All(s => CombatSimulation.GlitchPool.Contains(s)) && me.BorrowNext != null, id + " borrows different pool weapons and shows the next");
                    break;
                }
                case ArsenalWeapon.SaucerBlaster:
                {
                    var row = Enumerable.Range(0, 4).Select(i => f.Enemy(new Vector2(1.8f + i * 1.1f, 0))).ToList();
                    sim.ArsenalAttackFixture(me, row[0]); f.Tick(1.2f);
                    Require(row.All(e => e.Health < f.Full), id + " the saucer pierces the line");
                    break;
                }
                case ArsenalWeapon.FanfareHorn:
                {
                    UnitState t = f.Enemy(new Vector2(2.2f, 0)), left = f.Enemy(new Vector2(2.4f, 0.9f)), right = f.Enemy(new Vector2(2.4f, -0.9f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.8f);
                    Require(t.Health < f.Full && left.Health < f.Full && right.Health < f.Full && f.Full - t.Health <= w * 0.45f * 1.01f, id + " the fan of fins strikes each foe once");
                    break;
                }
                case ArsenalWeapon.WindupWrench:
                {
                    UnitState t = f.Enemy(new Vector2(1.2f, 0)), next = f.Enemy(new Vector2(1.6f, 0.8f));
                    sim.ArsenalAttackFixture(me, t);
                    Require(sim.ArsenalProps.Count(p => p.Kind == ArsenalPropKind.Wound) == 1, id + " a hit winds the enemy up");
                    sim.ArsenalAttackFixture(me, t);
                    Require(sim.ArsenalProps.Count(p => p.Kind == ArsenalPropKind.Wound) == 1, id + " one key per enemy");
                    float before = next.Health; f.Tick(1.2f);
                    Require(next.Health < before && t.ChillUntil > sim.Elapsed - 0.5f, id + " the wound-up enemy spins into its neighbour");
                    break;
                }
                case ArsenalWeapon.CorkRifle:
                {
                    UnitState near = f.Enemy(new Vector2(2, 0)), far = f.Enemy(new Vector2(7.5f, 0.05f));
                    sim.ArsenalAttackFixture(me, near); f.Tick(0.8f);
                    Require(near.Health < f.Full && far.Health < f.Full, id + " a long shot through the whole line");
                    break;
                }
                case ArsenalWeapon.CatcherMitt:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    ProjectileState shot = sim.ProjectileFixtureShot(new Vector2(1.0f, 0.1f), Vector2.left * 0.5f);
                    f.Tick(0.1f);
                    Require(!sim.Projectiles.Contains(shot) && sim.ArsenalProps.Count(p => p.Kind == ArsenalPropKind.Catch) == 1, id + " catches a shot in front");
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.5f);
                    Require(f.Full - t.Health > w * 1.3f * 1.2f && sim.ArsenalProps.Count(p => p.Kind == ArsenalPropKind.Catch) == 0, id + " throws the catch back after the fastball");
                    break;
                }
                case ArsenalWeapon.FizzBottle:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.4f);
                    Require(t.Health == f.Full, id + " the bottle is still flying");
                    f.Tick(1.2f);
                    Require(t.Health < f.Full && t.ChillUntil > sim.Elapsed, id + " the fizzing puddle hurts and slows");
                    break;
                }
                case ArsenalWeapon.LeekBaton:
                {
                    UnitState t = f.Enemy(new Vector2(1.2f, 0)), side = f.Enemy(new Vector2(0.5f, 1.2f));
                    sim.ArsenalAttackFixture(me, t);
                    Require(t.Health < f.Full && side.Health < f.Full && t.Position.x > 1.2f && t.ChillUntil > sim.Elapsed, id + " a wide stinging swing that pushes");
                    break;
                }
                case ArsenalWeapon.DowsingRod:
                {
                    UnitState strong = f.Enemy(new Vector2(2, 0)), weak = f.Enemy(new Vector2(3.5f, 1.2f));
                    weak.Health = f.Full * 0.3f; float before = weak.Health;
                    sim.ArsenalAttackFixture(me, strong); f.Tick(0.9f);
                    Require(weak.Health < before && strong.Health == f.Full && weak.ExposedUntil > sim.Elapsed, id + " finds and exposes the weakest enemy");
                    break;
                }
                case ArsenalWeapon.StarKnuckle:
                {
                    UnitState t = f.Enemy(new Vector2(1, 0)), beyond = f.Enemy(new Vector2(2.6f, 0));
                    sim.ArsenalAttackFixture(me, t); float jab = f.Full - t.Health; f.Tick(0.5f);
                    Require(jab > 0 && beyond.Health < f.Full, id + " every jab flicks a star shard forward");
                    break;
                }
                case ArsenalWeapon.BuzzerBlock:
                {
                    UnitState t = f.Enemy(new Vector2(5, 0)), a = f.Enemy(new Vector2(5.2f, 1.5f)), b = f.Enemy(new Vector2(6, 2.6f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.5f);
                    Require(sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Buzzer) && t.Health == f.Full, id + " the buzzer waits on the floor");
                    Vector2 at = sim.ArsenalProps.First(p => p.Kind == ArsenalPropKind.Buzzer).Position;
                    t.Position = at; a.Position = at + new Vector2(0.6f, 1.4f); b.Position = at + new Vector2(1.4f, 2.4f); f.Tick(0.2f);
                    Require(t.Health < f.Full && a.Health < f.Full && b.Health < f.Full, id + " stepping on it shocks a chain");
                    break;
                }
                case ArsenalWeapon.FlySwatter:
                {
                    UnitState t = f.Enemy(new Vector2(1.1f, 0));
                    sim.ArsenalAttackFixture(me, t); float one = f.Full - t.Health;
                    t.Health = f.Full * 0.2f; float low = t.Health;
                    sim.ArsenalAttackFixture(me, t);
                    Require(one > 0 && t.ChillUntil > sim.Elapsed && low - t.Health >= one * 1.55f, id + " dazes, and every swat is harder on the weak");
                    break;
                }
                case ArsenalWeapon.PawMitten:
                {
                    UnitState t = f.Enemy(new Vector2(0.8f, 0)), reach = f.Enemy(new Vector2(1.4f, 0.1f)), beyond = f.Enemy(new Vector2(2.4f, 0));
                    sim.ArsenalAttackFixture(me, t);
                    Require(t.Health < f.Full && reach.Health < f.Full && beyond.Health == f.Full, id + " every swipe drags claw lines a little further, no further");
                    break;
                }
                case ArsenalWeapon.HearthBrick:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.3f);
                    Require(t.Health == f.Full, id + " the brick is still in the air");
                    f.Tick(0.5f);
                    Require(t.Health < f.Full && t.ChillUntil > sim.Elapsed && t.Position.x > 3, id + " it thuds down, dazing and shoving");
                    break;
                }
                case ArsenalWeapon.VerdictGavel:
                {
                    UnitState t = f.Enemy(new Vector2(1, 0)), beyond = f.Enemy(new Vector2(2.6f, 0));
                    sim.ArsenalAttackFixture(me, t); float plain = f.Full - t.Health;
                    Require(plain > 0 && beyond.Health == f.Full, id + " the verdict lands on the enemy in front only");
                    f.Arena.Luck = 2; float before = t.Health;
                    sim.ArsenalAttackFixture(me, t);
                    Require(before - t.Health >= plain * 1.49f, id + " luck makes the verdict heavier");
                    f.Arena.Luck = 0;
                    break;
                }
                case ArsenalWeapon.TalismanIngot:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0)), side = f.Enemy(new Vector2(3, 0.5f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.6f);
                    Require(t.Health < f.Full && side.Health < f.Full, id + " the ingot pops in a small blast");
                    break;
                }
                case ArsenalWeapon.KunaiFan:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0)), next = f.Enemy(new Vector2(4, 0.05f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.05f);
                    Require(sim.Projectiles.Count(p => p.Arsenal == ArsenalWeapon.KunaiFan) == 1, id + " the kunai leave one at a time");
                    f.Tick(0.8f);
                    Require(f.Full - t.Health >= w * 0.6f * 3 * 0.99f && next.Health < f.Full, id + " all three fly down the line, each through one enemy");
                    break;
                }
                case ArsenalWeapon.FortuneGlobe:
                {
                    UnitState t = f.Enemy(new Vector2(4, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.05f);
                    var motes = sim.Projectiles.Where(p => p.Arsenal == ArsenalWeapon.FortuneGlobe).ToList();
                    float start = motes.Count == 3 ? motes[0].Radius : 0, damage0 = motes.Count == 3 ? motes[0].Damage : 0;
                    Require(motes.Count == 3, id + " three crystal motes wander out");
                    f.Tick(0.6f);
                    Require(motes.Any(m => sim.Projectiles.Contains(m) && m.Radius > start + 0.05f && m.Damage > damage0 * 1.2f) || t.Health < f.Full, id + " the motes grow as they fly");
                    f.Tick(1f);
                    Require(t.Health < f.Full, id + " the motes reach the target");
                    break;
                }
                case ArsenalWeapon.TopiaryShears:
                {
                    UnitState t = f.Enemy(new Vector2(1.2f, 0)), tough = f.Enemy(new Vector2(1.3f, 0.9f));
                    tough.Health = f.Full; t.Health = f.Full * 0.5f;
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.2f);
                    Require(t.Health < f.Full * 0.5f && tough.Health < f.Full, id + " one snip down the line, the next at the toughest enemy");
                    break;
                }
                // content expansion
                case ArsenalWeapon.KiteSpool:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(1f);
                    Require(sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Kite && !p.Resolved), id + " the kite loops ahead on its string");
                    f.Tick(2f);
                    Require(t.Health < f.Full && !sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Kite && !p.Resolved), id + " dives at the end of its flight");
                    break;
                }
                case ArsenalWeapon.ScoopLauncher:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.3f);
                    Require(t.Health == f.Full, id + " the scoop is still in the air");
                    f.Tick(1f);
                    Require(t.Health < f.Full && t.ChillUntil > sim.Elapsed, id + " splats on its enemy and slows it (no puddle)");
                    f.Tick(0.6f);
                    Require(!sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Scoop), id + " leaves nothing on the floor");
                    break;
                }
                case ArsenalWeapon.CuckooCannon:
                {
                    UnitState t = f.Enemy(new Vector2(2.5f, 0)), far = f.Enemy(new Vector2(5.4f, 0.3f));
                    for (int i = 0; i < 3; i++) { sim.ArsenalAttackFixture(me, t); f.Tick(0.1f); }
                    Require(sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Cuckoo), id + " every third shot sends the cuckoo");
                    f.Tick(1f);
                    Require(t.Health < f.Full && far.Health < f.Full, id + " the pendulum strikes and the cuckoo dives on the farthest");
                    break;
                }
                case ArsenalWeapon.JugglingPins:
                {
                    UnitState t = f.Enemy(new Vector2(2.2f, 0)), other = f.Enemy(new Vector2(3f, 1f));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.9f);
                    Require(t.Health < f.Full && other.Health == f.Full, id + " the pin lands on its enemy");
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.9f);
                    Require(other.Health < f.Full, id + " the next pin picks another enemy");
                    break;
                }
                case ArsenalWeapon.SonarHarpoon:
                {
                    UnitState t = f.Enemy(new Vector2(2, 0)), next = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.6f);
                    Require(t.Health < f.Full && next.Health < f.Full && t.ProbedUntil > sim.Elapsed, id + " pierces two and probes them");
                    Require(sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Buoy), id + " leaves a sonar buoy");
                    break;
                }
                case ArsenalWeapon.YoYo:
                {
                    UnitState t = f.Enemy(new Vector2(2.5f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(1.4f);
                    Require(f.Full - t.Health > w * 0.7f * 1.5f, id + " strikes on the way out and on the way home");
                    break;
                }
                case ArsenalWeapon.RollingPin:
                {
                    UnitState a = f.Enemy(new Vector2(1.5f, 0)), b = f.Enemy(new Vector2(2.6f, 0));
                    sim.ArsenalAttackFixture(me, a); f.Tick(1f);
                    Require(a.Health < f.Full && b.Health < f.Full && sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Dough), id + " rolls through the line and leaves dough");
                    break;
                }
                case ArsenalWeapon.BobaStraw:
                {
                    UnitState t = f.Enemy(new Vector2(2.2f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.8f);
                    Require(t.Health < f.Full, id + " the swaying pearls reach the enemy");
                    break;
                }
                case ArsenalWeapon.PaperPlaneSquadron:
                {
                    var three = new[] { f.Enemy(new Vector2(2, 1)), f.Enemy(new Vector2(2, -1)), f.Enemy(new Vector2(3, 0)) };
                    sim.ArsenalAttackFixture(me, three[2]); f.Tick(1.5f);
                    Require(three.All(e => e.Health < f.Full), id + " three planes chase three enemies");
                    break;
                }
                case ArsenalWeapon.WateringCan:
                {
                    UnitState t = f.Enemy(new Vector2(1.5f, 0));
                    sim.ArsenalAttackFixture(me, t);
                    Require(t.Health < f.Full && t.KitWetUntil > sim.Elapsed, id + " waters and soaks the enemy ahead");
                    break;
                }
                case ArsenalWeapon.SnowShovel:
                {
                    UnitState t = f.Enemy(new Vector2(2.5f, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(0.2f);
                    Require(t.Health == f.Full && sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Lob), id + " the snow is still in the air");
                    f.Tick(1f);
                    Require(t.Health < f.Full && t.ChillUntil > sim.Elapsed, id + " lands and slows");
                    break;
                }
                case ArsenalWeapon.SunLens:
                {
                    UnitState t = f.Enemy(new Vector2(3, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(2f);
                    Require(t.Health < f.Full && t.BurnUntil > sim.Elapsed + 1.5f, id + " the sunspot burns, then sets its enemy alight");
                    break;
                }
                case ArsenalWeapon.SpinningTop:
                {
                    UnitState t = f.Enemy(new Vector2(2, 0));
                    sim.ArsenalAttackFixture(me, t); f.Tick(1f);
                    Require(t.Health < f.Full && t.Position.x > 2.05f, id + " spins where it lands, grinding and flinging the enemy out");
                    break;
                }
                case ArsenalWeapon.BareKnuckles:
                {
                    UnitState t = f.Enemy(new Vector2(0.8f, 0)), ahead = f.Enemy(new Vector2(2.0f, 0));
                    sim.ArsenalAttackFixture(me, t);
                    Require(t.Health < f.Full && ahead.Health == f.Full, id + " punches reach only up close");
                    sim.ArsenalAttackFixture(me, t, true);
                    Require(ahead.Health < f.Full, id + " a critical punch pops the air ahead");
                    break;
                }
            }
            Require(behind.Health == f.Full, id + " never strikes the enemy right behind the wielder");
        }

        private sealed class Fixture : IDisposable
        {
            private readonly RunSession session;
            private readonly RunCatalog catalog;
            private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
            public readonly CombatSimulation Simulation;
            public readonly UnitState Me;
            public readonly float Full = 10000;
            public readonly CheckpointCatalog Arena;

            public Fixture(RunCatalog catalog, ArsenalSpecs.Spec spec)
            {
                this.catalog = catalog;
                session = new RunSession(catalog); session.Start("LanternKnight", 24017); session.State.Roster.Clear(); session.State.Wave = 2;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; Arena = arena;
                arena.EnableArenaToys = arena.EnableBossArenaPhase = arena.EnableSpectacles = arena.EnableHeroMechanics = false;
                arena.StoryCompanions = new UnitDefinition[0]; arena.CompanionLeash = 20;
                Simulation = new CombatSimulation(arena); Simulation.Start(24017);
                Simulation.Champion.NextAttack = float.PositiveInfinity;
                Simulation.Champion.Position = Simulation.Champion.PreviousPosition = new Vector2(-6, -3);
                var weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); owned.Add(weapon);
                weapon.Id = spec.Id; weapon.Arsenal = spec.Arsenal; weapon.Kind = spec.Kind; weapon.Motion = spec.Motion; weapon.Element = DamageElement.Physical;
                weapon.Damage = spec.Damage; weapon.Range = spec.Range; weapon.Cooldown = spec.Cooldown; weapon.Windup = spec.Windup; weapon.ConeDot = 0.2f;
                weapon.ProjectileSpeed = 8; weapon.ProjectileLifetime = 2.4f; weapon.MuzzleDistance = 0.4f; weapon.PushDistance = 0; weapon.ProjectileColor = Color.white;
                Me = Unit(weapon, Faction.Companion, Vector2.zero);
            }
            public UnitState Enemy(Vector2 at)
            {
                var weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); owned.Add(weapon); weapon.Kind = WeaponKind.Contact; weapon.Id = "fixture-contact";
                return Unit(weapon, Faction.Enemy, at);
            }
            private UnitState Unit(WeaponDefinition weapon, Faction faction, Vector2 at)
            {
                UnitDefinition definition = UnityEngine.Object.Instantiate(catalog.Companions[0]); owned.Add(definition);
                definition.Id = weapon.Id + "-arsenal-" + owned.Count; definition.Weapon = weapon;
                definition.Speed = definition.CollisionDamage = definition.CriticalChance = definition.KnockbackResistance = 0;
                definition.Health = Full; definition.Radius = 0.25f; definition.IsBoss = false; definition.CombatTier = 1;
                definition.Passive = PassiveKind.None; definition.Mutation = EnemyMutation.None; definition.Behavior = EnemyBehavior.Standard;
                UnitState unit = Simulation.AddFixture(definition, faction, at);
                unit.NextAttack = unit.NextContact = unit.NextBehavior = float.PositiveInfinity;
                unit.AttackDirection = Vector2.right; unit.Facing = Vector2.right;
                return unit;
            }
            public void Tick(float seconds) { for (float t = 0; t < seconds - 0.001f; t += 0.05f) Simulation.Step(0.05f, Vector2.zero); }
            public void Dispose() { session.Dispose(); foreach (UnityEngine.Object asset in owned) UnityEngine.Object.DestroyImmediate(asset); }
        }
    }
}
