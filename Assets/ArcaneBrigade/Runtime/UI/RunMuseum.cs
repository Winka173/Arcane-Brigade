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
        private int poseCount = 4;
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
            if ((tab == 2 || tab == 4) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))) diorama.Pose = (diorama.Pose + 1) % poseCount;
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
                foreach (ItemDefinition item in catalog.Items) exhibits.Add(new Exhibit { Id = item.Id, Name = item.Name, Icon = item.Icon, Seen = profile.SeenItems.Contains(item.Id), Detail = RelicDescriptions.Describe(item,1,catalog) });
            if (tab == 2)
                foreach (UnitDefinition unit in catalog.Companions) exhibits.Add(new Exhibit { Id = unit.Id, Name = unit.DisplayName, Icon = unit.Body, Unit = unit, Seen = profile.SeenUnits.Contains(unit.Id), Detail = CompanionDescriptions.Describe(unit,1,catalog.TierSupportMultiplier) });
            if (tab == 3)
                foreach (BuildLinkRule link in catalog.BuildLinks)
                    if (link.Id.StartsWith("signature-", System.StringComparison.Ordinal)) exhibits.Add(new Exhibit { Id = link.Id, Name = link.Name, Seen = profile.SeenChains.Contains(link.Id), Detail = link.Description + "\n\nDiscoveries require recorded activations/damage/healing, not merely equipping or viewing a graph." });
            if (tab == 4)
                foreach (UnitDefinition boss in catalog.Bosses) exhibits.Add(new Exhibit { Id = boss.Id, Name = boss.DisplayName, Icon = boss.Body, Unit = boss, Seen = profile.SeenBosses.Contains(boss.Id), Detail = boss.Description + "\n" + boss.Motif + " / " + boss.Material + "\nActually defeated, not previewed." });
            GUIStyle controls = new GUIStyle(GUI.skin.button) { font = ArcadeTypography.Heading, fontSize = 13, wordWrap = false, clipping = TextClipping.Clip, alignment = TextAnchor.MiddleCenter, padding = new RectOffset(10, 10, 5, 5) };
            GUIStyle poseControls = new GUIStyle(controls) { fontSize = 11, padding = new RectOffset(6, 6, 4, 4) };
            string[] tabs = { "WEAPONS", "RELICS", "COMPANIONS", "CHAINS", "BOSS" };
            for (int index = 0; index < tabs.Length; index++)
                if (GUI.Button(new Rect(60 + index * 233, 160, 218, 45), (index == tab ? "• " : "") + tabs[index], controls)) { SelectTab(index); exhibits.Clear(); return; }
            GUIStyle title = ArcadeTypography.Style(17, true);
            GUIStyle body = ArcadeTypography.Style(14);
            GUIStyle cardText = ArcadeTypography.Style(13);
            title.padding = body.padding = cardText.padding = new RectOffset();
            title.margin = body.margin = cardText.margin = new RectOffset();
            cardText.wordWrap = true; cardText.clipping = TextClipping.Clip;
            scroll = GUI.BeginScrollView(new Rect(60, 225, 745, 385), scroll, new Rect(0, 0, 710, Mathf.Max(380, Mathf.CeilToInt(exhibits.Count / 3f) * 130)));
            for (int index = 0; index < exhibits.Count; index++)
            {
                Exhibit exhibit = exhibits[index];
                Rect card = new Rect(index % 3 * 235, index / 3 * 130, 223, 118);
                ArcadeSkin.Panel(card, index == selection ? ArcadeSkin.CardLit : ArcadeSkin.Card);
                if (GUI.Button(card, GUIContent.none, GUIStyle.none)) { if (!exhibit.Seen || selection != index) CheckpointAudio.PlayInterfaceId(exhibit.Seen ? "ui.page" : "ui.locked"); selection = index; detailScroll = Vector2.zero; }
                if (exhibit.Seen) RunHud.Sprite(new Rect(card.x + 72, card.y + 10, 80, 65), exhibit.Icon);
                string caption = exhibit.Seen ? exhibit.Name.Replace("Contract · ", "") : "UNDISCOVERED";
                Rect nameBounds = new Rect(card.x + 16, card.y + 77, card.width - 32, 31);
                var nameStyle = new GUIStyle(cardText);
                while (nameStyle.fontSize > 10 && nameStyle.CalcHeight(new GUIContent(caption), nameBounds.width) > nameBounds.height) nameStyle.fontSize--;
                GUI.Label(nameBounds, caption, nameStyle);
            }
            GUI.EndScrollView();
            selection = Mathf.Clamp(selection, 0, Mathf.Max(0, exhibits.Count - 1));
            ArcadeSkin.Panel(new Rect(840, 225, 375, 385), ArcadeSkin.Card);
            if (exhibits.Count > 0)
            {
                Exhibit exhibit = exhibits[selection];
                if (exhibit.Seen)
                {
                    float time = Time.unscaledTime;
                    if (exhibit.Unit != null)
                    {
                        string[] poses = exhibit.Unit.IsBoss ? new[] { "IDLE", "WALK", "ATTACK", "HIT", "PHASE" } : new[] { "IDLE", "WALK", "ATTACK", "HIT" };
                        if (!exhibit.Unit.IsBoss && CharacterActingLibrary.Current?.Find(exhibit.Unit.Id) != null) poses = new[] { "IDLE", "WALK", "ATTACK", "HIT", "LOOK", "CHEER", "FLINCH", "TIRED" };
                        poseCount = poses.Length;
                        if (diorama.Pose >= poseCount) diorama.Pose = 0;
                        diorama.Draw(new Rect(860, 239, 335, 112), exhibit.Unit, catalog.Arena, exhibit.Unit.IsBoss ? 1 : previewTier);
                        for (int pose = 0; pose < poses.Length; pose++)
                        {
                            Color previousColor = GUI.color;
                            if (diorama.Pose == pose) GUI.color = new Color(1, 0.78f, 0.55f);
                            float poseWidth = 335f / Mathf.Min(5, poses.Length);
                            float poseY = poses.Length > 5 ? 354 + pose / 4 * 30 : 354;
                            if (poses.Length > 5) poseWidth = 335f / 4;
                            if (GUI.Button(new Rect(860 + (poses.Length > 5 ? pose % 4 : pose) * poseWidth, poseY, poseWidth - 5, 26), poses[pose], poseControls)) diorama.Pose = pose;
                            GUI.color = previousColor;
                        }
                        if (!exhibit.Unit.IsBoss)
                            for (int stars = 1; stars <= 3; stars++)
                                if (GUI.Button(new Rect(860 + (stars - 1) * 114, poses.Length > 5 ? 415 : 388, 107, 24), stars + " STAR" + (stars > 1 ? "S" : ""), controls)) previewTier = stars;
                        if (exhibit.Unit.IsBoss)
                        {
                            BossAnatomyEntry anatomy = BossAnatomy.For(exhibit.Unit);
                            if (anatomy != null)
                                for (int part = 0; part < 3; part++)
                                {
                                    bool broken = (diorama.BrokenMask & 1 << part) != 0;
                                    var label = new GUIContent((broken ? "FIX " : "BREAK ") + (part + 1), anatomy.Organs[part].Name + ": " + anatomy.Organs[part].BreakSummary);
                                    if (GUI.Button(new Rect(860 + part * 114, 388, 107, 28), label, controls)) diorama.BrokenMask ^= 1 << part;
                                }
                        }
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
                float titleTop = exhibit.Unit != null && exhibit.Seen ? poseCount > 5 ? 450 : 428 : 365;
                string heading = exhibit.Seen ? exhibit.Name : "Empty shelf";
                float titleHeight = Mathf.Clamp(title.CalcHeight(new GUIContent(heading), 323), 24, 52);
                GUI.Label(new Rect(866, titleTop, 323, titleHeight), heading, title);
                string description = exhibit.Seen ? exhibit.Detail : tab == 4 ? "Defeat this boss in a real hall to unlock its display." : tab == 3 ? "Activate the chain in actual combat. Owning its set is not enough." : "Find this item during a run. Viewing or testing does not unlock it.";
                if (exhibit.Seen && exhibit.Unit != null && exhibit.Unit.IsBoss) description += "\n\n" + BossPartRules.Describe(exhibit.Unit);
                Rect details = DescriptionBounds(titleTop, titleHeight);
                float textWidth = details.width - 22;
                float height = Mathf.Max(details.height, body.CalcHeight(new GUIContent(description), textWidth) + 12);
                detailScroll = GUI.BeginScrollView(details, detailScroll, new Rect(0, 0, textWidth, height));
                GUI.Label(new Rect(0, 4, textWidth, height - 8), description, body);
                GUI.EndScrollView();
            }
            int found = exhibits.FindAll(exhibit => exhibit.Seen).Count;
            GUI.Label(new Rect(65, 630, 730, 40), found + " / " + exhibits.Count + (tab == 2 || tab == 4 ? " · Arrows: sample · Enter/A: pose · T: tier" : " · Tab / LB/RB: category · arrows / X/Y: sample · Enter/A: try"), body);
        }
        public static Rect DescriptionBounds(float titleTop, float titleHeight)
        {
            float top = titleTop + titleHeight + 12;
            return new Rect(866, top, 323, Mathf.Max(64, 588 - top));
        }
    }
}
