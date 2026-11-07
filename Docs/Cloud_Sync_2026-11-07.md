# Cloud save (Google Drive)

Same design as Sticky Wall (`Sticky-Wall/docs/release/PLAY_CONSOLE_API_GUIDE.md`, section 10). The save is stored in the hidden app folder of the player's own Google Drive (`appDataFolder`, scope `drive.appdata` only). There is no game server, and no Firebase or Supabase.

## Where it lives

| Part | File |
|---|---|
| Merge, conflicts, device file, validation | `Assets/ArcaneBrigade/Runtime/Persistence/CloudSave.cs` |
| Drive REST (list, read, write in `appDataFolder`) | `Assets/ArcaneBrigade/Runtime/Persistence/DriveCloudBackend.cs` |
| Sign-in: Android bridge, iPhone OAuth with PKCE, revoke | `Assets/ArcaneBrigade/Runtime/Persistence/DriveAuth.cs` |
| Android authorization (Google Identity `AuthorizationClient`, play-services-auth 21.3.0) | `Assets/Plugins/Android/DriveAuth.androidlib/` |
| Triggers, status, applying, restore points | `Assets/ArcaneBrigade/Runtime/Presentation/CloudSync.cs` |
| Conflict choice and restore list | `Assets/ArcaneBrigade/Runtime/UI/CloudSaveCard.cs` |
| Settings → Cloud save section | `Assets/ArcaneBrigade/Runtime/UI/SettingsSheet.cs` (`CloudRows`) |
| Applying a run from another device | `Assets/ArcaneBrigade/Runtime/UI/RunMenu.cs` (`OnCloudRun`) |
| Upload at checkpoints | `Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs` (`PersistBoundary`, `OnApplicationPause`) |
| iOS URL scheme added at build time | `Assets/ArcaneBrigade/Editor/CloudSyncIosBuild.cs` |
| iOS client id (public) | `Assets/ArcaneBrigade/Resources/CloudSyncConfig.json` |
| Simulated devices (32 cases) | `Assets/ArcaneBrigade/Editor/CloudSyncChecks.cs` (menu *Arcane Brigade/Checks/Cloud sync*) |

## Behaviour

- **Offline-first.** The game saves locally exactly as before. Nothing needs a sign-in. Cloud save is off until the player taps *Connect Google*. It is not gated behind Deluxe.
- **Platforms.** Android now. iPhone as soon as `CloudSyncConfig.json` has an iOS client id. On PC the section is hidden. Android and iPhone share one save when both OAuth clients are in the same Google Cloud project.
- **Settings → Cloud save:**
  - *Google Drive*: *Connect Google* or *Disconnect*.
  - *Auto sync*: On or Off. When off, only *Sync now* syncs.
  - *Last synced*: the time, plus *Sync now*. The button becomes *Allow access* or *Choose a save* when needed.
  - *Restore…*: the list of restore points.
- **When it syncs (Auto sync on):**
  - when the main menu opens and when the game comes back to the front
  - a minute after a change in the menu
  - at checkpoints in a run: a wave or a purchase, at most once a minute
  - at once when the game goes to the background
  - In a run it only uploads. Changes from other devices are applied in the menu, never mid-fight.
- **What travels.** Three parts, each with its own Lamport revision:
  1. **Progress**: Lamp Oil, Keepsakes, the Lamp Tree, mastery, finds, records, history, achievements and story flags.
  2. **Settings that suit any device**: sound, language, comfort, display toggles and UI style.
  3. **The run in play**: roster, relics, gold and wave.
- **What stays on the device:** window or resolution, vsync, frame rate, phone sharpness and the last difficulty picked. Deluxe never travels; Google Play restores it.
- **One file per device.** Each device writes only `ab-dev-<deviceId>.json`, so two devices never overwrite each other's file.

### Merge and conflicts

- **Combined from every device, without asking** (things that only grow):
  - best wave, wins and kills
  - every *Seen* list, achievements, finished runs (newest 50) and best score per record
  - tips and story flags
  - keepsake sources, letters, mastery, pact and daily bests, and difficulties won
- **The newest change wins** for what is spent or chosen, for settings, and for the run.
  - A stale copy never wins.
  - A run that ended on one device leaves play on the others. The local copy goes to `ArchivedRuns`; it is never deleted.
- **Conflicts.** If this device and another both changed the progress since they last synced, and they disagree on what is spent or chosen, nothing is overwritten. The same applies when they hold two different runs. The card *Two saves differ* shows both sides (best wave, Lamp Oil, run in play, device, time) and the player picks:
  - *Keep this device*: it leads everywhere. The Drive side is kept as a restore point.
  - *Use Drive save*: this device's side is kept as a restore point first.
  - Finds and records from both are kept either way. During a run a conflict only defers. The menu asks.
- **Joining.** The first sync of a device that has played, when Drive already holds a save that has played, is also a conflict, so the player picks. An empty phone just takes the Drive save. A played phone joining an empty Drive leads.
- **Safety:**
  - Every file goes through the save's own load checks before use. A damaged file is ignored.
  - A file from a newer game version is left alone, with *Update the game* shown.
  - A merge the save would refuse is never applied.
  - A change made while a sync is in flight is never lost; it syncs next.

### Restore points

