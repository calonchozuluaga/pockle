using System;
using System.Collections.Generic;

namespace Pockle.Runtime
{
    /// <summary>How a toy idles on the living shelf. Each character has a nature; its variety tints it.</summary>
    public enum PersonalityMotionStyle { Curious, Dream, Show, Calm, Sleep }

    /// <summary>A toy's authored personality (docs/PERSONALITIES.md). Presentation only: never affects odds or inventory.</summary>
    public sealed class ToyPersonality
    {
        public string Id { get; }
        /// <summary>The character's shared nature, e.g. "Curious and sunny".</summary>
        public string Nature { get; }
        public string Tint { get; }
        public IReadOnlyList<string> Traits { get; }
        public string Voice { get; }
        public float VoicePitch { get; }
        public PersonalityMotionStyle Motion { get; }
        public IReadOnlyList<KeyValuePair<string, string>> Reactions { get; }
        /// <summary>One word for fusion personality lines ("dreamy").</summary>
        public string Mood { get; }
        /// <summary>One word for fusion colour lines ("moon blue").</summary>
        public string Colour { get; }
        /// <summary>How the variety colours the nature, e.g. "with a show-off streak".</summary>
        public string Flavor { get; }

        internal ToyPersonality(string id, string nature, string tint, string[] traits, string voice, float pitch,
            PersonalityMotionStyle motion, string[] reactions, string mood, string colour, string flavor)
        {
            Flavor = flavor; Id = id; Nature = nature; Tint = tint; Traits = Array.AsReadOnly(traits); Voice = voice; VoicePitch = pitch;
            Motion = motion; Mood = mood; Colour = colour;
            var list = new List<KeyValuePair<string, string>>();
            for (int i = 0; i + 1 < reactions.Length; i += 2) list.Add(new KeyValuePair<string, string>(reactions[i], reactions[i + 1]));
            Reactions = list.AsReadOnly();
        }
    }

    /// <summary>A short, authored moment two toys share when they sit side by side on the shelf.</summary>
    public sealed class ShelfMoment
    {
        public string Id { get; }
        public string First { get; }
        public string Second { get; }
        public bool Night { get; }
        public string Caption { get; }
        /// <summary>How each toy describes the other on its personality page.</summary>
        public string FirstSays { get; }
        public string SecondSays { get; }
        internal ShelfMoment(string first, string second, bool night, string caption, string firstSays, string secondSays)
        {
            First = first; Second = second; Night = night; Caption = caption; FirstSays = firstSays; SecondSays = secondSays;
            Id = first + "+" + second + (night ? "@night" : "@day");
        }
        public bool Involves(string id) => First == id || Second == id;
        public bool Pairs(string a, string b) => First == a && Second == b || First == b && Second == a;
        public string Partner(string id) => First == id ? Second : First;
        public string LabelFor(string id) => First == id ? FirstSays : SecondSays;
    }

    /// <summary>What a fusion of two spares would make (docs/FUSION.md). Rules plus a few secret recipes.</summary>
    public sealed class FusionOutcome
    {
        public string Name { get; internal set; }
        public string Body { get; internal set; }
        public string Colour { get; internal set; }
        public string Material { get; internal set; }
        public string Personality { get; internal set; }
        public string Hint { get; internal set; }
        public bool Secret { get; internal set; }
    }

    /// <summary>
    /// Authored, shipped personality content: no runtime AI or network. Owner-approved October 12, 2026.
    /// Engine-free so it can move into Pockle.Core once Codex adds the behaviour runtime.
    /// </summary>
    public static class PersonalityBook
    {
        public const string Peach = "pip.peach-jelly", Moon = "pip.moon-pearl", Gold = "pip.gold-confetti", Mint = "pip.mint-mochi";
        private const string PipNature = "Curious and sunny";

        private static readonly Dictionary<string, ToyPersonality> toys = new Dictionary<string, ToyPersonality>
        {
            [Peach] = new ToyPersonality(Peach, PipNature, "Peach tint: the classic Pip", new[] { "Curious", "Sunny", "Friendly" },
                "Soft chirp", 1f, PersonalityMotionStyle.Curious,
                new[] { "Poke", "Leans in to look", "Squish", "Wiggles happily", "Pick up", "Peeks over the edge", "Left for a while", "Tidies the shelf" },
                "curious", "peach", "through and through"),
            [Moon] = new ToyPersonality(Moon, PipNature, "Moon tint: a little dreamy", new[] { "Curious", "Sunny", "Dreamy" },
                "Sleepy hum", .82f, PersonalityMotionStyle.Dream,
                new[] { "Poke", "Blinks slowly", "Squish", "Lets out a sigh", "Pick up", "Gazes at the sky", "Left for a while", "Counts the stars" },
                "dreamy", "moon blue", "with a dreamy side"),
            [Gold] = new ToyPersonality(Gold, PipNature, "Gold tint: show-off", new[] { "Curious", "Sunny" },
                "Bright chirp", 1.25f, PersonalityMotionStyle.Show,
                new[] { "Poke", "Strikes a pose", "Squish", "Giggles and sparkles", "Pick up", "Waves at you", "Left for a while", "Polishes its crown" },
                "show-off", "gold", "with a show-off streak"),
            [Mint] = new ToyPersonality(Mint, PipNature, "Mint tint: calm and cosy", new[] { "Curious", "Sunny", "Calm" },
                "Gentle coo", .9f, PersonalityMotionStyle.Calm,
                new[] { "Poke", "Settles a bit lower", "Squish", "Lets out a slow puff", "Pick up", "Snuggles into your hand", "Left for a while", "Takes a little nap" },
                "calm", "mint", "and in no hurry")
        };

