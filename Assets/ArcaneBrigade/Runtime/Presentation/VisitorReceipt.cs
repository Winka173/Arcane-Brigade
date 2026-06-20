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
        private struct Line { public Sprite Icon; public string Text; public bool Gain; }
        private readonly List<Line> lines = new List<Line>();
        private string title;
        private float shownAt = -1, closeAt = -1;
        // requirement 25/10/2026: the card only opens for a relic or a friend; gold, eggs or an effect is a small notice that
        // slides in under the wave banner and goes by itself
        private bool toast;
        private const float ToastSeconds = 2.8f, CloseSeconds = .18f;
        private ArenaController owner;
        private GUIStyle heading, body, button;
        public bool Open => shownAt >= 0;

        public static void Record(ArenaController owner, RunSnapshot before, string visitorName, string dealGet)
        {
            var receipt = owner.GetComponent<VisitorReceipt>() ?? owner.gameObject.AddComponent<VisitorReceipt>();
            receipt.owner = owner; receipt.Build(before, visitorName, dealGet);
        }

        private void Build(RunSnapshot before, string visitorName, string dealGet)
        {
            lines.Clear(); title = visitorName;
            RunSnapshot after = owner.Session.State; RunCatalog catalog = owner.Content;
            // relics gained and given
            var had = new Dictionary<string, int>(); foreach (ItemStack s in before.Items) had[s.Id] = (had.TryGetValue(s.Id, out int n) ? n : 0) + s.Count;
            var has = new Dictionary<string, int>(); foreach (ItemStack s in after.Items) has[s.Id] = (has.TryGetValue(s.Id, out int n) ? n : 0) + s.Count;
            foreach (var pair in has)
            {
                int gained = pair.Value - (had.TryGetValue(pair.Key, out int n) ? n : 0); ItemDefinition item = catalog.Item(pair.Key);
                if (gained > 0 && item != null) Add(item.Icon, "New relic: " + item.Name + "\n" + owner.Session.SecretRewardStats(item.Id), true);
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
                if (old == null) Add(unit.Body, "New friend: " + RunUICopy.UnitName(unit), true);
                else if (r.Tier > old.Tier) Add(unit.Body, RunUICopy.UnitName(unit) + " · now " + r.Tier + " stars", true);
                else if (r.WeaponId != old.WeaponId) Add(catalog.Weapon(r.WeaponId)?.Sprite ?? unit.Body, RunUICopy.UnitName(unit) + " now carries " + RunUICopy.WeaponName(r.WeaponId), true);
            }
            foreach (RosterEntry r in before.Roster)
                if (!after.Roster.Exists(o => o.UnitId == r.UnitId)) { UnitDefinition unit = catalog.Unit(r.UnitId); if (unit != null) Add(unit.Body, "Left: " + RunUICopy.UnitName(unit), false); }
            int major = lines.Count; // relics and friends: worth the card
            // purse
            int gold = after.Gold - before.Gold, eggs = after.Eggs - before.Eggs;
            if (gold != 0) Add(catalog.Arena.Coin, (gold > 0 ? "+" : "−") + Mathf.Abs(gold) + " gold", gold > 0);
            if (eggs != 0) Add(catalog.EggSprite, (eggs > 0 ? "+" : "−") + Mathf.Abs(eggs) + (Mathf.Abs(eggs) == 1 ? " egg" : " eggs"), eggs > 0);
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
                    Add(isEgg ? catalog.EggSprite : isGold ? catalog.Arena.Coin : owner.Content.HeartSprite, "+" + count + (isEgg ? count == 1 ? " egg" : " eggs" : isGold ? " gold" : " HP"), true);
                }
                else if (!string.IsNullOrEmpty(what)) Add(null, what, true);
            }
            if (lines.Count == 0) Add(null, "Nothing changed.", false);
            toast = major == 0;
            shownAt = Time.unscaledTime; closeAt = -1;
            CheckpointAudio.PlayInterfaceId(toast ? "ui.coach.pop" : "ui.unlock");
        }
        private void Add(Sprite icon, string text, bool gain) { if (lines.Count < 6) lines.Add(new Line { Icon = icon, Text = text, Gain = gain }); }

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
            float rowH = 84, h = 150 + lines.Count * rowH + 110;
            Rect card = new Rect(560, (1080 - h) * .5f, 800, h);
            Color keep = GUI.color; Matrix4x4 m = GUI.matrix;
            float closing = closeAt >= 0 ? Mathf.Clamp01(1 - (Time.unscaledTime - closeAt) / CloseSeconds) : 1; // it folds away with motion
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, .45f * closing * Mathf.Clamp01((Time.unscaledTime - shownAt) / .16f)));
            LivingUI.Pop(Time.unscaledTime - shownAt, card.center);
            if (closing < 1) { ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(.85f, 1, closing), card.center); GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * closing); }
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Wax); // the visitor card's own paper
            Rect inner = ArcadeSkin.Content(card, KitFrame.Card);
            GUI.Label(new Rect(inner.x, inner.y + 4, inner.width, 44), "YOU RECEIVED", heading);
            body.normal.textColor = new Color(.42f, .32f, .22f);
            GUI.Label(new Rect(inner.x, inner.y + 46, inner.width, 30), title, new GUIStyle(body) { alignment = TextAnchor.MiddleCenter, fontSize = 20 });
            float y = inner.y + 92;
            for (int i = 0; i < lines.Count; i++)
            {
                Line l = lines[i];
                // each line lands a beat after the one before
                Matrix4x4 lm = GUI.matrix; Color lc = GUI.color;
                LivingUI.Pop(Time.unscaledTime - shownAt - .12f - i * .08f, new Vector2(card.center.x, y + rowH * .5f));
                if (l.Icon != null) ShopDescriptionCard.DrawSprite(new Rect(inner.x + 20, y + 6, 70, 70), l.Icon);
                body.normal.textColor = l.Gain ? new Color(.16f, .42f, .22f) : new Color(.62f, .14f, .16f);
                GUI.Label(new Rect(inner.x + (l.Icon != null ? 110 : 20), y, inner.width - (l.Icon != null ? 130 : 40), rowH), l.Text, body);
                GUI.matrix = lm; GUI.color = lc;
                y += rowH;
            }
            Rect ok = new Rect(card.center.x - 160, card.yMax - 96, 320, 70);
            bool close = ArcadeSkin.KitGuiButton(ok, "OK", button);
            GUI.matrix = m; GUI.color = keep;
            if (Event.current.type == EventType.MouseUp && !card.Contains(Event.current.mousePosition) && Time.unscaledTime - shownAt > .35f) { close = true; Event.current.Use(); }
            if (close && closeAt < 0) closeAt = Time.unscaledTime;
            GUI.matrix = previous;
        }
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
