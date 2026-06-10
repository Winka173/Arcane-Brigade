using UnityEngine;

namespace ArcaneBrigade
{
    public static class DesktopVisualQuality
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            QualitySettings.antiAliasing = Application.isMobilePlatform ? 2 : 4;
            if (Application.isMobilePlatform)
            {
                // landscape only, turning with the phone either way (was locked to one side in battle)
                Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
                Application.targetFrameRate = 60; // the profile's own pace follows once it loads
            }
        }
    }
}
