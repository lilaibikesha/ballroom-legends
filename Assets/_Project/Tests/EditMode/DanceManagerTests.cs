using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class DanceManagerTests
    {
        private const float Perfect = 0.05f;
        private const float Good = 0.12f;

        [Test]
        [Ignore("Implement DanceManager.EvaluateStep, then delete this [Ignore] line.")]
        public void EvaluateStep_ExactlyOnTheBeat_IsPerfect()
        {
            Assert.AreEqual(HitResult.Perfect, DanceManager.EvaluateStep(0f, Perfect, Good));
        }

        // TODO (you): write more tests BEFORE finishing the implementation:
        //  - a slightly late step (e.g. 0.08s) is Good
        //  - a slightly EARLY step (e.g. -0.08s) is also Good
        //  - a very late step (e.g. 0.3s) is a Miss
        //  - what about exactly 0.05s? Decide, then lock the decision in with a test.
    }
}
