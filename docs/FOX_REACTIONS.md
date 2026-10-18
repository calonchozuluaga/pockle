# Fox signature reactions — UX-025

All ten Fox finishes now share five Fox-specific tap reactions, on the existing approved Fox v2 mesh and paint:

1. Jump: a short hop, excited ears/tail and bright expression.
2. Tail Wag: anchored tail swishes, with happy eyes/mouth and a small body response.
3. Curious Tilt: the head tilts through the neck while the ears perk; the body stays planted.
4. Sleepy Blink: eyes close, ears settle and the head leans gently.
5. Greeting Bow: a head dip with planted paws, a happy expression and tail flourish.

Each single tap chooses a random reaction, excluding the previous one. First choice is uniform among five; subsequent choices are uniform among the other four. Existing gesture cancellation and the delayed tap preserve double-tap reset. Fox remains rigid for touch handling; zoom/pan, free rotation, Pip random reactions and soft-toy gestures are preserved.

`FoxReactions.Sample` provides engine-free, bounded two-second poses and Calm alternatives. `FoxDeformation` articulates the tail/head/ears and painted eye/mouth geometry with smooth spatial masks calibrated to the approved v2 mesh. The tail root blends into the body; front paws stay unchanged in a wag. Trigonometry is computed per pose. `JellyToy.SetFoxReaction` applies this to its existing instance mesh and normal buffers; the source asset, UVs, materials and stable GUIDs are untouched. Rigid Fox bypasses generic jelly deformation. This is procedural articulation, not a new skeletal rig or a new Meshy model.

`FoxReactionDriver.TryPlay(FoxAction)` is an explicit future sound/event hook. Body transforms use a separate reaction offset; eye/mouth/tail poses are applied after touch updates. `TactilePrototype` dispatches a tap by character, retaining separate Pip/Fox reaction histories. Glow follows the actual visual origin after reactions. No sounds, UI tray, awards, save or engine/settings/package changes.

Calm substitutes a slight happy facial pose without jumps, tail/head/ear motion or blinks. Focus loss, menus, dragging/holding and selection changes cancel reactions; pause freezes them. Every pose returns exactly to rest. Timing uses elapsed unscaled time so slow frames do not prolong the clip.

## Verification

Unity6000.6.5f1 focused PlayMode7/7 passed: all ten Fox finishes, five actions, visible tail/eye deformation, unchanged paws/source asset, exact reset/no vertex drift,100 random choices without consecutive repeats, reaction completion/switching, existing finish materials/grants, Pip reactions, soft/rigid gestures, glow and viewer controls. Portable Core passed; source checks145 C# /504 GUIDs passed. Task whitespace checks passed. Existing combined UI suite failures recorded previously remain outside this change; no new green full-suite or hosted-CI claim.

Native Unity previews were rendered for all five actions; jump, wag, tilt and blink poses were visually inspected. GIFs are native encodings of those frames, not generated concept art: docs/captures/fox-reactions/Fox-five-reactions.gif. Phone installation and owner motion/performance acceptance are recorded separately in VALIDATION.md and the Codex handoff.
