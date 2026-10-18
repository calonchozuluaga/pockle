using System;
using System.Collections.Generic;
using Pockle.Core;

namespace Pockle.Runtime
{
    /// <summary>What the toy just did, as far as its speech bubble cares (play screen and live shelf).</summary>
    public enum ReactionCue { Poke, Squish, PickUp, Turn, LeftAlone, Night, Hello }

    /// <summary>Small hand-drawn icons a bubble may show next to (or instead of) its sound. Not system emoji.</summary>
    public enum ExpressionSticker { None, Heart, Sparkle, Star, Moon, Note, Sweat, Zzz }

    /// <summary>One speech-bubble expression: a short written sound ("wheee!") and/or a sticker.</summary>
    public readonly struct ToyExpression
    {
        private readonly string sound;
        public readonly ExpressionSticker Sticker;
        /// <summary>Never null, also for <c>default</c> (a quiet turn).</summary>
        public string Sound => sound ?? "";
        public ToyExpression(string sound, ExpressionSticker sticker = ExpressionSticker.None) { this.sound = sound ?? ""; Sticker = sticker; }
        public bool IsEmpty => Sound.Length == 0 && Sticker == ExpressionSticker.None;
        public override string ToString() => Sticker == ExpressionSticker.None ? Sound : Sound.Length == 0 ? "[" + Sticker + "]" : Sound + " [" + Sticker + "]";
    }

    /// <summary>
    /// Speech-bubble expressions and "handle with care" warnings from a toy's personality (owner, October 16: sounds and
    /// stickers rather than sentences). Temperament picks the main pool; the finish's touch trait and extreme dials mix in
    /// a few of their own; Grumpy's third poke in a row gives in with a heart (docs/PERSONALITY_SYSTEM.md section 3).
    /// Authored and local: no AI or network. Engine-free so it can move to Pockle.Core with the personality runtime.
    /// </summary>
    public static class ToyExpressions
    {
        private static ToyExpression E(string sound, ExpressionSticker sticker = ExpressionSticker.None) => new ToyExpression(sound, sticker);
        private const ExpressionSticker Heart = ExpressionSticker.Heart, Sparkle = ExpressionSticker.Sparkle, Star = ExpressionSticker.Star,
            Moon = ExpressionSticker.Moon, Note = ExpressionSticker.Note, Sweat = ExpressionSticker.Sweat, Zzz = ExpressionSticker.Zzz;

