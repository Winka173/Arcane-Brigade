using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BuildMorphProfile
    {
        public BuildMorphKind Kind;
        public int Points, Relics, Partners, Tier, Stage;
        public readonly List<string> Sources = new List<string>();
        public string Summary => BuildMorphs.Get(Kind).Name + " / " + BuildMorphs.StageName(Stage) + " / " + Points + (Stage < 3 ? "/" + BuildMorphs.NextThreshold(Stage) : "/6") + " craft";
        public string Explanation => BuildMorphs.Get(Kind).Pattern + "\n\n" + BuildMorphs.Get(Kind).Silhouette + "\n\nCRAFT SOURCES\n" + string.Join("\n", Sources) + "\n\nForms unlock at 2 / 4 / 6 craft with at least one compatible relic. Each different compatible relic grants 1, each different supporting weapon grants 1 (max 2), evolved tier grants 1 / 2. Duplicate copies do not add craft. A supporting weapon must be carried by another permanent party member.\nEvery third attack can release a form, at most once per 2.4 seconds. Its basic hit retains 72% damage; the remaining choreography shares one finite damage budget. No extra input required.";
    }

    public sealed partial class RunSession
    {
        public BuildMorphProfile MorphProfile(WeaponDefinition weapon, int tier, bool companion)
        {
            var p = new BuildMorphProfile { Kind = BuildMorphs.For(weapon), Tier = Mathf.Clamp(tier, 1, 3) };
            if (p.Kind == BuildMorphKind.None) return p;
            var relicIds = new HashSet<string>();
            foreach (ItemStack stack in State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                if (item == null || item.EffectiveStacks(stack.Count) <= 0 || SecretItemSuspended(stack.Id) || !BuildMorphs.Affinity(p.Kind, item, companion) || !relicIds.Add(stack.Id)) continue;
                p.Relics++; p.Sources.Add("+1 " + item.Name);
            }
            var partners = new HashSet<string>();
            void Partner(WeaponDefinition candidate)
            {
                if (candidate == null || candidate.Id == weapon.Id || !BuildMorphs.Supports(p.Kind, candidate) || !partners.Add(candidate.Id) || p.Partners >= 2) return;
                p.Partners++; p.Sources.Add("+1 supporting " + RunUICopy.WeaponName(candidate.Id));
            }
            Partner(catalog.Unit(State.ChampionId).Weapon);
            foreach (RosterEntry entry in State.Roster) Partner(catalog.Weapon(entry.WeaponId));
            if (p.Tier > 1) p.Sources.Add("+" + (p.Tier - 1) + " evolved recruit tier");
            p.Points = p.Relics + p.Partners + p.Tier - 1;
            p.Stage = catalog.ExpansionRevision > 0 ? BuildMorphs.Stage(p.Points, p.Relics) : 0;
            if (p.Sources.Count == 0) p.Sources.Add("No compatible sources yet.");
            if (p.Relics == 0) p.Sources.Add("Needs at least one compatible relic.");
            return p;
        }
        private void PrepareBuildMorphs()
        {
            void Apply(UnitDefinition unit, bool companion)
            {
                if (unit == null) return;
                BuildMorphProfile p = MorphProfile(unit.Weapon, companion ? unit.CombatTier : 1, companion);
                unit.BuildMorph = p.Kind; unit.BuildMorphStage = p.Stage;
            }
            Apply(CombatCatalog.Champion, false);
            foreach (UnitDefinition unit in CombatCatalog.StartingCompanions) Apply(unit, true);
        }
    }
}
