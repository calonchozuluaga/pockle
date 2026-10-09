# Pockle visual system (v2, UX-032)

Version **v2 screens 01** (shown in **Settings → About**). Owner: Claude, per `AGENT_COORDINATION.md`. The owner approved the direction in the design canvas "Pockle screens v2" on October 9, 2026, replacing Visual system 01 after on-device feedback that the first pass felt clunky and dated. Tokens live in `Assets/Pockle/Runtime/UI/PockleTheme.cs`; screens take values from there.

## Personality

Calm, modern, and toy-first: a milk-white app with plum ink, where colour comes from the toys themselves. Each toy sits on its own colour field. The UI is flat, with no drop shadows or button lips, so the jelly toys provide the depth.

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
- Tab bar: a floating plum pill, 64 px tall, 20 px above the bottom safe area. The selected tab is a jelly-pink pill with plum ink.
- Gutter 20 px. Touch targets at least 44 px.

## Motion

`SquishFeedback` is on every button and tile: press squishes to 95% and springs back; Motion calm dips to 98% with no bounce.

## Screens

Home, Shelf, Boxes, You, Settings, Friends, and the toy header follow the canvas. The Collection ("Who's inside") and Collections pages need new pages in `MenuNavigation` (requested from Codex), and the Box reveal moment is UX-012.
