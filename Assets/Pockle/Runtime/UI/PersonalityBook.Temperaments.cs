using System;
using System.Collections.Generic;
using Pockle.Core;

namespace Pockle.Runtime
{
    /// <summary>The eight temperaments (docs/PERSONALITY_SYSTEM.md section 2, owner-approved October 15).</summary>
    public enum Temperament { Sunny, Grumpy, Shy, ShowOff, Dreamy, Sleepy, Mischief, Calm }

    /// <summary>How two neighbours on the live shelf get on (section 4).</summary>
    public enum Relationship { BestBuds, WarmingUp, BigAndLittle, PlayfulRivals, PartnersInMischief, QuietCompany, Stargazers }

    /// <summary>A toy's four dials, each 0..100: energy (sleepy to bouncy), mood (grumpy to sweet), boldness (shy to
    /// show-off) and bedtime (early bird to night owl).</summary>
    public readonly struct PersonalityDials
    {
        public readonly int Energy, Mood, Boldness, Bedtime;
        public PersonalityDials(int energy, int mood, int boldness, int bedtime)
        {
            Energy = Clamp(energy); Mood = Clamp(mood); Boldness = Clamp(boldness); Bedtime = Clamp(bedtime);
        }
        public int this[int dial] => dial == 0 ? Energy : dial == 1 ? Mood : dial == 2 ? Boldness : Bedtime;
        public PersonalityDials Plus(int energy, int mood, int boldness, int bedtime) =>
            new PersonalityDials(Energy + energy, Mood + mood, Boldness + boldness, Bedtime + bedtime);
        public float DistanceTo(PersonalityDials other)
        {
            float sum = 0;
            for (int i = 0; i < 4; i++) { float d = this[i] - other[i]; sum += d * d; }
            return (float)Math.Sqrt(sum);
        }
        private static int Clamp(int value) => value < 0 ? 0 : value > 100 ? 100 : value;
        public override string ToString() => Energy + "/" + Mood + "/" + Boldness + "/" + Bedtime;
    }

    /// <summary>A named personality a fusion can make from two parent temperaments (section 5).</summary>
    public sealed class HybridPersonality
    {
        public string Id { get; }
        public string Name { get; }
        /// <summary>How the reveal says it: "Secretly Sweet", "a Stargazer".</summary>
        public string Phrase { get; }
        public string Line { get; }
        internal HybridPersonality(string id, string name, string phrase, string line) { Id = id; Name = name; Phrase = phrase; Line = line; }
    }

    public static partial class PersonalityBook
    {
        // ---- Dials -------------------------------------------------------------------------------------------------

        /// <summary>Each character's base dials (energy / mood / boldness / bedtime). First draft, October 15.</summary>
        private static readonly Dictionary<string, PersonalityDials> characterDials = new Dictionary<string, PersonalityDials>
        {
            ["pip"] = new PersonalityDials(68, 82, 58, 38),     // Sunny
            ["fox"] = new PersonalityDials(64, 72, 62, 44),     // Sunny, a little more alert
            ["axolotl"] = new PersonalityDials(22, 70, 32, 58), // Sleepy
            ["tula"] = new PersonalityDials(36, 72, 40, 38),    // Calm
            ["firb"] = new PersonalityDials(78, 74, 86, 42),    // Show-off
            ["coco"] = new PersonalityDials(42, 80, 44, 40),    // Calm
            ["snowy"] = new PersonalityDials(72, 86, 56, 30),   // Sunny
            ["ginger"] = new PersonalityDials(82, 62, 72, 52),  // Mischief
            ["reiny"] = new PersonalityDials(64, 82, 46, 40),   // Sunny, a little shy (its "shy bow")
            ["guin"] = new PersonalityDials(38, 76, 42, 42),    // Calm
            ["bundy"] = new PersonalityDials(78, 78, 84, 42)    // Show-off
        };

        /// <summary>What every finish adds unless a toy has its own line below (small nudges, so the character still shows).</summary>
        private static readonly Dictionary<string, int[]> finishNudges = new Dictionary<string, int[]>
        {
            ["clear-jelly"] = new[] { 0, 0, 0, 0 },
            ["pearl-jelly"] = new[] { -6, 0, 0, 12 },
            ["confetti-gel"] = new[] { 6, 0, 12, 0 },
            ["mochi-foam"] = new[] { -10, 4, 0, 0 },
            ["velvet-flock"] = new[] { -6, 0, 8, 0 },
            ["boucle-plush"] = new[] { -8, 8, 0, 0 },
            ["matte-vinyl"] = new[] { -6, 0, 0, 0 },
            ["gloss-vinyl"] = new[] { 0, 0, 10, 0 },
            ["coated-metallic"] = new[] { 0, -6, 8, 0 },
            ["glow-jelly"] = new[] { -6, 0, 0, 20 }
        };

