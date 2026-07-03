using System.Linq;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Whose head a bought relic's lines rise over (batch: -executeMethod ArcaneBrigade.Editor.StatFeedbackCheck.Run):
    /// a hero-only relic over the hero alone, a ranged-damage relic over the ranged friends (and a ranged hero) alone.</summary>
    public static class StatFeedbackCheck
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var log = new StringBuilder();
            UnitDefinition ranged = catalog.Companions.First(u => u.Weapon != null && u.Weapon.Kind == WeaponKind.Projectile);
            UnitDefinition melee = catalog.Companions.First(u => u.Weapon != null && u.Weapon.Kind != WeaponKind.Projectile);
            foreach (ItemEffect effect in new[] { ItemEffect.ChampionDamage, ItemEffect.RangedDamage, ItemEffect.MeleeDamage, ItemEffect.CompanionDamage, ItemEffect.Damage })
            {
                ItemDefinition item = catalog.Items.FirstOrDefault(i => i.Modifiers != null && i.Modifiers.Length == 1 && i.Modifiers[0].Effect == effect);
                if (item == null) { log.AppendLine("STATCHECK " + effect + ": no single-stat relic"); continue; }
                var session = new RunSession(catalog);
                session.Start("LanternKnight", 4242); session.State.Phase = SessionPhase.Shop; session.State.Roster.Clear();
                session.State.Roster.Add(new RosterEntry { UnitId = ranged.Id, WeaponId = ranged.Weapon.Id, Tier = 1 });
                session.State.Roster.Add(new RosterEntry { UnitId = melee.Id, WeaponId = melee.Weapon.Id, Tier = 1 });
                ShopStatSnapshot before = ShopStatSnapshot.Capture(session, catalog, session.PrepareCombat());
                session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                ShopStatSnapshot after = ShopStatSnapshot.Capture(session, catalog, session.PrepareCombat());
                var lines = ShopStatSnapshot.Compare(before, after).Where(d => !d.Label.StartsWith("Relic / ") && d.Label != "Gold");
                string who(string key) => key == "hero" ? "HERO(" + catalog.Unit(session.State.ChampionId).Weapon.Kind + ")" : key.StartsWith(ranged.Id) ? "RANGED" : key.StartsWith(melee.Id) ? "MELEE" : key;
                log.AppendLine("STATCHECK " + effect + " (" + item.Name + ", scope " + item.Modifiers[0].Scope + "): "
                    + string.Join(" ; ", lines.Select(d => who(d.ActorKey) + " " + d.Label + " " + d.Display)));
                session.Dispose();
            }
            Debug.Log(log.ToString());
        }
    }
}
