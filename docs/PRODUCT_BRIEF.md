# Pockle — App Development Brief

## Concept

Build **Pockle**, a premium mobile game for discovering, collecting, and interacting with original digital toys called **Pockles**.

Pockles come in different materials: translucent jelly, soft plush, matte vinyl, glitter-filled gel, and pearlescent finishes. Each should have a distinct personality and satisfying interaction.

The experience combines the anticipation of unboxing, the pleasure of handling a beautiful toy, and the pride of building a collection. It should appeal to teens and adults who enjoy designer toys and cozy games.

**Previous working name:** Jelliroam. The project is now Pockle because the concept has expanded beyond jelly creatures and walking.

## Product Direction

The app should be enjoyable at home, on a couch, or on a tablet. **Walking is optional**, potentially added later as another way to earn rewards.

The core experience must feel polished: convincing materials, responsive touch interactions, expressive animations, good sound design, and optional haptics. Avoid intrusive ads.

All characters, names, and designs must be original. Physical collectible toys are inspiration for the emotional experience, not designs to copy.

## Core Loop

1. Earn a discovery box through clear gameplay progression.
2. Open it through a satisfying reveal animation.
3. Discover a Pockle, inspect it, and optionally name it.
4. Interact with it and add it to a personal display shelf.
5. Work toward another discovery, complete collections, and eventually trade duplicates.

Provide a first companion immediately and a clear path toward the next discovery. Exact reward thresholds remain open for testing.

## Toy Families

Start with three families sharing a cohesive visual style:

- **Jelly:** press, stretch, squash, and release; soft rebound, translucent depth, bubbles or suspended glitter.
- **Plush:** compress, pet, and gently wobble; soft fabric appearance and expressive reactions.
- **Vinyl:** rotate, nudge, and watch personality animations; solid form with matte or glossy finishes.

Materials should change how a toy responds, not just its color. Keep shapes simple enough for mobile rendering while making silhouettes recognizable.

Existing concept art explores Peach Jelly, Cloud Mochi, Lagoon Glass, Stardust, Pearl Swirl, and Liquid Gold. Treat these as visual references, not a finalized roster.

## Collection, Rarity, and Evolution

Separate three concepts:

- **Identity:** the character and material variant.
- **Rarity:** its discovery tier and, where applicable, fixed edition size.
- **Progression:** the relationship or evolution earned through play.

A common Pockle should remain desirable. Leveling it must not automatically turn it into a scarce edition.

Explore four evolution stages per character family later. Evolution should preserve the collectible’s identity and edition number. Start by proving one evolution path before producing a full roster of stages.

For limited editions, support a registry showing maximum edition size, issued quantity, and remaining unissued quantity. Each issued collectible receives a persistent serial number. Do not publish owner identities in the registry.

Scarcity must be enforced by the backend. No blockchain is required.

## Trading — Later Phase

Support item-for-item trades, including offers containing multiple collectibles.

The intended model is:

- Anonymous public listings and structured offers.
- Free-text messages only between mutually accepted friends.
- Persistent serial numbers and edition metadata when ownership changes.
- Atomic exchanges that prevent duplicated items or partial transfers.
- Reporting, blocking, and moderation controls.

Do not include cash sales, cash-out, or promises of resale value in the first release.

## Monetization Direction

Keep the core collection and interaction experience free.

Potential revenue sources:

- Known, specific character purchases.
- Display shelves, rooms, backgrounds, and decorations.
- Membership with clearly defined benefits.
- Later artist collaborations or sponsored collections.

For the initial version, earn randomized discovery boxes through gameplay. Avoid selling random boxes or paid boosts to random rewards until platform and legal requirements have been reviewed.

Do not tie rewards to endless tapping or require excessive screen time.

## Technology

**Preferred engine: Unity**, targeting iOS and Android, with phone and tablet layouts.

Build the tactile interactions locally on the device. Do not call an AI API every time a user touches a toy.

Use reusable base meshes, shared animation systems, and material variants to control production costs. Blender and procedural tools may help create assets; generated models will still need visual and performance checks.

Load full models when needed and use lightweight previews for collection browsing. Prioritize stable performance on midrange phones. Support reduced motion, muted sound, and disabled haptics.

The owner currently has a likely 2017 MacBook Pro; exact specifications are unconfirmed. Verify local Unity compatibility before choosing the editor version. Keep the project in Git so Codex can edit source while the owner runs Unity locally.

Biotadex, an existing React Native app, provides inspiration for collection and trading mechanics. Do not assume access to its code or that its implementation can transfer directly to Unity.

## First Playable Beta

Suggested initial scope: **12 collectibles across three base shapes**, with varied materials and reactions. This is a starting target, not a fixed requirement.

Include:

- First companion onboarding.
- Interactive toy viewer.
- One polished box-opening sequence.
- Earned discoveries and visible progression.
- Collection shelf and character details.
- Local save data.
- Sound, haptic, and motion settings.

Defer maps, AR, walking integration, trading, real purchases, and a full evolution system.

## Development Sequence

**First milestone: tactile prototype**

Create one original jelly Pockle that can be pressed, stretched, and released. Include rotation, reset, and a simple reveal. Test responsiveness and performance on a real phone.

This is an internal technical test, not the complete collecting beta.

**Second milestone: material variety**

Add one plush and one vinyl Pockle with distinct interactions. Establish the reusable asset and animation pipeline.

**Third milestone: collecting beta**

Expand the roster, implement discovery progression, and build the shelf. Test whether people enjoy repeated interactions and return to collect more.

**Fourth milestone: online systems**

Add accounts, authoritative inventories, edition issuance, and trading after the core experience is validated.

## Instructions for Codex

Start by inspecting the project and available development tools. Explain which parts can be built and verified in your environment and which require the local Unity editor or a physical device.

Then implement the first tactile prototype with organized source files and clear setup instructions. Use procedural placeholder geometry if necessary, but distinguish placeholders from final art.

Do not silently replace the Unity app with a website. Keep the first milestone focused on proving that a Pockle feels satisfying to touch.
