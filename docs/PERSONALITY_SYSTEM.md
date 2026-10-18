# Personality system: temperaments, faces, shelf friendships and fused personalities

October 15, 2026. Design by Claude. **Approved by the owner on October 15** from the canvas row "Personality system" (Eight temperaments, Grumpy face set, Shelf relationships, Secretly Sweet). Defaults in section 8 stand unless the owner changes them. Builds on [PERSONALITIES.md](PERSONALITIES.md) and the October 15 owner direction in [FUSION.md](FUSION.md): a fused toy gets a new personality made from its parents', which decides its faces and how it gets on with other toys, and never changes its colour or material. Everything stays authored and runs on the device: no AI at runtime, no network.

## In one paragraph

Every toy has four personality dials (energy, mood, boldness, bedtime). The dials put it in one of eight **temperaments** (Sunny, Grumpy, Shy, Show-off, Dreamy, Sleepy, Mischief, Calm). A temperament picks the toy's **face set** (how its eyes and mouth sit at rest and react) and its **reaction mix** on the play screen. When two toys sit side by side on the live shelf, their two temperaments decide their **relationship** (best buds, playful rivals, warming up…), and the relationship picks which shelf moments they play, which slowly deepen the more days they spend together. **Fusion** blends the parents' dials, keeps strong traits, adds one small spark, and some temperament pairs make a named **hybrid** (Grumpy + Sunny = Secretly Sweet) with its own faces and moments.

## 1. The four dials

Each dial runs 0–100. The toy details page already shows three as bars; mood is new.

| Dial | Low (0) | High (100) | What it changes |
| --- | --- | --- | --- |
| Energy | Sleepy | Bouncy | Idle speed and size, how big reactions are, how often it naps |
| Mood | Grumpy | Sweet | Resting face (lids, mouth), how quickly it smiles, who it warms to |
| Boldness | Shy | Show-off | Whether it leans in or peeks out, poses, starts moments or follows |
| Bedtime | Early bird | Night owl | Which shelf (day or night) it's livelier on, night glow moments |

- **Character sets the base, the finish nudges it.** Each character has base dials; each finish adds up to ±20 on one or two dials (Gold Glitter: +20 boldness; Moon Jelly: +25 bedtime, −10 energy). These are authored tables in `PersonalityBook`, replacing today's guessed slider values.
- Dials never change over time on their own. Nothing decays and nothing punishes being away (PERSONALITIES.md decision 3).

## 2. Eight temperaments

A toy's temperament is the one whose centre is nearest its dials (simple distance; ties go to the character's default). One word players learn quickly, shown as a badge on the details page.

| Temperament | Dial centre (E / M / B / N) | At rest | When poked | Signature |
| --- | --- | --- | --- | --- |
| **Sunny** | 70 / 85 / 60 / 35 | Wide eyes, soft smile, slow blinks | Leans in, big smile | Waves at whoever's next to it |
| **Grumpy** | 45 / 15 / 45 / 50 | Half-closed lids, flat or pouty mouth, side glance | Squints, turns away "hmph" | After the third poke, a tiny reluctant smile (its secret soft side) |
| **Shy** | 40 / 70 / 15 / 45 | Eyes down, small mouth, blushes | Hides, peeks back | Peeks out from behind a bolder neighbour |
| **Show-off** | 80 / 70 / 90 / 40 | Bright eyes, open grin | Strikes a pose, sparkle | Turns to face the camera when you look |
| **Dreamy** | 35 / 75 / 40 / 85 | Soft, slightly unfocused eyes looking up | Slow "hm?", drifts | Stargazes; glows on the night shelf |
| **Sleepy** | 15 / 65 / 35 / 60 | Heavy lids, little "o" mouth, long blinks | Startles awake, then dozes | Nods off mid-moment |
| **Mischief** | 85 / 60 / 75 / 55 | Narrowed sparkly eyes, crooked grin | Wiggles, sneaky look | Sneaks up on neighbours, booped-nose gag |
| **Calm** | 40 / 70 / 45 / 40 | Relaxed open eyes, gentle smile | Small nod | Settles others down |