        /// <summary>Authored per-toy nudges ("character:finish-id"), chosen to match each finish's touch trait.</summary>
        private static readonly Dictionary<string, int[]> toyNudges = new Dictionary<string, int[]>
        {
            // Pip: Peach is the classic Sunny Pip; Moon dreams, Gold shows off, Mint is calm.
            ["pip:pearl-jelly"] = new[] { -24, 0, -16, 38 },
            ["pip:confetti-gel"] = new[] { 12, 0, 32, 0 },
            ["pip:mochi-foam"] = new[] { -28, 0, -14, 0 },
            ["pip:velvet-flock"] = new[] { -8, 0, 14, 0 },
            ["pip:boucle-plush"] = new[] { -24, 4, -10, 0 },
            ["pip:matte-vinyl"] = new[] { -26, -8, 0, 0 },
            ["pip:gloss-vinyl"] = new[] { 10, 0, 30, 0 },
            ["pip:coated-metallic"] = new[] { 8, 0, 14, 0 },
            ["pip:glow-jelly"] = new[] { -24, 0, -14, 44 },
            // Fox, in label order: Honey, Pearl, Rose Quartz, Mochi, Cloud, Midnight, Apricot, Classic, Copper, Aurora.
            ["fox:clear-jelly"] = new[] { -16, 12, -10, 0 },
            ["fox:pearl-jelly"] = new[] { 6, 0, 22, 0 },
            ["fox:confetti-gel"] = new[] { -14, 0, -40, 0 },
            ["fox:mochi-foam"] = new[] { -44, 0, -22, 14 },
            ["fox:velvet-flock"] = new[] { -22, 0, -20, 36 },
            ["fox:boucle-plush"] = new[] { -6, 0, -4, 42 },
            ["fox:matte-vinyl"] = new[] { 22, 12, 8, -4 },
            ["fox:gloss-vinyl"] = new[] { 0, 0, 10, 0 },
            ["fox:coated-metallic"] = new[] { 0, -8, 26, 0 },
            ["fox:glow-jelly"] = new[] { -26, 0, -18, 42 },
            // Axolotl: mostly sleepy; Rose Glitter is shy, Sky Matte grumpy, Coral Gloss cheeky, Biolume dreamy.
            ["axolotl:confetti-gel"] = new[] { 16, 6, -20, -6 },
            ["axolotl:boucle-plush"] = new[] { 0, 10, 0, 0 },
            ["axolotl:matte-vinyl"] = new[] { 16, -56, 0, 0 },
            ["axolotl:gloss-vinyl"] = new[] { 60, -8, 42, 0 },
            ["axolotl:glow-jelly"] = new[] { 10, 0, 0, 30 },
            // Holiday: a frosty Snowy that has to warm up to you.
            ["snowy:coated-metallic"] = new[] { -20, -66, 0, 0 }
        };

        /// <summary>A toy's four dials: its character's base plus its finish's nudge.</summary>
        public static PersonalityDials Dials(string id)
        {
            string character = "pip", finish = "clear-jelly";
            if (ToyCatalog.TryGetCollectible(id, out var toy)) { character = toy.CharacterId; finish = toy.FinishId; }
            else if (!toys.ContainsKey(id ?? "")) return new PersonalityDials(50, 70, 50, 45);
            var dials = characterDials.TryGetValue(character, out var baseDials) ? baseDials : new PersonalityDials(60, 76, 52, 40);
            int[] nudge = toyNudges.TryGetValue(character + ":" + finish, out var own) ? own
                : finishNudges.TryGetValue(finish, out var shared) ? shared : null;
            return nudge == null ? dials : dials.Plus(nudge[0], nudge[1], nudge[2], nudge[3]);
        }

        // ---- Temperaments ------------------------------------------------------------------------------------------

        private static readonly PersonalityDials[] centres =
        {
            new PersonalityDials(70, 85, 60, 35), // Sunny
            new PersonalityDials(45, 15, 45, 50), // Grumpy
            new PersonalityDials(40, 70, 15, 45), // Shy
            new PersonalityDials(80, 70, 90, 40), // Show-off
            new PersonalityDials(35, 75, 40, 85), // Dreamy
            new PersonalityDials(15, 65, 35, 60), // Sleepy
            new PersonalityDials(85, 60, 75, 55), // Mischief
            new PersonalityDials(40, 70, 45, 40)  // Calm
        };

