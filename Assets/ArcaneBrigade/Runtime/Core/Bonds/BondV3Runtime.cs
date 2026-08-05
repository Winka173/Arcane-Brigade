using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class BondRuntime
    {
        private BondRunProgress runProgress = new BondRunProgress();
        public BondRunProgress RunProgress => runProgress;
        private HashSet<string> recordedSpecies => runProgress.Species;
        private readonly Dictionary<int, int> personalCasts = new Dictionary<int, int>(BondTuning.TeamCapacity);
        private readonly List<BondSupportRequest> supportRequests = new List<BondSupportRequest>(BondTuning.RequestCapacity);
        public IReadOnlyList<BondSupportRequest> SupportRequests => supportRequests;
        public void ClearSupportRequests() => supportRequests.Clear();
        public event Action<BondSupportRequest> SupportRequested;
        private float feastAt, riptideAt, weatherAt, galeAt, earthquakeAt, stopAt, wallAt, chorusAt, singularityAt, shieldAt, dashAt;
        private readonly List<float> spiritDeadlines = new List<float>(BondTuning.TeamCapacity);
        private UnitState singularityCenter;
        private float singularityUntil, singularityPulseAt;
        private float feastUntil, galeUntil, beatUntil, overclockUntil, stopUntil, lastMoveAt, momentum, rapidAt, clusterAt;
        private int rapidStacks, turrets, souls, v3HitCount, echoCasts, jackpot;
        private int runWaves { get => runProgress.Waves; set => runProgress.Waves = value; }
        private int growthWaves { get => runProgress.GrowthWaves; set => runProgress.GrowthWaves = value; }
        private uint randomState = BondTuning.RandomSeed;
        public BondEdict Edict { get; private set; }
        private BondLaw firstLaw { get => runProgress.FirstLaw; set => runProgress.FirstLaw = value; }
        private BondLaw secondLaw { get => runProgress.SecondLaw; set => runProgress.SecondLaw = value; }
        private bool doubledDown { get => runProgress.DoubledDown; set => runProgress.DoubledDown = value; }
        public BondRecordedBonus RecordedBonus => runProgress.RecordedBonus;
        public float ConstellationRange => Active("astral", 6) && Constellation == BondConstellation.Archer ? BondTuning.ChoiceBonus : 0;
        public float ConstellationHealth => Active("astral", 6) && Constellation == BondConstellation.Bear ? BondTuning.ChoiceBonus : 0;
        public float ConstellationCompanionDamage => Active("astral", 6) && Constellation == BondConstellation.Twins ? BondTuning.ChoiceBonus : 0;
        public float EdictDamage => Active("regal") && Edict == BondEdict.Damage ? BondTuning.ChoiceBonus : 0;
        public float EdictArmor => Active("regal") && Edict == BondEdict.Armor ? BondTuning.RootArmor : 0;
        public float EdictSkillFill => Active("regal") && Edict == BondEdict.Haste ? BondTuning.ChoiceBonus : 0;
        private float RandomFraction() { unchecked { randomState = randomState * BondTuning.RandomMultiplier + BondTuning.RandomIncrement; } return (randomState & BondTuning.RandomMask) / (float)BondTuning.RandomMask; }
        public BondRunProgress CaptureProgress() => runProgress;
        public void RestoreProgress(BondRunProgress progress)
        { if (progress != null) runProgress = progress; Records = recordedSpecies.Count; }
        public void ChooseEdict(BondEdict edict) { Edict = edict; }
        public bool ChooseLaw(BondLaw law, bool second = false)
        { if (!Active("regal", second ? BondTuning.FourthStep : BondTuning.ThirdStep)) return false; if (second) secondLaw = law; else firstLaw = law; return true; }
        public void CashOut(bool doubleDown)
        { if (!Active("oddity", 6)) return; doubledDown = doubleDown; if (!doubleDown) Request(BondSupportKind.Coin, hero, BondTuning.JackpotCoins, 0, "cash-out"); }
        private void TriggerLaws(BondLawCause cause, UnitState subject)
        { if (!Active("regal", 6)) return; TriggerLaw(firstLaw, cause, subject); if (Active("regal", 8)) TriggerLaw(secondLaw, cause, subject); }
        private void TriggerLaw(BondLaw law, BondLawCause cause, UnitState subject)
        { if (law.Cause != cause) return; if (law.Effect == BondLawEffect.Heal) Request(BondSupportKind.Heal, hero, BondTuning.SpiritHeal, 0, "court-law-heal"); else if (law.Effect == BondLawEffect.Shield) TeamRequest(BondSupportKind.Shield, BondTuning.SpiritHeal, 0, "court-law-shield"); else if (law.Effect == BondLawEffect.Burst && subject != null && subject.Faction == Faction.Enemy) Area(subject, bestFriend, Weapon(bestFriend) * BondTuning.CopyStrength, "court-law-burst"); }
        public float WeatherDamageBonus(UnitState friend)
        { if (!Active("tidal", 6) || friend == null) return 0; if (Has(friend, "tidal")) return BondTuning.TideWeatherBonus; string id = friend.Definition.Id ?? ""; int hash = 0; foreach (char ch in id) hash = (hash + ch) % BondTuning.SecondStep; return hash == (int)Weather ? BondTuning.WeatherBonus : 0; }
        private float runDamage { get => runProgress.Damage; set => runProgress.Damage = value; }
        private bool exhibitUsed { get => runProgress.ExhibitUsed; set => runProgress.ExhibitUsed = value; }
        private bool hatUsed { get => runProgress.HatUsed; set => runProgress.HatUsed = value; }
        private bool lastStandUsed, revenantUsed, treeChosen, finaleUsed;
        public BondWeather Weather { get; private set; }
        public BondConstellation Constellation { get => runProgress.Constellation; private set => runProgress.Constellation = value; }
        public int Souls => souls;
        public int Growth => growthWaves;
        public int Beats { get; private set; }
        public int Mass { get; private set; }
        public float Guard { get; private set; }
        public float RunHealthBonus => growthWaves * BondTuning.GrowthPerWave + ConstellationHealth;
        public float RunDamageBonus => runDamage + growthWaves * BondTuning.GrowthPerWave;
        public float SkillFillBonus => EdictSkillFill + (Active("clock") ? BondTuning.ClockFill : 0) + (Active("clock", 8) && time < overclockUntil ? 1 : 0);
        public float ReviveTimeReduction => Active("spirit", 4) ? BondTuning.SpiritReviveReduction : 0;
        public bool SpiritRealm => Active("spirit", 8) && time < BondTuning.RealmSeconds;
        public float FriendMoveSpeedBonus => time < galeUntil ? BondTuning.GaleSpeed : 0;
        public float DodgeChance => Active("cloud") ? (Active("cloud", 4) ? BondTuning.DodgeHigh : BondTuning.Dodge) : 0;
        public int BallBounces => Active("ball") ? (Active("ball", 4) ? BondTuning.SecondStep / BondTuning.FirstStep : 1) : 0;
        public float BounceDamageRetention => Active("ball", 4) ? BondTuning.BallRetention : 1;
        public int BoomerangReturns => Active("paper-wings", 4) ? (Active("paper-wings", 6) ? BondTuning.BoomerangHighReturns : 1) : 0;
        public float ProjectileDistanceBonus => Active("paper-wings") ? BondTuning.PaperDistance : 0;
        public float ArmorIgnored => Active("bludgeoner") ? (Active("bludgeoner", 4) ? BondTuning.ArmorIgnoreHigh : BondTuning.ArmorIgnore) : 0;
        private float MealBonus => Active("hearth") ? BondTuning.MealBonus * Mathf.Clamp01(1 - time / BondTuning.MealFade) * (Active("hearth", 8) ? BondTuning.SecondStep / BondTuning.FirstStep : 1) : 0;
        private float V3AttackSpeed => MealBonus + (time < feastUntil ? BondTuning.FeastSpeed : 0) + (time < beatUntil ? BondTuning.BeatSpeed : 0) + (Active("rapidfire") && time - rapidAt < BondTuning.RapidIdle ? rapidStacks * (Active("rapidfire", 4) ? BondTuning.RapidHighSpeed : BondTuning.RapidSpeed) : 0);
        private void Request(BondSupportKind kind, UnitState subject, float amount, float duration, string rule, float radius = 0)
        {
            var request = new BondSupportRequest(kind, subject, amount, duration, radius, rule);
            supportRequests.Add(request); SupportRequested?.Invoke(request);
        }
        private void TeamRequest(BondSupportKind kind, float fraction, float duration, string rule)
        {
            if (hero != null) Request(kind, hero, fraction, duration, rule);
            foreach (UnitState friend in friends) if (friend.Alive) Request(kind, friend, fraction, duration, rule);
        }
        private void ResetV3()
        {
            supportRequests.Clear(); personalCasts.Clear(); spiritDeadlines.Clear(); singularityCenter = null; nearbyCursor = 0;
            feastAt = Active("hearth", 8) ? BondTuning.FeastHighPeriod : BondTuning.FeastPeriod;
            riptideAt = BondTuning.RiptidePeriod; weatherAt = BondTuning.WeatherPeriod; galeAt = BondTuning.GalePeriod;
            earthquakeAt = BondTuning.EarthquakePeriod; stopAt = BondTuning.ClockStopPeriod; wallAt = BondTuning.WallPeriod;
            chorusAt = BondTuning.ChorusPeriod; singularityAt = BondTuning.SingularityPeriod; shieldAt = BondTuning.ShieldPeriod;
            dashAt = Active("shoe", 4) ? BondTuning.DashHighPeriod : BondTuning.DashPeriod;
            feastUntil = galeUntil = beatUntil = overclockUntil = stopUntil = lastMoveAt = momentum = rapidAt = clusterAt = 0;
            rapidStacks = turrets = souls = v3HitCount = echoCasts = jackpot = Beats = Mass = 0; Guard = 0;
            lastStandUsed = revenantUsed = treeChosen = finaleUsed = celestialUsed = false; Weather = BondWeather.Rain; Records = recordedSpecies.Count;
            Edict = BondEdict.Damage;
            if (Active("bulwark")) TeamRequest(BondSupportKind.Shield, BondTuning.BulwarkShield, 0, "bulwark-shield");
            if (Active("bastion")) TeamRequest(BondSupportKind.Shield, Active("bastion", 4) ? BondTuning.BastionHighShield : BondTuning.BastionShield, 0, "bastion-shield");
            if (Active("shield")) RenewMinorShields();
            if (Active("hearth", 8) && hero != null) Spawn(new BondSpawnRequest(BondSpawnKind.Chef, hero.Position, damage: Weapon(bestFriend)));
            if (Active("tidal", 8) && hero != null) { Spawn(new BondSpawnRequest(BondSpawnKind.Leviathan, hero.Position, damage: Weapon(bestFriend))); WholeArena("leviathan", Weapon(bestFriend)); }
            if (Active("windborne", 6) && hero != null) Spawn(new BondSpawnRequest(BondSpawnKind.WindBeast, hero.Position, damage: Weapon(bestFriend)));
            if (Active("orchard", 4) && hero != null)
                for (int i = 0; i < BondTuning.SproutLimit; i++) Spawn(new BondSpawnRequest(i == 0 && Active("orchard", 6) ? BondSpawnKind.Treant : BondSpawnKind.Sprout, hero.Position, damage: Weapon(bestFriend)));
            if (Active("stone", 8) && hero != null) Spawn(new BondSpawnRequest(BondSpawnKind.Colossus, hero.Position, damage: Weapon(bestFriend)));
            if (Active("melody", 8) && hero != null) for (int i = 0; i < BondTuning.OrchestraCount; i++) Spawn(new BondSpawnRequest(BondSpawnKind.Orchestra, hero.Position, sourceId: i, damage: Weapon(bestFriend)));
            if (Active("spirit", 8)) TeamRequest(BondSupportKind.SpiritRealm, 1, BondTuning.RealmSeconds, "spirit-realm");
            if (Active("regal")) Request(BondSupportKind.Choice, hero, BondTuning.SmallChoiceCount, 0, "court-edict");
            if (Active("regal", 4) && bestFriend != null) Request(BondSupportKind.Crown, HighestTierFriend(), 1, BondTuning.WaveBuffSeconds, "crown");
            if (Active("regal", 6)) Request(BondSupportKind.Law, hero, Active("regal", 8) ? BondTuning.SecondStep / BondTuning.FirstStep : 1, 0, "court-laws");
            if (Active("orchard", 8)) Request(BondSupportKind.Choice, hero, BondTuning.SmallChoiceCount, 0, "great-tree");
            if (Active("oddity", 6) && runWaves > 0 && runWaves % BondTuning.CashoutWaves == 0) Request(BondSupportKind.Choice, hero, BondTuning.SecondStep / BondTuning.FirstStep, 0, "cash-out-or-double-down");
            foreach (List<UnitState> buffer in nearbyBuffers) if (buffer.Capacity < enemies.Count) buffer.Capacity = enemies.Count;
        }
        private UnitState HighestTierFriend()
        {
            UnitState highest = null;
            foreach (UnitState friend in friends) if (highest == null || friend.Definition.CombatTier > highest.Definition.CombatTier || friend.Definition.CombatTier == highest.Definition.CombatTier && friend.Id < highest.Id) highest = friend;
            return highest;
        }
        private void WholeArena(string rule, float amount)
        { foreach (var pair in enemies) if (pair.Key.Alive) Damage(pair.Key, bestFriend, amount, rule); if (hero != null) Emit(rule, hero, amount); }
        private UnitState DensestCluster()
        {
            UnitState best = null; int count = -1;
            foreach (var pair in enemies) if (pair.Key.Alive)
            {
                int nearby = 0; foreach (var other in enemies) if (other.Key.Alive && (other.Key.Position - pair.Key.Position).sqrMagnitude <= BondTuning.ClumpRadius * BondTuning.ClumpRadius) nearby++;
                if (nearby > count || nearby == count && (best == null || pair.Key.Id < best.Id)) { best = pair.Key; count = nearby; }
            }
            return best;
        }
        private void RenewMinorShields()
        {
            foreach (UnitState friend in friends) if (friend.Alive && Has(friend, "shield"))
            {
                Request(BondSupportKind.Shield, friend, BondTuning.MinorShield, 0, "shield-member");
                if (!Active("shield", 4)) continue;
                UnitState nearest = hero != null && hero.Alive ? hero : null; float square = nearest == null ? float.PositiveInfinity : (nearest.Position - friend.Position).sqrMagnitude;
                foreach (UnitState other in friends) if (other != friend && other.Alive && (other.Position - friend.Position).sqrMagnitude < square) { nearest = other; square = (other.Position - friend.Position).sqrMagnitude; }
                if (nearest != null) Request(BondSupportKind.Shield, nearest, BondTuning.MinorShield, 0, "shield-neighbor");
            }
        }
        private float ModifyV3(UnitState attacker, UnitState enemy, float amount)
        {
            float bonus = RunDamageBonus + MealBonus + momentum + EdictDamage + WeatherDamageBonus(attacker) + (attacker.Faction == Faction.Companion ? ConstellationCompanionDamage : 0);
            if (State(enemy)?.SoakedUntil > time) bonus += BondTuning.SoakVulnerability;
            if (Active("archive") && recordedSpecies.Contains(enemy.Definition.Id ?? "")) bonus += BondTuning.RecordBonus;
            if (Active("sniper")) bonus += Mathf.Min((enemy.Position - attacker.Position).magnitude * BondTuning.SniperPerUnit, Active("sniper", 4) ? BondTuning.SniperHighCap : BondTuning.SniperCap);
            if (Active("gravity", 4) && State(enemy) != null && State(enemy).ClusterNeighbors >= BondTuning.ClusterMinimum) bonus += BondTuning.ClumpBonus;
            if (Active("spirit", 8)) bonus += souls / BondTuning.SoulsPerBonus * BondTuning.SoulsDamage;
            return amount * (1 + bonus);
        }
        private void HitV3(UnitState attacker, UnitState enemy, float amount, bool critical)
        {
            v3HitCount++; if (critical) TriggerLaws(BondLawCause.Critical, enemy);
            if (Member(attacker, "hearth") && Active("hearth", 6) && hero != null) Request(BondSupportKind.Heal, hero, amount * BondTuning.HearthLeech / Mathf.Max(1, hero.Definition.Health), 0, "hearth-leech");
            if (Member(attacker, "tidal") && Every("tide-splash", BondTuning.TideEvery))
            {
                foreach (UnitState next in Nearby(enemy, BondTuning.BurstRadius, true)) State(next).SoakedUntil = time + BondTuning.SoakSeconds;
                Emit("tide-splash", enemy, BondTuning.SoakSeconds);
            }
            if (Member(attacker, "gravity") && hero != null) { enemy.Position += (hero.Position - enemy.Position).normalized * BondTuning.GravityPull; Mass++; Emit("gravity-pull", enemy, BondTuning.GravityPull); }
            if (Active("singularity") && Every("minor-singularity", Active("singularity", 4) ? BondTuning.SingularityHighEvery : BondTuning.SingularityEvery))
                foreach (UnitState next in Nearby(enemy, BondTuning.BurstRadius)) { next.Position += (enemy.Position - next.Position).normalized * BondTuning.GravityPull; Mass++; }
            if (Active("bladenado")) Area(enemy, attacker, amount * (Active("bladenado", 4) ? BondTuning.BladeHighSplash : BondTuning.BladeSplash), "bladenado");
            if (Active("rapidfire")) { rapidStacks = Mathf.Min(BondTuning.RapidStackCap, time - rapidAt >= BondTuning.RapidIdle ? 1 : rapidStacks + 1); rapidAt = time; }
            if (Active("desperado") && Every("desperado", Active("desperado", 4) ? BondTuning.DesperadoHighEvery : BondTuning.DesperadoEvery))
            { Area(enemy, attacker, Weapon(attacker) * BondTuning.DesperadoDamage * BondTuning.DesperadoShots, "desperado-volley"); }
            if (Active("archive") && recordedSpecies.Add(enemy.Definition.Id ?? ""))
            {
                Records = recordedSpecies.Count; if (Active("archive", 4)) runDamage += BondTuning.RecordGrowth;
                Emit("species-recorded", enemy, Records);
                if (RecordedBonus == BondRecordedBonus.Coin) Request(BondSupportKind.Coin, hero, 1, 0, "archive-record-coin");
                else if (RecordedBonus == BondRecordedBonus.PageCopy) Request(BondSupportKind.Page, enemy, BondTuning.CopyStrength, 0, "archive-record-copy");
                else runDamage += BondTuning.RecordGrowth;
            }
            if (critical && Active("astral"))
            {
                if (Active("astral", 8) || RandomFraction() < BondTuning.StarChance) Area(enemy, attacker, Weapon(attacker) * BondTuning.StarDamage, "falling-star");
                if (Active("astral", 4) && Every("star-reroll", BondTuning.StarRerollEvery)) Request(BondSupportKind.Reroll, hero, 1, 0, "wish-reroll");
                if (Active("astral", 8) && !celestialUsed) { celestialUsed = true; TeamRequest(BondSupportKind.Transform, 1, BondTuning.CelestialSeconds, "celestial"); }
            }
            if (Active("oddity") && v3HitCount % BondTuning.DieEvery == 0)
            {
                int face = Mathf.Min(BondTuning.DieFaces - 1, (int)(RandomFraction() * BondTuning.DieFaces));
                if (face == 0) Area(enemy, attacker, Weapon(attacker), "carnival-burst");
                else if (face == 1) Request(BondSupportKind.Coin, hero, 1, 0, "carnival-coin");
                else if (face == 2) StunArea(enemy, BondTuning.HornStun, "carnival-slow", false);
                else if (face == 3) TeamRequest(BondSupportKind.Speed, BondTuning.BeatSpeed, BondTuning.BeatSeconds, "carnival-speed");
                else if (face == 4) Emit("carnival-crit", attacker, BondTuning.ChoiceBonus);
                else Emit("carnival-nothing", attacker);
            }
        }
        private bool celestialUsed;
        private void KillV3(UnitState enemy)
        {
            souls++; TriggerLaws(BondLawCause.Kill, enemy);
            if (Active("archive", 6) && recordedSpecies.Contains(enemy.Definition.Id ?? "")) Request(BondSupportKind.Page, enemy, BondTuning.CopyStrength, 0, "record-page");
            if (Active("unstoppable") && hero != null)
            {
                UnitState cluster = DensestCluster();
                if (cluster != null) foreach (UnitState friend in friends) if (friend.Alive && Has(friend, "unstoppable"))
                { Request(BondSupportKind.Dash, friend, BondTuning.DashDistance, BondTuning.DashSeconds, "unstoppable-charge", BondTuning.BurstRadius); if (Active("unstoppable", 4)) StunArea(cluster, BondTuning.HornStun, "unstoppable-stun", false); }
            }
        }
        private void CastV3(UnitState friend)
        {
            TriggerLaws(BondLawCause.Cast, friend);
            personalCasts.TryGetValue(friend.Id, out int count); personalCasts[friend.Id] = ++count;
            if (Active("multicaster") && count <= (Active("multicaster", 4) ? BondTuning.MulticastsHigh : BondTuning.Multicasts)) Request(BondSupportKind.SkillCopy, friend, 1, 0, "multicaster");
            if (Active("echo") && ++echoCasts % Interval("echo", BondTuning.MinorEchoEvery, BondTuning.MinorEchoFour, BondTuning.MinorEchoSix) == 0) Request(BondSupportKind.SkillCopy, friend, BondTuning.CopyStrength, 0, "echo");
            if (Active("clock", 4) && Ticks % BondTuning.EchoEvery == 0) foreach (UnitState other in friends) if (other.Alive && Has(other, "clock")) Request(BondSupportKind.SkillCopy, other, BondTuning.CopyStrength, 0, "clock-echo");
            if (Active("clock", 8) && Ticks % BondTuning.EchoEvery == 0 && turrets < BondTuning.TurretLimit) { turrets++; Spawn(new BondSpawnRequest(BondSpawnKind.Turret, friend.Position, damage: Weapon(friend))); }
            if (Active("melody") && Ticks % BondTuning.MelodyEvery == 0) { Beats++; beatUntil = time + BondTuning.BeatSeconds * (Active("melody", 6) && still >= BondTuning.StillSeconds ? BondTuning.SecondStep / BondTuning.FirstStep : 1); Emit("beat", friend, beatUntil - time); }
            if (Active("oddity", 4) && ++jackpot >= BondTuning.JackpotCasts) { jackpot = 0; Request(BondSupportKind.Coin, hero, BondTuning.JackpotCoins * (doubledDown ? BondTuning.SecondStep / BondTuning.FirstStep : 1), 0, "jackpot"); TeamRequest(BondSupportKind.Speed, BondTuning.JackpotSpeed, BondTuning.JackpotSeconds, "jackpot-buff"); }
            if (Active("oddity", 8) && !finaleUsed && Applause >= friends.Count) { finaleUsed = true; foreach (UnitState other in friends) if (other.Alive) Request(BondSupportKind.ForceCast, other, 1, 0, "grand-finale"); }
        }
        private float GravitySpeed(UnitState enemy) => time < stopUntil ? 0 : Active("gravity", 8) && hero != null && (enemy.Position - hero.Position).sqrMagnitude <= BondTuning.GravityRadius * BondTuning.GravityRadius ? 1 - BondTuning.GravitySlow : 1;
        private void TickV3(float dt)
        {
            if (hero == null) return;
            if (Moving) { lastMoveAt = time; if (Active("windborne")) momentum = Mathf.Min(BondTuning.MomentumCap, momentum + dt * BondTuning.MomentumPerSecond); }
            else if (time - lastMoveAt >= BondTuning.MomentumIdle) momentum = 0;
            if (Active("hearth", 4)) while (time >= feastAt) { feastAt += Active("hearth", 8) ? BondTuning.FeastHighPeriod : BondTuning.FeastPeriod; feastUntil = time + BondTuning.FeastSeconds; Request(BondSupportKind.Heal, hero, BondTuning.FeastHeal, 0, "feast"); Emit("feast", hero, BondTuning.FeastSeconds); }
            if (Active("tidal", 4)) while (time >= riptideAt) { riptideAt += BondTuning.RiptidePeriod; foreach (var pair in enemies) if (pair.Key.Alive) { pair.Value.SoakedUntil = time + BondTuning.SoakSeconds; pair.Key.Position += (pair.Key.Position - hero.Position).normalized * BondTuning.RiptidePush; } Emit("riptide", hero); }
            if (Active("tidal", 6)) while (time >= weatherAt) { weatherAt += BondTuning.WeatherPeriod; Weather = (BondWeather)(((int)Weather + 1) % BondTuning.SecondStep); Emit("weather", hero, (int)Weather); }
            if (Active("windborne", 8)) while (time >= galeAt) { galeAt += BondTuning.GalePeriod; galeUntil = time + BondTuning.GaleSeconds; foreach (var pair in enemies) if (pair.Key.Alive) pair.Key.Position += (pair.Key.Position - hero.Position).normalized * BondTuning.RiptidePush; Emit("gale", hero, BondTuning.GaleSeconds); }
            if (Active("stone", 6)) while (time >= earthquakeAt) { earthquakeAt += BondTuning.EarthquakePeriod; StunArea(hero, BondTuning.EarthquakeStun, "earthquake", false); }
            if (Active("clock", 6)) while (time >= stopAt) { stopAt += BondTuning.ClockStopPeriod; stopUntil = time + BondTuning.ClockStopSeconds; overclockUntil = time + BondTuning.OverclockSeconds; foreach (var pair in enemies) if (pair.Key.Alive) { pair.Value.StunnedUntil = Mathf.Max(pair.Value.StunnedUntil, stopUntil); pair.Key.NextAttack = Mathf.Max(pair.Key.NextAttack, stopUntil); } Request(BondSupportKind.TimeStop, hero, 1, BondTuning.ClockStopSeconds, "time-stop"); }
            if (Active("bulwark", 6)) while (time >= wallAt) { wallAt += BondTuning.WallPeriod; Request(BondSupportKind.Wall, FriendCluster(), 1, BondTuning.WallSeconds, "bastion-wall", BondTuning.BurstRadius); }
            if (Active("melody", 4)) while (time >= chorusAt) { chorusAt += BondTuning.ChorusPeriod; foreach (UnitState friend in friends) if (friend.Alive) Request(BondSupportKind.SkillCopy, friend, BondTuning.ChorusStrength, 0, "chorus"); }
            if (Active("gravity", 6)) while (time >= singularityAt)
            {
                singularityAt += BondTuning.SingularityPeriod; singularityCenter = DensestCluster();
                if (singularityCenter != null) { singularityUntil = time + BondTuning.SingularitySeconds; singularityPulseAt = time; Request(BondSupportKind.Pull, singularityCenter, BondTuning.GravityPull, BondTuning.SingularitySeconds, "singularity", BondTuning.GravityRadius); }
            }
            if (singularityCenter != null)
            {
                if (time < singularityUntil && time >= singularityPulseAt)
                {
                    singularityPulseAt += BondTuning.PlaguePeriod;
                    foreach (UnitState next in Nearby(singularityCenter, BondTuning.GravityRadius, true)) { next.Position += (singularityCenter.Position - next.Position).normalized * BondTuning.GravityPull; Mass++; Damage(next, bestFriend, Weapon(bestFriend) * BondTuning.SingularityDamage, "singularity-drag"); }
                }
                else if (time >= singularityUntil) { Area(singularityCenter, bestFriend, Weapon(bestFriend), "singularity-collapse"); singularityCenter = null; }
            }
            for (int i = spiritDeadlines.Count - 1; i >= 0; i--) if (spiritDeadlines[i] <= time) spiritDeadlines.RemoveAt(i);
            if (Active("spirit", 4) && spiritDeadlines.Count > 0) Request(BondSupportKind.Heal, hero, spiritDeadlines.Count * BondTuning.SpiritHeal * dt, 0, "spirit-heal");
            if (Active("shield", 6)) while (time >= shieldAt) { shieldAt += BondTuning.ShieldPeriod; RenewMinorShields(); }
            if (Active("shoe") && Moving && time >= dashAt) { dashAt = time + (Active("shoe", 4) ? BondTuning.DashHighPeriod : BondTuning.DashPeriod); Request(BondSupportKind.Dash, hero, BondTuning.DashDistance, BondTuning.DashSeconds, "quicksoles-dash"); if (Active("shoe", 6)) StunArea(hero, BondTuning.HornStun, "dash-shockwave", true); }
            if (Active("mend")) { float rate = (Active("mend", 4) ? BondTuning.MendHighRate : BondTuning.MendRate) * dt; if (hero.Alive && hero.Health < hero.Definition.Health * BondTuning.MendHealthThreshold) Request(BondSupportKind.Heal, hero, rate, 0, "mend"); foreach (UnitState friend in friends) if (friend.Alive && friend.Health < friend.Definition.Health * BondTuning.MendHealthThreshold) Request(BondSupportKind.Heal, friend, rate, 0, "mend"); }
            if (time >= clusterAt) { clusterAt = time + BondTuning.PlaguePeriod; Soak = 0; foreach (var pair in enemies) if (pair.Key.Alive) { if (pair.Value.SoakedUntil > time) Soak++; int count = 0; foreach (var other in enemies) if (other.Key != pair.Key && other.Key.Alive && (other.Key.Position - pair.Key.Position).sqrMagnitude <= BondTuning.ClumpRadius * BondTuning.ClumpRadius) count++; pair.Value.ClusterNeighbors = count; } }
        }
        private UnitState FriendCluster()
        {
            UnitState best = hero; int most = 0;
            foreach (UnitState friend in friends) if (friend.Alive) { int count = 0; foreach (UnitState other in friends) if (other.Alive && (other.Position - friend.Position).sqrMagnitude <= BondTuning.BurstRadius * BondTuning.BurstRadius) count++; if (count > most) { most = count; best = friend; } }
            return best;
        }
        public float MonolithArmor(UnitState friend)
        { if (!Active("monolith")) return 0; int count = 0; foreach (var pair in enemies) if (pair.Key.Alive && (pair.Key.Position - friend.Position).sqrMagnitude <= BondTuning.MonolithRadius * BondTuning.MonolithRadius) count++; return Mathf.Min(count, Active("monolith", 4) ? BondTuning.MonolithHighCap : BondTuning.MonolithCap); }
        public bool IgnoresStunAndPush(UnitState friend) => Member(friend, "unstoppable");
        public void OnReroll() { if (Active("rain-script")) runDamage += Active("rain-script", 4) ? BondTuning.RerollHighGrowth : BondTuning.RerollGrowth; }
        public void OnWaveSurvived() { runWaves++; if (Active("orchard")) growthWaves++; }
        public void ChooseConstellation(BondConstellation choice) { Constellation = choice; }
        public float FirstRecruitDiscount(bool firstRecruit, bool consumeFree = false)
        { if (!firstRecruit || !Active("hat")) return 0; if (Active("hat", 6) && !hatUsed) { if (consumeFree) hatUsed = true; return 1; } return Active("hat", 4) ? BondTuning.HatHighDiscount : BondTuning.HatDiscount; }
        public bool ChooseTreeFruit(BondTreeFruit fruit)
        { if (!Active("orchard", 8) || treeChosen) return false; treeChosen = true; if (fruit == BondTreeFruit.Gold) Request(BondSupportKind.Coin, hero, BondTuning.TreeCoins, 0, "tree-gold"); else if (fruit == BondTreeFruit.Heal) Request(BondSupportKind.Heal, hero, BondTuning.ExhibitHeal, 0, "tree-heal"); else runDamage += BondTuning.ChoiceBonus; return true; }
        public float AdjustFriendDamage(UnitState friend, float amount)
        {
            if (!Active("bulwark", 4) || amount <= 0) return amount;
            UnitState nearest = null; float square = float.PositiveInfinity;
            foreach (UnitState other in friends) if (other != friend && other.Alive && Has(other, "bulwark") && (other.Position - friend.Position).sqrMagnitude < square) { nearest = other; square = (other.Position - friend.Position).sqrMagnitude; }
            if (nearest == null) return amount;
            float share = amount * BondTuning.DamageShare; Guard += share;
            Request(BondSupportKind.DamageShared, nearest, share * (1 - BondTuning.ShareReduction), 0, "bulwark-share"); return amount - share;
        }
        public bool TryPreventFall(UnitState friend)
        {
            if (SpiritRealm) { friend.Health = 1; return true; }
            if (Active("bulwark", 8) && !lastStandUsed) { lastStandUsed = true; friend.Health = 1; Request(BondSupportKind.Untouchable, friend, 1, BondTuning.LastStandSeconds, "last-stand"); TeamRequest(BondSupportKind.Armor, BondTuning.LastStandArmor, BondTuning.WaveBuffSeconds, "last-stand-armor"); return true; }
            if (friend == hero && Active("exhibit-hall") && !exhibitUsed) { exhibitUsed = true; friend.Health = 1 + (Active("exhibit-hall", 4) ? hero.Definition.Health * BondTuning.ExhibitHeal : 0); return true; }
            return false;
        }
        public void OnFriendFell(UnitState friend)
        {
            souls++;
            if (Active("spirit")) { spiritDeadlines.Add(time + BondTuning.SpiritSeconds); Spawn(new BondSpawnRequest(BondSpawnKind.Spirit, friend.Position, friend.Id, BondTuning.SpiritSeconds, Weapon(friend))); }
            if (Active("spirit", 6) && !revenantUsed) { revenantUsed = true; Request(BondSupportKind.Revive, friend, BondTuning.RevenantHealth, 0, "revenant"); }
        }
        public void OnFriendDamaged(UnitState friend, UnitState attacker, float amount)
        {
            if (attacker == null) return;
            float reflect = (Active("spiky-shell") ? (Active("spiky-shell", 4) ? BondTuning.SpikyHighReflect : BondTuning.SpikyReflect) : 0) + (friend == hero && Active("stone") ? BondTuning.StoneReflect : 0);
            if (reflect > 0) Area(attacker, friend, amount * reflect, "thorn-reflect");
        }
        public void OnCollision(UnitState knocked, UnitState other)
        { if (Active("stone", 4) && knocked != null && other != null && other.Faction == Faction.Enemy) { Weight++; Area(other, bestFriend, Weapon(bestFriend) * BondTuning.CollisionMultiplier, "stone-collision"); } }
        public void OnSkillDamage(UnitState friend, UnitState enemy, int indexInCast)
        { if (Active("demolitionist") && (!Active("demolitionist", 4) || indexInCast < BondTuning.DemolitionLimit) && State(enemy) != null) { float seconds = Active("demolitionist", 4) ? BondTuning.DemolitionHighStun : BondTuning.DemolitionStun; State(enemy).StunnedUntil = time + seconds; enemy.NextAttack = Mathf.Max(enemy.NextAttack, time + seconds); Emit("demolitionist-stun", enemy, seconds); } }
    }
}
