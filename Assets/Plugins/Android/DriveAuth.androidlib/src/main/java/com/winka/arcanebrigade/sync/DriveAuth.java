package com.winka.arcanebrigade.sync;

import android.app.Activity;
import android.content.Intent;

import com.google.android.gms.auth.GoogleAuthUtil;
import com.google.android.gms.auth.api.identity.AuthorizationRequest;
import com.google.android.gms.auth.api.identity.Identity;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.common.api.Scope;

import java.util.Collections;

/** Drive access for "Sync across devices": asks Google for a drive.appdata token. Called from Unity (AndroidDriveAuth). */
public final class DriveAuth {
    public interface Callback {
        void onToken(String token);
        void onFailure(String reason);
    }

    static final String SCOPE = "https://www.googleapis.com/auth/drive.appdata";
    static Callback pending;

    private DriveAuth() {}

    /** A token for the Drive app folder. When the player must allow access first: Google's sheet if interactive, else "consent". */
    public static void authorize(final Activity activity, final boolean interactive, final Callback callback) {
        activity.runOnUiThread(() -> {
            try {
                AuthorizationRequest request = AuthorizationRequest.builder()
                        .setRequestedScopes(Collections.singletonList(new Scope(SCOPE)))
                        .build();
                Identity.getAuthorizationClient(activity).authorize(request)
                        .addOnSuccessListener(result -> {
                            if (result.hasResolution()) {
                                if (!interactive || result.getPendingIntent() == null) { callback.onFailure("consent"); return; }
                                pending = callback;
                                Intent intent = new Intent(activity, DriveAuthActivity.class);
                                intent.putExtra(DriveAuthActivity.EXTRA, result.getPendingIntent());
                                activity.startActivity(intent);
                            } else if (result.getAccessToken() != null) {
                                callback.onToken(result.getAccessToken());
                            } else {
                                callback.onFailure("no-token");
                            }
                        })
                        .addOnFailureListener(e -> callback.onFailure(describe(e)));
            } catch (Exception e) {
                callback.onFailure(describe(e));
            }
        });
    }

    /** Drops a token Drive refused, so the next authorize returns a fresh one. */
    public static void clearToken(final Activity activity, final String token) {
        final android.content.Context context = activity.getApplicationContext();
        new Thread(() -> {
            try { GoogleAuthUtil.clearToken(context, token); } catch (Exception ignored) { }
        }).start();
    }

    static String describe(Exception e) {
        if (e instanceof ApiException) return "api-" + ((ApiException) e).getStatusCode();
        return e.getClass().getSimpleName() + ": " + e.getMessage();
    }
}
