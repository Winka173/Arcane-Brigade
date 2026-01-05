using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class RunBootLoader : MonoBehaviour
    {
        private void Start() { SceneManager.LoadScene("RunFrontend"); }
    }
}
