using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The prism shards carried, under the wallet column: "PRISM 1 / 3". Three become a rainbow gift at the next shop.
    /// Shown only once the first shard is found (and while one is being picked up in a fight), so a run without any keeps a
    /// clean HUD. Its own small layer: the HUD column stays untouched.</summary>
    public sealed class PrismShardHud : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle text;
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
            text ??= ArcadeTypography.Style(18, true);
            icon ??= PacingArtwork.Prism(owner.Content);
            Rect plate = new Rect(22, 296, 214, 36);
            ArcadeSkin.Panel(plate, ArcadeSkin.Card);
            if (icon != null) RunHud.Sprite(new Rect(plate.x + 8, plate.y + 3, 30, 30), icon);
            text.normal.textColor = new Color(0.68f, 1, 0.92f);
            GUI.Label(new Rect(plate.x + 46, plate.y, plate.width - 50, plate.height), "PRISM  " + Mathf.Min(carried, 99) + " / 3", text);
            GUI.matrix = previous;
        }
    }
}
