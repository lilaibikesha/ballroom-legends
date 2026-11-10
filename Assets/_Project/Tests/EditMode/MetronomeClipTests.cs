using System;
using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>The generated click track: right length, sound on the beats, silence between.</summary>
    public class MetronomeClipTests
    {
        private const int SampleRate = 48000;
        private const double Bpm = 120; // 0.5 s per beat, easy numbers

        [Test]
        public void Length_CoversEveryBeatPlusATail()
        {
            float[] samples = MetronomeClip.BuildSamples(Bpm, 3, 3, 12, SampleRate);

            Assert.GreaterOrEqual(samples.Length, 12 * 0.5 * SampleRate);
        }

        [Test]
        public void ThereIsSound_JustAfterEachBeat()
        {
            float[] samples = MetronomeClip.BuildSamples(Bpm, 3, 3, 6, SampleRate);

            for (int beat = 0; beat < 6; beat++)
            {
                int start = (int)(beat * 0.5 * SampleRate);
                Assert.Greater(MaxLoudness(samples, start, start + 200), 0.01f, $"No click on beat {beat}");
            }
        }

        [Test]
        public void ThereIsSilence_BetweenTwoWeakBeats()
        {
            // Beats 4 and 5 are "two" and "three" of the second bar: clicks only, no bass.
            float[] samples = MetronomeClip.BuildSamples(Bpm, 3, 3, 6, SampleRate);
            int middle = (int)(4.5 * 0.5 * SampleRate);

            Assert.AreEqual(0f, MaxLoudness(samples, middle - 100, middle + 100));
        }

        [Test]
        public void Samples_StayBetweenMinusOneAndOne()
        {
            float[] samples = MetronomeClip.BuildSamples(Bpm, 3, 3, 12, SampleRate);

            Assert.LessOrEqual(MaxLoudness(samples, 0, samples.Length), 1f);
        }

        [Test]
        public void ZeroTempo_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MetronomeClip.BuildSamples(0, 3, 3, 6, SampleRate));
        }

        private static float MaxLoudness(float[] samples, int from, int to)
        {
            float max = 0f;
            for (int i = Math.Max(0, from); i < to && i < samples.Length; i++)
            {
                max = Math.Max(max, Math.Abs(samples[i]));
            }

            return max;
        }
    }
}
