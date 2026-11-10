using System;

namespace BallroomLegends.Dance
{
    /// <summary>
    /// The arithmetic of keeping time. Pure functions only (no Unity calls), so every rule
    /// here is unit tested in Tests/EditMode/RhythmMathTests.cs.
    ///
    /// Vocabulary:
    ///  - song time: seconds since the first click of the track (beat 0)
    ///  - beat index: 0, 1, 2, ... counted from the start of the track
    ///  - beat in bar: 0 for the strong first beat ("ONE"), then 1, 2, ...
    /// </summary>
    public static class RhythmMath
    {
        public const int DefaultBeatsPerBar = 4;

        /// <summary>"3/4" gives 3 beats per bar. Anything unreadable falls back to 4.</summary>
        public static int BeatsPerBar(string timeSignature)
        {
            if (string.IsNullOrWhiteSpace(timeSignature))
            {
                return DefaultBeatsPerBar;
            }

            string[] parts = timeSignature.Split('/');
            if (parts.Length != 2)
            {
                return DefaultBeatsPerBar;
            }

            return int.TryParse(parts[0].Trim(), out int beats) && beats > 0 ? beats : DefaultBeatsPerBar;
        }

        /// <summary>
        /// DanceSport tempos are given in bars per minute (Waltz: 28-30).
        /// Waltz at 29 bars x 3 beats = 87 beats per minute.
        /// </summary>
        public static double BeatsPerMinute(double barsPerMinute, int beatsPerBar) => barsPerMinute * beatsPerBar;

        public static double SecondsPerBeat(double beatsPerMinute) => 60.0 / beatsPerMinute;

        /// <summary>When beat number <paramref name="beatIndex"/> happens, in song time.</summary>
        public static double BeatTime(int beatIndex, double secondsPerBeat) => beatIndex * secondsPerBeat;

        /// <summary>The beat closest to a moment in the song (can be negative before the song starts).</summary>
        public static int NearestBeat(double songSeconds, double secondsPerBeat) =>
            (int)Math.Round(songSeconds / secondsPerBeat, MidpointRounding.AwayFromZero);

        /// <summary>How far a moment is from a beat. Negative = early, positive = late.</summary>
        public static double OffsetFromBeat(double songSeconds, int beatIndex, double secondsPerBeat) =>
            songSeconds - BeatTime(beatIndex, secondsPerBeat);

        /// <summary>
        /// Position inside the bar: 0 is the strong "ONE". Uses a true modulo so it also
        /// works for negative beats (C#'s % keeps the sign: -1 % 3 == -1).
        /// </summary>
        public static int BeatInBar(int beatIndex, int beatsPerBar)
        {
            int remainder = beatIndex % beatsPerBar;
            return remainder < 0 ? remainder + beatsPerBar : remainder;
        }
    }
}
