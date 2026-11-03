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
        public static Sprite Room => room != null ? room : room = Resources.Load<Sprite>("Hub/WaitingRoomPainted") ?? Resources.Load<Sprite>("Hub/WaitingRoom");
        /// <summary>The room painted whole, its furniture and stations in it (review 31/10/2026: pasted pieces never shared its light).</summary>
        public static bool Painted => Room != null && Room.name == "WaitingRoomPainted";
        public static Sprite Station(MetaStation station, bool restored)
        {
            string key = station + (restored ? "_Lit" : "_Dusty");
            if (!stations.TryGetValue(key, out Sprite s)) stations[key] = s = Resources.Load<Sprite>("Hub/Stations/" + key);
            return s;
        }
    }

    public enum HubAction { None, NextNight, Back, Daily, Collection }

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
        public static readonly Rect DoorArea = new Rect(552, 0, 176, 168); // the whole door and its mat
        public static readonly Vector2 DoorFoot = new Vector2(640, 168);
        public static readonly Rect Floor = new Rect(60, 160, 1160, 545);
        public const float Speed = 330, Reach = 150; // a station answers from well around it (review 31/10/2026: it had to be touched almost exactly)

        private Vector2 hero = new Vector2(640, 560);
        /// <summary>Captures: stand the traveller at a point of the room.</summary>
        public void StandAt(Vector2 at) { hero = at; target = null; targetSpot = -1; targetDoor = false; UpdateFocus(); }
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
            DoorFocus = Vector2.Distance(hero, DoorFoot) < Reach + 20;
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
                else if (NearThing() is Thing th) Say(th);
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
        /// <summary>The room's things that are not stations (painted into the room): tap one and a small line about it appears
        /// over it, a different one each time.</summary>
        private sealed class Thing { public string Name, What; public Rect Area; public string[] Lines; public int Next; }
        private static readonly Thing[] RoomThings =
        {
            new Thing { Name = "Cat", What = "The cat that came with the room. Nobody remembers whose it is.", Area = new Rect(785, 375, 90, 70), Lines = new[] { "The cat is asleep. It has been asleep since before the lamp was lit.", "One ear turns towards you. The rest of the cat does not.", "Purring, very quietly, in time with the clock." } },
            new Thing { Name = "Tea", What = "A pot of tea kept warm for whoever comes in from the night.", Area = new Rect(438, 345, 112, 120), Lines = new[] { "The tea is still warm. Nobody remembers pouring it.", "Three cups. One for you, one for a friend, one for whoever comes next.", "A biscuit is missing. The cat looks innocent." } },
            new Thing { Name = "Gramophone", What = "An old gramophone. It plays the same waltz every night.", Area = new Rect(195, 340, 90, 115), Lines = new[] { "A waltz, very slow, the needle almost asleep.", "It plays the same song every night. Nobody minds.", "When you come back from a long night, it skips once, like a heartbeat." } },
            new Thing { Name = "Cases", What = "Travel cases nobody has come back for.", Area = new Rect(985, 385, 120, 110), Lines = new[] { "Three cases, labelled for three different towns. None of them yours, yet.", "Something inside the top case ticks.", "A tag reads: please hold until dawn." } },
            new Thing { Name = "Coat rack", What = "Coats and a scarf left by travellers who went on.", Area = new Rect(860, 70, 60, 140), Lines = new[] { "A red scarf someone forgot. It smells of rain.", "The lantern on the hook glows a little brighter when you pass." } },
            new Thing { Name = "Bench", What = "A velvet bench where the waiting is done.", Area = new Rect(550, 540, 220, 105), Lines = new[] { "Someone left a ticket on the bench. It is for tonight.", "The velvet is warm, as if someone just stood up." } },
            new Thing { Name = "Rug", What = "The room's big red rug. The friends walk round its edge.", Area = new Rect(330, 260, 620, 250), Lines = new[] { "The rug has a path worn round its edge, exactly one friend wide.", "A burn mark in one corner. Nobody admits to it.", "It is softer than it looks. The cat agrees." } },
        };
        private Thing spoken; private string spokenLine; private float spokenAt = -10;
        private List<(UnitDefinition unit, Vector2 at, bool left)> shownResidents; private UnitDefinition shownHero;
        private readonly Dictionary<string, Thing> people = new Dictionary<string, Thing>();
        private static readonly string[] FriendLines =
        {
            "{0} waves at you, then pretends it did not.", "{0} is counting the rug's tassels again.", "{0}: \"Same time tomorrow night?\"",
            "{0} hums the gramophone's waltz, a little off key.", "{0} stops to sniff the tea. Too hot, still.", "{0} yawns. Dawn was a long walk.",
        };
        private static readonly string[] HeroLines =
        {
            "{0} straightens up. The brigade is watching.", "{0} checks the lamp oil. Enough for one more night.", "{0}: \"Rest first. Then the door.\"",
        };
        /// <summary>A person in the room as a tappable thing: its area follows it as it walks, its lines keep their turn.</summary>
        private Thing Person(string key, string name, Vector2 foot, float height, string[] lines)
        {
            if (!people.TryGetValue(key, out Thing th)) people[key] = th = new Thing { Name = name, What = key == "hero" ? "You, back from the night." : "A friend of the brigade, out for a stroll round the rug.", Lines = System.Array.ConvertAll(lines, l => string.Format(l, name)) };
            th.Area = new Rect(foot.x - 44, foot.y - height, 88, height + 8);
            return th;
        }
        /// <summary>The people first (they stand in front of the room's things), then the room's things, the rug last (it lies under all).</summary>
        private List<Thing> AllThings(bool withHero)
        {
            var all = new List<Thing>();
            if (shownResidents != null)
                foreach (var r in shownResidents) all.Add(Person(r.unit.Id, RunUICopy.UnitName(r.unit), r.at, 92, FriendLines));
            if (withHero && shownHero != null) all.Add(Person("hero", RunUICopy.UnitName(shownHero), hero, 112, HeroLines));
            all.AddRange(RoomThings);
            return all;
        }
        private void Say(Thing th)
        {
            spoken = th; spokenLine = th.Lines[th.Next % th.Lines.Length]; th.Next++; spokenAt = Time.unscaledTime;
            CheckpointAudio.PlayInterfaceId("ui.select");
        }
        /// <summary>The thing or friend the hero stands by (requirement 1/11/2026: like the shop's ferry, coming near shows its card
        /// and Space looks); a station or the door nearby takes the hero first.</summary>
        private Thing NearThing()
        {
            if (Focus >= 0 || DoorFocus) return null;
            Thing best = null; float bestD = ThingReach;
            foreach (Thing th in AllThings(false))
            {
                Vector2 foot = new Vector2(th.Area.center.x, th.Area.yMax);
                float d = Vector2.Distance(hero, foot), reach = ThingReach + Mathf.Min(th.Area.width * .3f, 90);
                if (d < reach && d < bestD + (reach - ThingReach)) { best = th; bestD = d - (reach - ThingReach); }
            }
            return best;
        }
        private const float ThingReach = 110;
        private Thing nearShown; private float nearAt;
        private void Things()
        {
            foreach (Thing th in AllThings(true))
                if (TouchTargets.Button(th.Area, GUIContent.none, GUIStyle.none)) Say(th);
            // the card for what the hero stands by: its name, what it is, and LOOK
            Thing near = NearThing();
            if (near != nearShown) { nearShown = near; nearAt = Time.unscaledTime; }
            if (near != null)
            {
                Matrix4x4 keepCard = GUI.matrix;
                GUI.matrix = keepCard * Matrix4x4.Scale(new Vector3(1280f / 1920f, 720f / 1080f, 1)); // the card is laid out on the 1920 canvas
                bool person = shownResidents != null && shownResidents.Exists(r => RunUICopy.UnitName(r.unit) == near.Name);
                if (ShopPrompt.Draw(near.Name.ToUpperInvariant(), near.What ?? "", person ? "TALK" : "LOOK", Time.unscaledTime - nearAt)) Say(near);
                GUI.matrix = keepCard;
            }
            if (spoken != null && Time.unscaledTime - spokenAt < 4.5f)
            {
                // the line in a small paper card over the thing, popping in and fading out
                if (thingStyle == null) { thingStyle = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote)); thingStyle.wordWrap = true; thingStyle.alignment = TextAnchor.MiddleCenter; thingStyle.font = ArcadeTypography.Hand; }
                float w = 340, h = thingStyle.CalcHeight(new GUIContent(spokenLine), w - 36) + 28, age = Time.unscaledTime - spokenAt;
                Rect card = new Rect(Mathf.Clamp(spoken.Area.center.x - w * .5f, 16, 1264 - w), Mathf.Max(96, spoken.Area.y - h - 12), w, h);
                Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
                LivingUI.Pop(age, card.center);
                GUI.color = new Color(keepColour.r, keepColour.g, keepColour.b, keepColour.a * Mathf.Clamp01((4.5f - age) / .4f));
                if (!ArcadeSkin.PaperArt(card, PaperKind.Receipt)) ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Accent);
                thingStyle.normal.textColor = ArcadeSkin.KitInk;
                GUI.Label(new Rect(card.x + 18, card.y + 14, w - 36, h - 28), spokenLine, thingStyle);
                GUI.matrix = keepMatrix; GUI.color = keepColour;
            }
        }
        private GUIStyle thingStyle;

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
            small = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote)); small.wordWrap = true;
            number = ArcadeTypography.Style(20, false, true); number.wordWrap = false;
            chip = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote), true); chip.alignment = TextAnchor.MiddleCenter; chip.wordWrap = false;
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
            shownResidents = residents; shownHero = catalog.Champions[Mathf.Clamp(heroIndex, 0, catalog.Champions.Length - 1)];
            for (int i = 0; i < residents.Count; i++) order.Add((residents[i].at.y, -2 - i));
            for (int i = 0; i < Decorations.Length; i++)
            {
                Decor d = Decorations[i];
                if (!d.Tried) { d.Tried = true; d.Sprite = Resources.Load<Sprite>("Hub/Props/" + d.Name); }
                if (d.Sprite != null && !MetaIcons.Painted) order.Add((d.Foot.y, -100 - i)); // painted into the room
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
                if (MetaIcons.Painted)
                {
                    // the station is painted into the room: the one you walk up to is lit by a warm light, nothing pasted on top
                    if (index == Focus && !PanelOpen) ArcadeSkin.Glow(new Rect(r.x - 30, r.y - 20, r.width + 60, r.height + 40), new Color(1f, .85f, .55f, works ? .3f + .1f * breathe : .14f));
                    continue;
                }
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

            tagsPlaced.Clear();
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

            // review 31/10/2026: every station wears its name (only the Lamp Tree did), and the room's things can be tapped
            if (!PanelOpen)
            {
                // the near tag and the door claim their place before the others hang theirs
                if (Focus >= 0) { Spot fs = Spots[Focus]; Rect fr = Bounds(fs, MetaIcons.Station(fs.Station, false)); tagsPlaced.Add(TagRect(new Vector2(fs.Foot.x, fr.y - 40), StationTag(fs.Station))); }
                if (DoorFocus) tagsPlaced.Add(TagRect(new Vector2(DoorFoot.x, DoorArea.yMax + 4), "NEW JOURNEY"));
                for (int i = 0; i < Spots.Length; i++)
                {
                    if (i == Focus || Spots[i].Station == MetaStation.Lantern) continue; // the near one pops its own tag; the Lamp Tree has its call-out
                    Rect sr = Bounds(Spots[i], MetaIcons.Station(Spots[i].Station, false));
                    string name = StationTag(Spots[i].Station);
                    // above the piece, else below it; never over another sign (the walnut board reaches 14 px past the tag each side)
                    Rect above = TagRect(new Vector2(Spots[i].Foot.x, sr.y - 40), name), below = TagRect(new Vector2(Spots[i].Foot.x, sr.yMax + 6), name);
                    Rect chosen = Free(above) ? above : Free(below) ? below : Rect.zero;
                    if (chosen.width > 0) TagIn(chosen, name);
                }
                Things();
            }
            // what is near: a paper tag over it
            if (!PanelOpen && Focus >= 0)
            {
                Spot s = Spots[Focus]; Rect r = Bounds(s, MetaIcons.Station(s.Station, false));
                if (focusShown != Focus) { focusShown = Focus; focusAt = Time.unscaledTime; }
                Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
                LivingUI.Pop(Time.unscaledTime - focusAt, new Vector2(s.Foot.x, r.y - 23));
                Tag(new Vector2(s.Foot.x, r.y - 40), StationTag(s.Station));
                GUI.matrix = keepMatrix; GUI.color = keepColour;
            }
            else focusShown = -2;
            if (!PanelOpen && DoorFocus) Tag(new Vector2(DoorFoot.x, DoorArea.yMax + 4), "NEW JOURNEY");

            // the room's frame: back, title, purse, the way on
            if (!PanelOpen && ArcadeSkin.Medallion(new Rect(22, 18, 64, 64), KitMedallion.Back)) action = HubAction.Back;
            // an open station shows its own title: the room's plaque steps aside (review 31/10/2026: two titles stacked)
            if (!PanelOpen)
            {
                ArcadeSkin.Frame(new Rect(96, 18, 400, 62), KitFrame.Plaque);
                heading.normal.textColor = ArcadeSkin.KitCream;
                GUI.Label(new Rect(96, 18, 400, 62), "THE WAITING ROOM", heading);
            }
            if (!PanelOpen) FavourStars(profile, new Rect(96, 80, 400, 30)); // an open station's title would cover it by half
            Purse(profile, new Rect(900, 26, 340, 62)); // clear of the screen's corner
            if (!PanelOpen && ArcadeSkin.Button(new Rect(1016, 646, 244, 58), "NEW JOURNEY ▶", KitButton.Primary, true, body)) action = HubAction.NextNight; // the door: on to the next night (the medallion top left goes back)

            // taps: a station or the door walks you there; anywhere else on the floor walks you to that spot
            if (!PanelOpen)
            {
                for (int i = 0; i < Spots.Length; i++)
                {
                    Rect r = Bounds(Spots[i], MetaIcons.Station(Spots[i].Station, false));
                    if (TouchTargets.Button(new Rect(r.x - 44, r.y - 36, r.width + 88, r.height + 72), GUIContent.none, GUIStyle.none)) WalkTo(Spots[i].Foot + new Vector2(0, 34), i, false); // a thumb-sized margin round each station
                }
                if (TouchTargets.Button(DoorArea, GUIContent.none, GUIStyle.none)) WalkTo(DoorFoot, -1, true);
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
                // the friends of your last night first: the room is where they wait for you
                RunRecord last = profile.History != null && profile.History.Count > 0 ? profile.History[profile.History.Count - 1] : null;
                if (last?.Build?.Roster != null)
                    foreach (RosterEntry entry in last.Build.Roster)
                        if (catalog.Unit(entry.UnitId) is UnitDefinition met && met.Body != null && !residentPool.Contains(met) && !RunSession.StorySpecies(met.Id)) residentPool.Add(met);
                int fromLastNight = residentPool.Count;
                foreach (UnitDefinition u in catalog.Companions) if (u != null && u.Body != null && profile.SeenUnits != null && profile.SeenUnits.Contains(u.Id) && !u.SpecialRecruit) residentPool.Add(u);
                if (residentPool.Count > fromLastNight) residentPool.Sort(fromLastNight, residentPool.Count - fromLastNight, Comparer<UnitDefinition>.Create((a, b) => string.CompareOrdinal(a.Id, b.Id)));
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
                UnitDefinition u = residentPool[i % residentPool.Count];
                // a lap round the rug (an ellipse round the tea table), each friend at its own place on it and its own pace
                // one easy stroll pace for all (about 70 px/s), evenly spaced: different paces bunched them up (review 31/10/2026)
                float lap = i * (Mathf.PI * 2 / count), speed = .2f;
                float a = reduced ? lap : t * speed + lap;
                Vector2 rx = new Vector2(345 + (i % 2) * 25, 172 + (i % 2) * 14); // round the painted rug's edge (centre 645, 385)
                Vector2 at = new Vector2(645 + Mathf.Cos(a) * rx.x, 385 + Mathf.Sin(a) * rx.y);
                result.Add((u, at, -Mathf.Sin(a) < 0)); // facing the way round it is walking
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
            // its own walk frames as it strolls, at a size you can read (the bodies stood about 30 px tall)
            Sprite[] frames = r.unit.AnimationFrames;
            // a steady identity (its position changed every frame and made the walk frames flicker), and the step bounce in time
            // with the two walk frames (7 a second)
            int identity = Mathf.Abs(r.unit.Id.GetHashCode() % 97);
            Sprite body = frames != null && frames.Length >= 4 ? frames[UnitVisual.Frame(r.unit, UnitVisual.AnimationFrame(Time.unscaledTime, identity, !reduced, reduced))] : r.unit.Body;
            float step = reduced ? 0 : Mathf.Abs(Mathf.Sin((Time.unscaledTime * 7 + identity * .37f) * Mathf.PI)) * 2; // a small step bounce, feet near the floor
            OnFloor(body, r.at, 84, r.left, step);
        }

        private void DrawTraveller(UnitDefinition unit, int identity, bool reduced)
        {
            Sprite pose = unit.AnimationFrames != null && unit.AnimationFrames.Length >= 4 ? unit.AnimationFrames[UnitVisual.Frame(unit, UnitVisual.AnimationFrame(Time.unscaledTime, identity, walking, reduced))] ?? unit.Body : unit.Body;
            ArcadeSkin.Glow(new Rect(hero.x - 120, hero.y - 150, 240, 190), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, .18f));
            OnFloor(pose, hero, 104, facingLeft, walking && !reduced ? Mathf.Abs(Mathf.Sin(Time.unscaledTime * 7f)) * 2 : 0);
        }

        /// <summary>Stands a sprite on the floor (review 31/10/2026: the room's characters looked to float): the rect is worked out from
        /// the painting's own ink, so the lowest painted pixel (the feet) is exactly on <paramref name="foot"/> and the picture is
        /// never letterboxed inside a box of another shape; a soft cast shadow and a dark contact shadow sit right under the feet.
        /// <paramref name="lift"/> raises the body (a hop) while the shadow stays, shrinking a little.</summary>
        public static void OnFloor(Sprite sprite, Vector2 foot, float inkHeight, bool faceLeft, float lift = 0)
        {
            if (sprite == null) return;
            Rect ink = SpriteInkMetrics.BoundsOf(sprite); Bounds b = sprite.bounds;
            float full = inkHeight * b.size.y / Mathf.Max(.001f, ink.height), w = full * sprite.rect.width / sprite.rect.height;
            float padBottom = (ink.yMin - b.min.y) / b.size.y * full;
            float inkX = (ink.center.x - b.center.x) / b.size.x * w * (faceLeft ? -1 : 1);
            float inkW = ink.width / b.size.x * w, shrink = 1 - Mathf.Clamp01(lift / 40f) * .3f;
            ArcadeSkin.Glow(new Rect(foot.x - inkW * .55f * shrink, foot.y - 8, inkW * 1.1f * shrink, 18), new Color(0, 0, 0, .4f));
            ArcadeSkin.Glow(new Rect(foot.x - inkW * .36f * shrink, foot.y - 5, inkW * .72f * shrink, 10), new Color(0, 0, 0, .75f * shrink));
            Rect r = new Rect(foot.x - w * .5f - inkX, foot.y - full + padBottom - lift, w, full);
            Matrix4x4 keep = GUI.matrix;
            if (faceLeft) ArcadeSkin.ScaleAround(new Vector2(-1, 1), r.center);
            RunHud.Sprite(r, sprite);
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

        private readonly List<Rect> tagsPlaced = new List<Rect>();
        /// <summary>A station's sign: its short name (the room already says "the"), or COMING SOON.</summary>
        private static string StationTag(MetaStation station)
        {
            if (!MetaProgression.Works(station)) return "COMING SOON";
            string name = MetaProgression.StationName(station).ToUpperInvariant();
            return name.StartsWith("THE ") ? name.Substring(4) : name;
        }
        private Rect TagRect(Vector2 topCentre, string text)
        {
            float w = Mathf.Max(130, chip.CalcSize(new GUIContent(text)).x + 44);
            return new Rect(Mathf.Clamp(topCentre.x - w * .5f, 20, 1260 - w), Mathf.Clamp(topCentre.y, 120, 680), w, 34); // its chains clear of the title plaque
        }
        private bool Free(Rect tag)
        {
            Rect board = new Rect(tag.x - 20, tag.y - 8, tag.width + 40, tag.height + 16);
            foreach (Rect other in tagsPlaced) if (board.Overlaps(other)) return false;
            return true;
        }
        private void Tag(Vector2 topCentre, string text) => TagIn(TagRect(topCentre, text), text);
        private void TagIn(Rect tag, string text)
        {
            tagsPlaced.Add(tag);
            // a walnut sign hanging on two short chains (its board, rows 46-118 of the 120 px art, is the tag)
            float k = tag.height / 72f;
            if (ArcadeSkin.Strip(new Rect(tag.x - 14, tag.y - 46 * k, tag.width + 28, 120 * k), "Menu/StationSign", 70, 70)) chip.normal.textColor = ArcadeSkin.KitCream;
            else { ArcadeSkin.Ticket(tag, ArcadeSkin.Paper, ArcadeSkin.Accent); chip.normal.textColor = ArcadeSkin.Ink; }
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
                // the Lamp Tree takes the whole panel (no station picture), over a painting of the tree itself, kept dim so the lanterns read
                if (ArcadeSkin.KitSprite("Menu/LampTreeBackdrop") is Sprite treeArt)
                {
                    Color keepTree = GUI.color; GUI.color = new Color(keepTree.r, keepTree.g, keepTree.b, keepTree.a * .7f);
                    GUI.DrawTexture(ArcadeSkin.Content(PanelRect, KitFrame.Dialogue, false, .5f), treeArt.texture, ScaleMode.ScaleAndCrop);
                    GUI.color = keepTree;
                }
                ArcadeSkin.Frame(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), KitFrame.Plaque);
                heading.normal.textColor = ArcadeSkin.KitCream;
                GUI.Label(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), "THE LAMP TREE", heading);
                if (ArcadeSkin.Medallion(new Rect(PanelRect.xMax - 58, PanelRect.y - 6, 60, 60), KitMedallion.Close)) { Close(); return; }
                DrawTree(catalog, profile, save);
                return;
            }
            ArcadeSkin.Glow(new Rect(PanelRect.x + 10, PanelRect.y + 60, 220, 260), new Color(1f, .8f, .45f, .22f));
            // the station's picture and its line as one group, centred down the left column (the line floated far below the picture)
            RunHud.Sprite(new Rect(PanelRect.x + 34, PanelRect.y + 128, 172, 206), MetaIcons.Station(station, MetaProgression.Restored(profile, station)));
            ArcadeSkin.Frame(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), KitFrame.Plaque);
            heading.normal.textColor = ArcadeSkin.KitCream;
            GUI.Label(new Rect(PanelRect.center.x - 230, PanelRect.y - 30, 460, 62), MetaProgression.StationName(station).ToUpperInvariant(), heading);
            small.normal.textColor = ArcadeSkin.KitCream; small.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(PanelRect.x + 20, PanelRect.y + 344, 200, 60), MetaProgression.StationLine(station), small); // just under the picture
            small.alignment = TextAnchor.UpperLeft;
            if (ArcadeSkin.Medallion(new Rect(PanelRect.xMax - 58, PanelRect.y - 6, 60, 60), KitMedallion.Close)) { Close(); return; }

            Rect area = new Rect(PanelRect.x + 248, PanelRect.y + 66, PanelRect.width - 286, PanelRect.height - 94); // a clear step below the title plaque, inside the rim
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
            GUI.Label(new Rect(tx, inner.y - 3, tw - 56, 34), track.Name, body);
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
            GUI.Label(new Rect(r.x + 16, r.y + 8, r.width - 32, 34), "FAMILY · " + (chosen != null ? chosen.Name.ToUpperInvariant() : "TAP A CREST"), body);
            float x = r.x + 16;
            foreach (BondDefinition bond in catalog.Bonds)
            {
                if (bond.Icon == null) continue;
                if (x + 44 > r.xMax - 10) break;
                Rect cell = new Rect(x, r.y + 38, 40, 40);
                if (bond.Id == profile.Meta.Heirloom) ArcadeSkin.Glow(new Rect(cell.x - 8, cell.y - 8, 56, 56), new Color(1f, .8f, .4f, .7f));
                RunHud.Sprite(cell, bond.Icon);
                if ((TouchTargets.Button(cell, GUIContent.none, GUIStyle.none) | Nav(cell)) && bond.Id != profile.Meta.Heirloom)
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
            int count = catalog.Champions.Length;
            // the arrows step over heroes that are not in the game yet (an update or a later act)
            if (ArcadeSkin.IconButton(new Rect(area.x, area.y, 56, 56), KitRound.Walnut, "◀") | Nav(new Rect(area.x, area.y, 56, 56)))
                do wardrobeHero = (wardrobeHero + count - 1) % count; while (!HeroUnlocks.Listed(catalog.Champions[wardrobeHero]));
            if (ArcadeSkin.IconButton(new Rect(area.xMax - 56, area.y, 56, 56), KitRound.Walnut, "▶") | Nav(new Rect(area.xMax - 56, area.y, 56, 56)))
                do wardrobeHero = (wardrobeHero + 1) % count; while (!HeroUnlocks.Listed(catalog.Champions[wardrobeHero]));
            RunHud.Sprite(new Rect(area.x + 70, area.y - 4, 64, 64), hero.Body);
            body.normal.textColor = ArcadeSkin.KitCream; number.normal.textColor = ArcadeSkin.KitCream;
            GUI.Label(new Rect(area.x + 142, area.y + 2, area.width - 280, 34), RunUICopy.UnitName(hero), body);
            GUI.Label(new Rect(area.x + 142, area.y + 28, area.width - 280, 26), waves + " waves together", number);
            float y = area.y + 72, ch = 116; // the three perks fill the page, each with its road to READY
            for (int i = 0; i < MetaProgression.MasteryWaves.Length; i++)
            {
                Rect c = new Rect(area.x, y + i * (ch + 14), area.width, ch);
                bool earned = rank > i;
                bool exists = i != 0 || MetaProgression.AltStarter(catalog, hero) != null;
                if (i == 1) exists = HeroKits.For(hero.Id) != null;
                if (i == 2) exists = MetaProgression.HabitDropped(HeroKits.For(hero.Id)).HasValue;
                ArcadeSkin.Frame(c, KitFrame.Card, .3f);
                Color ink = ArcadeSkin.FrameText(KitFrame.Card);
                body.normal.textColor = ink; small.normal.textColor = ink; number.normal.textColor = ink;
                Sprite icon = i == 0 && MetaProgression.AltStarter(catalog, hero) is UnitDefinition coat ? coat.Body : MetaIcons.Station(MetaStation.Wardrobe, earned);
                RunHud.Sprite(new Rect(c.x + 22, c.y + 16, 84, 84), icon);
                GUI.Label(new Rect(c.x + 122, c.y + 10, c.width - 330, 36), MetaProgression.MasteryNames[i], body); // a box as tall as the line (26 cut the tops of the letters)
                string line = MetaProgression.MasteryLines[i];
                if (i == 0 && MetaProgression.AltStarter(catalog, hero) is UnitDefinition alt) line = "Start with " + RunUICopy.UnitName(alt);
                if (i == 2 && exists) line = MetaProgression.HabitAdded(HeroKits.For(hero.Id)).Text.Replace("Second habit: ", "");
                GUI.Label(new Rect(c.x + 122, c.y + 44, c.width - 330, 40), exists ? RunUICopy.Sentence(line) : "Not for this hero", small);
                if (exists && !earned)
                {
                    Rect bar = new Rect(c.x + 122, c.y + 88, c.width - 330, 14);
                    float share = Mathf.Clamp01(waves / (float)MetaProgression.MasteryWaves[i]);
                    if (ArcadeSkin.Strip(bar, "Info/RatioTrack", 24, 24) && bar.width * share > bar.height) ArcadeSkin.Strip(new Rect(bar.x + 2, bar.y + 2, (bar.width - 4) * share, bar.height - 4), "Info/RatioFill", 20, 20);
                }
                number.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(c.xMax - 210, c.y, 180, ch), !exists ? "—" : earned ? "READY" : Mathf.Min(waves, MetaProgression.MasteryWaves[i]) + " / " + MetaProgression.MasteryWaves[i], number);
                number.alignment = TextAnchor.UpperLeft;
            }
        }
    }
}
