using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8MerchantChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K8 merchant: " + caption); notes.Add("PASS " + caption); };
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            require(art.GoldMerchant.Concat(art.EggMerchant).All(sprite => sprite.texture.name == "StrangeMerchants_v2") && art.GoldMerchant[0] != art.EggMerchant[0], "Eight original surreal merchant states have distinct silhouettes");
            require(art.MarketDecor.Length == 4 && art.MarketDecor.All(sprite => sprite != null), "Two painted rugs and two garlands replace UI-like floor rectangle");
            require(art.MarketRugs.Length == 2 && art.MarketRugs.All(sprite => sprite.rect.width >= 1536 && sprite.texture.name == "MarketRugs_v3"), "Market floor uses full-width source art rather than enlarging small atlas cells");
            foreach (MerchantMoment moment in Enum.GetValues(typeof(MerchantMoment)))
                require(MerchantDialogue.Pick(false, moment, 0).Length > 15 && MerchantDialogue.Pick(true, moment, 0).Length > 15 && MerchantDialogue.Pick(false, moment, 0) != MerchantDialogue.Pick(true, moment, 0), "Independent personalities for " + moment);
            var dialogue = new MerchantDialogue(); dialogue.Speak(false, MerchantMoment.Enter, 0);
            dialogue.Observe(false, "offer:1", 1);
            require(dialogue.SpokenCount == 1, "Entry cannot be immediately interrupted by proximity chatter");
            dialogue.Observe(false, "offer:1", 2.3f);
            require(dialogue.SpokenCount == 2 && dialogue.LastMoment == MerchantMoment.Inspect, "Waiting near goods speaks after entry cooldown without requiring reentry");
            dialogue.Observe(false, "offer:1", 5);
            require(dialogue.SpokenCount == 2, "Stationary focus does not repeatedly spam dialogue");
            dialogue.Speak(true, MerchantMoment.BloodBuy, 5);
            require(dialogue.LastMoment == MerchantMoment.BloodBuy && dialogue.CurrentLine.Length > 0, "HP transaction has separate committed reaction");
            dialogue.Speak(true, MerchantMoment.Denied, 6);
            require(dialogue.LastMoment == MerchantMoment.Denied, "Failed transaction has denial not success dialogue");
            require(RunHud.ShopTooltipBounds(600).y >= 845 && RunHud.ShopTooltipBounds(600).yMax <= 1030, "Long tooltip stays below stock prices and inside safe viewport");
            Directory.CreateDirectory("PreviewExports/K8"); File.WriteAllLines("PreviewExports/K8/Merchant_Checks.txt", notes);
            Debug.Log("K8_MERCHANT_CHECKS_PASSED " + notes.Count);
        }
    }
}
