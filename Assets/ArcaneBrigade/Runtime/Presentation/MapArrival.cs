using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A new map announces itself (requirement 23/10/2026: each map should feel its own): on the first fight of a map the
    /// map's name rises on a title banner with one short line of what is different here, for a few seconds, then fades.</summary>
    public sealed class MapArrival : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle banner, line;
        private string announced = "";
        private float since = -100;
        public void Configure(ArenaController value) => owner = value;

        public static string Tagline(MapTheme map) => map switch
        {
            MapTheme.WaitingRoom => "A called door lets a pack out. Stand at the door to shut it.",
            MapTheme.PostOffice => "Letters give a boost. Break a parcel: it bursts and spills gold.",
            MapTheme.LampHall => "Friends are faster in the light, foes tougher in the dark. Relight the lamps.",
            MapTheme.Glasshouse => "Pots bloom with boosts. Sun dazzles foes, rain slows them.", // requirement 25/10/2026: placeholder until its mechanics land
            MapTheme.Theatre => "Stay in the spotlight: its colour is your boost.",
            _ => "Stand by the clock when it chimes: enemies slow for longer."
        };

        private void Update()
        {
            var session = owner?.Session; var sim = owner?.Simulation;
            if (session == null || sim == null || session.State.Phase != SessionPhase.Combat) return;
            int wave = session.State.Wave;
            if (wave != MapIdentity.FirstWave(wave) || sim.Elapsed > 1.5f) return;
            string key = session.State.RunId + ":" + wave;
            if (key == announced) return;
            announced = key; since = Time.unscaledTime;
            CheckpointAudio.PlayInterfaceId("ui.unlock");
        }

        private void OnGUI()
        {
            if (owner != null && owner.GoalCardShowing) { if (Time.unscaledTime - since < 4.2f) since = Time.unscaledTime; return; } // one card at a time: the map's card waits for the goal card
            float t = Time.unscaledTime - since;
            if (t > 4.2f || owner?.Session == null || Camera.main == null || owner.PresentationPaused || owner.StoryConsumesInput) return;
            GUI.depth = -40;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (banner == null)
            {
                banner = ArcadeTypography.Style(32, true); banner.alignment = TextAnchor.MiddleCenter;
                line = ArcadeTypography.Style(22); line.alignment = TextAnchor.MiddleCenter; line.wordWrap = true;
            }
            float alpha = Mathf.Clamp01(t / .35f) * (1 - Mathf.Clamp01((t - 3.5f) / .7f));
            float rise = PresentationSettings.ReducedMotion ? 0 : (1 - Mathf.Clamp01(t / .45f)) * 18;
            Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * alpha);
            MapTheme map = MapIdentity.For(owner.Session.State.Wave);
            // below the tip zone at the top middle (UI standard: HUD zones), so a tip and the map's card never print over each other
            ArcadeSkin.TitleBanner(new Rect(560, 300 + rise, 800, 72), MapIdentity.Name(map).ToUpperInvariant(), banner);
            // only the name: the map's first tip ribbon (just above) already says what is new here; a line under the name said it twice
            GUI.color = keep;
            GUI.matrix = previous;
        }
    }
}
