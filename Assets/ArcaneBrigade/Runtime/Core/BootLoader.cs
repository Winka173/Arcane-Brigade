using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class BootLoader : MonoBehaviour
    {
        private void Start() { SceneManager.LoadScene("Frontend"); }
    }
}
