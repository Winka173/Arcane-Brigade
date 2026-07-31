using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Writes every hero (id, name, weapon, mechanic and its text, recruit themes, starting friend) to a tab-separated file for design
    /// reviews (batch: -executeMethod ArcaneBrigade.Editor.HeroDump.Run -dumpPath FILE).</summary>
    public static class HeroDump
    {
        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-dumpPath");
            string path = i >= 0 && i + 1 < args.Length ? args[i + 1] : Path.Combine(Application.dataPath, "..", "heroes.tsv");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var sb = new StringBuilder();
            sb.AppendLine("id\tname\tweapon\tkind\tmechanic\tdescription\tthemes\tfriend\tref");
            foreach (UnitDefinition u in catalog.Champions)
            {
                string text = ""; try { text = K11HeroRules.Describe(u); } catch { }
                string friend = HeroLoadout.Heroes.TryGetValue(u.Id, out HeroLoadout.Entry plan) ? plan.Friend : "";
                sb.AppendLine(u.Id + "\t" + u.DisplayName + "\t" + (u.Weapon != null ? u.Weapon.Id : "") + "\t" + (u.Weapon != null ? u.Weapon.Kind.ToString() : "") + "\t" + u.HeroMechanic
                    + "\t" + text.Replace('\n', ' ') + "\t" + string.Join(",", u.RecruitThemes ?? new string[0]) + "\t" + friend + "\t" + u.ReferenceHero);
            }
            File.WriteAllText(path, sb.ToString());
            Debug.Log("HERODUMP " + catalog.Champions.Length + " heroes -> " + path);
        }
    }
}
