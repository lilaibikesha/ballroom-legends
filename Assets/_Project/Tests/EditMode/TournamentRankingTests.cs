using System;
using System.Collections.Generic;
using BallroomLegends.Tournament;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>
    /// The ranking rules on their own. TournamentRanking is a pure static class, so these
    /// tests need no GameObject: build some couples and marks, rank them, check the order.
    /// The marks below are from 3 judges, the MVP default.
    /// </summary>
    public class TournamentRankingTests
    {
        private Dictionary<Couple, List<int>> _marks;

        [SetUp]
        public void CreateEmptyScoreSheet()
        {
            _marks = new Dictionary<Couple, List<int>>();
        }

        [Test]
        public void HighestTotalComesFirst()
        {
            Couple low = Marked("Low", 6, 6, 6); // 18
            Couple high = Marked("High", 9, 9, 9); // 27
            Couple middle = Marked("Middle", 7, 7, 7); // 21

            List<Couple> ranking = TournamentRanking.Rank(new[] { low, high, middle }, _marks);

            CollectionAssert.AreEqual(new[] { high, middle, low }, ranking);
        }

        [Test]
        public void TieOnTotal_TheHigherSingleMarkWins()
        {
            // Both total 24. One judge loved Spiky (a 10), nobody gave Steady more than 8.
            Couple steady = Marked("Steady", 8, 8, 8);
            Couple spiky = Marked("Spiky", 10, 6, 8);

            List<Couple> ranking = TournamentRanking.Rank(new[] { steady, spiky }, _marks);

            CollectionAssert.AreEqual(new[] { spiky, steady }, ranking);
        }

        [Test]
        public void CompleteTie_IsBrokenByName_AToZ()
        {
            // Same total, same best mark. Zara registered first, but the name decides.
            Couple zara = Marked("Zara & Leo", 8, 8, 8);
            Couple anna = Marked("Anna & Max", 8, 8, 8);

            List<Couple> ranking = TournamentRanking.Rank(new[] { zara, anna }, _marks);

            CollectionAssert.AreEqual(new[] { anna, zara }, ranking);
        }

        [Test]
        public void NameTieBreak_IgnoresUpperAndLowerCase()
        {
            // A plain (ordinal) comparison puts every capital letter before every small
            // letter ('B' is 66, 'a' is 97), so "Bella" would beat "anna". People don't
            // read names like that, so the comparison ignores case.
            Couple bella = Marked("Bella & Tom", 7, 7, 7);
            Couple anna = Marked("anna & max", 7, 7, 7);

            List<Couple> ranking = TournamentRanking.Rank(new[] { bella, anna }, _marks);

            CollectionAssert.AreEqual(new[] { anna, bella }, ranking);
        }

        [Test]
        public void AllThreeRules_WorkTogether()
        {
            Couple clearWinner = Marked("Will & Ida", 9, 9, 9); // 27: best total
            Couple bea = Marked("Bea & Sam", 10, 7, 7); // 24, best 10, "B"
            Couple steady = Marked("Cleo & Finn", 8, 8, 8); // 24, best 8
            Couple ava = Marked("Ava & Noah", 7, 7, 10); // 24, best 10, "A"

            List<Couple> ranking = TournamentRanking.Rank(new[] { clearWinner, bea, steady, ava }, _marks);

            // 27 first. Then three couples on 24: the two with a 10 beat the one without,
            // and between those two, Ava comes before Bea.
            CollectionAssert.AreEqual(new[] { clearWinner, ava, bea, steady }, ranking);
        }

        [Test]
        public void CouplesWithoutMarks_RankLast()
        {
            var noMarks = new Couple("Aaron & Abby", false); // would win any name tie-break
            Couple marked = Marked("Zed & Zoe", 1, 1, 1);

            List<Couple> ranking = TournamentRanking.Rank(new[] { noMarks, marked }, _marks);

            CollectionAssert.AreEqual(new[] { marked, noMarks }, ranking);
        }

        [Test]
        public void Rank_ReturnsANewList_AndLeavesTheInputAlone()
        {
            Couple low = Marked("Low", 5, 5, 5);
            Couple high = Marked("High", 9, 9, 9);
            var startOrder = new List<Couple> { low, high };

            TournamentRanking.Rank(startOrder, _marks);

            CollectionAssert.AreEqual(new[] { low, high }, startOrder);
        }

        [Test]
        public void Rank_WithNoCouples_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => TournamentRanking.Rank(null, _marks));
        }

        [Test]
        public void TotalAndBestMark_OfNoMarks_AreZero()
        {
            Assert.AreEqual(0, TournamentRanking.Total(null));
            Assert.AreEqual(0, TournamentRanking.Total(new List<int>()));
            Assert.AreEqual(0, TournamentRanking.BestMark(null));
            Assert.AreEqual(0, TournamentRanking.BestMark(new List<int>()));
        }

        [Test]
        public void TotalAndBestMark_OfSomeMarks()
        {
            var marks = new List<int> { 7, 10, 6 };

            Assert.AreEqual(23, TournamentRanking.Total(marks));
            Assert.AreEqual(10, TournamentRanking.BestMark(marks));
        }

        /// <summary>Creates a couple and writes its judges' marks on the score sheet.</summary>
        private Couple Marked(string name, params int[] judgeMarks)
        {
            var couple = new Couple(name, false);
            _marks[couple] = new List<int>(judgeMarks);
            return couple;
        }
    }
}
