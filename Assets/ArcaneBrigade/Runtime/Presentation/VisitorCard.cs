using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A shop visitor's card (Docs/Events_Ideas.md): the visitor, its line and its offer, and the choices as plates
    /// (relics and friends with their pictures). Opened from the visitor's table in the shop; Esc closes it and the visitor waits;
    /// "No, thank you" sends the visitor on its way.</summary>
    public sealed class VisitorCard : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle hand, plain, name, title;
        private bool shown; private int page;
        public int InputConsumedFrame { get; private set; } = -1;
        public void Configure(ArenaController value) => owner = value;
        public bool Open => shown && owner?.Session?.Visitor != null && !owner.StoryConsumesInput && !owner.TransitionPending;
        public void Show() { shown = true; page = 0; InputConsumedFrame = Time.frameCount; }
        private void Close() { shown = false; InputConsumedFrame = Time.frameCount; }

        private void Update()
        {
            if (!Open || owner.PresentationPaused) { if (owner?.Session?.Visitor == null) shown = false; return; }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) { Close(); return; }
            List<VisitorOption> options = owner.Session.VisitorOptions();
            int first = page * 9;
            for (int i = 0; i < Mathf.Min(9, options.Count - first); i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) { Choose(first + i); return; }
        }
        private void Choose(int index)
        {
            InputConsumedFrame = Time.frameCount;
            if (owner.VisitorChoose(index) && owner.Session.Visitor == null) shown = false;
        }
        private Sprite IconFor(VisitorOption o)
        {
            if (string.IsNullOrEmpty(o.Icon)) return null;
            int cut = o.Icon.IndexOf(':'); string kind = o.Icon.Substring(0, cut), id = o.Icon.Substring(cut + 1);
            return kind == "item" ? owner.Content.Item(id)?.Icon : kind == "unit" ? owner.Content.Unit(id)?.Body : owner.Content.Weapon(id)?.Sprite;
        }
        private void OnGUI()
        {
            if (!Open || owner.PresentationPaused || Camera.main == null) return;
            VisitorDefinition v = owner.Session.Visitor;
            List<VisitorOption> options = owner.Session.VisitorOptions();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (hand == null)
            {
                hand = ArcadeTypography.Style(28); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleLeft; hand.wordWrap = true;
                plain = ArcadeTypography.Style(20); plain.alignment = TextAnchor.UpperLeft; plain.wordWrap = true;
                name = ArcadeTypography.Style(16, true); name.alignment = TextAnchor.MiddleCenter; name.wordWrap = true;
                title = ArcadeTypography.Style(26, true); title.alignment = TextAnchor.MiddleLeft;
            }
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, 0.4f));
            Rect card = new Rect(360, 300, 1200, 560);
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Wax);
            Sprite portrait = ExpansionArtwork.Current?.Visitor(v.Id, 1) ?? ExpansionArtwork.Current?.Visitor(v.Id, 0);
            ShopDescriptionCard.DrawSprite(new Rect(card.x + 30, card.y + 40, 240, 300), portrait);
            title.normal.textColor = new Color(.25f, .16f, .1f);
            GUI.Label(new Rect(card.x + 300, card.y + 30, card.width - 340, 40), v.Name, title);
            hand.normal.textColor = new Color(.25f, .19f, .14f);
            GUI.Label(new Rect(card.x + 300, card.y + 72, card.width - 340, 80), "'" + v.Line + "'", hand);
            plain.normal.textColor = new Color(.18f, .14f, .1f);
            GUI.Label(new Rect(card.x + 300, card.y + 152, card.width - 340, 60), v.Offer, plain);
            int pages = Mathf.Max(1, (options.Count + 8) / 9); page = Mathf.Clamp(page, 0, pages - 1);
            int first = page * 9, shown9 = Mathf.Min(9, options.Count - first);
            bool pictures = false; for (int i = 0; i < shown9; i++) if (!string.IsNullOrEmpty(options[first + i].Icon)) pictures = true;
            float slotW = pictures ? 96 : Mathf.Min(260, (card.width - 340) / Mathf.Max(1, shown9) - 10), slotH = pictures ? 120 : 64;
            for (int i = 0; i < shown9; i++)
            {
                VisitorOption o = options[first + i];
                Rect slot = new Rect(card.x + 300 + i * (slotW + 10), card.y + 230, slotW, slotH);
                ArcadeSkin.Plate(slot, o.Enabled ? ArcadeSkin.Card : new Color(.4f, .38f, .36f));
                Sprite icon = IconFor(o);
                if (icon != null) ShopDescriptionCard.DrawSprite(new Rect(slot.x + 14, slot.y + 6, slot.width - 28, 62), icon);
                name.normal.textColor = o.Enabled ? ArcadeSkin.Cream : new Color(.8f, .78f, .75f);
                name.fontSize = pictures ? 13 : 16;
                GUI.Label(new Rect(slot.x + 3, icon != null ? slot.y + 68 : slot.y, slot.width - 6, icon != null ? 50 : slot.height), (i + 1) + " · " + o.Label, name);
                if (o.Enabled && GUI.Button(slot, GUIContent.none, GUIStyle.none)) Choose(first + i);
            }
            if (pages > 1)
            {
                Rect prev = new Rect(card.x + 300, card.y + 360, 44, 44), next = new Rect(card.x + 352, card.y + 360, 44, 44);
                ArcadeSkin.Plate(prev, ArcadeSkin.Card); ArcadeSkin.Plate(next, ArcadeSkin.Card); name.normal.textColor = ArcadeSkin.Cream;
                if (GUI.Button(prev, "◀", name)) page = (page + pages - 1) % pages;
                if (GUI.Button(next, "▶", name)) page = (page + 1) % pages;
            }
            Rect later = new Rect(card.x + 300, card.yMax - 90, 300, 56), no = new Rect(card.x + 620, card.yMax - 90, 300, 56);
            ArcadeSkin.Plate(later, ArcadeSkin.Teal); ArcadeSkin.Plate(no, ArcadeSkin.Wax);
            name.normal.textColor = ArcadeSkin.Cream; name.fontSize = 16;
            if (GUI.Button(later, "MAYBE LATER  [Esc]", name)) Close();
            if (GUI.Button(no, "NO, THANK YOU", name)) { owner.VisitorDecline(); Close(); }
            GUI.matrix = previous;
        }
    }
}
