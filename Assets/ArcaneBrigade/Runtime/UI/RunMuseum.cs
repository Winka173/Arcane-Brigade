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
            public UnitDefinition Unit;
        }
        private readonly List<Exhibit> exhibits = new List<Exhibit>();
        private int tab;
        private int selection;
        private Vector2 scroll;
        private Vector2 detailScroll;
        private readonly MuseumDiorama diorama = new MuseumDiorama();
        private int previewTier = 1;
        private float touchedAt = -100;
        public void Close() => diorama.Dispose();
        public void SetPreview(int stars, int pose) { previewTier = Mathf.Clamp(stars, 1, 3); diorama.Pose = Mathf.Clamp(pose, 0, 3); }
        public void SelectExhibitForDiagnostics(int index)
        {
            if (!RunPaths.K7Capture) throw new System.InvalidOperationException("Exhibit fixture requires isolated capture mode");
            selection = Mathf.Max(0, index); scroll.y = Mathf.Max(0, selection / 3 * 130 - 260); detailScroll = Vector2.zero;
        }
        public int Tab => tab;
        public void SelectTab(int value) { int next = Mathf.Clamp(value, 0, 4); if (tab != next) CheckpointAudio.PlayInterfaceId("ui.tab"); tab = next; selection = 0; scroll = detailScroll = Vector2.zero; }
        public void HandleInput()
        {
            if ((tab == 2 || tab == 4) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))) diorama.Pose = (diorama.Pose + 1) % 4;
            if (tab == 2 && Input.GetKeyDown(KeyCode.T)) previewTier = previewTier % 3 + 1;
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
                foreach (UnitDefinition unit in catalog.Companions) exhibits.Add(new Exhibit { Id = unit.Id, Name = unit.DisplayName, Icon = unit.Body, Unit = unit, Seen = profile.SeenUnits.Contains(unit.Id), Detail = unit.Description + "\nWeapons roll on purchase, not fixed by body." });
            if (tab == 3)
                foreach (BuildLinkRule link in catalog.BuildLinks)
                    if (link.Id.StartsWith("signature-", System.StringComparison.Ordinal)) exhibits.Add(new Exhibit { Id = link.Id, Name = link.Name, Seen = profile.SeenChains.Contains(link.Id), Detail = link.Description + "\n\nDiscoveries require recorded activations/damage/healing, not merely equipping or viewing a graph." });
            if (tab == 4)
                foreach (UnitDefinition boss in catalog.Bosses) exhibits.Add(new Exhibit { Id = boss.Id, Name = boss.DisplayName, Icon = boss.Body, Unit = boss, Seen = profile.SeenBosses.Contains(boss.Id), Detail = boss.Description + "\n" + boss.Motif + " / " + boss.Material + "\nActually defeated, not previewed." });
            string[] tabs = { "WEAPONS", "RELICS", "COMPANIONS", "CHAINS", "BOSS" };
            for (int index = 0; index < tabs.Length; index++)
                if (GUI.Button(new Rect(60 + index * 233, 160, 218, 45), (index == tab ? "• " : "") + tabs[index])) { SelectTab(index); exhibits.Clear(); return; }
            GUIStyle title = ArcadeTypography.Style(18, true);
            GUIStyle body = ArcadeTypography.Style(16);
            GUIStyle cardText = ArcadeTypography.Style(14);
            cardText.wordWrap = true; cardText.clipping = TextClipping.Clip;
            scroll = GUI.BeginScrollView(new Rect(60, 225, 745, 385), scroll, new Rect(0, 0, 710, Mathf.Max(380, Mathf.CeilToInt(exhibits.Count / 3f) * 130)));
            for (int index = 0; index < exhibits.Count; index++)
            {
                Exhibit exhibit = exhibits[index];
                Rect card = new Rect(index % 3 * 235, index / 3 * 130, 223, 118);
                ArcadeSkin.Panel(card, index == selection ? new Color(0.27f, 0.22f, 0.36f) : new Color(0.15f, 0.13f, 0.23f));
                if (GUI.Button(card, GUIContent.none, GUIStyle.none)) { if (!exhibit.Seen || selection != index) CheckpointAudio.PlayInterfaceId(exhibit.Seen ? "ui.page" : "ui.locked"); selection = index; detailScroll = Vector2.zero; }
                if (exhibit.Seen) RunHud.Sprite(new Rect(card.x + 72, card.y + 10, 80, 65), exhibit.Icon);
                GUI.Label(new Rect(card.x + 12, card.y + 78, 200, 38), exhibit.Seen ? exhibit.Name.Replace("Contract · ", "") : "?  UNDISCOVERED", cardText);
            }
            GUI.EndScrollView();
            selection = Mathf.Clamp(selection, 0, Mathf.Max(0, exhibits.Count - 1));
            ArcadeSkin.Panel(new Rect(840, 225, 375, 385), new Color(0.18f, 0.15f, 0.27f));
            if (exhibits.Count > 0)
            {
                Exhibit exhibit = exhibits[selection];
                if (exhibit.Seen)
                {
                    float time = Time.unscaledTime;
                    if (exhibit.Unit != null)
                    {
                        diorama.Draw(new Rect(850, 230, 355, 125), exhibit.Unit, catalog.Arena, exhibit.Unit.IsBoss ? 1 : previewTier);
                        string[] poses = { "IDLE", "WALK", "ATTACK", "HIT" };
                        for (int pose = 0; pose < poses.Length; pose++)
                            if (GUI.Button(new Rect(852 + pose * 88, 350, 84, 25), (diorama.Pose == pose ? "> " : "") + poses[pose])) diorama.Pose = pose;
                        if (!exhibit.Unit.IsBoss)
                            for (int stars = 1; stars <= 3; stars++)
                                if (GUI.Button(new Rect(866 + (stars - 1) * 109, 380, 101, 24), stars + " STAR" + (stars > 1 ? "S" : ""))) previewTier = stars;
                    }
                    else
                    {
                        diorama.Dispose();
                        float bob = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(time * 2.4f) * 4;
                        Rect icon = new Rect(952, 245 + bob, 150, 110);
                        if (GUI.Button(icon, GUIContent.none, GUIStyle.none)) { touchedAt = time; CheckpointAudio.PlayInterfaceId("ui.page"); }
                        float pop = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(Mathf.Clamp01((time - touchedAt) / 0.4f) * Mathf.PI) * 12;
                        icon = new Rect(icon.x - pop * 0.5f, icon.y - pop * 0.5f, icon.width + pop, icon.height + pop);
                        Matrix4x4 previous = GUI.matrix;
                        if (!PresentationSettings.ReducedMotion)
                        {
                            float breathe = Mathf.Sin(time * 1.7f) * 2;
                            icon = new Rect(icon.x - breathe, icon.y + breathe, icon.width + breathe * 2, icon.height - breathe * 2);
                        }
                        if (exhibit.Icon != null) RunHud.Sprite(icon, exhibit.Icon);
                        else
                            for (int link = 0; link < 3; link++)
                            {
                                float lift = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(time * 3 - link * 0.8f) * 8;
                                RunHud.Sprite(new Rect(icon.x + link * 52, icon.y + 34 + lift, 38, 38), K8Motion.TierStar());
                            }
                        GUI.matrix = previous;
                    }
                }
                else diorama.Dispose();
                GUI.Label(new Rect(865, exhibit.Unit != null && exhibit.Seen ? 406 : 365, 320, 30), exhibit.Seen ? exhibit.Name : "Empty shelf", title);
                string description = exhibit.Seen ? exhibit.Detail : tab == 4 ? "Defeat this boss in a real hall to unlock its display." : tab == 3 ? "Activate the chain in actual combat. Owning its set is not enough." : "Find this item during a run. Viewing or testing does not unlock it.";
                if (exhibit.Seen && exhibit.Unit != null && exhibit.Unit.IsBoss) description += "\n\n" + BossPartRules.Description;
                float height = Mathf.Max(135, body.CalcHeight(new GUIContent(description), 290));
                detailScroll = GUI.BeginScrollView(new Rect(865, 430, 325, 155), detailScroll, new Rect(0, 0, 295, height));
                GUI.Label(new Rect(0, 0, 290, height), description, body);
                GUI.EndScrollView();
            }
            int found = exhibits.FindAll(exhibit => exhibit.Seen).Count;
            GUI.Label(new Rect(65, 630, 730, 40), found + " / " + exhibits.Count + (tab == 2 || tab == 4 ? " · Arrows: sample · Enter/A: pose · T: tier" : " · Tab / LB/RB: category · arrows / X/Y: sample · Enter/A: try"), body);
        }
    }
}
