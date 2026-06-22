using System;
using System.Collections.Generic;

namespace ArcaneBrigade
{
    /// <summary>Each hero's own relic pool (requirement 25/10/2026: each hero's shop offers only relics that fit how it plays). The
    /// theme filter (OctoberItemEligible) still decides which themed relics a hero may see; this leaves out the relics that work
    /// against the hero's kit. Shared team and economy relics stay in every pool.</summary>
    public static class HeroRelicPools
    {
        /// <summary>Heroes whose power comes from their friends (gauge, orders, lead, beats, nursery, convoy, kites, bonds).</summary>
        private static readonly string[] FriendHeroes = { "LanternKnight", "VelvetRegent", "PatchworkHost", "TinVirtuoso", "VerdantMatron", "BonsaiClocksmith", "KiteAdmiral", "JugglingJester", "CitrusMarshal" };
        /// <summary>Heroes whose kit runs on their own basic hits (stamps, pins, debts, beats, records, ripostes...).</summary>
        private static readonly string[] OwnAttackHeroes = { "Postmaster", "DrowsySeer", "LoneJelly", "ThornTreasurer", "IvorySovereign", "PetalCitadel", "ArchiveCurator", "TinVirtuoso", "SupperMortar", "LanternWeaver", "HourglassStag", "KiteAdmiral", "JugglingJester", "BellDiver", "TreasureAcorn", "MoonProwler", "ButtonCavalier", "MirrorKeeper", "ChinaMarionette" };
        /// <summary>Where the rules are not enough: relics a hero's kit contradicts.</summary>
        private static readonly Dictionary<string, string[]> Excluded = new Dictionary<string, string[]>
        {
            // friends must stay within the lamp's 4 units / ride in the convoy's lanes
            ["LanternKnight"] = new[] { "CurioAnchorTack", "CurioWanderLetter" },
            ["CitrusMarshal"] = new[] { "CurioAnchorTack", "CurioWanderLetter", "CurioLastHopeMotor", "CurioStillwaterUrn" },
            // the gauge fills by moving: standing-still relics fight it
            ["CometRunner"] = new[] { "CurioLastHopeMotor", "CurioStillwaterUrn" },
            ["MoonProwler"] = new[] { "CurioLastHopeMotor", "CurioStillwaterUrn" },
            ["SoapSkater"] = new[] { "CurioLastHopeMotor", "CurioStillwaterUrn" },
            // the pulse scales with max HP, and the royal HP stays 18
            ["IvorySovereign"] = new[] { "CurioSorrowLantern" },
        };

        public static bool Fits(UnitDefinition hero, ItemDefinition item)
        {
            if (hero == null || item == null) return true;
            if (hero.Id != null && Excluded.TryGetValue(hero.Id, out string[] ids) && Array.IndexOf(ids, item.Id) >= 0) return false;
            bool friends = hero.HeroMechanic != HeroMechanic.SoloMaw, shoots = hero.Weapon != null && hero.Weapon.Kind == WeaponKind.Projectile;
            RelicMechanic mechanic = item.Mechanic;
            if (Array.IndexOf(FriendHeroes, hero.Id) >= 0 && SidelinesFriends(mechanic)) return false;
            if (!shoots && (mechanic == RelicMechanic.PrismEcho || mechanic == RelicMechanic.SingleTicket)) return false;
            if (Array.IndexOf(OwnAttackHeroes, hero.Id) >= 0 && mechanic == RelicMechanic.CollisionOnly) return false;
            if (mechanic != RelicMechanic.None && !PenaltyMechanic(mechanic))
            {
                if (ThroughFriends(mechanic)) return friends;
                if (mechanic == RelicMechanic.DrinkVitality || mechanic == RelicMechanic.LuckToHealth || mechanic == RelicMechanic.FoodGrowth) return !FixedHealth(hero);
                if (mechanic == RelicMechanic.SlowToArmor) return hero.ReferenceHero != "Card Queen";
                if (mechanic == RelicMechanic.QuietMending) return !NoRegeneration(hero);
                return true;
            }
            // a stat relic fits when its headline (first gain) works for the hero; one that trades the hero's attack for a shot it
            // never fires does not
            StatModifier headline = null; bool heroCost = false, lostShot = false;
            foreach (StatModifier modifier in item.EffectiveModifiers)
            {
                if (modifier.Value < 0 && modifier.Scope != ModifierScope.Run && Useful(hero, modifier, friends, shoots)) heroCost = true;
                if (modifier.Value <= 0) continue;
                headline ??= modifier;
                if (modifier.Scope == ModifierScope.Champion && Shot(modifier.Effect) && !shoots) lostShot = true;
            }
            if (lostShot && heroCost) return false;
            return headline == null || Useful(hero, headline, friends, shoots); // no gain listed: its effect is a trigger every hero gets
        }

