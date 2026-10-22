package studio.pockle.account;
import android.content.Context;
import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import android.util.Base64;
import java.security.KeyStore;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;
/** Device-bound encrypted tokens; AndroidKeyStore key material never enters PlayerPrefs or cloud backup. */
public final class SecureSession {
 private static final String ALIAS="pockle.account.session.v1";
 private static SecretKey key() throws Exception {
  KeyStore store=KeyStore.getInstance("AndroidKeyStore");store.load(null);
  if(!store.containsAlias(ALIAS)){
   KeyGenerator generator=KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES,"AndroidKeyStore");
   generator.init(new KeyGenParameterSpec.Builder(ALIAS,KeyProperties.PURPOSE_ENCRYPT|KeyProperties.PURPOSE_DECRYPT)
    .setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE).build());generator.generateKey();
  }
  return (SecretKey)store.getKey(ALIAS,null);
 }
 public static void put(Context context,String name,String value) throws Exception {
  java.io.File file=new java.io.File(context.getNoBackupFilesDir(),name);
  if(value==null||value.length()==0){if(file.exists()&&!file.delete())throw new java.io.IOException("clear_failed");return;}
  Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.ENCRYPT_MODE,key());
  byte[] encrypted=cipher.doFinal(value.getBytes(java.nio.charset.StandardCharsets.UTF_8));
  String output=Base64.encodeToString(cipher.getIV(),Base64.NO_WRAP)+":"+Base64.encodeToString(encrypted,Base64.NO_WRAP);
  android.util.AtomicFile atomic=new android.util.AtomicFile(file);java.io.FileOutputStream stream=null;
  try{stream=atomic.startWrite();stream.write(output.getBytes(java.nio.charset.StandardCharsets.UTF_8));atomic.finishWrite(stream);}
  catch(Exception e){if(stream!=null)atomic.failWrite(stream);throw e;}
 }
 public static String get(Context context,String name) throws Exception {
  java.io.File file=new java.io.File(context.getNoBackupFilesDir(),name);if(!file.exists())return "";
  String text=new String(new android.util.AtomicFile(file).readFully(),java.nio.charset.StandardCharsets.UTF_8);String[] parts=text.split(":",2);
  Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.DECRYPT_MODE,key(),new GCMParameterSpec(128,Base64.decode(parts[0],Base64.NO_WRAP)));
  return new String(cipher.doFinal(Base64.decode(parts[1],Base64.NO_WRAP)),java.nio.charset.StandardCharsets.UTF_8);
 }
 private SecureSession(){}
}
