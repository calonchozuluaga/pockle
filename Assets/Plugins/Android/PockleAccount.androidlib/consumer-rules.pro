# Called through Unity AndroidJavaClass; preserve the reflective JNI surface in release R8 builds.
-keep class studio.pockle.account.SecureSession { public static java.lang.String get(android.content.Context, java.lang.String); public static void put(android.content.Context, java.lang.String, java.lang.String); }
-keep class studio.pockle.account.AuthCallbackActivity { public <init>(); public void onCreate(android.os.Bundle); }
