using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A shop visitor's card (Docs/Events_Ideas.md; reworked from play-test feedback 19/10/2026): the visitor, its name
    /// and one dimmed line, then its choices as big plates that say only what to DO, what you PAY (red) and what you GET (green),
    /// with a neutral WHEN chip. A relic or friend pick shows that deal once above the pictures of the relics or friends. The
    /// visitor's full offer waits under the small i. An offer taken whole has its own accept button with the price; "Later"
    /// closes the card and the visitor waits at its table for the rest of the shop. Gold amounts follow the gold curve.
    /// The card is sized to its content and centred.</summary>
    public sealed class VisitorCard : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle hand, plain, title, verb, amount, caption, chip, plateName, button, mark;
        private bool shown, details; private int page;
        // a choice is picked first and taken with CONFIRM (owner 22/10/2026: one tap took it before it could be read)
        private int picked = -1;
        private readonly ShopDescriptionCard pickedCard = new ShopDescriptionCard();
        private readonly ShopOffer pickedOffer = new ShopOffer();
        private const float InfoH = 78;
        public int InputConsumedFrame { get; private set; } = -1;
        public void Configure(ArenaController value) => owner = value;
        public bool Open => shown && owner?.Session?.Visitor != null && !owner.StoryConsumesInput && !owner.TransitionPending;
        public void Show() { shown = true; page = 0; details = false; picked = -1; InputConsumedFrame = Time.frameCount; openedAt = Time.unscaledTime; closingAt = -1; }
        // any exit but accepting (LATER, Esc / B, a tap outside the card) only closes the card: the visitor keeps its table for
        // the whole shop and its card opens again (owner, 22/10/2026); the card shrinks and fades out first
        private void Close() { if (closingAt < 0) closingAt = Time.unscaledTime; InputConsumedFrame = Time.frameCount; }
        private float openedAt = -10, closingAt = -1;
        private const float OpenTime = .25f, CloseTime = .16f;
        /// <summary>The open overshoot (as the shop's inspect card): rises past 1 a little, then settles.</summary>
        private static float Overshoot(float t) => 1 + 2.2f * Mathf.Pow(t - 1, 3) + 1.2f * Mathf.Pow(t - 1, 2);

        private static readonly Color PayTone = new Color(1f, .62f, .55f), PayBack = new Color(.50f, .11f, .12f, .95f);
        private static readonly Color GetTone = new Color(.62f, .92f, .66f), GetBack = new Color(.14f, .40f, .25f, .95f);
        private static readonly Color Brown = new Color(.25f, .16f, .1f), Greyed = new Color(.5f, .48f, .45f);
        private const float Line = 46, Glyph = 38, Head = 150, ButtonH = 92;
        private const float SlotW = 152, SlotH = 232, SlotGap = 14;

        /// <summary>Pictures (relics, friends, weapons) go five to a page, four when there are more and arrows are needed.</summary>
        private static int PerPage(int count) => count > 5 ? 4 : 5;

        private void Update()
        {
            if (closingAt >= 0) { if (Time.unscaledTime - closingAt >= CloseTime) { shown = false; closingAt = -1; } return; }
            if (!Open || owner.PresentationPaused) { if (owner?.Session?.Visitor == null) shown = false; return; }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) { Close(); return; }
            List<VisitorOption> options = owner.Session.VisitorOptions();
            int per = PerPage(options.Count), first = page * per;
            for (int i = 0; i < Mathf.Min(per, options.Count - first); i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) { Pick(first + i, options); return; }
            if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && picked >= 0 && picked < options.Count && options[picked].Enabled) Choose(picked);
        }
        private void Pick(int index, List<VisitorOption> options)
        {
            if (index < 0 || index >= options.Count || !options[index].Enabled) return;
            if (picked != index) CheckpointAudio.PlayInterfaceId("ui.select");
            picked = index;
        }
        private void Choose(int index)
        {
            picked = -1;
            InputConsumedFrame = Time.frameCount;
            if (owner.VisitorChoose(index) && owner.Session.Visitor == null) shown = false;
        }
        private Sprite IconFor(VisitorOption o)
        {
            if (string.IsNullOrEmpty(o.Icon)) return null;
            int cut = o.Icon.IndexOf(':'); string kind = o.Icon.Substring(0, cut), id = o.Icon.Substring(cut + 1);
            return kind == "item" ? owner.Content.Item(id)?.Icon : kind == "unit" ? owner.Content.Unit(id)?.Body : owner.Content.Weapon(id)?.Sprite;
        }
        private string NameFor(VisitorOption o)
        {
            if (string.IsNullOrEmpty(o.Icon)) return o.Label;
            int cut = o.Icon.IndexOf(':'); string kind = o.Icon.Substring(0, cut), id = o.Icon.Substring(cut + 1);
            if (kind == "item") return owner.Content.Item(id)?.Name ?? o.Label;
            if (kind == "unit") { UnitDefinition u = owner.Content.Unit(id); return u != null ? RunUICopy.UnitName(u) : o.Label; }
            return RunUICopy.WeaponName(id);
        }

        private void Styles()
        {
            if (hand != null) return;
            hand = ArcadeTypography.Style(28); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.UpperLeft; hand.wordWrap = true;
            plain = ArcadeTypography.Style(22); plain.alignment = TextAnchor.UpperLeft; plain.wordWrap = true;
            title = ArcadeTypography.Style(34, true); title.alignment = TextAnchor.MiddleLeft; title.wordWrap = false; title.clipping = TextClipping.Clip;
            verb = ArcadeTypography.Style(28, true); verb.alignment = TextAnchor.MiddleCenter; verb.wordWrap = true;
            amount = ArcadeTypography.Style(24, true); amount.alignment = TextAnchor.MiddleLeft; amount.wordWrap = false;
            caption = ArcadeTypography.Style(15, true); caption.alignment = TextAnchor.MiddleCenter; caption.wordWrap = false; caption.normal.textColor = ArcadeSkin.Cream;
            chip = ArcadeTypography.Style(19, true); chip.alignment = TextAnchor.MiddleCenter; chip.wordWrap = false;
            plateName = ArcadeTypography.Style(18, true); plateName.alignment = TextAnchor.MiddleCenter; plateName.wordWrap = true;
            button = ArcadeTypography.Style(24, true); button.alignment = TextAnchor.MiddleCenter; button.normal.textColor = ArcadeSkin.Cream;
            mark = ArcadeTypography.Style(26, true); mark.alignment = TextAnchor.MiddleCenter; mark.wordWrap = false;
        }

        private void OnGUI()
        {
            if (!Open || owner.PresentationPaused || Camera.main == null) return;
            // above the HUD, its tickets and the speech bubbles; left set (a behaviour's depth is the one it ends its OnGUI with)
            GUI.depth = -60;
            VisitorDefinition v = owner.Session.Visitor;
            RunSnapshot state = owner.Session.State;
            List<VisitorOption> options = owner.Session.VisitorOptions();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            Styles();
            bool pictures = false; foreach (VisitorOption o in options) if (!string.IsNullOrEmpty(o.Icon)) pictures = true;
            const float width = 870;
            float content = pictures ? DrawPick(v, state, options, 0, 0, width, false) : DrawPlates(v, state, options, 0, 0, width, false);
            float height = Mathf.Max(560, Head + content + 36 + ButtonH + 32 + (pictures ? InfoH : 0));
            Rect card = new Rect(360, Mathf.Round((1080 - height) * .5f + 20), 1200, height);

            // the card opens from a little below with a slight overshoot and closes shrinking; Reduced Motion: a fade only
            bool still = PresentationSettings.ReducedMotion;
            float tOpen = Mathf.Clamp01((Time.unscaledTime - openedAt) / OpenTime), tClose = closingAt >= 0 ? Mathf.Clamp01((Time.unscaledTime - closingAt) / CloseTime) : 0;
            float fade = Mathf.Clamp01(tOpen * 1.6f) * (1 - tClose);
            Color tint = GUI.color; bool wasEnabled = GUI.enabled;
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, 0.5f * fade));
            GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * fade);
            if (!still) GUIUtility.ScaleAroundPivot(Vector2.one * (closingAt >= 0 ? Mathf.Lerp(1, .9f, tClose) : .88f + .12f * Overshoot(tOpen)), card.center + Vector2.up * ((1 - tOpen) * 18 + tClose * 10));
            if (closingAt >= 0) GUI.enabled = false; // no choice is taken while the card closes
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Wax);
            Sprite portrait = ExpansionArtwork.Current?.Visitor(v.Id, 1) ?? ExpansionArtwork.Current?.Visitor(v.Id, 0);
            Rect inner = ArcadeSkin.Content(card, KitFrame.Card); // inside the paper's rim and lantern corners
            ShopDescriptionCard.DrawSprite(new Rect(inner.x, inner.y + 10, 250, 320), portrait);
            float x = card.x + 300;

            // who: the name, one dimmed line in the visitor's hand (the full offer waits under the i)
            title.normal.textColor = Brown;
            GUI.Label(new Rect(x, inner.y - 6, width - 90, 50), v.Name, title);
            Rect info = new Rect(inner.xMax - 66, inner.y - 6, 66, 66);
            if (ArcadeSkin.Medallion(info, KitMedallion.Info, details)) details = !details; // the painted brass "i" (UI kit)
            if (details) { plain.normal.textColor = new Color(.18f, .14f, .1f); GUI.Label(new Rect(x, card.y + 76, width - 20, 70), ShopVisitors.ScaleText(v.Offer, owner.Session.GoldScale), plain); }
            else { hand.normal.textColor = new Color(.30f, .22f, .16f, .75f); GUI.Label(new Rect(x, card.y + 78, width - 20, 66), "“" + v.Line + "”", hand); }

            if (pictures) DrawPick(v, state, options, x, card.y + Head, width, true);
            else DrawPlates(v, state, options, x, card.y + Head, width, true);

            // the buttons (play-test 20/10/2026): an offer taken as a whole always shows its accept button with the price,
            // greyed with "Not enough gold" when short; one "Later" closes the card (the visitor waits at its table all shop)
            float buttonsY = inner.yMax - ButtonH;
            Rect later;
            if (!pictures && v.Choice == VisitorChoice.Accept && options.Count == 1)
            {
                Rect accept = new Rect(x + (width - 728) * .5f, buttonsY, 430, ButtonH);
                later = new Rect(accept.xMax + 48, buttonsY, 250, ButtonH); // clear of the gold button's caps
                AcceptButton(accept, v, state, options[0]);
            }
            else
            {
                // pick, read what it is, then CONFIRM; LATER beside it, clear of the gold button's caps
                bool ok = picked >= 0 && picked < options.Count && options[picked].Enabled;
                if (pictures) PickedInfo(new Rect(x, buttonsY - InfoH - 8, width, InfoH), ok ? options[picked] : null);
                Rect confirm = new Rect(x + (width - 728) * .5f, buttonsY, 430, ButtonH);
                later = new Rect(confirm.xMax + 48, buttonsY, 250, ButtonH);
                if (ArcadeSkin.Button(confirm, ok ? "CONFIRM" : "PICK ONE", KitButton.Primary, ok, button)) Choose(picked);
            }
            if (ArcadeSkin.Button(later, "LATER", KitButton.Secondary, true, button)) Close();
            // a tap outside the card closes it too (the visitor stays at its table)
            if (closingAt < 0 && Event.current.type == EventType.MouseDown && !card.Contains(Event.current.mousePosition)) { Close(); Event.current.Use(); }
            GUI.enabled = wasEnabled; GUI.color = tint;
            GUI.matrix = previous;
        }

        /// <summary>What the picked relic or friend does, in two short lines (the shop card's own summary), or a hint to pick.</summary>
        private void PickedInfo(Rect r, VisitorOption o)
        {
            ArcadeSkin.Plate(r, new Color(.2f, .14f, .09f, .9f));
            Rect inner = new Rect(r.x + 20, r.y + 10, r.width - 40, r.height - 20);
            plain.normal.textColor = ArcadeSkin.Cream;
            if (o == null) { GUI.Label(inner, "Tap a picture to see what it does.", plain); return; }
            string text = NameFor(o);
            int cut = string.IsNullOrEmpty(o.Icon) ? -1 : o.Icon.IndexOf(':');
            string kind = cut > 0 ? o.Icon.Substring(0, cut) : "", id = cut > 0 ? o.Icon.Substring(cut + 1) : "";
            if ((kind == "item" && owner.Content.Item(id) != null) || (kind == "unit" && owner.Content.Unit(id) != null))
            {
                pickedOffer.Kind = kind == "item" ? OfferKind.Item : OfferKind.Companion; pickedOffer.Id = id; pickedOffer.Tier = Mathf.Max(1, o.Tier); pickedOffer.Token = -77; pickedOffer.Price = 0;
                pickedCard.Refresh(owner.Session, owner.Content, pickedOffer);
                string[] lines = (pickedCard.Summary ?? "").Replace("\r", "").Split('\n');
                text += ": " + string.Join("  ", lines, 0, Mathf.Min(2, lines.Length)).Replace("{mark} ", "");
            }
            GUI.Label(inner, text, plain);
        }
        /// <summary>The accept button of a take-it-or-leave-it offer: its verb and its price with the HUD coin.</summary>
        private void AcceptButton(Rect r, VisitorDefinition v, RunSnapshot state, VisitorOption option)
        {
            int cost = owner.Session.VisitorCost(v);
            VisitorDeal deal = ShopVisitors.DealFor(v, 0, state.Wave, owner.Session.GoldScale);
            bool poor = cost > state.Gold;
            // the painted gold button (UI kit): ink lettering on the gold face, the grey-brown face when it cannot be taken
            bool hover = option.Enabled && r.Contains(Event.current.mousePosition);
            ArcadeSkin.ButtonArt(r, KitButton.Primary, !option.Enabled ? 3 : hover && GUIUtility.hotControl != 0 ? 2 : hover ? 1 : 0);
            string caption = poor ? "NOT ENOUGH GOLD" : (deal?.Do ?? "Accept").ToUpperInvariant();
            float priceW = 0;
            if (cost > 0)
            {
                string text = cost.ToString("N0"); float g = Glyph;
                priceW = g + 8 + amount.CalcSize(new GUIContent(text)).x + 18;
                Rect priceArea = ArcadeSkin.Content(r, KitPiece.Button); // the price inside the button's content
                float left = priceArea.xMax - priceW + 18;
                Icon(new Rect(left, r.y + (r.height - g) * .5f, g, g), "gold");
                amount.normal.textColor = poor ? new Color(.48f, .06f, .07f) : ArcadeSkin.KitInk;
                GUI.Label(new Rect(left + g + 8, r.y, priceW - g - 8, r.height), text, amount);
            }
            Rect wordsArea = ArcadeSkin.Content(r, KitPiece.Button);
            ArcadeSkin.KitCaption(new Rect(wordsArea.x, r.y, wordsArea.width - priceW, r.height), caption, ArcadeSkin.KitText(KitButton.Primary, option.Enabled), button);
            if (option.Enabled && GUI.Button(r, GUIContent.none, GUIStyle.none)) Choose(0);
        }

        // ---- choices that are actions: one plate each (DO, PAY, GET, WHEN). Returns the height used.
        private float DrawPlates(VisitorDefinition v, RunSnapshot state, List<VisitorOption> options, float x, float top, float width, bool draw)
        {
            int n = Mathf.Max(1, options.Count); const float gap = 18;
            float plateW = n == 1 ? 740 : (width - gap * (n - 1)) / n, inner = ArcadeSkin.Content(new Rect(0, 0, plateW, 240), KitPiece.Dialogue).width;
            float left = n == 1 ? x + (width - plateW) * .5f : x;
            var deals = new VisitorDeal[options.Count];
            float verbH = 0, payH = 0, getH = 0; bool anyWhen = false;
            for (int i = 0; i < options.Count; i++)
            {
                VisitorDeal d = deals[i] = ShopVisitors.DealFor(v, options[i].Value, state.Wave, owner.Session.GoldScale) ?? new VisitorDeal(options[i].Label, VisitorDeal.Free, "");
                verbH = Mathf.Max(verbH, verb.CalcHeight(new GUIContent(d.Do), inner));
                payH = Mathf.Max(payH, Row(new Rect(0, 0, inner, 0), "PAY", d.Pay, true, false));
                getH = Mathf.Max(getH, Row(new Rect(0, 0, inner, 0), "GET", d.Get, false, false));
                anyWhen |= !string.IsNullOrEmpty(d.When);
            }
            float chrome = 240 - ArcadeSkin.Content(new Rect(0, 0, plateW, 240), KitPiece.Dialogue).height; // the frame's rim and padding
            float height = chrome + verbH + 12 + payH + 8 + getH + (anyWhen ? 14 + 40 : 0);
            if (!draw) return height;
            for (int i = 0; i < options.Count; i++)
            {
                VisitorOption o = options[i]; VisitorDeal d = deals[i];
                Rect plate = new Rect(left + i * (plateW + gap), top, plateW, height);
                bool hover = o.Enabled && plate.Contains(Event.current.mousePosition);
                Color keep = GUI.color; if (!o.Enabled) GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * .5f);
                ArcadeSkin.Plate(plate, hover ? ArcadeSkin.Gold : o.Enabled ? ArcadeSkin.Accent : Greyed);
                if (hover) ArcadeSkin.WorldFocus(plate);
                Rect pc = ArcadeSkin.Content(plate, KitPiece.Dialogue);
                float y = pc.y;
                verb.normal.textColor = o.Enabled ? ArcadeSkin.Cream : new Color(.8f, .78f, .75f);
                GUI.Label(new Rect(pc.x, y, inner, verbH), d.Do, verb); y += verbH + 12;
                Row(new Rect(pc.x, y, inner, payH), "PAY", d.Pay, true, true); y += payH + 8;
                Row(new Rect(pc.x, y, inner, getH), "GET", d.Get, false, true); y += getH + 14;
                if (!string.IsNullOrEmpty(d.When)) When(new Rect(pc.x, y, pc.width, 40), d.When);
                GUI.color = keep;
                if (!o.Enabled) Missing(new Rect(plate.xMax - 62, plate.y - 14, 56, 56), v, d, state);
                if (i == picked) { ArcadeSkin.Plate(plate, new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .35f)); ArcadeSkin.WorldFocus(plate); }
                if (o.Enabled && GUI.Button(plate, GUIContent.none, GUIStyle.none)) Pick(i, options);
            }
            return height;
        }

        // ---- choices that are relics, friends or weapons: the deal once, then their pictures. Returns the height used.
        private float DrawPick(VisitorDefinition v, RunSnapshot state, List<VisitorOption> options, float x, float top, float width, bool draw)
        {
            VisitorDeal d = ShopVisitors.DealFor(v, v.Id == "EggTrader" ? 1 : 0, state.Wave, owner.Session.GoldScale);
            float y = top;
            if (d != null)
            {
                float half = (width - 20) * .5f;
                float rowsH = Mathf.Max(Row(new Rect(0, 0, half - 16, 0), "PAY", d.Pay, true, false), Row(new Rect(0, 0, half - 16, 0), "GET", d.Get, false, false));
                Rect strip = new Rect(x, y, width, 12 + 46 + 8 + rowsH + 12);
                if (draw)
                {
                    ArcadeSkin.Plate(strip, ArcadeSkin.Accent);
                    verb.normal.textColor = ArcadeSkin.Cream; verb.alignment = TextAnchor.MiddleLeft;
                    float whenW = string.IsNullOrEmpty(d.When) ? 0 : Mathf.Min(360, chip.CalcSize(new GUIContent(d.When)).x + 36);
                    GUI.Label(new Rect(strip.x + 24, strip.y + 12, strip.width - 48 - whenW, 46), d.Do, verb); verb.alignment = TextAnchor.MiddleCenter;
                    if (whenW > 0) When(new Rect(strip.xMax - 16 - whenW, strip.y + 15, whenW, 40), d.When);
                    Row(new Rect(strip.x + 24, strip.y + 66, half - 16, rowsH), "PAY", d.Pay, true, true);
                    Row(new Rect(strip.x + 24 + half, strip.y + 66, half - 16, rowsH), "GET", d.Get, false, true);
                }
                y = strip.yMax + 18;
            }
            int per = PerPage(options.Count), pages = Mathf.Max(1, (options.Count + per - 1) / per); page = Mathf.Clamp(page, 0, pages - 1);
            if (!draw) return y - top + SlotH;
            int first = page * per, count = Mathf.Min(per, options.Count - first);
            for (int i = 0; i < count; i++)
            {
                VisitorOption o = options[first + i];
                Rect slot = new Rect(x + i * (SlotW + SlotGap), y, SlotW, SlotH);
                bool hover = o.Enabled && slot.Contains(Event.current.mousePosition);
                Color keep = GUI.color; if (!o.Enabled) GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * .5f);
                ArcadeSkin.Plate(slot, hover ? ArcadeSkin.Gold : o.Enabled ? ArcadeSkin.Accent : Greyed);
                if (hover) ArcadeSkin.WorldFocus(slot);
                Sprite icon = IconFor(o);
                if (icon != null) ShopDescriptionCard.DrawSprite(new Rect(slot.x + 16, slot.y + 12, slot.width - 32, 112), icon);
                plateName.normal.textColor = o.Enabled ? ArcadeSkin.Cream : new Color(.8f, .78f, .75f);
                GUI.Label(new Rect(slot.x + 6, slot.y + 128, slot.width - 12, 52), NameFor(o), plateName);
                Rect foot = new Rect(slot.x + 8, slot.y + 182, slot.width - 16, 40);
                if (o.Gold > 0 || v.Id == "JunkCollector" || v.Id == "Pawnbroker")
                {
                    string text = "+" + o.Gold; float g = Glyph * .85f, w = g + 6 + amount.CalcSize(new GUIContent(text)).x, l = foot.center.x - w * .5f;
                    Icon(new Rect(l, foot.y + (foot.height - g) * .5f, g, g), "gold");
                    amount.normal.textColor = GetTone; GUI.Label(new Rect(l + g + 6, foot.y, w, foot.height), text, amount);
                }
                else if (o.Tier > 0) { chip.fontSize = 22; chip.normal.textColor = ArcadeSkin.Gold; GUI.Label(foot, new string('★', Mathf.Clamp(o.Tier, 1, 3)), chip); chip.fontSize = 19; }
                GUI.color = keep;
                if (!o.Enabled && d != null) Missing(new Rect(slot.xMax - 50, slot.y - 12, 50, 50), v, d, state);
                if (first + i == picked) ArcadeSkin.WorldFocus(slot);
                if (o.Enabled && GUI.Button(slot, GUIContent.none, GUIStyle.none)) Pick(first + i, options);
            }
            if (pages > 1)
            {
                float ax = x + per * (SlotW + SlotGap) + 6, aw = Mathf.Min(100, (x + width - ax - 12) * .5f);
                Rect prev = new Rect(ax, y + SlotH * .5f - 46, aw, 92), next = new Rect(ax + aw + 12, prev.y, aw, 92);
                if (ArcadeSkin.IconButton(prev, KitRound.Walnut, "◀", null, true, mark)) page = (page + pages - 1) % pages;
                if (ArcadeSkin.IconButton(next, KitRound.Walnut, "▶", null, true, mark)) page = (page + 1) % pages;
                caption.normal.textColor = Brown; GUI.Label(new Rect(ax, prev.yMax + 8, aw * 2 + 12, 24), (page + 1) + " / " + pages, caption); caption.normal.textColor = ArcadeSkin.Cream;
            }
            return y - top + SlotH;
        }

        // ---- the pieces
        /// <summary>A PAY or GET row: a small coloured tag, then each term as its icon and amount (wrapping), "+" between
        /// terms paid or won together and "or" between alternatives. Returns the height used.</summary>
        private float Row(Rect area, string tag, string row, bool pay, bool draw)
        {
            const float tagW = 58, tagGap = 10;
            Color tone = pay ? PayTone : GetTone;
            if (draw)
            {
                Rect pill = new Rect(area.x, area.y + (Line - 28) * .5f, tagW, 28);
                ArcadeSkin.Plate(pill, pay ? ArcadeSkin.Wax : ArcadeSkin.Teal); GUI.Label(pill, tag, caption);
            }
            float left = area.x + tagW + tagGap, cx = left, cy = area.y;
            List<VisitorTerm> terms = ShopVisitors.Terms(row);
            if (terms.Count == 0)
            {
                if (draw) { amount.normal.textColor = new Color(.86f, .84f, .78f); GUI.Label(new Rect(cx, cy, area.xMax - cx, Line), "Free", amount); }
                return Line;
            }
            for (int i = 0; i < terms.Count; i++)
            {
                VisitorTerm t = terms[i];
                string join = i == 0 ? "" : t.Or ? "or" : "+";
                float joinW = i == 0 ? 0 : (t.Or ? caption.CalcSize(new GUIContent("or")).x + 12 : 22);
                bool icon = !string.IsNullOrEmpty(t.Icon);
                float iconW = icon ? Glyph + 6 : 0, textW = amount.CalcSize(new GUIContent(t.Amount)).x;
                float w = joinW + iconW + textW;
                if (cx > left && cx + w > area.xMax) { cx = left; cy += Line; }
                if (draw)
                {
                    if (joinW > 0)
                    {
                        GUIStyle s = t.Or ? caption : amount; Color c = s.normal.textColor; s.normal.textColor = new Color(tone.r, tone.g, tone.b, .7f);
                        GUI.Label(new Rect(cx, cy, joinW, Line), join, s); s.normal.textColor = c;
                    }
                    float ix = cx + joinW;
                    if (icon) Icon(new Rect(ix, cy + (Line - Glyph) * .5f, Glyph, Glyph), t.Icon);
                    amount.normal.textColor = t.Icon == "relic" && !pay && Enum.TryParse(FirstWord(t.Amount), out ItemRarity rarity) && rarity != ItemRarity.Common ? Color.Lerp(RunModifierRules.RarityColor(rarity), Color.white, .25f) : tone;
                    GUI.Label(new Rect(ix + iconW, cy, Mathf.Min(textW + 4, area.xMax - ix - iconW + 4), Line), t.Amount, amount);
                }
                cx += w + 8;
            }
            return cy + Line - area.y;
        }
        private static string FirstWord(string s) { int sp = s.IndexOf(' '); return sp < 0 ? s : s.Substring(0, sp); }

        /// <summary>The neutral WHEN / IF chip; its words shrink to fit a narrow plate.</summary>
        private void When(Rect r, string when)
        {
            var text = new GUIContent(when);
            chip.fontSize = 19; while (chip.fontSize > 14 && chip.CalcSize(text).x + 24 > r.width) chip.fontSize--;
            float w = Mathf.Min(r.width, chip.CalcSize(text).x + 28);
            Rect c = new Rect(r.x + (r.width - w) * .5f, r.y, w, r.height);
            ArcadeSkin.Slant(c, new Color(1, 1, 1, .1f));
            chip.normal.textColor = new Color(.88f, .85f, .78f);
            GUI.Label(c, when, chip); chip.fontSize = 19;
        }

        /// <summary>A term's picture: coins, eggs, chests and lamps as the game paints them, the hero, a relic gem, a gauge,
        /// an umbrella, a surprise, or a stat's own glyph.</summary>
        private void Icon(Rect r, string icon)
        {
            Sprite sprite = null;
            switch (icon)
            {
                case "gold": case "coin": sprite = owner.Content.Arena != null ? owner.Content.Arena.Coin : null; break;
                case "egg": sprite = owner.Content.EggSprite; break;
                case "chest": sprite = owner.Content.ChestSprite; break;
                case "lamp": sprite = PacingArtwork.Flare(owner.Content); break;
                case "hero": sprite = owner.Content.Unit(owner.Session.State.ChampionId)?.Body; break;
            }
            if (sprite != null) { ShopDescriptionCard.DrawSprite(r, sprite); if (icon == "coin") ArcadeSkin.Glow(new Rect(r.x - 4, r.y - 4, r.width + 8, r.height + 8), new Color(1, .85f, .4f, .25f)); return; }
            switch (icon)
            {
                case "gold": case "coin": StatGlyph.Draw(r, ItemEffect.Income); return;
                case "egg": StatGlyph.Draw(r, ItemEffect.Luck); return;
                case "chest": StatGlyph.Draw(r, ItemEffect.TreasureDetection); return;
                case "relic": case "rarity": case "shelf": StatGlyph.Draw(r, ItemEffect.ShopRarity); return;
                case "friend": case "bond": StatGlyph.Draw(r, ItemEffect.CompanionSize); return;
                case "lure": StatGlyph.Draw(r, ItemEffect.Pickup); return;
                case "time": StatGlyph.Draw(r, ItemEffect.CollisionFrequency); return;
                case "price": StatGlyph.Draw(r, ItemEffect.Discount); return;
                case "weapon": StatGlyph.Draw(r, ItemEffect.MeleeDamage); return;
                case "lamp": StatGlyph.Draw(r, ItemEffect.CollisionDamage); return;
                case "hero": StatGlyph.Draw(r, ItemEffect.BodySize); return;
                case "gauge":
                    for (int i = 0; i < 3; i++)
                    {
                        Rect pip = new Rect(r.x + 2 + i * (r.width - 4) / 3f, r.y + r.height * .3f, (r.width - 4) / 3f - 3, r.height * .4f);
                        ArcadeSkin.Slant(pip, i == 0 ? ArcadeSkin.Gold : new Color(1, 1, 1, .18f));
                    }
                    return;
                case "umbrella":
                {
                    GUI.BeginGroup(new Rect(r.x, r.y + r.height * .1f, r.width, r.height * .42f));
                    ArcadeSkin.Dot(new Rect(r.width * .04f, 0, r.width * .92f, r.height * .84f), ArcadeSkin.Teal);
                    GUI.EndGroup();
                    ArcadeSkin.Line(new Rect(r.center.x - 1.5f, r.y + r.height * .5f, 3, r.height * .36f), ArcadeSkin.Cream);
                    ArcadeSkin.Line(new Rect(r.center.x - r.width * .16f, r.y + r.height * .84f, r.width * .16f + 1.5f, 3), ArcadeSkin.Cream);
                    return;
                }
                case "elite": ArcadeSkin.Dot(r, new Color(.35f, .26f, .08f)); mark.normal.textColor = ArcadeSkin.Gold; GUI.Label(r, "★", mark); return;
                case "?": ArcadeSkin.Dot(r, new Color(.30f, .22f, .45f)); mark.normal.textColor = ArcadeSkin.Gold; GUI.Label(r, "?", mark); return;
            }
            if (Enum.TryParse(icon, out ItemEffect effect)) StatGlyph.Draw(r, effect);
        }

        /// <summary>A greyed plate's reason: the thing that is missing (gold, eggs, a relic, a refresh to undo), crossed out.</summary>
        private void Missing(Rect r, VisitorDefinition v, VisitorDeal d, RunSnapshot state)
        {
            string icon = ShopVisitors.Amount(d.Pay, "gold") > state.Gold ? "gold" : ShopVisitors.Amount(d.Pay, "egg") > state.Eggs ? "egg"
                : d.Pay.Contains("relic") && state.Items.Count == 0 ? "relic" : v.Id == "ClockRewinder" ? "shelf" : "";
            if (icon.Length == 0) return;
            ArcadeSkin.Dot(new Rect(r.x + 2, r.y + 3, r.width, r.height), new Color(0, 0, 0, .35f));
            ArcadeSkin.Dot(r, new Color(.16f, .13f, .12f, .97f));
            Color keep = GUI.color; GUI.color = new Color(keep.r * .75f, keep.g * .75f, keep.b * .75f, keep.a * .8f);
            Icon(new Rect(r.x + r.width * .2f, r.y + r.height * .2f, r.width * .6f, r.height * .6f), icon);
            GUI.color = keep;
            Matrix4x4 m = GUI.matrix; GUIUtility.RotateAroundPivot(-45, r.center);
            ArcadeSkin.Line(new Rect(r.center.x - r.width * .42f, r.center.y - 2.5f, r.width * .84f, 5), ArcadeSkin.Danger);
            GUI.matrix = m;
        }
    }
}