        // [temperament][cue] → pool. Cue order: Poke, Squish, PickUp, Turn, LeftAlone, Night, Hello.
        private static readonly ToyExpression[][][] pools =
        {
            new[] { // Sunny
                new[] { E("hi hi!"), E("oh hi!"), E("hehe!") }, new[] { E("hehe!"), E("squee!"), E("tee-hee", Heart) },
                new[] { E("wheee!"), E("up up!") }, new[] { E("wheee!"), E("ooh?") },
                new[] { E("la la la~", Note), E("hello?") }, new[] { E("nini!", Moon), E("*yawn*") }, new[] { E("hiii!", Heart), E("yay!") } },
            new[] { // Grumpy
                new[] { E("hmph."), E("grrr"), E("…what.") }, new[] { E("hmph!"), E("grr…") },
                new[] { E("hey!"), E("put me down.") }, new[] { E("ugh."), E("hmph.") },
                new[] { E("…"), E("finally.") }, new[] { E("…mm."), E("g'night.") }, new[] { E("oh. it's you."), E("hmph.") } },
            new[] { // Shy
                new[] { E("eep!"), E("oh!"), E("…hi") }, new[] { E("eep!", Sweat), E("mm!") },
                new[] { E("eek!"), E("oh no oh no") }, new[] { E("…!"), E("um") },
                new[] { E("…"), E("phew") }, new[] { E("…nini", Moon) }, new[] { E("…hi", Heart), E("oh! hi") } },
            new[] { // Show-off
                new[] { E("ta-da!", Star), E("look!"), E("ooh yeah!") }, new[] { E("hah!"), E("nice!", Sparkle) },
                new[] { E("ta-da!", Star), E("wheee!") }, new[] { E("my good side!"), E("pose!", Sparkle) },
                new[] { E("ahem."), E("hello? fans?") }, new[] { E("encore tomorrow!", Star) }, new[] { E("you're back!", Star), E("ta-da!") } },
            new[] { // Dreamy
                new[] { E("hm…?"), E("oh…"), E("mm?") }, new[] { E("*sigh*"), E("mmm~", Heart) },
                new[] { E("ooh", Sparkle), E("floaty~") }, new[] { E("whoa…"), E("oooh") },
                new[] { E("la… la…", Note), E("…") }, new[] { E("ooh", Sparkle), E("so pretty…", Star) }, new[] { E("oh… hello", Moon) } },
            new[] { // Sleepy
                new[] { E("huh?!"), E("wha—"), E("mm?") }, new[] { E("mmf"), E("*yawn*") },
                new[] { E("five more min…"), E("zz— huh?") }, new[] { E("whoa—"), E("mm") },
                new[] { E("zzz…", Zzz), E("zzz") }, new[] { E("zzz…", Zzz) }, new[] { E("*yawn* hi"), E("mm… hi") } },
            new[] { // Mischief
                new[] { E("hehehe"), E("boop!"), E("nyeh!") }, new[] { E("hehe!"), E("nyeh~") },
                new[] { E("wheee!"), E("hehehe") }, new[] { E("boop!"), E("gotcha!") },
                new[] { E("psst!"), E("hehe…") }, new[] { E("*giggle*") }, new[] { E("psst! hi!"), E("hehehe") } },
            new[] { // Calm
                new[] { E("mm."), E("hello."), E("oh, hi.") }, new[] { E("ahh~"), E("mmm", Heart) },
                new[] { E("oh, okay."), E("hm~") }, new[] { E("easy now"), E("mm") },
                new[] { E("*breathes*"), E("…") }, new[] { E("goodnight~", Moon) }, new[] { E("hello again."), E("mm, hi") } },
        };

        /// <summary>Extra expressions a finish's touch trait (or a nature trait) adds, mixed in with every cue it lists.</summary>
        private sealed class Overlay
        {
            public readonly string[] Words; public readonly ReactionCue[] Cues; public readonly ToyExpression[] Lines;
            public Overlay(string[] words, ReactionCue[] cues, params ToyExpression[] lines) { Words = words; Cues = cues; Lines = lines; }
        }

        private static readonly ReactionCue[] Touchy = { ReactionCue.Poke, ReactionCue.Squish, ReactionCue.PickUp };
        private static readonly Overlay[] overlays =
        {
            new Overlay(new[] { "bouncy" }, new[] { ReactionCue.Poke, ReactionCue.PickUp, ReactionCue.Turn }, E("boing!"), E("boing boing!")),
            new Overlay(new[] { "cuddly", "snuggly" }, new[] { ReactionCue.Squish, ReactionCue.PickUp }, E("mmm~", Heart), E("hug!", Heart)),
            new Overlay(new[] { "fancy", "elegant" }, new[] { ReactionCue.Poke, ReactionCue.Turn }, E("ahem!"), E("how do you do", Sparkle)),
            new Overlay(new[] { "proud", "bold" }, new[] { ReactionCue.Turn, ReactionCue.PickUp }, E("ta-da!", Star)),
            new Overlay(new[] { "brave" }, new[] { ReactionCue.PickUp, ReactionCue.Turn }, E("onward!"), E("ha!")),
            new Overlay(new[] { "shy" }, Touchy, E("eep!", Sweat)),
            new Overlay(new[] { "sleepy" }, new[] { ReactionCue.Poke, ReactionCue.LeftAlone }, E("*yawn*"), E("zzz", Zzz)),
            new Overlay(new[] { "night-owl", "starry", "glowy", "dreamy", "daydreaming" }, new[] { ReactionCue.Night, ReactionCue.LeftAlone }, E("ooh", Sparkle), E("", Star)),
            new Overlay(new[] { "grumpy", "frosty" }, Touchy, E("hmph.")),
            new Overlay(new[] { "cheeky" }, new[] { ReactionCue.Poke, ReactionCue.LeftAlone }, E("blub!"), E("hehehe")),
            new Overlay(new[] { "chill" }, new[] { ReactionCue.Poke, ReactionCue.Squish }, E("s'all good")),
            new Overlay(new[] { "festive" }, new[] { ReactionCue.Poke, ReactionCue.Squish }, E("jingle!", Star)),
            new Overlay(new[] { "cosy", "warm" }, new[] { ReactionCue.Squish, ReactionCue.PickUp }, E("toasty~", Heart)),
        };

