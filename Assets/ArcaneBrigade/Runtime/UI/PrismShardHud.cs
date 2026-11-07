using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The prism shards carried, under the wallet column: "PRISM 1 / 3". Three become a rainbow gift at the next shop.
    /// Shown only once the first shard is found (and while one is being picked up in a fight), so a run without any keeps a
    /// clean HUD. Its own small layer: the HUD column stays untouched.</summary>
    public sealed class PrismShardHud : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle text, helpTitle, helpText;
        private float helpAt = -1; // the tap card: what the shards are for
        private Sprite icon;
        public void Configure(ArenaController value) => owner = value;
        private int Carried => owner?.Session == null ? 0 : owner.Session.State.PrismShards + (owner.Simulation != null && owner.Session.State.Phase == SessionPhase.Combat ? owner.Simulation.PrismShards : 0);
        private void OnGUI()
        {
            if (owner?.Session == null || owner.Simulation == null || owner.PresentationPaused || owner.Details.Open || owner.StoryConsumesInput || owner.NightmareEndingVisible) return;
            if (owner.Session.State.Phase != SessionPhase.Combat && owner.Session.State.Phase != SessionPhase.Shop) return;
            int carried = Carried; if (carried <= 0) return;
            Camera camera = Camera.main; if (camera == null) return;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            text ??= ArcadeTypography.Style(24, true);
            icon ??= PacingArtwork.Prism(owner.Content);
            // under the hero's gauge lines when they show (it sat inside them), else under the wallet
            // under the gauge lines; on a phone everything in this column sits 112 lower, under the pause and brigade buttons
            Rect plate = new Rect(22, (owner.Simulation.HeroIdentityEnabled ? 350 : 240) + (RunUIHints.Device == RunUIDevice.Touch ? 112 : 0), 214, 36);
            ArcadeSkin.Panel(plate, ArcadeSkin.Card);
            if (icon != null) RunHud.Sprite(new Rect(plate.x + 8, plate.y + 3, 30, 30), icon);
            text.normal.textColor = new Color(0.68f, 1, 0.92f);
            LocalizedGUI.Label(new Rect(plate.x + 46, plate.y, plate.width - 50, plate.height), "PRISM  " + Mathf.Min(carried, 99) + " / 3", text);
            if (TouchTargets.Button(plate, GUIContent.none, GUIStyle.none)) { helpAt = helpAt >= 0 ? -1 : Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.tooltip"); }
            if (helpAt >= 0 && Time.unscaledTime - helpAt > 8) helpAt = -1;
            if (helpAt >= 0)
            {
                // tapped: a small card says what the shards are for (icons are tappable and explained)
                Rect card = new Rect(250, 280, 560, 132);
                Matrix4x4 m = GUI.matrix; Color c = GUI.color;
                LivingUI.Pop(Time.unscaledTime - helpAt, card.center);
                ArcadeSkin.Frame(card, KitFrame.Dialogue, .6f);
                Rect inner = ArcadeSkin.Content(card, KitFrame.Dialogue);
                helpTitle ??= ArcadeTypography.Style(22, true);
                helpText ??= new GUIStyle(ArcadeTypography.Style(24)) { wordWrap = true };
                helpTitle.normal.textColor = new Color(0.68f, 1, 0.92f); helpText.normal.textColor = ArcadeSkin.KitCream;
                LocalizedGUI.Label(new Rect(inner.x, inner.y, inner.width, 28), "PRISM SHARDS", helpTitle);
                LocalizedGUI.Label(new Rect(inner.x, inner.y + 30, inner.width, inner.height - 30), "Collect 3 and the next shop gives you a rainbow gift. Elite enemies and won secret fights drop them.", helpText);
                GUI.matrix = m; GUI.color = c;
                if (Event.current.type == EventType.MouseUp && !card.Contains(Event.current.mousePosition) && !plate.Contains(Event.current.mousePosition)) helpAt = -1;
            }
            GUI.matrix = previous;
        }
    }
}
