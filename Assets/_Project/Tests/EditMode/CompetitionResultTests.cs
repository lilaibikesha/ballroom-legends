using BallroomLegends.Competition;
using BallroomLegends.Tournament;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class CompetitionResultTests
    {
        [TestCase(1, "1st")]
        [TestCase(2, "2nd")]
        [TestCase(3, "3rd")]
        [TestCase(4, "4th")]
        [TestCase(11, "11th")]
        [TestCase(12, "12th")]
        [TestCase(13, "13th")]
        [TestCase(21, "21st")]
        [TestCase(22, "22nd")]
        [TestCase(111, "111th")]
        public void Ordinal(int number, string expected)
        {
            Assert.AreEqual(expected, TextFormat.Ordinal(number));
        }

        [Test]
        public void Headline_NoRounds()
        {
            Assert.AreEqual("No competition danced yet", new CompetitionResult().Headline());
        }

        [Test]
        public void Headline_WinningTheFinal()
        {
            CompetitionResult result = ResultEndingIn(RoundType.Final, place: 1, couples: 6);

            Assert.IsTrue(result.Won);
            Assert.AreEqual("Champions of Blackpool!", result.Headline());
        }

        [Test]
        public void Headline_PlacedInTheFinal()
        {
            CompetitionResult result = ResultEndingIn(RoundType.Final, place: 3, couples: 6);

            Assert.IsFalse(result.Won);
            Assert.AreEqual("3rd place in the Final", result.Headline());
        }

        [Test]
        public void Headline_OutBeforeTheFinal()
        {
            CompetitionResult result = ResultEndingIn(RoundType.SemiFinal, place: 9, couples: 12);

            Assert.AreEqual("9th of 12 in the Semi-final", result.Headline());
        }

        [Test]
        public void TotalMarks_AddsTheJudgesMarks()
        {
            var report = new RoundReport { Marks = new[] { 8, 9, 7, 9, 8 } };

            Assert.AreEqual(41, report.TotalMarks);
        }

        private static CompetitionResult ResultEndingIn(RoundType round, int place, int couples)
        {
            var result = new CompetitionResult();
            result.Rounds.Add(new RoundReport { Round = round, Place = place, CouplesInRound = couples });
            return result;
        }
    }
}
