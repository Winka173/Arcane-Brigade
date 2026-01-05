using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class CheckpointPlayerSmoke : MonoBehaviour
    {
        private bool failed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-smoke") < 0) return;
            var runner = new GameObject("CheckpointPlayerSmoke");
            DontDestroyOnLoad(runner);
            runner.AddComponent<CheckpointPlayerSmoke>();
        }

        private void Awake() { Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) failed = true;
        }

        private IEnumerator Start()
        {
            yield return null;
            SceneManager.LoadScene(Application.CanStreamedLevelBeLoaded("RunArena") ? "RunArena" : "Arena");
            yield return new WaitForSecondsRealtime(3f);
            ArenaController controller = FindAnyObjectByType<ArenaController>();
            if (controller == null || controller.Simulation == null || controller.Simulation.Elapsed < 1f) failed = true;
            foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>())
                if (renderer.gameObject.activeInHierarchy && renderer.sprite == null) failed = true;
            Debug.Log(failed ? "ARCANE_PLAYER_SMOKE_FAILED" : "ARCANE_PLAYER_SMOKE_PASSED");
            Application.Quit(failed ? 1 : 0);
        }
    }
}
