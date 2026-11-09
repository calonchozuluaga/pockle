package studio.pockle.avatar;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.media.ExifInterface;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.MediaStore;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import org.json.JSONObject;

/** Selected-image access only. No gallery enumeration, broad storage grant, or network request. */
public final class AvatarPickerActivity extends Activity {
    private static String pending = "", destination = "", result = "";
    private static AvatarPickerActivity active;
    private String request;
    public static synchronized boolean pick(Activity parent, String token, String path) {
        if (!pending.isEmpty() || token == null || !token.matches("[a-f0-9]{32}")) return false;
        pending = token; destination = path; result = "";
        parent.runOnUiThread(() -> {
            if (!current(token)) return;
            try { parent.startActivity(new Intent(parent, AvatarPickerActivity.class).putExtra("request", token)); }
            catch (RuntimeException e) { finishResult(token, "error"); }
        });
        return true;
    }
    public static synchronized String takeResult() { String value = result; result = ""; return value; }
    private static synchronized boolean current(String token) { return token != null && token.equals(pending); }
    private static synchronized void finishResult(String token, String status) {
        if (!current(token)) return;
        try { result = new JSONObject().put("request", token).put("status", status).toString(); } catch (Exception ignored) { result = ""; }
        pending = ""; destination = "";
    }
    public static synchronized void cancel(String token) {
        if (!current(token)) return;
        pending = ""; destination = ""; result = "";
        AvatarPickerActivity old = active;
        if (old != null) old.runOnUiThread(old::finish);
    }
    @Override public void onCreate(Bundle state) {
        super.onCreate(state); request = getIntent().getStringExtra("request"); active = this;
        if (!current(request)) { finish(); return; }
        if (state != null) return;
        Intent intent = Build.VERSION.SDK_INT >= 33 ? new Intent(MediaStore.ACTION_PICK_IMAGES) : new Intent(Intent.ACTION_OPEN_DOCUMENT).addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("image/*"); intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
        try { startActivityForResult(intent, 1); }
        catch (RuntimeException error) { finishResult(request, "error"); finish(); }
    }
    @Override public void onActivityResult(int code, int status, Intent data) {
        super.onActivityResult(code, status, data);
        if (code != 1) return;
        if (status != RESULT_OK || data == null || data.getData() == null) { finishResult(request, "cancel"); finish(); return; }
        Uri uri = data.getData();
        new Thread(() -> {
            Bitmap bitmap = null, oriented = null;
            File output = null;
            try {
                BitmapFactory.Options options = new BitmapFactory.Options(); options.inJustDecodeBounds = true;
                try (InputStream input = getContentResolver().openInputStream(uri)) { BitmapFactory.decodeStream(input, null, options); }
                if (options.outWidth < 1 || options.outHeight < 1 || options.outWidth > 100000 || options.outHeight > 100000) throw new IllegalArgumentException();
                options.inSampleSize = 1;
                while ((Math.max(options.outWidth, options.outHeight) + options.inSampleSize - 1) / options.inSampleSize > 2048) options.inSampleSize *= 2;
                options.inJustDecodeBounds = false;
                try (InputStream input = getContentResolver().openInputStream(uri)) { bitmap = BitmapFactory.decodeStream(input, null, options); }
                if (bitmap == null) throw new IllegalArgumentException();
                int orientation = ExifInterface.ORIENTATION_NORMAL;
                try (InputStream input = getContentResolver().openInputStream(uri)) { orientation = new ExifInterface(input).getAttributeInt(ExifInterface.TAG_ORIENTATION, ExifInterface.ORIENTATION_NORMAL); }
                catch (Exception ignored) { }
                Matrix matrix = new Matrix();
                switch (orientation) {
                    case ExifInterface.ORIENTATION_FLIP_HORIZONTAL: matrix.setScale(-1, 1); break;
                    case ExifInterface.ORIENTATION_ROTATE_180: matrix.setRotate(180); break;
                    case ExifInterface.ORIENTATION_FLIP_VERTICAL: matrix.setScale(1, -1); break;
                    case ExifInterface.ORIENTATION_TRANSPOSE: matrix.setRotate(90); matrix.postScale(-1, 1); break;
                    case ExifInterface.ORIENTATION_ROTATE_90: matrix.setRotate(90); break;
                    case ExifInterface.ORIENTATION_TRANSVERSE: matrix.setRotate(-90); matrix.postScale(-1, 1); break;
                    case ExifInterface.ORIENTATION_ROTATE_270: matrix.setRotate(-90); break;
                }
                oriented = Bitmap.createBitmap(bitmap, 0, 0, bitmap.getWidth(), bitmap.getHeight(), matrix, true);
                synchronized (AvatarPickerActivity.class) {
                    if (!current(request)) return;
                    output = new File(destination);
                    try (FileOutputStream stream = new FileOutputStream(output)) {
                        if (!oriented.compress(Bitmap.CompressFormat.JPEG, 90, stream)) throw new IllegalArgumentException();
                    }
                    if (output.length() > 8 * 1024 * 1024) throw new IllegalArgumentException();
                    finishResult(request, "ok");
                }
            } catch (Exception | OutOfMemoryError error) { if (output != null) output.delete(); finishResult(request, "error"); }
            finally {
                if (oriented != null && oriented != bitmap) oriented.recycle(); if (bitmap != null) bitmap.recycle();
                runOnUiThread(this::finish);
            }
        }, "Pockle selected photo").start();
    }
    @Override public void onDestroy() { if (isFinishing()) finishResult(request, "cancel"); if (active == this) active = null; super.onDestroy(); }
}
