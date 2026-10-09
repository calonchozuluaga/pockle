# Pockle — App Development Brief

## Concept

Build **Pockle**, a premium mobile game for discovering, collecting, and interacting with original digital toys called **Pockles**.

Pockles come in different materials: translucent jelly, soft plush, matte vinyl, glitter-filled gel, and pearlescent finishes. Each should have a distinct personality and satisfying interaction.

The experience combines the anticipation of unboxing, the pleasure of handling a beautiful toy, and the pride of building a collection. It should appeal to teens and adults who enjoy designer toys and cozy games.

**Previous working name:** Jelliroam. The project is now Pockle because the concept has expanded beyond jelly creatures and walking.

## Product Direction

The current direction, clarified by the owner on October 8, 2026, is a simple collecting app: receive daily collection-themed boxes, unlock them by walking, or buy a box outright. Walking is the free earning path. The toy viewer remains enjoyable at home, on a couch, or on a tablet.

The core experience must feel polished: convincing materials, responsive touch interactions, expressive animations, good sound design, and optional haptics. Avoid intrusive ads.

All characters, names, and designs must be original. Physical collectible toys are inspiration for the emotional experience, not designs to copy.

## Core Loop

1. Receive daily boxes themed around recognizable original collections.
2. Unlock earned boxes by walking, or purchase a box directly.
3. Open the box, discover its color/variant, and add it to the collection.
4. Squish, stretch, inspect, and enjoy the toy.

Use a custom soft, rounded, squishy Pockle wordmark and surprise-toy packaging with collection motifs and mystery silhouettes. The next reveal pass should use the redesigned Jelly Garden box: the lid opens, Pip emerges and settles onto the plate, then touch becomes available. Preserve replay and reduced-motion support.

The box design identifies its collection; the specific color/model is a surprise. Proposed price points are $0.99 per standard box and $2.99 for special collections, subject to store pricing tiers. The first local beta uses one Jelly Garden box per UTC day, unlocked by 1,000 steps. Provisional pools and release requirements are recorded in `COLLECTION_UI.md`.

Use step counting alongside movement/location checks to distinguish walking from phone shaking. GPS alone does not prevent cheating or distinguish walking from vehicle travel; continuous tracking is not a requirement. Validate rewards and store purchases authoritatively before granting inventory.

Provide a first companion immediately. The current Unity project now has a shelf, local saved ownership, and a hardware-step-based daily reward. Payments and authoritative online validation remain unconnected.

## Toy Families

Start with three families sharing a cohesive visual style:

- **Jelly:** press, stretch, squash, and release; soft rebound, translucent depth, bubbles or suspended glitter.
- **Plush:** compress, pet, and gently wobble; soft fabric appearance and expressive reactions.
- **Vinyl:** rotate, nudge, and watch personality animations; solid form with matte or glossy finishes.

Materials should change how a toy responds, not just its color. Keep shapes simple enough for mobile rendering while making silhouettes recognizable.

Existing concept art explores Peach Jelly, Cloud Mochi, Lagoon Glass, Stardust, Pearl Swirl, and Liquid Gold. Treat these as visual references, not a finalized roster.

The owner expanded the target to **at least 12 distinct characters with 8–10 material/texture varieties each**, or 96–120 collectibles. The working content proposal is 12 × 9 = 108, including jelly, flocked/fuzzy, plush/foam, and firm vinyl/coated finishes with different handling properties. See [the character roster and material plan](CHARACTER_ROSTER.md) for twelve proposed silhouettes, nine finish families, the Blender-to-Unity pipeline, and production batches. Names and designs remain proposals; the current app still has one Pip model in four finishes.

## Viewer Interaction and Materials

Touch the toy directly to squish/stretch it. Drag its plate sideways to rotate the plate and toy together, without mode buttons. Capture the starting surface until release. The next gesture experiment is a two-finger pull on the toy that stretches along the fingers’ separation axis, with pinching inward to compress; it must not zoom the camera or take over a plate gesture. This is implemented in the current viewer, with grip handoffs preserving position.

Jelly should look wet, clear, and glossy around the whole shell, as close as practical to the approved reference. Favor mobile-friendly studio reflections and simulated filling before expensive scene refraction.

The viewer lets a one-finger upward drag lift the toy off its plate while two fingers stretch it. Gel sags around the grip, falls under gravity when released, and squashes/jiggles on landing, with a height-sensitive contact shadow. Bounded accelerometer motion shakes the jelly. The owner requested more dramatic sag, shakes, and landings; full-motion tuning is now more expressive while calm mode keeps direct control. Gyroscope tilt remains future work. Device feedback remains part of tuning.

