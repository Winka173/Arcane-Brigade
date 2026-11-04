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
        /// <summary>The same soft change across scenes (menu, hero select, the arena, the waiting room): the kept frame rides
        /// over the load and fades once the new scene has drawn its first frames.</summary>
        public static void Load(string scene)
        {
            if (Application.isBatchMode || loading) { UnityEngine.SceneManagement.SceneManager.LoadScene(scene); return; }
            var carrier = new GameObject("Scene dissolve").AddComponent<SceneDissolve>();
            DontDestroyOnLoad(carrier.gameObject);
            loading = true;
            // into a night: the stage curtain closes, the arena is built behind it, and it opens on the fight (requirement
            // 5/11/2026: starting a run held a frozen frame for about two seconds on phones)
            if (scene == ArenaScene && Curtain != null) carrier.StartCoroutine(carrier.RunCurtain(scene));
            else carrier.StartCoroutine(carrier.RunLoad(scene));
        }
        private static bool loading;
        public const string ArenaScene = "RunArena";
        private const float CurtainClose = .6f, CurtainOpen = 1.05f, ReducedCurtain = .25f;
        private static Sprite curtain; private static bool curtainLoaded;
        private static Sprite Curtain { get { if (!curtainLoaded) { curtainLoaded = true; curtain = Resources.Load<Sprite>("UI/Book/CurtainHalf"); } return curtain; } }
        private float closing = -1, opening = -1;
        private bool shut;
        private IEnumerator RunCurtain(string scene)
        {
            bool reduced = PresentationSettings.ReducedMotion;
            closing = Time.unscaledTime;
            while (Time.unscaledTime - closing < (reduced ? ReducedCurtain : CurtainClose)) yield return null;
            shut = true;
            yield return null; yield return new WaitForEndOfFrame(); // one whole frame shut on screen before the slow build
            try { UnityEngine.SceneManagement.SceneManager.LoadScene(scene); }
            finally { loading = false; }
            yield return null; yield return null; yield return null; // the arena builds and draws its first frames behind the curtain
            shut = false; opening = Time.unscaledTime;
            while (Time.unscaledTime - opening < (reduced ? ReducedCurtain : CurtainOpen)) yield return null;
            Destroy(gameObject);
        }
        /// <summary>The two halves of the curtain, closed by <paramref name="shutness"/> (0 open, 1 meeting in the middle).</summary>
        private static void DrawCurtain(float shutness)
        {
            Sprite art = Curtain; Texture t = art.texture; Rect r = art.textureRect;
            float half = Screen.width * .5f + Screen.width * .03f, h = Screen.height * 1.04f, y = -Screen.height * .02f;
            float x = -half * (1 - shutness);
            Rect uv = new Rect(r.x / t.width, r.y / t.height, r.width / t.width, r.height / t.height);
            GUI.DrawTextureWithTexCoords(new Rect(x, y, half, h), t, uv);
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - x - half, y, half, h), t, new Rect(uv.xMax, uv.y, -uv.width, uv.height)); // the right half, mirrored
            if (shutness > .98f) ArcadeSkin.Glow(new Rect(Screen.width * .5f - Screen.height * .16f, Screen.height * .38f, Screen.height * .32f, Screen.height * .24f), new Color(1f, .78f, .42f, .22f)); // candle light along the seam
        }
        private IEnumerator RunLoad(string scene)
        {
            yield return new WaitForEndOfFrame();
            try { kept = ScreenCapture.CaptureScreenshotAsTexture(); }
            catch (Exception) { kept = null; }
            try { UnityEngine.SceneManagement.SceneManager.LoadScene(scene); }
            finally { loading = false; }
            started = float.MaxValue; // held fully over the load
            yield return null; yield return null; yield return null; // the new scene builds and settles its first frames under the kept one
            started = Time.unscaledTime;
            float length = PresentationSettings.ReducedMotion ? ReducedSeconds : Seconds;
            while (Time.unscaledTime - started < length) yield return null;
            Destroy(gameObject);
        }
        private IEnumerator Run(Action change)
        {
            yield return new WaitForEndOfFrame();
            try { kept = ScreenCapture.CaptureScreenshotAsTexture(); }
            catch (Exception) { kept = null; }
            // the change runs as an ordinary action: a guard that refuses during a transition must not refuse the change the
            // dissolve carries (it left the secret room stuck: the way back was refused)
            Busy = false;
            change();
            started = Time.unscaledTime;
            float length = PresentationSettings.ReducedMotion ? ReducedSeconds : Seconds;
            while (Time.unscaledTime - started < length) yield return null;
            Destroy(gameObject);
        }
        private void OnGUI()
        {
            GUI.depth = -10000; // over every other GUI (the HUD, the menus)
            if ((closing >= 0 || shut || opening >= 0) && Event.current.type == EventType.Repaint)
            {
                bool calm = PresentationSettings.ReducedMotion;
                Matrix4x4 m = GUI.matrix; GUI.matrix = Matrix4x4.identity;
                float k = shut ? 1 : opening >= 0 ? 1 - EaseInOut((Time.unscaledTime - opening) / (calm ? ReducedCurtain : CurtainOpen)) : EaseInOut((Time.unscaledTime - closing) / (calm ? ReducedCurtain : CurtainClose));
                DrawCurtain(k);
                GUI.matrix = m;
                return;
            }
            if (kept == null || started < 0 || Event.current.type != EventType.Repaint) return;
            if (started == float.MaxValue) { GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), kept, ScaleMode.StretchToFill, false); return; }
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
        private static float EaseInOut(float t) { t = Mathf.Clamp01(t); return t * t * (3 - 2 * t); }
        private void OnDestroy() { if (kept != null) Destroy(kept); }
    }
}
