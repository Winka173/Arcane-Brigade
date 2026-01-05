using UnityEngine;

namespace ArcaneBrigade
{
    public static class DesktopVisualQuality
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            QualitySettings.antiAliasing = Application.isMobilePlatform ? 2 : 4;
        }
    }
}
