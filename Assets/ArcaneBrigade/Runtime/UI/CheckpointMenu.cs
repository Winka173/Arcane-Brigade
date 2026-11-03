using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class CheckpointMenu : MonoBehaviour
    {
        private GUIStyle title;
        private GUIStyle caption;
        private GUIStyle button;
        private void OnGUI()
        {
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 42, alignment = TextAnchor.MiddleCenter };
                caption = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                button = new GUIStyle(GUI.skin.button) { fontSize = 22 };
            }
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) * 0.5f, (Screen.height - 720f * scale) * 0.5f, 0f), Quaternion.identity, Vector3.one * scale);
            GUI.Label(new Rect(100, 190, 1080, 70), "ARCANE BRIGADE", title);
            GUI.Label(new Rect(190, 275, 900, 65), "SQUAD CHECKPOINT\nMove only. Allies roam nearby and fight automatically.", caption);
            if (TouchTargets.Button(new Rect(465, 390, 350, 65), "PLAY 60-SECOND WAVE", button)) SceneManager.LoadScene("Arena");
            GUI.Label(new Rect(170, 495, 940, 65), "WASD / arrows on PC. Drag the lower-left area on touch.\nPrototype: no shop, merge, final balance or complete content yet.", caption);
            GUI.matrix = previous;
        }
    }
}
