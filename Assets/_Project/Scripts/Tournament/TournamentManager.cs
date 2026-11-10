using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BallroomLegends.Tournament
{
    /// <summary>
    /// Runs the competition: registers couples, collects judges' marks for each round,
    /// ranks couples and recalls the best ones to the next round.
    ///
    /// Round sizes follow <see cref="TournamentRules.CouplesInRound"/>: 48, 24, 12, Final of 6.
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        [SerializeField, Min(1)] private int numberOfJudges = 5;
        [Tooltip("Couples in the Final. Each earlier round has twice as many (6, 12, 24, 48).")]
        [SerializeField, Min(1)] private int finalists = TournamentRules.DefaultFinalists;

        // In start-number order (the order they registered). Ties are broken by this order.
        private readonly List<Couple> _couples = new();

        // Marks for the current round: each couple -> one mark per judge.
        private readonly Dictionary<Couple, List<int>> _scores = new();

        public RoundType CurrentRound { get; private set; } = RoundType.Heat;
        public bool IsFinished { get; private set; }
        public IReadOnlyList<Couple> Couples => _couples;
        public int NumberOfJudges => numberOfJudges;
        public int Finalists => finalists;
        public int ScoredCoupleCount => _scores.Count;

        /// <summary>How many couples the first round holds (48 by default).</summary>
        public int FieldSize => TournamentRules.CouplesInRound(RoundType.Heat, finalists);

        /// <summary>How many couples go through to the next round (0 in the Final).</summary>
        public int CouplesAdvancingFromCurrentRound =>
            TournamentRules.IsLastRound(CurrentRound)
                ? 0
                : Math.Min(_couples.Count, TournamentRules.CouplesInRound(TournamentRules.Next(CurrentRound), finalists));

        /// <summary>Empties the floor and goes back to the first round.</summary>
        public void ResetTournament()
        {
            _couples.Clear();
            _scores.Clear();
            CurrentRound = RoundType.Heat;
            IsFinished = false;
        }

        public bool RegisterCouple(Couple couple)
        {
            if (couple == null)
            {
                Debug.LogWarning("[Tournament] Can't register a null couple.");
                return false;
            }

            if (CurrentRound != RoundType.Heat || _scores.Count > 0)
            {
                Debug.LogWarning($"[Tournament] Registration is closed. {couple} was not added.");
                return false;
            }

            if (_couples.Contains(couple))
            {
                Debug.LogWarning($"[Tournament] {couple} is already registered.");
                return false;
            }

            _couples.Add(couple);
            return true;
        }

        /// <summary>Stores one mark per judge for a couple. Submitting again replaces the old marks.</summary>
        public bool SubmitScores(Couple couple, IReadOnlyList<int> judgeScores)
        {
            if (IsFinished)
            {
                Debug.LogWarning("[Tournament] The tournament is over.");
                return false;
            }

            if (couple == null || !_couples.Contains(couple))
            {
                Debug.LogWarning($"[Tournament] {couple} isn't dancing in the {CurrentRound}.");
                return false;
            }

            if (judgeScores == null || judgeScores.Count != numberOfJudges)
            {
                Debug.LogWarning($"[Tournament] {couple}: expected {numberOfJudges} marks.");
                return false;
            }

            foreach (int mark in judgeScores)
            {
                if (mark < TournamentRules.MinMark || mark > TournamentRules.MaxMark)
                {
                    Debug.LogWarning($"[Tournament] {couple}: mark {mark} is outside {TournamentRules.MinMark}-{TournamentRules.MaxMark}.");
                    return false;
                }
            }

            // Store a copy, so the caller changing its list later can't change our data.
            _scores[couple] = new List<int>(judgeScores);
            return true;
        }

        public int GetTotal(Couple couple) =>
            couple != null && _scores.TryGetValue(couple, out List<int> marks) ? marks.Sum() : 0;

        public IReadOnlyList<int> GetScores(Couple couple) =>
            couple != null && _scores.TryGetValue(couple, out List<int> marks)
                ? marks
                : (IReadOnlyList<int>)Array.Empty<int>();

        /// <summary>
        /// Couples of the current round, best first:
        ///  1. couples with marks before couples without
        ///  2. higher total of all judges' marks
        ///  3. tie? the higher single best mark
        ///  4. still tied? the lower start number (LINQ's OrderBy is stable, so the original
        ///     registration order survives as the last tie-breaker)
        /// Stretch goal: real DanceSport uses the Skating System (judges give placements and
        /// the majority decides). See docs/COMPETITION.md.
        /// </summary>
        public List<Couple> GetRanking()
        {
            return _couples
                .OrderByDescending(couple => _scores.ContainsKey(couple))
                .ThenByDescending(GetTotal)
                .ThenByDescending(BestMark)
                .ToList();
        }

        /// <summary>1 = winner of the round. -1 if the couple isn't in this round.</summary>
        public int GetPlace(Couple couple)
        {
            int index = GetRanking().IndexOf(couple);
            return index < 0 ? -1 : index + 1;
        }

        /// <summary>
        /// Recalls the best couples to the next round and clears the marks.
        /// After the Final it only marks the tournament as finished, so the final marks and
        /// ranking stay readable.
        /// </summary>
        public void AdvanceToNextRound()
        {
            if (IsFinished)
            {
                Debug.LogWarning("[Tournament] Already finished.");
                return;
            }

            if (TournamentRules.IsLastRound(CurrentRound))
            {
                IsFinished = true;
                Debug.Log("[Tournament] The Final is over.");
                return;
            }

            if (_scores.Count < _couples.Count)
            {
                Debug.LogWarning($"[Tournament] Only {_scores.Count} of {_couples.Count} couples have marks. The rest rank last.");
            }

            int recalledCount = CouplesAdvancingFromCurrentRound;
            var recalled = new HashSet<Couple>(GetRanking().Take(recalledCount));

            // RemoveAll keeps the survivors in start-number order.
            _couples.RemoveAll(couple => !recalled.Contains(couple));
            _scores.Clear();
            CurrentRound = TournamentRules.Next(CurrentRound);
            Debug.Log($"[Tournament] {_couples.Count} couples recalled to the {CurrentRound}.");
        }

        private int BestMark(Couple couple) =>
            _scores.TryGetValue(couple, out List<int> marks) && marks.Count > 0 ? marks.Max() : 0;
    }
}
