using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a hero becomes while its gauge is spent (one per hero).</summary>
    public enum KitAction
    {
        Gatling, Engine, Dreams, Lighthouse, Treant, Mitosis, Casino, HighTide, Meteors, Mirrors, AcornGun, Spring, Kintsugi, Rink,
        Parade, Duel, Feast, Throne, Train, Coronation, Citadel, Showtime, Exhibit, BullMarket, Thunder, Symphony, Eclipse, Kitchen,
        Tapestry, Foreclosure, Monsoon,
        // the five expansion heroes (Docs/Content_Expansion_v5.md)
        KiteFleet, SundaeRain, TimeGarden, Circus, DivingBell
    }

    /// <summary>Everything a hero's passives can touch: team stats, the shop and economy, bonds, merging, conditions in
    /// battle, enemies, gold, and the hero itself. Companions are immortal, so team passives are never survival stats.</summary>
    public enum KitPassive
    {
        Innate,
        // team stats (folded into the run's bonuses before battle)
        TeamDamage, TeamAttackSpeed, TeamCrit, TeamMoveSpeed, TeamProjectileSpeed, TeamPierce, TeamRange, PickupRadius, InterestCap, Income,
        // shop and economy
        FirstPurchaseDiscount, SecondPurchaseDiscount, FreeFirstReroll, RerollLuck, RerollCheaperWithGold, DrinkDiscount, FoodDiscount,
        ExtraShopSlot, RoyalPick, CheapestDiscountWithGold, ForwardOrderDiscount, InterestOnGold, EggEveryWaves, Dividends, BossGold, WaveEndDice,
        NewRecruitPlusOne, BuyTwoStarChance, DuplicateCopyChance, MergeGold, MergeHeroDamage, AbsorbGold, DuplicateRelicBonus, FoodPotency,
        // bonds
        BondBiggestPlusOne, BondDualPlusOne, BondLeadDouble, BondPlusOneSprout, BondPlusOneRegal, BondLevelAttackSpeed,
        /// <summary>Version 4: the hero's signature minor bond (its third RecruitTheme) counts one level higher.</summary>
        SignatureBondLevel,
        /// <summary>Version 5 own passives (Docs/Content_Expansion_v5.md §3 and §8).</summary>
        RootGrip, DuplicateRelicGold, ThornCoin, SlowToFreeze, TeamRearCrit, DrinkIncome, HurtCollision, PinnedPierce, RareBuyDamage,
        TeamAttackSpeedPerRelic,
        /// <summary>The five expansion heroes (Docs/Content_Expansion_v5.md §2–§5 and §8).</summary>
        FarShotDamage, RangedTeamRange, RangedPierceWhileFull, FoodHealth, FrozenKillGold, FrozenCritShatter, KeepHalfTier,
        ThemeBondAttackSpeed, CritBall, BigTopTwoStar, ProbedCoin, TeamDamageVsProbed, ProbedBounce,
        // team in battle (conditional)
        TeamDamageVsMarked, TeamDamageVsBounty, TeamDamageVsSlowed, TeamDamageVsLowHealth, TeamDamageVsRear, TeamDamageVsPinned, TeamDamageVsRecorded,
        TeamDamageVsShocked, TeamDamageVsWet, TeamDamageNearHero, TeamDamageWhileHeroHealthy, TeamDamagePerGold, TeamAttackSpeedPerGold, TeamDamagePerFood,
        TeamDamagePerPetal, TeamAttackSpeedOnHeroHurt, TeamDamageOnHeroHurt, TeamCritOnHeroDodge, TeamAttackSpeedWaveStart, TeamAttackSpeedPerKill,
        TeamAttackSpeedWhileDebt, TeamAttackSpeedNearHero, TeamDamageBehindHero, TeamLeadDamage, LeadStarAttackSpeed, TeamFirstHitBonus, TeamChainChance,
        SproutDamage, TeamCritVsRecorded, TeamAttackSpeedFullGarden, TeamMeleeAttackSpeed, EnemyVulnerableNearHero,
        // enemies
        EnemySlowNearHero, EnemySlowOnHeroHit, EnemySlowPinned, EnemySlowWithDebt, EnemySlowOnRear, EnemyShotSlow, EnemyCrackOnHit,
        // gold in battle
        CoinOnMarkedKill, CoinOnRearHit, KillCoinsNearHero, DebtKillGold, GoldOnHeroHurt, ChestBonusGold, EliteEgg, SpeciesGold,
        // the hero
        HeroDamageVsMarked, HeroDamageVsRecorded, Stamps, LidCatch, AttackSpeedWhenHurt, StillNextAttack, ArmorNearAllies, RootCatch, HealWhileStill,
        HeroHealPerHits, HeroArmor, DiceFaceBuff, DrinkExtendsAwaken, SprintSpeed, MirrorCatch, BountyGold, CrackDamage, TurnBubbles, DamageWithAllies,
        PoiseDodge, Thorns, HealOnLeadAct, ChainOnHit, RearDamage, Pins, TimeDebt, CritBeats
    }

    public readonly struct KitTrait
    {
        public readonly KitPassive Kind; public readonly float Value; public readonly string Text; public readonly bool Hero;
        public KitTrait(KitPassive kind, float value, string text, bool hero) { Kind = kind; Value = value; Text = text; Hero = hero; }
    }

    /// <summary>One hero's whole kit (18/10/2026): one mechanic (its gauge fills from what the hero does; full, it transforms by
    /// itself for a few seconds), one or two hero passives and two to four passives for the team, the shop or the run.</summary>
    public sealed class HeroKit
    {
        public string Id, Transformation, Fill, Effect;
        public KitAction Action;
        public float Seconds = 6, HeroAttackSpeed, HeroMoveSpeed, HeroDamageTaken = 1, HeroCrit, TeamAttackSpeed;
        /// <summary>True when the transformation is a gun: its shots flash at the barrel. Everything else has no muzzle.</summary>
        public bool Gun;
        /// <summary>True when the transformed piece is a held weapon (art &lt;Id&gt;_Awake_Held); false for a worn or summoned
        /// object (art &lt;Id&gt;_Awake).</summary>
        public bool Held;
        /// <summary>True when the transformation replaces the hero's own attacks.</summary>
        public bool ReplacesAttacks;
        public KitTrait[] Traits = Array.Empty<KitTrait>();
        public IEnumerable<KitTrait> HeroTraits { get { foreach (var t in Traits) if (t.Hero) yield return t; } }
        public IEnumerable<KitTrait> TeamTraits { get { foreach (var t in Traits) if (!t.Hero) yield return t; } }
        public float Value(KitPassive kind) { float v = 0; foreach (var t in Traits) if (t.Kind == kind) v += t.Value; return v; }
        public bool Has(KitPassive kind) { foreach (var t in Traits) if (t.Kind == kind) return true; return false; }
        public string ArtKey => Id == "CopperCauldron" ? "CopperCauldron_Engine" : Id + "_Awake";
        /// <summary>The same kit with other passives (the Wardrobe's second habit).</summary>
        public HeroKit WithTraits(KitTrait[] traits) { var copy = (HeroKit)MemberwiseClone(); copy.Traits = traits; return copy; }
    }

    public static class HeroKits
    {
        private static KitTrait H(KitPassive k, float v, string text) => new KitTrait(k, v, text, true);
        private static KitTrait T(KitPassive k, float v, string text) => new KitTrait(k, v, text, false);
        private static HeroKit K(string id, string name, KitAction action, string fill, string effect, params KitTrait[] traits)
            => new HeroKit { Id = id, Transformation = name, Action = action, Fill = fill, Effect = effect, Traits = traits };

        public static readonly HeroKit[] All =
        {
            With(K("Postmaster", "SORTING GATLING", KitAction.Gatling, "your attacks", "A six-barrel sorter replaces your mailbox: a homing parcel every 0.2s, round the stamped addresses first; attack speed +40%.",
                H(KitPassive.Stamps, 3, "Basic hits stamp up to 3 enemies for 6s; stamped enemies take +10% damage from you."),
                T(KitPassive.FirstPurchaseDiscount, .2f, "Registered post: the first thing you buy in each shop costs 20% less."),
                T(KitPassive.TeamDamageVsMarked, .08f, "Friends deal +8% damage to stamped enemies."),
                T(KitPassive.TeamProjectileSpeed, .1f, "Friends' shots fly 10% faster."),
                T(KitPassive.CoinOnMarkedKill, .15f, "A stamped enemy's fall has a 15% chance to drop an extra coin.")),
                k => { k.Gun = true; k.Held = true; k.ReplacesAttacks = true; k.HeroAttackSpeed = .4f; }),
            With(K("CopperCauldron", "STEAM ENGINE", KitAction.Engine, "enemies near you and blows you take", "You run as a locomotive: +60% move speed, +30% attack speed, 25% less damage taken; you ram and shove what is in front; pressure builds and you finish with one long steam jet.",
                H(KitPassive.LidCatch, 4, "Lid: stand still for 0.5s and the lid catches one hostile shot (every 4s)."),
                H(KitPassive.AttackSpeedWhenHurt, .1f, "A blow you take gives you +10% attack speed for 2s."),
                T(KitPassive.FreeFirstReroll, 1, "Steam clears the shelves: the first reroll in each shop is free."),
                T(KitPassive.HurtCollision, 2, "Each blow you take this wave gives friends +2 collision damage (max +10)."),
                T(KitPassive.TeamAttackSpeedOnHeroHurt, .15f, "When you are hit, friends near you get +15% attack speed for 2s.")),
                k => { k.HeroMoveSpeed = .6f; k.HeroAttackSpeed = .3f; k.HeroDamageTaken = .75f; }),
            With(K("DrowsySeer", "FULL MOON", KitAction.Dreams, "walking and your attacks", "Three dreams of you walk your route a second, two and three seconds behind, casting at enemies near them and firing your attack at 35% damage (below 40% HP you swap places with the farthest, every 8s); +30% move speed and +40% attack speed.",
                H(KitPassive.StillNextAttack, .3f, "Stand still for 0.5s and your next attack deals +30%."),
                H(KitPassive.HeroArmor, .3f, "Dream veil: you take 30% less damage."),
                T(KitPassive.EggEveryWaves, 3, "Dream eggs: every third wave you get an extra egg."),
                T(KitPassive.TeamAttackSpeedWaveStart, .15f, "Waking up: friends get +15% attack speed for the first 5s of each wave."),
                T(KitPassive.ExtraShopSlot, 1, "Foresight: every shop shows one more offer."),
                T(KitPassive.EnemySlowOnHeroHit, .1f, "Your hits leave enemies drowsy: slowed 10% for 1s.")),
                k => { k.Held = true; k.HeroMoveSpeed = .3f; k.HeroAttackSpeed = .4f; }),
            With(K("LanternKnight", "LIGHTHOUSE", KitAction.Lighthouse, "friends' attacks near you", "Your lamp becomes a lighthouse: a beam sweeps the field striking what it crosses; friends in the light get +50% attack speed; you gain 20% damage reduction.",
                H(KitPassive.ArmorNearAllies, .1f, "With at least 2 friends near you, you take 10% less damage."),
                T(KitPassive.SignatureBondLevel, 1, "Lantern Guild counts one level higher."),
                T(KitPassive.TeamAttackSpeedNearHero, .08f, "Friends within 4 units of you get +8% attack speed."),
                T(KitPassive.TeamRange, .05f, "Friends' attack range +5%."),
                T(KitPassive.EnemyVulnerableNearHero, .05f, "Enemies in your lamplight (within 4 units) take +5% damage.")),
                k => { k.Held = true; k.HeroDamageTaken = .8f; k.TeamAttackSpeed = .5f; }),
            With(K("RootOracle", "LIVING TREE", KitAction.Treant, "time spent among enemies", "You uproot and walk on your roots: every step raises thorns, your roots strike twice as often, and you gain 50% more maximum health while it lasts.",
                H(KitPassive.RootCatch, 4, "Your roots catch one hostile shot near you every 4s."),
                H(KitPassive.HealWhileStill, 3, "Standing among your roots heals 1 HP every 3s."),
                T(KitPassive.InterestCap, 2, "Deep roots: interest cap +2."),
                T(KitPassive.EnemySlowNearHero, .15f, "Enemies near your roots are slowed 15%."),
                T(KitPassive.TeamDamageNearHero, .08f, "Friends deal +8% damage to enemies near your roots."),
                T(KitPassive.RootGrip, 5, "Every 5s the enemy nearest to you is held by roots for 1s.")),
                k => { k.HeroDamageTaken = .7f; }),
            With(K("LoneJelly", "MITOSIS", KitAction.Mitosis, "your attacks", "You split into three: two jellies carrying your remembered organs fight beside you; +30% attack speed.",
                H(KitPassive.HeroHealPerHits, 5, "Remembered organs: every 5 hits heal 1 HP; your hits slow enemies 10%."),
                H(KitPassive.HeroArmor, .15f, "Remembered shells: you take 15% less damage."),
                T(KitPassive.MergeHeroDamage, .02f, "Each merge in this run gives you +2% damage for good (max 25%)."),
                T(KitPassive.AbsorbGold, 2, "Absorbing a memory refunds 2 gold."),
                T(KitPassive.EnemySlowOnHeroHit, .1f, "Your hits slow enemies 10% for 1s.")),
                k => { k.HeroAttackSpeed = .3f; }),
            With(K("GlassDice", "CASINO TABLE", KitAction.Casino, "gold you collect", "Three dice roll every second: a pair pays a strike, three of a kind a jackpot; +25% crit chance.",
                H(KitPassive.DiceFaceBuff, 1, "The next die face is a small buff: damage, attack speed, crit, speed, gold or healing."),
                T(KitPassive.RerollLuck, 1, "Each reroll has a 1 in 6 chance to be free and a 1 in 6 chance to roll rarer goods."),
                T(KitPassive.TeamCrit, .05f, "Friends +5% crit chance."),
                T(KitPassive.WaveEndDice, 6, "At the end of each wave a die is cast: a six pays 6 gold.")),
                k => { k.Held = false; k.HeroCrit = .25f; }),
            With(K("TideSinger", "HIGH TIDE", KitAction.HighTide, "coins you collect", "You ride a wave: +40% move speed, the wave shoves enemies it meets and you heal 2 HP every second.",
                H(KitPassive.DrinkExtendsAwaken, .5f, "Every 2 drinks bought add 1s to the tide (max +3s)."),
                H(KitPassive.HeroArmor, .2f, "Sea foam: you take 20% less damage."),
                T(KitPassive.DrinkDiscount, .25f, "Drinks cost 25% less."),
                T(KitPassive.DrinkIncome, 5, "Every drink bought adds 1 gold to every later wave's income (max +5)."),
                T(KitPassive.TeamDamage, .05f, "Friends +5% damage.")),
                k => { k.HeroMoveSpeed = .4f; }),
            With(K("CometRunner", "METEOR SHOWER", KitAction.Meteors, "distance travelled", "Meteors rain down along the route you run; +50% move speed and +25% attack speed.",
                H(KitPassive.SprintSpeed, .25f, "Run for 2s without stopping and you gain +25% move speed."),
                T(KitPassive.PickupRadius, 1f, "Coins fly to you from 30% further."),
                T(KitPassive.TeamMoveSpeed, .08f, "Friends +8% move speed."),
                T(KitPassive.EnemySlowNearHero, .1f, "Enemies you rush past are slowed 10%.")),
                k => { k.HeroMoveSpeed = .5f; k.HeroAttackSpeed = .25f; }),
            With(K("MirrorKeeper", "HALL OF MIRRORS", KitAction.Mirrors, "hostile shots that pass close", "Four mirrors circle you and turn every hostile shot that reaches them back at the dream; +25% attack speed.",
                H(KitPassive.MirrorCatch, 3, "The mirror catches one hostile shot every 3s and returns it with your next attack."),
                T(KitPassive.DuplicateCopyChance, .1f, "Reflection: buying a friend you already have has a 10% chance to bring a second copy."),
                T(KitPassive.TeamPierce, 1, "Friends' shots pierce one more enemy."),
                T(KitPassive.TeamFirstHitBonus, .15f, "A friend's first hit on each enemy deals +15%."),
                T(KitPassive.EnemyShotSlow, .1f, "Hostile shots fly 10% slower.")),
                k => { k.HeroAttackSpeed = .25f; }),
            With(K("TreasureAcorn", "ACORN MACHINE GUN", KitAction.AcornGun, "hits on the bounty", "A crank-driven acorn gun fires only at the bounty; every acorn that hits it shakes coins loose for the team.",
                H(KitPassive.BountyGold, 8, "A bounty is always posted; felling it pays 1 gold (max 8 per wave)."),
                T(KitPassive.ChestBonusGold, 3, "The first chest of each wave holds 3 extra gold."),
                T(KitPassive.TeamDamageVsBounty, .1f, "Friends deal +10% damage to the bounty."),
                T(KitPassive.CheapestDiscountWithGold, 1, "Holding at least 20 gold, the cheapest offer costs 1 less."),
                T(KitPassive.Income, 1, "Gold income +1 each wave.")),
                k => { k.Gun = true; k.Held = true; k.ReplacesAttacks = true; k.HeroAttackSpeed = .3f; }),
            With(K("VerdantMatron", "SPRING FESTIVAL", KitAction.Spring, "time in battle", "Every sprout blooms at once: thorns burst from around your sprouts and you, and you heal while it lasts.",
                H(KitPassive.Innate, 0, "The nursery: sprouts still grow into a chosen kind of friend."),
                T(KitPassive.BuyTwoStarChance, .15f, "Greenhouse: a one-star friend you buy has a 15% chance to arrive at two stars."),
                T(KitPassive.SproutDamage, .1f, "Sprouts deal +10% damage."),
                T(KitPassive.TeamAttackSpeed, .05f, "Friends +5% attack speed."),
                T(KitPassive.SignatureBondLevel, 1, "Jade Nursery counts one level higher.")),
                k => { k.Held = true; }),
            With(K("ChinaMarionette", "KINTSUGI", KitAction.Kintsugi, "injuries and healing", "Your cracks fill with gold: you take no damage for 3s, +40% attack speed, and your strikes leave gold seams that burst.",
                H(KitPassive.CrackDamage, .05f, "Each crack (up to 3, from injuries) gives you +5% damage."),
                T(KitPassive.GoldOnHeroHurt, 5, "Mending money: each injury pays 1 gold (max 5 per wave)."),
                T(KitPassive.TeamDamageOnHeroHurt, .1f, "When you are hurt, friends near you deal +10% damage for 3s."),
                T(KitPassive.MergeGold, 2, "Every merge pays 2 gold (grows by wave)."),
                T(KitPassive.EnemyCrackOnHit, .05f, "An enemy that strikes you cracks: it takes +5% damage for 4s.")),
                k => { k.Held = true; k.HeroAttackSpeed = .4f; }),
            With(K("SoapSkater", "ICE RINK", KitAction.Rink, "sharp turns", "The floor turns to ice: enemies slide and slow, you drift; +40% move speed and +25% attack speed.",
                H(KitPassive.TurnBubbles, .5f, "Sharp turns leave bubbles that slow and gently push enemies."),
                T(KitPassive.TeamDamageVsSlowed, .08f, "Friends deal +8% damage to slowed enemies."),
                T(KitPassive.SlowToFreeze, 4, "A friend's hit on a slowed enemy freezes it for 0.3s (once every 4s per enemy)."),
                T(KitPassive.KillCoinsNearHero, .1f, "Bubble-wrapped coins: enemies that fall near you have a 10% chance to drop an extra coin.")),
                k => { k.HeroMoveSpeed = .4f; k.HeroAttackSpeed = .25f; }),
            With(K("VelvetRegent", "ROYAL PARADE", KitAction.Parade, "friends' attacks", "Knighting: for the parade every friend fights one star higher (+50% damage, +20% attack speed).",
                H(KitPassive.DamageWithAllies, .1f, "With at least 3 friends, you deal +10% damage."),
                T(KitPassive.NewRecruitPlusOne, 1, "The court grows: you may recruit one more new kind of friend each wave."),
                T(KitPassive.TeamDamage, .06f, "Friends +6% damage."),
                T(KitPassive.SignatureBondLevel, 1, "Velvet Court counts one level higher.")),
                k => { k.Held = true; k.TeamAttackSpeed = .2f; }),
            With(K("ButtonCavalier", "THE DUEL", KitAction.Duel, "near misses, dodges and blows", "Time slows around one foe: you thrust six times in a flurry; +50% attack speed.",
                H(KitPassive.PoiseDodge, .04f, "Each poise (up to 3) gives +4% dodge."),
                T(KitPassive.SecondPurchaseDiscount, .1f, "Courtesy: from the second purchase on, each shop's goods cost 10% less."),
                T(KitPassive.TeamCritOnHeroDodge, .1f, "When you dodge, friends get +10% crit chance for 2s."),
                T(KitPassive.TeamMeleeAttackSpeed, .06f, "Melee friends +6% attack speed.")),
                k => { k.Held = true; k.HeroAttackSpeed = .5f; }),
            With(K("NightMaw", "THE FEAST", KitAction.Feast, "kills", "A huge mouth of night swallows ordinary enemies in front of you (never bosses); every swallow heals you.",
                H(KitPassive.Innate, 0, "Hunger: kills heal you."),
                T(KitPassive.FoodPotency, .5f, "Food you buy works 50% better on you."),
                T(KitPassive.TeamDamageVsLowHealth, .08f, "Friends deal +8% damage to enemies below 30% health."),
                T(KitPassive.KillCoinsNearHero, .15f, "Enemies that fall near you have a 15% chance to drop an extra coin."),
                T(KitPassive.TeamAttackSpeedPerKill, .01f, "Each kill gives friends +1% attack speed for the wave (max 10%).")),
                k => { }),
            With(K("ThornTreasurer", "THORN THRONE", KitAction.Throne, "your attacks and blows you take", "You sit on a throne of thorns: every blow against you is answered with thorns; 30% damage reduction.",
                H(KitPassive.Thorns, 1, "Thorns answer the blows you take (once per second)."),
                T(KitPassive.InterestOnGold, .5f, "Compound interest: interest is reckoned on 150% of the gold you hold."),
                T(KitPassive.TeamDamagePerGold, .05f, "Friends +5% damage for every 50 gold you hold (max 10%)."),
                T(KitPassive.RerollCheaperWithGold, 1, "Holding at least 50 gold, rerolls cost 1 less."),
                T(KitPassive.ThornCoin, .1f, "Thorns pay: each thorn that answers a blow has a 10% chance to shake loose a coin.")),
                k => { k.HeroDamageTaken = .7f; }),
            With(K("CitrusMarshal", "CITRUS TRAIN", KitAction.Train, "distance leading the convoy", "Your convoy couples up into a train that runs over enemies; the whole brigade +30% move speed.",
                H(KitPassive.Innate, 0, "The convoy: friends follow in lanes behind you."),
                T(KitPassive.Income, 2, "Supplies: +2 gold at the end of each wave."),
                T(KitPassive.TeamDamageBehindHero, .05f, "Friends behind you deal +5% damage."),
                T(KitPassive.TeamMoveSpeed, .08f, "Friends +8% move speed."),
                T(KitPassive.PickupRadius, .6f, "The convoy sweeps up coins along the way.")),
                k => { k.HeroMoveSpeed = .3f; }),
            With(K("IvorySovereign", "CORONATION", KitAction.Coronation, "injuries and your attacks", "A last decree crosses the field as a verdict line; ordinary enemies under 25% health on it are executed.",
                H(KitPassive.Innate, 0, "Royal health: 18 HP and each blow costs only 1, then 0.5s royal guard."),
                H(KitPassive.Innate, 0, "A Verdict execution restores 1 royal HP (once per 3s)."),
                T(KitPassive.RoyalPick, .3f, "Royal pick: each shop's rarest item costs 30% less."),
                T(KitPassive.TeamDamageWhileHeroHealthy, .06f, "While you hold at least 6 HP, friends deal +6% damage."),
                T(KitPassive.RareBuyDamage, .05f, "Every Mythic or Prismatic you buy gives the team +5% damage for the run (max +25%)."),
                T(KitPassive.BossGold, 5, "Each boss defeated pays 5 gold.")),
                k => { k.Held = true; }),
            With(K("PetalCitadel", "PETAL CITADEL", KitAction.Citadel, "your attacks", "A citadel of petals rises around you: you shoot from its walls with 20% damage reduction.",
                H(KitPassive.Innate, 0, "Four petals grow from the relics you own."),
                H(KitPassive.HeroArmor, .15f, "Petal guard: you take 15% less damage."),
                T(KitPassive.DuplicateRelicBonus, .5f, "A second copy of a relic gives 50% more of its effect."),
                T(KitPassive.TeamDamagePerPetal, .03f, "Each grown petal gives friends +3% damage."),
                T(KitPassive.DuplicateRelicGold, 3, "Twin petals: buying a relic you already own refunds 3 gold (grows by wave)."),
                T(KitPassive.TeamAttackSpeedFullGarden, .1f, "With all four petals grown, friends +10% attack speed.")),
                k => { k.HeroDamageTaken = .8f; }),
            With(K("PatchworkHost", "SHOWTIME", KitAction.Showtime, "your lead friend's hits", "The curtain rises: your featured friend splits into three and performs together. Your lead friend strikes enemies near you first.",
                H(KitPassive.HealOnLeadAct, 4, "Each time your lead friend performs, you heal 1 HP (every 4s at most)."),
                T(KitPassive.BondLeadDouble, 1, "Your lead friend counts twice in its bond."),
                T(KitPassive.TeamLeadDamage, .1f, "Your lead friend +10% damage."),
                T(KitPassive.LeadStarAttackSpeed, .05f, "With your lead at two stars or more, friends +5% attack speed.")),
                k => { }),
            With(K("ArchiveCurator", "LIVING EXHIBIT", KitAction.Exhibit, "new species recorded", "The recorded species step out of your book as paper puppets and fight for you.",
                H(KitPassive.HeroDamageVsRecorded, .15f, "You deal +15% damage to species you have recorded."),
                H(KitPassive.HeroArmor, .15f, "Vellum binding: you take 15% less damage."),
                T(KitPassive.SpeciesGold, 2, "Every 5 new species recorded pay 2 gold."),
                T(KitPassive.TeamDamageVsRecorded, .05f, "Friends deal +5% damage to recorded species."),
                T(KitPassive.TeamCritVsRecorded, .05f, "Friends +5% crit chance against recorded species.")),
                k => { k.Held = true; }),
            With(K("PennyBroker", "BULL MARKET", KitAction.BullMarket, "gold you collect", "Coins you pick up count double and each pickup fires a receipt bolt.",
                H(KitPassive.Innate, 0, "Forward orders: reserve a relic for the next shop."),
                T(KitPassive.Dividends, 5, "Dividends: +1 gold per relic you own at the end of each wave (max 5)."),
                T(KitPassive.TeamAttackSpeedPerRelic, .02f, "Friends +2% attack speed for every relic you own (max 20%)."),
                T(KitPassive.ForwardOrderDiscount, .1f, "Forward orders cost 10% less."),
                T(KitPassive.InterestCap, 1, "Interest cap +1.")),
                k => { }),
            With(K("StormFencer", "THUNDER GOD", KitAction.Thunder, "danger, near misses and dodges", "Your blade becomes lightning: every strike chains to four enemies; +40% attack speed.",
                H(KitPassive.ChainOnHit, .1f, "Your basic attacks have a 10% chance to chain to another enemy."),
                T(KitPassive.EliteEgg, 1, "Large enemies and leaders drop an egg."),
                T(KitPassive.TeamDamageVsShocked, .08f, "Friends deal +8% damage to enemies shocked in the last 3s."),
                T(KitPassive.TeamAttackSpeed, .05f, "Friends +5% attack speed."),
                T(KitPassive.TeamChainChance, .05f, "Friends' hits have a 5% chance to chain.")),
                k => { k.Held = true; k.HeroAttackSpeed = .4f; }),
            With(K("TinVirtuoso", "SYMPHONY", KitAction.Symphony, "your attacks (crits count twice)", "The brigade plays together: on every beat all friends strike at once.",
                H(KitPassive.CritBeats, 2, "Your crits count as two beats."),
                T(KitPassive.BondLevelAttackSpeed, .03f, "Harmony: each active bond level gives friends +3% attack speed (max 15%)."),
                T(KitPassive.TeamCrit, .03f, "Friends +3% crit chance."),
                T(KitPassive.TeamAttackSpeed, .05f, "Friends +5% attack speed.")),
                k => { k.Held = true; k.TeamAttackSpeed = .25f; }),
            With(K("MoonProwler", "ECLIPSE", KitAction.Eclipse, "distance travelled", "The field darkens: you slip behind enemy after enemy, and every attack counts as from behind.",
                H(KitPassive.RearDamage, .45f, "Attacks from behind an enemy deal +45%."),
                T(KitPassive.CoinOnRearHit, .1f, "Pickpocket: a hit from behind has a 10% chance to shake loose a coin."),
                T(KitPassive.TeamDamageVsRear, .08f, "Friends deal +8% damage to enemies turned away from them."),
                T(KitPassive.EnemySlowOnRear, .1f, "Enemies struck from behind are slowed 10%."),
                T(KitPassive.TeamRearCrit, 1, "Melee friends striking an enemy from behind always land a critical hit.")),
                k => { k.HeroAttackSpeed = .25f; }),
            With(K("SupperMortar", "SIX-BARREL KITCHEN", KitAction.Kitchen, "your attacks", "A six-barrel supper mortar replaces your teapot and serves the whole menu; every 6th course lands on the enemy nearest you and knocks it back.",
                H(KitPassive.Innate, 0, "Two courses: each basic attack is served as two quick courses."),
                T(KitPassive.ExtraShopSlot, 1, "The menu: every shop shows one more offer."),
                T(KitPassive.TeamDamagePerFood, .03f, "Each food bought gives friends +3% damage (max 15%)."),
                T(KitPassive.FoodPotency, .2f, "Food works 20% better on you."),
                T(KitPassive.FoodDiscount, .2f, "Food costs 20% less.")),
                k => { k.Gun = true; k.Held = true; k.ReplacesAttacks = true; k.HeroAttackSpeed = .3f; }),
            With(K("LanternWeaver", "GRAND TAPESTRY", KitAction.Tapestry, "new targets pinned", "Every knot joins into one net that tightens: enemies in it are slowed 40% and the net cuts them.",
                H(KitPassive.Pins, 3, "Your hits pin up to 3 enemies; pinned enemies are slowed 10%."),
                T(KitPassive.PinnedPierce, 1, "A friend's shot that passes through a pinned enemy pierces one more."),
                T(KitPassive.TeamDamageVsPinned, .08f, "Friends deal +8% damage to pinned enemies."),
                T(KitPassive.TeamPierce, 1, "Friends' shots pierce one more enemy.")),
                k => { k.Held = true; }),
            With(K("HourglassStag", "FORECLOSURE", KitAction.Foreclosure, "the debt you bank", "Time stops for 1.5s and every debt is collected at once, with interest.",
                H(KitPassive.TimeDebt, .35f, "Time debt: part of your damage is deferred and collected later with interest."),
                T(KitPassive.DebtKillGold, 1, "A debt that finishes its debtor pays 1 gold."),
                T(KitPassive.TeamAttackSpeedWhileDebt, .05f, "While a debt is outstanding, friends +5% attack speed."),
                T(KitPassive.EnemySlowWithDebt, .1f, "Debtors are slowed 10%.")),
                k => { }),
            With(K("RainArchivist", "MONSOON CHAPTER", KitAction.Monsoon, "pages written (chests, healing, dodges, attacks)", "A storm sweeps the field: lightning strikes enemies and the rain heals you.",
                H(KitPassive.Innate, 0, "Chests heal you."),
                T(KitPassive.ChestBonusGold, 2, "Wet chests: the first chest of each wave holds 2 extra gold."),
                T(KitPassive.EnemySlowOnHeroHit, .1f, "Your hits soak enemies: slowed 10% for 1s."),
                T(KitPassive.TeamDamageVsWet, .05f, "Friends deal +5% damage to soaked enemies."),
                T(KitPassive.TeamAttackSpeed, .05f, "Friends +5% attack speed.")),
                k => { k.Held = true; }),
            // the five expansion heroes (Docs/Content_Expansion_v5.md §2–§5, §8): each keeps "+1 signature bond level"
            With(K("KiteAdmiral", "KITE FLEET", KitAction.KiteFleet, "your hits beyond 4 units", "Five kites fly over your five nearest ranged friends: each of their shots splits in two; +20% attack speed.",
                H(KitPassive.FarShotDamage, .15f, "Your hits beyond 4 units deal +15%."),
                T(KitPassive.SignatureBondLevel, 1, "Paper Wings counts one level higher."),
                T(KitPassive.RangedTeamRange, .1f, "Ranged friends +10% range."),
                T(KitPassive.RangedPierceWhileFull, 1, "While your gauge is full, ranged friends' shots pierce 1 more.")),
                k => { k.HeroAttackSpeed = .2f; }),
            With(K("SundaeDuchess", "SUNDAE STORM", KitAction.SundaeRain, "time, blows you take and frozen enemies felled", "Giant scoops drop on enemies; each one freezes what it lands on (bosses are only chilled).",
                H(KitPassive.FoodHealth, 5, "Each food bought: +5 max HP (max +40)."),
                T(KitPassive.SignatureBondLevel, 1, "Sweet Tooth counts one level higher."),
                T(KitPassive.FrozenKillGold, 5, "Each frozen enemy felled: +1 gold (max 5 per wave)."),
                T(KitPassive.FrozenCritShatter, .05f, "A crit on a frozen enemy takes 5% of its max HP (not bosses)."),
                T(KitPassive.FoodDiscount, .15f, "Food costs 15% less.")),
                k => { }),
            With(K("BonsaiClocksmith", "TIME GARDEN", KitAction.TimeGarden, "your hits, time and kills", "A clockwork bonsai is planted ahead: its hand sweeps once round, striking and slowing what it crosses; friends near it +40% attack speed.",
                H(KitPassive.KeepHalfTier, .5f, "After transforming, half a stars stays filled."),
                T(KitPassive.SignatureBondLevel, 1, "Clockwork Union counts one level higher."),
                T(KitPassive.MergeGold, 2, "Every merge pays 2 gold (grows by wave)."),
                T(KitPassive.ThemeBondAttackSpeed, .04f, "Friends +4% attack speed per Clock or Orchard bond level (max 16%).")),
                k => { }),
            With(K("JugglingJester", "GRAND CIRCUS", KitAction.Circus, "friends' crits and your hits", "Five balls slam down on five enemies, then every throw tosses two pins; +30% attack speed.",
                H(KitPassive.CritBall, .5f, "Your crits toss a ball that drops on a random enemy for 50% damage."),
                T(KitPassive.SignatureBondLevel, 1, "Big Top counts one level higher."),
                T(KitPassive.BigTopTwoStar, .1f, "A Big Top friend you recruit: 10% chance to arrive at two stars."),
                T(KitPassive.TeamCrit, .06f, "Friends +6% crit chance.")),
                k => { k.Held = true; k.HeroAttackSpeed = .3f; }),
            With(K("BellDiver", "DIVING BELL", KitAction.DivingBell, "time near enemies and your hits (more on probed ones)", "You ride inside a brass bell: it rams and probes what is ahead and drops depth charges behind; +25% move speed, 50% less damage taken.",
                H(KitPassive.ProbedCoin, .15f, "Your hits on probed enemies: 15% chance to drop a coin (max 5 per wave)."),
                T(KitPassive.SignatureBondLevel, 1, "Rain Script counts one level higher."),
                T(KitPassive.TeamDamageVsProbed, .08f, "Friends deal +8% damage to probed enemies."),
                T(KitPassive.ProbedBounce, 1, "Friends' shots bounce once more off probed enemies.")),
                k => { k.HeroMoveSpeed = .25f; k.HeroDamageTaken = .5f; }),
        };

        private static HeroKit With(HeroKit kit, Action<HeroKit> setup) { setup(kit); return kit; }
        private static Dictionary<string, HeroKit> lookup;
        public static HeroKit For(string id)
        {
            if (id == null) return null;
            if (lookup == null) { lookup = new Dictionary<string, HeroKit>(); foreach (var k in All) lookup[k.Id] = k; }
            return lookup.TryGetValue(id, out var kit) ? kit : null;
        }
        /// <summary>A passive's value for this hero (0 when it does not have it).</summary>
        public static float Value(string heroId, KitPassive kind) { var k = For(heroId); return k == null ? 0 : k.Value(kind); }

        /// <summary>The kit as player-facing text.</summary>
        public static string Describe(HeroKit kit)
        {
            if (kit == null) return "";
            var text = new System.Text.StringBuilder();
            text.Append("TRANSFORMATION: ").Append(kit.Transformation).Append("\nThe gauge beside you fills from ").Append(kit.Fill)
                .Append(" and climbs three steps; when full you transform by yourself for ").Append(kit.Seconds.ToString("0")).Append("s, then rest for 3s. ").Append(kit.Effect);
            text.Append("\n\nHERO PASSIVES");
            foreach (var t in kit.HeroTraits) text.Append("\n- ").Append(t.Text);
            text.Append("\n\nTEAM AND RUN PASSIVES");
            foreach (var t in kit.TeamTraits) text.Append("\n- ").Append(t.Text);
            return text.ToString();
        }
    }
}