Characters' natural temperaments (draft): Pip Sunny, Fox Sunny/Curious, Axolotl Sleepy, Tula Calm, Firb Show-off, Coco Calm, Snowy Sunny, Ginger Mischief. Finishes move some toys into another temperament (for example Moon Jelly Pip lands on Dreamy, Gold Glitter Pip on Show-off), which is what makes collecting every finish interesting.

## 3. Faces

Today the face shader has three channels: blink, happy (smile) and surprise. Temperaments need a slightly bigger vocabulary. **Request for Codex:** four more face channels, each 0–1 and blended like the existing ones:

| Channel | 0 | 1 | Used by |
| --- | --- | --- | --- |
| Lids | Open | Half-closed | Grumpy, Sleepy, Dreamy |
| Frown | Neutral | Pout / frown (mouth corners down) | Grumpy, Shy (small) |
| Glance | Centre | Looking to the side (signed: left/right) | Grumpy, Shy, Mischief |
| Blush | None | Full cheek blush | Shy, Sunny, Secretly Sweet |

Each temperament defines a small **face set**: rest, poke, squish, pick up, left alone, night, neighbour moment, and its signature. Each pose is a set of channel values plus timing (blink interval, how long a pose holds, how fast it returns). Example, Grumpy:

| Moment | Lids | Frown | Glance | Happy | Timing |
| --- | --- | --- | --- | --- | --- |
| Rest | .45 | .35 | .6 (away) | 0 | Blinks every 5–7 s, slow |
| Poke | .7 | .6 | .9 | 0 | Squints and turns away, holds 1.2 s |
| Third poke | .3 | 0 | .2 | .4 | Reluctant smile, 0.8 s, then back to rest |
| Night | .6 | .2 | 0 | .1 | Softer: grumpy toys are nicer at night |

Calm motion keeps faces but drops body motion, as today.

## 4. Shelf relationships

When two toys sit next to each other on the live shelf, their temperaments decide a **relationship**, and the relationship picks their moments. Moments stay short, authored and kind.

| Relationship | Made by (examples) | Day moment | Night moment |
| --- | --- | --- | --- |
| **Best buds** | Sunny + Sunny, Sunny + Calm, Dreamy + Dreamy | Matching wiggles | Lean together and doze |
| **Warming up** | Grumpy + Sunny, Grumpy + Show-off | Sunny waves, Grumpy looks away… then sneaks a tiny wave back | Grumpy lets Sunny lean on it |
| **Big and little** | Calm + Shy, Show-off + Shy | Shy peeks out from behind the bolder one | The bold one keeps watch while Shy sleeps |
| **Playful rivals** | Show-off + Show-off, Mischief + Show-off | Pose-off: each tries a bigger pose | Both pretend not to be sleepy |
| **Partners in mischief** | Mischief + Mischief, Mischief + Sunny | Booped-nose gag, both giggle | Whispering and giggling |
| **Quiet company** | Calm + Sleepy, Grumpy + Grumpy, Shy + Shy | Sit still, synchronised blinks | Nod off at the same time |
| **Stargazers** | Dreamy + anyone at night | — | Look up together; Dreamy glows |

The full 8 × 8 table lives in `PersonalityBook` (36 unordered pairs → 7 relationship kinds), with authored moments per relationship and a few hand-written ones for specific toy pairs that override the table, like today's "Gold Glitter shows off for Moon Jelly".

**Friendship that deepens (never decays).** Each pair that has been neighbours on the live shelf builds a level from 1 to 3 on the days they're seen together (1 day, 3 days, 7 days; *defaults*). Higher levels unlock new moments in that relationship ("Warming up" level 3: Grumpy saves Sunny a spot). Moving toys apart keeps the level; it never goes down. This gives the live shelf a reason to come back to without any guilt, and the details page's "Shelf friends" shows each friend's level.

**Kid-safety rules for all moments:** no insults, no one left out, no scary faces, no romance; any friction (rivals, grumpy) resolves warmly inside the same moment.

## 5. Fused personalities

The fused toy's personality is resolved once, when it's made, and saved with it, so later table edits never change a toy someone already owns.

