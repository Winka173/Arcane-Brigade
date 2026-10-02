using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ArcaneBrigade.Editor
{
    /// <summary>Renders the element reactions (marks on enemies, combos, element deaths) frame by frame onto the real floor, a
    /// grid of cells, one per element or pair, so they can be judged side by side. Batch mode with graphics:
    /// -executeMethod ArcaneBrigade.Editor.ElementFxPreview.Run -efMode marks|deaths|combos -efOut C:/x [-efLength 2] [-efZoom 6.5]</summary>
    public static class ElementFxPreview
    {
        private static string Arg(string name, string fallback)
        {
            string[] a = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == "-" + name) return a[i + 1];
            return fallback;
        }
        private static float F(string name, float fallback) => float.TryParse(Arg(name, null), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : fallback;
        private static Sprite Load(string path, float ppu)
        {
            Texture2D t = Resources.Load<Texture2D>(path); if (t == null) return null;
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), ppu);
        }

        public static readonly ElementLook[] Looks = { ElementLook.Fire, ElementLook.Ice, ElementLook.Water, ElementLook.Storm, ElementLook.Poison, ElementLook.Bloom, ElementLook.Void, ElementLook.Paper, ElementLook.Earth, ElementLook.Wind, ElementLook.Light, ElementLook.Sound, ElementLook.Time, ElementLook.Dream, ElementLook.Crystal };
        private static readonly (ElementLook, ElementLook)[] Pairs = { (ElementLook.Fire, ElementLook.Ice), (ElementLook.Storm, ElementLook.Water), (ElementLook.Fire, ElementLook.Poison), (ElementLook.Wind, ElementLook.Fire), (ElementLook.Void, ElementLook.Fire), (ElementLook.Ice, ElementLook.Storm), (ElementLook.Water, ElementLook.Ice), (ElementLook.Bloom, ElementLook.Fire), (ElementLook.Earth, ElementLook.Water), (ElementLook.Storm, ElementLook.Fire), (ElementLook.Poison, ElementLook.Wind), (ElementLook.Bloom, ElementLook.Water) };

        public static void Run()
        {
            string mode = Arg("efMode", "marks"), outDir = Arg("efOut", "C:/ABClones/fxprev/element_" + mode);
            float dt = 1f / 12, length = F("efLength", 2f); int frames = Mathf.CeilToInt(length / dt);
            Directory.CreateDirectory(outDir); foreach (string old in Directory.GetFiles(outDir, "f*.png")) File.Delete(old);
            var root = new GameObject("efprev").transform;
            var camGo = new GameObject("cam"); camGo.transform.SetParent(root); camGo.transform.position = new Vector3(0, 0, -10);
            Camera cam = camGo.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = F("efZoom", 6.5f); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.05f, .04f, .06f);
            Sprite floor = Load("MienCho/ActOne/Floor_House", 52);
            if (floor != null) for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
            {
                var g = new GameObject("floor"); g.transform.SetParent(root); g.transform.position = new Vector3(x * 24.1f, y * 24.1f, 0);
                var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = floor; sr.color = new Color(.42f, .36f, .4f); sr.sortingOrder = -10;
            }
            string[] crowd = { "Ambusher", "ArmourGrower", "BannerBearer", "Binder" };
            var bodies = new List<(SpriteRenderer sr, Vector2 at)>();
            SpriteRenderer Enemy(Vector2 p, int i)
            {
                Sprite s = Load("EnemiesV3/" + crowd[i % crowd.Length], 330); if (s == null) return null;
                var g = new GameObject("enemy"); g.transform.SetParent(root); g.transform.position = new Vector3(p.x, p.y, 0);
                var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = s; sr.sortingOrder = 100 - Mathf.RoundToInt(p.y * 10);
                bodies.Add((sr, p)); return sr;
            }
            float h = 1.1f;
            var fx = new ElementReactions(root, null) { ComboCap = 16 };
            bool Locate(int id, out Vector2 c, out float height) { c = id < bodies.Count ? bodies[id].at : default; height = h; return id < bodies.Count && bodies[id].sr != null && bodies[id].sr.enabled; }
            int cols = mode == "combos" ? 4 : 5, rows = 3;
            float vh = cam.orthographicSize * 2 * .9f, vw = vh * 16f / 9, cw = vw / cols, ch = vh / rows;
            Vector2 Cell(int i) => new Vector2(-vw / 2 + cw * (i % cols + .5f), vh / 2 - ch * (i / cols + .5f) - ch * .08f);

            var killAt = new Dictionary<int, (float t, ElementLook look)>();
            var hits = new List<(float t, int id, ElementLook look)>();
            if (mode == "combos")
            {
                for (int i = 0; i < Pairs.Length; i++)
                {
                    Vector2 c = Cell(i); int main = bodies.Count; Enemy(c, i);
                    Enemy(c + new Vector2(-.8f, -.35f), i + 1); Enemy(c + new Vector2(.75f, -.45f), i + 2); Enemy(c + new Vector2(.3f, .5f), i + 3);
                    hits.Add((0, main, Pairs[i].Item1)); hits.Add((.2f, main, Pairs[i].Item2));
                }
            }
            else
            {
                for (int i = 0; i < Looks.Length; i++)
                {
                    Vector2 c = Cell(i); int id = bodies.Count; Enemy(c, i);
                    hits.Add((0, id, Looks[i]));
                    if (mode == "deaths") killAt[id] = (.25f, Looks[i]);
                    else if (mode == "crowd") { for (int j = 1; j < 6; j++) { int extra = bodies.Count; Enemy(c + new Vector2(Mathf.Cos(j * 1.3f) * 1.3f, Mathf.Sin(j * 1.3f) * .8f), i + j); hits.Add((0, extra, Looks[i])); } }
                }
            }
            for (int f = 0; f < frames; f++)
            {
                float now = f * dt;
                for (int i = hits.Count - 1; i >= 0; i--) if (now >= hits[i].t) { var hh = hits[i]; fx.Struck(hh.id, bodies[hh.id].at, h, hh.look, now); hits.RemoveAt(i); }
                foreach (var pair in new List<KeyValuePair<int, (float t, ElementLook look)>>(killAt))
                    if (now >= pair.Value.t) { fx.Killed(pair.Key, bodies[pair.Key].at, h, ElementLook.None, Vector2.right, now); bodies[pair.Key].sr.enabled = false; killAt.Remove(pair.Key); }
                fx.Draw(now, Locate);
                RenderFrame(cam, outDir, f);
            }
            Debug.Log("ElementFxPreview: " + frames + " frames of " + mode + " -> " + outDir + " (" + fx.Quads + " quads on the last frame)");
            EditorApplication.Exit(0);
        }

        private static RenderTexture rt; private static Texture2D tex;
        private static void RenderFrame(Camera cam, string outDir, int f)
        {
            if (rt == null) { rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32); tex = new Texture2D(1280, 720, TextureFormat.RGB24, false); }
            cam.targetTexture = rt; cam.aspect = 16f / 9f; cam.Render();
            RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(outDir, "f" + f.ToString("000") + ".png"), tex.EncodeToPNG());
        }
    }
}
