# Unity invokes this class through JNI; R8 cannot see those calls.
-keep class studio.pockle.walking.StepCounter { public *; }
