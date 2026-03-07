using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    // Natural spending strategies and explicit laboratory samples are recorded separately.
    public static class BuildBalanceDiagnostics
    {
        static readonly string[] Representatives = { "LanternKnight", "Postmaster", "RootOracle", "LoneJelly", "GlassDice", "TideSinger" };
        static readonly string[] CoreBuilds = { "Buff", "Recruit", "Hybrid" };
        static readonly string[] Builds = { "Buff", "Recruit", "Hybrid", "Melee", "Ranged", "Critical", "Collision", "Sustain", "Economy" };
        static readonly MethodInfo Roll = typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly MethodInfo BuildEnemy = typeof(RunSession).GetMethod("BuildEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
        static string F(float x) => x.ToString("0.###", CultureInfo.InvariantCulture);
        static string Arg(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }

        public static void Run()
        {
            string root = "PreviewExports/BuildBalance/" + Arg("-balance-label", "Baseline");
            Directory.CreateDirectory(root);
            string mode = Arg("-balance-mode", "pilot");
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var hash = System.Security.Cryptography.SHA256.Create())
                File.AppendAllText(root + "/Assembly.txt", mode + " " + DateTime.UtcNow.ToString("O") + " " + BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(typeof(RunSession).Assembly.Location))).Replace("-", "") + "\n");
            File.WriteAllLines(root + "/Catalog_Heroes.csv", new[] { "hero,reference,mechanic,weapon,kind,damage,cooldown,hp,speed" }.Concat(catalog.Champions.Select(h => string.Join(",", h.Id, h.ReferenceHero, h.HeroMechanic, h.Weapon.Id, h.Weapon.Kind, F(h.Weapon.Damage), F(h.Weapon.Cooldown), F(h.Health), F(h.Speed)))));
            File.WriteAllLines(root + "/Catalog_Companions.csv", new[] { "companion,reference,rarity,price,weapon,damage,cooldown,passive" }.Concat(catalog.Companions.Select(h => string.Join(",", h.Id, h.ReferenceCompanion, h.Rarity, h.Price, h.Weapon.Id, F(h.Weapon.Damage), F(h.Weapon.Cooldown), h.Passive))));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (mode == "solo-gate" || mode == "foundation-gate")
            {
                if (mode == "solo-gate") Natural(catalog, root, "pilot", "LoneJelly", new[] { 77138, 161026 });
                else Natural(catalog, root, "pilot");
                if (mode == "solo-gate" && !File.ReadLines(root + "/Pilot.csv").Any(s => s.StartsWith("LoneJelly/0/Buff/0/") && s.Contains(",VICTORY,")))
                    throw new InvalidOperationException("Solo buff build still has no normal campaign clear in the gate seed");
                if (mode == "foundation-gate" && File.ReadLines(root + "/Pilot.csv").Count(s => s.Contains("/Buff/") && s.Contains(",VICTORY,")) < 3)
                    throw new InvalidOperationException("Foundation pilot needs review before the broad sweep");
                string gateRoot = root;
                root = "PreviewExports/BuildBalance/Final"; Directory.CreateDirectory(root);
                foreach (string file in new[] { "Assembly.txt", "Catalog_Heroes.csv", "Catalog_Companions.csv" }) File.Copy(gateRoot + "/" + file, root + "/" + file, true);
                Natural(catalog, root, "natural");
                Units(catalog, root); Weapons(catalog, root); Bosses(catalog, root);
                Natural(catalog, root, "archetypes");
                HeroMechanicBalance.Run();
                BuildBalanceChecks.Run();
            }
            else if (mode == "suite")
            {
                Natural(catalog, root, "natural");
                Units(catalog, root); Weapons(catalog, root); Bosses(catalog, root);
                Natural(catalog, root, "archetypes");
            }
            else if (mode == "host")
            {
                Natural(catalog, root, "natural", "PatchworkHost");
                Natural(catalog, root, "focused", "PatchworkHost");
                HeroMechanicBalance.Run();
                BuildBalanceChecks.Run();
            }
            else if (mode == "supplement")
            {
                Natural(catalog, root, "focused");
                Natural(catalog, root, "collision");
                Natural(catalog, root, "natural", "ThornTreasurer,IvorySovereign,PatchworkHost");
                HeroMechanicBalance.Run();
                BuildBalanceChecks.Run();
            }
            else if (mode == "laboratory") { Units(catalog, root); Weapons(catalog, root); Bosses(catalog, root); }
            else if (mode == "units") Units(catalog, root);
            else if (mode == "weapons") Weapons(catalog, root);
            else if (mode == "bosses") Bosses(catalog, root);
            else Natural(catalog, root, mode);
            File.AppendAllText(root + "/Timing.txt", mode + " " + watch.Elapsed.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " wall seconds\n");
            Debug.Log("BUILD_BALANCE_COMPLETE " + mode + " " + root + " " + watch.Elapsed.TotalSeconds);
        }

        static void Natural(RunCatalog catalog, string root, string mode, string heroFilter = null, int[] seeds = null)
        {
            string suffix = mode == "pilot" ? "Pilot" : mode == "archetypes" ? "Archetypes" : mode == "focused" ? "Focused" : mode == "focused6" ? "FocusedSix" : mode == "collision" ? "Collision" : "Natural";
            string path = root + "/" + suffix + ".csv";
            string waves = root + "/" + suffix + "_Waves.csv";
            if (!File.Exists(path)) File.WriteAllText(path, "id,hero,approach,build,difficulty,seed,outcome,last_wave,kills,seconds,allies,items,gold_spent,recruit_spent,item_spent,reroll_spent,hero_damage,ally_damage,boss_kills\n");
            if (!File.Exists(waves)) File.WriteAllText(waves, "id,wave,result,seconds,hp_share,kills,earned,allies,items,boss_hp,boss_remaining,hero_damage,ally_damage,hero_weapon_damage,hero_cooldown,max_hp,armor\n");
            var done = new HashSet<string>(File.ReadLines(path).Skip(1).Select(s => s.Split(',')[0]));
            int count = 0;
            foreach (var hero in catalog.Champions)
            {
                string only = heroFilter ?? Arg("-balance-heroes", "");
                if (only.Length > 0 && Array.IndexOf(only.Split(','), hero.Id) < 0) continue;
                bool representative = Array.IndexOf(Representatives, hero.Id) >= 0;
                if ((mode == "pilot" || mode == "archetypes" || mode == "collision") && !representative) continue;
                foreach (int approach in new[] { 0 })
                foreach (string build in mode == "focused" ? new[] { "FocusedBuff" } : mode == "focused6" ? new[] { "FocusedBuff6" } : mode == "collision" ? new[] { "Collision" } : mode == "archetypes" ? Builds.Skip(3) : CoreBuilds)
                foreach (int difficulty in mode == "pilot" ? new[] { 0, 5 } : mode == "archetypes" || mode == "collision" ? new[] { 0, 2, 5 } : representative ? Enumerable.Range(0, RunMeta.DifficultyNames.Length) : new[] { 0, 5 })
                foreach (int seed in seeds ?? (mode == "pilot" || mode == "archetypes" || mode == "collision" ? new[] { 77138 } : representative && (difficulty == 0 || difficulty == 5) ? new[] { 77138, 161026 } : new[] { 77138 }))
                {
                    string id = hero.Id + "/" + approach + "/" + build + "/" + difficulty + "/" + seed;
                    if (done.Contains(id)) continue;
                    using var run = new RunSession(catalog);
                    run.Start(hero.Id, seed, difficulty); run.State.HeroIdentity.Approach = approach;
                    string outcome = "TIMEOUT"; float seconds = 0, heroDamage = 0, allyDamage = 0;
                    int recruitSpent = 0, itemSpent = 0, rerollSpent = 0, goldSpent = 0;
                    for (int encounter = 0; encounter < catalog.NormalWaves; encounter++)
                    {
                        var arena = run.PrepareCombat();
                        var sim = Fight(arena, seed + run.State.Wave * 397, arena.Boss == null ? 65 : 180, build == "Collision");
                        seconds += sim.Elapsed;
                        var damage = Damage(sim); heroDamage += damage.hero; allyDamage += damage.ally;
                        File.AppendAllText(waves, string.Join(",", id, run.State.Wave, sim.Phase, F(sim.Elapsed), F(sim.Champion.Health / sim.Champion.Definition.Health), sim.Kills, sim.Gold, run.State.Roster.Count, run.State.Items.Sum(x => x.Count), F(arena.Boss?.Health ?? 0), F(sim.Boss?.Health ?? 0), F(damage.hero), F(damage.ally), F(arena.Champion.Weapon.Damage), F(arena.Champion.Weapon.Cooldown), F(arena.Champion.Health), F(arena.Armor)) + "\n");
                        if (sim.Phase == RunPhase.Combat) break;
                        run.Settle(sim);
                        if (run.State.Phase == SessionPhase.Defeat) { outcome = "DEFEAT"; break; }
                        if (run.State.Phase == SessionPhase.Victory) { outcome = "VICTORY"; break; }
                        int wallet = run.State.Gold;
                        Purchase(run, catalog, build, ref recruitSpent, ref itemSpent, ref rerollSpent);
                        goldSpent += wallet - run.State.Gold;
                        if (mode != "pilot" && representative && approach == 0 && seed == 77138 && (difficulty == 0 || difficulty == 2 || difficulty == 5))
                        {
                            if (run.State.Wave == 10 && run.SitCloserAvailable) OptionalBoss(catalog, run, root, id, false);
                            if (run.OutsideChallengeAvailable) OptionalBoss(catalog, run, root, id, true);
                        }
                        run.ContinueWave();
                        if (run.State.Phase != SessionPhase.Combat) { outcome = "BLOCKED_SHOP"; break; }
                    }
                    File.AppendAllText(path, string.Join(",", id, hero.Id, approach, build, difficulty, seed, outcome, run.State.Wave, run.State.TotalKills, F(seconds), run.State.Roster.Count, run.State.Items.Sum(x => x.Count), goldSpent, recruitSpent, itemSpent, rerollSpent, F(heroDamage), F(allyDamage), run.State.BossKills) + "\n");
                    File.AppendAllText(root + "/" + suffix + "_Loadouts.tsv", id + "\t" + string.Join(";", run.State.Items.Select(s => s.Id + ":" + s.Count)) + "\t" + string.Join(";", run.State.Roster.Select(s => s.UnitId + ":" + s.WeaponId + ":" + s.Tier)) + "\n");
                    count++;
                    if (count % 6 == 0) Debug.Log("BUILD_BALANCE_PROGRESS " + suffix + " " + count + " " + id + " " + outcome + " wave " + run.State.Wave);
                }
            }
            File.WriteAllText(root + "/" + suffix + "_Method.txt", "Production campaign, prices, quotas, starter/story companions, growth and loot. No injected income, HP, wins or items. Same seeds and movement for strategies; Buff never buys companions, Recruit prioritises companions and spends leftovers on useful items, Hybrid buys both. SoloMaw recruitment follows production absorption rules and is not a swarm build. Card Queen does not buy HP/armor for ignored stats; Periwinky retains a growing gold reserve for its gold-based HP. Additional archetypes prioritise melee/ranged/critical/collision/sustain/economy items without removing hero restrictions. Coverage: all 31 heroes x Buff/Recruit/Hybrid at difficulties 0/5, approach 0; six representatives additionally cover all six difficulties and a second seed at 0/5. Natural total 294 campaigns. All 93 approaches are separately measured by HeroMechanicBalance at waves 5/10/15 with two seeds, on/off counterfactuals and explicit standard loadouts. Additional archetypes: six representatives x six strategies x difficulties 0/2/5 x one seed = 108 campaigns. Unfiltered pilot is 36 runs; explicit hero/seed overrides alter its size. Normal cutoff 65s, boss cutoff 180s; Combat means unresolved, never victory. Bot uses weapon/body reach and avoids visible projectiles/telegraphs; not a human win-rate estimate. No claim to cover every item permutation.\n");
        }

        public static Vector2 Steer(CombatSimulation sim, bool preferCollision = false)
        {
            var hero = sim.Champion; Vector2 p = hero.Position;
            UnitState nearest = null; float nearestD = float.PositiveInfinity;
            foreach (var enemy in sim.Units)
                if (enemy.Alive && enemy.Faction == Faction.Enemy)
                { float d = Vector2.Distance(enemy.Position, p) - enemy.Definition.Radius; if (d < nearestD) { nearestD = d; nearest = enemy; } }
            Vector2 move = new Vector2(Mathf.Cos(sim.Elapsed * .27f) * 7, Mathf.Sin(sim.Elapsed * .27f) * 4.2f) - p;
            if (nearest != null)
            {
                Vector2 offset = p - nearest.Position; float distance = offset.magnitude;
                if (distance < .001f) offset = Vector2.right;
                Vector2 away = offset.normalized;
                float reach = hero.Definition.Weapon.Kind == WeaponKind.Melee ? MeleeTactics.Reach(hero, nearest) : hero.Definition.Weapon.Range;
                float safe = hero.Definition.Radius + nearest.Definition.Radius + .24f;
                float desired = Mathf.Min(reach * .9f, Mathf.Max(safe, reach * .78f));
                move = away * Mathf.Clamp((desired - distance) * 2, -1, 1);
                if (Mathf.Abs(distance - desired) < .35f) move += new Vector2(-away.y, away.x) * .35f;
                if ((preferCollision && hero.Definition.CollisionDamage > 0 || hero.Definition.CollisionDamage > hero.Definition.Weapon.Damage * .8f) && hero.Health > hero.Definition.Health * .6f)
                    move = -away;
                if (hero.Definition.HeroMechanic == HeroMechanic.RootedOracle && distance < reach && distance > safe) move = Vector2.zero;
            }
            foreach (var coin in sim.Pickups)
                if ((coin.Position - p).sqrMagnitude < (hero.Definition.ReferenceHero == "Periwinky" ? 36 : 9) && (nearest == null || nearestD > 2.2f)) { move = Vector2.ClampMagnitude(coin.Position - p, 1); break; }
            foreach (var enemy in sim.Units)
            {
                if (!enemy.Alive || enemy.Faction != Faction.Enemy) continue;
                Vector2 offset = p - enemy.Position; float d = offset.magnitude;
                float safe = hero.Definition.Radius + enemy.Definition.Radius + (preferCollision && enemy == nearest && hero.Health > hero.Definition.Health * .6f ? -.03f : .18f);
                if (d < safe && d > .001f) move += offset / d * (safe - d) * 5;
            }
            foreach (var shot in sim.Projectiles)
            {
                if (shot.Faction != Faction.Enemy) continue;
                Vector2 a = shot.Position - p, step = shot.Velocity * .22f;
                float t = step.sqrMagnitude < .0001f ? 0 : Mathf.Clamp01(-Vector2.Dot(a, step) / step.sqrMagnitude);
                Vector2 offset = -(a + step * t); float d = offset.magnitude;
                float safe = hero.Definition.Radius + shot.Radius + .22f;
                if (d < safe && d > .001f) move += offset / d * (safe - d) * 7;
            }
            foreach (var field in sim.ExpandedFields)
            {
                if (field.Source?.Faction != Faction.Enemy || sim.Elapsed + .6f < field.At || sim.Elapsed > field.Until) continue;
                Vector2 offset = p - field.Position; float d = offset.magnitude;
                if (d < field.Radius + hero.Definition.Radius + .25f && d > .001f) move += offset / d * 2;
            }
            return Vector2.ClampMagnitude(move, 1);
        }

        static CombatSimulation Fight(CheckpointCatalog arena, int seed, float seconds, bool preferCollision = false)
        {
            var sim = new CombatSimulation(arena); sim.Start(seed);
            for (int tick = 0; tick < Mathf.CeilToInt(seconds / .05f) && sim.Phase == RunPhase.Combat; tick++) sim.Step(.05f, Steer(sim, preferCollision));
            if (!float.IsFinite(sim.Champion.Health)) throw new InvalidOperationException("Nonfinite champion HP");
            return sim;
        }

        static (float hero, float ally) Damage(CombatSimulation sim)
        {
            float hero = 0, ally = 0;
            foreach (var entry in sim.Contributions)
                if (entry.UnitId == sim.Champion.Definition.Id) hero += entry.Damage; else ally += entry.Damage;
            return (hero, ally);
        }

        static void OptionalBoss(RunCatalog catalog, RunSession original, string root, string id, bool keeper)
        {
            using var run = new RunSession(catalog);
            run.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(original.State)));
            if (!(keeper ? run.AcceptOutsideStage() : run.AcceptSitCloser())) throw new InvalidOperationException("Natural optional boss unavailable");
            var arena = run.PrepareCombat(); var sim = Fight(arena, 77138, 180);
            string path = root + "/Optional_Bosses.csv";
            if (!File.Exists(path)) File.WriteAllText(path, "id,boss,result,boss_defeated,withdrawn,seconds,boss_hp,boss_left,hp_share,allies,items\n");
            File.AppendAllText(path, string.Join(",", id, keeper ? "Keeper" : "Bedside", sim.Phase, sim.BossDefeated, sim.KeeperWithdrew, F(sim.Elapsed), F(arena.Boss.Health), F(sim.Boss.Health), F(sim.Champion.Health / sim.Champion.Definition.Health), run.State.Roster.Count, run.State.Items.Sum(x => x.Count)) + "\n");
        }

        static float Score(RunSession run, RunCatalog catalog, ShopOffer offer, string build)
        {
            if (offer.Sold) return -1;
            bool focused = build == "FocusedBuff" || build == "FocusedBuff6";
            if (offer.Kind == OfferKind.Companion)
            {
                bool merge = ShopMergePreview.Ready(run, offer);
                bool owned = run.State.Roster.Exists(r => r.UnitId == offer.Id && r.Tier == offer.Tier);
                if (build == "Buff" || focused && (run.State.Roster.Count >= (build == "FocusedBuff6" ? 6 : 4) && !merge || run.State.Roster.Count >= 2 && offer.Tier == 1 && !owned && !merge)) return -1;
                float priority = build == "Recruit" ? 4 : build == "Hybrid" ? 1 : focused ? .15f * (offer.Tier >= 3 ? 2.8f : offer.Tier == 2 ? 1.8f : owned ? 1.5f : 1) : .45f;
                return (merge ? 120 : 35) * priority / Mathf.Pow(Mathf.Max(1, offer.Price), .5f);
            }
            var item = catalog.Item(offer.Id); if (item == null) return -1;
            if (catalog.Unit(run.State.ChampionId).ReferenceHero == "Card Queen" && (item.Mechanic == RelicMechanic.GambleRecovery || item.Mechanic == RelicMechanic.BleedingAlarm)) return -1;
            if (offer.Currency == ShopCurrency.Health && offer.Price > run.State.Health * .2f) return -1;
            bool ranged = catalog.Unit(run.State.ChampionId).Weapon.Kind == WeaponKind.Projectile;
            float result = item.Mechanic != RelicMechanic.None || item.SignatureRole != SignatureRelicKind.None ? 2 : 0;
            foreach (var m in item.EffectiveModifiers)
            {
                float weight = m.Effect switch
                {
                    ItemEffect.Damage or ItemEffect.ChampionDamage or ItemEffect.CompanionDamage or ItemEffect.DamageCorrection => 40,
                    ItemEffect.AttackSpeed or ItemEffect.CompanionAttackSpeed or ItemEffect.Crit => 35,
                    ItemEffect.MeleeDamage => ranged ? 1 : 4,
                    ItemEffect.RangedDamage => ranged ? 4 : 1,
                    ItemEffect.Projectiles or ItemEffect.BulletPierce or ItemEffect.BulletSplit => ranged ? 5 : 2,
                    ItemEffect.Armor or ItemEffect.Dodge => 20,
                    ItemEffect.Heal => 5,
                    ItemEffect.Health => .35f,
                    ItemEffect.MoveSpeed or ItemEffect.AttackRange => 10,
                    ItemEffect.Income => 1.2f,
                    ItemEffect.Luck or ItemEffect.TreasureDetection or ItemEffect.Discount or ItemEffect.RelicDiscount => 12,
                    ItemEffect.CollisionDamage => 1.2f,
                    ItemEffect.EnemyCount or ItemEffect.EnemyHealth or ItemEffect.EnemySpeed => -40,
                    _ => 1
                };
                string reference = catalog.Unit(run.State.ChampionId).ReferenceHero;
                if ((reference == "Card Queen" || reference == "Periwinky") && (m.Effect == ItemEffect.Health || m.Effect == ItemEffect.HealthCorrection)) weight = 0;
                if (reference == "Card Queen" && (m.Effect == ItemEffect.Armor || m.Effect == ItemEffect.ArmorCorrection)) weight = 0;
                bool defense = m.Effect == ItemEffect.Health || m.Effect == ItemEffect.Heal || m.Effect == ItemEffect.Armor || m.Effect == ItemEffect.Dodge;
                bool economy = RunModifierRules.DefaultScope(m.Effect) == ModifierScope.Run;
                if (build == "Sustain" && defense || build == "Economy" && economy || build == "Melee" && m.Effect == ItemEffect.MeleeDamage || build == "Ranged" && (m.Effect == ItemEffect.RangedDamage || m.Effect == ItemEffect.Projectiles) || build == "Critical" && m.Effect == ItemEffect.Crit || build == "Collision" && (m.Effect == ItemEffect.CollisionDamage || m.Effect == ItemEffect.CollisionFrequency || m.Effect == ItemEffect.Armor)) weight *= 3;
                if (m.Scope == ModifierScope.Companions && (build == "Buff" || focused)) weight *= .3f;
                if (m.Scope == ModifierScope.Champion && (build == "Buff" || focused)) weight *= 1.4f;
                result += m.Value * weight;
            }
            return result / Mathf.Pow(Mathf.Max(1, offer.Price), .5f);
        }

        static void Purchase(RunSession run, RunCatalog catalog, string build, ref int recruits, ref int items, ref int rerolls)
        {
            int reserve = catalog.Unit(run.State.ChampionId).ReferenceHero == "Periwinky" ? Mathf.Min(400, 50 + run.State.Wave * 25) : 0;
            if (run.State.OutsideStage == OutsideStageStatus.Reward)
            {
                int best = run.State.OutsideRewards.FindIndex(id => build == "Recruit" ? catalog.Unit(id) != null : catalog.Item(id) != null);
                run.ClaimOutsideStageReward(Mathf.Max(0, best));
            }
            int buyRecruits = 0, buyItems = 0;
            void BuyOffers(IEnumerable<ShopOffer> offers)
            {
                foreach (var offer in offers.OrderByDescending(o => Score(run, catalog, o, build)).ToArray())
                {
                    if (Score(run, catalog, offer, build) <= 0 || !run.CanBuy(offer)) continue;
                    if (offer.Currency == ShopCurrency.Gold && run.State.Gold - offer.Price < reserve) continue;
                    int before = run.State.Gold;
                    if (run.Buy(offer.Token) && offer.Currency == ShopCurrency.Gold)
                    { if (offer.Kind == OfferKind.Companion) buyRecruits += before - run.State.Gold; else buyItems += before - run.State.Gold; }
                }
            }
            BuyOffers(run.State.SpecialOffers); run.State.SpecialShopVisited = true;
            for (int pass = 0; pass < 10; pass++)
            {
                BuyOffers(run.State.Offers);
                int cheapest = run.State.Offers.Where(o => !o.Sold && Score(run, catalog, o, build) > 0).Select(o => o.Price).DefaultIfEmpty(25).Min();
                if (run.State.Gold <= reserve + run.RerollCost + cheapest) break;
                int before = run.State.Gold; if (!run.Reroll()) break; rerolls += before - run.State.Gold;
            }
            recruits += buyRecruits; items += buyItems;
        }

        static void Units(RunCatalog catalog, string root)
        {
            string path = root + "/Companions.csv";
            File.WriteAllText(path, "companion,tier,wave,damage,healing,coins,kills,seconds,hero_hp,hero_max_hp,armor,heal,champion_damage_bonus,team_attack_speed,ally_damage_bonus,ally_attack_speed\n");
            foreach (var ally in catalog.Companions)
            for (int tier = 1; tier <= 3; tier++)
            foreach (int wave in new[] { 5, 15 })
            {
                using var run = new RunSession(catalog); run.Start("LanternKnight", 77138);
                run.State.Wave = wave; run.State.Roster.Clear();
                run.State.Roster.Add(new RosterEntry { UnitId = ally.Id, WeaponId = ally.Weapon.Id, Tier = tier });
                var arena = run.PrepareCombat(); arena.Boss = null; arena.StoryCompanions = Array.Empty<UnitDefinition>();
                arena.Champion.Weapon.Damage = 0; arena.Champion.CollisionDamage = 0; arena.EnableHeroMechanics = arena.EnableHeroIdentities = false;
                var sim = Fight(arena, 77138, 25);
                var contributions = sim.Contributions.Where(c => c.UnitId == ally.Id).ToArray();
                File.AppendAllText(path, string.Join(",", ally.Id, tier, wave, F(contributions.Sum(c => c.Damage)), F(contributions.Sum(c => c.Healing)), contributions.Sum(c => c.Coins), sim.Kills, F(sim.Elapsed), F(sim.Champion.Health), F(arena.Champion.Health), F(arena.Armor), F(run.Bonus(ItemEffect.Heal)), F(run.Bonus(ItemEffect.Damage, ModifierScope.Champion)), F(run.Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion)), F(run.Bonus(ItemEffect.CompanionDamage)), F(run.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions))) + "\n");
            }
        }

        static void Weapons(RunCatalog catalog, string root)
        {
            string path = root + "/Weapons.csv";
            File.WriteAllText(path, "weapon,kind,tier,wave,damage,kills,seconds,hp\n");
            foreach (var weapon in catalog.Weapons)
            for (int tier = 1; tier <= 3; tier++)
            foreach (int wave in new[] { 5, 15 })
            {
                using var run = new RunSession(catalog); run.Start("LanternKnight", 77138);
                run.State.Wave = wave; run.State.Roster.Clear();
                run.State.Roster.Add(new RosterEntry { UnitId = "JellySwordsman", WeaponId = weapon.Id, Tier = tier });
                var arena = run.PrepareCombat(); arena.Boss = null; arena.StoryCompanions = Array.Empty<UnitDefinition>();
                arena.Champion.Weapon.Damage = 0; arena.Champion.CollisionDamage = 0; arena.EnableHeroMechanics = arena.EnableHeroIdentities = false;
                var sim = Fight(arena, 77138, 25);
                File.AppendAllText(path, string.Join(",", weapon.Id, weapon.Kind, tier, wave, F(sim.Contributions.Where(c => c.UnitId == "JellySwordsman").Sum(c => c.Damage)), sim.Kills, F(sim.Elapsed), F(sim.Champion.Health)) + "\n");
            }
        }

        static void Bosses(RunCatalog catalog, string root)
        {
            // Controlled, equal purchase-wallet progression: no combat income or artificial victories.
            string path = root + "/Bosses.csv";
            File.WriteAllText(path, "hero,build,difficulty,boss,result,seconds,boss_hp,boss_left,hp_share,allies,items,spent\n");
            foreach (string hero in Representatives)
            foreach (string build in CoreBuilds)
            foreach (int difficulty in new[] { 0, 2, 5 })
            foreach (string encounter in new[] { "5", "10", "15", "20", "Keeper", "Bedside" }.Concat(
                (hero == "LanternKnight" || hero == "Postmaster" || hero == "LoneJelly") && difficulty != 2 ? catalog.Bosses.Select(b => "Body:" + b.Id) : Array.Empty<string>()))
            {
                int wave = encounter == "Keeper" ? 17 : encounter == "Bedside" || encounter.StartsWith("Body:") ? 10 : int.Parse(encounter);
                using var run = new RunSession(catalog); run.Start(hero, 77138, difficulty);
                int r = 0, i = 0, rolls = 0, spent = 0;
                for (int shop = 1; shop < wave; shop++)
                {
                    run.State.Wave = shop; run.State.Phase = SessionPhase.Shop;
                    run.State.Health = K10StatRules.MaximumHealth(run, catalog);
                    run.State.Gold += 65 + shop * 5;
                    run.State.NewRecruitsThisWave = run.State.NewSpeciesRecruitsThisWave = run.State.SpecialNewSpeciesRecruitsThisWave = 0;
                    Roll.Invoke(run, new object[] { true }); int before = run.State.Gold;
                    Purchase(run, catalog, build, ref r, ref i, ref rolls); spent += before - run.State.Gold;
                }
                run.State.Wave = wave; run.State.Phase = SessionPhase.Combat;
                if (encounter == "Keeper" || encounter == "Bedside")
                {
                    run.State.Phase = SessionPhase.Shop;
                    if (!(encounter == "Keeper" ? run.AcceptOutsideStage() : run.AcceptSitCloser())) throw new InvalidOperationException("Unavailable boss fixture " + encounter);
                }
                var arena = run.PrepareCombat();
                if (encounter.StartsWith("Body:"))
                {
                    var body = Array.Find(catalog.Bosses, b => b.Id == encounter.Substring(5));
                    float scaling = (1 + (wave - 1) * .1f) * JourneyBalance.EnemyHealth(difficulty, wave);
                    arena.Boss = (UnitDefinition)BuildEnemy.Invoke(run, new object[] { body, scaling });
                    arena.Boss.Health *= WaveCadence.StoryBossBulk(wave) * OctoberBalance.EnemyHealthRelief * OctoberBalance.BossHealthRelief;
                }
                var sim = Fight(arena, 77138, 180);
                File.AppendAllText(path, string.Join(",", hero, build, difficulty, arena.Boss.Id, sim.Phase, F(sim.Elapsed), F(arena.Boss.Health), F(sim.Boss.Health), F(sim.Champion.Health / sim.Champion.Definition.Health), run.State.Roster.Count, run.State.Items.Sum(x => x.Count), spent) + "\n");
            }
            File.WriteAllText(root + "/Bosses_Method.txt", "Explicit laboratory loadouts: identical per-shop gold allowance 65 + shop*5, real shop RNG/prices/quotas/merges, no combat earnings, no injected HP during fights. Production rest healing between fixture shops. Six heroes x three purchase strategies x difficulties 0/2/5 x story bosses at 5/10/15/20 plus Keeper17 and Bedside10; additionally all ten regular boss bodies at wave10 with melee/ranged/solo representatives at difficulties 0/5. Boss body substitutions apply the same production scaling and relief. 180-second cutoff; Combat is unresolved. Keeper withdrawal is not a boss kill. These are purchase-efficiency encounters, not naturally earned campaign win rates.\n");
        }
    }
}