        /// <summary>Six pairs of Pips, each with a day and a night moment: twelve to find.</summary>
        private static readonly ShelfMoment[] moments =
        {
            new ShelfMoment(Gold, Moon, false, "Gold Glitter shows off for Moon Jelly", "Shows off for Moon", "Watches Gold shine"),
            new ShelfMoment(Gold, Moon, true, "Moon Jelly stays up stargazing", "Snoozes mid-pose", "Stargazes all night"),
            new ShelfMoment(Peach, Mint, false, "Two Pips say hi", "Says hi to Mint", "Says hi to Peach"),
            new ShelfMoment(Peach, Mint, true, "Peach and Mint nod off together", "Dozes with Mint", "Dozes with Peach"),
            new ShelfMoment(Peach, Moon, false, "Peach Jelly peeks at Moon Jelly's pearls", "Peeks at Moon's pearls", "Lets Peach peek"),
            new ShelfMoment(Peach, Moon, true, "Peach and Moon share a sleepy lean", "Leans on Moon", "Leans on Peach"),
            new ShelfMoment(Peach, Gold, false, "Peach Jelly copies Gold Glitter's pose", "Copies Gold's pose", "Teaches Peach a pose"),
            new ShelfMoment(Peach, Gold, true, "Gold Glitter dozes off mid-pose", "Tucks Gold in", "Snoozes mid-pose"),
            new ShelfMoment(Moon, Mint, false, "Mint Soft keeps Moon Jelly company", "Daydreams with Mint", "Keeps Moon company"),
            new ShelfMoment(Moon, Mint, true, "Mint and Moon nod off together", "Dozes with Mint", "Dozes with Moon"),
            new ShelfMoment(Gold, Mint, false, "Gold Glitter shows off, Mint Soft stays calm", "Shows off for Mint", "Stays calm near Gold"),
            new ShelfMoment(Gold, Mint, true, "Mint Soft snoozes through the sparkle", "Sparkles in its sleep", "Snoozes through it")
        };

        public static IReadOnlyList<ShelfMoment> Moments { get; } = Array.AsReadOnly(moments);

        public static bool TryGet(string id, out ToyPersonality personality) => toys.TryGetValue(id ?? "", out personality);

        /// <summary>The moment two neighbours share at this time of day, if one is authored.</summary>
        public static ShelfMoment FindMoment(string a, string b, bool night)
        {
            foreach (var moment in moments) if (moment.Night == night && moment.Pairs(a, b)) return moment;
            return null;
        }

        private static readonly Dictionary<string, string> pairNames = new Dictionary<string, string>
        {
            [Key(Peach, Moon)] = "Dusk Pip", [Key(Peach, Gold)] = "Sunrise Pip", [Key(Peach, Mint)] = "Sorbet Pip",
            [Key(Moon, Gold)] = "Starlight Pip", [Key(Moon, Mint)] = "Sea Glass Pip", [Key(Gold, Mint)] = "Honeydew Pip",
            [Key(Peach, Peach)] = "Deep Peach Pip", [Key(Moon, Moon)] = "Full Moon Pip", [Key(Gold, Gold)] = "Treasure Pip",
            [Key(Mint, Mint)] = "Deep Mint Pip"
        };
        private static readonly HashSet<string> secretRecipes = new HashSet<string> { Key(Gold, Moon) };

        private static string Key(string a, string b) => string.CompareOrdinal(a, b) <= 0 ? a + "|" + b : b + "|" + a;

        public static bool IsSecretRecipe(string a, string b) => secretRecipes.Contains(Key(a, b));

        /// <summary>Describes what two spares would fuse into. The first spare's body is kept.</summary>
        public static FusionOutcome DescribeFusion(string first, string second)
        {
            if (!TryGet(first, out var a) || !TryGet(second, out var b)) return null;
            bool same = first == second;
            bool opaque = first == Mint || second == Mint;
            var outcome = new FusionOutcome
            {
                Name = pairNames.TryGetValue(Key(first, second), out var name) ? name : "Fusion Pip",
                Body = "Pip",
                // Read top to bottom: the second spare tints the top, the first settles below.
                Colour = same ? "Deep " + a.Colour : Capitalise(b.Colour) + " to " + a.Colour,
                Material = same && opaque ? "Soft gel" : opaque ? "Pearl gel" : "Pearl jelly",
                Personality = same ? "Extra " + a.Mood : Capitalise(a.Mood) + ", a bit " + b.Mood,
                Secret = IsSecretRecipe(first, second)
            };
            outcome.Hint = same
                ? "Two " + Name(first) + "s: a deeper " + a.Colour + " Pip, extra " + a.Mood + "."
                : "A Pip in " + a.Colour + " and " + b.Colour + ", " + a.Mood + " with a " + b.Mood + " side.";
            return outcome;
        }

        private static string Name(string id)
        {
            switch (id)
            {
                case Peach: return "Peach Jelly";
                case Moon: return "Moon Jelly";
                case Gold: return "Gold Glitter";
                case Mint: return "Mint Soft";
                default: return "Pip";
            }
        }

        private static string Capitalise(string text) => string.IsNullOrEmpty(text) ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);
    }
}
