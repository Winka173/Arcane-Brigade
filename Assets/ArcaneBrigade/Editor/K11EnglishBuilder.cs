using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11EnglishBuilder
    {
        public static bool NeedsTranslation(string text) => !string.IsNullOrEmpty(text) && text.Any(letter => letter > 127 && char.IsLetter(letter));
        private static string Title(string id)
        {
            string words = Regex.Replace(id ?? "", "([a-z0-9])([A-Z])", "$1 $2").Replace('-', ' ').Replace('_', ' ');
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words);
        }

        public static void Apply()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Concat(catalog.Bosses))
            {
                if (NeedsTranslation(unit.DisplayName)) unit.DisplayName = Title(unit.Id);
                if (NeedsTranslation(unit.Description))
                    unit.Description = unit.Id switch
                    {
                        "LanternKnight" => "A keyblade-bearing pioneer. Travel to charge a lantern pulse; build a bustling brigade.",
                        "Postmaster" => "A wandering parcel launcher. Collected gold charges automatic express volleys.",
                        "DrowsySeer" => "A sleepy tuning-bell oracle. Stand still to chill nearby threats and erase a few hostile bullets.",
                        _ => RunUICopy.PassiveDescription(unit.Passive) + "\n" + RunModifierRules.DescribeCompanion(unit, 1)
                    };
                EditorUtility.SetDirty(unit);
            }
            foreach (ItemDefinition item in catalog.Items)
            {
                if (NeedsTranslation(item.Name)) item.Name = item.Id.StartsWith("k10-") ? "Contract · " + K10StatRules.Name(item.Effect) : Title(item.Id);
                if (NeedsTranslation(item.Description)) item.Description = "Lasts for this run, including waves and endless. Resets in a new run.\n" + RunModifierRules.Describe(item, 1);
            }
            foreach (BondDefinition bond in catalog.Bonds)
                if (NeedsTranslation(bond.Name)) bond.Name = Title(bond.Id);
            foreach (BuildLinkRule link in catalog.BuildLinks)
            {
                if (NeedsTranslation(link.Name)) link.Name = Title(link.Id);
                if (NeedsTranslation(link.Description)) link.Description = "Connected relics: " + string.Join(", ", link.ItemIds.Select(id => catalog.Item(id).Name)) + ". No hidden set bonus.";
            }
            for (int index = 0; index < catalog.WaveModifiers.Length; index++)
            {
                WaveModifier wave = catalog.WaveModifiers[index];
                if (NeedsTranslation(wave.Name)) wave.Name = wave.Healing > 0 ? "Breathing Room" : wave.GoldPerKill > 2 ? "Golden Rush" : "Restless Night " + (index + 1);
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            Debug.Log("K11_ENGLISH_CONTENT_APPLIED");
        }
    }
}