        /// <summary>The relics of a list that fit the hero; the whole list when none would, so a shop always has its offers.</summary>
        public static List<ItemDefinition> Pool(UnitDefinition hero, List<ItemDefinition> items)
        {
            var fitting = items.FindAll(item => Fits(hero, item));
            return fitting.Count > 0 ? fitting : items;
        }

        private static bool Useful(UnitDefinition hero, StatModifier modifier, bool friends, bool shoots)
        {
            ItemEffect effect = modifier.Effect;
            if (modifier.Scope == ModifierScope.Companions || effect is ItemEffect.CompanionDamage or ItemEffect.CompanionAttackSpeed or ItemEffect.CompanionSize or ItemEffect.Interceptor) return friends;
            if (effect is ItemEffect.Health or ItemEffect.HealthCorrection) return !FixedHealth(hero);
            if (effect is ItemEffect.Armor or ItemEffect.ArmorCorrection) return hero.ReferenceHero != "Card Queen"; // a royal blow always costs 1
            if (effect == ItemEffect.Heal) return !NoRegeneration(hero);
            if (Shot(effect)) return modifier.Scope == ModifierScope.Champion ? shoots : shoots || friends;
            if (effect == ItemEffect.MeleeDamage) return modifier.Scope == ModifierScope.Champion ? !shoots : !shoots || friends;
            return true;
        }
        /// <summary>Card Queen's royal HP and Periwinky's gold-made HP ignore max-HP bonuses.</summary>
        private static bool FixedHealth(UnitDefinition hero) => hero.ReferenceHero == "Card Queen" || hero.ReferenceHero == "Periwinky";
        /// <summary>Omnom (kills heal) and Card Queen never regenerate, so regeneration bonuses do nothing for them.</summary>
        private static bool NoRegeneration(UnitDefinition hero) => hero.ReferenceHero == "Omnom" || hero.ReferenceHero == "Card Queen";
        private static bool Shot(ItemEffect effect) => effect is ItemEffect.Projectiles or ItemEffect.BulletSplit or ItemEffect.BulletPierce or ItemEffect.BulletBounce
            or ItemEffect.BulletSize or ItemEffect.BulletSpeed or ItemEffect.BulletDistance or ItemEffect.RangedDamage;
        /// <summary>The Lone line and the relics that bench friends or take over their attacks.</summary>
        private static bool SidelinesFriends(RelicMechanic m) => m is RelicMechanic.LoneLantern or RelicMechanic.EmptyChairs or RelicMechanic.SingleTicket
            or RelicMechanic.NightAlone or RelicMechanic.IronResolve or RelicMechanic.SoloOverture or RelicMechanic.SoloOverdrive or RelicMechanic.SixWeapons;
        /// <summary>Mechanics that act only through friends (their attacks, stars, merges, bonds or movement).</summary>
        private static bool ThroughFriends(RelicMechanic m) => m is RelicMechanic.CloseFollowers or RelicMechanic.FreeFollowers or RelicMechanic.StaticFollowers
            or RelicMechanic.SpringDouble or RelicMechanic.EchoForward or RelicMechanic.BlowerReturn or RelicMechanic.BorrowedCrown or RelicMechanic.ImpossiblePrism
            or RelicMechanic.LivingThread or RelicMechanic.StarPolish or RelicMechanic.KiteTailRibbon or RelicMechanic.CuckooPendulum or RelicMechanic.TwinTicket
            or RelicMechanic.DuelistsRibbon or RelicMechanic.FunhouseMirror or RelicMechanic.SpliceKit or RelicMechanic.WindChime or RelicMechanic.SlipstreamScarf
            or RelicMechanic.CompanionGift or RelicMechanic.MirrorOfTwins or RelicMechanic.ClockworkHeart or RelicMechanic.MoonClockKey;
        /// <summary>Drawback mechanics: the relic is judged by its stats.</summary>
        private static bool PenaltyMechanic(RelicMechanic m) => m is RelicMechanic.CrowdedTape or RelicMechanic.ExposedRecovery or RelicMechanic.HalfHealthSeed
            or RelicMechanic.BleedingAlarm or RelicMechanic.FragilePower or RelicMechanic.ChestOnlyRecovery;
    }
}
