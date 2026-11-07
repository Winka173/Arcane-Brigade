package com.winka.arcanebrigade.sync;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.Intent;
import android.os.Bundle;

import com.google.android.gms.auth.api.identity.AuthorizationResult;
import com.google.android.gms.auth.api.identity.Identity;
import com.google.android.gms.common.api.ApiException;

/** A see-through step that shows Google's consent sheet for Drive access and passes the answer back to DriveAuth. */
public final class DriveAuthActivity extends Activity {
    static final String EXTRA = "pending";
    private static final int CODE = 7301;

    @Override
    protected void onCreate(Bundle saved) {
        super.onCreate(saved);
        if (saved != null) return; // recreated while the sheet is open: the sheet still answers onActivityResult
        PendingIntent pending = getIntent().getParcelableExtra(EXTRA);
        if (pending == null) { finishWith(null, "launch"); return; }
        try {
            startIntentSenderForResult(pending.getIntentSender(), CODE, null, 0, 0, 0);
        } catch (Exception e) {
            finishWith(null, "launch");
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode != CODE) return;
        if (resultCode != RESULT_OK || data == null) { finishWith(null, "denied"); return; }
        try {
            AuthorizationResult result = Identity.getAuthorizationClient(this).getAuthorizationResultFromIntent(data);
            String token = result.getAccessToken();
            finishWith(token, token == null ? "no-token" : null);
        } catch (ApiException e) {
            finishWith(null, "api-" + e.getStatusCode());
        }
    }

    private void finishWith(String token, String reason) {
        DriveAuth.Callback callback = DriveAuth.pending;
        DriveAuth.pending = null;
        if (callback != null) {
            if (token != null) callback.onToken(token); else callback.onFailure(reason);
        }
        finish();
        overridePendingTransition(0, 0);
    }
}
