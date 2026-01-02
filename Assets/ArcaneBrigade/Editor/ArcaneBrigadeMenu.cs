using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ArcaneBrigade.Core;

namespace ArcaneBrigade.EditorTools
{
    /// <summary>
    /// Editor menu utilities for setting up the project. Chapter 1 only creates the
    /// empty <c>Game.unity</c> scene; later chapters will extend this with default
    /// ScriptableObject creation.
    /// </summary>
    public static class ArcaneBrigadeMenu
    {
        private const string SceneFolder = "Assets/ArcaneBrigade/Scenes";
        private const string GameScenePath = SceneFolder + "/Game.unity";

        [MenuItem("Tools/Arcane Brigade/Create Game Scene")]
        public static void CreateGameScene()
        {
            if (!Directory.Exists(SceneFolder))
            {
                Directory.CreateDirectory(SceneFolder);
                AssetDatabase.Refresh();
            }

            if (File.Exists(GameScenePath))
            {
                if (!EditorUtility.DisplayDialog(
                        "Game scene exists",
                        $"{GameScenePath} already exists. Overwrite?",
                        "Overwrite", "Cancel"))
                {
                    return;
                }
            }

            // Build a minimal 2D scene with a GameManager root, an orthographic camera,
            // and a global 2D light (matches the URP 2D pipeline already in use).
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Game";

            // Camera
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.AddComponent<AudioListener>();

            // GameManager root
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            // Save
            EditorSceneManager.SaveScene(scene, GameScenePath);
            AssetDatabase.Refresh();

            // Add to build settings if not already there.
            AddSceneToBuildSettings(GameScenePath);

            Debug.Log($"[Arcane Brigade] Created scene at {GameScenePath}");
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            System.Array.Copy(scenes, newScenes, scenes.Length);
            newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
