# playpockle.com — content and image brief

November 3, 2026. Owner request: explain how the game and series work, and replace the flat PNG cut-outs with custom graphics that look like the toys do in the game. Claude wrote the copy and the image specs; **Codex renders the images from the Unity project** (real models, shaders, worlds and lighting) so the site matches what players will see.

Facts below follow the November 3 launch decisions (PRODUCT_BRIEF, CLAUDE_LAUNCH_HANDOFF_2026-11-03, LAUNCH_BOX_PRICING). Anything marked **[confirm]** is decided but not live yet, or not yet approved for public wording. Remove or reword those before the site goes public. Never publish a price, odds number, serial or supply count the store/server isn't actually using.

---

## Part 1 — Rules for every image

1. **Render from the game, don't generate.** Use the actual Unity models, finish materials (jelly translucency, plush flock, vinyl gloss, glitter, rose gold, diamond facets, molten cracks) and the collection worlds. AI image generation is only acceptable for a background plate if no in-game world fits, and must then be re-lit to match.
2. **No text, logos, UI, prices, serial numbers or odds inside images.** The site adds all words as live text. Box art may show its printed Pockle wordmark because that is the real product.
3. **Only active characters:** Pip, Fox, Axolotl, Firb, Coco, Snowy, Ginger, Reiny, Guin, Bundy, Wally. Never retired ones (Loop, Mallow, Moss, Nook, Ripple, Rolo, Sprig, Wisp, Bop, Dew, Tula) and never Fusion Lab studies presented as buyable.
4. **Toys at hero scale look like tabletop designer toys:** soft key light from upper left, warm rim light, a contact shadow on a surface, shallow depth of field. Expressions happy or curious, eyes open; no sleepy/grumpy faces in heroes.
5. **Leave text room.** Every wide image has a stated "quiet zone" (no toys, low detail) where the headline sits. Desktop images keep it on the left 40%; mobile crops keep it in the top 35%.
6. **Deliver each image at:** desktop 2560 px wide (stated ratio), mobile 1080 × 1350 (4:5) re-composed, not just cropped. PNG master in `ArtSource/Website/<id>/`, plus WebP at quality 82 for the site. Cut-outs: transparent PNG/WebP, 1600 px on the long side, 2 px padding-free bounding box.
7. **Two palettes are allowed:** light "milk" sections (#FBF8F5 base, cream/peach worlds) and dark "plum" sections (#2A1430 to #3E0A35, used for glow, Epic and Secret finishes). Match the background tone so images blend into the page colour without a visible edge.
8. **Loops (optional but recommended):** where marked 🎞, also export a 4–6 s seamless loop, 1080 × 1080, MP4 (H.264, no audio, ≤ 2 MB) plus a poster frame. Real in-game motion: squish and rebound, hop, tail wag, box lid opening.
9. **Alt text** for each image is in the table; keep it with the file in `ArtSource/Website/manifest.json`.

---

## Part 2 — Image list

| ID | Section | What it shows | Ratio / sizes | Quiet zone | Alt text |
|---|---|---|---|---|---|
| **H1** 🎞 | Home hero | Five or six Pockles from different characters and materials gathered on a soft cream tabletop in front of an open Pockle box: a peach-jelly Pip in front squishing slightly, a plush Fox, a vinyl Wally, an Axolotl, one glitter-gel and one glowing finish. Box lid open, faint glow from inside. Warm, inviting, premium. | 16:9 (2560×1440) + 4:5 mobile | Left 40%, plain cream backdrop | "A group of Pockle toys in jelly, plush and vinyl finishes around an open surprise box." |
| **H2** | Home hero (dark alt, for dark mode or a second slide) | Same idea on plum: a single Secret-tier molten Wally lit by its own glow, small Common Wallys softly out of focus behind. | 16:9 + 4:5 | Left 40% | "A glowing molten Wally Pockle on a dark background." |
| **S1** 🎞 | How it works · Walk | A Pip mid-hop on a garden path world, a closed collection box floating slightly above it, golden dust trailing like footsteps. Suggest motion, not a phone or map. | 1:1 (1600) | None (sits in a card) | "Pip hopping along a path toward a surprise box." |
| **S2** 🎞 | How it works · Open | The real opening animation captured at its peak: lid popping, light burst in the tier colour, the toy rising out. Render three variants: Common (soft white), Rare (gold), Epic (icy diamond). Site uses Common by default. | 1:1 (1600) ×3 | None | "A Pockle box bursting open as a toy pops out." |
| **S3** 🎞 | How it works · Play | Close-up of one jelly Pip being squished by an implied off-screen finger: body compressed, highlights stretched, tiny bubbles inside. The money shot for the material. | 1:1 (1600) | None | "A translucent jelly Pockle being squished." |
| **M1–M4** | Materials strip | Four tight close-ups, same camera and light, of one finish each: jelly (Pip peach), plush (Fox), vinyl (Wally matte), glitter/pearl (any glitter gel). Fill frame with texture plus part of the face. | 3:4 (1200×1600) each | None | "Close-up of a [jelly/plush/vinyl/glitter] Pockle finish." |
| **SE1** | Series explainer | One full series laid out like a collector's display: all 15 Wally finishes in rows on tiered acrylic risers in Wally's Pearl Grotto world, Commons on lower rows, three gold Rares, two diamond Epics, one molten Secret on a small raised plinth at the top centre. | 21:9 (2560×1100) + 4:5 mobile (stacked) | Top 20% thin band | "All fifteen Wally Pockles arranged on a display, from common to secret." |
| **SE2** | Rarity tiers | Four cut-outs, transparent, same pose and scale: Common Wally (Kelp Mint), Rare (Treasure Leaf / gold), Epic (Diamond Frost), Secret (Molten Abyss). | 4 × transparent 1600 | — | "Wally in [Common/Rare/Epic/Secret] finish." |
| **SE3** | Finite editions | A single Rare toy on a plinth with a soft spotlight and a small, blank brass plaque at its base (the site overlays "#412 of 6,000" style text in HTML **only as an illustrative example**, labelled as such). | 4:3 (2000×1500) | Plaque area | "A gold Pockle on a display stand with a blank plaque." |
| **C1–C11** | Collections gallery | One card per active character: its real closed box at three-quarter view on the left, its hero toy (approved lead finish) sitting beside it, inside one of its own worlds, softly blurred. Same camera height and scale for all eleven so the grid lines up. | 4:3 (1600×1200) each | None | "[Name] Pockle beside its collection box." |
| **F1** 🎞 | Fusion | Two Pip spares (e.g. Peach + Moon) on the left and right flowing into a swirl of both colours in the middle that resolves into a new blended Pip (use an existing approved fusion result, e.g. Dusk or Sorbet, rendered in its real material). | 16:9 + 4:5 | Left 35% | "Two Pip toys combining into a new one-of-a-kind Pip." |
| **T1** | Trading | Two hands' worth of shelves facing each other (no people): left shelf offers a plush Fox, right shelf offers a jelly Axolotl, a soft arc of light swapping them mid-air. Friendly, not a marketplace or money image. | 16:9 + 4:5 | Centre-top band | "Two Pockle shelves swapping a Fox and an Axolotl." |
| **W1** | Walking rewards | Isometric miniature world (a curving path through a small park) with four boxes placed along it at intervals, a Fox walking the path. Reads as "a box every so often as you walk". | 16:9 + 4:5 | Left 40% sky | "A Fox Pockle walking a path with surprise boxes along the way." |
| **D1** | Download / closing CTA | The app icon's coral jelly blob character, rendered in 3D from the Pip jelly material on deep plum, peeking over the bottom edge of the frame, waving. | 21:9 + 4:5 | Upper 60% | "A jelly Pockle peeking up and waving." |
| **OG** | Social share image | H1 composition re-framed. **Text allowed here only:** Pockle wordmark top-left. | 1200×630 | — | "Pockle collectible toys." |

Priority if time is short: **H1, S1–S3, SE1, SE2, C1–C11**, then the rest.

---

## Part 3 — Site structure and copy

Tone: warm, playful, plain. Short sentences. Explain clearly, never hype. No "limited time", no countdowns, no crossed-out prices, no implied rare guarantees.

### Nav
Home · How it works · Series · Collections · Trading · FAQ · **Get Pockle** (button)

### 1. Hero (H1)
**Headline:** Little toys. Big squish.
**Subhead:** Pockle is a collecting game of original designer toys you can squish, stretch and play with. Walk to earn boxes, open them to find out who's inside, and build your shelf.
**Buttons:** App Store · Google Play **[confirm: hide until listings are live; until then "Join the beta" or "Coming soon"]**
**Small line:** For ages 13+.

### 2. How it works (S1, S2, S3 as three cards)
**Walk.** Every 1,000 steps earns a free box, and a new one is ready every few hours. **[confirm: "up to 4 a day"]** Walking is the free way to collect, no purchase needed.
**Open.** Each box shows which collection it's from, but not which toy. Tap it, it shakes, and pop: someone new is inside.
**Play.** Every Pockle reacts to your touch. Jelly wobbles, plush squashes, vinyl spins and shines. Each one has its own personality, so no two act quite the same.

### 3. Made of different stuff (M1–M4)
**Heading:** Every finish feels different
Pockles come in translucent jelly, soft plush, glossy and matte vinyl, glitter gel and pearl. The finish changes more than the colour: it changes how a toy bounces, squishes and catches the light.

### 4. How series work (SE1)
**Heading:** One character, one series
Each series is a single character in many finishes. Wally's series, for example, has fifteen: the same whale in fifteen different looks.
- **The box tells you the series, not the toy.** You always know whose box you're opening; the finish is the surprise.
- **New series keep coming.** Earlier series stay available until they run out, and newer ones show up first. **[confirm cadence before naming one]**
- **Vintage counts.** Every toy remembers which series it came from and when you found it.

### 5. Rarity (SE2)
**Heading:** Common, Rare, Epic and Secret
Most finishes in a series are Common. Fewer are Rare, fewer still are Epic, and a series may hide a single Secret.
- **Common:** the everyday heroes of the series.
- **Rare:** gilded and golden.
- **Epic:** crystal-cut and glowing.
- **Secret:** the one you'll show everyone. **[confirm which series have a Secret at launch; Wally is the first with all four tiers]**
**Odds line:** Before you buy a box, the app shows you the exact chances for that box. They're based on how many of each tier are left, so they change as copies run out.

### 6. Finite editions (SE3)
**Heading:** When they're gone, they're gone
Every finish is made in a fixed number of copies, worldwide. Rarer tiers have fewer:
| Tier | Copies per finish |
|---|---|
| Common | 10,000 |
| Rare | 6,000 |
| Epic | 3,500 |
| Secret | 1,600 |
**[confirm before publishing these numbers]**
Each copy has its own number, like #412 of 6,000. *(Example only.)* When a series sells out, no more are made. You keep yours, can still play with it, and can still trade it.

### 7. Getting boxes
**Heading:** Walk for free, or pick up a box
- **Walking boxes** are free and come from your steps.
- **Collection Box:** choose a series, get one random toy from it.
- **Mystery Mix (3 or 5 boxes):** random series, random toys, for a little less per box.
Prices are shown in your local currency in the App Store and Google Play. **[confirm; optional line "Boxes from $0.99" only once store prices are live]**
Extra copies aren't wasted: they become spares for fusing and trading.
*No subscriptions, no coins, no ads in the way.* **[confirm "no ads"]**

### 8. Fusion (F1)
**Heading:** Two spares, one new toy
Have two spare copies of the same character? Fuse them. The result blends both finishes into a brand-new Pockle with its own name and personality, and it's yours alone. Fused toys can't be bought, traded or made again.
*(Both spares are used up; you always keep at least one of each original.)*

### 9. Trading (T1)
**Heading:** Swap your spares
Trade spare toys with other collectors, one toy for another. Both of you see exactly what's being swapped and both confirm before anything moves. Your last copy of a toy is never up for trade, and there's no money involved, only toys.
Sold-out finishes can still change hands, so a series never really ends.
Trading uses preset messages only, and you can block or report anyone. **[confirm public ledger page before linking it]**

### 10. Collections (C1–C11 grid)
**Heading:** Meet the collections
Card per character: name, one line, "See the series" link. Suggested lines:
- **Pip:** Sunny little jelly with two crown bumps. Where it all started.
- **Fox:** A plush fox with a big tail and bigger opinions.
- **Axolotl:** Wavy, wiggly and always smiling.
- **Firb:** **[one-liner: Codex drafts from the approved model and personality, Claude reviews]**
- **Coco:** **[one-liner: Codex drafts, Claude reviews]**
- **Snowy:** **[one-liner: Codex drafts, Claude reviews]**
- **Ginger:** **[one-liner: Codex drafts, Claude reviews]**
- **Reiny:** **[one-liner: Codex drafts, Claude reviews]**
- **Guin:** **[one-liner: Codex drafts, Claude reviews]**
- **Bundy:** **[one-liner: Codex drafts, Claude reviews]**
- **Wally:** A round ocean whale in fifteen finishes, from Kelp Mint to Molten Abyss.
**[confirm which collections are in the first release before listing all eleven]**

### 11. Fair play
**Heading:** Collecting, kept fair
- Odds are shown before every purchase.
- No bonus for spending more: packs buy more boxes, not better odds.
- Toys can't be sold for money, in or out of the game.
- A paid box is saved to your account before it opens, so a dropped connection never loses it or charges you twice.
- Pockle is for ages 13 and up.

### 12. FAQ
- **Is Pockle free?** Yes. You can collect by walking without paying. Boxes can also be bought.
- **Do I need to walk?** No, but walking is how you earn free boxes. Pockle only uses your step count.
- **Can I get duplicates?** Yes. Duplicates become spares you can fuse or trade.
- **What happens when a series sells out?** No new copies are made. Everyone who has one keeps it, and it can still be traded.
- **Can I see the odds?** Always, before you buy. They update as copies run out.
- **Which devices?** iPhone and Android. **[confirm minimum OS]**
- **What do I sign in with?** Apple or Google. Your collection is saved to your account.
- **How do I delete my account?** Link to the deletion page. **[required by both stores]**

### 13. Closing CTA (D1)
**Headline:** Someone's waiting in the box.
**Buttons:** App Store · Google Play **[confirm]**

### Footer
Privacy · Terms · Odds & editions · Delete account · Support (email on playpockle.com) · © 2026 Zulapp LLC

---

## Part 4 — Onboarding toy art (same renders, used in the app)

Owner, November 3: onboarding's toys look pixelated and sometimes glitched. Cause: they use the runtime `PortraitCutout`, a **256 px** render with no MSAA, matted by rendering on black and on white. Shown at up to 200 pt (≈600 px on a 3× phone) it is upscaled 2–3×, and the black/white matte miscomputes alpha for translucent jelly, refraction and emissive glow, so edges fringe or go grey.

The HUD now loads a pre-rendered image first and only falls back to that cut-out when none exists (`PrototypeHud.Onboarding.cs`, `OnboardingToyArt`). Codex provides:

- **Five hero toys**, one per slot (splash arc, birthday, sign-in, username and tutorial steps use slots 0–4). One toy per character, different materials, all happy/curious faces, no Secrets. Suggested: Pip (peach jelly), Fox (plush), Wally (Kelp Mint vinyl), Axolotl, plus one glitter or pearl finish.
- **File:** `Assets/Pockle/Runtime/Resources/UI/Onboarding/Toys/<collectible id with "." replaced by "_">.png`, e.g. `wally_kelp-mint.png`. 1024 × 1024, transparent, toy centred, feet at about 15% from the bottom, same camera/scale for all five. Import: Sprite/Default texture, no mipmaps needed, max size 1024, compression High Quality, alpha is transparency on.
- **Order:** `Resources/UI/Onboarding/Toys/heroes.txt`, one collectible ID per line in slot order. Without it, the old automatic pick is used.
- **Render properly:** 4× supersampled (render 4096, downsample), MSAA on, HDR on, transparent background from the camera's alpha (not a black/white difference), same studio lighting as Part 1 rule 4. Jelly keeps its translucency and a soft internal glow against transparency; check the edges on both the cream and the plum onboarding backgrounds.
- These are the same renders as the website cut-outs (SE2 style), so make them once and use them in both places.

**Optional, better still for the tutorial step:** a live 3D toy (the real `JellyToy` in a small camera rendered to the UI, idle animation and touch reactions on) instead of a still. That is a runtime change in Codex's lane; the HUD slot can take any `Texture`, including a live `RenderTexture`. Keep stills for the splash arc (five live cameras there would cost too much on older phones).

The same 256 px cut-out also feeds the live shelf and the box reveal. If the onboarding renders look right, a follow-up is raising `ToyPortrait` to 512 px with MSAA for those screens too (Codex's file; check memory and frame time).

## Notes for Codex

- Save renders under `ArtSource/Website/<ID>/` with `manifest.json` (id, files, ratio, alt text, toys/finishes used, world used). Commit masters there; the site gets the WebP/MP4 exports.
- If a finish named here isn't approved or playable on your branch, substitute the nearest approved finish of the same tier and note it in the manifest.
- Send Claude a contact sheet of first renders (a review HTML like the other `docs/*-review/` pages) before final export; Claude will review composition and quiet zones against this copy.
- The site itself (Squarespace or code) is not in this repo; this brief does not decide the platform.
