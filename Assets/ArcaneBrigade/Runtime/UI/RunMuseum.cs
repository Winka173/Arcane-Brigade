using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunMuseum
    {
        private sealed class Exhibit
        {
            public string Id;
            public string Name;
            public string Detail;
            public Sprite Icon;
            public bool Seen;
        }
        private readonly List<Exhibit> exhibits = new List<Exhibit>();
        private int tab;
        private int selection;
        private Vector2 scroll;
        private Vector2 detailScroll;
        public int Tab => tab;
        public void SelectTab(int value) { int next = Mathf.Clamp(value, 0, 4); if (tab != next) CheckpointAudio.PlayInterfaceId("ui.tab"); tab = next; selection = 0; scroll = detailScroll = Vector2.zero; }
        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.JoystickButton5)) SelectTab((tab + 1) % 5);
            if (Input.GetKeyDown(KeyCode.JoystickButton4)) SelectTab((tab + 4) % 5);
            int move = Input.GetKeyDown(KeyCode.DownArrow) ? 3 : Input.GetKeyDown(KeyCode.UpArrow) ? -3 : Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton3) ? 1 : Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton2) ? -1 : 0;
            selection = Mathf.Clamp(selection + move, 0, Mathf.Max(0, exhibits.Count - 1));
            if (move != 0) { scroll.y = Mathf.Max(0, selection / 3 * 130 - 260); detailScroll = Vector2.zero; }
        }
        public string SelectedWeapon => tab == 0 && exhibits.Count > selection && exhibits[selection].Seen ? exhibits[selection].Id : null;
        public void Draw(RunCatalog catalog, PlayerProfile profile)
        {
            exhibits.Clear();
            if (tab == 0)
                foreach (WeaponDefinition weapon in catalog.Weapons)
                {
                    string tiers = "Highest acquired tier: ";
                    for (int tier = 1; tier <= catalog.MaximumTier; tier++) if (profile.SeenTiers.Contains(weapon.Id + "/" + tier)) tiers += tier + " ";
                    exhibits.Add(new Exhibit { Id = weapon.Id, Name = weapon.Id, Icon = weapon.Sprite, Seen = profile.SeenWeapons.Contains(weapon.Id), Detail = tiers + "\n\n" + weapon.Kind + " / " + weapon.Element + "\nBase: " + weapon.Damage.ToString("0.#") + " damage · cycle " + weapon.Cooldown.ToString("0.##") + "s · range " + weapon.Range.ToString("0.#") + "\n\n" + SignatureDescription.Describe(weapon, 1) + "\n\nSandbox can preview other tiers; previews do not count as ownership." });
                }
            if (tab == 1)
                foreach (ItemDefinition item in catalog.Items) exhibits.Add(new Exhibit { Id = item.Id, Name = item.Name, Icon = item.Icon, Seen = profile.SeenItems.Contains(item.Id), Detail = item.Description + "\n\n" + item.Rarity + " · " + item.Stacking + " · max " + item.StackLimit });
            if (tab == 2)
                foreach (UnitDefinition unit in catalog.Companions) exhibits.Add(new Exhibit { Id = unit.Id, Name = unit.DisplayName, Icon = unit.Body, Seen = profile.SeenUnits.Contains(unit.Id), Detail = unit.Description + "\nWeapons roll on purchase, not fixed by body." });
            if (tab == 3)
                foreach (BuildLinkRule link in catalog.BuildLinks)
                    if (link.Id.StartsWith("signature-", System.StringComparison.Ordinal)) exhibits.Add(new Exhibit { Id = link.Id, Name = link.Name, Seen = profile.SeenChains.Contains(link.Id), Detail = link.Description + "\n\nDiscoveries require recorded activations/damage/healing, not merely equipping or viewing a graph." });
            if (tab == 4)
                foreach (UnitDefinition boss in catalog.Bosses) exhibits.Add(new Exhibit { Id = boss.Id, Name = boss.DisplayName, Icon = boss.Body, Seen = profile.SeenBosses.Contains(boss.Id), Detail = boss.Description + "\n" + boss.Motif + " / " + boss.Material + "\nActually defeated, not previewed." });
            string[] tabs = { "WEAPONS", "RELICS", "COMPANIONS", "CHAINS", "BOSS" };
            for (int index = 0; index < tabs.Length; index++)
                if (GUI.Button(new Rect(60 + index * 233, 160, 218, 45), (index == tab ? "• " : "") + tabs[index])) { SelectTab(index); exhibits.Clear(); return; }
            GUIStyle title = ArcadeTypography.Style(18, true);
            GUIStyle body = ArcadeTypography.Style(16);
            scroll = GUI.BeginScrollView(new Rect(60, 225, 745, 385), scroll, new Rect(0, 0, 710, Mathf.Max(380, Mathf.CeilToInt(exhibits.Count / 3f) * 130)));
            for (int index = 0; index < exhibits.Count; index++)
            {
                Exhibit exhibit = exhibits[index];
                Rect card = new Rect(index % 3 * 235, index / 3 * 130, 223, 118);
                ArcadeSkin.Panel(card, index == selection ? new Color(0.27f, 0.22f, 0.36f) : new Color(0.15f, 0.13f, 0.23f));
                if (GUI.Button(card, GUIContent.none, GUIStyle.none)) { if (!exhibit.Seen || selection != index) CheckpointAudio.PlayInterfaceId(exhibit.Seen ? "ui.page" : "ui.locked"); selection = index; detailScroll = Vector2.zero; }
                if (exhibit.Seen) RunHud.Sprite(new Rect(card.x + 72, card.y + 10, 80, 65), exhibit.Icon);
                GUI.Label(new Rect(card.x + 12, card.y + 80, 200, 35), exhibit.Seen ? exhibit.Name : "?  UNDISCOVERED", body);
            }
            GUI.EndScrollView();
            selection = Mathf.Clamp(selection, 0, Mathf.Max(0, exhibits.Count - 1));
            ArcadeSkin.Panel(new Rect(840, 225, 375, 385), new Color(0.18f, 0.15f, 0.27f));
            if (exhibits.Count > 0)
            {
                Exhibit exhibit = exhibits[selection];
                if (exhibit.Seen) RunHud.Sprite(new Rect(952, 245, 150, 110), exhibit.Icon);
                GUI.Label(new Rect(865, 365, 320, 65), exhibit.Seen ? exhibit.Name : "Empty shelf", title);
                string description = exhibit.Seen ? exhibit.Detail : tab == 4 ? "Defeat this boss in a real hall to unlock its display." : tab == 3 ? "Activate the chain in actual combat. Owning its set is not enough." : "Find this item during a run. Viewing or testing does not unlock it.";
                float height = Mathf.Max(135, body.CalcHeight(new GUIContent(description), 290));
                detailScroll = GUI.BeginScrollView(new Rect(865, 430, 325, 155), detailScroll, new Rect(0, 0, 295, height));
                GUI.Label(new Rect(0, 0, 290, height), description, body);
                GUI.EndScrollView();
            }
            int found = exhibits.FindAll(exhibit => exhibit.Seen).Count;
            GUI.Label(new Rect(65, 630, 730, 40), found + " / " + exhibits.Count + " · Tab / LB/RB: category · arrows / X/Y: sample · Enter/A: try", body);
        }
    }
}
