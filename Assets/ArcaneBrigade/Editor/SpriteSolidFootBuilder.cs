using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Measures, for every world sprite, the lowest row of solid paint (several pixels at least half opaque) and stores it
    /// in Resources/SpriteSolidFoot.asset, so a ground shadow starts at the painted feet and not at a faint glow below them.</summary>
    public static class SpriteSolidFootBuilder
    {
        private const string AssetPath = "Assets/ArcaneBrigade/Resources/SpriteSolidFoot.asset";
        private const byte Solid = 128;

        public static void Build()
        {
            var entries = new List<SpriteSolidFoot.Entry>();
            var files = Directory.GetFiles("Assets/ArcaneBrigade/Art", "*.png", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles("Assets/ArcaneBrigade/Resources", "*.png", SearchOption.AllDirectories).Where(f => !f.Replace('\\', '/').Contains("/Resources/UI/")));
            int measured = 0;
            foreach (string path in files)
            {
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                if (sprites.Length == 0) continue;
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(File.ReadAllBytes(path))) { Object.DestroyImmediate(texture); continue; }
                Color32[] pixels = texture.GetPixels32();
                foreach (Sprite sprite in sprites)
                {
                    // import downscaling maps textureRect back to the delivered pixels
                    Rect rect = sprite.textureRect; float rx = texture.width / (float)sprite.texture.width, ry = texture.height / (float)sprite.texture.height;
                    int left = Mathf.RoundToInt(rect.xMin * rx), right = Mathf.RoundToInt(rect.xMax * rx), bottom = Mathf.RoundToInt(rect.yMin * ry), top = Mathf.RoundToInt(rect.yMax * ry);
                    int need = Mathf.Max(3, (right - left) / 60), row = -1;
                    for (int y = bottom; y < top && row < 0; y++)
                    {
                        int count = 0;
                        for (int x = left; x < right; x++) if (pixels[y * texture.width + x].a >= Solid && ++count >= need) { row = y; break; }
                    }
                    if (row < 0) continue;
                    entries.Add(new SpriteSolidFoot.Entry { Sprite = sprite, Foot = ((row - bottom) / ry - sprite.pivot.y) / sprite.pixelsPerUnit });
                    measured++;
                }
                Object.DestroyImmediate(texture);
            }
            var table = AssetDatabase.LoadAssetAtPath<SpriteSolidFoot>(AssetPath);
            if (table == null) { table = ScriptableObject.CreateInstance<SpriteSolidFoot>(); AssetDatabase.CreateAsset(table, AssetPath); }
            table.Entries = entries.ToArray();
            EditorUtility.SetDirty(table); AssetDatabase.SaveAssets(); SpriteSolidFoot.Invalidate();
            Debug.Log("SOLID_FOOT measured " + measured + " sprites");
        }
    }
}
