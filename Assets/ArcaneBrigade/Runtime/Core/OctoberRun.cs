using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        public int LeafPurchases, OctoberRefreshes, OctoberDrinks, OctoberFood, OctoberHearts, OctoberChests;
        public List<StatModifier> OctoberGains = new List<StatModifier>();
        public List<string> AbsorbedSkills = new List<string>();
    }
    public sealed partial class RunSession
    {
        private string OctoberHero => catalog.Unit(State.ChampionId).ReferenceHero ?? "";
        private bool OctoberAbsorbs => OctoberHero == "Capped Gallant"; // Wise now buys heroes, not friends' memories (WiseHeroItems)
        /// <summary>The Dream Seekers Wise has bought (their reference names).</summary>
        private List<string> WiseHeroRefs()
        {
            var refs = new List<string>();
            foreach (ItemStack stack in State.Items) if (WiseHeroItems.Is(stack.Id)) { string r = catalog.Unit(stack.Id.Substring(WiseHeroItems.Prefix.Length))?.ReferenceHero; if (!string.IsNullOrEmpty(r)) refs.Add(r); }
            return refs;
        }
        private void ValidateOctoberState(RunSnapshot r)
        {
            if (r == null) return;
            if (r.LeafPurchases < 0 || r.OctoberRefreshes < 0 || r.OctoberDrinks < 0 || r.OctoberFood < 0 || r.OctoberHearts < 0 || r.OctoberChests < 0) throw new InvalidOperationException("Invalid character progress");
            if ((r.OctoberGains?.Count ?? 0) > 128 || (r.AbsorbedSkills?.Count ?? 0) > 2048 || (r.AchievementEvidence?.Count ?? 0) > 512) throw new InvalidOperationException("Invalid character journal size");
            foreach (StatModifier m in r.OctoberGains ?? new List<StatModifier>()) if (!Enum.IsDefined(typeof(ItemEffect), m.Effect) || !Enum.IsDefined(typeof(ModifierScope), m.Scope) || !float.IsFinite(m.Value)) throw new InvalidOperationException("Invalid character modifier");
            foreach (string id in r.AbsorbedSkills ?? new List<string>()) if (!Array.Exists(catalog.Companions, u => u.Id == id)) throw new InvalidOperationException("Invalid companion memory");
            var keys = new HashSet<string>();
            foreach (AchievementMetric m in r.AchievementEvidence ?? new List<AchievementMetric>()) if (m == null || string.IsNullOrEmpty(m.Id) || !float.IsFinite(m.Value) || m.Value < 0 || !keys.Add(m.Id)) throw new InvalidOperationException("Invalid achievement evidence");
        }
        private int Talent(string name)
        {
            int count = 0; foreach (RosterEntry r in ActiveRoster) if (catalog.Unit(r.UnitId).ReferenceCompanion == name) count++;
            if (State.AbsorbedSkills != null) foreach (string id in State.AbsorbedSkills) if (catalog.Unit(id)?.ReferenceCompanion == name) count++;
            return count;
        }
        private int BondLevel(string id) => BondCounts.TryGetValue(id, out int count) ? catalog.Bonds[Array.FindIndex(catalog.Bonds, b => b.Id == id)].ActiveLevel(count) : 0;
        private float OctoberBonus(ItemEffect effect, ModifierScope scope)
        {
            float value = 0; bool hero = scope == ModifierScope.Champion, ally = scope == ModifierScope.Companions;
            if (State.OctoberGains != null) foreach (StatModifier m in State.OctoberGains) if (m.Effect == effect && RunModifierRules.Applies(m.Scope, scope)) value += m.Value;
            if (effect == ItemEffect.Damage && (hero || ally)) value += .05f * Talent("Chieftain") + .2f * Talent("\"Quick Production\" Tao");
            if (effect == ItemEffect.MoveSpeed && (hero || ally)) value += .05f * Talent("Cheer Bunny") + .1f * Talent("\"Quick Production\" Tao");
            if (hero && effect == ItemEffect.Health) value += 5 * Talent("Floofy") + 4 * Talent("Hopping Zombie");
            if (hero && effect == ItemEffect.Armor) value += .08f * (Talent("Floofy") - Talent("Phantom"));
            if (effect == ItemEffect.Income) value += 4 * Talent("Judge") * BondLevel("clover") + 15 * Talent("\"Overtime King\" Philip");
            if ((effect == ItemEffect.MeleeDamage || effect == ItemEffect.RangedDamage) && (hero || ally)) value += State.OctoberFood * Talent("\"Art of Food\" Roshi");
            if (effect == ItemEffect.Health && hero) value -= Mathf.Min(20, State.OctoberFood * .5f * Talent("\"Art of Food\" Roshi"));
            if (effect == ItemEffect.EnemySpeed) value += .1f * Talent("\"Quick Production\" Tao");
            if (hero && effect == ItemEffect.Projectiles) value += Mathf.Min(3, Talent("Matchstick Head") * BondLevel("morning"));
            value += HeroInnate(OctoberHero, effect, scope, hero, ally);
            // Wise: the bought heroes' innate bonuses are his too, positive ones +50%
            if (OctoberHero == "Wise") foreach (string bought in WiseHeroRefs()) { float gift = HeroInnate(bought, effect, scope, hero, ally); value += gift > 0 ? gift * WiseHeroItems.GiftBoost : gift; }
            return value;
        }
        private float HeroInnate(string reference, ItemEffect effect, ModifierScope scope, bool hero, bool ally)
        {
            float value = 0;
            switch (reference)
            {
                case "Mr. Work": if (hero && effect == ItemEffect.Health) value += 5; if (hero && effect == ItemEffect.MoveSpeed) value += .05f; if (hero && effect == ItemEffect.Heal) value += .2f; break;
                case "Derpy Wolf": if ((hero || ally) && effect == ItemEffect.MoveSpeed) value += .3f; if (hero && effect == ItemEffect.Armor) value -= .15f; if (hero && (effect == ItemEffect.Crit || effect == ItemEffect.Dodge)) value += Mathf.Min(.6f, Mathf.Max(0, Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion))); break; // original: brigade speed +30; each 1% speed is +1% dodge and crit
                case "Sassy Senior": if ((hero || ally) && effect == ItemEffect.Damage) value += Mathf.Min(.5f, Mathf.Max(0, Bonus(ItemEffect.Luck)) * .5f); break;
                case "Baggy": if (effect == ItemEffect.Discount) value += .3f; if (hero && effect == ItemEffect.Pickup) value += 1000; if (hero && (effect == ItemEffect.Dodge || effect == ItemEffect.TreasureDetection)) value -= 1; break;
                case "Red Scale": if (hero && effect == ItemEffect.Health) value += State.OctoberDrinks * 2; if (hero && effect == ItemEffect.Armor) value -= Mathf.Min(.6f, State.OctoberDrinks * .05f); if (effect == ItemEffect.EnemyCount) value += .1f * Mathf.Max(0, State.OctoberDrinks - State.DrinksBeforeShop); break; // original: each drink armor -2, HP +2, next fight +10% monsters
                case "Gulpy": if ((hero || ally) && effect == ItemEffect.MoveSpeed || effect == ItemEffect.EnemySpeed) value += .3f; if (hero && effect == ItemEffect.Health) value += State.OctoberChests; break;
                case "Steel Shell": if (hero && effect == ItemEffect.CollisionDamage) value += 3 * BondLevel("gauntlet"); break;
                case "Box Shell": if ((hero || ally) && effect == ItemEffect.RangedDamage) value += 3; if ((hero || ally) && effect == ItemEffect.MeleeDamage) value -= 2; break;
                case "Omnom": { int steps = 1 + (State.Wave >= 3 ? 1 : 0) + (State.Wave >= 6 ? 1 : 0) + (State.Wave >= 9 ? 1 : 0); if ((hero || ally) && effect == ItemEffect.MoveSpeed || effect == ItemEffect.EnemySpeed) value += .3f * steps; if (effect == ItemEffect.EnemyCount) value += .2f; } break; // original: everyone +30 speed, again at waves 3/6/9; monsters +20%
                case "Periwinky": if ((hero || ally) && effect == ItemEffect.Damage) value -= .5f; if (hero && effect == ItemEffect.CollisionDamage) value += 15; break; // original: brigade damage -50%, collision +15
                case "Jade Cabbage": if (hero && effect == ItemEffect.Projectiles) value += Mathf.Min(6, State.LeafPurchases); if (hero && effect == ItemEffect.Health) value -= State.LeafPurchases * 2; break;
                case "The Big Pot": if (hero && effect == ItemEffect.CollisionDamage) value += 8; if (hero && effect == ItemEffect.Armor) value += BondLevel("morning") * .15f; if (effect == ItemEffect.EnemySpeed) value += .15f; break; // original: monsters +15% speed
                case "Porcelain Doll": if (hero && effect == ItemEffect.HealCorrection) value += 1.5f; if (hero && effect == ItemEffect.Armor) value -= .25f; if (hero && effect == ItemEffect.Heal) value += BondLevel("hide") * .4f; break;
                case "Slippy": if ((hero || ally) && effect == ItemEffect.Damage) value -= .55f; if ((hero || ally) && effect == ItemEffect.Knockback) value += 2; if (effect == ItemEffect.Income) value += 5 * BondLevel("gear"); if (effect == ItemEffect.TreasureDetection) value += .3f; break; // balance 24/10: brigade damage -55% (was -80%), knockback +200%, treasure +30, +5 income per Clockwork bond
                case "Card King": if (effect == ItemEffect.EnemyCount) value += .3f; break; // original: monsters +30%
                case "Sir Purrcival": if (hero && effect == ItemEffect.Dodge) value += .45f; if (hero && effect == ItemEffect.HealthCorrection) value -= .5f; break; // original: dodge +45%, health correction -50%
                case "Clementie": if (hero && effect == ItemEffect.HealthCorrection) value += 1f; if (hero && effect == ItemEffect.MoveSpeed) value += .3f; if (hero && effect == ItemEffect.Pickup) value -= .5f; // original: speed +30, pickup -50%, health correction +100%
                     if ((hero || ally) && effect == ItemEffect.AttackSpeed) value -= .25f; break;
                case "Bloom Shell": if (hero && effect == ItemEffect.Health) value += 10; if ((hero || ally) && (effect == ItemEffect.RangedDamage || effect == ItemEffect.MeleeDamage)) value += 2; if (hero && effect == ItemEffect.Armor) value += .15f; if ((hero || ally) && effect == ItemEffect.MoveSpeed) value += .05f; break;
                case "Belle":
                    // original: every friend may appear (K11HeroRules); monsters +30% and +60% health, +5% and +10% more each wave
                    if (effect == ItemEffect.EnemyHealth) value += .6f + .1f * Mathf.Max(0, State.Wave - 1);
                    if (effect == ItemEffect.EnemyCount) value += .3f + .05f * Mathf.Max(0, State.Wave - 1);
                    break;
                case "Wise": { int bought = WiseHeroRefs().Count; if (effect == ItemEffect.EnemyHealth) value += .1f * bought; if (effect == ItemEffect.EnemyCount) value += .01f * bought; } break; // each hero bought: enemies +10% health, +1% more (original)
                case "Nicole": if (effect == ItemEffect.Discount) value += .2f; if (effect == ItemEffect.Income) value += State.OctoberRefreshes; if (effect == ItemEffect.Luck) value += Mathf.Min(1, State.OctoberRefreshes * .01f); break;
                case "Anby": if (hero && effect == ItemEffect.Dodge) value += .2f; if (hero && effect == ItemEffect.CollisionDamage) value += 12; break;
                case "Billy": if (hero && effect == ItemEffect.Crit) value += .1f; if (hero && effect == ItemEffect.Projectiles) value += 2; break;
                case "Nekomata": if (hero && effect == ItemEffect.MoveSpeed) value += .2f; if (hero && effect == ItemEffect.AttackSpeed) value += .3f; if (effect == ItemEffect.Income) value -= ActiveRoster.Count; break;
                case "Burgerboo": if (hero && effect == ItemEffect.Armor) value += .4f; if (hero && effect == ItemEffect.MoveSpeed) value += .5f; if (hero && effect == ItemEffect.Pickup) value -= .5f; if (effect == ItemEffect.EnemySpeed) value += .75f; break; // original: speed +50, pickup -50%, monsters +75% speed
            }
            return value;
        }
        public bool OctoberItemEligible(ItemDefinition item)
        {
            if (item.ShopThemes == null || item.ShopThemes.Length == 0) return true; // shared (every Common item)
            foreach (string theme in catalog.Unit(State.ChampionId).RecruitThemes) if (Array.IndexOf(item.ShopThemes, theme) >= 0) return true;
            return false;
        }
        private void OctoberPurchase(UnitDefinition unit)
        {
            string talent = unit.ReferenceCompanion;
            if (OctoberHero == "Jade Cabbage" && talent == "Little Cabbage") State.LeafPurchases++;
            if (talent == "\"Expert Nurturer\" Daisy") State.Eggs = BoundedCounter((long)State.Eggs + 6);
            if (talent == "\"The Creator\" Roger") State.Gold = BoundedCounter((long)State.Gold + ScaledGold(100)); // on the gold curve (owner, 22/10/2026)
            if (talent == "\"GLHF\" Nia")
            {
                int pick = Next(ref State.WeaponRandom, 4); State.OctoberGains.Add(new StatModifier { Effect = new[] { ItemEffect.Damage, ItemEffect.MoveSpeed, ItemEffect.Luck, ItemEffect.Armor }[pick], Scope = pick == 2 ? ModifierScope.Run : pick == 3 ? ModifierScope.Champion : ModifierScope.Team, Value = pick == 3 ? .12f : .06f });
            }
            string[] gifts = talent == "\"Pet Master\" Lisa" ? new[] { "MonsterMarble", "MonsterMarble", "MonsterMarble", "MonsterMarble" } : talent == "\"Shove Around\" Bella" ? new[] { "BalloonMallet", "BalloonMallet" } : talent == "\"Smug and Satisfied\" Jake" ? new[] { "TeacupCricket", "RibbonOtter" } : Array.Empty<string>();
            foreach (string id in gifts)
            {
                if (OctoberAbsorbs) { if (State.AbsorbedSkills.Count < 2048) State.AbsorbedSkills.Add(id); continue; }
                if (catalog.Unit(State.ChampionId).HeroMechanic == HeroMechanic.SoloMaw) break;
                UnitDefinition friend = catalog.Unit(id); if (friend == null) continue;
                var choices = new List<WeaponDefinition>(); foreach (WeaponDefinition w in catalog.Weapons) if (RecruitWeaponEligible(id, w) && VariantFree(id, w)) choices.Add(w);
                if (choices.Count == 0) continue; var entry = new RosterEntry { UnitId = id, WeaponId = choices[Next(ref State.WeaponRandom, choices.Count)].Id }; State.Roster.Add(entry); RememberVariant(entry);
            }
        }
        private void OctoberBoughtRelic(ItemDefinition item)
        {
            if (OctoberHero == "Red Scale" && item.Category == RelicCategory.Drink) State.Gold = BoundedCounter((long)State.Gold + ScaledGold(1 + Mathf.Min(15, Mathf.FloorToInt(MaximumHealth() / 10))));
            if (OctoberHero != "Periwinky" || item.ReferenceName != "Spiky Yoga Ball") return;
            string id = "BristleBall";
            var pool = Array.FindAll(catalog.Weapons, w => RecruitWeaponEligible(id, w) && VariantFree(id, w));
            if (pool.Length == 0) return;
            var entry = new RosterEntry { UnitId = id, WeaponId = pool[Next(ref State.WeaponRandom, pool.Length)].Id };
            State.Roster.Add(entry); RememberVariant(entry);
        }
        private void OctoberSettled(CombatSimulation battle)
        {
            State.OctoberHearts = BoundedCounter((long)State.OctoberHearts + battle.OctoberHeartsCollected);
            State.OctoberChests = BoundedCounter((long)State.OctoberChests + battle.OctoberChestsOpened);
            void Gain(ItemEffect effect, float value)
            {
                if (value <= 0) return;
                StatModifier existing = State.OctoberGains.Find(m => m.Effect == effect && m.Scope == ModifierScope.Champion);
                if (existing == null) State.OctoberGains.Add(new StatModifier { Effect = effect, Scope = ModifierScope.Champion, Value = value });
                else existing.Value = Mathf.Min(effect == ItemEffect.CollisionDamage ? 80 : 6, existing.Value + value);
            }
            if (OctoberHero == "The Big Pot") Gain(ItemEffect.Heal, battle.OctoberInjuries * .15f);
            if (OctoberHero == "Porcelain Doll") Gain(ItemEffect.CollisionDamage, battle.OctoberInjuries);
            if (OctoberHero == "Sassy Senior") Gain(ItemEffect.Luck, battle.OctoberHeartsCollected * .02f);
            if (OctoberHero == "Sir Purrcival") Gain(ItemEffect.TreasureDetection, battle.OctoberDodges * .02f);
            RunAchievements.Mark(State.AchievementEvidence, "battle-gold", battle.Gold);
        }
        private int OctoberOfferTier(string id)
        {
            // owner 22/10/2026 (revised): 3 stars take three copies: 1+1 make 2 stars, then the copy is offered at the owned star
            int merge = OctoberMergeTier(id); if (merge > 0) return merge;
            if (OctoberHero == "Card King" && !State.Roster.Exists(r => r.UnitId == id)) return 2;
            if (ActiveRoster.Count >= 7 || State.Wave < 4 || State.Roster.Exists(r => r.UnitId == id)) return 1;
            int roll = Next(ref State.ShopRandom, 100);
            // a new species rolls 2 stars a little less often (55/30% -> 45/24%) and 3 stars much less (18% -> 6%) (owner 22/10/2026)
            return State.Wave >= 10 && ActiveRoster.Count <= 3 && roll < 6 ? 3 : roll < (ActiveRoster.Count <= 3 ? 45 : 24) ? 2 : 1;
        }
        private void OctoberRefreshed()
        {
            State.OctoberRefreshes++;
            State.Gold = BoundedCounter((long)State.Gold + ScaledGold(Mathf.Min(6, 2 * Talent("Smiley Fox"))));
        }
        /// <summary>The star a copy is offered at: the highest owned star still short of a merge (Evil never merges).</summary>
        private int OctoberMergeTier(string id)
        {
            if (catalog.Unit(id).ReferenceCompanion == "Evil") return 0;
            for (int tier = 2; tier >= 1; tier--) if (State.Roster.FindAll(r => r.UnitId == id && r.Tier == tier).Count >= CompanionDeployment.MergeCopies - 1) return tier;
            return 0;
        }
        private List<RosterEntry> AltarMaterials(AltarRecipe recipe)
        {
            var found = new List<RosterEntry>();
            foreach (string id in new[] { recipe.FirstUnit, recipe.SecondUnit, recipe.ThirdUnit })
            {
                if (string.IsNullOrEmpty(id)) continue;
                RosterEntry chosen = null;
                foreach (RosterEntry r in State.Roster) if (r.UnitId == id && r.Tier >= recipe.MinimumMaterialTier && !found.Contains(r) && (chosen == null || r.Tier < chosen.Tier)) chosen = r;
                if (chosen == null) return null; found.Add(chosen);
            }
            return found;
        }
        private void CompileOctoberRoster()
        {
            CombatCatalog.OctoberMemories = (State.AbsorbedSkills ?? new List<string>()).ConvertAll(id => catalog.Unit(id).ReferenceCompanion).ToArray();
            CombatCatalog.OctoberHealingReceived = K10StatRules.Factor(Bonus(ItemEffect.HealCorrection));
            foreach (UnitDefinition unit in CombatCatalog.StartingCompanions)
            {
                WeaponDefinition w = unit.Weapon; string t = unit.ReferenceCompanion ?? "";
                if (t == "Alien") w.Damage *= 1 + Mathf.Min(.4f, State.OctoberDrinks * .04f);
                if (t == "Student") { w.ProjectileCount = Mathf.Min(10, w.ProjectileCount + 2); w.Damage *= .65f; }
                if (t == "\"Unstoppable\" Paul") unit.CollisionDamage += 40;
                if (t == "Vengeful Spirit") unit.CriticalChance = Mathf.Min(1, unit.CriticalChance + .15f);
                if (t == "Iron Swordsman") w.Cooldown /= 1 + Mathf.Min(.3f, Talent(t) * .1f);
                if (t == "Prince") w.ProjectileCount += Mathf.Min(3, Talent("Princess"));
                if (t == "Princess") w.Range *= 1 + Mathf.Min(.24f, Talent("Prince") * .08f);
                if (t == "Smiley Fox") w.Damage *= 1 + Mathf.Min(.3f, State.OctoberRefreshes * .03f);
                if (t == "Pointy Hat Witch") w.Damage *= 1 + Mathf.Min(.3f, State.OctoberChests * .02f);
                if (t == "Phantom") w.Range *= 1 + Mathf.Min(.2f, State.OctoberHearts * .02f);
                if (t == "Pirate") w.Cooldown /= 1 + Mathf.Min(.2f, State.OctoberChests * .02f);
                if (t == "Jungle Folk") { int n = BondLevel("hide") + BondLevel("hare"); w.Damage *= 1 + Mathf.Min(.5f, n * .1f); w.Range *= 1 + Mathf.Min(.2f, n * .05f); }
                if (t == "\"Cars ARE My Kids\" Chris") { int n = Mathf.Min(3, Talent("Pony")); w.Damage *= 1 + n * .3f; unit.CollisionDamage += n * 5; }
                if (t == "Inflatable Hammer") { w.Damage *= .3f; w.PushDistance = .45f; }
                if (t == "Little Cabbage" || t == "Big Cabbage" || t == "Domestic Spiked Ball") unit.CollisionDamage = t == "Little Cabbage" ? 12 : t == "Big Cabbage" ? 24 : 18;
                bool Tagged(string tag) => Array.IndexOf(unit.Tags, tag) >= 0;
                if (Tagged("orchard")) w.Cooldown /= 1 + Mathf.Min(.24f, Talent("Applehead") * .08f);
                if (Tagged("gear")) { float bonus = Mathf.Min(.25f, Talent("Bullhorn Warrior") * .08f + Talent("Intelligent Construct") * .04f); w.Cooldown /= 1 + bonus; unit.Speed *= 1 + bonus; }
                if (Tagged("rune") && Talent("Little Witch") > 0) { w.Cooldown *= 1.15f; w.ProjectileVisualWidth *= 1.3f; w.ProjectilePierce += 2; }
                if (Tagged("shoe") && Talent("Monkey King") > 0) { w.Cooldown /= 1.1f; unit.Speed *= 1.1f; }
                if (Tagged("lantern-guild") && Talent("Original:SpindleWren") > 0) w.Range *= 1.08f;
                if (Tagged("knight") && OctoberHero == "Sir Purrcival") { w.Damage *= 2f; unit.CollisionDamage += 10; } // original: Knight friends +100% damage, +10 collision
                if (Tagged("regal") && OctoberHero == "Card Queen") w.ProjectileCount++;
                if (Talent("\"Formatting Pixie\" Quentin") > 0) { unit.BodyHeight *= .88f; unit.Radius *= .88f; }
                if (OctoberHero == "Card King")
                {
                    // he never attacks himself (OctoberBlocksAttack): his own bonuses carry over to the friends (caps 100% / 50%)
                    UnitDefinition main = CombatCatalog.Champion; w.Damage *= 1 + Mathf.Min(1f, Mathf.Max(0, Bonus(ItemEffect.ChampionDamage, ModifierScope.Champion)));
                    w.Cooldown /= 1 + Mathf.Min(.5f, Mathf.Max(0, Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion)));
                    unit.BodyHeight *= 1.2f; unit.CollisionDamage += Mathf.Max(0, main.CollisionDamage - 3) * .5f;
                    unit.CriticalChance = Mathf.Max(unit.CriticalChance, main.CriticalChance); w.ProjectileCount = Mathf.Min(10, w.ProjectileCount + Mathf.Max(0, main.Weapon.ProjectileCount - 1));
                }
                if (OctoberHero == "Clementie") unit.CollisionDamage += CombatCatalog.Champion.Speed * 2.5f;
                if (OctoberHero == "Frosty") { w.Damage *= Mathf.Max(.65f, 1 - ActiveRoster.Count * .02f); unit.BodyHeight *= Mathf.Max(.75f, 1 - ActiveRoster.Count * .01f); }
                w.Damage*=1.08f;unit.CollisionDamage*=1.08f;unit.Speed=Mathf.Min(10,unit.Speed*1.05f);
                w.Range = Mathf.Min(w.Range, w.Kind == WeaponKind.Melee ? FriendlyReach.MeleeMaximum : FriendlyReach.RangedMaximum); w.Cooldown = Mathf.Max(.28f, w.Cooldown);
            }
            if (OctoberHero == "Periwinky") CombatCatalog.Champion.Health = BloodPrice.Apply(Mathf.Clamp(State.Gold, 20, 400), State.BloodPaid);
            if (OctoberHero == "Card Queen") CombatCatalog.Champion.Health = BloodPrice.Apply(PlayerBuildBalance.CardQueenHealth, State.BloodPaid);
            if (OctoberHero == "Billy") CombatCatalog.Champion.Weapon.ProjectileSplit = 0;
            if (OctoberHero == "Omnom" || OctoberHero == "Card Queen") CombatCatalog.PassiveHeal = 0;
            if (OctoberHero == "Burgerboo") { CombatCatalog.Champion.Weapon.ProjectileCount = Mathf.Min(2, CombatCatalog.Champion.Weapon.ProjectileCount); CombatCatalog.Champion.Weapon.ProjectilePierce = 0; }
            if (OctoberHero == "Clementie") { CombatCatalog.CompanionLeash = 1.8f; CombatCatalog.Champion.CollisionDamage += CombatCatalog.Champion.Speed * 4f; } // she never attacks: her body does
            if (OctoberHero == "Anby") CombatCatalog.Champion.CollisionDamage *= 3; // she only attacks while transformed; her launch (contact) does the work
            if (OctoberHero == "Frosty") { CombatCatalog.Champion.BodyHeight *= 1 + Mathf.Min(.35f, ActiveRoster.Count * .02f); CombatCatalog.Champion.Weapon.ProjectileSplit = Mathf.Max(3, CombatCatalog.Champion.Weapon.ProjectileSplit); } // original: bullet division +3
        }
    }
}
