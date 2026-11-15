using System;
using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>
    /// Which beats are steps, how long a routine is, and how long the count-in is.
    /// Tempos are the real ones from the dance assets (bars per minute x beats per bar).
    /// </summary>
    public class StepPatternTests
    {
        [TestCase(87, 3, 1, TestName = "Waltz (87 BPM): every beat")]
        [TestCase(64, 2, 1, TestName = "Tango (64 BPM): every beat")]
        [TestCase(116, 4, 1, TestName = "Slow Foxtrot (116 BPM): every beat")]
        [TestCase(204, 4, 2, TestName = "Quickstep (204 BPM): every 2nd beat")]
        [TestCase(172, 4, 2, TestName = "Jive (172 BPM): every 2nd beat")]
        [TestCase(177, 3, 3, TestName = "Viennese Waltz (177 BPM, 3/4): once a bar")]
        [TestCase(122, 2, 1, TestName = "Paso Doble (122 BPM): every beat")]
        public void StepInterval_SlowsDownFastDances(double bpm, int beatsPerBar, int expected)
        {
            Assert.AreEqual(expected, RhythmMath.StepInterval(bpm, beatsPerBar));
        }

        [Test]
        public void StepInterval_AlwaysDividesTheBar_SoOneIsAStep()
        {
            for (int beatsPerBar = 1; beatsPerBar <= 6; beatsPerBar++)
            {
                for (int bpm = 40; bpm <= 400; bpm += 7)
                {
                    int interval = RhythmMath.StepInterval(bpm, beatsPerBar);
                    Assert.AreEqual(0, beatsPerBar % interval, $"{bpm} BPM in {beatsPerBar}/4");
                }
            }
        }

        [Test]
        public void StepInterval_BadInput_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RhythmMath.StepInterval(0, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => RhythmMath.StepInterval(120, 0));
        }

        [TestCase(24, 29, 12, TestName = "Waltz: 12 bars")]
        [TestCase(24, 59, 24, TestName = "Viennese Waltz: 24 bars")]
        [TestCase(24, 51, 20, TestName = "Quickstep: 20 bars")]
        [TestCase(1, 29, 4, TestName = "Never shorter than 4 bars")]
        public void RoutineBars_TakeAboutTheSameTimeForEveryDance(double seconds, double barsPerMinute, int expected)
        {
            Assert.AreEqual(expected, RhythmMath.RoutineBars(seconds, barsPerMinute));
        }

        [TestCase(1, 3, 87, 3, TestName = "Waltz: one bar")]
        [TestCase(1, 2, 102, 4, TestName = "Samba: two bars of 2/4")]
        [TestCase(1, 4, 204, 8, TestName = "Quickstep: two bars, so you get 2 seconds to listen")]
        [TestCase(1, 3, 177, 6, TestName = "Viennese Waltz: two bars")]
        [TestCase(2, 3, 87, 6, TestName = "Asking for more bars gives more bars")]
        public void CountInBeats_IsWholeBars_AtLeastThreeBeatsAndTwoSeconds(int bars, int beatsPerBar, double bpm, int expected)
        {
            Assert.AreEqual(expected, RhythmMath.CountInBeats(bars, beatsPerBar, bpm));
        }

        [Test]
        public void StepBeats_EverySecondBeat_AfterTheCountIn()
        {
            // Count-in of 8 beats, a step every 2 beats: steps on 8, 10, 12, ...
            Assert.AreEqual(8, RhythmMath.StepBeat(0, 8, 2));
            Assert.AreEqual(12, RhythmMath.StepBeat(2, 8, 2));

            Assert.IsFalse(RhythmMath.IsStepBeat(6, 8, 2), "count-in");
            Assert.IsTrue(RhythmMath.IsStepBeat(8, 8, 2));
            Assert.IsFalse(RhythmMath.IsStepBeat(9, 8, 2), "in-between beat");
            Assert.IsTrue(RhythmMath.IsStepBeat(10, 8, 2));
        }

        [Test]
        public void NearestStep_FindsTheClosestStep()
        {
            const double spb = 0.5; // 120 BPM
            // First step on beat 8 (4.0 s), then every 2 beats (every 1.0 s).
            Assert.AreEqual(0, RhythmMath.NearestStep(4.04, spb, 8, 2), "just late for step 0");
            Assert.AreEqual(1, RhythmMath.NearestStep(4.97, spb, 8, 2), "just early for step 1");
            Assert.AreEqual(1, RhythmMath.NearestStep(4.5, spb, 8, 2), "halfway goes to the later step");
            Assert.Less(RhythmMath.NearestStep(3.0, spb, 8, 2), 0, "during the count-in");
        }

        [Test]
        public void ClickTrack_BeatsWithoutAStep_AreQuieter()
        {
            const int sampleRate = 48000;
            // 120 BPM (0.5 s a beat), 4/4, one count-in bar, a step on every 2nd beat.
            float[] samples = MetronomeClip.BuildSamples(120, 4, 4, 12, sampleRate, stepInterval: 2);

            float step = Loudness(samples, 6 * 0.5, sampleRate);    // beat 6 = "three": a step
            float noStep = Loudness(samples, 7 * 0.5, sampleRate);  // beat 7 = "four": no step

            Assert.Greater(noStep, 0.01f, "You still hear the in-between beat");
            Assert.Greater(step, noStep * 2f, "...but much softer than a step");
        }

        private static float Loudness(float[] samples, double seconds, int sampleRate)
        {
            int start = (int)(seconds * sampleRate);
            float max = 0f;
            for (int i = start; i < start + 200 && i < samples.Length; i++)
            {
                max = Math.Max(max, Math.Abs(samples[i]));
            }

            return max;
        }
    }
}
