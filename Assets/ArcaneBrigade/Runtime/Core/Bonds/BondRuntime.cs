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
        private bool RuleApplies(UnitState unit, string id) => Active(id) && (Has(unit,id) || Active(id, BondTuning.SecondStep));
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
            ResetV5();
            ResetV6();
            if (hero != null && Active("lantern-guild",6))for(int i=0;i<BondV5Tuning.WispCount;i++)Spawn(new BondSpawnRequest(BondSpawnKind.LampWisp,hero.Position,sourceId:i,damage:Weapon(bestFriend)));
        }
        public void RegisterEnemy(UnitState enemy)
        {
            if (enemy == null || enemy.Faction != Faction.Enemy || enemies.ContainsKey(enemy)) return;
            enemies.Add(enemy, new BondEnemyState { Chill = Active("frost", 8) ? BondTuning.WinterStacks : 0 });
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
            if(isCrit&&time<celestialUntil)damage*=(BondV5Tuning.BaseCritMultiplier+BondV5Tuning.CelestialCritDamage)/BondV5Tuning.BaseCritMultiplier;
            if (state != null && state.FrozenHitReady && state.FrozenUntil > time)
            { damage *= 1 + BondTuning.FrozenBonus; state.FrozenHitReady = false; Shatter(enemy, attacker); Emit("frozen-hit", enemy, BondTuning.FrozenBonus); }
            if (Member(attacker,"big-top")) damage *= 1 + Mathf.Min(combo * BondTuning.ComboPerLink, Active("big-top", 4) ? BondTuning.ComboHighCap : BondTuning.ComboCap);
            if (Member(attacker,"gauntlet") && attacker.Definition.Weapon != null && attacker.Definition.Weapon.Kind == WeaponKind.Melee) damage *= 1 + grit;
            if (RuleApplies(attacker,"lantern-guild") && still >= BondTuning.StillSeconds) damage *= 1 + BondTuning.LanternBonus;
            damage *= 1 + LitBonus(enemy);
            if(State(enemy)?.Collisions>=BondV5Tuning.CollisionCrack)damage*=1+BondV5Tuning.CrackBonus;
            if(Member(attacker,"frost")&&Active("frost",6)&&Frozen(enemy))damage*=1+BondV5Tuning.FrostMemberDamage;
            if(Member(attacker,"horn")&&Active("horn",6)&&State(enemy)?.StunnedUntil>time)damage*=1+BondTuning.ChoiceBonus*BondV5Tuning.Double;
            if(InZone("glyph",enemy.Position))damage*=1+BondV5Tuning.GlyphBonus;
            damage += StolenDamageFor(attacker);
            if (isCrit && Member(attacker,"mask") && ++crits % Interval("mask", BondTuning.MaskEvery, BondTuning.MaskFourEvery, BondTuning.MaskSixEvery) == 0)
            { damage *= BondTuning.MaskMultiplier; MaskShards(enemy,attacker); Emit("mask-double", enemy, BondTuning.MaskMultiplier); }
            return ModifyV3(attacker, enemy, damage);
        }
        public void OnHit(UnitState attacker, UnitState enemy, float damage, bool isCrit)
        {
            if (attacker == null || attacker.Faction == Faction.Enemy || enemy == null || enemy.Faction != Faction.Enemy || !enemy.Alive) return;
            RegisterEnemy(enemy); BondEnemyState state = State(enemy); float weapon = Weapon(attacker);
            HitV3(attacker, enemy, damage, isCrit);
            if (isCrit) Wishes++;
            if (Member(attacker,"big-top")) { combo = lastCombo == enemy ? 0 : combo + 1; lastCombo = enemy; Emit("combo", enemy, Mathf.Min(combo * BondTuning.ComboPerLink, Active("big-top", 4) ? BondTuning.ComboHighCap : BondTuning.ComboCap)); }
            if (Member(attacker,"gauntlet") && attacker.Definition.Weapon != null && attacker.Definition.Weapon.Kind == WeaponKind.Melee)
            { grit = Mathf.Min(grit + BondTuning.GritPerHit, Active("gauntlet", 4) ? BondTuning.GritHighCap : BondTuning.GritCap); lastGrit = time; Emit("grit", attacker, grit); }
            if (RuleApplies(attacker, "ember")) AddBurn(enemy, attacker, weapon);
            if (RuleApplies(attacker, "frost"))
            {
                state.Chill = Mathf.Min(BondTuning.ChillStacks, state.Chill + 1);
                Emit("chill-stack", enemy, state.Chill);
                if (Active("frost", 4) && state.Chill == BondTuning.ChillStacks && state.FrozenUntil <= time)
                { state.FrozenUntil = time + BondTuning.FreezeSeconds; state.FrozenHitReady = true; Emit("freeze", enemy, BondTuning.FreezeSeconds); }
            }
            if (RuleApplies(attacker, "venom")) AddPoison(enemy, attacker, weapon, 1, false);
            if (RuleApplies(attacker, "static-club") && EveryUnit("storm",attacker,LegacyCapstones?BondTuning.StormEvery:BondStepTuning.StormEvery)) StormChain(enemy, attacker, weapon);
            if (RuleApplies(attacker,"dusk") && !state.Marked && marked < (Active("dusk",4)?BondTuning.DuskMarks:BondV5Tuning.EarlyMarks)) Mark(enemy,attacker);
            CheckExecute(enemy, attacker);
            if (Member(attacker,"horn") && Every("horn", Interval("horn", BondTuning.HornEvery, BondTuning.HornFourEvery, BondTuning.HornSixEvery))) StunArea(enemy, BondTuning.HornStun, "horn", true);
            if (Member(attacker,"rune") && Every("rune", Interval("rune", BondTuning.RuneEvery, BondTuning.RuneFourEvery, BondTuning.RuneSixEvery))) RuneStrike(enemy,attacker,weapon);
            if (Member(attacker,"gear") && Every("gear", Interval("gear", BondTuning.GearEvery, BondTuning.GearFourEvery)))
            { BonusShotRequested?.Invoke(attacker, weapon); Emit("gear", attacker, weapon); }
            if (Active("sea-shanty") && Every("sea-shanty", BondTuning.ShantyEvery))
            { verses=time>=shantyUntil?1:Mathf.Min(BondV5Tuning.VerseCap,verses+1);shantyUntil=time+BondTuning.ShantySeconds;if(Active("sea-shanty",4)&&verses>=BondV5Tuning.VerseCap){verses=0;chorusUntil=time+BondV5Tuning.ChorusSeconds;TeamRequest(BondSupportKind.DamageBuff,BondV5Tuning.ChorusDamage,BondV5Tuning.ChorusSeconds,"shanty-chorus");} Emit("sea-shanty",attacker,BondTuning.ShantySpeed); }
        }
        public void CheckExecute(UnitState enemy, UnitState attacker)
        {
            if ((State(enemy)?.Marked == true || Active("dusk",8)&&time>=BondTuning.EclipseStart) && Active("dusk", 6) && enemy.Health > 0 && enemy.Health < enemy.Definition.Health * BondTuning.ExecuteHealth)
                { shadeDamage+=BondTuning.ChoiceBonus; Damage(enemy, attacker, enemy.Health, "execute"); Emit("shade-execute-growth",enemy,shadeDamage); }
        }
        private void StunArea(UnitState center, float seconds, string rule, bool push)
        {
            foreach (UnitState enemy in Nearby(center, BondTuning.BurstRadius, true))
            {
                enemy.SleepUntil = Mathf.Max(enemy.SleepUntil, time + seconds);
                State(enemy).StunnedUntil = Mathf.Max(State(enemy).StunnedUntil, time + seconds);
                enemy.NextAttack = Mathf.Max(enemy.NextAttack, time + seconds); enemy.NextContact = Mathf.Max(enemy.NextContact, time + seconds);
                if (push)
                {
                    Vector2 direction=hero==null?Vector2.up:(enemy.Position-hero.Position).normalized;
                    enemy.Position+=direction*BondTuning.HornPush;Weight++;
                    if(rule=="horn"&&Active("horn",4))foreach(var pair in enemies)
                    {
                        UnitState other=pair.Key;if(other==enemy||!other.Alive)continue;
                        float contact=enemy.Definition.Radius+other.Definition.Radius;
                        if((other.Position-enemy.Position).sqrMagnitude>contact*contact)continue;
                        pair.Value.StunnedUntil=Mathf.Max(pair.Value.StunnedUntil,time+seconds);
                        Emit("horn-pile-up",other,seconds);
                    }
                }
            }
            Emit(rule, center, seconds);
        }
        private void AddBurn(UnitState enemy, UnitState source, float weapon)
        {
            BondEnemyState state=State(enemy); if(state==null)return;
            int cap=Active("ember",6)?BondV5Tuning.HighBurnStacks:BondTuning.BurnStacks;
            state.Burn=Mathf.Min(cap,state.Burn+1);state.BurnUntil=time+BondTuning.BurnSeconds;state.BurnWeapon=weapon;state.BurnSource=source;
            Emit("burn-stack",enemy,state.Burn);
            QueueBurst(enemy); DrainBursts();
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

            if (Active("frost", 6) && state.FrozenUntil > time && soldiers < BondTuning.IceSoldierLimit)
            { soldiers++; Spawn(new BondSpawnRequest(BondSpawnKind.IceSoldier, enemy.Position, enemy.Id, BondTuning.IceSoldierSeconds, Weapon(bestFriend))); }
            if (state.Poison > 0 && state.PoisonUntil > time && Active("venom", 4))
                foreach (UnitState next in Nearby(enemy, BondTuning.BurstRadius)) AddPoison(next, state.PoisonSource, state.PoisonWeapon, state.Poison + BondV5Tuning.PoisonDeathBonus, true);
            PassMark(enemy);
            if (Active("clover") && kills % Interval("clover", BondTuning.CloverEvery, BondTuning.CloverFourEvery) == 0)
                Spawn(new BondSpawnRequest(BondSpawnKind.Clover, enemy.Position, freeRerolls: 1));
            if (Active("sweet-tooth") && kills % Interval("sweet-tooth", BondTuning.SnackEvery, BondTuning.SnackFourEvery) == 0)
                Spawn(new BondSpawnRequest(BondSpawnKind.Snack, enemy.Position, healMaxHealthFraction: BondTuning.SnackHeal, coins: Active("sweet-tooth", 6) ? 1 : 0));
        }
        public void OnCast(UnitState companion)
        { if (companion == null || companion.Faction != Faction.Companion) return; Ticks++; casts[companion.Id] = time; RefreshApplause(); CastV3(companion); Emit("cast", companion); }
        public void OnVolley() { OnVolley(hero); }
        public void OnVolley(UnitState source) { Volleys++; if (RuleApplies(source,"windborne") && Active("windborne",4) && EveryUnit("wind",source,BondTuning.PiercingVolley)) { Request(BondSupportKind.PiercingVolley,source,1,0,"piercing-volley"); AddZone("wind-lane",source,BondV5Tuning.LaneSeconds); } }
        public void OnHeal(float amount) { if(amount>0 && Active("hearth") && hero!=null) Emit("warmth-heal",hero,amount); }
        public void OnSoak() { Soak++; }
        public void OnRecord() { Records++; }
        public bool Underdog => false;
        public float CriticalBonus => Underdog ? BondTuning.UnderdogBonus : 0;
        public float LuckBonus => Underdog ? BondTuning.UnderdogBonus : 0;
        public float RangeBonus => RangeBonusFor(hero);
        public float RegenBonus => Active("root") && still >= BondTuning.StillSeconds ? (Active("root", 4) ? BondTuning.RootHighRegen : BondTuning.RootRegen) : 0;
        public float ArmorBonus => Active("root") && still >= BondTuning.StillSeconds ? (Active("root", 4) ? BondTuning.RootHighArmor : BondTuning.RootArmor) : 0;
        public float AttackSpeedBonus => AttackSpeedBonusFor(hero);
        public float OutgoingScale(UnitState enemy) => (Active("venom",4) && State(enemy)?.Poison>0 ? 1-BondV5Tuning.PoisonWeakness : 1) * (IsEnemyMarked(enemy) ? 1 - BondTuning.StrengthSteal : 1) * (Active("archive", 8) && recordedSpecies.Contains(enemy.Definition.Id ?? "") ? 1 - BondTuning.CompleteSetWeakness : 1);
        private float LitBonus(UnitState enemy) => Active("lantern-guild", 8) ? BondTuning.LighthouseBonus :
            Active("lantern-guild", 4) && hero != null && ((enemy.Position - hero.Position).sqrMagnitude <= LightRadius * LightRadius || LampLights(enemy.Position)) ? BondTuning.LitBonus : 0;
        public float SpeedScale(UnitState enemy)
        {
            BondEnemyState state = State(enemy); if (state == null) return 1;
            if (state.FrozenUntil > time || state.StunnedUntil > time || state.RootedUntil>time) return 0;
            return StepSlow * ZoneSpeedScale(enemy) * GravitySpeed(enemy) * (1 - state.Chill * BondTuning.ChillSlow) * (Active("venom", 8) && state.Poison > 0 && state.PoisonUntil > time ? 1 - BondTuning.PlagueSlow : 1);
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
            TickV3(dt); TickV5(dt); TickV6(dt);
            if (!Underdog) thunderHits = 0;
            if (!Active("gauntlet", 6) && time - lastGrit >= BondTuning.GritIdle) grit = 0;
            RefreshCounters();
            if (LegacyCapstones && Active("ember", 8) && !infernoUsed && Heat >= BondTuning.InfernoHeat)
            { infernoUsed = true; infernoUntil = before + BondTuning.InfernoSeconds; if (hero != null) Emit("inferno", hero, BondTuning.InfernoMultiplier); }
            enemyTickBuffer.Clear(); foreach (var pair in enemies) enemyTickBuffer.Add(pair.Key);
            foreach (UnitState enemy in enemyTickBuffer)
            {
                if (!enemy.Alive) continue; BondEnemyState state = State(enemy);
                if (state.Burn > 0)
                {
                    float duration = Mathf.Max(0, Mathf.Min(time, state.BurnUntil) - before);

                    Damage(enemy, state.BurnSource, state.BurnWeapon * state.Burn * BondTuning.BurnDps *
                        duration, "burn");
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
            if (Active("venom", 6)) while (time >= plagueAt)
            {
                plagueAt += BondTuning.PlaguePeriod;
                poisonBuffer.Clear(); foreach (var pair in enemies) if (pair.Key.Alive && pair.Value.Poison > 0 && (Active("venom",8) || pair.Value.Poison>=BondTuning.RotStacks)) poisonBuffer.Add(pair.Key);
                foreach (UnitState source in poisonBuffer) foreach (UnitState next in Nearby(source, BondTuning.BurstRadius))
                    AddPoison(next, State(source).PoisonSource, State(source).PoisonWeapon, 1, true);
            }
            if (LegacyCapstones && Active("frost", 8)) while (time >= blizzardAt)
            { blizzardAt += BondTuning.BlizzardPeriod; foreach (var pair in enemies) if (pair.Key.Alive && pair.Value.Chill >= BondV5Tuning.BlizzardChill) { pair.Value.Chill = BondTuning.ChillStacks; pair.Value.FrozenUntil = time + BondTuning.FreezeSeconds; pair.Value.FrozenHitReady = true; } if (hero != null) Emit("blizzard", hero); }
            if (LegacyCapstones && Active("static-club", 8)) while (time >= cloudAt)
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
                if (charged != null) { Area(charged,bestFriend,Weapon(bestFriend)*BondTuning.CloudDamage*(mostCharge>0?BondV5Tuning.Double:1),"storm-cloud"); foreach(UnitState next in Nearby(charged,BondTuning.BurstRadius,true)) State(next).Charge=0; }
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
            else if(rule=="cherry-warmth")AddWarmth(1);
        }
        private void RefreshCounters()
        {
            Chill = Toxin = Light = 0;
            foreach (var pair in enemies)
            {
                if (!pair.Key.Alive || pair.Value.DeathHandled) continue; BondEnemyState state = pair.Value;

                if (state.Chill > 0) Chill++;
                if (state.PoisonUntil > time) Toxin += state.Poison;
                if (LitBonus(pair.Key) > 0) Light++;
            }
            RefreshApplause();
        }
        private void RefreshApplause()
        {
            Applause=0;
            foreach (float at in casts.Values) if (time - at <= BondTuning.ApplauseWindow) Applause++;
            if(time<traitApplauseUntil)Applause+=traitApplause;else traitApplause=0;
        }
    }
}
