using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>
    /// The timing judge. Early (negative) and late (positive) count the same, because
    /// <see cref="DanceManager.EvaluateStep(float, float, float)"/> looks at the
    /// absolute distance from the beat.
    ///
    /// We test the static overload: it takes the windows as arguments, so the test doesn't
    /// need a GameObject or a scene. The windows below are the Inspector defaults.
    /// </summary>
    public class DanceManagerTests
    {
        private const float PerfectWindow = 0.05f;
        private const float GoodWindow = 0.12f;

        private static HitResult Judge(float secondsFromBeat) =>
            DanceManager.EvaluateStep(secondsFromBeat, PerfectWindow, GoodWindow);

        // ---------- The everyday cases ----------

        [Test]
        public void OnTheBeat_IsPerfect()
        {
            Assert.AreEqual(HitResult.Perfect, Judge(0f));
        }

        [Test]
        public void SlightlyLate_008s_IsGood()
        {
            Assert.AreEqual(HitResult.Good, Judge(0.08f));
        }

        [Test]
        public void SlightlyEarly_008s_IsGood()
        {
            // Early is negative. Without Math.Abs this would wrongly be Perfect,
            // because -0.08 is "less than" 0.05.
            Assert.AreEqual(HitResult.Good, Judge(-0.08f));
        }

        [Test]
        public void WayTooLate_03s_IsMiss()
        {
            Assert.AreEqual(HitResult.Miss, Judge(0.3f));
        }

        [Test]
        public void WayTooEarly_03s_IsMiss()
        {
            Assert.AreEqual(HitResult.Miss, Judge(-0.3f));
        }

        // ---------- The boundaries ----------
        // Decision: the windows are inclusive. A press exactly on the edge gets the better
        // result. Boundary tests are where off-by-one bugs (< instead of <=) get caught.

        [TestCase(0.05f)]
        [TestCase(-0.05f)]
        public void ExactlyOnThePerfectEdge_IsPerfect(float offset)
        {
            Assert.AreEqual(HitResult.Perfect, Judge(offset));
        }

        [TestCase(0.051f)]
        [TestCase(-0.051f)]
        public void JustOutsideThePerfectEdge_IsGood(float offset)
        {
            Assert.AreEqual(HitResult.Good, Judge(offset));
        }

        [TestCase(0.12f)]
        [TestCase(-0.12f)]
        public void ExactlyOnTheGoodEdge_IsGood(float offset)
        {
            Assert.AreEqual(HitResult.Good, Judge(offset));
        }

        [TestCase(0.121f)]
        [TestCase(-0.121f)]
        public void JustOutsideTheGoodEdge_IsMiss(float offset)
        {
            Assert.AreEqual(HitResult.Miss, Judge(offset));
        }

        [Test]
        public void NeverPressed_IsMiss()
        {
            // DanceManager reports a step you never pressed with an offset of infinity.
            Assert.AreEqual(HitResult.Miss, Judge(float.PositiveInfinity));
        }

        [Test]
        public void ZeroWindows_OnlyAnExactHitIsPerfect()
        {
            // Odd settings shouldn't crash: with no windows, only a perfect 0 scores.
            Assert.AreEqual(HitResult.Perfect, DanceManager.EvaluateStep(0f, 0f, 0f));
            Assert.AreEqual(HitResult.Miss, DanceManager.EvaluateStep(0.001f, 0f, 0f));
        }
    }
}
