using System.Collections.Generic;
using BallroomLegends.Tournament;

namespace BallroomLegends.Competition
{
    /// <summary>
    /// Everything the Results screen needs about the competition that just ended.
    /// The ballroom scene fills it in and hands it to the GameManager, because the
    /// ballroom's objects are destroyed when the Results scene loads.
    /// </summary>
    public class CompetitionResult
    {
        public string DanceName = string.Empty;
        public string PlayerCoupleName = string.Empty;
        public int FieldSize;

        /// <summary>One report per round the player danced, in order.</summary>
        public List<RoundReport> Rounds = new List<RoundReport>();

        /// <summary>The top of the last round the player danced in, best first.</summary>
        public List<RankingEntry> LastRoundTop = new List<RankingEntry>();

        public RoundReport LastRound => Rounds.Count > 0 ? Rounds[Rounds.Count - 1] : null;
        public bool ReachedFinal => LastRound != null && LastRound.Round == RoundType.Final;
        public bool Won => ReachedFinal && LastRound.Place == 1;

        public string Headline()
        {
            RoundReport last = LastRound;
            if (last == null)
            {
                return "No competition danced yet";
            }

            if (Won)
            {
                return "Champions of Blackpool!";
            }

            if (ReachedFinal)
            {
                return $"{TextFormat.Ordinal(last.Place)} place in the Final";
            }

            return $"{TextFormat.Ordinal(last.Place)} of {last.CouplesInRound} in the {TournamentRules.DisplayName(last.Round)}";
        }
    }
}
