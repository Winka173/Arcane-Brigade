using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Deluxe offer (requirement 4/11/2026): one card that opens only where the player asks for something Deluxe holds
    /// (a locked difficulty, Endless at dawn) or from its main-menu entry; never in a fight. What it adds, the price on the gold
    /// button, a quieter restore, and a close. It draws over every screen and takes every tap while it is open.</summary>
    public sealed class DeluxeOffer : MonoBehaviour
    {
        private const string Key = "deluxe-offer";
        private static DeluxeOffer instance;
        private bool open;
        private string reason;
        private GUIStyle heading, line, note, primary, secondary;

        /// <summary>Opens the card; <paramref name="why"/> is the one line above the list (what the tap asked for).</summary>
        public static void Open(string why = null)
        {
            if (!Deluxe.Sold || Deluxe.Owned) return;
            if (instance == null) { var holder = new GameObject("Deluxe offer"); DontDestroyOnLoad(holder); instance = holder.AddComponent<DeluxeOffer>(); }
            instance.open = true; instance.reason = why;
            CheckpointAudio.PlayInterfaceId("ui.open");
        }
        public static bool Showing => instance != null && instance.open;

        private void OnEnable() => Deluxe.Changed += OnChanged;
        private void OnDisable() => Deluxe.Changed -= OnChanged;
        private void OnChanged() { if (open && Deluxe.Owned) LivingUI.CloseThen(Key, () => open = false); }

        private void OnGUI()
        {
            if (!open) return;
            GUI.depth = -200; // over the menu, the results and every other card
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1920, 1080);
            Color tint = GUI.color; GUISkin skin = GUI.skin; GUI.skin = ArcadeSkin.Skin;
            if (heading == null)
            {
                heading = ArcadeTypography.Style(44, true); heading.alignment = TextAnchor.MiddleCenter; if (ArcadeTypography.Display != null) heading.font = ArcadeTypography.Display;
                line = ArcadeTypography.Style(30); line.alignment = TextAnchor.MiddleLeft; line.wordWrap = true;
                note = ArcadeTypography.Style(28); note.alignment = TextAnchor.MiddleCenter; note.wordWrap = true;
                primary = ArcadeSkin.Primary(new GUIStyle(ArcadeSkin.Skin.button) { fontSize = 30, font = ArcadeTypography.Heading });
                secondary = new GUIStyle(ArcadeSkin.Skin.button) { fontSize = 26, font = ArcadeTypography.Heading };
            }
            RunHud.Fill(new Rect(-1920, -1080, 5760, 3240), new Color(0, 0, 0, .6f));
            Rect card = new Rect(460, 170, 1000, 740);
            if (LivingUI.Auto(Key, card.center))
            {
                ArcadeSkin.Frame(card, KitFrame.Dialogue, .55f);
                heading.normal.textColor = ArcadeSkin.Gold;
                LocalizedGUI.Label(new Rect(card.x, card.y + 46, card.width, 64), "ARCANE BRIGADE DELUXE", heading);
                note.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, .8f);
                LocalizedGUI.Label(new Rect(card.x + 80, card.y + 118, card.width - 160, 50), string.IsNullOrEmpty(reason) ? "Bought once. The whole game stays free to play." : reason, note);
                float y = card.y + 196;
                line.normal.textColor = ArcadeSkin.Cream;
                foreach (string item in Deluxe.Contents)
                {
                    ArcadeSkin.Diamond(new Vector2(card.x + 120, y + 30), 11, ArcadeSkin.Gold);
                    LocalizedGUI.Label(new Rect(card.x + 148, y, card.width - 240, 60), item, line);
                    y += 74;
                }
                LocalizedGUI.Label(new Rect(card.x + 80, y + 4, card.width - 160, 46), "Nothing that makes a run stronger is ever sold.", note);
                bool busy = DeluxeStore.Busy, ready = DeluxeStore.Available;
                Rect buy = new Rect(card.center.x - 260, card.yMax - 230, 520, 96);
                if (ArcadeSkin.KitGuiButton(buy, busy ? "…" : ready ? "GET DELUXE · " + Deluxe.Price : "STORE NOT READY", primary) && ready && !busy) DeluxeStore.Buy();
                Rect restore = new Rect(card.center.x - 190, buy.yMax + 14, 380, 64);
                if (ArcadeSkin.Button(restore, "RESTORE PURCHASE", KitButton.Secondary, !busy, secondary) && !busy) DeluxeStore.Restore();
                if (!string.IsNullOrEmpty(DeluxeStore.Message)) { note.normal.textColor = ArcadeSkin.Gold; LocalizedGUI.Label(new Rect(card.x + 60, buy.y - 58, card.width - 120, 50), DeluxeStore.Message, note); }
                if (ArcadeSkin.Medallion(new Rect(card.xMax - 70, card.y + 6, 64, 64), KitMedallion.Close) && !LivingUI.Closing(Key)) LivingUI.CloseThen(Key, () => open = false);
                // a tap outside the card closes it too
                if (Event.current.type == EventType.MouseDown && !card.Contains(Event.current.mousePosition) && !LivingUI.Closing(Key)) LivingUI.CloseThen(Key, () => open = false);
            }
            GUI.matrix = previous; GUI.color = tint; GUI.skin = skin;
            if (Event.current.isMouse || Event.current.type == EventType.ScrollWheel || Event.current.isKey) Event.current.Use(); // nothing under the card takes the tap
        }
    }
}
