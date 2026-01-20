using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ShadowChecks
    {
        public static void Build()
        {
            var host = new GameObject("ShadowFixture");
            Texture2D texture = new Texture2D(8, 8);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), Vector2.one * 0.5f, 8);
            int assertions = 0;
            Action<bool, string> require = (condition, caption) =>
            {
                if (!condition) throw new InvalidOperationException("Shadow: " + caption);
                assertions++;
                Debug.Log("SHADOW_PASS " + caption);
            };
            try
            {
                GroundShadow shadow = host.AddComponent<GroundShadow>();
                foreach (Vector3 scale in new[] { Vector3.one, new Vector3(3, 2, 1), new Vector3(-2, 0.4f, 1) })
                {
                    host.transform.localScale = scale;
                    host.transform.rotation = Quaternion.Euler(0, 0, 37);
                    Vector2 ground = new Vector2(2, -1);
                    shadow.Display(sprite, ground, 1.5f);
                    require(shadow.Contact.enabled && shadow.Penumbra.enabled && shadow.Cast.enabled, "All three shadow layers visible");
                    require(Vector2.Distance(shadow.Contact.transform.position, ground) < 0.001f, "Contact anchored to ground despite parent transform");
                    require(Quaternion.Angle(shadow.Contact.transform.rotation, Quaternion.identity) < 0.01f, "Contact does not rotate with its actor");
                    require(shadow.Contact.bounds.size.x > 1 && shadow.Contact.bounds.size.x < 1.4f, "World footprint independent of inherited scale");
                    require(shadow.Contact.color.a >= 0.55f && shadow.Penumbra.color.a > 0.25f, "Ground contrast visible on textured floor");
                    float restingOpacity = shadow.Contact.color.a;
                    shadow.Display(sprite, ground, 1.5f, 2);
                    require(shadow.Contact.color.a < restingOpacity, "Airborne shadow softens");
                    shadow.Fade(0);
                    require(shadow.Contact.color.a == 0 && shadow.Penumbra.color.a == 0 && shadow.Cast.color.a == 0, "All layers fade with actor");
                    shadow.Hide();
                    require(!shadow.Contact.enabled && !shadow.Penumbra.enabled && !shadow.Cast.enabled, "Pooling hides all layers");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
            }
            Directory.CreateDirectory("PreviewExports/Shadows");
            File.WriteAllText("PreviewExports/Shadows/Checks.txt", "PASS " + assertions + " shadow assertions\n");
            K13Builder.VerifyAndBuild();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/Shadows/Windows_Build.txt", true);
        }
    }
}
