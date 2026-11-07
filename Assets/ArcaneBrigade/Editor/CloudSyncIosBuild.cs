using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>iPhone builds: Google sends the player back to the game on the reversed iOS client id after the Drive sign-in, so that
    /// scheme must be one of the app's URL schemes (Info.plist CFBundleURLTypes). Added before every iOS build when
    /// Resources/CloudSyncConfig.json names the client.</summary>
    public sealed class CloudSyncIosBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;
            string scheme = DriveAuth.IosScheme(DriveAuth.IosClientId);
            if (string.IsNullOrEmpty(scheme)) { Debug.Log("Cloud save: no iOS client id in CloudSyncConfig.json, Drive sync stays hidden on iPhone"); return; }
            string[] schemes = PlayerSettings.iOS.iOSUrlSchemes ?? new string[0];
            if (!schemes.Contains(scheme)) PlayerSettings.iOS.iOSUrlSchemes = schemes.Concat(new[] { scheme }).ToArray();
            Debug.Log("Cloud save: iOS URL scheme " + scheme);
        }
    }
}
