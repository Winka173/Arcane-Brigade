using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.StylePreview.Editor
{
    public static class StylePreviewBuilder
    {
        private const string Root = "Assets/ArcaneBrigade/StylePreview";
        private const string SheetPath = Root + "/Art/MinimalArcade_Sheet.png";
        private static readonly string[] Names = { "SleepyKnight", "HoodWisp", "GrumpyJelly" };
        private static readonly string[] PoseNames = { "Idle", "StepLeft", "StepRight", "Action" };
        private static readonly float[] RowBoundaries = { 0f, 0.39f, 0.72f, 1f };

        [MenuItem("Arcane Brigade/Style Preview/Build scene and capture")]
        public static void BuildAndCapture()
        {
            Directory.CreateDirectory(Root + "/Prefabs");
            Directory.CreateDirectory(Root + "/Animations");
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Materials");
            Directory.CreateDirectory("PreviewExports");
            AssetDatabase.Refresh();

            Sprite[][] characterFrames = ImportSheet();
            GameObject[] prefabs = CreatePrefabs(characterFrames);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = new GameObject("PreviewCamera").AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.backgroundColor = new Color(0.15f, 0.24f, 0.29f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.tag = "MainCamera";

            Material panelMaterial = CreateMaterial("Panel", new Color(0.12f, 0.19f, 0.23f));
            Material lineMaterial = CreateMaterial("Divider", new Color(0.27f, 0.38f, 0.41f));
            Material floorMaterial = CreateMaterial("FloorMark", new Color(0.17f, 0.27f, 0.31f));
            CreateQuad("HeaderPanel", new Vector3(0f, 3.85f, 2f), new Vector2(16f, 1.3f), panelMaterial);
            AddText("ARCANE BRIGADE  /  MINIMAL ARCADE", new Vector3(0f, 4.02f, 0f), 0.3f, Color.white);
            AddText("Original characters - flat shapes, tiny faces, no detailed armour", new Vector3(0f, 3.55f, 0f), 0.14f, new Color(0.68f, 0.8f, 0.82f));

            var actors = new List<PreviewSpriteActor>();
            for (int characterIndex = 0; characterIndex < Names.Length; characterIndex++)
            {
                float horizontalPosition = (characterIndex - 1) * 5f;
                AddText(Names[characterIndex], new Vector3(horizontalPosition, 2.75f, 0f), 0.22f, new Color(0.88f, 0.87f, 0.69f));
                float spriteHeight = characterFrames[characterIndex][0].bounds.size.y;
                GameObject large = Spawn(prefabs[characterIndex], new Vector3(horizontalPosition, -0.18f, 0f), 2.4f / spriteHeight);
                actors.Add(large.GetComponent<PreviewSpriteActor>());
                CreateQuad("Divider" + characterIndex, new Vector3(horizontalPosition, -0.72f, 1f), new Vector2(4.3f, 0.018f), lineMaterial);
                AddText("48px / 32px readability", new Vector3(horizontalPosition, -1.03f, 0f), 0.14f, new Color(0.69f, 0.8f, 0.81f));
                for (int thumbnailIndex = 0; thumbnailIndex < 4; thumbnailIndex++)
                {
                    float position = horizontalPosition - 1.35f + thumbnailIndex * 0.9f;
                    float pixelHeight = thumbnailIndex < 2 ? 48f : 32f;
                    GameObject thumbnail = Spawn(prefabs[characterIndex], new Vector3(position, -1.72f, 0f), pixelHeight * 9f / 1080f / spriteHeight);
                    thumbnail.GetComponent<PreviewSpriteActor>().ShowFrame(thumbnailIndex);
                    actors.Add(thumbnail.GetComponent<PreviewSpriteActor>());
                }
            }

            CreateQuad("CrowdPanel", new Vector3(0f, -3.06f, 2f), new Vector2(15.2f, 1.95f), panelMaterial);
            AddText("CROWD READABILITY - 54 UNITS", new Vector3(0f, -2.4f, 0f), 0.16f, new Color(0.7f, 0.81f, 0.82f));
            for (int crowdIndex = 0; crowdIndex < 54; crowdIndex++)
            {
                int column = crowdIndex % 18;
                int row = crowdIndex / 18;
                float horizontalPosition = -6.7f + column * 0.79f;
                float verticalPosition = -2.96f - row * 0.43f;
                CreateQuad("Tile" + crowdIndex, new Vector3(horizontalPosition, verticalPosition, 1f), new Vector2(0.62f, 0.018f), floorMaterial);
                float spriteHeight = characterFrames[crowdIndex % 3][0].bounds.size.y;
                GameObject crowd = Spawn(prefabs[crowdIndex % 3], new Vector3(horizontalPosition, verticalPosition, 0f), 32f * 9f / 1080f / spriteHeight);
                PreviewSpriteActor actor = crowd.GetComponent<PreviewSpriteActor>();
                actor.Configure(characterFrames[crowdIndex % 3], crowdIndex * 0.071f);
                actor.ShowFrame(crowdIndex % 4);
                actors.Add(actor);
            }

            AddText("Sprite-frame preview only - not final gameplay, rig or finished animation", new Vector3(0f, -4.23f, 0f), 0.13f, new Color(0.65f, 0.77f, 0.79f));
            string scenePath = Root + "/Scenes/MinimalArcadePreview.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };

            Capture(camera, "PreviewExports/Unity_MinimalArcade_Overview.png");
            foreach (PreviewSpriteActor actor in actors)
                actor.ShowFrame(3);
            Capture(camera, "PreviewExports/Unity_MinimalArcade_Action.png");

            AssetDatabase.SaveAssets();
            AssetDatabase.ExportPackage(Root, "PreviewExports/ArcaneBrigade_MinimalArcade.unitypackage", ExportPackageOptions.Recurse);
            File.WriteAllText("PreviewExports/CAPTURE_REPORT.txt", "Rendered by Unity Camera.Render.\nEditor: " + Application.unityVersion + "\nRenderer: " + SystemInfo.graphicsDeviceName + "\nResolution: 1920 x 1080\nCharacters: 3\nSource poses per character: 4\nCrowd units: 54\n");
            Debug.Log("ARCANE_PREVIEW_CAPTURE_COMPLETE");
        }

        private static Sprite[][] ImportSheet()
        {
            AssetDatabase.ImportAsset(SheetPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException("Missing generated sprite sheet: " + SheetPath);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            Color32[] pixels = texture.GetPixels32();
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> existingIds = provider.GetSpriteRects().ToDictionary(sprite => sprite.name, sprite => sprite.spriteID);
            var metadata = new List<SpriteRect>();
            float pixelsPerUnit = texture.height / 3f * 0.8f;
            importer.spritePixelsPerUnit = pixelsPerUnit;

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    int minimumHorizontal = column * texture.width / 4;
                    int maximumHorizontal = (column + 1) * texture.width / 4;
                    int minimumVertical = Mathf.RoundToInt((1f - RowBoundaries[row + 1]) * texture.height);
                    int maximumVertical = Mathf.RoundToInt((1f - RowBoundaries[row]) * texture.height);
                    int left = maximumHorizontal;
                    int right = minimumHorizontal;
                    int bottom = maximumVertical;
                    int top = minimumVertical;
                    for (int vertical = minimumVertical; vertical < maximumVertical; vertical++)
                    {
                        for (int horizontal = minimumHorizontal; horizontal < maximumHorizontal; horizontal++)
                        {
                            if (pixels[vertical * texture.width + horizontal].a < 32)
                                continue;
                            left = Mathf.Min(left, horizontal);
                            right = Mathf.Max(right, horizontal);
                            bottom = Mathf.Min(bottom, vertical);
                            top = Mathf.Max(top, vertical);
                        }
                    }

                    if (right < left || top < bottom)
                        throw new InvalidOperationException("Empty sprite cell: " + row + ", " + column);

                    Rect rect = new Rect(left, bottom, right - left + 1, top - bottom + 1);
                    float centre = (minimumHorizontal + maximumHorizontal) * 0.5f;
                    string spriteName = Names[row] + "_" + PoseNames[column];
                    metadata.Add(new SpriteRect
                    {
                        name = spriteName,
                        spriteID = existingIds.TryGetValue(spriteName, out GUID identifier) ? identifier : GUID.Generate(),
                        rect = rect,
                        alignment = SpriteAlignment.Custom,
                        pivot = new Vector2((centre - left) / rect.width, 0f)
                    });
                }
            }

            provider.SetSpriteRects(metadata.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(metadata.Select(sprite => new SpriteNameFileIdPair(sprite.name, sprite.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            Sprite[] imported = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>().ToArray();
            var frames = new Sprite[3][];
            for (int row = 0; row < 3; row++)
                frames[row] = PoseNames.Select(pose => imported.Single(sprite => sprite.name == Names[row] + "_" + pose)).ToArray();
            return frames;
        }

        private static GameObject[] CreatePrefabs(Sprite[][] frames)
        {
            var prefabs = new GameObject[3];
            for (int characterIndex = 0; characterIndex < 3; characterIndex++)
            {
                GameObject character = new GameObject(Names[characterIndex]);
                character.AddComponent<SpriteRenderer>();
                character.AddComponent<PreviewSpriteActor>().Configure(frames[characterIndex], 0f);
                prefabs[characterIndex] = PrefabUtility.SaveAsPrefabAsset(character, Root + "/Prefabs/" + Names[characterIndex] + ".prefab");
                UnityEngine.Object.DestroyImmediate(character);
                SaveClip(Names[characterIndex] + "_Idle", new[] { frames[characterIndex][0], frames[characterIndex][0] }, true);
                SaveClip(Names[characterIndex] + "_Walk", new[] { frames[characterIndex][1], frames[characterIndex][2], frames[characterIndex][1] }, true);
                SaveClip(Names[characterIndex] + "_Action", new[] { frames[characterIndex][0], frames[characterIndex][3], frames[characterIndex][0] }, false);
            }
            return prefabs;
        }

        private static void SaveClip(string name, Sprite[] frames, bool loop)
        {
            string path = Root + "/Animations/" + name + ".anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            bool isNew = clip == null;
            if (isNew)
                clip = new AnimationClip();
            clip.name = name;
            clip.frameRate = 6f;
            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keys = frames.Select((sprite, index) => new ObjectReferenceKeyframe { time = index / 6f, value = sprite }).ToArray();
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            if (isNew)
                AssetDatabase.CreateAsset(clip, path);
            else
                EditorUtility.SetDirty(clip);
        }

        private static GameObject Spawn(GameObject prefab, Vector3 position, float scale)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            instance.transform.localScale = Vector3.one * scale;
            return instance;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = Root + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Unlit/Color"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateQuad(string name, Vector3 position, Vector2 size, Material material)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.position = position;
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            quad.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
        }

        private static void AddText(string text, Vector3 position, float size, Color color)
        {
            TextMesh label = new GameObject(text).AddComponent<TextMesh>();
            label.transform.position = position;
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 72;
            label.characterSize = size * 0.1f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = color;
            label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
        }

        private static void Capture(Camera camera, string path)
        {
            RenderTexture target = RenderTexture.GetTemporary(1920, 1080, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }
    }
}
