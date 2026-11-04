using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The one purchase (requirement 4/11/2026, Docs/Monetization_Plan_2026-11-03.md; no ads): the phone game is free and
    /// whole, and Deluxe, bought once, adds Endless and the two hardest difficulties (Act II later). On PC (Steam) the game is sold
    /// once with Deluxe included. Nothing that makes a run stronger is ever sold.</summary>
    public static class Deluxe
    {
        /// <summary>The one-time product's id in Google Play Console (Monetize › In-app products).</summary>
        public const string ProductId = "deluxe";
        /// <summary>Shown until the store answers with the player's own price.</summary>
        public const string FallbackPrice = "$4.99";
        /// <summary>The first difficulty that needs Deluxe (Sleepless, then The Last Night).</summary>
        public const int FirstDeluxeDifficulty = 3;
        private const string OwnedKey = "arcane.deluxe.owned";
        private static int sold = -1;

        /// <summary>Sold in the phone builds; the PC game includes it. "-arcane-store" shows the phone store on PC (checks).</summary>
        public static bool Sold
        {
            get
            {
                if (sold < 0) sold = Application.isMobilePlatform && !Application.isEditor || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-store") >= 0 ? 1 : 0;
                return sold == 1;
            }
        }
        /// <summary>Whether the player has Deluxe: always on PC; on phones once bought (the store confirms it again at each launch).</summary>
        public static bool Owned { get { if (!Sold) return true; try { return PlayerPrefs.GetInt(OwnedKey, 0) == 1; } catch (Exception) { return false; } } }
        public static event Action Changed;
        /// <summary>Records the purchase (or a refund) the store reported.</summary>
        public static void Grant(bool owned)
        {
            if (!Sold) return;
            bool before = Owned;
            try { PlayerPrefs.SetInt(OwnedKey, owned ? 1 : 0); PlayerPrefs.Save(); } catch (Exception) { }
            if (before != owned) Changed?.Invoke();
        }
        public static bool DifficultyLocked(int difficulty) => difficulty >= FirstDeluxeDifficulty && !Owned;
        public static bool EndlessLocked => !Owned;
        public static string Price => string.IsNullOrEmpty(DeluxeStore.PriceText) ? FallbackPrice : DeluxeStore.PriceText;
        /// <summary>What Deluxe adds, one short line each (the offer card).</summary>
        public static readonly string[] Contents =
        {
            "Endless: keep your brigade and relics past dawn",
            "Two more difficulties: Sleepless and The Last Night",
            "Act II when it arrives",
        };
    }
}
