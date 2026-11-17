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
    /// With 4 couples or fewer there's no one to knock out, so the first round is the Final
    /// (<see cref="TournamentRules.FirstRound"/>).
    ///
    /// Like the real Blackpool Festival, a round can have several dances (all five of the
    /// style). Each dance adds one mark per judge with <see cref="AddDanceScores"/>, and the
    /// ranking uses the total over every dance of the round.
    ///
    /// The ranking rules themselves live in <see cref="TournamentRanking"/>, a pure static
    /// class: this MonoBehaviour only stores the marks and asks it for the order.
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        [Tooltip("Marks per couple per dance. Changing it here only affects new components: an existing scene keeps its saved value.")]
        [SerializeField, Min(1)] private int numberOfJudges = TournamentRules.DefaultNumberOfJudges;
        [Tooltip("Couples in the Final. Each earlier round has twice as many (6, 12, 24, 48).")]
        [SerializeField, Min(1)] private int finalists = TournamentRules.DefaultFinalists;

        // In start-number order (the order they registered).
        private readonly List<Couple> _couples = new();

        // Marks for the current round: each couple -> one mark per judge, for every dance
        // danced so far (3 judges x 5 dances = 15 marks).
        private readonly Dictionary<Couple, List<int>> _scores = new();

        // How many dances of the current round each couple has been marked for.
        private readonly Dictionary<Couple, int> _dancesMarked = new();

        /// <summary>
        /// The round being danced. While couples register it follows the field size
        /// (4 or fewer: Final), and it's fixed once registration closes.
        /// </summary>
        public RoundType CurrentRound { get; private set; } = RoundType.Heat;
        public bool IsFinished { get; private set; }

        /// <summary>Open until the first marks arrive (or the tournament moves on).</summary>
        public bool IsRegistrationOpen { get; private set; } = true;
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
            _dancesMarked.Clear();
            CurrentRound = RoundType.Heat;
            IsFinished = false;
            IsRegistrationOpen = true;
        }

        /// <summary>
        /// Adds a couple to the start list. Returns false (and changes nothing) for null, a
        /// couple that's already registered, or once registration has closed.
        /// Returning a bool instead of throwing lets the caller decide what a refusal means.
        /// </summary>
        public bool RegisterCouple(Couple couple)
        {
            // Guard clauses: check every way the input can be wrong first, then do the work.
            if (couple == null)
            {
                Debug.LogWarning("[Tournament] Can't register a null couple.");
                return false;
            }

            if (!IsRegistrationOpen)
            {
                Debug.LogWarning($"[Tournament] Registration is closed. {couple} was not added.");
                return false;
            }

            // Contains compares references: two different Couple objects with the same name
            // are two couples (like two real couples who happen to share a name).
            if (_couples.Contains(couple))
            {
                Debug.LogWarning($"[Tournament] {couple} is already registered.");
                return false;
            }

            _couples.Add(couple);

            // The field size decides the first round, so update it after every new couple.
            CurrentRound = TournamentRules.FirstRound(_couples.Count);
            return true;
        }

        /// <summary>
        /// Stores one mark per judge for a couple, for a one-dance round. Submitting again
        /// replaces the old marks (including marks from other dances).
        /// </summary>
        public bool SubmitScores(Couple couple, IReadOnlyList<int> judgeScores)
        {
            if (!AreValidMarks(couple, judgeScores))
            {
                return false;
            }

            // Store a copy, so the caller changing its list later can't change our data.
            _scores[couple] = new List<int>(judgeScores);
            _dancesMarked[couple] = 1;

            // Dancing has started: from now on the start list (and so the round) is fixed.
            IsRegistrationOpen = false;
            return true;
        }

        /// <summary>
        /// Adds the judges' marks for one more dance of the round (Waltz, then Tango, ...).
        /// The couple's total grows with every dance.
        /// </summary>
        public bool AddDanceScores(Couple couple, IReadOnlyList<int> judgeScores)
        {
            if (!AreValidMarks(couple, judgeScores))
            {
                return false;
            }

            if (_scores.TryGetValue(couple, out List<int> marks))
            {
                marks.AddRange(judgeScores);
                _dancesMarked[couple]++;
            }
            else
            {
                _scores[couple] = new List<int>(judgeScores);
                _dancesMarked[couple] = 1;
            }

            IsRegistrationOpen = false;
            return true;
        }

        /// <summary>How many dances of the current round this couple has marks for.</summary>
        public int GetDancesMarked(Couple couple) =>
            couple != null && _dancesMarked.TryGetValue(couple, out int count) ? count : 0;

        /// <summary>Guard clauses shared by SubmitScores and AddDanceScores.</summary>
        private bool AreValidMarks(Couple couple, IReadOnlyList<int> judgeScores)
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

            return true;
        }

        public int GetTotal(Couple couple) =>
            couple != null && _scores.TryGetValue(couple, out List<int> marks) ? TournamentRanking.Total(marks) : 0;

        public IReadOnlyList<int> GetScores(Couple couple) =>
            couple != null && _scores.TryGetValue(couple, out List<int> marks)
                ? marks
                : (IReadOnlyList<int>)Array.Empty<int>();

        /// <summary>
        /// Couples of the current round, best first: highest total, then the highest single
        /// mark, then the name A to Z. The rules live in <see cref="TournamentRanking.Rank"/>
        /// so they can be tested without a GameObject.
        /// </summary>
        public List<Couple> GetRanking() => TournamentRanking.Rank(_couples, _scores);

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

            // Moving on (or finishing) always closes registration, even if nobody was marked.
            IsRegistrationOpen = false;

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
            _dancesMarked.Clear();
            CurrentRound = TournamentRules.Next(CurrentRound);
            Debug.Log($"[Tournament] {_couples.Count} couples recalled to the {CurrentRound}.");
        }
    }
}
