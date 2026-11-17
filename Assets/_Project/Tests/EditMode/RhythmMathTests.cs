using System;
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

            // ---------- The beat clock: song seconds -> beats ----------
        // 120 BPM keeps the numbers easy: one beat every 0.5 s.

        [TestCase(0.0, 0.0)]
        [TestCase(0.5, 1.0)]
        [TestCase(1.25, 2.5)] // halfway between beat 2 and beat 3
        [TestCase(-0.5, -1.0)] // the lead-in, before the first click
        public void SongPositionInBeats_At120Bpm(double songSeconds, double expectedBeats)
        {
            Assert.AreEqual(expectedBeats, RhythmMath.SongPositionInBeats(songSeconds, 120f), 1e-9);
        }

        [Test]
        public void SongPositionInBeats_OneMinuteOfWaltz_Is87Beats()
        {
            Assert.AreEqual(87.0, RhythmMath.SongPositionInBeats(60.0, 87f), 1e-9);
        }

        [TestCase(0f)]
        [TestCase(-120f)]
        public void BeatClock_RejectsATempoThatIsNotPositive(float bpm)
        {
            // A tempo of 0 would mean dividing by zero later on. Fail loudly instead.
            Assert.Throws<ArgumentOutOfRangeException>(() => RhythmMath.SongPositionInBeats(1.0, bpm));
            Assert.Throws<ArgumentOutOfRangeException>(() => RhythmMath.SecondsFromNearestBeat(1.0, bpm));
        }

        [TestCase(1.0, 0.0)] // exactly on beat 2
        [TestCase(1.03, 0.03)] // a little late for beat 2
        [TestCase(0.97, -0.03)] // a little early for beat 2
        [TestCase(0.2, 0.2)] // nearer beat 0 (late) than beat 1
        [TestCase(-0.1, -0.1)] // just before the first click: early for beat 0
        public void SecondsFromNearestBeat_At120Bpm(double songSeconds, double expectedOffset)
        {
            Assert.AreEqual(expectedOffset, RhythmMath.SecondsFromNearestBeat(songSeconds, 120f), 1e-9);
        }

        [Test]
        public void SecondsFromNearestBeat_ExactlyHalfway_CountsAsEarlyForTheNextBeat()
        {
            // 0.25 s is exactly between beat 0 and beat 1. Like NearestBeat, we round away
            // from zero, so it belongs to beat 1 and is 0.25 s early.
            Assert.AreEqual(-0.25, RhythmMath.SecondsFromNearestBeat(0.25, 120f), 1e-9);
        }

        [Test]
        public void SecondsFromNearestBeat_StaysPreciseAfterALongSession()
        {
            // dspTime counts from when the audio system started, so after a day it is
            // around 86,400 s. A float only has about 7 digits: at that size it can't see
            // anything smaller than about 8 ms. A double keeps the 30 ms here exact enough.
            double aDayLater = 86_400.0 + 0.03;

            Assert.AreEqual(0.03, RhythmMath.SecondsFromNearestBeat(aDayLater, 120f), 1e-6);
        }

        [Test]
        public void SecondsFromNearestBeat_MatchesTheStepJudge_WhenEveryBeatIsAStep()
        {
            // DanceManager judges a press against the nearest STEP (fast dances skip beats).
            // When every beat is a step (interval 1) both ways must agree.
            const float bpm = 87f;
            double secondsPerBeat = RhythmMath.SecondsPerBeat(bpm);
            const int firstStepBeat = 3; // after a one-bar Waltz count-in

            for (double songSeconds = 2.5; songSeconds < 10.0; songSeconds += 0.037)
            {
                int step = RhythmMath.NearestStep(songSeconds, secondsPerBeat, firstStepBeat, 1);
                int beat = RhythmMath.StepBeat(step, firstStepBeat, 1);
                double stepOffset = RhythmMath.OffsetFromBeat(songSeconds, beat, secondsPerBeat);

                Assert.AreEqual(stepOffset, RhythmMath.SecondsFromNearestBeat(songSeconds, bpm), 1e-9,
                    $"at {songSeconds:0.000} s");
            }
        }
    }
}
