# Fusion (proposal)

October 9, 2026. **Proposal for the owner's review.** Owner direction: two spare toys can combine into a unique toy you can't buy, and the result depends on what you mix. Design by Claude; the art generation and runtime are Codex's lane (see "Who builds what"). This builds on [PERSONALITIES.md](PERSONALITIES.md) and the rules in [COLLECTIONS.md](COLLECTIONS.md).

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
- **Personality:** a blend of the two temperaments, or one parent's temperament with a quirk from the other (open question 2).
- **Secret recipes:** a short hand-made list that overrides the rules for specific pairs.

## Open questions for the owner

1. Can a fused toy be fused again?
2. How does personality blend: does one parent win, does it land in between, or is it a mix with a random lean?
3. How many secret recipes for a first pass?
4. How are fused toys shown? They aren't part of any series' finite set, so they could have a "Fusions" section on the shelf, their own showcase, or sit next to their parents.
5. Is the same pair always the same result, or is there some chance? Always the same lets players share recipes. A chance-based result feels more like opening a box but is less predictable.

## Things to keep in mind

- **Cost and safety:** everything stays on the device and follows rules. There's no live AI generating art or text at runtime, for the same cost, offline, and child-safety reasons as personalities.
- **Server checks:** consuming two spares and creating a fused toy must be validated by the server once inventory is server-owned, like crafting. Fusion must save before any reveal animation, so a crash can't eat your spares.
- **Legal:** fusion uses no money and its results can't be traded, which keeps it away from loot-box concerns. Still include it in the legal review of the box mechanics (`COLLECTIONS.md`).

## Who builds what

| Part | Owner |
| --- | --- |
| Fusion rules, recipe list, how fused toys look and feel, the fusion screen and reveal | **Claude** (design and UI) |
| Rule-based art (combining meshes, materials, colours), the fusion runtime, save and ledger changes, server validation | **Codex** (runtime, art pipeline, inventory) |

This is more technically ambitious than anything scoped so far, especially the shape and material blending. **Codex: please don't start building until the owner approves the approach.** After that, a short technical spike on blending two materials and attaching one parent's detail to the other's body would tell us how far the rules can go.