- They are kept on the device in `CloudBackups/`, newest 20, with no duplicates.
- One is made:
  - before using the Drive save in a conflict
  - for the Drive side when the player keeps this device
  - before every restore
- *Restore…* lists them (time, reason, best wave, Lamp Oil). A second tap restores. The restored save then syncs as the newest change.
- There are no multiple save slots. The game has one profile and one run in play.

### Disconnect

- Google revokes the token (`oauth2.googleapis.com/revoke`), sync stops, and the save on the device is untouched.
- The Drive copy stays until the player deletes it: Drive → Settings → Manage apps → Arcane Brigade → Delete hidden app data.

## Compatibility

- `profile.json` and `run.json` keep their format and checksum envelope. Older saves load exactly as before.
- Sync state lives in its own file, `cloudsync.json`, which is never uploaded.
- The device file has a `Format` number. A newer format is ignored by an older game.

## Owner setup in Google Cloud Console (manual, once)

Until this is done, *Connect Google* shows Google's error and the game keeps saving locally.

1. **Project.** Create a dedicated project, for example **Arcane Brigade**. Do not reuse `play-publisher-513712`: its consent screen is set up for Sticky Wall, and the consent screen shows the project's app name. Use the same project for Android and iPhone so they share one save.
2. **APIs & Services → Library:** enable the **Google Drive API**.
3. **Google Auth Platform (OAuth consent screen):**
   - Audience: **External**.
   - Branding:
     - App name **Arcane Brigade**
     - Support email `apps.winka.studio@gmail.com`
     - App logo: `Assets/ArcaneBrigade/Art/Store/AppIcon.png` (optional)
     - Privacy policy `https://github.com/Winka173/Arcane-Brigade/blob/main/PRIVACY.md`
     - Developer email
   - Data access: add `https://www.googleapis.com/auth/drive.appdata`. It is not a sensitive or restricted scope, so no security assessment is needed.
   - While the screen is in *Testing*, only listed **test users** can connect (up to 100). Add the closed testers' Google accounts, or *Publish app* before the public release.
4. **Credentials → Create credentials → OAuth client ID → Android**, twice:

   | Client | Package | SHA-1 |
   |---|---|---|
   | Play app signing (every install from Play, testers included) | `com.winka.arcanebrigade` | `2B:F2:8F:B3:F2:A6:F8:36:BA:F4:AA:EF:B3:F3:EA:17:7F:E4:F2:20` |
   | Upload key (locally built or sideloaded release builds) | `com.winka.arcanebrigade` | `E7:4F:28:74:B6:A3:98:F2:55:20:45:A6:5C:D5:2C:6A:9D:02:59:CF` |

   - The Play signing SHA-1 was read from the universal APK Play generated for version 6. Its SHA-256 `EF:1B:0B:EF:89:0D:51:3D:54:62:F0:5B:4D:C8:02:1D:7C:6E:20:28:13:B7:FB:85:3D:AC:F4:92:D7:39:31:3A` matches Play Console → App integrity.
   - Nothing goes into the Android build: no `google-services.json` and no client id. Google matches package and certificate.
5. **iPhone (when there is an iOS build):** *Create OAuth client ID → iOS* in the same project.
   - Bundle ID: the iOS bundle id in Player Settings.
   - Team ID: from the Apple developer account.
   - Put the client id (`<number>-<id>.apps.googleusercontent.com`) into `Assets/ArcaneBrigade/Resources/CloudSyncConfig.json` → `iosClientId`.
   - The build adds the URL scheme `com.googleusercontent.apps.<number>-<id>` by itself.
   - Needs a Mac to build and test. It has never been run.
6. **Play Console → Data safety:** the data goes only to the player's own Google Drive, at their request, and never to the developer. Answer the same way as for Sticky Wall. `PRIVACY.md` already describes it.

Changes in Google Cloud can take minutes, sometimes hours, to reach devices.

## Testing on real Drive

1. Install the closed-testing build on two Android devices signed in to a Google account that is a test user.
2. Settings → Cloud save → *Connect Google* → allow.
3. Check each case:
   - progress and a setting changed on each device
   - a run started on one device and continued on the other
   - one device offline while the other plays
   - Lamp Oil spent on both devices, which should show *Two saves differ*
   - *Restore…*
   - *Auto sync* off, then *Sync now*
   - *Disconnect*
   - access taken back in the Google Account (Security → Third-party connections)

| What you see | Cause | Fix |
|---|---|---|
| *Allow access again* right after allowing; logcat `api-10` / `DEVELOPER_ERROR` | No Android client for this package + SHA-1 | Add the client for the key that signed this install |
| `access_denied` / app not verified | The account is not a test user | Add it, or publish the consent screen |
| *Couldn't sync*, logcat `HTTP 403` | Drive API not enabled in the project | Step 2 |
| iPhone returns to the game without connecting | URL scheme or client id missing | Step 5 |

## Verification

| Level | Status |
|---|---|
| Merge, conflicts, run, settings, checkpoints, restore points (`CloudSyncChecks`, 32 cases) | **Passed** 7/11/2026 |
| Localization checks after the new strings | **Passed** |
| Android bundle with `DriveAuth.androidlib` | **Built** (Unity 6000.6, AGP 9.1.1) |
| Real Google Drive, two devices | **Blocked** until the owner setup above |
| iPhone | **Not built** (needs a Mac and the iOS client) |
