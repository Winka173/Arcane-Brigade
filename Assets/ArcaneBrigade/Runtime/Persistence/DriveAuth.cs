using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcaneBrigade
{
    /// <summary>The Google sign-in for Drive sync, one entry for both phones: Android through Google Identity's authorization client
    /// (the DriveAuth.androidlib plugin), iPhone through Google's OAuth page in Safari with PKCE, back through the game's URL scheme.
    /// Both ask only for drive.appdata. One Google Cloud project for both means one app folder: Android and iPhone share the save.</summary>
    public static class DriveAuth
    {
        [Serializable] private sealed class Config { public string iosClientId = ""; }
        private static Config config;
        /// <summary>Resources/CloudSyncConfig.json: the iOS OAuth client id (public, not a secret). Empty: no Drive sync on iPhone.</summary>
        public static string IosClientId
        {
            get
            {
                if (config == null)
                {
                    TextAsset asset = Resources.Load<TextAsset>("CloudSyncConfig");
                    try { config = asset != null ? JsonUtility.FromJson<Config>(asset.text) : null; } catch { }
                    config ??= new Config();
                }
                return (config.iosClientId ?? "").Trim();
            }
        }
        /// <summary>The iOS URL scheme Google sends the player back on: the client id reversed.</summary>
        public static string IosScheme(string clientId) => string.IsNullOrEmpty(clientId) ? "" : "com.googleusercontent.apps." + clientId.Replace(".apps.googleusercontent.com", "");

        public static bool Supported => Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer && IosClientId != "";

        public static Task<(string Token, string Error)> Token(bool interactive) =>
            Application.platform == RuntimePlatform.IPhonePlayer ? IosDriveAuth.Token(interactive) : AndroidDriveAuth.Token(interactive);
        public static void Clear(string token) { if (Application.platform == RuntimePlatform.IPhonePlayer) IosDriveAuth.Clear(); else AndroidDriveAuth.Clear(token); }

        /// <summary>Disconnect: Google takes the game's access back (the player can also do it from their Google Account).</summary>
        public static async Task Revoke()
        {
            string token = Application.platform == RuntimePlatform.IPhonePlayer ? IosDriveAuth.Refresh : (await AndroidDriveAuth.Token(false)).Token;
            if (!string.IsNullOrEmpty(token))
            {
                using (UnityWebRequest request = UnityWebRequest.PostWwwForm("https://oauth2.googleapis.com/revoke?token=" + UnityWebRequest.EscapeURL(token), ""))
                {
                    var done = new TaskCompletionSource<bool>();
                    request.SendWebRequest().completed += _ => done.TrySetResult(true);
                    await done.Task;
                    if (request.responseCode != 200) Debug.LogWarning("Drive access not revoked online: HTTP " + request.responseCode);
                }
                Clear(token);
            }
            IosDriveAuth.Forget();
        }
    }

    /// <summary>iPhone: OAuth 2.0 for installed apps (authorization code + PKCE, no client secret). Google's page opens in Safari,
    /// the answer comes back as a deep link on the reversed client id; the refresh token stays on the device (PlayerPrefs).</summary>
    public static class IosDriveAuth
    {
        private const string RefreshKey = "arcane.drive.refresh";
        private static string access;
        private static DateTime accessUntil;
        private static TaskCompletionSource<(string, string)> waiting;
        private static string verifier, expectedState, redirect;

        public static string Refresh { get { try { return PlayerPrefs.GetString(RefreshKey, ""); } catch { return ""; } } }
        public static void Clear() { access = null; }
        public static void Forget() { access = null; try { PlayerPrefs.DeleteKey(RefreshKey); PlayerPrefs.Save(); } catch { } }

        [Serializable] private sealed class TokenReply { public string access_token, refresh_token, error; public int expires_in; }

        public static async Task<(string Token, string Error)> Token(bool interactive)
        {
            string client = DriveAuth.IosClientId;
            if (client == "") return (null, "unsupported");
            if (access != null && DateTime.UtcNow < accessUntil) return (access, null);
            string refresh = Refresh;
            if (refresh != "")
            {
                var (reply, code) = await Post("grant_type=refresh_token&client_id=" + Esc(client) + "&refresh_token=" + Esc(refresh));
                if (reply?.access_token != null) return (Keep(reply), null);
                if (code == 0) return (null, "api-7"); // no network
                Forget(); // taken back or expired: ask again
            }
            if (!interactive) return (null, "consent");
            return await SignIn(client);
        }

        private static async Task<(string, string)> SignIn(string client)
        {
            byte[] random = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(random);
            verifier = Base64Url(random);
            using (SHA256 sha = SHA256.Create())
            {
                string challenge = Base64Url(sha.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
                expectedState = Guid.NewGuid().ToString("N");
                redirect = DriveAuth.IosScheme(client) + ":/oauth2redirect";
                waiting?.TrySetResult((null, "replaced"));
                waiting = new TaskCompletionSource<(string, string)>();
                Application.deepLinkActivated -= OnDeepLink;
                Application.deepLinkActivated += OnDeepLink;
                Application.OpenURL("https://accounts.google.com/o/oauth2/v2/auth?client_id=" + Esc(client) + "&redirect_uri=" + Esc(redirect)
                    + "&response_type=code&scope=" + Esc(DriveCloudBackend.Scope) + "&code_challenge=" + challenge + "&code_challenge_method=S256&state=" + expectedState);
            }
            var (code, error) = await waiting.Task;
            Application.deepLinkActivated -= OnDeepLink;
            if (code == null) return (null, error);
            var (reply, status) = await Post("grant_type=authorization_code&client_id=" + Esc(client) + "&code=" + Esc(code) + "&code_verifier=" + Esc(verifier) + "&redirect_uri=" + Esc(redirect));
            if (reply?.access_token == null) return (null, status == 0 ? "api-7" : "exchange " + (reply?.error ?? status.ToString()));
            return (Keep(reply), null);
        }

        private static void OnDeepLink(string url)
        {
            if (waiting == null || string.IsNullOrEmpty(redirect) || !url.StartsWith(redirect, StringComparison.Ordinal)) return;
            string code = Query(url, "code"), state = Query(url, "state"), error = Query(url, "error");
            if (state != expectedState) waiting.TrySetResult((null, "state"));
            else if (!string.IsNullOrEmpty(code)) waiting.TrySetResult((code, null));
            else waiting.TrySetResult((null, error == "access_denied" ? "denied" : error ?? "denied"));
        }

        private static string Keep(TokenReply reply)
        {
            access = reply.access_token;
            accessUntil = DateTime.UtcNow.AddSeconds(Math.Max(60, reply.expires_in - 60));
            if (!string.IsNullOrEmpty(reply.refresh_token)) { try { PlayerPrefs.SetString(RefreshKey, reply.refresh_token); PlayerPrefs.Save(); } catch { } }
            return access;
        }

        private static async Task<(TokenReply, long)> Post(string form)
        {
            using (var request = new UnityWebRequest("https://oauth2.googleapis.com/token", "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(form))))
            {
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                request.timeout = 30;
                var done = new TaskCompletionSource<bool>();
                request.SendWebRequest().completed += _ => done.TrySetResult(true);
                await done.Task;
                TokenReply reply = null;
                try { reply = JsonUtility.FromJson<TokenReply>(request.downloadHandler.text); } catch { }
                return (reply, request.result == UnityWebRequest.Result.ConnectionError ? 0 : request.responseCode);
            }
        }

        public static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        private static string Esc(string s) => UnityWebRequest.EscapeURL(s ?? "");
        public static string Query(string url, string key)
        {
            int q = url.IndexOf('?');
            if (q < 0) return null;
            foreach (string pair in url.Substring(q + 1).Split('&'))
            {
                int eq = pair.IndexOf('=');
                if (eq > 0 && pair.Substring(0, eq) == key) return UnityWebRequest.UnEscapeURL(pair.Substring(eq + 1));
            }
            return null;
        }
    }
}
