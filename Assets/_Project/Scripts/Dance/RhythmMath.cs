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

        // ---------- Steps (in fast dances not every beat is a step) ----------

        /// <summary>Pressing faster than this feels like button mashing, not dancing.</summary>
        public const double DefaultMaxStepsPerMinute = 140.0;

        /// <summary>
        /// How many beats apart the steps are: 1 = a step on every beat.
        /// Quickstep (204 beats a minute) and Jive (172) are too fast for that, so you step on
        /// every 2nd beat. Viennese Waltz (177, in 3/4) steps once a bar, on "ONE".
        /// The interval always divides the bar, so "ONE" is always a step.
        /// </summary>
        public static int StepInterval(double beatsPerMinute, int beatsPerBar,
            double maxStepsPerMinute = DefaultMaxStepsPerMinute)
        {
            if (beatsPerBar < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(beatsPerBar));
            }

            if (beatsPerMinute <= 0 || maxStepsPerMinute <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(beatsPerMinute), "Tempos must be positive.");
            }

            for (int interval = 1; interval < beatsPerBar; interval++)
            {
                if (beatsPerBar % interval == 0 && beatsPerMinute / interval <= maxStepsPerMinute)
                {
                    return interval;
                }
            }

            return beatsPerBar; // One step per bar, on "ONE".
        }

        /// <summary>
        /// How many bars a routine of about targetSeconds lasts. Fast dances get more bars and
        /// slow ones fewer, so every dance takes roughly the same time.
        /// Waltz (29 bars a minute) for 24 seconds: 11.6, rounded to 12 bars.
        /// </summary>
        public static int RoutineBars(double targetSeconds, double barsPerMinute, int minBars = 4)
        {
            if (barsPerMinute <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(barsPerMinute));
            }

            int bars = (int)Math.Round(targetSeconds * barsPerMinute / 60.0, MidpointRounding.AwayFromZero);
            return Math.Max(minBars, bars);
        }

        /// <summary>
        /// Listening beats before the first step. Always whole bars (so the first step is a
        /// "ONE"), at least minBeats beats and at least minSeconds long, so fast dances
        /// don't rush you. Waltz: 1 bar (3 beats). Samba (2/4): 2 bars. Quickstep: 2 bars.
        /// </summary>
        public static int CountInBeats(int countInBars, int beatsPerBar, double beatsPerMinute,
            double minSeconds = 2.0, int minBeats = 3)
        {
            if (beatsPerBar < 1 || beatsPerMinute <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(beatsPerBar));
            }

            // The tiny subtraction stops 3.0000000001 from rounding up to an extra beat.
            int beatsForTime = (int)Math.Ceiling(minSeconds * beatsPerMinute / 60.0 - 1e-9);
            int neededBeats = Math.Max(minBeats, beatsForTime);
            int bars = Math.Max(Math.Max(1, countInBars), (neededBeats + beatsPerBar - 1) / beatsPerBar);
            return bars * beatsPerBar;
        }

        /// <summary>The beat index of step number <paramref name="step"/> (0 = the first step).</summary>
        public static int StepBeat(int step, int firstStepBeat, int stepInterval) =>
            firstStepBeat + step * stepInterval;

        /// <summary>True when a beat is one you should step on.</summary>
        public static bool IsStepBeat(int beatIndex, int firstStepBeat, int stepInterval) =>
            beatIndex >= firstStepBeat && (beatIndex - firstStepBeat) % stepInterval == 0;

        /// <summary>
        /// The step closest to a moment in the song. Negative before the first step.
        /// With a step on every 2nd beat, a press on the in-between beat is exactly halfway
        /// and rounds to the later step (so it is judged as far too early for that step).
        /// </summary>
        public static int NearestStep(double songSeconds, double secondsPerBeat, int firstStepBeat, int stepInterval)
        {
            if (stepInterval < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stepInterval));
            }

            double beatsAfterFirstStep = songSeconds / secondsPerBeat - firstStepBeat;
            return (int)Math.Round(beatsAfterFirstStep / stepInterval, MidpointRounding.AwayFromZero);
        }
    }
}
