# Expanded Pockle roster and material plan

## Confirmed target

The owner wants **at least 12 distinct characters, each with 8–10 material/texture varieties**. This is **96–120 collectibles**, not 12 recolors of Pip. The working production target is **12 characters × 9 finishes = 108 collectibles**, with an optional tenth glow edition per character later.

The count is confirmed direction. The character names, silhouettes, finish set, production batches, and technical budgets below are proposals for review with the owner and Claude. They are not models or collectible drops already available in the app. The current app still contains one character, Pip, in four finishes.

Fuzzy designer toys, vinyl art toys, and other physical collectibles inform the material feel. Pockle characters should have their own silhouettes, faces, proportions, and stories. A fuzzy Pockle need not share Pip's silhouette, and a vinyl Pockle should not inherit jelly stretching automatically.

## Twelve original character concepts

Working names are design labels, not finalized commercial names. All designs share a friendly face language, rounded forms, and an inviting tabletop scale; each must also be recognizable as a flat silhouette without color or filling.

| Character | Distinct silhouette and personality | Lead material for its first art pass | Shape constraints across finishes |
| --- | --- | --- | --- |
| **Pip** | Established teardrop body with two integrated crown bumps; curious and sunny | Peach jelly | Keep the crown asymmetry and familiar face spacing |
| **Dew** | A broad cloud-shaped body with one curled droplet antenna; a sleepy little rain spirit | Clear aqua gel | Keep three broad cloud lobes and a short, thick antenna |
| **Tula** | A low, domed turtle pebble with four stubby feet and a tiny head; calm and patient | Opaque mochi foam | Keep the low dome and feet distinct from Pip's standing pose |
| **Ripple** | A rounded manta-like cushion with wide fins and a short tail; playful and floaty | Opalescent jelly | Keep the horizontal silhouette and rounded fin tips |
| **Moss** | A squat garden sprite framed by a broad leaf hood; a shy little gardener | Short green flock | Keep the hood outline and a visible face recess |
| **Nook** | A rounded square pillow creature with soft corner paws; cozy and unhurried | Bouclé plush | Keep the square body and soft seam/paw placement |
| **Wisp** | A round comet head tapering into a thick curling tail; curious and energetic | Velvet flock | Keep the tail curl chunky enough to read on the shelf |
| **Loop** | A small bow creature with two thick loops and a rounded central body; cheerful and springy | Fine plush | Keep the loop openings readable and the connectors substantial |
| **Bop** | A wide spinning-top body with a little cap and chunky feet; a confident show-off | Gloss vinyl | Keep the top-like body profile and the offset cap |
| **Rolo** | A friendly barrel robot with short side knobs and a belly panel; helpful and earnest | Matte vinyl | Keep the barrel proportions and face/panel layout |
| **Mallow** | A marshmallow-shaped moon creature with an off-center crescent crest; dreamy and quiet | Satin vinyl | Keep the flat rounded base and crescent crest silhouette |
| **Sprig** | A rounded seed capsule with a three-leaf sprout and two tiny feet; eager and optimistic | Coated metallic vinyl | Keep three thick leaf shapes, avoiding fragile thin tips |

These lead materials help the first twelve designs look varied together. Each character still receives its own 8–10 finishes; the lead-material groups do not restrict a character to only jelly, plush, or vinyl.

## Nine finish families

A finish is more than a base color. It combines a surface, interior where applicable, deformation/weight behavior, sound, and optional haptics. Color palettes then make those finishes specific to each character. Reuse shader and interaction profiles, while making authored textures and details suit the model.

| Finish | Visual construction | Intended handling |
| --- | --- | --- |
| **Clear jelly** | Wet highlights, translucent tinted shell, colored filling and a few bubbles | Most stretch and grip sag; lively bounded rebound; suspended filling reacts |
| **Pearl jelly** | Milky translucency and a soft changing sheen | Slightly firmer gel, rounded wobble, slower settling than hard toys |
| **Confetti gel** | Clear shell with larger visible stars/flecks; glitter can use a shader approximation | Gel deformation, heavier-looking sag and filling lag; no full fluid simulation |
| **Mochi foam** | Opaque pastel skin with broad soft highlights and subtle surface grain | Strong compression, limited stretch, slow recovery and a soft landing |
| **Velvet flock** | Short fine fibers and a soft edge sheen | Gentle compression and damped sway; little stretch; soft landing |
| **Bouclé plush** | Loop-like fabric texture, selected tufts and understated seams | Broader stuffed compression, low-frequency wobble and soft settling |
| **Matte vinyl** | Opaque satin/matte molded surface | Firm body; whole-toy tilt, rotation, lift, contact wobble and personality reactions |
| **Gloss vinyl** | Smooth opaque shell with crisp studio highlights | Firm body and bounded landing wobble; no jelly-like elongation |
| **Coated metallic** | Metallic-looking reflective coating over a molded body | Firm coated-toy feel; distinct contact sound; not a heavy solid-metal physics simulation |

**Optional finish 10: Glow jelly.** A lights-off edition with emission and a restrained halo. Glowing appearance does not automatically illuminate the scene; any nearby light effect needs a deliberate mobile implementation. Keep this a later material/environment experiment rather than claiming today's pearlescent Moon finish already glows.

