using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace ArcaneBrigade.Editor
{
    /// <summary>Renders every effect of the sandbox table in one Unity run: four key frames per rule on the real floor with a crowd,
    /// saved as one strip per rule (C:/ABClones/fxreview/&lt;rule&gt;.png) for a frame-by-frame review.
    /// -executeMethod ArcaneBrigade.Editor.FxReview.Run [-fxOut dir]</summary>
    public static class FxReview
    {
        private static Sprite Load(string path, float ppu)
        {
            Texture2D t = Resources.Load<Texture2D>(path); if (t == null) return null;
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), ppu);
        }

        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs(); string outDir = "C:/ABClones/fxreview";
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-fxOut") outDir = args[i + 1];
            Directory.CreateDirectory(outDir); foreach (string old in Directory.GetFiles(outDir, "*.png")) File.Delete(old);
            string table = typeof(FxSandbox).GetField("Table", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string ?? "";

            var root = new GameObject("fxreview").transform;
            var camGo = new GameObject("cam"); camGo.transform.SetParent(root); camGo.transform.position = new Vector3(0, .3f, -10);
            Camera cam = camGo.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 4.2f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.05f, .04f, .06f);
            Sprite floor = Load("MienCho/ActOne/Floor_House", 52);
            if (floor != null) for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
            { var g = new GameObject("floor"); g.transform.SetParent(root); g.transform.position = new Vector3(x * 24.1f, y * 24.1f, 0); var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = floor; sr.color = new Color(.42f, .36f, .4f); sr.sortingOrder = -10; }
            string[] crowd = { "Ambusher", "ArmourGrower", "BannerBearer", "Binder" }; var rnd = new System.Random(5); Vector2 at = new Vector2(.6f, .6f);
            for (int i = 0; i < 22; i++)
            {
                Sprite s = Load("EnemiesV3/" + crowd[i % crowd.Length], 330); if (s == null) break;
                var g = new GameObject("enemy"); g.transform.SetParent(root);
                float a = (float)rnd.NextDouble() * 6.28f, d = Mathf.Sqrt((float)rnd.NextDouble()) * 2.6f; Vector2 p = at + new Vector2(Mathf.Cos(a) * d, Mathf.Sin(a) * d * .62f);
                g.transform.position = p; var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = s; sr.sortingOrder = 100 - Mathf.RoundToInt(p.y * 10);
            }
            Vector2 hero = new Vector2(-4.6f, -2.2f);
            Sprite hs = Load("CompanionsV3/AnvilOx", 300);
            if (hs != null) { var g = new GameObject("hero"); g.transform.SetParent(root); g.transform.position = hero; var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = hs; sr.sortingOrder = 80; }

            var rt = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32); cam.targetTexture = rt; cam.aspect = 16f / 9f;
            var frame = new Texture2D(960, 540, TextureFormat.RGB24, false); var strip = new Texture2D(960 * 4, 540, TextureFormat.RGB24, false);
            var done = new HashSet<string>(); int count = 0;
            foreach (string line in table.Split('\n'))
            {
                string[] c = line.Trim().Split('|'); if (c.Length < 4) continue;
                string rule = c[2], kind = c[3]; if (!done.Add(rule) || !FxDirector.Has(rule)) continue;
                float F(int i, float f) => c.Length > i && float.TryParse(c[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : f;
                int step = (int)F(1, 2); float r = F(4, 1), delay = F(5, 0), dur = F(6, 0);
                var fx = new FxDirector(root);
                BondMomentRequest m = kind switch
                {
                    "W" => new BondMomentRequest("review", rule, step, BondMomentKind.Weather, hero, hero, 0, 0, 30),
                    "Z" => new BondMomentRequest("review", rule, step, BondMomentKind.Zone, at, at, r, 0, dur > 0 ? dur : 3),
                    "L" => new BondMomentRequest("review", rule, step, BondMomentKind.Line, new Vector2(-5.5f, -.8f), new Vector2(6, 1.6f), r, delay, 0),
                    "D" => new BondMomentRequest("review", rule, step, BondMomentKind.Strike, hero, at, 1.2f, 0, 0),
                    "T" => new BondMomentRequest("review", rule, step, BondMomentKind.Strike, at, at, 1 + .25f * F(4, 1), 0, 0),
                    _ => new BondMomentRequest("review", rule, step, BondMomentKind.Strike, hero, at, r, delay, 0),
                };
                fx.Spawn(m, 0);
                float land = kind == "W" ? 0 : delay;
                float[] times = kind == "W" ? new[] { 1f, 2.2f, 3.4f, 4.6f } : kind == "Z" ? new[] { .3f, 1f, 2f, Mathf.Max(2.5f, dur - .2f) } : new[] { Mathf.Max(.02f, land - .2f), land + .08f, land + .3f, land + .75f };
                for (int k = 0; k < 4; k++)
                {
                    fx.Draw(times[k]); cam.Render();
                    RenderTexture.active = rt; frame.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); frame.Apply(); RenderTexture.active = null;
                    strip.SetPixels(960 * k, 0, 960, 540, frame.GetPixels());
                }
                fx.Clear(); fx.Draw(0);
                strip.Apply(); File.WriteAllBytes(Path.Combine(outDir, string.Format("{0:000}_{1}_s{2}.png", count++, rule, step)), strip.EncodeToPNG());
                foreach (Transform t in root) if (t.name.StartsWith("Fx ")) Object.DestroyImmediate(t.gameObject);
            }
            Debug.Log("FxReview: " + count + " rules -> " + outDir);
            EditorApplication.Exit(0);
        }
    }
}