1. **Blend the dials.** Each dial starts at the parents' average.
2. **Strong traits carry.** If either parent is extreme on a dial (below 20 or above 80), the child moves 60% of the way from the average toward that parent's value. Two grumpy parents make a really grumpy child.
3. **One spark.** One dial, picked from the fusion's ID, shifts by up to ±12. The same pair always gives the same result (which lets players share recipes), but the child isn't a plain average.
4. **Temperament.** Nearest centre, as in section 2, unless the parents' temperaments form a **hybrid**:

| Parents | Hybrid | Faces | Signature |
| --- | --- | --- | --- |
| Grumpy + Sunny | **Secretly Sweet** | Grumpy rest face, blushes when it smiles | Smiles first time, every time, for its best bud only |
| Shy + Show-off | **Stage Fright Star** | Shy rest face, Show-off poses that end in a blush | Poses only when no one's next to it |
| Sleepy + Sunny or Mischief | **Power Napper** | Heavy lids, sudden bright wake-ups | Naps, bounces up, naps again |
| Dreamy + Sunny | **Stargazer** | Upward glance, soft smile | Points everyone at the night sky |
| Mischief + Calm | **Sneaky Sage** | Calm face with a crooked grin | Pranks so gently no one notices |
| Grumpy + Grumpy | **Grumpy Royalty** | Maximum lids and pout | Grumps at everyone, then shares its spot |
| Show-off + Show-off | **Superstar** | Permanent sparkle eyes | Steals every moment, bows |
| Shy + Shy | **Whisper Pair** | Tiny mouth, constant blush | Hides with any neighbour |

5. **Faces and reactions.** A hybrid has its own face set (above). A plain-blend child uses its temperament's set, takes **one signature reaction from each parent**, and its voice pitch is the parents' average.
6. **Its relationship row.** Hybrids map to the closest base temperament for relationships, plus their own hand-written moments with each parent's temperament (Secretly Sweet + Sunny: "the only one it waves to first").
7. **Never appearance.** None of this touches the fused toy's colour, material, inclusions or glow; those come from the visual recipe (FUSION.md, October 15).

The reveal says who it is: "It's… Secretly Sweet! Grumpy on the outside, soft on the inside." The details page shows a "Made from" row with both parents.

## 6. Where players see it

- **Toy details:** temperament badge, four bars (adds Mood), the face set as tiny sticker faces under "How it reacts", shelf friends with friendship levels, and "Made from" for fused toys.
- **Play screen:** tap reactions are weighted by temperament (Grumpy turns away more, Show-off poses more) from each character's existing five reactions, plus face poses from its set.
- **Live shelf:** resting faces, relationship moments, friendship levels deepening.
- **Fusion reveal and activity feed:** "Theo fused a Secretly Sweet."
- **Home peek:** the moment chip uses relationship moments.

## 7. Who builds what

| Part | Owner |
| --- | --- |
| Dial tables (characters and finishes), temperament centres, face sets as data, relationship table, moment and hybrid writing, all UI | **Claude** |
| Face channels (lids, frown, glance, blush) in the toy shaders and expression drivers; temperament-weighted reaction choice; shelf face poses for live toys | **Codex** |
| Engine-free `PersonalityProfile` (four dials, temperament, hybrid id), the fusion inheritance rule with portable checks, saving the resolved profile with the fused toy (versioned save) | **Codex**, from this spec |
| Friendship levels per shelf pair (local presentation preference, like found moments) | **Claude** |

## 8. Defaults adopted with the approval (owner can still change)

1. Eight temperaments with the names above.
2. Mood dial shown as "Grumpy ↔ Sweet".
3. Friendship levels at 1 / 3 / 7 days together.
4. All eight hybrids are designed; build order starts with Secretly Sweet, Stage Fright Star, Power Napper and Stargazer.
5. Still open (FUSION.md question 1): can a fused toy be fused again? If yes, hybrids of hybrids resolve by dials only, with no further named hybrids.

## 9. Build notes (Claude, October 15, UiBuild "v2 screens 19")

