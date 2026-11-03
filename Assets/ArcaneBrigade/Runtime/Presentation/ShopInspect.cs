using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Everything placed in the shop can be clicked and read. Hovering any stall, machine, door, invitation,
    /// the chair, the merchant, the lamp, the window, the curtain, the sign, a keepsake, a lantern, a watcher or a seated
    /// traveller shows its name; clicking opens a card with its story or what it does (and how to use it). Things in the
    /// room with nothing to say are not placed there (NightmareGrandStage keeps its map architecture out of the shop).
    /// Drawn behind the HUD's buttons (GUI.depth), so BUY, INFO, CONTINUE and the journal note always take a click first.</summary>
    public sealed class ShopInspect : MonoBehaviour, ShopFocus.ISource
    {
        public sealed class Entry
        {
            public string Key, Title, Body, Use;
            public Rect World;
            public int Order;
            public ShopOffer Offer;
        }
        private ArenaController owner;
        private readonly List<Entry> entries = new List<Entry>();
        private Entry hovered, open;
        private string shownKey; private float shownAt;   // when the open card appeared, for its pop-in
        private Vector2 openedFor;
        private GUIStyle name, title, body, hint;
        private readonly ShopDescriptionCard offerCard = new ShopDescriptionCard();
        private Vector2 offerScroll;
        public IReadOnlyList<Entry> Entries => entries;
        public Entry Open => open;
        public void Configure(ArenaController value) { owner = value; ShopFocus.Register(this); }
        private void OnDestroy() => ShopFocus.Unregister(this);

        // ---- the ferry-like approach: things with nothing else to answer for them are focus spots of their own
        private readonly List<Entry> near = new List<Entry>();
        /// <summary>Things another system already answers for (a stall, the warm chair, a secret invitation, the Keeper and its gifts).</summary>
        private static bool OwnAnswer(Entry e) => e.Offer != null || e.Key == "TheUnoccupiedChair" || e.Key.StartsWith("SecretInvitation_", StringComparison.Ordinal)
            || e.Key == "TheHandWaits" || e.Key.StartsWith("PrismaticReward", StringComparison.Ordinal);
        private static Vector2 Foot(Entry e) => new Vector2(e.World.center.x, e.World.yMin);
        private static float ReachOf(Entry e) => Mathf.Clamp(e.World.width * .5f + .7f, 1.1f, 2.2f);
        public int SpotCount => near.Count;
        public bool Spot(int index, out Vector2 position, out float reach)
        {
            position = default; reach = 0;
            if (index < 0 || index >= near.Count || !Allowed) return false;
            position = Foot(near[index]); reach = ReachOf(near[index]); return true;
        }
        private Entry focused; private float focusedAt;
        /// <summary>For checks: the thing whose card is up and the card open, as last painted.</summary>
        public static string DebugBy, DebugOpen;
        /// <summary>The thing the hero stands by, when nothing else holds the focus there.</summary>
        private Entry Focused()
        {
            if (owner?.Simulation?.Champion == null || owner.Shop?.Nearby != null) return null;
            ShopFocus.ISource holder = ShopFocus.Holder(owner.Simulation.Champion.Position, out int index);
            return holder == (ShopFocus.ISource)this && index >= 0 && index < near.Count ? near[index] : null;
        }

        private bool Allowed => owner?.Session != null && owner.Simulation != null && owner.Shop != null && owner.Session.State.Phase == SessionPhase.Shop
            && !owner.Session.SecretRoomOpen && owner.Simulation.Phase == RunPhase.Rest && !owner.StoryConsumesInput && !owner.PresentationPaused
            && !owner.Details.Open && !owner.TransitionPending && !owner.HallExitPending && !owner.Shop.Animating && owner.Session.State.OutsideStage != OutsideStageStatus.Reward
            && owner.GetComponent<CustomerOffer>()?.Open != true && owner.GetComponent<MemoryReturn>()?.Open != true && !FerryDock.Showing && owner.GetComponent<VisitorCard>()?.Open != true && owner.GetComponent<LucidAltar>()?.Open != true && owner.GetComponent<WaitingRealmStage>()?.Asking != true;

        private void Update()
        {
            if (!Allowed) { entries.Clear(); hovered = open = null; return; }
            Collect();
            near.Clear(); foreach (Entry e in entries) if (!OwnAnswer(e)) near.Add(e);
            Entry f = Focused();
            if (f != null && open == null && !owner.StoryConsumesInput && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0)))
            { open = f; offerScroll = Vector2.zero; openedFor = owner.Simulation.Champion.Position; Speak(f); CheckpointAudio.PlayInterfaceId("ui.open"); }
            // walking off closes the card; so does anything that takes the room away (Allowed)
            if (open != null && Vector2.Distance(owner.Simulation.Champion.Position, openedFor) > 1.6f) open = null;
            if (open != null && entries.Find(e => e.Key == open.Key) is Entry fresh) open = fresh; else open = null;
        }
        /// <summary>Opens the card for the entry with this key (diagnostics and keyboardless review).</summary>
        public bool Show(string key)
        {
            Entry entry = entries.Find(e => e.Key == key); if (entry == null) return false;
            open = entry; offerScroll = Vector2.zero; openedFor = owner.Simulation.Champion.Position; return true;
        }
        public void Close() => open = null;

        // a figure in the shop answers a tap in its own voice, a different line each time
        private static readonly string[] CustomerLines =
        {
            "\"Don't mind me. I'm only waiting for my turn.\"",
            "\"Your coins are warm. Someone held them a long time.\"",
            "\"The chair isn't for sale. Not yet.\"",
            "\"I always pay. Eventually.\"",
            "\"Such a lovely brigade. I'd take good care of one.\"",
        };
        private int customerLine;
        private void Speak(Entry entry)
        {
            if (entry.Key == "Shopkeeper") { owner.Shop?.MerchantChat(); return; }
            if (entry.Title != "THE LAST CUSTOMER") return;
            Rect world = entry.World;
            SpeechBubbles.Show("Shop:LastCustomerTap", "THE LAST CUSTOMER", CustomerLines[customerLine++ % CustomerLines.Length],
                () => new Vector3(world.center.x, world.yMax, 0), SpeechTone.Story, 4, 2);
        }

        private void Collect()
        {
            entries.Clear();
            WorldShop shop = owner.Shop; RunSnapshot state = owner.Session.State;
            // stations show their own card when you stand at them; the merchant and decorations are not things to use
            // the room's own things, each with its story
            Transform room = owner.transform;
            for (int i = 0; i < room.childCount; i++)
            {
                Transform child = room.GetChild(i);
                if (!child.gameObject.activeInHierarchy) continue;
                SpriteRenderer view = child.GetComponent<SpriteRenderer>();
                if (view == null || !view.enabled || view.sprite == null || view.color.a < 0.2f) continue;
                string id = child.name;
                if (id.StartsWith("SecretInvitation_", StringComparison.Ordinal) && Enum.TryParse(id.Substring(17), out SecretEventKind kind))
                    Add(id, SecretEventAssets.Titles[(int)kind], SecretEventAssets.Hints[(int)kind] + " A secret room: a deal, a trade or a fight. Losing a fight there ends the run.",
                        "Step beside it to enter", Ink(view), view.sortingOrder);
                else if (id == "TheUnoccupiedChair")
                    Add(id, "THE WARM CHAIR", "It is always a little warm, as if someone has only just stood up. The shopkeeper: “Not that one. Please. If you sit in it, you are in there alone with it — and if it keeps you, you do not get up.”",
                        owner.Session.SitCloserAvailable ? "Sit · a hidden fight; losing ends the run" : state.Wave < ActOneCanon.SitFrom ? "Not yet. It moves a little closer every night." : "Only a chair now.", Ink(view), view.sortingOrder);
                else if (id.StartsWith("ActOne-LastCustomer", StringComparison.Ordinal) || id == ActOneCustomerVisits.FigureName)
                    Add(id, "THE LAST CUSTOMER", id.EndsWith("BorrowedFace", StringComparison.Ordinal) ? "It stands by the warm chair wearing a face you almost know. It has been buying all night, and it has never once paid." : "A pleasant stranger who stands by the warm chair. It buys what nobody should sell, and it would like to buy something from you.",
                        state.Wave == 8 ? "It will make you an offer once you have read the telephone page" : "It only waits", Ink(view), view.sortingOrder);
                else if (id.StartsWith("ActOne-" + ActOneCanon.CompanionFigure, StringComparison.Ordinal) && Array.IndexOf(ActOneCanon.StoryNames, id.Substring(7 + ActOneCanon.CompanionFigure.Length)) is int who && who >= 0)
                    Add(id, ActOneCanon.StoryNames[who].ToUpperInvariant(), ActOneCanon.StoryLines[who], "Travels with you; it does not fight", Ink(view), view.sortingOrder);
                else if (id == "TheHandWaits")
                    Add(id, "THE KEEPER, WHOLE", "An optional boss. Break its hand, carpet and eye and it kneels.\nWin: pick one rainbow gift.\nLast 3 minutes: no gift, no harm.\nFall: the run ends.", "Step up to it to take the test", Ink(view), view.sortingOrder);
                else if (id.StartsWith("PrismaticReward", StringComparison.Ordinal))
                    Add(id, "AN IMPOSSIBLE GIFT", "The Keeper's reward. Take one of the three; the others go back into the dark.", "Step up to it to take it", Ink(view), view.sortingOrder);
            }
            // the shopkeeper (or the dream shop's keeper) answers a tap with a line of chat
            ShopDisplayActor keeper = owner.GetComponentInChildren<ShopDisplayActor>();
            if (keeper != null && keeper.isActiveAndEnabled)
            {
                Rect body = Rect.zero;
                foreach (SpriteRenderer piece in keeper.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (!piece.enabled || piece.sprite == null && piece.bounds.size.x <= 0 || piece.GetComponent<GroundShadow>() != null || piece.color.a < .2f) continue;
                    Bounds b = piece.bounds; Rect r = Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y);
                    body = body.width <= 0 ? r : Rect.MinMaxRect(Mathf.Min(body.xMin, r.xMin), Mathf.Min(body.yMin, r.yMin), Mathf.Max(body.xMax, r.xMax), Mathf.Max(body.yMax, r.yMax));
                }
                bool dream = shop.Special;
                if (body.width > 0) Add("Shopkeeper", dream ? "MOTHER OF A THOUSAND EGG-EYES" : "THE SHOPKEEPER",
                    dream ? "She keeps the dream shop. Her eyes open only when someone remembers something." : "He keeps the night market between the waves, and he has seen every traveller who ever came through.",
                    "Tap to chat", body, 0);
            }
            // a sold stall's ticket or a hidden prop never answers; nothing else is placed without a card
        }
        private void Landmark(int index, SpriteRenderer view, RunSnapshot state)
        {
            string id = "DreamLandmark" + index, sprite = view.sprite.name;
            switch (index)
            {
                case 0: Add(id, "AN EMPTY CHAIR", "Do not mind the chair. It has always been empty.", "It only waits", Ink(view), view.sortingOrder); break;
                case 1: Add(id, "THE COUNTER LAMP", "A lamp, a teacup and a sprout by the counter. “I kept the lamp lit. I did not know what else to do.” Its warm light is the one the whole shop is read by.", "It keeps burning", Ink(view), view.sortingOrder); break;
                case 2: Add(id, "THE JOURNAL WINDOW", "A lit shop window that shows the room you are standing in. Your journal is kept behind the glass: every page the night has written about you, and the ones still waiting to be read.", "Open it to read your journal", Ink(view), view.sortingOrder); break;
                case 4: Add(id, "THE CLOSED CURTAIN", "Heavy curtains over a stage nobody uses. There is applause behind them, though nobody has come to watch." + (NightmareRules.Depth(state.Wave) >= 2 ? " Lately an eye watches from the fold; it is afraid to close." : ""), "It only listens", Ink(view), view.sortingOrder); break;
                case 5: Add(id, "THE BLANK SIGN", "A hanging sign with nothing written on it yet." + (NightmareRules.Depth(state.Wave) >= 2 ? " A small figure that looks like you has started sitting on it: a place being kept for you." : ""), "It only waits", Ink(view), view.sortingOrder); break;
                case 6: Add(id, "THE WOODEN HEART", "Taken gently from the drawer in the lost-property vault. The cabinet remembers that you were kind to it.", "A keepsake from a secret room", Ink(view), view.sortingOrder); break;
                case 7: Add(id, "THE LETTER BIRD", "The letter you sent from the dead-letter office, folded into a bird. It has an address beyond these shelves.", "A keepsake from a secret room", Ink(view), view.sortingOrder); break;
                default: Add(id, "A SHELF", "The shelf is dreaming of a spine.", "It only waits", Ink(view), view.sortingOrder); break;
            }
        }
        private static string Price(int price, ShopCurrency currency) => price + (currency == ShopCurrency.Eggs ? " eggs" : currency == ShopCurrency.Health ? " max HP" : " gold"); // round 4: an HP price is taken from maximum HP for the run
        // light and particles around a stall (rarity bloom, its light, drifting motes) are not the thing itself: left out of its box
        private static bool Decorative(string name) => name.Contains("Contact") || name.Contains("Shadow") || name.Contains("Glow") || name.Contains("Halo") || name.Contains("Aura") || name.Contains("Floor")
            || name.Contains("Bloom") || name.Contains("Mote") || name.Contains("RarityLight");
        private void Add(string key, string heading, string text, string use, Rect world, int order) => entries.Add(new Entry { Key = key, Title = heading, Body = text, Use = use, World = world, Order = order });
        private static Rect? Union(Rect? a, Rect b) => a == null ? b : Rect.MinMaxRect(Mathf.Min(a.Value.xMin, b.xMin), Mathf.Min(a.Value.yMin, b.yMin), Mathf.Max(a.Value.xMax, b.xMax), Mathf.Max(a.Value.yMax, b.yMax));
        /// <summary>The world box of the sprite's painted ink (not its transparent margins).</summary>
        public static Rect Ink(SpriteRenderer view)
        {
            Rect ink = SpriteInkMetrics.BoundsOf(view.sprite); if (view.flipX) ink.x = -ink.xMax;
            Vector3 a = view.transform.TransformPoint(new Vector3(ink.xMin, ink.yMin)), b = view.transform.TransformPoint(new Vector3(ink.xMax, ink.yMax));
            Vector3 c = view.transform.TransformPoint(new Vector3(ink.xMin, ink.yMax)), d = view.transform.TransformPoint(new Vector3(ink.xMax, ink.yMin));
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x, c.x, d.x), Mathf.Min(a.y, b.y, c.y, d.y), Mathf.Max(a.x, b.x, c.x, d.x), Mathf.Max(a.y, b.y, c.y, d.y));
        }
        /// <summary>The entry drawn on top at this screen point (1920x1080 design space): the smallest box wins.</summary>
        public Entry At(Vector2 design, Camera camera)
        {
            Entry best = null; float area = float.PositiveInfinity;
            foreach (Entry entry in entries)
            {
                Rect screen = Screen(entry.World, camera);
                if (!screen.Contains(design)) continue;
                float size = screen.width * screen.height;
                if (size < area || Mathf.Approximately(size, area) && best != null && entry.Order > best.Order) { area = size; best = entry; }
            }
            return best;
        }
        public static Rect Screen(Rect world, Camera camera)
        {
            Vector3 min = camera.WorldToViewportPoint(new Vector3(world.xMin, world.yMin)), max = camera.WorldToViewportPoint(new Vector3(world.xMax, world.yMax));
            return Rect.MinMaxRect(min.x * 1920, (1 - max.y) * 1080, max.x * 1920, (1 - min.y) * 1080);
        }

        private void OnGUI()
        {
            if (!Allowed || entries.Count == 0) return;
            Camera camera = Camera.main; if (camera == null) return;
            GUI.depth = open?.Offer != null ? -1 : 5; // purchase cards must also cover the merchant's speech bubble
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            name ??= ArcadeTypography.Style(ArcadeTypography.SizeNote, true); name.alignment = TextAnchor.MiddleCenter;
            title ??= ArcadeTypography.Style(26, true); title.wordWrap = true;
            body ??= ArcadeTypography.Style(ArcadeTypography.SizeNote); body.wordWrap = true;
            hint ??= ArcadeTypography.Style(24); hint.wordWrap = true;
            Event current = Event.current;
            Vector2 mouse = current.mousePosition;
            bool overHud = mouse.y < 180 || mouse.y > 1000 || mouse.x < 280 && mouse.y < 300 || mouse.x > 1580 && mouse.y < 260;
            hovered = overHud || RunUIHints.Device != RunUIDevice.KeyboardMouse ? null : At(mouse, camera);
            if (current.type == EventType.MouseDown && current.button == 0)
            {
                // a tap finds what is under the finger on any device (only the hover name is mouse-only): on a phone nothing in
                // the shop answered a tap
                Entry under = overHud ? null : At(mouse, camera);
                if (under != null && (open == null || !CardRect(open, camera).Contains(mouse)))
                {
                    open = open != null && open.Key == under.Key ? null : under; offerScroll = Vector2.zero; openedFor = owner.Simulation.Champion.Position; current.Use();
                    if (open != null) Speak(open);
                }
                else if (open != null && !CardRect(open, camera).Contains(mouse)) open = null;
            }
            if (hovered != null && (open == null || open.Key != hovered.Key))
            {
                Rect box = Screen(hovered.World, camera);
                // the hover marks stop above the shop's description card instead of being drawn over its BUY button
                Rect card = RunHud.ShownShopCard;
                if (card.height > 0 && box.Overlaps(card) && box.yMin < card.yMin) box.yMax = card.yMin - 4;
                if (card.height <= 0 || box.yMin < card.yMin) ArcadeSkin.WorldFocus(box);
                Rect tag = new Rect(Mathf.Clamp(box.center.x - 160, 12, 1588), Mathf.Clamp(box.yMin - 46, 186, 1000), 320, 40);
                if (ArcadeSkin.HasKit) ArcadeSkin.Frame(tag, KitFrame.Plaque); else ArcadeSkin.Panel(tag, ArcadeSkin.Card); // the velvet title plaque (UI kit)
                ArcadeSkin.KitCaption(ArcadeSkin.HasKit ? ArcadeSkin.Content(tag, KitFrame.Plaque, false) : tag, hovered.Title, ArcadeSkin.Cream, name, true);
            }
            Entry by = open == null ? Focused() : null;
            if (current.type == EventType.Repaint) { DebugBy = by?.Title; DebugOpen = open?.Title; }
            if (by?.Key != focused?.Key) focusedAt = Time.unscaledTime; // entries are rebuilt every frame: the same thing keeps its key, not its object
            focused = by;
            if (by != null)
            {
                bool figure = by.Title == "THE LAST CUSTOMER" || by.Key == "Shopkeeper";
                string plain = by.Body; int stop = plain.IndexOf(". ", StringComparison.Ordinal); if (stop > 0 && stop < 110) plain = plain.Substring(0, stop + 1);
                if (ShopPrompt.Draw(by.Title, plain, figure ? "TALK" : "LOOK", Time.unscaledTime - focusedAt))
                { open = by; offerScroll = Vector2.zero; openedFor = owner.Simulation.Champion.Position; Speak(by); }
            }
            if (open?.Key != shownKey) { shownKey = open?.Key; shownAt = Time.unscaledTime; }
            Color tint = GUI.color;
            if (open != null && !PresentationSettings.ReducedMotion)
            {
                // the card pops in: rises from 92% with a small overshoot and fades up over 0.22 s
                float t = Mathf.Clamp01((Time.unscaledTime - shownAt) / 0.22f), back = 1 + 2.2f * Mathf.Pow(t - 1, 3) + 1.2f * Mathf.Pow(t - 1, 2);
                Rect pivot = CardRect(open, camera);
                ArcadeSkin.ScaleAround(Vector2.one * (0.92f + 0.08f * back), pivot.center + Vector2.up * (1 - t) * 14);
                GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * Mathf.Clamp01(t * 1.6f));
            }
            if (open != null)
            {
                if (open.Offer != null)
                {
                    DrawOffer(camera); GUI.matrix = previous; GUI.color = tint; return;
                }
                Rect card = CardRect(open, camera);
                if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .55f);
                else { ArcadeSkin.Panel(card, ArcadeSkin.CardLit); ArcadeSkin.Line(new Rect(card.x, card.y, card.width, 3), ArcadeSkin.Accent); }
                float y = card.y + 18, width = card.width - 40;
                title.normal.textColor = ArcadeSkin.Gold; float h = title.CalcHeight(new GUIContent(open.Title), width); GUI.Label(new Rect(card.x + 20, y, width, h), open.Title, title); y += h + 8;
                body.normal.textColor = ArcadeSkin.Cream; h = body.CalcHeight(new GUIContent(open.Body), width); GUI.Label(new Rect(card.x + 20, y, width, h), open.Body, body); y += h + 12;
                hint.normal.textColor = ArcadeSkin.Teal; h = hint.CalcHeight(new GUIContent(open.Use), width); GUI.Label(new Rect(card.x + 20, y, width, h), open.Use, hint);
            }
            GUI.matrix = previous; GUI.color = tint;
        }
        private Rect CardRect(Entry entry, Camera camera)
        {
            if (entry.Offer != null) return new Rect(485, 190, 950, 430);
            const float width = 560;
            float inner = width - 40;
            title ??= ArcadeTypography.Style(ArcadeTypography.SizeBody, true); body ??= ArcadeTypography.Style(ArcadeTypography.SizeNote); hint ??= ArcadeTypography.Style(24);
            float height = 18 + title.CalcHeight(new GUIContent(entry.Title), inner) + 8 + body.CalcHeight(new GUIContent(entry.Body), inner) + 12 + hint.CalcHeight(new GUIContent(entry.Use), inner) + 20;
            Rect box = Screen(entry.World, camera);
            float x = box.center.x < 960 ? box.xMax + 24 : box.xMin - 24 - width; // beside the thing, toward the middle of the room
            return new Rect(Mathf.Clamp(x, 16, 1904 - width), Mathf.Clamp(box.center.y - height * 0.5f, 190, 990 - height), width, height);
        }
        private void DrawOffer(Camera camera)
        {
            offerCard.Refresh(owner.Session, owner.Content, open.Offer);
            Rect card = CardRect(open, camera);
            RarityDescriptionFrame.Draw(card, offerCard.Rarity, owner.PresentationClock); if (open.Offer.Currency == ShopCurrency.Health) BloodPriceFrame.Draw(card, owner.PresentationClock);
            RarityDescriptionFrame.Backdrop(new Rect(card.x + 36, card.y + 20, 42, 42), offerCard.Rarity, owner.PresentationClock);
            ShopDescriptionCard.DrawSprite(new Rect(card.x + 36, card.y + 20, 42, 42), offerCard.Portrait); // inside the frame's padding
            title.normal.textColor = RunModifierRules.RarityColor(offerCard.Rarity);
            GUI.Label(new Rect(card.x + 90, card.y + 16, 760, 37), open.Title, title);
            hint.normal.textColor = ArcadeSkin.Teal; GUI.Label(new Rect(card.x + 90, card.y + 52, 760, 24), offerCard.Identity, hint);
            if (ArcadeSkin.Medallion(new Rect(card.xMax - 74, card.y + 6, 64, 64), KitMedallion.Close)) { Close(); return; } // painted close medallion, a full tap target
            float header = offerCard.HeaderHeight;
            offerCard.DrawHeader(new Rect(card.x + 36, card.y + 86, card.width - 72, header), owner.PresentationClock);
            float y = card.y + 86 + header + 12, width = card.width - 102;
            string copy = offerCard.Details + "\n\n" + open.Use;
            body.normal.textColor = ArcadeSkin.Cream;
            float height = GlossaryUI.Height(copy, body, width);
            offerScroll = ScrollPanel.Begin(new Rect(card.x + 36, y, card.width - 72, card.yMax - y - 24), offerScroll, new Rect(0, 0, width, height));
            GlossaryUI.Text(new Rect(0, 0, width, height), copy, body); ScrollPanel.End();
            GlossaryUI.Draw(new Vector2(1920, 1080));
        }
    }
}
