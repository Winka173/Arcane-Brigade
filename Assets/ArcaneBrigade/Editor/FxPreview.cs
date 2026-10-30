using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ArcaneBrigade.Editor
{
    /// <summary>Renders one bond moment frame by frame onto the real floor with a crowd of enemies for scale, so an effect can be
    /// judged in seconds instead of after a full build and showcase. Batch mode with graphics (not -nographics):
    /// -executeMethod ArcaneBrigade.Editor.FxPreview.Run -fxRule storm-thunderbolt -fxKind Strike -fxRadius 2.2 -fxDelay .4 -fxOut C:/x</summary>
    public static class FxPreview
    {
        private static string Arg(string name, string fallback)
        {
            string[] a = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == "-" + name) return a[i + 1];
            return fallback;
        }
        private static float F(string name, float fallback) => float.TryParse(Arg(name, null), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : fallback;

        private static Sprite Load(string path, float pixelsPerUnit)
        {
            Texture2D t = Resources.Load<Texture2D>(path); if (t == null) return null;
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), pixelsPerUnit);
        }

        public static void Run()
        {
            string rule = Arg("fxRule", "storm-thunderbolt"), outDir = Arg("fxOut", "C:/ABClones/fxprev/" + rule);
            var kind = (BondMomentKind)System.Enum.Parse(typeof(BondMomentKind), Arg("fxKind", "Strike"));
            float radius = F("fxRadius", 2f), delay = F("fxDelay", .4f), dt = F("fxDt", 1f / 12), length = F("fxLength", 2.6f); int frames = Mathf.CeilToInt(length / dt);
            Directory.CreateDirectory(outDir); foreach (string old in Directory.GetFiles(outDir, "f*.png")) File.Delete(old);

            var root = new GameObject("fxprev").transform;
            var camGo = new GameObject("cam"); camGo.transform.SetParent(root); camGo.transform.position = new Vector3(0, 0, -10);
            Camera cam = camGo.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 6.5f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.05f, .04f, .06f);
            // the floor, darkened like the combat lighting, with a crowd of enemies and the hero for scale
            Sprite floor = Load("MienCho/ActOne/Floor_House", 52);
            if (floor != null) for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
            {
                var g = new GameObject("floor"); g.transform.SetParent(root); g.transform.position = new Vector3(x * 24.1f, y * 24.1f, 0);
                var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = floor; sr.color = new Color(.42f, .36f, .4f); sr.sortingOrder = -10;
            }
            Vector2 at = new Vector2(F("fxX", 0), F("fxY", .5f)); string[] crowd = { "Ambusher", "ArmourGrower", "BannerBearer", "Binder" };
            System.Random rnd = new System.Random(5);
            for (int i = 0; i < 26; i++)
            {
                Sprite s = Load("EnemiesV3/" + crowd[i % crowd.Length], 330); if (s == null) break;
                var g = new GameObject("enemy"); g.transform.SetParent(root);
                float a = (float)rnd.NextDouble() * 6.28f, d = Mathf.Sqrt((float)rnd.NextDouble()) * radius * 1.3f;
                Vector2 p = at + new Vector2(Mathf.Cos(a) * d, Mathf.Sin(a) * d * .62f); g.transform.position = new Vector3(p.x, p.y, 0);
                var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = s; sr.sortingOrder = 100 - Mathf.RoundToInt(p.y * 10);
            }
            Sprite hero = Load("CompanionsV3/AnvilOx", 300);
            if (hero != null) { var g = new GameObject("hero"); g.transform.SetParent(root); g.transform.position = new Vector3(-6, -3.2f, 0); var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = hero; sr.sortingOrder = 80; }

            var fx = new FxDirector(root);
            Vector2 from = kind == BondMomentKind.Weather || kind == BondMomentKind.Strike ? at : new Vector2(-6, -3);
            if (kind == BondMomentKind.Line) { from = new Vector2(-9, -2); at = new Vector2(9, 1.5f); }
            bool ok = fx.Spawn(new BondMomentRequest("preview", rule, 6, kind, from, at, radius, delay, F("fxDuration", 30)), 0);
            if (!ok) { Debug.LogError("FxPreview: no effect registered for " + rule); EditorApplication.Exit(2); return; }

            var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32); cam.targetTexture = rt; cam.aspect = 16f / 9f;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            for (int f = 0; f < frames; f++)
            {
                float now = f * dt; fx.Draw(now); cam.Render();
                RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(outDir, "f" + f.ToString("000") + ".png"), tex.EncodeToPNG());
            }
            Debug.Log("FxPreview: " + frames + " frames of " + rule + " -> " + outDir + " (" + fx.Quads + " quads on the last frame)");
            EditorApplication.Exit(0);
        }
    }
}
