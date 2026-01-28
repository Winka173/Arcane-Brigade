using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class OctoberAudit
    {
        public static void Export()
        {
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            const string root = "PreviewExports/October"; Directory.CreateDirectory(root);
            string Cell(object value) => (value?.ToString() ?? "").Replace("\t", " ").Replace("\r", "").Replace("\n", " / ");
            void Save(string name, string header, IEnumerable<object[]> rows) => File.WriteAllLines(Path.Combine(root, name + ".tsv"), new[] { header }.Concat(rows.Select(r => string.Join("\t", r.Select(Cell)))), new UTF8Encoding(false));
            Save("Heroes", "Reference\tId\tName\tThemes\tMechanics", c.Champions.Select(u => new object[] { u.ReferenceHero, u.Id, u.DisplayName, string.Join(", ", u.RecruitThemes), u.Description }));
            Save("Companions", "Reference\tId\tName\tBonds\tAcquisition\tMechanics", c.Companions.Select(u => new object[] { u.ReferenceCompanion, u.Id, u.DisplayName, string.Join(", ", u.Tags), u.GiftOnly ? "Gift only" : u.SpecialRecruit ? "Altar / special reward" : string.IsNullOrEmpty(u.ExclusiveHero) ? "Themed shop" : "Exclusive: " + u.ExclusiveHero, u.Description }));
            Save("Relics", "Reference\tId\tName\tRarity\tThemes\tEffects", c.Items.Select(i => new object[] { i.ReferenceName, i.Id, i.Name, i.Rarity, string.Join(", ", i.ShopThemes ?? Array.Empty<string>()), RunModifierRules.Describe(i, 1) }));
            Save("Bonds", "Reference\tId\tName\tThreshold\tEffect\tValue", c.Bonds.Select(b => { int at = Array.IndexOf(OctoberContent.BondIds, b.Id); return new object[] { at < 0 ? "Original" : OctoberContent.ReferenceBonds[at], b.Id, b.Name, b.Threshold, b.Effect, b.Value }; }));
            Save("Recipes", "Id\tFirst\tSecond\tThird\tResult\tTier\tEggs", c.Recipes.Select(r => new object[] { r.Id, r.FirstUnit, r.SecondUnit, r.ThirdUnit, r.ResultUnit, r.ResultTier, r.EggCost }));
            Save("Achievements", "Id\tName\tCondition\tMetric\tGoal", RunAchievements.Definitions(c).Select(a => new object[] { a.Id, a.Name, a.Description, a.Metric, a.Goal }));
            PromptInventory.Export();
            Debug.Log("OCTOBER_AUDIT exported actual runtime catalog");
        }
    }
}