- **Dials:** `PersonalityBook.Dials(id)` = character base (`characterDials`) + the toy's own nudge (`toyNudges`, chosen to match its touch trait) or the finish's shared nudge (`finishNudges`). Per-toy nudges go past ±20 where a finish needs to land on another temperament (Moon Jelly +38 bedtime). Results: Peach Sunny, Moon Dreamy, Gold Show-off, Mint Calm; fox Rose Quartz Shy, Pearl and Copper Show-off, Mochi Sleepy, Cloud/Midnight/Aurora Dreamy; axolotl Sky Matte Grumpy, Rose Glitter Shy, Coral Gloss Mischief, Biolume Dreamy; Snowy Coated Metallic Grumpy ("frosty"). New touch lines for those axolotl and Snowy finishes are first drafts.
- **Temperament:** nearest centre, ties to the character's base temperament (`TemperamentOf`).
- **Relationships:** the 8 × 8 table and moment templates live in `PersonalityBook.Temperaments.cs`. Level 1 keeps the twelve authored Pip moments and their ids (moments already found stay found); levels 2 and 3 add new moments with ids ending `#2`/`#3`. A show-off with an easy-going neighbour still "shows off for" it on level 1.
- **Friendship:** `ShelfFriendships` (`pockle.shelf.friendship.v1`, presentation only) counts one day per neighbouring pair while the Shelf page is open. The details page shows "Best buds · Lv 2" once a pair's moment is found.
- **Fusion copy:** the reveal shows the hybrid or temperament ("Stargazer") and its line ("It's… a Stargazer! Points everyone at the night sky."). `PreviewFusedDials` follows section 5 for copy only; switch to Codex's `PersonalityProfile` once it lands so the copy and the saved profile can't disagree.

## 10. Speech bubbles and "handle with care" (owner, October 16)

Approved on the canvas boards "Play: speech bubble + personality": tapping the toy shows a small bubble above its head with a **written sound or a sticker, never a sentence** ("wheee!", "hmph.", "zzz…", a heart). Under the name, the play screen shows the temperament badge, two traits and a subtle "handle with care" chip (tap for the next one).

Logic (`Runtime/UI/ToyExpressions.cs`, tests in `ToyExpressionTests.cs`):

- **Cues:** Poke, Squish, PickUp, Turn, LeftAlone, Night, Hello (opening the toy).
- **Pools:** each temperament has 1–3 sounds per cue (weight 3). Examples: Sunny "hi hi!", "wheee!"; Grumpy "hmph.", "grrr", "put me down."; Shy "eep!", "…hi"; Show-off "ta-da!", "pose!"; Dreamy "hm…?", "floaty~"; Sleepy "huh?!", "zzz…"; Mischief "boop!", "hehehe"; Calm "mm.", "ahh~".
- **Traits mix in** (weight 2): a toy's nature and touch traits add their own sounds to some cues. Bouncy: "boing!"; cuddly/snuggly: "hug!"; fancy/elegant: "ahem!"; shy: "eep!"; sleepy: "*yawn*"; night-owl/starry/glowy/dreamy: sparkle at night; grumpy/frosty: "hmph."; cheeky: "blub!"; festive, cosy.
- **Extreme dials leak through** (weight 1): mood < 30 grumbles a little, mood > 88 adds Sunny sounds, energy < 25 adds Sleepy sounds, bedtime > 75 sparkles at night.
- **Picker:** weighted random, never the same bubble twice in a row. Chattiness: 0.45 + boldness/200, between 35% and 95% of taps (shy toys talk less). Hello and PickUp always speak. **Grumpy's third poke in a row** gives in: "…fine." with a heart.
- **Stickers:** heart, sparkle, star, moon, note, sweat, zzz. Small hand-drawn icons rather than system emoji, which the current text system can't draw.
- **Warnings:** the temperament's first line, then the finish's touch trait line if there is one, then the temperament's second line. Examples: "Grumpy. Poke at your own risk.", "Bouncy. Keep away from the edge of the shelf.", "Daydreams on duty. Poke gently."
- **Needed from Codex:** a reaction event from `TactilePrototype` (which cue just played) so the HUD can call `ExpressionPicker.Next` and show the bubble above the toy's head (head position in screen space, or the stage viewport and toy bounds).
