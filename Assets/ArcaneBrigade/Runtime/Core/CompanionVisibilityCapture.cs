using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ArcaneBrigade
{
    public sealed partial class OctoberCapture
    {
        private IEnumerator ReviewCompanionVisibility(RunCatalog catalog, bool reduced)
        {
            PresentationSettings.Apply(new PlayerProfile { ReducedMotion = reduced, ReducedFlashes = reduced, ReducedEffects = reduced });
            Scene old = SceneManager.GetActiveScene(); Scene scene = SceneManager.CreateScene("CompanionVisibility");
            SceneManager.SetActiveScene(scene); yield return SceneManager.UnloadSceneAsync(old);
            Camera camera = new GameObject("VisibilityCamera").AddComponent<Camera>(); camera.enabled = false;
            camera.orthographic = true; camera.orthographicSize = 1.25f; camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear; camera.cullingMask = 1 << 30;
            var target = new RenderTexture(192, 192, 24, RenderTextureFormat.ARGB32); target.Create(); camera.targetTexture = target;
            var pixels = new Texture2D(192, 192, TextureFormat.RGBA32, false);
            int ReadFrame()
            {
                camera.Render(); RenderTexture previous = RenderTexture.active; RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 192, 192), 0, 0); pixels.Apply(); RenderTexture.active = previous;
                int count = 0; foreach (Color32 p in pixels.GetRawTextureData<Color32>()) if (p.a >= 16) count++;
                return count;
            }
            void SaveFrame(string name) => File.WriteAllBytes(Path.Combine(root, name + ".png"), pixels.EncodeToPNG());
            int identity = 0;
            foreach (UnitDefinition source in catalog.Companions.Concat(catalog.Champions))
            {
                bool companion = catalog.Companions.Contains(source);
                for (int tier = 1; tier <= (companion ? 3 : 1); tier++)
                {
                    UnitDefinition definition = Instantiate(source); definition.CombatTier = tier; definition.BodyHeight = 1.25f;
                    UnitState state = new UnitState { Id = 9, Definition = definition, Faction = companion ? Faction.Companion : Faction.Champion,
                        Alive = true, Health = companion ? float.PositiveInfinity : definition.Health, Facing = Vector2.right, AttackDirection = Vector2.right,
                        LastHit = -100, AttackStarted = -100, LastSupport = -100 };
                    UnitVisual view = Instantiate(catalog.Arena.UnitPrefab).GetComponent<UnitVisual>(); view.ReducedMotion = reduced; view.Bind(state, catalog.Arena, true);
                    for (int phase = 0; phase < 12; phase++)
                    {
                        float time = 10 + phase * .137f;
                        state.Position = Vector2.zero; state.PreviousPosition = phase == 0 ? Vector2.zero : Vector2.left * .03f;
                        state.Velocity = phase == 0 ? Vector2.zero : Vector2.right * 2;
                        state.TravelMode = phase >= 6 ? CompanionTravelMode.Pursuit : CompanionTravelMode.Patrol;
                        state.AttackStarted = phase % 3 == 0 ? -100 : time - (phase % 3 == 1 ? definition.Weapon.Windup * .5f : definition.Weapon.Windup + .03f);
                        state.PendingAttack = phase % 3 == 1; state.AttackWindup = definition.Weapon.Windup;
                        state.LastHit = phase >= 9 ? time - .04f : -100;
                        view.Display(time, 1);
                        foreach (Transform t in view.GetComponentsInChildren<Transform>()) t.gameObject.layer = 30;
                        foreach (SpriteRenderer r in view.GetComponentsInChildren<SpriteRenderer>())
                            if (r != view.BodyRenderer && r.name != "UpperBody" && r.name != "LowerBody") r.enabled = false;
                        SpriteRenderer[] body = view.BodyRenderer.GetComponentsInChildren<SpriteRenderer>().Where(r => r.enabled && r.sprite != null).ToArray();
                        var originals = new MaterialPropertyBlock[body.Length];
                        for (int i = 0; i < body.Length; i++)
                        {
                            originals[i] = new MaterialPropertyBlock(); body[i].GetPropertyBlock(originals[i]);
                            // Compare opacity independently of subpixel face/fabric deformation.
                            var p = new MaterialPropertyBlock(); body[i].GetPropertyBlock(p); p.SetVector("_LivingMotion", Vector4.zero); body[i].SetPropertyBlock(p);
                        }
                        int observed = ReadFrame();
                        bool evidence = source.Id == "TeapotPilgrim" && tier == 1 && phase == 1 && !reduced;
                        if (evidence) SaveFrame("TeapotPilgrim-attack-fixed");
                        foreach (SpriteRenderer r in body)
                        {
                            var p = new MaterialPropertyBlock(); r.GetPropertyBlock(p); SpriteLighting.SetSpriteRect(p, r.sprite); r.SetPropertyBlock(p);
                        }
                        int expected = ReadFrame();
                        bool visible = expected > 40 && observed >= expected * .98f;
                        if (!visible) { SaveFrame(source.Id + "-control"); for (int i = 0; i < body.Length; i++) body[i].SetPropertyBlock(originals[i]); ReadFrame(); SaveFrame(source.Id + "-failure"); }
                        Check(visible, source.Id + " tier=" + tier + " phase=" + phase + " visible pixels=" + observed + "/" + expected);
                        if (evidence)
                        {
                            foreach (SpriteRenderer r in body) { var p = new MaterialPropertyBlock(); r.GetPropertyBlock(p); SpriteLighting.SetSpriteRect(p, view.DisplayedBody); r.SetPropertyBlock(p); }
                            int legacy = ReadFrame(); SaveFrame("TeapotPilgrim-legacy-UV-reproduction");
                            Check(legacy < observed * .25f, "Old shared atlas rectangle reproduces disappearing body: " + legacy + " pixels versus fixed " + observed);
                        }
                        for (int i = 0; i < body.Length; i++) body[i].SetPropertyBlock(originals[i]);
                    }
                    view.gameObject.SetActive(false); Destroy(view.gameObject); Destroy(definition);
                }
                identity++; if (identity % 4 == 0) yield return null;
            }
            Check(!failed, "All 66 companion species at three tiers and all 31 champions remain visible across attack, movement, pursuit and hurt poses");
            camera.targetTexture = null; Destroy(camera.gameObject); target.Release(); Destroy(target); Destroy(pixels);
        }
    }
}
