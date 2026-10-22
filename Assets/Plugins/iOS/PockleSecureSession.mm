#import <Foundation/Foundation.h>
#import <Security/Security.h>
#include <stdlib.h>
extern "C" {
 bool PockleSecureSet(const char* value) {
  @autoreleasepool {
   NSDictionary* query=@{(__bridge id)kSecClass:(__bridge id)kSecClassGenericPassword,(__bridge id)kSecAttrService:@"com.playpockle.session.v1",(__bridge id)kSecAttrAccount:@"session"};
   SecItemDelete((__bridge CFDictionaryRef)query);
   if(!value||!value[0])return true;
   NSMutableDictionary* add=[query mutableCopy];add[(__bridge id)kSecValueData]=[[NSString stringWithUTF8String:value] dataUsingEncoding:NSUTF8StringEncoding];
   add[(__bridge id)kSecAttrAccessible]=(__bridge id)kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly;
   return SecItemAdd((__bridge CFDictionaryRef)add,NULL)==errSecSuccess;
  }
 }
 const char* PockleSecureGet() {
  @autoreleasepool {
   NSDictionary* query=@{(__bridge id)kSecClass:(__bridge id)kSecClassGenericPassword,(__bridge id)kSecAttrService:@"com.playpockle.session.v1",(__bridge id)kSecAttrAccount:@"session",(__bridge id)kSecReturnData:@YES,(__bridge id)kSecMatchLimit:(__bridge id)kSecMatchLimitOne};
   CFTypeRef result=NULL;OSStatus status=SecItemCopyMatching((__bridge CFDictionaryRef)query,&result);
   if(status!=errSecSuccess)return strdup("");
   NSData* data=CFBridgingRelease(result);NSString* text=[[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];return strdup(text.UTF8String?:"");
  }
 }
 void PockleSecureFree(const char* value){free((void*)value);}
}
