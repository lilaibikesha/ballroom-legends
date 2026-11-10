using System;
using UnityEngine;

namespace BallroomLegends.Dance
{
    /// <summary>
    /// Builds a click track in code, so the game has "music" before you add real audio.
    ///  - count-in bar: low clicks (660 Hz), so you can feel the tempo before dancing
    ///  - beat 1 of each bar: high click (1320 Hz) plus a soft bass note ("ONE")
    ///  - other beats: middle click (880 Hz) ("two, three")
    ///
    /// Sound is just numbers: a list of samples between -1 and 1, played back
    /// sampleRate times per second. Each click is a sine wave that fades out quickly.
    /// </summary>
    public static class MetronomeClip
    {
        private const float ClickSeconds = 0.06f;
        private const float BassSeconds = 0.35f;
        private const float TailSeconds = 1f;

        public static AudioClip Create(string name, double beatsPerMinute, int beatsPerBar,
            int countInBeats, int totalBeats, int sampleRate)
        {
            float[] samples = BuildSamples(beatsPerMinute, beatsPerBar, countInBeats, totalBeats, sampleRate);
            AudioClip clip = AudioClip.Create($"{name} click track", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>Pure part of <see cref="Create"/>: returns the mono samples. Unit tested.</summary>
        public static float[] BuildSamples(double beatsPerMinute, int beatsPerBar, int countInBeats,
            int totalBeats, int sampleRate)
        {
            if (beatsPerMinute <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(beatsPerMinute));
            }

            if (beatsPerBar < 1 || totalBeats < 1 || sampleRate < 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(beatsPerBar), "Need at least 1 beat per bar, 1 beat and a real sample rate.");
            }

            double secondsPerBeat = RhythmMath.SecondsPerBeat(beatsPerMinute);
            int length = (int)Math.Ceiling((totalBeats * secondsPerBeat + TailSeconds) * sampleRate);
            var samples = new float[length];

            for (int beat = 0; beat < totalBeats; beat++)
            {
                int start = (int)Math.Round(RhythmMath.BeatTime(beat, secondsPerBeat) * sampleRate);
                bool isCountIn = beat < countInBeats;
                bool isDownbeat = RhythmMath.BeatInBar(beat, beatsPerBar) == 0;

                double pitch = isCountIn ? 660.0 : isDownbeat ? 1320.0 : 880.0;
                float volume = isDownbeat ? 0.55f : 0.35f;
                AddTone(samples, start, ClickSeconds, pitch, volume, sampleRate, 40.0);

                if (isDownbeat && !isCountIn)
                {
                    AddTone(samples, start, BassSeconds, 110.0, 0.35f, sampleRate, 8.0);
                }
            }

            return samples;
        }

        /// <summary>Adds (mixes) a fading sine wave into the buffer.</summary>
        private static void AddTone(float[] samples, int start, float seconds, double frequency,
            float volume, int sampleRate, double fadeSpeed)
        {
            int count = (int)(seconds * sampleRate);
            for (int i = 0; i < count && start + i < samples.Length; i++)
            {
                double t = (double)i / sampleRate;
                double envelope = Math.Exp(-fadeSpeed * t);
                samples[start + i] += (float)(volume * envelope * Math.Sin(2.0 * Math.PI * frequency * t));
            }
        }
    }
}
