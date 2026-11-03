using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a visitor's deal actually gave (requirement 25/10/2026: a gift was taken and nothing said what it was). After
    /// every deal the run before and after are compared, and a paper card in the visitor card's own style pops up listing each
    /// thing gained or given: a relic with its stats, a friend, stars, a new weapon, gold, eggs, or the effect the deal starts.
    /// It closes on OK, a tap outside, or after a while.</summary>
    public sealed class VisitorReceipt : MonoBehaviour
    {
        private struct Line { public Sprite Icon; public string Text; public bool Gain; public string Id; public bool Friend; public int Tier; }
        private readonly List<Line> lines = new List<Line>();
        private string title;
        private float shownAt = -1, closeAt = -1;
        // requirement 25/10/2026: the card only opens for a relic or a friend; gold, eggs or an effect is a small notice that
        // slides in under the wave banner and goes by itself
        private bool toast;
        private const float ToastSeconds = 2.8f, CloseSeconds = .18f;
        private ArenaController owner;
        private GUIStyle heading, body, button, receivedStyle, identityStyle;
        public bool Open => shownAt >= 0;

        public static void Record(ArenaController owner, RunSnapshot before, string visitorName, string dealGet)
        {
            var receipt = owner.GetComponent<VisitorReceipt>() ?? owner.gameObject.AddComponent<VisitorReceipt>();
            receipt.owner = owner; receipt.Build(before, visitorName, dealGet);
        }

        private void Build(RunSnapshot before, string visitorName, string dealGet)
        {
            lines.Clear(); cards.Clear(); title = visitorName;
            RunSnapshot after = owner.Session.State; RunCatalog catalog = owner.Content;
            // relics gained and given
            var had = new Dictionary<string, int>(); foreach (ItemStack s in before.Items) had[s.Id] = (had.TryGetValue(s.Id, out int n) ? n : 0) + s.Count;
            var has = new Dictionary<string, int>(); foreach (ItemStack s in after.Items) has[s.Id] = (has.TryGetValue(s.Id, out int n) ? n : 0) + s.Count;
            foreach (var pair in has)
            {
                int gained = pair.Value - (had.TryGetValue(pair.Key, out int n) ? n : 0); ItemDefinition item = catalog.Item(pair.Key);
                if (gained > 0 && item != null) Add(item.Icon, item.Name, true, item.Id, false, 1);
            }
            foreach (var pair in had)
            {
                int lost = pair.Value - (has.TryGetValue(pair.Key, out int n) ? n : 0); ItemDefinition item = catalog.Item(pair.Key);
                if (lost > 0 && item != null) Add(item.Icon, "Gave: " + item.Name, false);
            }
            // friends: new, gone, starred up, re-armed
            foreach (RosterEntry r in after.Roster)
            {
                UnitDefinition unit = catalog.Unit(r.UnitId); if (unit == null) continue;
                RosterEntry old = before.Roster.Find(o => o.UnitId == r.UnitId && o.WeaponId == r.WeaponId) ?? before.Roster.Find(o => o.UnitId == r.UnitId);
                if (old == null) Add(unit.Body, RunUICopy.UnitName(unit), true, unit.Id, true, r.Tier);
                else if (r.Tier > old.Tier) Add(unit.Body, RunUICopy.UnitName(unit) + " · now " + r.Tier + " stars", true, unit.Id, true, r.Tier);
                else if (r.WeaponId != old.WeaponId) Add(catalog.Weapon(r.WeaponId)?.Sprite ?? unit.Body, RunUICopy.UnitName(unit) + " now carries " + RunUICopy.WeaponName(r.WeaponId), true);
            }
            foreach (RosterEntry r in before.Roster)
                if (!after.Roster.Exists(o => o.UnitId == r.UnitId)) { UnitDefinition unit = catalog.Unit(r.UnitId); if (unit != null) Add(unit.Body, "Left: " + RunUICopy.UnitName(unit), false); }
            int major = lines.Count; // relics and friends: worth the card
            // purse
            int gold = after.Gold - before.Gold, eggs = after.Eggs - before.Eggs;
            if (gold != 0) Add(catalog.Arena.Coin, (gold > 0 ? "+" : "−") + Mathf.Abs(gold) + " gold", gold > 0);
            if (eggs != 0) Add(catalog.EggSprite, (eggs > 0 ? "+" : "−") + Mathf.Abs(eggs) + (Mathf.Abs(eggs) == 1 ? " dreamsand" : " dreamsand"), eggs > 0);
            // a deal whose gift is an effect for later says what it starts
            if (lines.Count == 0 || after.VisitorEffects.Count > before.VisitorEffects.Count)
            {
                string notice = owner.Session.Notice;
                string what = !string.IsNullOrEmpty(dealGet) ? dealGet : notice;
                // a raw amount ("egg:1") reads as words with its own icon
                var amount = System.Text.RegularExpressions.Regex.Match(what ?? "", @"^\s*(egg|eggs|gold|heart|hp)\s*:\s*(\d+)\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (amount.Success)
                {
                    string kind = amount.Groups[1].Value.ToLowerInvariant(); int count = int.Parse(amount.Groups[2].Value);
                    bool isEgg = kind.StartsWith("egg"), isGold = kind == "gold";
                    Add(isEgg ? catalog.EggSprite : isGold ? catalog.Arena.Coin : CurrencyIcons.Heart, "+" + count + (isEgg ? count == 1 ? " dreamsand" : " dreamsand" : isGold ? " gold" : " HP"), true);
                }
                else if (!string.IsNullOrEmpty(what)) Add(null, what, true);
            }
            if (lines.Count == 0) Add(null, "Nothing changed.", false);
            toast = major == 0;
            shownAt = Time.unscaledTime; closeAt = -1;
            CheckpointAudio.PlayInterfaceId(toast ? "ui.coach.pop" : "ui.unlock");
        }
        private void Add(Sprite icon, string text, bool gain, string id = null, bool friend = false, int tier = 1) { if (lines.Count < 6) lines.Add(new Line { Icon = icon, Text = text, Gain = gain, Id = id, Friend = friend, Tier = tier }); }

        private void Update()
        {
            if (!Open) return;
            if (owner == null || owner.Session == null || owner.Session.State.Phase != SessionPhase.Shop || Time.unscaledTime - shownAt > (toast ? ToastSeconds : 12)) shownAt = -1;
            else if (!toast && closeAt < 0 && Time.unscaledTime - shownAt > .35f && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))) closeAt = Time.unscaledTime;
            if (closeAt >= 0 && Time.unscaledTime - closeAt > CloseSeconds) { shownAt = -1; closeAt = -1; }
        }

        private void OnGUI()
        {
            if (!Open || owner == null || Camera.main == null) return;
            GUI.depth = -70; // over the visitor card
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (heading == null)
            {
                heading = ArcadeTypography.Style(34, true); heading.alignment = TextAnchor.MiddleCenter; heading.normal.textColor = ArcadeSkin.Ink;
                if (ArcadeTypography.Display != null) heading.font = ArcadeTypography.Display;
                body = ArcadeTypography.Style(24); body.alignment = TextAnchor.MiddleLeft; body.wordWrap = true;
                button = ArcadeSkin.Primary(new GUIStyle(ArcadeSkin.Skin.button) { fontSize = 26 });
            }
            if (toast) { DrawToast(); GUI.matrix = previous; return; }
            // requirement 25/10/2026: the card is the shop's own description frame, and each relic or friend received shows its
            // full description (the same lines as its shop card); gold, eggs or anything given away are small lines under them
            cardName ??= new GUIStyle(ArcadeTypography.Style(30, true)) { wordWrap = false };
            cardLine ??= new GUIStyle(ArcadeTypography.Style(22)) { wordWrap = true };
            cardName.normal.textColor = ArcadeSkin.Cream; cardLine.normal.textColor = ArcadeSkin.Cream;
            const float width = 1040, textW = width - 112 - 150;
            var majors = new List<int>(); var minors = new List<int>();
            for (int i = 0; i < lines.Count; i++) (lines[i].Id != null ? majors : minors).Add(i);
            float listH = 0;
            foreach (int i in majors) listH += Mathf.Max(130, 76 + GlossaryUI.Height(Summary(lines[i]), cardLine, textW)) + 16;
            float h = 128 + listH + minors.Count * 50 + 120;
            Rect card = new Rect(960 - width * .5f, Mathf.Max(30, (1080 - h) * .5f), width, h);
            ItemRarity rarity = majors.Count > 0 ? Card(lines[majors[0]]).Rarity : ItemRarity.Uncommon;
            Color keep = GUI.color; Matrix4x4 m = GUI.matrix;
            float closing = closeAt >= 0 ? Mathf.Clamp01(1 - (Time.unscaledTime - closeAt) / CloseSeconds) : 1; // it folds away with motion
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, .55f * closing * Mathf.Clamp01((Time.unscaledTime - shownAt) / .16f)));
            LivingUI.Pop(Time.unscaledTime - shownAt, card.center);
            if (closing < 1) { ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(.85f, 1, closing), card.center); GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * closing); }
            RarityDescriptionFrame.Draw(card, rarity, Time.unscaledTime);
            Rect inner = new Rect(card.x + 56, card.y + 30, card.width - 112, card.height - 60);
            heading.normal.textColor = ArcadeSkin.Gold;
            GUI.Label(new Rect(inner.x, inner.y, inner.width, 44), "YOU RECEIVED", heading);
            body.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, .7f);
            receivedStyle ??= new GUIStyle(body) { alignment = TextAnchor.MiddleCenter, fontSize = 20 }; // made once, not every GUI pass
            receivedStyle.normal.textColor = body.normal.textColor;
            GUI.Label(new Rect(inner.x, inner.y + 44, inner.width, 28), title, receivedStyle);
            float y = inner.y + 92, step = 0;
            foreach (int i in majors)
            {
                Line l = lines[i]; ShopDescriptionCard shown = Card(l); string summary = Summary(l);
                float rowH = Mathf.Max(130, 76 + GlossaryUI.Height(summary, cardLine, textW));
                Matrix4x4 lm = GUI.matrix; Color lc = GUI.color;
                LivingUI.Pop(Time.unscaledTime - shownAt - .12f - step++ * .08f, new Vector2(card.center.x, y + rowH * .5f)); // each lands a beat after the last
                // the treasure is held up to the light: rays in its rarity's colour fan out and turn slowly behind it, it rises
                // with a little bounce and settles (requirement 25/10/2026: like finding the sword)
                float held = Time.unscaledTime - shownAt - .1f - (step - 1) * .08f;
                Vector2 treasure = new Vector2(inner.x + 60, y + 60);
                // a soft glow in its rarity's colour behind it (the turning rays drew as white dashes round it)
                ArcadeSkin.SelectedAura(new Rect(treasure.x - 55, treasure.y - 55, 110, 110), RunModifierRules.RarityColor(shown.Rarity));
                RarityDescriptionFrame.Backdrop(new Rect(inner.x, y, 120, 120), shown.Rarity, Time.unscaledTime);
                float rise = PresentationSettings.ReducedMotion ? 1 : Mathf.Clamp01(held / .45f), bounce = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(Mathf.Clamp01(held / .7f) * Mathf.PI) * 14;
                float scale = PresentationSettings.ReducedMotion ? 1 : Mathf.Lerp(.55f, 1, Mathf.SmoothStep(0, 1, rise)) * (1 + .08f * Mathf.Sin(Mathf.Clamp01((held - .45f) / .35f) * Mathf.PI));
                float side = 108 * scale;
                ShopDescriptionCard.DrawSprite(new Rect(treasure.x - side * .5f, treasure.y - side * .5f - bounce, side, side), l.Icon);
                GUI.Label(new Rect(inner.x + 150, y - 4, textW, 40), l.Text, cardName);
                Color tag = RunModifierRules.RarityColor(shown.Rarity);
                body.normal.textColor = tag;
                identityStyle ??= new GUIStyle(body) { fontSize = 22, wordWrap = false };
                identityStyle.normal.textColor = body.normal.textColor;
                // a friend you already had comes as a star-up, not a new friend; under the name with a gap
                bool starUp = l.Friend && l.Text.Contains(" now ");
                GUI.Label(new Rect(inner.x + 150, y + 42, textW, 26), (starUp ? "STAR UP · " : l.Friend ? "NEW FRIEND · " : "NEW RELIC · ") + shown.Identity, identityStyle);
                GlossaryUI.Text(new Rect(inner.x + 150, y + 76, textW, rowH - 76), summary, cardLine);
                GUI.matrix = lm; GUI.color = lc;
                y += rowH + 16;
            }
            foreach (int i in minors)
            {
                Line l = lines[i];
                Matrix4x4 lm = GUI.matrix; Color lc = GUI.color;
                LivingUI.Pop(Time.unscaledTime - shownAt - .12f - step++ * .08f, new Vector2(card.center.x, y + 24));
                if (l.Icon != null) ShopDescriptionCard.DrawSprite(new Rect(inner.x + 40, y + 4, 40, 40), l.Icon);
                body.normal.textColor = l.Gain ? new Color(.62f, .95f, .66f) : new Color(1f, .58f, .55f);
                GUI.Label(new Rect(inner.x + 96, y, inner.width - 110, 48), l.Text, body);
                GUI.matrix = lm; GUI.color = lc;
                y += 50;
            }
            GlossaryUI.Draw(new Vector2(1920, 1080));
            Rect ok = new Rect(card.center.x - 160, card.yMax - 100, 320, 70);
            bool close = ArcadeSkin.KitGuiButton(ok, "OK", button);
            GUI.matrix = m; GUI.color = keep;
            if (Event.current.type == EventType.MouseUp && !card.Contains(Event.current.mousePosition) && Time.unscaledTime - shownAt > .35f) { close = true; Event.current.Use(); }
            if (close && closeAt < 0) closeAt = Time.unscaledTime;
            GUI.matrix = previous;
        }
        private GUIStyle cardName, cardLine;
        private static Texture2D beam;
        /// <summary>Soft tapered light beams from a point, turning slowly, with a glow at their root; they grow in as the card opens.</summary>
        public static void Rays(Vector2 centre, float length, float age, Color colour)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (beam == null)
            {
                const int w = 32, h = 128;
                beam = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                for (int py = 0; py < h; py++)
                    for (int px = 0; px < w; px++)
                    {
                        float along = py / (h - 1f), across = Mathf.Abs((px + .5f) / w * 2 - 1); // along: 0 at the tip, 1 at the root
                        float width = Mathf.Lerp(.15f, 1f, along), a = Mathf.Clamp01(1 - across / width) * Mathf.Pow(along, 1.6f);
                        beam.SetPixel(px, py, new Color(1, 1, 1, a));
                    }
                beam.Apply(false, true);
            }
            bool still = PresentationSettings.ReducedMotion;
            float grow = still ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .55f)), spin = still ? 0 : age * 14f, breathe = still ? 1 : .85f + .15f * Mathf.Sin(age * 3.2f);
            Color warm = Color.Lerp(colour, new Color(1f, .93f, .7f), .45f);
            ArcadeSkin.Glow(new Rect(centre.x - length, centre.y - length, length * 2, length * 2), new Color(warm.r, warm.g, warm.b, .55f * grow));
            Matrix4x4 keep = GUI.matrix; Color keepColour = GUI.color;
            const int count = 16;
            for (int k = 0; k < count; k++)
            {
                GUI.matrix = keep;
                GUIUtility.RotateAroundPivot(spin + k * 360f / count, centre);
                float l = length * grow * breathe * (k % 2 == 0 ? 1 : .68f), wide = k % 2 == 0 ? 34 : 22;
                GUI.color = new Color(warm.r, warm.g, warm.b, keepColour.a * (k % 2 == 0 ? .5f : .32f));
                GUI.DrawTexture(new Rect(centre.x - wide * .5f, centre.y - l, wide, l), beam);
            }
            GUI.matrix = keep; GUI.color = keepColour;
        }
        private readonly Dictionary<string, ShopDescriptionCard> cards = new Dictionary<string, ShopDescriptionCard>();
        /// <summary>The shop's description of a relic or friend received (its rarity, kind and full lines), built once.</summary>
        private ShopDescriptionCard Card(Line l)
        {
            string key = (l.Friend ? "u:" : "i:") + l.Id + ":" + l.Tier;
            if (cards.TryGetValue(key, out ShopDescriptionCard known)) return known;
            var made = new ShopDescriptionCard();
            made.Refresh(owner.Session, owner.Content, new ShopOffer { Token = -88, Kind = l.Friend ? OfferKind.Companion : OfferKind.Item, Id = l.Id, Tier = Mathf.Max(1, l.Tier), Price = 0 });
            return cards[key] = made;
        }
        private string Summary(Line l) => Card(l).Summary ?? "";
        /// <summary>A small paper strip under the wave banner: the icon and what was received; it slides in and fades by itself.</summary>
        private void DrawToast()
        {
            float age = Time.unscaledTime - shownAt;
            float enter = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .25f)), leave = Mathf.Clamp01((ToastSeconds - age) / .4f);
            string text = ""; Sprite icon = null;
            foreach (Line l in lines) { text += (text.Length > 0 ? "   ·   " : "") + l.Text; if (icon == null) icon = l.Icon; }
            float width = Mathf.Clamp(body.CalcSize(new GUIContent(text)).x + (icon != null ? 150 : 80), 360, 1000);
            Rect strip = new Rect(960 - width * .5f, 150 - (1 - enter) * 30, width, 76);
            Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * enter * leave);
            ArcadeSkin.Ticket(strip, ArcadeSkin.Paper, ArcadeSkin.Wax);
            if (icon != null) ShopDescriptionCard.DrawSprite(new Rect(strip.x + 26, strip.y + 10, 56, 56), icon);
            body.normal.textColor = new Color(.16f, .42f, .22f);
            GUI.Label(new Rect(strip.x + (icon != null ? 100 : 30), strip.y, strip.width - (icon != null ? 120 : 60), strip.height), text, body);
            GUI.color = keep;
        }
    }
}
