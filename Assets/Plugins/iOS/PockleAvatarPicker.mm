#import <UIKit/UIKit.h>
#import <PhotosUI/PhotosUI.h>
#import <ImageIO/ImageIO.h>
#import <UniformTypeIdentifiers/UniformTypeIdentifiers.h>
#include <stdlib.h>
#include <string.h>

extern UIViewController *UnityGetGLViewController(void);
static NSString *PockleAvatarRequest = @"";
static NSString *PockleAvatarDestination = @"";
static NSString *PockleAvatarReply = @"";
static UIViewController *PocklePhotoPicker;

static void AvatarReply(NSString *request, NSString *status) {
    if (![PockleAvatarRequest isEqualToString:request]) return;
    NSData *data = [NSJSONSerialization dataWithJSONObject:@{@"request":request, @"status":status} options:0 error:nil];
    PockleAvatarReply = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    PockleAvatarRequest = @"";
    PockleAvatarDestination = @"";
}

API_AVAILABLE(ios(14.0))
@interface PockleAvatarDelegate : NSObject <PHPickerViewControllerDelegate, UIAdaptivePresentationControllerDelegate>
@property(nonatomic, copy) NSString *request;
@property(nonatomic, copy) NSString *destination;
@end
@implementation PockleAvatarDelegate
- (void)picker:(PHPickerViewController *)picker didFinishPicking:(NSArray<PHPickerResult *> *)results {
    NSString *request = self.request;
    NSString *destination = self.destination;
    if (![PockleAvatarRequest isEqualToString:request] || picker != PocklePhotoPicker) return;
    [picker dismissViewControllerAnimated:YES completion:nil];
    PocklePhotoPicker = nil;
    if (results.count == 0) { AvatarReply(request, @"cancel"); return; }
    NSItemProvider *provider = results.firstObject.itemProvider;
    if (![provider canLoadObjectOfClass:[UIImage class]]) { AvatarReply(request, @"error"); return; }
    NSString *type = nil;
    for (NSString *candidate in provider.registeredTypeIdentifiers)
        if ([provider hasItemConformingToTypeIdentifier:candidate] && [[UTType typeWithIdentifier:candidate] conformsToType:UTTypeImage]) { type = candidate; break; }
    if (type == nil) { AvatarReply(request, @"error"); return; }
    [provider loadFileRepresentationForTypeIdentifier:type completionHandler:^(NSURL *url, NSError *error) {
        // ImageIO downsamples before decoding; a large camera photo cannot become a full-resolution Unity texture.
        CGImageSourceRef source = url && !error ? CGImageSourceCreateWithURL((__bridge CFURLRef)url, NULL) : NULL;
        NSDictionary *options = @{ (NSString *)kCGImageSourceCreateThumbnailFromImageAlways:@YES,
            (NSString *)kCGImageSourceCreateThumbnailWithTransform:@YES,
            (NSString *)kCGImageSourceThumbnailMaxPixelSize:@2048,
            (NSString *)kCGImageSourceShouldCacheImmediately:@YES };
        CGImageRef image = source ? CGImageSourceCreateThumbnailAtIndex(source, 0, (__bridge CFDictionaryRef)options) : NULL;
        if (source) CFRelease(source);
        NSMutableData *bytes = [NSMutableData data];
        CGImageDestinationRef output = image ? CGImageDestinationCreateWithData((__bridge CFMutableDataRef)bytes, CFSTR("public.jpeg"), 1, NULL) : NULL;
        BOOL encoded = NO;
        if (output) {
            // Only new pixels and compression quality are written: no source EXIF/GPS or filename.
            CGImageDestinationAddImage(output, image, (__bridge CFDictionaryRef)@{(NSString *)kCGImageDestinationLossyCompressionQuality:@0.9});
            encoded = CGImageDestinationFinalize(output); CFRelease(output);
        }
        if (image) CGImageRelease(image);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (![PockleAvatarRequest isEqualToString:request]) return;
            NSError *writeError = nil;
            BOOL ok = encoded && bytes.length > 0 && bytes.length <= 8 * 1024 * 1024 &&
                [bytes writeToFile:destination options:NSDataWritingAtomic | NSDataWritingFileProtectionComplete error:&writeError];
            AvatarReply(request, ok && writeError == nil ? @"ok" : @"error");
        });
    }];
}
- (void)presentationControllerDidDismiss:(UIPresentationController *)presentationController {
    if (presentationController.presentedViewController != PocklePhotoPicker) return;
    PocklePhotoPicker = nil; AvatarReply(self.request, @"cancel");
}
@end
static NSObject *PocklePickerDelegate;

extern "C" bool PockleAvatarPick(const char *request, const char *destination) {
    if (@available(iOS 14.0, *)) {
        if (PockleAvatarRequest.length || !request || !destination) return false;
        PockleAvatarRequest = [NSString stringWithUTF8String:request];
        PockleAvatarDestination = [NSString stringWithUTF8String:destination]; PockleAvatarReply = @"";
        NSString *token = PockleAvatarRequest;
        dispatch_async(dispatch_get_main_queue(), ^{
            if (![PockleAvatarRequest isEqualToString:token]) return;
            PHPickerConfiguration *config = [[PHPickerConfiguration alloc] init];
            config.selectionLimit = 1; config.filter = [PHPickerFilter imagesFilter];
            PHPickerViewController *picker = [[PHPickerViewController alloc] initWithConfiguration:config];
            PocklePhotoPicker = picker;
            PockleAvatarDelegate *delegate = [PockleAvatarDelegate new]; delegate.request = token; delegate.destination = PockleAvatarDestination;
            PocklePickerDelegate = delegate;
            picker.delegate = (id<PHPickerViewControllerDelegate>)PocklePickerDelegate;
            UIViewController *presenter = UnityGetGLViewController();
            while (presenter.presentedViewController != nil) presenter = presenter.presentedViewController;
            if (presenter == nil) { AvatarReply(PockleAvatarRequest, @"error"); return; }
            [presenter presentViewController:PocklePhotoPicker animated:YES completion:nil];
            picker.presentationController.delegate = delegate;
        });
        return true;
    }
    return false;
}
extern "C" char *PockleAvatarResult(void) {
    NSString *result = PockleAvatarReply; PockleAvatarReply = @"";
    return strdup([result UTF8String] ?: "");
}
extern "C" void PockleAvatarFree(char *value) { free(value); }
extern "C" void PockleAvatarCancel(const char *request) {
    NSString *token = request ? [NSString stringWithUTF8String:request] : @"";
    if (![PockleAvatarRequest isEqualToString:token]) return;
    PockleAvatarRequest = @""; PockleAvatarDestination = @""; PockleAvatarReply = @"";
    UIViewController *oldPicker = PocklePhotoPicker; PocklePhotoPicker = nil;
    dispatch_async(dispatch_get_main_queue(), ^{
        [oldPicker dismissViewControllerAnimated:YES completion:nil];
    });
}
