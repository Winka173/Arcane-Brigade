using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class AssetRoleAudit
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var forbidden = new HashSet<Sprite>();
            foreach (UnitDefinition unit in catalog.Companions.Concat(catalog.Champions).Concat(catalog.Enemies).Concat(catalog.Bosses))
            {
                if (unit.Body != null) forbidden.Add(unit.Body);
                foreach (Sprite frame in unit.AnimationFrames) if (frame != null) forbidden.Add(frame);
                if (unit.AuthoredMotion == null) continue;
                foreach (Sprite frame in unit.AuthoredMotion.Idle.Concat(unit.AuthoredMotion.Walk).Concat(unit.AuthoredMotion.Windup).Concat(unit.AuthoredMotion.Release).Concat(unit.AuthoredMotion.Recovery)) if (frame != null) forbidden.Add(frame);
            }
            foreach (WeaponDefinition weapon in catalog.Weapons) { if (weapon.Sprite != null) forbidden.Add(weapon.Sprite); if (weapon.ProjectileSprite != null) forbidden.Add(weapon.ProjectileSprite); }
            var rows = new List<string> { "item_id,name,reason,sprite,asset,description" };
            var used = new HashSet<Sprite>();
            foreach (ItemDefinition item in catalog.Items)
            {
                string path = AssetDatabase.GetAssetPath(item.Icon);
                bool oldCharacter = path.IndexOf("Characters", System.StringComparison.OrdinalIgnoreCase) >= 0 || catalog.Companions.Concat(catalog.Champions).Any(unit => item.Icon != null && unit.Id == item.Icon.name);
                string reason = item.Icon == null ? "missing" : forbidden.Contains(item.Icon) || oldCharacter ? "unit-or-weapon" : !used.Add(item.Icon) ? "duplicate-relic" : "unique";
                if (item.Icon != null) used.Add(item.Icon);
                rows.Add(string.Join(",", new[] { item.Id, item.Name, reason, item.Icon == null ? "" : item.Icon.name, AssetDatabase.GetAssetPath(item.Icon), item.Description }.Select(value => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"")));
            }
            Directory.CreateDirectory("PreviewExports/K19"); File.WriteAllLines("PreviewExports/K19/Asset_Role_Audit.csv", rows);
            Debug.Log("ASSET_ROLE_AUDIT " + (rows.Count - 1) + " relics; report PreviewExports/K19/Asset_Role_Audit.csv");
        }
    }
}
