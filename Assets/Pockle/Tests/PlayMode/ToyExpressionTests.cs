using System;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;

namespace Pockle.Tests
{
    /// <summary>Speech-bubble expressions and warnings follow the personality system (docs/PERSONALITY_SYSTEM.md section 10).</summary>
    public sealed class ToyExpressionTests
    {
        private const string SkyMatte = "axolotl.matte-vinyl"; // Grumpy

        [Test]
        public void EveryToyHasShortExpressionsForEveryCueAndAFewWarnings()
        {
            foreach (var toy in ToyCatalog.Collectibles.Where(c => c.Available))
            {
                foreach (ReactionCue cue in Enum.GetValues(typeof(ReactionCue)))
                    foreach (bool night in new[] { false, true })
                    {
                        var candidates = ToyExpressions.Candidates(toy.Id, cue, night);
                        Assert.IsNotEmpty(candidates, toy.Id + " " + cue);
                        foreach (var c in candidates)
                        {
                            Assert.IsFalse(c.Key.IsEmpty, toy.Id + " " + cue);
                            Assert.LessOrEqual(c.Key.Sound.Length, 16, "Bubbles hold a sound, not a sentence: " + c.Key.Sound);
                        }
                    }
                var warnings = ToyExpressions.Warnings(toy.Id);
                Assert.That(warnings.Count, Is.InRange(2, 3), toy.Id);
                Assert.AreEqual(warnings.Count, warnings.Distinct().Count(), toy.Id);
            }
        }

        [Test]
        public void GrumpyGivesInOnTheThirdPokeInARow()
        {
            Assert.AreEqual(Temperament.Grumpy, PersonalityBook.TemperamentOf(SkyMatte));
            var picker = new ExpressionPicker(1);
            picker.Next(SkyMatte, ReactionCue.Poke, false);
            picker.Next(SkyMatte, ReactionCue.Poke, false);
            var third = picker.Next(SkyMatte, ReactionCue.Poke, false);
            Assert.AreEqual("…fine.", third.Sound);
            Assert.AreEqual(ExpressionSticker.Heart, third.Sticker);
            // Any other reaction starts the streak over.
            picker.Next(SkyMatte, ReactionCue.Poke, false);
            picker.Next(SkyMatte, ReactionCue.Squish, false);
            picker.Next(SkyMatte, ReactionCue.Poke, false);
            Assert.AreNotEqual("…fine.", picker.Next(SkyMatte, ReactionCue.Poke, false).Sound);
        }

        [Test]
        public void PickerNeverRepeatsItselfAndBigMomentsAlwaysSpeak()
        {
            var picker = new ExpressionPicker(42);
            string last = null;
            for (int i = 0; i < 200; i++)
            {
                var e = picker.Next(ToyCatalog.PeachId, i % 5 == 0 ? ReactionCue.Squish : ReactionCue.Poke, false);
                if (e.IsEmpty) continue;
                Assert.AreNotEqual(last, e.ToString(), "Never the same bubble twice in a row.");
                last = e.ToString();
            }
            for (int i = 0; i < 20; i++) Assert.IsFalse(picker.Next(ToyCatalog.MoonId, ReactionCue.PickUp, false).IsEmpty);
            // Bolder toys talk more than shy ones.
            Assert.Greater(ToyExpressions.Chattiness(ToyCatalog.GoldId, ReactionCue.Poke), ToyExpressions.Chattiness("fox.confetti-gel", ReactionCue.Poke));
        }

        [Test]
        public void TraitsAndTemperamentFlavourTheBubbles()
        {
            string Sounds(string id, ReactionCue cue, bool night = false) => string.Join(" ", ToyExpressions.Candidates(id, cue, night).Select(c => c.Key.Sound));
            StringAssert.Contains("hmph", Sounds(SkyMatte, ReactionCue.Poke));
            StringAssert.Contains("ta-da!", Sounds(ToyCatalog.GoldId, ReactionCue.Poke));
            StringAssert.Contains("boing!", Sounds("fox.matte-vinyl", ReactionCue.Poke), "Apricot's bouncy touch adds its own sound.");
            StringAssert.Contains("eep!", Sounds("fox.confetti-gel", ReactionCue.Squish));
            Assert.AreEqual(Temperament.Dreamy, PersonalityBook.TemperamentOf(ToyCatalog.MoonId));
            StringAssert.StartsWith("Daydreams", ToyExpressions.Warnings(ToyCatalog.MoonId)[0]);
            // The same seed gives the same bubbles, so a review build can be replayed.
            var a = new ExpressionPicker(9); var b = new ExpressionPicker(9);
            for (int i = 0; i < 10; i++) Assert.AreEqual(a.Next(ToyCatalog.PeachId, ReactionCue.Poke, false).ToString(), b.Next(ToyCatalog.PeachId, ReactionCue.Poke, false).ToString());
        }
    }
}
