using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class BuildMorphReadModel
    {
        public static void Append(RunBuildReadModel model, RunSession session, RunCatalog catalog, CheckpointCatalog compiled, bool statsOnly = false)
        {
            BuildMorphArtwork art = Resources.Load<BuildMorphArtwork>("BuildMorphArtwork");
            BuildMorphProfile main = session.MorphProfile(catalog.Unit(session.State.ChampionId).Weapon, 1, false);
            if (main.Kind != BuildMorphKind.None)
                model.Stats.Add(new RunBuildReadModel.Row { Name = "Champion form", Icon = art == null ? null : art.Charm(main.Kind), Summary = main.Summary, Explanation = main.Explanation });
            if (statsOnly) return;
            for (int i = 0; i < model.Companions.Count && i < session.ActiveRoster.Count; i++)
            {
                RosterEntry entry = session.ActiveRoster[i];
                BuildMorphProfile p = session.MorphProfile(catalog.Weapon(entry.WeaponId), entry.Tier, true);
                if (p.Kind != BuildMorphKind.None) model.Companions[i].Explanation += "\n\nBUILD FORM\n" + p.Summary + "\n" + p.Explanation;
            }
            // Put forms before older stat-link receipts so their next unlock is easy to find.
            foreach (BuildMorphDefinition definition in BuildMorphs.All)
            {
                BuildMorphProfile best = null;
                string carriers = "";
                void Carrier(WeaponDefinition w, int tier, bool companion, string name)
                {
                    if (BuildMorphs.For(w) != definition.Kind) return;
                    BuildMorphProfile p = session.MorphProfile(w, tier, companion);
                    if (best == null || p.Stage > best.Stage || p.Stage == best.Stage && p.Points > best.Points) best = p;
                    carriers += name + " / " + RunUICopy.WeaponName(w.Id) + " / " + BuildMorphs.StageName(p.Stage) + " / " + p.Points + " craft\n";
                }
                Carrier(catalog.Unit(session.State.ChampionId).Weapon, 1, false, "Champion");
                foreach (RosterEntry entry in session.ActiveRoster) Carrier(catalog.Weapon(entry.WeaponId), entry.Tier, true, RunUICopy.UnitName(catalog.Unit(entry.UnitId)));
                string weapons = "";
                foreach (WeaponDefinition w in catalog.Weapons)
                    if (!w.SpecialOnly && BuildMorphs.For(w) == definition.Kind) weapons += RunUICopy.WeaponName(w.Id) + ", ";
                string affinities = "";
                foreach (ItemEffect effect in definition.Affinities) affinities += RunModifierRules.ModifierName(effect) + ", ";
                string explanation = (best == null ? definition.Pattern + "\n\nRecruit a carrier first.\n" : best.Explanation + "\n\nCURRENT CARRIERS\n" + carriers) + "\nCOMPATIBLE WEAPONS\n" + weapons.TrimEnd(',', ' ') + "\n\nRELIC AFFINITIES\nPositive " + affinities.TrimEnd(',', ' ') + ". Champion/companion scope must match the carrier. Deposited, lent or rewritten relics do not contribute their old identity.\n\nSpecial attacks share a total budget of 80% / 100% / 120% of one basic attack across the entire pattern and at most 3 / 4 / 5 targets per segment. Enemy danger cues always draw above friendly form effects.";
                var row = new RunBuildReadModel.Row { Name = "Form / " + definition.Name, Icon = art == null ? null : art.Charm(definition.Kind), Summary = best == null ? "No carrier yet\n" + definition.Silhouette : BuildMorphs.StageName(best.Stage) + " / " + best.Points + "/" + BuildMorphs.NextThreshold(best.Stage) + " craft\n" + definition.Silhouette, Explanation = explanation };
                model.Links.Add(row);
            }
        }

        public static string ItemPreview(RunSession session, RunCatalog catalog, ItemDefinition item)
        {
            if (item == null || session.StackCount(item.Id) > 0 || catalog.ExpansionRevision <= 0) return "";
            var seen = new HashSet<BuildMorphKind>();
            var previews = new List<string>();
            void Preview(WeaponDefinition w, int tier, bool companion)
            {
                BuildMorphProfile p = session.MorphProfile(w, tier, companion);
                if (p.Kind == BuildMorphKind.None || !BuildMorphs.Affinity(p.Kind, item, companion) || seen.Contains(p.Kind) || p.Stage >= 3) return;
                seen.Add(p.Kind);
                int after = BuildMorphs.Stage(p.Points + 1, p.Relics + 1);
                string name = BuildMorphs.Get(p.Kind).Name;
                previews.Add(after > p.Stage ? "FORM UP: " + name + " / " + BuildMorphs.StageName(after) : "CRAFT +1: " + name + " (" + (p.Points + 1) + "/" + BuildMorphs.NextThreshold(p.Stage) + ")");
            }
            Preview(catalog.Unit(session.State.ChampionId).Weapon, 1, false);
            foreach (RosterEntry entry in session.ActiveRoster) Preview(catalog.Weapon(entry.WeaponId), entry.Tier, true);
            return string.Join("\n", previews);
        }
    }
}
