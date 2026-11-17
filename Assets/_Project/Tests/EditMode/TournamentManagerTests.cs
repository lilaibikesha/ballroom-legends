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
        public void ByDefault_ThreeJudgesMarkEachCouple()
        {
            Assert.AreEqual(3, _tournament.NumberOfJudges);
        }

        [Test]
        public void Submit_RejectsBadInput()
        {
            var registered = new Couple("A & B", false);
            var stranger = new Couple("C & D", false);
            _tournament.RegisterCouple(registered);

            Assert.IsFalse(_tournament.SubmitScores(stranger, Marks(3, 5)), "unregistered couple");
            Assert.IsFalse(_tournament.SubmitScores(null, Marks(3, 5)), "no couple at all");
            Assert.IsFalse(_tournament.SubmitScores(registered, null), "no marks at all");
            Assert.IsFalse(_tournament.SubmitScores(registered, Marks(2, 5)), "one mark too few");
            Assert.IsFalse(_tournament.SubmitScores(registered, Marks(4, 5)), "one mark too many");
            Assert.IsFalse(_tournament.SubmitScores(registered, new[] { 5, 5, 11 }), "mark above 10");
            Assert.IsFalse(_tournament.SubmitScores(registered, new[] { 0, 5, 5 }), "mark below 1");
            Assert.AreEqual(0, _tournament.GetTotal(registered), "rejected marks are not stored");

            Assert.IsTrue(_tournament.SubmitScores(registered, Marks(3, 5)));
            Assert.AreEqual(15, _tournament.GetTotal(registered));
        }

        [Test]
        public void Submit_StoresACopyOfTheMarks()
        {
            var couple = new Couple("A & B", false);
            _tournament.RegisterCouple(couple);
            int[] marks = { 6, 6, 6 };
            _tournament.SubmitScores(couple, marks);

            marks[0] = 10; // the caller changes its array afterwards...

            Assert.AreEqual(18, _tournament.GetTotal(couple), "...but our copy doesn't change");
        }

        [Test]
        public void Ranking_IsByTotal_BestFirst()
        {
            // Everyone registers first: registration closes as soon as marks arrive.
            Couple low = Register("Low");
            Couple high = Register("High");
            Couple middle = Register("Middle");
            Score(low, 6);
            Score(high, 9);
            Score(middle, 7);

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
            _tournament.SubmitScores(steady, new[] { 8, 8, 8 }); // 24, best 8
            _tournament.SubmitScores(spiky, new[] { 10, 6, 8 }); // 24, best 10

            Assert.AreEqual(spiky, _tournament.GetRanking()[0]);
        }

        [Test]
        public void Ranking_CompleteTie_IsBrokenByName()
        {
            // Same total and same best mark: alphabetical order decides, not who
            // registered first. (TournamentRankingTests has more tie cases.)
            Couple zara = Register("Zara & Leo");
            Couple anna = Register("Anna & Max");
            Score(zara, 8);
            Score(anna, 8);

            CollectionAssert.AreEqual(new[] { anna, zara }, _tournament.GetRanking());
            Assert.AreEqual(1, _tournament.GetPlace(anna));
        }

        [Test]
        public void Ranking_CouplesWithoutMarks_RankLast()
        {
            Couple noMarks = Register("No marks");
            Couple marked = Register("Marked");
            Score(marked, 1);

            Assert.AreEqual(marked, _tournament.GetRanking()[0]);
        }

        [Test]
        public void Advance_FromTheHeat_RecallsTheTop24_AndClearsMarks()
        {
            var couples = new List<Couple>();
            for (int i = 0; i < 30; i++)
            {
                couples.Add(Register($"Couple {i}"));
            }

            for (int i = 0; i < 30; i++)
            {
                // Couple 0 gets marks of 1, couple 29 gets 10: higher index = better.
                Score(couples[i], 1 + i * 9 / 29);
            }

            _tournament.AdvanceToNextRound();

            Assert.AreEqual(RoundType.QuarterFinal, _tournament.CurrentRound);
            Assert.AreEqual(24, _tournament.Couples.Count);
            Assert.AreEqual(0, _tournament.ScoredCoupleCount);
            CollectionAssert.Contains(_tournament.Couples, couples[29]);
            CollectionAssert.DoesNotContain(_tournament.Couples, couples[0]);
        }

        [Test]
        public void Advance_AfterTheFinish_DoesNothing()
        {
            Add("Only", 5); // one couple: straight to the Final
            _tournament.AdvanceToNextRound();
            _tournament.AdvanceToNextRound(); // a second call must not crash or change anything

            Assert.IsTrue(_tournament.IsFinished);
            Assert.AreEqual(RoundType.Final, _tournament.CurrentRound);
            Assert.IsFalse(_tournament.SubmitScores(_tournament.Couples[0], Marks(3, 9)), "no marks after the end");
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

        [TestCase(1)]
        [TestCase(4)]
        public void FourCouplesOrFewer_GoStraightToTheFinal(int coupleCount)
        {
            for (int i = 0; i < coupleCount; i++)
            {
                _tournament.RegisterCouple(new Couple($"Couple {i}", false));
            }

            Assert.AreEqual(RoundType.Final, _tournament.CurrentRound);
            Assert.AreEqual(0, _tournament.CouplesAdvancingFromCurrentRound);

            ScoreEveryone();
            _tournament.AdvanceToNextRound();

            Assert.IsTrue(_tournament.IsFinished, "the Final was the only round");
            Assert.AreEqual(coupleCount, _tournament.GetRanking().Count);
        }

        [Test]
        public void AFifthCouple_MeansAFirstRoundAgain()
        {
            for (int i = 0; i < 5; i++)
            {
                _tournament.RegisterCouple(new Couple($"Couple {i}", false));
            }

            Assert.AreEqual(RoundType.Heat, _tournament.CurrentRound);
        }

        [Test]
        public void Register_IsClosedOnceMarksArrive()
        {
            Add("Early", 5);

            Assert.IsFalse(_tournament.RegisterCouple(new Couple("Late", false)));
            Assert.AreEqual(RoundType.Final, _tournament.CurrentRound, "a late couple can't change the round");
        }

        [Test]
        public void Register_IsClosedOnceTheTournamentMovesOn()
        {
            Add("Only", 5);
            _tournament.AdvanceToNextRound();

            Assert.IsFalse(_tournament.RegisterCouple(new Couple("Late", false)));
        }

        private Couple Register(string name)
        {
            var couple = new Couple(name, false);
            _tournament.RegisterCouple(couple);
            return couple;
        }

        /// <summary>Every judge gives this couple the same mark.</summary>
        private void Score(Couple couple, int markFromEveryJudge)
        {
            _tournament.SubmitScores(couple, Marks(_tournament.NumberOfJudges, markFromEveryJudge));
        }

        /// <summary>Registers a couple and marks it straight away (which closes registration).</summary>
        private Couple Add(string name, int markFromEveryJudge)
        {
            Couple couple = Register(name);
            Score(couple, markFromEveryJudge);
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
