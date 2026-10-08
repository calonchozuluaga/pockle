package studio.pockle.walking;

import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.SystemClock;
import android.provider.Settings;

/** Counts hardware-classified steps, never the toy's shake accelerometer. */
public final class StepCounter implements SensorEventListener {
    private final Context context;
    private final SensorManager manager;
    private final Sensor sensor;
    private volatile long count = -1;
    private boolean listening;

    public StepCounter(Context context) {
        this.context = context.getApplicationContext();
        manager = (SensorManager)this.context.getSystemService(Context.SENSOR_SERVICE);
        sensor = manager == null ? null : manager.getDefaultSensor(Sensor.TYPE_STEP_COUNTER);
    }

    public boolean start() {
        if (sensor == null) return false;
        if (!listening) listening = manager.registerListener(this, sensor, SensorManager.SENSOR_DELAY_NORMAL);
        return listening;
    }
    public void stop() { if (listening) manager.unregisterListener(this); listening = false; }
    public long total() { return count; }
    public long uptimeMillis() { return SystemClock.elapsedRealtime(); }
    public int boot() {
        try { return Settings.Global.getInt(context.getContentResolver(), "boot_count", -1); }
        catch (SecurityException exception) { return -1; }
    }
    @Override public void onSensorChanged(SensorEvent event) {
        if (event.sensor.getType() == Sensor.TYPE_STEP_COUNTER && event.values.length > 0)
            count = (long)event.values[0];
    }
    @Override public void onAccuracyChanged(Sensor sensor, int accuracy) { }
}
