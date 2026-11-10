using System.Collections.Generic;
using BallroomLegends.Tournament;
using NUnit.Framework;
using UnityEngine;

namespace BallroomLegends.Tests
{
    /// <summary>
    /// TournamentManager is a MonoBehaviour, so each test puts one on a temporary
    /// GameObject and destroys it afterwards. (Its warnings in the Console are expected.)
    /// </summary>
    public class TournamentManagerTests
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
        public void Register_RejectsNullAndDuplicates()
        {
            var couple = new Couple("A & B", false);

            Assert.IsTrue(_tournament.RegisterCouple(couple));
            Assert.IsFalse(_tournament.RegisterCouple(couple));
            Assert.IsFalse(_tournament.RegisterCouple(null));
            Assert.AreEqual(1, _tournament.Couples.Count);
        }

        [Test]
        public void Submit_RejectsBadInput()
        {
            var registered = new Couple("A & B", false);
            var stranger = new Couple("C & D", false);
            _tournament.RegisterCouple(registered);

            Assert.IsFalse(_tournament.SubmitScores(stranger, Marks(5, 5)), "unregistered couple");
            Assert.IsFalse(_tournament.SubmitScores(registered, new[] { 5, 5, 5 }), "wrong number of marks");
            Assert.IsFalse(_tournament.SubmitScores(registered, new[] { 5, 5, 5, 5, 11 }), "mark above 10");
            Assert.IsFalse(_tournament.SubmitScores(registered, new[] { 0, 5, 5, 5, 5 }), "mark below 1");
            Assert.IsTrue(_tournament.SubmitScores(registered, Marks(5, 5)));
            Assert.AreEqual(25, _tournament.GetTotal(registered));
        }

        [Test]
        public void Ranking_IsByTotal_BestFirst()
        {
            Couple low = Add("Low", 6);
            Couple high = Add("High", 9);
            Couple middle = Add("Middle", 7);

            CollectionAssert.AreEqual(new[] { high, middle, low }, _tournament.GetRanking());
            Assert.AreEqual(1, _tournament.GetPlace(high));
            Assert.AreEqual(3, _tournament.GetPlace(low));
        }

        [Test]
        public void Ranking_TieOnTotal_HigherBestMarkWins()
        {
            var steady = new Couple("Steady", false);
            var spiky = new Couple("Spiky", false);
            _tournament.RegisterCouple(steady);
            _tournament.RegisterCouple(spiky);
            _tournament.SubmitScores(steady, new[] { 8, 8, 8, 8, 8 }); // 40, best 8
            _tournament.SubmitScores(spiky, new[] { 10, 6, 8, 8, 8 }); // 40, best 10

            Assert.AreEqual(spiky, _tournament.GetRanking()[0]);
        }

        [Test]
        public void Ranking_CompleteTie_LowerStartNumberWins()
        {
            Couple first = Add("First", 8);
            Couple second = Add("Second", 8);

            CollectionAssert.AreEqual(new[] { first, second }, _tournament.GetRanking());
        }

        [Test]
        public void Ranking_CouplesWithoutMarks_RankLast()
        {
            var noMarks = new Couple("No marks", false);
            _tournament.RegisterCouple(noMarks);
            Couple marked = Add("Marked", 1);

            Assert.AreEqual(marked, _tournament.GetRanking()[0]);
        }

        [Test]
        public void Advance_FromTheHeat_RecallsTheTop24_AndClearsMarks()
        {
            var couples = new List<Couple>();
            for (int i = 0; i < 30; i++)
            {
                // Couple 0 gets marks of 1, couple 29 gets 10: higher index = better.
                couples.Add(Add($"Couple {i}", 1 + i * 9 / 29));
            }

            _tournament.AdvanceToNextRound();

            Assert.AreEqual(RoundType.QuarterFinal, _tournament.CurrentRound);
            Assert.AreEqual(24, _tournament.Couples.Count);
            Assert.AreEqual(0, _tournament.ScoredCoupleCount);
            CollectionAssert.Contains(_tournament.Couples, couples[29]);
            CollectionAssert.DoesNotContain(_tournament.Couples, couples[0]);
        }

        [Test]
        public void Advance_ThroughEveryRound_ThenFinishes()
        {
            for (int i = 0; i < 48; i++)
            {
                _tournament.RegisterCouple(new Couple($"Couple {i}", false));
            }

            int[] expectedSizes = { 24, 12, 6 };
            foreach (int expected in expectedSizes)
            {
                ScoreEveryone();
                _tournament.AdvanceToNextRound();
                Assert.AreEqual(expected, _tournament.Couples.Count);
            }

            Assert.AreEqual(RoundType.Final, _tournament.CurrentRound);
            Assert.AreEqual(0, _tournament.CouplesAdvancingFromCurrentRound);

            ScoreEveryone();
            _tournament.AdvanceToNextRound();

            Assert.IsTrue(_tournament.IsFinished);
            Assert.AreEqual(6, _tournament.GetRanking().Count, "The final ranking stays readable");
        }

        [Test]
        public void Register_IsClosedOnceTheTournamentMovesOn()
        {
            Add("Only", 5);
            _tournament.AdvanceToNextRound();

            Assert.IsFalse(_tournament.RegisterCouple(new Couple("Late", false)));
        }

        private Couple Add(string name, int markFromEveryJudge)
        {
            var couple = new Couple(name, false);
            _tournament.RegisterCouple(couple);
            _tournament.SubmitScores(couple, Marks(_tournament.NumberOfJudges, markFromEveryJudge));
            return couple;
        }

        private void ScoreEveryone()
        {
            for (int i = 0; i < _tournament.Couples.Count; i++)
            {
                int mark = 1 + i % 10;
                _tournament.SubmitScores(_tournament.Couples[i], Marks(_tournament.NumberOfJudges, mark));
            }
        }

        private static int[] Marks(int judges, int mark)
        {
            var marks = new int[judges];
            for (int i = 0; i < judges; i++)
            {
                marks[i] = mark;
            }

            return marks;
        }
    }
}
