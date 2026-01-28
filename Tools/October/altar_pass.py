from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
p=ROOT/'Assets/ArcaneBrigade/Runtime/Core/RunSession.cs';s=p.read_text(encoding='utf-8-sig')
start=s.index('            RosterEntry first = LowestTier',s.index('public bool Altar('));end=s.index('            State.Eggs -= recipe.EggCost;',start)
s=s[:start]+'''            List<RosterEntry> materials = AltarMaterials(recipe);
            if (materials == null) return false;
            var choices = new List<WeaponDefinition>();
            foreach (RosterEntry r in materials)
                if (RecruitWeaponEligible(recipe.ResultUnit, catalog.Weapon(r.WeaponId)) && !State.Roster.Exists(other => !materials.Contains(other) && other.UnitId == recipe.ResultUnit && other.WeaponId == r.WeaponId)) choices.Add(catalog.Weapon(r.WeaponId));
            if (choices.Count == 0) return false;
            float previousMaximum = MaximumHealth();
            foreach (RosterEntry r in materials) State.Roster.Remove(r);
            var result = new RosterEntry { UnitId = recipe.ResultUnit, Tier = recipe.ResultTier, WeaponId = choices[Next(ref State.WeaponRandom, choices.Count)].Id };
            State.Roster.Add(result); RememberVariant(result); OctoberPurchase(catalog.Unit(recipe.ResultUnit));
            if (OctoberAbsorbs) { State.AbsorbedSkills.Add(result.UnitId); State.Roster.Remove(result); }
'''+s[end:]
start=s.index('            RosterEntry first = LowestTier',s.index('public bool CanAltar('));end=s.index('\n        }',start)
s=s[:start]+'''            List<RosterEntry> materials = AltarMaterials(recipe);
            return materials != null && materials.Exists(r => RecruitWeaponEligible(recipe.ResultUnit, catalog.Weapon(r.WeaponId)) && !State.Roster.Exists(other => !materials.Contains(other) && other.UnitId == recipe.ResultUnit && other.WeaponId == r.WeaponId));'''+s[end:]
start=s.index('            RosterEntry first = LowestTier',s.index('public string RecipePreview('));end=s.index('\n        }',start)
s=s[:start]+'''            var result = new System.Text.StringBuilder("Spend " + recipe.EggCost + " eggs and the listed materials:\\n");
            foreach (string id in new[] { recipe.FirstUnit, recipe.SecondUnit, recipe.ThirdUnit })
                if (!string.IsNullOrEmpty(id)) result.Append("- ").Append(catalog.Unit(id).DisplayName).Append(" (tier ").Append(recipe.MinimumMaterialTier).Append("+)\\n");
            result.Append("Result: ").Append(catalog.Unit(recipe.ResultUnit).DisplayName).Append(" / tier ").Append(recipe.ResultTier).Append("\\nKeeps one random ingredient weapon.\\n").Append(catalog.Unit(recipe.ResultUnit).Description).Append("\\n").Append(CanAltar(recipeId) ? "READY" : "NOT READY");
            return result.ToString();'''+s[end:]
s=s.replace('State.Roster.FindAll(r => r.UnitId == unit.Id && r.Tier == 1).Count >= 2','OctoberMergeTier(unit.Id) > 0')
p.write_text(s,encoding='utf-8')
p=ROOT/'Assets/ArcaneBrigade/Runtime/Core/RunModifierRules.cs';s=p.read_text(encoding='utf-8-sig').replace('!Array.Exists(catalog.Companions, unit => unit.Id == recipe.ResultUnit) || recipe.EggCost','!Array.Exists(catalog.Companions, unit => unit.Id == recipe.ResultUnit) || !string.IsNullOrEmpty(recipe.ThirdUnit) && !Array.Exists(catalog.Companions, unit => unit.Id == recipe.ThirdUnit) || recipe.EggCost');p.write_text(s,encoding='utf-8')
print('Three ingredient altar wiring applied')
