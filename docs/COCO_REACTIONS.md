# Coco tap reactions

Coco has five two-second reactions on all ten finishes: HandleHello (a friendly turn and tilt that presents the handle), MarshmallowBounce, CreamWobble, HappyHop, and CozySway. Smiles, surprised mouths and sleepy eyes accompany the motion. The cup, peppermint heart, feet and handle keep their shape; topping masks above the rim provide small cream and marshmallow movements without changing the source mesh or paint.

Single taps choose uniformly among five actions initially, then among the four other actions. Taps during an active reaction do not interrupt it or queue another action. Gesture start, reset, collectible changes, disable and focus loss cancel the pose. Rotation, material-aware pinch, lift and double-tap reset retain their current behavior. Calm replaces movement/blinking with a small smile. No action tray or new audio is added; TryPlay remains available for future sound cues.

Core/CocoReactions contains engine-free timing and random selection. CocoReactionDriver coordinates the visual offset and HolidayExpressionDriver face; HolidayFinish's Coco-only topping controls leave other characters unchanged. All finish IDs, inventory and save format remain intact; this task adds no ownership grants.

Preview: docs/captures/coco-reactions/Coco-five-reactions.gif. Executed checks and phone status are recorded in VALIDATION.md.