        private static readonly string[] temperamentNames = { "Sunny", "Grumpy", "Shy", "Show-off", "Dreamy", "Sleepy", "Mischief", "Calm" };
        private static readonly string[] temperamentPhrases = { "Sunny", "Grumpy", "Shy", "a Show-off", "Dreamy", "Sleepy", "a Mischief-maker", "Calm" };
        private static readonly string[] temperamentLines =
        {
            "Waves at whoever's next to it.",
            "Turns away when poked, then sneaks a smile.",
            "Peeks out from behind bolder friends.",
            "Strikes a pose whenever you look.",
            "Stargazes and glows on the night shelf.",
            "Nods off in the middle of things.",
            "Sneaks up and boops its neighbours.",
            "Settles everyone down."
        };

        public static PersonalityDials Centre(Temperament temperament) => centres[(int)temperament];
        public static string TemperamentName(Temperament temperament) => temperamentNames[(int)temperament];
        public static string TemperamentLine(Temperament temperament) => temperamentLines[(int)temperament];

        /// <summary>The temperament whose centre is nearest; ties go to <paramref name="tieBreak"/>.</summary>
        public static Temperament Nearest(PersonalityDials dials, Temperament tieBreak = Temperament.Calm)
        {
            var best = tieBreak; float bestDistance = dials.DistanceTo(Centre(tieBreak));
            for (int i = 0; i < centres.Length; i++)
            {
                float d = dials.DistanceTo(centres[i]);
                if (d < bestDistance - .001f) { best = (Temperament)i; bestDistance = d; }
            }
            return best;
        }

        public static Temperament TemperamentOf(string id)
        {
            string character = ToyCatalog.TryGetCollectible(id, out var toy) ? toy.CharacterId : "pip";
            var tie = characterDials.TryGetValue(character, out var baseDials) ? Nearest(baseDials) : Temperament.Calm;
            return Nearest(Dials(id), tie);
        }

        // ---- Relationships -----------------------------------------------------------------------------------------

        private const Relationship BB = Relationship.BestBuds, WU = Relationship.WarmingUp, BL = Relationship.BigAndLittle,
            PR = Relationship.PlayfulRivals, PM = Relationship.PartnersInMischief, QC = Relationship.QuietCompany;

        /// <summary>Temperament × temperament (symmetric), in enum order: Sunny, Grumpy, Shy, Show-off, Dreamy, Sleepy, Mischief, Calm.</summary>
        private static readonly Relationship[,] relationships =
        {
            //          Su  Gr  Sh  SO  Dr  Sl  Mi  Ca
            /* Su */ {  BB, WU, BL, BB, BB, BB, PM, BB },
            /* Gr */ {  WU, QC, QC, WU, QC, QC, WU, QC },
            /* Sh */ {  BL, QC, QC, BL, QC, QC, BL, BL },
            /* SO */ {  BB, WU, BL, PR, BB, BB, PR, BB },
            /* Dr */ {  BB, QC, QC, BB, BB, QC, BB, QC },
            /* Sl */ {  BB, QC, QC, BB, QC, QC, PM, QC },
            /* Mi */ {  PM, WU, BL, PR, BB, PM, PM, BB },
            /* Ca */ {  BB, QC, BL, BB, QC, QC, BB, QC }
        };

        private static readonly string[] relationshipNames = { "Best buds", "Warming up", "Big and little", "Playful rivals", "Partners in mischief", "Quiet company", "Stargazers" };

        public static string RelationshipName(Relationship relationship) => relationshipNames[(int)relationship];

        /// <summary>How two toys get on. At night, any pair with a dreamer becomes stargazers.</summary>
        public static Relationship RelationshipOf(string a, string b, bool night)
        {
            Temperament ta = TemperamentOf(a), tb = TemperamentOf(b);
            if (night && (ta == Temperament.Dreamy || tb == Temperament.Dreamy)) return Relationship.Stargazers;
            return relationships[(int)ta, (int)tb];
        }

        /// <summary>Friendship deepens with days spent side by side: level 1 at 1 day, 2 at 3 days, 3 at 7 (never decays).</summary>
        public static int FriendshipLevel(int days) => days >= 7 ? 3 : days >= 3 ? 2 : days >= 1 ? 1 : 0;

