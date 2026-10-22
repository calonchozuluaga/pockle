package studio.pockle.account;
import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.net.Uri;
/** Redirect only authorization codes to Unity. PKCE verifier and request binding are checked there. */
public final class AuthCallbackActivity extends Activity {
 @Override public void onCreate(Bundle state) {
  super.onCreate(state); Uri uri=getIntent().getData();
  if(uri!=null&&"com.playpockle.app".equals(uri.getScheme())&&"auth".equals(uri.getHost())&&"/callback".equals(uri.getPath())) {
   Intent launch=getPackageManager().getLaunchIntentForPackage(getPackageName());
   if(launch!=null){launch.setData(uri);launch.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP|Intent.FLAG_ACTIVITY_SINGLE_TOP);startActivity(launch);}
  }
  finish();
 }
}
