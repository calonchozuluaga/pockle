#import <Foundation/Foundation.h>
#import <CoreMotion/CoreMotion.h>
#import <mach/mach_time.h>

// All state mutations are on Unity's main thread; Core Motion callbacks hop back to it.
@interface PocklePedometer : NSObject
@property(nonatomic,strong) CMPedometer* meter;
@property(nonatomic,strong) NSDate* day;
@property(nonatomic) NSInteger generation;
@property(nonatomic) BOOL querying, active;
@property(nonatomic) int state; // 0 disabled,1 requesting,2 active,3 denied,4 restricted,5 unavailable,6 error
@property(nonatomic) int64_t total;
-(void)resume:(BOOL)request;
-(void)stop;
@end
@implementation PocklePedometer
-(instancetype)init { if ((self=[super init])) { _meter=[CMPedometer new]; _total=-1; } return self; }
-(void)stop { [_meter stopPedometerUpdates]; _generation++; _querying=NO; _active=NO; _total=-1; }
-(void)resume:(BOOL)request {
 if (![CMPedometer isStepCountingAvailable]) { [self stop]; _state=5; return; }
 CMAuthorizationStatus auth=[CMPedometer authorizationStatus];
 if (auth==CMAuthorizationStatusDenied || auth==CMAuthorizationStatusRestricted) { [self stop]; _state=auth==CMAuthorizationStatusDenied?3:4; return; }
 if (auth==CMAuthorizationStatusNotDetermined && !request) { _state=0; return; }
 NSDate* day=[[NSCalendar currentCalendar] startOfDayForDate:[NSDate date]];
 if (_day && ![_day isEqualToDate:day]) [self stop];
 if (_active || _querying) return;
 if (_state==6 && !request) return; // A failed query retries only on explicit Enable, never every polling frame.
 _day=day; _querying=YES; _state=1;
 NSInteger generation=++_generation;
 [_meter queryPedometerDataFromDate:day toDate:[NSDate date] withHandler:^(CMPedometerData* data,NSError* error) {
  dispatch_async(dispatch_get_main_queue(), ^{
   if (generation!=self.generation) return;
   self.querying=NO;
   CMAuthorizationStatus current=[CMPedometer authorizationStatus];
   if (current==CMAuthorizationStatusDenied || current==CMAuthorizationStatusRestricted) { [self stop]; self.state=current==CMAuthorizationStatusDenied?3:4; return; }
   if (error || !data || current!=CMAuthorizationStatusAuthorized) { self.state=6; return; }
   self.total=MAX(0,data.numberOfSteps.longLongValue); self.active=YES; self.state=2;
   [self.meter startPedometerUpdatesFromDate:day withHandler:^(CMPedometerData* update,NSError* updateError) {
    dispatch_async(dispatch_get_main_queue(), ^{
     if (generation!=self.generation) return;
     if (updateError || !update) { [self stop]; self.state=6; return; }
     self.total=MAX(self.total,update.numberOfSteps.longLongValue);
    });
   }];
  });
 }];
}
@end
static PocklePedometer* PockleStepsService() { static PocklePedometer* service; static dispatch_once_t once; dispatch_once(&once, ^{service=[PocklePedometer new];}); return service; }
extern "C" {
 void PockleStepsResume(int request) { [PockleStepsService() resume:request!=0]; }
 int PockleStepsState() {
  PocklePedometer* s=PockleStepsService();
  if (s.state==2 && [CMPedometer authorizationStatus]!=CMAuthorizationStatusAuthorized) { [s resume:NO]; }
  return s.state;
 }
 int64_t PockleStepsTotal() { return PockleStepsService().total; }
 double PockleStepsUptime() { mach_timebase_info_data_t scale; mach_timebase_info(&scale); return (double)mach_continuous_time()*scale.numer/scale.denom/1e9; }
 // The cumulative counter belongs to the Core Motion query window, not the OS boot.
 // Local midnight/time-zone changes can reset it while the device stays running.
 // Minutes retain the start-of-day instant (including DST offsets) within the int ABI.
 int PockleStepsBoot() {
  NSDate* day=PockleStepsService().day;
  return day?(int)([day timeIntervalSince1970]/60.0):-1;
 }
 void PockleStepsStop() { [PockleStepsService() stop]; PockleStepsService().state=0; }
}