        // Moment templates per relationship: [level 1..3][day, night] → caption, lead says, other says.
        // {L} leads (the bolder, warmer, livelier or dreamier one, per relationship); {O} is the other.
        // Kid-safety (section 4): no insults, no one left out, no scary faces, no romance; friction resolves warmly.
        private static readonly string[][][] templates =
        {
            new[] { // Best buds
                new[] { "{L} and {O} do matching wiggles", "Wiggles with {O}", "Wiggles with {L}" },
                new[] { "{L} and {O} lean together and doze", "Leans on {O}", "Leans on {L}" },
                new[] { "{L} saves {O} the sunny spot", "Saves {O} a spot", "Gets the sunny spot" },
                new[] { "{L} tucks {O} in", "Tucks {O} in", "Gets tucked in by {L}" },
                new[] { "{L} and {O} do their secret handshake", "Secret handshake with {O}", "Secret handshake with {L}" },
                new[] { "{L} and {O} snore in time", "Snores in time with {O}", "Snores in time with {L}" }
            },
            new[] { // Warming up: {L} is the warm one, {O} the grumpy one
                new[] { "{L} waves, {O} looks away… then waves back", "Waves at {O}", "Sneaks a wave back" },
                new[] { "{O} lets {L} lean on it", "Leans on {O}", "Lets {L} lean" },
                new[] { "{O} pretends not to watch {L}'s trick", "Shows {O} a trick", "Secretly watches {L}" },
                new[] { "{O} hums {L} to sleep", "Gets hummed to sleep", "Hums for {L}" },
                new[] { "{O} saves {L} a spot", "Gets a spot from {O}", "Saves {L} a spot" },
                new[] { "{O} smiles in its sleep next to {L}", "Makes {O} smile", "Smiles near {L}" }
            },
            new[] { // Big and little: {L} is the bold one
                new[] { "{O} peeks out from behind {L}", "Lets {O} hide behind it", "Peeks out from behind {L}" },
                new[] { "{L} keeps watch while {O} sleeps", "Keeps watch over {O}", "Sleeps safe near {L}" },
                new[] { "{L} helps {O} say hi", "Helps {O} say hi", "Says hi with {L}" },
                new[] { "{L} leaves a light on for {O}", "Leaves a light on", "Sleeps by {L}'s light" },
                new[] { "{O} copies {L}'s pose", "Teaches {O} a pose", "Copies {L}'s pose" },
                new[] { "{O} and {L} doze back to back", "Dozes with {O}", "Dozes with {L}" }
            },
            new[] { // Playful rivals
                new[] { "{L} and {O} have a pose-off", "Pose-off with {O}", "Pose-off with {L}" },
                new[] { "{L} and {O} pretend they're not sleepy", "Not sleepy at all", "Not sleepy either" },
                new[] { "{L} and {O} race to wave first", "Races {O} to wave", "Races {L} to wave" },
                new[] { "{L} and {O} have a yawning contest", "Yawns at {O}", "Yawns back at {L}" },
                new[] { "{L} and {O} take a bow together", "Bows with {O}", "Bows with {L}" },
                new[] { "{L} and {O} call it a tie and doze off", "Calls it a tie", "Calls it a tie" }
            },
            new[] { // Partners in mischief: {L} is the livelier one
                new[] { "{L} boops {O}'s nose, and they both giggle", "Boops {O}'s nose", "Giggles at {L}" },
                new[] { "{L} and {O} whisper and giggle", "Whispers with {O}", "Giggles with {L}" },
                new[] { "{L} and {O} play peekaboo", "Plays peekaboo", "Plays peekaboo" },
                new[] { "{L} and {O} sneak a midnight snack", "Sneaks a snack", "Shares the snack" },
                new[] { "{L} and {O} plan a surprise for you", "Plans with {O}", "Plans with {L}" },
                new[] { "{L} and {O} giggle in their sleep", "Giggles in its sleep", "Giggles in its sleep" }
            },
            new[] { // Quiet company
                new[] { "{L} and {O} blink in time", "Blinks with {O}", "Blinks with {L}" },
                new[] { "{L} and {O} nod off together", "Dozes with {O}", "Dozes with {L}" },
                new[] { "{L} and {O} watch the clouds go by", "Cloud-watches with {O}", "Cloud-watches with {L}" },
                new[] { "{L} and {O} breathe in time", "Breathes with {O}", "Breathes with {L}" },
                new[] { "{L} and {O} scoot a little closer", "Scoots up to {O}", "Scoots up to {L}" },
                new[] { "{L} and {O} dream the same dream", "Shares a dream", "Shares a dream" }
            },
            new[] { // Stargazers (night only; {L} is the dreamer)
                new[] { "{L} and {O} look at the sky", "Looks up with {O}", "Looks up with {L}" },
                new[] { "{L} stays up stargazing", "Stargazes all night", "Dozes nearby" },
                new[] { "{L} and {O} look at the sky", "Looks up with {O}", "Looks up with {L}" },
                new[] { "{L} shows {O} the brightest star", "Shows {O} a star", "Looks up with {L}" },
                new[] { "{L} and {O} look at the sky", "Looks up with {O}", "Looks up with {L}" },
                new[] { "{L} and {O} make a wish on a star", "Makes a wish", "Makes a wish too" }
            }
        };

