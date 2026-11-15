using System.Collections.Generic;
using BallroomLegends.Competition;
using BallroomLegends.Tournament;
using NUnit.Framework;
using UnityEngine;

namespace BallroomLegends.Tests
{
    /// <summary>Rounds with several dances: marks add up across the dances.</summary>
    public class MultiDanceRoundTests
    {
        private GameObject _gameObject;
        private TournamentManager _tournament;

        [SetUp]
        public void CreateTournament()
        {
            _gameObject = new GameObject("TestTournament");
            _tournament = _gameObject.AddComponent<TournamentManager>();
        }

        [TearDown]
        public void DestroyTournament()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void AddDanceScores_AddsUpOverTheDances()
        {
            var couple = Register("Luca & Mia");

            Assert.IsTrue(_tournament.AddDanceScores(couple, new[] { 8, 8, 8, 8, 8 })); // Waltz: 40
            Assert.IsTrue(_tournament.AddDanceScores(couple, new[] { 6, 7, 6, 7, 6 })); // Tango: 32

            Assert.AreEqual(72, _tournament.GetTotal(couple));
            Assert.AreEqual(2, _tournament.GetDancesMarked(couple));
            Assert.AreEqual(10, _tournament.GetScores(couple).Count);
        }

        [Test]
        public void AddDanceScores_RejectsBadMarks_WithoutChangingTheTotal()
        {
            var couple = Register("Hugo & Isla");
            _tournament.AddDanceScores(couple, new[] { 5, 5, 5, 5, 5 });

            Assert.IsFalse(_tournament.AddDanceScores(couple, new[] { 5, 5, 5 }));
            Assert.IsFalse(_tournament.AddDanceScores(couple, new[] { 5, 5, 5, 5, 11 }));

            Assert.AreEqual(25, _tournament.GetTotal(couple));
            Assert.AreEqual(1, _tournament.GetDancesMarked(couple));
        }

        [Test]
        public void Ranking_UsesTheTotalOfAllDances()
        {
            // A great Waltz isn't enough if the other dances are weak.
            var waltzStar = Register("Waltz star");
            var allRounder = Register("All-rounder");

            _tournament.AddDanceScores(waltzStar, new[] { 10, 10, 10, 10, 10 });
            _tournament.AddDanceScores(waltzStar, new[] { 5, 5, 5, 5, 5 });
            _tournament.AddDanceScores(allRounder, new[] { 8, 8, 8, 8, 8 });
            _tournament.AddDanceScores(allRounder, new[] { 8, 8, 8, 8, 8 });

            Assert.AreEqual(allRounder, _tournament.GetRanking()[0]);
        }

        [Test]
        public void SubmitScores_ReplacesEveryDance()
        {
            var couple = Register("Leo & Zara");
            _tournament.AddDanceScores(couple, new[] { 9, 9, 9, 9, 9 });
            _tournament.AddDanceScores(couple, new[] { 9, 9, 9, 9, 9 });

            _tournament.SubmitScores(couple, new[] { 2, 2, 2, 2, 2 });

            Assert.AreEqual(10, _tournament.GetTotal(couple));
            Assert.AreEqual(1, _tournament.GetDancesMarked(couple));
        }

        [Test]
        public void NextRound_StartsAgainFromZeroDances()
        {
            var couple = Register("Only");
            _tournament.AddDanceScores(couple, new[] { 7, 7, 7, 7, 7 });

            _tournament.AdvanceToNextRound();

            Assert.AreEqual(0, _tournament.GetDancesMarked(couple));
            Assert.AreEqual(0, _tournament.GetTotal(couple));
        }

        [Test]
        public void RoundReport_AddDance_KeepsTheRoundTotals()
        {
            var round = new RoundReport();
            round.AddDance(new DanceReport { DanceName = "Waltz", Score = 80f, Perfect = 30, Good = 4, Miss = 2, MaxCombo = 20, Marks = new[] { 8, 8, 8, 8, 8 } });
            round.AddDance(new DanceReport { DanceName = "Tango", Score = 60f, Perfect = 10, Good = 10, Miss = 6, MaxCombo = 25, Marks = new[] { 6, 6, 6, 6, 6 } });

            Assert.AreEqual(2, round.Dances.Count);
            Assert.AreEqual(70f, round.DanceScore, 0.001f, "average of 80 and 60");
            Assert.AreEqual(40, round.Perfect);
            Assert.AreEqual(14, round.Good);
            Assert.AreEqual(8, round.Miss);
            Assert.AreEqual(25, round.MaxCombo, "best combo of any dance");
            Assert.AreEqual(70, round.TotalMarks);
            Assert.AreEqual(10, round.Marks.Length);
        }

        [Test]
        public void RoundProgramme_PicksTheFirstDancesInOrder()
        {
            var standard = new List<string> { "Waltz", "Tango", "Viennese Waltz", "Slow Foxtrot", "Quickstep" };

            CollectionAssert.AreEqual(standard, RoundProgramme.Pick(standard, 5));
            CollectionAssert.AreEqual(new[] { "Waltz", "Tango" }, RoundProgramme.Pick(standard, 2));
            CollectionAssert.AreEqual(new[] { "Waltz" }, RoundProgramme.Pick(standard, 0), "at least one dance");
            Assert.AreEqual(5, RoundProgramme.Pick(standard, 9).Count, "never more than there are");
        }

        [Test]
        public void RoundProgramme_Describe_ReadsLikeASentence()
        {
            Assert.AreEqual("Waltz", RoundProgramme.Describe(new[] { "Waltz" }));
            Assert.AreEqual("Samba and Jive", RoundProgramme.Describe(new[] { "Samba", "Jive" }));
            Assert.AreEqual("Samba, Rumba and Jive", RoundProgramme.Describe(new[] { "Samba", "Rumba", "Jive" }));
            Assert.AreEqual(string.Empty, RoundProgramme.Describe(new string[0]));
        }

        private Couple Register(string name)
        {
            var couple = new Couple(name, false);
            _tournament.RegisterCouple(couple);
            return couple;
        }
    }
}