Add a lights-off environment for glowing editions. Internal gold stars, pearls, and other inclusions should mostly react to squeezing/stretching, lag slightly, and drift briefly after release before settling.

## Expanded UI/UX planning direction

On October 8, 2026, the owner requested a tracked plan for a finished mobile game: home menus, full Settings, a user/profile area, friends, visits to other users' shelves, rewards, badges, and milestones. This adds social and achievement planning to the earlier simpler release direction. See [the UI/UX task tracker](UI_UX_TASKS.md) for priorities, acceptance criteria, dependencies, and the confirmed home/social direction. The first Home/Settings/local-profile UI pass is now implemented; live accounts, social services, cloud save, and achievements remain pending. See [Home hub 01](HOME_UI.md) for what can be reviewed.

Keep the collecting economy centered on walking-earned boxes and optional purchases. Badge and milestone rules/rewards need definition. The owner chose a separate home hub with Collection, Rewards, and Friends tiles, plus public profiles and shelf discovery. Public shelf visits do not require friendship; friend connections remain a separate feature. The first navigation pass uses Home/Boxes/You tabs, with a local profile and explicit Discover/Friends empty states. Visual refinement and online services remain implementation work. Guest access and a playable first toy remain part of the experience.

## Initial Release Scope

Keep the collecting loop to walking-unlocked daily boxes and optional paid boxes. Packaging identifies the collection; color and variant are revealed on opening. Trading, evolution, memberships, and edition registries remain outside the initial release. Badges and milestones are now requested planning scope; define their rules before implementing rewards.

Keep tactile interaction available without a purchase, and do not require endless tapping or excessive screen time. Store billing, box contents/probabilities, reward validation, and applicable store requirements must be specified before release implementation.

## Technology

**Preferred engine: Unity**, targeting iOS and Android, with phone and tablet layouts.

Build the tactile interactions locally on the device. Do not call an AI API every time a user touches a toy.

Use reusable base meshes, shared animation systems, and material variants to control production costs. Blender and procedural tools may help create assets; generated models will still need visual and performance checks.

Load full models when needed and use lightweight previews for collection browsing. Prioritize stable performance on midrange phones. Support reduced motion, muted sound, and disabled haptics.

The owner currently has a likely 2017 MacBook Pro; exact specifications are unconfirmed. Verify local Unity compatibility before choosing the editor version. Keep the project in Git so Codex can edit source while the owner runs Unity locally.

Biotadex, an existing React Native app, provides inspiration for collection and trading mechanics. Do not assume access to its code or that its implementation can transfer directly to Unity.

## First Playable Beta

The expanded roster target is **12 distinct characters with 8–10 finishes each**. Build a representative batch across jelly, fuzzy/plush, foam, and vinyl first so materials and handling can be validated before producing the full 96–120 collectibles. This replaces the earlier provisional suggestion of 12 collectibles across three base shapes.

Include:

- First companion onboarding.
- Interactive toy viewer.
- One polished box-opening sequence.
- Daily walking-unlocked boxes and clearly priced optional box purchases.
- Collection shelf and character details.
- Local save data.
- Sound, haptic, and motion settings.

Defer maps, AR, trading, edition registries, and evolution. The owner has approved beginning the collection UI, walking, and purchase milestone after the tactile/material prototype.

## Development Sequence

**First milestone: tactile prototype**

Create one original jelly Pockle that can be pressed, stretched, and released. Include rotation, reset, and a simple reveal. Test responsiveness and performance on a real phone.

This is an internal technical test, not the complete collecting beta.

**Second milestone: material variety**

Add one plush and one vinyl Pockle with distinct interactions. Establish the reusable asset and animation pipeline.

**Third milestone: collecting beta**

Expand the roster, implement daily walking-unlocked boxes, store purchases, and the collection shelf. Test whether people enjoy repeated interactions and return to collect more.

**Online systems supporting the collecting milestone**

Add the minimum online systems needed for purchase validation and authoritative inventories. Trading and edition issuance are outside the current release scope.

## Instructions for Codex

Start by inspecting the project and available development tools. Explain which parts can be built and verified in your environment and which require the local Unity editor or a physical device.

Continue the current collecting/UI milestone with organized source files and clear test instructions, using `UI_UX_TASKS.md` to track finished-game work. Distinguish shipped beta behavior, provisional designs, and online integrations still required.

Do not silently replace the Unity app with a website. Keep tactile play satisfying as the mobile menus, collection, rewards, and social plans expand.