        /// <summary>
        /// The moment two neighbours share at this time of day and friendship level (1..3). A few hand-written Pip moments
        /// take level 1; everything else comes from the pair's relationship. The same pair gets the same moment whichever
        /// side each sits on. Level 1 keeps the original moment ids, so moments already found stay found.
        /// </summary>
        public static ShelfMoment FindMoment(string a, string b, bool night, int level)
        {
            if (a == null || b == null || a == b) return null;
            level = level < 1 ? 1 : level > 3 ? 3 : level;
            if (level == 1) foreach (var moment in moments) if (moment.Night == night && moment.Pairs(a, b)) return moment;
            return RelationshipMoment(a, b, night, level);
        }

        /// <summary>The level-1 moment (what the details page and tests use when no friendship is known).</summary>
        public static ShelfMoment FindMoment(string a, string b, bool night) => FindMoment(a, b, night, 1);

        public static ShelfMoment RelationshipMoment(string a, string b, bool night, int level)
        {
            if (a == null || b == null || a == b) return null;
            if (string.CompareOrdinal(a, b) > 0) { var swap = a; a = b; b = swap; } // one moment per pair
            if (!TryGet(a, out _) || !TryGet(b, out _)) return null;
            var relationship = RelationshipOf(a, b, night);
            string lead = Lead(relationship, a, b), other = lead == a ? b : a;
            string nl = ShortName(lead), no = ShortName(other);
            string[] t;
            // A show-off with an easy-going neighbour performs for it on their first day (the original Pip moment).
            Temperament tl = TemperamentOf(lead), to = TemperamentOf(other);
            if (!night && level == 1 && (relationship == Relationship.BestBuds || relationship == Relationship.BigAndLittle)
                && (tl == Temperament.ShowOff) != (to == Temperament.ShowOff))
            {
                if (to == Temperament.ShowOff) { var s = lead; lead = other; other = s; nl = ShortName(lead); no = ShortName(other); }
                t = new[] { "{L} shows off for {O}", "Shows off for {O}", "Watches {L} shine" };
            }
            else t = templates[(int)relationship][(level - 1) * 2 + (night ? 1 : 0)];
            string Fill(string s) => s.Replace("{L}", nl).Replace("{O}", no);
            // The details page's friend cards are narrow, so what each says uses first names ("Shows off for Moon").
            string Brief(string s) => s.Replace("{L}", nl.Split(' ')[0]).Replace("{O}", no.Split(' ')[0]);
            string leadSays = Brief(t[1]), otherSays = Brief(t[2]);
            bool leadFirst = lead == a;
            var result = new ShelfMoment(a, b, night, Fill(t[0]), leadFirst ? leadSays : otherSays, leadFirst ? otherSays : leadSays, level);
            return result;
        }

        /// <summary>Who leads a relationship's moments.</summary>
        private static string Lead(Relationship relationship, string a, string b)
        {
            PersonalityDials da = Dials(a), db = Dials(b);
            int Pick(int va, int vb) => va > vb ? 0 : vb > va ? 1 : -1;
            int choice;
            switch (relationship)
            {
                case Relationship.WarmingUp: choice = Pick(da.Mood, db.Mood); break;          // the warm one waves first
                case Relationship.PartnersInMischief: choice = Pick(da.Energy, db.Energy); break;
                case Relationship.Stargazers: choice = Pick(da.Bedtime, db.Bedtime); break;
                default: choice = Pick(da.Boldness, db.Boldness); break;
            }
            return choice == 1 ? b : a;
        }

        // ---- Fusion personality (reveal copy) ----------------------------------------------------------------------

