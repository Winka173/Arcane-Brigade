using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Writes every companion (id, name, rarity, bonds, role, weapon, own skill, passive) and every bond to a tab-separated file, for design
    /// reviews (batch: -executeMethod ArcaneBrigade.Editor.CompanionDump.Run -dumpPath FILE).</summary>
    public static class CompanionDump
    {
        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-dumpPath");
            string path = i >= 0 && i + 1 < args.Length ? args[i + 1] : Path.Combine(Application.dataPath, "..", "companions.tsv");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var sb = new StringBuilder();
            sb.AppendLine("COMPANION\tid\tname\trarity\tspecial\tbonds\trole\tweapon\tskill\tpassive");
            foreach (UnitDefinition u in catalog.Companions)
            {
                string role = "";
                try { role = CompanionRoles.Word(CompanionRoles.For(u)); } catch { }
                CombatSimulation.SkillCopy.TryGetValue(u.Id, out string skill);
                string passive = "";
                try { passive = CompanionDescriptions.ShortPassive(u, 1, catalog.TierSupportMultiplier, 1); } catch { }
                sb.AppendLine("C\t" + u.Id + "\t" + u.DisplayName + "\t" + u.Rarity + "\t" + u.SpecialRecruit + "\t" + string.Join(",", u.Tags ?? new string[0]) + "\t" + role
                    + "\t" + (u.Weapon != null ? u.Weapon.Id + "/" + u.Weapon.Kind : "") + "\t" + (skill ?? "").Replace('\n', ' ') + "\t" + passive.Replace('\n', ' '));
            }
            sb.AppendLine("BOND\tid\tname\tthreshold\teffect\tvalue\textra");
            foreach (BondDefinition b in catalog.Bonds)
                sb.AppendLine("B\t" + b.Id + "\t" + b.Name + "\t" + b.Threshold + "\t" + b.Effect + "\t" + b.Value + "\t" + string.Join(";", (b.ExtraBonuses ?? new BondThresholdBonus[0]).Select(x => x.Threshold + ":" + x.Value)));
            sb.AppendLine("HERO\tid\tname\tmechanic");
            foreach (UnitDefinition u in catalog.Champions) sb.AppendLine("H\t" + u.Id + "\t" + u.DisplayName + "\t" + u.HeroMechanic);
            File.WriteAllText(path, sb.ToString());
            Debug.Log("COMPANIONDUMP " + catalog.Companions.Length + " companions, " + catalog.Bonds.Length + " bonds -> " + path);
        }
    }
}
