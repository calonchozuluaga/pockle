# Firb tap reactions

Firb has five local two-second reactions across all ten finishes: BranchWave (one side of the lower branches greets you), StarNod (the crown star wobbles), OrnamentShimmy (alternating tiers shimmy), HappyHop, and SleepySway. Each coordinates a smile, surprised mouth, or sleepy eyes. Painted ornaments stay attached to their branch tier; they are not separate rigid objects.

A single tap chooses uniformly among all five initially, then among the four other actions. An active reaction finishes before another tap can start one. Gesture start, reset, collectible changes, disable and focus loss cancel the pose; existing rotation, material-aware pinch, lift and double-tap reset remain intact. Calm uses a small smile without body, branch, star or blink movement. Rigid finishes keep their body scale.

`FirbReactions` is engine-free; `FirbReactionDriver` coordinates its offset and facial controls. Firb-only spatial masks in HolidayFinish animate the star and branch tiers without changing source geometry, textures or finish recipes. Other holiday characters retain their existing motion. The public TryPlay API can later pair each reaction with a sound; audio is not added here. No new inventory grants or save format changes.

Native Unity capture: `docs/captures/firb-reactions/Firb-five-reactions.gif`. Device acceptance remains separate from the rendered preview. See VALIDATION.md for executed checks.
