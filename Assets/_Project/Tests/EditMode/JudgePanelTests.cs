using System;
using BallroomLegends.Tournament;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class JudgePanelTests
    {
        [TestCase(85f, 9)]   // 8.5 rounds up
        [TestCase(72f, 7)]
        [TestCase(100f, 10)]
        [TestCase(0f, 1)]    // never below 1
        [TestCase(150f, 10)] // never above 10
        public void JudgesWhoAgree_GiveSkillOverTen(float skill, int expectedMark)
        {
            var panel = new JudgePanel(new Random(1), disagreement: 0);

            int[] marks = panel.Score(skill, 5);

            Assert.AreEqual(5, marks.Length);
            foreach (int mark in marks)
            {
                Assert.AreEqual(expectedMark, mark);
            }
        }

        [Test]
        public void SameSeed_GivesTheSameMarks()
        {
            int[] first = new JudgePanel(new Random(42)).Score(80f, 5);
            int[] second = new JudgePanel(new Random(42)).Score(80f, 5);

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void Marks_AlwaysStayBetweenOneAndTen()
        {
            var panel = new JudgePanel(new Random(7), disagreement: 3);

            for (int i = 0; i < 200; i++)
            {
                foreach (int mark in panel.Score(i % 101, 5))
                {
                    Assert.That(mark, Is.InRange(TournamentRules.MinMark, TournamentRules.MaxMark));
                }
            }
        }

        [Test]
        public void NoRandom_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new JudgePanel(null));
        }
    }
}
