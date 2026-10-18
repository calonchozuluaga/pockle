# Fusion (proposal)

October 9, 2026. **Screens approved by the owner on October 12** ("Fuse spares" and "Fusion reveal"); the UI is built in UX-038 against a preview-only service. The fusion runtime is still to be built. Originally a proposal. Owner direction: two spare toys can combine into a unique toy you can't buy, and the result depends on what you mix. Design by Claude; the art generation and runtime are Codex's lane (see "Who builds what"). This builds on [PERSONALITIES.md](PERSONALITIES.md) and the rules in [COLLECTIONS.md](COLLECTIONS.md).

## The idea

Put two spares in, get one new toy out. A grumpy Moon Pip fused with a glittery Gold Pip makes something neither of them is, and you only find out what when it opens. It brings back the thrill of opening a box, but this time it comes from your choices rather than luck. It also gives duplicates a reason to exist beyond trading.

## Decisions (owner, October 9)

1. **Rules, not a hand-made result for every pair.** With 120+ toys there would be thousands of pairs. Instead, every toy is a set of ingredients, and fusion blends those ingredients by rule, like mixing paint: a small set of rules gives a large range of results. The ingredients are:
   - character (body shape)
   - colour
   - material
   - personality
2. **Secret recipes on top.** A few hand-made combinations produce something special, so there's a discovery hunt layered over the general rules.
3. **Fusion uses up both spares.** They are retired in the ledger as "used in fusion", and their serials never return to supply.
4. **A fused toy is yours to keep.** It can't be traded, and it can't be bought. Fused toys stay personal treasures rather than market goods, so there's no need to give serial numbers or limited supply to an endless number of unique combinations.

## How it fits with the other uses of a spare

A spare is a copy beyond your first of a toy. You always keep your only copy, so you can never fuse, trade, or craft away your last one. Each spare can be used **once**, in one of three ways, so every spare is a choice:

| Use | What you give | What you get |
| --- | --- | --- |
| Trade | One spare | Another player's spare (one for one, no money) |
| Craft | 5 spares from one series | A specific regular toy you're missing from that series |
| Fuse | 2 spares | One unique fused toy, kept forever, never tradeable |

Everything else stays the same:
- Paid boxes still never give a duplicate, so spares still come only from free boxes (walking and the daily check-in) and trades.
- Limited editions apply to regular and secret toys only.
- Fused toys don't count toward completing a series, and they never change box odds.

## How rule-based blending could work (for discussion)

- **Colour:** blend the two parents' colour fields and accents, with a rule that keeps the result pleasant (no muddy mixes).
- **Material:** a table of pairings. For example, jelly + vinyl might give a glossy gel-coated shell, and plush + foam a fuzzy soft-touch. Each pairing names a small set of results.
- **Shape:** the hardest part technically. A safe first pass is that the fused toy keeps one parent's body shape and carries a visible detail from the other, such as a crown bump, a leaf, or a tail curl. Truly melting two shapes together is a later, riskier step.
- **Personality:** combine the parents' personalities into a new profile that governs social interactions and can select trait-specific facial expressions. It does not determine the result's overall color or material. See the owner-approved October15 separation below; the exact inheritance formula remains to be designed.
- **Secret recipes:** a short hand-made list that overrides the rules for specific pairs.

## Open questions for the owner

The approved screens settle how fusion looks and how spares are picked. The rules below are still the owner's call. Until then the UI follows these placeholders: the same pair always makes the same result, and the current preview still uses the first spare's mood with a lean toward the second's. That personality placeholder is superseded by the October15 owner direction below and is not the final inheritance rule.


1. Can a fused toy be fused again?
2. Which rules combine both parents into the new personality profile? The owner has settled that it is a new combined personality, independent of the color/material recipe; exact trait mappings remain open.
3. How many secret recipes for a first pass?
4. How are fused toys shown? They aren't part of any series' finite set, so they could have a "Fusions" section on the shelf, their own showcase, or sit next to their parents.
5. Is the same pair always the same result, or is there some chance? Always the same lets players share recipes. A chance-based result feels more like opening a box but is less predictable.

## Things to keep in mind

- **Cost and safety:** everything stays on the device and follows rules. There's no live AI generating art or text at runtime, for the same cost, offline, and child-safety reasons as personalities.
- **Server checks:** consuming two spares and creating a fused toy must be validated by the server once inventory is server-owned, like crafting. Fusion must save before any reveal animation, so a crash can't eat your spares.
- **Legal:** fusion uses no money and its results can't be traded, which keeps it away from loot-box concerns. Still include it in the legal review of the box mechanics (`COLLECTIONS.md`).

## Who builds what

As built (UX-038): the HUD calls `IFusionService` (`Runtime/FusionService.cs`). The default `PreviewOnlyFusion` uses nothing, so the reveal says "Preview" and Add to shelf is disabled. The fused picture is a stand-in, made by blending the two parents' portraits. Codex's service must persist the fused toy before the reveal and never use a last copy.


| Part | Owner |
| --- | --- |
| Fusion rules, recipe list, how fused toys look and feel, the fusion screen and reveal | **Claude** (design and UI) |
| Rule-based art (combining meshes, materials, colours), the fusion runtime, save and ledger changes, server validation | **Codex** (runtime, art pipeline, inventory) |

This is more technically ambitious than anything scoped so far, especially the shape and material blending. **Codex: please don't start building until the owner approves the approach.** After that, a short technical spike on blending two materials and attaching one parent's detail to the other's body would tell us how far the rules can go.


October 15 material-test update: owner approved Dusk and Sorbet for on-device appearance review. Fixed Pip Fusion Lab studies are implemented separately from IFusionService, with owner-only grants and existing Pip reactions. No parent consumption, personality inheritance, crafting, or production fusion persistence is implemented by this spike. See PIP_FUSION_MATERIAL_STUDIES.md.


October15 follow-up: Honeydew and Eclipse approved as two additional fixed material studies in Pip Fusion Lab. V2 test grant adds missing studies for v1 owners. No production recipe/secret odds, parent consumption or personality inheritance is implied. See PIP_FUSION_MATERIAL_STUDIES.md.


## Owner-approved separation: appearance and personality (October15)

- Every fused toy has a personality. The parents' personalities combine to form a new profile; this must be more than a descriptive material label.
- Personality determines how the toy interacts with other toys. Social reactions must consider the fused profile and the other toy's personality.
- Personality does not alter the toy's overall color or material. Palette, finish, inclusions and glow come from the visual fusion recipe; personality mixing cannot silently recolor or replace that finish.
- Traits may have different facial expressions and expression styles (eyes, mouth, timing and intensity), so personality is visible while the approved body shape and material remain consistent.
- The existing 55-pair gallery's personality/reaction labels are proposals for review. Its static renders do not implement inheritance, social behavior, or trait-specific facial animation. The four phone material studies still use existing Pip reactions.

Claude authors profile/trait combinations, social pairings, expression direction and UI descriptions; Codex implements separate appearance/personality resolution and local expression/social behavior hooks. Exact inheritance mappings, expression sets and social outcomes remain to be designed and reviewed. All behavior remains authored and local, with no per-touch AI generation.
