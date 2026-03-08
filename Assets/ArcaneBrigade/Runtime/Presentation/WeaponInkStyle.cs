using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every weapon's own painted-ink look, so no two weapons share a swing, a hit or a muzzle flash (18/10/2026).
    /// Melee weapons have a hand-picked strike (form, palette, token); every weapon's hits and muzzle flashes use its own
    /// palette and token. ArsenalChecks verifies that no two weapons share a look.</summary>
    public static class WeaponInkStyle
    {
        public enum StrikeForm { Crescent, Chop, Thrust, Drill, Whip, Pound, Snip, Twin, Spin, Rake, Jab, Glaive, Print }

        public readonly struct Strike
        {
            public readonly StrikeForm Form; public readonly InkPalette Main; public readonly InkToken Token; public readonly int Strokes; public readonly string Palette;
            public Strike(StrikeForm form, string palette, InkToken token, int strokes = 1) { Form = form; Palette = palette; Main = Pal(palette); Token = token; Strokes = strokes; }
        }

        public static InkPalette Pal(string name) => name switch
        {
            "Cream" => ArsenalInk.Cream, "Gold" => ArsenalInk.Gold, "Amber" => ArsenalInk.Amber, "Wine" => ArsenalInk.Wine, "Water" => ArsenalInk.Water,
            "Green" => ArsenalInk.Green, "Violet" => ArsenalInk.Violet, "Terracotta" => ArsenalInk.Terracotta, "Leather" => ArsenalInk.Leather,
            "Candy" => ArsenalInk.Candy, "Cyan" => ArsenalInk.Cyan, "Silver" => ArsenalInk.Silver, "Midnight" => ArsenalInk.Midnight, "Glass" => ArsenalInk.Glass,
            "Dust" => ArsenalInk.Dust, "Cotton" => ArsenalInk.Cotton, _ => ArsenalInk.Cream
        };

        /// <summary>The 40 melee weapons' swings: each (form, palette, token) appears once.</summary>
        public static readonly Dictionary<string, Strike> Strikes = new Dictionary<string, Strike>
        {
            ["KeyCleaver"] = new Strike(StrikeForm.Chop, "Gold", InkToken.Coin),
            ["UmbrellaLance"] = new Strike(StrikeForm.Thrust, "Water", InkToken.Drop, 2),
            ["BookPaddle"] = new Strike(StrikeForm.Crescent, "Cream", InkToken.Page),
            ["DreamStamp"] = new Strike(StrikeForm.Print, "Wine", InkToken.Seal),
            ["CloudScissors"] = new Strike(StrikeForm.Snip, "Cyan", InkToken.Puff),
            ["DreamIron"] = new Strike(StrikeForm.Pound, "Amber", InkToken.Puff),
            ["RubberHammer"] = new Strike(StrikeForm.Pound, "Candy", InkToken.Star),
            ["SpoonSabre"] = new Strike(StrikeForm.Crescent, "Silver", InkToken.Drop),
            ["ToastCleaver"] = new Strike(StrikeForm.Chop, "Amber", InkToken.Sand),
            ["LollipopLance"] = new Strike(StrikeForm.Thrust, "Candy", InkToken.Spark),
            ["PaperFan"] = new Strike(StrikeForm.Crescent, "Wine", InkToken.Petal, 2),
            ["CorkscrewRapier"] = new Strike(StrikeForm.Drill, "Silver", InkToken.Glint),
            ["TeabagFlail"] = new Strike(StrikeForm.Whip, "Amber", InkToken.Drop),
            ["UmbrellaHook"] = new Strike(StrikeForm.Crescent, "Midnight", InkToken.Bubble),
            ["PizzaWheel"] = new Strike(StrikeForm.Spin, "Amber", InkToken.Citrus),
            ["MoonScythe"] = new Strike(StrikeForm.Crescent, "Violet", InkToken.Moon),
            ["ForkTrident"] = new Strike(StrikeForm.Thrust, "Silver", InkToken.Spear, 3),
            ["MaracaHammer"] = new Strike(StrikeForm.Pound, "Green", InkToken.Note),
            ["TapeWhip"] = new Strike(StrikeForm.Whip, "Gold", InkToken.Patch),
            ["TwinSabres"] = new Strike(StrikeForm.Twin, "Silver", InkToken.Glint),
            ["ClockCrescent"] = new Strike(StrikeForm.Spin, "Gold", InkToken.Star),
            ["ParasolNeedle"] = new Strike(StrikeForm.Thrust, "Candy", InkToken.Petal),
            ["StampHammer"] = new Strike(StrikeForm.Pound, "Midnight", InkToken.Envelope),
            ["ClockworkYoYo"] = new Strike(StrikeForm.Spin, "Water", InkToken.Button),
            ["SpringKnocker"] = new Strike(StrikeForm.Pound, "Silver", InkToken.Bolt),
            ["RibbonWheel"] = new Strike(StrikeForm.Spin, "Wine", InkToken.Knot),
            ["AfterimageSabre"] = new Strike(StrikeForm.Twin, "Violet", InkToken.Shard),
            ["HomeRunBat"] = new Strike(StrikeForm.Crescent, "Leather", InkToken.Star),
            ["StarforgeClaymore"] = new Strike(StrikeForm.Crescent, "Gold", InkToken.Star, 2),
            ["DragonLanternGlaive"] = new Strike(StrikeForm.Glaive, "Amber", InkToken.Ember),
            ["DrumstickClub"] = new Strike(StrikeForm.Pound, "Amber", InkToken.Sand),
            ["LeekBaton"] = new Strike(StrikeForm.Crescent, "Green", InkToken.Leaf, 2),
            ["FlySwatter"] = new Strike(StrikeForm.Print, "Wine", InkToken.Fang),
            ["PawMitten"] = new Strike(StrikeForm.Rake, "Silver", InkToken.Glint),
            ["VerdictGavel"] = new Strike(StrikeForm.Chop, "Gold", InkToken.Seal),
            ["BareKnuckles"] = new Strike(StrikeForm.Jab, "Cream", InkToken.Star),
            ["SignpostSlammer"] = new Strike(StrikeForm.Chop, "Cyan", InkToken.Glint),
            ["WindupWrench"] = new Strike(StrikeForm.Crescent, "Silver", InkToken.Bolt),
            ["StarKnuckle"] = new Strike(StrikeForm.Jab, "Silver", InkToken.Star),
            ["TopiaryShears"] = new Strike(StrikeForm.Snip, "Green", InkToken.Leaf),
        };

        private static readonly string[] Palettes = { "Cream", "Gold", "Amber", "Wine", "Water", "Green", "Violet", "Terracotta", "Leather", "Candy", "Cyan", "Silver", "Midnight", "Glass", "Dust" };
        private static readonly InkToken[] Tokens = (InkToken[])Enum.GetValues(typeof(InkToken));
        private static Dictionary<string, (string palette, InkToken token)> ranged;

        /// <summary>The palette and token of a weapon's hits and muzzle flash: a melee weapon's strike look; a ranged weapon's
        /// own pair, given out in name order so no two ranged weapons share one.</summary>
        public static (InkPalette palette, InkToken token, string paletteName) Look(WeaponDefinition weapon)
        {
            if (weapon == null) return (ArsenalInk.Cream, InkToken.Spark, "Cream");
            if (Strikes.TryGetValue(weapon.Id, out Strike s)) return (s.Main, s.Token, s.Palette);
            if (ranged == null)
            {
                ranged = new Dictionary<string, (string, InkToken)>();
                RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
                var ids = new List<string>();
                if (catalog != null) foreach (WeaponDefinition w in catalog.Weapons) if (w != null && !Strikes.ContainsKey(w.Id) && !ids.Contains(w.Id)) ids.Add(w.Id);
                foreach (string extra in ExtraRanged) if (!ids.Contains(extra)) ids.Add(extra);
                ids.Sort(StringComparer.Ordinal);
                // (i mod 15, 7i mod |tokens|): distinct for the first 15 x |tokens| / gcd weapons
                for (int i = 0; i < ids.Count; i++) ranged[ids[i]] = (Palettes[i % Palettes.Length], Tokens[(i * 7) % Tokens.Length]);
            }
            if (!ranged.TryGetValue(weapon.Id, out var pair)) pair = (Palettes[Mathf.Abs(weapon.Id.GetHashCode()) % Palettes.Length], Tokens[Mathf.Abs(weapon.Id.GetHashCode() / 7) % Tokens.Length]);
            return (Pal(pair.palette), pair.token, pair.palette);
        }
        /// <summary>Ranged weapons that may not be in the catalog yet (waiting for art) still get a fixed pair.</summary>
        private static readonly string[] ExtraRanged = { "FortuneGlobe", "PaperLanternBomb", "SnowGlobeGrenade", "PinataBomb", "AcornCluster" };
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { ranged = null; }
    }
}
