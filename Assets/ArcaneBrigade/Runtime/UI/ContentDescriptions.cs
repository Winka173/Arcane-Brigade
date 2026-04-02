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
                "Alien" => "Each DRINK purchased during this run adds 4% to this friend's weapon damage, up to +40%. not a team-wide damage buff.",
                "Applehead" => "Orchard friends gain 8% attack speed per Applehead talent, up to 24%. Does not affect the hero. Stacks by talent bodies/memories, not star level.",
                "Bullhorn Warrior" => "Clockwork Union friends gain 8% movement and attack speed per talent. Combined with Intelligent Construct, this buff is capped at 25%.",
                "Intelligent Construct" => "Clockwork Union friends gain 4% movement AND attack speed per talent. Combined with Bullhorn Warrior, this buff is capped at 25%.",
                "Fierce Fighter" => "Each basic attack adds 1% attack speed to this friend, up to 20% this wave. A hit from every third basic attack can make a 0.8-radius blast at 15% weapon damage; once every 1s, no recursive blast.",
                "Grey Mage" => "Your basic attack prompts this friend to fire one bolt for 65% of its own weapon damage. once every 0.7s. Secondary attacks cannot trigger another response.",
                "Hopping Zombie" => "Hero maximum HP +4 per talent. Each 8 team kills gives this friend 1% attack speed, up to 15% this wave.",
                "Iron Swordsman" => "This friend gains 10% attack speed per Iron Swordsman talent, including itself, up to 30%. Count includes absorbed talents; star levels do not multiply this count.",
                "Jungle Folk" => "Each Wild Stitch or Springstep bond level grants this friend 10% weapon damage and 5% reach. Caps: +50% damage, +20% reach; final reach cap still applies.",
                "Little Witch" => "Runic School friends gain +2 projectile pierce and 30% wider projectile visuals, but they attack 15% slower. Extra copies add nothing.",
                "Masked Wrestler" => "Each injury recorded on the hero this wave gives Prizefighter friends 1% attack speed, up to 15%. Multiple copies do not multiply this speed bonus.",
                "Mummy" => "A basic hit schedules a poison strike after 1s for 25% of this friend's weapon damage. The target and source must still be alive. Poison cannot create another poison chain.",
                "Phantom" => "Hero armor -0.08 per talent. Every heart collected during the run gives this friend 2% reach, up to 20%.",
                "Pirate" => "Each chest opened during the run adds 2% attack speed to this friend, up to 20%.",
                "Pointy Hat Witch" => "Each chest opened during the run adds 2% weapon damage to this friend, up to 30%.",
                "Smiley Fox" => "Each paid shop refresh grants 2 gold per talent immediately, capped at 6 gold total per refresh. Each refresh during the run also adds 3% weapon damage to this friend, up to 30%, for the next prepared battle.",
                "Cheer Bunny" => "Hero and all friends gain 5% movement speed per talent. Applies while owned.",
                "Chieftain" => "Hero and all friends gain 5% weapon damage per talent. Applies while owned.",
                "Floofy" => "Hero maximum HP +5 and armor +0.08 per talent. These are hero-only bonuses, not extra health for immortal friends.",
                "Judge" => "Clear-wave income gains 4 gold per talent per active Lucky Leaves bond LEVEL. This uses the current bond level; repeated combat activations do not accumulate more income.",
                "Matchstick Head" => "Hero projectile count gains 1 per talent per active Morning Bells bond LEVEL, capped at +3 total from this talent. Requires a projectile attack; final projectile limits still apply.",
                "Monkey King" => "Quicksole friends gain 10% movement and attack speed while at least one Monkey King talent is owned. Extra copies add nothing.",
                "\"Quick Production\" Tao" => "Hero and all friends gain 20% weapon damage and 10% movement speed per talent. Enemies also gain 10% movement speed per talent. This does not grant attack speed.",
                "Student" => "This friend fires 2 extra projectiles, up to 10 total, and its weapon damage is multiplied by 0.65. Extra projectiles require a projectile weapon to fire normally.",
                "Prince" => "Each Princess talent grants this friend +1 projectile, up to +3. Projectile count helps projectile weapons; this does not turn a melee weapon into a ranged weapon.",
                "Princess" => "Each Prince talent grants this friend 8% reach, up to 24%. Final melee/ranged reach caps still apply.",
                "TV Head" => "A critical basic hit has a 25% chance to grant 1 gold. All TV Head talents share a once every 0.8s; extra hits cannot bypass it.",
                "\"Art of Food\" Roshi" => "Each FOOD purchased adds +1 flat melee AND +1 flat ranged team damage per Roshi talent. Hero maximum HP loses 0.5 per food per talent, up to 20 HP lost. Applied in the next prepared battle.",
                "\"Colorful Fantasy\" Pinky" => "An ordinary incoming hit is reduced by 25% if at least 6s have passed since the last recorded hero injury. This protection does not stack from multiple copies; deliberate health costs are not reduced.",
                "\"Enduring Suffering\" Dorothy" => "Once per wave, a recorded hero injury permanently removes 2 maximum HP for that battle and then attempts a full heal. Royal health cannot be refilled; hero healing restrictions still apply. Shared across copies.",
                "\"Expert Nurturer\" Daisy" => "ON RECRUIT / ALTAR CREATION: gain 6 eggs immediately once. Merely keeping this friend does not grant 6 eggs every wave.",
                "\"Formatting Pixie\" Quentin" => "All friend bodies are 12% smaller. Extra copies add nothing.",
                "\"GLHF\" Nia" => "ON RECRUIT / ALTAR CREATION: gain one seeded random run bonus: +6% team damage, +6% team movement speed, +0.06 Luck, or +0.12 hero armor. Once per acquisition.",
                "\"Leisurely Walk\" Mary" => "Paid refreshes cost 2 less gold per talent, minimum 1 gold. Applies to both normal and dream-stock refreshes.",
                "\"Misfortune\" Emily" => "A recorded hero injury grants 3 gold. at most once every 2s; shares the rebate timer with TV Head. Multiple copies do not multiply the payout.",
                "\"Naming Demon\" Yan" => "Every third basic attack may fire a coloured echo for 25% weapon damage. Shares this friend's once every 0.7s. The echo cannot trigger itself.",
                "\"Overtime King\" Philip" => "Wave income +15 per talent. During combat, the hero spends 0.5 HP every 5s while at least one Philip talent is present; this health cost does not stack by copies and bypasses armor/dodge.",
                "\"The Creator\" Roger" => "ON RECRUIT / ALTAR CREATION: gain 100 gold immediately once. No recurring payment just for keeping this friend.",
                "\"The All-Knowing\" Walter" => "Every bond gains +1 supporting count while at least one Walter talent is owned. Additional Walters and star levels do not add more supporting counts.",
                "Evil" => "Cannot merge. Each 8 kills credited to Evil friends in total gives the hero 2% attack speed, up to 20% this wave. Shared across Evil talents.",
                "Original:TeacupCricket" => "Every fifth basic attack may release one echo for 40% of this friend's weapon damage. Uses the friend's once every 0.7s; no echo chain.",
                "Original:RibbonOtter" => "Collecting a heart attempts 1 extra HP of healing while at least one Ribbon Otter talent is owned. Extra healing does not stack by copies; hero healing restrictions apply.",
                "Original:SpindleWren" => "Lantern Guild friends gain 8% reach while at least one Spindle Wren talent is owned. Extra copies add nothing.",
                _ => string.IsNullOrEmpty(name) ? u.Id switch { "SecretPatchBear" => "A living resident rescued from the Lost-Property Vault. Permanent friend; its support and assigned weapon are listed below.", "SecretPaperBird" => "A folded courier rescued from the Lost-Property Vault. Permanent friend; remembers your letter and provides periodic healing.", "SecretDreamLamb" => "Garden visitor for two normal-wave victories. The dream friend then departs; its support ends with the contract.", _ => Authored(u) } : OctoberCompanions.Copy(name)
            };
        }
        public static string Passive(UnitDefinition u,int tier,float support=3)
        {
            float strength=(u.Passive==PassiveKind.Scavenge?Mathf.Round(u.PassiveStrength):u.PassiveStrength)*Mathf.Pow(support,Mathf.Clamp(tier,1,3)-1);
            return u.Passive switch {
                PassiveKind.HealChampion => "Every 4s in combat, attempt to heal the hero for "+(strength*3).ToString("0.###")+" HP before healing modifiers. Healing restrictions apply.",
                PassiveKind.Scavenge => "Every 4s in combat, gain "+Mathf.RoundToInt(strength)+" gold directly. This is not kill loot and does not count as collecting a coin.",
                PassiveKind.ChillAura => "Every 4s, chill enemies within "+(2+strength*.625f).ToString("0.#")+" steps: 1.5s normally, 0.6s on bosses.",
                PassiveKind.Rally => "Own weapon damage gains "+(strength).ToString("0.###")+"% per friend body in the roster.",
                _ => "No additional periodic personal passive."
            };
        }
        /// <summary>The talent in one short line for the shop card (the full rules stay in Talent, shown under INFO).</summary>
        /// <summary>A line about each new friend without an own skill (content expansion).</summary>
        public static readonly System.Collections.Generic.Dictionary<string,string> Flavour=new System.Collections.Generic.Dictionary<string,string>{
            ["PaperPlanePigeon"]="A pigeon who folds the post into paper planes and lets them chase the trouble.",
            ["BobaTadpole"]="A tadpole in a milk-tea cup who sips, then spits pearls in swaying waves.",
            ["BakerMouse"]="A floury mouse baker who rolls everything flat.",
            ["WateringSnail"]="A slow snail who waters the garden, and the enemies in it.",
            ["RaincoatFrog"]="A frog in a yellow raincoat with a plunger for every puddle.",
            ["LampGoat"]="A goat whose horns hold lanterns, swinging a lantern glaive.",
            ["YoyoSquirrel"]="A springy squirrel who never misses with a yo-yo.",
            ["ShovelPenguin"]="A penguin who heaves snow at anyone in the way.",
            ["TopBeetle"]="A clockwork beetle who spins tops into the crowd.",
            ["LensOwl"]="An owl scholar who focuses sunlight through a reading lens.",
            ["RainHeron"]="A heron who copies the rain into its notebook and takes any weapon offered.",
        };
        /// <summary>A talent written by hand, never a baked card (a check stores whole cards in Description).</summary>
        private static string Authored(UnitDefinition u)
        {
            if(Flavour.TryGetValue(u.Id,out string f))return f;
            if(CombatSimulation.SkillCopy.TryGetValue(u.Id,out string s))return s;
            string d=u.Description??""; return d.StartsWith("FRIEND /")?"":d;
        }
        public static string ShortTalent(UnitDefinition u)
        {
            if(string.IsNullOrEmpty(u.ReferenceCompanion)) { if(CombatSimulation.SkillCopy.ContainsKey(u.Id)) return ""; string t=Talent(u); return t.StartsWith("FRIEND /")?"":t; }
            string copy=OctoberCompanions.Copy(u.ReferenceCompanion);
            return string.IsNullOrEmpty(copy)?Talent(u):copy;
        }
        public static string ShortPassive(UnitDefinition u,int tier,float support=3)
        {
            float strength=(u.Passive==PassiveKind.Scavenge?Mathf.Round(u.PassiveStrength):u.PassiveStrength)*Mathf.Pow(support,Mathf.Clamp(tier,1,3)-1);
            return u.Passive switch {
                PassiveKind.HealChampion => "Every 4s heals the hero "+(strength*3).ToString("0.#")+" HP.",
                PassiveKind.Scavenge => "Every 4s finds "+Mathf.RoundToInt(strength)+" gold.",
                PassiveKind.ChillAura => "Every 4s chills enemies within "+(2+strength*.625f).ToString("0.#")+" units.",
                PassiveKind.Rally => "+"+strength.ToString("0.#")+"% damage per friend in the roster.",
                _ => ""
            };
        }
        public static string Describe(UnitDefinition u,int tier=1,float support=3)
        {
            var b=new StringBuilder();b.AppendLine("FRIEND · "+RunModifierRules.RarityName(u.Rarity)+" · "+new string('★',Mathf.Clamp(tier,1,3)));
            b.AppendLine("TALENT\n"+Talent(u));
            if(CombatSimulation.SkillCopy.TryGetValue(u.Id,out string own)&&own!=Talent(u))b.AppendLine("\nOWN SKILL\n"+own);
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
            RelicMechanic.CompoundInterest=>"At each wave start, multiply saved gold by 1.2 per copy, compounded. Separate from normal clear-wave interest.",
            RelicMechanic.RetortVolley=>"When hit, 30% chance to fire 8 radial shots for 12 damage per copy each. Shots move at 5 units/s and last 2s.",
            RelicMechanic.ExposedRecovery=>"Next wave: hero armor -0.8 per purchased copy. Listed recovery modifiers remain separate.",
            RelicMechanic.TreasureLease=>"Next wave: Treasure Detection +1 per purchased copy.",
            RelicMechanic.PrismEcho=>"A personal basic shot can strike one random enemy for max(2,2 times hero flat ranged bonus) damage per copy. at most once every 0.15s; no recursive response.",
            RelicMechanic.RerollFeather=>"Refreshes cost 2 less gold per active stack; minimum 1 gold.",
            RelicMechanic.ChestLightning=>"Opening a chest strikes 1-3 different enemies for max(8,100 times Treasure Detection) damage per copy.",
            RelicMechanic.DrinkVitality=>"ON EACH DRINK PURCHASE: permanently gain +4 maximum HP per active stack for this run. A drink carrying this effect counts itself when purchased.",
            RelicMechanic.DrinkTempo=>"ON EACH DRINK PURCHASE: permanently gain +6% team attack speed per active stack for this run. Drink shop weight also increases 5% per copy.",
            RelicMechanic.PanicMagnet=>"When hit, 5% chance to attract all pickups currently on the floor.",
            RelicMechanic.ChestLuck=>"Opening a chest permanently adds +0.06 Luck per active stack for this run.",
            RelicMechanic.ChaosIncome=>"At each wave start, gain a seeded random 25-75 gold per active stack.",
            RelicMechanic.HoardPower=>"Team damage bonus +1% per 5 gold held, per copy. Changes as collected combat gold grows your wallet.",
            RelicMechanic.HalfHealthSeed=>"At each battle start, current hero HP is reduced to at most 50% maximum HP; it does not reduce maximum HP.",
            RelicMechanic.StillRecovery=>"Standing still heals 3 HP/s per active stack, subject to hero healing restrictions.",
            RelicMechanic.CoinLightning=>"Each coin pickup strikes one enemy for max(4,50 times Luck) damage per active stack.",
            RelicMechanic.TurnDiscount=>"ON PURCHASE: other unsold gold-shop relics cost 30% less for the current shop. Also adds 30% discount to newly generated prices that shop, subject to the total discount cap.",
            RelicMechanic.CloseFollowers=>"Friend leash and outer patrol radius are multiplied by 0.65. Extra copies add nothing.",
            RelicMechanic.WaveGrowth=>"At every wave start, permanently gain +4 flat melee damage, +4 flat ranged damage and +10% team damage per copy for this run.",
            RelicMechanic.MeleeToCollision=>"Hero contact damage gains 50% of the positive hero flat melee bonus, per copy.",
            RelicMechanic.SlowToArmor=>"Hero armor gains the absolute value of negative hero movement-speed bonus, per copy.",
            RelicMechanic.DodgeMending=>"On a successful dodge, 30% chance to heal 10 HP per active stack.",
            RelicMechanic.DamageToIncome=>"Wave income gains 1 per full +4% positive hero damage bonus, per copy.",
            RelicMechanic.ChestOnlyRecovery=>"Disable passive regeneration and ordinary heart healing. Opening a chest attempts to heal 30 HP per copy. Other explicit healing effects still follow their own rules.",
            RelicMechanic.SorrowPulse=>"Every 3s, all eligible enemies take damage equal to 20% of hero maximum HP per copy.",
            RelicMechanic.BleedingAlarm=>"During combat, take 1 injected damage per copy each second. Armor and other injected-damage rules still apply; this differs from hunger's direct HP cost.",
            RelicMechanic.CollisionOnly=>"Hero basic attacks stop. Contact damage gains 10 times positive armor per copy. Independent triggered attacks may still fire.",
            RelicMechanic.SoloOverdrive=>"NEXT WAVE ONLY: friends stop their basic attacks and your hero attacks 6 times as often. Every 10 team kills attempts to heal 3 HP.",
            RelicMechanic.FreshLotus=>"Start each wave with +3 pierce and +30% crit per copy. The temporary bonus ends on the first recorded hero hit.",
            RelicMechanic.EggGift=>"ON PURCHASE: receive 3 eggs immediately once per acquisition.",
            RelicMechanic.CompanionGift=>"ON PURCHASE: try to recruit up to 2 ordinary friends from your hero's pool. Gifts use the 3-friend regular purchase quota. At most 1 new species per wave; from wave 10 both shops share one new species every 2 waves. Checks unlock wave and unique weapon; no body cap. Solo heroes receive no bodies.",
            RelicMechanic.FragilePower=>"Each recorded hero hit removes 5% team damage per copy, including a saved run penalty. Its positive listed modifiers are separate.",
            RelicMechanic.StatFloor=>"ON PURCHASE: record current modifier bonuses as floors; future bonuses cannot fall below those recorded floors. Does not remove every direct combat restriction or HP cost.",
            RelicMechanic.LuckToHealth=>"Maximum HP gains 100 times positive Luck per copy.",
            RelicMechanic.GambleRecovery=>"Every 3s make two independent rolls: 50% to heal 20 HP per copy; 50% to take 8 injected damage per copy. Both or neither can occur.",
            RelicMechanic.FoodGrowth=>"ON EACH FOOD PURCHASE: permanently gain +10 max HP, +0.5 HP/s and +5% hero size per active stack for this run.",
            RelicMechanic.FreeFollowers=>"Friend outer patrol radius becomes half the arena's diagonal. Does not add extra friend bodies.",
            RelicMechanic.IncomePoison=>"Hero basic hits poison the target for 4s. Each 1s tick deals max(1,base wave income plus income bonus) damage. Refreshes duration, not an unbounded stack of poisons.",
            RelicMechanic.HurtGrowth=>"Each time you are hit, gain one small bonus for the run per copy: +4 max HP, a little HP regen, a little armor, +2% damage, a little Luck or +1% crit.",
            RelicMechanic.StillPower=>"During each battle, +16 flat attack damage per copy until you move for the first time. Stopping later does not restore this bonus.",
            RelicMechanic.StaticFollowers=>"Friends stop moving; their range and projectile lifetime double and pierce increases by 3.",
            RelicMechanic.VoidPulse=>"Every 3s, all eligible enemies lose damage equal to 15% of their CURRENT HP per copy. Not an instant execution.",
            RelicMechanic.SixWeapons=>"ON FIRST PURCHASE: remember 6 random weapons. Replace ordinary team basic attacks with six phantom weapons fired from the hero. Each uses its own weapon stats.",
            RelicMechanic.RoyalBlow=>"Every 8th hit of the hero is a royal blow: triple damage.",
            RelicMechanic.QuiltGuard=>"Once per wave, a blow that would defeat the hero leaves 1 HP and 1.5s of safety instead.",
            RelicMechanic.ApronShove=>"Enemies that strike the hero up close are shoved back 1.5 units (once per second).",
            RelicMechanic.QuietMending=>"After 3s without being hit, the hero's healing per second is tripled.",
            RelicMechanic.EchoForward=>"A friend's basic attack that defeats an enemy echoes onward: the nearest enemy takes half that weapon's damage (once every 0.2s).",
            RelicMechanic.BlowerReturn=>"A hostile shot a friend intercepts is blown back as a friendly shot with double damage.",
            RelicMechanic.InvoiceSettles=>"Every relic you own settles part of the invoice: the damage penalty shrinks by 2% each, to nothing at 10 relics.",
            RelicMechanic.SealBurst=>"A collision stamps a crimson seal; 1s later it bursts for triple the collision damage around the enemy.",
            RelicMechanic.SpringDouble=>"Every 4s each friend's next attack springs twice.",
            RelicMechanic.BorrowedCrown=>"Your highest-star friend also fires your hero's weapon.",
            RelicMechanic.ClockworkHeart=>"Every merge winds the heart: the team gains +3% attack speed for the run (max +30%).",
            RelicMechanic.ImpossiblePrism=>"Split shots of friends break into three colours: ember, frost and arcane.",
            RelicMechanic.LivingThread=>"Melee friends are stitched together: a melee friend's blow pulls a strike (50%) from the nearest other melee friend.",
            RelicMechanic.TallyChalk=>"Every 25 enemies defeated this wave: the team deals +1% damage (max +8%).",
            RelicMechanic.CoinSash=>"While the hero runs, pickup reach +0.5.",
            RelicMechanic.PostageStamp=>"Marked enemies (stamped, pinned, exposed or anchored) take +10% damage.",
            RelicMechanic.FrostSalt=>"Freezes last 0.3s longer.",
            RelicMechanic.RefundSlip=>"Once per shop, the last relic you bought can be returned at the refund counter for the full price paid.",
            RelicMechanic.WhetstoneWafer=>"Melee hits cleave one more enemy beside the target for 40% of the hit.",
            RelicMechanic.LateFee=>"Burning, poison and deferred damage hit 25% harder.",
            RelicMechanic.HagglersAbacus=>"Each refresh in this shop takes 1 gold off every gold offer (at most 30% off).",
            RelicMechanic.SlipstreamScarf=>"Friends gain +1% attack speed for every 5% movement speed bonus of the hero (max +15%).",
            RelicMechanic.BrainFreeze=>"A critical hit on a frozen enemy breaks off 3 ice shards (30% of the hit each).",
            RelicMechanic.WindChime=>"Every 10 critical hits of the team ring the chime: the next 3 shots of every friend pierce one more enemy.",
            RelicMechanic.SpecimenPin=>"Every enemy species defeated in this run gives +1% critical chance (max +20%).",
            RelicMechanic.StarPolish=>"Three-star friends with shooting weapons fire one more projectile.",
            RelicMechanic.KiteTailRibbon=>"Critical shots of ranged friends bounce on to one more enemy.",
            RelicMechanic.MeltingSundae=>"When the hero is hit, enemies within 2 units freeze for 0.5s (once every 4s).",
            RelicMechanic.CuckooPendulum=>"Every 12s the cuckoo calls: the next attack of every friend is a critical hit.",
            RelicMechanic.DivingBellLantern=>"Marked enemies (stamped, pinned, exposed or anchored) take +15% damage.",
            RelicMechanic.TwinTicket=>"A merge into a three-star friend brings a free one-star copy of it.",
            RelicMechanic.TreasureMimic=>"The first chest opened each wave wakes up and bites the nearest enemy every 0.5s for 10s.",
            RelicMechanic.DuelistsRibbon=>"Every 4th hit of each melee friend strikes twice.",
            RelicMechanic.FrostLedger=>"Every frozen enemy defeated this wave gives the team +2% damage for the wave (max +20%).",
            RelicMechanic.UndertowAnchor=>"Every 8s an undertow from the front line pushes the 5 enemies nearest the hero back 2 units and soaks them.",
            RelicMechanic.FunhouseMirror=>"Every 10s one friend's weapon is warped for 3s: double size and damage, half the speed.",
            RelicMechanic.SpliceKit=>"Once per wave, three different one-star friends are spliced into one random two-star friend.",
            RelicMechanic.EndlessEncore=>"No 3s rest after a transformation: the gauge starts filling again at once.",
            RelicMechanic.MoonClockKey=>"Every active bond counts one more member.",
            RelicMechanic.MirrorOfTwins=>"Your strongest friend deals +60% damage and its shooting weapon fires one more projectile.",
            RelicMechanic.StorybookEnding=>"Every wave won adds a page to the story: the team gains one random stat for the rest of the run.",
            RelicMechanic.DreamLoom=>"Common items give double their stats.",
            _=>throw new ArgumentOutOfRangeException(nameof(m),m,"Missing relic mechanic copy")
        };
        /// <summary>One short line per special effect for the shop card (the full rules stay in Mechanic, shown under INFO).</summary>
        public static string ShortMechanic(RelicMechanic m)=>m switch {
            RelicMechanic.CrowdedTape=>"Next wave: +10% more enemies.",
            RelicMechanic.CoinMending=>"Coins have a 15% chance to heal 2 HP.",
            RelicMechanic.CompoundInterest=>"Each wave start: saved gold x1.2.",
            RelicMechanic.RetortVolley=>"When hit: 30% chance of a ring of 8 shots.",
            RelicMechanic.ExposedRecovery=>"Next wave: hero armor -0.8.",
            RelicMechanic.TreasureLease=>"Next wave: +1 Treasure Detection.",
            RelicMechanic.PrismEcho=>"Your shots echo onto a random enemy.",
            RelicMechanic.RerollFeather=>"Refreshes cost 2 gold less.",
            RelicMechanic.ChestLightning=>"Opening a chest strikes 1-3 enemies.",
            RelicMechanic.DrinkVitality=>"Each drink bought: +4 max HP for the run.",
            RelicMechanic.DrinkTempo=>"Each drink bought: +6% team attack speed for the run.",
            RelicMechanic.PanicMagnet=>"When hit: 5% chance to pull in every pickup.",
            RelicMechanic.ChestLuck=>"Each chest opened: +0.06 Luck for the run.",
            RelicMechanic.ChaosIncome=>"Each wave start: 25-75 random gold.",
            RelicMechanic.HoardPower=>"+1% team damage per 5 gold you hold.",
            RelicMechanic.HalfHealthSeed=>"Each fight starts with the hero at half HP at most.",
            RelicMechanic.StillRecovery=>"Standing still heals 3 HP per second.",
            RelicMechanic.CoinLightning=>"Each coin picked up strikes an enemy.",
            RelicMechanic.TurnDiscount=>"On buy: the other relics in this shop cost 30% less.",
            RelicMechanic.CloseFollowers=>"Friends stay closer to the hero.",
            RelicMechanic.WaveGrowth=>"Each wave start: +4 melee, +4 ranged and +10% team damage for the run.",
            RelicMechanic.MeleeToCollision=>"Hero contact damage gains half your flat melee bonus.",
            RelicMechanic.SlowToArmor=>"Hero armor grows with your hero speed penalty.",
            RelicMechanic.DodgeMending=>"On a dodge: 30% chance to heal 10 HP.",
            RelicMechanic.DamageToIncome=>"+1 wave income per +4% hero damage.",
            RelicMechanic.ChestOnlyRecovery=>"No regeneration or hearts; each chest heals 30 HP.",
            RelicMechanic.SorrowPulse=>"Every 3s all enemies take 20% of the hero's max HP.",
            RelicMechanic.BleedingAlarm=>"In battle the hero loses 1 HP each second.",
            RelicMechanic.CollisionOnly=>"The hero stops attacking; contact damage +10 per armor.",
            RelicMechanic.SoloOverdrive=>"Next wave: friends rest, the hero attacks 6 times faster.",
            RelicMechanic.FreshLotus=>"Each wave starts with +3 pierce and +30% crit until the hero is hit.",
            RelicMechanic.EggGift=>"On buy: 3 eggs.",
            RelicMechanic.CompanionGift=>"On buy: up to 2 friends from your hero's pool join.",
            RelicMechanic.FragilePower=>"Each hit on the hero: -5% team damage for the run.",
            RelicMechanic.StatFloor=>"On buy: your current bonuses can no longer drop.",
            RelicMechanic.LuckToHealth=>"+100 max HP per point of Luck.",
            RelicMechanic.GambleRecovery=>"Every 3s: 50% heal 20 HP, 50% take 8 damage.",
            RelicMechanic.FoodGrowth=>"Each food bought: +10 max HP, +0.5 HP/s, +5% size.",
            RelicMechanic.FreeFollowers=>"Friends roam the whole arena.",
            RelicMechanic.IncomePoison=>"Hero hits poison for 4s, ticking for your wave income.",
            RelicMechanic.HurtGrowth=>"Each hit on the hero: a small random bonus for the run.",
            RelicMechanic.StillPower=>"+16 attack damage until the hero first moves in a fight.",
            RelicMechanic.StaticFollowers=>"Friends stand still with double range and +3 pierce.",
            RelicMechanic.VoidPulse=>"Every 3s all enemies lose 15% of their current HP.",
            RelicMechanic.SixWeapons=>"Six phantom weapons fire from the hero instead of the team's attacks.",
            _=>Mechanic(m)
        };
        public static string ShortSignature(SignatureRelicKind role)=>role switch {
            SignatureRelicKind.HarvestReceipt=>"With a bell weapon nearby, coins quicken its beat.",
            SignatureRelicKind.ReflectiveInk=>"Stamps add 2 marks; marks last 65% as long.",
            SignatureRelicKind.CarbonRibbon=>"Stamp bursts pull in coins; bell pulses expose.",
            SignatureRelicKind.OfficeLens=>"Crits on exposed enemies hit 25-50% harder.",
            SignatureRelicKind.TeaCoaster=>"Tea trails 40% wider, 20% weaker.",
            SignatureRelicKind.ScratchRecord=>"Music-box hits echo; bells hit marked enemies harder.",
            SignatureRelicKind.TreasureRibbon=>"Chests heal 3 HP and power up bow hits for 4s.",
            SignatureRelicKind.PatchSpring=>"Arena toys charge themselves every 3s.",
            SignatureRelicKind.ThermalInk=>"Hot trails strike marked enemies.",
            SignatureRelicKind.RepairCoupon=>"Arena-toy kills heal 1 HP.",
            _=>Signature(role)
        };
        public static string Signature(SignatureRelicKind role)=>role switch {
            SignatureRelicKind.None=>"", SignatureRelicKind.HarvestReceipt=>"Requires a bell signature weapon within 6 units. Collecting a coin advances its bell beat, up to 2 steps; at most once every 0.35s.",
            SignatureRelicKind.ReflectiveInk=>"Stamp hits add 2 marks instead of 1, but mark lifetime is 65% of normal. Stamp exposure and marked-target synergies remain active.",
            SignatureRelicKind.CarbonRibbon=>"A stamp burst attracts coins within 2.5 units of its target. Bell pulses expose surviving targets for 1.5s. Extra copies add nothing.",
            SignatureRelicKind.OfficeLens=>"Critical hits on an exposed enemy gain a separate damage multiplier: 1.25 + min(0.25,0.05 times stacks). Requires exposure and a critical hit.",
            SignatureRelicKind.TeaCoaster=>"Tea projectile trails have 40% more radius but deal 20% less trail damage. Extra copies add nothing.",
            SignatureRelicKind.ScratchRecord=>"Music-box hits add a delayed echo after 0.3s, damage min(70%,35% times stacks). Bell pulses also add up to 50% extra damage against marked survivors.",
            SignatureRelicKind.TreasureRibbon=>"Opening a chest heals 3 HP per copy and empowers bow-motion hits for 4s: up to +45% damage, +15% per copy.",
            SignatureRelicKind.PatchSpring=>"Every 3s, automatically add one charge to each charging arena toy. Extra copies add nothing.",
            SignatureRelicKind.ThermalInk=>"A hot trail touching a marked enemy adds a 40%-trail-damage strike, with a once every 0.9s.",
            SignatureRelicKind.RepairCoupon=>"An arena-toy credited kill heals 1 HP per copy, at most once every 0.6s. Ordinary basic kills do not activate this effect.",
            _=>throw new ArgumentOutOfRangeException(nameof(role),role,"Missing signature copy")
        };
        public static string Describe(ItemDefinition item,int stacks=1,RunCatalog catalog=null)
        {
            var b=new StringBuilder();b.AppendLine(Category(item)+" · "+RunModifierRules.RarityName(item.Rarity)+" · "+(item.Stacking==StackPolicy.Unique?"ONE ONLY":"STACKS"));
            if(item.Category==RelicCategory.Drink)b.AppendLine("Counts as a drink when bought.");
            if(item.Category==RelicCategory.Food)b.AppendLine("Counts as a food when bought.");
            int copies=item.EffectiveStacks(stacks);
            bool any=false;foreach(var m in item.EffectiveModifiers)if(m.Value!=0){if(!any)b.AppendLine("\nWHILE YOU HAVE IT"+(copies>1?" (×"+copies+")":""));b.AppendLine(RunModifierRules.Line(m.Effect,m.Scope,m.Value*copies));any=true;}
            string mechanic=Mechanic(item.Mechanic),signature=Signature(item.SignatureRole);
            if(mechanic.Length>0)b.AppendLine("\nSPECIAL EFFECT\n"+mechanic);
            if(signature.Length>0)b.AppendLine("\nWEAPON SYNERGY\n"+signature);
            RunCatalog c=catalog!=null?catalog:Resources.Load<RunCatalog>("RunCatalog");
            if(c!=null)foreach(var p in c.ProcRules??Array.Empty<ProcRule>())if(p.ItemId==item.Id)b.AppendLine("\n"+(p.Trigger==ProcTrigger.CoinCollected?"When you pick up a coin":p.Trigger==ProcTrigger.ChestOpened?"When you open a chest":"When an enemy falls")+": "+(p.Chance<1?(p.Chance*100).ToString("0")+"% chance to ":"")+(p.Action==ProcAction.HealChampion?"heal you "+p.Power.ToString("0.#")+" HP":"hit enemies nearby for "+p.Power.ToString("0.#"))+(copies>1?" (×"+copies+")":"")+" (at most once every "+p.Cooldown.ToString("0.#")+"s).");
            if(item.StackLimit<100000)b.AppendLine("\nYou can own up to "+item.StackLimit+".");
            return b.ToString().Trim();
        }
        public static string PurchaseReactions(RunSession s,RunCatalog c,ItemDefinition item)
        {
            var b=new StringBuilder();
            int Talents(string name){int n=0;foreach(var r in s.State.Roster)if(c.Unit(r.UnitId).ReferenceCompanion==name)n++;foreach(var id in s.State.AbsorbedSkills)if(c.Unit(id).ReferenceCompanion==name)n++;return n;}
            int Stacks(RelicMechanic m)=>s.MechanicStacks(m)+(item.Mechanic==m?1:0);
            if(item.Category==RelicCategory.Drink)
            {
                int alien=Talents("Alien");if(alien>0)b.AppendLine("Your "+alien+" drink-reactive friend/memory talent(s): each relevant friend gets +4% weapon damage, cap +40%, for the next battle.");
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
