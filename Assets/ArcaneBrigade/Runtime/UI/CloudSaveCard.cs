using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The cloud save card (requirement 7/11/2026), opened over the main menu only: when two devices changed the save, the two
    /// sides side by side (best wave, Lamp Oil, the run in play, when) and the player picks one, the other kept as a restore point; or
    /// the list of restore points, a second tap on the same one puts it back. It takes every tap while open.</summary>
    public sealed class CloudSaveCard : MonoBehaviour
    {
        private const string Key = "cloud-save";
        private enum Mode { Conflict, Restore }
        private static CloudSaveCard instance;
        private bool open;
        private Mode mode;
        private List<CloudBackup> backups = new List<CloudBackup>();
        private int armed = -1;
        private Vector2 scroll;
        private GUIStyle heading, line, note, primary, secondary, column;

        public static void OpenConflict() => Show(Mode.Conflict);
        public static void OpenRestore() => Show(Mode.Restore);
        public static bool Showing => instance != null && instance.open;
        private static void Show(Mode which)
        {
            if (instance == null) { var holder = new GameObject("Cloud save card"); DontDestroyOnLoad(holder); instance = holder.AddComponent<CloudSaveCard>(); }
            instance.open = true; instance.mode = which; instance.armed = -1; instance.scroll = Vector2.zero;
            if (which == Mode.Restore) instance.backups = CloudSync.Backups();
            CheckpointAudio.PlayInterfaceId("ui.open");
        }
        private void Close() { if (!LivingUI.Closing(Key)) LivingUI.CloseThen(Key, () => open = false); }

        private static string When(string utc)
        {
            if (!DateTime.TryParse(utc, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime at)) return "";
            DateTime local = at.ToLocalTime();
            return local.Date == DateTime.Now.Date ? local.ToString("HH:mm") : local.ToString("dd/MM HH:mm");
        }
        /// <summary>Best wave · Lamp Oil · the run in play, for one side of the choice.</summary>
        private static string Summary(PlayerProfile p, string run)
        {
            string text = p == null ? "" : "Best wave " + p.BestWave + "\n" + "Lamp Oil " + (p.Meta?.LampOil ?? 0);
            RunSnapshot snapshot = null;
            try { snapshot = string.IsNullOrEmpty(run) ? null : JsonUtility.FromJson<RunSnapshot>(run); } catch { }
            return text + "\n" + (snapshot != null ? "Run in play: wave " + snapshot.Wave : "No run in play");
        }

        private void OnGUI()
        {
            if (!open) return;
            GUI.depth = -200;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1920, 1080);
            Color tint = GUI.color; GUISkin skin = GUI.skin; GUI.skin = ArcadeSkin.Skin;
            if (heading == null)
            {
                heading = ArcadeTypography.Style(44, true); heading.alignment = TextAnchor.MiddleCenter; if (ArcadeTypography.Display != null) heading.font = ArcadeTypography.Display;
                line = ArcadeTypography.Style(30); line.alignment = TextAnchor.MiddleLeft; line.wordWrap = true;
                column = ArcadeTypography.Style(30); column.alignment = TextAnchor.UpperCenter; column.wordWrap = true;
                note = ArcadeTypography.Style(28); note.alignment = TextAnchor.MiddleCenter; note.wordWrap = true;
                primary = ArcadeSkin.Primary(new GUIStyle(ArcadeSkin.Skin.button) { fontSize = 30, font = ArcadeTypography.Heading });
                secondary = new GUIStyle(ArcadeSkin.Skin.button) { fontSize = 28, font = ArcadeTypography.Heading };
            }
            RunHud.Fill(new Rect(-1920, -1080, 5760, 3240), new Color(0, 0, 0, .6f));
            Rect card = new Rect(360, 150, 1200, 780);
            if (mode == Mode.Conflict && CloudSync.Conflict == null) Close();
            if (LivingUI.Auto(Key, card.center))
            {
                ArcadeSkin.Frame(card, KitFrame.Dialogue, .55f);
                heading.normal.textColor = ArcadeSkin.Gold;
                note.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, .8f);
                if (mode == Mode.Conflict) DrawConflict(card); else DrawRestore(card);
                if (ArcadeSkin.Medallion(new Rect(card.xMax - 76, card.y + 12, 64, 64), KitMedallion.Close)) Close();
                if (Event.current.type == EventType.MouseDown && !card.Contains(Event.current.mousePosition)) Close();
            }
            GUI.matrix = previous; GUI.color = tint; GUI.skin = skin;
            if (Event.current.isMouse || Event.current.type == EventType.ScrollWheel || Event.current.isKey) Event.current.Use();
        }

        private void DrawConflict(Rect card)
        {
            CloudConflict c = CloudSync.Conflict;
            if (c == null) return;
            LocalizedGUI.Label(new Rect(card.x, card.y + 50, card.width, 64), "TWO SAVES DIFFER", heading);
            LocalizedGUI.Label(new Rect(card.x + 90, card.y + 120, card.width - 180, 50), "Pick the one to keep. The other stays as a restore point.", note);
            column.normal.textColor = ArcadeSkin.Cream;
            float w = (card.width - 240) / 2;
            Rect left = new Rect(card.x + 90, card.y + 200, w, 380), right = new Rect(left.xMax + 60, left.y, w, 380);
            ArcadeSkin.Frame(left, KitFrame.Card, .45f); ArcadeSkin.Frame(right, KitFrame.Card, .45f);
            heading.fontSize = 34;
            LocalizedGUI.Label(new Rect(left.x, left.y + 26, left.width, 50), "This device", heading);
            LocalizedGUI.Label(new Rect(right.x, right.y + 26, right.width, 50), "Google Drive", heading);
            heading.fontSize = 44;
            LocalizedGUI.Label(new Rect(left.x + 30, left.y + 96, left.width - 60, 170), Summary(CloudSync.CurrentProfile, CloudSync.CurrentRunPayload), column);
            string from = string.IsNullOrEmpty(c.CloudDevice) ? "" : c.CloudDevice + " · ";
            LocalizedGUI.Label(new Rect(right.x + 30, right.y + 96, right.width - 60, 170), Summary(c.CloudProfile, c.CloudRun) + "\n" + from + When(c.CloudWritten), column);
            if (ArcadeSkin.KitGuiButton(new Rect(left.x + 40, left.yMax - 112, left.width - 80, 84), "KEEP THIS DEVICE", secondary)) { CloudSync.Resolve(CloudChoice.KeepThisDevice); Close(); }
            if (ArcadeSkin.KitGuiButton(new Rect(right.x + 40, right.yMax - 112, right.width - 80, 84), "USE DRIVE SAVE", secondary)) { CloudSync.Resolve(CloudChoice.UseCloud); Close(); }
            LocalizedGUI.Label(new Rect(card.x + 90, card.yMax - 150, card.width - 180, 90), "Finds, records and achievements from both are kept either way.", note);
        }

        private void DrawRestore(Rect card)
        {
            LocalizedGUI.Label(new Rect(card.x, card.y + 50, card.width, 64), "RESTORE POINTS", heading);
            LocalizedGUI.Label(new Rect(card.x + 90, card.y + 120, card.width - 180, 50), "Kept before anything replaced your save. Tap twice to restore.", note);
            Rect view = new Rect(card.x + 70, card.y + 190, card.width - 140, card.height - 250);
            if (backups.Count == 0) { LocalizedGUI.Label(view, "No restore points yet", note); return; }
            const float row = 108;
            scroll = ScrollPanel.Begin(view, scroll, new Rect(0, 0, view.width - 30, backups.Count * row));
            line.normal.textColor = ArcadeSkin.Cream;
            for (int i = 0; i < backups.Count; i++)
            {
                CloudBackup b = backups[i];
                Rect r = new Rect(0, i * row, view.width - 30, row - 12);
                ArcadeSkin.Frame(r, KitFrame.Plaque, .4f);
                PlayerProfile p = null;
                try { p = string.IsNullOrEmpty(b.Progress) ? null : JsonUtility.FromJson<PlayerProfile>(b.Progress); } catch { }
                string words = When(b.CreatedUtc) + " · " + b.Reason + (p != null ? " · " + "Best wave " + p.BestWave + " · " + "Lamp Oil " + (p.Meta?.LampOil ?? 0) : "");
                LocalizedGUI.Label(new Rect(r.x + 28, r.y, r.width - 360, r.height), words, line);
                if (ArcadeSkin.KitGuiButton(new Rect(r.xMax - 310, r.y + 12, 290, r.height - 24), armed == i ? "RESTORE NOW" : "RESTORE", armed == i ? primary : secondary))
                {
                    if (armed != i) { armed = i; CheckpointAudio.PlayInterfaceId("ui.setting"); }
                    else if (CloudSync.Restore(b)) { CheckpointAudio.PlayInterfaceId("ui.confirm"); Close(); }
                    else CheckpointAudio.PlayInterfaceId("ui.error");
                }
            }
            ScrollPanel.End();
        }
    }
}