For eight-finish characters, omit a finish that fails the silhouette/material review rather than padding the roster with an indistinguishable recolor. Additional pearlescent paint, clay-like, frosted resin, and rubber finishes can be explored after the first representative materials work well.

## Art and Android pipeline

- **Blender authors meshes, UVs, face anchors, seams, and texture maps.** Its procedural nodes, fur systems, and offline renders do not automatically become equivalent Unity materials. Bake applicable color/normal/roughness/mask maps, then reproduce the finish with the project's Built-in Unity shaders.
- **Fuzz:** start with short flock shading, fabric normal maps, and a few silhouette tufts. Avoid building the initial mobile version around strand simulation or many transparent fur layers. Evaluate the softer silhouette from multiple angles on the phone.
- **Gel:** reuse and improve the current studio-reflection shell, while allowing character-specific interior accents. Reactive filling and environment refraction remain separate pending features.
- **Vinyl:** define a firm handling profile. A shared lift/gravity controller can serve all families, but jelly sag/stretch must be limited or disabled by material. Each shape needs reviewed grip anchors, contact clearance, and face deformation rules.
- **Authoring targets to measure:** start around 3,000–6,000 triangles per active body, shared shader programs, roughly 512px material maps, and small face/filling meshes. These are proposed starting targets, not established Android performance limits. Pip currently uses 5,814 body triangles. Raise or lower budgets after profiling.
- **Shelf:** use cached or pre-rendered thumbnails and create full viewer assets on selection. Do not eagerly render 108 live toys or allocate 108 permanent 256px RenderTextures at startup. A single 256px RGBA32 color buffer is about 256 KiB; 108 already cost roughly 27 MiB before depth buffers and other assets.
- **Reduced motion:** provide calm reactions for every material, while keeping direct manipulation and readable character identity.

## Catalog and saved ownership

The current `PipVariant` enum, four-element inventory, and HUD arrays are prototype assumptions. Do not append more than a hundred enum branches or use list ordering as saved identity.

Use separate, stable definitions for:

1. **Character:** ID, display name, silhouette/asset, face/grip anchors, scale, compatible finishes.
2. **Finish:** ID, shader/material recipe, palette/masks, interior recipe, interaction profile, feedback.
3. **Collectible:** stable character + finish identity, preview reference, availability, collection membership.
4. **Box offer:** explicit collectible pool and weights. An expanded catalog does not automatically change current Jelly Garden odds or make every planned toy obtainable.
5. **Ownership:** counts keyed by stable collectible ID, separate from definitions and reward eligibility.

Examples: `pip.peach-jelly`, `pip.moon-pearl`, `pip.gold-confetti`, `pip.mint-mochi`, `moss.velvet-flock`. Final keys must be agreed before shipping a save migration. Preserve the meanings and counts of all four existing Pip entries, plus pending reveals and local profile favorites/avatars. Map old indexes explicitly; do not reset inventories or give all 108 toys as starter gifts. Unknown future IDs need safe round-tripping when older clients read newer saves.

The shelf and profile UI should consume catalog definitions rather than fixed arrays. Coordinate that interface with Claude before changing both the data and HUD sides.

## Production batches and task IDs

- [ ] **ART-001 — Character identity sheet.** Review all 12 silhouettes, front/side/back views, face placement, and relative scale. Agree working names and shape identities before making full finish sets.
- [ ] **ART-002 — Representative material batch.** Use Pip for gel, Moss for flock, Nook for plush/foam, and Bop for vinyl. Build the four representative models and a small set of finishes; compare appearance and touch on Android before multiplying recipes.
  - Draft models on `codex/material-toys`: Moss / Velvet Flock, Bop / Gloss Vinyl, and Nook / Oat Bouclé Plush + Lilac Mochi Foam, alongside Pip. See [study assets and review instructions](CHARACTER_STUDIES.md). Concept approval and Unity/Android review remain pending.
- [ ] **ART-003 — Nine finish recipes.** Establish repeatable Unity materials, texture baking, interaction profiles, calm motion, feedback, and mobile budgets. Approve flock/plush and firm vinyl behavior alongside the established jelly baseline.
- [ ] **ART-004 — Full character production.** Expand to the twelve reviewed models, then make each one's 8–10 distinct finishes. Reuse tooling and profiles while preserving character identity.
- [ ] **ART-005 — Collection presentation.** Finish previews, names, collection packaging, box pools, and visual consistency. A planned catalog entry becomes an available collectible only after its assets, behavior, and acquisition rules are ready.

Catalog migration and scalable UI/runtime loading are tracked separately as **UX-029–031** in `UI_UX_TASKS.md`. These art tasks are new scope, not completed content.

## Proposed collaboration

Codex can own catalog/save migration, material interaction profiles, Unity runtime integration, and validation. Claude can refine the character/material concepts and collection presentation alongside the proposed UI lane. This extends the earlier proposed split; Claude's model/art scope and exact file ownership still need acknowledgment in their handoff. Avoid both editing the same HUD or generated character assets concurrently.
