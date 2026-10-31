using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Waiting Room's painted pieces (Resources/Hub, Resources/UI/Meta; cut by scratchpad hub_cut.py).</summary>
    public static class MetaIcons
    {
        private static Sprite oil, keepsake, room;
        private static readonly Dictionary<string, Sprite> stations = new Dictionary<string, Sprite>();
        public static Sprite LampOil => oil != null ? oil : oil = Resources.Load<Sprite>("UI/Meta/LampOil");
        public static Sprite Keepsake => keepsake != null ? keepsake : keepsake = Resources.Load<Sprite>("UI/Meta/Keepsake");
        public static Sprite Currency(MetaCurrency currency) => currency == MetaCurrency.LampOil ? LampOil : Keepsake;
        public static Sprite Room => room != null ? room : room = Resources.Load<Sprite>("Hub/WaitingRoom");
        public static Sprite Station(MetaStation station, bool restored)
        {
            string key = station + (restored ? "_Lit" : "_Dusty");
            if (!stations.TryGetValue(key, out Sprite s)) stations[key] = s = Resources.Load<Sprite>("Hub/Stations/" + key);
            return s;
        }
    }

    public enum HubAction { None, NextNight, Back, Daily }

    /// <summary>The Waiting Room hub between runs (play-test item 27): a room the traveller walks in (keys, stick, or a tap on the
    /// floor), ten station objects that are restored as they level, the door to the next night. Tapping a working station
    /// opens its cards (icon, one line, now → next, price); the five not built yet stand dusty and say so. Drawn in the menu's
    /// 1280×720 space.</summary>
    public sealed partial class MetaHub
    {
        public sealed class Spot { public MetaStation Station; public Vector2 Foot; public float Height; }
        /// <summary>Where each station stands on the painted floor (its foot point) and how tall it is drawn.</summary>
        public static readonly Spot[] Spots =
        {
            // the Lamp Tree (the Lantern) stands where the Ledger, Clock and Nursery stood before they became its branches (24/10/2026)
            new Spot { Station = MetaStation.Lantern, Foot = new Vector2(392, 226), Height = 190 },
            new Spot { Station = MetaStation.Wardrobe, Foot = new Vector2(345, 668), Height = 150 },
            new Spot { Station = MetaStation.Postbox, Foot = new Vector2(925, 668), Height = 128 },
            new Spot { Station = MetaStation.Museum, Foot = new Vector2(196, 300), Height = 112 },
            new Spot { Station = MetaStation.NightPact, Foot = new Vector2(1086, 300), Height = 104 },
            new Spot { Station = MetaStation.Window, Foot = new Vector2(150, 640), Height = 108 },
            new Spot { Station = MetaStation.Bell, Foot = new Vector2(1130, 640), Height = 84 },
        };
        /// <summary>Things that live in the room without being stations (a tea table with steam, a sleeping cat): they only fill the empty carpet.</summary>
        public sealed class Decor { public string Name; public Vector2 Foot; public float Height; public Sprite Sprite; public bool Tried; }
        public static readonly Decor[] Decorations =
        {
            new Decor { Name = "TeaTable", Foot = new Vector2(500, 470), Height = 120 },
            new Decor { Name = "CatCushion", Foot = new Vector2(820, 452), Height = 78 },
        };
        public static readonly Rect DoorArea = new Rect(582, 4, 116, 128);
        public static readonly Vector2 DoorFoot = new Vector2(640, 168);
        public static readonly Rect Floor = new Rect(60, 160, 1160, 545);
        public const float Speed = 330, Reach = 82;

        private Vector2 hero = new Vector2(640, 560);
        private Vector2? target;
        private int targetSpot = -1;
        private bool targetDoor, walking, facingLeft;
        private Vector2 scroll;
        private int wardrobeHero = -1;
        private string notice = ""; private float noticeAt = -10;
        private float boughtAt = -10; private string bought;
        private float openedAt = -10, focusAt = -10; private int focusShown = -2;
        private GUIStyle heading, body, small, number, chip;

        public MetaStation? Open { get; private set; }
        public bool PanelOpen => Open.HasValue;
        public int Focus { get; private set; } = -1;
        public bool DoorFocus { get; private set; }
        public Vector2 Hero => hero;

        public void Enter() { hero = new Vector2(640, 560); target = null; targetSpot = -1; targetDoor = false; Open = null; }
        public void OpenStation(MetaStation station)
        {
            if (!MetaProgression.Works(station)) { Say("Coming soon"); CheckpointAudio.PlayInterfaceId("ui.locked"); return; }
            Open = station; openedAt = Time.unscaledTime; closingAt = -1; scroll = Vector2.zero; navIndex = 0; navKeys = false; bellArmed = false; respecArmed = false; CheckpointAudio.PlayInterfaceId("ui.open");
        }
        /// <summary>Closes the open station with motion (requirement 25/10/2026): the panel shrinks and fades, then it is gone.</summary>
        public void Close() { if (!Open.HasValue || closingAt >= 0) return; CheckpointAudio.PlayInterfaceId("ui.back"); closingAt = Time.unscaledTime; }
        private float closingAt = -1;
        /// <summary>Diagnostics: stand the traveller beside a station.</summary>
        public void Approach(MetaStation station) { Spot s = Array.Find(Spots, x => x.Station == station); hero = s.Foot + new Vector2(s.Foot.x < 640 ? 70 : -70, 30); target = null; UpdateFocus(); }
        private void Say(string line) { notice = line; noticeAt = Time.unscaledTime; }

        private static Rect Bounds(Spot s, Sprite sprite)
        {
            float aspect = sprite != null ? sprite.rect.width / sprite.rect.height : .8f;
            float w = s.Height * aspect;
            return new Rect(s.Foot.x - w * .5f, s.Foot.y - s.Height, w, s.Height);
        }

        private void UpdateFocus()
        {
            Focus = -1; float best = Reach;
            for (int i = 0; i < Spots.Length; i++)
            {
                float d = Vector2.Distance(hero, Spots[i].Foot + new Vector2(0, 30));
                if (d < best) { best = d; Focus = i; }
            }
            DoorFocus = Vector2.Distance(hero, DoorFoot) < Reach + 10;
        }

        /// <summary>Once a frame from the menu's Update: walking (keys, stick, a tapped target) and using what is near.</summary>
        public HubAction Tick(float dt)
        {
            if (PanelOpen) { PanelInput(); return HubAction.None; }
            Vector2 pad = ArcadeNavigation.Pad;
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal") + pad.x, -(Input.GetAxisRaw("Vertical") + pad.y));
            walking = false;
            if (input.sqrMagnitude > .04f) { target = null; targetSpot = -1; targetDoor = false; Move(Vector2.ClampMagnitude(input, 1) * Speed * dt); }
            else if (target.HasValue)
            {
                Vector2 to = target.Value - hero;
                if (to.magnitude <= Speed * dt + .5f)
                {
                    hero = target.Value; target = null;
                    if (targetDoor) { targetDoor = false; return HubAction.NextNight; }
                    if (targetSpot >= 0) { int s = targetSpot; targetSpot = -1; OpenStation(Spots[s].Station); }
                }
                else Move(to.normalized * Speed * dt);
            }
            UpdateFocus();
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                if (DoorFocus) return HubAction.NextNight;
                if (Focus >= 0) { OpenStation(Spots[Focus].Station); navKeys = PanelOpen; } // opened by keys: the focus shows at once
            }
            return HubAction.None;
        }
        private void Move(Vector2 delta)
        {
            if (delta.sqrMagnitude < 1e-6f) return;
            walking = true; if (Mathf.Abs(delta.x) > .01f) facingLeft = delta.x < 0;
            Vector2 next = hero + delta;
            next.x = Mathf.Clamp(next.x, Floor.xMin, Floor.xMax); next.y = Mathf.Clamp(next.y, Floor.yMin, Floor.yMax);
            hero = next;
        }
        private void WalkTo(Vector2 point, int spot, bool door)
        {
            target = new Vector2(Mathf.Clamp(point.x, Floor.xMin, Floor.xMax), Mathf.Clamp(point.y, Floor.yMin, Floor.yMax));
            targetSpot = spot; targetDoor = door;
            CheckpointAudio.PlayInterfaceId("ui.select");
        }

        private void Styles()
        {
            if (heading != null) return;
            heading = ArcadeTypography.Style(26, true); heading.alignment = TextAnchor.MiddleCenter; heading.wordWrap = false;
            body = ArcadeTypography.Style(19, true); body.wordWrap = false; body.clipping = TextClipping.Clip;
            small = ArcadeTypography.Style(15); small.wordWrap = true;
            number = ArcadeTypography.Style(20, false, true); number.wordWrap = false;
            chip = ArcadeTypography.Style(16, true); chip.alignment = TextAnchor.MiddleCenter; chip.wordWrap = false;
        }

        /// <summary>Draws the room and, when open, a station's cards. Returns what the traveller chose (next night / back).</summary>
        public HubAction Draw(RunCatalog catalog, PlayerProfile profile, int heroIndex, Func<bool> save)
        {
            Styles();
            MetaProgression.Ensure(profile);
            HubAction action = HubAction.None;
            bool reduced = profile.ReducedMotion;
            float t = Time.unscaledTime, breathe = reduced ? .5f : .5f + .5f * Mathf.Sin(t * 1.3f);
            Sprite room = MetaIcons.Room;
            if (room != null && Event.current.type == EventType.Repaint)
            {
                // wider or taller screens: the room itself, enlarged and darkened, fills the space round the 16:9 frame
                Rect beyond = new Rect(-1280, -720, 3840, 2160);
                GUI.DrawTexture(beyond, room.texture, ScaleMode.ScaleAndCrop);
                GUI.DrawTexture(beyond, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, new Color(0.02f, 0.03f, 0.055f, 0.62f), 0, 0);
            }
            if (room != null) { if (Event.current.type == EventType.Repaint) GUI.DrawTexture(new Rect(0, 0, 1280, 720), room.texture, ScaleMode.ScaleAndCrop); }
            else ArcadeSkin.MenuBackdrop(new Rect(0, 0, 1280, 720));
            // the door breathes the next night's light
            ArcadeSkin.Glow(new Rect(DoorArea.x - 70, DoorArea.y - 30, DoorArea.width + 140, DoorArea.height + 110), new Color(1f, .82f, .55f, (DoorFocus ? .30f : .10f) + .06f * breathe));

            // stations and the traveller, back to front
            var order = new List<(float y, int index)>();
            for (int i = 0; i < Spots.Length; i++) order.Add((Spots[i].Foot.y, i));
            order.Add((hero.y, -1));
            // the room is lived in: friends the traveller has met wander the carpet, and warm motes drift up through the lamp light
            List<(UnitDefinition unit, Vector2 at, bool left)> residents = Residents(catalog, profile, t, reduced);
            for (int i = 0; i < residents.Count; i++) order.Add((residents[i].at.y, -2 - i));
            for (int i = 0; i < Decorations.Length; i++)
            {
                Decor d = Decorations[i];
                if (!d.Tried) { d.Tried = true; d.Sprite = Resources.Load<Sprite>("Hub/Props/" + d.Name); }
                if (d.Sprite != null) order.Add((d.Foot.y, -100 - i));
            }
            if (!reduced && Event.current.type == EventType.Repaint)
                for (int i = 0; i < 16; i++)
                {
                    float q = Mathf.Repeat(t * (.05f + .02f * (i % 4)) + i * .137f, 1), x = 120 + (i * 331 % 1040) + Mathf.Sin(t * .6f + i) * 24, y = 700 - q * 560;
                    ArcadeSkin.Glow(new Rect(x - 12, y - 12, 24, 24), new Color(1f, .82f, .5f, .5f * Mathf.Sin(q * Mathf.PI) * (.6f + .4f * Mathf.Sin(t * 3 + i * 2))));
                }
            order.Sort((a, b) => a.y.CompareTo(b.y));
            UnitDefinition traveller = catalog.Champions[Mathf.Clamp(heroIndex, 0, catalog.Champions.Length - 1)];
            foreach (var (_, index) in order)
            {
                if (index == -1) { DrawTraveller(traveller, heroIndex, reduced); continue; }
                if (index <= -100) { DrawDecor(Decorations[-100 - index], t, reduced); continue; }
                if (index < -1) { DrawResident(residents[-2 - index], reduced); continue; }
                Spot spot = Spots[index];
                bool works = MetaProgression.Works(spot.Station), restored = MetaProgression.Restored(profile, spot.Station);
                Sprite sprite = MetaIcons.Station(spot.Station, restored);
                Rect r = Bounds(spot, sprite);
                if (restored) ArcadeSkin.Glow(new Rect(r.x - 40, r.y - 30, r.width + 80, r.height + 60), new Color(1f, .78f, .45f, .16f + .06f * breathe));
                if (index == Focus && !PanelOpen) ArcadeSkin.Glow(new Rect(r.x - 30, r.y - 20, r.width + 60, r.height + 40), new Color(1f, .85f, .55f, works ? .38f : .18f));
                // grounded (requirement 24/10/2026: the stations looked to float): a soft cast shadow falling down-right from the room's
                // lamp light, and a dark contact shadow right under the feet
                ArcadeSkin.Glow(new Rect(r.x + r.width * .14f, spot.Foot.y - 12, r.width * .95f, 34), new Color(0, 0, 0, .42f));
                ArcadeSkin.Glow(new Rect(r.x + r.width * .12f, spot.Foot.y - 9, r.width * .76f, 16), new Color(0, 0, 0, .7f));
                Color keep = GUI.color;
                if (!works) GUI.color = new Color(keep.r * .78f, keep.g * .76f, keep.b * .74f, keep.a);
                RunHud.Sprite(r, sprite);
                GUI.color = keep;
            }

            // the Lamp Tree is where the permanent upgrades are bought: it always wears a call-out above it, and a lit badge when the oil can buy something
            if (!PanelOpen)
            {
                Spot lamp = Array.Find(Spots, x => x.Station == MetaStation.Lantern);
                Rect lr = Bounds(lamp, MetaIcons.Station(MetaStation.Lantern, MetaProgression.Restored(profile, MetaStation.Lantern)));
                float bob = reduced ? 0 : Mathf.Sin(t * 2.4f) * 5;
                bool afford = Affordable(profile);
                if (Focus < 0 || Spots[Focus].Station != MetaStation.Lantern) Tag(new Vector2(lamp.Foot.x, lr.y - 52 + bob), afford ? "UPGRADES READY" : "PERMANENT UPGRADES");
                if (afford) { ArcadeSkin.Glow(new Rect(lr.xMax - 54, lr.y - 14 + bob, 46, 46), new Color(1f, .82f, .45f, .6f)); ArcadeSkin.Seal(new Vector2(lr.xMax - 31, lr.y + 9 + bob), 26, "!"); }
            }

            // what is near: a paper tag over it
            if (!PanelOpen && Focus >= 0)
            {
                Spot s = Spots[Focus]; Rect r = Bounds(s, MetaIcons.Station(s.Station, false));
                if (focusShown != Focus) { focusShown = Focus; focusAt = Time.unscaledTime; }
                Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
                LivingUI.Pop(Time.unscaledTime - focusAt, new Vector2(s.Foot.x, r.y - 23));
                Tag(new Vector2(s.Foot.x, r.y - 40), MetaProgression.Works(s.Station) ? MetaProgression.StationName(s.Station).ToUpperInvariant() : "COMING SOON");
                GUI.matrix = keepMatrix; GUI.color = keepColour;
            }
            else focusShown = -2;
            if (!PanelOpen && DoorFocus) Tag(new Vector2(DoorFoot.x, DoorArea.yMax + 4), "MAIN MENU");

            // the room's frame: back, title, purse, the way on
            if (!PanelOpen && ArcadeSkin.Medallion(new Rect(22, 18, 64, 64), KitMedallion.Back)) action = HubAction.Back;
            ArcadeSkin.Frame(new Rect(96, 18, 400, 62), KitFrame.Plaque);
            heading.normal.textColor = ArcadeSkin.KitCream;
            GUI.Label(new Rect(96, 18, 400, 62), "THE WAITING ROOM", heading);
            if (!PanelOpen) FavourStars(profile, new Rect(96, 80, 400, 30)); // an open station's title would cover it by half
            Purse(profile, new Rect(900, 26, 340, 62)); // clear of the screen's corner
            if (!PanelOpen && ArcadeSkin.Button(new Rect(1036, 646, 224, 58), "MAIN MENU ▶", KitButton.Primary, true, body)) action = HubAction.NextNight;

            // taps: a station or the door walks you there; anywhere else on the floor walks you to that spot
            if (!PanelOpen)
            {
                for (int i = 0; i < Spots.Length; i++)
                {
                    Rect r = Bounds(Spots[i], MetaIcons.Station(Spots[i].Station, false));
                    if (GUI.Button(r, GUIContent.none, GUIStyle.none)) WalkTo(Spots[i].Foot + new Vector2(0, 34), i, false);
                }
                if (GUI.Button(DoorArea, GUIContent.none, GUIStyle.none)) WalkTo(DoorFoot, -1, true);
                Event e = Event.current;
                if (e.type == EventType.MouseDown && Floor.Contains(e.mousePosition)) { WalkTo(e.mousePosition, -1, false); e.Use(); }
            }
            if (Time.unscaledTime - noticeAt < 1.6f) Tag(new Vector2(640, 600), notice);
            if (PanelOpen)
            {
                NavBegin();
                Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
                bool drawPanel = true;
                if (closingAt >= 0)
                {
                    float k = (Time.unscaledTime - closingAt) / LivingUI.CloseSeconds;
                    if (k >= 1) { Open = null; closingAt = -1; drawPanel = false; }
                    else { if (!PresentationSettings.ReducedMotion) ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(1, .9f, k), PanelRect.center); GUI.color *= new Color(1, 1, 1, 1 - k); GUI.enabled = false; }
                }
                else LivingUI.Pop(Time.unscaledTime - openedAt, PanelRect.center);
                if (drawPanel) DrawPanel(catalog, profile, heroIndex, save);
                GUI.enabled = true;
                GUI.matrix = keepMatrix; GUI.color = keepColour;
                NavEnd();
            }
            if (panelAction != HubAction.None) { if (action == HubAction.None) action = panelAction; panelAction = HubAction.None; }
            return action;
        }

        private readonly List<UnitDefinition> residentPool = new List<UnitDefinition>();
        private string residentsFor;
        /// <summary>Up to five friends the traveller has met, each on a slow loop across the carpet (clear of the stations).</summary>
        private List<(UnitDefinition unit, Vector2 at, bool left)> Residents(RunCatalog catalog, PlayerProfile profile, float t, bool reduced)
        {
            var result = new List<(UnitDefinition, Vector2, bool)>();
            string key = (profile.SeenUnits?.Count ?? 0) + "/" + catalog.Companions.Length;
            if (residentsFor != key)
            {
                residentsFor = key; residentPool.Clear();
                foreach (UnitDefinition u in catalog.Companions) if (u != null && u.Body != null && profile.SeenUnits != null && profile.SeenUnits.Contains(u.Id) && !u.SpecialRecruit) residentPool.Add(u);
                residentPool.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
                // a new traveller has met no one yet: the first friends of the roster keep the room company so it is never empty
                foreach (UnitDefinition u in catalog.Companions)
                {
                    if (residentPool.Count >= 3) break;
                    if (u != null && u.Body != null && !u.SpecialRecruit && !residentPool.Contains(u)) residentPool.Add(u);
                }
            }
            int count = Mathf.Min(5, residentPool.Count);
            for (int i = 0; i < count; i++)
            {
                UnitDefinition u = residentPool[(i * 7) % residentPool.Count];
                float phase = i * 1.9f, speed = .09f + .02f * (i % 3);
                Vector2 home = new Vector2(300 + i * 170, 440 + (i % 2) * 120);
                float a = reduced ? phase : t * speed + phase, b = reduced ? phase : t * speed * 1.4f + phase;
                Vector2 at = home + new Vector2(Mathf.Sin(a) * 90, Mathf.Cos(b) * 38);
                at.y = Mathf.Clamp(at.y, 380, 650);
                result.Add((u, at, Mathf.Cos(a) < 0));
            }
            return result;
        }
        private void DrawDecor(Decor d, float t, bool reduced)
        {
            float ratio = d.Sprite.rect.width / d.Sprite.rect.height, h = d.Height, w = h * ratio;
            float breathe = d.Name == "CatCushion" && !reduced ? 1 + .018f * Mathf.Sin(t * 1.7f) : 1;
            Rect r = new Rect(d.Foot.x - w * .5f, d.Foot.y - h * breathe, w, h * breathe);
            r.y += FootPad(d.Sprite, new Rect(r.x, r.y, w, h)) * .5f;
            ArcadeSkin.Glow(new Rect(d.Foot.x - w * .45f, d.Foot.y - 10, w * .9f, 22), new Color(0, 0, 0, .45f));
            RunHud.Sprite(r, d.Sprite);
            if (d.Name == "TeaTable" && !reduced && Event.current.type == EventType.Repaint)
                for (int i = 0; i < 4; i++)
                {
                    float q = Mathf.Repeat(t * .35f + i * .25f, 1);
                    ArcadeSkin.Glow(new Rect(d.Foot.x - 8 + Mathf.Sin(t * 1.5f + i * 2) * 10 - q * 6, d.Foot.y - h * .78f - q * 46, 26 + q * 18, 26 + q * 18), new Color(1, 1, 1, .22f * Mathf.Sin(q * Mathf.PI)));
                }
        }
        private void DrawResident((UnitDefinition unit, Vector2 at, bool left) r, bool reduced)
        {
            Sprite body = r.unit.Body;
            float bob = reduced ? 0 : Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.2f + r.at.x)) * 3;
            Rect rect = new Rect(r.at.x - 34, r.at.y - 76, 68, 76);
            rect.y += FootPad(body, rect) - bob;
            ArcadeSkin.Glow(new Rect(r.at.x - 30, r.at.y - 9, 60, 18), new Color(0, 0, 0, .5f));
            Matrix4x4 keep = GUI.matrix;
            if (r.left) ArcadeSkin.ScaleAround(new Vector2(-1, 1), rect.center);
            RunHud.Sprite(rect, body);
            GUI.matrix = keep;
        }

        private void DrawTraveller(UnitDefinition unit, int identity, bool reduced)
        {
            Sprite pose = unit.AnimationFrames != null && unit.AnimationFrames.Length >= 4 ? unit.AnimationFrames[UnitVisual.AnimationFrame(Time.unscaledTime, identity, walking, reduced)] ?? unit.Body : unit.Body;
            Rect r = new Rect(hero.x - 58, hero.y - 128, 116, 128);
            r.y += FootPad(pose, r); // the painting's empty strip under the feet would lift it off its shadow: the feet stand on the shadow
            ArcadeSkin.Glow(new Rect(hero.x - 120, hero.y - 150, 240, 190), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, .18f));
            ArcadeSkin.Glow(new Rect(hero.x - 46, hero.y - 14, 92, 26), new Color(0, 0, 0, .5f));
            Matrix4x4 keep = GUI.matrix;
            if (facingLeft) ArcadeSkin.ScaleAround(new Vector2(-1, 1), r.center);
            RunHud.Sprite(r, pose);
            GUI.matrix = keep;
        }

        /// <summary>How far below a sprite's feet its picture runs on (empty paint), in the pixels of a rect it is fitted into.</summary>
        private static float FootPad(Sprite sprite, Rect fit)
        {
            if (sprite == null) return 0;
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            float pad = Mathf.Max(0, ink.yMin - sprite.bounds.min.y) / Mathf.Max(.001f, sprite.bounds.size.y);
            float ratio = sprite.rect.width / sprite.rect.height, drawn = Mathf.Min(fit.height, fit.width / ratio);
            return pad * drawn;
        }

        /// <summary>Whether the lamp oil or keepsakes can buy something here now (the lit badge on the Lamp Tree and on the menu).</summary>
        public static bool Affordable(PlayerProfile profile)
        {
            if (profile == null) return false;
            if (MetaProgression.AnyLightable(profile)) return true;
            foreach (MetaStation station in MetaProgression.Working)
                foreach (MetaTrack track in MetaProgression.For(station))
                    if (MetaProgression.Blocked(profile, track) == null) return true;
            return false;
        }

        private void Tag(Vector2 topCentre, string text)
        {
            float w = Mathf.Max(150, chip.CalcSize(new GUIContent(text)).x + 44);
            Rect tag = new Rect(Mathf.Clamp(topCentre.x - w * .5f, 8, 1272 - w), Mathf.Clamp(topCentre.y, 90, 680), w, 34);
            ArcadeSkin.Ticket(tag, ArcadeSkin.Paper, ArcadeSkin.Accent);
            chip.normal.textColor = ArcadeSkin.Ink;
            GUI.Label(tag, text, chip);
        }

        /// <summary>Lamp Oil and Keepsakes on a velvet plaque.</summary>
        public void Purse(PlayerProfile profile, Rect r)
        {
            Styles();
            ArcadeSkin.Frame(r, KitFrame.Plaque);
            number.normal.textColor = ArcadeSkin.KitCream; number.alignment = TextAnchor.MiddleLeft;
            // requirement 24/10/2026: the icons sat on the plaque's scrolled end; lay them out inside the velvet (the frame's padded content)
            Rect c = ArcadeSkin.Content(r, KitFrame.Plaque); float half = c.width * .5f, icon = Mathf.Min(40, c.height - 8);
            RunHud.Sprite(new Rect(c.x + 4, c.center.y - icon * .5f, icon, icon), MetaIcons.LampOil);
            GUI.Label(new Rect(c.x + icon + 12, c.y, half - icon - 14, c.height), profile.Meta.LampOil.ToString("N0"), number);
            RunHud.Sprite(new Rect(c.x + half + 4, c.center.y - icon * .5f, icon, icon), MetaIcons.Keepsake);
            GUI.Label(new Rect(c.x + half + icon + 12, c.y, half - icon - 14, c.height), profile.Meta.Keepsakes.ToString("N0"), number);
            number.alignment = TextAnchor.UpperLeft;
        }

        // ---------------------------------------------------------------- a station's cards
        private static readonly Rect PanelRect = new Rect(140, 96, 1000, 540);
        private void DrawPanel(RunCatalog catalog, PlayerProfile profile, int heroIndex, Func<bool> save)
        {
            MetaStation station = Open.Value;
            ArcadeSkin.Line(new Rect(0, 0, 1280, 720), new Color(0, 0, 0, .5f));
            Purse(profile, new Rect(900, 26, 340, 62)); // clear of the screen's corner
            ArcadeSkin.Frame(PanelRect, KitFrame.Dialogue, .5f);
            if (station == MetaStation.Lantern)
            {
                // the Lamp Tree takes the whole panel (no station picture)
                ArcadeSkin.Frame(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), KitFrame.Plaque);
                heading.normal.textColor = ArcadeSkin.KitCream;
                GUI.Label(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), "THE LAMP TREE", heading);
                if (ArcadeSkin.Medallion(new Rect(PanelRect.xMax - 58, PanelRect.y - 6, 60, 60), KitMedallion.Close)) { Close(); return; }
                DrawTree(catalog, profile, save);
                return;
            }
            ArcadeSkin.Glow(new Rect(PanelRect.x + 10, PanelRect.y + 60, 220, 260), new Color(1f, .8f, .45f, .22f));
            RunHud.Sprite(new Rect(PanelRect.x + 34, PanelRect.y + 72, 172, 206), MetaIcons.Station(station, MetaProgression.Restored(profile, station)));
            ArcadeSkin.Frame(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), KitFrame.Plaque);
            heading.normal.textColor = ArcadeSkin.KitCream;
            GUI.Label(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), MetaProgression.StationName(station).ToUpperInvariant(), heading);
            small.normal.textColor = ArcadeSkin.KitCream; small.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(PanelRect.x + 20, PanelRect.y + 308, 200, 60), MetaProgression.StationLine(station), small); // clear of the picture above
            small.alignment = TextAnchor.UpperLeft;
            if (ArcadeSkin.Medallion(new Rect(PanelRect.xMax - 58, PanelRect.y - 6, 60, 60), KitMedallion.Close)) { Close(); return; }

            Rect area = new Rect(PanelRect.x + 240, PanelRect.y + 48, PanelRect.width - 270, PanelRect.height - 74);
            if (station == MetaStation.Wardrobe) { DrawWardrobe(area, catalog, profile, heroIndex); return; }
            if (station >= MetaStation.Postbox) { DrawStation(station, area, catalog, profile, heroIndex, save); return; }
            navCols = 2;
            var tracks = new List<MetaTrack>(MetaProgression.For(station));
            float cw = (area.width - 16) * .5f, ch = 150;
            int rows = Mathf.CeilToInt(tracks.Count / 2f);
            float extra = station == MetaStation.Nursery && MetaProgression.Rank(profile, "nursery.heirloom") > 0 ? 96 : 0;
            float height = rows * (ch + 14) + extra;
            ScrollBegin(area, height);
            for (int i = 0; i < tracks.Count; i++)
                Card(new Rect(i % 2 * (cw + 12), i / 2 * (ch + 14), cw - 4, ch), tracks[i], catalog, profile, save);
            if (extra > 0) Heirloom(new Rect(0, rows * (ch + 14), area.width - 20, 86), catalog, profile, save);
            ScrollEnd(area);
        }

        private void Card(Rect c, MetaTrack track, RunCatalog catalog, PlayerProfile profile, Func<bool> save)
        {
            int rank = MetaProgression.Rank(profile, track.Id);
            bool flash = bought == track.Id && Time.unscaledTime - boughtAt < .6f;
            if (flash) ArcadeSkin.Glow(new Rect(c.x - 12, c.y - 12, c.width + 24, c.height + 24), new Color(1f, .85f, .5f, .5f * (1 - (Time.unscaledTime - boughtAt) / .6f)));
            ArcadeSkin.Frame(c, KitFrame.Card, .3f);
            Color ink = ArcadeSkin.FrameText(KitFrame.Card);
            Rect inner = ArcadeSkin.Content(c, KitFrame.Card, true, .3f); // inside the card's rim with the standard padding
            float tx = inner.x + 60, tw = inner.xMax - tx;
            TrackIcon(new Rect(inner.x, inner.y, 48, 48), track);
            body.normal.textColor = ink; small.normal.textColor = new Color(ink.r, ink.g, ink.b, .85f); number.normal.textColor = ink;
            GUI.Label(new Rect(tx, inner.y - 3, tw - 56, 26), track.Name, body);
            small.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(inner.xMax - 52, inner.y, 52, 22), rank + "/" + track.Max, small); small.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(tx, inner.y + 23, tw, 22), track.Line, small);
            string values = rank >= track.Max ? track.Value(rank) : track.Value(rank) + "  →  " + track.Value(rank + 1);
            GUI.Label(new Rect(tx, inner.y + 43, tw, 26), values, number);
            string blocked = MetaProgression.Blocked(profile, track);
            Rect button = new Rect(tx, inner.yMax - 38, tw, 38);
            if (rank >= track.Max) { ArcadeSkin.Button(button, "MAX", KitButton.Secondary, false, chip); Nav(button, false); return; }
            bool can = blocked == null;
            string caption = track.Cost[rank].ToString("N0");
            if ((ArcadeSkin.Button(button, "      " + caption, can ? KitButton.Primary : KitButton.Secondary, can, body) | Nav(button, can)) && can)
            {
                string before = JsonUtility.ToJson(profile.Meta);
                if (MetaProgression.Buy(profile, track.Id))
                {
                    if (!save()) { JsonUtility.FromJsonOverwrite(before, profile.Meta); CheckpointAudio.PlayInterfaceId("ui.error"); }
                    else { bought = track.Id; boughtAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId(rank + 1 >= track.Max ? "up.star3" : "shop.buy.gold"); }
                }
            }
            float cw = body.CalcSize(new GUIContent(caption)).x;
            RunHud.Sprite(new Rect(button.center.x - cw * .5f - 28, button.center.y - 15, 30, 30), MetaIcons.Currency(track.Currency));
            if (!can && blocked != "Not enough oil" && blocked != "Not enough keepsakes")
            {
                small.normal.textColor = new Color(.55f, .18f, .12f);
                small.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(c.x + 76, c.y + 60, c.width - 90, 22), blocked, small); small.alignment = TextAnchor.UpperLeft;
            }
        }

        private static void TrackIcon(Rect r, MetaTrack track)
        {
            switch (track.Id)
            {
                case "lantern.wick": if (PaintedStatIcons.TryDraw(r, ItemEffect.Health)) return; break;
                case "lantern.stride": if (PaintedStatIcons.TryDraw(r, ItemEffect.MoveSpeed)) return; break;
                case "lantern.reach": if (PaintedStatIcons.TryDraw(r, ItemEffect.Pickup)) return; break;
                case "lantern.spark": if (PaintedStatIcons.TryDraw(r, ItemEffect.Crit)) return; break;
                case "ledger.pockets": if (PaintedStatIcons.TryDraw(r, ItemEffect.Interest)) return; break;
                case "ledger.chest": if (PaintedStatIcons.TryDraw(r, ItemEffect.ChestQuality)) return; break;
                case "nursery.stork": RunHud.Sprite(r, CurrencyIcons.Egg); return;
            }
            if (track.Station == MetaStation.Ledger) { RunHud.Sprite(r, CurrencyIcons.Coin); return; }
            RunHud.Sprite(r, MetaIcons.Station(track.Station, true));
        }

        /// <summary>The Nursery's heirloom: tap one family's crest.</summary>
        private void Heirloom(Rect r, RunCatalog catalog, PlayerProfile profile, Func<bool> save)
        {
            ArcadeSkin.Frame(r, KitFrame.Card, .3f);
            Color ink = ArcadeSkin.FrameText(KitFrame.Card);
            body.normal.textColor = ink;
            BondDefinition chosen = Array.Find(catalog.Bonds, b => b.Id == profile.Meta.Heirloom);
            GUI.Label(new Rect(r.x + 16, r.y + 8, r.width - 32, 26), "FAMILY · " + (chosen != null ? chosen.Name.ToUpperInvariant() : "TAP A CREST"), body);
            float x = r.x + 16;
            foreach (BondDefinition bond in catalog.Bonds)
            {
                if (bond.Icon == null) continue;
                if (x + 44 > r.xMax - 10) break;
                Rect cell = new Rect(x, r.y + 38, 40, 40);
                if (bond.Id == profile.Meta.Heirloom) ArcadeSkin.Glow(new Rect(cell.x - 8, cell.y - 8, 56, 56), new Color(1f, .8f, .4f, .7f));
                RunHud.Sprite(cell, bond.Icon);
                if ((GUI.Button(cell, GUIContent.none, GUIStyle.none) | Nav(cell)) && bond.Id != profile.Meta.Heirloom)
                {
                    string before = profile.Meta.Heirloom; profile.Meta.Heirloom = bond.Id;
                    if (!save()) profile.Meta.Heirloom = before; else CheckpointAudio.PlayInterfaceId("ui.select");
                }
                x += 46;
            }
        }

        /// <summary>The Wardrobe: each hero's mastery, earned by playing (no price): three perks, chosen at hero select.</summary>
        private void DrawWardrobe(Rect area, RunCatalog catalog, PlayerProfile profile, int heroIndex)
        {
            if (wardrobeHero < 0 || wardrobeHero >= catalog.Champions.Length) wardrobeHero = Mathf.Clamp(heroIndex, 0, catalog.Champions.Length - 1);
            UnitDefinition hero = catalog.Champions[wardrobeHero];
            int waves = MetaProgression.Mastery(profile, hero.Id)?.Waves ?? 0, rank = MetaProgression.MasteryRank(waves);
            navCols = 2;
            if (ArcadeSkin.IconButton(new Rect(area.x, area.y, 56, 56), KitRound.Walnut, "◀") | Nav(new Rect(area.x, area.y, 56, 56))) wardrobeHero = (wardrobeHero + catalog.Champions.Length - 1) % catalog.Champions.Length;
            if (ArcadeSkin.IconButton(new Rect(area.xMax - 56, area.y, 56, 56), KitRound.Walnut, "▶") | Nav(new Rect(area.xMax - 56, area.y, 56, 56))) wardrobeHero = (wardrobeHero + 1) % catalog.Champions.Length;
            RunHud.Sprite(new Rect(area.x + 70, area.y - 4, 64, 64), hero.Body);
            body.normal.textColor = ArcadeSkin.KitCream; number.normal.textColor = ArcadeSkin.KitCream;
            GUI.Label(new Rect(area.x + 142, area.y + 2, area.width - 280, 26), RunUICopy.UnitName(hero), body);
            GUI.Label(new Rect(area.x + 142, area.y + 28, area.width - 280, 26), waves + " waves together", number);
            float y = area.y + 72, ch = 100;
            for (int i = 0; i < MetaProgression.MasteryWaves.Length; i++)
            {
                Rect c = new Rect(area.x, y + i * (ch + 10), area.width, ch);
                bool earned = rank > i;
                bool exists = i != 0 || MetaProgression.AltStarter(catalog, hero) != null;
                if (i == 1) exists = HeroKits.For(hero.Id) != null;
                if (i == 2) exists = MetaProgression.HabitDropped(HeroKits.For(hero.Id)).HasValue;
                ArcadeSkin.Frame(c, KitFrame.Card, .3f);
                Color ink = ArcadeSkin.FrameText(KitFrame.Card);
                body.normal.textColor = ink; small.normal.textColor = ink; number.normal.textColor = ink;
                Sprite icon = i == 0 && MetaProgression.AltStarter(catalog, hero) is UnitDefinition coat ? coat.Body : MetaIcons.Station(MetaStation.Wardrobe, earned);
                RunHud.Sprite(new Rect(c.x + 14, c.y + 14, 70, 70), icon);
                GUI.Label(new Rect(c.x + 96, c.y + 12, c.width - 300, 26), MetaProgression.MasteryNames[i], body);
                string line = MetaProgression.MasteryLines[i];
                if (i == 0 && MetaProgression.AltStarter(catalog, hero) is UnitDefinition alt) line = "Start with " + RunUICopy.UnitName(alt);
                if (i == 2 && exists) line = MetaProgression.HabitAdded(HeroKits.For(hero.Id)).Text.Replace("Second habit: ", "");
                GUI.Label(new Rect(c.x + 96, c.y + 40, c.width - 300, 44), exists ? line : "Not for this hero", small);
                number.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(c.xMax - 200, c.y, 180, ch), !exists ? "—" : earned ? "READY" : Mathf.Min(waves, MetaProgression.MasteryWaves[i]) + " / " + MetaProgression.MasteryWaves[i], number);
                number.alignment = TextAnchor.UpperLeft;
            }
        }
    }
}
