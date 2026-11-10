using System;

namespace BallroomLegends.Tournament
{
    /// <summary>
    /// The competition format, as pure rules.
    /// Real Blackpool events start with hundreds of couples and many rounds. The MVP keeps
    /// four rounds, and each round recalls half the floor: 48 -> 24 -> 12 -> Final of 6.
    /// </summary>
    public static class TournamentRules
    {
        public const int DefaultFinalists = 6;

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
