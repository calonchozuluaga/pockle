using System;
using System.Collections.Generic;
using Pockle.Core;

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
        /// <summary>The character the nature comes from ("fox").</summary>
        public string CharacterId { get; internal set; } = "pip";
        /// <summary>"Fox by nature" card: the shared nature and how every toy of the character behaves.</summary>
        public string NatureDetail { get; internal set; } = "";
        /// <summary>"Aurora touch" card: the finish's own trait ("Dreamy") and what it adds.</summary>
        public string TouchName { get; internal set; } = "";
        public string TouchTrait { get; internal set; } = "";
        public string TouchDetail { get; internal set; } = "";
        /// <summary>One line for the stage's About tab, e.g. "Curious, with a dreamy streak".</summary>
        public string Summary { get; internal set; } = "";

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
        /// <summary>The friendship level (1..3) that unlocks it (docs/PERSONALITY_SYSTEM.md section 4).</summary>
        public int Level { get; }
        internal ShelfMoment(string first, string second, bool night, string caption, string firstSays, string secondSays, int level = 1)
        {
            First = first; Second = second; Night = night; Caption = caption; FirstSays = firstSays; SecondSays = secondSays; Level = level;
            // Level 1 keeps the original id format, so moments players already found stay found.
            Id = first + "+" + second + (night ? "@night" : "@day") + (level > 1 ? "#" + level : "");
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
        /// <summary>The fused toy's temperament or named hybrid ("Stargazer").</summary>
        public string Personality { get; internal set; }
        /// <summary>The reveal's line: "It's… a Stargazer! Points everyone at the night sky."</summary>
        public string PersonalityLine { get; internal set; }
        public string Hint { get; internal set; }
        public bool Secret { get; internal set; }
    }

    /// <summary>
    /// Authored, shipped personality content: no runtime AI or network. Owner-approved October 12, 2026.
    /// Engine-free so it can move into Pockle.Core once Codex adds the behaviour runtime.
    /// </summary>
    public static partial class PersonalityBook
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

        /// <summary>
        /// Every collectible has a personality: the four original Pips are fully authored; every other toy is its
        /// character's nature plus its finish's touch (owner-approved October 14, canvas "Personalities for every pockle").
        /// </summary>
        public static bool TryGet(string id, out ToyPersonality personality)
        {
            id = id ?? "";
            if (toys.TryGetValue(id, out personality)) { EnsureLayers(personality); return true; }
            if (composed.TryGetValue(id, out personality)) return true;
            if (!ToyCatalog.TryGetCollectible(id, out var toy)) return false;
            var nature = NatureOf(toy.CharacterId);
            if (nature == null) return false;
            string finish = FinishName(toy);
            Touch touch = touches.TryGetValue(toy.CharacterId + ":" + finish, out var authored) ? authored
                : new Touch(Lower(finish) + " charm", "Its own " + Lower(finish) + " charm, on top of the " + nature.Name + " nature.", null, null);
            var reactions = new List<string>(nature.Reactions);
            if (touch.NightReaction != null) { reactions.Add("At night"); reactions.Add(touch.NightReaction); }
            if (reactions.Count > 8) reactions.RemoveRange(6, reactions.Count - 8);
            var traits = new List<string>(nature.Traits) { Capitalise(touch.Trait) };
            personality = new ToyPersonality(id, nature.Nature, finish + " touch: " + touch.Trait, traits.ToArray(), nature.Voice, nature.Pitch,
                touch.Motion ?? nature.Motion, reactions.ToArray(), touch.Trait, Lower(finish), "with a " + touch.Trait + " streak")
            {
                CharacterId = toy.CharacterId, NatureDetail = nature.Detail,
                TouchName = finish + " touch", TouchTrait = Capitalise(touch.Trait), TouchDetail = touch.Detail,
                Summary = nature.Summary + ", with a " + touch.Trait + " streak"
            };
            composed[id] = personality;
            return true;
        }

        private static readonly Dictionary<string, ToyPersonality> composed = new Dictionary<string, ToyPersonality>();

        private static void EnsureLayers(ToyPersonality p)
        {
            if (p.TouchName.Length > 0) return;
            var nature = natures["pip"];
            string finish = Name(p.Id).Split(' ')[0];
            p.CharacterId = "pip"; p.NatureDetail = nature.Detail;
            p.TouchName = finish + " touch"; p.TouchTrait = Capitalise(p.Mood);
            p.TouchDetail = p.Tint.Substring(p.Tint.IndexOf(':') + 1).Trim();
            p.TouchDetail = Capitalise(p.TouchDetail) + ".";
            p.Summary = nature.Summary + ", " + p.Flavor;
        }

        /// <summary>A character's shared nature: the "by nature" card and its defaults.</summary>
        public sealed class CharacterNature
        {
            public string Name, Nature, Summary, Detail, Voice; public float Pitch; public PersonalityMotionStyle Motion;
            public string[] Traits, Reactions;
        }

        private sealed class Touch
        {
            public readonly string Trait, Detail, NightReaction; public readonly PersonalityMotionStyle? Motion;
            public Touch(string trait, string detail, PersonalityMotionStyle? motion, string night) { Trait = trait; Detail = detail; Motion = motion; NightReaction = night; }
        }

        private static readonly Dictionary<string, CharacterNature> natures = new Dictionary<string, CharacterNature>
        {
            ["pip"] = new CharacterNature { Name = "Pip", Nature = PipNature, Summary = "Curious and sunny", Voice = "Soft chirp", Pitch = 1f, Motion = PersonalityMotionStyle.Curious,
                Detail = "Every Pip leans in to look, chirps hello and wiggles when squished.", Traits = new[] { "Curious", "Sunny" },
                Reactions = new[] { "Poke", "Leans in to look", "Squish", "Wiggles happily", "Pick up", "Peeks over the edge" } },
            ["fox"] = new CharacterNature { Name = "fox", Nature = "Alert and curious", Summary = "Curious", Voice = "Soft yip", Pitch = 1.1f, Motion = PersonalityMotionStyle.Curious,
                Detail = "Every fox keeps its ears up, glances around and rocks gently.", Traits = new[] { "Alert", "Curious" },
                Reactions = new[] { "Poke", "Ears perk up", "Turn it", "Keeps an eye on you", "Left for a while", "Curls up its tail" } },
            ["axolotl"] = new CharacterNature { Name = "axolotl", Nature = "Sleepy and gentle", Summary = "Gentle", Voice = "Bubbly hum", Pitch = .85f, Motion = PersonalityMotionStyle.Calm,
                Detail = "Every axolotl blinks slowly and waves its frills.", Traits = new[] { "Sleepy", "Gentle" },
                Reactions = new[] { "Poke", "Waves its frills", "Squeeze", "Gives a tiny smile", "Left for a while", "Drifts off to sleep" } },
            // Holiday characters (first drafts, October 15, from Codex's reaction notes; the owner may rename any line).
            ["tula"] = new CharacterNature { Name = "tula", Nature = "Calm and patient", Summary = "Patient", Voice = "Slow hum", Pitch = .8f, Motion = PersonalityMotionStyle.Calm,
                Detail = "Every Tula takes its time, tucks in its feet and peeks out slowly.", Traits = new[] { "Calm", "Patient" },
                Reactions = new[] { "Poke", "Peeks out its head", "Pick up", "Wiggles its feet", "Left for a while", "Naps in its shell" } },
            ["firb"] = new CharacterNature { Name = "firb", Nature = "Festive and proud", Summary = "Festive", Voice = "Jingly giggle", Pitch = 1.05f, Motion = PersonalityMotionStyle.Show,
                Detail = "Every Firb waves a branch, wobbles its star and shimmies its ornaments.", Traits = new[] { "Festive", "Proud" },
                Reactions = new[] { "Tap", "Waves a branch", "Tap again", "Wobbles its star", "Left for a while", "Sways sleepily" } },
            ["coco"] = new CharacterNature { Name = "coco", Nature = "Cosy and warm", Summary = "Cosy", Voice = "Warm slurp", Pitch = .95f, Motion = PersonalityMotionStyle.Curious,
                Detail = "Every Coco turns its handle to say hello and bounces its marshmallows.", Traits = new[] { "Cosy", "Warm" },
                Reactions = new[] { "Tap", "Turns its handle hello", "Tap again", "Bounces its marshmallows", "Left for a while", "Sways cosily" } },
            ["snowy"] = new CharacterNature { Name = "snowy", Nature = "Chilly and cheerful", Summary = "Cheerful", Voice = "Frosty giggle", Pitch = 1.1f, Motion = PersonalityMotionStyle.Curious,
                Detail = "Every Snowy waves a mitten, wobbles its pom and shuffles in the snow.", Traits = new[] { "Cheerful", "Chilly" },
                Reactions = new[] { "Tap", "Waves a mitten", "Tap again", "Wobbles its pom", "Left for a while", "Nods off" } },
            ["ginger"] = new CharacterNature { Name = "ginger", Nature = "Sweet and playful", Summary = "Sweet", Voice = "Crunchy giggle", Pitch = 1.15f, Motion = PersonalityMotionStyle.Show,
                Detail = "Every Ginger waves an iced paw, wiggles its ears and does a button dance.", Traits = new[] { "Sweet", "Playful" },
                Reactions = new[] { "Tap", "Waves an iced paw", "Tap again", "Does a button dance", "Left for a while", "Takes a sweet bow" } }
        };

        public static CharacterNature NatureOf(string characterId) => natures.TryGetValue(characterId ?? "", out var nature) ? nature : FallbackNature(characterId);

        /// <summary>
        /// A plain nature for a character nobody has written one for yet, so a new collection never shows a toy without a
        /// personality (or fails the every-toy test). It says only what every pockle does.
        /// </summary>
        private static CharacterNature FallbackNature(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;
            if (natures.TryGetValue(characterId, out var cached)) return cached;
            string name = ToyCatalog.TryGetCharacter(characterId, out var character) ? character.DisplayName : Capitalise(characterId);
            var nature = new CharacterNature { Name = name, Nature = "Friendly and curious", Summary = "Friendly", Voice = "Happy hum", Pitch = 1f,
                Motion = PersonalityMotionStyle.Curious, Detail = "Every " + name + " says hello when you tap it.", Traits = new[] { "Friendly", "Curious" },
                Reactions = new[] { "Tap", "Says hello", "Left for a while", "Settles in" } };
            natures[characterId] = nature;
            return nature;
        }

        /// <summary>Each finish adds one trait (first draft, October 14; the owner may rename any line).</summary>
        private static readonly Dictionary<string, Touch> touches = new Dictionary<string, Touch>
        {
            ["fox:Honey"] = new Touch("cuddly", "Sweet and cuddly. Leans into a hug.", PersonalityMotionStyle.Calm, null),
            ["fox:Pearl"] = new Touch("fancy", "Polite and a little fancy. Sits up straight.", PersonalityMotionStyle.Show, null),
            ["fox:Rose Quartz"] = new Touch("shy", "Blushes easily and hides behind its tail.", PersonalityMotionStyle.Calm, null),
            ["fox:Mochi"] = new Touch("sleepy", "Squishy and sleepy. First to nap.", PersonalityMotionStyle.Calm, null),
            ["fox:Cloud"] = new Touch("daydreaming", "A daydreamer, always looking up.", PersonalityMotionStyle.Dream, null),
            ["fox:Midnight"] = new Touch("night-owl", "Wide awake when everyone sleeps.", PersonalityMotionStyle.Dream, "Stays up watching"),
            ["fox:Apricot"] = new Touch("bouncy", "Sunny and bouncy. Can't sit still.", PersonalityMotionStyle.Show, null),
            ["fox:Classic"] = new Touch("brave", "The brave one. Leads the pack.", PersonalityMotionStyle.Curious, null),
            ["fox:Copper"] = new Touch("proud", "Proud and shiny. Loves being seen.", PersonalityMotionStyle.Show, null),
            ["fox:Aurora"] = new Touch("dreamy", "Night owl. Glows under the night lamp and on the night shelf.", PersonalityMotionStyle.Dream, "Glows and stargazes"),
            ["pip:Starlight Glow"] = new Touch("starry", "Rare and starry. Glows softly in the dark.", PersonalityMotionStyle.Dream, "Glows like a star"),
            ["pip:Boucle Plush"] = new Touch("snuggly", "Fuzzy and snuggly.", PersonalityMotionStyle.Calm, null),
            ["pip:Bouclé Plush"] = new Touch("snuggly", "Fuzzy and snuggly.", PersonalityMotionStyle.Calm, null),
            ["pip:Velvet Flock"] = new Touch("elegant", "Velvety and elegant.", PersonalityMotionStyle.Show, null),
            ["pip:Gloss Vinyl"] = new Touch("bold", "Shiny and bold.", PersonalityMotionStyle.Show, null),
            ["pip:Matte Vinyl"] = new Touch("chill", "Laid-back and chill.", PersonalityMotionStyle.Calm, null),
            ["pip:Coated Metallic"] = new Touch("brave", "Metallic and brave.", PersonalityMotionStyle.Curious, null),
            ["axolotl:Boucle Plush"] = new Touch("cuddly", "Fuzzy, soft and cuddly.", PersonalityMotionStyle.Calm, null),
            // First drafts, October 15 (personality system): one toy per temperament the characters don't cover yet.
            ["axolotl:Blossom Plush"] = new Touch("cuddly", "Fuzzy, soft and cuddly.", PersonalityMotionStyle.Calm, null),
            ["axolotl:Rose Glitter"] = new Touch("shy", "Shy and sparkly. Hides behind its frills.", PersonalityMotionStyle.Calm, null),
            ["axolotl:Sky Matte"] = new Touch("grumpy", "Grumpy when woken up. Secretly loves a cuddle.", PersonalityMotionStyle.Calm, null),
            ["axolotl:Coral Gloss"] = new Touch("cheeky", "Cheeky. Blows bubbles at its neighbours.", PersonalityMotionStyle.Show, null),
            ["axolotl:Biolume Glow"] = new Touch("glowy", "Glows softly and gazes at the stars.", PersonalityMotionStyle.Dream, "Glows in the dark"),
            ["snowy:Coated Metallic"] = new Touch("frosty", "Frosty at first. Warms up to its friends.", PersonalityMotionStyle.Calm, null)
        };

        /// <summary>A finish's name without its rarity suffix ("Aurora · Super rare" is "Aurora").</summary>
        public static string FinishName(CollectibleDefinition toy)
        {
            string name = toy.FinishDisplayName; int dot = name.IndexOf(" · ", StringComparison.Ordinal);
            return dot > 0 ? name.Substring(0, dot) : name;
        }

        /// <summary>What the shelf calls a toy: "Gold Glitter", "Aurora".</summary>
        public static string ShortName(string id)
        {
            if (toys.ContainsKey(id ?? "")) return Name(id);
            return ToyCatalog.TryGetCollectible(id, out var toy) ? FinishName(toy) : "Pockle";
        }

        private static string Lower(string text) => string.IsNullOrEmpty(text) ? text : char.ToLowerInvariant(text[0]) + text.Substring(1);

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
                Secret = IsSecretRecipe(first, second)
            };
            DescribeFusedPersonality(first, second, out string personality, out string line);
            outcome.Personality = personality; outcome.PersonalityLine = line;
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
