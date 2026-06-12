using System;
using System.Collections;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A soft change of scene (requirement 24/10/2026: the end of a wave cut straight to the shop, its props and light
    /// jumping in one frame): the last frame is kept, the new scene is built under it, and the kept frame fades away while it
    /// swells a touch, so the fight melts into the shop (and the shop into the next fight). Reduced motion: a shorter fade, no swell.</summary>
    public sealed class SceneDissolve : MonoBehaviour
    {
        private const float Seconds = .7f, ReducedSeconds = .35f;
        private Texture2D kept;
        private float started = -1;
        public static bool Busy { get; private set; }

        /// <summary>Keeps this frame, then runs the change (the new scene builds under the kept frame) and fades the frame away.</summary>
        public static void Then(MonoBehaviour host, Action change)
        {
            if (host == null || !host.isActiveAndEnabled || Application.isBatchMode) { change(); return; }
            var carrier = new GameObject("Scene dissolve").AddComponent<SceneDissolve>();
            Busy = true;
            carrier.StartCoroutine(carrier.Run(change));
        }
        private IEnumerator Run(Action change)
        {
            yield return new WaitForEndOfFrame();
            try { kept = ScreenCapture.CaptureScreenshotAsTexture(); }
            catch (Exception) { kept = null; }
            try { change(); }
            finally { Busy = false; }
            started = Time.unscaledTime;
            float length = PresentationSettings.ReducedMotion ? ReducedSeconds : Seconds;
            while (Time.unscaledTime - started < length) yield return null;
            Destroy(gameObject);
        }
        private void OnGUI()
        {
            GUI.depth = -10000; // over every other GUI (the HUD, the menus)
            if (kept == null || started < 0 || Event.current.type != EventType.Repaint) return;
            bool reduced = PresentationSettings.ReducedMotion;
            float t = Mathf.Clamp01((Time.unscaledTime - started) / (reduced ? ReducedSeconds : Seconds));
            float alpha = 1 - Mathf.SmoothStep(0, 1, t), swell = reduced ? 1 : 1 + .035f * t;
            Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
            GUI.matrix = Matrix4x4.identity;
            float w = Screen.width * swell, h = Screen.height * swell;
            GUI.color = new Color(1, 1, 1, alpha);
            GUI.DrawTexture(new Rect((Screen.width - w) * .5f, (Screen.height - h) * .5f, w, h), kept, ScaleMode.StretchToFill, false);
            GUI.matrix = keepMatrix; GUI.color = keepColour;
        }
        private void OnDestroy() { if (kept != null) Destroy(kept); }
    }
}
