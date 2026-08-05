using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Wave-local gameplay state. Generated damage never feeds OnHit again.</summary>
    public sealed partial class BondRuntime
    {
        private readonly Dictionary<string, BondDefinition> definitions = new Dictionary<string, BondDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> members = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<UnitState, BondEnemyState> enemies = new Dictionary<UnitState, BondEnemyState>();
        private readonly List<UnitState> friends = new List<UnitState>(BondTuning.TeamCapacity);
        private readonly Dictionary<string, int> hits = new Dictionary<string, int>(BondTuning.RequestCapacity);
        private readonly Dictionary<int, float> casts = new Dictionary<int, float>(BondTuning.TeamCapacity);
        private readonly List<EmberPatch> patches = new List<EmberPatch>(BondTuning.RequestCapacity);
        private readonly List<BondSpawnRequest> spawnRequests = new List<BondSpawnRequest>(BondTuning.RequestCapacity);
        public IReadOnlyList<BondSpawnRequest> SpawnRequests => spawnRequests;
        public void ClearSpawnRequests() => spawnRequests.Clear();
        private void Spawn(BondSpawnRequest request) { spawnRequests.Add(request); SpawnRequested?.Invoke(request); }
        private struct EmberPatch { public Vector2 Position; public float Until, Weapon; public UnitState Source; }
        private UnitState hero, bestFriend, lastCombo;
        private float time, still, lastGrit, grit, stolenStrength, shantyUntil, infernoUntil, plagueAt, cloudAt, blizzardAt, eclipseAt;
        private int marked, soldiers, kills, crits, combo, stormHits, thunderHits;
        private bool infernoUsed;
        public int Heat { get; private set; }
        public int Chill { get; private set; }
        public int Soak { get; private set; }
        public int Charge { get; private set; }
        public int Volleys { get; private set; }
        public int Wishes { get; private set; }
        public int Records { get; private set; }
        public int Ticks { get; private set; }
        public float Warmth { get; private set; }
        public int Toxin { get; private set; }
        public int Rank { get; private set; }
        public int Applause { get; private set; }
        public int Light { get; private set; }
        public int Weight { get; private set; }
        public float Time => time;
        public float StillSeconds => still;
        public float GritBonus => grit;
        public float ComboBonus => Active("big-top") ? Mathf.Min(combo * BondTuning.ComboPerLink, Active("big-top", 4) ? BondTuning.ComboHighCap : BondTuning.ComboCap) : 0;
        public bool Moving { get; set; }
        public event Action<BondRuleEvent> RuleTriggered;
        public event Action<BondSpawnRequest> SpawnRequested;
        public event Action<BondDamageRequest> DamageRequested;
        public event Action<UnitState, UnitState, float> StrengthStolen;
        public event Action<UnitState, float> BonusShotRequested;
        public event Action<Vector2, float> EmbersCreated;

        public BondRuntime(IEnumerable<BondDefinition> bonds, IDictionary<string, int> counts, BondRunProgress progress = null)
        {
            RestoreProgress(progress);
            if (bonds != null) foreach (BondDefinition bond in bonds) if (bond != null && bond.Id != null) definitions[bond.Id] = bond;
            if (counts != null) foreach (var pair in counts) members[pair.Key] = pair.Value;
        }
        // Existing majors encode step 2 in Threshold; minors have no Steps yet. Explicit Steps override later gates.
        public bool Active(string id, int step = 2)
        {
            if (!definitions.TryGetValue(id, out BondDefinition bond) || !members.TryGetValue(id, out int count) || count < step) return false;
            if (step == 2) return count >= bond.Threshold;
            if (bond.Steps != null && bond.Steps.Length > 0)
            {
                foreach (BondStep entry in bond.Steps) if (entry.Count == step) return true;
                return false;
            }
            return step <= (id == "mask" || id == "horn" || id == "rune" || id == "gauntlet" || id == "sweet-tooth" || id == "paper-wings" || id == "shoe" || id == "hat" || id == "shield" || id == "echo" ? 6 : 4);
        }
        private bool Has(UnitState unit, string id)
        {
            if (unit == null || unit.Definition == null || unit.Definition.Tags == null) return false;
            foreach (string tag in unit.Definition.Tags) if (string.Equals(tag, id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        private bool Member(UnitState unit, string id) => Active(id) && Has(unit, id);
        private static float Weapon(UnitState unit) => unit?.Definition?.Weapon == null ? 0 : unit.Definition.Weapon.Damage;
        private void Emit(string rule, UnitState unit, float value = 0) => RuleTriggered?.Invoke(new BondRuleEvent(rule, unit, value));
        private void Damage(UnitState unit, UnitState source, float amount, string rule)
        { if (unit.Alive && amount > 0) DamageRequested?.Invoke(new BondDamageRequest(unit, source, amount, rule)); }
        public BondEnemyState State(UnitState enemy) => enemies.TryGetValue(enemy, out BondEnemyState value) ? value : null;

        public void ResetWave(UnitState champion, IEnumerable<UnitState> units)
        {
            traitApplause=0;traitApplauseUntil=0;DuskFallen=0;
            enemies.Clear(); friends.Clear(); hits.Clear(); casts.Clear(); patches.Clear(); spawnRequests.Clear();
            foreach (string key in BondTuning.HitCounterKeys) hits[key] = 0;
            hero = champion; bestFriend = lastCombo = null;
            time = still = lastGrit = grit = stolenStrength = shantyUntil = infernoUntil = 0;
            plagueAt = BondTuning.PlaguePeriod; cloudAt = BondTuning.CloudPeriod;
            blizzardAt = BondTuning.BlizzardPeriod; eclipseAt = BondTuning.EclipseStart;
            marked = soldiers = kills = crits = combo = stormHits = thunderHits = 0; infernoUsed = Moving = false;
            Heat = Chill = Soak = Charge = Volleys = Wishes = Records = Ticks = Rank = Applause = Light = Weight = 0; Warmth = 0;
            Toxin = 0;
            if (units != null) foreach (UnitState unit in units)
            {
                if (unit.Faction == Faction.Enemy) RegisterEnemy(unit);
                else if (unit.Faction == Faction.Companion)
                {
                    friends.Add(unit); Rank = Mathf.Max(Rank, unit.Definition.CombatTier);
                    if (bestFriend == null || Weapon(unit) > Weapon(bestFriend)) bestFriend = unit;
                }
            }
            ResetV3();
            if (hero != null && Active("ember", 8)) Spawn(new BondSpawnRequest(BondSpawnKind.Phoenix, hero.Position, lifetime: BondTuning.PhoenixSeconds, damage: Weapon(bestFriend)));
            if (hero != null && Active("lantern-guild", 6)) Spawn(new BondSpawnRequest(BondSpawnKind.LampWisp, hero.Position, damage: Weapon(bestFriend)));
        }
        public void RegisterEnemy(UnitState enemy)
        {
            if (enemy == null || enemy.Faction != Faction.Enemy || enemies.ContainsKey(enemy)) return;
            enemies.Add(enemy, new BondEnemyState { Chill = Active("frost", 8) ? BondTuning.WinterStacks : 0 });
            RefreshCounters();
        }
        // Scratch lists are reused; nested area work rotates to a different buffer.
        private readonly List<UnitState>[] nearbyBuffers = { new List<UnitState>(BondTuning.EnemyCapacity), new List<UnitState>(BondTuning.EnemyCapacity), new List<UnitState>(BondTuning.EnemyCapacity), new List<UnitState>(BondTuning.EnemyCapacity) };
        private sealed class NearbyOrder : IComparer<UnitState>
        {
            public Vector2 Center;
            public int Compare(UnitState a,UnitState b)
            {int order=(a.Position-Center).sqrMagnitude.CompareTo((b.Position-Center).sqrMagnitude);return order!=0?order:a.Id.CompareTo(b.Id);}
        }
        private readonly NearbyOrder[] nearbyOrders={new NearbyOrder(),new NearbyOrder(),new NearbyOrder(),new NearbyOrder()};
        private int nearbyCursor;
        private List<UnitState> Nearby(UnitState center, float radius, bool includeCenter = false)
        {
            int slot=nearbyCursor++ % nearbyBuffers.Length;
            List<UnitState> result = nearbyBuffers[slot]; result.Clear();
            float square = radius * radius;
            foreach (var pair in enemies) if (pair.Key.Alive && (includeCenter || pair.Key != center) &&
                (pair.Key.Position - center.Position).sqrMagnitude <= square) result.Add(pair.Key);
            // Cached comparers keep dense-area ordering deterministic without per-hit closures.
            nearbyOrders[slot].Center=center.Position;result.Sort(nearbyOrders[slot]);
            return result;
        }
        private void Area(UnitState center, UnitState source, float damage, string rule, bool ring = false)
        {
            foreach (UnitState enemy in Nearby(center, ring ? BondTuning.RuneRadius : BondTuning.BurstRadius, true))
                if (!ring || (enemy.Position - center.Position).sqrMagnitude >= BondTuning.RuneInnerRadius * BondTuning.RuneInnerRadius) Damage(enemy, source, damage, rule);
            Emit(rule, center, damage);
        }
        private bool Every(string id, int interval)
        { hits.TryGetValue(id, out int count); hits[id] = ++count; return count % interval == 0; }
        private int Interval(string id, int two, int four, int six = 0) => six > 0 && Active(id, 6) ? six : Active(id, 4) ? four : two;

        // Called before direct hit damage is applied; consumes frozen vulnerability and the Nth-crit counter once.
        public float ModifyHit(UnitState attacker, UnitState enemy, float damage, bool isCrit)
        {
            if (attacker == null || attacker.Faction == Faction.Enemy || !enemy.Alive) return damage;
            BondEnemyState state = State(enemy);
            if (state != null && state.FrozenHitReady && state.FrozenUntil > time)
            { damage *= 1 + BondTuning.FrozenBonus; state.FrozenHitReady = false; Emit("frozen-hit", enemy, BondTuning.FrozenBonus); }
            if (Active("big-top")) damage *= 1 + Mathf.Min(combo * BondTuning.ComboPerLink, Active("big-top", 4) ? BondTuning.ComboHighCap : BondTuning.ComboCap);
            if (Active("gauntlet") && attacker.Definition.Weapon != null && attacker.Definition.Weapon.Kind == WeaponKind.Melee) damage *= 1 + grit;
            if (Active("lantern-guild") && still >= BondTuning.StillSeconds) damage *= 1 + BondTuning.LanternBonus;
            damage *= 1 + LitBonus(enemy);
            if (attacker == bestFriend) damage += stolenStrength;
            if (isCrit && Active("mask") && ++crits % Interval("mask", BondTuning.MaskEvery, BondTuning.MaskFourEvery, BondTuning.MaskSixEvery) == 0)
            { damage *= BondTuning.MaskMultiplier; Emit("mask-double", enemy, BondTuning.MaskMultiplier); }
            return ModifyV3(attacker, enemy, damage);
        }
        public void OnHit(UnitState attacker, UnitState enemy, float damage, bool isCrit)
        {
            if (attacker == null || attacker.Faction == Faction.Enemy || enemy == null || enemy.Faction != Faction.Enemy || !enemy.Alive) return;
            RegisterEnemy(enemy); BondEnemyState state = State(enemy); float weapon = Weapon(attacker);
            HitV3(attacker, enemy, damage, isCrit);
            if (isCrit) Wishes++;
            if (Active("big-top")) { combo = lastCombo == enemy ? 0 : combo + 1; lastCombo = enemy; Emit("combo", enemy, Mathf.Min(combo * BondTuning.ComboPerLink, Active("big-top", 4) ? BondTuning.ComboHighCap : BondTuning.ComboCap)); }
            if (Active("gauntlet") && attacker.Definition.Weapon != null && attacker.Definition.Weapon.Kind == WeaponKind.Melee)
            { grit = Mathf.Min(grit + BondTuning.GritPerHit, Active("gauntlet", 4) ? BondTuning.GritHighCap : BondTuning.GritCap); lastGrit = time; Emit("grit", attacker, grit); }
            if (Member(attacker, "ember")) AddBurn(enemy, attacker, weapon);
            if (Member(attacker, "frost"))
            {
                state.Chill = Mathf.Min(BondTuning.ChillStacks, state.Chill + 1);
                Emit("chill-stack", enemy, state.Chill);
                if (Active("frost", 4) && state.Chill == BondTuning.ChillStacks && state.FrozenUntil <= time)
                { state.FrozenUntil = time + BondTuning.FreezeSeconds; state.FrozenHitReady = true; Emit("freeze", enemy, BondTuning.FreezeSeconds); }
            }
            if (Member(attacker, "venom")) AddPoison(enemy, attacker, weapon, 1, false);
            if (Member(attacker, "static-club"))
            {
                Charge++; stormHits++;
                Emit("charge", enemy, Charge);
                if (stormHits % BondTuning.StormEvery == 0)
                {
                    List<UnitState> chain = Nearby(enemy, float.PositiveInfinity);
                    for (int i = 0; i < Mathf.Min(BondTuning.ChainTargets, chain.Count); i++) Damage(chain[i], attacker, weapon * BondTuning.ChainDamage, "storm-chain");
                    Emit("storm-chain", enemy, weapon * BondTuning.ChainDamage);
                }
                if (Active("static-club", 4) && ++state.Charge >= BondTuning.DischargeStacks)
                { state.Charge = 0; Area(enemy, attacker, weapon * BondTuning.DischargeDamage, "discharge"); }
                if (Underdog)
                {
                    if (++thunderHits % BondTuning.ThunderHits == 0)
                    {
                        List<UnitState> nearest = hero == null ? null : Nearby(hero, float.PositiveInfinity);
                        StunArea(nearest != null && nearest.Count > 0 ? nearest[0] : enemy, BondTuning.ThunderStun, "thunderclap", false);
                    }
                }
                else thunderHits = 0;
            }
            if (Active("dusk") && !state.Marked && marked < BondTuning.DuskMarks)
            {
                state.Marked = true; marked++;
                stolenStrength += Mathf.Max(Weapon(enemy), enemy.Definition.ContactDamage) * BondTuning.StrengthSteal;
                StrengthStolen?.Invoke(enemy, bestFriend, BondTuning.StrengthSteal); Emit("dusk-mark", enemy, BondTuning.StrengthSteal);
            }
            CheckExecute(enemy, attacker);
            if (Active("horn") && Every("horn", Interval("horn", BondTuning.HornEvery, BondTuning.HornFourEvery, BondTuning.HornSixEvery))) StunArea(enemy, BondTuning.HornStun, "horn", true);
            if (Active("rune") && Every("rune", Interval("rune", BondTuning.RuneEvery, BondTuning.RuneFourEvery, BondTuning.RuneSixEvery))) Area(enemy, attacker, weapon * BondTuning.RuneDamage, "rune", true);
            if (Active("gear") && Every("gear", Interval("gear", BondTuning.GearEvery, BondTuning.GearFourEvery)))
            { BonusShotRequested?.Invoke(attacker, weapon); Emit("gear", attacker, weapon); }
            if (Active("sea-shanty") && Every("sea-shanty", Interval("sea-shanty", BondTuning.ShantyEvery, BondTuning.ShantyFourEvery)))
            { shantyUntil = time + BondTuning.ShantySeconds; Emit("sea-shanty", attacker, BondTuning.ShantySpeed); }
            RefreshCounters();
        }
        public void CheckExecute(UnitState enemy, UnitState attacker)
        {
            if (State(enemy)?.Marked == true && Active("dusk", 6) && enemy.Health > 0 && enemy.Health < enemy.Definition.Health * BondTuning.ExecuteHealth)
                Damage(enemy, attacker, enemy.Health, "execute");
        }
        private void StunArea(UnitState center, float seconds, string rule, bool push)
        {
            foreach (UnitState enemy in Nearby(center, BondTuning.BurstRadius, true))
            {
                enemy.SleepUntil = Mathf.Max(enemy.SleepUntil, time + seconds);
                State(enemy).StunnedUntil = Mathf.Max(State(enemy).StunnedUntil, time + seconds);
                enemy.NextAttack = Mathf.Max(enemy.NextAttack, time + seconds); enemy.NextContact = Mathf.Max(enemy.NextContact, time + seconds);
                if (push) { Vector2 direction = hero == null ? Vector2.up : (enemy.Position - hero.Position).normalized; enemy.Position += direction * BondTuning.HornPush; Weight++; }
            }
            Emit(rule, center, seconds);
        }
        private void AddBurn(UnitState enemy, UnitState source, float weapon)
        {
            BondEnemyState state = State(enemy); if (state == null) return;
            state.Burn = Mathf.Min(BondTuning.BurnStacks, state.Burn + 1); state.BurnUntil = time + BondTuning.BurnSeconds; state.BurnWeapon = weapon; state.BurnSource = source;
            Emit("burn-stack", enemy, state.Burn);
            if (state.Burn == BondTuning.BurnStacks && Active("ember", 4))
            {
                state.Burn = 0; Area(enemy, source, weapon * BondTuning.BurstDamage, "burn-burst");
                // Spread adds one stack without recursively bursting the neighbours in this call.
                foreach (UnitState next in Nearby(enemy, BondTuning.BurstRadius))
                { BondEnemyState other = State(next); other.Burn = Mathf.Min(BondTuning.BurnStacks, other.Burn + BondTuning.BurnSpreadStacks); other.BurnUntil = time + BondTuning.BurnSeconds; other.BurnWeapon = weapon; other.BurnSource = source; }
                state.Burn = BondTuning.BurnAfterBurst;
            }
        }
        private void AddPoison(UnitState enemy, UnitState source, float weapon, int stacks, bool spread)
        {
            BondEnemyState state = State(enemy); if (state == null) return;
            int cap = spread ? BondTuning.PoisonSpreadCap : BondTuning.PoisonHitCap;
            int previous = state.Poison;
            state.Poison = Mathf.Max(state.Poison, Mathf.Min(cap, state.Poison + stacks));
            state.PoisonUntil = time + BondTuning.PoisonSeconds; state.PoisonWeapon = weapon; state.PoisonSource = source;
            Emit(spread ? "poison-spread" : "poison-stack", enemy, state.Poison);
            if (Active("venom", 6) && previous < BondTuning.RotStacks && state.Poison >= BondTuning.RotStacks) Emit("rot", enemy, BondTuning.RotMaxHealthDps);
        }
        public void OnKill(UnitState enemy)
        {
            BondEnemyState state = State(enemy); if (state == null || state.DeathHandled) return;
            state.DeathHandled = true; if(state.Marked)DuskFallen++; kills++; KillV3(enemy);
            if (state.Burn > 0 && state.BurnUntil > time && Active("ember", 6))
            { patches.Add(new EmberPatch { Position = enemy.Position, Until = time + BondTuning.EmberSeconds, Source = state.BurnSource, Weapon = state.BurnWeapon }); EmbersCreated?.Invoke(enemy.Position, BondTuning.EmberSeconds); }
            if (state.FrozenUntil > time && Active("frost", 4)) Area(enemy, null, Weapon(bestFriend) * BondTuning.ShatterDamage, "shatter");
            if (Active("frost", 6) && soldiers < BondTuning.IceSoldierLimit)
            { soldiers++; Spawn(new BondSpawnRequest(BondSpawnKind.IceSoldier, enemy.Position, enemy.Id, BondTuning.IceSoldierSeconds, Weapon(bestFriend))); }
            if (state.Poison > 0 && state.PoisonUntil > time && Active("venom", 4))
                foreach (UnitState next in Nearby(enemy, BondTuning.BurstRadius)) AddPoison(next, state.PoisonSource, state.PoisonWeapon, state.Poison, true);
            if (state.Marked && Active("dusk", 4)) Spawn(new BondSpawnRequest(BondSpawnKind.Shade, enemy.Position, enemy.Id, BondTuning.ShadeSeconds, Weapon(bestFriend)));
            if (Active("clover") && kills % Interval("clover", BondTuning.CloverEvery, BondTuning.CloverFourEvery) == 0)
                Spawn(new BondSpawnRequest(BondSpawnKind.Clover, enemy.Position, freeRerolls: 1));
            if (Active("sweet-tooth") && kills % Interval("sweet-tooth", BondTuning.SnackEvery, BondTuning.SnackFourEvery) == 0)
                Spawn(new BondSpawnRequest(BondSpawnKind.Snack, enemy.Position, healMaxHealthFraction: BondTuning.SnackHeal, coins: Active("sweet-tooth", 6) ? 1 : 0));
            RefreshCounters();
        }
        public void OnCast(UnitState companion)
        { if (companion == null || companion.Faction != Faction.Companion) return; Ticks++; casts[companion.Id] = time; RefreshCounters(); CastV3(companion); Emit("cast", companion); }
        public void OnVolley() { Volleys++; if (Active("windborne", 4) && Volleys % BondTuning.PiercingVolley == 0 && hero != null) Request(BondSupportKind.PiercingVolley, hero, 1, 0, "piercing-volley"); }
        public void OnHeal(float amount) { Warmth += Mathf.Max(0, amount); }
        public void OnSoak() { Soak++; }
        public void OnRecord() { Records++; }
        public bool Underdog => Active("static-club", 6) && hero != null && hero.Health < hero.Definition.Health * BondTuning.UnderdogHealth;
        public float CriticalBonus => Underdog ? BondTuning.UnderdogBonus : 0;
        public float LuckBonus => Underdog ? BondTuning.UnderdogBonus : 0;
        public float RangeBonus => ConstellationRange + (Active("lantern-guild") && still >= BondTuning.StillSeconds ? BondTuning.LanternBonus : 0);
        public float RegenBonus => Active("root") && still >= BondTuning.StillSeconds ? (Active("root", 4) ? BondTuning.RootHighRegen : BondTuning.RootRegen) : 0;
        public float ArmorBonus => Active("root") && still >= BondTuning.StillSeconds ? (Active("root", 4) ? BondTuning.RootHighArmor : BondTuning.RootArmor) : 0;
        public float AttackSpeedBonus => (time < shantyUntil ? BondTuning.ShantySpeed : 0) + V3AttackSpeed;
        public float OutgoingScale(UnitState enemy) => (State(enemy)?.Marked == true ? 1 - BondTuning.StrengthSteal : 1) * (Active("archive", 8) && recordedSpecies.Contains(enemy.Definition.Id ?? "") ? 1 - BondTuning.CompleteSetWeakness : 1);
        private float LitBonus(UnitState enemy) => Active("lantern-guild", 8) ? BondTuning.LighthouseBonus :
            Active("lantern-guild", 4) && hero != null && (enemy.Position - hero.Position).sqrMagnitude <= BondTuning.LanternRadius * BondTuning.LanternRadius ? BondTuning.LitBonus : 0;
        public float SpeedScale(UnitState enemy)
        {
            BondEnemyState state = State(enemy); if (state == null) return 1;
            if (state.FrozenUntil > time || state.StunnedUntil > time) return 0;
            return GravitySpeed(enemy) * (1 - state.Chill * BondTuning.ChillSlow) * (Active("venom", 8) && state.Poison > 0 && state.PoisonUntil > time ? 1 - BondTuning.PlagueSlow : 1);
        }
        public bool Frozen(UnitState enemy) => State(enemy) != null && State(enemy).FrozenUntil > time;
        public bool BlocksAction(UnitState enemy) => Frozen(enemy) || State(enemy) != null && State(enemy).StunnedUntil > time;

        public void Tick(float dt)
        {
            if (!float.IsFinite(dt) || dt <= 0) return;
            float before = time; bool wasStill = still >= BondTuning.StillSeconds;
            time += dt; still = Moving ? 0 : still + dt;
            if (hero != null && wasStill != (still >= BondTuning.StillSeconds))
            {
                if (Active("root")) Emit("root-stillness", hero, RegenBonus);
                if (Active("lantern-guild")) Emit("lantern-stillness", hero, RangeBonus);
            }
            TickV3(dt);
            if (!Underdog) thunderHits = 0;
            if (!Active("gauntlet", 6) && time - lastGrit >= BondTuning.GritIdle) grit = 0;
            RefreshCounters();
            if (Active("ember", 8) && !infernoUsed && Heat >= BondTuning.InfernoHeat)
            { infernoUsed = true; infernoUntil = before + BondTuning.InfernoSeconds; if (hero != null) Emit("inferno", hero, BondTuning.InfernoMultiplier); }
            enemyTickBuffer.Clear(); foreach (var pair in enemies) enemyTickBuffer.Add(pair.Key);
            foreach (UnitState enemy in enemyTickBuffer)
            {
                if (!enemy.Alive) continue; BondEnemyState state = State(enemy);
                if (state.Burn > 0)
                {
                    float duration = Mathf.Max(0, Mathf.Min(time, state.BurnUntil) - before);
                    float boosted = Mathf.Max(0, Mathf.Min(Mathf.Min(time, state.BurnUntil), infernoUntil) - before);
                    Damage(enemy, state.BurnSource, state.BurnWeapon * state.Burn * BondTuning.BurnDps *
                        (duration + boosted * (BondTuning.InfernoMultiplier - 1)), "burn");
                }
                if (state.Poison > 0)
                {
                    float duration = Mathf.Max(0, Mathf.Min(time, state.PoisonUntil) - before);
                    Damage(enemy, state.PoisonSource, (state.PoisonWeapon * state.Poison * BondTuning.PoisonDps +
                        (Active("venom", 6) && state.Poison >= BondTuning.RotStacks ? enemy.Definition.Health * BondTuning.RotMaxHealthDps : 0)) * duration, "poison");
                }
                if (state.BurnUntil <= time) state.Burn = 0;
                if (state.PoisonUntil <= time) state.Poison = 0;
                foreach (EmberPatch patch in patches) if (patch.Until > time && (enemy.Position - patch.Position).sqrMagnitude <= BondTuning.BurstRadius * BondTuning.BurstRadius && state.Burn == 0) AddBurn(enemy, patch.Source, patch.Weapon);
            }
            patches.RemoveAll(patch => patch.Until <= time);
            if (Active("venom", 8)) while (time >= plagueAt)
            {
                plagueAt += BondTuning.PlaguePeriod;
                poisonBuffer.Clear(); foreach (var pair in enemies) if (pair.Key.Alive && pair.Value.Poison > 0) poisonBuffer.Add(pair.Key);
                foreach (UnitState source in poisonBuffer) foreach (UnitState next in Nearby(source, BondTuning.BurstRadius))
                    AddPoison(next, State(source).PoisonSource, State(source).PoisonWeapon, 1, true);
            }
            if (Active("frost", 8)) while (time >= blizzardAt)
            { blizzardAt += BondTuning.BlizzardPeriod; foreach (var pair in enemies) if (pair.Key.Alive) { pair.Value.Chill = BondTuning.ChillStacks; pair.Value.FrozenUntil = time + BondTuning.FreezeSeconds; pair.Value.FrozenHitReady = true; } if (hero != null) Emit("blizzard", hero); }
            if (Active("static-club", 8)) while (time >= cloudAt)
            {
                cloudAt += BondTuning.CloudPeriod; UnitState charged = null;
                int mostCharge = -1;
                foreach (var pair in enemies)
                {
                    if (!pair.Key.Alive) continue;
                    int clusterCharge = 0; foreach (UnitState nearby in Nearby(pair.Key, BondTuning.BurstRadius, true)) clusterCharge += State(nearby).Charge;
                    if (clusterCharge > mostCharge || clusterCharge == mostCharge && (charged == null || pair.Key.Id < charged.Id))
                    { charged = pair.Key; mostCharge = clusterCharge; }
                }
                if (charged != null) Area(charged, bestFriend, Weapon(bestFriend) * BondTuning.CloudDamage, "storm-cloud");
            }
            if (Active("dusk", 8)) while (time >= eclipseAt)
            {
                eclipseAt += BondTuning.EclipsePeriod; List<UnitState> live = eclipseBuffer; live.Clear(); foreach (var pair in enemies) if (pair.Key.Alive) live.Add(pair.Key);
                live.Sort(HealthOrder);
                int count = Mathf.CeilToInt(live.Count * BondTuning.EclipseFraction);
                for (int i = 0; i < count; i++) Damage(live[i], bestFriend, live[i].Health, "eclipse");
                if (hero != null) Emit("eclipse", hero, count);
            }
            RefreshCounters();
        }
        private readonly List<UnitState> enemyTickBuffer = new List<UnitState>(), poisonBuffer = new List<UnitState>(), eclipseBuffer = new List<UnitState>();
        private static int HealthOrder(UnitState a, UnitState b) { int order = a.Health.CompareTo(b.Health); return order != 0 ? order : a.Id.CompareTo(b.Id); }
        private int traitApplause;
        private float traitApplauseUntil;
        public int DuskFallen { get; private set; }
        public void OnTraitCounter(string rule)
        {
            if(rule=="centre-stage"){traitApplause++;traitApplauseUntil=time+BondTuning.ApplauseWindow;}
            else if(rule=="dawn-wishes")Wishes++;
            else if(rule=="dusk-form")DuskFallen++;
            else if(rule=="cherry-warmth")Warmth++;
        }
        private void RefreshCounters()
        {
            Heat = Chill = Toxin = Light = Applause = 0;
            foreach (var pair in enemies)
            {
                if (!pair.Key.Alive || pair.Value.DeathHandled) continue; BondEnemyState state = pair.Value;
                if (state.Burn > 0 && state.BurnUntil > time) Heat++;
                if (state.Chill > 0) Chill++;
                if (state.PoisonUntil > time) Toxin += state.Poison;
                if (LitBonus(pair.Key) > 0) Light++;
            }
            foreach (float at in casts.Values) if (time - at <= BondTuning.ApplauseWindow) Applause++;
            if(time<traitApplauseUntil)Applause+=traitApplause;else traitApplause=0;
        }
    }
}