        /// <summary>How likely a cue is to show a bubble at all (0..1): bolder toys talk more; Hello always speaks.</summary>
        public static float Chattiness(string toyId, ReactionCue cue)
        {
            if (cue == ReactionCue.Hello || cue == ReactionCue.PickUp) return 1f; // big moments always get a bubble
            var dials = PersonalityBook.Dials(toyId);
            return Math.Max(.35f, Math.Min(.95f, .45f + dials.Boldness / 200f));
        }

        /// <summary>Every expression the toy can use for a cue, with weights (temperament 3, traits 2, mood mix 1).</summary>
        public static List<KeyValuePair<ToyExpression, int>> Candidates(string toyId, ReactionCue cue, bool night)
        {
            var list = new List<KeyValuePair<ToyExpression, int>>();
            var temperament = PersonalityBook.TemperamentOf(toyId);
            var dials = PersonalityBook.Dials(toyId);
            void Add(Temperament t, int weight) { foreach (var e in pools[(int)t][(int)cue]) list.Add(new KeyValuePair<ToyExpression, int>(e, weight)); }
            Add(temperament, 3);
            // Extreme dials leak through whatever the temperament: a very low mood grumbles a little, a night owl sparkles at night.
            if (dials.Mood < 30 && temperament != Temperament.Grumpy) Add(Temperament.Grumpy, 1);
            if (dials.Mood > 88 && temperament != Temperament.Sunny) Add(Temperament.Sunny, 1);
            if (dials.Energy < 25 && temperament != Temperament.Sleepy) Add(Temperament.Sleepy, 1);
            if (night && dials.Bedtime > 75 && temperament != Temperament.Dreamy) Add(Temperament.Dreamy, 1);
            foreach (string word in TraitWords(toyId))
                foreach (var overlay in overlays)
                    if (Array.IndexOf(overlay.Words, word) >= 0 && Array.IndexOf(overlay.Cues, cue) >= 0)
                        foreach (var e in overlay.Lines) list.Add(new KeyValuePair<ToyExpression, int>(e, 2));
            return list;
        }

        private static IEnumerable<string> TraitWords(string toyId)
        {
            if (!PersonalityBook.TryGet(toyId, out var p)) yield break;
            foreach (string t in p.Traits) yield return (t ?? "").ToLowerInvariant();
            if (!string.IsNullOrEmpty(p.TouchTrait)) yield return p.TouchTrait.ToLowerInvariant();
        }

        // ---- Warnings ------------------------------------------------------------------------------------------------

        private static readonly string[][] temperamentWarnings =
        {
            new[] { "Extremely friendly. May wave at strangers.", "Giggles are contagious. Squish responsibly." },
            new[] { "Grumpy. Poke at your own risk.", "Will pretend not to like you. Keep trying." },
            new[] { "Shy. Approach slowly.", "Blushes easily. Compliments may cause hiding." },
            new[] { "Show-off. May pose without warning.", "Do not point cameras unless ready for a show." },
            new[] { "Daydreams on duty. Poke gently.", "Easily distracted by stars." },
            new[] { "Naps frequently. Do not wake suddenly.", "Yawns are contagious." },
            new[] { "Mischief maker. Guard your nose.", "Was definitely not doing anything." },
            new[] { "Very calm. May calm you down too.", "Unbothered. Mostly." },
        };

