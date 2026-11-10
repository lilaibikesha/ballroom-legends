using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class RhythmMathTests
    {
        [TestCase("3/4", 3)]
        [TestCase("2/4", 2)]
        [TestCase("4/4", 4)]
        [TestCase(" 3 / 4 ", 3)]
        [TestCase("", 4)]
        [TestCase(null, 4)]
        [TestCase("waltz", 4)]
        [TestCase("0/4", 4)]
        public void BeatsPerBar_ReadsTheTimeSignature(string signature, int expected)
        {
            Assert.AreEqual(expected, RhythmMath.BeatsPerBar(signature));
        }

        [Test]
        public void Waltz_29BarsPerMinute_Is87Bpm()
        {
            Assert.AreEqual(87.0, RhythmMath.BeatsPerMinute(29, 3), 1e-9);
        }

        [Test]
        public void SecondsPerBeat_At120Bpm_IsHalfASecond()
        {
            Assert.AreEqual(0.5, RhythmMath.SecondsPerBeat(120), 1e-9);
        }

        [TestCase(0.0, 0)]
        [TestCase(0.24, 0)]
        [TestCase(0.26, 1)]
        [TestCase(0.25, 1)] // exactly halfway rounds away from zero
        [TestCase(1.1, 2)]
        [TestCase(-0.4, -1)]
        public void NearestBeat_At120Bpm(double songSeconds, int expected)
        {
            Assert.AreEqual(expected, RhythmMath.NearestBeat(songSeconds, 0.5));
        }

        [Test]
        public void OffsetFromBeat_EarlyIsNegative_LateIsPositive()
        {
            Assert.AreEqual(-0.1, RhythmMath.OffsetFromBeat(0.9, 2, 0.5), 1e-9);
            Assert.AreEqual(0.05, RhythmMath.OffsetFromBeat(1.05, 2, 0.5), 1e-9);
        }

        [TestCase(0, 3, 0)]
        [TestCase(1, 3, 1)]
        [TestCase(3, 3, 0)]
        [TestCase(7, 3, 1)]
        [TestCase(-1, 3, 2)]
        [TestCase(5, 4, 1)]
        public void BeatInBar_WrapsEveryBar(int beat, int beatsPerBar, int expected)
        {
            Assert.AreEqual(expected, RhythmMath.BeatInBar(beat, beatsPerBar));
        }
    }
}
