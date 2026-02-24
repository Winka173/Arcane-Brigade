using System;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class CompanionDescriptions
    {
        public static string Talent(UnitDefinition u)
        {
            string name=u.ReferenceCompanion??"";
            return name switch {
                "Alien" => "Each DRINK purchased during this run adds 4% to this ally's weapon damage, up to +40%. Applies when the next battle is prepared; not a team-wide damage buff.",
                "Applehead" => "Fruit Basket companions gain 8% attack speed per Applehead talent, up to 24%. Does not affect the hero. Stacks by talent bodies/memories, not star level.",
                "Bullhorn Warrior" => "Clockwork Union companions gain 8% movement and attack speed per talent. Combined with Intelligent Construct, this buff is capped at 25%.",
                "Intelligent Construct" => "Clockwork Union companions gain 4% movement AND attack speed per talent. Combined with Bullhorn Warrior, this buff is capped at 25%.",
                "Fierce Fighter" => "Each basic attack adds 1% attack speed to this ally, up to 20% this wave. A hit from every third basic attack can make a 0.8-radius blast at 15% weapon damage; 1s cooldown, no recursive blast.",
                "Grey Mage" => "Your basic attack prompts this ally to fire one bolt for 65% of its own weapon damage. 0.7s cooldown per responding ally. Secondary attacks cannot trigger another response.",
                "Hopping Zombie" => "Hero maximum HP +4 per talent. Each 8 team kills gives this ally 1% attack speed, up to 15% this wave.",
                "Iron Swordsman" => "This ally gains 10% attack speed per Iron Swordsman talent, including itself, up to 30%. Count includes absorbed talents; star levels do not multiply this count.",
                "Jungle Folk" => "Each Wild Stitch or Springstep bond level grants this ally 10% weapon damage and 5% reach. Caps: +50% damage, +20% reach; final reach cap still applies.",
                "Little Witch" => "Runic School companions gain +2 projectile pierce and 30% wider projectile visuals, but their attack cooldown is 15% longer. This talent is a presence effect and does not stack with another copy.",
                "Masked Wrestler" => "Each injury recorded on the hero this wave gives Prizefighter companions 1% attack speed, up to 15%. Multiple copies do not multiply this speed bonus.",
                "Mummy" => "A basic hit schedules a poison strike after 1s for 25% of this ally's weapon damage. The target and source must still be alive. Poison cannot create another poison chain.",
                "Phantom" => "Hero armor -0.08 per talent. Every heart collected during the run gives this ally 2% reach, up to 20%, applied when the next battle is prepared.",
                "Pirate" => "Each chest opened during the run adds 2% attack speed to this ally, up to 20%, applied when the next battle is prepared.",
                "Pointy Hat Witch" => "Each chest opened during the run adds 2% weapon damage to this ally, up to 30%, applied when the next battle is prepared.",
                "Smiley Fox" => "Each paid shop refresh grants 2 gold per talent immediately, capped at 6 gold total per refresh. Each refresh during the run also adds 3% weapon damage to this ally, up to 30%, for the next prepared battle.",
                "Cheer Bunny" => "Hero and all companions gain 5% movement speed per talent. Applies while owned, subject to final speed limits.",
                "Chieftain" => "Hero and all companions gain 5% weapon damage per talent. Applies while owned before later damage modifiers.",
                "Floofy" => "Hero maximum HP +5 and armor +0.08 per talent. These are hero-only bonuses, not extra health for immortal companions.",
                "Judge" => "Clear-wave income gains 4 gold per talent per active Lucky Leaves bond LEVEL. This uses the current bond level; repeated combat activations do not accumulate more income.",
                "Matchstick Head" => "Hero projectile count gains 1 per talent per active Morning Bells bond LEVEL, capped at +3 total from this talent. Requires a projectile attack; final projectile limits still apply.",
                "Monkey King" => "Quicksole companions gain 10% movement and attack speed while at least one Monkey King talent is owned. Presence effect; extra copies do not multiply it.",
                "\"Quick Production\" Tao" => "Hero and all companions gain 20% weapon damage and 10% movement speed per talent. Enemies also gain 10% movement speed per talent. This does not grant attack speed.",
                "Student" => "This ally fires 2 extra projectiles, up to 10 total, and its weapon damage is multiplied by 0.65. Extra projectiles require a projectile weapon to fire normally.",
                "Prince" => "Each Princess talent grants this ally +1 projectile, up to +3. Projectile count helps projectile weapons; this does not turn a melee weapon into a ranged weapon.",
                "Princess" => "Each Prince talent grants this ally 8% reach, up to 24%. Final melee/ranged reach caps still apply.",
                "TV Head" => "A critical basic hit has a 25% chance to grant 1 gold. All TV Head talents share a 0.8s cooldown; extra hits cannot bypass it.",
                "\"Art of Food\" Roshi" => "Each FOOD purchased adds +1 flat melee AND +1 flat ranged team damage per Roshi talent. Hero maximum HP loses 0.5 per food per talent, up to 20 HP lost. Applied in the next prepared battle.",
                "\"Colorful Fantasy\" Pinky" => "An ordinary incoming hit is reduced by 25% if at least 6s have passed since the last recorded hero injury. This protection does not stack from multiple copies; deliberate health costs are not reduced.",
                "\"Enduring Suffering\" Dorothy" => "Once per wave, a recorded hero injury permanently removes 2 maximum HP for that battle and then attempts a full heal. Royal health cannot be refilled; hero healing restrictions still apply. Shared across copies.",
                "\"Expert Nurturer\" Daisy" => "ON RECRUIT / ALTAR CREATION: gain 6 eggs immediately once. Merely keeping this ally does not grant 6 eggs every wave.",
                "\"Formatting Pixie\" Quentin" => "All companion bodies and hitboxes are 12% smaller. Presence effect: multiple copies do not shrink them repeatedly.",
                "\"GLHF\" Nia" => "ON RECRUIT / ALTAR CREATION: gain one seeded random run bonus: +6% team damage, +6% team movement speed, +0.06 Luck, or +0.12 hero armor. Once per acquisition.",
                "\"Leisurely Walk\" Mary" => "Paid refreshes cost 2 less gold per talent, minimum 1 gold. Applies to both normal and dream-stock refreshes.",
                "\"Misfortune\" Emily" => "A recorded hero injury grants 3 gold. Shared 2s cooldown; shares the rebate timer with TV Head. Multiple copies do not multiply the payout.",
                "\"Naming Demon\" Yan" => "Every third basic attack may fire a coloured echo for 25% weapon damage. Shares this ally's 0.7s response cooldown. The echo cannot trigger itself.",
                "\"Overtime King\" Philip" => "Wave income +15 per talent. During combat, the hero spends 0.5 HP every 5s while at least one Philip talent is present; this health cost does not stack by copies and bypasses armor/dodge.",
                "\"The Creator\" Roger" => "ON RECRUIT / ALTAR CREATION: gain 100 gold immediately once. No recurring payment just for keeping this ally.",
                "\"The All-Knowing\" Walter" => "Every bond gains +1 supporting count while at least one Walter talent is owned. Additional Walters and star levels do not add more supporting counts.",
                "Evil" => "Cannot merge. Each 8 kills credited to Evil allies in total gives the hero 2% attack speed, up to 20% this wave. Shared across Evil talents.",
                "Original:TeacupCricket" => "Every fifth basic attack may release one echo for 40% of this ally's weapon damage. Uses the ally's 0.7s response cooldown; no echo chain.",
                "Original:RibbonOtter" => "Collecting a heart attempts 1 extra HP of healing while at least one Ribbon Otter talent is owned. Extra healing does not stack by copies; hero healing restrictions apply.",
                "Original:SpindleWren" => "Lantern Guild companions gain 8% reach while at least one Spindle Wren talent is owned, subject to final range caps. Presence effect; extra copies do not multiply it.",
                _ => string.IsNullOrEmpty(name) ? u.Id switch { "SecretPatchBear" => "A living resident rescued from the Lost-Property Vault. Permanent friend; its support and assigned weapon are listed below.", "SecretPaperBird" => "A folded courier rescued from the Lost-Property Vault. Permanent friend; remembers your letter and provides periodic healing.", "SecretDreamLamb" => "Garden visitor for two normal-wave victories. The dream friend then departs; its support ends with the contract.", _ => u.Description } : OctoberCompanions.Copy(name)
            };
        }
        public static string Passive(UnitDefinition u,int tier,float support=3)
        {
            float strength=(u.Passive==PassiveKind.Scavenge?Mathf.Round(u.PassiveStrength):u.PassiveStrength)*Mathf.Pow(support,Mathf.Clamp(tier,1,3)-1);
            return u.Passive switch {
                PassiveKind.HealChampion => "Every 4s in combat, attempt to heal the hero for "+(strength*3).ToString("0.###")+" HP before healing modifiers. Healing restrictions apply.",
                PassiveKind.Scavenge => "Every 4s in combat, gain "+Mathf.RoundToInt(strength)+" gold directly. This is not kill loot and does not count as collecting a coin.",
                PassiveKind.ChillAura => "Every 4s, chill enemies within "+(2+strength*.625f).ToString("0.###")+" units: 1.5s normally, 0.6s on bosses.",
                PassiveKind.Rally => "Own weapon damage gains "+(strength).ToString("0.###")+"% per companion body in the roster, before later balance modifiers.",
                _ => "No additional periodic personal passive."
            };
        }
        public static string Describe(UnitDefinition u,int tier=1,float support=3)
        {
            var b=new StringBuilder();b.AppendLine("COMPANION / "+RunModifierRules.RarityName(u.Rarity)+" / "+tier+" STAR");
            b.AppendLine("TALENT\n"+Talent(u));
            b.AppendLine("\nSUPPORT WHILE OWNED\n"+RunModifierRules.DescribeCompanion(u,tier));
            b.AppendLine("\nLIFE IN THE WAITING REALM\n" + CreatureLives.Origin(u.Id));
            b.AppendLine("PERSONAL PASSIVE\n"+Passive(u,tier,support));
            if(!string.IsNullOrEmpty(u.ReferenceCompanion)) b.AppendLine("\nNamed talents count bodies and absorbed memories; their stated percentages are not multiplied by star level unless explicitly stated. Listed support modifiers scale separately at 1/2/4.");
            if(u.GiftOnly)b.AppendLine("Source: gift / nursery / special reward; not ordinary shop stock.");
            else if(!string.IsNullOrEmpty(u.ExclusiveHero))b.AppendLine("Recruitment exclusive: "+u.ExclusiveHero+".");
            else if(u.SpecialRecruit)b.AppendLine("Source: dream shop or special reward, subject to hero theme and unlock wave.");
            b.AppendLine("Bonds: "+string.Join(", ",u.Tags??Array.Empty<string>())+". Immortal. Weapon rolls on recruitment; three equal-species/equal-star bodies merge unless the talent forbids it.");
            return b.ToString().Trim();
        }
    }

    public static class RelicDescriptions
    {
        public static string Category(ItemDefinition item)=>item.Category==RelicCategory.Drink?"DRINK":item.Category==RelicCategory.Food?"FOOD":"CURIO";
        public static string Mechanic(RelicMechanic m)=>m switch {
            RelicMechanic.None=>"",
            RelicMechanic.CrowdedTape=>"Next wave: +10% enemy count per purchased copy. Its listed permanent modifiers remain separate.",
            RelicMechanic.CoinMending=>"Each coin pickup has a 15% chance to heal 2 HP per active stack.",
            RelicMechanic.CompoundInterest=>"At each wave start, multiply saved gold by 1.2 per stack, compounded. Separate from normal clear-wave interest.",
            RelicMechanic.RetortVolley=>"When hit, 30% chance to fire 8 radial shots for 12 damage per stack each. Shots move at 5 units/s and last 2s.",
            RelicMechanic.ExposedRecovery=>"Next wave: hero armor -0.8 per purchased copy. Listed recovery modifiers remain separate.",
            RelicMechanic.TreasureLease=>"Next wave: Treasure Detection +1 per purchased copy.",
            RelicMechanic.PrismEcho=>"A personal basic shot can strike one random enemy for max(2,2 times hero flat ranged bonus) damage per stack. Shared 0.15s cooldown; no recursive response.",
            RelicMechanic.RerollFeather=>"Refreshes cost 2 less gold per active stack; minimum 1 gold.",
            RelicMechanic.ChestLightning=>"Opening a chest strikes 1-3 different enemies for max(8,100 times Treasure Detection) damage per stack.",
            RelicMechanic.DrinkVitality=>"ON EACH DRINK PURCHASE: permanently gain +4 maximum HP per active stack for this run. A drink carrying this effect counts itself when purchased.",
            RelicMechanic.DrinkTempo=>"ON EACH DRINK PURCHASE: permanently gain +6% team attack speed per active stack for this run. Drink shop weight also increases 5% per stack.",
            RelicMechanic.PanicMagnet=>"When hit, 5% chance to attract all pickups currently on the floor.",
            RelicMechanic.ChestLuck=>"Opening a chest permanently adds +0.06 Luck per active stack for this run.",
            RelicMechanic.ChaosIncome=>"At each wave start, gain a seeded random 25-75 gold per active stack.",
            RelicMechanic.HoardPower=>"Team damage bonus +1% per 5 gold held, per stack. Changes as collected combat gold grows your wallet.",
            RelicMechanic.HalfHealthSeed=>"At each battle start, current hero HP is reduced to at most 50% maximum HP; it does not reduce maximum HP.",
            RelicMechanic.StillRecovery=>"Standing still heals 3 HP/s per active stack, subject to hero healing restrictions.",
            RelicMechanic.CoinLightning=>"Each coin pickup strikes one enemy for max(4,50 times Luck) damage per active stack.",
            RelicMechanic.TurnDiscount=>"ON PURCHASE: other unsold gold-shop relics cost 30% less for the current shop. Also adds 30% discount to newly generated prices that shop, subject to the total discount cap.",
            RelicMechanic.CloseFollowers=>"Companion leash and outer patrol radius are multiplied by 0.65. Presence effect; extra stacks do not tighten it again.",
            RelicMechanic.WaveGrowth=>"At every wave start, permanently gain +4 flat melee damage, +4 flat ranged damage and +10% team damage per stack for this run.",
            RelicMechanic.MeleeToCollision=>"Hero contact damage gains 50% of the positive hero flat melee bonus, per stack.",
            RelicMechanic.SlowToArmor=>"Hero armor gains the absolute value of negative hero movement-speed bonus, per stack.",
            RelicMechanic.DodgeMending=>"On a successful dodge, 30% chance to heal 10 HP per active stack.",
            RelicMechanic.DamageToIncome=>"Wave income gains 1 per full +4% positive hero damage bonus, per stack.",
            RelicMechanic.ChestOnlyRecovery=>"Disable passive regeneration and ordinary heart healing. Opening a chest attempts to heal 30 HP per stack. Other explicit healing effects still follow their own rules.",
            RelicMechanic.SorrowPulse=>"Every 3s, all eligible enemies take damage equal to 20% of hero maximum HP per stack.",
            RelicMechanic.BleedingAlarm=>"During combat, take 1 injected damage per stack each second. Armor and other injected-damage rules still apply; this differs from hunger's direct HP cost.",
            RelicMechanic.CollisionOnly=>"Hero basic attacks stop. Contact damage gains 10 times positive armor per stack. Independent triggered attacks may still fire.",
            RelicMechanic.SoloOverdrive=>"NEXT WAVE ONLY: companions stop their basic attacks and hero attack cooldown is divided by 6, subject to the attack loop's cap. Every 10 team kills attempts to heal 3 HP.",
            RelicMechanic.FreshLotus=>"Start each wave with +3 pierce and +30% crit per stack. The temporary bonus ends on the first recorded hero hit.",
            RelicMechanic.EggGift=>"ON PURCHASE: receive 3 eggs immediately once per acquisition.",
            RelicMechanic.CompanionGift=>"ON PURCHASE: try to recruit up to 2 ordinary companions from your hero's pool. Gifts use the 3-companion regular purchase quota. At most 1 new species per wave; from wave 10 both shops share one new species every 2 waves. Checks unlock wave and unique weapon; no body cap. Solo heroes receive no bodies.",
            RelicMechanic.FragilePower=>"Each recorded hero hit removes 5% team damage per stack, including a saved run penalty. Its positive listed modifiers are separate.",
            RelicMechanic.StatFloor=>"ON PURCHASE: record current modifier bonuses as floors; future bonuses cannot fall below those recorded floors. Does not remove every direct combat restriction or HP cost.",
            RelicMechanic.LuckToHealth=>"Maximum HP gains 100 times positive Luck per stack, before HP correction and limits.",
            RelicMechanic.GambleRecovery=>"Every 3s make two independent rolls: 50% to heal 20 HP per stack; 50% to take 8 injected damage per stack. Both or neither can occur.",
            RelicMechanic.FoodGrowth=>"ON EACH FOOD PURCHASE: permanently gain +10 max HP, +0.5 HP/s and +5% hero size per active stack for this run.",
            RelicMechanic.FreeFollowers=>"Companion outer patrol radius becomes half the arena's diagonal. Does not add extra companion bodies.",
            RelicMechanic.IncomePoison=>"Hero basic hits poison the target for 4s. Each 1s tick deals max(1,base wave income plus income bonus) damage. Refreshes duration, not an unbounded stack of poisons.",
            RelicMechanic.HurtGrowth=>"Each recorded hero hit randomly adds one run bonus per stack: +4 max HP, +0.08 HP/s, +0.015 armor, +2% team damage, +0.02 Luck or +1% crit.",
            RelicMechanic.StillPower=>"During each battle, +16 flat attack damage per stack until you move for the first time. Stopping later does not restore this bonus.",
            RelicMechanic.StaticFollowers=>"Companions stop moving; their range and projectile lifetime double and pierce increases by 3. Final compile range/cooldown limits still apply where enforced.",
            RelicMechanic.VoidPulse=>"Every 3s, all eligible enemies lose damage equal to 15% of their CURRENT HP per stack. Not an instant execution.",
            RelicMechanic.SixWeapons=>"ON FIRST PURCHASE: remember 6 random weapon IDs. Replace ordinary team basic attacks with six phantom weapons fired from the hero. Each uses compiled weapon stats and its own cooldown, minimum 0.2s.",
            _=>throw new ArgumentOutOfRangeException(nameof(m),m,"Missing relic mechanic copy")
        };
        public static string Signature(SignatureRelicKind role)=>role switch {
            SignatureRelicKind.None=>"", SignatureRelicKind.HarvestReceipt=>"Requires a bell signature weapon within 6 units. Collecting a coin advances its bell beat, up to 2 steps; shared 0.35s cooldown.",
            SignatureRelicKind.ReflectiveInk=>"Stamp hits add 2 marks instead of 1, but mark lifetime is 65% of normal. Stamp exposure and marked-target synergies remain active.",
            SignatureRelicKind.CarbonRibbon=>"A stamp burst attracts coins within 2.5 units of its target. Bell pulses expose surviving targets for 1.5s. Presence effect; requires matching stamp/bell attacks.",
            SignatureRelicKind.OfficeLens=>"Critical hits on an exposed enemy gain a separate damage multiplier: 1.25 + min(0.25,0.05 times stacks). Requires exposure and a critical hit.",
            SignatureRelicKind.TeaCoaster=>"Tea projectile trails have 40% more radius but deal 20% less trail damage. Presence effect; extra copies do not multiply trail size again.",
            SignatureRelicKind.ScratchRecord=>"Music-box hits add a delayed echo after 0.3s, damage min(70%,35% times stacks). Bell pulses also add up to 50% extra damage against marked survivors.",
            SignatureRelicKind.TreasureRibbon=>"Opening a chest heals 3 HP per stack and empowers bow-motion hits for 4s: up to +45% damage, +15% per stack.",
            SignatureRelicKind.PatchSpring=>"Every 3s, automatically add one charge to each charging arena toy. Normal warning and cooldown rules still apply. Presence effect; extra copies do not speed it up.",
            SignatureRelicKind.ThermalInk=>"A hot trail touching a marked enemy adds a 40%-trail-damage strike, with a 0.9s cooldown per target.",
            SignatureRelicKind.RepairCoupon=>"An arena-toy credited kill heals 1 HP per stack, shared 0.6s cooldown. Ordinary basic kills do not activate this effect.",
            _=>throw new ArgumentOutOfRangeException(nameof(role),role,"Missing signature copy")
        };
        public static string Describe(ItemDefinition item,int stacks=1,RunCatalog catalog=null)
        {
            var b=new StringBuilder();b.AppendLine(Category(item)+" / "+RunModifierRules.RarityName(item.Rarity)+" / "+(item.Stacking==StackPolicy.Unique?"UNIQUE":"STACKABLE"));
            if(item.Category==RelicCategory.Drink)b.AppendLine("ON PURCHASE: counts as 1 drink for drink-reactive heroes, companions and relics. No separate use action; the item stays owned.");
            if(item.Category==RelicCategory.Food)b.AppendLine("ON PURCHASE: counts as 1 food for food-reactive heroes, companions and relics. No separate use action; the item stays owned.");
            b.AppendLine("\nWHILE OWNED / per copy");
            bool any=false;foreach(var m in item.EffectiveModifiers)if(m.Value!=0){b.AppendLine(RunModifierRules.ScopeName(m.Scope)+" / "+RunModifierRules.ModifierName(m.Effect)+": "+RunModifierRules.ValueText(m.Effect,m.Value)+(item.EffectiveStacks(stacks)>1?" each / owned total "+RunModifierRules.ValueText(m.Effect,m.Value*item.EffectiveStacks(stacks)):""));any=true;}
            if(!any)b.AppendLine("No direct stat modifier; effects are described below.");
            string mechanic=Mechanic(item.Mechanic),signature=Signature(item.SignatureRole);
            if(mechanic.Length>0)b.AppendLine("\nSPECIAL EFFECT\n"+mechanic);
            if(signature.Length>0)b.AppendLine("\nWEAPON SYNERGY\n"+signature);
            RunCatalog c=catalog!=null?catalog:Resources.Load<RunCatalog>("RunCatalog");
            if(c!=null)foreach(var p in c.ProcRules??Array.Empty<ProcRule>())if(p.ItemId==item.Id)b.AppendLine("\nTRIGGER: "+p.Trigger+" / "+(p.Chance*100).ToString("0.###")+"% chance / "+p.Cooldown.ToString("0.###")+"s cooldown. "+(p.Action==ProcAction.HealChampion?"Heal hero ":"Damage pulse ")+p.Power.ToString("0.###")+" per stack; radius "+p.Radius.ToString("0.###")+".");
            b.AppendLine("\nOwned effective copies: "+item.EffectiveStacks(stacks)+"."+(item.StackLimit<100000?" Stack limit: "+item.StackLimit+".":"")+" Values apply before combat limits.");
            return b.ToString().Trim();
        }
        public static string PurchaseReactions(RunSession s,RunCatalog c,ItemDefinition item)
        {
            var b=new StringBuilder();
            int Talents(string name){int n=0;foreach(var r in s.State.Roster)if(c.Unit(r.UnitId).ReferenceCompanion==name)n++;foreach(var id in s.State.AbsorbedSkills)if(c.Unit(id).ReferenceCompanion==name)n++;return n;}
            int Stacks(RelicMechanic m)=>s.MechanicStacks(m)+(item.Mechanic==m?1:0);
            if(item.Category==RelicCategory.Drink)
            {
                int alien=Talents("Alien");if(alien>0)b.AppendLine("Your "+alien+" drink-reactive companion/memory talent(s): each relevant companion gets +4% weapon damage, cap +40%, for the next battle.");
                int hp=Stacks(RelicMechanic.DrinkVitality),speed=Stacks(RelicMechanic.DrinkTempo);
                if(hp>0)b.AppendLine("Drink relics: gain +"+(hp*4)+" maximum HP this run.");
                if(speed>0)b.AppendLine("Drink relics: gain +"+(speed*6)+"% team attack-speed bonus this run, before soft caps.");
                if(s.State.ChampionId=="TideSinger")b.AppendLine("Tide Singer: +2 max HP, -0.02 armor (penalty cap 0.4), +2% enemies (cap 40%); refund 1 + floor(new max HP/10) gold, up to 16. Every second drink adds a tide note, up to 3 extra.");
            }
            if(item.Category==RelicCategory.Food)
            {
                int roshi=Talents("\"Art of Food\" Roshi");if(roshi>0)b.AppendLine("Food talents: +"+roshi+" flat melee and ranged team damage. Hero max HP penalty increases by "+(roshi*.5f).ToString("0.###")+", capped at 20 total, before HP limits.");
                int growth=Stacks(RelicMechanic.FoodGrowth);if(growth>0)b.AppendLine("Food relics: +"+(growth*10)+" maximum HP, +"+(growth*.5f).ToString("0.###")+" HP/s and +"+(growth*5)+"% body size this run.");
                if(s.State.ChampionId=="SupperMortar")b.AppendLine("Supper Mortar: food expands shell radius by 0.025, up to +0.35 units.");
            }
            if(s.State.ChampionId=="ThornTreasurer" && item.ReferenceName=="Spiky Yoga Ball")b.AppendLine("Thorn Treasurer: buying this relic also attempts to recruit a Bristle Ball, subject to unused weapon variants.");
            if((s.State.ChampionId=="NightMaw"||s.State.ChampionId=="IvorySovereign") && (Array.Exists(item.EffectiveModifiers,m=>m.Effect==ItemEffect.Heal||m.Effect==ItemEffect.HeartHealing)||item.Mechanic==RelicMechanic.StillRecovery))b.AppendLine("Your hero restricts ordinary healing; these recovery effects cannot bypass that rule.");
            return b.Length==0?"":"\n\nON PURCHASE / YOUR CURRENT BRIGADE\n"+b.ToString().Trim();
        }
    }
}