        private static readonly HybridPersonality SecretlySweet = new HybridPersonality("secretly-sweet", "Secretly Sweet", "Secretly Sweet", "Grumpy on the outside, soft on the inside.");
        private static readonly HybridPersonality StageFrightStar = new HybridPersonality("stage-fright-star", "Stage Fright Star", "a Stage Fright Star", "Shy in a crowd, a star when no one's looking.");
        private static readonly HybridPersonality PowerNapper = new HybridPersonality("power-napper", "Power Napper", "a Power Napper", "Naps, bounces up, naps again.");
        private static readonly HybridPersonality Stargazer = new HybridPersonality("stargazer", "Stargazer", "a Stargazer", "Points everyone at the night sky.");
        private static readonly HybridPersonality SneakySage = new HybridPersonality("sneaky-sage", "Sneaky Sage", "a Sneaky Sage", "Pranks so gently no one notices.");
        private static readonly HybridPersonality GrumpyRoyalty = new HybridPersonality("grumpy-royalty", "Grumpy Royalty", "Grumpy Royalty", "Grumps at everyone, then shares its spot.");
        private static readonly HybridPersonality Superstar = new HybridPersonality("superstar", "Superstar", "a Superstar", "Steals every moment, then takes a bow.");
        private static readonly HybridPersonality WhisperPair = new HybridPersonality("whisper-pair", "Whisper Pair", "a Whisper Pair", "Tiny voice, big heart. Hides with any neighbour.");

        /// <summary>The named hybrid two parent temperaments make, if any (section 5 table).</summary>
        public static HybridPersonality HybridOf(Temperament a, Temperament b)
        {
            bool Is(Temperament x, Temperament y) => a == x && b == y || a == y && b == x;
            if (Is(Temperament.Grumpy, Temperament.Sunny)) return SecretlySweet;
            if (Is(Temperament.Shy, Temperament.ShowOff)) return StageFrightStar;
            if (Is(Temperament.Sleepy, Temperament.Sunny) || Is(Temperament.Sleepy, Temperament.Mischief)) return PowerNapper;
            if (Is(Temperament.Dreamy, Temperament.Sunny)) return Stargazer;
            if (Is(Temperament.Mischief, Temperament.Calm)) return SneakySage;
            if (Is(Temperament.Grumpy, Temperament.Grumpy)) return GrumpyRoyalty;
            if (Is(Temperament.ShowOff, Temperament.ShowOff)) return Superstar;
            if (Is(Temperament.Shy, Temperament.Shy)) return WhisperPair;
            return null;
        }

        /// <summary>
        /// Preview of a fused toy's dials by the section 5 rule: average, extremes (below 20 or above 80) pull 60% toward
        /// that parent, then one spark of up to ±12 on a dial picked from the pair. Copy only: the saved profile is
        /// Codex's engine-free PersonalityProfile, resolved once when the toy is made; switch to it when it lands.
        /// </summary>
        public static PersonalityDials PreviewFusedDials(string first, string second)
        {
            PersonalityDials a = Dials(first), b = Dials(second);
            var values = new int[4];
            for (int i = 0; i < 4; i++)
            {
                float avg = (a[i] + b[i]) / 2f, v = avg;
                bool ea = a[i] < 20 || a[i] > 80, eb = b[i] < 20 || b[i] > 80;
                if (ea && (!eb || Math.Abs(a[i] - avg) >= Math.Abs(b[i] - avg))) v = avg + (a[i] - avg) * .6f;
                else if (eb) v = avg + (b[i] - avg) * .6f;
                values[i] = (int)Math.Round(v);
            }
            uint hash = 2166136261;
            foreach (char c in Key(first, second)) { hash ^= c; hash *= 16777619; }
            int dial = (int)(hash % 4), spark = (int)(hash / 4 % 25) - 12;
            values[dial] += spark;
            return new PersonalityDials(values[0], values[1], values[2], values[3]);
        }

        /// <summary>What a fusion's personality would be: a named hybrid, or the temperament nearest its blended dials.</summary>
        public static void DescribeFusedPersonality(string first, string second, out string name, out string line)
        {
            var hybrid = HybridOf(TemperamentOf(first), TemperamentOf(second));
            if (hybrid != null) { name = hybrid.Name; line = "It's… " + hybrid.Phrase + "! " + hybrid.Line; return; }
            var temperament = Nearest(PreviewFusedDials(first, second), TemperamentOf(first));
            name = TemperamentName(temperament);
            line = "It's… " + temperamentPhrases[(int)temperament] + "! " + TemperamentLine(temperament);
        }
    }
}
