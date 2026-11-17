using System;
using System.Collections.Generic;
using System.Linq;

namespace BallroomLegends.Tournament
{
    /// <summary>
    /// Puts couples in order, best first. Pure functions only: no Unity calls and no
    /// state, so every rule is unit tested in Tests/EditMode/TournamentRankingTests.cs.
    /// TournamentManager keeps the marks and calls this class to rank them.
    ///
    /// The rules, in order. Each one only matters when the ones before it are a tie:
    ///  1. The higher total of all the judges' marks (over every dance of the round).
    ///  2. The higher single best mark: one judge who loved you beats a flat "fine".
    ///  3. The name, A to Z (ignoring upper and lower case), so the order never depends
    ///     on luck or on who registered first.
    /// A couple with no marks has a total of 0, and every real mark is at least 1, so it
    /// always ranks below the couples that danced.
    ///
    /// Stretch goal: real DanceSport uses the Skating System (judges give placements and
    /// the majority decides). See docs/COMPETITION.md.
    /// </summary>
    public static class TournamentRanking
    {
        /// <summary>
        /// Returns a NEW list, best first. The input is left alone (the caller may need
        /// its original order, e.g. start numbers).
        /// </summary>
        /// <param name="couples">Who is in the round.</param>
        /// <param name="marksByCouple">Each couple's judges' marks. A couple missing from it has no marks yet.</param>
        public static List<Couple> Rank(IEnumerable<Couple> couples,
            IReadOnlyDictionary<Couple, List<int>> marksByCouple)
        {
            if (couples == null)
            {
                throw new ArgumentNullException(nameof(couples));
            }

            // OrderBy... sorts by the first rule; ThenBy... is only used to break ties left
            // by the rules before it. That chain reads exactly like the rules above.
            return couples
                .Where(couple => couple != null)
                .OrderByDescending(couple => Total(MarksOf(couple, marksByCouple)))
                .ThenByDescending(couple => BestMark(MarksOf(couple, marksByCouple)))
                .ThenBy(couple => couple.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>The sum of all the marks. No marks = 0.</summary>
        public static int Total(IReadOnlyList<int> marks) =>
            marks == null ? 0 : marks.Sum();

        /// <summary>The highest single mark. No marks = 0 (Max() would throw on an empty list).</summary>
        public static int BestMark(IReadOnlyList<int> marks) =>
            marks == null || marks.Count == 0 ? 0 : marks.Max();

        private static IReadOnlyList<int> MarksOf(Couple couple, IReadOnlyDictionary<Couple, List<int>> marksByCouple) =>
            marksByCouple != null && marksByCouple.TryGetValue(couple, out List<int> marks) ? marks : null;
    }
}