        private static readonly Dictionary<string, string> traitWarnings = new Dictionary<string, string>
        {
            ["curious"] = "Very curious. Will peek over any edge.",
            ["bouncy"] = "Bouncy. Keep away from the edge of the shelf.",
            ["night-owl"] = "Wide awake at night. Please don't wake the stargazer.",
            ["starry"] = "Glows in the dark. Not a night light (but close).",
            ["glowy"] = "Glows in the dark. Not a night light (but close).",
            ["sleepy"] = "Sleepy. Handle like a sleeping thing.",
            ["cuddly"] = "Cuddly. Hugs may last longer than expected.",
            ["snuggly"] = "Snuggly. Hugs may last longer than expected.",
            ["fancy"] = "Fancy. Please use your best manners.",
            ["proud"] = "Proud. Compliments accepted at all times.",
            ["frosty"] = "Frosty at first. Warms up with time.",
            ["cheeky"] = "Cheeky. May blow bubbles at neighbours.",
        };

        /// <summary>
        /// Two or three light-hearted "handle with care" lines: its temperament's first, then its finish's touch trait (so
        /// finishes of one character read differently), then the temperament's second.
        /// </summary>
        public static IReadOnlyList<string> Warnings(string toyId)
        {
            var list = new List<string>(temperamentWarnings[(int)PersonalityBook.TemperamentOf(toyId)]);
            if (PersonalityBook.TryGet(toyId, out var p) && traitWarnings.TryGetValue((p.TouchTrait ?? "").ToLowerInvariant(), out var line) && !list.Contains(line))
                list.Insert(1, line);
            return list;
        }
    }

    /// <summary>
    /// Picks the next bubble for the toy on screen: weighted by personality, never the same expression twice in a row,
    /// sometimes silent (shy toys talk less), and Grumpy's third poke in a row gives in with a heart. One per play screen.
    /// </summary>
    public sealed class ExpressionPicker
    {
        private readonly Random random;
        private string toyId, last;
        private int pokeStreak;

        public ExpressionPicker(int seed) { random = new Random(seed); }
        public ExpressionPicker() : this(Environment.TickCount) { }

        /// <summary>Call when the toy changes or the player stops for a while; streaks start over.</summary>
        public void Reset(string toy = null) { toyId = toy; last = null; pokeStreak = 0; }

        /// <summary>The bubble for this reaction, or an empty expression when the toy stays quiet this time.</summary>
        public ToyExpression Next(string toy, ReactionCue cue, bool night)
        {
            if (toy != toyId) Reset(toy);
            pokeStreak = cue == ReactionCue.Poke ? pokeStreak + 1 : 0;
            var temperament = PersonalityBook.TemperamentOf(toy);
            if (cue == ReactionCue.Poke && pokeStreak >= 3 && temperament == Temperament.Grumpy)
            {
                pokeStreak = 0;
                return Remember(new ToyExpression("…fine.", ExpressionSticker.Heart)); // the secret soft side
            }
            if (random.NextDouble() > ToyExpressions.Chattiness(toy, cue)) return default;
            var candidates = ToyExpressions.Candidates(toy, cue, night);
            int total = 0;
            foreach (var c in candidates) if (c.Key.ToString() != last) total += c.Value;
            if (total == 0) return default;
            int roll = random.Next(total);
            foreach (var c in candidates)
            {
                if (c.Key.ToString() == last) continue;
                roll -= c.Value;
                if (roll < 0) return Remember(c.Key);
            }
            return default;
        }

        private ToyExpression Remember(ToyExpression e) { last = e.ToString(); return e; }
    }
}
