# Snowy tap reactions

Snowy has five two-second reactions across all ten finishes: MittenWave, PomWobble, FrostyShuffle, SnowHop, and CozyNod. Expressions coordinate happy eyes/smiles, surprised mouths, and a sleepy nod. Local mitten, hat/pom and head masks carry the original paint without modifying the source mesh. The nod keeps the scarf, torso and feet planted; rigid finishes keep their whole-body scale.

Single taps choose uniformly among all five initially, then among the four other actions. Taps during an active reaction do not interrupt it or queue another. Gesture start, reset, collectible changes, disable and focus loss cancel the pose. Rotation, material-aware pinch, lift and double-tap reset remain intact. Calm uses a small smile with no body/appendage motion or blinking.

Core/SnowyReactions contains engine-free timing/selection. SnowyReactionDriver coordinates the visual offset, HolidayExpressionDriver face, and Snowy-only HolidayFinish articulation. Other character paths and finish recipes are preserved. No action tray, audio, new inventory grants or save format changes are added; TryPlay remains available for future sound cues.

Native Unity preview: docs/captures/snowy-reactions/Snowy-five-reactions.gif. Executed checks and device status are recorded in VALIDATION.md.
