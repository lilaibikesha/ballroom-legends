using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>The timing judge. Early (negative) and late (positive) count the same.</summary>
    public class DanceManagerTests
    {
        private const float Perfect = 0.05f;
        private const float Good = 0.12f;

        [TestCase(0f, HitResult.Perfect)]
        [TestCase(0.03f, HitResult.Perfect)]
        [TestCase(-0.03f, HitResult.Perfect)]
        [TestCase(0.08f, HitResult.Good)]
        [TestCase(-0.08f, HitResult.Good)]
        [TestCase(0.3f, HitResult.Miss)]
        [TestCase(-0.3f, HitResult.Miss)]
        public void EvaluateStep_RatesByDistanceFromTheBeat(float offset, HitResult expected)
        {
            Assert.AreEqual(expected, DanceManager.EvaluateStep(offset, Perfect, Good));
        }

        // Boundary decision: the windows are inclusive. Exactly on the edge still counts.
        [Test]
        public void EvaluateStep_ExactlyOnThePerfectEdge_IsPerfect()
        {
            Assert.AreEqual(HitResult.Perfect, DanceManager.EvaluateStep(Perfect, Perfect, Good));
            Assert.AreEqual(HitResult.Perfect, DanceManager.EvaluateStep(-Perfect, Perfect, Good));
        }

        [Test]
        public void EvaluateStep_ExactlyOnTheGoodEdge_IsGood()
        {
            Assert.AreEqual(HitResult.Good, DanceManager.EvaluateStep(Good, Perfect, Good));
        }

        [Test]
        public void EvaluateStep_JustOutsideTheGoodEdge_IsMiss()
        {
            Assert.AreEqual(HitResult.Miss, DanceManager.EvaluateStep(Good + 0.001f, Perfect, Good));
        }

        [Test]
        public void EvaluateStep_NeverPressed_IsMiss()
        {
            Assert.AreEqual(HitResult.Miss, DanceManager.EvaluateStep(float.PositiveInfinity, Perfect, Good));
        }
    }
}
