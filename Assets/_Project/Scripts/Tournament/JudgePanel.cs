using System;

namespace BallroomLegends.Tournament
{
    /// <summary>
    /// Turns how well a couple danced (skill 0-100) into one mark per judge (1-10).
    /// Judges roughly agree but not exactly: each mark is skill / 10 plus a little random
    /// disagreement, rounded and kept inside 1-10.
    ///
    /// The randomness comes in through the constructor (dependency injection). Tests pass
    /// a seeded System.Random, so the "random" marks are the same on every run.
    /// </summary>
    public class JudgePanel
    {
        private readonly Random _random;
        private readonly double _disagreement;

        /// <param name="disagreement">Standard deviation of a judge's opinion, in marks. 0 = all judges agree.</param>
        public JudgePanel(Random random, double disagreement = 0.8)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            if (disagreement < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(disagreement));
            }

            _disagreement = disagreement;
        }

        public int[] Score(float skill, int numberOfJudges)
        {
            if (numberOfJudges < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfJudges));
            }

            var marks = new int[numberOfJudges];
            for (int i = 0; i < marks.Length; i++)
            {
                double opinion = skill / 10.0 + RandomUtil.NextGaussian(_random) * _disagreement;
                marks[i] = ClampMark((int)Math.Round(opinion, MidpointRounding.AwayFromZero));
            }

            return marks;
        }

        public static int ClampMark(int mark) =>
            Math.Max(TournamentRules.MinMark, Math.Min(TournamentRules.MaxMark, mark));
    }
}
