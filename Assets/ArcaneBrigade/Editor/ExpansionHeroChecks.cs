using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Content expansion step 4c: the five new heroes (Kite Admiral, Sundae Duchess, Bonsai Clocksmith, Juggling Jester,
    /// Bell Diver) are in the roster with their whole kit. For each: the kit, weapon, starter, signature bond and item family; the
    /// gauge fills from the hero's own source (fed real combat events); the transformation fires and does its thing; the hero and
    /// team passives apply (Docs/Content_Expansion_v5.md §2–§5, §8).</summary>
    public static class ExpansionHeroChecks
    {
        public static readonly string[] Ids = { "KiteAdmiral", "SundaeDuchess", "BonsaiClocksmith", "JugglingJester", "BellDiver" };
        private static readonly Dictionary<string, string> SignatureBond = new Dictionary<string, string>
        { ["KiteAdmiral"] = "paper-wings", ["SundaeDuchess"] = "sweet-tooth", ["BonsaiClocksmith"] = "gear", ["JugglingJester"] = "big-top", ["BellDiver"] = "rain-script" };
        private static readonly Dictionary<string, string> Family = new Dictionary<string, string>
        { ["KiteAdmiral"] = "fam-ranged", ["SundaeDuchess"] = "fam-freeze", ["BonsaiClocksmith"] = "fam-merge", ["JugglingJester"] = "fam-crit", ["BellDiver"] = "fam-marks" };
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static readonly List<string> notes = new List<string>();

        private static void Require(bool ok, string caption)
        {
            notes.Add((ok ? "PASS " : "FAIL ") + caption);
            Directory.CreateDirectory("PreviewExports/ExpansionHeroes"); File.WriteAllLines("PreviewExports/ExpansionHeroes/Checks.txt", notes);
            if (!ok) throw new InvalidOperationException("ExpansionHeroes " + caption);
        }
        private static object Call(object target, string method, params object[] args)
        {
            MethodInfo m = target.GetType().GetMethods(Flags).First(x => x.Name == method && x.GetParameters().Length == args.Length);
            return m.Invoke(target, args);
        }
        private static T Get<T>(object target, string field) => (T)target.GetType().GetField(field, Flags).GetValue(target);
        private static float Pending(CombatSimulation sim) => Get<float>(sim, "gaugePending");
        /// <summary>Feed one combat event to the hero's observers and return what it added to the gauge.</summary>
        private static float Feed(CombatSimulation sim, CombatEvent e)
        {
            var events = Get<List<CombatEvent>>(sim, "events"); events.Clear();
            typeof(CombatSimulation).GetField("gaugePending", Flags).SetValue(sim, 0f);
            events.Add(e); Call(sim, "ObserveHeroIdentityEvents");
            float added = Pending(sim); events.Clear(); return added;
        }

        private sealed class Fight : IDisposable
        {
            public RunSession Run; public CheckpointCatalog Arena; public CombatSimulation Sim; public UnitDefinition Dummy;
            public readonly List<UnitState> Foes = new List<UnitState>();
            public Fight(RunCatalog catalog, string hero, int seed, int approach = 0)
            {
                Run = new RunSession(catalog); Run.Start(hero, seed); Run.State.Wave = 8; Run.State.HeroIdentity.Approach = approach;
                Arena = Run.PrepareCombat(); Arena.EnemyBudget = 0; Arena.WaveDuration = 90; Arena.Boss = null; Arena.EnableArenaToys = Arena.EnableSpectacles = false; Arena.EnableHeroIdentities = true;
                Sim = new CombatSimulation(Arena); Sim.Start(seed);
                Dummy = UnityEngine.Object.Instantiate(Arena.MeleeEnemy); Dummy.Weapon = UnityEngine.Object.Instantiate(Dummy.Weapon);
                Dummy.Speed = 0; Dummy.Health = 100000; Dummy.ContactDamage = 0; Dummy.CollisionDamage = 0; Dummy.Behavior = EnemyBehavior.Standard; Dummy.SpecialEnemy = SpecialEnemyKind.None; Dummy.IsBoss = false;
            }
            public UnitState Foe(Vector2 offset)
            {
                UnitState u = Sim.AddFixture(Dummy, Faction.Enemy, Sim.Champion.Position + offset); u.NextAttack = u.NextContact = float.PositiveInfinity; Foes.Add(u); return u;
            }
            public void Step(float seconds, Vector2 move = default)
            {
                for (float s = 0; s < seconds; s += .05f) { Sim.Champion.Health = Sim.Champion.Definition.Health; Sim.Step(.05f, move); }
            }
            /// <summary>Gauge full: steps until the hero has transformed by itself.</summary>
            public void Transform()
            {
                Sim.PrimeAwakeningForCapture();
                for (int i = 0; i < 80 && !Sim.HeroAwakening; i++) Step(.05f);
            }
            public CombatCause Personal => new CombatCause(910000 + Foes.Count, Sim.Champion.Id);
            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Dummy.Weapon); UnityEngine.Object.DestroyImmediate(Dummy); Run.Dispose();
            }
        }

        [MenuItem("Arcane Brigade/Checks/Expansion heroes")]
        public static void Run()
        {
            notes.Clear();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            foreach (string id in Ids) Kit(catalog, id);
            Kite(catalog); Sundae(catalog); Bonsai(catalog); Jester(catalog); Bell(catalog);
            Directory.CreateDirectory("PreviewExports/ExpansionHeroes"); File.WriteAllLines("PreviewExports/ExpansionHeroes/Checks.txt", notes);
            Debug.Log("EXPANSION_HERO_CHECKS_PASSED " + notes.Count);
        }

        /// <summary>Roster, kit, weapon, starter, signature bond, item family, copy, style, balance.</summary>
        private static void Kit(RunCatalog catalog, string id)
        {
            UnitDefinition hero = catalog.Champions.FirstOrDefault(u => u.Id == id);
            Require(hero != null && hero.Body != null && hero.AnimationFrames != null && hero.AnimationFrames.Length == 4 && hero.AuthoredMotion != null, id + " is in the roster with its painted poses");
            HeroKit kit = HeroKits.For(id);
            Require(kit != null && kit.HeroTraits.Any() && kit.TeamTraits.Any(t => t.Kind == KitPassive.SignatureBondLevel), id + " has a kit and keeps +1 signature bond level");
            Require(kit.Traits.All(t => t.Text.Length <= 80) && kit.Effect.Length <= 160, id + " copy is short");
            Require(ArsenalArtwork.Frame(kit.Held ? kit.ArtKey + "_Held" : kit.ArtKey, 0) != null, id + " transformation art " + kit.ArtKey + (kit.Held ? "_Held" : ""));
            Require(HeroLoadout.Heroes.TryGetValue(id, out var plan) && hero.Weapon != null && hero.Weapon.Id == plan.Weapon, id + " carries " + plan.Weapon);
            RosterEntry start = K11HeroRules.Starter(catalog, hero);
            Require(start.UnitId == plan.Friend && start.WeaponId == plan.FriendWeapon && catalog.Unit(start.UnitId).Rarity < ItemRarity.Mythic, id + " starts with " + plan.Friend + " / " + plan.FriendWeapon);
            Require(hero.RecruitThemes.Contains(SignatureBond[id]) && hero.RecruitThemes.Contains(Family[id]) && hero.Tags.Length == 2, id + " pool: two themes, " + SignatureBond[id] + ", " + Family[id]);
            var pool = catalog.Companions.Where(u => u.Body != null && K11HeroRules.Eligible(hero, u)).ToList();
            Require(pool.Count(u => u.Tags.Contains(SignatureBond[id])) >= 2, id + " recruits its signature bond's members (" + pool.Count + " friends in pool)");
            var identity = HeroIdentities.For(id);
            Require(identity != null && identity.Approaches.Length == 3 && identity.ApproachDescriptions.Length == 3 && identity.ApproachDescriptions.All(d => d.Length > 60), id + " identity and three approaches");
            Require(HeroMechanicStyle.All.Any(s => s.Id == id) && WaitingArrivals.For(id).Length > 40, id + " ink style and arrival line");
            string copy = RunUICopy.ChampionDescription(hero);
            Require(copy.Contains(kit.Transformation) && copy.Contains("INNATE TRAITS") && !copy.Contains("press ") && !copy.Contains("click"), id + " description shows the kit, no input hints");
            Require(PlayerBuildBalance.HeroPower(id) <= 5 && PlayerBuildBalance.HeroVitality(id) <= 1.5f, id + " power and vitality in the roster's band");
            using (var fight = new Fight(catalog, id, 41900))
                Require(fight.Sim.HeroAwakenEnabled && fight.Sim.HeroAwakenMeter == 0, id + " starts a fight with an empty gauge");
        }

        // ------------------------------------------------------------------ the five heroes
        private static void Kite(RunCatalog catalog)
        {
            using var f = new Fight(catalog, "KiteAdmiral", 41901);
            UnitState far = f.Foe(new Vector2(5, 0)), near = f.Foe(new Vector2(1.2f, 0));
            float farFill = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, far.Position, Faction.Enemy, 10, Vector2.right, subject: far, cause: f.Personal));
            float nearFill = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, near.Position, Faction.Enemy, 10, Vector2.right, subject: near, cause: f.Personal));
            Require(farFill > nearFill * 3 && nearFill > 0, "KiteAdmiral: own hits beyond 4 units fill the gauge fastest (" + farFill + " vs " + nearFill + ")");
            float farDamage = (float)Call(f.Sim, "KitDamageDealt", far, 100f, Faction.Champion, f.Personal), nearDamage = (float)Call(f.Sim, "KitDamageDealt", near, 100f, Faction.Champion, f.Personal);
            Require(Mathf.Abs(farDamage - 115) < .01f && Mathf.Abs(nearDamage - 100) < .01f, "KiteAdmiral: hits beyond 4 units deal +15% (" + farDamage + ")");
            // ranged friends +10% range
            UnitDefinition pigeon = UnityEngine.Object.Instantiate(catalog.Unit("PaperPlanePigeon")); pigeon.Weapon = UnityEngine.Object.Instantiate(catalog.Weapon("PaperPlaneSquadron"));
            float range = pigeon.Weapon.Range; Call(f.Run, "KitCompileUnit", pigeon, catalog.Unit("PaperPlanePigeon"), true);
            Require(Mathf.Abs(pigeon.Weapon.Range - range * 1.1f) < .001f, "KiteAdmiral: ranged friends +10% range");
            UnityEngine.Object.DestroyImmediate(pigeon.Weapon); UnityEngine.Object.DestroyImmediate(pigeon);
            Signature(f.Run, catalog, "KiteAdmiral");
            // the fleet: a kite over the ranged friend, whose shots split and pierce one more while the gauge is full
            UnitState friend = f.Sim.Units.First(u => u.Faction == Faction.Companion && u.Definition.Weapon.Kind == WeaponKind.Projectile);
            friend.Position = f.Sim.Champion.Position + Vector2.up * .6f;
            for (int i = 0; i < 4; i++) f.Foe(new Vector2(2.5f + i * .3f, (i - 1.5f) * .4f));
            f.Transform();
            Require(f.Sim.HeroAwakening && f.Sim.HeroKiteFleet.Contains(friend), "KiteAdmiral: KITE FLEET flies over the nearest ranged friend");
            int split = 0, pierced = 0;
            for (int i = 0; i < 80 && f.Sim.HeroAwakening; i++)
            {
                f.Step(.05f);
                foreach (var p in f.Sim.Projectiles.Where(p => p.SourceUnitId == friend.Id)) { if (p.Cause.ItemId == "kite-split") split++; if (p.ExtraPierce >= 1) pierced++; }
            }
            Require(split > 0 && pierced > 0, "KiteAdmiral: escorted friends' shots split in two and pierce one more (" + split + " split, " + pierced + " pierced)");
        }

        private static void Sundae(RunCatalog catalog)
        {
            using var f = new Fight(catalog, "SundaeDuchess", 41902);
            UnitState frozen = f.Foe(new Vector2(1.5f, 0)), warm = f.Foe(new Vector2(1.5f, 1));
            frozen.FrozenUntil = f.Sim.Elapsed + 2;
            float frozenFill = Feed(f.Sim, new CombatEvent(CombatEventKind.Kill, frozen.Position, Faction.Enemy, subject: frozen, cause: new CombatCause(920001, 0)));
            float warmFill = Feed(f.Sim, new CombatEvent(CombatEventKind.Kill, warm.Position, Faction.Enemy, subject: warm, cause: new CombatCause(920002, 0)));
            float hurtFill = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, f.Sim.Champion.Position, Faction.Champion, 1, subject: f.Sim.Champion, cause: new CombatCause(920003, frozen.Id)));
            Require(frozenFill >= 2 && warmFill == 0 && hurtFill >= 2, "SundaeDuchess: frozen enemies felled and blows taken melt the sundae (" + frozenFill + "/" + warmFill + "/" + hurtFill + ")");
            // frozen kills pay a coin, at most five a wave
            int coins = f.Sim.Pickups.Count;
            for (int i = 0; i < 8; i++) Feed(f.Sim, new CombatEvent(CombatEventKind.Kill, frozen.Position, Faction.Enemy, subject: frozen, cause: new CombatCause(920010 + i, 0)));
            Require(f.Sim.Pickups.Count - coins == 4, "SundaeDuchess: each frozen enemy felled pays a coin, at most 5 per wave (" + (f.Sim.Pickups.Count - coins + 1) + ")");
            // a crit on a frozen enemy cracks 5% of its maximum health
            float before = frozen.Health;
            Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, frozen.Position, Faction.Enemy, 10, Vector2.right, critical: true, subject: frozen, cause: f.Personal));
            Require(before - frozen.Health >= frozen.Definition.Health * .03f, "SundaeDuchess: a crit on a frozen enemy takes 5% of its max HP (" + (before - frozen.Health) + ")");
            // food: +5 max HP each (max +40), 15% cheaper
            f.Run.State.OctoberFood = 0; float none = f.Run.Bonus(ItemEffect.Health);
            f.Run.State.OctoberFood = 3; float three = f.Run.Bonus(ItemEffect.Health);
            f.Run.State.OctoberFood = 100; float lots = f.Run.Bonus(ItemEffect.Health);
            Require(Mathf.Abs(three - none - 15) < .01f && Mathf.Abs(lots - none - 40) < .01f, "SundaeDuchess: each food +5 max HP, up to +40");
            ItemDefinition food = catalog.Items.First(i => i.Category == RelicCategory.Food);
            f.Run.State.Phase = SessionPhase.Shop; f.Run.State.Offers.Clear();
            var offer = new ShopOffer { Token = ++f.Run.State.NextOffer, Id = food.Id, Kind = OfferKind.Item, Price = 40, BasePrice = 40 }; f.Run.State.Offers.Add(offer);
            Call(f.Run, "ApplyKitPrices");
            Require(offer.Price == 34, "SundaeDuchess: food costs 15% less (" + offer.Price + ")");
            Signature(f.Run, catalog, "SundaeDuchess");
            // SUNDAE STORM: giant scoops fall and freeze what they land on
            f.Run.State.Phase = SessionPhase.Combat;
            for (int i = 0; i < 4; i++) f.Foe(new Vector2(2 + i * .4f, (i - 1.5f) * .6f));
            f.Transform();
            bool drops = false, froze = false;
            for (int i = 0; i < 60 && f.Sim.HeroAwakening; i++) { f.Step(.05f); drops |= f.Sim.HeroAwakenDrops.Count > 0; froze |= f.Foes.Skip(2).Any(u => u.FrozenUntil > f.Sim.Elapsed); }
            Require(drops && froze, "SundaeDuchess: SUNDAE STORM drops scoops that freeze enemies");
        }

        private static void Bonsai(RunCatalog catalog)
        {
            using var f = new Fight(catalog, "BonsaiClocksmith", 41903);
            UnitState foe = f.Foe(new Vector2(1.5f, 0));
            float hit = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, foe.Position, Faction.Enemy, 10, Vector2.right, subject: foe, cause: f.Personal));
            float kill = Feed(f.Sim, new CombatEvent(CombatEventKind.Kill, foe.Position, Faction.Enemy, subject: foe, cause: f.Personal));
            Require(hit >= 1 && kill > 0, "BonsaiClocksmith: hits and kills wind the clock (" + hit + "/" + kill + ")");
            int gold = f.Run.State.Gold = 10; Call(f.Run, "KitMerged");
            Require(f.Run.State.Gold == gold + 2, "BonsaiClocksmith: every merge pays 2 gold");
            BondDefinition clock = catalog.Bonds.First(b => b.Id == "clock"), orchard = catalog.Bonds.First(b => b.Id == "orchard");
            f.Run.BondCounts.Clear(); f.Run.BondCounts["clock"] = clock.Threshold; f.Run.BondCounts["orchard"] = orchard.Threshold * 2;
            float haste = (float)Call(f.Run, "KitBonus", ItemEffect.CompanionAttackSpeed, ModifierScope.Companions);
            Require(Mathf.Abs(haste - .04f * 3) < .001f, "BonsaiClocksmith: friends +4% attack speed per Clock or Orchard bond level (" + haste + ")");
            f.Run.BondCounts["clock"] = 60; haste = (float)Call(f.Run, "KitBonus", ItemEffect.CompanionAttackSpeed, ModifierScope.Companions);
            Require(Mathf.Abs(haste - .16f) < .001f, "BonsaiClocksmith: that haste stops at 16%");
            Signature(f.Run, catalog, "BonsaiClocksmith");
            // TIME GARDEN: planted ahead; its hand strikes and slows; friends beside it fight faster
            f.Step(.3f, Vector2.right);
            f.Transform();
            Vector2 plot = f.Sim.HeroAwakenPlot;
            Require(f.Sim.HeroAwakening && (plot - f.Sim.Champion.Position).magnitude > .8f, "BonsaiClocksmith: the bonsai is planted ahead of the hero");
            var ring = new List<UnitState>();
            for (int i = 0; i < 6; i++) ring.Add(f.Foe(plot + (Vector2)(Quaternion.Euler(0, 0, i * 60) * Vector2.right) * 1.6f - f.Sim.Champion.Position));
            UnitState friend = f.Sim.Units.First(u => u.Faction == Faction.Companion);
            friend.Position = plot + Vector2.down * .5f; float close = (float)Call(f.Sim, "KitCycle", friend);
            friend.Position = plot + Vector2.down * 6f; float away = (float)Call(f.Sim, "KitCycle", friend);
            Require(close < away * .8f, "BonsaiClocksmith: friends near the bonsai +40% attack speed (" + close + " vs " + away + ")");
            bool slowed = false;
            for (int i = 0; i < 160 && f.Sim.HeroAwakening; i++) { f.Step(.05f); slowed |= ring.Any(u => u.ChillUntil > f.Sim.Elapsed && u.Health < u.Definition.Health); }
            Require(slowed, "BonsaiClocksmith: the clock hand strikes and slows what it crosses");
            for (int i = 0; i < 60 && f.Sim.HeroAwakening; i++) f.Step(.05f);
            Require(!f.Sim.HeroAwakening && f.Sim.HeroIdentityCharge >= f.Sim.HeroIdentityRequired * .5f - .01f, "BonsaiClocksmith: half a tier stays filled after transforming (" + f.Sim.HeroIdentityCharge + ")");
        }

        private static void Jester(RunCatalog catalog)
        {
            using var f = new Fight(catalog, "JugglingJester", 41904);
            UnitState foe = f.Foe(new Vector2(1.5f, 0));
            UnitState friend = f.Sim.Units.First(u => u.Faction == Faction.Companion);
            float crit = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, foe.Position, Faction.Enemy, 10, Vector2.right, critical: true, subject: foe, cause: new CombatCause(930001, friend.Id)));
            float plain = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, foe.Position, Faction.Enemy, 10, Vector2.right, subject: foe, cause: new CombatCause(930002, friend.Id)));
            Require(crit >= 1 && plain == 0, "JugglingJester: friends' crits fill the gauge (" + crit + "/" + plain + ")");
            // a hero crit tosses a ball onto a random enemy
            int strikes = Get<System.Collections.IList>(f.Sim, "identityStrikes").Count;
            Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, foe.Position, Faction.Enemy, 20, Vector2.right, critical: true, subject: foe, cause: f.Personal));
            Require(Get<System.Collections.IList>(f.Sim, "identityStrikes").Count == strikes + 1 && f.Sim.HeroIdentityGlyphs.Any(g => g.Role == HeroGlyphRole.Lob), "JugglingJester: a crit tosses a ball that drops on an enemy");
            float before = foe.Health; f.Step(.8f);
            Require(foe.Health < before, "JugglingJester: the ball lands");
            float teamCrit = (float)Call(f.Run, "KitBonus", ItemEffect.Crit, ModifierScope.Companions);
            Require(Mathf.Abs(teamCrit - .06f) < .001f, "JugglingJester: friends +6% crit");
            // Big Top recruits: 10% arrive at two stars
            UnitDefinition bigTop = catalog.Companions.First(u => u.Body != null && u.Tags.Contains("big-top") && u.Rarity < ItemRarity.Mythic);
            UnitDefinition other = catalog.Companions.First(u => u.Body != null && !u.Tags.Contains("big-top") && u.Rarity < ItemRarity.Mythic);
            int raised = 0, otherRaised = 0; var weapons = new List<WeaponDefinition>();
            for (int i = 0; i < 2000; i++)
            {
                var e = new RosterEntry { UnitId = bigTop.Id, Tier = 1, WeaponId = bigTop.Weapon.Id }; Call(f.Run, "KitRecruited", e, false, weapons); if (e.Tier == 2) raised++;
                var o = new RosterEntry { UnitId = other.Id, Tier = 1, WeaponId = other.Weapon.Id }; Call(f.Run, "KitRecruited", o, false, weapons); if (o.Tier == 2) otherRaised++;
            }
            Require(raised > 100 && raised < 320 && otherRaised == 0, "JugglingJester: a Big Top recruit arrives at two stars 10% of the time (" + raised + "/2000)");
            f.Run.State.Roster.Add(new RosterEntry { UnitId = bigTop.Id, Tier = 1, WeaponId = bigTop.Weapon.Id });
            Signature(f.Run, catalog, "JugglingJester");
            // GRAND CIRCUS: five balls slam down, then every throw tosses two pins
            for (int i = 0; i < 5; i++) f.Foe(new Vector2(1.8f + i * .35f, (i - 2) * .45f));
            f.Transform();
            Require(f.Sim.HeroAwakening && f.Sim.Champion.AwakenArt == HeroKits.For("JugglingJester").ArtKey, "JugglingJester: GRAND CIRCUS holds the golden clubs");
            int pairs = 0;
            for (int i = 0; i < 100 && f.Sim.HeroAwakening; i++)
            {
                f.Step(.05f);
                // two pins of one throw fly together (landing 0.12s apart); one pin per throw never overlaps like that
                var pins = f.Sim.ArsenalProps.Where(p => p.Source == f.Sim.Champion && p.Kind == ArsenalPropKind.Pin && !p.Resolved).Select(p => p.LandAt).OrderBy(x => x).ToList();
                for (int k = 1; k < pins.Count; k++) if (pins[k] - pins[k - 1] < .2f) { pairs++; break; }
            }
            Require(pairs > 0, "JugglingJester: every throw tosses two pins (" + pairs + " steps with a pair in the air)");
            Require(f.Foes.Skip(1).Count(u => u.Health < u.Definition.Health) >= 3, "JugglingJester: the balls and pins strike the enemies");
        }

        private static void Bell(RunCatalog catalog)
        {
            using var f = new Fight(catalog, "BellDiver", 41905);
            UnitState probed = f.Foe(new Vector2(1.5f, 0)), plain = f.Foe(new Vector2(1.5f, 1));
            probed.ProbedUntil = f.Sim.Elapsed + 30;
            float onProbed = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, probed.Position, Faction.Enemy, 10, Vector2.right, subject: probed, cause: f.Personal));
            float onPlain = Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, plain.Position, Faction.Enemy, 10, Vector2.right, subject: plain, cause: f.Personal));
            Require(onPlain >= 1 && onProbed > onPlain, "BellDiver: hits fill the depth gauge, probed enemies more (" + onProbed + "/" + onPlain + ")");
            int coins = f.Sim.Pickups.Count;
            for (int i = 0; i < 200; i++) Feed(f.Sim, new CombatEvent(CombatEventKind.Hit, probed.Position, Faction.Enemy, 10, Vector2.right, subject: probed, cause: new CombatCause(940000 + i, f.Sim.Champion.Id)));
            Require(f.Sim.Pickups.Count - coins >= 1 && f.Sim.Pickups.Count - coins <= 5, "BellDiver: hits on probed enemies drop coins, at most 5 a wave (" + (f.Sim.Pickups.Count - coins) + ")");
            UnitState friend = f.Sim.Units.First(u => u.Faction == Faction.Companion);
            var friendCause = new CombatCause(940900, friend.Id);
            float vsProbed = (float)Call(f.Sim, "KitDamageDealt", probed, 100f, Faction.Companion, friendCause), vsPlain = (float)Call(f.Sim, "KitDamageDealt", plain, 100f, Faction.Companion, friendCause);
            Require(Mathf.Abs(vsProbed / vsPlain - 1.08f) < .001f, "BellDiver: friends deal +8% to probed enemies (" + vsProbed + "/" + vsPlain + ")");
            var shot = new ProjectileState { Faction = Faction.Companion, SourceUnitId = friend.Id };
            bool first = (bool)Call(f.Sim, "KitProbedBounce", shot, probed), again = (bool)Call(f.Sim, "KitProbedBounce", shot, probed), off = (bool)Call(f.Sim, "KitProbedBounce", new ProjectileState { Faction = Faction.Companion, SourceUnitId = friend.Id }, plain);
            Require(first && !again && !off, "BellDiver: a friend's shot bounces once more off a probed enemy");
            Signature(f.Run, catalog, "BellDiver");
            // DIVING BELL: half the damage, the ram probes what is ahead, depth charges behind
            f.Step(.3f, Vector2.right);
            var front = new List<UnitState>(); for (int i = 0; i < 3; i++) front.Add(f.Foe(new Vector2(.8f, (i - 1) * .3f)));
            UnitState behind = f.Foe(new Vector2(-1.1f, 0));
            f.Transform();
            float taken = (float)Call(f.Sim, "KitDamageTaken", new CombatCause(941000, front[0].Id));
            Require(f.Sim.HeroAwakening && Mathf.Abs(taken - .5f) < .001f, "BellDiver: inside the bell the hero takes 50% less damage (" + taken + ")");
            bool probing = false, charges = false;
            for (int i = 0; i < 60 && f.Sim.HeroAwakening; i++) { f.Step(.05f); probing |= front.Any(u => u.ProbedUntil > f.Sim.Elapsed && u.Health < u.Definition.Health); charges |= f.Sim.HeroAwakenDrops.Any(d => d.Kind == 1); }
            Require(probing && charges && behind.Health < behind.Definition.Health, "BellDiver: the bell rams and probes what is ahead and drops depth charges behind");
        }

        /// <summary>The hero's signature minor bond counts one level higher once a member is in the brigade.</summary>
        private static void Signature(RunSession run, RunCatalog catalog, string id)
        {
            string bond = SignatureBond[id];
            if (!run.State.Roster.Any(r => catalog.Unit(r.UnitId).Tags.Contains(bond)))
            {
                UnitDefinition member = catalog.Companions.First(u => u.Body != null && u.Tags.Contains(bond));
                run.State.Roster.Add(new RosterEntry { UnitId = member.Id, Tier = 1, WeaponId = member.Weapon.Id });
            }
            Call(run, "RecountBonds");
            Require(run.KitBondExtra(bond) > 0, id + ": " + bond + " counts one level higher (+" + run.KitBondExtra(bond) + ")");
        }

        /// <summary>Clone-only: apply the expansion builder to the clone's catalog, then the wide Act I regression sweep.</summary>
        public static void ApplyAndRunWide() { ContentExpansionBuilder.Apply(); ActOneStoryChecks.RunWide(); }

        /// <summary>Clone-only batch (scratchpad/unity_clone.sh): apply the expansion builder to the clone's catalog, then run the
        /// suites that touch the heroes; one failing suite does not hide the others.</summary>
        public static void BuildAndVerify()
        {
            var lines = new List<string>(); int failed = 0;
            void Suite(string name, Action run)
            {
                try { run(); lines.Add("PASS " + name); }
                catch (Exception error) { failed++; Exception root = error.InnerException ?? error; lines.Add("FAIL " + name + ": " + root.Message + " @ " + (root.StackTrace ?? "").Split('\n').FirstOrDefault()); }
            }
            Suite("Apply", ContentExpansionBuilder.Apply);
            Suite("ExpansionHeroes", Run);
            Suite("HeroAwakening", HeroAwakeningChecks.Run);
            Suite("HeroIdentity", HeroIdentityChecks.Run);
            Suite("ContentExpansion", ContentExpansionChecks.Run);
            Suite("ShopDescription", ShopDescriptionChecks.Run);
            Suite("Arsenal", ArsenalChecks.Run);
            Directory.CreateDirectory("PreviewExports/ExpansionHeroes"); File.WriteAllLines("PreviewExports/ExpansionHeroes/Suites.txt", lines);
            Debug.Log("EXPANSION_HERO_SUITES " + (lines.Count - failed) + "/" + lines.Count + " PASS\n" + string.Join("\n", lines));
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
    }
}
