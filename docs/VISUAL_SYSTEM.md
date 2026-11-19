# Pockle visual system (v2, UX-032)

Version **v2 screens 04** (shown in **Settings → About**). Owner: Claude, per `AGENT_COORDINATION.md`. The owner approved the direction in the design canvas "Pockle screens v2" on October 9, 2026, replacing Visual system 01 after on-device feedback that the first pass felt clunky and dated. Tokens live in `Assets/Pockle/Runtime/UI/PockleTheme.cs`; screens take values from there.

## Personality

Calm, modern, and toy-first: a milk-white app with plum ink, where colour comes from the toys themselves. Each toy sits on its own colour field. The UI is flat, with no drop shadows or button lips, so the jelly toys provide the depth.

## Jelly glass (UX-053, owner's pick November 18, 2026)

The owner chose "A2 · Jelly glass" from the look boards (`docs/ui-audit/02-looks/`): the jelly look in calmer colours, with clear, light-catching glass. It replaces "flat, no drop shadows" above. The kit lives in three places:

- `Resources/UI/PockleGlass.shader` draws every rounded surface in one pass: shape, a bright rim that is strongest along the top, a soft gloss, a faint diagonal sheen and a soft shadow. `Runtime/UI/GlassSurface.cs` gives it one quad per surface with the size, corner radius and style. `PrototypeHud.Surface` adds it to everything, so no screen builds its own.
- `Runtime/UI/PockleGlass.cs` tones the page painting (less colour, a milk lift) and makes a small blurred copy at run time. Frosted surfaces show that blurred copy at their place on screen. The painting is one the app already ships (`UI/Onboarding/OnboardingTutorial2`), so the look adds no pictures to the download.
- `PrototypeHud.Glass.cs` puts the painting behind browsing pages and tells frost when it is really there. On the toy's stage, onboarding, a box opening and the colour-field pages, frost turns nearly solid instead.

| Style | Token | Use |
| --- | --- | --- |
| Clear glass | `Glass` (white, 30%) | Rows, chips, inputs, secondary buttons |
| Frost | `Frost` (warm white, 60%) | Cards, sheets, dialogs, the tab capsule: anything behind words |
| Lens | `Lens` (white, 62%) | The selected tab, chip or segment |
| Jelly | `Rose` `#8E3B66` | The main action. It is the only strong colour in the chrome |
| Flat | | Apple and Google sign-in buttons, which keep their own plain styles |

The tab bar is a frosted capsule floating over the page with a lens on the selected tab; pages scroll behind it. Jelly pictures (`Resources/UI/Icons/Jelly/`, 128 px) sit above the words on the play controls and beside them on Home's four sections; words stay with icons. The older tokens below remain for colour fields and ink; `Fill` and `Milk` passed to `Surface` become `Glass` and `Frost`, and `Plum` passed to `CreateButton` becomes the rose jelly button.

## Colour

| Token | Hex | Use |
| --- | --- | --- |
| `Milk` | `#FBF8F5` | Page ground, dialogs, selected segments |
| `Plum` | `#2A1430` | Text, primary buttons, the tab bar, progress fills |
| `PlumSoft` | `#6E5A72` | Secondary text and unselected segments |
| `OnPlum` / `OnPlumMuted` | `#FBF8F5` / `#E8DCE6` | Text and icons on plum |
| `Fill` / `FillDeep` | `#F1ECEF` / `#E2D8DE` | Rows, inputs, header buttons, segmented controls; dividers and switch-off |
| `Mystery` | `#EFE9EC` | Tiles for toys you haven't found |
| `Jelly` / `JellyInk` | `#FFC6D6` / `#7A2A4E` | Brand pink: today's box, the selected tab; text on pink |
| `FieldPeach` / `FieldMoon` / `FieldGold` / `FieldMint` | `#FFD8C4` / `#D8DDFF` / `#FFE3A3` / `#CDEFDB` | Each toy's colour field (`FieldFor(id)`) |
| `Midnight`, `Confetti` | `#2E2A5C`, `#F1E6FF` | Box/pool cards (`CollectionField(poolId, ...)`) |
| `Heart` | `#C2416E` | Favorite |

Toy portraits render once, on their own field colour (`ToyPortrait.Render(id, background)`), so they sit seamlessly on tiles. The 3D stage behind toy play belongs to the runtime lane; a per-toy backdrop (`PrototypeStage.SetBackdrop`) is requested in the handoff note.

## Type

Bricolage Grotesque (SIL Open Font License), shipped as three static instances in `Resources/UI/Fonts/`: **Display** (bold, large titles and numbers), **Label** (semibold, buttons, names, headings), **Body** (regular, descriptions). Bold labels 24 px and up use Display; other bold labels use Label; everything else uses Body.

| Role | Size (ref. px at 390×844) |
| --- | --- |
| Screen title | 32 |
| Section heading | 21 |
| Button | 17 |
| Body | 16 |
| Caption | 14 |
| Small (badges, tile names) | 13 |

Labels best-fit down by up to 5 px, never below 11. The font has no CJK or emoji, so Unity falls back to system fonts; a name like `Lucía 李` needs a device check.

## Shape and spacing

- Rounded corners from one 128 px 9-slice sprite; `SetRadius` sets the drawn radius per surface. Cards 28, tiles 22, rows 20, buttons are pills.
- Header: 44 px circular buttons (back, settings, favorite, avatar) on `Fill`, or translucent milk over the toy stage.
- Tab bar: a floating plum pill, 68 px tall, 20 px above the bottom safe area, with four tabs: Home, Shelf, Boxes, You (owner request, October 10: the shelf stays one tap away everywhere). Each tab is an icon over its label. The selected tab is a jelly-pink pill with plum ink.
- Gutter 20 px. Touch targets at least 44 px.

## Motion

`SquishFeedback` is on every button and tile: press squishes to 95% and springs back; Motion calm dips to 98% with no bounce.

## Screens

v2 screens 02 adds the living shelf (Shelf tab), the toy personality page, and the fusion screens, from the canvas row "Personalities and fusion". The personality and reveal pages are full-bleed: a colour field (the toy's field, or midnight for a reveal) with a milk sheet, and no tab bar. Sparkles and the crescent moon are drawn at runtime (`UiShapes`).


Home, Shelf, Boxes, You, Settings, Friends, and the toy header follow the canvas. The Collection ("Who's inside") and Collections pages need new pages in `MenuNavigation` (requested from Codex), and the Box reveal moment is UX-012.

v2 screens 04 adds the toy stage and finishes Collections, from the canvas rows "Toy stage" and "Collection: who's inside":
- **Play:** a collection pill in the header (opens that collection), an eyebrow ("FOX · NO. 10"), the finish name as the title, and a separate rarity badge ("Secret · Super rare"). The ways-to-play hint sits under the title.
- **Stage tray:** a milk strip at the bottom with Collection, World and Light. Closed, it stays clear of the plate; a tab opens its panel above the strip. World and light are remembered (`StageLook.WorldKey`, `LightKey`). New players start in Studio with Daylight, which matches the old stage exactly.
- **Until the stage draws worlds itself,** the HUD tints the backdrop (`StageLook.Backdrop`); dark worlds switch the title to light ink.
- **All collections:** neutral rows with the character on its colour field (shelf portraits for Pips, one small render for other characters), the series line, and "N of M found" / "Complete".
- **One collection:** a hero on the collection's colour with the title, caption, the character and a progress bar; "Who's inside"; the lineup; and a plum secret card when the collection has a secret.
