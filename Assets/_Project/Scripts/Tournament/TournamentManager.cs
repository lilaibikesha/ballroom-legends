using System.Collections.Generic;
using UnityEngine;

namespace BallroomLegends.Tournament
{
    /// <summary>
    /// Runs the competition: registers couples, collects judges' scores for each round,
    /// ranks couples and decides who advances.
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        [SerializeField, Min(1)] private int numberOfJudges = 5;
        [SerializeField, Min(1)] private int couplesAdvancingPerRound = 6;

        private readonly List<Couple> _couples = new();

        // Scores for the current round: each couple -> one score per judge.
        private readonly Dictionary<Couple, List<int>> _scores = new();

        public RoundType CurrentRound { get; private set; } = RoundType.Heat;
        public IReadOnlyList<Couple> Couples => _couples;
        public int NumberOfJudges => numberOfJudges;
        public int CouplesAdvancingPerRound => couplesAdvancingPerRound;
        public int ScoredCoupleCount => _scores.Count;

        public void RegisterCouple(Couple couple)
        {
            // TODO (you): add the couple, but guard against null and duplicates.
        }

        public void SubmitScores(Couple couple, IReadOnlyList<int> judgeScores)
        {
            // TODO (you): validate (registered couple? exactly numberOfJudges scores?)
            // then store them in _scores.
        }

        public List<Couple> GetRanking()
        {
            // TODO (you): return couples ordered best-first.
            // Start simple (total or average score). Then think about ties.
            // Stretch goal: real ballroom uses the "Skating System" (judges give
            // placements, majority rules). It's a great algorithms exercise.
            return new List<Couple>(_couples);
        }

        public void AdvanceToNextRound()
        {
            // TODO (you): keep the top couplesAdvancingPerRound couples, clear _scores,
            // and move CurrentRound forward. What happens after the Final?
            Debug.Log($"[TournamentManager] AdvanceToNextRound from {CurrentRound} is not implemented yet.");
        }
    }
}
