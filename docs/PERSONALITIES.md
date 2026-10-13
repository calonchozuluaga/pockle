# Personalities and a living shelf (proposal)

October 9, 2026. **Approved by the owner on October 12** from the design canvas ("Living shelf" and "Toy personality" screens); built in UX-037. Originally a proposal. Owner direction from today's discussion: give every Pockle a personality that ships with the app, plus a few ways toys react to each other on the shelf. Design by Claude; the runtime is Codex's lane (see "Who builds what").

## The problem it solves

After you open today's box, there's nothing more to do: the toys are a reward with no life afterward, and "come back tomorrow" is a weak reason to return. A shelf that feels a little alive gives people a reason to open the app just to see what their toys are up to. Trading, crafting, and set completion (`COLLECTIONS.md`) are the other long-term hooks.

## Decisions

1. **Personalities are authored and shipped in the app, not generated live by AI.** Every line, sound, animation, and reaction is pre-made and chosen by simple rules on the device.
   - **Why:**
     - Live AI costs grow with every player and every tap, and a child chatting endlessly with a toy could run up a large bill without realizing it.
     - It would need a server, wouldn't work offline, would add a pause before each reaction, and would make us responsible for moderating free-form text shown to children.
   - AI may help **during development** to draft voices and reaction ideas, which people then review and edit. Nothing is generated at runtime. This matches the project rule of no per-touch network or AI calls.
2. **Personality lives on the character, and the variety tints it.** All Pips share Pip's nature (curious and sunny); a finish nudges it. This keeps the writing to twelve characters plus small per-variety adjustments, not 120 separate personalities.
3. **Alive, but never needy.** No decay, hunger, feeding, sadness from neglect, streak penalties, or anything that punishes being away. Toys are always happy to see you. Pockle is a cozy collecting game, and guilt is the wrong feeling for it.

## How a personality is built

Each character gets one small, authored profile:

| Part | What it is | Example (Pip) |
| --- | --- | --- |
| Temperament | One line from `CHARACTER_ROSTER.md` | Curious and sunny |
| Idle style | Codex's `CharacterMotion` idle and eager recipes, already built | Jelly breath and a curious bounce |
| Reactions | A handful of short responses to a poke, a squish, being picked up, or a long wait | Leans toward your finger; a delighted bounce when squished |
| Voice | A few short sounds (no speech) and optional one-to-three-word lines | A soft "hm?" chirp |
| Variety tint | A small adjustment per finish | See below |

**Variety tints for Pip (draft):**
- **Peach Jelly:** the baseline.
- **Moon Jelly:** dreamier and a little slower.
- **Gold Glitter:** more of a show-off; poses when looked at.
- **Mint Soft:** calmer; settles quickly.

A tint changes timing, intensity, and which reactions come up most often. It never changes who the character is.

The other characters' temperaments already exist in the roster: Dew sleepy, Tula patient, Ripple playful, Moss shy, Nook cozy, Wisp energetic, Loop cheerful, Bop a show-off, Rolo earnest, Mallow dreamy, Sprig eager. Each needs its own profile before release; names are still working names.

## The living shelf

- **Ambient life.** Toys on screen idle in character using the existing motion recipes. They react gently to the time of day (sleepy at night, brighter in the morning). Nothing needs your input, and there's nothing to miss.
- **Pairing reactions.** When certain toys sit next to each other, they do a short pre-made moment together.
  - Two of the same character recognize each other.
  - A shy toy (Moss) peeks out from behind a bold one (Bop).
  - Two sleepy toys (Dew and Mallow) nod off together.
  - Rules choose which moment plays and how often. They never repeat so often that they become wallpaper.
- **Poke to respond.** Tapping a toy on the shelf gives an in-character reaction. Picking it up still opens full tactile play.
- **A little surprise.** Rare reactions (for example, after the tenth poke, or the first time two particular toys meet) give regular players something to discover. Discovering one never costs or grants toys.

**Motion calm and performance:** Motion calm / reduced motion turns the shelf's ambient and pairing motion off; pokes still give a gentle response. Only the toys on screen animate, within Codex's `LineupMotionBudget`, so a full shelf of 120 toys stays light on battery.

## Consistency with the collection rules

Personalities are presentation only:
- they don't affect odds, edition sizes, crafting, trading, or what any box contains;
- no reaction is ever sold;
- a toy's personality comes with every copy, including traded and crafted ones.

## Answered by the approved screens (October 12)

1. **First pass:** the four Pip finishes form six pairs, each with a day and a night moment, for twelve moments to find. More arrive with each character.
2. **How personality shows up:** animation and a short voice sound, plus short labels: trait chips, a reactions list, and moment captions. There is no dialogue.
3. **Arranging the shelf:** not yet. The shelf places owned toys in a fixed order (show-offs and dreamers on the top shelf). Player arranging is a later option.
4. **"Meet your toy":** still open.

The approved screens also add a **Day / Night** switch on the shelf. It starts from the phone's clock (night is 9 pm to 6 am). At night everyone dozes except the dreamers, and the night moments differ from the day ones.

## Who builds what

As built (UX-037): the content lives in `Runtime/UI/PersonalityBook.cs`, which is engine-free so it can move to Core. The shelf toys are Codex's live meshes (`ShelfPreviewRuntime`) on the scene's colour, with `PersonalityMotion` adding each personality's sway. Moments found are a UI preference (`pockle.shelf.moments.v1`), never inventory.


| Part | Owner |
| --- | --- |
| Personality profiles, reaction list, variety tints, pairing list, the shelf's look | **Claude** (design and UI) |
| Behavior runtime: reaction selection, pairing detection, per-character hooks, motion and sound playback, the motion budget | **Codex** (runtime, `CharacterMotion`, `JellyToy`) |
| Animation and sound assets | To be decided with the owner |

Before either agent builds this, the two need to agree on an interface: profile data in the engine-free catalog, and reactions requested by ID from the HUD. Proposed in the handoff note.
