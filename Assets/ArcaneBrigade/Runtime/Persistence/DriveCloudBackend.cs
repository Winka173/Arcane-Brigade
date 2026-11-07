using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcaneBrigade
{
    /// <summary>Google Drive's hidden app folder (appDataFolder): only this game sees the files there; the player's own Drive files
    /// are never listed or touched, and the player can delete the data from Drive's settings (Manage apps). The only scope asked
    /// for is drive.appdata. Needs Android OAuth clients for this package and its signing certificates in Google Cloud
    /// (Docs/Cloud_Sync_2026-11-07.md); no client id lives in the game.</summary>
    public sealed class DriveCloudBackend : ICloudBackend
    {
        public const string Scope = "https://www.googleapis.com/auth/drive.appdata";
        private const string Api = "https://www.googleapis.com/drive/v3/files";
        private const string Upload = "https://www.googleapis.com/upload/drive/v3/files";
        private const string Prefix = "ab-dev-";
        private string token;
        private readonly Dictionary<string, string> ids = new Dictionary<string, string>();

        /// <summary>Asks the player to let the game use its Drive app folder (Google's own sheet). Null on success, else why not.</summary>
        public static async Task<string> Authorize()
        {
            var (value, error) = await DriveAuth.Token(true);
            return value != null ? null : error;
        }

        private async Task<string> AccessToken(bool fresh)
        {
            if (token != null && !fresh) return token;
            var (value, error) = await DriveAuth.Token(false);
            if (value == null) throw new CloudSyncException(error != null && error.StartsWith("api-7") ? CloudFailure.Offline : CloudFailure.AuthNeeded, error);
            return token = value;
        }

        private async Task<string> Send(Func<UnityWebRequest> make)
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                string bearer = await AccessToken(attempt > 0);
                using (UnityWebRequest request = make())
                {
                    request.SetRequestHeader("Authorization", "Bearer " + bearer);
                    request.timeout = 60;
                    var done = new TaskCompletionSource<bool>();
                    request.SendWebRequest().completed += _ => done.TrySetResult(true);
                    await done.Task;
                    long code = request.responseCode;
                    string body = request.downloadHandler?.text ?? "";
                    if (request.result == UnityWebRequest.Result.ConnectionError) throw new CloudSyncException(CloudFailure.Offline, request.error);
                    if (code == 401 && attempt == 0) { DriveAuth.Clear(bearer); token = null; continue; } // a stale token: once more
                    if (code >= 200 && code < 300) return body;
                    if (code == 401) throw new CloudSyncException(CloudFailure.AuthNeeded);
                    if (code == 403 && body.Contains("storageQuotaExceeded")) throw new CloudSyncException(CloudFailure.Quota);
                    if (code == 0 || code == 429 || code >= 500) throw new CloudSyncException(CloudFailure.Offline, "HTTP " + code);
                    throw new CloudSyncException(CloudFailure.Other, "HTTP " + code);
                }
            }
            throw new CloudSyncException(CloudFailure.AuthNeeded);
        }

        [Serializable] private sealed class FileEntry { public string id, name, version; }
        [Serializable] private sealed class FileList { public string nextPageToken; public FileEntry[] files; }
        [Serializable] private sealed class Created { public string id; }

        public async Task<Dictionary<string, string>> ListDevices()
        {
            var result = new Dictionary<string, string>();
            string page = null;
            do
            {
                string query = "spaces=appDataFolder&pageSize=1000&fields=" + UnityWebRequest.EscapeURL("nextPageToken,files(id,name,version)")
                    + "&q=" + UnityWebRequest.EscapeURL("name contains '" + Prefix + "' and trashed = false")
                    + (page != null ? "&pageToken=" + UnityWebRequest.EscapeURL(page) : "");
                FileList list = JsonUtility.FromJson<FileList>(await Send(() => UnityWebRequest.Get(Api + "?" + query)));
                foreach (FileEntry f in list?.files ?? Array.Empty<FileEntry>())
                {
                    if (f?.name == null || !f.name.StartsWith(Prefix, StringComparison.Ordinal) || !f.name.EndsWith(".json", StringComparison.Ordinal)) continue;
                    ids[f.name] = f.id;
                    result[f.name.Substring(Prefix.Length, f.name.Length - Prefix.Length - 5)] = f.version ?? "";
                }
                page = string.IsNullOrEmpty(list?.nextPageToken) ? null : list.nextPageToken;
            } while (page != null);
            return result;
        }

        public async Task<string> ReadDevice(string deviceId)
        {
            if (!ids.TryGetValue(Prefix + deviceId + ".json", out string id)) return null;
            return await Send(() => UnityWebRequest.Get(Api + "/" + id + "?alt=media"));
        }

        public async Task WriteDevice(string deviceId, string document)
        {
            string name = Prefix + deviceId + ".json";
            byte[] bytes = Encoding.UTF8.GetBytes(document);
            if (ids.TryGetValue(name, out string id))
            {
                await Send(() =>
                {
                    var r = new UnityWebRequest(Upload + "/" + id + "?uploadType=media", "PATCH", new DownloadHandlerBuffer(), new UploadHandlerRaw(bytes));
                    r.SetRequestHeader("Content-Type", "application/json");
                    return r;
                });
                return;
            }
            const string boundary = "arcane-brigade-sync-boundary";
            string head = "--" + boundary + "\r\nContent-Type: application/json; charset=UTF-8\r\n\r\n{\"name\":\"" + name + "\",\"parents\":[\"appDataFolder\"]}\r\n--" + boundary + "\r\nContent-Type: application/json\r\n\r\n";
            byte[] a = Encoding.UTF8.GetBytes(head), z = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            byte[] body = new byte[a.Length + bytes.Length + z.Length];
            Buffer.BlockCopy(a, 0, body, 0, a.Length); Buffer.BlockCopy(bytes, 0, body, a.Length, bytes.Length); Buffer.BlockCopy(z, 0, body, a.Length + bytes.Length, z.Length);
            string created = await Send(() =>
            {
                var r = new UnityWebRequest(Upload + "?uploadType=multipart&fields=id", "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(body));
                r.SetRequestHeader("Content-Type", "multipart/related; boundary=" + boundary);
                return r;
            });
            string newId = JsonUtility.FromJson<Created>(created)?.id;
            if (!string.IsNullOrEmpty(newId)) ids[name] = newId;
        }
    }

    /// <summary>The Android side of Drive access (Assets/Plugins/Android/DriveAuth.androidlib): Google Identity's authorization
    /// client hands an access token for drive.appdata; the first time, or after access was taken back, Google shows its own sheet.</summary>
    public static class AndroidDriveAuth
    {
        private const string Bridge = "com.winka.arcanebrigade.sync.DriveAuth";

        private sealed class Callback : AndroidJavaProxy
        {
            private readonly TaskCompletionSource<(string, string)> done;
            private readonly System.Threading.SynchronizationContext unity;
            public Callback(TaskCompletionSource<(string, string)> done, System.Threading.SynchronizationContext unity) : base(Bridge + "$Callback") { this.done = done; this.unity = unity; }
            // called on Android's thread: the answer goes back to Unity's
            public void onToken(string value) => unity.Post(_ => done.TrySetResult((value, null)), null);
            public void onFailure(string reason) => unity.Post(_ => done.TrySetResult((null, reason ?? "failed")), null);
        }

        private static AndroidJavaObject Activity()
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) return player.GetStatic<AndroidJavaObject>("currentActivity");
        }

        /// <summary>A token, or why there is none ("consent" when the player must allow access first and <paramref name="interactive"/>
        /// is false; "denied" when they said no; "api-N" for a Google status code).</summary>
        public static Task<(string Token, string Error)> Token(bool interactive)
        {
            var done = new TaskCompletionSource<(string, string)>();
            if (Application.platform != RuntimePlatform.Android) { done.SetResult((null, "unsupported")); return done.Task; }
            try
            {
                using (var bridge = new AndroidJavaClass(Bridge))
                using (AndroidJavaObject activity = Activity())
                    bridge.CallStatic("authorize", activity, interactive, new Callback(done, System.Threading.SynchronizationContext.Current));
            }
            catch (Exception exception) { done.TrySetResult((null, "bridge: " + exception.Message)); }
            return done.Task;
        }

        /// <summary>Drops a token Drive refused, so the next request gets a fresh one.</summary>
        public static void Clear(string token)
        {
            if (Application.platform != RuntimePlatform.Android) return;
            try
            {
                using (var bridge = new AndroidJavaClass(Bridge))
                using (AndroidJavaObject activity = Activity())
                    bridge.CallStatic("clearToken", activity, token);
            }
            catch (Exception exception) { Debug.LogWarning("Drive token not cleared: " + exception.Message); }
        }
    }
}
