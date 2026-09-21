using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>How a bond-step moment looks: a blast where it lands, a strike along a line, a lasting zone, or weather over the arena.</summary>
    public enum BondMomentKind { Strike, Line, Zone, Weather }

    /// <summary>A visible bond-step moment for the presentation: what lands where and when (Delay seconds after it is announced).</summary>
    public readonly struct BondMomentRequest
    {
        public readonly string Bond, Rule; public readonly int Step; public readonly BondMomentKind Kind;
        public readonly Vector2 From, To; public readonly float Radius, Delay, Duration;
        public BondMomentRequest(string bond, string rule, int step, BondMomentKind kind, Vector2 from, Vector2 to, float radius, float delay, float duration)
        { Bond = bond; Rule = rule; Step = step; Kind = kind; From = from; To = to; Radius = radius; Delay = delay; Duration = duration; }
    }

    /// <summary>Numbers of the bond step ladder (Docs/Bond_Steps_Proposal_2026-10-28.md): step 6 an event that falls on the
    /// crowds on a beat, step 8 weather or a legendary summon for the whole wave. Damage is a share of the best friend's weapon.</summary>
    public static class BondStepTuning
    {
        public const int StormEvery = 5, StormStrongTargets = 6, WindEvery = 8, VineEvery = 3, SealEvery = 6, ShoveEvery = 4, SpikeEvery = 8, NoteEvery = 4, Notes = 5, Gears = 3;
        public const float VineSeconds = .5f, SealRadius = .9f, SealDamage = .8f, ShoveStun = .2f, SpikeRadius = 1.4f, SpikeDamage = 1, GearDamage = .6f, CastNoteDamage = .5f, WaveRadius = 2, WaveDamage = .8f, OnHitCooldown = .5f;
        public const float StormStrongStun = .3f;
        public const float ThunderboltPeriod = 6, ThunderboltRadius = 2.5f, ThunderboltDamage = 2.5f, StormRainPeriod = .6f, StormRainRadius = 1.4f, StormRainDamage = 1;
        public const float MeteorPeriod = 8, MeteorRadius = 1.6f, MeteorDamage = 1.6f, PhoenixDivePeriod = 3, PhoenixDiveDamage = 1.5f;
        public const int Meteors = 3;
        public const float IceSpikePeriod = 10, IceSpikeRadius = 2.5f, IceSpikeDamage = 1.5f, HailPeriod = 4, HailRadius = 2, HailDamage = 1.2f, BlizzardSlow = .3f;
        public const float WhirlpoolPeriod = 10, WhirlpoolRadius = 2.5f, WhirlpoolSeconds = 3, WhirlpoolDamage = 2, LeviathanPeriod = 12, LeviathanDamage = 2.5f, LeviathanReach = 10;
        public const float TornadoPeriod = 10, TornadoRadius = 1.8f, TornadoSeconds = 5, TornadoSpeed = 2.5f, TornadoTick = .4f, GalePeriod = 8;
        public const int GaleTornadoes = 3;
        public const float TreantSlamPeriod = 5, TreantSlamRadius = 3, TreantSlamDamage = 2, RootEruptionPeriod = 4, RootEruptionRadius = 1.5f, RootEruptionDamage = 1, RootSeconds = 1.5f;
        public const int SmallTreants = 2, GroveTreants = 4, RootEruptions = 3;
        public const float FlaskPeriod = 8, FlaskRadius = 2.2f, FlaskDamage = 1, FlaskCloudSeconds = 4, PlagueMistPeriod = 1;
        public const float StarShowerPeriod = 10, StarRadius = 1, StarDamage = 1, StarryNightPeriod = .8f, StarryNightDamage = .8f, CometPeriod = 15, CometRadius = 4, CometDamage = 4, CelestialCrit = .25f;
        public const int ShowerStars = 8;
        public const float ShadowPitPeriod = 10, ShadowPitRadius = 2.5f, ShadowPitSeconds = 1.5f, ShadowPitDamage = 1, ShadowPitExecute = .2f;
        public const float ConfettiPeriod = 10, ConfettiRadius = 1.2f, ConfettiDamage = 1.2f, FinalePeriod = 12;
        public const int ConfettiShells = 6, RoyalGuards = 2, RoyalGuardsCrowned = 4;
        public const float DecreePeriod = 12, DecreeRadius = 2.5f, DecreeDamage = 2, FanfarePeriod = 10;
        public const float BookPeriod = 12, BookRadius = 2.5f, BookDamage = 2.5f, PagePeriod = 1, PageRadius = 1, PageDamage = .7f;
        public const float LighthouseTurn = 1.2f, LighthouseTick = .25f, LighthouseReach = 12, LighthouseWidth = .8f, LighthouseDamage = .3f;
        public const float BoulderPeriod = 10, BoulderWidth = 1.2f, BoulderDamage = 2, BoulderReach = 7, StompPeriod = 2.5f, StompRadius = 2.5f, StompDamage = 1.5f;
        public const float PendulumPeriod = 6, PendulumWidth = 1.5f, PendulumDamage = 2, PendulumReach = 10;
        public const int Turrets = 1, OverclockTurrets = 3;
        public const float DishPeriod = 12, HotPotPeriod = 12, HotPotRadius = 2.5f, HotPotSeconds = 3, HotPotTick = .5f, PiePeriod = 1.5f, PieRadius = 1.3f, PieDamage = 1, FeastPeriod = 15, FeastBoost = .5f, FeastSeconds = 5;
        public const int SmallChefs = 2, PotDishes = 2;
        public const float FortressPeriod = 20, FortressSeconds = 5, FortressBolt = .5f, FortressDamage = .8f;
        public const float SwarmPeriod = 10, SwarmRadius = 1.5f, SwarmDamage = 1.5f, ReaperPeriod = 6, ReaperRadius = 3, ReaperDamage = 2.5f;
        public const int SwarmSpirits = 3;
        public const float DrumPeriod = 8, DrumRadius = 2.5f, DrumDamage = .8f, NoteRainPeriod = 1, NoteRadius = 1, NoteDamage = .6f;
        public const int DrumBeats = 3, NoteSpots = 3;
        public const float PlanetPeriod = 10, PlanetRadius = 3.5f, PlanetDamage = 3.5f;
        public const float LetterPeriod = 10, LetterRadius = 1.6f, LetterDamage = 1, StillRootPeriod = 3, CloudPeriod = 8, CloudRadius = 2, CloudSeconds = 4, CloudSpeed = 1, CloudRain = .25f, MonolithReach = 2.5f, MonolithRadius = 1.8f, ShardDamage = .8f;
        public const float ZoneTick = .5f, KnockDistance = 1.5f, StunSeconds = 1, ClusterApart = 2.5f, Wave = 999;
    }

    public sealed partial class BondRuntime
    {
        /// <summary>The older invisible step-6 and step-8 rules (counters that unlock a once-a-wave moment) that the step ladder
        /// replaces. Kept switchable for the older checks; off in play.</summary>
        public static bool LegacyCapstones = false;
        public event Action<BondMomentRequest> MomentRequested;

        private enum StepEffect { None, Burn, Freeze, Soak, Root, Poison, Stun, Execute, Knock, Slow }
        private struct PendingStrike { public string Rule; public Vector2 From, To; public float Radius, Damage, At; public StepEffect Effect; public bool Line, Chain; }
        private struct StepZone { public string Bond, Rule; public Vector2 Position; public float Radius, Until, NextTick, TickDamage, Pull, Speed, EndDamage; public StepEffect Effect, EndEffect; }
        private readonly List<PendingStrike> pendingStrikes = new List<PendingStrike>(BondTuning.RequestCapacity);
        private readonly List<StepZone> stepZones = new List<StepZone>(BondTuning.RequestCapacity);
        private readonly Dictionary<string, float> stepTimers = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly List<Vector2> spotBuffer = new List<Vector2>(16);
        private readonly List<UnitState> spotOrder = new List<UnitState>(BondTuning.EnemyCapacity);
        private readonly Dictionary<UnitState, int> spotCrowd = new Dictionary<UnitState, int>();
        private float fortressUntil, lighthouseAngle;
        private readonly Dictionary<string, int> ownHits = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, float> enemyEffectAt = new Dictionary<string, float>(StringComparer.Ordinal);

        /// <summary>"Every Nth hit" counts this friend's own hits only, so a full team never fires it nonstop.</summary>
        private bool EveryOwn(string kind, UnitState friend, int interval)
        {
            string key = kind + "#" + friend.Id; ownHits.TryGetValue(key, out int count); ownHits[key] = ++count; return count % interval == 0;
        }
        /// <summary>An enemy takes the same on-hit effect at most once every half second.</summary>
        private bool Fresh(string kind, UnitState enemy)
        {
            string key = kind + "#" + enemy.Id;
            if (enemyEffectAt.TryGetValue(key, out float at) && time - at < BondStepTuning.OnHitCooldown) return false;
            enemyEffectAt[key] = time; return true;
        }

        /// <summary>Steps 2 and 4 on a friend's own hits: vines root (Garden), a royal seal stamps (Court), a shove and a rock spike (Stone).</summary>
        private void HitV6(UnitState attacker, UnitState enemy)
        {
            if (LegacyCapstones || attacker == null || attacker.Faction != Faction.Companion && attacker.Faction != Faction.Champion) return;
            float w = Weapon(attacker);
            if (RuleApplies(attacker, "orchard") && EveryOwn("vine", attacker, BondStepTuning.VineEvery) && Fresh("vine", enemy))
            { BondEnemyState state = State(enemy); if (state != null) state.RootedUntil = Mathf.Max(state.RootedUntil, time + BondStepTuning.VineSeconds); Strike("orchard", "garden-vine", 2, enemy.Position, .6f, 0, StepEffect.Root, 0); }
            if (RuleApplies(attacker, "regal") && EveryOwn("seal", attacker, BondStepTuning.SealEvery) && Fresh("seal", enemy))
                Strike("regal", "court-seal", 2, enemy.Position, BondStepTuning.SealRadius, w * BondStepTuning.SealDamage, StepEffect.Stun, .25f);
            if (RuleApplies(attacker, "stone") && EveryOwn("shove", attacker, BondStepTuning.ShoveEvery) && Fresh("shove", enemy)) StunArea(enemy, BondStepTuning.ShoveStun, "stone-shove", true);
            if (Active("stone", 4) && RuleApplies(attacker, "stone") && EveryOwn("spike", attacker, BondStepTuning.SpikeEvery))
                Strike("stone", "stone-spike", 4, enemy.Position, BondStepTuning.SpikeRadius, w * BondStepTuning.SpikeDamage, StepEffect.Knock, .2f);
        }

        /// <summary>Steps 2 and 4 on a friend's own casts: gears fly at the crowds (Clockwork), notes and then a sound wave (Melody).</summary>
        private void CastV6(UnitState friend)
        {
            if (LegacyCapstones || friend == null || hero == null) return;
            float w = Weapon(friend); List<Vector2> spots;
            if (RuleApplies(friend, "clock") && (spots = Spots(BondStepTuning.Gears)).Count > 0)
                for (int i = 0; i < BondStepTuning.Gears; i++) Strike("clock", "clock-gears", 2, spots[i % spots.Count], .8f, w * BondStepTuning.GearDamage, StepEffect.None, .3f + i * .08f, false, friend.Position);
            if (RuleApplies(friend, "melody") && EveryOwn("note", friend, BondStepTuning.NoteEvery) && (spots = Spots(BondStepTuning.Notes)).Count > 0)
            {
                for (int i = 0; i < BondStepTuning.Notes; i++) Strike("melody", "melody-notes", 2, spots[i % spots.Count], .7f, w * BondStepTuning.CastNoteDamage, StepEffect.None, .3f + i * .07f, false, friend.Position);
                if (Active("melody", 4)) Strike("melody", "melody-sound-wave", 4, spots[0], BondStepTuning.WaveRadius, w * BondStepTuning.WaveDamage, StepEffect.Knock, .5f);
            }
        }

        private float StepWeapon => Mathf.Max(Weapon(bestFriend), Weapon(hero));
        private float StepSlow => !LegacyCapstones && Active("frost", 8) ? 1 - BondStepTuning.BlizzardSlow : 1;

        private void ResetV6()
        {
            pendingStrikes.Clear(); stepZones.Clear(); stepTimers.Clear(); ownHits.Clear(); enemyEffectAt.Clear(); fortressUntil = 0; lighthouseAngle = 0;
            if (LegacyCapstones || hero == null) return;
            float weapon = StepWeapon;
            void Summon(BondSpawnKind kind, int count) { for (int i = 0; i < count; i++) Spawn(new BondSpawnRequest(kind, hero.Position + new Vector2(i - (count - 1) * .5f, -1), sourceId: i, damage: weapon)); }
            // summons that stand with the team for the whole wave
            if (Active("orchard", 4)) Summon(BondSpawnKind.Sapling, Active("orchard", 8) ? BondStepTuning.GroveTreants : BondStepTuning.SmallTreants);
            if (Active("orchard", 6)) Summon(Active("orchard", 8) ? BondSpawnKind.GreatTree : BondSpawnKind.Treant, 1);
            if (Active("hearth", 4)) Summon(BondSpawnKind.Chef, BondStepTuning.SmallChefs);
            if (Active("hearth", 8)) Summon(BondSpawnKind.GrandChef, 1);
            if (Active("regal", 4)) Summon(BondSpawnKind.RoyalGuard, Active("regal", 8) ? BondStepTuning.RoyalGuardsCrowned : BondStepTuning.RoyalGuards);
            if (Active("clock", 4)) Summon(BondSpawnKind.Turret, Active("clock", 8) ? BondStepTuning.OverclockTurrets : BondStepTuning.Turrets);
            if (Active("ember", 8)) Summon(BondSpawnKind.Phoenix, 1);
            if (Active("oddity", 8)) Summon(BondSpawnKind.ParadeFloat, 1);
            if (Active("spirit", 8)) Summon(BondSpawnKind.Reaper, 1);
            // weather that colours the whole wave
            if (Active("static-club", 8)) StepWeather("static-club", "storm-thunderstorm");
            if (Active("frost", 8)) StepWeather("frost", "frost-blizzard");
            if (Active("tidal", 8)) StepWeather("tidal", "tide-rain");
            if (Active("venom", 8)) StepWeather("venom", "plague-mist");
            if (Active("astral", 8)) { StepWeather("astral", "starry-night"); celestialUntil = BondStepTuning.Wave; }
            if (Active("dusk", 8)) StepWeather("dusk", "eclipse");
            if (Active("lantern-guild", 8)) StepWeather("lantern-guild", "lighthouse");
            if (Active("gravity", 8)) StepWeather("gravity", "gravity-heavy");
            if (Active("windborne", 8)) StepWeather("windborne", "wind-gale");
        }

        private void Moment(string bond, string rule, int step, BondMomentKind kind, Vector2 from, Vector2 to, float radius, float delay, float duration = 0)
            => MomentRequested?.Invoke(new BondMomentRequest(bond, rule, step, kind, from, to, radius, delay, duration));
        private void StepWeather(string bond, string rule) => Moment(bond, rule, 8, BondMomentKind.Weather, hero.Position, hero.Position, 0, 0, BondStepTuning.Wave);

        /// <summary>True on a step event's beat; the first beat comes half a period into the wave.</summary>
        private bool Due(string key, float period)
        {
            if (!stepTimers.TryGetValue(key, out float at)) { stepTimers[key] = time + period * .5f; return false; }
            if (time < at) return false;
            stepTimers[key] = at + period; return true;
        }

        /// <summary>Up to count crowd centres, densest first, at least ClusterApart from each other.</summary>
        private List<Vector2> Spots(int count)
        {
            spotBuffer.Clear(); spotOrder.Clear(); spotCrowd.Clear();
            float square = BondTuning.ClumpRadius * BondTuning.ClumpRadius;
            foreach (var pair in enemies) if (pair.Key.Alive)
            {
                int crowd = 0; foreach (var other in enemies) if (other.Key.Alive && (other.Key.Position - pair.Key.Position).sqrMagnitude <= square) crowd++;
                spotCrowd[pair.Key] = crowd; spotOrder.Add(pair.Key);
            }
            spotOrder.Sort((a, b) => { int order = spotCrowd[b].CompareTo(spotCrowd[a]); return order != 0 ? order : a.Id.CompareTo(b.Id); });
            float apart = BondStepTuning.ClusterApart * BondStepTuning.ClusterApart;
            foreach (UnitState unit in spotOrder)
            {
                bool far = true; foreach (Vector2 spot in spotBuffer) if ((spot - unit.Position).sqrMagnitude < apart) { far = false; break; }
                if (far) spotBuffer.Add(unit.Position);
                if (spotBuffer.Count >= count) break;
            }
            return spotBuffer;
        }

        private void Strike(string bond, string rule, int step, Vector2 at, float radius, float damage, StepEffect effect, float delay, bool chain = false, Vector2? from = null)
        {
            Moment(bond, rule, step, BondMomentKind.Strike, from ?? at, at, radius, delay);
            pendingStrikes.Add(new PendingStrike { Rule = rule, To = at, Radius = radius, Damage = damage, Effect = effect, At = time + delay, Chain = chain });
        }
        private void LineStrike(string bond, string rule, int step, Vector2 from, Vector2 to, float width, float damage, StepEffect effect, float delay)
        {
            Moment(bond, rule, step, BondMomentKind.Line, from, to, width, delay);
            pendingStrikes.Add(new PendingStrike { Rule = rule, From = from, To = to, Radius = width, Damage = damage, Effect = effect, At = time + delay, Line = true });
        }
        private void Zone(string bond, string rule, int step, Vector2 at, float radius, float seconds, float tickDamage, float pull, StepEffect effect, float endDamage, StepEffect endEffect, float speed = 0)
        {
            Moment(bond, rule, step, BondMomentKind.Zone, at, at, radius, 0, seconds);
            stepZones.Add(new StepZone { Bond = bond, Rule = rule, Position = at, Radius = radius, Until = time + seconds, NextTick = time + BondStepTuning.ZoneTick, TickDamage = tickDamage, Pull = pull, Speed = speed, EndDamage = endDamage, Effect = effect, EndEffect = endEffect });
        }

        private void Land(PendingStrike s)
        {
            UnitState first = null; float nearest = float.PositiveInfinity;
            enemyTickBuffer.Clear(); foreach (var pair in enemies) if (pair.Key.Alive) enemyTickBuffer.Add(pair.Key);
            foreach (UnitState enemy in enemyTickBuffer)
            {
                bool inside = s.Line ? CompanionSkillRuntime.Contains(SkillShape.Line, s.From, s.To, s.Radius, enemy.Position) : (enemy.Position - s.To).sqrMagnitude <= s.Radius * s.Radius;
                if (!inside) continue;
                float square = (enemy.Position - s.To).sqrMagnitude; if (square < nearest) { nearest = square; first = enemy; }
                Damage(enemy, bestFriend ?? hero, s.Damage, s.Rule); Affect(enemy, s.Effect, s.Line ? (s.To - s.From).normalized : (enemy.Position - s.To).normalized);
            }
            if (s.Chain && first != null) StormChain(first, bestFriend ?? hero, StepWeapon);
        }

        private void Affect(UnitState enemy, StepEffect effect, Vector2 away)
        {
            BondEnemyState state = State(enemy); if (state == null || !enemy.Alive) return;
            switch (effect)
            {
                case StepEffect.Burn: AddBurn(enemy, bestFriend ?? hero, StepWeapon); AddBurn(enemy, bestFriend ?? hero, StepWeapon); break;
                case StepEffect.Freeze: state.Chill = BondTuning.ChillStacks; state.FrozenUntil = Mathf.Max(state.FrozenUntil, time + BondTuning.FreezeSeconds); state.FrozenHitReady = true; break;
                case StepEffect.Soak: state.SoakedUntil = time + BondTuning.SoakSeconds; break;
                case StepEffect.Root: state.RootedUntil = Mathf.Max(state.RootedUntil, time + BondStepTuning.RootSeconds); break;
                case StepEffect.Poison: AddPoison(enemy, bestFriend ?? hero, StepWeapon, BondTuning.BurnStacks, true); break;
                case StepEffect.Stun: state.StunnedUntil = Mathf.Max(state.StunnedUntil, time + BondStepTuning.StunSeconds); enemy.NextAttack = Mathf.Max(enemy.NextAttack, time + BondStepTuning.StunSeconds); break;
                case StepEffect.Execute: if (enemy.Health < enemy.Definition.Health * BondStepTuning.ShadowPitExecute) Damage(enemy, bestFriend ?? hero, enemy.Health, "dusk-execute"); break;
                case StepEffect.Slow: state.Chill = Mathf.Min(BondTuning.ChillStacks, state.Chill + 2); break;
                case StepEffect.Knock: if (away.sqrMagnitude > 0) enemy.Position += away.normalized * BondStepTuning.KnockDistance; break;
            }
        }

        private void TickV6(float dt)
        {
            if (hero == null) return;
            for (int i = pendingStrikes.Count - 1; i >= 0; i--) if (pendingStrikes[i].At <= time) { PendingStrike s = pendingStrikes[i]; pendingStrikes.RemoveAt(i); Land(s); }
            TickZones(dt);
            if (LegacyCapstones) return;
            float w = StepWeapon; List<Vector2> spots;
            TickMinors(w);
            // Storm: a bolt from the sky on the crowd, then a thunderstorm raining bolts that chain on
            if (Active("static-club", 6) && Due("thunderbolt", BondStepTuning.ThunderboltPeriod) && (spots = Spots(1)).Count > 0)
                Strike("static-club", "storm-thunderbolt", 6, spots[0], BondStepTuning.ThunderboltRadius, w * BondStepTuning.ThunderboltDamage, StepEffect.Stun, .35f, true);
            if (Active("static-club", 8) && Due("thunderstorm", BondStepTuning.StormRainPeriod) && (spots = Spots(4)).Count > 0)
                Strike("static-club", "storm-thunderstorm", 8, spots[Mathf.FloorToInt(time * 7) % spots.Count], BondStepTuning.StormRainRadius, w * BondStepTuning.StormRainDamage, StepEffect.None, .2f, true);
            // Ember: meteors leave lava; a phoenix dives through the crowds
            if (Active("ember", 6) && Due("meteor", BondStepTuning.MeteorPeriod))
            { spots = Spots(BondStepTuning.Meteors); for (int i = 0; i < spots.Count; i++) { Strike("ember", "ember-meteor", 6, spots[i], BondStepTuning.MeteorRadius, w * BondStepTuning.MeteorDamage, StepEffect.Burn, .5f + i * .15f); patches.Add(new EmberPatch { Position = spots[i], Until = time + .5f + BondTuning.EmberSeconds, Source = bestFriend ?? hero, Weapon = w }); EmbersCreated?.Invoke(spots[i], BondTuning.EmberSeconds); } }
            if (Active("ember", 8) && Due("phoenix", BondStepTuning.PhoenixDivePeriod) && (spots = Spots(1)).Count > 0)
            { Vector2 across = (spots[0] - hero.Position).sqrMagnitude > .01f ? (spots[0] - hero.Position).normalized : Vector2.right; LineStrike("ember", "phoenix-dive", 8, spots[0] - across * 5, spots[0] + across * 5, .9f, w * BondStepTuning.PhoenixDiveDamage, StepEffect.Burn, .3f); }
            // Frost: ice spikes freeze a crowd; a blizzard slows everything and drops hail
            if (Active("frost", 6) && Due("ice-spikes", BondStepTuning.IceSpikePeriod) && (spots = Spots(1)).Count > 0)
                Strike("frost", "frost-ice-spikes", 6, spots[0], BondStepTuning.IceSpikeRadius, w * BondStepTuning.IceSpikeDamage, StepEffect.Freeze, .4f);
            if (Active("frost", 8) && Due("hail", BondStepTuning.HailPeriod) && (spots = Spots(1)).Count > 0)
                Strike("frost", "frost-blizzard-hail", 8, spots[0], BondStepTuning.HailRadius, w * BondStepTuning.HailDamage, StepEffect.Freeze, .6f);
            // Tide: a whirlpool drags a crowd in and bursts; in the rain the leviathan sweeps across the arena
            if (Active("tidal", 6) && Due("whirlpool", BondStepTuning.WhirlpoolPeriod) && (spots = Spots(1)).Count > 0)
                Zone("tidal", "tide-whirlpool", 6, spots[0], BondStepTuning.WhirlpoolRadius, BondStepTuning.WhirlpoolSeconds, 0, BondTuning.GravityPull * 2, StepEffect.Soak, w * BondStepTuning.WhirlpoolDamage, StepEffect.Soak);
            if (Active("tidal", 8))
            {
                if (Due("rain", 1)) foreach (var pair in enemies) if (pair.Key.Alive) pair.Value.SoakedUntil = time + BondTuning.SoakSeconds;
                if (Due("leviathan", BondStepTuning.LeviathanPeriod) && (spots = Spots(1)).Count > 0)
                    LineStrike("tidal", "tide-leviathan", 8, spots[0] - Vector2.right * BondStepTuning.LeviathanReach, spots[0] + Vector2.right * BondStepTuning.LeviathanReach, 1.5f, w * BondStepTuning.LeviathanDamage, StepEffect.Knock, .6f);
            }
            // Windborne: a tornado wanders through the crowds; a gale raises three at once
            if (Active("windborne", 6) && Due("tornado", BondStepTuning.TornadoPeriod) && (spots = Spots(1)).Count > 0)
                Zone("windborne", "wind-tornado", 6, spots[0], BondStepTuning.TornadoRadius, BondStepTuning.TornadoSeconds, w * BondStepTuning.TornadoTick, BondTuning.GravityPull, StepEffect.None, 0, StepEffect.None, BondStepTuning.TornadoSpeed);
            if (Active("windborne", 8) && Due("gale", BondStepTuning.GalePeriod))
            { spots = Spots(BondStepTuning.GaleTornadoes); foreach (Vector2 spot in spots) Zone("windborne", "wind-tornado", 8, spot, BondStepTuning.TornadoRadius, BondStepTuning.TornadoSeconds, w * BondStepTuning.TornadoTick, BondTuning.GravityPull, StepEffect.None, 0, StepEffect.None, BondStepTuning.TornadoSpeed); }
            // Garden: the treant slams a crowd; the sacred grove's roots erupt all over the arena
            if (Active("orchard", 6) && Due("treant", BondStepTuning.TreantSlamPeriod) && (spots = Spots(1)).Count > 0)
                Strike("orchard", "treant-slam", 6, spots[0], BondStepTuning.TreantSlamRadius, w * BondStepTuning.TreantSlamDamage, StepEffect.Root, .45f);
            if (Active("orchard", 8) && Due("roots", BondStepTuning.RootEruptionPeriod))
            { spots = Spots(BondStepTuning.RootEruptions); for (int i = 0; i < spots.Count; i++) Strike("orchard", "root-eruption", 8, spots[i], BondStepTuning.RootEruptionRadius, w * BondStepTuning.RootEruptionDamage, StepEffect.Root, .35f + i * .12f); }
            // Venom: a flask of poison bursts into a cloud; plague mist poisons everything
            if (Active("venom", 6) && Due("flask", BondStepTuning.FlaskPeriod) && (spots = Spots(1)).Count > 0)
            { Strike("venom", "venom-flask", 6, spots[0], BondStepTuning.FlaskRadius, w * BondStepTuning.FlaskDamage, StepEffect.Poison, .5f, false, hero.Position); Zone("venom", "venom-cloud", 6, spots[0], BondStepTuning.FlaskRadius, BondStepTuning.FlaskCloudSeconds, 0, 0, StepEffect.Poison, 0, StepEffect.None); }
            if (Active("venom", 8) && Due("plague-mist", BondStepTuning.PlagueMistPeriod))
                foreach (var pair in enemies) if (pair.Key.Alive) AddPoison(pair.Key, bestFriend ?? hero, w, 1, true);
            // Starlight: a shower of stars; a starry night with stars falling all wave and a giant comet
            if (Active("astral", 6) && Due("star-shower", BondStepTuning.StarShowerPeriod))
            { spots = Spots(BondStepTuning.ShowerStars); for (int i = 0; i < spots.Count; i++) Strike("astral", "star-shower", 6, spots[i], BondStepTuning.StarRadius, w * BondStepTuning.StarDamage, StepEffect.None, .4f + i * .12f); }
            if (Active("astral", 8))
            {
                if (Due("starry-night", BondStepTuning.StarryNightPeriod) && (spots = Spots(6)).Count > 0)
                    Strike("astral", "star-fall", 8, spots[Mathf.FloorToInt(time * 5) % spots.Count], BondStepTuning.StarRadius, w * BondStepTuning.StarryNightDamage, StepEffect.None, .35f);
                if (Due("comet", BondStepTuning.CometPeriod) && (spots = Spots(1)).Count > 0)
                    Strike("astral", "star-comet", 8, spots[0], BondStepTuning.CometRadius, w * BondStepTuning.CometDamage, StepEffect.Knock, .9f);
            }
            // Dusk: a shadow pit drags a crowd down and finishes the weak
            if (Active("dusk", 6) && Due("shadow-pit", BondStepTuning.ShadowPitPeriod) && (spots = Spots(1)).Count > 0)
                Zone("dusk", "dusk-pit", 6, spots[0], BondStepTuning.ShadowPitRadius, BondStepTuning.ShadowPitSeconds, 0, BondTuning.GravityPull, StepEffect.None, w * BondStepTuning.ShadowPitDamage, StepEffect.Execute);
            // Carnival: confetti shells over the crowds; the parade's grand finale makes every friend cast at once
            if (Active("oddity", 6) && Due("confetti", BondStepTuning.ConfettiPeriod))
            { spots = Spots(BondStepTuning.ConfettiShells); for (int i = 0; i < spots.Count; i++) Strike("oddity", "carnival-confetti", 6, spots[i], BondStepTuning.ConfettiRadius, w * BondStepTuning.ConfettiDamage, StepEffect.None, .5f + i * .15f, false, hero.Position); }
            if (Active("oddity", 8) && Due("finale", BondStepTuning.FinalePeriod)) AllCast("oddity", "grand-finale");
            // Court: a decree unrolls over a crowd and a giant seal stamps it; the crowned court's fanfare makes every friend cast
            if (Active("regal", 6) && Due("decree", BondStepTuning.DecreePeriod) && (spots = Spots(1)).Count > 0)
                Strike("regal", "court-decree", 6, spots[0], BondStepTuning.DecreeRadius, w * BondStepTuning.DecreeDamage, StepEffect.Stun, 1);
            if (Active("regal", 8) && Due("fanfare", BondStepTuning.FanfarePeriod)) AllCast("regal", "court-fanfare");
            // Archive: a giant book slams shut on a crowd; a flying library rains pages
            if (Active("archive", 6) && Due("book", BondStepTuning.BookPeriod) && (spots = Spots(1)).Count > 0)
                Strike("archive", "archive-giant-book", 6, spots[0], BondStepTuning.BookRadius, w * BondStepTuning.BookDamage, StepEffect.Stun, .8f);
            if (Active("archive", 8) && Due("pages", BondStepTuning.PagePeriod) && (spots = Spots(4)).Count > 0)
                Strike("archive", "archive-pages", 8, spots[Mathf.FloorToInt(time * 3) % spots.Count], BondStepTuning.PageRadius, w * BondStepTuning.PageDamage, StepEffect.None, .3f, false, hero.Position);
            // Lantern: the lighthouse beam turns around the hero and burns what it sweeps
            if (Active("lantern-guild", 8))
            {
                lighthouseAngle += dt * BondStepTuning.LighthouseTurn;
                if (Due("lighthouse", BondStepTuning.LighthouseTick))
                {
                    Vector2 end = hero.Position + new Vector2(Mathf.Cos(lighthouseAngle), Mathf.Sin(lighthouseAngle)) * BondStepTuning.LighthouseReach;
                    foreach (var pair in enemies) if (pair.Key.Alive && CompanionSkillRuntime.Contains(SkillShape.Line, hero.Position, end, BondStepTuning.LighthouseWidth, pair.Key.Position)) Damage(pair.Key, bestFriend ?? hero, w * BondStepTuning.LighthouseDamage, "lighthouse");
                }
            }
            // Stone: a boulder rolls through a crowd; the colossus stomps
            if (Active("stone", 6) && Due("boulder", BondStepTuning.BoulderPeriod) && (spots = Spots(1)).Count > 0)
            { Vector2 across = (spots[0] - hero.Position).sqrMagnitude > .01f ? (spots[0] - hero.Position).normalized : Vector2.right; LineStrike("stone", "stone-boulder", 6, spots[0] - across * BondStepTuning.BoulderReach, spots[0] + across * BondStepTuning.BoulderReach, BondStepTuning.BoulderWidth, w * BondStepTuning.BoulderDamage, StepEffect.Knock, .5f); }
            if (Active("stone", 8) && Due("stomp", BondStepTuning.StompPeriod) && (spots = Spots(1)).Count > 0)
                Strike("stone", "colossus-stomp", 8, spots[0], BondStepTuning.StompRadius, w * BondStepTuning.StompDamage, StepEffect.Stun, .3f);
            // Clockwork: a giant pendulum swings across the arena
            if (Active("clock", 8) && Due("pendulum", BondStepTuning.PendulumPeriod) && (spots = Spots(1)).Count > 0)
                LineStrike("clock", "clock-pendulum", 8, spots[0] - Vector2.right * BondStepTuning.PendulumReach, spots[0] + Vector2.right * BondStepTuning.PendulumReach, BondStepTuning.PendulumWidth, w * BondStepTuning.PendulumDamage, StepEffect.Stun, .5f);
            // Hearth: a dish to eat; a hot pot boiling over a crowd; the grand chef's pies and a feast
            if (Active("hearth") && Due("dish", BondStepTuning.DishPeriod)) Spawn(new BondSpawnRequest(BondSpawnKind.Dish, hero.Position + new Vector2(Mathf.Cos(time) * 2.5f, Mathf.Sin(time) * 2.5f)));
            if (Active("hearth", 6) && Due("hot-pot", BondStepTuning.HotPotPeriod) && (spots = Spots(1)).Count > 0)
            { Zone("hearth", "hearth-hot-pot", 6, spots[0], BondStepTuning.HotPotRadius, BondStepTuning.HotPotSeconds, w * BondStepTuning.HotPotTick, 0, StepEffect.Burn, 0, StepEffect.None); for (int i = 0; i < BondStepTuning.PotDishes; i++) Spawn(new BondSpawnRequest(BondSpawnKind.Dish, spots[0] + new Vector2(i * 2 - 1, 0))); }
            if (Active("hearth", 8))
            {
                if (Due("pie", BondStepTuning.PiePeriod) && (spots = Spots(3)).Count > 0)
                    Strike("hearth", "chef-pie", 8, spots[Mathf.FloorToInt(time) % spots.Count], BondStepTuning.PieRadius, w * BondStepTuning.PieDamage, StepEffect.Burn, .45f, false, hero.Position);
                if (Due("feast", BondStepTuning.FeastPeriod))
                { TeamRequest(BondSupportKind.DamageBuff, BondStepTuning.FeastBoost, BondStepTuning.FeastSeconds, "hearth-feast"); TeamRequest(BondSupportKind.Speed, BondStepTuning.FeastBoost, BondStepTuning.FeastSeconds, "hearth-feast"); Moment("hearth", "hearth-feast", 8, BondMomentKind.Weather, hero.Position, hero.Position, 0, 0, BondStepTuning.FeastSeconds); }
            }
            // Bulwark: a fortress dome; every half second while it stands a shield bolt flies at a crowd
            if (Active("bulwark", 8))
            {
                if (Due("fortress", BondStepTuning.FortressPeriod)) { fortressUntil = time + BondStepTuning.FortressSeconds; Moment("bulwark", "bulwark-fortress", 8, BondMomentKind.Weather, hero.Position, hero.Position, 0, 0, BondStepTuning.FortressSeconds); }
                if (time < fortressUntil && Due("shield-bolt", BondStepTuning.FortressBolt) && (spots = Spots(4)).Count > 0)
                    Strike("bulwark", "bulwark-shield-bolt", 8, spots[Mathf.FloorToInt(time * 4) % spots.Count], .8f, w * BondStepTuning.FortressDamage, StepEffect.Knock, .25f, false, hero.Position);
            }
            // Spirit: a swarm of spirits dives onto three crowds; the reaper sweeps its scythe
            if (Active("spirit", 6) && Due("swarm", BondStepTuning.SwarmPeriod))
            { spots = Spots(BondStepTuning.SwarmSpirits); for (int i = 0; i < spots.Count; i++) Strike("spirit", "spirit-swarm", 6, spots[i], BondStepTuning.SwarmRadius, w * BondStepTuning.SwarmDamage, StepEffect.None, .4f + i * .15f, false, hero.Position); }
            if (Active("spirit", 8) && Due("reaper", BondStepTuning.ReaperPeriod) && (spots = Spots(1)).Count > 0)
                Strike("spirit", "spirit-reaper", 8, spots[0], BondStepTuning.ReaperRadius, w * BondStepTuning.ReaperDamage, StepEffect.Execute, .5f);
            // Melody: a giant war drum booms three times on a crowd; notes rain on the beat
            if (Active("melody", 6) && Due("drum", BondStepTuning.DrumPeriod) && (spots = Spots(1)).Count > 0)
                for (int i = 0; i < BondStepTuning.DrumBeats; i++) Strike("melody", "melody-war-drum", 6, spots[0], BondStepTuning.DrumRadius, w * BondStepTuning.DrumDamage, StepEffect.Stun, .4f + i * .4f);
            if (Active("melody", 8) && Due("notes", BondStepTuning.NoteRainPeriod))
            { spots = Spots(BondStepTuning.NoteSpots); for (int i = 0; i < spots.Count; i++) Strike("melody", "melody-notes", 8, spots[i], BondStepTuning.NoteRadius, w * BondStepTuning.NoteDamage, StepEffect.None, .3f + i * .1f); }
            // Gravity: under heavy gravity a small planet falls
            if (Active("gravity", 8) && Due("planet", BondStepTuning.PlanetPeriod) && (spots = Spots(1)).Count > 0)
                Strike("gravity", "gravity-orb", 8, spots[0], BondStepTuning.PlanetRadius, w * BondStepTuning.PlanetDamage, StepEffect.Knock, 1);
        }

        /// <summary>Minor bonds that used to give only hidden numbers now show on the field: rain of letters (Rain Script),
        /// roots under the crowd while the hero stands still (Root), drifting clouds (Cloud), stomps among the crowd (Monolith).</summary>
        private void TickMinors(float w)
        {
            List<Vector2> spots;
            if (Active("rain-script") && Due("letters", BondStepTuning.LetterPeriod))
            { spots = Spots(Active("rain-script", 4) ? 3 : 1); for (int i = 0; i < spots.Count; i++) Strike("rain-script", "rain-letters", 2, spots[i], BondStepTuning.LetterRadius, w * BondStepTuning.LetterDamage, StepEffect.Soak, .5f + i * .15f); }
            if (Active("root") && still >= BondTuning.StillSeconds && Due("still-roots", BondStepTuning.StillRootPeriod) && (spots = Spots(1)).Count > 0)
                Strike("root", "root-eruption", 2, spots[0], Active("root", 4) ? BondStepTuning.RootEruptionRadius * 1.5f : BondStepTuning.RootEruptionRadius, w * BondStepTuning.RootEruptionDamage * .6f, StepEffect.Root, .35f);
            if (Active("cloud") && Due("cloud", BondStepTuning.CloudPeriod) && (spots = Spots(1)).Count > 0)
                Zone("cloud", "cloud-drift", 2, spots[0], BondStepTuning.CloudRadius, BondStepTuning.CloudSeconds, Active("cloud", 4) ? w * BondStepTuning.CloudRain : 0, 0, StepEffect.Slow, 0, StepEffect.None, BondStepTuning.CloudSpeed);
            if (Active("monolith") && Due("monolith", BondStepTuning.StompPeriod * 1.6f))
                foreach (UnitState friend in friends)
                {
                    if (!friend.Alive || !Has(friend, "monolith")) continue;
                    List<UnitState> near = Nearby(friend, BondStepTuning.MonolithReach);
                    if (near.Count < 3) continue;
                    Strike("monolith", "stone-monolith-stomp", 2, near[0].Position, BondStepTuning.MonolithRadius, Weapon(friend) * BondStepTuning.StompDamage * .6f, StepEffect.Stun, .25f);
                    if (Active("monolith", 4)) Zone("monolith", "stone-monolith-pull", 4, near[0].Position, BondStepTuning.MonolithRadius, 1, 0, BondTuning.GravityPull, StepEffect.None, 0, StepEffect.None);
                }
        }

        /// <summary>A Bastion shield that breaks throws its shards at the crowds.</summary>
        public void OnBastionShieldBroken(UnitState friend)
        {
            if (LegacyCapstones || friend == null || !Active("bastion")) return;
            List<Vector2> spots = Spots(Active("bastion", 4) ? 6 : 3);
            for (int i = 0; i < spots.Count; i++) Strike("bastion", "bastion-shield-shard", 2, spots[i], .8f, Weapon(friend) * BondStepTuning.ShardDamage, StepEffect.None, .3f + i * .06f, false, friend.Position);
        }

        private void AllCast(string bond, string rule)
        {
            foreach (UnitState friend in friends) if (friend.Alive) Request(BondSupportKind.ForceCast, friend, 1, 0, rule);
            Moment(bond, rule, 8, BondMomentKind.Weather, hero.Position, hero.Position, 0, 0, 1.5f);
            List<Vector2> spots = Spots(BondStepTuning.ConfettiShells);
            for (int i = 0; i < spots.Count; i++) Strike(bond, bond == "oddity" ? "carnival-confetti" : "court-seal", 8, spots[i], BondStepTuning.ConfettiRadius, StepWeapon * BondStepTuning.ConfettiDamage, StepEffect.None, .5f + i * .12f, false, hero.Position);
        }

        private void TickZones(float dt)
        {
            for (int z = stepZones.Count - 1; z >= 0; z--)
            {
                StepZone zone = stepZones[z];
                if (zone.Speed > 0)
                {
                    UnitState target = DensestCluster();
                    if (target != null) zone.Position = Vector2.MoveTowards(zone.Position, target.Position, zone.Speed * dt);
                }
                bool tick = time >= zone.NextTick; if (tick) zone.NextTick += BondStepTuning.ZoneTick;
                foreach (var pair in enemies)
                {
                    if (!pair.Key.Alive || (pair.Key.Position - zone.Position).sqrMagnitude > zone.Radius * zone.Radius) continue;
                    if (zone.Pull > 0) pair.Key.Position = Vector2.MoveTowards(pair.Key.Position, zone.Position, zone.Pull * dt);
                    if (tick) { if (zone.TickDamage > 0) Damage(pair.Key, bestFriend ?? hero, zone.TickDamage, zone.Rule); Affect(pair.Key, zone.Effect, Vector2.zero); }
                }
                if (time >= zone.Until)
                {
                    stepZones.RemoveAt(z);
                    if (zone.EndDamage > 0 || zone.EndEffect != StepEffect.None)
                    { pendingStrikes.Add(new PendingStrike { Rule = zone.Rule + "-burst", To = zone.Position, Radius = zone.Radius, Damage = zone.EndDamage, Effect = zone.EndEffect, At = time }); Moment(zone.Bond, zone.Rule + "-burst", 6, BondMomentKind.Strike, zone.Position, zone.Position, zone.Radius, 0); }
                    continue;
                }
                stepZones[z] = zone;
            }
        }
        /// <summary>Where each lasting zone stands now (a tornado wanders): the presentation follows it.</summary>
        public int StepZoneCount => stepZones.Count;
        public bool StepZoneAt(int index, out string rule, out Vector2 position, out float radius)
        {
            rule = null; position = default; radius = 0; if (index < 0 || index >= stepZones.Count) return false;
            rule = stepZones[index].Rule; position = stepZones[index].Position; radius = stepZones[index].Radius; return true;
        }
        public float LighthouseAngle => lighthouseAngle;
    }
}
