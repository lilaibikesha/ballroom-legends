using System;

namespace BallroomLegends.Tournament
{
    /// <summary>
    /// The competition format, as pure rules.
    /// Real Blackpool events start with hundreds of couples and many rounds. The MVP keeps
    /// four rounds, and each round recalls half the floor: 48 -> 24 -> 12 -> Final of 6.
    /// A tiny field of 4 couples or fewer skips all that and dances only the Final.
    /// </summary>
    public static class TournamentRules
    {
        public const int DefaultFinalists = 6;

        /// <summary>
        /// Judges on the panel. Real Blackpool panels are bigger (often 9 or more), but 3
        /// keeps the MVP simple, and an odd number means a majority always exists, which the
        /// Skating System exercise needs.
        /// </summary>
        public const int DefaultNumberOfJudges = 3;

        /// <summary>With this many couples or fewer there is nobody to knock out: dance the Final.</summary>
        public const int MaxCouplesForStraightFinal = 4;

        /// <summary>Each judge gives a mark from 1 to 10.</summary>
        public const int MinMark = 1;
        public const int MaxMark = 10;

        /// <summary>Couples dancing in a round: Final 6, Semi-final 12, Quarter-final 24, Heat 48.</summary>
        public static int CouplesInRound(RoundType round, int finalists = DefaultFinalists)
        {
            if (finalists < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(finalists));
            }

            int roundsBeforeFinal = (int)RoundType.Final - (int)round;

            // Shifting left by n multiplies by 2^n: 6 << 3 == 48.
            return finalists << roundsBeforeFinal;
        }

        /// <summary>
        /// The round a tournament of this many couples starts with.
        /// 1 to 4 couples: straight to the Final. 5 or more: the First Round (Heat).
        /// </summary>
        public static RoundType FirstRound(int coupleCount)
        {
            // Guard clause: an empty tournament has no first round, so say so loudly.
            if (coupleCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(coupleCount), "A tournament needs at least one couple.");
            }

            return coupleCount <= MaxCouplesForStraightFinal ? RoundType.Final : RoundType.Heat;
        }

        public static bool IsLastRound(RoundType round) => round == RoundType.Final;

        public static RoundType Next(RoundType round)
        {
            if (IsLastRound(round))
            {
                throw new InvalidOperationException("There is no round after the Final.");
            }

            return round + 1;
        }

        public static string DisplayName(RoundType round)
        {
            return round switch
            {
                RoundType.Heat => "First Round",
                RoundType.QuarterFinal => "Quarter-final",
                RoundType.SemiFinal => "Semi-final",
                RoundType.Final => "Final",
                _ => round.ToString()
            };
        }
    }
}
