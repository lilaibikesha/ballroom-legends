using System;
using BallroomLegends.Tournament;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class TournamentRulesTests
    {
        [TestCase(RoundType.Final, 6)]
        [TestCase(RoundType.SemiFinal, 12)]
        [TestCase(RoundType.QuarterFinal, 24)]
        [TestCase(RoundType.Heat, 48)]
        public void EachRound_HoldsTwiceTheNextOne(RoundType round, int expected)
        {
            Assert.AreEqual(expected, TournamentRules.CouplesInRound(round));
        }

        [Test]
        public void Next_GoesThroughTheRoundsInOrder()
        {
            Assert.AreEqual(RoundType.QuarterFinal, TournamentRules.Next(RoundType.Heat));
            Assert.AreEqual(RoundType.SemiFinal, TournamentRules.Next(RoundType.QuarterFinal));
            Assert.AreEqual(RoundType.Final, TournamentRules.Next(RoundType.SemiFinal));
        }

        [Test]
        public void Next_AfterTheFinal_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => TournamentRules.Next(RoundType.Final));
        }

        [Test]
        public void TheMvpHasThreeJudges()
        {
            Assert.AreEqual(3, TournamentRules.DefaultNumberOfJudges);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)] // the boundary: 4 couples still go straight to the Final
        public void FourCouplesOrFewer_GoStraightToTheFinal(int couples)
        {
            Assert.AreEqual(RoundType.Final, TournamentRules.FirstRound(couples));
        }

        [TestCase(5)] // one more than the boundary
        [TestCase(48)]
        public void MoreThanFourCouples_StartWithTheFirstRound(int couples)
        {
            Assert.AreEqual(RoundType.Heat, TournamentRules.FirstRound(couples));
        }

        [Test]
        public void FirstRound_WithNobodyRegistered_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TournamentRules.FirstRound(0));
        }
    }
}
