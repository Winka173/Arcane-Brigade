using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a map lends the hero, said plainly: each gain rises over the hero's head in the same words as a stat gained
    /// by a skill or mechanic (requirement 25/10/2026: the map's lines must look like those), queued a beat apart and never
    /// dropped (no row of running boosts is kept on the HUD).</summary>
    public sealed class MapBoostView : MonoBehaviour
    {
        private ArenaController owner;
        private readonly Queue<MapPop> waiting = new Queue<MapPop>();
        private float lastShown = -10;
        private GUIStyle chip;

        public void Configure(ArenaController value) => owner = value;

        private void Update()
        {
            MapEnvironmentState s = owner?.Simulation?.MapEnvironment;
            if (s == null) { waiting.Clear(); return; }
            foreach (MapPop pop in s.Pops) waiting.Enqueue(pop);
            s.Pops.Clear();
            // a line waits its turn rather than being dropped, and lines rise a beat apart so each can be read
            if (waiting.Count > 0 && Time.unscaledTime - lastShown > .55f && !owner.PresentationPaused)
            {
                MapPop next = waiting.Dequeue(); lastShown = Time.unscaledTime;
                owner.HeroWord(next.Text, next.Text.Contains("+") ? ArcadeSkin.Better : next.Colour);
            }
        }

        private void OnGUI()
        {
            if (owner?.Session == null || owner.Simulation == null || owner.PresentationPaused || Camera.main == null) return;
            if (owner.Session.State.Phase != SessionPhase.Combat) return;
            if (Event.current.type != EventType.Repaint) return;
            chip ??= new GUIStyle(ArcadeTypography.Style(20, true)) { alignment = TextAnchor.MiddleCenter, wordWrap = false };
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            // (no row of running boosts under the HUD any more: requirement 1/11/2026, the gain is said once over the hero's head)
            GUI.matrix = previous;
        }
    }
}
